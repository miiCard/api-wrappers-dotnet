using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using miiCard.Consumers.Service.v1.Claims;
using Newtonsoft.Json;

namespace miiCard.Consumers.Service.v1
{
    public class MiiCardDirectoryService
    {
        public static readonly string CRITERION_EMAIL = "email";
        public static readonly string CRITERION_PHONE = "phone";
        public static readonly string CRITERION_TWITTER = "twitter";
        public static readonly string CRITERION_FACEBOOK = "facebook";
        public static readonly string CRITERION_LINKEDIN = "linkedin";
        public static readonly string CRITERION_GOOGLE = "google";
        public static readonly string CRITERION_MICROSOFT_ID = "liveid";
        public static readonly string CRITERION_EBAY = "ebay";
        public static readonly string CRITERION_VERITAS_VITAE = "veritasvitae";
        public static readonly string CRITERION_USERNAME = "username";

        public MiiUserProfile FindByEmail(string emailAddress, bool hashed = false) 
        {
            return this.FindBy(CRITERION_EMAIL, emailAddress, hashed);
        }

        public MiiUserProfile FindByPhoneNumber(string phoneNumber, bool hashed = false)
        {
            return this.FindBy(CRITERION_PHONE, phoneNumber, hashed);
        }

        public MiiUserProfile FindByUsername(string username, bool hashed = false)
        {
            return this.FindBy(CRITERION_USERNAME, username, hashed);
        }

        public MiiUserProfile FindByTwitter(string twitterHandleOrProfileUrl, bool hashed = false)
        {
            return this.FindBy(CRITERION_TWITTER, twitterHandleOrProfileUrl, hashed);
        }

        public MiiUserProfile FindByFacebook(string facebookHandleOrProfileUrl, bool hashed = false)
        {
            return this.FindBy(CRITERION_FACEBOOK, facebookHandleOrProfileUrl, hashed);
        }

        public MiiUserProfile FindByLinkedIn(string linkedinHandleOrProfileUrl, bool hashed = false)
        {
            return this.FindBy(CRITERION_LINKEDIN, linkedinHandleOrProfileUrl, hashed);
        }

        public MiiUserProfile FindByGoogle(string googleHandleOrProfileUrl, bool hashed = false)
        {
            return this.FindBy(CRITERION_GOOGLE, googleHandleOrProfileUrl, hashed);
        }

        public MiiUserProfile FindByMicrosoftId(string microsoftIdHandleOrProfileUrl, bool hashed = false)
        {
            return this.FindBy(CRITERION_MICROSOFT_ID, microsoftIdHandleOrProfileUrl, hashed);
        }

        public MiiUserProfile FindByEbay(string ebayHandleOrProfileUrl, bool hashed = false)
        {
            return this.FindBy(CRITERION_EBAY, ebayHandleOrProfileUrl, hashed);
        }

        public MiiUserProfile FindByVeritasVitae(string veritasVitaeHandleOrProfileUrl, bool hashed = false)
        {
            return this.FindBy(CRITERION_VERITAS_VITAE, veritasVitaeHandleOrProfileUrl, hashed);
        }

        public MiiUserProfile FindBy(string criterion, string value, bool hashed = false)
        {
            return this.FindByCore(criterion, value, hashed);
        }

        protected virtual MiiUserProfile FindByCore(string criterion, string value, bool hashed)
        {
            MiiUserProfile toReturn = null;

            WebRequest request = HttpWebRequest.Create(this.GetQueryUrl(criterion, value, hashed));
            request.ContentType = "application/json";
            var response = request.GetResponse();

            using (var responseStream = response.GetResponseStream())
            {
                using (var jsonReader = new JsonTextReader(new StreamReader(responseStream)))
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings() 
                    {
                        
                    };

                    var serialiser = JsonSerializer.Create(settings);
                    var parsedResponse = serialiser.Deserialize(jsonReader, typeof(MiiApiResponse<MiiUserProfile>)) as MiiApiResponse<MiiUserProfile>;

                    if (parsedResponse != null && parsedResponse.Status == MiiApiCallStatus.Success && parsedResponse.ErrorCode == MiiApiErrorCode.Success)
                    {
                        toReturn = parsedResponse.Data;
                    }
                }
            }

            return toReturn;
        }

        protected virtual string GetQueryUrl(string criterion, string value, bool hashed)
        {
            string toReturn = ServiceUrls.DirectoryServiceEndpoint + string.Format("?{0}={1}", criterion, HttpUtility.UrlEncode(value));
            if (hashed)
            {
                toReturn += "&hashed=true";
            }

            return toReturn;
        }

        public static string HashIdentifier(string identifier)
        {
            using (var sha1 = SHA1.Create()) 
            {
                var hashedBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(identifier.ToLower()));
                return string.Join("", hashedBytes.Select(x => x.ToString("x2")));
            }
        }
    }
}
