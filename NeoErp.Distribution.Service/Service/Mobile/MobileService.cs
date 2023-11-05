using NeoErp.Core.Helpers;
using NeoErp.Core.Models;
using NeoErp.Core.Services;
using NeoErp.Distribution.Service.Model.Mobile;
using NepaliDateConverter.Net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
//using NepaliDateConverter.Net;



namespace NeoErp.Distribution.Service.Service.Mobile
{
    public class MobileService : IMobileService
    {
        private const string CATEGORY_CODE = "FG";
        private const string GROUP_SKU_FLAG = "I";
        private IMessageService _MessageService;
        private readonly string UploadPath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + @"Areas\NeoErp.Distribution\Images";

        public MobileService(IMessageService messageService)
        {
            _MessageService = messageService;
        }

        #region Private Functions
        private int GetMaxId(string table, string column, NeoErpCoreEntity dbContext)
        {
            var query = $"SELECT nvl(max({column}),0) +1 as p_key FROM {table}";
            var result = dbContext.SqlQuery<int>(query).FirstOrDefault();
            return result;
        }

        private List<ItemModel> FetchAllCompanyItems(string companyCode, NeoErpCoreEntity dbContext)
        {
            var pref = FetchPreferences(companyCode, dbContext);
            string conversionClause = "";
            if (pref.SQL_NN_CONVERSION_UNIT_FACTOR == "Y")
                conversionClause = "AND IUS.MU_CODE IS NOT NULL AND IUS.CONVERSION_FACTOR IS NOT NULL";
            var Query = $@"SELECT IM.ITEM_CODE, IM.ITEM_EDESC, ISS.BRAND_NAME, IM.INDEX_MU_CODE AS UNIT, IM.INDEX_MU_CODE AS MU_CODE, MC.MU_EDESC, IUS.MU_CODE CONVERSION_UNIT,TO_CHAR(IUS.CONVERSION_FACTOR) CONVERSION_FACTOR, IM.COMPANY_CODE, IM.BRANCH_CODE
				FROM IP_ITEM_MASTER_SETUP IM
				  INNER JOIN IP_MU_CODE MC ON MC.MU_CODE = IM.INDEX_MU_CODE AND MC.COMPANY_CODE = IM.COMPANY_CODE
				  INNER JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = IM.ITEM_CODE AND ISS.COMPANY_CODE = IM.COMPANY_CODE AND TRIM(ISS.BRAND_NAME) IS NOT NULL
				  LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON IUS.ITEM_CODE = ISS.ITEM_CODE AND IUS.COMPANY_CODE = ISS.COMPANY_CODE AND IUS.MU_CODE IS NOT NULL AND IUS.CONVERSION_FACTOR IS NOT NULL
				WHERE 1 = 1
				AND IM.COMPANY_CODE IN (SELECT COMPANY_CODE FROM COMPANY_SETUP) AND IM.CATEGORY_CODE = '{CATEGORY_CODE}' AND IM.GROUP_SKU_FLAG = '{GROUP_SKU_FLAG}' AND IM.DELETED_FLAG = 'N'
                {conversionClause}
				ORDER BY IM.COMPANY_CODE, IM.BRANCH_CODE, UPPER(IM.ITEM_EDESC) ASC";
            var data = dbContext.SqlQuery<ItemModel>(Query).ToList();
            return data;
        }

        private List<ItemModel> FetchAllCompanyBranchItemRate(NeoErpCoreEntity dbContext)
        {
            var Query = $@"SELECT B.COMPANY_CODE, B.BRANCH_CODE, A.ITEM_CODE, TO_CHAR(NVL(B.SALES_RATE, 0)) SALES_RATE, A.APPLY_DATE
                  FROM (SELECT ITEM_CODE, COMPANY_CODE, BRANCH_CODE,TO_CHAR(MAX(APP_DATE)) APPLY_DATE 
                    FROM IP_ITEM_RATE_APPLICAT_SETUP
                    WHERE 1 = 1
                    GROUP BY ITEM_CODE, COMPANY_CODE, BRANCH_CODE) A
                  INNER JOIN IP_ITEM_RATE_APPLICAT_SETUP B
                    ON B.ITEM_CODE = A.ITEM_CODE
                    AND B.APP_DATE = A.APPLY_DATE
                    AND B.COMPANY_CODE = A.COMPANY_CODE
                    AND B.COMPANY_CODE IN (SELECT COMPANY_CODE FROM COMPANY_SETUP)
                    AND B.BRANCH_CODE = A.BRANCH_CODE
                  WHERE 1 = 1
                  AND SALES_RATE <> 0
                  ORDER BY B.COMPANY_CODE, B.BRANCH_CODE, TO_NUMBER(A.ITEM_CODE)";
            var data = dbContext.SqlQuery<ItemModel>(Query).ToList();
            return data;
        }

        private List<SubLedgerMapModel> FetchAllCompanySubLedgerMap(NeoErpCoreEntity dbContext)
        {
            var Query = $@"SELECT SLM.ACC_CODE, SLM.SUB_CODE, CS.PARTY_TYPE_CODE AS DEFAULT_PARTY_TYPE_CODE, SLM.COMPANY_CODE
                FROM FA_SUB_LEDGER_MAP SLM
                INNER JOIN SA_CUSTOMER_SETUP CS ON TRIM(CS.LINK_SUB_CODE) = TRIM(SLM.SUB_CODE) AND CS.GROUP_SKU_FLAG = 'I' AND CS.COMPANY_CODE = SLM.COMPANY_CODE
                WHERE SUBSTR(SLM.SUB_CODE, 1, 1) = 'C'
                ORDER BY TO_NUMBER(SLM.ACC_CODE), TO_NUMBER(SUBSTR(SLM.SUB_CODE, 2)), SLM.COMPANY_CODE";
            var data = dbContext.SqlQuery<SubLedgerMapModel>(Query).ToList();
            return data;
        }

        private List<PartyTypeModel> FetchAllCompanyPartyType(NeoErpCoreEntity dbContext)
        {
            var data = dbContext.SqlQuery<PartyTypeModel>("SELECT PARTY_TYPE_CODE, PARTY_TYPE_EDESC PARTY_TYPE_NAME, TO_CHAR(CREDIT_DAYS) CREDIT_DAYS, TO_CHAR(CREDIT_LIMIT) CREDIT_LIMIT, COMPANY_CODE FROM IP_PARTY_TYPE_CODE WHERE DELETED_FLAG = 'N'").ToList();
            return data;
        }

        private List<CustomerModel> FetchAllCompanySaCustomer(NeoErpCoreEntity dbContext)
        {
            var Query = $@"SELECT CUSTOMER_CODE, CUSTOMER_EDESC CUSTOMER_NAME, REGD_OFFICE_EADDRESS ADDRESS, PARTY_TYPE_CODE, LINK_SUB_CODE, ACC_CODE,
                 TO_CHAR(CREDIT_DAYS) CREDIT_DAYS, To_CHAR(CREDIT_LIMIT) CREDIT_LIMIT, COMPANY_CODE, BRANCH_CODE
                FROM SA_CUSTOMER_SETUP 
                WHERE GROUP_SKU_FLAG = 'I'
                AND DELETED_FLAG = 'N'";
            var data = dbContext.SqlQuery<CustomerModel>(Query).ToList();
            return data;
        }

        private List<AreaResponseModel> FetchAllCompanyArea(NeoErpCoreEntity dbContext)
        {
            string Query = $@"SELECT A.AREA_CODE,A.AREA_NAME,A.ZONE_CODE,A.DISTRICT_CODE,A.VDC_CODE,A.REG_CODE,
                       B.ZONE_NAME,B.DISTRICT_NAME,B.VDC_NAME,B.REG_NAME,A.COMPANY_CODE
                FROM DIST_AREA_MASTER A,DIST_ADDRESS_MASTER B
                WHERE (A.DISTRICT_CODE = B.DISTRICT_CODE
                AND  A.REG_CODE = B.REG_CODE
                AND  A.ZONE_CODE = B.ZONE_CODE
                AND  A.VDC_CODE = B.VDC_CODE) ORDER BY UPPER(A.AREA_NAME) ASC";
            var data = dbContext.SqlQuery<AreaResponseModel>(Query).ToList();
            return data;
        }

        private List<SalesTypeModel> FetchAllCompanySaSalesType(NeoErpCoreEntity dbContext)
        {
            string Query = $@"SELECT SALES_TYPE_CODE, SALES_TYPE_EDESC, COMPANY_CODE FROM SA_SALES_TYPE
                            WHERE GROUP_SKU_FLAG = '{GROUP_SKU_FLAG}' AND DELETED_FLAG = 'N'
                            ORDER BY COMPANY_CODE, UPPER(TRIM(SALES_TYPE_EDESC))";
            var data = dbContext.SqlQuery<SalesTypeModel>(Query).ToList();
            return data;
        }

        private List<ShippingAddressModel> FetchShippingAddress(NeoErpCoreEntity dbContext)
        {
            string Query = $@"SELECT TRIM(CC.CITY_CODE) CITY_CODE, TRIM(CC.CITY_EDESC) CITY_EDESC, TRIM(DC.DISTRICT_EDESC) DISTRICT_EDESC,
                (CASE WHEN TRIM(CC.CITY_EDESC) = TRIM(DC.DISTRICT_EDESC) THEN TRIM(CC.CITY_EDESC) ELSE TRIM(CC.CITY_EDESC) || ', ' || TRIM(DC.DISTRICT_EDESC) END) CITY
                FROM CITY_CODE CC
                INNER JOIN DISTRICT_CODE DC ON TRIM(DC.DISTRICT_CODE) = TRIM(CC.DISTRICT_CODE)
                WHERE 1 = 1 
                  AND DELETED_FLAG = 'N'
                ORDER BY UPPER(TRIM(CITY_EDESC))";
            var data = dbContext.SqlQuery<ShippingAddressModel>(Query).ToList();
            data = data == null ? new List<ShippingAddressModel>() : data;
            return data;
        }

        private PreferenceModel FetchPreferences(string comp_code, NeoErpCoreEntity dbContext)
        {
            var PreferenceQuery = $"SELECT * FROM DIST_PREFERENCE_SETUP WHERE COMPANY_CODE='{comp_code}'";
            var preferences = dbContext.SqlQuery<PreferenceModel>(PreferenceQuery).FirstOrDefault();
            return preferences;
        }

        #endregion Private Functions

        #region Fetching Data
        public List<LoginResponseModel> Login(LoginModel model, NeoErpCoreEntity dbContext)
        {
            LoginResponseModel result = new LoginResponseModel();
            if (string.IsNullOrEmpty(model.UserName as string))
                throw new Exception("Username is empty.");

            if (string.IsNullOrEmpty(model.Password as string))
                throw new Exception("Password is empty.");

            if (string.IsNullOrEmpty(model.Imei as string))
                model.Imei = "EMPTY";

            //user validation
            string UserQuery = $@"SELECT TO_CHAR(LU.USERID) AS USER_ID, TO_CHAR(LU.GROUPID) AS GROUP_ID, LU.USER_NAME, LU.IS_MOBILE, LU.ATTENDANCE, 
                    ES.EMPLOYEE_EDESC AS FULL_NAME, LU.PASS_WORD, LU.CONTACT_NO, LU.SP_CODE, LU.GROUPID,
                    LU.USER_TYPE, LU.SUPER_USER, TO_CHAR(LU.EXPIRY_DATE) EXPIRY_DATE, TO_CHAR(LU.LINK_SYN_USER_NO) AS LINK_SYN_USER_NO,TO_CHAR(DRU.ROLE_CODE) AS ROLE_CODE,
                    LU.COMPANY_CODE,LU.BRANCH_CODE,LU.AREA_CODE,LU.BRANDING
                FROM DIST_LOGIN_USER LU
                INNER JOIN DIST_SALESPERSON_MASTER SM ON SM.SP_CODE = LU.SP_CODE
                INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = SM.SP_CODE AND ES.COMPANY_CODE = LU.COMPANY_CODE --for user's actual company code in synergy
                INNER JOIN DIST_ROLE_USER DRU ON DRU.USERID = LU.USERID
                WHERE 1 = 1
                AND LU.USER_NAME = '{model.UserName}' 
                AND LU.PASS_WORD = '{model.Password}'
                AND LU.ACTIVE = 'Y'";
            result = dbContext.SqlQuery<LoginResponseModel>(UserQuery).FirstOrDefault();
            if (result == null)
                throw new Exception("Incorrect username or password.");
            DateTime userDate;
            DateTime.TryParse(result.EXPIRY_DATE, out userDate);
            if (userDate < DateTime.Now)
                throw new Exception("User is expired.");
            if (result.IS_MOBILE == "N")
                throw new Exception("Not a Mobile User.");

            //imei validation
            var companyName = dbContext.SqlQuery<string>($"SELECT COMPANY_EDESC FROM COMPANY_SETUP WHERE COMPANY_CODE='{result.COMPANY_CODE}'").FirstOrDefault();
            if (!companyName.Equals("JGI Distribution Pvt. Ltd."))
            {
                List<string> SavedImei = dbContext.SqlQuery<string>($"SELECT IMEI_NO FROM DIST_LOGIN_DEVICE WHERE USERID='{result.USER_ID}' AND APPROVED_FLAG='Y' AND ACTIVE='Y'").ToList();
                var imei = dbContext.SqlQuery<string>($"SELECT IMEI_NO FROM DIST_LOGIN_DEVICE WHERE IMEI_NO='{model.Imei}' ").ToList();
                if (SavedImei.Count == 0)
                {
                    if (imei.Count > 0 && model.Imei != "EMPTY")
                        throw new Exception("Device already in use by another user");
                    string imeiInsert = $@"INSERT INTO DIST_LOGIN_DEVICE (USERID,IMEI_NO,DEVICE_NAME,CREATED_BY,APPROVED_FLAG,ACTIVE,APP_VERSION,FIREBASE_ID,CURRENT_LOGIN)
                VALUES ('{result.USER_ID}','{model.Imei}','{model.Device_Name}','{result.USER_ID}','Y','Y','{model.App_Version}','{model.Firebase_key}','Y')";
                    var rowNum = dbContext.ExecuteSqlCommand(imeiInsert);
                }
                else if (!SavedImei.Contains(model.Imei.Trim()))
                {

                    if (imei.Count > 0 && model.Imei != "EMPTY")
                        throw new Exception("Device already in use by another user");
                    string imeiInsert = $@"INSERT INTO DIST_LOGIN_DEVICE (USERID,IMEI_NO,DEVICE_NAME,CREATED_BY,APP_VERSION,FIREBASE_ID)
                VALUES ('{result.USER_ID}','{model.Imei}','{model.Device_Name}','{result.USER_ID}','{model.App_Version}','{model.Firebase_key}')";
                    var rowNum = dbContext.ExecuteSqlCommand(imeiInsert);
                    throw new Exception("IMEI_REG_ERROR");
                }
            }
            else
            {
                List<string> SavedImei = dbContext.SqlQuery<string>($"SELECT IMEI_NO FROM DIST_LOGIN_DEVICE WHERE USERID='{result.USER_ID}'").ToList();
                var imei = dbContext.SqlQuery<string>($"SELECT IMEI_NO FROM DIST_LOGIN_DEVICE WHERE IMEI_NO='{model.Imei}' ").ToList();
                if (SavedImei.Count == 0)
                {
                    if (imei.Count > 0 && model.Imei != "EMPTY")
                        throw new Exception("Device already in use by another user");
                    string imeiInsert = $@"INSERT INTO DIST_LOGIN_DEVICE (USERID,IMEI_NO,DEVICE_NAME,CREATED_BY,APPROVED_FLAG,ACTIVE,APP_VERSION,FIREBASE_ID,CURRENT_LOGIN)
                VALUES ('{result.USER_ID}','{model.Imei}','{model.Device_Name}','{result.USER_ID}','Y','Y','{model.App_Version}','{model.Firebase_key}','Y')";
                    var rowNum = dbContext.ExecuteSqlCommand(imeiInsert);
                }
                else if (!SavedImei.Contains(model.Imei.Trim()))
                {

                    if (imei.Count > 0 && model.Imei != "EMPTY")
                        throw new Exception("Device already in use by another user");
                    string imeiInsert = $@"INSERT INTO DIST_LOGIN_DEVICE (USERID,IMEI_NO,DEVICE_NAME,CREATED_BY,APP_VERSION,FIREBASE_ID)
                VALUES ('{result.USER_ID}','{model.Imei}','{model.Device_Name}','{result.USER_ID}','{model.App_Version}','{model.Firebase_key}')";
                    var rowNum = dbContext.ExecuteSqlCommand(imeiInsert);
                    throw new Exception("IMEI_REG_ERROR");
                }
            }
            
            

            //make all devices as not current login
            var row = dbContext.ExecuteSqlCommand($"UPDATE DIST_LOGIN_DEVICE SET CURRENT_LOGIN='N' WHERE USERID='{result.USER_ID}' AND IMEI_NO !='{model.Imei}'");

            //update App version
            row = dbContext.ExecuteSqlCommand($"UPDATE DIST_LOGIN_DEVICE SET APP_VERSION='{model.App_Version}',FIREBASE_ID='{model.Firebase_key}',CURRENT_LOGIN='Y',INSTALLED_APPS='{model.Installed_Apps}' WHERE USERID='{result.USER_ID}' AND IMEI_NO='{model.Imei}'");

            //inserting location
            var insertQuery = $@"INSERT INTO DIST_LM_LOCATION_TRACKING (SP_CODE, SUBMIT_DATE, LATITUDE, LONGITUDE,COMPANY_CODE,BRANCH_CODE,TRACK_TYPE) VALUES 
                            ('{result.SP_CODE}',SYSDATE,'{model.latitude}','{model.longitude}','{result.COMPANY_CODE}','{result.BRANCH_CODE}','TRK')";
            row = dbContext.ExecuteSqlCommand(insertQuery);
            //inserting location

            var compQuery = $@"SELECT CC.COMPANY_CODE, INITCAP(TRIM(CS.COMPANY_EDESC)) COMPANY_EDESC,
                            BC.BRANCH_CODE, TRIM(BS.BRANCH_EDESC) BRANCH_EDESC,
                            TO_CHAR(PS.FY_START_DATE, 'YYYY-MM-DD') AS FISCAL_START, TO_CHAR(PS.FY_END_DATE, 'YYYY-MM-DD') AS FISCAL_END
                    FROM SC_APPLICATION_USERS AU
                    LEFT JOIN SC_COMPANY_CONTROL CC ON CC.USER_NO = AU.USER_NO
                    INNER JOIN COMPANY_SETUP CS ON CS.COMPANY_CODE = CC.COMPANY_CODE
                    LEFT JOIN SC_BRANCH_CONTROL BC ON BC.USER_NO = AU.USER_NO AND BC.COMPANY_CODE = CC.COMPANY_CODE
                    INNER JOIN FA_BRANCH_SETUP BS ON BS.BRANCH_CODE = BC.BRANCH_CODE AND BS.COMPANY_CODE = BC.COMPANY_CODE AND BS.GROUP_SKU_FLAG = 'I' AND BS.DELETED_FLAG = 'N'
                    LEFT JOIN (SELECT DISTINCT COMPANY_CODE, FY_START_DATE, FY_END_DATE FROM PREFERENCE_SETUP) PS ON PS.COMPANY_CODE = CC.COMPANY_CODE
                    WHERE 1 = 1
                      AND AU.EMPLOYEE_CODE = '{result.SP_CODE}'
                       AND AU.DELETED_FLAG='N'
                    GROUP BY CC.COMPANY_CODE, INITCAP(TRIM(CS.COMPANY_EDESC)),
                              BC.BRANCH_CODE, TRIM(BS.BRANCH_EDESC),
                              TO_CHAR(PS.FY_START_DATE, 'YYYY-MM-DD'), TO_CHAR(PS.FY_END_DATE, 'YYYY-MM-DD')
                    ORDER BY '', INITCAP(TRIM(CS.COMPANY_EDESC)),TRIM(BS.BRANCH_EDESC)";

            var allCompany = dbContext.SqlQuery<TempCompanyModel>(compQuery).GroupBy(x => x.COMPANY_CODE);
            var companies = new List<CompanyModel>();
            foreach (var group in allCompany)
            {
                var compTemp = group.FirstOrDefault();
                var comp = new CompanyModel()
                {
                    COMPANY_CODE = compTemp.COMPANY_CODE,
                    COMPANY_EDESC = compTemp.COMPANY_EDESC,
                    FISCAL_START = compTemp.FISCAL_START,
                    FISCAL_END = compTemp.FISCAL_END
                };
                foreach (var branch in group)
                {
                    comp.BRANCH.Add(branch.BRANCH_CODE, new BranchModel
                    {
                        BRANCH_CODE = branch.BRANCH_CODE,
                        BRANCH_EDESC = branch.BRANCH_EDESC
                    });
                }
                comp.PREFERENCE = FetchPreferences(comp.COMPANY_CODE, dbContext);
                result.COMPANY.Add(comp.COMPANY_CODE, comp);
            }

            var list = new List<LoginResponseModel>();
            list.Add(result);
            return list;
        }

