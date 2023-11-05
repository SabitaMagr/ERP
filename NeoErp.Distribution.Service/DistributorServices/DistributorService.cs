using NeoErp.Core.Domain;
using NeoErp.Core.Helpers;
using NeoErp.Core.Models;
using NeoErp.Core.Services;
using NeoErp.Distribution.Service.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoErp.Distribution.Service.DistributorServices
{
    public class DistributorService : IDistributorService
    {
        private NeoErpCoreEntity _objectEntity;
        private IControlService _controlService;
        public DistributorService(NeoErpCoreEntity objectEntity, IControlService controlService)
        {
            this._objectEntity = objectEntity;
            this._controlService = controlService;
        }
        public User DistributorLogin(User model)
        {
            var userQuery = $@"SELECT LU.USERID USER_ID, LU.USER_NAME USERNAME, LU.PASS_WORD PASSWORD, TRIM(CS.CUSTOMER_EDESC) LOGIN_EDESC, LU.COMPANY_CODE COMPANY, LU.BRANCH_CODE BRANCH, LU.SP_CODE DISTRIBUTERNO, 'DIS' USERTYPE
FROM DIST_LOGIN_USER LU
INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = LU.SP_CODE AND CS.COMPANY_CODE = LU.COMPANY_CODE
WHERE 1 = 1
AND UPPER(LU.USER_NAME)=UPPER('{model.UserName}')
AND LU.PASS_WORD='{model.Password}'
AND TRIM(LU.USER_TYPE) IN( 'D','L')";
            var user = this._objectEntity.SqlQuery<User>(userQuery).ToList();
            return user.FirstOrDefault();
        }

        public string SavePurchaseOrder(DistPurchaseOrder model, User userInfo)
        {
            var userType = string.Empty;
            var partyTypeCode = string.Empty;
            var orderFrom = string.Empty;
            var customerCode = string.Empty;
            var billingCustomerCode = string.Empty;
            var billingCustomerName = string.Empty;
            var queryUserType = $@"SELECT USER_TYPE FROM DIST_LOGIN_USER WHERE USERID='{userInfo.User_id}' AND ACTIVE='Y' AND COMPANY_CODE='{userInfo.company_code}'";
            userType = _objectEntity.SqlQuery<string>(queryUserType).FirstOrDefault();

            if (userType == "L")
            {
                var queryPartyTypeCode = $@"SELECT PARTY_TYPE_CODE FROM FA_SUB_LEDGER_DEALER_MAP WHERE CUSTOMER_CODE='{model.CUSTOMER_CODE}' AND DELETED_FLAG='N' AND COMPANY_CODE='{userInfo.company_code}'";
                partyTypeCode = _objectEntity.SqlQuery<string>(queryPartyTypeCode).FirstOrDefault();
                customerCode = userInfo.DistributerNo;
                billingCustomerCode = model.CUSTOMER_CODE;
                billingCustomerName = model.CUSTOMER_EDESC;
                orderFrom = "L";
            }
            else
            {
                customerCode = userInfo.DistributerNo;
                billingCustomerCode = null;
                billingCustomerName = userInfo.LOGIN_EDESC;
                orderFrom = "O";
                partyTypeCode = null;
            }
            var distPOArray = model.selectedPOArray;
            string query = @"SELECT NVL(MAX(TO_NUMBER(ORDER_NO))+1, 1) MAXID FROM DIST_IP_SSD_PURCHASE_ORDER";
            var data = _objectEntity.SqlQuery<DistPurchaseOrder>(query).ToList();
            var ORDER_NO = data[0].MAXID;
            try
            {
                for (int i = 0; i < distPOArray.Count(); i++)
                {
                    var insertQuery1 = $@"INSERT INTO DIST_IP_SSD_PURCHASE_ORDER
   (ORDER_NO,ORDER_DATE,CUSTOMER_CODE,ITEM_CODE,MU_CODE,QUANTITY,UNIT_PRICE,TOTAL_PRICE,CREATED_BY,CREATED_DATE,COMPANY_CODE,BRANCH_CODE,REMARKS,ORDER_FROM,PARTY_TYPE_CODE,BILLING_CUSTOMER_CODE,BILLING_NAME,APPROVED_FLAG,DISPATCH_FLAG,ACKNOWLEDGE_FLAG,REJECT_FLAG,DELETED_FLAG)
   VALUES('{ORDER_NO}',SYSDATE,'{billingCustomerCode}','{distPOArray[i].selectedItems[0]}','{distPOArray[i].reqUnit}','{distPOArray[i].reqQuantity}','{distPOArray[i].unitPrice}','{distPOArray[i].totalPrice}','{userInfo.User_id}',SYSDATE,'{userInfo.company_code}','{userInfo.branch_code}','{model.Remarks}','{orderFrom}','{partyTypeCode}','{customerCode}','{billingCustomerName}','N','N','N','N','N')";
                    _objectEntity.ExecuteSqlCommand(insertQuery1);
                    _objectEntity.SaveChanges();

                    //var insertQuery = $@"INSERT INTO DIST_IP_SSD_PURCHASE_ORDER(ORDER_NO,ORDER_DATE,CUSTOMER_CODE,ITEM_CODE,MU_CODE,QUANTITY,BILLING_NAME,REMARKS,UNIT_PRICE,TOTAL_PRICE,CREATED_BY,CREATED_DATE,APPROVED_FLAG,DISPATCH_FLAG, ACKNOWLEDGE_FLAG, REJECT_FLAG, DELETED_FLAG, PARTY_TYPE_CODE, CITY_CODE, SALES_TYPE_CODE, SHIPPING_CONTACT, COMPANY_CODE, BRANCH_CODE, SYNC_ID, TEMP_ORDER_NO)
                    //        VALUES('{ORDER_NO}',SYSDATE,'{userInfo.DistributerNo}','{distPOArray[i].selectedItems[0]}','{distPOArray[i].reqUnit}','{item.quantity}','{item.billing_name}','{item.remarks}','{item.rate}','{total}','{model.user_id}',{ model.Saved_Date},'N','N','N','N','N','{item.party_type_code}','{item.Po_Shipping_Address}','{item.Po_Sales_Type}','{item.Po_Shipping_Contact}','{model.COMPANY_CODE}','{model.BRANCH_CODE}','{item.Sync_Id}','{model.Order_No}')";
                    //_objectEntity.ExecuteSqlCommand(insertQuery);
                    //_objectEntity.SaveChanges();
                }
                return "Purchase order successfully created";
            }
            catch (Exception e)
            {
                return "Something Went Wrong" + e.Message;
            }

        }
        public List<SalesRegisterProductModel> GetDistributorItems(User userInfo, string distributorCode)
        {
            if (userInfo.LoginType.ToUpper() == "DISTRIBUTOR")
                distributorCode = userInfo.DistributerNo;
            string SalesRateClause = string.Empty;
            string ConversionClause = string.Empty;
            var rateTableQuery = "";
            var rateCol = "";
            var distributerFilter = distributorCode == "" ? "" : $"AND CS_CODE = '{userInfo.DistributerNo}'";

            var PO = _objectEntity.SqlQuery<PreferenceSetupModel>($"SELECT * FROM DIST_PREFERENCE_SETUP WHERE COMPANY_CODE='{userInfo.company_code}'").FirstOrDefault();
            if (PO.PO_SYN_RATE == "Y")
                SalesRateClause = "AND SALES_RATE IS NOT NULL AND SALES_RATE <> 0";
            if (PO.SQL_NN_CONVERSION_UNIT_FACTOR == "Y")
                ConversionClause = "AND IUS.MU_CODE IS NOT NULL AND IUS.CONVERSION_FACTOR IS NOT NULL";
            rateCol = (string.IsNullOrEmpty(PO.PO_DIST_RATE_COLUMN) || PO.PO_DIST_RATE_COLUMN == "SALES_RATE") ? "STANDARD_RATE" : PO.PO_DIST_RATE_COLUMN;
            if (PO.PO_RATE_TABLE == "IP_ITEM_RATE_SCHEDULE_SETUP")
                rateTableQuery = $@"SELECT distinct A.CS_CODE,A.COMPANY_CODE,A.ITEM_CODE,A.MU_CODE,TO_CHAR(NVL(A.{rateCol},0)) SALES_RATE,
                                    A.EFFECTIVE_DATE APPLY_DATE,A.MRP_RATE FROM IP_ITEM_RATE_SCHEDULE_SETUP A
                              WHERE EFFECTIVE_DATE = (SELECT MAX(TO_DATE(EFFECTIVE_DATE)) FROM IP_ITEM_RATE_SCHEDULE_SETUP 
                                        WHERE ITEM_CODE = A.ITEM_CODE 
                                        AND CS_CODE=A.CS_CODE
                                        AND COMPANY_CODE = A.COMPANY_CODE )
                              {distributerFilter}
                              AND COMPANY_CODE = '{userInfo.company_code}'
                              ORDER BY ITEM_CODE,cs_code";
            else
                rateTableQuery = $@"SELECT A.ITEM_CODE, A.APPLY_DATE, B.SALES_RATE, B.COMPANY_CODE
                              FROM (SELECT ITEM_CODE, COMPANY_CODE, MAX(APP_DATE) APPLY_DATE 
                                FROM IP_ITEM_RATE_APPLICAT_SETUP
                                WHERE COMPANY_CODE = '{userInfo.company_code}' 
                                AND BRANCH_CODE = '{userInfo.branch_code}'
                                GROUP BY ITEM_CODE, COMPANY_CODE) A
                              INNER JOIN IP_ITEM_RATE_APPLICAT_SETUP B
                                ON B.ITEM_CODE = A.ITEM_CODE
                                AND B.APP_DATE = A.APPLY_DATE
                                AND B.COMPANY_CODE = '{userInfo.company_code}'
                                AND B.BRANCH_CODE = '{userInfo.branch_code}'";

            if (PO.PO_DISPLAY_DIST_ITEM == "Y")
            {
                string query = $@"SELECT DISTINCT IM.ITEM_CODE, IM.ITEM_EDESC, ISS.BRAND_NAME, IM.INDEX_MU_CODE AS UNIT, MC.MU_EDESC, IUS.MU_CODE CONVERSION_UNIT,
                TO_CHAR(IUS.CONVERSION_FACTOR) AS CONVERSION_FACTOR, TO_CHAR(NVL(IR.SALES_RATE, 0)) SALES_RATE, TO_CHAR(IR.APPLY_DATE) AS APPLY_DATE
                FROM IP_ITEM_MASTER_SETUP IM
                  INNER JOIN IP_MU_CODE MC ON UPPER(TRIM(MC.MU_CODE)) = UPPER(TRIM(IM.INDEX_MU_CODE)) AND MC.COMPANY_CODE = IM.COMPANY_CODE
                  LEFT JOIN DIST_DISTRIBUTOR_ITEM DI ON  DI.ITEM_CODE = IM.ITEM_CODE AND DI.COMPANY_CODE = IM.COMPANY_CODE 
                  LEFT JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = IM.ITEM_CODE AND ISS.COMPANY_CODE = IM.COMPANY_CODE AND TRIM(ISS.BRAND_NAME) IS NOT NULL
                  LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON IUS.ITEM_CODE = ISS.ITEM_CODE AND IUS.COMPANY_CODE = ISS.COMPANY_CODE
                  LEFT JOIN ({rateTableQuery}) IR 
                    ON IR.ITEM_CODE = IM.ITEM_CODE AND IR.COMPANY_CODE = IM.COMPANY_CODE
                WHERE 1 = 1
                AND IM.COMPANY_CODE = '{userInfo.company_code}' AND IM.CATEGORY_CODE = 'FG' AND IM.GROUP_SKU_FLAG = 'I' AND IM.DELETED_FLAG = 'N'   
                 {SalesRateClause}
                {ConversionClause}
                ORDER BY UPPER(IM.ITEM_EDESC) ASC";
                var product = _objectEntity.SqlQuery<SalesRegisterProductModel>(query).ToList();
                return product;
            }

            else
            {
                string query = $@"SELECT DISTINCT IM.ITEM_CODE, IM.ITEM_EDESC, ISS.BRAND_NAME, IM.INDEX_MU_CODE AS UNIT, MC.MU_EDESC, IUS.MU_CODE CONVERSION_UNIT,
                TO_CHAR(IUS.CONVERSION_FACTOR) AS CONVERSION_FACTOR, TO_CHAR(NVL(IR.SALES_RATE, 0)) SALES_RATE, TO_CHAR(IR.APPLY_DATE) AS APPLY_DATE
                FROM IP_ITEM_MASTER_SETUP IM
                  INNER JOIN IP_MU_CODE MC ON UPPER(TRIM(MC.MU_CODE)) = UPPER(TRIM(IM.INDEX_MU_CODE)) AND MC.COMPANY_CODE = IM.COMPANY_CODE
                  --INNER JOIN DIST_DISTRIBUTOR_ITEM DI ON  DI.ITEM_CODE = IM.ITEM_CODE AND DI.COMPANY_CODE = IM.COMPANY_CODE 
                  INNER JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = IM.ITEM_CODE AND ISS.COMPANY_CODE = IM.COMPANY_CODE AND TRIM(ISS.BRAND_NAME) IS NOT NULL
                  LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON IUS.ITEM_CODE = ISS.ITEM_CODE AND IUS.COMPANY_CODE = ISS.COMPANY_CODE
                  LEFT JOIN ({rateTableQuery}) IR 
                    ON IR.ITEM_CODE = IM.ITEM_CODE AND IR.COMPANY_CODE = IM.COMPANY_CODE
                WHERE 1 = 1
                AND IM.COMPANY_CODE = '{userInfo.company_code}' AND IM.CATEGORY_CODE = 'FG' AND IM.GROUP_SKU_FLAG = 'I' AND IM.DELETED_FLAG = 'N'   
                {SalesRateClause}
                {ConversionClause}
                ORDER BY UPPER(IM.ITEM_EDESC) ASC";

                var product = _objectEntity.SqlQuery<SalesRegisterProductModel>(query).ToList();
                return product;
            }

        }

        public PurchaseOrderReportModel GetMaxOrderNoFromDistributor()
        {
            var query = "select to_number((max(to_number(order_no)) + 1)) ORDER_NO from dist_ip_ssd_purchase_order";
            var data = _objectEntity.SqlQuery<PurchaseOrderReportModel>(query).FirstOrDefault();
            return data;
        }

        //public PurchaseOrderReportModel GetMaxOrderNoFromDistributor()
        //{
        //    var query = "select to_number((max(to_number(order_no)) + 1)) ORDER_NO from dist_ip_ssd_purchase_order";
        //    var data = _objectEntity.SqlQuery<PurchaseOrderReportModel>(query).FirstOrDefault();
        //    return data;
        //}
        public List<CollectionModel> GetCollections(User UserInfo)
        {
            string query = $@"SELECT C.SP_CODE, ES.EMPLOYEE_EDESC AS SALESPERSON_NAME, C.ENTITY_CODE, 'Distributor'  ENTITY_TYPE, CS.CUSTOMER_EDESC AS ENTITY_NAME, C.BILL_NO, C.PAYMENT_MODE, C.CHEQUE_NO, C.BANK_NAME,BS_DATE(TO_CHAR(C.CREATED_DATE)) AS MITI,
                C.AMOUNT, C.REMARKS, C.CHEQUE_CLEARANCE_DATE, C.CHEQUE_DEPOSIT_BANK
                        FROM DIST_COLLECTION C
                        LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = C.ENTITY_CODE AND CS.COMPANY_CODE = C.COMPANY_CODE AND CS.GROUP_SKU_FLAG = 'I' AND CS.DELETED_FLAG = 'N'
                        LEFT JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = C.SP_CODE AND ES.GROUP_SKU_FLAG = 'I' AND ES.DELETED_FLAG = 'N'
                WHERE C.ENTITY_TYPE = 'D' AND C.COMPANY_CODE = '{UserInfo.company_code}' AND C.ENTITY_CODE = '{UserInfo.DistributerNo}'";
            var data = this._objectEntity.SqlQuery<CollectionModel>(query).ToList();
            return data;
        }
        public string SaveCollection(CollectionModel model, User UserInfo)
        {
            string result = string.Empty;
            using (var trans = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    var clearDate = model.CHEQUE_CLEARANCE_DATE == null ? "''" : $"TO_DATE('{model.CHEQUE_CLEARANCE_DATE.Value.ToString("dd-MMM-yyyy")}','DD-MON-RRRR')";
                    string query = string.Empty;
                    if (model.SAVE_FLAG.ToUpper() == "S")
                    {
                        query = $@"INSERT INTO DIST_COLLECTION (SP_CODE, ENTITY_CODE, ENTITY_TYPE, BILL_NO, CHEQUE_NO, CHEQUE_DEPOSIT_BANK, BANK_NAME, AMOUNT, PAYMENT_MODE, 
                           CHEQUE_CLEARANCE_DATE, LATITUDE, LONGITUDE, REMARKS, CREATED_BY, CREATED_DATE, DELETED_FLAG, COMPANY_CODE, BRANCH_CODE) 
                        VALUES('{model.SP_CODE}','{UserInfo.DistributerNo}','D','{model.BILL_NO}','{model.CHEQUE_NO}','{model.CHEQUE_DEPOSIT_BANK}','{model.BANK_NAME}','{model.AMOUNT}','{model.PAYMENT_MODE}',
                        {clearDate},'0','0','{model.REMARKS}','{UserInfo.DistributerNo}',TO_DATE(SYSDATE),'N','{UserInfo.company_code}','{UserInfo.branch_code}')";
                        result = "ADDED";
                    }
                    else
                    {
                        query = $@"UPDATE DIST_COLLECTION SET SP_CODE='{model.SP_CODE}',CHEQUE_NO='{model.CHEQUE_NO}',CHEQUE_DEPOSIT_BANK='{model.CHEQUE_DEPOSIT_BANK}',BANK_NAME='{model.BANK_NAME}',AMOUNT='{model.AMOUNT}',
                        PAYMENT_MODE='{model.PAYMENT_MODE}',CHEQUE_CLEARANCE_DATE={clearDate},REMARKS='{model.REMARKS}',MODIFY_DATE=TO_DATE(SYSDATE),MODIFY_BY='{UserInfo.DistributerNo}'
                        WHERE BILL_NO='{model.BILL_NO}'";
                        string deleteQuery = $"DELETE FROM DIST_COLLECTION_DETAIL WHERE BILL_NO='{model.BILL_NO}'";
                        var delrow = _objectEntity.ExecuteSqlCommand(deleteQuery);
                        result = "UPDATED";
                    }
                    var row = this._objectEntity.ExecuteSqlCommand(query);
                    foreach (var div in model.DIVISIONS)
                    {
                        query = $@"INSERT INTO DIST_COLLECTION_DETAIL (BILL_NO,DIVISION_CODE,AMOUNT,CREATED_DATE,CREATED_BY,COMPANY_CODE,BRANCH_CODE)
                        VALUES('{model.BILL_NO}','{div.DIVISION_CODE}','{div.AMOUNT}',SYSDATE,'{UserInfo.User_id}','{UserInfo.company_code}','{UserInfo.branch_code}')";
                        row = _objectEntity.ExecuteSqlCommand(query);
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    result = "FAILED";
                }
            }
            return result;
        }
        public List<DivisionModel> GetCollectionDetail(string BillNo)
        {
            var Query = $@"SELECT CD.DIVISION_CODE,CD.AMOUNT,DS.DIVISION_EDESC
                FROM DIST_COLLECTION_DETAIL CD
                LEFT JOIN FA_DIVISION_SETUP DS
                ON CD.DIVISION_CODE = DS.DIVISION_CODE
                WHERE BILL_NO='{BillNo}'";
            var result = _objectEntity.SqlQuery<DivisionModel>(Query).ToList();
            return result;
        }
        public List<AccountStatementModel> GetAccountStatement(filterOption model, User UserInfo)
        {
            var accountFilterQuery = $@"SELECT ACC_EDESC AccountName FROM FA_CHART_OF_ACCOUNTS_SETUP WHERE ACC_CODE IN ('{string.Join("','", model.ReportFilters.AccountFilter)}')";
            var accountFilter = _objectEntity.SqlQuery<string>(accountFilterQuery).ToList();
            var result = new List<AccountStatementModel>();
            string OpeningQuery = string.Empty;
            string NonOpeningQuery = string.Empty;
            var subCode = "C" + UserInfo.DistributerNo;
            string filterFrom = "", filterTO = "";
            if (!string.IsNullOrWhiteSpace(model.ReportFilters.FromDate))
            {
                filterFrom = $"AND VSL.VOUCHER_DATE < TO_DATE('{model.ReportFilters.FromDate.ToUpper()}','RRRR-MON-DD')";
                filterTO = $"AND TRUNC(VSL.VOUCHER_DATE) BETWEEN TO_DATE('{model.ReportFilters.FromDate.ToUpper()}','RRRR-MON-DD') AND TO_DATE('{model.ReportFilters.ToDate.ToUpper()}','RRRR-MON-DD')";
            }
            else
            {
                filterFrom = $"AND VSL.VOUCHER_DATE < '16-JUL-{DateTime.Now.Year}'";
                filterTO = $"AND TRUNC(VSL.VOUCHER_DATE) BETWEEN '16-JUL-{DateTime.Now.Year}' AND '{DateTime.Now.ToString("dd-MMM-yyyy").ToUpper()}";
            }

            OpeningQuery = $@"SELECT VSL.VOUCHER_NO,FAS.ACC_EDESC,VSL.VOUCHER_DATE,NVL(VSL.PARTICULARS,' - ') PARTICULARS,VSL.SUB_EDESC,
                    TO_CHAR(NVL(VSL.DR_AMOUNT,0)) DR_AMOUNT,TO_CHAR(NVL(VSL.CR_AMOUNT,0)) CR_AMOUNT,VSL.TRANSACTION_TYPE
                    FROM V$VIRTUAL_SUB_LEDGER VSL INNER JOIN FA_CHART_OF_ACCOUNTS_SETUP FAS ON VSL.ACC_CODE=FAS.ACC_CODE AND VSL.COMPANY_CODE=FAS.COMPANY_CODE
                    WHERE 1 = 1
                    AND VSL.SUB_CODE = '{subCode}'
                    {filterFrom}
                    AND VSL.COMPANY_CODE IN ('{UserInfo.company_code}')
                    --AND VSL.BRANCH_CODE IN ('{UserInfo.branch_code}')
                    ORDER BY VSL.VOUCHER_DATE, UPPER(VSL.VOUCHER_NO) ASC";

            NonOpeningQuery = $@"SELECT VSL.VOUCHER_NO,FAS.ACC_EDESC,VSL.VOUCHER_DATE,NVL(VSL.PARTICULARS,' - ') PARTICULARS,VSL.SUB_EDESC,
                    TO_CHAR(NVL(VSL.DR_AMOUNT,0)) DR_AMOUNT,TO_CHAR(NVL(VSL.CR_AMOUNT,0)) CR_AMOUNT,VSL.TRANSACTION_TYPE
                    FROM V$VIRTUAL_SUB_LEDGER VSL INNER JOIN FA_CHART_OF_ACCOUNTS_SETUP FAS ON VSL.ACC_CODE=FAS.ACC_CODE AND VSL.COMPANY_CODE=FAS.COMPANY_CODE
                    WHERE 1=1
                    AND VSL.SUB_CODE = '{subCode}'
                    {filterTO}
                    AND VSL.COMPANY_CODE IN ('{UserInfo.company_code}')
                    --AND VSL.BRANCH_CODE IN ('{UserInfo.branch_code}')
                    ORDER BY VSL.VOUCHER_DATE, UPPER(VSL.VOUCHER_NO) ASC";
            var openingData = _objectEntity.SqlQuery<AccountStatementModel>(OpeningQuery).ToList();
            decimal OpeningBalance = 0;
            foreach (var item in openingData)
            {
                decimal amount;
                if (item.TRANSACTION_TYPE == "DR")
                {
                    decimal.TryParse(item.DR_AMOUNT, out amount);
                    OpeningBalance += amount;
                }
                else
                {
                    decimal.TryParse(item.CR_AMOUNT, out amount);
                    OpeningBalance -= amount;
                }
            }
            var OpeningObject = new AccountStatementModel
            {
                VOUCHER_NO = "0",
                PARTICULARS = "Opening Balance",
                VOUCHER_DATE = DateTime.ParseExact(model.ReportFilters.FromDate, "yyyy-MMM-dd", CultureInfo.InvariantCulture)
            };
            if (OpeningBalance >= 0)
            {
                OpeningObject.DR_AMOUNT = OpeningBalance.ToString();
                OpeningObject.CR_AMOUNT = "0";
                OpeningObject.TRANSACTION_TYPE = "DR";
            }
            else
            {
                OpeningObject.CR_AMOUNT = (-1 * OpeningBalance).ToString();
                OpeningObject.DR_AMOUNT = "0";
                OpeningObject.TRANSACTION_TYPE = "CR";
            }

            //non opening balance
            result = _objectEntity.SqlQuery<AccountStatementModel>(NonOpeningQuery).ToList();
            var itemsToRemove = new List<AccountStatementModel>();
            OpeningBalance = 0;
            bool hasOpening = false;
            foreach (var item in result)
            {
                if (item.VOUCHER_NO == "0")
                {
                    decimal amount;
                    if (item.TRANSACTION_TYPE == "DR")
                    {
                        decimal.TryParse(item.DR_AMOUNT, out amount);
                        OpeningBalance += amount;
                    }
                    else
                    {
                        decimal.TryParse(item.CR_AMOUNT, out amount);
                        OpeningBalance -= amount;
                    }
                    hasOpening = true;
                    itemsToRemove.Add(item);
                }
            }
            foreach (var item in itemsToRemove)
                result.Remove(item);
            var OpeningObject2 = new AccountStatementModel
            {
                VOUCHER_NO = "0",
                PARTICULARS = "Opening Balance",
                VOUCHER_DATE = DateTime.ParseExact(model.ReportFilters.FromDate, "yyyy-MMM-dd", CultureInfo.InvariantCulture)
            };
            if (OpeningBalance >= 0)
            {
                OpeningObject2.DR_AMOUNT = OpeningBalance.ToString();
                OpeningObject2.CR_AMOUNT = "0";
                OpeningObject2.TRANSACTION_TYPE = "DR";
            }
            else
            {
                OpeningObject2.CR_AMOUNT = (-1 * OpeningBalance).ToString();
                OpeningObject2.DR_AMOUNT = "0";
                OpeningObject2.TRANSACTION_TYPE = "CR";
            }
            if (openingData.Count != 0)
                result.Insert(0, OpeningObject);
            if (hasOpening)
                result.Insert(0, OpeningObject2);
            result = result.Select(c => { c.BALANCE = (Convert.ToDecimal(c.DR_AMOUNT) - Convert.ToDecimal(c.CR_AMOUNT)); return c; }).ToList();
            if (model.ReportFilters.AccountFilter.Count > 0)
                result = result.Where(x => accountFilter.Contains(x.ACC_EDESC) || x.PARTICULARS == "Opening Balance").ToList();
            return result;
        }
        public List<DivisionModel> GetDivisions(User userInfo)
        {
            var query = $"SELECT DIVISION_CODE,DIVISION_EDESC FROM FA_DIVISION_SETUP WHERE COMPANY_CODE='{userInfo.company_code}' AND GROUP_SKU_FLAG='I'";
            var divisions = _objectEntity.SqlQuery<DivisionModel>(query).ToList();
            return divisions;
        }
        public List<ClosingReportModel> GetClosingStock(User UserInfo, filterOption model)
        {
            var CustomerFilter = string.Empty;
            var result = new List<ClosingReportModel>();
            if (UserInfo.LoginType.Trim().ToUpper() == "D" || UserInfo.LoginType.Trim().ToUpper() == "DISTRIBUTOR")
            {

                if (model.ReportFilters.CustomerFilter.Count > 0)
                {
                    CustomerFilter = $"AND PO.CUSTOMER_CODE IN ('{string.Join("','", model.ReportFilters.CustomerFilter) + "','" + UserInfo.DistributerNo}')";
                }
                else
                {
                    CustomerFilter = $" AND PO.CUSTOMER_CODE='{UserInfo.DistributerNo}'";
                }
            }
            else
            {
                if (model.ReportFilters.CustomerFilter.Count > 0)
                    CustomerFilter = $"AND PO.CUSTOMER_CODE IN ('{string.Join(",", model.ReportFilters.CustomerFilter)}')";
            }

            var sp_code = string.Empty;

            if (model.ReportFilters.ItemBrandFilter.Count > 0)
                sp_code = $" AND PO.SP_CODE IN  ('{ string.Join("','", model.ReportFilters.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(UserInfo.sp_codes))
            {
                sp_code = $" AND PO.SP_CODE IN ({UserInfo.sp_codes})";
            }

            //In customer filter CUSTOMER_CODE is replaced by DISTRIBUTOR_CODE ↵
            //since opening stock table has distributer code column
            string OpeningQury = $@"SELECT IMS.ITEM_EDESC,PO.ITEM_CODE,PO.DISTRIBUTOR_CODE CUSTOMER_CODE,CS.CUSTOMER_EDESC,SUM(PO.CURRENT_STOCK) QUANTITY
                FROM DIST_DISTRIBUTOR_OPENING_STOCK PO
                INNER JOIN SA_CUSTOMER_SETUP CS ON PO.DISTRIBUTOR_CODE=CS.CUSTOMER_CODE AND CS.COMPANY_CODE=PO.COMPANY_CODE
                INNER JOIN IP_ITEM_MASTER_SETUP IMS ON PO.ITEM_CODE=IMS.ITEM_CODE AND PO.COMPANY_CODE=IMS.COMPANY_CODE
                WHERE 1=1 {sp_code} {CustomerFilter.Replace("CUSTOMER_CODE", "DISTRIBUTOR_CODE")}
                GROUP BY IMS.ITEM_EDESC,PO.ITEM_CODE,PO.DISTRIBUTOR_CODE,CS.CUSTOMER_EDESC";

            string SalesQuery = $@"SELECT IMS.ITEM_EDESC,PO.ITEM_CODE,PO.CUSTOMER_CODE,CS.CUSTOMER_EDESC,SUM(PO.QUANTITY) QUANTITY
                FROM SA_SALES_INVOICE PO
                    INNER JOIN SA_CUSTOMER_SETUP CS ON PO.CUSTOMER_CODE=CS.CUSTOMER_CODE AND PO.COMPANY_CODE=CS.COMPANY_CODE
                    INNER JOIN IP_ITEM_MASTER_SETUP IMS ON PO.ITEM_CODE=IMS.ITEM_CODE AND PO.COMPANY_CODE=IMS.COMPANY_CODE
                WHERE TO_DATE(PO.SALES_DATE)<=TO_DATE('{model.ReportFilters.FromDate.ToUpper()}','RRRR-MON-DD') {CustomerFilter}
                GROUP BY IMS.ITEM_EDESC,PO.ITEM_CODE,PO.CUSTOMER_CODE,CS.CUSTOMER_EDESC";

            string POQuery = $@"SELECT IMS.ITEM_EDESC,PO.ITEM_CODE,PO.CUSTOMER_CODE,CS.CUSTOMER_EDESC,SUM(PO.QUANTITY) QUANTITY
                FROM DIST_IP_SSD_PURCHASE_ORDER PO
                    INNER JOIN SA_CUSTOMER_SETUP CS ON PO.CUSTOMER_CODE=CS.CUSTOMER_CODE AND PO.COMPANY_CODE=CS.COMPANY_CODE
                    INNER JOIN IP_ITEM_MASTER_SETUP IMS ON PO.ITEM_CODE=IMS.ITEM_CODE AND PO.COMPANY_CODE=IMS.COMPANY_CODE
                WHERE TO_DATE(ORDER_DATE)<=TO_DATE('{model.ReportFilters.FromDate.ToUpper()}','RRRR-MON-DD') {CustomerFilter}
                    AND PO.APPROVED_FLAG='Y'
                GROUP BY IMS.ITEM_EDESC,PO.ITEM_CODE,PO.CUSTOMER_CODE,CS.CUSTOMER_EDESC";

            var OpeningData = _objectEntity.SqlQuery<ClosingReportModel>(OpeningQury).ToList();
            var SalesData = _objectEntity.SqlQuery<ClosingReportModel>(SalesQuery).ToList();
            var POData = _objectEntity.SqlQuery<ClosingReportModel>(POQuery).ToList();
            foreach (var item in SalesData)
            {
                decimal secondaryQty = 0;
                decimal OpQty = 0;
                var PoObj = POData.FirstOrDefault(x => x.ITEM_CODE == item.ITEM_CODE && x.CUSTOMER_CODE == item.CUSTOMER_CODE);
                var OpeningObj = OpeningData.FirstOrDefault(x => x.ITEM_CODE == item.ITEM_CODE && x.CUSTOMER_CODE == item.CUSTOMER_CODE);
                if (PoObj != null)
                {
                    secondaryQty = PoObj.QUANTITY;
                    POData.Remove(PoObj);
                }
                if (OpeningObj != null)
                {
                    item.OPENING_QTY = OpeningObj.QUANTITY;
                    OpQty = OpeningObj.QUANTITY;
                    OpeningData.Remove(OpeningObj);
                }
                item.CLOSING_QTY = item.QUANTITY + OpQty - secondaryQty;
                item.SALES_QTY = item.QUANTITY;
                item.QUANTITY = secondaryQty;
            }
            POData.ForEach(x =>
            {
                var openingObj = OpeningData.FirstOrDefault(op => op.ITEM_CODE == x.ITEM_CODE && op.CUSTOMER_CODE == x.CUSTOMER_CODE);
                if (openingObj != null)
                {
                    x.OPENING_QTY = openingObj.QUANTITY;
                    x.CLOSING_QTY = openingObj.QUANTITY - x.QUANTITY;
                }
                else
                    x.CLOSING_QTY = 0 - x.QUANTITY;
                x.SALES_QTY = 0;
            });
            OpeningData.ForEach(x =>
            {
                x.OPENING_QTY = x.CLOSING_QTY = x.QUANTITY;
                x.QUANTITY = 0;
            });
            result.AddRange(OpeningData);
            result.AddRange(SalesData);
            result.AddRange(POData);
            return result;
        }

        public List<ClosingReportModel> GetClosingStockNew(User UserInfo, filterOption model)
        {
            var fiscalYear = System.Configuration.ConfigurationManager.AppSettings["FiscalYear"].ToString();
            var query = "";
            //query = $@"SELECT ITEM_CODE,ITEM_EDESC,NEPALIMONTH,BRAND_NAME,SUM(OPENING_QTY) OPENING_QTY,SUM(PURCHASE_QTY) PURCHASE_QTY,SUM(SALES_QTY)SALES_QTY,SUM((OPENING_QTY + PURCHASE_QTY - SALES_QTY)) CLOSING_QTY
            //                FROM(
            //                SELECT IMS.ITEM_CODE,IMS.ITEM_EDESC,DRS.RANGENAME AS NEPALIMONTH,DRS.STARTDATE,ISS.BRAND_NAME,
            //                (SELECT NVL(SUM(CURRENT_STOCK),0) FROM DIST_DISTRIBUTOR_STOCK  WHERE TRUNC(CREATED_DATE) BETWEEN DRS.STARTDATE AND DRS.ENDDATE AND DISTRIBUTOR_CODE='{UserInfo.DistributerNo}' AND ITEM_CODE = IMS.ITEM_CODE) OPENING_QTY, 
            //                (SELECT NVL(SUM(QUANTITY),0) FROM SA_SALES_INVOICE WHERE TRUNC(CREATED_DATE) BETWEEN DRS.STARTDATE AND DRS.ENDDATE AND CUSTOMER_CODE='{UserInfo.DistributerNo}' AND ITEM_CODE = IMS.ITEM_CODE) PURCHASE_QTY,
            //                (SELECT NVL(SUM(QUANTITY),0) FROM DIST_IP_SSR_PURCHASE_ORDER WHERE TRUNC(CREATED_DATE) BETWEEN DRS.STARTDATE AND DRS.ENDDATE AND CUSTOMER_CODE='{UserInfo.DistributerNo}' AND ITEM_CODE = IMS.ITEM_CODE) SALES_QTY
            //                FROM IP_ITEM_MASTER_SETUP IMS
            //                INNER JOIN IP_ITEM_SPEC_SETUP ISS ON IMS.ITEM_CODE = ISS.ITEM_CODE AND IMS.COMPANY_CODE = ISS.COMPANY_CODE
            //                INNER JOIN (SELECT DISTINCT CS.AD_DATE STARTDATE, (CS.AD_DATE + CS.DAYS_NO - 1) ENDDATE, FN_BS_MONTH (SUBSTR (CS.BS_MONTH, -2, 2)) RANGENAME
            //                                FROM HR_FISCAL_YEAR_CODE FY
            //                                INNER JOIN CALENDAR_SETUP CS ON CS.AD_DATE BETWEEN FY.START_DATE AND FY.END_DATE
            //                                WHERE FY.FISCAL_YEAR_CODE = '{fiscalYear}'
            //                                ORDER BY CS.AD_DATE) DRS ON 1=1
            //                            WHERE 1 =1 
            //                            --AND IMS.COMPANY_CODE = '06'
            //                            AND IMS.CATEGORY_CODE = 'FG'
            //                            AND IMS.GROUP_SKU_FLAG = 'I'
            //                            AND ISS.BRAND_NAME IS NOT NULL
            //                )
            //                GROUP BY ITEM_CODE,ITEM_EDESC,NEPALIMONTH,BRAND_NAME,STARTDATE
            //                ORDER BY STARTDATE";

            query = $@"SELECT ITEM_CODE,ITEM_EDESC,MU_CODE,NEPALIMONTH,BRAND_NAME,ROUND(SUM(OPENING_QTY),2) OPENING_QTY,ROUND(SUM(PURCHASE_QTY),2) PURCHASE_QTY,
                        ROUND(SUM(SALES_QTY),2) SALES_QTY, ROUND(SUM(OPENING_QTY + PURCHASE_QTY - SALES_QTY),2) CLOSING_QTY
                            FROM(
                            SELECT IMS.INDEX_MU_CODE MU_CODE, IMS.ITEM_CODE,IMS.ITEM_EDESC,DRS.RANGENAME AS NEPALIMONTH,DRS.STARTDATE,ISS.BRAND_NAME,
                            (SELECT NVL(SUM(QTY),0) FROM(
                                    SELECT DDS.ITEM_CODE,DDS.CREATED_DATE,
                                        CASE DDS.MU_CODE
                                            WHEN IUS.MU_CODE THEN NVL(SUM(DDS.CURRENT_STOCK/IUS.CONVERSION_FACTOR),0)
                                            ELSE  NVL(SUM(DDS.CURRENT_STOCK),0)
                                        END QTY
                                     FROM DIST_DISTRIBUTOR_STOCK DDS, IP_ITEM_UNIT_SETUP IUS
                                     WHERE DDS.ITEM_CODE = IUS.ITEM_CODE
                                     AND DDS.COMPANY_CODE = IUS.COMPANY_CODE
                                     AND DDS.DISTRIBUTOR_CODE = '{UserInfo.DistributerNo}'
                                     GROUP BY DDS.MU_CODE,IUS.MU_CODE,DDS.ITEM_CODE,DDS.CREATED_DATE)
                                 WHERE ITEM_CODE = IMS.ITEM_CODE
                                 AND TRUNC(CREATED_DATE) BETWEEN DRS.STARTDATE AND DRS.ENDDATE) OPENING_QTY, 
                            (SELECT NVL(SUM(QTY),0) FROM(
                                    SELECT SAI.ITEM_CODE,SAI.CREATED_DATE,
                                        CASE SAI.MU_CODE
                                            WHEN IUS.MU_CODE THEN NVL(SUM(SAI.QUANTITY/IUS.CONVERSION_FACTOR),0)
                                            ELSE  NVL(SUM(SAI.QUANTITY),0)
                                        END QTY
                                      FROM SA_SALES_INVOICE SAI, IP_ITEM_UNIT_SETUP IUS
                                     WHERE SAI.ITEM_CODE = IUS.ITEM_CODE(+)
                                     AND SAI.COMPANY_CODE = IUS.COMPANY_CODE(+)
                                     AND SAI.CUSTOMER_CODE = '{UserInfo.DistributerNo}'
                                     GROUP BY SAI.MU_CODE,IUS.MU_CODE,SAI.ITEM_CODE,SAI.CREATED_DATE)
                                 WHERE ITEM_CODE = IMS.ITEM_CODE
                                 AND TRUNC(CREATED_DATE) BETWEEN DRS.STARTDATE AND DRS.ENDDATE) PURCHASE_QTY,
                            (SELECT NVL(SUM(QTY),0) FROM(
                                    SELECT RPO.ITEM_CODE,RPO.CREATED_DATE,
                                        CASE RPO.MU_CODE
                                            WHEN IUS.MU_CODE THEN NVL(SUM(RPO.QUANTITY/IUS.CONVERSION_FACTOR),0)
                                            ELSE  NVL(SUM(RPO.QUANTITY),0)
                                        END QTY
                                      FROM DIST_IP_SSR_PURCHASE_ORDER RPO, IP_ITEM_UNIT_SETUP IUS
                                     WHERE RPO.ITEM_CODE = IUS.ITEM_CODE
                                     AND RPO.COMPANY_CODE = IUS.COMPANY_CODE
                                     AND RPO.CUSTOMER_CODE = '{UserInfo.DistributerNo}'
                                     GROUP BY RPO.MU_CODE,IUS.MU_CODE,RPO.ITEM_CODE,RPO.CREATED_DATE)
                                 WHERE ITEM_CODE = IMS.ITEM_CODE
                                 AND TRUNC(CREATED_DATE) BETWEEN DRS.STARTDATE AND DRS.ENDDATE) SALES_QTY
                            FROM IP_ITEM_MASTER_SETUP IMS
                            INNER JOIN IP_ITEM_SPEC_SETUP ISS ON IMS.ITEM_CODE = ISS.ITEM_CODE AND IMS.COMPANY_CODE = ISS.COMPANY_CODE
                            LEFT JOIN IP_ITEM_UNIT_SETUP IIUS ON IMS.ITEM_CODE = IIUS.ITEM_CODE AND IMS.COMPANY_CODE = IIUS.COMPANY_CODE
                            INNER JOIN (SELECT DISTINCT CS.AD_DATE STARTDATE, (CS.AD_DATE + CS.DAYS_NO - 1) ENDDATE, FN_BS_MONTH (SUBSTR (CS.BS_MONTH, -2, 2)) RANGENAME
                                            FROM HR_FISCAL_YEAR_CODE FY
                                            INNER JOIN CALENDAR_SETUP CS ON CS.AD_DATE BETWEEN FY.START_DATE AND FY.END_DATE
                                            WHERE FY.FISCAL_YEAR_CODE = '{fiscalYear}'
                                            ORDER BY CS.AD_DATE) DRS ON 1=1
                                        WHERE 1 =1 
                                        AND IMS.COMPANY_CODE = '06'
                                        AND IMS.CATEGORY_CODE = 'FG'
                                        AND IMS.GROUP_SKU_FLAG = 'I'
                                        AND ISS.BRAND_NAME IS NOT NULL )
                            GROUP BY ITEM_CODE,ITEM_EDESC,MU_CODE,NEPALIMONTH,BRAND_NAME,STARTDATE
                            ORDER BY STARTDATE";
            var data = _objectEntity.SqlQuery<ClosingReportModel>(query).ToList();
            var groupedData = data.GroupBy(x => x.ITEM_EDESC);
            var resultData = new List<ClosingReportModel>();
            foreach (var group in groupedData)
            {
                var temp = group.ToList();
                for (int i = 1; i < temp.Count; i++)
                {
                    temp[i].OPENING_QTY = temp[i - 1].CLOSING_QTY + temp[i].OPENING_QTY;
                    temp[i].CLOSING_QTY = temp[i].OPENING_QTY + temp[i].PURCHASE_QTY - temp[i].SALES_QTY;
                }
                resultData.AddRange(temp);
            }
            return resultData;
        }

        public List<SalesVsTargetModel> GetAllSalesVsTarget(User userInfo, filterOption model)
        {
            string query = $@"  SELECT CUSTOMER_CODE,BRAND_NAME,ITEM_EDESC,NEPALI_MONTH,
                    FN_BS_MONTH (SUBSTR (NEPALI_MONTH, 5, 2)) AS NEPALI_MONTHINT,
                    ROUND(SUM(TARGET_QUANTITY),0) TARGET_QUANTITY,
                    SUM(TARGET_VALUE) TARGET_VALUE,
                    ROUND(SUM(QUANTITY_ACHIVE),0) QUANTITY_ACHIVE,
                    SUM(ACHIVE_VALUE) ACHIVE_VALUE
                FROM (SELECT DT.CUSTOMER_CODE,DT.COMPANY_CODE,B.BRAND_NAME,C.ITEM_EDESC,
                              TO_NUMBER(REPLACE (SUBSTR (BS_DATE (DT.PLAN_DATE), 0, 7), '-')) NEPALI_MONTH,
                              SUM (DT.PER_DAY_QUANTITY) AS TARGET_QUANTITY,
                              SUM (DT.PER_DAY_AMOUNT) TARGET_VALUE,0 QUANTITY_ACHIVE,0 ACHIVE_VALUE
                          FROM PL_SALES_PLAN_DTL DT, IP_ITEM_SPEC_SETUP B, IP_ITEM_MASTER_SETUP C
                          WHERE DT.ITEM_CODE = B.ITEM_CODE(+)
                              AND DT.COMPANY_CODE = B.COMPANY_CODE
                              AND DT.COMPANY_CODE = C.COMPANY_CODE
                              AND B.ITEM_CODE = C.ITEM_CODE
                              AND DT.DELETED_FLAG = 'N'
                              AND DT.CUSTOMER_CODE = '{userInfo.DistributerNo}'
                              AND DT.COMPANY_CODE = '{userInfo.company_code}'
                              AND C.GROUP_SKU_FLAG = 'I'
                      GROUP BY DT.CUSTOMER_CODE,DT.COMPANY_CODE,TO_NUMBER(REPLACE (SUBSTR (BS_DATE (PLAN_DATE), 0, 7), '-')),
                              DT.CUSTOMER_CODE,B.BRAND_NAME,C.ITEM_EDESC
                      UNION ALL
                        SELECT A.CUSTOMER_CODE,A.COMPANY_CODE,B.BRAND_NAME,C.ITEM_EDESC,TO_NUMBER(REPLACE (SUBSTR (BS_DATE (A.SALES_DATE), 0, 7), '-')) NEPALI_MONTH,
                              0 TARGET_QUANTITY,0 TARGET_VALUE,SUM (A.QUANTITY) AS QUANTITY_ACHIVE,SUM (A.CALC_TOTAL_PRICE) ACHIVE_VALUE
                          FROM SA_SALES_INVOICE A, IP_ITEM_SPEC_SETUP B, IP_ITEM_MASTER_SETUP C
                          WHERE A.ITEM_CODE = B.ITEM_CODE(+)
                              AND A.COMPANY_CODE = B.COMPANY_CODE
                              AND B.ITEM_CODE = C.ITEM_CODE
                               AND B.COMPANY_CODE=C.COMPANY_CODE
                              AND A.DELETED_FLAG = 'N'
                              AND A.CUSTOMER_CODE = '{userInfo.DistributerNo}'
                              AND A.COMPANY_CODE = '{userInfo.company_code}'
                      GROUP BY A.CUSTOMER_CODE,A.COMPANY_CODE,B.BRAND_NAME,TO_NUMBER(REPLACE (SUBSTR (BS_DATE (A.SALES_DATE), 0, 7), '-')),C.ITEM_EDESC)
            GROUP BY NEPALI_MONTH,CUSTOMER_CODE,BRAND_NAME,ITEM_EDESC
            ORDER BY NEPALI_MONTH";
            var data = _objectEntity.SqlQuery<SalesVsTargetModel>(query).ToList();
            return data;

        }
        public List<DivisionWiseCreditLimitModel> GetDivisionWiseCreditLimitList(User userInfo, filterOption filter)
        {
            var disvisionQuery = $@" select CUSTOMER_EDESC,DIVISION_EDESC,CREDIT_LIMIT,NVL(BALANCE,0) UTI, CREDIT_LIMIT-BALANCE BALANCE from (SELECT SC.CUSTOMER_EDESC,
         DS.DIVISION_EDESC,
         DL.CREDIT_LIMIT,
         DL.REMARKS,
         (select sum(CUSTOMER_DRAMOUNT -CUSTOMER_CRAMOUNT)  from V$QUICK_CUSTOMER_BALANCE
where CUSTOMER_CODE = SC.customer_code and company_code  ='06'
and division_code = DS.division_code) balance
    FROM SA_CUSTOMER_SETUP SC
         INNER JOIN SA_CUSTOMER_DIVISION_CR_LIMIT DL
            ON SC.CUSTOMER_CODE = DL.CUSTOMER_CODE
               AND SC.COMPANY_CODE = DL.COMPANY_CODE
         INNER JOIN FA_DIVISION_SETUP DS
            ON DS.DIVISION_CODE = DL.DIVISION_CODE
               AND DS.COMPANY_CODE = DL.COMPANY_CODE
   WHERE DL.CUSTOMER_CODE = '{userInfo.DistributerNo}' AND DL.DELETED_FLAG = 'N' and sc.company_code ='06'
ORDER BY SC.CUSTOMER_CODE ASC ) ";
            var result = _objectEntity.SqlQuery<DivisionWiseCreditLimitModel>(disvisionQuery).ToList();
            return result;

        }
        public List<VoucherDetailModel> GetLedgerDetailBySubCode(string SubAccCode)
        {
            var FincalYear = System.Configuration.ConfigurationManager.AppSettings["FiscalYear"].ToString();
            var dateRange = this._controlService.GetDateFilters(FincalYear).OrderByDescending(q => q.SortOrder).ToList();
            var thisYear = dateRange.Where(x => x.RangeName == "This Year").FirstOrDefault();
            SubAccCode = "'" + SubAccCode.Replace(",", "','") + "'";

            var companyCode = $@"'06'";
            //string Query = "SELECT TO_DATE ('"+ formDate + "', 'YYYY-MM-DD') AS voucher_date, bs_date(TO_DATE ('" + formDate + "', 'YYYY-MM-DD') ) as Miti, '' as Voucher_no,'' as manual_no,'Opening' AS PARTICULARS," +
            //                " CASE WHEN SUM(NVL(dr_amount, 0) - NVL(cr_amount, 0)) >= 0 THEN SUM(NVL(dr_amount, 0) - NVL(cr_amount, 0)) ELSE 0 END AS dr_amount,CASE WHEN SUM(NVL(dr_amount, 0) - NVL(cr_amount, 0)) < 0 THEN ABS(SUM(NVL(dr_amount, 0) - NVL(cr_amount, 0))) ELSE 0 END   AS cr_amount," +
            //                "0 as Balance, '' as BalanceHeader FROM v$virtual_sub_ledger WHERE acc_code = '" + accountCode + "' and sub_code = '" + SubAccCode + "' and company_code = '01'" +
            //                "AND VOUCHER_DATE < TO_DATE('" + formDate + "', 'YYYY-MM-DD') UNION ALL SELECT voucher_date, bs_date(voucher_date) as Miti, Voucher_no, manual_no, PARTICULARS, dr_amount, cr_amount, CASE WHEN NVL(dr_amount, 0) > 0 THEN ABS(NVL(dr_amount, 0) - NVL(cr_amount, 0))  else ABS(NVL(cr_amount, 0) - NVL(dr_amount, 0)) End as Balance," +
            //                "CASE WHEN NVL(dr_amount, 0) > 0 THEN 'DR' else 'CR' End as BalanceHeader FROM v$virtual_sub_ledger" +
            //                " WHERE acc_code = '"+ accountCode + "' and sub_code = '" + SubAccCode + "' and company_code = '01' AND" +
            //                " VOUCHER_DATE BETWEEN TO_DATE('" + formDate + "', 'YYYY-MM-DD') AND TO_DATE('" + toDate + "', 'YYYY-MM-DD')";
            string Query = $@"SELECT sub_code, PARTICULARS, manual_no, Voucher_no,voucher_date,bs_date(voucher_date) as Miti, dr_amount,cr_amount," +
                           " abs(l_csum) as Balance, (CASE WHEN l_csum >= 0 THEN 'DR' ELSE 'CR' END) BalanceHeader FROM ( SELECT sub_code, PARTICULARS, manual_no, Voucher_no," +
                          " voucher_date,bs_date(TO_DATE(voucher_date, 'YYYY-MM-DD')) as Miti,dr_amount,cr_amount, balance,SUM(dr_amount - cr_amount)" +
                          " OVER(ORDER BY sub_code, voucher_date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) l_csum FROM( SELECT '0' as sub_code, '' as sub_edesc,  TO_DATE('" + thisYear.StartDateString + "', 'YYYY-MM-DD') AS voucher_date," +
                           " bs_date(TO_DATE('" + thisYear.StartDateString + "', 'YYYY-MM-DD')) as Miti, '0' as Voucher_no, '' as manual_no,'Opening' AS PARTICULARS, CASE WHEN SUM(NVL(dr_amount, 0) - NVL(cr_amount, 0)) >= 0 THEN SUM(NVL(dr_amount, 0) - NVL(cr_amount, 0))" +
                          " ELSE 0 END AS dr_amount, CASE WHEN SUM(NVL(dr_amount, 0) - NVL(cr_amount, 0)) < 0 THEN ABS(SUM(NVL(dr_amount, 0) - NVL(cr_amount, 0))) ELSE 0 END   AS cr_amount," +
                          " 0 as balance FROM v$virtual_sub_ledger WHERE  sub_code IN (" + SubAccCode + ") and company_code = '06' AND (VOUCHER_DATE < TO_DATE('" + thisYear.StartDateString + "', 'YYYY-MM-DD') OR FORM_CODE = '0')" +
                          " UNION ALL SELECT sub_code, sub_edesc, voucher_date, bs_date(TO_DATE(voucher_date, 'YYYY-MM-DD')) as Miti, Voucher_no,manual_no, PARTICULARS,dr_amount,cr_amount, l_csum balance" +
                          " FROM(SELECT sub_code,sub_edesc, voucher_date, bs_date(TO_DATE(voucher_date, 'YYYY-MM-DD')) as Miti, Voucher_no, manual_no, PARTICULARS,dr_amount," +
                          " cr_amount,dr_amount - cr_amount balance,SUM(dr_amount - cr_amount) OVER(ORDER BY sub_code, sub_edesc, voucher_date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) l_csum FROM v$virtual_sub_ledger" +
                          "  WHERE sub_code IN (" + SubAccCode + ") AND FORM_CODE <> '0' AND company_code = '01' and voucher_date >= TO_DATE('" + thisYear.StartDateString + "', 'YYYY-MM-DD')AND voucher_date <= TO_DATE('" + thisYear.EndDateString + "', 'YYYY-MM-DD') ORDER BY voucher_date)))";
            var subvoucherList = _objectEntity.SqlQuery<VoucherDetailModel>(Query).ToList();
            return subvoucherList;
        }

        public List<SalesVsTargetModel> GetAllSalesTargetVsAchievement(User userInfo, filterOption model)
        {
            string query = $@"  SELECT BRAND_NAME,ITEM_EDESC,
                    ROUND(SUM(TARGET_QUANTITY),0) TARGET_QUANTITY,
                    SUM(TARGET_VALUE) TARGET_VALUE,
                    ROUND(SUM(QUANTITY_ACHIVE),0) QUANTITY_ACHIVE,
                    SUM(ACHIVE_VALUE) ACHIVE_VALUE
                FROM (SELECT B.BRAND_NAME,C.ITEM_EDESC,                             
                              SUM (DT.PER_DAY_QUANTITY) AS TARGET_QUANTITY,
                              SUM (DT.PER_DAY_AMOUNT) TARGET_VALUE,0 QUANTITY_ACHIVE,0 ACHIVE_VALUE
                          FROM PL_SALES_PLAN_DTL DT, IP_ITEM_SPEC_SETUP B, IP_ITEM_MASTER_SETUP C
                          WHERE DT.ITEM_CODE = B.ITEM_CODE(+)
                              AND DT.COMPANY_CODE = B.COMPANY_CODE
                              AND DT.COMPANY_CODE = C.COMPANY_CODE
                              AND B.ITEM_CODE = C.ITEM_CODE
                              AND DT.DELETED_FLAG = 'N'
                              AND DT.COMPANY_CODE = '{userInfo.company_code}'
                              AND DT.BRANCH_CODE = '{userInfo.branch_code}'
                              AND C.GROUP_SKU_FLAG = 'I'
                      GROUP BY B.BRAND_NAME,C.ITEM_EDESC
                      UNION ALL
                        SELECT B.BRAND_NAME,C.ITEM_EDESC,
                              0 TARGET_QUANTITY,0 TARGET_VALUE,SUM (A.QUANTITY) AS QUANTITY_ACHIVE,SUM (A.CALC_TOTAL_PRICE) ACHIVE_VALUE
                          FROM SA_SALES_INVOICE A, IP_ITEM_SPEC_SETUP B, IP_ITEM_MASTER_SETUP C
                          WHERE A.ITEM_CODE = B.ITEM_CODE(+)
                              AND A.COMPANY_CODE = B.COMPANY_CODE
                              AND B.ITEM_CODE = C.ITEM_CODE
                               AND B.COMPANY_CODE=C.COMPANY_CODE
                              AND A.DELETED_FLAG = 'N'
                              AND A.COMPANY_CODE = '{userInfo.company_code}'
                              AND A.BRANCH_CODE = '{userInfo.branch_code}'
                      GROUP BY B.BRAND_NAME,C.ITEM_EDESC)
            GROUP BY BRAND_NAME,ITEM_EDESC ORDER BY ITEM_EDESC";
            var data = _objectEntity.SqlQuery<SalesVsTargetModel>(query).ToList();
            return data;
        }


        public List<SalesVsCustomerTargetModel> GetAllSalesTargetVsCustomerAchievement(User userInfo, filterOption model)
        {
            List<SalesVsCustomerTargetModel> targetmodellst = new List<SalesVsCustomerTargetModel>();
            string query = $@"  SELECT BRAND_NAME,ITEM_EDESC, FN_BS_MONTH (SUBSTR (NEPALI_MONTH, 5, 2)) AS NEPALI_MONTHINT,          
                    ROUND(SUM(TARGET_QUANTITY),0) TARGET_QUANTITY,
                    SUM(TARGET_VALUE) TARGET_VALUE,
                    ROUND(SUM(QUANTITY_ACHIVE),0) QUANTITY_ACHIVE,
                    SUM(ACHIVE_VALUE) ACHIVE_VALUE
                FROM (SELECT DT.CUSTOMER_CODE,DT.COMPANY_CODE,B.BRAND_NAME,C.ITEM_EDESC,
                              TO_NUMBER(REPLACE (SUBSTR (BS_DATE (DT.PLAN_DATE), 0, 7), '-')) NEPALI_MONTH,
                              SUM (DT.PER_DAY_QUANTITY) AS TARGET_QUANTITY,
                              SUM (DT.PER_DAY_AMOUNT) TARGET_VALUE,0 QUANTITY_ACHIVE,0 ACHIVE_VALUE
                          FROM PL_SALES_PLAN_DTL DT, IP_ITEM_SPEC_SETUP B, IP_ITEM_MASTER_SETUP C
                          WHERE DT.ITEM_CODE = B.ITEM_CODE(+)
                              AND DT.COMPANY_CODE = B.COMPANY_CODE
                              AND DT.COMPANY_CODE = C.COMPANY_CODE
                              AND B.ITEM_CODE = C.ITEM_CODE
                              AND DT.DELETED_FLAG = 'N'
                              AND DT.CUSTOMER_CODE = '{userInfo.DistributerNo}'
                              AND DT.COMPANY_CODE = '{userInfo.company_code}'
                              AND DT.BRANCH_CODE = '{userInfo.branch_code}'
                              AND C.GROUP_SKU_FLAG = 'I'
                      GROUP BY DT.CUSTOMER_CODE,DT.COMPANY_CODE,TO_NUMBER(REPLACE (SUBSTR (BS_DATE (PLAN_DATE), 0, 7), '-')),
                              DT.CUSTOMER_CODE,B.BRAND_NAME,C.ITEM_EDESC
                      UNION ALL
                        SELECT A.CUSTOMER_CODE,A.COMPANY_CODE,B.BRAND_NAME,C.ITEM_EDESC,TO_NUMBER(REPLACE (SUBSTR (BS_DATE (A.SALES_DATE), 0, 7), '-')) NEPALI_MONTH,
                              0 TARGET_QUANTITY,0 TARGET_VALUE,SUM (A.QUANTITY) AS QUANTITY_ACHIVE,SUM (A.CALC_TOTAL_PRICE) ACHIVE_VALUE
                          FROM SA_SALES_INVOICE A, IP_ITEM_SPEC_SETUP B, IP_ITEM_MASTER_SETUP C
                          WHERE A.ITEM_CODE = B.ITEM_CODE(+)
                              AND A.COMPANY_CODE = B.COMPANY_CODE
                              AND B.ITEM_CODE = C.ITEM_CODE
                               AND B.COMPANY_CODE=C.COMPANY_CODE
                              AND A.DELETED_FLAG = 'N'
                              AND A.CUSTOMER_CODE = '{userInfo.DistributerNo}'
                              AND A.COMPANY_CODE = '{userInfo.company_code}'
                              AND A.BRANCH_CODE = '{userInfo.branch_code}'
                              AND C.GROUP_SKU_FLAG = 'I'   
                      GROUP BY A.CUSTOMER_CODE,A.COMPANY_CODE,B.BRAND_NAME,TO_NUMBER(REPLACE (SUBSTR (BS_DATE (A.SALES_DATE), 0, 7), '-')),C.ITEM_EDESC)
            GROUP BY BRAND_NAME,ITEM_EDESC, FN_BS_MONTH (SUBSTR (NEPALI_MONTH, 5, 2))
            ORDER BY ITEM_EDESC";
            //var a = GetAllSalesVsTarget(userInfo, model);
            //var data = _objectEntity.SqlQuery(query);
            var data = _objectEntity.SqlQuery<SalesVsTargetModel>(query).ToList();
            var a = data.Select(x => x.ITEM_EDESC).Distinct().ToList();
            var months = data.Select(x => x.NEPALI_MONTHINT).Distinct().ToList();
            foreach (var item in a)
            {
                SalesVsCustomerTargetModel targetmodel = new SalesVsCustomerTargetModel();
                targetmodel.ITEM_EDESC = item;
                //List<MONTH> monthmodellst = new List<MONTH>();
                foreach (var month in months)
                {
                    MONTH monthmodel = new MONTH();
                    monthmodel.MONTH_NAME = month;
                    var monthdata = data.Where(x => x.NEPALI_MONTHINT == month && x.ITEM_EDESC==item).ToList();
                    if (monthdata.Count !=0)
                    {
                        foreach (var dt in monthdata)
                        {
                            MonthWiseTarget monthtargetmodel = new MonthWiseTarget();
                            monthtargetmodel.TARGET_QUANTITY = dt.TARGET_QUANTITY;
                            monthtargetmodel.TARGET_VALUE = dt.TARGET_VALUE;
                            monthtargetmodel.QUANTITY_ACHIVE = dt.QUANTITY_ACHIVE;
                            monthtargetmodel.ACHIVE_VALUE = dt.ACHIVE_VALUE;
                            monthmodel.Target = monthtargetmodel;
                        }
                        //monthmodellst.Add(monthmodel);
                        targetmodel.MONTH_TARGET.Add(monthmodel);
                    }
                }
                targetmodellst.Add(targetmodel);
                
            }
            
            
            return targetmodellst;
        }

        public List<SchemeModel> GetSchemeData(User userInfo, filterOption model)
        {
            string query = $@"";
            return null;
        }

    }
}