// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;


using ClioTasks.TokenStorage;
using System.Net;
using Newtonsoft.Json;
using ClioTasks.Models;
using System.Text;
using ClioTasks.Utility;
using Utility;

namespace ClioTasks.Controllers
{
    public class HomeController : Controller
    {

        //https://msdn.microsoft.com/en-us/office/office365/api/extended-properties-rest-operations?f=255&MSPPError=-2147217396#get-item-expanded-with-extended-property
        //Above link tells how to use extended property for tasks.

        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                string userName = ClaimsPrincipal.Current.FindFirst("name").Value;
                string userId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userId))
                {
                    // Invalid principal, sign out
                    return RedirectToAction("SignOut");
                }

                // Since we cache tokens in the session, if the server restarts
                // but the browser still has a cached cookie, we may be
                // authenticated but not have a valid token cache. Check for this
                // and force signout.
                SessionTokenCache tokenCache = new SessionTokenCache(userId, HttpContext);
                if (!tokenCache.HasData())
                {
                    // Cache is empty, sign out
                    return RedirectToAction("SignOut");
                }

                ViewBag.UserName = userName;
            }
            return View();
        }
              
        //Get all my tasks.
        public async Task<ActionResult> Tasks(string lastSyncDate)
        {
            string token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return Redirect("/");
            }
            var tasks = await OutlookTaskHelper.GetAllTasksAsync(token);
            if (!string.IsNullOrEmpty(lastSyncDate))
            {
                DateTime lastSyncDateUTC = Convert.ToDateTime(lastSyncDate).ToUniversalTime();
               // tasks = tasks.Where(c => DateTime.Compare(c.LastModifiedDateTime, lastSyncDateUTC) >= 0).ToList();
                TempData["lastSyncDate"] = lastSyncDate;
                foreach (var item in tasks)
                {
                    item.SyncCheckResult = "";

                    if (item.SingleValueExtendedProperties == null )
                    {
                        item.SyncCheckResult = "This task should sync to Clio.<br/>";
                    }
                    if (item.SingleValueExtendedProperties != null && item.ClioTask == null)
                    {
                        item.SyncCheckResult += "<br/>Delete this task in Exchange as it was deleted in Clio.<br/>";
                    }

                    //Compare subject, due date, status and priority
                    if (item.ClioTask != null)
                    {
                        if (item.Subject.ToLower() != item.ClioTask.name.ToLower())
                        {
                            item.SyncCheckResult += "Title doesn't match.<br/>";
                        }
                        if (StringsHelper.DateMatchCheck(item.StartDateTime.DateTime, item.ClioTask.created_at) == false)
                        {
                            item.SyncCheckResult += "Start date doesn't match.<br/>";
                        }
                        if (StringsHelper.DateMatchCheck(item.DueDateTime.DateTime, Convert.ToDateTime(item.ClioTask.due_at)) == false)
                        {
                            item.SyncCheckResult += "Due date doesn't match.<br/>";
                        }
                        if (item.Importance.ToLower() != item.ClioTask.priority.ToLower())
                        {
                            item.SyncCheckResult += "Priority date doesn't match.<br/>";
                        }
                    }
                }
            }
          
            return View(tasks);
        }





        //Edit task.
        public async Task<ActionResult> EditTask(string id)
        {
            string token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return Redirect("/");
            }
            OutlookTaskModel result = await OutlookTaskHelper.GetOutlookTaskByIdAsync(id, token);
            return View(result);
        }



        //In this example I only update subject and due date.
        [HttpPost]
        public async Task<ActionResult> EditTask(OutlookTaskModel model)
        {
            string token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return Redirect("/");
            }
            string url = string.Format("https://outlook.office.com/api/v2.0/me/tasks/{0}", model.Id);
            string json = string.Format("{{\"DueDateTime\":  {{\"DateTime\": \"{0}\",\"TimeZone\": \"Eastern Standard Time\"}},\"Subject\":\"{1}\", \"SingleValueExtendedProperties\": [{{\"PropertyId\":\"String {{{2}}} Name {3}\",\"Value\":\"{4}\"}}]}}", 
                model.DueDateTime.DateTime, model.Subject,  ConfigurationManager.AppSettings["outlook_customfield_guid"], 
                ConfigurationManager.AppSettings["outlook_customfield_name"], model.SingleValueExtendedProperties[0].Value);
            await HttpClientHelper.PatchAsync(token, url, json);
            return RedirectToAction("Tasks");
        }

        public async Task<ActionResult> Compare(string outlookId, string clioId)
        {
            string token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return Redirect("/");
            }
            OutlookTaskModel outlookTask = await OutlookTaskHelper.GetOutlookTaskByIdAsync(outlookId, token);
            ClioSingleTaskModel clioTask = await ClioTaskHelper.GetClioTaskByIdAsync(clioId);
            CompoundModel model = new CompoundModel();
            model.OutlookTaskModel = outlookTask;
            model.ClioTask = clioTask.task;
            return View(model);
        }

 

        public async Task<ActionResult> DeleteTask(string id)
        {
            string token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return Redirect("/");
            }
            await OutlookTaskHelper.DeleteTask(id, token);
            return RedirectToAction("Tasks");
        }


        public async Task<string> GetAccessToken()
        {
            return await TokenHelper.GetAccessToken(HttpContext);           
        }


       

      



        public ActionResult Error(string message, string debug)
        {
            ViewBag.Message = message;
            ViewBag.Debug = debug;
            return View("Error");
        }

        public void SignIn()
        {
            if (!Request.IsAuthenticated)
            {
                // Signal OWIN to send an authorization request to Azure
                HttpContext.GetOwinContext().Authentication.Challenge(
                  new AuthenticationProperties { RedirectUri = "/" },
                  OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        public void SignOut()
        {
            if (Request.IsAuthenticated)
            {
                string userId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    // Get the user's token cache and clear it
                    SessionTokenCache tokenCache = new SessionTokenCache(userId, HttpContext);
                    tokenCache.Clear();
                }
            }
            // Send an OpenID Connect sign-out request. 
            HttpContext.GetOwinContext().Authentication.SignOut(
              CookieAuthenticationDefaults.AuthenticationType);
            Response.Redirect("/");
        }
        //public async Task<ActionResult> Contacts()
        //{
        //    string token = await GetAccessToken();
        //    if (string.IsNullOrEmpty(token))
        //    {
        //        // If there's no token in the session, redirect to Home
        //        return Redirect("/");
        //    }

        //    string userEmail = await GetUserEmail();

        //    GraphServiceClient client = new GraphServiceClient(
        //        new DelegateAuthenticationProvider(
        //            (requestMessage) =>
        //            {
        //                requestMessage.Headers.Authorization =
        //                    new AuthenticationHeaderValue("Bearer", token);

        //                requestMessage.Headers.Add("X-AnchorMailbox", userEmail);

        //                return Task.FromResult(0);
        //            }));

        //    try
        //    {
        //        var contactResults = await client.Me.Contacts.Request()
        //                            .OrderBy("displayName")
        //                            .Select("displayName,emailAddresses,mobilePhone")
        //                            .Top(10)
        //                            .GetAsync();

        //        return View(contactResults.CurrentPage);
        //    }
        //    catch (ServiceException ex)
        //    {
        //        return RedirectToAction("Error", "Home", new { message = "ERROR retrieving contacts", debug = ex.Message });
        //    }
        //}

        //public ActionResult About()
        //{
        //    ViewBag.Message = "Your application description page.";

        //    return View();
        //}

        //public ActionResult Contact()
        //{
        //    ViewBag.Message = "Your contact page.";

        //    return View();
        //}
        #region get user email. To use below code need to add scope on web.config.
        //public async Task<string> GetUserEmail()
        //{
        //    GraphServiceClient client = new GraphServiceClient(
        //        new DelegateAuthenticationProvider(
        //            async (requestMessage) =>
        //            {
        //                string accessToken = await GetAccessToken();
        //                requestMessage.Headers.Authorization =
        //                    new AuthenticationHeaderValue("Bearer", accessToken);
        //            }));

        //    // Get the user's email address
        //    try
        //    {

        //        Microsoft.Graph.User user = await client.Me.Request().GetAsync();
        //        return user.Mail;
        //    }
        //    catch (ServiceException ex)
        //    {
        //        return string.Format("#ERROR#: Could not get user's email address. {0}", ex.Message);
        //    }
        //}

        //public async Task<ActionResult> Inbox()
        //{
        //    string token = await GetAccessToken();
        //    if (string.IsNullOrEmpty(token))
        //    {
        //        // If there's no token in the session, redirect to Home
        //        return Redirect("/");
        //    }

        //    string userEmail = await GetUserEmail();

        //    GraphServiceClient client = new GraphServiceClient(
        //        new DelegateAuthenticationProvider(
        //            (requestMessage) =>
        //            {
        //                requestMessage.Headers.Authorization =
        //                    new AuthenticationHeaderValue("Bearer", token);

        //                requestMessage.Headers.Add("X-AnchorMailbox", userEmail);

        //                return Task.FromResult(0);
        //            }));

        //    try
        //    {


        //            var mailResults = await client.Me.MailFolders.Inbox.Messages.Request()
        //                            .OrderBy("receivedDateTime DESC")
        //                            .Select("subject,receivedDateTime,from")
        //                            .Top(10)
        //                            .GetAsync();

        //        return View(mailResults.CurrentPage);
        //    }
        //    catch (ServiceException ex)
        //    {
        //        return RedirectToAction("Error", "Home", new { message = "ERROR retrieving messages", debug = ex.Message });
        //    }
        //}

        //public async Task<ActionResult> Calendar()
        //{
        //    string token = await GetAccessToken();
        //    if (string.IsNullOrEmpty(token))
        //    {
        //        // If there's no token in the session, redirect to Home
        //        return Redirect("/");
        //    }

        //    string userEmail = await GetUserEmail();

        //    GraphServiceClient client = new GraphServiceClient(
        //        new DelegateAuthenticationProvider(
        //            (requestMessage) =>
        //            {
        //                requestMessage.Headers.Authorization =
        //                    new AuthenticationHeaderValue("Bearer", token);

        //                requestMessage.Headers.Add("X-AnchorMailbox", userEmail);

        //                return Task.FromResult(0);
        //            }));

        //    try
        //    {
        //        var eventResults = await client.Me.Events.Request()
        //                            .OrderBy("start/dateTime DESC")
        //                            .Select("subject,start,end")
        //                            .Top(10)
        //                            .GetAsync();

        //        return View(eventResults.CurrentPage);
        //    }
        //    catch (ServiceException ex)
        //    {
        //        return RedirectToAction("Error", "Home", new { message = "ERROR retrieving events", debug = ex.Message });
        //    }
        //}
        #endregion
    }
}