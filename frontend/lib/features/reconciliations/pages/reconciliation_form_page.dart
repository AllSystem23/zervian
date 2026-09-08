import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../shared/ds/ds.dart';
import '../../../auth/auth_provider.dart';

final class ReconciliationFormPage extends ConsumerStatefulWidget {
  const ReconciliationFormPage({super.key});
  @override
  ConsumerState<ReconciliationFormPage> createState() => _ReconciliationFormPageState();
}

final class _ReconciliationFormPageState extends ConsumerState<ReconciliationFormPage> {
  final _formKey = GlobalKey<FormState>();
  String? _bankAccountId;
  final _dateFromCtrl = TextEditingController();
  final _dateToCtrl = TextEditingController();
  final _notesCtrl = TextEditingController();
  bool _loading = false;
  List<Map<String, String>> _bankAccounts = [];

  @override
  void initState() {
    super.initState();
    _loadBankAccounts();
  }

  Future<void> _loadBankAccounts() async {
    try {
      final dio = ref.read(dioClientProvider);
      final r = await dio.get('/zorvian/v1/bank-accounts');
      final data = r.data as List? ?? [];
      setState(() => _bankAccounts = data.map((b) => {
        'id': b['id']?.toString() ?? '',
        'name': '${b['bankName'] ?? ''} - ${b['accountNumber'] ?? ''}',
      }).toList());
    } catch (_) {}
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate() || _bankAccountId == null) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Complete todos los campos')));
      return;
    }
    setState(() => _loading = true);
    try {
      final dio = ref.read(dioClientProvider);
      await dio.post('/zorvian/v1/reconciliations', data: {
        'bankAccountId': _bankAccountId,
        'dateFrom': _dateFromCtrl.text.trim(),
        'dateTo': _dateToCtrl.text.trim(),
        'notes': _notesCtrl.text.trim().isEmpty ? null : _notesCtrl.text.trim(),
      });
      if (mounted) context.pop(true);
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Error: $e')));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  void dispose() {
    _dateFromCtrl.dispose();
    _dateToCtrl.dispose();
    _notesCtrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SingleChildScrollView(
        padding: EdgeInsets.all(MediaQuery.of(context).size.width < 576 ? 12 : MediaQuery.of(context).size.width < 992 ? 16 : 24),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              DropdownButtonFormField<String>(
                decoration: const InputDecoration(labelText: 'Cuenta Bancaria', prefixIcon: Icon(Icons.account_balance)),
                items: _bankAccounts.map((b) => DropdownMenuItem(value: b['id'], child: Text(b['name']!))).toList(),
                onChanged: (v) => setState(() => _bankAccountId = v),
                validator: (v) => v == null ? 'Requerido' : null,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _dateFromCtrl,
                decoration: const InputDecoration(labelText: 'Fecha Desde (YYYY-MM-DD)', prefixIcon: Icon(Icons.calendar_today)),
                validator: (v) => v == null || v.isEmpty ? 'Requerido' : null,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _dateToCtrl,
                decoration: const InputDecoration(labelText: 'Fecha Hasta (YYYY-MM-DD)', prefixIcon: Icon(Icons.date_range)),
                validator: (v) => v == null || v.isEmpty ? 'Requerido' : null,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _notesCtrl,
                decoration: const InputDecoration(labelText: 'Notas (opcional)', prefixIcon: Icon(Icons.notes)),
                maxLines: 3,
              ),
              const SizedBox(height: 24),
              ZButton(
                text: 'Crear Conciliación',
                onPressed: _save,
                isLoading: _loading,
                icon: Icons.save,
              ),
            ],
          ),
        ),
      ),
    );
  }
}
