using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace miiCard.Consumers.TestHarness.Models
{
    public class HarnessViewModel
    {
        [Required]
        [Display(Name="Consumer key")]
        public string ConsumerKey { get; set; }
        [Required]
        [Display(Name = "Consumer secret")]
        public string ConsumerSecret { get; set; }

        [Display(Name = "Access token")]
        public string AccessToken { get; set; }
        [Display(Name = "Access token secret")]
        public string AccessTokenSecret { get; set; }

        [Display(Name = "Referrer code (if any)")]
        public string ReferrerCode { get; set; }

        [Display(Name = "Force claims picker")]
        public bool ForceClaimsPicker { get; set; }

        [Display(Name = "Signup mode (initially redirect to a signup, rather than a login page)")]
        public bool SignupMode { get; set; }

        public string LastGetClaimsResult { get; set; }
        public string LastIsUserAssuredResult { get; set; }
        public string LastIsSocialAccountAssuredResult { get; set; }
        public string LastGetIdentitySnapshotDetailsResult { get; set; }
        public string LastGetIdentitySnapshotResult { get; set; }
        public string LastGetFinancialTransactionsResult { get; set; }
        public string LastGetAuthenticationDetailsResult { get; set; }
        public string LastRefreshFinancialDataResult { get; set; }
        public string LastIsRefreshInProgressResult { get; set; }
        public string LastDirectorySearchResult { get; set; }

        public bool ShowAssuranceImage { get; set; }
        public string AssuranceImageType { get; set; }

        public bool ShowCardImage { get; set; }
        public string CardImageFormat { get; set; }
        public string CardImageSnapshotId { get; set; }
        public bool CardImageShowEmailAddress { get; set; }
        public bool CardImageShowPhoneNumber { get; set; }

        public string SocialAccountId { get; set; }
        public string SocialAccountType { get; set; }

        public string SnapshotDetailsId { get; set; }
        public string SnapshotId { get; set; }
        public string SnapshotPdfId { get; set; }
        public string AuthenticationDetailsSnapshotId { get; set; }
        
        public bool ShowOAuthDetailsRequiredError { get; set; }
        public string OAuthProcessErrorText { get; set; }

        [Display(Name = "Hide values absolutely greater than this for modesty (blank to disable)")]
        public decimal? FinancialDataModestyLimit { get; set; }

        public string DirectoryCriterion { get; set; }
        public string DirectoryCriterionValue { get; set; }
        public bool DirectoryCriterionValueHashed { get; set; }

        public HarnessViewModel()
        {
            this.FinancialDataModestyLimit = 100m;
        }
    }
}