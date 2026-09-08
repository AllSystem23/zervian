using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zorvian.Application.Interfaces.PalmTrack;
using Zorvian.Application.Messages;
using Zorvian.Core.Entities;
using Zorvian.Core.Entities.Fleet;
using Zorvian.Core.Enums;
using Zorvian.Infrastructure.Data;

namespace Zorvian.Infrastructure.Services.PalmTrack;

/// <summary>
/// Maps PalmTrack webhook events to Zorvian entity persistence operations.
/// Plan §8.2: dispatches to specific handlers per event type.
/// Sets tenant context for RLS before processing.
/// </summary>
public sealed class PalmTrackEventMapper : IPalmTrackEventMapper
{
    private const string ExternalSystemName = "palmtrack";

    private readonly ZorvianDbContext _db;
    private readonly IPalmTrackIdentityService _identityService;
    private readonly ILogger<PalmTrackEventMapper> _logger;

    public PalmTrackEventMapper(
        ZorvianDbContext db,
        IPalmTrackIdentityService identityService,
        ILogger<PalmTrackEventMapper> logger)
    {
        _db = db;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<MappingResult> ProcessAsync(PalmTrackWebhookReceived message)
    {
        var tenantId = await _identityService.GetTenantIdAsync(message.OrganizationId);
        if (!tenantId.HasValue)
            return MappingResult.Fail($"Organization {message.OrganizationId} not reconciled");

        // Set tenant context for RLS (skip on InMemory for testing)
        try
        {
            _db.Database.ExecuteSql(
                $"SET app.tenant_id = {tenantId.Value.ToString()}");
        }
        catch (InvalidOperationException)
        {
            // InMemory database doesn't support ExecuteSql — skip RLS
        }

        return message.Event.ToLowerInvariant() switch
        {
            "vehicle.created" or "vehicle.updated" => await ProcessVehicleEventAsync(message.Payload, tenantId.Value),
            "trip.created" or "trip.updated" => await ProcessTripEventAsync(message.Payload, tenantId.Value),
            "fuel_log.created" => await ProcessFuelLogEventAsync(message.Payload, tenantId.Value),
            "machinery.created" => await ProcessMachineryEventAsync(message.Payload, tenantId.Value),
            "maintenance_log.created" => await ProcessMaintenanceLogEventAsync(message.Payload, tenantId.Value),
            "sale.created" => await ProcessSaleCreatedAsync(message.Payload, tenantId.Value),
            "production.logged" => await ProcessProductionLoggedAsync(message.Payload, tenantId.Value),
            "inventory.updated" or "inventory.entry.created" or "inventory.exit.created" =>
                await ProcessInventoryEventAsync(message.Payload, tenantId.Value, message.Event),
            "expense.created" => await ProcessExpenseCreatedAsync(message.Payload, tenantId.Value),
            "labor_log.created" => await ProcessLaborLogCreatedAsync(message.Payload, tenantId.Value),
            _ => MappingResult.Fail($"Unhandled event: {message.Event}")
        };
    }

    private async Task<MappingResult> ProcessVehicleEventAsync(JsonElement payload, Guid tenantId)
    {
        var data = TryGetData(payload);
        var externalId = GetRequiredString(data, "id");
        var plate = GetRequiredString(data, "licensePlate");
        var model = data.TryGetProperty("model", out var m) ? m.GetString() : null;
        var year = data.TryGetProperty("year", out var y) && y.ValueKind == JsonValueKind.Number ? y.GetInt32() : (int?)null;
        var brand = data.TryGetProperty("brand", out var b) ? b.GetString() : null;
        var km = data.TryGetProperty("mileage", out var kmEl) && kmEl.ValueKind == JsonValueKind.Number ? kmEl.GetDecimal() : (decimal?)null;

        // Resolve VehicleType
        var typeName = data.TryGetProperty("type", out var t) ? t.GetString() : null;
        var vehicleType = await ResolveVehicleTypeAsync(typeName);

        // Resolve VehicleBrand
        var brandName = data.TryGetProperty("brand", out var br) ? br.GetString() : null;
        var vehicleBrand = await ResolveVehicleBrandAsync(brandName);

        // Resolve FuelType
        var fuelTypeName = data.TryGetProperty("fuelType", out var ft) ? ft.GetString() : null;
        var fuelType = await ResolveFuelTypeAsync(fuelTypeName);

        if (string.IsNullOrWhiteSpace(plate))
            return MappingResult.Fail("422 plate_required");

        var cleanPlate = plate.Replace("-", "").Replace(" ", "").ToUpperInvariant();
        var existing = await _db.Set<Vehicle>()
            .FirstOrDefaultAsync(v =>
                v.Plate.Replace("-", "").Replace(" ", "").ToUpperInvariant() == cleanPlate &&
                !v.IsDeleted);

        Guid vehicleIdForReference;
        if (existing != null)
        {
            if (km.HasValue) existing.CurrentKm = km.Value;
            if (year.HasValue) existing.Year = year.Value;
            if (!string.IsNullOrEmpty(model)) existing.Model = model;
            if (vehicleType != null) existing.VehicleTypeId = vehicleType.Id;
            if (vehicleBrand != null) existing.BrandId = vehicleBrand.Id;
            if (fuelType != null) existing.FuelTypeId = fuelType.Id;

            await _db.SaveChangesAsync();
            vehicleIdForReference = existing.Id;

            _logger.LogInformation("Updated vehicle from PalmTrack: {Plate}", plate);
        }
        else
        {
            var vehicle = new Vehicle
            {
                Code = $"PT-{externalId}",
                Plate = plate,
                Model = model ?? "",
                Year = year ?? 0,
                VehicleTypeId = vehicleType?.Id ?? Guid.Empty,
                BrandId = vehicleBrand?.Id ?? Guid.Empty,
                FuelTypeId = fuelType?.Id ?? Guid.Empty,
                CurrentKm = km ?? 0,
                Status = MapVehicleStatus(data.TryGetProperty("status", out var st) ? st.GetString() : null),
                BranchId = Guid.Empty, // Will need resolution from org
                TenantId = tenantId.ToString(),
                CreatedBy = ExternalSystemName,
            };

            _db.Set<Vehicle>().Add(vehicle);
            await _db.SaveChangesAsync();
            vehicleIdForReference = vehicle.Id;

            _logger.LogInformation("Created vehicle from PalmTrack: {Plate}, Code={Code}", plate, vehicle.Code);
        }

        // Registrar la referencia externa (patrón de consolidación §4) para que
        // trip/fuel/maintenance resuelvan el vehículo por id externo.
        await UpsertExternalReferenceAsync("Vehicle", vehicleIdForReference, externalId, data, tenantId);

        return MappingResult.Ok("vehicle_processed");
    }

    private async Task<MappingResult> ProcessTripEventAsync(JsonElement payload, Guid tenantId)
    {
        var data = TryGetData(payload);
        _logger.LogInformation("Processing trip event from PalmTrack (consolidation)");

        var externalId = GetRequiredString(data, "id");
        var vehicleIdStr = data.TryGetProperty("vehicleId", out var vid) ? vid.GetString() : null;
        var driverName = data.TryGetProperty("driver", out var dr) ? dr.GetString() : null;

        if (string.IsNullOrWhiteSpace(externalId))
            return MappingResult.Fail("422 trip_id_required");

        var vehicle = await ResolveVehicleByExternalIdAsync(vehicleIdStr);
        if (vehicle == null)
            return MappingResult.Fail($"422 vehicle_unresolved:{vehicleIdStr}");

        var driver = await ResolveDriverByAliasAsync(driverName);
        if (driver == null)
            return MappingResult.Fail($"422 driver_alias_unresolved:{driverName}");

        var startDate = TryGetDate(data, "departureDate") ?? DateTime.UtcNow;
        var endDate = TryGetDate(data, "arrivalDate");

        // Duplicados / trip.updated: referencia externa primero, luego convención de código.
        var existing = await FindByReferenceAsync<Trip>("Trip", externalId)
            ?? await _db.Set<Trip>().FirstOrDefaultAsync(t => t.Code == $"PT-TRIP-{externalId}" && !t.IsDeleted);
        if (existing != null)
        {
            existing.VehicleId = vehicle.Id;
            existing.DriverId = driver.Id;
            existing.StartDateTime = startDate;
            existing.EndDateTime = endDate;
            existing.Origin = GetRequiredString(data, "origin");
            existing.Destination = GetRequiredString(data, "destination");
            existing.Status = MapTripStatus(data.TryGetProperty("status", out var st) ? st.GetString() : null);
            existing.Notes = BuildTripNotes(data);
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = ExternalSystemName;
            await _db.SaveChangesAsync();

            await UpsertExternalReferenceAsync("Trip", existing.Id, externalId, data, tenantId);
            _logger.LogInformation("Updated trip from PalmTrack: {ExternalId} -> Trip {TripId}", externalId, existing.Id);
            return MappingResult.Ok("trip_consolidated_updated");
        }

        var trip = new Trip
        {
            Code = $"PT-TRIP-{externalId}",
            VehicleId = vehicle.Id,
            DriverId = driver.Id,
            StartDateTime = startDate,
            EndDateTime = endDate,
            Origin = GetRequiredString(data, "origin"),
            Destination = GetRequiredString(data, "destination"),
            Status = MapTripStatus(data.TryGetProperty("status", out var st2) ? st2.GetString() : null),
            Notes = BuildTripNotes(data),
            TenantId = tenantId.ToString(),
            CreatedBy = ExternalSystemName,
        };

        _db.Set<Trip>().Add(trip);
        await _db.SaveChangesAsync();

        await UpsertExternalReferenceAsync("Trip", trip.Id, externalId, data, tenantId);
        _logger.LogInformation("Created trip from PalmTrack: {ExternalId} -> Trip {TripId}", externalId, trip.Id);
        return MappingResult.Ok("trip_consolidated");
    }

    private async Task<MappingResult> ProcessFuelLogEventAsync(JsonElement payload, Guid tenantId)
    {
        var data = TryGetData(payload);
        _logger.LogInformation("Processing fuel log event from PalmTrack (consolidation)");

        var externalId = GetRequiredString(data, "id");
        var vehicleIdStr = data.TryGetProperty("vehicleId", out var vid) ? vid.GetString() : null;

        if (string.IsNullOrWhiteSpace(externalId))
            return MappingResult.Fail("422 fuel_log_id_required");

        var vehicle = await ResolveVehicleByExternalIdAsync(vehicleIdStr);
        if (vehicle == null)
            return MappingResult.Fail($"422 vehicle_unresolved:{vehicleIdStr}");

        var liters = TryGetDecimal(data, "liters");
        if (!liters.HasValue || liters.Value <= 0)
            return MappingResult.Fail("422 liters_required");

        var cost = TryGetDecimal(data, "cost");
        var date = TryGetDate(data, "date") ?? DateTime.UtcNow;
        var mileage = TryGetDecimal(data, "mileage");

        // FuelType: del payload; si no viene, hereda el del vehículo (puede quedar
        // Guid.Empty si el vehículo tampoco lo tiene — mismo criterio que Vehicle).
        var fuelTypeName = data.TryGetProperty("fuelType", out var ft) ? ft.GetString() : null;
        var fuelType = await ResolveFuelTypeAsync(fuelTypeName);
        var fuelTypeId = fuelType?.Id ?? vehicle.FuelTypeId;

        // PalmTrack no reporta conductor en fuel_logs → DriverId vacío (mismo
        // criterio que Vehicle/BranchId); se puede asignar en reconciliación manual.
        // FuelRefill no tiene Code → defensa anti-duplicados solo por referencia.
        var existing = await FindByReferenceAsync<FuelRefill>("FuelRefill", externalId);
        if (existing != null)
        {
            existing.RefillDateTime = date;
            existing.VehicleId = vehicle.Id;
            existing.FuelTypeId = fuelTypeId;
            existing.Liters = liters.Value;
            existing.TotalCost = cost ?? 0;
            existing.PricePerLiter = cost.HasValue && liters.Value > 0
                ? Math.Round(cost.Value / liters.Value, 4) : 0;
            if (mileage.HasValue) existing.CurrentKm = mileage.Value;
            existing.Observations = BuildFuelObservations(data);
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = ExternalSystemName;
            await _db.SaveChangesAsync();

            await UpsertExternalReferenceAsync("FuelRefill", existing.Id, externalId, data, tenantId);
            _logger.LogInformation("Updated fuel refill from PalmTrack: {ExternalId}", externalId);
            return MappingResult.Ok("fuel_log_consolidated_updated");
        }

        var refill = new FuelRefill
        {
            RefillDateTime = date,
            VehicleId = vehicle.Id,
            DriverId = Guid.Empty,
            FuelTypeId = fuelTypeId,
            Liters = liters.Value,
            TotalCost = cost ?? 0,
            PricePerLiter = cost.HasValue && liters.Value > 0
                ? Math.Round(cost.Value / liters.Value, 4) : 0,
            CurrentKm = mileage ?? vehicle.CurrentKm,
            Observations = BuildFuelObservations(data),
            RefillType = "Full",
            PaymentMethod = "Cash",
            ValidForCalculation = true, // plan §6: entra al cálculo de rendimiento
            TenantId = tenantId.ToString(),
            CreatedBy = ExternalSystemName,
        };

        _db.Set<FuelRefill>().Add(refill);
        await _db.SaveChangesAsync();

        // El odómetro reportado avanza el CurrentKm del vehículo (fuel log es
        // la fuente más frecuente de kilometraje real).
        if (mileage.HasValue && mileage.Value > vehicle.CurrentKm)
        {
            vehicle.CurrentKm = mileage.Value;
            vehicle.UpdatedAt = DateTime.UtcNow;
            vehicle.UpdatedBy = ExternalSystemName;
            await _db.SaveChangesAsync();
        }

        await UpsertExternalReferenceAsync("FuelRefill", refill.Id, externalId, data, tenantId);
        _logger.LogInformation("Created fuel refill from PalmTrack: {ExternalId} -> FuelRefill {RefillId}", externalId, refill.Id);
        return MappingResult.Ok("fuel_log_consolidated");
    }

    private async Task<MappingResult> ProcessMachineryEventAsync(JsonElement payload, Guid tenantId)
    {
        var data = TryGetData(payload);
        _logger.LogInformation("Processing machinery event from PalmTrack (maps to Vehicle per plan §7.1)");
        return await ProcessVehicleEventAsync(payload, tenantId);
    }

    private async Task<MappingResult> ProcessMaintenanceLogEventAsync(JsonElement payload, Guid tenantId)
    {
        var data = TryGetData(payload);
        _logger.LogInformation("Processing maintenance log event from PalmTrack (consolidation per plan §7.2)");

        var externalId = GetRequiredString(data, "id");
        var machineryIdStr = data.TryGetProperty("machineryId", out var mid) ? mid.GetString() : null;
        var maintenanceType = GetRequiredString(data, "maintenanceType").ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(externalId))
            return MappingResult.Fail("422 maintenance_log_id_required");

        var vehicle = await ResolveVehicleByExternalIdAsync(machineryIdStr);
        if (vehicle == null)
            return MappingResult.Fail($"422 vehicle_unresolved:{machineryIdStr}");

        var date = TryGetDate(data, "date") ?? DateTime.UtcNow;
        var nextMaintenance = TryGetDate(data, "nextMaintenance");
        var cost = TryGetDecimal(data, "cost");
        var hourMeter = TryGetDecimal(data, "hourMeter");
        var technician = data.TryGetProperty("technician", out var tech) ? tech.GetString() : null;
        var description = GetRequiredString(data, "description");

        // correctivo → WorkOrder (falla real a atender); preventivo/revision →
        // MaintenanceSchedule (ejecución programada + próxima fecha).
        if (maintenanceType == "correctivo")
        {
            // WorkOrder usa Number (no Code) como clave de negocio.
            var existingWo = await FindByReferenceAsync<WorkOrder>("WorkOrder", externalId)
                ?? await _db.Set<WorkOrder>().FirstOrDefaultAsync(w => w.Number == $"PT-WO-{externalId}" && !w.IsDeleted);
            if (existingWo != null)
            {
                existingWo.ReportDateTime = date;
                existingWo.ProblemDescription = BuildMaintenanceDescription(description, technician, data);
                if (cost.HasValue) existingWo.CostTotal = cost.Value;
                existingWo.UpdatedAt = DateTime.UtcNow;
                existingWo.UpdatedBy = ExternalSystemName;
                await _db.SaveChangesAsync();

                await UpsertExternalReferenceAsync("WorkOrder", existingWo.Id, externalId, data, tenantId);
                _logger.LogInformation("Updated work order from PalmTrack: {ExternalId}", externalId);
                return MappingResult.Ok("maintenance_consolidated_workorder_updated");
            }

            var workOrder = new WorkOrder
            {
                Number = $"PT-WO-{externalId}",
                VehicleId = vehicle.Id,
                ReportDateTime = date,
                ProblemDescription = BuildMaintenanceDescription(description, technician, data),
                Priority = "Medium",
                Status = "Reported",
                CostTotal = cost ?? 0,
                TenantId = tenantId.ToString(),
                CreatedBy = ExternalSystemName,
            };

            _db.Set<WorkOrder>().Add(workOrder);
            await _db.SaveChangesAsync();

            await UpsertExternalReferenceAsync("WorkOrder", workOrder.Id, externalId, data, tenantId);
            _logger.LogInformation("Created work order from PalmTrack: {ExternalId} -> WO {WorkOrderId}", externalId, workOrder.Id);
            return MappingResult.Ok("maintenance_consolidated_workorder");
        }

        // preventivo / revision → MaintenanceSchedule
        // MaintenanceSchedule no tiene Code → solo referencia externa.
        var existingSchedule = await FindByReferenceAsync<MaintenanceSchedule>("MaintenanceSchedule", externalId);
        if (existingSchedule != null)
        {
            existingSchedule.LastExecutionDate = date;
            existingSchedule.NextExecutionDate = nextMaintenance ?? existingSchedule.NextExecutionDate;
            if (hourMeter.HasValue) existingSchedule.NextExecutionHourMeter = hourMeter.Value;
            existingSchedule.UpdatedAt = DateTime.UtcNow;
            existingSchedule.UpdatedBy = ExternalSystemName;
            await _db.SaveChangesAsync();

            await UpsertExternalReferenceAsync("MaintenanceSchedule", existingSchedule.Id, externalId, data, tenantId);
            _logger.LogInformation("Updated maintenance schedule from PalmTrack: {ExternalId}", externalId);
            return MappingResult.Ok("maintenance_consolidated_schedule_updated");
        }

        // Intervalo en días entre la ejecución y la próxima (mínimo 1 si hay próxima).
        var intervalValue = 0;
        if (nextMaintenance.HasValue && date < nextMaintenance.Value)
            intervalValue = Math.Max(1, (int)Math.Ceiling((nextMaintenance.Value - date).TotalDays));

        var schedule = new MaintenanceSchedule
        {
            VehicleId = vehicle.Id,
            ScheduleType = "Date",
            IntervalValue = intervalValue,
            NextExecutionDate = nextMaintenance,
            LastExecutionDate = date,
            ToleranceValue = 0,
            Status = "Active",
            TenantId = tenantId.ToString(),
            CreatedBy = ExternalSystemName,
        };

        _db.Set<MaintenanceSchedule>().Add(schedule);
        await _db.SaveChangesAsync();

        await UpsertExternalReferenceAsync("MaintenanceSchedule", schedule.Id, externalId, data, tenantId);
        _logger.LogInformation("Created maintenance schedule from PalmTrack: {ExternalId} -> Schedule {ScheduleId}", externalId, schedule.Id);
        return MappingResult.Ok("maintenance_consolidated_schedule");
    }

