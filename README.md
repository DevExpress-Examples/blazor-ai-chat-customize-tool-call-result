# Blazor AI Chat — Customize Tool Calling Result

DevExpress Blazor [AI Chat](https://docs.devexpress.com/Blazor/405290) can query live enterprise data using natural language. You can register custom AI tools designed to aggregate Help Desk ticket feedback and display results as an interactive bar chart directly inside chat responses.

![Customize Tool Calling Result](ai-chat-tool-call-customize.png)

The sample app relies on the following DevExpress Blazor components:

- [DxAIChat](https://docs.devexpress.com/Blazor/DevExpress.AIIntegration.Blazor.Chat.DxAIChat)
- [DxChart](https://docs.devexpress.com/Blazor/DevExpress.Blazor.Charts.DxChart)
- [DxChartBarSeries](https://docs.devexpress.com/Blazor/DevExpress.Blazor.Charts.DxChartBarSeries-3)
- [DxAIChatPromptSuggestion](https://docs.devexpress.com/Blazor/DevExpress.AIIntegration.Blazor.Chat.DxAIChatPromptSuggestion)

## Setup and Configuration

To run this sample, restore project dependencies and set up secure authentication for Azure OpenAI.

### Required Packages

We use the following versions of Microsoft AI packages in this project:

| NuGet Package                                                                                   | Version                 |
| ----------------------------------------------------------------------------------------------- | ----------------------- |
| [Microsoft.Extensions.AI](https://www.nuget.org/packages/Microsoft.Extensions.AI)               | 9.7.1                   |
| [Microsoft.Extensions.AI.OpenAI](https://www.nuget.org/packages/Microsoft.Extensions.AI.OpenAI) | 9.7.1-preview.1.25365.4 |
| [Azure.AI.OpenAI](https://www.nuget.org/packages/Azure.AI.OpenAI)                               | 2.3.0-beta.2            |

> [!NOTE]
> We cannot guarantee compatibility or correct execution with newer versions. Refer to the following announcement for additional information: [DevExpress.AIIntegration references stable versions of Microsoft AI packages](https://supportcenter.devexpress.com/ticket/details/t1292705/devexpress-aiintegration-references-stable-versions-of-microsoft-ai-packages).

### Register AI Services

> [!NOTE]
> DevExpress AI-powered extensions follow the "bring your own key" principle. DevExpress does not offer a REST API and does not ship any built-in LLMs/SLMs. You need an active Azure OpenAI subscription to obtain the REST API endpoint, key, and model deployment name. These variables must be specified at application startup to register AI clients and enable DevExpress AI-powered Extensions in your application.

This example uses the [Azure OpenAI](https://azure.microsoft.com/en-us/products/ai-foundry/models/openai) service.

Secrets are stored in [appsettings.json](CS/appsettings.json). Update the following section with your own credentials:

- `AzureOpenAISettings`
  - `Endpoint`: Your Azure OpenAI endpoint
  - `Key`: Your Azure OpenAI key
  - `DeploymentName`: Azure OpenAI [model ID](https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/models)

The following code in [Program.cs](CS/Program.cs) retrieves the Azure OpenAI configuration and registers the chat client with tool support:

```csharp
var openAiServiceSettings = builder.Configuration.GetSection("AzureOpenAISettings")
    .Get<AzureOpenAIServiceSettings>();

var azureOpenAIClient = new AzureOpenAIClient(
    new Uri(openAiServiceSettings.Endpoint),
    new AzureKeyCredential(openAiServiceSettings.Key));

var chatClient = azureOpenAIClient
    .GetChatClient(openAiServiceSettings.DeploymentName)
    .AsIChatClient();

builder.Services.AddScoped<IChatClient>((sp) => {
    return chatClient.AsBuilder()
        .UseDXTools()
        .UseFunctionInvocation()
        .Build(sp);
});

builder.Services.AddDevExpressAI();
```

## Implementation Details

### Custom Tool Calling

The [HelpDeskAITools](CS/Services/HelpDeskAITools.cs) class defines the AI tool - a static method decorated with `[AIIntegrationTool]` that the AI model can invoke when the user asks about feedback data. The tool accepts an optional `categories` filter to control which feedback types appear in the chart.

The tool stores aggregated chart data in a static `PendingChartData` property. After receiving the AI response, `ConsumePendingChartData()` retrieves and clears the data for rendering:

```csharp
[AIIntegrationTool("HelpDesk_GetFeedbackChart")]
[Description("Returns a feedback summary chart...")]
public static string GetFeedbackChart(
    [AIIntegrationToolTarget("The help desk data service.")] HelpDeskDataService dataService,
    [Description("Feedback categories to include...")] string[] categories) {
    var summary = dataService.GetFeedbackSummary();
    // ...filter and store chart data
    PendingChartData = summary;
    return string.Join(", ", summary.Select(s => $"{s.Label}: {s.Value}"));
}
```

AI tool is registered in [Index.razor](CS/Components/Pages/Index/Index.razor) on the first render using `AIToolsContextBuilder`:

```csharp
toolsContext = new AIToolsContextBuilder()
    .WithToolTarget(HelpDeskService, "The Help Desk data service.")
    .WithToolMethods(HelpDeskAITools.GetFeedbackChart)
    .Build();

AIToolsContainer.Add(toolsContext);
```

### Inline Chart Rendering

The [AI Chat](CS/Components/Pages/Index/Index.razor) component uses the [MessageContentTemplate](https://docs.devexpress.com/Blazor/DevExpress.AIIntegration.Blazor.Chat.DxAIChat.MessageContentTemplate) property to conditionally render a [bar chart](https://docs.devexpress.com/Blazor/DevExpress.Blazor.DxChart-1.-ctor) below the AI text response whenever chart data is available for that message:

```razor
<DxAIChat UseStreaming="true" ShowHeader="true" ResponseReceived="OnResponseReceived">
    <MessageContentTemplate>
        @context.Content
        @if (chartDataByMessage.TryGetValue(context, out var chartData))
        {
            <DxChart Data="@chartData" Width="100%" Height="300px"
                     CustomizeSeriesPoint="OnCustomizePoint">
                <DxChartBarSeries ArgumentField="@((ChartReportData d) => d.Label)"
                                  ValueField="@((ChartReportData d) => d.Value)"
                                  Name="Tickets" />
                <DxChartLegend Visible="false" />
            </DxChart>
        }
    </MessageContentTemplate>
</DxAIChat>
```

Bars are color-coded by feedback category using [CustomizeSeriesPoint](https://docs.devexpress.com/Blazor/DevExpress.Blazor.DxChartBase.CustomizeSeriesPoint)`: green for Positive, red for Negative, and orange for Neutral.

### Help Desk Data Service

[HelpDeskDataService.cs](CS/Services/HelpDeskDataService.cs) generates 100 randomized `HelpDeskTicket` records on startup (using a fixed seed for reproducibility) and exposes a `GetFeedbackSummary()` method that groups tickets by feedback type and returns a list of `ChartReportData` label/value pairs.

### Prompt Suggestions

[Index.razor](CS/Components/Pages/Index/Index.razor) includes two [prompt suggestions](https://docs.devexpress.com/Blazor/DevExpress.AIIntegration.Blazor.Chat.DxAIChatPromptSuggestion) to guide users toward the chart-generating queries:

- **Feedback Chart** — asks for full feedback breakdown across all categories.
- **Positive vs. Negative Chart** — filters the chart to compare Positive and Negative ratings.

## Files to Review

- [Program.cs](CS/Program.cs)
- [appsettings.json](CS/appsettings.json)
- [Components/Pages/Index/Index.razor](CS/Components/Pages/Index/Index.razor)
- [Components/Pages/Index/Index.razor.css](CS/Components/Pages/Index/Index.razor.css)
- [Services/HelpDeskAITools.cs](CS/Services/HelpDeskAITools.cs)
- [Services/HelpDeskDataService.cs](CS/Services/HelpDeskDataService.cs)
- [Services/AzureOpenAIServiceSettings.cs](CS/Services/AzureOpenAIServiceSettings.cs)
- [Models/HelpDeskTicket.cs](CS/Models/HelpDeskTicket.cs)
- [Models/ChartReportData.cs](CS/Models/ChartReportData.cs)

## Documentation

- [DevExpress AI-powered Extensions for Blazor](https://docs.devexpress.com/Blazor/405228/ai-powered-extensions)
- [DevExpress Blazor AI Chat Control](https://docs.devexpress.com/Blazor/DevExpress.AIIntegration.Blazor.Chat.DxAIChat)
- [DevExpress Blazor Chart](https://docs.devexpress.com/Blazor/DevExpress.Blazor.Charts.DxChart)
- [AIIntegrationTool Attribute](https://docs.devexpress.com/Blazor/405228/ai-powered-extensions)
- [Microsoft.Extensions.AI IChatClient](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.ai.ichatclient)

## Related Examples

- [DevExpress Blazor AI Chat — Implement Function/Tool Calling](https://github.com/DevExpress-Examples/blazor-ai-chat-function-calling)
- [Blazor AI Chat — Confirm Tool Calls](https://github.com/DevExpress-Examples/blazor-ai-chat-confirm-tool-calls)
