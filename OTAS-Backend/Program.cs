
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OTAS;
using OTAS.Data;


var builder = WebApplication.CreateBuilder(args);
var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services); // calling ConfigureServices method
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();
startup.Configure(app, builder.Environment); // calling Configure method

/* Add the DataConext */
//builder.Services.AddDbContext<OtasContext>(options =>
//{
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
//});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

// navigate to the 'swagger/v1/swagger.json' page you should see some more information which will point you in useful direction.
app.UseDeveloperExceptionPage();

app.Run();