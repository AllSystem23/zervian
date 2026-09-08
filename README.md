# Zorvian ERP

**"Tan simple como una aplicación moderna, tan robusto como un ERP empresarial."**

Zorvian ERP es un sistema ERP moderno multiplataforma construido con **Flutter** (frontend) y **ASP.NET Core 9** (backend), diseñado para empresas de Centroamérica y el Caribe.

**Países soportados:** Nicaragua · Costa Rica · Guatemala · Honduras · El Salvador · Panamá

---

## Stack Tecnológico

| Capa | Tecnología | Notas |
|------|-----------|-------|
| **Frontend** | Flutter 3.x (SDK ^3.12.0) · Riverpod 2.x · GoRouter · Material 3 | ✅ Implementado |
| **Backend** | ASP.NET Core 9 · EF Core 9 · Clean Architecture · FluentValidation | ✅ Implementado |
| **Base de Datos** | PostgreSQL 16 (Neon) · Row-Level Security (RLS) · 13 migraciones | ✅ Implementado |
| **Cache** | Redis 7 (IDistributedCache) · InMemoryCacheService (fallback automático) | ✅ RedisCacheService con auto-fallback a InMemoryCache cuando Redis no está configurado |
| **Message Queue** | Hangfire (persistencia PostgreSQL) · RabbitMQ 3.13 via MassTransit | ✅ Hangfire activo. RabbitMQ integrado con MassTransit: 4 consumers (SaleCreated, SaleCancelled, PaymentReceived, EmployeeCreated) con retry policy + circuit breaker |
| **Auth** | Firebase Auth + JWT (1h access / 7d refresh) · API Keys · MFA | ✅ MFA backend + frontend UI (verificación login + configuración con QR local) |
| **Realtime** | SignalR · WebSocket | ✅ Implementado (NotificationHub) |
| **Jobs** | Hangfire (15 jobs: 11 recurring + 4 ad-hoc/triggered) | ✅ Jobs distribuidos en Web.Jobs y Application.Jobs |
| **AI/ML** | ML.NET · Vertex AI (ChatService) · RAG (EmbeddingService) · OCR (OcrService) | ⚠️ XGBoost/pgvector documentados pero no implementados en código |
| **Docs/PDF** | QuestPDF · ESC/POS · QR · Thermal Printing | ✅ Implementado |
| **Offline** | SQLite (Drift) · SyncEngine · ConnectivityMonitor | ✅ Implementado para productos, cotizaciones y créditos (3 repositorios locales) |
| **Observability** | OpenTelemetry SDK (tracing + metrics + OTLP) · Serilog (console + Elasticsearch + file) · Sentry (frontend) | ✅ OpenTelemetry con instrumentación ASP.NET Core, HTTP, EF Core. Serilog con 3 sinks |
| **CI/CD** | GitHub Actions (1 pipeline consolidado, 5 jobs) | ✅ Implementado |
| **Container** | Docker Multi-stage + Docker Compose (6 servicios: postgres, redis, rabbitmq, api, migrate, frontend) | ✅ Implementado |
| **Edge** | CloudFlare (configuración externa) | ⚠️ Configurado a nivel DNS/WAF, no en código |
| **Hosting** | Firebase Hosting (frontend) · Render.com (backend) | ✅ Implementado |

---

## Identidad Visual

| Rol | Color | HEX |
|-----|-------|-----|
| Primario | Deep Violet-Navy | `#1A0A3E` |
| Secundario | Cyan Eléctrico | `#00E5FF` |
| Accent | Medium Violet | `#7C4DFF` |
| Éxito | Green Gemini | `#00C853` |
| Advertencia | Amber Alert | `#FF6D00` |
| Z-IA | Purple Aura | `#B388FF` |
| CRM | Cyan Comercial | `#00BCD4` |
| Ventas | Sales Blue | `#2979FF` |
| Finanzas | Green Bosque | `#1B5E20` |
| Tesorería | Treasury Gold | `#FFD54F` |
| Inventario | Amber Logístico | `#FF8F00` |
| RRHH | Magenta Talento | `#E040FB` |
| Flota | Blue Fleet | `#2196F3` |
| Garantías | Teal Warranty | `#2EE59D` |
| Admin | Admin Gray | `#607D8B` |

---

## Módulos

| Módulo | Controllers | Services | Páginas FE | Estado |
|--------|:-----------:|:--------:|:----------:|:------:|
| **Multisucursal** | 3 | 2 | 4 | ✅ |
| **Comercial / CRM** | 4 | 4 | 6 | ✅ |
| **Ventas** | 2 | 3 | 3 | ✅ |
| **Cotizaciones** | 1 | 1 | 4 (incl. Kanban) | ✅ |
| **Créditos** | 1 | 3 | 4 | ✅ |
| **Notas de Crédito** | 1 | 1 | 2 | ✅ |
| **Caja / Arqueo** | 1 | 1 | 3 | ✅ |
| **Tesorería** | 4 | 4 | 6 | ✅ |
| **Reconciliación Bancaria** | 1 | 1 | 3 | ✅ |
| **Tipo de Cambio** | 1 | 1 | 2 | ✅ |
| **Contabilidad** | 10 | 8 | 10 | ✅ |
| **Presupuestos** | 2 | 2 | 4 | ✅ |
| **Centros de Costo** | 1 | 1 | 2 | ✅ |
| **Activos Fijos** | 2 | 3 | — | ✅ |
| **Inventario / Productos** | 4 | 4 | 7 | ✅ |
| **Categorías / Marcas** | 2 | 2 | 4 | ✅ |
| **Compras / Proveedores** | 4 | 5 | 10 | ✅ |
| **Prestadores de Servicio** | 3 | 3 | 8 | ✅ |
| **RRHH / Empleados** | 3 | 4 | 3 | ✅ |
| **Departamentos** | 1 | 1 | 2 | ✅ |
| **Nómina** | 3 | 6 | 6 | ✅ |
| **Asistencia / Kiosk** | 2 | 1 | 4 | ✅ |
| **Vacaciones** | 1 | 1 | 3 | ✅ |
| **Permisos** | 1 | 1 | 3 | ✅ |
| **Incapacidades** | 1 | 1 | 2 | ✅ |
| **Biometría** | 1 | — | 1 | ✅ |
| **Flota / Logística** | 28 | 29 | 27 | ✅ |
| **Garantías** | 10 | 12 | 5 | ✅ |
| **Talleres** | 2 | 2 | 3 | ✅ |
| **Metas / Performance** | 3 | 4 | 3 | ✅ |
| **BI / Analytics** | 3 | 4 | 4 | ✅ |
| **IA (Z-IA) / ML** | 3 | 6 | 1 | ✅ |
| **Reportes Custom** | 1 | 1 | 3 | ✅ |
| **Reportes / Auditoría** | 2 | 2 | 2 | ✅ |
| **Documentos** | 1 | 3 | 4 | ✅ |
| **Aprobaciones** | 2 | 2 | 3 | ✅ |
| **Webhooks** | 1 | 1 | 3 | ✅ |
| **API Keys** | 1 | 1 | — | ✅ |
| **Facturación Electrónica** | 1 | 2 | — | ✅ |
| **Chat Interno** | 1 | 1 | 1 | ✅ |
| **POS** | 1 | — | 1 | ✅ |
| **Offline / Sync** | 1 | 1 | — | ✅ |
| **Predicciones** | 1 | 1 | 1 (ruta /predictions/sales integrada en router) | ✅ |
| **Admin (Usuarios, Planes)** | 4 | 3 | 5 | ✅ |
| **Config Fiscal** | 2 | 2 | 2 | ✅ |
| **Ajustes / Settings** | 1 | 1 | 3 | ✅ |
| **Sucursales** | 1 | 1 | 2 | ✅ |
| **Profile / Onboarding** | — | — | 3 | ✅ |
| **Splash / Auth** | — | — | 5 | ✅ |
| **404 / 403** | — | — | 2 | ✅ |

