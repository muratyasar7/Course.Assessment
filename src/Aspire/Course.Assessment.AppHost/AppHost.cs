var builder = DistributedApplication.CreateBuilder(args);
var queueSystem = builder.Configuration.GetSection("QueueSystem").Value;
IResourceBuilder<KafkaServerResource>? kafka = null;
IResourceBuilder<RabbitMQServerResource>? rabbitmq = null;
IResourceBuilder<RedisResource>? redis = null;
switch (queueSystem)
{
    case "RabbitMq":
        rabbitmq = builder.AddRabbitMQ("RabbitMq").WithManagementPlugin();
        break;
    case "Kafka":
        kafka = builder.AddKafka("Kafka")
                  .WithKafkaUI()
                  .WithDataVolume();
        break;
    case "RedisStreams":
        redis = builder.AddRedis("Redis", 6379).WithPassword(null).WithDataVolume();
        builder.AddContainer("redisinsight", "redis/redisinsight:latest")
            .WithHttpEndpoint(port: 5540, targetPort: 5540)
            .WithReference(redis);
        break;
    default:
        throw new ArgumentException(nameof(queueSystem));
}
if (redis == null)
{
    redis = builder.AddRedis("Redis", 6379).WithPassword(null).WithDataVolume();
    builder.AddContainer("redisinsight", "redis/redisinsight:latest")
        .WithHttpEndpoint(port: 5540, targetPort: 5540)
        .WithReference(redis);
}
var postgres = builder.AddPostgres("Database").WithPgAdmin().WithDataVolume();



var orderDb = postgres.AddDatabase("OrderDb", databaseName: "order");
var paymentDb = postgres.AddDatabase("PaymentDb", databaseName: "payment");


var orderApi = builder.AddProject<Projects.Course_Assessment_Order_API>("course-assesment-orderapi")
    .WithEnvironment("QueueSystem", queueSystem)
    .WithReference(orderDb)
    .WaitFor(postgres);

var paymentApi = builder.AddProject<Projects.Course_Assessment_Payment_API>("course-assesment-paymentapi")
    .WithEnvironment("QueueSystem", queueSystem)
    .WithReference(paymentDb)
    .WaitFor(postgres);

switch (queueSystem)
{
    case "RabbitMq":
        orderApi.WithReference(rabbitmq!);
        orderApi.WaitFor(rabbitmq!);
        orderApi.WithReference(redis!);
        orderApi.WaitFor(redis!);
        paymentApi.WithReference(rabbitmq!);
        paymentApi.WaitFor(rabbitmq!);
        paymentApi.WithReference(redis!);
        paymentApi.WaitFor(redis!);
        break;
    case "Kafka":
        orderApi.WithReference(kafka!);
        orderApi.WaitFor(kafka!);
        orderApi.WithReference(redis!);
        orderApi.WaitFor(redis!);
        paymentApi.WithReference(kafka!);
        paymentApi.WaitFor(kafka!);
        paymentApi.WithReference(redis!);
        paymentApi.WaitFor(redis!);
        break;
    case "RedisStreams":
        orderApi.WithReference(redis!);
        orderApi.WaitFor(redis!);
        orderApi.WithReference(redis!);
        orderApi.WaitFor(redis!);
        paymentApi.WithReference(redis!);
        paymentApi.WaitFor(redis!);
        paymentApi.WithReference(redis!);
        paymentApi.WaitFor(redis!);
        break;
    default:
        throw new ArgumentException(nameof(queueSystem));


}
builder.Build().Run();
