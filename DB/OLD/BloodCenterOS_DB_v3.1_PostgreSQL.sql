-- ============================================================================
-- BloodCenterOS Database Schema v3.1 — PostgreSQL Conversion
-- Original: SQL Server (T-SQL) → PostgreSQL (PL/pgSQL)
-- Converted on: 16 July 2026
-- Design Philosophy: No foreign key constraints (matches original design;
--   data integrity enforced via application layer / stored procedures)
-- ============================================================================

-- ============================================================================
-- 1. User & Access Management
-- ============================================================================

CREATE TABLE UserMaster (
    UserId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    UserName VARCHAR(150) NOT NULL,
    DisplayName VARCHAR(200),
    Email VARCHAR(200),
    Phone VARCHAR(50),
    PasswordHash VARCHAR(512),
    PasswordSalt VARCHAR(256),
    IsLocked BOOLEAN NOT NULL DEFAULT FALSE,
    LastLoginAt TIMESTAMPTZ,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CreatedBy BIGINT,
    UpdatedAt TIMESTAMPTZ,
    UpdatedBy BIGINT,
    PRIMARY KEY (UserId)
);

CREATE TABLE RoleMaster (
    RoleId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    RoleName VARCHAR(150) NOT NULL,
    Description VARCHAR(500),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CreatedBy BIGINT,
    UpdatedAt TIMESTAMPTZ,
    UpdatedBy BIGINT,
    PRIMARY KEY (RoleId)
);

CREATE TABLE PermissionMaster (
    PermissionId BIGSERIAL NOT NULL,
    PermissionCode VARCHAR(200) NOT NULL,
    Description VARCHAR(500),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (PermissionId)
);

CREATE TABLE RolePermissionMap (
    RolePermissionMapId BIGSERIAL NOT NULL,
    RoleId BIGINT NOT NULL,
    PermissionId BIGINT NOT NULL,
    CenterId BIGINT,
    AssignedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    AssignedBy BIGINT,
    PRIMARY KEY (RolePermissionMapId)
);

CREATE TABLE UserRoleMap (
    UserRoleMapId BIGSERIAL NOT NULL,
    UserId BIGINT NOT NULL,
    RoleId BIGINT NOT NULL,
    CenterId BIGINT,
    AssignedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    AssignedBy BIGINT,
    PRIMARY KEY (UserRoleMapId)
);

CREATE TABLE LoginHistory (
    LoginHistoryId BIGSERIAL NOT NULL,
    UserId BIGINT NOT NULL,
    CenterId BIGINT,
    LoginAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    LogoutAt TIMESTAMPTZ,
    IpAddress VARCHAR(50),
    UserAgent VARCHAR(500),
    PRIMARY KEY (LoginHistoryId)
);

CREATE TABLE AuditLog (
    AuditLogId BIGSERIAL NOT NULL,
    PropertyOwnerId BIGINT NOT NULL,
    UserId BIGINT NOT NULL,
    Action VARCHAR(100) NOT NULL,
    TableName VARCHAR(200),
    RecordId VARCHAR(100),
    ActionDetails VARCHAR(4000),
    OldValue VARCHAR(4000),
    NewValue VARCHAR(4000),
    IpAddress VARCHAR(50),
    UserAgent VARCHAR(500),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (AuditLogId)
);

CREATE TABLE CenterUserMap (
    CenterUserMapId BIGSERIAL NOT NULL,
    CenterId BIGINT NOT NULL,
    UserId BIGINT NOT NULL,
    RoleId BIGINT,
    AssignedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (CenterUserMapId)
);

CREATE TABLE UserSettings (
    UserSettingsId BIGSERIAL NOT NULL,
    UserId BIGINT NOT NULL,
    SettingsKey VARCHAR(200) NOT NULL,
    SettingsValue TEXT,
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (UserSettingsId)
);

-- ============================================================================
-- 2. Blood Center & Infrastructure
-- ============================================================================

