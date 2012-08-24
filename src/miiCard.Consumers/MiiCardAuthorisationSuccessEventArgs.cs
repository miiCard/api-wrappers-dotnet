using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using miiCard.Consumers.Service.v1.Claims;

namespace miiCard.Consumers
{
    /// <summary>
    /// Contains data associated with a user's successful authorisation with miiCard.
    /// </summary>
    public class MiiCardAuthorisationSuccessEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the access token to be used in subsequent OAuth-authorised API requests.
        /// </summary>
        public string AccessToken { get; private set; }
        /// <summary>
        /// Gets the access token secret to be used in subsequent OAuth-authorised API requests.
        /// </summary>
        public string AccessTokenSecret { get; private set; }
        /// <summary>
        /// Gets any additional information that was passed back with the authorisation response.
        /// </summary>
        public IDictionary<string, string> ExtraInfo { get; private set; }

        /// <summary>
        /// Gets the miiCard user's profile information.
        /// </summary>
        public MiiUserProfile UserProfile { get; private set; }

        public MiiCardAuthorisationSuccessEventArgs(string accessToken, string accessTokenSecret, IDictionary<string, string> extraInfo, MiiUserProfile userProfile)
        {
            this.AccessToken = accessToken;
            this.AccessTokenSecret = accessTokenSecret;

            this.ExtraInfo = extraInfo;
            this.UserProfile = userProfile;
        }
    }
}
