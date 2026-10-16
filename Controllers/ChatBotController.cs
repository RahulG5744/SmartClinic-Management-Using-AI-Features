using Microsoft.AspNetCore.Mvc;
using SmartClinicSystem.Data;
using SmartClinicSystem.Services;

namespace SmartClinicSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatBotController : ControllerBase
    {
        private readonly SymptomAssistantService _assistant;
        private readonly ApplicationDbContext _context;

        public ChatBotController(SymptomAssistantService assistant, ApplicationDbContext context)
        {
            _assistant = assistant;
            _context = context;
        }

        public class ChatRequest
        {
            public string Message { get; set; } = string.Empty;
        }

        // POST: api/ChatBot/ask
        [HttpPost("ask")]
        public IActionResult Ask([FromBody] ChatRequest request)
        {
            // This function is passed INTO the AI service so it can look up
            // a real doctor without needing to know how the database works.
            Func<string, string?> findDoctorBySpecialization = (specialization) =>
            {
                var doctor = _context.Doctors
                    .FirstOrDefault(d => d.Specialization == specialization);
                return doctor?.FullName;
            };

            var reply = _assistant.GetResponse(request.Message, HttpContext.Session, findDoctorBySpecialization);
            return Ok(new { reply });
        }
    }
}