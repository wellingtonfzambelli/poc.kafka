using Microsoft.Extensions.Options;
using poc.kafka.crosscutting.Domain;
using poc.kafka.crosscutting.Kafka;
using poc.kafka.crosscutting.Settings;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("KafkaSettings"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<KafkaSettings>>().Value);

builder.Services.AddTransient<IKafkaService, KafkaService>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.MapPost("/user", async (User user, IKafkaService kafkaService, CancellationToken cancellationToken) =>
{
    try
    {
        await kafkaService.ProduceAsync(JsonSerializer.Serialize(user), cancellationToken);
        return Results.Ok($"Message produced succefully: {user.Id}");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error by producing the user: {ex.Message}");
    }
})
.WithOpenApi();

app.Run();