using SiatBillingSystem.Application.Interfaces;
using SiatBillingSystem.Application.Services;
using SiatBillingSystem.Infrastructure.Interfaces;
using SiatBillingSystem.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════════════════════
// SERVICIOS — Contenedor de Inyección de Dependencias (IoC)
// ═══════════════════════════════════════════════════════════════════════════════

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "SIAT Billing System API",
        Version = "v1",
        Description = "API de Facturación Electrónica SIAT — Sector Servicios — Bolivia"
    });
});

// ── Capa Infrastructure ──
// Scoped: una instancia por request HTTP (correcto para servicios con estado por request)
builder.Services.AddScoped<ISignatureService, SignatureService>();

// ── Capa Application ──
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// ── CORS para Blazor Client (SaaS) ──
// En On-Premise (WPF standalone) este middleware no se usa pero no causa daño
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// ═══════════════════════════════════════════════════════════════════════════════
// PIPELINE DE MIDDLEWARE
// ═══════════════════════════════════════════════════════════════════════════════

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SIAT Billing v1");
        options.RoutePrefix = string.Empty; // Swagger en la raíz: https://localhost:xxxx/
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorClient");
app.UseAuthorization();
app.MapControllers();

app.Run();
