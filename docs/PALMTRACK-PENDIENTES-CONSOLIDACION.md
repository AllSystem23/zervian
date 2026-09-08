# PalmTrack — Pendientes de Consolidación

**Fecha:** 2026-09-07
**Estado:** CONSOLIDACIÓN COMPLETA — 10 de 10 handlers implementados

---

## Resumen

La integración PalmTrack ↔ Zorvian ERP tiene toda la infraestructura funcionando:
- ✅ Webhooks se envían desde PalmTrack post-commit (15 Server Actions)
- ✅ Zorvian recibe, valida (HMAC, idempotencia, org reconciliada) y encola en MassTransit
- ✅ Consumer procesa y delega a `PalmTrackEventMapper`
- ✅ **Fleet consolidado (trip, fuel_log, maintenance)**: 2026-09-07 — resolución por `FleetExternalReference`/`FleetDriverAlias`, anti-duplicados, registro bidireccional. 11 tests nuevos en `PalmTrackEventMapperTests`
- ✅ **Venta/Inventario/Gasto consolidados**: 2026-09-07 — sale → Sale+SaleDetail+kardex (cliente auto-creado, producto SOLO existente), inventory.updated → Product + entry/exit → InventoryMovement, expense → asiento DRAFT (cola de reconciliación contable). 12 tests nuevos adicionales
- ✅ **Mano de obra consolidada**: 2026-09-07 — PalmTrack ahora emite `labor_log.created` (añadido en `createLaborLog`); Zorvian consolida como `AttendanceRecord` por colaborador/día (jornal y actividad en Notes); colaboradores sin Employee → `labor_log_consolidated_partial` (cola de reconciliación de identidad); todos sin resolver → fail-closed. 5 tests nuevos

Los stubs no rompen nada — solo loguean y retornan OK. La consolidación real requiere la decisión A/C por categoría (ver `PalmTrack-Zorvian-Integration-Revision-Ejecutiva-Colecciones.md` §5).

---

## Handlers implementados (no tocar)

| Evento | Handler | Estado | Razón |
|--------|---------|--------|-------|
| `vehicle.created` / `vehicle.updated` | `ProcessVehicleEventAsync` | ✅ Completo | Fleet es dueño de verdad en Zorvian. 2026-09-07: fix de update sin SaveChangesAsync (los updates se perdían) + registro de referencia externa + TenantId/CreatedBy en create |
| `machinery.created` | `ProcessMachineryEventAsync` | ✅ Completo | Delega a Vehicle |
| `production.logged` | `ProcessProductionLoggedAsync` | ✅ Completo | PalmTrack es dueño de verdad |
| `trip.created` / `trip.updated` | `ProcessTripEventAsync` | ✅ Consolidado | 2026-09-07: resuelve Vehicle (referencia o código PT-*) y Driver (alias); requiere alias; anti-duplicados |
| `fuel_log.created` | `ProcessFuelLogEventAsync` | ✅ Consolidado | 2026-09-07: crea FuelRefill con ValidForCalculation=true, PricePerLiter calculado, avanza CurrentKm del vehículo; sin driver en payload → DriverId vacío |
| `maintenance_log.created` | `ProcessMaintenanceLogEventAsync` | ✅ Consolidado | 2026-09-07: correctivo → WorkOrder (Reported); preventivo/revisión → MaintenanceSchedule con NextExecutionDate/LastExecutionDate |

---

## Handlers pendientes (stub → consolidación)

**NINGUNO — consolidación completa (2026-09-07).**

### `labor_log.created` → AttendanceRecord (implementado)
- **Emisor:** `enqueueWebhookEvent('labor_log.created', ...)` añadido en `createLaborLog` de PalmTrack (post-commit, patrón estándar).
- **Receptor:** resuelve colaborador → `Employee` por nombre normalizado (`FirstName + LastName`); PalmTrack permite varios colaboradores por log (separados por coma) → un `AttendanceRecord` por colaborador/día.
- Jornal, actividad, categoría y estatus (Directa/Indirecta) se preservan en `Notes` (con el id externo `PT-{id}` para anti-duplicados).
- **El pago real se liquida en la corrida de nómina** (PayrollRun + INSS/IR) según decisión A/C: Zorvian Payroll es dueño del ciclo administrativo. El AttendanceRecord alimenta el ciclo como registro de asistencia del día.
- Colaboradores sin Employee → `labor_log_consolidated_partial:{nombres}` (cola de reconciliación de identidad, Paso 0); todos sin resolver → `422 labor_log_unresolved` (fail-closed).

