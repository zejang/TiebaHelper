using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
namespace TiebaHelper.Controllers
{
    public class HomeController : Controller
    {

        private static string url_getQrcodeUrl = "https://passport.baidu.com/v2/api/getqrcode?lp=pc&qrloginfrom=pc";
        private static string sign = string.Empty;

        public ActionResult Index()
        {
            ViewBag.Title = "熊沐风贴吧签到助手";

            //Setting Qrcode to the page.
            JObject getQrcodeJson = GetUrlJson(url_getQrcodeUrl);
            ViewBag.Qrcode = getQrcodeJson["imgurl"];
            ViewBag.Sign = getQrcodeJson["sign"];
            return View();
        }



        private JObject GetUrlJson(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "Get";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                return JObject.Parse(reader.ReadToEnd());
            }
        }
    }
}
