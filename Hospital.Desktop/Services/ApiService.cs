using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Hospital.Desktop.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private static string _token;
        private const string BaseUrl = "https://localhost:7278/api/";

        public ApiService()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };

            if (!string.IsNullOrEmpty(_token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            }
        }

        public void SetToken(string token)
        {
            _token = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }
        private async Task HandleError(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            string cleanMessage = "خطأ غير معروف";

            try
            {
                var errorData = JObject.Parse(content);
                cleanMessage = errorData["message"]?.ToString() ?? "خطأ في السيرفر";

                if (errorData["errors"] != null)
                {
                    var details = string.Join("\n", errorData["errors"]);
                    cleanMessage += "\n" + details;
                }
            }
            catch
            {
                cleanMessage = "تعذر الاتصال بالسيرفر أو استجابة غير صالحة.";
            }

            throw new Exception(cleanMessage);
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }

            await HandleError(response);
            return default;
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            await HandleError(response);
            return default;
        }
    }
}