using OrderHub.Domain.Data;
using OrderHub.Infrastructure.Repositories;
using OrderHub.Infrastructure.UnitOfWork;
using OrderHub.Application.Services;
using Microsoft.EntityFrameworkCore;
using OrderHub.Application.Contracts;
using OrderHub.Application.Services.Contracts;
using OrderHub.Application.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("OrderHubDBConnection1");
builder.Services.AddDbContext<OrderHubDBContext>(options =>
    options.UseSqlServer(connectionString));

// Register UnitOfWork and Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderLineRepository, OrderLineRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ISchoolRepository, SchoolRepository>();
builder.Services.AddScoped<IStockRepository, StockRepository>();

// Register Application Services
builder.Services.AddScoped<ProductApplicationService>();
builder.Services.AddScoped<OrderApplicationService>();

// Configure and register HttpClient for PaymentService
builder.Services.AddHttpClient<PaymentService>();

// Register Payment Service Configuration
var paymentConfig = new PaymentServiceConfig
{
    PaymentApiUrl = builder.Configuration["Payment:ApiUrl"] ?? "https://api.paymentprovider.com/intents"
};
builder.Services.AddSingleton<IPaymentServiceConfig>(paymentConfig);
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Register Email Service Configuration
var emailConfig = new EmailServiceConfig
{
    SmtpHost = builder.Configuration["Email:SmtpHost"] ?? "localhost",
    SmtpPort = int.Parse(builder.Configuration["Email:SmtpPort"] ?? "25"),
    SmtpUsername = builder.Configuration["Email:SmtpUsername"] ?? "",
    SmtpPassword = builder.Configuration["Email:SmtpPassword"] ?? "",
    EnableSsl = bool.Parse(builder.Configuration["Email:EnableSsl"] ?? "false"),
    UseDefaultCredentials = bool.Parse(builder.Configuration["Email:UseDefaultCredentials"] ?? "true"),
    FromAddress = builder.Configuration["Email:FromAddress"] ?? "orders@brindleford.co.uk",
    IsHtmlBody = false
};
builder.Services.AddSingleton<IEmailServiceConfig>(emailConfig);
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
