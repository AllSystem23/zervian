import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../auth/auth_provider.dart' show dioClientProvider;
import '../../../shared/ds/ds.dart';
import '../models/palmtrack_models.dart';

/// Read-only explorer for PalmTrack producers (plan §3.2).
class PalmTrackProducersPage extends ConsumerStatefulWidget {
  const PalmTrackProducersPage({super.key});

  @override
  ConsumerState<PalmTrackProducersPage> createState() =>
      _PalmTrackProducersPageState();
}

class _PalmTrackProducersPageState extends ConsumerState<PalmTrackProducersPage> {
  final _scrollController = ScrollController();
  final _searchController = TextEditingController();
  List<PalmTrackProducer> _producers = [];
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
      final response = await dio.get('palmtrack/producers', params: {
        'limit': 50,
      });
      final data = response.data as Map<String, dynamic>? ?? {};
      final items = (data['items'] as List? ?? [])
          .whereType<Map<String, dynamic>>()
          .map((j) => PalmTrackProducer.fromJson(j))
          .toList();
      final pagination = data['pagination'] as Map<String, dynamic>?;
      setState(() {
        _producers = items;
        _hasMore = pagination?['hasMore'] == true;
        _nextCursor = pagination?['nextCursor'] as String?;
        _loading = false;
      });
    } on DioException catch (e) {
      _handleError(e, 'Error al cargar productores');
    } catch (e) {
      _handleError(e, 'Error inesperado al cargar productores');
    }
  }

  Future<void> _loadMore() async {
    if (_loading || !_hasMore || _nextCursor == null) return;
    setState(() => _loading = true);
    try {
      final dio = ref.read(dioClientProvider);
      final response = await dio.get('palmtrack/producers', params: {
        'limit': 50,
        'startAfter': _nextCursor,
      });
      final data = response.data as Map<String, dynamic>? ?? {};
      final items = (data['items'] as List? ?? [])
          .whereType<Map<String, dynamic>>()
          .map((j) => PalmTrackProducer.fromJson(j))
          .toList();
      final pagination = data['pagination'] as Map<String, dynamic>?;
      setState(() {
        _producers.addAll(items);
        _hasMore = pagination?['hasMore'] == true;
        _nextCursor = pagination?['nextCursor'] as String?;
        _loading = false;
      });
    } on DioException catch (e) {
      _handleError(e, 'Error al cargar más productores');
    } catch (e) {
      _handleError(e, 'Error inesperado al cargar más productores');
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
        title: const Text('PalmTrack — Productores'),
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
              hintText: 'Buscar productores...',
              onChanged: (v) {},
            ),
          ),
          Expanded(
            child: _error != null
                ? _buildError()
                : _producers.isEmpty && !_loading
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
        itemCount: _producers.length + (_hasMore ? 1 : 0),
        itemBuilder: (context, index) {
          if (index == _producers.length) {
            return const Center(
              child: Padding(
                padding: EdgeInsets.all(16),
                child: CircularProgressIndicator(),
              ),
            );
          }
          return _buildProducerTile(_producers[index]);
        },
      ),
    );
  }

  Widget _buildProducerTile(PalmTrackProducer p) {
    return ZCard(
      margin: const EdgeInsets.only(bottom: 8),
      child: ListTile(
        contentPadding:
            const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        leading: CircleAvatar(
          backgroundColor: Colors.brown.withValues(alpha: 0.1),
          child:
              const Icon(Icons.person_outline, color: Colors.brown, size: 20),
        ),
        title:
            Text(p.name, style: const TextStyle(fontWeight: FontWeight.w600)),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            if (p.code != null) Text('Código: ${p.code}',
                style: const TextStyle(fontSize: 12)),
            if (p.phone != null) Text('Tel: ${p.phone}',
                style: const TextStyle(fontSize: 12)),
            Text('${p.totalFarms} finca(s)',
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
            Icon(Icons.person_outline, size: 64, color: Colors.grey[300]),
            const SizedBox(height: 16),
            const Text('No hay productores registrados',
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
