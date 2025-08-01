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
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Microsoft.Azure.SignalR;

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

            builder.Services.AddSignalR().AddAzureSignalR();

            builder.Host.UseSerilog();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
            builder.Services.AddSingleton(jwtSettings);
            builder.Services.AddHostedService<CartProcessingService>();

            builder.Services.AddMemoryCache();


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
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
            // image blob
            builder.Services.AddSingleton(x =>
            {
                var config = x.GetRequiredService<IConfiguration>();
                return new BlobServiceClient(config["AzureBlob:ConnectionString"]);
            });

            // CORS
builder.Services.AddCors(options => 
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "https://zealous-stone-0b5e11903.2.azurestaticapps.net",
                "https://green-flower-00291e603.2.azurestaticapps.net",
                "https://mango-plant-0d70ff103.1.azurestaticapps.net",
                "https://lively-forest-01e6ed703.1.azurestaticapps.net",
                "https://dashboard.wonderfulsand-657cf16a.polandcentral.azurecontainerapps.io",
                "http://localhost:4200",
                "http://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("Authorization", "Location");
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
            app.Use(async (context, next) =>
            {
                context.Request.EnableBuffering(); 
                await next();
            });


            // Middleware
            //if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                //}

                app.UseRouting();
                app.UseCors("AllowFrontend");



                app.UseAuthentication();
                app.UseAuthorization();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapHub<OrderHub>("/orderHub");

                });


                /*
                // Static files local storage
                var imageFolder = Path.Combine(app.Environment.ContentRootPath, "Images");
                Directory.CreateDirectory(imageFolder);
                
                
                
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(imageFolder),
                    RequestPath = "/Images"
                });
                */
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