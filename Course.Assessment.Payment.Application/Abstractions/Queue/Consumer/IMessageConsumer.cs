namespace Course.Assessment.Payment.Application.Abstractions.Queue.Consumer
{
    public interface IMessageConsumer
    {
        Task StartAsync(
            Func<ConsumedMessage, CancellationToken, Task> handler,
            CancellationToken cancellationToken);
    }

}
