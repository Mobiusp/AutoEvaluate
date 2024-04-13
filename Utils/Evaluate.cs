using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HtmlAgilityPack;

namespace AutoEvaluate.Utils
{
    public static class Evaluate
    {

        private static CookieContainer? loginCookie = null;
        private static CookieContainer? evaluationCookie = null;
        private static HttpClient? evaluateClient = null;

        public static async Task InitEvaluateClient()
        {
            if (evaluateClient == null)
            {
                bool res = await RefreshCookie();
                if (! res) return;
                var handler = new HttpClientHandler() { CookieContainer = evaluationCookie!, AllowAutoRedirect = false };
                evaluateClient = new HttpClient(handler);
                evaluateClient.Timeout = TimeSpan.FromSeconds(15);
                evaluateClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                evaluateClient.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9");
                evaluateClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
                evaluateClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36");
            }
        }

        public static async Task<(bool, string)> Login(string username1, string password1)
        {
            try
            {
                var cookie = new CookieContainer();
                var handler = new HttpClientHandler() { CookieContainer = cookie, AllowAutoRedirect = false };
                using HttpClient client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9");
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36");

                var text = await client.GetStringAsync("http://sso.jwc.whut.edu.cn/Certification/toIndex.do");
                var html = new HtmlDocument();
                html.LoadHtml(text);

                var fingerPrint = "b9a7a7901c83c4c0dad90bd2bbf19498";
                var postData = new HttpRequestMessage(HttpMethod.Post, "http://sso.jwc.whut.edu.cn/Certification/getCode.do");
                postData.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    {"webfinger", fingerPrint }
                });
                var httpCode = await client.SendAsync(postData);
                var code = await httpCode.Content.ReadAsStringAsync();

