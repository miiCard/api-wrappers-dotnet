using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using miiCard.Consumers.Service.v1;
using miiCard.Consumers.Service.v1.Claims;
using miiCard.Consumers.Service.v1.Financial;
using miiCard.Consumers.TestHarness.Models;

namespace miiCard.Consumers.TestHarness.Extensions
{
    public static class MiiApiResponseExtensions
    {
        static Func<object, PrettifyConfiguration, string> DEFAULT_RENDERER = (value, config) => RenderFact("Data", value);

        static Dictionary<Type, Func<object, PrettifyConfiguration, string>> RENDERERS = GetRenderers();

        private static Dictionary<Type, Func<object, PrettifyConfiguration, string>> GetRenderers()
        {
            var toReturn = new Dictionary<Type, Func<object, PrettifyConfiguration, string>>();

            toReturn[typeof(MiiUserProfile)] = (value, config) => RenderUserProfile(value as MiiUserProfile);
            toReturn[typeof(IdentitySnapshot)] = (value, config) => RenderIdentitySnapshot(value as IdentitySnapshot);
            toReturn[typeof(IdentitySnapshotDetails)] = (value, config) => RenderIdentitySnapshotDetails(value as IdentitySnapshotDetails);
            toReturn[typeof(MiiFinancialData)] = (value, config) => RenderFinancialData(value as MiiFinancialData, config);
            toReturn[typeof(AuthenticationDetails)] = (value, config) => RenderAuthenticationDetails(value as AuthenticationDetails);
            toReturn[typeof(FinancialRefreshStatus)] = (value, config) => RenderFinancialRefreshStatus(value as FinancialRefreshStatus);
            toReturn[typeof(Qualification)] = (value, config) => RenderQualification(value as Qualification);

            return toReturn;
        }
        
        public static string Prettify<T>(this MiiApiResponse<T> response, PrettifyConfiguration configuration = null)
        {
            if (configuration == null)
            {
                configuration = new PrettifyConfiguration();
            }

            string toReturn = "<div class='response'>";
        
		    toReturn += RenderFact("Status", response.Status);
            toReturn += RenderFact("Error code", response.ErrorCode);
            toReturn += RenderFact("Error message", response.ErrorMessage);
            toReturn += RenderFact("Is a test user?", response.IsTestUser);

            if (response.Data == null)
            {
                toReturn += RenderFact("Data", null);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(typeof(T)) && typeof(T) != typeof(string))
            {
                int ct = 0;

                foreach (var obj in response.Data as IEnumerable)
                {
                    toReturn += "<div class='fact'><h4>[" + ct.ToString() + "]</h4>";

                    Func<object, PrettifyConfiguration, string> renderer = DEFAULT_RENDERER;
                    if (obj != null)
                    {
                        if (!RENDERERS.TryGetValue(obj.GetType(), out renderer))
                        {
                            renderer = DEFAULT_RENDERER;
                        }
                    }

                    toReturn += renderer(obj, configuration);

                    toReturn += "</div>";
                    ct++;
                }
            }
            else
            {
                Func<object, PrettifyConfiguration, string> renderer = null;
                if (!RENDERERS.TryGetValue(typeof(T), out renderer))
                {
                    renderer = DEFAULT_RENDERER;
                }

                toReturn += renderer(response.Data, configuration);
            }
        
            toReturn += "</div>";

            return toReturn;
        }
        
        private static string RenderFact(string key, object value)
        {
            string factValueRender = "[Empty]";

            if (value != null)
            {
                factValueRender = value.ToString();
            }

            return "<div class='fact-row'><span class='fact-name'>"
                + key
                + "</span><span class='fact-value'>"
                + factValueRender
                + "</span></div>";
        }

        private static string RenderIdentity(Identity identity) 
        {
    	    string toReturn = "<div class='fact'>";
        
            toReturn += RenderFact("Source", identity.Source);
            toReturn += RenderFact("User ID", identity.UserId);
            toReturn += RenderFact("Profile URL", identity.ProfileUrl);
            toReturn += RenderFact("Verified?", identity.Verified);
            toReturn += "</div>";
        
            return toReturn;
        }

