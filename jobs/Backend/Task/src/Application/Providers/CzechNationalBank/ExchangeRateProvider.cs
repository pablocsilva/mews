using Application.Providers.CzechNationalBank.Clients;
using ExchangeRateUpdater.Domain;
using ExchangeRateUpdater.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ExchangeRateUpdater.Application.Providers.CzechNationalBank;

public class CzezhNationalBankExchangeRateProvider : IExchangeRateProvider
{
    private static readonly Currency TargetCurrency = new("CZK");

    private readonly ICNBClient _cnbClient;
    private readonly IDailyExchangeRatesResponseParser _parser;
    private readonly ILogger<CzezhNationalBankExchangeRateProvider> _logger;

    public CzezhNationalBankExchangeRateProvider(
        ICNBClient cnbClient,
        IDailyExchangeRatesResponseParser parser,
        ILogger<CzezhNationalBankExchangeRateProvider> logger)
    {
        _cnbClient = cnbClient;
        _parser = parser;
        _logger = logger;
    }

    public async Task<IEnumerable<ExchangeRate>> GetExchangeRatesAsync(
        IEnumerable<Currency> currencies,
        CancellationToken cancellationToken = default)
    {
        var currencyList = currencies.ToList();
        _logger.LogInformation("Fetching exchange rates for {Count} currencies: {Currencies}",
            currencyList.Count,
            string.Join(", ", currencyList.Select(c => c.Code)));

        try
        {
            var rawData = await _cnbClient.GetDailyRatesAsync(cancellationToken);
            var data = _parser.Parse(rawData);

            var requestedCodes = new HashSet<string>(
                currencyList.Select(c => c.Code),
                StringComparer.OrdinalIgnoreCase
            );

            var exchangeRates = data
                .ExchangeRates
                .Where(model => requestedCodes.Contains(model.Code))
                .Select(DomainExchangeRate)
                .ToList();

            _logger.LogInformation("Successfully retrieved {Count} exchange rates out of {Requested} requested",
                exchangeRates.Count, currencyList.Count);

            LogNotFoundCurrencies(currencyList, exchangeRates);

            return exchangeRates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve exchange rates");
            throw;
        }
    }

    private void LogNotFoundCurrencies(List<Currency> currencyList, List<ExchangeRate> exchangeRates)
    {
        var foundCodes = new HashSet<string>(
            exchangeRates.Select(rate => rate.SourceCurrency.Code),
            StringComparer.OrdinalIgnoreCase
        );

        var missingCurrencies = currencyList
            .Where(rate => !foundCodes.Contains(rate.Code))
            .Select(rate => rate.Code)
            .ToList();

        if (missingCurrencies.Any())
        {
            _logger.LogWarning("Could not find exchange rates for currencies: {Currencies}",
                string.Join(", ", missingCurrencies)
            );
        }
    }

    private static ExchangeRate DomainExchangeRate(Models.ExchangeRate model) =>
        new(
            sourceCurrency: new Currency(model.Code),
            targetCurrency: TargetCurrency,
            value: model.Rate / model.Amount
        );
}