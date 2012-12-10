using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.OAuth.Messages;
using miiCard.Consumers.Infrastructure;
using miiCard.Consumers.TestHarness.Extensions;
using miiCard.Consumers.TestHarness.Models;

namespace miiCard.Consumers.TestHarness.Controllers
{
    public class HomeController : Controller
    {
        private static readonly string SESSION_KEY_CONSUMER_KEY = typeof(HomeController).FullName + ".OAuth.ConsumerKey";
        private static readonly string SESSION_KEY_CONSUMER_SECRET = typeof(HomeController).FullName + ".OAuth.ConsumerSecret";

        [HttpGet]
        public ActionResult Index()
        {
            // Force a session cookie down on the initial view, otherwise
            // we'll not create it correctly when doing a login
            Session["Running"] = true;
         
            return View(new HarnessViewModel());
        }

        [HttpPost]
        public ActionResult Index(HarnessViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(Request.Params["btnLoginWithMiiCard"]))
            {
                return this.LoginWithMiiCard(model);
            }

            if (!string.IsNullOrWhiteSpace(this.Request.Params["btn-invoke"]))
            {
                if (string.IsNullOrWhiteSpace(model.ConsumerKey) || string.IsNullOrWhiteSpace(model.ConsumerSecret) || string.IsNullOrWhiteSpace(model.AccessToken) || string.IsNullOrWhiteSpace(model.AccessTokenSecret))
                {
                    model.ShowOAuthDetailsRequiredError = true;
                }
                else
                {
                    var apiWrapper = new miiCard.Consumers.Service.v1.MiiCardOAuthClaimsService(model.ConsumerKey, model.ConsumerSecret, model.AccessToken, model.AccessTokenSecret);

                    switch (this.Request.Params["btn-invoke"])
                    {
                        case "get-claims":
                            model.LastGetClaimsResult = apiWrapper.GetClaims().Prettify();
                            break;
                        case "is-user-assured":
                            model.LastIsUserAssuredResult = apiWrapper.IsUserAssured().Prettify();
                            break;
                        case "is-social-account-assured":
                            if (!string.IsNullOrWhiteSpace(model.SocialAccountId) && !string.IsNullOrWhiteSpace(model.SocialAccountType))
                            {
                                model.LastIsSocialAccountAssuredResult = apiWrapper.IsSocialAccountAssured(model.SocialAccountId, model.SocialAccountType).Prettify();
                            }
                            break;
                        case "assurance-image":
                            if (!string.IsNullOrWhiteSpace(model.AssuranceImageType))
                            {
                                model.ShowAssuranceImage = true;
                            }
                            break;
                        case "get-identity-snapshot-details":
                            model.LastGetIdentitySnapshotDetailsResult = apiWrapper.GetIdentitySnapshotDetails(model.SnapshotDetailsId).Prettify();
                            break;
                        case "get-identity-snapshot":
                            if (!string.IsNullOrWhiteSpace(model.SnapshotId))
                            {
                                model.LastGetIdentitySnapshotResult = apiWrapper.GetIdentitySnapshot(model.SnapshotId).Prettify();
                            }
                            break;
                    }
                }
            }
            
            return View(model);
        }

        public ActionResult LoginWithMiiCard(HarnessViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                Session[SESSION_KEY_CONSUMER_KEY] = model.ConsumerKey;
                Session[SESSION_KEY_CONSUMER_SECRET] = model.ConsumerSecret;

                var tokenManager = this.GetTokenManager();
                var consumer = miiCard.Consumers.MiiCardConsumer.GetConsumer(tokenManager);

                string redirectUrl = Url.Action("HandleLoginWithMiiCard", RouteData.Values["controller"].ToString(), null, "http");

                var redirectParams = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(model.ReferrerCode))
                {
                    // Tack in the referrer code to see how this manifests in the signup process
                    redirectParams["referrer"] = model.ReferrerCode;
                }

                if (model.ForceClaimsPicker)
                {
                    // Cause the claims picker to be shown even if a valid relationship exists between the
                    // relying party described by the consumer key and the user who logs in
                    // Note: skipping the claims picker in the situation described needs to be enabled by
                    // miiCard - please contact support if you think you want to make use of this feature
                    redirectParams["force_claims"] = "true";
                }

                if (redirectParams.Count == 0)
                {
                    redirectParams = null;
                }

                var request = consumer.PrepareRequestUserAuthorization(new Uri(redirectUrl), null, redirectParams);
                consumer.Channel.Send(request);
            }

            return View("Index", model);
        }

        public ActionResult HandleLoginWithMiiCard()
        {
            var tokenManager = this.GetTokenManager();
            var consumer = miiCard.Consumers.MiiCardConsumer.GetConsumer(tokenManager);

            var model = new HarnessViewModel();

            AuthorizedTokenResponse response = null;
            try
            {
                response = consumer.ProcessUserAuthorization();
            }
            catch (Exception ex)
            {
                model.OAuthProcessErrorText = ex.ToString();
            }

            if (response != null)
            {
                model.AccessToken = response.AccessToken;
                model.AccessTokenSecret = tokenManager.GetTokenSecret(model.AccessToken);
            }

            model.ConsumerKey = Session[SESSION_KEY_CONSUMER_KEY] as string;
            model.ConsumerSecret = Session[SESSION_KEY_CONSUMER_SECRET] as string;

            return View("Index", model);
        }

        public FileStreamResult AssuranceImage(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, string type)
        {
            var apiWrapper = new miiCard.Consumers.Service.v1.MiiCardOAuthClaimsService(consumerKey, consumerSecret, accessToken, accessTokenSecret);

            FileStreamResult result = null;
            try
            {
                result = new FileStreamResult(apiWrapper.AssuranceImage(type), "image/png");
            }
            catch (Exception ex)
            {
                result = new FileStreamResult(new MemoryStream(), "image/png");
            }

            return result;
        }

        private SessionStateConsumerTokenManager GetTokenManager()
        {
            var consumerKey = Session[SESSION_KEY_CONSUMER_KEY] as string;
            var consumerSecret = Session[SESSION_KEY_CONSUMER_SECRET] as string;

            if (string.IsNullOrWhiteSpace(consumerKey) || string.IsNullOrWhiteSpace(consumerSecret))
            {
                throw new InvalidOperationException("Consumer key or secret information not found");
            }

            return new SessionStateConsumerTokenManager(consumerKey, consumerSecret);
        }
    }
}
