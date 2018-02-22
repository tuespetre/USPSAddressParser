using System;

namespace USPSAddressParser.Syntax
{
    public struct Token
    {
        public static readonly Token None;

        public Token(Span span, string value, TokenKind kind)
        {
            Span = span;
            Value = value ?? string.Empty;
            Kind = kind;
        }

        public readonly Span Span;

        public readonly string Value;

        public readonly TokenKind Kind;

        public bool IsNone => Kind == TokenKind.None;

        public bool IsAlphabetic => Kind == TokenKind.Alphabetic;

        public bool IsNumeric => Kind == TokenKind.Numeric;

        public bool IsAlphanumeric => IsAlphabetic || IsNumeric;

        public bool IsWhiteSpace => Kind == TokenKind.WhiteSpace;

        public bool IsUnknown => Kind == TokenKind.Unknown;

        public bool IsCommaOrPeriod => Kind == TokenKind.Period || Kind == TokenKind.Comma;

        public bool IsSingleLetter => Kind == TokenKind.Alphabetic && Value.Length == 1;

        public bool IsOctothorpe => Kind == TokenKind.Octothorpe;

        public bool IsRangeSeparator => Kind == TokenKind.Period || Kind == TokenKind.Hyphen;

        public bool IsRangeComponent =>
            (Kind == TokenKind.Numeric || Kind == TokenKind.Alphabetic || Kind == TokenKind.Fraction)
            && Value?.Length <= 8;

        public bool Matches(string text) => Value.Equals(text, StringComparison.InvariantCultureIgnoreCase);

        public Token WithValue(string value) => new Token(Span, value, Kind);

        public override string ToString() => $"{Span}({Kind}) {Value}";
    }
}
