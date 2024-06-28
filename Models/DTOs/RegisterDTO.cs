using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_backend.Models.DTOs
{
    public class RegisterDTO
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}