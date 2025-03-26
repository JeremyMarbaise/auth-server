using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;
using auth_server.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add this near the top of your Program.cs, before app building

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Use CORS middleware - add this after app building but before other middleware
app.UseCors("AllowAll");

// ... rest of your existing code


app.UseHttpsRedirection();

// In-memory storage for challenges (replace with a database in production)
app.EIDAuthenticateModule();    

// Combined endpoint for challenge generation and authentication


app.Run();

// Helper methods
