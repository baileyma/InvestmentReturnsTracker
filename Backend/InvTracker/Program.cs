using System.Text;
using InvTracker.Authentication;
using InvTracker.DbContexts;
using InvTracker.Repositories;
using InvTracker.Services;
using InvTracker.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
// sets up kestrel server, configures IIS integration, specifies content root, read app settings

// Add services to the container.

// services are all classes used by the app

builder.Services.AddScoped<IAccountsRepository, AccountsRepository>();
builder.Services.AddScoped<IBalancesRepository, BalancesRepository>();
builder.Services.AddScoped<IPaymentsRepository, PaymentsRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAggregateAccountService, AggregateAccountsService>();
builder.Services.AddScoped<IXIRRCalculator, XIRRCalculator>();
builder.Services.AddMemoryCache();
builder.Services.AddLogging();
//builder.Logging.AddConsole();
//builder.Logging.ClearProviders();

// update 14th March: i have updated the repositories...now have to sort build errors for where changed repo methods are used
// dbs, register endpoint, and login endpoint seem to work well...dad's login in onenote

// balances and payments have now been put in new db
// userid also added to accounts scheme but accounts table not added..
// now need to create some users (see Jasper Kent second video...register dad and luke using swagger) before populating accounts table as its userid column is NOTNULL
// then update the repository classes so they account for the user id...then anything else???

// every number except xirr stuff should be decimal (xirr package does double)
// repository method addaccountasync needs sorting


builder.Services.AddDbContext<InvTrackerContext>(dbContextOptions
    => dbContextOptions.UseMySql(builder.Configuration.GetConnectionString("InvestmentTrackerDB"), new MySqlServerVersion(new Version(8, 0, 40))));

builder.Services.AddIdentity<ClientUser, IdentityRole>()
    .AddEntityFrameworkStores<InvTrackerContext>()
    .AddDefaultTokenProviders();

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter 'Bearer [jwt]'",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    var scheme = new OpenApiSecurityScheme
    {
        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { scheme, Array.Empty<string>() } });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontEnd", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var secret = builder.Configuration["JWT:Secret"] ?? throw new InvalidOperationException("Secret not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontEnd");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
