namespace Course.Assessment.Payment.Application.Exceptions;

public sealed record ValidationError(string PropertyName, string ErrorMessage);
