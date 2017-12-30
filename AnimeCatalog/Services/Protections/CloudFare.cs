using Common;
using Common.Logging;
using JadeFlix.Services;
using System;
using System.Net;
using System.Text;
using System.Threading;

namespace TvShows.Domain.SiteProtections
{
    public class CloudFare : SiteProtection
    {
        public CloudFare() : base("CloudFare")
        {
        }

        public override Uri ProcessRequest(Uri baseUrl, WebRequest request, WebResponse response)
        {
            string redirect = SolveChallenge(baseUrl, response);
            Logger.Debug("Waiting ...");
            Thread.Sleep(10000);
            return new Uri(redirect);
        }

        private string SolveChallenge(Uri baseUrl, WebResponse response)
        {
            Logger.Debug("Solving Challenge ...");
            var httpResponse = response as HttpWebResponse;
            if (httpResponse == null) return string.Empty;

            //var rayId = httpResponse.Headers["CF-RAY"];
            var refresh = httpResponse.Headers["Refresh"];
            string data;
            using (var stm = response.GetResponseStream())
            {
                data = stm.ReadToEndAsync().Result;
            }
            var script = StripChallengeScript(data);
            var retVal = EvalScript(PreprocessChallengeScript(baseUrl.ToString(), script));
            Logger.Debug("Challenge result = " + retVal);

            if (!int.TryParse(retVal, out int i)) return string.Empty;

            var idx = refresh.IndexOf("URL=", StringComparison.Ordinal);
            var burl = GetBaseUri(baseUrl);
            var redirection = burl + refresh.Substring(idx + 4) + "&jschl-answer=" + i;
            Logger.Debug("Redirect Url: " + redirection);

            return redirection;
        }

        private string StripChallengeScript(string html)
        {
            Logger.Debug("Stripping Challenge Script ...");
            var startIndex = html.IndexOf("setTimeout(function(){", StringComparison.Ordinal);
            if (startIndex < 0) return string.Empty;
            startIndex += "setTimeout(".Length;
            var endIndex = html.IndexOf("},", startIndex, StringComparison.Ordinal);
            if (endIndex < 0) return string.Empty;
            return html.Substring(startIndex, endIndex - startIndex);
        }

        private string PreprocessChallengeScript(string baseUrl, string html)
        {
            StringBuilder sb = new StringBuilder();
            var lines = html.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            var varName = string.Empty;
            foreach (var line in lines)
            {
                var lowline = line.ToLower().Trim();
                if (string.IsNullOrEmpty(lowline.Replace("\n", "").Trim())) continue;
                if (lowline.Replace("\n", "").Trim() == "'") continue;

                if (lowline.Contains("createelement('div')") ||
                    lowline.Contains("innerhtml") ||
                    lowline.Contains("challenge-form") ||
                    lowline.Contains("submit()"))
                {
                }
                else if (lowline.Contains("function()"))
                {
                    sb.AppendLine(line.Replace("function(){", ""));
                }
                else if (lowline.Contains("firstchild.href"))
                {
                    sb.AppendLine(line.Replace(".firstChild.href", " || \"" + GetBaseUri(baseUrl).Trim() + "\";"));
                }
                else if (lowline.Contains("document.getelementbyid('jschl-answer')"))
                {
                    varName = line.Replace("document.getElementById('jschl-answer')", "").Replace("=", "").Trim();
                    sb.AppendLine(varName + " = {'value':'0'};");
                }
                else
                {
                    sb.AppendLine(line.Trim() + ";");
                }
            }
            sb.AppendLine("return " + varName + ".value;");
            return sb.ToString();
        }

        private string EvalScript(string script)
        {
            Logger.Debug("Evaluating script ...");
            return JsEngine.ExecuteFunction(script);
        }

        public override bool IsActive(WebResponse response)
        {
            if (response is HttpWebResponse httpResponse && httpResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                var rayId = httpResponse.Headers["CF-RAY"];
                var refresh = httpResponse.Headers["Refresh"];
                if (!string.IsNullOrEmpty(rayId) && !string.IsNullOrEmpty(refresh))
                {
                    Logger.Debug(Name + " is active");
                    return true;
                }
            }
            return false;
        }
    }
}
