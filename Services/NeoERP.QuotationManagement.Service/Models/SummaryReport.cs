using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoERP.QuotationManagement.Service.Models
{
   public class SummaryReport
    {
        public string TENDER_NO { get; set; }
        public DateTime? CREATED_DATE { get; set; }
        public DateTime? VALID_DATE { get; set; }
        public string ITEM_DESC { get; set; }
        public string STATUS { get; set; }
    }
}
