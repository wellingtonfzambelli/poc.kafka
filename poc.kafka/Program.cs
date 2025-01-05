using poc.kafka.crosscutting.Domain;
using poc.kafka.crosscutting.Kafka;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IUserProducer>(p =>
    new UserProducer(
        builder.Configuration["kafkaConfig:TopicName"],
        builder.Configuration["kafkaConfig:BootstrapServer"],
        p.GetService<ILogger<UserProducer>>()
    )
);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/user", async (User user, IUserProducer producer, CancellationToken cancellationToken) =>
{
    try
    {
        await producer.ProduceAsync(user, cancellationToken);
        return Results.Ok($"Message produced succefully: {user.Id}");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error by producing the user: {ex.Message}");
    }
})
.WithOpenApi();

app.Run();