using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ai_chat;

public interface IRecordAudio
{
    Task<string> StartRecording();
    Task<string> GetRecording(string file);
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
        Console.WriteLine("\nListening for 10 seconds...");

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-f pulse -i alsa_input.pci-0000_00_1f.3.analog-stereo -t 10 -y {filePath}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        process.Start();
        var  output = await process.StandardOutput.ReadToEndAsync();
        var errorOutput = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        Console.WriteLine("ffmpeg output:");
        Console.WriteLine(output);
        Console.WriteLine("ffmpeg errors:");
        Console.WriteLine(errorOutput);
        return File.Exists(filePath) ? filePath : "";
    }


    

    public Task<string> GetRecording(string file)
    {
        throw new NotImplementedException();
    }
}