namespace Course.Assessment.Payment.Application.Clock
{
    public interface IDateTimeProvider
    {
        DateTimeOffset UtcNow { get; }
    }

}
