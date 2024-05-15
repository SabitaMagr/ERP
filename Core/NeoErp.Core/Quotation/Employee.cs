using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace NeoErp.Core.Quotation
{
    public class Employee
    {
        public string EMPLOYEE_CODE { get; set; }
        public string EMPLOYEE_EDESC { get; set; }
        public string EMAIL { get; set; }
        public string ADDRESS { get; set; }
        public string CONTACT_NO { get; set; }
    }
}