CREATE TABLE BloodCenterMaster (
    CenterId BIGSERIAL NOT NULL,
    CenterCode VARCHAR(100),
    CenterName VARCHAR(250) NOT NULL,
    LicenseNumber VARCHAR(100),
    AddressLine1 VARCHAR(300),
    AddressLine2 VARCHAR(300),
    City VARCHAR(100),
    District VARCHAR(100),
    State VARCHAR(100),
    Pincode VARCHAR(20),
    Phone VARCHAR(50),
    Email VARCHAR(200),
    Website VARCHAR(200),
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CreatedBy BIGINT,
    UpdatedAt TIMESTAMPTZ,
    UpdatedBy BIGINT,
    PRIMARY KEY (CenterId)
);

CREATE TABLE BranchMaster (
    BranchId BIGSERIAL NOT NULL,
    CenterId BIGINT NOT NULL,
    BranchCode VARCHAR(100),
    BranchName VARCHAR(250),
    AddressLine1 VARCHAR(300),
    AddressLine2 VARCHAR(300),
    City VARCHAR(100),
    State VARCHAR(100),
    Pincode VARCHAR(20),
    Phone VARCHAR(50),
    Email VARCHAR(200),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CreatedBy BIGINT,
    PRIMARY KEY (BranchId)
);

CREATE TABLE DepartmentMaster (
    DepartmentId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    DepartmentCode VARCHAR(100),
    DepartmentName VARCHAR(200) NOT NULL,
    Description VARCHAR(500),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (DepartmentId)
);

CREATE TABLE DesignationMaster (
    DesignationId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    DesignationName VARCHAR(200) NOT NULL,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (DesignationId)
);

CREATE TABLE EmployeeMaster (
    EmployeeId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    EmployeeCode VARCHAR(100),
    FirstName VARCHAR(150),
    LastName VARCHAR(150),
    Email VARCHAR(200),
    Phone VARCHAR(50),
    Designation VARCHAR(150),
    DepartmentId BIGINT,
    JoinDate DATE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CreatedBy BIGINT,
    UpdatedAt TIMESTAMPTZ,
    PRIMARY KEY (EmployeeId)
);

CREATE TABLE DeviceMaster (
    DeviceId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    DeviceName VARCHAR(200),
    DeviceType VARCHAR(100),
    SerialNumber VARCHAR(200),
    PurchaseDate DATE,
    WarrantyEndDate DATE,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (DeviceId)
);

CREATE TABLE FridgeStorageMaster (
    FridgeId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    FridgeCode VARCHAR(100),
    FridgeName VARCHAR(200),
    Capacity INTEGER,
    Location VARCHAR(200),
    TemperatureLogRequired BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (FridgeId)
);

-- ============================================================================
-- 3. Donor Management
-- ============================================================================

CREATE TABLE DonorMaster (
    DonorId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    DonorCode VARCHAR(100),
    FirstName VARCHAR(200) NOT NULL,
    LastName VARCHAR(200),
    Gender VARCHAR(50),
    DateOfBirth DATE,
    BloodGroup VARCHAR(10),
    Phone VARCHAR(50),
    Email VARCHAR(200),
    AadhaarNumber VARCHAR(20),
    AddressLine1 VARCHAR(300),
    AddressLine2 VARCHAR(300),
    City VARCHAR(100),
    Pincode VARCHAR(20),
    Occupation VARCHAR(200),
    PreferredLanguage VARCHAR(50),
    LastDonationDate DATE,
    TotalDonations INTEGER NOT NULL DEFAULT 0,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CreatedBy BIGINT,
    UpdatedAt TIMESTAMPTZ,
    UpdatedBy BIGINT,
    PRIMARY KEY (DonorId)
);

CREATE INDEX IX_DonorMaster_Phone ON DonorMaster (Phone);

CREATE TABLE DonorHealthHistory (
    DonorHealthHistoryId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    DonorId BIGINT NOT NULL,
    VisitDate TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    WeightKg NUMERIC(5,2),
    Temperature NUMERIC(5,2),
    BloodPressure VARCHAR(50),
    Hemoglobin NUMERIC(5,2),
    PulseRate INTEGER,
    Remarks VARCHAR(2000),
    RecordedBy BIGINT,
    PRIMARY KEY (DonorHealthHistoryId)
);

