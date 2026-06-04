namespace GymRoute.Presentation.Infrastructure.ExceptionHandling;

public sealed class GymNotFoundException(string message) : Exception(message)
{
}
