using SiatBillingSystem.Application.Interfaces;
using SiatBillingSystem.Application.Services;
using SiatBillingSystem.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "SIAT Billing System API",
        Version = "v1",
        Description = "API de Facturacion Electronica SIAT - Sector Servicios - Bolivia"
    });
});

builder.Services.AddScoped<ISignatureService, SignatureService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SIAT Billing v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorClient");
app.UseAuthorization();
app.MapControllers();

app.Run();
