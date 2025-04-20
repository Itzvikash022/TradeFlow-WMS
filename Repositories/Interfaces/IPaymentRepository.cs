namespace WMS_Application.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<string> CreatePaymentAsync(int amountInPaise, string currency, string receipt);
    }
}
