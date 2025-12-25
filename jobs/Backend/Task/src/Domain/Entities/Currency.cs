namespace ExchangeRateUpdater.Domain.Entities;

public class Currency
{
    public string Code { get; }

    public Currency(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Currency code cannot be empty", nameof(code));
        }

        if (code.Length != 3)
        {
            throw new ArgumentException("Currency code must be 3 characters", nameof(code));
        }

        Code = code.ToUpperInvariant();
    }

    public override string ToString() => Code;
}