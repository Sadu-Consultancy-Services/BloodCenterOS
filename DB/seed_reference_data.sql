-- BloodCenterOS Reference/Lookup Data Seed
-- Run once to populate reference/lookup tables with standard Indian blood center data
-- Idempotent: uses ON CONFLICT to skip existing rows

BEGIN;

-- ============================================================
-- Component Types (NBTC standard)
-- ============================================================
INSERT INTO ComponentTypeMaster (ComponentTypeId, ComponentTypeCode, Description) VALUES
  (1, 'WB',     'Whole Blood'),
  (2, 'PRBC',   'Packed Red Blood Cells'),
  (3, 'FFP',    'Fresh Frozen Plasma'),
  (4, 'PC',     'Platelet Concentrate'),
  (5, 'CRYO',   'Cryoprecipitate'),
  (6, 'CPP',    'Cryo-poor Plasma'),
  (7, 'SDP',    'Single Donor Platelet'),
  (8, 'WRBC',   'Washed Red Blood Cells'),
  (9, 'IRBC',   'Irradiated Red Blood Cells')
ON CONFLICT (ComponentTypeId) DO NOTHING;
SELECT setval('componenttypemaster_componenttypeid_seq', COALESCE((SELECT MAX(ComponentTypeId) FROM ComponentTypeMaster), 1));

-- ============================================================
-- Lookup Types
-- ============================================================
INSERT INTO LookupType (LookupTypeId, TypeCode, TypeName, Description) VALUES
  (1,  'DEFERRAL_REASON',  'Deferral Reasons',      'Donor deferral reasons (temporary/permanent)'),
  (2,  'DISCARD_REASON',   'Discard Reasons',       'Component/bag discard reasons'),
  (3,  'DONATION_TYPE',    'Donation Types',         'Type of blood donation'),
  (4,  'ISSUE_TYPE',       'Issue Types',            'Blood issue/transfer types'),
  (5,  'BAG_TYPE',         'Bag Types',              'Blood bag types (CPD, CPDA, etc.)'),
  (6,  'GENDER',           'Gender',                 'Donor/patient gender'),
  (7,  'OCCUPATION',       'Occupation',             'Donor occupation list'),
  (8,  'URGENCY',          'Request Urgency',        'Patient request urgency levels'),
  (9,  'TEST_RESULT',      'Test Results',           'Blood screening test results'),
  (10, 'TEST_METHOD',      'Test Methods',           'Screening test methods/technologies')
ON CONFLICT (LookupTypeId) DO NOTHING;
SELECT setval('lookuptype_lookuptypeid_seq', COALESCE((SELECT MAX(LookupTypeId) FROM LookupType), 1));

-- ============================================================
-- Lookup Values
-- ============================================================

-- Deferral Reasons (LookupTypeId = 1)
INSERT INTO LookupValue (LookupTypeId, CenterId, ValueCode, ValueText, SortOrder) VALUES
  (1, NULL, 'LOW_HEMOGLOBIN',   'Low Hemoglobin',      1),
  (1, NULL, 'LOW_WEIGHT',       'Low Weight (<45kg)',  2),
  (1, NULL, 'ILLNESS',          'Illness / Fever',     3),
  (1, NULL, 'MEDICATION',       'On Medication',       4),
  (1, NULL, 'LOW_BP',           'Low Blood Pressure',  5),
  (1, NULL, 'HIGH_BP',          'High Blood Pressure', 6),
  (1, NULL, 'ALCOHOL',          'Alcohol Consumption', 7),
  (1, NULL, 'SURGERY',          'Recent Surgery',      8),
  (1, NULL, 'TATTOO',           'Tattoo / Piercing',   9),
  (1, NULL, 'PREGNANCY',        'Pregnancy / Lactation', 10),
  (1, NULL, 'TRAVEL',           'Travel to Endemic Area', 11),
  (1, NULL, 'VACCINATION',      'Recent Vaccination',  12),
  (1, NULL, 'OTHER_TEMP',       'Other (Temporary)',   13),
  (1, NULL, 'OTHER_PERM',       'Other (Permanent)',   14)
ON CONFLICT DO NOTHING;

