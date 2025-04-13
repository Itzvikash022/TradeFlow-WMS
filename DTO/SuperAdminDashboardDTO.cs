namespace WMS_Application.DTO
{
    public class SuperAdminDashboardDto
    {
        // Section 2: Key Stats
        public int ActiveAdminsCount { get; set; }
        public int TotalCompaniesCount { get; set; }
        public int SuccessfulOrdersThisMonth { get; set; }
        public int TotalRegisteredUsers { get; set; }

        // Section 4: Donut Chart - User Status Split
        public int ActiveUserCount { get; set; }
        public int PendingUserCount { get; set; }
        public int RejectedUserCount { get; set; }

        // Last Section: Latest Registered Users
        public List<LatestUserDTO> LatestRegisteredUsers { get; set; }

    }

}