---

## Navegación (Sidebar)

**10 módulos · 71 items · 5 grupos**

### Operaciones
| Módulo | Items |
|--------|-------|
| **Ventas** | Cotizaciones, Facturación/Ventas, Créditos y Cobros, Dashboard Vencimientos, Cartera de Clientes, Notas de Crédito, Punto de Venta, CRM |
| **Inventario** | Productos, Categorías, Marcas, Movimientos de Inventario, Ajustes de Inventario, Garantías, Talleres |
| **Compras** | Facturas de Compra, Órdenes de Compra, Proveedores, Pagos a Proveedores, Notas de Crédito a Proveedores |
| **Flota y Logística** | Dashboard, Vehículos, Conductores, Rutas, Entregas, Viajes, Combustible, Mantenimientos, Taller, Documentos, Gastos, GPS, Alertas, Tracking Entregas, IA Predictiva, Catálogos, Reportes |

### Financiero
| Módulo | Items |
|--------|-------|
| **Finanzas** | Caja, Tesorería y Bancos, Contabilidad, Catálogo de Cuentas, Tipo de Cambio, Presupuestos, Presupuesto vs Real, Conciliaciones Bancarias, Centros de Costo |

### Talento
| Módulo | Items |
|--------|-------|
| **Talento Humano** | Capital Humano, Reloj y Asistencia, Gestión de Nómina, Prestadores Externos, Dashboard Prestadores, Ausencias y Vacaciones, Permisos, Metas e Incentivos |

### Inteligencia
| Módulo | Items |
|--------|-------|
| **BI e Inteligencia** | Asistente Z-IA, Dashboard Financiero, Dashboard Comercial, Dashboard Operacional, Reportes Personalizados |
| **Comunicación** | Centro de Comunicacion |

### Configuración
| Módulo | Items |
|--------|-------|
| **Administración** | Gestión de Empresas, Planes de Suscripción, Usuarios y Seguridad, Sucursales, Motor Documental, Flujos de Aprobación, Webhooks, Ajustes del Sistema, Logs de Auditoría |
| **Config Fiscal** | Config Fiscal por País, Tasas Fiscales Regionales |

---

## Arquitectura