                var pwSha1 = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(username1 + password1));
                var pwBuilder = new StringBuilder();
                foreach (var key in pwSha1) pwBuilder.Append(key.ToString("x2"));
                var usernameMD5 = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(username1));
                var unBuilder = new StringBuilder();
                foreach (var key in usernameMD5) unBuilder.Append(key.ToString("x2"));

                var form = new Dictionary<string, string>() {
                    { "MsgID", ""}, {"KeyID", ""}, {"UserName", "" }, {"Password", ""},
                    { "rnd", html.DocumentNode.SelectSingleNode("//form/input[@name='rnd']").GetAttributeValue("value", "") },
                    {"return_EncData", "" }, {"code", code},
                    { "userName1", unBuilder.ToString() }, {"password1", pwBuilder.ToString() },
                    { "webfinger",  fingerPrint}, {"falg", ""}, {"type", "xs" }, {"userName", "" }, {"password", "" }
                };
                var res = await client.PostAsync("http://sso.jwc.whut.edu.cn/Certification/login.do", new FormUrlEncodedContent(form));
                text = await res.Content.ReadAsStringAsync();
                if (text.IndexOf("用户名或密码错误") != -1) return (false, "用户名或密码错误");
                loginCookie = cookie;
            }catch (Exception)
            {
                return (false, "登录失败，请重试或确认网络状态（最好使用校园网）以及教务系统是否正常。\n如无法解决请反馈。≧ ﹏ ≦");
            }
            return (true, "success");
        }

        public static async Task<bool> RefreshCookie()
        {
            try
            {
                if (loginCookie == null) return false;
                var cookie = new CookieContainer();
                var handler = new HttpClientHandler() { CookieContainer = cookie, AllowAutoRedirect = false };
                using HttpClient client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9");
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36");

                var res = await client.GetAsync("http://218.197.101.69:8080/edu/union");
                var nextLocation = res.Headers.Location;

                var tempHandler = new HttpClientHandler() { CookieContainer = loginCookie, AllowAutoRedirect = false };
                using (var tempClient = new HttpClient(tempHandler))
                {
                    tempClient.Timeout = TimeSpan.FromSeconds(15);
                    tempClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                    tempClient.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9");
                    tempClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
                    tempClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36");
                    res = await tempClient.GetAsync(nextLocation);
                    nextLocation = res.Headers.Location;
                }

                res = await client.GetAsync(nextLocation);
                evaluationCookie = cookie;
            }catch(Exception)
            {
                return false;
            }
            return true;
        }

        public static async Task<EvaluationData?> GetEvaluationData(bool isRefreshCookie = false)
        {
            if (loginCookie == null) return null;
            if (evaluationCookie == null || isRefreshCookie)
            {
                bool flag = await RefreshCookie();
                if (! flag) return null;
            }
            var result = new EvaluationData();
            try
            {
                var handler = new HttpClientHandler() { CookieContainer = evaluationCookie!, AllowAutoRedirect = false };
                using HttpClient client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9");
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36");

                var res = await client.GetStringAsync("http://218.197.101.69:8080/edu/task/deal?kind=1");
                if (res.IndexOf("loginError") != -1)
                {
                    await RefreshCookie();
                    res = await client.GetStringAsync("http://218.197.101.69:8080/edu/task/deal?kind=1");
                }
                var html = new HtmlDocument();
                html.LoadHtml(res);

                var xnxq = html.DocumentNode.SelectSingleNode("//form/select[@id='xnxq']/option[@selected]").InnerText;
                var objId = html.DocumentNode.SelectSingleNode("//form[@id='form']/input[@name='objId']").GetAttributeValue("value", "");
                for (int i = 1; i <= 6; ++i)
                {
                    var form = new Dictionary<string, string>()
                {
                    { "objId", objId},
                    { "teacherId", "" },
                    { "kind", "1" },
                    {"status", "" },
                    {"xnxq", xnxq },
                    {"page", i.ToString() },
                    {"rows", "50" },
                    {"sort", "a.status" },
                    {"order", "asc" },
                    { "total", "-1" }
                };
                    var response = await client.PostAsync("http://218.197.101.69:8080/edu/task/page", new FormUrlEncodedContent(form));
                    var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                    var evaluationT = json.GetProperty("data").GetProperty("easyuiData");
                    foreach (var item in evaluationT.GetProperty("rows").EnumerateArray())
                    {
                        if (item.GetProperty("status").GetString() == "0")
                        {
                            if (item.GetProperty("canEval").GetBoolean()) result.Unevaluated.Add(new EvaluationItem(
                                item.GetProperty("id").GetString()!, item.GetProperty("teacherName").GetString()!,
                                item.GetProperty("teacherKcmc").GetString()!, item.GetProperty("endTime").GetString()!));
                            else result.UnexpiredOrNotStarted.Add(new EvaluationItem(
                                item.GetProperty("id").GetString()!, item.GetProperty("teacherName").GetString()!,
                                item.GetProperty("teacherKcmc").GetString()!, item.GetProperty("endTime").GetString()!));
                        }
                        else if (item.GetProperty("status").GetString() == "1") result.Evaluated.Add(new EvaluationItem(
                                item.GetProperty("id").GetString()!, item.GetProperty("teacherName").GetString()!,
                                item.GetProperty("teacherKcmc").GetString()!, item.GetProperty("submitTime").GetString()!));
                        else if (item.GetProperty("status").GetString() == "2") result.UnexpiredOrNotStarted.Add(new EvaluationItem(
                                item.GetProperty("id").GetString()!, item.GetProperty("teacherName").GetString()!,
                                item.GetProperty("teacherKcmc").GetString()!, item.GetProperty("endTime").GetString()!));
                    }
                    if (evaluationT.GetProperty("total").GetInt32() <= 50 * i) break;
                }
            }
            catch (Exception)
            {
                return null;
            }
            return result;
        }

        public static async Task<bool> EvaluateCourse(string id)
        {
            if (evaluateClient == null) return false;
            try
            {
                var res = await evaluateClient.GetStringAsync("http://218.197.101.69:8080/edu/task/evaluate/" + id);
                var html = new HtmlDocument();
                html.LoadHtml(res);

                var htmlForm = html.DocumentNode.SelectSingleNode("//div[@class='form-table']");
                var dict = new Dictionary<string, object>()
                {
                    {"taskId", htmlForm.SelectSingleNode("input[@name='taskId']").GetAttributeValue("value", "")},
                    {"dataId", htmlForm.SelectSingleNode("input[@name='dataId']").GetAttributeValue("value", "")},
                    {"kind", htmlForm.SelectSingleNode("input[@name='kind']").GetAttributeValue("value", "")},
                    {"dataName", htmlForm.SelectSingleNode("input[@name='dataName']").GetAttributeValue("value", "")},
                    {"xnxq", htmlForm.SelectSingleNode("input[@name='xnxq']").GetAttributeValue("value", "")},
                    {"teacherId", htmlForm.SelectSingleNode("input[@name='teacherId']").GetAttributeValue("value", "")},
                    {"teacherName", htmlForm.SelectSingleNode("input[@name='teacherName']").GetAttributeValue("value", "")},
                    {"code", htmlForm.SelectSingleNode("input[@name='code']").GetAttributeValue("value", "")},
                    {"kcmc", htmlForm.SelectSingleNode("input[@name='kcmc']").GetAttributeValue("value", "")},
                    { "suggestDetail", ""}
                };

                var table = htmlForm.SelectNodes("table/tbody/tr[@class='suject-box']");
                foreach (var tr in table)
                {
                    if (tr.SelectSingleNode("td/div[@class='exam-select' and @data-code='A']/i").GetAttributeValue("class", "") == "fa fa-square-o")
                    {
                        var codeA = tr.SelectSingleNode("td/div[@class='exam-select' and @data-code='A']");
                        var form = new Dictionary<string, string>()
                        {
                            {
                                "eduTaskRecord", ParseParam(new Dictionary<string, object>(dict)
                                {
                                    {"tabId", codeA.GetAttributeValue("data-tab", "") },
                                    {"subjectId", codeA.GetAttributeValue("data-subject", "")},
                                    {"optionId", codeA.GetAttributeValue("data-option", "")},
                                    {"optionCode", "A"},
                                    {"saved", true}
                                })
                            }
                        };
                        var aRes = await evaluateClient.PostAsync("http://218.197.101.69:8080/edu/task/record/save", new FormUrlEncodedContent(form));
                        var json = await aRes.Content.ReadFromJsonAsync<JsonElement>();
                        if (!json.GetProperty("success").GetBoolean()) return false;
                    }
                }
                var saveForm = new Dictionary<string, string>()
                {
                    {"eduTask", ParseParam(new Dictionary<string, object>(dict)
                    {
                        {"id", dict["taskId"]},
                        {"status", "1"}
                    })}
                };
                var saveRes = await evaluateClient.PostAsync("http://218.197.101.69:8080/edu/task/edit", new FormUrlEncodedContent(saveForm));
                var saveJson = await saveRes.Content.ReadFromJsonAsync<JsonElement>();
                if (! saveJson.GetProperty("success").GetBoolean()) return false;
                await Task.Delay(new Random().Next(250, 1000));
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static string ParseParam(Dictionary<string, object> dict)
        {
            var builder = new StringBuilder();
            builder.Append("{");
            foreach (var item in dict)
            {
                if (item.Value is int || item.Value is float) builder.Append($"\"{item.Key}\":{item.Value}");
                else if (item.Value is bool)
                {
                    builder.Append($"\"{item.Key}\":");
                    builder.Append((bool)item.Value ? "true" : "false");
                }
                else builder.Append($"\"{item.Key}\":\"{item.Value.ToString()}\"");
                builder.Append(',');
            }
            builder.Remove(builder.Length - 1, 1);
            builder.Append("}");
            return builder.ToString();
        }
    }
}