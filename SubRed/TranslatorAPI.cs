using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SubRed
{
    public class TranslatorAPI
    {
        private string targetLanguage;
        public bool SetTargetLanguage(string language)
        {
            switch (language)
            {
                case "eng":
                    targetLanguage = "";
                    break;
                case "chi":
                    targetLanguage = "";
                    break;
                case "jpn":
                    targetLanguage = "";
                    break;
                default:
                    return false;
            }
            return true;
        }
        public async Task<string> TranslateString(string text, string sourceLanguage = "auto")
        {
            if (text.Length > 0)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://libretranslate.com");
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("q", text),
                        new KeyValuePair<string, string>("source", sourceLanguage),
                        new KeyValuePair<string, string>("target", targetLanguage)
                    });
                    var result = await client.PostAsync("/translate", content);
                    string resultContent = await result.Content.ReadAsStringAsync();
                    return resultContent;
                }
            }
            else return "Error: Строка должна быть не пустой";
        }
        //https://libretranslate.com/
        //https://github.com/Grizley56/GoogleTranslateFreeApi

    }
}
