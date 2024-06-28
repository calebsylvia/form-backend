using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_backend.Models.DTOs
{
    public class ResetPasswordDTO
    {
        public string Email { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}