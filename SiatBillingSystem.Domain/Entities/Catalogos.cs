namespace SiatBillingSystem.Domain.Entities;

/// <summary>
/// Tipo de documento de identidad según catálogo SIN.
/// Ejemplos: CI, NIT, Pasaporte, CEX.
/// </summary>
public class TipoDocumentoIdentidad
{
    public int CodigoClasificador { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}

/// <summary>
/// Método de pago según catálogo SIN.
/// Ejemplos: Efectivo, Tarjeta débito, Transferencia bancaria.
/// </summary>
public class MetodoPago
{
    public int CodigoClasificador { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}

/// <summary>
/// Unidad de medida según catálogo SIN.
/// Para sector servicios el código es 58 = "Servicio".
/// </summary>
public class UnidadMedida
{
    public int CodigoClasificador { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}

/// <summary>
/// Leyenda de la Ley 453 (Derechos del Consumidor) según catálogo SIN.
/// Se imprime dinámicamente en la factura según la actividad económica.
/// </summary>
public class LeyendaFactura
{
    public int Id { get; set; }
    public string CodigoActividad { get; set; } = string.Empty;
    public string DescripcionLeyenda { get; set; } = string.Empty;
}

/// <summary>
/// Tipo de moneda según catálogo SIN.
/// Bolivia factura principalmente en Bolivianos (código 1).
/// </summary>
public class TipoMoneda
{
    public int CodigoClasificador { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
