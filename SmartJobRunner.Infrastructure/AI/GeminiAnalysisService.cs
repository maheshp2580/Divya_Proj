using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SmartJobRunner.Application.Interfaces;

namespace SmartJobRunner.Infrastructure.AI;

public class GeminiAnalysisService : IAiAnalysisService
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private readonly string? _apiKey;

    public GeminiAnalysisService(IConfiguration configuration)
    {
        _apiKey = configuration["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
    }

    public async Task<string> AnalyzeFailureAsync(string jobName, string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return "AI Analysis skipped: Gemini API key is missing. Please set GEMINI_API_KEY environment variable.";
        }

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";
        var prompt = $"Analyze the following background job failure. Job Name: {jobName}. Error Message: {errorMessage}. Provide a brief, one-paragraph root cause analysis and a suggested fix.";

        var payload = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return $"AI Analysis failed with status code: {response.StatusCode}. Details: {errorContent}";
            }

            var responseStr = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseStr);
            var root = doc.RootElement;
            
            var generatedText = root
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text").GetString();

            return generatedText ?? "No analysis returned from Gemini.";
        }
        catch (Exception ex)
        {
            return $"AI Analysis encountered an exception: {ex.Message}";
        }
    }
}
