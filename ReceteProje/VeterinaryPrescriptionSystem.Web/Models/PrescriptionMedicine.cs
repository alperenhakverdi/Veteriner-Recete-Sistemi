using System.ComponentModel.DataAnnotations;

namespace VeterinaryPrescriptionSystem.Web.Models
{
    public class PrescriptionMedicine
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Reçete seçimi zorunludur.")]
        public int PrescriptionId { get; set; }

        [Required(ErrorMessage = "İlaç seçimi zorunludur.")]
        [Display(Name = "İlaç")]
        public int MedicineId { get; set; }

        [Required(ErrorMessage = "Günlük doz bilgisi zorunludur.")]
        [Range(0.01, 1000, ErrorMessage = "Günlük doz 0.01 ile 1000 arasında olmalıdır.")]
        [Display(Name = "Günlük Doz")]
        public decimal DailyDose { get; set; }

        [Required(ErrorMessage = "Kullanım süresi zorunludur.")]
        [Range(1, 365, ErrorMessage = "Kullanım süresi 1 ile 365 gün arasında olmalıdır.")]
        [Display(Name = "Kullanım Süresi (Gün)")]
        public int DurationInDays { get; set; }

        [StringLength(200, ErrorMessage = "Kullanım talimatı en fazla {1} karakter olabilir.")]
        [Display(Name = "Kullanım Talimatı")]
        [DataType(DataType.MultilineText)]
        public string? Instructions { get; set; }

        // Navigation Properties
        public virtual Prescription Prescription { get; set; } = null!;
        public virtual Medicine Medicine { get; set; } = null!;
    }
} 