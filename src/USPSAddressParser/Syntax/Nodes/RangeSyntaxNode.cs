using System;
using System.Collections.Immutable;
using System.Text;

namespace USPSAddressParser.Syntax.Nodes
{
    public class RangeSyntaxNode : SyntaxNode
    {
        public RangeSyntaxNode(ImmutableArray<Token> components, Token fraction, ImmutableArray<Token> tokens)
        {
            Components = components;
            Fraction = fraction;
            Tokens = tokens;
        }

        public ImmutableArray<Token> Components { get; }

        public Token Fraction { get; }

        public override ImmutableArray<Token> Tokens { get; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach (var component in Components)
            {
                switch (component.Kind)
                {
                    case TokenKind.Alphabetic:
                        builder.Append(component.Value.ToUpperInvariant());
                        continue;

                    case TokenKind.Numeric:
                    case TokenKind.Hyphen:
                    case TokenKind.Period:
                        builder.Append(component.Value);
                        continue;

                    default:
                        continue;
                }
            }

            if (!Fraction.IsNone)
            {
                builder.Append(" ");
                builder.Append(Fraction.Value);
            }

            return builder.ToString();
        }
    }
}
