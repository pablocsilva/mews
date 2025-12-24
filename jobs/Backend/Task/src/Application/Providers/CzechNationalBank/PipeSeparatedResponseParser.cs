using System.Globalization;
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
    private static readonly char[] _newLineCharacters = ['\r', '\n'];
    private const string _expectedHeaderColumns = "Country|Currency|Amount|Code|Rate";
    public DailyResponse Parse(string rawData)
    {
        if (string.IsNullOrWhiteSpace(rawData))
        {
            throw new CnbParsingException("Raw data is empty, null or white space.");
        }

        var contents = rawData.Split(_newLineCharacters, StringSplitOptions.RemoveEmptyEntries);
        if (contents.Length < 3)
        {
            throw new CnbParsingException("Response does not contain enough lines.");
        }

        var header = contents[0];
        var headerParts = GetHeaderParts(header);
        var exchangeDate = GetExchangeDate(headerParts);
        var exchangeSequence = GetExchangeSequence(headerParts);

        ValidateColumnNames(contents[1]);

        return new DailyResponse
        (
            Date: exchangeDate,
            Sequence: exchangeSequence,
            Records: contents[2..]
                .Select(ParseRecord)
                .ToArray()
        );
    }
    private static DailyRecord ParseRecord(string line)
    {
        var parts = line.Split('|', StringSplitOptions.TrimEntries);

        if (parts.Length != 5)
        {
            throw new CnbParsingException($"Invalid record format: '{line}'");
        }

        if (!int.TryParse(parts[2], out var amount))
        {
            throw new CnbParsingException($"Invalid amount: '{parts[2]}'");
        }

        if (!decimal.TryParse(parts[4], NumberStyles.Number, CultureInfo.InvariantCulture, out var rate))
        {
            throw new CnbParsingException($"Invalid rate: '{parts[4]}'");
        }

        return new DailyRecord(
            Country: parts[0],
            CurrencyName: parts[1],
            Amount: amount,
            Code: parts[3],
            Rate: rate
        );
    }
    private static int GetExchangeSequence(string[] headerParts)
    {
        if (!int.TryParse(headerParts[1], out var sequence))
        {
            throw new CnbParsingException("Header sequence is not in expected format.");
        }

        return sequence;
    }

    private static DateOnly GetExchangeDate(string[] headerParts)
    {
        if (!DateOnly.TryParse(headerParts[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            throw new CnbParsingException("Header date is not in expected format.");
        }

        return date;
    }

    private static string[] GetHeaderParts(string header)
    {
        var headerParts = header.Split('#', StringSplitOptions.TrimEntries);

        if (headerParts.Length != 2)
        {
            throw new CnbParsingException("Header is not in expected format.");
        }

        return headerParts;
    }

    private static void ValidateColumnNames(string columnNames)
    {
        if (!string.Equals(columnNames.Trim(), _expectedHeaderColumns, StringComparison.OrdinalIgnoreCase))
        {
            throw new CnbParsingException("Column names are not in expected format.");
        }
    }
}


public class CnbParsingException(string message) : Exception(message) { }