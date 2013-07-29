using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenAuth.OAuth.ChannelElements;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel;
using DotNetOpenAuth.OAuth.Messages;
using miiCard.Consumers.Service.v1.Claims;

namespace miiCard.Consumers.Service.v1
{
    /// <summary>
    /// Base class for miiCard API wrappers that perform OAuth-authorised requests to the API.
    /// </summary>
    public abstract class MiiCardOAuthServiceBase
    {
        /// <summary>
        /// Gets the access token used in the request.
        /// </summary>
        public string AccessToken { get; private set; }

        private Binding _specificBinding;
        private Binding _defaultBinding;

        protected Binding Binding 
        {
            get
            {
                if (this.UseDeclarativeBindingConfiguration)
                {
                    return null;
                }
                else
                {
                    return this._specificBinding ?? this._defaultBinding;
                }
            }
        }

        private EndpointAddress _specificEndpointAddress;
        protected EndpointAddress EndpointAddress 
        {
            get
            {
                return _specificEndpointAddress ?? this.GetDefaultEndpointAddress();
            }
        }

        /// <summary>
        /// Gets or sets whether the service wrapper should use declarative bindings or programmatic.
        /// </summary>
        public bool UseDeclarativeBindingConfiguration { get; set; }

        private IConsumerTokenManager _tokenManager;

        public MiiCardOAuthServiceBase(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
            : this(consumerKey, consumerSecret, accessToken, accessTokenSecret, false, null, null)
        {

        }

        public MiiCardOAuthServiceBase(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, bool useDeclarativeBindings)
            : this(consumerKey, consumerSecret, accessToken, accessTokenSecret, useDeclarativeBindings, null, null)
        {

        }

        public MiiCardOAuthServiceBase(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, Binding binding, EndpointAddress endpointAddress)
            : this(consumerKey, consumerSecret, accessToken, accessTokenSecret, false, binding, endpointAddress)
        {

        }

        protected MiiCardOAuthServiceBase(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, bool useDeclarativeBinding, Binding binding, EndpointAddress endpointAddress)
        {
            this.AccessToken = accessToken;

            this._tokenManager = new SingleAccessTokenManager(consumerKey, consumerSecret, AccessToken, accessTokenSecret);

            this._defaultBinding = this.GetDefaultBinding();
            this._specificBinding = binding;
            this._specificEndpointAddress = endpointAddress;

            this.UseDeclarativeBindingConfiguration = useDeclarativeBinding;
        }

        protected virtual Binding GetDefaultBinding()
        {
            // Our default binding is an HTTPS SOAP request
            BasicHttpBinding toReturn = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
            toReturn.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;

            // Allow 60 seconds for all network operations by default
            toReturn.CloseTimeout = toReturn.OpenTimeout = toReturn.SendTimeout = toReturn.ReceiveTimeout = TimeSpan.FromSeconds(60);
            toReturn.AllowCookies = true;
            toReturn.MaxReceivedMessageSize = 1 * 1024 * 1024; // Allow up to 1MB of incoming message
            toReturn.ReaderQuotas.MaxDepth = int.MaxValue;
            toReturn.ReaderQuotas.MaxArrayLength = int.MaxValue;
            toReturn.TransferMode = TransferMode.Buffered;
            
            return toReturn;
        }

        protected abstract EndpointAddress GetDefaultEndpointAddress();

        protected HttpRequestMessageProperty GetOAuthHeader(Uri endpointUri, bool post)
        {
            WebConsumer consumer = this.GetConsumer();
            MessageReceivingEndpoint endpoint = new MessageReceivingEndpoint(endpointUri, HttpDeliveryMethods.AuthorizationHeaderRequest | (post ? HttpDeliveryMethods.PostRequest : HttpDeliveryMethods.GetRequest));

            // Prepare a request so that we can get the requisite Authorization header (which we'll return)
            // but leave it to the caller to figure out what to do with it
            WebRequest authorisedRequest = consumer.PrepareAuthorizedRequest(endpoint, this.AccessToken);

            // Give the caller the headers we'd have sent if it were up to us to make the invocation:
            // they can then transplant them into the actual request they end up making.
            HttpRequestMessageProperty toReturn = new HttpRequestMessageProperty();
            toReturn.Headers[HttpRequestHeader.Authorization] = authorisedRequest.Headers[HttpRequestHeader.Authorization];

            return toReturn;
        }

        private WebConsumer GetConsumer()
        {
            ServiceProviderDescription serviceDescription = new ServiceProviderDescription()
            {
                TamperProtectionElements = new DotNetOpenAuth.Messaging.ITamperProtectionChannelBindingElement[] 
                {
                    new HmacSha1SigningBindingElement()
                }
            };

            return new WebConsumer(serviceDescription, this._tokenManager);
        }

        private class SingleAccessTokenManager : IConsumerTokenManager
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
                // This should never be called
                // TODO: Throw an exception here?
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
                // This method should never be ca lled
                // TODO: Throw an exception here?
            }

            private void AssertValidToken(string token)
            {
                if (token != this.AccessToken)
                {
                    throw new InvalidOperationException("The specified token is not managed by this instance");
                }
            }
        }
    }
}
