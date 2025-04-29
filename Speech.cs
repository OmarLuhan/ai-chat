using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ai_chat.Settings;
using Deepgram;
using Deepgram.Logger;
using Deepgram.Models.Listen.v1.REST;
using Microsoft.Extensions.Options;

namespace ai_chat;

public interface ISpeech
{
    Task<string>SpeechToTextAsync(string filePath);
    Task<string> TextToSpeechAsync(string text);
    Task SpeakAsync(string text);
}
public class Speech(HttpClient httpClient,IOptions<ElevenLabsSettings> elevenLabsOptions,IOptions<DeepGramSettings>deepGramOptions):ISpeech
{
    private readonly ElevenLabsSettings _elevenLabsSettings=elevenLabsOptions.Value;
    private readonly DeepGramSettings _deepGramSettings=deepGramOptions.Value;
   

    public async Task<string> SpeechToTextAsync(string filePath)
    {
        try
        {
            Library.Initialize(LogLevel.Error);
            var deepGramClient = ClientFactory.CreateListenRESTClient(_deepGramSettings.ApiKey);
            var audio=await File.ReadAllBytesAsync(filePath);
            var response = await deepGramClient.TranscribeFile(audio, new PreRecordedSchema()
            {
                Model = "whisper", 
                Language = "es"
            });
            Library.Terminate();
            return response.Results?.Channels?[0].Alternatives?[0].Transcript ?? "responde con: 'no se ha encontrado la transcripcion'";
        }catch (Exception)
        {
            return "Error al transcribir el audio";
        }
    }
    public async Task<string> TextToSpeechAsync(string text)
    {
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Content");
        Directory.CreateDirectory(folderPath);
        var filePath = Path.Combine(folderPath, "output.mp3");
        try
        {
            var requestBody = new
            {
                text,
                model_id = _elevenLabsSettings.ModelId,
                voice_settings = new { stability = 0.5, similarity_boost = 0.5 }
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, _elevenLabsSettings.ApiUrl);
            request.Headers.Add("xi-api-key", _elevenLabsSettings.ApiKey);
            request.Content = content;
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var audioData=await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(filePath, audioData);
            return filePath;
        }catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error en la solicitud HTTP: {ex.Message}");
            return "";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inesperado: {ex.Message}");
            return "";
        }
    }
    public async Task SpeakAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("El archivo de audio no existe.");
            return;
        }

        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "play",
                Arguments = $"\"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();
            Console.WriteLine("Reproduciendo audio...");
            await process.WaitForExitAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al reproducir el audio: {ex.Message}");
        }
    }
}