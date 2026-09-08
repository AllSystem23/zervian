using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Zorvian.Application.Interfaces.PalmTrack;
using Zorvian.Application.Messages;
using Zorvian.Core.Entities;
using Zorvian.Core.Entities.Fleet;
using Zorvian.Core.Interfaces;
using Zorvian.Infrastructure.Data;
using Zorvian.Infrastructure.Services.PalmTrack;

namespace Zorvian.Tests.PalmTrack;

/// <summary>
/// Tests for PalmTrackEventMapper.
/// </summary>
public sealed class PalmTrackEventMapperTests : IDisposable
{
    private readonly ZorvianDbContext _db;
    private readonly Mock<IPalmTrackIdentityService> _identityService = new();
    private readonly PalmTrackEventMapper _sut;
    private readonly Guid _tenantId;

    public PalmTrackEventMapperTests()
    {
        _tenantId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<ZorvianDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantMock = new Mock<ITenantContext>();
        tenantMock.Setup(t => t.TenantId).Returns(new TenantId(_tenantId));
        _db = new ZorvianDbContext(options, tenantMock.Object);

        _sut = new PalmTrackEventMapper(
            _db,
            _identityService.Object,
            Mock.Of<ILogger<PalmTrackEventMapper>>());

        _identityService.Setup(i => i.GetTenantIdAsync(It.IsAny<string>()))
            .ReturnsAsync(_tenantId);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task ProcessAsync_UnreconciledOrg_ShouldFail()
    {
        _identityService.Setup(i => i.GetTenantIdAsync("unknown-org"))
            .ReturnsAsync((Guid?)null);

        var message = new PalmTrackWebhookReceived
        {
            Event = "sale.created",
            OrganizationId = "unknown-org",
            IdempotencyKey = "key-1",
            Payload = JsonDocument.Parse("{}").RootElement,
            ReceivedAt = DateTime.UtcNow,
        };

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not reconciled");
    }

    [Fact]
    public async Task ProcessAsync_VehicleCreated_ShouldSucceed()
    {
        var payload = JsonSerializer.Serialize(new
        {
            organizationId = "org-123",
            data = new
            {
                id = "palm-doc-001",
                licensePlate = "RNJ-1234",
                model = "9370R",
                year = 2021,
                brand = "John Deere",
                type = "tractor",
                fuelType = "diesel",
                mileage = 1580.0,
                status = "disponible"
            }
        });

        var message = new PalmTrackWebhookReceived
        {
            Event = "vehicle.created",
            OrganizationId = "org-123",
            IdempotencyKey = "key-1",
            Payload = JsonDocument.Parse(payload).RootElement,
            ReceivedAt = DateTime.UtcNow,
        };

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("vehicle_processed");
    }

    [Fact]
    public async Task ProcessAsync_VehicleCreated_MissingPlate_ShouldFail()
    {
        var payload = JsonSerializer.Serialize(new
        {
            organizationId = "org-123",
            data = new
            {
                id = "palm-doc-002",
                licensePlate = "",
                model = "Test"
            }
        });

        var message = new PalmTrackWebhookReceived
        {
            Event = "vehicle.created",
            OrganizationId = "org-123",
            IdempotencyKey = "key-2",
            Payload = JsonDocument.Parse(payload).RootElement,
            ReceivedAt = DateTime.UtcNow,
        };

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("plate_required");
    }

    [Fact]
    public async Task ProcessAsync_UnhandledEvent_ShouldFail()
    {
        var message = new PalmTrackWebhookReceived
        {
            Event = "unknown.event",
            OrganizationId = "org-123",
            IdempotencyKey = "key-3",
            Payload = JsonDocument.Parse("{}").RootElement,
            ReceivedAt = DateTime.UtcNow,
        };

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Unhandled event");
    }

    [Fact]
    public async Task ProcessAsync_VehicleCreated_CreatesNewBrandIfNotExist()
    {
        var payload = JsonSerializer.Serialize(new
        {
            organizationId = "org-123",
            data = new
            {
                id = "palm-doc-003",
                licensePlate = "ABC-5678",
                brand = "NewBrand",
                type = "camioneta",
                fuelType = "gasolina",
            }
        });

        var message = new PalmTrackWebhookReceived
        {
            Event = "vehicle.created",
            OrganizationId = "org-123",
            IdempotencyKey = "key-4",
            Payload = JsonDocument.Parse(payload).RootElement,
            ReceivedAt = DateTime.UtcNow,
        };

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessAsync_SaleCreated_WithoutProduct_ShouldFailForReconciliation()
    {
        var message = new PalmTrackWebhookReceived
        {
            Event = "sale.created",
            OrganizationId = "org-123",
            IdempotencyKey = "key-5",
            Payload = JsonDocument.Parse("{}").RootElement,
            ReceivedAt = DateTime.UtcNow,
        };

        var result = await _sut.ProcessAsync(message);

        // Payload vacío → fail-closed (sin id externo). La venta ya se consolida
        // (ver tests SaleCreated_*); este test fija el gate del payload mínimo.
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessAsync_ProductionLogged_ShouldSucceed()
    {
        var message = new PalmTrackWebhookReceived
        {
            Event = "production.logged",
            OrganizationId = "org-123",
            IdempotencyKey = "key-6",
            Payload = JsonDocument.Parse("{}").RootElement,
            ReceivedAt = DateTime.UtcNow,
        };

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("production_logged_received");
    }

    // =========================================================================
    // Consolidación Fleet: trip / fuel_log / maintenance_log (plan §5-7)
    // =========================================================================

    private async Task<Vehicle> SeedVehicleAsync(string externalId = "palm-veh-1")
    {
        var vehicle = new Vehicle
        {
            Code = $"PT-{externalId}",
            Plate = $"SEED-{externalId[^4..].ToUpperInvariant()}",
            Model = "Seed Model",
            Status = "Active",
            TenantId = _tenantId.ToString(),
        };
        _db.Set<Vehicle>().Add(vehicle);
        await _db.SaveChangesAsync();
        return vehicle;
    }

    private async Task SeedDriverAliasAsync(string externalName, string driverFirstName)
    {
        var driver = new Driver
        {
            FirstName = driverFirstName,
            LastName = "Pérez",
            IdDocument = "001-010999-0001",
            LicenseNumber = "LN-001",
            TenantId = _tenantId.ToString(),
        };
        _db.Set<Driver>().Add(driver);
        _db.Set<FleetDriverAlias>().Add(new FleetDriverAlias
        {
            DriverId = driver.Id,
            ExternalSystem = "palmtrack",
            ExternalName = externalName,
            MatchType = "manual",
            IsPrimary = true,
            TenantId = _tenantId.ToString(),
        });
        await _db.SaveChangesAsync();
    }

    private PalmTrackWebhookReceived MakeMessage(string @event, string idempotencyKey, object data)
    {
        var payload = JsonSerializer.Serialize(new { organizationId = "org-123", data });
        return new PalmTrackWebhookReceived
        {
            Event = @event,
            OrganizationId = "org-123",
            IdempotencyKey = idempotencyKey,
            Payload = JsonDocument.Parse(payload).RootElement,
            ReceivedAt = DateTime.UtcNow,
        };
    }

    [Fact]
    public async Task ProcessAsync_TripCreated_WithResolvableRefs_CreatesTrip()
    {
        var vehicle = await SeedVehicleAsync();
        await SeedDriverAliasAsync("Juan Pérez", "Juan");

        var message = MakeMessage("trip.created", "trip-k1", new
        {
            id = "palm-trip-001",
            vehicleId = "palm-veh-1",
            driver = "Juan Pérez",
            departureDate = "2026-09-01T08:00:00Z",
            arrivalDate = "2026-09-01T10:30:00Z",
            origin = "Finca Central",
            destination = "Empacadora",
            status = "completado",
            notes = "Entrega racimos",
        });

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("trip_consolidated");

        var trip = await _db.Set<Trip>().SingleAsync(t => t.Code == "PT-TRIP-palm-trip-001");
        trip.VehicleId.Should().Be(vehicle.Id);
        trip.DriverId.Should().NotBe(Guid.Empty);
        trip.Origin.Should().Be("Finca Central");
        trip.Destination.Should().Be("Empacadora");
        trip.Status.Should().Be("Completed");
        trip.Notes.Should().Contain("Entrega racimos");
        trip.TenantId.Should().Be(_tenantId.ToString());

        // Referencia externa registrada (patrón §4)
        var reference = await _db.Set<FleetExternalReference>().SingleAsync(
            r => r.EntityType == "Trip" && r.ExternalId == "palm-trip-001");
        reference.EntityId.Should().Be(trip.Id);
        reference.Status.Should().Be("synced");
    }

    [Fact]
    public async Task ProcessAsync_TripCreated_UnresolvedVehicle_ShouldFail()
    {
        await SeedDriverAliasAsync("Juan Pérez", "Juan");

        var message = MakeMessage("trip.created", "trip-k2", new
        {
            id = "palm-trip-002",
            vehicleId = "vehiculo-inexistente",
            driver = "Juan Pérez",
            departureDate = "2026-09-01T08:00:00Z",
            origin = "A",
            destination = "B",
        });

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("vehicle_unresolved");
    }

    [Fact]
    public async Task ProcessAsync_TripCreated_UnresolvedDriver_ShouldFail()
    {
        await SeedVehicleAsync();

        var message = MakeMessage("trip.created", "trip-k3", new
        {
            id = "palm-trip-003",
            vehicleId = "palm-veh-1",
            driver = "Conductor Sin Alias",
            departureDate = "2026-09-01T08:00:00Z",
            origin = "A",
            destination = "B",
        });

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("driver_alias_unresolved");
    }

    [Fact]
    public async Task ProcessAsync_TripUpdated_ShouldNotDuplicate()
    {
        await SeedVehicleAsync();
        await SeedDriverAliasAsync("Juan Pérez", "Juan");

        var message = MakeMessage("trip.created", "trip-k4", new
        {
            id = "palm-trip-004",
            vehicleId = "palm-veh-1",
            driver = "Juan Pérez",
            departureDate = "2026-09-02T08:00:00Z",
            origin = "A",
            destination = "B",
            status = "en_progreso",
        });

        (await _sut.ProcessAsync(message)).Success.Should().BeTrue();

        var update = MakeMessage("trip.updated", "trip-k5", new
        {
            id = "palm-trip-004",
            vehicleId = "palm-veh-1",
            driver = "Juan Pérez",
            departureDate = "2026-09-02T08:00:00Z",
            origin = "A",
            destination = "B",
            status = "completado",
        });

        var result = await _sut.ProcessAsync(update);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("trip_consolidated_updated");
        (await _db.Set<Trip>().CountAsync(t => t.Code == "PT-TRIP-palm-trip-004")).Should().Be(1);
        (await _db.Set<Trip>().FirstAsync(t => t.Code == "PT-TRIP-palm-trip-004")).Status.Should().Be("Completed");
    }

    [Fact]
    public async Task ProcessAsync_VehicleCreated_RegistersReference_ForLaterResolution()
    {
        var vehicleMessage = MakeMessage("vehicle.created", "veh-k1", new
        {
            id = "palm-veh-ref-1",
            licensePlate = "REF-0001",
            model = "F-150",
        });

        (await _sut.ProcessAsync(vehicleMessage)).Success.Should().BeTrue();

        var reference = await _db.Set<FleetExternalReference>().FirstOrDefaultAsync(
            r => r.EntityType == "Vehicle" && r.ExternalId == "palm-veh-ref-1");
        reference.Should().NotBeNull("vehicle.created debe registrar la referencia para trip/fuel/maintenance");
        reference!.Status.Should().Be("synced");
    }

    [Fact]
    public async Task ProcessAsync_FuelLogCreated_CreatesRefill_AndUpdatesVehicleKm()
    {
        var vehicle = await SeedVehicleAsync();
        vehicle.CurrentKm = 1500;
        await _db.SaveChangesAsync();

        var message = MakeMessage("fuel_log.created", "fuel-k1", new
        {
            id = "palm-fuel-001",
            vehicleId = "palm-veh-1",
            date = "2026-09-03T09:00:00Z",
            liters = 50.0,
            cost = 250.0,
            mileage = 1600.0,
            fuelType = "diesel",
            station = "UNO Central",
        });

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("fuel_log_consolidated");

        var refill = await _db.Set<FuelRefill>().SingleAsync(r => r.VehicleId == vehicle.Id);
        refill.Liters.Should().Be(50.0m);
        refill.TotalCost.Should().Be(250.0m);
        refill.PricePerLiter.Should().Be(5.0m);
        refill.CurrentKm.Should().Be(1600.0m);
        refill.ValidForCalculation.Should().BeTrue("plan §6: el refill entra al cálculo de rendimiento");
        refill.Observations.Should().Contain("UNO Central");

        // El odómetro del vehículo avanza con el fuel log
        (await _db.Set<Vehicle>().FirstAsync(v => v.Id == vehicle.Id)).CurrentKm.Should().Be(1600.0m);
    }

    [Fact]
    public async Task ProcessAsync_FuelLogCreated_MissingLiters_ShouldFail()
    {
        await SeedVehicleAsync();

        var message = MakeMessage("fuel_log.created", "fuel-k2", new
        {
            id = "palm-fuel-002",
            vehicleId = "palm-veh-1",
            date = "2026-09-03T09:00:00Z",
        });

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("liters_required");
    }

    [Fact]
    public async Task ProcessAsync_FuelLogDuplicate_ShouldNotDuplicate()
    {
        await SeedVehicleAsync();

        var message = MakeMessage("fuel_log.created", "fuel-k3", new
        {
            id = "palm-fuel-003",
            vehicleId = "palm-veh-1",
            date = "2026-09-04T09:00:00Z",
            liters = 40.0,
            cost = 200.0,
        });

        (await _sut.ProcessAsync(message)).Success.Should().BeTrue();
        (await _sut.ProcessAsync(message)).Message.Should().Be("fuel_log_consolidated_updated");

        (await _db.Set<FuelRefill>().CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task ProcessAsync_MaintenanceCorrectivo_CreatesWorkOrder()
    {
        await SeedVehicleAsync();

        var message = MakeMessage("maintenance_log.created", "maint-k1", new
        {
            id = "palm-maint-001",
            machineryId = "palm-veh-1",
            maintenanceType = "correctivo",
            description = "Falla de hidráulico",
            date = "2026-09-05T10:00:00Z",
            cost = 320.0,
            technician = "Mecánico 1",
        });

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("maintenance_consolidated_workorder");

        var workOrder = await _db.Set<WorkOrder>().SingleAsync(w => w.Number == "PT-WO-palm-maint-001");
        workOrder.Status.Should().Be("Reported");
        workOrder.CostTotal.Should().Be(320.0m);
        workOrder.ProblemDescription.Should().Contain("Falla de hidráulico");
        workOrder.ProblemDescription.Should().Contain("Mecánico 1");
    }

    [Fact]
    public async Task ProcessAsync_MaintenancePreventivo_CreatesSchedule()
    {
        await SeedVehicleAsync();

        var message = MakeMessage("maintenance_log.created", "maint-k2", new
        {
            id = "palm-maint-002",
            machineryId = "palm-veh-1",
            maintenanceType = "preventivo",
            description = "Cambio de aceite",
            date = "2026-09-05T08:00:00Z",
            nextMaintenance = "2026-12-05T08:00:00Z",
        });

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("maintenance_consolidated_schedule");

        var schedule = await _db.Set<MaintenanceSchedule>().SingleAsync(s => s.VehicleId != Guid.Empty);
        schedule.LastExecutionDate.Should().NotBeNull();
        schedule.NextExecutionDate.Should().NotBeNull();
        schedule.IntervalValue.Should().BeGreaterThan(0);
        schedule.Status.Should().Be("Active");
    }

    [Fact]
    public async Task ProcessAsync_MaintenanceDuplicate_ShouldNotDuplicate()
    {
        await SeedVehicleAsync();

        var message = MakeMessage("maintenance_log.created", "maint-k3", new
        {
            id = "palm-maint-003",
            machineryId = "palm-veh-1",
            maintenanceType = "correctivo",
            description = "Falla X",
            date = "2026-09-06T10:00:00Z",
            cost = 100.0,
        });

        (await _sut.ProcessAsync(message)).Success.Should().BeTrue();
        (await _sut.ProcessAsync(message)).Message.Should().Be("maintenance_consolidated_workorder_updated");

        (await _db.Set<WorkOrder>().CountAsync(w => w.Number == "PT-WO-palm-maint-003")).Should().Be(1);
    }

    // =========================================================================
    // Consolidación Venta / Inventario / Gasto (doc §1-3, ex §4-6)
    // =========================================================================

    private async Task<Product> SeedProductAsync(string name = "Palma Aceite", int stock = 100)
    {
        var product = new Product
        {
            Code = "PRD-SEED-1",
            Name = name,
            UnitOfMeasure = "kg",
            CostPrice = 10m,
            SellingPrice = 20m,
            Stock = stock,
            TenantId = _tenantId.ToString(),
            CompanyId = _tenantId,
        };
        _db.Set<Product>().Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    private async Task<Account> SeedAccountAsync(string code, string name)
    {
        var account = new Account
        {
            Code = code,
            Name = name,
            Type = "Expense",
            NormalSide = "Debit",
            IsActive = true,
            CompanyId = _tenantId,
            TenantId = _tenantId.ToString(),
        };
        _db.Set<Account>().Add(account);
        await _db.SaveChangesAsync();
        return account;
    }

    private async Task<AccountingPeriod> SeedOpenPeriodAsync(int year = 2026, int month = 9)
    {
        var period = new AccountingPeriod
        {
            Year = year,
            Month = month,
            Name = $"{year}-{month:D2}",
            Status = "Open",
            TenantId = _tenantId.ToString(),
            CompanyId = _tenantId,
        };
        _db.Set<AccountingPeriod>().Add(period);
        await _db.SaveChangesAsync();
        return period;
    }

    [Fact]
    public async Task ProcessAsync_SaleCreated_WithExistingProduct_CreatesSaleAndMovement()
    {
        var product = await SeedProductAsync(stock: 100);

        var message = MakeMessage("sale.created", "sale-k1", new
        {
            id = "palm-sale-001",
            client = "Juan Comprador",
            productName = "Palma Aceite",
            unitOfMeasure = "kg",
            quantity = 25.5,
            unitPrice = 20.0,
            totalAmount = 510.0,
            currency = "USD",
            paymentMethod = "Efectivo",
            paymentStatus = "Pagado",
            date = "2026-09-01T15:00:00Z",
        });

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("sale_consolidated");

        var sale = await _db.Set<Sale>().Include(s => s.Details).SingleAsync(s => s.Notes!.Contains("palm-sale-001") == false);
        sale.ClientId.Should().NotBe(Guid.Empty);
        sale.Total.Should().Be(510.0m);
        sale.PaidAmount.Should().Be(510.0m);
        sale.Balance.Should().Be(0m);
        sale.Status.Should().Be("completed");
        sale.Details.Should().HaveCount(1);
        sale.Details.First().ProductId.Should().Be(product.Id);

        // Kardex: descarga de stock + movimiento (25.5 → 26 por redondeo)
        (await _db.Set<Product>().FirstAsync(p => p.Id == product.Id)).Stock.Should().Be(74);
        var movement = await _db.Set<InventoryMovement>().SingleAsync(m => m.MovementType == "sale");
        movement.Quantity.Should().Be(26);
        movement.StockBefore.Should().Be(100);
        movement.StockAfter.Should().Be(74);

        // Cliente creado por nombre
        var client = await _db.Set<Client>().SingleAsync(c => c.FirstName == "Juan Comprador");
        sale.ClientId.Should().Be(client.Id);
    }

    [Fact]
    public async Task ProcessAsync_SaleCreated_UnknownProduct_ShouldFailForReconciliation()
    {
        var message = MakeMessage("sale.created", "sale-k2", new
        {
            id = "palm-sale-002",
            client = "Otro Cliente",
            productName = "Producto Inexistente",
            quantity = 5.0,
            unitPrice = 10.0,
            totalAmount = 50.0,
        });

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("product_unresolved");
        (await _db.Set<Sale>().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ProcessAsync_SaleCreated_SameClientReused_NotDuplicated()
    {
        await SeedProductAsync();

        var msg1 = MakeMessage("sale.created", "sale-k3", new
        {
            id = "palm-sale-003", client = "Cliente Recurrente", productName = "Palma Aceite",
            quantity = 1.0, unitPrice = 20.0, totalAmount = 20.0, paymentStatus = "Pagado",
        });
        var msg2 = MakeMessage("sale.created", "sale-k4", new
        {
            id = "palm-sale-004", client = "Cliente Recurrente", productName = "Palma Aceite",
            quantity = 2.0, unitPrice = 20.0, totalAmount = 40.0, paymentStatus = "Pagado",
        });

        (await _sut.ProcessAsync(msg1)).Success.Should().BeTrue();
        (await _sut.ProcessAsync(msg2)).Success.Should().BeTrue();

        (await _db.Set<Client>().CountAsync(c => c.FirstName == "Cliente Recurrente")).Should().Be(1);
    }

    [Fact]
    public async Task ProcessAsync_SaleDuplicate_ShouldNotDuplicate()
    {
        await SeedProductAsync();

        var message = MakeMessage("sale.created", "sale-k5", new
        {
            id = "palm-sale-005", client = "C", productName = "Palma Aceite",
            quantity = 1.0, unitPrice = 20.0, totalAmount = 20.0, paymentStatus = "Pagado",
        });

        (await _sut.ProcessAsync(message)).Success.Should().BeTrue();
        (await _sut.ProcessAsync(message)).Message.Should().Be("sale_consolidated_updated");

        (await _db.Set<Sale>().CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task ProcessAsync_InventoryUpdated_NewItem_CreatesProductAndReference()
    {
        var message = MakeMessage("inventory.updated", "inv-k1", new
        {
            id = "palm-item-001",
            productName = "Fertilizante X",
            unit = "kg",
            stock = 500.0,
            unitCost = 3.5,
        });

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("inventory_item_consolidated");

        var product = await _db.Set<Product>().SingleAsync(p => p.Code == "PT-PROD-palm-item-001");
        product.Name.Should().Be("Fertilizante X");
        product.Stock.Should().Be(500);
        product.CostPrice.Should().Be(3.5m);

        var reference = await _db.Set<FleetExternalReference>().SingleAsync(
            r => r.EntityType == "Product" && r.ExternalId == "palm-item-001");
        reference.EntityId.Should().Be(product.Id);
    }

    [Fact]
    public async Task ProcessAsync_InventoryEntry_CreatesMovementAndUpdatesStock()
    {
        var product = await SeedProductAsync(stock: 100);
        _db.Set<FleetExternalReference>().Add(new FleetExternalReference
        {
            ExternalSystem = "palmtrack", EntityType = "Product",
            ExternalId = "palm-item-002", EntityId = product.Id,
            Status = "synced", TenantId = _tenantId.ToString(),
        });
        await _db.SaveChangesAsync();

        var message = MakeMessage("inventory.entry.created", "inv-k2", new
        {
            id = "palm-entry-001",
            inventoryItemId = "palm-item-002",
            quantity = 50.0,
            unitCost = 10.0,
            entryDate = "2026-09-02T10:00:00Z",
        });

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("inventory_movement_consolidated");

        var movement = await _db.Set<InventoryMovement>().SingleAsync(m => m.MovementType == "entry");
        movement.Quantity.Should().Be(50);
        movement.StockBefore.Should().Be(100);
        movement.StockAfter.Should().Be(150);
        (await _db.Set<Product>().FirstAsync(p => p.Id == product.Id)).Stock.Should().Be(150);
    }

    [Fact]
    public async Task ProcessAsync_InventoryExit_DoesNotGoNegative()
    {
        var product = await SeedProductAsync(stock: 10);
        _db.Set<FleetExternalReference>().Add(new FleetExternalReference
        {
            ExternalSystem = "palmtrack", EntityType = "Product",
            ExternalId = "palm-item-003", EntityId = product.Id,
            Status = "synced", TenantId = _tenantId.ToString(),
        });
        await _db.SaveChangesAsync();

        var message = MakeMessage("inventory.exit.created", "inv-k3", new
        {
            id = "palm-exit-001",
            inventoryItemId = "palm-item-003",
            quantity = 25.0,
        });

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeTrue();
        var movement = await _db.Set<InventoryMovement>().SingleAsync(m => m.MovementType == "exit");
        movement.StockAfter.Should().Be(0, "el stock nunca debe quedar negativo");
        (await _db.Set<Product>().FirstAsync(p => p.Id == product.Id)).Stock.Should().Be(0);
    }

    [Fact]
    public async Task ProcessAsync_InventoryMovement_UnresolvedProduct_ShouldFail()
    {
        var message = MakeMessage("inventory.exit.created", "inv-k4", new
        {
            id = "palm-exit-002",
            inventoryItemId = "item-desconocido",
            quantity = 5.0,
        });

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("product_unresolved");
    }

    [Fact]
    public async Task ProcessAsync_ExpenseCreated_CreatesDraftEntryWithSeededAccounts()
    {
        await SeedAccountAsync("6.1.01", "Gastos Administrativos");
        await SeedAccountAsync("1.1.01", "Caja General");
        await SeedOpenPeriodAsync(2026, 9);

        var message = MakeMessage("expense.created", "exp-k1", new
        {
            id = "palm-exp-001",
            producerCode = "PROD001",
            date = "2026-09-05T12:00:00Z",
            category = "administrative",
            description = "Combino de oficina",
            amount = 120.0,
            currency = "USD",
        });

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("expense_consolidated_draft");

        var entry = await _db.Set<AccountingEntry>().SingleAsync();
        entry.Status.Should().Be("draft", "la cola de reconciliación contable exige asiento DRAFT");
        entry.TotalDebit.Should().Be(120.0m);
        entry.TotalCredit.Should().Be(120.0m);
        var detailList = await _db.Set<AccountingEntryDetail>().Where(d => d.AccountingEntryId == entry.Id).ToListAsync();
        detailList.Should().HaveCount(2);
        detailList.Single(d => d.DebitAmount > 0).AccountId.Should().Be((await _db.Set<Account>().FirstAsync(a => a.Code == "6.1.01")).Id);
        detailList.Single(d => d.CreditAmount > 0).AccountId.Should().Be((await _db.Set<Account>().FirstAsync(a => a.Code == "1.1.01")).Id);
    }

    [Fact]
    public async Task ProcessAsync_ExpenseCreated_OperativeCategory_UsesSalesExpenseAccount()
    {
        await SeedAccountAsync("6.1.01", "Gastos Administrativos");
        await SeedAccountAsync("6.1.02", "Gastos de Venta");
        await SeedAccountAsync("1.1.01", "Caja General");
        await SeedOpenPeriodAsync(2026, 9);

        var message = MakeMessage("expense.created", "exp-k2", new
        {
            id = "palm-exp-002",
            date = "2026-09-05T12:00:00Z",
            category = "operative",
            description = "Fletes",
            amount = 80.0,
        });

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeTrue();
        var entries = await _db.Set<AccountingEntry>().Where(e => e.ReferenceType == "PalmTrackExpense").ToListAsync();
        var entry = entries.Single(e => e.Description.Contains("operative"));
        var details = await _db.Set<AccountingEntryDetail>().Where(d => d.AccountingEntryId == entry.Id).ToListAsync();
        var debitDetail = details.Single(d => d.DebitAmount > 0);
        var debitAccount = await _db.Set<Account>().FirstAsync(a => a.Id == debitDetail.AccountId);
        debitAccount.Code.Should().Be("6.1.02");
    }

    [Fact]
    public async Task ProcessAsync_ExpenseCreated_ClosedPeriod_ShouldFail()
    {
        await SeedAccountAsync("6.1.01", "Gastos Administrativos");
        await SeedAccountAsync("1.1.01", "Caja General");
        // Periodo de septiembre NO sembrado → cerrado/inexistente

        var message = MakeMessage("expense.created", "exp-k3", new
        {
            id = "palm-exp-003",
            date = "2026-09-05T12:00:00Z",
            category = "administrative",
            description = "Gasto sin periodo",
            amount = 50.0,
        });

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("accounting_period_closed");
    }

    [Fact]
    public async Task ProcessAsync_ExpenseDuplicate_ShouldNotDuplicate()
    {
        await SeedAccountAsync("6.1.01", "Gastos Administrativos");
        await SeedAccountAsync("1.1.01", "Caja General");
        await SeedOpenPeriodAsync(2026, 9);

        var message = MakeMessage("expense.created", "exp-k4", new
        {
            id = "palm-exp-004",
            date = "2026-09-05T12:00:00Z",
            category = "administrative",
            description = "Duplicado",
            amount = 30.0,
        });

        (await _sut.ProcessAsync(message)).Success.Should().BeTrue();
        (await _sut.ProcessAsync(message)).Message.Should().Be("expense_consolidated_updated");

        (await _db.Set<AccountingEntry>().CountAsync(e => e.ReferenceType == "PalmTrackExpense")).Should().Be(1);
    }

    // =========================================================================
    // Consolidación Mano de Obra (labor_log.created → AttendanceRecord)
    // =========================================================================

    private async Task<Employee> SeedEmployeeAsync(string firstName, string lastName)
    {
        var employee = new Employee
        {
            FirstName = firstName,
            LastName = lastName,
            Status = "active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            TenantId = _tenantId.ToString(),
            CompanyId = _tenantId,
        };
        _db.Set<Employee>().Add(employee);
        await _db.SaveChangesAsync();
        return employee;
    }

    [Fact]
    public async Task ProcessAsync_LaborLogCreated_WithKnownCollaborator_CreatesAttendance()
    {
        await SeedEmployeeAsync("Pedro", "Ramírez");

        var message = MakeMessage("labor_log.created", "labor-k1", new
        {
            id = "palm-labor-001",
            producerCode = "PROD001",
            activityCategory = "cosecha",
            activity = "Cosecha de racimos",
            date = "2026-09-04T14:00:00Z",
            collaborators = "Pedro Ramírez",
            cost = "500",
            currency = "NIO",
            status = "Directa",
        });

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("labor_log_consolidated");

        var record = await _db.Set<AttendanceRecord>().SingleAsync();
        record.Status.Should().Be("present");
        record.Date.Should().Be(DateOnly.FromDateTime(new DateTime(2026, 9, 4)));
        record.Notes.Should().Contain("PT-palm-labor-001");
        record.Notes.Should().Contain("cosecha/Cosecha de racimos");
        record.Notes.Should().Contain("Jornal: 500");
        record.TenantId.Should().Be(_tenantId.ToString());
    }

    [Fact]
    public async Task ProcessAsync_LaborLogCreated_MultipleCollaborators_CreatesOneRecordEach()
    {
        await SeedEmployeeAsync("Ana", "López");
        await SeedEmployeeAsync("Luis", "Martínez");

        var message = MakeMessage("labor_log.created", "labor-k2", new
        {
            id = "palm-labor-002",
            activity = "Fumigación",
            date = "2026-09-05T14:00:00Z",
            collaborators = "Ana López, Luis Martínez",
            cost = "800",
            status = "Directa",
        });

        var result = await _sut.ProcessAsync(message);

        result.Success.Should().BeTrue();
        (await _db.Set<AttendanceRecord>().CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task ProcessAsync_LaborLogCreated_UnknownCollaborator_ReturnsPartial()
    {
        await SeedEmployeeAsync("Ana", "López");

        var message = MakeMessage("labor_log.created", "labor-k3", new
        {
            id = "palm-labor-003",
            activity = "Deshierbe",
            date = "2026-09-06T14:00:00Z",
            collaborators = "Ana López, Colaborador Sin Mapear",
            cost = "300",
            status = "Indirecta",
        });

        var result = await _sut.ProcessAsync(message);

        // Cola de reconciliación de identidad (doc §7): parcial con nombres sin resolver.
        result.Success.Should().BeTrue();
        result.Message.Should().StartWith("labor_log_consolidated_partial:");
        result.Message.Should().Contain("Colaborador Sin Mapear");
        (await _db.Set<AttendanceRecord>().CountAsync()).Should().Be(1); // solo Ana
    }

    [Fact]
    public async Task ProcessAsync_LaborLogCreated_AllUnknown_ShouldFail()
    {
        var message = MakeMessage("labor_log.created", "labor-k4", new
        {
            id = "palm-labor-004",
            activity = "Siembra",
            date = "2026-09-06T14:00:00Z",
            collaborators = "Nadie Conocido",
            cost = "100",
            status = "Directa",
        });

        var result = await _sut.ProcessAsync(message);

        // Sin NINGÚN colaborador resoluble → no consolida nada (fail-closed).
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("labor_log_unresolved");
        (await _db.Set<AttendanceRecord>().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ProcessAsync_LaborLogDuplicate_ShouldNotDuplicate()
    {
        await SeedEmployeeAsync("Pedro", "Ramírez");

        var message = MakeMessage("labor_log.created", "labor-k5", new
        {
            id = "palm-labor-005",
            activity = "Cosecha",
            date = "2026-09-04T14:00:00Z",
            collaborators = "Pedro Ramírez",
            cost = "500",
            status = "Directa",
        });

        (await _sut.ProcessAsync(message)).Success.Should().BeTrue();
        (await _sut.ProcessAsync(message)).Success.Should().BeTrue();

        (await _db.Set<AttendanceRecord>().CountAsync()).Should().Be(1);
    }
}
