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
            int numberOfTweets = twitter.GetNumberTweets(feed);
            
            //data door sturen naar de index pagina.
            ViewData["number"] = numberOfTweets;
            ViewData["tweetbodys"]= twitter.FeedToHTML(feed, userName);
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