```
Zorvian ERP
├── frontend/                         # Flutter (Web, Android, iOS)
│   └── lib/
│       ├── main.dart                  # Entry point, Firebase init, auth check
│       ├── app/
│       │   ├── app.dart               # MaterialApp.router + ProviderScope
│       │   ├── router.dart            # GoRouter: 130+ rutas, ShellRoute, role redirect
│       │   └── theme.dart             # Material 3 light/dark (Inter font)
│       ├── auth/
│       │   ├── auth_provider.dart     # AuthProvider, auth state machine
│       │   └── tenants_provider.dart  # Tenant/multi-company switcher
│       ├── core/
│       │   ├── navigation/
│       │   │   ├── nav_config.dart    # 46 NavItems, 10 módulos, 5 grupos
│       │   │   └── nav_provider.dart
│       │   ├── widgets/
│       │   │   ├── sidebar/           # sidebar.dart + section.dart + item.dart
│       │   │   ├── bi/                # 10 charts: Bar, Pie, Line, Gauge, KPI, + ML sections
│       │   │   ├── header/            # global_header, breadcrumbs, mobile_bottom_nav
│       │   │   ├── app_shell.dart
│       │   │   ├── command_palette.dart
│       │   │   ├── responsive_layout.dart
│       │   │   ├── skeleton_loader.dart
│       │   │   ├── empty_state.dart
│       │   │   ├── error_handler_widget.dart
│       │   │   └── particle_background.dart
│       │   ├── services/              # SignalR, Notifications, Keyboard, FileSaver, Biometric
│       │   ├── network/               # Dio client, Interceptors, API config
│       │   ├── offline/               # SQLite (Drift), SyncEngine, ConnectivityMonitor
│       │   ├── storage/               # Secure Storage
│       │   ├── providers/             # CompanyBranch, CompanyCurrency
│       │   ├── theme/                 # ThemeModeProvider
│       │   ├── entities/              # Goal*, PaymentMilestone, Service*, Provider*
│       │   ├── mixins/                # AutoRefreshMixin
│       │   ├── error/                 # ErrorNotifier
│       │   └── utils/                 # Formatters, Validators, Debouncer, ExportUtils, CountryConfig, CurrencyColors
│       ├── features/                  # 55 módulos de features
│       │   ├── splash/                # SplashPage (logo + loading)
│       │   ├── login/                 # LoginPage, RegisterPage, ForgotPasswordPage
│       │   ├── dashboard/             # DashboardPage, MobileDashboardPage, AbsenceCalendar
│       │   ├── dashboard_v2/          # DashboardV2Page (role-based)
│       │   ├── executive_dashboard/   # ExecutiveDashboardPage (KPIs)
│       │   ├── sales/                 # SaleList, SaleForm, SaleDetail
│       │   ├── quotes/                # QuoteList, QuoteKanban, QuoteForm, QuoteDetail
│       │   ├── credits/               # CreditList, CreditDetail, CreditRefinancing, OverdueDashboard
│       │   ├── credit_notes/          # CreditNoteList, CreditNoteForm
│       │   ├── clients/               # ClientList, ClientForm, ClientStatement
│       │   ├── crm/                   # CRM, LeadForm, LeadDetail, OpportunityForm, OpportunityDetail
│       │   ├── pos/                   # POS Page
│       │   ├── products/              # ProductList, ProductForm, Kardex, InventoryValuation, InventoryDashboard
│       │   ├── categories/            # CategoryList, CategoryForm
│       │   ├── brands/                # BrandList, BrandForm
│       │   ├── inventory_movements/   # InventoryMovementList
│       │   ├── warranties/            # WarrantyList, WarrantyForm, WarrantyDetail, WarrantyDashboard, WarrantyTracking
│       │   ├── workshops/             # WorkshopList, WorkshopForm, WorkshopDetail
│       │   ├── purchases/             # PurchaseList, PurchaseForm, PurchaseDetail, SupplierPayment, SupplierCreditNote, InventoryAdjustment
│       │   ├── purchase_orders/       # PurchaseOrderList, PurchaseOrderForm, PurchaseOrderDetail
│       │   ├── suppliers/             # SupplierList, SupplierForm
│       │   ├── providers/             # ProviderList, ProviderForm, ProviderDetail, ProviderDashboard, ProviderInvoices, ProviderContracts, ContractForm, ServiceContractDetail
│       │   ├── fleet/                 # 27 páginas: Dashboard, Vehicles, Drivers, Routes, Deliveries, Trips, Fuel, Maintenance, Workshop, Documents, Expenses, GPS, Alerts, Tracking, Predictive, Catalogs, Reports
│       │   ├── accounting/            # ChartOfAccounts, TrialBalance, IncomeStatement, Entries, Periods, FiscalYears, EquityChanges, ComparativeReports, AccountLinks, AI-Assistant
│       │   ├── cash_registers/        # CashRegisterList, CashRegisterDetail, CashRegisterArqueo
│       │   ├── treasury/              # TreasuryDashboard, CheckIssuance, BankTransfer, BankDeposit, BankCommission, BankCollection
│       │   ├── exchange_rates/        # ExchangeRateList, ExchangeRateForm
│       │   ├── budgets/               # BudgetList, BudgetForm, BudgetVsActual, BudgetVsActualDetail
│       │   ├── reconciliations/       # ReconciliationList, ReconciliationForm, ReconciliationDetail
│       │   ├── cost_centers/          # CostCenterList, CostCenterForm
│       │   ├── employees/             # EmployeeList, EmployeeForm, EmployeeDetail
│       │   ├── departments/           # DepartmentList, DepartmentForm
│       │   ├── payroll/               # Payroll, PayrollPeriods, PayrollRunDetail, Salaries, SettlementForm, DeductionTypes
│       │   ├── attendance/            # Attendance, AttendanceHistory, Kiosk, QRCheckin
│       │   ├── permissions/           # PermissionList, PermissionForm, PermissionDetail
│       │   ├── vacations/             # VacationList, VacationForm, VacationDetail
│       │   ├── sick_leave/            # SickLeaveList, SickLeaveForm
│       │   ├── biometrics/            # BiometricUnlock
│       │   ├── goals/                 # GoalsDashboard, GoalsConfig, MyGoals (usa *_screen.dart)
│       │   ├── bi/                    # FinancialDashboard, CommercialDashboard, OperationalDashboard, ExecutiveDashboard
│       │   ├── predictions/           # SalesPredictions
│       │   ├── reports/               # Reports, AuditLogs
│       │   ├── custom_reports/        # CustomReportList, CustomReportBuilder, CustomReportResult
│       │   ├── documents/             # DocumentCenter, DocumentDetail, TemplateEditor, QuickGenerateWizard
│       │   ├── approval/              # ApprovalPending, ApprovalFlowList, ApprovalFlowForm
│       │   ├── webhooks/              # WebhookList, WebhookForm, WebhookLogs
│       │   ├── chat/                  # ChatPage
│       │   ├── admin/                 # UserList, InviteUser, InvitationForm, SuperAdminCompanies, SubscriptionPlans
│       │   ├── settings/              # CompanySettings, LeaveTypes, LeaveTypeForm
│       │   ├── branches/              # BranchList, BranchForm
│       │   ├── fiscal/                # CountryTaxConfig, RegionalTaxConfig
│       │   ├── profile/               # ProfilePage
│       │   ├── onboarding/            # OnboardingPage
│       │   ├── not_found/             # 404
│       │   └── unauthorized/          # 403
│       └── shared/
│           ├── ds/                    # Design System
│           │   ├── ds.dart            # Barrel export
│           │   ├── tokens/            # colors, spacing, typography, shadows, radii, assets (6 archivos)
│           │   └── components/        # 43 componentes Z-*
│           ├── printing/              # PDF, ESC/POS, QR, Thermal, PrintShareSheet
│           │   └── platform/          # 8 archivos platform-specific
│           └── l10n/                  # Localización (app_en.arb, app_es.arb)
├── src/                               # ASP.NET Core Backend
│   ├── Zorvian.Web/
│   │   ├── Controllers/               # 87 core controllers + 28 Fleet + 8 Warranty = 123 total
│   │   ├── Middleware/                # 8 middleware
│   │   ├── Filters/                   # AuditFilter, IdempotentAttribute, ValidationFilter
│   │   ├── Hubs/                      # NotificationHub (SignalR)
│   │   ├── Jobs/                      # 12 Hangfire jobs (Web.Jobs + Application.Jobs)
│   │   ├── Validators/                # SaleValidators, FleetValidators, CreditValidators, DepartmentValidators, EmployeeValidators
│   │   ├── Extensions/                # ApiResponse, ApiVersioning, AuditQuery, BatchOp, Cache, Metrics, Polly, DI
│   │   ├── appsettings*.json          # 4 configs: base, Development, Production, Testing
│   │   └── .env.example               # Template de variables de entorno
│   ├── Zorvian.Core/
│   │   ├── Entities/                  # 165 entities (root) + 30 Fleet entities
│   │   ├── Enums/                     # 9 enums
│   │   ├── Interfaces/                # 141+ interfaces
│   │   ├── Domain/                    # WarrantyStateMachine, GoalEngine, InvalidWarrantyStateTransitionException
│   │   └── Attributes/                # EncryptedAttribute
│   ├── Zorvian.Application/
│   │   ├── Services/                  # 111 application services (incl. Fleet, Warranty, CommissionEngine, GoalEngine)
│   │   │   ├── Fleet/                 # 29 fleet services
│   │   │   ├── PayrollStrategies/     # NicaraguaCalculationStrategy, PayrollStrategyBase
│   │   │   ├── CommissionEngine/      # CommissionCalculator, CommissionEngine, RuleEvaluator
│   │   │   ├── DepreciationCalculators/ # StraightLine, DecliningBalance, SumOfYearsDigits, UnitsOfProduction
│   │   │   └── GoalEngine/            # GoalEngine, GoalEvaluator
│   │   ├── DTOs/                      # 38 directorios de DTOs
│   │   ├── Validators/                # EmployeeValidators, DepartmentValidators
│   │   ├── Interfaces/                # 141+ interfaces (incl. Fleet)
│   │   ├── Mapping/                   # AutoMapper MappingProfile
│   │   ├── Jobs/                      # VacationAutomatedJob, OcrProcessingJob, WarrantySlaMonitorJob
│   │   ├── Config/                    # SubscriptionPlanConfig
│   │   └── Helpers/                   # FiscalYearHelper, StoragePathHelper
│   └── Zorvian.Infrastructure/
│       ├── Data/
│       │   ├── ZorvianDbContext.cs     # EF Core DbContext
│       │   ├── UnitOfWork.cs           # Unit of Work pattern
│       │   ├── TenantContext.cs        # Tenant resolution
│       │   ├── Interceptors/           # 7 interceptors (Audit, Immutability, EntityHistory, FileCleanup, TenantAudit, TenantSession, Encryption)
│       │   └── Seeders/                # 4 seeders (CountryTax, DocumentTemplate, FleetCatalog, SubscriptionPlan)│   ├── Repositories/               # 86 core repositories + 23 Fleet repositories = 109 total
│       ├── Services/                   # 35 infrastructure services
│       └── Migrations/                 # 13 migraciones (Jun-Jul 2026)
├── tests/
│   ├── Zorvian.Tests/                  # 93 archivos de test (xUnit + Moq)
│   │   ├── Services/                   # 68 unit tests
│   │   ├── Controllers/                # 4 controller tests
│   │   ├── Integration/                # 6 integration tests
│   │   ├── Infrastructure/             # 4 interceptor tests
│   │   ├── Middleware/                  # 2 middleware tests
│   │   ├── Jobs/                       # 3 job tests
│   │   ├── Authorization/              # 2 authorization tests
│   │   ├── AccountingIntegrationTests/ # 1 test
│   │   └── PayrollIntegrationTests/    # 2 tests
│   └── load/                           # K6 load testing (zorvian-load-test.js)
├── scripts/
│   ├── init_neon.sql                   # Schema inicial Neon
│   ├── init_neon_full.sql              # Schema completo
│   ├── create_fleet_tables.sql         # Tablas de flota
│   ├── add_companyid_to_all_tables.sql # Migración CompanyId
│   ├── migration.sql                   # Migración general
│   ├── migration_SyncAllEntityColumns.sql # Sync entity columns
│   ├── generate_companyid_sql.py       # Generador de SQL
│   ├── list_tables.csx                 # Script C# para listar tablas
│   └── TableCheck/                     # Console app .NET 9 para verificar tablas
├── docs/
│   ├── diagrams/                       # 12 diagramas Mermaid
│   ├── *.md                            # Auditorías, guías, planes (17+ documentos)
│   ├── *.sql                           # SQL docs (SECURITY_RLS, PARTITIONING, etc.)
│   └── *.pdf                           # architecture_overview.pdf
├── plans/
│   └── phase3-goals-incentives.md      # Plan de implementación de metas
├── frontend/test/                      # 22 archivos de test Flutter
├── frontend/lib/core/firebase_options.dart          # Firebase config
├── frontend/lib/core/firebase_options.template.dart # Firebase config template
├── Dockerfile                          # Multi-stage .NET 9 build
├── docker-compose.yml                  # 5 servicios locales
├── firebase.json                       # Firebase Hosting config
├── Zorvian.sln                         # .NET Solution (4 proyectos)
├── AGENTS.md                           # Project memory & conventions
├── README.md                           # Este archivo
├── SPEC.md                             # Especificación original (legacy "Nexora")
├── INFORME_EVALUACION_ESTRATEGICA.pdf  # Informe estratégico
└── *.png                               # Assets de branding
```

