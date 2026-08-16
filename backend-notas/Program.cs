using backend_notas.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

//Chamando a api de produtos para pegar dados do produto. 
builder.Services.AddHttpClient("ProdutosApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5019");
});

builder.Services.AddDbContext<NotasDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("NotasDatabase")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