        public Dictionary<string, string> Logout(LogoutRequestModel model, NeoErpCoreEntity dbContext)
        {
            string AttendanceQuery = $@"INSERT INTO HRIS_ATTENDANCE (EMPLOYEE_ID,ATTENDANCE_DT,ATTENDANCE_FROM,ATTENDANCE_TIME) VALUES ('{model.SP_CODE}',TRUNC(SYSDATE),'MOBILE',CURRENT_TIMESTAMP)";
            var row = dbContext.ExecuteSqlCommand(AttendanceQuery);
            var time = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss.fffffff tt", CultureInfo.InvariantCulture);
            try
            {
                var thumbId = dbContext.SqlQuery<string>($"SELECT ID_THUMB_ID FROM HRIS_EMPLOYEES WHERE EMPLOYEE_ID = '{model.SP_CODE}'").FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(thumbId))
                {
                    var hris_procedure = $"BEGIN HRIS_ATTENDANCE_INSERT ({thumbId}, TRUNC(SYSDATE), NULL, 'MOBILE', TO_TIMESTAMP('{time}')); END;";
                    dbContext.ExecuteSqlCommand(hris_procedure);
                }
            }
            catch (Exception ex)
            {
            }
            if (row <= 0)
                throw new Exception("Attendance could not be made!!!");
            else
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                result.Add("msg", "Your logout attendance has been made successfully");
                return result;
            }
        }

        public Dictionary<string, VisitPlanResponseModel> GetVisitPlan(VisitPlanRequestModel model, NeoErpCoreEntity dbContext)
        {
            string Superuser;
            string today = string.Empty;
            if (string.IsNullOrEmpty(model.date))
                today = DateTime.Now.ToString("dd-MMM-yyyy");
            else
                today = model.date;
            Dictionary<string, VisitPlanResponseModel> Result = new Dictionary<string, VisitPlanResponseModel>();
            VisitPlanResponseModel VisitPlans = new VisitPlanResponseModel();

            if (string.IsNullOrEmpty(model.spcode as string))
                throw new Exception("User code is empty.");
            string VisitQuery = string.Empty;
            string UserQuery = $@"select super_user from dist_login_user where sp_code = '{model.spcode}'";
            var users = dbContext.SqlQuery<string>(UserQuery).ToList();

            if (users.Count == 0)
                throw new Exception("Invalid user");
            else
                Superuser = users[0];//for future use. if the user is super user, different query will be used

            VisitQuery = $@"SELECT * FROM (
                (SELECT
                DM.DEALER_CODE AS CODE,DM.EMAIL,
                PT.PARTY_TYPE_EDESC AS NAME,
                PT.ACC_CODE,
                DM.CONTACT_NO AS P_CONTACT_NO, DM.REG_OFFICE_ADDRESS AS ADDRESS,
                RM.ROUTE_CODE, TO_CHAR(RD.ASSIGN_DATE,'DD-MON-RRRR') AS ASSIGN_DATE, RM.ROUTE_NAME,
                AM.AREA_CODE, AM.AREA_NAME,
                TO_CHAR(RE.ORDER_NO) AS ORDER_NO,
                RM.COMPANY_CODE,
                'dealer' AS TYPE,
                'N' AS WHOLESELLER,
                '' DEFAULT_PARTY_TYPE_CODE,
                '' PARENT_DISTRIBUTOR_CODE,
                '' PARENT_DISTRIBUTOR_NAME,
                NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'P' AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_DEALER_MASTER DM ON DM.DEALER_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN IP_PARTY_TYPE_CODE PT ON PT.PARTY_TYPE_CODE = DM.DEALER_CODE AND PT.COMPANY_CODE = DM.COMPANY_CODE
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS, 
                          (CASE 
                            WHEN A.IS_VISITED IS NULL THEN 'X' 
                              ELSE
                                CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                           END
                          ) IS_VISITED, 
                          A.REMARKS,
                          TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                          FROM DIST_LOCATION_TRACK A
                          INNER JOIN IP_PARTY_TYPE_CODE B ON B.PARTY_TYPE_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                          INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                          WHERE 1 = 1
                          -- AND SP_CODE = '1000097' -- Commented out because customer can be visited by another SP_CODE
                          AND A.CUSTOMER_TYPE = 'P'
                          AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                              (CASE 
                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                  ELSE
                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                               END
                              ), 
                              A.REMARKS
                          ) LT
                          ON LT.CUSTOMER_CODE = DM.DEALER_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
                WHERE 1 = 1
                AND TO_CHAR(RD.ASSIGN_DATE, 'DD-MON-RRRR') = UPPER('{today}')
                AND RD.EMP_CODE = '{model.spcode}'
                AND RD.COMPANY_CODE = '{model.COMPANY_CODE}'
                )
              UNION
                (SELECT
                DM.DISTRIBUTOR_CODE AS CODE,DM.EMAIL,
                CS.CUSTOMER_EDESC AS NAME,
                '' AS ACC_CODE,
                DM.CONTACT_NO AS P_CONTACT_NO, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS) AS ADDRESS,
                RM.ROUTE_CODE, TO_CHAR(RD.ASSIGN_DATE,'DD-MON-RRRR') AS ASSIGN_DATE, RM.ROUTE_NAME,
                AM.AREA_CODE, AM.AREA_NAME,
                TO_CHAR(RE.ORDER_NO) AS ORDER_NO,
                RM.COMPANY_CODE,
                'distributor' AS TYPE,
                'N' AS WHOLESELLER,
                CS.PARTY_TYPE_CODE AS DEFAULT_PARTY_TYPE_CODE,
                '' PARENT_DISTRIBUTOR_CODE,
                '' PARENT_DISTRIBUTOR_NAME,
                NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_DISTRIBUTOR_MASTER DM ON DM.DISTRIBUTOR_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS, 
                          (CASE 
                            WHEN A.IS_VISITED IS NULL THEN 'X' 
                              ELSE
                                CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                           END
                          ) IS_VISITED, 
                          A.REMARKS,
                          TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                          FROM DIST_LOCATION_TRACK A
                          INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                          INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                          WHERE 1 = 1
                          -- AND SP_CODE = '1000097' -- Commented out because customer can be visited by another SP_CODE
                          AND A.CUSTOMER_TYPE = 'D'
                          AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                              (CASE 
                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                  ELSE
                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                               END
                              ), 
                              A.REMARKS
                          ) LT
                          ON LT.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
                WHERE 1 = 1
                AND TO_CHAR(RD.ASSIGN_DATE, 'DD-MON-RRRR') = UPPER('{today}')
                AND RD.EMP_CODE = '{model.spcode}'
                AND RD.COMPANY_CODE = '{model.COMPANY_CODE}'
                )
            UNION
                (SELECT
                RES.RESELLER_CODE AS CODE,RES.EMAIL,
                RES.RESELLER_NAME AS NAME,
                '' AS ACC_CODE,
                RES.CONTACT_NO AS P_CONTACT_NO, RES.REG_OFFICE_ADDRESS AS ADDRESS,
                RM.ROUTE_CODE, TO_CHAR(RD.ASSIGN_DATE,'DD-MON-RRRR') AS ASSIGN_DATE, RM.ROUTE_NAME,
                AM.AREA_CODE, AM.AREA_NAME,
                TO_CHAR(RE.ORDER_NO) AS ORDER_NO,
                RM.COMPANY_CODE,
                'reseller' AS TYPE,
                RES.WHOLESELLER AS WHOLESELLER,
                '' AS DEFAULT_PARTY_TYPE_CODE,
                DISTRIBUTOR_CODE AS PARENT_DISTRIBUTOR_CODE,
                CS.CUSTOMER_EDESC AS PARENT_DISTRIBUTOR_NAME,
                NVL(RES.LATITUDE,0) LATITUDE, NVL(RES.LONGITUDE,0) LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS               
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'R' AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_RESELLER_MASTER RES ON RES.RESELLER_CODE = RE.ENTITY_CODE AND RES.COMPANY_CODE = RM.COMPANY_CODE
                LEFT JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = RES.AREA_CODE AND AM.COMPANY_CODE = RM.COMPANY_CODE
                LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = RES.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = RM.COMPANY_CODE
                LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS, 
                          (CASE 
                            WHEN A.IS_VISITED IS NULL THEN 'X' 
                              ELSE
                                CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                           END
                          ) IS_VISITED, 
                          A.REMARKS,
                          TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                          FROM DIST_LOCATION_TRACK A
                          INNER JOIN DIST_RESELLER_MASTER B ON B.RESELLER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                          INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                          WHERE 1 = 1
                          AND A.CUSTOMER_TYPE = 'R'
                          AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                              (CASE 
                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                  ELSE
                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                               END
                              ), 
                              A.REMARKS
                          ) LT
                          ON LT.CUSTOMER_CODE = RES.RESELLER_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
                WHERE 1 = 1
                AND TO_CHAR(RD.ASSIGN_DATE, 'DD-MON-RRRR') = UPPER('{today}')
                AND RD.EMP_CODE = '{model.spcode}'
                AND RD.COMPANY_CODE = '{model.COMPANY_CODE}'
                )
            )
            ORDER BY UPPER(ROUTE_NAME), ORDER_NO, UPPER(AREA_NAME), UPPER(NAME), LAST_VISIT_DATE DESC";


            var Visitlist = dbContext.SqlQuery<VisitEntityModel>(VisitQuery).ToList();
            if (Visitlist.Count <= 0)
                throw new Exception("No records found");

            var RouteCode = Visitlist[0].Route_Code;
            var RouteName = Visitlist[0].Route_Name;
            foreach (var visit in Visitlist)
            {
                var code = visit.Code;
                VisitPlans.entity.Add(code, visit);
            }
            VisitPlans.code = RouteCode;
            Result.Add(RouteName, VisitPlans);

            return Result;
        }

        public Dictionary<string, List<EntityResponseModel>> FetchEntity(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, List<EntityResponseModel>>();
            string EntityQuery = string.Empty;
            EntityQuery = $@"SELECT * FROM (
                  (SELECT
                     DM.DEALER_CODE AS CODE,
                     PT.PARTY_TYPE_EDESC AS NAME,
                     PT.ACC_CODE,
                     DM.CONTACT_NO AS P_CONTACT_NO, '' AS P_CONTACT_NAME, DM.REG_OFFICE_ADDRESS AS ADDRESS,DM.EMAIL,
                     RM.ROUTE_CODE, RM.ROUTE_NAME,
                     AM.AREA_CODE, AM.AREA_NAME,
                     NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                     'dealer' AS TYPE,
                     'N' AS WHOLESELLER,
                     '' DEFAULT_PARTY_TYPE_CODE,
                     '' PARENT_DISTRIBUTOR_CODE,
                     '' PARENT_DISTRIBUTOR_NAME,
                     LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                     DM.COMPANY_CODE, DM.BRANCH_CODE,'' TYPE_EDESC,'' SUBTYPE_EDESC
                   FROM
                     DIST_DEALER_MASTER DM
                     LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = DM.DEALER_CODE AND RE.ENTITY_TYPE = 'P' AND RE.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN IP_PARTY_TYPE_CODE PT ON PT.PARTY_TYPE_CODE = DM.DEALER_CODE AND PT.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ) IS_VISITED, 
                                  A.REMARKS,
                                  TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                FROM DIST_LOCATION_TRACK A
                                  INNER JOIN IP_PARTY_TYPE_CODE B ON B.PARTY_TYPE_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                WHERE 1 = 1
                                      AND A.CUSTOMER_TYPE = 'P'
                                      AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ), 
                                  A.REMARKS
                               ) LT
                       ON LT.CUSTOMER_CODE = DM.DEALER_CODE AND LT.COMPANY_CODE = DM.COMPANY_CODE
                   WHERE 1 = 1
                         AND DM.COMPANY_CODE = '{model.COMPANY_CODE}'
                  )
                UNION
                  (SELECT
                     DM.DISTRIBUTOR_CODE AS CODE,
                     CS.CUSTOMER_EDESC AS NAME,
                     CS.ACC_CODE,
                     DM.CONTACT_NO AS P_CONTACT_NO, '' AS P_CONTACT_NAME, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS) AS ADDRESS,DM.EMAIL,
                     RM.ROUTE_CODE, RM.ROUTE_NAME,
                     AM.AREA_CODE, AM.AREA_NAME,
                     NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                     'distributor' AS TYPE,
                     'N' AS WHOLESELLER,
                     CS.PARTY_TYPE_CODE AS DEFAULT_PARTY_TYPE_CODE,
                     '' PARENT_DISTRIBUTOR_CODE,
                     '' PARENT_DISTRIBUTOR_NAME,
                     LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                     DM.COMPANY_CODE, DM.BRANCH_CODE,'' TYPE_EDESC,'' SUBTYPE_EDESC
                   FROM
                     DIST_DISTRIBUTOR_MASTER DM
                     LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = DM.DISTRIBUTOR_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ) IS_VISITED, 
                                  A.REMARKS,
                                  TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                FROM DIST_LOCATION_TRACK A
                                  INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                WHERE 1 = 1
                                      AND A.CUSTOMER_TYPE = 'D'
                                      AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ), 
                                  A.REMARKS
                               ) LT
                       ON LT.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND LT.COMPANY_CODE = DM.COMPANY_CODE
                   WHERE 1 = 1
                         AND DM.COMPANY_CODE = '{model.COMPANY_CODE}'
                  )
                  UNION
                  (SELECT
                     REM.RESELLER_CODE AS CODE,
                     REM.RESELLER_NAME AS NAME,
                     '' AS ACC_CODE,
                     REM.CONTACT_NO AS P_CONTACT_NO, REM.CONTACT_NAME AS P_CONTACT_NAME, REM.REG_OFFICE_ADDRESS AS ADDRESS,REM.EMAIL,
                     RM.ROUTE_CODE, RM.ROUTE_NAME,
                     AM.AREA_CODE, AM.AREA_NAME,
                     NVL(REM.LATITUDE,0) LATITUDE, NVL(REM.LONGITUDE,0) LONGITUDE,
                     'reseller' AS TYPE,
                     REM.WHOLESELLER AS WHOLESELLER,
                     '' AS DEFAULT_PARTY_TYPE_CODE,
                     DISTRIBUTOR_CODE AS PARENT_DISTRIBUTOR_CODE,
                     CS.CUSTOMER_EDESC AS PARENT_DISTRIBUTOR_NAME,
                     LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                     REM.COMPANY_CODE, REM.BRANCH_CODE,
                     DOT.TYPE_EDESC,DOS.SUBTYPE_EDESC
                   FROM DIST_RESELLER_MASTER REM

                     LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = REM.RESELLER_CODE AND RE.ENTITY_TYPE = 'R' AND RE.COMPANY_CODE = REM.COMPANY_CODE
                     LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = REM.COMPANY_CODE
                     INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = REM.AREA_CODE AND AM.COMPANY_CODE = REM.COMPANY_CODE
                     LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = REM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = REM.COMPANY_CODE
                     LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                 (CASE 
                                        WHEN A.IS_VISITED IS NULL THEN 'X' 
                                          ELSE
                                            CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                       END
                                      ) IS_VISITED, 
                                 A.REMARKS,
                                 TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                FROM DIST_LOCATION_TRACK A
                                  INNER JOIN DIST_RESELLER_MASTER B ON B.RESELLER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                WHERE 1 = 1
                                      AND A.CUSTOMER_TYPE = 'R'
                                      AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ), 
                                  A.REMARKS
                               ) LT
                       ON LT.CUSTOMER_CODE = REM.RESELLER_CODE AND LT.COMPANY_CODE = REM.COMPANY_CODE
               LEFT JOIN DIST_OUTLET_TYPE DOT ON REM.OUTLET_TYPE_ID=DOT.TYPE_ID AND REM.COMPANY_CODE=DOT.COMPANY_CODE
               LEFT JOIN DIST_OUTLET_SUBTYPE DOS ON REM.OUTLET_SUBTYPE_ID=DOS.SUBTYPE_ID AND REM.COMPANY_CODE=DOS.COMPANY_CODE
                   WHERE 1 = 1
                         AND REM.COMPANY_CODE = '{model.COMPANY_CODE}'
                  )
                )
                ORDER BY UPPER(NAME)";
            var Entities = dbContext.SqlQuery<EntityResponseModel>(EntityQuery).GroupBy(x => x.TYPE);
            if (Entities.Count() <= 0)
                throw new Exception("No records found");
            foreach (var EntGroup in Entities)
            {
                result.Add(EntGroup.Key, EntGroup.ToList());
            }
            return (result);
        }

        public Dictionary<string, List<EntityResponseModel>> FetchAllCompanyEntity(NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, List<EntityResponseModel>>();
            string EntityQuery = string.Empty;
            EntityQuery = $@"SELECT * FROM (
                  (SELECT
                     DM.DEALER_CODE AS CODE,
                     TRIM(PT.PARTY_TYPE_EDESC) AS NAME,
                     PT.ACC_CODE,
                     DM.CONTACT_NO AS P_CONTACT_NO, '' AS P_CONTACT_NAME, DM.REG_OFFICE_ADDRESS AS ADDRESS,
                     --RM.ROUTE_CODE, RM.ROUTE_NAME,
                     AM.AREA_CODE, AM.AREA_NAME,
                     NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                     'dealer' AS TYPE,
                     'N' AS WHOLESELLER,
                     '' DEFAULT_PARTY_TYPE_CODE,
                     '' PARENT_DISTRIBUTOR_CODE,
                     '' PARENT_DISTRIBUTOR_NAME,
                     LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                     DM.COMPANY_CODE, DM.BRANCH_CODE,'' TYPE_EDESC,'' SUBTYPE_EDESC
                     FROM DIST_DEALER_MASTER DM
                     LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = DM.DEALER_CODE AND RE.ENTITY_TYPE = 'P' AND RE.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN IP_PARTY_TYPE_CODE PT ON PT.PARTY_TYPE_CODE = DM.DEALER_CODE AND PT.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ) IS_VISITED, 
                                  A.REMARKS,
                                  TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                FROM DIST_LOCATION_TRACK A
                                  INNER JOIN IP_PARTY_TYPE_CODE B ON B.PARTY_TYPE_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                WHERE 1 = 1
                                      AND A.CUSTOMER_TYPE = 'P'
                                      AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ), 
                                  A.REMARKS
                               ) LT
                       ON LT.CUSTOMER_CODE = DM.DEALER_CODE AND LT.COMPANY_CODE = DM.COMPANY_CODE)
                UNION
                  (SELECT
                     DM.DISTRIBUTOR_CODE AS CODE,
                     TRIM(CS.CUSTOMER_EDESC) AS NAME,
                     CS.ACC_CODE,
                     DM.CONTACT_NO AS P_CONTACT_NO, '' AS P_CONTACT_NAME, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS) AS ADDRESS,
                     --RM.ROUTE_CODE, RM.ROUTE_NAME,
                     AM.AREA_CODE, AM.AREA_NAME,
                     NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                     'distributor' AS TYPE,
                     'N' AS WHOLESELLER,
                     CS.PARTY_TYPE_CODE AS DEFAULT_PARTY_TYPE_CODE,
                     '' PARENT_DISTRIBUTOR_CODE,
                     '' PARENT_DISTRIBUTOR_NAME,
                     LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                     DM.COMPANY_CODE, DM.BRANCH_CODE,'' TYPE_EDESC,'' SUBTYPE_EDESC
                     FROM DIST_DISTRIBUTOR_MASTER DM
                     LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = DM.DISTRIBUTOR_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ) IS_VISITED, 
                                  A.REMARKS,
                                  TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                FROM DIST_LOCATION_TRACK A
                                  INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                WHERE 1 = 1
                                      AND A.CUSTOMER_TYPE = 'D'
                                      AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ), 
                                  A.REMARKS
                               ) LT
                       ON LT.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND LT.COMPANY_CODE = DM.COMPANY_CODE)
                  UNION
                  (SELECT
                     REM.RESELLER_CODE AS CODE,
                     TRIM(REM.RESELLER_NAME) AS NAME,
                     '' AS ACC_CODE,
                     REM.CONTACT_NO AS P_CONTACT_NO, REM.CONTACT_NAME AS P_CONTACT_NAME, REM.REG_OFFICE_ADDRESS AS ADDRESS,
                     --RM.ROUTE_CODE, RM.ROUTE_NAME,
                     AM.AREA_CODE, AM.AREA_NAME,
                     NVL(REM.LATITUDE,0) LATITUDE, NVL(REM.LONGITUDE,0) LONGITUDE,
                     'reseller' AS TYPE,
                     REM.WHOLESELLER AS WHOLESELLER,
                     '' AS DEFAULT_PARTY_TYPE_CODE,
                     DISTRIBUTOR_CODE AS PARENT_DISTRIBUTOR_CODE,
                     CS.CUSTOMER_EDESC AS PARENT_DISTRIBUTOR_NAME,
                     LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                     REM.COMPANY_CODE, REM.BRANCH_CODE,
                     DOT.TYPE_EDESC,DOS.SUBTYPE_EDESC
                   FROM DIST_RESELLER_MASTER REM

                     LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = REM.RESELLER_CODE AND RE.ENTITY_TYPE = 'R' AND RE.COMPANY_CODE = REM.COMPANY_CODE
                     LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = REM.COMPANY_CODE
                     INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = REM.AREA_CODE AND AM.COMPANY_CODE = REM.COMPANY_CODE
                     LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = REM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = REM.COMPANY_CODE
                     LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                 (CASE 
                                        WHEN A.IS_VISITED IS NULL THEN 'X' 
                                          ELSE
                                            CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                       END
                                      ) IS_VISITED, 
                                 A.REMARKS,
                                 TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                FROM DIST_LOCATION_TRACK A
                                  INNER JOIN DIST_RESELLER_MASTER B ON B.RESELLER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                WHERE 1 = 1
                                      AND A.CUSTOMER_TYPE = 'R'
                                      AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ), 
                                  A.REMARKS
                               ) LT ON LT.CUSTOMER_CODE = REM.RESELLER_CODE AND LT.COMPANY_CODE = REM.COMPANY_CODE
               LEFT JOIN DIST_OUTLET_TYPE DOT ON REM.OUTLET_TYPE_ID=DOT.TYPE_ID AND REM.COMPANY_CODE=DOT.COMPANY_CODE
               LEFT JOIN DIST_OUTLET_SUBTYPE DOS ON REM.OUTLET_SUBTYPE_ID=DOS.SUBTYPE_ID AND REM.COMPANY_CODE=DOS.COMPANY_CODE)
                )
                ORDER BY TYPE";
            var Entities = dbContext.SqlQuery<EntityResponseModel>(EntityQuery).GroupBy(x => x.TYPE);
            if (Entities.Count() <= 0)
                throw new Exception("No records found");
            foreach (var EntGroup in Entities)
            {
                result.Add(EntGroup.Key, EntGroup.ToList());
            }
            return (result);
        }

        public List<ItemModel> FetchItems(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            var pref = FetchPreferences(model.COMPANY_CODE, dbContext);
            string salesClause = "", conversionClause = "";
            if ("Y" == pref.PO_SYN_RATE)
                salesClause = "AND SALES_RATE IS NOT NULL AND SALES_RATE <> 0";
            if ("Y" == pref.SQL_NN_CONVERSION_UNIT_FACTOR)
                conversionClause = "AND IUS.MU_CODE IS NOT NULL AND IUS.CONVERSION_FACTOR IS NOT NULL";

            string ItemsQuery = string.Empty;
            ItemsQuery = $@"SELECT IM.ITEM_CODE, IM.ITEM_EDESC, ISS.BRAND_NAME, IM.INDEX_MU_CODE AS UNIT, MC.MU_EDESC, IUS.MU_CODE CONVERSION_UNIT,
                TO_CHAR(IUS.CONVERSION_FACTOR) AS CONVERSION_FACTOR, TO_CHAR(NVL(IR.SALES_RATE, 0)) SALES_RATE, TO_CHAR(IR.APPLY_DATE) AS APPLY_DATE
                FROM IP_ITEM_MASTER_SETUP IM
                  INNER JOIN IP_MU_CODE MC ON MC.MU_CODE = IM.INDEX_MU_CODE AND MC.COMPANY_CODE = IM.COMPANY_CODE
                  INNER JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = IM.ITEM_CODE AND ISS.COMPANY_CODE = IM.COMPANY_CODE AND TRIM(ISS.BRAND_NAME) IS NOT NULL
                  LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON IUS.ITEM_CODE = ISS.ITEM_CODE AND IUS.COMPANY_CODE = ISS.COMPANY_CODE
                  LEFT JOIN (SELECT A.ITEM_CODE, A.APPLY_DATE, B.SALES_RATE, B.COMPANY_CODE
                              FROM (SELECT ITEM_CODE, COMPANY_CODE, MAX(APP_DATE) APPLY_DATE 
                                FROM IP_ITEM_RATE_APPLICAT_SETUP
                                WHERE COMPANY_CODE = '{model.COMPANY_CODE}' 
                                AND BRANCH_CODE = '{model.BRANCH_CODE}'
                                GROUP BY ITEM_CODE, COMPANY_CODE) A
                              INNER JOIN IP_ITEM_RATE_APPLICAT_SETUP B
                                ON B.ITEM_CODE = A.ITEM_CODE
                                AND B.APP_DATE = A.APPLY_DATE
                                AND B.COMPANY_CODE = '{model.COMPANY_CODE}'
                                AND B.BRANCH_CODE = '{model.BRANCH_CODE}') IR 
                    ON IR.ITEM_CODE = IM.ITEM_CODE AND IR.COMPANY_CODE = IM.COMPANY_CODE
                WHERE IM.COMPANY_CODE = '{model.COMPANY_CODE}' AND IM.CATEGORY_CODE = '{CATEGORY_CODE}' AND IM.GROUP_SKU_FLAG = '{GROUP_SKU_FLAG}' AND IM.DELETED_FLAG = 'N'
                {salesClause}
                {conversionClause}
                ORDER BY UPPER(IM.ITEM_EDESC) ASC";
            var Items = dbContext.SqlQuery<ItemModel>(ItemsQuery).ToList();
            if (Items.Count <= 0)
                throw new Exception("No records found");
            return Items;
        }

        public QuestionResponseModel FetchAllQuestions(QuestionRequestModel model, NeoErpCoreEntity dbContext)
        {
            QuestionResponseModel Result = new QuestionResponseModel();
            //general Questions
            var generalQuery = string.Empty;
            generalQuery = $@"SELECT TO_CHAR(A.QA_CODE) AS QA_CODE, A.QA_TYPE, A.QUESTION FROM DIST_QA_MASTER A
                    WHERE A.DELETED_FLAG = 'N' AND 
                    A.SET_CODE = (SELECT B.SET_CODE FROM DIST_QA_SET B WHERE B.COMPANY_CODE=A.COMPANY_CODE AND
                                B.QA_TYPE='{model.SetType}' AND
                                B.SET_CODE=(SELECT C.SET_CODE FROM DIST_QA_SET_SALESPERSON_MAP C WHERE C.SP_CODE='{model.sp_code}' AND 
                                C.COMPANY_CODE=B.COMPANY_CODE AND C.DELETED_FLAG='N'))
                    AND A.COMPANY_CODE='{model.COMPANY_CODE}'";
            var generalQuestion = dbContext.SqlQuery<GeneralModel>(generalQuery).ToList();
            for (int i = 0; i < generalQuestion.Count; i++)
            {
                string AnswerQuery = string.Empty;
                string[] types = { "MCR", "MCC" };
                if (types.Contains(generalQuestion[i].QA_TYPE))
                {
                    AnswerQuery = $@"SELECT ANSWERS FROM DIST_QA_DETAIL WHERE QA_CODE='{generalQuestion[i].QA_CODE}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
                    var answers = dbContext.SqlQuery<string>(AnswerQuery).ToList();
                    generalQuestion[i].ANSWERS = answers;
                }
            }

            //tabular questions
            string TabularQuery = string.Empty;
            TabularQuery = $@"SELECT TBL.*, TC.CELL_ID, TC.ROW_NO, TC.CELL_NO, TC.CELL_TYPE, TC.CELL_LABEL
                FROM DIST_QA_TAB_TABLE TBL
                LEFT JOIN DIST_QA_TAB_CELL TC ON TBL.TABLE_ID = TC.TABLE_ID
                WHERE TBL.COMPANY_CODE = '{model.COMPANY_CODE}'
                ORDER BY TBL.TABLE_ID, TC.ROW_NO, TC.CELL_NO";
            var tempData = dbContext.SqlQuery<TabularTemp>(TabularQuery).GroupBy(x => x.TABLE_ID);
            var tabularQuestions = new Dictionary<string, TabularModel>();
            foreach (var table in tempData)
            {
                var tables = table.ToList();
                var resultTable = new TabularModel
                {
                    TABLE_ID = tables[0].TABLE_ID.ToString(),
                    TABLE_TITLE = tables[0].TABLE_TITLE,
                    CREATED_DATE = tables[0].CREATED_DATE == null ? null : tables[0].CREATED_DATE.Value.ToString("dd-MMM-yy"),
                    DELETED_FLAG = tables[0].DELETED_FLAG
                };
                var groupTables = tables.GroupBy(x => x.ROW_NO);
                foreach (var rowGroup in groupTables)
                {
                    var CellGroup = (from r in rowGroup
                                     select new TabularCellModel
                                     {
                                         CELL_ID = r.CELL_ID.ToString(),
                                         CELL_LABEL = r.CELL_LABEL,
                                         CELL_NO = r.CELL_NO.ToString(),
                                         CELL_TYPE = r.CELL_TYPE,
                                         ROW_NO = r.ROW_NO.ToString()
                                     }).ToList();
                    resultTable.CELL_DATA.Add(CellGroup);
                }
                tabularQuestions.Add(table.Key.ToString(), resultTable);
            }
            Result.general = generalQuestion;
            Result.tabular = tabularQuestions;
            if (Result.general.Count <= 0 && Result.tabular.Count <= 0)
                throw new Exception("No records found");
            return Result;
        }

        public List<AreaResponseModel> FetchArea(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            var AreaQuery = $@"SELECT A.AREA_CODE,A.AREA_NAME,A.ZONE_CODE,A.DISTRICT_CODE,A.VDC_CODE,A.REG_CODE,
                       B.ZONE_NAME,B.DISTRICT_NAME,B.VDC_NAME,B.REG_NAME
                FROM DIST_AREA_MASTER A,DIST_ADDRESS_MASTER B
                WHERE (A.DISTRICT_CODE = B.DISTRICT_CODE
                AND  A.REG_CODE = B.REG_CODE
                AND  A.ZONE_CODE = B.ZONE_CODE
                AND  A.VDC_CODE = B.VDC_CODE) AND A.COMPANY_CODE = '{model.COMPANY_CODE}' ORDER BY UPPER(A.AREA_NAME) ASC";

            var result = dbContext.SqlQuery<AreaResponseModel>(AreaQuery).ToList();
            if (result.Count <= 0)
                throw new Exception("No records found");
            return result;
        }

        public Dictionary<string, OutletResponseModel> FetchOutlets(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, OutletResponseModel>();
            string OutletsQuery = $@"SELECT TO_CHAR(T.TYPE_ID) AS TYPE_ID, T.TYPE_CODE, T.TYPE_EDESC, T.DELETED_FLAG, TO_CHAR(NVL(ST.SUBTYPE_ID,0)) AS SUBTYPE_ID, ST.SUBTYPE_CODE, ST.SUBTYPE_EDESC FROM DIST_OUTLET_TYPE T
                LEFT JOIN DIST_OUTLET_SUBTYPE ST ON ST.TYPE_ID = T.TYPE_ID AND ST.COMPANY_CODE = T.COMPANY_CODE
                WHERE T.COMPANY_CODE='{model.COMPANY_CODE}' AND  T.DELETED_FLAG='N'
                ORDER BY UPPER(T.TYPE_EDESC), ST.SUBTYPE_EDESC ASC";
            var tempOutlets = dbContext.SqlQuery<OutletTemp>(OutletsQuery).GroupBy(x => x.TYPE_ID);
            if (tempOutlets.Count() <= 0)
                throw new Exception("No records found");
            foreach (var typeGroup in tempOutlets)
            {
                var outlet = new OutletResponseModel();
                foreach (var subType in typeGroup)
                {
                    var SubObj = new SubTypeModel
                    {
                        SUBTYPE_ID = subType.SUBTYPE_ID,
                        SUBTYPE_CODE = subType.SUBTYPE_CODE,
                        SUBTYPE_EDESC = subType.SUBTYPE_EDESC,
                        TYPE_ID = typeGroup.Key
                    };
                    outlet.SIZE.Add(subType.SUBTYPE_ID, SubObj);
                }
                var typeObjTemp = typeGroup.FirstOrDefault();
                outlet.TYPE_ID = typeObjTemp.TYPE_ID;
                outlet.TYPE_CODE = typeObjTemp.TYPE_CODE;
                outlet.TYPE_EDESC = typeObjTemp.TYPE_EDESC;
                outlet.DELETED_FLAG = typeObjTemp.DELETED_FLAG;

                result.Add(typeGroup.Key, outlet);
            }
            return result;
        }

        public ClosingStockResponseModel GetEntityItemByBrand(ClosingStockRequestModel model, NeoErpCoreEntity dbContext)
        {
            ClosingStockResponseModel result = new ClosingStockResponseModel();
            var ItemsQuery = string.Empty;
            if (model.entity_type.ToUpper() == "R" || model.entity_type.ToUpper() == "RESELLER")
            {
                ItemsQuery = $@"SELECT A.BRAND_NAME,
                  B.ITEM_CODE,
                  B.ITEM_EDESC,
                  B.INDEX_MU_CODE,
                  CONCAT(CONCAT(NVL(C.LVS, 0), ' '), NVL(B.INDEX_MU_CODE, C.MU_CODE)) AS LVS
                FROM IP_ITEM_SPEC_SETUP A
                  INNER JOIN IP_ITEM_MASTER_SETUP B
                    ON B.ITEM_CODE = A.ITEM_CODE
                       AND B.CATEGORY_CODE = '{CATEGORY_CODE}'
                       AND B.GROUP_SKU_FLAG = '{GROUP_SKU_FLAG}'
                       AND B.DELETED_FLAG = A.DELETED_FLAG
                       AND A.COMPANY_CODE = B.COMPANY_CODE
                  LEFT JOIN (
                    SELECT TO_CHAR(T.CREATED_DATE, 'DD-MON-RRRR') CREATED_DATE, T.RESELLER_CODE, T.ITEM_CODE, T.MU_CODE, T.CURRENT_STOCK AS LVS
                    FROM DIST_RESELLER_STOCK T
                    WHERE T.RESELLER_CODE='{model.entity_code}'
                          AND T.CREATED_DATE = (SELECT MAX(CREATED_DATE) FROM DIST_RESELLER_STOCK WHERE RESELLER_CODE='{model.entity_code}' AND DELETED_FLAG='N' AND COMPANY_CODE='{model.COMPANY_CODE}')
                    ) C
                      ON C.ITEM_CODE = B.ITEM_CODE
                WHERE TRIM(A.BRAND_NAME) IS NOT NULL
                      AND A.COMPANY_CODE = '{model.COMPANY_CODE}'
                      AND A.DELETED_FLAG = 'N'
                ORDER BY UPPER(A.BRAND_NAME), UPPER(B.ITEM_EDESC) ASC";
            }
            else if (model.entity_type.ToUpper() == "D" || model.entity_type.ToUpper() == "DISTRIBUTOR")
            {
                ItemsQuery = $@"SELECT A.BRAND_NAME,
                  B.ITEM_CODE,
                  B.ITEM_EDESC,
                  B.INDEX_MU_CODE,
                  CONCAT(CONCAT(NVL(C.LVS, 0), ' '), NVL(B.INDEX_MU_CODE, C.MU_CODE)) AS LVS
                FROM IP_ITEM_SPEC_SETUP A
                  INNER JOIN IP_ITEM_MASTER_SETUP B
                    ON A.ITEM_CODE = B.ITEM_CODE
                       AND B.CATEGORY_CODE = '{CATEGORY_CODE}'
                       AND B.GROUP_SKU_FLAG = '{GROUP_SKU_FLAG}'
                       AND B.DELETED_FLAG = A.DELETED_FLAG
                       AND A.COMPANY_CODE = B.COMPANY_CODE
                  LEFT JOIN (
                    SELECT TO_CHAR(T.CREATED_DATE, 'DD-MON-RRRR') CREATED_DATE, T.DISTRIBUTOR_CODE, T.ITEM_CODE, T.MU_CODE, T.CURRENT_STOCK AS LVS
                    FROM DIST_DISTRIBUTOR_STOCK T
                    WHERE T.DISTRIBUTOR_CODE='{model.entity_type}'
                          AND T.CREATED_DATE = (SELECT MAX(CREATED_DATE) FROM DIST_DISTRIBUTOR_STOCK WHERE DISTRIBUTOR_CODE='{model.entity_type}' AND DELETED_FLAG='N' AND COMPANY_CODE='{model.COMPANY_CODE}')
                    ) C
                      ON C.ITEM_CODE = B.ITEM_CODE
                WHERE TRIM(A.BRAND_NAME) IS NOT NULL
                      AND A.COMPANY_CODE = '{model.COMPANY_CODE}'
                      AND A.DELETED_FLAG = 'N'
                      --AND UPPER(A.BRAND_NAME) = 'SUJI'
                ORDER BY UPPER(A.BRAND_NAME), UPPER(B.ITEM_EDESC) ASC";
            }
            else
                throw new Exception("Entity type not specified");
            var items = dbContext.SqlQuery<ClosingStockItemModel>(ItemsQuery).GroupBy(x => x.BRAND_NAME);

            foreach (var brand in items)
            {
                var temp = new Dictionary<string, ClosingStockItemModel>();
                foreach (var item in brand)
                {
                    temp.Add(item.ITEM_CODE, item);
                }
                result.item.Add(brand.Key, temp);
            }
            if (result.item.Count <= 0)
                throw new Exception("No records found");
            result.mu_code = this.FetchMU(model, dbContext);
            return result;
        }

        public Dictionary<string, Dictionary<string, MuCodeResponseModel>> FetchMU(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            Dictionary<string, MuCodeResponseModel> result = new Dictionary<string, MuCodeResponseModel>();
            var pref = FetchPreferences(model.COMPANY_CODE, dbContext);
            var conversionClause = "";
            if ("Y" == pref.SQL_NN_CONVERSION_UNIT_FACTOR)
            {
                conversionClause = "AND IUS.MU_CODE IS NOT NULL AND IUS.CONVERSION_FACTOR IS NOT NULL";
            }
            string MuQuery = $@"SELECT IMS.ITEM_CODE, IMS.ITEM_EDESC, IMS.INDEX_MU_CODE, IUS.MU_CODE, TO_CHAR(IUS.CONVERSION_FACTOR) AS CONVERSION_FACTOR
                FROM IP_ITEM_MASTER_SETUP IMS 
                LEFT JOIN IP_ITEM_UNIT_SETUP IUS
                ON IUS.ITEM_CODE = IMS.ITEM_CODE AND IUS.COMPANY_CODE = IMS.COMPANY_CODE
                WHERE 1 = 1
                AND IMS.COMPANY_CODE = '{model.COMPANY_CODE}'
                AND IMS.CATEGORY_CODE = 'FG'
                AND IMS.GROUP_SKU_FLAG = 'I'
                AND IMS.DELETED_FLAG = 'N'
                {conversionClause}
                ORDER BY UPPER(IMS.ITEM_EDESC), UPPER(IMS.INDEX_MU_CODE), UPPER(MU_CODE)";
            var allMu = dbContext.SqlQuery<MuCodeResponseModel>(MuQuery);
            foreach (var Mu in allMu)
            {
                if (Mu.MU_CODE != null)
                    Mu.CONVERSION_UNIT_FACTOR.Add(Mu.MU_CODE, Mu.CONVERSION_FACTOR);
                result.Add(Mu.ITEM_CODE, Mu);
            }
            var finalResult = new Dictionary<string, Dictionary<string, MuCodeResponseModel>>();
            finalResult.Add("UNIT", result);
            return finalResult;
        }

        public List<TransactionResponseModel> FetchTransactions(TransactionRequestModel model, NeoErpCoreEntity dbContext)
        {
            var result = new List<TransactionResponseModel>();
            DateTime fromDate, toDate;
            if (!DateTime.TryParseExact(model.from_date, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate) || !DateTime.TryParseExact(model.to_date, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out toDate))
                throw new Exception("Invalid Date");

            //opening balance
            string OpeningQuery = string.Empty;
            string NonOpeningQuery = string.Empty;
            model.COMPANY_CODE = model.COMPANY_CODE.Replace(" ", string.Empty);
            model.COMPANY_CODE = model.COMPANY_CODE.Replace(",", "','");
            model.BRANCH_CODE = model.BRANCH_CODE.Replace(" ", string.Empty);
            model.BRANCH_CODE = model.BRANCH_CODE.Replace(",", "','");

            if (!string.IsNullOrWhiteSpace(model.acc_code))
            {
                OpeningQuery = $@"SELECT VGL.VOUCHER_NO, TO_CHAR(VGL.VOUCHER_DATE, 'DD-MON-RRRR') VOUCHER_DATE, VGL.PARTICULARS, TO_CHAR(NVL(VGL.DR_AMOUNT, 0)) DR_AMOUNT, TO_CHAR(NVL(VGL.CR_AMOUNT, 0)) CR_AMOUNT, VGL.TRANSACTION_TYPE
                    FROM V$VIRTUAL_GENERAL_LEDGER VGL
                    WHERE 1=1
                    AND VGL.ACC_CODE = '{model.acc_code}'
                    AND VGL.VOUCHER_DATE < '{fromDate.ToString("dd-MMM-yyyy")}'
                    AND VGL.COMPANY_CODE IN ('{model.COMPANY_CODE}')
                    AND VGL.BRANCH_CODE IN ('{model.BRANCH_CODE}')
                    AND VGL.DELETED_FLAG = 'N'
                    ORDER BY VGL.VOUCHER_DATE, UPPER(VGL.VOUCHER_NO) ASC";

                //NonOpeningQuery = $@"SELECT VGL.VOUCHER_NO, TO_CHAR(VGL.VOUCHER_DATE, 'DD-MON-RRRR') VOUCHER_DATE, VGL.PARTICULARS,
                //    TO_CHAR(NVL(VGL.DR_AMOUNT, 0)) DR_AMOUNT, TO_CHAR(NVL(VGL.CR_AMOUNT, 0)) CR_AMOUNT, VGL.TRANSACTION_TYPE
                //    FROM V$VIRTUAL_GENERAL_LEDGER1 VGL
                //    WHERE 1=1
                //    AND VGL.ACC_CODE = '{model.acc_code}'
                //    AND VGL.VOUCHER_DATE BETWEEN '{fromDate.ToString("dd-MMM-yyyy")}' AND '{toDate.ToString("dd-MMM-yyyy")}'
                //    AND VGL.COMPANY_CODE = '{model.COMPANY_CODE}'
                //    ORDER BY VGL.VOUCHER_DATE, UPPER(VGL.VOUCHER_NO) ASC";
                NonOpeningQuery = $@"SELECT VGL.VOUCHER_NO, TO_CHAR(VGL.VOUCHER_DATE, 'DD-MON-RRRR') VOUCHER_DATE, VGL.PARTICULARS, TO_CHAR(NVL(VGL.DR_AMOUNT, 0)) DR_AMOUNT, TO_CHAR(NVL(VGL.CR_AMOUNT, 0)) CR_AMOUNT, VGL.TRANSACTION_TYPE
                    FROM V$VIRTUAL_GENERAL_LEDGER VGL
                    WHERE 1=1
                    AND VGL.ACC_CODE = '{model.acc_code}'
                    AND TRUNC(VGL.VOUCHER_DATE) BETWEEN TO_DATE('{fromDate.ToString("dd-MMM-yyyy")}','DD-MON-RRRR') AND TO_DATE('{toDate.ToString("dd-MMM-yyyy")}','DD-MON-RRRR')
                    AND VGL.COMPANY_CODE IN ('{model.COMPANY_CODE}')
                    AND VGL.BRANCH_CODE IN ('{model.BRANCH_CODE}')
                    AND VGL.DELETED_FLAG = 'N'
                    ORDER BY VGL.VOUCHER_DATE, UPPER(VGL.VOUCHER_NO) ASC";
            }
            else
            {
                //OpeningQuery = $@"SELECT VSL.VOUCHER_NO, TO_CHAR(VSL.VOUCHER_DATE, 'DD-MON-RRRR') VOUCHER_DATE, VSL.PARTICULARS,
                //    TO_CHAR(NVL(VSL.DR_AMOUNT, 0)) DR_AMOUNT, TO_CHAR(NVL(VSL.CR_AMOUNT, 0)) CR_AMOUNT, VSL.TRANSACTION_TYPE
                //    FROM V$VIRTUAL_SUB_LEDGER VSL
                //    WHERE 1=1
                //    AND VSL.SUB_CODE = '{model.sub_code}'
                //    AND VSL.VOUCHER_DATE < '{fromDate.ToString("dd-MMM-yyyy")}'
                //    AND VSL.COMPANY_CODE = '{model.COMPANY_CODE}'
                //    ORDER BY VSL.VOUCHER_DATE, UPPER(VSL.VOUCHER_NO) ASC";
                OpeningQuery = $@"SELECT VSL.VOUCHER_NO, TO_CHAR(VSL.VOUCHER_DATE, 'DD-MON-RRRR') VOUCHER_DATE, VSL.PARTICULARS, TO_CHAR(NVL(VSL.DR_AMOUNT, 0)) DR_AMOUNT, TO_CHAR(NVL(VSL.CR_AMOUNT, 0)) CR_AMOUNT, VSL.TRANSACTION_TYPE
                    FROM V$VIRTUAL_SUB_LEDGER VSL
                    WHERE 1 = 1
                    AND VSL.SUB_CODE = '{model.sub_code}'
                    AND VSL.VOUCHER_DATE < '{fromDate.ToString("dd-MMM-yyyy")}'
                    AND VSL.COMPANY_CODE IN ('{model.COMPANY_CODE}')
                    AND VSL.BRANCH_CODE IN ('{model.BRANCH_CODE}')
                    AND VSL.DELETED_FLAG = 'N'
                    ORDER BY VSL.VOUCHER_DATE, UPPER(VSL.VOUCHER_NO) ASC";

                //NonOpeningQuery = $@"SELECT VSL.VOUCHER_NO, TO_CHAR(VSL.VOUCHER_DATE, 'DD-MON-RRRR') VOUCHER_DATE, VSL.PARTICULARS,
                //    TO_CHAR(NVL(VSL.DR_AMOUNT, 0)) DR_AMOUNT, TO_CHAR(NVL(VSL.CR_AMOUNT, 0)) CR_AMOUNT, VSL.TRANSACTION_TYPE
                //    FROM V$VIRTUAL_SUB_LEDGER VSL
                //    WHERE 1=1
                //    AND VSL.SUB_CODE = '{model.sub_code}'
                //    AND VSL.VOUCHER_DATE BETWEEN '{fromDate.ToString("dd-MMM-yyyy")}' AND '{toDate.ToString("dd-MMM-yyyy")}'
                //    AND VSL.COMPANY_CODE = '{model.COMPANY_CODE}'
                //    ORDER BY VSL.VOUCHER_DATE, UPPER(VSL.VOUCHER_NO) ASC";

                NonOpeningQuery = $@"SELECT VSL.VOUCHER_NO, TO_CHAR(VSL.VOUCHER_DATE, 'DD-MON-RRRR') VOUCHER_DATE, VSL.PARTICULARS, TO_CHAR(NVL(VSL.DR_AMOUNT, 0)) DR_AMOUNT, TO_CHAR(NVL(VSL.CR_AMOUNT, 0)) CR_AMOUNT, VSL.TRANSACTION_TYPE
                    FROM V$VIRTUAL_SUB_LEDGER VSL
                    WHERE 1=1
                    AND VSL.SUB_CODE = '{model.sub_code}'
                    AND TRUNC(VSL.VOUCHER_DATE) BETWEEN TO_DATE('{fromDate.ToString("dd-MMM-yyyy")}','DD-MON-RRRR') AND TO_DATE('{toDate.ToString("dd-MMM-yyyy")}','DD-MON-RRRR')
                    AND VSL.COMPANY_CODE IN ('{model.COMPANY_CODE}')
                    AND VSL.BRANCH_CODE IN ('{model.BRANCH_CODE}')
                    AND VSL.DELETED_FLAG = 'N'
                    ORDER BY VSL.VOUCHER_DATE, UPPER(VSL.VOUCHER_NO) ASC";
            }
            var openingData = dbContext.SqlQuery<TransactionResponseModel>(OpeningQuery).ToList();
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
            var OpeningObject = new TransactionResponseModel
            {
                VOUCHER_NO = "0",
                PARTICULARS = "Opening Balance",
                VOUCHER_DATE = fromDate.ToString("dd-MMM-yyyy")
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
            result = dbContext.SqlQuery<TransactionResponseModel>(NonOpeningQuery).ToList();
            var itemsToRemove = new List<TransactionResponseModel>();
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
            var OpeningObject2 = new TransactionResponseModel
            {
                VOUCHER_NO = "0",
                PARTICULARS = "Opening Balance",
                VOUCHER_DATE = fromDate.ToString("dd-MMM-yyyy")
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
            if (result.Count <= 0)
                throw new Exception("No records found");
            return result;
        }

        public List<MoveTransactionResponseModel> FetchMovementTransactions(TransactionRequestModel model, NeoErpCoreEntity dbContext)
        {
            var result = new List<MoveTransactionResponseModel>();
            DateTime fromDate, toDate;
            if (!DateTime.TryParseExact(model.from_date, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate) || !DateTime.TryParseExact(model.to_date, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out toDate))
                throw new Exception("Invalid Date");

          

            //opening balance
            string OpeningQuery = string.Empty;
            string NonOpeningQuery = string.Empty;
            model.COMPANY_CODE = model.COMPANY_CODE.Replace(" ", string.Empty);
            model.COMPANY_CODE = model.COMPANY_CODE.Replace(",", "','");
            model.BRANCH_CODE = model.BRANCH_CODE.Replace(" ", string.Empty);
            model.BRANCH_CODE = model.BRANCH_CODE.Replace(",", "','");
            if (string.IsNullOrWhiteSpace(model.acc_code))
                throw new Exception("No records found");

            //var query = $@"SELECT DISTINCT SUB_CODE,SUB_EDESC, VOUCHER_NO,MANUAL_NO,TO_CHAR(VOUCHER_DATE) AS VOUCHER_DATE, CREDIT_LIMIT, CREDIT_DAYS, VOUCHER_DATE+CREDIT_DAYS DUE_DATE,
            //                 COALESCE( SUM(DR_AMOUNT - CR_AMOUNT) OVER (PARTITION BY SUB_EDESC ORDER BY VOUCHER_DATE
            //                     RANGE BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0 )       AS OP_BAL,
            //                 SUM(DR_AMOUNT) OVER (PARTITION BY SUB_EDESC, VOUCHER_DATE)                  AS DAILY_DR,
            //                 SUM(CR_AMOUNT) OVER (PARTITION BY SUB_EDESC, VOUCHER_DATE)                  AS DAILY_CR,
            //                 SUM(DR_AMOUNT - CR_AMOUNT) OVER (PARTITION BY SUB_EDESC ORDER BY VOUCHER_DATE) AS CL_BAL
            //            FROM     V$VIRTUAL_SUB_DEALER_LEDGER
            //        WHERE 1=1
            //        AND COMPANY_CODE IN ('{model.COMPANY_CODE}')
            //        AND  TRUNC(VOUCHER_DATE) BETWEEN TO_DATE('{fromDate.ToString("dd-MMM-yyyy")}','DD-MON-RRRR') AND TO_DATE('{toDate.ToString("dd-MMM-yyyy")}','DD-MON-RRRR')
            //        AND SUB_CODE in (select distinct link_sub_code from sa_customer_setup  where deleted_flag='N' and company_code in ('{model.COMPANY_CODE}') and deleted_flag='N' and group_sku_flag='I'  and customer_code='{model.acc_code}')
            //        ORDER BY SUB_EDESC, VOUCHER_DATE";
            //var query = $@"SELECT * FROM M$V_MOVEMENT_ANALYSIS WHERE 'C'|| CUSTOMER_CODE ='{model.acc_code}' AND COMPANY_CODE IN('{model.COMPANY_CODE}')";
            //var query1 = $@"SELECT VOUCHER_NO,VOUCHER_DATE,CREDIT_LIMIT,CREDIT_DAYS,DUE_DAYS,SALES_AMT,REC_AMT,BALANCE FROM M$V_MOVEMENT_ANALYSIS WHERE CUSTOMER_CODE='2836'";
            var query1 = $@"SELECT CUSTOMER_EDESC,VOUCHER_NO,VOUCHER_DATE,CREDIT_LIMIT,CREDIT_DAYS,DUE_DAYS,SALES_AMT,REC_AMT,BALANCE FROM M$V_MOVEMENT_ANALYSIS WHERE 'C' || CUSTOMER_CODE ='{model.sub_code}' AND COMPANY_CODE IN('{model.COMPANY_CODE}')";
            result= dbContext.SqlQuery<MoveTransactionResponseModel>(query1).ToList();




            if (result.Count <= 0)
                throw new Exception("No records found");
            return result;
        }

        public Dictionary<string, List<PurchaseOrderResponseModel>> FetchPurchaseOrder(PurchaseOrderRequestModel model, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, List<PurchaseOrderResponseModel>>();
            string PurchaseQuery = string.Empty;
            string DateQuery = string.Empty;

            DateQuery = $@"select FY_START_DATE from PREFERENCE_SETUP where company_code = '{model.COMPANY_CODE}'";
            var startDate = dbContext.SqlQuery<DateTime>(DateQuery).FirstOrDefault();
            var endDate = DateTime.Now;
            if (model.type.Equals("distributor", StringComparison.OrdinalIgnoreCase))
                PurchaseQuery = $@"select a.distributor_code as code,TO_CHAR(c.order_no) order_no, to_char(c.order_date, 'DD-MON-YYYY') as order_date, c.item_code, b.item_edesc, c.mu_code,
             TO_CHAR(c.quantity) quantity, TO_CHAR(c.unit_price) unit_price,TO_CHAR(c.total_price) total_price, c.remarks, c.approved_flag, c.dispatch_flag, c.acknowledge_flag,
             c.reject_flag, TO_CHAR(NVL(IR.SALES_RATE, 0)) SALES_RATE, To_CHAR(IR.APPLY_DATE) APPLY_DATE
                    from dist_distributor_master a, ip_item_master_setup b, dist_ip_ssd_purchase_order c,
                    (SELECT A.ITEM_CODE, A.APPLY_DATE, B.SALES_RATE, B.COMPANY_CODE
                              FROM (SELECT ITEM_CODE, COMPANY_CODE, MAX(APP_DATE) APPLY_DATE 
                                FROM IP_ITEM_RATE_APPLICAT_SETUP
                                WHERE COMPANY_CODE = '{model.COMPANY_CODE}' 
                                AND BRANCH_CODE = '{model.BRANCH_CODE}'
                                GROUP BY ITEM_CODE, COMPANY_CODE) A
                              INNER JOIN IP_ITEM_RATE_APPLICAT_SETUP B
                                ON B.ITEM_CODE = A.ITEM_CODE
                                AND B.APP_DATE = A.APPLY_DATE
                                AND B.COMPANY_CODE = '{model.COMPANY_CODE}'
                                AND B.BRANCH_CODE = '{model.BRANCH_CODE}') IR
                    where a.distributor_code = '{model.code}'
                    and a.distributor_code = c.customer_code
                    and b.item_code = c.item_code
                    and b.group_sku_flag='{GROUP_SKU_FLAG}'
                    and b.category_code='{CATEGORY_CODE}'
                    and TRUNC(c.order_date) between TO_DATE('{startDate.ToString("dd-MMM-yyyy")}','DD-MON-RRRR') and TO_DATE('{endDate.ToString("dd-MMM-yyyy")}','DD-MON-RRRR')
                    and a.company_code='{model.COMPANY_CODE}'
                    and a.company_code=b.company_code
                    and b.company_code=c.company_code
                    AND IR.ITEM_CODE(+) = c.ITEM_CODE AND IR.COMPANY_CODE(+) = c.COMPANY_CODE
                    order by c.order_no DESC";
            else if (model.type.Equals("reseller", StringComparison.OrdinalIgnoreCase))
                PurchaseQuery = $@"select a.distributor_code as code,TO_CHAR(c.order_no) order_no, to_char(c.order_date, 'DD-MON-YYYY') as order_date, c.item_code, b.item_edesc, c.mu_code,
             TO_CHAR(c.quantity) quantity, TO_CHAR(c.unit_price) unit_price,TO_CHAR(c.total_price) total_price, c.remarks, c.approved_flag, c.dispatch_flag, c.acknowledge_flag,
             c.reject_flag, TO_CHAR(NVL(IR.SALES_RATE, 0)) SALES_RATE, To_CHAR(IR.APPLY_DATE) APPLY_DATE
                    from dist_reseller_master a, ip_item_master_setup b, dist_ip_ssr_purchase_order c,
                    (SELECT A.ITEM_CODE, A.APPLY_DATE, B.SALES_RATE, B.COMPANY_CODE
                              FROM (SELECT ITEM_CODE, COMPANY_CODE, MAX(APP_DATE) APPLY_DATE 
                                FROM IP_ITEM_RATE_APPLICAT_SETUP
                                WHERE COMPANY_CODE = '{model.COMPANY_CODE}' 
                                AND BRANCH_CODE = '{model.BRANCH_CODE}'
                                GROUP BY ITEM_CODE, COMPANY_CODE) A
                              INNER JOIN IP_ITEM_RATE_APPLICAT_SETUP B
                                ON B.ITEM_CODE = A.ITEM_CODE
                                AND B.APP_DATE = A.APPLY_DATE
                                AND B.COMPANY_CODE = '{model.COMPANY_CODE}'
                                AND B.BRANCH_CODE = '{model.BRANCH_CODE}') IR
                    where a.reseller_code = '{model.code}'
                    and a.reseller_code = c.reseller_code
                    and b.item_code = c.item_code
                    and b.group_sku_flag='{GROUP_SKU_FLAG}'
                    and b.category_code='{CATEGORY_CODE}'
                    and TRUNC(c.order_date) between TO_DATE('{startDate.ToString("dd-MMM-yyyy")}','DD-MON-RRRR') and TO_DATE('{endDate.ToString("dd-MMM-yyyy")}','DD-MON-RRRR')
                    and a.company_code='{model.COMPANY_CODE}'
                    and a.company_code=b.company_code
                    and b.company_code=c.company_code
                    AND IR.ITEM_CODE(+) = c.ITEM_CODE AND IR.COMPANY_CODE(+) = c.COMPANY_CODE
                    order by c.order_no DESC";
            else
                throw new Exception("Type Not specified");

            var data = dbContext.SqlQuery<PurchaseOrderResponseModel>(PurchaseQuery).ToList();
            if (data.Count <= 0)
                throw new Exception("No records found");
            var groups = data.GroupBy(x => x.ORDER_NO);
            foreach (var group in groups)
            {
                result.Add(group.Key, group.ToList());
            }
            //foreach (var item in data)
            //{
            //    var list = new List<PurchaseOrderResponseModel>();
            //    list.Add(item);
            //    result.Add(item.ORDER_NO, list);
            //}
            return result;
        }

        public SalesAgeReportResponseModel SalesAgingReport(ReportRequestModel model, NeoErpCoreEntity dbContext)
        {
            var result = new SalesAgeReportResponseModel();
            result.sales = this.MonthWiseSales(model, dbContext);
            result.age = this.AgingReport(model, dbContext);
            return result;
        }

        public Dictionary<string, string> MonthWiseSales(ReportRequestModel model, NeoErpCoreEntity dbContext)
        {
            if (string.IsNullOrWhiteSpace(model.code) || string.IsNullOrEmpty(model.code))
                throw new Exception("Code cannot be empty");

            var result = new Dictionary<string, string>();
            var SalesQuery = string.Empty;
            SalesQuery = $@"SELECT 
                  UPPER(trim(substr(FN_CHARTBS_MONTH(SUBSTR(BS_DATE(SI.sales_date),6,2)),5,20))) MONTH, 
                  TO_CHAR(NVL(SUM(NVL(SI.CALC_TOTAL_PRICE,0))/1,0)) AS AMOUNT,
                  TO_CHAR(SUM(NVL(SI.CALC_QUANTITY,0))/'1') AS QUANTITY,
                  TO_CHAR(SUBSTR(BS_DATE(SI.SALES_DATE),1,7)) AS MONTHINT
                FROM SA_SALES_INVOICE SI
                WHERE 1 = 1
                  AND  SI.DELETED_FLAG = 'N'
                  AND SI.COMPANY_CODE IN ('{model.COMPANY_CODE}')
                  AND SI.CUSTOMER_CODE = '{model.code}'
              GROUP BY FN_CHARTBS_MONTH(SUBSTR(BS_DATE(SI.SALES_DATE),6,2)),SUBSTR(BS_DATE(SI.SALES_DATE),1,7)
              ORDER BY SUBSTR(BS_DATE(SI.SALES_DATE),1,7)";
            var data = dbContext.SqlQuery<SalesReportResponseModel>(SalesQuery).ToList();
            string[] Months = { "BAISAKH", "JESTHA", "ASHADH", "SHRAWAN", "BHADRA", "ASHOJ", "KARTIK", "MANGSIR", "POUSH", "MAGH", "FALGUN", "CHAITRA" };

            decimal Netsales = 0;
            foreach (var m in Months)
            {
                decimal amount = 0;
                var monthData = data.FirstOrDefault(x => x.MONTH == m);
                if (monthData == null)
                    result.Add(m, "0");
                else
                {
                    decimal.TryParse(monthData.AMOUNT, out amount);
                    Netsales += amount;
                    result.Add(m, monthData.AMOUNT);
                }
            }
            result.Add("NETSALES", Netsales.ToString());
            return result;
        }

        public Dictionary<string, string> AgingReport(ReportRequestModel model, NeoErpCoreEntity dbContext)
        {
            model.code = model.code.FirstOrDefault() == 'C' ? model.code : "C" + model.code;
            var result = new Dictionary<string, string>();
            string AgingQuery = string.Empty;
            var Dates = new List<AgingDateRange>();
            Dates.Add(new AgingDateRange { StartDate = DateTime.Now.AddDays(-30), EndDate = DateTime.Now });
            for (int i = 1; i < 5; i++)
            {
                var date = new AgingDateRange();
                date.StartDate = Dates[i - 1].StartDate.AddDays(-30);
                date.EndDate = Dates[i - 1].StartDate.AddDays(-1);
                Dates.Add(date);
            }
            AgingQuery += "SELECT DISTINCT A.SUB_CODE, ";
            for (int i = 5; i >= 0; i--)
            {
                if (i == 5)
                    AgingQuery += $@"
                    SUM((SELECT SUM(ROUND(NVL(C.CR_AMOUNT,0),2)) FROM V$CUSTOMER_BILLAGE_LEDGER C
                    WHERE C.COMPANY_CODE = A.COMPANY_CODE AND C.BRANCH_CODE = A.BRANCH_CODE AND C.SERIAL_NO = A.SERIAL_NO AND C.SUB_CODE = A.SUB_CODE
                    AND C.ACC_CODE = A.ACC_CODE AND C.FORM_CODE = A.FORM_CODE AND C.VOUCHER_NO = A.VOUCHER_NO
                    AND TO_DATE(TO_CHAR(C.VOUCHER_DATE,'DD-MON-YYYY')) <= TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}')
                    AND C.COMPANY_CODE IN ('{model.COMPANY_CODE}') )) CR_AMOUNT, ";
                else
                {
                    if (i == 4)
                        AgingQuery += $@"
                    SUM((SELECT SUM(ROUND(NVL(C.DR_AMOUNT,0),2)) FROM V$CUSTOMER_BILLAGE_LEDGER C WHERE C.COMPANY_CODE = A.COMPANY_CODE
                    AND C.BRANCH_CODE = A.BRANCH_CODE AND C.SERIAL_NO = A.SERIAL_NO AND C.SUB_CODE = A.SUB_CODE AND C.ACC_CODE = A.ACC_CODE
                    AND C.FORM_CODE = A.FORM_CODE AND C.VOUCHER_NO = A.VOUCHER_NO 
                    AND TO_DATE(TO_CHAR(C.VOUCHER_DATE,'DD-MON-YYYY')) <= TO_DATE('{Dates[i].EndDate.ToString("dd-MMM-yyyy")}')
                    AND C.COMPANY_CODE IN ('{model.COMPANY_CODE}')  )) DR_AMOUNT{i}, ";
                    else
                    {
                        AgingQuery += $@"
                    SUM((SELECT SUM(ROUND(NVL(C.DR_AMOUNT,0),2)) FROM V$CUSTOMER_BILLAGE_LEDGER C WHERE C.COMPANY_CODE = A.COMPANY_CODE
                    AND C.BRANCH_CODE = A.BRANCH_CODE AND C.SERIAL_NO = A.SERIAL_NO AND C.SUB_CODE = A.SUB_CODE AND C.ACC_CODE = A.ACC_CODE
                    AND C.FORM_CODE = A.FORM_CODE AND C.VOUCHER_NO = A.VOUCHER_NO AND TO_DATE(TO_CHAR(C.VOUCHER_DATE,'DD-MON-YYYY')) >= TO_DATE('{Dates[i].StartDate.ToString("dd-MMM-yyyy")}')
                    AND TO_DATE(TO_CHAR(C.VOUCHER_DATE,'DD-MON-YYYY')) <= TO_DATE('{Dates[i].EndDate.ToString("dd-MMM-yyyy")}')
                    AND C.COMPANY_CODE IN ('{model.COMPANY_CODE}')   )) DR_AMOUNT";
                        AgingQuery += i;
                        if (i != 0)
                            AgingQuery += ", ";
                    }
                }
            }
            AgingQuery += $@"
            FROM V$CUSTOMER_BILLAGE_LEDGER A
            WHERE A.DELETED_FLAG='N' AND A.SUB_CODE ='{model.code}'
            AND TO_DATE(TO_CHAR(A.VOUCHER_DATE,'DD-MON-YYYY')) <= TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}')
            GROUP BY A.SUB_CODE";
            var data = dbContext.SqlQuery<AgingReportModel>(AgingQuery).FirstOrDefault();

            result.Add("SUB_CODE", model.code);
            result.Add("120+", "0");
            result.Add("91-120", "0");
            result.Add("61-90", "0");
            result.Add("31-60", "0");
            result.Add("0-30", "0");
            result.Add("total", "0");

            if (data != null)
            {
                result["SUB_CODE"] = data.SUB_CODE;
                decimal total = 0;
                var CRAmount = data.CR_AMOUNT == null ? 0 : data.CR_AMOUNT.Value;
                //DR_AMOUNT4
                if (data.DR_AMOUNT4 != null)
                {
                    if (CRAmount >= data.DR_AMOUNT4)
                        CRAmount = CRAmount - data.DR_AMOUNT4.Value;
                    else
                    {
                        result["120+"] = (data.DR_AMOUNT4 - CRAmount).ToString();
                        total += data.DR_AMOUNT4.Value - CRAmount;
                        CRAmount = 0;
                    }
                }
                //DR_AMOUNT3
                if (data.DR_AMOUNT3 != null)
                {
                    if (CRAmount >= data.DR_AMOUNT3)
                        CRAmount = CRAmount - data.DR_AMOUNT3.Value;
                    else
                    {
                        result["91-120"] = (data.DR_AMOUNT3 - CRAmount).ToString();
                        total += data.DR_AMOUNT3.Value - CRAmount;
                        CRAmount = 0;
                    }
                }
                //DR_AMOUNT2
                if (data.DR_AMOUNT2 != null)
                {
                    if (CRAmount >= data.DR_AMOUNT2)
                        CRAmount = CRAmount - data.DR_AMOUNT2.Value;
                    else
                    {
                        result["61-90"] = (data.DR_AMOUNT2 - CRAmount).ToString();
                        total += data.DR_AMOUNT2.Value - CRAmount;
                        CRAmount = 0;
                    }
                }
                //DR_AMOUNT1
                if (data.DR_AMOUNT1 != null)
                {
                    if (CRAmount >= data.DR_AMOUNT1)
                        CRAmount = CRAmount - data.DR_AMOUNT1.Value;
                    else
                    {
                        result["31-60"] = (data.DR_AMOUNT1 - CRAmount).ToString();
                        total += data.DR_AMOUNT1.Value - CRAmount;
                        CRAmount = 0;
                    }
                }
                //DR_AMOUNT0
                if (data.DR_AMOUNT0 != null)
                {
                    if (CRAmount >= data.DR_AMOUNT0)
                        CRAmount = CRAmount - data.DR_AMOUNT0.Value;
                    else
                    {
                        result["0-30"] = (data.DR_AMOUNT0 - CRAmount).ToString();
                        total += data.DR_AMOUNT0.Value - CRAmount;
                        CRAmount = 0;
                    }
                }
                result["total"] = total.ToString();
            }
            return result;
        }

        public List<Dictionary<string, string>> AgingReportGroup(ReportRequestModel model, NeoErpCoreEntity dbContext)
        {
            //model.code = model.code.FirstOrDefault() == 'C' ? model.code : "C" + model.code;
            var listResult = new List<Dictionary<string, string>>();
            string AgingQuery = string.Empty;
            var Dates = new List<AgingDateRange>();
            Dates.Add(new AgingDateRange { StartDate = DateTime.Now.AddDays(-30), EndDate = DateTime.Now });
            for (int i = 1; i < 4; i++)
            {
                var date = new AgingDateRange();
                date.StartDate = Dates[i - 1].StartDate.AddDays(-30);
                date.EndDate = Dates[i - 1].StartDate.AddDays(-1);
                Dates.Add(date);
            }
            AgingQuery += "SELECT DISTINCT A.SUB_CODE,A.SUB_EDESC,";
            for (int i = 4; i >= 0; i--)
            {
                if (i == 4)
                    AgingQuery += $@"
                    SUM((SELECT SUM(ROUND(NVL(C.CR_AMOUNT,0),2)) FROM V$CUSTOMER_BILLAGE_LEDGER C
                    WHERE C.COMPANY_CODE = A.COMPANY_CODE AND C.BRANCH_CODE = A.BRANCH_CODE AND C.SERIAL_NO = A.SERIAL_NO AND C.SUB_CODE = A.SUB_CODE
                    AND C.ACC_CODE = A.ACC_CODE AND C.FORM_CODE = A.FORM_CODE AND C.VOUCHER_NO = A.VOUCHER_NO
                    AND TO_DATE(TO_CHAR(C.VOUCHER_DATE,'DD-MON-YYYY')) <= TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}')
                    AND C.COMPANY_CODE='{model.COMPANY_CODE}'  AND C.BRANCH_CODE='{model.BRANCH_CODE}' )) CR_AMOUNT, ";
                else
                {
                    if (i == 3)
                        AgingQuery += $@"
                    SUM((SELECT SUM(ROUND(NVL(C.DR_AMOUNT,0),2)) FROM V$CUSTOMER_BILLAGE_LEDGER C WHERE C.COMPANY_CODE = A.COMPANY_CODE
                    AND C.BRANCH_CODE = A.BRANCH_CODE AND C.SERIAL_NO = A.SERIAL_NO AND C.SUB_CODE = A.SUB_CODE AND C.ACC_CODE = A.ACC_CODE
                    AND C.FORM_CODE = A.FORM_CODE AND C.VOUCHER_NO = A.VOUCHER_NO 
                    AND TO_DATE(TO_CHAR(C.VOUCHER_DATE,'DD-MON-YYYY')) <= TO_DATE('{Dates[i].EndDate.ToString("dd-MMM-yyyy")}')
                    AND C.COMPANY_CODE='{model.COMPANY_CODE}' AND C.BRANCH_CODE='{model.BRANCH_CODE}' )) DR_AMOUNT{i}, ";
                    else
                    {
                        AgingQuery += $@"
                    SUM((SELECT SUM(ROUND(NVL(C.DR_AMOUNT,0),2)) FROM V$CUSTOMER_BILLAGE_LEDGER C WHERE C.COMPANY_CODE = A.COMPANY_CODE
                    AND C.BRANCH_CODE = A.BRANCH_CODE AND C.SERIAL_NO = A.SERIAL_NO AND C.SUB_CODE = A.SUB_CODE AND C.ACC_CODE = A.ACC_CODE
                    AND C.FORM_CODE = A.FORM_CODE AND C.VOUCHER_NO = A.VOUCHER_NO AND TO_DATE(TO_CHAR(C.VOUCHER_DATE,'DD-MON-YYYY')) >= TO_DATE('{Dates[i].StartDate.ToString("dd-MMM-yyyy")}')
                    AND TO_DATE(TO_CHAR(C.VOUCHER_DATE,'DD-MON-YYYY')) <= TO_DATE('{Dates[i].EndDate.ToString("dd-MMM-yyyy")}')
                    AND C.COMPANY_CODE='{model.COMPANY_CODE}'  AND C.BRANCH_CODE='{model.BRANCH_CODE}' )) DR_AMOUNT";
                        AgingQuery += i;
                        if (i != 0)
                            AgingQuery += ", ";
                    }
                }
            }
            AgingQuery += $@"
            FROM V$CUSTOMER_BILLAGE_LEDGER A
            WHERE A.DELETED_FLAG='N' AND A.SUB_CODE IN (SELECT SUB_CODE FROM  FA_SUB_LEDGER_DEALER_MAP WHERE PARTY_TYPE_CODE='{model.party_type_code}')
            AND TO_DATE(TO_CHAR(A.VOUCHER_DATE,'DD-MON-YYYY')) <= TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}')
            GROUP BY A.SUB_CODE,A.SUB_EDESC";
            var AllData = dbContext.SqlQuery<AgingReportModel>(AgingQuery).ToList();


            if (AllData.Count > 0)
            {
                foreach (var data in AllData)
                {
                    var result = new Dictionary<string, string>();
                    result.Add("SUB_CODE", data.SUB_CODE);
                    result.Add("SUB_EDESC", data.SUB_EDESC);
                    //result.Add("120+", "0");
                    result.Add("90+", "0");
                    result.Add("61-90", "0");
                    result.Add("31-60", "0");
                    result.Add("0-30", "0");
                    result.Add("total", "0");
                    decimal total = 0;
                    var CRAmount = data.CR_AMOUNT == null ? 0 : data.CR_AMOUNT.Value;
                    //DR_AMOUNT4
                    //if (data.DR_AMOUNT4 != null)
                    //{
                    //    if (CRAmount >= data.DR_AMOUNT4)
                    //        CRAmount = CRAmount - data.DR_AMOUNT4.Value;
                    //    else
                    //    {
                    //        result["120+"] = (data.DR_AMOUNT4 - CRAmount).ToString();
                    //        total += data.DR_AMOUNT4.Value - CRAmount;
                    //        CRAmount = 0;
                    //    }
                    //}

                    //DR_AMOUNT3
                    if (data.DR_AMOUNT3 != null)
                    {
                        if (CRAmount >= data.DR_AMOUNT3)
                            CRAmount = CRAmount - data.DR_AMOUNT3.Value;
                        else
                        {
                            result["90+"] = (data.DR_AMOUNT3 - CRAmount).ToString();
                            total += data.DR_AMOUNT3.Value - CRAmount;
                            CRAmount = 0;
                        }
                    }
                    //DR_AMOUNT2
                    if (data.DR_AMOUNT2 != null)
                    {
                        if (CRAmount >= data.DR_AMOUNT2)
                            CRAmount = CRAmount - data.DR_AMOUNT2.Value;
                        else
                        {
                            result["61-90"] = (data.DR_AMOUNT2 - CRAmount).ToString();
                            total += data.DR_AMOUNT2.Value - CRAmount;
                            CRAmount = 0;
                        }
                    }
                    //DR_AMOUNT1
                    if (data.DR_AMOUNT1 != null)
                    {
                        if (CRAmount >= data.DR_AMOUNT1)
                            CRAmount = CRAmount - data.DR_AMOUNT1.Value;
                        else
                        {
                            result["31-60"] = (data.DR_AMOUNT1 - CRAmount).ToString();
                            total += data.DR_AMOUNT1.Value - CRAmount;
                            CRAmount = 0;
                        }
                    }
                    //DR_AMOUNT0
                    if (data.DR_AMOUNT0 != null)
                    {
                        if (CRAmount >= data.DR_AMOUNT0)
                            CRAmount = CRAmount - data.DR_AMOUNT0.Value;
                        else
                        {
                            result["0-30"] = (data.DR_AMOUNT0 - CRAmount).ToString();
                            total += data.DR_AMOUNT0.Value - CRAmount;
                            CRAmount = 0;
                        }
                    }
                    result["total"] = total.ToString();
                    listResult.Add(result);
                }
            }
            return listResult;
        }

        public DistributorItemResponseModel FetchEntityPartyTypeAndMu(EntityRequestModel model, NeoErpCoreEntity dbContext)
        {
            var result = new DistributorItemResponseModel();
            model.entity_code = model.entity_code.FirstOrDefault() == 'C' ? model.entity_code : "C" + model.entity_code;
            var PartyQuery = string.Empty;
            switch (model.entity_type.ToUpper())
            {
                case "D":
                case "DISTRIBUTOR":
                    PartyQuery = $@"SELECT 
                            PT.PARTY_TYPE_CODE AS CODE, 
                            PT.PARTY_TYPE_EDESC AS NAME, 
                            PT.ACC_CODE,
                            DM.CONTACT_NO AS P_CONTACT_NO, '' AS P_CONTACT_NAME, DM.REG_OFFICE_ADDRESS AS ADDRESS,
                            RM.ROUTE_CODE, RM.ROUTE_NAME,
                            AM.AREA_CODE, AM.AREA_NAME,
                            NVL(DM.LATITUDE, 0) LATITUDE, NVL(DM.LONGITUDE, 0) LONGITUDE,
                            'dealer' AS TYPE,
                            '' DEFAULT_PARTY_TYPE_CODE,
                            '' PARENT_DISTRIBUTOR_CODE,
                            '' PARENT_DISTRIBUTOR_NAME,
                            LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                            DM.COMPANY_CODE, DM.BRANCH_CODE
                            FROM IP_PARTY_TYPE_CODE PT
                            LEFT JOIN DIST_DEALER_MASTER DM ON DM.DEALER_CODE = PT.PARTY_TYPE_CODE AND DM.COMPANY_CODE = PT.COMPANY_CODE
                            LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = DM.DEALER_CODE AND RE.ENTITY_TYPE = 'P' AND RE.COMPANY_CODE = DM.COMPANY_CODE
                            LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = DM.COMPANY_CODE
                            LEFT JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                            LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                              (CASE 
                                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                                  ELSE
                                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                               END
                                              ) IS_VISITED, 
                                              A.REMARKS,
                                              TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                            FROM DIST_LOCATION_TRACK A
                                              INNER JOIN IP_PARTY_TYPE_CODE B ON B.PARTY_TYPE_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                              INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                            WHERE 1 = 1
                                                  AND A.CUSTOMER_TYPE = 'P'
                                                  AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                            GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                              (CASE 
                                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                                  ELSE
                                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                               END
                                              ), 
                                              A.REMARKS
                                           ) LT
                                   ON LT.CUSTOMER_CODE = DM.DEALER_CODE AND LT.COMPANY_CODE = DM.COMPANY_CODE
                            WHERE TRIM(PT.ACC_CODE) IN (SELECT TRIM(ACC_CODE) FROM FA_SUB_LEDGER_MAP WHERE COMPANY_CODE = '{model.COMPANY_CODE}' AND SUB_CODE = TRIM('{model.entity_code}'))
                              AND PT.COMPANY_CODE = '{model.COMPANY_CODE}'
                              AND PT.DELETED_FLAG = 'N'
                            ORDER BY UPPER(PT.PARTY_TYPE_EDESC)";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(PartyQuery))
            {
                var allParty = dbContext.SqlQuery<EntityResponseModel>(PartyQuery).ToList();
                if (allParty.Count > 0)
                {
                    foreach (var party in allParty)
                    {
                        result.PARTY.Add(party.CODE, party);
                    }
                }
            }

            //preferences
            var pref = this.FetchPreferences(model.COMPANY_CODE, dbContext);
            string salesRateClause = string.Empty;
            string conversionClause = string.Empty;
            if (pref.PO_SYN_RATE.Trim().ToUpper() == "Y")
                salesRateClause = "AND SALES_RATE IS NOT NULL AND SALES_RATE <> 0";
            if (pref.SQL_NN_CONVERSION_UNIT_FACTOR.Trim().ToUpper() == "Y")
                conversionClause = "AND IUS.MU_CODE IS NOT NULL AND IUS.CONVERSION_FACTOR IS NOT NULL";

            //Unit
            Dictionary<string, MuCodeResponseModel> MuResult = new Dictionary<string, MuCodeResponseModel>();
            string MuQuery = $@"SELECT IMS.ITEM_CODE, IMS.ITEM_EDESC, IMS.INDEX_MU_CODE, IUS.MU_CODE, IUS.CONVERSION_FACTOR, IMS.COMPANY_CODE
                FROM IP_ITEM_MASTER_SETUP IMS 
                LEFT JOIN IP_ITEM_UNIT_SETUP IUS
                ON IUS.ITEM_CODE = IMS.ITEM_CODE AND IUS.COMPANY_CODE = IMS.COMPANY_CODE
                WHERE 1 = 1
                AND IMS.COMPANY_CODE IN (SELECT COMPANY_CODE FROM COMPANY_SETUP)
                AND IMS.CATEGORY_CODE = '{CATEGORY_CODE}'
                AND IMS.GROUP_SKU_FLAG = '{GROUP_SKU_FLAG}'
                AND IMS.DELETED_FLAG = 'N'
                {conversionClause}
                ORDER BY IMS.COMPANY_CODE, UPPER(IMS.ITEM_EDESC), UPPER(IMS.INDEX_MU_CODE), UPPER(MU_CODE)";
            var allMu = dbContext.SqlQuery<MuCodeResponseModel>(MuQuery).GroupBy(x => x.COMPANY_CODE);
            foreach (var Mu in allMu)
            {
                var tempResult = new Dictionary<string, MuCodeResponseModel>();
                var CompanyMu = Mu.ToList();
                foreach (var CM in CompanyMu)
                {
                    if (CM.MU_CODE != null)
                        CM.CONVERSION_UNIT_FACTOR.Add(CM.MU_CODE, CM.CONVERSION_FACTOR);
                    tempResult.Add(CM.ITEM_CODE, CM);
                }
                result.UNIT[Mu.Key] = tempResult;
            }

            //sales types
            var salesTypes = FetchAllCompanySaSalesType(dbContext).GroupBy(x => x.COMPANY_CODE);
            foreach (var type in salesTypes)
            {
                result.SALES_TYPE.Add(type.Key, type.ToList());
            }
            result.SHIPPING_ADDRESS = FetchShippingAddress(dbContext); //shipping addresses
            return result;
        }

        public Dictionary<string, List<EntityResponseModel>> FetchPartyTypeBillingEntity(EntityRequestModel model, NeoErpCoreEntity dbContext)
        {
            if (string.IsNullOrWhiteSpace(model.COMPANY_CODE))
                throw new Exception("Company code not found!!!");
            if (string.IsNullOrWhiteSpace(model.ACC_CODE))
                throw new Exception("Account code not found!!!");

            string Query = $@"SELECT CS.CUSTOMER_CODE AS CODE,
                  CS.CUSTOMER_EDESC AS NAME,
                  CS.ACC_CODE AS ACC_CODE,
                  CS.PARTY_TYPE_CODE AS DEFAULT_PARTY_TYPE_CODE,
                  DM.CONTACT_NO AS P_CONTACT_NO, '' AS P_CONTACT_NAME, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS) AS ADDRESS,
                  AM.AREA_CODE, AM.AREA_NAME,
                  NVL(DM.LATITUDE, '0') AS LATITUDE,
                  NVL(DM.LONGITUDE, '0') AS LONGITUDE,
                  'distributor' AS TYPE,
                   '' AS PARENT_DISTRIBUTOR_CODE,
                   '' AS PARENT_DISTRIBUTOR_NAME,
                   LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                   CS.COMPANY_CODE, CS.BRANCH_CODE
                FROM SA_CUSTOMER_SETUP CS
                LEFT JOIN DIST_DISTRIBUTOR_MASTER DM ON DM.DISTRIBUTOR_CODE = CS.CUSTOMER_CODE AND DM.COMPANY_CODE = CS.COMPANY_CODE
                LEFT JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = CS.COMPANY_CODE
                LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                              (CASE 
                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                  ELSE
                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                               END
                              ) IS_VISITED, 
                              A.REMARKS,
                              TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                          FROM DIST_LOCATION_TRACK A
                          INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                          INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                          WHERE 1 = 1
                          AND A.CUSTOMER_TYPE = 'D'
                          AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                              (CASE 
                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                  ELSE
                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                               END
                              ), 
                              A.REMARKS
                          ) LT
                       ON LT.CUSTOMER_CODE = CS.CUSTOMER_CODE AND LT.COMPANY_CODE = CS.COMPANY_CODE                       
                WHERE CS.GROUP_SKU_FLAG = 'I'
                AND TRIM(CS.LINK_SUB_CODE) IN (SELECT TRIM(SUB_CODE) FROM FA_SUB_LEDGER_MAP WHERE COMPANY_CODE = '{model.COMPANY_CODE}' AND ACC_CODE ='{model.ACC_CODE}')
                AND CS.COMPANY_CODE = '{model.COMPANY_CODE}'
                ORDER BY UPPER(CS.CUSTOMER_EDESC)";
            var AllParty = dbContext.SqlQuery<EntityResponseModel>(Query).ToList();
            var result = new Dictionary<string, List<EntityResponseModel>>();
            if (AllParty != null)
                result.Add("distributor", AllParty);
            return result;
        }

        public List<EntityResponseModel> FetchEntityById(EntityRequestModel model, NeoErpCoreEntity dbContext)
        {
            string Query = string.Empty;
            if (model.entity_type.Equals("P", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("DEALER", StringComparison.OrdinalIgnoreCase))
            {
                Query = $@"SELECT
                     DM.DEALER_CODE AS CODE,
                     PT.PARTY_TYPE_EDESC AS NAME,
                     PT.ACC_CODE,
                     DM.CONTACT_NO AS P_CONTACT_NO, '' AS P_CONTACT_NAME, DM.REG_OFFICE_ADDRESS AS ADDRESS,
                     RM.ROUTE_CODE, RM.ROUTE_NAME,
                     AM.AREA_CODE, AM.AREA_NAME,
                     NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                     'dealer' AS TYPE,
                     '' DEFAULT_PARTY_TYPE_CODE,
                     '' PARENT_DISTRIBUTOR_CODE,
                     '' PARENT_DISTRIBUTOR_NAME,
                     LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                     DM.COMPANY_CODE, DM.BRANCH_CODE
                   FROM
                     DIST_DEALER_MASTER DM
                     LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = DM.DEALER_CODE AND RE.ENTITY_TYPE = 'P' AND RE.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN IP_PARTY_TYPE_CODE PT ON PT.PARTY_TYPE_CODE = DM.DEALER_CODE AND PT.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ) IS_VISITED, 
                                  A.REMARKS,
                                  TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                FROM DIST_LOCATION_TRACK A
                                  INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                WHERE 1 = 1
                                      AND A.CUSTOMER_TYPE = 'P'
                                      AND A.CUSTOMER_CODE = '{model.entity_code}'
                                      AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ), 
                                  A.REMARKS
                               ) LT
                       ON LT.CUSTOMER_CODE = DM.DEALER_CODE AND LT.COMPANY_CODE = DM.COMPANY_CODE
                   WHERE 1 = 1
                         AND DM.COMPANY_CODE = '{model.COMPANY_CODE}'
                         AND DM.DEALER_CODE = '{model.entity_code}'";
            }
            else if (model.entity_type.Equals("D", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase))
            {
                Query = $@"SELECT
                     DM.DISTRIBUTOR_CODE AS CODE,
                     CS.CUSTOMER_EDESC AS NAME,
                     CS.ACC_CODE,
                     DM.CONTACT_NO AS P_CONTACT_NO, '' AS P_CONTACT_NAME, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS) AS ADDRESS,
                     RM.ROUTE_CODE, RM.ROUTE_NAME,
                     AM.AREA_CODE, AM.AREA_NAME,
                     NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                     'distributor' AS TYPE,
                     CS.PARTY_TYPE_CODE AS DEFAULT_PARTY_TYPE_CODE,
                     '' PARENT_DISTRIBUTOR_CODE,
                     '' PARENT_DISTRIBUTOR_NAME,
                     LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                     DM.COMPANY_CODE, DM.BRANCH_CODE
                   FROM
                     DIST_DISTRIBUTOR_MASTER DM
                     LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = DM.DISTRIBUTOR_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ) IS_VISITED, 
                                  A.REMARKS,
                                  TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                FROM DIST_LOCATION_TRACK A
                                  INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                WHERE 1 = 1
                                      AND A.CUSTOMER_TYPE = 'D'
                                      AND A.CUSTOMER_CODE = '{model.entity_code}'
                                      AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ), 
                                  A.REMARKS
                               ) LT
                       ON LT.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND LT.COMPANY_CODE = DM.COMPANY_CODE
                   WHERE 1 = 1
                         AND DM.COMPANY_CODE = '{model.COMPANY_CODE}'
                         AND DM.DISTRIBUTOR_CODE = '{model.entity_code}'";
            }
            else if (model.entity_type.Equals("R", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("RESELLER", StringComparison.OrdinalIgnoreCase))
            {
                Query = $@"SELECT
                     REM.RESELLER_CODE AS CODE,
                     REM.RESELLER_NAME AS NAME,
                     '' AS ACC_CODE,
                     REM.CONTACT_NO AS P_CONTACT_NO, REM.CONTACT_NAME AS P_CONTACT_NAME, REM.REG_OFFICE_ADDRESS AS ADDRESS,
                     RM.ROUTE_CODE, RM.ROUTE_NAME,
                     AM.AREA_CODE, AM.AREA_NAME,
                     NVL(REM.LATITUDE,0) LATITUDE, NVL(REM.LONGITUDE,0) LONGITUDE,
                     'reseller' AS TYPE,
                     '' AS DEFAULT_PARTY_TYPE_CODE,
                     DISTRIBUTOR_CODE AS PARENT_DISTRIBUTOR_CODE,
                     CS.CUSTOMER_EDESC AS PARENT_DISTRIBUTOR_NAME,
                     LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                     REM.COMPANY_CODE, REM.BRANCH_CODE
                   FROM DIST_RESELLER_MASTER REM
                     LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = REM.RESELLER_CODE AND RE.ENTITY_TYPE = 'R' AND RE.COMPANY_CODE = REM.COMPANY_CODE
                     LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = REM.COMPANY_CODE
                     INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = REM.AREA_CODE AND AM.COMPANY_CODE = REM.COMPANY_CODE
                     LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = REM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = REM.COMPANY_CODE
                     LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ) IS_VISITED, 
                                  A.REMARKS,
                                  TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                FROM DIST_LOCATION_TRACK A
                                  INNER JOIN DIST_RESELLER_MASTER B ON B.RESELLER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                WHERE 1 = 1
                                      AND A.CUSTOMER_TYPE = 'R'
                                      AND A.CUSTOMER_CODE = '{model.entity_code}'
                                      AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ), 
                                  A.REMARKS
                               ) LT
                       ON LT.CUSTOMER_CODE = REM.RESELLER_CODE AND LT.COMPANY_CODE = REM.COMPANY_CODE
                   WHERE 1 = 1
                         AND REM.COMPANY_CODE = '{model.COMPANY_CODE}'
                         AND REM.RESELLER_CODE = '{model.entity_code}'";
            }
            else
                throw new Exception("Invalid entity type");
            if (string.IsNullOrWhiteSpace(Query))
                return new List<EntityResponseModel>();
            var result = dbContext.SqlQuery<EntityResponseModel>(Query).ToList();
            return result;
        }

        public Dictionary<string, List<EntityResponseModel>> FetchDistributorWithConstraint(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            string Query = string.Empty;
            Query = $@"SELECT
                     DM.DISTRIBUTOR_CODE AS CODE,
                     CS.CUSTOMER_EDESC AS NAME,
                     CS.ACC_CODE,
                     DM.CONTACT_NO AS P_CONTACT_NO, '' AS P_CONTACT_NAME, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS) AS ADDRESS,
                     RM.ROUTE_CODE, RM.ROUTE_NAME,
                     AM.AREA_CODE, AM.AREA_NAME,
                     NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                     'distributor' AS TYPE,
                     CS.PARTY_TYPE_CODE AS DEFAULT_PARTY_TYPE_CODE,
                     '' PARENT_DISTRIBUTOR_CODE,
                     '' PARENT_DISTRIBUTOR_NAME,
                     LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,
                     DM.COMPANY_CODE, DM.BRANCH_CODE
                   FROM
                     DIST_DISTRIBUTOR_MASTER DM
                     LEFT JOIN DIST_ROUTE_ENTITY RE ON RE.ENTITY_CODE = DM.DISTRIBUTOR_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN DIST_ROUTE_MASTER RM ON RM.ROUTE_CODE = RE.ROUTE_CODE AND RM.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
                     INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                     LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ) IS_VISITED, 
                                  A.REMARKS,
                                  TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                                FROM DIST_LOCATION_TRACK A
                                  INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                                  INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                WHERE 1 = 1
                                      AND A.CUSTOMER_TYPE = 'D'
                                      AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                                GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                                  (CASE 
                                    WHEN A.IS_VISITED IS NULL THEN 'X' 
                                      ELSE
                                        CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                                   END
                                  ), 
                                  A.REMARKS
                               ) LT
                       ON LT.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND LT.COMPANY_CODE = DM.COMPANY_CODE
                   WHERE 1 = 1
                         AND DM.COMPANY_CODE = '{model.COMPANY_CODE}'";
            var distributors = dbContext.SqlQuery<EntityResponseModel>(Query).ToList();
            var result = new Dictionary<string, List<EntityResponseModel>>();
            result.Add("distributor", distributors);
            return result;
        }

        public List<SPEntityModel> FetchSpPartyType(VisitPlanRequestModel model, NeoErpCoreEntity dbContext)
        {
            var Query = $@"SELECT
                DM.DEALER_CODE AS CODE,
                TRIM(PT.PARTY_TYPE_EDESC) AS NAME,
                TRIM(PT.ACC_CODE) ACC_CODE,
                DM.CONTACT_NO AS P_CONTACT_NO, DM.REG_OFFICE_ADDRESS AS ADDRESS,
                RM.ROUTE_CODE, RM.ROUTE_NAME,
                AM.AREA_CODE, AM.AREA_NAME,
                RE.ORDER_NO,
                RM.COMPANY_CODE,
                'dealer' AS TYPE,
                '' DEFAULT_PARTY_TYPE_CODE,
                '' PARENT_DISTRIBUTOR_CODE,
                '' PARENT_DISTRIBUTOR_NAME,
                NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, NVL(LT.LAST_VISIT_STATUS, 'X') LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN (SELECT ROUTE_CODE, EMP_CODE, COMPANY_CODE FROM DIST_ROUTE_DETAIL WHERE DELETED_FLAG = 'N' GROUP BY ROUTE_CODE, EMP_CODE, COMPANY_CODE) RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'P' AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_DEALER_MASTER DM ON DM.DEALER_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN IP_PARTY_TYPE_CODE PT ON PT.PARTY_TYPE_CODE = DM.DEALER_CODE AND PT.COMPANY_CODE = DM.COMPANY_CODE
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS, 
                          (CASE 
                            WHEN A.IS_VISITED IS NULL THEN 'X' 
                              ELSE
                                CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                           END
                          ) IS_VISITED, 
                          A.REMARKS,
                          TO_CHAR(MAX(A.UPDATE_DATE), 'DD-MON-RRRR HH24:MI A.M.') LAST_VISIT_DATE
                          FROM DIST_LOCATION_TRACK A
                          INNER JOIN IP_PARTY_TYPE_CODE B ON B.PARTY_TYPE_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                          INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                          WHERE 1 = 1
                          -- AND SP_CODE = '1000097' -- Commented out because customer can be visited by another SP_CODE
                          AND A.CUSTOMER_TYPE = 'P'
                          AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                              (CASE 
                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                  ELSE
                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                               END
                              ), 
                              A.REMARKS
                          ) LT
                          ON LT.CUSTOMER_CODE = DM.DEALER_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
                WHERE 1 = 1
                --AND TO_CHAR(RD.ASSIGN_DATE, 'DD-MON-RRRR') = UPPER('23-MAY-2017')
                AND RD.EMP_CODE = '{model.spcode}'
                AND RD.COMPANY_CODE = '{model.COMPANY_CODE}'
                GROUP BY 
                  DM.DEALER_CODE,
                  TRIM(PT.PARTY_TYPE_EDESC),
                  TRIM(PT.ACC_CODE),
                  DM.CONTACT_NO, DM.REG_OFFICE_ADDRESS,
                  RM.ROUTE_CODE, RM.ROUTE_NAME,
                  AM.AREA_CODE, AM.AREA_NAME,
                  RE.ORDER_NO,
                  RM.COMPANY_CODE,
                  'dealer',
                  '',
                  '',
                  '',
                  NVL(DM.LATITUDE,0), NVL(DM.LONGITUDE,0),
                  LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, NVL(LT.LAST_VISIT_STATUS, 'X'), NVL(LT.IS_VISITED, 'X'), LT.REMARKS
                ORDER BY UPPER(TRIM(PT.PARTY_TYPE_EDESC)) ASC";

            var data = dbContext.SqlQuery<SPEntityModel>(Query).ToList();
            return data;
        }

        public List<SPEntityModel> FetchSpCustomer(VisitPlanRequestModel model, NeoErpCoreEntity dbContext)
        {
            var Query = $@"SELECT
                DM.DISTRIBUTOR_CODE AS CODE,
                TRIM(CS.CUSTOMER_EDESC) AS NAME,
                TRIM(CS.ACC_CODE) AS ACC_CODE,
                DM.CONTACT_NO AS P_CONTACT_NO, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS) AS ADDRESS,
                RM.ROUTE_CODE, RM.ROUTE_NAME,
                AM.AREA_CODE, AM.AREA_NAME,
                RE.ORDER_NO,
                RM.COMPANY_CODE,
                'distributor' AS TYPE,
                CS.PARTY_TYPE_CODE AS DEFAULT_PARTY_TYPE_CODE,
                '' PARENT_DISTRIBUTOR_CODE,
                '' PARENT_DISTRIBUTOR_NAME,
                NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, NVL(LT.LAST_VISIT_STATUS, 'X') LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_DISTRIBUTOR_MASTER DM ON DM.DISTRIBUTOR_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS, 
                          (CASE 
                            WHEN A.IS_VISITED IS NULL THEN 'X' 
                              ELSE
                                CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                           END
                          ) IS_VISITED, 
                          A.REMARKS,
                          TO_CHAR(MAX(A.UPDATE_DATE), 'DD-MON-RRRR HH24:MI A.M.') LAST_VISIT_DATE
                          FROM DIST_LOCATION_TRACK A
                          INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                          INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                          WHERE 1 = 1
                          -- AND SP_CODE = '1000097' -- Commented out because customer can be visited by another SP_CODE
                          AND A.CUSTOMER_TYPE = 'D'
                          AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                              (CASE 
                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                  ELSE
                                    CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                               END
                              ), 
                              A.REMARKS
                          ) LT
                          ON LT.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
                WHERE 1 = 1
                --AND TO_CHAR(RD.ASSIGN_DATE, 'DD-MON-RRRR') = UPPER(''23-MAY-2017'')
                AND RD.EMP_CODE = '{model.spcode}'
                AND RD.COMPANY_CODE = '{model.COMPANY_CODE}'
                GROUP BY 
                  DM.DISTRIBUTOR_CODE,
                  TRIM(CS.CUSTOMER_EDESC),
                  TRIM(CS.ACC_CODE),
                  DM.CONTACT_NO, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS),
                  RM.ROUTE_CODE, RM.ROUTE_NAME,
                  AM.AREA_CODE, AM.AREA_NAME,
                  RE.ORDER_NO,
                  RM.COMPANY_CODE,
                  'dealer',
                  CS.PARTY_TYPE_CODE,
                  '',
                  '',
                  NVL(DM.LATITUDE,0), NVL(DM.LONGITUDE,0),
                  LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, NVL(LT.LAST_VISIT_STATUS, 'X'), NVL(LT.IS_VISITED, 'X'), LT.REMARKS
                ORDER BY UPPER(TRIM(CS.CUSTOMER_EDESC)) ASC";

            var data = dbContext.SqlQuery<SPEntityModel>(Query).ToList();
            return data;
        }

        public Dictionary<string, List<PurchaseOrderResponseModel>> FetchPOStatus(PurchaseOrderRequestModel model, NeoErpCoreEntity dbContext)
        {
            var date = dbContext.SqlQuery<DateTime>($"SELECT FY_START_DATE FROM PREFERENCE_SETUP WHERE COMPANY_CODE = '{model.COMPANY_CODE}'").FirstOrDefault();
            var fromDate = date.ToString("dd-MMM-yyyy");
            var endDate = DateTime.Now.ToString("dd-MMM-yyyy");

            string Query = string.Empty;
            if (model.type.ToUpper() == "P" || model.type.ToUpper() == "DEALER")
            {
                Query = $@"SELECT A.DEALER_CODE AS CODE, TO_CHAR(C.ORDER_NO) ORDER_NO, TO_CHAR(C.ORDER_DATE, 'DD-MON-YYYY') AS ORDER_DATE, 
                    C.ITEM_CODE, B.ITEM_EDESC, C.MU_CODE, TO_CHAR(C.QUANTITY) QUANTITY, TO_CHAR(C.UNIT_PRICE) UNIT_PRICE, TO_CHAR(C.TOTAL_PRICE) TOTAL_PRICE, C.REMARKS, C.APPROVED_FLAG, 
                    C.DISPATCH_FLAG, C.ACKNOWLEDGE_FLAG, C.REJECT_FLAG, TO_CHAR(NVL(IR.SALES_RATE, 0)) SALES_RATE, TO_CHAR(IR.APPLY_DATE) APPLY_DATE
                    FROM DIST_DEALER_MASTER A, IP_ITEM_MASTER_SETUP B, DIST_IP_SSD_PURCHASE_ORDER C,
                    (SELECT A.ITEM_CODE, A.APPLY_DATE, B.SALES_RATE, B.COMPANY_CODE
                              FROM (SELECT ITEM_CODE, COMPANY_CODE, MAX(APP_DATE) APPLY_DATE 
                                FROM IP_ITEM_RATE_APPLICAT_SETUP
                                WHERE COMPANY_CODE = '{model.COMPANY_CODE}' 
                                AND BRANCH_CODE = '{model.BRANCH_CODE}'
                                GROUP BY ITEM_CODE, COMPANY_CODE) A
                              INNER JOIN IP_ITEM_RATE_APPLICAT_SETUP B
                                ON B.ITEM_CODE = A.ITEM_CODE
                                AND B.APP_DATE = A.APPLY_DATE
                                AND B.COMPANY_CODE = '{model.COMPANY_CODE}'
                                AND B.BRANCH_CODE = '{model.BRANCH_CODE}') IR
                    WHERE A.DEALER_CODE = '{model.code}'
                    AND A.DEALER_CODE = C.PARTY_TYPE_CODE
                    AND B.ITEM_CODE = C.ITEM_CODE
                    AND B.GROUP_SKU_FLAG = '{GROUP_SKU_FLAG}'
                    AND B.CATEGORY_CODE = '$CATEGORY_CODE'
                    AND TRUNC(C.ORDER_DATE) BETWEEN TO_DATE('{fromDate}','DD-MON-RRRR') AND TO_DATE('{endDate}','DD-MON-RRRR')
                    AND A.COMPANY_CODE = '{model.COMPANY_CODE}'
                    AND A.COMPANY_CODE = B.COMPANY_CODE
                    AND B.COMPANY_CODE = C.COMPANY_CODE
                    AND IR.ITEM_CODE(+) = C.ITEM_CODE AND IR.COMPANY_CODE(+) = C.COMPANY_CODE
                    ORDER BY C.ORDER_NO DESC";
            }
            else if (model.type.ToUpper() == "D" || model.type.ToUpper() == "DISTRIBUTOR")
            {
                Query = $@"SELECT A.DISTRIBUTOR_CODE AS CODE, TO_CHAR(C.ORDER_NO) ORDER_NO, TO_CHAR(C.ORDER_DATE, 'DD-MON-YYYY') AS ORDER_DATE, 
                    C.ITEM_CODE, B.ITEM_EDESC, C.MU_CODE, TO_CHAR(C.QUANTITY) QUANTITY, TO_CHAR(C.UNIT_PRICE) UNIT_PRICE, TO_CHAR(C.TOTAL_PRICE) TOTAL_PRICE, C.REMARKS, C.APPROVED_FLAG, 
                    C.DISPATCH_FLAG, C.ACKNOWLEDGE_FLAG, C.REJECT_FLAG, TO_CHAR(NVL(IR.SALES_RATE, 0)) SALES_RATE, TO_CHAR(IR.APPLY_DATE) APPLY_DATE
                    FROM DIST_DISTRIBUTOR_MASTER A, IP_ITEM_MASTER_SETUP B, DIST_IP_SSD_PURCHASE_ORDER C,
                    (SELECT A.ITEM_CODE, A.APPLY_DATE, B.SALES_RATE, B.COMPANY_CODE
                              FROM (SELECT ITEM_CODE, COMPANY_CODE, MAX(APP_DATE) APPLY_DATE 
                                FROM IP_ITEM_RATE_APPLICAT_SETUP
                                WHERE COMPANY_CODE = '{model.COMPANY_CODE}' 
                                AND BRANCH_CODE = '{model.BRANCH_CODE}'
                                GROUP BY ITEM_CODE, COMPANY_CODE) A
                              INNER JOIN IP_ITEM_RATE_APPLICAT_SETUP B
                                ON B.ITEM_CODE = A.ITEM_CODE
                                AND B.APP_DATE = A.APPLY_DATE
                                AND B.COMPANY_CODE = '{model.COMPANY_CODE}'
                                AND B.BRANCH_CODE = '{model.BRANCH_CODE}') IR
                    WHERE A.DISTRIBUTOR_CODE = '{model.code}'
                    AND A.DISTRIBUTOR_CODE = C.CUSTOMER_CODE
                    AND B.ITEM_CODE = C.ITEM_CODE
                    AND B.GROUP_SKU_FLAG = '{GROUP_SKU_FLAG}'
                    AND B.CATEGORY_CODE = '{CATEGORY_CODE}'
                    AND TRUNC(C.ORDER_DATE) BETWEEN TO_DATE('{fromDate}','DD-MON-RRRR') AND TO_DATE('{endDate}','DD-MON-RRRR')
                    AND A.COMPANY_CODE = '{model.COMPANY_CODE}'
                    AND A.COMPANY_CODE = B.COMPANY_CODE
                    AND B.COMPANY_CODE = C.COMPANY_CODE
                    AND IR.ITEM_CODE(+) = C.ITEM_CODE AND IR.COMPANY_CODE(+) = C.COMPANY_CODE
                    ORDER BY C.ORDER_NO DESC";
            }
            else if (model.type.ToUpper() == "R" || model.type.ToUpper() == "RESELLER")
            {
                Query = $@"SELECT A.RESELLER_CODE AS CODE, TO_CHAR(C.ORDER_NO) ORDER_NO, TO_CHAR(C.ORDER_DATE, 'DD-MON-YYYY') AS ORDER_DATE, 
                    C.ITEM_CODE, B.ITEM_EDESC, C.MU_CODE, TO_CHAR(C.QUANTITY) QUANTITY, TO_CHAR(C.UNIT_PRICE) UNIT_PRICE, TO_CHAR(C.TOTAL_PRICE) TOTAL_PRICE, C.REMARKS, C.APPROVED_FLAG, 
                    C.DISPATCH_FLAG, C.ACKNOWLEDGE_FLAG, C.REJECT_FLAG, TO_CHAR(NVL(IR.SALES_RATE, 0)) SALES_RATE, TO_CHAR(IR.APPLY_DATE) APPLY_DATE
                    FROM DIST_RESELLER_MASTER A, IP_ITEM_MASTER_SETUP B, DIST_IP_SSR_PURCHASE_ORDER C,
                    (SELECT A.ITEM_CODE, A.APPLY_DATE, B.SALES_RATE, B.COMPANY_CODE
                              FROM (SELECT ITEM_CODE, COMPANY_CODE, MAX(APP_DATE) APPLY_DATE 
                                FROM IP_ITEM_RATE_APPLICAT_SETUP
                                WHERE COMPANY_CODE = '{model.COMPANY_CODE}' 
                                AND BRANCH_CODE = '{model.BRANCH_CODE}'
                                GROUP BY ITEM_CODE, COMPANY_CODE) A
                              INNER JOIN IP_ITEM_RATE_APPLICAT_SETUP B
                                ON B.ITEM_CODE = A.ITEM_CODE
                                AND B.APP_DATE = A.APPLY_DATE
                                AND B.COMPANY_CODE = '{model.COMPANY_CODE}'
                                AND B.BRANCH_CODE = '{model.BRANCH_CODE}') IR
                    WHERE A.RESELLER_CODE = '{model.code}'
                    AND A.RESELLER_CODE = C.RESELLER_CODE
                    AND B.ITEM_CODE = C.ITEM_CODE
                    AND B.GROUP_SKU_FLAG = '{GROUP_SKU_FLAG}'
                    AND B.CATEGORY_CODE = '{CATEGORY_CODE}'
                    AND TRUNC(C.ORDER_DATE) BETWEEN TO_DATE('{fromDate}','DD-MON-RRRR') AND TO_DATE('{endDate}','DD-MON-RRRR')
                    AND A.COMPANY_CODE = '{model.COMPANY_CODE}'
                    AND A.COMPANY_CODE = B.COMPANY_CODE
                    AND B.COMPANY_CODE = C.COMPANY_CODE
                    AND IR.ITEM_CODE(+) = C.ITEM_CODE AND IR.COMPANY_CODE(+) = C.COMPANY_CODE
                    ORDER BY C.ORDER_NO DESC";
            }
            else
                throw new Exception("Invalid type");
            var data = dbContext.SqlQuery<PurchaseOrderResponseModel>(Query).ToList();
            var groups = data.GroupBy(x => x.ORDER_NO);
            var result = new Dictionary<string, List<PurchaseOrderResponseModel>>();
            foreach (var group in groups)
            {
                result.Add(group.Key, group.ToList());
            }
            return result;
        }

        public List<ImageCategoryModel> FetchImageCategory(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            string Query = $@"SELECT CATEGORYID,CATEGORY_CODE,CATEGORY_EDESC,MAX_ITEMS,COMPANY_CODE FROM DIST_IMAGE_CATEGORY WHERE COMPANY_CODE='{model.COMPANY_CODE}'";
            var data = dbContext.SqlQuery<ImageCategoryModel>(Query).ToList();
            return data;
        }

        public List<ResellerEntityModel> FetchResellerEntity(EntityRequestModel model, NeoErpCoreEntity dbContext)
        {
            var data = dbContext.SqlQuery<ResellerEntityModel>($"SELECT RESELLER_CODE,ENTITY_CODE,ENTITY_TYPE,COMPANY_CODE FROM DIST_RESELLER_ENTITY WHERE DELETED_FLAG='N' AND COMPANY_CODE='{model.COMPANY_CODE}' AND RESELLER_CODE='{model.entity_code}'").ToList();
            return data;
        }

        public List<DistributorItemModel> FetchDistributorItems(EntityRequestModel model, NeoErpCoreEntity dbContext)
        {
            var data = dbContext.SqlQuery<DistributorItemModel>($"SELECT DISTRIBUTOR_CODE,ITEM_CODE,COMPANY_CODE FROM DIST_DISTRIBUTOR_ITEM WHERE DELETED_FLAG='N' AND COMPANY_CODE='{model.COMPANY_CODE}' AND DISTRIBUTOR_CODE='{model.entity_code}'").ToList();
            return data;
        }

        public List<ResellerGroupModel> GetResellerGroups(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            string Query = $"SELECT GROUPID,GROUP_EDESC,GROUP_CODE FROM DIST_GROUP_MASTER WHERE DELETED_FLAG='N' AND COMPANY_CODE='{model.COMPANY_CODE}' ORDER BY TRIM(GROUP_EDESC) ASC";
            var list = dbContext.SqlQuery<ResellerGroupModel>(Query).ToList();
            return list;
        }

        public List<ContractModel> GetContracts(CommonRequestModel model, NeoErpCoreEntity dbContext)
        {
            var query = $@"SELECT CON.CONTRACT_CODE,CON.SUPPLIER_CODE,SS.SUPPLIER_EDESC,CON.CUSTOMER_CODE,CS.CUSTOMER_EDESC,CON.CONTRACT_EDESC,
                                    CON.BRAND_CODE,CON.BRANDING_TYPE,CON.SPROVIDER_CODE,CON.START_DATE,CON.END_DATE,CON.AREA_CODE,CON.CONTRACT_TYPE,
                                    CON.AMOUNT_TYPE,CON.AMOUNT,CON.NEXT_PAYMENT_DATE,CON.PAYMENT_DATE,CON.ADVANCE_AMOUNT,CON.CONTRACTOR_NAME,
                                    CON.CONTRACTOR_ADDRESS,CON.CONTRACTOR_EMAIL,CON.CONTRACTOR_PHONE,CON.CONTRACTOR_MOBILE,CON.CONTRACTOR_DESIGNATION,
                                    CON.CONTRACTOR_PAN_NO,CON.CONTRACTOR_VAT_NO,CON.OWNER_NAME,CON.OWNER_ADDRESS,CON.OWNER_PHONE,CON.OWNER_MOBILE,
                                    CON.OWNER_COMPANY_NAME,CON.OWNER_PAN_NO,CON.OWNER_VAT_NO,CON.JOB_ORDER_NO,CON.DESCRIPTION,CON.REMARKS,CON.COMPANY_CODE,CON.BRANCH_CODE
                        FROM BRD_CONTRACT CON
                        JOIN SA_CUSTOMER_SETUP CS ON CON.CUSTOMER_CODE=CS.CUSTOMER_CODE AND CON.COMPANY_CODE=CS.COMPANY_CODE
                        JOIN IP_SUPPLIER_SETUP SS ON CON.SUPPLIER_CODE=SS.SUPPLIER_CODE AND CON.COMPANY_CODE=SS.COMPANY_CODE
                        WHERE SYSDATE BETWEEN CON.START_DATE AND CON.END_DATE
                                AND CON.AMOUNT_TYPE='SCHEME_ITEM'
                                AND CON.APPROVED_FLAG='Y'
                                AND CON.DELETED_FLAG='N'
                                AND CON.COMPANY_CODE='{model.COMPANY_CODE}'
                                AND CON.BRANCH_CODE='{model.BRANCH_CODE}'";
            var data = dbContext.SqlQuery<ContractModel>(query).ToList();
            return data;
        }


        public List<AchievementReportResponseModel> GetAchievementData(AchievementReportRequestModel model, NeoErpCoreEntity dbContext)
        {
            string subquery = String.Empty;
            string MTDfilter = String.Empty;
            string filter = String.Empty;
            string SUBfilter = String.Empty;           

            if (model.REPORT_TYPE.ToUpper() == "MTD")
            {
                
                int month = DateTime.Now.Month;
                int year = DateTime.Now.Year;
                int day = DateTime.Now.Day;
                DateConverter converter = DateConverter.ConvertToNepali(year, month, day); //converting english date to nepali date
                string monthName = converter.MonthName;
                MTDfilter = $@"HAVING FN_BS_MONTH (SUBSTR (NEPALI_MONTH, 5, 2))='{monthName}'";
               

            }
            else if (model.REPORT_TYPE.ToUpper() == "YTD")
            {
                filter = $@"AND DT.PLAN_DATE < trunc(sysdate)";


                if (model.TYPE.ToUpper() == "D")
                {
                    SUBfilter= $@"AND A.SALES_DATE < trunc(sysdate)";
                   
                }
                else if (model.TYPE.ToUpper() == "R")
                {
                    SUBfilter = $@"AND A.ORDER_DATE < trunc(sysdate)";
                    
                }
            }

            if (model.TYPE.ToUpper() == "D")
            {
                subquery = $@"SELECT A.CUSTOMER_CODE,A.COMPANY_CODE,B.BRAND_NAME,C.ITEM_EDESC,TO_NUMBER(REPLACE (SUBSTR (BS_DATE (A.SALES_DATE), 0, 7), '-')) NEPALI_MONTH,
                              0 TARGET_QUANTITY,0 TARGET_VALUE,SUM(A.QUANTITY) AS QUANTITY_ACHIVE, SUM (A.CALC_TOTAL_PRICE) ACHIVE_VALUE
                          FROM SA_SALES_INVOICE A, IP_ITEM_SPEC_SETUP B, IP_ITEM_MASTER_SETUP C
                          WHERE A.ITEM_CODE = B.ITEM_CODE(+)
                              AND A.COMPANY_CODE = B.COMPANY_CODE
                              AND B.ITEM_CODE = C.ITEM_CODE
                               AND B.COMPANY_CODE = C.COMPANY_CODE
                              AND A.DELETED_FLAG = 'N'
                              AND A.CUSTOMER_CODE = '{model.SP_CODE}'
                              AND A.COMPANY_CODE = '{model.COMPANY_CODE}'
                              AND A.BRANCH_CODE='{model.BRANCH_CODE}'
                               {SUBfilter}
                      GROUP BY A.CUSTOMER_CODE,A.COMPANY_CODE,B.BRAND_NAME,TO_NUMBER(REPLACE(SUBSTR(BS_DATE(A.SALES_DATE), 0, 7), '-')),C.ITEM_EDESC";
            }
            else if (model.TYPE.ToUpper() == "R")
            {
                subquery = $@"SELECT A.CUSTOMER_CODE,A.COMPANY_CODE,B.BRAND_NAME,C.ITEM_EDESC,TO_NUMBER(REPLACE (SUBSTR (BS_DATE (TRUNC(A.ORDER_DATE)), 0, 7), '-')) NEPALI_MONTH,
                            0 TARGET_QUANTITY,0 TARGET_VALUE,SUM (A.QUANTITY) AS QUANTITY_ACHIVE,SUM (A.TOTAL_PRICE) ACHIVE_VALUE
                        FROM DIST_IP_SSR_PURCHASE_ORDER A, IP_ITEM_SPEC_SETUP B, IP_ITEM_MASTER_SETUP C
                        WHERE A.ITEM_CODE = B.ITEM_CODE(+)
                            AND A.COMPANY_CODE = B.COMPANY_CODE
                           AND B.ITEM_CODE = C.ITEM_CODE
                             AND B.COMPANY_CODE=C.COMPANY_CODE
                            AND A.DELETED_FLAG = 'N'
                            AND A.CUSTOMER_CODE =  '{model.SP_CODE}'
                            AND A.COMPANY_CODE = '{model.COMPANY_CODE}'
                            AND A.BRANCH_CODE='{model.BRANCH_CODE}'
                            {SUBfilter}
                    GROUP BY A.CUSTOMER_CODE,A.COMPANY_CODE,B.BRAND_NAME,TO_NUMBER(REPLACE (SUBSTR (BS_DATE (TRUNC(A.ORDER_DATE)), 0, 7), '-')),C.ITEM_EDESC";
            }


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
                              AND DT.CUSTOMER_CODE = '{model.SP_CODE}'
                              AND DT.COMPANY_CODE = '{model.COMPANY_CODE}'
                              AND DT.BRANCH_CODE='{model.BRANCH_CODE}'
                              AND C.GROUP_SKU_FLAG = 'I' 
                              {filter}
                      GROUP BY DT.CUSTOMER_CODE,DT.COMPANY_CODE,TO_NUMBER(REPLACE (SUBSTR (BS_DATE (PLAN_DATE), 0, 7), '-')),
                              DT.CUSTOMER_CODE,B.BRAND_NAME,C.ITEM_EDESC
                      UNION ALL
                        {subquery})
            GROUP BY NEPALI_MONTH,CUSTOMER_CODE,BRAND_NAME,ITEM_EDESC {MTDfilter}
            ORDER BY NEPALI_MONTH";
            var list = dbContext.SqlQuery<AchievementReportResponseModel>(query).ToList();
            return list;
        }
        public List<AchievementReportResponseModel> fetchAchievementReportMonthWise(AchievementReportRequestModel model, NeoErpCoreEntity dbContext)
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
                              AND DT.CUSTOMER_CODE = '{model.SP_CODE}'
                              AND DT.COMPANY_CODE = '{model.COMPANY_CODE}'
                              AND DT.BRANCH_CODE='{model.BRANCH_CODE}'                              
                              AND C.GROUP_SKU_FLAG = 'I'
                      GROUP BY DT.CUSTOMER_CODE,DT.COMPANY_CODE,TO_NUMBER(REPLACE (SUBSTR (BS_DATE (PLAN_DATE), 0, 7), '-')),
                              DT.CUSTOMER_CODE,B.BRAND_NAME,C.ITEM_EDESC
                      UNION ALL
                        SELECT A.CUSTOMER_CODE,A.COMPANY_CODE,B.BRAND_NAME,C.ITEM_EDESC,TO_NUMBER(REPLACE (SUBSTR (BS_DATE (A.SALES_DATE), 0, 7), '-')) NEPALI_MONTH,
                              0 TARGET_QUANTITY,0 TARGET_VALUE,SUM(A.QUANTITY) AS QUANTITY_ACHIVE, SUM (A.CALC_TOTAL_PRICE) ACHIVE_VALUE
                          FROM SA_SALES_INVOICE A, IP_ITEM_SPEC_SETUP B, IP_ITEM_MASTER_SETUP C
                          WHERE A.ITEM_CODE = B.ITEM_CODE(+)
                              AND A.COMPANY_CODE = B.COMPANY_CODE
                              AND B.ITEM_CODE = C.ITEM_CODE
                               AND B.COMPANY_CODE = C.COMPANY_CODE
                              AND A.DELETED_FLAG = 'N'
                              AND A.CUSTOMER_CODE = '{model.SP_CODE}'
                              AND A.COMPANY_CODE = '{model.COMPANY_CODE}'
                              AND A.BRANCH_CODE='{model.BRANCH_CODE}'                              
                      GROUP BY A.CUSTOMER_CODE,A.COMPANY_CODE,B.BRAND_NAME,TO_NUMBER(REPLACE(SUBSTR(BS_DATE(A.SALES_DATE), 0, 7), '-')),C.ITEM_EDESC)
            GROUP BY NEPALI_MONTH,CUSTOMER_CODE,BRAND_NAME,ITEM_EDESC
            ORDER BY NEPALI_MONTH";
            var list = dbContext.SqlQuery<AchievementReportResponseModel>(query).ToList();
            return list;
        }
        public List<SchemeReportResponseModel> fetchSchemeReportData(SchemeReportRequestModel model, NeoErpCoreEntity dbContext)
        {
            List<SchemeReportResponseModel> NoItemShcemeList = new List<SchemeReportResponseModel>();
            string date = model.DATE.ToString("MM/dd/yyyy");
            string query = $@"select ds.SCHEME_ID as SchemeID ,ds.SCHEME_NAME as SchemeName , ds.Start_Date as StartDate, ds.End_Date as EndDate, ds.AREA_CODE as AreaCode, da.AREA_NAME as AreaName,  ds.OFFER_TYPE as OfferType, dm.ENTITY_CODE as SP_CODE from DIST_SCHEME ds,DIST_AREA_MASTER da,  DIST_SCHEME_ENTITY_MAPPING dm  where ds.AREA_CODE=da.AREA_CODE and  ds.Scheme_ID=dm.Scheme_ID and  TO_DATE( '{date}', 'MM/DD/RRRR' ) BETWEEN ds.Start_Date AND ds.End_Date and dm.ENTITY_CODE='{model.SP_CODE}' and   ds.COMPANY_CODE={model.COMPANY_CODE} and ds.BRANCH_CODE={model.BRANCH_CODE} and ds.DELETED_FLAG='N'";
            var schemes = dbContext.SqlQuery<SchemeReportResponseModel>(query).ToList();
            foreach(var scheme in schemes)
            {                
                var Itemquery = $@"select  distinct sc.Item_code, it.Item_Edesc from DIST_SCHEME_ITEMS sc, IP_ITEM_MASTER_SETUP it where sc.ITEM_CODE=it.ITEM_CODE and sc.SCHEME_ID={scheme.SchemeID} and sc.ITEM_CODE='{model.ITEM_CODE}'" ;
                scheme.Items = dbContext.SqlQuery<ItemDetails>(Itemquery).ToList();
                if (scheme.Items.Count != 0)
                {
                    if (scheme.OfferType == "GIFT")
                    {
                        var Ruleresult = new List<SchemeDetailModel>();
                        var Itemresult = new List<ItemDetails>();
                        var Mappingquery = $@"select distinct sr.Rule_ID, sr.Max_Value, sr.Min_Value, sr.Gift_QTY from DIST_SCHEME_GIFT_ITEMS scg, DIST_SCHEME_RULE_MAPPING sr where scg.RULE_ID=sr.RULE_ID and sr.SCHEME_ID={scheme.SchemeID}";
                        Ruleresult = dbContext.SqlQuery<SchemeDetailModel>(Mappingquery).ToList();
                        foreach (var rule in Ruleresult)
                        {
                            var query2 = $@"select distinct it.Item_code, it.Item_Edesc from DIST_SCHEME_GIFT_ITEMS scg, IP_ITEM_MASTER_SETUP it where scg.GIFT_ITEM_CODE=it.ITEM_CODE and scg.RULE_ID={rule.Rule_ID} and it.COMPANY_CODE={model.COMPANY_CODE} and it.BRANCH_CODE={model.BRANCH_CODE}";
                            Itemresult = dbContext.SqlQuery<ItemDetails>(query2).ToList();
                            foreach (var item in Itemresult)
                            {
                                rule.Gift_Items.Add(item);
                            }


                        }
                        scheme.SchemeDetails = Ruleresult;

                    }
                    else
                    {
                        var result = new List<SchemeDetailModel>();
                        var Discountquery = $@"select distinct sr.Rule_ID, sr.Max_Value, sr.Min_Value, sr.Discount, sr.Discount_Type as DiscountType from DIST_SCHEME_RULE_MAPPING sr where sr.SCHEME_ID={scheme.SchemeID}";
                        result = dbContext.SqlQuery<SchemeDetailModel>(Discountquery).ToList();
                        scheme.SchemeDetails = result;
                    }
                }
                else
                {
                    NoItemShcemeList.Add(scheme);
                }
            }
            foreach (var scheme in NoItemShcemeList)
            {
                schemes.Remove(scheme);
            }
            return schemes;
            
        }

        public List<DistributionSalesReturnModel> GetAllDistSalesReturn(CommonRequestModel requestParam, NeoErpCoreEntity dbContext)
        {
            var distSR = new List<DistributionSalesReturnModel>() {new DistributionSalesReturnModel { Id = "1", Response = "Wawooo You Hit the return API" }};
            return distSR;
        }
        #endregion Fetching Data

        #region Inserting Data
        public Dictionary<string, string> UpdateMyLocation(UpdateRequestModel model, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, string>();
            var query = string.Empty;
            if (string.IsNullOrWhiteSpace(model.sp_code))
            {
                throw new Exception("Salesperson code missing.");
            }
            if (string.IsNullOrWhiteSpace(model.latitude))
            {
                throw new Exception("Latitude missing.");
            }
            if (string.IsNullOrWhiteSpace(model.longitude))
            {
                throw new Exception("Longitude missing.");
            }
            if (string.IsNullOrWhiteSpace(model.customer_code))
            {
                throw new Exception("Distributor/Reseller code missing.");
            }
            model.is_visited = (string.IsNullOrWhiteSpace(model.is_visited)) ? "Y" : model.is_visited;
            model.destination = model.destination == null ? "X" : model.destination;

            if (model.customer_type.Equals("P", StringComparison.OrdinalIgnoreCase) || model.customer_type.Equals("DEALER", StringComparison.OrdinalIgnoreCase))
                model.customer_type = "P";
            else if (model.customer_type.Equals("D", StringComparison.OrdinalIgnoreCase) || model.customer_type.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase))
                model.customer_type = "D";
            else if (model.customer_type.Equals("R", StringComparison.OrdinalIgnoreCase) || model.customer_type.Equals("RESELLER", StringComparison.OrdinalIgnoreCase))
                model.customer_type = "R";
            else
                throw new Exception("Invalid customer type");

            query = $@"INSERT INTO DIST_LOCATION_TRACK (SP_CODE, UPDATE_DATE, LATITUDE, LONGITUDE, DESTINATION, CUSTOMER_CODE, CUSTOMER_TYPE, REMARKS, IS_VISITED,COMPANY_CODE,BRANCH_CODE)
            VALUES ('{model.sp_code}', TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}', 'MM/dd/yyyy hh24:mi:ss'), '{model.latitude}', '{model.longitude}', '{model.destination}', '{model.customer_code}',
            '{model.customer_type}', '{model.remarks}', '{model.is_visited}', '{model.COMPANY_CODE}', '{model.BRANCH_CODE}')";
            var row = dbContext.ExecuteSqlCommand(query);
            if (row > 0)
            {
                result.Add("date", DateTime.Now.ToString("yyyy-MM-dd hh:mm tt"));
                return result;
            }
            else
                throw new Exception("Unable to update location.");
        }

        public Dictionary<string, string> UpdateCurrentLocation(UpdateRequestModel model, NeoErpCoreEntity dbContext)
        {
            //validate sp_code removed since multiple company items can be inserted in eod

            //var query = $"SELECT * FROM HR_EMPLOYEE_SETUP WHERE EMPLOYEE_CODE='{model.sp_code}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
            //var user = dbContext.SqlQuery<object>(query).FirstOrDefault();
            //if (user == null)
            //    throw new Exception("Invalid User!!!");
            string insertQuery = "";
            int row;
            if (model.Track_Type == "ATN" || model.Track_Type == "EOD") //checking if EOD and ATN are already inserted or not
            {
                var prev = dbContext.SqlQuery<int>($"SELECT COUNT(*) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE ='{model.Track_Type}' AND SP_CODE = '{model.sp_code}' AND TRUNC(SUBMIT_DATE) =TRUNC(SYSDATE) AND COMPANY_CODE = '{model.COMPANY_CODE}'").FirstOrDefault();
                if (prev > 0) //if previously inserted update the EOD insert a TRK row
                {
                    insertQuery = $@"INSERT INTO DIST_LM_LOCATION_TRACKING (SP_CODE, SUBMIT_DATE, LATITUDE, LONGITUDE,COMPANY_CODE,BRANCH_CODE,TRACK_TYPE) VALUES 
                            ('{model.sp_code}',TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'{model.latitude}','{model.longitude}','{model.COMPANY_CODE}','{model.BRANCH_CODE}','TRK')";
                    if (model.Track_Type == "EOD")
                    {
                        string updateQuery = $@"UPDATE DIST_LM_LOCATION_TRACKING SET SUBMIT_DATE = TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),LATITUDE='{model.latitude}' ,LONGITUDE='{model.longitude}'
                        WHERE SP_CODE = '{model.sp_code}' AND COMPANY_CODE = '{model.COMPANY_CODE}' AND TRUNC(SUBMIT_DATE) = TRUNC(SYSDATE) AND TRACK_TYPE = '{model.Track_Type}'";
                        row = dbContext.ExecuteSqlCommand(insertQuery);
                    }
                } //if not previous entry, insert a new row for EOD/ATN
                else
                    insertQuery = $@"INSERT INTO DIST_LM_LOCATION_TRACKING (SP_CODE, SUBMIT_DATE, LATITUDE, LONGITUDE,COMPANY_CODE,BRANCH_CODE,TRACK_TYPE) VALUES 
                            ('{model.sp_code}',TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'{model.latitude}','{model.longitude}','{model.COMPANY_CODE}','{model.BRANCH_CODE}','{model.Track_Type}')";
            }
            else //insert a new TRK record
                insertQuery = $@"INSERT INTO DIST_LM_LOCATION_TRACKING (SP_CODE, SUBMIT_DATE, LATITUDE, LONGITUDE,COMPANY_CODE,BRANCH_CODE,TRACK_TYPE) VALUES 
                            ('{model.sp_code}',TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'{model.latitude}','{model.longitude}','{model.COMPANY_CODE}','{model.BRANCH_CODE}','{model.Track_Type}')";
            row = dbContext.ExecuteSqlCommand(insertQuery);
            if (model.Track_Type == "EOD") //inserting/updating a EOD details record. Maximum of one record per day for each salesperson
            {
                string queryEod = string.Empty;
                var data = dbContext.SqlQuery<object>($"SELECT * FROM DIST_EOD_UPDATE WHERE TO_DATE(CREATED_DATE)=TO_DATE(TO_DATE('{model.Time_Eod}','MM/dd/yyyy hh24:mi:ss')) AND SP_CODE='{model.sp_code}' AND COMPANY_CODE='{model.COMPANY_CODE}'").ToList();
                if (data.Count > 0)
                    queryEod = $@"UPDATE DIST_EOD_UPDATE SET PO_DCOUNT='{model.PO_DCOUNT}', PO_RCOUNT='{model.PO_RCOUNT}', RES_MASTER='{model.RES_MASTER}', RES_DETAIL='{model.RES_DETAIL}',RES_ENTITY='{model.RES_ENTITY}',
                                RES_STORE_PHOTO='{model.RES_PHOTO}',RES_CONTACT_PHOTO='{model.RES_CONTACT_PHOTO}',LATITUDE='{model.latitude}',LONGITUDE='{model.longitude}',REMARKS='{model.remarks}'
                                WHERE TO_DATE(CREATED_DATE)=TO_DATE(TO_DATE('{model.Time_Eod}','MM/dd/yyyy hh24:mi:ss')) AND SP_CODE='{model.sp_code}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
                else
                    queryEod = $@"INSERT INTO DIST_EOD_UPDATE (SP_CODE, PO_DCOUNT, PO_RCOUNT, RES_MASTER, RES_DETAIL,RES_ENTITY,RES_STORE_PHOTO,RES_CONTACT_PHOTO,LATITUDE,LONGITUDE,CREATED_DATE,CREATED_BY,COMPANY_CODE, BRANCH_CODE,REMARKS) VALUES 
                            ('{model.sp_code}','{model.PO_DCOUNT}','{model.PO_RCOUNT}','{model.RES_MASTER}','{model.RES_DETAIL}','{model.RES_ENTITY}','{model.RES_PHOTO}','{model.RES_CONTACT_PHOTO}','{model.latitude}','{model.longitude}',TO_DATE('{model.Time_Eod}','MM/dd/yyyy hh24:mi:ss'),'{model.user_id}','{model.COMPANY_CODE}','{model.BRANCH_CODE}','{model.remarks}')";

                string AttendanceQuery = $@"INSERT INTO HRIS_ATTENDANCE (EMPLOYEE_ID,ATTENDANCE_DT,ATTENDANCE_FROM,ATTENDANCE_TIME) VALUES ('{model.sp_code}',TRUNC(TO_DATE('{model.Time_Eod}','MM/dd/yyyy hh24:mi:ss')),'MOBILE',TO_TIMESTAMP('{model.Time_Eod}','MM/dd/yyyy hh24:mi:ss'))";
                row = dbContext.ExecuteSqlCommand(AttendanceQuery);
                var time = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss.fffffff tt", CultureInfo.InvariantCulture);
                try
                {
                    var thumbId = dbContext.SqlQuery<string>($"SELECT to_char(ID_THUMB_ID) FROM HRIS_EMPLOYEES WHERE EMPLOYEE_ID = '{model.sp_code}'").FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(thumbId))
                    {
                        try
                        {
                            var hris_procedure = $"BEGIN HRIS_ATTENDANCE_INSERT ({thumbId}, TRUNC(SYSDATE), NULL, 'MOBILE', TO_TIMESTAMP('{time}')); END;";
                           dbContext.ExecuteSqlCommand(hris_procedure);
                        }catch(Exception ex)
                        {
                            
                        }
                    
                    }

                    var rowCal = dbContext.ExecuteSqlCommand(queryEod);
                }
                catch(Exception ex)
                {

                }
            }
            var result = new Dictionary<string, string>();
            result.Add(model.sp_code, model.Sync_Id==null?"":model.Sync_Id);
            return result;
        }

        public bool SaveExtraActivity(UpdateRequestModel model, NeoErpCoreEntity dbContext)
        {
            var query = string.Empty;
            if (string.IsNullOrWhiteSpace(model.sp_code))
            {
                throw new Exception("Salesperson code missing.");
            }
            if (string.IsNullOrWhiteSpace(model.latitude))
            {
                throw new Exception("Latitude missing.");
            }
            if (string.IsNullOrWhiteSpace(model.longitude))
            {
                throw new Exception("Longitude missing.");
            }
            if (string.IsNullOrWhiteSpace(model.remarks))
            {
                throw new Exception("Remark is null.");
            }
            query = $@"INSERT INTO DIST_EXTRA_ACTIVITY (SP_CODE,LATITUDE,LONGITUDE,REMARKS,COMPANY_CODE,BRANCH_CODE) VALUES(
                    '{model.sp_code}','{model.latitude}','{model.longitude}','{model.remarks}','{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
            var row = dbContext.ExecuteSqlCommand(query);
            if (row > 0)
                return true;
            else
                throw new Exception("Error Processing Request");
        }

        public bool UpdateCustomerLocation(UpdateCustomerRequestModel model, NeoErpCoreEntity dbContext)
        {
            var query = string.Empty;
            int a = 0;
            a++;
            if (string.IsNullOrWhiteSpace(model.user_id))
            {
                throw new Exception("User ID missing.");
            }
            if (string.IsNullOrWhiteSpace(model.code))
            {
                throw new Exception("Customer code missing.");
            }
            if (string.IsNullOrWhiteSpace(model.latitude))
            {
                throw new Exception("Latitude missing.");
            }
            if (string.IsNullOrWhiteSpace(model.longitude))
            {
                throw new Exception("Longitude missing.");
            }
            if (string.IsNullOrWhiteSpace(model.type))
            {
                throw new Exception("Customer type missing.");
            }
            if (model.type.Equals("P", StringComparison.OrdinalIgnoreCase) || model.type.Equals("DEALER", StringComparison.OrdinalIgnoreCase))
                query = $@"UPDATE DIST_DEALER_MASTER SET LUPDATE_BY = '{model.user_id}', LUPDATE_DATE = TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}','MM/dd/yyyy hh24:mi:ss'),
                LATITUDE = '{model.latitude}', LONGITUDE = '{model.longitude}' WHERE DEALER_CODE = '{model.code}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
            else if (model.type.Equals("D", StringComparison.OrdinalIgnoreCase) || model.type.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase))
                query = $@"UPDATE DIST_DISTRIBUTOR_MASTER SET LUPDATE_BY = '{model.user_id}', LUPDATE_DATE = TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}','MM/dd/yyyy hh24:mi:ss'),
                LATITUDE = '{model.latitude}', LONGITUDE = '{model.longitude}' WHERE DISTRIBUTOR_CODE = '{model.code}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
            else if (model.type.Equals("R", StringComparison.OrdinalIgnoreCase) || model.type.Equals("RESELLER", StringComparison.OrdinalIgnoreCase))
                query = $@"UPDATE DIST_RESELLER_MASTER SET LUPDATE_BY = '{model.user_id}', LUPDATE_DATE = TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}','MM/dd/yyyy hh24:mi:ss'),
                LATITUDE = '{model.latitude}', LONGITUDE = '{model.longitude}' WHERE RESELLER_CODE = '{model.code}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
            else if (model.type.Equals("B", StringComparison.OrdinalIgnoreCase) || model.type.Equals("BRANDING", StringComparison.OrdinalIgnoreCase))
                query = $@"UPDATE BRD_OTHER_ENTITY SET LATITUDE = '{model.latitude}', LONGITUDE = '{model.longitude}', SYNC_ID='{model.Sync_Id}' WHERE CODE = '{model.code}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
            else
                throw new Exception("Invalid customer type");

            var row = dbContext.ExecuteSqlCommand(query);
            if (row > 0)
            {
                return true;
            }
            else
            {
                throw new Exception("Unable to update customer location.");
            }
        }

        public string NewPurchaseOrder(PurchaseOrderModel model, NeoErpCoreEntity dbContext)
        {
            int id = 0;
            if (model.type.Equals("P", StringComparison.OrdinalIgnoreCase) || model.type.Equals("DEALER", StringComparison.OrdinalIgnoreCase)
                || model.type.Equals("D", StringComparison.OrdinalIgnoreCase) || model.type.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase))
                id = this.GetMaxId("DIST_IP_SSD_PURCHASE_ORDER", "ORDER_NO", dbContext);
            else if (model.type.Equals("R", StringComparison.OrdinalIgnoreCase) || model.type.Equals("RESELLER", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(model.reseller_code))
                    throw new Exception("Reseller code is empty");
                id = this.GetMaxId("DIST_IP_SSR_PURCHASE_ORDER", "ORDER_NO", dbContext);
            }
            else
                throw new Exception("Invalid customer type");
            if (id <= 0)
                throw new Exception("Unable to get next ID for the purchase order.");

            var today = $"TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}','MM/dd/yyyy hh24:mi:ss')";
            foreach (var item in model.products)
            {
                item.party_type_code = item.party_type_code == null ? "" : item.party_type_code;
                string InsertQuery = string.Empty;
                string priceQuery = $"SELECT NVL(SALES_PRICE,0) SALES_PRICE FROM IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '{item.item_code}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
                decimal SP = dbContext.SqlQuery<decimal>(priceQuery).FirstOrDefault();
                item.rate = item.rate == 0 ? SP : item.rate;

                var total = item.rate * item.quantity;

                if (model.type.Equals("P", StringComparison.OrdinalIgnoreCase) || model.type.Equals("DEALER", StringComparison.OrdinalIgnoreCase)
                    || model.type.Equals("D", StringComparison.OrdinalIgnoreCase) || model.type.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase))
                    InsertQuery = $@"INSERT INTO DIST_IP_SSD_PURCHASE_ORDER (ORDER_NO,ORDER_DATE,CUSTOMER_CODE,ITEM_CODE,MU_CODE,QUANTITY,BILLING_NAME,REMARKS,UNIT_PRICE,TOTAL_PRICE,CREATED_BY,CREATED_DATE,APPROVED_FLAG,DISPATCH_FLAG,ACKNOWLEDGE_FLAG,REJECT_FLAG,DELETED_FLAG,PARTY_TYPE_CODE,CITY_CODE,SALES_TYPE_CODE,SHIPPING_CONTACT,COMPANY_CODE,BRANCH_CODE)
                            VALUES('{id}',{today},'{model.distributor_code}','{item.item_code}','{item.mu_code}','{item.quantity}','{item.billing_name}','{item.remarks}','{item.rate}','{total}','{model.user_id}',{today},'N','N','N','N','N','{item.party_type_code}','{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
                else
                    InsertQuery = $@"INSERT INTO DIST_IP_SSR_PURCHASE_ORDER (ORDER_NO,ORDER_DATE,RESELLER_CODE,CUSTOMER_CODE,ITEM_CODE,MU_CODE,QUANTITY,BILLING_NAME,REMARKS,UNIT_PRICE,TOTAL_PRICE,CREATED_BY,CREATED_DATE,APPROVED_FLAG,DISPATCH_FLAG,ACKNOWLEDGE_FLAG,REJECT_FLAG,DELETED_FLAG,PARTY_TYPE_CODE,CITY_CODE,SALES_TYPE_CODE,SHIPPING_CONTACT,COMPANY_CODE,BRANCH_CODE,DISPATCH_FROM,WHOLESELLER_CODE)
                            VALUES('{id}',{today},'{model.reseller_code}','{model.distributor_code}','{item.item_code}','{item.mu_code}','{item.quantity}','{item.billing_name}','{item.remarks}','{item.rate}','{total}','{model.user_id}',{today},'N','N','N','N','N','{item.party_type_code}','{item.Po_Shipping_Address}','{item.Po_Sales_Type}','{item.Po_Shipping_Contact}','{model.COMPANY_CODE}','{model.BRANCH_CODE}','{model.Dispatch_From}','{model.WholeSeller_Code}')";
                int rowNum = dbContext.ExecuteSqlCommand(InsertQuery);
            }
            return id.ToString();
        }

        public bool NewCollection(CollectionRequestModel model, NeoErpCoreEntity dbContext)
        {
            if (string.IsNullOrWhiteSpace(model.sp_code))
                throw new Exception("Sp code is empty");
            if (string.IsNullOrWhiteSpace(model.entity_type))
                throw new Exception("Entity type is empty");
            if (string.IsNullOrWhiteSpace(model.created_by))
                throw new Exception("Created by is empty");
            decimal Amount;
            string[] types = { "P", "D", "R" };
            if (string.IsNullOrWhiteSpace(model.amount) || !decimal.TryParse(model.amount, out Amount))
                throw new Exception("Amount should be in Number");
            if (!types.Contains(model.entity_type.ToUpper()))
                throw new Exception(@"ENITY_TYPE must be 'P' or 'D' or 'R' ");
            string insertQuery = $@"INSERT INTO DIST_COLLECTION (SP_CODE,ENTITY_CODE,ENTITY_TYPE,BILL_NO, CHEQUE_NO, BANK_NAME, AMOUNT,PAYMENT_MODE,CHEQUE_CLEARANCE_DATE,CHEQUE_DEPOSIT_BANK,LATITUDE,LONGITUDE,REMARKS,CREATED_BY,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE)
            VALUES ('{model.sp_code}','{model.entity_code}','{model.entity_type}','{model.bill_no}','{model.cheque_no}','{model.bank_name}','{model.amount}','{model.payment_mode}',TO_DATE('{model.cheque_clearance_date}','dd-mm-yyyy'),
            '{model.cheque_deposit_bank}', '{model.latitude}','{model.longitude}','{model.remarks}','{model.created_by}','N','{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
            var row = dbContext.ExecuteSqlCommand(insertQuery);
            if (row <= 0)
                throw new Exception("Unable to save collection");
            return true;
        }

        public bool NewMarketingInformation(InformationSaveModel model, NeoErpCoreEntity dbContext)
        {
            var today = $"TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy')";
            var MktCode = this.GetMaxId("DIST_MKT_INFO", "MKT_CODE", dbContext);
            if (model.entity_type.Equals("P", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("DEALER", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "P";
            else if (model.entity_type.Equals("D", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "D";
            else if (model.entity_type.Equals("R", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("RESELLER", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "R";
            else
                throw new Exception("Invalid customer type");
            string InsertQuery = $@"INSERT INTO DIST_MKT_INFO (MKT_CODE, INFO_TEXT, ENTITY_TYPE, ENTITY_CODE, USER_ID, CREATE_DATE,COMPANY_CODE,BRANCH_CODE)
                VALUES ({MktCode}, '{model.information}', '{model.entity_type}', '{model.entity_code}', '{model.user_id}', {today},'{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
            var row = dbContext.ExecuteSqlCommand(InsertQuery);
            if (row <= 0)
                throw new Exception("Unable to add marketing information.");

            return true;
        }

        public bool NewCompetitorInformation(InformationSaveModel model, NeoErpCoreEntity dbContext)
        {
            var today = $"TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy')";
            var ComptCode = this.GetMaxId("DIST_COMPT_INFO", "COMPT_CODE", dbContext);
            if (model.entity_type.Equals("P", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("DEALER", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "P";
            else if (model.entity_type.Equals("D", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "D";
            else if (model.entity_type.Equals("R", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("RESELLER", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "R";
            else
                throw new Exception("Invalid customer type");
            string InsertQuery = $@"INSERT INTO DIST_COMPT_INFO (COMPT_CODE, INFO_TEXT, ENTITY_TYPE, ENTITY_CODE, USER_ID, CREATE_DATE,COMPANY_CODE,BRANCH_CODE)
                VALUES ({ComptCode}, '{model.information}', '{model.entity_type}', '{model.entity_code}', '{model.user_id}', {today},'{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
            var row = dbContext.ExecuteSqlCommand(InsertQuery);
            if (row <= 0)
                throw new Exception("Unable to add competitor information.");

            return true;
        }

        public bool SaveQuestionaire(QuestionaireSaveModel model, NeoErpCoreEntity dbContext)
        {
            var today = $"TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy')";
            var ComptCode = this.GetMaxId("DIST_COMPT_INFO", "COMPT_CODE", dbContext);
            if (model.entity_type.Equals("P", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("DEALER", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "P";
            else if (model.entity_type.Equals("D", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "D";
            else if (model.entity_type.Equals("R", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("RESELLER", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "R";
            else
                throw new Exception("Invalid customer type");
            var AnsQuery = $"SELECT QA_CODE,ANSWER FROM DIST_QA_ANSWER WHERE CREATED_DATE ='{DateTime.Now.ToString("dd-MMM-yyyy")}' AND ENTITY_CODE='{model.entity_code}' AND ENTITY_TYPE='{model.entity_type}' AND DELETED_FLAG='N' AND COMPANY_CODE='{model.COMPANY_CODE}'";
            var answers = dbContext.SqlQuery<object>(AnsQuery).ToList();
            if (answers.Count > 0)
                throw new Exception("You Have Already Answers These Questionaire For The day!!");
            foreach (var general in model.general)
            {
                var InsertQuery = $@"INSERT INTO DIST_QA_ANSWER (SP_CODE, QA_CODE,ANSWER,ENTITY_TYPE,ENTITY_CODE,DELETED_FLAG,CREATED_DATE,CREATED_BY,COMPANY_CODE,BRANCH_CODE)
                    VALUES('{model.sp_code}','{general.qa_code}','{general.answer}','{model.entity_type}','{model.entity_code}','N',{today},'{model.entity_code}','{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
                var row = dbContext.ExecuteSqlCommand(InsertQuery);
                if (row <= 0)
                    throw new Exception("Error Processing Request.");
            }
            foreach (var tab in model.tabular)
            {
                var AnsId = this.GetMaxId("DIST_QA_TAB_CELL_ANSWER", "ANSWER_ID", dbContext);
                if (tab.answer.Length > 30)
                {
                    Byte[] bytes = Convert.FromBase64String(tab.answer);
                    var name = $"qaFile_{AnsId}.jpg";
                    File.WriteAllBytes(UploadPath + @"\QAFiles\" + name, bytes);
                    tab.answer = name;
                }
                var InsertQuery = $@"INSERT INTO DIST_QA_TAB_CELL_ANSWER (ANSWER_ID,CELL_ID,ANSWER,ENTITY_CODE,ENTITY_TYPE,SP_CODE,CREATED_DATE)
                    VALUES('{AnsId}','{tab.cell_id}','{tab.answer}','{model.entity_code}','{model.entity_type}','{model.sp_code}',{today})";
                var row = dbContext.ExecuteSqlCommand(InsertQuery);
                if (row <= 0)
                    throw new Exception("Error Processing Request.");
            }
            return true;
        }

        public UpdateEntityResponsetModel UpdateDealerStock(UpdateEntityRequestModel model, NeoErpCoreEntity dbContext)
        {
            var result = new UpdateEntityResponsetModel();
            var CheckQuery = $"SELECT * FROM DIST_DEALER_STOCK WHERE COMPANY_CODE='{model.COMPANY_CODE}' AND DEALER_CODE='{model.customer_code}' AND trunc(CREATED_DATE)='{DateTime.Now.ToString("dd-MMM-yyyy")}'";
            var data = dbContext.SqlQuery<object>(CheckQuery).ToList();
            if (data.Count > 0)
            {
                result.msg = "You Have Already updated Stock For " + DateTime.Now.ToString("dd-MMM-yyyy");
            }
            else
            {
                var today = $"TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy')";
                foreach (var stock in model.stock)
                {
                    var InsertQuery = $@"INSERT INTO DIST_DEALER_STOCK(DEALER_CODE,ITEM_CODE,MU_CODE,CURRENT_STOCK,PURCHASE_QTY,SP_CODE,CREATED_DATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE)
                        VALUES('{model.customer_code}','{stock.item_code}','{stock.mu_code}','{stock.cs}','{stock.p_qty}','{model.sp_code}',{today},'N','{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
                    var row = dbContext.ExecuteSqlCommand(InsertQuery);
                    if (row <= 0)
                        throw new Exception("Error updating the stock");
                }
                result.msg = "Stock has been successfully updated For " + DateTime.Now.ToString("dd-MMM-yyyy");
            }
            var fetchModel = new EntityRequestModel
            {
                entity_code = model.customer_code,
                BRANCH_CODE = model.BRANCH_CODE,
                COMPANY_CODE = model.COMPANY_CODE,
                entity_type = "P"
            };
            var entityList = this.FetchEntityById(fetchModel, dbContext);
            result.entity = entityList.FirstOrDefault();
            return result;
        }

        public UpdateEntityResponsetModel UpdateDistributorStock(UpdateEntityRequestModel model, NeoErpCoreEntity dbContext)
        {
            var result = new UpdateEntityResponsetModel();
            var CheckQuery = $"SELECT * FROM DIST_DISTRIBUTOR_STOCK WHERE COMPANY_CODE='{model.COMPANY_CODE}' AND DISTRIBUTOR_CODE='{model.customer_code}' AND trunc(CREATED_DATE)='{DateTime.Now.ToString("dd-MMM-yyyy")}'";
            var data = dbContext.SqlQuery<object>(CheckQuery).ToList();
            if (data.Count > 0)
            {
                result.msg = "You Have Already updated Stock For " + DateTime.Now.ToString("dd-MMM-yyyy");
            }
            else
            {
                var today = $"TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy')";
                var StockId = dbContext.SqlQuery<int>("SELECT (NVL(MAX(STOCK_ID),0)+1) MAXID FROM DIST_DISTRIBUTOR_STOCK").FirstOrDefault();
                foreach (var stock in model.stock)
                {
                    var InsertQuery = $@"INSERT INTO DIST_DISTRIBUTOR_STOCK(STOCK_ID,DISTRIBUTOR_CODE,ITEM_CODE,MU_CODE,CURRENT_STOCK,PURCHASE_QTY,SP_CODE,CREATED_DATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE)
                        VALUES('{StockId}','{model.customer_code}','{stock.item_code}','{stock.mu_code}','{stock.cs}','{stock.p_qty}','{model.sp_code}',{today},'N','{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
                    var row = dbContext.ExecuteSqlCommand(InsertQuery);
                    if (row <= 0)
                        throw new Exception("Error updating the stock");
                }
                result.msg = "Stock has been successfully updated For " + DateTime.Now.ToString("dd-MMM-yyyy");
            }
            var fetchModel = new EntityRequestModel
            {
                entity_code = model.customer_code,
                BRANCH_CODE = model.BRANCH_CODE,
                COMPANY_CODE = model.COMPANY_CODE,
                entity_type = "D"
            };
            var entityList = this.FetchEntityById(fetchModel, dbContext);
            result.entity = entityList.FirstOrDefault();
            return result;
        }

        public UpdateEntityResponsetModel UpdateResellerStock(UpdateEntityRequestModel model, NeoErpCoreEntity dbContext)
        {

            var result = new UpdateEntityResponsetModel();
            var CheckQuery = $"SELECT * FROM DIST_RESELLER_STOCK WHERE COMPANY_CODE='{model.COMPANY_CODE}' AND RESELLER_CODE='{model.customer_code}' AND trunc(CREATED_DATE)='{DateTime.Now.ToString("dd-MMM-yyyy")}'";
            var data = dbContext.SqlQuery<object>(CheckQuery).ToList();
            if (data.Count > 0)
            {
                result.msg = "You Have Already updated Stock For " + DateTime.Now.ToString("dd-MMM-yyyy");
            }
            else
            {
                var today = $"TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy')";
                foreach (var stock in model.stock)
                {
                    var InsertQuery = $@"INSERT INTO DIST_RESELLER_STOCK(RESELLER_CODE,ITEM_CODE,MU_CODE,CURRENT_STOCK,PURCHASE_QTY,SP_CODE,CREATED_DATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE)
                        VALUES('{model.customer_code}','{stock.item_code}','{stock.mu_code}','{stock.cs}','{stock.p_qty}','{model.sp_code}',{today},'N','{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
                    var row = dbContext.ExecuteSqlCommand(InsertQuery);
                    if (row <= 0)
                        throw new Exception("Error updating the stock");
                }
                result.msg = "Stock has been successfully updated For " + DateTime.Now.ToString("dd-MMM-yyyy");
            }
            var fetchModel = new EntityRequestModel
            {
                entity_code = model.customer_code,
                BRANCH_CODE = model.BRANCH_CODE,
                COMPANY_CODE = model.COMPANY_CODE,
                entity_type = "R"
            };
            var entityList = this.FetchEntityById(fetchModel, dbContext);
            result.entity = entityList.FirstOrDefault();
            return result;
        }

        public EntityResponseModel CreateReseller(CreateResellerModel model, HttpFileCollection Files, Dictionary<string, string> descriptions, NeoErpCoreEntity dbContext)
        {
            //primary contact
            var primary = new ContactModel();
            foreach (var c in model.contact)
                if (c.primary.Equals("Y", StringComparison.OrdinalIgnoreCase))
                    primary = c;
            if (primary != null)
                model.contact.Remove(primary);

            if (string.IsNullOrWhiteSpace(model.address))
                throw new Exception("Address is empty.");
            if (string.IsNullOrWhiteSpace(model.latitude))
                throw new Exception("Latitude is empty.");
            if (string.IsNullOrWhiteSpace(model.longitude))
                throw new Exception("Longitude is empty.");
            if (string.IsNullOrWhiteSpace(model.area_code))
                throw new Exception("Area code not selected.");
            string testQuery = $"SELECT * FROM DIST_RESELLER_MASTER WHERE RESELLER_NAME = '{model.reseller_name}' AND PAN_NO = '{model.pan}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
            var testObj = dbContext.SqlQuery<object>(testQuery).ToList();
            if (testObj.Count > 0)
                throw new Exception("Reseller with the provided name and PAN no. already exists.");
            //Generate reseller code
            string RCodeQuery = $"SELECT 'R-{model.user_id.Trim()}-'||TO_CHAR(SYSDATE,'YYMMDD-HH24MMSS') FROM DUAL";
            string ResellerCode = dbContext.SqlQuery<string>(RCodeQuery).FirstOrDefault();

            //insert reseller
            string ResellerInsert = $@"INSERT INTO DIST_RESELLER_MASTER
                        (RESELLER_CODE,RESELLER_NAME,REG_OFFICE_ADDRESS,EMAIL,PAN_NO,LATITUDE,LONGITUDE,WHOLESELLER,AREA_CODE,CONTACT_SUFFIX,CONTACT_NAME,CONTACT_NO,OUTLET_TYPE_ID,OUTLET_SUBTYPE_ID,GROUPID,CREATED_BY,CREATED_DATE,COMPANY_CODE,BRANCH_CODE,RESELLER_CONTACT,SOURCE,ACTIVE,TEMP_ROUTE_CODE) VALUES 
                        ('{ResellerCode}','{model.reseller_name}','{model.address}','{model.email}','{model.pan}','{model.latitude}','{model.longitude}','{model.wholeseller}','{model.area_code}','{primary.contact_suffix}','{primary.name}',
                        '{primary.number}','{model.type_id}','{model.subtype_id}','{model.Group_id}','{model.user_id}',TO_DATE(SYSDATE),'{model.COMPANY_CODE}','{model.BRANCH_CODE}','{model.Reseller_contact}','MOB','N','{model.ROUTE_CODE}')";
            var row = dbContext.ExecuteSqlCommand(ResellerInsert);

            //insert contact details
            foreach (var con in model.contact)
            {
                string ContactQuery = $@"INSERT INTO DIST_RESELLER_DETAIL(RESELLER_CODE,COMPANY_CODE,CONTACT_SUFFIX,CONTACT_NAME,CONTACT_NO,DESIGNATION,CREATED_BY,CREATED_DATE) VALUES
                            ('{ResellerCode}','{model.COMPANY_CODE}','{con.contact_suffix}','{con.name}','{con.number}','{con.designation}','{model.user_id}',TO_DATE(SYSDATE))";
                row = dbContext.ExecuteSqlCommand(ContactQuery);
            }

            List<string> dist = new List<string>();
            List<string> Who = new List<string>();
            if (!string.IsNullOrWhiteSpace(model.distributor_code))
                dist = model.distributor_code.Replace(" ", string.Empty).Split(',').ToList();
            if (!string.IsNullOrWhiteSpace(model.wholeseller_code))
                Who = model.wholeseller_code.Replace(" ", string.Empty).Split(',').ToList();

            foreach (var distributor in dist)
            {
                string disInsertQuery = $@"INSERT INTO DIST_RESELLER_ENTITY (RESELLER_CODE,ENTITY_CODE,ENTITY_TYPE,CREATED_BY,CREATED_DATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE)
                                                VALUES('{ResellerCode}','{distributor}','D','{model.user_id}',TO_DATE(SYSDATE),'N','{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
                row = dbContext.ExecuteSqlCommand(disInsertQuery);
            }
            foreach (var wholeseller in Who)
            {
                string whoInsertQuery = $@"INSERT INTO DIST_RESELLER_ENTITY (RESELLER_CODE,ENTITY_CODE,ENTITY_TYPE,CREATED_BY,CREATED_DATE,DELETED_FLAG,COMPANY_CODE,BRANCH_CODE)
                                                VALUES('{ResellerCode}','{wholeseller}','W','{model.user_id}',TO_DATE(SYSDATE),'N','{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
                row = dbContext.ExecuteSqlCommand(whoInsertQuery);
            }

            //upload files
            foreach (string tagName in Files)
            {
                HttpPostedFile file = Files[tagName];
                string ResellerPath = string.Empty;

                ResellerPath = UploadPath + "\\ResellerImages";

                if (!Directory.Exists(ResellerPath))
                    Directory.CreateDirectory(ResellerPath);
                string FileName = string.Format("{0}{1}", ResellerCode, Path.GetExtension(file.FileName));
                string filePath = Path.Combine(ResellerPath, FileName);
                int count = 1;
                while (File.Exists(filePath))
                {
                    FileName = string.Format("{0}_{1}{2}", ResellerCode, count++, Path.GetExtension(file.FileName));
                    filePath = Path.Combine(ResellerPath, FileName);
                }
                string mediaType;
                if (tagName.IndexOf("store", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    mediaType = "STORE";
                    file.SaveAs(filePath);
                }
                else if (tagName.IndexOf("pcontact", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    mediaType = "PCONTACT";
                    file.SaveAs(filePath);
                }
                else
                    continue;
                string ImageQuery = $@"INSERT INTO DIST_PHOTO_INFO (FILENAME,DESCRIPTION,ENTITY_TYPE,ENTITY_CODE,MEDIA_TYPE,CREATED_BY,CREATE_DATE,COMPANY_CODE,BRANCH_CODE) VALUES
                            ('{FileName}','{descriptions[tagName]}','R','{ResellerCode}','{mediaType}','{model.user_id}',SYSDATE,'{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
                row = dbContext.ExecuteSqlCommand(ImageQuery);
            }

            var fetchModel = new EntityRequestModel
            {
                entity_code = ResellerCode,
                BRANCH_CODE = model.BRANCH_CODE,
                COMPANY_CODE = model.COMPANY_CODE,
                entity_type = "R"
            };
            var entityList = this.FetchEntityById(fetchModel, dbContext);
            var result = entityList.FirstOrDefault();
            return result;
        }

        public string UpdateReseller(CreateResellerModel model, NeoErpCoreEntity dbContext)
        {

            if (string.IsNullOrWhiteSpace(model.reseller_code))
                throw new Exception("Reseller code is empty.");
            //if (string.IsNullOrWhiteSpace(model.address))
            //    throw new Exception("Address is empty.");o
            //if (model.contact.Count == 0)
            //    model.contact.Add(new ContactModel());

            //update reseller
            //string ResellerInsert = $@"UPDATE DIST_RESELLER_MASTER
            //            SET RESELLER_NAME='{model.reseller_name}',REG_OFFICE_ADDRESS='{model.address}',EMAIL='{model.email}',CONTACT_SUFFIX='{model.contact[0].contact_suffix}',
            //            CONTACT_NAME='{model.contact[0].name}',CONTACT_NO='{model.contact[0].number}',OUTLET_TYPE_ID='{model.type_id}',OUTLET_SUBTYPE_ID='{model.subtype_id}',
            //            RESELLER_CONTACT='{model.Reseller_contact}',LUPDATE_BY='{model.user_id}',LUPDATE_DATE=SYSDATE
            //            WHERE RESELLER_CODE='{model.reseller_code}' AND COMPANY_CODE='{model.COMPANY_CODE}'";

            string ResellerInsert = $@"UPDATE DIST_RESELLER_MASTER
                        SET REG_OFFICE_ADDRESS='{model.address}',EMAIL='{model.email}',CONTACT_NO='{model.Reseller_contact}',
                        LUPDATE_BY='{model.user_id}',LUPDATE_DATE=SYSDATE
                        WHERE RESELLER_CODE='{model.reseller_code}' AND COMPANY_CODE='{model.COMPANY_CODE}'";

            var row = dbContext.ExecuteSqlCommand(ResellerInsert);
            return "Reseller Updated Successfully";
        }

        public Dictionary<string, string> UploadEntityMedia(EntityRequestModel model, HttpFileCollection files, Dictionary<string, ImageSaveModel> descriptions, NeoErpCoreEntity dbContext)
        {
            model.Sync_Id = model.Sync_Id == null ? "" : model.Sync_Id;
            var result = new Dictionary<string, string>();
            if (model.entity_type.Equals("P", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("DEALER", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "P";
            else if (model.entity_type.Equals("D", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "D";
            else if (model.entity_type.Equals("R", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("RESELLER", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "R";
            else if (model.entity_type.Equals("B", StringComparison.OrdinalIgnoreCase) || model.entity_type.Equals("BRANDING", StringComparison.OrdinalIgnoreCase))
                model.entity_type = "B";
            else
                throw new Exception("Invalid customer type");

            int row = 0;
            var folderpath = UploadPath + "\\EntityImages";
            foreach (string tagName in files)
            {
                HttpPostedFile file = files[tagName];
                var ImageId = this.GetMaxId("DIST_VISIT_IMAGE", "IMAGE_CODE", dbContext);
                if (!Directory.Exists(folderpath))
                    Directory.CreateDirectory(folderpath);
                string FileName = string.Format("{0}{1}{2}", "EntityImage", ImageId, Path.GetExtension(file.FileName));
                string filePath = Path.Combine(folderpath, FileName);
                int count = 1;
                while (File.Exists(filePath))
                {
                    FileName = string.Format("{0}{1}_{2}{3}", "EntityImage", ImageId, count++, Path.GetExtension(file.FileName));
                    filePath = Path.Combine(folderpath, FileName);
                }
                file.SaveAs(filePath);

                //model.ACC_CODE is actually sp_code
                var InsertQuery = $@"INSERT INTO DIST_VISIT_IMAGE (IMAGE_CODE,IMAGE_NAME,IMAGE_TITLE,IMAGE_DESC,SP_CODE,ENTITY_CODE,TYPE,UPLOAD_DATE,LONGITUDE,LATITUDE,CATEGORYID,COMPANY_CODE,BRANCH_CODE,SYNC_ID)
                                    VALUES ({ImageId}, '{FileName}', '{DBNull.Value}', '{descriptions[tagName].Description}', '{model.ACC_CODE}', '{model.entity_code}', '{model.entity_type}', SYSDATE,'0', '0','{descriptions[tagName].CategoryId}','{model.COMPANY_CODE}', '{model.BRANCH_CODE}','{model.Sync_Id}')";
                row += dbContext.ExecuteSqlCommand(InsertQuery);
            }
            result.Add("msg", "Image Successfully Uploaded");
            return result;
        }

        public Dictionary<string, string> UploadAttendencePic(EntityRequestModel model, HttpFileCollection Files, Dictionary<string, string> descriptions, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, string>();
            var today = DateTime.Now.ToString("MM/dd/yyyyHH:mm:ss");
            var todayAttendence = dbContext.SqlQuery<object>($"SELECT * FROM HRIS_ATTENDANCE WHERE EMPLOYEE_ID='{model.entity_code}' AND TO_DATE(ATTENDANCE_DT)=TO_DATE(SYSDATE)").ToList();
            //if (todayAttendence.Count <= 0)
            // {
            foreach (string tagName in Files)
            {
                HttpPostedFile file = Files[tagName];
                string UserFolderpath = string.Empty;

                UserFolderpath = UploadPath + "\\AttnImages";  //model.entity_code is the sp_code(sales person code)

                if (!Directory.Exists(UserFolderpath))
                    Directory.CreateDirectory(UserFolderpath);
                string FileName = string.Format("ATTN_{0}_{1}{2}", model.entity_code, DateTime.Now.ToString("dd-MM-yyyy"), Path.GetExtension(file.FileName));
                string filePath = Path.Combine(UserFolderpath, FileName);
                int count = 1;
                while (File.Exists(filePath))
                {
                    FileName = string.Format("ATTN_{0}_{1}_{2}{3}", model.entity_code, DateTime.Now.ToString("dd-MM-yyyy"), count++, Path.GetExtension(file.FileName));
                    filePath = Path.Combine(UserFolderpath, FileName);
                }

                file.SaveAs(filePath);

                string ImageQuery = $@"INSERT INTO DIST_PHOTO_INFO (FILENAME,DESCRIPTION,ENTITY_TYPE,ENTITY_CODE,MEDIA_TYPE,CATEGORYID,CREATED_BY,CREATE_DATE,COMPANY_CODE,BRANCH_CODE) VALUES
                            ('{FileName}','{descriptions[tagName]}','S','{model.entity_code}','ATTN',1,'{model.user_id}',TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'),'{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
                var row = dbContext.ExecuteSqlCommand(ImageQuery);
            }
            string Atten = dbContext.SqlQuery<string>($"SELECT ATN_DEFAULT FROM DIST_PREFERENCE_SETUP WHERE COMPANY_CODE='{model.COMPANY_CODE}'").FirstOrDefault();
            Atten = string.IsNullOrWhiteSpace(Atten) ? "N" : Atten.Trim();
            string resultValue = "Image successfully uploaded";
            if (Atten == "Y")
            {
                string type = dbContext.SqlQuery<string>($"SELECT USER_TYPE FROM DIST_LOGIN_USER WHERE SP_CODE='{model.entity_code}' AND COMPANY_CODE='{model.COMPANY_CODE}'").FirstOrDefault();
                type = string.IsNullOrWhiteSpace(type) ? "N" : type.Trim();

                if (type.ToUpper() == "E" || type.ToUpper() == "S")
                {
                    string AttendanceQuery = $@"INSERT INTO HRIS_ATTENDANCE (EMPLOYEE_ID,ATTENDANCE_DT,ATTENDANCE_FROM,ATTENDANCE_TIME) VALUES ('{model.entity_code}',TRUNC(TO_DATE('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss')),'MOBILE',TO_TIMESTAMP('{model.Saved_Date}','MM/dd/yyyy hh24:mi:ss'))";
                    var row = dbContext.ExecuteSqlCommand(AttendanceQuery);
                    var time = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss.fffffff tt", CultureInfo.InvariantCulture);
                    try
                    {
                        var thumbId = dbContext.SqlQuery<string>($"SELECT ID_THUMB_ID FROM HRIS_EMPLOYEES WHERE EMPLOYEE_ID = '{model.entity_code}'").FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(thumbId))
                        {
                            var hris_procedure = $"BEGIN HRIS_ATTENDANCE_INSERT ({thumbId}, TRUNC(SYSDATE), NULL, 'MOBILE', TO_TIMESTAMP('{time}')); END;";
                            dbContext.ExecuteSqlCommand(hris_procedure);
                        }
                    }
                    catch(Exception ex)
                    {

                    }
                    if (row <= 0)
                        resultValue += " But could not make attendence made";
                    else
                        resultValue += " And attendence made";
                }
            }
            result.Add("msg", resultValue);
            // }
            // else
            // {
            //  result.Add("msg", "Attendence Successful");
            // }
            return result;
        }

        public Dictionary<string, string> UploadDistSalesReturnPic(NameValueCollection form, HttpFileCollection Files, NeoErpCoreEntity dbContext)
        {
            var result = new Dictionary<string, string>();
            var today = DateTime.Now.ToString("MM/dd/yyyyHH:mm:ss");
           // var time = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            var coll = new Dictionary<string, string>();
            string item_code = string.Empty;
            int itemCount = 0;
            foreach (string tagName in Files)
            {
                HttpPostedFile file = Files[tagName];
                string UserFolderpath = string.Empty;

                coll.Add(tagName, form[$"description"]);
                UserFolderpath = UploadPath + "\\DistSalesReturnImages";  //model.entity_code is the sp_code(sales person code)

                if (!Directory.Exists(UserFolderpath))
                    Directory.CreateDirectory(UserFolderpath);
                string FileName = string.Format("DSR_{0}_{1}{2}", form["itemcode[" + itemCount + "]"],form["order_no["+itemCount+"]"], Path.GetExtension(file.FileName));
                string filePath = Path.Combine(UserFolderpath, FileName);
                int count = 1;
                while (File.Exists(filePath))
                {
                    FileName = string.Format("DSR_{0}_{1}_{2}{3}", form["itemcode[" + itemCount + "]"], form["order_no[" + itemCount + "]"],itemCount, Path.GetExtension(file.FileName));
                    filePath = Path.Combine(UserFolderpath, FileName);
                    break;
                }

                file.SaveAs(filePath);

                string ImageQuery = $@"INSERT INTO DIST_PHOTO_INFO (FILENAME,DESCRIPTION,ENTITY_TYPE,ENTITY_CODE,MEDIA_TYPE,CATEGORYID,CREATED_BY,CREATE_DATE,COMPANY_CODE,BRANCH_CODE) VALUES
                            ('{FileName}','{coll[tagName]}','R','{form["SP_CODE"]}','GENERAL',1,'{form["SP_CODE"]}',TO_DATE('{today}','MM/dd/yyyy hh24:mi:ss'),'{form["COMPANY_CODE"]}','{form["BRANCH_CODE"]}')";
                var row = dbContext.ExecuteSqlCommand(ImageQuery);
                itemCount++;
            }
           
            string resultValue = "Image successfully uploaded";
            
            result.Add("msg", resultValue);
            // }
            // else
            // {
            //  result.Add("msg", "Attendence Successful");
            // }
            return result;
        }

        public string SaveScheme(SchemeModel model, NeoErpCoreEntity dbContext)
        {
            try
            {
                var SchemeId = dbContext.SqlQuery<int>("SELECT NVL(MAX(SCHEME_CODE),0)+1 S_ID FROM BRD_SCHEME").FirstOrDefault();
                string InsertQuery = $@"INSERT INTO BRD_SCHEME (SCHEME_CODE, CONTRACT_CODE, RESELLER_CODE, EMPLOYEE_CODE, ITEM_CODE, QUANTITY, MU_CODE, END_USER,
                        DIVISION_CODE, BRAND_CODE, HANDOVER_DATE, CREATED_BY, CREATED_DATE, DELETED_FLAG) VALUES(
                        '{SchemeId}','{model.CONTRACT_CODE}','{model.RESELLER_CODE}','{model.user_id}','{model.ITEM_CODE}','{model.QUANTITY}','{model.MU_CODE}','{model.END_USER}',
                        '{model.DIVISION_CODE}','{model.BRAND_CODE}',TO_DATE('{model.HANDOVER_DATE.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR'),'{model.user_id}',SYSDATE,'N')";
                dbContext.ExecuteSqlCommand(InsertQuery);
            }
            catch
            {
                throw new Exception("Something went Wrong");
            }
            return "Success";
        }
        #endregion Inserting Data

        #region Sending Mail
        public bool SendEODMail(List<UpdateEodUpdate> model, NeoErpCoreEntity dbContext)
        {
            var SpCode = model[0].sp_code;
            var Company = model[0].COMPANY_CODE;

            //Old Running -aaku
            //var emailQuery = $@"(SELECT EMAIL FROM DIST_LOGIN_USER WHERE SP_CODE='{SpCode}')
            //                UNION ALL
            //                (SELECT EMAIL FROM DIST_LOGIN_USER WHERE USERID IN (SELECT PARENT_USERID FROM DIST_LOGIN_USER WHERE SP_CODE='{SpCode}'))";


            var emailQuery = $@"SELECT EMAIL  FROM DIST_LOGIN_USER START WITH SP_CODE = '{SpCode}' CONNECT BY PRIOR  PARENT_USERID = USERID 
                                         UNION  
                                        SELECT EMAIL FROM DIST_LOGIN_USER WHERE SUPER_USER='Y'";

            var parentEmail = dbContext.SqlQuery<string>(emailQuery).ToList();
            //List<string> parentEmail = new List<string>();
            //parentEmail.Add("animesh.gautam@neosoftware.com.np");
            ////parentEmail.Add("ashok.chhetri@neosoftware.com.np");

            //parentEmail.Add("bikalp.karn@neosoftware.com.np");

            if (parentEmail.Count > 0)
            {
                var SpName = dbContext.SqlQuery<string>($"SELECT FULL_NAME FROM DIST_LOGIN_USER WHERE SP_CODE='{SpCode}'").FirstOrDefault();
                //sending mail start
                var mailModel = new Core.Models.CustomModels.MailListModel()
                {
                    EMAIL_TO = string.Join(",", parentEmail),
                    SUBJECT = $"End Of Day Update Of {SpName}({SpCode})",
                    MESSAGE = "Attachments"
                };
                var companyName = dbContext.SqlQuery<string>($@"SELECT COMPANY_EDESC FROM COMPANY_SETUP WHERE COMPANY_CODE='{Company}'").FirstOrDefault();
                var ResPOQuery = String.Empty;
                var SalesPersonQuery = String.Empty;
                if (companyName.Equals("Bhudeo Khadya Udyog P. Ltd."))
                {
                     ResPOQuery = $@"   SELECT WM_CONCAT(DISTINCT ROUTE_NAME) ROUTE_NAME, GROUP_EDESC,SP_CODE, EMPLOYEE_EDESC, ASSIGN_DATE,ATN_TIME,EOD_TIME,WORKING_HOURS,
sum(TARGET) TARGET,sum(VISITED) TARGET_VISITED,sum(TOTAL_VISITED) VISITED,sum(EXTRA) EXTRA,sum(NOT_VISITED) NOT_VISITED,
sum(TOTAL_PJP) TOTAL_PJP,sum(PJP) PJP_PRODUCTIVE,sum(NON_PJP) PJP_NON_PRODUCTIVE,
sum(NON_N_PJP) NPJP_PRODUCTIVE,sum(TOTAL_QUANTITY) PJP_TOTAL_QUANTITY,sum(TOTAL_PRICE) PJP_TOTAL_AMOUNT,
ROUND( (sum(TOTAL_VISITED)/DECODE(sum(TARGET),0,1,sum(TARGET))  * 100),2)  PERCENT_EFFECTIVE_CALLS,
ROUND( (sum(TOTAL_PJP)/DECODE(sum(TOTAL_VISITED),0,1,sum(TOTAL_VISITED)) * 100),2)  PERCENT_PRODUCTIVE_CALLS,
EOD_REMARKS
FROM(
SELECT WM_CONCAT(DISTINCT ROUTE_NAME) ROUTE_NAME,GROUP_EDESC,SP_CODE, FULL_NAME EMPLOYEE_EDESC,TRUNC(ASSIGN_DATE) ASSIGN_DATE,
TO_CHAR(ATN_TIME,'HH:MI:SS A.M.') ATN_TIME,
CASE WHEN ATN_TIME = EOD_TIME THEN NULL
ELSE TO_CHAR(EOD_TIME,'HH:MI:SS A.M.')
END EOD_TIME,
--TO_CHAR(EOD_TIME,'HH:MI:SS A.M.') EOD_TIME,
NVL(ROUND(24 * (EOD_TIME - ATN_TIME),2),0) WORKING_HOURS,
SUM(TARGET) TARGET,
SUM(VISITED) VISITED
,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = AA.USERID AND COMPANY_CODE = AA.COMPANY_CODE AND TRUNC(UPDATE_DATE)= TRUNC(AA.ASSIGN_DATE)),0) TOTAL_VISITED
, SUM(NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = AA.USERID AND COMPANY_CODE = AA.COMPANY_CODE AND TRUNC(UPDATE_DATE)= TRUNC(AA.ASSIGN_DATE)),0)  - VISITED) EXTRA, SUM(TARGET- VISITED) NOT_VISITED
,SUM(PJP) PJP
, SUM(VISITED - PJP)  NON_PJP
, SUM(NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = AA.USERID AND COMPANY_CODE = AA.COMPANY_CODE AND TRUNC(ORDER_DATE) = TRUNC(AA.ASSIGN_DATE)),0)- PJP) NON_N_PJP
,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = AA.USERID AND COMPANY_CODE = AA.COMPANY_CODE AND TRUNC(ORDER_DATE) = TRUNC(AA.ASSIGN_DATE) ),0) TOTAL_PJP
,NVL((SELECT SUM(QUANTITY)  FROM DIST_VISITED_PO WHERE USERID = AA.USERID AND COMPANY_CODE = AA.COMPANY_CODE AND TRUNC(ORDER_DATE) = TRUNC(AA.ASSIGN_DATE)),0) TOTAL_QUANTITY
,NVL((SELECT SUM(TOTAL_PRICE)  FROM DIST_VISITED_PO WHERE USERID = AA.USERID AND COMPANY_CODE = AA.COMPANY_CODE AND TRUNC(ORDER_DATE) =TRUNC(AA.ASSIGN_DATE)),0) TOTAL_PRICE                   
,EOD_REMARKS
FROM(
SELECT  B.ROUTE_NAME ROUTE_NAME,  B.GROUP_EDESC, A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE, B.COMPANY_CODE
,(SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='ATN' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) =TRUNC(B.ASSIGN_DATE)) ATN_TIME
,(SELECT TO_DATE(TO_CHAR(MAX(ATTENDANCE_TIME),'DD/MM/YYYY HH:MI:SS AM'),'DD/MM/YYYY HH:MI:SS AM') FROM HRIS_ATTENDANCE WHERE EMPLOYEE_ID = A.SP_CODE AND TRUNC(ATTENDANCE_DT) = TRUNC(B.ASSIGN_DATE)) EOD_TIME
,(SELECT REMARKS FROM DIST_EOD_UPDATE WHERE TRUNC(CREATED_DATE) = TO_DATE(B.ASSIGN_DATE,'RRRR-MON-DD')  AND SP_CODE =  A.SP_CODE AND ROWNUM = 1) EOD_REMARKS
,CASE WHEN WM_CONCAT(B.ENTITY_CODE) IS NULL THEN 0
ELSE NVL(COUNT(*),0)
END TARGET
,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE) = trunc(B.ASSIGN_DATE) AND CUSTOMER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE  AND ROUTE_CODE = B.ROUTE_CODE AND TRUNC(ASSIGN_DATE) = trunc(B.ASSIGN_DATE) )),0) VISITED
,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) =trunc(B.ASSIGN_DATE) AND RESELLER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND ROUTE_CODE = B.ROUTE_CODE AND TRUNC(ASSIGN_DATE) = trunc(B.ASSIGN_DATE))),0) PJP
FROM DIST_LOGIN_USER A, DIST_TARGET_ENTITY B  
WHERE A.USERID = B.USERID
AND A.COMPANY_CODE = B.COMPANY_CODE
AND A.ACTIVE = 'Y'
AND A.COMPANY_CODE IN ('01')/*('{Company}')*/
AND B.ASSIGN_DATE  BETWEEN TO_DATE('2021-Dec-15','RRRR-MON-DD') AND TO_DATE('2021-Dec-15','RRRR-MON-DD') /*sysdate*/
GROUP BY A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE, A.COMPANY_CODE,B.ROUTE_CODE, B.ROUTE_NAME,B.GROUP_EDESC, B.COMPANY_CODE
ORDER BY B.ASSIGN_DATE) AA
WHERE 1=1  
 AND SP_CODE IN  ('1001030') /*('{SpCode}')*/ 
 GROUP BY  USERID, COMPANY_CODE,TRUNC(ASSIGN_DATE),  ATN_TIME,EOD_TIME, SP_CODE,GROUP_EDESC,SP_CODE, FULL_NAME,EOD_REMARKS)  group by   ASSIGN_DATE,ATN_TIME,EOD_TIME, SP_CODE,GROUP_EDESC,SP_CODE,EOD_REMARKS,EMPLOYEE_EDESC,WORKING_HOURS  order by sp_code
 ";


                     SalesPersonQuery = $@"SELECT * FROM (
                                SELECT DPO1.ORDER_NO, DPO1.ORDER_DATE,BS_DATE(TO_CHAR(DPO1.ORDER_DATE)) MITI, DPO1.CUSTOMER_CODE, DPO1.BILLING_NAME CUSTOMER_EDESC, '' RESELLER_NAME, 'D' ORDER_ENTITY, TRIM(IMS.ITEM_EDESC) ITEM_EDESC, 
                                        DPO1.MU_CODE, DPO1.QUANTITY, DPO1.UNIT_PRICE, DPO1.TOTAL_PRICE NET_TOTAL, IUS.MU_CODE CONVERSION_MU_CODE, IUS.CONVERSION_FACTOR,
                                         DPO1.PARTY_TYPE_CODE,
                                        (CASE WHEN DPO1.PARTY_TYPE_CODE IS NULL
                                          THEN FN_FETCH_DESC (DPO1.COMPANY_CODE,'IP_PARTY_TYPE_CODE',CS.PARTY_TYPE_CODE)
                                          ELSE FN_FETCH_DESC (DPO1.COMPANY_CODE,'IP_PARTY_TYPE_CODE',DPO1.PARTY_TYPE_CODE)
                                        END
                                        ) PARTY_TYPE_EDESC,
                                        DPO1.CREATED_BY, DPO1.CREATED_DATE, DPO1.DELETED_FLAG,
                                        --DPO1.COMPANY_CODE, DPO1.BRANCH_CODE,
                                        DPO1.REMARKS,
                                        DPO1.APPROVED_FLAG, DPO1.DISPATCH_FLAG, DPO1.ACKNOWLEDGE_FLAG, DPO1.REJECT_FLAG,
                                        ES.EMPLOYEE_EDESC,
                                        PS.PO_PARTY_TYPE,
                                        PS.PO_CONVERSION_UNIT,
                                        PS.PO_CONVERSION_FACTOR,
                                        PS.SO_CREDIT_LIMIT_CHK SO_CREDIT_LIMIT_FLAG,
                                        NVL(DPO2.TOTAL_QUANTITY,0) TOTAL_QUANTITY,
                                        DPO2.TOTAL_AMOUNT Grand_Total_Amount,
                                        NVL(DPO2.TOTAL_APPROVE_QTY,0) GRAND_APPROVE_QUENTITY,
                                        NVL(DPO2.TOTAL_APPROVE_AMT,0) TOTAL_APPROVE_AMT,'Distributor' EntityName
                                FROM DIST_IP_SSD_PURCHASE_ORDER DPO1
                                INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DPO1.ITEM_CODE AND IMS.COMPANY_CODE = DPO1.COMPANY_CODE AND IMS.CATEGORY_CODE in (select CATEGORY_CODE  from IP_CATEGORY_CODE WHERE CATEGORY_TYPE IN ('FG','TF') and company_code='01') AND IMS.GROUP_SKU_FLAG = 'I'
                                LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DPO1.CUSTOMER_CODE AND CS.COMPANY_CODE = DPO1.COMPANY_CODE
                                INNER JOIN DIST_LOGIN_USER LU ON LU.USERID = DPO1.CREATED_BY AND LU.ACTIVE = 'Y'
                                INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = LU.SP_CODE AND ES.COMPANY_CODE = LU.COMPANY_CODE
                                LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON IUS.ITEM_CODE = DPO1.ITEM_CODE AND IUS.COMPANY_CODE = DPO1.COMPANY_CODE
                                INNER JOIN DIST_PREFERENCE_SETUP PS ON PS.COMPANY_CODE = DPO1.COMPANY_CODE
                                INNER JOIN (SELECT POT.ORDER_NO, SUM(POT.NET_QUANTITY) TOTAL_QUANTITY, SUM(POT.NET_PRICE) TOTAL_AMOUNT, SUM(POT.APPROVE_QTY) TOTAL_APPROVE_QTY, SUM(POT.APPROVE_AMT) TOTAL_APPROVE_AMT
                                FROM (SELECT A.ORDER_NO, A.ITEM_CODE, A.MU_CODE, A.QUANTITY, A.TOTAL_PRICE NET_PRICE, A.APPROVE_QTY, A.APPROVE_AMT, C.MU_CODE AS CONVERSION_UNIT, C.CONVERSION_FACTOR,
                                      (CASE
                                        WHEN (C.MU_CODE IS NULL AND C.CONVERSION_FACTOR IS NULL)
                                        THEN A.QUANTITY
                                        ELSE (CASE WHEN A.MU_CODE = C.MU_CODE THEN A.QUANTITY ELSE (A.QUANTITY * C.CONVERSION_FACTOR) END)
                                      END) NET_QUANTITY
                                      FROM DIST_IP_SSD_PURCHASE_ORDER A
                                      LEFT JOIN IP_ITEM_UNIT_SETUP C ON C.ITEM_CODE = A.ITEM_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                ) POT
                                GROUP BY POT.ORDER_NO) DPO2 ON DPO2.ORDER_NO = DPO1.ORDER_NO
                                WHERE 1 = 1
                                      AND TRUNC(DPO1.ORDER_DATE) BETWEEN TO_DATE('2021-Jul-16','YYYY-MON-DD') AND TO_DATE('2022-Jul-16','YYYY-MON-DD')
                                      AND DPO1.DELETED_FLAG = 'N'
                                       AND DPO1.REJECT_FLAG = 'N'
                                AND DPO1.APPROVED_FLAG = 'N'
                                AND  LU.SP_CODE ='1000839' 
                                    AND DPO1.COMPANY_CODE IN ('01')
                                GROUP BY DPO1.ORDER_NO, DPO1.ORDER_DATE,BS_DATE(TO_CHAR(DPO1.ORDER_DATE)), DPO1.CUSTOMER_CODE, DPO1.BILLING_NAME, '', TRIM(IMS.ITEM_EDESC), 
                                       DPO1.MU_CODE, DPO1.QUANTITY, DPO1.UNIT_PRICE, DPO1.TOTAL_PRICE, IUS.MU_CODE, IUS.CONVERSION_FACTOR, 
                                       'D', DPO1.PARTY_TYPE_CODE,
                                       (CASE WHEN DPO1.PARTY_TYPE_CODE IS NULL
                                          THEN FN_FETCH_DESC (DPO1.COMPANY_CODE,'IP_PARTY_TYPE_CODE',CS.PARTY_TYPE_CODE)
                                          ELSE FN_FETCH_DESC (DPO1.COMPANY_CODE,'IP_PARTY_TYPE_CODE',DPO1.PARTY_TYPE_CODE)
                                        END
                                       ),
                                       DPO1.CREATED_BY, DPO1.CREATED_DATE, DPO1.DELETED_FLAG,
                                       --DPO1.COMPANY_CODE, DPO1.BRANCH_CODE,
                                       DPO1.REMARKS,
                                       DPO1.APPROVED_FLAG, DPO1.DISPATCH_FLAG, DPO1.ACKNOWLEDGE_FLAG, DPO1.REJECT_FLAG,
                                       ES.EMPLOYEE_EDESC,
                                       PS.PO_PARTY_TYPE,
                                       PS.PO_CONVERSION_UNIT,
                                       PS.PO_CONVERSION_FACTOR,
                                       PS.SO_CREDIT_LIMIT_CHK,
                                       DPO2.TOTAL_QUANTITY,
                                       DPO2.TOTAL_AMOUNT,
                                       DPO2.TOTAL_APPROVE_QTY,
                                       DPO2.TOTAL_APPROVE_AMT,
                                       LU.SP_CODE
                            union all SELECT DPO1.ORDER_NO,DPO1.ORDER_DATE,BS_DATE(TO_CHAR(DPO1.ORDER_DATE)), DPO1.CUSTOMER_CODE,DPO1.BILLING_NAME CUSTOMER_EDESC, RM.RESELLER_NAME, 'R' ORDER_ENTITY,TRIM(IMS.ITEM_EDESC) ITEM_EDESC,
                                   DPO1.MU_CODE, DPO1.QUANTITY, DPO1.UNIT_PRICE, DPO1.TOTAL_PRICE NET_TOTAL, IUS.MU_CODE CONVERSION_MU_CODE, IUS.CONVERSION_FACTOR,
                                         DPO1.PARTY_TYPE_CODE,
                                        (CASE WHEN DPO1.PARTY_TYPE_CODE IS NULL
                                          THEN FN_FETCH_DESC (DPO1.COMPANY_CODE,'IP_PARTY_TYPE_CODE',CS.PARTY_TYPE_CODE)
                                          ELSE FN_FETCH_DESC (DPO1.COMPANY_CODE,'IP_PARTY_TYPE_CODE',DPO1.PARTY_TYPE_CODE)
                                        END
                                        ) PARTY_TYPE_EDESC,
                                        DPO1.CREATED_BY, DPO1.CREATED_DATE, DPO1.DELETED_FLAG,
                                        --DPO1.COMPANY_CODE, DPO1.BRANCH_CODE,
                                        DPO1.REMARKS,
                                        DPO1.APPROVED_FLAG, DPO1.DISPATCH_FLAG, DPO1.ACKNOWLEDGE_FLAG, DPO1.REJECT_FLAG,
                                        ES.EMPLOYEE_EDESC,
                                        PS.PO_PARTY_TYPE,
                                        PS.PO_CONVERSION_UNIT,
                                        PS.PO_CONVERSION_FACTOR,
                                        PS.SO_CREDIT_LIMIT_CHK SO_CREDIT_LIMIT_FLAG,
                                        NVL(DPO2.TOTAL_QUANTITY,0) TOTAL_QUANTITY,
                                        DPO2.TOTAL_AMOUNT Grand_Total_Amount,
                                        NVL(DPO2.TOTAL_APPROVE_QTY,0) GRAND_APPROVE_QUENTITY,
                                        NVL(DPO2.TOTAL_APPROVE_AMT,0) TOTAL_APPROVE_AMT,'Reseller' EntityName
                            FROM DIST_IP_SSR_PURCHASE_ORDER DPO1
                            INNER JOIN DIST_RESELLER_MASTER RM ON RM.RESELLER_CODE = DPO1.RESELLER_CODE AND RM.IS_CLOSED = 'N'
                            INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DPO1.ITEM_CODE AND IMS.COMPANY_CODE = DPO1.COMPANY_CODE AND IMS.CATEGORY_CODE in(select CATEGORY_CODE  from IP_CATEGORY_CODE WHERE CATEGORY_TYPE IN ('FG','TF') and company_code='01') AND IMS.GROUP_SKU_FLAG = 'I'
                            LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DPO1.CUSTOMER_CODE AND CS.COMPANY_CODE = DPO1.COMPANY_CODE
                            INNER JOIN DIST_LOGIN_USER LU ON LU.USERID = DPO1.CREATED_BY AND LU.ACTIVE = 'Y'
                            INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = LU.SP_CODE AND ES.COMPANY_CODE = LU.COMPANY_CODE
                            LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON IUS.ITEM_CODE = DPO1.ITEM_CODE AND IUS.COMPANY_CODE = DPO1.COMPANY_CODE
                            INNER JOIN DIST_PREFERENCE_SETUP PS ON PS.COMPANY_CODE = DPO1.COMPANY_CODE
                            --LEFT JOIN (SELECT V.SUB_CODE, NVL((SUM (V.DR_AMOUNT) - SUM (V.CR_AMOUNT)),0) BALANCE
                            --  FROM V$VIRTUAL_SUB_LEDGER V
                            --  WHERE 1 = 1
                            --  AND V.COMPANY_CODE IN ('01')
                            --  AND V.SUB_LEDGER_FLAG = 'C'
                            -- GROUP BY V.SUB_CODE) VSL ON TRIM(VSL.SUB_CODE) = TRIM(CS.LINK_SUB_CODE)
                            INNER JOIN (SELECT POT.ORDER_NO, SUM(POT.NET_QUANTITY) TOTAL_QUANTITY, SUM(POT.NET_PRICE) TOTAL_AMOUNT, SUM(POT.APPROVE_QTY) TOTAL_APPROVE_QTY, SUM(POT.APPROVE_AMT) TOTAL_APPROVE_AMT
                                        FROM (SELECT A.ORDER_NO, A.ITEM_CODE, A.MU_CODE, A.QUANTITY, A.TOTAL_PRICE NET_PRICE, A.APPROVE_QTY, A.APPROVE_AMT, C.MU_CODE AS CONVERSION_UNIT, C.CONVERSION_FACTOR,
                                        (CASE
                                          WHEN (C.MU_CODE IS NULL AND C.CONVERSION_FACTOR IS NULL)
                                          THEN A.QUANTITY
                                          ELSE (CASE WHEN A.MU_CODE = C.MU_CODE THEN A.QUANTITY ELSE (A.QUANTITY * C.CONVERSION_FACTOR) END)
                                        END) NET_QUANTITY
                                        FROM DIST_IP_SSR_PURCHASE_ORDER A
                                        LEFT JOIN IP_ITEM_UNIT_SETUP C ON C.ITEM_CODE = A.ITEM_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                                        WHERE 1=1 
                                        --ORDER BY A.ORDER_NO DESC, A.ITEM_CODE
                            ) POT
                           GROUP BY POT.ORDER_NO) DPO2 ON DPO2.ORDER_NO = DPO1.ORDER_NO
                            WHERE 1 = 1
                              AND TRUNC(DPO1.ORDER_DATE) >= TO_DATE('2021-Jul-16','YYYY-MM-DD') AND TRUNC(DPO1.ORDER_DATE) <= TO_DATE('2022-Jul-16','YYYY-MM-DD')
                              AND DPO1.DELETED_FLAG = 'N'  
                              AND DPO1.COMPANY_CODE IN ('01')  
                              AND DPO1.CUSTOMER_CODE = '3081'                              
                            ) ORDER BY EMPLOYEE_EDESC, ITEM_EDESC, ORDER_NO
                            ";
                }
                else
                {

                     ResPOQuery = $@"SELECT  PO.EMPLOYEE_EDESC,  PO.BRAND_NAME,
                                    SUM(PO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(PO.TOTAL_AMOUNT) TOTAL_AMOUNT,PO.MU_CODE FROM (SELECT DPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME) BRAND_NAME,
                                    SUM(DPO.QUANTITY) TOTAL_QUANTITY, SUM(DPO.TOTAL_PRICE) TOTAL_AMOUNT,DPO.MU_CODE
                                   FROM DIST_IP_SSD_PURCHASE_ORDER DPO
                                   INNER JOIN DIST_LOGIN_USER DLU ON DLU.USERID = DPO.CREATED_BY AND DLU.COMPANY_CODE = DPO.COMPANY_CODE AND DLU.SP_CODE = '{SpCode}'
                                   INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DPO.COMPANY_CODE
                                   INNER JOIN DIST_DISTRIBUTOR_MASTER D ON D.DISTRIBUTOR_CODE = DPO.CUSTOMER_CODE AND D.COMPANY_CODE = DPO.COMPANY_CODE
                                   INNER JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = D.DISTRIBUTOR_CODE AND SCS.COMPANY_CODE = D.COMPANY_CODE
                                   LEFT JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = DPO.ITEM_CODE AND ISS.COMPANY_CODE = DPO.COMPANY_CODE
                                   WHERE TRUNC(DPO.ORDER_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR') AND DPO.COMPANY_CODE = '{Company}' AND
                                    TRIM(ISS.BRAND_NAME) IS NOT NULL
                                   GROUP BY DPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DPO.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME),DPO.MU_CODE
                                       UNION ALL
                                       SELECT RPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, RPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME) BRAND_NAME,
                                SUM(RPO.QUANTITY) TOTAL_QUANTITY, SUM(RPO.TOTAL_PRICE) TOTAL_AMOUNT,RPO.MU_CODE
                                   FROM DIST_IP_SSR_PURCHASE_ORDER RPO
                                   INNER JOIN DIST_LOGIN_USER DLU ON DLU.USERID = RPO.CREATED_BY AND DLU.COMPANY_CODE = RPO.COMPANY_CODE AND DLU.SP_CODE = '{SpCode}'
                                   INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = RPO.COMPANY_CODE
                                   INNER JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = RPO.RESELLER_CODE AND DRM.COMPANY_CODE = RPO.COMPANY_CODE
                                   LEFT JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = RPO.ITEM_CODE AND ISS.COMPANY_CODE = RPO.COMPANY_CODE
                                   WHERE TRUNC(RPO.ORDER_DATE) = TRUNC(TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')) AND RPO.COMPANY_CODE = '{Company}' 
                                    AND TRIM(ISS.BRAND_NAME) IS NOT NULL
                                                                       GROUP BY RPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), RPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME),RPO.MU_CODE
                                    ) PO
                                    GROUP BY PO.SP_CODE, PO.EMPLOYEE_EDESC, PO.COMPANY_CODE, PO.BRAND_NAME,PO.MU_CODE";
                    //var DisPOQuery = $@"SELECT DISTINCT DS.ORDER_NO,IM.ITEM_EDESC,CS.CUSTOMER_EDESC, DS.ORDER_DATE, DS.CUSTOMER_CODE,DS.BILLING_NAME,DS.ITEM_CODE,DS.MU_CODE,DS.QUANTITY, DS.UNIT_PRICE,DS.TOTAL_PRICE
                    //                    FROM DIST_IP_SSD_PURCHASE_ORDER DS, DIST_LOGIN_USER LU,SA_CUSTOMER_SETUP CS,IP_ITEM_MASTER_SETUP IM,DIST_PHOTO_INFO DF,DIST_LM_LOCATION_TRACKING LT
                    //                     WHERE DS.CUSTOMER_CODE = CS.CUSTOMER_CODE AND CS.COMPANY_CODE =DS.COMPANY_CODE AND DS.ITEM_CODE = IM.ITEM_CODE AND DS.COMPANY_CODE= IM.COMPANY_CODE
                    //                     AND LU.USERID=DS.CREATED_BY AND LU.COMPANY_CODE=DS.COMPANY_CODE
                    //                     AND LT.SP_CODE=LU.SP_CODE AND LT.TRACK_TYPE='EOD'
                    //                     AND LU.SP_CODE = '{SpCode}'
                    //                  AND TRUNC(DS.ORDER_DATE) >=TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy') AND TRUNC(DS.ORDER_DATE) <=TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy')";


                }
                var ResellerData = dbContext.SqlQuery(ResPOQuery);
                //   var DistrubutorData = dbContext.SqlQuery(DisPOQuery);
                
                string EodData = string.Empty;


                if(companyName.Equals("JGI Distribution Pvt. Ltd."))
                {

                    #region EODQuery
                    EodData = $@"SELECT PFMTBL.GROUP_EDESC, PFMTBL.SP_CODE, PFMTBL.EMPLOYEE_EDESC,
  TODTBL.TOD_ROUTE_CODE, TODTBL.TOD_ROUTE_NAME, TOMTBL.TOM_ROUTE_CODE, TOMTBL.TOM_ROUTE_NAME,
  PFMTBL.ATN_IMAGE, PFMTBL.ATN_DATE, PFMTBL.ATN_LATITUDE, PFMTBL.ATN_LONGITUDE, PFMTBL.EOD_DATE, PFMTBL.EOD_LATITUDE, PFMTBL.EOD_LONGITUDE,
  SUM(PFMTBL.TARGET) TARGET, SUM(PFMTBL.VISITED) VISITED, SUM(PFMTBL.NOT_VISITED) NOT_VISITED,
  SUM(PFMTBL.PJP_PRODUCTIVE) PJP_PRODUCTIVE,
  SUM(PFMTBL.VISITED) - SUM(PFMTBL.PJP_PRODUCTIVE) PJP_NON_PRODUCTIVE,
  SUM(PFMTBL.NPJP_PRODUCTIVE) NPJP_PRODUCTIVE,  
ROUND(DECODE(SUM(PFMTBL.PJP_PRODUCTIVE), NULL, 0,
       0, 0,
      (SUM(PFMTBL.PJP_PRODUCTIVE) /DECODE(SUM(PFMTBL.VISITED),0,1, SUM(PFMTBL.VISITED)) * 100),2)) PERCENT_EFFECTIVE_CALLS,
  SUM(PFMTBL.OUTLET_ADDED) OUTLET_ADDED,
  SUM(PFMTBL.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(PFMTBL.TOTAL_AMOUNT) TOTAL_AMOUNT
FROM (SELECT PTBL.*
      FROM (SELECT PETBL.GROUP_EDESC, PETBL.SP_CODE, PETBL.EMPLOYEE_EDESC, PETBL.COMPANY_CODE, PETBL.ATN_IMAGE, PETBL.ATN_DATE, PETBL.ATN_LATITUDE, PETBL.ATN_LONGITUDE, PETBL.EOD_DATE, PETBL.EOD_LATITUDE, PETBL.EOD_LONGITUDE, NVL(PETBL.TARGET, 0) TARGET, 
            NVL(PVTBL.VISITED, 0) VISITED, NVL(PNVTBL.NOT_VISITED,0) NOT_VISITED, 
            NVL(PPJPTBL.PJP_PRODUCTIVE,0) PJP_PRODUCTIVE,
            (NVL(PVTBL.VISITED, 0) - NVL(PPJPTBL.PJP_PRODUCTIVE,0)) PJP_NON_PRODUCTIVE,
            NVL(PNPJPTBL.NPJP_PRODUCTIVE,0) NPJP_PRODUCTIVE,
            NVL(DECODE(PPJPTBL.PJP_PRODUCTIVE, NULL, 0,
                                             0, 0,
                                             ROUND((PPJPTBL.PJP_PRODUCTIVE / PVTBL.VISITED) * 100, 2)),0) NET_PERCENT_EFFECTIVE_CALLS,
            0 OUTLET_ADDED,
            NVL(PPJPTBL.TOTAL_QUANTITY,0) TOTAL_QUANTITY, NVL(PPJPTBL.TOTAL_PRICE,0) TOTAL_AMOUNT
            FROM (SELECT ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, ENT.ATN_IMAGE, ENT.ATN_DATE, ENT.ATN_LATITUDE, ENT.ATN_LONGITUDE, ENT.EOD_DATE, ENT.EOD_LATITUDE, ENT.EOD_LONGITUDE, COUNT(ENT.ENTITY_CODE) TARGET
                  FROM (
                      SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, TRIM(DGM.GROUP_EDESC) GROUP_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE, API.ATN_IMAGE, ATN.ATN_DATE, ATN.ATN_LATITUDE, ATN.ATN_LONGITUDE, EOD.EOD_DATE, EOD.EOD_LATITUDE, EOD.EOD_LONGITUDE
                      FROM DIST_LOGIN_USER DLU
                      INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN DIST_GROUP_MASTER DGM ON DGM.GROUPID = DLU.GROUPID AND DGM.COMPANY_CODE = DGM.COMPANY_CODE
                      LEFT JOIN (SELECT A.ENTITY_CODE SP_CODE, A.COMPANY_CODE, A.CREATE_DATE, A.FILENAME ATN_IMAGE
                                  FROM DIST_PHOTO_INFO A
                                  WHERE A.CREATE_DATE = (SELECT MAX(CREATE_DATE) FROM DIST_PHOTO_INFO WHERE ENTITY_CODE = A.ENTITY_CODE AND ENTITY_TYPE = 'S' AND (MEDIA_TYPE = 'ATN' OR CATEGORYID = 1) AND TRUNC(CREATE_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR'))
                                  GROUP BY A.ENTITY_CODE, A.COMPANY_CODE, A.CREATE_DATE, A.FILENAME
                                  ORDER BY A.ENTITY_CODE DESC
                      ) API ON API.SP_CODE = DLU.SP_CODE AND API.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (SELECT A.EMPLOYEE_ID SP_CODE, A.ATTENDANCE_TIME ATN_DATE,''ATN_LATITUDE, '' ATN_LONGITUDE
                                  FROM HRIS_ATTENDANCE A
                                  WHERE A.ATTENDANCE_TIME = (SELECT MIN(ATTENDANCE_TIME) FROM HRIS_ATTENDANCE WHERE EMPLOYEE_ID = A.EMPLOYEE_ID AND TRUNC(ATTENDANCE_DT) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR'))
                                  GROUP BY A.EMPLOYEE_ID, A.ATTENDANCE_TIME
                                  ORDER BY A.EMPLOYEE_ID DESC
                      ) ATN ON ATN.SP_CODE = DLU.SP_CODE
                      LEFT JOIN (SELECT A.EMPLOYEE_ID SP_CODE, A.ATTENDANCE_TIME EOD_DATE, '' EOD_LATITUDE,'' EOD_LONGITUDE
                                  FROM HRIS_ATTENDANCE A
                                  WHERE A.ATTENDANCE_TIME = (SELECT MAX(ATTENDANCE_TIME) FROM HRIS_ATTENDANCE WHERE EMPLOYEE_ID = A.EMPLOYEE_ID AND TRUNC(ATTENDANCE_DT) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR'))
                                  GROUP BY A.EMPLOYEE_ID, A.ATTENDANCE_TIME
                                  ORDER BY A.EMPLOYEE_ID DESC
                      ) EOD ON EOD.SP_CODE = DLU.SP_CODE
                      LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                      LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                      LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                      GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), TRIM(DGM.GROUP_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC), DLU.COMPANY_CODE, API.ATN_IMAGE, ATN.ATN_DATE, ATN.ATN_LATITUDE, ATN.ATN_LONGITUDE, EOD.EOD_DATE, EOD.EOD_LATITUDE, EOD.EOD_LONGITUDE
                  ) ENT
                  GROUP BY ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, ENT.ATN_IMAGE, ENT.ATN_DATE, ENT.ATN_LATITUDE, ENT.ATN_LONGITUDE, ENT.EOD_DATE, ENT.EOD_LATITUDE, ENT.EOD_LONGITUDE
                  ) PETBL -- Party Type Entity Table
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) VISITED
                        FROM (
                            SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                            GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                        ) ENT
                        INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                        WHERE DLT.SP_CODE IS NOT NULL
                        GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) PVTBL -- Party Type Visit Table
                  ON PVTBL.SP_CODE = PETBL.SP_CODE AND PVTBL.COMPANY_CODE = PETBL.COMPANY_CODE
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) NOT_VISITED
                      FROM (
                          SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.PARTY_TYPE_CODE ENTITY_CODE, TRIM(SCS.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                          FROM DIST_LOGIN_USER DLU
                          INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                          LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                          LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                          LEFT JOIN IP_PARTY_TYPE_CODE SCS ON SCS.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE 
                          GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.PARTY_TYPE_CODE, TRIM(SCS.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                      ) ENT
                      LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                      WHERE DLT.SP_CODE IS NULL
                      GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) PNVTBL -- Party Type Not Visited Table
                  ON PNVTBL.SP_CODE = PETBL.SP_CODE AND PNVTBL.COMPANY_CODE = PETBL.COMPANY_CODE
            LEFT JOIN (SELECT PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE, COUNT(PPO.PARTY_TYPE_CODE) PJP_PRODUCTIVE, SUM(PPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(PPO.TOTAL_PRICE) TOTAL_PRICE
                      FROM (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                            GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                          ) PJPENT
                      LEFT JOIN (SELECT CREATED_BY, PARTY_TYPE_CODE, COMPANY_CODE, COUNT(PARTY_TYPE_CODE) TOTAL_ORDER, SUM(QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE
                                  FROM DIST_IP_SSD_PURCHASE_ORDER
                                  WHERE TRUNC(ORDER_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                                  GROUP BY CREATED_BY, PARTY_TYPE_CODE, COMPANY_CODE
                                ) PPO ON PPO.CREATED_BY = PJPENT.USERID AND PPO.PARTY_TYPE_CODE = PJPENT.ENTITY_CODE AND PPO.COMPANY_CODE = PJPENT.COMPANY_CODE
                      GROUP BY PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE
                   ) PPJPTBL -- Party Type PJP Table
                   ON PPJPTBL.SP_CODE = PETBL.SP_CODE AND PPJPTBL.COMPANY_CODE = PETBL.COMPANY_CODE
            LEFT JOIN (SELECT PPO.SP_CODE, PPO.EMPLOYEE_EDESC, PPO.COMPANY_CODE, COUNT(PPO.PARTY_TYPE_CODE) NPJP_PRODUCTIVE, SUM(PPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(PPO.TOTAL_PRICE) TOTAL_PRICE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, A.PARTY_TYPE_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.COMPANY_CODE, COUNT(A.PARTY_TYPE_CODE) TOTAL_ORDER, SUM(A.QUANTITY) TOTAL_QUANTITY, SUM(A.TOTAL_PRICE) TOTAL_PRICE
                            FROM DIST_IP_SSD_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            WHERE TRUNC(A.ORDER_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                            GROUP BY A.CREATED_BY, B.SP_CODE, A.PARTY_TYPE_CODE, TRIM(C.EMPLOYEE_EDESC), A.COMPANY_CODE
                      ) PPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                                  GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                      ) NPJPENT ON NPJPENT.USERID = PPO.CREATED_BY AND NPJPENT.SP_CODE = PPO.SP_CODE AND NPJPENT.ENTITY_CODE = PPO.PARTY_TYPE_CODE AND NPJPENT.COMPANY_CODE = PPO.COMPANY_CODE
                      WHERE 1 = 1
                        AND NPJPENT.SP_CODE IS NULL
                      GROUP BY PPO.SP_CODE, PPO.EMPLOYEE_EDESC, PPO.COMPANY_CODE
                   ) PNPJPTBL -- Party Type PJP Table
                   ON PNPJPTBL.SP_CODE = PETBL.SP_CODE AND PNPJPTBL.COMPANY_CODE = PETBL.COMPANY_CODE
      ) PTBL
      UNION ALL
      SELECT DTBL.*
      FROM (SELECT DETBL.GROUP_EDESC, DETBL.SP_CODE, DETBL.EMPLOYEE_EDESC, DETBL.COMPANY_CODE, DETBL.ATN_IMAGE, DETBL.ATN_DATE, DETBL.ATN_LATITUDE, DETBL.ATN_LONGITUDE, DETBL.EOD_DATE, DETBL.EOD_LATITUDE, DETBL.EOD_LONGITUDE, NVL(DETBL.TARGET, 0) TARGET, 
            NVL(DVTBL.VISITED, 0) VISITED, NVL(DNVTBL.NOT_VISITED,0) NOT_VISITED, 
            NVL(DPJPTBL.PJP_PRODUCTIVE,0) PJP_PRODUCTIVE,
            (NVL(DVTBL.VISITED, 0) - NVL(DPJPTBL.PJP_PRODUCTIVE,0)) PJP_NON_PRODUCTIVE,
            NVL(DNPJPTBL.NPJP_PRODUCTIVE,0) NPJP_PRODUCTIVE,
            NVL(DECODE(DPJPTBL.PJP_PRODUCTIVE, NULL, 0,
                                             0, 0,
                                             ROUND((DPJPTBL.PJP_PRODUCTIVE / DVTBL.VISITED) * 100,2)),0) NET_PERCENT_EFFECTIVE_CALLS,
            0 OUTLET_ADDED,
            NVL(DPJPTBL.TOTAL_QUANTITY,0) TOTAL_QUANTITY, NVL(DPJPTBL.TOTAL_PRICE,0) TOTAL_AMOUNT
            FROM (SELECT ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, ENT.ATN_IMAGE, ENT.ATN_DATE, ENT.ATN_LATITUDE, ENT.ATN_LONGITUDE, ENT.EOD_DATE, ENT.EOD_LATITUDE, ENT.EOD_LONGITUDE, COUNT(ENT.ENTITY_CODE) TARGET
                  FROM (
                      SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, TRIM(DGM.GROUP_EDESC) GROUP_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE, API.ATN_IMAGE, ATN.ATN_DATE, ATN.ATN_LATITUDE, ATN.ATN_LONGITUDE, EOD.EOD_DATE, EOD.EOD_LATITUDE, EOD.EOD_LONGITUDE
                      FROM DIST_LOGIN_USER DLU
                      INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN DIST_GROUP_MASTER DGM ON DGM.GROUPID = DLU.GROUPID AND DGM.COMPANY_CODE = DGM.COMPANY_CODE
                      LEFT JOIN (SELECT A.ENTITY_CODE SP_CODE, A.COMPANY_CODE, A.CREATE_DATE, A.FILENAME ATN_IMAGE
                                  FROM DIST_PHOTO_INFO A
                                  WHERE A.CREATE_DATE = (SELECT MAX(CREATE_DATE) FROM DIST_PHOTO_INFO WHERE ENTITY_CODE = A.ENTITY_CODE AND ENTITY_TYPE = 'S' AND (MEDIA_TYPE = 'ATN' OR CATEGORYID = 1) AND TRUNC(CREATE_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR'))
                                  GROUP BY A.ENTITY_CODE, A.COMPANY_CODE, A.CREATE_DATE, A.FILENAME
                                  ORDER BY A.ENTITY_CODE DESC
                      ) API ON API.SP_CODE = DLU.SP_CODE AND API.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (SELECT A.EMPLOYEE_ID SP_CODE, A.ATTENDANCE_TIME ATN_DATE,''ATN_LATITUDE, '' ATN_LONGITUDE
                                  FROM HRIS_ATTENDANCE A
                                  WHERE A.ATTENDANCE_TIME = (SELECT MIN(ATTENDANCE_TIME) FROM HRIS_ATTENDANCE WHERE EMPLOYEE_ID = A.EMPLOYEE_ID AND TRUNC(ATTENDANCE_DT) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR'))
                                  GROUP BY A.EMPLOYEE_ID, A.ATTENDANCE_TIME
                                  ORDER BY A.EMPLOYEE_ID DESC
                      ) ATN ON ATN.SP_CODE = DLU.SP_CODE
                      LEFT JOIN (SELECT A.EMPLOYEE_ID SP_CODE, A.ATTENDANCE_TIME EOD_DATE, '' EOD_LATITUDE,'' EOD_LONGITUDE
                                  FROM HRIS_ATTENDANCE A
                                  WHERE A.ATTENDANCE_TIME = (SELECT MAX(ATTENDANCE_TIME) FROM HRIS_ATTENDANCE WHERE EMPLOYEE_ID = A.EMPLOYEE_ID AND TRUNC(ATTENDANCE_DT) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR'))
                                  GROUP BY A.EMPLOYEE_ID, A.ATTENDANCE_TIME
                                  ORDER BY A.EMPLOYEE_ID DESC
                      ) EOD ON EOD.SP_CODE = DLU.SP_CODE
                      LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                      LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                      LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                      GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), TRIM(DGM.GROUP_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE, API.ATN_IMAGE, ATN.ATN_DATE, ATN.ATN_LATITUDE, ATN.ATN_LONGITUDE, EOD.EOD_DATE, EOD.EOD_LATITUDE, EOD.EOD_LONGITUDE
                  ) ENT
                  GROUP BY ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, ENT.ATN_IMAGE, ENT.ATN_DATE, ENT.ATN_LATITUDE, ENT.ATN_LONGITUDE, ENT.EOD_DATE, ENT.EOD_LATITUDE, ENT.EOD_LONGITUDE
                  ) DETBL -- Customer/Distributor Entity Table
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) VISITED
                        FROM (
                            SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                            GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                        ) ENT
                        INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                        WHERE DLT.SP_CODE IS NOT NULL
                        GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) DVTBL -- Customer/Distributor Visit Table
                  ON DVTBL.SP_CODE = DETBL.SP_CODE AND DVTBL.COMPANY_CODE = DETBL.COMPANY_CODE
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) NOT_VISITED
                      FROM (
                          SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                          FROM DIST_LOGIN_USER DLU
                          INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                          LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                          LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                          LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE 
                          GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                      ) ENT
                      LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                      WHERE DLT.SP_CODE IS NULL
                      GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                ) DNVTBL -- Customer/Distributor Not Visited Table
                ON DNVTBL.SP_CODE = DETBL.SP_CODE AND DNVTBL.COMPANY_CODE = DETBL.COMPANY_CODE
            LEFT JOIN (SELECT PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE, COUNT(DPO.CUSTOMER_CODE) PJP_PRODUCTIVE, SUM(DPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(DPO.TOTAL_PRICE) TOTAL_PRICE
                      FROM (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                            GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                      ) PJPENT
                      LEFT JOIN (SELECT CREATED_BY, CUSTOMER_CODE, COMPANY_CODE, COUNT(CUSTOMER_CODE) TOTAL_ORDER, SUM(QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE
                                  FROM DIST_IP_SSD_PURCHASE_ORDER
                                  WHERE TRUNC(ORDER_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                                  GROUP BY CREATED_BY, CUSTOMER_CODE, COMPANY_CODE
                      ) DPO ON DPO.CREATED_BY = PJPENT.USERID AND DPO.CUSTOMER_CODE = PJPENT.ENTITY_CODE AND DPO.COMPANY_CODE = PJPENT.COMPANY_CODE
                      GROUP BY PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE
                   ) DPJPTBL -- Customer/Distributor PJP Table
                   ON DPJPTBL.SP_CODE = DETBL.SP_CODE AND DPJPTBL.COMPANY_CODE = DETBL.COMPANY_CODE
            LEFT JOIN (SELECT DPO.SP_CODE, DPO.EMPLOYEE_EDESC, DPO.COMPANY_CODE, COUNT(DPO.CUSTOMER_CODE) NPJP_PRODUCTIVE, SUM(DPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(DPO.TOTAL_PRICE) TOTAL_PRICE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, A.CUSTOMER_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.COMPANY_CODE, COUNT(A.CUSTOMER_CODE) TOTAL_ORDER, SUM(A.QUANTITY) TOTAL_QUANTITY, SUM(A.TOTAL_PRICE) TOTAL_PRICE
                            FROM DIST_IP_SSD_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            WHERE TRUNC(A.ORDER_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                            GROUP BY A.CREATED_BY, B.SP_CODE, A.CUSTOMER_CODE, TRIM(C.EMPLOYEE_EDESC), A.COMPANY_CODE
                      ) DPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                                  GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                      ) NPJPENT ON NPJPENT.USERID = DPO.CREATED_BY AND NPJPENT.SP_CODE = DPO.SP_CODE AND NPJPENT.ENTITY_CODE = DPO.CUSTOMER_CODE AND NPJPENT.COMPANY_CODE = DPO.COMPANY_CODE
                      WHERE 1 = 1
                        AND NPJPENT.SP_CODE IS NULL
                      GROUP BY DPO.SP_CODE, DPO.EMPLOYEE_EDESC, DPO.COMPANY_CODE
                   ) DNPJPTBL -- Customer/Distributor NPJP Table
                   ON DNPJPTBL.SP_CODE = DETBL.SP_CODE AND DNPJPTBL.COMPANY_CODE = DETBL.COMPANY_CODE
      ) DTBL
      UNION ALL
      SELECT RTBL.*
      FROM (SELECT RETBL.GROUP_EDESC, RETBL.SP_CODE, RETBL.EMPLOYEE_EDESC, RETBL.COMPANY_CODE, RETBL.ATN_IMAGE, RETBL.ATN_DATE, RETBL.ATN_LATITUDE, RETBL.ATN_LONGITUDE, RETBL.EOD_DATE, RETBL.EOD_LATITUDE, RETBL.EOD_LONGITUDE, NVL(RETBL.TARGET, 0) TARGET, 
            NVL(RVTBL.VISITED, 0) VISITED, NVL(RNVTBL.NOT_VISITED,0) NOT_VISITED, 
            NVL(RPJPTBL.PJP_PRODUCTIVE,0) PJP_PRODUCTIVE,
            (NVL(RVTBL.VISITED, 0) - NVL(RPJPTBL.PJP_PRODUCTIVE,0)) PJP_NON_PRODUCTIVE,
            NVL(RNPJPTBL.NPJP_PRODUCTIVE,0) NPJP_PRODUCTIVE,
            NVL(DECODE(RPJPTBL.PJP_PRODUCTIVE, NULL, 0,
                                             0, 0,
                                             ROUND((RPJPTBL.PJP_PRODUCTIVE / RVTBL.VISITED) * 100,2)),0) NET_PERCENT_EFFECTIVE_CALLS,
            NVL(ROUT.OUTLET_ADDED, 0) OUTLET_ADDED,
            NVL(RPJPTBL.TOTAL_QUANTITY,0) TOTAL_QUANTITY, NVL(RPJPTBL.TOTAL_PRICE,0) TOTAL_AMOUNT
            FROM (SELECT ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, ENT.ATN_IMAGE, ENT.ATN_DATE, ENT.ATN_LATITUDE, ENT.ATN_LONGITUDE, ENT.EOD_DATE, ENT.EOD_LATITUDE, ENT.EOD_LONGITUDE, COUNT(ENT.ENTITY_CODE) TARGET
                  FROM (
                      SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, TRIM(DGM.GROUP_EDESC) GROUP_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE, API.ATN_IMAGE, ATN.ATN_DATE, ATN.ATN_LATITUDE, ATN.ATN_LONGITUDE, EOD.EOD_DATE, EOD.EOD_LATITUDE, EOD.EOD_LONGITUDE
                      FROM DIST_LOGIN_USER DLU
                      INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN DIST_GROUP_MASTER DGM ON DGM.GROUPID = DLU.GROUPID AND DGM.COMPANY_CODE = DGM.COMPANY_CODE
                      LEFT JOIN (SELECT A.ENTITY_CODE SP_CODE, A.COMPANY_CODE, A.CREATE_DATE, A.FILENAME ATN_IMAGE
                                  FROM DIST_PHOTO_INFO A
                                  WHERE A.CREATE_DATE = (SELECT MAX(CREATE_DATE) FROM DIST_PHOTO_INFO WHERE ENTITY_CODE = A.ENTITY_CODE AND ENTITY_TYPE = 'S' AND (MEDIA_TYPE = 'ATN' OR CATEGORYID = 1) AND TRUNC(CREATE_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR'))
                                  GROUP BY A.ENTITY_CODE, A.COMPANY_CODE, A.CREATE_DATE, A.FILENAME
                                  ORDER BY A.ENTITY_CODE DESC
                      ) API ON API.SP_CODE = DLU.SP_CODE AND API.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (SELECT A.EMPLOYEE_ID SP_CODE, A.ATTENDANCE_TIME ATN_DATE,''ATN_LATITUDE, '' ATN_LONGITUDE
                                  FROM HRIS_ATTENDANCE A
                                  WHERE A.ATTENDANCE_TIME = (SELECT MIN(ATTENDANCE_TIME) FROM HRIS_ATTENDANCE WHERE EMPLOYEE_ID = A.EMPLOYEE_ID AND TRUNC(ATTENDANCE_DT) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR'))
                                  GROUP BY A.EMPLOYEE_ID, A.ATTENDANCE_TIME
                                  ORDER BY A.EMPLOYEE_ID DESC
                      ) ATN ON ATN.SP_CODE = DLU.SP_CODE
                      LEFT JOIN (SELECT A.EMPLOYEE_ID SP_CODE, A.ATTENDANCE_TIME EOD_DATE, '' EOD_LATITUDE,'' EOD_LONGITUDE
                                  FROM HRIS_ATTENDANCE A
                                  WHERE A.ATTENDANCE_TIME = (SELECT MAX(ATTENDANCE_TIME) FROM HRIS_ATTENDANCE WHERE EMPLOYEE_ID = A.EMPLOYEE_ID AND TRUNC(ATTENDANCE_DT) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR'))
                                  GROUP BY A.EMPLOYEE_ID, A.ATTENDANCE_TIME
                                  ORDER BY A.EMPLOYEE_ID DESC
                      ) EOD ON EOD.SP_CODE = DLU.SP_CODE
                      LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                      LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                      LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y'
                      GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), TRIM(DGM.GROUP_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE, API.ATN_IMAGE, ATN.ATN_DATE, ATN.ATN_LATITUDE, ATN.ATN_LONGITUDE, EOD.EOD_DATE, EOD.EOD_LATITUDE, EOD.EOD_LONGITUDE
                  ) ENT
                  GROUP BY ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, ENT.ATN_IMAGE, ENT.ATN_DATE, ENT.ATN_LATITUDE, ENT.ATN_LONGITUDE, ENT.EOD_DATE, ENT.EOD_LATITUDE, ENT.EOD_LONGITUDE
                  ) RETBL -- Retailer Entity Table
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) VISITED
                        FROM (
                            SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y'
                            GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                        ) ENT
                        INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                        WHERE DLT.SP_CODE IS NOT NULL
                        GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) RVTBL -- Retailer Visit Table
                  ON RVTBL.SP_CODE = RETBL.SP_CODE AND RVTBL.COMPANY_CODE = RETBL.COMPANY_CODE
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) NOT_VISITED
                      FROM (
                          SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                          FROM DIST_LOGIN_USER DLU
                          INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                          LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                          LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                          LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y'
                          GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                      ) ENT
                      LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                      WHERE DLT.SP_CODE IS NULL
                      GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                ) RNVTBL -- Retailer Not Visited Table
                ON RNVTBL.SP_CODE = RETBL.SP_CODE AND RNVTBL.COMPANY_CODE = RETBL.COMPANY_CODE
            LEFT JOIN (SELECT PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE, COUNT(RPO.RESELLER_CODE) PJP_PRODUCTIVE, SUM(RPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(RPO.TOTAL_PRICE) TOTAL_PRICE
                      FROM (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y'
                            GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                      ) PJPENT
                      LEFT JOIN (SELECT CREATED_BY, RESELLER_CODE, COMPANY_CODE, COUNT(RESELLER_CODE) TOTAL_ORDER, SUM(QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE
                                  FROM DIST_IP_SSR_PURCHASE_ORDER
                                  WHERE TRUNC(ORDER_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                                  GROUP BY CREATED_BY, RESELLER_CODE, COMPANY_CODE
                      ) RPO ON RPO.CREATED_BY = PJPENT.USERID AND RPO.RESELLER_CODE = PJPENT.ENTITY_CODE AND RPO.COMPANY_CODE = PJPENT.COMPANY_CODE
                      GROUP BY PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE
                   ) RPJPTBL -- Retailer PJP Table
                   ON RPJPTBL.SP_CODE = RETBL.SP_CODE AND RPJPTBL.COMPANY_CODE = RETBL.COMPANY_CODE
            LEFT JOIN (SELECT RPO.SP_CODE, RPO.EMPLOYEE_EDESC, RPO.COMPANY_CODE, COUNT(RPO.RESELLER_CODE) NPJP_PRODUCTIVE, SUM(RPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(RPO.TOTAL_PRICE) TOTAL_PRICE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, A.RESELLER_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.COMPANY_CODE, COUNT(A.RESELLER_CODE) TOTAL_ORDER, SUM(A.QUANTITY) TOTAL_QUANTITY, SUM(A.TOTAL_PRICE) TOTAL_PRICE
                            FROM DIST_IP_SSR_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            WHERE TRUNC(A.ORDER_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                            GROUP BY A.CREATED_BY, B.SP_CODE, A.RESELLER_CODE, TRIM(C.EMPLOYEE_EDESC), A.COMPANY_CODE
                      ) RPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y'
                                  GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                      ) NPJPENT ON NPJPENT.USERID = RPO.CREATED_BY AND NPJPENT.SP_CODE = RPO.SP_CODE AND NPJPENT.ENTITY_CODE = RPO.RESELLER_CODE AND NPJPENT.COMPANY_CODE = RPO.COMPANY_CODE
                      WHERE 1 = 1
                        AND NPJPENT.SP_CODE IS NULL
                      GROUP BY RPO.SP_CODE, RPO.EMPLOYEE_EDESC, RPO.COMPANY_CODE
                   ) RNPJPTBL -- Retailer NPJP Table
                   ON RNPJPTBL.SP_CODE = RETBL.SP_CODE AND RNPJPTBL.COMPANY_CODE = RETBL.COMPANY_CODE
            LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, DLU.COMPANY_CODE, COUNT(DRM.RESELLER_CODE) OUTLET_ADDED
                        FROM DIST_RESELLER_MASTER DRM 
                        INNER JOIN DIST_LOGIN_USER DLU ON DLU.USERID = DRM.CREATED_BY AND DLU.COMPANY_CODE = DRM.COMPANY_CODE
                        WHERE 1 = 1
                          AND DRM.CREATED_DATE = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                        GROUP BY DLU.USERID, DLU.SP_CODE, DLU.COMPANY_CODE
            ) ROUT -- New Retailer/Outlets
            ON ROUT.SP_CODE = RETBL.SP_CODE AND ROUT.COMPANY_CODE = RETBL.COMPANY_CODE
      ) RTBL
) PFMTBL
LEFT JOIN (SELECT TOD.SP_CODE, LISTAGG(TOD.ROUTE_CODE, ', ') WITHIN GROUP (ORDER BY TOD.ROUTE_CODE) TOD_ROUTE_CODE, LISTAGG(TOD.ROUTE_NAME, ', ') WITHIN GROUP (ORDER BY TOD.ROUTE_NAME) TOD_ROUTE_NAME
            FROM (SELECT DRD.EMP_CODE SP_CODE, DRM.ROUTE_CODE, TRIM(DRM.ROUTE_NAME) ROUTE_NAME, DRM.COMPANY_CODE
                  FROM DIST_ROUTE_DETAIL DRD
                  INNER JOIN DIST_ROUTE_MASTER DRM ON DRM.ROUTE_CODE = DRD.ROUTE_CODE AND DRM.COMPANY_CODE = DRD.COMPANY_CODE
                  WHERE 1 = 1
                    AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR') 
                    AND DRD.COMPANY_CODE = '{Company}'
                  GROUP BY DRD.EMP_CODE, DRM.ROUTE_CODE, DRM.ROUTE_NAME, DRM.COMPANY_CODE
                  ORDER BY UPPER(TRIM(DRM.ROUTE_NAME))
            ) TOD
            GROUP BY TOD.SP_CODE
) TODTBL ON TODTBL.SP_CODE = PFMTBL.SP_CODE
LEFT JOIN (SELECT TOM.SP_CODE, LISTAGG(TOM.ROUTE_CODE, ', ') WITHIN GROUP (ORDER BY TOM.ROUTE_CODE) TOM_ROUTE_CODE, LISTAGG(TOM.ROUTE_NAME, ', ') WITHIN GROUP (ORDER BY TOM.ROUTE_NAME) TOM_ROUTE_NAME
            FROM (SELECT DRD.EMP_CODE SP_CODE, DRM.ROUTE_CODE, TRIM(DRM.ROUTE_NAME) ROUTE_NAME, DRM.COMPANY_CODE
                  FROM DIST_ROUTE_DETAIL DRD
                  INNER JOIN DIST_ROUTE_MASTER DRM ON DRM.ROUTE_CODE = DRD.ROUTE_CODE AND DRM.COMPANY_CODE = DRD.COMPANY_CODE
                  WHERE 1 = 1
                    AND TRUNC(DRD.ASSIGN_DATE) = (TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR') + 1)
                    AND DRD.COMPANY_CODE = '{Company}'
                  GROUP BY DRD.EMP_CODE, DRM.ROUTE_CODE, DRM.ROUTE_NAME, DRM.COMPANY_CODE
                  ORDER BY UPPER(TRIM(DRM.ROUTE_NAME))
            ) TOM
            GROUP BY TOM.SP_CODE
) TOMTBL ON TOMTBL.SP_CODE = PFMTBL.SP_CODE
WHERE 1 = 1
  AND PFMTBL.COMPANY_CODE = '{Company}'  
GROUP BY PFMTBL.GROUP_EDESC, PFMTBL.SP_CODE, PFMTBL.EMPLOYEE_EDESC, 
  TODTBL.TOD_ROUTE_CODE, TODTBL.TOD_ROUTE_NAME, TOMTBL.TOM_ROUTE_CODE, TOMTBL.TOM_ROUTE_NAME,
  PFMTBL.ATN_IMAGE, PFMTBL.ATN_DATE, PFMTBL.ATN_LATITUDE, PFMTBL.ATN_LONGITUDE, PFMTBL.EOD_DATE, PFMTBL.EOD_LATITUDE, PFMTBL.EOD_LONGITUDE
ORDER BY UPPER(PFMTBL.GROUP_EDESC), UPPER(PFMTBL.EMPLOYEE_EDESC)";

                    //                var totalNPJPQuery = $@"SELECT
                    //NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE SP_CODE = '{SpCode}' AND COMPANY_CODE = '{Company}' AND TRUNC(UPDATE_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR') ),0)
                    //-
                    //NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE SP_CODE = '{SpCode}' AND COMPANY_CODE = '{Company}' AND TRUNC(ORDER_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR') 
                    //        AND RESELLER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  SP_CODE = '{SpCode}' AND COMPANY_CODE = '{Company}' AND TRUNC(ASSIGN_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR') )),0)
                    //TOTAL_NPJP
                    //FROM DUAL";

                    #endregion EODQuery

                }
                else
                {
                    var totalVis = $@"-- VIIST, PLAN AND NEW OUTLET COUNT
SELECT DRD.EMP_CODE
	,DRD.ASSIGN_DATE
	,DRD.COMPANY_CODE
	,(
		SELECT COUNT(ROUTE_CODE)
		FROM DIST_ROUTE_ENTITY
		WHERE ROUTE_CODE = DRD.ROUTE_CODE
		)  TARGET
	,(
		SELECT COUNT(*)
		FROM DIST_LOCATION_TRACK
		WHERE SP_CODE = DRD.EMP_CODE
			AND IS_VISITED = 'Y'
			AND TRUNC(UPDATE_DATE) = DRD.ASSIGN_DATE
		) VISITED
	,(
		SELECT COUNT(*)
		FROM DIST_RESELLER_MASTER DRM
			,DIST_LOGIN_USER DLU
		WHERE DRM.CREATED_BY = DLU.USERID
			AND DLU.SP_CODE = DRD.EMP_CODE
			AND TRUNC(DRM.CREATED_DATE) = DRD.ASSIGN_DATE
		) OUTLET_ADDED
FROM DIST_ROUTE_DETAIL DRD
WHERE COMPANY_CODE = '01'";

                    var eodInOut = $@"--ATTENDANCE IN AND OUT TIME IN REFERENCE WITH ALL TRACK_TYPE, EMP CODE IS TAKEN REFERENCE FROM ABOVE QUERY RESULT
SELECT MIN(LLT.SUBMIT_DATE) ATN_DATE
	,MAX(LLT.SUBMIT_DATE) EOD_DATE
FROM DIST_LM_LOCATION_TRACKING LLT
WHERE TRUNC(LLT.SUBMIT_DATE) = sysdate
	AND LLT.SP_CODE = '10001'";



                    var toDRout = $@"SELECT DRM.ROUTE_CODE, DRM.ROUTE_NAME TOD_ROUTE_NAME
  FROM DIST_ROUTE_DETAIL DRD, DIST_ROUTE_MASTER DRM
  WHERE DRD.ROUTE_CODE = DRM.ROUTE_CODE
  AND DRD.COMPANY_CODE = DRM.COMPANY_CODE
  AND TRUNC(DRD.ASSIGN_DATE) = '16-JUN-20'
  AND DRD.EMP_CODE = '1001'";

                    var tomRout = $@"  --TOMORROW ROUTE, DATE AND EMP CODE IS TAKEN FROM ABOVE
SELECT DRM.ROUTE_CODE
	,DRM.ROUTE_NAME TOM_ROUTE_NAME
FROM DIST_ROUTE_DETAIL DRD
	,DIST_ROUTE_MASTER DRM
WHERE DRD.ROUTE_CODE = DRM.ROUTE_CODE
	AND DRD.COMPANY_CODE = DRM.COMPANY_CODE
	AND DRD.ASSIGN_DATE = sysdate
	AND DRD.EMP_CODE = '1001'";


                    var eodTime = $@" -- IF DATA, EOD IS DONE ELSE NOT DONE, REPLACE OUT TIME BY EOD TIME IF DATA EXISTS, EMP CODE IS TAKEN REFERENCE FROM ABOVE QUERY RESULT
SELECT MAX(SUBMIT_DATE) EOD_TIME
FROM DIST_LM_LOCATION_TRACKING
WHERE TRUNC(SUBMIT_DATE) = sysdate
	AND SP_CODE = '1001'
	AND TRACK_TYPE = 'EOD'";


                    var order = $@"-- EMP CODE AND DATE IS TAKEN REFERENCE FROM ABOVE QUERY RESULT 
SELECT BRAND_NAME
	,MU_CODE
	,SUM(QTY) QTY
	,SUM(TOTAL_AMT) AMT
FROM (
	SELECT ODATA.*
	FROM (
		SELECT NVL(ISS.BRAND_NAME, 'NA') BRAND_NAME
			,DPO.CREATED_BY
			,TRUNC(ORDER_DATE) ORDER_DATE
			,DPO.MU_CODE
			,SUM(QUANTITY) QTY
			,SUM(TOTAL_PRICE) TOTAL_AMT
		FROM DIST_IP_SSD_PURCHASE_ORDER DPO
			,IP_ITEM_SPEC_SETUP ISS
		WHERE 1 = 1
			AND DPO.ITEM_CODE = ISS.ITEM_CODE
			AND DPO.COMPANY_CODE = ISS.COMPANY_CODE
		GROUP BY DPO.MU_CODE
			,NVL(ISS.BRAND_NAME, 'NA')
			,DPO.CREATED_BY
			,TRUNC(ORDER_DATE)
		
		UNION ALL
		
		SELECT NVL(ISS.BRAND_NAME, 'NA') BRAND_NAME
			,DPO.CREATED_BY
			,TRUNC(ORDER_DATE) ORDER_DATE
			,DPO.MU_CODE
			,SUM(QUANTITY) QTY
			,SUM(TOTAL_PRICE) TOTAL_AMT
		FROM DIST_IP_SSR_PURCHASE_ORDER DPO
			,IP_ITEM_SPEC_SETUP ISS
		WHERE 1 = 1
			AND DPO.ITEM_CODE = ISS.ITEM_CODE
			AND DPO.COMPANY_CODE = ISS.COMPANY_CODE
		GROUP BY DPO.MU_CODE
			,NVL(ISS.BRAND_NAME, 'NA')
			,DPO.CREATED_BY
			,TRUNC(ORDER_DATE)
		) ODATA
		,DIST_LOGIN_USER DLU
	WHERE ODATA.CREATED_BY = DLU.USERID(+)
		AND DLU.SP_CODE = '10001'
		AND ODATA.ORDER_DATE = NVL(sysdate, TRUNC(SYSDATE))
	)
GROUP BY BRAND_NAME
	,MU_CODE";

                    var collec = $@"  --Collection report
-- EMP CODE AND DATE IS TAKEN REFERENCE FROM ABOVE QUERY RESULT 
SELECT PAYMENT_MODE
	,(
		CASE 
			WHEN ENTITY_TYPE = 'D'
				THEN 'DISTRIBUTOR'
			WHEN ENTITY_TYPE = 'R'
				THEN 'RESELLER'
			END
		) COLLECTED_FROM
	,SUM(AMOUNT)
FROM DIST_COLLECTION
WHERE SP_CODE = '10001'
	AND TRUNC(CREATED_DATE) = sysdate
GROUP BY PAYMENT_MODE
	,(
		CASE 
			WHEN ENTITY_TYPE = 'D'
				THEN 'DISTRIBUTOR'
			WHEN ENTITY_TYPE = 'R'
				THEN 'RESELLER'
			END
		)";


                    var sReturn = $@"--SALES RETURN 
--EMP CODE AND DATE IS TAKEN REFERENCE FROM ABOVE QUERY RESULT 
SELECT NVL(ISS.BRAND_NAME, 'NA') BRAND_NAME
	,DPO.MU_CODE
	,SUM(QUANTITY) QTY
    ,SUM(TOTAL_PRICE) TOTAL_AMT
FROM DIST_SALES_RETURN DPO
	,IP_ITEM_SPEC_SETUP ISS
	,DIST_LOGIN_USER DLU
WHERE 1 = 1
	AND DPO.ITEM_CODE = ISS.ITEM_CODE
	AND DPO.COMPANY_CODE = ISS.COMPANY_CODE
	AND DPO.CREATED_BY = DLU.USERID(+)
	AND DPO.COMPANY_CODE = DLU.COMPANY_CODE(+)
	AND DLU.SP_CODE = '1001'
	AND TRUNC(DPO.CREATED_DATE) = NVL(sysdate, TRUNC(SYSDATE))
GROUP BY DPO.MU_CODE
	,NVL(ISS.BRAND_NAME, 'NA')
	,DPO.MU_CODE";

                    EodData = $@"SELECT DRD.EMP_CODE
	,DRD.ASSIGN_DATE
	,DRD.COMPANY_CODE
  
	,(
		SELECT COUNT(ROUTE_CODE)
		FROM DIST_ROUTE_ENTITY
		WHERE ROUTE_CODE = DRD.ROUTE_CODE
		)  TARGET
    
	,(
		SELECT COUNT(*)
		FROM DIST_LOCATION_TRACK
		WHERE SP_CODE = DRD.EMP_CODE
			AND IS_VISITED = 'Y'
			AND TRUNC(UPDATE_DATE) = DRD.ASSIGN_DATE
		) VISITED
      ,(SELECT COUNT(*) FROM DIST_LOCATION_TRACK WHERE SP_CODE = DRD.EMP_CODE AND IS_VISITED = 'N'
			AND TRUNC(UPDATE_DATE) = DRD.ASSIGN_DATE ) NOT_VISITED
    
	,(
		SELECT COUNT(*)
		FROM DIST_RESELLER_MASTER DRM
			,DIST_LOGIN_USER DLU
		WHERE DRM.CREATED_BY = DLU.USERID
			AND DLU.SP_CODE = DRD.EMP_CODE
			AND TRUNC(DRM.CREATED_DATE) = DRD.ASSIGN_DATE
		) OUTLET_ADDED
    
    ,
    (
      SELECT MIN(LLT.SUBMIT_DATE) ATN_DATE
	--,MAX(LLT.SUBMIT_DATE) EOD_DATE
FROM DIST_LM_LOCATION_TRACKING LLT
WHERE TRUNC(LLT.SUBMIT_DATE) = TRUNC(TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR'))
	AND LLT.SP_CODE = '{SpCode}'
  ) ATN_DATE
  
  ,(
  SELECT 
	  MAX(LLT.SUBMIT_DATE) EOD_DATE
FROM DIST_LM_LOCATION_TRACKING LLT
WHERE TRUNC(LLT.SUBMIT_DATE) = TRUNC(TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR'))
	AND LLT.SP_CODE = '{SpCode}'
  ) EOD_DATE
  
  ,(
  SELECT DRM.ROUTE_NAME TOD_ROUTE_NAME
  FROM DIST_ROUTE_DETAIL DRD, DIST_ROUTE_MASTER DRM
  WHERE DRD.ROUTE_CODE = DRM.ROUTE_CODE
  AND DRD.COMPANY_CODE = DRM.COMPANY_CODE
  AND TRUNC(DRD.ASSIGN_DATE) = TRUNC(TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR'))
  AND DRD.EMP_CODE = '{SpCode}'
  ) TOD_ROUTE_NAME
  
  ,(
    SELECT 
	DRM.ROUTE_NAME TOM_ROUTE_NAME
FROM DIST_ROUTE_DETAIL DRD
	,DIST_ROUTE_MASTER DRM
WHERE DRD.ROUTE_CODE = DRM.ROUTE_CODE
	AND DRD.COMPANY_CODE = DRM.COMPANY_CODE
	AND DRD.ASSIGN_DATE = TRUNC(TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR'))
	AND DRD.EMP_CODE = '{SpCode}'
  ) TOM_ROUTE_NAME
FROM DIST_ROUTE_DETAIL DRD
WHERE COMPANY_CODE = '{Company}'";

                    var data1 = dbContext.SqlQuery<EODUpdate>(EodData).FirstOrDefault();
                    var orderData = dbContext.SqlQuery(order);
                    var returnData = dbContext.SqlQuery(sReturn);
                    var message1 = $@"<b>PJP Call</b><br>Today's:{data1.TOD_ROUTE_NAME}<br>Target Calls:{data1.TARGET}<br> Actual Calls (TC): {data1.VISITED}
                                 <br>Productive Calls (PC): {data1.PJP_PRODUCTIVE}<br>Total NPJP (NPJP): N/A<br>NPJP Productive Calls (NPC): {data1.NPJP_PRODUCTIVE}
                                 <br>Added Outlet (AO): {data1.OUTLET_ADDED}
                                 <br>Total Not Visited:{data1.NOT_VISITED}<br><br><b>Attendance Time: {data1.ATN_DATE}</b><br>EOD Time:{data1.EOD_DATE}
                                 <br>Tomorrow Route: {data1.TOM_ROUTE_NAME} <br> Remarks: {model[0].Remarks}";
                    message1 += @"<br><br><b>Today's order details</b><br><table border='2'><tr><th>Brand</th><th>Quantity</td><th>Amount</th><th>Unit</th></tr>";
                    //foreach (DataRow row in ResellerData.Rows)
                    //{
                    //    message1 += $@"<tr><td>{row["BRAND_NAME"]}</td><td>{row["TOTAL_QUANTITY"]}</td><td>{row["TOTAL_AMOUNT"]}</td><td>{row["MU_CODE"]}</td></tr>";
                    //}
                    if(orderData != null)
                    {
                        foreach (DataRow row in orderData.Rows)
                        {
                            message1 += $@"<tr><td>{row["BRAND_NAME"]}</td><td>{row["QTY"]}</td><td>{row["AMT"]}</td><td>{row["MU_CODE"]}</td></tr>";
                        }
                    }

                    if (returnData != null)
                    {
                        foreach (DataRow row in returnData.Rows)
                        {
                            message1 += $@"<tr><td>{row["BRAND_NAME"]}</td><td>{row["QTY"]}</td><td>{row["TOTAL_AMT"]}</td><td>{row["MU_CODE"]}</td></tr>";
                        }
                    }
                    message1 += "</table>";
                    mailModel.MESSAGE = message1;
                    System.Net.Mail.Attachment ResellerAttach1;
                    if (companyName.Equals("Bhudeo Khadya Udyog P. Ltd."))
                    {
                        var SalesPersonData = dbContext.SqlQuery(SalesPersonQuery);
                         ResellerAttach1 = new System.Net.Mail.Attachment(CommonHelper.ConvertTableIntoExcel(ResellerData, SalesPersonData), string.Format("{0}.{1}", "Purchase Orders", "xls"));

                    }
                    else
                    {
                         ResellerAttach1 = new System.Net.Mail.Attachment(ResellerData.DataToExcel(), string.Format("{0}.{1}", "Purchase Orders", "xls"));

                    }
                    // var DistrubutorAttach = new System.Net.Mail.Attachment(DistrubutorData.DataToExcel(), string.Format("{0}.{1}", "Distributor Purchase Orders", "xls"));
                    System.Net.Mail.Attachment[] file1 = new System.Net.Mail.Attachment[] {ResellerAttach1 };


                    //mailModel.ATTACHMENT_FILE = file;
                    var emailSuccess1 = MailHelper.SendMailDirectAttach(string.Empty, mailModel.SUBJECT, mailModel.MESSAGE, mailModel.EMAIL_TO, mailModel.EMAIL_CC, mailModel.EMAIL_BCC, "", file1);
                    return true;
                }
                var totalNPJPQuery = $@"SELECT COUNT(DISTINCT CUSTOMER_CODE) TOTAL_NPJP  FROM DIST_VISITED_ENTITY WHERE SP_CODE='{SpCode}' AND COMPANY_CODE='{Company}' AND TRUNC(UPDATE_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR')
                                         AND CUSTOMER_CODE NOT IN (SELECT coalesce(ENTITY_CODE,'0') FROM DIST_TARGET_ENTITY WHERE  SP_CODE = '{SpCode}' AND COMPANY_CODE = '{Company}' AND TRUNC(ASSIGN_DATE) = TO_DATE('{DateTime.Now.ToString("dd-MMM-yyyy")}', 'DD-MON-RRRR') )";

                var data = dbContext.SqlQuery<EODUpdate>(EodData).Where(x => x.SP_CODE.Trim() == SpCode.Trim()).FirstOrDefault();
                int? total_npjp = dbContext.SqlQuery<int>(totalNPJPQuery).FirstOrDefault();
                string message = string.Empty;
                if (data != null)
                    message = $@"<b>PJP Call</b><br>Today's:{data.TOD_ROUTE_NAME}<br>Target Calls:{data.TARGET}<br> Actual Calls (TC): {data.VISITED}
                                 <br>Productive Calls (PC): {data.PJP_PRODUCTIVE}<br>Total NPJP (NPJP): {total_npjp}<br>NPJP Productive Calls (NPC): {data.NPJP_PRODUCTIVE}
                                 <br>Added Outlet (AO): {data.OUTLET_ADDED}
                                 <br>Total Not Visited:{data.NOT_VISITED}<br><br><b>Attendance Time: {data.ATN_DATE}</b><br>EOD Time:{data.EOD_DATE}
                                 <br>Tomorrow Route: {data.TOM_ROUTE_NAME} <br> Remarks: {model[0].Remarks}";

                else
                    message = $@"<b>PJP Call</b><br>Today's: ---<br>Target Calls: 0<br>Actual Calls (TC): 0
                                 <br>Productive Calls (PC): 0<br>Total NPJP (NPJP): 0<br>NPJP Productive Calls (NPC):0<br>Addition Outlet (AO): 0
                                 <br>Total Not Visisted: 0<br><br><b>Attendance Time: ---</b><br>EOD Time: ---
                                 <br>Tomorrow Route: ---<br> Remarks: { model[0].Remarks}";
                message += @"<br><br><b>Today's order details</b><br><table border='2'><tr><th>Brand</th><th>Quantity</td><th>Amount</th><th>Unit</th></tr>";
                foreach (DataRow row in ResellerData.Rows)
                {
                    message += $@"<tr><td>{row["BRAND_NAME"]}</td><td>{row["TOTAL_QUANTITY"]}</td><td>{row["TOTAL_AMOUNT"]}</td><td>{row["MU_CODE"]}</td></tr>";
                }
                message += "</table>";
                mailModel.MESSAGE = message;

                //mailModel.MESSAGE="K xa halkhabar dai meeting kasto hudai xa lol hahhahaha";

                var ResellerAttach = new System.Net.Mail.Attachment(ResellerData.DataToExcel(), string.Format("{0}.{1}", "Purchase Orders", "xls"));
                // var DistrubutorAttach = new System.Net.Mail.Attachment(DistrubutorData.DataToExcel(), string.Format("{0}.{1}", "Distributor Purchase Orders", "xls"));
                System.Net.Mail.Attachment[] file = new System.Net.Mail.Attachment[] { ResellerAttach };


                //mailModel.ATTACHMENT_FILE = file;
                var emailSuccess = MailHelper.SendMailDirectAttach(string.Empty, mailModel.SUBJECT, mailModel.MESSAGE, mailModel.EMAIL_TO, mailModel.EMAIL_CC, mailModel.EMAIL_BCC, "", file);
            }
            return true;
        }
        #endregion Sending Mail
    }
}