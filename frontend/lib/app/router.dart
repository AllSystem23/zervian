import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../auth/auth_provider.dart';
import '../core/widgets/app_shell.dart';
import '../features/dashboard/dashboard_page.dart';
import '../features/dashboard/pages/absence_calendar_page.dart';
import '../features/executive_dashboard/executive_dashboard_page.dart';
import '../features/profile/profile_page.dart';
import '../features/attendance/pages/attendance_page.dart';
import '../features/attendance/pages/attendance_history_page.dart';
import '../features/attendance/pages/kiosk_page.dart';
import '../features/attendance/pages/qr_checkin_page.dart';
import '../features/reports/pages/reports_page.dart';
import '../features/reports/pages/audit_logs_page.dart';
import '../features/settings/pages/company_settings_page.dart';
import '../features/settings/pages/leave_types_page.dart';
import '../features/settings/pages/leave_type_form_page.dart';
import '../features/admin/pages/user_list_page.dart';
import '../features/admin/pages/invite_user_page.dart';
import '../features/admin/pages/invitation_history_page.dart';
import '../features/admin/pages/super_admin_companies_page.dart';
import '../features/admin/pages/subscription_plans_page.dart';
import '../features/branches/pages/branch_list_page.dart';
import '../features/branches/pages/branch_form_page.dart';
import '../features/departments/pages/department_form_page.dart';
import '../features/departments/pages/department_list_page.dart';
import '../features/employees/pages/employee_detail_page.dart';
import '../features/employees/pages/employee_form_page.dart';
import '../features/employees/pages/employee_list_page.dart';
import '../features/login/register_page.dart';
import '../features/login/login_page.dart';
import '../features/login/forgot_password_page.dart';
import '../features/mfa/mfa_page.dart';
import '../features/vacations/pages/vacation_detail_page.dart';
import '../features/vacations/pages/vacation_form_page.dart';
import '../features/vacations/pages/vacation_list_page.dart';
import '../features/permissions/pages/permission_detail_page.dart';
import '../features/permissions/pages/permission_form_page.dart';
import '../features/permissions/pages/permission_list_page.dart';
import '../features/onboarding/onboarding_page.dart';
import '../features/splash/splash_page.dart';
import '../features/payroll/pages/payroll_page.dart';
import '../features/payroll/pages/settlement_form_page.dart';
import '../features/payroll/pages/payroll_run_detail_page.dart';
import '../features/payroll/pages/payroll_periods_page.dart';
import '../features/payroll/pages/salaries_page.dart';
import '../features/payroll/pages/deduction_types_page.dart';
import '../features/goals/configurator/goals_config_screen.dart';
import '../features/goals/dashboard/goals_dashboard_screen.dart';
import '../features/goals/portal/my_goals_screen.dart';
import '../features/providers/pages/provider_list_page.dart';
import '../features/providers/pages/provider_detail_page.dart';
import '../features/providers/pages/provider_form_page.dart';
import '../features/providers/pages/service_contract_detail_page.dart';
import '../features/providers/pages/provider_contracts_page.dart';
import '../features/providers/pages/contract_form_page.dart';
import '../features/providers/pages/provider_invoices_page.dart';
import '../features/providers/pages/provider_dashboard_page.dart';
import '../features/unauthorized/unauthorized_page.dart';
import '../features/clients/pages/client_list_page.dart';
import '../features/clients/pages/client_form_page.dart';
import '../features/clients/pages/client_statement_page.dart';
import '../features/sales/pages/sale_list_page.dart';
import '../features/sales/pages/sale_detail_page.dart';
import '../features/sales/pages/sale_form_page.dart';
import '../features/quotes/pages/quote_list_page.dart';
import '../features/quotes/pages/quote_form_page.dart';
import '../features/quotes/pages/quote_detail_page.dart';
import '../features/quotes/pages/quote_kanban_page.dart';
import '../features/products/pages/kardex_page.dart';
import '../features/products/pages/inventory_dashboard_page.dart';
import '../features/products/pages/inventory_valuation_page.dart';
import '../features/products/pages/product_list_page.dart';
import '../features/products/pages/product_form_page.dart';
import '../features/categories/pages/category_list_page.dart';
import '../features/categories/pages/category_form_page.dart';
import '../features/brands/pages/brand_list_page.dart';
import '../features/brands/pages/brand_form_page.dart';
import '../features/cost_centers/pages/cost_center_list_page.dart';
import '../features/cost_centers/pages/cost_center_form_page.dart';
import '../features/budgets/pages/budget_list_page.dart';
import '../features/budgets/pages/budget_form_page.dart';
import '../features/budgets/pages/budget_vs_actual_page.dart';
import '../features/budgets/pages/budget_vs_actual_detail_page.dart';
import '../features/reconciliations/pages/reconciliation_list_page.dart';
import '../features/reconciliations/pages/reconciliation_form_page.dart';
import '../features/reconciliations/pages/reconciliation_detail_page.dart';
import '../features/credit_notes/pages/credit_note_list_page.dart';
import '../features/credit_notes/pages/credit_note_form_page.dart';
import '../features/suppliers/pages/supplier_list_page.dart';
import '../features/suppliers/pages/supplier_form_page.dart';
import '../features/inventory_movements/pages/inventory_movement_list_page.dart';
import '../features/credits/pages/credit_list_page.dart';
import '../features/credits/pages/credit_detail_page.dart';
import '../features/cash_registers/pages/cash_register_list_page.dart';
import '../features/cash_registers/pages/cash_register_detail_page.dart';
import '../features/cash_registers/pages/cash_register_arqueo_page.dart';
import '../features/dashboard_v2/dashboard_v2_page.dart';
import '../features/warranties/pages/warranty_list_page.dart';
import '../features/warranties/pages/warranty_form_page.dart';
import '../features/warranties/pages/warranty_detail_page.dart';
import '../features/warranties/pages/warranty_dashboard_page.dart';
import '../features/warranties/pages/warranty_tracking_page.dart';
import '../features/workshops/pages/workshop_list_page.dart';
import '../features/workshops/pages/workshop_form_page.dart';
import '../features/workshops/pages/workshop_detail_page.dart';
import '../features/purchases/pages/purchase_list_page.dart';
import '../features/purchases/pages/purchase_form_page.dart';
import '../features/purchases/pages/purchase_detail_page.dart';
import '../features/supplier_payments/pages/supplier_payment_list_page.dart';
import '../features/supplier_payments/pages/supplier_payment_form_page.dart';
import '../features/supplier_credit_notes/pages/supplier_credit_note_list_page.dart';
import '../features/supplier_credit_notes/pages/supplier_credit_note_form_page.dart';
import '../features/purchases/pages/inventory_adjustment_page.dart';
import '../features/purchase_orders/pages/purchase_order_list_page.dart';
import '../features/purchase_orders/pages/purchase_order_form_page.dart';
import '../features/purchase_orders/pages/purchase_order_detail_page.dart';
import '../features/approval/pages/approval_flow_list_page.dart';
import '../features/approval/pages/approval_flow_form_page.dart';
import '../features/approval/pages/approval_pending_page.dart';
import '../features/credits/pages/credit_refinancing_form_page.dart';
import '../features/credits/pages/overdue_dashboard_page.dart';
import '../features/bi/pages/executive_dashboard_page.dart' as bi;
import '../features/bi/pages/financial_dashboard_page.dart';
import '../features/bi/pages/commercial_dashboard_page.dart';
import '../features/bi/pages/operational_dashboard_page.dart';
import '../features/chat/pages/chat_page.dart';
import '../features/exchange_rates/pages/exchange_rate_list_page.dart';
import '../features/exchange_rates/pages/exchange_rate_form_page.dart';
import '../features/custom_reports/pages/custom_report_list_page.dart';
import '../features/custom_reports/pages/custom_report_builder_page.dart';
import '../features/custom_reports/pages/custom_report_result_page.dart';
import '../features/webhooks/pages/webhook_list_page.dart';
import '../features/fiscal/pages/country_tax_config_page.dart';
import '../features/fiscal/pages/regional_tax_config_page.dart';
import '../features/webhooks/pages/webhook_form_page.dart';
import '../features/webhooks/pages/webhook_logs_page.dart';
import '../features/accounting/pages/equity_changes_page.dart';
import '../features/accounting/pages/comparative_reports_page.dart';
import '../features/accounting/pages/ai_assistant_page.dart';
import '../features/accounting/pages/trial_balance_page.dart';
import '../features/accounting/pages/income_statement_page.dart';
import '../features/accounting/pages/chart_of_accounts_page.dart';
import '../features/accounting/pages/accounting_periods_page.dart';
import '../features/accounting/pages/fiscal_years_page.dart';
import '../features/accounting/pages/accounting_entries_page.dart';
import '../features/accounting/pages/account_links_page.dart';
import '../features/treasury/pages/treasury_dashboard_page.dart';
import '../features/treasury/pages/check_issuance_page.dart';
import '../features/treasury/pages/bank_transfer_page.dart';
import '../features/treasury/pages/bank_deposit_page.dart';
import '../features/treasury/pages/bank_commission_page.dart';
import '../features/treasury/pages/bank_collection_page.dart';
import '../features/documents/pages/document_center_page.dart';
import '../features/documents/pages/document_detail_page.dart';
import '../features/documents/pages/quick_generate_wizard_page.dart';
import '../features/documents/pages/template_editor_page.dart';
import '../features/pos/pages/pos_page.dart';
import '../features/crm/pages/crm_page.dart';
import '../features/crm/pages/lead_detail_page.dart';
import '../features/crm/pages/lead_form_page.dart';
import '../features/crm/pages/opportunity_detail_page.dart';
import '../features/crm/pages/opportunity_form_page.dart';
import '../features/fleet/pages/fleet_dashboard_page.dart';
import '../features/sick_leave/pages/sick_leave_list_page.dart';
import '../features/sick_leave/pages/sick_leave_form_page.dart';
import '../features/not_found/not_found_page.dart';
import '../features/fleet/pages/fleet_vehicle_list_page.dart';
import '../features/fleet/pages/fleet_vehicle_form_page.dart';
import '../features/fleet/pages/fleet_driver_list_page.dart';
import '../features/fleet/pages/fleet_driver_form_page.dart';
import '../features/fleet/pages/fleet_document_list_page.dart';
import '../features/fleet/pages/fleet_document_form_page.dart';
import '../features/fleet/pages/fleet_route_list_page.dart';
import '../features/fleet/pages/fleet_route_form_page.dart';
import '../features/fleet/pages/fleet_delivery_list_page.dart';
import '../features/fleet/pages/fleet_delivery_form_page.dart';
import '../features/fleet/pages/fleet_trip_list_page.dart';
import '../features/fleet/pages/fleet_trip_form_page.dart';
import '../features/fleet/pages/fleet_reports_page.dart';
import '../features/fleet/pages/fleet_fuel_list_page.dart';
import '../features/fleet/pages/fleet_fuel_form_page.dart';
import '../features/fleet/pages/fleet_maintenance_list_page.dart';
import '../features/fleet/pages/fleet_maintenance_form_page.dart';
import '../features/fleet/pages/fleet_workshop_list_page.dart';
import '../features/fleet/pages/fleet_workshop_form_page.dart';
import '../features/fleet/pages/fleet_expense_list_page.dart';
import '../features/fleet/pages/fleet_expense_form_page.dart';
import '../features/fleet/pages/fleet_gps_page.dart';
import '../features/fleet/pages/fleet_alerts_page.dart';
import '../features/fleet/pages/fleet_tracking_page.dart';
import '../features/fleet/pages/fleet_predictive_page.dart';
import '../features/fleet/pages/fleet_catalog_page.dart';
import '../features/palmtrack/pages/palmtrack_sync_page.dart';
import '../features/palmtrack/pages/palmtrack_conflicts_page.dart';
import '../features/palmtrack/pages/palmtrack_production_page.dart';
import '../features/palmtrack/pages/palmtrack_farms_page.dart';
import '../features/palmtrack/pages/palmtrack_lots_page.dart';
import '../features/palmtrack/pages/palmtrack_producers_page.dart';
import '../features/palmtrack/pages/palmtrack_sales_page.dart';
import '../features/palmtrack/pages/palmtrack_inventory_page.dart';
import '../features/palmtrack/pages/palmtrack_settings_page.dart';
import '../features/palmtrack/pages/palmtrack_complete_profile_page.dart';
import '../features/predictions/pages/sales_predictions_page.dart';

