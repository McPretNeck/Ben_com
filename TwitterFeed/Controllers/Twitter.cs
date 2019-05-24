using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Net;
using System.Text;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;


namespace TwitterFeed.Controllers
{
    public class Twitter
    {
        public const string OauthVersion = "1.0";
        public const string OauthSignatureMethod = "HMAC-SHA1";

        public Twitter
          (string consumerKey, string consumerKeySecret, string accessToken, string accessTokenSecret)
        {
            this.ConsumerKey = consumerKey;
            this.ConsumerKeySecret = consumerKeySecret;
            this.AccessToken = accessToken;
            this.AccessTokenSecret = accessTokenSecret;
        }

        public string ConsumerKey { set; get; }
        public string ConsumerKeySecret { set; get; }
        public string AccessToken { set; get; }
        public string AccessTokenSecret { set; get; }

        public string GetMentions(int count)
        {
            string resourceUrl =
                string.Format("http://api.twitter.com/1/statuses/mentions.json");

            var requestParameters = new SortedDictionary<string, string>();
            requestParameters.Add("count", count.ToString());
            requestParameters.Add("include_entities", "true");

            var response = GetResponse(resourceUrl, Method.GET, requestParameters);

            return response;
        }

        public string FeedToHTML(string feed, int startPunt,string userName)
        {   //feed is de string die vanuit deze class van twitter gehaalt word.
            //startPunt is voor het vinden van de eerst volgende tweet.
            //userName voor het gemak. 

            //lijsten met de tweets 
            List<string> text = new List<string>();
            string tweetbodys = "";

            //Het samen voegen van alle tweets
            for (int i = 0; i < GetNumberTweets(feed); i++)
            {
                //het begin van de twwets vinden
                int split = feed.Substring(startPunt).IndexOf("location");
                string tweet = feed.Substring(startPunt);

                //de body van de tweet isoleren.
                int textIn = tweet.IndexOf("\"text");
                int textEnd = tweet.Substring(textIn + 8, 280).IndexOf("\",\"");
                text.Add(tweet.Substring(textIn + 8, textEnd));

                //nieuw startpunt instellen
                startPunt += split + 20;
            }

            //Het opstellen van de tweet voor op de website.
            for (int x = 0; x < text.Count(); x++)
            {
                tweetbodys += ("<p>" + userName + "</p><p>");
                tweetbodys += ("<p>" + text[x] + "</p><br/><br/>");
            }
            return tweetbodys;
        }
        public int GetNumberTweets(string feed)
        {   //om te kijken hoeveel tweets er zijn. maakt het makkelijker in de forloop.
            
            //de hoeveelheid tweets ophalen.
            int tweetsIndex = feed.IndexOf("\"statuses_count\":");
            int tweetsIndexEnd = feed.Substring(tweetsIndex).IndexOf(",");
            int numberOfTweets = Convert.ToInt32(feed.Substring(tweetsIndex + 17, tweetsIndexEnd - 17));

            return numberOfTweets;
        }

        public string GetFeed(string screenName, int count)
        {
            string resourceUrl =
                string.Format("https://api.twitter.com/1.1/statuses/user_timeline.json");

            var requestParameters = new SortedDictionary<string, string>();
            requestParameters.Add("count", count.ToString());
            requestParameters.Add("screen_name", screenName);

            var response = GetResponse(resourceUrl, Method.GET, requestParameters);

            return response;
        }

        public string PostStatusUpdate(string status, double latitude, double longitude)
        {
            const string resourceUrl = "http://api.twitter.com/1/statuses/update.json";

            var requestParameters = new SortedDictionary<string, string>();
            requestParameters.Add("status", status);
            requestParameters.Add("lat", latitude.ToString());
            requestParameters.Add("long", longitude.ToString());

            return GetResponse(resourceUrl, Method.POST, requestParameters);
        }