CREATE TABLE DonorDonationHistory (
    DonationId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    DonorId BIGINT NOT NULL,
    CollectionId BIGINT,
    DonationDate TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    DonationType VARCHAR(100),
    VolumeMl INTEGER,
    BagNumber VARCHAR(100),
    Remarks VARCHAR(1000),
    CreatedBy BIGINT,
    PRIMARY KEY (DonationId)
);

CREATE TABLE DeferralRecord (
    DeferralId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    DonorId BIGINT NOT NULL,
    DeferralDate TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    Reason VARCHAR(1000),
    DeferralUntil DATE,
    Notes VARCHAR(2000),
    CreatedBy BIGINT,
    PRIMARY KEY (DeferralId)
);

CREATE TABLE DonorAppointment (
    AppointmentId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    DonorId BIGINT,
    AppointmentDate TIMESTAMPTZ,
    Slot VARCHAR(100),
    Status VARCHAR(50),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CreatedBy BIGINT,
    PRIMARY KEY (AppointmentId)
);

CREATE TABLE DonorCommunicationLog (
    CommId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    DonorId BIGINT,
    Channel VARCHAR(50),
    Message VARCHAR(2000),
    SentAt TIMESTAMPTZ,
    SentBy BIGINT,
    Status VARCHAR(100),
    PRIMARY KEY (CommId)
);

-- ============================================================================
-- 4. Blood Camps & Collection
-- ============================================================================

CREATE TABLE BloodCampMaster (
    CampId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    CampCode VARCHAR(100),
    CampName VARCHAR(300),
    OrganizerId BIGINT,
    Venue VARCHAR(500),
    City VARCHAR(200),
    CampDate DATE,
    StartTime TIMESTAMPTZ,
    EndTime TIMESTAMPTZ,
    TotalDonorsExpected INTEGER,
    TotalDonorsCollected INTEGER,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CreatedBy BIGINT,
    PRIMARY KEY (CampId)
);

CREATE TABLE CampOrganizer (
    OrganizerId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    OrganizerName VARCHAR(300),
    ContactPerson VARCHAR(200),
    Phone VARCHAR(50),
    Email VARCHAR(200),
    Address VARCHAR(400),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (OrganizerId)
);

CREATE TABLE CampDonorMap (
    CampDonorMapId BIGSERIAL NOT NULL,
    CampId BIGINT NOT NULL,
    DonorId BIGINT,
    CenterId BIGINT,
    RegisteredAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (CampDonorMapId)
);

CREATE TABLE CollectionRecord (
    CollectionId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    BranchId BIGINT,
    CampId BIGINT,
    DonorId BIGINT,
    BloodBagNumber VARCHAR(100),
    BagBarcode VARCHAR(150),
    BagLotNumber VARCHAR(100),
    BagVolumeMl INTEGER,
    CollectorEmployeeId BIGINT,
    CollectionLocationType VARCHAR(50),
    CollectionStartTime TIMESTAMPTZ,
    CollectionEndTime TIMESTAMPTZ,
    Notes VARCHAR(2000),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CreatedBy BIGINT,
    PRIMARY KEY (CollectionId)
);

CREATE INDEX IX_CollectionRecord_CreatedAt ON CollectionRecord (CreatedAt);

CREATE TABLE CollectionStaffMap (
    CollectionStaffMapId BIGSERIAL NOT NULL,
    CollectionId BIGINT NOT NULL,
    EmployeeId BIGINT,
    Role VARCHAR(100),
    AssignedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (CollectionStaffMapId)
);

CREATE TABLE CampInventory (
    CampInventoryId BIGSERIAL NOT NULL,
    CampId BIGINT NOT NULL,
    ItemName VARCHAR(300),
    Quantity INTEGER,
    Unit VARCHAR(50),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (CampInventoryId)
);

CREATE TABLE CampExpenseLog (
    CampExpenseId BIGSERIAL NOT NULL,
    CampId BIGINT NOT NULL,
    ExpenseCategory VARCHAR(200),
    Amount NUMERIC(18,2),
    Notes VARCHAR(2000),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (CampExpenseId)
);

-- ============================================================================
-- 5. Blood Testing & Screening
-- ============================================================================

