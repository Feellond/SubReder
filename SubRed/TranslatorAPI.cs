using ControlzEx.Standard;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace SubRed
{
    public static class TranslatorAPI
    {
        public static string userProxy = "";
        public static string DetectTextLanguage(string inputText)
        {
            int isRussianCount = 0, isEnglishCount = 0, isChineseCount = 0, isJapaneseCount = 0;

            foreach (char c in inputText)
            {
                if (c >= 'А' && c <= 'я')
                {
                    isRussianCount++;
                }
                else if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z')
                {
                    isEnglishCount++;
                }
                else if (c >= '\u4e00' && c <= '\u9fff')
                {
                    isChineseCount++;
                }
                else if (c >= '\u3040' && c <= '\u309f' || c >= '\u30a0' && c <= '\u30ff')
                {
                    isJapaneseCount++;
                }
            }

            if (isEnglishCount > isRussianCount && isEnglishCount > isChineseCount && isEnglishCount > isJapaneseCount)
                return "en";
            if (isChineseCount > isRussianCount && isChineseCount > isEnglishCount && isChineseCount > isJapaneseCount)
                return "zh";
            if (isJapaneseCount > isRussianCount && isJapaneseCount > isChineseCount && isJapaneseCount > isEnglishCount)
                return "ja";

            return "ru";
        }
        public static async Task<string> Translate(string text, string toLang)
        {
            string fromLang = DetectTextLanguage(text);
            string url = $"https://api.mymemory.translated.net/get?q={HttpUtility.UrlEncode(text)}&langpair={fromLang}|{toLang}";

            if (userProxy != null && userProxy != "" && userProxy.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0] != "")
            {
                try
                {
                    var proxy = new WebProxy
                    {
                        Address = new Uri(userProxy),   //задавать свой proxy сервер
                        BypassProxyOnLocal = false,
                        UseDefaultCredentials = false
                    };
                    // Create a client handler that uses the proxy
                    var httpClientHandler = new HttpClientHandler
                    {
                        Proxy = proxy,
                    };
                    // Disable SSL verification
                    httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                    using (HttpClient client = new HttpClient(handler: httpClientHandler, disposeHandler: true))
                    {
                        HttpResponseMessage response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();

                        dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
                        return result.responseData.translatedText;
                    }
                } catch (Exception ex)
                {
                    MessageBox.Show("Ошибка перевода текста. Возможно не правильно введен proxi. Попробуйте оставить пустой proxi",
                        "Ошибка перевода", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                return "Ошибка перевода текста.";
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
                    return result.responseData.translatedText;
                }
            }
        }
        //https://libretranslate.com/
        //https://github.com/Grizley56/GoogleTranslateFreeApi

    }
}
