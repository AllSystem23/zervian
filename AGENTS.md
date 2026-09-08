# Zorvian ERP — Project Memory

## Stack
- **Frontend**: Flutter 3.x · Riverpod · GoRouter · Material 3
- **Backend**: ASP.NET Core 9 · EF Core 9 · Clean Architecture · FluentValidation
- **Database**: PostgreSQL 16 (Neon) · Row-Level Security
- **Cache**: Redis 7 (RedisCacheService con IDistributedCache + auto-fallback a InMemoryCacheService)
- **Messaging**: SignalR (NotificationHub) · Hangfire (job scheduler) · **RabbitMQ 3.13 via MassTransit** (4 consumers activos)
- **Jobs**: Hangfire con persistencia PostgreSQL (15 jobs: 11 recurring + 4 ad-hoc)
- **Search/Logs**: Serilog (console + Elasticsearch condicional + file). Elasticsearch integrado via Serilog.Sinks.Elasticsearch
- **Auth**: Firebase Auth + JWT (1h access / 7d refresh). API Keys. MFA (backend + frontend UI)
- **Observability**: OpenTelemetry SDK (tracing + metrics + OTLP). Serilog (console + ES + file). Sentry (frontend). Prometheus/Grafana pendientes
- **AI/ML**: ML.NET · Vertex AI (ChatService) · RAG (EmbeddingService) · OCR (OcrService)
- **Infra**: Render.com · Firebase Hosting · CloudFlare · Docker
- **CI/CD**: GitHub Actions (1 workflow: ci-cd.yml, 5 jobs)
- **Testing**: xUnit y Moq (backend). Flutter test (frontend). K6 (load testing). 93 archivos de test backend

## Documentación: Lo que NO está en el código (actualizar al implementar)
- ❌ **CQRS/MediatR** — Mencionado en SPEC.md pero 0 referencias en código
- ✅ **Elasticsearch** — Integrado via Serilog.Sinks.Elasticsearch (configurable en appsettings.json)
- ❌ **Prometheus/Grafana** — 0 referencias en código
- ✅ **OpenTelemetry SDK** — Implementado en OpenTelemetryExtensions.cs (ASP.NET Core, HTTP, EF Core, OTLP exporter)
- ❌ **XGBoost** — Mencionado en docs pero no implementado en código
- ❌ **pgvector** — Mencionado en docs pero no implementado en código
- ✅ **Redis como cache** — RedisCacheService implementado con IDistributedCache + auto-fallback a InMemoryCache
- ✅ **MFA** — `MfaService` backend + `MfaLoginPage`/`MfaSettingsPage` frontend (qr_flutter para QR local)
- ✅ **Offline-First** — SyncEngine + Drift implementados para productos, cotizaciones y créditos (3 repositorios locales, schema v3)

## RabbitMQ / MassTransit — Consumers implementados
- `SaleCreatedConsumer` — Enqueuea GoalIntegrationService + CommissionService
- `SaleCancelledConsumer` — Enqueuea CommissionService.ClawbackCommissionsBySaleAsync
- `PaymentReceivedConsumer` — Enqueuea GoalIntegrationService + CommissionService
- `EmployeeCreatedConsumer` — Inicializa LeaveBalances + EmployeeSalary
- Config: 3 retries con 5s intervalo, circuit breaker (15 trips / 5min reset)
- Host: configurable via appsettings (default localhost:5672)

## Directory Structure
```
/ (repo root)
├── frontend/lib/
│   ├── app/                    # Router, theme, app entry
│   │   ├── router.dart          # GoRouter: 130+ routes, ShellRoute, role redirect
│   │   └── theme.dart           # Material 3 light/dark, new brand tokens
│   ├── auth/                    # AuthProvider, auth state machine
│   ├── core/
│   │   ├── navigation/
│   │   │   └── nav_config.dart  # 46 NavItems, 9 NavModules, 5 groups, role-filtered
│   │   ├── widgets/
│   │   │   ├── sidebar/         # sidebar.dart + section.dart + item.dart
│   │   │   ├── bi/              # BarChart, PieChart, LineChart, KpiCard, Gauge
│   │   │   ├── responsive_layout.dart
│   │   │   └── command_palette.dart
│   │   ├── theme/              # ThemeModeProvider
│   │   └── services/           # SignalR, Dio, SecureStorage
│   ├── features/
│   │   ├── splash/             # SplashPage (logo + loading on auth check)
│   │   ├── dashboard/          # DashboardPage (v1 redesign with charts)
│   │   ├── dashboard_v2/       # Alternative dashboard
│   │   ├── executive_dashboard/ # Executive KPI view
│   │   ├── login/              # LoginPage + RegisterPage + ForgotPasswordPage
│   │   ├── bi/                 # 4 BI dashboards (exec, financial, commercial, operational)
│   │   ├── goals/              # Goals dashboard
│   │   └── credits/            # Overdue credits dashboard
│   └── shared/ds/
│       ├── tokens/             # colors.dart, spacing.dart, typography.dart, etc.
│       └── components/         # z_button, z_card, z_stat_card, z_data_table, etc.
│
├── src/                        # ASP.NET Core 9 backend
│   ├── Zorvian.Web/            # Controllers, middleware, DI
│   ├── Zorvian.Core/           # Entities, enums
│   ├── Zorvian.Application/    # Services, DTOs, interfaces
│   └── Zorvian.Infrastructure/ # EF Core DbContext, repositories, migrations
│
├── docs/
│   ├── INFORME_AUDITORIA_VISUAL.md  # Full audit (6.93→8.5 roadmap)
│   ├── PRESENTACION_EJECUTIVA.md    # Investor deck ($1.2B TAM, $500K ARR)
│   └── diagrams/                    # 12 Mermaid diagrams
│       ├── architecture_overview.md # v2.0 with API Gateway, LB, WAF
│       ├── z_ia_architecture.md     # Z-IA: RAG, OCR, ML, rules engine
│       ├── crm_pipeline.md          # Lead → qualification → pipeline → funnel
│       ├── accounting_cycle.md      # Full cycle, multi-country policies
│       ├── treasury_flow.md         # Inflows/outflows, reconciliation
│       ├── inventory_costing.md     # Kardex FIFO/avg/specific
│       ├── multi_tenant.md          # 3-phase isolation strategy
│       ├── payroll_flow.md          # INSS/IR → ACH → accounting entry
│       ├── security_architecture.md # Defense in depth, RBAC, compliance
│       ├── integrations_architecture.md # WhatsApp, Email, Webhooks, API
│       ├── electronic_invoicing.md  # Multi-country DGI/Hacienda/SAT flow
│       └── disaster_recovery.md     # PITR, failover, DR site, runbook
│
├── .github/workflows/
│   └── ci-cd.yml              # Full pipeline: build, test, security, deploy
│
└── assets/                    # zorvian_enterprise_logo_refined.png, Zorvian.png
```

