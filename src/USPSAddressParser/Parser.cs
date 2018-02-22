using System;
using System.Collections.Immutable;
using System.Linq;
using USPSAddressParser.Syntax;
using USPSAddressParser.Syntax.Nodes;

namespace USPSAddressParser
{
    public static partial class Parser
    {
        #region private APIs

        private struct ParserState
        {
            public ParserState(ImmutableArray<Token> tokens)
            {
                Index = 0;
                Tokens = tokens;
                Buffer = ImmutableArray<Token>.Empty;
            }

            public ParserState(ImmutableArray<Token> tokens, int index)
            {
                Index = index;
                Tokens = tokens;
                Buffer = ImmutableArray<Token>.Empty;
            }

            public int Index { get; private set; }

            public ImmutableArray<Token> Tokens { get; }

            public ImmutableArray<Token> Buffer { get; private set; }

            public Token Current => Tokens.ElementAtOrDefault(Index);

            public void ConsumeWhiteSpace()
            {
                if (TryConsume(t => t.IsWhiteSpace || t.IsUnknown))
                {
                    ConsumeWhiteSpace();
                }
            }

            public bool TryConsume(Func<Token, bool> predicate)
            {
                if (Index < Tokens.Length)
                {
                    var current = Tokens[Index];

                    if (predicate(current))
                    {
                        Buffer = Buffer.Add(current);
                        Index++;
                        return true;
                    }
                }

                return false;
            }

            public bool TryConsume(string text)
            {
                if (Index < Tokens.Length)
                {
                    var current = Tokens[Index];

                    if (current.Matches(text))
                    {
                        Buffer = Buffer.Add(current);
                        Index++;
                        return true;
                    }
                }

                return false;
            }

            public ImmutableArray<Token> TakeBuffer()
            {
                var result = Buffer.Reverse().ToImmutableArray();

                Buffer = ImmutableArray<Token>.Empty;

                return result;
            }
        }

        private delegate bool ParseDelegate<T>(ref ParserState state, out T result);

        private static bool TryParseFromEnd<T>(ref ImmutableArray<Token> tokens, out T result, ParseDelegate<T> parseDelegate)
            where T : SyntaxNode
        {
            result = default;

            if (tokens.IsDefaultOrEmpty)
            {
                return false;
            }

            var snapshot = tokens;
            var success = false;

            for (var i = 0; i < tokens.Length; i++)
            {
                var state = new ParserState(tokens, tokens.Length - 1 - i);

                if (parseDelegate(ref state, out var temp))
                {
                    if (temp.Tokens.Length != i + 1)
                    {
                        break;
                    }
                    else
                    {
                        success = true;
                        result = temp;
                    }
                }
            }

            if (success)
            {
                tokens = tokens.Take(tokens.Length - result.Tokens.Length).ToImmutableArray();

                return true;
            }

            tokens = snapshot;
            result = default;

            return false;
        }

        private static void TryParseRangeAppend(ref ParserState state, ref RangeSyntaxNode range)
        {
            var snapshot = (state, range);

            var gotSeparator = state.TryConsume(t => t.IsRangeSeparator);

            if (state.TryConsume(t => t.IsRangeComponent))
            {
                var tokens = state.TakeBuffer();

                range = new RangeSyntaxNode(
                    tokens.AddRange(range.Components),
                    range.Fraction,
                    tokens.AddRange(range.Tokens));

                if (range.ToString().Length <= 8)
                {
                    TryParseRangeAppend(ref state, ref range);

                    return;
                }
            }

            (state, range) = snapshot;
        }

        private static bool TryParseRange(ref ParserState state, out RangeSyntaxNode result)
        {
            result = null;

            var snapshot = state;

            state.ConsumeWhiteSpace();

            var fraction = Token.None;

            if (state.TryConsume(t => t.Kind == TokenKind.Fraction))
            {
                fraction = state.Buffer.Last();

                state.ConsumeWhiteSpace();
            }

            if (state.TryConsume(t => t.IsRangeComponent))
            {
                var tokens = state.TakeBuffer();

                result = new RangeSyntaxNode(
                    tokens.Take(1).ToImmutableArray(),
                    fraction,
                    tokens);

                TryParseRangeAppend(ref state, ref result);

                if (result != null)
                {
                    state.TakeBuffer();
                    return true;
                }
            }

            state = snapshot;

            return false;
        }