---

## Design System (43 Componentes Z-*)

| Componente | Archivo | Descripción |
|------------|---------|-------------|
| `ZActivityFeed` | z_activity_feed.dart | Feed de actividades |
| `ZAlertCard` | z_alert_card.dart | Tarjeta de alertas |
| `ZAsyncRenderer` | z_async_renderer.dart | Renderizado asíncrono |
| `ZAvatarGroup` | z_avatar_group.dart | Grupo de avatares |
| `ZBadge` | z_badge.dart | Insignias/badges |
| `ZBreadcrumb` | z_breadcrumb.dart | NavegaciónBreadcrumb |
| `ZButton` | z_button.dart | Botón unificado |
| `ZCalendar` | z_calendar.dart | Calendario |
| `ZCard` | z_card.dart | Tarjeta contenedora |
| `ZCommandPalette` | z_command_palette.dart | Paleta de comandos |
| `ZCompanyDropdown` | z_company_dropdown.dart | Dropdown de empresas |
| `ZCompanySwitcher` | z_company_switcher.dart | Selector de empresa |
| `ZConfirmDialog` | z_confirm_dialog.dart | Diálogo de confirmación |
| `ZCountryInfoCard` | z_country_info_card.dart | Info card por país |
| `ZCountrySelector` | z_country_selector.dart | Selector de país |
| `ZCurrencyConverter` | z_currency_converter.dart | Conversor de moneda |
| `ZDataTable` | z_data_table.dart | Tabla de datos (reemplaza DataTable nativo) |
| `ZDateRangeFilter` | z_date_range_filter.dart | Filtro por rango de fechas |
| `ZDocumentViewer` | z_document_viewer.dart | Visor de documentos |
| `ZDropdownFormField` | z_dropdown_form_field.dart | Campo dropdown para formularios |
| `ZEmptyState` | z_empty_state.dart | Estado vacío |
| `ZErrorBoundary` | z_error_boundary.dart | Límite de errores |
| `ZFilterBar` | z_filter_bar.dart | Barra de filtros |
| `ZInfoTooltip` | z_info_tooltip.dart | Tooltip informativo |
| `ZInputFormatter` | z_input_formatter.dart | Formateador de inputs |
| `ZLiveRegion` | z_live_region.dart | Región viva (accesibilidad) |
| `ZLoadingOverlay` | z_loading_overlay.dart | Overlay de carga |
| `ZMainContent` | z_main_content.dart | Contenido principal |
| `ZModal` | z_modal.dart | Modal/dialogo |
| `ZPagination` | z_pagination.dart | Paginación |
| `ZPeriodDropdown` | z_period_dropdown.dart | Selector de período |
| `ZProgress` | z_progress.dart | Barra de progreso |
| `ZQuickActionsFab` | z_quick_actions_fab.dart | FAB de acciones rápidas |
| `ZSearchField` | z_search_field.dart | Campo de búsqueda |
| `ZSelect` | z_select.dart | Select unificado |
| `ZSkeleton` | z_skeleton.dart | Skeleton loader |
| `ZSkipLink` | z_skip_link.dart | Skip link (accesibilidad) |
| `ZStatCard` | z_stat_card.dart | Tarjeta de estadística |
| `ZStepper` | z_stepper.dart | Stepper/pasos |
| `ZTagInput` | z_tag_input.dart | Input de tags |
| `ZTextField` | z_text_field.dart | Campo de texto |
| `ZTimeline` | z_timeline.dart | Línea de tiempo |
| `ZToast` | z_toast.dart | Notificación toast |

**Design Tokens:** `colors.dart` · `spacing.dart` · `typography.dart` · `shadows.dart` · `radii.dart` · `assets.dart`

---

## Backend — Entidades (194 total)

### Core (164 entidades en `src/Zorvian.Core/Entities/`)

**Organización:** BaseEntity, TenantId, User, Role, RolePermission, UserRole, UserTenant, RefreshToken, Company, CompanySettings, CompanyPlanPricing, Branch, Department, Location, Invitation, SubscriptionPlan

**Contabilidad:** Account, AccountingEntry, AccountingPeriod, AccountingRule, AccountingRuleTemplate, AccountLink, FiscalYear, CostCenter

**CRM/Ventas:** Client, Lead, Opportunity, PipelineStage, Partner, CommercialActivity, Sale, SaleDetail, SalePayment, Quote, QuoteDetail, Credit, CreditInstallment, CreditPayment, CreditNote, CreditNoteDetail, CreditRefinancing, LateFee

**Compras:** Purchase, PurchaseDetail, PurchaseOrder, PurchaseOrderDetail, Supplier, SupplierPayment, SupplierCreditNote

**Proveedores:** ProviderInvoice, ProviderBrand, ProviderContact, ServiceProvider, ServiceContract, ServiceWorkshop

**Productos:** Product, Category, Brand, TaxCategory, InventoryMovement

**RRHH:** Employee, EmployeeSalary, EmployeeHistory, EmployeeDocument, EmployeeBankAccount, EmployeeSupervisor, EmployeeLoan, LoanInstallment, EmployeePayrollExemption, PayrollConcept, PayrollConceptDefinition, PayrollPeriod, PayrollRun, PayrollDetail, PayrollDetailConcept, Collaborator

**Asistencia/Tiempo libre:** AttendanceRecord, BiometricRegistration, LeaveType, LeaveBalances, VacationRequest, SickLeaveRecord, PermissionRequest, OvertimeRecord, TerminationRecord

**Finanzas:** Bank, BankAccount, Check, Checkbook, CheckPrintTemplate, CheckAuditTrail, CashRegister, CashRegisterArqueo, CashMovement, CashArqueoDenomination, ExchangeRate, Reconciliation, ReconciliationDetail, Budget, BudgetDetail, BudgetTracking

**Activos Fijos:** FixedAsset, FixedAssetCategory, DepreciationEntry, AssetDisposal, AssetMaintenance, AssetRevaluation

