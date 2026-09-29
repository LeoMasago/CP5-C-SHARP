using System.Text.Json.Serialization;
using Cp5Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CP5 - API de Clientes e Pedidos",
        Version = "v1",
        Description = "API RESTful para gerenciamento de clientes e pedidos, desenvolvida com .NET 10 e EF Core."
    });
});

builder.Services.AddProblemDetails();

var connectionString = builder.Configuration.GetConnectionString("OracleConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'OracleConnection' não configurada. Defina-a em appsettings.json " +
        "ou via 'dotnet user-secrets set ConnectionStrings:OracleConnection \"...\"'.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseOracle(connectionString));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CP5 API v1");
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
