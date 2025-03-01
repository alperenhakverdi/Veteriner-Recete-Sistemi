using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VeterinaryPrescriptionSystem.Web.Models
{
    public class Patient
    {
        public Patient()
        {
            Prescriptions = new List<Prescription>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Hasta adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Hasta adı en fazla {1} karakter olabilir.")]
        [Display(Name = "Hasta Adı")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yaş bilgisi zorunludur.")]
        [Range(0, 100, ErrorMessage = "Yaş 0 ile 100 arasında olmalıdır.")]
        [Display(Name = "Yaş")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Tür bilgisi zorunludur.")]
        [StringLength(50, ErrorMessage = "Tür bilgisi en fazla {1} karakter olabilir.")]
        [Display(Name = "Tür")]
        public string Species { get; set; } = string.Empty;

        [Required(ErrorMessage = "Irk bilgisi zorunludur.")]
        [StringLength(50, ErrorMessage = "Irk bilgisi en fazla {1} karakter olabilir.")]
        [Display(Name = "Irk")]
        public string Breed { get; set; } = string.Empty;

        // Owner Information
        [Required(ErrorMessage = "Hasta sahibinin adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Hasta sahibinin adı en fazla {1} karakter olabilir.")]
        [Display(Name = "Hasta Sahibinin Adı")]
        public string OwnerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [Display(Name = "Telefon Numarası")]
        public string OwnerPhone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta Adresi")]
        public string? OwnerEmail { get; set; }

        // Navigation Properties
        public virtual ICollection<Prescription> Prescriptions { get; set; }
    }
} 