using WMS_Application.Repositories.Interfaces;
using Razorpay.Api;
using WMS_Application.Models;

namespace WMS_Application.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly RazorpaySettings _razorpaySettings;

        public PaymentRepository(RazorpaySettings razorpaySettings)
        {
            _razorpaySettings = razorpaySettings;
        }
        public async Task<string> CreatePaymentAsync(int amountInPaise, string currency, string receipt)
        {
            RazorpayClient client = new RazorpayClient(_razorpaySettings.KeyId, _razorpaySettings.KeySecret);

            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", amountInPaise); // Amount in paise (100 INR = 10000 paise)
            options.Add("currency", currency);
            options.Add("receipt", receipt);
            options.Add("payment_capture", 1); // Auto-capture payment

            Order order = client.Order.Create(options);

            return order["id"].ToString(); // Return Razorpay OrderId
        }
    }
}
