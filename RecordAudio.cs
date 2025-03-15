using System.Diagnostics;
using System.Runtime.InteropServices;
using Deepgram.Models.Agent.v2.WebSocket;

namespace ai_chat;

public interface IRecordAudio
{
    Task<string> StartRecording();
}
public class RecordAudio: IRecordAudio
{
  public async Task<string> StartRecording()
    {
        const string fileName = "input.wav";
        var contentDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Content");
        var filePath = Path.Combine(contentDirectory, fileName);
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        Directory.CreateDirectory(contentDirectory);
        Console.WriteLine("Press a key to start listening...");
        Console.ReadKey();
        Console.WriteLine("\nListening for 5 seconds...");

        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-f pulse -i alsa_input.pci-0000_00_1f.3.analog-stereo -t 5 -y {filePath}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();
            _= await process.StandardOutput.ReadToEndAsync();
            _= await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            return File.Exists(filePath) ? filePath : "";
        }catch (Exception)
        {
            return "";
        }
    }
}