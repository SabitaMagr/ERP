using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoERP.QuotationManagement.Service.Models
{
    public class Employee
    {
        public string ID { get; set; }
        public string Employee_Name { get; set; }
        public string EMPLOYEE_CODE { get; set; }
        public string EMPLOYEE_EDESC { get; set; }
        public string EMAIL { get; set; }
        public string EPERMANENT_ADDRESS1 { get; set; }
        public string ETEMPORARY_ADDRESS1 { get; set; }
        public string MOBILE { get; set; }
        public string CITIZENSHIP_NO { get; set; }
        public string Type { get; set; }
    }
}
