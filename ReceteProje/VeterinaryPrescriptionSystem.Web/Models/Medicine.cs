using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VeterinaryPrescriptionSystem.Web.Models
{
    public class Medicine
    {
        public Medicine()
        {
            PrescriptionMedicines = new List<PrescriptionMedicine>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "İlaç adı zorunludur.")]
        [StringLength(100, ErrorMessage = "İlaç adı en fazla {1} karakter olabilir.")]
        [Display(Name = "İlaç Adı")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açıklama en fazla {1} karakter olabilir.")]
        [Display(Name = "Açıklama")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Stok miktarı zorunludur.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı 0'dan küçük olamaz.")]
        [Display(Name = "Stok Miktarı")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Birim seçimi zorunludur.")]
        [StringLength(50, ErrorMessage = "Birim en fazla {1} karakter olabilir.")]
        [Display(Name = "Birim")]
        public string Unit { get; set; } = string.Empty;

        // Navigation Properties
        public virtual ICollection<PrescriptionMedicine> PrescriptionMedicines { get; set; }
    }
} 