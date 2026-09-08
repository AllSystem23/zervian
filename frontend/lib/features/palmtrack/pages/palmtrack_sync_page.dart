import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/navigation/nav_config.dart';
import '../../../core/widgets/responsive_layout.dart';
import '../../../shared/ds/ds.dart';
import '../providers/palmtrack_sync_provider.dart';

class PalmTrackSyncPage extends ConsumerStatefulWidget {
  const PalmTrackSyncPage({super.key});

  @override
  ConsumerState<PalmTrackSyncPage> createState() => _PalmTrackSyncPageState();
}

class _PalmTrackSyncPageState extends ConsumerState<PalmTrackSyncPage> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(palmTrackSyncProvider.notifier).load();
    });
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(palmTrackSyncProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('PalmTrack — Sincronización'),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: () => ref.read(palmTrackSyncProvider.notifier).load(),
            tooltip: 'Actualizar',
          ),
        ],
      ),
      body: state.loading
          ? const Center(child: CircularProgressIndicator())
          : state.error != null
              ? _ErrorState(
                  message: state.error!,
                  onRetry: () =>
                      ref.read(palmTrackSyncProvider.notifier).load(),
                )
              : _buildContent(context, state),
    );
  }

  Widget _buildContent(BuildContext context, PalmTrackSyncState state) {
    final stats = state.stats;
    final moduleColor = NavConfig.colorForModule('flota');

    return RefreshIndicator(
      onRefresh: () => ref.read(palmTrackSyncProvider.notifier).load(),
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          // ── Header ──
          Row(
            children: [
              Icon(Icons.sync, color: moduleColor, size: 28),
              const SizedBox(width: 12),
              const Text(
                'Estado de Sincronización',
                style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
              ),
              const Spacer(),
              if (stats.lastSyncAt != null)
                Text(
                  'Última sync: ${_formatDate(stats.lastSyncAt!)}',
                  style: Theme.of(context).textTheme.bodySmall,
                ),
            ],
          ),
          const SizedBox(height: 16),

          // ── Stats Cards ──
          ResponsiveGrid(
            mobileColumns: 2,
            tabletColumns: 4,
            desktopColumns: 4,
            children: [
              ZStatCard(
                title: 'Total Referencias',
                value: '${stats.totalReferences}',
                icon: Icons.link,
                moduleColor: moduleColor,
              ),
              ZStatCard(
                title: 'Sincronizadas',
                value: '${stats.syncedCount}',
                icon: Icons.check_circle_outline,
                variant: ZStatVariant.success,
              ),
              ZStatCard(
                title: 'Pendientes',
                value: '${stats.pendingCount}',
                icon: Icons.pending_outlined,
                variant: ZStatVariant.warning,
              ),
              ZStatCard(
                title: 'Conflictos',
                value: '${stats.conflictCount}',
                icon: Icons.warning_amber_outlined,
                variant: stats.conflictCount > 0
                    ? ZStatVariant.danger
                    : ZStatVariant.neutral,
              ),
            ],
          ),
          const SizedBox(height: 16),

          // ── Sync Rate Progress ──
          ZCard(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text(
                  'Tasa de Sincronización',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
                ),
                const SizedBox(height: 12),
                LinearProgressIndicator(
                  value: stats.syncRate,
                  backgroundColor: Colors.grey.shade200,
                  color: stats.syncRate > 0.9
                      ? Colors.green
                      : stats.syncRate > 0.5
                          ? Colors.orange
                          : Colors.red,
                  minHeight: 8,
                  borderRadius: BorderRadius.circular(4),
                ),
                const SizedBox(height: 8),
                Text(
                  '${(stats.syncRate * 100).toStringAsFixed(1)}% sincronizado '
                  '(${stats.syncedCount}/${stats.totalReferences})',
                  style: Theme.of(context).textTheme.bodySmall,
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),

          // ── Entity Breakdown ──
          ZCard(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text(
                  'Entidades Mapeadas',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
                ),
                const SizedBox(height: 12),
                _buildEntityRow(
                  context,
                  icon: Icons.directions_car_outlined,
                  label: 'Vehículos',
                  count: stats.vehicleCount,
                  color: moduleColor,
                ),
                const Divider(),
                _buildEntityRow(
                  context,
                  icon: Icons.person_outline,
                  label: 'Aliases de Conductores',
                  count: stats.driverAliasCount,
                  color: Colors.blue,
                ),
                const Divider(),
                _buildEntityRow(
                  context,
                  icon: Icons.error_outline,
                  label: 'Errores',
                  count: stats.errorCount,
                  color: stats.errorCount > 0 ? Colors.red : Colors.grey,
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),

          // ── Recent References ──
          if (state.references.isNotEmpty) ...[
            const Text(
              'Referencias Recientes',
              style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
            ),
            const SizedBox(height: 8),
            ...state.references.take(10).map(
                  (ref) => _buildReferenceTile(context, ref),
                ),
          ],

          // ── Webhook Delivery History (plan §3.3) ──
          const SizedBox(height: 24),
          const Text(
            'Historial de Entregas Webhook',
            style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
          ),
          const SizedBox(height: 8),
          if (state.deliveries.isEmpty)
            ZCard(
              child: Center(
                child: Padding(
                  padding: const EdgeInsets.all(24),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Icon(Icons.inbox_outlined,
                          size: 48, color: Colors.grey[300]),
                      const SizedBox(height: 12),
                      Text(
                        'No hay entregas de webhooks registradas',
                        style: TextStyle(color: Colors.grey[500]),
                      ),
                    ],
                  ),
                ),
              ),
            )
          else
            ...state.deliveries.take(15).map(
                  (d) => _buildDeliveryTile(context, d),
                ),
        ],
      ),
    );
  }

  Widget _buildEntityRow(
    BuildContext context, {
    required IconData icon,
    required String label,
    required int count,
    required Color color,
  }) {
    return Row(
      children: [
        Icon(icon, color: color, size: 20),
        const SizedBox(width: 12),
        Expanded(child: Text(label)),
        Text(
          '$count',
          style: TextStyle(
            fontWeight: FontWeight.w600,
            color: color,
          ),
        ),
      ],
    );
  }

  Widget _buildReferenceTile(BuildContext context, ExternalReference ref) {
    final statusColor = _statusColor(ref.status);

    return ZCard(
      margin: const EdgeInsets.only(bottom: 8),
      child: ListTile(
        contentPadding: EdgeInsets.zero,
        leading: CircleAvatar(
          backgroundColor: statusColor.withValues(alpha: 0.1),
          child: Icon(_statusIcon(ref.status), color: statusColor, size: 20),
        ),
        title: Text(
          '${ref.entityType} — ${ref.externalId}',
          style: const TextStyle(fontSize: 14),
        ),
        subtitle: Text(
          ref.lastSyncAt != null
              ? 'Sync: ${_formatDate(ref.lastSyncAt!)}'
              : 'Sin sincronizar',
          style: Theme.of(context).textTheme.bodySmall,
        ),
        trailing: Chip(
          label: Text(ref.status, style: const TextStyle(fontSize: 11)),
          backgroundColor: statusColor.withValues(alpha: 0.1),
          side: BorderSide.none,
          padding: EdgeInsets.zero,
          visualDensity: VisualDensity.compact,
        ),
      ),
    );
  }

  /// Plan §3.3 — Webhook delivery tile: fecha, intento, código HTTP.
  Widget _buildDeliveryTile(BuildContext context, WebhookDelivery delivery) {
    final successColor = delivery.success ? Colors.green : Colors.red;

    return ZCard(
      margin: const EdgeInsets.only(bottom: 8),
      child: ListTile(
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
        leading: CircleAvatar(
          backgroundColor: successColor.withValues(alpha: 0.1),
          child: Icon(
            delivery.success ? Icons.check_circle_outline : Icons.error_outline,
            color: successColor,
            size: 20,
          ),
        ),
        title: Text(
          delivery.event,
          style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w500),
        ),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            if (delivery.receivedAt != null)
              Text(
                _formatDate(delivery.receivedAt!),
                style: Theme.of(context).textTheme.bodySmall,
              ),
            const SizedBox(height: 2),
            Row(
              children: [
                _buildChip(
                  'Intento ${delivery.attempt}/${delivery.maxRetries}',
                  Colors.blue,
                ),
                const SizedBox(width: 8),
                if (delivery.httpStatusCode != null)
                  _buildChip(
                    'HTTP ${delivery.httpStatusCode}',
                    delivery.httpStatusCode! >= 200 &&
                            delivery.httpStatusCode! < 300
                        ? Colors.green
                        : Colors.red,
                  ),
                const SizedBox(width: 8),
                if (delivery.durationMs != null)
                  _buildChip('${delivery.durationMs}ms', Colors.grey),
              ],
            ),
            if (delivery.errorMessage != null)
              Padding(
                padding: const EdgeInsets.only(top: 4),
                child: Text(
                  delivery.errorMessage!,
                  style: TextStyle(fontSize: 12, color: Colors.red[700]),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ),
          ],
        ),
        isThreeLine: true,
      ),
    );
  }

  Widget _buildChip(String label, Color color) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Text(label, style: TextStyle(fontSize: 10, color: color)),
    );
  }

  Color _statusColor(String status) {
    switch (status) {
      case 'synced':
        return Colors.green;
      case 'pending':
        return Colors.orange;
      case 'conflict':
        return Colors.red;
      case 'error':
        return Colors.red.shade700;
      default:
        return Colors.grey;
    }
  }

  IconData _statusIcon(String status) {
    switch (status) {
      case 'synced':
        return Icons.check_circle;
      case 'pending':
        return Icons.pending;
      case 'conflict':
        return Icons.warning;
      case 'error':
        return Icons.error;
      default:
        return Icons.help_outline;
    }
  }

  String _formatDate(String dateStr) {
    try {
      final date = DateTime.parse(dateStr);
      return '${date.day}/${date.month}/${date.year} '
          '${date.hour}:${date.minute.toString().padLeft(2, '0')}';
    } catch (_) {
      return dateStr;
    }
  }
}

class _ErrorState extends StatelessWidget {
  final String message;
  final VoidCallback onRetry;

  const _ErrorState({required this.message, required this.onRetry});

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          const Icon(Icons.cloud_off_outlined, size: 64, color: Colors.grey),
          const SizedBox(height: 16),
          Text(message, style: const TextStyle(color: Colors.grey)),
          const SizedBox(height: 16),
          ZButton(
            text: 'Reintentar',
            icon: Icons.refresh,
            onPressed: onRetry,
          ),
        ],
      ),
    );
  }
}
