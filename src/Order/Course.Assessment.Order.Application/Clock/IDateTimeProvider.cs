namespace Course.Assessment.Order.Application.Clock
{
    public interface IDateTimeProvider
    {
        DateTimeOffset UtcNow { get; }
    }

}
