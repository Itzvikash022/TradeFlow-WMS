using Microsoft.AspNetCore.Mvc;
using WMS_Application.Repositories;
using WMS_Application.Repositories.Interfaces;
using WMS_Application.Models;

namespace WMS_Application.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentRepository _payment;

        public PaymentController(IPaymentRepository payment)
        {
            _payment = payment;
        }

        [HttpPost]
        public async Task<IActionResult> InitiatePayment([FromBody] PaymentRequestModel model)
        {
            // model will have amount, order id etc.
            int amountInPaise = (int)(model.Amount * 100); // Razorpay needs paise
            string receiptId = "rcpt_" + model.OrderId;

            var orderId = await _payment.CreatePaymentAsync(amountInPaise, "INR", receiptId);

            return Ok(new {success = true, orderId = orderId });
        }
    }
}
