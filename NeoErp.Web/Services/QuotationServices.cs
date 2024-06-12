using NeoErp.Data;
using NeoErp.Models.QueryBuilder;
using System;
using NeoErp.Core;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NeoErp.Core.Quotation;
using System.IO;

namespace NeoErp.Services
{
    public class QuotationServices: IQuotation
    {
        IWorkContext _workContext;
        private IDbContext _dbContext;
        public QuotationServices(IDbContext dbContext, IWorkContext workContext)
        {
            this._workContext = workContext;
            _dbContext = dbContext;
        }

        public List<Quotation> GetQuotationDetails(string id)
        {
            try
            {
                // Fetch project data
                string Query = $@"SELECT TENDER_NO, ISSUE_DATE, VALID_DATE, CREATED_DATE, CREATED_BY,bs_date(ISSUE_DATE) as NEPALI_DATE,bs_date(VALID_DATE) as DELIVERY_DT_BS, COMPANY_CODE,BRANCH_CODE FROM sa_quotation_setup WHERE ID = '{id}' AND STATUS = 'E'";
                List<Quotation> quotations = _dbContext.SqlQuery<Quotation>(Query).ToList();
                foreach (var quotation in quotations)
                {
                    string subQuery = $@"select sqi.id,iims.Item_code as ITEM_CODE,iims.Item_edesc as ITEM_DESC,sqi.SPECIFICATION,sqi.IMAGE,sqi.UNIT,sqi.QUANTITY,sqi.CATEGORY,sqi.BRAND_NAME,
                    sqi.INTERFACE,sqi.TYPE,sqi.LAMINATION,sqi.ITEM_SIZE,sqi.THICKNESS,sqi.COLOR,sqi.GRADE,sqi.SIZE_LENGTH,sqi.SIZE_WIDTH 
                    from sa_quotation_items sqi,ip_item_master_setup iims,sa_quotation_setup sqs where (sqi.item_code=iims.item_code) and 
                    (sqs.tender_no=sqi.tender_no) and (sqs.company_code=iims.company_code) and sqi.tender_no='{quotation.TENDER_NO}' and sqi.deleted_flag='N' order by sqi.id";
                    List<Item> itemData = this._dbContext.SqlQuery<Item>(subQuery).ToList();
                    quotation.Items = itemData;
                }
                return quotations;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<Supplier> GetSupplierDetails(string panNo)
        {
            try
            {
                List<Supplier> supList = new List<Supplier>();

                string query = $@"select SUPPLIER_CODE,SUPPLIER_EDESC ,Email,regd_office_eaddress as ADDRESS,tel_mobile_no1 as CONTACT_NO from ip_supplier_setup where tpin_vat_no='{panNo}'";
                supList = this._dbContext.SqlQuery<Supplier>(query).ToList();
                return supList;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<Company> GetCompanyDetails(string id)
        {
            try
            {
                List<Company> ProductsList = new List<Company>();

                string query = $@"select cs.COMPANY_CODE,cs.COMPANY_EDESC,cs.ADDRESS,cs.EMAIL,cs.LOGO_FILE_NAME from COMPANY_SETUP cs,
                                sa_quotation_setup sqs where cs.company_code=sqs.company_code and sqs.id='{id}'";
                ProductsList = this._dbContext.SqlQuery<Company>(query).ToList();
                return ProductsList;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public int? InsertQuotationDetails(Quotation_Details data)
        {
            try
            { 
                int quotationNo=0;
                var newmaxitemcode = "null";
                var maxPreCode = string.Empty;
                var maxSupCOde = string.Empty;
                var newmasteracccode = string.Empty;
                if (!string.IsNullOrEmpty(data.PAN_NO)) {
                    var query = $@"select count(*) from IP_SUPPLIER_SETUP where TPIN_VAT_NO='{data.PAN_NO}'";
                    int count = _dbContext.SqlQuery<int>(query).FirstOrDefault();
                    if (count == 0)
                    {

                        List<Supplier>supplierDetail = checkSupplier();
                        if (supplierDetail.Count==0)
                        {
                            var newmaxitemcodequery = $@"select NVL(MAX(TO_NUMBER(REGEXP_SUBSTR(TO_NUMBER(SUPPLIER_CODE), '[^.]+', 1, 1))),0)+1 as MASTER_SUPPLIER_CODE from IP_SUPPLIER_SETUP  WHERE COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'";
                            newmaxitemcode = _dbContext.SqlQuery<int>(newmaxitemcodequery).FirstOrDefault().ToString();
                            var maxprecodequery = $@"SELECT NVL(MAX(substr(MASTER_SUPPLIER_CODE,-instr(reverse(MASTER_SUPPLIER_CODE),'.')+1))+1,0) as MAXCODE FROM IP_SUPPLIER_SETUP 
                                                         WHERE company_code = '{_workContext.CurrentUserinformation.company_code}'";
                            var maxCode = _dbContext.SqlQuery<int>(maxprecodequery).FirstOrDefault();
                            maxPreCode = maxCode == 0 ? "1" : maxCode.ToString();
                            if (maxPreCode == null)
                                maxPreCode = "1";
                            if (Convert.ToInt32(maxPreCode) <= 9)
                            {
                                maxPreCode = "0" + maxPreCode.ToString();
                            }
                            var supsetupquery = $@"Insert into IP_SUPPLIER_SETUP 
                        (SUPPLIER_CODE,SUPPLIER_EDESC,SUPPLIER_NDESC,REGD_OFFICE_EADDRESS,REGD_OFFICE_NADDRESS,TEL_MOBILE_NO1,TEL_MOBILE_NO2,
                        FAX_NO,EMAIL,PARTY_TYPE_CODE,LINK_SUB_CODE,REMARKS,ACTIVE_FLAG,GROUP_SKU_FLAG,MASTER_SUPPLIER_CODE,PRE_SUPPLIER_CODE,
                        COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,CREDIT_DAYS,CREDIT_ACTION_FLAG,ACC_CODE,PR_CODE,TPIN_VAT_NO,SYN_ROWID,
                        CREDIT_LIMIT,MODIFY_DATE,BRANCH_CODE,MODIFY_BY,M_DAYS,OPENING_DATE,APPROVED_FLAG,SUBSTITUTE_NAME,MATURITY_DATE,IMAGE_FILE_NAME,
                        INTEREST_RATE,CASH_SUPPLIER_FLAG,SUPPLIER_ID,GROUP_START_NO,PREFIX_TEXT,DELTA_FLAG,EXCISE_NO,TIN,TDS_CODE) 
                        values ('{newmaxitemcode}','Quotation Supplier','Quotation Supplier',null,null,null,null,null,null,null,
                        null,null,'Y','G','{maxCode}','00','{data.COMPANY_CODE}','{data.CREATED_BY}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),null,
                        null,null,null,null,null,null,null,null,null,
                        null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null)";
                          _dbContext.ExecuteSqlCommand(supsetupquery);
                        }

                        List<Supplier> supDetail = checkSupplier();
                            var newmaxquery = $@"select NVL(MAX(TO_NUMBER(REGEXP_SUBSTR(TO_NUMBER(SUPPLIER_CODE), '[^.]+', 1, 1))),0)+1 as MASTER_SUPPLIER_CODE from IP_SUPPLIER_SETUP  WHERE COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'";
                            newmaxitemcode = _dbContext.SqlQuery<int>(newmaxquery).FirstOrDefault().ToString();
                            var maxCOde = supDetail[0].MASTER_SUPPLIER_CODE + "." + "00";
                            var setupquery = $@"INSERT INTO ip_supplier_setup (supplier_code,supplier_edesc,supplier_ndesc,regd_office_eaddress,regd_office_naddress,tel_mobile_no1,tel_mobile_no2,fax_no,
                            email,party_type_code,link_sub_code,remarks,active_flag,group_sku_flag,master_supplier_code,pre_supplier_code,company_code,created_by,created_date,
                            deleted_flag,credit_days,credit_action_flag,acc_code,pr_code,tpin_vat_no,syn_rowid,credit_limit,modify_date,branch_code,modify_by,m_days,opening_date,
                            approved_flag,substitute_name,maturity_date,image_file_name,interest_rate,cash_supplier_flag,supplier_id,group_start_no,prefix_text,delta_flag,excise_no,tin,tds_code)
                            VALUES ('{newmaxitemcode}','{data.PARTY_NAME}','{data.PARTY_NAME}','{data.ADDRESS}','{data.ADDRESS}','{data.CONTACT_NO}',NULL,NULL,'{data.EMAIL}',NULL,NULL,NULL,'Y',
                            'I','{maxCOde}','{supDetail[0].MASTER_SUPPLIER_CODE}','{data.COMPANY_CODE}','{data.CREATED_BY}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}', 'MM/DD/YYYY'),
                            'N',0,NULL,NULL,NULL,'{data.PAN_NO}',NULL,0,NULL,NULL,NULL,NULL,null,NULL,NULL,NULL,NULL,null,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL)";
                            _dbContext.ExecuteSqlCommand(setupquery);
                    }
                }
                    var idquery = $@"SELECT COALESCE(MAX(QUOTATION_NO) + 1, 1) AS id FROM QUOTATION_DETAILS";
                     quotationNo = _dbContext.SqlQuery<int>(idquery).FirstOrDefault();

                    string insertQuery = string.Format(@"INSERT INTO QUOTATION_DETAILS(QUOTATION_NO, TENDER_NO, PAN_NO, supplier_code,CURRENCY, CURRENCY_RATE, DELIVERY_DATE, TOTAL_AMOUNT, TOTAL_DISCOUNT, TOTAL_EXCISE, TOTAL_TAXABLE_AMOUNT, TOTAL_VAT, TOTAL_NET_AMOUNT, STATUS, DISCOUNT_TYPE,REVISE) 
                                             VALUES({0}, '{1}', '{2}', '{3}', '{4}', {5}, TO_DATE('{6}', 'DD-MON-YYYY'), {7}, {8}, {9}, {10}, {11}, {12}, 'RQ', '{13}','{14}')",
                                                         quotationNo, data.TENDER_NO, data.PAN_NO,string.IsNullOrEmpty(data.CUSTOMER_CODE) ? newmaxitemcode : data.CUSTOMER_CODE, data.CURRENCY, data.CURRENCY_RATE,
                                                         data.DELIVERY_DATE.HasValue ? data.DELIVERY_DATE.Value.ToString("dd-MMM-yyyy") : "NULL", data.TOTAL_AMOUNT, data.TOTAL_DISCOUNT,
                                                         data.TOTAL_EXCISE, data.TOTAL_TAXABLE_AMOUNT, data.TOTAL_VAT, data.TOTAL_NET_AMOUNT, data.DISCOUNT_TYPE,data.REVISE);

                    _dbContext.ExecuteSqlCommand(insertQuery);

                    List<Item_details> itemDetails = data.Item_Detail;
                    if (itemDetails != null)
                    {
                        foreach (var item in itemDetails)
                        {
                            InsertItemDetails(item, quotationNo);
                        }
                    }

                    List<Term_Conditions> termConditions = data.TermsCondition;
                    if (termConditions != null)
                    {
                        foreach (var term in termConditions)
                        {
                            InsertTermConditions(term, quotationNo, data.TENDER_NO);
                        }
                    }
                return quotationNo;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool InsertItemDetails(Item_details itemDetail, int quotationId)
        {
            try
            {
                var idquery = $@"SELECT COALESCE(MAX(ID) + 1, 1) AS id FROM QUOTATION_DETAIL_ITEMWISE";
                int id = _dbContext.SqlQuery<int>(idquery).FirstOrDefault();
                string insertItemQuery = string.Format(@"INSERT INTO QUOTATION_DETAIL_ITEMWISE (ID,QUOTATION_NO, ITEM_CODE, RATE, AMOUNT, DISCOUNT, DISCOUNT_AMOUNT, EXCISE, TAXABLE_AMOUNT, VAT_AMOUNT, NET_AMOUNT) 
                             VALUES({0}, {1}, '{2}', {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10})",
                                     id, quotationId, itemDetail.ITEM_CODE, itemDetail.RATE, itemDetail.AMOUNT, itemDetail.DISCOUNT, itemDetail.DISCOUNT_AMOUNT, itemDetail.EXCISE, itemDetail.TAXABLE_AMOUNT, itemDetail.VAT_AMOUNT, itemDetail.NET_AMOUNT);

                _dbContext.ExecuteSqlCommand(insertItemQuery);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<Supplier> checkSupplier()
        {
            try
            {
                var checkQuery = $@"select * from IP_SUPPLIER_SETUP where supplier_edesc='Quotation Supplier'";
                List<Supplier> supplierDetail = _dbContext.SqlQuery<Supplier>(checkQuery).ToList();
                return supplierDetail;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public bool InsertTermConditions(Term_Conditions termDetail, int quotationId,string tenderNo)
        {
            try
            {
                var idquery = $@"SELECT COALESCE(MAX(ID) + 1, 1) AS id FROM QUOTATION_TERM_CONDITION";
                int id = _dbContext.SqlQuery<int>(idquery).FirstOrDefault();
                string insertItemQuery = string.Format(@"INSERT INTO QUOTATION_TERM_CONDITION (ID,TENDER_NO,QUOTATION_NO,TERM_CONDITION) 
                             VALUES({0}, '{1}', {2}, '{3}')",
                                     id, tenderNo, quotationId,termDetail.TERM_CONDITION);
                _dbContext.ExecuteSqlCommand(insertItemQuery);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public int? DocumentIfExists(string tenderNo, string quotationNo)
        {
            string query = $@"SELECT MAX(SERIAL_NO) FROM QUOTATION_TRANSACTION WHERE TENDER_NO = '{tenderNo}' AND QUOTATION_NO = '{quotationNo}' AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'";

            int? result = _dbContext.SqlQuery<int?>(query).FirstOrDefault();
            return result;
        }
        public string InsertQuotationImage(QuotationTranscation quotationdetail)
        {
            var insertitem = $@"INSERT INTO QUOTATION_TRANSACTION(TENDER_NO,SERIAL_NO,QUOTATION_NO,DOCUMENT_NAME,DOCUMENT_FILE_NAME,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SESSION_ROWID,SYN_ROWID)VALUES('{quotationdetail.TENDER_NO}', '{quotationdetail.SERIAL_NO}','{quotationdetail.QUOTATION_NO}','{quotationdetail.DOCUMENT_FILE_NAME}', '{quotationdetail.DOCUMENT_NAME}','{_workContext.CurrentUserinformation.company_code}', '{_workContext.CurrentUserinformation.branch_code}','{_workContext.CurrentUserinformation.User_id}',SYSDATE,'{'N'}','','')";
            var iteminsert = _dbContext.ExecuteSqlCommand(insertitem);
            return null;
        }
        public int? GetSupplierCount(string panNo,string tenderNo)
        {
            string query = $@"SELECT COUNT(*) AS MAX FROM QUOTATION_DETAILS WHERE PAN_NO='{panNo}' AND TENDER_NO='{tenderNo}'";
            int? result = _dbContext.SqlQuery<int?>(query).FirstOrDefault();
            return result;
        }
    }
}