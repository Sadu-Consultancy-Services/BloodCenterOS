# BloodCenterOS — User Guide for Blood Centers

This guide explains, step by step, how a blood center staff member uses BloodCenterOS to run **day-to-day blood center operations** — from donor registration through blood testing, component preparation, inventory, and issue to hospitals. It also shows **where to find each option** in the menu.

---

## Table of Contents

1. [First Login & the Dashboard](#1-first-login--the-dashboard)
2. [Where to Find Each Option (Menu Map)](#2-where-to-find-each-option-menu-map)
3. [Step 0 — One-Time Setup (Masters)](#3-step-0--one-time-setup-masters)
4. [The Blood Center Workflow, End-to-End](#4-the-blood-center-workflow-end-to-end)
5. [Donors — Register & Search](#5-donors--register--search)
6. [Camps — Plan & Run a Donation Camp](#6-camps--plan--run-a-donation-camp)
7. [Collection — Record a Blood Bag](#7-collection--record-a-blood-bag)
8. [Testing — Enter Lab Results](#8-testing--enter-lab-results)
9. [Components — Prepare & Stock](#9-components--prepare--stock)
10. [Inventory — Monitor Stock](#10-inventory--monitor-stock)
11. [Issue — Supply Hospitals](#11-issue--supply-hospitals)
12. [Billing & Emergency](#12-billing--emergency)
13. [Reports & Compliance](#13-reports--compliance)
14. [Administration (users, roles, settings)](#14-administration)
15. [Tips & Troubleshooting](#15-tips--troubleshooting)

---

## 1. First Login & the Dashboard

1. **Open the application** in your browser (your administrator will give you the web address, e.g. `https://your-server:7180`).
2. **Sign in** on the Login screen with your *Username* and *Password*. The default administrator is `admin` / `admin@123` (change it after first login).
3. **The Dashboard** appears after login. It shows summary cards (donors, collections, tests, issues) — click any card to drill into that module. Use the **"New Collection"** button to jump straight into recording a blood bag, and **"Refresh"** to reload live numbers.

> **Tip:** the left **sidebar** is your main navigation. It is grouped into *Masters*, *Operations*, *Camps*, *Hospitals*, *Reports*, and *Administration*.

---

## 2. Where to Find Each Option (Menu Map)

| Menu / Option | What it is for |
|---|---|
| Dashboard | Home screen with live summary counts. |
| Branches | Your center and its branches (HQ + collection units). |
| Departments | Lab, Collection, Testing, Inventory, Issue, etc. |
| Devices | Centrifuges, meters, and other equipment. |
| Fridges | Cold-storage units where blood is kept. |
| Designations | Job titles (Technician, MO, etc.). |
| Employees | Staff records linked to designations & departments. |
| Blood Groups | A/B/O/AB × Rh +/− reference list. |
| Component Types | RBC, Plasma, Platelets, Cryo definitions. |
| SMS Templates | Reusable text messages (OTP, reminders). |
| Email Templates | Reusable email content. |
| Newsletter Subs | Donors/hospitals subscribed to updates. |
| **Donors** | Register, search, and view donation history. |
| **Collection** | Record each blood bag collected. |
| **Testing** | Enter screening lab results. |
| **Components** | Prepare components from collected units. |
| **Inventory** | Stock of blood groups & components. |
| **Issue** | Issue blood/components to hospitals. |
| **Billing** | Receipts & charges for issued units. |
| **Emergency** | Emergency / urgent blood requests. |
| Camps ▸ Camps | Plan and track donation camps. |
| Camps ▸ Camp Inventory | Consumables/expenses stock for a camp. |
| Camps ▸ Camp Expenses | Track camp costs. |
| Hospitals | Hospitals that request blood. |
| Reports | Donor / Inventory / Camp summaries. |
| Administration ▸ Users | System login accounts. |
| Administration ▸ Roles | Permission roles (Admin, MO, Tech…). |
| Administration ▸ Settings | Center configuration. |

---

## 3. Step 0 — One-Time Setup (Masters)

Before daily operations, an administrator should configure the **master data**. These are found under the top section of the sidebar:

1. **Branches** — confirm your main center (HQ) and any branch/collection units. Each has a code (e.g. `HQ`, `PUN-MR`).
2. **Departments** — ensure Collection, Testing, Inventory, Issue exist.
3. **Designations** & **Employees** — add staff so collections/tests are attributed to a person.
4. **Blood Groups** & **Component Types** — these usually come pre-loaded; verify they are present.
5. **Devices** & **Fridges** — register the equipment you will log against collections and storage.

> Masters are the "lookup tables" everything else depends on. Get them right once; daily entry then becomes simple dropdowns.

---

## 4. The Blood Center Workflow, End-to-End

```
Donor → Collection → Testing → Components → Inventory → Issue → Billing
```

Each step is a separate menu item. Follow them in order; the system carries the unit's identity (via its bag/reference id) from one step to the next.

---

## 5. Donors — Register & Search

1. Open **Donors** from the sidebar.
2. Click **"New Donor"** (or "Register") and fill in name, blood group, gender, contact (phone/email), and eligibility notes.
3. To find an existing donor, use the **search box** (by name, blood group, gender) or look them up **by phone**. Repeat donors are matched so their **donation history** stays together.
4. Open a donor's profile to see past **donations** and send them **reminders** via SMS/Email (templates configured under Masters).

---

## 6. Camps — Plan & Run a Donation Camp

1. Expand **Camps ▸ Camps** and click **"New Camp"**.
2. Enter camp code, name, organizer, venue, city, date, and expected donors.
3. On camp day, register donors (see §5) and record their collections (see §7) — the camp summary updates automatically.
4. Use **Camps ▸ Camp Inventory** to track consumables and **Camps ▸ Camp Expenses** to log costs for that camp.
5. View **upcoming camps** from the camps list to plan ahead.

---

## 7. Collection — Record a Blood Bag

1. Open **Collection** and click **"New Collection"** (also reachable from the Dashboard button).
2. Link the **donor**, choose the **camp** (if any), the **device** used, and capture the **barcode / bag id**.
3. Save — the unit is now in the system and ready for testing.

> From a donor's profile you can also launch **"New Collection"** directly, pre-filled with that donor.

---

## 8. Testing — Enter Lab Results

1. Open **Testing** (sidebar, blood-drop icon).
2. Find the collected unit (from Collection) and enter screening results: **HIV, HBsAg, HCV, VDRL, Malaria**.
3. Mark the unit **Reactive** or **Non-Reactive**. Reactive units are segregated and will not enter safe inventory.

> You can also start testing directly from a donor or collection record via the **"Send for Testing"** / **"New Test"** button (eyedropper icon).

---

## 9. Components — Prepare & Stock

1. Open **Components** for a tested, non-reactive unit.
2. Prepare components — **RBC, Plasma, Platelets, Cryo** — as configured under Component Types.
3. Each prepared component becomes a separate stock line tracked in Inventory.

---

## 10. Inventory — Monitor Stock

1. Open **Inventory** to see the live dashboard of **blood groups & components** by available / reserved / quarantined quantity.
2. Watch **expiry** and **quarantine** statuses; record **discard/destruction** when units expire or fail.
3. Use **inter-center transfers** if your setup spans multiple branches.

---

## 11. Issue — Supply Hospitals

1. Open **Hospitals** and confirm the requesting hospital is registered.
2. Open **Issue** and create a request: pick hospital, required blood group/component, and run **crossmatch / compatibility** validation.
3. Issue the unit; handle **returns** and **replacements** as needed. The issued quantity leaves Inventory automatically.

---

## 12. Billing & Emergency

1. **Billing** — generate the **receipt** / charges for issued units.
2. **Emergency** — log **urgent blood requests** so staff can prioritize matching and issue quickly.

---

## 13. Reports & Compliance

1. Open **Reports** for **Donor**, **Inventory**, and **Camp** summaries.
2. Export to **Excel / PDF** for regulatory submission (NBTC/NACO alignment is in progress).

> Audit trails are maintained automatically on key actions to support traceability and accountability.

---

## 14. Administration

1. **Administration ▸ Users** — create login accounts for staff.
2. **Administration ▸ Roles** — assign permissions (Super Admin, Center Admin, Medical Officer, Technician, Camp Coordinator, Data Entry, Hospital User).
3. **Administration ▸ Settings** — center configuration and reference data.
4. **Audit Logs** — review who did what, for accountability.

---

## 15. Tips & Troubleshooting

- **Always use HTTPS.** Open the app over `https://`; using `http://` can drop your session and show a login prompt again.
- **First-time admin:** change the default `admin@123` password promptly.
- **Lost a step?** The unit flows forward by its bag/reference id — if Testing can't find a unit, check it was saved in **Collection** first.
- **Reactive units:** never issue them; they are auto-segregated after **Testing**.
- **Reports for government portals:** use **Reports** → export Excel/PDF; NBTC/NACO compliance workflows are being aligned.
- **Need help?** Contact Sadu Consultancy Services — `bloodcenteros@saducs.com` or WhatsApp from the login screen footer.

### Quick Start Checklist for Day One

1. Log in (admin / admin@123) → change password.
2. Verify Masters: Branches, Departments, Designations, Employees, Blood Groups, Component Types, Devices, Fridges.
3. Register a Donor → New Collection → Testing → Components → check Inventory → Issue to a Hospital → Billing.
4. Run a Report and export to confirm your data is complete.

---

*BloodCenterOS v1.0 — Developed & Maintained by **Sadu Consultancy Services**, Pune, Maharashtra, India.*
*Open-source under the Apache License 2.0.*
