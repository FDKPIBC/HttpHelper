using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;

namespace CSharperHttp
{
    /// <summary>
    /// Post请求内容的类型
    /// </summary>
    public enum PostDataType
    {
        String,
        Byte,
    }
    /// <summary>
    /// Cookie的类型
    /// </summary>
    public enum CookieType
    {
        String,
        CookieCollection,
    }
    /// <summary>
    /// 返回内容的类型
    /// </summary>
    public enum ResultDataType
    {
        String,
        Byte
    }
    /// <summary>
    /// Http请求参数
    /// </summary>
    public class HttpItem
    {
        /// <summary>
        /// 网址
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// POST数据,string类型,具体提交string还是byte[]取决于PostDataType
        /// </summary>
        public string PostData { get; set; }
        /// <summary>
        /// POST数据,Byte数组类型,具体提交string还是byte[]取决于PostDataType
        /// </summary>
        public byte[] PostByte { get; set; }
        private PostDataType _postDataType = PostDataType.String;
        /// <summary>
        /// Post数据类型,默认String
        /// </summary>
        public PostDataType PostDataType
        {
            get { return _postDataType; }
            set { _postDataType = value; }
        }
        private string _resultEncoding = "UTF-8";
        /// <summary>
        /// 返回编码类型,默认UTF-8
        /// </summary>
        public string ResultEncoding
        {
            get { return _resultEncoding; }
            set { _resultEncoding = value; }
        }
        /// <summary>
        /// 来路
        /// </summary>
        public string Referer { get; set; }
        private string _userAgent = "Mozilla/5.0";
        /// <summary>
        /// 请求UA,默认为Mozilla/5.0
        /// </summary>
        public string UserAgent
        {
            get { return _userAgent; }
            set { _userAgent = value; }
        }
        private string _method = "GET";
        /// <summary>
        /// 请求方式,默认GET
        /// </summary>
        public string Method
        {
            get { return _method; }
            set { _method = value; }
        }
        /// <summary>
        /// 文本型提交Cookie,具体提交string还是CookieCollection根据HttpItem制定的CookieType
        /// </summary>
        public string Cookie { get; set; }
        /// <summary>
        /// CookieCollection型提交Cookie,具体提交string还是CookieCollection根据HttpItem制定的CookieType
        /// </summary>
        public CookieCollection CookieCollection { get; set; }

        private CookieType _cookieType = CookieType.String;
        /// <summary>
        /// 提交请求的Cookie类型,默认为string
        /// </summary>
        public CookieType CookieType
        {
            get { return _cookieType; }
            set { _cookieType = value; }
        }

        private WebHeaderCollection _header = new WebHeaderCollection();
        /// <summary>
        /// 自定义请求头
        /// </summary>
        public WebHeaderCollection Header
        {
            get { return _header; }
            set { _header = value; }
        }
        /// <summary>
        /// 代理,格式:127.0.0.1:8888
        /// </summary>
        public string Proxy { get; set; }

        private int _overTime = 10 * 1000;
        /// <summary>
        /// 请求超时时间,默认10秒
        /// </summary>
        public int OverTime
        {
            get { return _overTime; }
            set { _overTime = value; }
        }
        /// <summary>
        /// 是否跳转,默认false
        /// </summary>
        public bool AllowAutoRedirect { get; set; }

