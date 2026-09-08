import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../auth/auth_provider.dart' show dioClientProvider;
import '../../../shared/ds/ds.dart';
import '../models/palmtrack_models.dart';

/// Read-only explorer for PalmTrack inventory (plan §3.2).
class PalmTrackInventoryPage extends ConsumerStatefulWidget {
  const PalmTrackInventoryPage({super.key});

  @override
  ConsumerState<PalmTrackInventoryPage> createState() =>
      _PalmTrackInventoryPageState();
}

class _PalmTrackInventoryPageState extends ConsumerState<PalmTrackInventoryPage> {
  final _scrollController = ScrollController();
  final _searchController = TextEditingController();
  List<PalmTrackInventory> _items = [];
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
      final response = await dio.get('/palmtrack/inventory', params: {
        'limit': 50,
      });
      final data = response.data as Map<String, dynamic>? ?? {};
      final items = (data['items'] as List? ?? [])
          .whereType<Map<String, dynamic>>()
          .map((j) => PalmTrackInventory.fromJson(j))
          .toList();
      final pagination = data['pagination'] as Map<String, dynamic>?;
      setState(() {
        _items = items;
        _hasMore = pagination?['hasMore'] == true;
        _nextCursor = pagination?['nextCursor'] as String?;
        _loading = false;
      });
    } on DioException catch (e) {
      _handleError(e, 'Error al cargar inventario');
    } catch (e) {
      _handleError(e, 'Error inesperado al cargar inventario');
    }
  }

  Future<void> _loadMore() async {
    if (_loading || !_hasMore || _nextCursor == null) return;
    setState(() => _loading = true);
    try {
      final dio = ref.read(dioClientProvider);
      final response = await dio.get('/palmtrack/inventory', params: {
        'limit': 50,
        'startAfter': _nextCursor,
      });
      final data = response.data as Map<String, dynamic>? ?? {};
      final items = (data['items'] as List? ?? [])
          .whereType<Map<String, dynamic>>()
          .map((j) => PalmTrackInventory.fromJson(j))
          .toList();
      final pagination = data['pagination'] as Map<String, dynamic>?;
      setState(() {
        _items.addAll(items);
        _hasMore = pagination?['hasMore'] == true;
        _nextCursor = pagination?['nextCursor'] as String?;
        _loading = false;
      });
    } on DioException catch (e) {
      _handleError(e, 'Error al cargar más inventario');
    } catch (e) {
      _handleError(e, 'Error inesperado al cargar más inventario');
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
        title: const Text('PalmTrack — Inventario'),
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
              hintText: 'Buscar productos...',
              onChanged: (v) {},
            ),
          ),
          Expanded(
            child: _error != null
                ? _buildError()
                : _items.isEmpty && !_loading
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
        itemCount: _items.length + (_hasMore ? 1 : 0),
        itemBuilder: (context, index) {
          if (index == _items.length) {
            return const Center(
              child: Padding(
                padding: EdgeInsets.all(16),
                child: CircularProgressIndicator(),
              ),
            );
          }
          return _buildInventoryTile(_items[index]);
        },
      ),
    );
  }

  Widget _buildInventoryTile(PalmTrackInventory item) {
    final stockColor =
        item.stock > 0 ? Colors.green : Colors.red;
    return ZCard(
      margin: const EdgeInsets.only(bottom: 8),
      child: ListTile(
        contentPadding:
            const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        leading: CircleAvatar(
          backgroundColor: Colors.orange.withValues(alpha: 0.1),
          child: const Icon(
              Icons.inventory_2_outlined, color: Colors.orange, size: 20),
        ),
        title: Text(
          item.productName ?? item.code ?? 'Sin nombre',
          style: const TextStyle(fontWeight: FontWeight.w600),
        ),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            if (item.code != null)
              Text('Código: ${item.code}',
                  style: const TextStyle(fontSize: 12)),
            Text(
              'Stock: ${item.stock.toStringAsFixed(0)} ${item.unit ?? ""}',
              style: TextStyle(
                  fontSize: 12,
                  color: stockColor,
                  fontWeight: FontWeight.w500),
            ),
          ],
        ),
        trailing: Text(
          "\$${item.unitCost.toStringAsFixed(2)}",
          style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13),
        ),
        isThreeLine: true,
      ),
    );
  }

  Widget _buildEmpty() => Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.inventory_2_outlined, size: 64, color: Colors.grey[300]),
            const SizedBox(height: 16),
            const Text('No hay productos en inventario',
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
