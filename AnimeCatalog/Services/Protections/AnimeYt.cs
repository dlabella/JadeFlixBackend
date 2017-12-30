using System;
using System.Collections.Generic;
using System.Text;

namespace JadeFlix.Services.Protections
{
    public class AnimeYt
    { 
        public string GetToken()
        {
            //string js = "http://s.animeyt.tv/v4/gsuite.js?v=01";
            //var gsuite = AppContext.Web.Read(js);
            //gsuite = gsuite + Environment.NewLine + "(return _xenox();)();";
            var gsuite = Properties.Resources.gst_js;
            var token = JsEngine.ExecuteFunction(gsuite, "_xenox");
            return token;
        }
    }
}