**Metas/Performance:** GoalDefinition, GoalAssignment, GoalProgress, KpiDefinition, KpiRecord, PerformanceEntities, Ranking, Badge

**Comisiones:** CommissionScheme, CommissionRule, CommissionAssignment, CommissionRecord, CommissionType

**Incentivos:** Incentive, IncentivePayment, BonusRecord, BenefitProvision, SalaryAdvance, WageGarnishment

**Garantías:** Warranty, WarrantyClaim, WarrantyEvent, WarrantyStateHistory, WarrantyAttachment, WarrantyCost, WarrantyProvider, WarrantySlaConfig, WarrantyPartRequest, WarrantyPartReceipt, WarrantyPartUsage, WarrantyCommunication, WarrantyRevenueSchedule

**Talleres:** Workshop, WorkshopBrand, WorkshopTechnician

**Documentos:** DocumentTemplate, DocumentVersion, GeneratedDocument, DocumentSignature

**Compliance:** PolicyEntities, ApprovalEntities, ApprovalFlow

**Impuestos:** CountryTaxConfig, RegionalTaxConfiguration, Withholding, ElectronicInvoice

**Integraciones:** WebhookSubscription, WebhookDeliveryLog, ApiKey, DeviceToken, SyncJournal, EntityHistory, AuditLog, SupportSystem, Marketplace

**Presupuestos/Reportes:** CustomReport, DeductionType, IntercompanyTransaction, CollectionAction

### Fleet (30 entidades en `Entities/Fleet/`)

Delivery, DeliveryItem, Document, DocumentType, Driver, DriverInfraction, DriverLicenseCategory, DriverTraining, Expense, ExpenseCategory, ExpenseSubcategory, FailureType, FleetAlert, FleetAlertRule, FuelRefill, FuelType, Geofence, GpsPosition, MaintenanceSchedule, MaintenanceTemplate, Route, RoutePoint, Trip, Vehicle, VehicleBrand, VehicleGeofenceState, VehicleType, WorkOrder, WorkOrderPart, Workshop

---

## Backend — Enums (9)

`CollaboratorType` · `CommissionType` · `ContractStatus` · `GoalStatus` · `LeadStatus` · `QuoteStatus` · `RoleType` · `SaleStatus` · `WarrantyStatus`

---

## Backend — DTOs (38 directorios)

Accounting · Approval · Attendance · Auth · Bi · Biometrics · Branch · CashRegister · Commercial · Common · Company · Consolidation · Credit · Dashboard · Department · Document · ElectronicInvoice · Employee · FixedAssets · Fleet · Goal · Inventory · Mfa · ML · MultiCurrency · Notifications · Partner · Payroll · Performance · Permission · Provider · PurchaseOrder · Report · SubscriptionPlan · Tax · Treasury · Vacation · Warranty

---

## Backend — Controllers (123)

### Core (87 controllers en `Controllers/`)

**Auth/Users:** AuthController, UsersController, RegistrationController, SeedController, SyncController, NotificationsController, BadgesController, DashboardController, KioskController

**Accounting:** AccountsController, AccountingEntriesController, AccountingPeriodsController, AccountLinksController, AccountingAssistantController, FiscalYearsController, CostCentersController, ExpenseClassificationController

**Sales/CRM:** SalesController, QuotesController, LeadsController, OpportunitiesController, PipelineStagesController, ClientsController, CreditNotesController, CreditsController

**Purchasing:** PurchasesController, PurchaseOrdersController, ProductsController, CategoriesController, BrandsController, InventoryMovementsController, SuppliersController, ProvidersController

**Finance:** TreasuryController, BankAccountsController, ExchangeRatesController, ReconciliationController, FinancialReportsController, BiController, KpiController

**HR:** EmployeesController, PayrollController, AttendanceController, VacationsController, SickLeaveController, LeavePermissionsController, TerminationController, SettlementController, PerformanceController, CommissionsController, GoalsController

**Admin:** ApiKeysController, AuditLogsController, WebhooksController, SubscriptionPlansController, InvitationsController, CountryTaxConfigsController, RegionalTaxConfigsController, ElectronicInvoiceController, DocumentsController, ApprovalFlowConfigsController, ApprovalRequestsController, CustomReportsController, ReportsController, ChatController, SalesPredictionController, PurchaseRecommendationController

### Fleet (28 controllers en `Controllers/Fleet/`)

VehiclesController, DriversController, TripsController, RoutesController, GpsController, GeofencesController, FuelRefillsController, FleetExpensesController, MaintenanceSchedulesController, WorkOrdersController, WorkshopsController, FleetAlertsController, FleetDashboardController, FleetReportsController, DeliveriesController, FleetDocumentsController, VehicleBrandsController, VehicleTypesController, FuelTypesController, FailureTypesController, ExpenseCategoriesController, ExpenseSubcategoriesController, DriverLicenseCategoriesController, DriverInfractionsController, DriverTrainingController, MaintenanceTemplatesController, RouteOptimizationController, PredictiveMaintenanceController

### Warranty (8 controllers en `Controllers/`)

WarrantyCommunicationsController, WarrantyCostsController, WarrantyDashboardController, WarrantyPartRequestsController, WarrantyPartUsagesController, WarrantyProfitabilityController, WarrantyProvidersController, WarrantySlaConfigsController

---

## Backend — Services (111 Application + 35 Infrastructure = 146 total)

### Application Services (`Zorvian.Application/Services/`)

**Auth/Cuenta:** AuthService, AccountService, AccountLinkService, AutoAccountingService, CompanyService, BadgeService, PasswordHelper

**CRM:** ClientService, LeadService, OpportunityService, PartnerService

**Ventas:** SaleService, QuoteService, CreditService, CreditNoteService

**Compras:** PurchaseService, PurchaseOrderService, SupplierService, SupplierPaymentService, SupplierCreditNoteService, ProviderService, ProductService, CategoryService, BrandService

**Finanzas:** TreasuryService, BankAccountService, ExchangeRateService, ReconciliationService, FixedAssetService

**Contabilidad:** FinancialReportService, EnhancedReportService, FinancialAssistantService, FiscalService, FiscalIntegrationService, RegionalFinancialDashboardService, PayableAgingService, CurrencyConverter

**RRHH:** EmployeeService, PayrollService, PayrollConceptService, PayrollCalculationFactory, PayrollLocalizationService, PayrollDeductionServices, AttendanceService, VacationService, VacationRecommendationService, SickLeaveService, TerminationService, PermissionService, PolicyService, DepartmentService, CostCenterService, CollaboratorService, CommissionService

**Metas:** GoalService, GoalIntegrationService, GoalEngine

**BI:** BiService, KpiService, CustomReportService, DashboardService, DynamicReportEngine

**Documentos:** DocumentGenerationService, DocumentService, SignatureService

**Impuestos:** TaxCategoryService, CountryTaxConfigService, RegionalTaxConfigService, ElectronicInvoiceService

**Inventario:** InventoryMovementService, InventoryAlertService

**Presupuestos:** BudgetService, BudgetDetailService

**Cash:** CashRegisterService

**Admin:** SubscriptionPlanService, ApprovalEngine, ApprovalFlowConfigService, EntityHistoryHelper

### Fleet Application Services (29 en `Services/Fleet/`)

