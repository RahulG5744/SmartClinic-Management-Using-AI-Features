using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartClinicSystem.Data;
using SmartClinicSystem.Models;

namespace SmartClinicSystem.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString)
        {
            var doctors = _context.Doctors.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                doctors = doctors.Where(d =>
                    d.FullName.Contains(searchString) ||
                    d.Specialization.Contains(searchString));
            }

            ViewBag.CurrentFilter = searchString;
            return View(await doctors.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var doctor = await _context.Doctors
                .Include(d => d.Appointments)!
                .ThenInclude(a => a.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (doctor == null) return NotFound();
            return View(doctor);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("FullName,Specialization,Phone,Email,ExperienceYears,AvailableFrom,AvailableTo")] Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(doctor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Doctor added successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(doctor);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();
            return View(doctor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,FullName,Specialization,Phone,Email,ExperienceYears,AvailableFrom,AvailableTo")] Doctor doctor)
        {
            if (id != doctor.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Doctor details updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(doctor);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var doctor = await _context.Doctors.FirstOrDefaultAsync(m => m.Id == id);
            if (doctor == null) return NotFound();
            return View(doctor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Doctor record deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(e => e.Id == id);
        }
    }
}
