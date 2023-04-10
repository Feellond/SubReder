using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SubRed
{
    public static class TranslatorAPI
    {
        public static async Task<string> Translate(string text, string fromLang, string toLang)
        {
            string url = $"https://api.mymemory.translated.net/get?q={HttpUtility.UrlEncode(text)}&langpair={fromLang}|{toLang}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
                return result.responseData.translatedText;
            }
        }
        //https://libretranslate.com/
        //https://github.com/Grizley56/GoogleTranslateFreeApi

    }
}
