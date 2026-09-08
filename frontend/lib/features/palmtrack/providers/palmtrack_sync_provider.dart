import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../auth/auth_provider.dart' show dioClientProvider;

// ═══════════════════════════════════════════
// Data Models
// ═══════════════════════════════════════════

final class PalmTrackSyncStats {
  final int totalReferences;
  final int syncedCount;
  final int pendingCount;
  final int conflictCount;
  final int errorCount;
  final String? lastSyncAt;
  final int vehicleCount;
  final int driverAliasCount;

  const PalmTrackSyncStats({
    this.totalReferences = 0,
    this.syncedCount = 0,
    this.pendingCount = 0,
    this.conflictCount = 0,
    this.errorCount = 0,
    this.lastSyncAt,
    this.vehicleCount = 0,
    this.driverAliasCount = 0,
  });

  factory PalmTrackSyncStats.fromJson(Map<String, dynamic> j) =>
      PalmTrackSyncStats(
        totalReferences: (j['totalReferences'] as num?)?.toInt() ?? 0,
        syncedCount: (j['syncedCount'] as num?)?.toInt() ?? 0,
        pendingCount: (j['pendingCount'] as num?)?.toInt() ?? 0,
        conflictCount: (j['conflictCount'] as num?)?.toInt() ?? 0,
        errorCount: (j['errorCount'] as num?)?.toInt() ?? 0,
        lastSyncAt: j['lastSyncAt'] as String?,
        vehicleCount: (j['vehicleCount'] as num?)?.toInt() ?? 0,
        driverAliasCount: (j['driverAliasCount'] as num?)?.toInt() ?? 0,
      );

  double get syncRate =>
      totalReferences > 0 ? syncedCount / totalReferences : 0;
}

final class ExternalReference {
  final String id;
  final String entityType;
  final String entityId;
  final String externalId;
  final String? externalPayload;
  final String? lastSyncAt;
  final String syncDirection;
  final String status;
  final String? lastError;
  final int consecutiveFailures;

  const ExternalReference({
    required this.id,
    required this.entityType,
    required this.entityId,
    required this.externalId,
    this.externalPayload,
    this.lastSyncAt,
    this.syncDirection = 'bidirectional',
    this.status = 'pending',
    this.lastError,
    this.consecutiveFailures = 0,
  });

  factory ExternalReference.fromJson(Map<String, dynamic> j) =>
      ExternalReference(
        id: j['id'] as String? ?? '',
        entityType: j['entityType'] as String? ?? '',
        entityId: j['entityId'] as String? ?? '',
        externalId: j['externalId'] as String? ?? '',
        externalPayload: j['externalPayload'] as String?,
        lastSyncAt: j['lastSyncAt'] as String?,
        syncDirection: j['syncDirection'] as String? ?? 'bidirectional',
        status: j['status'] as String? ?? 'pending',
        lastError: j['lastError'] as String?,
        consecutiveFailures: (j['consecutiveFailures'] as num?)?.toInt() ?? 0,
      );
}

/// Plan §3.3 — Webhook delivery history entry.
/// Maps to backend PalmTrackWebhookLog entity.
final class WebhookDelivery {
  final String id;
  final String event;
  final String? receivedAt;
  final bool success;
  final int attempt;
  final int maxRetries;
  final int? httpStatusCode;
  final String? errorMessage;
  final int? durationMs;
  final String? idempotencyKey;

  const WebhookDelivery({
    required this.id,
    required this.event,
    this.receivedAt,
    this.success = false,
    this.attempt = 1,
    this.maxRetries = 3,
    this.httpStatusCode,
    this.errorMessage,
    this.durationMs,
    this.idempotencyKey,
  });

  factory WebhookDelivery.fromJson(Map<String, dynamic> j) =>
      WebhookDelivery(
        id: j['id'] as String? ?? '',
        event: j['event'] as String? ?? '',
        receivedAt: j['receivedAt'] as String?,
        success: j['success'] as bool? ?? false,
        attempt: (j['attempt'] as num?)?.toInt() ?? 1,
        maxRetries: (j['maxRetries'] as num?)?.toInt() ?? 3,
        httpStatusCode: (j['httpStatusCode'] as num?)?.toInt(),
        errorMessage: j['errorMessage'] as String?,
        durationMs: (j['durationMs'] as num?)?.toInt(),
        idempotencyKey: j['idempotencyKey'] as String?,
      );
}

// ═══════════════════════════════════════════
// Sync Stats State
// ═══════════════════════════════════════════

final class PalmTrackSyncState {
  final PalmTrackSyncStats stats;
  final List<ExternalReference> references;
  final List<WebhookDelivery> deliveries;
  final bool loading;
  final String? error;

  const PalmTrackSyncState({
    this.stats = const PalmTrackSyncStats(),
    this.references = const [],
    this.deliveries = const [],
    this.loading = false,
    this.error,
  });

  PalmTrackSyncState copyWith({
    PalmTrackSyncStats? stats,
    List<ExternalReference>? references,
    List<WebhookDelivery>? deliveries,
    bool? loading,
    String? error,
  }) =>
      PalmTrackSyncState(
        stats: stats ?? this.stats,
        references: references ?? this.references,
        deliveries: deliveries ?? this.deliveries,
        loading: loading ?? this.loading,
        error: error,
      );
}

final class PalmTrackSyncNotifier extends Notifier<PalmTrackSyncState> {
  @override
  PalmTrackSyncState build() => const PalmTrackSyncState();

  Future<void> load() async {
    state = state.copyWith(loading: true, error: null);
    try {
      final dio = ref.read(dioClientProvider);
      final results = await Future.wait([
        dio.get('fleet/palmtrack/stats'),
        dio.get('fleet/palmtrack/references'),
        dio.get('zorvian/v1/palm/webhooks/dlq').catchError(
              (_) => Response(
                data: [],
                statusCode: 404,
                requestOptions: RequestOptions(path: 'zorvian/v1/palm/webhooks/logs'),
              ),
            ),
      ]);

      final statsData = results[0].data;
      final refsData = results[1].data;
      final deliveriesData = results[2].data;

      state = PalmTrackSyncState(
        stats: statsData is Map<String, dynamic>
            ? PalmTrackSyncStats.fromJson(statsData)
            : const PalmTrackSyncStats(),
        references: (refsData is List ? refsData : [])
            .whereType<Map<String, dynamic>>()
            .map((e) => ExternalReference.fromJson(e))
            .toList(),
        deliveries: (deliveriesData is List ? deliveriesData : [])
            .whereType<Map<String, dynamic>>()
            .map((e) => WebhookDelivery.fromJson(e))
            .toList(),
      );
    } on DioException {
      state = state.copyWith(
        error: 'No se pudo cargar datos de sincronización PalmTrack',
        loading: false,
      );
    } catch (_) {
      state = state.copyWith(
        error: 'Error al cargar sincronización',
        loading: false,
      );
    }
  }
}

final palmTrackSyncProvider =
    NotifierProvider<PalmTrackSyncNotifier, PalmTrackSyncState>(
      PalmTrackSyncNotifier.new,
    );