final _routeRoles = <String, List<String>>{
  '/goals/configurator': ['SuperAdmin', 'CompanyAdmin'],
  '/goals/dashboard': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor'],
  '/goals/my-goals': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/dashboard': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/executive-dashboard': ['SuperAdmin', 'CompanyAdmin'],
  '/employees': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor'],
  '/departments': ['SuperAdmin', 'CompanyAdmin', 'Rrhh'],
  '/vacations': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/permissions': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/attendance': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/reports': ['SuperAdmin', 'CompanyAdmin', 'Rrhh'],
  '/audit-logs': ['SuperAdmin', 'CompanyAdmin'],
  '/admin': ['SuperAdmin', 'CompanyAdmin'],
  '/admin/companies': ['SuperAdmin'],
  '/admin/subscription-plans': ['SuperAdmin'],
  '/settings': ['SuperAdmin', 'CompanyAdmin'],
  '/payroll': ['SuperAdmin', 'CompanyAdmin', 'Rrhh'],
  '/clients': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/sales': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/quotes': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/products': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/products/movements': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/products/adjustments': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/products/valuation': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/categories': ['SuperAdmin', 'CompanyAdmin'],
  '/brands': ['SuperAdmin', 'CompanyAdmin'],
  '/cost-centers': ['SuperAdmin', 'CompanyAdmin'],
  '/budgets': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/budgets/vs-actual': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/credit-notes': ['SuperAdmin', 'CompanyAdmin', 'Supervisor'],
  '/approval-flows': ['SuperAdmin', 'CompanyAdmin'],
  '/approval-pending': ['SuperAdmin', 'CompanyAdmin', 'Supervisor'],
  '/suppliers': ['SuperAdmin', 'CompanyAdmin', 'Supervisor'],
  '/inventory-movements': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/credits': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/credits/overdue-dashboard': ['SuperAdmin', 'CompanyAdmin', 'Supervisor'],
  '/cash-registers': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/warranties': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/workshops': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/providers': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor'],
  '/providers/dashboard': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor'],
  '/branches': ['SuperAdmin', 'CompanyAdmin', 'Supervisor', 'Rrhh', 'Employee'],
  '/bi/executive': ['SuperAdmin', 'CompanyAdmin'],
  '/bi/financial': ['SuperAdmin', 'CompanyAdmin'],
  '/bi/commercial': ['SuperAdmin', 'CompanyAdmin', 'Supervisor', 'Employee'],
  '/dashboard-v2': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee', 'Accountant'],
  '/bi/operational': ['SuperAdmin', 'CompanyAdmin', 'Rrhh'],
  '/predictions/sales': ['SuperAdmin', 'CompanyAdmin', 'Supervisor', 'Employee'],
  '/chat': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/exchange-rates': ['SuperAdmin', 'CompanyAdmin'],
  '/custom-reports': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/webhooks': ['SuperAdmin', 'CompanyAdmin'],
  '/admin/country-tax-configs': ['SuperAdmin'],
  '/admin/regional-tax-configs': ['SuperAdmin', 'CompanyAdmin'],
  '/accounting': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/accounting/reports': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/accounting/reports/equity': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/accounting/reports/comparative': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/accounting/ai-assistant': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/accounting/trial-balance': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/accounting/income-statement': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/accounting/chart-of-accounts': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/accounting/periods': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/accounting/fiscal-years': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/accounting/entries': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/accounting/account-links': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/budgets/vs-actual/detail': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/reconciliations': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/treasury': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/treasury/checks': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/treasury/transfers': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/treasury/deposits': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/treasury/commissions': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/treasury/collections': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/pos': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/crm': ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee'],
  '/fleet': ['SuperAdmin', 'CompanyAdmin', 'Supervisor', 'Dispatcher', 'Employee'],
  '/purchases/payments': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
  '/purchases/credit-notes': ['SuperAdmin', 'CompanyAdmin', 'Accountant'],
};

