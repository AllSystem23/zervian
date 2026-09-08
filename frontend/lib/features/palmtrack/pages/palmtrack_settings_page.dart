import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../shared/ds/ds.dart';
import '../providers/palmtrack_feature_flags_provider.dart';

/// Plan §3.6 / §10 — Feature flags settings for PalmTrack module.
/// Allows gradual activation of the module and sub-features.
class PalmTrackSettingsPage extends ConsumerStatefulWidget {
  const PalmTrackSettingsPage({super.key});

  @override
  ConsumerState<PalmTrackSettingsPage> createState() =>
      _PalmTrackSettingsPageState();
}

class _PalmTrackSettingsPageState extends ConsumerState<PalmTrackSettingsPage> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(palmTrackFeatureFlagsProvider.notifier).load();
    });
  }

  @override
  Widget build(BuildContext context) {
    final flags = ref.watch(palmTrackFeatureFlagsProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('PalmTrack — Configuración'),
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          // ── Module Toggle ──
          ZCard(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    const Icon(Icons.agriculture_outlined, size: 24),
                    const SizedBox(width: 12),
                    const Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            'Módulo PalmTrack',
                            style: TextStyle(
                              fontSize: 18,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                          SizedBox(height: 4),
                          Text(
                            'Activar o desactivar el módulo completo de integración '
                            'con PalmTrack. Cuando está desactivado, el módulo '
                            'no aparece en la navegación.',
                            style: TextStyle(fontSize: 13, color: Colors.grey),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 16),
                SwitchListTile(
                  title: const Text(
                    'Módulo habilitado',
                    style: TextStyle(fontWeight: FontWeight.w500),
                  ),
                  subtitle: Text(
                    flags.moduleEnabled ? 'Activo — visible en el menú' : 'Inactivo — oculto del menú',
                    style: TextStyle(
                      color: flags.moduleEnabled ? Colors.green : Colors.grey,
                      fontSize: 12,
                    ),
                  ),
                  value: flags.moduleEnabled,
                  onChanged: (v) => ref
                      .read(palmTrackFeatureFlagsProvider.notifier)
                      .updateFlag('moduleEnabled', v),
                  secondary: Icon(
                    flags.moduleEnabled
                        ? Icons.toggle_on
                        : Icons.toggle_off,
                    color: flags.moduleEnabled ? Colors.green : Colors.grey,
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),

          // ── SSO Flags ──
          ZCard(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text(
                  'SSO (Single Sign-On)',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
                ),
                const SizedBox(height: 8),
                Text(
                  'Configuración del SSO compartido entre PalmTrack y Zorvian.',
                  style: TextStyle(fontSize: 13, color: Colors.grey[600]),
                ),
                const SizedBox(height: 16),

                SwitchListTile(
                  title: const Text('SSO habilitado'),
                  subtitle: const Text(
                    'Permitir inicio de sesión compartido desde PalmTrack',
                    style: TextStyle(fontSize: 12),
                  ),
                  value: flags.ssoEnabled,
                  onChanged: (v) => ref
                      .read(palmTrackFeatureFlagsProvider.notifier)
                      .updateFlag('ssoEnabled', v),
                  contentPadding: EdgeInsets.zero,
                ),

                SwitchListTile(
                  title: const Text('Auto-crear usuarios'),
                  subtitle: const Text(
                    'Crear usuarios en Zorvian automáticamente al primer SSO',
                    style: TextStyle(fontSize: 12),
                  ),
                  value: flags.ssoAutoCreateUsers,
                  onChanged: flags.ssoEnabled
                      ? (v) => ref
                          .read(palmTrackFeatureFlagsProvider.notifier)
                          .updateFlag('ssoAutoCreateUsers', v)
                      : null,
                  contentPadding: EdgeInsets.zero,
                ),

                SwitchListTile(
                  title: const Text('Propagar roles'),
                  subtitle: const Text(
                    'Sincronizar cambios de rol desde PalmTrack vía webhook',
                    style: TextStyle(fontSize: 12),
                  ),
                  value: flags.ssoPropagateRoles,
                  onChanged: flags.ssoEnabled
                      ? (v) => ref
                          .read(palmTrackFeatureFlagsProvider.notifier)
                          .updateFlag('ssoPropagateRoles', v)
                      : null,
                  contentPadding: EdgeInsets.zero,
                ),

                SwitchListTile(
                  title: const Text('Proyecto Firebase compartido'),
                  subtitle: const Text(
                    'Validar que ambos usen el mismo proyecto Firebase',
                    style: TextStyle(fontSize: 12),
                  ),
                  value: flags.ssoSharedProject,
                  onChanged: flags.ssoEnabled
                      ? (v) => ref
                          .read(palmTrackFeatureFlagsProvider.notifier)
                          .updateFlag('ssoSharedProject', v)
                      : null,
                  contentPadding: EdgeInsets.zero,
                ),
              ],
            ),
          ),

          // ── Activation phases ──
          const SizedBox(height: 16),
          ZCard(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text(
                  'Fases de Activación',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
                ),
                const SizedBox(height: 12),
                _buildPhaseRow(
                  'Fase 0',
                  'Solo API key, módulo desactivado',
                  !flags.moduleEnabled,
                ),
                _buildPhaseRow(
                  'Fase 1',
                  'Módulo activo, SSO desactivado',
                  flags.moduleEnabled && !flags.ssoEnabled,
                ),
                _buildPhaseRow(
                  'Fase 2',
                  'SSO activo, auto-crear usuarios',
                  flags.ssoEnabled && !flags.ssoAutoCreateUsers,
                ),
                _buildPhaseRow(
                  'Fase 3',
                  'SSO completo con propagación de roles',
                  flags.ssoEnabled &&
                      flags.ssoAutoCreateUsers &&
                      flags.ssoPropagateRoles,
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildPhaseRow(String phase, String description, bool active) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6),
      child: Row(
        children: [
          Icon(
            active ? Icons.radio_button_checked : Icons.radio_button_unchecked,
            size: 20,
            color: active ? Colors.green : Colors.grey,
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  phase,
                  style: TextStyle(
                    fontWeight: FontWeight.w600,
                    fontSize: 14,
                    color: active ? Colors.green[700] : Colors.grey[600],
                  ),
                ),
                Text(
                  description,
                  style: TextStyle(
                    fontSize: 12,
                    color: active ? Colors.grey[800] : Colors.grey[500],
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
