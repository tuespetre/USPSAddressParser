using System;
using System.Collections.Immutable;

namespace USPSAddressParser.Syntax.Nodes
{
    public class PostOfficeBoxSyntaxNode : SyntaxNode
    {
        public PostOfficeBoxSyntaxNode(ImmutableArray<Token> boxKeywordTokens, RangeSyntaxNode boxNumber)
        {
            BoxKeywordTokens = boxKeywordTokens;
            BoxNumber = boxNumber ?? throw new ArgumentNullException(nameof(boxNumber));

            var builder = ImmutableArray.CreateBuilder<Token>();

            builder.AddRange(boxKeywordTokens);
            builder.AddRange(boxNumber.Tokens);

            Tokens = builder.ToImmutable();
        }

        public ImmutableArray<Token> BoxKeywordTokens { get; }

        public RangeSyntaxNode BoxNumber { get; }

        public override ImmutableArray<Token> Tokens { get; }

        public override string ToString() => $"PO BOX {BoxNumber}";
    }
}