CREATE TABLE BloodTestRecord (
    TestRecordId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    CollectionId BIGINT,
    BagNumber VARCHAR(100),
    SampleTakenAt TIMESTAMPTZ,
    PerformedBy BIGINT,
    OverallStatus VARCHAR(100),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (TestRecordId)
);

CREATE TABLE BloodTestResult (
    TestResultId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    TestRecordId BIGINT,
    BagId BIGINT,
    TestCode VARCHAR(100) NOT NULL,
    Result VARCHAR(100),
    Method VARCHAR(200),
    KitLotNo VARCHAR(200),
    PerformedBy BIGINT,
    PerformedAt TIMESTAMPTZ,
    Remarks VARCHAR(2000),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (TestResultId)
);

CREATE TABLE TestKitMaster (
    TestKitId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    KitName VARCHAR(300),
    Manufacturer VARCHAR(300),
    LotNumber VARCHAR(200),
    ExpiryDate DATE,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (TestKitId)
);

CREATE TABLE TestTechnicianMap (
    TestTechnicianMapId BIGSERIAL NOT NULL,
    TestRecordId BIGINT NOT NULL,
    TechnicianId BIGINT,
    AssignedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (TestTechnicianMapId)
);

CREATE TABLE QualityControlRecord (
    QCRecordId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    DeviceId BIGINT,
    QCDate TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    QCDetail VARCHAR(2000),
    PerformedBy BIGINT,
    PRIMARY KEY (QCRecordId)
);

-- ============================================================================
-- 6. Component Preparation & Storage
-- ============================================================================

CREATE TABLE BloodBagMaster (
    BagId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    BloodBagNumber VARCHAR(100) NOT NULL,
    CollectionId BIGINT,
    DonorId BIGINT,
    BagBarcode VARCHAR(200),
    BagLotNumber VARCHAR(100),
    BagVolumeMl INTEGER,
    BagType VARCHAR(100),
    BagStatus VARCHAR(100),
    InitialCollectedAt TIMESTAMPTZ,
    ExpiryDate DATE,
    QuarantineReason VARCHAR(1000),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CreatedBy BIGINT,
    UpdatedAt TIMESTAMPTZ,
    PRIMARY KEY (BagId)
);

CREATE TABLE ComponentTypeMaster (
    ComponentTypeId BIGSERIAL NOT NULL,
    ComponentTypeCode VARCHAR(50) NOT NULL,
    Description VARCHAR(200),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (ComponentTypeId)
);

CREATE TABLE ComponentMaster (
    ComponentId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    ComponentCode VARCHAR(100) NOT NULL,
    ParentBagId BIGINT,
    ComponentType VARCHAR(50),
    VolumeMl INTEGER,
    ExpiryDate DATE,
    StorageLocation VARCHAR(200),
    CurrentStatus VARCHAR(100),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (ComponentId)
);

CREATE TABLE ComponentPreparation (
    PreparationId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    ParentBagId BIGINT,
    ComponentType VARCHAR(100),
    VolumeMl INTEGER,
    PreparedBy BIGINT,
    PreparedAt TIMESTAMPTZ,
    Notes VARCHAR(2000),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (PreparationId)
);

CREATE TABLE ComponentPreparationLog (
    PreparationLogId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    PreparationId BIGINT,
    ComponentId BIGINT,
    Notes VARCHAR(2000),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (PreparationLogId)
);

CREATE TABLE ComponentStorage (
    StorageId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    ComponentId BIGINT,
    FridgeId BIGINT,
    StorageLocation VARCHAR(200),
    PlacedAt TIMESTAMPTZ,
    Notes VARCHAR(2000),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (StorageId)
);

CREATE TABLE ComponentTransferLog (
    TransferId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    ComponentId BIGINT,
    FromCenterId BIGINT,
    ToCenterId BIGINT,
    TransferDate TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    TransportDetails VARCHAR(1000),
    CreatedBy BIGINT,
    PRIMARY KEY (TransferId)
);

CREATE TABLE DiscardRecord (
    DiscardId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    BagId BIGINT,
    ComponentId BIGINT,
    DiscardReason VARCHAR(500),
    DiscardedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    DiscardedBy BIGINT,
    Notes VARCHAR(2000),
    PRIMARY KEY (DiscardId)
);

