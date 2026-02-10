using System;
using SiatBillingSystem.Application.Interfaces;
using SiatBillingSystem.Application.Common;
using SiatBillingSystem.Domain.Entities;
using SiatBillingSystem.Domain.Constants;

namespace SiatBillingSystem.Application.Services
{
    public class InvoiceService : IInvoiceService
    {
        public string GenerarCuf(ServiceInvoice invoice, string codigoControlCufd)
        {
            // 1. Formatear los datos según especificación del SIN
            string nit = invoice.NitEmisor.PadLeft(13, '0');
            string fechaHora = invoice.FechaEmision.ToString("yyyyMMddHHmmssfff");
            string sucursal = invoice.CodigoSucursal.ToString().PadLeft(4, '0');
            string modalidad = SiatConstants.ModalidadElectronicaEnLinea.ToString();
            string tipoEmision = SiatConstants.TipoEmisionOnline.ToString();
            string tipoFactura = SiatConstants.TipoFacturaConDerechoCreditoFiscal.ToString();
            string tipoDocSector = SiatConstants.SectorCompraVenta.ToString().PadLeft(2, '0');
            string numeroFactura = invoice.NumeroFactura.ToString().PadLeft(10, '0');
            string puntoVenta = (invoice.CodigoPuntoVenta ?? 0).ToString().PadLeft(4, '0');

            // 2. Concatenar la cadena base
            string cadenaCuf = $"{nit}{fechaHora}{sucursal}{modalidad}{tipoEmision}{tipoFactura}{tipoDocSector}{numeroFactura}{puntoVenta}";

            // 3. Aplicar Módulo 11 (1 dígito de control)
            string digitoControl = SiatAlgorithms.ObtenerModulo11(cadenaCuf, 1, 9, false);

            // 4. Concatenar con código de control CUFD y pasar a Base 16
            string resultadoFinal = SiatAlgorithms.Base16(cadenaCuf + digitoControl) + codigoControlCufd;

            return resultadoFinal.ToUpper();
        }
    }
}