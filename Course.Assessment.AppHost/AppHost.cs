var builder = DistributedApplication.CreateBuilder(args);
var postgres = builder.AddPostgres("Database").WithPgAdmin();
var kafka = builder.AddKafka("Kafka")
                   .WithKafkaUI()
                   .WithDataVolume();
var rabbitmq = builder.AddRabbitMQ("RabbitMq");
var redis = builder.AddRedis("Redis");

var orderDb = postgres.AddDatabase("OrderDb", databaseName: "order");
var paymentDb = postgres.AddDatabase("PaymentDb", databaseName: "payment");


builder.AddProject<Projects.Course_Assessment_Order_API>("course-assesment-orderapi")
    .WithReference(kafka)
    .WaitFor(kafka)
    .WithReference(orderDb)
    .WaitFor(postgres)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(redis)
    .WaitFor(redis);
    
builder.AddProject<Projects.Course_Assessment_Payment_API>("course-assesment-paymentapi")
    .WithReference(paymentDb)
    .WaitFor(postgres)
    .WithReference(kafka)
    .WaitFor(kafka)
    .WithReference(orderDb)
    .WaitFor(postgres)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(redis)
    .WaitFor(redis);

builder.Build().Run();
