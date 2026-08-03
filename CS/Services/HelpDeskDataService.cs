using DxAiChatCustomToolCallingChartReport.Models;

namespace DxAiChatCustomToolCallingChartReport.Services
{
    public class HelpDeskDataService
    {
        private readonly List<HelpDeskTicket> _tickets;

        public const string PositiveFeedbackLabel = "Positive";
        public const string NegativeFeedbackLabel = "Negative";
        public const string NeutralFeedbackLabel = "Neutral";

        private static List<HelpDeskTicket> GenerateRandomTickets(int count)
        {
            var random = new Random(42);
            var feedbackTypes = new[] { PositiveFeedbackLabel, NegativeFeedbackLabel, NeutralFeedbackLabel };
            var tickets = new List<HelpDeskTicket>();

            for (int i = 1; i <= count; i++)
            {
                tickets.Add(new HelpDeskTicket
                {
                    Id = i,
                    Feedback = feedbackTypes[random.Next(feedbackTypes.Length)]
                });
            }

            return tickets;
        }

        public HelpDeskDataService() {
            _tickets = GenerateRandomTickets(100);
        }

        public List<ChartReportData> GetFeedbackSummary()
        {
            return _tickets
                .GroupBy(t => t.Feedback)
                .Select(g => new ChartReportData { Label = g.Key, Value = g.Count() })
                .ToList();
        }
    }
}
