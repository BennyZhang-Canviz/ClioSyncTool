using ClioTasks.Models;
using ClioTasks.Utility;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Utility;

namespace ClioTasks.Controllers
{
    public class ClioTasksController : Controller
    {

        private string clio_access_token = ConfigurationManager.AppSettings["clio_token"];
        // GET: ClioTasks
        public async Task<ActionResult> Index(string lastSyncDate)
        {
            string outlookAccessToken =await GetOutlookAccessToken();
            ClioTaskModel model = await ClioTaskHelper.GetAllCloiTasksWithOutlookTask(outlookAccessToken);
            if (!string.IsNullOrEmpty(lastSyncDate))
            {
                DateTime lastSyncDateUTC = Convert.ToDateTime(lastSyncDate).ToUniversalTime();
                //model.tasks = model.tasks.Where(c => DateTime.Compare(c.updated_at, lastSyncDateUTC) >= 0).ToList();
                TempData["lastSyncDate"] = lastSyncDate;
                foreach (var item in model.tasks)
                {
                    item.SyncCheckResult = "";
                    int dateCompare = DateTime.Compare(Convert.ToDateTime( item.created_at).ToUniversalTime(), lastSyncDateUTC);
                    if (dateCompare>0 && item.OutlookTask == null)
                    {
                        item.SyncCheckResult = "This task should sync to Outlook.<br/>";
                    }
                    if (dateCompare<0 && item.OutlookTask ==null)
                    {
                        item.SyncCheckResult = "This task should be deleted as it doesn't exist in Exchange and were created BEFORE the last synch.<br/>";
                    }

                    //Compare subject, due date, status and priority
                    if (item.OutlookTask != null)
                    {
                        if (item.name.ToLower() != item.OutlookTask.Subject.ToLower())
                        {
                            item.SyncCheckResult += "Title doesn't match.<br/>";
                        }
                        if (StringsHelper.DateMatchCheck(item.OutlookTask.StartDateTime.DateTime, item.created_at) == false)
                        {
                            item.SyncCheckResult += "Start date doesn't match.<br/>";
                        }
                        if (StringsHelper.DateMatchCheck(item.OutlookTask.DueDateTime.DateTime, Convert.ToDateTime(item.due_at)) == false)
                        {
                            item.SyncCheckResult += "Due date doesn't match.<br/>";
                        }
                        if (item.priority.ToLower() != item.OutlookTask.Importance.ToLower())
                        {
                            item.SyncCheckResult += "Priority date doesn't match.<br/>";
                        }
                    }
                }
            }

            return View(model.tasks);
        }


        public async Task<ActionResult> Detail(string id)
        {
            ClioSingleTaskModel clioTask = await ClioTaskHelper.GetClioTaskByIdAsync(id);
            return View(clioTask.task);
        }

        public async Task<ActionResult> DeleteTask(string id)
        {
            await ClioTaskHelper.DeleteClioTaskByIdAsync(id);
            return RedirectToAction("Index");
        }

        private async Task<string> GetOutlookAccessToken()
        {
            return await TokenHelper.GetAccessToken(HttpContext);
        }
    }
}