        private string _contentType = "text/html";
        /// <summary>
        /// contenttype值,默认text/html
        /// </summary>
        public string ContentType
        {
            get { return _contentType; }
            set { _contentType = value; }
        }
        /// <summary>
        /// Expect100Continue,默认为false
        /// </summary>
        public bool Expect100Continue { get; set; }
        private ResultDataType _resultDataType = ResultDataType.String;
        /// <summary>
        /// 返回数据的类型,默认为string
        /// </summary>
        public ResultDataType ResultDataType
        {
            get { return _resultDataType; }
            set { _resultDataType = value; }
        }
        /// <summary>
        /// 伪造IP值
        /// </summary>
        public string XForwardedFor { get; set; }
        /// <summary>
        /// XRequestedWith值
        /// </summary>
        public string XRequestedWith { get; set; }

    }
    /// <summary>
    /// Http请求后返回的结果
    /// </summary>
    public class HttpResult
    {
        private string _html = string.Empty;
        /// <summary>
        /// 请求返回的网页代码,具体返回string还是Byte[]根据HttpItem制定的ResultDataType
        /// </summary>
        public string Html
        {
            get { return _html; }
            set { _html = value; }
        }
        /// <summary>
        /// 返回的Byte[],具体返回string还是Byte[]根据HttpItem制定的ResultDataType
        /// </summary>
        public byte[] Bytes { get; set; }
        /// <summary>
        /// 错误信息,若没有则为Null值
        /// </summary>
        public string ErrorInfo { get; set; }

        private string _cookie = string.Empty;
        /// <summary>
        /// 返回的Cookie,具体返回string还是CookieCollection根据HttpItem制定的CookieType
        /// </summary>
        public string Cookie
        {
            get { return _cookie; }
            set { _cookie = value; }
        }
        private CookieCollection _cookieCollection = new CookieCollection();
        /// <summary>
        /// 返回的CookieCollection,具体返回string还是CookieCollection根据HttpItem制定的CookieType
        /// </summary>
        public CookieCollection CookieCollection
        {
            get { return _cookieCollection; }
            set { _cookieCollection = value; }
        }

        private WebHeaderCollection _webHeader = new WebHeaderCollection();
        /// <summary>
        /// 获取请求返回的WebHeaderCollection
        /// </summary>
        public WebHeaderCollection Header
        {
            get { return _webHeader; }
            set { _webHeader = value; }
        }





        /// <summary>
        /// 根据Cookie名称获取Cookie值
        /// </summary>
        /// <param name="cookieName">Cookie名称</param>
        /// <param name="isResultName">返回的时候是否带上Cookie名称,默认false</param>
        /// <returns></returns>
        public string GetCookieValue(string cookieName, bool isResultName = false)
        {
            if (!string.IsNullOrEmpty(this.Cookie))
            {
                Match mc = Regex.Match(this.Cookie, cookieName + "=([^;]+)");
                string value = mc.Groups[0].Value.Replace(cookieName + "=", "");
                return isResultName ? cookieName + ":" + value + ";" : value;
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// 返回的状态码
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }


    }

    public class HttpHelper
    {
        /// <summary>
        /// 是否自动处理Cookie,默认为false.当为true时,会自动处理cookie,httpitem的cookie将不用
        /// </summary>
        public bool AutoCookie { get; set; }
        /// <summary>
        /// 当AutoCookie为true时自动处理的Cookie容器
        /// </summary>
        private CookieContainer _cc = new CookieContainer();