        private static bool TryParseDirectional(ref ParserState state, out DirectionalSyntaxNode result)
        {
            result = default;

            var snapshot = state;

            state.ConsumeWhiteSpace();

            if (state.TryConsume("northeast"))
            {
                result = new DirectionalSyntaxNode("NE", state.TakeBuffer());
                return true;
            }
            else if (state.TryConsume("northwest"))
            {
                result = new DirectionalSyntaxNode("NW", state.TakeBuffer());
                return true;
            }
            else if (state.TryConsume("southeast"))
            {
                result = new DirectionalSyntaxNode("SE", state.TakeBuffer());
                return true;
            }
            else if (state.TryConsume("southwest"))
            {
                result = new DirectionalSyntaxNode("SW", state.TakeBuffer());
                return true;
            }
            else if (state.TryConsume("north"))
            {
                result = new DirectionalSyntaxNode("N", state.TakeBuffer());
                return true;
            }
            else if (state.TryConsume("south"))
            {
                result = new DirectionalSyntaxNode("S", state.TakeBuffer());
                return true;
            }
            else if (state.TryConsume("east"))
            {
                result = new DirectionalSyntaxNode("E", state.TakeBuffer());
                return true;
            }
            else if (state.TryConsume("west"))
            {
                result = new DirectionalSyntaxNode("W", state.TakeBuffer());
                return true;
            }
            else if (state.TryConsume("ne"))
            {
                result = new DirectionalSyntaxNode("NE", state.TakeBuffer());
                return true;
            }
            else if (state.TryConsume("nw"))
            {
                result = new DirectionalSyntaxNode("NW", state.TakeBuffer());
                return true;
            }
            else if (state.TryConsume("se"))
            {
                result = new DirectionalSyntaxNode("SE", state.TakeBuffer());
                return true;
            }
            else if (state.TryConsume("sw"))
            {
                result = new DirectionalSyntaxNode("SW", state.TakeBuffer());
                return true;
            }
            else
            {
                state.TryConsume(t => t.IsCommaOrPeriod);

                if (state.TryConsume("e"))
                {
                    //parser.ConsumeWhiteSpace();

                    if (state.TryConsume(t => t.IsCommaOrPeriod))
                    {
                        //parser.ConsumeWhiteSpace();
                    }

                    if (state.TryConsume("n"))
                    {
                        result = new DirectionalSyntaxNode("NE", state.TakeBuffer());
                    }
                    else if (state.TryConsume("s"))
                    {
                        result = new DirectionalSyntaxNode("SE", state.TakeBuffer());
                    }
                    else
                    {
                        result = new DirectionalSyntaxNode("E", state.TakeBuffer());
                    }

                    return true;
                }
                else if (state.TryConsume("w"))
                {
                    //parser.ConsumeWhiteSpace();

                    if (state.TryConsume(t => t.IsCommaOrPeriod))
                    {
                        //parser.ConsumeWhiteSpace();
                    }

                    if (state.TryConsume("n"))
                    {
                        result = new DirectionalSyntaxNode("NW", state.TakeBuffer());
                    }
                    else if (state.TryConsume("s"))
                    {
                        result = new DirectionalSyntaxNode("SW", state.TakeBuffer());
                    }
                    else
                    {
                        result = new DirectionalSyntaxNode("W", state.TakeBuffer());
                    }

                    return true;
                }
                else if (state.TryConsume("s"))
                {
                    result = new DirectionalSyntaxNode("S", state.TakeBuffer());

                    return true;
                }
                else if (state.TryConsume("n"))
                {
                    result = new DirectionalSyntaxNode("N", state.TakeBuffer());

                    return true;
                }
            }

            state = snapshot;

            return false;
        }

        private static bool TryParseStreetSuffix(ref ParserState state, out StreetSuffixSyntaxNode result)
        {
            result = default;

            var snapshot = state;

            state.ConsumeWhiteSpace();

            state.TryConsume(t => t.IsCommaOrPeriod);

            foreach (var (common, expanded, abbreviated) in PostalDesignators.StreetSuffixes)
            {
                // The check against a current numeric token prevents us from errantly picking
                // up things like the "RD" in "3RD".
                if (state.TryConsume(t => t.Matches(common)) && !state.Current.IsNumeric)
                {
                    result = new StreetSuffixSyntaxNode(abbreviated, state.TakeBuffer());

                    return true;
                }
            }

            state = snapshot;

            return false;
        }

