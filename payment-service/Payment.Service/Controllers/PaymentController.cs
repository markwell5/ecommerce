using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Model.Payment.Response;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Payment.Application.Commands;
using Payment.Application.Queries;

namespace Payment.Service.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly IMediator _mediator;

        public PaymentController(ILogger<PaymentController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet("{orderId:guid}")]
        [ProducesResponseType(200, Type = typeof(PaymentResponse))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPaymentByOrder(Guid orderId)
        {
            var payment = await _mediator.Send(new GetPaymentByOrderQuery(orderId));

            if (payment == null)
                return NotFound();

            return Ok(payment);
        }

        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(200, Type = typeof(List<PaymentResponse>))]
        public async Task<IActionResult> GetPaymentsByCustomer(string customerId)
        {
            var payments = await _mediator.Send(new GetPaymentsByCustomerQuery(customerId));
            return Ok(payments);
        }

        [HttpPost("{paymentId:long}/refund")]
        [ProducesResponseType(200, Type = typeof(PaymentResponse))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RefundPayment(long paymentId)
        {
            var result = await _mediator.Send(new RefundPaymentCommand(paymentId));

            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}
