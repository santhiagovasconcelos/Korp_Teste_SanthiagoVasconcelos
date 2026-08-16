using backend_notas.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

//Chamando a API de produtos para obter dados do produto. 
builder.Services.AddHttpClient("ProdutosApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5019");
});

builder.Services.AddDbContext<NotasDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("NotasDatabase")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.MapControllers();

app.Run();
