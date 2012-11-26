using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using miiCard.Consumers.Service.v1;
using miiCard.Consumers.Service.v1.Claims;

namespace miiCard.Consumers.TestHarness.Extensions
{
    public static class MiiApiResponseExtensions
    {
        static Func<object, string> DEFAULT_RENDERER = x => RenderFact("Data", x);

        static Dictionary<Type, Func<object, string>> RENDERERS = GetRenderers();

        private static Dictionary<Type, Func<object, string>> GetRenderers()
        {
            var toReturn = new Dictionary<Type, Func<object, string>>();

            toReturn[typeof(MiiUserProfile)] = x => RenderUserProfile(x as MiiUserProfile);
            toReturn[typeof(IdentitySnapshot)] = x => RenderIdentitySnapshot(x as IdentitySnapshot);
            toReturn[typeof(IdentitySnapshotDetails)] = x => RenderIdentitySnapshotDetails(x as IdentitySnapshotDetails);
            
            return toReturn;
        }

        public static string Prettify<T>(this MiiApiResponse<T> response)
        {
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

                    Func<object, string> renderer = DEFAULT_RENDERER;
                    if (obj != null)
                    {
                        if (!RENDERERS.TryGetValue(obj.GetType(), out renderer))
                        {
                            renderer = DEFAULT_RENDERER;
                        }
                    }

                    toReturn += renderer(obj);

                    toReturn += "</div>";
                    ct++;
                }
            }
            else
            {
                Func<object, string> renderer = null;
                if (!RENDERERS.TryGetValue(typeof(T), out renderer))
                {
                    renderer = DEFAULT_RENDERER;
                }

                toReturn += renderer(response.Data);
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
        
            if (profile.PublicProfile != null) {
        	
        	    toReturn += "<div class='fact'>";
                toReturn += RenderUserProfile(profile.PublicProfile);
                toReturn += "</div>";
            }
        
            toReturn += "</div>";
        
            return toReturn;
        }
    }
}