namespace SmartClinicSystem.Services
{
    /// <summary>
    /// Represents one symptom category the AI assistant can discuss.
    /// Holds the follow-up question to ask, and the logic to produce
    /// a final answer once the user has answered it.
    /// </summary>
    public class SymptomTopic
    {
        public string Name { get; set; } = string.Empty;
        public string[] Keywords { get; set; } = Array.Empty<string>();
        public string FollowUpQuestion { get; set; } = string.Empty;
        public string RecommendedSpecialization { get; set; } = string.Empty;

        /// <summary>
        /// Builds the final reply using both the original message and the
        /// user's answer to the follow-up question. Keeping this as a
        /// function (not just static text) lets each topic apply its own
        /// simple severity logic.
        /// </summary>
        public Func<string, string, string> BuildFinalResponse { get; set; }
            = (initialMessage, followUpAnswer) => "Please consider booking an appointment for a proper evaluation.";
    }
}