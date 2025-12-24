using ExchangeRateUpdater.Providers.CzechNationalBank;

namespace ExchangeRateUpdater.UnitTests;

public class PipeSeparatedResponseParserTests
{
    private readonly PipeSeparatedResponseParser _sut = new();
    private readonly DateOnly _testDate = new(2025, 12, 23);
    private const int _testSequence = 248;
    private const string _validHeader = "23 Dec 2025 #248";
    private const string _validColumnNames = "Country|Currency|Amount|Code|Rate";

    private static string RawData(params string[] parts)
        => string.Join(Environment.NewLine, parts);

    [Fact]
    public void Parse_ShouldThrowCnbParsingException_WhenRawDataIsNull()
    {
        AssertThrowsWithMessage(() => _sut.Parse(null), "Raw data is empty, null or white space.");
    }

    [Theory]
    [InlineData("")] // Empty string
    [InlineData("   ")] // Whitespace only
    [InlineData("\t")] // Tab character
    [InlineData("\n")] // Newline character
    [InlineData("\r\n")] // Carriage return and newline
    public void Parse_ShouldThrowCnbParsingException_WhenRawDataIsEmptyOrWhitespace(string input)
    {
        AssertThrowsWithMessage(() => _sut.Parse(input), "Raw data is empty, null or white space.");
    }

    [Theory]
    [InlineData("invalidheader\n anything \n anything")] // No hash symbolon header
    [InlineData("Multiple##hash##symbols\n anything \n anything")] // More than one hash symbol
    [InlineData("##StartsWithHash\n anything \n anything")] // Hash at the beginning
    public void Parse_ShouldThrowCnbParsingException_WhenHeaderIsInvalid(string input)
    {
        AssertThrowsWithMessage(() => _sut.Parse(input), "Header is not in expected format.");
    }


    public static IEnumerable<object[]> GetInvalidColumnFormats()
    {
        // Wrong column names
        yield return new object[] { RawData(_validHeader, "WrongColumn1|WrongColumn2|WrongColumn3|WrongColumn4|WrongColumn5", "anything") };
        // Comma separator instead of pipe
        yield return new object[] { RawData(_validHeader, "Country,Currency,Amount,Code,Rate", "anything") };
        // Too many columns
        yield return new object[] { RawData(_validHeader, "Country|Currency|Amount|Code|Rate|ExtraColumn", "anything") };
        // Too few columns
        yield return new object[] { RawData(_validHeader, "Country|Currency|Amount", "anything") };
        // Missing Rate column
        yield return new object[] { RawData(_validHeader, "Country|Currency|Amount|Code", "anything") };
    }

    [Theory]
    [MemberData(nameof(GetInvalidColumnFormats))]
    public void Parse_ShouldThrowCnbParsingException_WhenColumnNamesAreInvalid(string input)
    {
        AssertThrowsWithMessage(() => _sut.Parse(input), "Column names are not in expected format.");
    }

    [Fact]
    public void Parse_ShouldThrowCnbParsingException_WhenRecordIsMalformed()
    {
        var malformedData = RawData(
            _validHeader,
            _validColumnNames,
            "||" // missing columns
        );

        AssertThrowsWithMessage(() => _sut.Parse(malformedData), "Invalid record format");
    }

    [Fact]
    public void Parse_ShouldThrow_WhenAmountIsNotNumeric()
    {
        var data = RawData(
            _validHeader,
            _validColumnNames,
            "Australia|dollar|X|AUD|13.818"
        );

        AssertThrowsWithMessage(() => _sut.Parse(data), "Invalid amount");
    }

    [Fact]
    public void Parse_ShouldThrow_WhenRateIsNotNumeric()
    {
        var data = RawData(
            _validHeader,
            _validColumnNames,
            "Australia|dollar|1|AUD|abc"
        );

        AssertThrowsWithMessage(() => _sut.Parse(data), "Invalid rate");
    }

    [Fact]
    public void Parse_ShouldThrow_WhenHeaderDateIsInvalid()
    {
        var data = RawData(
            "99 Dec 99999 #248", // wrong format
            _validColumnNames,
            "Australia|dollar|1|AUD|13.818"
        );

        AssertThrowsWithMessage(() => _sut.Parse(data), "Header date is not in expected format.");
    }

    [Fact]
    public void Parse_ShouldThrow_WhenHeaderSequenceIsInvalid()
    {
        var data = RawData(
            "23 Dec 2025 #ABC",
            _validColumnNames,
            "Australia|dollar|1|AUD|13.818"
        );

        AssertThrowsWithMessage(() => _sut.Parse(data), "Header sequence is not in expected format.");
    }

    [Fact]
    public void Parse_ShouldThrow_WhenNoRecordsAreProvided()
    {
        var data = RawData(_validHeader, _validColumnNames);

        AssertThrowsWithMessage(() => _sut.Parse(data), "Response does not contain enough lines.");
    }

    [Fact]
    public void Parse_ShouldReturnExchangeRates_WhenDataIsValid()
    {
        var validData = RawData(
            _validHeader,
            _validColumnNames,
            "Australia|dollar|1|AUD|13.818",
            "Brazil|real|1|BRL|3.694",
            "Canada|dollar|1|CAD|15.064"
        );

        var actual = _sut.Parse(validData);

        Assert.NotNull(actual);
        Assert.Equal(_testDate, actual.Date);
        Assert.Equal(_testSequence, actual.Sequence);

        Assert.Equal(3, actual.Records.Count);

        Assert.Collection(actual.Records,
            record =>
            {
                Assert.Equal("Australia", record.Country);
                Assert.Equal("dollar", record.CurrencyName);
                Assert.Equal(1, record.Amount);
                Assert.Equal("AUD", record.Code);
                Assert.Equal(13.818m, record.Rate);
            },
            record =>
            {
                Assert.Equal("Brazil", record.Country);
                Assert.Equal("real", record.CurrencyName);
                Assert.Equal(1, record.Amount);
                Assert.Equal("BRL", record.Code);
                Assert.Equal(3.694m, record.Rate);
            },
            record =>
            {
                Assert.Equal("Canada", record.Country);
                Assert.Equal("dollar", record.CurrencyName);
                Assert.Equal(1, record.Amount);
                Assert.Equal("CAD", record.Code);
                Assert.Equal(15.064m, record.Rate);
            }
        );
    }

    [Fact]
    public void Parse_ShouldIgnoreExtraWhitespace_WhenDataHasTrailingNewlines()
    {
        var dataWithExtraWhitespace = RawData(
            _validHeader,
            _validColumnNames,
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

    private static void AssertThrowsWithMessage(Action act, string expectedMessage)
    {
        var ex = Assert.Throws<CnbParsingException>(act);
        Assert.Contains(expectedMessage, ex.Message);
    }
}