using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auth.Api.DTOs
{
    public record RegisterRequest(string Email, string Password);
}