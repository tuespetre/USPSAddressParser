using System.Linq;
using USPSAddressParser.Syntax;
using Xunit;

namespace USPSAddressParser.Tests
{
    public partial class ParserTests
    {
        [Fact]
        public void TryParseRange_Tokens_Include_Trailing_WhiteSpace()
        {
            const string input = "5 ";

            var success = Parser.TryParseRange(input, out var syntax);

            Assert.True(success);
            Assert.NotNull(syntax);
            Assert.Equal(input, string.Concat(syntax.Tokens.Select(t => t.Value)));

            var tokens = new[]
            {
                new Token(Span.Between(00, 00), "5", TokenKind.Numeric),
                new Token(Span.Between(01, 01), " ", TokenKind.WhiteSpace),
            };

            Assert.Equal(tokens.Take(1), syntax.Components);
            Assert.Equal(Token.None, syntax.Fraction);
            Assert.Equal(tokens, syntax.Tokens);
        }

        [Fact]
        public void TryParseRange_Fraction()
        {
            const string input = "5 1/2";

            var success = Parser.TryParseRange(input, out var syntax);

            Assert.True(success);
            Assert.NotNull(syntax);
            Assert.Equal(input, string.Concat(syntax.Tokens.Select(t => t.Value)));

            var tokens = new[]
            {
                new Token(Span.Between(00, 00), "5", TokenKind.Numeric),
                new Token(Span.Between(01, 01), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(02, 04), "1/2", TokenKind.Fraction),
            };

            Assert.Equal(tokens.Take(1), syntax.Components);
            Assert.Equal(tokens.Last(), syntax.Fraction);
            Assert.Equal(tokens, syntax.Tokens);
        }

        [Fact]
        public void TryParseRange_Fraction_With_Separators()
        {
            const string input = "5.7 1/2";

            var success = Parser.TryParseRange(input, out var syntax);

            Assert.True(success);
            Assert.NotNull(syntax);
            Assert.Equal(input, string.Concat(syntax.Tokens.Select(t => t.Value)));

            var tokens = new[]
            {
                new Token(Span.Between(00, 00), "5", TokenKind.Numeric),
                new Token(Span.Between(01, 01), ".", TokenKind.Period),
                new Token(Span.Between(02, 02), "7", TokenKind.Numeric),
                new Token(Span.Between(03, 03), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(04, 06), "1/2", TokenKind.Fraction),
            };

            Assert.Equal(tokens.Take(3), syntax.Components);
            Assert.Equal(tokens.Last(), syntax.Fraction);
            Assert.Equal(tokens, syntax.Tokens);
        }

        [Fact]
        public void TryParseSecondaryAddress_With_Range()
        {
            const string input = "APARTMENT 5";

            var success = Parser.TryParseSecondaryAddress(input, out var syntax);

            Assert.True(success);
            Assert.NotNull(syntax);
            Assert.Equal(input, string.Concat(syntax.Tokens.Select(t => t.Value)));

            var tokens = new[]
            {
                new Token(Span.Between(00, 08), "APARTMENT", TokenKind.Alphabetic),
                new Token(Span.Between(09, 09), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(10, 10), "5", TokenKind.Numeric),
            };

            Assert.Equal(tokens, syntax.Tokens);
            Assert.Equal(tokens.Skip(2), syntax.UnitIdentifier.Tokens);
            Assert.Equal("APT", syntax.Designator);
        }

        [Fact]
        public void TryParseSecondaryAddress_With_Range_TrailingWhiteSpace()
        {
            const string input = "APARTMENT 5 ";

            var success = Parser.TryParseSecondaryAddress(input, out var syntax);

            Assert.True(success);
            Assert.NotNull(syntax);
            Assert.Equal(input, string.Concat(syntax.Tokens.Select(t => t.Value)));

            var tokens = new[]
            {
                new Token(Span.Between(00, 08), "APARTMENT", TokenKind.Alphabetic),
                new Token(Span.Between(09, 09), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(10, 10), "5", TokenKind.Numeric),
                new Token(Span.Between(11, 11), " ", TokenKind.WhiteSpace),
            };

            Assert.Equal(tokens, syntax.Tokens);
            Assert.Equal(tokens.Skip(2), syntax.UnitIdentifier.Tokens);
            Assert.Equal("APT", syntax.Designator);
        }

        [Fact]
        public void TryParseSecondaryAddress_With_NoRange()
        {
            const string input = "BASEMENT";

            var success = Parser.TryParseSecondaryAddress(input, out var syntax);

            Assert.True(success);
            Assert.NotNull(syntax);
            Assert.Equal(input, string.Concat(syntax.Tokens.Select(t => t.Value)));

            var tokens = new[]
            {
                new Token(Span.Between(00, 07), "BASEMENT", TokenKind.Alphabetic),
            };

            Assert.Equal(tokens, syntax.Tokens);
            Assert.Equal("BSMT", syntax.Designator);
            Assert.Null(syntax.UnitIdentifier);
        }