    private async Task<MappingResult> ProcessSaleCreatedAsync(JsonElement payload, Guid tenantId)
    {
        var data = TryGetData(payload);
        _logger.LogInformation("Processing sale.created from PalmTrack (consolidation into Sale entity)");

        var externalId = GetRequiredString(data, "id");
        if (string.IsNullOrWhiteSpace(externalId))
            return MappingResult.Fail("422 sale_id_required");

        // Producto: SOLO se consolida contra un Product EXISTENTE (decisión A/C
        // del doc §5: Zorvian gestiona el catálogo). El texto libre de PalmTrack
        // se resuelve por nombre normalizado; sin match → pendiente de
        // reconciliación (no se inventan productos).
        var productName = GetRequiredString(data, "productName");
        if (string.IsNullOrWhiteSpace(productName))
            return MappingResult.Fail("422 product_name_required");

        var normalizedProduct = productName.Trim().ToLowerInvariant();
        var product = await _db.Set<Product>()
            .FirstOrDefaultAsync(p => p.Name.ToLower() == normalizedProduct && !p.IsDeleted);
        if (product == null)
            return MappingResult.Fail($"422 product_unresolved:{productName}");

        // Cliente: resolver por nombre; si no existe, se crea (doc §4: "resolver o crear Client").
        var clientName = data.TryGetProperty("client", out var c) ? c.GetString() : null;
        if (string.IsNullOrWhiteSpace(clientName))
            clientName = "Cliente PalmTrack";

        var normalizedClient = clientName.Trim().ToLowerInvariant();
        var client = await _db.Set<Client>()
            .FirstOrDefaultAsync(cl =>
                (cl.FirstName + " " + cl.LastName).Trim().ToLower() == normalizedClient && !cl.IsDeleted);
        if (client == null)
        {
            client = new Client
            {
                Code = $"PT-CLI-{externalId}",
                FirstName = clientName.Trim(),
                Status = "active",
                BranchId = Guid.Empty,
                TenantId = tenantId.ToString(),
                CompanyId = tenantId,
                CreatedBy = ExternalSystemName,
            };
            _db.Set<Client>().Add(client);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Created client from PalmTrack sale: {Client}", clientName);
        }

        // Duplicados: solo referencia externa (Sale no tiene Code de negocio).
        var existing = await FindByReferenceAsync<Sale>("Sale", externalId);
        if (existing != null)
        {
            existing.SaleDate = TryGetDate(data, "date") ?? existing.SaleDate;
            existing.Notes = BuildSaleNotes(data);
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = ExternalSystemName;
            await _db.SaveChangesAsync();

            await UpsertExternalReferenceAsync("Sale", existing.Id, externalId, data, tenantId);
            _logger.LogInformation("Updated sale from PalmTrack: {ExternalId}", externalId);
            return MappingResult.Ok("sale_consolidated_updated");
        }

        var total = TryGetDecimal(data, "totalAmount") ?? 0;
        var quantityDecimal = TryGetDecimal(data, "quantity") ?? 0;
        // PalmTrack captura cantidades decimales (kg); Zorvian maneja stock entero
        // (fricción documentada §5) → se redondea al entero más cercano.
        var quantity = (int)Math.Round(quantityDecimal);
        var unitPrice = TryGetDecimal(data, "unitPrice") ?? (quantity > 0 ? total / quantity : 0);
        var paid = GetRequiredString(data, "paymentStatus").Equals("Pagado", StringComparison.OrdinalIgnoreCase);
        var currency = GetRequiredString(data, "currency");
        if (string.IsNullOrWhiteSpace(currency)) currency = "USD";

        var sale = new Sale
        {
            InvoiceNumber = await GenerateInvoiceNumberAsync(),
            ClientId = client.Id,
            EmployeeId = Guid.Empty, // PalmTrack no reporta vendedor (campo operativo)
            SaleDate = TryGetDate(data, "date") ?? DateTime.UtcNow,
            SaleType = paid ? "cash" : "credit",
            Subtotal = total,
            Tax = 0,
            Discount = 0,
            Total = total,
            PaidAmount = paid ? total : 0,
            Balance = paid ? 0 : total,
            Status = SaleStatus.Completed,
            Notes = BuildSaleNotes(data),
            BranchId = Guid.Empty,
            CurrencyCode = currency,
            TenantId = tenantId.ToString(),
            CompanyId = tenantId,
            CreatedBy = ExternalSystemName,
        };

        sale.Details.Add(new SaleDetail
        {
            ProductId = product.Id,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Discount = 0,
            Subtotal = total,
            BranchId = Guid.Empty,
            TenantId = tenantId.ToString(),
            CompanyId = tenantId,
            CreatedBy = ExternalSystemName,
        });

        _db.Set<Sale>().Add(sale);
        await _db.SaveChangesAsync();

        // Kardex: descarga de stock + movimiento (mismo criterio que SaleService).
        // El asiento contable NO se genera aquí: la decisión A/C asigna el ciclo
        // administrativo (facturación electrónica, contabilidad) al flujo de Zorvian.
        if (quantity > 0)
        {
            var stockBefore = product.Stock;
            product.Stock = Math.Max(0, stockBefore - quantity);
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = ExternalSystemName;

            _db.Set<InventoryMovement>().Add(new InventoryMovement
            {
                ProductId = product.Id,
                MovementType = "sale",
                Quantity = quantity,
                StockBefore = stockBefore,
                StockAfter = product.Stock,
                UnitCost = product.CostPrice,
                ReferenceNumber = sale.InvoiceNumber,
                BranchId = Guid.Empty,
                TenantId = tenantId.ToString(),
                CompanyId = tenantId,
                CreatedBy = ExternalSystemName,
            });
            await _db.SaveChangesAsync();
        }

        await UpsertExternalReferenceAsync("Sale", sale.Id, externalId, data, tenantId);
        _logger.LogInformation("Created sale from PalmTrack: {ExternalId} -> Sale {SaleId}", externalId, sale.Id);
        return MappingResult.Ok("sale_consolidated");
    }

