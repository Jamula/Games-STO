using STO.Data;
using STO.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStoData("Data Source=sto.db");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

app.MapAccountEndpoints();
app.MapCharacterEndpoints();
app.MapBuildEndpoints();
app.MapInventoryEndpoints();
app.MapReputationEndpoints();
app.MapValuableItemEndpoints();

app.Run();
