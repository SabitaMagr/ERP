﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoERP.DocumentTemplate.Service.Models
{
    public class SalesChalanDetail
    {
        public decimal? SECOND_QUANTITY { get; set; }
        public decimal? THIRD_QUANTITY { get; set; }
        public string CHALAN_NO { get; set; }
        public string CHALAN_DATE { get; set; }
        public DateTime VOUCHER_DATE { get; set; }
        public string ORDER_DATE { get; set; }
        public string MANUAL_NO { get; set; }
        public string CUSTOMER_CODE { get; set; }
        public string FROM_LOCATION_CODE { get; set; }
        public int SERIAL_NO { get; set; }
        public string ITEM_CODE { get; set; }
        public string MU_CODE { get; set; }
        public decimal QUANTITY { get; set; }
        public decimal UNIT_PRICE { get; set; }
        public decimal TOTAL_PRICE { get; set; }
        public decimal CALC_QUANTITY { get; set; }
        public decimal CALC_UNIT_PRICE { get; set; }
        public decimal CALC_TOTAL_PRICE { get; set; }
       
        public string REMARKS { get; set; }
        public string FORM_CODE { get; set; }

        public string COMPANY_CODE { get; set; }
        public string BRANCH_CODE { get; set; }
        public string CREATED_BY { get; set; }
        public DateTime CREATED_DATE { get; set; } = DateTime.Now;
        public string DELETED_FLAG { get; set; }

        public string DELIVERY_DATE { get; set; }

        public string CURRENCY_CODE { get; set; }
        public decimal EXCHANGE_RATE { get; set; }
        public string TRACKING_NO { get; set; }
        public string STOCK_BLOCK_FLAG { get; set; }
        public string SESSION_ROWID { get; set; }
        public DateTime? MODIFY_DATE { get; set; }
        public string MODIFY_BY { get; set; }

        public string FREE_QTY { get; set; }
        public string SALES_TYPE_CODE { get; set; }

        public string SYN_ROWID { get; set; }
        public string BATCH_NO { get; set; }
        public string PARTY_TYPE_CODE { get; set; }

        public string CANCEL_FLAG { get; set; }
        public string CANCEL_BY { get; set; }

        public string CANCEL_REMARKS { get; set; }
        public string PRIORITY_CODE { get; set; }
        public string OPENING_DATA_FLAG { get; set; }
        public string SHIPPING_ADDRESS { get; set; }
        public string SHIPPING_CONTACT_NO { get; set; }
        public string EMPLOYEE_CODE { get; set; }
        public string MISC_CODE { get; set; }
        public string AGENT_CODE { get; set; }
        public string DIVISION_CODE { get; set; }
        public int? PRINT_COUNT { get; set; }
        public string AREA_CODE { get; set; }
        public decimal? LINE_ITEM_DISCOUNT { get; set; }
        public string SECTOR_CODE { get; set; }
        public string ALT1_MU_CODE { get; set; }
        public string ALT1_QUANTITY { get; set; }
        //public string FROM_LOCATION_CODE { get; set; }
    }
}
