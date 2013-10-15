using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using miiCard.Consumers.Properties;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.ChannelElements;
using miiCard.Consumers.Infrastructure;
using DotNetOpenAuth.OAuth.Messages;
using miiCard.Consumers.Service.v1;
using miiCard.Consumers.Service.v1.Claims;
using System.Configuration;

namespace miiCard.Consumers
{
    /// <summary>
    /// Renders a miiCard user's miiCard image, or provides a login mechanism for them to share identity information with
    /// the site.
    /// </summary>
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:miiCard runat=\"server\"></{0}:miiCard>")]
    public class MiiCard : WebControl, INamingContainer
    {
        /// <summary>
        /// The appSettings configuration key that should be used to configure the consumer key to be used for API calls.
        /// </summary>
        public static readonly string ConfigSettingNameMiiCardConsumerKey = "MiiCardConsumerKey";
        /// <summary>
        /// The appSettings configuration key that should be used to configure the consumer secret to be used for API calls.
        /// </summary>
        public static readonly string ConfigSettingNameMiiCardConsumerSecret = "MiiCardConsumerSecret";

        private static readonly string VIEWSTATE_KEY_TEXT = "Text";
        private static readonly string VIEWSTATE_KEY_LOAD_PROFILE_ON_AUTH = "LoadProfileOnAuth";
        private static readonly string VIEWSTATE_KEY_USER_PROFILE_STORAGE_MODE = "UserProfileStorageMode";
        private static readonly string VIEWSTATE_KEY_LINK_CSS_CLASS = "LinkCssClass";

        /// <summary>
        /// Gets the key that is used when the user's miiCard user profile is stored in session state (if the
        /// UserProfileStorage property is appropriately configured).
        /// </summary>
        public static readonly string UserProfileStorageKey = typeof(MiiCard).FullName + " UserProfile";

        /// <summary>
        /// Occurs when the user elects to login with their miiCard account and that login process completes successfully.
        /// </summary>
        public event EventHandler<MiiCardAuthorisationSuccessEventArgs> AuthorisationSucceeded = delegate { };
        /// <summary>
        /// Occurs when the user elects to login with their miiCard account but that login process fails or the user refuses
        /// to share information with the site.
        /// </summary>
        public event EventHandler<MiiCardAuthorisationFailureEventArgs> AuthorisationFailed = delegate { };

        private string _text;
        /// <summary>
        /// Gets or sets the text that should be shown to the user to direct them to login with their miiCard.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Text
        {
            get
            {
                string s = _text;
                if (this.IsViewStateEnabled)
                {
                    s = (string)ViewState[VIEWSTATE_KEY_TEXT];
                }

                if (string.IsNullOrWhiteSpace(s))
                {
                    return string.Empty;
                }
                else 
                {
                    return s;
                }
            }
            set
            {
                if (this.IsViewStateEnabled)
                {
                    ViewState[VIEWSTATE_KEY_TEXT] = value;
                }
                else
                {
                    this._text = value;
                }
            }
        }

        private string _linkCssClass;
        /// <summary>
        /// Gets or sets the CSS class to be applied to the link that initiates the miiCard authorisation process.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string LinkCssClass
        {
            get
            {
                string s = _linkCssClass;
                if (this.IsViewStateEnabled)
                {
                    s = (string)ViewState[VIEWSTATE_KEY_LINK_CSS_CLASS];
                }

                if (string.IsNullOrWhiteSpace(s))
                {
                    return string.Empty;
                }
                else
                {
                    return s;
                }
            }
            set
            {
                if (this.IsViewStateEnabled)
                {
                    ViewState[VIEWSTATE_KEY_LINK_CSS_CLASS] = value;
                }
                else
                {
                    this._linkCssClass = value;
                }
            }
        }

        private bool _loadUserProfileOnAuthorise = true;
        /// <summary>
        /// Gets or sets whether the control should attempt to retrieve the user's miiCard profile information
        /// upon a successful authorisation process. The storage mechanism used is defined by the value of the
        /// UserProfileStorage property.
        /// </summary>
        [Category("Behaviour")]
        [DefaultValue(true)]
        public bool LoadUserProfileOnAuthorise
        {
            get
            {
                if (this.IsViewStateEnabled)
                {
                    // Default to true unless told otherwise
                    return ((bool?)ViewState[VIEWSTATE_KEY_LOAD_PROFILE_ON_AUTH]) ?? _loadUserProfileOnAuthorise;
                }
                else
                {
                    return _loadUserProfileOnAuthorise;
                }
            }
            set
            {
                if (this.IsViewStateEnabled)
                {
                    ViewState[VIEWSTATE_KEY_LOAD_PROFILE_ON_AUTH] = value;
                }
                else
                {
                    _loadUserProfileOnAuthorise = value;
                }
            }
        }

        private UserProfileStorageMode _userProfileStorage = UserProfileStorageMode.Session;
        /// <summary>
        /// Gets or sets a UserProfileStorageMode describing where a user's miiCard profile information should
        /// be stored when they first authorise its consumption by the hosting site.
        /// </summary>
        public UserProfileStorageMode UserProfileStorage
        {
            get
            {
                if (this.IsViewStateEnabled)
                {
                    // Default to session storage mode unless told otherwise
                    return ((UserProfileStorageMode?)ViewState[VIEWSTATE_KEY_USER_PROFILE_STORAGE_MODE]) ?? _userProfileStorage;
                }
                else
                {
                    return _userProfileStorage;
                }
            }
            set
            {
                ViewState[VIEWSTATE_KEY_USER_PROFILE_STORAGE_MODE] = value;
            }
        }

        private MiiUserProfile _userProfileNonPersisted;
        /// <summary>
        /// Gets or sets the MiiUserProfile whose details should be rendered by the control. If null at the point of rendering,
        /// a link is shown to login with miiCard - otherwise an appropriate miiCard card image shall be shown.
        /// </summary>
        public MiiUserProfile UserProfile
        {
            get
            {
                switch (this.UserProfileStorage)
                {
                    case UserProfileStorageMode.Session:
                        return HttpContext.Current.Session[UserProfileStorageKey] as MiiUserProfile;
                    case UserProfileStorageMode.ViewState:
                        if (!this.IsViewStateEnabled)
                        {
                            throw new InvalidOperationException("ViewState is disabled for the control, but the UserProfileStorage storage property is set to UserProfileStorageMode.ViewState");
                        }

                        return ViewState[UserProfileStorageKey] as MiiUserProfile;
                    default:
                        return _userProfileNonPersisted;
                }
            }
            set
            {
                switch (this.UserProfileStorage)
                {
                    case UserProfileStorageMode.Session:
                        HttpContext.Current.Session[UserProfileStorageKey] = value;
                        break;
                    case UserProfileStorageMode.ViewState:
                        if (!this.IsViewStateEnabled)
                        {
                            throw new InvalidOperationException("ViewState is disabled for the control, but the UserProfileStorage storage property is set to UserProfileStorageMode.ViewState");
                        }

                        ViewState[UserProfileStorageKey] = value;
                        break;
                    default:
                        _userProfileNonPersisted = value;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the URL that should be used for OAuth authorisation.
        /// </summary>
        public string AuthorisationUrl
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the miiCard referrer code that should be used when making requests.
        /// </summary>
        public string ReferrerCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the user should be forced to re-select the information they wish to share with your
        /// application even if they already have a valid relationship with you.
        /// </summary>
        public bool ForceClaimsPicker
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the user should be directed straight to a miiCard signup form (if you believe
        /// </summary>
        public bool SignupMode
        {
            get;
            set;
        }

        private IConsumerTokenManager _tokenManager;
        /// <summary>
        /// Gets or sets the IConsumerTokenManager that should be used to store and subsequently look-up
        /// request and access tokens that are exchanged as part of the OAuth authorisation process. By default
        /// a provider will be used that stores tokens in session state - it is highly recommended that the hosting
        /// site replace this with a persistent mechanism.
        /// </summary>
        public IConsumerTokenManager TokenManager
        {
            get
            {
                return _tokenManager;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("TokenManager cannot be null");
                }

                _tokenManager = value;
            }
        }

        /// <summary>
        /// Initialises a new MiiCard control and configures it with default values.
        /// </summary>
        public MiiCard()
        {
            this.AuthorisationUrl = ServiceUrls.OAuthEndpoint;

            this.SetupDefaultPropertyValues();
        }

        protected override void CreateChildControls()
        {
            if (this.UserProfile != null)
            {
                // Show the miiCard image, making it a hyperlink through to the user's profile page
                HyperLink imageControl = new HyperLink();
                imageControl.ID = "imgMiiCard";
                imageControl.CssClass = "miiCard-card";
                imageControl.NavigateUrl = this.GetMiiCardProfileUrl();
                imageControl.ImageUrl = this.GetMiiCardImageUrl();
                imageControl.Target = "_blank";
                imageControl.ToolTip = string.Format("{0}'s miiCard", this.GetMiiCardAssuredName());

                this.Controls.Add(imageControl);
            }
            else
            {
                // Show a signup link
                LinkButton toRender = new LinkButton();
                toRender.ID = "btnLoginToMiicard";
                toRender.Text = this.Text;
                toRender.CssClass = (this.LinkCssClass + " miiCardLoginButton").Trim();
                toRender.Click += new EventHandler(miiCard_btnLoginToMiicardClick);
                this.Controls.Add(toRender);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            // If we've not yet been supplied a TokenManager then build a session-based one
            if (this.TokenManager == null)
            {
                // First try pulling key and secret information from application settings
                string consumerKey = ConfigurationManager.AppSettings[MiiCard.ConfigSettingNameMiiCardConsumerKey];
                string consumerSecret = ConfigurationManager.AppSettings[MiiCard.ConfigSettingNameMiiCardConsumerSecret];
                
                // We require at least a consumer key be available - if it's not, bail out as there's no further
                // we can realistically go
                if (string.IsNullOrWhiteSpace(consumerKey))
                {
                    throw new InvalidOperationException(
                        string.Format(
                        "The TokenManager was not initialised with suitable consumer key and secret information, and the information could not " +
                        "be found in web.config. Either explicitly specify an IConsumerTokenManager to be used via the TokenManager property, or " +
                        "add appropriate entries to your web.config's appSettings section with key names {0} and {1}.", 
                        MiiCard.ConfigSettingNameMiiCardConsumerKey, 
                        MiiCard.ConfigSettingNameMiiCardConsumerSecret)
                        );
                }

                this.TokenManager = new SessionStateConsumerTokenManager(consumerKey, consumerSecret);
            }

            Page.RegisterRequiresViewStateEncryption();

            var consumer = this.GetConsumer();
            AuthorizedTokenResponse authTokenResponse = null;

            try
            {
                authTokenResponse = consumer.ProcessUserAuthorization();
            }
            catch (Exception ex)
            {
                this.AuthorisationFailed(this, new MiiCardAuthorisationFailureEventArgs(ex));
            }

            if (authTokenResponse != null)
            {
                // We've been successfully authenticated - if we've been configured to do so then pull down the
                // user's profile so that it can be made available to the event handler
                MiiApiResponse<MiiUserProfile> response = null;

                if (this.LoadUserProfileOnAuthorise)
                {
                    var service = new MiiCardOAuthClaimsService(this.TokenManager.ConsumerKey, this.TokenManager.ConsumerSecret, authTokenResponse.AccessToken, this.TokenManager.GetTokenSecret(authTokenResponse.AccessToken));
                    response = service.GetClaims();

                    if (response.Status == MiiApiCallStatus.Success)
                    {
                        // User profile will be stored in the correct location based on the setting of the
                        // this.UserProfileStorage property
                        this.UserProfile = response;
                        this.AuthorisationSucceeded(this, new MiiCardAuthorisationSuccessEventArgs(authTokenResponse.AccessToken, this.TokenManager.GetTokenSecret(authTokenResponse.AccessToken), authTokenResponse.ExtraData, response));
                    }
                    else
                    {
                        this.AuthorisationFailed(this, new MiiCardAuthorisationFailureEventArgs(response.ErrorCode, response.ErrorMessage));
                    }
                }
                else
                {
                    this.AuthorisationSucceeded(this, new MiiCardAuthorisationSuccessEventArgs(authTokenResponse.AccessToken, this.TokenManager.GetTokenSecret(authTokenResponse.AccessToken), authTokenResponse.ExtraData, null));
                }
            }

            base.OnLoad(e);
        }

        protected override object SaveViewState()
        {
            // If we're being asked to save view-state and the host is asking us to store miiCard user profile
            // information in ViewState then bail if it's not encrypted
            if (this.UserProfileStorage == UserProfileStorageMode.ViewState && Page.ViewStateEncryptionMode == ViewStateEncryptionMode.Never)
            {
                throw new InvalidOperationException("Cannot store user profile information in unencrypted view state");
            }

            return base.SaveViewState();
        }

        public override void RenderControl(HtmlTextWriter writer)
        {
            // Avoid calling the base class to avoid spitting a <SPAN> into the output
            this.RenderContents(writer);
        }

        private void SetupDefaultPropertyValues()
        {
            this.ViewState[VIEWSTATE_KEY_TEXT] = this._text = Resources.TEXT_MIICARD_TEXT_DEFAULT;
        }

        void miiCard_btnLoginToMiicardClick(object sender, EventArgs e)
        {
            // Handle sign-in as per old control
            this.BeginAuthentication();
        }

        private string GetMiiCardImageUrl()
        {
            var profile = this.UserProfile;
            if (profile != null)
            {
                return profile.CardImageUrl;
            }
            else
            {
                return null;
            }
        }

        private string GetMiiCardAssuredName()
        {
            var profile = this.UserProfile;
            if (profile != null)
            {
                return (profile.FirstName + " " + profile.LastName).Trim();
            }
            else
            {
                return null;
            }
        }

        private string GetMiiCardProfileUrl()
        {
            var profile = this.UserProfile;
            if (profile != null)
            {
                return profile.ProfileUrl;
            }
            else
            {
                return null;
            }
        }

        public void BeginAuthentication()
        {
            WebConsumer consumer = this.GetConsumer();

            UriBuilder callback = new UriBuilder(HttpContext.Current.Request.Url);
            callback.Query = null;
            string[] scopes = new string[0];
            string scope = string.Join("|", scopes);
            var requestParams = new Dictionary<string, string> { { "scope", scope }, };

            var redirectParams = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(this.ReferrerCode))
            {
                redirectParams[MiiCardConsumer.OAUTH_PARAM_REFERRER_CODE] = this.ReferrerCode;
            }

            if (this.ForceClaimsPicker)
            {
                redirectParams[MiiCardConsumer.OAUTH_PARAM_FORCE_CLAIMS_PICKER] = "true";
            }

            if (this.SignupMode)
            {
                redirectParams[MiiCardConsumer.OAUTH_PARAM_SIGNUP_MODE] = "true";
            }

            if (redirectParams.Count == 0)
            {
                redirectParams = null;
            }

            var request = consumer.PrepareRequestUserAuthorization(callback.Uri, requestParams, redirectParams);
            consumer.Channel.Send(request);
        }

        private WebConsumer GetConsumer()
        {
            MessageReceivingEndpoint oauthEndpoint = new MessageReceivingEndpoint(new Uri(this.AuthorisationUrl), HttpDeliveryMethods.PostRequest);
            ServiceProviderDescription serviceDescription = new ServiceProviderDescription()
            {
                RequestTokenEndpoint = oauthEndpoint,
                UserAuthorizationEndpoint = oauthEndpoint,
                AccessTokenEndpoint = oauthEndpoint,
                TamperProtectionElements = new DotNetOpenAuth.Messaging.ITamperProtectionChannelBindingElement[] 
                {
                    new HmacSha1SigningBindingElement()
                }
            };

            return new WebConsumer(serviceDescription, this.TokenManager);
        }

        /// <summary>
        /// Specifies where a user's miiCard profile information should be temporarily stored
        /// immediately after they authorise the hosting site's consumption of their identity data.
        /// </summary>
        public enum UserProfileStorageMode
        {
            /// <summary>
            /// Specifies that the user's profile should be stored in memory only - it will become unavailable 
            /// the next time the control is rendered.
            /// </summary>
            Memory,
            /// <summary>
            /// Specifies that the user's profile should be stored in session state.
            /// </summary>
            Session,
            /// <summary>
            /// Specifies that the user's profile should be serialised into the control's ViewState. The caller must ensure
            /// that ViewState is encrypted, or an exception will be thrown at runtime when the user profile is stored.
            /// </summary>
            ViewState
        }
    }
}