    private async Task<MappingResult> ProcessProductionLoggedAsync(JsonElement payload, Guid tenantId)
    {
        _logger.LogInformation("Processing production.logged from PalmTrack (PalmTrack is truth owner)");
        return MappingResult.Ok("production_logged_received");
    }

    private async Task<MappingResult> ProcessInventoryEventAsync(JsonElement payload, Guid tenantId, string eventName)
    {
        var data = TryGetData(payload);
        _logger.LogInformation("Processing {Event} from PalmTrack (consolidation into InventoryMovement)", eventName);

        var externalId = GetRequiredString(data, "id");
        if (string.IsNullOrWhiteSpace(externalId))
            return MappingResult.Fail("422 inventory_id_required");

        // inventory.updated = upsert del ITEM → Product (registrar referencia para
        // que entry/exit resuelvan el producto).
        if (eventName.Equals("inventory.updated", StringComparison.OrdinalIgnoreCase))
        {
            var itemName = GetRequiredString(data, "productName");
            if (string.IsNullOrWhiteSpace(itemName))
                return MappingResult.Fail("422 product_name_required");

            var itemProduct = await FindByReferenceAsync<Product>("Product", externalId)
                ?? await _db.Set<Product>().FirstOrDefaultAsync(p => p.Code == $"PT-PROD-{externalId}" && !p.IsDeleted);

            var itemUnitCost = TryGetDecimal(data, "unitCost");
            var stock = TryGetDecimal(data, "stock");

            if (itemProduct == null)
            {
                itemProduct = new Product
                {
                    Code = $"PT-PROD-{externalId}",
                    Name = itemName,
                    UnitOfMeasure = GetRequiredString(data, "unit"),
                    CostPrice = itemUnitCost ?? 0,
                    SellingPrice = itemUnitCost ?? 0,
                    Stock = stock.HasValue ? (int)Math.Round(stock.Value) : 0,
                    TenantId = tenantId.ToString(),
                    CompanyId = tenantId,
                    CreatedBy = ExternalSystemName,
                };
                _db.Set<Product>().Add(itemProduct);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Created product from PalmTrack inventory item: {Name}", itemName);
            }
            else
            {
                itemProduct.Name = itemName;
                itemProduct.UnitOfMeasure = GetRequiredString(data, "unit");
                if (itemUnitCost.HasValue) itemProduct.CostPrice = itemUnitCost.Value;
                if (stock.HasValue)
                {
                    // El stock de PalmTrack es la verdad operativa del campo.
                    itemProduct.Stock = (int)Math.Round(stock.Value);
                    itemProduct.UpdatedAt = DateTime.UtcNow;
                    itemProduct.UpdatedBy = ExternalSystemName;
                }
                await _db.SaveChangesAsync();
            }

            await UpsertExternalReferenceAsync("Product", itemProduct.Id, externalId, data, tenantId);
            return MappingResult.Ok("inventory_item_consolidated");
        }

        // inventory.entry.created / inventory.exit.created → InventoryMovement.
        var product = await FindByReferenceAsync<Product>("Product",
                GetRequiredString(data, "inventoryItemId"))
            ?? await _db.Set<Product>().FirstOrDefaultAsync(p =>
                p.Code == $"PT-PROD-{GetRequiredString(data, "inventoryItemId")}" && !p.IsDeleted);
        if (product == null)
            return MappingResult.Fail($"422 product_unresolved:{GetRequiredString(data, "inventoryItemId")} (esperando inventory.updated del item)");

        var quantity = TryGetDecimal(data, "quantity");
        if (!quantity.HasValue || quantity.Value <= 0)
            return MappingResult.Fail("422 quantity_required");

        var qty = (int)Math.Round(quantity.Value);
        var stockBefore = product.Stock;
        var isEntry = eventName.Equals("inventory.entry.created", StringComparison.OrdinalIgnoreCase);
        var stockAfter = isEntry ? stockBefore + qty : Math.Max(0, stockBefore - qty);
        var unitCost = TryGetDecimal(data, "unitCost") ?? product.CostPrice;

        // Duplicados: referencia externa del movimiento.
        var existingMovement = await FindByReferenceAsync<InventoryMovement>("InventoryMovement", externalId);
        if (existingMovement != null)
        {
            existingMovement.UnitCost = unitCost;
            existingMovement.Notes = BuildInventoryNotes(data, isEntry);
            existingMovement.UpdatedAt = DateTime.UtcNow;
            existingMovement.UpdatedBy = ExternalSystemName;
            await _db.SaveChangesAsync();

            await UpsertExternalReferenceAsync("InventoryMovement", existingMovement.Id, externalId, data, tenantId);
            return MappingResult.Ok("inventory_movement_consolidated_updated");
        }

        var movement = new InventoryMovement
        {
            ProductId = product.Id,
            MovementType = isEntry ? "entry" : "exit",
            Quantity = qty,
            StockBefore = stockBefore,
            StockAfter = stockAfter,
            UnitCost = unitCost,
            ReferenceNumber = externalId,
            Notes = BuildInventoryNotes(data, isEntry),
            BranchId = Guid.Empty,
            TenantId = tenantId.ToString(),
            CompanyId = tenantId,
            CreatedBy = ExternalSystemName,
        };

        _db.Set<InventoryMovement>().Add(movement);

        product.Stock = stockAfter;
        product.UpdatedAt = DateTime.UtcNow;
        product.UpdatedBy = ExternalSystemName;

        await _db.SaveChangesAsync();

        await UpsertExternalReferenceAsync("InventoryMovement", movement.Id, externalId, data, tenantId);
        _logger.LogInformation("Created inventory {Type} from PalmTrack: {ExternalId} -> Movement {MovementId}",
            movement.MovementType, externalId, movement.Id);
        return MappingResult.Ok("inventory_movement_consolidated");
    }

