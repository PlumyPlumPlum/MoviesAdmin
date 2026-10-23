namespace MoviesAdmin.Models
{
    public class ErrorViewModel // The ErrorViewModel class is a simple model that contains information about an error that occurred during the processing of a request. It is used to pass error information to the Error view for display to the user.
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
