using System;

namespace USPSAddressParser.Syntax
{
    public struct Span
    {
        public Span(int start, int length)
        {
            Start = start < 0 ? throw new ArgumentOutOfRangeException(nameof(start)) : start;
            Length = length < 1 ? throw new ArgumentOutOfRangeException(nameof(length)) : length;
        }

        public int Start { get; }

        public int End => Start + Length - 1;

        public int Length { get; }

        public static Span Combine(Span from, Span through) => new Span(from.Start, through.Start - from.Start + through.Length);

        public static Span Between(int start, int end) => new Span(start, (end - start) + 1);

        public override string ToString() => $"[{Length}:{Start}-{End}]";
    }
}
