using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.AddMemoryGrainStorage("PubSubStore");
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddMemoryStreams("MemoryStream");
});
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();

app.MapPost("/stream", async ([FromBody] string text, [FromServices] IClusterClient client) =>
    {
        var provider = client.GetStreamProvider("MemoryStream");
        var stream = provider.GetStream<string>(StreamId.Create("InputStream", 0));
        await stream.OnNextAsync(text);
    })
    .WithName("PostToStream");

app.Run();