-- ============================================================================
-- 7. Inventory Management
-- ============================================================================

CREATE TABLE InventoryStock (
    InventoryStockId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    ComponentType VARCHAR(100),
    BloodGroup VARCHAR(20),
    AvailableQty INTEGER NOT NULL DEFAULT 0,
    ReservedQty INTEGER NOT NULL DEFAULT 0,
    QuarantinedQty INTEGER NOT NULL DEFAULT 0,
    LastUpdatedAt TIMESTAMPTZ,
    LastUpdatedBy BIGINT,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (InventoryStockId)
);

CREATE TABLE InventoryTransactionLog (
    InventoryTxId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    TransactionType VARCHAR(50) NOT NULL,
    ReferenceType VARCHAR(100),
    ReferenceId VARCHAR(200),
    ComponentId BIGINT,
    BagId BIGINT,
    Quantity INTEGER NOT NULL DEFAULT 1,
    FromLocation VARCHAR(200),
    ToLocation VARCHAR(200),
    Notes VARCHAR(2000),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CreatedBy BIGINT,
    PRIMARY KEY (InventoryTxId)
);

-- ============================================================================
-- 8. Issue / Requests / Crossmatch / Returns
-- ============================================================================

CREATE TABLE HospitalMaster (
    HospitalId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    HospitalCode VARCHAR(100),
    HospitalName VARCHAR(300) NOT NULL,
    Address VARCHAR(500),
    ContactPerson VARCHAR(200),
    Phone VARCHAR(100),
    Email VARCHAR(200),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (HospitalId)
);

CREATE TABLE PatientRequest (
    RequestId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    HospitalId BIGINT,
    PatientName VARCHAR(300),
    PatientAge INTEGER,
    PatientGender VARCHAR(50),
    BloodGroup VARCHAR(20),
    ComponentType VARCHAR(100),
    UnitsRequested INTEGER,
    RequestDate TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    RequestUrgency VARCHAR(50),
    PrescriptionAttachmentId BIGINT,
    RequestedByUserId BIGINT,
    RelatedIssueId BIGINT,
    PRIMARY KEY (RequestId)
);

CREATE TABLE CrossMatchRecord (
    CrossMatchId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    RequestId BIGINT,
    ComponentId BIGINT,
    Result VARCHAR(200),
    Method VARCHAR(200),
    PerformedBy BIGINT,
    PerformedAt TIMESTAMPTZ,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (CrossMatchId)
);

CREATE TABLE IssueRecord (
    IssueRecordId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    ComponentId BIGINT,
    BagId BIGINT,
    PatientName VARCHAR(300),
    HospitalId BIGINT,
    IssueDate TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    IssuedByUserId BIGINT,
    IssueType VARCHAR(50),
    IssueSlipNumber VARCHAR(200),
    RelatedBillingId BIGINT,
    Notes VARCHAR(2000),
    PRIMARY KEY (IssueRecordId)
);

CREATE TABLE ReturnRecord (
    ReturnId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    IssueRecordId BIGINT,
    ComponentId BIGINT,
    ReturnDate TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    Reason VARCHAR(1000),
    CreatedBy BIGINT,
    PRIMARY KEY (ReturnId)
);

CREATE TABLE ReplacementDonor (
    ReplacementDonorId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    PatientRequestId BIGINT,
    DonorId BIGINT,
    DonatedAt TIMESTAMPTZ,
    PRIMARY KEY (ReplacementDonorId)
);

CREATE TABLE RequestStatusLog (
    RequestStatusLogId BIGSERIAL NOT NULL,
    RequestId BIGINT,
    OldStatus VARCHAR(100),
    NewStatus VARCHAR(100),
    ChangedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    ChangedBy BIGINT,
    Notes VARCHAR(2000),
    PRIMARY KEY (RequestStatusLogId)
);

-- ============================================================================
-- 9. Billing & Finance
-- ============================================================================

