# NEXUS — Sistema de Facturación Electrónica SIAT Bolivia

> Solución de facturación digital **offline-first** para empresas del sector servicios en Bolivia,
> diseñada bajo los estándares del **SIAT** del Servicio de Impuestos Nacionales (SIN).

---

## Índice

1. [Descripción del Proyecto](#descripción-del-proyecto)
2. [Estado del Desarrollo](#estado-del-desarrollo)
3. [Funcionalidades Actuales](#funcionalidades-actuales)
4. [Stack Tecnológico](#stack-tecnológico)
5. [Arquitectura](#arquitectura)
6. [Flujo de Facturación](#flujo-de-facturación)
7. [Cálculo de IVA](#cálculo-de-iva)
8. [Modos de Operación](#modos-de-operación)
9. [Instalación y Ejecución](#instalación-y-ejecución)
10. [Atajos de Teclado](#atajos-de-teclado)
11. [Estructura del Repositorio](#estructura-del-repositorio)
12. [Pendiente para Producción](#pendiente-para-producción)
13. [Equipo](#equipo)

---

## Descripción del Proyecto

**NEXUS** es una aplicación de escritorio WPF para Windows que permite emitir facturas electrónicas
cumpliendo con los esquemas XML, algoritmos criptográficos y flujos de comunicación que exige el
**SIAT (Sistema de Facturación)** del SIN Bolivia.

El sistema está pensado para clínicas, consultoras, firmas de abogados y cualquier empresa del
sector servicios que necesite:

- Facturar en menos de 5 segundos por emisión.
- Seguir operando si Internet cae (modo contingencia automático).
- Cumplir con la Ley 453 (leyendas dinámicas por actividad económica).
- Tener un historial local de todas las facturas emitidas.

---

## Estado del Desarrollo

| Sprint | Contenido | Estado |
|--------|-----------|--------|
| Sprint 0 | Clean Architecture, algoritmo CUF, firma digital XML (DSIG), DPAPI | ✅ Completo |
| Sprint 1 | SQLite + EF Core, repositorios, seed catálogos SIN, migraciones | ✅ Completo |
| Sprint 2 | WPF dark theme, MVVM, POS Grid, gestión de clientes, configuración empresa | ✅ Completo |
| Sprint 3 | CUF real, PDF+QR profesional, historial facturas, IVA correcto, sidebar colapsable | ✅ Completo |
| Sprint 4 | CUFD real (SIN), envío SOAP, Background Worker sincronización | 🔲 Pendiente |

---

## Funcionalidades Actuales

### 🧾 Facturación POS (F1)
- Ingreso de cliente por NIT o CI con autocompletado desde base de datos local.
- Grilla de ítems con descripción libre, cantidad, precio unitario y descuento.
- Cálculo en tiempo real de IVA (13%) y total.
- Emisión con un solo comando (F5): genera CUF, guarda en SQLite, crea PDF con QR y lo abre automáticamente.
- Modo contingencia automático cuando no hay certificado digital configurado.

### 👤 Clientes Frecuentes (F2)
- CRUD completo de clientes frecuentes.
- Búsqueda en tiempo real por NIT o nombre.
- Registro automático de última factura y contador total por cliente.

### 📋 Historial de Facturas (F3)
- Listado de todas las facturas emitidas con filtros por fecha y NIT.
- Filtros rápidos: Hoy / Este mes.
- Estados de envío al SIN: Pendiente, Enviando, Aceptada, Rechazada, Anulada, Contingencia.
- Doble clic sobre cualquier factura para regenerar y abrir su PDF.
- Barra de resumen: total de facturas, pendientes de envío, monto acumulado.

### ⚙ Configuración de Empresa (F10)
- Datos del emisor: NIT, razón social, actividad económica, leyenda Ley 453.
- Configuración del certificado digital (.p12/.pfx) con contraseña cifrada por DPAPI.
- Parámetros SIAT: modalidad, sucursal, punto de venta.
- CUFD (se llenará automáticamente en Sprint 4 al conectar con el SIN).

### 🖨 PDF Profesional
- Generado con QuestPDF — formato A4 o rollo térmico.
- Incluye: datos del emisor, datos del cliente, detalle de ítems, subtotal, IVA, total.
- Código QR según estándar SIN para verificación en línea.
- Monto en palabras (Ley 1178).
- Se guarda automáticamente en `~/Documents/NEXUS/Facturas/`.

---

## Stack Tecnológico

| Capa | Tecnología | Justificación |
|------|-----------|---------------|
| Runtime | .NET 10 | Rendimiento AOT, tipado estricto para montos fiscales |
| UI | WPF (Windows Presentation Foundation) | Acceso nativo a hardware POS, sin sandbox de browser |
| MVVM | CommunityToolkit.Mvvm | Source generators, ICommand sin boilerplate |
| ORM | Entity Framework Core 10 + SQLite | Migrations Code-First, funciona offline sin servidor |
| PDF | QuestPDF 2026 | Diseño programático, licencia Community gratuita |
| QR | QRCoder 1.7 | Genera PNG sin dependencia de GDI+ |
| Criptografía | System.Security.Cryptography.Xml | Firma DSIG estándar W3C (requerido por SIN) |
| Protección local | DPAPI (Windows) | Cifra la contraseña del certificado a nivel de máquina |
| DI / Host | Microsoft.Extensions.Hosting | Mismo patrón de ASP.NET Core en escritorio |
| Arquitectura | Clean Architecture (5 capas) | Las normativas del SIN cambian — el dominio no |

---

## Arquitectura

```
SiatBillingSystem/
├── Domain          → Entidades, enums, constantes SIN. Cero dependencias externas.
├── Application     → Interfaces, casos de uso, algoritmos SIAT (CUF, IVA, XML).
├── Infrastructure  → EF Core, repositorios SQLite, SignatureService, PdfService, QrService.
├── Desktop         → WPF: Views, ViewModels, Converters, estilos dark theme.
└── Client          → Blazor WASM (desconectado de la solución, reservado para SaaS futuro)
```

**Regla de dependencias (Clean Architecture):**
```
Desktop → Application ← Infrastructure
              ↑
           Domain
```
Ninguna capa interna conoce las capas externas. El dominio fiscal nunca depende de WPF ni de SQLite.

---

## Flujo de Facturación

```
Cajero ingresa datos
        ↓
F5 — Emitir Factura
        ↓
1. Validar campos obligatorios
2. Obtener configuración empresa (SQLite)
3. Obtener siguiente N° factura (atómico con SemaphoreSlim)
4. Detectar modo: ¿hay certificado .p12 válido?
        ├─ SÍ → Tipo Emisión 1 (Online)  → CUF + XML + Firma DSIG
        └─ NO → Tipo Emisión 2 (Offline) → Solo CUF local (contingencia)
5. Guardar factura en SQLite (EstadoEnvio = PendienteEnvio)
6. Actualizar frecuencia del cliente si existe
7. Generar QR con URL de verificación SIN
8. Generar PDF profesional y abrirlo
9. Limpiar grilla → lista para siguiente factura
```

---

## Cálculo de IVA

Bolivia aplica IVA del **13%** sobre el precio base del servicio:

```
Precio ingresado  = BASE (sin IVA)
IVA               = BASE × 0.13
Total cliente     = BASE + IVA

Ejemplo: Servicio Bs. 100
  IVA   = 100 × 0.13 = Bs. 13.00
  Total = 100 + 13   = Bs. 113.00
```

**Lo que recibe el SIN en el XML:**
- `montoTotalSujetoIva` = BASE (100.00) — base imponible
- `montoTotal` = Total (113.00) — lo que pagó el cliente

---

## Modos de Operación

### Modo Contingencia (actual — sin certificado)
El sistema opera 100% offline. Emite facturas válidas localmente, las guarda en SQLite
y genera el PDF para el cliente. Cuando se conecte con el SIN en Sprint 4, el
Background Worker regularizará automáticamente todas las facturas pendientes creando
el "Evento Significativo" requerido por normativa.

### Modo Online (Sprint 4 — con certificado)
Con el archivo `.p12` o `.pfx` de ADSIB/Digicert configurado en F10, el sistema:
1. Obtiene el CUFD real del SIN (válido 24 horas).
2. Firma el XML con la clave privada del certificado.
3. Envía al endpoint `recepcionFactura` vía SOAP.
4. Actualiza el estado de la factura a `Aceptada` o `Rechazada`.

---

## Instalación y Ejecución

### Requisitos
- Windows 10/11 (64 bits)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Git

### Pasos

```powershell
# 1. Clonar el repositorio
git clone https://github.com/CarlosQE/SiatBillingSystem.git
cd SiatBillingSystem

# 2. Restaurar paquetes NuGet
dotnet restore

# 3. Compilar la solución
dotnet build

# 4. Aplicar migraciones (crea siat.db automáticamente al primer arranque)
dotnet ef database update --project SiatBillingSystem.Infrastructure --startup-project SiatBillingSystem.Desktop

# 5. Ejecutar la aplicación
dotnet run --project SiatBillingSystem.Desktop
```

> La base de datos `siat.db` se crea automáticamente en el directorio de ejecución.
> Está excluida del repositorio (`.gitignore`).

---

## Atajos de Teclado

| Atajo | Acción |
|-------|--------|
| `F1` | Ir a Facturación POS |
| `F2` | Ir a Clientes Frecuentes |
| `F3` | Ir a Historial de Facturas |
| `F5` | Emitir factura (desde POS) |
| `F9` | Agregar ítem a la grilla |
| `F10` | Ir a Configuración de Empresa |
| `Ctrl+B` | Expandir / Contraer menú lateral |

---

## Estructura del Repositorio

```
SiatBillingSystem/
│
├── SiatBillingSystem.Domain/
│   ├── Entities/          ServiceInvoice.cs, ClienteFrecuente.cs, ConfiguracionEmpresa.cs
│   ├── Enums/             EstadoEnvioSin.cs
│   └── Constants/         SiatConstants.cs
│
├── SiatBillingSystem.Application/
│   ├── Interfaces/        IInvoiceRepository, IClienteRepository, IConfiguracionRepository,
│   │                      IInvoiceService, ISignatureService, IPdfService, IQrCodeService
│   ├── Services/          InvoiceService.cs, SiatAlgorithms.cs
│   ├── Helpers/           MontoEnPalabrasHelper.cs
│   └── Common/            InvoiceResult.cs
│
├── SiatBillingSystem.Infrastructure/
│   ├── Persistence/       SiatDbContext.cs, SiatDbContextDesignTimeFactory.cs
│   ├── Repositories/      InvoiceRepository, ClienteRepository, ConfiguracionRepository
│   ├── Services/          SignatureService.cs, PdfService.cs, QrCodeService.cs
│   ├── Security/          DpapiProtector.cs
│   └── Migrations/        Sprint3_EstadoEnvioYCamposFactura
│
├── SiatBillingSystem.Desktop/
│   ├── Views/             PosGridView, ClientesView, ConfiguracionView, HistorialView
│   ├── ViewModels/        MainWindowViewModel, PosGridViewModel, ClientesViewModel,
│   │                      ConfiguracionViewModel, HistorialViewModel
│   ├── Converters/        BoolToWidthConverter, StringToVisibilityConverter
│   └── Resources/         Colors.xaml, Typography.xaml, Controls.xaml (dark theme)
│
└── SiatBillingSystem.Tests/   ← Sprint 3: pruebas unitarias CUF e IVA
```

---

## Pendiente para Producción

Para que el sistema esté 100% en producción con el SIN solo resta:

1. **Certificado digital** — Obtener el archivo `.p12` o `.pfx` de ADSIB o Digicert Bolivia
   y configurarlo en la pantalla de Configuración (F10).

2. **CUFD real** — Implementar el cliente SOAP para `ServicioFacturacionCodigos`
   y renovar el CUFD automáticamente cada 24 horas.

3. **Envío SOAP** — Background Worker que tome las facturas con estado `PendienteEnvio`
   y las transmita al endpoint `recepcionFactura` del SIN.

4. **Token delegado** — Singleton que obtiene y renueva el JWT del SIN antes de su expiración.

5. **Evento Significativo** — Registrar inicio/fin de cortes de conexión para
   regularizar las facturas de contingencia.

> Todo el código de dominio, criptografía y XML ya está implementado.
> Lo que resta es **burocracia del SIN**, no lógica de negocio.

---

## Equipo

| Nombre | Rol |
|--------|-----|
| José Carlos Quiroga España | Desarrollo |
| José Andrés Villavicencio Aguayo | Desarrollo |
| Camilo Alandia | Desarrollo |
| Jorge Toledo | Desarrollo |

---

*Documento generado al cierre de Sprint 3 — Marzo 2026*
