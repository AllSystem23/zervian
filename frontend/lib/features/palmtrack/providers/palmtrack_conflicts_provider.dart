import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../auth/auth_provider.dart' show dioClientProvider;

// ═══════════════════════════════════════════
// Conflict Data Model
// ═══════════════════════════════════════════

final class SyncConflict {
  final String id;
  final String entityType;
  final String entityId;
  final String externalId;
  final String? externalPayload;
  final String? lastSyncAt;
  final String status;
  final String? lastError;
  final int consecutiveFailures;
  final String? entityTypeLabel;
  final String? entityIdLabel;

  const SyncConflict({
    required this.id,
    required this.entityType,
    required this.entityId,
    required this.externalId,
    this.externalPayload,
    this.lastSyncAt,
    this.status = 'conflict',
    this.lastError,
    this.consecutiveFailures = 0,
    this.entityTypeLabel,
    this.entityIdLabel,
  });

  factory SyncConflict.fromJson(Map<String, dynamic> j) => SyncConflict(
        id: j['id'] as String? ?? '',
        entityType: j['entityType'] as String? ?? '',
        entityId: j['entityId'] as String? ?? '',
        externalId: j['externalId'] as String? ?? '',
        externalPayload: j['externalPayload'] as String?,
        lastSyncAt: j['lastSyncAt'] as String?,
        status: j['status'] as String? ?? 'conflict',
        lastError: j['lastError'] as String?,
        consecutiveFailures:
            (j['consecutiveFailures'] as num?)?.toInt() ?? 0,
        entityTypeLabel: j['entityTypeLabel'] as String?,
        entityIdLabel: j['entityIdLabel'] as String?,
      );
}

// ═══════════════════════════════════════════
// Conflicts State
// ═══════════════════════════════════════════

final class PalmTrackConflictsState {
  final List<SyncConflict> conflicts;
  final bool loading;
  final String? error;
  final String? resolvingId;

  const PalmTrackConflictsState({
    this.conflicts = const [],
    this.loading = false,
    this.error,
    this.resolvingId,
  });

  PalmTrackConflictsState copyWith({
    List<SyncConflict>? conflicts,
    bool? loading,
    String? error,
    String? resolvingId,
  }) =>
      PalmTrackConflictsState(
        conflicts: conflicts ?? this.conflicts,
        loading: loading ?? this.loading,
        error: error,
        resolvingId: resolvingId ?? this.resolvingId,
      );
}

final class PalmTrackConflictsNotifier
    extends Notifier<PalmTrackConflictsState> {
  @override
  PalmTrackConflictsState build() => const PalmTrackConflictsState();

  Future<void> load() async {
    state = state.copyWith(loading: true, error: null);
    try {
      final dio = ref.read(dioClientProvider);
      final response = await dio.get('/fleet/palmtrack/conflicts');
      final data = response.data;

      state = PalmTrackConflictsState(
        conflicts: (data is List ? data : [])
            .whereType<Map<String, dynamic>>()
            .map((e) => SyncConflict.fromJson(e))
            .toList(),
      );
    } on DioException {
      state = state.copyWith(
        error: 'No se pudieron cargar los conflictos',
        loading: false,
      );
    } catch (_) {
      state = state.copyWith(
        error: 'Error al cargar conflictos',
        loading: false,
      );
    }
  }

  Future<bool> resolveConflict({
    required String referenceId,
    required bool acceptExternal,
  }) async {
    state = state.copyWith(resolvingId: referenceId);
    try {
      final dio = ref.read(dioClientProvider);
      await dio.post('/fleet/palmtrack/conflicts/$referenceId/resolve', data: {
        'acceptExternal': acceptExternal,
      });

      // Remove resolved conflict from list
      state = state.copyWith(
        conflicts: state.conflicts.where((c) => c.id != referenceId).toList(),
        resolvingId: null,
      );
      return true;
    } on DioException {
      state = state.copyWith(
        error: 'No se pudo resolver el conflicto',
        resolvingId: null,
      );
      return false;
    } catch (_) {
      state = state.copyWith(
        error: 'Error al resolver conflicto',
        resolvingId: null,
      );
      return false;
    }
  }
}

final palmTrackConflictsProvider =
    NotifierProvider<PalmTrackConflictsNotifier, PalmTrackConflictsState>(
      PalmTrackConflictsNotifier.new,
    );
