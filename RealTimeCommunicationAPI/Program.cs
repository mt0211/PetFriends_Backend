using DataAccess.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RealTimeCommunicationAPI.Hubs;
using RealTimeCommunicationAPI.Repositories;
using RealTimeCommunicationAPI.Services;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Kestrel Web Server
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(80); // HTTP port
     serverOptions.ListenAnyIP(3000);
    
    // Tối ưu cho WebSocket
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(120);
    serverOptions.Limits.MaxConcurrentUpgradedConnections = 100;
});
// 2. Thêm services vào container
builder.Services.AddControllers();

// 3. Cấu hình Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Version = "v1", 
        Title = "PetFriends API", 
        Description = "API for PetFriends Application" 
    });

    // Cấu hình JWT trong Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
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
            Array.Empty<string>()
        }
    });
});

// 4. Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", builder =>
    {
        builder.WithOrigins("https://petfriends.io.vn", "https://localhost:3000")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

// 5. Cấu hình Database Context
builder.Services.AddDbContext<PetfriendsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking),
    ServiceLifetime.Transient);

// 6. Cấu hình JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"])),
                 NameClaimType = "userid"
        };

        // Xử lý token cho SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && 
                    (path.StartsWithSegments("/chatHub") || path.StartsWithSegments("/videoHub")))
                {
                    context.Token = accessToken;
                }
                else if (context.HttpContext.WebSockets.IsWebSocketRequest && 
                         context.Request.Headers.TryGetValue("Sec-WebSocket-Protocol", out var protocols))
                {
                    context.Token = protocols.FirstOrDefault()?.Split(" ").Last();
                }
                return Task.CompletedTask;
            }
        };
    });
// 7. Cấu hình SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

// 8. Cấu hình WebSockets
builder.Services.AddWebSockets(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(120);
});

// 9. Đăng ký các services
builder.Services.AddScoped<RealTimeCommunicationService>();
builder.Services.AddScoped<IRealTimeCommunicationService, RealTimeCommunicationService>();
builder.Services.AddScoped<IRealTimeCommunicationRepository, RealTimeCommunicationRepository>();

// 10. Build ứng dụng
var app = builder.Build();

// 11. Cấu hình HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
    app.UseSwagger();
    app.UseSwaggerUI(); 
app.UseHttpsRedirection();

// 12. Cấu hình middleware theo đúng thứ tự
app.UseRouting();

// CORS phải đứng sau UseRouting và trước UseAuthentication
app.UseCors("AllowClient");

app.UseAuthentication();
app.UseAuthorization();

// 13. WebSocket middleware
app.UseWebSockets();

// 14. Custom middleware xử lý WebSocket protocol
app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        if (context.Request.Headers.TryGetValue("Sec-WebSocket-Protocol", out var protocols))
        {
            context.Request.Headers["Authorization"] = $"Bearer {protocols.FirstOrDefault()?.Split(" ").Last()}";
        }
    }
    await next();
});

// 15. Map SignalR Hubs
app.MapHub<ChatHub>("/chatHub").RequireCors("AllowClient");
app.MapHub<VideoHub>("/videoHub");


// 16. Map Controllers
app.MapControllers();

// 17. Chạy ứng dụng
app.Run();