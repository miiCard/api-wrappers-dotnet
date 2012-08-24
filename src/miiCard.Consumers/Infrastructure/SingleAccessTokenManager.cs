using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenAuth.OAuth.ChannelElements;
using DotNetOpenAuth.OAuth.Messages;

namespace miiCard.Consumers.Infrastructure
{
    internal class SingleAccessTokenManager : IConsumerTokenManager
    {
        public string ConsumerKey { get; private set; }
        public string ConsumerSecret { get; private set; }

        public string AccessToken { get; private set; }
        public string AccessTokenSecret { get; private set; }

        public SingleAccessTokenManager(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            this.ConsumerKey = consumerKey;
            this.ConsumerSecret = consumerSecret;

            this.AccessToken = accessToken;
            this.AccessTokenSecret = accessTokenSecret;
        }

        public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret)
        {
            // No-op
        }

        public string GetTokenSecret(string token)
        {
            AssertValidToken(token);

            return this.AccessTokenSecret;
        }

        public TokenType GetTokenType(string token)
        {
            AssertValidToken(token);

            return TokenType.AccessToken;
        }

        public void StoreNewRequestToken(UnauthorizedTokenRequest request, ITokenSecretContainingMessage response)
        {
            // No-op
        }

        private void AssertValidToken(string token)
        {
            if (token != this.AccessToken)
            {
                throw new InvalidOperationException(string.Format("The specified token is not managed by this instance of {0}", this.GetType().Name));
            }
        }
    }
}