bool _hasAccess(String role, String location) {
  final allowed = _routeRoles.entries.firstWhere(
    (e) => location.startsWith(e.key),
    orElse: () => MapEntry('', ['SuperAdmin', 'CompanyAdmin', 'Rrhh', 'Supervisor', 'Employee']),
  ).value;
  return allowed.contains(role);
}

final routerProvider = Provider<GoRouter>((ref) {
  final authState = ref.watch(authProvider);

  return GoRouter(
    initialLocation: '/splash',
    debugLogDiagnostics: true,
    errorBuilder: (context, state) => NotFoundPage(state: state),
    redirect: (context, state) {
      final status = authState.status;
      final location = state.matchedLocation;
      final isLoginRoute = location == '/login';
      final isOnboardingRoute = location == '/onboarding';
      final isSplashRoute = location == '/splash';
      final isRootRoute = location == '/';

      if (isRootRoute) return '/splash';

      if (status == AuthStatus.unknown && !isSplashRoute) return '/splash';

      final isPublicRoute = location == '/warranties/track';

      if (status == AuthStatus.mfaRequired && !location.startsWith('/mfa')) {
        return '/mfa-login';
      }

      if (status == AuthStatus.unauthenticated && !isLoginRoute && !isPublicRoute) {
        return '/login';
      }

      if (status == AuthStatus.authenticated && isSplashRoute) {
        final hasCompany = authState.tenantId != null && authState.tenantId!.isNotEmpty;
        return hasCompany ? '/dashboard' : '/onboarding';
      }

      if (status == AuthStatus.authenticated && isLoginRoute) {
        final hasCompany = authState.tenantId != null && authState.tenantId!.isNotEmpty;
        return hasCompany ? '/dashboard' : '/onboarding';
      }

      if (status == AuthStatus.authenticated && !isLoginRoute && !isOnboardingRoute && !isSplashRoute) {
        if (!_hasAccess(authState.role ?? 'Employee', location)) {
          return '/unauthorized';
        }
      }

      return null;
    },
    routes: [
      GoRoute(
        path: '/login',
        name: 'login',
        builder: (_, _) => const LoginPage(),
      ),
      GoRoute(
        path: '/splash',
        name: 'splash',
        builder: (_, _) => const SplashPage(),
      ),
      GoRoute(
        path: '/register',
        name: 'register',
        builder: (_, state) => RegisterPage(inviteCode: state.uri.queryParameters['code']),
      ),
      GoRoute(
        path: '/forgot-password',
        name: 'forgot-password',
        builder: (_, _) => const ForgotPasswordPage(),
      ),
      GoRoute(
        path: '/mfa-login',
        name: 'mfa-login',
        builder: (_, state) => MfaLoginPage(mfaToken: (state.extra as String?) ?? ''),
      ),
      GoRoute(
        path: '/mfa-settings',
        name: 'mfa-settings',
        builder: (_, _) => const MfaSettingsPage(),
      ),
      GoRoute(
        path: '/onboarding',
        name: 'onboarding',
        builder: (_, _) => const OnboardingPage(),
      ),
      GoRoute(
        path: '/palmtrack/complete-profile',
        name: 'palmtrack-complete-profile',
        builder: (_, _) => const PalmTrackCompleteProfilePage(),
      ),
      GoRoute(
        path: '/warranties/track',
        name: 'warranty-tracking',
        builder: (_, _) => const WarrantyTrackingPage(),
      ),
      ShellRoute(
        builder: (_, _, child) => AppShell(child: child),
        routes: [
          GoRoute(
            path: '/dashboard',
            name: 'dashboard',
            builder: (_, _) => const DashboardPage(),
          ),
          GoRoute(
            path: '/dashboard-v2',
            name: 'dashboard-v2',
            builder: (_, _) => const DashboardV2Page(),
          ),
          GoRoute(
            path: '/executive-dashboard',
            name: 'executive-dashboard',
            builder: (_, _) => const ExecutiveDashboardPage(),
          ),
          GoRoute(
            path: '/profile',
            name: 'profile',
            builder: (_, _) => const ProfilePage(),
          ),
          GoRoute(
            path: '/absence-calendar',
            name: 'absence-calendar',
            builder: (_, _) => const AbsenceCalendarPage(),
          ),
          GoRoute(
            path: '/attendance',
            name: 'attendance',
            builder: (_, _) => const AttendancePage(),
            routes: [
              GoRoute(path: 'history', name: 'attendance-history', builder: (_, _) => const AttendanceHistoryPage()),
              GoRoute(path: 'kiosk', name: 'attendance-kiosk', builder: (_, _) => const KioskPage()),
              GoRoute(path: 'qr', name: 'attendance-qr', builder: (_, _) => const QRCheckInPage()),
            ],
          ),
          GoRoute(
            path: '/goals',
            name: 'goals',
            redirect: (_, _) => '/goals/dashboard',
          ),
          GoRoute(
            path: '/goals/configurator',
            name: 'goals-configurator',
            builder: (_, _) => const GoalsConfigScreen(),
          ),
          GoRoute(
            path: '/goals/dashboard',
            name: 'goals-dashboard',
            builder: (_, _) => const GoalsDashboardScreen(),
          ),
          GoRoute(
            path: '/goals/my-goals',
            name: 'goals-my-goals',
            builder: (_, _) => const MyGoalsScreen(),
          ),
          GoRoute(
            path: '/unauthorized',
            name: 'unauthorized',
            builder: (_, _) => const UnauthorizedPage(),
          ),
          GoRoute(
            path: '/employees',
            name: 'employees',
            builder: (_, _) => const EmployeeListPage(),
            routes: [
              GoRoute(path: 'new', name: 'employee-new', builder: (_, _) => const EmployeeFormPage()),
              GoRoute(
                path: ':employeeId',
                name: 'employee-detail',
                builder: (_, state) => EmployeeDetailPage(employeeId: state.pathParameters['employeeId']!),
                routes: [
                  GoRoute(path: 'edit', name: 'employee-edit', builder: (_, state) => EmployeeFormPage(employeeId: state.pathParameters['employeeId']!)),
                  GoRoute(
                    path: 'sick-leaves',
                    name: 'employee-sick-leaves',
                    builder: (_, state) => SickLeaveListPage(employeeId: state.pathParameters['employeeId']!),
                    routes: [
                      GoRoute(path: 'new', name: 'employee-sick-leave-new', builder: (_, state) => SickLeaveFormPage(employeeId: state.pathParameters['employeeId']!)),
                    ],
                  ),
                ],
              ),
            ],
          ),
          GoRoute(
            path: '/vacations',
            name: 'vacations',
            builder: (_, _) => const VacationListPage(),
            routes: [
              GoRoute(path: 'new', name: 'vacation-new', builder: (_, _) => const VacationFormPage()),
              GoRoute(path: ':vacationId', name: 'vacation-detail', builder: (_, state) => VacationDetailPage(vacationId: state.pathParameters['vacationId']!)),
            ],
          ),
          GoRoute(
            path: '/permissions',
            name: 'permissions',
            builder: (_, _) => const PermissionListPage(),
            routes: [
              GoRoute(path: 'new', name: 'permission-new', builder: (_, _) => const PermissionFormPage()),
              GoRoute(path: ':permissionId', name: 'permission-detail', builder: (_, state) => PermissionDetailPage(permissionId: state.pathParameters['permissionId']!)),
            ],
          ),
          GoRoute(
            path: '/settings',
            name: 'settings',
            builder: (_, _) => const CompanySettingsPage(),
          ),
          GoRoute(
            path: '/admin',
            name: 'admin',
            redirect: (_, _) => '/admin/companies',
          ),
          GoRoute(
            path: '/admin/users',
            name: 'admin-users',
            builder: (_, _) => const UserListPage(),
          ),
          GoRoute(
            path: '/admin/invite',
            name: 'admin-invite',
            builder: (_, _) => const InviteUserPage(),
          ),
          GoRoute(
            path: '/admin/invitations-history',
            name: 'admin-invitations-history',
            builder: (_, _) => const InvitationHistoryPage(),
          ),
          GoRoute(
            path: '/admin/companies',
            name: 'admin-companies',
            builder: (_, _) => const SuperAdminCompaniesPage(),
          ),
          GoRoute(
            path: '/admin/subscription-plans',
            name: 'admin-subscription-plans',
            builder: (_, _) => const SubscriptionPlansPage(),
          ),
          GoRoute(
            path: '/leave-types',
            name: 'leave-types',
            builder: (_, _) => const LeaveTypesPage(),
            routes: [
              GoRoute(
                path: 'new',
                name: 'leave-type-new',
                builder: (_, _) => const LeaveTypeFormPage(),
              ),
            ],
          ),
          GoRoute(
            path: '/branches',
            name: 'branches',
            builder: (_, _) => const BranchListPage(),
            routes: [
              GoRoute(path: 'new', name: 'branch-new', builder: (_, _) => const BranchFormPage()),
              GoRoute(path: ':branchId', name: 'branch-detail', builder: (_, state) => BranchFormPage(branchId: state.pathParameters['branchId'])),
            ],
          ),
          GoRoute(
            path: '/reports',
            name: 'reports',
            builder: (_, _) => const ReportsPage(),
          ),
          GoRoute(
            path: '/audit-logs',
            name: 'audit-logs',
            builder: (_, _) => const AuditLogsPage(),
          ),
          GoRoute(
            path: '/departments',
            name: 'departments',
            builder: (_, _) => const DepartmentListPage(),
            routes: [
              GoRoute(path: 'new', name: 'department-new', builder: (_, _) => const DepartmentFormPage()),
              GoRoute(path: ':departmentId/edit', name: 'department-edit', builder: (_, state) => DepartmentFormPage(departmentId: state.pathParameters['departmentId']!)),
            ],
          ),
          GoRoute(
            path: '/payroll',
            name: 'payroll',
            builder: (_, _) => const PayrollPage(),
            routes: [
              GoRoute(
                path: 'periods',
                name: 'payroll-periods',
                builder: (_, _) => const PayrollPeriodsPage(),
                routes: [GoRoute(path: 'new', name: 'payroll-period-new', builder: (_, _) => const PayrollPeriodsPage())],
              ),
              GoRoute(path: 'runs/:runId', name: 'payroll-run-detail', builder: (_, state) => PayrollRunDetailPage(runId: state.pathParameters['runId']!)),
              GoRoute(path: 'salaries', name: 'payroll-salaries', builder: (_, _) => const SalariesPage()),
              GoRoute(path: 'deduction-types', name: 'payroll-deduction-types', builder: (_, _) => const DeductionTypesPage()),
              GoRoute(
                path: 'settlement/:employeeId/:companyId',
                name: 'payroll-settlement',
                builder: (_, state) => SettlementFormPage(
                  employeeId: state.pathParameters['employeeId']!,
                  companyId: state.pathParameters['companyId']!,
                ),
              ),
            ],
          ),
          GoRoute(
            path: '/clients',
            name: 'clients',
            builder: (_, _) => const ClientListPage(),
            routes: [
              GoRoute(path: 'new', name: 'client-new', builder: (_, _) => const ClientFormPage()),
              GoRoute(path: ':clientId/edit', name: 'client-edit', builder: (_, state) => ClientFormPage(clientId: state.pathParameters['clientId']!)),
              GoRoute(path: ':clientId/statement', name: 'client-statement', builder: (_, state) => ClientStatementPage(clientId: state.pathParameters['clientId']!)),
            ],
          ),
          GoRoute(
            path: '/sales',
            name: 'sales',
            builder: (_, _) => const SaleListPage(),
          ),
          GoRoute(
            path: '/sales/new',
            name: 'new-sale',
            builder: (_, _) => const NewSalePage(),
          ),
          GoRoute(
            path: '/sales/:id',
            name: 'sale-detail',
            builder: (_, state) => SaleDetailPage(saleId: state.pathParameters['id']!),
          ),
          GoRoute(
            path: '/quotes',
            name: 'quotes',
            builder: (_, _) => const QuoteListPage(),
            routes: [
              GoRoute(path: 'new', name: 'quote-new', builder: (_, _) => const QuoteFormPage()),
              GoRoute(path: 'kanban', name: 'quote-kanban', builder: (_, _) => const QuoteKanbanPage()),
              GoRoute(path: ':quoteId', name: 'quote-detail', builder: (_, state) => QuoteDetailPage(quoteId: state.pathParameters['quoteId']!)),
              GoRoute(path: ':quoteId/edit', name: 'quote-edit', builder: (_, state) => QuoteFormPage(quoteId: state.pathParameters['quoteId'])),
            ],
          ),
          GoRoute(
            path: '/products',
            name: 'products',
            builder: (_, _) => const ProductListPage(),
            routes: [
              GoRoute(path: 'dashboard', name: 'product-dashboard', builder: (_, _) => const InventoryDashboardPage()),
              GoRoute(path: 'new', name: 'product-new', builder: (_, _) => const ProductFormPage()),
              GoRoute(path: ':productId/edit', name: 'product-edit', builder: (_, state) => ProductFormPage(productId: state.pathParameters['productId']!)),
              GoRoute(path: 'movements', name: 'product-movements', builder: (_, _) => const InventoryMovementListPage()),
              GoRoute(path: 'adjustments', name: 'product-adjustments', builder: (_, _) => const InventoryAdjustmentPage()),
              GoRoute(path: 'valuation', name: 'product-valuation', builder: (_, _) => const InventoryValuationPage()),
              GoRoute(path: 'kardex', name: 'product-kardex', builder: (_, _) => const KardexPage()),
            ],
          ),
          GoRoute(
            path: '/categories',
            name: 'categories',
            builder: (_, _) => const CategoryListPage(),
            routes: [
              GoRoute(path: 'new', name: 'category-new', builder: (_, _) => const CategoryFormPage()),
              GoRoute(path: ':categoryId/edit', name: 'category-edit', builder: (_, state) => CategoryFormPage(categoryId: state.pathParameters['categoryId']!)),
            ],
          ),
          GoRoute(
            path: '/brands',
            name: 'brands',
            builder: (_, _) => const BrandListPage(),
            routes: [
              GoRoute(path: 'new', name: 'brand-new', builder: (_, _) => const BrandFormPage()),
              GoRoute(path: ':brandId/edit', name: 'brand-edit', builder: (_, state) => BrandFormPage(brandId: state.pathParameters['brandId']!)),
            ],
          ),
          GoRoute(
            path: '/cost-centers',
            name: 'cost-centers',
            builder: (_, _) => const CostCenterListPage(),
            routes: [
              GoRoute(path: 'new', name: 'cost-center-new', builder: (_, _) => const CostCenterFormPage()),
              GoRoute(path: ':costCenterId/edit', name: 'cost-center-edit', builder: (_, state) => CostCenterFormPage(costCenterId: state.pathParameters['costCenterId']!)),
            ],
          ),
          GoRoute(
            path: '/budgets',
            name: 'budgets',
            builder: (_, _) => const BudgetListPage(),
            routes: [
              GoRoute(path: 'new', name: 'budget-new', builder: (_, _) => const BudgetFormPage()),
              GoRoute(path: ':budgetId/edit', name: 'budget-edit', builder: (_, state) => BudgetFormPage(budgetId: state.pathParameters['budgetId']!)),
            ],
          ),
          GoRoute(
            path: '/budgets/vs-actual',
            name: 'budget-vs-actual',
            builder: (_, _) => const BudgetVsActualPage(),
            routes: [
              GoRoute(
                path: 'detail',
                name: 'budget-vs-actual-detail',
                builder: (_, state) {
                  final year = int.tryParse(state.uri.queryParameters['year'] ?? '');
                  final month = int.tryParse(state.uri.queryParameters['month'] ?? '');
                  return BudgetVsActualDetailPage(year: year ?? DateTime.now().year, month: month ?? DateTime.now().month);
                },
              ),
            ],
          ),
          GoRoute(
            path: '/credit-notes',
            name: 'credit-notes',
            builder: (_, _) => const CreditNoteListPage(),
            routes: [
              GoRoute(path: 'new/:saleId', name: 'credit-note-new', builder: (_, state) => CreditNoteFormPage(saleId: state.pathParameters['saleId']!)),
            ],
          ),
          GoRoute(
            path: '/approval-flows',
            name: 'approval-flows',
            builder: (_, _) => const ApprovalFlowListPage(),
            routes: [
              GoRoute(path: 'new', name: 'approval-flow-new', builder: (_, _) => const ApprovalFlowFormPage()),
              GoRoute(path: ':flowId/edit', name: 'approval-flow-edit', builder: (_, state) => ApprovalFlowFormPage(flowId: state.pathParameters['flowId']!)),
            ],
          ),
          GoRoute(
            path: '/approval-pending',
            name: 'approval-pending',
            builder: (_, _) => const ApprovalPendingPage(),
          ),
          GoRoute(
            path: '/suppliers',
            name: 'suppliers',
            builder: (_, _) => const SupplierListPage(),
            routes: [
              GoRoute(path: 'new', name: 'supplier-new', builder: (_, _) => const SupplierFormPage()),
              GoRoute(path: ':supplierId/edit', name: 'supplier-edit', builder: (_, state) => SupplierFormPage(supplierId: state.pathParameters['supplierId']!)),
            ],
          ),
          GoRoute(
            path: '/inventory-movements',
            name: 'inventory-movements',
            builder: (_, _) => const InventoryMovementListPage(),
          ),
          GoRoute(
            path: '/credits',
            name: 'credits',
            builder: (_, _) => const CreditListPage(),
            routes: [
              GoRoute(path: ':creditId', name: 'credit-detail', builder: (_, state) => CreditDetailPage(creditId: state.pathParameters['creditId']!)),
              GoRoute(path: ':creditId/refinancing', name: 'credit-refinancing', builder: (_, state) => CreditRefinancingFormPage(creditId: state.pathParameters['creditId']!)),
            ],
          ),
          GoRoute(
            path: '/credits/overdue-dashboard',
            name: 'overdue-dashboard',
            builder: (_, _) => const OverdueDashboardPage(),
          ),
          GoRoute(
            path: '/cash-registers',
            name: 'cash-registers',
            builder: (_, _) => const CashRegisterListPage(),
            routes: [
              GoRoute(path: ':registerId', name: 'cash-register-detail', builder: (_, state) => CashRegisterDetailPage(registerId: state.pathParameters['registerId']!),
                routes: [
                  GoRoute(path: 'arqueo', name: 'cash-register-arqueo', builder: (_, state) => CashRegisterArqueoPage(registerId: state.pathParameters['registerId']!)),
                ],
              ),
            ],
          ),
          GoRoute(
            path: '/treasury',
            name: 'treasury',
            builder: (_, _) => const TreasuryDashboardPage(),
            routes: [
              GoRoute(
                path: 'checks',
                name: 'treasury-checks',
                builder: (_, _) => const CheckIssuancePage(),
              ),
              GoRoute(
                path: 'transfers',
                name: 'treasury-transfers',
                builder: (_, _) => const BankTransferPage(),
              ),
              GoRoute(
                path: 'deposits',
                name: 'treasury-deposits',
                builder: (_, _) => const BankDepositPage(),
              ),
              GoRoute(
                path: 'commissions',
                name: 'treasury-commissions',
                builder: (_, _) => const BankCommissionPage(),
              ),
              GoRoute(
                path: 'collections',
                name: 'treasury-collections',
                builder: (_, _) => const BankCollectionPage(),
              ),
            ],
          ),
          GoRoute(
            path: '/warranties',
            name: 'warranties',
            builder: (_, _) => const WarrantyDashboardPage(),
            routes: [
              GoRoute(path: 'list', name: 'warranty-list', builder: (_, _) => const WarrantyListPage()),
              GoRoute(path: 'new', name: 'warranty-new', builder: (_, _) => const WarrantyFormPage()),
              GoRoute(path: ':warrantyId', name: 'warranty-detail', builder: (_, state) => WarrantyDetailPage(warrantyId: state.pathParameters['warrantyId']!)),
              GoRoute(path: ':warrantyId/edit', name: 'warranty-edit', builder: (_, state) => WarrantyFormPage(warrantyId: state.pathParameters['warrantyId']!)),
            ],
          ),
          GoRoute(
            path: '/workshops',
            name: 'workshops',
            builder: (_, _) => const WorkshopListPage(),
            routes: [
              GoRoute(path: 'new', name: 'workshop-new', builder: (_, _) => const WorkshopFormPage()),
              GoRoute(path: ':workshopId', name: 'workshop-detail', builder: (_, state) => WorkshopDetailPage(workshopId: state.pathParameters['workshopId']!)),
              GoRoute(path: ':workshopId/edit', name: 'workshop-edit', builder: (_, state) => WorkshopFormPage(workshopId: state.pathParameters['workshopId']!)),
            ],
          ),
          GoRoute(
            path: '/providers/dashboard',
            name: 'provider-dashboard',
            builder: (_, _) => const ProviderDashboardPage(),
          ),
          GoRoute(
            path: '/providers',
            name: 'providers',
            builder: (_, _) => const ProviderListPage(),
            routes: [
              GoRoute(
                path: 'new',
                name: 'provider-new',
                builder: (_, _) => const ProviderFormPage(),
              ),
              GoRoute(
                path: 'contracts',
                name: 'provider-contracts',
                builder: (_, _) => const ProviderContractsPage(),
              ),
              GoRoute(
                path: 'contracts/new',
                name: 'contract-new',
                builder: (_, state) => ContractFormPage(
                  providerId: state.uri.queryParameters['providerId'],
                ),
              ),
              GoRoute(
                path: 'contracts/:contractId',
                name: 'contract-detail',
                builder: (_, state) => ServiceContractDetailPage(id: state.pathParameters['contractId']!),
              ),
              GoRoute(
                path: 'contracts/:contractId/edit',
                name: 'contract-edit',
                builder: (_, state) => ContractFormPage(
                  contractId: state.pathParameters['contractId']!,
                ),
              ),
              GoRoute(
                path: 'payments',
                name: 'provider-payments',
                builder: (_, _) => const ProviderInvoicesPage(),
              ),
              GoRoute(
                path: ':providerId',
                name: 'provider-detail',
                builder: (_, state) => ProviderDetailPage(id: state.pathParameters['providerId']!),
                routes: [
                  GoRoute(
                    path: 'edit',
                    name: 'provider-edit',
                    builder: (_, state) => ProviderFormPage(
                      providerId: state.pathParameters['providerId']!,
                    ),
                  ),
                ],
              ),
            ],
          ),
          GoRoute(
            path: '/purchases',
            name: 'purchases',
            builder: (_, _) => const PurchaseListPage(),
            routes: [
              GoRoute(path: 'new', name: 'purchase-new', builder: (_, _) => const PurchaseFormPage()),
              GoRoute(path: ':purchaseId', name: 'purchase-detail', builder: (_, state) => PurchaseDetailPage(purchaseId: state.pathParameters['purchaseId']!)),
              GoRoute(
                path: 'payments',
                name: 'purchase-payments',
                builder: (_, _) => const SupplierPaymentListPage(),
                routes: [
                  GoRoute(path: 'new', name: 'purchase-payment-new', builder: (_, state) {
                    final extra = state.extra as Map<String, dynamic>?;
                    return SupplierPaymentFormPage(purchaseId: extra?['purchaseId'] as String?);
                  }),
                ],
              ),
              GoRoute(
                path: 'credit-notes',
                name: 'purchase-credit-notes',
                builder: (_, _) => const SupplierCreditNoteListPage(),
                routes: [
                  GoRoute(path: 'new', name: 'purchase-credit-note-new', builder: (_, state) {
                    final extra = state.extra as Map<String, dynamic>?;
                    return SupplierCreditNoteFormPage(purchaseId: extra?['purchaseId'] as String?);
                  }),
                ],
              ),
            ],
          ),
          GoRoute(
            path: '/purchase-orders',
            name: 'purchase-orders',
            builder: (_, _) => const PurchaseOrderListPage(),
            routes: [
              GoRoute(path: 'new', name: 'purchase-order-new', builder: (_, _) => const PurchaseOrderFormPage()),
              GoRoute(path: ':purchaseOrderId', name: 'purchase-order-detail', builder: (_, state) => PurchaseOrderDetailPage(purchaseOrderId: state.pathParameters['purchaseOrderId']!)),
            ],
          ),
          GoRoute(
            path: '/inventory-adjustment',
            name: 'inventory-adjustment',
            builder: (_, _) => const InventoryAdjustmentPage(),
          ),
          GoRoute(
            path: '/bi',
            name: 'bi',
            redirect: (_, _) => '/bi/executive',
          ),
          GoRoute(
            path: '/bi/executive',
            name: 'bi-executive',
            builder: (_, _) => const bi.ExecutiveDashboardPage(),
          ),
          GoRoute(
            path: '/bi/financial',
            name: 'bi-financial',
            builder: (_, _) => const FinancialDashboardPage(),
          ),
          GoRoute(
            path: '/bi/commercial',
            name: 'bi-commercial',
            builder: (_, _) => const CommercialDashboardPage(),
          ),
          GoRoute(
            path: '/bi/operational',
            name: 'bi-operational',
            builder: (_, _) => const OperationalDashboardPage(),
          ),
          GoRoute(
            path: '/exchange-rates',
            name: 'exchange-rates',
            builder: (_, _) => const ExchangeRateListPage(),
            routes: [
              GoRoute(
                path: 'new',
                name: 'exchange-rate-new',
                builder: (_, _) => const ExchangeRateFormPage(),
              ),
              GoRoute(
                path: ':exchangeRateId/edit',
                name: 'exchange-rate-edit',
                builder: (_, state) => ExchangeRateFormPage(
                  exchangeRateId: state.pathParameters['exchangeRateId']!,
                ),
              ),
            ],
          ),
          GoRoute(
            path: '/custom-reports',
            name: 'custom-reports',
            builder: (_, _) => const CustomReportListPage(),
            routes: [
              GoRoute(
                path: 'new',
                name: 'custom-report-new',
                builder: (_, _) => const CustomReportBuilderPage(),
              ),
              GoRoute(
                path: ':reportId/edit',
                name: 'custom-report-edit',
                builder: (_, state) => CustomReportBuilderPage(
                  reportId: state.pathParameters['reportId']!,
                ),
              ),
              GoRoute(
                path: 'result',
                name: 'custom-reports-result',
                builder: (_, state) {
                  final extra = state.extra as Map<String, dynamic>;
                  return CustomReportResultPage(
                    reportName: extra['reportName'] as String? ?? '',
                    module: extra['module'] as String? ?? 'sales',
                    fields: extra['fields'] as List<dynamic>? ?? [],
                    columns: (extra['columns'] as List<dynamic>?)?.cast<String>() ?? [],
                    rows: (extra['rows'] as List<dynamic>?)?.cast<Map<String, dynamic>>() ?? [],
                  );
                },
              ),
            ],
          ),
          GoRoute(
            path: '/accounting',
            name: 'accounting',
            redirect: (_, _) => '/accounting/trial-balance',
          ),
          GoRoute(
            path: '/accounting/reports',
            name: 'accounting-reports',
            redirect: (_, _) => '/accounting/reports/equity',
          ),
          GoRoute(
            path: '/accounting/reports/equity',
            name: 'accounting-equity',
            builder: (_, _) => const EquityChangesPage(),
          ),
          GoRoute(
            path: '/accounting/reports/comparative',
            name: 'accounting-comparative',
            builder: (_, _) => const ComparativeReportsPage(),
          ),
          GoRoute(
            path: '/accounting/ai-assistant',
            name: 'accounting-ai-assistant',
            builder: (_, _) => const AiAssistantPage(),
          ),
          GoRoute(
            path: '/accounting/trial-balance',
            name: 'accounting-trial-balance',
            builder: (_, _) => const TrialBalancePage(),
          ),
          GoRoute(
            path: '/accounting/income-statement',
            name: 'accounting-income-statement',
            builder: (_, _) => const IncomeStatementPage(),
          ),
          GoRoute(
            path: '/accounting/chart-of-accounts',
            name: 'accounting-chart-of-accounts',
            builder: (_, _) => const ChartOfAccountsPage(),
          ),
          GoRoute(
            path: '/accounting/periods',
            name: 'accounting-periods',
            builder: (_, _) => const AccountingPeriodsPage(),
          ),
          GoRoute(
            path: '/accounting/entries',
            name: 'accounting-entries',
            builder: (_, _) => const AccountingEntriesPage(),
          ),
          GoRoute(
            path: '/accounting/account-links',
            name: 'accounting-account-links',
            builder: (_, _) => const AccountLinksPage(),
          ),
          GoRoute(
            path: '/accounting/fiscal-years',
            name: 'accounting-fiscal-years',
            builder: (_, _) => const FiscalYearsPage(),
          ),
          GoRoute(
            path: '/reconciliations',
            name: 'reconciliations',
            builder: (_, _) => const ReconciliationListPage(),
            routes: [
              GoRoute(
                path: 'new',
                name: 'reconciliation-new',
                builder: (_, _) => const ReconciliationFormPage(),
              ),
              GoRoute(
                path: ':reconciliationId',
                name: 'reconciliation-detail',
                builder: (_, state) => ReconciliationDetailPage(
                  reconciliationId: state.pathParameters['reconciliationId']!,
                ),
              ),
            ],
          ),
          GoRoute(
            path: '/admin/country-tax-configs',
            name: 'country-tax-configs',
            builder: (_, _) => const CountryTaxConfigPage(),
          ),
          GoRoute(
            path: '/admin/regional-tax-configs',
            name: 'regional-tax-configs',
            builder: (_, _) => const RegionalTaxConfigPage(),
          ),
          GoRoute(
            path: '/webhooks',
            name: 'webhooks',
            builder: (_, _) => const WebhookListPage(),
            routes: [
              GoRoute(
                path: 'new',
                name: 'webhook-new',
                builder: (_, _) => const WebhookFormPage(),
              ),
              GoRoute(
                path: ':webhookId/edit',
                name: 'webhook-edit',
                builder: (_, state) => WebhookFormPage(
                  webhookId: state.pathParameters['webhookId']!,
                ),
              ),
              GoRoute(
                path: ':webhookId/logs',
                name: 'webhook-logs',
                builder: (_, state) => WebhookLogsPage(
                  webhookId: state.pathParameters['webhookId']!,
                ),
              ),
            ],
          ),
          // ── Document Management Module ──
          GoRoute(
            path: '/documents',
            name: 'document-center',
            builder: (_, _) => const DocumentCenterPage(),
            routes: [
              GoRoute(
                path: 'quick-generate',
                name: 'document-quick-generate',
                builder: (_, state) {
                  final extra = state.extra as Map<String, dynamic>?;
                  return QuickGenerateWizardPage(
                    preselectedTemplateId: extra?['preselectedTemplateId'] as String?,
                  );
                },
              ),
              GoRoute(
                path: ':documentId',
                name: 'document-detail',
                builder: (_, state) => DocumentDetailPage(
                  documentId: state.pathParameters['documentId']!,
                ),
              ),
            ],
          ),
          GoRoute(
            path: '/documents/templates/new',
            name: 'template-new',
            builder: (_, _) => const TemplateEditorPage(),
          ),
          GoRoute(
            path: '/documents/templates/:templateId/edit',
            name: 'template-edit',
            builder: (_, state) => TemplateEditorPage(templateId: state.pathParameters['templateId']!),
          ),
          // ── POS (Point of Sale) ──
          GoRoute(
            path: '/pos',
            name: 'pos',
            builder: (_, _) => const PosPage(),
          ),
          // ── CRM (Customer Relationship Management) ──
          GoRoute(
            path: '/crm',
            name: 'crm',
            builder: (_, _) => const CRMPage(),
            routes: [
              GoRoute(
                path: 'leads/:leadId',
                name: 'lead-detail',
                builder: (_, state) => LeadDetailPage(leadId: state.pathParameters['leadId']!),
                routes: [
                  GoRoute(
                    path: 'edit',
                    name: 'lead-edit',
                    builder: (_, state) => LeadFormPage(leadId: state.pathParameters['leadId']!),
                  ),
                ],
              ),
              GoRoute(
                path: 'opportunities/:opportunityId',
                name: 'opportunity-detail',
                builder: (_, state) => OpportunityDetailPage(opportunityId: state.pathParameters['opportunityId']!),
                routes: [
                  GoRoute(
                    path: 'edit',
                    name: 'opportunity-edit',
                    builder: (_, state) => OpportunityFormPage(opportunityId: state.pathParameters['opportunityId']!),
                  ),
                ],
              ),
            ],
          ),
          GoRoute(
            path: '/predictions/sales',
            name: 'predictions-sales',
            builder: (_, _) => const SalesPredictionsPage(),
          ),
          // ── Chat now inside ShellRoute for sidebar/header ──
          GoRoute(
            path: '/chat',
            name: 'chat',
            builder: (_, _) => const ChatPage(),
          ),
          // ── Fleet & Logistics Module ──
          GoRoute(
            path: '/fleet',
            name: 'fleet-dashboard',
            builder: (_, _) => const FleetDashboardPage(),
          ),
          GoRoute(
            path: '/fleet/vehicles',
            name: 'fleet-vehicles',
            builder: (_, _) => const FleetVehicleListPage(),
            routes: [
              GoRoute(
                path: 'new',
                name: 'fleet-vehicle-new',
                builder: (_, _) => const FleetVehicleFormPage(),
              ),
              GoRoute(
                path: ':vehicleId/edit',
                name: 'fleet-vehicle-edit',
                builder: (_, state) => FleetVehicleFormPage(vehicleId: state.pathParameters['vehicleId']!),
              ),
            ],
          ),
          GoRoute(
            path: '/fleet/drivers',
            name: 'fleet-drivers',
            builder: (_, _) => const FleetDriverListPage(),
            routes: [
              GoRoute(
                path: 'new',
                name: 'fleet-driver-new',
                builder: (_, _) => const FleetDriverFormPage(),
              ),
              GoRoute(
                path: ':driverId/edit',
                name: 'fleet-driver-edit',
                builder: (_, state) => FleetDriverFormPage(driverId: state.pathParameters['driverId']!),
              ),
            ],
          ),
          GoRoute(
            path: '/fleet/routes',
            name: 'fleet-routes',
            builder: (_, _) => const FleetRouteListPage(),
            routes: [
              GoRoute(path: 'new', name: 'fleet-routes-new', builder: (_, _) => const FleetRouteFormPage()),
              GoRoute(path: ':routeId/edit', name: 'fleet-routes-edit', builder: (_, state) => FleetRouteFormPage(routeId: state.pathParameters['routeId'])),
            ],
          ),
          GoRoute(
            path: '/fleet/deliveries',
            name: 'fleet-deliveries',
            builder: (_, _) => const FleetDeliveryListPage(),
            routes: [
              GoRoute(path: 'new', name: 'fleet-deliveries-new', builder: (_, _) => const FleetDeliveryFormPage()),
              GoRoute(path: ':deliveryId/edit', name: 'fleet-deliveries-edit', builder: (_, state) => FleetDeliveryFormPage(deliveryId: state.pathParameters['deliveryId'])),
            ],
          ),
          GoRoute(
            path: '/fleet/trips',
            name: 'fleet-trips',
            builder: (_, _) => const FleetTripListPage(),
            routes: [
              GoRoute(path: 'new', name: 'fleet-trips-new', builder: (_, _) => const FleetTripFormPage()),
              GoRoute(path: ':tripId/edit', name: 'fleet-trips-edit', builder: (_, state) => FleetTripFormPage(tripId: state.pathParameters['tripId'])),
            ],
          ),
          GoRoute(
            path: '/fleet/fuel',
            name: 'fleet-fuel',
            builder: (_, _) => const FleetFuelListPage(),
            routes: [
              GoRoute(path: 'new', name: 'fleet-fuel-new', builder: (_, _) => const FleetFuelFormPage()),
              GoRoute(path: ':fuelId/edit', name: 'fleet-fuel-edit', builder: (_, state) => FleetFuelFormPage(fuelId: state.pathParameters['fuelId'])),
            ],
          ),
          GoRoute(
            path: '/fleet/maintenance',
            name: 'fleet-maintenance',
            builder: (_, _) => const FleetMaintenanceListPage(),
            routes: [
              GoRoute(path: 'new', name: 'fleet-maintenance-new', builder: (_, _) => const FleetMaintenanceFormPage()),
              GoRoute(path: ':workOrderId/edit', name: 'fleet-maintenance-edit', builder: (_, state) => FleetMaintenanceFormPage(workOrderId: state.pathParameters['workOrderId'])),
            ],
          ),
          GoRoute(
            path: '/fleet/workshop',
            name: 'fleet-workshop',
            builder: (_, _) => const FleetWorkshopListPage(),
            routes: [
              GoRoute(path: 'new', name: 'fleet-workshop-new', builder: (_, _) => const FleetWorkshopFormPage()),
              GoRoute(path: ':workshopId/edit', name: 'fleet-workshop-edit', builder: (_, state) => FleetWorkshopFormPage(workshopId: state.pathParameters['workshopId'])),
            ],
          ),
          GoRoute(
            path: '/fleet/documents',
            name: 'fleet-documents',
            builder: (_, _) => const FleetDocumentListPage(),
            routes: [
              GoRoute(
                path: 'new',
                name: 'fleet-documents-new',
                builder: (_, state) => FleetDocumentFormPage(extra: state.extra as Map<String, dynamic>?),
              ),
              GoRoute(
                path: ':documentId/edit',
                name: 'fleet-documents-edit',
                builder: (_, state) => FleetDocumentFormPage(documentId: state.pathParameters['documentId']),
              ),
            ],
          ),
          GoRoute(
            path: '/fleet/expenses',
            name: 'fleet-expenses',
            builder: (_, _) => const FleetExpenseListPage(),
            routes: [
              GoRoute(path: 'new', name: 'fleet-expenses-new', builder: (_, _) => const FleetExpenseFormPage()),
              GoRoute(path: ':expenseId/edit', name: 'fleet-expenses-edit', builder: (_, state) => FleetExpenseFormPage(expenseId: state.pathParameters['expenseId'])),
            ],
          ),
          GoRoute(
            path: '/fleet/reports',
            name: 'fleet-reports',
            builder: (_, _) => const FleetReportsPage(),
          ),
          GoRoute(
            path: '/fleet/gps',
            name: 'fleet-gps',
            builder: (_, _) => const FleetGpsPage(),
          ),
          GoRoute(
            path: '/fleet/alerts',
            name: 'fleet-alerts',
            builder: (_, _) => const FleetAlertsPage(),
          ),
          GoRoute(
            path: '/fleet/tracking',
            name: 'fleet-tracking',
            builder: (_, _) => const FleetTrackingPage(),
          ),
          GoRoute(
            path: '/fleet/predictive',
            name: 'fleet-predictive',
            builder: (_, _) => const FleetPredictivePage(),
          ),
          GoRoute(
            path: '/fleet/catalogs',
            name: 'fleet-catalogs',
            builder: (_, _) => const FleetCatalogPage(),
          ),
        ],
      ),

      // ── PalmTrack Integration ──
      GoRoute(
        path: '/palmtrack/sync',
        name: 'palmtrack-sync',
        builder: (_, _) => const PalmTrackSyncPage(),
      ),        GoRoute(
          path: '/palmtrack/conflicts',
          name: 'palmtrack-conflicts',
          builder: (_, _) => const PalmTrackConflictsPage(),
        ),
        GoRoute(
          path: '/palmtrack/production',
          name: 'palmtrack-production',
          builder: (_, _) => const PalmTrackProductionPage(),
        ),
        GoRoute(
          path: '/palmtrack/farms',
          name: 'palmtrack-farms',
          builder: (_, _) => const PalmTrackFarmsPage(),
        ),
        GoRoute(
          path: '/palmtrack/lots',
          name: 'palmtrack-lots',
          builder: (_, _) => const PalmTrackLotsPage(),
        ),
        GoRoute(
          path: '/palmtrack/producers',
          name: 'palmtrack-producers',
          builder: (_, _) => const PalmTrackProducersPage(),
        ),
        GoRoute(
          path: '/palmtrack/sales',
          name: 'palmtrack-sales',
          builder: (_, _) => const PalmTrackSalesPage(),
        ),
        GoRoute(
          path: '/palmtrack/inventory',
          name: 'palmtrack-inventory',
          builder: (_, _) => const PalmTrackInventoryPage(),
        ),
        GoRoute(
          path: '/palmtrack/settings',
          name: 'palmtrack-settings',
          builder: (_, _) => const PalmTrackSettingsPage(),
        ),
    ],
  );
});
