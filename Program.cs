using kebabBackend.Data;
using kebabBackend.Repositories.Cart;
using kebabBackend.Repositories.CartItems;
using kebabBackend.Repositories.Ingridients;
using kebabBackend.Repositories.meatType;
using kebabBackend.Repositories.MenuItem;
using kebabBackend.Repositories.menuItemCat;
using kebabBackend.Repositories.Souces;
using kebabBackend.Repositories.UsersRep;
using kebabBackend.Services;
using kebabBackend.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Serilog;

namespace kebabBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration;

            builder.Services.AddSignalR();
            builder.Host.UseSerilog();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
            builder.Services.AddSingleton(jwtSettings);
            builder.Services.AddMemoryCache();


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Swagger with JWT support
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Kebab API", Version = "v1" });

                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    BearerFormat = "JWT",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    Description = "Wpisz token w formacie: Bearer {token}",

                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });
            });

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                    policy.WithOrigins("http://localhost:4400")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // DbContext
            builder.Services.AddDbContext<KebabDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("KebabConnectionStrings")));

            // Repositories
            builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
            builder.Services.AddScoped<IMeatTypeRepository, MeatTypeRepository>();
            builder.Services.AddScoped<IExtraIngredientRepository, ExtraIngredientRepository>();
            builder.Services.AddScoped<ISouceRepository, SouceRepository>();
            builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<IMenuItemCategoryRepository, MenuItemCategoryRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<TokenService>();
            builder.Services.AddScoped<EmailService>();
            builder.Services.AddScoped<AzureMapsDistanceService>();


            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Middleware
            //if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                //}

                app.UseHttpsRedirection();
                app.UseCors("AllowFrontend");

                app.UseAuthentication();
                app.UseAuthorization();
                app.MapHub<OrderHub>("/orderHub");

                // Static files
                var imageFolder = Path.Combine(app.Environment.ContentRootPath, "Images");
                Directory.CreateDirectory(imageFolder);
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(imageFolder),
                    RequestPath = "/Images"
                });

                app.MapControllers();
                try
                {
                    Log.Information("Strat....");
                    app.Run();
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Aplikacja zakoñczy³a siê niepowodzeniem");
                }
                finally
                {
                    Log.CloseAndFlush();
                }
            }
        }
    }
}