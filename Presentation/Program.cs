using Npgsql;

 
var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder().SetBasePath(builder.Environment.ContentRootPath).AddJsonFile(
    builder.Environment.IsDevelopment() ? "appsettings.Development.json" : "appsettings.json"
).Build();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddTransient<NpgsqlConnection>(
    _ => new NpgsqlConnection(configuration.GetConnectionString("PostgradeSQL"))
);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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