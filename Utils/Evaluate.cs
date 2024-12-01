using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using HtmlAgilityPack;
using System.Collections;

namespace AutoEvaluate.Utils
{
    public static class Evaluate
    {

        private static CookieContainer? loginCookie = null;
        private static HttpClient? evaluateClient = null;
        private static string un, pw;

        public static async Task<(bool, string)> Login(string username1, string password1)
        {
            try
            {
                un = username1;
                pw = password1;
                var cookie = new CookieContainer();
                var handler = new HttpClientHandler() { CookieContainer = cookie, AllowAutoRedirect = false };
                using HttpClient client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9");
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Google Chrome\";v=\"131\",\"Chromium\";v=\"131\",\"Not_A Brand\";v=\"24\"");
                client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");

                var res = await client.GetStringAsync("https://zhlgd.whut.edu.cn/tpass/login?service=https%3A%2F%2Fzhlgd.whut.edu.cn%2Ftp_up%2F");
                var html = new HtmlDocument();
                html.LoadHtml(res);
                var lt = html.DocumentNode.SelectSingleNode("//*[@id=\"lt\"]").GetAttributeValue("value", "");

                var response = await client.PostAsync("https://zhlgd.whut.edu.cn/tpass/rsa?skipWechat=true", null);
                var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                var key = json.GetProperty("publicKey").GetString();

                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(key!), out _);

