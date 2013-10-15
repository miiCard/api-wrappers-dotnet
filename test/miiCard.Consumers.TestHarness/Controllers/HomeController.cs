using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.OAuth.Messages;
using miiCard.Consumers.Infrastructure;
using miiCard.Consumers.Service.v1;
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
                // Cover off certain edge cases where the test harness is started then restarted
                // killing session state - we need a session active to perform the OAuth exchange and the
                // redirect that LoginWithMiiCard causes can prevent that from happening unless it's in place
                // before it's called
                if (!(Session["Running"] as bool? ?? false))
                {
                    Session["Running"] = true;
                    return View(model);
                }
                else
                {
                    return this.LoginWithMiiCard(model);
                }
            }

            if (!string.IsNullOrWhiteSpace(this.Request.Params["btn-invoke"]))
            {
                if (this.Request.Params["btn-invoke"] == "directory-search")
                {
                    var response = new MiiCardDirectoryService().FindBy(model.DirectoryCriterion, model.DirectoryCriterionValue, model.DirectoryCriterionValueHashed);
                    if (response != null)
                    {
                        model.LastDirectorySearchResult = MiiApiResponseExtensions.RenderUserProfile(response);
                    }
                }
                else if (string.IsNullOrWhiteSpace(model.ConsumerKey) || string.IsNullOrWhiteSpace(model.ConsumerSecret) || string.IsNullOrWhiteSpace(model.AccessToken) || string.IsNullOrWhiteSpace(model.AccessTokenSecret))
                {
                    model.ShowOAuthDetailsRequiredError = true;
                }
                else
                {
                    var apiWrapper = new MiiCardOAuthClaimsService(model.ConsumerKey, model.ConsumerSecret, model.AccessToken, model.AccessTokenSecret);
                    var financialWrapper = new MiiCardOAuthFinancialService(model.ConsumerKey, model.ConsumerSecret, model.AccessToken, model.AccessTokenSecret);
                    
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
                        case "card-image":
                            model.ShowCardImage = true;
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
                        case "get-identity-snapshot-pdf":
                            if (!string.IsNullOrWhiteSpace(model.SnapshotPdfId))
                            {
                                return new FileStreamResult(apiWrapper.GetIdentitySnapshotPdf(model.SnapshotPdfId), "application/pdf")
                                { 
                                    FileDownloadName = model.SnapshotPdfId 
                                };
                            }
                            break;
                        case "get-authentication-details":
                            model.LastGetAuthenticationDetailsResult = apiWrapper.GetAuthenticationDetails(model.AuthenticationDetailsSnapshotId).Prettify();
                            break;
                        case "refresh-financial-data":
                            model.LastRefreshFinancialDataResult = financialWrapper.RefreshFinancialData().Prettify();
                            break;
                        case "is-refresh-in-progress":
                            model.LastIsRefreshInProgressResult = financialWrapper.IsRefreshInProgress().Prettify();
                            break;
                        case "get-financial-transactions": 
                            var configuration = new PrettifyConfiguration() { ModestyLimit = model.FinancialDataModestyLimit };
                            model.LastGetFinancialTransactionsResult = financialWrapper.GetFinancialTransactions().Prettify(configuration);
                            break;
                    }
                }
            }
            
            return View(model);
        }

        public ActionResult SHA1(string identifier)
        {
            return new ContentResult()
            {
                Content = MiiCardDirectoryService.HashIdentifier(identifier),
                ContentType = "text/plain"
            };
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
                    redirectParams[MiiCardConsumer.OAUTH_PARAM_REFERRER_CODE] = model.ReferrerCode;
                }

                if (model.ForceClaimsPicker)
                {
                    // Cause the claims picker to be shown even if a valid relationship exists between the
                    // relying party described by the consumer key and the user who logs in
                    // Note: skipping the claims picker in the situation described needs to be enabled by
                    // miiCard - please contact support if you think you want to make use of this feature
                    redirectParams[MiiCardConsumer.OAUTH_PARAM_FORCE_CLAIMS_PICKER] = "true";
                }

                if (model.SignupMode)
                {
                    redirectParams[MiiCardConsumer.OAUTH_PARAM_SIGNUP_MODE] = "true";
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

        public FileStreamResult CardImage(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, string snapshotId, string format, bool showEmailAddress, bool showPhoneNumber)
        {
            var apiWrapper = new miiCard.Consumers.Service.v1.MiiCardOAuthClaimsService(consumerKey, consumerSecret, accessToken, accessTokenSecret);

            FileStreamResult result = null;
            try
            {
                var configuration = new Service.v1.Claims.CardImageConfiguration()
                {
                    Format = format,
                    ShowEmailAddress = showEmailAddress,
                    ShowPhoneNumber = showPhoneNumber,
                    SnapshotId = snapshotId
                };

                result = new FileStreamResult(apiWrapper.GetCardImage(configuration), "image/png");
            }
            catch (Exception)
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
