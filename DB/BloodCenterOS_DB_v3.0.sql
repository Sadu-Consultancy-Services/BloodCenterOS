/******************************************************************************************
 BloodCenterOS_DB_v3.sql
 - Phase 1: Schema-only (CREATE TABLE statements)
 - No foreign keys, hard-delete policy
 - datetime2(7) timestamps with DEFAULT SYSUTCDATETIME()
 - CreatedBy / UpdatedBy nullable bigints for audit
 - Run in a dev environment first
******************************************************************************************/

PRINT 'Starting BloodCenterOS_DB_v3.sql - schema creation...';
GO

/* ---------------------------
   1. User & Access Management
   --------------------------- */
IF OBJECT_ID('dbo.UserMaster','U') IS NULL
CREATE TABLE dbo.UserMaster (
  UserId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  UserName NVARCHAR(150) NOT NULL,
  DisplayName NVARCHAR(200) NULL,
  Email NVARCHAR(200) NULL,
  Phone NVARCHAR(50) NULL,
  PasswordHash NVARCHAR(512) NULL,
  PasswordSalt NVARCHAR(256) NULL,
  IsLocked BIT NOT NULL DEFAULT 0,
  LastLoginAt DATETIME2(7) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  CreatedBy BIGINT NULL,
  UpdatedAt DATETIME2(7) NULL,
  UpdatedBy BIGINT NULL
);
GO

IF OBJECT_ID('dbo.RoleMaster','U') IS NULL
CREATE TABLE dbo.RoleMaster (
  RoleId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  RoleName NVARCHAR(150) NOT NULL,
  Description NVARCHAR(500) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  CreatedBy BIGINT NULL,
  UpdatedAt DATETIME2(7) NULL,
  UpdatedBy BIGINT NULL
);
GO

IF OBJECT_ID('dbo.UserRoleMap','U') IS NULL
CREATE TABLE dbo.UserRoleMap (
  UserRoleMapId BIGINT IDENTITY(1,1) PRIMARY KEY,
  UserId BIGINT NOT NULL,
  RoleId BIGINT NOT NULL,
  CenterId BIGINT NULL,
  AssignedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  AssignedBy BIGINT NULL
);
GO

