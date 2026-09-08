import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../auth/auth_provider.dart' show dioClientProvider;
import '../../../shared/ds/ds.dart';
import '../models/palmtrack_models.dart';

/// Read-only explorer for PalmTrack lots (plan §3.2).
class PalmTrackLotsPage extends ConsumerStatefulWidget {
  const PalmTrackLotsPage({super.key});

  @override
  ConsumerState<PalmTrackLotsPage> createState() => _PalmTrackLotsPageState();
}

class _PalmTrackLotsPageState extends ConsumerState<PalmTrackLotsPage> {
  final _scrollController = ScrollController();
  final _searchController = TextEditingController();
  List<PalmTrackLot> _lots = [];
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
      final response = await dio.get('palmtrack/lots', params: {'limit': 50});
      final data = response.data as Map<String, dynamic>? ?? {};
      final items = (data['items'] as List? ?? [])
          .whereType<Map<String, dynamic>>()
          .map((j) => PalmTrackLot.fromJson(j))
          .toList();
      final pagination = data['pagination'] as Map<String, dynamic>?;
      setState(() {
        _lots = items;
        _hasMore = pagination?['hasMore'] == true;
        _nextCursor = pagination?['nextCursor'] as String?;
        _loading = false;
      });
    } on DioException catch (e) {
      _handleError(e, 'Error al cargar lotes');
    } catch (e) {
      _handleError(e, 'Error inesperado al cargar lotes');
    }
  }

  Future<void> _loadMore() async {
    if (_loading || !_hasMore || _nextCursor == null) return;
    setState(() => _loading = true);
    try {
      final dio = ref.read(dioClientProvider);
      final response = await dio.get('palmtrack/lots', params: {
        'limit': 50,
        'startAfter': _nextCursor,
      });
      final data = response.data as Map<String, dynamic>? ?? {};
      final items = (data['items'] as List? ?? [])
          .whereType<Map<String, dynamic>>()
          .map((j) => PalmTrackLot.fromJson(j))
          .toList();
      final pagination = data['pagination'] as Map<String, dynamic>?;
      setState(() {
        _lots.addAll(items);
        _hasMore = pagination?['hasMore'] == true;
        _nextCursor = pagination?['nextCursor'] as String?;
        _loading = false;
      });
    } on DioException catch (e) {
      _handleError(e, 'Error al cargar más lotes');
    } catch (e) {
      _handleError(e, 'Error inesperado al cargar más lotes');
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
        title: const Text('PalmTrack — Lotes'),
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
              hintText: 'Buscar lotes...',
              onChanged: (v) {},
            ),
          ),
          Expanded(
            child: _error != null
                ? _buildError()
                : _lots.isEmpty && !_loading
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
        itemCount: _lots.length + (_hasMore ? 1 : 0),
        itemBuilder: (context, index) {
          if (index == _lots.length) {
            return const Center(
              child: Padding(
                padding: EdgeInsets.all(16),
                child: CircularProgressIndicator(),
              ),
            );
          }
          return _buildLotTile(_lots[index]);
        },
      ),
    );
  }

  Widget _buildLotTile(PalmTrackLot lot) {
    return ZCard(
      margin: const EdgeInsets.only(bottom: 8),
      child: ListTile(
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        leading: CircleAvatar(
          backgroundColor: Colors.purple.withValues(alpha: 0.1),
          child: const Icon(Icons.grid_view_outlined, color: Colors.purple, size: 20),
        ),
        title:
            Text(lot.name, style: const TextStyle(fontWeight: FontWeight.w600)),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            if (lot.farmName != null)
              Text('Finca: ${lot.farmName}', style: const TextStyle(fontSize: 12)),
            if (lot.cropVariety != null)
              Text('Variedad: ${lot.cropVariety}',
                  style: const TextStyle(fontSize: 12)),
            if (lot.areaHectares != null)
              Text('${lot.areaHectares} ha',
                  style:
                      const TextStyle(fontSize: 12, fontWeight: FontWeight.w500)),
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
            Icon(Icons.grid_view_outlined, size: 64, color: Colors.grey[300]),
            const SizedBox(height: 16),
            const Text('No hay lotes registrados',
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
