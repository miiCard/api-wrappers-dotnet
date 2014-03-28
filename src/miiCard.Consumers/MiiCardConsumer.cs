using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.ChannelElements;

namespace miiCard.Consumers
{
    public static class MiiCardConsumer
    {
        public static readonly string OAUTH_PARAM_FORCE_CLAIMS_PICKER = "force_claims";
        public static readonly string OAUTH_PARAM_SIGNUP_MODE = "signup";

        public static ServiceProviderDescription ServiceDescription
        {
            get 
            { 
                // Return a fresh copy each time to avoid the caller accidentally corrupting state
                // by modifying the returned description's fields
                return MiiCardConsumer.GetServiceProviderDescription(new Uri(ServiceUrls.OAuthEndpoint)); 
            }
        }

        public static WebConsumer GetConsumer(IConsumerTokenManager tokenManager)
        {
            return MiiCardConsumer.GetConsumer(MiiCardConsumer.ServiceDescription, tokenManager);
        }

        public static WebConsumer GetConsumer(ServiceProviderDescription serviceProviderDescription, IConsumerTokenManager tokenManager)
        {
            return new WebConsumer(serviceProviderDescription, tokenManager);
        }

        public static WebConsumer GetConsumer(Uri oauthUri, IConsumerTokenManager tokenManager)
        {
            return new WebConsumer(GetServiceProviderDescription(oauthUri), tokenManager);
        }

        private static ServiceProviderDescription GetServiceProviderDescription(Uri oauthUri)
        {
            var deliveryMethods = DotNetOpenAuth.Messaging.HttpDeliveryMethods.AuthorizationHeaderRequest | DotNetOpenAuth.Messaging.HttpDeliveryMethods.PostRequest;
            var endpoint = new DotNetOpenAuth.Messaging.MessageReceivingEndpoint(oauthUri.ToString(), deliveryMethods);

            var serviceDescription = new DotNetOpenAuth.OAuth.ServiceProviderDescription()
            {
                AccessTokenEndpoint = endpoint,
                RequestTokenEndpoint = endpoint,
                UserAuthorizationEndpoint = endpoint,

                TamperProtectionElements = new DotNetOpenAuth.Messaging.ITamperProtectionChannelBindingElement[] {
                    new DotNetOpenAuth.OAuth.ChannelElements.HmacSha1SigningBindingElement()
                }
            };

            return serviceDescription;
        }
    }
}
