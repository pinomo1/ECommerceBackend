namespace ECommerce1.Models
{
    public enum ReportType
    {
        Product,
        Review,
        Seller,
        Suggestion,
        Other,
    }

    public enum ReportStatus
    {
        Pending,
        Rejected,
        Resolved
    }

    public class Report : AModel
    {
        public string? ReporterEmail { get; set; } = string.Empty;
        public string? ReporterName { get; set; } = string.Empty;
        public bool IsAuthorized { get; set; } = false;
        public ReportType ReportType { get; set; } = ReportType.Other;
        public string? ReportedItemId { get; set; } = string.Empty;
        public ReportStatus ReportStatus { get; set; } = ReportStatus.Pending;
        public string? ReportText { get; set; } = string.Empty;
    }
}
