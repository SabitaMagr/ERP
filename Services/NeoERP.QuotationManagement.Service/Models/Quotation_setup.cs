using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NeoERP.QuotationManagement.Service.Models
{
    public class Quotation_setup
    {
        public int ID { get; set; }
        public string TENDER_NO { get; set; }
        public DateTime? ISSUE_DATE { get; set; }
        public DateTime? VALID_DATE { get; set; }
        public DateTime? CREATED_DATE { get; set; }
        public string CREATED_BY { get; set; }
        public string NEPALI_DATE { get; set; }
        public string DELIVERY_DT_BS { get; set; }

        public string COMPANY_CODE { get; set; }
        public string REMARKS { get; set; }
        public char STATUS { get; set; }
        public string BRANCH_CODE { get; set; }
        public string STATUS_DETAILS { get; set; }
        public string APPROVED_STATUS { get; set; }
        public List<Item> Items { get; set; }

    }

    public class Item
    {
        public int ID { get; set; }
        public string ITEM_CODE { get; set; }
        public string SPECIFICATION { get; set; }
        public string IMAGE { get; set; } 
        public string IMAGE_NAME { get; set; }
        public string UNIT { get; set; }
        public int QUANTITY { get; set; }
        public string CATEGORY { get; set; }
        public string BRAND_NAME { get; set; }
        public string INTERFACE { get; set; }
        public string TYPE { get; set; }
        public string LAMINATION { get; set; }
        public string ITEM_SIZE { get; set; }
        public string THICKNESS { get; set; }
        public string COLOR { get; set; }
        public string GRADE { get; set; }
        public int SIZE_LENGTH { get; set; }
        public int SIZE_WIDTH { get; set; }
        public float PRICE { get; set; }
    }
}