-- Discard Reasons (LookupTypeId = 2)
INSERT INTO LookupValue (LookupTypeId, CenterId, ValueCode, ValueText, SortOrder) VALUES
  (2, NULL, 'EXPIRED',           'Expired',              1),
  (2, NULL, 'CLOTTED',           'Clotted / Hemolyzed',  2),
  (2, NULL, 'BAG_DAMAGED',       'Bag Damaged / Leaking',3),
  (2, NULL, 'CONTAMINATED',      'Suspected Contamination',4),
  (2, NULL, 'TEST_POSITIVE',     'TTHI Positive',        5),
  (2, NULL, 'SHORT_BLEED',       'Short Bleed / Low Volume',6),
  (2, NULL, 'DISCARD_EXPIRED',   'Transferred & Expired',7)
ON CONFLICT DO NOTHING;

-- Donation Types (LookupTypeId = 3)
INSERT INTO LookupValue (LookupTypeId, CenterId, ValueCode, ValueText, SortOrder) VALUES
  (3, NULL, 'VOLUNTARY',   'Voluntary',     1),
  (3, NULL, 'REPLACEMENT', 'Replacement',   2),
  (3, NULL, 'CAMP',        'Camp Donation', 3),
  (3, NULL, 'APHERESIS',   'Apheresis',     4)
ON CONFLICT DO NOTHING;

-- Issue Types (LookupTypeId = 4)
INSERT INTO LookupValue (LookupTypeId, CenterId, ValueCode, ValueText, SortOrder) VALUES
  (4, NULL, 'ISSUE',     'Issue to Patient',  1),
  (4, NULL, 'TRANSFER',  'Transfer to Center',2),
  (4, NULL, 'DISCARD',   'Discard',           3)
ON CONFLICT DO NOTHING;

-- Bag Types (LookupTypeId = 5)
INSERT INTO LookupValue (LookupTypeId, CenterId, ValueCode, ValueText, SortOrder) VALUES
  (5, NULL, 'CPD',     'CPD (Citrate-Phosphate-Dextrose)',     1),
  (5, NULL, 'CPDA',    'CPDA-1',                                2),
  (5, NULL, 'SAGM',    'SAG-M (Saline-Adenine-Glucose-Mannitol)',3),
  (5, NULL, 'DOUBLE',  'Double Bag',                             4),
  (5, NULL, 'TRIPLE',  'Triple Bag',                             5),
  (5, NULL, 'QUAD',    'Quadruple Bag',                          6),
  (5, NULL, 'APHERESIS_BAG', 'Apheresis Kit',                    7)
ON CONFLICT DO NOTHING;

-- Gender (LookupTypeId = 6)
INSERT INTO LookupValue (LookupTypeId, CenterId, ValueCode, ValueText, SortOrder) VALUES
  (6, NULL, 'Male',   'Male',   1),
  (6, NULL, 'Female', 'Female', 2),
  (6, NULL, 'Other',  'Other',  3)
ON CONFLICT DO NOTHING;

-- Occupation (LookupTypeId = 7)
INSERT INTO LookupValue (LookupTypeId, CenterId, ValueCode, ValueText, SortOrder) VALUES
  (7, NULL, 'GOVT_SERVICE', 'Government Service', 1),
  (7, NULL, 'PVT_SERVICE',  'Private Service',    2),
  (7, NULL, 'BUSINESS',     'Business / Self-Employed', 3),
  (7, NULL, 'STUDENT',      'Student',            4),
  (7, NULL, 'FARMER',       'Farmer',             5),
  (7, NULL, 'LABORER',      'Laborer',            6),
  (7, NULL, 'HOUSEWIFE',    'Housewife',          7),
  (7, NULL, 'RETIRED',      'Retired',            8),
  (7, NULL, 'UNEMPLOYED',   'Unemployed',         9),
  (7, NULL, 'OTHER',        'Other',             10)
ON CONFLICT DO NOTHING;

-- Urgency (LookupTypeId = 8)
INSERT INTO LookupValue (LookupTypeId, CenterId, ValueCode, ValueText, SortOrder) VALUES
  (8, NULL, 'ROUTINE',    'Routine',  1),
  (8, NULL, 'URGENT',     'Urgent',   2),
  (8, NULL, 'EMERGENCY',  'Emergency',3)
ON CONFLICT DO NOTHING;

