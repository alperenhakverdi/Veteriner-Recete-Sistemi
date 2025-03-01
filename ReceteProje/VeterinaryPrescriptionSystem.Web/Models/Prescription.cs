using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VeterinaryPrescriptionSystem.Web.Models
{
    public class Prescription
    {
        public Prescription()
        {
            PrescriptionMedicines = new List<PrescriptionMedicine>();
            PrescriptionDate = DateTime.Now;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Reçete tarihi zorunludur.")]
        [Display(Name = "Reçete Tarihi")]
        [DataType(DataType.Date)]
        public DateTime PrescriptionDate { get; set; }

        [Required(ErrorMessage = "Hasta seçimi zorunludur.")]
        [Range(1, int.MaxValue, ErrorMessage = "Lütfen bir hasta seçiniz.")]
        [Display(Name = "Hasta")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Tanı bilgisi zorunludur.")]
        [StringLength(500, ErrorMessage = "Tanı bilgisi en fazla {1} karakter olabilir.")]
        [Display(Name = "Tanı")]
        [DataType(DataType.MultilineText)]
        public string Diagnosis { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Not en fazla {1} karakter olabilir.")]
        [Display(Name = "Notlar")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Veteriner hekim bilgisi zorunludur.")]
        [Display(Name = "Veteriner Hekim")]
        public string VeterinarianId { get; set; } = string.Empty;

        // Navigation Properties
        public virtual Patient? Patient { get; set; }
        public virtual ICollection<PrescriptionMedicine> PrescriptionMedicines { get; set; }
    }
} 