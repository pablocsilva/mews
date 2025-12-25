namespace ExchangeRateUpdater.Domain.Entities;

public record ExchangeRate
{
    public Currency SourceCurrency { get; }

    public Currency TargetCurrency { get; }

    public decimal Value { get; }

    public ExchangeRate(Currency sourceCurrency, Currency targetCurrency, decimal value)
    {
        ArgumentNullException.ThrowIfNull(sourceCurrency, nameof(sourceCurrency));
        ArgumentNullException.ThrowIfNull(targetCurrency, nameof(targetCurrency));
        if (value <= 0)
        {
            throw new ArgumentException("Exchange rate value must be positive", nameof(value));
        }

        SourceCurrency = sourceCurrency;
        TargetCurrency = targetCurrency;
        Value = value;
    }

    public override string ToString() =>
        $"{SourceCurrency}/{TargetCurrency} = {Value}";

}