        private static bool TryParseSecondaryAddress(ref ParserState state, out SecondaryAddressSyntaxNode result)
        {
            result = default;

            var snapshot = state;

            state.ConsumeWhiteSpace();

            foreach (var (expanded, abbreviated, requiresRange) in PostalDesignators.SecondaryAddressUnitDesignators)
            {
                if (!requiresRange && state.TryConsume(t => t.Matches(expanded) || t.Matches(abbreviated)))
                {
                    result = new SecondaryAddressSyntaxNode(abbreviated, state.TakeBuffer());

                    return true;
                }
            }

            if (!TryParseStreetSuffix(ref state, out _) && TryParseRange(ref state, out var rangeUnitIdentifier))
            {
                state.ConsumeWhiteSpace();

                string designator = null;

                if (state.TryConsume(t => t.IsOctothorpe))
                {
                    designator = "#";
                    state.ConsumeWhiteSpace();
                }

                if (state.TryConsume(t => t.IsCommaOrPeriod))
                {
                    state.ConsumeWhiteSpace();
                }

                foreach (var (expanded, abbreviated, _) in PostalDesignators.SecondaryAddressUnitDesignators)
                {
                    if (state.TryConsume(t => t.Matches(expanded) || t.Matches(abbreviated)))
                    {
                        designator = abbreviated;
                        break;
                    }
                }

                if (designator != null)
                {
                    result = new SecondaryAddressSyntaxNode(designator, state.TakeBuffer(), rangeUnitIdentifier);

                    return true;
                }
            }

            state = snapshot;

            return false;
        }

        private static bool TryParseStreetAddress(ref ParserState state, out StreetAddressSyntaxNode result)
        {
            result = default;

            var snapshot = state;

            TryParseSecondaryAddress(ref state, out var secondaryAddress);

            if (!TryParseSecondaryAddress(ref state, out _))
            {
                var gotPostDirectional = TryParseDirectional(ref state, out var postDirectional);

                var gotSuffix = TryParseStreetSuffix(ref state, out var suffix);

                var primaryNameTokens = state.Tokens.Skip(state.Index).ToImmutableArray();

                state = new ParserState(state.Tokens, state.Tokens.Length);

                if (TryParseFromEnd<RangeSyntaxNode>(ref primaryNameTokens, out var addressNumber, TryParseRange)
                    && addressNumber.Components.Any(c => c.IsNumeric))
                {
                    var gotPreDirectional = TryParseFromEnd<DirectionalSyntaxNode>(ref primaryNameTokens, out var preDirectional, TryParseDirectional);

                    primaryNameTokens = primaryNameTokens.Reverse().ToImmutableArray();

                    if (primaryNameTokens.All(t => t.IsWhiteSpace))
                    {
                        if (gotSuffix && !gotPreDirectional)
                        {
                            primaryNameTokens = suffix.Tokens.ToImmutableArray();
                            suffix = null;
                        }
                        else if (gotPostDirectional)
                        {
                            primaryNameTokens = postDirectional.Tokens.ToImmutableArray();
                            postDirectional = null;
                        }
                        else if (gotPreDirectional)
                        {
                            primaryNameTokens = preDirectional.Tokens.ToImmutableArray();
                            preDirectional = null;
                        }
                    }

                    if (primaryNameTokens.Length >= 3)
                    {
                        var token0 = primaryNameTokens[0];
                        var token2 = primaryNameTokens[2];

                        if (token0.Matches("CO") || token0.Matches("CTY") || token0.Matches("COUNTY"))
                        {
                            if (token2.Matches("RD") || token2.Matches("ROAD"))
                            {
                                primaryNameTokens
                                    = primaryNameTokens
                                        .SetItem(0, token0.WithValue("COUNTY"))
                                        .SetItem(2, token0.WithValue("ROAD"));
                            }
                        }
                    }

                    if (!primaryNameTokens.IsEmpty)
                    {
                        result = new StreetAddressSyntaxNode(
                            addressNumber,
                            preDirectional,
                            primaryNameTokens,
                            suffix,
                            postDirectional,
                            secondaryAddress);

                        return true;
                    }
                }
            }

            state = snapshot;

            return false;
        }

        private static bool TryParsePostOfficeBoxKeywords(ref ParserState state, out ImmutableArray<Token> result)
        {
            result = default;

            var snapshot = state;

            state.ConsumeWhiteSpace();

            if (state.TryConsume("office"))
            {
                state.ConsumeWhiteSpace();

                if (state.TryConsume("post"))
                {
                    result = state.TakeBuffer();
                }

                return true;
            }
            else
            {
                state.TryConsume(t => t.IsCommaOrPeriod);

                state.ConsumeWhiteSpace();

                if (state.TryConsume("po"))
                {
                    result = state.TakeBuffer();

                    return true;
                }
                else if (state.TryConsume("o"))
                {
                    state.ConsumeWhiteSpace();

                    state.TryConsume(t => t.IsCommaOrPeriod);

                    state.ConsumeWhiteSpace();

                    if (state.TryConsume("p"))
                    {
                        result = state.TakeBuffer();

                        return true;
                    }
                }
            }

            state = snapshot;

            return false;
        }

