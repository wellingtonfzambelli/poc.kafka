using poc.kafka.crosscutting.Domain;
using poc.kafka.crosscutting.Kafka;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IUserKafka>(p =>
    new UserKafka(
        builder.Configuration["kafkaConfig:TopicName"],
        builder.Configuration["kafkaConfig:BootstrapServer"],
        builder.Configuration["kafkaConfig:GroupId"],
        p.GetService<ILogger<UserKafka>>()
    )
);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/user", async (User user, IUserKafka userKafka, CancellationToken cancellationToken) =>
{
    try
    {
        await userKafka.ProduceAsync(user, cancellationToken);
        return Results.Ok($"Message produced succefully: {user.Id}");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error by producing the user: {ex.Message}");
    }
})
.WithOpenApi();

app.Run();