using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartClinicSystem.Data;
using SmartClinicSystem.Models;

namespace SmartClinicSystem.Controllers
{
    public class PrescriptionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrescriptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Prescriptions/Create/5   (5 = AppointmentId)
        public async Task<IActionResult> Create(int? appointmentId)
        {
            if (appointmentId == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Prescription)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) return NotFound();

            // Business rule: only allow adding a prescription for Completed appointments
            if (appointment.Status != AppointmentStatus.Completed)
            {
                TempData["Error"] = "You can only add a prescription once the appointment is marked Completed.";
                return RedirectToAction("Details", "Appointments", new { id = appointmentId });
            }

            // Business rule: don't allow a second prescription for the same appointment
            if (appointment.Prescription != null)
            {
                return RedirectToAction(nameof(Edit), new { id = appointment.Prescription.Id });
            }

            ViewBag.Appointment = appointment;

            var prescription = new Prescription { AppointmentId = appointment.Id };
            return View(prescription);
        }

        // POST: Prescriptions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("AppointmentId,Diagnosis,Medicines,Notes,FollowUpDate")] Prescription prescription)
        {
            if (ModelState.IsValid)
            {
                prescription.CreatedOn = DateTime.Now;
                _context.Add(prescription);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Prescription saved successfully.";
                return RedirectToAction("Details", "Appointments", new { id = prescription.AppointmentId });
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == prescription.AppointmentId);
            ViewBag.Appointment = appointment;
            return View(prescription);
        }

        // GET: Prescriptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var prescription = await _context.Prescriptions
                .Include(p => p.Appointment)!
                .ThenInclude(a => a!.Patient)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null) return NotFound();

            ViewBag.Appointment = prescription.Appointment;
            return View(prescription);
        }

        // POST: Prescriptions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,AppointmentId,Diagnosis,Medicines,Notes,FollowUpDate,CreatedOn")] Prescription prescription)
        {
            if (id != prescription.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(prescription);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Prescription updated.";
                return RedirectToAction("Details", "Appointments", new { id = prescription.AppointmentId });
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == prescription.AppointmentId);
            ViewBag.Appointment = appointment;
            return View(prescription);
        }
    }
}