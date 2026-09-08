import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../auth/auth_provider.dart' show dioClientProvider;
import '../../../shared/ds/ds.dart';
import '../models/palmtrack_models.dart';

/// Read-only explorer for PalmTrack sales logs (plan §3.2).
class PalmTrackSalesPage extends ConsumerStatefulWidget {
  const PalmTrackSalesPage({super.key});

  @override
  ConsumerState<PalmTrackSalesPage> createState() =>
      _PalmTrackSalesPageState();
}

class _PalmTrackSalesPageState extends ConsumerState<PalmTrackSalesPage> {
  final _scrollController = ScrollController();
  final _searchController = TextEditingController();
  List<PalmTrackSalesLog> _sales = [];
  bool _loading = false;
  bool _hasMore = true;
  String? _error;
  String? _nextCursor;

  @override
  void initState() {
    super.initState();
    _loadInitial();
    _scrollController.addListener(_onScroll);
  }

  @override
  void dispose() {
    _scrollController.dispose();
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _loadInitial() async {
    setState(() => _loading = true);
    _error = null;
    _nextCursor = null;
    try {
      final dio = ref.read(dioClientProvider);
      final response = await dio.get('/palmtrack/sales-logs', params: {
        'limit': 50,
      });
      final data = response.data as Map<String, dynamic>? ?? {};
      final items = (data['items'] as List? ?? [])
          .whereType<Map<String, dynamic>>()
          .map((j) => PalmTrackSalesLog.fromJson(j))
          .toList();
      final pagination = data['pagination'] as Map<String, dynamic>?;
      setState(() {
        _sales = items;
        _hasMore = pagination?['hasMore'] == true;
        _nextCursor = pagination?['nextCursor'] as String?;
        _loading = false;
      });
    } on DioException catch (e) {
      _handleError(e, 'Error al cargar ventas');
    } catch (e) {
      _handleError(e, 'Error inesperado al cargar ventas');
    }
  }

  Future<void> _loadMore() async {
    if (_loading || !_hasMore || _nextCursor == null) return;
    setState(() => _loading = true);
    try {
      final dio = ref.read(dioClientProvider);
      final response = await dio.get('/palmtrack/sales-logs', params: {
        'limit': 50,
        'startAfter': _nextCursor,
      });
      final data = response.data as Map<String, dynamic>? ?? {};
      final items = (data['items'] as List? ?? [])
          .whereType<Map<String, dynamic>>()
          .map((j) => PalmTrackSalesLog.fromJson(j))
          .toList();
      final pagination = data['pagination'] as Map<String, dynamic>?;
      setState(() {
        _sales.addAll(items);
        _hasMore = pagination?['hasMore'] == true;
        _nextCursor = pagination?['nextCursor'] as String?;
        _loading = false;
      });
    } on DioException catch (e) {
      _handleError(e, 'Error al cargar más ventas');
    } catch (e) {
      _handleError(e, 'Error inesperado al cargar más ventas');
    }
  }

  void _onScroll() {
    if (_scrollController.position.pixels >=
            _scrollController.position.maxScrollExtent - 200 &&
        !_loading &&
        _hasMore) {
      _loadMore();
    }
  }

  void _handleError(dynamic error, String message) {
    setState(() {
      _error = message;
      _loading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('PalmTrack — Ventas'),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: _loadInitial,
            tooltip: 'Actualizar',
          ),
        ],
      ),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(16),
            child: ZSearchField(
              controller: _searchController,
              hintText: 'Buscar ventas...',
              onChanged: (v) {},
            ),
          ),
          Expanded(
            child: _error != null
                ? _buildError()
                : _sales.isEmpty && !_loading
                    ? _buildEmpty()
                    : _buildList(),
          ),
        ],
      ),
    );
  }

  Widget _buildList() {
    return RefreshIndicator(
      onRefresh: _loadInitial,
      child: ListView.builder(
        controller: _scrollController,
        padding: const EdgeInsets.symmetric(horizontal: 16),
        itemCount: _sales.length + (_hasMore ? 1 : 0),
        itemBuilder: (context, index) {
          if (index == _sales.length) {
            return const Center(
              child: Padding(
                padding: EdgeInsets.all(16),
                child: CircularProgressIndicator(),
              ),
            );
          }
          return _buildSaleTile(_sales[index]);
        },
      ),
    );
  }

  Widget _buildSaleTile(PalmTrackSalesLog sale) {
    final statusColor = sale.paymentStatus == 'paid' ? Colors.green : Colors.orange;
    return ZCard(
      margin: const EdgeInsets.only(bottom: 8),
      child: ListTile(
        contentPadding:
            const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        leading: CircleAvatar(
          backgroundColor: Colors.green.withValues(alpha: 0.1),
          child:
              const Icon(Icons.receipt_long_outlined, color: Colors.green, size: 20),
        ),
        title: Text(
          sale.clientName ?? 'Sin cliente',
          style: const TextStyle(fontWeight: FontWeight.w600),
        ),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            if (sale.productName != null)
              Text(sale.productName!, style: const TextStyle(fontSize: 12)),
            Text(
              '${sale.quantity.toStringAsFixed(0)} x '
              '${sale.unitPrice.toStringAsFixed(2)} = '
              '${sale.totalAmount.toStringAsFixed(2)} '
              '${sale.currency ?? ""}',
              style: const TextStyle(fontSize: 12),
            ),
          ],
        ),
        trailing: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          crossAxisAlignment: CrossAxisAlignment.end,
          children: [
            Text(sale.date, style: const TextStyle(fontSize: 11)),
            const SizedBox(height: 4),
            Container(
              padding:
                  const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
              decoration: BoxDecoration(
                color: statusColor.withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Text(
                sale.paymentStatus ?? 'N/A',
                style: TextStyle(fontSize: 10, color: statusColor),
              ),
            ),
          ],
        ),
        isThreeLine: true,
      ),
    );
  }

  Widget _buildEmpty() => Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.receipt_long_outlined, size: 64, color: Colors.grey[300]),
            const SizedBox(height: 16),
            const Text('No hay ventas registradas',
                style: TextStyle(color: Colors.grey)),
          ],
        ),
      );

  Widget _buildError() => Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.error_outline, size: 64, color: Colors.grey),
            const SizedBox(height: 16),
            Text(_error!, style: const TextStyle(color: Colors.grey)),
            const SizedBox(height: 16),
            ZButton(
                text: 'Reintentar',
                icon: Icons.refresh,
                onPressed: _loadInitial),
          ],
        ),
      );
}