        /// <summary>
        /// 设置WebHeaderCollection
        /// </summary>
        /// <param name="item">HttpItem请求参数</param>
        /// <param name="hwr">要设置的HttpWebRequest</param>
        /// <returns></returns>
        private void SetWebHeader(HttpItem item, HttpWebRequest hwr)
        {
            var header = item.Header;
            if (!AutoCookie)
            {
                if (item.CookieType == CookieType.CookieCollection && item.CookieCollection != null)
                {
                    hwr.CookieContainer.Add(item.CookieCollection);
                }
                else
                {
                    header.Add("Cookie", item.Cookie);
                }
            }
            else
            {
                hwr.CookieContainer = _cc;
            }
            if (!string.IsNullOrEmpty(item.XForwardedFor))
            {
                header.Add("x-forwarded-for", item.XForwardedFor);
                header.Add("client_ip", item.XForwardedFor);
            }
            if (!string.IsNullOrEmpty(item.XRequestedWith))
            {
                header.Add("x-requested-with", item.XRequestedWith);
            }
            hwr.Headers = header;
            hwr.ContentType = item.ContentType;
            hwr.Proxy = string.IsNullOrEmpty(item.Proxy) ? hwr.Proxy = null : hwr.Proxy = new WebProxy(item.Proxy.Replace("：", ""));
            hwr.Referer = item.Referer;
            hwr.AllowAutoRedirect = item.AllowAutoRedirect;
            hwr.ServicePoint.Expect100Continue = item.Expect100Continue;
            hwr.Method = item.Method.ToUpper();
            hwr.Timeout = item.OverTime;
            hwr.UserAgent = item.UserAgent;
            item.Method = item.Method.ToUpper();
        }
        /// <summary>
        /// 执行Http请求
        /// </summary>
        /// <param name="item">HttpItem请求参数</param>
        /// <returns>HttpResult类型结果</returns>
        public HttpResult GetHtml(HttpItem item)
        {
            HttpWebRequest hwr = null;
            var result = new HttpResult();
            try
            {
                hwr = (HttpWebRequest)WebRequest.Create(item.Url);
            }
            catch (Exception e)
            {
                result.ErrorInfo = e.Message;
                return result;
            }
            SetWebHeader(item, hwr);


            if (item.Method == "POST")
            {
                //构建POST内容
                var buffer = new List<byte>();
                if (item.PostDataType == PostDataType.String && !string.IsNullOrEmpty(item.PostData))
                {
                    buffer.AddRange(Encoding.Default.GetBytes(item.PostData));
                }
                else if (item.PostDataType == PostDataType.Byte && item.PostByte.Length > 0)
                {
                    buffer.AddRange(item.PostByte);
                }
                else
                {
                    result.ErrorInfo = "没有POST内容";
                    return result;
                }
                hwr.ContentLength = buffer.Count;
                try
                {
                    hwr.ContentLength = buffer.Count;
                    hwr.GetRequestStream().Write(buffer.ToArray(), 0, buffer.Count);
                    buffer.Clear();
                }
                catch (Exception e)
                {
                    result.ErrorInfo = e.Message;
                    return result;
                }
            }
            HttpWebResponse response = null;
            try
            {
                using (response = (HttpWebResponse)hwr.GetResponse())
                {
                    return GetData(item, response, result, hwr);
                }
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
                return GetData(item, response, result, hwr);
            }
            catch (Exception e)
            {
                result.ErrorInfo = e.Message;
                return result;
            }


        }

