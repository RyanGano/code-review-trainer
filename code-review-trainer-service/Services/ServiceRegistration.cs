using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.Options;

namespace code_review_trainer_service.Services;

public static class ServiceRegistration
{
  public static IServiceCollection AddCodeReviewServices(this IServiceCollection services, IConfiguration config)
  {

    services.Configure<AzureOpenAISettings>(config.GetSection("AzureOpenAI"));

    // Register the underlying AzureOpenAIClient once using bound options (avoids repeated config lookups)
    services.AddSingleton<AzureOpenAIClient>(sp =>
    {
      var aiSettings = sp.GetRequiredService<IOptions<AzureOpenAISettings>>().Value;
      if (string.IsNullOrWhiteSpace(aiSettings.Endpoint)) throw new InvalidOperationException("AzureOpenAI:Endpoint missing");
      if (string.IsNullOrWhiteSpace(aiSettings.ApiKey)) throw new InvalidOperationException("AzureOpenAI:ApiKey missing (use user-secrets)");
      return new AzureOpenAIClient(new Uri(aiSettings.Endpoint), new AzureKeyCredential(aiSettings.ApiKey));
    });

    // Create ChatClient per deployment
    services.AddSingleton<ChatClient>(sp =>
    {
      var aiSettings = sp.GetRequiredService<IOptions<AzureOpenAISettings>>().Value;
      if (string.IsNullOrWhiteSpace(aiSettings.DeploymentName)) throw new InvalidOperationException("AzureOpenAI:DeploymentName missing");
      var azureClient = sp.GetRequiredService<AzureOpenAIClient>();
      return azureClient.GetChatClient(aiSettings.DeploymentName);
    });

    services.AddSingleton<AzureOpenAICodeReviewModel>();
    services.AddSingleton<ICodeReviewModel>(sp => sp.GetRequiredService<AzureOpenAICodeReviewModel>());
    services.AddSingleton<IProblemRepository, ProblemRepository>();
    return services;
  }
}
