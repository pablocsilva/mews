namespace ExchangeRateUpdater.Providers.CzechNationalBank;

using ExchangeRateUpdater.Providers.CzechNationalBank.Models;

/// <summary>
/// Defines a contract for parsing CNB exchange rate data.
/// </summary>
public interface IResponseParser
{
    /// <summary>
    /// Parses raw CNB data into structured format.
    /// </summary>
    /// <param name="rawData">Raw text data from CNB.</param>
    /// <returns>Parsed exchange rate data.</returns>
    /// <exception cref="CnbParsingException">Thrown when data cannot be parsed.</exception>
    DailyResponse Parse(string rawData);
}