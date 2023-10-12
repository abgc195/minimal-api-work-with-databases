using Microsoft.EntityFrameworkCore;
using PizzaStore.Data;
using Microsoft.OpenApi.Models;
using PizzaStore.Models;

var builder = WebApplication.CreateBuilder(args);
var relativePath = "pizzas.db";
var parentdir = Path.GetDirectoryName(Environment.CurrentDirectory);
string absolutePath = Path.Combine(parentdir, "PizzaStore", relativePath);
Console.WriteLine("{0} {1}", parentdir, absolutePath);
var connectionString = builder.Configuration.GetConnectionString("pizzas") ?? string.Format("Data Source={0}", absolutePath);
builder.Services.AddDbContext<PizzaDb>(options => options.UseSqlite(connectionString));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "Pizzas API", Description = "Pizza pizza", Version = "v1" });
});
// 1) define a unique string
string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// 2) define allowed domains, in this case "http://example.com" and "*" = all
//    domains, for testing purposes only.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
      builder =>
      {
          builder.WithOrigins(
            "http://example.com", "*");
      });
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
  c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pizzas API V1");
});



// 3) use the capability
app.UseCors(MyAllowSpecificOrigins);

app.MapGet("/", (PizzaDb db) => new { ConnectionString = db.Database.GetConnectionString(), db.Database.ProviderName });

app.MapGet("/pizzas", async(PizzaDb db) => await db.Pizzas.ToListAsync());

app.MapPost("/pizzas", async(PizzaDb db, Pizza pizza) => {
    await db.Pizzas.AddAsync(pizza);
    await db.SaveChangesAsync();
    return Results.Created($"/pizzas/{pizza.Id}", pizza);
});

app.MapPut("/pizzas/{id}", async (PizzaDb db, Pizza updatePizza, int id) =>
{
  var pizzaItem = await db.Pizzas.FindAsync(id);
  if (pizzaItem is null) return Results.NotFound();
  pizzaItem.Name = updatePizza.Name;
  pizzaItem.Description = updatePizza.Description;
  await db.SaveChangesAsync();
  return Results.NoContent();
});

app.MapDelete("/pizzas/{id}", async (PizzaDb db, int id) =>
{
  var todo = await db.Pizzas.FindAsync(id);
  if (todo is null)
  {
    return Results.NotFound();
  }
  db.Pizzas.Remove(todo);
  await db.SaveChangesAsync();
  return Results.Ok();
});
app.Run();