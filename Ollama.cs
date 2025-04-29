using System.Text;
using System.Text.Json;
using ai_chat.Settings;
using Microsoft.Extensions.Options;

namespace ai_chat;

public interface IOllama
{
    Task<string> AskAsync(string question);
}
public class Ollama(HttpClient httpClient, IOptions<OllamaSettings> ollamaSettings) : IOllama
{
    private readonly OllamaSettings _ollamaSettings = ollamaSettings.Value;

    public async Task<string> AskAsync(string prompt)
    {
        try
        {
            var requestBody = new
            {
                _ollamaSettings.Model,
                messages = new[]
                {
                    new { role = "system", content = "Eres un asistente muy sarcastico" },
                    new { role = "user", content = prompt }
                },
                stream = false
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(_ollamaSettings.ApiUrl, content);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            var result = doc.RootElement.GetProperty("message").GetProperty("content").GetString()??"la propiedad content  no ha sido encontrada";
            return result;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error en la solicitud HTTP: {ex.Message}");
            return "La solicitud a ollama no ha sido exitosa";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inesperado: {ex.Message}");
            return "Ocurrió un error inesperado.";
        }
    }
}