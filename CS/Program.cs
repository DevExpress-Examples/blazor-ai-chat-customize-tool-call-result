using Azure;
using Azure.AI.OpenAI;
using DxAiChatCustomToolCallingChartReport.Components;
using DxAiChatCustomToolCallingChartReport.Services;
using Microsoft.Extensions.AI;
using DevExpress.AIIntegration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
  .AddInteractiveServerComponents();

builder.Services.AddDevExpressBlazor(options =>
{
    options.SizeMode = DevExpress.Blazor.SizeMode.Medium;
});
builder.Services.AddMvc();

builder.Services.AddSingleton<HelpDeskDataService>();
builder.Services.AddScoped<HelpDeskAITools>();

var openAiServiceSettings = builder.Configuration.GetSection("AzureOpenAISettings").Get<AzureOpenAIServiceSettings>();
if (openAiServiceSettings == null ||
    string.IsNullOrEmpty(openAiServiceSettings.Endpoint) ||
    string.IsNullOrEmpty(openAiServiceSettings.Key) ||
    string.IsNullOrEmpty(openAiServiceSettings.DeploymentName))
    throw new InvalidOperationException("Specify the Azure OpenAI endpoint, key, and deployment name in the 'appsettings.json' file.");

var azureOpenAIClient = new AzureOpenAIClient(
     new Uri(openAiServiceSettings.Endpoint),
     new AzureKeyCredential(openAiServiceSettings.Key));

var chatClient = azureOpenAIClient
    .GetChatClient(openAiServiceSettings.DeploymentName)
    .AsIChatClient();

builder.Services.AddScoped<IChatClient>((sp) =>
{
    return chatClient.AsBuilder()
        .UseDXTools()
        .UseFunctionInvocation()
        .Build(sp);
});

builder.Services.AddDevExpressAI();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AllowAnonymous();

app.Run();