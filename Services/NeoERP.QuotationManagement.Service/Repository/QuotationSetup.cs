using NeoErp.Core;
using NeoErp.Core.Caching;
using NeoErp.Core.Domain;
using NeoErp.Core.Models;
using NeoErp.Core.Models.Log4NetLoggin;
using NeoErp.Data;
using System;
using NeoErp.Core.Services.CommonSetting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using NeoErp.Core.Models.CustomModels;
using System.Text;
using NeoERP.QuotationManagement.Service.Interface;
using NeoERP.QuotationManagement.Service.Models;
using System.Web;
using System.IO;
using System.Net.Http;
using System.Net;

namespace NeoERP.QuotationManagment.Service.Repository
{
    public class QuotationSetup : IQuotationRepo
    {
        IWorkContext _workContext;
        IDbContext _dbContext;

        public QuotationSetup(IWorkContext workContext, IDbContext dbContext)
        {
            this._workContext = workContext;
            this._dbContext = dbContext;
        }
        public List<Company> GetCompany()
        {
            var company_code = _workContext.CurrentUserinformation.company_code;
            string query = $@"select COMPANY_CODE,COMPANY_EDESC,ADDRESS,EMAIL,LOGO_FILE_NAME from COMPANY_SETUP WHERE COMPANY_CODE='{company_code}'";
            List<Company> company = _dbContext.SqlQuery<Company>(query).ToList();
            return company;
        }
        public List<Products> GetAllProducts()
        {
            try
            {
                List<Products> ProductsList = new List<Products>();

                string query = $@"select 
                    COALESCE(iims.item_code,' ') as ItemCode
                    ,COALESCE(iims.item_edesc,' ') as ItemDescription
                    ,COALESCE(iims.index_mu_code,' ') as ItemUnit
                    ,COALESCE(iiss.ITEM_SPECIFICATION,' ') as SPECIFICATION
                    ,COALESCE(iiss.BRAND_NAME,' ') as BRAND_NAME
                    ,COALESCE(iiss.INTERFACE,' ') as INTERFACE
                    ,COALESCE(iiss.TYPE,' ') as TYPE
                     ,COALESCE(iiss.LAMINATION,' ') as LAMINATION
                    ,COALESCE(iiss.ITEM_SIZE,' ') as ITEM_SIZE
                    ,COALESCE(iiss.THICKNESS,' ') as THICKNESS
                    ,COALESCE(iiss.COLOR,' ') as COLOR
                    ,COALESCE(iiss.GRADE,' ') as GRADE
                     ,COALESCE(iiss.SIZE_LENGHT,0) as SIZE_LENGHT
                    ,COALESCE(iiss.SIZE_WIDTH,0) as SIZE_WIDTH
                    from ip_item_master_setup iims,IP_ITEM_SPEC_SETUP iiss
                    where iims.item_code=iiss.item_code and iims.company_code=iiss.company_code
                     and iims.deleted_flag='N'
                    AND iims.company_code='{_workContext.CurrentUserinformation.company_code}' and iims.branch_code='{_workContext.CurrentUserinformation.branch_code}'
                     order by iims.item_code desc";
                ProductsList = this._dbContext.SqlQuery<Products>(query).ToList();
                return ProductsList;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<Quotation_setup> GetQuotationId()
        {
            //string query = $@"select FN_NEW_TENDERNO('{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}') as TENDER_NO from dual";
            //string query = $@"select GENERATE_TENDER_NO('{_workContext.CurrentUserinformation.company_code}') as TENDER_NO from dual";
            //List<Quotation_setup> id = _dbContext.SqlQuery<Quotation_setup>(query).ToList();
            string fQuery = $@"SELECT fs.form_code,fds.table_name as TableName FROM form_setup fs,form_detail_setup fds
                where ( fds.form_code = fs.form_code ) and ( fds.company_code = fs.company_code )
                 AND fs.quotation_flag = 'Y'and fs.quotation_flag is not null and fs.company_code='{_workContext.CurrentUserinformation.company_code}' and rownum=1";
            Inventory data = _dbContext.SqlQuery<Inventory>(fQuery).FirstOrDefault();
            var companycode = _workContext.CurrentUserinformation.company_code;
            //string tablename = "IP_QUOTATION_INQUIRY";
            string transactiondate = DateTime.Now.ToString("dd-MMM-yyyy");
            string query = string.Format(@"select FN_NEW_VOUCHER_NO('{0}','{1}','{2}','{3}') as TENDER_NO FROM DUAL", companycode, data.FORM_CODE, transactiondate, data.TableName);
            List<Quotation_setup> id = _dbContext.SqlQuery<Quotation_setup>(query).ToList();
            return id;
        }
        //public bool InsertQuotationData(FormDetails data)
        //{
        //    try
        //    {
        //        //int tenderId = data.ID;
        //        //if (tenderId == 0)
        //        //{
        //        //    var idquery = $@"SELECT COALESCE(MAX(ID) + 1, 1) AS id FROM sa_quotation_setup";
        //        //    int id = _dbContext.SqlQuery<int>(idquery).FirstOrDefault();
        //        //    string insertQuery = string.Format(@"INSERT INTO sa_quotation_setup(TENDER_NO, VALID_DATE, ISSUE_DATE, CREATED_DATE, CREATED_BY, COMPANY_CODE, STATUS,REMARKS,ID,APPROVED_STATUS,BRANCH_CODE) 
        //        //                 VALUES('{0}', TO_DATE('{1}', 'DD-MON-YYYY'), TO_DATE('{2}', 'DD-MON-YYYY'), TO_DATE('{3}', 'DD-MON-YYYY'), '{4}', '{5}', '{6}','{7}',{8},'{9}','{10}')",
        //        //                              data.TENDER_NO,
        //        //                              data.VALID_DATE.HasValue ? $"{data.VALID_DATE.Value.ToString("dd-MMM-yyyy")}" : null,
        //        //                              data.ISSUE_DATE.HasValue ? $"{data.ISSUE_DATE.Value.ToString("dd-MMM-yyyy")}" : null,
        //        //                              DateTime.Now.ToString("dd-MMM-yyyy"),
        //        //                              _workContext.CurrentUserinformation.login_code,
        //        //                              _workContext.CurrentUserinformation.company_code,
        //        //                              "E", data.REMARKS, id, "N", _workContext.CurrentUserinformation.branch_code);
        //        //    _dbContext.ExecuteSqlCommand(insertQuery);
        //        //    List<Item> itemData = data.Items;
        //        //    if (itemData != null)
        //        //    {
        //        //        foreach (var item in itemData)
        //        //        {

        //        //            InsertItemData(item, data.TENDER_NO); // Pass each item individually to InsertItemData
        //        //        }
        //        //    }
        //        //}
        //        //else
        //        //{
        //        //    string updateQuery = $@"UPDATE sa_quotation_setup 
        //        //       SET VALID_DATE = {(data.VALID_DATE.HasValue ? $"'{data.VALID_DATE.Value.ToString("dd-MMM-yyyy")}'" : "null")},
        //        //           ISSUE_DATE = {(data.ISSUE_DATE.HasValue ? $"'{data.ISSUE_DATE.Value.ToString("dd-MMM-yyyy")}'" : "null")},
        //        //           MODIFIED_DATE = '{DateTime.Now.ToString("dd-MMM-yyyy")}',REMARKS='{data.REMARKS}',
        //        //           MODIFIED_BY = '{_workContext.CurrentUserinformation.login_code}',
        //        //           COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'
        //        //       WHERE id = '{data.ID}' 
        //        //       AND tender_no = '{data.TENDER_NO}'";
        //        //    _dbContext.ExecuteSqlCommand(updateQuery);
        //        //    List<Item> itemData = data.Items;
        //        //    if (itemData != null)
        //        //    {
        //        //        foreach (var item in itemData)
        //        //        {
        //        //            var query = $@"SELECT * FROM sa_quotation_Items WHERE tender_no='{data.TENDER_NO}' AND id='{item.ID}'";
        //        //            List<Item> itemDetails = _dbContext.SqlQuery<Item>(query).ToList();

        //        //            if (itemDetails.Any())
        //        //            {
        //        //                    UpdateItemData(item, data.TENDER_NO);
        //        //            }
        //        //            else
        //        //            {
        //        //                InsertItemData(item, data.TENDER_NO);
        //        //            }

        //        //        }
        //        //    }
        //        //}
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}
        public bool InsertQuotationData(List<Inventory> childColumnValue, Inventory masterColumnValue, CommonFieldsForInventory commonValue, FormDetails model)
        {
            try
            {
                int serialno = 1;
                bool insertedToChild = false;
                Inventory inventoryChildDetails = new Inventory();
                foreach (var childCol in childColumnValue)
                {
                    inventoryChildDetails.VOUCHER_NO = (commonValue.VoucherNumber == "undefined") ? commonValue.NewVoucherNumber : commonValue.VoucherNumber;
                    inventoryChildDetails.MANUAL_NO = string.IsNullOrEmpty(childCol.MANUAL_NO) ? masterColumnValue.MANUAL_NO : childCol.MANUAL_NO;
                    inventoryChildDetails.SERIAL_NO = string.IsNullOrEmpty(childCol.SERIAL_NO) ? serialno.ToString() : childCol.SERIAL_NO;
                    inventoryChildDetails.TO_LOCATION_CODE = string.IsNullOrEmpty(childCol.TO_LOCATION_CODE) ? masterColumnValue.TO_LOCATION_CODE : childCol.TO_LOCATION_CODE;
                    inventoryChildDetails.REMARKS = string.IsNullOrEmpty(childCol.REMARKS) ? masterColumnValue.REMARKS : childCol.REMARKS;
                    inventoryChildDetails.CALC_TOTAL_PRICE = childCol.CALC_TOTAL_PRICE;
                    inventoryChildDetails.CALC_UNIT_PRICE = childCol.CALC_UNIT_PRICE;
                    inventoryChildDetails.CALC_QUANTITY = childCol.CALC_QUANTITY;
                    inventoryChildDetails.TOTAL_PRICE = childCol.TOTAL_PRICE;
                    inventoryChildDetails.UNIT_PRICE = childCol.UNIT_PRICE;
                    inventoryChildDetails.QUANTITY = childCol.QUANTITY;
                    inventoryChildDetails.MU_CODE = string.IsNullOrEmpty(childCol.MU_CODE) ? masterColumnValue.MU_CODE : childCol.MU_CODE;
                    inventoryChildDetails.ITEM_CODE = string.IsNullOrEmpty(childCol.ITEM_CODE) ? masterColumnValue.ITEM_CODE : childCol.ITEM_CODE;

                    if (string.IsNullOrEmpty(masterColumnValue.MODIFY_DATE))
                    {
                        inventoryChildDetails.MODIFY_DATE = "''";
                    }
                    else
                    {
                        inventoryChildDetails.MODIFY_DATE = masterColumnValue.MODIFY_DATE;
                    }
                    if (string.IsNullOrEmpty(masterColumnValue.MODIFY_BY))
                    {
                        inventoryChildDetails.MODIFY_BY = "";
                    }
                    else
                    {
                        inventoryChildDetails.MODIFY_BY = masterColumnValue.MODIFY_BY;
                    }
                    inventoryChildDetails.DELETED_FLAG = "N";
                    inventoryChildDetails.CREATED_DATE = commonValue.VoucherDate;
                    inventoryChildDetails.CREATED_BY = string.IsNullOrEmpty(childCol.CREATED_BY) ? masterColumnValue.CREATED_BY : _workContext.CurrentUserinformation.login_code.ToUpper();
                    inventoryChildDetails.BRANCH_CODE = string.IsNullOrEmpty(childCol.BRANCH_CODE) ? _workContext.CurrentUserinformation.branch_code : childCol.BRANCH_CODE;
                    inventoryChildDetails.MRR_NO = string.IsNullOrEmpty(childCol.MRR_NO) ? masterColumnValue.MRR_NO : childCol.MRR_NO;
                    inventoryChildDetails.COMPLETED_QUANTITY = childCol.COMPLETED_QUANTITY;
                    inventoryChildDetails.FORM_CODE = string.IsNullOrEmpty(commonValue.FormCode) ? masterColumnValue.FORM_CODE : commonValue.FormCode;
                    inventoryChildDetails.COMPANY_CODE = string.IsNullOrEmpty(childCol.COMPANY_CODE) ? _workContext.CurrentUserinformation.company_code : childCol.COMPANY_CODE;

                    inventoryChildDetails.IMAGE =(childCol.IMAGE);
                    inventoryChildDetails.CATEGORY = string.IsNullOrEmpty(childCol.CATEGORY) ? masterColumnValue.CATEGORY : childCol.CATEGORY;
                    inventoryChildDetails.BRAND_NAME = string.IsNullOrEmpty(childCol.BRAND_NAME) ? masterColumnValue.BRAND_NAME : childCol.BRAND_NAME;
                    inventoryChildDetails.INTERFACE = string.IsNullOrEmpty(childCol.INTERFACE) ? masterColumnValue.INTERFACE : childCol.INTERFACE;
                    inventoryChildDetails.TYPE = string.IsNullOrEmpty(childCol.TYPE) ? masterColumnValue.TYPE : childCol.TYPE;
                    inventoryChildDetails.LAMINATION = string.IsNullOrEmpty(childCol.LAMINATION) ? masterColumnValue.LAMINATION : childCol.LAMINATION;
                    inventoryChildDetails.ITEM_SIZE = string.IsNullOrEmpty(childCol.ITEM_SIZE) ? masterColumnValue.ITEM_SIZE : childCol.ITEM_SIZE;
                    inventoryChildDetails.THICKNESS = string.IsNullOrEmpty(childCol.THICKNESS) ? masterColumnValue.THICKNESS : childCol.THICKNESS;
                    inventoryChildDetails.COLOR = string.IsNullOrEmpty(childCol.COLOR) ? masterColumnValue.COLOR : childCol.COLOR;
                    inventoryChildDetails.GRADE = string.IsNullOrEmpty(childCol.GRADE) ? masterColumnValue.GRADE : childCol.GRADE;
                    inventoryChildDetails.SIZE_LENGTH =childCol.SIZE_LENGTH;
                    inventoryChildDetails.SIZE_WIDTH =childCol.SIZE_WIDTH;

                    inventoryChildDetails.SYN_ROWID = string.IsNullOrEmpty(childCol.SYN_ROWID) ? masterColumnValue.SYN_ROWID : childCol.SYN_ROWID;
                    inventoryChildDetails.SESSION_ROWID = string.IsNullOrEmpty(childCol.SESSION_ROWID) ? masterColumnValue.SESSION_ROWID : childCol.SESSION_ROWID;
                    inventoryChildDetails.DIVISION_CODE = string.IsNullOrEmpty(childCol.DIVISION_CODE) ? masterColumnValue.DIVISION_CODE : childCol.DIVISION_CODE;
                    inventoryChildDetails.INVOICE_NO = string.IsNullOrEmpty(childCol.INVOICE_NO) ? masterColumnValue.INVOICE_NO : childCol.INVOICE_NO;
                    inventoryChildDetails.INVOICE_DATE = string.IsNullOrEmpty(childCol.INVOICE_DATE) ? masterColumnValue.INVOICE_DATE : childCol.INVOICE_DATE;
                    inventoryChildDetails.SUPPLIER_CODE = string.IsNullOrEmpty(childCol.SUPPLIER_CODE) ? masterColumnValue.SUPPLIER_CODE : childCol.SUPPLIER_CODE;
                    inventoryChildDetails.SUPPLIER_INV_NO = string.IsNullOrEmpty(childCol.SUPPLIER_INV_NO) ? masterColumnValue.SUPPLIER_INV_NO : childCol.SUPPLIER_INV_NO;
                    inventoryChildDetails.SUPPLIER_INV_DATE = string.IsNullOrEmpty(childCol.SUPPLIER_INV_DATE) ? (masterColumnValue.SUPPLIER_INV_DATE == "Invalid date" ? null : masterColumnValue.SUPPLIER_INV_DATE) : childCol.SUPPLIER_INV_DATE;
                    inventoryChildDetails.SUPPLIER_BUDGET_FLAG = string.IsNullOrEmpty(childCol.SUPPLIER_BUDGET_FLAG) ? masterColumnValue.SUPPLIER_BUDGET_FLAG : childCol.SUPPLIER_BUDGET_FLAG;
                    inventoryChildDetails.BUDGET_FLAG = string.IsNullOrEmpty(inventoryChildDetails.BUDGET_FLAG) ? inventoryChildDetails.BUDGET_FLAG : inventoryChildDetails.BUDGET_FLAG;
                    inventoryChildDetails.DUE_DATE = string.IsNullOrEmpty(childCol.DUE_DATE) ? (masterColumnValue.DUE_DATE == "Invalid date" ? null : masterColumnValue.DUE_DATE) : childCol.DUE_DATE;
                    inventoryChildDetails.CURRENCY_CODE = string.IsNullOrEmpty(childCol.CURRENCY_CODE) ? "NRS" : masterColumnValue.CURRENCY_CODE;
                    inventoryChildDetails.EXCHANGE_RATE = string.IsNullOrEmpty(childCol.EXCHANGE_RATE) ? "1" : masterColumnValue.EXCHANGE_RATE;
                    inventoryChildDetails.TERMS_DAY = string.IsNullOrEmpty(childCol.TERMS_DAY) ? masterColumnValue.TERMS_DAY : childCol.TERMS_DAY;
                    inventoryChildDetails.TRACKING_NO = string.IsNullOrEmpty(childCol.TRACKING_NO) ? masterColumnValue.TRACKING_NO : childCol.TRACKING_NO;
                    inventoryChildDetails.BATCH_NO = string.IsNullOrEmpty(childCol.BATCH_NO) ? masterColumnValue.BATCH_NO : childCol.BATCH_NO;
                    inventoryChildDetails.LOT_NO = string.IsNullOrEmpty(childCol.LOT_NO) ? masterColumnValue.LOT_NO : childCol.LOT_NO;
                    inventoryChildDetails.SUPPLIER_MRR_NO = string.IsNullOrEmpty(childCol.SUPPLIER_MRR_NO) ? masterColumnValue.SUPPLIER_MRR_NO : childCol.SUPPLIER_MRR_NO;
                    inventoryChildDetails.PP_NO = string.IsNullOrEmpty(childCol.PP_NO) ? masterColumnValue.PP_NO : childCol.PP_NO;
                    inventoryChildDetails.P_TYPE = string.IsNullOrEmpty(childCol.P_TYPE) ? masterColumnValue.P_TYPE : childCol.P_TYPE;
                    inventoryChildDetails.PP_DATE = string.IsNullOrEmpty(childCol.PP_DATE) ? masterColumnValue.PP_DATE : childCol.PP_DATE;
                    inventoryChildDetails.NET_GROSS_RATE = string.IsNullOrEmpty(childCol.NET_GROSS_RATE) ? masterColumnValue.NET_GROSS_RATE : childCol.NET_GROSS_RATE;
                    inventoryChildDetails.NET_SALES_RATE = string.IsNullOrEmpty(childCol.NET_SALES_RATE) ? masterColumnValue.NET_SALES_RATE : childCol.NET_SALES_RATE;
                    inventoryChildDetails.NET_TAXABLE_RATE = string.IsNullOrEmpty(childCol.NET_TAXABLE_RATE) ? masterColumnValue.NET_TAXABLE_RATE : childCol.NET_TAXABLE_RATE;
                    inventoryChildDetails.MASTER_PP_NO = string.IsNullOrEmpty(childCol.MASTER_PP_NO) ? masterColumnValue.MASTER_PP_NO : childCol.MASTER_PP_NO;
                    inventoryChildDetails.SECOND_QUANTITY = string.IsNullOrEmpty(childCol.SECOND_QUANTITY) ? masterColumnValue.SECOND_QUANTITY : childCol.SECOND_QUANTITY;
                    inventoryChildDetails.THIRD_QUANTITY = string.IsNullOrEmpty(childCol.THIRD_QUANTITY) ? masterColumnValue.THIRD_QUANTITY : childCol.THIRD_QUANTITY;
                    inventoryChildDetails.RECONCILE_DATE = string.IsNullOrEmpty(childCol.RECONCILE_DATE) ? masterColumnValue.RECONCILE_DATE : childCol.RECONCILE_DATE;
                    inventoryChildDetails.RECONCILE_FLAG = string.IsNullOrEmpty(childCol.RECONCILE_FLAG) ? masterColumnValue.RECONCILE_FLAG : childCol.RECONCILE_FLAG;
                    inventoryChildDetails.RECONCILE_BY = string.IsNullOrEmpty(childCol.RECONCILE_BY) ? masterColumnValue.RECONCILE_BY : childCol.RECONCILE_BY;
                    inventoryChildDetails.PHOTO_FILE_NAME1 = string.IsNullOrEmpty(childCol.PHOTO_FILE_NAME1) ? masterColumnValue.PHOTO_FILE_NAME1 : childCol.PHOTO_FILE_NAME1;
                    inventoryChildDetails.PHOTO_FILE_NAME2 = string.IsNullOrEmpty(childCol.PHOTO_FILE_NAME2) ? masterColumnValue.PHOTO_FILE_NAME2 : childCol.PHOTO_FILE_NAME2;
                    inventoryChildDetails.SPECIFICATION = string.IsNullOrEmpty(childCol.SPECIFICATION) ? masterColumnValue.SPECIFICATION : childCol.SPECIFICATION;
                    inventoryChildDetails.SUPPLIER_MRR_DATE = string.IsNullOrEmpty(childCol.SUPPLIER_MRR_DATE) ? masterColumnValue.SUPPLIER_MRR_DATE : childCol.SUPPLIER_MRR_DATE;
                    inventoryChildDetails.BRAND_NAME = string.IsNullOrEmpty(childCol.BRAND_NAME) ? masterColumnValue.BRAND_NAME : childCol.BRAND_NAME;
                    inventoryChildDetails.BRAND_ACCEPT_FLAG = string.IsNullOrEmpty(childCol.BRAND_ACCEPT_FLAG) ? masterColumnValue.BRAND_ACCEPT_FLAG : childCol.BRAND_ACCEPT_FLAG;
                    inventoryChildDetails.BRAND_REMARKS = string.IsNullOrEmpty(childCol.BRAND_REMARKS) ? masterColumnValue.BRAND_REMARKS : childCol.BRAND_REMARKS;
                    inventoryChildDetails.RACK_QTY = string.IsNullOrEmpty(childCol.RACK_QTY) ? masterColumnValue.RACK_QTY : childCol.RACK_QTY;
                    inventoryChildDetails.RACK2_QTY = string.IsNullOrEmpty(childCol.RACK2_QTY) ? masterColumnValue.RACK2_QTY : childCol.RACK2_QTY;
                    inventoryChildDetails.GATE_ENTRY_NO = string.IsNullOrEmpty(childCol.GATE_ENTRY_NO) ? masterColumnValue.GATE_ENTRY_NO : childCol.GATE_ENTRY_NO;
                    inventoryChildDetails.ISSUE_NO = string.IsNullOrEmpty(childCol.ISSUE_NO) ? masterColumnValue.ISSUE_NO : childCol.ISSUE_NO;
                    inventoryChildDetails.ISSUE_DATE = string.IsNullOrEmpty(childCol.ISSUE_DATE) ? masterColumnValue.ISSUE_DATE : childCol.ISSUE_DATE;
                    inventoryChildDetails.ISSUE_TYPE_CODE = string.IsNullOrEmpty(childCol.ISSUE_TYPE_CODE) ? masterColumnValue.ISSUE_TYPE_CODE : childCol.ISSUE_TYPE_CODE;
                    inventoryChildDetails.FROM_LOCATION_CODE = string.IsNullOrEmpty(childCol.FROM_LOCATION_CODE) ? masterColumnValue.FROM_LOCATION_CODE : childCol.FROM_LOCATION_CODE;
                    inventoryChildDetails.TO_BUDGET_FLAG = string.IsNullOrEmpty(childCol.TO_BUDGET_FLAG) ? masterColumnValue.TO_BUDGET_FLAG : childCol.TO_BUDGET_FLAG;
                    inventoryChildDetails.REQ_QUANTITY = string.IsNullOrEmpty(childCol.REQ_QUANTITY) ? masterColumnValue.REQ_QUANTITY : childCol.REQ_QUANTITY;
                    inventoryChildDetails.PRODUCTION_QTY = string.IsNullOrEmpty(childCol.PRODUCTION_QTY) ? masterColumnValue.PRODUCTION_QTY : childCol.PRODUCTION_QTY;
                    inventoryChildDetails.PRODUCT_CODE = string.IsNullOrEmpty(childCol.PRODUCT_CODE) ? masterColumnValue.PRODUCT_CODE : childCol.PRODUCT_CODE;
                    inventoryChildDetails.USE_PLACE = string.IsNullOrEmpty(childCol.USE_PLACE) ? masterColumnValue.USE_PLACE : childCol.USE_PLACE;
                    inventoryChildDetails.CUSTOMER_CODE = string.IsNullOrEmpty(childCol.CUSTOMER_CODE) ? masterColumnValue.CUSTOMER_CODE : childCol.CUSTOMER_CODE;
                    inventoryChildDetails.EMPLOYEE_CODE = string.IsNullOrEmpty(childCol.EMPLOYEE_CODE) ? masterColumnValue.EMPLOYEE_CODE : childCol.EMPLOYEE_CODE;
                    inventoryChildDetails.ISSUE_SLIP_NO = string.IsNullOrEmpty(childCol.ISSUE_SLIP_NO) ? masterColumnValue.ISSUE_SLIP_NO : childCol.ISSUE_SLIP_NO;
                    inventoryChildDetails.REFERENCE_NO = string.IsNullOrEmpty(childCol.REFERENCE_NO) ? masterColumnValue.REFERENCE_NO : childCol.REFERENCE_NO;
                    inventoryChildDetails.RETURN_NO = string.IsNullOrEmpty(childCol.RETURN_NO) ? masterColumnValue.RETURN_NO : childCol.RETURN_NO;
                    inventoryChildDetails.RETURN_DATE = string.IsNullOrEmpty(childCol.RETURN_DATE) ? masterColumnValue.RETURN_DATE : childCol.RETURN_DATE;
                    inventoryChildDetails.BUDGET_CODE = string.IsNullOrEmpty(childCol.BUDGET_CODE) ? masterColumnValue.BUDGET_CODE : childCol.BUDGET_CODE;
                    inventoryChildDetails.TERMS_DAYS = string.IsNullOrEmpty(childCol.TERMS_DAYS) ? masterColumnValue.TERMS_DAYS : childCol.TERMS_DAYS;
                    inventoryChildDetails.REQUISITION_NO = string.IsNullOrEmpty(childCol.REQUISITION_NO) ? masterColumnValue.REQUISITION_NO : childCol.REQUISITION_NO;
                    inventoryChildDetails.REQUISITION_DATE = string.IsNullOrEmpty(childCol.REQUISITION_DATE) ? masterColumnValue.REQUISITION_DATE : childCol.REQUISITION_DATE;
                    inventoryChildDetails.BUYERS_NAME = string.IsNullOrEmpty(childCol.BUYERS_NAME) ? masterColumnValue.BUYERS_NAME : childCol.BUYERS_NAME;
                    inventoryChildDetails.BUYERS_ADDRESS = string.IsNullOrEmpty(childCol.BUYERS_ADDRESS) ? masterColumnValue.BUYERS_ADDRESS : childCol.BUYERS_ADDRESS;
                    inventoryChildDetails.ACTUAL_QUANTITY = string.IsNullOrEmpty(childCol.ACTUAL_QUANTITY) ? masterColumnValue.ACTUAL_QUANTITY : childCol.ACTUAL_QUANTITY;
                    inventoryChildDetails.ACKNOWLEDGE_BY = string.IsNullOrEmpty(childCol.ACKNOWLEDGE_BY) ? masterColumnValue.ACKNOWLEDGE_BY : childCol.ACKNOWLEDGE_BY;
                    inventoryChildDetails.ACKNOWLEDGE_DATE = string.IsNullOrEmpty(childCol.ACKNOWLEDGE_DATE) ? masterColumnValue.ACKNOWLEDGE_DATE : childCol.ACKNOWLEDGE_DATE;
                    inventoryChildDetails.OPENING_DATA_FLAG = string.IsNullOrEmpty(childCol.OPENING_DATA_FLAG) ? masterColumnValue.OPENING_DATA_FLAG : childCol.OPENING_DATA_FLAG;
                    inventoryChildDetails.TO_FORM_CODE = string.IsNullOrEmpty(childCol.TO_FORM_CODE) ? masterColumnValue.TO_FORM_CODE : childCol.TO_FORM_CODE;
                    inventoryChildDetails.ORDER_NO = string.IsNullOrEmpty(childCol.ORDER_NO) ? masterColumnValue.ORDER_NO : childCol.ORDER_NO;
                    inventoryChildDetails.ORDER_DATE = string.IsNullOrEmpty(childCol.ORDER_DATE) ? masterColumnValue.ORDER_DATE : childCol.ORDER_DATE;
                    inventoryChildDetails.DELIVERY_DATE = string.IsNullOrEmpty(childCol.DELIVERY_DATE) ? masterColumnValue.DELIVERY_DATE : childCol.DELIVERY_DATE;
                    inventoryChildDetails.DELIVERY_TERMS = string.IsNullOrEmpty(childCol.DELIVERY_TERMS) ? masterColumnValue.DELIVERY_TERMS : childCol.DELIVERY_TERMS;
                    inventoryChildDetails.CANCEL_QUANTITY = string.IsNullOrEmpty(childCol.CANCEL_QUANTITY) ? masterColumnValue.CANCEL_QUANTITY : childCol.CANCEL_QUANTITY;
                    inventoryChildDetails.ADJUST_QUANTITY = string.IsNullOrEmpty(childCol.ADJUST_QUANTITY) ? masterColumnValue.ADJUST_QUANTITY : childCol.ADJUST_QUANTITY;
                    inventoryChildDetails.CANCEL_FLAG = string.IsNullOrEmpty(childCol.CANCEL_FLAG) ? masterColumnValue.CANCEL_FLAG : childCol.CANCEL_FLAG;
                    inventoryChildDetails.CANCEL_BY = string.IsNullOrEmpty(childCol.CANCEL_BY) ? masterColumnValue.CANCEL_BY : childCol.CANCEL_BY;
                    inventoryChildDetails.CANCEL_DATE = string.IsNullOrEmpty(childCol.CANCEL_DATE) ? masterColumnValue.CANCEL_DATE : childCol.CANCEL_DATE;
                    inventoryChildDetails.PARTY_CODE = string.IsNullOrEmpty(childCol.PARTY_CODE) ? masterColumnValue.PARTY_CODE : childCol.PARTY_CODE;
                    inventoryChildDetails.MRR_DATE = string.IsNullOrEmpty(childCol.MRR_DATE) ? masterColumnValue.MRR_DATE : childCol.MRR_DATE;
                    inventoryChildDetails.REQUEST_DATE = string.IsNullOrEmpty(childCol.REQUEST_DATE) ? masterColumnValue.REQUEST_DATE : childCol.REQUEST_DATE;
                    inventoryChildDetails.QUOTE_DATE = string.IsNullOrEmpty(childCol.QUOTE_DATE) ? masterColumnValue.QUOTE_DATE : "SYSDATE";
                  
                     if (commonValue.TableName.ToUpper() == "IP_QUOTATION_INQUIRY")
                    {
                        inventoryChildDetails = ProcessImageData(inventoryChildDetails);
                        var idquery = $@"SELECT COALESCE(MAX(ID) + 1, 1) AS id FROM sa_quotation_Items";
                        int id = _dbContext.SqlQuery<int>(idquery).FirstOrDefault();
                        string insertItemQuery = $@"INSERT INTO sa_quotation_Items (ID,TENDER_NO, ITEM_CODE, SPECIFICATION, IMAGE, UNIT, QUANTITY, Category, BRAND_NAME, INTERFACE, TYPE, LAMINATION, ITEM_SIZE, THICKNESS, COLOR, GRADE, SIZE_LENGTH, SIZE_WIDTH,DELETED_FLAG,REMARKS) 
                             VALUES({id},'{commonValue.NewVoucherNumber}','{inventoryChildDetails.ITEM_CODE}','{inventoryChildDetails.SPECIFICATION}', '{inventoryChildDetails.IMAGE}', '{inventoryChildDetails.MU_CODE}', {inventoryChildDetails.QUANTITY}, '{inventoryChildDetails.CATEGORY}',
                            '{inventoryChildDetails.BRAND_NAME}', '{inventoryChildDetails.INTERFACE}', '{inventoryChildDetails.TYPE}', '{inventoryChildDetails.LAMINATION}', '{inventoryChildDetails.ITEM_SIZE}', '{inventoryChildDetails.THICKNESS}', '{inventoryChildDetails.COLOR}', '{inventoryChildDetails.GRADE}', {inventoryChildDetails.SIZE_LENGTH}, {inventoryChildDetails.SIZE_WIDTH},'N','{inventoryChildDetails.REMARKS}')";

                        _dbContext.ExecuteSqlCommand(insertItemQuery);
                        serialno++;
                        insertedToChild = true;
                    }
                }
                serialno++;
                return insertedToChild;
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }

        public bool InsertItemData(Item item, string tenderNo)
        {
            try
            {
                //item = ProcessImageData(item);
                var idquery = $@"SELECT COALESCE(MAX(ID) + 1, 1) AS id FROM sa_quotation_Items";
                int id = _dbContext.SqlQuery<int>(idquery).FirstOrDefault();
                string insertItemQuery = string.Format(@"INSERT INTO sa_quotation_Items (ID,TENDER_NO, ITEM_CODE, SPECIFICATION, IMAGE, UNIT, QUANTITY, Category, BRAND_NAME, INTERFACE, TYPE, LAMINATION, ITEM_SIZE, THICKNESS, COLOR, GRADE, SIZE_LENGTH, SIZE_WIDTH,DELETED_FLAG) 
                             VALUES({0}, '{1}', '{2}', '{3}', '{4}', '{5}', {6}, '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', {16}, {17},'{18}')",
                                     id, tenderNo,item.ITEM_CODE, item.SPECIFICATION, item.IMAGE, item.UNIT, item.QUANTITY, item.CATEGORY, item.BRAND_NAME, item.INTERFACE, item.TYPE, item.LAMINATION, item.ITEM_SIZE, item.THICKNESS, item.COLOR, item.GRADE, item.SIZE_LENGTH, item.SIZE_WIDTH,"N");

                _dbContext.ExecuteSqlCommand(insertItemQuery);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        //private Item ProcessImageData(Item item)
        //{
        //    if (!string.IsNullOrEmpty(item.IMAGE))
        //    {
        //        byte[] imageBytes = Convert.FromBase64String(item.IMAGE);

        //        string folderPath = "~/Areas/NeoERP.QuotationManagement/Image/Items/";
        //        string imageName = $"{Guid.NewGuid()}.png"; // Generating unique image name
        //        string imagePath = $"{folderPath}{imageName}"; // Combining folder path and image name
        //        string physicalPath = HttpContext.Current.Server.MapPath(imagePath);
        //        File.WriteAllBytes(physicalPath, imageBytes);
        //        item.IMAGE = imageName;
        //    }
        //    else
        //    {
        //        item.IMAGE = item.IMAGE_NAME;
        //    }

        //    return item;
        //}
        private Inventory ProcessImageData(Inventory data)
        {
            if (!string.IsNullOrEmpty(data.IMAGE))
            {
                byte[] imageBytes = Convert.FromBase64String(data.IMAGE);

                string folderPath = "~/Areas/NeoERP.QuotationManagement/Image/Items/";
                string imageName = $"{Guid.NewGuid()}.png"; // Generating unique image name
                string imagePath = $"{folderPath}{imageName}"; // Combining folder path and image name
                string physicalPath = HttpContext.Current.Server.MapPath(imagePath);
                File.WriteAllBytes(physicalPath, imageBytes);
                data.IMAGE = imageName;
            }
            else
            {
                data.IMAGE = data.IMAGE_NAME;
            }

            return data;
        }

        //public bool UpdateItemData(Item item, string tenderNo)
        //{
        //    try
        //    {
        //        //item = ProcessImageData(item);

        //        string updateItemQuery = string.Format(@"UPDATE sa_quotation_Items  SET ITEM_CODE = '{2}', 
        //         SPECIFICATION = '{3}', IMAGE = '{4}',UNIT = '{5}', QUANTITY = {6},Category = '{7}',BRAND_NAME = '{8}', 
        //         INTERFACE = '{9}',TYPE = '{10}',LAMINATION = '{11}', ITEM_SIZE = '{12}', THICKNESS = '{13}', COLOR = '{14}', 
        //         GRADE = '{15}',SIZE_LENGTH = {16},SIZE_WIDTH = {17} WHERE TENDER_NO = '{1}' AND ID = {0}",
        //                                 item.ID, tenderNo, item.ITEM_CODE, item.SPECIFICATION, item.IMAGE, item.UNIT, item.QUANTITY, item.CATEGORY, item.BRAND_NAME, item.INTERFACE, item.TYPE, item.LAMINATION, item.ITEM_SIZE, item.THICKNESS, item.COLOR, item.GRADE, item.SIZE_LENGTH, item.SIZE_WIDTH, "N");
        //        _dbContext.ExecuteSqlCommand(updateItemQuery);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}
        public bool UpdateItemData(List<Inventory> childColumnValue, Inventory masterColumnValue, CommonFieldsForInventory commonValue, FormDetails model)
        {
            try
            {
                foreach (var childCol in childColumnValue)
                {
                    var query = $@"SELECT * FROM sa_quotation_Items WHERE tender_no='{commonValue.NewVoucherNumber}' AND id='{childCol.ID}'";
                    List<Item> itemDetails = _dbContext.SqlQuery<Item>(query).ToList();

                    if (itemDetails.Any())
                    {
                        Inventory invDetails = ProcessImageData(childCol);
                        string updateItemQuery = $@"UPDATE sa_quotation_Items SET ITEM_CODE = '{childCol.ITEM_CODE}',SPECIFICATION = '{childCol.SPECIFICATION}', IMAGE = '{invDetails.IMAGE}',UNIT = '{childCol.MU_CODE}', QUANTITY = {childCol.QUANTITY},REMARKS='{childCol.REMARKS}'";

                        List<string> setClauses = new List<string>();

                        if (!string.IsNullOrEmpty(childCol.CATEGORY))
                            setClauses.Add($"CATEGORY = '{childCol.CATEGORY}'");
                        if (!string.IsNullOrEmpty(childCol.BRAND_NAME))
                            setClauses.Add($"BRAND_NAME = '{childCol.BRAND_NAME}'");
                        if (!string.IsNullOrEmpty(childCol.INTERFACE))
                            setClauses.Add($"INTERFACE = '{childCol.INTERFACE}'");
                        if (!string.IsNullOrEmpty(childCol.TYPE))
                            setClauses.Add($"TYPE = '{childCol.TYPE}'");
                        if (!string.IsNullOrEmpty(childCol.LAMINATION))
                            setClauses.Add($"LAMINATION = '{childCol.LAMINATION}'");
                        if (!string.IsNullOrEmpty(childCol.ITEM_SIZE))
                            setClauses.Add($"ITEM_SIZE = '{childCol.ITEM_SIZE}'");
                        if (!string.IsNullOrEmpty(childCol.THICKNESS))
                            setClauses.Add($"THICKNESS = '{childCol.THICKNESS}'");
                        if (!string.IsNullOrEmpty(childCol.COLOR))
                            setClauses.Add($"COLOR = '{childCol.COLOR}'");
                        if (!string.IsNullOrEmpty(childCol.GRADE))
                            setClauses.Add($"GRADE = '{childCol.GRADE}'");
                        if (childCol.SIZE_LENGTH != 0)
                            setClauses.Add($"SIZE_LENGTH = {childCol.SIZE_LENGTH}");
                        if (childCol.SIZE_WIDTH != 0)
                            setClauses.Add($"SIZE_WIDTH = {childCol.SIZE_WIDTH}");
                        if (setClauses.Count > 0)
                        {
                            updateItemQuery += ", " + string.Join(", ", setClauses);
                        }

                        updateItemQuery += $" WHERE TENDER_NO = '{commonValue.NewVoucherNumber}' AND ID={childCol.ID}";

                        _dbContext.ExecuteSqlCommand(updateItemQuery);

                    }
                    else
                    {
                        Inventory invDetails = ProcessImageData(childCol);
                        var idquery = $@"SELECT COALESCE(MAX(ID) + 1, 1) AS id FROM sa_quotation_Items";
                        int id = _dbContext.SqlQuery<int>(idquery).FirstOrDefault();
                        string insertItemQuery = $@"INSERT INTO sa_quotation_Items (ID,TENDER_NO, ITEM_CODE, SPECIFICATION, IMAGE, UNIT, QUANTITY, Category, BRAND_NAME, INTERFACE, TYPE, LAMINATION, ITEM_SIZE, THICKNESS, COLOR, GRADE, SIZE_LENGTH, SIZE_WIDTH,DELETED_FLAG,REMARKS) 
                             VALUES({id},'{commonValue.NewVoucherNumber}','{childCol.ITEM_CODE}','{childCol.SPECIFICATION}', '{invDetails.IMAGE}', '{childCol.MU_CODE}', {childCol.QUANTITY}, '{childCol.CATEGORY}',
                            '{childCol.BRAND_NAME}', '{childCol.INTERFACE}', '{childCol.TYPE}', '{childCol.LAMINATION}', '{childCol.ITEM_SIZE}', '{childCol.THICKNESS}', '{childCol.COLOR}', '{childCol.GRADE}', {childCol.SIZE_LENGTH}, {childCol.SIZE_WIDTH},'N','{childCol.REMARKS}')";
                        _dbContext.ExecuteSqlCommand(insertItemQuery);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating item data: " + ex.Message);
            }
        }

        public List<Quotation_setup> GetTenderId(string tenderNo)
        {
            string query = $@"select id from sa_quotation_setup where tender_no='{tenderNo}'";
            List<Quotation_setup> id = _dbContext.SqlQuery<Quotation_setup>(query).ToList();
            return id;
        }

        public List<Quotation_setup> ListAllTenders()
        {
            string query = $@"SELECT  TENDER_NO,ISSUE_DATE,VALID_DATE,CREATED_DATE,bs_date(ISSUE_DATE) as NEPALI_DATE,
       CASE   WHEN approved_status = 'Y' THEN 'Approved'  ELSE 'Pending'  END AS approved_status FROM sa_quotation_setup WHERE status = 'E' and company_code='{_workContext.CurrentUserinformation.company_code}' ORDER BY id desc";
            List<Quotation_setup> tenderDetails = _dbContext.SqlQuery<Quotation_setup>(query).ToList();
            return tenderDetails;
        }
        public bool deleteQuotationId(string tenderNo)
        {
            try
            {
                var UPDATE_QUERY = $@"UPDATE sa_quotation_setup SET STATUS ='D' WHERE TENDER_NO='{tenderNo}'";
                _dbContext.ExecuteSqlCommand(UPDATE_QUERY);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<Quotation_setup> GetQuotationById(string tenderNo)
        {
            try
            {
                // Fetch project data
                string Query = $@"SELECT TENDER_NO,ISSUE_DATE,VALID_DATE,CREATED_DATE,bs_date(ISSUE_DATE) as NEPALI_DATE,bs_date(VALID_DATE) as DELIVERY_DT_BS,REMARKS,ID,
                (CASE WHEN APPROVED_STATUS='Y' THEN 'Approved' else 'Pending' END) AS APPROVED_STATUS FROM sa_quotation_setup WHERE TENDER_NO = '{tenderNo}' AND STATUS = 'E'";
                List<Quotation_setup> quotations = this._dbContext.SqlQuery<Quotation_setup>(Query).ToList();
                foreach (var quotation in quotations)
                {
                    string query = $@"select * from sa_quotation_Items where TENDER_NO='{quotation.TENDER_NO}' AND DELETED_FLAG='N' order by id";
                    List<Item> itemData = this._dbContext.SqlQuery<Item>(query).ToList();
                    quotation.Items = itemData;
                }

                return quotations;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public bool updateItemsById(string id)
        {
            try
            {
                var UPDATE_QUERY = $@"UPDATE sa_quotation_Items SET deleted_flag ='Y' WHERE id='{id}'";
                _dbContext.ExecuteSqlCommand(UPDATE_QUERY);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<Quotation_Details> ListQuotationDetails()
        {
            string query = $@"SELECT QD.QUOTATION_NO,QD.TENDER_NO,QD.PAN_NO,QD.PARTY_NAME,QD.ADDRESS,QD.CONTACT_NO,QD.EMAIL,QD.CURRENCY,QD.CURRENCY_RATE,QD.DELIVERY_DATE,
            QD.TOTAL_AMOUNT,QD.TOTAL_DISCOUNT,QD.TOTAL_EXCISE,QD.TOTAL_TAXABLE_AMOUNT,QD.TOTAL_VAT,QD.TOTAL_NET_AMOUNT,
            (CASE WHEN QD.STATUS='RQ' then 'Pending' when QD.status='AP' then 'Approved' else 'Reject' end) AS STATUS,
            QD.TERM_CONDITION FROM  QUOTATION_DETAILS QD,SA_QUOTATION_SETUP SQS WHERE SQS.TENDER_NO=QD.TENDER_NO AND
            SQS.COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' ORDER BY QD.QUOTATION_NO DESC";
            List<Quotation_Details> tenderDetails = _dbContext.SqlQuery<Quotation_Details>(query).ToList();
            return tenderDetails;
        }
        public List<Quotation_Details> QuotationDetailsById(string quotationNo,string tenderNo)
        {
            try
            {
                string Query = $@"SELECT SQS.ISSUE_DATE,SQS.VALID_DATE,BS_DATE(SQS.ISSUE_DATE) AS NEPALI_DATE,BS_DATE(SQS.VALID_DATE) AS DELIVERY_DT_BS,QD.QUOTATION_NO,QD.TENDER_NO,QD.PAN_NO,scs.supplier_edesc as party_name,scs.regd_office_eaddress as address,
                scs.tel_mobile_no1 as contact_no,scs.EMAIL,QD.CURRENCY,QD.CURRENCY_RATE,QD.DELIVERY_DATE,BS_DATE(QD.DELIVERY_DATE) as DELIVERY_DT_NEP,
                QD.TOTAL_AMOUNT,QD.TOTAL_DISCOUNT,QD.TOTAL_EXCISE,QD.TOTAL_TAXABLE_AMOUNT,QD.TOTAL_VAT,QD.TOTAL_NET_AMOUNT,
                CASE 
                WHEN QD.STATUS = 'AP' THEN 'Approved' WHEN QD.STATUS = 'R' THEN 'Reject'
                WHEN NOT EXISTS (SELECT 1 FROM QUOTATION_DETAILS WHERE TENDER_NO = QD.TENDER_NO AND STATUS = 'AP') THEN 'Pending'
                ELSE 'Reject' END AS STATUS,QD.DISCOUNT_TYPE,SQS.REMARKS  FROM  QUOTATION_DETAILS  QD,SA_QUOTATION_SETUP SQS , ip_supplier_setup scs WHERE SQS.TENDER_NO=QD.TENDER_NO and scs.supplier_code=qd.supplier_code and sqs.company_code=scs.company_code AND  QD.QUOTATION_NO='{quotationNo}'";
                List<Quotation_Details> quotations = this._dbContext.SqlQuery<Quotation_Details>(Query).ToList();
                foreach (var quotation in quotations)
                {
                    string query =$@"SELECT IIMS.ITEM_CODE,SQI.SPECIFICATION, SQI.IMAGE,SQI.UNIT,SQI.QUANTITY,SQI.CATEGORY, SQI.BRAND_NAME,SQI.INTERFACE,
                        SQI.TYPE, SQI.LAMINATION, SQI.ITEM_SIZE,SQI.THICKNESS,SQI.COLOR,SQI.GRADE,SQI.SIZE_LENGTH, SQI.SIZE_WIDTH,QDI.RATE,
                        QDI.AMOUNT, QDI.DISCOUNT, QDI.DISCOUNT_AMOUNT,QDI.EXCISE,QDI.TAXABLE_AMOUNT, QDI.VAT_AMOUNT,QDI.NET_AMOUNT
                        FROM   SA_QUOTATION_ITEMS SQI, QUOTATION_DETAIL_ITEMWISE QDI,ip_item_master_setup iims, sa_quotation_setup sqs,quotation_details qd  WHERE SQI.ITEM_CODE = QDI.ITEM_CODE AND
                        IIMS.ITEM_CODE = SQI.ITEM_CODE AND SQS.TENDER_NO = SQI.TENDER_NO AND IIMS.COMPANY_CODE = SQS.COMPANY_CODE AND qd.tender_no = sqi.tender_no and qdi.quotation_no=qd.quotation_no AND
                        QDI.QUOTATION_NO = '{quotationNo}' AND SQI.DELETED_FLAG = 'N' ORDER BY SQI.ID";
                    List<Item_details> itemData = this._dbContext.SqlQuery<Item_details>(query).ToList();
                    quotation.Item_Detail = itemData;
                    string vquery = $@"SELECT * FROM QUOTATION_TERM_CONDITION WHERE TENDER_NO='{quotation.TENDER_NO}' AND QUOTATION_NO='{quotationNo}'";
                    List<Term_Conditions> TermCondition = this._dbContext.SqlQuery<Term_Conditions>(vquery).ToList();
                    quotation.TermsCondition = TermCondition;
                }
                List<QuotationTransaction> imagelist = new List<QuotationTransaction>();
                if (quotations.Count > 0)
                {
                    string imagequery = $@"SELECT * FROM QUOTATION_TRANSACTION WHERE QUOTATION_NO ='{quotationNo}' and COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' and TENDER_NO='{tenderNo}' and DELETED_FLAG='N'";
                    imagelist = this._dbContext.SqlQuery<QuotationTransaction>(imagequery).ToList();
                    quotations[0].IMAGES_LIST = imagelist;
                }
                return quotations;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<Quotation_Details> QuotationDetailsId(string quotationNo, string tenderNo)
        {
            try
            {
                string Query = $@"SELECT SQS.ISSUE_DATE,SQS.VALID_DATE,BS_DATE(SQS.ISSUE_DATE) AS NEPALI_DATE,BS_DATE(SQS.VALID_DATE) AS DELIVERY_DT_BS,QD.QUOTATION_NO,QD.TENDER_NO,QD.PAN_NO,scs.supplier_edesc as party_name,scs.regd_office_eaddress as address,
                scs.tel_mobile_no1 as contact_no,scs.EMAIL,QD.CURRENCY,QD.CURRENCY_RATE,QD.DELIVERY_DATE,BS_DATE(QD.DELIVERY_DATE) as DELIVERY_DT_NEP,
                QD.TOTAL_AMOUNT,QD.TOTAL_DISCOUNT,QD.TOTAL_EXCISE,QD.TOTAL_TAXABLE_AMOUNT,QD.TOTAL_VAT,QD.TOTAL_NET_AMOUNT,
                CASE 
                WHEN QD.STATUS = 'AP' THEN 'Approved' WHEN QD.STATUS = 'R' THEN 'Reject'
                WHEN NOT EXISTS (SELECT 1 FROM QUOTATION_DETAILS WHERE TENDER_NO = QD.TENDER_NO AND STATUS = 'AP') THEN 'Pending'
                ELSE 'Reject' END AS STATUS,QD.DISCOUNT_TYPE,SQS.REMARKS  FROM  QUOTATION_DETAILS  QD,SA_QUOTATION_SETUP SQS , ip_supplier_setup scs WHERE SQS.TENDER_NO=QD.TENDER_NO and scs.supplier_code=qd.supplier_code and sqs.company_code=scs.company_code AND  QD.QUOTATION_NO='{quotationNo}'";
                List<Quotation_Details> quotations = this._dbContext.SqlQuery<Quotation_Details>(Query).ToList();
                foreach (var quotation in quotations)
                {
                    string query = $@"SELECT IIMS.ITEM_EDESC as item_code,SQI.SPECIFICATION, SQI.IMAGE,SQI.UNIT,SQI.QUANTITY,SQI.CATEGORY, SQI.BRAND_NAME,SQI.INTERFACE,
                        SQI.TYPE, SQI.LAMINATION, SQI.ITEM_SIZE,SQI.THICKNESS,SQI.COLOR,SQI.GRADE,SQI.SIZE_LENGTH, SQI.SIZE_WIDTH,QDI.RATE,
                        QDI.AMOUNT, QDI.DISCOUNT, QDI.DISCOUNT_AMOUNT,QDI.EXCISE,QDI.TAXABLE_AMOUNT, QDI.VAT_AMOUNT,QDI.NET_AMOUNT
                        FROM   SA_QUOTATION_ITEMS SQI, QUOTATION_DETAIL_ITEMWISE QDI,ip_item_master_setup iims, sa_quotation_setup sqs,quotation_details qd  WHERE SQI.ITEM_CODE = QDI.ITEM_CODE AND
                        IIMS.ITEM_CODE = SQI.ITEM_CODE AND SQS.TENDER_NO = SQI.TENDER_NO AND IIMS.COMPANY_CODE = SQS.COMPANY_CODE AND qd.tender_no = sqi.tender_no and qdi.quotation_no=qd.quotation_no AND
                        QDI.QUOTATION_NO = '{quotationNo}' AND SQI.DELETED_FLAG = 'N' ORDER BY SQI.ID";
                    List<Item_details> itemData = this._dbContext.SqlQuery<Item_details>(query).ToList();
                    quotation.Item_Detail = itemData;
                    string vquery = $@"SELECT * FROM QUOTATION_TERM_CONDITION WHERE TENDER_NO='{quotation.TENDER_NO}' AND QUOTATION_NO='{quotationNo}'";
                    List<Term_Conditions> TermCondition = this._dbContext.SqlQuery<Term_Conditions>(vquery).ToList();
                    quotation.TermsCondition = TermCondition;
                }
                List<QuotationTransaction> imagelist = new List<QuotationTransaction>();
                if (quotations.Count > 0)
                {
                    string imagequery = $@"SELECT * FROM QUOTATION_TRANSACTION WHERE QUOTATION_NO ='{quotationNo}' and COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' and TENDER_NO='{tenderNo}' and DELETED_FLAG='N'";
                    imagelist = this._dbContext.SqlQuery<QuotationTransaction>(imagequery).ToList();
                    quotations[0].IMAGES_LIST = imagelist;
                }
                return quotations;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<SummaryReport> TendersItemWise()
        {
            try
            {
                string Query = $@"SELECT SQS.TENDER_NO,SQS.CREATED_DATE,SQS.VALID_DATE,IIMS.ITEM_EDESC as ITEM_DESC, CASE WHEN EXISTS (
                 SELECT 1 FROM QUOTATION_DETAILS QD WHERE QD.TENDER_NO = SQS.TENDER_NO AND QD.STATUS = 'AP') THEN 'Close'
                 ELSE 'Open' END AS STATUS FROM SA_QUOTATION_SETUP SQS,SA_QUOTATION_ITEMS SQI,IP_ITEM_MASTER_SETUP IIMS
                 WHERE SQS.TENDER_NO=SQI.TENDER_NO AND IIMS.ITEM_CODE=SQI.ITEM_CODE AND SQS.COMPANY_CODE=IIMS.COMPANY_CODE
                 AND SQS.STATUS='E' AND SQI.DELETED_FLAG='N' ORDER BY SQS.ID DESC";
                List<SummaryReport> tenderItemwise = this._dbContext.SqlQuery<SummaryReport>(Query).ToList();
                return tenderItemwise;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<Quotation> ItemDetailsTenderNo(string tenderNo)
        {
            try
            {
                string query = $@"SELECT TENDER_NO, ISSUE_DATE, VALID_DATE, CREATED_DATE, bs_date(ISSUE_DATE) as NEPALI_DATE,bs_date(VALID_DATE) as DELIVERY_DT_BS, COMPANY_CODE FROM SA_QUOTATION_SETUP WHERE STATUS='E' AND TENDER_NO='{tenderNo}'";
                List<Quotation> quotations = this._dbContext.SqlQuery<Quotation>(query).ToList();
                foreach (var quotation in quotations)
                {
                    string vquery = $@"SELECT QD.QUOTATION_NO,scs.supplier_edesc as party_name,ROUND((QDI.TAXABLE_AMOUNT-QDI.EXCISE)/SQI.QUANTITY,2) AS ACTUAL_PRICE,QDI.ITEM_CODE,QD.STATUS,QD.REVISE FROM QUOTATION_DETAILS QD,QUOTATION_DETAIL_ITEMWISE QDI,SA_QUOTATION_ITEMS SQI,ip_supplier_setup scs
                    WHERE QD.QUOTATION_NO=QDI.QUOTATION_NO AND SQI.TENDER_NO=QD.TENDER_NO AND QDI.ITEM_CODE=SQI.ITEM_CODE AND scs.supplier_code=qd.supplier_code AND QD.TENDER_NO = '{quotation.TENDER_NO}' AND SCS.COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' AND SQI.deleted_flag = 'N' order by quotation_no";
                    List<PARTY_DETAIL> partyData = this._dbContext.SqlQuery<PARTY_DETAIL>(vquery).ToList();
                    quotation.PartDetails = partyData;
                    string vQuery = $@"SELECT SQI.*, 
                                   IIMS.ITEM_EDESC AS ITEM_DESC,IIR.CALC_UNIT_PRICE AS LAST_PRICE,IIR.SUPPLIER_CODE AS LAST_VENDOR
                            FROM SA_QUOTATION_ITEMS SQI
                            LEFT JOIN IP_ITEM_MASTER_SETUP IIMS ON IIMS.ITEM_CODE = SQI.ITEM_CODE
                            LEFT JOIN VPRINT_IP_PURCHASE_REQUEST IIR ON IIR.ITEM_CODE = SQI.ITEM_CODE
                            WHERE SQI.TENDER_NO = '{quotation.TENDER_NO}'
                            AND SQI.deleted_flag = 'N'
                            AND IIMS.COMPANY_CODE ='{quotation.COMPANY_CODE}'";
                            List<Item_Detail> itemDetail = this._dbContext.SqlQuery<Item_Detail>(vQuery).ToList();
                        quotation.Items = itemDetail;
                }
                return quotations;
            }   
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public bool acceptQuotation(string quotationNo, string status)
        {
            try
            {
                bool insertedData = false;

                var UPDATE_QUERY = $@"UPDATE quotation_details SET status ='{status}',approved_by='{_workContext.CurrentUserinformation.login_code}',approved_date='{DateTime.Now.ToString("dd - MMM - yyyy")}' WHERE QUOTATION_NO='{quotationNo}'";
                _dbContext.ExecuteSqlCommand(UPDATE_QUERY);


                string fQuery = $@"select FORM_CODE from form_setup where form_edesc ='Quotation Enquiry' and  company_code= '{_workContext.CurrentUserinformation.company_code}'";
                string formCode = _dbContext.SqlQuery<string>(fQuery).FirstOrDefault();

                string vquery = $@"select qs.quotation_no,qs.tender_no,qs.created_date,qs.supplier_code,scs.regd_office_eaddress as address,scs.tel_mobile_no1 as contact_no ,iims.item_code,sqi.specification,
                iims.index_mu_code,sqi.quantity,qdi.rate,qdi.net_amount as total_net_amount,sqs.company_code,sqs.branch_code,qs.currency,qs.currency_rate,qs.delivery_date
                from quotation_details qs ,ip_supplier_setup scs,sa_quotation_items sqi,ip_item_master_setup iims ,sa_quotation_setup sqs,quotation_detail_itemwise qdi
                where scs.supplier_code=qs.supplier_code and iims.item_code=sqi.item_code and qs.tender_no=sqi.tender_no and sqs.tender_no=qs.tender_no and 
                sqs.company_code=iims.company_code and sqs.company_code=scs.company_code and qdi.quotation_no=qs.quotation_no and  sqi.item_code=qdi.item_code
                and  qs.quotation_no='{quotationNo}' and sqi.deleted_flag='N' and sqs.status='E'";
                List<QuotationDetails> quoteData = _dbContext.SqlQuery<QuotationDetails>(vquery).ToList();
                quoteData.ForEach(q => q.Form_Code = formCode);

                insertedData = InsertQuotesData(quoteData);
                if (insertedData)
                {
                    string query = $@"SELECT qs.tender_no,  qs.created_date,  qs.supplier_code,  qs.total_net_amount, sqs.company_code,
                      sqs.branch_code,  qs.currency, qs.currency_rate,  qs.delivery_date FROM  quotation_details qs,
                       sa_quotation_setup sqs WHERE qs.tender_no =sqs.tender_no  AND qs.quotation_no = '{quotationNo}' AND sqs.status = 'E'";
                    List<QuotationDetails> masterData = _dbContext.SqlQuery<QuotationDetails>(query).ToList();
                    masterData.ForEach(q => q.Form_Code = formCode);
                    SaveMasterColumnValue(masterData);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public bool InsertQuotesData(List<QuotationDetails> quoteData)
        {
            try
            {
                foreach (var data in quoteData)
                {
                    var idquery = $@"SELECT COALESCE(MAX(SERIAL_NO) + 1, 1) AS id FROM ip_quotation_inquiry where QUOTE_NO='{data.Tender_No}'";
                    int id = _dbContext.SqlQuery<int>(idquery).FirstOrDefault();
                    //var date=data.Created_Date-data.Delivery_Date;
                    TimeSpan dateDifference = data.Delivery_Date.Date - data.Created_Date.Date;
                    int deliveryDays = (int)dateDifference.TotalDays;
                    string sQuery = $@"SELECT MYSEQUENCE.NEXTVAL FROM DUAL";
                    var sessionRowId =_dbContext.SqlQuery<int>(sQuery).FirstOrDefault();

                    string insertItemQuery = $@"
                    INSERT INTO ip_quotation_inquiry (
                    QUOTE_NO, QUOTE_DATE, ORDER_NO, REQUEST_NO, MANUAL_NO, SUPPLIER_CODE, ADDRESS, CONTACT_PERSON, PHONE_NO, SERIAL_NO, ITEM_CODE,
                    SPECIFICATION, MU_CODE, QUANTITY, UNIT_PRICE, TOTAL_PRICE, REMARKS, FORM_CODE, COMPANY_CODE, BRANCH_CODE, CREATED_BY, CREATED_DATE, CURRENCY_CODE, EXCHANGE_RATE,
                    BRAND_NAME, DELIVERY_DATE, APPROVED_FLAG, APPROVED_BY, APPROVED_DATE,SESSION_ROWID,DELETED_FLAG,DELIVERY_DAYS
                    ) VALUES (
                '{data.Tender_No}', TO_DATE('{data.Created_Date.ToString("dd-MMM-yyyy")}', 'DD-MON-YYYY'), null,null, '{id}', '{data.SUPPLIER_Code}',
                '{data.Address}', NULL, '{data.Contact_No}', {id}, '{data.Item_Code}', '{data.Specification}', '{data.Index_Mu_Code}', {data.Quantity}, '{data.Rate}', '{data.Total_Net_Amount}', null, '{data.Form_Code}',
                '{data.Company_Code}', '{data.Branch_Code}', '{_workContext.CurrentUserinformation.login_code}', '{DateTime.Now.ToString("dd-MMM-yyyy")}','{data.Currency}', '{data.Currency_Rate}', '{data.Brand_Name}', TO_DATE('{data.Delivery_Date.ToString("dd-MMM-yyyy")}', 'DD-MON-YYYY'), 'Y', '{_workContext.CurrentUserinformation.login_code}', '{DateTime.Now.ToString("dd-MMM-yyyy")}'
                ,'{sessionRowId}','N',{deliveryDays} )";
                    _dbContext.ExecuteSqlCommand(insertItemQuery);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }

        }
        public bool SaveMasterColumnValue(List<QuotationDetails> quodata)
        {
            try
            {
                foreach (var data in quodata)
                {
                    string sQuery = $@"SELECT MYSEQUENCE.NEXTVAL FROM DUAL";
                    var sessionRowId = _dbContext.SqlQuery<int>(sQuery).FirstOrDefault();

                    string insertmasterQuery = $@"INSERT INTO MASTER_TRANSACTION(VOUCHER_NO, VOUCHER_AMOUNT, FORM_CODE, COMPANY_CODE, BRANCH_CODE, CREATED_BY, DELETED_FLAG, CURRENCY_CODE, CREATED_DATE, VOUCHER_DATE, SESSION_ROWID, SYN_ROWID, EXCHANGE_RATE, IS_SYNC_WITH_IRD, IS_REAL_TIME) 
            VALUES ('{data.Tender_No}', '{data.Total_Net_Amount}', '{data.Form_Code}', '{_workContext.CurrentUserinformation.company_code}', '{_workContext.CurrentUserinformation.branch_code}', '{_workContext.CurrentUserinformation.login_code.ToUpper()}', 'N', '{data.Currency}', '{DateTime.Now.ToString("dd-MMM-yyyy")}',
            '{DateTime.Now.ToString("dd-MMM-yyyy")}', '{sessionRowId}', '1', '{data.Currency_Rate}', 'N', 'N')";
                    _dbContext.ExecuteSqlCommand(insertmasterQuery);
                }
                return true;
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }
        public bool rejectQuotation(string quotationNo, string status)
        {
            try
            {
                var UPDATE_QUERY = $@"UPDATE quotation_details SET status ='{status}',rejected_by='{_workContext.CurrentUserinformation.login_code}',rejected_date='{DateTime.Now.ToString("dd - MMM - yyyy")}' WHERE QUOTATION_NO='{quotationNo}'";
                _dbContext.ExecuteSqlCommand(UPDATE_QUERY);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public bool InsertTenderData(Tender data)
        {
            try
            {
                int tenderId = data.ID;
                if (tenderId == 0)
                {
                    var idquery = $@"SELECT COALESCE(MAX(ID) + 1, 1) AS id FROM QA_TENDER_SETUP";
                    int id = _dbContext.SqlQuery<int>(idquery).FirstOrDefault();
                    string insertQuery = string.Format(@"INSERT INTO QA_TENDER_SETUP(ID, PREFIX, SUFFIX,BODY_LENGTH,STATUS,CREATED_DATE, CREATED_BY, COMPANY_CODE) 
                                 VALUES({0},'{1}','{2}',{3},'{4}', TO_DATE('{5}', 'DD-MON-YYYY'),'{6}','{7}')",
                                              id,data.PREFIX,data.SUFFIX,data.BODY_LENGTH,"E",
                                              DateTime.Now.ToString("dd-MMM-yyyy"),
                                              _workContext.CurrentUserinformation.login_code,
                                              _workContext.CurrentUserinformation.company_code
                                              );
                    _dbContext.ExecuteSqlCommand(insertQuery);
                }
                else
                {
                    string updateQuery = $@"UPDATE QA_TENDER_SETUP 
                       SET PREFIX = '{data.PREFIX}',SUFFIX='{data.SUFFIX}',BODY_LENGTH={data.BODY_LENGTH},
                           MODIFIED_DATE = '{DateTime.Now.ToString("dd-MMM-yyyy")}',
                           MODIFIED_BY = '{_workContext.CurrentUserinformation.login_code}',
                           COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'
                       WHERE id = '{data.ID}' ";
                    _dbContext.ExecuteSqlCommand(updateQuery);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<Tender> getTenderDetails()
        {
            try
            {
                string query = $@"SELECT * FROM QA_TENDER_SETUP WHERE STATUS='E' ORDER BY ID DESC";
                List<Tender> tenders = this._dbContext.SqlQuery<Tender>(query).ToList();
                return tenders;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public bool deleteTenderId(string id)
        {
            try
            {
                var UPDATE_QUERY = $@"UPDATE QA_TENDER_SETUP SET STATUS ='D' WHERE ID='{id}'";
                _dbContext.ExecuteSqlCommand(UPDATE_QUERY);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<Tender> getTenderById( string id)
        {
            try
            {
                string query = $@"SELECT * FROM QA_TENDER_SETUP WHERE STATUS='E' AND ID='{id}'";
                List<Tender> tenders = this._dbContext.SqlQuery<Tender>(query).ToList();
                return tenders;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<QuotationCount> GetQuotationCount()
        {
            try
            {
                string query = $@"SELECT COUNT(*) AS count, 'Quotation Request' AS heading, '#bbbbf0' AS color, 'fa fa-star fa-2x' AS icon, 1 AS sortOrder
                                FROM sa_quotation_setup
                                WHERE status = 'E'
                                UNION 
                                SELECT COUNT(*) AS count, 'Quotation Received ' AS heading, '#f0bbee' AS color, 'fa fa-asterisk' AS icon, 2 AS sortOrder
                                FROM quotation_details sps 
                                LEFT JOIN sa_quotation_setup ps ON (ps.tender_no = sps.tender_no)
                                WHERE  ps.status = 'E' 
                                UNION 
                                SELECT COUNT(DISTINCT sps.quotation_no) AS count, 'Quotation Approved ' AS heading, '#f0d19c' AS color, 'fa fa-spinner' AS icon, 3 AS sortOrder
                                 FROM quotation_details sps 
                                LEFT JOIN sa_quotation_setup ps ON (ps.tender_no = sps.tender_no)
                                WHERE  ps.status = 'E' and sps.status='AP'
                                UNION 
                                SELECT ((SELECT COUNT(*) FROM sa_quotation_setup WHERE status = 'E') - (SELECT COUNT(DISTINCT sps.tender_no) 
                                 FROM quotation_details sps WHERE sps.status = 'AP' AND sps.tender_no IN (SELECT tender_no FROM sa_quotation_setup WHERE status = 'E'))) AS count, 
                                'Quotation Open' AS heading,  '#d4fa93' AS color,'fa fa-opera' AS icon, 4 AS sortOrder from dual ORDER BY sortOrder";

                List<QuotationCount> entity = this._dbContext.SqlQuery<QuotationCount>(query).ToList();
                return entity;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<FormDetailSetup> GetFormDetailSetup()
        {
            string Query = $@"SELECT FDS.SERIAL_NO,
                            FS.FORM_EDESC,
                            FS.FORM_TYPE,
                            FS.NEGATIVE_STOCK_FLAG,
                           FDS.FORM_CODE,
                           FDS.TABLE_NAME,
                           FDS.COLUMN_NAME,
                           FDS.COLUMN_WIDTH,
                           FDS.COLUMN_HEADER,
                           FDS.TOP_POSITION,
                           FDS.LEFT_POSITION,
                           FDS.DISPLAY_FLAG,
                           FDS.DEFA_VALUE,
                           FDS.IS_DESC_FLAG,
                           FDS.MASTER_CHILD_FLAG,
                           FDS.FORM_CODE,
                           FDS.COMPANY_CODE,
                           CS.COMPANY_EDESC,
                            CS.TELEPHONE,
                            CS.EMAIL,
                            CS.TPIN_VAT_NO,
                            CS.ADDRESS,
                           FDS.CREATED_BY,
                           FDS.CREATED_DATE,
                           FDS.DELETED_FLAG,
                           FDS.FILTER_VALUE,
                           FDS.SYN_ROWID,
                           FDS.MODIFY_DATE,
                           FDS.MODIFY_BY,
                           FS.REFERENCE_FLAG,
                           FS.FREEZE_MASTER_REF_FLAG,
                           FS.REF_FIX_QUANTITY,
                           FS.REF_FIX_PRICE                          
                      FROM    FORM_DETAIL_SETUP FDS
                           LEFT JOIN
                              COMPANY_SETUP CS ON FDS.COMPANY_CODE = CS.COMPANY_CODE
                              LEFT JOIN FORM_SETUP FS
                               ON FDS.FORM_CODE = FS.FORM_CODE AND FDS.COMPANY_CODE = FS.COMPANY_CODE
                     WHERE FS.QUOTATION_FLAG='Y'  AND CS.COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'";
            List<FormDetailSetup> entity = this._dbContext.SqlQuery<FormDetailSetup>(Query).ToList();
            return entity;
        }
        public Inventory MapMasterColumnWithValue(string masterColumn)
        {
            try
            {
                var masterColVal = JsonConvert.DeserializeObject<Inventory>(masterColumn);
                return masterColVal;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        public List<Inventory> MapChildColumnWithValue(string childColumn)
        {
            try
            {
                var childColVal = JsonConvert.DeserializeObject<List<Inventory>>(childColumn);
                return childColVal;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        public bool SaveColumnValue(Inventory masterColumnValue, CommonFieldsForInventory commonValue)
        {
            try
            {
                bool insertedToMaster = false;
                var idquery = $@"SELECT COALESCE(MAX(ID) + 1, 1) AS id FROM sa_quotation_setup";
                int id = _dbContext.SqlQuery<int>(idquery).FirstOrDefault();
                string insertQuery = $@"INSERT INTO sa_quotation_setup(TENDER_NO,ISSUE_DATE,VALID_DATE, CREATED_DATE, CREATED_BY, COMPANY_CODE, 
                        STATUS, REMARKS, ID, APPROVED_STATUS, BRANCH_CODE,MANUAL_NO) VALUES('{commonValue.VoucherNumber}',
                         TO_DATE('{masterColumnValue.QUOTE_DATE}', 'DD-MON-YYYY'), 
                         TO_DATE('{masterColumnValue.TO_DELIVERED_DATE}', 'DD-MON-YYYY'), 
                         TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-YYYY'), '{_workContext.CurrentUserinformation.login_code}',
                        '{_workContext.CurrentUserinformation.company_code}', 'E', '{masterColumnValue.REMARKS}', {id}, 'N', '{_workContext.CurrentUserinformation.branch_code}','{masterColumnValue.MANUAL_NO}')";

                _dbContext.ExecuteSqlCommand(insertQuery);
                insertedToMaster = true;
                return insertedToMaster;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public bool UpdateColumnValue(Inventory masterColumnValue, CommonFieldsForInventory commonValue)
        {
            try
            {
                bool insertedToMaster = false;
                string updateQuery = $@"UPDATE sa_quotation_setup 
                       SET VALID_DATE = TO_DATE('{masterColumnValue.TO_DELIVERED_DATE}', 'DD-MON-YYYY'),
                           ISSUE_DATE = TO_DATE('{masterColumnValue.QUOTE_DATE}', 'DD-MON-YYYY'),
                           MODIFIED_DATE = '{DateTime.Now.ToString("dd-MMM-yyyy")}',REMARKS='{masterColumnValue.REMARKS}',
                           MODIFIED_BY = '{_workContext.CurrentUserinformation.login_code}',
                           COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}',MANUAL_NO='{masterColumnValue.MANUAL_NO}'
                       WHERE tender_no = '{commonValue.VoucherNumber}'";
                _dbContext.ExecuteSqlCommand(updateQuery);
                insertedToMaster = true;
                return insertedToMaster;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool SaveMasterColumnValue(Inventory masterColumnValue, CommonFieldsForInventory commonValue)
        {
            try
            {
                bool insertedToMaster = false;
                string fQuery = $@"SELECT form_code FROM form_setup where  quotation_flag = 'Y' and company_code='{_workContext.CurrentUserinformation.company_code}'";
                string formCode = _dbContext.SqlQuery<string>(fQuery).FirstOrDefault();
                string insertmasterQuery = string.Format(@"INSERT INTO MASTER_TRANSACTION(VOUCHER_NO,VOUCHER_AMOUNT,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,DELETED_FLAG,CURRENCY_CODE,CREATED_DATE,VOUCHER_DATE,SESSION_ROWID,SYN_ROWID,EXCHANGE_RATE,REFERENCE_NO) 
                     VALUES('{0}',{1},'{2}','{3}','{4}','{5}','{6}','{7}',{8},TO_DATE({9},'DD-MON-YY hh24:mi:ss'),'{10}','{11}',{12},'{13}')",
                    commonValue.VoucherNumber, 0, formCode, _workContext.CurrentUserinformation.company_code, _workContext.CurrentUserinformation.branch_code, _workContext.CurrentUserinformation.login_code.ToUpper(), 'N', commonValue.CurrencyFormat, "SYSDATE", commonValue.VoucherDate, masterColumnValue.MANUAL_NO, '1', commonValue.ExchangeRate, masterColumnValue.MANUAL_NO);
                _dbContext.ExecuteSqlCommand(insertmasterQuery);

                insertedToMaster = true;
                return insertedToMaster;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public bool UpdateMasterTransaction(CommonFieldsForInventory commonUpdateValue)
        {
            try
            {
                bool insertedToMaster = false;
                string query = $@"UPDATE MASTER_TRANSACTION SET VOUCHER_DATE={commonUpdateValue.VoucherDate},MODIFY_BY = '{_workContext.CurrentUserinformation.login_code.ToUpper()}',SYN_ROWID='{commonUpdateValue.ManualNumber}',REFERENCE_NO='{commonUpdateValue.ManualNumber}' , MODIFY_DATE = SYSDATE,CURRENCY_CODE='{commonUpdateValue.CurrencyFormat}',EXCHANGE_RATE={commonUpdateValue.ExchangeRate} where VOUCHER_NO='{commonUpdateValue.VoucherNumber}'  and COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'";
                _dbContext.ExecuteSqlCommand(query);
                insertedToMaster = true;
                return insertedToMaster;
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }

        public List<Inventory> GetMasterTransactionByVoucherNo(string voucherNumber)
        {
            try
            {
                //var getPrevDataQuery = $@"SELECT VOUCHER_NO,SESSION_ROWID, CREATED_BY, TO_CHAR(CREATED_DATE, 'DD-MON-YY hh12:mi:ss PM') as CREATED_DATE FROM MASTER_TRANSACTION WHERE VOUCHER_NO= '{voucherNumber}'"; //previously

                var getPrevDataQuery = $@"SELECT VOUCHER_NO,SESSION_ROWID, CREATED_BY, TO_CHAR(CREATED_DATE, 'DD-MON-YY') as CREATED_DATE FROM MASTER_TRANSACTION WHERE VOUCHER_NO= '{voucherNumber}'";
                var defaultData = _dbContext.SqlQuery<Inventory>(getPrevDataQuery).ToList();
                return defaultData;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        public List<Products> GetProductDataByProductCode(string productcode)
        {
            try
            {
                var productdata = new List<Products>();
                if (productcode != "")
                {
                    string query = $@"SELECT distinct  IT.ITEM_CODE AS ItemCode,IT.ITEM_EDESC AS ItemDescription,IT.INDEX_MU_CODE AS ItemUnit,
                    IT.MULTI_MU_CODE AS MultiItemUnit,IC.CATEGORY_EDESC  AS Category,COALESCE(iiss.ITEM_SPECIFICATION,' ') as SPECIFICATION
                    ,COALESCE(iiss.BRAND_NAME,' ') as BRAND_NAME,COALESCE(iiss.INTERFACE,' ') as INTERFACE,COALESCE(iiss.TYPE,' ') as TYP
                    ,COALESCE(iiss.LAMINATION,' ') as LAMINATION ,COALESCE(iiss.ITEM_SIZE,' ') as ITEM_SIZE,COALESCE(iiss.THICKNESS,' ') as THICKNESS,COALESCE(iiss.COLOR,' ') as COLOR
                     ,COALESCE(iiss.GRADE,' ') as GRADE  ,COALESCE(iiss.SIZE_LENGHT,0) as SIZE_LENGHT,COALESCE(iiss.SIZE_WIDTH,0) as SIZE_WIDTH
                     FROM ip_item_master_setup IT, IP_CATEGORY_CODE IC,IP_ITEM_SPEC_SETUP iiss WHERE IT.deleted_flag = 'N' and IT.item_code=iiss.item_code and IT.company_code=iiss.company_code
                     AND IT.CATEGORY_CODE= IC.CATEGORY_CODE AND IT.COMPANY_CODE = IC.COMPANY_CODE AND IT.COMPANY_CODE ='{_workContext.CurrentUserinformation.company_code}' AND IT.item_code ='{productcode}'";
                    productdata = this._dbContext.SqlQuery<Products>(query).ToList();
                    return productdata;
                }
                else
                { return productdata; }

            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<Category> GetCategoryList()
        {
            try
            {
                string query = $@"SELECT DISTINCT CATEGORY_CODE AS CATEGORY,CATEGORY_EDESC  FROM IP_CATEGORY_CODE WHERE DELETED_FLAG = 'N' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'";
                List<Category> categories = this._dbContext.SqlQuery<Category>(query).ToList();
                return categories;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public string CheckVoucherNoReferenced(string voucherno)
        {
            try
            {
                var Count_query = $@"select voucher_no from reference_detail where reference_no='{voucherno}'
                          AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'";
                string Result = _dbContext.SqlQuery<string>(Count_query).FirstOrDefault();
                return Result;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public bool deletevouchernoInv(string voucherno)
        {
                try
                {
                    var deletmaintableequery = $@"UPDATE sa_quotation_setup SET STATUS='D',MODIFIED_DATE=SYSDATE,MODIFIED_BY='{_workContext.CurrentUserinformation.login_code
                        }' WHERE tender_no='{voucherno}' AND COMPANY_CODE='{_workContext.CurrentUserinformation.Company}'";
                    var maintablerowCount = _dbContext.ExecuteSqlCommand(deletmaintableequery);

                    if (maintablerowCount > 0)
                    {
                        var deletemastertable = $@"UPDATE MASTER_TRANSACTION SET DELETED_FLAG='Y', MODIFY_DATE=SYSDATE,MODIFY_BY='{_workContext.CurrentUserinformation.login_code}' WHERE VOUCHER_NO='{voucherno}' AND COMPANY_CODE='{_workContext.CurrentUserinformation.Company}'";
                         _dbContext.ExecuteSqlCommand(deletemastertable);
                    }
                    return true;
                }

                catch (Exception ex)
                {
                    throw ex;
                }
        }
        public List<COMMON_COLUMN> GetQuestOrderFormDetail(string voucherNo)
        {
            string fQuery = $@"SELECT form_code FROM form_setup where  quotation_flag = 'Y' and company_code='{_workContext.CurrentUserinformation.company_code}'";
            string formCode = _dbContext.SqlQuery<string>(fQuery).FirstOrDefault();

            string columname = $@"SELECT COLUMN_NAME, TABLE_NAME FROM FORM_DETAIL_SETUP WHERE FORM_CODE='{formCode}' and company_code='{_workContext.CurrentUserinformation.company_code}' ORDER BY SERIAL_NO ASC";
            List<FORM_DETAIL_SETUP_COLUMN> columnameentity = this._dbContext.SqlQuery<FORM_DETAIL_SETUP_COLUMN>(columname).ToList();
            var tableName = "";
            List<string> columns = new List<string>();
            StringBuilder sb = new StringBuilder();
            var column = sb.ToString().TrimEnd(',');
            //tableName = columnameentity[0].TABLE_NAME;
            string Query = string.Empty;
            StringBuilder condition = new StringBuilder();
            foreach (var item in columnameentity)
            {
                columns.Add($"{item.COLUMN_NAME}");
            }
            //if (columns.Contains("QUOTE_NO"))
            //{
            //    column ="SQS.TENDER_NO AS QUOTE_NO,SQS.ISSUE_DATE AS QUOTE_DATE,SQS.VALID_DATE AS TO_DELIVERED_DATE,SQS.MANUAL_NO";
            //    tableName ="SA_QUOTATION_SETUP SQS";
            //    condition.Append("AND SQS.STATUS='E'");
            //    Query = $@"SELECT {column} FROM {tableName} WHERE SQS.COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' and SQS.TENDER_NO='{voucherNo}' {condition.ToString()}";
            //}
            //if (columns.Contains("ITEM_CODE"))
            //{
            //    column = column + ",IMS.ITEM_EDESC,SQI.ITEM_CODE,SQI.SPECIFICATION,SQI.UNIT AS MU_CODE,SQI.QUANTITY,SQI.BRAND_NAME,SQI.INTERFACE,SQI.TYPE,SQI.LAMINATION,SQI.ITEM_SIZE,SQI.THICKNESS,SQI.IMAGE,SQI.REMARKS," +
            //        "SQI.COLOR,SQI.GRADE,SQI.SIZE_LENGTH,SQI.SIZE_WIDTH,SQI.ID";
            //    tableName = tableName + ", IP_ITEM_MASTER_SETUP IMS,SA_QUOTATION_ITEMS SQI";
            //    condition.Append("AND SQI.ITEM_CODE=IMS.ITEM_CODE AND SQS.COMPANY_CODE=IMS.COMPANY_CODE AND  IMS.DELETED_FLAG='N' AND SQI.DELETED_FLAG='N' AND SQS.TENDER_NO=SQI.TENDER_NO");
            //    Query = $@"SELECT {column} FROM {tableName} WHERE SQS.COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' and SQS.TENDER_NO='{voucherNo}' {condition.ToString()}";
            //}
            //if (columns.Contains("CATEGORY"))
            //{
            //    column = column + ",ICC.CATEGORY_EDESC AS CATEGORY";
            //    tableName = tableName + ",IP_CATEGORY_CODE ICC";
            //    condition.Append(" AND ICC.CATEGORY_CODE=SQI.CATEGORY AND ICC.DELETED_FLAG='N' AND IMS.COMPANY_CODE=ICC.COMPANY_CODE");
            //    Query = $@"SELECT {column} FROM {tableName} WHERE SQS.COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' and SQS.TENDER_NO='{voucherNo}' {condition.ToString()}";
            //}
            Query = $@"SELECT sqs.tender_no AS quote_no, sqs.issue_date AS quote_date,sqs.valid_date AS to_delivered_date,sqi.id,sqs.remarks,
    sqs.manual_no, ims.item_edesc,sqi.item_code, sqi.specification,sqi.unit AS mu_code,sqi.quantity, sqi.brand_name,sqi.interface,
    sqi.type,sqi.lamination,sqi.item_size,sqi.thickness,sqi.image,sqi.remarks,sqi.color,sqi.grade,sqi.size_length,sqi.size_width,sqi.id,
    icc.category_edesc   AS category FROM sa_quotation_setup     sqs JOIN sa_quotation_items sqi ON sqs.tender_no = sqi.tender_no
    JOIN ip_item_master_setup ims ON sqi.item_code = ims.item_code  AND sqs.company_code = ims.company_code AND ims.deleted_flag = 'N'
    LEFT JOIN ip_category_code icc ON icc.category_code = sqi.category AND ims.company_code = icc.company_code AND icc.deleted_flag = 'N'
    WHERE  sqs.company_code = '{_workContext.CurrentUserinformation.company_code}' AND sqs.tender_no = '{voucherNo}' AND sqs.status = 'E' AND sqi.deleted_flag = 'N' order by sqi.id ";
            var entity = this._dbContext.SqlQuery<COMMON_COLUMN>(Query).ToList();
            return entity;
        }
    }
}
