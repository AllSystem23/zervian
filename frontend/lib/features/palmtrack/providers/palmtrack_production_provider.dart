import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';
import '../../../auth/auth_provider.dart' show dioClientProvider;

// ═══════════════════════════════════════════
// Production Summary Data Models
// ═══════════════════════════════════════════

final class ProductionSummary {
  final int totalBunches;
  final double totalWeight;
  final double avgWeight;
  final int totalBags;
  final double avgBunchWeight;
  final int activeFarms;
  final int activeLots;
  final int totalProducers;
  final List<ProductionByFarm> byFarm;
  final List<DailyProduction> dailyTrend;

  const ProductionSummary({
    this.totalBunches = 0,
    this.totalWeight = 0,
    this.avgWeight = 0,
    this.totalBags = 0,
    this.avgBunchWeight = 0,
    this.activeFarms = 0,
    this.activeLots = 0,
    this.totalProducers = 0,
    this.byFarm = const [],
    this.dailyTrend = const [],
  });

  factory ProductionSummary.fromJson(Map<String, dynamic> j) =>
      ProductionSummary(
        totalBunches: (j['totalBunches'] as num?)?.toInt() ?? 0,
        totalWeight: (j['totalWeight'] as num?)?.toDouble() ?? 0,
        avgWeight: (j['avgWeight'] as num?)?.toDouble() ?? 0,
        totalBags: (j['totalBags'] as num?)?.toInt() ?? 0,
        avgBunchWeight: (j['avgBunchWeight'] as num?)?.toDouble() ?? 0,
        activeFarms: (j['activeFarms'] as num?)?.toInt() ?? 0,
        activeLots: (j['activeLots'] as num?)?.toInt() ?? 0,
        totalProducers: (j['totalProducers'] as num?)?.toInt() ?? 0,
        byFarm: ((j['byFarm'] as List?) ?? [])
            .whereType<Map<String, dynamic>>()
            .map((e) => ProductionByFarm.fromJson(e))
            .toList(),
        dailyTrend: ((j['dailyTrend'] as List?) ?? [])
            .whereType<Map<String, dynamic>>()
            .map((e) => DailyProduction.fromJson(e))
            .toList(),
      );
}

final class ProductionByFarm {
  final String farmName;
  final int bunches;
  final double weight;

  const ProductionByFarm({
    required this.farmName,
    this.bunches = 0,
    this.weight = 0,
  });

  factory ProductionByFarm.fromJson(Map<String, dynamic> j) =>
      ProductionByFarm(
        farmName: j['farmName'] as String? ?? '',
        bunches: (j['bunches'] as num?)?.toInt() ?? 0,
        weight: (j['weight'] as num?)?.toDouble() ?? 0,
      );
}

final class DailyProduction {
  final String date;
  final int bunches;
  final double weight;

  const DailyProduction({
    required this.date,
    this.bunches = 0,
    this.weight = 0,
  });

  factory DailyProduction.fromJson(Map<String, dynamic> j) =>
      DailyProduction(
        date: j['date'] as String? ?? '',
        bunches: (j['bunches'] as num?)?.toInt() ?? 0,
        weight: (j['weight'] as num?)?.toDouble() ?? 0,
      );
}

// ═══════════════════════════════════════════
// State
// ═══════════════════════════════════════════

final class PalmTrackProductionState {
  final ProductionSummary summary;
  final bool loading;
  final String? error;
  final DateTime? startDate;
  final DateTime? endDate;

  const PalmTrackProductionState({
    this.summary = const ProductionSummary(),
    this.loading = false,
    this.error,
    this.startDate,
    this.endDate,
  });

  PalmTrackProductionState copyWith({
    ProductionSummary? summary,
    bool? loading,
    String? error,
    DateTime? startDate,
    DateTime? endDate,
  }) =>
      PalmTrackProductionState(
        summary: summary ?? this.summary,
        loading: loading ?? this.loading,
        error: error,
        startDate: startDate ?? this.startDate,
        endDate: endDate ?? this.endDate,
      );
}

// ═══════════════════════════════════════════
// Notifier
// ═══════════════════════════════════════════

final class PalmTrackProductionNotifier
    extends Notifier<PalmTrackProductionState> {
  @override
  PalmTrackProductionState build() => const PalmTrackProductionState();

  Future<void> load() async {
    state = state.copyWith(loading: true, error: null);
    try {
      final dio = ref.read(dioClientProvider);
      final queryParams = <String, dynamic>{};
      if (state.startDate != null) {
        queryParams['startDate'] = DateFormat('yyyy-MM-dd').format(state.startDate!);
      }
      if (state.endDate != null) {
        queryParams['endDate'] = DateFormat('yyyy-MM-dd').format(state.endDate!);
      }
      final response = await dio.get('/palmtrack/production/summary', params: queryParams);
      final data = response.data;

      state = PalmTrackProductionState(
        summary: data is Map<String, dynamic>
            ? ProductionSummary.fromJson(data)
            : const ProductionSummary(),
        startDate: state.startDate,
        endDate: state.endDate,
      );
    } on DioException {
      state = state.copyWith(
        error: 'No se pudo cargar el resumen de producción',
        loading: false,
      );
    } catch (_) {
      state = state.copyWith(
        error: 'Error al cargar producción',
        loading: false,
      );
    }
  }

  void setDateRange(DateTime? start, DateTime? end) {
    state = state.copyWith(startDate: start, endDate: end);
    load();
  }
}

final palmTrackProductionProvider =
    NotifierProvider<PalmTrackProductionNotifier, PalmTrackProductionState>(
      PalmTrackProductionNotifier.new,
    );