    private async Task<MappingResult> ProcessExpenseCreatedAsync(JsonElement payload, Guid tenantId)
    {
        var data = TryGetData(payload);
        _logger.LogInformation("Processing expense.created from PalmTrack (consolidation into Accounting)");

        var externalId = GetRequiredString(data, "id");
        if (string.IsNullOrWhiteSpace(externalId))
            return MappingResult.Fail("422 expense_id_required");

        var amount = TryGetDecimal(data, "amount");
        if (!amount.HasValue || amount.Value <= 0)
            return MappingResult.Fail("422 amount_required");

        // Mapeo categoría PalmTrack → cuenta del plan sembrado (AccountService):
        //   fixed/administrative → 6.1.01 Gastos Administrativos
        //   operative            → 6.1.02 Gastos de Venta
        //   unforeseen           → 6.1.01 (la categoría original queda en la
        //                          descripción para revisión del asiento DRAFT)
        var category = GetRequiredString(data, "category").ToLowerInvariant();
        var expenseAccountCode = category switch
        {
            "operative" => "6.1.02",
            _ => "6.1.01",
        };

        var expenseAccount = await ResolveAccountByCodeAsync(expenseAccountCode, tenantId);
        if (expenseAccount == null)
            return MappingResult.Fail($"422 account_unresolved:{expenseAccountCode}");

        var cashAccount = await ResolveAccountByCodeAsync("1.1.01", tenantId);
        if (cashAccount == null)
            return MappingResult.Fail("422 account_unresolved:1.1.01 (Caja General)");

        var entryDate = TryGetDate(data, "date") ?? DateTime.UtcNow;
        var period = await _db.Set<AccountingPeriod>()
            .FirstOrDefaultAsync(p =>
                p.CompanyId == tenantId &&
                p.Year == entryDate.Year &&
                p.Month == entryDate.Month &&
                p.Status == "Open" && !p.IsDeleted);
        if (period == null)
            return MappingResult.Fail($"422 accounting_period_closed:{entryDate:yyyy-MM}");

        // Duplicados: referencia externa del asiento.
        var existingEntry = await FindByReferenceAsync<AccountingEntry>("AccountingEntry", externalId);
        if (existingEntry != null)
        {
            existingEntry.Description = BuildExpenseDescription(data, category);
            existingEntry.UpdatedAt = DateTime.UtcNow;
            existingEntry.UpdatedBy = ExternalSystemName;
            await _db.SaveChangesAsync();

            await UpsertExternalReferenceAsync("AccountingEntry", existingEntry.Id, externalId, data, tenantId);
            return MappingResult.Ok("expense_consolidated_updated");
        }

        var currency = GetRequiredString(data, "currency");
        if (string.IsNullOrWhiteSpace(currency)) currency = "USD";

        // Estado DRAFT: es la cola de reconciliación contable del doc §6 — el
        // asiento queda visible para revisión/afinación de cuentas antes de postear.
        var entry = new AccountingEntry
        {
            EntryNumber = $"AS-PT-{externalId}",
            EntryDate = entryDate,
            Description = BuildExpenseDescription(data, category),
            ReferenceType = "PalmTrackExpense",
            Status = "draft",
            AccountingPeriodId = period.Id,
            TotalDebit = amount.Value,
            TotalCredit = amount.Value,
            CurrencyCode = currency,
            TenantId = tenantId.ToString(),
            CompanyId = tenantId,
            CreatedBy = ExternalSystemName,
            Details =
            [
                new AccountingEntryDetail
                {
                    AccountId = expenseAccount.Id,
                    DebitAmount = amount.Value,
                    CreditAmount = 0,
                    Description = $"Gasto PalmTrack (categoría: {category})",
                    TenantId = tenantId.ToString(),
                    CompanyId = tenantId,
                    CreatedBy = ExternalSystemName,
                },
                new AccountingEntryDetail
                {
                    AccountId = cashAccount.Id,
                    DebitAmount = 0,
                    CreditAmount = amount.Value,
                    Description = "Contrapartida Caja General",
                    TenantId = tenantId.ToString(),
                    CompanyId = tenantId,
                    CreatedBy = ExternalSystemName,
                },
            ],
        };

        _db.Set<AccountingEntry>().Add(entry);
        await _db.SaveChangesAsync();

        await UpsertExternalReferenceAsync("AccountingEntry", entry.Id, externalId, data, tenantId);
        _logger.LogInformation("Created draft accounting entry from PalmTrack expense: {ExternalId} -> {EntryId}", externalId, entry.Id);
        return MappingResult.Ok("expense_consolidated_draft");
    }

