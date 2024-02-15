using System.Net.Http;
using System.Threading.Tasks;

namespace SocialConnector.Helpers
{
    public class HttpClientHelper
    {
        public static async Task<string> GetAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