        private HttpResult GetData(HttpItem item, HttpWebResponse response, HttpResult result, HttpWebRequest hwr)
        {
            StreamReader stream = null;
            Stream ss = null;
            try
            {
                using (ss = response.GetResponseStream())
                {
                    using (stream = new StreamReader(ss, Encoding.GetEncoding(item.ResultEncoding)))
                    {
                        //构建返回内容
                        if (AutoCookie)
                        {
                            _cc.Add(response.Cookies);
                        }
                        result.StatusCode = response.StatusCode;
                        if (item.CookieType == CookieType.String)
                        {
                            result.Cookie = response.Headers["Set-Cookie"];
                        }
                        else
                        {
                            result.CookieCollection = response.Cookies;
                        }
                        result.Header = response.Headers;
                        if (item.ResultDataType == ResultDataType.Byte)
                        {
                            var length = response.ContentLength;
                            var bytes = new byte[length];
                            ss.Read(bytes, 0, (int)length);
                            result.Bytes = bytes;
                        }
                        else
                        {
                            try
                            {
                                result.Html = stream.ReadToEnd();
                            }
                            catch (Exception ex)
                            {
                                result.Html = ex.Message;
                            }
                        }
                        hwr.Abort();
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                result.ErrorInfo = e.Message;
                return result;
            }
        }

        /// <summary>
        /// 随机生成IP地址
        /// </summary>
        /// <returns>返回string类型的IP地址</returns>
        public static string GetRandomIp()
        {
            var sb = new StringBuilder();
            var r = new Random();
            sb.Append(r.Next(1, 256) + ".");
            sb.Append(r.Next(1, 256) + ".");
            sb.Append(r.Next(1, 256) + ".");
            sb.Append(r.Next(1, 256));
            return sb.ToString();
        }

        /// <summary>  
        /// 获取时间戳  
        /// </summary>  
        /// <returns></returns>  
        public static string GetTimeStamp()
        {
            return RunJavaScript("function getTimeStamp (){return new Date().getTime();}", "getTimeStamp", null).ToString();
        }
        /// <summary>
        /// 执行JavaScript
        /// </summary>
        /// <param name="code">js代码</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="pms">参数</param>
        /// <returns></returns>
        public static object RunJavaScript(string code,string methodName ,params object[] pms)
        {
            var js = new CreateObject("MSScriptControl.ScriptControl");
            js["Language"] = "JavaScript";
            //构建JS代码
            var jsCode = new StringBuilder();
            var temp = js.DoMethod("AddCode", code);
           return js.DoMethod("Run", "methodName",pms);
        }
        /// <summary>
        /// 计算MD5值得到结果
        /// </summary>
        /// <param name="md5Str">欲加密的文本</param>
        /// <returns>加密后的文本</returns>
        public static string GetMd5(string md5Str)
        {
            var md5 = MD5.Create();
            var buffer = Encoding.UTF8.GetBytes(md5Str);
            var output = md5.ComputeHash(buffer);
            return BitConverter.ToString(output).Replace("-", "");
        }

        /// <summary>
        /// 随机获取数字
        /// </summary>
        /// <param name="digits">位数</param>
        /// <returns></returns>
        public static string GetRandomNum(int digits)
        {
            var r = new Random();
            var sb = new StringBuilder();
            for (int i = 0; i < digits; i++)
            {
                sb.Append(r.Next(0, 10));
            }
            return sb.ToString();
        }
        /// <summary>
        ///  取文本中间值
        /// </summary>
        /// <param name="allStr">完整文本</param>
        /// <param name="leftStr">左边文本</param>
        /// <param name="rightStr">右边文本</param>
        /// <returns>返回结果</returns>
        public static string Mids(string allStr, string leftStr, string rightStr)
        {
            if (string.IsNullOrEmpty(allStr) || string.IsNullOrEmpty(leftStr) || string.IsNullOrEmpty(rightStr))
            {
                return string.Empty;
            }
            var leftIndex = allStr.IndexOf(leftStr, System.StringComparison.Ordinal) + leftStr.Length;
            var rightIndex = allStr.IndexOf(rightStr, leftIndex, System.StringComparison.Ordinal);
            var resultStr = allStr.Substring(leftIndex, rightIndex - leftIndex - rightStr.Length);
            return resultStr;
        }
        /// <summary>
        ///  批量取文本中间值
        /// </summary>
        /// <param name="allStr">完整文本</param>
        /// <param name="leftStr">左边文本</param>
        /// <param name="rightStr">右边文本</param>
        /// <returns>返回结果</returns>
        public static string[] MidsArr(string allStr, string leftStr, string rightStr)
        {
            var list = new List<string>();
            var x = allStr.IndexOf(leftStr, System.StringComparison.Ordinal);
            var y = 0;
            while (x != -1 && y != -1)
            {
                var temp = x + 1;
                y = allStr.IndexOf(rightStr, x, System.StringComparison.Ordinal);
                list.Add(allStr.Substring(x + leftStr.Length, y - x - leftStr.Length));
                x = allStr.IndexOf(leftStr, temp, System.StringComparison.Ordinal);
            }
            return list.ToArray();
        }
        /// <summary>
        /// 去除Html标记
        /// </summary>
        /// <param name="html">html文本</param>
        /// <returns></returns>
        public static string NoHtml(string html)
        {
            html = Regex.Replace(html, "<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, "[<].*?[>]", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, "-->", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, "<!--.*", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, "&(quot|#34);", "\"", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, "&(amp|#38);", "&", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, "&(lt|#60);", "<", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, "&(gt|#62);", ">", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, "&(nbsp|#160);", " ", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, "&(iexcl|#161);", "\x00a1", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, "&(cent|#162);", "\x00a2", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, "&(pound|#163);", "\x00a3", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, "&(copy|#169);", "\x00a9", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&#(\d+);", "", RegexOptions.IgnoreCase);
            html.Replace("<", "");
            html.Replace(">", "");
            html.Replace("\r\n", "");
            return html;
        }
        /// <summary>
        /// 获取随机字符
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns></returns>
        public static string GetRandomChar(int count)
        {
            var sb = new StringBuilder();
            var r = new Random();
            for (int i = 0; i < count; i++)
            {
                var temp = new string[3];
                temp[0] = Convert.ToChar(r.Next('a', 'z' + 1)).ToString();
                temp[1] = Convert.ToChar(r.Next('A', 'Z' + 1)).ToString();
                temp[2] = Convert.ToChar(r.Next('0', '9' + 1)).ToString();
                sb.Append(temp[r.Next(0, 3)]);
            }
            return sb.ToString();
        }

        public static string UrlEncoding(string url, Encoding e)
        {
            return Regex.Replace(url, "[^a-zA-Z0-9]", delegate(Match match) { return "%" + BitConverter.ToString(e.GetBytes(match.Value)).Replace("-", "%"); });
        }
    }

    class CreateObject
    {
        private readonly Type _type;
        private readonly object _obj;
        /// <summary>
        /// 构造函数创建对象
        /// </summary>
        /// <param name="objName">对象名称</param>
        public CreateObject(string objName)
        {
            this._type = Type.GetTypeFromProgID(objName);
            if (this._type == null) throw new Exception(@"名称无效");
            this._obj = Activator.CreateInstance(_type);
        }
        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="methodName">方法名称</param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        public object DoMethod(string methodName, params object[] args)
        {
            return _type.InvokeMember(methodName, BindingFlags.InvokeMethod, null, _obj, args);
        }

        //设置属性
        public object this[string propName]
        {
            get { return _type.InvokeMember(propName, BindingFlags.GetProperty, null, _obj, null); }
            set { _type.InvokeMember(propName, BindingFlags.SetProperty, null, _obj, new object[] { value }); }
        }
    }
}

namespace CSharperHttp
{
    public class CookieHelper
    {
        /// <summary>
        /// 根据Cookie名称获取Cookie值
        /// </summary>
        /// <param name="cookieStr">Cookie文本</param>
        /// <param name="cookieName">Cookie值</param>
        /// <returns></returns>
        public static string GetCookieValue(string cookieStr, string cookieName)
        {
            if (!string.IsNullOrEmpty(cookieStr))
            {
                Match mc = Regex.Match(cookieStr, cookieName + "=([^;]+)");
                string value = mc.Groups[0].Value.Replace(cookieName + "=", "");
                return cookieName + ":" + value + ";";
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// 格式化Cookie为String,例:uid:http;
        /// </summary>
        /// <param name="cookieName">cookie名称</param>
        /// <param name="cookieValue">cookie值</param>
        /// <returns></returns>
        public static string FormatCookieToString(string cookieName, string cookieValue)
        {
            return string.Format("{0}:{1};", cookieName, cookieValue);
        }
        /// <summary>
        /// 格式化Cookie为Cookie类型
        /// </summary>
        /// <param name="cookieName">cookie名称</param>
        /// <param name="cookieValue">cookie值</param>
        /// <returns></returns>
        public static Cookie FormatCookie(string cookieName, string cookieValue)
        {
            return new Cookie(cookieName, cookieValue);
        }
        /// <summary>
        /// 根据字符生成Cookie列表
        /// </summary>
        /// <param name="cookieStr">Cookie字符串</param>
        /// <returns></returns>
        public static List<CookieItem> GetCookieItem(string cookieStr)
        {
            var cookieList = new List<CookieItem>();
            foreach (string item in cookieStr.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (Regex.IsMatch(item, @"([\s\S]*?)=([\s\S]*?)$"))
                {
                    Match m = Regex.Match(item, @"([\s\S]*?)=([\s\S]*?)$");
                    cookieList.Add(new CookieItem() { Name = m.Groups[1].Value, Value = m.Groups[2].Value });
                }
            }
            return cookieList;
        }
    }

    public class CookieItem
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }
    }
}