DeliveryService, DeliveryTrackingService, DocumentTypeService, DriverLicenseCategoryService, DriverService, ExpenseCategoryService, ExpenseSubcategoryService, FailureTypeService, FleetAlertService, FleetDashboardService, FleetDocumentService, FleetExpenseService, FuelAnomalyDetectionService, FuelRefillService, FuelTypeService, GeofenceService, GpsService, MaintenanceScheduleService, MaintenanceTemplateService, PredictiveMaintenanceService, RouteOptimizationService, RouteService, TripService, VehicleBrandService, VehicleService, VehicleTypeService, WorkOrderService, WorkshopService

### Warranty Application Services (12 en `Services/`)

WarrantyService, WarrantyCommunicationService, WarrantyCostService, WarrantyDashboardService, WarrantyPartRequestService, WarrantyPartUsageService, WarrantyProfitabilityReportService, WarrantyProviderService, WarrantySlaConfigService, WarrantyTimelineService

### Infrastructure Services (`Zorvian.Infrastructure/Services/`)

**Auth:** FirebaseAuthService, JwtService, MfaService, ApiKeyService

**Storage:** FirebaseStorageService, LocalFileStorageService

**Communications:** EmailService, FCMNotificationService, ChatService, WebhookService

**AI/ML:** AiDocumentService, OcrService, EmbeddingService, ExpenseClassificationService, SalesPredictionService, PurchaseIntelligenceService, PurchaseRecommendationService, AbsenteeismPredictionService

**Financial:** BankTransferService, AchExportService, ConsolidationService, TreasuryDashboardService, AccountingAssistantService

**Documents:** ReportExportService, ReportService, QuestPdfService, SettlementPdfService, FleetPdfChartService

**Jobs:** HangfireJobScheduler, OrphanFileCleanupService

**Data:** ExcelImportService, SyncService, SeedService

**Other:** EncryptionService, PerformanceService

---

## Backend — Repositories (109 total)

**Core:** 86 repos en `Repositories/` (AccountRepository, SaleRepository, EmployeeRepository, etc.)

**Fleet:** 23 repos en `Repositories/Fleet/` (VehicleRepository, DriverRepository, TripRepository, GpsPositionRepository, etc.)

---

## Backend — Interceptors EF Core (7)

| Interceptor | Función |
|-------------|---------|
| `AuditInterceptor` | Registro de cambios de auditoría |
| `AuditImmutabilityInterceptor` | Protección de logs inmutables |
| `EntityHistoryInterceptor` | Historial de entidades |
| `FileCleanupSaveChangesInterceptor` | Limpieza de archivos huérfanos |
| `TenantAuditInterceptor` | Auditoría por tenant |
| `TenantSessionInterceptor` | Sesión de tenant |
| `EncryptionInterceptor` | Encriptación de datos sensibles |

---

## Backend — Middleware (8)

| Middleware | Función |
|------------|---------|
| `ApiKeyMiddleware` | Autenticación por API Key |
| `CorrelationIdMiddleware` | Trace de requests (X-Correlation-ID) |
| `CsrfMiddleware` | Protección anti-CSRF |
| `GlobalExceptionMiddleware` | Manejo centralizado de errores |
| `RateLimitingMiddleware` | 120 req/min general, 5 req/15min auth |
| `RequestLoggingMiddleware` | Logging de requests/responses |
| `SecurityHeadersMiddleware` | CSP, HSTS, X-Frame-Options, Permissions-Policy |
| `TenantMiddleware` | Resolución y aislamiento multi-tenant |

---

## Backend — Hangfire Jobs (15: 11 recurring + 4 ad-hoc/triggered)

| Job | Tipo |
|-----|------|
| `CheckInReminderJob` | Recordatorio de check-in |
| `DocumentExpirationJob` | Expiración de documentos |
| `AttendancePhotoCleanupJob` | Limpieza de fotos de asistencia |
| `AbsenteeismTrainingJob` | Entrenamiento de ML absentismo |
| `SalesPredictionTrainingJob` | Entrenamiento de ML ventas |
| `ExpenseClassificationTrainingJob` | Entrenamiento de ML gastos |
| `AuditLogCleanupJob` | Limpieza de logs antiguos |
| `DatabaseBackupJob` | Backup de base de datos |
| `FleetAlertJob` | Alertas de flota |
| `FleetExpenseNotificationJob` | Notificación de gastos de flota |
| `HealthCheckJob` | Health check |
| `VacationAutomatedJob` | Procesamiento automático de vacaciones |
| `OcrProcessingJob` | Procesamiento OCR |
| `WarrantySlaMonitorJob` | Monitoreo de SLA de garantías |

---

## Backend — Seeders (5)

| Seeder | Contenido |
|--------|-----------|
| `CountryTaxConfigSeeder` | Configuración fiscal por país (INSS, IR, etc.) |
| `DocumentTemplateSeeder` | 6 plantillas de documentos profesionales |
| `FleetCatalogSeeder` | Marcas, tipos de vehículos, combustible, licencias |
| `SubscriptionPlanSeeder` | Planes: Starter, Professional, Enterprise |
| `SeedService` | Catálogo de cuentas + AccountLinks al crear empresa |

---

## Backend — Migraciones (13)

| # | Migración | Fecha |
|---|-----------|-------|
| 1 | `BaselineSync` | Jun 16, 2026 |
| 2 | `EnableRLS` | Jun 17, 2026 |
| 3 | `FixFleetDocumentTableName` | Jun 18, 2026 |
| 4 | `FixFleetDocumentTable` | Jun 18, 2026 |
| 5 | `AddVariablesToDocumentTemplate` | Jun 22, 2026 |
| 6 | `AddCompanyIdToBaseEntity` | Jun 23, 2026 |
| 7 | `SyncAllEntityColumns` | Jun 25, 2026 |
| 8 | `FixLeaveTypeIndexAndQueryFilter` | Jun 27, 2026 |
| 9 | `AddCountryCodeToProviders` | Jun 27, 2026 |
| 10 | `AddMissingEntities` | Jul 2, 2026 |
| 11 | `PendingModelChanges_Jul2026` | Jul 3, 2026 |
| 12 | `AddReconciliationAndBudgetDetailTracking` | Jul 3, 2026 |
| 13 | `FixPendingModelChanges` | Jul 3, 2026 |

---

## Frontend — Providers Riverpod (92 archivos)

Los providers están distribuidos en los directorios de features. Se listan los módulos con su cantidad de providers:

