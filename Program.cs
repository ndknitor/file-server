using FileServer.Context;
using FileServer.Middlewares;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<FileServerContext>(options => options
.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
.UseMySql(builder.Configuration.GetConnectionString("Default"), new MySqlServerVersion(new Version(10, 11, 3))));

builder.Services.AddSingleton<HttpClient>(ins =>
{
    HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.TryAddWithoutValidation("FileAuthToken", builder.Configuration.GetValue<string>("FileAuthToken"));
    return client;
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>());
    });
});
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            Name = "FileAuthToken",
            In = ParameterLocation.Header,
            Scheme = "authtoken",
            Description = "Please insert auth token token into field"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] { }
    }
    });
    });
}
else
{
    builder.Logging.ClearProviders();
    builder.Logging.AddFile(builder.Configuration.GetSection("FileLogging"));
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
    app.UseMiddleware<LoggingMiddleware>();
}
app.UseCors();
app.MapControllers();

app.Run();
//dotnet ef dbcontext scaffold "server=localhost;database=FileServer;user=kn;password=123456" Pomelo.EntityFrameworkCore.MySql  -f --no-pluralize --no-onconfiguring -o Models/Entities/ --context-dir Context