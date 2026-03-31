using InvTracker.DbContexts;
using InvTracker.Repositories;
using InvTracker.Services;
using InvTracker.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

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

builder.Services.AddDbContext<InvTrackerContext>(dbContextOptions
    => dbContextOptions.UseMySql(builder.Configuration.GetConnectionString("InvestmentTrackerDB"), new MySqlServerVersion(new Version(8, 0, 40))));

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontEnd", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
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

app.UseAuthorization();

app.MapControllers();

app.Run();
