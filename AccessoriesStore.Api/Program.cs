using AccessoriesStore.Api.Filters;
using AccessoriesStore.Api.Middleware;
using AccessoriesStore.Application.Abstractions.Addresses;
using AccessoriesStore.Application.Abstractions.Auth;
using AccessoriesStore.Application.Abstractions.Carts;
using AccessoriesStore.Application.Abstractions.Categories;
using AccessoriesStore.Application.Abstractions.Common;
using AccessoriesStore.Application.Abstractions.Email;
using AccessoriesStore.Application.Abstractions.Images;
using AccessoriesStore.Application.Abstractions.Inventory;
using AccessoriesStore.Application.Abstractions.Orders;
using AccessoriesStore.Application.Abstractions.Payments;
using AccessoriesStore.Application.Abstractions.ProductReviews;
using AccessoriesStore.Application.Abstractions.Products;
using AccessoriesStore.Application.Abstractions.Wishlist;
using AccessoriesStore.Application.Validators.Categories;
using AccessoriesStore.Infrastructure.Identity;
using AccessoriesStore.Infrastructure.Persistence;
using AccessoriesStore.Infrastructure.Services.Addresses;
using AccessoriesStore.Infrastructure.Services.Carts;
using AccessoriesStore.Infrastructure.Services.Categories;
using AccessoriesStore.Infrastructure.Services.Common;
using AccessoriesStore.Infrastructure.Services.Email;
using AccessoriesStore.Infrastructure.Services.Images;
using AccessoriesStore.Infrastructure.Services.Inventory;
using AccessoriesStore.Infrastructure.Services.Orders;
using AccessoriesStore.Infrastructure.Services.Payments;
using AccessoriesStore.Infrastructure.Services.ProductReviews;
using AccessoriesStore.Infrastructure.Services.Products;
using AccessoriesStore.Infrastructure.Services.Wishlist;
using AccessoriesStore.Infrastructure.Settings;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddSignInManager()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IImageStorageService, CloudinaryImageStorageService>();
builder.Services.AddScoped<IProductImageService,ProductImageService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ValidationFilter>();
builder.Services.AddScoped<ISlugGenerator, SlugGenerator>();
builder.Services.AddHttpClient<IPaymentService, PaymobPaymentService>();
builder.Services.AddScoped<PaymobWebhookService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IProductReviewService,ProductReviewService>();
builder.Services.AddScoped<IWishlistService,WishlistService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();


builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.Configure<PaymobSettings>(
    builder.Configuration.GetSection("PaymobSettings"));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
                {
                    var jwtSettings = builder.Configuration
                        .GetSection("JwtSettings")
                        .Get<JwtSettings>()!;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,

                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };
                });

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddValidatorsFromAssemblyContaining<CreateCategoryRequestValidator>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider
                    .GetRequiredService<RoleManager<IdentityRole>>();

                await IdentitySeeder.SeedRolesAsync(roleManager);

                var userManager = scope.ServiceProvider
                    .GetRequiredService<UserManager<ApplicationUser>>();

                var configuration = scope.ServiceProvider
                    .GetRequiredService<IConfiguration>();

                await IdentitySeeder.SeedAdminAsync(
                    userManager,
                    configuration);
            }

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();

