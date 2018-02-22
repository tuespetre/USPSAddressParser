using Xunit;

namespace USPSAddressParser.Tests
{
    public partial class ParserTests
    {
        public static TheoryData<string, bool, string> Parser_TryParseStreetAddress_Data_Suffixes
        {
            get
            {
                var data = new TheoryData<string, bool, string>();

                foreach (var (common, name, abbreviation) in PostalDesignators.StreetSuffixes)
                {
                    data.Add($"123 ABBREVIATED {common}", true, $"123 ABBREVIATED {abbreviation}");

                    if (PostalDesignators.StreetSuffixesExemptFromExpansion.Contains(common))
                    {
                        data.Add($"123 {common} WAS NOT EXPANDED", true, $"123 {common} WAS NOT EXPANDED");
                    }
                    else
                    {
                        data.Add($"123 {common} WAS EXPANDED", true, $"123 {name} WAS EXPANDED");
                    }
                }

                // Suffix is not abbreviated if it is the only street name token
                data.Add("123 CIRCLE", true, "123 CIRCLE");

                // Suffix is abbreviated if the only other street name token is a predirectional
                data.Add("123 SOUTH CIRCLE", true, "123 SOUTH CIR");

                // Suffix is not abbreviated if the only other street name token is a postdirectional
                data.Add("123 CIRCLE SOUTH", true, "123 CIRCLE S");

                // Only the first of two suffixes is abbreviated
                data.Add("123 SOME ROAD CIRCLE", true, "123 SOME ROAD CIR");

                // Suffix is abbreviated if there is a postdirectional
                data.Add("123 SOME ROAD S", true, "123 SOME RD S");

                // Suffix is not abbreviated if there is not a postdirectional
                data.Add("123 SOME ROAD Q", true, "123 SOME ROAD Q");

                // Period is dropped
                data.Add("123 SOUTH CIR.", true, "123 SOUTH CIR");

                // Suffix is not expanded if part of an alphanumeric word
                data.Add("123 S 3RD ST", true, "123 S 3RD ST");

                // Suffix is not taken as a suffix if part of an alphanumeric word
                data.Add("123 S 3RD", true, "123 S 3RD");

                return data;
            }
        }

        public static TheoryData<string, bool, string> Parser_TryParseStreetAddress_Data_Directionals
        {
            get
            {
                var data = new TheoryData<string, bool, string>();

                var directionals = new[]
                {
                    ("n", "N"),
                    ("s", "S"),
                    ("e", "E"),
                    ("w", "W"),
                    ("ne", "NE"),
                    ("nw", "NW"),
                    ("se", "SE"),
                    ("sw", "SW"),
                    ("n.", "N"),
                    ("s.", "S"),
                    ("e.", "E"),
                    ("w.", "W"),
                    ("n.e.", "NE"),
                    ("n.w.", "NW"),
                    ("s.e.", "SE"),
                    ("s.w.", "SW"),
                    ("north", "N"),
                    ("south", "S"),
                    ("east", "E"),
                    ("west", "W"),
                    ("northeast", "NE"),
                    ("northwest", "NW"),
                    ("southeast", "SE"),
                    ("southwest", "SW"),
                };

                foreach (var (from, to) in directionals)
                {
                    // Predirectional
                    data.Add($"123 {from.ToUpperInvariant()} STREETNAME", true, $"123 {to} STREETNAME");
                }

                // Postdirectional is abbreviated
                data.Add($"123 STREETNAME SOUTH", true, $"123 STREETNAME S");

                // Predirectional and postdirectional are abbreviated
                data.Add($"123 SOUTH STREETNAME NORTH", true, $"123 S STREETNAME N");

                // Predirectional is not abbreviated if it is the only street name token
                data.Add($"123 SOUTH", true, $"123 SOUTH");

                // Predirectional is abbreviated if the only other street name token is a postdirectional
                data.Add($"123 SOUTH WEST", true, $"123 S WEST");

                // Predirectional and postdirectional are abbreviated if the only other street name token is a directional
                data.Add($"123 SOUTH WEST NORTH", true, $"123 S WEST N");

                return data;
            }
        }

