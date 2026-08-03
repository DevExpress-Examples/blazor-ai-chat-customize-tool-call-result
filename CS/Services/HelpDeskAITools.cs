using System.ComponentModel;
using DevExpress.AIIntegration;
using DxAiChatCustomToolCallingChartReport.Models;

namespace DxAiChatCustomToolCallingChartReport.Services {
    public class HelpDeskAITools {
        const string GetFeedbackChartToolDescription =
            "Returns a feedback summary chart showing how many tickets are Positive, Negative, and Neutral. " +
            "Use when the user asks about feedback or ratings. " +
            $"The categories parameter filters which feedback categories to display (e.g. [\"{HelpDeskDataService.PositiveFeedbackLabel}\", \"{HelpDeskDataService.NeutralFeedbackLabel}\"] to exclude {HelpDeskDataService.NegativeFeedbackLabel}). " +
            "If empty or not specified, all categories are shown.";

        const string GetFeedbackChartToolFilterDescription =
            $"Feedback categories to include in the chart, e.g. [\"{HelpDeskDataService.PositiveFeedbackLabel}\", \"{HelpDeskDataService.NegativeFeedbackLabel}\", \"{HelpDeskDataService.NeutralFeedbackLabel}\"]. " +
            "Leave empty to include all.";

        public const string GetFeedbackChartToolName = "HelpDesk_GetFeedbackChart";

        private readonly HelpDeskDataService dataService;

        public HelpDeskAITools(HelpDeskDataService dataService) {
            this.dataService = dataService;
        }

        [AIIntegrationTool(GetFeedbackChartToolName)]
        [Description(GetFeedbackChartToolDescription)]
        public List<ChartReportData> GetFeedbackChart([Description(GetFeedbackChartToolFilterDescription)] string[] categories) {
            var summary = dataService.GetFeedbackSummary();

            if(categories != null && categories.Length > 0) {
                var filter = new HashSet<string>(categories, StringComparer.OrdinalIgnoreCase);
                summary = summary.Where(s => filter.Contains(s.Label)).ToList();
            }

            return summary;
        }   
    }
}
