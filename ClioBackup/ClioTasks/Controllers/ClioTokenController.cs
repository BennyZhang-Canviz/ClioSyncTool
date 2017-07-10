using ClioTasks.Models;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ClioTasks.Controllers
{
    public class ClioTokenController : Controller
    {
        private string client_id = ConfigurationManager.AppSettings["clio_client_id"];
        private string client_secret = ConfigurationManager.AppSettings["clio_client_secret"];
        private string return_url = ConfigurationManager.AppSettings["clio_return_url_base"];
        
        // GET: Clio
        public async Task<ActionResult> Index(string code)
        {
            string token = "";
            if (string.IsNullOrEmpty(code))
            {
                string url = string.Format("https://app.clio.com/oauth/authorize?response_type=code&client_id={0}&redirect_uri={1}", client_id, HttpUtility.UrlEncode(return_url + "Index"));
                return Redirect(url);
            }
            else {
                token =await GetAccessTokenAsync(code);
                UpdateConfig(token);
            }
            TempData["token"] = token;
            return View();
        }
        private async Task<string> GetAccessTokenAsync(string code)
        {
            string url = string.Format("https://app.clio.com/oauth/token?client_id={0}&client_secret={1}&grant_type=authorization_code&code={2}&redirect_uri={3}",
                client_id, client_secret, code, HttpUtility.UrlEncode(return_url + "Index"));
            using (var httpClient = new HttpClient())
            {
                HttpContent content = new StringContent("");
                var response = await httpClient.PostAsync(url, content);

                //will throw an exception if not successful
                response.EnsureSuccessStatusCode();

                string result = await response.Content.ReadAsStringAsync();
                var token = JsonConvert.DeserializeObject<TokenModel>(result);
                return token.access_token;
            }
           
        }

        private void UpdateConfig(string token)
        {
            Configuration objConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            AppSettingsSection objAppsettings = (AppSettingsSection)objConfig.GetSection("appSettings");
            //Edit
            if (objAppsettings != null)
            {
                objAppsettings.Settings["clio_token"].Value = token;
                objConfig.Save();
            }
        }

    }
}