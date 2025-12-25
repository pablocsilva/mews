using ExchangeRateUpdater.Application.Providers.CzechNationalBank.Models;

namespace ExchangeRateUpdater.Application.Providers.CzechNationalBank;

/// <summary>
/// Defines a contract for parsing CNB exchange rate data.
/// </summary>
public interface IDailyExchangeRatesResponseParser
{
    /// <summary>
    /// Parses raw CNB data into structured format.
    /// </summary>
    /// <param name="rawData">Raw text data from CNB.</param>
    /// <returns>Parsed exchange rate data.</returns>
    /// <exception cref="CnbParsingException">Thrown when data cannot be parsed.</exception>
    DailyExchangeRatesResponse Parse(string rawData);
}