        [Fact]
        public void TryParseSecondaryAddress_With_NoRange_TrailingWhiteSpace()
        {
            const string input = "BASEMENT ";

            var success = Parser.TryParseSecondaryAddress(input, out var syntax);

            Assert.True(success);
            Assert.NotNull(syntax);
            Assert.Equal(input, string.Concat(syntax.Tokens.Select(t => t.Value)));

            var tokens = new[]
            {
                new Token(Span.Between(00, 07), "BASEMENT", TokenKind.Alphabetic),
                new Token(Span.Between(08, 08), " ", TokenKind.WhiteSpace),
            };

            Assert.Equal(tokens, syntax.Tokens);
            Assert.Equal("BSMT", syntax.Designator);
            Assert.Null(syntax.UnitIdentifier);
        }

        [Fact]
        public void TryParseSecondaryAddress_With_Period_Range()
        {
            const string input = "Apt. 5";

            var success = Parser.TryParseSecondaryAddress(input, out var syntax);

            Assert.True(success);
            Assert.NotNull(syntax);
            Assert.Equal(input, string.Concat(syntax.Tokens.Select(t => t.Value)));

            var tokens = new[]
            {
                new Token(Span.Between(00, 02), "Apt", TokenKind.Alphabetic),
                new Token(Span.Between(03, 03), ".", TokenKind.Period),
                new Token(Span.Between(04, 04), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(05, 05), "5", TokenKind.Numeric),
            };

            Assert.Equal(tokens, syntax.Tokens);
            Assert.Equal(tokens.Skip(3), syntax.UnitIdentifier.Tokens);
            Assert.Equal("APT", syntax.Designator);
        }

        [Fact]
        public void TryParseSecondaryAddress_With_Comma_Range()
        {
            const string input = "Apt, 5";

            var success = Parser.TryParseSecondaryAddress(input, out var syntax);

            Assert.True(success);
            Assert.NotNull(syntax);
            Assert.Equal(input, string.Concat(syntax.Tokens.Select(t => t.Value)));

            var tokens = new[]
            {
                new Token(Span.Between(00, 02), "Apt", TokenKind.Alphabetic),
                new Token(Span.Between(03, 03), ",", TokenKind.Comma),
                new Token(Span.Between(04, 04), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(05, 05), "5", TokenKind.Numeric),
            };

            Assert.Equal(tokens, syntax.Tokens);
            Assert.Equal(tokens.Skip(3), syntax.UnitIdentifier.Tokens);
            Assert.Equal("APT", syntax.Designator);
        }

        [Fact]
        public void TryParseSecondaryAddress_With_Octothorpe_Range()
        {
            const string input = "Apt# 5";

            var success = Parser.TryParseSecondaryAddress(input, out var syntax);

            Assert.True(success);
            Assert.NotNull(syntax);
            Assert.Equal(input, string.Concat(syntax.Tokens.Select(t => t.Value)));

            var tokens = new[]
            {
                new Token(Span.Between(00, 02), "Apt", TokenKind.Alphabetic),
                new Token(Span.Between(03, 03), "#", TokenKind.Octothorpe),
                new Token(Span.Between(04, 04), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(05, 05), "5", TokenKind.Numeric),
            };

            Assert.Equal(tokens, syntax.Tokens);
            Assert.Equal(tokens.Skip(3), syntax.UnitIdentifier.Tokens);
            Assert.Equal("APT", syntax.Designator);
        }

        [Fact]
        public void TryParseRuralRouteBox_With_PunctuationGalore()
        {
            const string input = "R,R. #1 BOX # 7 ";

            var success = Parser.TryParseRuralRouteBox(input, out var syntax);

            Assert.True(success);
            Assert.NotNull(syntax);
            Assert.Equal(input, string.Concat(syntax.Tokens.Select(t => t.Value)));

            var tokens = new[]
            {
                new Token(Span.Between(00, 00), "R", TokenKind.Alphabetic),
                new Token(Span.Between(01, 01), ",", TokenKind.Comma),
                new Token(Span.Between(02, 02), "R", TokenKind.Alphabetic),
                new Token(Span.Between(03, 03), ".", TokenKind.Period),
                new Token(Span.Between(04, 04), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(05, 05), "#", TokenKind.Octothorpe),
                new Token(Span.Between(06, 06), "1", TokenKind.Numeric),
                new Token(Span.Between(07, 07), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(08, 10), "BOX", TokenKind.Alphabetic),
                new Token(Span.Between(11, 11), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(12, 12), "#", TokenKind.Octothorpe),
                new Token(Span.Between(13, 13), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(14, 14), "7", TokenKind.Numeric),
                new Token(Span.Between(15, 15), " ", TokenKind.WhiteSpace),
            };

            Assert.Equal(tokens, syntax.Tokens);
            Assert.Equal(tokens.Skip(00).Take(06), syntax.RouteKeywordTokens);
            Assert.Equal(tokens.Skip(06).Take(01), syntax.RouteNumber.Tokens);
            Assert.Equal(tokens.Skip(07).Take(05), syntax.BoxKeywordTokens);
            Assert.Equal(tokens.Skip(12).Take(02), syntax.BoxNumber.Tokens);
        }

