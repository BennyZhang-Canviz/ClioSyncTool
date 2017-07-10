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
    public static class OutlookTaskHelper
    {
        public static async Task<List<OutlookTaskModel>> GetAllTasksAsync(string token)
        {
            string uri = string.Format("https://outlook.office.com/api/v2.0/me/tasks?$expand=SingleValueExtendedProperties($filter=PropertyId%20eq%20'String%20{{{0}}}%20Name%20{1}')",
                ConfigurationManager.AppSettings["outlook_customfield_guid"], ConfigurationManager.AppSettings["outlook_customfield_name"]);

            var response = await HttpClientHelper.GetResponseAsync(uri, token);//GetRequestAsync(token, uri);
            var result = JsonConvert.DeserializeObject<OutlookTasks>(response);
            foreach(var item in result.Tasks)
            {
                if (item.SingleValueExtendedProperties != null)
                {
                    var clioId = item.SingleValueExtendedProperties[0].Value;
                    if (!string.IsNullOrEmpty(clioId))
                    {
                        var clioTask =await ClioTaskHelper.GetClioTaskByIdAsync(clioId);
                        if (clioTask != null)
                        {
                            item.ClioTask = clioTask.task;
                        }
                    }
                }
            }
            return result.Tasks;
        }
        public static async Task<OutlookTaskModel> GetOutlookTaskByIdAsync(string id, string token)
        {
            string uri = string.Format("https://outlook.office.com/api/v2.0/me/tasks('{0}')?$expand=SingleValueExtendedProperties($filter=PropertyId%20eq%20'String%20{{{1}}}%20Name%20{2}')", id,
                ConfigurationManager.AppSettings["outlook_customfield_guid"], ConfigurationManager.AppSettings["outlook_customfield_name"]);

            var response = await HttpClientHelper.GetResponseAsync(uri, token); //await GetRequestAsync(token, uri);
            var result = JsonConvert.DeserializeObject<OutlookTaskModel>(response);
            return result;
        }

        public static async Task<string> DeleteTask(string id, string token)
        {
            string url = string.Format("https://outlook.office365.com/api/v2.0/me/tasks('{0}')", id);
            var result = await HttpClientHelper.DeleteAsync(token, url);
            return result.ToString();
        }
    }
}