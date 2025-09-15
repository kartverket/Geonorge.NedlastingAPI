using System;
using System.ComponentModel.DataAnnotations;

namespace Geonorge.Download.Models
{
    public class MachineAccountCreate_VM
    {
        [Required, StringLength(100)]
        public string Username { get; set; } = string.Empty;

        // NOTE: keeping your original property name "Passsword" in case it matches DB/DTO.
        // If it was a typo, fix to "Password" across the stack.
        [Required, DataType(DataType.Password), StringLength(200)]
        public string Password { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Company { get; set; }

        [Display(Name = "Contact person"), StringLength(200)]
        public string? ContactPerson { get; set; }

        [EmailAddress, Display(Name = "Contact email")]
        public string? ContactEmail { get; set; }

        public DateTime? Created { get; set; }

        // The roles chosen in the UI
        public List<string> Roles { get; set; } = new();
    }

    public class MachineAccountEdit_VM
    {
        [Required, StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [DataType(DataType.Password), StringLength(200)]
        public string? Password { get; set; }

        [StringLength(200)]
        public string? Company { get; set; }

        [Display(Name = "Contact person"), StringLength(200)]
        public string? ContactPerson { get; set; }

        [EmailAddress, Display(Name = "Contact email")]
        public string? ContactEmail { get; set; }

        public DateTime? Created { get; set; }

        // The roles chosen in the UI
        public List<string> Roles { get; set; } = new();
    }
}