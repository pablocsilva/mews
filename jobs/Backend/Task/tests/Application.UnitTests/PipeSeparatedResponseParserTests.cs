using ExchangeRateUpdater.Providers.CzechNationalBank;

namespace ExchangeRateUpdater.UnitTests;

public class PipeSeparatedResponseParserTests
{
    private readonly PipeSeparatedResponseParser _sut = new();
    private readonly DateOnly _testDate = new(2025, 12, 23);
    private const int _testSequence = 248;
    private const string _validHeader = "23 Dec 2025 #248";
    private const string _validColumns = "Country|Currency|Amount|Code|Rate";

    private static string RawData(params string[] parts)
        => string.Join(Environment.NewLine, parts);

    [Fact]
    public void Parse_ShouldThrowCnbParsingException_WhenRawDataIsNull()
    {
        var exception = Assert.Throws<CnbParsingException>(() => _sut.Parse(null));

        Assert.Equal("Raw data is empty, null or white space.", exception.Message);
    }

    [Theory]
    [InlineData("")] // Empty string
    [InlineData("   ")] // Whitespace only
    [InlineData("\t")] // Tab character
    [InlineData("\n")] // Newline character
    [InlineData("\r\n")] // Carriage return and newline
    public void Parse_ShouldThrowCnbParsingException_WhenRawDataIsWhitespace(string input)
    {
        var exception = Assert.Throws<CnbParsingException>(() => _sut.Parse(input));

        Assert.Equal("Raw data is empty, null or white space.", exception.Message);
    }

    [Theory]
    [InlineData("Anything without hash symbol")] // No hash symbol
    [InlineData("Multiple##hash##symbols")] // More than one hash symbol
    [InlineData("##StartsWithHash")] // Hash at the beginning
    public void Parse_ShouldThrowCnbParsingException_WhenHeaderIsInvalid(string input)
    {
        var exception = Assert.Throws<CnbParsingException>(() => _sut.Parse(input));

        Assert.Equal("Header is not in expected format.", exception.Message);
    }

    [Theory]
    [MemberData(nameof(GetInvalidColumnFormats))]
    public void Parse_ShouldThrowCnbParsingException_WhenColumnNamesAreInvalid(string input)
    {
        var exception = Assert.Throws<CnbParsingException>(() => _sut.Parse(input));

        Assert.Equal("Column names are not in expected format.", exception.Message);
    }

    public static IEnumerable<object[]> GetInvalidColumnFormats()
    {
        // Wrong column names
        yield return new object[] { RawData(_validHeader, "WrongColumn1|WrongColumn2|WrongColumn3|WrongColumn4|WrongColumn5") };
        // Comma separator instead of pipe
        yield return new object[] { RawData(_validHeader, "Country,Currency,Amount,Code,Rate") };
        // Too many columns
        yield return new object[] { RawData(_validHeader, "Country|Currency|Amount|Code|Rate|ExtraColumn") };
        // Too few columns
        yield return new object[] { RawData(_validHeader, "Country|Currency|Amount") };
        // Missing Rate column
        yield return new object[] { RawData(_validHeader, "Country|Currency|Amount|Code") };
        // Lowercase column names
        yield return new object[] { RawData(_validHeader, "country|currency|amount|code|rate") };
    }

    [Fact]
    public void Parse_ShouldReturnExchangeRates_WhenDataIsValid()
    {
        var validData = RawData(
            _validHeader,
            _validColumns,
            "Australia|dollar|1|AUD|13.818",
            "Brazil|real|1|BRL|3.694",
            "Canada|dollar|1|CAD|15.064"
        );

        var actual = _sut.Parse(validData);

        Assert.NotNull(actual);
        Assert.Equal(_testDate, actual.Date);
        Assert.Equal(_testSequence, actual.Sequence);

        Assert.Equal(3, actual.Records.Count);

        var firstRecord = actual.Records[0];
        Assert.Equal("Australia", firstRecord.Country);
        Assert.Equal("dollar", firstRecord.CurrencyName);
        Assert.Equal(1, firstRecord.Amount);
        Assert.Equal("AUD", firstRecord.Code);
        Assert.Equal(13.818m, firstRecord.Rate);

        var secondRecord = actual.Records[1];
        Assert.Equal("Brazil", secondRecord.Country);
        Assert.Equal("real", secondRecord.CurrencyName);
        Assert.Equal(1, secondRecord.Amount);
        Assert.Equal("BRL", secondRecord.Code);
        Assert.Equal(3.694m, secondRecord.Rate);

        var thirdRecord = actual.Records[2];
        Assert.Equal("Canada", thirdRecord.Country);
        Assert.Equal("dollar", thirdRecord.CurrencyName);
        Assert.Equal(1, thirdRecord.Amount);
        Assert.Equal("CAD", thirdRecord.Code);
        Assert.Equal(15.064m, thirdRecord.Rate);
    }

    [Fact]
    public void Parse_ShouldReturnEmptyCollection_WhenOnlyHeaderAndColumnsProvided()
    {
        var dataWithoutRates = RawData(_validHeader, _validColumns);

        var actual = _sut.Parse(dataWithoutRates);

        Assert.NotNull(actual);
        Assert.Equal(_testDate, actual.Date);
        Assert.Equal(_testSequence, actual.Sequence);
        Assert.Empty(actual.Records);
    }


    [Fact]
    public void Parse_ShouldIgnoreExtraWhitespace_WhenDataHasTrailingNewlines()
    {
        var dataWithExtraWhitespace = RawData(
            _validHeader,
            _validColumns,
            "Australia|dollar|1|AUD|13.818",
            "",
            ""
        );

        var actual = _sut.Parse(dataWithExtraWhitespace);

        Assert.NotNull(actual);
        Assert.Equal(_testDate, actual.Date);
        Assert.Equal(_testSequence, actual.Sequence);
        Assert.Single(actual.Records);
    }
}