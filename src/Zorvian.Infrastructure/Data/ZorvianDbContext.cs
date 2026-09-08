using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Zorvian.Core.Entities;
using Zorvian.Core.Enums;
using Zorvian.Core.Interfaces;

namespace Zorvian.Infrastructure.Data;

public sealed class ZorvianDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public ZorvianDbContext(DbContextOptions<ZorvianDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<CompanySettings> CompanySettings => Set<CompanySettings>();
    public DbSet<EmployeeSupervisor> EmployeeSupervisors => Set<EmployeeSupervisor>();
    public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();
    public DbSet<LeaveBalances> LeaveBalances => Set<LeaveBalances>();
    public DbSet<EmployeeHistory> EmployeeHistories => Set<EmployeeHistory>();
    public DbSet<EntityHistory> EntityHistories => Set<EntityHistory>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<VacationRequest> VacationRequests => Set<VacationRequest>();
    public DbSet<ApprovalFlow> ApprovalFlows => Set<ApprovalFlow>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<PermissionRequest> PermissionRequests => Set<PermissionRequest>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<BiometricRegistration> BiometricRegistrations => Set<BiometricRegistration>();
    public DbSet<DeductionType> DeductionTypes => Set<DeductionType>();
    public DbSet<EmployeeSalary> EmployeeSalaries => Set<EmployeeSalary>();
    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();
    public DbSet<PayrollDetail> PayrollDetails => Set<PayrollDetail>();
    public DbSet<PayrollDetailConcept> PayrollDetailConcepts => Set<PayrollDetailConcept>();
    // NOTE: CountryTaxConfig is a GLOBAL catalog table Â no tenant isolation applied by design.
    // All companies share the same country tax configurations. If per-tenant customization is
    // needed in the future, move to a per-tenant HasQueryFilter approach.
    public DbSet<CountryTaxConfig> CountryTaxConfigs => Set<CountryTaxConfig>();
    public DbSet<OvertimeRecord> OvertimeRecords => Set<OvertimeRecord>();
    public DbSet<CommissionRecord> CommissionRecords => Set<CommissionRecord>();
    public DbSet<BonusRecord> BonusRecords => Set<BonusRecord>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<WebhookSubscription> WebhookSubscriptions => Set<WebhookSubscription>();
    public DbSet<WebhookDeliveryLog> WebhookDeliveryLogs => Set<WebhookDeliveryLog>();
    public DbSet<PolicyDocument> PolicyDocuments => Set<PolicyDocument>();
    public DbSet<PolicyChunk> PolicyChunks => Set<PolicyChunk>();
    public DbSet<Objective> Objectives => Set<Objective>();
    public DbSet<KeyResult> KeyResults => Set<KeyResult>();
    public DbSet<PayrollConceptDefinition> PayrollConceptDefinitions => Set<PayrollConceptDefinition>();
    public DbSet<EmployeeLoan> EmployeeLoans => Set<EmployeeLoan>();
    public DbSet<LoanInstallment> LoanInstallments => Set<LoanInstallment>();
    public DbSet<SalaryAdvance> SalaryAdvances => Set<SalaryAdvance>();
    public DbSet<WageGarnishment> WageGarnishments => Set<WageGarnishment>();
    public DbSet<BenefitProvision> BenefitProvisions => Set<BenefitProvision>();
    public DbSet<EmployeeBankAccount> EmployeeBankAccounts => Set<EmployeeBankAccount>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<Collaborator> Collaborators => Set<Collaborator>();
    public DbSet<UserTenant> UserTenants => Set<UserTenant>();

    // New Module: Multisucursal
    public DbSet<Branch> Branches => Set<Branch>();

    // New Module: Comercial
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuoteDetail> QuoteDetails => Set<QuoteDetail>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleDetail> SaleDetails => Set<SaleDetail>();
    public DbSet<SalePayment> SalePayments => Set<SalePayment>();
    public DbSet<CreditNote> CreditNotes => Set<CreditNote>();
    public DbSet<CreditNoteDetail> CreditNoteDetails => Set<CreditNoteDetail>();
    public DbSet<ApprovalFlowConfig> ApprovalFlowConfigs => Set<ApprovalFlowConfig>();
    public DbSet<ApprovalFlowStep> ApprovalFlowSteps => Set<ApprovalFlowStep>();
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();
    public DbSet<ApprovalRequestAction> ApprovalRequestActions => Set<ApprovalRequestAction>();
    
    // New Module: CRM
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<PipelineStage> PipelineStages => Set<PipelineStage>();
    public DbSet<CommercialActivity> CommercialActivities => Set<CommercialActivity>();

    // New Module: Inventario
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<IntercompanyTransaction> IntercompanyTransactions => Set<IntercompanyTransaction>();

    // New Module: CrÃÂÃÂ©ditos
    public DbSet<Credit> Credits => Set<Credit>();
    public DbSet<CreditInstallment> CreditInstallments => Set<CreditInstallment>();
    public DbSet<CreditPayment> CreditPayments => Set<CreditPayment>();
    public DbSet<LateFee> LateFees => Set<LateFee>();
    public DbSet<CollectionAction> CollectionActions => Set<CollectionAction>();
    public DbSet<CreditRefinancing> CreditRefinancings => Set<CreditRefinancing>();

    // New Module: Caja
    public DbSet<CashRegister> CashRegisters => Set<CashRegister>();
    public DbSet<CashMovement> CashMovements => Set<CashMovement>();
    public DbSet<CashRegisterArqueo> CashRegisterArqueos => Set<CashRegisterArqueo>();
    public DbSet<CashArqueoDenomination> CashArqueoDenominations => Set<CashArqueoDenomination>();

    // New Module: Compras
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseDetail> PurchaseDetails => Set<PurchaseDetail>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderDetail> PurchaseOrderDetails => Set<PurchaseOrderDetail>();
    public DbSet<SupplierPayment> SupplierPayments => Set<SupplierPayment>();
    public DbSet<SupplierCreditNote> SupplierCreditNotes => Set<SupplierCreditNote>();
    public DbSet<Withholding> Withholdings => Set<Withholding>();

    // New Module: GarantÃÂÃÂ­as
    public DbSet<Warranty> Warranties => Set<Warranty>();
    public DbSet<WarrantyClaim> WarrantyClaims => Set<WarrantyClaim>();
    public DbSet<ServiceWorkshop> ServiceWorkshops => Set<ServiceWorkshop>();
    public DbSet<WorkshopTechnician> WorkshopTechnicians => Set<WorkshopTechnician>();
    public DbSet<WorkshopBrand> WorkshopBrands => Set<WorkshopBrand>();
    public DbSet<WarrantyProvider> WarrantyProviders => Set<WarrantyProvider>();
    public DbSet<ProviderContact> ProviderContacts => Set<ProviderContact>();
    public DbSet<ProviderBrand> ProviderBrands => Set<ProviderBrand>();
    public DbSet<WarrantyCost> WarrantyCosts => Set<WarrantyCost>();
    public DbSet<WarrantyPartRequest> WarrantyPartRequests => Set<WarrantyPartRequest>();
    public DbSet<WarrantyPartReceipt> WarrantyPartReceipts => Set<WarrantyPartReceipt>();
    public DbSet<WarrantyPartUsage> WarrantyPartUsages => Set<WarrantyPartUsage>();
    public DbSet<WarrantyCommunication> WarrantyCommunications => Set<WarrantyCommunication>();
    public DbSet<WarrantyEvent> WarrantyEvents => Set<WarrantyEvent>();
    public DbSet<WarrantyAttachment> WarrantyAttachments => Set<WarrantyAttachment>();
    public DbSet<WarrantyStateHistory> WarrantyStateHistories => Set<WarrantyStateHistory>();

    // New Module: Activos Fijos
    public DbSet<FixedAsset> FixedAssets => Set<FixedAsset>();
    public DbSet<FixedAssetCategory> FixedAssetCategories => Set<FixedAssetCategory>();
    public DbSet<DepreciationEntry> DepreciationEntries => Set<DepreciationEntry>();
    public DbSet<AssetRevaluation> AssetRevaluations => Set<AssetRevaluation>();
    public DbSet<AssetMaintenance> AssetMaintenances => Set<AssetMaintenance>();
    public DbSet<AssetDisposal> AssetDisposals => Set<AssetDisposal>();
    public DbSet<Location> Locations => Set<Location>();

    // Multimoneda
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();

    // Constructor de Reportes
    public DbSet<CustomReport> CustomReports => Set<CustomReport>();

    // Offline-First Sync
    public DbSet<SyncJournal> SyncJournals => Set<SyncJournal>();

    // New Module: Contabilidad
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Reconciliation> Reconciliations => Set<Reconciliation>();
    public DbSet<ReconciliationDetail> ReconciliationDetails => Set<ReconciliationDetail>();
    public DbSet<BudgetDetail> BudgetDetails => Set<BudgetDetail>();
    public DbSet<BudgetTracking> BudgetTrackings => Set<BudgetTracking>();
    public DbSet<AccountingEntry> AccountingEntries => Set<AccountingEntry>();
    public DbSet<AccountingEntryDetail> AccountingEntryDetails => Set<AccountingEntryDetail>();
    public DbSet<FiscalYear> FiscalYears => Set<FiscalYear>();
    public DbSet<AccountingPeriod> AccountingPeriods => Set<AccountingPeriod>();
    public DbSet<AccountLink> AccountLinks => Set<AccountLink>();
    public DbSet<AccountingRule> AccountingRules => Set<AccountingRule>();
    public DbSet<TaxCategory> TaxCategories => Set<TaxCategory>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<RegionalTaxConfiguration> RegionalTaxConfigurations => Set<RegionalTaxConfiguration>();
    public DbSet<PayrollConcept> PayrollConcepts => Set<PayrollConcept>();
    public DbSet<AccountingRuleTemplate> AccountingRuleTemplates => Set<AccountingRuleTemplate>();
    public DbSet<EmployeePayrollExemption> EmployeePayrollExemptions => Set<EmployeePayrollExemption>();
    
    // New Module: TesorerÃÂÃÂ­a
    public DbSet<Bank> Banks => Set<Bank>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<Checkbook> Checkbooks => Set<Checkbook>();
    public DbSet<Check> Checks => Set<Check>();
    public DbSet<CheckAuditTrail> CheckAuditTrails => Set<CheckAuditTrail>();
    public DbSet<CheckPrintTemplate> CheckPrintTemplates => Set<CheckPrintTemplate>();
    
    // New Module: NÃÂÃÂ³mina Avanzada
    public DbSet<SickLeaveRecord> SickLeaveRecords => Set<SickLeaveRecord>();
    public DbSet<TerminationRecord> TerminationRecords => Set<TerminationRecord>();

    // New Module: Compensaciones y Comisiones
    public DbSet<CommissionScheme> CommissionSchemes => Set<CommissionScheme>();
    public DbSet<CommissionRule> CommissionRules => Set<CommissionRule>();
    public DbSet<CommissionAssignment> CommissionAssignments => Set<CommissionAssignment>();

    // New Module: Metas e Incentivos
    public DbSet<GoalDefinition> GoalDefinitions => Set<GoalDefinition>();
    public DbSet<GoalAssignment> GoalAssignments => Set<GoalAssignment>();
    public DbSet<GoalProgress> GoalProgressEntries => Set<GoalProgress>();
    public DbSet<Incentive> Incentives => Set<Incentive>();
    public DbSet<IncentivePayment> IncentivePayments => Set<IncentivePayment>();

    // New Module: KPI y DesempeÃÂÃÂ±o
    public DbSet<KpiDefinition> KpiDefinitions => Set<KpiDefinition>();
    public DbSet<KpiRecord> KpiRecords => Set<KpiRecord>();
    public DbSet<Ranking> Rankings => Set<Ranking>();

    // New Module: FacturaciÃÂÃÂ³n ElectrÃÂÃÂ³nica
    public DbSet<ElectronicInvoice> ElectronicInvoices => Set<ElectronicInvoice>();
    public DbSet<ElectronicInvoiceXml> ElectronicInvoiceXmls => Set<ElectronicInvoiceXml>();

    // New Module: Partners (Canal)
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<CompanyPlanPricing> CompanyPlanPricings => Set<CompanyPlanPricing>();

    public DbSet<Partner> Partners => Set<Partner>();

    // New Module: Prestadores de Servicios
    public DbSet<ServiceProvider> ServiceProviders => Set<ServiceProvider>();
    public DbSet<ServiceContract> ServiceContracts => Set<ServiceContract>();
    public DbSet<PaymentMilestone> PaymentMilestones => Set<PaymentMilestone>();
    public DbSet<ProviderInvoice> ProviderInvoices => Set<ProviderInvoice>();

    // New Module: Motor Documental
    public DbSet<DocumentTemplate> DocumentTemplates => Set<DocumentTemplate>();
    public DbSet<GeneratedDocument> GeneratedDocuments => Set<GeneratedDocument>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<DocumentSignature> DocumentSignatures => Set<DocumentSignature>();

    // New Module: Flota y LogÃÂÃÂ­stica
    public DbSet<Zorvian.Core.Entities.Fleet.VehicleBrand> VehicleBrands => Set<Zorvian.Core.Entities.Fleet.VehicleBrand>();
    public DbSet<Zorvian.Core.Entities.Fleet.VehicleType> VehicleTypes => Set<Zorvian.Core.Entities.Fleet.VehicleType>();
    public DbSet<Zorvian.Core.Entities.Fleet.FuelType> FuelTypes => Set<Zorvian.Core.Entities.Fleet.FuelType>();
    public DbSet<Zorvian.Core.Entities.Fleet.DriverLicenseCategory> DriverLicenseCategories => Set<Zorvian.Core.Entities.Fleet.DriverLicenseCategory>();
    public DbSet<Zorvian.Core.Entities.Fleet.Vehicle> Vehicles => Set<Zorvian.Core.Entities.Fleet.Vehicle>();
    public DbSet<Zorvian.Core.Entities.Fleet.Driver> Drivers => Set<Zorvian.Core.Entities.Fleet.Driver>();
    public DbSet<Zorvian.Core.Entities.Fleet.Route> Routes => Set<Zorvian.Core.Entities.Fleet.Route>();
    public DbSet<Zorvian.Core.Entities.Fleet.RoutePoint> RoutePoints => Set<Zorvian.Core.Entities.Fleet.RoutePoint>();
    public DbSet<Zorvian.Core.Entities.Fleet.Delivery> Deliveries => Set<Zorvian.Core.Entities.Fleet.Delivery>();
    public DbSet<Zorvian.Core.Entities.Fleet.DeliveryItem> DeliveryItems => Set<Zorvian.Core.Entities.Fleet.DeliveryItem>();
    public DbSet<Zorvian.Core.Entities.Fleet.Trip> Trips => Set<Zorvian.Core.Entities.Fleet.Trip>();
    public DbSet<Zorvian.Core.Entities.Fleet.FuelRefill> FuelRefills => Set<Zorvian.Core.Entities.Fleet.FuelRefill>();
    public DbSet<Zorvian.Core.Entities.Fleet.MaintenanceSchedule> MaintenanceSchedules => Set<Zorvian.Core.Entities.Fleet.MaintenanceSchedule>();
    public DbSet<Zorvian.Core.Entities.Fleet.MaintenanceTemplate> MaintenanceTemplates => Set<Zorvian.Core.Entities.Fleet.MaintenanceTemplate>();
    public DbSet<Zorvian.Core.Entities.Fleet.WorkOrder> WorkOrders => Set<Zorvian.Core.Entities.Fleet.WorkOrder>();
    public DbSet<Zorvian.Core.Entities.Fleet.WorkOrderPart> WorkOrderParts => Set<Zorvian.Core.Entities.Fleet.WorkOrderPart>();
    public DbSet<Zorvian.Core.Entities.Fleet.FailureType> FailureTypes => Set<Zorvian.Core.Entities.Fleet.FailureType>();
    public DbSet<Zorvian.Core.Entities.Fleet.Workshop> Workshops => Set<Zorvian.Core.Entities.Fleet.Workshop>();
    public DbSet<Zorvian.Core.Entities.Fleet.FleetDocument> FleetDocuments => Set<Zorvian.Core.Entities.Fleet.FleetDocument>();
    public DbSet<Zorvian.Core.Entities.Fleet.DocumentType> DocumentTypes => Set<Zorvian.Core.Entities.Fleet.DocumentType>();
    public DbSet<Zorvian.Core.Entities.Fleet.FleetExpense> FleetExpenses => Set<Zorvian.Core.Entities.Fleet.FleetExpense>();
    public DbSet<Zorvian.Core.Entities.Fleet.ExpenseCategory> ExpenseCategories => Set<Zorvian.Core.Entities.Fleet.ExpenseCategory>();
    public DbSet<Zorvian.Core.Entities.Fleet.ExpenseSubcategory> ExpenseSubcategories => Set<Zorvian.Core.Entities.Fleet.ExpenseSubcategory>();
    public DbSet<Zorvian.Core.Entities.Fleet.GpsPosition> GpsPositions => Set<Zorvian.Core.Entities.Fleet.GpsPosition>();
    public DbSet<Zorvian.Core.Entities.Fleet.Geofence> Geofences => Set<Zorvian.Core.Entities.Fleet.Geofence>();
    public DbSet<Zorvian.Core.Entities.Fleet.DriverInfraction> DriverInfractions => Set<Zorvian.Core.Entities.Fleet.DriverInfraction>();
    public DbSet<Zorvian.Core.Entities.Fleet.DriverTraining> DriverTrainings => Set<Zorvian.Core.Entities.Fleet.DriverTraining>();
    public DbSet<Zorvian.Core.Entities.Fleet.FleetAlert> FleetAlerts => Set<Zorvian.Core.Entities.Fleet.FleetAlert>();
    public DbSet<Zorvian.Core.Entities.Fleet.FleetAlertRule> FleetAlertRules => Set<Zorvian.Core.Entities.Fleet.FleetAlertRule>();
    public DbSet<Zorvian.Core.Entities.Fleet.VehicleGeofenceState> VehicleGeofenceStates => Set<Zorvian.Core.Entities.Fleet.VehicleGeofenceState>();

    // PalmTrack Integration
    public DbSet<Zorvian.Core.Entities.Fleet.FleetExternalReference> FleetExternalReferences => Set<Zorvian.Core.Entities.Fleet.FleetExternalReference>();
    public DbSet<Zorvian.Core.Entities.Fleet.FleetDriverAlias> FleetDriverAliases => Set<Zorvian.Core.Entities.Fleet.FleetDriverAlias>();
    public DbSet<Zorvian.Core.Entities.PalmTrackWebhookLog> PalmTrackWebhookLogs => Set<Zorvian.Core.Entities.PalmTrackWebhookLog>();
    public DbSet<Zorvian.Core.Entities.PalmTrackWebhookDlq> PalmTrackWebhookDlqs => Set<Zorvian.Core.Entities.PalmTrackWebhookDlq>();
    public DbSet<Zorvian.Core.Entities.PalmTrackWebhookSecret> PalmTrackWebhookSecrets => Set<Zorvian.Core.Entities.PalmTrackWebhookSecret>();
    public DbSet<Zorvian.Core.Entities.ExternalIdentityMapping> ExternalIdentityMappings => Set<Zorvian.Core.Entities.ExternalIdentityMapping>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<TenantId>().HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<PayrollConceptDefinition>(e =>
        {
            e.HasKey(pcd => pcd.Id);
            e.Property(pcd => pcd.Code).HasMaxLength(50).IsRequired();
            e.Property(pcd => pcd.Name).HasMaxLength(255).IsRequired();
            e.Property(pcd => pcd.ConceptType).HasMaxLength(20).IsRequired();
            e.Property(pcd => pcd.CalculationMethod).HasMaxLength(50);
            e.HasIndex(pcd => new { pcd.Code, pcd.TenantId }).IsUnique();
            e.HasQueryFilter(pcd => (pcd.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !pcd.IsDeleted);
        });

        builder.Entity<EmployeeLoan>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.LoanNumber).HasMaxLength(50).IsRequired();
            e.Property(l => l.Status).HasMaxLength(20).IsRequired();
            e.HasOne(l => l.Employee).WithMany().HasForeignKey(l => l.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(l => (l.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !l.IsDeleted);
        });

        builder.Entity<LoanInstallment>(e =>
        {
            e.HasKey(li => li.Id);
            e.Property(li => li.Status).HasMaxLength(20).IsRequired();
            e.HasOne(li => li.EmployeeLoan).WithMany(l => l.Installments).HasForeignKey(li => li.EmployeeLoanId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(li => (li.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !li.IsDeleted);
        });

        builder.Entity<SalaryAdvance>(e =>
        {
            e.HasKey(sa => sa.Id);
            e.Property(sa => sa.Status).HasMaxLength(20).IsRequired();
            e.HasOne(sa => sa.Employee).WithMany().HasForeignKey(sa => sa.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(sa => sa.ApprovedBy).WithMany().HasForeignKey(sa => sa.ApprovedByEmployeeId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(sa => (sa.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !sa.IsDeleted);
        });

        builder.Entity<WageGarnishment>(e =>
        {
            e.HasKey(wg => wg.Id);
            e.Property(wg => wg.CourtOrder).HasMaxLength(100).IsRequired();
            e.Property(wg => wg.Status).HasMaxLength(20).IsRequired();
            e.HasOne(wg => wg.Employee).WithMany().HasForeignKey(wg => wg.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(wg => (wg.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !wg.IsDeleted);
        });

        builder.Entity<EmployeeBankAccount>(e =>
        {
            e.HasKey(ba => ba.Id);
            e.Property(ba => ba.BankName).HasMaxLength(255).IsRequired();
            e.Property(ba => ba.AccountNumber).HasMaxLength(50).IsRequired();
            e.Property(ba => ba.AccountType).HasMaxLength(30).IsRequired();
            e.Property(ba => ba.AccountCurrency).HasMaxLength(3).IsRequired();
            e.HasOne(ba => ba.Employee).WithMany(e => e.BankAccounts).HasForeignKey(ba => ba.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(ba => (ba.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !ba.IsDeleted);
        });

        builder.Entity<SickLeaveRecord>(e =>
        {
            e.HasKey(sl => sl.Id);
            e.Property(sl => sl.Status).HasMaxLength(20).IsRequired();
            e.HasOne(sl => sl.Employee).WithMany().HasForeignKey(sl => sl.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(sl => (sl.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !sl.IsDeleted);
        });

        builder.Entity<TerminationRecord>(e =>
        {
            e.HasKey(tr => tr.Id);
            e.Property(tr => tr.Status).HasMaxLength(20).IsRequired();
            e.HasOne(tr => tr.Employee).WithMany().HasForeignKey(tr => tr.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(tr => (tr.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !tr.IsDeleted);
        });

        builder.Entity<Invitation>(e =>
        {
            e.HasKey(i => i.Id);
            e.HasIndex(i => i.Code).IsUnique();
            e.Property(i => i.Email).HasMaxLength(255).IsRequired();
            e.HasQueryFilter(i => (i.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !i.IsDeleted);
        });

        builder.Entity<Collaborator>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.CollaboratorCode).HasMaxLength(50).IsRequired();
            e.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
            e.Property(c => c.LastName).HasMaxLength(100).IsRequired();
            e.Property(c => c.Email).HasMaxLength(255).IsRequired();
            e.Property(c => c.CollaboratorType).HasMaxLength(30).IsRequired();
            e.Property(c => c.Status).HasMaxLength(20).IsRequired();
            e.HasIndex(c => c.CollaboratorCode).IsUnique();
            e.HasQueryFilter(c => (c.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !c.IsDeleted);
        });

        builder.Entity<Company>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.TenantId).IsUnique();
            e.Property(c => c.Name).HasMaxLength(255).IsRequired();
            e.Property(c => c.LegalName).HasMaxLength(255).IsRequired();
            e.Property(c => c.TaxId).HasMaxLength(50);
            e.Property(c => c.Country).HasMaxLength(100).IsRequired();
            e.Property(c => c.Currency).HasMaxLength(3);
            e.Property(c => c.Timezone).HasMaxLength(50).IsRequired();
            e.HasQueryFilter(c => (c.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !c.IsDeleted);
        });

        builder.Entity<ExchangeRate>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FromCurrency).HasMaxLength(3).IsRequired();
            e.Property(x => x.ToCurrency).HasMaxLength(3).IsRequired();
            e.Property(x => x.Rate).HasColumnType("decimal(18,6)");
            e.HasIndex(x => new { x.FromCurrency, x.ToCurrency, x.EffectiveDate });
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        // Subscription Plans (global catalog — no tenant filter)
        builder.Entity<SubscriptionPlan>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.PlanId).IsUnique();
            e.Property(p => p.PlanId).HasMaxLength(50).IsRequired();
            e.Property(p => p.Name).HasMaxLength(100).IsRequired();
            e.Property(p => p.Period).HasMaxLength(30).IsRequired();
            e.Property(p => p.ShortDescription).HasMaxLength(500);
        });

        builder.Entity<CompanyPlanPricing>(e =>
        {
            e.HasKey(cp => cp.Id);
            e.Property(cp => cp.PlanId).HasMaxLength(50).IsRequired();
            e.Property(cp => cp.CustomPeriod).HasMaxLength(30);
            e.Property(cp => cp.Notes).HasMaxLength(500);
            e.HasOne(cp => cp.Company)
                .WithMany()
                .HasForeignKey(cp => cp.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(cp => cp.Plan)
                .WithMany()
                .HasForeignKey(cp => cp.PlanId)
                .HasPrincipalKey(sp => sp.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(cp => new { cp.CompanyId, cp.PlanId, cp.IsActive });
        });

        builder.Entity<CustomReport>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(255).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.Module).HasMaxLength(50).IsRequired();
            e.HasIndex(x => new { x.Module, x.CompanyId });
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.FirebaseUid).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.FirebaseUid).HasMaxLength(128).IsRequired();
            e.Property(u => u.Email).HasMaxLength(255).IsRequired();
            e.Property(u => u.DisplayName).HasMaxLength(255).IsRequired();
            e.Property(u => u.TenantId).HasConversion<string>().HasMaxLength(50).IsRequired();
            e.HasOne(u => u.Employee)
                .WithMany()
                .HasForeignKey(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(u => (u.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !u.IsDeleted);
        });

        builder.Entity<Role>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.DisplayName).HasMaxLength(100).IsRequired();
            e.Property(r => r.Name).HasConversion<string>().HasMaxLength(50).IsRequired();
            e.HasQueryFilter(r => (r.TenantId == _tenantContext.TenantId.ToString() || r.IsSystem || _tenantContext.IsSuperAdmin) && !r.IsDeleted);
        });

        builder.Entity<UserRole>(e =>
        {
            e.HasKey(ur => new { ur.UserId, ur.RoleId });
            e.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);
            e.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);
        });

        builder.Entity<RolePermission>(e =>
        {
            e.HasKey(rp => new { rp.RoleId, rp.PermissionCode });
            e.Property(rp => rp.PermissionCode).HasMaxLength(100).IsRequired();
            e.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);
        });

        builder.Entity<CompanySettings>(e =>
        {
            e.HasKey(cs => cs.Id);
            e.Property(cs => cs.LateFeeDailyRate).HasColumnType("decimal(18,6)");
            e.Property(cs => cs.LateFeePercentage).HasColumnType("decimal(18,6)");
            e.Property(cs => cs.TaxRate).HasColumnType("decimal(18,6)");
            e.HasOne(cs => cs.Company)
                .WithOne(c => c.Settings)
                .HasForeignKey<CompanySettings>(cs => cs.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(cs => (cs.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !cs.IsDeleted);
        });

        builder.Entity<Department>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Name).HasMaxLength(255).IsRequired();
            e.Property(d => d.Code).HasMaxLength(50);
            e.Property(d => d.Description).HasMaxLength(500);
            e.HasOne(d => d.Manager)
                .WithMany()
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(d => d.ParentDepartment)
                .WithMany(d => d.ChildDepartments)
                .HasForeignKey(d => d.ParentDepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(d => (d.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !d.IsDeleted);
        });

        builder.Entity<Employee>(e =>
        {
            e.HasKey(em => em.Id);
            e.Property(em => em.EmployeeCode).HasMaxLength(50);
            e.Property(em => em.CollaboratorCode).HasMaxLength(50);
            e.Property(em => em.CollaboratorType).HasMaxLength(30).IsRequired().HasDefaultValue("employee");
            e.Property(em => em.FirstName).HasMaxLength(100).IsRequired();
            e.Property(em => em.LastName).HasMaxLength(100).IsRequired();
            e.Property(em => em.Email).HasMaxLength(255).IsRequired();
            e.Property(em => em.Phone).HasMaxLength(20);
            e.Property(em => em.Gender).HasMaxLength(20);
            e.Property(em => em.IdentificationType).HasMaxLength(50);
            e.Property(em => em.IdentificationNumber).HasMaxLength(50);
            e.Property(em => em.Position).HasMaxLength(255);
            e.Property(em => em.TerminationReason).HasMaxLength(500);
            e.Property(em => em.SalaryType).HasMaxLength(20);
            e.Property(em => em.Status).HasMaxLength(20).IsRequired();
            e.Property(em => em.PhotoUrl).HasMaxLength(500);
            e.Property(em => em.Nationality).HasMaxLength(100);
            e.Property(em => em.MaritalStatus).HasMaxLength(30);
            e.Property(em => em.Address).HasMaxLength(500);
            e.Property(em => em.City).HasMaxLength(100);
            e.Property(em => em.EmergencyContact).HasMaxLength(200);
            e.Property(em => em.EmergencyPhone).HasMaxLength(20);
            e.HasOne(em => em.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(em => em.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(em => em.ServiceProviderDetails)
                .WithOne(sp => sp.Employee)
                .HasForeignKey<ServiceProvider>(sp => sp.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(em => new { em.CollaboratorType, em.Status });
            e.HasIndex(em => new { em.Status, em.DepartmentId });
            e.HasQueryFilter(em => (em.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !em.IsDeleted);
        });

        builder.Entity<EmployeeSupervisor>(e =>
        {
            e.HasKey(es => es.Id);
            e.HasOne(es => es.Employee)
                .WithMany(emp => emp.SupervisedBy)
                .HasForeignKey(es => es.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(es => es.Supervisor)
                .WithMany(emp => emp.Supervisors)
                .HasForeignKey(es => es.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(es => new { es.EmployeeId, es.SupervisorId }).IsUnique();
            e.HasQueryFilter(es => (es.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !es.IsDeleted);
        });

        builder.Entity<EmployeeDocument>(e =>
        {
            e.HasKey(ed => ed.Id);
            e.Property(ed => ed.DocumentType).HasMaxLength(100).IsRequired();
            e.Property(ed => ed.FileName).HasMaxLength(255).IsRequired();
            e.Property(ed => ed.StoragePath).HasMaxLength(500).IsRequired();
            e.Property(ed => ed.Description).HasMaxLength(500);
            e.Property(ed => ed.ContentType).HasMaxLength(100).IsRequired();
            e.HasOne(ed => ed.Employee)
                .WithMany(emp => emp.Documents)
                .HasForeignKey(ed => ed.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(ed => (ed.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !ed.IsDeleted);
        });

        builder.Entity<LeaveBalances>(e =>
        {
            e.HasKey(lb => lb.Id);
            e.HasOne(lb => lb.Employee)
                .WithMany(emp => emp.LeaveBalances)
                .HasForeignKey(lb => lb.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(lb => new { lb.EmployeeId, lb.Year }).IsUnique();
            e.HasQueryFilter(lb => (lb.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !lb.IsDeleted);
        });

        builder.Entity<EmployeeHistory>(e =>
        {
            e.HasKey(eh => eh.Id);
            e.Property(eh => eh.FieldName).HasMaxLength(100).IsRequired();
            e.Property(eh => eh.ChangeType).HasMaxLength(50).IsRequired();
            e.HasOne(eh => eh.Employee)
                .WithMany(emp => emp.History)
                .HasForeignKey(eh => eh.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(eh => new { eh.EmployeeId, eh.CreatedAt });
            e.HasQueryFilter(eh => (eh.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !eh.IsDeleted);
        });

        builder.Entity<EntityHistory>(e =>
        {
            e.HasKey(eh => eh.Id);
            e.Property(eh => eh.EntityType).HasMaxLength(100).IsRequired();
            e.Property(eh => eh.FieldName).HasMaxLength(100).IsRequired();
            e.Property(eh => eh.ChangeType).HasMaxLength(50).IsRequired();
            e.HasIndex(eh => new { eh.EntityType, eh.EntityId, eh.CreatedAt });
            e.HasQueryFilter(eh => (eh.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !eh.IsDeleted);
        });

        builder.Entity<VacationRequest>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.Status).HasMaxLength(30).IsRequired();
            e.Property(v => v.Comments).HasMaxLength(500);
            e.Property(v => v.RejectionReason).HasMaxLength(500);
            e.HasOne(v => v.Employee)
                .WithMany(emp => emp.VacationRequests)
                .HasForeignKey(v => v.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(v => new { v.EmployeeId, v.Status });
            e.HasIndex(v => new { v.StartDate, v.EndDate });
            e.HasQueryFilter(v => (v.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !v.IsDeleted);
        });

        builder.Entity<ApprovalFlow>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.RequestType).HasMaxLength(50).IsRequired();
            e.Property(a => a.Status).HasMaxLength(30).IsRequired();
            e.Property(a => a.Comments).HasMaxLength(500);
            e.HasOne(a => a.Request)
                .WithMany(v => v.ApprovalSteps)
                .HasForeignKey(a => a.RequestId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Approver)
                .WithMany()
                .HasForeignKey(a => a.ApproverId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(a => new { a.ApproverId, a.Status });
            e.HasIndex(a => new { a.RequestId, a.RequestType });
            e.HasQueryFilter(a => (a.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !a.IsDeleted);
        });

        builder.Entity<LeaveType>(e =>
        {
            e.HasKey(lt => lt.Id);
            e.Property(lt => lt.Code).HasMaxLength(50).IsRequired();
            e.Property(lt => lt.Name).HasMaxLength(100).IsRequired();
            e.Property(lt => lt.Description).HasMaxLength(500);
            e.Property(lt => lt.Country).HasMaxLength(100);
            e.HasOne(lt => lt.Company)
                .WithMany()
                .HasForeignKey(lt => lt.CompanyId)
                .OnDelete(DeleteBehavior.SetNull);
            // Index scoped to tenant so different companies can have the same leave type code
            e.HasIndex(lt => new { lt.Code, lt.TenantId }).IsUnique();
            e.HasQueryFilter(lt => (lt.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !lt.IsDeleted);
        });

        builder.Entity<PermissionRequest>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Status).HasMaxLength(30).IsRequired();
            e.Property(p => p.Reason).HasMaxLength(1000);
            e.Property(p => p.SupportingDocumentUrl).HasMaxLength(500);
            e.Property(p => p.SupportingDocumentFileName).HasMaxLength(255);
            e.Property(p => p.RejectionReason).HasMaxLength(500);
            e.HasOne(p => p.Employee)
                .WithMany(emp => emp.PermissionRequests)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.LeaveType)
                .WithMany(lt => lt.PermissionRequests)
                .HasForeignKey(p => p.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Approver)
                .WithMany()
                .HasForeignKey(p => p.ApprovedBy)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(p => new { p.EmployeeId, p.Status });
            e.HasIndex(p => p.LeaveTypeId);
            e.HasIndex(p => new { p.StartDate, p.EndDate });
            e.HasQueryFilter(p => (p.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !p.IsDeleted);
        });

        builder.Entity<AttendanceRecord>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Status).HasMaxLength(30).IsRequired();
            e.Property(a => a.Notes).HasMaxLength(500);
            e.HasOne(a => a.Employee)
                .WithMany(emp => emp.AttendanceRecords)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(a => new { a.EmployeeId, a.Date }).IsUnique();
            e.HasQueryFilter(a => (a.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !a.IsDeleted);
        });

        builder.Entity<DeviceToken>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Token).HasMaxLength(500).IsRequired();
            e.Property(d => d.Platform).HasMaxLength(20).IsRequired();
            e.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(d => d.Token).IsUnique();
            e.HasQueryFilter(d => (d.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !d.IsDeleted);
        });

        builder.Entity<BiometricRegistration>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.DeviceId).HasMaxLength(255).IsRequired();
            e.Property(b => b.DeviceName).HasMaxLength(255).IsRequired();
            e.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(b => new { b.UserId, b.DeviceId }).IsUnique();
            e.HasQueryFilter(b => (b.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !b.IsDeleted);
        });

        builder.Entity<RefreshToken>(e =>
        {
            e.HasKey(rt => rt.Id);
            e.HasIndex(rt => rt.Token).IsUnique();
            e.HasIndex(rt => new { rt.UserId, rt.ExpiresAt });
            e.Property(rt => rt.Token).HasMaxLength(500).IsRequired();
            e.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(rt => (rt.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !rt.IsDeleted);
        });

        builder.Entity<AuditLog>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.EntityName).HasMaxLength(100).IsRequired();
            e.Property(a => a.EntityId).HasMaxLength(100).IsRequired();
            e.Property(a => a.Action).HasMaxLength(50).IsRequired();
            e.Property(a => a.IpAddress).HasMaxLength(50);
            e.Property(a => a.UserAgent).HasMaxLength(500);
            e.Property(a => a.RequestPath).HasMaxLength(500);
            e.HasIndex(a => new { a.EntityName, a.Action });
            e.HasIndex(a => a.CreatedAt);
            e.HasQueryFilter(a => (a.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !a.IsDeleted);
        });

        builder.Entity<DeductionType>(e =>
        {
            e.HasKey(d => d.Id);
            e.HasIndex(d => d.Code).IsUnique();
            e.Property(d => d.Code).HasMaxLength(50).IsRequired();
            e.Property(d => d.Name).HasMaxLength(100).IsRequired();
            e.Property(d => d.CalculationMethod).HasMaxLength(20).IsRequired();
            e.Property(d => d.Rate).HasColumnType("decimal(5,2)");
            e.Property(d => d.FixedAmount).HasColumnType("decimal(18,2)");
            e.HasQueryFilter(d => (d.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !d.IsDeleted);
        });

        builder.Entity<EmployeeSalary>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.BaseSalary).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(s => s.SalaryType).HasMaxLength(20).IsRequired();
            e.Property(s => s.Notes).HasMaxLength(500);
            e.HasOne(s => s.Employee)
                .WithMany()
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.DeductionType)
                .WithMany()
                .HasForeignKey(s => s.DeductionTypeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(s => new { s.EmployeeId, s.IsActive });
            e.HasQueryFilter(s => (s.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !s.IsDeleted);
        });

        builder.Entity<PayrollPeriod>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).HasMaxLength(100).IsRequired();
            e.Property(p => p.Status).HasMaxLength(20).IsRequired();
            e.HasIndex(p => new { p.Year, p.Month, p.PeriodNumber }).IsUnique();
            e.HasQueryFilter(p => (p.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !p.IsDeleted);
        });

        builder.Entity<PayrollRun>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Status).HasMaxLength(20).IsRequired();
            e.Property(r => r.TotalSalaries).HasColumnType("decimal(18,2)");
            e.Property(r => r.TotalDeductions).HasColumnType("decimal(18,2)");
            e.Property(r => r.TotalNetPay).HasColumnType("decimal(18,2)");
            e.Property(r => r.Notes).HasMaxLength(500);
            e.HasOne(r => r.PayrollPeriod)
                .WithMany()
                .HasForeignKey(r => r.PayrollPeriodId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(r => (r.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !r.IsDeleted);
        });

        builder.Entity<CommissionRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.BaseAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.SourceType).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<ApiKey>(e =>
        {
            e.HasKey(k => k.Id);
            e.Property(k => k.Name).HasMaxLength(100).IsRequired();
            e.Property(k => k.Prefix).HasMaxLength(8).IsRequired();
            e.Property(k => k.KeyHash).HasMaxLength(128).IsRequired();
            e.HasQueryFilter(k => (k.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !k.IsDeleted);
        });

        builder.Entity<WebhookSubscription>(e =>
        {
            e.HasKey(w => w.Id);
            e.Property(w => w.EventType).HasMaxLength(100).IsRequired();
            e.Property(w => w.TargetUrl).HasMaxLength(500).IsRequired();
            e.Property(w => w.Secret).HasMaxLength(100).IsRequired();
            e.Property(w => w.Description).HasMaxLength(500);
            e.HasQueryFilter(w => (w.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !w.IsDeleted);
        });

        builder.Entity<WebhookDeliveryLog>(e =>
        {
            e.HasKey(w => w.Id);
            e.Property(w => w.EventType).HasMaxLength(100).IsRequired();
            e.Property(w => w.TargetUrl).HasMaxLength(500).IsRequired();
            e.Property(w => w.ErrorMessage).HasMaxLength(1000);
            e.Property(w => w.PayloadJson);
            e.HasQueryFilter(w => (w.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !w.IsDeleted);
        });

        builder.Entity<PolicyDocument>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Title).HasMaxLength(200).IsRequired();
            e.Property(p => p.Content).IsRequired();
            e.HasQueryFilter(p => (p.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !p.IsDeleted);
        });

        builder.Entity<PolicyChunk>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Content).IsRequired();
            e.HasQueryFilter(c => (c.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !c.IsDeleted);
        });

        builder.Entity<Objective>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Title).HasMaxLength(200).IsRequired();
            e.HasQueryFilter(o => (o.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !o.IsDeleted);
        });

        builder.Entity<KeyResult>(e =>
        {
            e.HasKey(k => k.Id);
            e.Property(k => k.TargetValue).HasColumnType("decimal(18,2)");
            e.Property(k => k.CurrentValue).HasColumnType("decimal(18,2)");
            e.HasQueryFilter(k => (k.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !k.IsDeleted);
        });

        // ---- New Module: Multisucursal ----
        builder.Entity<Branch>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Name).HasMaxLength(255).IsRequired();
            e.Property(b => b.Code).HasMaxLength(50);
            e.Property(b => b.Address).HasMaxLength(500);
            e.Property(b => b.Phone).HasMaxLength(20);
            e.Property(b => b.Email).HasMaxLength(255);
            e.HasOne(b => b.Company)
                .WithMany(c => c.Branches)
                .HasForeignKey(b => b.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(b => (b.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !b.IsDeleted);
        });

        // ---- New Module: Comercial ----
        builder.Entity<Client>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Code).HasMaxLength(50).IsRequired();
            e.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
            e.Property(c => c.LastName).HasMaxLength(100).IsRequired();
            e.Property(c => c.IdentificationNumber).HasMaxLength(50);
            e.Property(c => c.Phone).HasMaxLength(20);
            e.Property(c => c.Address).HasMaxLength(500);
            e.Property(c => c.City).HasMaxLength(100);
            e.Property(c => c.State).HasMaxLength(100);
            e.Property(c => c.References).HasMaxLength(500);
            e.Property(c => c.Status).HasMaxLength(20).IsRequired();
            e.Property(c => c.CreditLimit).HasColumnType("decimal(18,2)");
            e.HasIndex(c => c.Code);
            e.HasQueryFilter(c => (c.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !c.IsDeleted);
        });

        builder.Entity<Quote>(e =>
        {
            e.HasKey(q => q.Id);
            e.Property(q => q.QuoteNumber).HasMaxLength(50).IsRequired();
            e.Property(q => q.Status).HasMaxLength(20).IsRequired();
            e.Property(q => q.Notes).HasMaxLength(500);
            e.Property(q => q.Subtotal).HasColumnType("decimal(18,2)");
            e.Property(q => q.Tax).HasColumnType("decimal(18,2)");
            e.Property(q => q.Discount).HasColumnType("decimal(18,2)");
            e.Property(q => q.Total).HasColumnType("decimal(18,2)");
            e.Property(q => q.CurrencyCode).HasMaxLength(3);
            e.Property(q => q.ExchangeRateToReporting).HasColumnType("decimal(18,6)");
            e.HasOne(q => q.Client)
                .WithMany(c => c.Quotes)
                .HasForeignKey(q => q.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(q => q.Employee)
                .WithMany()
                .HasForeignKey(q => q.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(q => q.QuoteNumber).IsUnique();
            e.HasQueryFilter(q => (q.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !q.IsDeleted);
        });

        builder.Entity<QuoteDetail>(e =>
        {
            e.HasKey(qd => qd.Id);
            e.Property(qd => qd.UnitPrice).HasColumnType("decimal(19,4)");
            e.Property(qd => qd.Discount).HasColumnType("decimal(18,2)");
            e.Property(qd => qd.Subtotal).HasColumnType("decimal(18,2)");
            e.HasOne(qd => qd.Quote)
                .WithMany(q => q.Details)
                .HasForeignKey(qd => qd.QuoteId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(qd => qd.Product)
                .WithMany()
                .HasForeignKey(qd => qd.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(qd => (qd.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !qd.IsDeleted);
        });

        builder.Entity<Sale>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.InvoiceNumber).HasMaxLength(50).IsRequired();
            e.Property(s => s.SaleType).HasMaxLength(20).IsRequired();
            e.Property(s => s.Status).HasMaxLength(20).IsRequired();
            e.Property(s => s.Notes).HasMaxLength(500);
            e.Property(s => s.Subtotal).HasColumnType("decimal(18,2)");
            e.Property(s => s.Tax).HasColumnType("decimal(18,2)");
            e.Property(s => s.Discount).HasColumnType("decimal(18,2)");
            e.Property(s => s.Total).HasColumnType("decimal(18,2)");
            e.Property(s => s.PaidAmount).HasColumnType("decimal(18,2)");
            e.Property(s => s.Balance).HasColumnType("decimal(18,2)");
            e.Property(s => s.CurrencyCode).HasMaxLength(3);
            e.Property(s => s.ExchangeRateToReporting).HasColumnType("decimal(18,6)");
            e.HasOne(s => s.Client)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Employee)
                .WithMany(emp => emp.Sales)
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(s => s.InvoiceNumber).IsUnique();
            e.HasIndex(s => s.SaleDate);
            e.HasQueryFilter(s => (s.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !s.IsDeleted);
        });

        builder.Entity<SaleDetail>(e =>
        {
            e.HasKey(sd => sd.Id);
            e.Property(sd => sd.UnitPrice).HasColumnType("decimal(19,4)");
            e.Property(sd => sd.Discount).HasColumnType("decimal(18,2)");
            e.Property(sd => sd.Subtotal).HasColumnType("decimal(18,2)");
            e.HasOne(sd => sd.Sale)
                .WithMany(s => s.Details)
                .HasForeignKey(sd => sd.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(sd => sd.Product)
                .WithMany()
                .HasForeignKey(sd => sd.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(sd => (sd.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !sd.IsDeleted);
        });

        builder.Entity<SalePayment>(e =>
        {
            e.HasKey(sp => sp.Id);
            e.Property(sp => sp.Amount).HasColumnType("decimal(18,2)");
            e.Property(sp => sp.PaymentMethod).HasMaxLength(50).IsRequired();
            e.Property(sp => sp.ReferenceNumber).HasMaxLength(100);
            e.HasOne(sp => sp.Sale)
                .WithMany(s => s.Payments)
                .HasForeignKey(sp => sp.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(sp => (sp.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !sp.IsDeleted);
        });

        builder.Entity<CreditNote>(e =>
        {
            e.HasKey(cn => cn.Id);
            e.Property(cn => cn.CreditNoteNumber).HasMaxLength(50).IsRequired();
            e.Property(cn => cn.Status).HasMaxLength(20).IsRequired();
            e.Property(cn => cn.Reason).HasMaxLength(500);
            e.Property(cn => cn.Subtotal).HasColumnType("decimal(18,2)");
            e.Property(cn => cn.Tax).HasColumnType("decimal(18,2)");
            e.Property(cn => cn.Total).HasColumnType("decimal(18,2)");
            e.HasOne(cn => cn.Sale)
                .WithMany()
                .HasForeignKey(cn => cn.SaleId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(cn => cn.CreditNoteNumber).IsUnique();
            e.HasQueryFilter(cn => (cn.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !cn.IsDeleted);
        });

        builder.Entity<CreditNoteDetail>(e =>
        {
            e.HasKey(cnd => cnd.Id);
            e.Property(cnd => cnd.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(cnd => cnd.Subtotal).HasColumnType("decimal(18,2)");
            e.Property(cnd => cnd.Tax).HasColumnType("decimal(18,2)");
            e.Property(cnd => cnd.Total).HasColumnType("decimal(18,2)");
            e.HasOne(cnd => cnd.CreditNote)
                .WithMany(cn => cn.Details)
                .HasForeignKey(cnd => cnd.CreditNoteId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(cnd => cnd.Product)
                .WithMany()
                .HasForeignKey(cnd => cnd.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(cnd => (cnd.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !cnd.IsDeleted);
        });

        // ---- New Module: Inventario ----
        builder.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
            e.Property(c => c.Description).HasMaxLength(500);
            e.HasIndex(c => new { c.Name, c.CompanyId }).IsUnique();
            e.HasQueryFilter(c => (c.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !c.IsDeleted);
        });

        builder.Entity<Brand>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Name).HasMaxLength(100).IsRequired();
            e.Property(b => b.Description).HasMaxLength(500);
            e.HasIndex(b => new { b.Name, b.CompanyId }).IsUnique();
            e.HasQueryFilter(b => (b.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !b.IsDeleted);
        });

        builder.Entity<Supplier>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Code).HasMaxLength(50).IsRequired();
            e.Property(s => s.Name).HasMaxLength(255).IsRequired();
            e.Property(s => s.ContactName).HasMaxLength(255);
            e.Property(s => s.Phone).HasMaxLength(20);
            e.Property(s => s.Email).HasMaxLength(255);
            e.Property(s => s.Address).HasMaxLength(500);
            e.Property(s => s.TaxId).HasMaxLength(50);
            e.HasIndex(s => new { s.TaxId, s.CompanyId }).IsUnique();
            e.HasQueryFilter(s => (s.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !s.IsDeleted);
        });

        builder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Code).HasMaxLength(50).IsRequired();
            e.Property(p => p.Name).HasMaxLength(255).IsRequired();
            e.Property(p => p.Description).HasMaxLength(500);
            e.Property(p => p.UnitOfMeasure).HasMaxLength(20).IsRequired();
            e.Property(p => p.Location).HasMaxLength(100);
            e.Property(p => p.Barcode).HasMaxLength(100);
            e.Property(p => p.ImageUrl).HasMaxLength(500);
            e.Property(p => p.CostPrice).HasColumnType("decimal(19,4)");
            e.Property(p => p.SellingPrice).HasColumnType("decimal(19,4)");
            e.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(p => new { p.Code, p.BranchId }).IsUnique();
            e.HasQueryFilter(p => (p.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !p.IsDeleted);
        });

        builder.Entity<InventoryMovement>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.MovementType).HasMaxLength(30).IsRequired();
            e.Property(m => m.UnitCost).HasColumnType("decimal(19,4)");
            e.Property(m => m.ReferenceNumber).HasMaxLength(100);
            e.Property(m => m.Notes).HasMaxLength(500);
            e.HasOne(m => m.Product)
                .WithMany(p => p.InventoryMovements)
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(m => m.PerformedBy)
                .WithMany(emp => emp.PerformedInventoryMovements)
                .HasForeignKey(m => m.PerformedByEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(m => new { m.ProductId, m.CreatedAt });
            e.HasQueryFilter(m => (m.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !m.IsDeleted);
        });

        // ---- New Module: CrÃÂÃÂ©ditos ----
        builder.Entity<Credit>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.CreditNumber).HasMaxLength(50).IsRequired();
            e.Property(c => c.FinancedAmount).HasColumnType("decimal(18,2)");
            e.Property(c => c.InterestRate).HasColumnType("decimal(5,2)");
            e.Property(c => c.InstallmentAmount).HasColumnType("decimal(18,2)");
            e.Property(c => c.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(c => c.PaidAmount).HasColumnType("decimal(18,2)");
            e.Property(c => c.Balance).HasColumnType("decimal(18,2)");
            e.Property(c => c.InterestAmount).HasColumnType("decimal(18,2)");
            e.Property(c => c.Status).HasMaxLength(20).IsRequired();
            e.Property(c => c.Notes).HasMaxLength(500);
            e.Property(c => c.CurrencyCode).HasMaxLength(3);
            e.Property(c => c.ExchangeRateToReporting).HasColumnType("decimal(18,6)");
            e.HasOne(c => c.Client)
                .WithMany(cl => cl.Credits)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Sale)
                .WithOne(s => s.Credit)
                .HasForeignKey<Credit>(c => c.SaleId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(c => c.Employee)
                .WithMany(emp => emp.ManagedCredits)
                .HasForeignKey(c => c.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(c => c.CreditNumber).IsUnique();
            e.HasQueryFilter(c => (c.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !c.IsDeleted);
        });

        builder.Entity<CreditInstallment>(e =>
        {
            e.HasKey(ci => ci.Id);
            e.Property(ci => ci.Amount).HasColumnType("decimal(18,2)");
            e.Property(ci => ci.PrincipalAmount).HasColumnType("decimal(18,2)");
            e.Property(ci => ci.InterestAmount).HasColumnType("decimal(18,2)");
            e.Property(ci => ci.PaidAmount).HasColumnType("decimal(18,2)");
            e.Property(ci => ci.Balance).HasColumnType("decimal(18,2)");
            e.Property(ci => ci.Status).HasMaxLength(20).IsRequired();
            e.HasOne(ci => ci.Credit)
                .WithMany(c => c.Installments)
                .HasForeignKey(ci => ci.CreditId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(ci => new { ci.CreditId, ci.InstallmentNumber }).IsUnique();
            e.HasQueryFilter(ci => (ci.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !ci.IsDeleted);
        });

        builder.Entity<CreditPayment>(e =>
        {
            e.HasKey(cp => cp.Id);
            e.Property(cp => cp.Amount).HasColumnType("decimal(18,2)");
            e.Property(cp => cp.PrincipalAmount).HasColumnType("decimal(18,2)");
            e.Property(cp => cp.InterestAmount).HasColumnType("decimal(18,2)");
            e.Property(cp => cp.PaymentMethod).HasMaxLength(50).IsRequired();
            e.Property(cp => cp.ReferenceNumber).HasMaxLength(100);
            e.HasOne(cp => cp.Credit)
                .WithMany(c => c.Payments)
                .HasForeignKey(cp => cp.CreditId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(cp => cp.CreditInstallment)
                .WithMany()
                .HasForeignKey(cp => cp.CreditInstallmentId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(cp => cp.Employee)
                .WithMany()
                .HasForeignKey(cp => cp.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(cp => (cp.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !cp.IsDeleted);
        });

        builder.Entity<LateFee>(e =>
        {
            e.HasKey(lf => lf.Id);
            e.Property(lf => lf.FeeAmount).HasColumnType("decimal(18,2)");
            e.Property(lf => lf.InterestAmount).HasColumnType("decimal(18,2)");
            e.Property(lf => lf.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(lf => lf.PaidAmount).HasColumnType("decimal(18,2)");
            e.Property(lf => lf.Balance).HasColumnType("decimal(18,2)");
            e.Property(lf => lf.Status).HasMaxLength(20).IsRequired();
            e.Property(lf => lf.Notes).HasMaxLength(500);
            e.HasOne(lf => lf.CreditInstallment)
                .WithMany()
                .HasForeignKey(lf => lf.CreditInstallmentId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(lf => lf.Credit)
                .WithMany()
                .HasForeignKey(lf => lf.CreditId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(lf => new { lf.CreditInstallmentId, lf.CalculatedAt });
            e.HasQueryFilter(lf => (lf.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !lf.IsDeleted);
        });

        builder.Entity<CollectionAction>(e =>
        {
            e.HasKey(ca => ca.Id);
            e.Property(ca => ca.ActionType).HasMaxLength(30).IsRequired();
            e.Property(ca => ca.Description).HasMaxLength(1000);
            e.Property(ca => ca.ContactPerson).HasMaxLength(200);
            e.Property(ca => ca.ContactPhone).HasMaxLength(20);
            e.Property(ca => ca.PromiseAmount).HasMaxLength(50);
            e.Property(ca => ca.Status).HasMaxLength(20).IsRequired();
            e.Property(ca => ca.Result).HasMaxLength(500);
            e.HasOne(ca => ca.Credit)
                .WithMany()
                .HasForeignKey(ca => ca.CreditId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ca => ca.Employee)
                .WithMany()
                .HasForeignKey(ca => ca.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(ca => new { ca.CreditId, ca.ActionDate });
            e.HasQueryFilter(ca => (ca.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !ca.IsDeleted);
        });

        builder.Entity<CreditRefinancing>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.PreviousBalance).HasColumnType("decimal(18,2)");
            e.Property(r => r.PreviousInterestRate).HasColumnType("decimal(5,2)");
            e.Property(r => r.PreviousInstallmentAmount).HasColumnType("decimal(18,2)");
            e.Property(r => r.NewFinancedAmount).HasColumnType("decimal(18,2)");
            e.Property(r => r.NewInterestRate).HasColumnType("decimal(5,2)");
            e.Property(r => r.NewInstallmentAmount).HasColumnType("decimal(18,2)");
            e.Property(r => r.NewTotalAmount).HasColumnType("decimal(18,2)");
            e.Property(r => r.NewInterestAmount).HasColumnType("decimal(18,2)");
            e.Property(r => r.Reason).HasMaxLength(500);
            e.HasOne(r => r.Credit)
                .WithMany(c => c.Refinancings)
                .HasForeignKey(r => r.CreditId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(r => (r.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !r.IsDeleted);
        });

        // ---- New Module: Caja ----
        builder.Entity<CashRegister>(e =>
        {
            e.HasKey(cr => cr.Id);
            e.Property(cr => cr.Code).HasMaxLength(50).IsRequired();
            e.Property(cr => cr.OpeningBalance).HasColumnType("decimal(18,2)");
            e.Property(cr => cr.ClosingBalance).HasColumnType("decimal(18,2)");
            e.Property(cr => cr.TotalIncome).HasColumnType("decimal(18,2)");
            e.Property(cr => cr.TotalExpense).HasColumnType("decimal(18,2)");
            e.Property(cr => cr.ExpectedBalance).HasColumnType("decimal(18,2)");
            e.Property(cr => cr.Difference).HasColumnType("decimal(18,2)");
            e.Property(cr => cr.Status).HasMaxLength(20).IsRequired();
            e.Property(cr => cr.Notes).HasMaxLength(500);
            e.HasOne(cr => cr.Employee)
                .WithMany(emp => emp.CashRegisters)
                .HasForeignKey(cr => cr.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(cr => new { cr.BranchId, cr.Status });
            e.HasQueryFilter(cr => (cr.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !cr.IsDeleted);
        });

        builder.Entity<CashMovement>(e =>
        {
            e.HasKey(cm => cm.Id);
            e.Property(cm => cm.MovementType).HasMaxLength(20).IsRequired();
            e.Property(cm => cm.Amount).HasColumnType("decimal(18,2)");
            e.Property(cm => cm.Concept).HasMaxLength(255);
            e.Property(cm => cm.ReferenceNumber).HasMaxLength(100);
            e.Property(cm => cm.DocumentReference).HasMaxLength(100);
            e.Property(cm => cm.ApprovalStatus).HasMaxLength(20).IsRequired();
            e.HasOne(cm => cm.CashRegister)
                .WithMany(cr => cr.Movements)
                .HasForeignKey(cm => cm.CashRegisterId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(cm => cm.Employee)
                .WithMany()
                .HasForeignKey(cm => cm.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(cm => (cm.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !cm.IsDeleted);
        });

        builder.Entity<CashRegisterArqueo>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.ExpectedBalance).HasColumnType("decimal(18,2)");
            e.Property(a => a.CountedTotal).HasColumnType("decimal(18,2)");
            e.Property(a => a.Difference).HasColumnType("decimal(18,2)");
            e.Property(a => a.Notes).HasMaxLength(500);
            e.HasOne(a => a.CashRegister)
                .WithMany()
                .HasForeignKey(a => a.CashRegisterId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Employee)
                .WithMany()
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasMany(a => a.Denominations)
                .WithOne(d => d.Arqueo)
                .HasForeignKey(d => d.ArqueoId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(a => a.CashRegisterId).IsUnique();
            e.HasQueryFilter(a => (a.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !a.IsDeleted);
        });

        builder.Entity<CashArqueoDenomination>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.DenominationType).HasMaxLength(10).IsRequired();
            e.Property(d => d.DenominationValue).HasColumnType("decimal(18,2)");
            e.Ignore(d => d.Total);
            e.HasQueryFilter(d => (d.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !d.IsDeleted);
        });

        // ---- New Module: Contabilidad ----
        builder.Entity<Account>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Code).HasMaxLength(50).IsRequired();
            e.Property(a => a.Name).HasMaxLength(255).IsRequired();
            e.Property(a => a.Description).HasMaxLength(500);
            e.Property(a => a.Type).HasMaxLength(30).IsRequired();
            e.Property(a => a.NormalSide).HasMaxLength(10).IsRequired();
            e.Property(a => a.OpeningBalance).HasColumnType("decimal(18,2)");
            e.HasOne(a => a.Parent)
                .WithMany(a => a.Children)
                .HasForeignKey(a => a.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.CostCenter)
                .WithMany()
                .HasForeignKey(a => a.CostCenterId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(a => new { a.Code, a.CompanyId }).IsUnique();
            e.HasQueryFilter(a => (a.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !a.IsDeleted);
        });

        builder.Entity<FiscalYear>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Name).HasMaxLength(50).IsRequired();
            e.Property(f => f.Status).HasMaxLength(20).IsRequired();
            e.Property(f => f.StartDate).HasColumnType("date");
            e.Property(f => f.EndDate).HasColumnType("date");
            e.HasIndex(f => new { f.Year, f.CompanyId }).IsUnique();
            e.HasMany(f => f.Periods)
                .WithOne(p => p.FiscalYear)
                .HasForeignKey(p => p.FiscalYearId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(f => (f.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !f.IsDeleted);
        });

        builder.Entity<AccountingPeriod>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).HasMaxLength(20).IsRequired();
            e.Property(p => p.Status).HasMaxLength(20).IsRequired();
            e.Property(p => p.CloseNotes).HasMaxLength(500);
            e.Property(p => p.ReopenReason).HasMaxLength(500);
            e.HasIndex(p => new { p.Year, p.Month, p.CompanyId }).IsUnique();
            e.HasIndex(p => p.Status);
            e.HasQueryFilter(p => (p.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !p.IsDeleted);
        });

        builder.Entity<AccountingEntry>(e =>
        {
            e.HasKey(en => en.Id);
            e.Property(en => en.EntryNumber).HasMaxLength(50).IsRequired();
            e.Property(en => en.Description).HasMaxLength(500).IsRequired();
            e.Property(en => en.ReferenceType).HasMaxLength(50);
            e.Property(en => en.Status).HasMaxLength(20).IsRequired();
            e.Property(en => en.TotalDebit).HasColumnType("decimal(18,2)");
            e.Property(en => en.TotalCredit).HasColumnType("decimal(18,2)");
            e.Property(en => en.CurrencyCode).HasMaxLength(3);
            e.Property(en => en.ExchangeRateToReporting).HasColumnType("decimal(18,6)");
            e.HasOne(en => en.AccountingPeriod)
                .WithMany(p => p.Entries)
                .HasForeignKey(en => en.AccountingPeriodId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(en => en.CostCenter)
                .WithMany()
                .HasForeignKey(en => en.CostCenterId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(en => en.EntryNumber).IsUnique();
            e.HasIndex(en => en.EntryDate);
            e.HasQueryFilter(en => (en.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !en.IsDeleted);
        });

        builder.Entity<AccountingEntryDetail>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.DebitAmount).HasColumnType("decimal(18,2)");
            e.Property(d => d.CreditAmount).HasColumnType("decimal(18,2)");
            e.Property(d => d.Description).HasMaxLength(500);
            e.HasOne(d => d.AccountingEntry)
                .WithMany(en => en.Details)
                .HasForeignKey(d => d.AccountingEntryId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(d => d.Account)
                .WithMany()
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(d => d.CostCenter)
                .WithMany()
                .HasForeignKey(d => d.CostCenterId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(d => (d.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !d.IsDeleted);
        });

        builder.Entity<AccountLink>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.TransactionType).HasMaxLength(50).IsRequired();
            e.Property(l => l.Role).HasMaxLength(50).IsRequired();
            e.HasOne(l => l.Account)
                .WithMany()
                .HasForeignKey(l => l.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(l => new { l.TransactionType, l.Role, l.CompanyId }).IsUnique();
            e.HasQueryFilter(l => (l.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !l.IsDeleted);
        });

        builder.Entity<AccountingRule>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.EventType).HasMaxLength(50).IsRequired();
            e.Property(r => r.LineType).HasMaxLength(10).IsRequired();
            e.Property(r => r.AccountRole).HasMaxLength(50).IsRequired();
            e.Property(r => r.Formula).HasMaxLength(200);
            e.HasQueryFilter(r => (r.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !r.IsDeleted);
        });

        builder.Entity<CostCenter>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(200).IsRequired();
            e.Property(c => c.Code).HasMaxLength(50).IsRequired();
            e.Property(c => c.Description).HasMaxLength(500);
            e.HasIndex(c => new { c.Code, c.CompanyId }).IsUnique();
            e.HasQueryFilter(c => (c.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !c.IsDeleted);
        });

        builder.Entity<Budget>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.BudgetedAmount).HasColumnType("decimal(18,2)").IsRequired();
            e.HasOne(b => b.Account)
                .WithMany()
                .HasForeignKey(b => b.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(b => b.CostCenter)
                .WithMany()
                .HasForeignKey(b => b.CostCenterId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(b => new { b.Year, b.Month, b.AccountId, b.CostCenterId, b.CompanyId }).IsUnique();
            e.HasQueryFilter(b => (b.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !b.IsDeleted);
        });

        // ---- New Module: TesorerÃÂÃÂ­a ----
        builder.Entity<Bank>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Name).HasMaxLength(100).IsRequired();
            e.HasQueryFilter(b => (b.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !b.IsDeleted);
        });

        builder.Entity<BankAccount>(e =>
        {
            e.HasKey(ba => ba.Id);
            e.Property(ba => ba.AccountNumber).HasMaxLength(50).IsRequired();
            e.HasOne(ba => ba.Bank).WithMany().HasForeignKey(ba => ba.BankId);
            e.HasQueryFilter(ba => (ba.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !ba.IsDeleted);
        });

        builder.Entity<Checkbook>(e =>
        {
            e.HasKey(cb => cb.Id);
            e.Property(cb => cb.Series).HasMaxLength(20).IsRequired();
            e.HasOne(cb => cb.BankAccount).WithMany().HasForeignKey(cb => cb.BankAccountId);
            e.HasQueryFilter(cb => (cb.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !cb.IsDeleted);
        });

        builder.Entity<Check>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Beneficiary).HasMaxLength(200).IsRequired();
            e.HasOne(c => c.BankAccount).WithMany().HasForeignKey(c => c.BankAccountId);
            e.HasQueryFilter(c => (c.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !c.IsDeleted);
        });

        builder.Entity<CheckAuditTrail>(e =>
        {
            e.HasKey(cat => cat.Id);
            e.HasOne(cat => cat.Check).WithMany().HasForeignKey(cat => cat.CheckId);
            e.HasQueryFilter(cat => (cat.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !cat.IsDeleted);
        });

        builder.Entity<CheckPrintTemplate>(e =>
        {
            e.HasKey(cpt => cpt.Id);
            e.HasOne(cpt => cpt.Bank).WithMany().HasForeignKey(cpt => cpt.BankId);
            e.HasQueryFilter(cpt => (cpt.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !cpt.IsDeleted);
        });

        builder.Entity<ApprovalFlowConfig>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Module).HasMaxLength(50).IsRequired();
            e.Property(a => a.EventType).HasMaxLength(50).IsRequired();
            e.Property(a => a.Description).HasMaxLength(500);
            e.HasMany(a => a.Steps)
                .WithOne(s => s.ApprovalFlowConfig)
                .HasForeignKey(s => s.ApprovalFlowConfigId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(a => new { a.Module, a.EventType, a.CompanyId }).IsUnique();
            e.HasQueryFilter(a => (a.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !a.IsDeleted);
        });

        builder.Entity<ApprovalFlowStep>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.ApproverRole).HasMaxLength(50).IsRequired();
            e.Property(s => s.MinAmount).HasColumnType("decimal(18,2)");
            e.Property(s => s.MaxAmount).HasColumnType("decimal(18,2)");
            e.HasQueryFilter(s => (s.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !s.IsDeleted);
        });

        builder.Entity<ApprovalRequest>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Module).HasMaxLength(50).IsRequired();
            e.Property(r => r.EventType).HasMaxLength(50).IsRequired();
            e.Property(r => r.Status).HasMaxLength(20).IsRequired();
            e.Property(r => r.RequestedBy).HasMaxLength(100).IsRequired();
            e.Property(r => r.Notes).HasMaxLength(500);
            e.HasMany(r => r.Actions)
                .WithOne(a => a.ApprovalRequest)
                .HasForeignKey(a => a.ApprovalRequestId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(r => (r.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !r.IsDeleted);
        });

        builder.Entity<ApprovalRequestAction>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Action).HasMaxLength(20).IsRequired();
            e.Property(a => a.Comment).HasMaxLength(500);
            e.Property(a => a.ActedBy).HasMaxLength(100).IsRequired();
            e.HasQueryFilter(a => (a.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !a.IsDeleted);
        });

        // ---- New Module: Compras ----
        builder.Entity<Purchase>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.PurchaseNumber).HasMaxLength(50).IsRequired();
            e.Property(p => p.Status).HasMaxLength(20).IsRequired();
            e.Property(p => p.Notes).HasMaxLength(500);
            e.Property(p => p.InvoiceReference).HasMaxLength(100);
            e.Property(p => p.WithholdingType).HasMaxLength(30);
            e.Property(p => p.Subtotal).HasColumnType("decimal(18,2)");
            e.Property(p => p.Tax).HasColumnType("decimal(18,2)");
            e.Property(p => p.Discount).HasColumnType("decimal(18,2)");
            e.Property(p => p.Total).HasColumnType("decimal(18,2)");
            e.Property(p => p.PaidAmount).HasColumnType("decimal(18,2)");
            e.Property(p => p.Balance).HasColumnType("decimal(18,2)");
            e.Property(p => p.WithholdingRate).HasColumnType("decimal(5,2)");
            e.Property(p => p.WithholdingAmount).HasColumnType("decimal(18,2)");
            e.Property(p => p.CurrencyCode).HasMaxLength(3);
            e.Property(p => p.ExchangeRateToReporting).HasColumnType("decimal(18,6)");
            e.HasOne(p => p.Supplier)
                .WithMany()
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(p => p.PurchaseNumber).IsUnique();
            e.HasQueryFilter(p => (p.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !p.IsDeleted);
        });

        builder.Entity<PurchaseDetail>(e =>
        {
            e.HasKey(pd => pd.Id);
            e.Property(pd => pd.UnitCost).HasColumnType("decimal(19,4)");
            e.Property(pd => pd.Discount).HasColumnType("decimal(18,2)");
            e.Property(pd => pd.Subtotal).HasColumnType("decimal(18,2)");
            e.HasOne(pd => pd.Purchase)
                .WithMany(p => p.Details)
                .HasForeignKey(pd => pd.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(pd => pd.Product)
                .WithMany()
                .HasForeignKey(pd => pd.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(pd => (pd.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !pd.IsDeleted);
        });

        // ---- PurchaseOrder Configuration ----
        builder.Entity<PurchaseOrder>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.OrderNumber).HasMaxLength(30).IsRequired();
            e.Property(o => o.Status).HasMaxLength(30).IsRequired();
            e.Property(o => o.Subtotal).HasColumnType("decimal(18,2)");
            e.Property(o => o.Tax).HasColumnType("decimal(18,2)");
            e.Property(o => o.Discount).HasColumnType("decimal(18,2)");
            e.Property(o => o.Total).HasColumnType("decimal(18,2)");
            e.Property(o => o.CurrencyCode).HasMaxLength(3);
            e.Property(o => o.CountryCode).HasMaxLength(3).IsRequired();
            e.Property(o => o.ExchangeRateToReporting).HasColumnType("decimal(18,6)");
            e.Property(o => o.Notes).HasMaxLength(500);
            e.HasOne(o => o.Supplier)
                .WithMany()
                .HasForeignKey(o => o.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(o => o.OrderNumber).IsUnique();
            e.HasQueryFilter(o => (o.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !o.IsDeleted);
        });

        builder.Entity<PurchaseOrderDetail>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.UnitCost).HasColumnType("decimal(19,4)");
            e.Property(d => d.Discount).HasColumnType("decimal(18,2)");
            e.Property(d => d.Subtotal).HasColumnType("decimal(18,2)");
            e.HasOne(d => d.PurchaseOrder)
                .WithMany(o => o.Details)
                .HasForeignKey(d => d.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(d => (d.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !d.IsDeleted);
        });

        // Add Purchase -> PurchaseOrder relationship
        builder.Entity<Purchase>(e =>
        {
            e.Property(p => p.CountryCode).HasMaxLength(3).IsRequired();
            e.HasOne(p => p.PurchaseOrder)
                .WithMany()
                .HasForeignKey(p => p.PurchaseOrderId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ---- SupplierPayment Configuration ----
        builder.Entity<SupplierPayment>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            e.Property(p => p.PaymentMethod).HasMaxLength(30).IsRequired();
            e.Property(p => p.ReferenceNumber).HasMaxLength(100);
            e.Property(p => p.Notes).HasMaxLength(500);
            e.HasOne(p => p.Purchase)
                .WithMany(pur => pur.Payments)
                .HasForeignKey(p => p.PurchaseId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(p => (p.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !p.IsDeleted);
        });

        // ---- SupplierCreditNote Configuration ----
        builder.Entity<SupplierCreditNote>(e =>
        {
            e.HasKey(cn => cn.Id);
            e.Property(cn => cn.CreditNoteNumber).HasMaxLength(50).IsRequired();
            e.Property(cn => cn.Reason).HasMaxLength(1000);
            e.Property(cn => cn.Status).HasMaxLength(20).IsRequired();
            e.Property(cn => cn.Subtotal).HasColumnType("decimal(18,2)");
            e.Property(cn => cn.Tax).HasColumnType("decimal(18,2)");
            e.Property(cn => cn.Total).HasColumnType("decimal(18,2)");
            e.Property(cn => cn.Amount).HasColumnType("decimal(18,2)");
            e.Property(cn => cn.CurrencyCode).HasMaxLength(3).IsRequired();
            e.Property(cn => cn.Notes).HasMaxLength(1000);
            e.HasOne(cn => cn.Supplier)
                .WithMany()
                .HasForeignKey(cn => cn.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(cn => cn.Purchase)
                .WithMany(pur => pur.CreditNotes)
                .HasForeignKey(cn => cn.PurchaseId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(cn => cn.Warranty)
                .WithMany()
                .HasForeignKey(cn => cn.WarrantyId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(cn => cn.WarrantyPartRequest)
                .WithMany()
                .HasForeignKey(cn => cn.WarrantyPartRequestId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(cn => cn.WarrantyProvider)
                .WithMany()
                .HasForeignKey(cn => cn.WarrantyProviderId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(cn => cn.WarrantyCost)
                .WithMany()
                .HasForeignKey(cn => cn.WarrantyCostId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(cn => cn.CreditNoteNumber).IsUnique();
            e.HasQueryFilter(cn => (cn.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !cn.IsDeleted);
        });

        // ---- Withholding Configuration ----
        builder.Entity<Withholding>(e =>
        {
            e.HasKey(w => w.Id);
            e.Property(w => w.WithholdingType).HasMaxLength(30).IsRequired();
            e.Property(w => w.Rate).HasColumnType("decimal(5,2)");
            e.Property(w => w.BaseAmount).HasColumnType("decimal(18,2)");
            e.Property(w => w.Amount).HasColumnType("decimal(18,2)");
            e.Property(w => w.CertificateNumber).HasMaxLength(50);
            e.Property(w => w.Status).HasMaxLength(20).IsRequired();
            e.HasOne(w => w.Purchase)
                .WithMany()
                .HasForeignKey(w => w.PurchaseId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(w => (w.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !w.IsDeleted);
        });

        // ---- New Module: Activos Fijos ----
        builder.Entity<FixedAssetCategory>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
            e.Property(c => c.Description).HasMaxLength(500);
            e.Property(c => c.DefaultDepreciationMethod).HasMaxLength(20);
            e.HasQueryFilter(c => (c.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !c.IsDeleted);
        });

        builder.Entity<Location>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Name).HasMaxLength(100).IsRequired();
            e.Property(l => l.Description).HasMaxLength(500);
            e.Property(l => l.Address).HasMaxLength(500);
            e.HasQueryFilter(l => (l.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !l.IsDeleted);
        });

        builder.Entity<FixedAsset>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Code).HasMaxLength(50).IsRequired();
            e.Property(a => a.Name).HasMaxLength(255).IsRequired();
            e.Property(a => a.Description).HasMaxLength(1000);
            e.Property(a => a.SerialNumber).HasMaxLength(100);
            e.Property(a => a.Barcode).HasMaxLength(100);
            e.Property(a => a.Brand).HasMaxLength(100);
            e.Property(a => a.Model).HasMaxLength(100);
            e.Property(a => a.InvoiceReference).HasMaxLength(100);
            e.Property(a => a.DepreciationMethod).HasMaxLength(20).IsRequired();
            e.Property(a => a.Status).HasMaxLength(20).IsRequired();
            e.Property(a => a.AssignedTo).HasMaxLength(255);
            e.Property(a => a.ImageUrl).HasMaxLength(500);
            e.Property(a => a.AcquisitionCost).HasColumnType("decimal(18,2)");
            e.Property(a => a.ResidualValue).HasColumnType("decimal(18,2)");
            e.Property(a => a.TotalUnits).HasColumnType("decimal(18,2)");
            e.Property(a => a.UnitsProduced).HasColumnType("decimal(18,2)");
            e.HasOne(a => a.Category)
                .WithMany()
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(a => a.Supplier)
                .WithMany()
                .HasForeignKey(a => a.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(a => a.Location)
                .WithMany()
                .HasForeignKey(a => a.LocationId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(a => a.Department)
                .WithMany()
                .HasForeignKey(a => a.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(a => a.Purchase)
                .WithMany()
                .HasForeignKey(a => a.PurchaseId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(a => a.Code).IsUnique();
            e.HasQueryFilter(a => (a.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !a.IsDeleted);
        });

        builder.Entity<DepreciationEntry>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Amount).HasColumnType("decimal(18,2)");
            e.Property(d => d.AccumulatedDepreciation).HasColumnType("decimal(18,2)");
            e.Property(d => d.NetBookValue).HasColumnType("decimal(18,2)");
            e.Property(d => d.Notes).HasMaxLength(500);
            e.HasOne(d => d.FixedAsset)
                .WithMany(a => a.DepreciationEntries)
                .HasForeignKey(d => d.FixedAssetId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(d => d.AccountingEntry)
                .WithMany()
                .HasForeignKey(d => d.AccountingEntryId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(d => (d.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !d.IsDeleted);
        });

        builder.Entity<AssetRevaluation>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.PreviousValue).HasColumnType("decimal(18,2)");
            e.Property(r => r.NewValue).HasColumnType("decimal(18,2)");
            e.Property(r => r.PreviousAccumulatedDepreciation).HasColumnType("decimal(18,2)");
            e.Property(r => r.Reason).HasMaxLength(500);
            e.Property(r => r.ApprovedBy).HasMaxLength(255);
            e.HasOne(r => r.FixedAsset)
                .WithMany(a => a.Revaluations)
                .HasForeignKey(r => r.FixedAssetId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.AccountingEntry)
                .WithMany()
                .HasForeignKey(r => r.AccountingEntryId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(r => (r.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !r.IsDeleted);
        });

        builder.Entity<AssetMaintenance>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Description).HasMaxLength(1000).IsRequired();
            e.Property(m => m.MaintenanceType).HasMaxLength(20).IsRequired();
            e.Property(m => m.Provider).HasMaxLength(255);
            e.Property(m => m.Status).HasMaxLength(20).IsRequired();
            e.Property(m => m.Cost).HasColumnType("decimal(18,2)");
            e.HasOne(m => m.FixedAsset)
                .WithMany(a => a.MaintenanceRecords)
                .HasForeignKey(m => m.FixedAssetId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(m => (m.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !m.IsDeleted);
        });

        builder.Entity<AssetDisposal>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.DisposalType).HasMaxLength(20).IsRequired();
            e.Property(d => d.Reason).HasMaxLength(1000);
            e.Property(d => d.ApprovedBy).HasMaxLength(255);
            e.Property(d => d.SaleAmount).HasColumnType("decimal(18,2)");
            e.Property(d => d.NetBookValueAtDisposal).HasColumnType("decimal(18,2)");
            e.Property(d => d.GainOrLoss).HasColumnType("decimal(18,2)");
            e.HasOne(d => d.FixedAsset)
                .WithOne(a => a.Disposal)
                .HasForeignKey<AssetDisposal>(d => d.FixedAssetId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(d => d.AccountingEntry)
                .WithMany()
                .HasForeignKey(d => d.AccountingEntryId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(d => (d.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !d.IsDeleted);
        });

        // ---- New Module: GarantÃÂÃÂ­as ----
        builder.Entity<Warranty>(e =>
        {
            e.HasKey(w => w.Id);
            e.Property(w => w.WarrantyNumber).HasMaxLength(50).IsRequired();
            e.Property(w => w.Terms).HasMaxLength(2000);
            e.Property(w => w.SerialNumber).HasMaxLength(100);
            e.Property(w => w.Imei).HasMaxLength(20);
            e.Property(w => w.LotNumber).HasMaxLength(50);
            e.Property(w => w.Status)
                .HasMaxLength(30)
                .IsRequired()
                .HasConversion(new EnumToStringConverter<WarrantyStatus>());
            e.HasOne(w => w.Client)
                .WithMany(c => c.Warranties)
                .HasForeignKey(w => w.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(w => w.Product)
                .WithMany()
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(w => w.Sale)
                .WithMany()
                .HasForeignKey(w => w.SaleId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(w => w.Brand)
                .WithMany()
                .HasForeignKey(w => w.BrandId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(w => w.Category)
                .WithMany()
                .HasForeignKey(w => w.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(w => w.WarrantyNumber).IsUnique();
            e.HasQueryFilter(w => (w.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !w.IsDeleted);
        });

        builder.Entity<WarrantyClaim>(e =>
        {
            e.HasKey(wc => wc.Id);
            e.Property(wc => wc.Description).HasMaxLength(2000).IsRequired();
            e.Property(wc => wc.Status)
                .HasMaxLength(30)
                .IsRequired()
                .HasConversion(new EnumToStringConverter<WarrantyStatus>());
            e.Property(wc => wc.Resolution).HasMaxLength(2000);
            e.HasOne(wc => wc.Warranty)
                .WithMany(w => w.Claims)
                .HasForeignKey(wc => wc.WarrantyId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(wc => wc.ApprovedBy)
                .WithMany()
                .HasForeignKey(wc => wc.ApprovedByEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.Property(wc => wc.Accessories).HasMaxLength(500);
            e.Property(wc => wc.FailureType).HasMaxLength(100);
            e.Property(wc => wc.FailureDescription).HasMaxLength(2000);
            e.Property(wc => wc.Priority).HasMaxLength(20).IsRequired();
            e.Property(wc => wc.ProductCondition).HasMaxLength(100);
            e.Property(wc => wc.ProviderAuthorizationCode).HasMaxLength(100);
            e.HasOne(wc => wc.Workshop)
                .WithMany()
                .HasForeignKey(wc => wc.WorkshopId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(wc => wc.Technician)
                .WithMany()
                .HasForeignKey(wc => wc.TechnicianId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(wc => wc.Provider)
                .WithMany()
                .HasForeignKey(wc => wc.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(wc => (wc.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !wc.IsDeleted);
        });

        builder.Entity<ServiceWorkshop>(e =>
        {
            e.HasKey(w => w.Id);
            e.Property(w => w.Code).HasMaxLength(20).IsRequired();
            e.Property(w => w.Name).HasMaxLength(255).IsRequired();
            e.Property(w => w.LegalName).HasMaxLength(255);
            e.Property(w => w.TaxId).HasMaxLength(50);
            e.Property(w => w.ContactName).HasMaxLength(255);
            e.Property(w => w.Phone).HasMaxLength(50);
            e.Property(w => w.Email).HasMaxLength(255);
            e.Property(w => w.Address).HasMaxLength(500);
            e.Property(w => w.City).HasMaxLength(100);
            e.Property(w => w.Country).HasMaxLength(100);
            e.Property(w => w.Notes).HasMaxLength(2000);
            e.HasOne(w => w.Branch)
                .WithMany()
                .HasForeignKey(w => w.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasMany(w => w.Technicians)
                .WithOne(t => t.Workshop)
                .HasForeignKey(t => t.WorkshopId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(w => new { w.TenantId, w.Code }).IsUnique();
            e.HasQueryFilter(w => (w.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !w.IsDeleted);
        });

        builder.Entity<WorkshopTechnician>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.FullName).HasMaxLength(255).IsRequired();
            e.Property(t => t.Identification).HasMaxLength(50);
            e.Property(t => t.Phone).HasMaxLength(50);
            e.Property(t => t.Email).HasMaxLength(255);
            e.Property(t => t.Specialties).HasColumnType("text[]");
            e.HasOne(t => t.Workshop)
                .WithMany(w => w.Technicians)
                .HasForeignKey(t => t.WorkshopId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(t => (t.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !t.IsDeleted);
        });

        builder.Entity<WorkshopBrand>(e =>
        {
            e.HasKey(wb => new { wb.WorkshopId, wb.BrandId });
            e.Property(wb => wb.TenantId).HasMaxLength(50).IsRequired();
            e.HasOne(wb => wb.Workshop)
                .WithMany()
                .HasForeignKey(wb => wb.WorkshopId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(wb => wb.Brand)
                .WithMany()
                .HasForeignKey(wb => wb.BrandId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WarrantyProvider>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Code).HasMaxLength(20).IsRequired();
            e.Property(p => p.Name).HasMaxLength(255).IsRequired();
            e.Property(p => p.LegalName).HasMaxLength(255);
            e.Property(p => p.TaxId).HasMaxLength(50);
            e.Property(p => p.Type).HasMaxLength(20).IsRequired();
            e.Property(p => p.ContactName).HasMaxLength(255);
            e.Property(p => p.Phone).HasMaxLength(50);
            e.Property(p => p.Email).HasMaxLength(255);
            e.Property(p => p.Address).HasMaxLength(500);
            e.Property(p => p.City).HasMaxLength(100);
            e.Property(p => p.Country).HasMaxLength(100);
            e.Property(p => p.Website).HasMaxLength(255);
            e.Property(p => p.Notes).HasMaxLength(2000);
            e.HasMany(p => p.Contacts)
                .WithOne(c => c.Provider)
                .HasForeignKey(c => c.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(p => new { p.TenantId, p.Code }).IsUnique();
            e.HasQueryFilter(p => (p.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !p.IsDeleted);
        });

        builder.Entity<ProviderContact>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.FullName).HasMaxLength(255).IsRequired();
            e.Property(c => c.Role).HasMaxLength(100);
            e.Property(c => c.Phone).HasMaxLength(50);
            e.Property(c => c.Email).HasMaxLength(255);
            e.HasOne(c => c.Provider)
                .WithMany(p => p.Contacts)
                .HasForeignKey(c => c.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(c => (c.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !c.IsDeleted);
        });

        builder.Entity<ProviderBrand>(e =>
        {
            e.HasKey(pb => new { pb.ProviderId, pb.BrandId });
            e.Property(pb => pb.TenantId).HasMaxLength(50).IsRequired();
            e.HasOne(pb => pb.Provider)
                .WithMany()
                .HasForeignKey(pb => pb.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(pb => pb.Brand)
                .WithMany()
                .HasForeignKey(pb => pb.BrandId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Warranty: Fase 1.4-2.4 entities ----
        builder.Entity<WarrantyCost>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.CostCategory).HasMaxLength(50).IsRequired();
            e.Property(c => c.Description).HasMaxLength(1000);
            e.Property(c => c.Quantity).HasColumnType("decimal(18,2)");
            e.Property(c => c.UnitCost).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(c => c.CurrencyCode).HasMaxLength(3).IsRequired();
            e.Property(c => c.ExchangeRate).HasColumnType("decimal(18,6)");
            e.Property(c => c.PaidBy).HasMaxLength(20).IsRequired();
            e.Property(c => c.InvoiceNumber).HasMaxLength(100);
            e.Property(c => c.InvoiceUrl).HasMaxLength(500);
            e.Property(c => c.Notes).HasMaxLength(1000);
            e.HasOne(c => c.Warranty)
                .WithMany(w => w.Costs)
                .HasForeignKey(c => c.WarrantyId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.Claim)
                .WithMany()
                .HasForeignKey(c => c.ClaimId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(c => c.RegisteredBy)
                .WithMany()
                .HasForeignKey(c => c.RegisteredByEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(c => (c.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !c.IsDeleted);
        });

        builder.Entity<WarrantyPartRequest>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.QuantityRequested).IsRequired();
            e.Property(r => r.QuantityReceived).IsRequired();
            e.Property(r => r.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(r => r.CurrencyCode).HasMaxLength(3).IsRequired();
            e.Property(r => r.RequestNumber).HasMaxLength(50).IsRequired();
            e.Property(r => r.Status).HasMaxLength(20).IsRequired();
            e.Property(r => r.ProviderAuthorizationCode).HasMaxLength(100);
            e.Property(r => r.ProviderNotes).HasMaxLength(1000);
            e.Property(r => r.InternalNotes).HasMaxLength(1000);
            e.HasOne(r => r.Warranty)
                .WithMany()
                .HasForeignKey(r => r.WarrantyId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Claim)
                .WithMany()
                .HasForeignKey(r => r.ClaimId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Provider)
                .WithMany()
                .HasForeignKey(r => r.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.RequestedBy)
                .WithMany()
                .HasForeignKey(r => r.RequestedByEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(r => r.ApprovedBy)
                .WithMany()
                .HasForeignKey(r => r.ApprovedByEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(r => r.RequestNumber).IsUnique();
            e.HasQueryFilter(r => (r.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !r.IsDeleted);
        });

        builder.Entity<WarrantyPartReceipt>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.QuantityReceived).IsRequired();
            e.Property(r => r.BatchLot).HasMaxLength(100);
            e.Property(r => r.SerialNumber).HasMaxLength(100);
            e.Property(r => r.Condition).HasMaxLength(50);
            e.Property(r => r.Notes).HasMaxLength(1000);
            e.HasOne(r => r.PartRequest)
                .WithMany()
                .HasForeignKey(r => r.PartRequestId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.StorageLocation)
                .WithMany()
                .HasForeignKey(r => r.StorageLocationId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(r => r.ReceivedBy)
                .WithMany()
                .HasForeignKey(r => r.ReceivedByEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(r => (r.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !r.IsDeleted);
        });

        builder.Entity<WarrantyPartUsage>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.QuantityUsed).IsRequired();
            e.Property(u => u.UnitCost).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(u => u.Notes).HasMaxLength(1000);
            e.HasOne(u => u.Claim)
                .WithMany()
                .HasForeignKey(u => u.ClaimId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(u => u.PartReceipt)
                .WithMany()
                .HasForeignKey(u => u.PartReceiptId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(u => u.Product)
                .WithMany()
                .HasForeignKey(u => u.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(u => u.UsedBy)
                .WithMany()
                .HasForeignKey(u => u.UsedByEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(u => (u.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !u.IsDeleted);
        });

        builder.Entity<WarrantyCommunication>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Channel).HasMaxLength(20).IsRequired();
            e.Property(c => c.Direction).HasMaxLength(20).IsRequired();
            e.Property(c => c.Subject).HasMaxLength(500);
            e.Property(c => c.Body).IsRequired();
            e.Property(c => c.Status).HasMaxLength(20).IsRequired();
            e.Property(c => c.ErrorMessage).HasMaxLength(1000);
            e.Property(c => c.ExternalId).HasMaxLength(200);
            e.Property(c => c.Metadata).HasMaxLength(2000);
            e.HasOne(c => c.Warranty)
                .WithMany()
                .HasForeignKey(c => c.WarrantyId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.Claim)
                .WithMany()
                .HasForeignKey(c => c.ClaimId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(c => c.SentBy)
                .WithMany()
                .HasForeignKey(c => c.SentByEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(c => (c.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !c.IsDeleted);
        });

        builder.Entity<WarrantyEvent>(e =>
        {
            e.HasKey(ev => ev.Id);
            e.Property(ev => ev.EventType).HasMaxLength(50).IsRequired();
            e.Property(ev => ev.EventData).HasMaxLength(2000);
            e.Property(ev => ev.Description).HasMaxLength(1000);
            e.HasOne(ev => ev.Warranty)
                .WithMany()
                .HasForeignKey(ev => ev.WarrantyId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ev => ev.Claim)
                .WithMany()
                .HasForeignKey(ev => ev.ClaimId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(ev => ev.Employee)
                .WithMany()
                .HasForeignKey(ev => ev.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(ev => new { ev.WarrantyId, ev.OccurredAt });
            e.HasQueryFilter(ev => (ev.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !ev.IsDeleted);
        });

        // Commission Scheme
        builder.Entity<CommissionScheme>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(255).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.CommissionType).HasMaxLength(30).IsRequired();
            e.Property(x => x.CalculationMethod).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        // Commission Rule
        builder.Entity<CommissionRule>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ConditionType).HasMaxLength(50).IsRequired();
            e.Property(x => x.ConditionOperator).HasMaxLength(20).IsRequired();
            e.Property(x => x.CalculationType).HasMaxLength(20).IsRequired();
            e.Property(x => x.ApplyOn).HasMaxLength(20).IsRequired();
            e.HasOne(x => x.CommissionScheme)
                .WithMany(s => s.Rules)
                .HasForeignKey(x => x.CommissionSchemeId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        // Commission Assignment
        builder.Entity<CommissionAssignment>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.CommissionScheme)
                .WithMany(s => s.Assignments)
                .HasForeignKey(x => x.CommissionSchemeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Employee)
                .WithMany(emp => emp.CommissionAssignments)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.EmployeeId, x.CommissionSchemeId }).IsUnique();
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        // Goal Definition
        builder.Entity<GoalDefinition>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(255).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.GoalType).HasMaxLength(30).IsRequired();
            e.Property(x => x.MetricType).HasMaxLength(20).IsRequired();
            e.Property(x => x.Frequency).HasMaxLength(20).IsRequired();
            e.Property(x => x.DataSource).HasMaxLength(50).IsRequired();
            e.Property(x => x.CalculationFormula).HasMaxLength(500);
            e.Property(x => x.GateDescription).HasMaxLength(500);
            e.Property(x => x.GateFormula).HasMaxLength(500);
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<GoalAssignment>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Status).HasMaxLength(20).IsRequired();
            e.HasOne(a => a.GoalDefinition)
                .WithMany(g => g.Assignments)
                .HasForeignKey(a => a.GoalDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Employee)
                .WithMany(emp => emp.GoalAssignments)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(a => new { a.GoalDefinitionId, a.EmployeeId });
            e.HasQueryFilter(a => (a.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !a.IsDeleted);
        });

        builder.Entity<GoalProgress>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.PeriodKey).HasMaxLength(20);
            e.HasOne(p => p.GoalAssignment)
                .WithMany(a => a.ProgressEntries)
                .HasForeignKey(p => p.GoalAssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(p => new { p.GoalAssignmentId, p.EvaluationDate });
            e.HasQueryFilter(p => (p.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !p.IsDeleted);
        });

        builder.Entity<Incentive>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.Name).HasMaxLength(100).IsRequired();
            e.Property(i => i.IncentiveType).HasMaxLength(20).IsRequired();
            e.Property(i => i.Currency).HasMaxLength(3).IsRequired();
            e.Property(i => i.PaymentTrigger).HasMaxLength(20).IsRequired();
            e.Property(i => i.Status).HasMaxLength(20).IsRequired();
            e.HasOne(i => i.GoalDefinition)
                .WithMany()
                .HasForeignKey(i => i.GoalDefinitionId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(i => (i.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !i.IsDeleted);
        });

        builder.Entity<IncentivePayment>(e =>
        {
            e.HasKey(ip => ip.Id);
            e.Property(ip => ip.Status).HasMaxLength(20).IsRequired();
            e.HasOne(ip => ip.Incentive)
                .WithMany()
                .HasForeignKey(ip => ip.IncentiveId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ip => ip.GoalAssignment)
                .WithMany(a => a.IncentivePayments)
                .HasForeignKey(ip => ip.GoalAssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ip => ip.Employee)
                .WithMany(emp => emp.IncentivePayments)
                .HasForeignKey(ip => ip.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(ip => new { ip.GoalAssignmentId, ip.EmployeeId });
            e.HasQueryFilter(ip => (ip.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !ip.IsDeleted);
        });

        builder.Entity<RegionalTaxConfiguration>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.CountryCode).HasMaxLength(3).IsRequired();
            e.Property(x => x.TaxType).HasMaxLength(50).IsRequired();
            e.Property(x => x.Rate).HasColumnType("decimal(18,4)").IsRequired();
            e.HasIndex(x => new { x.CountryCode, x.TaxType, x.EffectiveDate, x.CompanyId });
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<PayrollConcept>(e =>
        {
            e.HasKey(pc => pc.Id);
            e.Property(pc => pc.CountryCode).HasMaxLength(3).IsRequired();
            e.Property(pc => pc.Code).HasMaxLength(50).IsRequired();
            e.Property(pc => pc.Name).HasMaxLength(255).IsRequired();
            e.Property(pc => pc.CalculationFormula).HasMaxLength(500).IsRequired();
            e.HasOne(pc => pc.AccountMapping).WithMany().HasForeignKey(pc => pc.AccountMappingId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(pc => new { pc.CountryCode, pc.Code, pc.CompanyId }).IsUnique();
            e.HasQueryFilter(pc => (pc.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !pc.IsDeleted);
        });

        builder.Entity<AccountingRuleTemplate>(e =>
        {
            e.HasKey(ar => ar.Id);
            e.Property(ar => ar.CountryCode).HasMaxLength(3).IsRequired();
            e.Property(ar => ar.ProcessTrigger).HasMaxLength(100).IsRequired();
            e.Property(ar => ar.EntryStructureJson).IsRequired();
            e.HasIndex(ar => new { ar.CountryCode, ar.ProcessTrigger, ar.CompanyId });
            e.HasQueryFilter(ar => (ar.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !ar.IsDeleted);
        });

        builder.Entity<WarrantyAttachment>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.FileName).HasMaxLength(255).IsRequired();
            e.Property(a => a.FileUrl).HasMaxLength(500).IsRequired();
            e.Property(a => a.FileType).HasMaxLength(100).IsRequired();
            e.Property(a => a.Category).HasMaxLength(50).IsRequired();
            e.Property(a => a.Description).HasMaxLength(500);
            e.HasOne(a => a.Warranty)
                .WithMany()
                .HasForeignKey(a => a.WarrantyId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Claim)
                .WithMany()
                .HasForeignKey(a => a.ClaimId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(a => a.UploadedBy)
                .WithMany()
                .HasForeignKey(a => a.UploadedByEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(a => (a.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !a.IsDeleted);
        });

        builder.Entity<WarrantyStateHistory>(e =>
        {
            e.HasKey(h => h.Id);
            e.Property(h => h.FromStatus).HasMaxLength(30);
            e.Property(h => h.ToStatus).HasMaxLength(30).IsRequired();
            e.Property(h => h.Reason).HasMaxLength(1000);
            e.HasOne(h => h.Warranty)
                .WithMany()
                .HasForeignKey(h => h.WarrantyId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(h => h.Claim)
                .WithMany()
                .HasForeignKey(h => h.ClaimId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(h => h.ChangedBy)
                .WithMany()
                .HasForeignKey(h => h.ChangedByEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(h => new { h.WarrantyId, h.ChangedAt });
            e.HasQueryFilter(h => (h.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !h.IsDeleted);
        });

        // ---- FacturaciÃÂÃÂ³n ElectrÃÂÃÂ³nica ----
        builder.Entity<ElectronicInvoice>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.CountryCode).HasMaxLength(3).IsRequired();
            e.Property(x => x.InvoiceNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.AuthorizationCode).HasMaxLength(100);
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.HasOne(x => x.Sale).WithMany().HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.InvoiceNumber);
            e.HasIndex(x => new { x.SaleId, x.CountryCode });
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<ElectronicInvoiceXml>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.XmlType).HasMaxLength(50).IsRequired();
            e.HasOne(x => x.ElectronicInvoice).WithMany().HasForeignKey(x => x.ElectronicInvoiceId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        // ---- Partner Configuration ----
        builder.Entity<Partner>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Code).HasMaxLength(50).IsRequired();
            e.Property(p => p.Name).HasMaxLength(255).IsRequired();
            e.Property(p => p.LegalName).HasMaxLength(255).IsRequired();
            e.Property(p => p.TaxId).HasMaxLength(50).IsRequired();
            e.Property(p => p.PartnerType).HasMaxLength(30).IsRequired();
            e.Property(p => p.Email).HasMaxLength(255);
            e.Property(p => p.Phone).HasMaxLength(20);
            e.Property(p => p.Address).HasMaxLength(500);
            e.Property(p => p.CountryCode).HasMaxLength(3).IsRequired();
            e.Property(p => p.City).HasMaxLength(100);
            e.Property(p => p.Status).HasMaxLength(20).IsRequired();
            e.Property(p => p.ContactName).HasMaxLength(255);
            e.Property(p => p.ContactEmail).HasMaxLength(255);
            e.Property(p => p.ContactPhone).HasMaxLength(20);
            e.Property(p => p.CommissionRate).HasMaxLength(20);
            e.Property(p => p.ContractUrl).HasMaxLength(500);
            e.Property(p => p.RevenueGenerated).HasColumnType("decimal(18,2)");
            e.HasIndex(p => p.Code).IsUnique();
            e.HasIndex(p => new { p.CountryCode, p.Status });
            e.HasQueryFilter(p => (p.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !p.IsDeleted);
        });

                // ---- New Module: Prestadores de Servicios ----
        builder.Entity<ServiceProvider>(e =>
        {
            e.HasKey(sp => sp.Id);
            e.Property(sp => sp.BusinessName).HasMaxLength(255).IsRequired();
            e.Property(sp => sp.CountryCode).HasMaxLength(3).IsRequired();
            e.Property(sp => sp.ServiceCategory).HasMaxLength(100).IsRequired();
            e.Property(sp => sp.Status).HasMaxLength(20).IsRequired();
            e.HasOne(sp => sp.Employee).WithOne(em => em.ServiceProviderDetails).HasForeignKey<ServiceProvider>(sp => sp.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(sp => (sp.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !sp.IsDeleted);
        });

        builder.Entity<ServiceContract>(e =>
        {
            e.HasKey(sc => sc.Id);
            e.Property(sc => sc.ContractNumber).HasMaxLength(50).IsRequired();
            e.Property(sc => sc.ContractName).HasMaxLength(255).IsRequired();
            e.Property(sc => sc.Currency).HasMaxLength(3).IsRequired();
            e.Property(sc => sc.CountryCode).HasMaxLength(3).IsRequired();
            e.Property(sc => sc.Status).HasMaxLength(20).IsRequired();
            e.Property(sc => sc.TotalContractAmount).HasColumnType("decimal(18,2)");
            e.HasOne(sc => sc.ServiceProvider).WithMany(sp => sp.Contracts).HasForeignKey(sc => sc.ServiceProviderId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(sc => (sc.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !sc.IsDeleted);
        });

// ---- Motor Documental Configuration ----
        builder.Entity<DocumentTemplate>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Name).HasMaxLength(255).IsRequired();
            e.Property(t => t.Category).HasMaxLength(50).IsRequired();
            e.Property(t => t.CountryCode).HasMaxLength(10).IsRequired();
            e.Property(t => t.Module).HasMaxLength(50);
            e.HasQueryFilter(t => (t.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !t.IsDeleted);
        });

        builder.Entity<GeneratedDocument>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Name).HasMaxLength(255).IsRequired();
            e.Property(d => d.EntityType).HasMaxLength(100).IsRequired();
            e.Property(d => d.Status).HasMaxLength(30).IsRequired();
            e.HasOne(d => d.Template).WithMany(t => t.GeneratedDocuments).HasForeignKey(d => d.TemplateId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(d => (d.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !d.IsDeleted);
        });

        builder.Entity<DocumentVersion>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.FilePath).HasMaxLength(500).IsRequired();
            e.Property(v => v.FileHash).HasMaxLength(256);
            e.Property(v => v.ChangesSummary).HasMaxLength(1000);
            e.HasOne(v => v.Document).WithMany(d => d.Versions).HasForeignKey(v => v.DocumentId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(v => (v.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !v.IsDeleted);
        });

        builder.Entity<DocumentSignature>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.SignerType).HasMaxLength(30).IsRequired();
            e.Property(s => s.SignerRole).HasMaxLength(50).IsRequired();
            e.Property(s => s.Status).HasMaxLength(30).IsRequired();
            e.Property(s => s.IPAddress).HasMaxLength(50);
            e.Property(s => s.SignatureToken).HasMaxLength(255);
            e.HasOne(s => s.Document).WithMany(d => d.Signatures).HasForeignKey(s => s.DocumentId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(s => (s.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !s.IsDeleted);
        });

        // ÃÂ¢Ã¢ÂÂÃ¢ÂÂ¬ÃÂ¢Ã¢ÂÂÃ¢ÂÂ¬ Fleet Module ÃÂ¢Ã¢ÂÂÃ¢ÂÂ¬ÃÂ¢Ã¢ÂÂÃ¢ÂÂ¬
        builder.Entity<Zorvian.Core.Entities.Fleet.VehicleBrand>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.VehicleType>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.FuelType>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.DriverLicenseCategory>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.Vehicle>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Plate).HasMaxLength(15).IsRequired();
            e.Property(x => x.Model).HasMaxLength(50).IsRequired();
            e.Property(x => x.Vin).HasMaxLength(17);
            e.Property(x => x.EngineNumber).HasMaxLength(30);
            e.Property(x => x.ChassisNumber).HasMaxLength(30);
            e.Property(x => x.Color).HasMaxLength(30);
            e.Property(x => x.Status).HasMaxLength(30).IsRequired();
            e.Property(x => x.GpsDeviceId).HasMaxLength(50);
            e.Property(x => x.CurrentKm).HasColumnType("decimal(10,2)");
            e.Property(x => x.PreviousKm).HasColumnType("decimal(10,2)");
            e.Property(x => x.HourMeter).HasColumnType("decimal(10,2)");
            e.Property(x => x.LoadCapacityKg).HasColumnType("decimal(10,2)");
            e.Property(x => x.LoadCapacityM3).HasColumnType("decimal(10,2)");
            e.Property(x => x.PurchaseValue).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.VehicleType).WithMany().HasForeignKey(x => x.VehicleTypeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.FuelType).WithMany().HasForeignKey(x => x.FuelTypeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Driver).WithMany().HasForeignKey(x => x.DriverId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => new { x.Code, x.TenantId }).IsUnique();
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.Driver>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            e.Property(x => x.IdDocument).HasMaxLength(20).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(20).IsRequired();
            e.Property(x => x.Email).HasMaxLength(100).IsRequired();
            e.Property(x => x.Address).HasMaxLength(200);
            e.Property(x => x.LicenseNumber).HasMaxLength(30).IsRequired();
            e.Property(x => x.AdditionalCategories).HasMaxLength(100);
            e.Property(x => x.Status).HasMaxLength(30).IsRequired();
            e.Property(x => x.PhotoUrl).HasMaxLength(500);
            e.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.LicenseCategory).WithMany().HasForeignKey(x => x.LicenseCategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.IdDocument, x.TenantId }).IsUnique();
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.Route>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Type).HasMaxLength(30).IsRequired();
            e.Property(x => x.OriginAddress).HasMaxLength(200).IsRequired();
            e.Property(x => x.DestinationAddress).HasMaxLength(200);
            e.Property(x => x.Status).HasMaxLength(30).IsRequired();
            e.Property(x => x.DistanceEstKm).HasColumnType("decimal(10,2)");
            e.Property(x => x.CostEst).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Vehicle).WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Driver).WithMany().HasForeignKey(x => x.DriverId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.CoDriver).WithMany().HasForeignKey(x => x.CoDriverId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.RoutePoint>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).HasMaxLength(30).IsRequired();
            e.Property(x => x.Address).HasMaxLength(200).IsRequired();
            e.Property(x => x.Instructions).HasMaxLength(500);
            e.Property(x => x.DistanceFromPreviousKm).HasColumnType("decimal(10,2)");
            e.HasOne(x => x.Route).WithMany(r => r.Points).HasForeignKey(x => x.RouteId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Client).WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Sale).WithMany().HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.Delivery>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.DeliveryAddress).HasMaxLength(200).IsRequired();
            e.Property(x => x.Status).HasMaxLength(30).IsRequired();
            e.Property(x => x.ReceiverName).HasMaxLength(100);
            e.Property(x => x.ReceiverId).HasMaxLength(20);
            e.Property(x => x.SignatureUrl).HasMaxLength(500);
            e.Property(x => x.Observations).HasMaxLength(1000);
            e.Property(x => x.DocumentUrl).HasMaxLength(500);
            e.HasOne(x => x.Sale).WithMany().HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Client).WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Route).WithMany().HasForeignKey(x => x.RouteId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Vehicle).WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Driver).WithMany().HasForeignKey(x => x.DriverId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.DeliveryItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.QtyOrdered).HasColumnType("decimal(10,2)");
            e.Property(x => x.QtyDelivered).HasColumnType("decimal(10,2)");
            e.Property(x => x.QtyReturned).HasColumnType("decimal(10,2)");
            e.Property(x => x.LotSerial).HasMaxLength(50);
            e.Property(x => x.Status).HasMaxLength(30).IsRequired();
            e.HasOne(x => x.Delivery).WithMany(d => d.Items).HasForeignKey(x => x.DeliveryId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.Trip>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Origin).HasMaxLength(200).IsRequired();
            e.Property(x => x.Destination).HasMaxLength(200).IsRequired();
            e.Property(x => x.Status).HasMaxLength(30).IsRequired();
            e.Property(x => x.StartKm).HasColumnType("decimal(10,2)");
            e.Property(x => x.EndKm).HasColumnType("decimal(10,2)");
            e.Property(x => x.TotalKm).HasColumnType("decimal(10,2)");
            e.Property(x => x.Notes).HasMaxLength(500);
            e.HasOne(x => x.Vehicle).WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Driver).WithMany().HasForeignKey(x => x.DriverId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CoDriver).WithMany().HasForeignKey(x => x.CoDriverId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.FuelRefill>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Liters).HasColumnType("decimal(10,2)").IsRequired();
            e.Property(x => x.PricePerLiter).HasColumnType("decimal(10,4)").IsRequired();
            e.Property(x => x.TotalCost).HasColumnType("decimal(12,2)").IsRequired();
            e.Property(x => x.CurrentKm).HasColumnType("decimal(10,2)");
            e.Property(x => x.HourMeter).HasColumnType("decimal(10,2)");
            e.Property(x => x.RefillType).HasMaxLength(20).IsRequired();
            e.Property(x => x.PaymentMethod).HasMaxLength(20).IsRequired();
            e.Property(x => x.InvoiceUrl).HasMaxLength(500);
            e.Property(x => x.Observations).HasMaxLength(500);
            e.Property(x => x.AnomalyNotes).HasMaxLength(500);
            e.HasOne(x => x.Vehicle).WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Driver).WithMany().HasForeignKey(x => x.DriverId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.FuelType).WithMany().HasForeignKey(x => x.FuelTypeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.MaintenanceSchedule>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ScheduleType).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.HasOne(x => x.Vehicle).WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Template).WithMany().HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.MaintenanceTemplate>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.ApplicableVehicleTypes).HasMaxLength(200);
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.WorkOrder>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Number).HasMaxLength(20).IsRequired();
            e.Property(x => x.Priority).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasMaxLength(30).IsRequired();
            e.Property(x => x.ProblemDescription).HasMaxLength(2000);
            e.Property(x => x.Diagnosis).HasMaxLength(2000);
            e.Property(x => x.RootCause).HasMaxLength(2000);
            e.Property(x => x.SolutionApplied).HasMaxLength(2000);
            e.Property(x => x.MechanicResponsible).HasMaxLength(100);
            e.Property(x => x.CostEst).HasColumnType("decimal(18,2)");
            e.Property(x => x.CostLabor).HasColumnType("decimal(18,2)");
            e.Property(x => x.CostParts).HasColumnType("decimal(18,2)");
            e.Property(x => x.CostTotal).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Vehicle).WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Driver).WithMany().HasForeignKey(x => x.DriverId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.FailureType).WithMany().HasForeignKey(x => x.FailureTypeId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Workshop).WithMany().HasForeignKey(x => x.WorkshopId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => new { x.Number, x.TenantId }).IsUnique();
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.WorkOrderPart>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Quantity).HasColumnType("decimal(10,2)");
            e.Property(x => x.UnitCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.SupplierCode).HasMaxLength(50);
            e.HasOne(x => x.WorkOrder).WithMany().HasForeignKey(x => x.WorkOrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.FailureType>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.Workshop>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.ContactPerson).HasMaxLength(100);
            e.Property(x => x.Phone).HasMaxLength(20).IsRequired();
            e.Property(x => x.Email).HasMaxLength(100);
            e.Property(x => x.Address).HasMaxLength(200);
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.FleetDocument>(e =>
        {
            e.ToTable("FleetDocuments");
            e.HasKey(x => x.Id);
            e.Property(x => x.EntityType).HasMaxLength(20).IsRequired();
            e.Property(x => x.DocumentNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.FileUrl).HasMaxLength(500);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.Status).HasMaxLength(30).IsRequired();
            e.HasOne(x => x.DocumentType).WithMany().HasForeignKey(x => x.DocumentTypeId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.DocumentType>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.EntityType).HasMaxLength(20).IsRequired();
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.FleetExpense>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Description).HasMaxLength(200).IsRequired();
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            e.Property(x => x.ExchangeRate).HasColumnType("decimal(10,4)");
            e.Property(x => x.AmountBaseCurrency).HasColumnType("decimal(18,2)");
            e.Property(x => x.PaymentMethod).HasMaxLength(20).IsRequired();
            e.Property(x => x.DocumentUrl).HasMaxLength(500);
            e.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Subcategory).WithMany().HasForeignKey(x => x.SubcategoryId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Vehicle).WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Driver).WithMany().HasForeignKey(x => x.DriverId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Trip).WithMany().HasForeignKey(x => x.TripId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Route).WithMany().HasForeignKey(x => x.RouteId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.ExpenseCategory>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.ExpenseSubcategory>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.GpsPosition>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Latitude).HasColumnType("double precision").IsRequired();
            e.Property(x => x.Longitude).HasColumnType("double precision").IsRequired();
            e.Property(x => x.Altitude).HasColumnType("double precision");
            e.Property(x => x.Speed).HasColumnType("double precision");
            e.Property(x => x.Odometer).HasColumnType("decimal(10,2)");
            e.Property(x => x.FuelLevel).HasColumnType("decimal(5,2)");
            e.Property(x => x.Temperature).HasColumnType("decimal(5,2)");
            e.Property(x => x.DeviceBattery).HasColumnType("decimal(5,2)");
            e.HasOne(x => x.Vehicle).WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.VehicleId, x.GpsTimestamp });
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.Geofence>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Type).HasMaxLength(20).IsRequired();
            e.Property(x => x.CoordinatesJson).IsRequired();
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.DriverInfraction>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.FineAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.HasOne(x => x.Driver).WithMany().HasForeignKey(x => x.DriverId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<Zorvian.Core.Entities.Fleet.DriverTraining>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.CourseName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Institution).HasMaxLength(200);
            e.HasOne(x => x.Driver).WithMany().HasForeignKey(x => x.DriverId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => (x.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !x.IsDeleted);
        });

        builder.Entity<UserTenant>(e =>
        {
            e.HasKey(ut => new { ut.UserId, ut.TenantId });
            e.Property(ut => ut.TenantId).HasMaxLength(50).IsRequired();
            e.HasOne(ut => ut.User)
                .WithMany()
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Fleet: Geofence State Tracking ----
        builder.Entity<Zorvian.Core.Entities.Fleet.VehicleGeofenceState>(e =>
        {
            e.HasKey(gs => gs.Id);
            e.HasOne(gs => gs.Vehicle)
                .WithMany()
                .HasForeignKey(gs => gs.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(gs => gs.Geofence)
                .WithMany()
                .HasForeignKey(gs => gs.GeofenceId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(gs => new { gs.VehicleId, gs.GeofenceId, gs.IsInside });
            e.HasQueryFilter(gs => (gs.TenantId == _tenantContext.TenantId.ToString() || _tenantContext.IsSuperAdmin) && !gs.IsDeleted);
        });
    }

    public override int SaveChanges()
    {
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }


}


