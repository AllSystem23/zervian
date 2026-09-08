import 'package:flutter/material.dart';
import '../../shared/ds/ds.dart';

final class NavItem {
  final String id;
  final String label;
  final IconData icon;
  final String route;
  final List<String> roles;
  final String badgeRef;
  final bool selectedExact;

  const NavItem({
    required this.id,
    required this.label,
    required this.icon,
    required this.route,
    this.roles = const [],
    this.badgeRef = '',
    this.selectedExact = false,
  });
}

final class NavModule {
  final String id;
  final String label;
  final IconData icon;
  final Color color;
  final Color textColor; // WCAG AA compliant text variant for light mode
  final String group; // Operations, Financial, Talent, Intelligence, Admin
  final List<NavItem> children;
  final List<String> roles;

  const NavModule({
    required this.id,
    required this.label,
    required this.icon,
    required this.color,
    this.textColor = const Color(0xFF484854), // default: neutral600
    required this.group,
    required this.children,
    this.roles = const [],
  });
}

final class NavConfig {
  NavConfig._();

  static const Map<String, String> groupLabels = {
    'operations': 'OPERACIONES',
    'financial': 'FINANCIERO',
    'talent': 'TALENTO',
    'intelligence': 'INTELIGENCIA',
    'admin': 'CONFIGURACIÓN',
  };

  static const Map<String, Color> groupColors = {
    'operations': ZColors.moduleSales,
    'financial': ZColors.moduleFinance,
    'talent': ZColors.moduleHr,
    'intelligence': ZColors.moduleIa,
    'admin': ZColors.moduleAdmin,
  };

  /// Darkened group colors for light-mode text (WCAG AA ≥4.5:1 on white)
  static const Map<String, Color> groupTextColors = {
    'operations': ZColors.moduleSalesText,
    'financial': ZColors.moduleFinanceText,
    'talent': ZColors.moduleHrText,
    'intelligence': ZColors.moduleIaText,
    'admin': ZColors.moduleAdminText,
  };

  static Color colorForModule(String moduleId) {
    return allModules.firstWhere((m) => m.id == moduleId, orElse: () => allModules.first).color;
  }