## Brand System
- **Primary**: Deep Violet-Navy `#1A0A3E`
- **Secondary**: Cyan Eléctrico `#00E5FF`
- **Accent**: Medium Violet `#7C4DFF` (CTAs, highlights)
- **Teal**: `#2EE59D` (secondary accent)
- **Gold**: `#FFD54F` (premium emphasis)
- **Module colors**: 10 distinct colors (IA, CRM, Sales, Inventory, Purchases, Finance, Treasury, HR, Admin, Security)
- **Gradients**: `brandGradient` (violet→navy→purple), `accentGradient` (cyan→violet→purple)

## Key Conventions
- **Router** (`router.dart`): `ShellRoute` wraps authenticated pages; `GoRoute` per page; `_routeRoles` map for RBAC
- **State**: Riverpod `NotifierProvider` with immutable state + `copyWith`
- **Pages**: `ConsumerStatefulWidget` with `ConsumerState`, loading in `initState` via `addPostFrameCallback`
- **Widgets**: Design System (`ZCard`, `ZButton`, `ZStatCard`, `ZDataTable`, etc.) from `shared/ds/ds.dart`
- **API**: Dio client injected via `ref.read(dioClientProvider)`
- **Sidebar**: 9 modules in 5 groups (Operations, Financial, Talent, Intelligence, Admin)
- **Responsive**: `ResponsiveBuilder`, `ResponsiveGrid`, `ResponsivePadding` (mobile <576px, tablet <992px, desktop 992px+)
- **Accessibility**: `ZSkipLink`, `ZLiveRegion`, semantic labels, keyboard nav

## Important Notes
- The `/splash` page shows Zorvian logo + loading spinner during auth initialization
- Dashboard is NOT in sidebar; it's the default landing page via `/dashboard` redirect
- ZDataTable replaces Flutter's native DataTable; ensure all lists use `ZColumn` not `DataColumn`
- All architecture diagrams live in `docs/diagrams/` (12 files)
- Electronic invoicing supports 6 countries with per-country XML generation
- Current deployment: Render.com (512MB RAM) + Neon PostgreSQL — bottleneck for multi-tenant production
- **Rate limiting**: Configurable via `appsettings.json` pero deshabilitado POR DEFECTO (`"Enabled": false`)
- **JWT**: 1h access / 7d refresh. NO hay blacklist de tokens revocados
- **Build status**: `dotnet build` produce 0 errores, 0 warnings ✅
- **Controllers**: 123 total (87 core + 28 Fleet + 8 Warranty)
- **Application Services**: 111 archivos
- **Flutter Providers**: 92 archivos
- **Repositories**: 109 total (86 core + 23 Fleet)
- Legacy "Nexora" naming appears in `SPEC.md` and some env defaults
- [x] Consolidate 3 CI/CD workflows into 1
- [x] Fix `DataColumn`/`ZColumn` type errors in 5 files
- [x] Add password recovery flow (currently TODO placeholder)
- [x] Add preloader/splash screen on auth check
- [x] Add tenant switcher for multi-company users
- [x] Add `SaleStatus`/`CreditStatus` string constants to prevent typos (`Zorvian.Core/Enums/SaleStatus.cs`)
- [x] Auto-seed chart of accounts + account links on company creation (`SeedService.SeedAsync`)
- [x] Fix account code mismatch: `AccountLinkService.ResolveAccountAsync` tries 3 code formats + name fallback
- [x] Fix account code mismatch: `AutoAccountingService.GetAccountIdByCodeAsync` tries padded variants
- [x] Add name-based fallback in `AutoAccountingService.GetAccountIdAsync` when no AccountLink found
- [x] Add transaction safety to `CreditNoteService.CreateAsync`
- [x] Add `CancelSaleAsync` endpoint (`POST /zorvian/v1/sales/{id}/cancel`)
- [x] Add cancel sale + accounting entries link to frontend `sale_detail_page.dart`
- [x] Handle null `Product.TaxCategory` gracefully in `AutoAccountingService.GenerateSaleEntryAsync`
