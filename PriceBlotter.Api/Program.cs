using PriceBlotter.Api.Hubs;
using PriceBlotter.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSingleton<PriceService>();
builder.Services.AddHostedService(p => p.GetRequiredService<PriceService>());

// Build
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler("/error"); 
app.UseCors();
app.UseRouting();

app.MapHub<PriceHub>("/hubs/prices");

app.Run();