        private static string RenderEmail(EmailAddress email) 
        {        
    	    string toReturn = "<div class='fact'>";
        
            toReturn += RenderFact("Display name", email.DisplayName);
            toReturn += RenderFact("Address", email.Address);
            toReturn += RenderFact("Is primary?", email.IsPrimary);
            toReturn += RenderFact("Verified?", email.Verified);
            toReturn += "</div>";
        
            return toReturn;
        }

        private static string RenderAddress(PostalAddress address) 
        {
            string toReturn = "<div class='fact'>";
        
            toReturn += RenderFact("House", address.House);
            toReturn += RenderFact("Line1", address.Line1);
            toReturn += RenderFact("Line2", address.Line2);
            toReturn += RenderFact("City", address.City);
            toReturn += RenderFact("Region", address.Region);
            toReturn += RenderFact("Code", address.Code);
            toReturn += RenderFact("Country", address.Country);
            toReturn += RenderFact("Is primary?", address.IsPrimary);
            toReturn += RenderFact("Verified?", address.Verified);        
            toReturn += "</div>";
        
            return toReturn;   
        }

        private static string RenderPhone(PhoneNumber number)
        {
            string toReturn = "<div class='fact'>";

            toReturn += RenderFact("Display name", number.DisplayName);
            toReturn += RenderFact("Country code", number.CountryCode);
            toReturn += RenderFact("National number", number.NationalNumber);
            toReturn += RenderFact("Is mobile?", number.IsMobile);
            toReturn += RenderFact("Is primary?", number.IsPrimary);
            toReturn += RenderFact("Verified?", number.Verified);
            toReturn += "</div>";

            return toReturn;
        }

        private static string RenderAuthenticationDetails(AuthenticationDetails authenticationDetails)
        {
            string toReturn = "<div class='fact'>";
            toReturn += RenderFactHeading("Authentication details");

            toReturn += RenderFact("Timestamp UTC", authenticationDetails.AuthenticationTimeUtc);
            toReturn += RenderFact("2FA type", authenticationDetails.SecondFactorTokenType);
            toReturn += RenderFact("2FA provider", authenticationDetails.SecondFactorProvider);

            toReturn += "<div class='fact'>";
            toReturn += RenderFactHeading("Locations");

            int ct = 0;
            if (authenticationDetails.Locations != null && authenticationDetails.Locations.Any())
            {
                foreach (var location in authenticationDetails.Locations)
                {
                    toReturn += "<div class='fact'><h4>[" + ct++ + "]</h4>";
                    toReturn += RenderGeographicLocation(location);
                    toReturn += "</div>";
                }
            }
            else
            {
                toReturn += "<p><i>No locations</i></p>";
            }

            toReturn += "</div></div>";

            return toReturn;
        }

        private static string RenderGeographicLocation(GeographicLocation location)
        {
            string toReturn = "<div class='fact'>";

            toReturn += RenderFact("Provider", location.LocationProvider);
            toReturn += RenderFact("Latitude", location.Latitude);
            toReturn += RenderFact("Longitude", location.Longitude);
            toReturn += RenderFact("Accuracy (metres, est.)", location.LatLongAccuracyMetres);

            if (location.ApproximateAddress != null)
            {
                toReturn += RenderFactHeading("Approximate postal address");

                toReturn += RenderAddress(location.ApproximateAddress);
            }
            else
            {
                toReturn += RenderFact("Approximate postal address", null);
            }

            toReturn += "</div>";
            return toReturn;
        }

        public static string RenderFinancialProvider(FinancialProvider financialProvider, PrettifyConfiguration configuration)
        {
            string toReturn = "<div class='fact'>";

            toReturn += RenderFact("Name", financialProvider.ProviderName);

            toReturn += RenderFactHeading("Financial Accounts");

            int ct = 0;
            if (financialProvider.FinancialAccounts != null)
            {
                foreach (var account in financialProvider.FinancialAccounts)
                {
                    toReturn += "<div class='fact'><h4>[" + ct++ + "]</h4>";
                    toReturn += RenderFinancialAccount(account, configuration);
                    toReturn += "</div>";
                }
            }

            toReturn += "</div>";

            return toReturn;
        }