    private async Task<MappingResult> ProcessLaborLogCreatedAsync(JsonElement payload, Guid tenantId)
    {
        var data = TryGetData(payload);
        _logger.LogInformation("Processing labor_log.created from PalmTrack (consolidation into Workforce)");

        var externalId = GetRequiredString(data, "id");
        if (string.IsNullOrWhiteSpace(externalId))
            return MappingResult.Fail("422 labor_log_id_required");

        // PalmTrack captura mano de obra por JORNAL (día trabajado, nombre libre
        // del colaborador en 'collaborators'); Zorvian-Payroll exige PayrollRun
        // batch (INSS/IR). La consolidación honesta (decisión A/C: Zorvian Payroll
        // es dueño del ciclo administrativo) es registrar la ASISTENCIA del día:
        // AttendanceRecord por colaborador con las horas/notes; el pago real se
        // liquida en la corrida de nómina (fricción doc §7).
        var collaboratorsRaw = GetRequiredString(data, "collaborators");
        if (string.IsNullOrWhiteSpace(collaboratorsRaw))
            return MappingResult.Fail("422 collaborators_required");

        var logDate = TryGetDate(data, "date") ?? DateTime.UtcNow;
        var dateOnly = DateOnly.FromDateTime(logDate);
        var cost = TryGetDecimal(data, "cost") ?? 0;
        var activity = GetRequiredString(data, "activity");
        var activityCategory = GetRequiredString(data, "activityCategory");
        var laborStatus = GetRequiredString(data, "status"); // Directa/Indirecta

        // PalmTrack permite varios colaboradores por log (separados por coma).
        var names = collaboratorsRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (names.Count == 0)
            return MappingResult.Fail("422 collaborators_required");

        var created = 0;
        var matched = 0;
        var unresolved = new List<string>();

        foreach (var name in names)
        {
            var employee = await ResolveEmployeeByCollaboratorNameAsync(name, tenantId);
            if (employee == null)
            {
                unresolved.Add(name);
                continue;
            }

            // Anti-duplicados: un AttendanceRecord por empleado/día/nota PT-{id}.
            var existing = await _db.Set<AttendanceRecord>().FirstOrDefaultAsync(a =>
                a.EmployeeId == employee.Id &&
                a.Date == dateOnly &&
                a.Notes != null && a.Notes.Contains($"PT-{externalId}") &&
                !a.IsDeleted);
            if (existing != null)
            {
                existing.Status = "present";
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = ExternalSystemName;
                matched++;
                continue;
            }

            _db.Set<AttendanceRecord>().Add(new AttendanceRecord
            {
                EmployeeId = employee.Id,
                Date = dateOnly,
                Status = "present",
                TotalHours = null, // PalmTrack captura jornal (día), no horas
                Notes = $"PT-{externalId} | {activityCategory}/{activity} | {laborStatus} | Jornal: {cost} | Colaborador PT: {name}",
                TenantId = tenantId.ToString(),
                CompanyId = tenantId,
                CreatedBy = ExternalSystemName,
            });
            created++;
        }

        await _db.SaveChangesAsync();

        if (created == 0 && matched == 0)
        {
            // Sin NINGÚN colaborador resoluble (ni duplicado que actualizar) →
            // no consolida nada (fail-closed): el evento queda visible en los
            // logs del consumer para Paso 0.
            _logger.LogWarning(
                "Labor log {ExternalId}: no collaborators resolved: {Unresolved}",
                externalId, string.Join(", ", unresolved));
            return MappingResult.Fail($"422 labor_log_unresolved:{string.Join(",", unresolved)}");
        }

        // La referencia apunta al PRIMER AttendanceRecord creado (o Guid.Empty si
        // todos los colaboradores quedaron sin resolver).
        var firstRecordId = await _db.Set<AttendanceRecord>()
            .Where(a => a.Notes != null && a.Notes.Contains($"PT-{externalId}"))
            .Select(a => a.Id)
            .FirstOrDefaultAsync();
        await UpsertExternalReferenceAsync("LaborLog", firstRecordId, externalId, data, tenantId);

        if (unresolved.Count > 0)
        {
            // Cola de reconciliación de identidad (doc §7): los colaboradores sin
            // Employee se reportan en el resultado para revisión (Paso 0).
            _logger.LogWarning(
                "Labor log {ExternalId}: {Created} attendance records created; unresolved collaborators: {Unresolved}",
                externalId, created, string.Join(", ", unresolved));
            return MappingResult.Ok($"labor_log_consolidated_partial:{string.Join(",", unresolved)}");
        }

        _logger.LogInformation("Labor log {ExternalId}: {Created} attendance records created", externalId, created);
        return MappingResult.Ok("labor_log_consolidated");
    }

