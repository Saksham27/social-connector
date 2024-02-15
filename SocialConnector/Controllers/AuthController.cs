using System.Web;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace SocialConnector.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        // Action method to initiate the Facebook login process
        public ActionResult FacebookLogin()
        {
            // Construct the Facebook login URL
            var loginUrl = "https://www.facebook.com/v12.0/dialog/oauth";
            var queryParams = new Dictionary<string, string>
            {
                { "client_id", "359737446942540" },
                { "redirect_uri", "https://localhost:7137/auth/facebookcallback" },
                { "response_type", "code" },
                { "scope", "pages_read_engagement,pages_read_user_content" } // Add other scopes as needed
            };
            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
            var fullUrl = $"{loginUrl}?{queryString}";

            // Redirect the user to Facebook login page
            return Redirect(fullUrl);
        }

        // Action method to handle the callback from Facebook after login
        public async Task<ActionResult> FacebookCallback(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                // Handle error case
                return RedirectToAction("LoginFailed");
            }

            try
            {
                // Exchange code for access token
                var tokenUrl = "https://graph.facebook.com/v12.0/oauth/access_token";
                var tokenParams = new Dictionary<string, string>
                {
                    { "client_id", "359737446942540" },
                    { "redirect_uri", "https://localhost:7137/auth/facebookcallback" },
                    { "client_secret", "ffae1390f15966c176ef763ac41b95c4" },
                    { "code", code }
                };

                using (var httpClient = new HttpClient())
                {
                    var tokenResponse = await httpClient.GetAsync($"{tokenUrl}?{string.Join("&", tokenParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"))}");
                    tokenResponse.EnsureSuccessStatusCode();

                    var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
                    var accessToken = JObject.Parse(tokenJson)["access_token"].Value<string>();

                    // Fetch user data using the access token
                    var userDataUrl = $"https://graph.facebook.com/me?fields=id,name,email&access_token={accessToken}";
                    var userDataResponse = await httpClient.GetAsync(userDataUrl);
                    userDataResponse.EnsureSuccessStatusCode();

                    var userDataJson = await userDataResponse.Content.ReadAsStringAsync();
                    var userData = JObject.Parse(userDataJson);

                    // Process user data as needed
                    var userEmail = userData["email"].Value<string>();
                    var userName = userData["name"].Value<string>();

                    // Here you can handle user authentication or registration
                    // For example, you can create a new user account in your system
                    // based on the retrieved email and name
                    
                    return RedirectToAction("LoggedIn", new { Name = userName, Email = userEmail });
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                return RedirectToAction("LoginFailed");
            }
        }

        // Action method to display a view indicating successful login
        public ActionResult LoggedIn(string name, string email)
        {
            ViewBag.Name = name;
            ViewBag.Email = email;
            return View();
        }

        // Action method to display a view indicating login failure
        public ActionResult LoginFailed()
        {
            return View();
        }
    }
}
