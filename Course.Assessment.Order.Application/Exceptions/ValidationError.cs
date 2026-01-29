namespace Course.Assessment.Order.Application.Exceptions;

public sealed record ValidationError(string PropertyName, string ErrorMessage);
