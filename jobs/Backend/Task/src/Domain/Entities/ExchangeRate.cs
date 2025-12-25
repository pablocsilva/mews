namespace ExchangeRateUpdater.Domain.Entities;

public record ExchangeRate
{
    public Currency SourceCurrency { get; }

    public Currency TargetCurrency { get; }

    public decimal Value { get; }

    public ExchangeRate(Currency sourceCurrency, Currency targetCurrency, decimal value)
    {
        SourceCurrency = sourceCurrency ?? throw new ArgumentNullException(nameof(sourceCurrency));
        TargetCurrency = targetCurrency ?? throw new ArgumentNullException(nameof(targetCurrency));

        if (value <= 0)
        {
            throw new ArgumentException("Exchange rate value must be positive", nameof(value));
        }

        Value = value;
    }

    public override string ToString()
    {
        return $"{SourceCurrency}/{TargetCurrency} = {Value}";
    }
}
