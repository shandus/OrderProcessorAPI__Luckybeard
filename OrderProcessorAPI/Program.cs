using Microsoft.EntityFrameworkCore;
using OrderProcessorAPI.Data;
using OrderProcessorAPI.Exceptions;
using OrderProcessorAPI.Repositories;
using OrderProcessorAPI.Services;
using OrderProcessorAPI.Services.Payments;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Register Entity Framework Core
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//For DI we use scoped lifetine 
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

//payment methods
builder.Services.AddScoped<IPaymentStrategy, CreditCardPaymentStrategy>();
builder.Services.AddScoped<IPaymentStrategy, PayPalPaymentStrategy>();
builder.Services.AddScoped<IPaymentStrategy, BankTransferPaymentStrategy>();
builder.Services.AddScoped<PaymentStrategyFactory>();

//services
builder.Services.AddScoped<IEmailSender, MockEmailSender>();
builder.Services.AddScoped<IOrderProcessor, OrderProcessor>();

//hndling exceptions globally 
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseExceptionHandler();


// Enable Swagger UI in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
