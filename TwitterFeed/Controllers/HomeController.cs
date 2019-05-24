using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TwitterFeed.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //authentikatie voor de twitter API
            var twitter = new Twitter("I5RTAh94ztM4Erf6kigbDN9lh", "i9YUK3mBL1uNpCTYfQMLsFMCSoPDRvuvLiez0jigJmhnGoNZno", "1127907902784770049-9hgmoVWX07fLWDM9gH4UQIwVIRe9lj", "ebmcdjhFkT3WXdiNRPzkKaDN2eFcnm02EnRqkFWe0SpAs");

            //informate ophalen van de twitter ipa
            string userName = "RobertHeeren";
            string feed = twitter.GetFeed(userName,0);
           
            //begin punt voor het ophalen van individuele tweets
            int startPunt = 16;
            //int initaliseren voor de forloop
            int split = 0;

            //de hoeveelheid tweets ophalen.
            int tweetsIndex = feed.IndexOf("\"statuses_count\":");
            int tweetsIndexEnd = feed.Substring(tweetsIndex).IndexOf(",");
            int numberOfTweets = Convert.ToInt32(feed.Substring(tweetsIndex + 17, tweetsIndexEnd - 17));

            ViewData["number"] = numberOfTweets;

            //lijsten met de tweets 
            List<string> text = new List<string>();
            string tweetbodys = "";

            //Het samen voegen van alle tweets
            for (int i = 0; i < numberOfTweets; i++) {

                //het begin van de twwets vinden
                split = feed.Substring(startPunt).IndexOf("location");
                string tweet = feed.Substring(startPunt);
                
                //de body van de tweet isoleren.
                int textIn = tweet.IndexOf("\"text");
                int textEnd = tweet.Substring(textIn+8, 280).IndexOf("\",\"");
                text.Add(tweet.Substring(textIn + 8, textEnd));                

                //nieuw startpunt instellen
                startPunt += split+20;
            }

            //Het opstellen van de tweet voor op de website.
            for (int x = 0;x< text.Count(); x++)
            {
                tweetbodys += ("<p>" + userName + "</p><p>");
                tweetbodys += ("<p>"+ text[x] + "</p><br/><br/>");
            }

            //data door sturen naar de index pagina.
            ViewData["tweetbodys"]= tweetbodys;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}