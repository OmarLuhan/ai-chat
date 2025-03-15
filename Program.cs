using ai_chat;
using ai_chat.Dependencies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();
var services = new ServiceCollection();
services.AddDependencies(configuration);
var serviceProvider = services.BuildServiceProvider();
var ollama=serviceProvider.GetRequiredService<IOllama>();
var speaker = serviceProvider.GetRequiredService<ISpeech>();
var recordAudio= serviceProvider.GetRequiredService<IRecordAudio>();
while (true)
{
    var file =await recordAudio.StartRecording();
    if(string.IsNullOrEmpty(file)) continue;
    var question = await speaker.SpeechToTextAsync(file);
    Console.WriteLine(question);
    var response=await ollama.AskAsync(question);
    Console.WriteLine(response);
    var filePath=await speaker.TextToSpeechAsync(response);
    if(string.IsNullOrEmpty(filePath)) continue;
    await speaker.SpeakAsync(filePath);
}
