namespace SiatBillingSystem.Domain.Constants
{
    /// <summary>
    /// Official constants based on SIAT catalogs from the National Tax Service (SIN).
    /// </summary>
    public static class SiatConstants
    {
        // Emission Types
        public const int TipoEmisionOnline = 1;
        public const int TipoEmisionOffline = 2;

        // Modality Types
        public const int ModalidadElectronicaEnLinea = 1;
        public const int ModalidadComputarizadaEnLinea = 2;

        // Invoice Types
        public const int TipoFacturaConDerechoCreditoFiscal = 1;
        public const int TipoFacturaSinDerechoCreditoFiscal = 2;

        // Sector Document Types (Service sector is usually 1)
        public const int SectorCompraVenta = 1;
        public const int SectorServiciosTuristicos = 12;

        // Identity Document Types
        public const int TipoDocumentoNIT = 5;
        public const int TipoDocumentoCedulaIdentidad = 1;

        // Currency
        public const int MonedaBoliviano = 1;
    }
}