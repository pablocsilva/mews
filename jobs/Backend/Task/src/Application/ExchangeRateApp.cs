using ExchangeRateUpdater.Domain.Entities;
using ExchangeRateUpdater.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ExchangeRateUpdater.Application;

public class ExchangeRateApp
{
    private readonly IExchangeRateProvider _provider;
    private readonly ILogger<ExchangeRateApp> _logger;

    public ExchangeRateApp(
        IExchangeRateProvider provider,
        ILogger<ExchangeRateApp> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public async Task RunAsync(IEnumerable<Currency> currencies)
    {
        try
        {
            var rates = await _provider.GetExchangeRatesAsync(currencies);

            Console.WriteLine($"Successfully retrieved {rates.Count()} exchange rates:");

            foreach (var rate in rates)
            {
                Console.WriteLine(rate);
            }

            _logger.LogInformation(
                "Application completed successfully. Retrieved {Count} exchange rates",
                rates.Count()
            );
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while retrieving exchange rates");
            Console.WriteLine("Unable to retrieve exchange rates due to network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving exchange rates");
            Console.WriteLine("Unable to retrieve exchange rates at this time. Please try again later.");
        }
    }
}