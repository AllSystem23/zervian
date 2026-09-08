import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../shared/ds/ds.dart';
import '../providers/palmtrack_conflicts_provider.dart';

class PalmTrackConflictsPage extends ConsumerStatefulWidget {
  const PalmTrackConflictsPage({super.key});

  @override
  ConsumerState<PalmTrackConflictsPage> createState() =>
      _PalmTrackConflictsPageState();
}

class _PalmTrackConflictsPageState
    extends ConsumerState<PalmTrackConflictsPage> {
  final _searchController = TextEditingController();
  String _searchQuery = '';

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(palmTrackConflictsProvider.notifier).load();
    });
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  /// Plan §3.4 — Search/filter over Zorvian entities for linking.
  List<SyncConflict> _filteredConflicts(List<SyncConflict> conflicts) {
    if (_searchQuery.isEmpty) return conflicts;
    final query = _searchQuery.toLowerCase();
    return conflicts.where((c) {
      return (c.entityType.toLowerCase().contains(query)) ||
          (c.entityId.toLowerCase().contains(query)) ||
          (c.externalId.toLowerCase().contains(query)) ||
          (c.entityTypeLabel?.toLowerCase().contains(query) ?? false) ||
          (c.entityIdLabel?.toLowerCase().contains(query) ?? false);
    }).toList();
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(palmTrackConflictsProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('PalmTrack — Conflictos de Sincronización'),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: () =>
                ref.read(palmTrackConflictsProvider.notifier).load(),
            tooltip: 'Actualizar',
          ),
        ],
      ),
      body: state.loading
          ? const Center(child: CircularProgressIndicator())
          : state.error != null
              ? _buildError(context, state)
              : _buildContent(context, state),
    );
  }

  Widget _buildContent(
      BuildContext context, PalmTrackConflictsState state) {
    final filtered = _filteredConflicts(state.conflicts);

    if (state.conflicts.isEmpty) {
      return Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.check_circle_outline,
                size: 64, color: Colors.green.shade300),
            const SizedBox(height: 16),
            const Text(
              'Sin conflictos de sincronización',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.w500),
            ),
            const SizedBox(height: 8),
            Text(
              'Todas las entidades PalmTrack están sincronizadas correctamente.',
              style: Theme.of(context).textTheme.bodySmall,
            ),
          ],
        ),
      );
    }

    return Column(
      children: [
        // ── Summary Banner ──
        Container(
          width: double.infinity,
          padding: const EdgeInsets.all(16),
          color: Colors.orange.shade50,
          child: Row(
            children: [
              Icon(Icons.warning_amber_outlined,
                  color: Colors.orange.shade700),
              const SizedBox(width: 12),
              Expanded(
                child: Text(
                  '${state.conflicts.length} conflicto(s) requieren resolución. '
                  'Los datos de PalmTrack y ZorvianERP difieren para estas entidades.',
                  style: TextStyle(color: Colors.orange.shade900),
                ),
              ),
            ],
          ),
        ),

        // ── Search bar (plan §3.4) ──
        Padding(
          padding: const EdgeInsets.fromLTRB(16, 12, 16, 0),
          child: ZSearchField(
            controller: _searchController,
            hintText: 'Buscar por entidad, ID o PalmTrack ID...',
            onChanged: (value) {
              setState(() => _searchQuery = value);
            },
          ),
        ),

        // ── Conflicts List ──
        Expanded(
          child: filtered.isEmpty
              ? Center(
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Icon(Icons.search_off,
                          size: 48, color: Colors.grey[300]),
                      const SizedBox(height: 12),
                      Text(
                        'No se encontraron conflictos para "$_searchQuery"',
                        style: TextStyle(color: Colors.grey[500]),
                      ),
                    ],
                  ),
                )
              : RefreshIndicator(
                  onRefresh: () =>
                      ref.read(palmTrackConflictsProvider.notifier).load(),
                  child: ListView.builder(
                    padding: const EdgeInsets.all(16),
                    itemCount: filtered.length,
                    itemBuilder: (context, index) =>
                        _buildConflictCard(
                            context, filtered[index], state),
                  ),
                ),
        ),
      ],
    );
  }

  Widget _buildConflictCard(
    BuildContext context,
    SyncConflict conflict,
    PalmTrackConflictsState state,
  ) {
    final isResolving = state.resolvingId == conflict.id;

    return ZCard(
      margin: const EdgeInsets.only(bottom: 12),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // ── Header ──
          Row(
            children: [
              CircleAvatar(
                backgroundColor: Colors.red.withValues(alpha: 0.1),
                child: const Icon(Icons.sync_problem,
                    color: Colors.red, size: 20),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      conflict.entityTypeLabel ?? conflict.entityType,
                      style: const TextStyle(
                        fontWeight: FontWeight.w600,
                        fontSize: 16,
                      ),
                    ),
                    Text(
                      'ID Zorvian: ${conflict.entityIdLabel ?? conflict.entityId}',
                      style: Theme.of(context).textTheme.bodySmall,
                    ),
                  ],
                ),
              ),
              if (conflict.consecutiveFailures > 0)
                Chip(
                  label: Text(
                    '${conflict.consecutiveFailures} fallos',
                    style: const TextStyle(fontSize: 11),
                  ),
                  backgroundColor: Colors.red.shade50,
                  side: BorderSide.none,
                ),
            ],
          ),
          const SizedBox(height: 12),

          // ── Error Details ──
          if (conflict.lastError != null)
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(8),
              decoration: BoxDecoration(
                color: Colors.red.shade50,
                borderRadius: BorderRadius.circular(4),
              ),
              child: Text(
                conflict.lastError!,
                style: TextStyle(
                  fontSize: 12,
                  color: Colors.red.shade900,
                  fontFamily: 'monospace',
                ),
              ),
            ),
          const SizedBox(height: 12),

          // ── External ID ──
          Text(
            'PalmTrack ID: ${conflict.externalId}',
            style: Theme.of(context).textTheme.bodySmall?.copyWith(
                  fontFamily: 'monospace',
                ),
          ),
          if (conflict.lastSyncAt != null) ...[
            const SizedBox(height: 4),
            Text(
              'Último sync: ${_formatDate(conflict.lastSyncAt!)}',
              style: Theme.of(context).textTheme.bodySmall,
            ),
          ],
          const SizedBox(height: 12),

          // ── Resolution Actions ──
          Row(
            children: [
              Expanded(
                child: ZButton(
                  text: 'Mantener Local',
                  icon: Icons.storage_outlined,
                  type: ZButtonType.secondary,
                  onPressed: isResolving
                      ? () {}
                      : () => _resolve(conflict.id, false),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: ZButton(
                  text: 'Aceptar PalmTrack',
                  icon: Icons.cloud_download_outlined,
                  onPressed: isResolving
                      ? () {}
                      : () => _resolve(conflict.id, true),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Future<void> _resolve(String referenceId, bool acceptExternal) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(acceptExternal
            ? 'Aceptar datos de PalmTrack'
            : 'Mantener datos locales'),
        content: Text(acceptExternal
            ? 'Se sobrescribirán los datos locales con los datos de PalmTrack. ¿Continuar?'
            : 'Se descartarán los cambios de PalmTrack y se mantendrán los datos locales. ¿Continuar?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(false),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () => Navigator.of(ctx).pop(true),
            child: const Text('Confirmar'),
          ),
        ],
      ),
    );

    if (confirmed == true && mounted) {
      final success = await ref
          .read(palmTrackConflictsProvider.notifier)
          .resolveConflict(
            referenceId: referenceId,
            acceptExternal: acceptExternal,
          );

      if (mounted && success) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Conflicto resuelto exitosamente'),
            backgroundColor: Colors.green,
          ),
        );
      }
    }
  }

  Widget _buildError(BuildContext context, PalmTrackConflictsState state) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          const Icon(Icons.cloud_off_outlined, size: 64, color: Colors.grey),
          const SizedBox(height: 16),
          Text(state.error ?? 'Error desconocido'),
          const SizedBox(height: 16),
          ZButton(
            text: 'Reintentar',
            icon: Icons.refresh,
            onPressed: () =>
                ref.read(palmTrackConflictsProvider.notifier).load(),
          ),
        ],
      ),
    );
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
