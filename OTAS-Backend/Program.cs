using OTAS.Data;
using Microsoft.EntityFrameworkCore;
using OTAS.Repository;
using System.Text.Json.Serialization;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;


var builder = WebApplication.CreateBuilder(args);

/**************** Add services to the container. ***************/
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/* Dependency Injection - This enables Constructor Injection for controllers, services... */

//Repositories
builder.Services.AddScoped<IAvanceCaisseRepository, AvanceCaisseRepository>();
builder.Services.AddScoped<IAvanceVoyageRepository, AvanceVoyageRepository>();
builder.Services.AddScoped<IDelegationRepository, DelegationRepository>();
builder.Services.AddScoped<IDepenseCaisseRepository, DepenseCaisseRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<ILiquidationRepository, LiquidationRepository>();
builder.Services.AddScoped<IOrdreMissionRepository, OrdreMissionRepository>();
builder.Services.AddScoped<IStatusHistoryRepository, StatusHistoryRepository>();
builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IActualRequesterRepository, ActualRequesterRepository>();
builder.Services.AddScoped<ITestingRepository, TestingRepository>();
//Services
builder.Services.AddScoped<IOrdreMissionService, OrdreMissionService>();
builder.Services.AddScoped<IAvanceCaisseService, AvanceCaisseService>();
builder.Services.AddScoped<IDepenseCaisseService, DepenseCaisseService>();


//Add the DataConext
builder.Services.AddDbContext<OtasContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add Authorization & Authentication
//builder.Services.AddAuthentication(x =>
//{
//    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//}).AddJwtBearer(o =>
//{
//    var Key = Encoding.UTF8.GetBytes(Configuration["JWT:Key"]);
//    o.SaveToken = true;
//    o.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = false,
//        ValidateAudience = false,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = Configuration["JWT:Issuer"],
//        ValidAudience = Configuration["JWT:Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(Key),
//        ClockSkew = TimeSpan.Zero
//    };
//    o.Events = new JwtBearerEvents
//    {
//        OnChallenge = async (context) =>
//        {
//            context.HandleResponse();

//            // the details about why the authentication has failed
//            if (context.AuthenticateFailure != null)
//            {
//                context.Response.StatusCode = 401;

//                context.HttpContext.Response.Headers.Add("Token-Expired", "true");
//                await context.HttpContext.Response.WriteAsync("TOKEN-EXPIRED");
//            }
//        }

//    };
//});




/* Inject Automapper */
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


/* Avoid Infinite loop when you bring nested json response (inculde method in EF)*/
builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);


/***************************************************************/

var app = builder.Build();

// To deal with files
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

//app.Use(async (context, next) =>
//{
//    if (!context.User.Identity?.IsAuthenticated ?? false)
//    {
//        context.Response.StatusCode = 401;
//        await context.Response.WriteAsync("Not Authenticated");
//    }
//    else await next();
//});

app.MapControllers();

app.Run();