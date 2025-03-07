namespace WMS_Application.DTO
{
    public class AdminReportsDTO
    {
        public int AdminId { get; set; }
        public DateTime RegisteredOn { get; set; }
        public bool ShopDetails { get; set; }
        public bool Documents { get; set; }
        public bool IsActive { get; set; }
        public int Employees { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ProfilePic { get; set; }
        public string Status { get; set; }
    }
}
