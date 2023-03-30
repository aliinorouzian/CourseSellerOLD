using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseSeller.Core.Convertors
{
    public static class DateConvertor
    {
        public static string ToShamsi(this DateTime val)
        {
            PersianCalendar pc = new PersianCalendar();

            return $"{pc.GetYear(val):00}/{pc.GetMonth(val):00}/{pc.GetDayOfMonth(val):00}";
        }
    }
}