-- Test Results (LookupTypeId = 9)
INSERT INTO LookupValue (LookupTypeId, CenterId, ValueCode, ValueText, SortOrder) VALUES
  (9, NULL, 'NEGATIVE', 'Negative / Non-Reactive', 1),
  (9, NULL, 'POSITIVE', 'Positive / Reactive',     2),
  (9, NULL, 'PENDING',  'Pending',                 3)
ON CONFLICT DO NOTHING;

-- Test Methods (LookupTypeId = 10)
INSERT INTO LookupValue (LookupTypeId, CenterId, ValueCode, ValueText, SortOrder) VALUES
  (10, NULL, 'ELISA',     'ELISA',     1),
  (10, NULL, 'CHEMILUMI', 'Chemiluminescence', 2),
  (10, NULL, 'RAPID',     'Rapid Card Test', 3),
  (10, NULL, 'NAT',       'NAT (Nucleic Acid Test)', 4),
  (10, NULL, 'MP_GOLD',   'Malaria Gold',  5)
ON CONFLICT DO NOTHING;

-- ============================================================
-- ReasonMaster — Categorized deferral/discard reasons
-- ============================================================
INSERT INTO ReasonMaster (ReasonId, Category, ReasonCode, ReasonText) VALUES
  -- Deferral reasons
  (1,  'DEFERRAL', 'LOW_HEMOGLOBIN', 'Low Hemoglobin (below 12.5 g/dL)'),
  (2,  'DEFERRAL', 'LOW_WEIGHT',     'Weight below 45 kg'),
  (3,  'DEFERRAL', 'ILLNESS',        'Recent illness or fever'),
  (4,  'DEFERRAL', 'MEDICATION',     'Currently on medication'),
  (5,  'DEFERRAL', 'LOW_BP',         'Low blood pressure (below 100/60)'),
  (6,  'DEFERRAL', 'HIGH_BP',        'High blood pressure (above 160/100)'),
  (7,  'DEFERRAL', 'ALCOHOL',        'Alcohol consumption in last 24 hours'),
  (8,  'DEFERRAL', 'SURGERY',        'Major surgery in last 6 months'),
  (9,  'DEFERRAL', 'TATTOO',         'Tattoo/piercing in last 6 months'),
  (10, 'DEFERRAL', 'PREGNANCY',      'Pregnancy or lactation'),
  (11, 'DEFERRAL', 'TRAVEL',         'Travel to malaria-endemic area'),
  (12, 'DEFERRAL', 'VACCINATION',    'Vaccination in last 4 weeks'),
  -- Discard reasons
  (13, 'DISCARD',  'EXPIRED',        'Component has expired'),
  (14, 'DISCARD',  'CLOTTED',        'Clotted or hemolyzed blood'),
  (15, 'DISCARD',  'BAG_DAMAGED',    'Bag damaged or leaking'),
  (16, 'DISCARD',  'POSITIVE_TTI',   'Transfusion-transmissible infection positive'),
  (17, 'DISCARD',  'SHORT_BLEED',    'Insufficient volume collected')
ON CONFLICT (ReasonId) DO NOTHING;
SELECT setval('reasonmaster_reasonid_seq', COALESCE((SELECT MAX(ReasonId) FROM ReasonMaster), 1));

-- ============================================================
-- PermissionMaster — CRUD permissions per module
-- ============================================================
INSERT INTO PermissionMaster (PermissionId, PermissionCode, Description) VALUES
  (1,  'DONOR_CREATE',   'Create new donor records'),
  (2,  'DONOR_VIEW',     'View donor records'),
  (3,  'DONOR_EDIT',     'Edit donor records'),
  (4,  'DONOR_DELETE',   'Delete donor records'),
  (5,  'COLLECTION_CREATE', 'Create blood collections'),
  (6,  'COLLECTION_VIEW',   'View blood collections'),
  (7,  'COLLECTION_EDIT',   'Edit blood collections'),
  (8,  'COMPONENT_CREATE',  'Prepare blood components'),
  (9,  'COMPONENT_VIEW',    'View blood components'),
  (10, 'COMPONENT_TRANSFER','Transfer components'),
  (11, 'COMPONENT_DISCARD', 'Discard components'),
  (12, 'TEST_CREATE',    'Create test records'),
  (13, 'TEST_VIEW',      'View test results'),
  (14, 'INVENTORY_VIEW', 'View inventory'),
  (15, 'INVENTORY_ADJUST','Adjust inventory'),
  (16, 'ISSUE_CREATE',   'Issue blood to patients'),
  (17, 'ISSUE_VIEW',     'View issue records'),
  (18, 'CAMP_CREATE',    'Create blood donation camps'),
  (19, 'CAMP_VIEW',      'View camp records'),
  (20, 'CAMP_EDIT',      'Edit camp records'),
  (21, 'USER_CREATE',    'Create system users'),
  (22, 'USER_VIEW',      'View system users'),
  (23, 'USER_EDIT',      'Edit system users'),
  (24, 'REPORT_VIEW',    'View reports'),
  (25, 'BILLING_CREATE', 'Create billing records'),
  (26, 'BILLING_VIEW',   'View billing records'),
  (27, 'EMERGENCY_REQ',  'Create emergency requests'),
  (28, 'HOSPITAL_CREATE', 'Create hospital records'),
  (29, 'HOSPITAL_VIEW',   'View hospital records'),
  (30, 'ADMIN_PANEL',     'Access admin panel')
