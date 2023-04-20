using ControlzEx.Standard;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SubRed
{
    public static class TranslatorAPI
    {
        public static string DetectTextLanguage(string inputText)
        {
            string text = inputText;
            text = text.ToLower();

            // Создание массивов символов для каждого языка
            char[] russianLetters = { 'а', 'б', 'в', 'г', 'д', 'е', 'ё', 'ж', 'з', 'и', 'й', 'к', 'л', 'м', 'н', 'о', 'п', 'р', 'с', 'т', 'у', 'ф', 'х', 'ц', 'ч', 'ш', 'щ', 'ъ', 'ы', 'ь', 'э', 'ю', 'я' };
            char[] englishLetters = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

            // Счетчики символов для каждого языка
            int russianCount = 0;
            int englishCount = 0;

            // Перебор символов входной строки
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                // Проверка, является ли символ русской буквой
                if (Array.IndexOf(russianLetters, char.ToLower(c)) != -1)
                {
                    russianCount++;
                }

                // Проверка, является ли символ английской буквой
                if (Array.IndexOf(englishLetters, char.ToLower(c)) != -1)
                {
                    englishCount++;
                }
            }

            // Определение языка по количеству символов каждого языка
            if (russianCount > englishCount)
            {
                return "ru";
            }
            else
            {
                return "en";
            }
        }
        public static async Task<string> Translate(string text, string toLang)
        {
            string fromLang = DetectTextLanguage(text);
            

            string url = $"https://api.mymemory.translated.net/get?q={HttpUtility.UrlEncode(text)}&langpair={fromLang}|{toLang}";

            var proxy = new WebProxy
            {
                Address = new Uri($"http://172.104.241."),   //задавать свой proxy сервер
                BypassProxyOnLocal = false,
                UseDefaultCredentials = false
            };

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
