using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoERP.QuotationManagement.Service.Models
{
    public class Quotation_Details
    {
        public int QUOTATION_NO { get; set; }
        public string TENDER_NO { get; set; }
        public string PAN_NO { get; set; }
        public string PARTY_NAME { get; set; }
        public string ADDRESS { get; set; }
        public string CONTACT_NO { get; set; }
        public DateTime? ISSUE_DATE { get; set; }
        public DateTime? VALID_DATE { get; set; }
        public string NEPALI_DATE { get; set; }
        public string EMAIL { get; set; }
        public string CURRENCY { get; set; }
        public decimal CURRENCY_RATE { get; set; }
        public DateTime? DELIVERY_DATE { get; set; }
        public decimal TOTAL_AMOUNT { get; set; }
        public decimal TOTAL_DISCOUNT { get; set; }
        public decimal TOTAL_EXCISE { get; set; }
        public decimal TOTAL_TAXABLE_AMOUNT { get; set; }
        public decimal TOTAL_VAT { get; set; }
        public decimal TOTAL_NET_AMOUNT { get; set; }
        public string DISCOUNT_TYPE { get; set; }
        public string STATUS { get; set; }
        public List<Item_details> Item_Detail { get; set; }
        public List<Term_Conditions> TermsCondition { get; set; }
        public List<QuotationTransaction> IMAGES_LIST { get; set; }


    }
    public class Term_Conditions
    {
        public string TERM_CONDITION { get; set; }
    }
    public class Item_details
    {
        public int ID { get; set; }
        public int QUOTATION_NO { get; set; }
        public string ITEM_CODE { get; set; }
        public decimal RATE { get; set; }
        public decimal AMOUNT { get; set; }
        public decimal DISCOUNT { get; set; }
        public decimal DISCOUNT_AMOUNT { get; set; }
        public decimal EXCISE { get; set; }
        public decimal TAXABLE_AMOUNT { get; set; }
        public decimal VAT_AMOUNT { get; set; }
        public decimal NET_AMOUNT { get; set; }
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
    }
}
