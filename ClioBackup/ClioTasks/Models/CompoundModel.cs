using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClioTasks.Models
{
    public class CompoundModel
    {
        public ClioTask ClioTask { get; set; }
        public OutlookTaskModel OutlookTaskModel { get; set; }
    }
}