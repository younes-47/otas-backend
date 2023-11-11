using OTAS.Data;
using Microsoft.EntityFrameworkCore;
using OTAS.Repository;
using System.Text.Json.Serialization;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Services;

var builder = WebApplication.CreateBuilder(args);

/**************** Add services to the container. ***************/

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/* Dependency Injection - This enables Constructor Injection for controllers/services */
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
//Services
builder.Services.AddScoped<IOrdreMissionService, OrdreMissionService>();
builder.Services.AddScoped<IActualRequesterService, ActualRequesterService>();

//Add the DataConext
builder.Services.AddDbContext<OtasContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add Authorization & Authentication
builder.Services.AddAuthentication(Microsoft.AspNetCore.Server.IISIntegration.IISDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

// Add Roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireHRRole", policy => policy.RequireRole("DOMAIN\\AD-Group"));
    options.AddPolicy("RequireMDRole", policy => policy.RequireRole("DOMAIN\\AD-Group"));
    options.AddPolicy("RequireFDRole", policy => policy.RequireRole("DOMAIN\\AD-Group"));
    options.AddPolicy("RequireGDRole", policy => policy.RequireRole("DOMAIN\\AD-Group"));
    options.AddPolicy("RequireTRRole", policy => policy.RequireRole("DOMAIN\\AD-Group"));

});
/* decorate your controller Method with this -> [Authorize(Policy = "RequireDeciderRole")] */


/* Inject Automapper */
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


/* Avoid Infinite loop when you bring nested json response (inculde method in EF)*/
builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);


/***************************************************************/

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
