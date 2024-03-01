using System.IO.Pipes;
using System.IO;
using Microsoft.Extensions.Hosting;

namespace StudyHub.WPF.Services;

public class NamedPipeManagerService : BackgroundService {
    private const string PipeName = "c2bd2bfd-7f52-485f-bddd-f6c63e17d008";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        await Task.CompletedTask;
        await HandleNamedPipeServerStreamAsync(stoppingToken);
    }

    private static async Task HandleNamedPipeServerStreamAsync(CancellationToken stoppingToken) {
        while (stoppingToken.IsCancellationRequested is false) {
            using var stream = new NamedPipeServerStream(PipeName, PipeDirection.In, 10, PipeTransmissionMode.Message, PipeOptions.None);
            await stream.WaitForConnectionAsync(stoppingToken);
            using var reader = new StreamReader(stream);
            var message = reader.ReadLine();
            HandleMessage(message);
        }
    }

    private static void HandleMessage(string? message) {
        if (string.IsNullOrWhiteSpace(message)) return;
        if (message == "Activate") App.ActivateMainWindow();
    }

    public static void NotifyActivateMainWindow() {
        try {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(500);
            using var writer = new StreamWriter(client);
            writer.WriteLine("Activate");
            writer.Flush();
        }
        catch (TimeoutException) {
        }
    }
}
