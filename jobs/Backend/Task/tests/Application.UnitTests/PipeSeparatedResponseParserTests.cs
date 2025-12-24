using ExchangeRateUpdater.Providers.CzechNationalBank;

namespace ExchangeRateUpdater.UnitTests;

public class PipeSeparatedResponseParserTests
{
    private readonly PipeSeparatedResponseParser _sut = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_NullRawData_ShouldThrowException(string input)
    {
        var actual = Assert.Throws<CnbParsingException>(() => _sut.Parse(input));

        Assert.Equal("Raw data is empty, null or white space.", actual.Message);
    }

    [Theory]
    [InlineData("Anything without hash symbol")]
    [InlineData("Anything with more than 1 hash symbol ##")]
    public void Parse_HeaderContainsOtherThanTwoPartsSparatedByHash_ShouldThrowException(string input)
    {
        var actual = Assert.Throws<CnbParsingException>(() => _sut.Parse(input));

        Assert.Equal("Header is not in expected format.", actual.Message);
    }

    [Theory]
    [MemberData(nameof(ValidHeaderButInvalidColumns))]
    public void Parse_UnexpectedColumnNames_ShouldThrowException(string input)
    {
        var actual = Assert.Throws<CnbParsingException>(() => _sut.Parse(input));

        Assert.Equal("Column names are not in expected format.", actual.Message);
    }

    public static string ValidHeader => "23 Dec 2025 #248";
    public static string ValidColumns => "Country|Currency|Amount|Code|Rate";

    public static IEnumerable<object[]> ValidHeaderButInvalidColumns()
    {
        yield return new object[] { RawData(ValidHeader, "WrongColumn1|WrongColumn2") };
        yield return new object[] { RawData(ValidHeader, "Country,Currency,Amount,Code,Rate") };
        yield return new object[] { RawData(ValidHeader, "Country|Currency|Amount|Code|Rate|ExtraColumn") };
    }

    private static string RawData(params string[] parts)
        => string.Join(Environment.NewLine, parts);
}
