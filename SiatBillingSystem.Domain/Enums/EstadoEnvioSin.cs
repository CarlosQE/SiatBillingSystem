namespace SiatBillingSystem.Domain.Enums;

/// <summary>
/// Estado del ciclo de vida de una factura respecto al SIN.
/// Refleja el flujo Offline-First: primero se guarda local, luego se sincroniza.
/// </summary>
public enum EstadoEnvioSin
{
    /// <summary>Guardada localmente. Pendiente de envío al SIN (modo offline o en cola).</summary>
    PendienteEnvio = 0,

    /// <summary>Enviada al SIN. Esperando respuesta de validación.</summary>
    EnviandoAlSin = 1,

    /// <summary>Aceptada y autorizada por el SIN. Tiene código de autorización.</summary>
    Aceptada = 2,

    /// <summary>Rechazada por el SIN por error de formato o datos. Ver MotivoRechazo.</summary>
    Rechazada = 3,

    /// <summary>Anulada correctamente en el SIN.</summary>
    Anulada = 4,

    /// <summary>Emitida en modo contingencia (offline). Pendiente regularización.</summary>
    Contingencia = 5,
}
