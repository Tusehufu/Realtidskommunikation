using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Realtidskommunikation.Services;
using Realtidskommunikation.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// L�gg till SignalR-tj�nster
builder.Services.AddSignalR();

// L�gg till CORS-tj�nster
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:8080") // Till�t Vue-applikationen p� denna URL
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials(); // Till�t delning av cookies och autentisering �ver CORS
    });
});

// Registrera HttpClient f�r v�dertj�nst
builder.Services.AddHttpClient<WeatherService>();
builder.Services.AddSingleton<WeatherBackgroundService>();


// L�gg till bakgrundstj�nst f�r att h�mta och pusha v�derdata
builder.Services.AddHostedService<WeatherBackgroundService>();

// Om vi vill st�dja SPA i produktion (statiska filer)
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

// Anv�nd CORS
app.UseCors(); 


// SignalR konfiguration
app.MapHub<WeatherHub>("/weatherHub"); // WeatherHub f�r v�deruppdateringar

app.MapHub<ChatHub>("/chat-hub"); // ChatHub f�r chattfunktionen


// �vrig konfiguration f�r produktion, anv�nd statiska filer f�r SPA
if (!app.Environment.IsDevelopment())
{
    app.UseSpaStaticFiles();  // Serva statiska filer fr�n "ClientApp/dist"
}

app.Run();

