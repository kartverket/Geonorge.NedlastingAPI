using System;
using System.ComponentModel.DataAnnotations;

namespace Geonorge.Download.Models
{
    public class MachineAccount
    {
        [Key] public string Username { get; set; }

        public string? Passsword { get; set; }

        public string? Company { get; set; }

        public string? ContactPerson { get; set; }

        public string? ContactEmail { get; set; }

        public DateTime Created { get; set; }

        public string? Roles { get; set; }
    }
}