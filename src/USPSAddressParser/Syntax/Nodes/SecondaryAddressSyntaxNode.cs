using System.Collections.Immutable;
using System.Text;

namespace USPSAddressParser.Syntax.Nodes
{
    public class SecondaryAddressSyntaxNode : SyntaxNode
    {
        public SecondaryAddressSyntaxNode(string designator, ImmutableArray<Token> unitKeywordTokens, RangeSyntaxNode unitIdentifier)
        {
            Designator = designator;
            UnitKeywordTokens = unitKeywordTokens;
            UnitIdentifier = unitIdentifier;

            var builder = ImmutableArray.CreateBuilder<Token>();

            builder.AddRange(unitKeywordTokens);
            builder.AddRange(unitIdentifier?.Tokens ?? ImmutableArray<Token>.Empty);

            Tokens = builder.ToImmutable();
        }

        public SecondaryAddressSyntaxNode(string designator, ImmutableArray<Token> unitKeywordTokens)
            : this(designator, unitKeywordTokens, null)
        {
        }

        public string Designator { get; set; }

        public ImmutableArray<Token> UnitKeywordTokens { get; }

        public RangeSyntaxNode UnitIdentifier { get; }

        public override ImmutableArray<Token> Tokens { get; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(Designator);

            if (UnitIdentifier != null)
            {
                builder.Append(" ");
                builder.Append(UnitIdentifier.ToString());
            }

            return builder.ToString();
        }
    }
}
