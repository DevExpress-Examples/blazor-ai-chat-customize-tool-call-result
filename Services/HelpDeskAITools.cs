using DevExpress.AIIntegration;
using DxAiChatCustomToolCallingChartReport.Models;
using System.ComponentModel;

namespace DxAiChatCustomToolCallingChartReport.Services
{
    public class HelpDeskAITools
    {
        public static List<ChartReportData>? PendingChartData { get; private set; }

        public static List<ChartReportData>? ConsumePendingChartData()
        {
            var data = PendingChartData;
            PendingChartData = null;
            return data;
        }

        [AIIntegrationTool("HelpDesk_GetFeedbackChart")]
        [Description("Returns a feedback summary chart showing how many tickets are Positive, Negative, and Neutral. Use when the user asks about feedback or ratings.")]
        public static string GetFeedbackChart([AIIntegrationToolTarget("The help desk data service.")] HelpDeskDataService dataService)
        {
            var summary = dataService.GetFeedbackSummary();
            PendingChartData = summary;
            return string.Join(", ", summary.Select(s => $"{s.Label}: {s.Value}"));
        }
    }
}
