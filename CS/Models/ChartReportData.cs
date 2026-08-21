using System.Text.Json.Serialization;

namespace DxAiChatCustomToolCallingChartReport.Models
{
    public class ChartReportData
    {
        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;
        [JsonPropertyName("value")]
        public int Value { get; set; }
    }
}
