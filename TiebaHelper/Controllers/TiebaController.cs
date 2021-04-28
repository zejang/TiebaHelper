using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Text;

namespace TiebaHelper.Controllers
{
    public class TiebaController : ApiController
    {

        private static string url_main = "https://tieba.baidu.com/";
        private static string url_sign = "https://tieba.baidu.com/sign/add/";
        private static string url_tbs = "http://tieba.baidu.com/dc/common/tbs/";

        [HttpGet]
        public JObject GetForumList(string STOKEN, string BDUSS)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url_main);
            request.Method = "Get";
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(new Cookie("STOKEN", STOKEN, "/", ".baidu.com"));
            request.CookieContainer.Add(new Cookie("BDUSS", BDUSS, "/", ".baidu.com"));
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                JObject returnJson = new JObject();
                JArray forumJarray = new JArray();
                MatchCollection forumRegex = Regex.Matches(reader.ReadToEnd(), "(?<=\"forum_name\":\")[^}]*?(?=\",\"is_like\")");
                foreach (Match item in forumRegex)
                {
                    forumJarray.Add(Regex.Unescape(item.Value));
                }

                returnJson = new JObject()
                {
                    { "ForumCount",forumJarray.Count},
                    { "ForumList",forumJarray}
                };
                return returnJson;
            }
        }


        [HttpGet]
        public JObject SignIn(string STOKEN, string BDUSS)
        {
            string tbs = string.Empty;
            JObject forumList = GetForumList(STOKEN, BDUSS);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url_tbs);
            request.Method = "Get";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                tbs = JObject.Parse(reader.ReadToEnd())["tbs"].ToString();
            }

            int forumCount = (int)forumList["ForumCount"];

            for (int i = 0; i < forumCount; i++)
            {
                string postData = "ie=utf-8&kw=" + forumList["ForumList"][i] + "&tbs=" + tbs;
                HttpWebRequest _request = (HttpWebRequest)WebRequest.Create(url_sign);
                _request.Method = "Post";
                _request.ContentLength = postData.Length;
                _request.CookieContainer = new CookieContainer();
                _request.CookieContainer.Add(new Cookie("STOKEN", STOKEN, "/", ".baidu.com"));
                _request.CookieContainer.Add(new Cookie("BDUSS", BDUSS, "/", ".baidu.com"));
                using (Stream _stream = _request.GetRequestStream())
                {
                    _stream.Write(Encoding.UTF8.GetBytes(postData), 0, postData.Length);
                }
            }
            return null;
        }

        [HttpGet]
        public JObject CheckScanIsSuccess(string sign)
        {
            string url_checkscan = $"https://passport.baidu.com/channel/unicast?channel_id={sign}&callback=tangram_guid_1593274789639";
            JObject ScanEndJson = new JObject();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url_checkscan);
            request.Method = "Get";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                Match m = Regex.Match(reader.ReadToEnd(), @"(?<=\()[\s\S]*(?=\))");
                ScanEndJson = JObject.Parse(m.ToString());
            }

            if (ScanEndJson["errno"].ToString() != "-1")
            {
                JObject channel_v = JObject.Parse(ScanEndJson["channel_v"].ToString());
                string v = channel_v["v"].ToString();
                string url_getdata = $"https://passport.baidu.com/v3/login/main/qrbdusslogin?bduss={v}";
                JObject endJson = GetUrlJson(url_getdata);
                string username = (string)endJson["data"]["user"]["username"];
                string bduss = (string)endJson["data"]["session"]["bduss"];
                string stoken = (string)endJson["data"]["session"]["stokenList"];
                Match re = Regex.Match(stoken, @"(?<=\[&quot;tb#)[\s\S]*?(?=&quot)");
                stoken = re.ToString();
                return new JObject() { { "username", username }, { "bduss", bduss }, { "stoken", stoken } };
            }
            else
            {
                return null;
            }
        }

        private JObject GetUrlJson(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "Get";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                return JObject.Parse(Regex.Unescape(reader.ReadToEnd()));
            }
        }
    }
}