    // ═══════════════════════════════════════════
    // External Reference Resolution (consolidación §4-5)
    // ═══════════════════════════════════════════

    /// <summary>
    /// Resuelve un vehículo por su referencia externa (FleetExternalReference)
    /// o, como fallback, por la convención de código PT-{externalId} que usa
    /// ProcessVehicleEventAsync al crear vehículos desde PalmTrack.
    /// </summary>
    private async Task<Vehicle?> ResolveVehicleByExternalIdAsync(string? externalId)
    {
        if (string.IsNullOrWhiteSpace(externalId)) return null;

        var reference = await _db.Set<FleetExternalReference>()
            .FirstOrDefaultAsync(r =>
                r.ExternalSystem == ExternalSystemName &&
                r.EntityType == "Vehicle" &&
                r.ExternalId == externalId &&
                !r.IsDeleted);

        if (reference != null)
        {
            return await _db.Set<Vehicle>()
                .FirstOrDefaultAsync(v => v.Id == reference.EntityId && !v.IsDeleted);
        }

        // Fallback: convención de código del handler de vehículos.
        return await _db.Set<Vehicle>()
            .FirstOrDefaultAsync(v => v.Code == $"PT-{externalId}" && !v.IsDeleted);
    }

    /// <summary>
    /// Resuelve un Employee de Zorvian por el nombre de un colaborador de
    /// PalmTrack: Employee.FirstName+LastName normalizado (Employee hereda el
    /// nombre del Collaborator al crearse vía CollaboratorService).
    /// </summary>
    private async Task<Employee?> ResolveEmployeeByCollaboratorNameAsync(string collaboratorName, Guid tenantId)
    {
        if (string.IsNullOrWhiteSpace(collaboratorName)) return null;

        var normalized = collaboratorName.Trim().ToLowerInvariant();
        return await _db.Set<Employee>()
            .FirstOrDefaultAsync(e =>
                (e.FirstName + " " + e.LastName).Trim().ToLower() == normalized &&
                e.Status == "active" &&
                !e.IsDeleted);
    }

