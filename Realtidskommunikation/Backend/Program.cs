using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Realtidskommunikation.Services;
using Realtidskommunikation.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Lägg till SignalR-tjänster
builder.Services.AddSignalR();

// Lägg till CORS-tjänster
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:8080") // Tillåt Vue-applikationen på denna URL
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials(); // Tillåt delning av cookies och autentisering över CORS
    });
});

// Registrera HttpClient för vädertjänst
builder.Services.AddHttpClient<WeatherService>();
builder.Services.AddSingleton<WeatherBackgroundService>();


// Lägg till bakgrundstjänst för att hämta och pusha väderdata
builder.Services.AddHostedService<WeatherBackgroundService>();

// Om vi vill stödja SPA i produktion (statiska filer)
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "ClientApp/dist";  // Vue-applikationens byggda filer
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Använd CORS
app.UseCors(); 


// SignalR konfiguration
app.MapHub<WeatherHub>("/weatherHub"); // WeatherHub för väderuppdateringar

app.MapHub<ChatHub>("/chat-hub"); // ChatHub för chattfunktionen


// Övrig konfiguration för produktion, använd statiska filer för SPA
if (!app.Environment.IsDevelopment())
{
    app.UseSpaStaticFiles();  // Serva statiska filer från "ClientApp/dist"
}

app.Run();

