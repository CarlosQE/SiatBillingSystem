using SiatBillingSystem.Domain.Entities;

namespace SiatBillingSystem.Application.Interfaces
{
    public interface IInvoiceService
    {
        /// <summary>
        /// Genera el Código Único de Factura (CUF) requerido por el SIN.
        /// </summary>
        string GenerarCuf(ServiceInvoice invoice, string codigoControlCufd);
    }
}