        public static TheoryData<string, bool, string> Parser_TryParseStreetAddress_Data_SpecificCases
        {
            get
            {
                var data = new TheoryData<string, bool, string>
                {
                    // Apostrophe is preserved
                    { $"123 LORD'S SUPPER LN", true, $"123 LORD'S SUPPER LN" },

                    // Hyphen is preserved
                    { $"123 BRICKLE-BRACKLE", true, $"123 BRICKLE-BRACKLE" },

                    // Hyphen is preserved
                    { $"12345 HIGHWAY K-99 S", true, $"12345 HIGHWAY K-99 S" },

                    // Multiple units (not valid)
                    { $"12345 SOME STREET BLDG 1 APT 1", false, null },

                    // Range starts with alpha
                    { $"N12345 SOME RD", true, "N12345 SOME RD" },

                    // Not a valid street address
                    { "CITY CLERK", false, null },

                    // Not a valid street address
                    { "GENERAL DELIVERY", false, null },

                    // Not a valid street address
                    { "JOHN E APPLESEED", false, null },

                    // Not a valid street address
                    { "", false, null }
                };

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(Parser_TryParseStreetAddress_Data_Suffixes))]
        [MemberData(nameof(Parser_TryParseStreetAddress_Data_Directionals))]
        [MemberData(nameof(Parser_TryParseStreetAddress_Data_SpecificCases))]
        public void Parser_TryParseStreetAddress(string input, bool expectedSuccess, string expectedOutput)
        {
            var actualSuccess = Parser.TryParseStreetAddress(input, out var syntax);
            var actualOutput = syntax?.ToString();

            Assert.Equal(expectedSuccess, actualSuccess);
            Assert.Equal(expectedOutput, actualOutput);
        }

        [Theory]
        [InlineData("5", true, "5")]
        [InlineData("5 ", true, "5")]
        [InlineData("F", true, "F")]
        [InlineData("555", true, "555")]
        [InlineData("5F", true, "5F")]
        [InlineData("2-F", true, "2-F")]
        [InlineData("2-1", true, "2-1")]
        [InlineData("F2", true, "F2")]
        [InlineData("5½", true, "5 1/2")]
        [InlineData("5.5½", true, "5.5 1/2")]
        [InlineData("5 ½", true, "5 1/2")]
        [InlineData("5.5 ½", true, "5.5 1/2")]
        [InlineData("5 1/2", true, "5 1/2")]
        [InlineData("5.5 1/2", true, "5.5 1/2")]
        [InlineData("1.123", true, "1.123")]
        [InlineData("ABCDEFGH", true, "ABCDEFGH")]
        [InlineData("EL-OH-EL", true, "EL-OH-EL")]
        [InlineData("1.2.W45A", true, "1.2.W45A")]
        [InlineData("N23W2400", true, "N23W2400")]
        [InlineData("4N3000", true, "4N3000")]
        [InlineData("N12345", true, "N12345")]
        [InlineData("2 F", false, null)]
        [InlineData("ABCDEFGHI", false, null)]
        [InlineData("", false, null)]
        public void Parser_TryParseRange(string input, bool expectedSuccess, string expectedOutput)
        {
            var actualSuccess = Parser.TryParseRange(input, out var actualOutput);

            Assert.Equal(expectedSuccess, actualSuccess);
            Assert.Equal(expectedOutput, actualOutput?.ToString());
        }

        public static TheoryData<string, bool, string> Parser_TryParseSecondaryAddressUnit_Data
        {
            get
            {
                var data = new TheoryData<string, bool, string>();

                foreach (var (common, abbreviation, requiresRange) in PostalDesignators.SecondaryAddressUnitDesignators)
                {
                    data.Add($"{common} 5", true, $"{abbreviation} 5");
                    data.Add($"{abbreviation} 5", true, $"{abbreviation} 5");

                    if (!requiresRange)
                    {
                        data.Add($"{common}", true, $"{abbreviation}");
                        data.Add($"{abbreviation}", true, $"{abbreviation}");
                    }
                }

                // Octothorpe in the middle
                data.Add("APT # 2", true, "APT 2");

                // Hyphenated range
                data.Add("APT 2-1", true, "APT 2-1");

                // Street suffixes not considered for secondary unit ranges
                data.Add("FRONT ST", false, null);

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(Parser_TryParseSecondaryAddressUnit_Data))]
        public void Parser_TryParseSecondaryAddress(string input, bool expectedSuccess, string expectedOutput)
        {
            var actualSuccess = Parser.TryParseSecondaryAddress(input, out var syntax);
            var actualOutput = syntax?.ToString();

            Assert.Equal(expectedSuccess, actualSuccess);
            Assert.Equal(expectedOutput, actualOutput);
        }

