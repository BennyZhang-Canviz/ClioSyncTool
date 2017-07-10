using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Utility
{
    public static class StringsHelper
    {
       public static string GetColor(string value1, string value2)
        {
            if (value1 != value2)
            {
                return "warning";
            }
            return "";
        }
        public static bool DateMatchCheck(DateTime date1, DateTime date2)
        {
            if (date1.Year != date2.Year &&
                        date1.Month != date2.Month &&
                        date1.Day != date2.Day &&
                        date1.Hour != date2.Hour &&
                        date1.Minute != date2.Minute)
                return false;
            return true;
        }
    }
}