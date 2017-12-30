using Jint;
using Jint.Runtime;
using System;

namespace JadeFlix.Services
{
    public class JsEngine
    {
        public static string ExecuteFunction(string body)
        {
            var evalFunction = new Engine();
            evalFunction.Execute("function evalFunction() { " + body + "}");
            return evalFunction.Invoke("evalFunction").ToString();
        }

        public static string ExecuteFunction(string body,string function, params object[] parameters)
        {
            var engine = new Engine();
            try
            {

                engine.Execute(body);
                return engine.Invoke(function, parameters).ToString();
            }
            catch (JavaScriptException exc)
            {
                var location = engine.GetLastSyntaxNode().Location.Start;
                throw new ApplicationException(
                  String.Format("{0} (Line {1}, Column {2})",
                    exc.Error,
                    location.Line,
                    location.Column
                    ), exc);
            }
        }
    }
}
