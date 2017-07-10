using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClioTasks.Models
{
    public class OutlookTasks
    {
        [JsonProperty("value")]
        public List<OutlookTaskModel> Tasks { get; set; }
    }


    public class OutlookTaskModel
    {
        [JsonProperty("Id")]
        public string Id { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastModifiedDateTime { get; set; }

        public string Subject { get; set; }
        public string BodyPreview { get; set; }
        public Body Body { get; set; }
        public string Owner { get; set; }
        public string Status { get; set; }

        public OutlookTaskDateTime DueDateTime { get; set; }
        public OutlookTaskDateTime StartDateTime { get; set; }
        public List<SingleValueExtendedProperties> SingleValueExtendedProperties { get; set; }

        public ClioTask ClioTask { get; set; }

        public string Importance { get; set; }

        public string SyncCheckResult { get; set; }
    }

    public class Body
    {
        public string ContentType { get; set; }
        public string Content { get; set; }
    }

    public class OutlookTaskDateTime
    {
        public DateTime DateTime { get; set; }
        public string TimeZone { get; set; }
    }

    public class SingleValueExtendedProperties
    {
        public string PropertyId { get; set; }
        public string Value { get; set; }
    }
}