        [Fact]
        public void TryParseHighwayContractRouteBox_With_PunctuationGalore()
        {
            const string input = "H,C. #1 BOX # 7 ";

            var success = Parser.TryParseHighwayContractRouteBox(input, out var syntax);

            Assert.True(success);
            Assert.NotNull(syntax);
            Assert.Equal(input, string.Concat(syntax.Tokens.Select(t => t.Value)));

            var tokens = new[]
            {
                new Token(Span.Between(00, 00), "H", TokenKind.Alphabetic),
                new Token(Span.Between(01, 01), ",", TokenKind.Comma),
                new Token(Span.Between(02, 02), "C", TokenKind.Alphabetic),
                new Token(Span.Between(03, 03), ".", TokenKind.Period),
                new Token(Span.Between(04, 04), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(05, 05), "#", TokenKind.Octothorpe),
                new Token(Span.Between(06, 06), "1", TokenKind.Numeric),
                new Token(Span.Between(07, 07), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(08, 10), "BOX", TokenKind.Alphabetic),
                new Token(Span.Between(11, 11), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(12, 12), "#", TokenKind.Octothorpe),
                new Token(Span.Between(13, 13), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(14, 14), "7", TokenKind.Numeric),
                new Token(Span.Between(15, 15), " ", TokenKind.WhiteSpace),
            };

            Assert.Equal(tokens, syntax.Tokens);
            Assert.Equal(tokens.Skip(00).Take(06), syntax.RouteKeywordTokens);
            Assert.Equal(tokens.Skip(06).Take(01), syntax.RouteNumber.Tokens);
            Assert.Equal(tokens.Skip(07).Take(05), syntax.BoxKeywordTokens);
            Assert.Equal(tokens.Skip(12).Take(02), syntax.BoxNumber.Tokens);
        }

        [Fact]
        public void TryParsePostOfficeBox_With_PunctuationGalore()
        {
            const string input = "P,O. BOX # 7 ";

            var success = Parser.TryParsePostOfficeBox(input, out var syntax);

            Assert.True(success);
            Assert.NotNull(syntax);
            Assert.Equal(input, string.Concat(syntax.Tokens.Select(t => t.Value)));

            var tokens = new[]
            {
                new Token(Span.Between(00, 00), "P", TokenKind.Alphabetic),
                new Token(Span.Between(01, 01), ",", TokenKind.Comma),
                new Token(Span.Between(02, 02), "O", TokenKind.Alphabetic),
                new Token(Span.Between(03, 03), ".", TokenKind.Period),
                new Token(Span.Between(04, 04), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(05, 07), "BOX", TokenKind.Alphabetic),
                new Token(Span.Between(08, 08), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(09, 09), "#", TokenKind.Octothorpe),
                new Token(Span.Between(10, 10), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(11, 11), "7", TokenKind.Numeric),
                new Token(Span.Between(12, 12), " ", TokenKind.WhiteSpace),
            };

            Assert.Equal(tokens, syntax.Tokens);
            Assert.Equal(tokens.Skip(00).Take(09), syntax.BoxKeywordTokens);
            Assert.Equal(tokens.Skip(09).Take(02), syntax.BoxNumber.Tokens);
        }

        [Fact]
        public void TryParseStreetAddress_Whatever()
        {
            const string input = "123 1/2 N. South Circle Drive SE Unit  7.2";

            var success = Parser.TryParseStreetAddress(input, out var syntax);

            Assert.True(success);
            Assert.NotNull(syntax);
            Assert.Equal(input, string.Concat(syntax.Tokens.Select(t => t.Value)));

            var tokens = new[]
            {
                new Token(Span.Between(00, 02), "123", TokenKind.Numeric),
                new Token(Span.Between(03, 03), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(04, 06), "1/2", TokenKind.Fraction),
                new Token(Span.Between(07, 07), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(08, 08), "N", TokenKind.Alphabetic),
                new Token(Span.Between(09, 09), ".", TokenKind.Period),
                new Token(Span.Between(10, 10), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(11, 15), "South", TokenKind.Alphabetic),
                new Token(Span.Between(16, 16), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(17, 22), "Circle", TokenKind.Alphabetic),
                new Token(Span.Between(23, 23), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(24, 28), "Drive", TokenKind.Alphabetic),
                new Token(Span.Between(29, 29), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(30, 31), "SE", TokenKind.Alphabetic),
                new Token(Span.Between(32, 32), " ", TokenKind.WhiteSpace),
                new Token(Span.Between(33, 36), "Unit", TokenKind.Alphabetic),
                new Token(Span.Between(37, 38), "  ", TokenKind.WhiteSpace),
                new Token(Span.Between(39, 39), "7", TokenKind.Numeric),
                new Token(Span.Between(40, 40), ".", TokenKind.Period),
                new Token(Span.Between(41, 41), "2", TokenKind.Numeric),
            };

            Assert.Equal(tokens, syntax.Tokens);
        }
    }
}
