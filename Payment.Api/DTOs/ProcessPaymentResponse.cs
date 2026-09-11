using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payment.Api.DTOs
{
    public record PaymentResponse(Guid Id, Guid OrderId, decimal Amount, string Status);
}