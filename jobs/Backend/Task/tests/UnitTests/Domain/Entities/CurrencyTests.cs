using ExchangeRateUpdater.Domain.Entities;

namespace ExchangeRateUpdater.UnitTests;

public class CurrencyTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenCodeIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new Currency(string.Empty));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCodeIsWhitespace()
    {
        Assert.Throws<ArgumentException>(() => new Currency("   "));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCodeIsNotThreeCharacters()
    {
        Assert.Throws<ArgumentException>(() => new Currency("US"));
        Assert.Throws<ArgumentException>(() => new Currency("USDA"));
    }

    [Fact]
    public void Constructor_ShouldSetCode_ToUpperInvariant()
    {
        var currency = new Currency("usd");
        Assert.Equal("USD", currency.Code);
    }
}