using System.Text;
using back.Data;
using back.Data.Repos;
using back.Data.Repos.Interfaces;
using back.Infrastructure;
using back.Models.Settings;
using back.Services.AI;
using back.Services.Auth;
using back.Services.Auth.Email;
using back.Services.Document;
using back.Services.Parser;
using back.Services.Question;
using back.Services.Regulatory;
using back.Services.Report;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.Configure<Email>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailRepo, EmailRepo>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Repositories
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<ILegalReferenceRepository, LegalReferenceRepository>();
builder.Services.AddScoped<IRegulatoryUpdateRepository, RegulatoryUpdateRepository>();
builder.Services.AddScoped<IRegulatoryAlertRepository, RegulatoryAlertRepository>();

// Services
builder.Services.AddScoped<IDocumentParserService, DocumentParserService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IAIService, OpenAIService>();
builder.Services.AddScoped<IContractReportPdfService, ContractReportPdfService>();

// Regulatory Alert Services
builder.Services.AddScoped<ILegalReferenceExtractionService, LegalReferenceExtractionService>();
builder.Services.AddScoped<IRegulatoryMatchingService, RegulatoryMatchingService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddHostedService<RegulatoryMonitorBackgroundService>();

// WebSocket
builder.Services.AddSingleton<WebSocketHandler>();
builder.Services.AddScoped<QuestionWebSocketManager>();

// HTTP Client for OpenAI
builder.Services.AddHttpClient("OpenAI", client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JWToken>();
builder.Services.Configure<JWToken>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings!.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));



// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Legal Guards API",
        Version = "v1",
        Description = "Backend API for the Legal Guards",
    });
});


var app = builder.Build();

// =======================
// Middleware
// =======================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Legal Guagrds API v1");
        options.RoutePrefix = string.Empty; // Swagger at root (/)
    });
}

app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
});

app.Use(async (context, next) =>
{
        if (context.Request.Path == "/ws/questions")
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var wsManager = context.RequestServices.GetRequiredService<QuestionWebSocketManager>();
                await wsManager.HandleWebSocketAsync(webSocket, context, context.RequestAborted);
            }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    else
    {
        await next(context);
    }
});

app.MapControllers();

app.Run();

