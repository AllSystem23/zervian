import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../auth/auth_provider.dart' show dioClientProvider;

// ═══════════════════════════════════════════
// Feature Flags State
// ═══════════════════════════════════════════

/// Plan §10.1 — Feature flags for PalmTrack module activation.
/// All flags default to false for gradual activation.
final class PalmTrackFeatureFlags {
  /// Master flag: if false, the entire PalmTrack module is hidden.
  final bool moduleEnabled;

  /// Plan §10.1 — SSO enabled between PalmTrack and Zorvian.
  final bool ssoEnabled;

  /// Plan §10.1 — Auto-create Zorvian users from PalmTrack SSO.
  final bool ssoAutoCreateUsers;

  /// Plan §10.1 — Propagate role changes from PalmTrack.
  final bool ssoPropagateRoles;

  /// Plan §10.1 — Validate shared Firebase project.
  final bool ssoSharedProject;

  const PalmTrackFeatureFlags({
    this.moduleEnabled = false,
    this.ssoEnabled = false,
    this.ssoAutoCreateUsers = false,
    this.ssoPropagateRoles = false,
    this.ssoSharedProject = false,
  });

  factory PalmTrackFeatureFlags.fromJson(Map<String, dynamic> j) =>
      PalmTrackFeatureFlags(
        moduleEnabled: j['moduleEnabled'] as bool? ?? false,
        ssoEnabled: j['ssoEnabled'] as bool? ?? false,
        ssoAutoCreateUsers: j['ssoAutoCreateUsers'] as bool? ?? false,
        ssoPropagateRoles: j['ssoPropagateRoles'] as bool? ?? false,
        ssoSharedProject: j['ssoSharedProject'] as bool? ?? false,
      );

  PalmTrackFeatureFlags copyWith({
    bool? moduleEnabled,
    bool? ssoEnabled,
    bool? ssoAutoCreateUsers,
    bool? ssoPropagateRoles,
    bool? ssoSharedProject,
  }) =>
      PalmTrackFeatureFlags(
        moduleEnabled: moduleEnabled ?? this.moduleEnabled,
        ssoEnabled: ssoEnabled ?? this.ssoEnabled,
        ssoAutoCreateUsers: ssoAutoCreateUsers ?? this.ssoAutoCreateUsers,
        ssoPropagateRoles: ssoPropagateRoles ?? this.ssoPropagateRoles,
        ssoSharedProject: ssoSharedProject ?? this.ssoSharedProject,
      );

  Map<String, dynamic> toJson() => {
        'moduleEnabled': moduleEnabled,
        'ssoEnabled': ssoEnabled,
        'ssoAutoCreateUsers': ssoAutoCreateUsers,
        'ssoPropagateRoles': ssoPropagateRoles,
        'ssoSharedProject': ssoSharedProject,
      };
}

// ═══════════════════════════════════════════
// Provider
// ═══════════════════════════════════════════

final class PalmTrackFeatureFlagsNotifier
    extends Notifier<PalmTrackFeatureFlags> {
  @override
  PalmTrackFeatureFlags build() => const PalmTrackFeatureFlags();

  Future<void> load() async {
    try {
      final dio = ref.read(dioClientProvider);
      final response = await dio.get('/settings/palmtrack/feature-flags');
      final data = response.data;
      if (data is Map<String, dynamic>) {
        state = PalmTrackFeatureFlags.fromJson(data);
      }
    } on DioException catch (e) {
      // If 404, flags haven't been configured yet — keep defaults (all false)
      if (e.response?.statusCode != 404) {
        // On other errors, keep defaults silently
      }
    } catch (_) {
      // Keep defaults on error
    }
  }

  Future<void> updateFlag(String key, bool value) async {
    final previous = state;
    // Optimistic update
    state = state.copyWith(
      moduleEnabled:
          key == 'moduleEnabled' ? value : state.moduleEnabled,
      ssoEnabled: key == 'ssoEnabled' ? value : state.ssoEnabled,
      ssoAutoCreateUsers:
          key == 'ssoAutoCreateUsers' ? value : state.ssoAutoCreateUsers,
      ssoPropagateRoles:
          key == 'ssoPropagateRoles' ? value : state.ssoPropagateRoles,
      ssoSharedProject:
          key == 'ssoSharedProject' ? value : state.ssoSharedProject,
    );

    try {
      final dio = ref.read(dioClientProvider);
      await dio.put(
        '/settings/palmtrack/feature-flags',
        data: state.toJson(),
      );
    } catch (_) {
      // Revert on error
      state = previous;
    }
  }
}

final palmTrackFeatureFlagsProvider =
    NotifierProvider<PalmTrackFeatureFlagsNotifier, PalmTrackFeatureFlags>(
      PalmTrackFeatureFlagsNotifier.new,
    );

/// Convenience provider to check if PalmTrack module should be visible.
final isPalmTrackModuleEnabled = Provider<bool>((ref) {
  final flags = ref.watch(palmTrackFeatureFlagsProvider);
  return flags.moduleEnabled;
});
