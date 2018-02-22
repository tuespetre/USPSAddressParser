using USPSAddressParser.Syntax;
using Xunit;

namespace USPSAddressParser.Tests
{
    public class TokenizerTests
    {
        [Fact]
        public void Tokenizes()
        {
            var input = "XX  11 1/2 / . , - ' ";

            var expected = new[]
            {
                new Token(Span.Between(00, 01), "XX", TokenKind.Alphabetic),
                new Token(Span.Between(02, 03), "  ", TokenKind.WhiteSpace),
                new Token(Span.Between(04, 05), "11", TokenKind.Numeric),
                new Token(Span.Between(06, 06), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(07, 09), "1/2", TokenKind.Fraction),
                new Token(Span.Between(10, 10), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(11, 11), "/", TokenKind.Unknown),
                new Token(Span.Between(12, 12), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(13, 13), ".", TokenKind.Period),
                new Token(Span.Between(14, 14), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(15, 15), ",", TokenKind.Comma),
                new Token(Span.Between(16, 16), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(17, 17), "-", TokenKind.Hyphen),
                new Token(Span.Between(18, 18), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(19, 19), "'", TokenKind.Apostrophe),
                new Token(Span.Between(20, 20), " ", TokenKind.WhiteSpace),
            };

            var actual = Tokenizer.Tokenize(input);

            Assert.Equal(expected, actual);
        }
    }
}