CREATE TABLE ServiceChargeMaster (
    ServiceChargeId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    ServiceCode VARCHAR(100),
    ServiceName VARCHAR(300) NOT NULL,
    Amount NUMERIC(18,2),
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (ServiceChargeId)
);

CREATE TABLE BillingTransaction (
    BillingTransactionId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    InvoiceNumber VARCHAR(200),
    PatientId BIGINT,
    TotalAmount NUMERIC(18,2),
    TaxAmount NUMERIC(18,2) DEFAULT 0,
    Discount NUMERIC(18,2) DEFAULT 0,
    PaymentStatus VARCHAR(50),
    PaymentMode VARCHAR(50),
    InvoiceDate TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CreatedBy BIGINT,
    PRIMARY KEY (BillingTransactionId)
);

CREATE TABLE InvoiceDetail (
    InvoiceDetailId BIGSERIAL NOT NULL,
    BillingTransactionId BIGINT NOT NULL,
    ComponentId BIGINT,
    ServiceChargeId BIGINT,
    ServiceName VARCHAR(300),
    Quantity INTEGER NOT NULL DEFAULT 1,
    UnitPrice NUMERIC(18,2),
    LineTotal NUMERIC(18,2),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (InvoiceDetailId)
);

CREATE TABLE PaymentRecord (
    PaymentId BIGSERIAL NOT NULL,
    BillingTransactionId BIGINT,
    CenterId BIGINT,
    PaymentDate TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    Amount NUMERIC(18,2),
    PaymentMode VARCHAR(100),
    Reference VARCHAR(200),
    CreatedBy BIGINT,
    PRIMARY KEY (PaymentId)
);

CREATE TABLE ExpenseMaster (
    ExpenseId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    ExpenseDate TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    Category VARCHAR(200),
    Amount NUMERIC(18,2),
    Notes VARCHAR(2000),
    CreatedBy BIGINT,
    PRIMARY KEY (ExpenseId)
);

-- ============================================================================
-- 10. Reporting & Compliance
-- ============================================================================

CREATE TABLE MonthlyReportLog (
    MonthlyReportId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    ReportYear INTEGER,
    ReportMonth INTEGER,
    ReportType VARCHAR(200),
    DataSnapshot TEXT,
    FilePath VARCHAR(4000),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (MonthlyReportId)
);

CREATE TABLE ExportFileLog (
    ExportFileLogId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    FileName VARCHAR(400),
    FileType VARCHAR(100),
    FilePath VARCHAR(4000),
    ExportedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    ExportedBy BIGINT,
    PRIMARY KEY (ExportFileLogId)
);

CREATE TABLE DataUploadHistory (
    UploadHistoryId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    TargetSystem VARCHAR(200),
    PayloadPath VARCHAR(4000),
    ResponseStatus VARCHAR(200),
    UploadedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (UploadHistoryId)
);

CREATE TABLE ReportTemplate (
    ReportTemplateId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    TemplateName VARCHAR(300),
    TemplateFilePath VARCHAR(4000),
    FileType VARCHAR(50),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (ReportTemplateId)
);

CREATE TABLE AnalyticsDashboardData (
    DashboardDataId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    DataKey VARCHAR(200),
    DataValue TEXT,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (DashboardDataId)
);

CREATE TABLE ChangeLog (
    ChangeLogId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    EntityName VARCHAR(200),
    EntityId VARCHAR(100),
    ChangeType VARCHAR(50),
    ChangeData TEXT,
    ChangedBy BIGINT,
    ChangedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (ChangeLogId)
);

-- ============================================================================
-- 11. Communication & Outreach
-- ============================================================================

CREATE TABLE NotificationMaster (
    NotificationId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    NotificationType VARCHAR(50),
    Title VARCHAR(300),
    Body VARCHAR(4000),
    TargetAudience VARCHAR(500),
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (NotificationId)
);

CREATE TABLE SmsTemplateMaster (
    SmsTemplateId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    TemplateCode VARCHAR(200),
    TemplateText VARCHAR(2000),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (SmsTemplateId)
);

CREATE TABLE EmailTemplateMaster (
    EmailTemplateId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    TemplateCode VARCHAR(200),
    Subject VARCHAR(400),
    BodyHtml TEXT,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (EmailTemplateId)
);