        private static string GetModestyFilteredAmount(decimal? value, PrettifyConfiguration configuration)
        {
            string toReturn = string.Empty;

            if (value.HasValue)
            {
                if (!configuration.ModestyLimit.HasValue || Math.Abs(value.Value) <= configuration.ModestyLimit.Value)
                {
                    toReturn = value.Value.ToString("n2");
                }
                else
                {
                    toReturn = "?.??";
                }
            }

            return toReturn;
        }

        private static string RenderFinancialAccount(FinancialAccount account, PrettifyConfiguration configuration)
        {
            string toReturn = "<div class='fact'>";

            toReturn += RenderFact("Holder", account.Holder);
            toReturn += RenderFact("Account number", account.AccountNumber);
            toReturn += RenderFact("Sort code", account.SortCode);
            toReturn += RenderFact("Account name", account.AccountName);
            toReturn += RenderFact("Type", account.Type);
            toReturn += RenderFact("Last updated", account.LastUpdatedUtc);
            toReturn += RenderFact("Currency", account.CurrencyIso);
            toReturn += RenderFact("Closing balance", GetModestyFilteredAmount(account.ClosingBalance, configuration));
            toReturn += RenderFact("Credits (count)", account.CreditsCount);
            toReturn += RenderFact("Credits (sum)", GetModestyFilteredAmount(account.CreditsSum, configuration));
            toReturn += RenderFact("Debits (count)", account.DebitsCount);
            toReturn += RenderFact("Debits (sum)", GetModestyFilteredAmount(account.DebitsSum, configuration));

            toReturn += RenderFactHeading("Transactions");

            toReturn += "<table class='table table-striped table-condensed table-hover'><thead><tr><th>Date</th><th>Description</th><th class='r'>Credit</th><th class='r'>Debit</th></tr></thead><tbody>";

            foreach (var transaction in account.Transactions)
            {
                toReturn += string.Format("<tr><td>{0}</td><td title='ID: {4}'>{1}</td><td class='r'>{2}</td><td class='r d'>{3}</td></tr>", transaction.Date.ToString("yyyy-MM-dd HH:mm"), transaction.Description ?? "[None]", GetModestyFilteredAmount(transaction.AmountCredited, configuration), GetModestyFilteredAmount(transaction.AmountDebited, configuration), transaction.ID);
            }
            
            toReturn += "</tbody></table>";

            toReturn += "</div>";
            return toReturn;
        }
    
        private static string RenderFactHeading(string heading) 
        {
            return "<h3>" + heading + "</h3>";
        }

        public static string RenderIdentitySnapshotDetails(IdentitySnapshotDetails details)
        {
            string toReturn = "<div class='fact'>";

            toReturn += RenderFact("Snapshot ID", details.SnapshotId);
            toReturn += RenderFact("Username", details.Username);
            toReturn += RenderFact("Timestamp", details.TimestampUtc);
            toReturn += RenderFact("Was a test user?", details.WasTestUser.ToString());

            toReturn += "</div>";

            return toReturn;
        }

        public static string RenderIdentitySnapshot(IdentitySnapshot snapshot)
        {
            string toReturn = "<div class='fact'>";

            toReturn += RenderFactHeading("Snapshot details");
            toReturn += RenderIdentitySnapshotDetails(snapshot.Details);
            toReturn += RenderFactHeading("Snapshot contents");
            toReturn += RenderUserProfile(snapshot.Snapshot);

            toReturn += "</div>";

            return toReturn;
        }

