using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CefSharp;
using CefSharp.WinForms;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MP3CutAd.App {
    static class CefSharpExtensions {
        public static Task<JavascriptResponse> ExecuteJsonAsync(this IJavascriptCallback callback, params object[] args) {
            var jsonArgs = new string[args.Length];
            for (var i = 0; i < args.Length; ++i) {
                var arg = args[i];
                jsonArgs[i] = JsonConvert.SerializeObject(arg);
            }
            return callback.ExecuteAsync(jsonArgs);
        }
    }
}
