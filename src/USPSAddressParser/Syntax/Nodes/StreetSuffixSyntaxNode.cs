using System;
using System.Collections.Immutable;

namespace USPSAddressParser.Syntax.Nodes
{
    public class StreetSuffixSyntaxNode : SyntaxNode
    {
        public StreetSuffixSyntaxNode(string abbreviation, ImmutableArray<Token> tokens)
        {
            Abbreviation = abbreviation ?? throw new ArgumentNullException(nameof(abbreviation));
            Tokens = tokens;
        }

        public string Abbreviation { get; }

        public override ImmutableArray<Token> Tokens { get; }

        public override string ToString() => Abbreviation;
    }
}
