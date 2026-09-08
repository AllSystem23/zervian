import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../auth/auth_provider.dart';
import '../../../core/network/dio_client.dart';

// Provider para la lógica de tesorería
final treasuryServiceProvider = Provider<TreasuryService>((ref) {
  final dioClient = ref.watch(dioClientProvider);
  return TreasuryService(dioClient);
});

/// Provider that loads bank accounts for dropdowns
final treasuryBankAccountsProvider = FutureProvider<List<DropdownMenuItem<String>>>((ref) async {
  final dio = ref.read(dioClientProvider);
  try {
    final res = await dio.get('bank-accounts');
    final list = res.data as List? ?? [];
    return list.map((b) => DropdownMenuItem(
      value: b['id']?.toString(),
      child: Text('${b['bankName'] ?? ''} - ${b['accountNumber'] ?? ''}'),
    )).toList();
  } catch (_) {
    return [];
  }
});

/// Provider that loads cost centers for dropdowns
final treasuryCostCentersProvider = FutureProvider<List<DropdownMenuItem<String>>>((ref) async {
  final dio = ref.read(dioClientProvider);
  try {
    final res = await dio.get('cost-centers');
    final list = res.data as List? ?? [];
    return list.map((c) => DropdownMenuItem(
      value: c['id']?.toString(),
      child: Text(c['name']?.toString() ?? ''),
    )).toList();
  } catch (_) {
    return [];
  }
});

/// Provider that loads invoices for dropdown
final treasuryInvoicesProvider = FutureProvider<List<DropdownMenuItem<String>>>((ref) async {
  final dio = ref.read(dioClientProvider);
  try {
    final res = await dio.get('sales', params: {'page': 1, 'pageSize': 200});
    final list = res.data['items'] as List? ?? res.data as List? ?? [];
    return list.map((inv) => DropdownMenuItem(
      value: inv['id']?.toString(),
      child: Text('${inv['invoiceNumber'] ?? inv['number'] ?? ''}'),
    )).toList();
  } catch (_) {
    return [];
  }
});

class TreasuryService {
  final DioClient _client;
  TreasuryService(this._client);

  Future<dynamic> issueCheck(Map<String, dynamic> checkData) async {
    return await _client.post('treasury/checks/issue', data: checkData);
  }

  Future<void> generateBankDepositEntry(Map<String, dynamic> entryData) async {
    await _client.post('treasury/bank-deposits/accounting-entry', data: entryData);
  }

  Future<void> generateCheckEntry(Map<String, dynamic> entryData) async {
    await _client.post('treasury/checks/accounting-entry', data: entryData);
  }

  Future<void> generateBankTransferEntry(Map<String, dynamic> entryData) async {
    await _client.post('treasury/bank-transfers/accounting-entry', data: entryData);
  }

  Future<void> generateBankCommissionEntry(Map<String, dynamic> entryData) async {
    await _client.post('treasury/bank-commissions/accounting-entry', data: entryData);
  }

  Future<void> generateCollectionEntry(Map<String, dynamic> entryData) async {
    await _client.post('treasury/collections/accounting-entry', data: entryData);
  }
}
