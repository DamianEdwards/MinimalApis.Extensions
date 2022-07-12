using System.ComponentModel.DataAnnotations;
using MinimalApis.Extensions.Binding;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
#if NET6_0
builder.Services.AddEndpointsMetadataProviderApiExplorer();
#else
builder.Services.AddEndpointsApiExplorer();
#endif
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/bodyas/bytes", (Body<byte[]> body) => $"Received {body.Value.Length} bytes")
    .Accepts<byte[]>("application/octet-stream");
app.MapPost("/bodyas/rom", (Body<ReadOnlyMemory<byte>> body) => $"Received {body.Value.Length} bytes. Request content type was {body.ContentType?.ToString() ?? "[null]"}. Request encoding was {body.Encoding?.ToString() ?? "[null]"}")
    .Accepts<string>("text/plain");
app.MapPost("/bodyas/string", (Body<string> body) => $"Received the following: {body}");
app.MapPost("/bodyas/string/max100", ([MaxLength(100)] Body<string> body) => $"Received the following: {body}");

#if NET7_0_OR_GREATER
var todos = app.MapGroup("/todos");
todos.WithTags("Todos");
todos.MapGet("/{id}", (int id) => TypedResults.Ok(new Todo(id, "Buy the groceries")));
todos.MapPost("/", (Todo todo) => TypedResults.Created($"/todos/{123}", todo with { Id = 123 }))
    .WithParameterValidation();
#endif

app.Run();

record Todo(int Id, [property: Required] string Title);
