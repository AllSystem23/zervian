namespace Zorvian.Web.Authorization;

public static class Permissions
{
    public const string UserRead = "user.read";
    public const string UserWrite = "user.write";
    public const string UserManage = "user.manage";

    public const string EmployeeRead = "employee.read";
    public const string EmployeeWrite = "employee.write";
    public const string EmployeeDelete = "employee.delete";
    public const string EmployeeImport = "employee.import";

    public const string PayrollRead = "payroll.read";
    public const string PayrollWrite = "payroll.write";
    public const string PayrollProcess = "payroll.process";

    public const string CreditRead = "credit.read";
    public const string CreditWrite = "credit.write";

    public const string SaleRead = "sale.read";
    public const string SaleWrite = "sale.write";

    public const string AccountingRead = "accounting.read";
    public const string AccountingWrite = "accounting.write";

    public const string AuditRead = "audit.read";

    public const string ReportRead = "report.read";

    public const string ApiKeyManage = "apikey.manage";

    public const string CompanyManage = "company.manage";

    public const string RoleManage = "role.manage";

    public const string InventoryRead = "inventory.read";
    public const string InventoryWrite = "inventory.write";

    public const string CashRead = "cash.read";
    public const string CashWrite = "cash.write";

    public const string FixedAssetRead = "fixedasset.read";
    public const string FixedAssetWrite = "fixedasset.write";

    public const string WarrantyRead = "warranty.read";
    public const string WarrantyWrite = "warranty.write";
    public const string WarrantyViewProfitability = "warranty.view_profitability";

    public const string PurchaseRead = "purchase.read";
    public const string PurchaseWrite = "purchase.write";

    public const string CommissionRead = "commission.read";
    public const string CommissionWrite = "commission.write";
    public const string CommissionProcess = "commission.process";

    public const string GoalRead = "goal.read";
    public const string GoalWrite = "goal.write";
    public const string GoalEvaluate = "goal.evaluate";

    public const string KpiRead = "kpi.read";
    public const string KpiWrite = "kpi.write";

    public const string ProviderRead = "provider.read";
    public const string ProviderWrite = "provider.write";

    public const string ClientRead = "client.read";
    public const string ClientWrite = "client.write";

    public const string DocumentRead = "document.read";
    public const string DocumentWrite = "document.write";

    // Settings Module
    public const string SettingsRead = "settings.read";
    public const string SettingsWrite = "settings.write";

    // Fleet Module
    public const string FleetRead = "fleet.read";
    public const string FleetWrite = "fleet.write";
    public const string FleetDelete = "fleet.delete";
    public const string FleetConfig = "fleet.config";
}
