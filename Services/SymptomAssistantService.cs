using Microsoft.AspNetCore.Http;

namespace SmartClinicSystem.Services
{
    /// <summary>
    /// Multi-question AI Symptom Assistant. Instead of replying to a single
    /// message, it asks one clarifying follow-up question first (the way
    /// real triage tools like Ada/Apollo work), then combines both answers
    /// into a more specific, non-diagnostic response.
    /// </summary>
    public class SymptomAssistantService
    {
        private const string SessionKeyTopic = "AI_ActiveTopic";
        private const string SessionKeyFirstMessage = "AI_FirstMessage";

        private readonly List<SymptomTopic> _topics = new()
        {
            new SymptomTopic
            {
                Name = "fever",
                Keywords = new[] { "fever", "temperature", "hot body" },
                RecommendedSpecialization = "General Physician",
                FollowUpQuestion = "How many days have you had the fever, and is it above 102°F (39°C)?",
                BuildFinalResponse = (first, followUp) =>
                {
                    var lower = followUp.ToLowerInvariant();
                    bool longDuration = lower.Contains("3") || lower.Contains("4") || lower.Contains("5") || lower.Contains("week");
                    bool highTemp = lower.Contains("102") || lower.Contains("103") || lower.Contains("104") || lower.Contains("high");

                    if (longDuration || highTemp)
                    {
                        return "A fever that's lasted several days or is running high needs a proper check-up rather than just home care. " +
                               "I'd recommend booking with a {DOCTOR} soon.";
                    }
                    return "For a mild, recent fever: rest, fluids, and monitor your temperature. " +
                           "If it doesn't improve in a day or two, book with a {DOCTOR}.";
                }
            },
            new SymptomTopic
            {
                Name = "headache",
                Keywords = new[] { "headache", "migraine", "head pain" },
                RecommendedSpecialization = "General Physician",
                FollowUpQuestion = "How long have you had this headache, and do you also have nausea, fever, or sensitivity to light?",
                BuildFinalResponse = (first, followUp) =>
                {
                    var lower = followUp.ToLowerInvariant();
                    bool warning = lower.Contains("nausea") || lower.Contains("fever") || lower.Contains("light") || lower.Contains("vomit");

                    if (warning)
                    {
                        return "A headache along with nausea, fever, or light sensitivity is worth getting checked properly. " +
                               "I'd recommend booking with a {DOCTOR}.";
                    }
                    return "A plain headache is often stress, dehydration, or lack of sleep — try resting in a quiet, dark room and drink water. " +
                           "If it persists beyond a day or keeps returning, book with a {DOCTOR}.";
                }
            },
            new SymptomTopic
            {
                Name = "chest pain",
                Keywords = new[] { "chest pain", "heart", "palpitation" },
                RecommendedSpecialization = "Cardiologist",
                FollowUpQuestion = "Is the pain sharp or pressure-like, and does it spread to your arm, jaw, or back?",
                BuildFinalResponse = (first, followUp) =>
                {
                    return "Chest pain should always be evaluated by a doctor, especially if it spreads or feels like pressure. " +
                           "Please book with a {DOCTOR} as soon as possible, and if it's severe or sudden, seek emergency care immediately.";
                }
            },
            new SymptomTopic
            {
                Name = "stomach",
                Keywords = new[] { "stomach", "abdominal", "vomiting", "nausea", "diarrhea" },
                RecommendedSpecialization = "General Physician",
                FollowUpQuestion = "How long has this been going on, and is there any blood or severe pain involved?",
                BuildFinalResponse = (first, followUp) =>
                {
                    var lower = followUp.ToLowerInvariant();
                    bool warning = lower.Contains("blood") || lower.Contains("severe");

                    if (warning)
                    {
                        return "Blood or severe pain alongside stomach issues needs prompt medical attention. " +
                               "Please book with a {DOCTOR} right away.";
                    }
                    return "Mild stomach issues are often diet-related — stay hydrated and eat light food. " +
                           "If it persists beyond 2 days, book with a {DOCTOR}.";
                }
            }
        };

        public string GetResponse(string userMessage, ISession session, Func<string, string?> findDoctorBySpecialization)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return "Please type your symptom or question so I can help you.";

            var message = userMessage.ToLowerInvariant();

            // ---- Are we in the middle of a follow-up conversation? ----
            var activeTopicName = session.GetString(SessionKeyTopic);
            if (!string.IsNullOrEmpty(activeTopicName))
            {
                var topic = _topics.FirstOrDefault(t => t.Name == activeTopicName);
                var firstMessage = session.GetString(SessionKeyFirstMessage) ?? string.Empty;

                // Clear state now that we're completing this conversation
                session.Remove(SessionKeyTopic);
                session.Remove(SessionKeyFirstMessage);

                if (topic != null)
                {
                    var response = topic.BuildFinalResponse(firstMessage, userMessage);
                    var doctorName = findDoctorBySpecialization(topic.RecommendedSpecialization);
                    var doctorText = doctorName != null
                        ? $"{doctorName} ({topic.RecommendedSpecialization})"
                        : $"a {topic.RecommendedSpecialization}";

                    return response.Replace("{DOCTOR}", doctorText);
                }
            }

            // ---- Greetings / small talk (no follow-up needed) ----
            if (message.Contains("hi") || message.Contains("hello") || message.Contains("hey"))
                return "Hello! I'm the SmartClinic AI Assistant. Tell me your symptoms and I'll ask a couple of quick questions to guide you.";

            if (message.Contains("thank"))
                return "You're welcome! Take care, and don't hesitate to book an appointment if symptoms continue.";

            // ---- New symptom message: find a matching topic and ask the follow-up ----
            foreach (var topic in _topics)
            {
                if (topic.Keywords.Any(k => message.Contains(k)))
                {
                    session.SetString(SessionKeyTopic, topic.Name);
                    session.SetString(SessionKeyFirstMessage, userMessage);
                    return topic.FollowUpQuestion;
                }
            }

            return "I couldn't find specific guidance for that. For anything beyond mild, everyday symptoms, it's best to book an appointment " +
                   "so you get a proper diagnosis. This assistant only provides general information and is not a substitute for professional medical advice.";
        }
    }
}