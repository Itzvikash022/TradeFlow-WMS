namespace WMS_Application.DTO
{
    public class UpdateOrderStatusRequestDto
    {
        public int OrderId { get; set; }
        public string NewStatus { get; set; }
    }

}
