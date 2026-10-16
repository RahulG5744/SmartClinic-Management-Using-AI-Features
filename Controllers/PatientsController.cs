using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartClinicSystem.Data;
using SmartClinicSystem.Models;

namespace SmartClinicSystem.Controllers
{
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Patients
        public async Task<IActionResult> Index(string? searchString)
        {
            var patients = _context.Patients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                patients = patients.Where(p =>
                    p.FullName.Contains(searchString) ||
                    p.Phone.Contains(searchString));
            }

            ViewBag.CurrentFilter = searchString;
            return View(await patients.OrderByDescending(p => p.RegisteredOn).ToListAsync());
        }

        // GET: Patients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients
                .Include(p => p.Appointments)!
                .ThenInclude(a => a.Doctor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (patient == null) return NotFound();

            return View(patient);
        }

        // GET: Patients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("FullName,Age,Gender,Phone,Email,Address,MedicalHistory")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                patient.RegisteredOn = DateTime.Now;
                _context.Add(patient);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Patient registered successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        // GET: Patients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            return View(patient);
        }

        // POST: Patients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,FullName,Age,Gender,Phone,Email,Address,MedicalHistory,RegisteredOn")] Patient patient)
        {
            if (id != patient.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Patient details updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        // GET: Patients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients.FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null) return NotFound();

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Patient record deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.Id == id);
        }
    }
}
