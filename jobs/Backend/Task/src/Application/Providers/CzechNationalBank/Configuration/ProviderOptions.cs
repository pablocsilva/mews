using System;

namespace ExchangeRateUpdater.Providers.CzechNationalBank.Configuration;

/// <summary>
/// Configuration options for the Czech National Bank exchange rate provider.
/// </summary>
public class ProviderOptions
{
    public const string SectionName = "CnbProvider";
    public string BaseUrl { get; set; } = "https://www.cnb.cz";
    public string DailyRatesPath { get; set; } = "/en/financial-markets/foreign-exchange-market/central-bank-exchange-rate-fixing/central-bank-exchange-rate-fixing/daily.txt";
    public int TimeoutSeconds { get; set; } = 10;
    public int RetryCount { get; set; } = 3;
    public string UserAgent { get; set; } = "ExchangeRateUpdater/1.0";

    public string FullUrl => $"{BaseUrl.TrimEnd('/')}{DailyRatesPath}";
}