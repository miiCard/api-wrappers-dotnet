using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenAuth.OAuth.ChannelElements;
using System.Web;
using System.Web.SessionState;

namespace miiCard.Consumers.Infrastructure
{
    /// <summary>
    /// Represents a default token manager that stores request and access tokens in session state. This class is
    /// intended for rapid prototyping use, and the user should consider replacing it with a database-backed
    /// implementation of IConsumerTokenManager.
    /// </summary>
    public class SessionStateConsumerTokenManager : IConsumerTokenManager
    {
        private static readonly string SESSION_KEY_TOKENS = typeof(SessionStateConsumerTokenManager).FullName + ".Tokens";

        /// <summary>
        /// Gets the consumer key used to sign OAuth requests and responses.
        /// </summary>
        public string ConsumerKey
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the consumer secret used to sign OAuth requests and responses.
        /// </summary>
        public string ConsumerSecret
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Initialises a new SessionStateConsumerTokenManager using the specified consumer key and secret.
        /// </summary>
        /// <param name="consumerKey">The consumer key to be used when signing OAuth requests.</param>
        /// <param name="consumerSecret">The consumer secret to be used when signing OAuth requests.</param>
        public SessionStateConsumerTokenManager(string consumerKey, string consumerSecret)
        {
            this.ConsumerKey = consumerKey;
            this.ConsumerSecret = consumerSecret;
        }

        public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret)
        {
            var storage = this.GetTokenStorage();
            if (!storage.Remove(requestToken))
            {
                throw new ArgumentException("Specified requestToken doesn't exist");
            }

            storage[accessToken] = new InMemoryToken() { Token = accessToken, TokenType = TokenType.AccessToken, TokenSecret = accessTokenSecret };
        }

        public string GetTokenSecret(string token)
        {
            return this.GetTokenMetadata(token).TokenSecret;
        }

        public TokenType GetTokenType(string token)
        {
            return this.GetTokenMetadata(token).TokenType;
        }

        private InMemoryToken GetTokenMetadata(string token)
        {
            var storage = this.GetTokenStorage();
            InMemoryToken inMemoryToken = null;
            if (!storage.TryGetValue(token, out inMemoryToken))
            {
                throw new ArgumentException("Cannot find secret for specified token");
            }

            return inMemoryToken;
        }

        public void StoreNewRequestToken(DotNetOpenAuth.OAuth.Messages.UnauthorizedTokenRequest request, DotNetOpenAuth.OAuth.Messages.ITokenSecretContainingMessage response)
        {
            var storage = this.GetTokenStorage();
            InMemoryToken token = new InMemoryToken() { Token = response.Token, TokenSecret = response.TokenSecret, TokenType = TokenType.RequestToken };

            storage[token.Token] = token;
        }

        private IDictionary<string, InMemoryToken> GetTokenStorage()
        {
            AssertSessionState();

            HttpSessionState session = HttpContext.Current.Session;
            IDictionary<string, InMemoryToken> storage = session[SESSION_KEY_TOKENS] as IDictionary<string, InMemoryToken>;

            if (storage == null)
            {
                storage = new Dictionary<string, InMemoryToken>();
                session[SESSION_KEY_TOKENS] = storage;
            }

            return storage;
        }

        private static void AssertSessionState()
        {
            if (HttpContext.Current == null || HttpContext.Current.Session == null)
            {
                throw new InvalidOperationException("Session state is unavailable");
            }
        }

        [Serializable]
        protected class InMemoryToken
        {
            public TokenType TokenType { get; set; }
            public string Token { get; set; }
            public string TokenSecret { get; set; }

            public InMemoryToken()
            {
                this.TokenType = DotNetOpenAuth.OAuth.ChannelElements.TokenType.InvalidToken;
            }
        }
    }
}
