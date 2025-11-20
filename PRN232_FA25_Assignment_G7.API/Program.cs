using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PRN232_FA25_Assignment_G7.API.Extensions;
using PRN232_FA25_Assignment_G7.API.Middleware;
using PRN232_FA25_Assignment_G7.API.SignalR;
using PRN232_FA25_Assignment_G7.Repositories;
using PRN232_FA25_Assignment_G7.Repositories.Extensions;
using PRN232_FA25_Assignment_G7.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container
builder.Services.AddControllers()
    .AddODataConfiguration();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();

// Authentication & Authorization
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add role-based authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));
    options.AddPolicy("ModeratorOnly", policy => policy.RequireRole("Moderator"));
    options.AddPolicy("ExaminerOnly", policy => policy.RequireRole("Examiner"));
});

// Application services (Repositories + Services + AutoMapper)
builder.Services.AddApplicationServices(builder.Configuration);

// Register repositories and services (must be done before building the app)
builder.Services.AddRepositories(builder.Configuration);
builder.Services.AddServices();

// SignalR
builder.Services.AddSignalRConfiguration();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" };
        policy.WithOrigins(origins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<SubmissionHub>("/hubs/submission");

app.Run();
