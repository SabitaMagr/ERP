using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace NeoErp.Core.Quotation
{
    public class Supplier
    {
        public string SUPPLIER_CODE { get; set; }
        public string SUPPLIER_EDESC { get; set; }
        public string EMAIL { get; set; }
        public string ADDRESS { get; set; }
        public string CONTACT_NO { get; set; }
        public string MASTER_SUPPLIER_CODE { get; set; }
    }
}