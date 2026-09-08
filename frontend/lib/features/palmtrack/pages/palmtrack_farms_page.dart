import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../auth/auth_provider.dart' show dioClientProvider;
import '../../../shared/ds/ds.dart';
import '../models/palmtrack_models.dart';

/// Read-only explorer for PalmTrack farms (plan §3.2).
/// Paginated with cursor, filtered by organizationId.
class PalmTrackFarmsPage extends ConsumerStatefulWidget {
  const PalmTrackFarmsPage({super.key});

  @override
  ConsumerState<PalmTrackFarmsPage> createState() => _PalmTrackFarmsPageState();
}

class _PalmTrackFarmsPageState extends ConsumerState<PalmTrackFarmsPage> {
  final _scrollController = ScrollController();
  final _searchController = TextEditingController();
  List<PalmTrackFarm> _farms = [];
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
      final response = await dio.get('/palmtrack/farms', params: {'limit': 50});
      final data = response.data as Map<String, dynamic>? ?? {};
      final items = (data['items'] as List? ?? [])
          .whereType<Map<String, dynamic>>()
          .map((j) => PalmTrackFarm.fromJson(j))
          .toList();
      final pagination = data['pagination'] as Map<String, dynamic>?;
      setState(() {
        _farms = items;
        _hasMore = pagination?['hasMore'] == true;
        _nextCursor = pagination?['nextCursor'] as String?;
        _loading = false;
      });
    } on DioException catch (e) {
      _handleError(e, 'Error al cargar fincas');
    } catch (e) {
      _handleError(e, 'Error inesperado al cargar fincas');
    }
  }

  Future<void> _loadMore() async {
    if (_loading || !_hasMore || _nextCursor == null) return;
    setState(() => _loading = true);
    try {
      final dio = ref.read(dioClientProvider);
      final response = await dio.get('/palmtrack/farms', params: {
        'limit': 50,
        'startAfter': _nextCursor,
      });
      final data = response.data as Map<String, dynamic>? ?? {};
      final items = (data['items'] as List? ?? [])
          .whereType<Map<String, dynamic>>()
          .map((j) => PalmTrackFarm.fromJson(j))
          .toList();
      final pagination = data['pagination'] as Map<String, dynamic>?;
      setState(() {
        _farms.addAll(items);
        _hasMore = pagination?['hasMore'] == true;
        _nextCursor = pagination?['nextCursor'] as String?;
        _loading = false;
      });
    } on DioException catch (e) {
      _handleError(e, 'Error al cargar más fincas');
    } catch (e) {
      _handleError(e, 'Error inesperado al cargar más fincas');
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
        title: const Text('PalmTrack — Fincas'),
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
          // ── Search bar ──
          Padding(
            padding: const EdgeInsets.all(16),
            child: ZSearchField(
              controller: _searchController,
              hintText: 'Buscar fincas...',
              onChanged: (value) {
                // TODO: Filter locally or re-fetch with search param
              },
            ),
          ),

          // ── Content ──
          Expanded(
            child: _error != null
                ? _buildError()
                : _farms.isEmpty && !_loading
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
        itemCount: _farms.length + (_hasMore ? 1 : 0),
        itemBuilder: (context, index) {
          if (index == _farms.length) {
            return const Center(
              child: Padding(
                padding: EdgeInsets.all(16),
                child: CircularProgressIndicator(),
              ),
            );
          }
          return _buildFarmTile(_farms[index]);
        },
      ),
    );
  }

  Widget _buildFarmTile(PalmTrackFarm farm) {
    return ZCard(
      margin: const EdgeInsets.only(bottom: 8),
      child: ListTile(
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        leading: CircleAvatar(
          backgroundColor: Colors.teal.withValues(alpha: 0.1),
          child: const Icon(Icons.landscape_outlined, color: Colors.teal, size: 20),
        ),
        title: Text(farm.name, style: const TextStyle(fontWeight: FontWeight.w600)),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            if (farm.location != null)
              Text(farm.location!, style: const TextStyle(fontSize: 12)),
            const SizedBox(height: 4),
            Row(
              children: [
                _buildChip('${farm.totalLots} lotes', Colors.blue),
                const SizedBox(width: 8),
                _buildChip('${farm.activeLots} activos', Colors.green),
              ],
            ),
          ],
        ),
        isThreeLine: true,
      ),
    );
  }

  Widget _buildChip(String label, Color color) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Text(label, style: TextStyle(fontSize: 11, color: color)),
    );
  }

  Widget _buildEmpty() {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(Icons.landscape_outlined, size: 64, color: Colors.grey[300]),
          const SizedBox(height: 16),
          const Text('No hay fincas registradas', style: TextStyle(color: Colors.grey)),
        ],
      ),
    );
  }

  Widget _buildError() {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          const Icon(Icons.error_outline, size: 64, color: Colors.grey),
          const SizedBox(height: 16),
          Text(_error!, style: const TextStyle(color: Colors.grey)),
          const SizedBox(height: 16),
          ZButton(text: 'Reintentar', icon: Icons.refresh, onPressed: _loadInitial),
        ],
      ),
    );
  }
}
