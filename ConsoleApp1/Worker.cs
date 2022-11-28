namespace ConsoleApp1;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHost _host;

    public Worker(ILogger<Worker> logger, IHost host)
    {
        _logger = logger;
        _host = host;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Working..");
            await Task.Delay(1000, stoppingToken);
        }

        //_host.StopAsync();
    }
}