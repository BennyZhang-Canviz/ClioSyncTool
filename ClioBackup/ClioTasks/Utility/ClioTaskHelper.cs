using ClioTasks.Models;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ClioTasks.Utility
{
    public static class ClioTaskHelper
    {
        public static async Task<ClioTaskModel> GetAllCloiTasks()
        {
            string url = "https://app.clio.com/api/v2/tasks";
            string result = await HttpClientHelper.GetResponseAsync(url, ConfigurationManager.AppSettings["clio_token"]);
            var model = JsonConvert.DeserializeObject<ClioTaskModel>(result);
            return model;
        }

        public static async Task<ClioTaskModel> GetAllCloiTasksWithOutlookTask(string outlookAccessToken)
        {
            var model = await GetAllCloiTasks();
            foreach (var item in model.tasks)
            {
                string uri = string.Format("https://outlook.office.com/api/v2.0/me/tasks?$filter=SingleValueExtendedProperties%2FAny(ep%3A%20ep%2FPropertyId%20eq%20'String%20{{{0}}}%20Name%20{1}'%20and%20ep%2FValue%20eq%20'{2}')",
                         ConfigurationManager.AppSettings["outlook_customfield_guid"],
                         ConfigurationManager.AppSettings["outlook_customfield_name"], item.id);
                var response = await HttpClientHelper.GetResponseAsync(uri, outlookAccessToken);
                if (string.IsNullOrEmpty(response))
                {
                    continue;
                }
                var outlookTask = JsonConvert.DeserializeObject<OutlookTasks>(response);
                if (outlookTask != null && outlookTask.Tasks.Count > 0)
                    item.OutlookTask = outlookTask.Tasks[0];
            }
            return model;
        }

        public static async Task<ClioSingleTaskModel> GetClioTaskByIdAsync(string clioId)
        {
            string url = "https://app.clio.com/api/v2/tasks/" + clioId;
            string result = await HttpClientHelper.GetResponseAsync(url, 
                ConfigurationManager.AppSettings["clio_token"]);
            var model = JsonConvert.DeserializeObject<ClioSingleTaskModel>(result);
            return model;
        }
        public static async Task<string> DeleteClioTaskByIdAsync(string clioId)
        {
            string url = "https://app.clio.com/api/v2/tasks/" + clioId;
            string result = await HttpClientHelper.DeleteAsync(ConfigurationManager.AppSettings["clio_token"], url);
            return result;
        }

    }
}