        private static bool TryParsePostOfficeBox(ref ParserState state, out PostOfficeBoxSyntaxNode result)
        {
            result = null;

            var snapshot = state;

            if (TryParseRange(ref state, out var boxNumber))
            {
                state.ConsumeWhiteSpace();

                if (state.TryConsume(t => t.IsOctothorpe))
                {
                    state.ConsumeWhiteSpace();
                }

                if (state.TryConsume("caller"))
                {
                    state.ConsumeWhiteSpace();

                    state.TryConsume("firm");
                }
                else if (state.TryConsume(t => t.Matches("box") || t.Matches("drawer") || t.Matches("bin") || t.Matches("drawer") || t.Matches("lockbox")))
                {
                    // no-op
                }
                else
                {
                    goto Failure;
                }

                var boxKeywordTokens = state.TakeBuffer();

                if (TryParsePostOfficeBoxKeywords(ref state, out var additionalTokens))
                {
                    boxKeywordTokens = additionalTokens.AddRange(boxKeywordTokens);
                }

                result = new PostOfficeBoxSyntaxNode(boxKeywordTokens, boxNumber);

                return true;
            }

            Failure:
            state = snapshot;
            return false;
        }

        private static bool TryParseRuralRouteBox(ref ParserState state, out RuralRouteBoxSyntaxNode result)
        {
            result = null;

            var snapshot = state;

            RangeSyntaxNode routeNumber;
            ImmutableArray<Token> boxKeywordTokens;

            if (TryParseRange(ref state, out var boxNumber))
            {
                state.ConsumeWhiteSpace();

                if (state.TryConsume(t => t.IsOctothorpe))
                {
                    state.ConsumeWhiteSpace();
                }

                if (state.TryConsume("box"))
                {
                    state.ConsumeWhiteSpace();

                    if (state.TryConsume(t => t.IsCommaOrPeriod))
                    {
                        state.ConsumeWhiteSpace();
                    }

                    boxKeywordTokens = state.TakeBuffer();

                    if (TryParseRange(ref state, out routeNumber))
                    {
                        state.ConsumeWhiteSpace();

                        if (state.TryConsume(t => t.IsOctothorpe))
                        {
                            state.ConsumeWhiteSpace();
                        }

                        if (state.TryConsume(t => t.IsCommaOrPeriod))
                        {
                            state.ConsumeWhiteSpace();
                        }

                        if (state.TryConsume(t => t.Matches("rr") || t.Matches("rd") || t.Matches("rfd")))
                        {
                            goto Parsed;
                        }
                        else if (state.TryConsume("r"))
                        {
                            state.ConsumeWhiteSpace();

                            if (state.TryConsume(t => t.IsCommaOrPeriod))
                            {
                                state.ConsumeWhiteSpace();
                            }

                            if (state.TryConsume("r"))
                            {
                                goto Parsed;
                            }
                        }
                        else if (state.TryConsume(t => t.Matches("route") || t.Matches("rt")))
                        {
                            state.ConsumeWhiteSpace();

                            state.TryConsume("rural");

                            goto Parsed;
                        }
                    }
                }
            }

            state = snapshot;

            return false;

            Parsed:

            result = new RuralRouteBoxSyntaxNode(
                state.TakeBuffer(),
                routeNumber,
                boxKeywordTokens,
                boxNumber);

            return true;
        }

