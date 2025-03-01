using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinaryPrescriptionSystem.Web.Data;
using VeterinaryPrescriptionSystem.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace VeterinaryPrescriptionSystem.Web.Controllers
{
    [Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
    public class PrescriptionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<PrescriptionsController> _logger;

        public PrescriptionsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<PrescriptionsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Prescriptions
        public async Task<IActionResult> Index()
        {
            var prescriptions = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.PrescriptionMedicines)
                    .ThenInclude(pm => pm.Medicine)
                .OrderByDescending(p => p.PrescriptionDate)
                .ToListAsync();
            return View(prescriptions);
        }

        // GET: Prescriptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.PrescriptionMedicines)
                    .ThenInclude(pm => pm.Medicine)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (prescription == null)
            {
                return NotFound();
            }

            return View(prescription);
        }

        // GET: Prescriptions/Create
        public IActionResult Create()
        {
            // Hasta listesi için SelectList oluştur
            ViewData["PatientId"] = new SelectList(_context.Patients.OrderBy(p => p.Name), "Id", "Name");
            
            // İlaç listesi için SelectList oluştur
            var medicines = _context.Medicines
                .Where(m => m.StockQuantity > 0) // Sadece stokta olan ilaçları göster
                .OrderBy(m => m.Name)
                .Select(m => new
                {
                    Value = m.Id.ToString(),
                    Text = m.Name,
                    Unit = m.Unit
                })
                .ToList();
            ViewBag.Medicines = medicines;

            return View();
        }

        // POST: Prescriptions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Prescription prescription, List<PrescriptionMedicine> medicines)
        {
            try
            {
                // Debug için model state hatalarını logla
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogError($"Model State Error: {error.ErrorMessage}");
                    }
                }

                // Veteriner hekim kontrolü
                if (string.IsNullOrWhiteSpace(prescription.VeterinarianId))
                {
                    ModelState.AddModelError("VeterinarianId", "Veteriner hekim bilgisi zorunludur.");
                    PrepareViewData(prescription.PatientId);
                    return View(prescription);
                }

                // Patient kontrolü
                if (prescription.PatientId <= 0)
                {
                    ModelState.AddModelError("PatientId", "Lütfen bir hasta seçiniz.");
                    PrepareViewData(prescription.PatientId);
                    return View(prescription);
                }

                var patient = await _context.Patients.FindAsync(prescription.PatientId);
                if (patient == null)
                {
                    ModelState.AddModelError("PatientId", "Seçilen hasta bulunamadı.");
                    PrepareViewData(prescription.PatientId);
                    return View(prescription);
                }

                if (string.IsNullOrWhiteSpace(prescription.Diagnosis))
                {
                    ModelState.AddModelError("Diagnosis", "Tanı bilgisi zorunludur.");
                    PrepareViewData(prescription.PatientId);
                    return View(prescription);
                }

                // İlaç listesini temizle ve yeniden oluştur
                prescription.PrescriptionMedicines = new List<PrescriptionMedicine>();

                // Reçeteyi kaydet
                _context.Add(prescription);
                await _context.SaveChangesAsync();

                // İlaçları ekle (eğer varsa)
                if (medicines != null && medicines.Any(m => m.MedicineId > 0))
                {
                    foreach (var medicine in medicines.Where(m => m.MedicineId > 0))
                    {
                        var dbMedicine = await _context.Medicines.FindAsync(medicine.MedicineId);
                        if (dbMedicine == null)
                        {
                            throw new InvalidOperationException($"İlaç bulunamadı (ID: {medicine.MedicineId})");
                        }

                        var totalUsage = medicine.DailyDose * medicine.DurationInDays;
                        if (dbMedicine.StockQuantity < totalUsage)
                        {
                            throw new InvalidOperationException($"{dbMedicine.Name} için yeterli stok bulunmamaktadır. Mevcut stok: {dbMedicine.StockQuantity} {dbMedicine.Unit}");
                        }

                        var prescriptionMedicine = new PrescriptionMedicine
                        {
                            PrescriptionId = prescription.Id,
                            MedicineId = medicine.MedicineId,
                            DailyDose = medicine.DailyDose,
                            DurationInDays = medicine.DurationInDays,
                            Instructions = medicine.Instructions
                        };

                        _context.PrescriptionMedicines.Add(prescriptionMedicine);
                        dbMedicine.StockQuantity -= (int)totalUsage;
                    }

                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Reçete başarıyla kaydedildi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Reçete kaydedilirken bir hata oluştu: {ex.Message}");
                _logger.LogError($"Reçete kaydetme hatası: {ex}");
            }

            PrepareViewData(prescription.PatientId);
            return View(prescription);
        }

        private void PrepareViewData(int? selectedPatientId = null)
        {
            ViewData["PatientId"] = new SelectList(_context.Patients.OrderBy(p => p.Name), "Id", "Name", selectedPatientId);
            var medicines = _context.Medicines
                .Where(m => m.StockQuantity > 0)
                .OrderBy(m => m.Name)
                .Select(m => new
                {
                    Value = m.Id.ToString(),
                    Text = m.Name,
                    Unit = m.Unit
                })
                .ToList();
            ViewBag.Medicines = medicines;
        }

        // GET: Prescriptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions
                .Include(p => p.PrescriptionMedicines)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null)
            {
                return NotFound();
            }

            ViewData["PatientId"] = new SelectList(_context.Patients.OrderBy(p => p.Name), "Id", "Name", prescription.PatientId);
            return View(prescription);
        }

        // POST: Prescriptions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PrescriptionDate,PatientId,Diagnosis,Notes,VeterinarianId")] Prescription prescription)
        {
            if (id != prescription.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(prescription);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrescriptionExists(prescription.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PatientId"] = new SelectList(_context.Patients.OrderBy(p => p.Name), "Id", "Name", prescription.PatientId);
            return View(prescription);
        }

        // GET: Prescriptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.PrescriptionMedicines)
                    .ThenInclude(pm => pm.Medicine)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (prescription == null)
            {
                return NotFound();
            }

            return View(prescription);
        }

        // POST: Prescriptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.PrescriptionMedicines)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription != null)
            {
                // İlaç stoklarını geri yükle
                foreach (var pm in prescription.PrescriptionMedicines)
                {
                    var medicine = await _context.Medicines.FindAsync(pm.MedicineId);
                    if (medicine != null)
                    {
                        var totalUsage = pm.DailyDose * pm.DurationInDays;
                        medicine.StockQuantity += (int)totalUsage;
                    }
                }

                _context.Prescriptions.Remove(prescription);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PrescriptionExists(int id)
        {
            return _context.Prescriptions.Any(e => e.Id == id);
        }
    }
}
