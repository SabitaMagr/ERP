using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoERP.QuotationManagement.Service.Models
{
    public class Tender
    {
        public int ID { get; set; }
        public string PREFIX { get; set; }
        public string SUFFIX { get; set; }
        public int BODY_LENGTH { get; set; }
        public string STATUS { get; set; }
        public DateTime? CREATED_DATE { get; set; }
        public string CREATED_BY {get;set;}
        public string COMPANY_CODE { get; set; }

    }
}
