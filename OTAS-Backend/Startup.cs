
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.EntityFrameworkCore;
//using System.Text;
//using OTAS.Repository;
//using OTAS.Services;
//using Microsoft.Extensions.Options;
//using OTAS.Interfaces.IRepository;
//using OTAS.Interfaces.IService;
//using OTAS.Data;
//using System.Text.Json.Serialization;
//namespace OTAS
//{
//    public class Startup
//    {
//        public IConfiguration Configuration { get; }
//        public Startup(IConfiguration configuration)
//        {
//            Configuration = configuration;

//            var builder = new ConfigurationBuilder()
//                .SetBasePath(Directory.GetCurrentDirectory())
//                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
//                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
//                .AddUserSecrets<Startup>()
//                .AddEnvironmentVariables();

//            Configuration = builder.Build();
//        }
//        public void ConfigureServices(IServiceCollection services)
//        {
//            services.AddControllers();

//            services.AddCors(options =>
//            {
//                options.AddPolicy("CorsPolicy", builder =>
//                {
//                    builder.AllowAnyOrigin()
//                        .AllowAnyMethod()
//                        .AllowAnyHeader();
//                });
//            });

//            services.AddAuthentication(x =>
//            {
//                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//            }).AddJwtBearer(o =>
//            {
//                var Key = Encoding.UTF8.GetBytes(Configuration["JWT:Key"]);
//                o.SaveToken = true;
//                o.TokenValidationParameters = new TokenValidationParameters
//                {
//                    ValidateIssuer = false,
//                    ValidateAudience = false,
//                    ValidateLifetime = true,
//                    ValidateIssuerSigningKey = true,
//                    ValidIssuer = Configuration["JWT:Issuer"],
//                    ValidAudience = Configuration["JWT:Audience"],
//                    IssuerSigningKey = new SymmetricSecurityKey(Key),
//                    ClockSkew = TimeSpan.Zero
//                };
//                o.Events = new JwtBearerEvents
//                {
//                    OnChallenge = async (context) =>
//                    {
//                        context.HandleResponse();

//                        // the details about why the authentication has failed
//                        if (context.AuthenticateFailure != null)
//                        {
//                            context.Response.StatusCode = 401;

//                            context.HttpContext.Response.Headers.Add("Token-Expired", "true");
//                            await context.HttpContext.Response.WriteAsync("TOKEN-EXPIRED");
//                        }
//                    }

//                };
//            });

//            //Repositories
//            services.AddScoped<IAvanceCaisseRepository, AvanceCaisseRepository>();
//            services.AddScoped<IAvanceVoyageRepository, AvanceVoyageRepository>();
//            services.AddScoped<IDelegationRepository, DelegationRepository>();
//            services.AddScoped<IDepenseCaisseRepository, DepenseCaisseRepository>();
//            services.AddScoped<IExpenseRepository, ExpenseRepository>();
//            services.AddScoped<ILiquidationRepository, LiquidationRepository>();
//            services.AddScoped<IOrdreMissionRepository, OrdreMissionRepository>();
//            services.AddScoped<IStatusHistoryRepository, StatusHistoryRepository>();
//            services.AddScoped<ITripRepository, TripRepository>();
//            services.AddScoped<IUserRepository, UserRepository>();
//            services.AddScoped<IActualRequesterRepository, ActualRequesterRepository>();
//            services.AddScoped<ITestingRepository, TestingRepository>();
//            //Services
//            services.AddScoped<IOrdreMissionService, OrdreMissionService>();
//            services.AddScoped<IAvanceCaisseService, AvanceCaisseService>();
//            services.AddScoped<IDepenseCaisseService, DepenseCaisseService>();


//            /* Add the DataConext */
//            services.AddDbContext<OtasContext>(options =>
//            {
//                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
//            });

//            /* Inject Automapper */
//            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


//            /* Avoid Infinite loop when you bring nested json response (inculde method in EF)*/
//            services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

            
//        }
//        public void Configure(WebApplication app, IWebHostEnvironment env)
//        {
//            if (app.Environment.IsDevelopment())
//            {
//                app.UseSwagger();
//                app.UseSwaggerUI();
//            }
//            app.UseHttpsRedirection();
//            app.UseCors("CorsPolicy");
//            app.UseRouting();
//            app.UseAuthentication();
//            app.UseAuthorization();
//            app.UseEndpoints(endpoints =>
//            {
//                endpoints.MapControllers();
//            });
//        }
//    }
//}
