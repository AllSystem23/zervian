library;

/// Data models for PalmTrack entities exposed via read-only API.
/// Plan §3.2: farms, lots, producers, sales-logs, inventory.
/// Models generated from Zorvian backend DTOs (not Zod schemas).

// ═══════════════════════════════════════════
// Farm
// ═══════════════════════════════════════════

final class PalmTrackFarm {
  final String id;
  final String name;
  final String? location;
  final int totalLots;
  final int activeLots;
  final String organizationId;

  const PalmTrackFarm({
    required this.id,
    required this.name,
    this.location,
    this.totalLots = 0,
    this.activeLots = 0,
    required this.organizationId,
  });

  factory PalmTrackFarm.fromJson(Map<String, dynamic> j) => PalmTrackFarm(
        id: j['id'] as String? ?? '',
        name: j['name'] as String? ?? '',
        location: j['location'] as String?,
        totalLots: (j['totalLots'] as num?)?.toInt() ?? 0,
        activeLots: (j['activeLots'] as num?)?.toInt() ?? 0,
        organizationId: j['organizationId'] as String? ?? '',
      );
}

// ═══════════════════════════════════════════
// Lot
// ═══════════════════════════════════════════

final class PalmTrackLot {
  final String id;
  final String name;
  final String? farmId;
  final String? farmName;
  final String? cropVariety;
  final int? areaHectares;
  final String organizationId;

  const PalmTrackLot({
    required this.id,
    required this.name,
    this.farmId,
    this.farmName,
    this.cropVariety,
    this.areaHectares,
    required this.organizationId,
  });

  factory PalmTrackLot.fromJson(Map<String, dynamic> j) => PalmTrackLot(
        id: j['id'] as String? ?? '',
        name: j['name'] as String? ?? '',
        farmId: j['farmId'] as String?,
        farmName: j['farmName'] as String?,
        cropVariety: j['cropVariety'] as String?,
        areaHectares: (j['areaHectares'] as num?)?.toInt(),
        organizationId: j['organizationId'] as String? ?? '',
      );
}

// ═══════════════════════════════════════════
// Producer
// ═══════════════════════════════════════════

final class PalmTrackProducer {
  final String id;
  final String name;
  final String? code;
  final String? phone;
  final String? email;
  final int totalFarms;
  final String organizationId;

  const PalmTrackProducer({
    required this.id,
    required this.name,
    this.code,
    this.phone,
    this.email,
    this.totalFarms = 0,
    required this.organizationId,
  });

  factory PalmTrackProducer.fromJson(Map<String, dynamic> j) =>
      PalmTrackProducer(
        id: j['id'] as String? ?? '',
        name: j['name'] as String? ?? '',
        code: j['code'] as String?,
        phone: j['phone'] as String?,
        email: j['email'] as String?,
        totalFarms: (j['totalFarms'] as num?)?.toInt() ?? 0,
        organizationId: j['organizationId'] as String? ?? '',
      );
}

// ═══════════════════════════════════════════
// Sales Log
// ═══════════════════════════════════════════

final class PalmTrackSalesLog {
  final String id;
  final String? clientName;
  final String? productName;
  final double quantity;
  final double unitPrice;
  final double totalAmount;
  final String? currency;
  final String? paymentMethod;
  final String? paymentStatus;
  final String date;
  final String organizationId;

  const PalmTrackSalesLog({
    required this.id,
    this.clientName,
    this.productName,
    this.quantity = 0,
    this.unitPrice = 0,
    this.totalAmount = 0,
    this.currency,
    this.paymentMethod,
    this.paymentStatus,
    required this.date,
    required this.organizationId,
  });

  factory PalmTrackSalesLog.fromJson(Map<String, dynamic> j) =>
      PalmTrackSalesLog(
        id: j['id'] as String? ?? '',
        clientName: j['clientName'] as String?,
        productName: j['productName'] as String?,
        quantity: (j['quantity'] as num?)?.toDouble() ?? 0,
        unitPrice: (j['unitPrice'] as num?)?.toDouble() ?? 0,
        totalAmount: (j['totalAmount'] as num?)?.toDouble() ?? 0,
        currency: j['currency'] as String?,
        paymentMethod: j['paymentMethod'] as String?,
        paymentStatus: j['paymentStatus'] as String?,
        date: j['date'] as String? ?? '',
        organizationId: j['organizationId'] as String? ?? '',
      );
}

// ═══════════════════════════════════════════
// Inventory
// ═══════════════════════════════════════════

final class PalmTrackInventory {
  final String id;
  final String? productName;
  final String? code;
  final String? unit;
  final double stock;
  final double unitCost;
  final String organizationId;

  const PalmTrackInventory({
    required this.id,
    this.productName,
    this.code,
    this.unit,
    this.stock = 0,
    this.unitCost = 0,
    required this.organizationId,
  });

  factory PalmTrackInventory.fromJson(Map<String, dynamic> j) =>
      PalmTrackInventory(
        id: j['id'] as String? ?? '',
        productName: j['productName'] as String?,
        code: j['code'] as String?,
        unit: j['unit'] as String?,
        stock: (j['stock'] as num?)?.toDouble() ?? 0,
        unitCost: (j['unitCost'] as num?)?.toDouble() ?? 0,
        organizationId: j['organizationId'] as String? ?? '',
      );
}

// ═══════════════════════════════════════════
// Paginated Response (cursor-based)
// ═══════════════════════════════════════════

final class PalmTrackPage<T> {
  final List<T> items;
  final String? nextCursor;
  final bool hasMore;

  const PalmTrackPage({
    required this.items,
    this.nextCursor,
    this.hasMore = false,
  });
}
