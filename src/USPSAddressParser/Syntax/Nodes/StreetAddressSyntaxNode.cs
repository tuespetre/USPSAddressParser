using System;
using System.Collections.Immutable;
using System.Text;

namespace USPSAddressParser.Syntax.Nodes
{
    public class StreetAddressSyntaxNode : SyntaxNode
    {
        public StreetAddressSyntaxNode(
            RangeSyntaxNode addressNumber,
            DirectionalSyntaxNode preDirectional,
            ImmutableArray<Token> primaryNameTokens,
            StreetSuffixSyntaxNode suffix,
            DirectionalSyntaxNode postDirectional,
            SecondaryAddressSyntaxNode secondaryAddress)
        {
            AddressNumber = addressNumber ?? throw new ArgumentNullException(nameof(addressNumber));
            PreDirectional = preDirectional;
            PrimaryNameTokens = primaryNameTokens;
            Suffix = suffix;
            PostDirectional = postDirectional;
            SecondaryAddress = secondaryAddress;

            var builder = ImmutableArray.CreateBuilder<Token>();

            builder.AddRange(addressNumber.Tokens);
            
            builder.AddRange(preDirectional?.Tokens ?? ImmutableArray<Token>.Empty);
            builder.AddRange(primaryNameTokens);
            builder.AddRange(suffix?.Tokens ?? ImmutableArray<Token>.Empty);
            builder.AddRange(postDirectional?.Tokens ?? ImmutableArray<Token>.Empty);
            builder.AddRange(secondaryAddress?.Tokens ?? ImmutableArray<Token>.Empty);

            Tokens = builder.ToImmutable();
        }

        public RangeSyntaxNode AddressNumber { get; }

        public DirectionalSyntaxNode PreDirectional { get; }

        public ImmutableArray<Token> PrimaryNameTokens { get; }

        public StreetSuffixSyntaxNode Suffix { get; }

        public DirectionalSyntaxNode PostDirectional { get; }

        public SecondaryAddressSyntaxNode SecondaryAddress { get; }

        public override ImmutableArray<Token> Tokens { get; }

        public string GetStreetName()
        {
            var builder = new StringBuilder();
            var lastIndex = PrimaryNameTokens.Length - 1;

            for (var i = 0; i < PrimaryNameTokens.Length; i++)
            {
                var token = PrimaryNameTokens[i];

                switch (token.Kind)
                {
                    case TokenKind.WhiteSpace when i > 0 && i < lastIndex:
                        builder.Append(" ");
                        continue;

                    case TokenKind.Apostrophe:
                    case TokenKind.Hyphen:
                    case TokenKind.Numeric:
                        builder.Append(token.Value);
                        continue;

                    case TokenKind.Alphabetic:
                        if (PrimaryNameTokens.Length > 1)
                        {
                            if (i == 0)
                            {
                                if (PrimaryNameTokens[i + 1].IsWhiteSpace)
                                {
                                    builder.Append(TryExpandWord(token.Value).ToUpperInvariant());
                                    continue;
                                }
                            }
                            else if (i == lastIndex)
                            {
                                if (PrimaryNameTokens[i - 1].IsWhiteSpace)
                                {
                                    builder.Append(TryExpandWord(token.Value).ToUpperInvariant());
                                    continue;
                                }
                            }
                            else if (PrimaryNameTokens[i - 1].IsWhiteSpace && PrimaryNameTokens[i + 1].IsWhiteSpace)
                            {
                                builder.Append(TryExpandWord(token.Value).ToUpperInvariant());
                                continue;
                            }
                        }

                        builder.Append(token.Value.ToUpperInvariant());
                        continue;

                    case TokenKind.Unknown:
                        builder.Append(token.Value);
                        continue;

                    default:
                        continue;
                }
            }

            return builder.ToString();
        }

        public override string ToString() => ToString(onlyStreet: false);

        public string ToString(bool onlyStreet)
        {
            var builder = new StringBuilder();

            if (!onlyStreet)
            {
                builder.Append(AddressNumber.ToString());
                builder.Append(" ");
            }

            if (PreDirectional != null)
            {
                builder.Append(PreDirectional.ToString());
                builder.Append(" ");
            }

            builder.Append(GetStreetName());

            if (Suffix != null)
            {
                builder.Append(" ");
                builder.Append(Suffix.ToString());
            }

            if (PostDirectional != null)
            {
                builder.Append(" ");
                builder.Append(PostDirectional.ToString());
            }

            if (SecondaryAddress != null && !onlyStreet)
            {
                builder.Append(" ");
                builder.Append(SecondaryAddress.ToString());
            }

            return builder.ToString();
        }

        private string TryExpandWord(string value)
        {
            foreach (var exempt in new[] { "st", "via", "express", "centre" })
            {
                if (exempt.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                {
                    return value;
                }
            }

            foreach (var (common, expanded, abbreviated) in PostalDesignators.StreetSuffixes)
            {
                if (value.Equals(common, StringComparison.InvariantCultureIgnoreCase))
                {
                    return expanded;
                }
            }

            return value;
        }
    }
}
