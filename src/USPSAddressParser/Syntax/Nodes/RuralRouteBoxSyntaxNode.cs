using System;
using System.Collections.Immutable;

namespace USPSAddressParser.Syntax.Nodes
{
    public class RuralRouteBoxSyntaxNode : SyntaxNode
    {
        public RuralRouteBoxSyntaxNode(
            ImmutableArray<Token> routeKeywordTokens,
            RangeSyntaxNode routeNumber,
            ImmutableArray<Token> boxKeywordTokens,
            RangeSyntaxNode boxNumber)
        {
            RouteKeywordTokens = routeKeywordTokens;
            RouteNumber = routeNumber ?? throw new ArgumentNullException(nameof(routeNumber));
            BoxKeywordTokens = boxKeywordTokens;
            BoxNumber = boxNumber ?? throw new ArgumentNullException(nameof(boxNumber));

            var builder = ImmutableArray.CreateBuilder<Token>();

            builder.AddRange(routeKeywordTokens);
            builder.AddRange(routeNumber.Tokens);
            builder.AddRange(boxKeywordTokens);
            builder.AddRange(boxNumber.Tokens);

            Tokens = builder.ToImmutable();
        }

        public ImmutableArray<Token> RouteKeywordTokens { get; }

        public RangeSyntaxNode RouteNumber { get; }

        public ImmutableArray<Token> BoxKeywordTokens { get; }

        public RangeSyntaxNode BoxNumber { get; }

        public override ImmutableArray<Token> Tokens { get; }

        public override string ToString() => $"RR {RouteNumber} BOX {BoxNumber}";
    }
}