    /// <summary>
    /// Resuelve un Driver por su alias de PalmTrack (FleetDriverAlias), por
    /// ExternalDriverId o por nombre normalizado. Incrementa MatchCount al usarlo.
    /// </summary>
    private async Task<Driver?> ResolveDriverByAliasAsync(string? driverName, string? externalDriverId = null)
    {
        if (string.IsNullOrWhiteSpace(driverName) && string.IsNullOrWhiteSpace(externalDriverId))
            return null;

        var aliases = await _db.Set<FleetDriverAlias>()
            .Include(a => a.Driver)
            .Where(a => a.ExternalSystem == ExternalSystemName && !a.IsDeleted)
            .ToListAsync();

        var normalized = driverName?.Trim().ToLowerInvariant();
        var alias = aliases.FirstOrDefault(a =>
            (!string.IsNullOrEmpty(externalDriverId) && a.ExternalDriverId == externalDriverId) ||
            (!string.IsNullOrEmpty(normalized) && a.ExternalName.Trim().ToLowerInvariant() == normalized));

        if (alias == null) return null;

        alias.MatchCount++;
        return alias.Driver;
    }

    /// <summary>
    /// Busca una entidad consolidada por su referencia externa. Defensa
    /// adicional contra duplicados (patrón de consolidación §2). El fallback
    /// por código/número de negocio es específico por entidad (los handlers).
    /// </summary>
    private async Task<T?> FindByReferenceAsync<T>(string entityType, string externalId)
        where T : BaseEntity
    {
        var reference = await GetExternalReferenceAsync(entityType, externalId);
        if (reference == null) return null;

        return await _db.Set<T>().FirstOrDefaultAsync(e => e.Id == reference.EntityId && !e.IsDeleted);
    }

    private async Task<FleetExternalReference?> GetExternalReferenceAsync(string entityType, string externalId)
    {
        if (string.IsNullOrWhiteSpace(externalId)) return null;

        return await _db.Set<FleetExternalReference>()
            .FirstOrDefaultAsync(r =>
                r.ExternalSystem == ExternalSystemName &&
                r.EntityType == entityType &&
                r.ExternalId == externalId &&
                !r.IsDeleted);
    }

