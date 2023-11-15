var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/hellowold/{id:int}", (int id) => 
{
    return Results.Ok("Id: " + id);
});

app.UseHttpsRedirection();

app.Run();
