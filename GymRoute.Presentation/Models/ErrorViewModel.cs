namespace GymRoute.Presentation.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public int StatusCode { get; set; } = 500;
    public string Title { get; set; } = "Something went wrong";
    public string Message { get; set; } = "An unexpected error occurred. Please try again later.";
    public string? ExceptionType { get; set; }
    public bool ShowDetails { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