    /// <summary>
    /// Registra/actualiza la referencia externa bidireccional (patrón §4) con
    /// snapshot del payload y estado synced.
    /// </summary>
    private async Task UpsertExternalReferenceAsync(string entityType, Guid entityId, string externalId, JsonElement? payload, Guid tenantId)
    {
        if (string.IsNullOrWhiteSpace(externalId)) return;

        var reference = await GetExternalReferenceAsync(entityType, externalId);

        if (reference == null)
        {
            reference = new FleetExternalReference
            {
                ExternalSystem = ExternalSystemName,
                EntityType = entityType,
                ExternalId = externalId,
                TenantId = tenantId.ToString(),
                CreatedBy = ExternalSystemName,
            };
            _db.Set<FleetExternalReference>().Add(reference);
        }

        reference.EntityId = entityId;
        reference.Status = "synced";
        reference.LastSyncAt = DateTime.UtcNow;
        reference.LastError = null;
        reference.ConsecutiveFailures = 0;
        if (payload.HasValue)
            reference.ExternalPayload = payload.Value.GetRawText();

        await _db.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════
    // Catalog Resolution (plan §3.1-3.3)
    // ═══════════════════════════════════════════

    private async Task<VehicleType?> ResolveVehicleTypeAsync(string? palmType)
    {
        if (string.IsNullOrWhiteSpace(palmType)) return null;

        var normalized = palmType.ToLowerInvariant().Trim();
        var zorvianName = normalized switch
        {
            "camion" or "camión" => "Truck",
            "camioneta" => "Pickup",
            "moto" or "motocicleta" => "Motorcycle",
            "tractor" => "Tractor",
            "fumigadora" => "Sprayer",
            "cosechadora" => "Harvester",
            "cortadora" => "Cutter",
            _ => "Other"
        };

        var type = await _db.Set<VehicleType>()
            .FirstOrDefaultAsync(t => t.Name == zorvianName && !t.IsDeleted);

        if (type == null)
        {
            type = new VehicleType { Name = zorvianName };
            _db.Set<VehicleType>().Add(type);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Created VehicleType from PalmTrack: {Name}", zorvianName);
        }

        return type;
    }

    private async Task<VehicleBrand?> ResolveVehicleBrandAsync(string? palmBrand)
    {
        if (string.IsNullOrWhiteSpace(palmBrand)) return null;

        var normalized = palmBrand.Trim().ToLowerInvariant();
        var brand = await _db.Set<VehicleBrand>()
            .FirstOrDefaultAsync(b => b.Name.ToLower() == normalized && !b.IsDeleted);

        if (brand == null)
        {
            brand = new VehicleBrand { Name = palmBrand.Trim() };
            _db.Set<VehicleBrand>().Add(brand);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Created VehicleBrand from PalmTrack: {Name}", brand.Name);
        }

        return brand;
    }

    private async Task<FuelType?> ResolveFuelTypeAsync(string? palmFuelType)
    {
        if (string.IsNullOrWhiteSpace(palmFuelType)) return null;

        var normalized = palmFuelType.ToLowerInvariant().Trim();
        var zorvianName = normalized switch
        {
            "diesel" => "Diesel",
            "gasolina" or "gas" => "Gasoline",
            "electrico" or "eléctrico" => "Electric",
            "hibrido" or "híbrido" => "Hybrid",
            _ => "Other"
        };

        var fuelType = await _db.Set<FuelType>()
            .FirstOrDefaultAsync(f => f.Name == zorvianName && !f.IsDeleted);

        if (fuelType == null)
        {
            fuelType = new FuelType { Name = zorvianName };
            _db.Set<FuelType>().Add(fuelType);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Created FuelType from PalmTrack: {Name}", zorvianName);
        }

        return fuelType;
    }

    // ═══════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════

    private static JsonElement TryGetData(JsonElement payload)
    {
        return payload.TryGetProperty("data", out var d) ? d : payload;
    }

    private static string GetRequiredString(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
            return prop.GetString() ?? "";
        return "";
    }

    private static DateTime? TryGetDate(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop) &&
            prop.ValueKind == JsonValueKind.String &&
            DateTime.TryParse(prop.GetString(), CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var date))
        {
            return date;
        }
        return null;
    }

    private static decimal? TryGetDecimal(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop))
        {
            if (prop.ValueKind == JsonValueKind.Number)
                return prop.GetDecimal();
            if (prop.ValueKind == JsonValueKind.String &&
                decimal.TryParse(prop.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                return value;
        }
        return null;
    }

    /// <summary>
    /// Número de factura para ventas consolidadas. Misma convención que
    /// SaleRepository.GenerateInvoiceNumberAsync (secuencia PG; conteo en InMemory
    /// para tests).
    /// </summary>
    private async Task<string> GenerateInvoiceNumberAsync()
    {
        if (_db.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            var count = await _db.Set<Sale>().CountAsync();
            return $"FAC-{DateTime.UtcNow:yyyyMMdd}-{(count + 1):D4}";
        }

        var raw = await _db.Database.SqlQueryRaw<int>("SELECT nextval('seq_invoice_number')::int").FirstOrDefaultAsync();
        return $"FAC-{DateTime.UtcNow:yyyyMMdd}-{raw:D4}";
    }

    /// <summary>
    /// Resuelve una Account por código (con variantes padded, mismo criterio que
    /// AutoAccountingService.GetAccountIdByCodeAsync).
    /// </summary>
    private async Task<Account?> ResolveAccountByCodeAsync(string code, Guid tenantId)
    {
        var account = await _db.Set<Account>()
            .FirstOrDefaultAsync(a => a.Code == code && a.CompanyId == tenantId && a.IsActive && !a.IsDeleted);
        if (account != null) return account;

        var parts = code.Split('.');
        if (parts.Length == 3)
        {
            var variants = new[]
            {
                $"{parts[0]}.{parts[1]}.{parts[2]}.000",
                $"{parts[0]}.{parts[1]}.{parts[2]}.0000",
                $"{parts[0]}.0{parts[1]}.{parts[2]}",
                $"{parts[0]}.0{parts[1]}.0{parts[2]}",
            };
            account = (await _db.Set<Account>()
                .Where(a => variants.Contains(a.Code) && a.CompanyId == tenantId && a.IsActive && !a.IsDeleted)
                .ToListAsync()).FirstOrDefault();
        }

        return account;
    }

    /// <summary>Notas de la venta consolidada: método de pago y estado original.</summary>
    private static string? BuildSaleNotes(JsonElement data)
    {
        var parts = new List<string> { "[PT] Venta capturada en PalmTrack" };
        var method = data.TryGetProperty("paymentMethod", out var pm) ? pm.GetString() : null;
        var status = data.TryGetProperty("paymentStatus", out var ps) ? ps.GetString() : null;
        if (!string.IsNullOrWhiteSpace(method)) parts.Add($"Pago: {method}");
        if (!string.IsNullOrWhiteSpace(status)) parts.Add($"Estado pago: {status}");
        var saleNotes = data.TryGetProperty("notes", out var sn) ? sn.GetString() : null;
        if (!string.IsNullOrWhiteSpace(saleNotes)) parts.Add(saleNotes);
        return string.Join(" | ", parts);
    }

    /// <summary>Notas del movimiento de inventario consolidado.</summary>
    private static string? BuildInventoryNotes(JsonElement data, bool isEntry)
    {
        var parts = new List<string> { $"[PT] {(isEntry ? "Entrada" : "Salida")} de inventario" };
        if (data.TryGetProperty("reason", out var r) && r.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(r.GetString()))
            parts.Add($"Motivo: {r.GetString()}");
        if (data.TryGetProperty("destination", out var d) && d.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(d.GetString()))
            parts.Add($"Destino: {d.GetString()}");
        if (data.TryGetProperty("responsible", out var res) && res.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(res.GetString()))
            parts.Add($"Responsable: {res.GetString()}");
        return string.Join(" | ", parts);
    }

    /// <summary>Descripción del asiento contable del gasto consolidado.</summary>
    private static string BuildExpenseDescription(JsonElement data, string category)
    {
        var description = GetRequiredString(data, "description");
        var notes = data.TryGetProperty("notes", out var n) ? n.GetString() : null;
        var parts = new List<string> { $"[PT] Gasto ({category}): {description}" };
        if (!string.IsNullOrWhiteSpace(notes)) parts.Add(notes);
        return string.Join(" | ", parts);
    }

    /// <summary>PalmTrack no tiene columnas de carga en Trip → se preserva en Notes.</summary>
    private static string? BuildTripNotes(JsonElement data)
    {
        var notes = data.TryGetProperty("notes", out var n) ? n.GetString() : null;
        var parts = new List<string>();
        if (data.TryGetProperty("cargo", out var cargo) && cargo.ValueKind == JsonValueKind.String)
            parts.Add($"Carga: {cargo.GetString()}");
        if (data.TryGetProperty("cargoWeight", out var cw) && cw.ValueKind == JsonValueKind.Number)
            parts.Add($"Peso: {cw.GetDecimal()} kg");
        if (data.TryGetProperty("tripCount", out var tc) && tc.ValueKind == JsonValueKind.Number && tc.GetInt32() != 1)
            parts.Add($"Viajes: {tc.GetInt32()}");

        var extra = parts.Count > 0 ? string.Join(" | ", parts) : null;
        if (extra == null) return notes;
        return string.IsNullOrWhiteSpace(notes) ? extra : $"{notes} | {extra}";
    }

    private static string? BuildFuelObservations(JsonElement data)
    {
        var notes = data.TryGetProperty("notes", out var n) ? n.GetString() : null;
        var station = data.TryGetProperty("station", out var s) ? s.GetString() : null;
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(station))
            parts.Add($"Estación: {station}");
        if (!string.IsNullOrWhiteSpace(notes))
            parts.Add(notes);
        return parts.Count > 0 ? string.Join(" | ", parts) : null;
    }

    private static string BuildMaintenanceDescription(string description, string? technician, JsonElement data)
    {
        var parts = new List<string> { description };
        if (!string.IsNullOrWhiteSpace(technician))
            parts.Add($"[PT] Técnico: {technician}");
        if (data.TryGetProperty("notes", out var n) && n.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(n.GetString()))
            parts.Add($"[PT] Notas: {n.GetString()}");
        return string.Join("\n", parts);
    }

    private static string MapTripStatus(string? palmStatus)
    {
        return palmStatus?.ToLowerInvariant() switch
        {
            "programado" => "Planned",
            "en_progreso" => "InProgress",
            "completado" => "Completed",
            "cancelado" => "Cancelled",
            _ => "Planned"
        };
    }

    private static string MapVehicleStatus(string? palmStatus)
    {
        return palmStatus?.ToLowerInvariant() switch
        {
            "disponible" or "activo" or "active" => "Active",
            "en_uso" => "Active",
            "mantenimiento" or "under_maintenance" => "UnderMaintenance",
            "fuera_servicio" or "inactivo" or "inactive" => "Inactive",
            _ => "Active"
        };
    }
}
