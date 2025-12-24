
using System;
using ExchangeRateUpdater.Providers.CzechNationalBank.Models;

namespace ExchangeRateUpdater.Providers.CzechNationalBank;

/// <summary>
/// Parses exchange rate data returned by the Czech National Bank daily rates endpoint.
/// </summary>
/// <remarks>
/// Expected format (simplified):
/// 
/// 23 Dec 2025 #248
/// Country|Currency|Amount|Code|Rate
// Australia|dollar|1|AUD|13.818
// Brazil|real|1|BRL|3.694
/// 
/// Source:
/// https://www.cnb.cz/en/financial-markets/foreign-exchange-market/central-bank-exchange-rate-fixing/central-bank-exchange-rate-fixing/daily.txt
/// </remarks>
public class PipeSeparatedResponseParser : IResponseParser
{
    public DailyResponse Parse(string rawData)
    {
        throw new NotImplementedException();
    }
}