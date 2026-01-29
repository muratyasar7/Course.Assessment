namespace Course.Assessment.Order.Application.Clock
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }

}
