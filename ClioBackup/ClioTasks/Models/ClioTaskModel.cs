using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClioTasks.Models
{
    public class ClioTaskModel
    {
        public List<ClioTask> tasks { get; set; }
        public string records { get; set; }
    }

    public class ClioSingleTaskModel
    {
        public ClioTask task { get; set; }
    }

    public class ClioTask
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string priority { get; set; }
        public string due_at { get; set; }
        public string complete { get; set; }
        public string completed_at { get; set; }
        public Boolean is_private { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string status { get; set; }

        public ClioUser user { get; set; }
        public ClioUser assignee { get; set; }
        public ClioUser assigner { get; set; }
        public ClioUser matter { get; set; }

        public OutlookTaskModel OutlookTask { get; set; }
        public string SyncCheckResult { get; set; }
    }

    public class ClioUser
    {
        public string id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string email { get; set; }

        public string type { get; set; }
    }
}