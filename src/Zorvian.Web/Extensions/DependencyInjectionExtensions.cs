using Zorvian.Application.Interfaces;
using Zorvian.Application.Jobs;
using Zorvian.Application.Services;
using Zorvian.Application.Services.CommissionEngine;
using Zorvian.Application.Services.DepreciationCalculators;
using Zorvian.Application.Services.PayrollStrategies;
using Zorvian.Core.Interfaces;
using Zorvian.Infrastructure.Data;
using Zorvian.Infrastructure.Identity;
using Zorvian.Infrastructure.Jobs;
using Zorvian.Infrastructure.Repositories;
using Zorvian.Infrastructure.Services;
using Zorvian.Web.Jobs;
using Zorvian.Web.Services;

namespace Zorvian.Web.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddZorvianInfrastructure(this IServiceCollection services)
    {
        // Tenant & Interceptors
        services.AddSingleton<TenantContext>();
        services.AddSingleton<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.AddSingleton<ITenantContextWriter>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<Zorvian.Infrastructure.Data.AuditInterceptor>();
        services.AddSingleton<Zorvian.Infrastructure.Data.AuditImmutabilityInterceptor>();
        services.AddSingleton<Zorvian.Infrastructure.Data.EntityHistoryInterceptor>();
        services.AddSingleton<Zorvian.Infrastructure.Data.TenantSessionInterceptor>();

        // Identity
        services.AddSingleton<IFirebaseAuthService, FirebaseAuthService>();
        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IEncryptionService, EncryptionService>();

        // Infrastructure Services
        services.AddScoped<IExcelImportService, ExcelImportService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IReportExportService, ReportExportService>();
        services.AddScoped<IReconciliationService, Zorvian.Infrastructure.Services.ReconciliationService>();
        services.AddScoped<ITreasuryDashboardService, TreasuryDashboardService>();
        services.AddScoped<IAchExportService, AchExportService>();
        services.AddScoped<IWebhookService, WebhookService>();
        services.AddScoped<IJobScheduler, HangfireJobScheduler>();
        services.AddScoped<IOcrService, OcrService>();
        services.AddScoped<IMfaService, Zorvian.Infrastructure.Services.MfaService>();
        services.AddScoped<IChatService>(sp => new ChatService(
            sp.GetRequiredService<Zorvian.Infrastructure.Data.ZorvianDbContext>(),
            sp.GetRequiredService<IEmbeddingService>(),
            sp.GetRequiredService<IConfiguration>()["GoogleAi:ProjectId"]!,
            sp.GetRequiredService<IConfiguration>()["GoogleAi:Location"]!));
        services.AddScoped<IEmbeddingService>(sp => new EmbeddingService(
            sp.GetRequiredService<IConfiguration>()["GoogleAi:ProjectId"]!,
            sp.GetRequiredService<IConfiguration>()["GoogleAi:Location"]!,
            "text-embedding-004"));
        services.AddHttpClient();

        services.AddScoped<IDocumentStorageService>(sp =>
        {
            var bucket = sp.GetRequiredService<IConfiguration>()["Firebase:StorageBucket"]
                ?? "zorvian-erp.firebasestorage.app";
            return new FirebaseStorageService(bucket);
        });

        services.AddScoped<QuestPdfService>();
        services.AddScoped<IEmailService, EmailService>();

        // File Cleanup
        services.AddScoped<IOrphanFileCleanupService, OrphanFileCleanupService>();
        services.AddScoped<FileCleanupSaveChangesInterceptor>();

        return services;
    }

    public static IServiceCollection AddZorvianRepositories(this IServiceCollection services)
    {
        // Collaborators
        services.AddScoped<ICollaboratorRepository, CollaboratorRepository>();
        services.AddScoped<ICollaboratorService, CollaboratorService>();

        // Badges
        services.AddScoped<IBadgeRepository, BadgeRepository>();

        // Auth
        services.AddScoped<IAuthRepository, AuthRepository>();

        // Core
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IEntityHistoryRepository, EntityHistoryRepository>();
        services.AddScoped<IPayrollRepository, PayrollRepository>();
        services.AddScoped<IPayrollConceptRepository, PayrollConceptRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<ISickLeaveRepository, SickLeaveRepository>();
        services.AddScoped<ITerminationRepository, TerminationRepository>();
        services.AddScoped<IEmployeeLoanRepository, EmployeeLoanRepository>();
        services.AddScoped<ISalaryAdvanceRepository, SalaryAdvanceRepository>();
        services.AddScoped<IWageGarnishmentRepository, WageGarnishmentRepository>();
        services.AddScoped<IBenefitProvisionRepository, BenefitProvisionRepository>();
        services.AddScoped<IOvertimeRecordRepository, OvertimeRecordRepository>();
        services.AddScoped<ICommissionRecordRepository, CommissionRecordRepository>();
        services.AddScoped<IBonusRecordRepository, BonusRecordRepository>();
        services.AddScoped<ICountryTaxConfigRepository, CountryTaxConfigRepository>();
        services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        services.AddScoped<IVacationRepository, VacationRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();

        // Documentary Engine
        services.AddScoped<IDocumentTemplateRepository, DocumentTemplateRepository>();
        services.AddScoped<IGeneratedDocumentRepository, GeneratedDocumentRepository>();
        services.AddScoped<IDocumentSignatureRepository, DocumentSignatureRepository>();
        services.AddScoped<ISignatureService, SignatureService>();

        // Incentives & Goals
        services.AddScoped<ICommissionRepository, CommissionRepository>();
        services.AddScoped<IGoalRepository, GoalRepository>();
        services.AddScoped<IKpiRepository, KpiRepository>();
        services.AddScoped<IProviderRepository, ProviderRepository>();

        // Branches
        services.AddScoped<IBranchRepository, BranchRepository>();

        // Commercial
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IQuoteRepository, QuoteRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        
        // CRM
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IOpportunityRepository, OpportunityRepository>();

        // Inventory
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ITaxCategoryRepository, TaxCategoryRepository>();
        services.AddScoped<IInventoryMovementRepository, InventoryMovementRepository>();

        // Purchases
        services.AddScoped<IPurchaseRepository, PurchaseRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<PurchaseOrderService>();
        services.AddScoped<ISupplierPaymentRepository, SupplierPaymentRepository>();
        services.AddScoped<ISupplierCreditNoteRepository, SupplierCreditNoteRepository>();
        services.AddScoped<IWithholdingRepository, WithholdingRepository>();

        // Credits
        services.AddScoped<ICreditRepository, CreditRepository>();
        services.AddScoped<ICreditPaymentRepository, CreditPaymentRepository>();
        services.AddScoped<ILateFeeRepository, LateFeeRepository>();
        services.AddScoped<ICollectionActionRepository, CollectionActionRepository>();
        services.AddScoped<ICreditRefinancingRepository, CreditRefinancingRepository>();

        // Cash
        services.AddScoped<ICashRegisterRepository, CashRegisterRepository>();
        services.AddScoped<ICashMovementRepository, CashMovementRepository>();
        services.AddScoped<ICashRegisterArqueoRepository, CashRegisterArqueoRepository>();

        // Treasury
        services.AddScoped<ICheckRepository, CheckRepository>();
        services.AddScoped<ICheckbookRepository, CheckbookRepository>();
        services.AddScoped<ICheckPrintTemplateRepository, CheckPrintTemplateRepository>();
        services.AddScoped<ICheckAuditTrailRepository, CheckAuditTrailRepository>();

        // Fixed Assets
        services.AddScoped<IFixedAssetRepository, FixedAssetRepository>();
        services.AddScoped<IFixedAssetCategoryRepository, FixedAssetCategoryRepository>();
        services.AddScoped<IDepreciationEntryRepository, DepreciationEntryRepository>();
        services.AddScoped<IAssetRevaluationRepository, AssetRevaluationRepository>();
        services.AddScoped<IAssetDisposalRepository, AssetDisposalRepository>();
        services.AddScoped<IAssetMaintenanceRepository, AssetMaintenanceRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();

        // Exchange Rates
        services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
        services.AddScoped<ICustomReportRepository, CustomReportRepository>();

        // Warranty
        services.AddScoped<IWarrantyRepository, WarrantyRepository>();
        services.AddScoped<IWarrantySlaConfigRepository, WarrantySlaConfigRepository>();
        services.AddScoped<IWarrantyProviderRepository, WarrantyProviderRepository>();
        services.AddScoped<IWarrantyCostRepository, WarrantyCostRepository>();
        services.AddScoped<IWarrantyPartRequestRepository, WarrantyPartRequestRepository>();
        services.AddScoped<IWarrantyCommunicationRepository, WarrantyCommunicationRepository>();
        services.AddScoped<IWarrantyEventRepository, WarrantyEventRepository>();
        services.AddScoped<IWarrantyAttachmentRepository, WarrantyAttachmentRepository>();
        services.AddScoped<IWarrantyStateHistoryRepository, WarrantyStateHistoryRepository>();
        services.AddScoped<IServiceWorkshopRepository, ServiceWorkshopRepository>();
        services.AddScoped<IWarrantyPartUsageRepository, WarrantyPartUsageRepository>();

        // Electronic Invoicing & Partners
        services.AddScoped<IPartnerRepository, PartnerRepository>();
        services.AddScoped<IElectronicInvoiceRepository, ElectronicInvoiceRepository>();

        // Fleet
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IVehicleBrandRepository, Zorvian.Infrastructure.Repositories.Fleet.VehicleBrandRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IVehicleTypeRepository, Zorvian.Infrastructure.Repositories.Fleet.VehicleTypeRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IFuelTypeRepository, Zorvian.Infrastructure.Repositories.Fleet.FuelTypeRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IDriverLicenseCategoryRepository, Zorvian.Infrastructure.Repositories.Fleet.DriverLicenseCategoryRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IVehicleRepository, Zorvian.Infrastructure.Repositories.Fleet.VehicleRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IDriverRepository, Zorvian.Infrastructure.Repositories.Fleet.DriverRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IFleetDocumentRepository, Zorvian.Infrastructure.Repositories.Fleet.FleetDocumentRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IDocumentTypeRepository, Zorvian.Infrastructure.Repositories.Fleet.DocumentTypeRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IRouteRepository, Zorvian.Infrastructure.Repositories.Fleet.RouteRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IDeliveryRepository, Zorvian.Infrastructure.Repositories.Fleet.DeliveryRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.ITripRepository, Zorvian.Infrastructure.Repositories.Fleet.TripRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IFuelRefillRepository, Zorvian.Infrastructure.Repositories.Fleet.FuelRefillRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IWorkOrderRepository, Zorvian.Infrastructure.Repositories.Fleet.WorkOrderRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IMaintenanceScheduleRepository, Zorvian.Infrastructure.Repositories.Fleet.MaintenanceScheduleRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IWorkshopRepository, Zorvian.Infrastructure.Repositories.Fleet.WorkshopRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IFailureTypeRepository, Zorvian.Infrastructure.Repositories.Fleet.FailureTypeRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IMaintenanceTemplateRepository, Zorvian.Infrastructure.Repositories.Fleet.MaintenanceTemplateRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IExpenseCategoryRepository, Zorvian.Infrastructure.Repositories.Fleet.ExpenseCategoryRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IExpenseSubcategoryRepository, Zorvian.Infrastructure.Repositories.Fleet.ExpenseSubcategoryRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IFleetExpenseRepository, Zorvian.Infrastructure.Repositories.Fleet.FleetExpenseRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IGpsPositionRepository, Zorvian.Infrastructure.Repositories.Fleet.GpsPositionRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IGeofenceRepository, Zorvian.Infrastructure.Repositories.Fleet.GeofenceRepository>();
        services.AddScoped<Zorvian.Application.Interfaces.Fleet.IGeofenceStateRepository, Zorvian.Infrastructure.Repositories.Fleet.GeofenceStateRepository>();

        // Accounting
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAccountingEntryRepository, AccountingEntryRepository>();
        services.AddScoped<IAccountingPeriodRepository, AccountingPeriodRepository>();
        services.AddScoped<IFiscalYearRepository, FiscalYearRepository>();
        services.AddScoped<IAccountLinkRepository, AccountLinkRepository>();
        services.AddScoped<IAccountingRuleRepository, AccountingRuleRepository>();
        services.AddScoped<ICostCenterRepository, CostCenterRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IBudgetDetailRepository, BudgetDetailRepository>();
        services.AddScoped<IBudgetTrackingRepository, BudgetTrackingRepository>();
        services.AddScoped<IReconciliationRepository, ReconciliationRepository>();
        services.AddScoped<ICreditNoteRepository, CreditNoteRepository>();
        services.AddScoped<IApprovalFlowConfigRepository, ApprovalFlowConfigRepository>();
        services.AddScoped<IApprovalRequestRepository, ApprovalRequestRepository>();

        return services;
    }

    public static IServiceCollection AddZorvianApplicationServices(this IServiceCollection services)
    {
        // Core Services
        services.AddScoped<IBadgeService, BadgeService>();
        services.AddScoped<AuthService>();
        services.AddScoped<SeedService>(sp => new SeedService(
            sp.GetRequiredService<ZorvianDbContext>(),
            sp.GetRequiredService<IFirebaseAuthService>(),
            sp.GetRequiredService<IFiscalService>(),
            sp.GetRequiredService<AccountService>(),
            sp.GetRequiredService<AccountLinkService>(),
            sp.GetRequiredService<IAccountingRuleTemplateRepository>()));
        services.AddScoped<CompanyService>();
        services.AddScoped<EmployeeService>();
        services.AddScoped<DepartmentService>();
        services.AddScoped<DashboardService>();
        services.AddScoped<AttendanceService>();

        // HR
        services.AddScoped<VacationService>();
        services.AddScoped<PermissionService>();
        services.AddScoped<IPolicyService, PolicyService>();
        services.AddScoped<PerformanceService>();
        services.AddScoped<IVacationRecommendationService, VacationRecommendationService>();

        // Commissions Engine
        services.AddScoped<RuleEvaluator>();
        services.AddScoped<CommissionCalculator>();
        services.AddScoped<ICommissionDataSource, CommissionDataSource>();
        services.AddScoped<CommissionEngine>();

        // Commissions, Goals, KPIs, Providers
        services.AddScoped<CommissionService>();
        services.AddScoped<ICommissionService>(sp => sp.GetRequiredService<CommissionService>());
        services.AddScoped<GoalService>();
        services.AddScoped<IGoalIntegrationService, GoalIntegrationService>();
        services.AddScoped<GoalIntegrationService>(); // Concrete registration for Hangfire jobs
        services.AddScoped<KpiService>();
        services.AddScoped<ProviderService>();

        // Payroll
        services.AddScoped<IPayrollCalculationStrategy, NicaraguaCalculationStrategy>();
        services.AddScoped<IPayrollCalculationStrategy, HondurasCalculationStrategy>();
        services.AddScoped<IPayrollCalculationStrategy, ElSalvadorCalculationStrategy>();
        services.AddScoped<IPayrollCalculationStrategy, GuatemalaCalculationStrategy>();
        services.AddScoped<IPayrollCalculationStrategy, CostaRicaCalculationStrategy>();
        services.AddScoped<IPayrollCalculationStrategy, PanamaCalculationStrategy>();
        services.AddScoped<PayrollCalculationFactory>();
        services.AddScoped<PayrollService>(sp => new PayrollService(
            sp.GetRequiredService<IPayrollRepository>(),
            sp.GetRequiredService<IEmployeeRepository>(),
            sp.GetRequiredService<ITenantContext>(),
            sp.GetRequiredService<IWebhookService>(),
            sp.GetRequiredService<IAchExportService>(),
            sp.GetRequiredService<AutoAccountingService>(),
            sp.GetRequiredService<ICountryTaxConfigRepository>(),
            sp.GetRequiredService<ICompanyRepository>(),
            sp.GetRequiredService<IOvertimeRecordRepository>(),
            sp.GetRequiredService<ICommissionRecordRepository>(),
            sp.GetRequiredService<IBonusRecordRepository>(),
            sp.GetRequiredService<IPayrollConceptRepository>(),
            sp.GetRequiredService<IEmployeeLoanRepository>(),
            sp.GetRequiredService<ISalaryAdvanceRepository>(),
            sp.GetRequiredService<IWageGarnishmentRepository>(),
            sp.GetRequiredService<IBenefitProvisionRepository>(),
            sp.GetRequiredService<IAuditLogRepository>(),
            sp.GetRequiredService<PayrollCalculationFactory>(),
            sp.GetRequiredService<IBankTransferService>(),
            sp.GetRequiredService<ISickLeaveRepository>()));
        services.AddScoped<PayrollConceptService>();
        services.AddScoped<EmployeeLoanService>();
        services.AddScoped<SalaryAdvanceService>();
        services.AddScoped<WageGarnishmentService>();
        services.AddScoped<TerminationService>();
        services.AddScoped<SickLeaveService>();

        // Banking
        services.AddScoped<BankAccountService>();
        services.AddScoped<IBankTransferService, BankTransferService>();

        // Electronic Invoicing
        services.AddScoped<IPartnerService, PartnerService>();
        services.AddScoped<IElectronicInvoiceService, ElectronicInvoiceService>();

        // Commercial
        services.AddScoped<ClientService>();
        services.AddScoped<QuoteService>();
        services.AddScoped<SaleService>();
        services.AddScoped<IPurchaseIntelligenceService, PurchaseIntelligenceService>();
        
        // CRM
        services.AddScoped<LeadService>();
        services.AddScoped<OpportunityService>();

        // Inventory
        services.AddScoped<CategoryService>();
        services.AddScoped<BrandService>();
        services.AddScoped<SupplierService>();
        services.AddScoped<ProductService>();
        services.AddScoped<TaxCategoryService>();
        services.AddScoped<IFiscalService, FiscalService>();
        services.AddScoped<IInventoryMovementService, InventoryMovementService>();
        services.AddScoped<InventoryMovementService>();
        services.AddScoped<PurchaseService>();
        services.AddScoped<SupplierPaymentService>();
        services.AddScoped<SupplierCreditNoteService>();

        // Credits
        services.AddScoped<CreditService>();

        // Cash
        services.AddScoped<CashRegisterService>();

        // Treasury
        services.AddScoped<ITreasuryService>(sp => new TreasuryService(
            sp.GetRequiredService<ICheckRepository>(),
            sp.GetRequiredService<ICheckbookRepository>(),
            sp.GetRequiredService<ICheckAuditTrailRepository>(),
            sp.GetRequiredService<ICheckPrintTemplateRepository>(),
            sp.GetRequiredService<IApprovalEngine>()));

        // Fixed Assets
        services.AddScoped<FixedAssetService>();
        services.AddSingleton<DepreciationCalculatorFactory>();

        // Branches
        services.AddScoped<BranchService>();

        // BI
        services.AddScoped<BiService>();

        // Exchange Rates & Reports
        services.AddScoped<IExchangeRateService, ExchangeRateService>();
        services.AddScoped<ICustomReportService, CustomReportService>();
        services.AddScoped<ISyncService, SyncService>();
        services.AddScoped<DynamicReportEngine>();

        // Approval
        services.AddScoped<ApprovalFlowConfigService>();
        services.AddScoped<IRegionalTaxConfigurationRepository, RegionalTaxConfigurationRepository>();
        services.AddScoped<IAccountingRuleTemplateRepository, AccountingRuleTemplateRepository>();
        services.AddScoped<IEmployeePayrollExemptionRepository, EmployeePayrollExemptionRepository>();
        services.AddScoped<IRegionalDashboardRepository, RegionalDashboardRepository>();
        services.AddScoped<IRegionalTaxConfigService, RegionalTaxConfigService>();
        services.AddScoped<ICountryTaxConfigService, CountryTaxConfigService>();
        services.AddScoped<SubscriptionPlanService>();
        services.AddScoped<IPayrollLocalizationService, PayrollLocalizationService>();
        services.AddScoped<ISettlementPdfService, SettlementPdfService>();
        services.AddScoped<RegionalFinancialDashboardService, RegionalFinancialDashboardService>();
        services.AddScoped<IApprovalEngine, ApprovalEngine>();

        // Accounting
        services.AddScoped<AccountService>();
        services.AddScoped<IConsolidationService, ConsolidationService>();
        services.AddScoped<AccountingEntryService>();
        services.AddScoped<AccountingPeriodService>();
        services.AddScoped<FiscalYearService>();
        services.AddScoped<AccountLinkService>();
        services.AddScoped<AutoAccountingService>(sp => new AutoAccountingService(
            sp.GetRequiredService<IAccountingEntryRepository>(),
            sp.GetRequiredService<IAccountingPeriodRepository>(),
            sp.GetRequiredService<IAccountLinkRepository>(),
            sp.GetRequiredService<IAccountingRuleRepository>(),
            sp.GetRequiredService<IAccountRepository>(),
            sp.GetRequiredService<ITenantContext>(),
            sp.GetRequiredService<IPayrollRepository>(),
            sp.GetRequiredService<ICashMovementRepository>(),
            sp.GetRequiredService<IAccountingRuleTemplateRepository>(),
            sp.GetRequiredService<ICompanyRepository>(),
            sp.GetRequiredService<IFiscalYearRepository>(),
            sp.GetRequiredService<ICountryTaxConfigRepository>()));
        services.AddScoped<IAutoAccountingService>(sp => sp.GetRequiredService<AutoAccountingService>());
        services.AddScoped<FinancialReportService>();
        services.AddScoped<CostCenterService>();
        services.AddScoped<BudgetService>();
        services.AddScoped<BudgetDetailService>();
        services.AddScoped<BudgetTrackingService>();
        services.AddScoped<Zorvian.Application.Services.ReconciliationService>();
        services.AddScoped<CreditNoteService>();

        // Warranty
        services.AddScoped<WarrantySlaConfigService>();
        services.AddScoped<WarrantyProviderService>();
        services.AddScoped<IWarrantyCostService, WarrantyCostService>();
        services.AddScoped<WarrantyCostService>();
        services.AddScoped<WarrantyProfitabilityReportService>();
        services.AddScoped<WarrantyPartRequestService>();
        services.AddScoped<WarrantyCommunicationService>();
        services.AddScoped<WarrantyPartUsageService>();
        services.AddScoped<WarrantyDashboardService>();
        services.AddScoped<WarrantyTimelineService>();
        services.AddScoped<WarrantyService>();
        services.AddScoped<WorkshopService>();

        // Documentary Engine (Professional)
        services.AddScoped<IDocumentService, Zorvian.Application.Services.Documentary.DocumentService>();
        services.AddScoped<IDocumentGenerationService, Zorvian.Application.Services.Documentary.DocumentGenerationService>();

        // API Keys
        services.AddScoped<ApiKeyService>();

        // Notifications
        services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();
        services.AddScoped<IFCMNotificationService, FCMNotificationService>();
        services.AddScoped<INotificationService, CombinedNotificationService>();

        // Fleet
        services.AddScoped<Zorvian.Application.Services.Fleet.VehicleBrandService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.VehicleTypeService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.FuelTypeService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.DriverLicenseCategoryService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.VehicleService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.DriverService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.FleetDocumentService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.DocumentTypeService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.RouteService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.DeliveryService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.TripService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.FleetDashboardService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.FuelRefillService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.WorkOrderService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.MaintenanceScheduleService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.WorkshopService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.FailureTypeService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.MaintenanceTemplateService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.ExpenseCategoryService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.ExpenseSubcategoryService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.FleetExpenseService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.FleetReportService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.FleetAlertService>();
        services.AddScoped<Zorvian.Infrastructure.Services.FleetPdfChartService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.GpsService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.GeofenceService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.RouteOptimizationService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.DeliveryTrackingService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.PredictiveMaintenanceService>();
        services.AddScoped<Zorvian.Application.Services.Fleet.FuelAnomalyDetectionService>();
        services.AddScoped<Zorvian.Infrastructure.Services.Fleet.PalmTrackFleetMapper>();

        // PalmTrack Integration Services
        services.AddScoped<Zorvian.Application.Interfaces.PalmTrack.IPalmTrackSecretService, Zorvian.Infrastructure.Services.PalmTrack.PalmTrackSecretService>();
        services.AddScoped<Zorvian.Application.Interfaces.PalmTrack.IPalmTrackIdempotencyService, Zorvian.Infrastructure.Services.PalmTrack.PalmTrackIdempotencyService>();
        services.AddScoped<Zorvian.Application.Interfaces.PalmTrack.IPalmTrackIdentityService, Zorvian.Infrastructure.Services.PalmTrack.PalmTrackIdentityService>();
        services.AddScoped<Zorvian.Web.Services.PalmTrack.IPalmTrackWebhookValidator, Zorvian.Web.Services.PalmTrack.PalmTrackWebhookValidator>();
        services.AddScoped<Zorvian.Application.Interfaces.PalmTrack.IPalmTrackEventMapper, Zorvian.Infrastructure.Services.PalmTrack.PalmTrackEventMapper>();
        services.AddScoped<Zorvian.Application.Interfaces.PalmTrack.IPalmTrackApiKeyService, Zorvian.Infrastructure.Services.PalmTrack.PalmTrackApiKeyService>();
        services.AddScoped<Zorvian.Application.Interfaces.PalmTrack.IPalmTrackWriteService, Zorvian.Infrastructure.Services.PalmTrack.PalmTrackWriteService>();

        // External Identity Mapping Service
        services.AddScoped<Zorvian.Application.Interfaces.IExternalIdentityMappingService, Zorvian.Infrastructure.Services.ExternalIdentityMappingService>();

        // SSO Service (Paso 6)
        services.AddScoped<Zorvian.Application.Interfaces.ISsoService, Zorvian.Infrastructure.Services.SsoService>();

        // AI Services
        services.AddSingleton<AbsenteeismPredictionService>();
        services.AddSingleton<SalesPredictionService>();
        services.AddSingleton<ExpenseClassificationService>();
        services.AddSingleton<Zorvian.Application.Interfaces.IExpenseClassificationService>(sp => sp.GetRequiredService<ExpenseClassificationService>());
        services.AddScoped<AccountingAssistantService>();
        services.AddScoped<PurchaseRecommendationService>();
        services.AddScoped<EnhancedReportService>();
        services.AddScoped<FinancialAssistantService>();
        services.AddScoped<IAiDocumentService>(sp => new AiDocumentService(
            sp.GetRequiredService<IConfiguration>()["GoogleAi:ProjectId"]!,
            sp.GetRequiredService<IConfiguration>()["GoogleAi:Location"]!));

        return services;
    }

    public static IServiceCollection AddZorvianJobs(this IServiceCollection services)
    {
        services.AddScoped<CheckInReminderJob>();
        services.AddScoped<DocumentExpirationJob>();
        services.AddScoped<AttendancePhotoCleanupJob>();
        services.AddScoped<AbsenteeismTrainingJob>();
        services.AddScoped<SalesPredictionTrainingJob>();
        services.AddScoped<ExpenseClassificationTrainingJob>();
        services.AddScoped<WarrantySlaMonitorJob>();
        services.AddScoped<WebhookDeliveryJob>();
        services.AddScoped<AuditLogCleanupJob>();
        services.AddScoped<DatabaseBackupJob>();
        services.AddScoped<VacationAutomatedJob>();
        services.AddScoped<OcrProcessingJob>();
        services.AddScoped<Zorvian.Web.Jobs.FleetAlertJob>();
        services.AddScoped<Zorvian.Web.Jobs.FleetExpenseNotificationJob>();
        return services;
    }
}
