using System.Collections.Immutable;
using System.Linq;

namespace USPSAddressParser.Syntax.Nodes
{
    public abstract class SyntaxNode
    {
        public abstract ImmutableArray<Token> Tokens { get; }

        public virtual Span FullSpan => Span.Combine(FirstToken.Span, LastToken.Span);

        public virtual Token FirstToken => Tokens.FirstOrDefault();

        public virtual Token LastToken => Tokens.LastOrDefault();
    }
}
