using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_backend.Models.DTOs
{
    public class EditAccountDTO
    {
        public int ID { get; set; }
        public string First { get; set; }
        public string Last { get; set; }
        public string Email { get; set; }
        public string DoB { get; set; }
        public string? Phone  { get; set; }
        public string? Address { get; set; }
    }
}