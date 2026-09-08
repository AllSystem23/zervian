import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../auth/auth_provider.dart';
import '../../../shared/ds/ds.dart';

/// Plan Paso 6 §8.1 — Profile completion for first-time SSO users.
/// Shown when a user arrives from PalmTrack via SSO and has
/// `requiresProfileCompletion: true` in their JWT.
///
/// Pre-fills EmployeeCode from `palmTrackProducerCode` claim,
/// department and position from SSO data.
class PalmTrackCompleteProfilePage extends ConsumerStatefulWidget {
  const PalmTrackCompleteProfilePage({super.key});

  @override
  ConsumerState<PalmTrackCompleteProfilePage> createState() =>
      _PalmTrackCompleteProfilePageState();
}

class _PalmTrackCompleteProfilePageState
    extends ConsumerState<PalmTrackCompleteProfilePage> {
  final _formKey = GlobalKey<FormState>();
  final _employeeCodeController = TextEditingController();
  final _departmentController = TextEditingController();
  final _positionController = TextEditingController();
  final _phoneController = TextEditingController();
  bool _saving = false;
  String? _error;

  // SSO claims extracted from JWT/query params
  String? _palmTrackRole;
  String? _palmTrackOrgId;
  String? _palmTrackProducerCode;

  @override
  void initState() {
    super.initState();
    _loadSsoClaims();
  }

  @override
  void dispose() {
    _employeeCodeController.dispose();
    _departmentController.dispose();
    _positionController.dispose();
    _phoneController.dispose();
    super.dispose();
  }

  void _loadSsoClaims() {
    // In production, these come from the JWT token parsed by auth middleware.
    // For now, they're extracted from query parameters or auth state.
    final extra = GoRouterState.of(context).extra;
    if (extra is Map<String, dynamic>) {
      _palmTrackRole = extra['palmTrackRole'] as String?;
      _palmTrackOrgId = extra['palmTrackOrgId'] as String?;
      _palmTrackProducerCode = extra['palmTrackProducerCode'] as String?;
    }

    // Pre-fill EmployeeCode with producerCode from SSO
    if (_palmTrackProducerCode != null) {
      _employeeCodeController.text = _palmTrackProducerCode!;
    }
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _saving = true;
      _error = null;
    });

    try {
      final dio = ref.read(dioClientProvider);
      await dio.post('auth/complete-profile', data: {
        'employeeCode': _employeeCodeController.text.trim(),
        'department': _departmentController.text.trim(),
        'position': _positionController.text.trim(),
        'phone': _phoneController.text.trim().isEmpty
            ? null
            : _phoneController.text.trim(),
      });

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Perfil completado exitosamente'),
            backgroundColor: Colors.green,
          ),
        );
        context.go('/dashboard');
      }
    } on DioException catch (e) {
      setState(() {
        _error = e.response?.data is Map<String, dynamic>
            ? e.response?.data['message'] as String? ?? 'Error al guardar perfil'
            : 'Error al guardar perfil';
        _saving = false;
      });
    } catch (_) {
      setState(() {
        _error = 'Error inesperado al guardar perfil';
        _saving = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      body: Center(
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 480),
          child: ListView(
            padding: const EdgeInsets.all(24),
            children: [
              const SizedBox(height: 40),

              // ── Header ──
              Icon(
                Icons.agriculture_outlined,
                size: 64,
                color: theme.colorScheme.primary,
              ),
              const SizedBox(height: 16),
              Text(
                'Bienvenido a Zorvian',
                textAlign: TextAlign.center,
                style: theme.textTheme.headlineSmall?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 8),
              Text(
                'Completá tu perfil para empezar a usar Zorvian ERP.',
                textAlign: TextAlign.center,
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: Colors.grey[600],
                ),
              ),

              // ── SSO Identity Badge ──
              if (_palmTrackRole != null || _palmTrackOrgId != null) ...[
                const SizedBox(height: 20),
                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: Colors.blue.shade50,
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(color: Colors.blue.shade100),
                  ),
                  child: Row(
                    children: [
                      Icon(Icons.login, color: Colors.blue[700], size: 20),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              'Identidad desde PalmTrack',
                              style: TextStyle(
                                fontWeight: FontWeight.w600,
                                color: Colors.blue[800],
                                fontSize: 13,
                              ),
                            ),
                            if (_palmTrackRole != null)
                              Text(
                                'Rol: $_palmTrackRole',
                                style: TextStyle(
                                    fontSize: 12, color: Colors.blue[600]),
                              ),
                            if (_palmTrackOrgId != null)
                              Text(
                                'Organización: $_palmTrackOrgId',
                                style: TextStyle(
                                    fontSize: 12, color: Colors.blue[600]),
                              ),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),
              ],

              const SizedBox(height: 32),

              // ── Form ──
              Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Datos del Empleado',
                      style: theme.textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                    const SizedBox(height: 16),

                    ZTextField(
                      controller: _employeeCodeController,
                      label: 'Código de Empleado',
                      hint: 'Ej: P001',
                      validator: (v) => (v == null || v.trim().isEmpty)
                          ? 'El código de empleado es requerido'
                          : null,
                    ),
                    const SizedBox(height: 16),

                    ZTextField(
                      controller: _departmentController,
                      label: 'Departamento',
                      hint: 'Ej: Producción, Administración',
                      validator: (v) => (v == null || v.trim().isEmpty)
                          ? 'El departamento es requerido'
                          : null,
                    ),
                    const SizedBox(height: 16),

                    ZTextField(
                      controller: _positionController,
                      label: 'Cargo / Posición',
                      hint: 'Ej: Productor, Supervisor, Gerente',
                      validator: (v) => (v == null || v.trim().isEmpty)
                          ? 'El cargo es requerido'
                          : null,
                    ),
                    const SizedBox(height: 16),

                    ZTextField(
                      controller: _phoneController,
                      label: 'Teléfono (opcional)',
                      hint: 'Ej: +505 8888-7777',
                      keyboardType: TextInputType.phone,
                    ),
                    const SizedBox(height: 24),

                    // ── Error message ──
                    if (_error != null) ...[
                      Container(
                        width: double.infinity,
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(
                          color: Colors.red.shade50,
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Text(
                          _error!,
                          style: TextStyle(color: Colors.red.shade700),
                        ),
                      ),
                      const SizedBox(height: 16),
                    ],

                    // ── Submit ──
                    SizedBox(
                      width: double.infinity,
                      child: ZButton(
                        text: _saving ? 'Guardando...' : 'Completar Perfil',
                        icon: _saving ? null : Icons.check_circle_outline,
                        onPressed: _saving ? () {} : _submit,
                      ),
                    ),
                  ],
                ),
              ),

              const SizedBox(height: 24),

              // ── Skip for now ──
              Center(
                child: TextButton(
                  onPressed: () => context.go('/dashboard'),
                  child: const Text(
                    'Omitir por ahora',
                    style: TextStyle(color: Colors.grey),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
