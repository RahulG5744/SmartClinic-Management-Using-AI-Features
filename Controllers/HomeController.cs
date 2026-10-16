using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartClinicSystem.Data;

namespace SmartClinicSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.TotalPatients = await _context.Patients.CountAsync();
            ViewBag.TotalDoctors = await _context.Doctors.CountAsync();
            ViewBag.TotalAppointments = await _context.Appointments.CountAsync();
            ViewBag.PendingAppointments = await _context.Appointments
                .CountAsync(a => a.Status == Models.AppointmentStatus.Pending);

            ViewBag.RecentAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.CreatedOn)
                .Take(5)
                .ToListAsync();

            // Chart 1: appointments by status
            var statusCounts = await _context.Appointments
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            ViewBag.StatusLabels = statusCounts.Select(s => s.Status).ToList();
            ViewBag.StatusData = statusCounts.Select(s => s.Count).ToList();

            // Chart 2: appointments booked per day, last 7 days
            var start = DateTime.Today.AddDays(-6);
            var dailyCounts = await _context.Appointments
                .Where(a => a.CreatedOn >= start)
                .GroupBy(a => a.CreatedOn.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var days = Enumerable.Range(0, 7).Select(i => start.AddDays(i)).ToList();
            ViewBag.DailyLabels = days.Select(d => d.ToString("dd MMM")).ToList();
            ViewBag.DailyData = days
                .Select(d => dailyCounts.FirstOrDefault(x => x.Date == d)?.Count ?? 0)
                .ToList();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}