        public static string RenderUserProfile(MiiUserProfile profile)
        {        
    	    string toReturn = "<div class='fact'>";
        
    	    toReturn += "<h2>User profile</h2>";
    	    toReturn += RenderFact("Username", profile.Username);
            toReturn += RenderFact("Salutation", profile.Salutation);
            toReturn += RenderFact("First name", profile.FirstName);
            toReturn += RenderFact("Middle name", profile.MiddleName);
            toReturn += RenderFact("Last name", profile.LastName);
            toReturn += RenderFact("Date of birth", profile.DateOfBirth);
            toReturn += RenderFact("Age", profile.Age);
            toReturn += RenderFact("Identity verified?", profile.IdentityAssured);
            toReturn += RenderFact("Identity last verified?", profile.LastVerified);
            toReturn += RenderFact("Has a public profile?", profile.HasPublicProfile);
            toReturn += RenderFact("Previous first name", profile.PreviousFirstName);
            toReturn += RenderFact("Previous middle name", profile.PreviousMiddleName);
            toReturn += RenderFact("Previous last name", profile.PreviousLastName);
            toReturn += RenderFact("Profile URL", profile.ProfileUrl);
            toReturn += RenderFact("Profile short URL", profile.ProfileShortUrl);
            toReturn += RenderFact("Card image URL", profile.CardImageUrl);
            toReturn += RenderFactHeading("Postal addresses");
        
            int ct = 0;
        
            if (profile.PostalAddresses != null) 
            {            
        	    foreach (var address in profile.PostalAddresses) 
                {        		
        		    toReturn += "<div class='fact'><h4>[" + ct++ + "]</h4>";
                    toReturn += RenderAddress(address);
                    toReturn += "</div>";
                }
            }
        
            toReturn += RenderFactHeading("Phone numbers");
            ct = 0;
        
            if (profile.PhoneNumbers != null) 
            {            
        	    foreach (var number in profile.PhoneNumbers) 
                {
                    toReturn += "<div class='fact'><h4>[" + ct++ + "]</h4>";
                    toReturn += RenderPhone(number);
                    toReturn += "</div>";
                }
            }
        
            toReturn += RenderFactHeading("Email addresses");
            ct = 0;
        
            if (profile.EmailAddresses != null) 
            {            
        	    foreach (var address in profile.EmailAddresses) 
                {
                    toReturn += "<div class='fact'><h4>[" + ct++ + "]</h4>";
                    toReturn += RenderEmail(address);
                    toReturn += "</div>";
                }
            }
        
            toReturn += RenderFactHeading("Internet identities");
            ct = 0;
        
            if (profile.Identities != null) 
            {            
        	    foreach (Identity identity in profile.Identities) 
                {
                    toReturn += "<div class='fact'><h4>[" + ct++ + "]</h4>";
                    toReturn += RenderIdentity(identity);
                    toReturn += "</div>";
                }
            }

            toReturn += RenderFactHeading("Qualifications");
            ct = 0;

            if (profile.Qualifications != null)
            {
                foreach (var qualification in profile.Qualifications)
                {
                    toReturn += "<div class='fact'><h4>[" + ct++ + "]</h4>";
                    toReturn += RenderQualification(qualification);
                    toReturn += "</div>";
                }
            }
        
            if (profile.PublicProfile != null) {
        	
        	    toReturn += "<div class='fact'>";
                toReturn += RenderUserProfile(profile.PublicProfile);
                toReturn += "</div>";
            }
        
            toReturn += "</div>";
        
            return toReturn;
        }

        public static string RenderFinancialData(MiiFinancialData miiFinancialData, PrettifyConfiguration configuration)
        {
            string toReturn = "<div class='fact'>";

            toReturn += "<h2>Financial Data</h2>";
            toReturn += RenderFactHeading("Financial Providers");

            int ct = 0;
            if (miiFinancialData.FinancialProviders != null)
            {
                foreach (var provider in miiFinancialData.FinancialProviders)
                {
                    toReturn += "<div class='fact'><h4>[" + ct++ + "]</h4>";
                    toReturn += RenderFinancialProvider(provider, configuration);
                    toReturn += "</div>";
                }
            }

            toReturn += "</div>";

            return toReturn;
        }

        public static string RenderFinancialRefreshStatus(FinancialRefreshStatus status)
        {
            string toReturn = "<div class='fact'>";

            toReturn += RenderFact("State", status.State.ToString());

            toReturn += "</div>";

            return toReturn;
        }

        public static string RenderQualification(Qualification qualification)
        {
            string toReturn = "<div class='fact'>";

            toReturn += RenderFact("Type", qualification.Type);
            toReturn += RenderFact("Title", qualification.Title);
            toReturn += RenderFact("Provider", qualification.DataProvider);
            toReturn += RenderFact("Provider URL", qualification.DataProviderUrl);

            toReturn += "</div>";

            return toReturn;
        }
    }
}