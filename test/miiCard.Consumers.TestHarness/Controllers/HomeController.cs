using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using miiCard.Consumers.TestHarness.Extensions;
using miiCard.Consumers.TestHarness.Models;

namespace miiCard.Consumers.TestHarness.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View(new HarnessViewModel());
        }

        [HttpPost]
        public ActionResult Index(HarnessViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(this.Request.Params["btn-invoke"]))
            {
                if (string.IsNullOrWhiteSpace(model.ConsumerKey) || string.IsNullOrWhiteSpace(model.ConsumerSecret) || string.IsNullOrWhiteSpace(model.AccessToken) || string.IsNullOrWhiteSpace(model.AccessTokenSecret))
                {
                    model.ShowOAuthDetailsRequiredError = true;
                }
                else
                {
                    var apiWrapper = new miiCard.Consumers.Service.v1.MiiCardOAuthClaimsService(model.ConsumerKey, model.ConsumerSecret, model.AccessToken, model.AccessTokenSecret, null, new System.ServiceModel.EndpointAddress("https://stsbeta.miicard.com/api/v1/Claims.svc"));

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
            return View("Index", model);   
        }

        public ActionResult HandleLoginWithMiiCard()
        {
            return View("Index");
        }
    }
}
