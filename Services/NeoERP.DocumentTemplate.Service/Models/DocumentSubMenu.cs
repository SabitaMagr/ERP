using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoERP.DocumentTemplate.Service.Models
{
   public class DocumentSubMenu
    {
        public string Form_CODE { get; set; }
        public string VOUCHER_NO { get; set; }
        public DateTime? VOUCHER_DATE { get; set; }
        public decimal? VOUCHER_AMOUNT { get; set; }
        public string CREATED_BY { get; set; }
        public DateTime? CREATED_DATE { get; set; }
        public string CHECKED_BY { get; set; }
        public DateTime? CHECKED_DATE { get; set; }
        public string AUTHORISED_BY { get; set; }
        public DateTime? POSTED_DATE { get; set; }
        public DateTime? MODIFY_DATE { get; set; }
        public string SYN_ROWID { get; set; }
        public string REFERENCE_NO { get; set; }
        public string SESSION_ROWID { get; set; } 
        public string MODULE_CODE { get; set; }

      //  public String 


    }
}