ON CONFLICT (PermissionId) DO NOTHING;
SELECT setval('permissionmaster_permissionid_seq', COALESCE((SELECT MAX(PermissionId) FROM PermissionMaster), 1));

-- ============================================================
-- Service Charges (CenterId = 1, Main Center)
-- ============================================================
INSERT INTO ServiceChargeMaster (CenterId, ServiceCode, ServiceName, Amount, IsActive) VALUES
  (1, 'WB_CHARGE',      'Whole Blood Processing',               1450.00, TRUE),
  (1, 'PRBC_CHARGE',    'Packed Red Blood Cells Processing',    1800.00, TRUE),
  (1, 'FFP_CHARGE',     'Fresh Frozen Plasma Processing',       1200.00, TRUE),
  (1, 'PC_CHARGE',      'Platelet Concentrate Processing',      2500.00, TRUE),
  (1, 'CRYO_CHARGE',    'Cryoprecipitate Processing',           2000.00, TRUE),
  (1, 'SDP_CHARGE',     'Single Donor Platelet Processing',     5000.00, TRUE),
  (1, 'CROSSMATCH',     'Cross Matching',                        350.00, TRUE),
  (1, 'SCREENING',      'Screening Tests (TTHI)',               1200.00, TRUE),
  (1, 'STORAGE',        'Storage Fee (per day)',                  50.00, TRUE),
  (1, 'TRANSPORT',      'Transport / Delivery Fee',              500.00, TRUE)
ON CONFLICT DO NOTHING;

-- ============================================================
-- States (Major Indian states)
-- ============================================================
INSERT INTO StateMaster (StateId, StateName) VALUES
  (1,  'Andhra Pradesh'),
  (2,  'Arunachal Pradesh'),
  (3,  'Assam'),
  (4,  'Bihar'),
  (5,  'Chhattisgarh'),
  (6,  'Goa'),
  (7,  'Gujarat'),
  (8,  'Haryana'),
  (9,  'Himachal Pradesh'),
  (10, 'Jharkhand'),
  (11, 'Karnataka'),
  (12, 'Kerala'),
  (13, 'Madhya Pradesh'),
  (14, 'Maharashtra'),
  (15, 'Manipur'),
  (16, 'Meghalaya'),
  (17, 'Mizoram'),
  (18, 'Nagaland'),
  (19, 'Odisha'),
  (20, 'Punjab'),
  (21, 'Rajasthan'),
  (22, 'Sikkim'),
  (23, 'Tamil Nadu'),
  (24, 'Telangana'),
  (25, 'Tripura'),
  (26, 'Uttar Pradesh'),
  (27, 'Uttarakhand'),
  (28, 'West Bengal'),
  (29, 'Andaman and Nicobar Islands'),
  (30, 'Chandigarh'),
  (31, 'Dadra and Nagar Haveli and Daman and Diu'),
  (32, 'Delhi'),
  (33, 'Jammu and Kashmir'),
  (34, 'Ladakh'),
  (35, 'Lakshadweep'),
  (36, 'Puducherry')
ON CONFLICT (StateId) DO NOTHING;
SELECT setval('statemaster_stateid_seq', COALESCE((SELECT MAX(StateId) FROM StateMaster), 1));

COMMIT;