                var encryptedUN = Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(username1), false));
                var encryptedPW = Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(password1), false));

                var form = new Dictionary<string, string>() {
                {"rsa", ""},
                {"ul",encryptedUN },
                {"pl",encryptedPW },
                {"lt",lt },
                {"execution", "e1s1" },
                {"_eventId", "submit" },
            };
                response = await client.PostAsync("https://zhlgd.whut.edu.cn/tpass/login?service=https%3A%2F%2Fzhlgd.whut.edu.cn%2Ftp_up%2F", new FormUrlEncodedContent(form));
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return (false, "用户名或密码错误");
                }
                var location = response.Headers.Location;
                response = await client.GetAsync(location);
                location = response.Headers.Location;
                response = await client.GetAsync(location);
                location = response.Headers.Location;
                response = await client.GetAsync(location);
                location = response.Headers.Location;
                response = await client.GetAsync(location);

                client.DefaultRequestHeaders.Remove("X-Requested-With");
                response = await client.GetAsync("https://jwxt.whut.edu.cn/jwapp/sys/homeapp/index.do?forceCas=1");
                location = response.Headers.Location;
                response = await client.GetAsync(location);
                location = response.Headers.Location;
                response = await client.GetAsync(location);
                location = response.Headers.Location;
                response = await client.GetAsync(location);
                location = new Uri("https://jwxt.whut.edu.cn/jwapp/sys/homeapp/home/index.html?av=1732803051828&contextPath=/jwapp");
                response = await client.GetAsync(location);
                loginCookie = cookie;
                handler = new HttpClientHandler() { CookieContainer = loginCookie, AllowAutoRedirect = false };
                evaluateClient = new HttpClient(handler);
                evaluateClient.Timeout = TimeSpan.FromSeconds(15);
                evaluateClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                evaluateClient.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9");
                evaluateClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
                evaluateClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36");
                evaluateClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
                evaluateClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
                evaluateClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
                evaluateClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Google Chrome\";v=\"131\",\"Chromium\";v=\"131\",\"Not_A Brand\";v=\"24\"");
                evaluateClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                evaluateClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
            }
            catch (Exception)
            {
                return (false, "登录失败，请重试或确认网络状态以及教务系统是否正常。\n如无法解决请反馈。≧ ﹏ ≦");
            }
            return (true, "success");
        }

        public static async Task<(bool, string)> Refresh()
        {
            return await Login(un, pw);
        }

        public static async Task<EvaluationData?> GetEvaluationData(bool isRefreshCookie = false)
        {
            if (evaluateClient == null) return null;
            var result = new EvaluationData();
            try
            {
                var form = new Dictionary<string, string>()
            {
                {"ZCSDM", "DQXNXQDM" },
                { "CSDM", "SYS" },
                {"SFSY", "1" },
                { "*order", "-DM" },
            };

                var json = await (await evaluateClient.PostAsync("https://jwxt.whut.edu.cn/jwapp/sys/jwpubapp/modules/gg/cxmrxnxq.do", new FormUrlEncodedContent(form))).Content.ReadFromJsonAsync<JsonElement>();

                var xnxqdm = json.GetProperty("datas").GetProperty("cxmrxnxq").GetProperty("rows")[0].GetProperty("XNXQDM").GetString();

                form = new Dictionary<string, string>()
            {
                {"PJLXDM","01" },
                {"querySetting", $"[{{\"name\":\"XNXQDM\",\"builder\":\"m_value_equal\",\"linkOpt\":\"AND\",\"value\":\"{xnxqdm}\"}}]" },
            };

                var response = await evaluateClient.PostAsync("https://jwxt.whut.edu.cn/jwapp/sys/pjapp/api/wdpj/getDpwj.do", new FormUrlEncodedContent(form));
                json = await response.Content.ReadFromJsonAsync<JsonElement>();
                foreach (var item in json.GetProperty("datas").GetProperty("getDpwj").EnumerateArray())
                {
                    var s = Convert.ToDateTime(item.GetProperty("KSSJ").GetString()!);
                    var e = Convert.ToDateTime(item.GetProperty("JSSJ").GetString());
                    if (DateTime.Now < s || DateTime.Now > e)
                    {
                        result.UnexpiredOrNotStarted.Add(new EvaluationItem(item.GetProperty("SKDX").GetString()!, item.GetProperty("KCM").GetString()!,
                                           item.GetProperty("GROUPNO").GetString()!, item.GetProperty("PJLXDM").GetString()!,
                                           item.GetProperty("XUH").GetInt32()!, item.GetProperty("JSSJ").GetString()!));
                    }
                    else
                    {
                        result.Unevaluated.Add(new EvaluationItem(item.GetProperty("SKDX").GetString()!, item.GetProperty("KCM").GetString()!,
                                            item.GetProperty("GROUPNO").GetString()!, item.GetProperty("PJLXDM").GetString()!,
                                            item.GetProperty("XUH").GetInt32()!, item.GetProperty("JSSJ").GetString()!));
                    }

                }

                response = await evaluateClient.PostAsync("https://jwxt.whut.edu.cn/jwapp/sys/pjapp/api/wdpj/getYpwj.do", new FormUrlEncodedContent(form));
                json = await response.Content.ReadFromJsonAsync<JsonElement>();
                foreach (var item in json.GetProperty("datas").GetProperty("getYpwj").EnumerateArray())
                {
                    result.Evaluated.Add(new EvaluationItem(item.GetProperty("SKDX").GetString()!, item.GetProperty("KCM").GetString()!,
                                            item.GetProperty("GROUPNO").GetString()!, item.GetProperty("PJLXDM").GetString()!,
                                            item.GetProperty("XUH").GetInt32()!, item.GetProperty("JSSJ").GetString()!));
                }
            }
            catch(Exception)
            {
                return null;
            }

            return result;
        }

        public static async Task<bool> EvaluateCourse(EvaluationItem item)
        {
            if (evaluateClient == null) return false;
            try
            {
                var form = new Dictionary<string, string>()
                {
                    { "GROUPNO", item.GROUPNO },
                    { "PJLXDM", item.PJLXDM },
                    { "XUH", Convert.ToString(item.XUH) }
                };
                var response = await evaluateClient.PostAsync ("https://jwxt.whut.edu.cn/jwapp/sys/pjapp/api/wdpj/getWjtxxx.do", new  FormUrlEncodedContent(form));
                var json = await response.Content.ReadFromJsonAsync<JsonElement>();

                var tt = ParseParam(json, item);
                form = new Dictionary<string, string>()
                {
                    {"requestParamStr", tt},
                };
                response = await evaluateClient.PostAsync("https://jwxt.whut.edu.cn/jwapp/sys/pjapp/api/wdpj/calculateQuestionnaireAnswerScore.do", new  FormUrlEncodedContent(form));
                response = await evaluateClient.PostAsync("https://jwxt.whut.edu.cn/jwapp/sys/pjapp/api/wdpj/commitQuestionnaireAnswer.do", new FormUrlEncodedContent(form));
                if (((int)response.StatusCode) != 200) return false;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static string ParseParam(JsonElement json, EvaluationItem item)
        {
            var teacher = json.GetProperty("datas").GetProperty("getWjtxxx").GetProperty("teachers")[0];
            var pjgxid = teacher.GetProperty("PJGXID").GetString();
            var pjzt = teacher.GetProperty("PJZT").GetString();
            var df = teacher.GetProperty("DF").GetString();
            var xuh = json.GetProperty("datas").GetProperty("getWjtxxx").GetProperty("XUH").GetInt32();
            var wjid = json.GetProperty("datas").GetProperty("getWjtxxx").GetProperty("WJID").GetString();

            var res = new PostJson[1];
            var t = new PostJson()
            {
                KCM=item.CourseName,
                PJZT=pjzt,
                DF=df,
                PJGXID=pjgxid,
                XM=item.TeacherName,
                XUH=xuh,
                FJTXXX= json.GetProperty("datas").GetProperty("getWjtxxx").GetProperty("FJTXXX").GetRawText(),
                WJID=wjid,
            };
            foreach (var j in json.GetProperty("datas").GetProperty("getWjtxxx").GetProperty("questionList").EnumerateArray ())
            {
                var temp = new DAJson()
                {
                    WJID=j.GetProperty("WJID").GetString()!,
                    TMID=j.GetProperty("TMID").GetString()!,
                    TX=j.GetProperty("TX").GetString()!
                };
                if (j.GetProperty("questionOptions").GetArrayLength() > 0)
                {
                    foreach (var ttttt in j.GetProperty ("questionOptions").EnumerateArray ())
                    {
                        if (ttttt.GetProperty("PX").GetInt32() == 1 || ttttt.GetProperty("MC").GetString() == "非常好")
                        {
                            temp.DA = new DAItem() 
                            { 
                                TMXXID=ttttt.GetProperty("WID").GetString()!,
                                FJXX=String.Empty,
                            };
                            break;
                        }
                    }
                }else
                {
                    temp.DA = "";
                }
                t.DA.Add (temp);
            }
            t.questionAnswers = JsonConvert.SerializeObject(t.DA);
            res[0] = t;
            return JsonConvert.SerializeObject (res);
        }

        private class PostJson
        {
            public string KCM = String.Empty;
            public string PJZT = String.Empty;
            public string? DF = null;
            public string PJGXID = String.Empty;
            public ArrayList DA = new ArrayList();
            public string XM = String.Empty;
            public int XUH = 1;
            public string FJTXXX = String.Empty;
            public string WJID = String.Empty;
            public string questionAnswers = String.Empty;
        }

        private class DAJson
        {
            public string WJID = String.Empty;
            public string TMID = String.Empty;
            public string TX = String.Empty;
            public Object? DA = null;
        }

        private class DAItem
        {
            public string TMXXID = String.Empty;
            public string FJXX = String.Empty;
        }
    }
}