IF OBJECT_ID('dbo.PermissionMaster','U') IS NULL
CREATE TABLE dbo.PermissionMaster (
  PermissionId BIGINT IDENTITY(1,1) PRIMARY KEY,
  PermissionCode NVARCHAR(200) NOT NULL,
  Description NVARCHAR(500) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.RolePermissionMap','U') IS NULL
CREATE TABLE dbo.RolePermissionMap (
  RolePermissionMapId BIGINT IDENTITY(1,1) PRIMARY KEY,
  RoleId BIGINT NOT NULL,
  PermissionId BIGINT NOT NULL,
  CenterId BIGINT NULL,
  AssignedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  AssignedBy BIGINT NULL
);
GO

IF OBJECT_ID('dbo.LoginHistory','U') IS NULL
CREATE TABLE dbo.LoginHistory (
  LoginHistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
  UserId BIGINT NOT NULL,
  CenterId BIGINT NULL,
  LoginAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  LogoutAt DATETIME2(7) NULL,
  IpAddress NVARCHAR(50) NULL,
  UserAgent NVARCHAR(500) NULL
);
GO

IF OBJECT_ID('dbo.AuditLog','U') IS NULL
CREATE TABLE dbo.AuditLog (
  AuditLogId BIGINT IDENTITY(1,1) PRIMARY KEY,
  PropertyOwnerId BIGINT NOT NULL,
  UserId BIGINT NOT NULL,
  Action NVARCHAR(100) NOT NULL,
  TableName NVARCHAR(200) NULL,
  RecordId NVARCHAR(100) NULL,
  ActionDetails NVARCHAR(4000) NULL,
  OldValue NVARCHAR(4000) NULL,
  NewValue NVARCHAR(4000) NULL,
  IpAddress NVARCHAR(50) NULL,
  UserAgent NVARCHAR(500) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

/* ---------------------------
   2. Blood Center & Infrastructure
   --------------------------- */
IF OBJECT_ID('dbo.BloodCenterMaster','U') IS NULL
CREATE TABLE dbo.BloodCenterMaster (
  CenterId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterCode NVARCHAR(100) NULL,
  CenterName NVARCHAR(250) NOT NULL,
  LicenseNumber NVARCHAR(100) NULL,
  AddressLine1 NVARCHAR(300) NULL,
  AddressLine2 NVARCHAR(300) NULL,
  City NVARCHAR(100) NULL,
  District NVARCHAR(100) NULL,
  State NVARCHAR(100) NULL,
  Pincode NVARCHAR(20) NULL,
  Phone NVARCHAR(50) NULL,
  Email NVARCHAR(200) NULL,
  Website NVARCHAR(200) NULL,
  IsActive BIT NOT NULL DEFAULT 1,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  CreatedBy BIGINT NULL,
  UpdatedAt DATETIME2(7) NULL,
  UpdatedBy BIGINT NULL
);
GO

IF OBJECT_ID('dbo.BranchMaster','U') IS NULL
CREATE TABLE dbo.BranchMaster (
  BranchId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NOT NULL,
  BranchCode NVARCHAR(100) NULL,
  BranchName NVARCHAR(250) NULL,
  AddressLine1 NVARCHAR(300) NULL,
  AddressLine2 NVARCHAR(300) NULL,
  City NVARCHAR(100) NULL,
  State NVARCHAR(100) NULL,
  Pincode NVARCHAR(20) NULL,
  Phone NVARCHAR(50) NULL,
  Email NVARCHAR(200) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  CreatedBy BIGINT NULL
);
GO

IF OBJECT_ID('dbo.DepartmentMaster','U') IS NULL
CREATE TABLE dbo.DepartmentMaster (
  DepartmentId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  DepartmentCode NVARCHAR(100) NULL,
  DepartmentName NVARCHAR(200) NOT NULL,
  Description NVARCHAR(500) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.EmployeeMaster','U') IS NULL
CREATE TABLE dbo.EmployeeMaster (
  EmployeeId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  EmployeeCode NVARCHAR(100) NULL,
  FirstName NVARCHAR(150) NULL,
  LastName NVARCHAR(150) NULL,
  Email NVARCHAR(200) NULL,
  Phone NVARCHAR(50) NULL,
  Designation NVARCHAR(150) NULL,
  DepartmentId BIGINT NULL,
  JoinDate DATE NULL,
  IsActive BIT NOT NULL DEFAULT 1,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  CreatedBy BIGINT NULL,
  UpdatedAt DATETIME2(7) NULL
);
GO

IF OBJECT_ID('dbo.DesignationMaster','U') IS NULL
CREATE TABLE dbo.DesignationMaster (
  DesignationId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  DesignationName NVARCHAR(200) NOT NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.DeviceMaster','U') IS NULL
CREATE TABLE dbo.DeviceMaster (
  DeviceId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  DeviceName NVARCHAR(200) NULL,
  DeviceType NVARCHAR(100) NULL,
  SerialNumber NVARCHAR(200) NULL,
  PurchaseDate DATE NULL,
  WarrantyEndDate DATE NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.FridgeStorageMaster','U') IS NULL
CREATE TABLE dbo.FridgeStorageMaster (
  FridgeId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  FridgeCode NVARCHAR(100) NULL,
  FridgeName NVARCHAR(200) NULL,
  Capacity INT NULL,
  Location NVARCHAR(200) NULL,
  TemperatureLogRequired BIT NOT NULL DEFAULT 1,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.LookupType','U') IS NULL
CREATE TABLE dbo.LookupType (
  LookupTypeId BIGINT IDENTITY(1,1) PRIMARY KEY,
  TypeCode NVARCHAR(100) NOT NULL,
  TypeName NVARCHAR(200) NOT NULL,
  Description NVARCHAR(500) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.LookupValue','U') IS NULL
CREATE TABLE dbo.LookupValue (
  LookupValueId BIGINT IDENTITY(1,1) PRIMARY KEY,
  LookupTypeId BIGINT NULL,
  CenterId BIGINT NULL,
  ValueCode NVARCHAR(100) NULL,
  ValueText NVARCHAR(500) NULL,
  SortOrder INT NULL,
  IsActive BIT NOT NULL DEFAULT 1,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

/* ---------------------------
   3. Donor Management
   --------------------------- */
IF OBJECT_ID('dbo.DonorMaster','U') IS NULL
CREATE TABLE dbo.DonorMaster (
  DonorId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  DonorCode NVARCHAR(100) NULL,
  FirstName NVARCHAR(200) NOT NULL,
  LastName NVARCHAR(200) NULL,
  Gender NVARCHAR(50) NULL,
  DateOfBirth DATE NULL,
  BloodGroup NVARCHAR(10) NULL,
  Phone NVARCHAR(50) NULL,
  Email NVARCHAR(200) NULL,
  AadhaarNumber NVARCHAR(20) NULL,
  AddressLine1 NVARCHAR(300) NULL,
  AddressLine2 NVARCHAR(300) NULL,
  City NVARCHAR(100) NULL,
  Pincode NVARCHAR(20) NULL,
  Occupation NVARCHAR(200) NULL,
  PreferredLanguage NVARCHAR(50) NULL,
  LastDonationDate DATE NULL,
  TotalDonations INT NOT NULL DEFAULT 0,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  CreatedBy BIGINT NULL,
  UpdatedAt DATETIME2(7) NULL,
  UpdatedBy BIGINT NULL
);
GO

IF OBJECT_ID('dbo.DonorHealthHistory','U') IS NULL
CREATE TABLE dbo.DonorHealthHistory (
  DonorHealthHistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  DonorId BIGINT NOT NULL,
  VisitDate DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  WeightKg DECIMAL(5,2) NULL,
  Temperature DECIMAL(5,2) NULL,
  BloodPressure NVARCHAR(50) NULL,
  Hemoglobin DECIMAL(5,2) NULL,
  PulseRate INT NULL,
  Remarks NVARCHAR(2000) NULL,
  RecordedBy BIGINT NULL
);
GO

IF OBJECT_ID('dbo.DonorDonationHistory','U') IS NULL
CREATE TABLE dbo.DonorDonationHistory (
  DonationId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  DonorId BIGINT NOT NULL,
  CollectionId BIGINT NULL,
  DonationDate DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  DonationType NVARCHAR(100) NULL, -- Voluntary/Replacement/Directed
  VolumeMl INT NULL,
  BagNumber NVARCHAR(100) NULL,
  Remarks NVARCHAR(1000) NULL,
  CreatedBy BIGINT NULL
);
GO

IF OBJECT_ID('dbo.DeferralRecord','U') IS NULL
CREATE TABLE dbo.DeferralRecord (
  DeferralId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  DonorId BIGINT NOT NULL,
  DeferralDate DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  Reason NVARCHAR(1000) NULL,
  DeferralUntil DATE NULL,
  Notes NVARCHAR(2000) NULL,
  CreatedBy BIGINT NULL
);
GO

IF OBJECT_ID('dbo.DonorAppointment','U') IS NULL
CREATE TABLE dbo.DonorAppointment (
  AppointmentId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  DonorId BIGINT NULL,
  AppointmentDate DATETIME2(7) NULL,
  Slot NVARCHAR(100) NULL,
  Status NVARCHAR(50) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  CreatedBy BIGINT NULL
);
GO

IF OBJECT_ID('dbo.DonorCommunicationLog','U') IS NULL
CREATE TABLE dbo.DonorCommunicationLog (
  CommId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  DonorId BIGINT NULL,
  Channel NVARCHAR(50) NULL, -- SMS/EMAIL
  Message NVARCHAR(2000) NULL,
  SentAt DATETIME2(7) NULL,
  SentBy BIGINT NULL,
  Status NVARCHAR(100) NULL
);
GO

/* ---------------------------
   4. Camps & Collection
   --------------------------- */
IF OBJECT_ID('dbo.BloodCampMaster','U') IS NULL
CREATE TABLE dbo.BloodCampMaster (
  CampId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  CampCode NVARCHAR(100) NULL,
  CampName NVARCHAR(300) NULL,
  OrganizerId BIGINT NULL,
  Venue NVARCHAR(500) NULL,
  City NVARCHAR(200) NULL,
  CampDate DATE NULL,
  StartTime DATETIME2(7) NULL,
  EndTime DATETIME2(7) NULL,
  TotalDonorsExpected INT NULL,
  TotalDonorsCollected INT NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  CreatedBy BIGINT NULL
);
GO

IF OBJECT_ID('dbo.CampOrganizer','U') IS NULL
CREATE TABLE dbo.CampOrganizer (
  OrganizerId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  OrganizerName NVARCHAR(300) NULL,
  ContactPerson NVARCHAR(200) NULL,
  Phone NVARCHAR(50) NULL,
  Email NVARCHAR(200) NULL,
  Address NVARCHAR(400) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.CampDonorMap','U') IS NULL
CREATE TABLE dbo.CampDonorMap (
  CampDonorMapId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CampId BIGINT NOT NULL,
  DonorId BIGINT NULL,
  CenterId BIGINT NULL,
  RegisteredAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.CollectionRecord','U') IS NULL
CREATE TABLE dbo.CollectionRecord (
  CollectionId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  BranchId BIGINT NULL,
  CampId BIGINT NULL,
  DonorId BIGINT NULL,
  BloodBagNumber NVARCHAR(100) NULL,
  BagBarcode NVARCHAR(150) NULL,
  BagLotNumber NVARCHAR(100) NULL,
  BagVolumeMl INT NULL,
  CollectorEmployeeId BIGINT NULL,
  CollectionLocationType NVARCHAR(50) NULL, -- Center/Camp/Mobile
  CollectionStartTime DATETIME2(7) NULL,
  CollectionEndTime DATETIME2(7) NULL,
  Notes NVARCHAR(2000) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  CreatedBy BIGINT NULL
);
GO

IF OBJECT_ID('dbo.CollectionStaffMap','U') IS NULL
CREATE TABLE dbo.CollectionStaffMap (
  CollectionStaffMapId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CollectionId BIGINT NOT NULL,
  EmployeeId BIGINT NULL,
  Role NVARCHAR(100) NULL, -- Phlebotomist/Assistant
  AssignedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.CampInventory','U') IS NULL
CREATE TABLE dbo.CampInventory (
  CampInventoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CampId BIGINT NOT NULL,
  ItemName NVARCHAR(300) NULL,
  Quantity INT NULL,
  Unit NVARCHAR(50) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.CampExpenseLog','U') IS NULL
CREATE TABLE dbo.CampExpenseLog (
  CampExpenseId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CampId BIGINT NOT NULL,
  ExpenseCategory NVARCHAR(200) NULL,
  Amount DECIMAL(18,2) NULL,
  Notes NVARCHAR(2000) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

/* ---------------------------
   5. Testing & Screening
   --------------------------- */
IF OBJECT_ID('dbo.BloodTestRecord','U') IS NULL
CREATE TABLE dbo.BloodTestRecord (
  TestRecordId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  CollectionId BIGINT NULL,
  BagNumber NVARCHAR(100) NULL,
  SampleTakenAt DATETIME2(7) NULL,
  PerformedBy BIGINT NULL,
  OverallStatus NVARCHAR(100) NULL, -- Pending/Completed/Failed
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.BloodTestResult','U') IS NULL
CREATE TABLE dbo.BloodTestResult (
  TestResultId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  TestRecordId BIGINT NULL,
  BagId BIGINT NULL,
  TestCode NVARCHAR(100) NOT NULL, -- HIV/HBsAg/HCV/VDRL/MALARIA
  Result NVARCHAR(100) NULL, -- Reactive/NonReactive/Indeterminate
  Method NVARCHAR(200) NULL,
  KitLotNo NVARCHAR(200) NULL,
  PerformedBy BIGINT NULL,
  PerformedAt DATETIME2(7) NULL,
  Remarks NVARCHAR(2000) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.TestKitMaster','U') IS NULL
CREATE TABLE dbo.TestKitMaster (
  TestKitId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  KitName NVARCHAR(300) NULL,
  Manufacturer NVARCHAR(300) NULL,
  LotNumber NVARCHAR(200) NULL,
  ExpiryDate DATE NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.TestTechnicianMap','U') IS NULL
CREATE TABLE dbo.TestTechnicianMap (
  TestTechnicianMapId BIGINT IDENTITY(1,1) PRIMARY KEY,
  TestRecordId BIGINT NOT NULL,
  TechnicianId BIGINT NULL,
  AssignedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.QualityControlRecord','U') IS NULL
CREATE TABLE dbo.QualityControlRecord (
  QCRecordId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  DeviceId BIGINT NULL,
  QCDate DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  QCDetail NVARCHAR(2000) NULL,
  PerformedBy BIGINT NULL
);
GO

/* ---------------------------
   6. Bag, Component & Storage
   --------------------------- */
IF OBJECT_ID('dbo.BloodBagMaster','U') IS NULL
CREATE TABLE dbo.BloodBagMaster (
  BagId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  BloodBagNumber NVARCHAR(100) NOT NULL,
  CollectionId BIGINT NULL,
  DonorId BIGINT NULL,
  BagBarcode NVARCHAR(200) NULL,
  BagLotNumber NVARCHAR(100) NULL,
  BagVolumeMl INT NULL,
  BagType NVARCHAR(100) NULL,
  BagStatus NVARCHAR(100) NULL,
  InitialCollectedAt DATETIME2(7) NULL,
  ExpiryDate DATE NULL,
  QuarantineReason NVARCHAR(1000) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  CreatedBy BIGINT NULL,
  UpdatedAt DATETIME2(7) NULL
);
GO

IF OBJECT_ID('dbo.ComponentPreparation','U') IS NULL
CREATE TABLE dbo.ComponentPreparation (
  PreparationId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  ParentBagId BIGINT NULL,
  ComponentType NVARCHAR(100) NULL,
  VolumeMl INT NULL,
  PreparedBy BIGINT NULL,
  PreparedAt DATETIME2(7) NULL,
  Notes NVARCHAR(2000) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.ComponentMaster','U') IS NULL
CREATE TABLE dbo.ComponentMaster (
  ComponentId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  ComponentCode NVARCHAR(100) NOT NULL,
  ParentBagId BIGINT NULL,
  ComponentType NVARCHAR(50) NULL,
  VolumeMl INT NULL,
  ExpiryDate DATE NULL,
  StorageLocation NVARCHAR(200) NULL,
  CurrentStatus NVARCHAR(100) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.ComponentPreparationLog','U') IS NULL
CREATE TABLE dbo.ComponentPreparationLog (
  PreparationLogId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  PreparationId BIGINT NULL,
  ComponentId BIGINT NULL,
  Notes NVARCHAR(2000) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.ComponentStorage','U') IS NULL
CREATE TABLE dbo.ComponentStorage (
  StorageId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  ComponentId BIGINT NULL,
  FridgeId BIGINT NULL,
  StorageLocation NVARCHAR(200) NULL,
  PlacedAt DATETIME2(7) NULL,
  Notes NVARCHAR(2000) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.ComponentTransferLog','U') IS NULL
CREATE TABLE dbo.ComponentTransferLog (
  TransferId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  ComponentId BIGINT NULL,
  FromCenterId BIGINT NULL,
  ToCenterId BIGINT NULL,
  TransferDate DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  TransportDetails NVARCHAR(1000) NULL,
  CreatedBy BIGINT NULL
);
GO

IF OBJECT_ID('dbo.DiscardRecord','U') IS NULL
CREATE TABLE dbo.DiscardRecord (
  DiscardId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  BagId BIGINT NULL,
  ComponentId BIGINT NULL,
  DiscardReason NVARCHAR(500) NULL,
  DiscardedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  DiscardedBy BIGINT NULL,
  Notes NVARCHAR(2000) NULL
);
GO

/* ---------------------------
   7. Inventory & Issue Management
   --------------------------- */
IF OBJECT_ID('dbo.InventoryStock','U') IS NULL
CREATE TABLE dbo.InventoryStock (
  InventoryStockId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  ComponentType NVARCHAR(100) NULL,
  BloodGroup NVARCHAR(20) NULL,
  AvailableQty INT NOT NULL DEFAULT 0,
  ReservedQty INT NOT NULL DEFAULT 0,
  QuarantinedQty INT NOT NULL DEFAULT 0,
  LastUpdatedAt DATETIME2(7) NULL,
  LastUpdatedBy BIGINT NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.InventoryTransactionLog','U') IS NULL
CREATE TABLE dbo.InventoryTransactionLog (
  InventoryTxId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  TransactionType NVARCHAR(50) NOT NULL, -- IN/OUT/TRANSFER/EXPIRE/ADJUST
  ReferenceType NVARCHAR(100) NULL,
  ReferenceId NVARCHAR(200) NULL,
  ComponentId BIGINT NULL,
  BagId BIGINT NULL,
  Quantity INT NOT NULL DEFAULT 1,
  FromLocation NVARCHAR(200) NULL,
  ToLocation NVARCHAR(200) NULL,
  Notes NVARCHAR(2000) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  CreatedBy BIGINT NULL
);
GO

IF OBJECT_ID('dbo.HospitalMaster','U') IS NULL
CREATE TABLE dbo.HospitalMaster (
  HospitalId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  HospitalCode NVARCHAR(100) NULL,
  HospitalName NVARCHAR(300) NOT NULL,
  Address NVARCHAR(500) NULL,
  ContactPerson NVARCHAR(200) NULL,
  Phone NVARCHAR(100) NULL,
  Email NVARCHAR(200) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.PatientRequest','U') IS NULL
CREATE TABLE dbo.PatientRequest (
  RequestId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  HospitalId BIGINT NULL,
  PatientName NVARCHAR(300) NULL,
  PatientAge INT NULL,
  PatientGender NVARCHAR(50) NULL,
  BloodGroup NVARCHAR(20) NULL,
  ComponentType NVARCHAR(100) NULL,
  UnitsRequested INT NULL,
  RequestDate DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  RequestUrgency NVARCHAR(50) NULL,
  PrescriptionAttachmentId BIGINT NULL,
  RequestedByUserId BIGINT NULL,
  RelatedIssueId BIGINT NULL
);
GO

IF OBJECT_ID('dbo.CrossMatchRecord','U') IS NULL
CREATE TABLE dbo.CrossMatchRecord (
  CrossMatchId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  RequestId BIGINT NULL,
  ComponentId BIGINT NULL,
  Result NVARCHAR(200) NULL,
  Method NVARCHAR(200) NULL,
  PerformedBy BIGINT NULL,
  PerformedAt DATETIME2(7) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.IssueRecord','U') IS NULL
CREATE TABLE dbo.IssueRecord (
  IssueRecordId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  ComponentId BIGINT NULL,
  BagId BIGINT NULL,
  PatientName NVARCHAR(300) NULL,
  HospitalId BIGINT NULL,
  IssueDate DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  IssuedByUserId BIGINT NULL,
  IssueType NVARCHAR(50) NULL,
  IssueSlipNumber NVARCHAR(200) NULL,
  RelatedBillingId BIGINT NULL,
  Notes NVARCHAR(2000) NULL
);
GO

IF OBJECT_ID('dbo.ReturnRecord','U') IS NULL
CREATE TABLE dbo.ReturnRecord (
  ReturnId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  IssueRecordId BIGINT NULL,
  ComponentId BIGINT NULL,
  ReturnDate DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  Reason NVARCHAR(1000) NULL,
  CreatedBy BIGINT NULL
);
GO

IF OBJECT_ID('dbo.ReplacementDonor','U') IS NULL
CREATE TABLE dbo.ReplacementDonor (
  ReplacementDonorId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  PatientRequestId BIGINT NULL,
  DonorId BIGINT NULL,
  DonatedAt DATETIME2(7) NULL
);
GO

/* ---------------------------
   8. Billing & Finance
   --------------------------- */
IF OBJECT_ID('dbo.ServiceChargeMaster','U') IS NULL
CREATE TABLE dbo.ServiceChargeMaster (
  ServiceChargeId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  ServiceCode NVARCHAR(100) NULL,
  ServiceName NVARCHAR(300) NOT NULL,
  Amount DECIMAL(18,2) NULL,
  IsActive BIT NOT NULL DEFAULT 1,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.BillingTransaction','U') IS NULL
CREATE TABLE dbo.BillingTransaction (
  BillingTransactionId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  InvoiceNumber NVARCHAR(200) NULL,
  PatientId BIGINT NULL,
  TotalAmount DECIMAL(18,2) NULL,
  TaxAmount DECIMAL(18,2) NULL DEFAULT 0,
  Discount DECIMAL(18,2) NULL DEFAULT 0,
  PaymentStatus NVARCHAR(50) NULL,
  PaymentMode NVARCHAR(50) NULL,
  InvoiceDate DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  CreatedBy BIGINT NULL
);
GO

IF OBJECT_ID('dbo.InvoiceDetail','U') IS NULL
CREATE TABLE dbo.InvoiceDetail (
  InvoiceDetailId BIGINT IDENTITY(1,1) PRIMARY KEY,
  BillingTransactionId BIGINT NOT NULL,
  ComponentId BIGINT NULL,
  ServiceChargeId BIGINT NULL,
  ServiceName NVARCHAR(300) NULL,
  Quantity INT NOT NULL DEFAULT 1,
  UnitPrice DECIMAL(18,2) NULL,
  LineTotal DECIMAL(18,2) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.PaymentRecord','U') IS NULL
CREATE TABLE dbo.PaymentRecord (
  PaymentId BIGINT IDENTITY(1,1) PRIMARY KEY,
  BillingTransactionId BIGINT NULL,
  CenterId BIGINT NULL,
  PaymentDate DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  Amount DECIMAL(18,2) NULL,
  PaymentMode NVARCHAR(100) NULL,
  Reference NVARCHAR(200) NULL,
  CreatedBy BIGINT NULL
);
GO

IF OBJECT_ID('dbo.ExpenseMaster','U') IS NULL
CREATE TABLE dbo.ExpenseMaster (
  ExpenseId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  ExpenseDate DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  Category NVARCHAR(200) NULL,
  Amount DECIMAL(18,2) NULL,
  Notes NVARCHAR(2000) NULL,
  CreatedBy BIGINT NULL
);
GO

/* ---------------------------
   9. Reporting & Compliance
   --------------------------- */
IF OBJECT_ID('dbo.MonthlyReportLog','U') IS NULL
CREATE TABLE dbo.MonthlyReportLog (
  MonthlyReportId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  ReportYear INT NULL,
  ReportMonth INT NULL,
  ReportType NVARCHAR(200) NULL,
  DataSnapshot NVARCHAR(MAX) NULL, -- JSON
  FilePath NVARCHAR(4000) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.ExportFileLog','U') IS NULL
CREATE TABLE dbo.ExportFileLog (
  ExportFileLogId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  FileName NVARCHAR(400) NULL,
  FileType NVARCHAR(100) NULL,
  FilePath NVARCHAR(4000) NULL,
  ExportedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  ExportedBy BIGINT NULL
);
GO

IF OBJECT_ID('dbo.DataUploadHistory','U') IS NULL
CREATE TABLE dbo.DataUploadHistory (
  UploadHistoryId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  TargetSystem NVARCHAR(200) NULL, -- e-RaktKosh / State Portal
  PayloadPath NVARCHAR(4000) NULL,
  ResponseStatus NVARCHAR(200) NULL,
  UploadedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.ReportTemplate','U') IS NULL
CREATE TABLE dbo.ReportTemplate (
  ReportTemplateId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  TemplateName NVARCHAR(300) NULL,
  TemplateFilePath NVARCHAR(4000) NULL,
  FileType NVARCHAR(50) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.AnalyticsDashboardData','U') IS NULL
CREATE TABLE dbo.AnalyticsDashboardData (
  DashboardDataId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  DataKey NVARCHAR(200) NULL,
  DataValue NVARCHAR(MAX) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

/* ---------------------------
   10. Communication & Outreach
   --------------------------- */
IF OBJECT_ID('dbo.NotificationMaster','U') IS NULL
CREATE TABLE dbo.NotificationMaster (
  NotificationId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  NotificationType NVARCHAR(50) NULL,
  Title NVARCHAR(300) NULL,
  Body NVARCHAR(4000) NULL,
  TargetAudience NVARCHAR(500) NULL,
  IsActive BIT NOT NULL DEFAULT 1,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.SmsTemplateMaster','U') IS NULL
CREATE TABLE dbo.SmsTemplateMaster (
  SmsTemplateId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  TemplateCode NVARCHAR(200) NULL,
  TemplateText NVARCHAR(2000) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.EmailTemplateMaster','U') IS NULL
CREATE TABLE dbo.EmailTemplateMaster (
  EmailTemplateId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  TemplateCode NVARCHAR(200) NULL,
  Subject NVARCHAR(400) NULL,
  BodyHtml NVARCHAR(MAX) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.OutboxLog','U') IS NULL
CREATE TABLE dbo.OutboxLog (
  OutboxId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  Channel NVARCHAR(50) NULL,
  Recipient NVARCHAR(500) NULL,
  Message NVARCHAR(MAX) NULL,
  SentAt DATETIME2(7) NULL,
  Status NVARCHAR(100) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.FeedbackMaster','U') IS NULL
CREATE TABLE dbo.FeedbackMaster (
  FeedbackId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  Source NVARCHAR(100) NULL,
  Subject NVARCHAR(300) NULL,
  Message NVARCHAR(MAX) NULL,
  SubmittedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  SubmittedBy NVARCHAR(300) NULL
);
GO

IF OBJECT_ID('dbo.NewsletterSubscription','U') IS NULL
CREATE TABLE dbo.NewsletterSubscription (
  SubscriptionId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  Email NVARCHAR(200) NULL,
  SubscribedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  IsActive BIT NOT NULL DEFAULT 1
);
GO

/* ---------------------------
   11. System Config & Support
   --------------------------- */
IF OBJECT_ID('dbo.SystemConfig','U') IS NULL
CREATE TABLE dbo.SystemConfig (
  ConfigId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  ConfigKey NVARCHAR(200) NOT NULL,
  ConfigValue NVARCHAR(MAX) NULL,
  Description NVARCHAR(1000) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.BackupLog','U') IS NULL
CREATE TABLE dbo.BackupLog (
  BackupLogId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  BackupType NVARCHAR(100) NULL,
  BackupPath NVARCHAR(4000) NULL,
  BackupStartedAt DATETIME2(7) NULL,
  BackupCompletedAt DATETIME2(7) NULL,
  Status NVARCHAR(100) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.ErrorLog','U') IS NULL
CREATE TABLE dbo.ErrorLog (
  ErrorId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  ErrorMessage NVARCHAR(MAX) NULL,
  StackTrace NVARCHAR(MAX) NULL,
  OccurredAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.ApiIntegrationMaster','U') IS NULL
CREATE TABLE dbo.ApiIntegrationMaster (
  ApiIntegrationId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  IntegrationName NVARCHAR(200) NULL,
  BaseUrl NVARCHAR(1000) NULL,
  ApiKey NVARCHAR(1000) NULL,
  Username NVARCHAR(500) NULL,
  PasswordEncrypted NVARCHAR(1000) NULL,
  LastSyncAt DATETIME2(7) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.SchedulerJobLog','U') IS NULL
CREATE TABLE dbo.SchedulerJobLog (
  JobLogId BIGINT IDENTITY(1,1) PRIMARY KEY,
  JobName NVARCHAR(200) NULL,
  LastRunAt DATETIME2(7) NULL,
  Status NVARCHAR(100) NULL,
  Message NVARCHAR(2000) NULL
);
GO

/* ---------------------------
   12. Master & Reference Data
   --------------------------- */
IF OBJECT_ID('dbo.StateMaster','U') IS NULL
CREATE TABLE dbo.StateMaster (
  StateId BIGINT IDENTITY(1,1) PRIMARY KEY,
  StateName NVARCHAR(200) NOT NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.DistrictMaster','U') IS NULL
CREATE TABLE dbo.DistrictMaster (
  DistrictId BIGINT IDENTITY(1,1) PRIMARY KEY,
  StateId BIGINT NULL,
  DistrictName NVARCHAR(200) NOT NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.CityMaster','U') IS NULL
CREATE TABLE dbo.CityMaster (
  CityId BIGINT IDENTITY(1,1) PRIMARY KEY,
  DistrictId BIGINT NULL,
  CityName NVARCHAR(200) NOT NULL,
  Pincode NVARCHAR(20) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.PincodeMaster','U') IS NULL
CREATE TABLE dbo.PincodeMaster (
  PincodeId BIGINT IDENTITY(1,1) PRIMARY KEY,
  Pincode NVARCHAR(20) NOT NULL,
  AreaName NVARCHAR(300) NULL,
  CityId BIGINT NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.BloodGroupMaster','U') IS NULL
CREATE TABLE dbo.BloodGroupMaster (
  BloodGroupId BIGINT IDENTITY(1,1) PRIMARY KEY,
  BloodGroupCode NVARCHAR(10) NOT NULL,
  Description NVARCHAR(200) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.ComponentTypeMaster','U') IS NULL
CREATE TABLE dbo.ComponentTypeMaster (
  ComponentTypeId BIGINT IDENTITY(1,1) PRIMARY KEY,
  ComponentTypeCode NVARCHAR(50) NOT NULL,
  Description NVARCHAR(200) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.ReasonMaster','U') IS NULL
CREATE TABLE dbo.ReasonMaster (
  ReasonId BIGINT IDENTITY(1,1) PRIMARY KEY,
  Category NVARCHAR(100) NULL,
  ReasonCode NVARCHAR(100) NULL,
  ReasonText NVARCHAR(500) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.HolidayMaster','U') IS NULL
CREATE TABLE dbo.HolidayMaster (
  HolidayId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  HolidayDate DATE NOT NULL,
  Description NVARCHAR(400) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

/* ---------------------------
   13. Emergency & Requests
   --------------------------- */
IF OBJECT_ID('dbo.EmergencyRequest','U') IS NULL
CREATE TABLE dbo.EmergencyRequest (
  EmergencyRequestId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  HospitalId BIGINT NULL,
  PatientName NVARCHAR(300) NULL,
  BloodGroup NVARCHAR(20) NULL,
  ComponentType NVARCHAR(100) NULL,
  UnitsRequired INT NULL,
  RequestStatus NVARCHAR(100) NULL,
  RequestedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  RequestedByUserId BIGINT NULL,
  FulfilledAt DATETIME2(7) NULL,
  Notes NVARCHAR(2000) NULL
);
GO

IF OBJECT_ID('dbo.EmergencyDonorResponse','U') IS NULL
CREATE TABLE dbo.EmergencyDonorResponse (
  ResponseId BIGINT IDENTITY(1,1) PRIMARY KEY,
  EmergencyRequestId BIGINT NOT NULL,
  DonorId BIGINT NULL,
  ResponseContact NVARCHAR(200) NULL,
  RespondedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  IsVerified BIT NOT NULL DEFAULT 0
);
GO

IF OBJECT_ID('dbo.RequestStatusLog','U') IS NULL
CREATE TABLE dbo.RequestStatusLog (
  RequestStatusLogId BIGINT IDENTITY(1,1) PRIMARY KEY,
  RequestId BIGINT NULL,
  OldStatus NVARCHAR(100) NULL,
  NewStatus NVARCHAR(100) NULL,
  ChangedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  ChangedBy BIGINT NULL,
  Notes NVARCHAR(2000) NULL
);
GO

/* ---------------------------
   14. Multi-center & API Integration Support
   --------------------------- */
IF OBJECT_ID('dbo.CenterUserMap','U') IS NULL
CREATE TABLE dbo.CenterUserMap (
  CenterUserMapId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NOT NULL,
  UserId BIGINT NOT NULL,
  RoleId BIGINT NULL,
  AssignedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.CenterConfig','U') IS NULL
CREATE TABLE dbo.CenterConfig (
  CenterConfigId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NOT NULL,
  ConfigKey NVARCHAR(200) NOT NULL,
  ConfigValue NVARCHAR(MAX) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.PortalUploadQueue','U') IS NULL
CREATE TABLE dbo.PortalUploadQueue (
  UploadQueueId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  TargetPortal NVARCHAR(200) NULL,
  PayloadPath NVARCHAR(4000) NULL,
  Status NVARCHAR(100) NULL,
  AttemptCount INT NOT NULL DEFAULT 0,
  NextAttemptAt DATETIME2(7) NULL,
  CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.ApiResponseLog','U') IS NULL
CREATE TABLE dbo.ApiResponseLog (
  ApiResponseLogId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  ApiIntegrationId BIGINT NULL,
  RequestPayload NVARCHAR(MAX) NULL,
  ResponsePayload NVARCHAR(MAX) NULL,
  StatusCode NVARCHAR(50) NULL,
  CalledAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

/* ---------------------------
   15. Utilities & Attachments
   --------------------------- */
IF OBJECT_ID('dbo.SequenceCounters','U') IS NULL
CREATE TABLE dbo.SequenceCounters (
  SequenceCounterId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  SequenceName NVARCHAR(100) NOT NULL,
  LastValue BIGINT NOT NULL DEFAULT 0,
  Prefix NVARCHAR(20) NULL,
  Suffix NVARCHAR(20) NULL,
  IncrementBy INT NOT NULL DEFAULT 1,
  UpdatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  UpdatedBy BIGINT NULL
);
GO

IF OBJECT_ID('dbo.AttachmentStore','U') IS NULL
CREATE TABLE dbo.AttachmentStore (
  AttachmentId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  RelatedTable NVARCHAR(128) NULL,
  RelatedRecordId NVARCHAR(100) NULL,
  FileName NVARCHAR(260) NULL,
  ContentType NVARCHAR(100) NULL,
  FileSize BIGINT NULL,
  FilePath NVARCHAR(4000) NULL,
  UploadedBy BIGINT NULL,
  UploadedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID('dbo.UserSettings','U') IS NULL
CREATE TABLE dbo.UserSettings (
  UserSettingsId BIGINT IDENTITY(1,1) PRIMARY KEY,
  UserId BIGINT NOT NULL,
  SettingsKey NVARCHAR(200) NOT NULL,
  SettingsValue NVARCHAR(MAX) NULL,
  UpdatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

/* ---------------------------
   16. Optional: lightweight audit trail for critical changes (if needed)
   --------------------------- */
IF OBJECT_ID('dbo.ChangeLog','U') IS NULL
CREATE TABLE dbo.ChangeLog (
  ChangeLogId BIGINT IDENTITY(1,1) PRIMARY KEY,
  CenterId BIGINT NULL,
  EntityName NVARCHAR(200) NULL,
  EntityId NVARCHAR(100) NULL,
  ChangeType NVARCHAR(50) NULL,
  ChangeData NVARCHAR(MAX) NULL,
  ChangedBy BIGINT NULL,
  ChangedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

/* ---------------------------
   17. Final housekeeping: small indexes examples (non-FK)
   --------------------------- */
-- Indexes can be added as needed post-deploy depending on query profiles.
-- Example: create index on CollectionRecord.CreatedAt for reporting queries.
IF OBJECT_ID('dbo.CollectionRecord','U') IS NOT NULL
BEGIN
  IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.CollectionRecord') AND name = 'IX_CollectionRecord_CreatedAt')
    CREATE INDEX IX_CollectionRecord_CreatedAt ON dbo.CollectionRecord (CreatedAt);
END
GO

IF OBJECT_ID('dbo.DonorMaster','U') IS NOT NULL
BEGIN
  IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.DonorMaster') AND name = 'IX_DonorMaster_Phone')
    CREATE INDEX IX_DonorMaster_Phone ON dbo.DonorMaster (Phone);
END
GO

PRINT 'Schema creation script BloodCenterOS_DB_v3.sql completed.';
GO