        private string GetResponse
        (string resourceUrl, Method method, SortedDictionary<string, string> requestParameters)
        {
            ServicePointManager.Expect100Continue = false;
            WebRequest request = null;
            string resultString = string.Empty;

            if (method == Method.POST)
            {
                var postBody = requestParameters.ToWebString();

                request = (HttpWebRequest)WebRequest.Create(resourceUrl);
                request.Method = method.ToString();
                request.ContentType = "application/x-www-form-urlencoded";

                using (var stream = request.GetRequestStream())
                {
                    byte[] content = Encoding.ASCII.GetBytes(postBody);
                    stream.Write(content, 0, content.Length);
                }
            }
            else if (method == Method.GET)
            {
                request = (HttpWebRequest)WebRequest.Create(resourceUrl + "?"
                    + requestParameters.ToWebString());
                request.Method = method.ToString();
            }
            else
            {
                //other verbs can be addressed here...
            }

            if (request != null)
            {
                var authHeader = CreateHeader(resourceUrl, method, requestParameters);
                request.Headers.Add("Authorization", authHeader);
                var response = request.GetResponse();

                using (var sd = new StreamReader(response.GetResponseStream()))
                {
                    resultString = sd.ReadToEnd();
                    response.Close();
                }
            }

            return resultString;
        }

        private string CreateOauthNonce()
        {
            return Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
        }

        private string CreateHeader(string resourceUrl, Method method,
                                    SortedDictionary<string, string> requestParameters)
        {
            var oauthNonce = CreateOauthNonce();
            // Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            var oauthTimestamp = CreateOAuthTimestamp();
            var oauthSignature = CreateOauthSignature
            (resourceUrl, method, oauthNonce, oauthTimestamp, requestParameters);

            //The oAuth signature is then used to generate the Authentication header. 
            const string headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method =\"{1}\", " + "oauth_timestamp=\"{2}\", oauth_consumer_key =\"{3}\", " + "oauth_token=\"{4}\", oauth_signature =\"{5}\", " + "oauth_version=\"{6}\"";

            var authHeader = string.Format(headerFormat,
                                           Uri.EscapeDataString(oauthNonce),
                                           Uri.EscapeDataString(OauthSignatureMethod),
                                           Uri.EscapeDataString(oauthTimestamp),
                                           Uri.EscapeDataString(ConsumerKey),
                                           Uri.EscapeDataString(AccessToken),
                                           Uri.EscapeDataString(oauthSignature),
                                           Uri.EscapeDataString(OauthVersion)
                );

            return authHeader;
        }

        private string CreateOauthSignature
        (string resourceUrl, Method method, string oauthNonce, string oauthTimestamp,
                                            SortedDictionary<string, string> requestParameters)
        {
            //firstly we need to add the standard oauth parameters to the sorted list
            requestParameters.Add("oauth_consumer_key", ConsumerKey);
            requestParameters.Add("oauth_nonce", oauthNonce);
            requestParameters.Add("oauth_signature_method", OauthSignatureMethod);
            requestParameters.Add("oauth_timestamp", oauthTimestamp);
            requestParameters.Add("oauth_token", AccessToken);
            requestParameters.Add("oauth_version", OauthVersion);

            var sigBaseString = requestParameters.ToWebString();

            var signatureBaseString = string.Concat
            (method.ToString(), "&", Uri.EscapeDataString(resourceUrl), "&",
                                Uri.EscapeDataString(sigBaseString.ToString()));

            //Using this base string, we then encrypt the data using a composite of the 
            //secret keys and the HMAC-SHA1 algorithm.
            var compositeKey = string.Concat(Uri.EscapeDataString(ConsumerKeySecret), "&",
                                             Uri.EscapeDataString(AccessTokenSecret));

            string oauthSignature;
            using (var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(compositeKey)))
            {
                oauthSignature = Convert.ToBase64String(
                    hasher.ComputeHash(Encoding.ASCII.GetBytes(signatureBaseString)));
            }

            return oauthSignature;
        }

        private static string CreateOAuthTimestamp()
        {

            var nowUtc = DateTime.UtcNow;
            var timeSpan = nowUtc - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();

            return timestamp;
        }
    }

    public enum Method
    {
        POST,
        GET
    }

    public static class Extensions
    {
        public static string ToWebString(this SortedDictionary<string, string> source)
        {
            var body = new StringBuilder();

            foreach (var requestParameter in source)
            {
                body.Append(requestParameter.Key);
                body.Append("=");
                body.Append(Uri.EscapeDataString(requestParameter.Value));
                body.Append("&");
            }
            //remove trailing '&'
            body.Remove(body.Length - 1, 1);

            return body.ToString();
        }
    }


}
