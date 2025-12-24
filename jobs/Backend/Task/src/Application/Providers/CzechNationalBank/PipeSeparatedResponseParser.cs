
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
        // read header split by #
        // read column names for validation
        // read lines split by |

        if (string.IsNullOrWhiteSpace(rawData))
        {
            throw new CnbParsingException("Raw data is empty, null or white space.");
        }

        var contents = rawData.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        var headerParts = contents[0].Split('#', StringSplitOptions.TrimEntries);
        if (headerParts.Length != 2)
        {
            throw new CnbParsingException("Header is not in expected format.");
        }

        var columnNames = contents[1];
        if (columnNames != "Country|Currency|Amount|Code|Rate")
        {
            throw new CnbParsingException("Column names are not in expected format.");
        }


        return null;
    }
}


public class CnbParsingException : Exception
{
    public CnbParsingException(string message)
        : base(message)
    {
    }
}