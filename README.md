### Sistema de Facturación Digital SIAT - Bolivia ###
Este proyecto es una solución integral para la facturación electrónica en Bolivia,
diseñada bajo los estándares del SIAT (Sistema de Facturación Virtual/Digital) e Impuestos Nacionales.}
Enfocado en empresas de servicios.

### Stack Tecnológico ###

Backend: .NET 9 (C#) con ASP.NET Core Web API.

Frontend: Blazor WebAssembly + MudBlazor.

Arquitectura: Clean Architecture (Domain, Application, Infrastructure, API, Client).

Seguridad: Firma Digital XML (DSIG), Módulo 11, Base 16.

### Arquitectura del Proyecto ###

La solución está dividida en las siguientes capas para asegurar el desacoplamiento y la facilidad de pruebas:

SiatBillingSystem.Domain: Entidades de negocio y constantes del SIAT.

SiatBillingSystem.Application: Lógica de negocio, interfaces y algoritmos (Generación de CUF).

SiatBillingSystem.Infrastructure: Implementaciones técnicas (Firma digital, persistencia, clientes SOAP).

SiatBillingSystem.API: Punto de entrada para servicios externos y frontend.

SiatBillingSystem.Client: Interfaz de usuario moderna e intuitiva.

### Metodología de Trabajo (XP) ###

Utilizamos Extreme Programming:

Iteraciones cortas: Entregas semanales de valor funcional.

Código Limpio: Refactorización constante.

Simplicidad: Soluciones directas a los requerimientos del SIN.

Colaboración: Uso de GitHub Projects para la gestión de tareas.

### Requisitos para Colaboradores ###

Clonar el repositorio: git clone https://github.com/<TU_USUARIO>/SiatBillingSystem.git

Instalar el SDK de .NET 9.

Ejecutar dotnet restore y dotnet build.
