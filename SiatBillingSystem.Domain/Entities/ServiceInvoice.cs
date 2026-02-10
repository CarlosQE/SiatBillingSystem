using System;
using System.Collections.Generic;
using SiatBillingSystem.Domain.Constants;

namespace SiatBillingSystem.Domain.Entities
{
    /// <summary>
    /// Structure for a Service Sector Invoice according to SIAT standards.
    /// </summary>
    public class ServiceInvoice
    {
        // Header data
        public string NitEmisor { get; set; } = string.Empty;
        public long NumeroFactura { get; set; }
        public string Cuf { get; set; } = string.Empty;
        public string Cufd { get; set; } = string.Empty; // Control Code for the day
        public int CodigoSucursal { get; set; } = 0;
        public int? CodigoPuntoVenta { get; set; }
        public DateTime FechaEmision { get; set; }

        // Client data
        public string NombreRazonSocial { get; set; } = string.Empty;
        public int CodigoTipoDocumentoIdentidad { get; set; } = SiatConstants.TipoDocumentoCedulaIdentidad;
        public string NumeroDocumento { get; set; } = string.Empty;
        public string? Complemento { get; set; }

        // Amounts
        public decimal MontoTotal { get; set; }
        public decimal MontoTotalSujetoIva { get; set; }
        public int CodigoMetodoPago { get; set; }
        public string Leyenda { get; set; } = string.Empty;

        // Details list
        public List<ServiceInvoiceDetail> Details { get; set; } = new();

        public ServiceInvoice()
        {
            FechaEmision = DateTime.Now;
        }
    }

    public class ServiceInvoiceDetail
    {
        public string ActividadEconomica { get; set; } = string.Empty;
        public int CodigoProductoSin { get; set; }
        public string CodigoProducto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public int UnidadMedida { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubTotal { get; set; }
    }
}