  static final List<NavModule> allModules = [
    // ── OPERACIONES ──
    const NavModule(
      id: 'ventas',
      label: 'Ventas',
      icon: Icons.point_of_sale_outlined,
      color: ZColors.moduleSales,
      textColor: ZColors.moduleSalesText,
      group: 'operations',
      children: [
        NavItem(id: 'cotizaciones', label: 'Cotizaciones', icon: Icons.request_quote_outlined, route: '/quotes'),
        NavItem(id: 'comercial', label: 'Facturación / Ventas', icon: Icons.receipt_long_outlined, route: '/sales'),
        NavItem(id: 'creditos', label: 'Créditos y Cobros', icon: Icons.credit_card_outlined, route: '/credits'),
        NavItem(id: 'creditos-vencidos', label: 'Dashboard Vencimientos', icon: Icons.warning_amber_outlined, route: '/credits/overdue-dashboard', roles: ['SuperAdmin', 'CompanyAdmin', 'Supervisor']),
        NavItem(id: 'clientes', label: 'Cartera de Clientes', icon: Icons.people_alt_outlined, route: '/clients'),
        NavItem(id: 'notas-credito', label: 'Notas de Crédito', icon: Icons.request_quote_outlined, route: '/credit-notes'),
        NavItem(id: 'pos', label: 'Punto de Venta', icon: Icons.storefront_outlined, route: '/pos'),
        NavItem(id: 'crm', label: 'CRM', icon: Icons.handshake_outlined, route: '/crm'),
      ],
    ),

    const NavModule(
      id: 'inventario',
      label: 'Inventario',
      icon: Icons.inventory_2_outlined,
      color: ZColors.moduleInventory,
      textColor: ZColors.moduleInventoryText,
      group: 'operations',
      children: [
        NavItem(id: 'productos', label: 'Productos', icon: Icons.shopping_bag_outlined, route: '/products'),
        NavItem(id: 'categorias', label: 'Categorías', icon: Icons.category_outlined, route: '/categories'),
        NavItem(id: 'marcas', label: 'Marcas', icon: Icons.branding_watermark_outlined, route: '/brands'),
        NavItem(id: 'movimientos-inv', label: 'Movimientos de Inventario', icon: Icons.swap_horiz_outlined, route: '/inventory-movements'),
        NavItem(id: 'ajustes-inv', label: 'Ajustes de Inventario', icon: Icons.tune_outlined, route: '/inventory-adjustment'),
        NavItem(id: 'garantias', label: 'Garantías', icon: Icons.shield_outlined, route: '/warranties'),
        NavItem(id: 'talleres', label: 'Talleres', icon: Icons.build_outlined, route: '/workshops'),
      ],
    ),

    const NavModule(
      id: 'compras',
      label: 'Compras',
      icon: Icons.shopping_cart_outlined,
      color: ZColors.modulePurchases,
      textColor: ZColors.modulePurchasesText,
      group: 'operations',
      children: [
        NavItem(id: 'facturas-compra', label: 'Facturas de Compra', icon: Icons.receipt_long_outlined, route: '/purchases'),
        NavItem(id: 'compras-ordenes', label: 'Órdenes de Compra', icon: Icons.receipt_outlined, route: '/purchase-orders'),
        NavItem(id: 'proveedores', label: 'Proveedores', icon: Icons.factory_outlined, route: '/suppliers'),
        NavItem(id: 'pagos-proveedores', label: 'Pagos a Proveedores', icon: Icons.payments_outlined, route: '/purchases/payments'),
        NavItem(id: 'notas-credito-proveedores', label: 'Notas de Crédito a Proveedores', icon: Icons.request_quote_outlined, route: '/purchases/credit-notes'),
      ],
    ),

    const NavModule(
      id: 'flota',
      label: 'Flota y Logística',
      icon: Icons.local_shipping_outlined,
      color: ZColors.moduleFleet,
      textColor: ZColors.moduleFleetText,
      group: 'operations',
      children: [
        NavItem(id: 'flota-dashboard', label: 'Dashboard', icon: Icons.dashboard_outlined, route: '/fleet', selectedExact: true),
        NavItem(id: 'flota-vehiculos', label: 'Vehículos', icon: Icons.directions_car_outlined, route: '/fleet/vehicles'),
        NavItem(id: 'flota-conductores', label: 'Conductores', icon: Icons.person_outline, route: '/fleet/drivers'),
        NavItem(id: 'flota-rutas', label: 'Rutas', icon: Icons.alt_route_outlined, route: '/fleet/routes'),
        NavItem(id: 'flota-entregas', label: 'Entregas', icon: Icons.local_shipping_outlined, route: '/fleet/deliveries'),
        NavItem(id: 'flota-viajes', label: 'Viajes', icon: Icons.flight_takeoff_outlined, route: '/fleet/trips'),
        NavItem(id: 'flota-combustible', label: 'Combustible', icon: Icons.local_gas_station_outlined, route: '/fleet/fuel'),
        NavItem(id: 'flota-mantenimientos', label: 'Mantenimientos', icon: Icons.build_outlined, route: '/fleet/maintenance'),
        NavItem(id: 'flota-taller', label: 'Taller', icon: Icons.precision_manufacturing_outlined, route: '/fleet/workshop'),
        NavItem(id: 'flota-documentos', label: 'Documentos', icon: Icons.folder_outlined, route: '/fleet/documents'),
        NavItem(id: 'flota-gastos', label: 'Gastos', icon: Icons.account_balance_wallet_outlined, route: '/fleet/expenses'),
        NavItem(id: 'flota-gps', label: 'GPS en Tiempo Real', icon: Icons.gps_fixed_outlined, route: '/fleet/gps'),
        NavItem(id: 'flota-alertas', label: 'Alertas', icon: Icons.notifications_active_outlined, route: '/fleet/alerts'),
        NavItem(id: 'flota-tracking', label: 'Tracking Entregas', icon: Icons.route_outlined, route: '/fleet/tracking'),
        NavItem(id: 'flota-predictive', label: 'IA Predictiva', icon: Icons.psychology_outlined, route: '/fleet/predictive'),
        NavItem(id: 'flota-catalogos', label: 'Catálogos', icon: Icons.list_alt_outlined, route: '/fleet/catalogs', roles: ['SuperAdmin', 'CompanyAdmin']),
        NavItem(id: 'flota-reportes', label: 'Reportes', icon: Icons.summarize_outlined, route: '/fleet/reports'),
      ],
    ),

    // ── PalmTrack: módulo independiente de integración externa ──
    const NavModule(
      id: 'palmtrack',
      label: 'PalmTrack',
      icon: Icons.agriculture_outlined,
      color: ZColors.moduleFleet,
      textColor: ZColors.moduleFleetText,
      group: 'operations',
      roles: ['SuperAdmin', 'CompanyAdmin'],
      children: [
        NavItem(id: 'palmtrack-production', label: 'Producción', icon: Icons.agriculture_outlined, route: '/palmtrack/production', selectedExact: true),
        NavItem(id: 'palmtrack-farms', label: 'Fincas', icon: Icons.landscape_outlined, route: '/palmtrack/farms'),
        NavItem(id: 'palmtrack-lots', label: 'Lotes', icon: Icons.grid_view_outlined, route: '/palmtrack/lots'),
        NavItem(id: 'palmtrack-producers', label: 'Productores', icon: Icons.people_outline, route: '/palmtrack/producers'),
        NavItem(id: 'palmtrack-sales', label: 'Ventas', icon: Icons.receipt_long_outlined, route: '/palmtrack/sales'),
        NavItem(id: 'palmtrack-inventory', label: 'Inventario', icon: Icons.inventory_2_outlined, route: '/palmtrack/inventory'),
        NavItem(id: 'palmtrack-sync', label: 'Sincronización', icon: Icons.sync_outlined, route: '/palmtrack/sync'),
        NavItem(id: 'palmtrack-conflicts', label: 'Conflictos', icon: Icons.sync_problem_outlined, route: '/palmtrack/conflicts'),
        NavItem(id: 'palmtrack-settings', label: 'Configuración', icon: Icons.settings_outlined, route: '/palmtrack/settings', roles: ['SuperAdmin']),
      ],
    ),

    // ── FINANCIERO ──
    const NavModule(
      id: 'finanzas',
      label: 'Finanzas',
      icon: Icons.account_balance_outlined,
      color: ZColors.moduleFinance,
      textColor: ZColors.moduleFinanceText,
      group: 'financial',
      children: [
        NavItem(id: 'caja', label: 'Caja', icon: Icons.monetization_on_outlined, route: '/cash-registers'),
        NavItem(id: 'tesoreria', label: 'Tesorería y Bancos', icon: Icons.savings_outlined, route: '/treasury/checks', roles: ['SuperAdmin', 'CompanyAdmin', 'Accountant']),
        NavItem(id: 'contabilidad', label: 'Contabilidad', icon: Icons.balance_outlined, route: '/accounting/trial-balance', roles: ['SuperAdmin', 'CompanyAdmin', 'Accountant']),
        NavItem(id: 'catalogo-cuentas', label: 'Catálogo de Cuentas', icon: Icons.account_tree_outlined, route: '/accounting/chart-of-accounts', roles: ['SuperAdmin', 'CompanyAdmin', 'Accountant']),
        NavItem(id: 'tipo-cambio', label: 'Tipo de Cambio', icon: Icons.currency_exchange_outlined, route: '/exchange-rates'),
        NavItem(id: 'presupuestos', label: 'Presupuestos', icon: Icons.account_balance_wallet_outlined, route: '/budgets', roles: ['SuperAdmin', 'CompanyAdmin', 'Accountant']),
        NavItem(id: 'presupuesto-vs-real', label: 'Presupuesto vs Real', icon: Icons.compare_arrows_outlined, route: '/budgets/vs-actual', roles: ['SuperAdmin', 'CompanyAdmin', 'Accountant']),
        NavItem(id: 'conciliaciones', label: 'Conciliaciones Bancarias', icon: Icons.compare_arrows_outlined, route: '/reconciliations', roles: ['SuperAdmin', 'CompanyAdmin', 'Accountant']),
        NavItem(id: 'centros-costo', label: 'Centros de Costo', icon: Icons.pie_chart_outline, route: '/cost-centers', roles: ['SuperAdmin', 'CompanyAdmin', 'Accountant']),
      ],
    ),

    // ── TALENTO ──
    const NavModule(
      id: 'talento',
      label: 'Talento Humano',
      icon: Icons.diversity_3_outlined,
      color: ZColors.moduleHr,
      textColor: ZColors.moduleHrText,
      group: 'talent',
      children: [
        NavItem(id: 'empleados', label: 'Capital Humano', icon: Icons.people_outline, route: '/employees'),
        NavItem(id: 'asistencia', label: 'Reloj y Asistencia', icon: Icons.schedule_outlined, route: '/attendance'),
        NavItem(id: 'nomina', label: 'Gestión de Nómina', icon: Icons.receipt_long_outlined, route: '/payroll', roles: ['SuperAdmin', 'CompanyAdmin', 'Rrhh']),
        NavItem(id: 'prestadores', label: 'Prestadores Externos', icon: Icons.business_center_outlined, route: '/providers'),
        NavItem(id: 'prestadores-dashboard', label: 'Dashboard Prestadores', icon: Icons.dashboard_outlined, route: '/providers/dashboard'),
        NavItem(id: 'vacaciones', label: 'Ausencias y Vacaciones', icon: Icons.event_busy_outlined, route: '/vacations'),
        NavItem(id: 'permisos', label: 'Permisos', icon: Icons.event_available_outlined, route: '/permissions'),
        NavItem(id: 'metas', label: 'Metas e Incentivos', icon: Icons.emoji_events_outlined, route: '/goals/dashboard'),
      ],
    ),

    // ── INTELIGENCIA ──
    const NavModule(
      id: 'bi',
      label: 'BI e Inteligencia',
      icon: Icons.insights_outlined,
      color: ZColors.moduleIa,
      textColor: ZColors.moduleIaText,
      group: 'intelligence',
      roles: ['SuperAdmin', 'CompanyAdmin'],
      children: [
        NavItem(id: 'asistente-ia', label: 'Asistente Z-IA', icon: Icons.smart_toy_outlined, route: '/accounting/ai-assistant'),
        NavItem(id: 'bi-financiero', label: 'Dashboard Financiero', icon: Icons.candlestick_chart_outlined, route: '/bi/financial'),
        NavItem(id: 'bi-comercial', label: 'Dashboard Comercial', icon: Icons.trending_up_outlined, route: '/bi/commercial'),
        NavItem(id: 'bi-operacional', label: 'Dashboard Operacional', icon: Icons.speed_outlined, route: '/bi/operational'),
        NavItem(id: 'reportes-custom', label: 'Reportes Personalizados', icon: Icons.analytics_outlined, route: '/custom-reports'),
      ],
    ),

    const NavModule(
      id: 'comunicacion',
      label: 'Comunicación',
      icon: Icons.forum_outlined,
      color: ZColors.moduleCrm,
      textColor: ZColors.moduleCrmText,
      group: 'intelligence',
      children: [
        NavItem(id: 'chat', label: 'Centro de Comunicación', icon: Icons.chat_bubble_outline, route: '/chat'),
      ],
    ),

    // ── CONFIGURACIÓN ──
    const NavModule(
      id: 'administracion',
      label: 'Administración',
      icon: Icons.admin_panel_settings_outlined,
      color: ZColors.moduleAdmin,
      textColor: ZColors.moduleAdminText,
      group: 'admin',
      roles: ['SuperAdmin', 'CompanyAdmin'],
      children: [
        NavItem(id: 'empresas', label: 'Gestión de Empresas', icon: Icons.business_outlined, route: '/admin/companies', roles: ['SuperAdmin']),
        NavItem(id: 'planes', label: 'Planes de Suscripción', icon: Icons.workspace_premium_outlined, route: '/admin/subscription-plans', roles: ['SuperAdmin']),
        NavItem(id: 'usuarios', label: 'Usuarios y Seguridad', icon: Icons.manage_accounts_outlined, route: '/admin/users'),
        NavItem(id: 'invitar', label: 'Invitar Usuario', icon: Icons.person_add_outlined, route: '/admin/invite', roles: ['SuperAdmin']),
        NavItem(id: 'invitaciones-historial', label: 'Historial Invitaciones', icon: Icons.history_outlined, route: '/admin/invitations-history', roles: ['SuperAdmin']),
        NavItem(id: 'sucursales', label: 'Sucursales', icon: Icons.store_outlined, route: '/branches'),
        NavItem(id: 'documental', label: 'Motor Documental', icon: Icons.auto_stories_outlined, route: '/documents'),
        NavItem(id: 'aprobaciones', label: 'Flujos de Aprobación', icon: Icons.fact_check_outlined, route: '/approval-pending'),
        NavItem(id: 'webhooks', label: 'Webhooks', icon: Icons.webhook_outlined, route: '/webhooks'),
        NavItem(id: 'configuracion', label: 'Ajustes del Sistema', icon: Icons.settings_outlined, route: '/settings'),
        NavItem(id: 'auditoria', label: 'Logs de Auditoría', icon: Icons.policy_outlined, route: '/audit-logs'),
      ],
    ),

    const NavModule(
      id: 'fiscal',
      label: 'Configuración Fiscal',
      icon: Icons.receipt_long_outlined,
      color: ZColors.moduleFinance,
      textColor: ZColors.moduleFinanceText,
      group: 'admin',
      roles: ['SuperAdmin', 'CompanyAdmin'],
      children: [
        NavItem(id: 'country-tax', label: 'Config Fiscal por País', icon: Icons.policy_outlined, route: '/admin/country-tax-configs', roles: ['SuperAdmin']),
        NavItem(id: 'regional-tax', label: 'Tasas Fiscales Regionales', icon: Icons.receipt_long_outlined, route: '/admin/regional-tax-configs', roles: ['SuperAdmin', 'CompanyAdmin']),
      ],
    ),
  ];

