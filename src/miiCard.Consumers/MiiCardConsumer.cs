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
        public static ServiceProviderDescription ServiceDescription
        {
            get 
            { 
                // Return a fresh copy each time to avoid the caller accidentally corrupting state
                // by modifying the returned description's fields
                return MiiCardConsumer.GetServiceProviderDescription(); 
            }
        }

        public static WebConsumer GetConsumer(IConsumerTokenManager tokenManager)
        {
            return new WebConsumer(MiiCardConsumer.ServiceDescription, tokenManager);
        }

        private static ServiceProviderDescription GetServiceProviderDescription()
        {
            var deliveryMethods = DotNetOpenAuth.Messaging.HttpDeliveryMethods.AuthorizationHeaderRequest | DotNetOpenAuth.Messaging.HttpDeliveryMethods.PostRequest;
            var endpoint = new DotNetOpenAuth.Messaging.MessageReceivingEndpoint(ServiceUrls.OAuthEndpoint, deliveryMethods);

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
