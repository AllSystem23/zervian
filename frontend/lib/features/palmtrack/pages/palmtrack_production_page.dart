import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/navigation/nav_config.dart';
import 'package:fl_chart/fl_chart.dart';
import '../../../core/widgets/bi/bi_bar_chart.dart';
import '../../../core/widgets/bi/bi_kpi_card.dart';
import '../../../core/widgets/bi/bi_line_chart.dart';
import '../../../shared/ds/ds.dart';
import '../providers/palmtrack_production_provider.dart';

/// Production dashboard for PalmTrack integration (plan §3.1).
/// Shows KPIs: total bunches, weight, avg weight, bags.
/// Uses existing BI widgets (BiKpiCard, BiBarChart, BiLineChart).
class PalmTrackProductionPage extends ConsumerStatefulWidget {
  const PalmTrackProductionPage({super.key});

  @override
  ConsumerState<PalmTrackProductionPage> createState() =>
      _PalmTrackProductionPageState();
}

class _PalmTrackProductionPageState
    extends ConsumerState<PalmTrackProductionPage> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(palmTrackProductionProvider.notifier).load();
    });
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(palmTrackProductionProvider);
    final moduleColor = NavConfig.colorForModule('flota');

    return Scaffold(
      appBar: AppBar(
        title: const Text('PalmTrack — Producción'),
        actions: [
          IconButton(
            icon: const Icon(Icons.date_range_outlined),
            onPressed: () => _pickDateRange(context),
            tooltip: 'Filtrar por fecha',
          ),
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: () =>
                ref.read(palmTrackProductionProvider.notifier).load(),
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
                      ref.read(palmTrackProductionProvider.notifier).load(),
                )
              : _buildContent(context, state, moduleColor),
    );
  }

  Widget _buildContent(
      BuildContext context, PalmTrackProductionState state, Color moduleColor) {
    return RefreshIndicator(
      onRefresh: () =>
          ref.read(palmTrackProductionProvider.notifier).load(),
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          // ── Date range indicator ──
          if (state.startDate != null || state.endDate != null)
            _buildDateRangeChip(state),

          // ── KPI Row 1: Production totals ──
          Row(
            children: [
              Expanded(
                child: BiKpiCard(
                  label: 'Racimos Totales',
                  value: '${state.summary.totalBunches}',
                  icon: Icons.agriculture_outlined,
                  color: moduleColor,
                ),
              ),
              Expanded(
                child: BiKpiCard(
                  label: 'Peso Total (kg)',
                  value: _formatWeight(state.summary.totalWeight),
                  icon: Icons.scale_outlined,
                  color: Colors.green,
                ),
              ),
              Expanded(
                child: BiKpiCard(
                  label: 'Promedio/Racimo',
                  value: '${state.summary.avgBunchWeight.toStringAsFixed(1)} kg',
                  icon: Icons.analytics_outlined,
                  color: Colors.blue,
                ),
              ),
              Expanded(
                child: BiKpiCard(
                  label: 'Bolsas',
                  value: '${state.summary.totalBags}',
                  icon: Icons.inventory_2_outlined,
                  color: Colors.orange,
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),

          // ── KPI Row 2: Entities ──
          Row(
            children: [
              Expanded(
                child: BiKpiCard(
                  label: 'Fincas Activas',
                  value: '${state.summary.activeFarms}',
                  icon: Icons.landscape_outlined,
                  color: Colors.teal,
                ),
              ),
              Expanded(
                child: BiKpiCard(
                  label: 'Lotes Activos',
                  value: '${state.summary.activeLots}',
                  icon: Icons.grid_view_outlined,
                  color: Colors.purple,
                ),
              ),
              Expanded(
                child: BiKpiCard(
                  label: 'Productores',
                  value: '${state.summary.totalProducers}',
                  icon: Icons.people_outline,
                  color: Colors.brown,
                ),
              ),
              const Expanded(child: SizedBox()), // spacer
            ],
          ),
          const SizedBox(height: 24),

          // ── Production by Farm chart ──
          if (state.summary.byFarm.isNotEmpty) ...[
            ZCard(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Producción por Finca',
                      style: Theme.of(context).textTheme.titleMedium),
                  const SizedBox(height: 16),
                  BiBarChart(
                    items: state.summary.byFarm
                        .map((f) => BarChartItem(
                              f.farmName,
                              f.weight,
                              color: moduleColor,
                            ))
                        .toList(),
                    height: 200,
                  ),
                ],
              ),
            ),
            const SizedBox(height: 16),
          ],

          // ── Daily trend chart ──
          if (state.summary.dailyTrend.isNotEmpty) ...[
            ZCard(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Tendencia Diaria',
                      style: Theme.of(context).textTheme.titleMedium),
                  const SizedBox(height: 16),
                  BiLineChart(
                    series: [
                      LineChartSeries(
                        state.summary.dailyTrend
                            .asMap()
                            .entries
                            .map((e) => FlSpot(
                                  e.key.toDouble(),
                                  e.value.weight,
                                ))
                            .toList(),
                        color: moduleColor,
                        showArea: true,
                      ),
                    ],
                    height: 200,
                  ),
                ],
              ),
            ),
            const SizedBox(height: 16),
          ],

          // ── Farm breakdown table ──
          if (state.summary.byFarm.isNotEmpty)
            ZCard(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Desglose por Finca',
                      style: Theme.of(context).textTheme.titleMedium),
                  const SizedBox(height: 12),
                  _buildFarmTable(context, state.summary.byFarm, moduleColor),
                ],
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildDateRangeChip(PalmTrackProductionState state) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Row(
        children: [
          Icon(Icons.calendar_today, size: 16, color: Colors.grey[600]),
          const SizedBox(width: 8),
          Text(
            '${state.startDate != null ? _formatDate(state.startDate!) : 'Inicio'} — ${state.endDate != null ? _formatDate(state.endDate!) : 'Fin'}',
            style: TextStyle(color: Colors.grey[600], fontSize: 13),
          ),
          const SizedBox(width: 8),
          GestureDetector(
            onTap: () =>
                ref.read(palmTrackProductionProvider.notifier).setDateRange(null, null),
            child: const Icon(Icons.close, size: 16, color: Colors.grey),
          ),
        ],
      ),
    );
  }

  Widget _buildFarmTable(
      BuildContext context, List<ProductionByFarm> farms, Color color) {
    return DataTable(
      columns: const [
        DataColumn(label: Text('Finca')),
        DataColumn(label: Text('Racimos'), numeric: true),
        DataColumn(label: Text('Peso (kg)'), numeric: true),
        DataColumn(label: Text('% del Total'), numeric: true),
      ],
      rows: farms.map((f) {
        final totalWeight = farms.fold(0.0, (sum, f) => sum + f.weight);
        final pct = totalWeight > 0 ? (f.weight / totalWeight * 100) : 0.0;
        return DataRow(cells: [
          DataCell(Text(f.farmName)),
          DataCell(Text('${f.bunches}')),
          DataCell(Text(_formatWeight(f.weight))),
          DataCell(Text('${pct.toStringAsFixed(1)}%')),
        ]);
      }).toList(),
    );
  }

  String _formatWeight(double weight) {
    if (weight >= 1000) return '${(weight / 1000).toStringAsFixed(1)}t';
    return weight.toStringAsFixed(0);
  }

  String _formatDate(DateTime date) {
    return '${date.day}/${date.month}/${date.year}';
  }

  Future<void> _pickDateRange(BuildContext context) async {
    final now = DateTime.now();
    final picked = await showDateRangePicker(
      context: context,
      firstDate: DateTime(now.year - 1),
      lastDate: now,
      initialDateRange: DateTimeRange(
        start: ref.read(palmTrackProductionProvider).startDate ??
            DateTime(now.year, now.month, 1),
        end: ref.read(palmTrackProductionProvider).endDate ?? now,
      ),
    );

    if (picked != null && mounted) {
      ref
          .read(palmTrackProductionProvider.notifier)
          .setDateRange(picked.start, picked.end);
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
          const Icon(Icons.error_outline, size: 64, color: Colors.grey),
          const SizedBox(height: 16),
          Text(message, style: const TextStyle(color: Colors.grey)),
          const SizedBox(height: 16),
          ZButton(text: 'Reintentar', icon: Icons.refresh, onPressed: onRetry),
        ],
      ),
    );
  }
}