  static List<NavModule> getModulesForRole(String role, {String? searchQuery}) {
    final bypass = role == 'SuperAdmin';

    final filtered = allModules.where((module) {
      if (!bypass && module.roles.isNotEmpty && !module.roles.contains(role)) return false;
      if (searchQuery != null && searchQuery.isNotEmpty) {
        final q = searchQuery.toLowerCase();
        final matchesModule = module.label.toLowerCase().contains(q);
        final matchesChild = module.children.any((item) =>
            item.label.toLowerCase().contains(q) &&
            (bypass || item.roles.isEmpty || item.roles.contains(role)));
        return matchesModule || matchesChild;
      }
      return true;
    }).map((module) {
      if (searchQuery != null && searchQuery.isNotEmpty) {
        final filteredItems = module.children.where((item) {
          final roleOk = bypass || item.roles.isEmpty || item.roles.contains(role);
          final searchOk = item.label.toLowerCase().contains(searchQuery.toLowerCase());
          return roleOk && searchOk;
        }).toList();
        return NavModule(id: module.id, label: module.label, icon: module.icon, color: module.color, textColor: module.textColor, group: module.group, children: filteredItems, roles: module.roles);
      }
      return module;
    }).where((m) => m.children.isNotEmpty).toList();

    return filtered;
  }

  static List<NavModule> getModulesByGroup(List<NavModule> modules) {
    final grouped = <String, List<NavModule>>{};
    for (final m in modules) {
      grouped.putIfAbsent(m.group, () => []).add(m);
    }
    final order = ['operations', 'financial', 'talent', 'intelligence', 'admin'];
    final result = <NavModule>[];
    for (final key in order) {
      if (grouped.containsKey(key)) result.addAll(grouped[key]!);
    }
    return result;
  }
}