| Módulo | Providers | Archivos |
|--------|-----------|----------|
| **Accounting** | 4 | accounting_entries_notifier, accounting_provider, assistant_provider, enhanced_reports_provider |
| **Admin** | 2 | user_management_provider, user_provider |
| **Approval** | 1 | approval_provider |
| **Attendance** | 2 | attendance_provider, kiosk_provider |
| **BI** | 1 | bi_provider |
| **Biometrics** | 1 | biometric_provider |
| **Branches** | 1 | branch_provider |
| **Brands** | 1 | brand_provider |
| **Budgets** | 2 | budget_provider, budget_detail_provider |
| **Cash Registers** | 1 | cash_register_provider |
| **Categories** | 1 | category_provider |
| **Chat** | 1 | chat_provider |
| **Clients** | 1 | client_provider |
| **Cost Centers** | 1 | cost_center_provider |
| **Credit Notes** | 1 | credit_note_provider |
| **Credits** | 1 | credit_provider |
| **CRM** | 3 | crm_activity_provider, leads_provider, opportunities_provider |
| **Custom Reports** | 1 | custom_report_provider |
| **Dashboard** | 2 | dashboard_provider, calendar_provider |
| **Dashboard V2** | 1 | role_dashboard_provider |
| **Departments** | 1 | department_provider |
| **Documents** | 1 | document_provider |
| **Employees** | 1 | employee_provider |
| **Exchange Rates** | 1 | exchange_rate_provider |
| **Executive Dashboard** | 1 | executive_dashboard_provider |
| **Fleet** | 17 | dashboard, vehicle, driver, route, delivery, trip, fuel, maintenance, workshop, document, expense, gps, alerts, tracking, predictive, catalog, reports |
| **Goals** | 1 | goal_provider |
| **Inventory Movements** | 1 | inventory_movement_provider |
| **Payroll** | 2 | payroll_provider, settlement_provider |
| **Permissions** | 1 | permission_provider |
| **POS** | 1 | pos_provider |
| **Predictions** | 1 | predictions_provider |
| **Products** | 1 | product_provider |
| **Purchase Orders** | 1 | purchase_order_provider |
| **Purchases** | 1 | purchase_provider |
| **Quotes** | 1 | quote_provider |
| **Reconciliations** | 1 | reconciliation_provider |
| **Reports** | 1 | reports_provider |
| **Sales** | 1 | sale_provider |
| **Settings** | 1 | company_settings_provider |
| **Sick Leave** | 1 | sick_leave_provider |
| **Suppliers** | 1 | supplier_provider |
| **Treasury** | 2 | treasury_provider, treasury_dashboard_provider |
| **Vacations** | 1 | vacation_provider |
| **Warranties** | 2 | warranty_provider, public_warranty_provider |
| **Webhooks** | 1 | webhook_provider |
| **Workshops** | 1 | workshop_provider |

---

## Frontend — Utilities (6)

| Utilidad | Archivo | Función |
|----------|---------|---------|
| `Formatters` | formatters.dart | Formateo de moneda, fechas, números |
| `Validators` | validators.dart | Validación de forms |
| `Debouncer` | debouncer.dart | Debounce de inputs |
| `ExportUtils` | export_utils.dart | Exportación a CSV/Excel |
| `CountryConfig` | country_config.dart | Configuración por país |
| `CurrencyColors` | currency_colors.dart | Colores por moneda |

---

## Frontend — Printing (8+ archivos)

`escpos_builder.dart` · `pdf_generator.dart` · `print_share_sheet.dart` · `print_utils.dart` · `qr_code_dialog.dart` · `thermal_printer_service.dart` · `thermal_template.dart` + 8 archivos platform-specific

---

## Frontend — Offline (7 archivos)

`app_database.dart` (SQLite/Drift) · `base_local_repository.dart` · `connectivity_monitor.dart` · `sync_engine.dart` · `sync_orchestrator.dart` · `product_repository.dart` · `connection_native.dart` · `connection_web.dart`

---

## Inicio Rápido

### Docker Compose (Recomendado)

```bash
git clone https://github.com/AllSystem23/zorvian-erp.git
cd zorvian-erp
cp src/Zorvian.Web/.env.example src/Zorvian.Web/.env
# Edita .env con tus credenciales
docker-compose up -d
curl http://localhost:8080/health
```

**Servicios:**

| Servicio | Puerto | Descripción |
|----------|:------:|-------------|
| `postgres` | 5432 | PostgreSQL 16 Alpine |
| `redis` | 6379 | Redis 7 Alpine |
| `rabbitmq` | 5672, 15672 | RabbitMQ 3.13 via MassTransit (4 consumers: SaleCreated, SaleCancelled, PaymentReceived, EmployeeCreated) |
| `api` | 8080 | Backend .NET 9 |
| `migrate` | — | EF Core migration runner |
| `frontend` | 3000 | Flutter Web (dev server) |

### Desarrollo Local

**Requisitos:** .NET 9 SDK, Flutter 3.x (SDK ^3.12.0), PostgreSQL 16, Redis 7

```bash
# Backend
cd src/Zorvian.Web
dotnet restore
dotnet run

# Frontend
cd frontend
flutter pub get
flutter run -d chrome
```

---

## Variables de Entorno

Copia `.env.example` a `.env` y configura:

```bash
# Base de Datos
ConnectionStrings__ZorvianDb=Host=...;Database=zorvian;Username=...;Password=...;SSL Mode=Require

# JWT
Jwt__Secret=tu-secreto-minimo-32-caracteres
Jwt__ExpirationMinutes=60
Jwt__RefreshExpirationDays=7

# Firebase (Auth + Storage)
Firebase__ProjectId=tu-project-id
Firebase__CredentialsFilePath=path/to/service-account.json
Firebase__WebApiKey=tu-web-api-key
Firebase__StorageBucket=tu-bucket.appspot.com

# Encriptación
Encryption__Key=tu-clave-32-caracteres

# CORS
Cors__AllowedOrigins__0=https://app.zorvian.com
Cors__AllowedOrigins__1=http://localhost:3000

# Google AI (Chatbot, OCR, Predictions)
GoogleAi__ProjectId=tu-project-id
GoogleAi__Location=us-central1

# Testing
Testing__MockExternalServices=false

# Features
Features__Swagger=false
```

---

## Seguridad

- **Autenticación**: Firebase Auth + JWT (1h access / 7d refresh) · API Keys
- **Autorización**: RBAC con permisos granulares (`RequirePermission` en 50+ controllers)
- **Multi-tenant**: Query Filters EF Core + Row-Level Security PostgreSQL (aplicado en migración automática)
- **Edge**: CloudFlare WAF (configuración externa)
- **Auditoría**: Logs inmutables (`AuditImmutabilityInterceptor`) + 7 interceptores EF Core
- **Rate Limiting**: Configurable via `appsettings.json` (deshabilitado por defecto: `"RateLimiting:Enabled": false`). 120 req/min general, 5 req/15min auth
- **Security Headers**: CSP, HSTS, X-Frame-Options, Permissions-Policy
- **Circuit Breaker**: Polly con retry + circuit breaker
- **Validación**: FluentValidation + ValidationFilter global
- **Encriptación**: `EncryptionInterceptor` (AES-256-GCM) + `EncryptedAttribute`
- **API Keys**: Middleware dedicado con `ApiKeyService`
- **MFA**: Backend `MfaService` + frontend UI completa (`MfaLoginPage` + `MfaSettingsPage` con QR local)
- **JWT**: Sin blacklist de tokens revocados actualmente
- **Elasticsearch**: Integrado via Serilog (solo si `Elasticsearch:Uri` está configurado)
- **OpenTelemetry**: SDK configurado con tracing (ASP.NET Core, HTTP, EF Core, MassTransit) y métricas, exportador OTLP condicional

---

## Testing

```bash
# Backend (xUnit + Moq, PostgreSQL test containers)
dotnet test

# Frontend (22 archivos de test)
cd frontend
flutter test --coverage

# Load testing (K6)
cd tests/load
k6 run zorvian-load-test.js
```

**Cobertura:**

| Tipo | Archivos | Cobertura |
|------|:--------:|-----------|
| Unit Tests (Services) | 68 | Auth, Sale, Payroll, Warranty, Fleet, Goal, Commission |
| Controller Tests | 4 | Badges, PublicWarranties, Sync, Webhooks |
| Integration Tests | 6 | AuditLog, Commissions, Consolidation, RLS, Treasury |
| Infrastructure Tests | 4 | AuditInterceptor, EntityHistory, SoftDelete, TenantAudit |
| Middleware Tests | 2 | RateLimiting, SecurityHeaders |
| Job Tests | 3 | AuditLogCleanup, OcrProcessing, WarrantySlaMonitor |
| Authorization Tests | 2 | Companies, RequirePermission |
| Accounting Integration | 1 | Accounting |
| Payroll Integration | 2 | Payroll, Phase7 |
| **Total Backend** | **93** | |
| **Frontend Tests** | **70** | Widgets, Providers, Pages, Utils |
| **Total General** | **115** | |

