namespace Lamazon.Web.Models
{
    public class ErrorViewModel
    {
        public int StatusCode { get; set; }
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public bool IsNotFound => StatusCode == StatusCodes.Status404NotFound;
    }
}