        public static TheoryData<string, bool, string> Parser_TryParsePostOfficeBox_Data
        {
            get
            {
                var data = new TheoryData<string, bool, string>();

                var keywords = new[]
                {
                    "p o box",
                    "po box",
                    "p.o. box",
                    "post office box",
                    "box",
                    "caller",
                    "firm caller",
                    "bin",
                    "lockbox",
                    "drawer",
                    "po caller",
                    "po firm caller",
                    "po bin",
                    "po lockbox",
                    "po drawer",
                };

                foreach (var keyword in keywords)
                {
                    data.Add($"{keyword.ToUpperInvariant()} 1", true, "PO BOX 1");
                }

                data.Add("P.O> BOX 1", true, "PO BOX 1");

                data.Add("X PO BOX 1", false, null);

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(Parser_TryParsePostOfficeBox_Data))]
        public void Parser_TryParsePostOfficeBox(string input, bool expectedSuccess, string expectedOutput)
        {
            var actualSuccess = Parser.TryParsePostOfficeBox(input, out var syntax);
            var actualOutput = syntax?.ToString();

            Assert.Equal(expectedSuccess, actualSuccess);
            Assert.Equal(expectedOutput, actualOutput);
        }

        public static TheoryData<string, bool, string> Parser_TryParseRuralRouteBox_Data
        {
            get
            {
                var data = new TheoryData<string, bool, string>();

                var keywords = new[]
                {
                    "rr",
                    "r r",
                    "r.r",
                    "rr.",
                    "r.r.",
                    "r,r,",
                    "rural route",
                    "rural rt",
                    "rural rt.",
                    "route",
                    "rt",
                    "rt.",
                    "rd",
                    "rfd",
                };

                foreach (var keyword in keywords)
                {
                    data.Add($"{keyword.ToUpperInvariant()} 1 BOX 1", true, "RR 1 BOX 1");
                }

                data.Add("RR 1, BOX 1", true, "RR 1 BOX 1");
                data.Add("X RR 1 BOX 1", false, null);

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(Parser_TryParseRuralRouteBox_Data))]
        public void Parser_TryParseRuralRouteBox(string input, bool expectedSuccess, string expectedOutput)
        {
            var actualSuccess = Parser.TryParseRuralRouteBox(input, out var syntax);
            var actualOutput = syntax?.ToString();

            Assert.Equal(expectedSuccess, actualSuccess);
            Assert.Equal(expectedOutput, actualOutput);
        }

        public static TheoryData<string, bool, string> Parser_TryParseHighwayContractRouteBox_Data
        {
            get
            {
                var data = new TheoryData<string, bool, string>();

                var keywords = new[]
                {
                    "hc",
                    "h c",
                    "h.c",
                    "hc.",
                    "h.c.",
                    "h,c,",
                    "highway contract",
                    "highway contract rt",
                    "highway contract route",
                    "star rt",
                    "star route",
                };

                foreach (var keyword in keywords)
                {
                    data.Add($"{keyword.ToUpperInvariant()} 1 BOX 1", true, "HC 1 BOX 1");
                }

                data.Add("HC 1, BOX 1", true, "HC 1 BOX 1");
                data.Add("X HC 1 BOX 1", false, null);

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(Parser_TryParseHighwayContractRouteBox_Data))]
        public void Parser_TryParseHighwayContractRouteBox(string input, bool expectedSuccess, string expectedOutput)
        {
            var actualSuccess = Parser.TryParseHighwayContractRouteBox(input, out var syntax);
            var actualOutput = syntax?.ToString();

            Assert.Equal(expectedSuccess, actualSuccess);
            Assert.Equal(expectedOutput, actualOutput);
        }
    }
}
