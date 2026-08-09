# BloodCenterOS — Developer Update: v2.0 Schema — BloodRequest Unification

**Date:** 2026-08-10 (Build 1)
**Scope:** Database schema v2.0, API/Core/Web code, all affected views
**Result:** Build green (0 warnings / 0 errors), DB fully migrated & verified live

---

## What changed and why

`PatientRequest`, `PatientReservation` and `EmergencyRequest` overlapped heavily — all
three described "a patient needs blood" with the same core fields (patient, blood group,
units, hospital, status). Using the recommendation from
`database_tables_data_flow_guide.html` (M9, High priority), the three tables were merged
into a single canonical entity:

> **`BloodRequest`** (header) + **`BloodRequestDetail`** (reserved component lines)

The physical tables were renamed to the canonical `BloodRequest*` names, and identity
columns were finally renamed from `reservation*` → `bloodrequest*` across every ancestor
table (`CrossMatchEntry`, `CrossMatchTestResult`, plus sequences and FK defaults).

Legacy `CrossMatchRecord` (already superseded by `CrossMatchEntry`+`CrossMatchTestResult`
from Patch 008) was **dropped**.

---

## 1. Database migration (apply in order)

All patches are idempotent where possible and run inside a transaction. Verify each file's
header comment before applying.

```sql
-- Step 1A  Dead table removal
& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d bloodcenter -f DB\patch_20260810_018a_schema_v2_drop_crossmatchrecord.sql

-- Step 1B (Part A)  Table rename + data migration from legacy tables
& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d bloodcenter -f DB\patch_20260810_018_schema_v2_blood_request.sql

-- Step 1B (Part B)  Auto-generated: reservation/crossmatch/issue/report SPs pointed at new tables
& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d bloodcenter -f DB\patch_20260810_018b_auto_gen.sql

-- Step 1B (Part B manual)  Patient / Emergency / Replacement flows rewritten
& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d bloodcenter -f DB\patch_20260810_018c_patient_emergency_rewrite.sql

-- Step 1C  Physical column renames (reservation* -> bloodrequest*)
& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d bloodcenter -f DB\patch_20260810_018d_full_id_rename.sql

-- Step 1D  Re-publish every affected function against the renamed columns
& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d bloodcenter -f DB\patch_20260810_018e_regenerate_functions.sql
```

### Column mapping applied

| Table | Old column | New column |
|---|---|---|
| `BloodRequest` | `reservationid` | `bloodrequestid` |
| `BloodRequestDetail` | `reservationid` | `bloodrequestid` |
| `BloodRequestDetail` | `reservationdetailid` | `bloodrequestdetailid` |
| `CrossMatchEntry` | `reservationid` | `bloodrequestid` |
| `CrossMatchTestResult` | `reservationdetailid` | `bloodrequestdetailid` |
| sequence | `patientreservation_reservationid_seq` | `bloodrequest_bloodrequestid_seq` |
| sequence | `reservationdetail_reservationdetailid_seq` | `bloodrequestdetail_bloodrequestdetailid_seq` |

### New columns added to `BloodRequest` (union columns)

`requesttype` (Reservation/Patient/Emergency), `hospitalid`, `patientgender`,
`requesturgency`, `prescriptionattachmentid`, `requestedbyuserid`, `relatedissueid`,
`fulfilledat`, `patientage`.

Legacy tables remain archived as `PatientRequest_legacy` / `EmergencyRequest_legacy`
until the team confirms no historical queries need them.

---

## 2. Code changes

### Entities (BloodCenterOS.Core)

- **`PatientReservation.cs` → `BloodRequest.cs`** (new) ; `PatientReservationDetail.cs` → `BloodRequestDetail.cs`
- `CrossMatchEntry` / `CrossMatchTestResult` / `IssueRecord` / `ReportModels` updated to `BloodRequestId` / `BloodRequestDetailId`
- `CrossMatchRecord.cs` **deleted** (orphaned model, no live writer)

### Repository & API (BloodCenterOS.API)

- `ReservationRepository` / `IReservationRepository` — all Dapper calls and JSON return
  aliases use `bloodrequestid` / `BloodRequestId`
- Controllers (`Reservation`, `CrossMatch`, `Issue`, `PatientRequest`, `Emergency`,
  `ReplacementDonor`) serialize `BloodRequestId` in request/response DTOs.
- API no longer exposes the legacy `reservation*` JSON property names on HTTP request bodies.

### Web (BloodCenterOS.Web)

- `ApiClient.cs` — sends `bloodRequestId` as the JSON body property to the API endpoints
  (Web action params can still be named `reservationId` for form binding; the API payload is `bloodRequestId`).
- Views updated: `Reservation/Index`, `Reservation/Details`, `CrossMatch/Create`,
  `CrossMatch/Details`, `Issue/Index`, `Issue/Create` — all model/route references use the
  new types (`BloodRequest`, `BloodRequestDetail`) and properties (`BloodRequestId`, `BloodRequestDetailId`).

---

## 3. Verification performed

| Check | Result |
|---|---|
| `dotnet build BloodCenterOS.API` | ✅ 0 warnings, 0 errors |
| `dotnet build BloodCenterOS.Web` | ✅ 0 warnings, 0 errors |
| Tables `BloodRequest` / `BloodRequestDetail` present + union columns | ✅ |
| Identity columns renamed at physical layer | ✅ |
| Sequences renamed + column defaults re-pointed | ✅ |
| `CrossMatchRecord` dropped (incl. legacy writer SP) | ✅ |
| Stale references to `reservationid` / `reservationdetailid` in any function body | 0 |
| `fn_reservation_get_by_center(1,...)` smoke test | ✅ 4 rows |
| `fn_crossmatch_get_pending_reservations(1)` smoke test | ✅ executes cleanly |

> Note: the untracked `BloodCenterOS.sln` was replaced by `BloodCenterOS.slnx` (new .NET
> solution format). Open the solution from `BloodCenterOS.slnx`.

---

## 4. Still pending / recommended follow-ups

1. **Commit** the rename changeset (currently all uncommitted in the working tree).
2. Run `BloodCenterOS.API.IntegrationTests` (end-to-end Issue flow references the new names).
3. Verify the React Native (mobile) app — check it does not still call API endpoints with
   `reservationId` body properties.
4. Decide on `PatientRequest_legacy` / `EmergencyRequest_legacy` retirement after
   confirming no reporting queries depend on them.
5. Re-run `bloodcenter_20260809_1_fulldb.sql` restore + patches against a fresh staging DB
   to validate the migration script end-to-end.

---

© Sadu Consultancy Services, Pune, Maharashtra, India — BloodCenterOS internal developer reference.