CREATE TABLE OutboxLog (
    OutboxId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    Channel VARCHAR(50),
    Recipient VARCHAR(500),
    Message TEXT,
    SentAt TIMESTAMPTZ,
    Status VARCHAR(100),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (OutboxId)
);

CREATE TABLE FeedbackMaster (
    FeedbackId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    Source VARCHAR(100),
    Subject VARCHAR(300),
    Message TEXT,
    SubmittedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    SubmittedBy VARCHAR(300),
    PRIMARY KEY (FeedbackId)
);

CREATE TABLE NewsletterSubscription (
    SubscriptionId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    Email VARCHAR(200),
    SubscribedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    PRIMARY KEY (SubscriptionId)
);

-- ============================================================================
-- 12. Emergency & Donation Requests
-- ============================================================================

CREATE TABLE EmergencyRequest (
    EmergencyRequestId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    HospitalId BIGINT,
    PatientName VARCHAR(300),
    BloodGroup VARCHAR(20),
    ComponentType VARCHAR(100),
    UnitsRequired INTEGER,
    RequestStatus VARCHAR(100),
    RequestedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    RequestedByUserId BIGINT,
    FulfilledAt TIMESTAMPTZ,
    Notes VARCHAR(2000),
    PRIMARY KEY (EmergencyRequestId)
);

CREATE TABLE EmergencyDonorResponse (
    ResponseId BIGSERIAL NOT NULL,
    EmergencyRequestId BIGINT NOT NULL,
    DonorId BIGINT,
    ResponseContact VARCHAR(200),
    RespondedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    IsVerified BOOLEAN NOT NULL DEFAULT FALSE,
    PRIMARY KEY (ResponseId)
);

-- ============================================================================
-- 13. System Configuration & Support
-- ============================================================================

CREATE TABLE SystemConfig (
    ConfigId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    ConfigKey VARCHAR(200) NOT NULL,
    ConfigValue TEXT,
    Description VARCHAR(1000),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (ConfigId)
);

CREATE TABLE BackupLog (
    BackupLogId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    BackupType VARCHAR(100),
    BackupPath VARCHAR(4000),
    BackupStartedAt TIMESTAMPTZ,
    BackupCompletedAt TIMESTAMPTZ,
    Status VARCHAR(100),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (BackupLogId)
);

CREATE TABLE ErrorLog (
    ErrorId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    ErrorMessage TEXT,
    StackTrace TEXT,
    OccurredAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (ErrorId)
);

CREATE TABLE SchedulerJobLog (
    JobLogId BIGSERIAL NOT NULL,
    JobName VARCHAR(200),
    LastRunAt TIMESTAMPTZ,
    Status VARCHAR(100),
    Message VARCHAR(2000),
    PRIMARY KEY (JobLogId)
);

CREATE TABLE SequenceCounters (
    SequenceCounterId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    SequenceName VARCHAR(100) NOT NULL,
    LastValue BIGINT NOT NULL DEFAULT 0,
    Prefix VARCHAR(20),
    Suffix VARCHAR(20),
    IncrementBy INTEGER NOT NULL DEFAULT 1,
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UpdatedBy BIGINT,
    PRIMARY KEY (SequenceCounterId)
);

-- ============================================================================
-- 13b. Center Configuration
-- ============================================================================

CREATE TABLE CenterConfig (
    CenterConfigId BIGSERIAL NOT NULL,
    CenterId BIGINT NOT NULL,
    ConfigKey VARCHAR(200) NOT NULL,
    ConfigValue TEXT,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (CenterConfigId)
);

-- ============================================================================
-- 14. Master & Reference Data
-- ============================================================================

CREATE TABLE StateMaster (
    StateId BIGSERIAL NOT NULL,
    StateName VARCHAR(200) NOT NULL,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (StateId)
);

CREATE TABLE DistrictMaster (
    DistrictId BIGSERIAL NOT NULL,
    StateId BIGINT,
    DistrictName VARCHAR(200) NOT NULL,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (DistrictId)
);

CREATE TABLE CityMaster (
    CityId BIGSERIAL NOT NULL,
    DistrictId BIGINT,
    CityName VARCHAR(200) NOT NULL,
    Pincode VARCHAR(20),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (CityId)
);