---

## Consolidados el 2026-09-07 (referencia de implementación)

### `sale.created` → Sale + SaleDetail + kardex
- Cliente: resuelve por nombre normalizado; si no existe lo **crea** (`PT-CLI-{id}`).
- Producto: **SOLO existente** (por nombre normalizado) — sin match → `422 product_unresolved` (pendiente de reconciliación; no se inventan productos).
- Crea `Sale` (`InvoiceNumber` con la misma secuencia `FAC-yyyyMMdd-####`), `SaleDetailItem`, y descarga stock con `InventoryMovement` (movementType `sale`). Stock nunca negativo; cantidades decimales de PalmTrack se redondean a entero (fricción documentada).
- **Sin asiento contable**: el ciclo administrativo (facturación electrónica, contabilidad) queda en Zorvian según decisión A/C.
- `paymentStatus=Pagado` → `SaleType=cash`, `PaidAmount=Total`; si no → `credit` con `Balance`.

### `inventory.updated` → Product + referencia externa
- Upsert del item: crea `Product` con código `PT-PROD-{id}` o actualiza nombre/unidad/costo/stock (stock PalmTrack = verdad operativa del campo).
- Registra `FleetExternalReference` EntityType=`Product` para que entry/exit resuelvan el producto.

### `inventory.entry.created` / `inventory.exit.created` → InventoryMovement
- Resuelve el producto por referencia externa (`inventoryItemId`) o código `PT-PROD-{id}`; sin match → `422 product_unresolved` (esperando `inventory.updated`).
- Crea `InventoryMovement` (`entry`/`exit`) con `StockBefore/StockAfter` y actualiza el stock del producto (nunca negativo).
- Anti-duplicados por referencia externa del movimiento.

### `expense.created` → AccountingEntry (DRAFT)
- Mapeo categoría → cuenta del plan sembrado: `fixed/administrative` → **6.1.01**, `operative` → **6.1.02**, `unforeseen` → 6.1.01 (categoría original en la descripción).
- Contrapartida: **1.1.01 Caja General**.
- Exige **AccountingPeriod abierto** del mes de la fecha del gasto → si no, `422 accounting_period_closed`.
- Asiento en estado **DRAFT** = la cola de reconciliación contable del doc: visible para revisión/afinación de cuentas antes de postear.
- `payment_proofs` (imágenes en Firebase Storage) pendiente: requiere diseño de almacenamiento en Zorvian.

---

---

## Patrón de implementación

Cuando se implemente cada handler:

1. **Resolver referencias externas** usando `FleetExternalReference` / `FleetDriverAlias` / `ExternalIdentityMapping`
2. **Verificar duplicados** antes de crear (defensa adicional contra duplicados)
3. **Crear entity Zorvian** con campos mapeados
4. **Registrar en `FleetExternalReference`** el mapping bidireccional
5. **Manejar errores** sin re-lanzar (el evento ya fue procesado exitosamente por MassTransit)

---

## Dependencias

| Handler | Requiere Paso 0 (Identidad) | Requiere cola de reconciliación |
|---------|----------------------------|--------------------------------|
| labor_log | ⚠️ Parcial: sin Employee → `labor_log_consolidated_partial` | ⚠️ Pago se liquida en nómina (PayrollRun) |

*(Todos los handlers consolidados el 2026-09-07. Reconciliaciones residuales: asientos DRAFT de gastos, productos `product_unresolved`, colaboradores `labor_log_consolidated_partial` — visibles en logs del consumer)*

---

## Documentos de referencia

- `PalmTrack-Zorvian-Integration-Paso5-Conflictos.md` — Matriz de escritura A/C
- `PalmTrack-Zorvian-Integration-Paso5-Fleet-Mapping.md` — Mapeo Fleet (plantilla)
- `PalmTrack-Zorvian-Integration-Revision-Ejecutiva-Colecciones.md` — Análisis por colección
- `PalmTrack-Zorvian-Integration-Paso4-Zorvian-Receptor.md` — §8.2 EventMapper