        private static bool TryParseHighwayContractRouteBox(ref ParserState state, out HighwayContractRouteBoxSyntaxNode result)
        {
            result = null;

            var snapshot = state;

            RangeSyntaxNode routeNumber;
            ImmutableArray<Token> boxKeywordTokens;

            if (TryParseRange(ref state, out var boxNumber))
            {
                state.ConsumeWhiteSpace();

                if (state.TryConsume(t => t.IsOctothorpe))
                {
                    state.ConsumeWhiteSpace();
                }

                if (state.TryConsume("box"))
                {
                    state.ConsumeWhiteSpace();

                    if (state.TryConsume(t => t.IsCommaOrPeriod))
                    {
                        state.ConsumeWhiteSpace();
                    }

                    boxKeywordTokens = state.TakeBuffer();

                    if (TryParseRange(ref state, out routeNumber))
                    {
                        state.ConsumeWhiteSpace();

                        if (state.TryConsume(t => t.IsOctothorpe))
                        {
                            state.ConsumeWhiteSpace();
                        }

                        if (state.TryConsume(t => t.IsCommaOrPeriod))
                        {
                            state.ConsumeWhiteSpace();
                        }

                        if (state.TryConsume("hc"))
                        {
                            goto Parsed;
                        }

                        if (state.TryConsume("c"))
                        {
                            state.ConsumeWhiteSpace();

                            if (state.TryConsume(t => t.IsCommaOrPeriod))
                            {
                                state.ConsumeWhiteSpace();
                            }

                            if (state.TryConsume("h"))
                            {
                                goto Parsed;
                            }
                        }
                        else if (state.TryConsume(t => t.Matches("route") || t.Matches("rt")))
                        {
                            state.ConsumeWhiteSpace();

                            if (state.TryConsume("contract"))
                            {
                                state.ConsumeWhiteSpace();

                                if (state.TryConsume("highway"))
                                {
                                    goto Parsed;
                                }
                            }
                            else if (state.TryConsume("star"))
                            {
                                goto Parsed;
                            }
                        }
                        else if (state.TryConsume("contract"))
                        {
                            state.ConsumeWhiteSpace();

                            if (state.TryConsume("highway"))
                            {
                                goto Parsed;
                            }
                        }
                    }
                }
            }

            state = snapshot;

            return false;

            Parsed:

            result = new HighwayContractRouteBoxSyntaxNode(
                state.TakeBuffer(),
                routeNumber,
                boxKeywordTokens,
                boxNumber);

            return true;
        }

        #endregion

        // TODO: Discarded leading tokens

        // TODO: Multiple lines

        // TODO: Attention lines

        // TODO: General Delivery

        // TODO: USPS addresses

        // TODO: Military addresses

        // TODO: Puerto Rico addresses

        // TODO: US Virgin Islands addresses

        #region public APIs

        public static bool TryParseRange(string input, out RangeSyntaxNode result)
        {
            var tokens = Tokenizer.Tokenize(input).Reverse().ToImmutableArray();

            var state = new ParserState(tokens);

            var success = TryParseRange(ref state, out var syntax) && state.Current.IsNone;

            result = success ? syntax : null;

            return success;
        }

        public static bool TryParseSecondaryAddress(string input, out SecondaryAddressSyntaxNode result)
        {
            var tokens = Tokenizer.Tokenize(input).Reverse().ToImmutableArray();

            var state = new ParserState(tokens);

            var success = TryParseSecondaryAddress(ref state, out var syntax) && state.Current.IsNone;

            result = success ? syntax : null;

            return success;
        }

        public static bool TryParseStreetAddress(string input, out StreetAddressSyntaxNode result)
        {
            var tokens = Tokenizer.Tokenize(input).Reverse().ToImmutableArray();

            var state = new ParserState(tokens);

            var success = TryParseStreetAddress(ref state, out var syntax) && state.Current.IsNone;

            result = success ? syntax : null;

            return success;
        }

        public static bool TryParsePostOfficeBox(string input, out PostOfficeBoxSyntaxNode result)
        {
            var tokens = Tokenizer.Tokenize(input).Reverse().ToImmutableArray();

            var state = new ParserState(tokens);

            var success = TryParsePostOfficeBox(ref state, out var syntax) && state.Current.IsNone;

            result = success ? syntax : null;

            return success;
        }

        public static bool TryParseRuralRouteBox(string input, out RuralRouteBoxSyntaxNode result)
        {
            var tokens = Tokenizer.Tokenize(input).Reverse().ToImmutableArray();

            var state = new ParserState(tokens);

            var success = TryParseRuralRouteBox(ref state, out var syntax) && state.Current.IsNone;

            result = success ? syntax : null;

            return success;
        }

        public static bool TryParseHighwayContractRouteBox(string input, out HighwayContractRouteBoxSyntaxNode result)
        {
            var tokens = Tokenizer.Tokenize(input).Reverse().ToImmutableArray();

            var state = new ParserState(tokens);

            var success = TryParseHighwayContractRouteBox(ref state, out var syntax) && state.Current.IsNone;

            result = success ? syntax : null;

            return success;
        }

        #endregion
    }
}
