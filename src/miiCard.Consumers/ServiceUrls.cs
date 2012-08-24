using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace miiCard.Consumers
{
    /// <summary>
    /// Houses the service URLs used by the miiCard API wrapper and controls
    /// </summary>
    public static class ServiceUrls
    {
        private static readonly string _oAuthEndpoint = "https://sts.miicard.com/auth/OAuth.ashx";
        private static readonly string _claimsServiceEndpoint = "https://sts.miicard.com/api/v1/Claims.svc";

        /// <summary>
        /// Gets the endpoint URL used for OAuth authorisation
        /// </summary>
        public static string OAuthEndpoint
        {
            get { return _oAuthEndpoint; }
        }

        /// <summary>
        /// Gets the endpoint URL used for the Claims service
        /// </summary>
        public static string ClaimsServiceEndpoint
        {
            get { return _claimsServiceEndpoint; }
        }
    }
}
