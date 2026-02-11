using SiatBillingSystem.Application.Interfaces;
using SiatBillingSystem.Application.Services;
using SiatBillingSystem.Infrastructure.Interfaces;
using SiatBillingSystem.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// --- SECCIÓN DE SERVICIOS (Dependency Injection) ---
// Aquí registramos nuestras capas para que la API pueda utilizarlas.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. Registro de la Capa de Aplicación (Lógica de Negocio)
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// 2. Registro de la Capa de Infraestructura (Firma Digital y externos)
builder.Services.AddScoped<ISignatureService, SignatureService>();

// 3. Configuración de CORS (Opcional pero recomendado para Blazor Client)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// --- SECCIÓN DE MIDDLEWARE (HTTP Request Pipeline) ---
// Aquí configuramos cómo se comporta la API cuando recibe una llamada.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Aplicar la política de CORS
app.UseCors("AllowBlazorClient");

app.UseAuthorization();

app.MapControllers();

app.Run();