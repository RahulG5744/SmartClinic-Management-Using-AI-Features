using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartClinicSystem.Data;
using SmartClinicSystem.Models;

namespace SmartClinicSystem.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(AppointmentStatus? statusFilter)
        {
            var appointments = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .AsQueryable();

            if (statusFilter.HasValue)
            {
                appointments = appointments.Where(a => a.Status == statusFilter);
            }

            ViewBag.StatusFilter = statusFilter;
            return View(await appointments.OrderByDescending(a => a.AppointmentDate).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Prescription)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null) return NotFound();
            return View(appointment);
        }

        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
           [Bind("PatientId,DoctorId,AppointmentDate,AppointmentTime,Reason")] Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                var conflictMessage = await CheckDoctorAvailabilityAsync(
                    appointment.DoctorId, appointment.AppointmentDate, appointment.AppointmentTime);

                if (conflictMessage != null)
                {
                    ModelState.AddModelError(string.Empty, conflictMessage);
                    PopulateDropdowns(appointment.PatientId, appointment.DoctorId);
                    return View(appointment);
                }

                appointment.CreatedOn = DateTime.Now;
                appointment.Status = AppointmentStatus.Pending;
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Appointment booked successfully.";
                return RedirectToAction(nameof(Index));
            }
            PopulateDropdowns(appointment.PatientId, appointment.DoctorId);
            return View(appointment);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();
            PopulateDropdowns(appointment.PatientId, appointment.DoctorId);
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,PatientId,DoctorId,AppointmentDate,AppointmentTime,Reason,Status,CreatedOn")] Appointment appointment)
        {
            if (id != appointment.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var conflictMessage = await CheckDoctorAvailabilityAsync(
                    appointment.DoctorId, appointment.AppointmentDate, appointment.AppointmentTime,
                    excludeAppointmentId: appointment.Id);

                if (conflictMessage != null)
                {
                    ModelState.AddModelError(string.Empty, conflictMessage);
                    PopulateDropdowns(appointment.PatientId, appointment.DoctorId);
                    return View(appointment);
                }

                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Appointment updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateDropdowns(appointment.PatientId, appointment.DoctorId);
            return View(appointment);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null) return NotFound();
            return View(appointment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Appointment cancelled/removed.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }

        /// <summary>
        /// Checks whether a doctor can be booked at the given date/time.
        /// Returns null if available, or an error message explaining why not.
        /// </summary>
        private async Task<string?> CheckDoctorAvailabilityAsync(
            int doctorId, DateTime date, TimeSpan time, int? excludeAppointmentId = null)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null)
            {
                return "Selected doctor does not exist.";
            }

            // Rule 1: time must fall inside the doctor's working hours
            if (time < doctor.AvailableFrom || time > doctor.AvailableTo)
            {
                return $"{doctor.FullName} is only available between " +
                       $"{doctor.AvailableFrom:hh\\:mm} and {doctor.AvailableTo:hh\\:mm}. " +
                       "Please choose a time within these hours.";
            }

            // Rule 2: doctor must not already have an active appointment at this exact slot
            var conflictExists = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == doctorId &&
                a.AppointmentDate.Date == date.Date &&
                a.AppointmentTime == time &&
                a.Status != AppointmentStatus.Cancelled &&
                (excludeAppointmentId == null || a.Id != excludeAppointmentId));

            if (conflictExists)
            {
                return $"{doctor.FullName} already has an appointment booked at this exact date and time. " +
                       "Please choose a different time slot.";
            }

            return null; // available
        }

        private void PopulateDropdowns(int? selectedPatient = null, int? selectedDoctor = null)
        {
            ViewBag.PatientId = new SelectList(_context.Patients.OrderBy(p => p.FullName), "Id", "FullName", selectedPatient);
            ViewBag.DoctorId = new SelectList(_context.Doctors.OrderBy(d => d.FullName), "Id", "FullName", selectedDoctor);
        }
    }
}