CREATE TABLE PincodeMaster (
    PincodeId BIGSERIAL NOT NULL,
    Pincode VARCHAR(20) NOT NULL,
    AreaName VARCHAR(300),
    CityId BIGINT,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (PincodeId)
);

CREATE TABLE BloodGroupMaster (
    BloodGroupId BIGSERIAL NOT NULL,
    BloodGroupCode VARCHAR(10) NOT NULL,
    Description VARCHAR(200),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (BloodGroupId)
);

CREATE TABLE LookupType (
    LookupTypeId BIGSERIAL NOT NULL,
    TypeCode VARCHAR(100) NOT NULL,
    TypeName VARCHAR(200) NOT NULL,
    Description VARCHAR(500),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (LookupTypeId)
);

CREATE TABLE LookupValue (
    LookupValueId BIGSERIAL NOT NULL,
    LookupTypeId BIGINT,
    CenterId BIGINT,
    ValueCode VARCHAR(100),
    ValueText VARCHAR(500),
    SortOrder INTEGER,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (LookupValueId)
);

CREATE TABLE ReasonMaster (
    ReasonId BIGSERIAL NOT NULL,
    Category VARCHAR(100),
    ReasonCode VARCHAR(100),
    ReasonText VARCHAR(500),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (ReasonId)
);

CREATE TABLE HolidayMaster (
    HolidayId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    HolidayDate DATE NOT NULL,
    Description VARCHAR(400),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (HolidayId)
);

-- ============================================================================
-- 15. API Integration
-- ============================================================================

CREATE TABLE ApiIntegrationMaster (
    ApiIntegrationId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    IntegrationName VARCHAR(200),
    BaseUrl VARCHAR(1000),
    ApiKey VARCHAR(1000),
    Username VARCHAR(500),
    PasswordEncrypted VARCHAR(1000),
    LastSyncAt TIMESTAMPTZ,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (ApiIntegrationId)
);

CREATE TABLE ApiResponseLog (
    ApiResponseLogId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    ApiIntegrationId BIGINT,
    RequestPayload TEXT,
    ResponsePayload TEXT,
    StatusCode VARCHAR(50),
    CalledAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (ApiResponseLogId)
);

CREATE TABLE PortalUploadQueue (
    UploadQueueId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    TargetPortal VARCHAR(200),
    PayloadPath VARCHAR(4000),
    Status VARCHAR(100),
    AttemptCount INTEGER NOT NULL DEFAULT 0,
    NextAttemptAt TIMESTAMPTZ,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (UploadQueueId)
);

-- ============================================================================
-- 16. Attachments
-- ============================================================================

CREATE TABLE AttachmentStore (
    AttachmentId BIGSERIAL NOT NULL,
    CenterId BIGINT,
    RelatedTable VARCHAR(128),
    RelatedRecordId VARCHAR(100),
    FileName VARCHAR(260),
    ContentType VARCHAR(100),
    FileSize BIGINT,
    FilePath VARCHAR(4000),
    UploadedBy BIGINT,
    UploadedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (AttachmentId)
);

-- ============================================================================
-- End of Schema — 89 Tables
-- ============================================================================

-- ============================================================================
-- Additional Unique Indexes (required by upsert functions)
-- ============================================================================

CREATE UNIQUE INDEX IX_InventoryStock_Unique
    ON InventoryStock (CenterId, COALESCE(ComponentType,''), COALESCE(BloodGroup,''));

CREATE UNIQUE INDEX IX_SystemConfig_Unique
    ON SystemConfig (CenterId, ConfigKey);

CREATE UNIQUE INDEX IX_CenterConfig_Unique
    ON CenterConfig (CenterId, ConfigKey);

CREATE UNIQUE INDEX IX_UserSettings_Unique
    ON UserSettings (UserId, SettingsKey);

CREATE UNIQUE INDEX IX_LookupType_Unique
    ON LookupType (TypeCode);

CREATE UNIQUE INDEX IX_SequenceCounters_Unique
    ON SequenceCounters (CenterId, SequenceName);
