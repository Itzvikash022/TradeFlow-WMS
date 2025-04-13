namespace WMS_Application.DTO
{
    public class LatestUserDTO
    {
        public string ImagePath { get; set; }
        public string FullName { get; set; }
        public string UserType { get; set; }   // "Shop" or "Company"
        public string Status { get; set; }     // "Active", "Pending", "Rejected"
        public DateTime? RegisteredOn { get; set; }
    }
}
