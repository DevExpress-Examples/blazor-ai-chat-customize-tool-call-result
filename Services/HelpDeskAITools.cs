using DevExpress.AIIntegration;
using DxAiChatCustomToolCallingChartReport.Models;
using System.ComponentModel;

namespace DxAiChatCustomToolCallingChartReport.Services {
    public class HelpDeskAITools {
        public static List<ChartReportData>? PendingChartData { get; private set; }

        public static List<ChartReportData>? ConsumePendingChartData() {
            var data = PendingChartData;
            PendingChartData = null;
            return data;
        }

        [AIIntegrationTool("HelpDesk_GetFeedbackChart")]
        [Description("Returns a feedback summary chart showing how many tickets are Positive, Negative, and Neutral. Use when the user asks about feedback or ratings. The categoriesToInclude parameter filters which feedback categories to display (e.g. [\"Positive\", \"Neutral\"] to exclude Negative). If empty or not specified, all categories are shown.")]
        public static string GetFeedbackChart(
            [AIIntegrationToolTarget("The help desk data service.")] HelpDeskDataService dataService,
            [Description("Feedback categories to include in the chart, e.g. [\"Positive\", \"Negative\", \"Neutral\"]. Leave empty to include all.")] string[] categories) {
            var summary = dataService.GetFeedbackSummary();

            if(categories != null && categories.Length > 0) {
                var filter = new HashSet<string>(categories, StringComparer.OrdinalIgnoreCase);
                summary = summary.Where(s => filter.Contains(s.Label)).ToList();
            }

            PendingChartData = summary;
            return string.Join(", ", summary.Select(s => $"{s.Label}: {s.Value}"));
        }
    }
}
