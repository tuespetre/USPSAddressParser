using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using USPSAddressParser.Syntax;

namespace USPSAddressParser
{
    public static class Tokenizer
    {
        private static bool Peek(string input, int index, out char result)
        {
            if (index < input.Length)
            {
                result = input[index];

                return true;
            }
            else
            {
                result = default;

                return false;
            }
        }

        public static IEnumerable<Token> Tokenize(string input)
        {
            var index = 0;

            while (Peek(input, index, out var current))
            {
                if (Char.IsWhiteSpace(current))
                {
                    yield return ConsumeHomogenousToken(input, ref index, Char.IsWhiteSpace, TokenKind.WhiteSpace);
                }
                else if (Char.IsDigit(current))
                {
                    if (Peek(input, index + 1, out var slash) && slash == '/' &&
                        Peek(input, index + 2, out var divisor) && Char.IsDigit(divisor))
                    {
                        yield return new Token(new Span(index, 3), $"{current}/{divisor}", TokenKind.Fraction);
                        index += 3;
                    }
                    else
                    {
                        yield return ConsumeHomogenousToken(input, ref index, Char.IsDigit, TokenKind.Numeric);
                    }
                }
                else if (Char.IsLetter(current))
                {
                    yield return ConsumeHomogenousToken(input, ref index, Char.IsLetter, TokenKind.Alphabetic);
                }
                else if (Char.IsNumber(current) && TryExpandUnicodeFraction(current, out var fraction))
                {
                    yield return new Token(new Span(index, 1), fraction, TokenKind.Fraction);
                    index++;
                }
                else if (Char.IsPunctuation(current))
                {
                    switch (current)
                    {
                        case '.':
                            yield return new Token(new Span(index, 1), ".", TokenKind.Period);
                            index++;
                            break;

                        case ',':
                            yield return new Token(new Span(index, 1), ",", TokenKind.Comma);
                            index++;
                            break;

                        case '-':
                            yield return new Token(new Span(index, 1), "-", TokenKind.Hyphen);
                            index++;
                            break;

                        case '#':
                            yield return new Token(new Span(index, 1), "#", TokenKind.Octothorpe);
                            index++;
                            break;

                        case '\'':
                            yield return new Token(new Span(index, 1), "'", TokenKind.Apostrophe);
                            index++;
                            break;

                        default:
                            yield return new Token(new Span(index, 1), current.ToString(), TokenKind.Unknown);
                            index++;
                            break;
                    }
                }
                else
                {
                    yield return new Token(new Span(index, 1), current.ToString(), TokenKind.Unknown);
                    index++;
                }
            }
        }

        private static Token ConsumeHomogenousToken(string input, ref int index, Func<char, bool> predicate, TokenKind kind)
        {
            var start = index;

            do
            {
                index++;
            }
            while (Peek(input, index, out var current) && predicate(current));

            var length = index - start;
            var value = input.Substring(start, length);

            return new Token(new Span(start, length), value, kind);
        }

        private static bool TryExpandUnicodeFraction(char input, out string expanded)
        {
            switch (input)
            {
                case '\xBC':
                    expanded = "1/4";
                    return true;

                case '\xBD':
                    expanded = "1/2";
                    return true;

                case '\xBE':
                    expanded = "3/4";
                    return true;

                case '\x2150':
                    expanded = "1/7";
                    return true;

                case '\x2151':
                    expanded = "1/9";
                    return true;

                case '\x2152':
                    expanded = "1/10";
                    return true;

                case '\x2153':
                    expanded = "1/3";
                    return true;

                case '\x2154':
                    expanded = "2/3";
                    return true;

                case '\x2155':
                    expanded = "1/5";
                    return true;

                case '\x2156':
                    expanded = "2/5";
                    return true;

                case '\x2157':
                    expanded = "3/5";
                    return true;

                case '\x2158':
                    expanded = "4/5";
                    return true;

                case '\x2159':
                    expanded = "1/6";
                    return true;

                case '\x215A':
                    expanded = "5/6";
                    return true;

                case '\x215B':
                    expanded = "1/8";
                    return true;

                case '\x215C':
                    expanded = "3/8";
                    return true;

                case '\x215D':
                    expanded = "5/8";
                    return true;

                case '\x215E':
                    expanded = "7/8";
                    return true;

                default:
                    expanded = null;
                    return false;
            }
        }
    }
}
