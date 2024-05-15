using NeoErp.Data;
using NeoErp.Models.QueryBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NeoErp.Core.Quotation;
using System.IO;

namespace NeoErp.Services
{
    public class QuotationServices: IQuotation
    {
        private IDbContext _dbContext;
        public QuotationServices(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<Quotation> GetQuotationDetails(string id)
        {
            try
            {
                // Fetch project data
                string Query = $@"SELECT * FROM sa_quotation_setup WHERE ID = '{id}' AND STATUS = 'E'";
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
        public List<Employee> GetEmployeeDetails(string panNo)
        {
            try
            {
                List<Employee> ProductsList = new List<Employee>();

                string query = $@"select customer_code as EMPLOYEE_CODE,customer_edesc as EMPLOYEE_EDESC,Email,regd_office_eaddress as ADDRESS,tel_mobile_no1 as CONTACT_NO from sa_customer_setup where pan_no='{panNo}'";
                ProductsList = this._dbContext.SqlQuery<Employee>(query).ToList();
                return ProductsList;
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
        public bool InsertQuotationDetails(Quotation_Details data)
        {
            try
            {
                var idquery = $@"SELECT COALESCE(MAX(QUOTATION_NO) + 1, 1) AS id FROM QUOTATION_DETAILS";
                int id = _dbContext.SqlQuery<int>(idquery).FirstOrDefault();
                string insertQuery = string.Format(@"INSERT INTO QUOTATION_DETAILS(QUOTATION_NO, TENDER_NO, PAN_NO, PARTY_NAME, ADDRESS, CONTACT_NO, EMAIL, CURRENCY, CURRENCY_RATE, DELIVERY_DATE, TOTAL_AMOUNT, TOTAL_DISCOUNT, TOTAL_EXCISE, TOTAL_TAXABLE_AMOUNT, TOTAL_VAT, TOTAL_NET_AMOUNT, STATUS,TERM_CONDITION) 
                                     VALUES({0}, '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', {8}, TO_DATE('{9}', 'DD-MON-YYYY'), {10}, {11}, {12}, {13}, {14}, {15}, 'RQ','{16}')",
                                     id, data.TENDER_NO, data.PAN_NO, data.PARTY_NAME, data.ADDRESS, data.CONTACT_NO, data.EMAIL, data.CURRENCY, data.CURRENCY_RATE,
                                     data.DELIVERY_DATE.HasValue ? data.DELIVERY_DATE.Value.ToString("dd-MMM-yyyy") : "NULL", data.TOTAL_AMOUNT, data.TOTAL_DISCOUNT,
                                     data.TOTAL_EXCISE, data.TOTAL_TAXABLE_AMOUNT, data.TOTAL_VAT, data.TOTAL_NET_AMOUNT,data.TERM_CONDITION);

                _dbContext.ExecuteSqlCommand(insertQuery);
                List<Item_details> itemDetails = data.Item_Detail;
                if (itemDetails != null)
                {
                    foreach (var item in itemDetails)
                    {
                        InsertItemDetails(item, id); 
                    }
                }
                List<Term_Conditions> termConditions = data.TermsCondition;
                if (termConditions != null)
                {
                    foreach (var term in termConditions)
                    {
                        InsertTermConditions(term, id,data.TENDER_NO); 
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
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
                return false;
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
                return false;
            }
        }
    }
}