---

## CI/CD

Pipeline consolidado en `.github/workflows/ci-cd.yml` (229 líneas):

```
push to master/dev ──┬── backend-build ──┬── deploy-backend (Render.com)
                     │                   │
                     ├── security-scan ──┤
                     │                   │
                     └── frontend-build ─┴── deploy-frontend (Firebase Hosting)
```

| Job | Runner | Descripción |
|-----|--------|-------------|
| `backend-build` | ubuntu-latest | Restore → Build → Test (PostgreSQL 16 service) |
| `security-scan` | ubuntu-latest | Detección de secrets hardcodeados |
| `frontend-build` | ubuntu-latest | Flutter analyze → Test → Coverage |
| `deploy-backend` | ubuntu-latest | `dotnet publish` → Render.com webhook |
| `deploy-frontend` | ubuntu-latest | `flutter build web` → Firebase Hosting |

---

## Despliegue

| Componente | Servicio | Configuración |
|------------|----------|---------------|
| **Backend API** | Render.com | `Dockerfile` multi-stage, puerto 8080, non-root user |
| **Frontend** | Firebase Hosting | `firebase.json`, SPA rewrites |
| **Base de Datos** | Neon (PostgreSQL 16) | Connection string en `.env`, RLS habilitado |
| **CDN/WAF** | CloudFlare | WAF rules, caching, SSL |
| **CI/CD** | GitHub Actions | Auto-deploy en push a `master` |

---

## Internacionalización

- **Idiomas:** Español (es) · Inglés (en)
- **Archivos ARB:** `frontend/lib/shared/l10n/app_es.arb`, `app_en.arb`
- **Config por país:** Impuestos, monedas, formatos de fecha/número por país

---

## Capacidades Offline

- **Storage:** SQLite local via Drift (`app_database.dart`)
- **Sync Engine:** `SyncEngine` + `SyncOrchestrator` + `SyncJournal`
- **Connectivity:** `ConnectivityMonitor` (nativo + web)
- **Repositories:** `ProductLocalRepository` + `QuoteLocalRepository` + `CreditLocalRepository` (offline-first para 3 entidades)
- **Tablas locales:** `ProductsLocal` + `QuotesLocal` + `CreditsLocal` (schema v3)

---

## Diagramas de Arquitectura (12)

| Diagrama | Enlace |
|----------|--------|
| Arquitectura General | [architecture_overview.md](docs/diagrams/architecture_overview.md) |
| Z-IA (IA + ML + OCR + Chatbot) | [z_ia_architecture.md](docs/diagrams/z_ia_architecture.md) |
| Pipeline CRM | [crm_pipeline.md](docs/diagrams/crm_pipeline.md) |
| Ciclo Contable | [accounting_cycle.md](docs/diagrams/accounting_cycle.md) |
| Flujo de Tesorería | [treasury_flow.md](docs/diagrams/treasury_flow.md) |
| Kardex y Costeo | [inventory_costing.md](docs/diagrams/inventory_costing.md) |
| Multi-Tenant | [multi_tenant.md](docs/diagrams/multi_tenant.md) |
| Flujo de Nómina | [payroll_flow.md](docs/diagrams/payroll_flow.md) |
| Arquitectura de Seguridad | [security_architecture.md](docs/diagrams/security_architecture.md) |
| Integraciones | [integrations_architecture.md](docs/diagrams/integrations_architecture.md) |
| Facturación Electrónica | [electronic_invoicing.md](docs/diagrams/electronic_invoicing.md) |
| Disaster Recovery | [disaster_recovery.md](docs/diagrams/disaster_recovery.md) |

---

## Documentación Adicional

| Documento | Descripción |
|-----------|-------------|
| [INFORME_AUDITORIA_VISUAL.md](docs/INFORME_AUDITORIA_VISUAL.md) | Auditoría visual completa (6.9→8.5) |
| [PRESENTACION_EJECUTIVA.md](docs/PRESENTACION_EJECUTIVA.md) | Deck para inversores ($1.2B TAM, $500K ARR) |
| [PUBLIC_API.md](docs/PUBLIC_API.md) | Documentación de API pública |
| [PLAN.md](docs/PLAN.md) | Plan general del proyecto |
| [MODULO_CONTABLE_GUIDE.md](docs/MODULO_CONTABLE_GUIDE.md) | Guía del módulo contable |
| [MODULO_FLOTA_TRANSPORTE_LOGISTICA.md](docs/MODULO_FLOTA_TRANSPORTE_LOGISTICA.md) | Guía del módulo flota |
| [MODULO_COMPENSACIONES_NOMINA_PRESTADORES.md](docs/MODULO_COMPENSACIONES_NOMINA_PRESTADORES.md) | Guía de nómina y compensaciones |
| [MOTOR_INTELIGENTE_DOCUMENTAL_ZORVIAN.md](docs/MOTOR_INTELIGENTE_DOCUMENTAL_ZORVIAN.md) | Motor documental con IA |
| [PLAN_ACCION_INTEGRACION.md](docs/PLAN_ACCION_INTEGRACION.md) | Plan de acción de integración |
| [PHASE1_DESIGN.md](docs/PHASE1_DESIGN.md) - [PHASE10_DESIGN.md](docs/PHASE10_DESIGN.md) | Diseños por fase |
| [SPEC.md](SPEC.md) | Especificación original del proyecto |

---

## Scripts

| Script | Descripción |
|--------|-------------|
| `scripts/init_neon.sql` | Schema inicial para Neon |
| `scripts/init_neon_full.sql` | Schema completo |
| `scripts/create_fleet_tables.sql` | Tablas del módulo flota |
| `scripts/add_companyid_to_all_tables.sql` | Migración CompanyId |
| `scripts/migration.sql` | Migración general |
| `scripts/migration_SyncAllEntityColumns.sql` | Sync de columnas |
| `scripts/generate_companyid_sql.py` | Generador de SQL (Python) |
| `scripts/list_tables.csx` | Listar tablas (C# script) |
| `scripts/TableCheck/` | Console app .NET 9 para verificar tablas |

---

## Roadmap 2026-2027

| Fase | Periodo | Entregable |
|------|---------|------------|
| Foundation | Jul-Sep 2026 | Nueva paleta, diagramas C4, Z-IA, CRM, Contable |
| Expansión | Oct 2026-Ene 2027 | Tesorería, Multi-Tenant, Inventory, Dashboard Redesign |
| Enterprise | Feb-Jun 2027 | Seguridad C4, Sidebar/Header redesign, Disaster Recovery |

---

## Calificación de Auditoría Visual (Junio 2026)

| Categoría | Puntaje |
|-----------|:-------:|
| Arquitectura Técnica | 8.0/10 |
| Visual / Diagramas | 6.0 → **8.5/10** |
| Branding / Identidad | 5.5 → **9.0/10** |
| UX/UI Empresarial | 7.0/10 |
| Cobertura Funcional | 8.0/10 |
| Competitividad | 6.5/10 |
| Documentación | 7.0 → **9.0/10** |
| **Global** | **6.9 → 8.5/10** |

---

## Licencia

Uso interno — Zorvian ERP © 2026
