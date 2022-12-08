using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> logger;
    private readonly IConfiguration configuration;
    private readonly IOptions<MyOptions> myOptions;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration, IOptions<MyOptions> myOptions)
    {
        this.logger = logger;
        this.configuration = configuration;
        this.myOptions = myOptions;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        logger.LogInformation("Returning forecast");

        logger.LogInformation("Config: {Config}.", myOptions);
        
        return Enumerable.Range(1, 5)
            .Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }
}