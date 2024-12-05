﻿using NeoErp.Core.Domain;
using NeoErp.Core.Helpers;
using NeoErp.Core.Models;
using NeoErp.Distribution.Service.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace NeoErp.Distribution.Service
{
    public class DistributionService : IDistributionService
    {
        private NeoErpCoreEntity _objectEntity;

        public DistributionService(NeoErpCoreEntity objectEntity)
        {
            this._objectEntity = objectEntity;
            //  var connectionString = ConfigurationManager.ConnectionStrings["DistributionNeoErpCoreEntity"].ConnectionString;
            // _objectEntity.ChangeConnectionString(connectionString);

            //  this._objectEntity.

        }

        public List<RouteList> GetRoute()
        {
            var Query = @"SELECT * FROM (
                (SELECT
                DM.DISTRIBUTOR_CODE AS CODE,
                CS.CUSTOMER_EDESC AS NAME,
                DM.CONTACT_NO AS P_CONTACT_NO, DM.REG_OFFICE_ADDRESS AS ADDRESS,
                RM.ROUTE_CODE, RD.ASSIGN_DATE, RM.ROUTE_NAME,
                AM.AREA_CODE, AM.AREA_NAME,
                RE.ORDER_NO,
                RM.COMPANY_CODE,
                'distributor' AS TYPE,
                '' PARENT_DISTRIBUTOR_CODE,
                '' PARENT_DISTRIBUTOR_NAME,
                DM.LATITUDE, DM.LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, LT.IS_VISITED, LT.REMARKS
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_DISTRIBUTOR_MASTER DM ON DM.DISTRIBUTOR_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RM.COMPANY_CODE AND DM.ACTIVE = 'Y'
                INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
                INNER JOIN DIST_DISTRIBUTOR_AREA DA ON DA.DISTRIBUTOR_CODE = DM.DISTRIBUTOR_CODE AND DA.COMPANY_CODE = DM.COMPANY_CODE
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DA.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                LEFT JOIN(SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                          (CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN 'Y' ELSE NULL END) IS_VISITED, A.REMARKS,
                          TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                          FROM DIST_LOCATION_TRACK A
                          INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                          INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                          WHERE 1 = 1
                          -- AND SP_CODE = '1000097'-- Commented out because customer can be visited by another SP_CODE
                          AND A.CUSTOMER_TYPE = 'D'
                          AND TO_CHAR(A.UPDATE_DATE, 'RRRR-MM-DD HH24:MI A.M.') = (SELECT TO_CHAR(MAX(UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, (CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN 'Y' ELSE NULL END), A.REMARKS
                          ) LT
                          ON LT.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
                WHERE 1 = 1
                AND TO_CHAR(RD.ASSIGN_DATE, 'DD-MON-RRRR') = '13-JAN-2017'
              --  AND RD.EMP_CODE = '{$code}'
                AND RD.COMPANY_CODE = '03'
                )
            UNION
                (SELECT
                RES.RESELLER_CODE AS CODE,
                RES.RESELLER_NAME AS NAME,
                RES.CONTACT_NO AS P_CONTACT_NO, RES.REG_OFFICE_ADDRESS AS ADDRESS,
                RM.ROUTE_CODE, RD.ASSIGN_DATE, RM.ROUTE_NAME,
                AM.AREA_CODE, AM.AREA_NAME,
                RE.ORDER_NO,
                RM.COMPANY_CODE,
                'reseller' AS TYPE,
                DISTRIBUTOR_CODE AS PARENT_DISTRIBUTOR_CODE,
                CS.CUSTOMER_EDESC AS PARENT_DISTRIBUTOR_NAME,
                RES.LATITUDE, RES.LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, LT.IS_VISITED, LT.REMARKS
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'R' AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_RESELLER_MASTER RES ON RES.RESELLER_CODE = RE.ENTITY_CODE AND RES.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = RES.AREA_CODE AND AM.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = RES.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = RM.COMPANY_CODE
                LEFT JOIN(SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS,
                          (CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN 'Y' ELSE NULL END) IS_VISITED, A.REMARKS,
                          TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
                          FROM DIST_LOCATION_TRACK A
                          INNER JOIN DIST_RESELLER_MASTER B ON B.RESELLER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                          INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                          WHERE 1 = 1
                          AND A.CUSTOMER_TYPE = 'R'
                          AND TO_CHAR(A.UPDATE_DATE, 'RRRR-MM-DD HH24:MI A.M.') = (SELECT TO_CHAR(MAX(UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, (CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN 'Y' ELSE NULL END), A.REMARKS
                          ) LT
                          ON LT.CUSTOMER_CODE = RES.RESELLER_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
                WHERE 1 = 1
                AND TO_CHAR(RD.ASSIGN_DATE, 'DD-MON-RRRR') = '13-JAN-2017'
             --   AND RD.EMP_CODE = '{$code}'
                AND RD.COMPANY_CODE = '03'   )
            )
            ORDER BY UPPER(ROUTE_NAME), ORDER_NO, UPPER(AREA_NAME), UPPER(NAME), LAST_VISIT_DATE DESC";
            var data = _objectEntity.SqlQuery<RouteList>(Query).ToList();
            return data;
        }

        public List<SalesRegisterModel> GetSalesRegister(ReportFiltersModel filters, User userinfo)
        {
            var companyCode = string.Empty;
            foreach (var company in filters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }
            companyCode = companyCode == "" ? $@"'{userinfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);
            string query = string.Format(@"SELECT sales_date as SalesDate,
                       bs_date (sales_date) Miti ,
                       sales_no as InvoiceNumber,
                       INITCAP (CS.CUSTOMER_EDESC) CustomerName,
                       INITCAP (IMS.ITEM_EDESC) ItemName,
                       INITCAP (ls.location_edesc) LocationName,
                       SI.MANUAL_NO as ManualNo,
                       SI.REMARKS as REMARKS,
                       --INITCAP (ES.EMPLOYEE_EDESC) Dealer,
                       INITCAP (PTC.PARTY_TYPE_EDESC) PartyType,
                       SI.SHIPPING_ADDRESS as SHIPPINGCODE,
                       SI.SHIPPING_CONTACT_NO as ShippingContactNo,
                       CT.CITY_EDESC as ShippingAddress,
                        ast.AREA_EDESC,
                       INITCAP (SI.MU_CODE) Unit ,
                       Round(NVL(SI.QUANTITY,0)/{0},{1}) as Quantity,
                       Round(NVL(FN_CONVERT_CURRENCY(NVL(SI.CALC_UNIT_PRICE,0),'NRS',SI.SALES_DATE),0)/{2},{3}) as UnitPrice,
                       Round(NVL(FN_CONVERT_CURRENCY(NVL(SI.CALC_TOTAL_PRICE,0),'NRS',SI.SALES_DATE),0)/{4},{5}) as TotalPrice
                        FROM SA_SALES_INVOICE si,
                       IP_ITEM_MASTER_SETUP ims,
                       SA_CUSTOMER_SETUP cs,
                       IP_LOCATION_SETUP ls,
                       --HR_EMPLOYEE_SETUP es,
                       IP_PARTY_TYPE_CODE ptc,
                             CITY_CODE ct,
                        AREA_SETUP ast
                       WHERE SI.ITEM_CODE = IMS.ITEM_CODE
                        and si.company_code=IMS.company_code
                        and si.AREA_CODE = ast.AREA_CODE(+)
                       and SI.COMPANY_CODE=cs.company_code
                       and SI.company_code=ls.company_code
                          and  SI.SHIPPING_ADDRESS= ct.city_code(+)
                         --and SI.COMPANY_CODE=ES.COMPANY_CODE
                      AND SI.COMPANY_CODE=PTC.COMPANY_CODE
                       AND si.COMPANY_CODE IN(" + companyCode + @") AND si.CUSTOMER_CODE = cs.CUSTOMER_CODE"
                     , ReportFilterHelper.FigureFilterValue(filters.QuantityFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.QuantityRoundUpFilter), ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter)
                        , ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter));
            if (userinfo.LoginType == "Synergy")
            {
                if (filters.CustomerFilter.Count > 0)
                {

                    var customers = filters.CustomerFilter;
                    var customerConditionQuery = string.Empty;
                    for (int i = 0; i < customers.Count; i++)
                    {

                        if (i == 0)
                            customerConditionQuery += string.Format("MASTER_CUSTOMER_CODE like (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%' from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G')", customers[i], companyCode);
                        else
                        {
                            customerConditionQuery += string.Format(" OR  MASTER_CUSTOMER_CODE like (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G')", companyCode);
                        }
                    }

                    query = query + string.Format(@" AND SI.CUSTOMER_CODE IN (SELECT DISTINCT(CUSTOMER_CODE) FROM SA_CUSTOMER_SETUP WHERE  {0} OR (CUSTOMER_CODE IN ({1}) AND GROUP_SKU_FLAG='I')) ", customerConditionQuery, string.Join(",", customers));
                }
                if (filters.DocumentFilter.Count > 0)
                {
                    query = query + string.Format(@" AND  SI. FORM_CODE  IN  ({0})", string.Join(",", filters.DocumentFilter).ToString());
                }
                if (filters.PartyTypeFilter.Count > 0)
                {
                    query = query + string.Format(@" AND SI.PARTY_TYPE_CODE IN ('{0}') ", string.Join("','", filters.PartyTypeFilter).ToString());
                }
                if (filters.AreaTypeFilter.Count > 0)
                {
                    query = query + string.Format(@" AND SI.AREA_CODE IN ('{0}') ", string.Join("','", filters.AreaTypeFilter).ToString());
                }
                if (filters.EmployeeFilter.Count > 0)
                {
                    query = query + string.Format(@" AND  SI.EMPLOYEE_CODE IN  ('{0}')", string.Join("','", filters.EmployeeFilter).ToString());
                }
                if (filters.AgentFilter.Count > 0)
                {
                    query = query + string.Format(@" AND  SI.AGENT_CODE IN  ('{0}')", string.Join("','", filters.AgentFilter).ToString());
                }
                if (filters.DivisionFilter.Count > 0)
                {
                    query = query + string.Format(@" AND  SI.DIVISION_CODE IN  ('{0}')", string.Join("','", filters.DivisionFilter).ToString());
                }
                if (filters.LocationFilter.Count > 0)
                {

                    var locations = filters.LocationFilter;
                    var locationConditionQuery = string.Empty;
                    for (int i = 0; i < locations.Count; i++)
                    {

                        if (i == 0)
                            locationConditionQuery += string.Format("SELECT LOCATION_CODE FROM IP_LOCATION_SETUP WHERE LOCATION_CODE LIKE '{0}%'", locations[i]);
                        else
                        {
                            locationConditionQuery += string.Format(" OR  LOCATION_CODE like '{0}%' ", locations[i]);
                        }
                    }
                    query = query + string.Format(@" AND SI.FROM_LOCATION_CODE IN ({0} OR LOCATION_CODE IN ('{1}'))", locationConditionQuery, string.Join("','", locations));
                    //query = query + string.Format(@" AND (SI.FROM_LOCATION_CODE = LS.LOCATION_CODE OR SI.FROM_LOCATION_CODE IN ('{0}'))", string.Join("','", filters.LocationFilter).ToString());
                }
                //if (filters.CompanyFilter.Count > 0)
                //{
                //    query = query + string.Format(@" AND si.COMPANY_CODE = cmps.COMPANY_CODE AND SI.COMPANY_CODE IN ('{0}')", string.Join("','", filters.CompanyFilter).ToString());
                //}
                if (filters.BranchFilter.Count > 0)
                {
                    query = query + string.Format(@" AND SI.BRANCH_CODE IN ('{0}')", string.Join("','", filters.BranchFilter).ToString());
                }
            }
            else
            {
                query = query + $" AND SI.CUSTOMER_CODE ='{userinfo.DistributerNo}'";
            }

            if (filters.ProductFilter.Count > 0)
            {

                var products = filters.ProductFilter;
                var productConditionQuery = string.Empty;
                for (int i = 0; i < products.Count; i++)
                {

                    if (i == 0)
                        productConditionQuery += string.Format("MASTER_ITEM_CODE like (Select DISTINCT(MASTER_ITEM_CODE) || '%' from IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G')", products[i], companyCode);
                    else
                    {
                        productConditionQuery += string.Format(" OR  MASTER_ITEM_CODE like (Select DISTINCT(MASTER_ITEM_CODE) || '%'  from IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G')", products[i], companyCode);
                    }
                }

                query = query + string.Format(@" AND SI.ITEM_CODE IN (SELECT DISTINCT(ITEM_CODE) FROM IP_ITEM_MASTER_SETUP WHERE {0} OR (ITEM_CODE IN ({1}) AND GROUP_SKU_FLAG='I')) ", productConditionQuery, string.Join(",", products));

                //query = query + string.Format(@" AND SI.ITEM_CODE IN ({0})", string.Join(",", filters.ProductFilter).ToString());
            }
            if (filters.CategoryFilter.Count > 0)
            {
                query = query + string.Format(@" AND ims.CATEGORY_CODE IN ('{0}') ", string.Join("','", filters.CategoryFilter).ToString());
            }
            //EMPLOYEE_CODE doesn't exist on the NNPL &WLINK database , So We have manually created column of EMPLOYEE_CODE for the both database .
            query = query + @" AND SI.FROM_LOCATION_CODE = LS.LOCATION_CODE
                        and si.Deleted_flag = 'N'
                      --AND SI.EMPLOYEE_CODE = ES.EMPLOYEE_CODE(+)
                       AND SI.PARTY_TYPE_CODE = PTC.PARTY_TYPE_CODE(+)";
            if (!string.IsNullOrEmpty(filters.FromDate))
                query = query + " and SALES_DATE>=TO_DATE('" + filters.FromDate + "', 'YYYY-MM-DD') and SALES_DATE <= TO_DATE('" + filters.ToDate + "', 'YYYY-MM-DD')";


            int min = 0, max = 0;

            ReportFilterHelper.RangeFilterValue(filters.AmountRangeFilter, out min, out max);
            if (!(min == 0 && max == 0))
                query = query + string.Format(@" and NVL(FN_CONVERT_CURRENCY(NVL(SI.TOTAL_PRICE,0),'NRS',SI.SALES_DATE),0) >= {0} and NVL(FN_CONVERT_CURRENCY(NVL(SI.TOTAL_PRICE,0),'NRS',SI.SALES_DATE),0)  <= {1}", min, max);

            ReportFilterHelper.RangeFilterValue(filters.QuantityRangeFilter, out min, out max);
            if (!(min == 0 && max == 0))
                query = query + string.Format(@" and NVL(SI.QUANTITY,0) >= {0} and NVL(SI.QUANTITY,0) <= {1}", min, max);

            ReportFilterHelper.RangeFilterValue(filters.RateRangeFilter, out min, out max);
            if (!(min == 0 && max == 0))
                query = query + string.Format(@" and NVL(FN_CONVERT_CURRENCY(NVL(SI.UNIT_PRICE,0),'NRS',SI.SALES_DATE),0) >= {0} and NVL(FN_CONVERT_CURRENCY(NVL(SI.UNIT_PRICE,0),'NRS',SI.SALES_DATE),0) <= {1}", min, max);

            //if (filters.ItemBrandFilter.Count > 0)
            //    query = $" AND ES.EMPLOYEE_CODE IN  ('{ string.Join("','", filters.ItemBrandFilter).ToString()}')";

            var salesRegisters = _objectEntity.SqlQuery<SalesRegisterModel>(query).ToList();
            return salesRegisters;
        }

        public List<VistitedPlanModel> GetCurrentVistedEntity(string date, string CompanyCode)
        {
            //var Query = @"SELECT * FROM (
            //    (SELECT
            //    DM.DISTRIBUTOR_CODE AS CODE,
            //    CS.CUSTOMER_EDESC AS NAME,
            //    DM.CONTACT_NO AS CONTACT, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS) AS ADDRESS,
            //    RM.ROUTE_CODE, RD.ASSIGN_DATE, RM.ROUTE_NAME,
            //    AM.AREA_CODE, AM.AREA_NAME,
            //    RE.ORDER_NO,
            //    RM.COMPANY_CODE,
            //    'distributor' AS TYPE,
            //    '' PARENT_DISTRIBUTOR_CODE,
            //    '' PARENT_DISTRIBUTOR_NAME,
            //    DM.LATITUDE, DM.LONGITUDE,
            //    LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, LT.IS_VISITED, LT.REMARKS,LT.LAST_VISIT_BY_CODE
            //    FROM DIST_ROUTE_MASTER RM
            //    INNER JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
            //    INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = RM.COMPANY_CODE
            //    INNER JOIN DIST_DISTRIBUTOR_MASTER DM ON DM.DISTRIBUTOR_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RM.COMPANY_CODE
            //    INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
            //    INNER JOIN DIST_DISTRIBUTOR_AREA DA ON DA.DISTRIBUTOR_CODE = DM.DISTRIBUTOR_CODE AND DA.COMPANY_CODE = DM.COMPANY_CODE
            //    INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DA.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
            //    LEFT JOIN(SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY,C.EMPLOYEE_CODE AS LAST_VISIT_BY_CODE, A.IS_VISITED AS LAST_VISIT_STATUS,
            //              (CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN 'Y' ELSE NULL END) IS_VISITED, A.REMARKS,
            //              TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
            //              FROM DIST_LOCATION_TRACK A
            //              INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
            //              INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
            //              WHERE 1 = 1
            //              -- AND SP_CODE = '1000097'-- Commented out because customer can be visited by another SP_CODE
            //              AND A.CUSTOMER_TYPE = 'D'
            //              AND TO_CHAR(A.UPDATE_DATE, 'RRRR-MM-DD HH24:MI A.M.') = (SELECT TO_CHAR(MAX(UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
            //              GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC,C.EMPLOYEE_CODE, A.IS_VISITED, (CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN 'Y' ELSE NULL END), A.REMARKS
            //              ) LT
            //              ON LT.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
            //    WHERE 1 = 1
            //    AND TO_CHAR(RD.ASSIGN_DATE, 'DD-MON-RRRR') = UPPER('" + date + @"')
            //    AND RD.COMPANY_CODE = '" + CompanyCode + @"'
            //    )
            //UNION
            //    (SELECT
            //    RES.RESELLER_CODE AS CODE,
            //    RES.RESELLER_NAME AS NAME,
            //    RES.CONTACT_NO AS CONTACT, RES.REG_OFFICE_ADDRESS AS ADDRESS,
            //    RM.ROUTE_CODE, RD.ASSIGN_DATE, RM.ROUTE_NAME,
            //    AM.AREA_CODE, AM.AREA_NAME,
            //    RE.ORDER_NO,
            //    RM.COMPANY_CODE,
            //    'reseller' AS TYPE,
            //    DISTRIBUTOR_CODE AS PARENT_DISTRIBUTOR_CODE,
            //    CS.CUSTOMER_EDESC AS PARENT_DISTRIBUTOR_NAME,
            //    RES.LATITUDE, RES.LONGITUDE,
            //    LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, LT.IS_VISITED, LT.REMARKS,LT.LAST_VISIT_BY_CODE
            //    FROM DIST_ROUTE_MASTER RM
            //    INNER JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
            //    INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'R' AND RE.COMPANY_CODE = RM.COMPANY_CODE
            //    INNER JOIN DIST_RESELLER_MASTER RES ON RES.RESELLER_CODE = RE.ENTITY_CODE AND RES.COMPANY_CODE = RM.COMPANY_CODE
            //    INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = RES.AREA_CODE AND AM.COMPANY_CODE = RM.COMPANY_CODE
            //    INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = RES.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = RM.COMPANY_CODE
            //    LEFT JOIN(SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY,C.EMPLOYEE_CODE AS LAST_VISIT_BY_CODE, A.IS_VISITED AS LAST_VISIT_STATUS,
            //              (CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN 'Y' ELSE NULL END) IS_VISITED, A.REMARKS,
            //              TO_CHAR(MAX(A.UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') LAST_VISIT_DATE
            //              FROM DIST_LOCATION_TRACK A
            //              INNER JOIN DIST_RESELLER_MASTER B ON B.RESELLER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
            //              INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
            //              WHERE 1 = 1
            //              AND A.CUSTOMER_TYPE = 'R'
            //              AND TO_CHAR(A.UPDATE_DATE, 'RRRR-MM-DD HH24:MI A.M.') = (SELECT TO_CHAR(MAX(UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
            //              GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC,C.EMPLOYEE_CODE, A.IS_VISITED, (CASE WHEN TO_CHAR(SYSDATE, 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN 'Y' ELSE NULL END), A.REMARKS
            //              ) LT
            //              ON LT.CUSTOMER_CODE = RES.RESELLER_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
            //    WHERE 1 = 1
            //    AND TO_CHAR(RD.ASSIGN_DATE, 'DD-MON-RRRR') = UPPER('" + date + @"')
            //    AND RD.COMPANY_CODE = '" + CompanyCode + @"'
            //    )
            //)
            //ORDER BY UPPER(ROUTE_NAME), ORDER_NO, UPPER(AREA_NAME), UPPER(NAME), LAST_VISIT_DATE DESC";


            var Query = @"SELECT * FROM(
                (SELECT
                DM.DEALER_CODE AS CODE,
                PT.PARTY_TYPE_EDESC AS NAME,
                DM.CONTACT_NO AS CONTACT, DM.REG_OFFICE_ADDRESS AS ADDRESS,
                RM.ROUTE_CODE, RD.ASSIGN_DATE, RM.ROUTE_NAME,
                AM.AREA_CODE, AM.AREA_NAME,
                RE.ORDER_NO,
                RM.COMPANY_CODE,
                'dealer' AS TYPE,
                '' PARENT_DISTRIBUTOR_CODE,
                '' PARENT_DISTRIBUTOR_NAME,
                DM.LATITUDE, DM.LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS, LT.LAST_VISIT_BY_CODE
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'P' AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_DEALER_MASTER DM ON DM.DEALER_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN IP_PARTY_TYPE_CODE PT ON PT.PARTY_TYPE_CODE = DM.DEALER_CODE AND PT.COMPANY_CODE = DM.COMPANY_CODE
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                LEFT JOIN(SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, C.EMPLOYEE_CODE AS LAST_VISIT_BY_CODE, A.IS_VISITED AS LAST_VISIT_STATUS,
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
                          -- AND SP_CODE = '1000097'-- Commented out because customer can be visited by another SP_CODE
                          AND A.CUSTOMER_TYPE = 'P'
                          AND TO_CHAR(A.UPDATE_DATE, 'RRRR-MM-DD HH24:MI A.M.') = (SELECT TO_CHAR(MAX(UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, C.EMPLOYEE_CODE,A.IS_VISITED, 
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
                AND RM.ROUTE_TYPE = 'D'
                AND TO_CHAR(RD.ASSIGN_DATE, 'DD-MON-RRRR') = UPPER('" + date + @"')
                AND RD.COMPANY_CODE = '" + CompanyCode + @"'
                )
              UNION
                (SELECT
                DM.DISTRIBUTOR_CODE AS CODE,
                CS.CUSTOMER_EDESC AS NAME,
                DM.CONTACT_NO AS CONTACT, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS) AS ADDRESS,
                RM.ROUTE_CODE, RD.ASSIGN_DATE, RM.ROUTE_NAME,
                AM.AREA_CODE, AM.AREA_NAME,
                RE.ORDER_NO,
                RM.COMPANY_CODE,
                'distributor' AS TYPE,
                '' PARENT_DISTRIBUTOR_CODE,
                '' PARENT_DISTRIBUTOR_NAME,
                DM.LATITUDE, DM.LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS, LT.LAST_VISIT_BY_CODE
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_DISTRIBUTOR_MASTER DM ON DM.DISTRIBUTOR_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RM.COMPANY_CODE AND DM.ACTIVE = 'Y'
                INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                LEFT JOIN(SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, C.EMPLOYEE_CODE AS LAST_VISIT_BY_CODE, A.IS_VISITED AS LAST_VISIT_STATUS,
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
                          -- AND SP_CODE = '1000097'-- Commented out because customer can be visited by another SP_CODE
                          AND A.CUSTOMER_TYPE = 'D'
                          AND TO_CHAR(A.UPDATE_DATE, 'RRRR-MM-DD HH24:MI A.M.') = (SELECT TO_CHAR(MAX(UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC,C.EMPLOYEE_CODE, A.IS_VISITED, 
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
                AND RM.ROUTE_TYPE = 'D'
                AND TO_CHAR(RD.ASSIGN_DATE, 'DD-MON-RRRR') = UPPER('" + date + @"')
                AND RD.COMPANY_CODE = '" + CompanyCode + @"'
                )
            UNION
                (SELECT
                RES.RESELLER_CODE AS CODE,
                RES.RESELLER_NAME AS NAME,
                RES.CONTACT_NO AS CONTACT, RES.REG_OFFICE_ADDRESS AS ADDRESS,
                RM.ROUTE_CODE, RD.ASSIGN_DATE, RM.ROUTE_NAME,
                AM.AREA_CODE, AM.AREA_NAME,
                RE.ORDER_NO,
                RM.COMPANY_CODE,
                'reseller' AS TYPE,
                DISTRIBUTOR_CODE AS PARENT_DISTRIBUTOR_CODE,
                CS.CUSTOMER_EDESC AS PARENT_DISTRIBUTOR_NAME,
                RES.LATITUDE, RES.LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, LT.LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS, LT.LAST_VISIT_BY_CODE
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'R' AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_RESELLER_MASTER RES ON RES.RESELLER_CODE = RE.ENTITY_CODE AND RES.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = RES.AREA_CODE AND AM.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = RES.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = RM.COMPANY_CODE
                LEFT JOIN(SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, C.EMPLOYEE_CODE AS LAST_VISIT_BY_CODE, A.IS_VISITED AS LAST_VISIT_STATUS,
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
                          AND B.IS_CLOSED = 'N'
                          AND TO_CHAR(A.UPDATE_DATE, 'RRRR-MM-DD HH24:MI A.M.') = (SELECT TO_CHAR(MAX(UPDATE_DATE), 'RRRR-MM-DD HH24:MI A.M.') FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC,C.EMPLOYEE_CODE, A.IS_VISITED, 
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
                AND RM.ROUTE_TYPE = 'D'
                AND RES.IS_CLOSED = 'N'
                AND TO_CHAR(RD.ASSIGN_DATE, 'DD-MON-RRRR') = UPPER('" + date + @"')
                AND RD.COMPANY_CODE = '" + CompanyCode + @"'
                )
            )
            ORDER BY UPPER(ROUTE_NAME), ORDER_NO, UPPER(AREA_NAME), UPPER(NAME), LAST_VISIT_DATE DESC";


            var data = _objectEntity.SqlQuery<VistitedPlanModel>(Query).ToList();
            return data;
        }

        public List<MrVisitedTrackingModel> GetCurrentMrTrackingDetails(DateTime SUBMITDATE)
        {
            var query = @"SELECT A.SP_CODE, TO_CHAR(A.SUBMIT_DATE, 'DD-MM-YYYY HH24:MI A.M.') SUBMIT_DATE, A.LATITUDE, A.LONGITUDE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC,A.Track_type,P.FILENAME,P.CATEGORYID FROM DIST_LM_LOCATION_TRACKING A INNER JOIN DIST_SALESPERSON_MASTER B
   ON B.SP_CODE = A.SP_CODE
INNER JOIN HR_EMPLOYEE_SETUP C
   ON C.EMPLOYEE_CODE = B.SP_CODE
INNER JOIN (SELECT SP_CODE, TO_CHAR(MAX(SUBMIT_DATE), 'DD-MM-YYYY HH24:MI A.M.') SUBMIT_DATE
             FROM DIST_LM_LOCATION_TRACKING GROUP BY SP_CODE) D
   ON D.SP_CODE = A.SP_CODE AND D.SUBMIT_DATE = TO_CHAR(A.SUBMIT_DATE, 'DD-MM-YYYY HH24:MI A.M.') 
      LEFT join DIST_PHOTO_INFO P
   ON P.ENTITY_CODE=A.SP_CODE AND TO_CHAR(A.SUBMIT_DATE , 'DD-MM-YYYY') = TO_CHAR(P.CREATE_DATE, 'DD-MM-YYYY') 
   ORDER BY TRIM(C.EMPLOYEE_EDESC)";
            //--WHERE TRUNC(A.SUBMIT_DATE) = TRUNC(TO_DATE('21-Feb-2017', 'DD-MON-YYYY'))

            var data = _objectEntity.SqlQuery<MrVisitedTrackingModel>(query).ToList();
            return data;
        }
        public List<MrVisitedTrackingRecord> GetDateWiseMrVistedRoute(string date, string spCode, string companyCode = "01")
        {

            var query = $@"SELECT *
FROM(
   SELECT LT.SP_CODE,
     LT.SUBMIT_DATE AS  ACTIVITY_TIME,
     LT.LATITUDE,
     LT.LONGITUDE,
     '' VISITED,
     '' REMARKS,
     '' ENTITY_CODE,
     '' ENTITY_NAME,
     '' ENTITY_TYPE,
     LT.track_type
   FROM DIST_LM_LOCATION_TRACKING LT
   WHERE 1 = 1
     AND LT.COMPANY_CODE = '{companyCode}'
     AND LT.SP_CODE = '{spCode}'
     AND TRUNC(LT.SUBMIT_DATE) = TRUNC(TO_DATE('{date}',
'DD-MON-RRRR'))
   UNION ALL
   SELECT LT.SP_CODE,
     LT.UPDATE_DATE AS ACTIVITY_TIME,
     LT.LATITUDE,
     LT.LONGITUDE,
     LT.IS_VISITED AS VISITED,
     LT.REMARKS,
     CS.CUSTOMER_CODE AS ENTITY_CODE,
     CS.CUSTOMER_EDESC AS ENTITY_NAME,
     LT.CUSTOMER_TYPE AS ENTITY_TYPE,
     '' track_type
   FROM DIST_LOCATION_TRACK LT
   INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = LT.CUSTOMER_CODE AND CS.COMPANY_CODE = LT.COMPANY_CODE
   WHERE 1 = 1
     AND LT.CUSTOMER_TYPE = 'D'
     AND LT.COMPANY_CODE = '{companyCode}'
     AND LT.SP_CODE = '{spCode}'
     AND TRUNC(LT.UPDATE_DATE) = TRUNC(TO_DATE('{date}',
'DD-MON-RRRR'))
   UNION ALL
   SELECT LT.SP_CODE,
     LT.UPDATE_DATE AS ACTIVITY_TIME,
     LT.LATITUDE,
     LT.LONGITUDE,
     LT.IS_VISITED AS VISITED,
     LT.REMARKS,
     RM.RESELLER_CODE AS ENTITY_CODE,
     RM.RESELLER_NAME AS ENTITY_NAME,
     LT.CUSTOMER_TYPE AS ENTITY_TYPE,
      '' track_type
   FROM DIST_LOCATION_TRACK LT
   INNER JOIN DIST_RESELLER_MASTER RM ON RM.RESELLER_CODE = LT.CUSTOMER_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
   WHERE 1 = 1
     AND LT.CUSTOMER_TYPE = 'R'
     AND RM.IS_CLOSED = 'N'
     AND LT.COMPANY_CODE = '{companyCode}'
     AND LT.SP_CODE = '{spCode}'
     AND TRUNC(LT.UPDATE_DATE) = TRUNC(TO_DATE('{date}',
'DD-MON-RRRR'))
   UNION ALL
   SELECT EA.SP_CODE,
     EA.VISIT_DATE AS ACTIVITY_TIME,
     EA.LATITUDE,
     EA.LONGITUDE,
     '' VISITED,
     EA.REMARKS,
     '' ENTITY_CODE,
     '' ENTITY_NAME,
     '' ENTITY_TYPE,
      '' track_type
   FROM DIST_EXTRA_ACTIVITY EA
   WHERE 1 = 1
     AND EA.COMPANY_CODE = '{companyCode}'
     AND EA.SP_CODE = '{spCode}'
     AND TRUNC(EA.VISIT_DATE) = TRUNC(TO_DATE('{date}',
'DD-MON-RRRR'))
)
ORDER BY ACTIVITY_TIME ASC";

            var data = _objectEntity.SqlQuery<MrVisitedTrackingRecord>(query).ToList();
            return data;
        }



        public List<MrVisitedTrackingModel> GetMRVisitedAllAssignRoute(string date, string spcode, string companyCode)
        {
            string Query = $@"SELECT * FROM (
                (SELECT
                DM.DEALER_CODE AS ENTITY_CODE,
                PT.PARTY_TYPE_EDESC AS ENTITY_NAME,
                DM.CONTACT_NO AS P_CONTACT_NO, DM.REG_OFFICE_ADDRESS AS ADDRESS,
--                RM.ROUTE_CODE, RM.ROUTE_NAME, TRUNC(RD.ASSIGN_DATE) ASSIGN_DATE,
--                AM.AREA_CODE, AM.AREA_NAME,
                'P' AS ENTITY_TYPE,
                'PJP' AS JOURNEY_PLAN,
                NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, NVL(LT.LAST_VISIT_STATUS, 'X') LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS
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
                                CASE WHEN TO_CHAR(TO_DATE('{date}', 'DD-MON-RRRR'), 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
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
                                    CASE WHEN TO_CHAR(TO_DATE('{date}', 'DD-MON-RRRR'), 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                               END
                              ), 
                              A.REMARKS
                          ) LT
                          ON LT.CUSTOMER_CODE = DM.DEALER_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
                WHERE 1 = 1
                AND RM.ROUTE_TYPE = 'D'
                AND TRUNC(RD.ASSIGN_DATE) = TRUNC(TO_DATE('{date}', 'DD-MON-RRRR'))
                AND RD.EMP_CODE = '{spcode}'
                AND RD.COMPANY_CODE IN('{companyCode}')
                )
              UNION
                (SELECT
                DM.DISTRIBUTOR_CODE AS ENTITY_CODE,
                CS.CUSTOMER_EDESC AS ENTITY_NAME,
                DM.CONTACT_NO AS P_CONTACT_NO, NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS) AS ADDRESS,
--                RM.ROUTE_CODE, RM.ROUTE_NAME, RD.ASSIGN_DATE,
--                AM.AREA_CODE, AM.AREA_NAME,
                'D' AS ENTITY_TYPE,
                'PJP' AS JOURNEY_PLAN,
                NVL(DM.LATITUDE,0) LATITUDE, NVL(DM.LONGITUDE,0) LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, NVL(LT.LAST_VISIT_STATUS, 'X') LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_DISTRIBUTOR_MASTER DM ON DM.DISTRIBUTOR_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RM.COMPANY_CODE AND DM.ACTIVE = 'Y'
                INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS, 
                          (CASE 
                            WHEN A.IS_VISITED IS NULL THEN 'X' 
                              ELSE
                                CASE WHEN TO_CHAR(TO_DATE('{date}', 'DD-MON-RRRR'), 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
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
                                    CASE WHEN TO_CHAR(TO_DATE('{date}', 'DD-MON-RRRR'), 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                               END
                              ), 
                              A.REMARKS
                          ) LT
                          ON LT.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
                WHERE 1 = 1
                AND RM.ROUTE_TYPE = 'D'
                AND TRUNC(RD.ASSIGN_DATE) = TRUNC(TO_DATE('{date}', 'DD-MON-RRRR'))
                AND RD.EMP_CODE = '{spcode}'
                AND RD.COMPANY_CODE IN('{companyCode}')
                )
            UNION
                (SELECT
                RES.RESELLER_CODE AS ENTITY_CODE,
                RES.RESELLER_NAME AS ENTITY_NAME,
                RES.CONTACT_NO AS P_CONTACT_NO, RES.REG_OFFICE_ADDRESS AS ADDRESS,
--                RM.ROUTE_CODE, RM.ROUTE_NAME, RD.ASSIGN_DATE,
--                AM.AREA_CODE, AM.AREA_NAME,
                'R' AS ENTITY_TYPE,
                'PJP' AS JOURNEY_PLAN,
                NVL(RES.LATITUDE,0) LATITUDE, NVL(RES.LONGITUDE,0) LONGITUDE,
                LT.LAST_VISIT_DATE, LT.LAST_VISIT_BY, NVL(LT.LAST_VISIT_STATUS, 'X') LAST_VISIT_STATUS, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS               
                FROM DIST_ROUTE_MASTER RM
                INNER JOIN DIST_ROUTE_DETAIL RD ON RD.ROUTE_CODE = RM.ROUTE_CODE AND RD.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'R' AND RE.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_RESELLER_MASTER RES ON RES.RESELLER_CODE = RE.ENTITY_CODE AND RES.COMPANY_CODE = RM.COMPANY_CODE
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = RES.AREA_CODE AND AM.COMPANY_CODE = RM.COMPANY_CODE
                LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC AS LAST_VISIT_BY, A.IS_VISITED AS LAST_VISIT_STATUS, 
                          (CASE 
                            WHEN A.IS_VISITED IS NULL THEN 'X' 
                              ELSE
                                CASE WHEN TO_CHAR(TO_DATE('{date}', 'DD-MON-RRRR'), 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                           END
                          ) IS_VISITED, 
                          A.REMARKS,
                          TO_CHAR(MAX(A.UPDATE_DATE), 'DD-MON-RRRR HH24:MI A.M.') LAST_VISIT_DATE
                          FROM DIST_LOCATION_TRACK A
                          INNER JOIN DIST_RESELLER_MASTER B ON B.RESELLER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                          INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                          WHERE 1 = 1
                          AND A.CUSTOMER_TYPE = 'R'
                          AND B.IS_CLOSED = 'N'
                          AND A.UPDATE_DATE = (SELECT MAX(UPDATE_DATE) FROM DIST_LOCATION_TRACK WHERE CUSTOMER_CODE = A.CUSTOMER_CODE AND CUSTOMER_TYPE = A.CUSTOMER_TYPE)
                          GROUP BY A.SP_CODE, A.CUSTOMER_CODE, A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC, A.IS_VISITED, 
                              (CASE 
                                WHEN A.IS_VISITED IS NULL THEN 'X' 
                                  ELSE
                                    CASE WHEN TO_CHAR(TO_DATE('{date}', 'DD-MON-RRRR'), 'DD-MON-RRRR') = TO_CHAR(A.UPDATE_DATE, 'DD-MON-RRRR') THEN A.IS_VISITED ELSE 'X' END
                               END
                              ), 
                              A.REMARKS
                          ) LT
                          ON LT.CUSTOMER_CODE = RES.RESELLER_CODE AND LT.COMPANY_CODE = RM.COMPANY_CODE
                WHERE 1 = 1
                AND RM.ROUTE_TYPE = 'D'
                AND RES.IS_CLOSED = 'N'
                AND TRUNC(RD.ASSIGN_DATE) = TRUNC(TO_DATE('{date}', 'DD-MON-RRRR'))
                AND RD.EMP_CODE = '{spcode}'
                AND RD.COMPANY_CODE in('{companyCode}')
                )
            )
            ORDER BY UPPER(ENTITY_NAME), LAST_VISIT_DATE DESC";

            var data = _objectEntity.SqlQuery<MrVisitedTrackingModel>(Query).ToList();
            return data;

        }

        public List<MrVisitedTrackingModel> GetMRVisitedAssignRouteWithOrder(string date, string spcode, string companyCode)
        {
            string Query = $@"SELECT RPO.SP_CODE, RPO.EMPLOYEE_EDESC, RPO.ENTITY_CODE, RPO.ENTITY_NAME, RPO.LATITUDE, RPO.LONGITUDE, 'R' ENTITY_TYPE, 'PJP' JOURNEY_PLAN, RPO.VISITED IS_VISITED, RPO.COMPANY_CODE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.RESELLER_CODE ENTITY_CODE, TRIM(D.RESELLER_NAME) ENTITY_NAME, D.LATITUDE, D.LONGITUDE, (CASE WHEN E.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END) VISITED, A.COMPANY_CODE
                            FROM DIST_IP_SSR_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.SP_CODE = '{spcode}' AND B.ACTIVE = 'Y'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN DIST_RESELLER_MASTER D ON D.RESELLER_CODE = A.RESELLER_CODE AND D.COMPANY_CODE = A.COMPANY_CODE
                            LEFT JOIN DIST_LOCATION_TRACK E ON E.CUSTOMER_CODE = A.RESELLER_CODE AND E.COMPANY_CODE = A.COMPANY_CODE AND E.CUSTOMER_TYPE = 'R' AND TRUNC(E.UPDATE_DATE) = TRUNC(TO_DATE('{date}', 'DD-MON-RRRR'))
                            WHERE TRUNC(A.ORDER_DATE) = TRUNC(TO_DATE('{date}', 'DD-MON-RRRR'))
                            AND D.IS_CLOSED = 'N'
                            GROUP BY A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC), A.RESELLER_CODE, TRIM(D.RESELLER_NAME), D.LATITUDE, D.LONGITUDE, (CASE WHEN E.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END), A.COMPANY_CODE
                      ) RPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TRUNC(TO_DATE('{date}', 'DD-MON-RRRR'))
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y'
                                  WHERE 1 = 1
                                    AND DRD.EMP_CODE = '{spcode}'
                                    AND DLU.COMPANY_CODE IN('{companyCode}')
                                    AND DRM.IS_CLOSED = 'N'
                                    AND DLU.ACTIVE = 'Y'
                                  GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                      ) PJPENT ON PJPENT.USERID = RPO.CREATED_BY AND PJPENT.SP_CODE = RPO.SP_CODE AND PJPENT.ENTITY_CODE = RPO.ENTITY_CODE AND PJPENT.COMPANY_CODE = RPO.COMPANY_CODE
                      WHERE 1 = 1
                        AND PJPENT.SP_CODE IS NOT NULL
                      GROUP BY RPO.SP_CODE, RPO.EMPLOYEE_EDESC, RPO.ENTITY_CODE, RPO.ENTITY_NAME, RPO.LATITUDE, RPO.LONGITUDE, 'R', 'PJP', RPO.VISITED, RPO.COMPANY_CODE
UNION ALL
----- PJP ORDER DISTRIBUTOR ENTITIES -----
SELECT DPO.SP_CODE, DPO.EMPLOYEE_EDESC, DPO.ENTITY_CODE, DPO.ENTITY_NAME, DPO.LATITUDE, DPO.LONGITUDE, 'D' ENTITY_TYPE, 'PJP' JOURNEY_PLAN, DPO.VISITED, DPO.COMPANY_CODE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.CUSTOMER_CODE ENTITY_CODE, TRIM(E.CUSTOMER_EDESC) ENTITY_NAME, D.LATITUDE, D.LONGITUDE, (CASE WHEN F.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END) VISITED, A.COMPANY_CODE
                            FROM DIST_IP_SSD_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.SP_CODE = '{spcode}' AND B.ACTIVE = 'Y'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN DIST_DISTRIBUTOR_MASTER D ON D.DISTRIBUTOR_CODE = A.CUSTOMER_CODE AND D.COMPANY_CODE = A.COMPANY_CODE AND D.ACTIVE = 'Y'
                            INNER JOIN SA_CUSTOMER_SETUP E ON E.CUSTOMER_CODE = D.DISTRIBUTOR_CODE AND E.COMPANY_CODE = D.COMPANY_CODE
                            LEFT JOIN DIST_LOCATION_TRACK F ON F.CUSTOMER_CODE = A.CUSTOMER_CODE AND F.COMPANY_CODE = A.COMPANY_CODE AND F.CUSTOMER_TYPE = 'D' AND TRUNC(F.UPDATE_DATE) = TRUNC(TO_DATE('{date}', 'DD-MON-RRRR'))
                            WHERE TRUNC(A.ORDER_DATE) = TO_DATE('{date}', 'DD-MON-RRRR')
                            GROUP BY A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC), A.CUSTOMER_CODE, TRIM(E.CUSTOMER_EDESC), D.LATITUDE, D.LONGITUDE, (CASE WHEN F.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END), A.COMPANY_CODE
                      ) DPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TRUNC(TO_DATE('{date}', 'DD-MON-RRRR'))
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                                  GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                      ) PJPENT ON PJPENT.USERID = DPO.CREATED_BY AND PJPENT.SP_CODE = DPO.SP_CODE AND PJPENT.ENTITY_CODE = DPO.ENTITY_CODE AND PJPENT.COMPANY_CODE = DPO.COMPANY_CODE
                      WHERE 1 = 1
                        AND PJPENT.SP_CODE IS NOT NULL
                      GROUP BY DPO.SP_CODE, DPO.EMPLOYEE_EDESC, DPO.ENTITY_CODE, DPO.ENTITY_NAME, DPO.LATITUDE, DPO.LONGITUDE, 'D', 'PJP', DPO.VISITED, DPO.COMPANY_CODE
UNION ALL
----- PJP ORDER PARTY TYPE ENTITIES -----
SELECT PPO.SP_CODE, PPO.EMPLOYEE_EDESC, PPO.ENTITY_CODE, PPO.ENTITY_NAME, PPO.LATITUDE, PPO.LONGITUDE, 'P' ENTITY_TYPE, 'PJP' JOURNEY_PLAN, PPO.VISITED, PPO.COMPANY_CODE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.PARTY_TYPE_CODE ENTITY_CODE, TRIM(E.PARTY_TYPE_EDESC) ENTITY_NAME, D.LATITUDE, D.LONGITUDE, (CASE WHEN F.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END) VISITED, A.COMPANY_CODE
                            FROM DIST_IP_SSD_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.ACTIVE = 'Y'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN DIST_DEALER_MASTER D ON D.DEALER_CODE = A.PARTY_TYPE_CODE AND D.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN IP_PARTY_TYPE_CODE E ON E.PARTY_TYPE_CODE = D.DEALER_CODE AND E.COMPANY_CODE = D.COMPANY_CODE
                            LEFT JOIN DIST_LOCATION_TRACK F ON F.CUSTOMER_CODE = A.CUSTOMER_CODE AND F.COMPANY_CODE = A.COMPANY_CODE AND F.CUSTOMER_TYPE = 'P' AND TRUNC(F.UPDATE_DATE) = TRUNC(TO_DATE('{date}', 'DD-MON-RRRR'))
                            WHERE TRUNC(A.ORDER_DATE) = TRUNC(TO_DATE('{date}', 'DD-MON-RRRR'))
                            GROUP BY A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC), A.PARTY_TYPE_CODE, TRIM(E.PARTY_TYPE_EDESC), D.LATITUDE, D.LONGITUDE, (CASE WHEN F.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END), A.COMPANY_CODE
                      ) PPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TRUNC(TO_DATE('{date}', 'DD-MON-RRRR'))
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                                  GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                      ) PJPENT ON PJPENT.USERID = PPO.CREATED_BY AND PJPENT.SP_CODE = PPO.SP_CODE AND PJPENT.ENTITY_CODE = PPO.ENTITY_CODE AND PJPENT.COMPANY_CODE = PPO.COMPANY_CODE
                      WHERE 1 = 1
                        AND PJPENT.SP_CODE IS NOT NULL
                      GROUP BY PPO.SP_CODE, PPO.EMPLOYEE_EDESC, PPO.ENTITY_CODE, PPO.ENTITY_NAME, PPO.LATITUDE, PPO.LONGITUDE, 'P', 'PJP', PPO.VISITED, PPO.COMPANY_CODE";

            var data = _objectEntity.SqlQuery<MrVisitedTrackingModel>(Query).ToList();
            return data;

        }

        public List<MrVisitedTrackingModel> GetMRVisitedUnAssignRouteWithOrder(string date, string spcode, string companyCode)
        {
            string Query = $@"SELECT RPO.SP_CODE, RPO.EMPLOYEE_EDESC, RPO.ENTITY_CODE, RPO.ENTITY_NAME, RPO.LATITUDE, RPO.LONGITUDE, 'R' ENTITY_TYPE, 'NJP' JOURNEY_PLAN, RPO.VISITED IS_VISITED, RPO.COMPANY_CODE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.RESELLER_CODE ENTITY_CODE, TRIM(D.RESELLER_NAME) ENTITY_NAME, D.LATITUDE, D.LONGITUDE, (CASE WHEN E.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END) VISITED, A.COMPANY_CODE
                            FROM DIST_IP_SSR_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.SP_CODE = '{spcode}' AND B.ACTIVE = 'Y'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN DIST_RESELLER_MASTER D ON D.RESELLER_CODE = A.RESELLER_CODE AND D.COMPANY_CODE = A.COMPANY_CODE
                            LEFT JOIN DIST_LOCATION_TRACK E ON E.CUSTOMER_CODE = A.RESELLER_CODE AND E.COMPANY_CODE = A.COMPANY_CODE AND E.CUSTOMER_TYPE = 'R' AND TRUNC(E.UPDATE_DATE) = TRUNC(TO_DATE('{date}', 'DD-MON-RRRR'))
                            WHERE TRUNC(A.ORDER_DATE) = TRUNC(TO_DATE('{date}', 'DD-MON-RRRR'))
                            AND D.IS_CLOSED = 'N'
                            GROUP BY A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC), A.RESELLER_CODE, TRIM(D.RESELLER_NAME), D.LATITUDE, D.LONGITUDE, (CASE WHEN E.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END), A.COMPANY_CODE
                      ) RPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TRUNC(TO_DATE('{date}', 'DD-MON-RRRR'))
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y'
                                  WHERE 1 = 1
                                    AND DRM.IS_CLOSED = 'N'
                                    AND DRD.EMP_CODE = '{spcode}'
                                    AND DLU.ACTIVE = 'Y'
                                    AND DLU.COMPANY_CODE IN('{companyCode}')
                                  GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                      ) NPJPENT ON NPJPENT.USERID = RPO.CREATED_BY AND NPJPENT.SP_CODE = RPO.SP_CODE AND NPJPENT.ENTITY_CODE = RPO.ENTITY_CODE AND NPJPENT.COMPANY_CODE = RPO.COMPANY_CODE
                      WHERE 1 = 1
                        AND NPJPENT.SP_CODE IS NULL
                      GROUP BY RPO.SP_CODE, RPO.EMPLOYEE_EDESC, RPO.ENTITY_CODE, RPO.ENTITY_NAME, RPO.LATITUDE, RPO.LONGITUDE, 'R', 'NJP', RPO.VISITED, RPO.COMPANY_CODE
UNION ALL
----- NON PJP ORDER DISTRIBUTOR ENTITIES -----
SELECT DPO.SP_CODE, DPO.EMPLOYEE_EDESC, DPO.ENTITY_CODE, DPO.ENTITY_NAME, DPO.LATITUDE, DPO.LONGITUDE, 'D' ENTITY_TYPE, 'NPJP' JOURNEY_PLAN, DPO.VISITED IS_VISITED, DPO.COMPANY_CODE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.CUSTOMER_CODE ENTITY_CODE, TRIM(E.CUSTOMER_EDESC) ENTITY_NAME, D.LATITUDE, D.LONGITUDE, (CASE WHEN F.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END) VISITED, A.COMPANY_CODE
                            FROM DIST_IP_SSD_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.SP_CODE = '{spcode}' AND B.ACTIVE = 'Y'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN DIST_DISTRIBUTOR_MASTER D ON D.DISTRIBUTOR_CODE = A.CUSTOMER_CODE AND D.COMPANY_CODE = A.COMPANY_CODE AND D.ACTIVE = 'Y'
                            INNER JOIN SA_CUSTOMER_SETUP E ON E.CUSTOMER_CODE = D.DISTRIBUTOR_CODE AND E.COMPANY_CODE = D.COMPANY_CODE
                            LEFT JOIN DIST_LOCATION_TRACK F ON F.CUSTOMER_CODE = A.CUSTOMER_CODE AND F.COMPANY_CODE = A.COMPANY_CODE AND F.CUSTOMER_TYPE = 'D' AND TRUNC(F.UPDATE_DATE) = TRUNC(TO_DATE('{date}', 'DD-MON-RRRR'))
                            WHERE TRUNC(A.ORDER_DATE) = TO_DATE('{date}', 'DD-MON-RRRR')
                            GROUP BY A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC), A.CUSTOMER_CODE, TRIM(E.CUSTOMER_EDESC), D.LATITUDE, D.LONGITUDE, (CASE WHEN F.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END), A.COMPANY_CODE
                      ) DPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TRUNC(TO_DATE('{date}', 'DD-MON-RRRR'))
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                                  GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                      ) NPJPENT ON NPJPENT.USERID = DPO.CREATED_BY AND NPJPENT.SP_CODE = DPO.SP_CODE AND NPJPENT.ENTITY_CODE = DPO.ENTITY_CODE AND NPJPENT.COMPANY_CODE = DPO.COMPANY_CODE
                      WHERE 1 = 1
                        AND NPJPENT.SP_CODE IS NULL
                      GROUP BY DPO.SP_CODE, DPO.EMPLOYEE_EDESC, DPO.ENTITY_CODE, DPO.ENTITY_NAME, DPO.LATITUDE, DPO.LONGITUDE, 'D', 'NPJP', DPO.VISITED, DPO.COMPANY_CODE
UNION ALL
----- NON PJP ORDER PARTY TYPE ENTITIES -----
SELECT PPO.SP_CODE, PPO.EMPLOYEE_EDESC, PPO.ENTITY_CODE, PPO.ENTITY_NAME, PPO.LATITUDE, PPO.LONGITUDE, 'P' ENTITY_TYPE, 'NPJP' JOURNEY_PLAN, PPO.VISITED IS_VISITED, PPO.COMPANY_CODE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.PARTY_TYPE_CODE ENTITY_CODE, TRIM(E.PARTY_TYPE_EDESC) ENTITY_NAME, D.LATITUDE, D.LONGITUDE, (CASE WHEN F.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END) VISITED, A.COMPANY_CODE
                            FROM DIST_IP_SSD_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.ACTIVE = 'Y'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN DIST_DEALER_MASTER D ON D.DEALER_CODE = A.PARTY_TYPE_CODE AND D.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN IP_PARTY_TYPE_CODE E ON E.PARTY_TYPE_CODE = D.DEALER_CODE AND E.COMPANY_CODE = D.COMPANY_CODE
                            LEFT JOIN DIST_LOCATION_TRACK F ON F.CUSTOMER_CODE = A.CUSTOMER_CODE AND F.COMPANY_CODE = A.COMPANY_CODE AND F.CUSTOMER_TYPE = 'P' AND TRUNC(F.UPDATE_DATE) = TRUNC(TO_DATE('{date}', 'DD-MON-RRRR'))
                            WHERE TRUNC(A.ORDER_DATE) = TRUNC(TO_DATE('{date}', 'DD-MON-RRRR'))
                            GROUP BY A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC), A.PARTY_TYPE_CODE, TRIM(E.PARTY_TYPE_EDESC), D.LATITUDE, D.LONGITUDE, (CASE WHEN F.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END), A.COMPANY_CODE
                      ) PPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TRUNC(TO_DATE('{date}', 'DD-MON-RRRR'))
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                                  GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                      ) NPJPENT ON NPJPENT.USERID = PPO.CREATED_BY AND NPJPENT.SP_CODE = PPO.SP_CODE AND NPJPENT.ENTITY_CODE = PPO.ENTITY_CODE AND NPJPENT.COMPANY_CODE = PPO.COMPANY_CODE
                      WHERE 1 = 1
                        AND NPJPENT.SP_CODE IS NULL
                      GROUP BY PPO.SP_CODE, PPO.EMPLOYEE_EDESC, PPO.ENTITY_CODE, PPO.ENTITY_NAME, PPO.LATITUDE, PPO.LONGITUDE, 'P', 'NPJP', PPO.VISITED, PPO.COMPANY_CODE";

            var data = _objectEntity.SqlQuery<MrVisitedTrackingModel>(Query).ToList();
            return data;

        }

        public List<MrVisitedTrackingModel> GetMRVisitTracking(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join("','", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            var sp_filter = "";
            if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                sp_filter = $" AND PFMTBL.SP_CODE IN ({userInfo.sp_codes})";

            var query = $@"SELECT PFMTBL.GROUP_EDESC, PFMTBL.SP_CODE, PFMTBL.EMPLOYEE_EDESC,
  TODTBL.TOD_ROUTE_CODE, TODTBL.TOD_ROUTE_NAME, TOMTBL.TOM_ROUTE_CODE, TOMTBL.TOM_ROUTE_NAME,
  PFMTBL.CUR_DATE, PFMTBL.CUR_LATITUDE, PFMTBL.CUR_LONGITUDE,
  PFMTBL.ATN_IMAGE, PFMTBL.ATN_DATE, PFMTBL.ATN_LATITUDE, PFMTBL.ATN_LONGITUDE, PFMTBL.EOD_DATE, PFMTBL.EOD_LATITUDE, PFMTBL.EOD_LONGITUDE,
  SUM(PFMTBL.TARGET) TARGET, SUM(PFMTBL.VISITED) VISITED, SUM(PFMTBL.NOT_VISITED) NOT_VISITED,
  SUM(PFMTBL.PJP_PRODUCTIVE) PJP_PRODUCTIVE,
  SUM(PFMTBL.VISITED) - SUM(PFMTBL.PJP_PRODUCTIVE) PJP_NON_PRODUCTIVE,
  SUM(PFMTBL.NPJP_PRODUCTIVE) NPJP_PRODUCTIVE,
  ROUND(DECODE(SUM(PFMTBL.PJP_PRODUCTIVE), NULL, 0,
        0, 0,
        (DECODE(SUM(PFMTBL.VISITED),0,0,SUM(PFMTBL.PJP_PRODUCTIVE) / SUM(PFMTBL.VISITED))) * 100),2) PERCENT_EFFECTIVE_CALLS,
  SUM(PFMTBL.OUTLET_ADDED) OUTLET_ADDED,
  SUM(PFMTBL.PJP_TOTAL_QUANTITY) PJP_TOTAL_QUANTITY, SUM(PFMTBL.PJP_TOTAL_AMOUNT) PJP_TOTAL_AMOUNT,
  SUM(PFMTBL.NPJP_TOTAL_QUANTITY) NPJP_TOTAL_QUANTITY, SUM(PFMTBL.NPJP_TOTAL_AMOUNT) NPJP_TOTAL_AMOUNT
FROM (SELECT PTBL.*
      FROM (SELECT PETBL.GROUP_EDESC, PETBL.SP_CODE, PETBL.EMPLOYEE_EDESC, PETBL.COMPANY_CODE, PETBL.CUR_DATE, PETBL.CUR_LATITUDE, PETBL.CUR_LONGITUDE, PETBL.ATN_IMAGE, PETBL.ATN_DATE, PETBL.ATN_LATITUDE, PETBL.ATN_LONGITUDE, PETBL.EOD_DATE, PETBL.EOD_LATITUDE, PETBL.EOD_LONGITUDE, NVL(PETBL.TARGET, 0) TARGET, 
            NVL(PVTBL.VISITED, 0) VISITED, NVL(PNVTBL.NOT_VISITED,0) NOT_VISITED, 
            NVL(PPJPTBL.PJP_PRODUCTIVE,0) PJP_PRODUCTIVE,
            (NVL(PVTBL.VISITED, 0) - NVL(PPJPTBL.PJP_PRODUCTIVE,0)) PJP_NON_PRODUCTIVE,
            NVL(PNPJPTBL.NPJP_PRODUCTIVE,0) NPJP_PRODUCTIVE,
            NVL(DECODE(PPJPTBL.PJP_PRODUCTIVE, NULL, 0,
                                             0, 0,
                                             ROUND(DECODE(PVTBL.VISITED,0,0,(PPJPTBL.PJP_PRODUCTIVE / PVTBL.VISITED)) * 100, 2)),0) NET_PERCENT_EFFECTIVE_CALLS,
            0 OUTLET_ADDED,
            NVL(PPJPTBL.PJP_TOTAL_QUANTITY,0) PJP_TOTAL_QUANTITY, NVL(PPJPTBL.PJP_TOTAL_PRICE,0) PJP_TOTAL_AMOUNT,
            NVL(PNPJPTBL.NPJP_TOTAL_QUANTITY,0) NPJP_TOTAL_QUANTITY, NVL(PNPJPTBL.NPJP_TOTAL_PRICE,0) NPJP_TOTAL_AMOUNT
            FROM (SELECT ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, ENT.CUR_DATE, ENT.CUR_LATITUDE, ENT.CUR_LONGITUDE, ENT.ATN_IMAGE, ENT.ATN_DATE, ENT.ATN_LATITUDE, ENT.ATN_LONGITUDE, ENT.EOD_DATE, ENT.EOD_LATITUDE, ENT.EOD_LONGITUDE, COUNT(ENT.ENTITY_CODE) TARGET
                  FROM (
                      SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, TRIM(DGM.GROUP_EDESC) GROUP_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE, LOC.CUR_DATE, LOC.CUR_LATITUDE, LOC.CUR_LONGITUDE, API.ATN_IMAGE, ATN.ATN_DATE, ATN.ATN_LATITUDE, ATN.ATN_LONGITUDE, EOD.EOD_DATE, EOD.EOD_LATITUDE, EOD.EOD_LONGITUDE
                      FROM DIST_LOGIN_USER DLU
                      INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN DIST_GROUP_MASTER DGM ON DGM.GROUPID = DLU.GROUPID AND DGM.COMPANY_CODE = DGM.COMPANY_CODE
                      LEFT JOIN (SELECT A.ENTITY_CODE SP_CODE, A.COMPANY_CODE, A.CREATE_DATE, A.FILENAME ATN_IMAGE
                                  FROM DIST_PHOTO_INFO A
                                  WHERE A.CREATE_DATE = (SELECT MAX(CREATE_DATE) FROM DIST_PHOTO_INFO WHERE ENTITY_CODE = A.ENTITY_CODE AND ENTITY_TYPE = 'S' AND (MEDIA_TYPE = 'ATN' OR CATEGORYID = 1) AND TRUNC(CREATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR'))
                                  GROUP BY A.ENTITY_CODE, A.COMPANY_CODE, A.CREATE_DATE, A.FILENAME
                                  ORDER BY A.ENTITY_CODE DESC
                      ) API ON API.SP_CODE = DLU.SP_CODE AND API.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (SELECT A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE CUR_DATE, A.LATITUDE CUR_LATITUDE, A.LONGITUDE CUR_LONGITUDE
                                  FROM DIST_LM_LOCATION_TRACKING A
                                  WHERE A.SUBMIT_DATE = (SELECT MAX(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND TRACK_TYPE = 'TRK')
                                  GROUP BY A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE, A.LATITUDE, A.LONGITUDE
                                  ORDER BY A.SP_CODE DESC
                      ) LOC ON LOC.SP_CODE = DLU.SP_CODE AND LOC.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (SELECT A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE ATN_DATE, A.LATITUDE ATN_LATITUDE, A.LONGITUDE ATN_LONGITUDE
                                  FROM DIST_LM_LOCATION_TRACKING A
                                  WHERE A.SUBMIT_DATE = (SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND TRACK_TYPE = 'ATN')
                                  GROUP BY A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE, A.LATITUDE, A.LONGITUDE
                                  ORDER BY A.SP_CODE DESC
                      ) ATN ON ATN.SP_CODE = DLU.SP_CODE AND ATN.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (SELECT A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE EOD_DATE, A.LATITUDE EOD_LATITUDE, A.LONGITUDE EOD_LONGITUDE
                                  FROM DIST_LM_LOCATION_TRACKING A
                                  WHERE A.SUBMIT_DATE = (SELECT MAX(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND TRACK_TYPE = 'EOD')
                                  GROUP BY A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE, A.LATITUDE, A.LONGITUDE
                                  ORDER BY A.SP_CODE DESC
                      ) EOD ON EOD.SP_CODE = DLU.SP_CODE AND EOD.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                      LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                      LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                      GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), TRIM(DGM.GROUP_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC), DLU.COMPANY_CODE, LOC.CUR_DATE, LOC.CUR_LATITUDE, LOC.CUR_LONGITUDE, API.ATN_IMAGE, ATN.ATN_DATE, ATN.ATN_LATITUDE, ATN.ATN_LONGITUDE, EOD.EOD_DATE, EOD.EOD_LATITUDE, EOD.EOD_LONGITUDE
                  ) ENT
                  GROUP BY ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, ENT.CUR_DATE, ENT.CUR_LATITUDE, ENT.CUR_LONGITUDE, ENT.ATN_IMAGE, ENT.ATN_DATE, ENT.ATN_LATITUDE, ENT.ATN_LONGITUDE, ENT.EOD_DATE, ENT.EOD_LATITUDE, ENT.EOD_LONGITUDE
                  ) PETBL -- Party Type Entity Table
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) VISITED
                        FROM (
                            SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                            GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                        ) ENT
                        INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                        WHERE DLT.SP_CODE IS NOT NULL
                        GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) PVTBL -- Party Type Visit Table
                  ON PVTBL.SP_CODE = PETBL.SP_CODE AND PVTBL.COMPANY_CODE = PETBL.COMPANY_CODE
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) NOT_VISITED
                      FROM (
                          SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.PARTY_TYPE_CODE ENTITY_CODE, TRIM(SCS.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                          FROM DIST_LOGIN_USER DLU
                          INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                          LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                          LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                          LEFT JOIN IP_PARTY_TYPE_CODE SCS ON SCS.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE 
                          GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.PARTY_TYPE_CODE, TRIM(SCS.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                      ) ENT
                      LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                      WHERE DLT.SP_CODE IS NULL
                      GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) PNVTBL -- Party Type Not Visited Table
                  ON PNVTBL.SP_CODE = PETBL.SP_CODE AND PNVTBL.COMPANY_CODE = PETBL.COMPANY_CODE
            LEFT JOIN (SELECT PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE, COUNT(PPO.PARTY_TYPE_CODE) PJP_PRODUCTIVE, SUM(PPO.TOTAL_QUANTITY) PJP_TOTAL_QUANTITY, SUM(PPO.TOTAL_PRICE) PJP_TOTAL_PRICE
                      FROM (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                            GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                          ) PJPENT
                      LEFT JOIN (SELECT CREATED_BY, PARTY_TYPE_CODE, COMPANY_CODE, COUNT(PARTY_TYPE_CODE) TOTAL_ORDER, SUM(QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE
                                  FROM DIST_IP_SSD_PURCHASE_ORDER
                                  WHERE TRUNC(ORDER_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                                  GROUP BY CREATED_BY, PARTY_TYPE_CODE, COMPANY_CODE
                                ) PPO ON PPO.CREATED_BY = PJPENT.USERID AND PPO.PARTY_TYPE_CODE = PJPENT.ENTITY_CODE AND PPO.COMPANY_CODE = PJPENT.COMPANY_CODE
                      GROUP BY PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE
                   ) PPJPTBL -- Party Type PJP Table
                   ON PPJPTBL.SP_CODE = PETBL.SP_CODE AND PPJPTBL.COMPANY_CODE = PETBL.COMPANY_CODE
            LEFT JOIN (SELECT PPO.SP_CODE, PPO.EMPLOYEE_EDESC, PPO.COMPANY_CODE, COUNT(PPO.PARTY_TYPE_CODE) NPJP_PRODUCTIVE, SUM(PPO.TOTAL_QUANTITY) NPJP_TOTAL_QUANTITY, SUM(PPO.TOTAL_PRICE) NPJP_TOTAL_PRICE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, A.PARTY_TYPE_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.COMPANY_CODE, COUNT(A.PARTY_TYPE_CODE) TOTAL_ORDER, SUM(A.QUANTITY) TOTAL_QUANTITY, SUM(A.TOTAL_PRICE) TOTAL_PRICE
                            FROM DIST_IP_SSD_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN IP_PARTY_TYPE_CODE D ON D.PARTY_TYPE_CODE = A.PARTY_TYPE_CODE AND D.COMPANY_CODE = A.COMPANY_CODE
                            WHERE TRUNC(A.ORDER_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            GROUP BY A.CREATED_BY, B.SP_CODE, A.PARTY_TYPE_CODE, TRIM(C.EMPLOYEE_EDESC), A.COMPANY_CODE
                      ) PPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
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
      FROM (SELECT DETBL.GROUP_EDESC, DETBL.SP_CODE, DETBL.EMPLOYEE_EDESC, DETBL.COMPANY_CODE, DETBL.CUR_DATE, DETBL.CUR_LATITUDE, DETBL.CUR_LONGITUDE, DETBL.ATN_IMAGE, DETBL.ATN_DATE, DETBL.ATN_LATITUDE, DETBL.ATN_LONGITUDE, DETBL.EOD_DATE, DETBL.EOD_LATITUDE, DETBL.EOD_LONGITUDE, NVL(DETBL.TARGET, 0) TARGET, 
            NVL(DVTBL.VISITED, 0) VISITED, NVL(DNVTBL.NOT_VISITED,0) NOT_VISITED, 
            NVL(DPJPTBL.PJP_PRODUCTIVE,0) PJP_PRODUCTIVE,
            (NVL(DVTBL.VISITED, 0) - NVL(DPJPTBL.PJP_PRODUCTIVE,0)) PJP_NON_PRODUCTIVE,
            NVL(DNPJPTBL.NPJP_PRODUCTIVE,0) NPJP_PRODUCTIVE,
            NVL(DECODE(DPJPTBL.PJP_PRODUCTIVE, NULL, 0,
                                             0, 0,
                                             ROUND(DECODE(DVTBL.VISITED,0,0,(DPJPTBL.PJP_PRODUCTIVE / DVTBL.VISITED)) * 100,2)),0) NET_PERCENT_EFFECTIVE_CALLS,
            0 OUTLET_ADDED,
            NVL(DPJPTBL.PJP_TOTAL_QUANTITY,0) PJP_TOTAL_QUANTITY, NVL(DPJPTBL.PJP_TOTAL_PRICE,0) PJP_TOTAL_AMOUNT,
            NVL(DNPJPTBL.NPJP_TOTAL_QUANTITY,0) NPJP_TOTAL_QUANTITY, NVL(DNPJPTBL.NPJP_TOTAL_PRICE,0) NPJP_TOTAL_AMOUNT
            FROM (SELECT ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, ENT.CUR_DATE, ENT.CUR_LATITUDE, ENT.CUR_LONGITUDE, ENT.ATN_IMAGE, ENT.ATN_DATE, ENT.ATN_LATITUDE, ENT.ATN_LONGITUDE, ENT.EOD_DATE, ENT.EOD_LATITUDE, ENT.EOD_LONGITUDE, COUNT(ENT.ENTITY_CODE) TARGET
                  FROM (
                      SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, TRIM(DGM.GROUP_EDESC) GROUP_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE, LOC.CUR_DATE, LOC.CUR_LATITUDE, LOC.CUR_LONGITUDE, API.ATN_IMAGE, ATN.ATN_DATE, ATN.ATN_LATITUDE, ATN.ATN_LONGITUDE, EOD.EOD_DATE, EOD.EOD_LATITUDE, EOD.EOD_LONGITUDE
                      FROM DIST_LOGIN_USER DLU
                      INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN DIST_GROUP_MASTER DGM ON DGM.GROUPID = DLU.GROUPID AND DGM.COMPANY_CODE = DGM.COMPANY_CODE
                      LEFT JOIN (SELECT A.ENTITY_CODE SP_CODE, A.COMPANY_CODE, A.CREATE_DATE, A.FILENAME ATN_IMAGE
                                  FROM DIST_PHOTO_INFO A
                                  WHERE A.CREATE_DATE = (SELECT MAX(CREATE_DATE) FROM DIST_PHOTO_INFO WHERE ENTITY_CODE = A.ENTITY_CODE AND ENTITY_TYPE = 'S' AND (MEDIA_TYPE = 'ATN' OR CATEGORYID = 1) AND TRUNC(CREATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR'))
                                  GROUP BY A.ENTITY_CODE, A.COMPANY_CODE, A.CREATE_DATE, A.FILENAME
                                  ORDER BY A.ENTITY_CODE DESC
                      ) API ON API.SP_CODE = DLU.SP_CODE AND API.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (SELECT A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE CUR_DATE, A.LATITUDE CUR_LATITUDE, A.LONGITUDE CUR_LONGITUDE
                                  FROM DIST_LM_LOCATION_TRACKING A
                                  WHERE A.SUBMIT_DATE = (SELECT MAX(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND TRACK_TYPE = 'TRK')
                                  GROUP BY A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE, A.LATITUDE, A.LONGITUDE
                                  ORDER BY A.SP_CODE DESC
                      ) LOC ON LOC.SP_CODE = DLU.SP_CODE AND LOC.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (SELECT A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE ATN_DATE, A.LATITUDE ATN_LATITUDE, A.LONGITUDE ATN_LONGITUDE
                                  FROM DIST_LM_LOCATION_TRACKING A
                                  WHERE A.SUBMIT_DATE = (SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND TRACK_TYPE = 'ATN')
                                  GROUP BY A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE, A.LATITUDE, A.LONGITUDE
                                  ORDER BY A.SP_CODE DESC
                      ) ATN ON ATN.SP_CODE = DLU.SP_CODE AND ATN.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (SELECT A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE EOD_DATE, A.LATITUDE EOD_LATITUDE, A.LONGITUDE EOD_LONGITUDE
                                  FROM DIST_LM_LOCATION_TRACKING A
                                  WHERE A.SUBMIT_DATE = (SELECT MAX(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND TRACK_TYPE = 'EOD')
                                  GROUP BY A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE, A.LATITUDE, A.LONGITUDE
                                  ORDER BY A.SP_CODE DESC
                      ) EOD ON EOD.SP_CODE = DLU.SP_CODE AND EOD.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                      LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                      LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                      GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), TRIM(DGM.GROUP_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE, LOC.CUR_DATE, LOC.CUR_LATITUDE, LOC.CUR_LONGITUDE, API.ATN_IMAGE, ATN.ATN_DATE, ATN.ATN_LATITUDE, ATN.ATN_LONGITUDE, EOD.EOD_DATE, EOD.EOD_LATITUDE, EOD.EOD_LONGITUDE
                  ) ENT
                  GROUP BY ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, ENT.CUR_DATE, ENT.CUR_LATITUDE, ENT.CUR_LONGITUDE, ENT.ATN_IMAGE, ENT.ATN_DATE, ENT.ATN_LATITUDE, ENT.ATN_LONGITUDE, ENT.EOD_DATE, ENT.EOD_LATITUDE, ENT.EOD_LONGITUDE
                  ) DETBL -- Customer/Distributor Entity Table
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) VISITED
                        FROM (
                            SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                            GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                        ) ENT
                        INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                        WHERE DLT.SP_CODE IS NOT NULL
                        GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) DVTBL -- Customer/Distributor Visit Table
                  ON DVTBL.SP_CODE = DETBL.SP_CODE AND DVTBL.COMPANY_CODE = DETBL.COMPANY_CODE
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) NOT_VISITED
                      FROM (
                          SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                          FROM DIST_LOGIN_USER DLU
                          INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                          LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                          LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                          LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE 
                          GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                      ) ENT
                      LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                      WHERE DLT.SP_CODE IS NULL
                      GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                ) DNVTBL -- Customer/Distributor Not Visited Table
                ON DNVTBL.SP_CODE = DETBL.SP_CODE AND DNVTBL.COMPANY_CODE = DETBL.COMPANY_CODE
            LEFT JOIN (SELECT PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE, COUNT(DPO.CUSTOMER_CODE) PJP_PRODUCTIVE, SUM(DPO.TOTAL_QUANTITY) PJP_TOTAL_QUANTITY, SUM(DPO.TOTAL_PRICE) PJP_TOTAL_PRICE
                      FROM (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                            GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                      ) PJPENT
                      LEFT JOIN (SELECT CREATED_BY, CUSTOMER_CODE, COMPANY_CODE, COUNT(CUSTOMER_CODE) TOTAL_ORDER, SUM(QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE
                                  FROM DIST_IP_SSD_PURCHASE_ORDER
                                  WHERE TRUNC(ORDER_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                                  GROUP BY CREATED_BY, CUSTOMER_CODE, COMPANY_CODE
                      ) DPO ON DPO.CREATED_BY = PJPENT.USERID AND DPO.CUSTOMER_CODE = PJPENT.ENTITY_CODE AND DPO.COMPANY_CODE = PJPENT.COMPANY_CODE
                      GROUP BY PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE
                   ) DPJPTBL -- Customer/Distributor PJP Table
                   ON DPJPTBL.SP_CODE = DETBL.SP_CODE AND DPJPTBL.COMPANY_CODE = DETBL.COMPANY_CODE
            LEFT JOIN (SELECT DPO.SP_CODE, DPO.EMPLOYEE_EDESC, DPO.COMPANY_CODE, COUNT(DPO.CUSTOMER_CODE) NPJP_PRODUCTIVE, SUM(DPO.TOTAL_QUANTITY) NPJP_TOTAL_QUANTITY, SUM(DPO.TOTAL_PRICE) NPJP_TOTAL_PRICE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, A.CUSTOMER_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.COMPANY_CODE, COUNT(A.CUSTOMER_CODE) TOTAL_ORDER, SUM(A.QUANTITY) TOTAL_QUANTITY, SUM(A.TOTAL_PRICE) TOTAL_PRICE
                            FROM DIST_IP_SSD_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            WHERE TRUNC(A.ORDER_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            GROUP BY A.CREATED_BY, B.SP_CODE, A.CUSTOMER_CODE, TRIM(C.EMPLOYEE_EDESC), A.COMPANY_CODE
                      ) DPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
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
      FROM (SELECT RETBL.GROUP_EDESC, RETBL.SP_CODE, RETBL.EMPLOYEE_EDESC, RETBL.COMPANY_CODE, RETBL.CUR_DATE, RETBL.CUR_LATITUDE, RETBL.CUR_LONGITUDE, RETBL.ATN_IMAGE, RETBL.ATN_DATE, RETBL.ATN_LATITUDE, RETBL.ATN_LONGITUDE, RETBL.EOD_DATE, RETBL.EOD_LATITUDE, RETBL.EOD_LONGITUDE, NVL(RETBL.TARGET, 0) TARGET, 
            NVL(RVTBL.VISITED, 0) VISITED, NVL(RNVTBL.NOT_VISITED,0) NOT_VISITED, 
            NVL(RPJPTBL.PJP_PRODUCTIVE,0) PJP_PRODUCTIVE,
            (NVL(RVTBL.VISITED, 0) - NVL(RPJPTBL.PJP_PRODUCTIVE,0)) PJP_NON_PRODUCTIVE,
            NVL(RNPJPTBL.NPJP_PRODUCTIVE,0) NPJP_PRODUCTIVE,
            NVL(DECODE(RPJPTBL.PJP_PRODUCTIVE, NULL, 0,
                                             0, 0,
                                             ROUND(DECODE(RVTBL.VISITED,0,0,(RPJPTBL.PJP_PRODUCTIVE / RVTBL.VISITED)) * 100,2)),0) NET_PERCENT_EFFECTIVE_CALLS,
            NVL(ROUT.OUTLET_ADDED, 0) OUTLET_ADDED,
            NVL(RPJPTBL.PJP_TOTAL_QUANTITY,0) PJP_TOTAL_QUANTITY, NVL(RPJPTBL.PJP_TOTAL_PRICE,0) PJP_TOTAL_AMOUNT,
            NVL(RNPJPTBL.NPJP_TOTAL_QUANTITY,0) NPJP_TOTAL_QUANTITY, NVL(RNPJPTBL.NPJP_TOTAL_PRICE,0) NPJP_TOTAL_AMOUNT
            FROM (SELECT ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, ENT.CUR_DATE, ENT.CUR_LATITUDE, ENT.CUR_LONGITUDE, ENT.ATN_IMAGE, ENT.ATN_DATE, ENT.ATN_LATITUDE, ENT.ATN_LONGITUDE, ENT.EOD_DATE, ENT.EOD_LATITUDE, ENT.EOD_LONGITUDE, COUNT(ENT.ENTITY_CODE) TARGET
                  FROM (
                      SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, TRIM(DGM.GROUP_EDESC) GROUP_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE, LOC.CUR_DATE, LOC.CUR_LATITUDE, LOC.CUR_LONGITUDE, API.ATN_IMAGE, ATN.ATN_DATE, ATN.ATN_LATITUDE, ATN.ATN_LONGITUDE, EOD.EOD_DATE, EOD.EOD_LATITUDE, EOD.EOD_LONGITUDE
                      FROM DIST_LOGIN_USER DLU
                      INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN DIST_GROUP_MASTER DGM ON DGM.GROUPID = DLU.GROUPID AND DGM.COMPANY_CODE = DGM.COMPANY_CODE
                      LEFT JOIN (SELECT A.ENTITY_CODE SP_CODE, A.COMPANY_CODE, A.CREATE_DATE, A.FILENAME ATN_IMAGE
                                  FROM DIST_PHOTO_INFO A
                                  WHERE A.CREATE_DATE = (SELECT MAX(CREATE_DATE) FROM DIST_PHOTO_INFO WHERE ENTITY_CODE = A.ENTITY_CODE AND ENTITY_TYPE = 'S' AND (MEDIA_TYPE = 'ATN' OR CATEGORYID = 1) AND TRUNC(CREATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR'))
                                  GROUP BY A.ENTITY_CODE, A.COMPANY_CODE, A.CREATE_DATE, A.FILENAME
                                  ORDER BY A.ENTITY_CODE DESC
                      ) API ON API.SP_CODE = DLU.SP_CODE AND API.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (SELECT A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE CUR_DATE, A.LATITUDE CUR_LATITUDE, A.LONGITUDE CUR_LONGITUDE
                                  FROM DIST_LM_LOCATION_TRACKING A
                                  WHERE A.SUBMIT_DATE = (SELECT MAX(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND TRACK_TYPE = 'TRK')
                                  GROUP BY A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE, A.LATITUDE, A.LONGITUDE
                                  ORDER BY A.SP_CODE DESC
                      ) LOC ON LOC.SP_CODE = DLU.SP_CODE AND LOC.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (SELECT A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE ATN_DATE, A.LATITUDE ATN_LATITUDE, A.LONGITUDE ATN_LONGITUDE
                                  FROM DIST_LM_LOCATION_TRACKING A
                                  WHERE A.SUBMIT_DATE = (SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND TRACK_TYPE = 'ATN')
                                  GROUP BY A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE, A.LATITUDE, A.LONGITUDE
                                  ORDER BY A.SP_CODE DESC
                      ) ATN ON ATN.SP_CODE = DLU.SP_CODE AND ATN.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (SELECT A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE EOD_DATE, A.LATITUDE EOD_LATITUDE, A.LONGITUDE EOD_LONGITUDE
                                  FROM DIST_LM_LOCATION_TRACKING A
                                  WHERE A.SUBMIT_DATE = (SELECT MAX(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND TRACK_TYPE = 'EOD')
                                  GROUP BY A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE, A.LATITUDE, A.LONGITUDE
                                  ORDER BY A.SP_CODE DESC
                      ) EOD ON EOD.SP_CODE = DLU.SP_CODE AND EOD.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                      LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                      LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y'
                      GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), TRIM(DGM.GROUP_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE, LOC.CUR_DATE, LOC.CUR_LATITUDE, LOC.CUR_LONGITUDE, API.ATN_IMAGE, ATN.ATN_DATE, ATN.ATN_LATITUDE, ATN.ATN_LONGITUDE, EOD.EOD_DATE, EOD.EOD_LATITUDE, EOD.EOD_LONGITUDE
                  ) ENT
                  GROUP BY ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, ENT.CUR_DATE, ENT.CUR_LATITUDE, ENT.CUR_LONGITUDE, ENT.ATN_IMAGE, ENT.ATN_DATE, ENT.ATN_LATITUDE, ENT.ATN_LONGITUDE, ENT.EOD_DATE, ENT.EOD_LATITUDE, ENT.EOD_LONGITUDE
                  ) RETBL -- Retailer Entity Table
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) VISITED
                        FROM (
                            SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y'
                            GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                        ) ENT
                        INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                        WHERE DLT.SP_CODE IS NOT NULL
                        GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) RVTBL -- Retailer Visit Table
                  ON RVTBL.SP_CODE = RETBL.SP_CODE AND RVTBL.COMPANY_CODE = RETBL.COMPANY_CODE
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) NOT_VISITED
                      FROM (
                          SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                          FROM DIST_LOGIN_USER DLU
                          INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                          LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                          LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                          LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y'
                          GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                      ) ENT
                      LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                      WHERE DLT.SP_CODE IS NULL
                      GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                ) RNVTBL -- Retailer Not Visited Table
                ON RNVTBL.SP_CODE = RETBL.SP_CODE AND RNVTBL.COMPANY_CODE = RETBL.COMPANY_CODE
            LEFT JOIN (SELECT PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE, COUNT(RPO.RESELLER_CODE) PJP_PRODUCTIVE, SUM(RPO.TOTAL_QUANTITY) PJP_TOTAL_QUANTITY, SUM(RPO.TOTAL_PRICE) PJP_TOTAL_PRICE
                      FROM (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y'
                            GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                      ) PJPENT
                      LEFT JOIN (SELECT CREATED_BY, RESELLER_CODE, COMPANY_CODE, COUNT(RESELLER_CODE) TOTAL_ORDER, SUM(QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE
                                  FROM DIST_IP_SSR_PURCHASE_ORDER
                                  WHERE TRUNC(ORDER_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                                  GROUP BY CREATED_BY, RESELLER_CODE, COMPANY_CODE
                      ) RPO ON RPO.CREATED_BY = PJPENT.USERID AND RPO.RESELLER_CODE = PJPENT.ENTITY_CODE AND RPO.COMPANY_CODE = PJPENT.COMPANY_CODE
                      GROUP BY PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE
                   ) RPJPTBL -- Retailer PJP Table
                   ON RPJPTBL.SP_CODE = RETBL.SP_CODE AND RPJPTBL.COMPANY_CODE = RETBL.COMPANY_CODE
            LEFT JOIN (SELECT RPO.SP_CODE, RPO.EMPLOYEE_EDESC, RPO.COMPANY_CODE, COUNT(RPO.RESELLER_CODE) NPJP_PRODUCTIVE, SUM(RPO.TOTAL_QUANTITY) NPJP_TOTAL_QUANTITY, SUM(RPO.TOTAL_PRICE) NPJP_TOTAL_PRICE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, A.RESELLER_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.COMPANY_CODE, COUNT(A.RESELLER_CODE) TOTAL_ORDER, SUM(A.QUANTITY) TOTAL_QUANTITY, SUM(A.TOTAL_PRICE) TOTAL_PRICE
                            FROM DIST_IP_SSR_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            WHERE TRUNC(A.ORDER_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            GROUP BY A.CREATED_BY, B.SP_CODE, A.RESELLER_CODE, TRIM(C.EMPLOYEE_EDESC), A.COMPANY_CODE
                      ) RPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
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
                          AND DRM.CREATED_DATE = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
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
                    AND DRM.ROUTE_TYPE = 'D'
                    AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') 
                    AND DRD.COMPANY_CODE IN('{companyCode}')
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
                    AND DRM.ROUTE_TYPE = 'D'
                    AND TRUNC(DRD.ASSIGN_DATE) = (TO_DATE('{model.ToDate}', 'DD-MON-RRRR') + 1)
                    AND DRD.COMPANY_CODE IN('{companyCode}')
                  GROUP BY DRD.EMP_CODE, DRM.ROUTE_CODE, DRM.ROUTE_NAME, DRM.COMPANY_CODE
                  ORDER BY UPPER(TRIM(DRM.ROUTE_NAME))
            ) TOM
            GROUP BY TOM.SP_CODE
) TOMTBL ON TOMTBL.SP_CODE = PFMTBL.SP_CODE
WHERE 1 = 1 {sp_filter}
  AND PFMTBL.COMPANY_CODE IN('{companyCode}')
GROUP BY PFMTBL.GROUP_EDESC, PFMTBL.SP_CODE, PFMTBL.EMPLOYEE_EDESC, 
  TODTBL.TOD_ROUTE_CODE, TODTBL.TOD_ROUTE_NAME, TOMTBL.TOM_ROUTE_CODE, TOMTBL.TOM_ROUTE_NAME, 
  PFMTBL.CUR_DATE, PFMTBL.CUR_LATITUDE, PFMTBL.CUR_LONGITUDE,
  PFMTBL.ATN_IMAGE, PFMTBL.ATN_DATE, PFMTBL.ATN_LATITUDE, PFMTBL.ATN_LONGITUDE, PFMTBL.EOD_DATE, PFMTBL.EOD_LATITUDE, PFMTBL.EOD_LONGITUDE
ORDER BY UPPER(PFMTBL.GROUP_EDESC), UPPER(PFMTBL.EMPLOYEE_EDESC)";

            //var Query = $@" SELECT ROUTE_NAME, GROUP_EDESC,SP_CODE, EMPLOYEE_EDESC,ATN_IMAGE, ATN_TIME,ATN_LATITUDE,ATN_LONGITUDE,EOD_TIME,EOD_LATITUDE,EOD_LONGITUDE,WORKING_HOURS,
            //        TARGET,VISITED TARGET_VISITED,TOTAL_VISITED VISITED,EXTRA NPJP_PRODUCTIVE,
            //        NOT_VISITED,TOTAL_PJP PJP_PRODUCTIVE,PJP,NON_PJP PJP_NON_PRODUCTIVE,
            //        NON_N_PJP,TOTAL_QUANTITY PJP_TOTAL_QUANTITY,TOTAL_PRICE PJP_TOTAL_AMOUNT,
            //        ROUND( (TOTAL_VISITED/DECODE(TARGET,0,1,TARGET)  * 100),2)  PERCENT_EFFECTIVE_CALLS,
            //        ROUND( (TOTAL_PJP/DECODE(TOTAL_VISITED,0,1,TOTAL_VISITED) * 100),2)  PERCENT_PRODUCTIVE_CALLS,
            //        EOD_REMARKS
            //        FROM(
            //        SELECT WM_CONCAT(DISTINCT ROUTE_NAME) ROUTE_NAME,GROUP_EDESC,SP_CODE, FULL_NAME EMPLOYEE_EDESC,
            //        TO_CHAR(ATN_TIME,'HH:MI:SS A.M.') ATN_TIME,
            //        ATN_IMAGE,ATN_LATITUDE,ATN_LONGITUDE,EOD_LATITUDE,EOD_LONGITUDE,
            //        CASE WHEN ATN_TIME = EOD_TIME THEN NULL
            //                ELSE TO_CHAR(EOD_TIME,'HH:MI:SS A.M.')
            //        END EOD_TIME,
            //        --TO_CHAR(EOD_TIME,'HH:MI:SS A.M.') EOD_TIME,
            //        NVL(ROUND(24 * (EOD_TIME - ATN_TIME),2),0) WORKING_HOURS,
            //        SUM(TARGET) TARGET,
            //        SUM(VISITED) VISITED,SUM(TOTAL_VISITED) TOTAL_VISITED, SUM(TOTAL_VISITED - VISITED) EXTRA, SUM(TARGET- VISITED) NOT_VISITED,
            //        SUM(TOTAL_PJP) TOTAL_PJP,SUM(PJP) PJP, SUM(VISITED - PJP)  NON_PJP, SUM(TOTAL_PJP- PJP) NON_N_PJP, SUM(TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE,
            //        EOD_REMARKS
            //        FROM(
            //        SELECT WM_CONCAT(DISTINCT B.ROUTE_NAME) ROUTE_NAME, WM_CONCAT(DISTINCT B.GROUP_EDESC) GROUP_EDESC, A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE
            //        ,(SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='ATN' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')) ATN_TIME
            //        ,(SELECT LATITUDE as ATN_LATITUDE FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='ATN' AND SP_CODE = A.SP_CODE AND SUBMIT_DATE =(SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='ATN' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR'))) ATN_LATITUDE
            //        ,(SELECT LONGITUDE as ATN_LONGITUDE FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='ATN' AND SP_CODE = A.SP_CODE AND SUBMIT_DATE =(SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='ATN' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR'))) ATN_LONGITUDE
            //        ,(SELECT LATITUDE  FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='EOD' AND SP_CODE = A.SP_CODE AND SUBMIT_DATE =(SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='EOD' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR'))) EOD_LATITUDE
            //         ,(SELECT LONGITUDE  FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='EOD' AND SP_CODE = A.SP_CODE AND SUBMIT_DATE =(SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='EOD' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR'))) EOD_LONGITUDE
            //         ,(SELECT  A.FILENAME ATN_IMAGE
            //                                  FROM DIST_PHOTO_INFO A
            //                                  WHERE A.CREATE_DATE = (SELECT MIN(CREATE_DATE) FROM DIST_PHOTO_INFO WHERE ENTITY_CODE = A.SP_CODE AND ENTITY_TYPE = 'S' AND (MEDIA_TYPE = 'ATN' OR CATEGORYID = 1) AND TRUNC(CREATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR'))) ATN_IMAGE
            //        --,(SELECT MAX(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='EOD' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')) EOD_TIME
            //        ,(SELECT TO_DATE(TO_CHAR(MAX(ATTENDANCE_TIME),'DD/MM/YYYY HH:MI:SS AM'),'DD/MM/YYYY HH:MI:SS AM') FROM HRIS_ATTENDANCE WHERE EMPLOYEE_ID = A.SP_CODE AND TRUNC(ATTENDANCE_DT) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')) EOD_TIME
            //        ,(SELECT REMARKS FROM DIST_EOD_UPDATE WHERE TRUNC(CREATED_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND SP_CODE =  A.SP_CODE AND ROWNUM = 1) EOD_REMARKS
            //        ,CASE WHEN WM_CONCAT(B.ENTITY_CODE) IS NULL THEN 0
            //            ELSE NVL(COUNT(*),0)
            //            END TARGET
            //        ,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND CUSTOMER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR'))),0) VISITED
            //        ,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')),0) TOTAL_VISITED
            //        ,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND RESELLER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR'))),0) PJP
            //        ,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')),0) TOTAL_PJP
            //        ,NVL((SELECT SUM(QUANTITY)  FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')),0) TOTAL_QUANTITY
            //        ,NVL((SELECT SUM(TOTAL_PRICE)  FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')),0) TOTAL_PRICE
            //        FROM DIST_LOGIN_USER A, DIST_TARGET_ENTITY B  
            //        WHERE A.USERID = B.USERID
            //        AND A.COMPANY_CODE = B.COMPANY_CODE
            //        AND A.ACTIVE = 'Y'
            //        AND A.COMPANY_CODE IN ('{companyCode}')
            //        AND B.ASSIGN_DATE = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
            //        GROUP BY A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE, A.COMPANY_CODE
            //        ORDER BY B.ASSIGN_DATE)
            //        WHERE 1=1  {sp_filter}
            //        GROUP BY ATN_TIME,ATN_LATITUDE,ATN_LONGITUDE,EOD_LATITUDE,EOD_LONGITUDE,EOD_TIME, SP_CODE,GROUP_EDESC,SP_CODE, FULL_NAME,EOD_REMARKS,ATN_IMAGE)";

            // Rabin Query

            //var query1 = $@"SELECT ROUTE_NAME, GROUP_EDESC,SP_CODE, EMPLOYEE_EDESC, ATN_TIME,EOD_TIME,WORKING_HOURS,
            //        TARGET,VISITED TARGET_VISITED,TOTAL_VISITED VISITED,EXTRA NPJP_PRODUCTIVE,
            //        NOT_VISITED,TOTAL_PJP PJP_PRODUCTIVE,PJP,NON_PJP PJP_NON_PRODUCTIVE,
            //        NON_N_PJP,TOTAL_QUANTITY PJP_TOTAL_QUANTITY,TOTAL_PRICE PJP_TOTAL_AMOUNT,
            //        ROUND( (TOTAL_VISITED/DECODE(TARGET,0,1,TARGET)  * 100),2)  PERCENT_EFFECTIVE_CALLS,
            //        ROUND( (TOTAL_PJP/DECODE(TOTAL_VISITED,0,1,TOTAL_VISITED) * 100),2)  PERCENT_PRODUCTIVE_CALLS,
            //        EOD_REMARKS
            //        FROM(
            //        SELECT WM_CONCAT(DISTINCT ROUTE_NAME) ROUTE_NAME,GROUP_EDESC,SP_CODE, FULL_NAME EMPLOYEE_EDESC,
            //        TO_CHAR(ATN_TIME,'HH:MI:SS A.M.') ATN_TIME,
            //        CASE WHEN ATN_TIME = EOD_TIME THEN NULL
            //                ELSE TO_CHAR(EOD_TIME,'HH:MI:SS A.M.')
            //        END EOD_TIME,
            //        --TO_CHAR(EOD_TIME,'HH:MI:SS A.M.') EOD_TIME,
            //        NVL(ROUND(24 * (EOD_TIME - ATN_TIME),2),0) WORKING_HOURS,
            //        SUM(TARGET) TARGET,
            //        SUM(VISITED) VISITED,SUM(TOTAL_VISITED) TOTAL_VISITED, SUM(TOTAL_VISITED - VISITED) EXTRA, SUM(TARGET- VISITED) NOT_VISITED,
            //        SUM(TOTAL_PJP) TOTAL_PJP,SUM(PJP) PJP, SUM(VISITED - PJP)  NON_PJP, SUM(TOTAL_PJP- PJP) NON_N_PJP, SUM(TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE,
            //        EOD_REMARKS
            //        FROM(
            //        SELECT WM_CONCAT(DISTINCT B.ROUTE_NAME) ROUTE_NAME, WM_CONCAT(DISTINCT B.GROUP_EDESC) GROUP_EDESC, A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE
            //        ,(SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='ATN' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')) ATN_TIME
            //        --,(SELECT MAX(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='EOD' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')) EOD_TIME
            //        ,(SELECT TO_DATE(TO_CHAR(MAX(ATTENDANCE_TIME),'DD/MM/YYYY HH:MI:SS AM'),'DD/MM/YYYY HH:MI:SS AM') FROM HRIS_ATTENDANCE WHERE EMPLOYEE_ID = A.SP_CODE AND TRUNC(ATTENDANCE_DT) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')) EOD_TIME
            //        ,(SELECT REMARKS FROM DIST_EOD_UPDATE WHERE TRUNC(CREATED_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND SP_CODE =  A.SP_CODE AND ROWNUM = 1) EOD_REMARKS
            //        ,CASE WHEN WM_CONCAT(B.ENTITY_CODE) IS NULL THEN 0
            //            ELSE NVL(COUNT(*),0)
            //            END TARGET
            //        ,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND CUSTOMER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR'))),0) VISITED
            //        ,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')),0) TOTAL_VISITED
            //        ,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND RESELLER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR'))),0) PJP
            //        ,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')),0) TOTAL_PJP
            //        ,NVL((SELECT SUM(QUANTITY)  FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')),0) TOTAL_QUANTITY
            //        ,NVL((SELECT SUM(TOTAL_PRICE)  FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')),0) TOTAL_PRICE
            //        FROM DIST_LOGIN_USER A, DIST_TARGET_ENTITY B  
            //        WHERE A.USERID = B.USERID
            //        AND A.COMPANY_CODE = B.COMPANY_CODE
            //        AND A.ACTIVE = 'Y'
            //        AND A.COMPANY_CODE IN ('{companyCode}')
            //        AND B.ASSIGN_DATE = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
            //        GROUP BY A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE, A.COMPANY_CODE
            //        ORDER BY B.ASSIGN_DATE)
            //        WHERE 1=1 {sp_filter}
            //        GROUP BY ATN_TIME,EOD_TIME, SP_CODE,GROUP_EDESC,SP_CODE, FULL_NAME,EOD_REMARKS)";

            var data = _objectEntity.SqlQuery<MrVisitedTrackingModel>(query).ToList();
            return data;
        }

        public List<SalesDistrictModel> GetSalesByDistrictAndAreaBoundary(string CompanyCode = "01")
        {
            var query = @"select x.customer_code,
                      sum(x.calc_total_price) as sales,
                      (select distinct a.district_name
                                  from dist_address_master a, dist_distributor_master b, dist_area_master c
                                  where a.district_code = c.district_code
                                  and b.area_code = c.area_code
                                  and b.company_code = c.company_code
                                  and b.company_code = '" + CompanyCode + @"'
                                  and b.distributor_code = x.customer_code) as district
                from sa_sales_invoice x, dist_distributor_master y
                where x.customer_code = y.distributor_code
                and x.company_code = y.company_code
                and x.company_code = '" + CompanyCode + @"'
                group by customer_code";
            var data = _objectEntity.SqlQuery<SalesDistrictModel>(query).ToList();
            return data;
        }

        public List<AreaBoundaryViewModel> GetSalesAreaBoundary(string CompanyCode = "01")
        {
            var query = @"SELECT A.*, D.DISTRICT_NAME,
                  NVL(AB.AREA_PATH, ' ') AREA_PATH, AB.REMARKS, AB.CREATED_BY, AB.CREATED_DATE, AB.MODIFY_BY, AB.MODIFY_DATE, AB.DELETED_FLAG
                FROM DIST_AREA_MASTER A
                LEFT JOIN DIST_AREA_BOUNDARY_SETUP AB ON AB.AREA_CODE = A.AREA_CODE
                INNER JOIN (SELECT DISTRICT_CODE, DISTRICT_NAME FROM DIST_ADDRESS_MASTER GROUP BY DISTRICT_CODE, DISTRICT_NAME) D ON D.DISTRICT_CODE = A.DISTRICT_CODE
                WHERE A.COMPANY_CODE = '" + CompanyCode + @"'
                ORDER BY A.AREA_CODE ASC";
            var data = _objectEntity.SqlQuery<AreaBoundaryViewModel>(query).ToList();
            return data;

        }


        public List<PurchaseOrderModel> GetPurchaseOrder(ReportFiltersModel model, User userInfo, string requestStatus)
        {
            var companyCode = model.CompanyFilter.Count > 0 ? "'" + string.Join("','", model.CompanyFilter) + "'" : userInfo.company_code;
            var PO = _objectEntity.SqlQuery<PreferenceSetupModel>($"SELECT * FROM DIST_PREFERENCE_SETUP WHERE COMPANY_CODE='{userInfo.company_code}'").FirstOrDefault();
            var flagFilter = "";
            if (requestStatus == "Approved")
                flagFilter = @" --AND DPO.APPROVED_FLAG = 'Y'
                                AND DPO.APPROVE_AMT<>0  
                               AND DPO.REJECT_FLAG = 'N'";
            else if (requestStatus == "Rejected")
                flagFilter = @" AND DPO.REJECT_FLAG = 'Y'
                                AND DPO.APPROVED_FLAG = 'N'";
            else if (requestStatus == "All")
            {
                flagFilter = "";
            }
            else if (requestStatus == "Active")
            {
                flagFilter = @" AND DPO.REJECT_FLAG = 'N'
                                AND DPO.QUANTITY <>0
                                AND DPO.APPROVED_FLAG = 'N'";
            }
            string brachFilter1 = "";
            string brachFilter2 = "";
            if (model.BranchFilter.Count() > 0)
            {
                brachFilter1 = " and DPO.BRANCH_CODE IN('" + string.Join("', '", model.BranchFilter) + "')";
                brachFilter2 = " and VSL.BRANCH_CODE IN('" + string.Join("', '", model.BranchFilter) + "')";
            }

            var customerFilter = string.Empty;
            if (model.CustomerFilter.Count() > 0)
            {
                customerFilter = @"select  DISTINCT(customer_code) from sa_customer_setup where (";
                //IF CUSTOMER_SKU_FLAG = G
                foreach (var item in model.CustomerFilter)
                {
                    customerFilter += "master_customer_code like  (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + companyCode + ")) OR ";
                }
                customerFilter = customerFilter.Substring(0, customerFilter.Length - 3);
                //IF CUSTOMER_SKU_FLAG = I                
                customerFilter += " or (customer_code in (" + string.Join(",", model.CustomerFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + companyCode + "))) ";


                customerFilter = " and CS.customer_code IN(" + customerFilter + ")";
            }
            var SalesPersonFilter = string.Empty;
            if (model.ItemBrandFilter.Count > 0)
                SalesPersonFilter = $" AND  ES.EMPLOYEE_CODE IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                SalesPersonFilter = $" AND ES.EMPLOYEE_CODE IN  ({userInfo.sp_codes})";


            string query = string.Empty;

            if (PO.SO_REPO_RATE_TABLE == "DIST_IP_SSD_PURCHASE_ORDER" && PO.SO_REPO_RATE_COLUMN == "UNIT_PRICE")
            {
                query = $@"SELECT X.ORDER_NO
,X.COMPANY_CODE
,X.COMPANY_EDESC
,x.GROUP_EDESC
,X.CUSTOMER_CODE
,X.CUSTOMER_EDESC
,X.EMPLOYEE_EDESC
,X.REMARKS
,X.RESELLER_NAME
,X.ORDER_ENTITY
,X.MITI
,X.PARTY_TYPE_CODE
,X.PARTY_TYPE_EDESC
,X.CREATED_BY
,X.CREATED_DATE
,X.DELETED_FLAG
,X.BRANCH_CODE
,X.APPROVED_FLAG
,X.DISPATCH_FLAG
,X.ACKNOWLEDGE_FLAG
,X.REJECT_FLAG
,X.PO_PARTY_TYPE
,X.PO_CONVERSION_UNIT
,X.PO_CONVERSION_FACTOR
,X.SO_CREDIT_LIMIT_FLAG
,X.SO_CREDIT_DAYS_FLAG
,X.PO_CUSTOM_RATE
,X.CREDIT_LIMIT
-- ,X.TOTAL_QUANTITY
,SUM(NVL(X.TOTAL_QUANTITY,0))TOTAL_QUANTITY
,SUM(NVL(GrantTotalAmount,0))GrantTotalAmount
,SUM(NVL(GRAND_APPROVE_QUENTITY,0))GRAND_APPROVE_QUENTITY
,SUM(NVL(TOTAL_APPROVE_AMT,0))TOTAL_APPROVE_AMT
--,SUM(NVL(X.TOTAL_QUANTITY,0))TOTAL_QUANTITY
,SUM(NVL(X.NEW_TOTAL_AMOUNT, 0)) NEW_TOTAL_AMOUNT
,SUM(NVL(X.NEW_TOTAL_AMOUNT1, 0)) NEW_TOTAL_AMOUNT1
,SUM(NVL(X.APPROVE_AMT, 0)) APPROVE_AMT
FROM (
SELECT ORDER_NO
,ORDER_DATE
,CUSTOMER_CODE
,CUSTOMER_EDESC
,'' RESELLER_NAME
,'P-D' ORDER_ENTITY
,MITI
,PARTY_TYPE_CODE
,REMARKS
,(
CASE
WHEN PARTY_TYPE_CODE IS NULL
THEN FN_FETCH_DESC(cpp, 'IP_PARTY_TYPE_CODE', CS_PARTY)
ELSE FN_FETCH_DESC(bpp, 'IP_PARTY_TYPE_CODE', PARTY_TYPE_CODE)
END
) PARTY_TYPE_EDESC
,CREATED_BY
,CREATED_DATE
,DELETED_FLAG
,cpp COMPANY_CODE
,COMPANY_EDESC
,bpp BRANCH_CODE
,APPROVED_FLAG
,DISPATCH_FLAG
,ACKNOWLEDGE_FLAG
,REJECT_FLAG
,EMPLOYEE_EDESC
,PO_PARTY_TYPE
,PO_CONVERSION_UNIT
,PO_CONVERSION_FACTOR
,SO_CREDIT_LIMIT_FLAG
,SO_CREDIT_DAYS_FLAG
,PO_CUSTOM_RATE
,NVL(CREDIT_LIMIT, 0) CREDIT_LIMIT
,NVL(QUANTITY, 0) TOTAL_QUANTITY
,--duplicate
TOTAL_AMOUNT GrantTotalAmount
,--duplicate
NVL(TOTAL_APPROVE_QTY, 0) GRAND_APPROVE_QUENTITY
,NVL(TOTAL_APPROVE_AMT, 0) TOTAL_APPROVE_AMT
,GROUP_NAME GROUP_EDESC
,SUM(SALES_RATE * QUANTITY) NEW_TOTAL_AMOUNT
,SUM(UNIT_PRICE * QUANTITY) NEW_TOTAL_AMOUNT1
,SUM(APPROVE_AMT) APPROVE_AMT
FROM (
SELECT *
FROM (
SELECT ORDER_NO
,ORDER_DATE
,TO_BS(ORDER_DATE) MITI
,DPO.CUSTOMER_CODE
,CS.CUSTOMER_EDESC
,GROUP_EDESC GROUP_NAME
,DPO.REJECT_FLAG
,DPO.DELETED_FLAG
,DPO.APPROVED_FLAG
,DPO.DISPATCH_FLAG
,DPO.ACKNOWLEDGE_FLAG
,DPO.PARTY_TYPE_CODE
,CS.PARTY_TYPE_CODE CS_PARTY
,CS.CREDIT_LIMIT
,DPO.TOTAL_PRICE TOTAL_AMOUNT
,DPO.APPROVE_QTY TOTAL_APPROVE_QTY
,DPO.APPROVE_AMT TOTAL_APPROVE_AMT
,DPO.CREATED_BY
,DPO.CREATED_DATE
,ES.EMPLOYEE_EDESC
,APPROVE_AMT
,ITEM_CODE
,DPO.COMPANY_CODE cpp
,DPO.BRANCH_CODE bpp
,DPO.QUANTITY
,DPO.UNIT_PRICE
,DPO.REMARKS
,CST.COMPANY_EDESC
,DPS.PO_PARTY_TYPE
,DPS.PO_CONVERSION_UNIT
,DPS.PO_CONVERSION_FACTOR
,DPS.SO_CREDIT_LIMIT_CHK SO_CREDIT_LIMIT_FLAG
,DPS.SO_CREDIT_DAYS_CHK SO_CREDIT_DAYS_FLAG
,DPS.PO_CUSTOM_RATE PO_CUSTOM_RATE
FROM DIST_IP_SSD_PURCHASE_ORDER DPO
,SA_CUSTOMER_SETUP CS
,DIST_DISTRIBUTOR_MASTER DDM
,DIST_GROUP_MASTER GM
,HR_EMPLOYEE_SETUP ES
,DIST_LOGIN_USER LU
,COMPANY_SETUP CST
,DIST_PREFERENCE_SETUP DPS
WHERE TRIM(DPO.CUSTOMER_CODE) = TRIM(CS.CUSTOMER_CODE(+))
AND DPO.COMPANY_CODE = CS.COMPANY_CODE(+)
AND TRIM(DPO.CUSTOMER_CODE) = TRIM(DDM.DISTRIBUTOR_CODE(+))
AND DPO.COMPANY_CODE = TRIM(CST.COMPANY_CODE(+))
AND DDM.COMPANY_CODE = GM.COMPANY_CODE(+)
AND DDM.GROUPID = GM.GROUPID
AND DPO.CREATED_BY = LU.USERID(+)
AND DPS.COMPANY_CODE = DPO.COMPANY_CODE(+)
AND DPO.COMPANY_CODE = CST.COMPANY_CODE
AND LU.SP_CODE = ES.EMPLOYEE_CODE
AND TRUNC(DPO.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'YYYY-MM-DD')
AND TO_DATE('{model.ToDate}', 'YYYY-MM-DD')
AND DPO.DELETED_FLAG = 'N'
{flagFilter}
                   {brachFilter1} {customerFilter}{SalesPersonFilter}
                    
) A
,(
SELECT ITEM_CODE
,APP_DATE
,SALES_RATE
,COMPANY_CODE
,BRANCH_CODE
FROM IP_ITEM_RATE_APPLICAT_SETUP D
WHERE 1 = 1
AND APP_DATE = (
SELECT MAX(APP_DATE)
FROM IP_ITEM_RATE_APPLICAT_SETUP
WHERE ITEM_CODE = D.ITEM_CODE
AND BRANCH_CODE = D.BRANCH_CODE
AND COMPANY_CODE = D.COMPANY_CODE
)
) B
WHERE A.ITEM_CODE = B.ITEM_CODE
AND A.cpp = B.COMPANY_CODE
AND A.bpp = B.BRANCH_CODE
)
GROUP BY ORDER_NO
,ORDER_DATE
,MITI
,GROUP_NAME
,CUSTOMER_CODE
,PARTY_TYPE_CODE
,APPROVED_FLAG
,DISPATCH_FLAG
,CUSTOMER_EDESC
,EMPLOYEE_EDESC
,REMARKS
,COMPANY_EDESC
,DELETED_FLAG
,CREATED_DATE
,CREATED_BY
,ACKNOWLEDGE_FLAG
,REJECT_FLAG
,CREDIT_LIMIT
,TOTAL_APPROVE_QTY
,TOTAL_APPROVE_AMT
,TOTAL_AMOUNT
,QUANTITY
,cpp
,bpp
,CS_PARTY
,PO_PARTY_TYPE
,PO_CONVERSION_UNIT
,PO_CONVERSION_FACTOR
,SO_CREDIT_LIMIT_FLAG
,SO_CREDIT_DAYS_FLAG
,PO_CUSTOM_RATE
ORDER BY ORDER_NO
) X
GROUP BY X.ORDER_NO
,X.COMPANY_CODE
,X.COMPANY_EDESC
,x.GROUP_EDESC
,X.CUSTOMER_CODE
,X.CUSTOMER_EDESC
,X.EMPLOYEE_EDESC
,X.REMARKS
,X.RESELLER_NAME
,X.ORDER_ENTITY
,X.MITI
,X.PARTY_TYPE_CODE
,X.PARTY_TYPE_EDESC
,X.CREATED_BY
,X.CREATED_DATE
,X.DELETED_FLAG
,X.BRANCH_CODE
,X.APPROVED_FLAG
,X.DISPATCH_FLAG
,X.ACKNOWLEDGE_FLAG
,X.REJECT_FLAG
,X.PO_PARTY_TYPE
,X.PO_CONVERSION_UNIT
,X.PO_CONVERSION_FACTOR
,X.SO_CREDIT_LIMIT_FLAG
,X.SO_CREDIT_DAYS_FLAG
,X.PO_CUSTOM_RATE
,X.CREDIT_LIMIT
ORDER BY X.ORDER_NO DESC";
            }
            else
            {
                query = $@"SELECT X.ORDER_NO
,X.COMPANY_CODE
,X.COMPANY_EDESC
,x.GROUP_EDESC
,X.CUSTOMER_CODE
,X.CUSTOMER_EDESC
,X.EMPLOYEE_EDESC
,X.REMARKS
,X.RESELLER_NAME
,X.ORDER_ENTITY
,X.MITI
,X.PARTY_TYPE_CODE
,X.PARTY_TYPE_EDESC
,X.CREATED_BY
,X.CREATED_DATE
,X.DELETED_FLAG
,X.BRANCH_CODE
,X.APPROVED_FLAG
,X.DISPATCH_FLAG
,X.ACKNOWLEDGE_FLAG
,X.REJECT_FLAG
,X.PO_PARTY_TYPE
,X.PO_CONVERSION_UNIT
,X.PO_CONVERSION_FACTOR
,X.SO_CREDIT_LIMIT_FLAG
,X.SO_CREDIT_DAYS_FLAG
,X.PO_CUSTOM_RATE
,X.CREDIT_LIMIT
-- ,X.TOTAL_QUANTITY
,SUM(NVL(X.TOTAL_QUANTITY,0))TOTAL_QUANTITY
,SUM(NVL(GrantTotalAmount,0))GrantTotalAmount
,SUM(NVL(GRAND_APPROVE_QUENTITY,0))GRAND_APPROVE_QUENTITY
,SUM(NVL(TOTAL_APPROVE_AMT,0))TOTAL_APPROVE_AMT
--,SUM(NVL(X.TOTAL_QUANTITY,0))TOTAL_QUANTITY
,SUM(NVL(X.NEW_TOTAL_AMOUNT, 0)) NEW_TOTAL_AMOUNT
,SUM(NVL(X.APPROVE_AMT, 0)) APPROVE_AMT
FROM (
SELECT ORDER_NO
,ORDER_DATE
,CUSTOMER_CODE
,CUSTOMER_EDESC
,'' RESELLER_NAME
,'P-D' ORDER_ENTITY
,MITI
,PARTY_TYPE_CODE
,REMARKS
,(
CASE
WHEN PARTY_TYPE_CODE IS NULL
THEN FN_FETCH_DESC(cpp, 'IP_PARTY_TYPE_CODE', CS_PARTY)
ELSE FN_FETCH_DESC(bpp, 'IP_PARTY_TYPE_CODE', PARTY_TYPE_CODE)
END
) PARTY_TYPE_EDESC
,CREATED_BY
,CREATED_DATE
,DELETED_FLAG
,cpp COMPANY_CODE
,COMPANY_EDESC
,bpp BRANCH_CODE
,APPROVED_FLAG
,DISPATCH_FLAG
,ACKNOWLEDGE_FLAG
,REJECT_FLAG
,EMPLOYEE_EDESC
,PO_PARTY_TYPE
,PO_CONVERSION_UNIT
,PO_CONVERSION_FACTOR
,SO_CREDIT_LIMIT_FLAG
,SO_CREDIT_DAYS_FLAG
,PO_CUSTOM_RATE
,NVL(CREDIT_LIMIT, 0) CREDIT_LIMIT
,NVL(QUANTITY, 0) TOTAL_QUANTITY
,--duplicate
TOTAL_AMOUNT GrantTotalAmount
,--duplicate
NVL(TOTAL_APPROVE_QTY, 0) GRAND_APPROVE_QUENTITY
,NVL(TOTAL_APPROVE_AMT, 0) TOTAL_APPROVE_AMT
,GROUP_NAME GROUP_EDESC
,SUM(SALES_RATE * QUANTITY) NEW_TOTAL_AMOUNT
,SUM(APPROVE_AMT) APPROVE_AMT
FROM (
SELECT *
FROM (
SELECT ORDER_NO
,ORDER_DATE
,TO_BS(ORDER_DATE) MITI
,DPO.CUSTOMER_CODE
,CS.CUSTOMER_EDESC
,GROUP_EDESC GROUP_NAME
,DPO.REJECT_FLAG
,DPO.DELETED_FLAG
,DPO.APPROVED_FLAG
,DPO.DISPATCH_FLAG
,DPO.ACKNOWLEDGE_FLAG
,DPO.PARTY_TYPE_CODE
,CS.PARTY_TYPE_CODE CS_PARTY
,CS.CREDIT_LIMIT
,DPO.TOTAL_PRICE TOTAL_AMOUNT
,DPO.APPROVE_QTY TOTAL_APPROVE_QTY
,DPO.APPROVE_AMT TOTAL_APPROVE_AMT
,DPO.CREATED_BY
,DPO.CREATED_DATE
,ES.EMPLOYEE_EDESC
,APPROVE_AMT
,ITEM_CODE
,DPO.COMPANY_CODE cpp
,DPO.BRANCH_CODE bpp
,DPO.QUANTITY
,DPO.REMARKS
,CST.COMPANY_EDESC
,DPS.PO_PARTY_TYPE
,DPS.PO_CONVERSION_UNIT
,DPS.PO_CONVERSION_FACTOR
,DPS.SO_CREDIT_LIMIT_CHK SO_CREDIT_LIMIT_FLAG
,DPS.SO_CREDIT_DAYS_CHK SO_CREDIT_DAYS_FLAG
,DPS.PO_CUSTOM_RATE PO_CUSTOM_RATE
FROM DIST_IP_SSD_PURCHASE_ORDER DPO
,SA_CUSTOMER_SETUP CS
,DIST_DISTRIBUTOR_MASTER DDM
,DIST_GROUP_MASTER GM
,HR_EMPLOYEE_SETUP ES
,DIST_LOGIN_USER LU
,COMPANY_SETUP CST
,DIST_PREFERENCE_SETUP DPS
WHERE TRIM(DPO.CUSTOMER_CODE) = TRIM(CS.CUSTOMER_CODE(+))
AND DPO.COMPANY_CODE = CS.COMPANY_CODE(+)
AND TRIM(DPO.CUSTOMER_CODE) = TRIM(DDM.DISTRIBUTOR_CODE(+))
AND DPO.COMPANY_CODE = TRIM(CST.COMPANY_CODE(+))
AND DDM.COMPANY_CODE = GM.COMPANY_CODE(+)
AND DDM.GROUPID = GM.GROUPID
AND DPO.CREATED_BY = LU.USERID(+)
AND DPS.COMPANY_CODE = DPO.COMPANY_CODE(+)
AND DPO.COMPANY_CODE = CST.COMPANY_CODE
AND LU.SP_CODE = ES.EMPLOYEE_CODE
AND TRUNC(DPO.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'YYYY-MM-DD')
AND TO_DATE('{model.ToDate}', 'YYYY-MM-DD')
AND DPO.DELETED_FLAG = 'N'
{flagFilter}
                   {brachFilter1} {customerFilter}{SalesPersonFilter}
) A
,(
SELECT ITEM_CODE
,APP_DATE
,SALES_RATE
,COMPANY_CODE
,BRANCH_CODE
FROM IP_ITEM_RATE_APPLICAT_SETUP D
WHERE 1 = 1
AND APP_DATE = (
SELECT MAX(APP_DATE)
FROM IP_ITEM_RATE_APPLICAT_SETUP
WHERE ITEM_CODE = D.ITEM_CODE
AND BRANCH_CODE = D.BRANCH_CODE
AND COMPANY_CODE = D.COMPANY_CODE
)
) B
WHERE A.ITEM_CODE = B.ITEM_CODE
AND A.cpp = B.COMPANY_CODE
AND A.bpp = B.BRANCH_CODE
)
GROUP BY ORDER_NO
,ORDER_DATE
,MITI
,GROUP_NAME
,CUSTOMER_CODE
,PARTY_TYPE_CODE
,APPROVED_FLAG
,DISPATCH_FLAG
,CUSTOMER_EDESC
,EMPLOYEE_EDESC
,REMARKS
,COMPANY_EDESC
,DELETED_FLAG
,CREATED_DATE
,CREATED_BY
,ACKNOWLEDGE_FLAG
,REJECT_FLAG
,CREDIT_LIMIT
,TOTAL_APPROVE_QTY
,TOTAL_APPROVE_AMT
,TOTAL_AMOUNT
,QUANTITY
,cpp
,bpp
,CS_PARTY
,PO_PARTY_TYPE
,PO_CONVERSION_UNIT
,PO_CONVERSION_FACTOR
,SO_CREDIT_LIMIT_FLAG
,SO_CREDIT_DAYS_FLAG
,PO_CUSTOM_RATE
ORDER BY ORDER_NO
) X
GROUP BY X.ORDER_NO
,X.COMPANY_CODE
,X.COMPANY_EDESC
,x.GROUP_EDESC
,X.CUSTOMER_CODE
,X.CUSTOMER_EDESC
,X.EMPLOYEE_EDESC
,X.REMARKS
,X.RESELLER_NAME
,X.ORDER_ENTITY
,X.MITI
,X.PARTY_TYPE_CODE
,X.PARTY_TYPE_EDESC
,X.CREATED_BY
,X.CREATED_DATE
,X.DELETED_FLAG
,X.BRANCH_CODE
,X.APPROVED_FLAG
,X.DISPATCH_FLAG
,X.ACKNOWLEDGE_FLAG
,X.REJECT_FLAG
,X.PO_PARTY_TYPE
,X.PO_CONVERSION_UNIT
,X.PO_CONVERSION_FACTOR
,X.SO_CREDIT_LIMIT_FLAG
,X.SO_CREDIT_DAYS_FLAG
,X.PO_CUSTOM_RATE
,X.CREDIT_LIMIT
ORDER BY X.ORDER_NO DESC";
            }





            var data = _objectEntity.SqlQuery<PurchaseOrderModel>(query).ToList();
            //foreach(var item in data)
            //{
            //    string tempQuery = @"SELECT NVL((SUM (V.DR_AMOUNT) - SUM (V.CR_AMOUNT)),0) balance FROM V$VIRTUAL_SUB_LEDGER V WHERE V.COMPANY_CODE in('"+userInfo.company_code+"') AND V.SUB_CODE ='C" + item.CUSTOMER_CODE + "'";
            //    var tempData = _objectEntity.SqlQuery<decimal>(tempQuery).FirstOrDefault();
            //    item.balance = tempData;
            //}

            //""
            return data;

        }

        public List<PurchaseOrderModel> GetDealerSalesOrder(ReportFiltersModel model, User userInfo, string requestStatus)
        {
            var companyCode = model.CompanyFilter.Count > 0 ? "'" + string.Join("','", model.CompanyFilter) + "'" : userInfo.company_code;
            var PO = _objectEntity.SqlQuery<PreferenceSetupModel>($"SELECT * FROM DIST_PREFERENCE_SETUP WHERE COMPANY_CODE='{userInfo.company_code}'").FirstOrDefault();
            var flagFilter = "";
            if (requestStatus == "Approved")
                flagFilter = @" --AND DPO.APPROVED_FLAG = 'Y'
                                AND DPO.APPROVE_AMT<>0  
                               AND DPO.REJECT_FLAG = 'N'";
            else if (requestStatus == "Rejected")
                flagFilter = @" AND DPO.REJECT_FLAG = 'Y'
                                AND DPO.APPROVED_FLAG = 'N'";
            else if (requestStatus == "All")
            {
                flagFilter = "";
            }
            else if (requestStatus == "Active")
            {
                flagFilter = @" AND DPO.REJECT_FLAG = 'N'
                                AND DPO.QUANTITY <>0
                                AND DPO.APPROVED_FLAG = 'N'";
            }
            string brachFilter1 = "";
            string brachFilter2 = "";
            if (model.BranchFilter.Count() > 0)
            {
                brachFilter1 = " and DPO.BRANCH_CODE IN('" + string.Join("', '", model.BranchFilter) + "')";
                brachFilter2 = " and VSL.BRANCH_CODE IN('" + string.Join("', '", model.BranchFilter) + "')";
            }

            var customerFilter = string.Empty;
            if (model.CustomerFilter.Count() > 0)
            {
                customerFilter = @"select  DISTINCT(customer_code) from sa_customer_setup where (";
                //IF CUSTOMER_SKU_FLAG = G
                foreach (var item in model.CustomerFilter)
                {
                    customerFilter += "master_customer_code like  (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + companyCode + ")) OR ";
                }
                customerFilter = customerFilter.Substring(0, customerFilter.Length - 3);
                //IF CUSTOMER_SKU_FLAG = I                
                customerFilter += " or (customer_code in (" + string.Join(",", model.CustomerFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + companyCode + "))) ";


                customerFilter = " and CS.customer_code IN(" + customerFilter + ")";
            }
            var SalesPersonFilter = string.Empty;
            if (model.ItemBrandFilter.Count > 0)
                SalesPersonFilter = $" AND  ES.EMPLOYEE_CODE IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                SalesPersonFilter = $" AND ES.EMPLOYEE_CODE IN  ({userInfo.sp_codes})";


            string query = string.Empty;
            if (PO.PO_DIST_RATE_COLUMN == "SALES_RATE_ZERO")
            {
                query = $@"
SELECT X.ORDER_NO
	,X.ORDER_FROM
	,X.COMPANY_CODE
	,X.COMPANY_EDESC
	--,x.GROUP_EDESC
	,X.CUSTOMER_CODE
	,X.CUSTOMER_EDESC
	--,X.EMPLOYEE_EDESC
	,X.REMARKS
	,X.RESELLER_NAME
	,X.ORDER_ENTITY
	,X.MITI
	,X.PARTY_TYPE_CODE
	,X.PARTY_TYPE_EDESC
	,X.CREATED_BY
	,X.CREATED_DATE
	,X.DELETED_FLAG
	,X.BRANCH_CODE
	,X.APPROVED_FLAG
	,X.DISPATCH_FLAG
	,X.ACKNOWLEDGE_FLAG
	,X.REJECT_FLAG
	,X.PO_PARTY_TYPE
	,X.PO_CONVERSION_UNIT
	,X.PO_CONVERSION_FACTOR
	,X.SO_CREDIT_LIMIT_FLAG
	,X.SO_CREDIT_DAYS_FLAG
	,X.PO_CUSTOM_RATE
	,X.CREDIT_LIMIT
	--,X.TOTAL_QUANTITY
	,SUM(NVL(X.TOTAL_QUANTITY, 0)) TOTAL_QUANTITY
	,SUM(NVL(GrantTotalAmount, 0)) GrantTotalAmount
	,SUM(NVL(GRAND_APPROVE_QUENTITY, 0)) GRAND_APPROVE_QUENTITY
	,SUM(NVL(TOTAL_APPROVE_AMT, 0)) TOTAL_APPROVE_AMT --,SUM(NVL(X.TOTAL_QUANTITY,0))TOTAL_QUANTITY
	--,SUM(NVL(X.NEW_TOTAL_AMOUNT, 0)) NEW_TOTAL_AMOUNT
	,SUM(NVL(X.APPROVE_AMT, 0)) APPROVE_AMT
FROM (
	SELECT ORDER_NO
		,ORDER_FROM
		,ORDER_DATE
		,CUSTOMER_CODE
		,CUSTOMER_EDESC
		,'' RESELLER_NAME
		,'P-D' ORDER_ENTITY
		,MITI
		,PARTY_TYPE_CODE
		,REMARKS
		,(
			CASE 
				WHEN PARTY_TYPE_CODE IS NULL
					THEN FN_FETCH_DESC(cpp, 'IP_PARTY_TYPE_CODE', CS_PARTY)
				ELSE FN_FETCH_DESC(CPP, 'IP_PARTY_TYPE_CODE', PARTY_TYPE_CODE)
				END
			) PARTY_TYPE_EDESC
		,CREATED_BY
		,CREATED_DATE
		,DELETED_FLAG
		,cpp COMPANY_CODE
		,COMPANY_EDESC
		,bpp BRANCH_CODE
		,APPROVED_FLAG
		,DISPATCH_FLAG
		,ACKNOWLEDGE_FLAG
		,REJECT_FLAG
		--,EMPLOYEE_EDESC
		,PO_PARTY_TYPE
		,PO_CONVERSION_UNIT
		,PO_CONVERSION_FACTOR
		,SO_CREDIT_LIMIT_FLAG
		,SO_CREDIT_DAYS_FLAG
		,PO_CUSTOM_RATE
		,NVL(CREDIT_LIMIT, 0) CREDIT_LIMIT
		,NVL(QUANTITY, 0) TOTAL_QUANTITY
		,--duplicate
		TOTAL_AMOUNT GrantTotalAmount
		,--duplicate
		NVL(TOTAL_APPROVE_QTY, 0) GRAND_APPROVE_QUENTITY
		,NVL(TOTAL_APPROVE_AMT, 0) TOTAL_APPROVE_AMT
		--,GROUP_NAME GROUP_EDESC
		--,SUM(SALES_RATE * QUANTITY) NEW_TOTAL_AMOUNT
		,SUM(APPROVE_AMT) APPROVE_AMT
	FROM (
		SELECT *
		FROM (
			SELECT ORDER_NO
				,ORDER_FROM
				,ORDER_DATE
				,TO_BS(ORDER_DATE) MITI
				,DPO.CUSTOMER_CODE
				,CS.CUSTOMER_EDESC
				--,GROUP_EDESC GROUP_NAME
				,DPO.REJECT_FLAG
				,DPO.DELETED_FLAG
				,DPO.APPROVED_FLAG
				,DPO.DISPATCH_FLAG
				,DPO.ACKNOWLEDGE_FLAG
				,DPO.PARTY_TYPE_CODE
				,CS.PARTY_TYPE_CODE CS_PARTY
				,CS.CREDIT_LIMIT
				,DPO.TOTAL_PRICE TOTAL_AMOUNT
				,DPO.APPROVE_QTY TOTAL_APPROVE_QTY
				,DPO.APPROVE_AMT TOTAL_APPROVE_AMT
				,DPO.CREATED_BY
				,DPO.CREATED_DATE
				--,ES.EMPLOYEE_EDESC
				,APPROVE_AMT
				,ITEM_CODE
				,DPO.COMPANY_CODE cpp
				,DPO.BRANCH_CODE bpp
				,DPO.QUANTITY
				,DPO.REMARKS
				,CST.COMPANY_EDESC
				,DPS.PO_PARTY_TYPE
				,DPS.PO_CONVERSION_UNIT
				,DPS.PO_CONVERSION_FACTOR
				,DPS.SO_CREDIT_LIMIT_CHK SO_CREDIT_LIMIT_FLAG
				,DPS.SO_CREDIT_DAYS_CHK SO_CREDIT_DAYS_FLAG
				,DPS.PO_CUSTOM_RATE PO_CUSTOM_RATE
			FROM DIST_IP_SSD_PURCHASE_ORDER DPO
				,SA_CUSTOMER_SETUP CS
				,DIST_DEALER_MASTER DDM
				,DIST_GROUP_MASTER GM
				--,HR_EMPLOYEE_SETUP ES
				,DIST_LOGIN_USER LU
				,COMPANY_SETUP CST
				,DIST_PREFERENCE_SETUP DPS
			WHERE TRIM(DPO.CUSTOMER_CODE) = TRIM(CS.CUSTOMER_CODE(+))
				AND DPO.COMPANY_CODE = CS.COMPANY_CODE(+)
				AND TRIM(DPO.PARTY_TYPE_CODE) = TRIM(DDM.DEALER_CODE(+))
				AND DPO.COMPANY_CODE = TRIM(CST.COMPANY_CODE(+))
				AND DDM.COMPANY_CODE = GM.COMPANY_CODE(+)
				--AND DDM.GROUPID = GM.GROUPID
				AND DPO.CREATED_BY = LU.USERID(+)
				AND DPS.COMPANY_CODE = DPO.COMPANY_CODE(+)
				AND DPO.COMPANY_CODE = CST.COMPANY_CODE
				--AND LU.SP_CODE = ES.EMPLOYEE_CODE
				--AND TRUNC(DPO.ORDER_DATE) BETWEEN TO_DATE('2019-Dec-02', 'YYYY-MM-DD') AND TO_DATE('2020-Dec-02', 'YYYY-MM-DD')
				AND DPO.DELETED_FLAG = 'N'
				AND DPO.REJECT_FLAG = 'N'
				AND DPO.QUANTITY <> 0
				AND DPO.APPROVED_FLAG = 'N'
				AND DPO.ORDER_FROM = 'L'
			) A
		)
	GROUP BY ORDER_NO
		,ORDER_FROM
		,ORDER_DATE
		,MITI
		--,GROUP_NAME
		,CUSTOMER_CODE
		,PARTY_TYPE_CODE
		,APPROVED_FLAG
		,DISPATCH_FLAG
		,CUSTOMER_EDESC
		--,EMPLOYEE_EDESC
		,REMARKS
		,COMPANY_EDESC
		,DELETED_FLAG
		,CREATED_DATE
		,CREATED_BY
		,ACKNOWLEDGE_FLAG
		,REJECT_FLAG
		,CREDIT_LIMIT
		,TOTAL_APPROVE_QTY
		,TOTAL_APPROVE_AMT
		,TOTAL_AMOUNT
		,QUANTITY
		,cpp
		,bpp
		,CS_PARTY
		,PO_PARTY_TYPE
		,PO_CONVERSION_UNIT
		,PO_CONVERSION_FACTOR
		,SO_CREDIT_LIMIT_FLAG
		,SO_CREDIT_DAYS_FLAG
		,PO_CUSTOM_RATE
	ORDER BY ORDER_NO
	) X
GROUP BY X.ORDER_NO
	,X.ORDER_FROM
	,X.COMPANY_CODE
	,X.COMPANY_EDESC
	--,x.GROUP_EDESC
	,X.CUSTOMER_CODE
	,X.CUSTOMER_EDESC
	--,X.EMPLOYEE_EDESC
	,X.REMARKS
	,X.RESELLER_NAME
	,X.ORDER_ENTITY
	,X.MITI
	,X.PARTY_TYPE_CODE
	,X.PARTY_TYPE_EDESC
	,X.CREATED_BY
	,X.CREATED_DATE
	,X.DELETED_FLAG
	,X.BRANCH_CODE
	,X.APPROVED_FLAG
	,X.DISPATCH_FLAG
	,X.ACKNOWLEDGE_FLAG
	,X.REJECT_FLAG
	,X.PO_PARTY_TYPE
	,X.PO_CONVERSION_UNIT
	,X.PO_CONVERSION_FACTOR
	,X.SO_CREDIT_LIMIT_FLAG
	,X.SO_CREDIT_DAYS_FLAG
	,X.PO_CUSTOM_RATE
	,X.CREDIT_LIMIT
ORDER BY X.ORDER_NO DESC";
            }
            else if (PO.PO_RATE_TABLE == "IP_ITEM_RATE_SCHEDULE_SETUP" && PO.PO_DIST_RATE_COLUMN != "SALES_RATE_ZERO")
            {
                var sales_rate = string.Empty;
                if (PO.PO_DIST_RATE_COLUMN == "SALES_RATE")
                {
                    sales_rate = "STANDARD_RATE";
                }
                else if (PO.PO_DIST_RATE_COLUMN == "MRP_RATE")
                {
                    sales_rate = "MRP_RATE";
                }
                else if (PO.PO_DIST_RATE_COLUMN == "RETAIL_PRICE")
                {
                    sales_rate = "RETAIL_PRICE";
                }
                query = $@"
SELECT X.ORDER_NO
  ,X.ORDER_FROM
	,X.COMPANY_CODE
	,X.COMPANY_EDESC
	--,x.GROUP_EDESC
	,X.CUSTOMER_CODE
	,X.CUSTOMER_EDESC
	--,X.EMPLOYEE_EDESC
	,X.REMARKS
	,X.RESELLER_NAME
	,X.ORDER_ENTITY
	,X.MITI
	,X.PARTY_TYPE_CODE
	,X.PARTY_TYPE_EDESC
	,X.CREATED_BY
	,X.CREATED_DATE
	,X.DELETED_FLAG
	,X.BRANCH_CODE
	,X.APPROVED_FLAG
	,X.DISPATCH_FLAG
	,X.ACKNOWLEDGE_FLAG
	,X.REJECT_FLAG
	,X.PO_PARTY_TYPE
	,X.PO_CONVERSION_UNIT
	,X.PO_CONVERSION_FACTOR
	,X.SO_CREDIT_LIMIT_FLAG
	,X.SO_CREDIT_DAYS_FLAG
	,X.PO_CUSTOM_RATE
	,X.CREDIT_LIMIT
	--,X.TOTAL_QUANTITY
	,SUM(NVL(X.TOTAL_QUANTITY, 0)) TOTAL_QUANTITY
	,SUM(NVL(GrantTotalAmount, 0)) GrantTotalAmount
	,SUM(NVL(GRAND_APPROVE_QUENTITY, 0)) GRAND_APPROVE_QUENTITY
	,SUM(NVL(TOTAL_APPROVE_AMT, 0)) TOTAL_APPROVE_AMT --,SUM(NVL(X.TOTAL_QUANTITY,0))TOTAL_QUANTITY
	,SUM(NVL(X.NEW_TOTAL_AMOUNT, 0)) NEW_TOTAL_AMOUNT
	,SUM(NVL(X.APPROVE_AMT, 0)) APPROVE_AMT
FROM (
	SELECT ORDER_NO
		,ORDER_FROM
		,ORDER_DATE
		,CUSTOMER_CODE
		,CUSTOMER_EDESC
		,'' RESELLER_NAME
		,'P-D' ORDER_ENTITY
		,MITI
		,PARTY_TYPE_CODE
		,REMARKS
		,(
			CASE 
				WHEN PARTY_TYPE_CODE IS NULL
					THEN FN_FETCH_DESC(cpp, 'IP_PARTY_TYPE_CODE', CS_PARTY)
				ELSE FN_FETCH_DESC(CPP, 'IP_PARTY_TYPE_CODE', PARTY_TYPE_CODE)
				END
			) PARTY_TYPE_EDESC
		,CREATED_BY
		,CREATED_DATE
		,DELETED_FLAG
		,cpp COMPANY_CODE
		,COMPANY_EDESC
		,bpp BRANCH_CODE
		,APPROVED_FLAG
		,DISPATCH_FLAG
		,ACKNOWLEDGE_FLAG
		,REJECT_FLAG
		--,EMPLOYEE_EDESC
		,PO_PARTY_TYPE
		,PO_CONVERSION_UNIT
		,PO_CONVERSION_FACTOR
		,SO_CREDIT_LIMIT_FLAG
		,SO_CREDIT_DAYS_FLAG
		,PO_CUSTOM_RATE
		,NVL(CREDIT_LIMIT, 0) CREDIT_LIMIT
		,NVL(QUANTITY, 0) TOTAL_QUANTITY
		,--duplicate
		TOTAL_AMOUNT GrantTotalAmount
		,--duplicate
		NVL(TOTAL_APPROVE_QTY, 0) GRAND_APPROVE_QUENTITY
		,NVL(TOTAL_APPROVE_AMT, 0) TOTAL_APPROVE_AMT
		--,GROUP_NAME GROUP_EDESC
		,SUM(NVL(({sales_rate}),0) * QUANTITY) NEW_TOTAL_AMOUNT
		,SUM(APPROVE_AMT) APPROVE_AMT
	FROM (
		SELECT *
		FROM (
			SELECT ORDER_NO
				,ORDER_FROM
				,ORDER_DATE
				,TO_BS(ORDER_DATE) MITI
				,DPO.CUSTOMER_CODE
				,CS.CUSTOMER_EDESC
				--,GROUP_EDESC GROUP_NAME
				,DPO.REJECT_FLAG
				,DPO.DELETED_FLAG
				,DPO.APPROVED_FLAG
				,DPO.DISPATCH_FLAG
				,DPO.ACKNOWLEDGE_FLAG
				,DPO.PARTY_TYPE_CODE
				,CS.PARTY_TYPE_CODE CS_PARTY
				,CS.CREDIT_LIMIT
				,DPO.TOTAL_PRICE TOTAL_AMOUNT
				,DPO.APPROVE_QTY TOTAL_APPROVE_QTY
				,DPO.APPROVE_AMT TOTAL_APPROVE_AMT
				,DPO.CREATED_BY
				,DPO.CREATED_DATE
				--,ES.EMPLOYEE_EDESC
				,APPROVE_AMT
				,ITEM_CODE
				,DPO.COMPANY_CODE cpp
				,DPO.BRANCH_CODE bpp
				,DPO.QUANTITY
				,DPO.REMARKS
				,CST.COMPANY_EDESC
				,DPS.PO_PARTY_TYPE
				,DPS.PO_CONVERSION_UNIT
				,DPS.PO_CONVERSION_FACTOR
				,DPS.SO_CREDIT_LIMIT_CHK SO_CREDIT_LIMIT_FLAG
				,DPS.SO_CREDIT_DAYS_CHK SO_CREDIT_DAYS_FLAG
				,DPS.PO_CUSTOM_RATE PO_CUSTOM_RATE
			FROM DIST_IP_SSD_PURCHASE_ORDER DPO
				,SA_CUSTOMER_SETUP CS
				,DIST_DEALER_MASTER DDM
				,DIST_GROUP_MASTER GM
				--,HR_EMPLOYEE_SETUP ES
				,DIST_LOGIN_USER LU
				,COMPANY_SETUP CST
				,DIST_PREFERENCE_SETUP DPS
			WHERE TRIM(DPO.CUSTOMER_CODE) = TRIM(CS.CUSTOMER_CODE(+))
				AND DPO.COMPANY_CODE = CS.COMPANY_CODE(+)
				AND TRIM(DPO.PARTY_TYPE_CODE) = TRIM(DDM.DEALER_CODE(+))
				AND DPO.COMPANY_CODE = TRIM(CST.COMPANY_CODE(+))
				AND DDM.COMPANY_CODE = GM.COMPANY_CODE(+)
				--AND DDM.GROUPID = GM.GROUPID
				AND DPO.CREATED_BY = LU.USERID(+)
				AND DPS.COMPANY_CODE = DPO.COMPANY_CODE(+)
				AND DPO.COMPANY_CODE = CST.COMPANY_CODE
				--AND LU.SP_CODE = ES.EMPLOYEE_CODE
				--AND TRUNC(DPO.ORDER_DATE) BETWEEN TO_DATE('2019-Dec-02', 'YYYY-MM-DD') AND TO_DATE('2020-Dec-02', 'YYYY-MM-DD')
				AND DPO.DELETED_FLAG = 'N'
				AND DPO.REJECT_FLAG = 'N'
				AND DPO.QUANTITY <> 0
				AND DPO.APPROVED_FLAG = 'N'
				AND DPO.ORDER_FROM = 'L'
			) A
			,(
				SELECT ITEM_CODE
					,EFFECTIVE_DATE
					,{sales_rate}
					,COMPANY_CODE
					--,BRANCH_CODE
				FROM IP_ITEM_RATE_SCHEDULE_SETUP D
				WHERE 1 = 1
					AND EFFECTIVE_DATE = (
						SELECT MAX(EFFECTIVE_DATE)
						FROM IP_ITEM_RATE_SCHEDULE_SETUP
						WHERE ITEM_CODE = D.ITEM_CODE
							--AND BRANCH_CODE = D.BRANCH_CODE
							AND COMPANY_CODE = D.COMPANY_CODE
						)
				) B
		WHERE A.ITEM_CODE = B.ITEM_CODE(+)
			AND A.cpp = B.COMPANY_CODE(+)
			--AND A.bpp = B.BRANCH_CODE(+)
		)
	GROUP BY ORDER_NO
    ,ORDER_FROM
		,ORDER_DATE
		,MITI
		--,GROUP_NAME
		,CUSTOMER_CODE
		,PARTY_TYPE_CODE
		,APPROVED_FLAG
		,DISPATCH_FLAG
		,CUSTOMER_EDESC
		--,EMPLOYEE_EDESC
		,REMARKS
		,COMPANY_EDESC
		,DELETED_FLAG
		,CREATED_DATE
		,CREATED_BY
		,ACKNOWLEDGE_FLAG
		,REJECT_FLAG
		,CREDIT_LIMIT
		,TOTAL_APPROVE_QTY
		,TOTAL_APPROVE_AMT
		,TOTAL_AMOUNT
		,QUANTITY
		,cpp
		,bpp
		,CS_PARTY
		,PO_PARTY_TYPE
		,PO_CONVERSION_UNIT
		,PO_CONVERSION_FACTOR
		,SO_CREDIT_LIMIT_FLAG
		,SO_CREDIT_DAYS_FLAG
		,PO_CUSTOM_RATE
	ORDER BY ORDER_NO
	) X
GROUP BY X.ORDER_NO
  ,X.ORDER_FROM
	,X.COMPANY_CODE
	,X.COMPANY_EDESC
	--,x.GROUP_EDESC
	,X.CUSTOMER_CODE
	,X.CUSTOMER_EDESC
	--,X.EMPLOYEE_EDESC
	,X.REMARKS
	,X.RESELLER_NAME
	,X.ORDER_ENTITY
	,X.MITI
	,X.PARTY_TYPE_CODE
	,X.PARTY_TYPE_EDESC
	,X.CREATED_BY
	,X.CREATED_DATE
	,X.DELETED_FLAG
	,X.BRANCH_CODE
	,X.APPROVED_FLAG
	,X.DISPATCH_FLAG
	,X.ACKNOWLEDGE_FLAG
	,X.REJECT_FLAG
	,X.PO_PARTY_TYPE
	,X.PO_CONVERSION_UNIT
	,X.PO_CONVERSION_FACTOR
	,X.SO_CREDIT_LIMIT_FLAG
	,X.SO_CREDIT_DAYS_FLAG
	,X.PO_CUSTOM_RATE
	,X.CREDIT_LIMIT
ORDER BY X.ORDER_NO DESC";
            }
            else if (PO.PO_RATE_TABLE == "IP_ITEM_RATE_APPLICAT_SETUP" && PO.PO_DIST_RATE_COLUMN != "SALES_RATE_ZERO")
            {
                query = $@"
SELECT X.ORDER_NO
  ,X.ORDER_FROM
	,X.COMPANY_CODE
	,X.COMPANY_EDESC
	--,x.GROUP_EDESC
	,X.CUSTOMER_CODE
	,X.CUSTOMER_EDESC
	--,X.EMPLOYEE_EDESC
	,X.REMARKS
	,X.RESELLER_NAME
	,X.ORDER_ENTITY
	,X.MITI
	,X.PARTY_TYPE_CODE
	,X.PARTY_TYPE_EDESC
	,X.CREATED_BY
	,X.CREATED_DATE
	,X.DELETED_FLAG
	,X.BRANCH_CODE
	,X.APPROVED_FLAG
	,X.DISPATCH_FLAG
	,X.ACKNOWLEDGE_FLAG
	,X.REJECT_FLAG
	,X.PO_PARTY_TYPE
	,X.PO_CONVERSION_UNIT
	,X.PO_CONVERSION_FACTOR
	,X.SO_CREDIT_LIMIT_FLAG
	,X.SO_CREDIT_DAYS_FLAG
	,X.PO_CUSTOM_RATE
	,X.CREDIT_LIMIT
	--,X.TOTAL_QUANTITY
	,SUM(NVL(X.TOTAL_QUANTITY, 0)) TOTAL_QUANTITY
	,SUM(NVL(GrantTotalAmount, 0)) GrantTotalAmount
	,SUM(NVL(GRAND_APPROVE_QUENTITY, 0)) GRAND_APPROVE_QUENTITY
	,SUM(NVL(TOTAL_APPROVE_AMT, 0)) TOTAL_APPROVE_AMT --,SUM(NVL(X.TOTAL_QUANTITY,0))TOTAL_QUANTITY
	,SUM(NVL(X.NEW_TOTAL_AMOUNT, 0)) NEW_TOTAL_AMOUNT
	,SUM(NVL(X.APPROVE_AMT, 0)) APPROVE_AMT
FROM (
	SELECT ORDER_NO
		,ORDER_FROM
		,ORDER_DATE
		,CUSTOMER_CODE
		,CUSTOMER_EDESC
		,'' RESELLER_NAME
		,'P-D' ORDER_ENTITY
		,MITI
		,PARTY_TYPE_CODE
		,REMARKS
		,(
			CASE 
				WHEN PARTY_TYPE_CODE IS NULL
					THEN FN_FETCH_DESC(cpp, 'IP_PARTY_TYPE_CODE', CS_PARTY)
				ELSE FN_FETCH_DESC(CPP, 'IP_PARTY_TYPE_CODE', PARTY_TYPE_CODE)
				END
			) PARTY_TYPE_EDESC
		,CREATED_BY
		,CREATED_DATE
		,DELETED_FLAG
		,cpp COMPANY_CODE
		,COMPANY_EDESC
		,bpp BRANCH_CODE
		,APPROVED_FLAG
		,DISPATCH_FLAG
		,ACKNOWLEDGE_FLAG
		,REJECT_FLAG
		--,EMPLOYEE_EDESC
		,PO_PARTY_TYPE
		,PO_CONVERSION_UNIT
		,PO_CONVERSION_FACTOR
		,SO_CREDIT_LIMIT_FLAG
		,SO_CREDIT_DAYS_FLAG
		,PO_CUSTOM_RATE
		,NVL(CREDIT_LIMIT, 0) CREDIT_LIMIT
		,NVL(QUANTITY, 0) TOTAL_QUANTITY
		,--duplicate
		TOTAL_AMOUNT GrantTotalAmount
		,--duplicate
		NVL(TOTAL_APPROVE_QTY, 0) GRAND_APPROVE_QUENTITY
		,NVL(TOTAL_APPROVE_AMT, 0) TOTAL_APPROVE_AMT
		--,GROUP_NAME GROUP_EDESC
		,SUM(SALES_RATE * QUANTITY) NEW_TOTAL_AMOUNT
		,SUM(APPROVE_AMT) APPROVE_AMT
	FROM (
		SELECT *
		FROM (
			SELECT ORDER_NO
				,ORDER_FROM
				,ORDER_DATE
				,TO_BS(ORDER_DATE) MITI
				,DPO.CUSTOMER_CODE
				,CS.CUSTOMER_EDESC
				--,GROUP_EDESC GROUP_NAME
				,DPO.REJECT_FLAG
				,DPO.DELETED_FLAG
				,DPO.APPROVED_FLAG
				,DPO.DISPATCH_FLAG
				,DPO.ACKNOWLEDGE_FLAG
				,DPO.PARTY_TYPE_CODE
				,CS.PARTY_TYPE_CODE CS_PARTY
				,CS.CREDIT_LIMIT
				,DPO.TOTAL_PRICE TOTAL_AMOUNT
				,DPO.APPROVE_QTY TOTAL_APPROVE_QTY
				,DPO.APPROVE_AMT TOTAL_APPROVE_AMT
				,DPO.CREATED_BY
				,DPO.CREATED_DATE
				--,ES.EMPLOYEE_EDESC
				,APPROVE_AMT
				,ITEM_CODE
				,DPO.COMPANY_CODE cpp
				,DPO.BRANCH_CODE bpp
				,DPO.QUANTITY
				,DPO.REMARKS
				,CST.COMPANY_EDESC
				,DPS.PO_PARTY_TYPE
				,DPS.PO_CONVERSION_UNIT
				,DPS.PO_CONVERSION_FACTOR
				,DPS.SO_CREDIT_LIMIT_CHK SO_CREDIT_LIMIT_FLAG
				,DPS.SO_CREDIT_DAYS_CHK SO_CREDIT_DAYS_FLAG
				,DPS.PO_CUSTOM_RATE PO_CUSTOM_RATE
			FROM DIST_IP_SSD_PURCHASE_ORDER DPO
				,SA_CUSTOMER_SETUP CS
				,DIST_DEALER_MASTER DDM
				,DIST_GROUP_MASTER GM
				--,HR_EMPLOYEE_SETUP ES
				,DIST_LOGIN_USER LU
				,COMPANY_SETUP CST
				,DIST_PREFERENCE_SETUP DPS
			WHERE TRIM(DPO.CUSTOMER_CODE) = TRIM(CS.CUSTOMER_CODE(+))
				AND DPO.COMPANY_CODE = CS.COMPANY_CODE(+)
				AND TRIM(DPO.PARTY_TYPE_CODE) = TRIM(DDM.DEALER_CODE(+))
				AND DPO.COMPANY_CODE = TRIM(CST.COMPANY_CODE(+))
				AND DDM.COMPANY_CODE = GM.COMPANY_CODE(+)
				--AND DDM.GROUPID = GM.GROUPID
				AND DPO.CREATED_BY = LU.USERID(+)
				AND DPS.COMPANY_CODE = DPO.COMPANY_CODE(+)
				AND DPO.COMPANY_CODE = CST.COMPANY_CODE
				--AND LU.SP_CODE = ES.EMPLOYEE_CODE
				--AND TRUNC(DPO.ORDER_DATE) BETWEEN TO_DATE('2019-Dec-02', 'YYYY-MM-DD') AND TO_DATE('2020-Dec-02', 'YYYY-MM-DD')
				AND DPO.DELETED_FLAG = 'N'
				AND DPO.REJECT_FLAG = 'N'
				AND DPO.QUANTITY <> 0
				AND DPO.APPROVED_FLAG = 'N'
				AND DPO.ORDER_FROM = 'L'
			) A
			,(
				SELECT ITEM_CODE
					,APP_DATE
					,SALES_RATE
					,COMPANY_CODE
					,BRANCH_CODE
				FROM IP_ITEM_RATE_APPLICAT_SETUP D
				WHERE 1 = 1
					AND APP_DATE = (
						SELECT MAX(APP_DATE)
						FROM IP_ITEM_RATE_APPLICAT_SETUP
						WHERE ITEM_CODE = D.ITEM_CODE
							AND BRANCH_CODE = D.BRANCH_CODE
							AND COMPANY_CODE = D.COMPANY_CODE
						)
				) B
		WHERE A.ITEM_CODE = B.ITEM_CODE(+)
			AND A.cpp = B.COMPANY_CODE(+)
			AND A.bpp = B.BRANCH_CODE(+)
		)
	GROUP BY ORDER_NO
    ,ORDER_FROM
		,ORDER_DATE
		,MITI
		--,GROUP_NAME
		,CUSTOMER_CODE
		,PARTY_TYPE_CODE
		,APPROVED_FLAG
		,DISPATCH_FLAG
		,CUSTOMER_EDESC
		--,EMPLOYEE_EDESC
		,REMARKS
		,COMPANY_EDESC
		,DELETED_FLAG
		,CREATED_DATE
		,CREATED_BY
		,ACKNOWLEDGE_FLAG
		,REJECT_FLAG
		,CREDIT_LIMIT
		,TOTAL_APPROVE_QTY
		,TOTAL_APPROVE_AMT
		,TOTAL_AMOUNT
		,QUANTITY
		,cpp
		,bpp
		,CS_PARTY
		,PO_PARTY_TYPE
		,PO_CONVERSION_UNIT
		,PO_CONVERSION_FACTOR
		,SO_CREDIT_LIMIT_FLAG
		,SO_CREDIT_DAYS_FLAG
		,PO_CUSTOM_RATE
	ORDER BY ORDER_NO
	) X
GROUP BY X.ORDER_NO
  ,X.ORDER_FROM
	,X.COMPANY_CODE
	,X.COMPANY_EDESC
	--,x.GROUP_EDESC
	,X.CUSTOMER_CODE
	,X.CUSTOMER_EDESC
	--,X.EMPLOYEE_EDESC
	,X.REMARKS
	,X.RESELLER_NAME
	,X.ORDER_ENTITY
	,X.MITI
	,X.PARTY_TYPE_CODE
	,X.PARTY_TYPE_EDESC
	,X.CREATED_BY
	,X.CREATED_DATE
	,X.DELETED_FLAG
	,X.BRANCH_CODE
	,X.APPROVED_FLAG
	,X.DISPATCH_FLAG
	,X.ACKNOWLEDGE_FLAG
	,X.REJECT_FLAG
	,X.PO_PARTY_TYPE
	,X.PO_CONVERSION_UNIT
	,X.PO_CONVERSION_FACTOR
	,X.SO_CREDIT_LIMIT_FLAG
	,X.SO_CREDIT_DAYS_FLAG
	,X.PO_CUSTOM_RATE
	,X.CREDIT_LIMIT
ORDER BY X.ORDER_NO DESC";
            }
            else
            {
                //if (PO.SO_REPO_RATE_TABLE == "DIST_IP_SSD_PURCHASE_ORDER" && PO.SO_REPO_RATE_COLUMN == "UNIT_PRICE")
                //{
                query = $@"
SELECT X.ORDER_NO
  ,X.ORDER_FROM
	,X.COMPANY_CODE
	,X.COMPANY_EDESC
	--,x.GROUP_EDESC
	,X.CUSTOMER_CODE
	,X.CUSTOMER_EDESC
	--,X.EMPLOYEE_EDESC
	,X.REMARKS
	,X.RESELLER_NAME
	,X.ORDER_ENTITY
	,X.MITI
	,X.PARTY_TYPE_CODE
	,X.PARTY_TYPE_EDESC
	,X.CREATED_BY
	,X.CREATED_DATE
	,X.DELETED_FLAG
	,X.BRANCH_CODE
	,X.APPROVED_FLAG
	,X.DISPATCH_FLAG
	,X.ACKNOWLEDGE_FLAG
	,X.REJECT_FLAG
	,X.PO_PARTY_TYPE
	,X.PO_CONVERSION_UNIT
	,X.PO_CONVERSION_FACTOR
	,X.SO_CREDIT_LIMIT_FLAG
	,X.SO_CREDIT_DAYS_FLAG
	,X.PO_CUSTOM_RATE
	,X.CREDIT_LIMIT
	--,X.TOTAL_QUANTITY
	,SUM(NVL(X.TOTAL_QUANTITY, 0)) TOTAL_QUANTITY
	,SUM(NVL(GrantTotalAmount, 0)) GrantTotalAmount
	,SUM(NVL(GRAND_APPROVE_QUENTITY, 0)) GRAND_APPROVE_QUENTITY
	,SUM(NVL(TOTAL_APPROVE_AMT, 0)) TOTAL_APPROVE_AMT --,SUM(NVL(X.TOTAL_QUANTITY,0))TOTAL_QUANTITY
	,SUM(NVL(X.NEW_TOTAL_AMOUNT, 0)) NEW_TOTAL_AMOUNT
	,SUM(NVL(X.APPROVE_AMT, 0)) APPROVE_AMT
FROM (
	SELECT ORDER_NO
		,ORDER_FROM
		,ORDER_DATE
		,CUSTOMER_CODE
		,CUSTOMER_EDESC
		,'' RESELLER_NAME
		,'P-D' ORDER_ENTITY
		,MITI
		,PARTY_TYPE_CODE
		,REMARKS
		,(
			CASE 
				WHEN PARTY_TYPE_CODE IS NULL
					THEN FN_FETCH_DESC(cpp, 'IP_PARTY_TYPE_CODE', CS_PARTY)
				ELSE FN_FETCH_DESC(CPP, 'IP_PARTY_TYPE_CODE', PARTY_TYPE_CODE)
				END
			) PARTY_TYPE_EDESC
		,CREATED_BY
		,CREATED_DATE
		,DELETED_FLAG
		,cpp COMPANY_CODE
		,COMPANY_EDESC
		,bpp BRANCH_CODE
		,APPROVED_FLAG
		,DISPATCH_FLAG
		,ACKNOWLEDGE_FLAG
		,REJECT_FLAG
		--,EMPLOYEE_EDESC
		,PO_PARTY_TYPE
		,PO_CONVERSION_UNIT
		,PO_CONVERSION_FACTOR
		,SO_CREDIT_LIMIT_FLAG
		,SO_CREDIT_DAYS_FLAG
		,PO_CUSTOM_RATE
		,NVL(CREDIT_LIMIT, 0) CREDIT_LIMIT
		,NVL(QUANTITY, 0) TOTAL_QUANTITY
		,--duplicate
		TOTAL_AMOUNT GrantTotalAmount
		,--duplicate
		NVL(TOTAL_APPROVE_QTY, 0) GRAND_APPROVE_QUENTITY
		,NVL(TOTAL_APPROVE_AMT, 0) TOTAL_APPROVE_AMT
		--,GROUP_NAME GROUP_EDESC
		,SUM(SALES_RATE * QUANTITY) NEW_TOTAL_AMOUNT
		,SUM(APPROVE_AMT) APPROVE_AMT
	FROM (
		SELECT *
		FROM (
			SELECT ORDER_NO
				,ORDER_FROM
				,ORDER_DATE
				,TO_BS(ORDER_DATE) MITI
				,DPO.CUSTOMER_CODE
				,CS.CUSTOMER_EDESC
				--,GROUP_EDESC GROUP_NAME
				,DPO.REJECT_FLAG
				,DPO.DELETED_FLAG
				,DPO.APPROVED_FLAG
				,DPO.DISPATCH_FLAG
				,DPO.ACKNOWLEDGE_FLAG
				,DPO.PARTY_TYPE_CODE
				,CS.PARTY_TYPE_CODE CS_PARTY
				,CS.CREDIT_LIMIT
				,DPO.TOTAL_PRICE TOTAL_AMOUNT
				,DPO.APPROVE_QTY TOTAL_APPROVE_QTY
				,DPO.APPROVE_AMT TOTAL_APPROVE_AMT
				,DPO.CREATED_BY
				,DPO.CREATED_DATE
				--,ES.EMPLOYEE_EDESC
				,APPROVE_AMT
				,ITEM_CODE
				,DPO.COMPANY_CODE cpp
				,DPO.BRANCH_CODE bpp
				,DPO.QUANTITY
				,DPO.REMARKS
				,CST.COMPANY_EDESC
				,DPS.PO_PARTY_TYPE
				,DPS.PO_CONVERSION_UNIT
				,DPS.PO_CONVERSION_FACTOR
				,DPS.SO_CREDIT_LIMIT_CHK SO_CREDIT_LIMIT_FLAG
				,DPS.SO_CREDIT_DAYS_CHK SO_CREDIT_DAYS_FLAG
				,DPS.PO_CUSTOM_RATE PO_CUSTOM_RATE
			FROM DIST_IP_SSD_PURCHASE_ORDER DPO
				,SA_CUSTOMER_SETUP CS
				,DIST_DEALER_MASTER DDM
				,DIST_GROUP_MASTER GM
				--,HR_EMPLOYEE_SETUP ES
				,DIST_LOGIN_USER LU
				,COMPANY_SETUP CST
				,DIST_PREFERENCE_SETUP DPS
			WHERE TRIM(DPO.CUSTOMER_CODE) = TRIM(CS.CUSTOMER_CODE(+))
				AND DPO.COMPANY_CODE = CS.COMPANY_CODE(+)
				AND TRIM(DPO.PARTY_TYPE_CODE) = TRIM(DDM.DEALER_CODE(+))
				AND DPO.COMPANY_CODE = TRIM(CST.COMPANY_CODE(+))
				AND DDM.COMPANY_CODE = GM.COMPANY_CODE(+)
				--AND DDM.GROUPID = GM.GROUPID
				AND DPO.CREATED_BY = LU.USERID(+)
				AND DPS.COMPANY_CODE = DPO.COMPANY_CODE(+)
				AND DPO.COMPANY_CODE = CST.COMPANY_CODE
				--AND LU.SP_CODE = ES.EMPLOYEE_CODE
				--AND TRUNC(DPO.ORDER_DATE) BETWEEN TO_DATE('2019-Dec-02', 'YYYY-MM-DD') AND TO_DATE('2020-Dec-02', 'YYYY-MM-DD')
				AND DPO.DELETED_FLAG = 'N'
				AND DPO.REJECT_FLAG = 'N'
				AND DPO.QUANTITY <> 0
				AND DPO.APPROVED_FLAG = 'N'
				AND DPO.ORDER_FROM = 'L'
			) A
			,(
				SELECT ITEM_CODE
					,APP_DATE
					,SALES_RATE
					,COMPANY_CODE
					,BRANCH_CODE
				FROM IP_ITEM_RATE_APPLICAT_SETUP D
				WHERE 1 = 1
					AND APP_DATE = (
						SELECT MAX(APP_DATE)
						FROM IP_ITEM_RATE_APPLICAT_SETUP
						WHERE ITEM_CODE = D.ITEM_CODE
							AND BRANCH_CODE = D.BRANCH_CODE
							AND COMPANY_CODE = D.COMPANY_CODE
						)
				) B
		WHERE A.ITEM_CODE = B.ITEM_CODE(+)
			AND A.cpp = B.COMPANY_CODE(+)
			AND A.bpp = B.BRANCH_CODE(+)
		)
	GROUP BY ORDER_NO
    ,ORDER_FROM
		,ORDER_DATE
		,MITI
		--,GROUP_NAME
		,CUSTOMER_CODE
		,PARTY_TYPE_CODE
		,APPROVED_FLAG
		,DISPATCH_FLAG
		,CUSTOMER_EDESC
		--,EMPLOYEE_EDESC
		,REMARKS
		,COMPANY_EDESC
		,DELETED_FLAG
		,CREATED_DATE
		,CREATED_BY
		,ACKNOWLEDGE_FLAG
		,REJECT_FLAG
		,CREDIT_LIMIT
		,TOTAL_APPROVE_QTY
		,TOTAL_APPROVE_AMT
		,TOTAL_AMOUNT
		,QUANTITY
		,cpp
		,bpp
		,CS_PARTY
		,PO_PARTY_TYPE
		,PO_CONVERSION_UNIT
		,PO_CONVERSION_FACTOR
		,SO_CREDIT_LIMIT_FLAG
		,SO_CREDIT_DAYS_FLAG
		,PO_CUSTOM_RATE
	ORDER BY ORDER_NO
	) X
GROUP BY X.ORDER_NO
  ,X.ORDER_FROM
	,X.COMPANY_CODE
	,X.COMPANY_EDESC
	--,x.GROUP_EDESC
	,X.CUSTOMER_CODE
	,X.CUSTOMER_EDESC
	--,X.EMPLOYEE_EDESC
	,X.REMARKS
	,X.RESELLER_NAME
	,X.ORDER_ENTITY
	,X.MITI
	,X.PARTY_TYPE_CODE
	,X.PARTY_TYPE_EDESC
	,X.CREATED_BY
	,X.CREATED_DATE
	,X.DELETED_FLAG
	,X.BRANCH_CODE
	,X.APPROVED_FLAG
	,X.DISPATCH_FLAG
	,X.ACKNOWLEDGE_FLAG
	,X.REJECT_FLAG
	,X.PO_PARTY_TYPE
	,X.PO_CONVERSION_UNIT
	,X.PO_CONVERSION_FACTOR
	,X.SO_CREDIT_LIMIT_FLAG
	,X.SO_CREDIT_DAYS_FLAG
	,X.PO_CUSTOM_RATE
	,X.CREDIT_LIMIT
ORDER BY X.ORDER_NO DESC";
                //}
                //            else
                //            {
                //                query = $@"SELECT X.ORDER_NO
                //,X.COMPANY_CODE
                //,X.COMPANY_EDESC
                //,x.GROUP_EDESC
                //,X.CUSTOMER_CODE
                //,X.CUSTOMER_EDESC
                //,X.EMPLOYEE_EDESC
                //,X.REMARKS
                //,X.RESELLER_NAME
                //,X.ORDER_ENTITY
                //,X.MITI
                //,X.PARTY_TYPE_CODE
                //,X.PARTY_TYPE_EDESC
                //,X.CREATED_BY
                //,X.CREATED_DATE
                //,X.DELETED_FLAG
                //,X.BRANCH_CODE
                //,X.APPROVED_FLAG
                //,X.DISPATCH_FLAG
                //,X.ACKNOWLEDGE_FLAG
                //,X.REJECT_FLAG
                //,X.PO_PARTY_TYPE
                //,X.PO_CONVERSION_UNIT
                //,X.PO_CONVERSION_FACTOR
                //,X.SO_CREDIT_LIMIT_FLAG
                //,X.SO_CREDIT_DAYS_FLAG
                //,X.PO_CUSTOM_RATE
                //,X.CREDIT_LIMIT
                //-- ,X.TOTAL_QUANTITY
                //,SUM(NVL(X.TOTAL_QUANTITY,0))TOTAL_QUANTITY
                //,SUM(NVL(GrantTotalAmount,0))GrantTotalAmount
                //,SUM(NVL(GRAND_APPROVE_QUENTITY,0))GRAND_APPROVE_QUENTITY
                //,SUM(NVL(TOTAL_APPROVE_AMT,0))TOTAL_APPROVE_AMT
                //--,SUM(NVL(X.TOTAL_QUANTITY,0))TOTAL_QUANTITY
                //,SUM(NVL(X.NEW_TOTAL_AMOUNT, 0)) NEW_TOTAL_AMOUNT
                //,SUM(NVL(X.APPROVE_AMT, 0)) APPROVE_AMT
                //FROM (
                //SELECT ORDER_NO
                //,ORDER_DATE
                //,CUSTOMER_CODE
                //,CUSTOMER_EDESC
                //,'' RESELLER_NAME
                //,'P-D' ORDER_ENTITY
                //,MITI
                //,PARTY_TYPE_CODE
                //,REMARKS
                //,(
                //CASE
                //WHEN PARTY_TYPE_CODE IS NULL
                //THEN FN_FETCH_DESC(cpp, 'IP_PARTY_TYPE_CODE', CS_PARTY)
                //ELSE FN_FETCH_DESC(bpp, 'IP_PARTY_TYPE_CODE', PARTY_TYPE_CODE)
                //END
                //) PARTY_TYPE_EDESC
                //,CREATED_BY
                //,CREATED_DATE
                //,DELETED_FLAG
                //,cpp COMPANY_CODE
                //,COMPANY_EDESC
                //,bpp BRANCH_CODE
                //,APPROVED_FLAG
                //,DISPATCH_FLAG
                //,ACKNOWLEDGE_FLAG
                //,REJECT_FLAG
                //,EMPLOYEE_EDESC
                //,PO_PARTY_TYPE
                //,PO_CONVERSION_UNIT
                //,PO_CONVERSION_FACTOR
                //,SO_CREDIT_LIMIT_FLAG
                //,SO_CREDIT_DAYS_FLAG
                //,PO_CUSTOM_RATE
                //,NVL(CREDIT_LIMIT, 0) CREDIT_LIMIT
                //,NVL(QUANTITY, 0) TOTAL_QUANTITY
                //,--duplicate
                //TOTAL_AMOUNT GrantTotalAmount
                //,--duplicate
                //NVL(TOTAL_APPROVE_QTY, 0) GRAND_APPROVE_QUENTITY
                //,NVL(TOTAL_APPROVE_AMT, 0) TOTAL_APPROVE_AMT
                //,GROUP_NAME GROUP_EDESC
                //,SUM(SALES_RATE * QUANTITY) NEW_TOTAL_AMOUNT
                //,SUM(APPROVE_AMT) APPROVE_AMT
                //FROM (
                //SELECT *
                //FROM (
                //SELECT ORDER_NO
                //,ORDER_DATE
                //,TO_BS(ORDER_DATE) MITI
                //,DPO.CUSTOMER_CODE
                //,CS.CUSTOMER_EDESC
                //,GROUP_EDESC GROUP_NAME
                //,DPO.REJECT_FLAG
                //,DPO.DELETED_FLAG
                //,DPO.APPROVED_FLAG
                //,DPO.DISPATCH_FLAG
                //,DPO.ACKNOWLEDGE_FLAG
                //,DPO.PARTY_TYPE_CODE
                //,CS.PARTY_TYPE_CODE CS_PARTY
                //,CS.CREDIT_LIMIT
                //,DPO.TOTAL_PRICE TOTAL_AMOUNT
                //,DPO.APPROVE_QTY TOTAL_APPROVE_QTY
                //,DPO.APPROVE_AMT TOTAL_APPROVE_AMT
                //,DPO.CREATED_BY
                //,DPO.CREATED_DATE
                //,ES.EMPLOYEE_EDESC
                //,APPROVE_AMT
                //,ITEM_CODE
                //,DPO.COMPANY_CODE cpp
                //,DPO.BRANCH_CODE bpp
                //,DPO.QUANTITY
                //,DPO.REMARKS
                //,CST.COMPANY_EDESC
                //,DPS.PO_PARTY_TYPE
                //,DPS.PO_CONVERSION_UNIT
                //,DPS.PO_CONVERSION_FACTOR
                //,DPS.SO_CREDIT_LIMIT_CHK SO_CREDIT_LIMIT_FLAG
                //,DPS.SO_CREDIT_DAYS_CHK SO_CREDIT_DAYS_FLAG
                //,DPS.PO_CUSTOM_RATE PO_CUSTOM_RATE
                //FROM DIST_IP_SSD_PURCHASE_ORDER DPO
                //,SA_CUSTOMER_SETUP CS
                //,DIST_DISTRIBUTOR_MASTER DDM
                //,DIST_GROUP_MASTER GM
                //,HR_EMPLOYEE_SETUP ES
                //,DIST_LOGIN_USER LU
                //,COMPANY_SETUP CST
                //,DIST_PREFERENCE_SETUP DPS
                //WHERE TRIM(DPO.CUSTOMER_CODE) = TRIM(CS.CUSTOMER_CODE(+))
                //AND DPO.COMPANY_CODE = CS.COMPANY_CODE(+)
                //AND TRIM(DPO.CUSTOMER_CODE) = TRIM(DDM.DISTRIBUTOR_CODE(+))
                //AND DPO.COMPANY_CODE = TRIM(CST.COMPANY_CODE(+))
                //AND DDM.COMPANY_CODE = GM.COMPANY_CODE(+)
                //AND DDM.GROUPID = GM.GROUPID
                //AND DPO.CREATED_BY = LU.USERID(+)
                //AND DPS.COMPANY_CODE = DPO.COMPANY_CODE(+)
                //AND DPO.COMPANY_CODE = CST.COMPANY_CODE
                //AND LU.SP_CODE = ES.EMPLOYEE_CODE
                //AND TRUNC(DPO.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'YYYY-MM-DD')
                //AND TO_DATE('{model.ToDate}', 'YYYY-MM-DD')
                //AND DPO.DELETED_FLAG = 'N'
                //{flagFilter}
                //                   {brachFilter1} {customerFilter}{SalesPersonFilter}
                //) A
                //,(
                //SELECT ITEM_CODE
                //,APP_DATE
                //,SALES_RATE
                //,COMPANY_CODE
                //,BRANCH_CODE
                //FROM IP_ITEM_RATE_APPLICAT_SETUP D
                //WHERE 1 = 1
                //AND APP_DATE = (
                //SELECT MAX(APP_DATE)
                //FROM IP_ITEM_RATE_APPLICAT_SETUP
                //WHERE ITEM_CODE = D.ITEM_CODE
                //AND BRANCH_CODE = D.BRANCH_CODE
                //AND COMPANY_CODE = D.COMPANY_CODE
                //)
                //) B
                //WHERE A.ITEM_CODE = B.ITEM_CODE
                //AND A.cpp = B.COMPANY_CODE
                //AND A.bpp = B.BRANCH_CODE
                //)
                //GROUP BY ORDER_NO
                //,ORDER_DATE
                //,MITI
                //,GROUP_NAME
                //,CUSTOMER_CODE
                //,PARTY_TYPE_CODE
                //,APPROVED_FLAG
                //,DISPATCH_FLAG
                //,CUSTOMER_EDESC
                //,EMPLOYEE_EDESC
                //,REMARKS
                //,COMPANY_EDESC
                //,DELETED_FLAG
                //,CREATED_DATE
                //,CREATED_BY
                //,ACKNOWLEDGE_FLAG
                //,REJECT_FLAG
                //,CREDIT_LIMIT
                //,TOTAL_APPROVE_QTY
                //,TOTAL_APPROVE_AMT
                //,TOTAL_AMOUNT
                //,QUANTITY
                //,cpp
                //,bpp
                //,CS_PARTY
                //,PO_PARTY_TYPE
                //,PO_CONVERSION_UNIT
                //,PO_CONVERSION_FACTOR
                //,SO_CREDIT_LIMIT_FLAG
                //,SO_CREDIT_DAYS_FLAG
                //,PO_CUSTOM_RATE
                //ORDER BY ORDER_NO
                //) X
                //GROUP BY X.ORDER_NO
                //,X.COMPANY_CODE
                //,X.COMPANY_EDESC
                //,x.GROUP_EDESC
                //,X.CUSTOMER_CODE
                //,X.CUSTOMER_EDESC
                //,X.EMPLOYEE_EDESC
                //,X.REMARKS
                //,X.RESELLER_NAME
                //,X.ORDER_ENTITY
                //,X.MITI
                //,X.PARTY_TYPE_CODE
                //,X.PARTY_TYPE_EDESC
                //,X.CREATED_BY
                //,X.CREATED_DATE
                //,X.DELETED_FLAG
                //,X.BRANCH_CODE
                //,X.APPROVED_FLAG
                //,X.DISPATCH_FLAG
                //,X.ACKNOWLEDGE_FLAG
                //,X.REJECT_FLAG
                //,X.PO_PARTY_TYPE
                //,X.PO_CONVERSION_UNIT
                //,X.PO_CONVERSION_FACTOR
                //,X.SO_CREDIT_LIMIT_FLAG
                //,X.SO_CREDIT_DAYS_FLAG
                //,X.PO_CUSTOM_RATE
                //,X.CREDIT_LIMIT
                //ORDER BY X.ORDER_NO DESC";
                //            }




            }
            var data = _objectEntity.SqlQuery<PurchaseOrderModel>(query).ToList();
            //foreach(var item in data)
            //{
            //    string tempQuery = @"SELECT NVL((SUM (V.DR_AMOUNT) - SUM (V.CR_AMOUNT)),0) balance FROM V$VIRTUAL_SUB_LEDGER V WHERE V.COMPANY_CODE in('"+userInfo.company_code+"') AND V.SUB_CODE ='C" + item.CUSTOMER_CODE + "'";
            //    var tempData = _objectEntity.SqlQuery<decimal>(tempQuery).FirstOrDefault();
            //    item.balance = tempData;
            //}

            //""
            return data;

        }

        public List<PurchaseOrderModel> GetPurchaseOrderSummary(ReportFiltersModel model, User userInfo, string requestStatus)
        {
            var companyCode = model.CompanyFilter.Count > 0 ? "'" + string.Join("','", model.CompanyFilter) + "'" : userInfo.company_code;
            var flagFilter = "";
            if (requestStatus == "Approved")
                flagFilter = @" AND DPO.APPROVED_FLAG = 'Y'
                               AND DPO.REJECT_FLAG = 'N'";
            else if (requestStatus == "Rejected")
                flagFilter = @" AND DPO.REJECT_FLAG = 'Y'
                                AND DPO.APPROVED_FLAG = 'N'";
            else if (requestStatus == "All")
            {
                flagFilter = "";
            }
            else if (requestStatus == "Active")
            {
                flagFilter = @" AND DPO.REJECT_FLAG = 'N'
                                AND DPO.APPROVED_FLAG = 'N'";
            }
            string brachFilter1 = "";
            string brachFilter2 = "";
            if (model.BranchFilter.Count() > 0)
            {
                brachFilter1 = " and DPO.BRANCH_CODE IN('" + string.Join("', '", model.BranchFilter) + "')";
                brachFilter2 = " and VSL.BRANCH_CODE IN('" + string.Join("', '", model.BranchFilter) + "')";
            }

            var customerFilter = string.Empty;
            if (model.CustomerFilter.Count() > 0)
            {
                customerFilter = @"select  DISTINCT(customer_code) from sa_customer_setup where (";
                //IF CUSTOMER_SKU_FLAG = G
                foreach (var item in model.CustomerFilter)
                {
                    customerFilter += "master_customer_code like  (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + companyCode + ")) OR ";
                }
                customerFilter = customerFilter.Substring(0, customerFilter.Length - 3);
                //IF CUSTOMER_SKU_FLAG = I                
                customerFilter += " or (customer_code in (" + string.Join(",", model.CustomerFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + companyCode + "))) ";


                customerFilter = " and CS.customer_code IN(" + customerFilter + ")";
            }
            var SalesPersonFilter = string.Empty;
            if (model.ItemBrandFilter.Count > 0)
                SalesPersonFilter = $" AND  ES.EMPLOYEE_CODE IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                SalesPersonFilter = $" AND ES.EMPLOYEE_CODE IN  ({userInfo.sp_codes})";
            var partyTypeCodeQuery = $@"SELECT  PARTY_TYPE_CODE FROM FA_SUB_LEDGER_DEALER_MAP
            WHERE CUSTOMER_CODE = '{userInfo.DistributerNo}'";
            var partyTypeCode = this._objectEntity.SqlQuery<string>(partyTypeCodeQuery).FirstOrDefault();

            string query = $@"SELECT ORDER_NO
,ORDER_DATE
,TO_BS(ORDER_DATE) MITI
,DPO.CUSTOMER_CODE
,SCS.CUSTOMER_EDESC
,DPO.REJECT_FLAG
,DPO.DELETED_FLAG
,DPO.APPROVED_FLAG
,DPO.DISPATCH_FLAG
,DPO.ACKNOWLEDGE_FLAG
,DPO.PARTY_TYPE_CODE
,DPO.TOTAL_PRICE TOTAL_AMOUNT
,DPO.APPROVE_QTY TOTAL_APPROVE_QTY
,DPO.APPROVE_AMT TOTAL_APPROVE_AMT
,DPO.CREATED_BY
,DPO.CREATED_DATE
,APPROVE_AMT
,ITEM_CODE
,CS.COMPANY_EDESC 
,DPO.BRANCH_CODE bpp
,DPO.QUANTITY
,DPO.REMARKS
,CASE WHEN DPO.APPROVED_FLAG ='Y' THEN 'APPROVED' 
     WHEN DPO.REJECT_FLAG ='Y' THEN 'REJECT' 
     ELSE 'PENDING' END AS ORDER_ENTITY
FROM DIST_IP_SSD_PURCHASE_ORDER DPO
LEFT JOIN COMPANY_SETUP CS ON CS.COMPANY_CODE=DPO.COMPANY_CODE
LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE= DPO.CUSTOMER_CODE
LEFT JOIN FA_SUB_LEDGER_DEALER_MAP  FSLDM ON FSLDM.CUSTOMER_CODE = SCS.CUSTOMER_CODE AND SCS.COMPANY_CODE= FSLDM.COMPANY_CODE
WHERE  TRUNC(DPO.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'YYYY-MM-DD')
AND TO_DATE('{model.ToDate}', 'YYYY-MM-DD')
AND DPO.DELETED_FLAG = 'N'
AND FSLDM.PARTY_TYPE_CODE='{partyTypeCode}' ORDER BY ORDER_NO DESC";




            var data = _objectEntity.SqlQuery<PurchaseOrderModel>(query).ToList();
            //foreach(var item in data)
            //{
            //    string tempQuery = @"SELECT NVL((SUM (V.DR_AMOUNT) - SUM (V.CR_AMOUNT)),0) balance FROM V$VIRTUAL_SUB_LEDGER V WHERE V.COMPANY_CODE in('"+userInfo.company_code+"') AND V.SUB_CODE ='C" + item.CUSTOMER_CODE + "'";
            //    var tempData = _objectEntity.SqlQuery<decimal>(tempQuery).FirstOrDefault();
            //    item.balance = tempData;
            //}

            //""
            return data;

        }

        public List<DistSalesReturnViewModel> GetDistributionSalesReturn(ReportFiltersModel reportFilters, string companyCode)
        {
            try
            {

                var distSalesReturnQuery = $@"SELECT DISTINCT singleRow.RETURN_NO,dlu.FULL_NAME AS ASM_NAME,singleRow.RETURN_DATE,singleRow.COMPANY_CODE,singleRow.CUSTOMER_CODE,SC.CUSTOMER_EDESC,
                                                ddm.AREA_CODE,dam.AREA_NAME as AREA,singleRow.CUSTOMER_TYPE AS ENTITY_TYPE,
                                                SC.REGD_OFFICE_EADDRESS as ADDRESS,SC.TEL_MOBILE_NO1 as CONTACT_NUMBER,
                                                FN_FETCH_DESC(singleRow.COMPANY_CODE,'COMPANY_SETUP', 'COMPANY_EDESC') as COMPANY_EDESC,
                                                FN_FETCH_DESC(singleRow.COMPANY_CODE,'FA_BRANCH_SETUP',singleRow.BRANCH_CODE) as BRANCH_EDESC,
                                                dam.area_code,singleRow.RETRUN_CONDITIONS as CONDITION,singleRow.COMPLAIN_TYPE,singleRow.COMPLAIN_SERIOUSNESS as SERIOUSNESS,
                                                singleRow.DISTRIBUTOR_REMARKS as REMARKS_DIST,singleRow.ASM_REMARKS as REMARKS_ASM,singleRow.APPROVED_FLAG,singleRow.CREATED_DATE
                                                FROM DIST_SALES_RETURN singleRow
                                                INNER JOIN SA_CUSTOMER_SETUP SC on SC.CUSTOMER_CODE = singleRow.CUSTOMER_CODE
                                                INNER JOIN dist_distributor_master ddm on ddm.DISTRIBUTOR_CODE = SC.CUSTOMER_CODE
                                                INNER JOIN dist_area_master dam on dam.AREA_CODE = ddm.AREA_CODE
                                                INNER JOIN dist_login_user dlu on dlu.USERID = singleRow.CREATED_BY
                                                WHERE singleRow.COMPANY_CODE='{companyCode}' AND 
                                                TRUNC(singleRow.RETURN_DATE) BETWEEN TO_DATE('{reportFilters.FromDate}', 'YYYY-MON-DD')
                                                AND TO_DATE('{reportFilters.ToDate}', 'YYYY-MON-DD')
                                                AND singleRow.DELETED_FLAG = 'N'";

                var distSalesReturnData = _objectEntity.SqlQuery<DistSalesReturnViewModel>(distSalesReturnQuery).ToList();


                return distSalesReturnData;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public List<DistSalesReturnItemViewModel> GetSalesReturnDetail(ReportFiltersModel reportFilters, string CompanyCode, string orderCode)
        {
            try
            {
                var distSalesReturnQuery = $@"SELECT distinct DST.COMPANY_CODE,DST.SYN_ROWID, DST.RETURN_NO, DST.ITEM_CODE,IIMS.ITEM_EDESC
                                               ,TO_CHAR(DST.QUANTITY) QUANTITY,DST.BATCH_NO,NVL(DST.EXPIRY_DATE,SYSDATE) as EXP_DATE,DST.COMPLAIN_TYPE
                                              ,DST.DISTRIBUTOR_REMARKS as REMARKS_DIST,DST.ASM_REMARKS as REMARKS_ASM
                                              ,DST.COMPLAIN_TYPE,DST.MU_CODE,DST.CREATED_DATE,DST.MFD_DATE
                                            FROM DIST_SALES_RETURN DST
                                            INNER JOIN IP_ITEM_MASTER_SETUP IIMS on IIMS.ITEM_CODE = DST.ITEM_CODE AND IIMS.COMPANY_CODE = DST.COMPANY_CODE
                                            WHERE DST.COMPANY_CODE='{CompanyCode}' AND DST.RETURN_NO='{orderCode}' AND 
                                                TRUNC(DST.RETURN_DATE) BETWEEN TO_DATE('{reportFilters.FromDate}', 'YYYY-MON-DD')
                                                AND TO_DATE('{reportFilters.ToDate}', 'YYYY-MON-DD')
                                                AND DST.DELETED_FLAG = 'N'";

                var distSalesReturnData = _objectEntity.SqlQuery<DistSalesReturnItemViewModel>(distSalesReturnQuery).ToList();
                return distSalesReturnData;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public DSRResponse UpdateDistSalesReturn(string returnNo, string updateFlag)
        {
            try
            {
                var response = new DSRResponse();
                if (updateFlag == "cancel")
                {
                    string updateQuery = $@"UPDATE DIST_SALES_RETURN SET APPROVED_FLAG='C' WHERE RETURN_NO='{returnNo}'";
                    var updatedRow = _objectEntity.ExecuteSqlCommand(updateQuery);
                    if (updatedRow > 0)
                    {
                        response.UpdateFlag = "cancel";
                        response.UpdateResponse = "Sales Return Cancled Successfully";
                    }
                    else
                    {
                        response.UpdateFlag = "cancel";
                        response.UpdateResponse = "No sales return to cancel";
                    }

                }
                else
                {
                    string updateQuery = $@"UPDATE DIST_SALES_RETURN SET APPROVED_FLAG='A' WHERE RETURN_NO='{returnNo}'";
                    var updatedRow = _objectEntity.ExecuteSqlCommand(updateQuery);
                    if (updatedRow > 0)
                    {
                        response.UpdateFlag = "approved";
                        response.UpdateResponse = "Sales Return Approved Successfully";
                    }
                    else
                    {
                        response.UpdateFlag = "approved";
                        response.UpdateResponse = "No sales return to approved";
                    }

                }

                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public List<DistSalesReturnViewModel> FilterDistSalesReturn(string value)
        {
            try
            {
                var distSalesReturnQuery = $@"SELECT distinct DST.RETURN_NO,DST.RETURN_DATE,DST.CUSTOMER_CODE,SC.CUSTOMER_EDESC,SC.REGD_OFFICE_EADDRESS as ADDRESS,SC.TEL_MOBILE_NO1 as CONTACT_NUMBER,
                                                   ddm.AREA_CODE,dam.AREA_NAME as AREA,
                                                   DST.ITEM_CODE,IIMS.ITEM_EDESC,DST.QUANTITY,DST.APPROVED_FLAG,
                                                   DST.BATCH_NO,DST.EXPIRY_DATE,DST.COMPANY_CODE,DST.BRANCH_CODE,FN_FETCH_DESC(
                                                  DST.COMPANY_CODE,'COMPANY_SETUP', 'COMPANY_EDESC') as COMPANY,
                                                  FN_FETCH_DESC(
                                                  DST.COMPANY_CODE,'FA_BRANCH_SETUP',DST.BRANCH_CODE) as BRANCH,DST.RETRUN_CONDITIONS as CONDITION,DST.COMPLAIN_TYPE,DST.COMPLAIN_SERIOUSNESS as SERIOUSNESS,DST.DISTRIBUTOR_REMARKS as REMARKS_DIST,DST.ASM_REMARKS as REMARKS_ASM
                                              FROM DIST_SALES_RETURN DST
                                              INNER JOIN SA_CUSTOMER_SETUP SC on SC.CUSTOMER_CODE = DST.CUSTOMER_CODE
                                              INNER JOIN IP_ITEM_MASTER_SETUP IIMS on IIMS.ITEM_CODE = DST.ITEM_CODE
                                              INNER JOIN dist_distributor_master ddm on ddm.DISTRIBUTOR_CODE = SC.CUSTOMER_CODE
                                              INNER JOIN dist_area_master dam on dam.AREA_CODE = ddm.AREA_CODE
                                              WHERE DST.APPROVED_FLAG='{value}'";

                var distSalesReturnData = _objectEntity.SqlQuery<DistSalesReturnViewModel>(distSalesReturnQuery).ToList();

                var distSalesReturnDetQuery = $@"SELECT distinct DST.COMPANY_CODE,DST.SYN_ROWID, TO_CHAR(DST.RETURN_NO) RETURN_NO, DST.ITEM_CODE,IIMS.ITEM_EDESC
                                               ,TO_CHAR(DST.QUANTITY) QUANTITY,DST.BATCH_NO,TO_CHAR(DST.EXPIRY_DATE,'DD-MON-YY') as EXP_DATE,DST.COMPLAIN_TYPE
                                              ,DST.DISTRIBUTOR_REMARKS as REMARKS_DIST,DST.ASM_REMARKS as REMARKS_ASM
                                              ,DST.COMPLAIN_TYPE
                                            FROM DIST_SALES_RETURN DST
                                            INNER JOIN IP_ITEM_MASTER_SETUP IIMS on IIMS.ITEM_CODE = DST.ITEM_CODE
                                            WHERE DST.APPROVED_FLAG='{value}'";

                var distSalesReturnDetData = _objectEntity.SqlQuery<DistSalesReturnItemViewModel>(distSalesReturnDetQuery).ToList();
                foreach (var sr in distSalesReturnData)
                {
                    sr.ReturnItemList = distSalesReturnDetData;
                }
                return distSalesReturnData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<PurchaseOrderModel> GetPurchaseOrderDaily(ReportFiltersModel model, User userInfo)
        {
            var companyCode = model.CompanyFilter.Count > 0 ? "'" + string.Join("','", model.CompanyFilter) + "'" : userInfo.company_code;
            string query = $@"SELECT DISTINCT * FROM (SELECT SSD.ORDER_NO,IM.ITEM_EDESC,CS.CUSTOMER_EDESC, SSD.ORDER_DATE, SSD.CUSTOMER_CODE,SSD.BILLING_NAME,SSD.ITEM_CODE,SSD.MU_CODE,SSD.QUANTITY, SSD.UNIT_PRICE,SSD.TOTAL_PRICE ,'Distributor' TYPE
                                    FROM DIST_IP_SSD_PURCHASE_ORDER SSD,SA_CUSTOMER_SETUP CS,IP_ITEM_MASTER_SETUP IM, DIST_LOGIN_USER LU,DIST_LM_LOCATION_TRACKING LT
                                    WHERE SSD.CUSTOMER_CODE = CS.CUSTOMER_CODE(+)
                                              AND  SSD.COMPANY_CODE = CS.COMPANY_CODE(+)   
                                              AND  LU.USERID=SSD.CREATED_BY AND LU.COMPANY_CODE=SSD.COMPANY_CODE
                                              AND LU.ACTIVE = 'Y'
                                              AND SSD.ITEM_CODE = IM.ITEM_CODE
                                                AND LT.SP_CODE=LU.SP_CODE and LT.COMPANY_CODE=SSD.COMPANY_CODE AND LT.TRACK_TYPE='EOD'
                                             AND SSD.ORDER_DATE >=TO_DATE('{model.FromDate}','YYYY-MM-DD') AND SSD.ORDER_DATE <=TO_DATE('{model.ToDate}','YYYY-MM-DD')
                                    UNION ALL
                                    SELECT DS.ORDER_NO,IM.ITEM_EDESC,CS.CUSTOMER_EDESC, DS.ORDER_DATE, DS.CUSTOMER_CODE,DS.BILLING_NAME,DS.ITEM_CODE,DS.MU_CODE,DS.QUANTITY, DS.UNIT_PRICE,DS.TOTAL_PRICE ,'Reseller' TYPE
                                    FROM DIST_IP_SSR_PURCHASE_ORDER DS, DIST_LOGIN_USER LU,SA_CUSTOMER_SETUP CS,IP_ITEM_MASTER_SETUP IM,DIST_PHOTO_INFO DF,DIST_LM_LOCATION_TRACKING LT
                                     WHERE DS.CUSTOMER_CODE = CS.CUSTOMER_CODE AND CS.COMPANY_CODE =DS.COMPANY_CODE AND DS.ITEM_CODE = IM.ITEM_CODE
                                     AND LU.USERID=DS.CREATED_BY AND LU.COMPANY_CODE=DS.COMPANY_CODE
                                    AND LU.ACTIVE = 'Y'
                                     AND LT.SP_CODE=LU.SP_CODE AND LT.TRACK_TYPE='EOD'
                                  AND DS.ORDER_DATE >=TO_DATE('{model.FromDate}','YYYY-MM-DD') AND DS.ORDER_DATE <=TO_DATE('{model.ToDate}','YYYY-MM-DD')
                                  )
                                     ORDER BY CUSTOMER_EDESC ASC";

            var data = _objectEntity.SqlQuery<PurchaseOrderModel>(query).ToList();
            return data;

        }

        public List<ItemModel> FetchItems(string CompanyCode, string BranchCode, string DistCode)
        {
            string ItemsQuery = string.Empty;
            string SalesRateClause = string.Empty;
            string ConversionClause = string.Empty;
            string DistributerFilter = string.Empty;

            var PO = _objectEntity.SqlQuery<PreferenceSetupModel>($"SELECT * FROM DIST_PREFERENCE_SETUP WHERE COMPANY_CODE='{CompanyCode}'").FirstOrDefault();
            if (PO.PO_SYN_RATE == "Y")
                SalesRateClause = "AND SALES_RATE IS NOT NULL AND SALES_RATE <> 0";
            if (PO.SQL_NN_CONVERSION_UNIT_FACTOR == "Y")
                ConversionClause = "AND IUS.MU_CODE IS NOT NULL AND IUS.CONVERSION_FACTOR IS NOT NULL";
            if (!string.IsNullOrEmpty(DistCode))
                DistributerFilter = $"LEFT JOIN DIST_DISTRIBUTOR_ITEM DDI ON ITEMS.ITEM_CODE=DDI.ITEM_CODE WHERE DDI.DISTRIBUTOR_CODE='{DistCode}'";

            ItemsQuery = $@"SELECT * FROM (SELECT IM.ITEM_CODE, IM.ITEM_EDESC, ISS.BRAND_NAME, IM.INDEX_MU_CODE AS UNIT, MC.MU_EDESC, IUS.MU_CODE CONVERSION_UNIT,
                TO_CHAR(IUS.CONVERSION_FACTOR) AS CONVERSION_FACTOR, TO_CHAR(NVL(IR.SALES_RATE, 0)) SALES_RATE, TO_CHAR(IR.APPLY_DATE) AS APPLY_DATE
                FROM IP_ITEM_MASTER_SETUP IM
                  INNER JOIN IP_MU_CODE MC ON MC.MU_CODE = IM.INDEX_MU_CODE AND MC.COMPANY_CODE = IM.COMPANY_CODE
                  INNER JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = IM.ITEM_CODE AND ISS.COMPANY_CODE = IM.COMPANY_CODE AND TRIM(ISS.BRAND_NAME) IS NOT NULL
                  LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON IUS.ITEM_CODE = ISS.ITEM_CODE AND IUS.COMPANY_CODE = ISS.COMPANY_CODE
                  LEFT JOIN (SELECT A.ITEM_CODE, A.APPLY_DATE, B.SALES_RATE, B.COMPANY_CODE
                              FROM (SELECT ITEM_CODE, COMPANY_CODE, MAX(APP_DATE) APPLY_DATE 
                                FROM IP_ITEM_RATE_APPLICAT_SETUP
                                WHERE COMPANY_CODE = '{CompanyCode}' 
                                AND BRANCH_CODE = '{BranchCode}'
                                GROUP BY ITEM_CODE, COMPANY_CODE) A
                              INNER JOIN IP_ITEM_RATE_APPLICAT_SETUP B
                                ON B.ITEM_CODE = A.ITEM_CODE
                                AND B.APP_DATE = A.APPLY_DATE
                                AND B.COMPANY_CODE = '{CompanyCode}'
                                AND B.BRANCH_CODE = '{BranchCode}') IR 
                    ON IR.ITEM_CODE = IM.ITEM_CODE AND IR.COMPANY_CODE = IM.COMPANY_CODE
                WHERE IM.COMPANY_CODE = '{CompanyCode}' AND IM.CATEGORY_CODE = 'FG' AND IM.GROUP_SKU_FLAG = 'I' AND IM.DELETED_FLAG = 'N'
                {SalesRateClause}
                {ConversionClause} ) ITEMS
                {DistributerFilter}
                ORDER BY UPPER(ITEMS.ITEM_EDESC) ASC";
            var Items = _objectEntity.SqlQuery<ItemModel>(ItemsQuery).ToList();
            return Items;
        }

        public List<ItemModel> FetchAllItems(User userInfo, string type)
        {
            string module = "";
            if (type.Trim().ToUpper() == "B")
                module = "Branding";
            else if (type.Trim().ToUpper() == "C")
                module = "Comp";
            else
                module = "Distribution";
            string item_filter_condition = "";
            try
            {
                var url = System.Web.HttpContext.Current.Request.Url.AbsoluteUri;
                string xmlpath = HttpContext.Current.Server.MapPath("~/Areas/NeoERP.Distribution/ProductCondition.xml");
                var xml = XDocument.Load(xmlpath);
                var condition_query = from c in xml.Root.Descendants("Vendor")
                                      where (string)c.Attribute("ModuleName") == module
                                      select c.Element("ConditionQuery").Value;
                var result = condition_query.FirstOrDefault();
                if (result != null)
                {
                    item_filter_condition = result;
                }
            }
            catch (Exception)
            {
                item_filter_condition = "";
            }
            string query = "";

            if (type.Trim().ToUpper() == "C")
                query = $@"SELECT IMS.ITEM_CODE, IMS.ITEM_EDESC, ISS.BRAND_NAME, IMS.INDEX_MU_CODE AS UNIT
                    FROM IP_ITEM_MASTER_SETUP IMS,IP_ITEM_SPEC_SETUP ISS WHERE
                        ISS.ITEM_CODE = IMS.ITEM_CODE(+) AND ISS.COMPANY_CODE = IMS.COMPANY_CODE
                    AND IMS.COMPANY_CODE ='{userInfo.company_code}'
                                AND IMS.DELETED_FLAG = 'N' {item_filter_condition}";
            else
                query = $@"SELECT IMS.ITEM_CODE, IMS.ITEM_EDESC, ISS.BRAND_NAME, IMS.INDEX_MU_CODE AS UNIT
                    FROM IP_ITEM_MASTER_SETUP IMS
                    INNER JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = IMS.ITEM_CODE AND ISS.COMPANY_CODE = IMS.COMPANY_CODE --AND TRIM(ISS.BRAND_NAME) IS NOT NULL
                    WHERE IMS.COMPANY_CODE ='{userInfo.company_code}'
                                AND IMS.DELETED_FLAG = 'N'
                                AND IMS.GROUP_SKU_FLAG='I'
                                {item_filter_condition}";
            var productList = _objectEntity.SqlQuery<ItemModel>(query).ToList();
            return productList;
        }

        public List<CustomerModel> CompItems(User userInfo)
        {
            //var query = $@"SELECT ITEM_CODE CODE,ITEM_EDESC NAME FROM IP_ITEM_MASTER_SETUP WHERE ITEM_CODE IN
            //            (SELECT DISTINCT ITEM_CODE FROM DIST_COMP_ITEM_MAP) AND COMPANY_CODE = '{userInfo.company_code}'
            //             ORDER BY ITEM_CODE";
            var query = $@"SELECT DISTINCT ITEM_CODE CODE,ITEM_EDESC NAME FROM IP_ITEM_MASTER_SETUP
            WHERE MASTER_ITEM_CODE IN
                     (SELECT DISTINCT PRE_ITEM_CODE FROM IP_ITEM_MASTER_SETUP
                                 WHERE ITEM_CODE IN (SELECT DISTINCT ITEM_CODE FROM DIST_COMP_ITEM_MAP WHERE COMPANY_CODE = '{userInfo.company_code}')
                                 AND COMPANY_CODE = '{userInfo.company_code}')
            AND GROUP_SKU_FLAG = 'G' AND COMPANY_CODE = '{userInfo.company_code}'
            ORDER BY ITEM_CODE";
            var data = _objectEntity.SqlQuery<CustomerModel>(query).ToList();
            return data;
        }
        public List<PurchaseOrderModel> GetPurchaseOrderDetail(string CompanyCode, string orderCode, string ORDER_ENTITY, string requestStatus)
        {
            var flagFilter = "";
            if (requestStatus == "Approved")
            {
                if (ORDER_ENTITY == "R")
                {
                    flagFilter = @" --AND DPO.approved_flag = 'Y'    
                                    AND DPO.APPROVE_QTY<>0 
                               AND DPO.REJECT_FLAG = 'N'";
                }
                else
                {
                    flagFilter = @" --AND RESULT.approved_flag = 'Y'    
                                    AND RESULT.APPROVEQTY<>0 
                               AND RESULT.REJECT_FLAG = 'N'";
                }

            }

            //OldRunning
            // flagFilter = @" AND DPO.approved_flag = 'Y'
            //               AND DPO.REJECT_FLAG = 'N'";
            else if (requestStatus == "Rejected")
                flagFilter = @" AND RESULT.REJECT_FLAG = 'Y'
                                AND RESULT.approved_flag = 'N'";
            //oldRunning
            //flagFilter = @" AND DPO.REJECT_FLAG = 'Y'
            //              AND DPO.approved_flag = 'N'";
            else if (requestStatus == "All")
            {
                flagFilter = "";
            }
            else if (requestStatus == "Active" && ORDER_ENTITY != "APPROVED")
            {
                if (ORDER_ENTITY == "R")
                {
                    //AA
                    flagFilter = @" AND REJECT_FLAG = 'N'
                                AND QUANTITY <>0
                                AND APPROVED_FLAG = 'N'";


                    //flagFilter = @" AND DPO.REJECT_FLAG = 'N'
                    //            AND DPO.QUANTITY <>0
                    //            AND DPO.APPROVED_FLAG = 'N'";
                }
                else
                {
                    flagFilter = @" AND RESULT.REJECT_FLAG = 'N'
                                AND RESULT.QUANTITY <>0
                                AND RESULT.APPROVED_FLAG = 'N'";
                }

                //oldRunning
                //flagFilter = @" AND DPO.REJECT_FLAG = 'N'
                //              AND DPO.APPROVED_FLAG = 'N'";
            }
            else if (ORDER_ENTITY== "APPROVED")
            {
                flagFilter = @" AND RESULT.REJECT_FLAG = 'N'
                                --AND RESULT.QUANTITY <>0
                                AND RESULT.APPROVED_FLAG = 'Y'";
            }
            //OLD QUERY
            //var query = $@"SELECT DPO.ORDER_NO, DPO.ORDER_DATE, DPO.PARTY_TYPE_CODE, 
            //                        PTC.PARTY_TYPE_EDESC, DPO.CUSTOMER_CODE, CS.CUSTOMER_EDESC, 
            //                        DPO.ITEM_CODE,TRIM(IMS.ITEM_EDESC) ITEM_EDESC, DPO.MU_CODE, NVL(DPO.QUANTITY,0) QUANTITY, NVL(DPO.APPROVE_QTY,0) APPROVEQTY,
            //                                  NVL(DPO.UNIT_PRICE,0) UNIT_PRICE, IUS.MU_CODE CONVERSION_MU_CODE, 
            //                        IUS.CONVERSION_FACTOR,
            //                                  DPO.TOTAL_PRICE, DPO.REMARKS, DPO.CREATED_BY, DPO.CREATED_DATE, 
            //                        DPO.MODIFY_DATE, DPO.DELETED_FLAG, DPO.CURRENCY_CODE, DPO.EXCHANGE_RATE,
            //                                  DPO.COMPANY_CODE, DPO.BRANCH_CODE,
            //                                  DPO.APPROVED_FLAG, DPO.DISPATCH_FLAG, DPO.ACKNOWLEDGE_FLAG, 
            //                        DPO.REJECT_FLAG, DPO.SYN_ROWID, DPO.MODIFY_DATE, DPO.MODIFY_BY,
            //                                  TRIM(IMS.ITEM_EDESC),
            //                                  ES.EMPLOYEE_EDESC,
            //                                  PS.PO_PARTY_TYPE,
            //                                  PS.PO_CONVERSION_FACTOR,
            //                                  PS.PO_BILLING_NAME,
            //                                  PS.PO_CUSTOM_RATE, PS.PO_REMARKS, PS.PO_CONVERSION_UNIT, 
            //                        PS.PO_CONVERSION_FACTOR, PS.CS_CONVERSION_UNIT,
            //                                  CS.CREDIT_LIMIT
            //                        FROM DIST_IP_SSD_PURCHASE_ORDER DPO
            //                        INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DPO.ITEM_CODE AND 
            //                        IMS.COMPANY_CODE = DPO.COMPANY_CODE AND IMS.CATEGORY_CODE = 'FG' AND 
            //                        IMS.GROUP_SKU_FLAG = 'I'
            //                        LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DPO.CUSTOMER_CODE 
            //                        AND CS.COMPANY_CODE = DPO.COMPANY_CODE AND CS.DELETED_FLAG = 'N'
            //                        INNER JOIN DIST_LOGIN_USER LU ON LU.USERID = DPO.CREATED_BY
            //                        INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = LU.SP_CODE AND 
            //                        ES.COMPANY_CODE = LU.COMPANY_CODE
            //                        LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON IUS.ITEM_CODE = DPO.ITEM_CODE AND 
            //                        IUS.COMPANY_CODE = DPO.COMPANY_CODE
            //                        INNER JOIN DIST_PREFERENCE_SETUP PS ON PS.COMPANY_CODE = 
            //                        DPO.COMPANY_CODE
            //                        LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = 
            //                        DPO.PARTY_TYPE_CODE AND PTC.PARTY_TYPE_CODE = DPO.COMPANY_CODE AND 
            //                        PTC.DELETED_FLAG = 'N'
            //                        WHERE 1 = 1
            //                          {flagFilter}
            //                           AND DPO.DELETED_FLAG = 'N'
            //                           AND DPO.ORDER_NO = '{orderCode}'
            //                        ORDER BY UPPER(TRIM(IMS.ITEM_EDESC))";


            var query = "";
            var query1 = "";
            var newQuery = "";
            var dynamicRate = "";
            var PO = _objectEntity.SqlQuery<PreferenceSetupModel>($"SELECT * FROM DIST_PREFERENCE_SETUP WHERE COMPANY_CODE='01'").FirstOrDefault();
            if (PO.SO_REPO_RATE_TABLE == "DIST_IP_SSD_PURCHASE_ORDER" && PO.SO_REPO_RATE_COLUMN == "UNIT_PRICE")
            {
                if (requestStatus == "Approved") dynamicRate = "NVL(RESULT.UNIT_PRICE,0) NEW_RATE,NVL(RESULT.UNIT_PRICE, 0) * RESULT.APPROVEQTY NEW_TOTAL_AMOUNT,";
                else dynamicRate = "NVL(RESULT.UNIT_PRICE,0) NEW_RATE,NVL(RESULT.UNIT_PRICE, 0) * RESULT.QUANTITY NEW_TOTAL_AMOUNT,";

            }
            else
            {
                if (requestStatus == "Approved") 
                    dynamicRate = "NVL(IRA.SALES_RATE,0) NEW_RATE,NVL(IRA.SALES_RATE, 0) * RESULT.APPROVEQTY NEW_TOTAL_AMOUNT,";
                else dynamicRate = "NVL(IRA.SALES_RATE,0) NEW_RATE,NVL(IRA.SALES_RATE, 0) * RESULT.QUANTITY NEW_TOTAL_AMOUNT,";

            }
            if (PO.PO_DIST_RATE_COLUMN == "SALES_RATE_ZERO")
            {
                newQuery = $@"SELECT 
                                {dynamicRate}
                                --IRA.APP_DATE,
                                RESULT.*
                                --,IRA.BRANCH_CODE rate_branch 
                                , RESULT.BRANCH_CODE order_branch
                                FROM (SELECT DPO.ORDER_NO,
                                DPO.ORDER_DATE,
                                DPO.PARTY_TYPE_CODE,
                                PTC.PARTY_TYPE_EDESC,
                                DPO.CUSTOMER_CODE,
                                CS.CUSTOMER_EDESC,
                                DPO.ITEM_CODE,
                                TRIM (IMS.ITEM_EDESC) ITEM_EDESC,
                                DPO.MU_CODE,
                                NVL (DPO.QUANTITY, 0) QUANTITY,
                                NVL (DPO.APPROVE_QTY, 0) APPROVEQTY,
                                NVL (DPO.UNIT_PRICE, 0) UNIT_PRICE,
                                --NVL(ITRAS.SALES_RATE,0) NEW_UNIT_PRICE, --aaku
                                IUS.MU_CODE CONVERSION_MU_CODE,
                                CASE (DPO.MU_CODE)
                                WHEN IUS.MU_CODE THEN 1
                                ELSE IUS.CONVERSION_FACTOR
                                END
                                CONVERSION_FACTOR,
                                DPO.TOTAL_PRICE,
                                DPO.REMARKS,
                                DPO.CREATED_BY,
                                DPO.CREATED_DATE,
                                DPO.MODIFY_DATE,
                                DPO.DELETED_FLAG,
                                DPO.CURRENCY_CODE,
                                DPO.EXCHANGE_RATE,
                                DPO.COMPANY_CODE,
                                DPO.BRANCH_CODE,
                                DPO.APPROVED_FLAG,
                                DPO.DISPATCH_FLAG,
                                DPO.ACKNOWLEDGE_FLAG,
                                DPO.REJECT_FLAG,
                                DPO.SYN_ROWID--,DPO.MODIFY_DATE
                                ,
                                DPO.MODIFY_BY,
                                TRIM (IMS.ITEM_EDESC),
                                ES.EMPLOYEE_EDESC,
                                PS.PO_PARTY_TYPE,
                                PS.PO_CONVERSION_FACTOR,
                                PS.PO_BILLING_NAME,
                                PS.PO_CUSTOM_RATE,
                                PS.PO_REMARKS,
                                PS.PO_CONVERSION_UNIT--,PS.PO_CONVERSION_FACTOR
                                ,
                                PS.CS_CONVERSION_UNIT,
                                CS.CREDIT_LIMIT
                                FROM DIST_IP_SSD_PURCHASE_ORDER DPO
                                INNER JOIN IP_ITEM_MASTER_SETUP IMS
                                ON IMS.ITEM_CODE = DPO.ITEM_CODE
                                AND IMS.COMPANY_CODE = DPO.COMPANY_CODE
                                AND IMS.CATEGORY_CODE IN
                                (SELECT CATEGORY_CODE
                                FROM IP_CATEGORY_CODE
                                WHERE CATEGORY_TYPE IN ('FG', 'TF'))
                                AND IMS.GROUP_SKU_FLAG = 'I'
                                LEFT JOIN SA_CUSTOMER_SETUP CS
                                ON CS.CUSTOMER_CODE = DPO.CUSTOMER_CODE
                                AND CS.COMPANY_CODE = DPO.COMPANY_CODE
                                AND CS.DELETED_FLAG = 'N'
                                INNER JOIN DIST_LOGIN_USER LU
                                ON LU.USERID = DPO.CREATED_BY
                                LEFT JOIN HR_EMPLOYEE_SETUP ES
                                ON ES.EMPLOYEE_CODE = LU.SP_CODE
                                AND ES.COMPANY_CODE = LU.COMPANY_CODE
                                LEFT JOIN IP_ITEM_UNIT_SETUP IUS
                                ON IUS.ITEM_CODE = DPO.ITEM_CODE
                                AND IUS.COMPANY_CODE = DPO.COMPANY_CODE
                                INNER JOIN DIST_PREFERENCE_SETUP PS
                                ON PS.COMPANY_CODE = DPO.COMPANY_CODE
                                LEFT JOIN IP_PARTY_TYPE_CODE PTC
                                ON PTC.PARTY_TYPE_CODE = DPO.PARTY_TYPE_CODE
                                AND PTC.PARTY_TYPE_CODE = DPO.COMPANY_CODE
                                AND PTC.DELETED_FLAG = 'N') RESULT
                                WHERE 1 = 1 { flagFilter} AND RESULT.DELETED_FLAG = 'N' AND RESULT.ORDER_NO = '{orderCode}'
                                ORDER BY UPPER (TRIM (RESULT.ITEM_EDESC))";
            }
            else if (PO.PO_RATE_TABLE == "IP_ITEM_RATE_SCHEDULE_SETUP" && PO.PO_DIST_RATE_COLUMN != "SALES_RATE_ZERO")
            {
                newQuery = $@"SELECT 
                                    {dynamicRate}
                                    IRA.effective_date,
                                    RESULT.*
                                    --,IRA.BRANCH_CODE rate_branch 
                                    , RESULT.BRANCH_CODE order_branch
                                    FROM (SELECT DPO.ORDER_NO,
                                    DPO.ORDER_DATE,
                                    DPO.PARTY_TYPE_CODE,
                                    PTC.PARTY_TYPE_EDESC,
                                    DPO.CUSTOMER_CODE,
                                    CS.CUSTOMER_EDESC,
                                    DPO.ITEM_CODE,
                                    TRIM (IMS.ITEM_EDESC) ITEM_EDESC,
                                    DPO.MU_CODE,
                                    NVL (DPO.QUANTITY, 0) QUANTITY,
                                    NVL (DPO.APPROVE_QTY, 0) APPROVEQTY,
                                    NVL (DPO.UNIT_PRICE, 0) UNIT_PRICE,
                                    --NVL(ITRAS.SALES_RATE,0) NEW_UNIT_PRICE, --aaku
                                    IUS.MU_CODE CONVERSION_MU_CODE,
                                    CASE (DPO.MU_CODE)
                                    WHEN IUS.MU_CODE THEN 1
                                    ELSE IUS.CONVERSION_FACTOR
                                    END
                                    CONVERSION_FACTOR,
                                    DPO.TOTAL_PRICE,
                                    DPO.REMARKS,
                                    DPO.CREATED_BY,
                                    DPO.CREATED_DATE,
                                    DPO.MODIFY_DATE,
                                    DPO.DELETED_FLAG,
                                    DPO.CURRENCY_CODE,
                                    DPO.EXCHANGE_RATE,
                                    DPO.COMPANY_CODE,
                                    DPO.BRANCH_CODE,
                                    DPO.APPROVED_FLAG,
                                    DPO.DISPATCH_FLAG,
                                    DPO.ACKNOWLEDGE_FLAG,
                                    DPO.REJECT_FLAG,
                                    DPO.SYN_ROWID--,DPO.MODIFY_DATE
                                    ,
                                    DPO.MODIFY_BY,
                                    TRIM (IMS.ITEM_EDESC),
                                    ES.EMPLOYEE_EDESC,
                                    PS.PO_PARTY_TYPE,
                                    PS.PO_CONVERSION_FACTOR,
                                    PS.PO_BILLING_NAME,
                                    PS.PO_CUSTOM_RATE,
                                    PS.PO_REMARKS,
                                    PS.PO_CONVERSION_UNIT--,PS.PO_CONVERSION_FACTOR
                                    ,
                                    PS.CS_CONVERSION_UNIT,
                                    CS.CREDIT_LIMIT
                                    FROM DIST_IP_SSD_PURCHASE_ORDER DPO
                                    INNER JOIN IP_ITEM_MASTER_SETUP IMS
                                    ON IMS.ITEM_CODE = DPO.ITEM_CODE
                                    AND IMS.COMPANY_CODE = DPO.COMPANY_CODE
                                    AND IMS.CATEGORY_CODE IN
                                    (SELECT CATEGORY_CODE
                                    FROM IP_CATEGORY_CODE
                                    WHERE CATEGORY_TYPE IN ('FG', 'TF'))
                                    AND IMS.GROUP_SKU_FLAG = 'I'
                                    LEFT JOIN SA_CUSTOMER_SETUP CS
                                    ON CS.CUSTOMER_CODE = DPO.CUSTOMER_CODE
                                    AND CS.COMPANY_CODE = DPO.COMPANY_CODE
                                    AND CS.DELETED_FLAG = 'N'
                                    INNER JOIN DIST_LOGIN_USER LU
                                    ON LU.USERID = DPO.CREATED_BY
                                    LEFT JOIN HR_EMPLOYEE_SETUP ES
                                    ON ES.EMPLOYEE_CODE = LU.SP_CODE
                                    AND ES.COMPANY_CODE = LU.COMPANY_CODE
                                    LEFT JOIN IP_ITEM_UNIT_SETUP IUS
                                    ON IUS.ITEM_CODE = DPO.ITEM_CODE
                                    AND IUS.COMPANY_CODE = DPO.COMPANY_CODE
                                    INNER JOIN DIST_PREFERENCE_SETUP PS
                                    ON PS.COMPANY_CODE = DPO.COMPANY_CODE
                                    LEFT JOIN IP_PARTY_TYPE_CODE PTC
                                    ON PTC.PARTY_TYPE_CODE = DPO.PARTY_TYPE_CODE
                                    AND PTC.PARTY_TYPE_CODE = DPO.COMPANY_CODE
                                    AND PTC.DELETED_FLAG = 'N') RESULT
                                    INNER JOIN
                                    (SELECT tt.*
                                    FROM IP_ITEM_RATE_SCHEDULE_SETUP tt
                                    INNER JOIN
                                    ( SELECT item_code
                                    --, branch_code
                                    ,company_code,MAX (effective_date) AS MaxDateTime
                                    FROM IP_ITEM_RATE_SCHEDULE_SETUP
                                    GROUP BY item_code
                                    --,branch_code
                                    ,company_code) groupedtt
                                    ON tt.item_code = groupedtt.item_code
                                    --AND TT.BRANCH_CODE= groupedtt.BRANCH_CODE
                                    AND TT.COMPANY_CODE= groupedtt.COMPANY_CODE
                                    AND tt.effective_date = groupedtt.MaxDateTime) IRA
                                    ON IRA.item_code = result.item_code
                                    AND IRA.COMPANY_CODE = RESULT.COMPANY_CODE
                                    --AND IRA.BRANCH_CODE = RESULT.BRANCH_CODE
                                    --LEFT JOIN IP_ITEM_RATE_APPLICAT_SETUP ITRAS on ITRAS.ITEM_CODE = DPO.ITEM_CODE AND ITRAS.COMPANY_CODE = DPO.COMPANY_CODE --aaku
                                    
                                    WHERE 1 = 1  AND RESULT.DELETED_FLAG = 'N' AND RESULT.ORDER_NO = '{orderCode}'
                                    ORDER BY UPPER (TRIM (RESULT.ITEM_EDESC))";
            }
            else if (PO.PO_RATE_TABLE == "IP_ITEM_RATE_APPLICAT_SETUP" && PO.PO_DIST_RATE_COLUMN != "SALES_RATE_ZERO")
            {
                newQuery = $@"SELECT 
                                {dynamicRate}
                                IRA.APP_DATE,
                                RESULT.*,IRA.BRANCH_CODE rate_branch , RESULT.BRANCH_CODE order_branch
                                FROM (SELECT DPO.ORDER_NO,
                                DPO.ORDER_DATE,
                                DPO.PARTY_TYPE_CODE,
                                PTC.PARTY_TYPE_EDESC,
                                DPO.CUSTOMER_CODE,
                                CS.CUSTOMER_EDESC,
                                DPO.ITEM_CODE,
                                TRIM (IMS.ITEM_EDESC) ITEM_EDESC,
                                DPO.MU_CODE,
                                NVL (DPO.QUANTITY, 0) QUANTITY,
                                NVL (DPO.APPROVE_QTY, 0) APPROVEQTY,
                                NVL (DPO.UNIT_PRICE, 0) UNIT_PRICE,
                                --NVL(ITRAS.SALES_RATE,0) NEW_UNIT_PRICE, --aaku
                                IUS.MU_CODE CONVERSION_MU_CODE,
                                CASE (DPO.MU_CODE)
                                WHEN IUS.MU_CODE THEN 1
                                ELSE IUS.CONVERSION_FACTOR
                                END
                                CONVERSION_FACTOR,
                                DPO.TOTAL_PRICE,
                                DPO.REMARKS,
                                DPO.CREATED_BY,
                                DPO.CREATED_DATE,
                                DPO.MODIFY_DATE,
                                DPO.DELETED_FLAG,
                                DPO.CURRENCY_CODE,
                                DPO.EXCHANGE_RATE,
                                DPO.COMPANY_CODE,
                                DPO.BRANCH_CODE,
                                DPO.APPROVED_FLAG,
                                DPO.DISPATCH_FLAG,
                                DPO.ACKNOWLEDGE_FLAG,
                                DPO.REJECT_FLAG,
                                DPO.SYN_ROWID--,DPO.MODIFY_DATE
                                ,
                                DPO.MODIFY_BY,
                                TRIM (IMS.ITEM_EDESC),
                                ES.EMPLOYEE_EDESC,
                                PS.PO_PARTY_TYPE,
                                PS.PO_CONVERSION_FACTOR,
                                PS.PO_BILLING_NAME,
                                PS.PO_CUSTOM_RATE,
                                PS.PO_REMARKS,
                                PS.PO_CONVERSION_UNIT--,PS.PO_CONVERSION_FACTOR
                                ,
                                PS.CS_CONVERSION_UNIT,
                                CS.CREDIT_LIMIT
                                FROM DIST_IP_SSD_PURCHASE_ORDER DPO
                                INNER JOIN IP_ITEM_MASTER_SETUP IMS
                                ON IMS.ITEM_CODE = DPO.ITEM_CODE
                                AND IMS.COMPANY_CODE = DPO.COMPANY_CODE
                                AND IMS.CATEGORY_CODE IN
                                (SELECT CATEGORY_CODE
                                FROM IP_CATEGORY_CODE
                                WHERE CATEGORY_TYPE IN ('FG', 'TF'))
                                AND IMS.GROUP_SKU_FLAG = 'I'
                                LEFT JOIN SA_CUSTOMER_SETUP CS
                                ON CS.CUSTOMER_CODE = DPO.CUSTOMER_CODE
                                AND CS.COMPANY_CODE = DPO.COMPANY_CODE
                                AND CS.DELETED_FLAG = 'N'
                                INNER JOIN DIST_LOGIN_USER LU
                                ON LU.USERID = DPO.CREATED_BY
                                LEFT JOIN HR_EMPLOYEE_SETUP ES
                                ON ES.EMPLOYEE_CODE = LU.SP_CODE
                                AND ES.COMPANY_CODE = LU.COMPANY_CODE
                                LEFT JOIN IP_ITEM_UNIT_SETUP IUS
                                ON IUS.ITEM_CODE = DPO.ITEM_CODE
                                AND IUS.COMPANY_CODE = DPO.COMPANY_CODE
                                INNER JOIN DIST_PREFERENCE_SETUP PS
                                ON PS.COMPANY_CODE = DPO.COMPANY_CODE
                                LEFT JOIN IP_PARTY_TYPE_CODE PTC
                                ON PTC.PARTY_TYPE_CODE = DPO.PARTY_TYPE_CODE
                                AND PTC.PARTY_TYPE_CODE = DPO.COMPANY_CODE
                                AND PTC.DELETED_FLAG = 'N') RESULT
                                INNER JOIN
                                (SELECT tt.*
                                FROM IP_ITEM_RATE_APPLICAT_SETUP tt
                                INNER JOIN
                                ( SELECT item_code, branch_code,company_code,MAX (app_date) AS MaxDateTime
                                FROM IP_ITEM_RATE_APPLICAT_SETUP
                                GROUP BY item_code,branch_code,company_code) groupedtt
                                ON tt.item_code = groupedtt.item_code
                                AND TT.BRANCH_CODE= groupedtt.BRANCH_CODE
                                AND TT.COMPANY_CODE= groupedtt.COMPANY_CODE
                                AND tt.app_date = groupedtt.MaxDateTime) IRA
                                ON IRA.item_code = result.item_code
                                AND IRA.COMPANY_CODE = RESULT.COMPANY_CODE
                                AND IRA.BRANCH_CODE = RESULT.BRANCH_CODE
                                --LEFT JOIN IP_ITEM_RATE_APPLICAT_SETUP ITRAS on ITRAS.ITEM_CODE = DPO.ITEM_CODE AND ITRAS.COMPANY_CODE = DPO.COMPANY_CODE --aaku
                                WHERE 1 = 1 { flagFilter} AND RESULT.DELETED_FLAG = 'N' AND RESULT.ORDER_NO = '{orderCode}'
                                ORDER BY UPPER (TRIM (RESULT.ITEM_EDESC))";
            }
            else
            {
                switch (ORDER_ENTITY)
                {
                    case "R":
                        newQuery = $@"SELECT DPO.ORDER_NO, DPO.ORDER_DATE, DPO.PARTY_TYPE_CODE, 
                                    PTC.PARTY_TYPE_EDESC, DPO.CUSTOMER_CODE, CS.CUSTOMER_EDESC, 
                                    DPO.ITEM_CODE,TRIM(IMS.ITEM_EDESC) ITEM_EDESC, DPO.MU_CODE, NVL(DPO.QUANTITY,0) QUANTITY, NVL(DPO.APPROVE_QTY,0) APPROVEQTY,
                                              NVL(DPO.UNIT_PRICE,0) UNIT_PRICE, IUS.MU_CODE CONVERSION_MU_CODE, 
                                    CASE(DPO.MU_CODE)
                                        WHEN IUS.MU_CODE THEN 1
                                        ELSE IUS.CONVERSION_FACTOR
                                      END
                                    CONVERSION_FACTOR,
                                              DPO.TOTAL_PRICE, DPO.REMARKS, DPO.CREATED_BY, DPO.CREATED_DATE, 
                                    DPO.MODIFY_DATE, DPO.DELETED_FLAG, DPO.CURRENCY_CODE, DPO.EXCHANGE_RATE,
                                              DPO.COMPANY_CODE, DPO.BRANCH_CODE,
                                              DPO.APPROVED_FLAG, DPO.DISPATCH_FLAG, DPO.ACKNOWLEDGE_FLAG, 
                                    DPO.REJECT_FLAG, DPO.SYN_ROWID, DPO.MODIFY_DATE, DPO.MODIFY_BY,
                                              TRIM(IMS.ITEM_EDESC),
                                              ES.EMPLOYEE_EDESC,
                                              PS.PO_PARTY_TYPE,
                                              PS.PO_CONVERSION_FACTOR,
                                              PS.PO_BILLING_NAME,
                                              PS.PO_CUSTOM_RATE, PS.PO_REMARKS, PS.PO_CONVERSION_UNIT, 
                                    PS.PO_CONVERSION_FACTOR, PS.CS_CONVERSION_UNIT,
                                              CS.CREDIT_LIMIT
                                    FROM DIST_IP_SSR_PURCHASE_ORDER DPO
                                    INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DPO.ITEM_CODE AND 
                                    IMS.COMPANY_CODE = DPO.COMPANY_CODE AND IMS.CATEGORY_CODE in (select CATEGORY_CODE  from IP_CATEGORY_CODE WHERE CATEGORY_TYPE IN ('FG','TF')) AND 
                                    IMS.GROUP_SKU_FLAG = 'I'
                                    LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DPO.CUSTOMER_CODE 
                                    AND CS.COMPANY_CODE = DPO.COMPANY_CODE AND CS.DELETED_FLAG = 'N'
                                    INNER JOIN DIST_LOGIN_USER LU ON LU.USERID = DPO.CREATED_BY
                                    INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = LU.SP_CODE AND 
                                    ES.COMPANY_CODE = LU.COMPANY_CODE
                                    LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON IUS.ITEM_CODE = DPO.ITEM_CODE AND 
                                    IUS.COMPANY_CODE = DPO.COMPANY_CODE
                                    INNER JOIN DIST_PREFERENCE_SETUP PS ON PS.COMPANY_CODE = 
                                    DPO.COMPANY_CODE
                                    LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = 
                                    DPO.PARTY_TYPE_CODE AND PTC.PARTY_TYPE_CODE = DPO.COMPANY_CODE AND 
                                    PTC.DELETED_FLAG = 'N'
                                    WHERE 1 = 1
                                      {flagFilter}
                                       AND DPO.DELETED_FLAG = 'N'
                                       AND DPO.ORDER_NO = '{orderCode}'
                                    ORDER BY UPPER(TRIM(IMS.ITEM_EDESC))";
                        //                    newQuery = $@"SELECT NVL(IRA.SALES_RATE,0) NEW_RATE,
                        //NVL(IRA.SALES_RATE,0) * RESULT.APPROVEQTY NEW_TOTAL_AMOUNT,
                        //IRA.APP_DATE,
                        //RESULT.*,IRA.BRANCH_CODE rate_branch , RESULT.BRANCH_CODE order_branch
                        //FROM (SELECT DPO.ORDER_NO,
                        //DPO.ORDER_DATE,
                        //DPO.PARTY_TYPE_CODE,
                        //PTC.PARTY_TYPE_EDESC,
                        //DPO.CUSTOMER_CODE,
                        //CS.CUSTOMER_EDESC,
                        //DPO.ITEM_CODE,
                        //TRIM (IMS.ITEM_EDESC) ITEM_EDESC,
                        //DPO.MU_CODE,
                        //NVL (DPO.QUANTITY, 0) QUANTITY,
                        //NVL (DPO.APPROVE_QTY, 0) APPROVEQTY,
                        //NVL (DPO.UNIT_PRICE, 0) UNIT_PRICE,
                        //--NVL(ITRAS.SALES_RATE,0) NEW_UNIT_PRICE, --aaku
                        //IUS.MU_CODE CONVERSION_MU_CODE,
                        //CASE (DPO.MU_CODE)
                        //WHEN IUS.MU_CODE THEN 1
                        //ELSE IUS.CONVERSION_FACTOR
                        //END
                        //CONVERSION_FACTOR,
                        //DPO.TOTAL_PRICE,
                        //DPO.REMARKS,
                        //DPO.CREATED_BY,
                        //DPO.CREATED_DATE,
                        //DPO.MODIFY_DATE,
                        //DPO.DELETED_FLAG,
                        //DPO.CURRENCY_CODE,
                        //DPO.EXCHANGE_RATE,
                        //DPO.COMPANY_CODE,
                        //DPO.BRANCH_CODE,
                        //DPO.APPROVED_FLAG,
                        //DPO.DISPATCH_FLAG,
                        //DPO.ACKNOWLEDGE_FLAG,
                        //DPO.REJECT_FLAG,
                        //DPO.SYN_ROWID--,DPO.MODIFY_DATE
                        //,
                        //DPO.MODIFY_BY,
                        //TRIM (IMS.ITEM_EDESC),
                        //ES.EMPLOYEE_EDESC,
                        //PS.PO_PARTY_TYPE,
                        //PS.PO_CONVERSION_FACTOR,
                        //PS.PO_BILLING_NAME,
                        //PS.PO_CUSTOM_RATE,
                        //PS.PO_REMARKS,
                        //PS.PO_CONVERSION_UNIT--,PS.PO_CONVERSION_FACTOR
                        //,
                        //PS.CS_CONVERSION_UNIT,
                        //CS.CREDIT_LIMIT
                        //FROM DIST_IP_SSD_PURCHASE_ORDER DPO
                        //INNER JOIN IP_ITEM_MASTER_SETUP IMS
                        //ON IMS.ITEM_CODE = DPO.ITEM_CODE
                        //AND IMS.COMPANY_CODE = DPO.COMPANY_CODE
                        //AND IMS.CATEGORY_CODE IN
                        //(SELECT CATEGORY_CODE
                        //FROM IP_CATEGORY_CODE
                        //WHERE CATEGORY_TYPE IN ('FG', 'TF'))
                        //AND IMS.GROUP_SKU_FLAG = 'I'
                        //LEFT JOIN SA_CUSTOMER_SETUP CS
                        //ON CS.CUSTOMER_CODE = DPO.CUSTOMER_CODE
                        //AND CS.COMPANY_CODE = DPO.COMPANY_CODE
                        //AND CS.DELETED_FLAG = 'N'
                        //INNER JOIN DIST_LOGIN_USER LU
                        //ON LU.USERID = DPO.CREATED_BY
                        //LEFT JOIN HR_EMPLOYEE_SETUP ES
                        //ON ES.EMPLOYEE_CODE = LU.SP_CODE
                        //AND ES.COMPANY_CODE = LU.COMPANY_CODE
                        //LEFT JOIN IP_ITEM_UNIT_SETUP IUS
                        //ON IUS.ITEM_CODE = DPO.ITEM_CODE
                        //AND IUS.COMPANY_CODE = DPO.COMPANY_CODE
                        //INNER JOIN DIST_PREFERENCE_SETUP PS
                        //ON PS.COMPANY_CODE = DPO.COMPANY_CODE
                        //LEFT JOIN IP_PARTY_TYPE_CODE PTC
                        //ON PTC.PARTY_TYPE_CODE = DPO.PARTY_TYPE_CODE
                        //AND PTC.PARTY_TYPE_CODE = DPO.COMPANY_CODE
                        //AND PTC.DELETED_FLAG = 'N') RESULT
                        //INNER JOIN
                        //(SELECT tt.*
                        //FROM IP_ITEM_RATE_APPLICAT_SETUP tt
                        //INNER JOIN
                        //( SELECT item_code, branch_code,company_code,MAX (app_date) AS MaxDateTime
                        //FROM IP_ITEM_RATE_APPLICAT_SETUP
                        //GROUP BY item_code,branch_code,company_code) groupedtt
                        //ON tt.item_code = groupedtt.item_code
                        //AND TT.BRANCH_CODE= groupedtt.BRANCH_CODE
                        //AND TT.COMPANY_CODE= groupedtt.COMPANY_CODE
                        //AND tt.app_date = groupedtt.MaxDateTime) IRA
                        //ON IRA.item_code = result.item_code
                        //AND IRA.COMPANY_CODE = RESULT.COMPANY_CODE
                        //AND IRA.BRANCH_CODE = RESULT.BRANCH_CODE
                        //--LEFT JOIN IP_ITEM_RATE_APPLICAT_SETUP ITRAS on ITRAS.ITEM_CODE = DPO.ITEM_CODE AND ITRAS.COMPANY_CODE = DPO.COMPANY_CODE --aaku
                        //WHERE 1 = 1 { flagFilter} AND RESULT.DELETED_FLAG = 'N' AND RESULT.ORDER_NO = '{orderCode}'
                        //ORDER BY UPPER (TRIM (RESULT.ITEM_EDESC))";
                        break;
                    default:
                        newQuery = $@"SELECT DPO.ORDER_NO, DPO.ORDER_DATE, DPO.PARTY_TYPE_CODE, 
                                    PTC.PARTY_TYPE_EDESC, DPO.CUSTOMER_CODE, CS.CUSTOMER_EDESC, 
                                    DPO.ITEM_CODE,TRIM(IMS.ITEM_EDESC) ITEM_EDESC, DPO.MU_CODE, NVL(DPO.QUANTITY,0) QUANTITY, NVL(DPO.APPROVE_QTY,0) APPROVEQTY,
                                              NVL(DPO.UNIT_PRICE,0) UNIT_PRICE, IUS.MU_CODE CONVERSION_MU_CODE, 
                                    CASE(DPO.MU_CODE)
                                        WHEN IUS.MU_CODE THEN 1
                                        ELSE IUS.CONVERSION_FACTOR
                                      END
                                    CONVERSION_FACTOR,
                                              DPO.TOTAL_PRICE, DPO.REMARKS, DPO.CREATED_BY, DPO.CREATED_DATE, 
                                    DPO.MODIFY_DATE, DPO.DELETED_FLAG, DPO.CURRENCY_CODE, DPO.EXCHANGE_RATE,
                                              DPO.COMPANY_CODE, DPO.BRANCH_CODE,
                                              DPO.APPROVED_FLAG, DPO.DISPATCH_FLAG, DPO.ACKNOWLEDGE_FLAG, 
                                    DPO.REJECT_FLAG, DPO.SYN_ROWID, DPO.MODIFY_DATE, DPO.MODIFY_BY,
                                              TRIM(IMS.ITEM_EDESC),
                                              ES.EMPLOYEE_EDESC,
                                              PS.PO_PARTY_TYPE,
                                              PS.PO_CONVERSION_FACTOR,
                                              PS.PO_BILLING_NAME,
                                              PS.PO_CUSTOM_RATE, PS.PO_REMARKS, PS.PO_CONVERSION_UNIT, 
                                    PS.PO_CONVERSION_FACTOR, PS.CS_CONVERSION_UNIT,
                                              CS.CREDIT_LIMIT
                                    FROM DIST_IP_SSD_PURCHASE_ORDER DPO
                                    INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DPO.ITEM_CODE AND 
                                    IMS.COMPANY_CODE = DPO.COMPANY_CODE AND IMS.CATEGORY_CODE in (select CATEGORY_CODE  from IP_CATEGORY_CODE WHERE CATEGORY_TYPE IN ('FG','TF')) AND 
                                    IMS.GROUP_SKU_FLAG = 'I'
                                    LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DPO.CUSTOMER_CODE 
                                    AND CS.COMPANY_CODE = DPO.COMPANY_CODE AND CS.DELETED_FLAG = 'N'
                                    INNER JOIN DIST_LOGIN_USER LU ON LU.USERID = DPO.CREATED_BY
                                    LEFT JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = LU.SP_CODE AND 
                                    ES.COMPANY_CODE = LU.COMPANY_CODE
                                    LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON IUS.ITEM_CODE = DPO.ITEM_CODE AND 
                                    IUS.COMPANY_CODE = DPO.COMPANY_CODE
                                    INNER JOIN DIST_PREFERENCE_SETUP PS ON PS.COMPANY_CODE = 
                                    DPO.COMPANY_CODE
                                    LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = 
                                    DPO.PARTY_TYPE_CODE AND PTC.PARTY_TYPE_CODE = DPO.COMPANY_CODE AND 
                                    PTC.DELETED_FLAG = 'N'
                                    WHERE 1 = 1
                                      {flagFilter}
                                       AND DPO.DELETED_FLAG = 'N'
                                       AND DPO.ORDER_NO = '{orderCode}'
                                    ORDER BY UPPER(TRIM(IMS.ITEM_EDESC))";

                        newQuery = $@"SELECT 
{dynamicRate}
IRA.APP_DATE,
RESULT.*,IRA.BRANCH_CODE rate_branch , RESULT.BRANCH_CODE order_branch
FROM (SELECT DPO.ORDER_NO,
DPO.ORDER_DATE,
DPO.PARTY_TYPE_CODE,
PTC.PARTY_TYPE_EDESC,
DPO.CUSTOMER_CODE,
CS.CUSTOMER_EDESC,
DPO.ITEM_CODE,
TRIM (IMS.ITEM_EDESC) ITEM_EDESC,
DPO.MU_CODE,
NVL (DPO.QUANTITY, 0) QUANTITY,
NVL (DPO.APPROVE_QTY, 0) APPROVEQTY,
NVL (DPO.UNIT_PRICE, 0) UNIT_PRICE,
--NVL(ITRAS.SALES_RATE,0) NEW_UNIT_PRICE, --aaku
IUS.MU_CODE CONVERSION_MU_CODE,
CASE (DPO.MU_CODE)
WHEN IUS.MU_CODE THEN 1
ELSE IUS.CONVERSION_FACTOR
END
CONVERSION_FACTOR,
DPO.TOTAL_PRICE,
DPO.REMARKS,
DPO.CREATED_BY,
DPO.CREATED_DATE,
DPO.MODIFY_DATE,
DPO.DELETED_FLAG,
DPO.CURRENCY_CODE,
DPO.EXCHANGE_RATE,
DPO.COMPANY_CODE,
DPO.BRANCH_CODE,
DPO.APPROVED_FLAG,
DPO.DISPATCH_FLAG,
DPO.ACKNOWLEDGE_FLAG,
DPO.REJECT_FLAG,
DPO.SYN_ROWID--,DPO.MODIFY_DATE
,
DPO.MODIFY_BY,
TRIM (IMS.ITEM_EDESC),
ES.EMPLOYEE_EDESC,
PS.PO_PARTY_TYPE,
PS.PO_CONVERSION_FACTOR,
PS.PO_BILLING_NAME,
PS.PO_CUSTOM_RATE,
PS.PO_REMARKS,
PS.PO_CONVERSION_UNIT--,PS.PO_CONVERSION_FACTOR
,
PS.CS_CONVERSION_UNIT,
CS.CREDIT_LIMIT
FROM DIST_IP_SSD_PURCHASE_ORDER DPO
INNER JOIN IP_ITEM_MASTER_SETUP IMS
ON IMS.ITEM_CODE = DPO.ITEM_CODE
AND IMS.COMPANY_CODE = DPO.COMPANY_CODE
AND IMS.CATEGORY_CODE IN
(SELECT CATEGORY_CODE
FROM IP_CATEGORY_CODE
WHERE CATEGORY_TYPE IN ('FG', 'TF'))
AND IMS.GROUP_SKU_FLAG = 'I'
LEFT JOIN SA_CUSTOMER_SETUP CS
ON CS.CUSTOMER_CODE = DPO.CUSTOMER_CODE
AND CS.COMPANY_CODE = DPO.COMPANY_CODE
AND CS.DELETED_FLAG = 'N'
INNER JOIN DIST_LOGIN_USER LU
ON LU.USERID = DPO.CREATED_BY
LEFT JOIN HR_EMPLOYEE_SETUP ES
ON ES.EMPLOYEE_CODE = LU.SP_CODE
AND ES.COMPANY_CODE = LU.COMPANY_CODE
LEFT JOIN IP_ITEM_UNIT_SETUP IUS
ON IUS.ITEM_CODE = DPO.ITEM_CODE
AND IUS.COMPANY_CODE = DPO.COMPANY_CODE
INNER JOIN DIST_PREFERENCE_SETUP PS
ON PS.COMPANY_CODE = DPO.COMPANY_CODE
LEFT JOIN IP_PARTY_TYPE_CODE PTC
ON PTC.PARTY_TYPE_CODE = DPO.PARTY_TYPE_CODE
AND PTC.PARTY_TYPE_CODE = DPO.COMPANY_CODE
AND PTC.DELETED_FLAG = 'N') RESULT
INNER JOIN
(SELECT tt.*
FROM IP_ITEM_RATE_APPLICAT_SETUP tt
INNER JOIN
( SELECT item_code, branch_code,company_code,MAX (app_date) AS MaxDateTime
FROM IP_ITEM_RATE_APPLICAT_SETUP
GROUP BY item_code,branch_code,company_code) groupedtt
ON tt.item_code = groupedtt.item_code
AND TT.BRANCH_CODE= groupedtt.BRANCH_CODE
AND TT.COMPANY_CODE= groupedtt.COMPANY_CODE
AND tt.app_date = groupedtt.MaxDateTime) IRA
ON IRA.item_code = result.item_code
AND IRA.COMPANY_CODE = RESULT.COMPANY_CODE
AND IRA.BRANCH_CODE = RESULT.BRANCH_CODE
--LEFT JOIN IP_ITEM_RATE_APPLICAT_SETUP ITRAS on ITRAS.ITEM_CODE = DPO.ITEM_CODE AND ITRAS.COMPANY_CODE = DPO.COMPANY_CODE --aaku
WHERE 1 = 1 { flagFilter} AND RESULT.DELETED_FLAG = 'N' AND RESULT.ORDER_NO = '{orderCode}'
ORDER BY UPPER (TRIM (RESULT.ITEM_EDESC))";
                        break;
                }
            }

            var data = _objectEntity.SqlQuery<PurchaseOrderModel>(newQuery).ToList();
            return data;

        }

        public List<PurchaseOrderModel> GetDealerSalesOrderDetail(string CompanyCode, string orderCode, string ORDER_ENTITY, string requestStatus)
        {
            var flagFilter = "";
            if (requestStatus == "Approved")
            {
                if (ORDER_ENTITY == "R")
                {
                    flagFilter = @" --AND DPO.approved_flag = 'Y'    
                                    AND DPO.APPROVE_QTY<>0 
                               AND DPO.REJECT_FLAG = 'N'";
                }
                else
                {
                    flagFilter = @" --AND RESULT.approved_flag = 'Y'    
                                    AND RESULT.APPROVEQTY<>0 
                               AND RESULT.REJECT_FLAG = 'N'";
                }

            }

            //OldRunning
            // flagFilter = @" AND DPO.approved_flag = 'Y'
            //               AND DPO.REJECT_FLAG = 'N'";
            else if (requestStatus == "Rejected")
                flagFilter = @" AND RESULT.REJECT_FLAG = 'Y'
                                AND RESULT.approved_flag = 'N'";
            //oldRunning
            //flagFilter = @" AND DPO.REJECT_FLAG = 'Y'
            //              AND DPO.approved_flag = 'N'";
            else if (requestStatus == "All")
            {
                flagFilter = "";
            }
            else if (requestStatus == "Active")
            {
                if (ORDER_ENTITY == "R")
                {
                    flagFilter = @" AND DPO.REJECT_FLAG = 'N'
                                AND DPO.QUANTITY <>0
                                AND DPO.APPROVED_FLAG = 'N'";
                }
                else
                {
                    flagFilter = @" AND RESULT.REJECT_FLAG = 'N'
                                AND RESULT.QUANTITY <>0
                                AND RESULT.APPROVED_FLAG = 'N'";
                }

                //oldRunning
                //flagFilter = @" AND DPO.REJECT_FLAG = 'N'
                //              AND DPO.APPROVED_FLAG = 'N'";
            }

            //OLD QUERY
            //var query = $@"SELECT DPO.ORDER_NO, DPO.ORDER_DATE, DPO.PARTY_TYPE_CODE, 
            //                        PTC.PARTY_TYPE_EDESC, DPO.CUSTOMER_CODE, CS.CUSTOMER_EDESC, 
            //                        DPO.ITEM_CODE,TRIM(IMS.ITEM_EDESC) ITEM_EDESC, DPO.MU_CODE, NVL(DPO.QUANTITY,0) QUANTITY, NVL(DPO.APPROVE_QTY,0) APPROVEQTY,
            //                                  NVL(DPO.UNIT_PRICE,0) UNIT_PRICE, IUS.MU_CODE CONVERSION_MU_CODE, 
            //                        IUS.CONVERSION_FACTOR,
            //                                  DPO.TOTAL_PRICE, DPO.REMARKS, DPO.CREATED_BY, DPO.CREATED_DATE, 
            //                        DPO.MODIFY_DATE, DPO.DELETED_FLAG, DPO.CURRENCY_CODE, DPO.EXCHANGE_RATE,
            //                                  DPO.COMPANY_CODE, DPO.BRANCH_CODE,
            //                                  DPO.APPROVED_FLAG, DPO.DISPATCH_FLAG, DPO.ACKNOWLEDGE_FLAG, 
            //                        DPO.REJECT_FLAG, DPO.SYN_ROWID, DPO.MODIFY_DATE, DPO.MODIFY_BY,
            //                                  TRIM(IMS.ITEM_EDESC),
            //                                  ES.EMPLOYEE_EDESC,
            //                                  PS.PO_PARTY_TYPE,
            //                                  PS.PO_CONVERSION_FACTOR,
            //                                  PS.PO_BILLING_NAME,
            //                                  PS.PO_CUSTOM_RATE, PS.PO_REMARKS, PS.PO_CONVERSION_UNIT, 
            //                        PS.PO_CONVERSION_FACTOR, PS.CS_CONVERSION_UNIT,
            //                                  CS.CREDIT_LIMIT
            //                        FROM DIST_IP_SSD_PURCHASE_ORDER DPO
            //                        INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DPO.ITEM_CODE AND 
            //                        IMS.COMPANY_CODE = DPO.COMPANY_CODE AND IMS.CATEGORY_CODE = 'FG' AND 
            //                        IMS.GROUP_SKU_FLAG = 'I'
            //                        LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DPO.CUSTOMER_CODE 
            //                        AND CS.COMPANY_CODE = DPO.COMPANY_CODE AND CS.DELETED_FLAG = 'N'
            //                        INNER JOIN DIST_LOGIN_USER LU ON LU.USERID = DPO.CREATED_BY
            //                        INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = LU.SP_CODE AND 
            //                        ES.COMPANY_CODE = LU.COMPANY_CODE
            //                        LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON IUS.ITEM_CODE = DPO.ITEM_CODE AND 
            //                        IUS.COMPANY_CODE = DPO.COMPANY_CODE
            //                        INNER JOIN DIST_PREFERENCE_SETUP PS ON PS.COMPANY_CODE = 
            //                        DPO.COMPANY_CODE
            //                        LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = 
            //                        DPO.PARTY_TYPE_CODE AND PTC.PARTY_TYPE_CODE = DPO.COMPANY_CODE AND 
            //                        PTC.DELETED_FLAG = 'N'
            //                        WHERE 1 = 1
            //                          {flagFilter}
            //                           AND DPO.DELETED_FLAG = 'N'
            //                           AND DPO.ORDER_NO = '{orderCode}'
            //                        ORDER BY UPPER(TRIM(IMS.ITEM_EDESC))";


            var query = "";
            var query1 = "";
            var newQuery = "";
            var dynamicRate = "";
            var PO = _objectEntity.SqlQuery<PreferenceSetupModel>($"SELECT * FROM DIST_PREFERENCE_SETUP WHERE COMPANY_CODE='{CompanyCode}'").FirstOrDefault();


            if (PO.SO_REPO_RATE_TABLE == "DIST_IP_SSD_PURCHASE_ORDER" && PO.SO_REPO_RATE_COLUMN == "UNIT_PRICE")
            {
                if (requestStatus == "Approved")
                    dynamicRate = "NVL(RESULT.UNIT_PRICE,0) NEW_RATE,NVL(RESULT.UNIT_PRICE, 0) * RESULT.APPROVEQTY NEW_TOTAL_AMOUNT,";
                else dynamicRate = "NVL(RESULT.UNIT_PRICE,0) NEW_RATE,NVL(RESULT.UNIT_PRICE, 0) * RESULT.QUANTITY NEW_TOTAL_AMOUNT,";

            }
            else
            {
                if (requestStatus == "Approved") dynamicRate = "NVL(IRA.SALES_RATE,0) NEW_RATE,NVL(IRA.SALES_RATE, 0) * RESULT.APPROVEQTY NEW_TOTAL_AMOUNT,";
                else dynamicRate = "NVL(IRA.SALES_RATE,0) NEW_RATE,NVL(IRA.SALES_RATE, 0) * RESULT.QUANTITY NEW_TOTAL_AMOUNT,";

            }
            if (PO.PO_DIST_RATE_COLUMN == "SALES_RATE_ZERO")
            {
                newQuery = $@"SELECT 
{dynamicRate}
--IRA.APP_DATE,
RESULT.*
--,IRA.BRANCH_CODE rate_branch 
, RESULT.BRANCH_CODE order_branch
FROM (SELECT DPO.ORDER_NO,
DPO.ORDER_DATE,
DPO.PARTY_TYPE_CODE,
PTC.PARTY_TYPE_EDESC,
DPO.CUSTOMER_CODE,
CS.CUSTOMER_EDESC,
DPO.ITEM_CODE,
TRIM (IMS.ITEM_EDESC) ITEM_EDESC,
DPO.MU_CODE,
NVL (DPO.QUANTITY, 0) QUANTITY,
NVL (DPO.APPROVE_QTY, 0) APPROVEQTY,
NVL (DPO.UNIT_PRICE, 0) UNIT_PRICE,
--NVL(ITRAS.SALES_RATE,0) NEW_UNIT_PRICE, --aaku
IUS.MU_CODE CONVERSION_MU_CODE,
CASE (DPO.MU_CODE)
WHEN IUS.MU_CODE THEN 1
ELSE IUS.CONVERSION_FACTOR
END
CONVERSION_FACTOR,
DPO.TOTAL_PRICE,
DPO.REMARKS,
DPO.CREATED_BY,
DPO.CREATED_DATE,
DPO.MODIFY_DATE,
DPO.DELETED_FLAG,
DPO.CURRENCY_CODE,
DPO.EXCHANGE_RATE,
DPO.COMPANY_CODE,
DPO.BRANCH_CODE,
DPO.APPROVED_FLAG,
DPO.DISPATCH_FLAG,
DPO.ACKNOWLEDGE_FLAG,
DPO.REJECT_FLAG,
DPO.SYN_ROWID--,DPO.MODIFY_DATE
,
DPO.MODIFY_BY,
TRIM (IMS.ITEM_EDESC),
ES.EMPLOYEE_EDESC,
PS.PO_PARTY_TYPE,
PS.PO_CONVERSION_FACTOR,
PS.PO_BILLING_NAME,
PS.PO_CUSTOM_RATE,
PS.PO_REMARKS,
PS.PO_CONVERSION_UNIT--,PS.PO_CONVERSION_FACTOR
,
PS.CS_CONVERSION_UNIT,
CS.CREDIT_LIMIT
FROM DIST_IP_SSD_PURCHASE_ORDER DPO
INNER JOIN IP_ITEM_MASTER_SETUP IMS
ON IMS.ITEM_CODE = DPO.ITEM_CODE
AND IMS.COMPANY_CODE = DPO.COMPANY_CODE
AND IMS.CATEGORY_CODE IN
(SELECT CATEGORY_CODE
FROM IP_CATEGORY_CODE
WHERE CATEGORY_TYPE IN ('FG', 'TF'))
AND IMS.GROUP_SKU_FLAG = 'I'
LEFT JOIN SA_CUSTOMER_SETUP CS
ON CS.CUSTOMER_CODE = DPO.CUSTOMER_CODE
AND CS.COMPANY_CODE = DPO.COMPANY_CODE
AND CS.DELETED_FLAG = 'N'
INNER JOIN DIST_LOGIN_USER LU
ON LU.USERID = DPO.CREATED_BY
LEFT JOIN HR_EMPLOYEE_SETUP ES
ON ES.EMPLOYEE_CODE = LU.SP_CODE
AND ES.COMPANY_CODE = LU.COMPANY_CODE
LEFT JOIN IP_ITEM_UNIT_SETUP IUS
ON IUS.ITEM_CODE = DPO.ITEM_CODE
AND IUS.COMPANY_CODE = DPO.COMPANY_CODE
INNER JOIN DIST_PREFERENCE_SETUP PS
ON PS.COMPANY_CODE = DPO.COMPANY_CODE
LEFT JOIN IP_PARTY_TYPE_CODE PTC
ON PTC.PARTY_TYPE_CODE = DPO.PARTY_TYPE_CODE
AND PTC.PARTY_TYPE_CODE = DPO.COMPANY_CODE
AND PTC.DELETED_FLAG = 'N') RESULT
WHERE 1 = 1 { flagFilter} AND RESULT.DELETED_FLAG = 'N' AND RESULT.ORDER_NO = '{orderCode}'
ORDER BY UPPER (TRIM (RESULT.ITEM_EDESC))";
            }
            else if (PO.PO_RATE_TABLE == "IP_ITEM_RATE_SCHEDULE_SETUP" && PO.PO_DIST_RATE_COLUMN != "SALES_RATE_ZERO")
            {
                newQuery = $@"SELECT 
            {dynamicRate}
            IRA.effective_date,
            RESULT.*
--,IRA.BRANCH_CODE rate_branch 
, RESULT.BRANCH_CODE order_branch
            FROM (SELECT DPO.ORDER_NO,
            DPO.ORDER_DATE,
            DPO.PARTY_TYPE_CODE,
            PTC.PARTY_TYPE_EDESC,
            DPO.CUSTOMER_CODE,
            CS.CUSTOMER_EDESC,
            DPO.ITEM_CODE,
            TRIM (IMS.ITEM_EDESC) ITEM_EDESC,
            DPO.MU_CODE,
            NVL (DPO.QUANTITY, 0) QUANTITY,
            NVL (DPO.APPROVE_QTY, 0) APPROVEQTY,
            NVL (DPO.UNIT_PRICE, 0) UNIT_PRICE,
            --NVL(ITRAS.SALES_RATE,0) NEW_UNIT_PRICE, --aaku
            IUS.MU_CODE CONVERSION_MU_CODE,
            CASE (DPO.MU_CODE)
            WHEN IUS.MU_CODE THEN 1
            ELSE IUS.CONVERSION_FACTOR
            END
            CONVERSION_FACTOR,
            DPO.TOTAL_PRICE,
            DPO.REMARKS,
            DPO.CREATED_BY,
            DPO.CREATED_DATE,
            DPO.MODIFY_DATE,
            DPO.DELETED_FLAG,
            DPO.CURRENCY_CODE,
            DPO.EXCHANGE_RATE,
            DPO.COMPANY_CODE,
            DPO.BRANCH_CODE,
            DPO.APPROVED_FLAG,
            DPO.DISPATCH_FLAG,
            DPO.ACKNOWLEDGE_FLAG,
            DPO.REJECT_FLAG,
            DPO.SYN_ROWID--,DPO.MODIFY_DATE
            ,
            DPO.MODIFY_BY,
            TRIM (IMS.ITEM_EDESC),
            ES.EMPLOYEE_EDESC,
            PS.PO_PARTY_TYPE,
            PS.PO_CONVERSION_FACTOR,
            PS.PO_BILLING_NAME,
            PS.PO_CUSTOM_RATE,
            PS.PO_REMARKS,
            PS.PO_CONVERSION_UNIT--,PS.PO_CONVERSION_FACTOR
            ,
            PS.CS_CONVERSION_UNIT,
            CS.CREDIT_LIMIT
            FROM DIST_IP_SSD_PURCHASE_ORDER DPO
            INNER JOIN IP_ITEM_MASTER_SETUP IMS
            ON IMS.ITEM_CODE = DPO.ITEM_CODE
            AND IMS.COMPANY_CODE = DPO.COMPANY_CODE
            AND IMS.CATEGORY_CODE IN
            (SELECT CATEGORY_CODE
            FROM IP_CATEGORY_CODE
            WHERE CATEGORY_TYPE IN ('FG', 'TF'))
            AND IMS.GROUP_SKU_FLAG = 'I'
            LEFT JOIN SA_CUSTOMER_SETUP CS
            ON CS.CUSTOMER_CODE = DPO.CUSTOMER_CODE
            AND CS.COMPANY_CODE = DPO.COMPANY_CODE
            AND CS.DELETED_FLAG = 'N'
            INNER JOIN DIST_LOGIN_USER LU
            ON LU.USERID = DPO.CREATED_BY
            LEFT JOIN HR_EMPLOYEE_SETUP ES
            ON ES.EMPLOYEE_CODE = LU.SP_CODE
            AND ES.COMPANY_CODE = LU.COMPANY_CODE
            LEFT JOIN IP_ITEM_UNIT_SETUP IUS
            ON IUS.ITEM_CODE = DPO.ITEM_CODE
            AND IUS.COMPANY_CODE = DPO.COMPANY_CODE
            INNER JOIN DIST_PREFERENCE_SETUP PS
            ON PS.COMPANY_CODE = DPO.COMPANY_CODE
            LEFT JOIN IP_PARTY_TYPE_CODE PTC
            ON PTC.PARTY_TYPE_CODE = DPO.PARTY_TYPE_CODE
            AND PTC.PARTY_TYPE_CODE = DPO.COMPANY_CODE
            AND PTC.DELETED_FLAG = 'N') RESULT
            INNER JOIN
            (SELECT tt.*
            FROM IP_ITEM_RATE_SCHEDULE_SETUP tt
            INNER JOIN
            ( SELECT item_code
--, branch_code
,company_code,MAX (effective_date) AS MaxDateTime
            FROM IP_ITEM_RATE_SCHEDULE_SETUP
            GROUP BY item_code
--,branch_code
,company_code) groupedtt
            ON tt.item_code = groupedtt.item_code
            --AND TT.BRANCH_CODE= groupedtt.BRANCH_CODE
            AND TT.COMPANY_CODE= groupedtt.COMPANY_CODE
            AND tt.effective_date = groupedtt.MaxDateTime) IRA
            ON IRA.item_code = result.item_code
            AND IRA.COMPANY_CODE = RESULT.COMPANY_CODE
            --AND IRA.BRANCH_CODE = RESULT.BRANCH_CODE
            --LEFT JOIN IP_ITEM_RATE_APPLICAT_SETUP ITRAS on ITRAS.ITEM_CODE = DPO.ITEM_CODE AND ITRAS.COMPANY_CODE = DPO.COMPANY_CODE --aaku
            WHERE 1 = 1 { flagFilter} AND RESULT.DELETED_FLAG = 'N' AND RESULT.ORDER_NO = '{orderCode}'
            ORDER BY UPPER (TRIM (RESULT.ITEM_EDESC))";
            }
            else if (PO.PO_RATE_TABLE == "IP_ITEM_RATE_APPLICAT_SETUP" && PO.PO_DIST_RATE_COLUMN != "SALES_RATE_ZERO")
            {
                newQuery = $@"SELECT 
            {dynamicRate}
            IRA.APP_DATE,
            RESULT.*,IRA.BRANCH_CODE rate_branch , RESULT.BRANCH_CODE order_branch
            FROM (SELECT DPO.ORDER_NO,
            DPO.ORDER_DATE,
            DPO.PARTY_TYPE_CODE,
            PTC.PARTY_TYPE_EDESC,
            DPO.CUSTOMER_CODE,
            CS.CUSTOMER_EDESC,
            DPO.ITEM_CODE,
            TRIM (IMS.ITEM_EDESC) ITEM_EDESC,
            DPO.MU_CODE,
            NVL (DPO.QUANTITY, 0) QUANTITY,
            NVL (DPO.APPROVE_QTY, 0) APPROVEQTY,
            NVL (DPO.UNIT_PRICE, 0) UNIT_PRICE,
            --NVL(ITRAS.SALES_RATE,0) NEW_UNIT_PRICE, --aaku
            IUS.MU_CODE CONVERSION_MU_CODE,
            CASE (DPO.MU_CODE)
            WHEN IUS.MU_CODE THEN 1
            ELSE IUS.CONVERSION_FACTOR
            END
            CONVERSION_FACTOR,
            DPO.TOTAL_PRICE,
            DPO.REMARKS,
            DPO.CREATED_BY,
            DPO.CREATED_DATE,
            DPO.MODIFY_DATE,
            DPO.DELETED_FLAG,
            DPO.CURRENCY_CODE,
            DPO.EXCHANGE_RATE,
            DPO.COMPANY_CODE,
            DPO.BRANCH_CODE,
            DPO.APPROVED_FLAG,
            DPO.DISPATCH_FLAG,
            DPO.ACKNOWLEDGE_FLAG,
            DPO.REJECT_FLAG,
            DPO.SYN_ROWID--,DPO.MODIFY_DATE
            ,
            DPO.MODIFY_BY,
            TRIM (IMS.ITEM_EDESC),
            ES.EMPLOYEE_EDESC,
            PS.PO_PARTY_TYPE,
            PS.PO_CONVERSION_FACTOR,
            PS.PO_BILLING_NAME,
            PS.PO_CUSTOM_RATE,
            PS.PO_REMARKS,
            PS.PO_CONVERSION_UNIT--,PS.PO_CONVERSION_FACTOR
            ,
            PS.CS_CONVERSION_UNIT,
            CS.CREDIT_LIMIT
            FROM DIST_IP_SSD_PURCHASE_ORDER DPO
            INNER JOIN IP_ITEM_MASTER_SETUP IMS
            ON IMS.ITEM_CODE = DPO.ITEM_CODE
            AND IMS.COMPANY_CODE = DPO.COMPANY_CODE
            AND IMS.CATEGORY_CODE IN
            (SELECT CATEGORY_CODE
            FROM IP_CATEGORY_CODE
            WHERE CATEGORY_TYPE IN ('FG', 'TF'))
            AND IMS.GROUP_SKU_FLAG = 'I'
            LEFT JOIN SA_CUSTOMER_SETUP CS
            ON CS.CUSTOMER_CODE = DPO.CUSTOMER_CODE
            AND CS.COMPANY_CODE = DPO.COMPANY_CODE
            AND CS.DELETED_FLAG = 'N'
            INNER JOIN DIST_LOGIN_USER LU
            ON LU.USERID = DPO.CREATED_BY
            LEFT JOIN HR_EMPLOYEE_SETUP ES
            ON ES.EMPLOYEE_CODE = LU.SP_CODE
            AND ES.COMPANY_CODE = LU.COMPANY_CODE
            LEFT JOIN IP_ITEM_UNIT_SETUP IUS
            ON IUS.ITEM_CODE = DPO.ITEM_CODE
            AND IUS.COMPANY_CODE = DPO.COMPANY_CODE
            INNER JOIN DIST_PREFERENCE_SETUP PS
            ON PS.COMPANY_CODE = DPO.COMPANY_CODE
            LEFT JOIN IP_PARTY_TYPE_CODE PTC
            ON PTC.PARTY_TYPE_CODE = DPO.PARTY_TYPE_CODE
            AND PTC.PARTY_TYPE_CODE = DPO.COMPANY_CODE
            AND PTC.DELETED_FLAG = 'N') RESULT
            INNER JOIN
            (SELECT tt.*
            FROM IP_ITEM_RATE_APPLICAT_SETUP tt
            INNER JOIN
            ( SELECT item_code, branch_code,company_code,MAX (app_date) AS MaxDateTime
            FROM IP_ITEM_RATE_APPLICAT_SETUP
            GROUP BY item_code,branch_code,company_code) groupedtt
            ON tt.item_code = groupedtt.item_code
            AND TT.BRANCH_CODE= groupedtt.BRANCH_CODE
            AND TT.COMPANY_CODE= groupedtt.COMPANY_CODE
            AND tt.app_date = groupedtt.MaxDateTime) IRA
            ON IRA.item_code = result.item_code
            AND IRA.COMPANY_CODE = RESULT.COMPANY_CODE
            AND IRA.BRANCH_CODE = RESULT.BRANCH_CODE
            --LEFT JOIN IP_ITEM_RATE_APPLICAT_SETUP ITRAS on ITRAS.ITEM_CODE = DPO.ITEM_CODE AND ITRAS.COMPANY_CODE = DPO.COMPANY_CODE --aaku
            WHERE 1 = 1 { flagFilter} AND RESULT.DELETED_FLAG = 'N' AND RESULT.ORDER_NO = '{orderCode}'
            ORDER BY UPPER (TRIM (RESULT.ITEM_EDESC))";
            }
            else
            {
                switch (ORDER_ENTITY)
                {
                    case "R":
                        newQuery = $@"SELECT DPO.ORDER_NO, DPO.ORDER_DATE, DPO.PARTY_TYPE_CODE, 
                                                PTC.PARTY_TYPE_EDESC, DPO.CUSTOMER_CODE, CS.CUSTOMER_EDESC, 
                                                DPO.ITEM_CODE,TRIM(IMS.ITEM_EDESC) ITEM_EDESC, DPO.MU_CODE, NVL(DPO.QUANTITY,0) QUANTITY, NVL(DPO.APPROVE_QTY,0) APPROVEQTY,
                                                          NVL(DPO.UNIT_PRICE,0) UNIT_PRICE, IUS.MU_CODE CONVERSION_MU_CODE, 
                                                CASE(DPO.MU_CODE)
                                                    WHEN IUS.MU_CODE THEN 1
                                                    ELSE IUS.CONVERSION_FACTOR
                                                  END
                                                CONVERSION_FACTOR,
                                                          DPO.TOTAL_PRICE, DPO.REMARKS, DPO.CREATED_BY, DPO.CREATED_DATE, 
                                                DPO.MODIFY_DATE, DPO.DELETED_FLAG, DPO.CURRENCY_CODE, DPO.EXCHANGE_RATE,
                                                          DPO.COMPANY_CODE, DPO.BRANCH_CODE,
                                                          DPO.APPROVED_FLAG, DPO.DISPATCH_FLAG, DPO.ACKNOWLEDGE_FLAG, 
                                                DPO.REJECT_FLAG, DPO.SYN_ROWID, DPO.MODIFY_DATE, DPO.MODIFY_BY,
                                                          TRIM(IMS.ITEM_EDESC),
                                                          ES.EMPLOYEE_EDESC,
                                                          PS.PO_PARTY_TYPE,
                                                          PS.PO_CONVERSION_FACTOR,
                                                          PS.PO_BILLING_NAME,
                                                          PS.PO_CUSTOM_RATE, PS.PO_REMARKS, PS.PO_CONVERSION_UNIT, 
                                                PS.PO_CONVERSION_FACTOR, PS.CS_CONVERSION_UNIT,
                                                          CS.CREDIT_LIMIT
                                                FROM DIST_IP_SSR_PURCHASE_ORDER DPO
                                                INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DPO.ITEM_CODE AND 
                                                IMS.COMPANY_CODE = DPO.COMPANY_CODE AND IMS.CATEGORY_CODE in (select CATEGORY_CODE  from IP_CATEGORY_CODE WHERE CATEGORY_TYPE IN ('FG','TF')) AND 
                                                IMS.GROUP_SKU_FLAG = 'I'
                                                LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DPO.CUSTOMER_CODE 
                                                AND CS.COMPANY_CODE = DPO.COMPANY_CODE AND CS.DELETED_FLAG = 'N'
                                                INNER JOIN DIST_LOGIN_USER LU ON LU.USERID = DPO.CREATED_BY
                                                INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = LU.SP_CODE AND 
                                                ES.COMPANY_CODE = LU.COMPANY_CODE
                                                LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON IUS.ITEM_CODE = DPO.ITEM_CODE AND 
                                                IUS.COMPANY_CODE = DPO.COMPANY_CODE
                                                INNER JOIN DIST_PREFERENCE_SETUP PS ON PS.COMPANY_CODE = 
                                                DPO.COMPANY_CODE
                                                LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = 
                                                DPO.PARTY_TYPE_CODE AND PTC.PARTY_TYPE_CODE = DPO.COMPANY_CODE AND 
                                                PTC.DELETED_FLAG = 'N'
                                                WHERE 1 = 1
                                                  {flagFilter}
                                                   AND DPO.DELETED_FLAG = 'N'
                                                   AND DPO.ORDER_NO = '{orderCode}'
                                                ORDER BY UPPER(TRIM(IMS.ITEM_EDESC))";
                        //                    newQuery = $@"SELECT NVL(IRA.SALES_RATE,0) NEW_RATE,
                        //NVL(IRA.SALES_RATE,0) * RESULT.APPROVEQTY NEW_TOTAL_AMOUNT,
                        //IRA.APP_DATE,
                        //RESULT.*,IRA.BRANCH_CODE rate_branch , RESULT.BRANCH_CODE order_branch
                        //FROM (SELECT DPO.ORDER_NO,
                        //DPO.ORDER_DATE,
                        //DPO.PARTY_TYPE_CODE,
                        //PTC.PARTY_TYPE_EDESC,
                        //DPO.CUSTOMER_CODE,
                        //CS.CUSTOMER_EDESC,
                        //DPO.ITEM_CODE,
                        //TRIM (IMS.ITEM_EDESC) ITEM_EDESC,
                        //DPO.MU_CODE,
                        //NVL (DPO.QUANTITY, 0) QUANTITY,
                        //NVL (DPO.APPROVE_QTY, 0) APPROVEQTY,
                        //NVL (DPO.UNIT_PRICE, 0) UNIT_PRICE,
                        //--NVL(ITRAS.SALES_RATE,0) NEW_UNIT_PRICE, --aaku
                        //IUS.MU_CODE CONVERSION_MU_CODE,
                        //CASE (DPO.MU_CODE)
                        //WHEN IUS.MU_CODE THEN 1
                        //ELSE IUS.CONVERSION_FACTOR
                        //END
                        //CONVERSION_FACTOR,
                        //DPO.TOTAL_PRICE,
                        //DPO.REMARKS,
                        //DPO.CREATED_BY,
                        //DPO.CREATED_DATE,
                        //DPO.MODIFY_DATE,
                        //DPO.DELETED_FLAG,
                        //DPO.CURRENCY_CODE,
                        //DPO.EXCHANGE_RATE,
                        //DPO.COMPANY_CODE,
                        //DPO.BRANCH_CODE,
                        //DPO.APPROVED_FLAG,
                        //DPO.DISPATCH_FLAG,
                        //DPO.ACKNOWLEDGE_FLAG,
                        //DPO.REJECT_FLAG,
                        //DPO.SYN_ROWID--,DPO.MODIFY_DATE
                        //,
                        //DPO.MODIFY_BY,
                        //TRIM (IMS.ITEM_EDESC),
                        //ES.EMPLOYEE_EDESC,
                        //PS.PO_PARTY_TYPE,
                        //PS.PO_CONVERSION_FACTOR,
                        //PS.PO_BILLING_NAME,
                        //PS.PO_CUSTOM_RATE,
                        //PS.PO_REMARKS,
                        //PS.PO_CONVERSION_UNIT--,PS.PO_CONVERSION_FACTOR
                        //,
                        //PS.CS_CONVERSION_UNIT,
                        //CS.CREDIT_LIMIT
                        //FROM DIST_IP_SSD_PURCHASE_ORDER DPO
                        //INNER JOIN IP_ITEM_MASTER_SETUP IMS
                        //ON IMS.ITEM_CODE = DPO.ITEM_CODE
                        //AND IMS.COMPANY_CODE = DPO.COMPANY_CODE
                        //AND IMS.CATEGORY_CODE IN
                        //(SELECT CATEGORY_CODE
                        //FROM IP_CATEGORY_CODE
                        //WHERE CATEGORY_TYPE IN ('FG', 'TF'))
                        //AND IMS.GROUP_SKU_FLAG = 'I'
                        //LEFT JOIN SA_CUSTOMER_SETUP CS
                        //ON CS.CUSTOMER_CODE = DPO.CUSTOMER_CODE
                        //AND CS.COMPANY_CODE = DPO.COMPANY_CODE
                        //AND CS.DELETED_FLAG = 'N'
                        //INNER JOIN DIST_LOGIN_USER LU
                        //ON LU.USERID = DPO.CREATED_BY
                        //LEFT JOIN HR_EMPLOYEE_SETUP ES
                        //ON ES.EMPLOYEE_CODE = LU.SP_CODE
                        //AND ES.COMPANY_CODE = LU.COMPANY_CODE
                        //LEFT JOIN IP_ITEM_UNIT_SETUP IUS
                        //ON IUS.ITEM_CODE = DPO.ITEM_CODE
                        //AND IUS.COMPANY_CODE = DPO.COMPANY_CODE
                        //INNER JOIN DIST_PREFERENCE_SETUP PS
                        //ON PS.COMPANY_CODE = DPO.COMPANY_CODE
                        //LEFT JOIN IP_PARTY_TYPE_CODE PTC
                        //ON PTC.PARTY_TYPE_CODE = DPO.PARTY_TYPE_CODE
                        //AND PTC.PARTY_TYPE_CODE = DPO.COMPANY_CODE
                        //AND PTC.DELETED_FLAG = 'N') RESULT
                        //INNER JOIN
                        //(SELECT tt.*
                        //FROM IP_ITEM_RATE_APPLICAT_SETUP tt
                        //INNER JOIN
                        //( SELECT item_code, branch_code,company_code,MAX (app_date) AS MaxDateTime
                        //FROM IP_ITEM_RATE_APPLICAT_SETUP
                        //GROUP BY item_code,branch_code,company_code) groupedtt
                        //ON tt.item_code = groupedtt.item_code
                        //AND TT.BRANCH_CODE= groupedtt.BRANCH_CODE
                        //AND TT.COMPANY_CODE= groupedtt.COMPANY_CODE
                        //AND tt.app_date = groupedtt.MaxDateTime) IRA
                        //ON IRA.item_code = result.item_code
                        //AND IRA.COMPANY_CODE = RESULT.COMPANY_CODE
                        //AND IRA.BRANCH_CODE = RESULT.BRANCH_CODE
                        //--LEFT JOIN IP_ITEM_RATE_APPLICAT_SETUP ITRAS on ITRAS.ITEM_CODE = DPO.ITEM_CODE AND ITRAS.COMPANY_CODE = DPO.COMPANY_CODE --aaku
                        //WHERE 1 = 1 { flagFilter} AND RESULT.DELETED_FLAG = 'N' AND RESULT.ORDER_NO = '{orderCode}'
                        //ORDER BY UPPER (TRIM (RESULT.ITEM_EDESC))";
                        break;
                    default:
                        newQuery = $@"SELECT DPO.ORDER_NO, DPO.ORDER_DATE, DPO.PARTY_TYPE_CODE, 
                                                PTC.PARTY_TYPE_EDESC, DPO.CUSTOMER_CODE, CS.CUSTOMER_EDESC, 
                                                DPO.ITEM_CODE,TRIM(IMS.ITEM_EDESC) ITEM_EDESC, DPO.MU_CODE, NVL(DPO.QUANTITY,0) QUANTITY, NVL(DPO.APPROVE_QTY,0) APPROVEQTY,
                                                          NVL(DPO.UNIT_PRICE,0) UNIT_PRICE, IUS.MU_CODE CONVERSION_MU_CODE, 
                                                CASE(DPO.MU_CODE)
                                                    WHEN IUS.MU_CODE THEN 1
                                                    ELSE IUS.CONVERSION_FACTOR
                                                  END
                                                CONVERSION_FACTOR,
                                                          DPO.TOTAL_PRICE, DPO.REMARKS, DPO.CREATED_BY, DPO.CREATED_DATE, 
                                                DPO.MODIFY_DATE, DPO.DELETED_FLAG, DPO.CURRENCY_CODE, DPO.EXCHANGE_RATE,
                                                          DPO.COMPANY_CODE, DPO.BRANCH_CODE,
                                                          DPO.APPROVED_FLAG, DPO.DISPATCH_FLAG, DPO.ACKNOWLEDGE_FLAG, 
                                                DPO.REJECT_FLAG, DPO.SYN_ROWID, DPO.MODIFY_DATE, DPO.MODIFY_BY,
                                                          TRIM(IMS.ITEM_EDESC),
                                                          ES.EMPLOYEE_EDESC,
                                                          PS.PO_PARTY_TYPE,
                                                          PS.PO_CONVERSION_FACTOR,
                                                          PS.PO_BILLING_NAME,
                                                          PS.PO_CUSTOM_RATE, PS.PO_REMARKS, PS.PO_CONVERSION_UNIT, 
                                                PS.PO_CONVERSION_FACTOR, PS.CS_CONVERSION_UNIT,
                                                          CS.CREDIT_LIMIT
                                                FROM DIST_IP_SSD_PURCHASE_ORDER DPO
                                                INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DPO.ITEM_CODE AND 
                                                IMS.COMPANY_CODE = DPO.COMPANY_CODE AND IMS.CATEGORY_CODE in (select CATEGORY_CODE  from IP_CATEGORY_CODE WHERE CATEGORY_TYPE IN ('FG','TF')) AND 
                                                IMS.GROUP_SKU_FLAG = 'I'
                                                LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DPO.CUSTOMER_CODE 
                                                AND CS.COMPANY_CODE = DPO.COMPANY_CODE AND CS.DELETED_FLAG = 'N'
                                                INNER JOIN DIST_LOGIN_USER LU ON LU.USERID = DPO.CREATED_BY
                                                LEFT JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = LU.SP_CODE AND 
                                                ES.COMPANY_CODE = LU.COMPANY_CODE
                                                LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON IUS.ITEM_CODE = DPO.ITEM_CODE AND 
                                                IUS.COMPANY_CODE = DPO.COMPANY_CODE
                                                INNER JOIN DIST_PREFERENCE_SETUP PS ON PS.COMPANY_CODE = 
                                                DPO.COMPANY_CODE
                                                LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = 
                                                DPO.PARTY_TYPE_CODE AND PTC.PARTY_TYPE_CODE = DPO.COMPANY_CODE AND 
                                                PTC.DELETED_FLAG = 'N'
                                                WHERE 1 = 1
                                                  {flagFilter}
                                                   AND DPO.DELETED_FLAG = 'N'
                                                   AND DPO.ORDER_NO = '{orderCode}'
                                                ORDER BY UPPER(TRIM(IMS.ITEM_EDESC))";

                        newQuery = $@"SELECT 
            {dynamicRate}
            IRA.APP_DATE,
            RESULT.*,IRA.BRANCH_CODE rate_branch , RESULT.BRANCH_CODE order_branch
            FROM (SELECT DPO.ORDER_NO,
            DPO.ORDER_DATE,
            DPO.PARTY_TYPE_CODE,
            PTC.PARTY_TYPE_EDESC,
            DPO.CUSTOMER_CODE,
            CS.CUSTOMER_EDESC,
            DPO.ITEM_CODE,
            TRIM (IMS.ITEM_EDESC) ITEM_EDESC,
            DPO.MU_CODE,
            NVL (DPO.QUANTITY, 0) QUANTITY,
            NVL (DPO.APPROVE_QTY, 0) APPROVEQTY,
            NVL (DPO.UNIT_PRICE, 0) UNIT_PRICE,
            --NVL(ITRAS.SALES_RATE,0) NEW_UNIT_PRICE, --aaku
            IUS.MU_CODE CONVERSION_MU_CODE,
            CASE (DPO.MU_CODE)
            WHEN IUS.MU_CODE THEN 1
            ELSE IUS.CONVERSION_FACTOR
            END
            CONVERSION_FACTOR,
            DPO.TOTAL_PRICE,
            DPO.REMARKS,
            DPO.CREATED_BY,
            DPO.CREATED_DATE,
            DPO.MODIFY_DATE,
            DPO.DELETED_FLAG,
            DPO.CURRENCY_CODE,
            DPO.EXCHANGE_RATE,
            DPO.COMPANY_CODE,
            DPO.BRANCH_CODE,
            DPO.APPROVED_FLAG,
            DPO.DISPATCH_FLAG,
            DPO.ACKNOWLEDGE_FLAG,
            DPO.REJECT_FLAG,
            DPO.SYN_ROWID--,DPO.MODIFY_DATE
            ,
            DPO.MODIFY_BY,
            TRIM (IMS.ITEM_EDESC),
            ES.EMPLOYEE_EDESC,
            PS.PO_PARTY_TYPE,
            PS.PO_CONVERSION_FACTOR,
            PS.PO_BILLING_NAME,
            PS.PO_CUSTOM_RATE,
            PS.PO_REMARKS,
            PS.PO_CONVERSION_UNIT--,PS.PO_CONVERSION_FACTOR
            ,
            PS.CS_CONVERSION_UNIT,
            CS.CREDIT_LIMIT
            FROM DIST_IP_SSD_PURCHASE_ORDER DPO
            INNER JOIN IP_ITEM_MASTER_SETUP IMS
            ON IMS.ITEM_CODE = DPO.ITEM_CODE
            AND IMS.COMPANY_CODE = DPO.COMPANY_CODE
            AND IMS.CATEGORY_CODE IN
            (SELECT CATEGORY_CODE
            FROM IP_CATEGORY_CODE
            WHERE CATEGORY_TYPE IN ('FG', 'TF'))
            AND IMS.GROUP_SKU_FLAG = 'I'
            LEFT JOIN SA_CUSTOMER_SETUP CS
            ON CS.CUSTOMER_CODE = DPO.CUSTOMER_CODE
            AND CS.COMPANY_CODE = DPO.COMPANY_CODE
            AND CS.DELETED_FLAG = 'N'
            INNER JOIN DIST_LOGIN_USER LU
            ON LU.USERID = DPO.CREATED_BY
            LEFT JOIN HR_EMPLOYEE_SETUP ES
            ON ES.EMPLOYEE_CODE = LU.SP_CODE
            AND ES.COMPANY_CODE = LU.COMPANY_CODE
            LEFT JOIN IP_ITEM_UNIT_SETUP IUS
            ON IUS.ITEM_CODE = DPO.ITEM_CODE
            AND IUS.COMPANY_CODE = DPO.COMPANY_CODE
            INNER JOIN DIST_PREFERENCE_SETUP PS
            ON PS.COMPANY_CODE = DPO.COMPANY_CODE
            LEFT JOIN IP_PARTY_TYPE_CODE PTC
            ON PTC.PARTY_TYPE_CODE = DPO.PARTY_TYPE_CODE
            AND PTC.PARTY_TYPE_CODE = DPO.COMPANY_CODE
            AND PTC.DELETED_FLAG = 'N') RESULT
            INNER JOIN
            (SELECT tt.*
            FROM IP_ITEM_RATE_APPLICAT_SETUP tt
            INNER JOIN
            ( SELECT item_code, branch_code,company_code,MAX (app_date) AS MaxDateTime
            FROM IP_ITEM_RATE_APPLICAT_SETUP
            GROUP BY item_code,branch_code,company_code) groupedtt
            ON tt.item_code = groupedtt.item_code
            AND TT.BRANCH_CODE= groupedtt.BRANCH_CODE
            AND TT.COMPANY_CODE= groupedtt.COMPANY_CODE
            AND tt.app_date = groupedtt.MaxDateTime) IRA
            ON IRA.item_code = result.item_code
            AND IRA.COMPANY_CODE = RESULT.COMPANY_CODE
            AND IRA.BRANCH_CODE = RESULT.BRANCH_CODE
            --LEFT JOIN IP_ITEM_RATE_APPLICAT_SETUP ITRAS on ITRAS.ITEM_CODE = DPO.ITEM_CODE AND ITRAS.COMPANY_CODE = DPO.COMPANY_CODE --aaku
            WHERE 1 = 1 { flagFilter} AND RESULT.DELETED_FLAG = 'N' AND RESULT.ORDER_NO = '{orderCode}'
            ORDER BY UPPER (TRIM (RESULT.ITEM_EDESC))";
                        break;
                }

            }
            var data = _objectEntity.SqlQuery<PurchaseOrderModel>(newQuery).ToList();
            return data;

        }

        public bool DeletePOItem(decimal orderCode, string itemCode, bool isParent)
        {
            try
            {
                var query = "";
                if (isParent)
                {
                    query = $@"update dist_ip_ssd_purchase_order
                                set REJECT_FLAG = 'Y', MODIFY_DATE=sysdate
                                WHERE ORDER_NO = '{orderCode}'";
                }
                else
                {
                    query = $@"update dist_ip_ssd_purchase_order
                                set REJECT_FLAG = 'Y',MODIFY_DATE=sysdate
                                WHERE ORDER_NO = '{orderCode}' AND ITEM_CODE = '{itemCode}'";
                }

                _objectEntity.ExecuteSqlCommand(query);
                _objectEntity.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public CreditDaysBalanceModel GetCreditDaysBalance(string companyCode, string subCode, string creditLimit = "", string daysLimit = "")
        {

            //string Query1 = $@"SELECT  VOUCHER_NO, VOUCHER_DATE,  FLOOR(SYSDATE - VOUCHER_DATE) DUEDAYS,
            //                    NVL(SUM(CUSTOMER_DRAMOUNT),0)  SALES_AMT
            //                    FROM V$QUICK_CUSTOMER_BALANCE
            //                    WHERE COMPANY_CODE IN(SELECT COMPANY_CODE FROM COMPANY_SETUP WHERE CONSOLIDATE_FLAG ='Y')
            //                    AND TO_DATE(VOUCHER_DATE) <= TO_DATE(TRUNC(SYSDATE))
            //                    AND DELETED_FLAG='N'
            //                    AND CUSTOMER_DRAMOUNT > 0
            //                    AND COMPANY_CODE= '{companyCode}'
            //                    AND SUB_CODE = '{subCode}'
            //                    group by  VOUCHER_NO, VOUCHER_DATE
            //                    ORDER BY VOUCHER_DATE, VOUCHER_NO";

            //string Query2 = $@"SELECT  NVL(SUM(NVL(A.CUSTOMER_CRAMOUNT,0)),0) + ABS(NVL(SUM(NVL(A.CUSTOMER_DRAMOUNT,0)),0)) TotalPaidAmt,CREDIT_DAYS      
            //                            FROM V$QUICK_CUSTOMER_BALANCE  A
            //                            wHERE A.DELETED_FLAG='N'
            //                            AND TO_DATE(A.VOUCHER_DATE) <= TO_DATE(TRUNC(SYSDATE))
            //                            AND (A.CUSTOMER_DRAMOUNT < 0 OR A.CUSTOMER_CRAMOUNT > 0)
            //                            AND A.COMPANY_CODE = '{companyCode}'
            //                            AND TRIM(A.SUB_CODE) = '{subCode}'
            //                            GROUP BY CREDIT_DAYS";

            //            string PL_BAL = $@"SELECT * FROM(
            //SELECT A.*, CS.CREDIT_DAYS, CS.CREDIT_LIMIT, A.VOUCHER_DATE+CS.CREDIT_DAYS DUE_DATE, TRUNC(SYSDATE)-A.VOUCHER_DATE+CS.CREDIT_DAYS OVERDUE_DAYS FROM(
            //SELECT DISTINCT SUB_CODE,SUB_EDESC, VOUCHER_NO,MANUAL_NO,VOUCHER_DATE,
            //--CREDIT_LIMIT, CREDIT_DAYS, VOUCHER_DATE+CREDIT_DAYS DUE_DATE, TRUNC(SYSDATE)-VOUCHER_DATE+CREDIT_DAYS OVERDUE_DAYS,
            //COALESCE( SUM(DR_AMOUNT - CR_AMOUNT) OVER (PARTITION BY SUB_EDESC ORDER BY VOUCHER_DATE
            //RANGE BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0 ) AS OP_BAL,
            //SUM(DR_AMOUNT) OVER (PARTITION BY SUB_EDESC, VOUCHER_DATE) AS DAILY_DR,
            //SUM(CR_AMOUNT) OVER (PARTITION BY SUB_EDESC, VOUCHER_DATE) AS DAILY_CR,
            //SUM(DR_AMOUNT - CR_AMOUNT) OVER (PARTITION BY SUB_EDESC ORDER BY VOUCHER_DATE) AS CL_BAL, COMPANY_CODE
            //FROM V$VIRTUAL_SUB_DEALER_LEDGER
            //WHERE 1=1
            //AND COMPANY_CODE='{companyCode}'
            //AND SUB_CODE= NVL('{subCode}',0)
            //--AND VOUCHER_NO <>'0'
            //--AND MANUAL_NO IS NOT NULL
            //ORDER BY COMPANY_CODE, SUB_EDESC, VOUCHER_DATE)A, SA_CUSTOMER_SETUP CS
            //WHERE A.COMPANY_CODE = CS.COMPANY_CODE
            //AND TRIM(A.SUB_CODE)= TRIM(CS.LINK_SUB_CODE)
            //ORDER BY VOUCHER_DATE DESC) WHERE ROWNUM=1";

            // var pendingData = _objectEntity.SqlQuery<PendingStatusModal>(PL_BAL).FirstOrDefault();

            //if (pendingData.CL_BAL)
            //{

            //}
            //else
            //{

            //}
            CreditDaysBalanceModel result = new CreditDaysBalanceModel();
            string creditLimitAmountQuery = string.Empty;

            string pendingBill = string.Empty;
            var so_consolidate_default = $@"SELECT  so_consolidate_default from dist_preference_setup WHERE COMPANY_CODE='{companyCode}'";
            var consolidate_flag_val = _objectEntity.SqlQuery<string>(so_consolidate_default).FirstOrDefault();
            // var refreshQuery = $@"BEGIN dbms_mview.refresh('M$V_MOVEMENT_ANALYSIS', 'C', '', true, false, 0, 0, 0, false, false) END";
            // var resultMvFresh = this._objectEntity.ExecuteSqlCommand(refreshQuery);
            var dueDays = 0;
            if (consolidate_flag_val == "Y")
            {

                creditLimitAmountQuery = $@"SELECT nvl(sum(NVL(CREDIT_LIMIT,0)),0) CREDIT_LIMIT, nvl(max(CREDIT_DAYS),0) CREDIT_DAYS FROM SA_CUSTOMER_SETUP WHERE LINK_SUB_CODE='{subCode}' AND DELETED_FLAG='N'";
                pendingBill = $@"SELECT VOUCHER_NO,VOUCHER_DATE,CREDIT_LIMIT,CREDIT_DAYS,TO_NUMBER(DUE_DAYS) DUE_DAYS,TO_NUMBER(SALES_AMT) SALES_AMT,TO_NUMBER(REC_AMT) REC_AMT,TO_NUMBER(BALANCE) BALANCE FROM M$V_MOVEMENT_ANALYSIS WHERE CUSTOMER_CODE= SUBSTR('{subCode}',2)";
                var creditLimitAmount = _objectEntity.SqlQuery<CreditDaysBalance>(creditLimitAmountQuery).FirstOrDefault();
                var pendingBillAmount = _objectEntity.SqlQuery<PendingBillData>(pendingBill).ToList();

                if (creditLimit == "Y" && daysLimit == "Y")
                {
                    result.CREDIT_DAYS = (int)creditLimitAmount.CREDIT_DAYS;
                    result.DUE_DAYS = pendingBillAmount == null ? 0 : (int)pendingBillAmount.Where(x => Convert.ToInt32(x.DUE_DAYS.Value) >= 0).Select(x => x.DUE_DAYS.Value).FirstOrDefault();
                    result.CREDIT_LIMIT = (creditLimitAmount.CREDIT_LIMIT <= 0 ? "Unlimited" : creditLimitAmount.CREDIT_LIMIT.ToString());
                    result.SALES_CREDIT_AMOUNT = Math.Round(pendingBillAmount == null ? 0 : Convert.ToDouble(pendingBillAmount.Sum(x => x.BALANCE.Value)));
                    result.FREE_LIMIT = result.CREDIT_LIMIT == "Unlimited" ? result.CREDIT_LIMIT : (Convert.ToDouble(creditLimitAmount.CREDIT_LIMIT) - result.SALES_CREDIT_AMOUNT).ToString();
                    return result;
                }
                else if (creditLimit == "Y" && daysLimit == "N")
                {
                    result.CREDIT_LIMIT = (creditLimitAmount.CREDIT_LIMIT <= 0 ? "Unlimited" : creditLimitAmount.CREDIT_LIMIT.ToString());
                    result.DUE_DAYS = pendingBillAmount == null ? 0 : (int)pendingBillAmount.Where(x => Convert.ToInt32(x.DUE_DAYS.Value) >= 0).Select(x => x.DUE_DAYS.Value).FirstOrDefault();
                    result.SALES_CREDIT_AMOUNT = Math.Round(Convert.ToDouble(pendingBillAmount.Sum(x => x.BALANCE.Value)), 2);
                    result.FREE_LIMIT = result.CREDIT_LIMIT == "Unlimited" ? "Unlimited" : (Convert.ToDouble(creditLimitAmount.CREDIT_LIMIT) - result.SALES_CREDIT_AMOUNT).ToString();
                    return result;
                }
                else if (creditLimit == "N" && daysLimit == "Y")
                {
                    result.CREDIT_DAYS = (int)creditLimitAmount.CREDIT_DAYS;
                    result.DUE_DAYS = pendingBillAmount == null ? 0 : (int)pendingBillAmount.Where(x => Convert.ToInt32(x.DUE_DAYS.Value) >= 0).Select(x => x.DUE_DAYS.Value).FirstOrDefault();
                    return result;
                }
            }
            else
            {
                creditLimitAmountQuery = $@"SELECT nvl(sum(NVL(CREDIT_LIMIT,0)),0) CREDIT_LIMIT, nvl(max(CREDIT_DAYS),0) CREDIT_DAYS FROM SA_CUSTOMER_SETUP WHERE LINK_SUB_CODE='{subCode}' AND DELETED_FLAG='N' AND COMPANY_CODE='{companyCode}'";
                pendingBill = $@"SELECT VOUCHER_NO,VOUCHER_DATE,CREDIT_LIMIT,CREDIT_DAYS,TO_NUMBER(DUE_DAYS) DUE_DAYS,TO_NUMBER(SALES_AMT) SALES_AMT,TO_NUMBER(REC_AMT) REC_AMT,TO_NUMBER(BALANCE) BALANCE FROM M$V_MOVEMENT_ANALYSIS WHERE CUSTOMER_CODE= SUBSTR('{subCode}',2)  AND COMPANY_CODE='{companyCode}'";
                var creditLimitAmount = _objectEntity.SqlQuery<CreditDaysBalance>(creditLimitAmountQuery).FirstOrDefault();
                var pendingBillAmount = _objectEntity.SqlQuery<PendingBillData>(pendingBill).ToList();

                if (creditLimit == "Y" && daysLimit == "Y")
                {
                    result.CREDIT_DAYS = (int)creditLimitAmount.CREDIT_DAYS;
                    result.DUE_DAYS = pendingBillAmount == null ? 0 : (int)pendingBillAmount.Where(x => x.DUE_DAYS.Value >= 0).Select(x => x.DUE_DAYS.Value).FirstOrDefault();
                    result.CREDIT_LIMIT = (creditLimitAmount.CREDIT_LIMIT <= 0 ? "Unlimited" : creditLimitAmount.CREDIT_LIMIT.ToString());
                    result.SALES_CREDIT_AMOUNT = Math.Round(pendingBillAmount == null ? 0 : Convert.ToDouble(pendingBillAmount.Sum(x => x.BALANCE.Value)), 2);
                    result.FREE_LIMIT = result.CREDIT_LIMIT == "Unlimited" ? result.CREDIT_LIMIT : (Convert.ToDouble(creditLimitAmount.CREDIT_LIMIT) - result.SALES_CREDIT_AMOUNT).ToString();
                    return result;
                }
                else if (creditLimit == "Y" && daysLimit == "N")
                {
                    result.CREDIT_LIMIT = (creditLimitAmount.CREDIT_LIMIT <= 0 ? "Unlimited" : creditLimitAmount.CREDIT_LIMIT.ToString());
                    result.DUE_DAYS = pendingBillAmount == null ? 0 : (int)pendingBillAmount.Where(x => x.DUE_DAYS.Value >= 0).Select(x => x.DUE_DAYS.Value).FirstOrDefault();
                    result.SALES_CREDIT_AMOUNT = Math.Round(Convert.ToDouble(pendingBillAmount.Sum(x => x.BALANCE.Value)), 2);
                    result.FREE_LIMIT = result.CREDIT_LIMIT == "Unlimited" ? "Unlimited" : (Convert.ToDouble(creditLimitAmount.CREDIT_LIMIT) - result.SALES_CREDIT_AMOUNT).ToString();
                    return result;

                }
                else if (creditLimit == "N" && daysLimit == "Y")
                {
                    result.CREDIT_DAYS = (int)creditLimitAmount.CREDIT_DAYS;
                    result.DUE_DAYS = pendingBillAmount == null ? 0 : (int)pendingBillAmount.Where(x => x.DUE_DAYS >= 0).Select(x => x.DUE_DAYS).FirstOrDefault();
                    return result;
                }
                else if (creditLimit == "N" && daysLimit == "N")
                {

                }

            }
            return result;
        }


        public List<VisitSummaryViewModel> GetVisitSummaryReport(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join(",", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;


            //**************************** 
            //CONDITIONS FITLER START HERE
            //****************************
            //for customer Filter
            var filter = string.Empty;
            var customerFilter = string.Empty;
            if (model.CustomerFilter.Count() > 0)
            {
                customerFilter = @"select  DISTINCT(customer_code) from sa_customer_setup where (";
                //IF CUSTOMER_SKU_FLAG = G
                foreach (var item in model.CustomerFilter)
                {
                    customerFilter += "master_customer_code like  (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + companyCode + ")) OR ";
                }
                customerFilter = customerFilter.Substring(0, customerFilter.Length - 3);
                //IF CUSTOMER_SKU_FLAG = I                
                customerFilter += " or (customer_code in (" + string.Join(",", model.CustomerFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + companyCode + "))) ";


                customerFilter = " and DLT.customer_code IN(" + customerFilter + ")";
            }


            var BranchFilterDLT = string.Empty;
            var BranchFilterDEA = string.Empty;
            if (model.BranchFilter.Count > 0)
            {
                BranchFilterDLT = string.Format(@" AND  DLT.BRANCH_CODE IN  ('{0}')", string.Join("','", model.BranchFilter).ToString());
                BranchFilterDEA = string.Format(@" AND  DEA.BRANCH_CODE IN  ('{0}')", string.Join("','", model.BranchFilter).ToString());
            }

            var FromDate = string.Empty;
            if (!string.IsNullOrEmpty(model.FromDate))
            {
                FromDate = string.Format(@" AND TRUNC(DLT.UPDATE_DATE) >= TO_DATE('" + model.FromDate + "','YYYY-MM-DD') AND TRUNC(DLT.UPDATE_DATE) <= TO_DATE('" + model.ToDate + "','YYYY-MM-DD')", string.Join("','", model.FromDate).ToString());
            }
            //query = query + " and A.SALES_DATE>=TO_DATE('" + model.FromDate + "', 'YYYY-MM-DD') and A.SALES_DATE <= TO_DATE('" + model.ToDate + "', 'YYYY-MM-DD')";

            var employeeFilterDLT = string.Empty;
            var employeeFilterDEA = string.Empty;
            if (model.ItemBrandFilter.Count > 0)
            {
                employeeFilterDEA = $" AND  DEA.SP_CODE IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
                employeeFilterDLT = $" AND  DLT.SP_CODE IN  ('{string.Join("','", model.ItemBrandFilter).ToString()}')";
                //employeeFilterDEA = $" AND  DEA.SP_CODE IN  (SELECT DISTINCT SP_CODE FROM DIST_USER_ITEM_MAPPING WHERE SP_CODE IN ({userInfo.sp_codes}) AND ITEM_CODE IN {string.Join("','", model.ItemBrandFilter).ToString()})";
                //employeeFilterDLT = $" AND  DLT.SP_CODE IN  (SELECT DISTINCT SP_CODE FROM DIST_USER_ITEM_MAPPING WHERE SP_CODE IN ({userInfo.sp_codes}) AND ITEM_CODE IN {string.Join("','", model.ItemBrandFilter).ToString()})";
            }
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                employeeFilterDEA = $" AND  DEA.SP_CODE IN  ({userInfo.sp_codes})";
                employeeFilterDLT = $" AND  DLT.SP_CODE IN  ({userInfo.sp_codes})";
            }

            if (model.AgentFilter.Count > 0)
            {
                filter = filter + string.Format(@" AND  SI.AGENT_CODE IN  ('{0}')", string.Join("','", model.AgentFilter).ToString());
            }
            if (model.DivisionFilter.Count > 0)
            {
                filter = filter + string.Format(@" AND  I.DIVISION_CODE IN  ('{0}')", string.Join("','", model.DivisionFilter).ToString());
            }
            string locationFilter = string.Empty;
            if (model.LocationFilter.Count > 0)
            {

                var locations = model.LocationFilter;
                for (int i = 0; i < locations.Count; i++)
                {

                    if (i == 0)
                        locationFilter += string.Format("SELECT LOCATION_CODE FROM IP_LOCATION_SETUP WHERE LOCATION_CODE LIKE '{0}%' ", locations[i]);
                    else
                    {
                        locationFilter += string.Format(" OR LOCATION_CODE like '{0}%' ", locations[i]);
                    }
                }
                locationFilter = string.Format(@" AND SI.FROM_LOCATION_CODE IN ({0} OR LOCATION_CODE IN ('{1}'))", locationFilter, string.Join("','", locations));
                //query = query.AppendFormat(@" AND A.FROM_LOCATION_CODE IN ('{0}')", string.Join("','", filters.LocationFilter).ToString());
                filter = filter + locationFilter;
            }
            //****************************
            //CONDITIONS FITLER END HERE
            //****************************

    string query = $@"SELECT * FROM (
	--Dealer
	SELECT dlt.sp_code visited_by
		,trunc(dlt.update_date) visit_date
		,bs_date(TO_CHAR(dlt.update_date)) miti
		,TO_DATE(TO_CHAR(dlt.update_date, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM') visit_time
		,dlt.customer_code
		,am.area_name
		,'-' outlet_type
		,'-' outlet_subtype
		,'-' group_name
		,dlt.latitude AS visit_lat
		,dlt.longitude AS visit_long
		,
		--DRE.ROUTE_CODE VISIT_ROUTE_CODE,
		TRIM(dlt.remarks) remarks
		,(
			CASE 
				WHEN dlt.is_visited IS NULL
					THEN 'PENDING VISIT'
				ELSE CASE 
						WHEN dlt.is_visited = 'Y'
							THEN 'VISIT'
						ELSE 'CANCELLED'
						END
				END
			) visit_type
		,drd.assign_date
		,drd.emp_code AS assigned_to
		,
		--DRD.ROUTE_CODE AS ASSIGNED_ROUTE,           
		'P' customer_type
		,dlt.company_code
		,dlt.branch_code
		,TRIM(hes1.employee_edesc) visit_by
		,TRIM(hes2.employee_edesc) assigned_employee
		,TRIM(ptc.party_type_edesc) customer_name
		,nvl(ddm.latitude, 0) cust_lat
		,nvl(ddm.longitude, 0) cust_long
	--TRIM(RM1.ROUTE_NAME) VISITED_ROUTE,
	--TRIM(RM2.ROUTE_NAME) ASSIGNED_ROUTE_NAME
	FROM dist_location_track dlt
	LEFT JOIN dist_route_entity dre ON dre.entity_code = dlt.customer_code
		AND dre.company_code = dlt.company_code
	LEFT JOIN dist_route_detail drd ON TRIM(drd.emp_code) = TRIM(dlt.sp_code)
		AND trunc(drd.assign_date) = trunc(dlt.update_date)
	INNER JOIN dist_dealer_master ddm ON ddm.dealer_code = dlt.customer_code
		AND ddm.company_code = dlt.company_code
	INNER JOIN ip_party_type_code ptc ON ptc.party_type_code = dlt.customer_code
		AND ptc.company_code = dlt.company_code
	INNER JOIN dist_login_user dlu1 ON TRIM(dlu1.sp_code) = TRIM(dlt.sp_code)
		AND dlu1.company_code = dlt.company_code
		AND dlu1.active = 'Y'
	INNER JOIN hr_employee_setup hes1 ON TRIM(hes1.employee_code) = TRIM(dlt.sp_code)
		AND hes1.company_code = dlu1.company_code
	-- ACTUAL COMPANY OF THE ASSIGNED EMPLOYEE
	LEFT JOIN dist_login_user dlu2 ON TRIM(dlu2.sp_code) = TRIM(drd.emp_code)
		AND dlu2.active = 'Y'
	LEFT JOIN hr_employee_setup hes2 ON TRIM(hes2.employee_code) = TRIM(drd.emp_code)
		AND hes2.company_code = dlu2.company_code
	LEFT JOIN dist_route_master rm1 ON rm1.route_code = dre.route_code
		AND rm1.company_code = dre.company_code
	LEFT JOIN dist_route_master rm2 ON rm2.route_code = drd.route_code
		AND rm2.company_code = drd.company_code
	LEFT JOIN dist_area_master am ON am.area_code = ddm.area_code
		AND am.company_code = ddm.company_code
	WHERE 1 = 1
		AND rm1.route_type = 'D'
		AND rm2.route_type = 'D'
		AND dlt.customer_type = 'P' {FromDate}
		AND DLT.COMPANY_CODE IN ({companyCode}) {BranchFilterDLT} {customerFilter} {employeeFilterDLT}
	GROUP BY dlt.sp_code
		,trunc(dlt.update_date)
		,bs_date(TO_CHAR(dlt.update_date))
		,TO_DATE(TO_CHAR(dlt.update_date, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM')
		,dlt.customer_code
		,dlt.latitude
		,dlt.longitude
		,
		--DRE.ROUTE_CODE,
		TRIM(dlt.remarks)
		,(
			CASE 
				WHEN dlt.is_visited IS NULL
					THEN 'PENDING VISIT'
				ELSE CASE 
						WHEN dlt.is_visited = 'Y'
							THEN 'VISIT'
						ELSE 'CANCELLED'
						END
				END
			)
		,drd.assign_date
		,drd.emp_code
		,
		--DRD.ROUTE_CODE,           
		'P'
		,dlt.company_code
		,dlt.branch_code
		,am.area_name
		,'-'
		,'-'
		,'-'
		,TRIM(hes1.employee_edesc)
		,TRIM(hes2.employee_edesc)
		,TRIM(ptc.party_type_edesc)
		,nvl(ddm.latitude, 0)
		,nvl(ddm.longitude, 0)
	--TRIM(RM1.ROUTE_NAME),
	--TRIM(RM2.ROUTE_NAME)
	
	UNION ALL
	
	--Distributor
	SELECT dlt.sp_code visited_by
		,trunc(dlt.update_date) visit_date
		,bs_date(TO_CHAR(dlt.update_date)) miti
		,TO_DATE(TO_CHAR(dlt.update_date, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM') visit_time
		,dlt.customer_code
		,vd.area_name
		,'-' outlet_type
		,'-' outlet_subtype
		,vd.group_name
		,dlt.latitude AS visit_lat
		,dlt.longitude AS visit_long
		,
		--DRE.ROUTE_CODE VISIT_ROUTE_CODE,
		TRIM(dlt.remarks) remarks
		,(
			CASE 
				WHEN dlt.is_visited IS NULL
					THEN 'PENDING VISIT'
				ELSE CASE 
						WHEN dlt.is_visited = 'Y'
							THEN 'VISIT'
						ELSE 'CANCELLED'
						END
				END
			) visit_type
		,drd.assign_date
		,drd.emp_code AS assigned_to
		,
		--DRD.ROUTE_CODE AS ASSIGNED_ROUTE,           
		'D' customer_type
		,dlt.company_code
		,dlt.branch_code
		,TRIM(hes1.employee_edesc) visit_by
		,TRIM(hes2.employee_edesc) assigned_employee
		,TRIM(scs.customer_edesc) customer_name
		,nvl(dim.latitude, 0) cust_lat
		,nvl(dim.longitude, 0) cust_long
	--TRIM(RM1.ROUTE_NAME) VISITED_ROUTE,
	--TRIM(RM2.ROUTE_NAME) ASSIGNED_ROUTE_NAME
	FROM dist_location_track dlt
	LEFT JOIN dist_route_entity dre ON dre.entity_code = dlt.customer_code
		AND dre.company_code = dlt.company_code
	LEFT JOIN dist_route_detail drd ON TRIM(drd.emp_code) = TRIM(dlt.sp_code)
		AND trunc(drd.assign_date) = trunc(dlt.update_date)
	INNER JOIN dist_distributor_master dim ON dim.distributor_code = dlt.customer_code
		AND dim.company_code = dlt.company_code
		AND dim.active = 'Y'
	INNER JOIN sa_customer_setup scs ON scs.customer_code = dlt.customer_code
		AND scs.company_code = dlt.company_code
	INNER JOIN dist_login_user dlu1 ON TRIM(dlu1.sp_code) = TRIM(dlt.sp_code)
		AND dlu1.company_code = dlt.company_code
		AND dlu1.active = 'Y'
	INNER JOIN hr_employee_setup hes1 ON TRIM(hes1.employee_code) = TRIM(dlt.sp_code)
		AND hes1.company_code = dlu1.company_code
	-- ACTUAL COMPANY OF THE ASSIGNED EMPLOYEE
	LEFT JOIN dist_login_user dlu2 ON TRIM(dlu2.sp_code) = TRIM(drd.emp_code)
		AND dlu2.active = 'Y'
	LEFT JOIN hr_employee_setup hes2 ON TRIM(hes2.employee_code) = TRIM(drd.emp_code)
		AND hes2.company_code = dlu2.company_code
	LEFT JOIN dist_route_master rm1 ON rm1.route_code = dre.route_code
		AND rm1.company_code = dre.company_code
	LEFT JOIN dist_route_master rm2 ON rm2.route_code = drd.route_code
		AND rm2.company_code = drd.company_code
	LEFT JOIN V_DIST_DISTRIBUTOR_DETAIL vd ON vd.distributor_code = dlt.customer_code
		AND vd.company_code = dlt.company_code
	WHERE 1 = 1
		AND rm1.route_type = 'D'
		AND rm2.route_type = 'D'
		AND dlt.customer_type = 'D' {FromDate}
		AND DLT.COMPANY_CODE IN ({companyCode}) {BranchFilterDLT} {customerFilter} {employeeFilterDLT}
	GROUP BY dlt.sp_code
		,trunc(dlt.update_date)
		,bs_date(TO_CHAR(dlt.update_date))
		,TO_DATE(TO_CHAR(dlt.update_date, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM')
		,dlt.customer_code
		,vd.area_name
		,'-'
		,'-'
		,vd.group_name
		,dlt.latitude
		,dlt.longitude
		,
		--DRE.ROUTE_CODE,
		TRIM(dlt.remarks)
		,(
			CASE 
				WHEN dlt.is_visited IS NULL
					THEN 'PENDING VISIT'
				ELSE CASE 
						WHEN dlt.is_visited = 'Y'
							THEN 'VISIT'
						ELSE 'CANCELLED'
						END
				END
			)
		,drd.assign_date
		,drd.emp_code
		,
		--DRD.ROUTE_CODE,           
		'D'
		,dlt.company_code
		,dlt.branch_code
		,TRIM(hes1.employee_edesc)
		,TRIM(hes2.employee_edesc)
		,TRIM(scs.customer_edesc)
		,nvl(dim.latitude, 0)
		,nvl(dim.longitude, 0)
	--TRIM(RM1.ROUTE_NAME),
	--TRIM(RM2.ROUTE_NAME)
	
	UNION ALL
	
	--RESELLER
	SELECT dlt.sp_code visited_by
		,trunc(dlt.update_date) visit_date
		,bs_date(TO_CHAR(dlt.update_date)) miti
		,TO_DATE(TO_CHAR(dlt.update_date, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM') visit_time
		,dlt.customer_code
		,vr.area_name
		,vr.outlet_type
		,vr.outlet_subtype
		,vr.group_name
		,dlt.latitude AS visit_lat
		,dlt.longitude AS visit_long
		,
		--DRE.ROUTE_CODE VISIT_ROUTE_CODE,
		TRIM(dlt.remarks) remarks
		,(
			CASE 
				WHEN dlt.is_visited IS NULL
					THEN 'PENDING VISIT'
				ELSE CASE 
						WHEN dlt.is_visited = 'Y'
							THEN 'VISIT'
						ELSE 'CANCELLED'
						END
				END
			) visit_type
		,drd.assign_date
		,drd.emp_code AS assigned_to
		,
		--DRD.ROUTE_CODE AS ASSIGNED_ROUTE,           
		'R' customer_type
		,dlt.company_code
		,dlt.branch_code
		,TRIM(hes1.employee_edesc) visit_by
		,TRIM(hes2.employee_edesc) assigned_employee
		,TRIM(drm.reseller_name) customer_name
		,nvl(drm.latitude, 0) cust_lat
		,nvl(drm.longitude, 0) cust_long
	--TRIM(RM1.ROUTE_NAME) VISITED_ROUTE,
	--TRIM(RM2.ROUTE_NAME) ASSIGNED_ROUTE_NAME
	FROM dist_location_track dlt
	LEFT JOIN dist_route_entity dre ON dre.entity_code = dlt.customer_code --AND DRE.COMPANY_CODE = DLT.COMPANY_CODE
	LEFT JOIN dist_route_detail drd ON TRIM(drd.emp_code) = TRIM(dlt.sp_code)
		AND trunc(drd.assign_date) = trunc(dlt.update_date) --AND DRD.ASSIGN_DATE BETWEEN TO_DATE('2021-Nov-23','YYYY-MM-DD') AND TO_DATE('2021-Nov-23','YYYY-MM-DD')
	INNER JOIN dist_reseller_master drm ON drm.reseller_code = dlt.customer_code --AND DRM.COMPANY_CODE = DLT.COMPANY_CODE
	INNER JOIN dist_login_user dlu1 ON TRIM(dlu1.sp_code) = TRIM(dlt.sp_code)
		AND dlu1.company_code = dlt.company_code
		AND dlu1.active = 'Y'
	INNER JOIN hr_employee_setup hes1 ON TRIM(hes1.employee_code) = TRIM(dlt.sp_code)
		AND hes1.company_code = dlu1.company_code
	-- ACTUAL COMPANY OF THE ASSIGNED EMPLOYEE
	LEFT JOIN dist_login_user dlu2 ON TRIM(dlu2.sp_code) = TRIM(drd.emp_code)
		AND dlu2.active = 'Y'
	LEFT JOIN hr_employee_setup hes2 ON TRIM(hes2.employee_code) = TRIM(drd.emp_code)
		AND hes2.company_code = dlu2.company_code
	LEFT JOIN dist_route_master rm1 ON rm1.route_code = dre.route_code
		AND rm1.company_code = dre.company_code
	LEFT JOIN dist_route_master rm2 ON rm2.route_code = drd.route_code
		AND rm2.company_code = drd.company_code
	LEFT JOIN V_RETAIL_DETAIL vr ON vr.reseller_code = dlt.customer_code
		AND vr.company_code = dlt.company_code
	WHERE 1 = 1
		AND drm.is_closed = 'N'
		AND rm1.route_type = 'D'
		AND rm2.route_type = 'D'
		AND dlt.customer_type = 'R' {FromDate}
		AND DLT.COMPANY_CODE IN ({companyCode}) {BranchFilterDLT} {customerFilter} {employeeFilterDLT}
	GROUP BY dlt.sp_code
		,trunc(dlt.update_date)
		,bs_date(TO_CHAR(dlt.update_date))
		,TO_DATE(TO_CHAR(dlt.update_date, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM')
		,dlt.customer_code
		,vr.area_name
		,vr.outlet_type
		,vr.outlet_subtype
		,vr.group_name
		,dlt.latitude
		,dlt.longitude
		,
		--DRE.ROUTE_CODE,
		TRIM(dlt.remarks)
		,(
			CASE 
				WHEN dlt.is_visited IS NULL
					THEN 'PENDING VISIT'
				ELSE CASE 
						WHEN dlt.is_visited = 'Y'
							THEN 'VISIT'
						ELSE 'CANCELLED'
						END
				END
			)
		,drd.assign_date
		,drd.emp_code
		,
		--DRD.ROUTE_CODE,           
		'R'
		,dlt.company_code
		,dlt.branch_code
		,TRIM(hes1.employee_edesc)
		,TRIM(hes2.employee_edesc)
		,TRIM(drm.reseller_name)
		,nvl(drm.latitude, 0)
		,nvl(drm.longitude, 0)
	--TRIM(RM1.ROUTE_NAME),
	--TRIM(RM2.ROUTE_NAME)    
	
	UNION ALL
	
	--Extra
	SELECT dea.sp_code visited_by
		,trunc(dea.visit_date) visit_date
		,bs_date(TO_CHAR(dea.visit_date)) miti
		,TO_DATE(TO_CHAR(dea.visit_date, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM') visit_time
		,'-' customer_code
		,'-' area_name
		,'-' outlet_type
		,'-' outlet_subtype
		,'-' group_name
		,dea.latitude AS visit_lat
		,dea.longitude AS visit_long
		,
		--'-' VISIT_ROUTE_CODE,
		TRIM(dea.remarks) remarks
		,'EXTRA' visit_type
		,dea.visit_date assign_date
		,'-' assigned_to
		,
		--'-' ASSIGNED_ROUTE,
		'-' customer_type
		,'-' company_code
		,'-' branch_code
		,hes.employee_edesc visit_by
		,'-' assigned_employee
		,'-' customer_name
		,'-' customer_lat
		,'-' customer_long
	--'-' VISITED_ROUTE,
	--'-' ASSIGNED_ROUTE_NAME
	FROM dist_extra_activity dea
	INNER JOIN hr_employee_setup hes ON hes.employee_code = dea.sp_code
		AND hes.company_code = dea.company_code
	WHERE 1 = 1
		AND DEA.COMPANY_CODE IN ({companyCode}) {BranchFilterDEA} {employeeFilterDEA}
		AND TRUNC(DEA.VISIT_DATE) BETWEEN TO_DATE('{model.FromDate}', 'YYYY-MM-DD')
			AND TO_DATE('{model.ToDate}', 'YYYY-MM-DD')
	GROUP BY dea.sp_code
		,dea.visit_date
		,bs_date(TO_CHAR(dea.visit_date))
		,trunc(dea.visit_date)
		,TO_DATE(TO_CHAR(dea.visit_date, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM')
		,'-'
		,'-'
		,'-'
		,'-'
		,'-'
		,dea.latitude
		,dea.longitude
		,
		--'-',
		TRIM(dea.remarks)
		,'EXTRA'
		,dea.visit_date
		,bs_date(TO_CHAR(dea.visit_date))
		,'-'
		,
		--'-',
		'-'
		,'-'
		,'-'
		,hes.employee_edesc
		,'-'
		,'-'
		,'-'
		,'-'
	)
--'-',
--'-'
ORDER BY visit_time DESC
	,customer_name ASC";

            query = string.Format(query, companyCode, BranchFilterDLT, BranchFilterDEA, customerFilter, employeeFilterDLT, FromDate);

            var data = _objectEntity.SqlQuery<VisitSummaryViewModel>(query).ToList();

            var imageQry = $@"SELECT IMAGE_CODE, IMAGE_NAME, IMAGE_TITLE, IMAGE_DESC, CATEGORYID, SP_CODE, ENTITY_CODE, TYPE,  trim(UPLOAD_DATE) UPLOAD_DATEString,UPLOAD_DATE, LATITUDE, LONGITUDE, COMPANY_CODE  FROM DIST_VISIT_IMAGE WHERE trunc(UPLOAD_DATE) BETWEEN TO_DATE('{model.FromDate}','YYYY-MM-DD') AND TO_DATE('{model.ToDate}','YYYY-MM-DD')";
            var imageResult = _objectEntity.SqlQuery<VisitImageModel>(imageQry).ToList();

            foreach (var item in data)
            {
                var hasImage = imageResult.Where(x => x.SP_CODE == item.VISITED_BY && x.ENTITY_CODE == item.CUSTOMER_CODE && Convert.ToDateTime(x.UPLOAD_DATEString) == item.Visit_Date).ToList();
                if (hasImage.Count > 0) item.HAS_IMAGE = true;
            }

            return data;

        }

        public List<VisitSummaryViewModel> GetVisitSummaryBrandingReport(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join(",", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;


            //**************************** 
            //CONDITIONS FITLER START HERE
            //****************************
            //for customer Filter
            var filter = string.Empty;
            var customerFilter = string.Empty;
            if (model.CustomerFilter.Count() > 0)
            {
                customerFilter = @"select  DISTINCT(customer_code) from sa_customer_setup where (";
                //IF CUSTOMER_SKU_FLAG = G
                foreach (var item in model.CustomerFilter)
                {
                    customerFilter += "master_customer_code like  (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + companyCode + ")) OR ";
                }
                customerFilter = customerFilter.Substring(0, customerFilter.Length - 3);
                //IF CUSTOMER_SKU_FLAG = I                
                customerFilter += " or (customer_code in (" + string.Join(",", model.CustomerFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + companyCode + "))) ";


                customerFilter = " and DLT.customer_code IN(" + customerFilter + ")";
            }


            var BranchFilterDLT = string.Empty;
            var BranchFilterDEA = string.Empty;
            if (model.BranchFilter.Count > 0)
            {
                BranchFilterDLT = string.Format(@" AND  DLT.BRANCH_CODE IN  ('{0}')", string.Join("','", model.BranchFilter).ToString());
                BranchFilterDEA = string.Format(@" AND  DEA.BRANCH_CODE IN  ('{0}')", string.Join("','", model.BranchFilter).ToString());
            }

            var FromDate = string.Empty;
            if (!string.IsNullOrEmpty(model.FromDate))
            {
                FromDate = string.Format(@" AND TRUNC(DLT.UPDATE_DATE) >= TO_DATE('" + model.FromDate + "','YYYY-MM-DD') AND TRUNC(DLT.UPDATE_DATE) <= TO_DATE('" + model.ToDate + "','YYYY-MM-DD')", string.Join("','", model.FromDate).ToString());
            }
            //query = query + " and A.SALES_DATE>=TO_DATE('" + model.FromDate + "', 'YYYY-MM-DD') and A.SALES_DATE <= TO_DATE('" + model.ToDate + "', 'YYYY-MM-DD')";

            var employeeFilterDLT = string.Empty;
            var employeeFilterDEA = string.Empty;
            if (model.ItemBrandFilter.Count > 0)
            {
                employeeFilterDEA = $" AND  DEA.SP_CODE IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
                employeeFilterDLT = $" AND  DLT.SP_CODE IN  ('{string.Join("','", model.ItemBrandFilter).ToString()}')";
            }
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                employeeFilterDEA = $" AND  DEA.SP_CODE IN  ({userInfo.sp_codes})";
                employeeFilterDLT = $" AND  DLT.SP_CODE IN  ({userInfo.sp_codes})";
            }

            if (model.AgentFilter.Count > 0)
            {
                filter = filter + string.Format(@" AND  SI.AGENT_CODE IN  ('{0}')", string.Join("','", model.AgentFilter).ToString());
            }
            if (model.DivisionFilter.Count > 0)
            {
                filter = filter + string.Format(@" AND  I.DIVISION_CODE IN  ('{0}')", string.Join("','", model.DivisionFilter).ToString());
            }
            string locationFilter = string.Empty;
            if (model.LocationFilter.Count > 0)
            {

                var locations = model.LocationFilter;
                for (int i = 0; i < locations.Count; i++)
                {

                    if (i == 0)
                        locationFilter += string.Format("SELECT LOCATION_CODE FROM IP_LOCATION_SETUP WHERE LOCATION_CODE LIKE '{0}%' ", locations[i]);
                    else
                    {
                        locationFilter += string.Format(" OR LOCATION_CODE like '{0}%' ", locations[i]);
                    }
                }
                locationFilter = string.Format(@" AND SI.FROM_LOCATION_CODE IN ({0} OR LOCATION_CODE IN ('{1}'))", locationFilter, string.Join("','", locations));

                filter = filter + locationFilter;
            }
            //****************************
            //CONDITIONS FITLER END HERE
            //****************************

            string query = $@"SELECT * FROM (
                          SELECT DLT.SP_CODE VISITED_BY,
                                  TRUNC(DLT.UPDATE_DATE) VISIT_DATE, BS_DATE(TO_CHAR(DLT.UPDATE_DATE)) MITI,
                                  TO_DATE(TO_CHAR(DLT.UPDATE_DATE, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM') VISIT_TIME,
                                  DLT.CUSTOMER_CODE,
                                  DLT.LATITUDE AS VISIT_LAT,
                                  DLT.LONGITUDE AS VISIT_LONG,
                                  --DRE.ROUTE_CODE VISIT_ROUTE_CODE,
                                  TRIM(DLT.REMARKS) REMARKS,
                                  (CASE
                                    WHEN DLT.IS_VISITED IS NULL THEN 'PENDING VISIT'
                                      ELSE
                                        CASE WHEN DLT.IS_VISITED ='Y' THEN 'VISIT' ELSE 'CANCELLED' END
                                      END
                                  ) VISIT_TYPE,
                                  DRD.ASSIGN_DATE,
                                  DRD.EMP_CODE AS ASSIGNED_TO,
                                  --DRD.ROUTE_CODE AS ASSIGNED_ROUTE,            
                                  'P' CUSTOMER_TYPE,
                                  DLT.COMPANY_CODE, DLT.BRANCH_CODE,
                                  TRIM(HES1.EMPLOYEE_EDESC) VISIT_BY,
                                  TRIM(HES2.EMPLOYEE_EDESC) ASSIGNED_EMPLOYEE,
                                  TRIM(PTC.PARTY_TYPE_EDESC) CUSTOMER_NAME,
                                  NVL(DDM.LATITUDE,0) CUST_LAT, NVL(DDM.LONGITUDE,0) CUST_LONG
                                  --TRIM(RM1.ROUTE_NAME) VISITED_ROUTE,
                                  --TRIM(RM2.ROUTE_NAME) ASSIGNED_ROUTE_NAME
                                FROM DIST_LOCATION_TRACK DLT
                                LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ENTITY_CODE = DLT.CUSTOMER_CODE AND DRE.COMPANY_CODE = DLT.COMPANY_CODE
                                LEFT JOIN DIST_BRANDING_ROUTE_DETAIL DRD ON TRIM(DRD.EMP_CODE) = TRIM(DLT.SP_CODE) AND TRUNC(DRD.ASSIGN_DATE) = TRUNC(DLT.UPDATE_DATE)  
                                INNER JOIN DIST_DEALER_MASTER DDM ON DDM.DEALER_CODE = DLT.CUSTOMER_CODE AND DDM.COMPANY_CODE = DLT.COMPANY_CODE
                                INNER JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DLT.CUSTOMER_CODE AND PTC.COMPANY_CODE = DLT.COMPANY_CODE
                                INNER JOIN DIST_LOGIN_USER DLU1 ON TRIM(DLU1.SP_CODE) = TRIM(DLT.SP_CODE) AND DLU1.COMPANY_CODE = DLT.COMPANY_CODE AND DLU1.ACTIVE = 'Y'
                                INNER JOIN HR_EMPLOYEE_SETUP HES1 ON TRIM(HES1.EMPLOYEE_CODE) = TRIM(DLT.SP_CODE) AND HES1.COMPANY_CODE = DLU1.COMPANY_CODE
                                -- ACTUAL COMPANY OF THE ASSIGNED EMPLOYEE
                                LEFT JOIN DIST_LOGIN_USER DLU2 ON TRIM(DLU2.SP_CODE) = TRIM(DRD.EMP_CODE) AND DLU2.ACTIVE = 'Y'
                                LEFT JOIN HR_EMPLOYEE_SETUP HES2 ON TRIM(HES2.EMPLOYEE_CODE) = TRIM(DRD.EMP_CODE) AND HES2.COMPANY_CODE = DLU2.COMPANY_CODE
                                LEFT JOIN DIST_ROUTE_MASTER RM1 ON RM1.ROUTE_CODE = DRE.ROUTE_CODE AND RM1.COMPANY_CODE = DRE.COMPANY_CODE
                                LEFT JOIN DIST_ROUTE_MASTER RM2 ON RM2.ROUTE_CODE = DRD.ROUTE_CODE AND RM2.COMPANY_CODE = DRD.COMPANY_CODE
                                WHERE 1 = 1
                                  AND RM1.ROUTE_TYPE='B'
                                  AND RM2.ROUTE_TYPE='B'
                                  AND DLT.CUSTOMER_TYPE = 'P' {FromDate}
                                  AND DLT.COMPANY_CODE IN ({companyCode}) {BranchFilterDLT} {customerFilter} {employeeFilterDLT}
                                GROUP BY DLT.SP_CODE,
                                  TRUNC(DLT.UPDATE_DATE),BS_DATE(TO_CHAR(DLT.UPDATE_DATE)),
                                  TO_DATE(TO_CHAR(DLT.UPDATE_DATE, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM'),
                                  DLT.CUSTOMER_CODE,
                                  DLT.LATITUDE,
                                  DLT.LONGITUDE,
                                  --DRE.ROUTE_CODE,
                                  TRIM(DLT.REMARKS),
                                  (CASE
                                    WHEN DLT.IS_VISITED IS NULL THEN 'PENDING VISIT'
                                      ELSE
                                        CASE WHEN DLT.IS_VISITED ='Y' THEN 'VISIT' ELSE 'CANCELLED' END
                                      END
                                  ),
                                  DRD.ASSIGN_DATE,
                                  DRD.EMP_CODE,
                                  --DRD.ROUTE_CODE,            
                                  'P',
                                  DLT.COMPANY_CODE, DLT.BRANCH_CODE,
                                  TRIM(HES1.EMPLOYEE_EDESC),
                                  TRIM(HES2.EMPLOYEE_EDESC),
                                  TRIM(PTC.PARTY_TYPE_EDESC), 
                                  NVL(DDM.LATITUDE,0), NVL(DDM.LONGITUDE,0)
                                  --TRIM(RM1.ROUTE_NAME),
                                  --TRIM(RM2.ROUTE_NAME)  
                                UNION ALL 
                                SELECT DLT.SP_CODE VISITED_BY,
                                  TRUNC(DLT.UPDATE_DATE) VISIT_DATE, BS_DATE(TO_CHAR(DLT.UPDATE_DATE)) MITI,
                                  TO_DATE(TO_CHAR(DLT.UPDATE_DATE, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM') VISIT_TIME, 
                                  DLT.CUSTOMER_CODE,
                                  DLT.LATITUDE AS VISIT_LAT,
                                  DLT.LONGITUDE AS VISIT_LONG,
                                  --DRE.ROUTE_CODE VISIT_ROUTE_CODE,
                                  TRIM(DLT.REMARKS) REMARKS,
                                  (CASE
                                    WHEN DLT.IS_VISITED IS NULL THEN 'PENDING VISIT'
                                      ELSE
                                        CASE WHEN DLT.IS_VISITED ='Y' THEN 'VISIT' ELSE 'CANCELLED' END
                                      END
                                  ) VISIT_TYPE,
                                  DRD.ASSIGN_DATE,
                                  DRD.EMP_CODE AS ASSIGNED_TO,
                                  --DRD.ROUTE_CODE AS ASSIGNED_ROUTE,            
                                  'D' CUSTOMER_TYPE,
                                  DLT.COMPANY_CODE, DLT.BRANCH_CODE,
                                  TRIM(HES1.EMPLOYEE_EDESC) VISIT_BY,
                                  TRIM(HES2.EMPLOYEE_EDESC) ASSIGNED_EMPLOYEE,
                                  TRIM(SCS.CUSTOMER_EDESC) CUSTOMER_NAME, 
                                  NVL(DIM.LATITUDE,0) CUST_LAT, NVL(DIM.LONGITUDE,0) CUST_LONG
                                  --TRIM(RM1.ROUTE_NAME) VISITED_ROUTE,
                                  --TRIM(RM2.ROUTE_NAME) ASSIGNED_ROUTE_NAME
                                FROM DIST_LOCATION_TRACK DLT
                                LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ENTITY_CODE = DLT.CUSTOMER_CODE AND DRE.COMPANY_CODE = DLT.COMPANY_CODE
                                LEFT JOIN DIST_BRANDING_ROUTE_DETAIL DRD ON TRIM(DRD.EMP_CODE) = TRIM(DLT.SP_CODE) AND TRUNC(DRD.ASSIGN_DATE) = TRUNC(DLT.UPDATE_DATE)  
                                INNER JOIN DIST_DISTRIBUTOR_MASTER DIM ON DIM.DISTRIBUTOR_CODE = DLT.CUSTOMER_CODE AND DIM.COMPANY_CODE = DLT.COMPANY_CODE AND DIM.ACTIVE = 'Y'
                                INNER JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DLT.CUSTOMER_CODE AND SCS.COMPANY_CODE = DLT.COMPANY_CODE
                                INNER JOIN DIST_LOGIN_USER DLU1 ON TRIM(DLU1.SP_CODE) = TRIM(DLT.SP_CODE) AND DLU1.COMPANY_CODE = DLT.COMPANY_CODE AND DLU1.ACTIVE = 'Y'
                                INNER JOIN HR_EMPLOYEE_SETUP HES1 ON TRIM(HES1.EMPLOYEE_CODE) = TRIM(DLT.SP_CODE) AND HES1.COMPANY_CODE = DLU1.COMPANY_CODE
                                -- ACTUAL COMPANY OF THE ASSIGNED EMPLOYEE
                                LEFT JOIN DIST_LOGIN_USER DLU2 ON TRIM(DLU2.SP_CODE) = TRIM(DRD.EMP_CODE) AND DLU2.ACTIVE = 'Y'
                                LEFT JOIN HR_EMPLOYEE_SETUP HES2 ON TRIM(HES2.EMPLOYEE_CODE) = TRIM(DRD.EMP_CODE) AND HES2.COMPANY_CODE = DLU2.COMPANY_CODE
                                LEFT JOIN DIST_ROUTE_MASTER RM1 ON RM1.ROUTE_CODE = DRE.ROUTE_CODE AND RM1.COMPANY_CODE = DRE.COMPANY_CODE
                                LEFT JOIN DIST_ROUTE_MASTER RM2 ON RM2.ROUTE_CODE = DRD.ROUTE_CODE AND RM2.COMPANY_CODE = DRD.COMPANY_CODE
                                WHERE 1 = 1
                                  AND RM1.ROUTE_TYPE='B'
                                  AND RM2.ROUTE_TYPE='B'
                                  AND DLT.CUSTOMER_TYPE = 'D' {FromDate}
                                  AND DLT.COMPANY_CODE IN ({companyCode}) {BranchFilterDLT} {customerFilter} {employeeFilterDLT}
                                GROUP BY DLT.SP_CODE, 
                                  TRUNC(DLT.UPDATE_DATE), BS_DATE(TO_CHAR(DLT.UPDATE_DATE)),
                                  TO_DATE(TO_CHAR(DLT.UPDATE_DATE, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM'),
                                  DLT.CUSTOMER_CODE,
                                  DLT.LATITUDE,
                                  DLT.LONGITUDE,
                                  --DRE.ROUTE_CODE,
                                  TRIM(DLT.REMARKS),
                                  (CASE
                                    WHEN DLT.IS_VISITED IS NULL THEN 'PENDING VISIT'
                                      ELSE
                                        CASE WHEN DLT.IS_VISITED ='Y' THEN 'VISIT' ELSE 'CANCELLED' END
                                      END
                                  ),
                                  DRD.ASSIGN_DATE,
                                  DRD.EMP_CODE,
                                  --DRD.ROUTE_CODE,            
                                  'D',
                                  DLT.COMPANY_CODE, DLT.BRANCH_CODE,
                                  TRIM(HES1.EMPLOYEE_EDESC),
                                  TRIM(HES2.EMPLOYEE_EDESC),
                                  TRIM(SCS.CUSTOMER_EDESC), 
                                  NVL(DIM.LATITUDE,0), NVL(DIM.LONGITUDE,0)
                                  --TRIM(RM1.ROUTE_NAME),
                                  --TRIM(RM2.ROUTE_NAME)
                                UNION ALL
                                SELECT DLT.SP_CODE VISITED_BY,
                                  TRUNC(DLT.UPDATE_DATE) VISIT_DATE, BS_DATE(TO_CHAR(DLT.UPDATE_DATE)) MITI,
                                  TO_DATE(TO_CHAR(DLT.UPDATE_DATE, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM') VISIT_TIME,
                                  DLT.CUSTOMER_CODE,
                                  DLT.LATITUDE AS VISIT_LAT,
                                  DLT.LONGITUDE AS VISIT_LONG,
                                  --DRE.ROUTE_CODE VISIT_ROUTE_CODE,
                                  TRIM(DLT.REMARKS) REMARKS,
                                  (CASE
                                    WHEN DLT.IS_VISITED IS NULL THEN 'PENDING VISIT'
                                      ELSE
                                        CASE WHEN DLT.IS_VISITED ='Y' THEN 'VISIT' ELSE 'CANCELLED' END
                                      END
                                  ) VISIT_TYPE,
                                  DRD.ASSIGN_DATE,
                                  DRD.EMP_CODE AS ASSIGNED_TO,
                                  --DRD.ROUTE_CODE AS ASSIGNED_ROUTE,            
                                  'R' CUSTOMER_TYPE,
                                  DLT.COMPANY_CODE, DLT.BRANCH_CODE,
                                  TRIM(HES1.EMPLOYEE_EDESC) VISIT_BY,
                                  TRIM(HES2.EMPLOYEE_EDESC) ASSIGNED_EMPLOYEE,
                                  TRIM(DRM.RESELLER_NAME) CUSTOMER_NAME,
                                  NVL(DRM.LATITUDE,0) CUST_LAT, NVL(DRM.LONGITUDE,0) CUST_LONG
                                  --TRIM(RM1.ROUTE_NAME) VISITED_ROUTE,
                                  --TRIM(RM2.ROUTE_NAME) ASSIGNED_ROUTE_NAME
                                FROM DIST_LOCATION_TRACK DLT
                                LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ENTITY_CODE = DLT.CUSTOMER_CODE --AND DRE.COMPANY_CODE = DLT.COMPANY_CODE
                                LEFT JOIN DIST_BRANDING_ROUTE_DETAIL DRD ON TRIM(DRD.EMP_CODE) = TRIM(DLT.SP_CODE) AND TRUNC(DRD.ASSIGN_DATE) = TRUNC(DLT.UPDATE_DATE) --AND DRD.ASSIGN_DATE BETWEEN TO_DATE('{model.FromDate}','YYYY-MM-DD') AND TO_DATE('{model.ToDate}','YYYY-MM-DD')
                                INNER JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DLT.CUSTOMER_CODE --AND DRM.COMPANY_CODE = DLT.COMPANY_CODE
                                INNER JOIN DIST_LOGIN_USER DLU1 ON TRIM(DLU1.SP_CODE) = TRIM(DLT.SP_CODE) AND DLU1.COMPANY_CODE = DLT.COMPANY_CODE AND DLU1.ACTIVE = 'Y'
                                INNER JOIN HR_EMPLOYEE_SETUP HES1 ON TRIM(HES1.EMPLOYEE_CODE) = TRIM(DLT.SP_CODE) AND HES1.COMPANY_CODE = DLU1.COMPANY_CODE
                                -- ACTUAL COMPANY OF THE ASSIGNED EMPLOYEE
                                LEFT JOIN DIST_LOGIN_USER DLU2 ON TRIM(DLU2.SP_CODE) = TRIM(DRD.EMP_CODE) AND DLU2.ACTIVE = 'Y'
                                LEFT JOIN HR_EMPLOYEE_SETUP HES2 ON TRIM(HES2.EMPLOYEE_CODE) = TRIM(DRD.EMP_CODE) AND HES2.COMPANY_CODE = DLU2.COMPANY_CODE
                                LEFT JOIN DIST_ROUTE_MASTER RM1 ON RM1.ROUTE_CODE = DRE.ROUTE_CODE AND RM1.COMPANY_CODE = DRE.COMPANY_CODE
                                LEFT JOIN DIST_ROUTE_MASTER RM2 ON RM2.ROUTE_CODE = DRD.ROUTE_CODE AND RM2.COMPANY_CODE = DRD.COMPANY_CODE
                                WHERE 1 = 1
                                  AND DRM.IS_CLOSED = 'N'
                                  AND RM1.ROUTE_TYPE='B'
                                  AND RM2.ROUTE_TYPE='B'
                                  AND DLT.CUSTOMER_TYPE = 'R' {FromDate}
                                  AND DLT.COMPANY_CODE IN ({companyCode}) {BranchFilterDLT} {customerFilter} {employeeFilterDLT}
                                GROUP BY DLT.SP_CODE,
                                  TRUNC(DLT.UPDATE_DATE), BS_DATE(TO_CHAR(DLT.UPDATE_DATE)),
                                   TO_DATE(TO_CHAR(DLT.UPDATE_DATE, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM'),
                                  DLT.CUSTOMER_CODE,
                                  DLT.LATITUDE,
                                  DLT.LONGITUDE,
                                  --DRE.ROUTE_CODE,

                                  TRIM(DLT.REMARKS),
                                  (CASE
                                    WHEN DLT.IS_VISITED IS NULL THEN 'PENDING VISIT'
                                      ELSE
                                        CASE WHEN DLT.IS_VISITED ='Y' THEN 'VISIT' ELSE 'CANCELLED' END
                                      END
                                  ),
                                  DRD.ASSIGN_DATE,
                                  DRD.EMP_CODE,
                                  --DRD.ROUTE_CODE,            
                                  'R',
                                  DLT.COMPANY_CODE, DLT.BRANCH_CODE,
                                  TRIM(HES1.EMPLOYEE_EDESC),
                                  TRIM(HES2.EMPLOYEE_EDESC),
                                  TRIM(DRM.RESELLER_NAME),
                                  NVL(DRM.LATITUDE,0), NVL(DRM.LONGITUDE,0)
                                  --TRIM(RM1.ROUTE_NAME),
                                  --TRIM(RM2.ROUTE_NAME)    
                                UNION ALL
                                SELECT DEA.SP_CODE VISITED_BY,
                                  TRUNC(DEA.VISIT_DATE) VISIT_DATE, BS_DATE(TO_CHAR(DEA.VISIT_DATE)) MITI,
                                  TO_DATE(TO_CHAR(DEA.VISIT_DATE, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM') VISIT_TIME,
                                   '-' CUSTOMER_CODE,
                                   DEA.LATITUDE AS VISIT_LAT,
                                   DEA.LONGITUDE AS VISIT_LONG,
                                   --'-' VISIT_ROUTE_CODE,
                                   TRIM(DEA.REMARKS) REMARKS,
                                   'EXTRA' VISIT_TYPE,
                                   DEA.VISIT_DATE ASSIGN_DATE,
                                   '-' ASSIGNED_TO,
                                   --'-' ASSIGNED_ROUTE,
                                   '-' CUSTOMER_TYPE,
                                   '-' COMPANY_CODE, '-' BRANCH_CODE,
                                   HES.EMPLOYEE_EDESC VISIT_BY,
                                   '-' ASSIGNED_EMPLOYEE,
                                   '-' CUSTOMER_NAME,
                                   '-' CUSTOMER_LAT,
                                   '-' CUSTOMER_LONG
                                   --'-' VISITED_ROUTE,
                                   --'-' ASSIGNED_ROUTE_NAME
                                FROM DIST_EXTRA_ACTIVITY DEA
                                INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DEA.SP_CODE AND HES.COMPANY_CODE = DEA.COMPANY_CODE
                                WHERE 1 = 1 AND DEA.SP_CODE IN (SELECT SP_CODE FROM DIST_LOGIN_USER WHERE BRANDING = 'Y')
                                  AND DEA.COMPANY_CODE IN ({companyCode}) {BranchFilterDEA} {employeeFilterDEA}
                                  AND TRUNC(DEA.VISIT_DATE) BETWEEN TO_DATE('{model.FromDate}','YYYY-MM-DD') AND TO_DATE('{model.ToDate}','YYYY-MM-DD')
                                GROUP BY DEA.SP_CODE,
                                  DEA.VISIT_DATE,BS_DATE(TO_CHAR(DEA.VISIT_DATE)),
                                  TRUNC(DEA.VISIT_DATE),
                                  TO_DATE(TO_CHAR(DEA.VISIT_DATE, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM'),
                                   '-',
                                   DEA.LATITUDE,
                                   DEA.LONGITUDE,
                                   --'-',
                                   TRIM(DEA.REMARKS),
                                   'EXTRA',
                                   DEA.VISIT_DATE, BS_DATE(TO_CHAR(DEA.VISIT_DATE)),
                                   '-',
                                   --'-',
                                   '-',
                                   '-', '-',
                                   HES.EMPLOYEE_EDESC,
                                   '-',
                                   '-',
                                   '-',
                                   '-'
                                   --'-',
                                   --'-'
                        ) ORDER BY VISIT_TIME DESC, CUSTOMER_NAME ASC
                        ";
            query = string.Format(query, companyCode, BranchFilterDLT, BranchFilterDEA, customerFilter, employeeFilterDLT, FromDate);

            var data = _objectEntity.SqlQuery<VisitSummaryViewModel>(query).ToList();

            var imageQry = $@"SELECT IMAGE_CODE, IMAGE_NAME, IMAGE_TITLE, IMAGE_DESC, CATEGORYID, SP_CODE, ENTITY_CODE, TYPE, UPLOAD_DATE, LATITUDE, LONGITUDE, COMPANY_CODE  FROM DIST_VISIT_IMAGE WHERE UPLOAD_DATE BETWEEN TO_DATE('{model.FromDate}','YYYY-MM-DD') AND TO_DATE('{model.ToDate}','YYYY-MM-DD')";
            var imageResult = _objectEntity.SqlQuery<VisitImageModel>(imageQry).ToList();

            foreach (var item in data)
            {
                var hasImage = imageResult.Where(x => x.SP_CODE == item.VISITED_BY && x.ENTITY_CODE == item.CUSTOMER_CODE && x.UPLOAD_DATE == item.Visit_Date).ToList();
                if (hasImage.Count > 0) item.HAS_IMAGE = true;
            }

            return data;

        }

        public List<VisitSummaryViewModel> GetVisitSummaryReportAll(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join(",", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;


            //**************************** 
            //CONDITIONS FITLER START HERE
            //****************************
            //for customer Filter
            var filter = string.Empty;
            var customerFilter = string.Empty;
            if (model.CustomerFilter.Count() > 0)
            {
                customerFilter = @"select  DISTINCT(customer_code) from sa_customer_setup where (";
                //IF CUSTOMER_SKU_FLAG = G
                foreach (var item in model.CustomerFilter)
                {
                    customerFilter += "master_customer_code like  (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + companyCode + ")) OR ";
                }
                customerFilter = customerFilter.Substring(0, customerFilter.Length - 3);
                //IF CUSTOMER_SKU_FLAG = I                
                customerFilter += " or (customer_code in (" + string.Join(",", model.CustomerFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + companyCode + "))) ";


                customerFilter = " and DLT.customer_code IN(" + customerFilter + ")";
            }


            var BranchFilterDLT = string.Empty;
            var BranchFilterDEA = string.Empty;
            if (model.BranchFilter.Count > 0)
            {
                BranchFilterDLT = string.Format(@" AND  DLT.BRANCH_CODE IN  ('{0}')", string.Join("','", model.BranchFilter).ToString());
                BranchFilterDEA = string.Format(@" AND  DEA.BRANCH_CODE IN  ('{0}')", string.Join("','", model.BranchFilter).ToString());
            }

            var FromDate = string.Empty;
            if (!string.IsNullOrEmpty(model.FromDate))
            {
                FromDate = string.Format(@" AND TRUNC(DLT.UPDATE_DATE) >= TO_DATE('" + model.FromDate + "','YYYY-MM-DD') AND TRUNC(DLT.UPDATE_DATE) <= TO_DATE('" + model.ToDate + "','YYYY-MM-DD')", string.Join("','", model.FromDate).ToString());
            }
            //query = query + " and A.SALES_DATE>=TO_DATE('" + model.FromDate + "', 'YYYY-MM-DD') and A.SALES_DATE <= TO_DATE('" + model.ToDate + "', 'YYYY-MM-DD')";

            var employeeFilter = string.Empty;
            if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                employeeFilter = $" AND  DRD.EMPLOYEE_CODE IN  ({userInfo.sp_codes})";
            }
            if (model.AgentFilter.Count > 0)
            {
                filter = filter + string.Format(@" AND  SI.AGENT_CODE IN  ('{0}')", string.Join("','", model.AgentFilter).ToString());
            }
            if (model.DivisionFilter.Count > 0)
            {
                filter = filter + string.Format(@" AND  I.DIVISION_CODE IN  ('{0}')", string.Join("','", model.DivisionFilter).ToString());
            }
            string locationFilter = string.Empty;
            if (model.LocationFilter.Count > 0)
            {

                var locations = model.LocationFilter;
                for (int i = 0; i < locations.Count; i++)
                {

                    if (i == 0)
                        locationFilter += string.Format("SELECT LOCATION_CODE FROM IP_LOCATION_SETUP WHERE LOCATION_CODE LIKE '{0}%' ", locations[i]);
                    else
                    {
                        locationFilter += string.Format(" OR LOCATION_CODE like '{0}%' ", locations[i]);
                    }
                }
                locationFilter = string.Format(@" AND SI.FROM_LOCATION_CODE IN ({0} OR LOCATION_CODE IN ('{1}'))", locationFilter, string.Join("','", locations));
                //query = query.AppendFormat(@" AND A.FROM_LOCATION_CODE IN ('{0}')", string.Join("','", filters.LocationFilter).ToString());
                filter = filter + locationFilter;
            }
            //****************************
            //CONDITIONS FITLER END HERE
            //****************************

            string query = $@"SELECT * FROM (
  SELECT DLT.SP_CODE VISITED_BY,
          TRUNC(DLT.UPDATE_DATE) VISIT_DATE, BS_DATE(TO_CHAR(DLT.UPDATE_DATE)) MITI,
          TO_DATE(TO_CHAR(DLT.UPDATE_DATE, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM') VISIT_TIME,
          DLT.CUSTOMER_CODE,
          DLT.LATITUDE AS VISIT_LAT,
          DLT.LONGITUDE AS VISIT_LONG,
          --DRE.ROUTE_CODE VISIT_ROUTE_CODE,
          TRIM(DLT.REMARKS) REMARKS,
          (CASE
            WHEN DLT.IS_VISITED IS NULL THEN 'PENDING VISIT'
              ELSE
                CASE WHEN DLT.IS_VISITED ='Y' THEN 'VISIT' ELSE 'CANCELLED' END
              END
          ) VISIT_TYPE,
          DRD.ASSIGN_DATE,
          DRD.EMP_CODE AS ASSIGNED_TO,
          --DRD.ROUTE_CODE AS ASSIGNED_ROUTE,            
          'P' CUSTOMER_TYPE,
          DLT.COMPANY_CODE, DLT.BRANCH_CODE,
          TRIM(HES1.EMPLOYEE_EDESC) VISIT_BY,
          TRIM(HES2.EMPLOYEE_EDESC) ASSIGNED_EMPLOYEE,
          TRIM(PTC.PARTY_TYPE_EDESC) CUSTOMER_NAME,
          NVL(DDM.LATITUDE,0) CUST_LAT, NVL(DDM.LONGITUDE,0) CUST_LONG
          --TRIM(RM1.ROUTE_NAME) VISITED_ROUTE,
          --TRIM(RM2.ROUTE_NAME) ASSIGNED_ROUTE_NAME
        FROM DIST_LOCATION_TRACK DLT
        LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ENTITY_CODE = DLT.CUSTOMER_CODE AND DRE.COMPANY_CODE = DLT.COMPANY_CODE
        LEFT JOIN DIST_ROUTE_DETAIL DRD ON TRIM(DRD.EMP_CODE) = TRIM(DLT.SP_CODE) AND TRUNC(DRD.ASSIGN_DATE) = TRUNC(DLT.UPDATE_DATE)  
        INNER JOIN DIST_DEALER_MASTER DDM ON DDM.DEALER_CODE = DLT.CUSTOMER_CODE AND DDM.COMPANY_CODE = DLT.COMPANY_CODE
        INNER JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DLT.CUSTOMER_CODE AND PTC.COMPANY_CODE = DLT.COMPANY_CODE
        INNER JOIN DIST_LOGIN_USER DLU1 ON TRIM(DLU1.SP_CODE) = TRIM(DLT.SP_CODE) AND DLU1.COMPANY_CODE = DLT.COMPANY_CODE AND DLU1.ACTIVE = 'Y'
        INNER JOIN HR_EMPLOYEE_SETUP HES1 ON TRIM(HES1.EMPLOYEE_CODE) = TRIM(DLT.SP_CODE) AND HES1.COMPANY_CODE = DLU1.COMPANY_CODE
        -- ACTUAL COMPANY OF THE ASSIGNED EMPLOYEE
        LEFT JOIN DIST_LOGIN_USER DLU2 ON TRIM(DLU2.SP_CODE) = TRIM(DRD.EMP_CODE) AND DLU2.ACTIVE = 'Y'
        LEFT JOIN HR_EMPLOYEE_SETUP HES2 ON TRIM(HES2.EMPLOYEE_CODE) = TRIM(DRD.EMP_CODE) AND HES2.COMPANY_CODE = DLU2.COMPANY_CODE
        LEFT JOIN DIST_ROUTE_MASTER RM1 ON RM1.ROUTE_CODE = DRE.ROUTE_CODE AND RM1.COMPANY_CODE = DRE.COMPANY_CODE
        LEFT JOIN DIST_ROUTE_MASTER RM2 ON RM2.ROUTE_CODE = DRD.ROUTE_CODE AND RM2.COMPANY_CODE = DRD.COMPANY_CODE
        WHERE 1 = 1
          AND RM1.ROUTE_TYPE='D'
          AND RM2.ROUTE_TYPE='D'
          AND DLT.CUSTOMER_TYPE = 'P' {FromDate}
          AND DLT.COMPANY_CODE IN ({companyCode}) {BranchFilterDLT} {customerFilter}
        GROUP BY DLT.SP_CODE,
          TRUNC(DLT.UPDATE_DATE),BS_DATE(TO_CHAR(DLT.UPDATE_DATE)),
          TO_DATE(TO_CHAR(DLT.UPDATE_DATE, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM'),
          DLT.CUSTOMER_CODE,
          DLT.LATITUDE,
          DLT.LONGITUDE,
          --DRE.ROUTE_CODE,
          TRIM(DLT.REMARKS),
          (CASE
            WHEN DLT.IS_VISITED IS NULL THEN 'PENDING VISIT'
              ELSE
                CASE WHEN DLT.IS_VISITED ='Y' THEN 'VISIT' ELSE 'CANCELLED' END
              END
          ),
          DRD.ASSIGN_DATE,
          DRD.EMP_CODE,
          --DRD.ROUTE_CODE,            
          'P',
          DLT.COMPANY_CODE, DLT.BRANCH_CODE,
          TRIM(HES1.EMPLOYEE_EDESC),
          TRIM(HES2.EMPLOYEE_EDESC),
          TRIM(PTC.PARTY_TYPE_EDESC), 
          NVL(DDM.LATITUDE,0), NVL(DDM.LONGITUDE,0)
          --TRIM(RM1.ROUTE_NAME),
          --TRIM(RM2.ROUTE_NAME)  
        UNION ALL 
        SELECT DLT.SP_CODE VISITED_BY,
          TRUNC(DLT.UPDATE_DATE) VISIT_DATE, BS_DATE(TO_CHAR(DLT.UPDATE_DATE)) MITI,
          TO_DATE(TO_CHAR(DLT.UPDATE_DATE, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM') VISIT_TIME, 
          DLT.CUSTOMER_CODE,
          DLT.LATITUDE AS VISIT_LAT,
          DLT.LONGITUDE AS VISIT_LONG,
          --DRE.ROUTE_CODE VISIT_ROUTE_CODE,
          TRIM(DLT.REMARKS) REMARKS,
          (CASE
            WHEN DLT.IS_VISITED IS NULL THEN 'PENDING VISIT'
              ELSE
                CASE WHEN DLT.IS_VISITED ='Y' THEN 'VISIT' ELSE 'CANCELLED' END
              END
          ) VISIT_TYPE,
          DRD.ASSIGN_DATE,
          DRD.EMP_CODE AS ASSIGNED_TO,
          --DRD.ROUTE_CODE AS ASSIGNED_ROUTE,            
          'D' CUSTOMER_TYPE,
          DLT.COMPANY_CODE, DLT.BRANCH_CODE,
          TRIM(HES1.EMPLOYEE_EDESC) VISIT_BY,
          TRIM(HES2.EMPLOYEE_EDESC) ASSIGNED_EMPLOYEE,
          TRIM(SCS.CUSTOMER_EDESC) CUSTOMER_NAME, 
          NVL(DIM.LATITUDE,0) CUST_LAT, NVL(DIM.LONGITUDE,0) CUST_LONG
          --TRIM(RM1.ROUTE_NAME) VISITED_ROUTE,
          --TRIM(RM2.ROUTE_NAME) ASSIGNED_ROUTE_NAME
        FROM DIST_LOCATION_TRACK DLT
        LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ENTITY_CODE = DLT.CUSTOMER_CODE AND DRE.COMPANY_CODE = DLT.COMPANY_CODE
        LEFT JOIN DIST_ROUTE_DETAIL DRD ON TRIM(DRD.EMP_CODE) = TRIM(DLT.SP_CODE) AND TRUNC(DRD.ASSIGN_DATE) = TRUNC(DLT.UPDATE_DATE)  
        INNER JOIN DIST_DISTRIBUTOR_MASTER DIM ON DIM.DISTRIBUTOR_CODE = DLT.CUSTOMER_CODE AND DIM.COMPANY_CODE = DLT.COMPANY_CODE AND DIM.ACTIVE = 'Y'
        INNER JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DLT.CUSTOMER_CODE AND SCS.COMPANY_CODE = DLT.COMPANY_CODE
        INNER JOIN DIST_LOGIN_USER DLU1 ON TRIM(DLU1.SP_CODE) = TRIM(DLT.SP_CODE) AND DLU1.COMPANY_CODE = DLT.COMPANY_CODE AND DLU1.ACTIVE = 'Y'
        INNER JOIN HR_EMPLOYEE_SETUP HES1 ON TRIM(HES1.EMPLOYEE_CODE) = TRIM(DLT.SP_CODE) AND HES1.COMPANY_CODE = DLU1.COMPANY_CODE
        -- ACTUAL COMPANY OF THE ASSIGNED EMPLOYEE
        LEFT JOIN DIST_LOGIN_USER DLU2 ON TRIM(DLU2.SP_CODE) = TRIM(DRD.EMP_CODE) AND DLU2.ACTIVE = 'Y'
        LEFT JOIN HR_EMPLOYEE_SETUP HES2 ON TRIM(HES2.EMPLOYEE_CODE) = TRIM(DRD.EMP_CODE) AND HES2.COMPANY_CODE = DLU2.COMPANY_CODE
        LEFT JOIN DIST_ROUTE_MASTER RM1 ON RM1.ROUTE_CODE = DRE.ROUTE_CODE AND RM1.COMPANY_CODE = DRE.COMPANY_CODE
        LEFT JOIN DIST_ROUTE_MASTER RM2 ON RM2.ROUTE_CODE = DRD.ROUTE_CODE AND RM2.COMPANY_CODE = DRD.COMPANY_CODE
        WHERE 1 = 1
          AND RM1.ROUTE_TYPE='D'
          AND RM2.ROUTE_TYPE='D'
          AND DLT.CUSTOMER_TYPE = 'D' {FromDate}
          AND DLT.COMPANY_CODE IN ({companyCode}) {BranchFilterDLT} {customerFilter}
        GROUP BY DLT.SP_CODE, 
          TRUNC(DLT.UPDATE_DATE), BS_DATE(TO_CHAR(DLT.UPDATE_DATE)),
          TO_DATE(TO_CHAR(DLT.UPDATE_DATE, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM'),
          DLT.CUSTOMER_CODE,
          DLT.LATITUDE,
          DLT.LONGITUDE,
          --DRE.ROUTE_CODE,
          TRIM(DLT.REMARKS),
          (CASE
            WHEN DLT.IS_VISITED IS NULL THEN 'PENDING VISIT'
              ELSE
                CASE WHEN DLT.IS_VISITED ='Y' THEN 'VISIT' ELSE 'CANCELLED' END
              END
          ),
          DRD.ASSIGN_DATE,
          DRD.EMP_CODE,
          --DRD.ROUTE_CODE,            
          'D',
          DLT.COMPANY_CODE, DLT.BRANCH_CODE,
          TRIM(HES1.EMPLOYEE_EDESC),
          TRIM(HES2.EMPLOYEE_EDESC),
          TRIM(SCS.CUSTOMER_EDESC), 
          NVL(DIM.LATITUDE,0), NVL(DIM.LONGITUDE,0)
          --TRIM(RM1.ROUTE_NAME),
          --TRIM(RM2.ROUTE_NAME)
        UNION ALL
        SELECT DLT.SP_CODE VISITED_BY,
          TRUNC(DLT.UPDATE_DATE) VISIT_DATE, BS_DATE(TO_CHAR(DLT.UPDATE_DATE)) MITI,
          TO_DATE(TO_CHAR(DLT.UPDATE_DATE, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM') VISIT_TIME,
          DLT.CUSTOMER_CODE,
          DLT.LATITUDE AS VISIT_LAT,
          DLT.LONGITUDE AS VISIT_LONG,
          --DRE.ROUTE_CODE VISIT_ROUTE_CODE,
          TRIM(DLT.REMARKS) REMARKS,
          (CASE
            WHEN DLT.IS_VISITED IS NULL THEN 'PENDING VISIT'
              ELSE
                CASE WHEN DLT.IS_VISITED ='Y' THEN 'VISIT' ELSE 'CANCELLED' END
              END
          ) VISIT_TYPE,
          DRD.ASSIGN_DATE,
          DRD.EMP_CODE AS ASSIGNED_TO,
          --DRD.ROUTE_CODE AS ASSIGNED_ROUTE,            
          'R' CUSTOMER_TYPE,
          DLT.COMPANY_CODE, DLT.BRANCH_CODE,
          TRIM(HES1.EMPLOYEE_EDESC) VISIT_BY,
          TRIM(HES2.EMPLOYEE_EDESC) ASSIGNED_EMPLOYEE,
          TRIM(DRM.RESELLER_NAME) CUSTOMER_NAME,
          NVL(DRM.LATITUDE,0) CUST_LAT, NVL(DRM.LONGITUDE,0) CUST_LONG
          --TRIM(RM1.ROUTE_NAME) VISITED_ROUTE,
          --TRIM(RM2.ROUTE_NAME) ASSIGNED_ROUTE_NAME
        FROM DIST_LOCATION_TRACK DLT
        LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ENTITY_CODE = DLT.CUSTOMER_CODE --AND DRE.COMPANY_CODE = DLT.COMPANY_CODE
        LEFT JOIN DIST_ROUTE_DETAIL DRD ON TRIM(DRD.EMP_CODE) = TRIM(DLT.SP_CODE) AND TRUNC(DRD.ASSIGN_DATE) = TRUNC(DLT.UPDATE_DATE) --AND DRD.ASSIGN_DATE BETWEEN TO_DATE('{model.FromDate}','YYYY-MM-DD') AND TO_DATE('{model.ToDate}','YYYY-MM-DD')
        INNER JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DLT.CUSTOMER_CODE --AND DRM.COMPANY_CODE = DLT.COMPANY_CODE
        INNER JOIN DIST_LOGIN_USER DLU1 ON TRIM(DLU1.SP_CODE) = TRIM(DLT.SP_CODE) AND DLU1.COMPANY_CODE = DLT.COMPANY_CODE AND DLU1.ACTIVE = 'Y'
        INNER JOIN HR_EMPLOYEE_SETUP HES1 ON TRIM(HES1.EMPLOYEE_CODE) = TRIM(DLT.SP_CODE) AND HES1.COMPANY_CODE = DLU1.COMPANY_CODE
        -- ACTUAL COMPANY OF THE ASSIGNED EMPLOYEE
        LEFT JOIN DIST_LOGIN_USER DLU2 ON TRIM(DLU2.SP_CODE) = TRIM(DRD.EMP_CODE) AND DLU2.ACTIVE = 'Y'
        LEFT JOIN HR_EMPLOYEE_SETUP HES2 ON TRIM(HES2.EMPLOYEE_CODE) = TRIM(DRD.EMP_CODE) AND HES2.COMPANY_CODE = DLU2.COMPANY_CODE
        LEFT JOIN DIST_ROUTE_MASTER RM1 ON RM1.ROUTE_CODE = DRE.ROUTE_CODE AND RM1.COMPANY_CODE = DRE.COMPANY_CODE
        LEFT JOIN DIST_ROUTE_MASTER RM2 ON RM2.ROUTE_CODE = DRD.ROUTE_CODE AND RM2.COMPANY_CODE = DRD.COMPANY_CODE
        WHERE 1 = 1
          AND DRM.IS_CLOSED = 'N'
          AND RM1.ROUTE_TYPE='D'
          AND RM2.ROUTE_TYPE='D'
          AND DLT.CUSTOMER_TYPE = 'R' {FromDate}
          AND DLT.COMPANY_CODE IN ({companyCode}) {BranchFilterDLT} {customerFilter}
        GROUP BY DLT.SP_CODE,
          TRUNC(DLT.UPDATE_DATE), BS_DATE(TO_CHAR(DLT.UPDATE_DATE)),
           TO_DATE(TO_CHAR(DLT.UPDATE_DATE, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM'),
          DLT.CUSTOMER_CODE,
          DLT.LATITUDE,
          DLT.LONGITUDE,
          --DRE.ROUTE_CODE,

          TRIM(DLT.REMARKS),
          (CASE
            WHEN DLT.IS_VISITED IS NULL THEN 'PENDING VISIT'
              ELSE
                CASE WHEN DLT.IS_VISITED ='Y' THEN 'VISIT' ELSE 'CANCELLED' END
              END
          ),
          DRD.ASSIGN_DATE,
          DRD.EMP_CODE,
          --DRD.ROUTE_CODE,            
          'R',
          DLT.COMPANY_CODE, DLT.BRANCH_CODE,
          TRIM(HES1.EMPLOYEE_EDESC),
          TRIM(HES2.EMPLOYEE_EDESC),
          TRIM(DRM.RESELLER_NAME),
          NVL(DRM.LATITUDE,0), NVL(DRM.LONGITUDE,0)
          --TRIM(RM1.ROUTE_NAME),
          --TRIM(RM2.ROUTE_NAME)    
        UNION ALL
        SELECT DEA.SP_CODE VISITED_BY,
          TRUNC(DEA.VISIT_DATE) VISIT_DATE, BS_DATE(TO_CHAR(DEA.VISIT_DATE)) MITI,
          TO_DATE(TO_CHAR(DEA.VISIT_DATE, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM') VISIT_TIME,
           '-' CUSTOMER_CODE,
           DEA.LATITUDE AS VISIT_LAT,
           DEA.LONGITUDE AS VISIT_LONG,
           --'-' VISIT_ROUTE_CODE,
           TRIM(DEA.REMARKS) REMARKS,
           'EXTRA' VISIT_TYPE,
           DEA.VISIT_DATE ASSIGN_DATE,
           '-' ASSIGNED_TO,
           --'-' ASSIGNED_ROUTE,
           '-' CUSTOMER_TYPE,
           '-' COMPANY_CODE, '-' BRANCH_CODE,
           HES.EMPLOYEE_EDESC VISIT_BY,
           '-' ASSIGNED_EMPLOYEE,
           '-' CUSTOMER_NAME,
           '-' CUSTOMER_LAT,
           '-' CUSTOMER_LONG
           --'-' VISITED_ROUTE,
           --'-' ASSIGNED_ROUTE_NAME
        FROM DIST_EXTRA_ACTIVITY DEA
        INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DEA.SP_CODE AND HES.COMPANY_CODE = DEA.COMPANY_CODE
        WHERE 1 = 1
          AND DEA.COMPANY_CODE IN ({companyCode}) {BranchFilterDEA}
          AND TRUNC(DEA.VISIT_DATE) BETWEEN TO_DATE('{model.FromDate}','YYYY-MM-DD') AND TO_DATE('{model.ToDate}','YYYY-MM-DD')
        GROUP BY DEA.SP_CODE,
          DEA.VISIT_DATE,BS_DATE(TO_CHAR(DEA.VISIT_DATE)),
          TRUNC(DEA.VISIT_DATE),
          TO_DATE(TO_CHAR(DEA.VISIT_DATE, 'DD-MON-RRRR HH:MI AM'), 'DD-MON-RRRR HH:MI AM'),
           '-',
           DEA.LATITUDE,
           DEA.LONGITUDE,
           --'-',
           TRIM(DEA.REMARKS),
           'EXTRA',
           DEA.VISIT_DATE, BS_DATE(TO_CHAR(DEA.VISIT_DATE)),
           '-',
           --'-',
           '-',
           '-', '-',
           HES.EMPLOYEE_EDESC,
           '-',
           '-',
           '-',
           '-'
           --'-',
           --'-'
) ORDER BY VISIT_TIME DESC, CUSTOMER_NAME ASC
";
            query = string.Format(query, companyCode, BranchFilterDLT, BranchFilterDEA, customerFilter, employeeFilter, FromDate);
            var data = _objectEntity.SqlQuery<VisitSummaryViewModel>(query).ToList();
            return data;

        }

        public List<EmployeeActivityDetail> GetEmployeeActivityReport(ReportFiltersModel model, User userInfo)
        {
            try
            {

                var datas = new List<EmployeeActivityDetail>();
                if (model.SalesPersonFilter.Count <= 0)
                    return datas;
                var query = $@"SELECT * FROM (select L.submit_date SUBMITDATE,U.FULL_NAME SALESPERSON,'-' CODE,'-' NAME,'-' REMARKS,'ATN' VISITTYPE,'-' TOTALAMOUNT from dist_lm_location_tracking L,dist_login_user u  
where L.SP_CODE=U.SP_CODE and  L.sp_code in ('{ string.Join("','", model.SalesPersonFilter).ToString()}') AND TRACK_TYPE='ATN' AND L.COMPANY_CODE='{userInfo.company_code}'  AND  TRUNC(L.submit_date)  BETWEEN TO_DATE('{model.FromDate}','YYYY-MM-DD')  AND TO_DATE('{model.ToDate}','YYYY-MM-DD')
UNION ALL
SELECT L.UPDATE_DATE SUBMITDATE ,U.FULL_NAME SALESPERSON,R.RESELLER_CODE CODE,R.RESELLER_NAME NAME,R.REMARKS ,'Reseller Visit' VISITTYPE,'-' TOTALAMOUNT from DIST_LOCATION_TRACK L ,DIST_LOGIN_USER U,DIST_RESELLER_MASTER R WHERE L.SP_CODE=U.SP_CODE AND L.CUSTOMER_CODE=R.RESELLER_CODE
 AND L.SP_CODE in ('{ string.Join("','", model.SalesPersonFilter).ToString()}') AND L.CUSTOMER_TYPE='R' AND TRUNC( L.UPDATE_DATE) BETWEEN  TO_DATE('{model.FromDate}','YYYY-MM-DD')  AND TO_DATE('{model.ToDate}','YYYY-MM-DD')
 union all
SELECT L.UPDATE_DATE SUBMITDATE,U.FULL_NAME SALESPERSON,R.CUSTOMER_CODE CODE,R.CUSTOMER_EDESC NAME,R.REMARKS ,'Distribution Visit' VISITTYPE,'-' TOTALAMOUNT from DIST_LOCATION_TRACK L ,DIST_LOGIN_USER U,SA_CUSTOMER_SETUP R WHERE L.SP_CODE=U.SP_CODE AND L.CUSTOMER_CODE=R.CUSTOMER_CODE
AND L.COMPANY_CODE=R.COMPANY_CODE
 AND L.SP_CODE in ('{ string.Join("','", model.SalesPersonFilter).ToString()}') AND L.CUSTOMER_TYPE='D' AND TRUNC( L.UPDATE_DATE) BETWEEN  TO_DATE('{model.FromDate}','YYYY-MM-DD')  AND TO_DATE('{model.ToDate}','YYYY-MM-DD')
UNION ALL 
SELECT R.ORDER_DATE SUBMITDATE,U.FULL_NAME SALESPERSON,R.RESELLER_CODE CODE,RM.RESELLER_NAME NAME,R.REMARKS,'RESELLER ORDER' VISITTYPE,TO_CHAR(SUM(TOTAL_PRICE)) TOTALAMOUNT FROM DIST_IP_SSR_PURCHASE_ORDER R,DIST_LOGIN_USER U,DIST_RESELLER_MASTER RM
 WHERE R.CREATED_BY=U.USERID AND RM.RESELLER_CODE=R.RESELLER_CODE  
AND U.SP_CODE in ('{ string.Join("','", model.SalesPersonFilter).ToString()}') AND TRUNC( R.ORDER_DATE) BETWEEN  TO_DATE('{model.FromDate}','YYYY-MM-DD')  AND TO_DATE('{model.ToDate}','YYYY-MM-DD') GROUP BY  U.SP_CODE,R.ORDER_DATE,U.FULL_NAME,R.RESELLER_CODE ,U.FULL_NAME,RM.RESELLER_NAME,R.REMARKS
union all 
SELECT R.ORDER_DATE SUBMITDATE,U.FULL_NAME SALESPERSON ,R.CUSTOMER_CODE CODE,RM.CUSTOMER_EDESC NAME,R.REMARKS,'DISTRIBUTION ORDER' VISITTYPE, TO_CHAR(SUM(TOTAL_PRICE)) TOTALAMOUNT FROM DIST_IP_SSD_PURCHASE_ORDER R,DIST_LOGIN_USER U,SA_CUSTOMER_SETUP RM
 WHERE R.CREATED_BY=U.USERID AND RM.CUSTOMER_CODE=R.CUSTOMER_CODE AND R.COMPANY_CODE=RM.COMPANY_CODE 
  AND U.SP_CODE in ('{ string.Join("','", model.SalesPersonFilter).ToString()}') AND TRUNC( R.ORDER_DATE) BETWEEN  TO_DATE('{model.FromDate}','YYYY-MM-DD')  AND TO_DATE('{model.ToDate}','YYYY-MM-DD')
 GROUP BY U.SP_CODE,R.ORDER_DATE,R.CUSTOMER_CODE ,U.FULL_NAME,RM.CUSTOMER_EDESC,R.REMARKS
 UNION ALL
 SELECT R.CREATED_DATE SUBMITDATE,R.CREATED_BY SALESPERSON ,R.ENTITY_CODE CODE,RM.RESELLER_NAME NAME,R.REMARKS,'RESELLER COLLECTION' VISITTYPE,TO_CHAR(SUM(AMOUNT)) TOTALAMOUNT FROM DIST_COLLECTION R,DIST_RESELLER_MASTER RM
 WHERE RM.RESELLER_CODE=R.ENTITY_CODE  AND R.ENTITY_TYPE='R' 
   AND UPPER(R.CREATED_BY) IN (SELECT UPPER(FULL_NAME) FROM DIST_LOGIN_USER WHERE SP_CODE in ('{ string.Join("','", model.SalesPersonFilter).ToString()}')) AND TRUNC( R.CREATED_DATE) BETWEEN  TO_DATE('{model.FromDate}','YYYY-MM-DD')  AND TO_DATE('{model.ToDate}','YYYY-MM-DD')
 GROUP BY R.BILL_NO,R.CREATED_DATE,R.ENTITY_CODE ,R.CREATED_BY,RM.RESELLER_NAME,R.REMARKS
 UNION ALL
 SELECT R.CREATED_DATE SUBMITDATE,R.CREATED_BY SALESPERSON ,R.ENTITY_CODE CODE,RM.CUSTOMER_EDESC NAME,R.REMARKS,'DISTRIBUTOR COLLECTION' VISITTYPE,TO_CHAR(SUM(AMOUNT)) TOTALAMOUNT FROM DIST_COLLECTION R,SA_CUSTOMER_SETUP RM
 WHERE RM.CUSTOMER_CODE=R.ENTITY_CODE  AND RM.COMPANY_CODE=R.COMPANY_CODE AND R.COMPANY_CODE='{userInfo.company_code}'
   AND R.ENTITY_TYPE='D'
   AND UPPER(R.CREATED_BY) IN (SELECT UPPER(FULL_NAME) FROM DIST_LOGIN_USER WHERE SP_CODE in ('{ string.Join("','", model.SalesPersonFilter).ToString()}')) AND TRUNC( R.CREATED_DATE) BETWEEN  TO_DATE('{model.FromDate}','YYYY-MM-DD')  AND TO_DATE('{model.ToDate}','YYYY-MM-DD')
   GROUP BY R.BILL_NO,R.CREATED_DATE,R.ENTITY_CODE ,R.CREATED_BY,RM.CUSTOMER_EDESC,R.REMARKS
   UNION ALL
    SELECT R.CREATED_DATE,U.FULL_NAME,R.RESELLER_CODE,R.RESELLER_NAME,R.RESELLER_CONTACT,'NEW OUTLET'  VISITTYPE,'-'  TOTALAMOUNT FROM DIST_RESELLER_MASTER  R,DIST_LOGIN_USER U WHERE
   R.CREATED_BY =U.USERID   
   AND U.SP_CODE in ('{ string.Join("','", model.SalesPersonFilter).ToString()}') AND TRUNC( R.CREATED_DATE) BETWEEN  TO_DATE('{model.FromDate}','YYYY-MM-DD')  AND TO_DATE('{model.ToDate}','YYYY-MM-DD')
   UNION ALL
   select L.submit_date SUBMITDATE,U.FULL_NAME SALESPERSON,'-' CODE,'-' NAME,'-' REMARKS,'EOD' VISITTYPE,'-' TOTALAMOUNT from dist_lm_location_tracking L,dist_login_user u  
where L.SP_CODE=U.SP_CODE and  L.sp_code in ('{ string.Join("','", model.SalesPersonFilter).ToString()}') AND TRACK_TYPE='EOD' AND L.COMPANY_CODE='{userInfo.company_code}'  AND  TRUNC(L.submit_date)  BETWEEN TO_DATE('{model.FromDate}','YYYY-MM-DD')  AND TO_DATE('{model.ToDate}','YYYY-MM-DD')
 ) ORDER BY SALESPERSON,SUBMITDATE";
                datas = _objectEntity.SqlQuery<EmployeeActivityDetail>(query).ToList();
                return datas;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<VisitTimeModel> GetVisitTimeSummary(ReportFiltersModel model, User userInfo)
        {
            var query = $@"SELECT TBL.*,BS_DATE(TO_CHAR(TBL.UPDATE_DATE)) MITI, --reseller
                            (SELECT LATITUDE||','||LONGITUDE FROM DIST_LOCATION_TRACK WHERE UPDATE_DATE = TBL.UPDATE_DATE AND SP_CODE = TBL.SP_CODE
                                         AND CUSTOMER_CODE = TBL.CUSTOMER_CODE AND CUSTOMER_TYPE = 'R' AND ROWNUM=1) LAT_LON
                FROM (SELECT DLT.SP_CODE,MIN(DLT.UPDATE_DATE) UPDATE_DATE,TO_CHAR(MIN(DLT.UPDATE_DATE),'HH:MI:SS A.M.') UPDATE_TIME,'Reseller' CUSTOMER_TYPE,
                            DLT.CUSTOMER_CODE,DRM.RESELLER_NAME CUSTOMER_NAME,DRM.REG_OFFICE_ADDRESS ADDRESS,DLU.FULL_NAME,MAX(DLT.REMARKS) REMARKS
                FROM DIST_LOCATION_TRACK DLT
                INNER JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DLT.CUSTOMER_CODE AND DLT.COMPANY_CODE = DRM.COMPANY_CODE AND DLT.CUSTOMER_TYPE = 'R'
                INNER JOIN DIST_LOGIN_USER DLU ON DLT.SP_CODE = DLU.SP_CODE AND DLT.COMPANY_CODE = DLU.COMPANY_CODE AND DRM.IS_CLOSED = 'N' AND DLU.ACTIVE = 'Y'
                WHERE TRUNC(DLT.UPDATE_DATE) = TO_DATE('{model.FromDate}','YYYY-MM-DD') AND DLT.COMPANY_CODE = '{userInfo.company_code}'
                GROUP BY DLT.SP_CODE,DLT.CUSTOMER_CODE,DRM.RESELLER_NAME,DRM.REG_OFFICE_ADDRESS,DLU.FULL_NAME,DLT.COMPANY_CODE,CUSTOMER_TYPE) TBL
                UNION
                SELECT TBL.*,BS_DATE(TO_CHAR(TBL.UPDATE_DATE)) MITI, --distributor
                            (SELECT LATITUDE||','||LONGITUDE FROM DIST_LOCATION_TRACK WHERE UPDATE_DATE = TBL.UPDATE_DATE AND SP_CODE = TBL.SP_CODE
                                         AND CUSTOMER_CODE = TBL.CUSTOMER_CODE AND CUSTOMER_TYPE = 'D' AND ROWNUM=1) LAT_LON
                FROM (SELECT DLT.SP_CODE,MIN(DLT.UPDATE_DATE) UPDATE_DATE,TO_CHAR(MIN(DLT.UPDATE_DATE),'HH:MI:SS A.M.') UPDATE_TIME,'Distributor' CUSTOMER_TYPE,
                            DLT.CUSTOMER_CODE,SCS.CUSTOMER_EDESC CUSTOMER_NAME,SCS.REGD_OFFICE_EADDRESS ADDRESS,DLU.FULL_NAME,MAX(DLT.REMARKS) REMARKS
                FROM DIST_LOCATION_TRACK DLT
                INNER JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DLT.CUSTOMER_CODE AND DLT.COMPANY_CODE = SCS.COMPANY_CODE AND DLT.CUSTOMER_TYPE = 'D'
                INNER JOIN DIST_LOGIN_USER DLU ON DLT.SP_CODE = DLU.SP_CODE AND DLT.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                WHERE TRUNC(DLT.UPDATE_DATE) = TO_DATE('{model.FromDate}','YYYY-MM-DD') AND DLT.COMPANY_CODE = '{userInfo.company_code}'
                GROUP BY DLT.SP_CODE,DLT.CUSTOMER_CODE,SCS.CUSTOMER_EDESC,SCS.REGD_OFFICE_EADDRESS,DLU.FULL_NAME,CUSTOMER_TYPE) TBL
                UNION
                SELECT TBL.*,BS_DATE(TO_CHAR(TBL.UPDATE_DATE)) MITI, --dealer
                            (SELECT LATITUDE||','||LONGITUDE FROM DIST_LOCATION_TRACK WHERE UPDATE_DATE = TBL.UPDATE_DATE AND SP_CODE = TBL.SP_CODE
                                         AND CUSTOMER_CODE = TBL.CUSTOMER_CODE AND CUSTOMER_TYPE = 'P' AND ROWNUM=1) LAT_LON
                FROM (SELECT DLT.SP_CODE,MIN(DLT.UPDATE_DATE) UPDATE_DATE,TO_CHAR(MIN(DLT.UPDATE_DATE),'HH:MI:SS A.M.') UPDATE_TIME,'Dealer' CUSTOMER_TYPE,
                            DLT.CUSTOMER_CODE,IPC.PARTY_TYPE_EDESC CUSTOMER_NAME,DDM.REG_OFFICE_ADDRESS ADDRESS,DLU.FULL_NAME,MAX(DLT.REMARKS) REMARKS
                FROM DIST_LOCATION_TRACK DLT
                INNER JOIN IP_PARTY_TYPE_CODE IPC ON IPC.PARTY_TYPE_CODE = DLT.CUSTOMER_CODE AND DLT.COMPANY_CODE = IPC.COMPANY_CODE AND DLT.CUSTOMER_TYPE = 'P'
                INNER JOIN DIST_DEALER_MASTER DDM ON DDM.DEALER_CODE =  IPC.PARTY_TYPE_CODE AND DDM.COMPANY_CODE = IPC.COMPANY_CODE
                INNER JOIN DIST_LOGIN_USER DLU ON DLT.SP_CODE = DLU.SP_CODE AND DLT.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                WHERE TRUNC(DLT.UPDATE_DATE) = TO_DATE('{model.FromDate}','YYYY-MM-DD') AND DLT.COMPANY_CODE = '{userInfo.company_code}'
                GROUP BY DLT.SP_CODE,DLT.CUSTOMER_CODE,IPC.PARTY_TYPE_EDESC,DDM.REG_OFFICE_ADDRESS,DLU.FULL_NAME,CUSTOMER_TYPE) TBL
                UNION
                SELECT DEA.SP_CODE,DEA.VISIT_DATE UPDATE_DATE,TO_CHAR(DEA.VISIT_DATE,'HH:MI:SS A.M.') UPDATE_TIME,'Extra' CUSTOMER_TYPE,'' CUSTOMER_CODE, '' CUSTOMER_NAME,
                            '' ADDRESS,DLU.FULL_NAME,DEA.REMARKS,BS_DATE(TO_CHAR(DEA.VISIT_DATE)) MITI,DEA.LATITUDE||','||DEA.LONGITUDE LAT_LON
                FROM DIST_EXTRA_ACTIVITY DEA
                INNER JOIN DIST_LOGIN_USER DLU ON DEA.SP_CODE = DLU.SP_CODE AND DEA.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                WHERE TRUNC(DEA.VISIT_DATE) = TO_DATE('{model.FromDate}','YYYY-MM-DD') AND DEA.COMPANY_CODE = '{userInfo.company_code}'";
            var data = _objectEntity.SqlQuery<VisitTimeModel>(query).ToList();
            for (int i = 0; i < data.Count; i++)
            {
                var detailQuery = $@"SELECT SP_CODE,SUBMIT_DATE UPDATE_DATE,LATITUDE||','||LONGITUDE LAT_LON FROM DIST_LM_LOCATION_TRACKING
                WHERE SP_CODE = '{data[i].SP_CODE}' AND SUBMIT_DATE > TO_DATE('{data[i].UPDATE_DATE.ToString().Replace("M", ".M.")}','MM/DD/YYYY HH:MI:SS A.M.')
                AND TRUNC(SUBMIT_DATE) =  TRUNC(TO_DATE('{data[i].UPDATE_DATE.ToString().Replace("M", ".M.")}','MM/DD/YYYY HH:MI:SS A.M.'))";
                var detailData = _objectEntity.SqlQuery<VisitTimeModel>(detailQuery);
                foreach (var item in detailData)
                {
                    var lat1 = double.Parse(data[i].LAT_LON.Split(',')[0]);
                    var lon1 = double.Parse(data[i].LAT_LON.Split(',')[1]);
                    var lat2 = double.Parse(item.LAT_LON.Split(',')[0]);
                    var lon2 = double.Parse(item.LAT_LON.Split(',')[1]);
                    var distance = CalcDistance(lat1, lon1, lat2, lon2);
                    if (distance > 0.05)
                    {
                        data[i].SPENT_TIME = item.UPDATE_DATE - data[i].UPDATE_DATE;
                        break;
                    }
                }
            }
            return data;
        }

        public List<VisitTimeModel> GetLastLocations(User userInfo)
        {
            var spcode = string.Empty;
            if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                spcode = $" AND  DET.SP_CODE IN  ({userInfo.sp_codes})";
            }

            var query = $@"SELECT
            DLU.FULL_NAME,
            DET.SP_CODE,UPDATE_DATE,TO_CHAR(UPDATE_DATE,'HH:MI:SS AM') UPDATE_TIME,LATITUDE||','||LONGITUDE LAT_LON
            FROM(
              SELECT SP_CODE,UPDATE_DATE,LATITUDE,LONGITUDE,
                ROW_NUMBER() OVER (PARTITION BY SP_CODE 
                                   ORDER BY UPDATE_DATE DESC) AS RN
                FROM(SELECT DISTINCT MAXS.SP_CODE,MAXS.UPDATE_DATE,DLT.LATITUDE,DLT.LONGITUDE
                     FROM DIST_LOCATION_TRACK DLT INNER JOIN
                    (SELECT SP_CODE,MAX(UPDATE_DATE) UPDATE_DATE FROM DIST_LOCATION_TRACK GROUP BY SP_CODE) MAXS
                            ON DLT.SP_CODE = MAXS.SP_CODE AND DLT.UPDATE_DATE = MAXS.UPDATE_DATE
                    UNION
                    SELECT DISTINCT MAXS.SP_CODE,MAXS.UPDATE_DATE,DLLT.LATITUDE,DLLT.LONGITUDE
                     FROM DIST_LM_LOCATION_TRACKING DLLT INNER JOIN
                    (SELECT SP_CODE,MAX(SUBMIT_DATE) UPDATE_DATE FROM DIST_LM_LOCATION_TRACKING GROUP BY SP_CODE) MAXS
                            ON DLLT.SP_CODE = MAXS.SP_CODE AND DLLT.SUBMIT_DATE = MAXS.UPDATE_DATE
                    UNION
                    SELECT DISTINCT MAXS.SP_CODE,MAXS.UPDATE_DATE,DEA.LATITUDE,DEA.LONGITUDE
                     FROM DIST_EXTRA_ACTIVITY DEA INNER JOIN
                    (SELECT SP_CODE,MAX(VISIT_DATE) UPDATE_DATE FROM DIST_EXTRA_ACTIVITY GROUP BY SP_CODE) MAXS
                            ON DEA.SP_CODE = MAXS.SP_CODE AND DEA.VISIT_DATE = MAXS.UPDATE_DATE)
            ) DET
            INNER JOIN DIST_LOGIN_USER DLU ON DLU.SP_CODE = DET.SP_CODE
            WHERE DET.RN = 1
            AND DLU.ACTIVE = 'Y' {spcode}
            AND DLU.COMPANY_CODE = '{userInfo.company_code}'";
            var data = _objectEntity.SqlQuery<VisitTimeModel>(query).ToList();
            return data;
        }

        public List<QuestionaireCustomerModel> GetQuestionaireReport(ReportFiltersModel model, User userInfo, string surveyCode)
        {
            var companyCode = string.Join(",", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            var employeeFilter = string.Empty;
            if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                employeeFilter = $" AND  ENTITIES.SP_CODE IN  ({userInfo.sp_codes})";
            }

            var Query = $@"SELECT ENTITIES.*,DAM.AREA_NAME,DLU.FULL_NAME FROM (
                        SELECT ENT.SP_CODE,ENT.ENTITY_CODE,ENT.ENTITY_TYPE,DDM.AREA_CODE,CS.CUSTOMER_EDESC ENTITY_NAME,CS.REGD_OFFICE_EADDRESS ADDRESS,CS.TEL_MOBILE_NO1 CONTACT_NO
                            FROM SA_CUSTOMER_SETUP CS
                            INNER JOIN DIST_DISTRIBUTOR_MASTER DDM ON CS.CUSTOMER_CODE = DDM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DDM.COMPANY_CODE AND DDM.ACTIVE = 'Y'
                            INNER JOIN (SELECT SP_CODE,ENTITY_CODE,ENTITY_TYPE FROM DIST_QA_ANSWER WHERE ENTITY_TYPE = 'D' AND COMPANY_CODE IN ('{companyCode}')
                            UNION
                            SELECT SP_CODE,ENTITY_CODE,ENTITY_TYPE FROM DIST_QA_TAB_CELL_ANSWER  WHERE ENTITY_TYPE = 'D') ENT
                                    ON CS.CUSTOMER_CODE = ENT.ENTITY_CODE
                                    WHERE CS.COMPANY_CODE IN ('{companyCode}')
                            UNION
                            SELECT ENT.SP_CODE,ENT.ENTITY_CODE,ENT.ENTITY_TYPE,RES.AREA_CODE,RES.RESELLER_NAME,RES.REG_OFFICE_ADDRESS,RES.RESELLER_CONTACT
                            FROM DIST_RESELLER_MASTER RES INNER JOIN
                            (SELECT SP_CODE,ENTITY_CODE,ENTITY_TYPE FROM DIST_QA_ANSWER WHERE ENTITY_TYPE = 'R' AND COMPANY_CODE IN ('{companyCode}')
                            UNION
                            SELECT SP_CODE,ENTITY_CODE,ENTITY_TYPE FROM DIST_QA_TAB_CELL_ANSWER  WHERE ENTITY_TYPE = 'R') ENT
                                    ON RES.RESELLER_CODE = ENT.ENTITY_CODE
                                    WHERE RES.COMPANY_CODE IN ('{companyCode}')
                            UNION
                            SELECT ENT.SP_CODE,ENT.ENTITY_CODE,ENT.ENTITY_TYPE,DPM.AREA_CODE,IPC.PARTY_TYPE_EDESC,DPM.REG_OFFICE_ADDRESS,DPM.CONTACT_NO
                            FROM IP_PARTY_TYPE_CODE IPC
                            INNER JOIN DIST_DEALER_MASTER DPM ON DPM.DEALER_CODE = IPC.PARTY_TYPE_CODE AND DPM.COMPANY_CODE = IPC.COMPANY_CODE
                            INNER JOIN (SELECT SP_CODE,ENTITY_CODE,ENTITY_TYPE FROM DIST_QA_ANSWER WHERE ENTITY_TYPE = 'P' AND COMPANY_CODE IN ('{companyCode}')
                            UNION
                            SELECT SP_CODE,ENTITY_CODE,ENTITY_TYPE FROM DIST_QA_TAB_CELL_ANSWER  WHERE ENTITY_TYPE = 'P') ENT
                                    ON DPM.DEALER_CODE = ENT.ENTITY_CODE
                                    WHERE DPM.COMPANY_CODE IN ('{companyCode}')) ENTITIES
                            INNER JOIN DIST_AREA_MASTER DAM ON ENTITIES.AREA_CODE = DAM.AREA_CODE
                            INNER JOIN DIST_LOGIN_USER DLU ON ENTITIES.SP_CODE = DLU.SP_CODE
                            WHERE 1=1 {employeeFilter} ";
            if (!string.IsNullOrWhiteSpace(surveyCode))
                Query += $@" AND ENTITIES.AREA_CODE IN (SELECT DISTINCT REGEXP_SUBSTR(AREA_CODES,'[^,]+', 1, LEVEL) FROM DIST_QA_SET_SALESPERSON_MAP WHERE SURVEY_CODE IN ({surveyCode})
                                        CONNECT BY REGEXP_SUBSTR(AREA_CODES, '[^,]+', 1, LEVEL) IS NOT NULL)";

            var data = _objectEntity.SqlQuery<QuestionaireCustomerModel>(Query).ToList();
            return data;
        }

        public bool UpdatePO(PurchaseOrderModel model, string userId)
        {
            if (model.ORDER_NO > 0)
            {
                var orderDetails = GetPurchaseOrderDetail(model.COMPANY_CODE, model.ORDER_NO.ToString(), model.ORDER_ENTITY, "ALL");
                var PreviousItemCodes = orderDetails.Select(x => x.ITEM_CODE).ToList();
                var oldOrders = model.ITEMS.Where(x => PreviousItemCodes.Contains(x.ITEM_CODE));
                var order = orderDetails.FirstOrDefault();
                var newOrders = model.ITEMS.Where(x => !PreviousItemCodes.Contains(x.ITEM_CODE));
                using (var trans = _objectEntity.Database.BeginTransaction())
                {
                    try
                    {
                        var today = $"TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}','MM/dd/yyyy hh24:mi:ss')";
                        var Orderdate = $"TO_DATE('{order.ORDER_DATE.Value.ToString("MM/dd/yyyy HH:mm:ss")}','MM/dd/yyyy hh24:mi:ss')";
                        var Createddate = $"TO_DATE('{order.CREATED_DATE.Value.ToString("MM/dd/yyyy HH:mm:ss")}','MM/dd/yyyy hh24:mi:ss')";
                        foreach (var item in oldOrders)
                        {
                            item.PARTY_TYPE_CODE = item.PARTY_TYPE_CODE == null ? "" : item.PARTY_TYPE_CODE;
                            string UpdateQuery = string.Empty;
                            string priceQuery = $"SELECT NVL(SALES_PRICE,0) SALES_PRICE FROM IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '{item.ITEM_CODE}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
                            decimal SP = _objectEntity.SqlQuery<decimal>(priceQuery).FirstOrDefault();
                            item.UNIT_PRICE = item.UNIT_PRICE == 0 ? SP : item.UNIT_PRICE;

                            var total = item.UNIT_PRICE * item.QUANTITY;

                            if (model.ORDER_ENTITY.Equals("P", StringComparison.OrdinalIgnoreCase) || model.ORDER_ENTITY.Equals("DEALER", StringComparison.OrdinalIgnoreCase)
                                || model.ORDER_ENTITY.Equals("D", StringComparison.OrdinalIgnoreCase) || model.ORDER_ENTITY.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase)
                                || model.ORDER_ENTITY.Equals("P-D", StringComparison.OrdinalIgnoreCase))
                                UpdateQuery = $@"UPDATE DIST_IP_SSD_PURCHASE_ORDER SET QUANTITY='{item.QUANTITY}',UNIT_PRICE='{item.UNIT_PRICE}',TOTAL_PRICE='{total}',REMARKS='{model.REMARKS}',MODIFY_BY='{userId}',MODIFY_DATE={today}
                                                WHERE ORDER_NO='{model.ORDER_NO}'
                                                AND COMPANY_CODE='{model.COMPANY_CODE}'
                                                AND BRANCH_CODE='{model.BRANCH_CODE}'
                                                AND CREATED_BY='{model.CREATED_BY}'
                                                AND ITEM_CODE='{item.ITEM_CODE}'";
                            //else
                            //    InsertQuery = $@"INSERT INTO DIST_IP_SSR_PURCHASE_ORDER (ORDER_NO,ORDER_DATE,RESELLER_CODE,CUSTOMER_CODE,ITEM_CODE,MU_CODE,QUANTITY,BILLING_NAME,REMARKS,UNIT_PRICE,TOTAL_PRICE,CREATED_BY,CREATED_DATE,APPROVED_FLAG,DISPATCH_FLAG,ACKNOWLEDGE_FLAG,REJECT_FLAG,DELETED_FLAG,PARTY_TYPE_CODE,CITY_CODE,SALES_TYPE_CODE,SHIPPING_CONTACT,COMPANY_CODE,BRANCH_CODE)
                            //VALUES('{model.ORDER_NO}',{order.ORDER_DATE},'{model.CUSTOMER_CODE}','{model.}','{item.item_code}','{item.MU_CODE}','{item.QUANTITY}','{order.BILLING_NAME}','{order.REMARKS}','{item.UNIT_PRICE}','{total}','{order.CREATED_BY}',{order.CREATED_DATE},'N','N','N','N','N','{item.PARTY_TYPE_CODE}','{order.po}','{item.Po_Sales_Type}','{item.Po_Shipping_Contact}','{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
                            int rowNum = _objectEntity.ExecuteSqlCommand(UpdateQuery);
                        }
                        foreach (var item in newOrders)
                        {
                            item.PARTY_TYPE_CODE = item.PARTY_TYPE_CODE == null ? "" : item.PARTY_TYPE_CODE;
                            string InsertQuery = string.Empty;
                            string priceQuery = $"SELECT NVL(SALES_PRICE,0) SALES_PRICE FROM IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '{item.ITEM_CODE}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
                            decimal SP = _objectEntity.SqlQuery<decimal>(priceQuery).FirstOrDefault();
                            item.UNIT_PRICE = item.UNIT_PRICE == 0 ? SP : item.UNIT_PRICE;

                            var total = item.UNIT_PRICE * item.QUANTITY;

                            if (model.ORDER_ENTITY.Equals("P", StringComparison.OrdinalIgnoreCase) || model.ORDER_ENTITY.Equals("DEALER", StringComparison.OrdinalIgnoreCase)
                                || model.ORDER_ENTITY.Equals("D", StringComparison.OrdinalIgnoreCase) || model.ORDER_ENTITY.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase)
                                || model.ORDER_ENTITY.Equals("P-D", StringComparison.OrdinalIgnoreCase))
                                InsertQuery = $@"INSERT INTO DIST_IP_SSD_PURCHASE_ORDER (ORDER_NO,ORDER_DATE,CUSTOMER_CODE,ITEM_CODE,MU_CODE,QUANTITY,BILLING_NAME,REMARKS,UNIT_PRICE,TOTAL_PRICE,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,APPROVED_FLAG,DISPATCH_FLAG,ACKNOWLEDGE_FLAG,REJECT_FLAG,DELETED_FLAG,PARTY_TYPE_CODE,COMPANY_CODE,BRANCH_CODE)
                            VALUES('{model.ORDER_NO}',{Orderdate},'{model.CUSTOMER_CODE}','{item.ITEM_CODE}','{item.MU_CODE}','{item.QUANTITY}','{model.CUSTOMER_EDESC}','{model.REMARKS}','{item.UNIT_PRICE}','{total}','{model.CREATED_BY}',{Createddate},'{userId}',{today},'N','N','N','N','N','{item.PARTY_TYPE_CODE}','{model.COMPANY_CODE}','{model.BRANCH_CODE}')"; //,CITY_CODE,SALES_TYPE_CODE,SHIPPING_CONTACT
                                                                                                                                                                                                                                                                                                                                                                            //else
                                                                                                                                                                                                                                                                                                                                                                            //    InsertQuery = $@"INSERT INTO DIST_IP_SSR_PURCHASE_ORDER (ORDER_NO,ORDER_DATE,RESELLER_CODE,CUSTOMER_CODE,ITEM_CODE,MU_CODE,QUANTITY,BILLING_NAME,REMARKS,UNIT_PRICE,TOTAL_PRICE,CREATED_BY,CREATED_DATE,APPROVED_FLAG,DISPATCH_FLAG,ACKNOWLEDGE_FLAG,REJECT_FLAG,DELETED_FLAG,PARTY_TYPE_CODE,CITY_CODE,SALES_TYPE_CODE,SHIPPING_CONTACT,COMPANY_CODE,BRANCH_CODE)
                                                                                                                                                                                                                                                                                                                                                                            //VALUES('{model.ORDER_NO}',{order.ORDER_DATE},'{model.CUSTOMER_CODE}','{model.}','{item.item_code}','{item.MU_CODE}','{item.QUANTITY}','{order.BILLING_NAME}','{order.REMARKS}','{item.UNIT_PRICE}','{total}','{order.CREATED_BY}',{order.CREATED_DATE},'N','N','N','N','N','{item.PARTY_TYPE_CODE}','{order.po}','{item.Po_Sales_Type}','{item.Po_Shipping_Contact}','{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
                            int rowNum = _objectEntity.ExecuteSqlCommand(InsertQuery);
                        }
                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                    }
                }
            }
            return false;
        }
        public bool UpdateDSO(PurchaseOrderModel model, string userId)
        {
            if (model.ORDER_NO > 0)
            {
                //var orderDetails = GetPurchaseOrderDetail(model.COMPANY_CODE, model.ORDER_NO.ToString(), model.ORDER_ENTITY, "ALL");
                //var PreviousItemCodes = orderDetails.Select(x => x.ITEM_CODE).ToList();
                //var oldOrders = model.ITEMS.Where(x => PreviousItemCodes.Contains(x.ITEM_CODE));
                //var order = orderDetails.FirstOrDefault();
                //var newOrders = model.ITEMS.Where(x => !PreviousItemCodes.Contains(x.ITEM_CODE));
                using (var trans = _objectEntity.Database.BeginTransaction())
                {
                    try
                    {
                        var today = $"TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}','MM/dd/yyyy hh24:mi:ss')";
                        //var Orderdate = $"TO_DATE('{order.ORDER_DATE.Value.ToString("MM/dd/yyyy HH:mm:ss")}','MM/dd/yyyy hh24:mi:ss')";
                        //var Createddate = $"TO_DATE('{order.CREATED_DATE.Value.ToString("MM/dd/yyyy HH:mm:ss")}','MM/dd/yyyy hh24:mi:ss')";
                        //foreach (var item in oldOrders)
                        //{
                        //    item.PARTY_TYPE_CODE = item.PARTY_TYPE_CODE == null ? "" : item.PARTY_TYPE_CODE;
                        string UpdateQuery = string.Empty;
                        //    string priceQuery = $"SELECT NVL(SALES_PRICE,0) SALES_PRICE FROM IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '{item.ITEM_CODE}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
                        //    decimal SP = _objectEntity.SqlQuery<decimal>(priceQuery).FirstOrDefault();
                        //    item.UNIT_PRICE = item.UNIT_PRICE == 0 ? SP : item.UNIT_PRICE;

                        //    var total = item.UNIT_PRICE * item.QUANTITY;

                        //    if (model.ORDER_ENTITY.Equals("P", StringComparison.OrdinalIgnoreCase) || model.ORDER_ENTITY.Equals("DEALER", StringComparison.OrdinalIgnoreCase)
                        //        || model.ORDER_ENTITY.Equals("D", StringComparison.OrdinalIgnoreCase) || model.ORDER_ENTITY.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase)
                        //        || model.ORDER_ENTITY.Equals("P-D", StringComparison.OrdinalIgnoreCase))
                        UpdateQuery = $@"UPDATE DIST_IP_SSD_PURCHASE_ORDER SET REMARKS='{model.REMARKS}',MODIFY_BY='{userId}',MODIFY_DATE={today}
                                                WHERE ORDER_NO='{model.ORDER_NO}'
                                                AND COMPANY_CODE='{model.COMPANY_CODE}'
                                                AND BRANCH_CODE='{model.BRANCH_CODE}'
                                                AND CREATED_BY='{model.CREATED_BY}'";
                        //AND ITEM_CODE='{item.ITEM_CODE}'";
                        //else
                        //    InsertQuery = $@"INSERT INTO DIST_IP_SSR_PURCHASE_ORDER (ORDER_NO,ORDER_DATE,RESELLER_CODE,CUSTOMER_CODE,ITEM_CODE,MU_CODE,QUANTITY,BILLING_NAME,REMARKS,UNIT_PRICE,TOTAL_PRICE,CREATED_BY,CREATED_DATE,APPROVED_FLAG,DISPATCH_FLAG,ACKNOWLEDGE_FLAG,REJECT_FLAG,DELETED_FLAG,PARTY_TYPE_CODE,CITY_CODE,SALES_TYPE_CODE,SHIPPING_CONTACT,COMPANY_CODE,BRANCH_CODE)
                        //VALUES('{model.ORDER_NO}',{order.ORDER_DATE},'{model.CUSTOMER_CODE}','{model.}','{item.item_code}','{item.MU_CODE}','{item.QUANTITY}','{order.BILLING_NAME}','{order.REMARKS}','{item.UNIT_PRICE}','{total}','{order.CREATED_BY}',{order.CREATED_DATE},'N','N','N','N','N','{item.PARTY_TYPE_CODE}','{order.po}','{item.Po_Sales_Type}','{item.Po_Shipping_Contact}','{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
                        int rowNum = _objectEntity.ExecuteSqlCommand(UpdateQuery);
                        //}
                        //foreach (var item in newOrders)
                        //{
                        //    item.PARTY_TYPE_CODE = item.PARTY_TYPE_CODE == null ? "" : item.PARTY_TYPE_CODE;
                        //    string InsertQuery = string.Empty;
                        //    string priceQuery = $"SELECT NVL(SALES_PRICE,0) SALES_PRICE FROM IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '{item.ITEM_CODE}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
                        //    decimal SP = _objectEntity.SqlQuery<decimal>(priceQuery).FirstOrDefault();
                        //    item.UNIT_PRICE = item.UNIT_PRICE == 0 ? SP : item.UNIT_PRICE;

                        //    var total = item.UNIT_PRICE * item.QUANTITY;

                        //    if (model.ORDER_ENTITY.Equals("P", StringComparison.OrdinalIgnoreCase) || model.ORDER_ENTITY.Equals("DEALER", StringComparison.OrdinalIgnoreCase)
                        //        || model.ORDER_ENTITY.Equals("D", StringComparison.OrdinalIgnoreCase) || model.ORDER_ENTITY.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase)
                        //        || model.ORDER_ENTITY.Equals("P-D", StringComparison.OrdinalIgnoreCase))
                        //        InsertQuery = $@"INSERT INTO DIST_IP_SSD_PURCHASE_ORDER (ORDER_NO,ORDER_DATE,CUSTOMER_CODE,ITEM_CODE,MU_CODE,QUANTITY,BILLING_NAME,REMARKS,UNIT_PRICE,TOTAL_PRICE,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,APPROVED_FLAG,DISPATCH_FLAG,ACKNOWLEDGE_FLAG,REJECT_FLAG,DELETED_FLAG,PARTY_TYPE_CODE,COMPANY_CODE,BRANCH_CODE)
                        //    VALUES('{model.ORDER_NO}',{Orderdate},'{model.CUSTOMER_CODE}','{item.ITEM_CODE}','{item.MU_CODE}','{item.QUANTITY}','{model.CUSTOMER_EDESC}','{model.REMARKS}','{item.UNIT_PRICE}','{total}','{model.CREATED_BY}',{Createddate},'{userId}',{today},'N','N','N','N','N','{item.PARTY_TYPE_CODE}','{model.COMPANY_CODE}','{model.BRANCH_CODE}')"; //,CITY_CODE,SALES_TYPE_CODE,SHIPPING_CONTACT
                        //    //else
                        //    //    InsertQuery = $@"INSERT INTO DIST_IP_SSR_PURCHASE_ORDER (ORDER_NO,ORDER_DATE,RESELLER_CODE,CUSTOMER_CODE,ITEM_CODE,MU_CODE,QUANTITY,BILLING_NAME,REMARKS,UNIT_PRICE,TOTAL_PRICE,CREATED_BY,CREATED_DATE,APPROVED_FLAG,DISPATCH_FLAG,ACKNOWLEDGE_FLAG,REJECT_FLAG,DELETED_FLAG,PARTY_TYPE_CODE,CITY_CODE,SALES_TYPE_CODE,SHIPPING_CONTACT,COMPANY_CODE,BRANCH_CODE)
                        //    //VALUES('{model.ORDER_NO}',{order.ORDER_DATE},'{model.CUSTOMER_CODE}','{model.}','{item.item_code}','{item.MU_CODE}','{item.QUANTITY}','{order.BILLING_NAME}','{order.REMARKS}','{item.UNIT_PRICE}','{total}','{order.CREATED_BY}',{order.CREATED_DATE},'N','N','N','N','N','{item.PARTY_TYPE_CODE}','{order.po}','{item.Po_Sales_Type}','{item.Po_Shipping_Contact}','{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
                        //    int rowNum = _objectEntity.ExecuteSqlCommand(InsertQuery);
                        //}
                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                    }
                }
            }
            return false;
        }

        public bool CancelDSO(PurchaseOrderModel model, string userId)
        {
            if (model.ORDER_NO > 0)
            {
                //var orderDetails = GetPurchaseOrderDetail(model.COMPANY_CODE, model.ORDER_NO.ToString(), model.ORDER_ENTITY, "ALL");
                //var PreviousItemCodes = orderDetails.Select(x => x.ITEM_CODE).ToList();
                //var oldOrders = model.ITEMS.Where(x => PreviousItemCodes.Contains(x.ITEM_CODE));
                //var order = orderDetails.FirstOrDefault();
                //var newOrders = model.ITEMS.Where(x => !PreviousItemCodes.Contains(x.ITEM_CODE));
                using (var trans = _objectEntity.Database.BeginTransaction())
                {
                    try
                    {
                        var today = $"TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}','MM/dd/yyyy hh24:mi:ss')";
                        //var Orderdate = $"TO_DATE('{order.ORDER_DATE.Value.ToString("MM/dd/yyyy HH:mm:ss")}','MM/dd/yyyy hh24:mi:ss')";
                        //var Createddate = $"TO_DATE('{order.CREATED_DATE.Value.ToString("MM/dd/yyyy HH:mm:ss")}','MM/dd/yyyy hh24:mi:ss')";
                        //foreach (var item in oldOrders)
                        //{
                        //    item.PARTY_TYPE_CODE = item.PARTY_TYPE_CODE == null ? "" : item.PARTY_TYPE_CODE;
                        string UpdateQuery = string.Empty;
                        //    string priceQuery = $"SELECT NVL(SALES_PRICE,0) SALES_PRICE FROM IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '{item.ITEM_CODE}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
                        //    decimal SP = _objectEntity.SqlQuery<decimal>(priceQuery).FirstOrDefault();
                        //    item.UNIT_PRICE = item.UNIT_PRICE == 0 ? SP : item.UNIT_PRICE;

                        //    var total = item.UNIT_PRICE * item.QUANTITY;

                        //    if (model.ORDER_ENTITY.Equals("P", StringComparison.OrdinalIgnoreCase) || model.ORDER_ENTITY.Equals("DEALER", StringComparison.OrdinalIgnoreCase)
                        //        || model.ORDER_ENTITY.Equals("D", StringComparison.OrdinalIgnoreCase) || model.ORDER_ENTITY.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase)
                        //        || model.ORDER_ENTITY.Equals("P-D", StringComparison.OrdinalIgnoreCase))
                        UpdateQuery = $@"UPDATE DIST_IP_SSD_PURCHASE_ORDER SET REMARKS='{model.REMARKS}',REJECT_FLAG='Y',MODIFY_BY='{userId}',MODIFY_DATE={today}
                                                WHERE ORDER_NO='{model.ORDER_NO}'
                                                AND COMPANY_CODE='{model.COMPANY_CODE}'
                                                AND BRANCH_CODE='{model.BRANCH_CODE}'
                                                AND CREATED_BY='{model.CREATED_BY}'";
                        // AND ITEM_CODE='{item.ITEM_CODE}'";
                        //else
                        //    InsertQuery = $@"INSERT INTO DIST_IP_SSR_PURCHASE_ORDER (ORDER_NO,ORDER_DATE,RESELLER_CODE,CUSTOMER_CODE,ITEM_CODE,MU_CODE,QUANTITY,BILLING_NAME,REMARKS,UNIT_PRICE,TOTAL_PRICE,CREATED_BY,CREATED_DATE,APPROVED_FLAG,DISPATCH_FLAG,ACKNOWLEDGE_FLAG,REJECT_FLAG,DELETED_FLAG,PARTY_TYPE_CODE,CITY_CODE,SALES_TYPE_CODE,SHIPPING_CONTACT,COMPANY_CODE,BRANCH_CODE)
                        //VALUES('{model.ORDER_NO}',{order.ORDER_DATE},'{model.CUSTOMER_CODE}','{model.}','{item.item_code}','{item.MU_CODE}','{item.QUANTITY}','{order.BILLING_NAME}','{order.REMARKS}','{item.UNIT_PRICE}','{total}','{order.CREATED_BY}',{order.CREATED_DATE},'N','N','N','N','N','{item.PARTY_TYPE_CODE}','{order.po}','{item.Po_Sales_Type}','{item.Po_Shipping_Contact}','{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
                        int rowNum = _objectEntity.ExecuteSqlCommand(UpdateQuery);
                        //}
                        //foreach (var item in newOrders)
                        //{
                        //    item.PARTY_TYPE_CODE = item.PARTY_TYPE_CODE == null ? "" : item.PARTY_TYPE_CODE;
                        //    string InsertQuery = string.Empty;
                        //    string priceQuery = $"SELECT NVL(SALES_PRICE,0) SALES_PRICE FROM IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '{item.ITEM_CODE}' AND COMPANY_CODE='{model.COMPANY_CODE}'";
                        //    decimal SP = _objectEntity.SqlQuery<decimal>(priceQuery).FirstOrDefault();
                        //    item.UNIT_PRICE = item.UNIT_PRICE == 0 ? SP : item.UNIT_PRICE;

                        //    var total = item.UNIT_PRICE * item.QUANTITY;

                        //    if (model.ORDER_ENTITY.Equals("P", StringComparison.OrdinalIgnoreCase) || model.ORDER_ENTITY.Equals("DEALER", StringComparison.OrdinalIgnoreCase)
                        //        || model.ORDER_ENTITY.Equals("D", StringComparison.OrdinalIgnoreCase) || model.ORDER_ENTITY.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase)
                        //        || model.ORDER_ENTITY.Equals("P-D", StringComparison.OrdinalIgnoreCase))
                        //        InsertQuery = $@"INSERT INTO DIST_IP_SSD_PURCHASE_ORDER (ORDER_NO,ORDER_DATE,CUSTOMER_CODE,ITEM_CODE,MU_CODE,QUANTITY,BILLING_NAME,REMARKS,UNIT_PRICE,TOTAL_PRICE,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,APPROVED_FLAG,DISPATCH_FLAG,ACKNOWLEDGE_FLAG,REJECT_FLAG,DELETED_FLAG,PARTY_TYPE_CODE,COMPANY_CODE,BRANCH_CODE)
                        //    VALUES('{model.ORDER_NO}',{Orderdate},'{model.CUSTOMER_CODE}','{item.ITEM_CODE}','{item.MU_CODE}','{item.QUANTITY}','{model.CUSTOMER_EDESC}','{model.REMARKS}','{item.UNIT_PRICE}','{total}','{model.CREATED_BY}',{Createddate},'{userId}',{today},'N','N','N','N','N','{item.PARTY_TYPE_CODE}','{model.COMPANY_CODE}','{model.BRANCH_CODE}')"; //,CITY_CODE,SALES_TYPE_CODE,SHIPPING_CONTACT
                        //                                                                                                                                                                                                                                                                                                                                                    //else
                        //                                                                                                                                                                                                                                                                                                                                                    //    InsertQuery = $@"INSERT INTO DIST_IP_SSR_PURCHASE_ORDER (ORDER_NO,ORDER_DATE,RESELLER_CODE,CUSTOMER_CODE,ITEM_CODE,MU_CODE,QUANTITY,BILLING_NAME,REMARKS,UNIT_PRICE,TOTAL_PRICE,CREATED_BY,CREATED_DATE,APPROVED_FLAG,DISPATCH_FLAG,ACKNOWLEDGE_FLAG,REJECT_FLAG,DELETED_FLAG,PARTY_TYPE_CODE,CITY_CODE,SALES_TYPE_CODE,SHIPPING_CONTACT,COMPANY_CODE,BRANCH_CODE)
                        //                                                                                                                                                                                                                                                                                                                                                    //VALUES('{model.ORDER_NO}',{order.ORDER_DATE},'{model.CUSTOMER_CODE}','{model.}','{item.item_code}','{item.MU_CODE}','{item.QUANTITY}','{order.BILLING_NAME}','{order.REMARKS}','{item.UNIT_PRICE}','{total}','{order.CREATED_BY}',{order.CREATED_DATE},'N','N','N','N','N','{item.PARTY_TYPE_CODE}','{order.po}','{item.Po_Sales_Type}','{item.Po_Shipping_Contact}','{model.COMPANY_CODE}','{model.BRANCH_CODE}')";
                        //    int rowNum = _objectEntity.ExecuteSqlCommand(InsertQuery);
                        //}
                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                    }
                }
            }
            return false;
        }

        public List<QuestionaireModel> GetQuestionaire(ReportFiltersModel model, string CustomerCode, string CustomerType, string surveyCode, User userInfo)
        {
            var companyCode = string.Join(",", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;
            var generalFilter = "";
            var tabularFilter = "";

            if (!string.IsNullOrWhiteSpace(surveyCode))
            {
                generalFilter = $" AND B.QA_CODE IN (SELECT QA_CODE FROM DIST_QA_MASTER WHERE SET_CODE IN (SELECT SET_ID FROM DIST_QA_SURVEY_MAP WHERE SET_TYPE = 'G' AND SURVEY_ID IN ({surveyCode})))";
                tabularFilter = $" WHERE AAA.CELL_ID IN (SELECT CELL_ID FROM DIST_QA_TAB_CELL WHERE TABLE_ID IN (SELECT SET_ID FROM DIST_QA_SURVEY_MAP WHERE SET_TYPE = 'T' AND SURVEY_ID IN ({surveyCode})))";
            }

            var Query = $@"SELECT B.QUESTION,A.ENTITY_CODE CUSTOMER_CODE,A.CREATED_DATE,A.ANSWER,
                                0 TABLE_ID,0 CELL_ID,0 CELL_NO,0 ROW_NO,''CELL_TYPE,'' CELL_LABEL,0 ANSWER_ID,'Simple' TYPE 
                                    FROM DIST_QA_ANSWER A
                                    JOIN DIST_QA_MASTER B ON (A.QA_CODE=B.QA_CODE)                
                                    WHERE
                                    A.COMPANY_CODE IN ({companyCode})
                                    AND A.ENTITY_TYPE='R' AND A.ENTITY_CODE='{CustomerCode}'
                                    {generalFilter}
                        UNION
                        SELECT AAA.TABLE_TITLE QUESTION,BBB.ENTITY_CODE CUSTOMER_CODE, AAA.CREATED_DATE,BBB.ANSWER,AAA.TABLE_ID,AAA.CELL_ID,AAA.CELL_NO,AAA.ROW_NO,AAA.CELL_TYPE,AAA.CELL_LABEL,
                                        BBB.ANSWER_ID, 'Tabular' TYPE
                                        FROM (SELECT AA.*,BB.* FROM (SELECT A.CREATED_DATE, B.TABLE_ID FROM DIST_QA_TAB_CELL_ANSWER A
                                        JOIN DIST_QA_TAB_CELL B ON (A.CELL_ID=B.CELL_ID)
                                        JOIN DIST_QA_TAB_TABLE C ON (B.TABLE_ID=C.TABLE_ID)
                                        WHERE C.COMPANY_CODE IN ({companyCode}) AND
                                         A.ENTITY_CODE='{CustomerCode}' AND A.ENTITY_TYPE='{CustomerType}' 
                                         GROUP BY A.CREATED_DATE,B.TABLE_ID) AA
                                        RIGHT JOIN (SELECT A.TABLE_ID AS T1, A.TABLE_TITLE, B.CELL_ID,B.CELL_NO, B.ROW_NO, B.CELL_TYPE, B.CELL_LABEL FROM DIST_QA_TAB_TABLE A
                                        JOIN DIST_QA_TAB_CELL B ON(A.TABLE_ID=B.TABLE_ID)) BB ON (AA.TABLE_ID=BB.T1)) AAA
                                        LEFT JOIN (SELECT M.*, N.TABLE_ID FROM DIST_QA_TAB_CELL_ANSWER M
                                        JOIN DIST_QA_TAB_CELL N ON (M.CELL_ID=N.CELL_ID)
                                        JOIN DIST_QA_TAB_TABLE O ON (N.TABLE_ID=O.TABLE_ID)
                                        WHERE O.COMPANY_CODE IN ({companyCode}) AND M.ENTITY_CODE='{CustomerCode}' AND M.ENTITY_TYPE='{CustomerType}') BBB 
                                        ON (AAA.TABLE_ID=BBB.TABLE_ID AND AAA.CREATED_DATE=BBB.CREATED_DATE AND AAA.CELL_ID=BBB.CELL_ID)
                                        {tabularFilter}";
            var tempData = _objectEntity.SqlQuery<QuestionaireModel>(Query).ToList();
            var questionList = tempData.Where(x => x.Type == "Tabular").Select(x => x.Question).Distinct().ToList();
            List<QuestionaireModel> finalDataList = tempData.Where(x => x.Type == "Simple").ToList();

            foreach (var question in questionList)
            {
                for (int i = 0; i <= tempData.Max(x => x.ROW_NO); i++)
                {
                    var tempStore = tempData.Where(x => x.Question == question && x.ROW_NO == i).ToList();
                    if (tempStore.Count() > 0)
                    {
                        QuestionaireModel finalData = new QuestionaireModel()
                        {
                            CUSTOMER_CODE = CustomerCode,
                            Question = (i == 0) ? tempStore.Select(x => x.Question).FirstOrDefault() : "",
                            Answer = tempStore.Where(x => x.CELL_NO == 0).Select(x => x.CELL_LABEL).FirstOrDefault(),
                            //Answer = tempStore.Where(x => x.CELL_TYPE == "LBL").Select(x => x.CELL_LABEL).FirstOrDefault(),
                            //Options = tempStore.Where(x => x.CELL_TYPE != "LBL").Select(x => x.Answer).FirstOrDefault() ?? "0",
                            CELL_TYPE = tempStore.Where(x => x.CELL_NO == 1).Select(x => x.CELL_TYPE).FirstOrDefault(),
                            CREATED_Date = tempStore.Select(x => x.CREATED_Date).FirstOrDefault()

                        };
                        var temp = tempStore.Where(x => x.CELL_NO == 1).FirstOrDefault();
                        if (temp != null && temp.CELL_TYPE == "LBL")
                            finalData.Options = tempStore.Where(x => x.CELL_NO == 1).Select(x => x.CELL_LABEL).FirstOrDefault();
                        else
                            finalData.Options = tempStore.Where(x => x.CELL_NO == 1).Select(x => x.Answer).FirstOrDefault() ?? "0";


                        finalDataList.Add(finalData);
                    }
                }
            }
            return finalDataList;
        }

        public List<SurveyDDl> GetSurveys(User userInfo)
        {
            var query = $"SELECT DISTINCT SURVEY_CODE,SURVEY_EDESC FROM DIST_QA_SET_SALESPERSON_MAP WHERE COMPANY_CODE = '{userInfo.company_code}'";
            var data = _objectEntity.SqlQuery<SurveyDDl>(query).ToList();
            return data;
        }

        public List<FormSetupModel> GetFormCode(User userIndo)
        {
            string query = $@"SELECT 
                            DISTINCT FS.FORM_CODE, 
                            INITCAP(FS.FORM_EDESC) FORM_EDESC
                            FROM FORM_DETAIL_SETUP DS, FORM_SETUP FS
                            WHERE table_name  IN ( 'SA_SALES_ORDER')                           
                            AND FS.DELETED_FLAG = 'N'
                            AND FS.FORM_CODE = DS.FORM_CODE
                            AND FS.COMPANY_CODE = DS.COMPANY_CODE
                            AND FS.COMPANY_CODE='{userIndo.Company}'
                            ORDER BY INITCAP(FS.FORM_EDESC)";
            var voucherList = _objectEntity.SqlQuery<FormSetupModel>(query).ToList();
            return voucherList;
        }

        public List<DistAreaModel> GetDistributionArea(User userInfo)
        {
            string query = $@"SELECT AREA_CODE,AREA_NAME,GROUPID
                FROM DIST_AREA_MASTER
                WHERE COMPANY_CODE = '{userInfo.company_code}' and deleted_flag='N' 
                ORDER BY UPPER(AREA_NAME) ASC";
            var List = _objectEntity.SqlQuery<DistAreaModel>(query).ToList();
            return List;
        }
        public List<DistAreaModel> GetIndividualGroup(User userInfo, string SingleAreaCode)
        {
            string selectQuery = $@"SELECT GROUPID FROM DIST_AREA_MASTER WHERE AREA_CODE = '{SingleAreaCode}' AND COMPANY_CODE ='{userInfo.company_code}'";
            var list = _objectEntity.SqlQuery<DistAreaModel>(selectQuery).ToList();
            return list;
        }
        public List<DistAreaModel> GetDistributionAreaByRouteCode(string routeCode, User userInfo)
        {
            string query = $@"SELECT B.AREA_CODE,B.AREA_NAME FROM DIST_ROUTE_AREA A, DIST_AREA_MASTER B WHERE A.ROUTE_CODE = '{routeCode}' 
                AND A.AREA_CODE = B.AREA_CODE AND A.COMPANY_CODE='{userInfo.company_code}' AND A.COMPANY_CODE=B.COMPANY_CODE ";
            var List = _objectEntity.SqlQuery<DistAreaModel>(query).ToList();
            return List;
        }


        public List<RouteEntityModel> getSelectedCustomerByRouteCode(string routeCode, User userInfo)
        {
            string query = $@" SELECT ENTITY_CODE,ENTITY_TYPE,TO_CHAR(ORDER_NO) ORDER_NO
                FROM DIST_ROUTE_ENTITY
                WHERE ROUTE_CODE = '{routeCode}' AND COMPANY_CODE ='{userInfo.company_code}'
        UNION ALL
        SELECT ENTITY_CODE,ENTITY_TYPE,TO_CHAR(ORDER_NO) ORDER_NO
                FROM BRD_ROUTE_ENTITY
                WHERE ROUTE_CODE = '{routeCode}' AND COMPANY_CODE ='{userInfo.company_code}'";
            var List = _objectEntity.SqlQuery<RouteEntityModel>(query).ToList();
            return List;
        }

        public List<CustomerSales> GetCustomerSales(string DivisionCode = "", string CompanyCode = "01", string IsorderFromOutlet = "false")
        {
            var divisionString = "";
            var subquery = "";
            if (!string.IsNullOrEmpty(DivisionCode))
                divisionString = $"and x.DIVISION_CODE in ({DivisionCode})";

            if (IsorderFromOutlet.ToLower() == "True".ToLower())
                subquery = $@"UNION ALL
                SELECT  RM.LATITUDE,RM.LONGITUDE,SUM(R.TOTAL_PRICE) as sales FROM DIST_IP_SSR_PURCHASE_ORDER R,DIST_RESELLER_MASTER RM WHERE R.COMPANY_CODE=RM.COMPANY_CODE
              AND R.RESELLER_CODE=RM.RESELLER_CODE AND R.COMPANY_CODE in ({CompanyCode})  group by RM.LATITUDE,RM.LONGITUDE";


            string query = $@"select Y.LATITUDE,Y.LONGITUDE,
                      sum(x.calc_total_price) as sales
                from sa_sales_invoice x, dist_distributor_master y
                where x.customer_code = y.distributor_code
                and x.company_code = y.company_code
                    and x.deleted_flag='N'
                and x.company_code in ({CompanyCode})
                 {divisionString}
                group by Y.LATITUDE,Y.LONGITUDE {subquery}";
            var List = _objectEntity.SqlQuery<CustomerSales>(query).ToList();
            return List;

        }

        public List<CustomerIModel> GetIndividualCustomer(User userInfo)
        {
            //string query = $@"SELECT DISTINCT TRIM(CS.CUSTOMER_EDESC)AS CUSTOMER_EDESC, CS.CUSTOMER_CODE            
            //FROM SA_CUSTOMER_SETUP CS
            //WHERE CS.DELETED_FLAG = 'N'  
            //AND CS.GROUP_SKU_FLAG = 'I'          
            //AND CS.COMPANY_CODE = '{userInfo.company_code}'
            //ORDER BY TRIM(CS.CUSTOMER_EDESC) ASC";

            string query = $@"SELECT CS.CUSTOMER_CODE DISTRIBUTOR_CODE, TRIM(CS.CUSTOMER_EDESC) AS CUSTOMER_EDESC, DM.AREA_CODE, TRIM(AM.AREA_NAME) AREA_NAME
                            FROM SA_CUSTOMER_SETUP CS
                            LEFT JOIN DIST_DISTRIBUTOR_MASTER DM ON DM.DISTRIBUTOR_CODE = CS.CUSTOMER_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE AND DM.ACTIVE = 'Y'
                            LEFT JOIN DIST_AREA_MASTER AM ON TRIM(AM.AREA_CODE) = TRIM(DM.AREA_CODE) AND AM.COMPANY_CODE = DM.COMPANY_CODE
                            WHERE 1 = 1
                           -- AND DM.DELETED_FLAG='N'
                            AND CS.COMPANY_CODE = '{userInfo.company_code}'
                            AND CS.GROUP_SKU_FLAG = 'I' AND CS.DELETED_FLAG = 'N'
                            ORDER BY CS.CUSTOMER_EDESC ASC";
            var List = _objectEntity.SqlQuery<CustomerIModel>(query).ToList();
            return List;
        }

        public List<ResellerListModel> GetResellerList(User userInfo)
        {
            string query = $@"SELECT RESELLER_CODE,RESELLER_NAME
                                FROM DIST_RESELLER_MASTER 
                                WHERE ACTIVE ='Y'
                                AND IS_CLOSED = 'N'
                                AND COMPANY_CODE = '{userInfo.company_code}'           
                                ORDER BY RESELLER_NAME";
            var List = _objectEntity.SqlQuery<ResellerListModel>(query).ToList();
            return List;
        }

        public List<CustomerIModel> GetWholeSellers(User userInfo)
        {
            string Query = $"SELECT RESELLER_CODE CUSTOMER_CODE,RESELLER_NAME CUSTOMER_EDESC,DISTRIBUTOR_CODE,AREA_CODE FROM DIST_RESELLER_MASTER WHERE TRIM(WHOLESELLER)='Y' AND COMPANY_CODE='{userInfo.company_code}' AND IS_CLOSED = 'N'";
            var wholeSellers = _objectEntity.SqlQuery<CustomerIModel>(Query).ToList();
            return wholeSellers;
        }

        public List<DistAreaModel> GetRouteDistributor(string areaCode, User userInfo)
        {
            if (areaCode == "none")
            {
                return new List<DistAreaModel>();
            }
            string query = $@"SELECT 
                DM.DISTRIBUTOR_CODE, 
                CS.CUSTOMER_EDESC AS DISTRIBUTOR_NAME,
                CS.REGD_OFFICE_EADDRESS REGD_OFFICE_ADDRESS,
                AM.AREA_CODE,
                AM.AREA_NAME,
                DM.LATITUDE, DM.LONGITUDE
                FROM DIST_DISTRIBUTOR_MASTER DM                
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE AND DM.ACTIVE = 'Y' AND DM.DELETED_FLAG = 'N'
                INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE AND CS.GROUP_SKU_FLAG ='I' AND CS.DELETED_FLAG='N'
                WHERE DM.AREA_CODE IN ({areaCode}) AND 
                DM.ACTIVE = 'Y'
                AND DM.COMPANY_CODE = '{userInfo.company_code}'
                ORDER BY UPPER(AM.AREA_NAME), UPPER(CS.CUSTOMER_EDESC)";
            var List = _objectEntity.SqlQuery<DistAreaModel>(query).ToList();
            return List;
        }


        public List<ResellerAreaModel> GetRouteReseller(string areaCode, User userInfo)
        {
            if (areaCode == "none")
            {
                return new List<ResellerAreaModel>();
            }
            string query = $@"SELECT 
                RM.RESELLER_CODE,
                RM.RESELLER_NAME,
                RM.REG_OFFICE_ADDRESS REGD_OFFICE_ADDRESS,
                AM.AREA_CODE,
                AM.AREA_NAME,
                RM.DISTRIBUTOR_CODE,
                CS.CUSTOMER_EDESC AS DISTRIBUTOR_NAME,
                RM.CONTACT_SUFFIX,
                RM.CONTACT_NAME,
                RM.CONTACT_NO,
                RM.LATITUDE,
                RM.LONGITUDE,
                OT.TYPE_EDESC AS OUTLET_TYPE,
                OST.SUBTYPE_EDESC AS OUTLET_SIZE
                FROM DIST_RESELLER_MASTER RM
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = RM.AREA_CODE AND AM.COMPANY_CODE = RM.COMPANY_CODE
                 LEFT JOIN DIST_RESELLER_ENTITY DM ON RM.RESELLER_CODE=DM.RESELLER_CODE AND DM.ENTITY_CODE = RM.DISTRIBUTOR_CODE  AND DM.COMPANY_CODE = RM.COMPANY_CODE AND DM.ENTITY_TYPE='D'
                LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.RESELLER_CODE AND CS.COMPANY_CODE = RM.COMPANY_CODE AND CS.GROUP_SKU_FLAG ='I' AND CS.DELETED_FLAG='N'
                LEFT JOIN DIST_OUTLET_TYPE OT ON OT.TYPE_ID = RM.OUTLET_TYPE_ID AND OT.COMPANY_CODE = RM.COMPANY_CODE
                LEFT JOIN DIST_OUTLET_SUBTYPE OST ON OST.TYPE_ID=RM.OUTLET_TYPE_ID AND OST.SUBTYPE_ID = RM.OUTLET_SUBTYPE_ID AND OST.COMPANY_CODE = RM.COMPANY_CODE
                WHERE RM.AREA_CODE IN ({areaCode})
                AND RM.ACTIVE = 'Y'
                AND RM.IS_CLOSED = 'N'
              --  AND DM.ACTIVE = 'Y'
               -- AND DM.DELETED_FLAG = 'N'
                AND RM.DELETED_FLAG = 'N'
                AND RM.COMPANY_CODE = '{userInfo.company_code}'
                ORDER BY UPPER(TRIM(RM.RESELLER_NAME))";
            var List = _objectEntity.SqlQuery<ResellerAreaModel>(query).ToList();
            return List;
        }


        public List<PhotoInfoModel> GetPhotoInfo(string entityType, string entityCode)
        {
            if (entityType.Equals("P", StringComparison.OrdinalIgnoreCase) || entityType.Equals("DEALER", StringComparison.OrdinalIgnoreCase))
                entityType = "P";
            else if (entityType.Equals("D", StringComparison.OrdinalIgnoreCase) || entityType.Equals("DISTRIBUTOR", StringComparison.OrdinalIgnoreCase))
                entityType = "D";
            else if (entityType.Equals("R", StringComparison.OrdinalIgnoreCase) || entityType.Equals("RESELLER", StringComparison.OrdinalIgnoreCase))
                entityType = "R";

            var query = $@"SELECT P.IMAGE_NAME AS FILENAME,NVL(P.IMAGE_DESC,' ') AS DESCRIPTION,'' AS URL,P.TYPE AS ENTITY_TYPE,
            P.ENTITY_CODE,'Visit' AS MEDIA_TYPE,
            P.SP_CODE CREATED_BY,P.UPLOAD_DATE CREATE_DATE,L.EMPLOYEE_EDESC USER_NAME
            FROM DIST_VISIT_IMAGE  P,
            HR_EMPLOYEE_SETUP L WHERE
            P.COMPANY_CODE = L.COMPANY_CODE
            AND L.EMPLOYEE_CODE = P.SP_CODE
            AND P.TYPE='{entityType}'
            AND P.ENTITY_CODE='{entityCode}'
            AND ROWNUM <=10  UNION ALL
             SELECT P.FILENAME AS FILENAME,NVL(P.DESCRIPTION,' ') AS DESCRIPTION,'' AS URL,P.ENTITY_TYPE AS ENTITY_TYPE,
            P.ENTITY_CODE,p. MEDIA_TYPE,
            '' as CREATED_BY,P.CREATE_DATE CREATE_DATE,'' USER_NAME
            FROM DIST_PHOTO_INFO  P
            where p.media_type in ('PCONTACT','STORE')
 AND P.ENTITY_CODE='{entityCode}'
            AND ROWNUM <=10";

            //query = $@"select P.FILENAME,P.DESCRIPTION,P.URL,
            //            (CASE  P.ENTITY_TYPE
            //           WHEN 'R' THEN 'Reseller'
            //           WHEN 'D' THEN 'Dealer'
            //           WHEN 'P' THEN 'Distributor'
            //           END) ENTITY_TYPE,
            //        P.ENTITY_CODE,P.MEDIA_TYPE,P.CREATED_BY,P.CREATE_DATE,L.USER_NAME from DIST_PHOTO_INFO  P,DIST_LOGIN_USER L WHERE
            //        P.COMPANY_CODE = L.COMPANY_CODE
            //    AND L.USERID = P.CREATED_BY
            //    AND P.ENTITY_CODE='{entityCode}'
            //    AND P.ENTITY_TYPE ='R'
            //    union all
            //           select P.image_name as FILENAME,P.image_desc as DESCRIPTION,'' as URL,
            //           (CASE  P.TYPE
            //           WHEN 'R' THEN 'Reseller'
            //           WHEN 'D' THEN 'Dealer'
            //           WHEN 'P' THEN 'Distributor'
            //           END) ENTITY_TYPE,
            //           P.ENTITY_CODE,'Visit' as MEDIA_TYPE,
            //    P.sp_code,P.upload_DATE,L.employee_edesc from DIST_VISIT_IMAGE  P,hr_employee_setup L WHERE
            //        P.COMPANY_CODE = L.COMPANY_CODE
            //    AND L.employee_code = P.sp_code
            //    AND P.ENTITY_CODE='{entityCode}'
            //    AND P.TYPE='{entityType}'";

            var data = _objectEntity.SqlQuery<PhotoInfoModel>(query).ToList();
            return data;
        }

        public DistCustomerInfoModel GetDistCustomerInfo(string entityType, string entityCode, string CompanyCode)
        {
            string query = string.Empty;
            string queryItems = string.Empty;
            var data = new DistCustomerInfoModel();
            if (entityType.Trim().ToUpper() == "DEALER")
            {
                query = $@"SELECT 
                    'Dealer' TYPE,
                    ADDRESS ADDRESS,
                    TEL_NO CONTACT_NO,PAN_NO,
                    (SELECT NVL(SUM(TOTAL_PRICE),0) FROM SA_SALES_INVOICE WHERE PARTY_TYPE_CODE = '{entityCode}') TOTAL_SALES,
                    (SELECT NVL(SUM(TOTAL_PRICE),0) FROM SA_SALES_INVOICE WHERE PARTY_TYPE_CODE = '{entityCode}' AND CREATED_DATE BETWEEN TRUNC(SYSDATE, 'mm') AND TRUNC(SYSDATE)) MONTH_SALES,
                    (SELECT NVL(SUM(SL.CR_AMOUNT),0) FROM V$VIRTUAL_SUB_LEDGER SL,IP_PARTY_TYPE_CODE PC WHERE SL.ACC_CODE=PC.ACC_CODE AND PC.PARTY_TYPE_CODE='{entityCode}') TOTAL_COLLECTION,
                    (0) REMAINING_PO
                FROM IP_PARTY_TYPE_CODE
                WHERE PARTY_TYPE_CODE='{entityCode}' AND COMPANY_CODE='{CompanyCode}'";
                queryItems = $@"SELECT TO_CHAR(PO.ORDER_DATE,'DD/MM/YYYY') ORDER_DATE,IMS.ITEM_EDESC,PO.QUANTITY,PO.TOTAL_PRICE FROM DIST_IP_SSD_PURCHASE_ORDER PO,IP_ITEM_MASTER_SETUP IMS
                    WHERE PO.ITEM_CODE=IMS.ITEM_CODE
                    AND ORDER_NO=(SELECT MAX(ORDER_NO) FROM DIST_IP_SSD_PURCHASE_ORDER WHERE PARTY_TYPE_CODE='{entityCode}')";

                data = _objectEntity.SqlQuery<DistCustomerInfoModel>(query).FirstOrDefault();
                data.ITEMS = _objectEntity.SqlQuery<LastPoItems>(queryItems).ToList();
            }

            else if (entityType.Trim().ToUpper() == "DISTRIBUTOR")
            {
                query = $@"SELECT 
                    'Distributor' TYPE,
                    REGD_OFFICE_EADDRESS ADDRESS,
                    TEL_MOBILE_NO1 CONTACT_NO,PAN_NO,
                    (SELECT NVL(SUM(CALC_TOTAL_PRICE),0) FROM SA_SALES_INVOICE WHERE customer_code = '{entityCode}') TOTAL_SALES,
                    (SELECT NVL(SUM(CALC_TOTAL_PRICE),0) FROM SA_SALES_INVOICE WHERE customer_code = '{entityCode}' AND CREATED_DATE BETWEEN TRUNC(SYSDATE, 'mm') AND TRUNC(SYSDATE)) MONTH_SALES,
                    (SELECT NVL(SUM(CR_AMOUNT),0) FROM V$VIRTUAL_SUB_LEDGER SL WHERE SUB_CODE = (SELECT LINK_SUB_CODE FROM SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{entityCode}')) TOTAL_COLLECTION
                FROM SA_CUSTOMER_SETUP
                WHERE CUSTOMER_CODE='{entityCode}' AND COMPANY_CODE='{CompanyCode}'";
                queryItems = $@"SELECT TO_CHAR(PO.ORDER_DATE,'DD/MM/YYYY') ORDER_DATE,IMS.ITEM_EDESC,PO.QUANTITY,PO.TOTAL_PRICE FROM DIST_IP_SSD_PURCHASE_ORDER PO,IP_ITEM_MASTER_SETUP IMS
                    WHERE PO.ITEM_CODE=IMS.ITEM_CODE
                    AND ORDER_NO=(SELECT MAX(ORDER_NO) FROM DIST_IP_SSD_PURCHASE_ORDER WHERE CUSTOMER_CODE='{entityCode}')";

                data = _objectEntity.SqlQuery<DistCustomerInfoModel>(query).FirstOrDefault();
                data.ITEMS = _objectEntity.SqlQuery<LastPoItems>(queryItems).ToList();
            }

            else if (entityType.Trim().ToUpper() == "RESELLER")
            {
                query = $@"SELECT 
                    'Reseller' TYPE,
                    REG_OFFICE_ADDRESS ADDRESS,
                    CONTACT_NO,PAN_NO,
                    (SELECT NVL(SUM(TOTAL_PRICE),0) FROM SA_SALES_INVOICE WHERE customer_code = '{entityCode}') TOTAL_SALES,
                    (SELECT NVL(SUM(TOTAL_PRICE),0) FROM SA_SALES_INVOICE WHERE customer_code = '{entityCode}' AND CREATED_DATE BETWEEN TRUNC(SYSDATE, 'mm') AND TRUNC(SYSDATE)) MONTH_SALES,
                    (SELECT NVL(SUM(CR_AMOUNT),0) FROM V$VIRTUAL_SUB_LEDGER WHERE sub_code = 'C'||'{entityCode}') TOTAL_COLLECTION,
                    (0) REMAINING_PO
                FROM DIST_RESELLER_MASTER
                WHERE RESELLER_CODE='{entityCode}' AND COMPANY_CODE='{CompanyCode}' AND IS_CLOSED = 'N'";
                queryItems = $@"SELECT TO_CHAR(PO.ORDER_DATE,'DD/MM/YYYY') ORDER_DATE,IMS.ITEM_EDESC,PO.QUANTITY,PO.TOTAL_PRICE FROM DIST_IP_SSR_PURCHASE_ORDER PO,IP_ITEM_MASTER_SETUP IMS
                    WHERE PO.ITEM_CODE=IMS.ITEM_CODE
                    AND ORDER_NO=(SELECT MAX(ORDER_NO) FROM DIST_IP_SSR_PURCHASE_ORDER WHERE RESELLER_CODE='{entityCode}')";

                data = _objectEntity.SqlQuery<DistCustomerInfoModel>(query).FirstOrDefault();
                if (data == null)
                    data = new DistCustomerInfoModel();
                else
                    data.ITEMS = _objectEntity.SqlQuery<LastPoItems>(queryItems).ToList();
            }
            return data;
        }

        public List<PartyTypeAreaModel> GetRouteDealer(string areaCode, User userInfo)
        {
            if (areaCode == "none")
            {
                return new List<PartyTypeAreaModel>();
            }
            string query = $@" SELECT
                DM.DEALER_CODE,
                CS.PARTY_TYPE_EDESC AS DEALER_NAME,
                DM.REG_OFFICE_ADDRESS REGD_OFFICE_ADDRESS,
                AM.AREA_CODE,
                AM.AREA_NAME,
                DM.LATITUDE, DM.LONGITUDE
                FROM DIST_DEALER_MASTER DM
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = DM.AREA_CODE AND AM.COMPANY_CODE = DM.COMPANY_CODE
                INNER JOIN IP_PARTY_TYPE_CODE CS ON CS.PARTY_TYPE_CODE = DM.DEALER_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
                WHERE DM.AREA_CODE IN ({areaCode}) AND
                DM.ACTIVE = 'Y'
                AND DM.COMPANY_CODE = '{userInfo.company_code}'
                ORDER BY UPPER(AM.AREA_NAME), UPPER(CS.PARTY_TYPE_EDESC)";
            var List = _objectEntity.SqlQuery<PartyTypeAreaModel>(query).ToList();
            return List;
        }

        public List<HoardingAreaModel> GetRouteHoarding(string areaCode, User userInfo)
        {
            if (areaCode == "none")
            {
                return new List<HoardingAreaModel>();
            }
            string query = $@"SELECT
                BOE.CODE AS ENTITY_CODE, BOE.DESCRIPTION AS ENTITY_NAME,
                AM.AREA_CODE, AM.AREA_NAME, BOE.LATITUDE, BOE.LONGITUDE
                FROM BRD_OTHER_ENTITY BOE
                INNER JOIN DIST_AREA_MASTER AM ON AM.AREA_CODE = BOE.AREA_CODE AND AM.COMPANY_CODE = BOE.COMPANY_CODE
                WHERE BOE.AREA_CODE IN ({areaCode}) AND
                BOE.DELETED_FLAG = 'N'
                AND BOE.COMPANY_CODE = '{userInfo.company_code}'
                ORDER BY UPPER(AM.AREA_NAME)";
            var List = _objectEntity.SqlQuery<HoardingAreaModel>(query).ToList();
            return List;
        }

        public List<CollectionModel> GetCollectionReport(ReportFiltersModel reportFilters, User userInfo)
        {
            var companyCode = string.Join(",", reportFilters.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            //****************************
            //CONDITIONS FITLER START HERE
            //****************************


            var filter = string.Empty;
            if (reportFilters.BranchFilter.Count > 0)
            {
                filter += string.Format(@" AND C.BRANCH_CODE IN ('{0}')", string.Join("','", reportFilters.BranchFilter).ToString());
            }
            if (!string.IsNullOrEmpty(reportFilters.FromDate))
                filter = filter + " AND C.CREATED_DATE>=TO_DATE('" + reportFilters.FromDate + "', 'YYYY-MM-DD') and c.CREATED_DATE <= TO_DATE('" + reportFilters.ToDate + "', 'YYYY-MM-DD')";

            if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                filter = filter + $" AND ES.EMPLOYEE_CODE IN  ({userInfo.sp_codes})";
            }

            //****************************
            //CONDITIONS FITLER END HERE
            //****************************

            var query = $@"SELECT SP_CODE, SALESPERSON_NAME, ENTITY_CODE, ENTITY_TYPE, ENTITY_NAME, BILL_NO, PAYMENT_MODE, CHEQUE_NO, BANK_NAME, AMOUNT, REMARKS, CHEQUE_CLEARANCE_DATE, CHEQUE_DEPOSIT_BANK, DELETED_FLAG, CREATED_DATE,MITI
                    FROM (
                        SELECT C.SP_CODE, ES.EMPLOYEE_EDESC AS SALESPERSON_NAME, C.ENTITY_CODE, 'Distributor'  ENTITY_TYPE, CS.CUSTOMER_EDESC AS ENTITY_NAME, C.BILL_NO, C.PAYMENT_MODE, C.CHEQUE_NO, C.BANK_NAME,BS_DATE(TO_CHAR(C.CREATED_DATE)) AS MITI,
                        C.AMOUNT, C.REMARKS, C.CHEQUE_CLEARANCE_DATE, C.CHEQUE_DEPOSIT_BANK, C.DELETED_FLAG, TO_CHAR(C.CREATED_DATE,'YYYY-MM-DD') AS CREATED_DATE 
                        FROM DIST_COLLECTION C 
                        LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = C.ENTITY_CODE AND CS.COMPANY_CODE = C.COMPANY_CODE AND CS.GROUP_SKU_FLAG = 'I' AND CS.DELETED_FLAG = 'N' 
                        LEFT JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = C.SP_CODE and c.company_code=es.company_code and c.company_code=cs.company_code   AND ES.GROUP_SKU_FLAG = 'I' AND ES.DELETED_FLAG = 'N' 
                        WHERE C.ENTITY_TYPE = 'D' AND C.COMPANY_CODE IN({companyCode }) {filter}
                        UNION 
                        SELECT C.SP_CODE, ES.EMPLOYEE_EDESC AS SALESPERSON_NAME, C.ENTITY_CODE, 'Reseller'  ENTITY_TYPE, RM.RESELLER_NAME AS ENTITY_NAME, C.BILL_NO, C.PAYMENT_MODE, C.CHEQUE_NO, C.BANK_NAME,BS_DATE(TO_CHAR(C.CREATED_DATE)) AS MITI,
                        C.AMOUNT, C.REMARKS, C.CHEQUE_CLEARANCE_DATE, C.CHEQUE_DEPOSIT_BANK, C.DELETED_FLAG, TO_CHAR(C.CREATED_DATE,'YYYY-MM-DD') AS CREATED_DATE
                        FROM DIST_COLLECTION C
                        LEFT JOIN DIST_RESELLER_MASTER RM ON RM.RESELLER_CODE = C.ENTITY_CODE 
                        LEFT JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = C.SP_CODE and c.company_code=es.company_code and c.company_code=es.company_code   AND ES.GROUP_SKU_FLAG = 'I' AND ES.DELETED_FLAG = 'N' 
                        WHERE C.ENTITY_TYPE = 'R' AND C.COMPANY_CODE IN({companyCode}) AND RM.IS_CLOSED = 'N' {filter}
                        UNION ALL
                           SELECT C.SP_CODE, ES.EMPLOYEE_EDESC AS SALESPERSON_NAME, C.ENTITY_CODE, 'Dealer'  ENTITY_TYPE, PS.PARTY_TYPE_EDESC AS ENTITY_NAME, C.BILL_NO, C.PAYMENT_MODE, C.CHEQUE_NO, C.BANK_NAME,BS_DATE(TO_CHAR(C.CREATED_DATE)) AS MITI,
                        C.AMOUNT, C.REMARKS, C.CHEQUE_CLEARANCE_DATE, C.CHEQUE_DEPOSIT_BANK, C.DELETED_FLAG, TO_CHAR(C.CREATED_DATE,'YYYY-MM-DD') AS CREATED_DATE
                        FROM DIST_COLLECTION C 
                        LEFT JOIN DIST_DEALER_MASTER RM ON RM.DEALER_CODE = C.ENTITY_CODE 
                        LEFT JOIN IP_PARTY_TYPE_CODE PS ON PS.PARTY_TYPE_CODE = C.ENTITY_CODE AND PS.DELETED_FLAG = 'N' 
                        LEFT JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = C.SP_CODE and c.company_code=es.company_code and c.company_code=es.company_code   AND ES.GROUP_SKU_FLAG = 'I' AND ES.DELETED_FLAG = 'N' 
                        WHERE C.ENTITY_TYPE = 'P'
                        AND C.COMPANY_CODE IN({companyCode}) {filter})";
            var data = _objectEntity.SqlQuery<CollectionModel>(query).ToList();
            return data;
        }

        public List<PerformanceModel> GetPerformanceReportList(ReportFiltersModel dateFilter)
        {
            string query = $@"SELECT DLU.USERID, DLU.SP_CODE, TRIM(ES.EMPLOYEE_EDESC) EMPLOYEE_EDESC,
                             SUM(NVL(DPO.QUANTITY,0) + NVL(RPO.QUANTITY,0)) QUANTITY,
                             SUM(NVL(DPO.TOTAL_PRICE,0) + NVL(RPO.TOTAL_PRICE,0)) TOTAL_PRICE FROM DIST_LOGIN_USER DLU
                                INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = DLU.SP_CODE AND ES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                                LEFT JOIN (SELECT CREATED_BY, NVL(SUM(QUANTITY),0) QUANTITY,
                             SUM(NVL(TOTAL_PRICE,0)) TOTAL_PRICE FROM DIST_IP_SSD_PURCHASE_ORDER WHERE ORDER_DATE >=TO_DATE('{dateFilter.FromDate}','YYYY-MM-DD') AND ORDER_DATE <=TO_DATE('{dateFilter.ToDate}','YYYY-MM-DD') GROUP BY CREATED_BY) DPO
                                    ON DPO.CREATED_BY = DLU.USERID
                             LEFT JOIN (SELECT CREATED_BY, NVL(SUM(QUANTITY),0) QUANTITY,
                             SUM(NVL(TOTAL_PRICE,0)) TOTAL_PRICE FROM DIST_IP_SSR_PURCHASE_ORDER WHERE ORDER_DATE >=TO_DATE('{dateFilter.FromDate}','YYYY-MM-DD') AND ORDER_DATE <=TO_DATE('{dateFilter.ToDate}','YYYY-MM-DD') GROUP BY CREATED_BY) RPO 
                                    ON RPO.CREATED_BY = DLU.USERID
                           WHERE 1 =1
                             GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(ES.EMPLOYEE_EDESC) ORDER BY UPPER(TRIM(ES.EMPLOYEE_EDESC)) ";
            var result = _objectEntity.SqlQuery<PerformanceModel>(query).ToList();
            return result;
        }

        public List<RouteModel> getAllRoutesByFilter(string filter, string empCode)
        {
            try
            {
                if (string.IsNullOrEmpty(filter)) { filter = string.Empty; }
                var sqlquery = $@" SELECT RM.ROUTE_CODE,RM.ROUTE_NAME,AR.AREA_CODE FROM DIST_ROUTE_MASTER RM, DIST_ROUTE_AREA AR,DIST_USER_AREAS UA
                                    WHERE AR.COMPANY_CODE=RM.COMPANY_CODE
                                     AND RM.ROUTE_CODE=AR.ROUTE_CODE AND AR.AREA_CODE=UA.AREA_CODE
                                     AND UA.AREA_CODE=AR.AREA_CODE AND RM.COMPANY_CODE=UA.COMPANY_CODE AND UA.SP_CODE='{empCode}'
                                     ORDER BY RM.ROUTE_NAME";
                var route = _objectEntity.SqlQuery<RouteModel>(sqlquery).ToList();
                return route;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<RouteModelPlan> GetRouteByPlanCode(string code, User userInfo)
        {
            var companyCode = userInfo.company_code;
            try
            {
                if (string.IsNullOrEmpty(code)) { code = string.Empty; }
                //var sqlquery = $@"select pr.ROUTE_EDESC, pr.ROUTE_CODE from  pl_plan_routes_mapping pprm, PL_ROUTES pr 
                //            WHERE pr.ROUTE_CODE = pprm.ROUTE_CODE AND pprm.PLAN_ROUTES_CODE = {code}";
                var sqlquery = $@"  SELECT RPD.ROUTE_CODE, RM.ROUTE_NAME, TO_CHAR(RPD.ASSIGN_DATE,'DD-MM-YYYY')ASSIGN_DATE, RPD.EMP_CODE, RP.START_DATE, RP.END_DATE FROM dist_route_detail RPD, DIST_ROUTE_MASTER RM, DIST_ROUTE_PLAN RP
                                WHERE RPD.ROUTE_CODE= RM.ROUTE_CODE
                                AND RPD.PLAN_CODE = RP.PLAN_CODE
                                AND RPD.DELETED_FLAG= 'N'
                                AND RPD.COMPANY_CODE = '{companyCode}'
                                AND RPD.EMP_CODE= '{code}' ORDER BY RM.ROUTE_NAME";
                var route = _objectEntity.SqlQuery<RouteModelPlan>(sqlquery).ToList();
                return route;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<DIST_PLAN_ROUTE> getAllPlanRoutes(string plancode, User userInfo)
        {
            var companyCode = userInfo.company_code;
            var sqlquery = $@"SELECT PLAN_CODE ,PLAN_EDESC,PLAN_NDESC,START_DATE,END_DATE,TARGET_NAME ,TARGET_VALUE,TIME_FRAME_CODE,
REMARKS ,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,LAST_MODIFIED_BY,LAST_MODIFIED_DATE,APPROVED_BY,APPROVED_DATE,DELETED_FLAG,
(SELECT  TIME_FRAME_EDESC FROM PL_TIME_FRAME WHERE TIME_FRAME_CODE = DIST_ROUTE_PLAN.TIME_FRAME_CODE) AS TIME_FRAME_EDESC 
FROM DIST_ROUTE_PLAN WHERE DELETED_FLAG='N' AND COMPANY_CODE = '{companyCode}' ORDER BY START_DATE DESC, UPPER(TRIM(PLAN_EDESC)) ASC";
            if (!string.IsNullOrEmpty(plancode))
            {
                sqlquery = $@"SELECT PLAN_CODE ,PLAN_EDESC,PLAN_NDESC,START_DATE,END_DATE,TARGET_NAME ,TARGET_VALUE,TIME_FRAME_CODE,
REMARKS ,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,LAST_MODIFIED_BY,LAST_MODIFIED_DATE,APPROVED_BY,APPROVED_DATE,DELETED_FLAG,
(SELECT  TIME_FRAME_EDESC FROM PL_TIME_FRAME WHERE TIME_FRAME_CODE = DIST_ROUTE_PLAN.TIME_FRAME_CODE) AS TIME_FRAME_EDESC 
FROM DIST_ROUTE_PLAN WHERE  DELETED_FLAG='N' AND COMPANY_CODE = '{companyCode}' AND PLAN_CODE='{plancode}' ORDER BY START_DATE DESC, UPPER(TRIM(PLAN_EDESC)) ASC";
            }
            var route = _objectEntity.SqlQuery<DIST_PLAN_ROUTE>(sqlquery).ToList();
            return route;
        }

        public List<ModelEmployee> getEmployees(string filter, string empGroup, User userInfo)
        {
            var companyCode = userInfo.company_code;
            try
            {
                var condition = string.Empty;
                if (empGroup != "" && empGroup != null)
                    condition = $@" AND LU.GROUPID ='{empGroup}'";

                string query = $@"SELECT DISTINCT SPM.SP_CODE SP_CODE,ES.EMPLOYEE_CODE EMPLOYEE_CODE,ES.EMPLOYEE_EDESC || ' ('||ES.EMPLOYEE_CODE||')' EMPLOYEE_EDESC,
                    ES.EMPLOYEE_NDESC EMPLOYEE_NDESC,ES.GROUP_SKU_FLAG GROUP_SKU_FLAG,ES.MASTER_EMPLOYEE_CODE MASTER_EMPLOYEE_CODE,
                    ES.PRE_EMPLOYEE_CODE PRE_EMPLOYEE_CODE
                    FROM HR_EMPLOYEE_SETUP ES,DIST_SALESPERSON_MASTER SPM, DIST_LOGIN_USER LU
                    WHERE SPM.SP_CODE = LU.SP_CODE
                    AND SPM.COMPANY_CODE = LU.COMPANY_CODE
                    AND ES.DELETED_FLAG='N' AND SPM.ACTIVE='Y' AND LU.ACTIVE = 'Y'
                    AND SPM.SP_CODE=ES.EMPLOYEE_CODE 
                    AND ES.COMPANY_CODE = SPM.COMPANY_CODE
                    AND SPM.COMPANY_CODE = '{companyCode}' {condition}
                    ORDER BY LOWER(TRIM(ES.EMPLOYEE_EDESC || ' ('||ES.EMPLOYEE_CODE||')'))";
                if (!string.IsNullOrEmpty(filter))
                {
                    //query = $@"SELECT EMPLOYEE_CODE,EMPLOYEE_EDESC,EMPLOYEE_NDESC,GROUP_SKU_FLAG,MASTER_EMPLOYEE_CODE,PRE_EMPLOYEE_CODE FROM HR_EMPLOYEE_SETUP WHERE DELETED_FLAG='N' AND LOWER(EMPLOYEE_EDESC) LIKE '%" + filter.ToLower() + "%'";
                    //query = $@"SELECT DISTINCT SPM.SP_CODE SP_CODE,ES.EMPLOYEE_CODE EMPLOYEE_CODE,ES.EMPLOYEE_EDESC EMPLOYEE_EDESC,
                    //            ES.EMPLOYEE_NDESC EMPLOYEE_NDESC,ES.GROUP_SKU_FLAG GROUP_SKU_FLAG,ES.MASTER_EMPLOYEE_CODE MASTER_EMPLOYEE_CODE,
                    //            ES.PRE_EMPLOYEE_CODE PRE_EMPLOYEE_CODE
                    //        FROM HR_EMPLOYEE_SETUP ES,DIST_SALESPERSON_MASTER SPM
                    //         WHERE  ES.DELETED_FLAG='N' AND SPM.ACTIVE='Y'
                    //         AND SPM.SP_CODE=ES.EMPLOYEE_CODE
                    //         AND LOWER (TRIM(ES.EMPLOYEE_EDESC)) LIKE '%" + filter.ToLower() + "%'  ORDER BY LOWER(TRIM(ES.EMPLOYEE_EDESC))";
                    query = $@"SELECT DISTINCT SPM.SP_CODE SP_CODE,ES.EMPLOYEE_CODE EMPLOYEE_CODE,ES.EMPLOYEE_EDESC || ' ('||ES.EMPLOYEE_CODE||')' EMPLOYEE_EDESC,
                        ES.EMPLOYEE_NDESC EMPLOYEE_NDESC,ES.GROUP_SKU_FLAG GROUP_SKU_FLAG,ES.MASTER_EMPLOYEE_CODE MASTER_EMPLOYEE_CODE,
                        ES.PRE_EMPLOYEE_CODE PRE_EMPLOYEE_CODE
                        FROM HR_EMPLOYEE_SETUP ES,DIST_SALESPERSON_MASTER SPM ,  DIST_LOGIN_USER LU
                        WHERE ES.DELETED_FLAG='N' AND SPM.ACTIVE='Y' AND LU.ACTIVE = 'Y'
                        AND SPM.SP_CODE = LU.SP_CODE
                        AND SPM.COMPANY_CODE = LU.COMPANY_CODE
                        AND SPM.SP_CODE=ES.EMPLOYEE_CODE 
                        AND ES.COMPANY_CODE = SPM.COMPANY_CODE
                        AND SPM.COMPANY_CODE = '{companyCode}' {condition}
                        AND LOWER (TRIM(ES.EMPLOYEE_EDESC)) LIKE '%" + filter.ToLower() + "%' ORDER BY LOWER(TRIM(ES.EMPLOYEE_EDESC || ' ('||ES.EMPLOYEE_CODE||')'))";
                }
                var result = this._objectEntity.SqlQuery<ModelEmployee>(query).ToList();
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<SumOutletModel> GetSumOutletReportList(ReportFiltersModel dateFilter)
        {
            string query = $@"SELECT RGM.GROUPID, TRIM(RGM.GROUP_EDESC) GROUP_EDESC,
                             COUNT(RGM.GROUPID) TOTAL
                             FROM DIST_RESELLER_MASTER RM
                             INNER JOIN DIST_GROUP_MASTER RGM ON RGM.GROUPID = RM.GROUPID AND RGM.COMPANY_CODE = RM.COMPANY_CODE WHERE 1 = 1 AND RM.IS_CLOSED = 'N'
                                    AND RM.CREATED_DATE BETWEEN TO_DATE('{dateFilter.FromDate}', 'YYYY-MM-DD') AND TO_DATE('{dateFilter.ToDate}', 'YYYY-MM-DD') GROUP BY RGM.GROUPID, TRIM(RGM.GROUP_EDESC)";

            var result = _objectEntity.SqlQuery<SumOutletModel>(query).ToList();
            return result;
        }
        public List<OutletSummaryModel> GetOutletSummaryReportList(ReportFiltersModel dateFilter)
        {
            string query = $@"SELECT VSTBL.SP_CODE, VSTBL.EMPLOYEE_EDESC, SUM(VSTBL.NOT_VISITED) NOT_VISITED, SUM(VSTBL.VISITED) VISITED FROM (SELECT PTBL.*
       FROM (SELECT PNVTBL.SP_CODE, PNVTBL.EMPLOYEE_EDESC,
NVL(PNVTBL.NOT_VISITED,0) NOT_VISITED, NVL(PVTBL.VISITED,0) VISITED
             FROM (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC,
COUNT(ENT.ENTITY_CODE) NOT_VISITED
                   FROM (
                       SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.PARTY_TYPE_CODE ENTITY_CODE,
TRIM(SCS.PARTY_TYPE_EDESC)
                       FROM DIST_LOGIN_USER DLU
                       INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                       LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N'
                       LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                       LEFT JOIN IP_PARTY_TYPE_CODE SCS ON SCS.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                       GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.PARTY_TYPE_CODE, TRIM(SCS.PARTY_TYPE_EDESC)
                   ) ENT
                   LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE FROM DIST_LOCATION_TRACK WHERE UPDATE_DATE BETWEEN TO_DATE('{dateFilter.FromDate}',
'YYYY-MM-DD') AND TO_DATE('{dateFilter.ToDate}', 'YYYY-MM-DD') GROUP BY SP_CODE, CUSTOMER_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE
                   WHERE DLT.SP_CODE IS NULL
                   GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC
             ) PNVTBL
             LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC,
COUNT(ENT.ENTITY_CODE) VISITED
                   FROM (
                       SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.PARTY_TYPE_CODE ENTITY_CODE,
TRIM(SCS.PARTY_TYPE_EDESC)
                       FROM DIST_LOGIN_USER DLU
                       INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                       LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N'
                       LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                       LEFT JOIN IP_PARTY_TYPE_CODE SCS ON SCS.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                       GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.PARTY_TYPE_CODE, TRIM(SCS.PARTY_TYPE_EDESC)
                   ) ENT
                   INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE FROM DIST_LOCATION_TRACK WHERE UPDATE_DATE BETWEEN TO_DATE('{dateFilter.FromDate}',
'YYYY-MM-DD') AND TO_DATE('{dateFilter.ToDate}', 'YYYY-MM-DD') GROUP BY SP_CODE, CUSTOMER_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE
                   WHERE DLT.SP_CODE IS NOT NULL
                   GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC
             ) PVTBL ON PVTBL.SP_CODE = PNVTBL.SP_CODE
       ) PTBL
       UNION ALL
       SELECT DTBL.*
       FROM (SELECT DNVTBL.SP_CODE, DNVTBL.EMPLOYEE_EDESC,
NVL(DNVTBL.NOT_VISITED,0) NOT_VISITED, NVL(DVTBL.VISITED,0) VISITED
             FROM (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC,
COUNT(ENT.ENTITY_CODE) NOT_VISITED
                   FROM (
                       SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC)
                       FROM DIST_LOGIN_USER DLU
                       INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                       LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N'
                       LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                       LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                       GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC)
                   ) ENT
                   LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE FROM DIST_LOCATION_TRACK WHERE UPDATE_DATE BETWEEN TO_DATE('{dateFilter.FromDate}',
'YYYY-MM-DD') AND TO_DATE('{dateFilter.ToDate}', 'YYYY-MM-DD') GROUP BY SP_CODE, CUSTOMER_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE
                   WHERE DLT.SP_CODE IS NULL
                   GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC
             ) DNVTBL
             LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC,
COUNT(ENT.ENTITY_CODE) VISITED
                   FROM (
                       SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC)
                       FROM DIST_LOGIN_USER DLU
                       INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                       LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N'
                       LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                       LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                       GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC)
                   ) ENT
                   INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE FROM DIST_LOCATION_TRACK WHERE UPDATE_DATE BETWEEN TO_DATE('{dateFilter.FromDate}',
'YYYY-MM-DD') AND TO_DATE('{dateFilter.ToDate}', 'YYYY-MM-DD') GROUP BY SP_CODE, CUSTOMER_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE
                   WHERE DLT.SP_CODE IS NOT NULL
                   GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC
             ) DVTBL ON DVTBL.SP_CODE = DNVTBL.SP_CODE
       ) DTBL
       UNION ALL
       SELECT RTBL.*
       FROM (SELECT RNVTBL.SP_CODE, RNVTBL.EMPLOYEE_EDESC,
NVL(RNVTBL.NOT_VISITED,0) NOT_VISITED, NVL(RVTBL.VISITED,0) VISITED
             FROM (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC,
COUNT(ENT.ENTITY_CODE) NOT_VISITED
                   FROM (
                       SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME)
                       FROM DIST_LOGIN_USER DLU
                       INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                       LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N'
                       LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                       LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y' AND DRM.IS_CLOSED = 'N'
                       GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME)
                   ) ENT
                   LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE FROM DIST_LOCATION_TRACK WHERE UPDATE_DATE BETWEEN TO_DATE('{dateFilter.FromDate}',
'YYYY-MM-DD') AND TO_DATE('{dateFilter.ToDate}', 'YYYY-MM-DD') GROUP BY SP_CODE, CUSTOMER_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE
                   WHERE DLT.SP_CODE IS NULL
                   GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC
             ) RNVTBL
             LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC,
COUNT(ENT.ENTITY_CODE) VISITED
                   FROM (
                       SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME)
                       FROM DIST_LOGIN_USER DLU
                       INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                       LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N'
                       LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                       LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y' AND DRM.IS_CLOSED = 'N'
                       GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME)
                   ) ENT
                   INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE FROM DIST_LOCATION_TRACK WHERE UPDATE_DATE BETWEEN TO_DATE('{dateFilter.FromDate}',
'YYYY-MM-DD') AND TO_DATE('{dateFilter.ToDate}', 'YYYY-MM-DD') GROUP BY SP_CODE, CUSTOMER_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE
                   WHERE DLT.SP_CODE IS NOT NULL
                   GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC
             ) RVTBL ON RVTBL.SP_CODE = RNVTBL.SP_CODE
       ) RTBL
) VSTBL
GROUP BY VSTBL.SP_CODE, VSTBL.EMPLOYEE_EDESC
ORDER BY UPPER(VSTBL.EMPLOYEE_EDESC)
";
            var result = _objectEntity.SqlQuery<OutletSummaryModel>(query).ToList();
            return result;
        }

        public List<TopEffectiveModel> GetTopEffectiveCallsReportList(string percentEffectiveCalls, ReportFiltersModel dateFilter)
        {
            var query = $@"
SELECT * FROM(SELECT PFMTBL.SP_CODE, PFMTBL.EMPLOYEE_EDESC,
   SUM(PFMTBL.TARGET) TARGET, SUM(PFMTBL.VISITED) VISITED,
SUM(PFMTBL.NOT_VISITED) NOT_VISITED,
   SUM(PFMTBL.PRODUCTIVE_CALLS) PRODUCTIVE_CALLS,
   ROUND(DECODE(SUM(PFMTBL.PRODUCTIVE_CALLS), NULL, 0,
         0, 0,
         (SUM(PFMTBL.PRODUCTIVE_CALLS) / SUM(PFMTBL.VISITED)) * 100),2) PERCENT_EFFECTIVE_CALLS ,
   SUM(PFMTBL.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(PFMTBL.TOTAL_AMOUNT) TOTAL_AMOUNT FROM (SELECT PTBL.*
       FROM (SELECT PETBL.SP_CODE, PETBL.EMPLOYEE_EDESC, NVL(PETBL.TARGET, 0) TARGET, NVL(PVTBL.VISITED, 0) VISITED,
NVL(PNVTBL.NOT_VISITED,0) NOT_VISITED,
             NVL(PPOTBL.PRODUCTIVE_CALLS,0) PRODUCTIVE_CALLS,
             NVL(DECODE(PRODUCTIVE_CALLS, NULL, 0,
                                              0, 0,                                             
ROUND((PPOTBL.PRODUCTIVE_CALLS / PVTBL.VISITED) * 100, 2)),0) NET_PERCENT_EFFECTIVE_CALLS,
             NVL(PPOTBL.TOTAL_QUANTITY,0) TOTAL_QUANTITY,
NVL(PPOTBL.TOTAL_PRICE,0) TOTAL_AMOUNT
             FROM (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC,
COUNT(ENT.ENTITY_CODE) TARGET
                   FROM (
                       SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE,
TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME
                       FROM DIST_LOGIN_USER DLU
                       INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                       LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N'
                       LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                       LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                       GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC)
                   ) ENT
                   GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC
                   ) PETBL -- Party Type Entity Table
             LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC,
COUNT(ENT.ENTITY_CODE) VISITED
                         FROM (
                             SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE,
TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME
                             FROM DIST_LOGIN_USER DLU
                             INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                             LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N'
                             LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                             LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                             GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), PTC.PARTY_TYPE_CODE,
TRIM(PTC.PARTY_TYPE_EDESC)
                         ) ENT
                         INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE FROM DIST_LOCATION_TRACK WHERE UPDATE_DATE BETWEEN TO_DATE('{dateFilter.FromDate}',
'YYYY-MM-DD') AND TO_DATE('{dateFilter.ToDate}', 'YYYY-MM-DD') GROUP BY SP_CODE, CUSTOMER_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE
                         WHERE DLT.SP_CODE IS NOT NULL
                         GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC
                   ) PVTBL -- Party Type Visit Table
                   ON PVTBL.SP_CODE = PETBL.SP_CODE
             LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC,
COUNT(ENT.ENTITY_CODE) NOT_VISITED
                       FROM (
                           SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.PARTY_TYPE_CODE ENTITY_CODE,
TRIM(SCS.PARTY_TYPE_EDESC) ENTITY_NAME
                           FROM DIST_LOGIN_USER DLU
                           INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                           LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N'
                           LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                           LEFT JOIN IP_PARTY_TYPE_CODE SCS ON SCS.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                           GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.PARTY_TYPE_CODE,
TRIM(SCS.PARTY_TYPE_EDESC)
                       ) ENT
                       LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE FROM DIST_LOCATION_TRACK WHERE UPDATE_DATE BETWEEN TO_DATE('{dateFilter.FromDate}',
'YYYY-MM-DD') AND TO_DATE('{dateFilter.ToDate}', 'YYYY-MM-DD') GROUP BY SP_CODE, CUSTOMER_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE
                       WHERE DLT.SP_CODE IS NULL
                       GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC
                   ) PNVTBL -- Party Type Not Visited Table
                   ON PNVTBL.SP_CODE = PETBL.SP_CODE
             LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC,
COUNT(PPO.PARTY_TYPE_CODE) PRODUCTIVE_CALLS, SUM(PPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(PPO.TOTAL_PRICE) TOTAL_PRICE
                       FROM (SELECT DLU.USERID, DLU.SP_CODE,
TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME
                             FROM DIST_LOGIN_USER DLU
                             INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                             LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N'
                             LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                             LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                             GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), PTC.PARTY_TYPE_CODE,
TRIM(PTC.PARTY_TYPE_EDESC)
                           ) ENT
                       LEFT JOIN (SELECT CREATED_BY, PARTY_TYPE_CODE,
COUNT(PARTY_TYPE_CODE) TOTAL_ORDER, SUM(QUANTITY) TOTAL_QUANTITY,
SUM(TOTAL_PRICE) TOTAL_PRICE
                                   FROM DIST_IP_SSD_PURCHASE_ORDER
                                   WHERE CREATED_DATE BETWEEN TO_DATE('{dateFilter.FromDate}', 'YYYY-MM-DD') AND TO_DATE('{dateFilter.ToDate}',
'YYYY-MM-DD')
                                   GROUP BY CREATED_BY, PARTY_TYPE_CODE
                                 ) PPO ON PPO.CREATED_BY = ENT.USERID AND PPO.PARTY_TYPE_CODE = ENT.ENTITY_CODE
                       GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC
                    ) PPOTBL -- Party Type PO Table
                    ON PPOTBL.SP_CODE = PETBL.SP_CODE
       ) PTBL
       UNION ALL
       SELECT DTBL.*
       FROM (SELECT DETBL.SP_CODE, DETBL.EMPLOYEE_EDESC, NVL(DETBL.TARGET, 0) TARGET, NVL(DVTBL.VISITED, 0) VISITED,
NVL(DNVTBL.NOT_VISITED,0) NOT_VISITED,
             NVL(DPOTBL.PRODUCTIVE_CALLS,0) PRODUCTIVE_CALLS,
             NVL(DECODE(PRODUCTIVE_CALLS, NULL, 0,
                                              0, 0,                                             
ROUND((DPOTBL.PRODUCTIVE_CALLS / DVTBL.VISITED) * 100,2)),0) NET_PERCENT_EFFECTIVE_CALLS,
             NVL(DPOTBL.TOTAL_QUANTITY,0) TOTAL_QUANTITY,
NVL(DPOTBL.TOTAL_PRICE,0) TOTAL_AMOUNT
             FROM (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC,
COUNT(ENT.ENTITY_CODE) TARGET
                   FROM (
                       SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME
                       FROM DIST_LOGIN_USER DLU
                       INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                       LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N'
                       LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                       LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                       GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC)
                   ) ENT
                   GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC
                   ) DETBL -- Customer/Distributor Entity Table
             LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC,
COUNT(ENT.ENTITY_CODE) VISITED
                         FROM (
                             SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME
                             FROM DIST_LOGIN_USER DLU
                             INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                             LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N'
                             LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                             LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                             GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC)
                         ) ENT
                         INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE FROM DIST_LOCATION_TRACK WHERE UPDATE_DATE BETWEEN TO_DATE('{dateFilter.FromDate}',
'YYYY-MM-DD') AND TO_DATE('{dateFilter.ToDate}', 'YYYY-MM-DD') GROUP BY SP_CODE, CUSTOMER_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE
                         WHERE DLT.SP_CODE IS NOT NULL
                         GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC
                   ) DVTBL -- Customer/Distributor Visit Table
                   ON DVTBL.SP_CODE = DETBL.SP_CODE
             LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC,
COUNT(ENT.ENTITY_CODE) NOT_VISITED
                       FROM (
                           SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME
                           FROM DIST_LOGIN_USER DLU
                           INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                           LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N'
                           LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                           LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                           GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC)
                       ) ENT
                       LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE FROM DIST_LOCATION_TRACK WHERE UPDATE_DATE BETWEEN TO_DATE('{dateFilter.FromDate}',
'YYYY-MM-DD') AND TO_DATE('{dateFilter.ToDate}', 'YYYY-MM-DD') GROUP BY SP_CODE, CUSTOMER_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE
                       WHERE DLT.SP_CODE IS NULL
                       GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC
                 ) DNVTBL -- Customer/Distributor Not Visited Table
                 ON DNVTBL.SP_CODE = DETBL.SP_CODE
             LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC,
COUNT(DPO.CUSTOMER_CODE) PRODUCTIVE_CALLS, SUM(DPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(DPO.TOTAL_PRICE) TOTAL_PRICE
                       FROM (SELECT DLU.USERID, DLU.SP_CODE,
TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE,
TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME
                             FROM DIST_LOGIN_USER DLU
                             INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                             LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N'
                             LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                             LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                             GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC)
                           ) ENT
                       LEFT JOIN (SELECT CREATED_BY, CUSTOMER_CODE,
COUNT(CUSTOMER_CODE) TOTAL_ORDER, SUM(QUANTITY) TOTAL_QUANTITY,
SUM(TOTAL_PRICE) TOTAL_PRICE
                                   FROM DIST_IP_SSD_PURCHASE_ORDER
                                   WHERE CREATED_DATE BETWEEN TO_DATE('{dateFilter.FromDate}', 'YYYY-MM-DD') AND TO_DATE('{dateFilter.ToDate}',
'YYYY-MM-DD')
                                   GROUP BY CREATED_BY, CUSTOMER_CODE
                                 ) DPO ON DPO.CREATED_BY = ENT.USERID AND DPO.CUSTOMER_CODE = ENT.ENTITY_CODE
                       GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC
                    ) DPOTBL -- Customer/Distributor PO Table
                    ON DPOTBL.SP_CODE = DETBL.SP_CODE
       ) DTBL
       UNION ALL
       SELECT RTBL.*
       FROM (SELECT RETBL.SP_CODE, RETBL.EMPLOYEE_EDESC, NVL(RETBL.TARGET, 0) TARGET, NVL(RVTBL.VISITED, 0) VISITED,
NVL(RNVTBL.NOT_VISITED,0) NOT_VISITED,
             NVL(RPOTBL.PRODUCTIVE_CALLS,0) PRODUCTIVE_CALLS,
             NVL(DECODE(PRODUCTIVE_CALLS, NULL, 0,
                                              0, 0,                                              
ROUND((RPOTBL.PRODUCTIVE_CALLS / RVTBL.VISITED) * 100,2)),0) NET_PERCENT_EFFECTIVE_CALLS,
             NVL(RPOTBL.TOTAL_QUANTITY,0) TOTAL_QUANTITY,
NVL(RPOTBL.TOTAL_PRICE,0) TOTAL_AMOUNT
             FROM (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC,
COUNT(ENT.ENTITY_CODE) TARGET
                   FROM (
                       SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME
                       FROM DIST_LOGIN_USER DLU
                       INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                       LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N'
                       LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                       LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y' AND DRM.IS_CLOSED = 'N'
                       GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME)
                   ) ENT
                   GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC
                   ) RETBL -- Retailer Entity Table
             LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC,
COUNT(ENT.ENTITY_CODE) VISITED
                         FROM (
                             SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME
                             FROM DIST_LOGIN_USER DLU
                             INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                             LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N'
                             LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                             LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y' AND DRM.IS_CLOSED = 'N'
                             GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME)
                         ) ENT
                         INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE FROM DIST_LOCATION_TRACK WHERE UPDATE_DATE BETWEEN TO_DATE('{dateFilter.FromDate}',
'YYYY-MM-DD') AND TO_DATE('{dateFilter.ToDate}', 'YYYY-MM-DD') GROUP BY SP_CODE, CUSTOMER_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE
                         WHERE DLT.SP_CODE IS NOT NULL
                         GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC
                   ) RVTBL -- Retailer Visit Table
                   ON RVTBL.SP_CODE = RETBL.SP_CODE
             LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC,
COUNT(ENT.ENTITY_CODE) NOT_VISITED
                       FROM (
                           SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME
                           FROM DIST_LOGIN_USER DLU
                           INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                           LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N'
                           LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                           LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y' AND DRM.IS_CLOSED = 'N'
                           GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME)
                       ) ENT
                       LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE FROM DIST_LOCATION_TRACK WHERE UPDATE_DATE BETWEEN TO_DATE('{dateFilter.FromDate}',
'YYYY-MM-DD') AND TO_DATE('{dateFilter.ToDate}', 'YYYY-MM-DD') GROUP BY SP_CODE, CUSTOMER_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE
                       WHERE DLT.SP_CODE IS NULL
                       GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC
                 ) RNVTBL -- Retailer Not Visited Table
                 ON RNVTBL.SP_CODE = RETBL.SP_CODE
             LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC,
COUNT(RPO.RESELLER_CODE) PRODUCTIVE_CALLS, SUM(RPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(RPO.TOTAL_PRICE) TOTAL_PRICE
                       FROM (SELECT DLU.USERID, DLU.SP_CODE,
TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE,
TRIM(DRM.RESELLER_NAME) ENTITY_NAME
                             FROM DIST_LOGIN_USER DLU
                             INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                             LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N'
                             LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                             LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y' AND DRM.IS_CLOSED = 'N'
                             GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME)
                           ) ENT
                       LEFT JOIN (SELECT CREATED_BY, RESELLER_CODE,
COUNT(RESELLER_CODE) TOTAL_ORDER, SUM(QUANTITY) TOTAL_QUANTITY,
SUM(TOTAL_PRICE) TOTAL_PRICE
                                   FROM DIST_IP_SSR_PURCHASE_ORDER
                                   WHERE CREATED_DATE BETWEEN TO_DATE('{dateFilter.FromDate}', 'YYYY-MM-DD') AND TO_DATE('{dateFilter.ToDate}',
'YYYY-MM-DD') 
                                   GROUP BY CREATED_BY, RESELLER_CODE
                                 ) RPO ON RPO.CREATED_BY = ENT.USERID AND RPO.RESELLER_CODE = ENT.ENTITY_CODE
                       GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC
                    ) RPOTBL -- Retailer PO Table
                    ON RPOTBL.SP_CODE = RETBL.SP_CODE
       ) RTBL
) PFMTBL
GROUP BY PFMTBL.SP_CODE, PFMTBL.EMPLOYEE_EDESC ORDER BY UPPER(PFMTBL.EMPLOYEE_EDESC)) WHERE PERCENT_EFFECTIVE_CALLS {percentEffectiveCalls}
";
            var data = _objectEntity.SqlQuery<TopEffectiveModel>(query).ToList();
            return data;
        }

        public List<TopEffectiveModel> GetALLPerformanceReport(ReportFiltersModel model, User userInfo)
        {
            //var companyCode = string.Join("','", model.CompanyFilter);
            //companyCode = companyCode == "" ? userInfo.company_code : companyCode;
            var companyCode = string.Join(",", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            var BranchFilter = string.Empty;
            var BranchFilter1 = string.Empty;
            var BranchFilter2 = string.Empty;
            if (model.BranchFilter.Count > 0)
            {
                BranchFilter = string.Format(@" AND  DLU.BRANCH_CODE IN  ('{0}')", string.Join("','", model.BranchFilter).ToString());
                BranchFilter1 = string.Format(@" AND  A.BRANCH_CODE IN  ('{0}')", string.Join("','", model.BranchFilter).ToString());
                BranchFilter2 = string.Format(@" AND  BRANCH_CODE IN  ('{0}')", string.Join("','", model.BranchFilter).ToString());
            }
            var spCode = string.Empty;
            if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                spCode = $" AND PFMTBL.SP_CODE IN  ({userInfo.sp_codes})";
            }

            var query = $@"SELECT PFMTBL.GROUP_EDESC, PFMTBL.SP_CODE, PFMTBL.EMPLOYEE_EDESC,
  SUM(PFMTBL.TARGET) TARGET, SUM(PFMTBL.VISITED) VISITED, SUM(PFMTBL.NOT_VISITED) NOT_VISITED,
  SUM(PFMTBL.PJP_PRODUCTIVE) PJP_PRODUCTIVE,
  SUM(PFMTBL.VISITED) - SUM(PFMTBL.PJP_PRODUCTIVE) PJP_NON_PRODUCTIVE,
  SUM(PFMTBL.NPJP_PRODUCTIVE) NPJP_PRODUCTIVE,
  ROUND(DECODE(SUM(PFMTBL.PJP_PRODUCTIVE), NULL, 0,
        0, 0,
        (DECODE(SUM(PFMTBL.VISITED), 0, 0 , SUM(PFMTBL.PJP_PRODUCTIVE) / SUM(PFMTBL.VISITED))) * 100),2) PERCENT_EFFECTIVE_CALLS,
  SUM(PFMTBL.PJP_TOTAL_QUANTITY) PJP_TOTAL_QUANTITY, SUM(PFMTBL.PJP_TOTAL_AMOUNT) PJP_TOTAL_AMOUNT,
  SUM(PFMTBL.NPJP_TOTAL_QUANTITY) NPJP_TOTAL_QUANTITY, SUM(PFMTBL.NPJP_TOTAL_AMOUNT) NPJP_TOTAL_AMOUNT
FROM (SELECT PTBL.*
      FROM (SELECT PETBL.GROUP_EDESC, PETBL.SP_CODE, PETBL.EMPLOYEE_EDESC, PETBL.COMPANY_CODE, NVL(PETBL.TARGET, 0) TARGET, NVL(PVTBL.VISITED, 0) VISITED, NVL(PNVTBL.NOT_VISITED,0) NOT_VISITED, 
            NVL(PPJPTBL.PJP_PRODUCTIVE,0) PJP_PRODUCTIVE,
            (NVL(PVTBL.VISITED, 0) - NVL(PPJPTBL.PJP_PRODUCTIVE,0)) PJP_NON_PRODUCTIVE,
            NVL(PNPJPTBL.NPJP_PRODUCTIVE,0) NPJP_PRODUCTIVE,
            NVL(DECODE(PPJPTBL.PJP_PRODUCTIVE, NULL, 0,
                                             0, 0,
                                             ROUND((DECODE(PVTBL.VISITED,0,0,PPJPTBL.PJP_PRODUCTIVE / PVTBL.VISITED)) * 100, 2)),0) NET_PERCENT_EFFECTIVE_CALLS,
            NVL(PPJPTBL.TOTAL_QUANTITY,0) PJP_TOTAL_QUANTITY, NVL(PPJPTBL.TOTAL_PRICE,0) PJP_TOTAL_AMOUNT,
            NVL(PNPJPTBL.TOTAL_QUANTITY,0) NPJP_TOTAL_QUANTITY, NVL(PNPJPTBL.TOTAL_PRICE,0) NPJP_TOTAL_AMOUNT
            FROM (SELECT ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) TARGET
                  FROM (
                      SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, TRIM(DGM.GROUP_EDESC) GROUP_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                      FROM DIST_LOGIN_USER DLU
                      INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                      LEFT JOIN DIST_GROUP_MASTER DGM ON DGM.GROUPID = DLU.GROUPID AND DGM.COMPANY_CODE = DGM.COMPANY_CODE
                      LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' 
                                    AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                      LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                      LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                      WHERE DLU.COMPANY_CODE IN ({companyCode}) {BranchFilter}
                      GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), TRIM(DGM.GROUP_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                  ) ENT
                  GROUP BY ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) PETBL -- Party Type Entity Table
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) VISITED
                        FROM (
                            SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' 
                                        AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                            WHERE DLU.COMPANY_CODE IN ({companyCode}) {BranchFilter}
                            GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                        ) ENT
                        INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND COMPANY_CODE IN ({companyCode}) {BranchFilter2} GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                        WHERE DLT.SP_CODE IS NOT NULL
                        GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) PVTBL -- Party Type Visit Table
                  ON PVTBL.SP_CODE = PETBL.SP_CODE AND PVTBL.COMPANY_CODE = PETBL.COMPANY_CODE
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) NOT_VISITED
                      FROM (
                          SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.PARTY_TYPE_CODE ENTITY_CODE, TRIM(SCS.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                          FROM DIST_LOGIN_USER DLU
                          INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                          LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                          LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                          LEFT JOIN IP_PARTY_TYPE_CODE SCS ON SCS.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE 
                          WHERE DLU.COMPANY_CODE IN ({companyCode}) {BranchFilter}
                          GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.PARTY_TYPE_CODE, TRIM(SCS.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                      ) ENT
                      LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND COMPANY_CODE IN ({companyCode}) {BranchFilter2} GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                      WHERE DLT.SP_CODE IS NULL
                      GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) PNVTBL -- Party Type Not Visited Table
                  ON PNVTBL.SP_CODE = PETBL.SP_CODE AND PNVTBL.COMPANY_CODE = PETBL.COMPANY_CODE
            LEFT JOIN (SELECT PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE, COUNT(PPO.PARTY_TYPE_CODE) PJP_PRODUCTIVE, SUM(PPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(PPO.TOTAL_PRICE) TOTAL_PRICE
                      FROM (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                            WHERE DLU.COMPANY_CODE IN ({companyCode}) {BranchFilter}
                            GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                          ) PJPENT
                      LEFT JOIN (SELECT CREATED_BY, PARTY_TYPE_CODE, COMPANY_CODE, COUNT(PARTY_TYPE_CODE) TOTAL_ORDER, SUM(QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE
                                  FROM DIST_IP_SSD_PURCHASE_ORDER
                                  WHERE TRUNC(ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND COMPANY_CODE IN ({companyCode}) {BranchFilter2}
                                  GROUP BY CREATED_BY, PARTY_TYPE_CODE, COMPANY_CODE
                                ) PPO ON PPO.CREATED_BY = PJPENT.USERID AND PPO.PARTY_TYPE_CODE = PJPENT.ENTITY_CODE AND PPO.COMPANY_CODE = PJPENT.COMPANY_CODE
                      GROUP BY PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE
                   ) PPJPTBL -- Party Type PJP Table
                   ON PPJPTBL.SP_CODE = PETBL.SP_CODE AND PPJPTBL.COMPANY_CODE = PETBL.COMPANY_CODE
            LEFT JOIN (SELECT PPO.SP_CODE, PPO.EMPLOYEE_EDESC, PPO.COMPANY_CODE, COUNT(PPO.PARTY_TYPE_CODE) NPJP_PRODUCTIVE, SUM(PPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(PPO.TOTAL_PRICE) TOTAL_PRICE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, A.PARTY_TYPE_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.COMPANY_CODE, COUNT(A.PARTY_TYPE_CODE) TOTAL_ORDER, SUM(A.QUANTITY) TOTAL_QUANTITY, SUM(A.TOTAL_PRICE) TOTAL_PRICE
                            FROM DIST_IP_SSD_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.ACTIVE = 'Y'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN IP_PARTY_TYPE_CODE D ON D.PARTY_TYPE_CODE = A.PARTY_TYPE_CODE AND D.COMPANY_CODE = A.COMPANY_CODE 
                            WHERE TRUNC(A.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND A.COMPANY_CODE IN ({companyCode}) {BranchFilter1}
                            GROUP BY A.CREATED_BY, B.SP_CODE, A.PARTY_TYPE_CODE, TRIM(C.EMPLOYEE_EDESC), A.COMPANY_CODE
                      ) PPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                                  WHERE DLU.COMPANY_CODE IN ({companyCode}) {BranchFilter}
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
      FROM (SELECT DETBL.GROUP_EDESC, DETBL.SP_CODE, DETBL.EMPLOYEE_EDESC, DETBL.COMPANY_CODE, NVL(DETBL.TARGET, 0) TARGET, NVL(DVTBL.VISITED, 0) VISITED, NVL(DNVTBL.NOT_VISITED,0) NOT_VISITED, 
            NVL(DPJPTBL.PJP_PRODUCTIVE,0) PJP_PRODUCTIVE,
            (NVL(DVTBL.VISITED, 0) - NVL(DPJPTBL.PJP_PRODUCTIVE,0)) PJP_NON_PRODUCTIVE,
            NVL(DNPJPTBL.NPJP_PRODUCTIVE,0) NPJP_PRODUCTIVE,
            NVL(DECODE(DPJPTBL.PJP_PRODUCTIVE, NULL, 0,
                                             0, 0,
                                             ROUND((DECODE(DVTBL.VISITED,0,0,DPJPTBL.PJP_PRODUCTIVE / DVTBL.VISITED)) * 100,2)),0) NET_PERCENT_EFFECTIVE_CALLS,
            NVL(DPJPTBL.TOTAL_QUANTITY,0) PJP_TOTAL_QUANTITY, NVL(DPJPTBL.TOTAL_PRICE,0) PJP_TOTAL_AMOUNT,
            NVL(DNPJPTBL.TOTAL_QUANTITY,0) NPJP_TOTAL_QUANTITY, NVL(DNPJPTBL.TOTAL_PRICE,0) NPJP_TOTAL_AMOUNT
            FROM (SELECT ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) TARGET
                  FROM (
                      SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, TRIM(DGM.GROUP_EDESC) GROUP_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                      FROM DIST_LOGIN_USER DLU
                      INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                      LEFT JOIN DIST_GROUP_MASTER DGM ON DGM.GROUPID = DLU.GROUPID AND DGM.COMPANY_CODE = DGM.COMPANY_CODE
                      LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                      LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                      LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                      WHERE DLU.COMPANY_CODE IN ({companyCode}) {BranchFilter}
                      GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), TRIM(DGM.GROUP_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                  ) ENT
                  GROUP BY ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) DETBL -- Customer/Distributor Entity Table
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) VISITED
                        FROM (
                            SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                            WHERE DLU.COMPANY_CODE IN ({companyCode}) {BranchFilter}
                            GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                        ) ENT
                        INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND COMPANY_CODE IN ({companyCode}) {BranchFilter2} GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                        WHERE DLT.SP_CODE IS NOT NULL
                        GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) DVTBL -- Customer/Distributor Visit Table
                  ON DVTBL.SP_CODE = DETBL.SP_CODE AND DVTBL.COMPANY_CODE = DETBL.COMPANY_CODE
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) NOT_VISITED
                      FROM (
                          SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                          FROM DIST_LOGIN_USER DLU
                          INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                          LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                          LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                          LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE 
                          WHERE DLU.COMPANY_CODE IN ({companyCode}) {BranchFilter}
                          GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                      ) ENT
                      LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND COMPANY_CODE IN ({companyCode}) {BranchFilter2} GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                      WHERE DLT.SP_CODE IS NULL
                      GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                ) DNVTBL -- Customer/Distributor Not Visited Table
                ON DNVTBL.SP_CODE = DETBL.SP_CODE AND DNVTBL.COMPANY_CODE = DETBL.COMPANY_CODE
            LEFT JOIN (SELECT PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE, COUNT(DPO.CUSTOMER_CODE) PJP_PRODUCTIVE, SUM(DPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(DPO.TOTAL_PRICE) TOTAL_PRICE
                      FROM (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                            WHERE DLU.COMPANY_CODE IN ({companyCode}) {BranchFilter}
                            GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                      ) PJPENT
                      LEFT JOIN (SELECT CREATED_BY, CUSTOMER_CODE, COMPANY_CODE, COUNT(CUSTOMER_CODE) TOTAL_ORDER, SUM(QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE
                                  FROM DIST_IP_SSD_PURCHASE_ORDER
                                  WHERE TRUNC(ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND COMPANY_CODE IN ({companyCode}) {BranchFilter2}
                                  GROUP BY CREATED_BY, CUSTOMER_CODE, COMPANY_CODE
                      ) DPO ON DPO.CREATED_BY = PJPENT.USERID AND DPO.CUSTOMER_CODE = PJPENT.ENTITY_CODE AND DPO.COMPANY_CODE = PJPENT.COMPANY_CODE
                      GROUP BY PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE
                   ) DPJPTBL -- Customer/Distributor PJP Table
                   ON DPJPTBL.SP_CODE = DETBL.SP_CODE AND DPJPTBL.COMPANY_CODE = DETBL.COMPANY_CODE
            LEFT JOIN (SELECT DPO.SP_CODE, DPO.EMPLOYEE_EDESC, DPO.COMPANY_CODE, COUNT(DPO.CUSTOMER_CODE) NPJP_PRODUCTIVE, SUM(DPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(DPO.TOTAL_PRICE) TOTAL_PRICE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, A.CUSTOMER_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.COMPANY_CODE, COUNT(A.CUSTOMER_CODE) TOTAL_ORDER, SUM(A.QUANTITY) TOTAL_QUANTITY, SUM(A.TOTAL_PRICE) TOTAL_PRICE
                            FROM DIST_IP_SSD_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.ACTIVE = 'Y'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            WHERE TRUNC(A.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND A.COMPANY_CODE IN ({companyCode}) {BranchFilter1}
                            GROUP BY A.CREATED_BY, B.SP_CODE, A.CUSTOMER_CODE, TRIM(C.EMPLOYEE_EDESC), A.COMPANY_CODE
                      ) DPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                                  WHERE DLU.COMPANY_CODE IN ({companyCode}) {BranchFilter}
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
      FROM (SELECT RETBL.GROUP_EDESC, RETBL.SP_CODE, RETBL.EMPLOYEE_EDESC, RETBL.COMPANY_CODE, NVL(RETBL.TARGET, 0) TARGET, NVL(RVTBL.VISITED, 0) VISITED, NVL(RNVTBL.NOT_VISITED,0) NOT_VISITED, 
            NVL(RPJPTBL.PJP_PRODUCTIVE,0) PJP_PRODUCTIVE,
            (NVL(RVTBL.VISITED, 0) - NVL(RPJPTBL.PJP_PRODUCTIVE,0)) PJP_NON_PRODUCTIVE,
            NVL(RNPJPTBL.NPJP_PRODUCTIVE,0) NPJP_PRODUCTIVE,
            NVL(DECODE(RPJPTBL.PJP_PRODUCTIVE, NULL, 0,
                                             0, 0,
                                             ROUND((DECODE(RVTBL.VISITED,0,0,RPJPTBL.PJP_PRODUCTIVE / RVTBL.VISITED)) * 100,2)),0) NET_PERCENT_EFFECTIVE_CALLS,
            NVL(RPJPTBL.TOTAL_QUANTITY,0) PJP_TOTAL_QUANTITY, NVL(RPJPTBL.TOTAL_PRICE,0) PJP_TOTAL_AMOUNT,
            NVL(RNPJPTBL.TOTAL_QUANTITY,0) NPJP_TOTAL_QUANTITY, NVL(RNPJPTBL.TOTAL_PRICE,0) NPJP_TOTAL_AMOUNT
            FROM (SELECT ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) TARGET
                  FROM (
                      SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, TRIM(DGM.GROUP_EDESC) GROUP_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                      FROM DIST_LOGIN_USER DLU
                      INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                      LEFT JOIN DIST_GROUP_MASTER DGM ON DGM.GROUPID = DLU.GROUPID AND DGM.COMPANY_CODE = DGM.COMPANY_CODE
                      LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                      LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                      LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y' AND DRM.IS_CLOSED = 'N'
                      WHERE DLU.COMPANY_CODE IN ({companyCode}) {BranchFilter}
                      GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), TRIM(DGM.GROUP_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                  ) ENT
                  GROUP BY ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) RETBL -- Retailer Entity Table
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) VISITED
                        FROM (
                            SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y' AND DRM.IS_CLOSED = 'N'
                            WHERE DLU.COMPANY_CODE IN ({companyCode}) {BranchFilter}
                            GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                        ) ENT
                        INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND COMPANY_CODE IN ({companyCode}) {BranchFilter2} GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                        WHERE DLT.SP_CODE IS NOT NULL
                        GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) RVTBL -- Retailer Visit Table
                  ON RVTBL.SP_CODE = RETBL.SP_CODE AND RVTBL.COMPANY_CODE = RETBL.COMPANY_CODE
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) NOT_VISITED
                      FROM (
                          SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                          FROM DIST_LOGIN_USER DLU
                          INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                          LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                          LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                          LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y' AND DRM.IS_CLOSED = 'N'
                          WHERE DLU.COMPANY_CODE IN ({companyCode}) {BranchFilter}
                          GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                      ) ENT
                      LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND COMPANY_CODE IN ({companyCode}) {BranchFilter2} GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                      WHERE DLT.SP_CODE IS NULL
                      GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                ) RNVTBL -- Retailer Not Visited Table
                ON RNVTBL.SP_CODE = RETBL.SP_CODE AND RNVTBL.COMPANY_CODE = RETBL.COMPANY_CODE
            LEFT JOIN (SELECT PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE, COUNT(RPO.RESELLER_CODE) PJP_PRODUCTIVE, SUM(RPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(RPO.TOTAL_PRICE) TOTAL_PRICE
                      FROM (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y' AND DRM.IS_CLOSED = 'N'
                            WHERE DLU.COMPANY_CODE IN ({companyCode}) {BranchFilter}
                            GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                      ) PJPENT
                      LEFT JOIN (SELECT CREATED_BY, RESELLER_CODE, COMPANY_CODE, COUNT(RESELLER_CODE) TOTAL_ORDER, SUM(QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE
                                  FROM DIST_IP_SSR_PURCHASE_ORDER
                                  WHERE TRUNC(ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND COMPANY_CODE IN ({companyCode}) {BranchFilter2}
                                  GROUP BY CREATED_BY, RESELLER_CODE, COMPANY_CODE
                      ) RPO ON RPO.CREATED_BY = PJPENT.USERID AND RPO.RESELLER_CODE = PJPENT.ENTITY_CODE AND RPO.COMPANY_CODE = PJPENT.COMPANY_CODE
                      GROUP BY PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE
                   ) RPJPTBL -- Retailer PJP Table
                   ON RPJPTBL.SP_CODE = RETBL.SP_CODE AND RPJPTBL.COMPANY_CODE = RETBL.COMPANY_CODE
            LEFT JOIN (SELECT RPO.SP_CODE, RPO.EMPLOYEE_EDESC, RPO.COMPANY_CODE, COUNT(RPO.RESELLER_CODE) NPJP_PRODUCTIVE, SUM(RPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(RPO.TOTAL_PRICE) TOTAL_PRICE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, A.RESELLER_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.COMPANY_CODE, COUNT(A.RESELLER_CODE) TOTAL_ORDER, SUM(A.QUANTITY) TOTAL_QUANTITY, SUM(A.TOTAL_PRICE) TOTAL_PRICE
                            FROM DIST_IP_SSR_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.ACTIVE = 'Y'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            WHERE TRUNC(A.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND A.COMPANY_CODE IN ({companyCode}) {BranchFilter1}
                            GROUP BY A.CREATED_BY, B.SP_CODE, A.RESELLER_CODE, TRIM(C.EMPLOYEE_EDESC), A.COMPANY_CODE
                      ) RPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'DD-MON-RRRR') AND TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y' AND DRM.IS_CLOSED = 'N'
                                  WHERE DLU.COMPANY_CODE IN ({companyCode}) {BranchFilter}
                                  GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                      ) NPJPENT ON NPJPENT.USERID = RPO.CREATED_BY AND NPJPENT.SP_CODE = RPO.SP_CODE AND NPJPENT.ENTITY_CODE = RPO.RESELLER_CODE AND NPJPENT.COMPANY_CODE = RPO.COMPANY_CODE
                      WHERE 1 = 1
                        AND NPJPENT.SP_CODE IS NULL
                      GROUP BY RPO.SP_CODE, RPO.EMPLOYEE_EDESC, RPO.COMPANY_CODE
                   ) RNPJPTBL -- Retailer NPJP Table
                   ON RNPJPTBL.SP_CODE = RETBL.SP_CODE AND RNPJPTBL.COMPANY_CODE = RETBL.COMPANY_CODE) RTBL
) PFMTBL
WHERE 1 = 1 {spCode}
GROUP BY PFMTBL.GROUP_EDESC, PFMTBL.SP_CODE, PFMTBL.EMPLOYEE_EDESC
ORDER BY UPPER(PFMTBL.GROUP_EDESC), UPPER(PFMTBL.EMPLOYEE_EDESC)";
            var data = _objectEntity.SqlQuery<TopEffectiveModel>(query).ToList();
            return data;
        }
        public List<DetailTopEffective> GetPresentASMBeat(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join(",", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;
            var sysdate = DateTime.Now.ToString("dd-MMM-yyy");

            //var BranchFilter = string.Empty;
            //var BranchFilter1 = string.Empty;
            //var BranchFilter2 = string.Empty;
            //if (model.BranchFilter.Count > 0)
            //{
            //    BranchFilter = string.Format(@" AND  DLU.BRANCH_CODE IN  ('{0}')", string.Join("','", model.BranchFilter).ToString());
            //    BranchFilter1 = string.Format(@" AND  A.BRANCH_CODE IN  ('{0}')", string.Join("','", model.BranchFilter).ToString());
            //    BranchFilter2 = string.Format(@" AND  BRANCH_CODE IN  ('{0}')", string.Join("','", model.BranchFilter).ToString());
            //}
            var spCode = string.Empty;
            if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                spCode = $"  AND aa.SP_CODE IN  ({userInfo.sp_codes})";
            }
             else if (model.DistEmployeeFilter.Count > 0)
                {
                    spCode = $" AND SP_CODE IN  ('{ string.Join("','", model.DistEmployeeFilter).ToString()}')";
                }


            //            var query = $@"SELECT wm_concat(DISTINCT route_name) route_name
            //	, group_edesc
            //	, sp_code
            //	, employee_edesc
            //	, assign_date
            //	, atn_time
            //	, eod_time
            //	, reseller_count as outlet_added
            //	, working_hours
            //	, SUM(target) target
            //	,SUM(visited) target_visited
            //	,SUM(total_visited) visited
            //	,SUM(extra) extra
            //	,SUM(not_visited) not_visited
            //	,SUM(total_pjp) total_pjp
            //	,SUM(pjp) pjp_productive
            //	,SUM(non_pjp) pjp_non_productive
            //	,SUM(non_n_pjp) npjp_productive
            //	,SUM(total_quantity) pjp_total_quantity
            //	,SUM(total_price) pjp_total_amount
            //	,round((SUM(total_visited) / DECODE(SUM(target), 0, 1, SUM(target)) * 100), 2) percent_effective_calls
            //	,round((SUM(total_pjp) / DECODE(SUM(total_visited), 0, 1, SUM(total_visited)) * 100), 2) percent_productive_calls
            //	,eod_remarks
            //FROM(
            //	SELECT wm_concat(DISTINCT route_name) route_name
            //		, group_edesc
            //		, sp_code
            //		, full_name employee_edesc
            //		, trunc(assign_date) assign_date
            //		, TO_CHAR(atn_time, 'HH:MI:SS A.M.') atn_time
            //		, CASE
            //			WHEN atn_time = eod_time
            //				THEN NULL
            //			ELSE TO_CHAR(eod_time, 'HH:MI:SS A.M.')
            //			END eod_time
            //		,
            //		--TO_CHAR(EOD_TIME, 'HH:MI:SS A.M.') EOD_TIME,
            //		nvl(round(24 * (eod_time - atn_time), 2), 0) working_hours
            //		, nvl(reseller_count, 0) reseller_count
            //		, SUM(target) target
            //		, SUM(visited) visited
            //		, nvl((
            //				SELECT COUNT(DISTINCT customer_code)
            //				FROM dist_visited_entity
            //				WHERE userid = aa.userid
            //					AND company_code = aa.company_code
            //					AND trunc(update_date) = trunc(aa.assign_date)
            //				), 0) total_visited
            //		,SUM(nvl((
            //					SELECT COUNT(DISTINCT customer_code)
            //					FROM dist_visited_entity
            //					WHERE userid = aa.userid
            //						AND company_code = aa.company_code
            //						AND trunc(update_date) = trunc(aa.assign_date)
            //					), 0) - visited) extra
            //		,SUM(target - visited) not_visited
            //		,SUM(pjp) pjp
            //		,SUM(visited - pjp) non_pjp
            //		,SUM(nvl((
            //					SELECT COUNT(DISTINCT reseller_code)
            //					FROM dist_visited_po
            //					WHERE userid = aa.userid
            //						AND company_code = aa.company_code
            //						AND trunc(order_date) = trunc(aa.assign_date)
            //					), 0) - pjp) non_n_pjp
            //		,nvl((
            //				SELECT COUNT(DISTINCT reseller_code)
            //				FROM dist_visited_po
            //				WHERE userid = aa.userid
            //					AND company_code = aa.company_code
            //					AND trunc(order_date) = trunc(aa.assign_date)
            //				), 0) total_pjp
            //		,nvl((
            //				SELECT SUM(quantity)
            //				FROM dist_visited_po
            //				WHERE userid = aa.userid
            //					AND company_code = aa.company_code
            //					AND trunc(order_date) = trunc(aa.assign_date)
            //				), 0) total_quantity
            //		,nvl((
            //				SELECT SUM(total_price)
            //				FROM dist_visited_po
            //				WHERE userid = aa.userid
            //					AND company_code = aa.company_code
            //					AND trunc(order_date) = trunc(aa.assign_date)
            //				), 0) total_price
            //		,eod_remarks
            //	FROM(
            //		SELECT b.route_name route_name
            //			, b.group_edesc
            //			, a.userid
            //			, a.full_name
            //			, a.sp_code
            //			, b.assign_date
            //			, b.company_code
            //			, (
            //				SELECT MIN(submit_date)
            //				FROM dist_lm_location_tracking
            //				WHERE track_type = 'ATN'
            //					AND sp_code = a.sp_code
            //					AND trunc(submit_date) = trunc(b.assign_date)
            //				) atn_time
            //			,(
            //				SELECT TO_DATE(TO_CHAR(MAX(attendance_time), 'DD/MM/YYYY HH:MI:SS AM'), 'DD/MM/YYYY HH:MI:SS AM')
            //				FROM hris_attendance
            //				WHERE employee_id = a.sp_code
            //					AND trunc(attendance_dt) = trunc(b.assign_date)
            //				) eod_time
            //			,(
            //				SELECT count(Reseller_code)
            //				FROM dist_reseller_master
            //				WHERE created_by = a.userid
            //					AND deleted_flag = 'N'
            //				) reseller_count
            //			,(
            //				SELECT remarks
            //				FROM dist_eod_update
            //				WHERE trunc(created_date) = TO_DATE(b.assign_date, 'RRRR-MON-DD')
            //					AND sp_code = a.sp_code
            //					AND ROWNUM = 1
            //				) eod_remarks
            //			,CASE
            //				WHEN wm_concat(b.entity_code) IS NULL
            //					THEN 0
            //				ELSE nvl(COUNT(*), 0)
            //				END target
            //			, nvl((
            //					SELECT COUNT(DISTINCT customer_code)
            //					FROM dist_visited_entity
            //					WHERE userid = a.userid
            //						AND company_code = a.company_code
            //						AND trunc(update_date) = trunc(b.assign_date)
            //						AND customer_code IN(
            //							SELECT entity_code
            //							FROM dist_target_entity
            //							WHERE userid = a.userid
            //								AND company_code = a.company_code
            //								AND route_code = b.route_code
            //								AND trunc(assign_date) = trunc(b.assign_date)
            //							)
            //					), 0) visited
            //			,nvl((
            //					SELECT COUNT(DISTINCT reseller_code)
            //					FROM dist_visited_po
            //					WHERE userid = a.userid
            //						AND company_code = a.company_code
            //						AND trunc(order_date) = trunc(b.assign_date)
            //						AND reseller_code IN(
            //							SELECT entity_code
            //							FROM dist_target_entity
            //							WHERE userid = a.userid
            //								AND company_code = a.company_code
            //								AND route_code = b.route_code
            //								AND trunc(assign_date) = trunc(b.assign_date)
            //							)
            //					), 0) pjp
            //		FROM dist_login_user a
            //			, dist_target_entity b
            //		 WHERE a.userid = b.userid
            //			AND a.company_code = b.company_code
            //			AND a.active = 'Y'
            //			AND a.company_code IN('{companyCode}')
            //			AND b.assign_date LIKE '{sysdate}'-- put sysdate here FOR EOD
            //		   --            BETWEEN TO_DATE('2021-Jul-16', 'RRRR-MON-DD') 	AND TO_DATE('2022-Jul-16', 'RRRR-MON-DD')
            //		GROUP BY a.userid
            //			,a.full_name
            //			,a.sp_code
            //			,b.assign_date
            //			,a.company_code
            //			,b.route_code
            //			,b.route_name
            //			,b.group_edesc
            //			,b.company_code
            //		ORDER BY b.assign_date
            //		) aa
            //	WHERE 1 = 1  {spCode}
            //	GROUP BY userid
            //		,company_code
            //		,trunc(assign_date)
            //		,atn_time
            //		,eod_time
            //		,reseller_count
            //		,sp_code
            //		,group_edesc
            //		,sp_code
            //		,full_name
            //		,eod_remarks
            //	)
            //GROUP BY assign_date
            //	,atn_time
            //	,eod_time
            //	,reseller_count
            //	,sp_code
            //	,group_edesc
            //	,sp_code
            //	,eod_remarks
            //	,employee_edesc
            //	,working_hours
            //ORDER BY sp_code"; 

            var query = $@"SELECT wm_concat(DISTINCT route_name) route_name
	, group_edesc
	, sp_code
	, employee_edesc
	, assign_date
	, atn_time
	, eod_time
	, reseller_count as outlet_added
	, working_hours
	, SUM(target) target
	,SUM(visited) target_visited
    ,SUM(ROUND(visited / DECODE(TARGET, 0, 1, TARGET) * 100, 1)) PERCENT_ACHIEVED_TARGET
    ,SUM(total_visited) visited
	,SUM(extra) extra
	,SUM(not_visited) not_visited
	,SUM(total_pjp) total_pjp
	,SUM(pjp) pjp_productive
	,SUM(non_pjp) pjp_non_productive
	,SUM(non_n_pjp) npjp_productive
	,SUM(total_quantity) pjp_total_quantity
	,SUM(total_price) pjp_total_amount
	,round((SUM(total_visited) / DECODE(SUM(target), 0, 1, SUM(target)) * 100), 2) percent_effective_calls
	,round((SUM(total_pjp) / DECODE(SUM(total_visited), 0, 1, SUM(total_visited)) * 100), 2) percent_productive_calls
	,eod_remarks
FROM(
	SELECT wm_concat(DISTINCT route_name) route_name
		, group_edesc
		, sp_code
		, full_name employee_edesc
		, trunc(assign_date) assign_date
		, TO_CHAR(atn_time, 'HH:MI:SS A.M.') atn_time
		, CASE
			WHEN atn_time = eod_time
				THEN NULL
			ELSE TO_CHAR(eod_time, 'HH:MI:SS A.M.')
			END eod_time
		,
		--TO_CHAR(EOD_TIME, 'HH:MI:SS A.M.') EOD_TIME,
		nvl(round(24 * (eod_time - atn_time), 2), 0) working_hours
		, nvl(reseller_count, 0) reseller_count
		, SUM(target) target
		, SUM(visited) visited
		, nvl((
				SELECT COUNT(DISTINCT customer_code)
				FROM dist_visited_entity
				WHERE userid = aa.userid
					AND company_code = aa.company_code
					AND trunc(update_date) = trunc(aa.assign_date)
				), 0) total_visited
		,SUM(nvl((
					SELECT COUNT(DISTINCT customer_code)
					FROM dist_visited_entity
					WHERE userid = aa.userid
						AND company_code = aa.company_code
						AND trunc(update_date) = trunc(aa.assign_date)
					), 0) - visited) extra
		,SUM(target - visited) not_visited
		,SUM(pjp) pjp
		,SUM(visited - pjp) non_pjp
		,SUM(nvl((
					SELECT COUNT(DISTINCT reseller_code)
					FROM dist_visited_po
					WHERE userid = aa.userid
						AND company_code = aa.company_code
						AND trunc(order_date) = trunc(aa.assign_date)
					), 0) - pjp) non_n_pjp
		,nvl((
				SELECT COUNT(DISTINCT reseller_code)
				FROM dist_visited_po
				WHERE userid = aa.userid
					AND company_code = aa.company_code
					AND trunc(order_date) = trunc(aa.assign_date)
				), 0) total_pjp
		,nvl((
				SELECT SUM(quantity)
				FROM dist_visited_po
				WHERE userid = aa.userid
					AND company_code = aa.company_code
					AND trunc(order_date) = trunc(aa.assign_date)
				), 0) total_quantity
		,nvl((
				SELECT SUM(total_price)
				FROM dist_visited_po
				WHERE userid = aa.userid
					AND company_code = aa.company_code
					AND trunc(order_date) = trunc(aa.assign_date)
				), 0) total_price
		,eod_remarks
	FROM(
		SELECT b.route_name route_name
			, b.group_edesc
			, a.userid
			, a.full_name
			, a.sp_code
			, b.assign_date
			, b.company_code
			, (
				SELECT MIN(submit_date)
				FROM dist_lm_location_tracking
				WHERE track_type = 'ATN'
					AND sp_code = a.sp_code
					AND trunc(submit_date) = trunc(b.assign_date)
				) atn_time
			,(
				SELECT TO_DATE(TO_CHAR(MAX(attendance_time), 'DD/MM/YYYY HH:MI:SS AM'), 'DD/MM/YYYY HH:MI:SS AM')
				FROM hris_attendance
				WHERE employee_id = a.sp_code
					AND trunc(attendance_dt) = trunc(b.assign_date)
				) eod_time
			,(
				SELECT count(Reseller_code)
				FROM dist_reseller_master
				WHERE created_by = a.userid
					AND deleted_flag = 'N'
				) reseller_count
			,(
				SELECT remarks
				FROM dist_eod_update
				WHERE trunc(created_date) = TO_DATE(b.assign_date, 'RRRR-MON-DD')
					AND sp_code = a.sp_code
					AND ROWNUM = 1
				) eod_remarks
			,CASE
				WHEN wm_concat(b.entity_code) IS NULL
					THEN 0
				ELSE nvl(COUNT(*), 0)
				END target
			, nvl((
					SELECT COUNT(DISTINCT customer_code)
					FROM dist_visited_entity
					WHERE userid = a.userid
						AND company_code = a.company_code
						AND trunc(update_date) = trunc(b.assign_date)
						AND customer_code IN(
							SELECT entity_code
							FROM dist_target_entity
							WHERE userid = a.userid
								AND company_code = a.company_code
								AND route_code = b.route_code
								AND trunc(assign_date) = trunc(b.assign_date)
							)
					), 0) visited
			,nvl((
					SELECT COUNT(DISTINCT reseller_code)
					FROM dist_visited_po
					WHERE userid = a.userid
						AND company_code = a.company_code
						AND trunc(order_date) = trunc(b.assign_date)
						AND reseller_code IN(
							SELECT entity_code
							FROM dist_target_entity
							WHERE userid = a.userid
								AND company_code = a.company_code
								AND route_code = b.route_code
								AND trunc(assign_date) = trunc(b.assign_date)
							)
					), 0) pjp
		FROM dist_login_user a
			, dist_target_entity b
		 WHERE a.userid = b.userid
			AND a.company_code = b.company_code
			AND a.active = 'Y'
			AND a.company_code IN ('{companyCode}')
			AND b.assign_date LIKE trunc(sysdate)-- put sysdate here FOR EOD
		   --            BETWEEN TO_DATE('2021-Jul-16', 'RRRR-MON-DD') 	AND TO_DATE('2022-Jul-16', 'RRRR-MON-DD')
		GROUP BY a.userid
			,a.full_name
			,a.sp_code
			,b.assign_date
			,a.company_code
			,b.route_code
			,b.route_name
			,b.group_edesc
			,b.company_code
		ORDER BY b.assign_date
		) aa
	WHERE 1 = 1  {spCode}
	GROUP BY userid
		,company_code
		,trunc(assign_date)
		,atn_time
		,eod_time
		,reseller_count
		,sp_code
		,group_edesc
		,sp_code
		,full_name
		,eod_remarks
	)
GROUP BY assign_date
	,atn_time
	,eod_time
	,reseller_count
	,sp_code
	,group_edesc
	,sp_code
	,eod_remarks
	,employee_edesc
	,working_hours
ORDER BY sp_code";
            var data = _objectEntity.SqlQuery<DetailTopEffective>(query).ToList();
            if(data.Count==0)
            {

                List<DetailTopEffective> lsteff = new List<DetailTopEffective>();
                DetailTopEffective effectiveModel = new DetailTopEffective();
                var query1 = $@"SELECT MIN(TO_CHAR(submit_date, 'HH:MI:SS A.M.')) ATN
				FROM dist_lm_location_tracking
				WHERE track_type = 'ATN'
				 {spCode}
					AND trunc(submit_date) =TRUNC(SYSDATE)";
                DataTable atndt = _objectEntity.SqlQuery(query1);
                if (atndt.Rows.Count != 0)
                {
                    effectiveModel.ATN_TIME = atndt.Rows[0]["ATN"].ToString();
                }
                
                var query2 = $@"SELECT MIN(TO_CHAR(submit_date, 'HH:MI:SS A.M.')) EOD
				FROM dist_lm_location_tracking
				WHERE track_type = 'EOD'
					 {spCode}
					AND trunc(submit_date) =trunc(sysdate)";
                DataTable eoddt = _objectEntity.SqlQuery(query2);
                if (eoddt.Rows.Count != 0)
                {
                    effectiveModel.EOD_TIME = eoddt.Rows[0]["EOD"].ToString();
                }
                

                var query3 = $@"SELECT FULL_NAME FROM DIST_LOGIN_USER 
                    WHERE SP_CODE='{ string.Join("','", model.DistEmployeeFilter).ToString()}'";
                DataTable usrdt = _objectEntity.SqlQuery(query3);
                if (usrdt.Rows.Count != 0)
                {
                    effectiveModel.EMPLOYEE_EDESC = usrdt.Rows[0]["FULL_NAME"].ToString();
                }
                lsteff.Add(effectiveModel);
                return lsteff;
                


            }
            return data;
        }

        public List<DetailTopEffective> GetASMBeat(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join(",", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;
            var dateFilter = $"BETWEEN TO_DATE('{model.FromDate}','RRRR-MON-DD') AND TO_DATE('{model.ToDate}','RRRR-MON-DD')";
           
            var spCode = string.Empty;
            if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                spCode = $" AND aa.SP_CODE IN  ({userInfo.sp_codes})";
            }
            else if (model.DistEmployeeFilter.Count > 0)
            {
                spCode = $" AND SP_CODE IN  ('{ string.Join("','", model.DistEmployeeFilter).ToString()}')";
            }
            var query = $@"SELECT WM_CONCAT(DISTINCT ROUTE_NAME) ROUTE_NAME, GROUP_EDESC,SP_CODE, EMPLOYEE_EDESC, ASSIGN_DATE,ATN_TIME,EOD_TIME,WORKING_HOURS,
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
AND A.COMPANY_CODE IN ('{companyCode}')
AND B.ASSIGN_DATE {dateFilter}
GROUP BY A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE, A.COMPANY_CODE,B.ROUTE_CODE, B.ROUTE_NAME,B.GROUP_EDESC, B.COMPANY_CODE
ORDER BY B.ASSIGN_DATE) AA
WHERE 1=1  
 {spCode} 
 GROUP BY  USERID, COMPANY_CODE,TRUNC(ASSIGN_DATE),  ATN_TIME,EOD_TIME, SP_CODE,GROUP_EDESC,SP_CODE, FULL_NAME,EOD_REMARKS)  group by   ASSIGN_DATE,ATN_TIME,EOD_TIME, SP_CODE,GROUP_EDESC,SP_CODE,EOD_REMARKS,EMPLOYEE_EDESC,WORKING_HOURS  order by sp_code
 "; 
            var data = _objectEntity.SqlQuery<DetailTopEffective>(query).ToList();
            return data;
        }


        public List<TopEffectiveModel> GetALLEmployeeReport(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join("','", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;
            var sp_filter = "";
            if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                sp_filter = $" AND PFMTBL.SP_CODE IN ({userInfo.sp_codes})";
            }

            var query = $@"SELECT PFMTBL.GROUP_EDESC, PFMTBL.SP_CODE, PFMTBL.EMPLOYEE_EDESC,
  SUM(PFMTBL.TARGET) TARGET, SUM(PFMTBL.VISITED) VISITED, SUM(PFMTBL.NOT_VISITED) NOT_VISITED,
  SUM(PFMTBL.PJP_PRODUCTIVE) PJP_PRODUCTIVE,
  SUM(PFMTBL.VISITED) - SUM(PFMTBL.PJP_PRODUCTIVE) PJP_NON_PRODUCTIVE,
  SUM(PFMTBL.NPJP_PRODUCTIVE) NPJP_PRODUCTIVE,
  ROUND(DECODE(SUM(PFMTBL.PJP_PRODUCTIVE), NULL, 0,
        0, 0,
        (DECODE(SUM(PFMTBL.VISITED), 0, 0 , SUM(PFMTBL.PJP_PRODUCTIVE) / SUM(PFMTBL.VISITED))) * 100),2) PERCENT_EFFECTIVE_CALLS,
  SUM(PFMTBL.PJP_TOTAL_QUANTITY) PJP_TOTAL_QUANTITY, SUM(PFMTBL.PJP_TOTAL_AMOUNT) PJP_TOTAL_AMOUNT,
  SUM(PFMTBL.NPJP_TOTAL_QUANTITY) NPJP_TOTAL_QUANTITY, SUM(PFMTBL.NPJP_TOTAL_AMOUNT) NPJP_TOTAL_AMOUNT
FROM (SELECT PTBL.*
      FROM (SELECT PETBL.GROUP_EDESC, PETBL.SP_CODE, PETBL.EMPLOYEE_EDESC, PETBL.COMPANY_CODE, NVL(PETBL.TARGET, 0) TARGET, NVL(PVTBL.VISITED, 0) VISITED, NVL(PNVTBL.NOT_VISITED,0) NOT_VISITED, 
            NVL(PPJPTBL.PJP_PRODUCTIVE,0) PJP_PRODUCTIVE,
            (NVL(PVTBL.VISITED, 0) - NVL(PPJPTBL.PJP_PRODUCTIVE,0)) PJP_NON_PRODUCTIVE,
            NVL(PNPJPTBL.NPJP_PRODUCTIVE,0) NPJP_PRODUCTIVE,
            NVL(DECODE(PPJPTBL.PJP_PRODUCTIVE, NULL, 0,
                                             0, 0,
                                             ROUND((DECODE(PVTBL.VISITED,0,0,PPJPTBL.PJP_PRODUCTIVE / PVTBL.VISITED)) * 100, 2)),0) NET_PERCENT_EFFECTIVE_CALLS,
            NVL(PPJPTBL.TOTAL_QUANTITY,0) PJP_TOTAL_QUANTITY, NVL(PPJPTBL.TOTAL_PRICE,0) PJP_TOTAL_AMOUNT,
            NVL(PNPJPTBL.TOTAL_QUANTITY,0) NPJP_TOTAL_QUANTITY, NVL(PNPJPTBL.TOTAL_PRICE,0) NPJP_TOTAL_AMOUNT
            FROM (SELECT ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) TARGET
                  FROM (
                      SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, TRIM(DGM.GROUP_EDESC) GROUP_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                      FROM DIST_LOGIN_USER DLU
                      INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                      LEFT JOIN DIST_GROUP_MASTER DGM ON DGM.GROUPID = DLU.GROUPID AND DGM.COMPANY_CODE = DGM.COMPANY_CODE
                      LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' 
                                    AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
                      LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                      LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                      WHERE DLU.COMPANY_CODE IN ('{companyCode}')
                      GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), TRIM(DGM.GROUP_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                  ) ENT
                  GROUP BY ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) PETBL -- Party Type Entity Table
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) VISITED
                        FROM (
                            SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' 
                                        AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                            WHERE DLU.COMPANY_CODE IN ('{companyCode}')
                            GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                        ) ENT
                        INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') AND COMPANY_CODE IN ('{companyCode}') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                        WHERE DLT.SP_CODE IS NOT NULL
                        GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) PVTBL -- Party Type Visit Table
                  ON PVTBL.SP_CODE = PETBL.SP_CODE AND PVTBL.COMPANY_CODE = PETBL.COMPANY_CODE
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) NOT_VISITED
                      FROM (
                          SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.PARTY_TYPE_CODE ENTITY_CODE, TRIM(SCS.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                          FROM DIST_LOGIN_USER DLU
                          INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                          LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
                          LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                          LEFT JOIN IP_PARTY_TYPE_CODE SCS ON SCS.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE 
                          WHERE DLU.COMPANY_CODE IN ('{companyCode}') 
                          GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.PARTY_TYPE_CODE, TRIM(SCS.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                      ) ENT
                      LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') AND COMPANY_CODE IN ('{companyCode}') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                      WHERE DLT.SP_CODE IS NULL
                      GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) PNVTBL -- Party Type Not Visited Table
                  ON PNVTBL.SP_CODE = PETBL.SP_CODE AND PNVTBL.COMPANY_CODE = PETBL.COMPANY_CODE
            LEFT JOIN (SELECT PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE, COUNT(PPO.PARTY_TYPE_CODE) PJP_PRODUCTIVE, SUM(PPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(PPO.TOTAL_PRICE) TOTAL_PRICE
                      FROM (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                            WHERE DLU.COMPANY_CODE IN ('{companyCode}')
                            GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                          ) PJPENT
                      LEFT JOIN (SELECT CREATED_BY, PARTY_TYPE_CODE, COMPANY_CODE, COUNT(PARTY_TYPE_CODE) TOTAL_ORDER, SUM(QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE
                                  FROM DIST_IP_SSD_PURCHASE_ORDER
                                  WHERE TRUNC(ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') AND COMPANY_CODE IN ('{companyCode}')
                                  GROUP BY CREATED_BY, PARTY_TYPE_CODE, COMPANY_CODE
                                ) PPO ON PPO.CREATED_BY = PJPENT.USERID AND PPO.PARTY_TYPE_CODE = PJPENT.ENTITY_CODE AND PPO.COMPANY_CODE = PJPENT.COMPANY_CODE
                      GROUP BY PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE
                   ) PPJPTBL -- Party Type PJP Table
                   ON PPJPTBL.SP_CODE = PETBL.SP_CODE AND PPJPTBL.COMPANY_CODE = PETBL.COMPANY_CODE
            LEFT JOIN (SELECT PPO.SP_CODE, PPO.EMPLOYEE_EDESC, PPO.COMPANY_CODE, COUNT(PPO.PARTY_TYPE_CODE) NPJP_PRODUCTIVE, SUM(PPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(PPO.TOTAL_PRICE) TOTAL_PRICE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, A.PARTY_TYPE_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.COMPANY_CODE, COUNT(A.PARTY_TYPE_CODE) TOTAL_ORDER, SUM(A.QUANTITY) TOTAL_QUANTITY, SUM(A.TOTAL_PRICE) TOTAL_PRICE
                            FROM DIST_IP_SSD_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.ACTIVE = 'Y'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN IP_PARTY_TYPE_CODE D ON D.PARTY_TYPE_CODE = A.PARTY_TYPE_CODE AND D.COMPANY_CODE = A.COMPANY_CODE 
                            WHERE TRUNC(A.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') AND A.COMPANY_CODE IN ('{companyCode}')
                            GROUP BY A.CREATED_BY, B.SP_CODE, A.PARTY_TYPE_CODE, TRIM(C.EMPLOYEE_EDESC), A.COMPANY_CODE
                      ) PPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                                  WHERE DLU.COMPANY_CODE IN ('{companyCode}')
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
      FROM (SELECT DETBL.GROUP_EDESC, DETBL.SP_CODE, DETBL.EMPLOYEE_EDESC, DETBL.COMPANY_CODE, NVL(DETBL.TARGET, 0) TARGET, NVL(DVTBL.VISITED, 0) VISITED, NVL(DNVTBL.NOT_VISITED,0) NOT_VISITED, 
            NVL(DPJPTBL.PJP_PRODUCTIVE,0) PJP_PRODUCTIVE,
            (NVL(DVTBL.VISITED, 0) - NVL(DPJPTBL.PJP_PRODUCTIVE,0)) PJP_NON_PRODUCTIVE,
            NVL(DNPJPTBL.NPJP_PRODUCTIVE,0) NPJP_PRODUCTIVE,
            NVL(DECODE(DPJPTBL.PJP_PRODUCTIVE, NULL, 0,
                                             0, 0,
                                             ROUND((DECODE(DVTBL.VISITED,0,0,DPJPTBL.PJP_PRODUCTIVE / DVTBL.VISITED)) * 100,2)),0) NET_PERCENT_EFFECTIVE_CALLS,
            NVL(DPJPTBL.TOTAL_QUANTITY,0) PJP_TOTAL_QUANTITY, NVL(DPJPTBL.TOTAL_PRICE,0) PJP_TOTAL_AMOUNT,
            NVL(DNPJPTBL.TOTAL_QUANTITY,0) NPJP_TOTAL_QUANTITY, NVL(DNPJPTBL.TOTAL_PRICE,0) NPJP_TOTAL_AMOUNT
            FROM (SELECT ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) TARGET
                  FROM (
                      SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, TRIM(DGM.GROUP_EDESC) GROUP_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                      FROM DIST_LOGIN_USER DLU
                      INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                      LEFT JOIN DIST_GROUP_MASTER DGM ON DGM.GROUPID = DLU.GROUPID AND DGM.COMPANY_CODE = DGM.COMPANY_CODE
                      LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
                      LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                      LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                      WHERE DLU.COMPANY_CODE IN ('{companyCode}')
                      GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), TRIM(DGM.GROUP_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                  ) ENT
                  GROUP BY ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) DETBL -- Customer/Distributor Entity Table
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) VISITED
                        FROM (
                            SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                            WHERE DLU.COMPANY_CODE IN ('{companyCode}')
                            GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                        ) ENT
                        INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') AND COMPANY_CODE IN ('{companyCode}') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                        WHERE DLT.SP_CODE IS NOT NULL
                        GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) DVTBL -- Customer/Distributor Visit Table
                  ON DVTBL.SP_CODE = DETBL.SP_CODE AND DVTBL.COMPANY_CODE = DETBL.COMPANY_CODE
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) NOT_VISITED
                      FROM (
                          SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                          FROM DIST_LOGIN_USER DLU
                          INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                          LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
                          LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                          LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE 
                          WHERE DLU.COMPANY_CODE IN ('{companyCode}')
                          GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                      ) ENT
                      LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') AND COMPANY_CODE IN ('{companyCode}') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                      WHERE DLT.SP_CODE IS NULL
                      GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                ) DNVTBL -- Customer/Distributor Not Visited Table
                ON DNVTBL.SP_CODE = DETBL.SP_CODE AND DNVTBL.COMPANY_CODE = DETBL.COMPANY_CODE
            LEFT JOIN (SELECT PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE, COUNT(DPO.CUSTOMER_CODE) PJP_PRODUCTIVE, SUM(DPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(DPO.TOTAL_PRICE) TOTAL_PRICE
                      FROM (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                            WHERE DLU.COMPANY_CODE IN ('{companyCode}')
                            GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                      ) PJPENT
                      LEFT JOIN (SELECT CREATED_BY, CUSTOMER_CODE, COMPANY_CODE, COUNT(CUSTOMER_CODE) TOTAL_ORDER, SUM(QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE
                                  FROM DIST_IP_SSD_PURCHASE_ORDER
                                  WHERE TRUNC(ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') AND COMPANY_CODE IN ('{companyCode}')
                                  GROUP BY CREATED_BY, CUSTOMER_CODE, COMPANY_CODE
                      ) DPO ON DPO.CREATED_BY = PJPENT.USERID AND DPO.CUSTOMER_CODE = PJPENT.ENTITY_CODE AND DPO.COMPANY_CODE = PJPENT.COMPANY_CODE
                      GROUP BY PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE
                   ) DPJPTBL -- Customer/Distributor PJP Table
                   ON DPJPTBL.SP_CODE = DETBL.SP_CODE AND DPJPTBL.COMPANY_CODE = DETBL.COMPANY_CODE
            LEFT JOIN (SELECT DPO.SP_CODE, DPO.EMPLOYEE_EDESC, DPO.COMPANY_CODE, COUNT(DPO.CUSTOMER_CODE) NPJP_PRODUCTIVE, SUM(DPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(DPO.TOTAL_PRICE) TOTAL_PRICE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, A.CUSTOMER_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.COMPANY_CODE, COUNT(A.CUSTOMER_CODE) TOTAL_ORDER, SUM(A.QUANTITY) TOTAL_QUANTITY, SUM(A.TOTAL_PRICE) TOTAL_PRICE
                            FROM DIST_IP_SSD_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.ACTIVE = 'Y'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            WHERE TRUNC(A.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') AND A.COMPANY_CODE IN ('{companyCode}')
                            GROUP BY A.CREATED_BY, B.SP_CODE, A.CUSTOMER_CODE, TRIM(C.EMPLOYEE_EDESC), A.COMPANY_CODE
                      ) DPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                                  WHERE DLU.COMPANY_CODE IN ('{companyCode}')
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
      FROM (SELECT RETBL.GROUP_EDESC, RETBL.SP_CODE, RETBL.EMPLOYEE_EDESC, RETBL.COMPANY_CODE, NVL(RETBL.TARGET, 0) TARGET, NVL(RVTBL.VISITED, 0) VISITED, NVL(RNVTBL.NOT_VISITED,0) NOT_VISITED, 
            NVL(RPJPTBL.PJP_PRODUCTIVE,0) PJP_PRODUCTIVE,
            (NVL(RVTBL.VISITED, 0) - NVL(RPJPTBL.PJP_PRODUCTIVE,0)) PJP_NON_PRODUCTIVE,
            NVL(RNPJPTBL.NPJP_PRODUCTIVE,0) NPJP_PRODUCTIVE,
            NVL(DECODE(RPJPTBL.PJP_PRODUCTIVE, NULL, 0,
                                             0, 0,
                                             ROUND((DECODE(RVTBL.VISITED,0,0,RPJPTBL.PJP_PRODUCTIVE / RVTBL.VISITED)) * 100,2)),0) NET_PERCENT_EFFECTIVE_CALLS,
            NVL(RPJPTBL.TOTAL_QUANTITY,0) PJP_TOTAL_QUANTITY, NVL(RPJPTBL.TOTAL_PRICE,0) PJP_TOTAL_AMOUNT,
            NVL(RNPJPTBL.TOTAL_QUANTITY,0) NPJP_TOTAL_QUANTITY, NVL(RNPJPTBL.TOTAL_PRICE,0) NPJP_TOTAL_AMOUNT
            FROM (SELECT ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) TARGET
                  FROM (
                      SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, TRIM(DGM.GROUP_EDESC) GROUP_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                      FROM DIST_LOGIN_USER DLU
                      INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                      LEFT JOIN DIST_GROUP_MASTER DGM ON DGM.GROUPID = DLU.GROUPID AND DGM.COMPANY_CODE = DGM.COMPANY_CODE
                      LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
                      LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                      LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y' AND DRM.IS_CLOSED = 'N'
                      WHERE DLU.COMPANY_CODE IN ('{companyCode}')
                      GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), TRIM(DGM.GROUP_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                  ) ENT
                  GROUP BY ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) RETBL -- Retailer Entity Table
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) VISITED
                        FROM (
                            SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y' AND DRM.IS_CLOSED = 'N'
                            WHERE DLU.COMPANY_CODE IN ('{companyCode}')
                            GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                        ) ENT
                        INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') AND COMPANY_CODE IN ('{companyCode}') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                        WHERE DLT.SP_CODE IS NOT NULL
                        GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) RVTBL -- Retailer Visit Table
                  ON RVTBL.SP_CODE = RETBL.SP_CODE AND RVTBL.COMPANY_CODE = RETBL.COMPANY_CODE
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) NOT_VISITED
                      FROM (
                          SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                          FROM DIST_LOGIN_USER DLU
                          INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                          LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
                          LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                          LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y' AND DRM.IS_CLOSED = 'N'
                          WHERE DLU.COMPANY_CODE IN ('{companyCode}')
                          GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                      ) ENT
                      LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') AND COMPANY_CODE IN ('{companyCode}') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                      WHERE DLT.SP_CODE IS NULL
                      GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                ) RNVTBL -- Retailer Not Visited Table
                ON RNVTBL.SP_CODE = RETBL.SP_CODE AND RNVTBL.COMPANY_CODE = RETBL.COMPANY_CODE
            LEFT JOIN (SELECT PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE, COUNT(RPO.RESELLER_CODE) PJP_PRODUCTIVE, SUM(RPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(RPO.TOTAL_PRICE) TOTAL_PRICE
                      FROM (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y' AND DRM.IS_CLOSED = 'N'
                            WHERE DLU.COMPANY_CODE IN ('{companyCode}')
                            GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                      ) PJPENT
                      LEFT JOIN (SELECT CREATED_BY, RESELLER_CODE, COMPANY_CODE, COUNT(RESELLER_CODE) TOTAL_ORDER, SUM(QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE
                                  FROM DIST_IP_SSR_PURCHASE_ORDER
                                  WHERE TRUNC(ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') AND COMPANY_CODE IN ('{companyCode}')
                                  GROUP BY CREATED_BY, RESELLER_CODE, COMPANY_CODE
                      ) RPO ON RPO.CREATED_BY = PJPENT.USERID AND RPO.RESELLER_CODE = PJPENT.ENTITY_CODE AND RPO.COMPANY_CODE = PJPENT.COMPANY_CODE
                      GROUP BY PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE
                   ) RPJPTBL -- Retailer PJP Table
                   ON RPJPTBL.SP_CODE = RETBL.SP_CODE AND RPJPTBL.COMPANY_CODE = RETBL.COMPANY_CODE
            LEFT JOIN (SELECT RPO.SP_CODE, RPO.EMPLOYEE_EDESC, RPO.COMPANY_CODE, COUNT(RPO.RESELLER_CODE) NPJP_PRODUCTIVE, SUM(RPO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(RPO.TOTAL_PRICE) TOTAL_PRICE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, A.RESELLER_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.COMPANY_CODE, COUNT(A.RESELLER_CODE) TOTAL_ORDER, SUM(A.QUANTITY) TOTAL_QUANTITY, SUM(A.TOTAL_PRICE) TOTAL_PRICE
                            FROM DIST_IP_SSR_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.ACTIVE = 'Y'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            WHERE TRUNC(A.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') AND A.COMPANY_CODE IN ('{companyCode}')
                            GROUP BY A.CREATED_BY, B.SP_CODE, A.RESELLER_CODE, TRIM(C.EMPLOYEE_EDESC), A.COMPANY_CODE
                      ) RPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y' AND DRM.IS_CLOSED = 'N'
                                  WHERE DLU.COMPANY_CODE IN ('{companyCode}')
                                  GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                      ) NPJPENT ON NPJPENT.USERID = RPO.CREATED_BY AND NPJPENT.SP_CODE = RPO.SP_CODE AND NPJPENT.ENTITY_CODE = RPO.RESELLER_CODE AND NPJPENT.COMPANY_CODE = RPO.COMPANY_CODE
                      WHERE 1 = 1
                        AND NPJPENT.SP_CODE IS NULL
                      GROUP BY RPO.SP_CODE, RPO.EMPLOYEE_EDESC, RPO.COMPANY_CODE
                   ) RNPJPTBL -- Retailer NPJP Table
                   ON RNPJPTBL.SP_CODE = RETBL.SP_CODE AND RNPJPTBL.COMPANY_CODE = RETBL.COMPANY_CODE) RTBL
) PFMTBL
WHERE 1 = 1 {sp_filter}
GROUP BY PFMTBL.GROUP_EDESC, PFMTBL.SP_CODE, PFMTBL.EMPLOYEE_EDESC
ORDER BY UPPER(PFMTBL.GROUP_EDESC), UPPER(PFMTBL.EMPLOYEE_EDESC)";
            var data = _objectEntity.SqlQuery<TopEffectiveModel>(query).ToList();
            return data;
        }

        public List<TopEffectiveModel> GetALLEmployeeReportNew(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join("','", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;
            var sp_filter = "";
            if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                sp_filter = $" AND SP_CODE IN ({userInfo.sp_codes})";
            }

            string query = $@"SELECT GROUP_EDESC,SP_CODE, EMPLOYEE_EDESC,
                    TARGET,VISITED,TOTAL_VISITED,EXTRA,NOT_VISITED,TOTAL_PJP,PJP PJP_PRODUCTIVE,NON_PJP PJP_NON_PRODUCTIVE,
                    NON_N_PJP NPJP_PRODUCTIVE,TOTAL_QUANTITY PJP_TOTAL_QUANTITY,TOTAL_PRICE PJP_TOTAL_AMOUNT,
                    ROUND( (VISITED/DECODE(TARGET,0,1,TARGET)  * 100),2)  PERCENT_EFFECTIVE_CALLS,
                    ROUND( (PJP/DECODE(VISITED,0,1,VISITED) * 100),2)  PERCENT_PRODUCTIVE_CALLS
                    FROM(
                    SELECT GROUP_EDESC,SP_CODE, FULL_NAME EMPLOYEE_EDESC, SUM(TARGET) TARGET,
                    SUM(VISITED) VISITED,SUM(TOTAL_VISITED) TOTAL_VISITED, SUM(TOTAL_VISITED - VISITED) EXTRA, SUM(TARGET- VISITED) NOT_VISITED,
                    SUM(TOTAL_PJP) TOTAL_PJP,SUM(PJP) PJP, SUM(VISITED - PJP)  NON_PJP, SUM(TOTAL_PJP- PJP) NON_N_PJP, SUM(TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE
                    FROM(
                    SELECT B.GROUP_EDESC, A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE,
                    CASE WHEN B.ENTITY_CODE IS NULL THEN 0
                        ELSE NVL(COUNT(*),0)
                        END TARGET
                    ,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE)  = TRUNC(B.ASSIGN_DATE)  AND CUSTOMER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ASSIGN_DATE)  = TRUNC(B.ASSIGN_DATE) )),0) VISITED
                    ,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE)  = TRUNC(B.ASSIGN_DATE)),0) TOTAL_VISITED
                    ,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE)  = TRUNC(B.ASSIGN_DATE) AND RESELLER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ASSIGN_DATE)  = TRUNC(B.ASSIGN_DATE) )),0) PJP
                    ,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE)  = TRUNC(B.ASSIGN_DATE) ),0) TOTAL_PJP
                    ,NVL((SELECT SUM(QUANTITY)  FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE)  = TRUNC(B.ASSIGN_DATE)),0) TOTAL_QUANTITY
                    ,NVL((SELECT SUM(TOTAL_PRICE)  FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE)  = TRUNC(B.ASSIGN_DATE)),0) TOTAL_PRICE
                    FROM DIST_LOGIN_USER A, DIST_TARGET_ENTITY B  
                    WHERE A.USERID = B.USERID
                    AND A.COMPANY_CODE = B.COMPANY_CODE
                    AND A.ACTIVE = 'Y'
                    AND A.COMPANY_CODE IN '{companyCode}'
                    AND B.ASSIGN_DATE BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
                    GROUP BY B.GROUP_EDESC, A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE, A.COMPANY_CODE, B.ENTITY_CODE
                    ORDER BY B.ASSIGN_DATE)
                    WHERE 1=1 {sp_filter}
                    GROUP BY SP_CODE,GROUP_EDESC,SP_CODE, FULL_NAME)";

            var data = _objectEntity.SqlQuery<TopEffectiveModel>(query).ToList();
            return data;
        }

        public List<DetailTopEffective> GetDetailEmployeeReport(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join("','", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;
            var dateFilter = $"BETWEEN TO_DATE('{model.FromDate}','RRRR-MON-DD') AND TO_DATE('{model.ToDate}','RRRR-MON-DD')";
            var sp_filter = "";

            if (model.ItemBrandFilter.Count > 0)
                sp_filter = $" AND SP_CODE IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                sp_filter = $" AND SP_CODE IN ({userInfo.sp_codes})";
            }
            string query = "";
            // if (model.FromDate.Equals(model.ToDate))
            // {
            // this is for bikalp test
            //query = $@"SELECT ROUTE_NAME, GROUP_EDESC,SP_CODE, EMPLOYEE_EDESC, ATN_TIME,EOD_TIME,WORKING_HOURS,
            //   TARGET,VISITED TARGET_VISITED,TOTAL_VISITED VISITED,EXTRA,NOT_VISITED,TOTAL_PJP,PJP PJP_PRODUCTIVE,NON_PJP PJP_NON_PRODUCTIVE,
            //   NON_N_PJP NPJP_PRODUCTIVE,TOTAL_QUANTITY PJP_TOTAL_QUANTITY,TOTAL_PRICE PJP_TOTAL_AMOUNT,
            //   ROUND( (TOTAL_VISITED/DECODE(TARGET,0,1,TARGET)  * 100),2)  PERCENT_EFFECTIVE_CALLS,
            //   ROUND( (TOTAL_PJP/DECODE(TOTAL_VISITED,0,1,TOTAL_VISITED) * 100),2)  PERCENT_PRODUCTIVE_CALLS,
            //   EOD_REMARKS
            //   FROM(
            //   SELECT WM_CONCAT(DISTINCT ROUTE_NAME) ROUTE_NAME,GROUP_EDESC,SP_CODE, FULL_NAME EMPLOYEE_EDESC,
            //   TO_CHAR(ATN_TIME,'HH:MI:SS A.M.') ATN_TIME,
            //   CASE WHEN ATN_TIME = EOD_TIME THEN NULL
            //           ELSE TO_CHAR(EOD_TIME,'HH:MI:SS A.M.')
            //   END EOD_TIME,
            //   --TO_CHAR(EOD_TIME,'HH:MI:SS A.M.') EOD_TIME,
            //   NVL(ROUND(24 * (EOD_TIME - ATN_TIME),2),0) WORKING_HOURS,
            //   SUM(TARGET) TARGET,
            //   SUM(VISITED) VISITED,SUM(TOTAL_VISITED) TOTAL_VISITED, SUM(TOTAL_VISITED - VISITED) EXTRA, SUM(TARGET- VISITED) NOT_VISITED,
            //   SUM(TOTAL_PJP) TOTAL_PJP,SUM(PJP) PJP, SUM(VISITED - PJP)  NON_PJP, SUM(TOTAL_PJP- PJP) NON_N_PJP, SUM(TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE,
            //   EOD_REMARKS
            //   FROM(
            //   SELECT WM_CONCAT(DISTINCT B.ROUTE_NAME) ROUTE_NAME, WM_CONCAT(DISTINCT B.GROUP_EDESC) GROUP_EDESC, A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE
            //   ,(SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='ATN' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) ATN_TIME
            //   --,(SELECT MAX(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='EOD' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) EOD_TIME
            //   ,(SELECT TO_DATE(TO_CHAR(MAX(ATTENDANCE_TIME),'DD/MM/YYYY HH:MI:SS AM'),'DD/MM/YYYY HH:MI:SS AM') FROM HRIS_ATTENDANCE WHERE EMPLOYEE_ID = A.SP_CODE AND TRUNC(ATTENDANCE_DT) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) EOD_TIME
            //   ,(SELECT REMARKS FROM DIST_EOD_UPDATE WHERE TRUNC(CREATED_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD') AND SP_CODE =  A.SP_CODE AND ROWNUM = 1) EOD_REMARKS
            //   ,CASE WHEN WM_CONCAT(B.ENTITY_CODE) IS NULL THEN 0
            //       ELSE NVL(COUNT(*),0)
            //       END TARGET
            //   ,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE) {dateFilter} AND CUSTOMER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ASSIGN_DATE) {dateFilter})),0) VISITED
            //   ,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE) {dateFilter}),0) TOTAL_VISITED
            //   ,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) {dateFilter} AND RESELLER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ASSIGN_DATE) {dateFilter})),0) PJP
            //   ,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) {dateFilter}),0) TOTAL_PJP
            //   ,NVL((SELECT SUM(QUANTITY)  FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) {dateFilter}),0) TOTAL_QUANTITY
            //   ,NVL((SELECT SUM(TOTAL_PRICE)  FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) {dateFilter}),0) TOTAL_PRICE
            //   FROM DIST_LOGIN_USER A, DIST_TARGET_ENTITY B  
            //   WHERE A.USERID = B.USERID
            //   AND A.COMPANY_CODE = B.COMPANY_CODE
            //   AND A.ACTIVE = 'Y'
            //   AND A.COMPANY_CODE IN ('{companyCode}')
            //   AND B.ASSIGN_DATE {dateFilter}
            //   GROUP BY A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE, A.COMPANY_CODE
            //   ORDER BY B.ASSIGN_DATE)
            //   WHERE 1=1 {sp_filter}
            //   GROUP BY ATN_TIME,EOD_TIME, SP_CODE,GROUP_EDESC,SP_CODE, FULL_NAME,EOD_REMARKS)";
            // }
            // else
            // {
            //query = $@"SELECT ROUTE_NAME, GROUP_EDESC,SP_CODE, EMPLOYEE_EDESC, ATN_TIME,EOD_TIME,WORKING_HOURS,
            //        TARGET,VISITED TARGET_VISITED,TOTAL_VISITED VISITED,EXTRA,NOT_VISITED,TOTAL_PJP,PJP PJP_PRODUCTIVE,NON_PJP PJP_NON_PRODUCTIVE,
            //        NON_N_PJP NPJP_PRODUCTIVE,TOTAL_QUANTITY PJP_TOTAL_QUANTITY,TOTAL_PRICE PJP_TOTAL_AMOUNT,
            //        ROUND( (TOTAL_VISITED/DECODE(TARGET,0,1,TARGET)  * 100),2)  PERCENT_EFFECTIVE_CALLS,
            //        ROUND( (TOTAL_PJP/DECODE(TOTAL_VISITED,0,1,TOTAL_VISITED) * 100),2)  PERCENT_PRODUCTIVE_CALLS,
            //        EOD_REMARKS
            //        FROM(
            //        SELECT WM_CONCAT(DISTINCT ROUTE_NAME) ROUTE_NAME,GROUP_EDESC,SP_CODE, FULL_NAME EMPLOYEE_EDESC,
            //        TO_CHAR(ATN_TIME,'HH:MI:SS A.M.') ATN_TIME,
            //        CASE WHEN ATN_TIME = EOD_TIME THEN NULL
            //                ELSE TO_CHAR(EOD_TIME,'HH:MI:SS A.M.')
            //        END EOD_TIME,
            //        --TO_CHAR(EOD_TIME,'HH:MI:SS A.M.') EOD_TIME,
            //        NVL(ROUND(24 * (EOD_TIME - ATN_TIME),2),0) WORKING_HOURS,
            //        SUM(TARGET) TARGET,
            //        SUM(VISITED) VISITED,SUM(TOTAL_VISITED) TOTAL_VISITED, SUM(TOTAL_VISITED - VISITED) EXTRA, SUM(TARGET- VISITED) NOT_VISITED,
            //        SUM(TOTAL_PJP) TOTAL_PJP,SUM(PJP) PJP, SUM(VISITED - PJP)  NON_PJP, SUM(TOTAL_PJP- PJP) NON_N_PJP, SUM(TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE,
            //        EOD_REMARKS
            //        FROM(
            //        SELECT  B.ROUTE_NAME ROUTE_NAME,  B.GROUP_EDESC, A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE
            //        ,(SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='ATN' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) ATN_TIME
            //        --,(SELECT MAX(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='EOD' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) EOD_TIME
            //        ,(SELECT TO_DATE(TO_CHAR(MAX(ATTENDANCE_TIME),'DD/MM/YYYY HH:MI:SS AM'),'DD/MM/YYYY HH:MI:SS AM') FROM HRIS_ATTENDANCE WHERE EMPLOYEE_ID = A.SP_CODE AND TRUNC(ATTENDANCE_DT) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) EOD_TIME
            //        ,(SELECT REMARKS FROM DIST_EOD_UPDATE WHERE TRUNC(CREATED_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD') AND SP_CODE =  A.SP_CODE AND ROWNUM = 1) EOD_REMARKS
            //        ,CASE WHEN WM_CONCAT(B.ENTITY_CODE) IS NULL THEN 0
            //            ELSE NVL(COUNT(*),0)
            //            END TARGET
            //        ,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE) = B.ASSIGN_DATE AND CUSTOMER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ASSIGN_DATE) = B.ASSIGN_DATE )),0) VISITED
            //        ,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE)= B.ASSIGN_DATE),0) TOTAL_VISITED
            //        ,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) = B.ASSIGN_DATE AND RESELLER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ASSIGN_DATE) = B.ASSIGN_DATE)),0) PJP
            //        ,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) = B.ASSIGN_DATE ),0) TOTAL_PJP
            //        ,NVL((SELECT SUM(QUANTITY)  FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) = B.ASSIGN_DATE),0) TOTAL_QUANTITY
            //        ,NVL((SELECT SUM(TOTAL_PRICE)  FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) =B.ASSIGN_DATE),0) TOTAL_PRICE
            //        FROM DIST_LOGIN_USER A, DIST_TARGET_ENTITY B  
            //        WHERE A.USERID = B.USERID
            //        AND A.COMPANY_CODE = B.COMPANY_CODE
            //        AND A.ACTIVE = 'Y'
            //        AND A.COMPANY_CODE IN ('{companyCode}')
            //        AND B.ASSIGN_DATE {dateFilter}
            //        GROUP BY A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE, A.COMPANY_CODE,B.ROUTE_NAME,B.GROUP_EDESC
            //        ORDER BY B.ASSIGN_DATE)
            //        WHERE 1=1 {sp_filter}
            //        GROUP BY ATN_TIME,EOD_TIME, SP_CODE,GROUP_EDESC,SP_CODE, FULL_NAME,EOD_REMARKS)";

            query = $@"SELECT WM_CONCAT(DISTINCT ROUTE_NAME) ROUTE_NAME, GROUP_EDESC,SP_CODE, EMPLOYEE_EDESC, ATN_TIME,EOD_TIME,WORKING_HOURS,
sum(TARGET) TARGET,sum(VISITED) TARGET_VISITED,sum(TOTAL_VISITED) VISITED,sum(EXTRA) EXTRA,sum(NOT_VISITED) NOT_VISITED,sum(TOTAL_PJP) TOTAL_PJP,sum(PJP) PJP_PRODUCTIVE,sum(NON_PJP) PJP_NON_PRODUCTIVE,
sum(NON_N_PJP) NPJP_PRODUCTIVE,sum(TOTAL_QUANTITY) PJP_TOTAL_QUANTITY,sum(TOTAL_PRICE) PJP_TOTAL_AMOUNT,
ROUND( (sum(TOTAL_VISITED)/DECODE(sum(TARGET),0,1,sum(TARGET))  * 100),2)  PERCENT_EFFECTIVE_CALLS,
ROUND( (sum(TOTAL_PJP)/DECODE(sum(TOTAL_VISITED),0,1,sum(TOTAL_VISITED)) * 100),2)  PERCENT_PRODUCTIVE_CALLS,
EOD_REMARKS
FROM(
SELECT WM_CONCAT(DISTINCT ROUTE_NAME) ROUTE_NAME,GROUP_EDESC,SP_CODE, FULL_NAME EMPLOYEE_EDESC,
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
,(SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='ATN' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) ATN_TIME
,(SELECT TO_DATE(TO_CHAR(MAX(ATTENDANCE_TIME),'DD/MM/YYYY HH:MI:SS AM'),'DD/MM/YYYY HH:MI:SS AM') FROM HRIS_ATTENDANCE WHERE EMPLOYEE_ID = A.SP_CODE AND TRUNC(ATTENDANCE_DT) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) EOD_TIME
,(SELECT REMARKS FROM DIST_EOD_UPDATE WHERE TRUNC(CREATED_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD') AND SP_CODE =  A.SP_CODE AND ROWNUM = 1) EOD_REMARKS
,CASE WHEN WM_CONCAT(B.ENTITY_CODE) IS NULL THEN 0
ELSE NVL(COUNT(*),0)
END TARGET
,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE) = B.ASSIGN_DATE AND CUSTOMER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE  AND ROUTE_CODE = B.ROUTE_CODE AND TRUNC(ASSIGN_DATE) = B.ASSIGN_DATE )),0) VISITED
,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) = B.ASSIGN_DATE AND RESELLER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND ROUTE_CODE = B.ROUTE_CODE AND TRUNC(ASSIGN_DATE) = B.ASSIGN_DATE)),0) PJP
FROM DIST_LOGIN_USER A, DIST_TARGET_ENTITY B  
WHERE A.USERID = B.USERID
AND A.COMPANY_CODE = B.COMPANY_CODE
AND A.ACTIVE = 'Y'
AND A.COMPANY_CODE IN ('{companyCode}')
AND B.ASSIGN_DATE {dateFilter}
GROUP BY A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE, A.COMPANY_CODE,B.ROUTE_CODE, B.ROUTE_NAME,B.GROUP_EDESC, B.COMPANY_CODE
ORDER BY B.ASSIGN_DATE) AA
WHERE 1=1  
{sp_filter}
GROUP BY  USERID, COMPANY_CODE,TRUNC(ASSIGN_DATE),  ATN_TIME,EOD_TIME, SP_CODE,GROUP_EDESC,SP_CODE, FULL_NAME,EOD_REMARKS)  group by   ATN_TIME,EOD_TIME, SP_CODE,GROUP_EDESC,SP_CODE,EOD_REMARKS,EMPLOYEE_EDESC,WORKING_HOURS  order by sp_code";
            // }
            var data = _objectEntity.SqlQuery<DetailTopEffective>(query).ToList();
            for (int i = 0; i < data.Count; i++)
            {
                var brandQuery = $@"SELECT PO.BRAND_NAME EDESC,SUM(PO.TOTAL_QUANTITY) QUANTITY, SUM(PO.TOTAL_AMOUNT) TOTAL_AMOUNT FROM
                (SELECT DPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME) BRAND_NAME,
                CASE DPO.MU_CODE
                    WHEN IUS.MU_CODE THEN ROUND(NVL(SUM(DPO.QUANTITY/IUS.CONVERSION_FACTOR),0),2)
                    ELSE NVL(SUM(DPO.QUANTITY),0)
                END TOTAL_QUANTITY,
               SUM(DPO.TOTAL_PRICE) TOTAL_AMOUNT,DPO.MU_CODE
               FROM DIST_IP_SSD_PURCHASE_ORDER DPO
               LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON DPO.ITEM_CODE = IUS.ITEM_CODE AND DPO.COMPANY_CODE = IUS.COMPANY_CODE
               INNER JOIN DIST_LOGIN_USER DLU ON DLU.USERID = DPO.CREATED_BY AND DLU.COMPANY_CODE = DPO.COMPANY_CODE AND DLU.SP_CODE = '{data[i].SP_CODE}'
               INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DPO.COMPANY_CODE
               INNER JOIN DIST_DISTRIBUTOR_MASTER D ON D.DISTRIBUTOR_CODE = DPO.CUSTOMER_CODE AND D.COMPANY_CODE = DPO.COMPANY_CODE AND D.ACTIVE = 'Y'
               INNER JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = D.DISTRIBUTOR_CODE AND SCS.COMPANY_CODE = D.COMPANY_CODE
               LEFT JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = DPO.ITEM_CODE AND ISS.COMPANY_CODE = DPO.COMPANY_CODE
               WHERE TRUNC(DPO.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
               AND ISS.DELETED_FLAG = 'N'
               AND DPO.COMPANY_CODE IN ('{companyCode}') AND
                TRIM(ISS.BRAND_NAME) IS NOT NULL
               GROUP BY DPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DPO.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME),DPO.MU_CODE,IUS.MU_CODE
                   UNION ALL
               SELECT RPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, RPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME) BRAND_NAME,
               CASE RPO.MU_CODE
                    WHEN IUS.MU_CODE THEN ROUND(NVL(SUM(RPO.QUANTITY/IUS.CONVERSION_FACTOR),0),2)
                    ELSE NVL(SUM(RPO.QUANTITY),0)
                END TOTAL_QUANTITY,
                SUM(RPO.TOTAL_PRICE) TOTAL_AMOUNT,RPO.MU_CODE
               FROM DIST_IP_SSR_PURCHASE_ORDER RPO
               INNER JOIN IP_ITEM_UNIT_SETUP IUS ON RPO.ITEM_CODE = IUS.ITEM_CODE AND RPO.COMPANY_CODE = IUS.COMPANY_CODE
               INNER JOIN DIST_LOGIN_USER DLU ON DLU.USERID = RPO.CREATED_BY AND DLU.COMPANY_CODE = RPO.COMPANY_CODE AND DLU.SP_CODE = '{data[i].SP_CODE}'
               INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = RPO.COMPANY_CODE
               INNER JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = RPO.RESELLER_CODE AND DRM.COMPANY_CODE = RPO.COMPANY_CODE
               LEFT JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = RPO.ITEM_CODE AND ISS.COMPANY_CODE = RPO.COMPANY_CODE
               WHERE TRUNC(RPO.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
               AND DRM.IS_CLOSED = 'N'
               AND ISS.DELETED_FLAG = 'N'
               AND RPO.COMPANY_CODE IN ('{companyCode}')
                AND TRIM(ISS.BRAND_NAME) IS NOT NULL
                 GROUP BY RPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), RPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME),RPO.MU_CODE,IUS.MU_CODE
                ) PO
                GROUP BY PO.BRAND_NAME";

                var typeQuery = $@"SELECT DOT.TYPE_EDESC EDESC,COUNT(DISTINCT RPO.CUSTOMER_CODE) QUANTITY
               FROM DIST_LOCATION_TRACK RPO
               INNER JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = RPO.CUSTOMER_CODE AND DRM.COMPANY_CODE = RPO.COMPANY_CODE
               INNER JOIN DIST_OUTLET_TYPE DOT ON DOT.TYPE_ID = DRM.OUTLET_TYPE_ID AND DOT.COMPANY_CODE = DRM.COMPANY_CODE
               WHERE TRUNC(RPO.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
               AND RPO.COMPANY_CODE IN ('{companyCode}')
               AND RPO.SP_CODE = '{data[i].SP_CODE}'
               AND DRM.IS_CLOSED = 'N'
               AND DOT.DELETED_FLAG = 'N'
               GROUP BY DOT.TYPE_EDESC
               ORDER BY DOT.TYPE_EDESC";

                var subTypeQuery = $@"SELECT DOT.SUBTYPE_EDESC EDESC,COUNT(DISTINCT RPO.CUSTOMER_CODE) QUANTITY
               FROM DIST_LOCATION_TRACK RPO
               INNER JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = RPO.CUSTOMER_CODE AND DRM.COMPANY_CODE = RPO.COMPANY_CODE
               INNER JOIN DIST_OUTLET_SUBTYPE DOT ON DOT.SUBTYPE_ID = DRM.OUTLET_SUBTYPE_ID AND DOT.COMPANY_CODE = DRM.COMPANY_CODE
               WHERE TRUNC(RPO.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
               AND RPO.COMPANY_CODE IN ('{companyCode}')
               AND RPO.SP_CODE = '{data[i].SP_CODE}'
               AND DOT.DELETED_FLAG = 'N'
               AND DRM.IS_CLOSED = 'N'
               GROUP BY DOT.SUBTYPE_EDESC
               ORDER BY DOT.SUBTYPE_EDESC";

                data[i].BRANDWISE = _objectEntity.SqlQuery<PairModel>(brandQuery).ToList();
                data[i].OUTLET_TYPE = _objectEntity.SqlQuery<PairModel>(typeQuery).ToList();
                data[i].OUTLET_SUB_TYPE = _objectEntity.SqlQuery<PairModel>(subTypeQuery).ToList();
            }
            return data;
        }

        public List<DetailTopEffective> GetDetailEmployeeReportDetail(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join("','", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;
            var dateFilter = $"BETWEEN TO_DATE('{model.FromDate}','RRRR-MON-DD') AND TO_DATE('{model.ToDate}','RRRR-MON-DD')";
            var sp_filter = "";

            if (model.ItemBrandFilter.Count > 0)
                sp_filter = $" AND SP_CODE IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                sp_filter = $" AND SP_CODE IN ({userInfo.sp_codes})";
            }
            string query = "";
            // if (model.FromDate.Equals(model.ToDate))
            // {
            // this is for bikalp test
            //query = $@"SELECT ROUTE_NAME, GROUP_EDESC,SP_CODE, EMPLOYEE_EDESC, ATN_TIME,EOD_TIME,WORKING_HOURS,
            //   TARGET,VISITED TARGET_VISITED,TOTAL_VISITED VISITED,EXTRA,NOT_VISITED,TOTAL_PJP,PJP PJP_PRODUCTIVE,NON_PJP PJP_NON_PRODUCTIVE,
            //   NON_N_PJP NPJP_PRODUCTIVE,TOTAL_QUANTITY PJP_TOTAL_QUANTITY,TOTAL_PRICE PJP_TOTAL_AMOUNT,
            //   ROUND( (TOTAL_VISITED/DECODE(TARGET,0,1,TARGET)  * 100),2)  PERCENT_EFFECTIVE_CALLS,
            //   ROUND( (TOTAL_PJP/DECODE(TOTAL_VISITED,0,1,TOTAL_VISITED) * 100),2)  PERCENT_PRODUCTIVE_CALLS,
            //   EOD_REMARKS
            //   FROM(
            //   SELECT WM_CONCAT(DISTINCT ROUTE_NAME) ROUTE_NAME,GROUP_EDESC,SP_CODE, FULL_NAME EMPLOYEE_EDESC,
            //   TO_CHAR(ATN_TIME,'HH:MI:SS A.M.') ATN_TIME,
            //   CASE WHEN ATN_TIME = EOD_TIME THEN NULL
            //           ELSE TO_CHAR(EOD_TIME,'HH:MI:SS A.M.')
            //   END EOD_TIME,
            //   --TO_CHAR(EOD_TIME,'HH:MI:SS A.M.') EOD_TIME,
            //   NVL(ROUND(24 * (EOD_TIME - ATN_TIME),2),0) WORKING_HOURS,
            //   SUM(TARGET) TARGET,
            //   SUM(VISITED) VISITED,SUM(TOTAL_VISITED) TOTAL_VISITED, SUM(TOTAL_VISITED - VISITED) EXTRA, SUM(TARGET- VISITED) NOT_VISITED,
            //   SUM(TOTAL_PJP) TOTAL_PJP,SUM(PJP) PJP, SUM(VISITED - PJP)  NON_PJP, SUM(TOTAL_PJP- PJP) NON_N_PJP, SUM(TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE,
            //   EOD_REMARKS
            //   FROM(
            //   SELECT WM_CONCAT(DISTINCT B.ROUTE_NAME) ROUTE_NAME, WM_CONCAT(DISTINCT B.GROUP_EDESC) GROUP_EDESC, A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE
            //   ,(SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='ATN' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) ATN_TIME
            //   --,(SELECT MAX(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='EOD' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) EOD_TIME
            //   ,(SELECT TO_DATE(TO_CHAR(MAX(ATTENDANCE_TIME),'DD/MM/YYYY HH:MI:SS AM'),'DD/MM/YYYY HH:MI:SS AM') FROM HRIS_ATTENDANCE WHERE EMPLOYEE_ID = A.SP_CODE AND TRUNC(ATTENDANCE_DT) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) EOD_TIME
            //   ,(SELECT REMARKS FROM DIST_EOD_UPDATE WHERE TRUNC(CREATED_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD') AND SP_CODE =  A.SP_CODE AND ROWNUM = 1) EOD_REMARKS
            //   ,CASE WHEN WM_CONCAT(B.ENTITY_CODE) IS NULL THEN 0
            //       ELSE NVL(COUNT(*),0)
            //       END TARGET
            //   ,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE) {dateFilter} AND CUSTOMER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ASSIGN_DATE) {dateFilter})),0) VISITED
            //   ,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE) {dateFilter}),0) TOTAL_VISITED
            //   ,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) {dateFilter} AND RESELLER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ASSIGN_DATE) {dateFilter})),0) PJP
            //   ,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) {dateFilter}),0) TOTAL_PJP
            //   ,NVL((SELECT SUM(QUANTITY)  FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) {dateFilter}),0) TOTAL_QUANTITY
            //   ,NVL((SELECT SUM(TOTAL_PRICE)  FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) {dateFilter}),0) TOTAL_PRICE
            //   FROM DIST_LOGIN_USER A, DIST_TARGET_ENTITY B  
            //   WHERE A.USERID = B.USERID
            //   AND A.COMPANY_CODE = B.COMPANY_CODE
            //   AND A.ACTIVE = 'Y'
            //   AND A.COMPANY_CODE IN ('{companyCode}')
            //   AND B.ASSIGN_DATE {dateFilter}
            //   GROUP BY A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE, A.COMPANY_CODE
            //   ORDER BY B.ASSIGN_DATE)
            //   WHERE 1=1 {sp_filter}
            //   GROUP BY ATN_TIME,EOD_TIME, SP_CODE,GROUP_EDESC,SP_CODE, FULL_NAME,EOD_REMARKS)";
            // }
            // else
            // {
            //query = $@"SELECT ROUTE_NAME, GROUP_EDESC,SP_CODE, EMPLOYEE_EDESC, ATN_TIME,EOD_TIME,WORKING_HOURS,
            //        TARGET,VISITED TARGET_VISITED,TOTAL_VISITED VISITED,EXTRA,NOT_VISITED,TOTAL_PJP,PJP PJP_PRODUCTIVE,NON_PJP PJP_NON_PRODUCTIVE,
            //        NON_N_PJP NPJP_PRODUCTIVE,TOTAL_QUANTITY PJP_TOTAL_QUANTITY,TOTAL_PRICE PJP_TOTAL_AMOUNT,
            //        ROUND( (TOTAL_VISITED/DECODE(TARGET,0,1,TARGET)  * 100),2)  PERCENT_EFFECTIVE_CALLS,
            //        ROUND( (TOTAL_PJP/DECODE(TOTAL_VISITED,0,1,TOTAL_VISITED) * 100),2)  PERCENT_PRODUCTIVE_CALLS,
            //        EOD_REMARKS
            //        FROM(
            //        SELECT WM_CONCAT(DISTINCT ROUTE_NAME) ROUTE_NAME,GROUP_EDESC,SP_CODE, FULL_NAME EMPLOYEE_EDESC,
            //        TO_CHAR(ATN_TIME,'HH:MI:SS A.M.') ATN_TIME,
            //        CASE WHEN ATN_TIME = EOD_TIME THEN NULL
            //                ELSE TO_CHAR(EOD_TIME,'HH:MI:SS A.M.')
            //        END EOD_TIME,
            //        --TO_CHAR(EOD_TIME,'HH:MI:SS A.M.') EOD_TIME,
            //        NVL(ROUND(24 * (EOD_TIME - ATN_TIME),2),0) WORKING_HOURS,
            //        SUM(TARGET) TARGET,
            //        SUM(VISITED) VISITED,SUM(TOTAL_VISITED) TOTAL_VISITED, SUM(TOTAL_VISITED - VISITED) EXTRA, SUM(TARGET- VISITED) NOT_VISITED,
            //        SUM(TOTAL_PJP) TOTAL_PJP,SUM(PJP) PJP, SUM(VISITED - PJP)  NON_PJP, SUM(TOTAL_PJP- PJP) NON_N_PJP, SUM(TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE,
            //        EOD_REMARKS
            //        FROM(
            //        SELECT  B.ROUTE_NAME ROUTE_NAME,  B.GROUP_EDESC, A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE
            //        ,(SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='ATN' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) ATN_TIME
            //        --,(SELECT MAX(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='EOD' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) EOD_TIME
            //        ,(SELECT TO_DATE(TO_CHAR(MAX(ATTENDANCE_TIME),'DD/MM/YYYY HH:MI:SS AM'),'DD/MM/YYYY HH:MI:SS AM') FROM HRIS_ATTENDANCE WHERE EMPLOYEE_ID = A.SP_CODE AND TRUNC(ATTENDANCE_DT) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) EOD_TIME
            //        ,(SELECT REMARKS FROM DIST_EOD_UPDATE WHERE TRUNC(CREATED_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD') AND SP_CODE =  A.SP_CODE AND ROWNUM = 1) EOD_REMARKS
            //        ,CASE WHEN WM_CONCAT(B.ENTITY_CODE) IS NULL THEN 0
            //            ELSE NVL(COUNT(*),0)
            //            END TARGET
            //        ,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE) = B.ASSIGN_DATE AND CUSTOMER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ASSIGN_DATE) = B.ASSIGN_DATE )),0) VISITED
            //        ,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE)= B.ASSIGN_DATE),0) TOTAL_VISITED
            //        ,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) = B.ASSIGN_DATE AND RESELLER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ASSIGN_DATE) = B.ASSIGN_DATE)),0) PJP
            //        ,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) = B.ASSIGN_DATE ),0) TOTAL_PJP
            //        ,NVL((SELECT SUM(QUANTITY)  FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) = B.ASSIGN_DATE),0) TOTAL_QUANTITY
            //        ,NVL((SELECT SUM(TOTAL_PRICE)  FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) =B.ASSIGN_DATE),0) TOTAL_PRICE
            //        FROM DIST_LOGIN_USER A, DIST_TARGET_ENTITY B  
            //        WHERE A.USERID = B.USERID
            //        AND A.COMPANY_CODE = B.COMPANY_CODE
            //        AND A.ACTIVE = 'Y'
            //        AND A.COMPANY_CODE IN ('{companyCode}')
            //        AND B.ASSIGN_DATE {dateFilter}
            //        GROUP BY A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE, A.COMPANY_CODE,B.ROUTE_NAME,B.GROUP_EDESC
            //        ORDER BY B.ASSIGN_DATE)
            //        WHERE 1=1 {sp_filter}
            //        GROUP BY ATN_TIME,EOD_TIME, SP_CODE,GROUP_EDESC,SP_CODE, FULL_NAME,EOD_REMARKS)";

            query = $@"SELECT WM_CONCAT(DISTINCT ROUTE_NAME) ROUTE_NAME, GROUP_EDESC,SP_CODE, EMPLOYEE_EDESC, ATN_TIME,EOD_TIME,WORKING_HOURS,
sum(TARGET) TARGET,sum(VISITED) TARGET_VISITED,sum(TOTAL_VISITED) VISITED,sum(EXTRA) EXTRA,sum(NOT_VISITED) NOT_VISITED,sum(TOTAL_PJP) TOTAL_PJP,sum(PJP) PJP_PRODUCTIVE,sum(NON_PJP) PJP_NON_PRODUCTIVE,
sum(NON_N_PJP) NPJP_PRODUCTIVE,sum(TOTAL_QUANTITY) PJP_TOTAL_QUANTITY,sum(TOTAL_PRICE) PJP_TOTAL_AMOUNT,
ROUND( (sum(TOTAL_VISITED)/DECODE(sum(TARGET),0,1,sum(TARGET))  * 100),2)  PERCENT_EFFECTIVE_CALLS,
ROUND( (sum(TOTAL_PJP)/DECODE(sum(TOTAL_VISITED),0,1,sum(TOTAL_VISITED)) * 100),2)  PERCENT_PRODUCTIVE_CALLS,
EOD_REMARKS
FROM(
SELECT WM_CONCAT(DISTINCT ROUTE_NAME) ROUTE_NAME,GROUP_EDESC,SP_CODE, FULL_NAME EMPLOYEE_EDESC,
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
,(SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='ATN' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) ATN_TIME
,(SELECT TO_DATE(TO_CHAR(MAX(ATTENDANCE_TIME),'DD/MM/YYYY HH:MI:SS AM'),'DD/MM/YYYY HH:MI:SS AM') FROM HRIS_ATTENDANCE WHERE EMPLOYEE_ID = A.SP_CODE AND TRUNC(ATTENDANCE_DT) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) EOD_TIME
,(SELECT REMARKS FROM DIST_EOD_UPDATE WHERE TRUNC(CREATED_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD') AND SP_CODE =  A.SP_CODE AND ROWNUM = 1) EOD_REMARKS
,CASE WHEN WM_CONCAT(B.ENTITY_CODE) IS NULL THEN 0
ELSE NVL(COUNT(*),0)
END TARGET
,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE) = B.ASSIGN_DATE AND CUSTOMER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE  AND ROUTE_CODE = B.ROUTE_CODE AND TRUNC(ASSIGN_DATE) = B.ASSIGN_DATE )),0) VISITED
,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) = B.ASSIGN_DATE AND RESELLER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND ROUTE_CODE = B.ROUTE_CODE AND TRUNC(ASSIGN_DATE) = B.ASSIGN_DATE)),0) PJP
FROM DIST_LOGIN_USER A, DIST_TARGET_ENTITY B  
WHERE A.USERID = B.USERID
AND A.COMPANY_CODE = B.COMPANY_CODE
AND A.ACTIVE = 'Y'
AND A.COMPANY_CODE IN ('{companyCode}')
AND B.ASSIGN_DATE {dateFilter}
GROUP BY A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE, A.COMPANY_CODE,B.ROUTE_CODE, B.ROUTE_NAME,B.GROUP_EDESC, B.COMPANY_CODE
ORDER BY B.ASSIGN_DATE) AA
WHERE 1=1  
{sp_filter}
GROUP BY  USERID, COMPANY_CODE,TRUNC(ASSIGN_DATE),  ATN_TIME,EOD_TIME, SP_CODE,GROUP_EDESC,SP_CODE, FULL_NAME,EOD_REMARKS)  group by   ATN_TIME,EOD_TIME, SP_CODE,GROUP_EDESC,SP_CODE,EOD_REMARKS,EMPLOYEE_EDESC,WORKING_HOURS  order by sp_code";
            // }
            var data = _objectEntity.SqlQuery<DetailTopEffective>(query).ToList();
            //for (int i = 0; i < data.Count; i++)
            //{


            //    data[i].BRANDWISE = new List<PairModel>();
            //    data[i].OUTLET_TYPE = new List<PairModel>();
            //    data[i].OUTLET_SUB_TYPE = new List<PairModel>();
            //}
            return data;
        }

        public List<DetailTopEffective> GetDetailEmpReportIndivisual(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join("','", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;
            var dateFilter = $"BETWEEN TO_DATE('{model.FromDate}','RRRR-MON-DD') AND TO_DATE('{model.ToDate}','RRRR-MON-DD')";
            var sp_filter = "";

            if (model.ItemBrandFilter.Count > 0)
                sp_filter = $" AND SP_CODE IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            //else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            //{
            //    sp_filter = $" AND SP_CODE IN ({userInfo.sp_codes})";
            //}

            if (model.DistEmployeeFilter.Count <= 0)
            {
                var Emptydata = new List<DetailTopEffective>();
                return Emptydata;
            }
            sp_filter = $" AND SP_CODE IN  ('{ string.Join("','", model.DistEmployeeFilter).ToString()}')";
            string query = "";
            // if (model.FromDate.Equals(model.ToDate))
            // {
            // this is for bikalp test
            //query = $@"SELECT ROUTE_NAME, GROUP_EDESC,SP_CODE, EMPLOYEE_EDESC, ATN_TIME,EOD_TIME,WORKING_HOURS,
            //   TARGET,VISITED TARGET_VISITED,TOTAL_VISITED VISITED,EXTRA,NOT_VISITED,TOTAL_PJP,PJP PJP_PRODUCTIVE,NON_PJP PJP_NON_PRODUCTIVE,
            //   NON_N_PJP NPJP_PRODUCTIVE,TOTAL_QUANTITY PJP_TOTAL_QUANTITY,TOTAL_PRICE PJP_TOTAL_AMOUNT,
            //   ROUND( (TOTAL_VISITED/DECODE(TARGET,0,1,TARGET)  * 100),2)  PERCENT_EFFECTIVE_CALLS,
            //   ROUND( (TOTAL_PJP/DECODE(TOTAL_VISITED,0,1,TOTAL_VISITED) * 100),2)  PERCENT_PRODUCTIVE_CALLS,
            //   EOD_REMARKS
            //   FROM(
            //   SELECT WM_CONCAT(DISTINCT ROUTE_NAME) ROUTE_NAME,GROUP_EDESC,SP_CODE, FULL_NAME EMPLOYEE_EDESC,
            //   TO_CHAR(ATN_TIME,'HH:MI:SS A.M.') ATN_TIME,
            //   CASE WHEN ATN_TIME = EOD_TIME THEN NULL
            //           ELSE TO_CHAR(EOD_TIME,'HH:MI:SS A.M.')
            //   END EOD_TIME,
            //   --TO_CHAR(EOD_TIME,'HH:MI:SS A.M.') EOD_TIME,
            //   NVL(ROUND(24 * (EOD_TIME - ATN_TIME),2),0) WORKING_HOURS,
            //   SUM(TARGET) TARGET,
            //   SUM(VISITED) VISITED,SUM(TOTAL_VISITED) TOTAL_VISITED, SUM(TOTAL_VISITED - VISITED) EXTRA, SUM(TARGET- VISITED) NOT_VISITED,
            //   SUM(TOTAL_PJP) TOTAL_PJP,SUM(PJP) PJP, SUM(VISITED - PJP)  NON_PJP, SUM(TOTAL_PJP- PJP) NON_N_PJP, SUM(TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE,
            //   EOD_REMARKS
            //   FROM(
            //   SELECT WM_CONCAT(DISTINCT B.ROUTE_NAME) ROUTE_NAME, WM_CONCAT(DISTINCT B.GROUP_EDESC) GROUP_EDESC, A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE
            //   ,(SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='ATN' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) ATN_TIME
            //   --,(SELECT MAX(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='EOD' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) EOD_TIME
            //   ,(SELECT TO_DATE(TO_CHAR(MAX(ATTENDANCE_TIME),'DD/MM/YYYY HH:MI:SS AM'),'DD/MM/YYYY HH:MI:SS AM') FROM HRIS_ATTENDANCE WHERE EMPLOYEE_ID = A.SP_CODE AND TRUNC(ATTENDANCE_DT) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) EOD_TIME
            //   ,(SELECT REMARKS FROM DIST_EOD_UPDATE WHERE TRUNC(CREATED_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD') AND SP_CODE =  A.SP_CODE AND ROWNUM = 1) EOD_REMARKS
            //   ,CASE WHEN WM_CONCAT(B.ENTITY_CODE) IS NULL THEN 0
            //       ELSE NVL(COUNT(*),0)
            //       END TARGET
            //   ,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE) {dateFilter} AND CUSTOMER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ASSIGN_DATE) {dateFilter})),0) VISITED
            //   ,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE) {dateFilter}),0) TOTAL_VISITED
            //   ,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) {dateFilter} AND RESELLER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ASSIGN_DATE) {dateFilter})),0) PJP
            //   ,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) {dateFilter}),0) TOTAL_PJP
            //   ,NVL((SELECT SUM(QUANTITY)  FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) {dateFilter}),0) TOTAL_QUANTITY
            //   ,NVL((SELECT SUM(TOTAL_PRICE)  FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) {dateFilter}),0) TOTAL_PRICE
            //   FROM DIST_LOGIN_USER A, DIST_TARGET_ENTITY B  
            //   WHERE A.USERID = B.USERID
            //   AND A.COMPANY_CODE = B.COMPANY_CODE
            //   AND A.ACTIVE = 'Y'
            //   AND A.COMPANY_CODE IN ('{companyCode}')
            //   AND B.ASSIGN_DATE {dateFilter}
            //   GROUP BY A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE, A.COMPANY_CODE
            //   ORDER BY B.ASSIGN_DATE)
            //   WHERE 1=1 {sp_filter}
            //   GROUP BY ATN_TIME,EOD_TIME, SP_CODE,GROUP_EDESC,SP_CODE, FULL_NAME,EOD_REMARKS)";
            // }
            // else
            // {
            //query = $@"SELECT ROUTE_NAME, GROUP_EDESC,SP_CODE, EMPLOYEE_EDESC, ATN_TIME,EOD_TIME,WORKING_HOURS,
            //        TARGET,VISITED TARGET_VISITED,TOTAL_VISITED VISITED,EXTRA,NOT_VISITED,TOTAL_PJP,PJP PJP_PRODUCTIVE,NON_PJP PJP_NON_PRODUCTIVE,
            //        NON_N_PJP NPJP_PRODUCTIVE,TOTAL_QUANTITY PJP_TOTAL_QUANTITY,TOTAL_PRICE PJP_TOTAL_AMOUNT,
            //        ROUND( (TOTAL_VISITED/DECODE(TARGET,0,1,TARGET)  * 100),2)  PERCENT_EFFECTIVE_CALLS,
            //        ROUND( (TOTAL_PJP/DECODE(TOTAL_VISITED,0,1,TOTAL_VISITED) * 100),2)  PERCENT_PRODUCTIVE_CALLS,
            //        EOD_REMARKS
            //        FROM(
            //        SELECT WM_CONCAT(DISTINCT ROUTE_NAME) ROUTE_NAME,GROUP_EDESC,SP_CODE, FULL_NAME EMPLOYEE_EDESC,
            //        TO_CHAR(ATN_TIME,'HH:MI:SS A.M.') ATN_TIME,
            //        CASE WHEN ATN_TIME = EOD_TIME THEN NULL
            //                ELSE TO_CHAR(EOD_TIME,'HH:MI:SS A.M.')
            //        END EOD_TIME,
            //        --TO_CHAR(EOD_TIME,'HH:MI:SS A.M.') EOD_TIME,
            //        NVL(ROUND(24 * (EOD_TIME - ATN_TIME),2),0) WORKING_HOURS,
            //        SUM(TARGET) TARGET,
            //        SUM(VISITED) VISITED,SUM(TOTAL_VISITED) TOTAL_VISITED, SUM(TOTAL_VISITED - VISITED) EXTRA, SUM(TARGET- VISITED) NOT_VISITED,
            //        SUM(TOTAL_PJP) TOTAL_PJP,SUM(PJP) PJP, SUM(VISITED - PJP)  NON_PJP, SUM(TOTAL_PJP- PJP) NON_N_PJP, SUM(TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE,
            //        EOD_REMARKS
            //        FROM(
            //        SELECT  B.ROUTE_NAME ROUTE_NAME,  B.GROUP_EDESC, A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE
            //        ,(SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='ATN' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) ATN_TIME
            //        --,(SELECT MAX(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='EOD' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) EOD_TIME
            //        ,(SELECT TO_DATE(TO_CHAR(MAX(ATTENDANCE_TIME),'DD/MM/YYYY HH:MI:SS AM'),'DD/MM/YYYY HH:MI:SS AM') FROM HRIS_ATTENDANCE WHERE EMPLOYEE_ID = A.SP_CODE AND TRUNC(ATTENDANCE_DT) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) EOD_TIME
            //        ,(SELECT REMARKS FROM DIST_EOD_UPDATE WHERE TRUNC(CREATED_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD') AND SP_CODE =  A.SP_CODE AND ROWNUM = 1) EOD_REMARKS
            //        ,CASE WHEN WM_CONCAT(B.ENTITY_CODE) IS NULL THEN 0
            //            ELSE NVL(COUNT(*),0)
            //            END TARGET
            //        ,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE) = B.ASSIGN_DATE AND CUSTOMER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ASSIGN_DATE) = B.ASSIGN_DATE )),0) VISITED
            //        ,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE)= B.ASSIGN_DATE),0) TOTAL_VISITED
            //        ,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) = B.ASSIGN_DATE AND RESELLER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ASSIGN_DATE) = B.ASSIGN_DATE)),0) PJP
            //        ,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) = B.ASSIGN_DATE ),0) TOTAL_PJP
            //        ,NVL((SELECT SUM(QUANTITY)  FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) = B.ASSIGN_DATE),0) TOTAL_QUANTITY
            //        ,NVL((SELECT SUM(TOTAL_PRICE)  FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) =B.ASSIGN_DATE),0) TOTAL_PRICE
            //        FROM DIST_LOGIN_USER A, DIST_TARGET_ENTITY B  
            //        WHERE A.USERID = B.USERID
            //        AND A.COMPANY_CODE = B.COMPANY_CODE
            //        AND A.ACTIVE = 'Y'
            //        AND A.COMPANY_CODE IN ('{companyCode}')
            //        AND B.ASSIGN_DATE {dateFilter}
            //        GROUP BY A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE, A.COMPANY_CODE,B.ROUTE_NAME,B.GROUP_EDESC
            //        ORDER BY B.ASSIGN_DATE)
            //        WHERE 1=1 {sp_filter}
            //        GROUP BY ATN_TIME,EOD_TIME, SP_CODE,GROUP_EDESC,SP_CODE, FULL_NAME,EOD_REMARKS)";

            query = $@"   SELECT WM_CONCAT(DISTINCT ROUTE_NAME) ROUTE_NAME, GROUP_EDESC,SP_CODE, EMPLOYEE_EDESC, ASSIGN_DATE,ATN_TIME,EOD_TIME,WORKING_HOURS,
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
AND A.COMPANY_CODE IN ('{companyCode}')
AND B.ASSIGN_DATE {dateFilter}
GROUP BY A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE, A.COMPANY_CODE,B.ROUTE_CODE, B.ROUTE_NAME,B.GROUP_EDESC, B.COMPANY_CODE
ORDER BY B.ASSIGN_DATE) AA
WHERE 1=1  
 {sp_filter} 
 GROUP BY  USERID, COMPANY_CODE,TRUNC(ASSIGN_DATE),  ATN_TIME,EOD_TIME, SP_CODE,GROUP_EDESC,SP_CODE, FULL_NAME,EOD_REMARKS)  group by   ASSIGN_DATE,ATN_TIME,EOD_TIME, SP_CODE,GROUP_EDESC,SP_CODE,EOD_REMARKS,EMPLOYEE_EDESC,WORKING_HOURS  order by sp_code
 ";
            // }
            var data = _objectEntity.SqlQuery<DetailTopEffective>(query).ToList();
            for (int i = 0; i < data.Count; i++)
            {
                var brandQuery = $@"SELECT PO.BRAND_NAME EDESC,SUM(PO.TOTAL_QUANTITY) QUANTITY, SUM(PO.TOTAL_AMOUNT) TOTAL_AMOUNT FROM
                (SELECT DPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME) BRAND_NAME,
                CASE DPO.MU_CODE
                    WHEN IUS.MU_CODE THEN ROUND(NVL(SUM(DPO.QUANTITY/IUS.CONVERSION_FACTOR),0),2)
                    ELSE NVL(SUM(DPO.QUANTITY),0)
                END TOTAL_QUANTITY,
               SUM(DPO.TOTAL_PRICE) TOTAL_AMOUNT,DPO.MU_CODE
               FROM DIST_IP_SSD_PURCHASE_ORDER DPO
               LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON DPO.ITEM_CODE = IUS.ITEM_CODE AND DPO.COMPANY_CODE = IUS.COMPANY_CODE
               INNER JOIN DIST_LOGIN_USER DLU ON DLU.USERID = DPO.CREATED_BY AND DLU.COMPANY_CODE = DPO.COMPANY_CODE AND DLU.SP_CODE = '{data[i].SP_CODE}'
               INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DPO.COMPANY_CODE
               INNER JOIN DIST_DISTRIBUTOR_MASTER D ON D.DISTRIBUTOR_CODE = DPO.CUSTOMER_CODE AND D.COMPANY_CODE = DPO.COMPANY_CODE AND D.ACTIVE = 'Y'
               INNER JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = D.DISTRIBUTOR_CODE AND SCS.COMPANY_CODE = D.COMPANY_CODE
               LEFT JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = DPO.ITEM_CODE AND ISS.COMPANY_CODE = DPO.COMPANY_CODE
               WHERE TRUNC(DPO.ORDER_DATE) BETWEEN trunc(TO_DATE('{data[i].ASSIGN_DATE.ToString("yyyy-MMM-dd")}', 'RRRR-MON-DD')) AND trunc(TO_DATE('{data[i].ASSIGN_DATE.ToString("yyyy-MMM-dd")}', 'RRRR-MON-DD'))
               AND ISS.DELETED_FLAG = 'N'
               AND DPO.COMPANY_CODE IN ('{companyCode}') AND
                TRIM(ISS.BRAND_NAME) IS NOT NULL
               GROUP BY DPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DPO.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME),DPO.MU_CODE,IUS.MU_CODE
                   UNION ALL
               SELECT RPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, RPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME) BRAND_NAME,
               CASE RPO.MU_CODE
                    WHEN IUS.MU_CODE THEN ROUND(NVL(SUM(RPO.QUANTITY/IUS.CONVERSION_FACTOR),0),2)
                    ELSE NVL(SUM(RPO.QUANTITY),0)
                END TOTAL_QUANTITY,
                SUM(RPO.TOTAL_PRICE) TOTAL_AMOUNT,RPO.MU_CODE
               FROM DIST_IP_SSR_PURCHASE_ORDER RPO
               INNER JOIN IP_ITEM_UNIT_SETUP IUS ON RPO.ITEM_CODE = IUS.ITEM_CODE AND RPO.COMPANY_CODE = IUS.COMPANY_CODE
               INNER JOIN DIST_LOGIN_USER DLU ON DLU.USERID = RPO.CREATED_BY AND DLU.COMPANY_CODE = RPO.COMPANY_CODE AND DLU.SP_CODE = '{data[i].SP_CODE}'
               INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = RPO.COMPANY_CODE
               INNER JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = RPO.RESELLER_CODE AND DRM.COMPANY_CODE = RPO.COMPANY_CODE
               LEFT JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = RPO.ITEM_CODE AND ISS.COMPANY_CODE = RPO.COMPANY_CODE
               WHERE TRUNC(RPO.ORDER_DATE) BETWEEN trunc(TO_DATE('{data[i].ASSIGN_DATE.ToString("yyyy-MMM-dd")}', 'RRRR-MON-DD')) AND trunc(TO_DATE('{data[i].ASSIGN_DATE.ToString("yyyy-MMM-dd")}', 'RRRR-MON-DD'))
               AND DRM.IS_CLOSED = 'N'
               AND ISS.DELETED_FLAG = 'N'
               AND RPO.COMPANY_CODE IN ('{companyCode}')
                AND TRIM(ISS.BRAND_NAME) IS NOT NULL
                 GROUP BY RPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), RPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME),RPO.MU_CODE,IUS.MU_CODE
                ) PO
                GROUP BY PO.BRAND_NAME";

                var typeQuery = $@"SELECT DOT.TYPE_EDESC EDESC,COUNT(DISTINCT RPO.CUSTOMER_CODE) QUANTITY
               FROM DIST_LOCATION_TRACK RPO
               INNER JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = RPO.CUSTOMER_CODE AND DRM.COMPANY_CODE = RPO.COMPANY_CODE
               INNER JOIN DIST_OUTLET_TYPE DOT ON DOT.TYPE_ID = DRM.OUTLET_TYPE_ID AND DOT.COMPANY_CODE = DRM.COMPANY_CODE
               WHERE TRUNC(RPO.UPDATE_DATE) BETWEEN trunc(TO_DATE('{data[i].ASSIGN_DATE.ToString("yyyy-MMM-dd")}', 'RRRR-MON-DD')) AND trunc(TO_DATE('{data[i].ASSIGN_DATE.ToString("yyyy-MMM-dd")}', 'RRRR-MON-DD'))
               AND RPO.COMPANY_CODE IN ('{companyCode}')
               AND RPO.SP_CODE = '{data[i].SP_CODE}'
               AND DRM.IS_CLOSED = 'N'
               AND DOT.DELETED_FLAG = 'N'
               GROUP BY DOT.TYPE_EDESC
               ORDER BY DOT.TYPE_EDESC";

                var subTypeQuery = $@"SELECT DOT.SUBTYPE_EDESC EDESC,COUNT(DISTINCT RPO.CUSTOMER_CODE) QUANTITY
               FROM DIST_LOCATION_TRACK RPO
               INNER JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = RPO.CUSTOMER_CODE AND DRM.COMPANY_CODE = RPO.COMPANY_CODE
               INNER JOIN DIST_OUTLET_SUBTYPE DOT ON DOT.SUBTYPE_ID = DRM.OUTLET_SUBTYPE_ID AND DOT.COMPANY_CODE = DRM.COMPANY_CODE
               WHERE TRUNC(RPO.UPDATE_DATE) BETWEEN trunc(TO_DATE('{data[i].ASSIGN_DATE.ToString("yyyy-MMM-dd")}', 'RRRR-MON-DD')) AND trunc(TO_DATE('{data[i].ASSIGN_DATE.ToString("yyyy-MMM-dd")}', 'RRRR-MON-DD'))
               AND RPO.COMPANY_CODE IN ('{companyCode}')
               AND RPO.SP_CODE = '{data[i].SP_CODE}'
               AND DOT.DELETED_FLAG = 'N'
               AND DRM.IS_CLOSED = 'N'
               GROUP BY DOT.SUBTYPE_EDESC
               ORDER BY DOT.SUBTYPE_EDESC";

                data[i].BRANDWISE = _objectEntity.SqlQuery<PairModel>(brandQuery).ToList();
                data[i].OUTLET_TYPE = _objectEntity.SqlQuery<PairModel>(typeQuery).ToList();
                data[i].OUTLET_SUB_TYPE = _objectEntity.SqlQuery<PairModel>(subTypeQuery).ToList();
            }
            return data;
        }

        public List<DetailTopEffective> GetDetailBrandingEmployeeReport(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join("','", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;
            var dateFilter = $"BETWEEN TO_DATE('{model.FromDate}','RRRR-MON-DD') AND TO_DATE('{model.ToDate}','RRRR-MON-DD')";
            var sp_filter = "";

            if (model.ItemBrandFilter.Count > 0)
                sp_filter = $" AND SP_CODE IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                sp_filter = $" AND SP_CODE IN ({userInfo.sp_codes})";
            }
            string query = "";


            query = $@"SELECT WM_CONCAT(DISTINCT ROUTE_NAME) ROUTE_NAME, GROUP_EDESC,SP_CODE, EMPLOYEE_EDESC, ATN_TIME,EOD_TIME,WORKING_HOURS,
                        sum(TARGET) TARGET,sum(VISITED) TARGET_VISITED,sum(TOTAL_VISITED) VISITED,sum(EXTRA) EXTRA,sum(NOT_VISITED) NOT_VISITED,sum(TOTAL_PJP) TOTAL_PJP,sum(PJP) PJP_PRODUCTIVE,sum(NON_PJP) PJP_NON_PRODUCTIVE,
                        sum(NON_N_PJP) NPJP_PRODUCTIVE,sum(TOTAL_QUANTITY) PJP_TOTAL_QUANTITY,sum(TOTAL_PRICE) PJP_TOTAL_AMOUNT,
                        ROUND( (sum(TOTAL_VISITED)/DECODE(sum(TARGET),0,1,sum(TARGET))  * 100),2)  PERCENT_EFFECTIVE_CALLS,
                        ROUND( (sum(TOTAL_PJP)/DECODE(sum(TOTAL_VISITED),0,1,sum(TOTAL_VISITED)) * 100),2)  PERCENT_PRODUCTIVE_CALLS,
                        EOD_REMARKS
                        FROM(
                        SELECT WM_CONCAT(DISTINCT ROUTE_NAME) ROUTE_NAME,GROUP_EDESC,SP_CODE, FULL_NAME EMPLOYEE_EDESC,
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
                        ,(SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE TRACK_TYPE='ATN' AND SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) ATN_TIME
                        ,(SELECT TO_DATE(TO_CHAR(MAX(ATTENDANCE_TIME),'DD/MM/YYYY HH:MI:SS AM'),'DD/MM/YYYY HH:MI:SS AM') FROM HRIS_ATTENDANCE WHERE EMPLOYEE_ID = A.SP_CODE AND TRUNC(ATTENDANCE_DT) = TO_DATE('{model.FromDate}','RRRR-MON-DD')) EOD_TIME
                        ,(SELECT REMARKS FROM DIST_EOD_UPDATE WHERE TRUNC(CREATED_DATE) = TO_DATE('{model.FromDate}','RRRR-MON-DD') AND SP_CODE =  A.SP_CODE AND ROWNUM = 1) EOD_REMARKS
                        ,CASE WHEN WM_CONCAT(B.ENTITY_CODE) IS NULL THEN 0
                        ELSE NVL(COUNT(*),0)
                        END TARGET
                        ,NVL((SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(UPDATE_DATE) = B.ASSIGN_DATE AND CUSTOMER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE  AND ROUTE_CODE = B.ROUTE_CODE AND TRUNC(ASSIGN_DATE) = B.ASSIGN_DATE )),0) VISITED
                        ,NVL((SELECT COUNT(DISTINCT RESELLER_CODE) FROM DIST_VISITED_PO WHERE USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND TRUNC(ORDER_DATE) = B.ASSIGN_DATE AND RESELLER_CODE IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE  USERID = A.USERID AND COMPANY_CODE = A.COMPANY_CODE AND ROUTE_CODE = B.ROUTE_CODE AND TRUNC(ASSIGN_DATE) = B.ASSIGN_DATE)),0) PJP
                        FROM DIST_LOGIN_USER A, DIST_TARGET_ENTITY B  
                        WHERE A.USERID = B.USERID
                        AND A.COMPANY_CODE = B.COMPANY_CODE
                        AND A.ACTIVE = 'Y'
                        AND A.BRANDING = 'Y'
                        AND A.COMPANY_CODE IN ('{companyCode}')
                        AND B.ASSIGN_DATE {dateFilter}
                        GROUP BY A.USERID, A.FULL_NAME, A.SP_CODE, B.ASSIGN_DATE, A.COMPANY_CODE,B.ROUTE_CODE, B.ROUTE_NAME,B.GROUP_EDESC, B.COMPANY_CODE
                        ORDER BY B.ASSIGN_DATE) AA
                        WHERE 1=1  
                        {sp_filter}
                        GROUP BY  USERID, COMPANY_CODE,TRUNC(ASSIGN_DATE),  ATN_TIME,EOD_TIME, SP_CODE,GROUP_EDESC,SP_CODE, FULL_NAME,EOD_REMARKS)  group by   ATN_TIME,EOD_TIME, SP_CODE,GROUP_EDESC,SP_CODE,EOD_REMARKS,EMPLOYEE_EDESC,WORKING_HOURS  order by sp_code";
            // }
            var data = _objectEntity.SqlQuery<DetailTopEffective>(query).ToList();
            for (int i = 0; i < data.Count; i++)
            {
                var brandQuery = $@"SELECT PO.BRAND_NAME EDESC,SUM(PO.TOTAL_QUANTITY) QUANTITY, SUM(PO.TOTAL_AMOUNT) TOTAL_AMOUNT FROM
                (SELECT DPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME) BRAND_NAME,
                CASE DPO.MU_CODE
                    WHEN IUS.MU_CODE THEN ROUND(NVL(SUM(DPO.QUANTITY/IUS.CONVERSION_FACTOR),0),2)
                    ELSE NVL(SUM(DPO.QUANTITY),0)
                END TOTAL_QUANTITY,
               SUM(DPO.TOTAL_PRICE) TOTAL_AMOUNT,DPO.MU_CODE
               FROM DIST_IP_SSD_PURCHASE_ORDER DPO
               LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON DPO.ITEM_CODE = IUS.ITEM_CODE AND DPO.COMPANY_CODE = IUS.COMPANY_CODE
               INNER JOIN DIST_LOGIN_USER DLU ON DLU.USERID = DPO.CREATED_BY AND DLU.COMPANY_CODE = DPO.COMPANY_CODE AND DLU.SP_CODE = '{data[i].SP_CODE}'
               INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DPO.COMPANY_CODE
               INNER JOIN DIST_DISTRIBUTOR_MASTER D ON D.DISTRIBUTOR_CODE = DPO.CUSTOMER_CODE AND D.COMPANY_CODE = DPO.COMPANY_CODE AND D.ACTIVE = 'Y'
               INNER JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = D.DISTRIBUTOR_CODE AND SCS.COMPANY_CODE = D.COMPANY_CODE
               LEFT JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = DPO.ITEM_CODE AND ISS.COMPANY_CODE = DPO.COMPANY_CODE
               WHERE TRUNC(DPO.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
               AND ISS.DELETED_FLAG = 'N'
               AND DPO.COMPANY_CODE IN ('{companyCode}') AND
                TRIM(ISS.BRAND_NAME) IS NOT NULL
               GROUP BY DPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DPO.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME),DPO.MU_CODE,IUS.MU_CODE
                   UNION ALL
               SELECT RPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, RPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME) BRAND_NAME,
               CASE RPO.MU_CODE
                    WHEN IUS.MU_CODE THEN ROUND(NVL(SUM(RPO.QUANTITY/IUS.CONVERSION_FACTOR),0),2)
                    ELSE NVL(SUM(RPO.QUANTITY),0)
                END TOTAL_QUANTITY,
                SUM(RPO.TOTAL_PRICE) TOTAL_AMOUNT,RPO.MU_CODE
               FROM DIST_IP_SSR_PURCHASE_ORDER RPO
               INNER JOIN IP_ITEM_UNIT_SETUP IUS ON RPO.ITEM_CODE = IUS.ITEM_CODE AND RPO.COMPANY_CODE = IUS.COMPANY_CODE
               INNER JOIN DIST_LOGIN_USER DLU ON DLU.USERID = RPO.CREATED_BY AND DLU.COMPANY_CODE = RPO.COMPANY_CODE AND DLU.SP_CODE = '{data[i].SP_CODE}'
               INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = RPO.COMPANY_CODE
               INNER JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = RPO.RESELLER_CODE AND DRM.COMPANY_CODE = RPO.COMPANY_CODE
               LEFT JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = RPO.ITEM_CODE AND ISS.COMPANY_CODE = RPO.COMPANY_CODE
               WHERE TRUNC(RPO.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
               AND DRM.IS_CLOSED = 'N'
               AND ISS.DELETED_FLAG = 'N'
               AND DLU.BRANDING='Y'
               AND RPO.COMPANY_CODE IN ('{companyCode}')
                AND TRIM(ISS.BRAND_NAME) IS NOT NULL
                 GROUP BY RPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), RPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME),RPO.MU_CODE,IUS.MU_CODE
                ) PO
                GROUP BY PO.BRAND_NAME";

                var typeQuery = $@"SELECT DOT.TYPE_EDESC EDESC,COUNT(DISTINCT RPO.CUSTOMER_CODE) QUANTITY
               FROM DIST_LOCATION_TRACK RPO
               INNER JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = RPO.CUSTOMER_CODE AND DRM.COMPANY_CODE = RPO.COMPANY_CODE
               INNER JOIN DIST_OUTLET_TYPE DOT ON DOT.TYPE_ID = DRM.OUTLET_TYPE_ID AND DOT.COMPANY_CODE = DRM.COMPANY_CODE
               WHERE TRUNC(RPO.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
               AND RPO.COMPANY_CODE IN ('{companyCode}')
               AND RPO.SP_CODE = '{data[i].SP_CODE}'
               AND DRM.IS_CLOSED = 'N'
               AND DOT.DELETED_FLAG = 'N'
               GROUP BY DOT.TYPE_EDESC
               ORDER BY DOT.TYPE_EDESC";

                var subTypeQuery = $@"SELECT DOT.SUBTYPE_EDESC EDESC,COUNT(DISTINCT RPO.CUSTOMER_CODE) QUANTITY
               FROM DIST_LOCATION_TRACK RPO
               INNER JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = RPO.CUSTOMER_CODE AND DRM.COMPANY_CODE = RPO.COMPANY_CODE
               INNER JOIN DIST_OUTLET_SUBTYPE DOT ON DOT.SUBTYPE_ID = DRM.OUTLET_SUBTYPE_ID AND DOT.COMPANY_CODE = DRM.COMPANY_CODE
               WHERE TRUNC(RPO.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
               AND RPO.COMPANY_CODE IN ('{companyCode}')
               AND RPO.SP_CODE = '{data[i].SP_CODE}'
               AND DOT.DELETED_FLAG = 'N'
               AND DRM.IS_CLOSED = 'N'
               GROUP BY DOT.SUBTYPE_EDESC
               ORDER BY DOT.SUBTYPE_EDESC";

                data[i].BRANDWISE = _objectEntity.SqlQuery<PairModel>(brandQuery).ToList();
                data[i].OUTLET_TYPE = _objectEntity.SqlQuery<PairModel>(typeQuery).ToList();
                data[i].OUTLET_SUB_TYPE = _objectEntity.SqlQuery<PairModel>(subTypeQuery).ToList();
            }
            return data;
        }

        public List<EmployeeWisePerformance> GetEmployeeProductive(ReportFiltersModel model, User userInfo, string SP_CODE)
        {
            var companyCode = string.Join("','", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;
            //            string query = $@"SELECT DISTINCT
            //  DM.DEALER_CODE AS ENTITY_CODE,
            //  TRIM(PT.PARTY_TYPE_EDESC) AS ENTITY_NAME,
            //  TRIM(DM.REG_OFFICE_ADDRESS) AS ADDRESS,
            //  'P' AS ENTITY_TYPE, 'PJP' AS JOURNEY_PLAN,
            //  TRUNC(RD.ASSIGN_DATE) ASSIGN_DATE, LT.VISIT_DATE,
            //  TO_CHAR(LT.VISIT_DATE,'HH:MI:SS A.M.') VISIT_TIME,
            //  LT.VISIT_BY, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS
            //  FROM DIST_ROUTE_DETAIL RD
            //  INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'P' AND RE.COMPANY_CODE = RD.COMPANY_CODE
            //  INNER JOIN DIST_DEALER_MASTER DM ON DM.DEALER_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RD.COMPANY_CODE AND DM.ACTIVE = 'Y'
            //  INNER JOIN IP_PARTY_TYPE_CODE PT ON PT.PARTY_TYPE_CODE = DM.DEALER_CODE AND PT.COMPANY_CODE = DM.COMPANY_CODE
            //  LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, TRIM(B.PARTY_TYPE_EDESC) CUSTOMER_EDESC, A.CUSTOMER_TYPE, A.COMPANY_CODE, MAX(A.UPDATE_DATE) VISIT_DATE, C.EMPLOYEE_EDESC AS VISIT_BY,
            //            (CASE WHEN A.IS_VISITED IS NULL THEN 'X' ELSE A.IS_VISITED END) IS_VISITED,
            //            TRIM(A.REMARKS) REMARKS
            //            FROM DIST_LOCATION_TRACK A
            //            INNER JOIN IP_PARTY_TYPE_CODE B ON B.PARTY_TYPE_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
            //            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
            //            WHERE 1 = 1
            //            AND SP_CODE = '{SP_CODE}'
            //            AND A.CUSTOMER_TYPE = 'P'
            //            AND TRUNC(A.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
            //            AND A.COMPANY_CODE IN ('{companyCode}')
            //            GROUP BY A.SP_CODE, A.CUSTOMER_CODE, TRIM(B.PARTY_TYPE_EDESC), A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC,
            //                (CASE WHEN A.IS_VISITED IS NULL THEN 'X' ELSE A.IS_VISITED END),
            //                A.REMARKS
            //            ) LT
            //            ON LT.SP_CODE = RD.EMP_CODE AND LT.CUSTOMER_CODE = DM.DEALER_CODE AND LT.COMPANY_CODE = RD.COMPANY_CODE AND TRUNC(LT.VISIT_DATE) = TRUNC(RD.ASSIGN_DATE)
            //  WHERE 1 = 1
            //  AND TRUNC(RD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
            //  AND RD.EMP_CODE = '{SP_CODE}'
            //  AND RD.COMPANY_CODE IN ('{companyCode}')
            //UNION ALL
            //  SELECT DISTINCT DLT.CUSTOMER_CODE ENTITY_CODE, 
            //    TRIM(PT.PARTY_TYPE_EDESC) ENTITY_NAME, 
            //    TRIM(DM.REG_OFFICE_ADDRESS) AS ADDRESS,
            //    'P' AS ENTITY_TYPE, 'NPJP' AS JOURNEY_PLAN,
            //    NULL AS ASSIGN_DATE, MAX(DLT.UPDATE_DATE) VISIT_DATE,
            //    TO_CHAR(MAX(DLT.UPDATE_DATE),'HH:MI:SS A.M.') VISIT_TIME,
            //    TRIM(ES.EMPLOYEE_EDESC) VISIT_BY,
            //    (CASE WHEN DLT.IS_VISITED IS NULL THEN 'X' ELSE DLT.IS_VISITED END) IS_VISITED,
            //    WM_CONCAT(DLT.REMARKS) REMARKS
            //  FROM DIST_LOCATION_TRACK DLT
            //  INNER JOIN IP_PARTY_TYPE_CODE PT ON PT.PARTY_TYPE_CODE = DLT.CUSTOMER_CODE AND PT.COMPANY_CODE = DLT.COMPANY_CODE
            //  INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = DLT.SP_CODE AND ES.COMPANY_CODE = DLT.COMPANY_CODE
            //  INNER JOIN DIST_DEALER_MASTER DM ON DM.DEALER_CODE = PT.PARTY_TYPE_CODE AND DM.COMPANY_CODE = DLT.COMPANY_CODE AND DM.ACTIVE = 'Y'
            //  WHERE 1 = 1
            //    AND DLT.CUSTOMER_TYPE = 'P'
            //    AND DLT.SP_CODE = '{SP_CODE}'
            //    AND TRUNC(DLT.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
            //    AND DLT.CUSTOMER_CODE NOT IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE SP_CODE = '{SP_CODE}' AND ENTITY_TYPE = 'P' AND ASSIGN_DATE BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
            //    AND DLT.COMPANY_CODE IN ('{companyCode}')
            //  GROUP BY DLT.CUSTOMER_CODE, 
            //    TRIM(PT.PARTY_TYPE_EDESC), 
            //    TRIM(DM.REG_OFFICE_ADDRESS),
            //    'P', 'NPJP',
            //    NULL, TRIM(ES.EMPLOYEE_EDESC),
            //    (CASE WHEN DLT.IS_VISITED IS NULL THEN 'X' ELSE DLT.IS_VISITED END)
            //UNION ALL
            //  SELECT DISTINCT
            //    DM.DISTRIBUTOR_CODE AS ENTITY_CODE,
            //    CS.CUSTOMER_EDESC AS ENTITY_NAME,
            //    TRIM(NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS)) AS ADDRESS,
            //    'D' AS ENTITY_TYPE, 'PJP' AS JOURNEY_PLAN,
            //    TRUNC(RD.ASSIGN_DATE) ASSIGN_DATE, LT.VISIT_DATE,
            //    TO_CHAR(LT.VISIT_DATE,'HH:MI:SS A.M.') VISIT_TIME,
            //    LT.VISIT_BY, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS
            //    FROM DIST_ROUTE_DETAIL RD
            //    INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = RD.COMPANY_CODE
            //    INNER JOIN DIST_DISTRIBUTOR_MASTER DM ON DM.DISTRIBUTOR_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RD.COMPANY_CODE AND DM.ACTIVE = 'Y'
            //    INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
            //    LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, TRIM(B.CUSTOMER_EDESC) CUSTOMER_EDESC, A.CUSTOMER_TYPE, A.COMPANY_CODE, MAX(A.UPDATE_DATE) VISIT_DATE, C.EMPLOYEE_EDESC AS VISIT_BY,
            //              (CASE WHEN A.IS_VISITED IS NULL THEN 'X' ELSE A.IS_VISITED END) IS_VISITED,
            //              TRIM(A.REMARKS) REMARKS
            //              FROM DIST_LOCATION_TRACK A
            //              INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
            //              INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
            //              WHERE 1 = 1
            //              AND SP_CODE = '{SP_CODE}'
            //              AND A.CUSTOMER_TYPE = 'D'
            //              AND TRUNC(A.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
            //              AND A.COMPANY_CODE IN ('{companyCode}')
            //              GROUP BY A.SP_CODE, A.CUSTOMER_CODE, TRIM(B.CUSTOMER_EDESC), A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC,
            //                  (CASE WHEN A.IS_VISITED IS NULL THEN 'X' ELSE A.IS_VISITED END),
            //                  A.REMARKS
            //              ) LT
            //              ON LT.SP_CODE = RD.EMP_CODE AND LT.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND LT.COMPANY_CODE = RD.COMPANY_CODE AND TRUNC(LT.VISIT_DATE) = TRUNC(RD.ASSIGN_DATE)
            //    WHERE 1 = 1
            //    AND TRUNC(RD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
            //    AND RD.EMP_CODE = '{SP_CODE}'
            //    AND RD.COMPANY_CODE IN ('{companyCode}')
            //UNION ALL
            //  SELECT DISTINCT DLT.CUSTOMER_CODE ENTITY_CODE, 
            //    TRIM(CS.CUSTOMER_EDESC) ENTITY_NAME, 
            //    TRIM(NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS)) AS ADDRESS,
            //    'D' AS ENTITY_TYPE, 'NPJP' AS JOURNEY_PLAN,
            //    NULL AS ASSIGN_DATE, MAX(DLT.UPDATE_DATE) VISIT_DATE,
            //    TO_CHAR(MAX(DLT.UPDATE_DATE),'HH:MI:SS A.M.') VISIT_TIME,
            //    TRIM(ES.EMPLOYEE_EDESC) VISIT_BY,
            //    (CASE WHEN DLT.IS_VISITED IS NULL THEN 'X' ELSE DLT.IS_VISITED END) IS_VISITED,
            //    WM_CONCAT(DLT.REMARKS) REMARKS
            //  FROM DIST_LOCATION_TRACK DLT
            //  INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DLT.CUSTOMER_CODE AND CS.COMPANY_CODE = DLT.COMPANY_CODE
            //  INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = DLT.SP_CODE AND ES.COMPANY_CODE = DLT.COMPANY_CODE
            //  INNER JOIN DIST_DISTRIBUTOR_MASTER DM ON DM.DISTRIBUTOR_CODE = CS.CUSTOMER_CODE AND DM.COMPANY_CODE = DLT.COMPANY_CODE AND DM.ACTIVE = 'Y'
            //  WHERE 1 = 1
            //    AND DLT.CUSTOMER_TYPE = 'D'
            //    AND DLT.SP_CODE = '{SP_CODE}'
            //    AND TRUNC(DLT.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
            //    AND DLT.CUSTOMER_CODE NOT IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE SP_CODE = '{SP_CODE}' AND ENTITY_TYPE = 'D' AND ASSIGN_DATE BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
            //    AND DLT.COMPANY_CODE IN ('{companyCode}')
            //  GROUP BY DLT.CUSTOMER_CODE, 
            //    TRIM(CS.CUSTOMER_EDESC), 
            //    TRIM(NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS)),
            //    'D', 'NPJP',
            //    TRIM(ES.EMPLOYEE_EDESC),
            //    (CASE WHEN DLT.IS_VISITED IS NULL THEN 'X' ELSE DLT.IS_VISITED END)
            //UNION ALL
            //  SELECT DISTINCT
            //    RS.RESELLER_CODE AS ENTITY_CODE,
            //    RS.RESELLER_NAME AS ENTITY_NAME,
            //    TRIM(RS.REG_OFFICE_ADDRESS) AS ADDRESS,
            //    'R' AS ENTITY_TYPE, 'PJP' AS JOURNEY_PLAN,
            //    TRUNC(RD.ASSIGN_DATE) ASSIGN_DATE, MAX(LT.VISIT_DATE) VISIT_DATE,
            //    TO_CHAR( MAX(LT.VISIT_DATE),'HH:MI:SS A.M.') VISIT_TIME,
            //    LT.VISIT_BY, NVL(LT.IS_VISITED, 'X') AS IS_VISITED,MAX(LT.REMARKS)
            //    FROM DIST_ROUTE_DETAIL RD
            //    INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'R' AND RE.COMPANY_CODE = RD.COMPANY_CODE
            //    INNER JOIN DIST_RESELLER_MASTER RS ON RS.RESELLER_CODE = RE.ENTITY_CODE AND RS.COMPANY_CODE= RD.COMPANY_CODE AND RS.ACTIVE='Y'
            //    LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, TRIM(B.RESELLER_NAME) RESELLER_NAME, A.CUSTOMER_TYPE, A.COMPANY_CODE, MAX(A.UPDATE_DATE) VISIT_DATE, C.EMPLOYEE_EDESC AS VISIT_BY,
            //              (CASE WHEN A.IS_VISITED IS NULL THEN 'X' ELSE A.IS_VISITED END) IS_VISITED,
            //              TRIM(A.REMARKS) REMARKS
            //              FROM DIST_LOCATION_TRACK A
            //              INNER JOIN DIST_RESELLER_MASTER B ON B.RESELLER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
            //              INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
            //              WHERE 1 = 1
            //              AND SP_CODE = '{SP_CODE}'
            //              AND A.CUSTOMER_TYPE = 'R'
            //              AND B.IS_CLOSED = 'N'
            //              AND TRUNC(A.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
            //              AND A.COMPANY_CODE IN ('{companyCode}')
            //              GROUP BY A.SP_CODE, A.CUSTOMER_CODE, TRIM(B.RESELLER_NAME), A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC,
            //                  (CASE WHEN A.IS_VISITED IS NULL THEN 'X' ELSE A.IS_VISITED END),
            //                  A.REMARKS
            //              ) LT
            //              ON LT.SP_CODE = RD.EMP_CODE AND LT.CUSTOMER_CODE = RS.RESELLER_CODE AND LT.COMPANY_CODE = RD.COMPANY_CODE AND TRUNC(LT.VISIT_DATE) = TRUNC(RD.ASSIGN_DATE)
            //    WHERE 1 = 1
            //    AND TRUNC(RD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
            //    AND RD.EMP_CODE = '{SP_CODE}'
            //    AND RD.COMPANY_CODE IN ('{companyCode}')
            //    AND RS.IS_CLOSED = 'N'
            //    GROUP BY RS.RESELLER_CODE, RS.RESELLER_NAME, RS.REG_OFFICE_ADDRESS, RD.ASSIGN_DATE, LT.VISIT_BY, LT.IS_VISITED
            //UNION ALL
            //  SELECT DISTINCT DLT.CUSTOMER_CODE ENTITY_CODE, 
            //      TRIM(RS.RESELLER_NAME) ENTITY_NAME, 
            //      TRIM(RS.REG_OFFICE_ADDRESS) AS ADDRESS,
            //      'R' AS ENTITY_TYPE, 'NPJP' AS JOURNEY_PLAN,
            //      NULL AS ASSIGN_DATE, MAX(DLT.UPDATE_DATE) VISIT_DATE,
            //      TO_CHAR(MAX(DLT.UPDATE_DATE),'HH:MI:SS A.M.') VISIT_TIME,
            //      TRIM(ES.EMPLOYEE_EDESC) VISIT_BY,
            //      (CASE WHEN DLT.IS_VISITED IS NULL THEN 'X' ELSE DLT.IS_VISITED END) IS_VISITED,
            //      WM_CONCAT(DLT.REMARKS) REMARKS
            //    FROM DIST_LOCATION_TRACK DLT
            //    INNER JOIN DIST_RESELLER_MASTER RS ON RS.RESELLER_CODE = DLT.CUSTOMER_CODE AND RS.COMPANY_CODE = DLT.COMPANY_CODE AND RS.ACTIVE='Y'
            //    INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = DLT.SP_CODE AND ES.COMPANY_CODE = DLT.COMPANY_CODE
            //    WHERE 1 = 1
            //      AND RS.IS_CLOSED = 'N'
            //      AND DLT.CUSTOMER_TYPE = 'R'
            //      AND DLT.SP_CODE = '{SP_CODE}'
            //      AND TRUNC(DLT.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
            //      AND DLT.CUSTOMER_CODE NOT IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE SP_CODE = '{SP_CODE}' AND ENTITY_TYPE = 'R' AND ASSIGN_DATE BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
            //      AND DLT.COMPANY_CODE IN ('{companyCode}')
            //    GROUP BY DLT.CUSTOMER_CODE, 
            //      TRIM(RS.RESELLER_NAME), 
            //      TRIM(RS.REG_OFFICE_ADDRESS),
            //      'R', 'NPJP',
            //      NULL, TRIM(ES.EMPLOYEE_EDESC),
            //      (CASE WHEN DLT.IS_VISITED IS NULL THEN 'X' ELSE DLT.IS_VISITED END)";

            string query = $@"SELECT DISTINCT
  DM.DEALER_CODE AS ENTITY_CODE,
  TRIM(PT.PARTY_TYPE_EDESC) AS ENTITY_NAME,
  TRIM(DM.REG_OFFICE_ADDRESS) AS ADDRESS,
  'P' AS ENTITY_TYPE, 'PJP' AS JOURNEY_PLAN,
  TRUNC(RD.ASSIGN_DATE) ASSIGN_DATE, LT.VISIT_DATE,
  TO_CHAR(LT.VISIT_DATE,'HH:MI:SS A.M.') VISIT_TIME,
  LT.VISIT_BY, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,'' TYPE_EDESC,'' SUBTYPE_EDESC
  FROM DIST_ROUTE_DETAIL RD
  INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'P' AND RE.COMPANY_CODE = RD.COMPANY_CODE
  INNER JOIN DIST_DEALER_MASTER DM ON DM.DEALER_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RD.COMPANY_CODE AND DM.ACTIVE = 'Y'
  INNER JOIN IP_PARTY_TYPE_CODE PT ON PT.PARTY_TYPE_CODE = DM.DEALER_CODE AND PT.COMPANY_CODE = DM.COMPANY_CODE
  LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, TRIM(B.PARTY_TYPE_EDESC) CUSTOMER_EDESC, A.CUSTOMER_TYPE, A.COMPANY_CODE, MAX(A.UPDATE_DATE) VISIT_DATE, C.EMPLOYEE_EDESC AS VISIT_BY,
            (CASE WHEN A.IS_VISITED IS NULL THEN 'X' ELSE A.IS_VISITED END) IS_VISITED,
            TRIM(A.REMARKS) REMARKS
            FROM DIST_LOCATION_TRACK A
            INNER JOIN IP_PARTY_TYPE_CODE B ON B.PARTY_TYPE_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
            WHERE 1 = 1
            AND SP_CODE = '{SP_CODE}'
            AND A.CUSTOMER_TYPE = 'P'
            AND TRUNC(A.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
            AND A.COMPANY_CODE IN ('{companyCode}')
            GROUP BY A.SP_CODE, A.CUSTOMER_CODE, TRIM(B.PARTY_TYPE_EDESC), A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC,
                (CASE WHEN A.IS_VISITED IS NULL THEN 'X' ELSE A.IS_VISITED END),
                A.REMARKS
            ) LT
            ON LT.SP_CODE = RD.EMP_CODE AND LT.CUSTOMER_CODE = DM.DEALER_CODE AND LT.COMPANY_CODE = RD.COMPANY_CODE AND TRUNC(LT.VISIT_DATE) = TRUNC(RD.ASSIGN_DATE)
  WHERE 1 = 1
  AND TRUNC(RD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
  AND RD.EMP_CODE = '{SP_CODE}'
  AND RD.COMPANY_CODE IN ('{companyCode}')
UNION ALL
  SELECT DISTINCT DLT.CUSTOMER_CODE ENTITY_CODE, 
    TRIM(PT.PARTY_TYPE_EDESC) ENTITY_NAME, 
    TRIM(DM.REG_OFFICE_ADDRESS) AS ADDRESS,
    'P' AS ENTITY_TYPE, 'NPJP' AS JOURNEY_PLAN,
    NULL AS ASSIGN_DATE, MAX(DLT.UPDATE_DATE) VISIT_DATE,
    TO_CHAR(MAX(DLT.UPDATE_DATE),'HH:MI:SS A.M.') VISIT_TIME,
    TRIM(ES.EMPLOYEE_EDESC) VISIT_BY,
    (CASE WHEN DLT.IS_VISITED IS NULL THEN 'X' ELSE DLT.IS_VISITED END) IS_VISITED,
    WM_CONCAT(DLT.REMARKS) REMARKS,'' TYPE_EDESC,'' SUBTYPE_EDESC
  FROM DIST_LOCATION_TRACK DLT
  INNER JOIN IP_PARTY_TYPE_CODE PT ON PT.PARTY_TYPE_CODE = DLT.CUSTOMER_CODE AND PT.COMPANY_CODE = DLT.COMPANY_CODE
  INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = DLT.SP_CODE AND ES.COMPANY_CODE = DLT.COMPANY_CODE
  INNER JOIN DIST_DEALER_MASTER DM ON DM.DEALER_CODE = PT.PARTY_TYPE_CODE AND DM.COMPANY_CODE = DLT.COMPANY_CODE AND DM.ACTIVE = 'Y'
  WHERE 1 = 1
    AND DLT.CUSTOMER_TYPE = 'P'
    AND DLT.SP_CODE = '{SP_CODE}'
    AND TRUNC(DLT.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
    AND DLT.CUSTOMER_CODE NOT IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE SP_CODE = '{SP_CODE}' AND ENTITY_TYPE = 'P' AND ASSIGN_DATE BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
    AND DLT.COMPANY_CODE IN ('{companyCode}')
  GROUP BY DLT.CUSTOMER_CODE, 
    TRIM(PT.PARTY_TYPE_EDESC), 
    TRIM(DM.REG_OFFICE_ADDRESS),
    'P', 'NPJP',
    NULL, TRIM(ES.EMPLOYEE_EDESC),
    (CASE WHEN DLT.IS_VISITED IS NULL THEN 'X' ELSE DLT.IS_VISITED END)
UNION ALL
  SELECT DISTINCT
    DM.DISTRIBUTOR_CODE AS ENTITY_CODE,
    CS.CUSTOMER_EDESC AS ENTITY_NAME,
    TRIM(NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS)) AS ADDRESS,
    'D' AS ENTITY_TYPE, 'PJP' AS JOURNEY_PLAN,
    TRUNC(RD.ASSIGN_DATE) ASSIGN_DATE, LT.VISIT_DATE,
    TO_CHAR(LT.VISIT_DATE,'HH:MI:SS A.M.') VISIT_TIME,
    LT.VISIT_BY, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS,'' TYPE_EDESC,'' SUBTYPE_EDESC
    FROM DIST_ROUTE_DETAIL RD
    INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = RD.COMPANY_CODE
    INNER JOIN DIST_DISTRIBUTOR_MASTER DM ON DM.DISTRIBUTOR_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RD.COMPANY_CODE AND DM.ACTIVE = 'Y'
    INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
    LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, TRIM(B.CUSTOMER_EDESC) CUSTOMER_EDESC, A.CUSTOMER_TYPE, A.COMPANY_CODE, MAX(A.UPDATE_DATE) VISIT_DATE, C.EMPLOYEE_EDESC AS VISIT_BY,
              (CASE WHEN A.IS_VISITED IS NULL THEN 'X' ELSE A.IS_VISITED END) IS_VISITED,
              TRIM(A.REMARKS) REMARKS
              FROM DIST_LOCATION_TRACK A
              INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
              INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
              WHERE 1 = 1
              AND SP_CODE = '{SP_CODE}'
              AND A.CUSTOMER_TYPE = 'D'
              AND TRUNC(A.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
              AND A.COMPANY_CODE IN ('{companyCode}')
              GROUP BY A.SP_CODE, A.CUSTOMER_CODE, TRIM(B.CUSTOMER_EDESC), A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC,
                  (CASE WHEN A.IS_VISITED IS NULL THEN 'X' ELSE A.IS_VISITED END),
                  A.REMARKS
              ) LT
              ON LT.SP_CODE = RD.EMP_CODE AND LT.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND LT.COMPANY_CODE = RD.COMPANY_CODE AND TRUNC(LT.VISIT_DATE) = TRUNC(RD.ASSIGN_DATE)
    WHERE 1 = 1
    AND TRUNC(RD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
    AND RD.EMP_CODE = '{SP_CODE}'
    AND RD.COMPANY_CODE IN ('{companyCode}')
UNION ALL
  SELECT DISTINCT DLT.CUSTOMER_CODE ENTITY_CODE, 
    TRIM(CS.CUSTOMER_EDESC) ENTITY_NAME, 
    TRIM(NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS)) AS ADDRESS,
    'D' AS ENTITY_TYPE, 'NPJP' AS JOURNEY_PLAN,
    NULL AS ASSIGN_DATE, MAX(DLT.UPDATE_DATE) VISIT_DATE,
    TO_CHAR(MAX(DLT.UPDATE_DATE),'HH:MI:SS A.M.') VISIT_TIME,
    TRIM(ES.EMPLOYEE_EDESC) VISIT_BY,
    (CASE WHEN DLT.IS_VISITED IS NULL THEN 'X' ELSE DLT.IS_VISITED END) IS_VISITED,
    WM_CONCAT(DLT.REMARKS) REMARKS,'' TYPE_EDESC,'' SUBTYPE_EDESC
  FROM DIST_LOCATION_TRACK DLT
  INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DLT.CUSTOMER_CODE AND CS.COMPANY_CODE = DLT.COMPANY_CODE
  INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = DLT.SP_CODE AND ES.COMPANY_CODE = DLT.COMPANY_CODE
  INNER JOIN DIST_DISTRIBUTOR_MASTER DM ON DM.DISTRIBUTOR_CODE = CS.CUSTOMER_CODE AND DM.COMPANY_CODE = DLT.COMPANY_CODE AND DM.ACTIVE = 'Y'
  WHERE 1 = 1
    AND DLT.CUSTOMER_TYPE = 'D'
    AND DLT.SP_CODE = '{SP_CODE}'
    AND TRUNC(DLT.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
    AND DLT.CUSTOMER_CODE NOT IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE SP_CODE = '{SP_CODE}' AND ENTITY_TYPE = 'D' AND ASSIGN_DATE BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
    AND DLT.COMPANY_CODE IN ('{companyCode}')
  GROUP BY DLT.CUSTOMER_CODE, 
    TRIM(CS.CUSTOMER_EDESC), 
    TRIM(NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS)),
    'D', 'NPJP',
    TRIM(ES.EMPLOYEE_EDESC),
    (CASE WHEN DLT.IS_VISITED IS NULL THEN 'X' ELSE DLT.IS_VISITED END)
UNION ALL
    SELECT DISTINCT
    RS.RESELLER_CODE AS ENTITY_CODE,
    RS.RESELLER_NAME AS ENTITY_NAME,
    TRIM(RS.REG_OFFICE_ADDRESS) AS ADDRESS,
    'R' AS ENTITY_TYPE, 'PJP' AS JOURNEY_PLAN,
    TRUNC(RD.ASSIGN_DATE) ASSIGN_DATE, MAX(LT.VISIT_DATE) VISIT_DATE,
    TO_CHAR( MAX(LT.VISIT_DATE),'HH:MI:SS A.M.') VISIT_TIME,
    LT.VISIT_BY, NVL(LT.IS_VISITED, 'X') AS IS_VISITED,MAX(LT.REMARKS),DOT.TYPE_EDESC,DOS.SUBTYPE_EDESC
    FROM DIST_ROUTE_DETAIL RD
    INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'R' AND RE.COMPANY_CODE = RD.COMPANY_CODE
    INNER JOIN DIST_RESELLER_MASTER RS ON RS.RESELLER_CODE = RE.ENTITY_CODE AND RS.COMPANY_CODE= RD.COMPANY_CODE AND RS.ACTIVE='Y'
       INNER JOIN DIST_OUTLET_TYPE DOT ON DOT.TYPE_ID=RS.OUTLET_TYPE_ID AND RS.COMPANY_CODE=DOT.COMPANY_CODE
    INNER JOIN DIST_OUTLET_SUBTYPE DOS ON DOS.SUBTYPE_ID=RS.OUTLET_SUBTYPE_ID AND DOS.TYPE_ID=DOT.TYPE_ID
    LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, TRIM(B.RESELLER_NAME) RESELLER_NAME, A.CUSTOMER_TYPE, A.COMPANY_CODE, MAX(A.UPDATE_DATE) VISIT_DATE, C.EMPLOYEE_EDESC AS VISIT_BY,
              (CASE WHEN A.IS_VISITED IS NULL THEN 'X' ELSE A.IS_VISITED END) IS_VISITED,
              TRIM(A.REMARKS) REMARKS
              FROM DIST_LOCATION_TRACK A
              INNER JOIN DIST_RESELLER_MASTER B ON B.RESELLER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
              INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
              WHERE 1 = 1
              AND SP_CODE = '{SP_CODE}'
              AND A.CUSTOMER_TYPE = 'R'
              AND B.IS_CLOSED = 'N'
              AND TRUNC(A.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
              AND A.COMPANY_CODE IN ('{companyCode}')
              GROUP BY A.SP_CODE, A.CUSTOMER_CODE, TRIM(B.RESELLER_NAME), A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC,
                  (CASE WHEN A.IS_VISITED IS NULL THEN 'X' ELSE A.IS_VISITED END),
                  A.REMARKS
              ) LT
              ON LT.SP_CODE = RD.EMP_CODE AND LT.CUSTOMER_CODE = RS.RESELLER_CODE AND LT.COMPANY_CODE = RD.COMPANY_CODE AND TRUNC(LT.VISIT_DATE) = TRUNC(RD.ASSIGN_DATE)
    WHERE 1 = 1
    AND TRUNC(RD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
    AND RD.EMP_CODE = '{SP_CODE}'
    AND RD.COMPANY_CODE IN ('{companyCode}')
    AND RS.IS_CLOSED = 'N'
    GROUP BY RS.RESELLER_CODE, RS.RESELLER_NAME, RS.REG_OFFICE_ADDRESS, RD.ASSIGN_DATE, LT.VISIT_BY, LT.IS_VISITED,DOT.TYPE_EDESC,DOS.SUBTYPE_EDESC
UNION ALL
    SELECT DISTINCT DLT.CUSTOMER_CODE ENTITY_CODE, 
      TRIM(RS.RESELLER_NAME) ENTITY_NAME, 
      TRIM(RS.REG_OFFICE_ADDRESS) AS ADDRESS,
      'R' AS ENTITY_TYPE, 'NPJP' AS JOURNEY_PLAN,
      NULL AS ASSIGN_DATE, MAX(DLT.UPDATE_DATE) VISIT_DATE,
      TO_CHAR(MAX(DLT.UPDATE_DATE),'HH:MI:SS A.M.') VISIT_TIME,
      TRIM(ES.EMPLOYEE_EDESC) VISIT_BY,
      (CASE WHEN DLT.IS_VISITED IS NULL THEN 'X' ELSE DLT.IS_VISITED END) IS_VISITED,
      WM_CONCAT(DLT.REMARKS) REMARKS,DOT.TYPE_EDESC,DOS.SUBTYPE_EDESC
    FROM DIST_LOCATION_TRACK DLT
    INNER JOIN DIST_RESELLER_MASTER RS ON RS.RESELLER_CODE = DLT.CUSTOMER_CODE AND RS.COMPANY_CODE = DLT.COMPANY_CODE AND RS.ACTIVE='Y'
    INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = DLT.SP_CODE AND ES.COMPANY_CODE = DLT.COMPANY_CODE
    INNER JOIN DIST_OUTLET_TYPE DOT ON DOT.TYPE_ID=RS.OUTLET_TYPE_ID AND RS.COMPANY_CODE=DOT.COMPANY_CODE
    INNER JOIN DIST_OUTLET_SUBTYPE DOS ON DOS.SUBTYPE_ID=RS.OUTLET_SUBTYPE_ID AND DOS.TYPE_ID=DOT.TYPE_ID
    WHERE 1 = 1
      AND RS.IS_CLOSED = 'N'
      AND DLT.CUSTOMER_TYPE = 'R'
      AND DLT.SP_CODE = '{SP_CODE}'
      AND TRUNC(DLT.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
      AND DLT.CUSTOMER_CODE NOT IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE SP_CODE = '{SP_CODE}' AND ENTITY_TYPE = 'R' AND ASSIGN_DATE BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
      AND DLT.COMPANY_CODE IN ('{companyCode}')
    GROUP BY DLT.CUSTOMER_CODE, 
      TRIM(RS.RESELLER_NAME), 
      TRIM(RS.REG_OFFICE_ADDRESS),
      'R', 'NPJP',
      NULL, TRIM(ES.EMPLOYEE_EDESC),
      (CASE WHEN DLT.IS_VISITED IS NULL THEN 'X' ELSE DLT.IS_VISITED END),DOT.TYPE_EDESC,DOS.SUBTYPE_EDESC";
            var result = _objectEntity.SqlQuery<EmployeeWisePerformance>(query).ToList();

            for (int i = 0; i < result.Count; i++)
            {
                result[i].BRANDS = GetBrandsProductive(companyCode, SP_CODE, result[i].ENTITY_CODE, result[i].ENTITY_TYPE, model.FromDate);
            }

            var EmployeeWithOrder = VisitedOrderList(model, userInfo, SP_CODE);

            foreach (var data in EmployeeWithOrder)
            {
                var temp = result.FirstOrDefault(x => x.SP_CODE == data.SP_CODE && x.ENTITY_CODE == data.ENTITY_CODE);
                if (temp != null)
                {
                    temp.IS_VISITED = "Z";
                }
            }

            return result;

        }

        public List<EmployeeWisePerformance> GetBrandingEmployeeProductive(ReportFiltersModel model, User userInfo, string SP_CODE)
        {
            var companyCode = string.Join("','", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;
            string query = $@"SELECT DISTINCT
                  DM.DEALER_CODE AS ENTITY_CODE,
                  TRIM(PT.PARTY_TYPE_EDESC) AS ENTITY_NAME,
                  TRIM(DM.REG_OFFICE_ADDRESS) AS ADDRESS,
                  'P' AS ENTITY_TYPE, 'PJP' AS JOURNEY_PLAN,
                  TRUNC(RD.ASSIGN_DATE) ASSIGN_DATE, LT.VISIT_DATE,
                  TO_CHAR(LT.VISIT_DATE,'HH:MI:SS A.M.') VISIT_TIME,
                  LT.VISIT_BY, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS
                  FROM DIST_ROUTE_DETAIL RD
                  INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'P' AND RE.COMPANY_CODE = RD.COMPANY_CODE
                  INNER JOIN DIST_DEALER_MASTER DM ON DM.DEALER_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RD.COMPANY_CODE AND DM.ACTIVE = 'Y'
                  INNER JOIN IP_PARTY_TYPE_CODE PT ON PT.PARTY_TYPE_CODE = DM.DEALER_CODE AND PT.COMPANY_CODE = DM.COMPANY_CODE
                  LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, TRIM(B.PARTY_TYPE_EDESC) CUSTOMER_EDESC, A.CUSTOMER_TYPE, A.COMPANY_CODE, MAX(A.UPDATE_DATE) VISIT_DATE, C.EMPLOYEE_EDESC AS VISIT_BY,
                            (CASE WHEN A.IS_VISITED IS NULL THEN 'X' ELSE A.IS_VISITED END) IS_VISITED,
                            TRIM(A.REMARKS) REMARKS
                            FROM DIST_LOCATION_TRACK A
                            INNER JOIN IP_PARTY_TYPE_CODE B ON B.PARTY_TYPE_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            WHERE 1 = 1
                            AND SP_CODE = '{SP_CODE}'
                            AND A.CUSTOMER_TYPE = 'P'
                            AND TRUNC(A.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
                            AND A.COMPANY_CODE IN ('{companyCode}')
                            GROUP BY A.SP_CODE, A.CUSTOMER_CODE, TRIM(B.PARTY_TYPE_EDESC), A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC,
                                (CASE WHEN A.IS_VISITED IS NULL THEN 'X' ELSE A.IS_VISITED END),
                                A.REMARKS
                            ) LT
                            ON LT.SP_CODE = RD.EMP_CODE AND LT.CUSTOMER_CODE = DM.DEALER_CODE AND LT.COMPANY_CODE = RD.COMPANY_CODE AND TRUNC(LT.VISIT_DATE) = TRUNC(RD.ASSIGN_DATE)
                  WHERE 1 = 1
                  AND TRUNC(RD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
                  AND RD.EMP_CODE = '{SP_CODE}'
                  AND RD.COMPANY_CODE IN ('{companyCode}')
                UNION ALL
                  SELECT DISTINCT DLT.CUSTOMER_CODE ENTITY_CODE, 
                    TRIM(PT.PARTY_TYPE_EDESC) ENTITY_NAME, 
                    TRIM(DM.REG_OFFICE_ADDRESS) AS ADDRESS,
                    'P' AS ENTITY_TYPE, 'NPJP' AS JOURNEY_PLAN,
                    NULL AS ASSIGN_DATE, MAX(DLT.UPDATE_DATE) VISIT_DATE,
                    TO_CHAR(MAX(DLT.UPDATE_DATE),'HH:MI:SS A.M.') VISIT_TIME,
                    TRIM(ES.EMPLOYEE_EDESC) VISIT_BY,
                    (CASE WHEN DLT.IS_VISITED IS NULL THEN 'X' ELSE DLT.IS_VISITED END) IS_VISITED,
                    WM_CONCAT(DLT.REMARKS) REMARKS
                  FROM DIST_LOCATION_TRACK DLT
                  INNER JOIN IP_PARTY_TYPE_CODE PT ON PT.PARTY_TYPE_CODE = DLT.CUSTOMER_CODE AND PT.COMPANY_CODE = DLT.COMPANY_CODE
                  INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = DLT.SP_CODE AND ES.COMPANY_CODE = DLT.COMPANY_CODE
                  INNER JOIN DIST_DEALER_MASTER DM ON DM.DEALER_CODE = PT.PARTY_TYPE_CODE AND DM.COMPANY_CODE = DLT.COMPANY_CODE AND DM.ACTIVE = 'Y'
                  WHERE 1 = 1
                    AND DLT.CUSTOMER_TYPE = 'P'
                    AND DLT.SP_CODE = '{SP_CODE}'
                    AND TRUNC(DLT.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
                    AND DLT.CUSTOMER_CODE NOT IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE SP_CODE = '{SP_CODE}' AND ENTITY_TYPE = 'P' AND ASSIGN_DATE BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
                    AND DLT.COMPANY_CODE IN ('{companyCode}')
                  GROUP BY DLT.CUSTOMER_CODE, 
                    TRIM(PT.PARTY_TYPE_EDESC), 
                    TRIM(DM.REG_OFFICE_ADDRESS),
                    'P', 'NPJP',
                    NULL, TRIM(ES.EMPLOYEE_EDESC),
                    (CASE WHEN DLT.IS_VISITED IS NULL THEN 'X' ELSE DLT.IS_VISITED END)
                UNION ALL
                  SELECT DISTINCT
                    DM.DISTRIBUTOR_CODE AS ENTITY_CODE,
                    CS.CUSTOMER_EDESC AS ENTITY_NAME,
                    TRIM(NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS)) AS ADDRESS,
                    'D' AS ENTITY_TYPE, 'PJP' AS JOURNEY_PLAN,
                    TRUNC(RD.ASSIGN_DATE) ASSIGN_DATE, LT.VISIT_DATE,
                    TO_CHAR(LT.VISIT_DATE,'HH:MI:SS A.M.') VISIT_TIME,
                    LT.VISIT_BY, NVL(LT.IS_VISITED, 'X') AS IS_VISITED, LT.REMARKS
                    FROM DIST_ROUTE_DETAIL RD
                    INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'D' AND RE.COMPANY_CODE = RD.COMPANY_CODE
                    INNER JOIN DIST_DISTRIBUTOR_MASTER DM ON DM.DISTRIBUTOR_CODE = RE.ENTITY_CODE AND DM.COMPANY_CODE = RD.COMPANY_CODE AND DM.ACTIVE = 'Y'
                    INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND CS.COMPANY_CODE = DM.COMPANY_CODE
                    LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, TRIM(B.CUSTOMER_EDESC) CUSTOMER_EDESC, A.CUSTOMER_TYPE, A.COMPANY_CODE, MAX(A.UPDATE_DATE) VISIT_DATE, C.EMPLOYEE_EDESC AS VISIT_BY,
                              (CASE WHEN A.IS_VISITED IS NULL THEN 'X' ELSE A.IS_VISITED END) IS_VISITED,
                              TRIM(A.REMARKS) REMARKS
                              FROM DIST_LOCATION_TRACK A
                              INNER JOIN SA_CUSTOMER_SETUP B ON B.CUSTOMER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                              INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                              WHERE 1 = 1
                              AND SP_CODE = '{SP_CODE}'
                              AND A.CUSTOMER_TYPE = 'D'
                              AND TRUNC(A.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
                              AND A.COMPANY_CODE IN ('{companyCode}')
                              GROUP BY A.SP_CODE, A.CUSTOMER_CODE, TRIM(B.CUSTOMER_EDESC), A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC,
                                  (CASE WHEN A.IS_VISITED IS NULL THEN 'X' ELSE A.IS_VISITED END),
                                  A.REMARKS
                              ) LT
                              ON LT.SP_CODE = RD.EMP_CODE AND LT.CUSTOMER_CODE = DM.DISTRIBUTOR_CODE AND LT.COMPANY_CODE = RD.COMPANY_CODE AND TRUNC(LT.VISIT_DATE) = TRUNC(RD.ASSIGN_DATE)
                    WHERE 1 = 1
                    AND TRUNC(RD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
                    AND RD.EMP_CODE = '{SP_CODE}'
                    AND RD.COMPANY_CODE IN ('{companyCode}')
                UNION ALL
                  SELECT DISTINCT DLT.CUSTOMER_CODE ENTITY_CODE, 
                    TRIM(CS.CUSTOMER_EDESC) ENTITY_NAME, 
                    TRIM(NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS)) AS ADDRESS,
                    'D' AS ENTITY_TYPE, 'NPJP' AS JOURNEY_PLAN,
                    NULL AS ASSIGN_DATE, MAX(DLT.UPDATE_DATE) VISIT_DATE,
                    TO_CHAR(MAX(DLT.UPDATE_DATE),'HH:MI:SS A.M.') VISIT_TIME,
                    TRIM(ES.EMPLOYEE_EDESC) VISIT_BY,
                    (CASE WHEN DLT.IS_VISITED IS NULL THEN 'X' ELSE DLT.IS_VISITED END) IS_VISITED,
                    WM_CONCAT(DLT.REMARKS) REMARKS
                  FROM DIST_LOCATION_TRACK DLT
                  INNER JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DLT.CUSTOMER_CODE AND CS.COMPANY_CODE = DLT.COMPANY_CODE
                  INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = DLT.SP_CODE AND ES.COMPANY_CODE = DLT.COMPANY_CODE
                  INNER JOIN DIST_DISTRIBUTOR_MASTER DM ON DM.DISTRIBUTOR_CODE = CS.CUSTOMER_CODE AND DM.COMPANY_CODE = DLT.COMPANY_CODE AND DM.ACTIVE = 'Y'
                  WHERE 1 = 1
                    AND DLT.CUSTOMER_TYPE = 'D'
                    AND DLT.SP_CODE = '{SP_CODE}'
                    AND TRUNC(DLT.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
                    AND DLT.CUSTOMER_CODE NOT IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE SP_CODE = '{SP_CODE}' AND ENTITY_TYPE = 'D' AND ASSIGN_DATE BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
                    AND DLT.COMPANY_CODE IN ('{companyCode}')
                  GROUP BY DLT.CUSTOMER_CODE, 
                    TRIM(CS.CUSTOMER_EDESC), 
                    TRIM(NVL(DM.REG_OFFICE_ADDRESS, CS.REGD_OFFICE_EADDRESS)),
                    'D', 'NPJP',
                    TRIM(ES.EMPLOYEE_EDESC),
                    (CASE WHEN DLT.IS_VISITED IS NULL THEN 'X' ELSE DLT.IS_VISITED END)
                UNION ALL
                  SELECT DISTINCT
                    RS.RESELLER_CODE AS ENTITY_CODE,
                    RS.RESELLER_NAME AS ENTITY_NAME,
                    TRIM(RS.REG_OFFICE_ADDRESS) AS ADDRESS,
                    'R' AS ENTITY_TYPE, 'PJP' AS JOURNEY_PLAN,
                    TRUNC(RD.ASSIGN_DATE) ASSIGN_DATE, MAX(LT.VISIT_DATE) VISIT_DATE,
                    TO_CHAR( MAX(LT.VISIT_DATE),'HH:MI:SS A.M.') VISIT_TIME,
                    LT.VISIT_BY, NVL(LT.IS_VISITED, 'X') AS IS_VISITED,MAX(LT.REMARKS)
                    FROM DIST_ROUTE_DETAIL RD
                    INNER JOIN DIST_ROUTE_ENTITY RE ON RE.ROUTE_CODE = RD.ROUTE_CODE AND RE.ENTITY_TYPE = 'R' AND RE.COMPANY_CODE = RD.COMPANY_CODE
                    INNER JOIN DIST_RESELLER_MASTER RS ON RS.RESELLER_CODE = RE.ENTITY_CODE AND RS.COMPANY_CODE= RD.COMPANY_CODE AND RS.ACTIVE='Y'
                    LEFT JOIN (SELECT A.SP_CODE, A.CUSTOMER_CODE, TRIM(B.RESELLER_NAME) RESELLER_NAME, A.CUSTOMER_TYPE, A.COMPANY_CODE, MAX(A.UPDATE_DATE) VISIT_DATE, C.EMPLOYEE_EDESC AS VISIT_BY,
                              (CASE WHEN A.IS_VISITED IS NULL THEN 'X' ELSE A.IS_VISITED END) IS_VISITED,
                              TRIM(A.REMARKS) REMARKS
                              FROM DIST_LOCATION_TRACK A
                              INNER JOIN DIST_RESELLER_MASTER B ON B.RESELLER_CODE = A.CUSTOMER_CODE AND B.COMPANY_CODE = A.COMPANY_CODE
                              INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = A.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                              WHERE 1 = 1
                              AND SP_CODE = '{SP_CODE}'
                              AND A.CUSTOMER_TYPE = 'R'
                              AND B.IS_CLOSED = 'N'
                              AND TRUNC(A.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
                              AND A.COMPANY_CODE IN ('{companyCode}')
                              GROUP BY A.SP_CODE, A.CUSTOMER_CODE, TRIM(B.RESELLER_NAME), A.CUSTOMER_TYPE, A.COMPANY_CODE, C.EMPLOYEE_EDESC,
                                  (CASE WHEN A.IS_VISITED IS NULL THEN 'X' ELSE A.IS_VISITED END),
                                  A.REMARKS
                              ) LT
                              ON LT.SP_CODE = RD.EMP_CODE AND LT.CUSTOMER_CODE = RS.RESELLER_CODE AND LT.COMPANY_CODE = RD.COMPANY_CODE AND TRUNC(LT.VISIT_DATE) = TRUNC(RD.ASSIGN_DATE)
                    WHERE 1 = 1
                    AND TRUNC(RD.ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
                    AND RD.EMP_CODE = '{SP_CODE}'
                    AND RD.COMPANY_CODE IN ('{companyCode}')
                    AND RS.IS_CLOSED = 'N'
                    GROUP BY RS.RESELLER_CODE, RS.RESELLER_NAME, RS.REG_OFFICE_ADDRESS, RD.ASSIGN_DATE, LT.VISIT_BY, LT.IS_VISITED
                UNION ALL
                  SELECT DISTINCT DLT.CUSTOMER_CODE ENTITY_CODE, 
                      TRIM(RS.RESELLER_NAME) ENTITY_NAME, 
                      TRIM(RS.REG_OFFICE_ADDRESS) AS ADDRESS,
                      'R' AS ENTITY_TYPE, 'NPJP' AS JOURNEY_PLAN,
                      NULL AS ASSIGN_DATE, MAX(DLT.UPDATE_DATE) VISIT_DATE,
                      TO_CHAR(MAX(DLT.UPDATE_DATE),'HH:MI:SS A.M.') VISIT_TIME,
                      TRIM(ES.EMPLOYEE_EDESC) VISIT_BY,
                      (CASE WHEN DLT.IS_VISITED IS NULL THEN 'X' ELSE DLT.IS_VISITED END) IS_VISITED,
                      WM_CONCAT(DLT.REMARKS) REMARKS
                    FROM DIST_LOCATION_TRACK DLT
                    INNER JOIN DIST_RESELLER_MASTER RS ON RS.RESELLER_CODE = DLT.CUSTOMER_CODE AND RS.COMPANY_CODE = DLT.COMPANY_CODE AND RS.ACTIVE='Y'
                    INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = DLT.SP_CODE AND ES.COMPANY_CODE = DLT.COMPANY_CODE
                    WHERE 1 = 1
                      AND RS.IS_CLOSED = 'N'
                      AND DLT.CUSTOMER_TYPE = 'R'
                      AND DLT.SP_CODE = '{SP_CODE}'
                      AND TRUNC(DLT.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD') 
                      AND DLT.CUSTOMER_CODE NOT IN (SELECT ENTITY_CODE FROM DIST_TARGET_ENTITY WHERE SP_CODE = '{SP_CODE}' AND ENTITY_TYPE = 'R' AND ASSIGN_DATE BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
                      AND DLT.COMPANY_CODE IN ('{companyCode}')
                    GROUP BY DLT.CUSTOMER_CODE, 
                      TRIM(RS.RESELLER_NAME), 
                      TRIM(RS.REG_OFFICE_ADDRESS),
                      'R', 'NPJP',
                      NULL, TRIM(ES.EMPLOYEE_EDESC),
                      (CASE WHEN DLT.IS_VISITED IS NULL THEN 'X' ELSE DLT.IS_VISITED END)";
            var result = _objectEntity.SqlQuery<EmployeeWisePerformance>(query).ToList();

            for (int i = 0; i < result.Count; i++)
            {
                result[i].BRANDS = GetBrandsProductive(companyCode, SP_CODE, result[i].ENTITY_CODE, result[i].ENTITY_TYPE, model.FromDate);
            }

            var EmployeeWithOrder = VisitedOrderList(model, userInfo, SP_CODE);

            foreach (var data in EmployeeWithOrder)
            {
                var temp = result.FirstOrDefault(x => x.SP_CODE == data.SP_CODE && x.ENTITY_CODE == data.ENTITY_CODE);
                if (temp != null)
                {
                    temp.IS_VISITED = "Z";
                }
            }

            return result;

        }

        private List<PairModel> GetBrandsProductive(string companyCode, string spCode, string entityCode, string entityType, string date)
        {
            var entQuery = "";
            if (entityType == "R")
                entQuery = $@"SELECT RPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, RPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME) BRAND_NAME,
               CASE RPO.MU_CODE
                    WHEN IUS.MU_CODE THEN NVL(SUM(RPO.QUANTITY),0)
                    ELSE NVL(SUM(RPO.QUANTITY*IUS.CONVERSION_FACTOR),0)
                END TOTAL_QUANTITY,
                SUM(RPO.TOTAL_PRICE) TOTAL_AMOUNT,RPO.MU_CODE
               FROM DIST_IP_SSR_PURCHASE_ORDER RPO
               LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON RPO.ITEM_CODE = IUS.ITEM_CODE AND RPO.COMPANY_CODE = IUS.COMPANY_CODE
               INNER JOIN DIST_LOGIN_USER DLU ON DLU.USERID = RPO.CREATED_BY AND DLU.COMPANY_CODE = RPO.COMPANY_CODE AND DLU.SP_CODE = '{spCode}'
               INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = RPO.COMPANY_CODE
               INNER JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = RPO.RESELLER_CODE AND DRM.COMPANY_CODE = RPO.COMPANY_CODE AND DRM.RESELLER_CODE = '{entityCode}' AND DRM.IS_CLOSED = 'N'
               LEFT JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = RPO.ITEM_CODE AND ISS.COMPANY_CODE = RPO.COMPANY_CODE
               WHERE TRUNC(RPO.ORDER_DATE) = TO_DATE('{date}', 'RRRR-MON-DD')
               AND RPO.COMPANY_CODE IN ('{companyCode}')
                AND TRIM(ISS.BRAND_NAME) IS NOT NULL
                 GROUP BY RPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), RPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME),RPO.MU_CODE,IUS.MU_CODE";
            else
                entQuery = $@"SELECT DPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME) BRAND_NAME,
                CASE DPO.MU_CODE
                    WHEN IUS.MU_CODE THEN NVL(SUM(DPO.QUANTITY),0)
                    ELSE NVL(SUM(DPO.QUANTITY*IUS.CONVERSION_FACTOR),0)
                END TOTAL_QUANTITY,
                SUM(DPO.TOTAL_PRICE) TOTAL_AMOUNT,DPO.MU_CODE
               FROM DIST_IP_SSD_PURCHASE_ORDER DPO
               LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON DPO.ITEM_CODE = IUS.ITEM_CODE AND DPO.COMPANY_CODE = IUS.COMPANY_CODE
               INNER JOIN DIST_LOGIN_USER DLU ON DLU.USERID = DPO.CREATED_BY AND DLU.COMPANY_CODE = DPO.COMPANY_CODE AND DLU.SP_CODE = '{spCode}'
               INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DPO.COMPANY_CODE
               INNER JOIN DIST_DISTRIBUTOR_MASTER D ON D.DISTRIBUTOR_CODE = DPO.CUSTOMER_CODE AND D.COMPANY_CODE = DPO.COMPANY_CODE AND D.DISTRIBUTOR_CODE = '{entityCode}' AND D.ACTIVE = 'Y'
               INNER JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = D.DISTRIBUTOR_CODE AND SCS.COMPANY_CODE = D.COMPANY_CODE
               LEFT JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = DPO.ITEM_CODE AND ISS.COMPANY_CODE = DPO.COMPANY_CODE
               WHERE TRUNC(DPO.ORDER_DATE) = TO_DATE('{date}', 'RRRR-MON-DD')
               AND DPO.COMPANY_CODE IN ('{companyCode}') AND
                TRIM(ISS.BRAND_NAME) IS NOT NULL
               GROUP BY DPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DPO.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME),DPO.MU_CODE,IUS.MU_CODE";

            var brandQuery = $@"SELECT PO.BRAND_NAME EDESC,SUM(PO.TOTAL_QUANTITY) QUANTITY, SUM(PO.TOTAL_AMOUNT) TOTAL_AMOUNT FROM
                ({entQuery}
                ) PO
                GROUP BY PO.BRAND_NAME";
            var data = _objectEntity.SqlQuery<PairModel>(brandQuery).ToList();
            return data;
        }
        public List<EmpBrandWiseModel> GetBrandwiseEmpData(ReportFiltersModel model, User userInfo, string SpCode)
        {
            var query = $@"SELECT  PO.EMPLOYEE_EDESC,  PO.BRAND_NAME,
                SUM(PO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(PO.TOTAL_AMOUNT) TOTAL_AMOUNT,PO.MU_CODE FROM (SELECT DPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME) BRAND_NAME,
                SUM(DPO.QUANTITY) TOTAL_QUANTITY, SUM(DPO.TOTAL_PRICE) TOTAL_AMOUNT,DPO.MU_CODE
               FROM DIST_IP_SSD_PURCHASE_ORDER DPO
               INNER JOIN DIST_LOGIN_USER DLU ON DLU.USERID = DPO.CREATED_BY AND DLU.COMPANY_CODE = DPO.COMPANY_CODE AND DLU.SP_CODE = '{SpCode}'
               INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DPO.COMPANY_CODE
               INNER JOIN DIST_DISTRIBUTOR_MASTER D ON D.DISTRIBUTOR_CODE = DPO.CUSTOMER_CODE AND D.COMPANY_CODE = DPO.COMPANY_CODE AND D.ACTIVE = 'Y'
               INNER JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = D.DISTRIBUTOR_CODE AND SCS.COMPANY_CODE = D.COMPANY_CODE
               LEFT JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = DPO.ITEM_CODE AND ISS.COMPANY_CODE = DPO.COMPANY_CODE
               WHERE (TRUNC(DPO.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')) AND DPO.COMPANY_CODE = '{userInfo.company_code}' AND
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
               WHERE (TRUNC(RPO.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
                    AND RPO.COMPANY_CODE = '{userInfo.company_code}'
                    AND DRM.IS_CLOSED = 'N'
                AND TRIM(ISS.BRAND_NAME) IS NOT NULL
                                                   GROUP BY RPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), RPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME),RPO.MU_CODE
                ) PO
                GROUP BY PO.SP_CODE, PO.EMPLOYEE_EDESC, PO.COMPANY_CODE, PO.BRAND_NAME,PO.MU_CODE";

            var data = _objectEntity.SqlQuery<EmpBrandWiseModel>(query).ToList();
            return data;
        }

        public List<EmpBrandWiseModel> GetBrandwiseEmpDataConversion(ReportFiltersModel model, User userInfo, string SpCode)
        {
            var brandQuery = $@"SELECT PO.BRAND_NAME BRAND_NAME,SUM(PO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(PO.TOTAL_AMOUNT) TOTAL_AMOUNT,PO.MU_CODE,round(sum(TOTAL_QUANTITY_case),2) TOTAL_CASE FROM
                (SELECT DPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME) BRAND_NAME,
                     CASE DPO.MU_CODE
                    WHEN IUS.MU_CODE THEN NVL(SUM(DPO.QUANTITY),0)
                    ELSE NVL(SUM(DPO.QUANTITY*IUS.CONVERSION_FACTOR),0)
                END TOTAL_QUANTITY,
                 CASE DPO.MU_CODE
                    WHEN IST.INDEX_MU_CODE  THEN NVL(SUM(DPO.QUANTITY),0)
                    ELSE NVL(SUM(DPO.QUANTITY/IUS.CONVERSION_FACTOR),0)
                END TOTAL_QUANTITY_case,
               SUM(DPO.TOTAL_PRICE) TOTAL_AMOUNT,IUS.MU_CODE
               FROM DIST_IP_SSD_PURCHASE_ORDER DPO
               LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON DPO.ITEM_CODE = IUS.ITEM_CODE AND DPO.COMPANY_CODE = IUS.COMPANY_CODE
               INNER JOIN DIST_LOGIN_USER DLU ON DLU.USERID = DPO.CREATED_BY AND DLU.COMPANY_CODE = DPO.COMPANY_CODE AND DLU.SP_CODE = '{SpCode}'
               INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DPO.COMPANY_CODE
               INNER JOIN DIST_DISTRIBUTOR_MASTER D ON D.DISTRIBUTOR_CODE = DPO.CUSTOMER_CODE AND D.COMPANY_CODE = DPO.COMPANY_CODE AND D.ACTIVE = 'Y'
               INNER JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = D.DISTRIBUTOR_CODE AND SCS.COMPANY_CODE = D.COMPANY_CODE
               LEFT JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = DPO.ITEM_CODE AND ISS.COMPANY_CODE = DPO.COMPANY_CODE
               LEFT JOIN ip_item_master_setup IST ON IST.ITEM_CODE = DPO.ITEM_CODE AND IST.COMPANY_CODE = DPO.COMPANY_CODE
               WHERE TRUNC(DPO.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
               AND ISS.DELETED_FLAG = 'N'
               AND DPO.COMPANY_CODE IN ('{userInfo.company_code}') AND
                TRIM(ISS.BRAND_NAME) IS NOT NULL
               GROUP BY DPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DPO.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME),DPO.MU_CODE,IUS.MU_CODE,IST.INDEX_MU_CODE 
                   UNION ALL
               SELECT RPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, RPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME) BRAND_NAME,
               CASE RPO.MU_CODE
                    WHEN IUS.MU_CODE THEN NVL(SUM(RPO.QUANTITY),0)
                    ELSE NVL(SUM(RPO.QUANTITY*IUS.CONVERSION_FACTOR),0)
                END TOTAL_QUANTITY,
                 CASE RPO.MU_CODE
                    WHEN IST.INDEX_MU_CODE  THEN NVL(SUM(RPO.QUANTITY),0)
                    ELSE NVL(SUM(RPO.QUANTITY/IUS.CONVERSION_FACTOR),0)
                END TOTAL_QUANTITY_case,
                SUM(RPO.TOTAL_PRICE) TOTAL_AMOUNT,IUS.MU_CODE
               FROM DIST_IP_SSR_PURCHASE_ORDER RPO
               INNER JOIN IP_ITEM_UNIT_SETUP IUS ON RPO.ITEM_CODE = IUS.ITEM_CODE AND RPO.COMPANY_CODE = IUS.COMPANY_CODE
               INNER JOIN DIST_LOGIN_USER DLU ON DLU.USERID = RPO.CREATED_BY AND DLU.COMPANY_CODE = RPO.COMPANY_CODE AND DLU.SP_CODE = '{SpCode}'
               INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = RPO.COMPANY_CODE
               INNER JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = RPO.RESELLER_CODE AND DRM.COMPANY_CODE = RPO.COMPANY_CODE
               LEFT JOIN IP_ITEM_SPEC_SETUP ISS ON ISS.ITEM_CODE = RPO.ITEM_CODE AND ISS.COMPANY_CODE = RPO.COMPANY_CODE
LEFT JOIN ip_item_master_setup IST ON IST.ITEM_CODE = RPO.ITEM_CODE AND IST.COMPANY_CODE = RPO.COMPANY_CODE
               WHERE TRUNC(RPO.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
               AND DRM.IS_CLOSED = 'N'
               AND ISS.DELETED_FLAG = 'N'
               AND RPO.COMPANY_CODE IN ('{userInfo.company_code}')
                AND TRIM(ISS.BRAND_NAME) IS NOT NULL
                 GROUP BY RPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), RPO.COMPANY_CODE, TRIM(ISS.BRAND_NAME),RPO.MU_CODE,IUS.MU_CODE,IST.INDEX_MU_CODE 
                ) PO
                GROUP BY PO.BRAND_NAME,PO.MU_CODE";

            var data = _objectEntity.SqlQuery<EmpBrandWiseModel>(brandQuery).ToList();
            return data;
        }

        public List<EmpBrandWiseModel> GetItemwiseEmpDataConversion(ReportFiltersModel model, User userInfo, string SpCode)
        {
            var brandQuery = $@"SELECT PO.BRAND_NAME BRAND_NAME,SUM(PO.TOTAL_QUANTITY) TOTAL_QUANTITY, SUM(PO.TOTAL_AMOUNT) TOTAL_AMOUNT,PO.MU_CODE,round(sum(TOTAL_QUANTITY_case),2) TOTAL_CASE FROM
                (SELECT DPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DPO.COMPANY_CODE, TRIM(ISS.ITEM_EDESC) BRAND_NAME,
                     CASE DPO.MU_CODE
                    WHEN IUS.MU_CODE THEN NVL(SUM(DPO.QUANTITY),0)
                    ELSE NVL(SUM(DPO.QUANTITY*IUS.CONVERSION_FACTOR),0)
                END TOTAL_QUANTITY,
                 CASE DPO.MU_CODE
                    WHEN ISS.INDEX_MU_CODE  THEN NVL(SUM(DPO.QUANTITY),0)
                    ELSE NVL(SUM(DPO.QUANTITY/IUS.CONVERSION_FACTOR),0)
                END TOTAL_QUANTITY_case,
               SUM(DPO.TOTAL_PRICE) TOTAL_AMOUNT,IUS.MU_CODE
               FROM DIST_IP_SSD_PURCHASE_ORDER DPO
               LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON DPO.ITEM_CODE = IUS.ITEM_CODE AND DPO.COMPANY_CODE = IUS.COMPANY_CODE
               INNER JOIN DIST_LOGIN_USER DLU ON DLU.USERID = DPO.CREATED_BY AND DLU.COMPANY_CODE = DPO.COMPANY_CODE AND DLU.SP_CODE = '{SpCode}'
               INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DPO.COMPANY_CODE
               INNER JOIN DIST_DISTRIBUTOR_MASTER D ON D.DISTRIBUTOR_CODE = DPO.CUSTOMER_CODE AND D.COMPANY_CODE = DPO.COMPANY_CODE AND D.ACTIVE = 'Y'
               INNER JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = D.DISTRIBUTOR_CODE AND SCS.COMPANY_CODE = D.COMPANY_CODE
               LEFT JOIN ip_item_master_setup ISS ON ISS.ITEM_CODE = DPO.ITEM_CODE AND ISS.COMPANY_CODE = DPO.COMPANY_CODE
               WHERE TRUNC(DPO.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
               AND ISS.DELETED_FLAG = 'N'
               AND DPO.COMPANY_CODE IN ('{userInfo.company_code}') AND
               ISS.DELETED_FLAG='N' AND ISS.GROUP_SKU_FLAG='I'
               GROUP BY DPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DPO.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DPO.COMPANY_CODE, TRIM(ISS.ITEM_EDESC),DPO.MU_CODE,IUS.MU_CODE,ISS.INDEX_MU_CODE 
                   UNION ALL
               SELECT RPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, RPO.COMPANY_CODE, TRIM(ISS.ITEM_EDESC) BRAND_NAME,
               CASE RPO.MU_CODE
                    WHEN IUS.MU_CODE THEN NVL(SUM(RPO.QUANTITY),0)
                    ELSE NVL(SUM(RPO.QUANTITY*IUS.CONVERSION_FACTOR),0)
                END TOTAL_QUANTITY,
                 CASE RPO.MU_CODE
                    WHEN ISS.INDEX_MU_CODE  THEN NVL(SUM(RPO.QUANTITY),0)
                    ELSE NVL(SUM(RPO.QUANTITY/IUS.CONVERSION_FACTOR),0)
                END TOTAL_QUANTITY_case,
                SUM(RPO.TOTAL_PRICE) TOTAL_AMOUNT,IUS.MU_CODE
               FROM DIST_IP_SSR_PURCHASE_ORDER RPO
               INNER JOIN IP_ITEM_UNIT_SETUP IUS ON RPO.ITEM_CODE = IUS.ITEM_CODE AND RPO.COMPANY_CODE = IUS.COMPANY_CODE
               INNER JOIN DIST_LOGIN_USER DLU ON DLU.USERID = RPO.CREATED_BY AND DLU.COMPANY_CODE = RPO.COMPANY_CODE AND DLU.SP_CODE = '{SpCode}'
               INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = RPO.COMPANY_CODE
               INNER JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = RPO.RESELLER_CODE AND DRM.COMPANY_CODE = RPO.COMPANY_CODE
               LEFT JOIN ip_item_master_setup ISS ON ISS.ITEM_CODE = RPO.ITEM_CODE AND ISS.COMPANY_CODE = RPO.COMPANY_CODE
               WHERE TRUNC(RPO.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}', 'RRRR-MON-DD') AND TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
               AND DRM.IS_CLOSED = 'N'
               AND ISS.DELETED_FLAG = 'N'
               AND RPO.COMPANY_CODE IN ('{userInfo.company_code}')
                AND    ISS.DELETED_FLAG='N' AND ISS.GROUP_SKU_FLAG='I'
                 GROUP BY RPO.CREATED_BY, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), RPO.COMPANY_CODE, TRIM(ISS.ITEM_EDESC),RPO.MU_CODE,IUS.MU_CODE,ISS.INDEX_MU_CODE 
                ) PO
                GROUP BY PO.BRAND_NAME,PO.MU_CODE";

            var data = _objectEntity.SqlQuery<EmpBrandWiseModel>(brandQuery).ToList();
            return data;
        }
        public List<EmployeeWisePerformance> VisitedOrderList(ReportFiltersModel model, User userInfo, string SP_CODE)
        {
            var companyCode = string.Join("','", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;
            string query = $@"SELECT RPO.SP_CODE, RPO.EMPLOYEE_EDESC, RPO.ENTITY_CODE, RPO.ENTITY_NAME, RPO.LATITUDE, RPO.LONGITUDE, 'R' ENTITY_TYPE, 'PJP' JOURNEY_PLAN, RPO.VISITED, RPO.COMPANY_CODE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.RESELLER_CODE ENTITY_CODE, TRIM(D.RESELLER_NAME) ENTITY_NAME, D.LATITUDE, D.LONGITUDE, (CASE WHEN E.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END) VISITED, A.COMPANY_CODE
                            FROM DIST_IP_SSR_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.SP_CODE = '{SP_CODE}'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN DIST_RESELLER_MASTER D ON D.RESELLER_CODE = A.RESELLER_CODE AND D.COMPANY_CODE = A.COMPANY_CODE
                            LEFT JOIN DIST_LOCATION_TRACK E ON E.CUSTOMER_CODE = A.RESELLER_CODE AND E.COMPANY_CODE = A.COMPANY_CODE AND E.CUSTOMER_TYPE = 'R' AND TRUNC(E.UPDATE_DATE) = TRUNC(TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
                            WHERE TRUNC(A.ORDER_DATE) = TRUNC(TO_DATE('{model.ToDate}', 'RRRR-MON-DD')) AND D.IS_CLOSED = 'N'
                            GROUP BY A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC), A.RESELLER_CODE, TRIM(D.RESELLER_NAME), D.LATITUDE, D.LONGITUDE, (CASE WHEN E.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END), A.COMPANY_CODE
                      ) RPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TRUNC(TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y'
                                  WHERE 1 = 1
                                    AND DRM.IS_CLOSED = 'N'
                                    AND DRD.EMP_CODE = '{SP_CODE}'
                                    AND DLU.COMPANY_CODE in ('{companyCode}')
                                  GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                      ) PJPENT ON PJPENT.USERID = RPO.CREATED_BY AND PJPENT.SP_CODE = RPO.SP_CODE AND PJPENT.ENTITY_CODE = RPO.ENTITY_CODE AND PJPENT.COMPANY_CODE = RPO.COMPANY_CODE
                      WHERE 1 = 1
                        AND PJPENT.SP_CODE IS NOT NULL
                      GROUP BY RPO.SP_CODE, RPO.EMPLOYEE_EDESC, RPO.ENTITY_CODE, RPO.ENTITY_NAME, RPO.LATITUDE, RPO.LONGITUDE, 'R', 'PJP', RPO.VISITED, RPO.COMPANY_CODE
UNION ALL
----- PJP ORDER DISTRIBUTOR ENTITIES -----
SELECT DPO.SP_CODE, DPO.EMPLOYEE_EDESC, DPO.ENTITY_CODE, DPO.ENTITY_NAME, DPO.LATITUDE, DPO.LONGITUDE, 'D' ENTITY_TYPE, 'PJP' JOURNEY_PLAN, DPO.VISITED, DPO.COMPANY_CODE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.CUSTOMER_CODE ENTITY_CODE, TRIM(E.CUSTOMER_EDESC) ENTITY_NAME, D.LATITUDE, D.LONGITUDE, (CASE WHEN F.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END) VISITED, A.COMPANY_CODE
                            FROM DIST_IP_SSD_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.SP_CODE = '{SP_CODE}'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN DIST_DISTRIBUTOR_MASTER D ON D.DISTRIBUTOR_CODE = A.CUSTOMER_CODE AND D.COMPANY_CODE = A.COMPANY_CODE AND D.ACTIVE = 'Y'
                            INNER JOIN SA_CUSTOMER_SETUP E ON E.CUSTOMER_CODE = D.DISTRIBUTOR_CODE AND E.COMPANY_CODE = D.COMPANY_CODE
                            LEFT JOIN DIST_LOCATION_TRACK F ON F.CUSTOMER_CODE = A.CUSTOMER_CODE AND F.COMPANY_CODE = A.COMPANY_CODE AND F.CUSTOMER_TYPE = 'D' AND TRUNC(F.UPDATE_DATE) = TRUNC(TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
                            WHERE TRUNC(A.ORDER_DATE) = TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
                            GROUP BY A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC), A.CUSTOMER_CODE, TRIM(E.CUSTOMER_EDESC), D.LATITUDE, D.LONGITUDE, (CASE WHEN F.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END), A.COMPANY_CODE
                      ) DPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TRUNC(TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                                  GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                      ) PJPENT ON PJPENT.USERID = DPO.CREATED_BY AND PJPENT.SP_CODE = DPO.SP_CODE AND PJPENT.ENTITY_CODE = DPO.ENTITY_CODE AND PJPENT.COMPANY_CODE = DPO.COMPANY_CODE
                      WHERE 1 = 1
                        AND PJPENT.SP_CODE IS NOT NULL
                      GROUP BY DPO.SP_CODE, DPO.EMPLOYEE_EDESC, DPO.ENTITY_CODE, DPO.ENTITY_NAME, DPO.LATITUDE, DPO.LONGITUDE, 'D', 'PJP', DPO.VISITED, DPO.COMPANY_CODE
UNION ALL
----- PJP ORDER PARTY TYPE ENTITIES -----
SELECT PPO.SP_CODE, PPO.EMPLOYEE_EDESC, PPO.ENTITY_CODE, PPO.ENTITY_NAME, PPO.LATITUDE, PPO.LONGITUDE, 'P' ENTITY_TYPE, 'PJP' JOURNEY_PLAN, PPO.VISITED, PPO.COMPANY_CODE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.PARTY_TYPE_CODE ENTITY_CODE, TRIM(E.PARTY_TYPE_EDESC) ENTITY_NAME, D.LATITUDE, D.LONGITUDE, (CASE WHEN F.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END) VISITED, A.COMPANY_CODE
                            FROM DIST_IP_SSD_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.ACTIVE = 'Y'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN DIST_DEALER_MASTER D ON D.DEALER_CODE = A.PARTY_TYPE_CODE AND D.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN IP_PARTY_TYPE_CODE E ON E.PARTY_TYPE_CODE = D.DEALER_CODE AND E.COMPANY_CODE = D.COMPANY_CODE
                            LEFT JOIN DIST_LOCATION_TRACK F ON F.CUSTOMER_CODE = A.CUSTOMER_CODE AND F.COMPANY_CODE = A.COMPANY_CODE AND F.CUSTOMER_TYPE = 'P' AND TRUNC(F.UPDATE_DATE) = TRUNC(TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
                            WHERE TRUNC(A.ORDER_DATE) = TRUNC(TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
                            GROUP BY A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC), A.PARTY_TYPE_CODE, TRIM(E.PARTY_TYPE_EDESC), D.LATITUDE, D.LONGITUDE, (CASE WHEN F.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END), A.COMPANY_CODE
                      ) PPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TRUNC(TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                                  GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                      ) PJPENT ON PJPENT.USERID = PPO.CREATED_BY AND PJPENT.SP_CODE = PPO.SP_CODE AND PJPENT.ENTITY_CODE = PPO.ENTITY_CODE AND PJPENT.COMPANY_CODE = PPO.COMPANY_CODE
                      WHERE 1 = 1
                        AND PJPENT.SP_CODE IS NOT NULL
                      GROUP BY PPO.SP_CODE, PPO.EMPLOYEE_EDESC, PPO.ENTITY_CODE, PPO.ENTITY_NAME, PPO.LATITUDE, PPO.LONGITUDE, 'P', 'PJP', PPO.VISITED, PPO.COMPANY_CODE";
            var data = _objectEntity.SqlQuery<EmployeeWisePerformance>(query).ToList();
            return data;
        }
        public List<EmployeeWisePerformance> GetEmployeeNonProductive(ReportFiltersModel model, User userInfo, string SP_CODE)
        {
            var companyCode = string.Join("','", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            string query = $@"SELECT RPO.SP_CODE, RPO.EMPLOYEE_EDESC, RPO.ENTITY_CODE, RPO.ENTITY_NAME, RPO.LATITUDE, RPO.LONGITUDE, 'R' ENTITY_TYPE, 'NJP' JOURNEY_PLAN, RPO.VISITED, RPO.COMPANY_CODE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.RESELLER_CODE ENTITY_CODE, TRIM(D.RESELLER_NAME) ENTITY_NAME, D.LATITUDE, D.LONGITUDE, (CASE WHEN E.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END) VISITED, A.COMPANY_CODE
                            FROM DIST_IP_SSR_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.SP_CODE = '{SP_CODE}'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN DIST_RESELLER_MASTER D ON D.RESELLER_CODE = A.RESELLER_CODE AND D.COMPANY_CODE = A.COMPANY_CODE
                            LEFT JOIN DIST_LOCATION_TRACK E ON E.CUSTOMER_CODE = A.RESELLER_CODE AND E.COMPANY_CODE = A.COMPANY_CODE AND E.CUSTOMER_TYPE = 'R' AND TRUNC(E.UPDATE_DATE) = TRUNC(TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
                            WHERE TRUNC(A.ORDER_DATE) = TRUNC(TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
                            AND D.IS_CLOSED = 'N'
                            GROUP BY A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC), A.RESELLER_CODE, TRIM(D.RESELLER_NAME), D.LATITUDE, D.LONGITUDE, (CASE WHEN E.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END), A.COMPANY_CODE
                      ) RPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TRUNC(TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y'
                                  WHERE 1 = 1
                                    AND DRM.IS_CLOSED = 'N'
                                    AND DRD.EMP_CODE = '{SP_CODE}'
                                    AND DLU.COMPANY_CODE IN ('{companyCode}')
                                  GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                      ) NPJPENT ON NPJPENT.USERID = RPO.CREATED_BY AND NPJPENT.SP_CODE = RPO.SP_CODE AND NPJPENT.ENTITY_CODE = RPO.ENTITY_CODE AND NPJPENT.COMPANY_CODE = RPO.COMPANY_CODE
                      WHERE 1 = 1
                        AND NPJPENT.SP_CODE IS NULL
                      GROUP BY RPO.SP_CODE, RPO.EMPLOYEE_EDESC, RPO.ENTITY_CODE, RPO.ENTITY_NAME, RPO.LATITUDE, RPO.LONGITUDE, 'R', 'NJP', RPO.VISITED, RPO.COMPANY_CODE
UNION ALL
----- NON PJP ORDER DISTRIBUTOR ENTITIES -----
SELECT DPO.SP_CODE, DPO.EMPLOYEE_EDESC, DPO.ENTITY_CODE, DPO.ENTITY_NAME, DPO.LATITUDE, DPO.LONGITUDE, 'D' ENTITY_TYPE, 'NPJP' JOURNEY_PLAN, DPO.VISITED, DPO.COMPANY_CODE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.CUSTOMER_CODE ENTITY_CODE, TRIM(E.CUSTOMER_EDESC) ENTITY_NAME, D.LATITUDE, D.LONGITUDE, (CASE WHEN F.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END) VISITED, A.COMPANY_CODE
                            FROM DIST_IP_SSD_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.SP_CODE = '{SP_CODE}'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN DIST_DISTRIBUTOR_MASTER D ON D.DISTRIBUTOR_CODE = A.CUSTOMER_CODE AND D.COMPANY_CODE = A.COMPANY_CODE AND D.ACTIVE = 'Y'
                            INNER JOIN SA_CUSTOMER_SETUP E ON E.CUSTOMER_CODE = D.DISTRIBUTOR_CODE AND E.COMPANY_CODE = D.COMPANY_CODE
                            LEFT JOIN DIST_LOCATION_TRACK F ON F.CUSTOMER_CODE = A.CUSTOMER_CODE AND F.COMPANY_CODE = A.COMPANY_CODE AND F.CUSTOMER_TYPE = 'D' AND TRUNC(F.UPDATE_DATE) = TRUNC(TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
                            WHERE TRUNC(A.ORDER_DATE) = TO_DATE('{model.ToDate}', 'RRRR-MON-DD')
                            GROUP BY A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC), A.CUSTOMER_CODE, TRIM(E.CUSTOMER_EDESC), D.LATITUDE, D.LONGITUDE, (CASE WHEN F.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END), A.COMPANY_CODE
                      ) DPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TRUNC(TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                                  GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                      ) NPJPENT ON NPJPENT.USERID = DPO.CREATED_BY AND NPJPENT.SP_CODE = DPO.SP_CODE AND NPJPENT.ENTITY_CODE = DPO.ENTITY_CODE AND NPJPENT.COMPANY_CODE = DPO.COMPANY_CODE
                      WHERE 1 = 1
                        AND NPJPENT.SP_CODE IS NULL
                      GROUP BY DPO.SP_CODE, DPO.EMPLOYEE_EDESC, DPO.ENTITY_CODE, DPO.ENTITY_NAME, DPO.LATITUDE, DPO.LONGITUDE, 'D', 'NPJP', DPO.VISITED, DPO.COMPANY_CODE
UNION ALL
----- NON PJP ORDER PARTY TYPE ENTITIES -----
SELECT PPO.SP_CODE, PPO.EMPLOYEE_EDESC, PPO.ENTITY_CODE, PPO.ENTITY_NAME, PPO.LATITUDE, PPO.LONGITUDE, 'P' ENTITY_TYPE, 'NPJP' JOURNEY_PLAN, PPO.VISITED, PPO.COMPANY_CODE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.PARTY_TYPE_CODE ENTITY_CODE, TRIM(E.PARTY_TYPE_EDESC) ENTITY_NAME, D.LATITUDE, D.LONGITUDE, (CASE WHEN F.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END) VISITED, A.COMPANY_CODE
                            FROM DIST_IP_SSD_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.ACTIVE = 'Y'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN DIST_DEALER_MASTER D ON D.DEALER_CODE = A.PARTY_TYPE_CODE AND D.COMPANY_CODE = A.COMPANY_CODE
                            INNER JOIN IP_PARTY_TYPE_CODE E ON E.PARTY_TYPE_CODE = D.DEALER_CODE AND E.COMPANY_CODE = D.COMPANY_CODE
                            LEFT JOIN DIST_LOCATION_TRACK F ON F.CUSTOMER_CODE = A.CUSTOMER_CODE AND F.COMPANY_CODE = A.COMPANY_CODE AND F.CUSTOMER_TYPE = 'P' AND TRUNC(F.UPDATE_DATE) = TRUNC(TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
                            WHERE TRUNC(A.ORDER_DATE) = TRUNC(TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
                            GROUP BY A.CREATED_BY, B.SP_CODE, TRIM(C.EMPLOYEE_EDESC), A.PARTY_TYPE_CODE, TRIM(E.PARTY_TYPE_EDESC), D.LATITUDE, D.LONGITUDE, (CASE WHEN F.CUSTOMER_CODE IS NULL THEN 'N' ELSE 'Y' END), A.COMPANY_CODE
                      ) PPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TRUNC(TO_DATE('{model.ToDate}', 'RRRR-MON-DD'))
                                  LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                                  LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                                  GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                      ) NPJPENT ON NPJPENT.USERID = PPO.CREATED_BY AND NPJPENT.SP_CODE = PPO.SP_CODE AND NPJPENT.ENTITY_CODE = PPO.ENTITY_CODE AND NPJPENT.COMPANY_CODE = PPO.COMPANY_CODE
                      WHERE 1 = 1
                        AND NPJPENT.SP_CODE IS NULL
                      GROUP BY PPO.SP_CODE, PPO.EMPLOYEE_EDESC, PPO.ENTITY_CODE, PPO.ENTITY_NAME, PPO.LATITUDE, PPO.LONGITUDE, 'P', 'NPJP', PPO.VISITED, PPO.COMPANY_CODE";
            var data = _objectEntity.SqlQuery<EmployeeWisePerformance>(query).ToList();
            return data;
        }



        public List<PurchaseOrderReportModel> GetPurchaseOrderList(ReportFiltersModel model, string requestStatus, User userInfo)
        {
            var customerFilter = string.Empty;
            var SalesPersonFilter = string.Empty;
            var flagFilter = string.Empty;
            if (requestStatus == "Approved")
                flagFilter = @" AND DPO1.approved_flag = 'Y'
                               AND DPO1.REJECT_FLAG = 'N'";
            else if (requestStatus == "Rejected")
                flagFilter = @" AND DPO1.REJECT_FLAG = 'Y'
                                AND DPO1.approved_flag = 'N'";
            else if (requestStatus == "All")
            {
                flagFilter = "";
            }
            else if (requestStatus == "Active")
            {
                flagFilter = @" AND DPO1.REJECT_FLAG = 'N'
                                AND DPO1.APPROVED_FLAG = 'N'";
            }
            if (model.CustomerFilter.Count() > 0)
            {
                customerFilter = @"select  DISTINCT(customer_code) from sa_customer_setup where (";
                //IF CUSTOMER_SKU_FLAG = G
                foreach (var item in model.CustomerFilter)
                {
                    customerFilter += "master_customer_code like  (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + userInfo.company_code + ")) OR ";
                }
                customerFilter = customerFilter.Substring(0, customerFilter.Length - 3);
                //IF CUSTOMER_SKU_FLAG = I                
                customerFilter += " or (customer_code in (" + string.Join(",", model.CustomerFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + userInfo.company_code + "))) ";


                customerFilter = " and CS.customer_code IN(" + customerFilter + ")";
            }
            if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                SalesPersonFilter = $" AND  ES.EMPLOYEE_CODE IN ({userInfo.sp_codes})";
            }
            string query = $@"SELECT DPO1.ORDER_NO,DPO1.ORDER_DATE, DPO1.CUSTOMER_CODE,DPO1.BILLING_NAME CUSTOMER_EDESC, RM.RESELLER_NAME, 'R' ORDER_ENTITY,
                                     DPO1.PARTY_TYPE_CODE,
                                     (CASE WHEN DPO1.PARTY_TYPE_CODE IS NULL
                                        THEN FN_FETCH_DESC (DPO1.COMPANY_CODE,'IP_PARTY_TYPE_CODE',CS.PARTY_TYPE_CODE)
                                        ELSE FN_FETCH_DESC (DPO1.COMPANY_CODE,'IP_PARTY_TYPE_CODE',DPO1.PARTY_TYPE_CODE)
                                      END
                                     ) PARTY_TYPE_EDESC,
                                      (CASE DPO1.DISPATCH_FROM
                                        WHEN 'D' THEN 'Distributor'
                                        WHEN 'W' THEN 'Wholeseller'
                                        ELSE NULL
                                      END) DISPATCH_FROM,
                                     DPO1.CREATED_BY, DPO1.CREATED_DATE, DPO1.DELETED_FLAG,
                                     DPO1.COMPANY_CODE, DPO1.BRANCH_CODE,
                                     DPO1.APPROVED_FLAG, DPO1.DISPATCH_FLAG, DPO1.ACKNOWLEDGE_FLAG, DPO1.REJECT_FLAG,
                                     ES.EMPLOYEE_EDESC,
                                     PS.PO_PARTY_TYPE,
                                     PS.PO_CONVERSION_FACTOR,
                                     PS.PO_BILLING_NAME,
                                     PS.SO_CREDIT_LIMIT_CHK SO_CREDIT_LIMIT_FLAG,
                                     CS.CREDIT_LIMIT,
                                     --VSL.BALANCE,
                                      NVL(DPO2.TOTAL_QUANTITY,0) QUANTITY,
                                     DPO2.TOTAL_AMOUNT GrantTotalAmount,
                                     NVL(DPO2.TOTAL_APPROVE_QTY,0) GRAND_APPROVE_QUENTITY,
                                     NVL(DPO2.TOTAL_APPROVE_AMT,0) TOTAL_APPROVE_AMT,DPO1.REMARKS_REVIEW,
                                     (SELECT RESELLER_NAME FROM DIST_RESELLER_MASTER WHERE RESELLER_CODE=DPO1.WHOLESELLER_CODE AND IS_CLOSED = 'N') WHOLESELLER_EDESC
                            FROM DIST_IP_SSR_PURCHASE_ORDER DPO1
                            INNER JOIN DIST_RESELLER_MASTER RM ON RM.RESELLER_CODE = DPO1.RESELLER_CODE
                            INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DPO1.ITEM_CODE AND IMS.COMPANY_CODE = DPO1.COMPANY_CODE AND IMS.CATEGORY_CODE = 'FG' AND IMS.GROUP_SKU_FLAG = 'I'
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
                                        WHERE A.CUSTOMER_CODE = '{userInfo.DistributerNo}'
                                        --ORDER BY A.ORDER_NO DESC, A.ITEM_CODE
                            ) POT
                           GROUP BY POT.ORDER_NO) DPO2 ON DPO2.ORDER_NO = DPO1.ORDER_NO
                            WHERE 1 = 1
                              AND DPO1.CUSTOMER_CODE = '{userInfo.DistributerNo}' {SalesPersonFilter}{customerFilter}
                               --AND DPO1.ORDER_DATE >= TO_DATE('2020-Sep-06','YYYY-MM-DD') AND DPO1.ORDER_DATE <= TO_DATE('2020-Sep-06','YYYY-MM-DD')
                               AND TRUNC(DPO1.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}','YYYY-MM-DD') AND TO_DATE('{model.ToDate}','YYYY-MM-DD')
                              AND DPO1.DELETED_FLAG = 'N'
                              AND RM.IS_CLOSED = 'N'
                           {flagFilter}
                              AND DPO1.COMPANY_CODE IN ('{userInfo.company_code}') AND DPO1.BRANCH_CODE IN('{userInfo.branch_code}') AND   DPO1.DISPATCH_FROM='D'
                            GROUP BY DPO1.ORDER_NO, DPO1.ORDER_DATE, DPO1.CUSTOMER_CODE, DPO1.BILLING_NAME, RM.RESELLER_NAME, 'R',
                                     DPO1.PARTY_TYPE_CODE,
                                     (CASE WHEN DPO1.PARTY_TYPE_CODE IS NULL
                                        THEN FN_FETCH_DESC (DPO1.COMPANY_CODE,'IP_PARTY_TYPE_CODE',CS.PARTY_TYPE_CODE)
                                        ELSE FN_FETCH_DESC (DPO1.COMPANY_CODE,'IP_PARTY_TYPE_CODE',DPO1.PARTY_TYPE_CODE)
                                      END
                                     ),
                                      (CASE DPO1.DISPATCH_FROM
                                        WHEN 'D' THEN 'Distributor'
                                        WHEN 'W' THEN 'Wholeseller'
                                        ELSE NULL
                                      END),
                                     DPO1.CREATED_BY, DPO1.CREATED_DATE, DPO1.DELETED_FLAG,
                                     DPO1.COMPANY_CODE, DPO1.BRANCH_CODE,
                                     DPO1.APPROVED_FLAG, DPO1.DISPATCH_FLAG, DPO1.ACKNOWLEDGE_FLAG, DPO1.REJECT_FLAG,
                                     ES.EMPLOYEE_EDESC,
                                     PS.PO_PARTY_TYPE,
                                     PS.PO_CONVERSION_FACTOR,
                                     PS.PO_BILLING_NAME,
                                     PS.SO_CREDIT_LIMIT_CHK,
                                     CS.CREDIT_LIMIT,
                                     --VSL.BALANCE,
                                     DPO2.TOTAL_QUANTITY,
                                     DPO2.TOTAL_AMOUNT,
                                     DPO2.TOTAL_APPROVE_QTY,
                                     DPO2.TOTAL_APPROVE_AMT,DPO1.REMARKS_REVIEW,
                                     DPO1.WHOLESELLER_CODE
                            ORDER BY DPO1.ORDER_NO DESC";
            var result = _objectEntity.SqlQuery<PurchaseOrderReportModel>(query).ToList();
            return result;
        }

        public List<PurchaseOrderReportModel> GetWholeSellerPurchaseOrderReport(ReportFiltersModel model, string requestStatus, User userInfo)
        {
            var customerFilter = string.Empty;
            var SalesPersonFilter = string.Empty;
            var flagFilter = string.Empty;
            if (requestStatus == "Approved")
                flagFilter = @" AND DPO1.approved_flag = 'Y'
                               AND DPO1.REJECT_FLAG = 'N'";
            else if (requestStatus == "Rejected")
                flagFilter = @" AND DPO1.REJECT_FLAG = 'Y'
                                AND DPO1.approved_flag = 'N'";
            else if (requestStatus == "All")
            {
                flagFilter = "";
            }
            else if (requestStatus == "Active")
            {
                flagFilter = @" AND DPO1.REJECT_FLAG = 'N'
                                AND DPO1.APPROVED_FLAG = 'N'";
            }
            if (model.CustomerFilter.Count() > 0)
            {
                customerFilter = @"select  DISTINCT(customer_code) from sa_customer_setup where (";
                //IF CUSTOMER_SKU_FLAG = G
                foreach (var item in model.CustomerFilter)
                {
                    customerFilter += "master_customer_code like  (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + userInfo.company_code + ")) OR ";
                }
                customerFilter = customerFilter.Substring(0, customerFilter.Length - 3);
                //IF CUSTOMER_SKU_FLAG = I                
                customerFilter += " or (customer_code in (" + string.Join(",", model.CustomerFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + userInfo.company_code + "))) ";


                customerFilter = " and CS.customer_code IN(" + customerFilter + ")";
            }
            if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                SalesPersonFilter = $" AND ES.EMPLOYEE_CODE IN ({userInfo.sp_codes})";
            }
            string query = $@"SELECT DPO1.ORDER_NO,DPO1.ORDER_DATE, DPO1.CUSTOMER_CODE,DPO1.BILLING_NAME CUSTOMER_EDESC, RM.RESELLER_NAME, 'R' ORDER_ENTITY,
                                     DPO1.PARTY_TYPE_CODE,
                                     (CASE WHEN DPO1.PARTY_TYPE_CODE IS NULL
                                        THEN FN_FETCH_DESC (DPO1.COMPANY_CODE,'IP_PARTY_TYPE_CODE',CS.PARTY_TYPE_CODE)
                                        ELSE FN_FETCH_DESC (DPO1.COMPANY_CODE,'IP_PARTY_TYPE_CODE',DPO1.PARTY_TYPE_CODE)
                                      END
                                     ) PARTY_TYPE_EDESC,
                                      (CASE DPO1.DISPATCH_FROM
                                        WHEN 'D' THEN 'Distributor'
                                        WHEN 'W' THEN 'Wholeseller'
                                        ELSE NULL
                                      END) DISPATCH_FROM,
                                     DPO1.CREATED_BY, DPO1.CREATED_DATE, DPO1.DELETED_FLAG,
                                     DPO1.COMPANY_CODE, DPO1.BRANCH_CODE,
                                     DPO1.APPROVED_FLAG, DPO1.DISPATCH_FLAG, DPO1.ACKNOWLEDGE_FLAG, DPO1.REJECT_FLAG,
                                     ES.EMPLOYEE_EDESC,
                                     PS.PO_PARTY_TYPE,
                                     PS.PO_CONVERSION_FACTOR,
                                     PS.PO_BILLING_NAME,
                                     PS.SO_CREDIT_LIMIT_CHK SO_CREDIT_LIMIT_FLAG,
                                     CS.CREDIT_LIMIT,
                                     --VSL.BALANCE,
                                      NVL(DPO2.TOTAL_QUANTITY,0) QUANTITY,
                                     DPO2.TOTAL_AMOUNT GrantTotalAmount,
                                     NVL(DPO2.TOTAL_APPROVE_QTY,0) GRAND_APPROVE_QUENTITY,
                                     NVL(DPO2.TOTAL_APPROVE_AMT,0) TOTAL_APPROVE_AMT,DPO1.REMARKS_REVIEW,
                                     (SELECT RESELLER_NAME FROM DIST_RESELLER_MASTER WHERE RESELLER_CODE=DPO1.WHOLESELLER_CODE AND IS_CLOSED = 'N') WHOLESELLER_EDESC
                            FROM DIST_IP_SSR_PURCHASE_ORDER DPO1
                            INNER JOIN DIST_RESELLER_MASTER RM ON RM.RESELLER_CODE = DPO1.RESELLER_CODE
                            INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DPO1.ITEM_CODE AND IMS.COMPANY_CODE = DPO1.COMPANY_CODE AND IMS.CATEGORY_CODE = 'FG' AND IMS.GROUP_SKU_FLAG = 'I'
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
                                        WHERE A.CUSTOMER_CODE = '{userInfo.DistributerNo}'
                                        --ORDER BY A.ORDER_NO DESC, A.ITEM_CODE
                            ) POT
                           GROUP BY POT.ORDER_NO) DPO2 ON DPO2.ORDER_NO = DPO1.ORDER_NO
                            WHERE 1 = 1
                              AND DPO1.CUSTOMER_CODE = '{userInfo.DistributerNo}' AND RM.IS_CLOSED = 'N' {SalesPersonFilter}{customerFilter}
                              AND DPO1.ORDER_DATE >= TO_DATE('{model.FromDate}','YYYY-MM-DD') AND DPO1.ORDER_DATE <= TO_DATE('{model.ToDate}','YYYY-MM-DD')
                              AND DPO1.DELETED_FLAG = 'N'
                           {flagFilter}
                              AND DPO1.COMPANY_CODE IN ('06') AND DPO1.BRANCH_CODE IN('06.01') AND   DPO1.DISPATCH_FROM='W'
                            GROUP BY DPO1.ORDER_NO, DPO1.ORDER_DATE, DPO1.CUSTOMER_CODE, DPO1.BILLING_NAME, RM.RESELLER_NAME, 'R',
                                     DPO1.PARTY_TYPE_CODE,
                                     (CASE WHEN DPO1.PARTY_TYPE_CODE IS NULL
                                        THEN FN_FETCH_DESC (DPO1.COMPANY_CODE,'IP_PARTY_TYPE_CODE',CS.PARTY_TYPE_CODE)
                                        ELSE FN_FETCH_DESC (DPO1.COMPANY_CODE,'IP_PARTY_TYPE_CODE',DPO1.PARTY_TYPE_CODE)
                                      END
                                     ),
                                      (CASE DPO1.DISPATCH_FROM
                                        WHEN 'D' THEN 'Distributor'
                                        WHEN 'W' THEN 'Wholeseller'
                                        ELSE NULL
                                      END),
                                     DPO1.CREATED_BY, DPO1.CREATED_DATE, DPO1.DELETED_FLAG,
                                     DPO1.COMPANY_CODE, DPO1.BRANCH_CODE,
                                     DPO1.APPROVED_FLAG, DPO1.DISPATCH_FLAG, DPO1.ACKNOWLEDGE_FLAG, DPO1.REJECT_FLAG,
                                     ES.EMPLOYEE_EDESC,
                                     PS.PO_PARTY_TYPE,
                                     PS.PO_CONVERSION_FACTOR,
                                     PS.PO_BILLING_NAME,
                                     PS.SO_CREDIT_LIMIT_CHK,
                                     CS.CREDIT_LIMIT,
                                     --VSL.BALANCE,
                                     DPO2.TOTAL_QUANTITY,
                                     DPO2.TOTAL_AMOUNT,
                                     DPO2.TOTAL_APPROVE_QTY,
                                     DPO2.TOTAL_APPROVE_AMT,DPO1.REMARKS_REVIEW,
                                     DPO1.WHOLESELLER_CODE
                            ORDER BY DPO1.ORDER_NO DESC";
            var result = _objectEntity.SqlQuery<PurchaseOrderReportModel>(query).ToList();
            return result;
        }

        public List<PurchaseOrderReportModel> GetResellerPurchaseOrderList(ReportFiltersModel model, string requestStatus, User userInfo)
        {
            var flagFilter = string.Empty;
            var custFilter = string.Empty;
            var salesPersonFilter = string.Empty;
            if (model.CustomerFilter.Count() > 0)
            {
                custFilter = @"select  DISTINCT(customer_code) from sa_customer_setup where (";
                //IF CUSTOMER_SKU_FLAG = G
                foreach (var item in model.CustomerFilter)
                {
                    custFilter += "master_customer_code like  (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + userInfo.company_code + ")) OR ";
                }
                custFilter = custFilter.Substring(0, custFilter.Length - 3);
                //IF CUSTOMER_SKU_FLAG = I                
                custFilter += " or (customer_code in (" + string.Join(",", model.CustomerFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + userInfo.company_code + "))) ";


                custFilter = " and CS.customer_code IN(" + custFilter + ")";
            }
            if (model.ItemBrandFilter.Count > 0)
                salesPersonFilter = $" AND ES.EMPLOYEE_CODE IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                salesPersonFilter = $" AND ES.EMPLOYEE_CODE IN ({userInfo.sp_codes})";
            }

            if (requestStatus == "Approved")
                flagFilter = @" AND DPO1.approved_flag = 'Y'
                               AND DPO1.REJECT_FLAG = 'N'";
            else if (requestStatus == "Rejected")
                flagFilter = @" AND DPO1.REJECT_FLAG = 'Y'
                                AND DPO1.approved_flag = 'N'";
            else if (requestStatus == "All")
            {
                flagFilter = "";
            }
            else if (requestStatus == "Active")
            {
                flagFilter = @" AND DPO1.REJECT_FLAG = 'N'
                                AND DPO1.APPROVED_FLAG = 'N'";
            }


            var BranchFilter = string.Empty;
            if (model.BranchFilter.Count > 0)
            {
                BranchFilter = string.Format(@" AND DPO1.BRANCH_CODE IN  ('{0}')", string.Join("','", model.BranchFilter).ToString());

            }

            string customerFilter = "";
            if (userInfo.LoginType == "Distributor")
            {
                customerFilter = $" AND DPO1.CUSTOMER_CODE = '{userInfo.DistributerNo}'";
            }
            string query = $@"SELECT DPO1.ORDER_NO,DPO1.ORDER_DATE,BS_DATE(TO_CHAR(DPO1.ORDER_DATE)) MITI, DPO1.CUSTOMER_CODE,DPO1.BILLING_NAME CUSTOMER_EDESC, RM.RESELLER_NAME, 'R' ORDER_ENTITY,
                                     DPO1.PARTY_TYPE_CODE,
                                     (CASE WHEN DPO1.PARTY_TYPE_CODE IS NULL
                                        THEN FN_FETCH_DESC (DPO1.COMPANY_CODE,'IP_PARTY_TYPE_CODE',CS.PARTY_TYPE_CODE)
                                        ELSE FN_FETCH_DESC (DPO1.COMPANY_CODE,'IP_PARTY_TYPE_CODE',DPO1.PARTY_TYPE_CODE)
                                      END
                                     ) PARTY_TYPE_EDESC,
                                     (CASE DPO1.DISPATCH_FROM
                                        WHEN 'D' THEN 'Distributor'
                                        WHEN 'W' THEN 'Wholeseller'
                                        ELSE NULL
                                      END) DISPATCH_FROM,
                                     DPO1.CREATED_BY, DPO1.CREATED_DATE, DPO1.DELETED_FLAG,
                                     DPO1.COMPANY_CODE, DPO1.BRANCH_CODE,
                                     DPO1.APPROVED_FLAG, DPO1.DISPATCH_FLAG, DPO1.ACKNOWLEDGE_FLAG, DPO1.REJECT_FLAG,
                                     ES.EMPLOYEE_EDESC,
                                     PS.PO_PARTY_TYPE,
                                     PS.PO_CONVERSION_FACTOR,
                                     PS.PO_BILLING_NAME,
                                     PS.SO_CREDIT_LIMIT_CHK SO_CREDIT_LIMIT_FLAG,
                                     CS.CREDIT_LIMIT,
                                     --VSL.BALANCE,
                                      NVL(DPO2.TOTAL_QUANTITY,0) QUANTITY,
                                     DPO2.TOTAL_AMOUNT GrantTotalAmount,
                                     NVL(DPO2.TOTAL_APPROVE_QTY,0) GRAND_APPROVE_QUENTITY,
                                     NVL(DPO2.TOTAL_APPROVE_AMT,0) TOTAL_APPROVE_AMT,DPO1.REMARKS_REVIEW,
                                     (SELECT RESELLER_NAME FROM DIST_RESELLER_MASTER WHERE RESELLER_CODE=DPO1.WHOLESELLER_CODE AND IS_CLOSED = 'N') WHOLESELLER_EDESC
                            FROM DIST_IP_SSR_PURCHASE_ORDER DPO1
                            INNER JOIN DIST_RESELLER_MASTER RM ON RM.RESELLER_CODE = DPO1.RESELLER_CODE
                            INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DPO1.ITEM_CODE AND IMS.COMPANY_CODE = DPO1.COMPANY_CODE AND IMS.CATEGORY_CODE in (select CATEGORY_CODE from IP_CATEGORY_CODE WHERE CATEGORY_TYPE IN ('FG','TF') AND COMPANY_CODE='{userInfo.company_code}') AND IMS.GROUP_SKU_FLAG = 'I'
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
                                        WHERE 1=1 {customerFilter}
                                        --ORDER BY A.ORDER_NO DESC, A.ITEM_CODE
                            ) POT
                           GROUP BY POT.ORDER_NO) DPO2 ON DPO2.ORDER_NO = DPO1.ORDER_NO
                            WHERE 1 = 1
                              AND TRUNC(DPO1.ORDER_DATE) >= TO_DATE('{model.FromDate}','YYYY-MM-DD') AND TRUNC(DPO1.ORDER_DATE) <= TO_DATE('{model.ToDate}','YYYY-MM-DD')
                              AND DPO1.DELETED_FLAG = 'N' AND RM.IS_CLOSED = 'N' {custFilter} {salesPersonFilter}
                           {flagFilter}
                              AND DPO1.COMPANY_CODE IN ('{userInfo.company_code}') {BranchFilter}
                            GROUP BY DPO1.ORDER_NO, DPO1.ORDER_DATE,BS_DATE(TO_CHAR(DPO1.ORDER_DATE)), DPO1.CUSTOMER_CODE, DPO1.BILLING_NAME, RM.RESELLER_NAME, 'R',
                                     DPO1.PARTY_TYPE_CODE,
                                     (CASE WHEN DPO1.PARTY_TYPE_CODE IS NULL
                                        THEN FN_FETCH_DESC (DPO1.COMPANY_CODE,'IP_PARTY_TYPE_CODE',CS.PARTY_TYPE_CODE)
                                        ELSE FN_FETCH_DESC (DPO1.COMPANY_CODE,'IP_PARTY_TYPE_CODE',DPO1.PARTY_TYPE_CODE)
                                      END
                                     ),
                                      (CASE DPO1.DISPATCH_FROM
                                        WHEN 'D' THEN 'Distributor'
                                        WHEN 'W' THEN 'Wholeseller'
                                        ELSE NULL
                                      END),
                                     DPO1.CREATED_BY, DPO1.CREATED_DATE, DPO1.DELETED_FLAG,
                                     DPO1.COMPANY_CODE, DPO1.BRANCH_CODE,
                                     DPO1.APPROVED_FLAG, DPO1.DISPATCH_FLAG, DPO1.ACKNOWLEDGE_FLAG, DPO1.REJECT_FLAG,
                                     ES.EMPLOYEE_EDESC,
                                     PS.PO_PARTY_TYPE,
                                     PS.PO_CONVERSION_FACTOR,
                                     PS.PO_BILLING_NAME,
                                     PS.SO_CREDIT_LIMIT_CHK,
                                     CS.CREDIT_LIMIT,
                                     --VSL.BALANCE,
                                     DPO2.TOTAL_QUANTITY,
                                     DPO2.TOTAL_AMOUNT,
                                     DPO2.TOTAL_APPROVE_QTY,
                                     DPO2.TOTAL_APPROVE_AMT,DPO1.REMARKS_REVIEW,
                                     DPO1.WHOLESELLER_CODE
                            ORDER BY DPO1.ORDER_NO DESC";
            var result = _objectEntity.SqlQuery<PurchaseOrderReportModel>(query).ToList();
            return result;
        }

        public List<SalesPersonPoModel> GetSalesPersonList(ReportFiltersModel model, string requestStatus, User userInfo)
        {
            var companyCode = string.Join("','", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            var salesPersonFilter = string.Empty;
            if (model.ItemBrandFilter.Count > 0)
                salesPersonFilter = $" AND ES.EMPLOYEE_CODE IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                salesPersonFilter = $" AND ES.EMPLOYEE_CODE IN ({userInfo.sp_codes})";
            }

            var flagFilter = string.Empty;
            switch (requestStatus)
            {
                case "Approved":
                    flagFilter = @" AND DPO1.approved_flag = 'Y'
                               AND DPO1.REJECT_FLAG = 'N'";
                    break;
                case "Reject":
                    flagFilter = @" AND DPO1.REJECT_FLAG = 'Y'
                                AND DPO1.approved_flag = 'N'";
                    break;
                case "Pending":
                    flagFilter = @" AND DPO1.REJECT_FLAG = 'N'
                                AND DPO1.APPROVED_FLAG = 'N'";
                    break;
                case "Reject,Approved":
                    flagFilter = @" AND DPO1.approved_flag = 'Y'
                               AND DPO1.REJECT_FLAG = 'Y'";
                    break;
                case "Pending,Approved":
                    flagFilter = @" AND DPO1.REJECT_FLAG = 'N'";
                    break;
                case "Pending,Reject":
                    flagFilter = @" AND DPO1.APPROVED_FLAG = 'N'";
                    break;
                default:
                    flagFilter = "";
                    break;

            }



            string query = $@"SELECT * FROM (
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
                                INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DPO1.ITEM_CODE AND IMS.COMPANY_CODE = DPO1.COMPANY_CODE AND IMS.CATEGORY_CODE in (select CATEGORY_CODE  from IP_CATEGORY_CODE WHERE CATEGORY_TYPE IN ('FG','TF') and company_code='{companyCode}') AND IMS.GROUP_SKU_FLAG = 'I'
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
                                      AND TRUNC(DPO1.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}','YYYY-MON-DD') AND TO_DATE('{model.ToDate}','YYYY-MON-DD')
                                      AND DPO1.DELETED_FLAG = 'N'
                                      {flagFilter} {salesPersonFilter}
                                      AND DPO1.COMPANY_CODE IN ('{companyCode}')
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
                                       DPO2.TOTAL_APPROVE_AMT
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
                            INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DPO1.ITEM_CODE AND IMS.COMPANY_CODE = DPO1.COMPANY_CODE AND IMS.CATEGORY_CODE in(select CATEGORY_CODE  from IP_CATEGORY_CODE WHERE CATEGORY_TYPE IN ('FG','TF') and company_code='{companyCode}') AND IMS.GROUP_SKU_FLAG = 'I'
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
                              AND TRUNC(DPO1.ORDER_DATE) >= TO_DATE('{model.FromDate}','YYYY-MM-DD') AND TRUNC(DPO1.ORDER_DATE) <= TO_DATE('{model.ToDate}','YYYY-MM-DD')
                              AND DPO1.DELETED_FLAG = 'N'  
                              AND DPO1.COMPANY_CODE IN ('{companyCode}')  {salesPersonFilter}
                            ) ORDER BY EMPLOYEE_EDESC, ITEM_EDESC, ORDER_NO";
            var result = _objectEntity.SqlQuery<SalesPersonPoModel>(query).ToList();
            return result;
        }

        public List<SalesPersonPoModel> GetItemCumulativeReport(ReportFiltersModel model, User userInfo, string dateFlag)
        {

            string dateflag = dateFlag;
            var GroupDate = string.Empty;
            var GroupDate1 = string.Empty;
            var GroupDate2 = string.Empty;
            // if (dateflag == "true")
            //{
            GroupDate = string.Format(@"TRUNC(DPO.ORDER_DATE) ORDER_DATE,BS_DATE(TO_CHAR(DPO.ORDER_DATE)) MITI,");
            GroupDate1 = string.Format(@"TRUNC(DPO.ORDER_DATE),BS_DATE(TO_CHAR(DPO.ORDER_DATE)),");
            GroupDate2 = string.Format(@"TRUNC(DPO.ORDER_DATE) DESC,");
            ///}
            ///

            var salesPersonFilter = string.Empty;
            //var filter = "";
            if (model.ItemBrandFilter.Count > 0)
                salesPersonFilter = $" AND ES.EMPLOYEE_CODE IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                salesPersonFilter = $" AND ES.EMPLOYEE_CODE IN ({userInfo.sp_codes})";
            }


            var companyCode = string.Join(",", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            var BranchFilter = string.Empty;
            if (model.BranchFilter.Count > 0)
            {
                BranchFilter = string.Format(@" AND  DPO.BRANCH_CODE IN  ('{0}')", string.Join("','", model.BranchFilter).ToString());
            }


            //            string query = $@"SELECT {GroupDate}
            //        --TRUNC(DPO.ORDER_DATE) ORDER_DATE,
            //        TRIM(IMS.ITEM_EDESC) ITEM_EDESC,
            //         DPO.MU_CODE, IUS.MU_CODE CONVERSION_MU_CODE, IUS.CONVERSION_FACTOR,
            //         'P-D' ORDER_ENTITY,
            //         DPO.CREATED_BY, DPO.CREATED_DATE, DPO.REMARKS,
            //         DPO.APPROVED_FLAG, DPO.DISPATCH_FLAG, DPO.ACKNOWLEDGE_FLAG, DPO.REJECT_FLAG, DPO.DELETED_FLAG,
            //         ES.EMPLOYEE_EDESC,
            //         PS.PO_PARTY_TYPE,
            //         PS.PO_CONVERSION_UNIT,
            //         PS.PO_CONVERSION_FACTOR,
            //         SUM(DPO.QUANTITY) TOTAL_QUANTITY,
            //         SUM(DPO.TOTAL_PRICE) TOTAL_AMOUNT FROM DIST_IP_SSD_PURCHASE_ORDER DPO INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DPO.ITEM_CODE AND IMS.COMPANY_CODE = DPO.COMPANY_CODE AND IMS.CATEGORY_CODE = 'FG' AND IMS.GROUP_SKU_FLAG = 'I'
            //LEFT JOIN SA_CUSTOMER_SETUP CS ON CS.CUSTOMER_CODE = DPO.CUSTOMER_CODE AND CS.COMPANY_CODE = DPO.COMPANY_CODE INNER JOIN DIST_LOGIN_USER LU ON LU.USERID = DPO.CREATED_BY INNER JOIN HR_EMPLOYEE_SETUP ES ON ES.EMPLOYEE_CODE = LU.SP_CODE AND ES.COMPANY_CODE = LU.COMPANY_CODE LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON IUS.ITEM_CODE = DPO.ITEM_CODE AND IUS.COMPANY_CODE = DPO.COMPANY_CODE INNER JOIN DIST_PREFERENCE_SETUP PS ON PS.COMPANY_CODE = DPO.COMPANY_CODE WHERE 1 = 1
            //       AND TRUNC(DPO.ORDER_DATE) BETWEEN
            //TO_DATE('{model.FromDate}','YYYY-MON-DD') AND
            //TO_DATE('{model.ToDate}','YYYY-MON-DD')
            //       AND DPO.DELETED_FLAG = 'N'
            //       AND DPO.REJECT_FLAG = 'N'
            //       AND DPO.APPROVED_FLAG = 'N'
            //       AND DPO.COMPANY_CODE IN ({companyCode}) {BranchFilter}
            //GROUP BY {GroupDate1}
            //        --TRUNC(DPO.ORDER_DATE),
            //        TRIM(IMS.ITEM_EDESC),
            //         DPO.MU_CODE, IUS.MU_CODE, IUS.CONVERSION_FACTOR,
            //         'P-D',
            //         DPO.CREATED_BY, DPO.CREATED_DATE, DPO.REMARKS,
            //         DPO.APPROVED_FLAG, DPO.DISPATCH_FLAG, DPO.ACKNOWLEDGE_FLAG, DPO.REJECT_FLAG, DPO.DELETED_FLAG,
            //         ES.EMPLOYEE_EDESC,
            //         PS.PO_PARTY_TYPE,
            //         PS.PO_CONVERSION_UNIT,
            //         PS.PO_CONVERSION_FACTOR
            //ORDER BY EMPLOYEE_EDESC,{GroupDate2}
            //    --ORDER_DATE DESC, 
            //    ITEM_EDESC
            //";

            string query = $@"SELECT {GroupDate}
TRIM (IMS.ITEM_EDESC) ITEM_EDESC,
         DPO.MU_CODE,
         IUS.MU_CODE CONVERSION_MU_CODE,
         IUS.CONVERSION_FACTOR,
         'P-D' ORDER_ENTITY,
         DPO.CREATED_BY,
         ES.EMPLOYEE_EDESC,
         SUM (DPO.QUANTITY) TOTAL_QUANTITY,
         SUM (DPO.TOTAL_PRICE) TOTAL_AMOUNT
    FROM DIST_IP_SSD_PURCHASE_ORDER DPO
         INNER JOIN IP_ITEM_MASTER_SETUP IMS
            ON     IMS.ITEM_CODE = DPO.ITEM_CODE
               AND IMS.COMPANY_CODE = DPO.COMPANY_CODE
               AND IMS.CATEGORY_CODE = 'FG'
               AND IMS.GROUP_SKU_FLAG = 'I'
         LEFT JOIN SA_CUSTOMER_SETUP CS
            ON CS.CUSTOMER_CODE = DPO.CUSTOMER_CODE
               AND CS.COMPANY_CODE = DPO.COMPANY_CODE
         INNER JOIN DIST_LOGIN_USER LU
            ON LU.USERID = DPO.CREATED_BY AND LU.ACTIVE = 'Y'
         INNER JOIN HR_EMPLOYEE_SETUP ES
            ON ES.EMPLOYEE_CODE = LU.SP_CODE
               AND ES.COMPANY_CODE = LU.COMPANY_CODE
         LEFT JOIN IP_ITEM_UNIT_SETUP IUS
            ON IUS.ITEM_CODE = DPO.ITEM_CODE
               AND IUS.COMPANY_CODE = DPO.COMPANY_CODE
         INNER JOIN DIST_PREFERENCE_SETUP PS
            ON PS.COMPANY_CODE = DPO.COMPANY_CODE
   WHERE 1 = 1
         AND TRUNC (DPO.ORDER_DATE) BETWEEN TO_DATE ('{model.FromDate}','YYYY-MON-DD')
                                        AND TO_DATE ('{model.ToDate}','YYYY-MON-DD')
         AND DPO.COMPANY_CODE IN ({companyCode}) {BranchFilter} {salesPersonFilter}
GROUP BY {GroupDate1}
TRIM (IMS.ITEM_EDESC),
         DPO.MU_CODE,
         IUS.MU_CODE,
         IUS.CONVERSION_FACTOR,
         'P-D',
         DPO.CREATED_BY,
         ES.EMPLOYEE_EDESC
ORDER BY ES.EMPLOYEE_EDESC,{GroupDate2} UPPER(TRIM (IMS.ITEM_EDESC))";
            var result = _objectEntity.SqlQuery<SalesPersonPoModel>(query).ToList();
            return result;
        }

        public List<AreaWiseDistributorModel> GetAreaWiseDistributor(filterOption model, User userInfo, string type)
        {
            var areaFilter = string.Empty;
            var entityFilter = string.Empty;
            var typeFilter = string.Empty;
            if (model.ReportFilters.DistAreaFilter.Count > 0)
                areaFilter = string.Format(@" AND AREACODE IN('{0}')", string.Join("','", model.ReportFilters.DistAreaFilter).ToString());

            if (model.ReportFilters.CustomerFilter.Count > 0)
                entityFilter = string.Format(@" AND CODE IN('{0}')", string.Join("','", model.ReportFilters.CustomerFilter).ToString());

            if (type == "R" || type == "D")
                typeFilter = $" AND TYPE IN ('{type}')";

            var query = $@"SELECT * FROM (
                    SELECT D.DISTRIBUTOR_CODE AS CODE,D.AREA_CODE AS AREACODE,D.LATITUDE AS LATITUDE,D.LONGITUDE AS LONGITUDE,
                           S.CUSTOMER_EDESC AS NAME,S.REGD_OFFICE_EADDRESS AS ADDRESS,A.AREA_NAME AS AREANAME,'D' AS TYPE,D.ACTIVE,TO_CHAR(D.CREATED_BY) AS CREATED_BY,D.CREATED_DATE
                      FROM DIST_DISTRIBUTOR_MASTER D, SA_CUSTOMER_SETUP S, DIST_AREA_MASTER A
                     WHERE S.COMPANY_CODE = D.COMPANY_CODE AND S.CUSTOMER_CODE = D.DISTRIBUTOR_CODE
                           AND A.COMPANY_CODE = D.COMPANY_CODE AND D.AREA_CODE = A.AREA_CODE
                           AND S.DELETED_FLAG = 'N'
                           AND D.ACTIVE = 'Y'
                           AND D.COMPANY_CODE = '{userInfo.company_code}'
                    UNION ALL
                    SELECT R.RESELLER_CODE AS CODE,R.AREA_CODE AS AREACODE,R.LATITUDE AS LATITUDE,R.LONGITUDE AS LONGITUDE,
                           R.RESELLER_NAME AS NAME,R.REG_OFFICE_ADDRESS AS ADDRESS,M.AREA_NAME AS AREANAME,'R' AS TYPE,R.ACTIVE,R.CREATED_BY_NAME AS CREATED_BY,R.CREATED_DATE
                      FROM DIST_RESELLER_MASTER R, DIST_AREA_MASTER M
                     WHERE     R.AREA_CODE = M.AREA_CODE
                           AND M.COMPANY_CODE = R.COMPANY_CODE)
                    WHERE LATITUDE <> 0 AND LONGITUDE <> 0
                          {areaFilter}
                          {entityFilter} {typeFilter}";
            var result = _objectEntity.SqlQuery<AreaWiseDistributorModel>(query).ToList();
            return result;
        }
        public List<AreaWiseDistributorModel> GetUserWiseOUtlet(filterOption model, User userInfo, string type)
        {
            var areaFilter = string.Empty;
            var entityFilter = string.Empty;
            var typeFilter = string.Empty;
            if (model.ReportFilters.DistAreaFilter.Count > 0)
                areaFilter = string.Format(@" AND M.AREACODE IN('{0}')", string.Join("','", model.ReportFilters.DistAreaFilter).ToString());

            if (model.ReportFilters.CustomerFilter.Count > 0)
                entityFilter = string.Format(@" AND  DU.SP_CODE IN('{0}')", string.Join("','", model.ReportFilters.CustomerFilter).ToString());

            var query = $@"
                      SELECT R.RESELLER_CODE AS CODE,R.AREA_CODE AS AREACODE,R.LATITUDE AS LATITUDE,R.LONGITUDE AS LONGITUDE,
                           R.RESELLER_NAME AS NAME,R.REG_OFFICE_ADDRESS AS ADDRESS,M.AREA_NAME AS AREANAME,'R' AS TYPE,R.ACTIVE,R.CREATED_BY_NAME AS CREATED_BY,R.CREATED_DATE,r.source,du.full_name
                      FROM DIST_RESELLER_MASTER R, DIST_AREA_MASTER M,dist_login_user du
                     WHERE     R.AREA_CODE = M.AREA_CODE
                           AND M.COMPANY_CODE = R.COMPANY_CODE
                          and R.created_by=du.userid
                          {areaFilter}
                          {entityFilter}";
            var result = _objectEntity.SqlQuery<AreaWiseDistributorModel>(query).ToList();
            return result;
        }
        public PreferenceSetupModel GetPrefSetting(User userInfo)
        {
            var com = userInfo.company_code;
            string prefQuery = $"SELECT * FROM DIST_PREFERENCE_SETUP WHERE COMPANY_CODE='{com}'";
            var data = _objectEntity.SqlQuery<PreferenceSetupModel>(prefQuery).FirstOrDefault();
            return data;
        }
        public PurchaseOrderReportModel GetMaxOrderNoFromDistributor()
        {
            var query = "select(max(to_number(order_no)) + 1) orderno from dist_ip_ssd_purchase_order";
            var data = _objectEntity.SqlQuery<PurchaseOrderReportModel>(query).FirstOrDefault();
            return data;
        }


        public List<EODModel> GetEODList(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join(",", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            //var BranchFilter = string.Empty;
            //if (model.BranchFilter.Count > 0)
            //{
            //    BranchFilter = string.Format(@" AND  DPO.BRANCH_CODE IN  ('{0}')", string.Join("','", model.BranchFilter).ToString());
            //}
            var filter = "";
            if (model.ItemBrandFilter.Count > 0)
                filter = $" AND DLU.SP_CODE IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                filter = $" AND DLU.SP_CODE IN ({userInfo.sp_codes})";

            var query = $@"SELECT LT.SP_CODE,
                               TRUNC (EU.CREATED_DATE) SUBMIT_DATE,
                               NVL(EU.PO_DCOUNT,0) PO_DCOUNT,
                               NVL(EU.PO_RCOUNT,0) PO_RCOUNT,
                               ES.EMPLOYEE_EDESC,
                               EPERMANENT_ADDRESS1 ADDRESS,DGM.GROUP_EDESC,
                              DLU.CONTACT_NO CONTACT,EU.REMARKS,EU.LATITUDE||','||EU.LONGITUDE EOD_LOCATION
                          FROM DIST_LM_LOCATION_TRACKING LT, DIST_EOD_UPDATE EU, HR_EMPLOYEE_SETUP ES,
                                   DIST_LOGIN_USER  DLU, DIST_GROUP_MASTER DGM 
                         WHERE     1 = 1
                               AND LT.SP_CODE = EU.SP_CODE
                               AND LT.COMPANY_CODE = EU.COMPANY_CODE
                               AND LT.SP_CODE = ES.EMPLOYEE_CODE(+)
                               AND LT.COMPANY_CODE = ES.COMPANY_CODE(+)
                               AND LT.SP_CODE = DLU.SP_CODE(+)
                               AND DLU.GROUPID = DGM.GROUPID(+)
                               AND DLU.ACTIVE = 'Y'
                               AND TRACK_TYPE = 'EOD'
                               AND LT.COMPANY_CODE IN ({userInfo.company_code}) {filter}
                              AND TRUNC(EU.CREATED_DATE) >=TO_DATE('{model.FromDate}','YYYY-MM-DD') AND TRUNC(EU.CREATED_DATE) <=TO_DATE('{model.ToDate}','YYYY-MM-DD')
                        GROUP BY  LT.SP_CODE,TRUNC (EU.CREATED_DATE) ,ES.EMPLOYEE_EDESC,EPERMANENT_ADDRESS1, GROUP_EDESC,DLU.CONTACT_NO,EU.REMARKS,EU.PO_DCOUNT,EU.PO_RCOUNT
                                   ,EU.LATITUDE,EU.LONGITUDE
                        ORDER BY TRUNC (EU.CREATED_DATE)  DESC,ES.EMPLOYEE_EDESC";
            var result = _objectEntity.SqlQuery<EODModel>(query).ToList();
            return result;
        }


        public List<DailyActivityModel> GetDailyAcivityList(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join(",", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            var spCode = "";
            if (model.ItemBrandFilter.Count > 0)
                spCode = $" AND PFMTBL.SP_CODE IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                spCode = $" AND PFMTBL.SP_CODE IN ({userInfo.sp_codes})";

            var query = $@"SELECT PFMTBL.GROUP_EDESC, PFMTBL.SP_CODE, PFMTBL.EMPLOYEE_EDESC,
  TODTBL.TOD_ROUTE_CODE, TODTBL.TOD_ROUTE_NAME, TOMTBL.TOM_ROUTE_CODE, TOMTBL.TOM_ROUTE_NAME,
  PFMTBL.CUR_DATE, PFMTBL.CUR_LATITUDE, PFMTBL.CUR_LONGITUDE,
  PFMTBL.ATN_IMAGE, PFMTBL.ATN_DATE, PFMTBL.ATN_LATITUDE, PFMTBL.ATN_LONGITUDE, PFMTBL.EOD_DATE, PFMTBL.EOD_LATITUDE, PFMTBL.EOD_LONGITUDE,
  SUM(PFMTBL.TARGET) TARGET, SUM(PFMTBL.VISITED) VISITED, SUM(PFMTBL.NOT_VISITED) NOT_VISITED,
  SUM(PFMTBL.PJP_PRODUCTIVE) PJP_PRODUCTIVE,
  SUM(PFMTBL.VISITED) - SUM(PFMTBL.PJP_PRODUCTIVE) PJP_NON_PRODUCTIVE,
  SUM(PFMTBL.NPJP_PRODUCTIVE) NPJP_PRODUCTIVE,
  ROUND(DECODE(SUM(PFMTBL.PJP_PRODUCTIVE), NULL, 0,
        0, 0,
        (DECODE(SUM(PFMTBL.VISITED),0,0,SUM(PFMTBL.PJP_PRODUCTIVE) / SUM(PFMTBL.VISITED))) * 100),2) PERCENT_EFFECTIVE_CALLS,
  SUM(PFMTBL.OUTLET_ADDED) OUTLET_ADDED,
  SUM(PFMTBL.PJP_TOTAL_QUANTITY) PJP_TOTAL_QUANTITY, SUM(PFMTBL.PJP_TOTAL_AMOUNT) PJP_TOTAL_AMOUNT,
  SUM(PFMTBL.NPJP_TOTAL_QUANTITY) NPJP_TOTAL_QUANTITY, SUM(PFMTBL.NPJP_TOTAL_AMOUNT) NPJP_TOTAL_AMOUNT
FROM (SELECT PTBL.*
      FROM (SELECT PETBL.GROUP_EDESC, PETBL.SP_CODE, PETBL.EMPLOYEE_EDESC, PETBL.COMPANY_CODE, PETBL.CUR_DATE, PETBL.CUR_LATITUDE, PETBL.CUR_LONGITUDE, PETBL.ATN_IMAGE, PETBL.ATN_DATE, PETBL.ATN_LATITUDE, PETBL.ATN_LONGITUDE, PETBL.EOD_DATE, PETBL.EOD_LATITUDE, PETBL.EOD_LONGITUDE, NVL(PETBL.TARGET, 0) TARGET, 
            NVL(PVTBL.VISITED, 0) VISITED, NVL(PNVTBL.NOT_VISITED,0) NOT_VISITED, 
            NVL(PPJPTBL.PJP_PRODUCTIVE,0) PJP_PRODUCTIVE,
            (NVL(PVTBL.VISITED, 0) - NVL(PPJPTBL.PJP_PRODUCTIVE,0)) PJP_NON_PRODUCTIVE,
            NVL(PNPJPTBL.NPJP_PRODUCTIVE,0) NPJP_PRODUCTIVE,
            NVL(DECODE(PPJPTBL.PJP_PRODUCTIVE, NULL, 0,
                                             0, 0,
                                             ROUND(DECODE(PVTBL.VISITED,0,0,(PPJPTBL.PJP_PRODUCTIVE / PVTBL.VISITED)) * 100, 2)),0) NET_PERCENT_EFFECTIVE_CALLS,
            0 OUTLET_ADDED,
            NVL(PPJPTBL.PJP_TOTAL_QUANTITY,0) PJP_TOTAL_QUANTITY, NVL(PPJPTBL.PJP_TOTAL_PRICE,0) PJP_TOTAL_AMOUNT,
            NVL(PNPJPTBL.NPJP_TOTAL_QUANTITY,0) NPJP_TOTAL_QUANTITY, NVL(PNPJPTBL.NPJP_TOTAL_PRICE,0) NPJP_TOTAL_AMOUNT
            FROM (SELECT ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, ENT.CUR_DATE, ENT.CUR_LATITUDE, ENT.CUR_LONGITUDE, ENT.ATN_IMAGE, ENT.ATN_DATE, ENT.ATN_LATITUDE, ENT.ATN_LONGITUDE, ENT.EOD_DATE, ENT.EOD_LATITUDE, ENT.EOD_LONGITUDE, COUNT(ENT.ENTITY_CODE) TARGET
                  FROM (
                      SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, TRIM(DGM.GROUP_EDESC) GROUP_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE, LOC.CUR_DATE, LOC.CUR_LATITUDE, LOC.CUR_LONGITUDE, API.ATN_IMAGE, ATN.ATN_DATE, ATN.ATN_LATITUDE, ATN.ATN_LONGITUDE, EOD.EOD_DATE, EOD.EOD_LATITUDE, EOD.EOD_LONGITUDE
                      FROM DIST_LOGIN_USER DLU
                      INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND HES.GROUP_SKU_FLAG = 'I' AND DLU.ACTIVE = 'Y'
                      LEFT JOIN DIST_GROUP_MASTER DGM ON DGM.GROUPID = DLU.GROUPID AND DGM.COMPANY_CODE = DGM.COMPANY_CODE
                      LEFT JOIN (SELECT A.ENTITY_CODE SP_CODE, A.COMPANY_CODE, A.CREATE_DATE, A.FILENAME ATN_IMAGE
                                  FROM DIST_PHOTO_INFO A
                                  WHERE A.CREATE_DATE = (SELECT MAX(CREATE_DATE) FROM DIST_PHOTO_INFO WHERE ENTITY_CODE = A.ENTITY_CODE AND ENTITY_TYPE = 'S' AND (MEDIA_TYPE = 'ATN' OR CATEGORYID = 1) AND TRUNC(CREATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR'))
   and substr(A.FILENAME,length(A.FILENAME)-7,4) = TO_char(A.CREATE_DATE, 'RRRR')
                                  GROUP BY A.ENTITY_CODE, A.COMPANY_CODE, A.CREATE_DATE, A.FILENAME
                                  ORDER BY A.ENTITY_CODE DESC
                      ) API ON API.SP_CODE = DLU.SP_CODE AND API.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (SELECT SP_CODE, COMPANY_CODE, CUR_DATE,WM_CONCAT(CUR_LATITUDE) CUR_LATITUDE, WM_CONCAT(CUR_LONGITUDE) CUR_LONGITUDE FROM( SELECT A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE CUR_DATE, A.LATITUDE CUR_LATITUDE, A.LONGITUDE CUR_LONGITUDE
                                  FROM DIST_LM_LOCATION_TRACKING A
                                  WHERE A.SUBMIT_DATE = (SELECT MAX(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND TRACK_TYPE = 'TRK')
                                  GROUP BY A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE, A.LATITUDE, A.LONGITUDE
                                  ORDER BY A.SP_CODE DESC)  GROUP BY SP_CODE, COMPANY_CODE, CUR_DATE
                      ) LOC ON LOC.SP_CODE = DLU.SP_CODE AND LOC.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (SELECT  SP_CODE, COMPANY_CODE,ATN_DATE, WM_CONCAT(ATN_LATITUDE) ATN_LATITUDE, WM_CONCAT(ATN_LONGITUDE) ATN_LONGITUDE FROM (SELECT A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE ATN_DATE, A.LATITUDE ATN_LATITUDE, A.LONGITUDE ATN_LONGITUDE
                                  FROM DIST_LM_LOCATION_TRACKING A
                                  WHERE A.SUBMIT_DATE = (SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND TRACK_TYPE = 'ATN')
                                  GROUP BY A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE, A.LATITUDE, A.LONGITUDE
                                  ORDER BY A.SP_CODE DESC) GROUP BY SP_CODE,COMPANY_CODE,ATN_DATE
                      ) ATN ON ATN.SP_CODE = DLU.SP_CODE AND ATN.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (SELECT SP_CODE, COMPANY_CODE, EOD_DATE, WM_CONCAT(EOD_LATITUDE)  EOD_LATITUDE, WM_CONCAT(EOD_LONGITUDE) EOD_LONGITUDE FROM (SELECT A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE EOD_DATE, A.LATITUDE EOD_LATITUDE, A.LONGITUDE EOD_LONGITUDE
                                  FROM DIST_LM_LOCATION_TRACKING A
                                  WHERE A.SUBMIT_DATE = (SELECT MAX(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND TRACK_TYPE = 'EOD')
                                  GROUP BY A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE, A.LATITUDE, A.LONGITUDE
                                  ORDER BY A.SP_CODE DESC) GROUP BY  SP_CODE, COMPANY_CODE,EOD_DATE
                      ) EOD ON EOD.SP_CODE = DLU.SP_CODE AND EOD.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                      LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                      LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                      GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), TRIM(DGM.GROUP_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC), DLU.COMPANY_CODE, LOC.CUR_DATE, LOC.CUR_LATITUDE, LOC.CUR_LONGITUDE, API.ATN_IMAGE, ATN.ATN_DATE, ATN.ATN_LATITUDE, ATN.ATN_LONGITUDE, EOD.EOD_DATE, EOD.EOD_LATITUDE, EOD.EOD_LONGITUDE
                  ) ENT
                  GROUP BY ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, ENT.CUR_DATE, ENT.CUR_LATITUDE, ENT.CUR_LONGITUDE, ENT.ATN_IMAGE, ENT.ATN_DATE, ENT.ATN_LATITUDE, ENT.ATN_LONGITUDE, ENT.EOD_DATE, ENT.EOD_LATITUDE, ENT.EOD_LONGITUDE
                  ) PETBL -- Party Type Entity Table
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) VISITED
                        FROM (
                            SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND HES.GROUP_SKU_FLAG = 'I' AND DLU.ACTIVE = 'Y'
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                            GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                        ) ENT
                        INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                        WHERE DLT.SP_CODE IS NOT NULL
                        GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) PVTBL -- Party Type Visit Table
                  ON PVTBL.SP_CODE = PETBL.SP_CODE AND PVTBL.COMPANY_CODE = PETBL.COMPANY_CODE
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) NOT_VISITED
                      FROM (
                          SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.PARTY_TYPE_CODE ENTITY_CODE, TRIM(SCS.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                          FROM DIST_LOGIN_USER DLU
                          INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND HES.GROUP_SKU_FLAG = 'I' AND DLU.ACTIVE = 'Y'
                          LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                          LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                          LEFT JOIN IP_PARTY_TYPE_CODE SCS ON SCS.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE 
                          GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.PARTY_TYPE_CODE, TRIM(SCS.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                      ) ENT
                      LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                      WHERE DLT.SP_CODE IS NULL
                      GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) PNVTBL -- Party Type Not Visited Table
                  ON PNVTBL.SP_CODE = PETBL.SP_CODE AND PNVTBL.COMPANY_CODE = PETBL.COMPANY_CODE
            LEFT JOIN (SELECT PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE, COUNT(PPO.PARTY_TYPE_CODE) PJP_PRODUCTIVE, SUM(PPO.TOTAL_QUANTITY) PJP_TOTAL_QUANTITY, SUM(PPO.TOTAL_PRICE) PJP_TOTAL_PRICE
                      FROM (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND HES.GROUP_SKU_FLAG = 'I' AND DLU.ACTIVE = 'Y'
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'P' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN IP_PARTY_TYPE_CODE PTC ON PTC.PARTY_TYPE_CODE = DRE.ENTITY_CODE AND PTC.COMPANY_CODE = DRE.COMPANY_CODE
                            GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), PTC.PARTY_TYPE_CODE, TRIM(PTC.PARTY_TYPE_EDESC), DLU.COMPANY_CODE
                          ) PJPENT
                      LEFT JOIN (SELECT CREATED_BY, PARTY_TYPE_CODE, COMPANY_CODE, COUNT(PARTY_TYPE_CODE) TOTAL_ORDER, SUM(QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE
                                  FROM DIST_IP_SSD_PURCHASE_ORDER
                                  WHERE TRUNC(ORDER_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                                  GROUP BY CREATED_BY, PARTY_TYPE_CODE, COMPANY_CODE
                                ) PPO ON PPO.CREATED_BY = PJPENT.USERID AND PPO.PARTY_TYPE_CODE = PJPENT.ENTITY_CODE AND PPO.COMPANY_CODE = PJPENT.COMPANY_CODE
                      GROUP BY PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE
                   ) PPJPTBL -- Party Type PJP Table
                   ON PPJPTBL.SP_CODE = PETBL.SP_CODE AND PPJPTBL.COMPANY_CODE = PETBL.COMPANY_CODE
            LEFT JOIN (SELECT PPO.SP_CODE, PPO.EMPLOYEE_EDESC, PPO.COMPANY_CODE, COUNT(PPO.PARTY_TYPE_CODE) NPJP_PRODUCTIVE, SUM(PPO.TOTAL_QUANTITY) NPJP_TOTAL_QUANTITY, SUM(PPO.TOTAL_PRICE) NPJP_TOTAL_PRICE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, A.PARTY_TYPE_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.COMPANY_CODE, COUNT(A.PARTY_TYPE_CODE) TOTAL_ORDER, SUM(A.QUANTITY) TOTAL_QUANTITY, SUM(A.TOTAL_PRICE) TOTAL_PRICE
                            FROM DIST_IP_SSD_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.ACTIVE = 'Y'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE AND C.GROUP_SKU_FLAG = 'I'
                            INNER JOIN IP_PARTY_TYPE_CODE D ON D.PARTY_TYPE_CODE = A.PARTY_TYPE_CODE AND D.COMPANY_CODE = A.COMPANY_CODE
                            WHERE TRUNC(A.ORDER_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            GROUP BY A.CREATED_BY, B.SP_CODE, A.PARTY_TYPE_CODE, TRIM(C.EMPLOYEE_EDESC), A.COMPANY_CODE
                      ) PPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, PTC.PARTY_TYPE_CODE ENTITY_CODE, TRIM(PTC.PARTY_TYPE_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND HES.GROUP_SKU_FLAG = 'I' AND DLU.ACTIVE = 'Y'
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
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
      FROM (SELECT DETBL.GROUP_EDESC, DETBL.SP_CODE, DETBL.EMPLOYEE_EDESC, DETBL.COMPANY_CODE, DETBL.CUR_DATE, DETBL.CUR_LATITUDE, DETBL.CUR_LONGITUDE, DETBL.ATN_IMAGE, DETBL.ATN_DATE, DETBL.ATN_LATITUDE, DETBL.ATN_LONGITUDE, DETBL.EOD_DATE, DETBL.EOD_LATITUDE, DETBL.EOD_LONGITUDE, NVL(DETBL.TARGET, 0) TARGET, 
            NVL(DVTBL.VISITED, 0) VISITED, NVL(DNVTBL.NOT_VISITED,0) NOT_VISITED, 
            NVL(DPJPTBL.PJP_PRODUCTIVE,0) PJP_PRODUCTIVE,
            (NVL(DVTBL.VISITED, 0) - NVL(DPJPTBL.PJP_PRODUCTIVE,0)) PJP_NON_PRODUCTIVE,
            NVL(DNPJPTBL.NPJP_PRODUCTIVE,0) NPJP_PRODUCTIVE,
            NVL(DECODE(DPJPTBL.PJP_PRODUCTIVE, NULL, 0,0, 0,ROUND(DECODE(DVTBL.VISITED,0,0,(DPJPTBL.PJP_PRODUCTIVE / DVTBL.VISITED)) * 100,2)),0) NET_PERCENT_EFFECTIVE_CALLS,
            0 OUTLET_ADDED,
            NVL(DPJPTBL.PJP_TOTAL_QUANTITY,0) PJP_TOTAL_QUANTITY, NVL(DPJPTBL.PJP_TOTAL_PRICE,0) PJP_TOTAL_AMOUNT,
            NVL(DNPJPTBL.NPJP_TOTAL_QUANTITY,0) NPJP_TOTAL_QUANTITY, NVL(DNPJPTBL.NPJP_TOTAL_PRICE,0) NPJP_TOTAL_AMOUNT
            FROM (SELECT ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, ENT.CUR_DATE, ENT.CUR_LATITUDE, ENT.CUR_LONGITUDE, ENT.ATN_IMAGE, ENT.ATN_DATE, ENT.ATN_LATITUDE, ENT.ATN_LONGITUDE, ENT.EOD_DATE, ENT.EOD_LATITUDE, ENT.EOD_LONGITUDE, COUNT(ENT.ENTITY_CODE) TARGET
                  FROM (
                      SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, TRIM(DGM.GROUP_EDESC) GROUP_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE, LOC.CUR_DATE, LOC.CUR_LATITUDE, LOC.CUR_LONGITUDE, API.ATN_IMAGE, ATN.ATN_DATE, ATN.ATN_LATITUDE, ATN.ATN_LONGITUDE, EOD.EOD_DATE, EOD.EOD_LATITUDE, EOD.EOD_LONGITUDE
                      FROM DIST_LOGIN_USER DLU
                      INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND HES.GROUP_SKU_FLAG = 'I' AND DLU.ACTIVE = 'Y'
                      LEFT JOIN DIST_GROUP_MASTER DGM ON DGM.GROUPID = DLU.GROUPID AND DGM.COMPANY_CODE = DGM.COMPANY_CODE
                      LEFT JOIN (SELECT A.ENTITY_CODE SP_CODE, A.COMPANY_CODE, A.CREATE_DATE, A.FILENAME ATN_IMAGE
                                  FROM DIST_PHOTO_INFO A
                                  WHERE A.CREATE_DATE = (SELECT MAX(CREATE_DATE) FROM DIST_PHOTO_INFO WHERE ENTITY_CODE = A.ENTITY_CODE AND ENTITY_TYPE = 'S' AND (MEDIA_TYPE = 'ATN' OR CATEGORYID = 1) AND TRUNC(CREATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR'))
    and substr(A.FILENAME,length(A.FILENAME)-7,4) = TO_char(A.CREATE_DATE, 'RRRR')
                                  GROUP BY A.ENTITY_CODE, A.COMPANY_CODE, A.CREATE_DATE, A.FILENAME
                                  ORDER BY A.ENTITY_CODE DESC
                      ) API ON API.SP_CODE = DLU.SP_CODE AND API.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (SELECT SP_CODE, COMPANY_CODE, CUR_DATE, WM_CONCAT(CUR_LATITUDE) CUR_LATITUDE, WM_CONCAT(CUR_LONGITUDE) CUR_LONGITUDE FROM ( SELECT A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE CUR_DATE, A.LATITUDE CUR_LATITUDE, A.LONGITUDE CUR_LONGITUDE
                                  FROM DIST_LM_LOCATION_TRACKING A
                                  WHERE A.SUBMIT_DATE = (SELECT MAX(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND TRACK_TYPE = 'TRK')
                                  GROUP BY A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE, A.LATITUDE, A.LONGITUDE
                                  ORDER BY A.SP_CODE DESC) GROUP BY SP_CODE, COMPANY_CODE, CUR_DATE
                      ) LOC ON LOC.SP_CODE = DLU.SP_CODE AND LOC.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (SELECT SP_CODE, COMPANY_CODE, ATN_DATE,WM_CONCAT(ATN_LATITUDE) ATN_LATITUDE, WM_CONCAT(ATN_LONGITUDE) ATN_LONGITUDE FROM (SELECT A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE ATN_DATE, A.LATITUDE ATN_LATITUDE, A.LONGITUDE ATN_LONGITUDE
                                  FROM DIST_LM_LOCATION_TRACKING A
                                  WHERE A.SUBMIT_DATE = (SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND TRACK_TYPE = 'ATN')
                                  GROUP BY A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE, A.LATITUDE, A.LONGITUDE
                                  ORDER BY A.SP_CODE DESC) GROUP BY SP_CODE,COMPANY_CODE,ATN_DATE
                      ) ATN ON ATN.SP_CODE = DLU.SP_CODE AND ATN.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (SELECT SP_CODE, COMPANY_CODE,EOD_DATE, WM_CONCAT(EOD_LATITUDE) EOD_LATITUDE, WM_CONCAT(EOD_LONGITUDE) EOD_LONGITUDE FROM ( SELECT A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE EOD_DATE, A.LATITUDE EOD_LATITUDE, A.LONGITUDE EOD_LONGITUDE
                                  FROM DIST_LM_LOCATION_TRACKING A
                                  WHERE A.SUBMIT_DATE = (SELECT MAX(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND TRACK_TYPE = 'EOD')
                                  GROUP BY A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE, A.LATITUDE, A.LONGITUDE
                                  ORDER BY A.SP_CODE DESC) GROUP BY SP_CODE, COMPANY_CODE, EOD_DATE
                      ) EOD ON EOD.SP_CODE = DLU.SP_CODE AND EOD.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                      LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                      LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                      GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), TRIM(DGM.GROUP_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE, LOC.CUR_DATE, LOC.CUR_LATITUDE, LOC.CUR_LONGITUDE, API.ATN_IMAGE, ATN.ATN_DATE, ATN.ATN_LATITUDE, ATN.ATN_LONGITUDE, EOD.EOD_DATE, EOD.EOD_LATITUDE, EOD.EOD_LONGITUDE
                  ) ENT
                  GROUP BY ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, ENT.CUR_DATE, ENT.CUR_LATITUDE, ENT.CUR_LONGITUDE, ENT.ATN_IMAGE, ENT.ATN_DATE, ENT.ATN_LATITUDE, ENT.ATN_LONGITUDE, ENT.EOD_DATE, ENT.EOD_LATITUDE, ENT.EOD_LONGITUDE
                  ) DETBL -- Customer/Distributor Entity Table
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) VISITED
                        FROM (
                            SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND HES.GROUP_SKU_FLAG = 'I' AND DLU.ACTIVE = 'Y'
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                            GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                        ) ENT
                        INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                        WHERE DLT.SP_CODE IS NOT NULL
                        GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) DVTBL -- Customer/Distributor Visit Table
                  ON DVTBL.SP_CODE = DETBL.SP_CODE AND DVTBL.COMPANY_CODE = DETBL.COMPANY_CODE
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) NOT_VISITED
                      FROM (
                          SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                          FROM DIST_LOGIN_USER DLU
                          INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND HES.GROUP_SKU_FLAG = 'I' AND DLU.ACTIVE = 'Y'
                          LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                          LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                          LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE 
                          GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                      ) ENT
                      LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                      WHERE DLT.SP_CODE IS NULL
                      GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                ) DNVTBL -- Customer/Distributor Not Visited Table
                ON DNVTBL.SP_CODE = DETBL.SP_CODE AND DNVTBL.COMPANY_CODE = DETBL.COMPANY_CODE
            LEFT JOIN (SELECT PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE, COUNT(DPO.CUSTOMER_CODE) PJP_PRODUCTIVE, SUM(DPO.TOTAL_QUANTITY) PJP_TOTAL_QUANTITY, SUM(DPO.TOTAL_PRICE) PJP_TOTAL_PRICE
                      FROM (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND HES.GROUP_SKU_FLAG = 'I' AND DLU.ACTIVE = 'Y'
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'D' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DRE.ENTITY_CODE AND SCS.COMPANY_CODE = DRE.COMPANY_CODE
                            GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), SCS.CUSTOMER_CODE, TRIM(SCS.CUSTOMER_EDESC), DLU.COMPANY_CODE
                      ) PJPENT
                      LEFT JOIN (SELECT CREATED_BY, CUSTOMER_CODE, COMPANY_CODE, COUNT(CUSTOMER_CODE) TOTAL_ORDER, SUM(QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE
                                  FROM DIST_IP_SSD_PURCHASE_ORDER
                                  WHERE TRUNC(ORDER_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                                  GROUP BY CREATED_BY, CUSTOMER_CODE, COMPANY_CODE
                      ) DPO ON DPO.CREATED_BY = PJPENT.USERID AND DPO.CUSTOMER_CODE = PJPENT.ENTITY_CODE AND DPO.COMPANY_CODE = PJPENT.COMPANY_CODE
                      GROUP BY PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE
                   ) DPJPTBL -- Customer/Distributor PJP Table
                   ON DPJPTBL.SP_CODE = DETBL.SP_CODE AND DPJPTBL.COMPANY_CODE = DETBL.COMPANY_CODE
            LEFT JOIN (SELECT DPO.SP_CODE, DPO.EMPLOYEE_EDESC, DPO.COMPANY_CODE, COUNT(DPO.CUSTOMER_CODE) NPJP_PRODUCTIVE, SUM(DPO.TOTAL_QUANTITY) NPJP_TOTAL_QUANTITY, SUM(DPO.TOTAL_PRICE) NPJP_TOTAL_PRICE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, A.CUSTOMER_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.COMPANY_CODE, COUNT(A.CUSTOMER_CODE) TOTAL_ORDER, SUM(A.QUANTITY) TOTAL_QUANTITY, SUM(A.TOTAL_PRICE) TOTAL_PRICE
                            FROM DIST_IP_SSD_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.ACTIVE = 'Y'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE AND C.GROUP_SKU_FLAG = 'I'
                            WHERE TRUNC(A.ORDER_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            GROUP BY A.CREATED_BY, B.SP_CODE, A.CUSTOMER_CODE, TRIM(C.EMPLOYEE_EDESC), A.COMPANY_CODE
                      ) DPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, SCS.CUSTOMER_CODE ENTITY_CODE, TRIM(SCS.CUSTOMER_EDESC) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND HES.GROUP_SKU_FLAG = 'I' AND DLU.ACTIVE = 'Y'
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
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
      FROM (SELECT RETBL.GROUP_EDESC, RETBL.SP_CODE, RETBL.EMPLOYEE_EDESC, RETBL.COMPANY_CODE, RETBL.CUR_DATE, RETBL.CUR_LATITUDE, RETBL.CUR_LONGITUDE, RETBL.ATN_IMAGE, RETBL.ATN_DATE, RETBL.ATN_LATITUDE, RETBL.ATN_LONGITUDE, RETBL.EOD_DATE, RETBL.EOD_LATITUDE, RETBL.EOD_LONGITUDE, NVL(RETBL.TARGET, 0) TARGET, 
            NVL(RVTBL.VISITED, 0) VISITED, NVL(RNVTBL.NOT_VISITED,0) NOT_VISITED, 
            NVL(RPJPTBL.PJP_PRODUCTIVE,0) PJP_PRODUCTIVE,
            (NVL(RVTBL.VISITED, 0) - NVL(RPJPTBL.PJP_PRODUCTIVE,0)) PJP_NON_PRODUCTIVE,
            NVL(RNPJPTBL.NPJP_PRODUCTIVE,0) NPJP_PRODUCTIVE,
            NVL(DECODE(RPJPTBL.PJP_PRODUCTIVE, NULL, 0,
                                             0, 0,
                                             ROUND(DECODE(RVTBL.VISITED,0,0,(RPJPTBL.PJP_PRODUCTIVE / RVTBL.VISITED)) * 100,2)),0) NET_PERCENT_EFFECTIVE_CALLS,
            NVL(ROUT.OUTLET_ADDED, 0) OUTLET_ADDED,
            NVL(RPJPTBL.PJP_TOTAL_QUANTITY,0) PJP_TOTAL_QUANTITY, NVL(RPJPTBL.PJP_TOTAL_PRICE,0) PJP_TOTAL_AMOUNT,
            NVL(RNPJPTBL.NPJP_TOTAL_QUANTITY,0) NPJP_TOTAL_QUANTITY, NVL(RNPJPTBL.NPJP_TOTAL_PRICE,0) NPJP_TOTAL_AMOUNT
            FROM (SELECT ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, ENT.CUR_DATE, ENT.CUR_LATITUDE, ENT.CUR_LONGITUDE, ENT.ATN_IMAGE, ENT.ATN_DATE, ENT.ATN_LATITUDE, ENT.ATN_LONGITUDE, ENT.EOD_DATE, ENT.EOD_LATITUDE, ENT.EOD_LONGITUDE, COUNT(ENT.ENTITY_CODE) TARGET
                  FROM (
                      SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, TRIM(DGM.GROUP_EDESC) GROUP_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE, LOC.CUR_DATE, LOC.CUR_LATITUDE, LOC.CUR_LONGITUDE, API.ATN_IMAGE, ATN.ATN_DATE, ATN.ATN_LATITUDE, ATN.ATN_LONGITUDE, EOD.EOD_DATE, EOD.EOD_LATITUDE, EOD.EOD_LONGITUDE
                      FROM DIST_LOGIN_USER DLU
                      INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND HES.GROUP_SKU_FLAG = 'I' AND DLU.ACTIVE = 'Y'
                      LEFT JOIN DIST_GROUP_MASTER DGM ON DGM.GROUPID = DLU.GROUPID AND DGM.COMPANY_CODE = DGM.COMPANY_CODE
                      LEFT JOIN (SELECT A.ENTITY_CODE SP_CODE, A.COMPANY_CODE, A.CREATE_DATE, A.FILENAME ATN_IMAGE
                                  FROM DIST_PHOTO_INFO A
                                  WHERE A.CREATE_DATE = (SELECT MAX(CREATE_DATE) FROM DIST_PHOTO_INFO WHERE ENTITY_CODE = A.ENTITY_CODE AND ENTITY_TYPE = 'S' AND (MEDIA_TYPE = 'ATN' OR CATEGORYID = 1) AND TRUNC(CREATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR'))
   and substr(A.FILENAME,length(A.FILENAME)-7,4) = TO_char(A.CREATE_DATE, 'RRRR')
                                  GROUP BY A.ENTITY_CODE, A.COMPANY_CODE, A.CREATE_DATE, A.FILENAME
                                  ORDER BY A.ENTITY_CODE DESC
                      ) API ON API.SP_CODE = DLU.SP_CODE AND API.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (SELECT SP_CODE, COMPANY_CODE,CUR_DATE, WM_CONCAT(CUR_LATITUDE) CUR_LATITUDE, WM_CONCAT(CUR_LONGITUDE) CUR_LONGITUDE FROM ( SELECT  A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE CUR_DATE, A.LATITUDE CUR_LATITUDE, A.LONGITUDE CUR_LONGITUDE
                                  FROM DIST_LM_LOCATION_TRACKING A
                                  WHERE A.SUBMIT_DATE = (SELECT MAX(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND TRACK_TYPE = 'TRK')
                                  GROUP BY A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE, A.LATITUDE, A.LONGITUDE
                                  ORDER BY A.SP_CODE DESC) GROUP BY SP_CODE, COMPANY_CODE, CUR_DATE 
                      ) LOC ON LOC.SP_CODE = DLU.SP_CODE AND LOC.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (select  SP_CODE, COMPANY_CODE,ATN_DATE, wm_concat(ATN_LATITUDE) ATN_LATITUDE,WM_CONCAT(ATN_LONGITUDE) ATN_LONGITUDE from (SELECT A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE ATN_DATE, A.LATITUDE ATN_LATITUDE, A.LONGITUDE ATN_LONGITUDE
                                  FROM DIST_LM_LOCATION_TRACKING A
                                  WHERE A.SUBMIT_DATE = (SELECT MIN(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND TRACK_TYPE = 'ATN')
                                  GROUP BY A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE, A.LATITUDE, A.LONGITUDE
                                  ORDER BY A.SP_CODE DESC) GROUP BY SP_CODE, COMPANY_CODE, ATN_DATE 
                      ) ATN ON ATN.SP_CODE = DLU.SP_CODE AND ATN.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN (SELECT SP_CODE, COMPANY_CODE, EOD_DATE, WM_CONCAT(EOD_LATITUDE) EOD_LATITUDE,WM_CONCAT(EOD_LONGITUDE) EOD_LONGITUDE FROM ( SELECT A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE EOD_DATE, A.LATITUDE EOD_LATITUDE, A.LONGITUDE EOD_LONGITUDE
                                  FROM DIST_LM_LOCATION_TRACKING A
                                  WHERE A.SUBMIT_DATE = (SELECT MAX(SUBMIT_DATE) FROM DIST_LM_LOCATION_TRACKING WHERE SP_CODE = A.SP_CODE AND TRUNC(SUBMIT_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') AND TRACK_TYPE = 'EOD')
                                  GROUP BY A.SP_CODE, A.COMPANY_CODE, A.SUBMIT_DATE, A.LATITUDE, A.LONGITUDE
                                  ORDER BY A.SP_CODE DESC) GROUP BY SP_CODE, COMPANY_CODE, EOD_DATE
                      ) EOD ON EOD.SP_CODE = DLU.SP_CODE AND EOD.COMPANY_CODE = DLU.COMPANY_CODE
                      LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                      LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                      LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y' AND DRM.IS_CLOSED = 'N'
                      GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), TRIM(DGM.GROUP_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE, LOC.CUR_DATE, LOC.CUR_LATITUDE, LOC.CUR_LONGITUDE, API.ATN_IMAGE, ATN.ATN_DATE, ATN.ATN_LATITUDE, ATN.ATN_LONGITUDE, EOD.EOD_DATE, EOD.EOD_LATITUDE, EOD.EOD_LONGITUDE
                  ) ENT
                  GROUP BY ENT.GROUP_EDESC, ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, ENT.CUR_DATE, ENT.CUR_LATITUDE, ENT.CUR_LONGITUDE, ENT.ATN_IMAGE, ENT.ATN_DATE, ENT.ATN_LATITUDE, ENT.ATN_LONGITUDE, ENT.EOD_DATE, ENT.EOD_LATITUDE, ENT.EOD_LONGITUDE
                  ) RETBL -- Retailer Entity Table
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) VISITED
                        FROM (
                            SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND HES.GROUP_SKU_FLAG = 'I' AND DLU.ACTIVE = 'Y'
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y' AND DRM.IS_CLOSED = 'N'
                            GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                        ) ENT
                        INNER JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                        WHERE DLT.SP_CODE IS NOT NULL
                        GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                  ) RVTBL -- Retailer Visit Table
                  ON RVTBL.SP_CODE = RETBL.SP_CODE AND RVTBL.COMPANY_CODE = RETBL.COMPANY_CODE
            LEFT JOIN (SELECT ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE, COUNT(ENT.ENTITY_CODE) NOT_VISITED
                      FROM (
                          SELECT DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                          FROM DIST_LOGIN_USER DLU
                          INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND HES.GROUP_SKU_FLAG = 'I' AND DLU.ACTIVE = 'Y'
                          LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                          LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                          LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y' AND DRM.IS_CLOSED = 'N'
                          GROUP BY DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                      ) ENT
                      LEFT JOIN (SELECT SP_CODE, CUSTOMER_CODE, COMPANY_CODE FROM DIST_LOCATION_TRACK WHERE TRUNC(UPDATE_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') GROUP BY SP_CODE, CUSTOMER_CODE, COMPANY_CODE) DLT ON DLT.SP_CODE = ENT.SP_CODE AND DLT.CUSTOMER_CODE = ENT.ENTITY_CODE AND DLT.COMPANY_CODE = ENT.COMPANY_CODE
                      WHERE DLT.SP_CODE IS NULL
                      GROUP BY ENT.SP_CODE, ENT.EMPLOYEE_EDESC, ENT.COMPANY_CODE
                ) RNVTBL -- Retailer Not Visited Table
                ON RNVTBL.SP_CODE = RETBL.SP_CODE AND RNVTBL.COMPANY_CODE = RETBL.COMPANY_CODE
            LEFT JOIN (SELECT PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE, COUNT(RPO.RESELLER_CODE) PJP_PRODUCTIVE, SUM(RPO.TOTAL_QUANTITY) PJP_TOTAL_QUANTITY, SUM(RPO.TOTAL_PRICE) PJP_TOTAL_PRICE
                      FROM (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                            FROM DIST_LOGIN_USER DLU
                            INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND HES.GROUP_SKU_FLAG = 'I' AND DLU.ACTIVE = 'Y'
                            LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            LEFT JOIN DIST_ROUTE_ENTITY DRE ON DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRE.ENTITY_TYPE = 'R' AND DRE.DELETED_FLAG = 'N'
                            LEFT JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DRE.ENTITY_CODE AND DRM.COMPANY_CODE = DRE.COMPANY_CODE AND DRM.ACTIVE = 'Y' AND DRM.IS_CLOSED = 'N'
                            GROUP BY DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC), DRM.RESELLER_CODE, TRIM(DRM.RESELLER_NAME), DLU.COMPANY_CODE
                      ) PJPENT
                      LEFT JOIN (SELECT CREATED_BY, RESELLER_CODE, COMPANY_CODE, COUNT(RESELLER_CODE) TOTAL_ORDER, SUM(QUANTITY) TOTAL_QUANTITY, SUM(TOTAL_PRICE) TOTAL_PRICE
                                  FROM DIST_IP_SSR_PURCHASE_ORDER
                                  WHERE TRUNC(ORDER_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                                  GROUP BY CREATED_BY, RESELLER_CODE, COMPANY_CODE
                      ) RPO ON RPO.CREATED_BY = PJPENT.USERID AND RPO.RESELLER_CODE = PJPENT.ENTITY_CODE AND RPO.COMPANY_CODE = PJPENT.COMPANY_CODE
                      GROUP BY PJPENT.SP_CODE, PJPENT.EMPLOYEE_EDESC, PJPENT.COMPANY_CODE
                   ) RPJPTBL -- Retailer PJP Table
                   ON RPJPTBL.SP_CODE = RETBL.SP_CODE AND RPJPTBL.COMPANY_CODE = RETBL.COMPANY_CODE
            LEFT JOIN (SELECT RPO.SP_CODE, RPO.EMPLOYEE_EDESC, RPO.COMPANY_CODE, COUNT(RPO.RESELLER_CODE) NPJP_PRODUCTIVE, SUM(RPO.TOTAL_QUANTITY) NPJP_TOTAL_QUANTITY, SUM(RPO.TOTAL_PRICE) NPJP_TOTAL_PRICE
                      FROM (SELECT A.CREATED_BY, B.SP_CODE, A.RESELLER_CODE, TRIM(C.EMPLOYEE_EDESC) EMPLOYEE_EDESC, A.COMPANY_CODE, COUNT(A.RESELLER_CODE) TOTAL_ORDER, SUM(A.QUANTITY) TOTAL_QUANTITY, SUM(A.TOTAL_PRICE) TOTAL_PRICE
                            FROM DIST_IP_SSR_PURCHASE_ORDER A
                            INNER JOIN DIST_LOGIN_USER B ON B.USERID = A.CREATED_BY AND B.COMPANY_CODE = A.COMPANY_CODE AND B.ACTIVE = 'Y'
                            INNER JOIN HR_EMPLOYEE_SETUP C ON C.EMPLOYEE_CODE = B.SP_CODE AND C.COMPANY_CODE = A.COMPANY_CODE AND C.GROUP_SKU_FLAG = 'I'
                            WHERE TRUNC(A.ORDER_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                            GROUP BY A.CREATED_BY, B.SP_CODE, A.RESELLER_CODE, TRIM(C.EMPLOYEE_EDESC), A.COMPANY_CODE
                      ) RPO
                      LEFT JOIN (SELECT DLU.USERID, DLU.SP_CODE, TRIM(HES.EMPLOYEE_EDESC) EMPLOYEE_EDESC, DRM.RESELLER_CODE ENTITY_CODE, TRIM(DRM.RESELLER_NAME) ENTITY_NAME, DLU.COMPANY_CODE
                                  FROM DIST_LOGIN_USER DLU
                                  INNER JOIN HR_EMPLOYEE_SETUP HES ON HES.EMPLOYEE_CODE = DLU.SP_CODE AND HES.COMPANY_CODE = DLU.COMPANY_CODE AND HES.GROUP_SKU_FLAG = 'I' AND DLU.ACTIVE = 'Y'
                                  LEFT JOIN DIST_ROUTE_DETAIL DRD ON DRD.EMP_CODE = DLU.SP_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE AND DRD.DELETED_FLAG = 'N' AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
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
                        INNER JOIN DIST_LOGIN_USER DLU ON DLU.USERID = DRM.CREATED_BY AND DLU.COMPANY_CODE = DRM.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                        WHERE 1 = 1
                          AND DRM.CREATED_DATE = TO_DATE('{model.ToDate}', 'DD-MON-RRRR')
                          AND DRM.IS_CLOSED = 'N'
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
                    AND DRM.ROUTE_TYPE='D'
                    AND TRUNC(DRD.ASSIGN_DATE) = TO_DATE('{model.ToDate}', 'DD-MON-RRRR') 
                    AND DRD.COMPANY_CODE IN('{companyCode}')  
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
                    AND DRM.ROUTE_TYPE='D'
                    AND TRUNC(DRD.ASSIGN_DATE) = (TO_DATE('{model.ToDate}', 'DD-MON-RRRR') + 1)
                    AND DRD.COMPANY_CODE IN('{companyCode}')  
                  GROUP BY DRD.EMP_CODE, DRM.ROUTE_CODE, DRM.ROUTE_NAME, DRM.COMPANY_CODE
                  ORDER BY UPPER(TRIM(DRM.ROUTE_NAME))
            ) TOM
            GROUP BY TOM.SP_CODE
) TOMTBL ON TOMTBL.SP_CODE = PFMTBL.SP_CODE
WHERE 1 = 1
  AND PFMTBL.COMPANY_CODE IN('{companyCode}')  {spCode}
GROUP BY PFMTBL.GROUP_EDESC, PFMTBL.SP_CODE, PFMTBL.EMPLOYEE_EDESC, 
  TODTBL.TOD_ROUTE_CODE, TODTBL.TOD_ROUTE_NAME, TOMTBL.TOM_ROUTE_CODE, TOMTBL.TOM_ROUTE_NAME, 
  PFMTBL.CUR_DATE, PFMTBL.CUR_LATITUDE, PFMTBL.CUR_LONGITUDE,
  PFMTBL.ATN_IMAGE, PFMTBL.ATN_DATE, PFMTBL.ATN_LATITUDE, PFMTBL.ATN_LONGITUDE, PFMTBL.EOD_DATE, PFMTBL.EOD_LATITUDE, PFMTBL.EOD_LONGITUDE
ORDER BY UPPER(PFMTBL.GROUP_EDESC), UPPER(PFMTBL.EMPLOYEE_EDESC)";
            var result = _objectEntity.SqlQuery<DailyActivityModel>(query).ToList();
            return result;
        }

        public List<EODModel> GetEODDetail(ReportFiltersModel model, string SP_CODE, string type, User userInfo)
        {
            var companyCode = string.Join("','", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            //var BranchFilter = string.Empty;
            //if (model.BranchFilter.Count > 0)
            //{
            //    BranchFilter = string.Format(@" AND  DPO.BRANCH_CODE IN  ('{0}')", string.Join("','", model.BranchFilter).ToString());
            //}
            string Query = "";
            if (type == "Distributor")
            {
                Query = $@"SELECT DISTINCT DS.ORDER_NO,IM.ITEM_EDESC,CS.CUSTOMER_EDESC, DS.ORDER_DATE, DS.CUSTOMER_CODE,DS.BILLING_NAME,DS.ITEM_CODE,DS.MU_CODE,DS.QUANTITY, DS.UNIT_PRICE,DS.TOTAL_PRICE
                                    FROM DIST_IP_SSD_PURCHASE_ORDER DS, DIST_LOGIN_USER LU,SA_CUSTOMER_SETUP CS,IP_ITEM_MASTER_SETUP IM,DIST_PHOTO_INFO DF,DIST_LM_LOCATION_TRACKING LT
                                     WHERE DS.CUSTOMER_CODE = CS.CUSTOMER_CODE AND CS.COMPANY_CODE =DS.COMPANY_CODE AND DS.ITEM_CODE = IM.ITEM_CODE AND DS.COMPANY_CODE= IM.COMPANY_CODE
                                     AND LU.USERID=DS.CREATED_BY AND LU.COMPANY_CODE=DS.COMPANY_CODE
                                     AND LT.SP_CODE=LU.SP_CODE AND LT.TRACK_TYPE='EOD'
                                     AND LU.SP_CODE = '{SP_CODE}' AND LU.ACTIVE = 'Y'
                                     AND DS.COMPANY_CODE IN('{companyCode}')
                                  AND TRUNC(DS.ORDER_DATE) >=TO_DATE('{model.FromDate}','YYYY-MM-DD') AND TRUNC(DS.ORDER_DATE) <=TO_DATE('{model.ToDate}','YYYY-MM-DD')";
            }
            else if (type == "Reseller")
            {
                Query = $@"SELECT DISTINCT DS.ORDER_NO,IM.ITEM_EDESC,CS.CUSTOMER_EDESC, DS.ORDER_DATE, DS.CUSTOMER_CODE,DS.BILLING_NAME,DS.ITEM_CODE,DS.MU_CODE,DS.QUANTITY, DS.UNIT_PRICE,DS.TOTAL_PRICE
                                    FROM DIST_IP_SSR_PURCHASE_ORDER DS, DIST_LOGIN_USER LU,SA_CUSTOMER_SETUP CS,IP_ITEM_MASTER_SETUP IM,DIST_PHOTO_INFO DF,DIST_LM_LOCATION_TRACKING LT
                                     WHERE DS.CUSTOMER_CODE = CS.CUSTOMER_CODE AND CS.COMPANY_CODE =DS.COMPANY_CODE AND DS.ITEM_CODE = IM.ITEM_CODE AND DS.COMPANY_CODE= IM.COMPANY_CODE
                                     AND LU.USERID=DS.CREATED_BY AND LU.COMPANY_CODE=DS.COMPANY_CODE
                                     AND LT.SP_CODE=LU.SP_CODE AND LT.TRACK_TYPE='EOD'
                                     AND LU.SP_CODE = '{SP_CODE}' AND LU.ACTIVE = 'Y'
                                     AND DS.COMPANY_CODE IN('{companyCode}')
                                  AND TRUNC(DS.ORDER_DATE) >=TO_DATE('{model.FromDate}','YYYY-MM-DD') AND TRUNC(DS.ORDER_DATE) <=TO_DATE('{model.ToDate}','YYYY-MM-DD')";
            }
            else if (type == "Outlet")
            {
                Query = $@"SELECT RM.RESELLER_NAME,RM.RESELLER_CONTACT CONTACT,RM.REG_OFFICE_ADDRESS ADDRESS,AM.AREA_NAME
                                FROM DIST_RESELLER_MASTER RM,DIST_AREA_MASTER AM,DIST_LOGIN_USER LU
                                WHERE 1=1
                                AND RM.AREA_CODE = AM.AREA_CODE
                                AND RM.COMPANY_CODE = AM.COMPANY_CODE
                                AND RM.CREATED_BY  = LU.USERID
                                AND RM.COMPANY_CODE = LU.COMPANY_CODE
                                AND RM.SOURCE <> 'WEB' 
                                AND LU.SP_CODE = '{SP_CODE}' AND LU.ACTIVE = 'Y'
                                AND RM.COMPANY_CODE IN('{companyCode}')
                                AND TRUNC(RM.CREATED_DATE) >=TO_DATE('{model.FromDate}','YYYY-MM-DD') AND TRUNC(RM.CREATED_DATE) <=TO_DATE('{model.ToDate}','YYYY-MM-DD')";
            }

            else if (type == "Stock")
            {
                Query = $@"SELECT RM.RESELLER_NAME NAME,IMS.ITEM_EDESC,RS.MU_CODE, RS.CURRENT_STOCK,RS.PURCHASE_QTY,'Reseller' TYPE
                            FROM DIST_RESELLER_STOCK RS,DIST_RESELLER_MASTER RM,IP_ITEM_MASTER_SETUP IMS
                            WHERE  1=1
                                        AND RS.COMPANY_CODE = RM.COMPANY_CODE
                                        AND RS.RESELLER_CODE = RM.RESELLER_CODE
                                        AND RS.ITEM_CODE = IMS.ITEM_CODE
                                        AND RS.COMPANY_CODE = IMS.COMPANY_CODE
                                        AND RS.SP_CODE = '{SP_CODE}'
                                        AND RS.COMPANY_CODE IN ('{companyCode}')
                                        AND RS.DELETED_FLAG = 'N'
                                        AND TRUNC(RS.CREATED_DATE) >=TO_DATE('{model.FromDate}','YYYY-MM-DD') AND TRUNC(RS.CREATED_DATE) <=TO_DATE('{model.ToDate}','YYYY-MM-DD')
                            UNION ALL                    
                            SELECT CS.CUSTOMER_EDESC NAME,IMS.ITEM_EDESC,RS.MU_CODE, RS.CURRENT_STOCK,RS.PURCHASE_QTY,'Distributor' TYPE
                            FROM DIST_DISTRIBUTOR_STOCK RS,SA_CUSTOMER_SETUP CS,IP_ITEM_MASTER_SETUP IMS
                            WHERE  1=1
                                        AND RS.COMPANY_CODE = CS.COMPANY_CODE
                                        AND RS.DISTRIBUTOR_CODE = CS.CUSTOMER_CODE
                                        AND RS.ITEM_CODE = IMS.ITEM_CODE
                                        AND RS.COMPANY_CODE = IMS.COMPANY_CODE
                                        AND RS.SP_CODE = '{SP_CODE}'
                                        AND RS.COMPANY_CODE IN ('{companyCode}')
                                        AND RS.DELETED_FLAG = 'N'
                                        AND TRUNC(RS.CREATED_DATE) >=TO_DATE('{model.FromDate}','YYYY-MM-DD') AND TRUNC(RS.CREATED_DATE) <=TO_DATE('{model.ToDate}','YYYY-MM-DD')  ";
            }
            var result = _objectEntity.SqlQuery<EODModel>(Query).ToList();



            return result;
        }


        public List<AttendanceModel> GetAttendanceReport(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join("','", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            //var BranchFilter = string.Empty;
            //if (model.BranchFilter.Count > 0)
            //{
            //    BranchFilter = string.Format(@" AND  DPO.BRANCH_CODE IN  ('{0}')", string.Join("','", model.BranchFilter).ToString());
            //}
            var SalesPersonFilter = "";
            if (model.ItemBrandFilter.Count > 0)
                SalesPersonFilter = $" AND LU.SP_CODE IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                SalesPersonFilter = $" AND LU.SP_CODE IN ({userInfo.sp_codes})";
            }
            //var query = $@"SELECT LT.SP_CODE,LT.ATT_DATE,LT.CHECKIN,LT.CHECKOUT,LT.ATTNCHECKOUT_TIME,
            //            LU.FULL_NAME,LU.USER_NAME,LU.CONTACT_NO,LU.EMAIL,
            //            GM.GROUP_EDESC,wm_concat(PI.FILENAME)  FILENAME,
            //            CASE LT.ATN_LOCATION
            //                WHEN ',' THEN NULL
            //                ELSE LT.ATN_LOCATION
            //            END ATN_LOCATION,
            //            CASE LT.EOD_LOCATION
            //                WHEN ',' THEN NULL
            //                ELSE LT.EOD_LOCATION
            //            END EOD_LOCATION
            //FROM (
            //  select AB.SP_CODE,AB.ATT_DATE,AB.CHECKIN,AB.CHECKOUT,AB.COMPANY_CODE,AB.ATN_LOCATION,AB.EOD_LOCATION,D.ATTNCHECKOUT_TIME from (SELECT A.SP_CODE,A.ATT_DATE,A.CHECKIN,B.CHECKOUT,A.COMPANY_CODE,A.LATITUDE||','||A.LONGITUDE ATN_LOCATION,B.LATITUDE||','||B.LONGITUDE EOD_LOCATION FROM 
            //               (SELECT DISTINCT SP_CODE, TRUNC(SUBMIT_DATE) ATT_DATE,MIN(SUBMIT_DATE) CHECKIN,COMPANY_CODE,LONGITUDE,LATITUDE  FROM   
            //               DIST_LM_LOCATION_TRACKING   
            //               WHERE TRACK_TYPE = 'ATN'
            //               GROUP BY SP_CODE,TRUNC(SUBMIT_DATE),COMPANY_CODE,LONGITUDE,LATITUDE
            //               ORDER BY SP_CODE, TRUNC(SUBMIT_DATE) ) A
            //           FULL OUTER JOIN
            //               (SELECT DISTINCT SP_CODE, TRUNC(SUBMIT_DATE) ATT_DATE,MAX(SUBMIT_DATE) CHECKOUT,COMPANY_CODE,LONGITUDE,LATITUDE   FROM   
            //               DIST_LM_LOCATION_TRACKING
            //               WHERE TRACK_TYPE = 'EOD'
            //               GROUP BY SP_CODE,TRUNC(SUBMIT_DATE),COMPANY_CODE,LONGITUDE,LATITUDE
            //               ORDER BY SP_CODE,MAX(SUBMIT_DATE) DESC ) B ON A.SP_CODE = B.SP_CODE AND A.COMPANY_CODE = B.COMPANY_CODE AND A.ATT_DATE = B.ATT_DATE) AB
            //                FULL OUTER JOIN
            //                 (SELECT EMPLOYEE_ID SP_CODE,TRUNC(ATTENDANCE_DT) ATTNCHECKOUT_DATE,MAX(ATTENDANCE_TIME) ATTNCHECKOUT_TIME FROM HRIS_ATTENDANCE WHERE ATTENDANCE_FROM ='MOBILE'
            //GROUP BY EMPLOYEE_ID,TRUNC(ATTENDANCE_DT) ORDER BY EMPLOYEE_ID,TRUNC(ATTENDANCE_DT)
            //) D on  AB.SP_CODE = D.SP_CODE 
            // AND AB.ATT_DATE = D.ATTNCHECKOUT_DATE ) LT,
            //           DIST_LOGIN_USER LU,
            //           DIST_GROUP_MASTER GM,
            //           DIST_PHOTO_INFO PI
            //    WHERE LT.SP_CODE = LU.SP_CODE
            //           AND LT.COMPANY_CODE = LU.COMPANY_CODE
            //           AND LU.GROUPID = GM.GROUPID
            //           AND LU.ACTIVE = 'Y' 
            //           AND LU.BRANDING='N'
            //           AND LU.COMPANY_CODE = GM.COMPANY_CODE
            //           AND LT.COMPANY_CODE = PI.COMPANY_CODE(+)
            //           AND LT.ATT_DATE= TRUNC(PI.CREATE_DATE(+))
            //           AND LT.SP_CODE = PI.ENTITY_CODE(+)
            //           AND PI.CATEGORYID(+) = '1' AND PI.ENTITY_TYPE(+) = 'S'
            //           AND LT.COMPANY_CODE IN('{companyCode}')
            //          {SalesPersonFilter}
            //          AND LT.ATT_DATE >=TO_DATE('{model.FromDate}','YYYY-MM-DD') AND LT.ATT_DATE <=TO_DATE('{model.ToDate}','YYYY-MM-DD')
            //GROUP BY  LT.SP_CODE,LT.ATT_DATE,LT.CHECKIN,LT.CHECKOUT,
            //                 LU.FULL_NAME,LU.USER_NAME,LU.CONTACT_NO,
            //                 LU.EMAIL,GM.GROUP_EDESC,LT.ATN_LOCATION,LT.EOD_LOCATION,LT.ATTNCHECKOUT_TIME
            //ORDER BY LT.ATT_DATE DESC,FULL_NAME";

            var query = $@"select ab.*,(select WM_CONCAT( distinct ROUTE_NAME) from DIST_TARGET_ENTITY  where sp_code=ab.sp_code AND TRUNC(ASSIGN_DATE)=TRUNC(att_date)) ROUTE_NAME from (select TA.SP_CODE,TA.ATT_DATE,TA.CHECKIN,TA.CHECKOUT,TA.ATTNCHECKIN_TIME,TA.ATTNCHECKOUT_TIME,C.FIRST_CALL,C.LAST_CALL,TA.FULL_NAME,TA.USER_NAME,TA.CONTACT_NO,TA.EMAIL,TA.GROUP_EDESC,TA.FILENAME,TA.ATN_LOCATION,TA.EOD_LOCATION
 ,CASE TA.ATTNCHECKOUT_TIME WHEN TA.ATTNCHECKIN_TIME THEN NULL ELSE TA.ATTNCHECKOUT_TIME   END ATTNCHECKOUT from(SELECT LT.SP_CODE, LT.ATT_DATE, LT.CHECKIN, LT.CHECKOUT, LT.ATTNCHECKIN_TIME, LT.ATTNCHECKOUT_TIME,
                        LU.FULL_NAME, LU.USER_NAME, LU.CONTACT_NO, LU.EMAIL,
                        GM.GROUP_EDESC, wm_concat(PI.FILENAME)  FILENAME,
                        CASE LT.ATN_LOCATION
                            WHEN ',' THEN NULL
                            ELSE LT.ATN_LOCATION
                        END ATN_LOCATION,
                        CASE LT.EOD_LOCATION
                            WHEN ',' THEN NULL
                            ELSE LT.EOD_LOCATION
                        END EOD_LOCATION
            FROM(
              select AB.SP_CODE, AB.ATT_DATE, AB.CHECKIN, AB.CHECKOUT, AB.COMPANY_CODE, AB.ATN_LOCATION, AB.EOD_LOCATION, D.ATTNCHECKIN_TIME, D.ATTNCHECKOUT_TIME from(
SELECT A.SP_CODE, A.ATT_DATE, A.CHECKIN, B.CHECKOUT, A.COMPANY_CODE, A.LATITUDE || ',' || A.LONGITUDE ATN_LOCATION, B.LATITUDE || ',' || B.LONGITUDE EOD_LOCATION FROM
                           (
                           SELECT DISTINCT SP_CODE, TRUNC(SUBMIT_DATE) ATT_DATE, MIN(SUBMIT_DATE) CHECKIN, COMPANY_CODE, LONGITUDE, LATITUDE  FROM
                           DIST_LM_LOCATION_TRACKING
                           WHERE TRACK_TYPE = 'ATN'
                           GROUP BY SP_CODE, TRUNC(SUBMIT_DATE), COMPANY_CODE, LONGITUDE, LATITUDE
                           ORDER BY SP_CODE, TRUNC(SUBMIT_DATE)) A
                       FULL OUTER JOIN
                           (SELECT DISTINCT SP_CODE, TRUNC(SUBMIT_DATE) ATT_DATE, MAX(SUBMIT_DATE) CHECKOUT, COMPANY_CODE, LONGITUDE, LATITUDE   FROM
                           DIST_LM_LOCATION_TRACKING
                           WHERE TRACK_TYPE = 'EOD'
                           GROUP BY SP_CODE, TRUNC(SUBMIT_DATE), COMPANY_CODE, LONGITUDE, LATITUDE
                           ORDER BY SP_CODE, MAX(SUBMIT_DATE) DESC) B ON A.SP_CODE = B.SP_CODE AND A.COMPANY_CODE = B.COMPANY_CODE AND A.ATT_DATE = B.ATT_DATE) AB
                            FULL OUTER JOIN
                             (
                             SELECT EMPLOYEE_ID SP_CODE, TRUNC(ATTENDANCE_DT) ATTNCHECKOUT_DATE, MIN(ATTENDANCE_TIME) ATTNCHECKIN_TIME, MAX(ATTENDANCE_TIME) ATTNCHECKOUT_TIME
                              FROM HRIS_ATTENDANCE WHERE ATTENDANCE_FROM = 'MOBILE'
            GROUP BY EMPLOYEE_ID, TRUNC(ATTENDANCE_DT) ORDER BY EMPLOYEE_ID, TRUNC(ATTENDANCE_DT)
            ) D on  AB.SP_CODE = D.SP_CODE
             AND AB.ATT_DATE = D.ATTNCHECKOUT_DATE
              ) LT,
                       DIST_LOGIN_USER LU,
                       DIST_GROUP_MASTER GM,
                       DIST_PHOTO_INFO PI
                WHERE LT.SP_CODE = LU.SP_CODE
                       AND LT.COMPANY_CODE = LU.COMPANY_CODE
                       AND LU.GROUPID = GM.GROUPID
                       AND LU.ACTIVE = 'Y'
                       AND LU.BRANDING = 'N'
                       AND LU.COMPANY_CODE = GM.COMPANY_CODE
                       AND LT.COMPANY_CODE = PI.COMPANY_CODE(+)
                       AND LT.ATT_DATE = TRUNC(PI.CREATE_DATE(+))
                       AND LT.SP_CODE = PI.ENTITY_CODE(+)
                       AND PI.CATEGORYID(+) = '1' AND PI.ENTITY_TYPE(+) = 'S'
                          AND LT.COMPANY_CODE IN('{companyCode}')
                      {SalesPersonFilter}
                      AND LT.ATT_DATE >=TO_DATE('{model.FromDate}','YYYY-MM-DD') AND LT.ATT_DATE <=TO_DATE('{model.ToDate}','YYYY-MM-DD')
            GROUP BY  LT.SP_CODE, LT.ATT_DATE, LT.CHECKIN, LT.CHECKOUT,
                             LU.FULL_NAME, LU.USER_NAME, LU.CONTACT_NO,
                             LU.EMAIL, GM.GROUP_EDESC, LT.ATN_LOCATION, LT.EOD_LOCATION, LT.ATTNCHECKIN_TIME, LT.ATTNCHECKOUT_TIME
            ORDER BY LT.ATT_DATE DESC, FULL_NAME) ta   left outer JOIN
              (select SP_CODE, TRUNC(UPDATE_DATE) CALL_DATE, MIN(UPDATE_DATE) FIRST_CALL, MAX(UPDATE_DATE) LAST_CALL, COMPANY_CODE from dist_location_track
              GROUP BY SP_CODE, TRUNC(UPDATE_DATE), COMPANY_CODE
              ORDER BY SP_CODE, TRUNC(UPDATE_DATE)) C ON ta.SP_CODE = C.SP_CODE  AND ta.ATT_DATE = C.CALL_DATE ORDER BY SP_CODE,ATT_DATE) AB";
            var result = _objectEntity.SqlQuery<AttendanceModel>(query).ToList();
            return result;
        }

        public List<AttendanceModel> GetBrandingAttendanceReport(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join("','", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            var SalesPersonFilter = "";
            if (model.ItemBrandFilter.Count > 0)
                SalesPersonFilter = $" AND LU.SP_CODE IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                SalesPersonFilter = $" AND LU.SP_CODE IN ({userInfo.sp_codes})";
            }
            var query = $@"SELECT LT.SP_CODE,LT.ATT_DATE,LT.CHECKIN,LT.CHECKOUT,
                        LU.FULL_NAME,LU.USER_NAME,LU.CONTACT_NO,LU.EMAIL,
                        GM.GROUP_EDESC,wm_concat(PI.FILENAME)  FILENAME,
                        CASE LT.ATN_LOCATION
                            WHEN ',' THEN NULL
                            ELSE LT.ATN_LOCATION
                        END ATN_LOCATION,
                        CASE LT.EOD_LOCATION
                            WHEN ',' THEN NULL
                            ELSE LT.EOD_LOCATION
                        END EOD_LOCATION
            FROM (
               SELECT A.SP_CODE,A.ATT_DATE,A.CHECKIN,B.CHECKOUT,A.COMPANY_CODE,A.LATITUDE||','||A.LONGITUDE ATN_LOCATION,B.LATITUDE||','||B.LONGITUDE EOD_LOCATION FROM 
                           (SELECT DISTINCT SP_CODE, TRUNC(SUBMIT_DATE) ATT_DATE,MIN(SUBMIT_DATE) CHECKIN,COMPANY_CODE,LONGITUDE,LATITUDE  FROM   
                           DIST_LM_LOCATION_TRACKING   
                           WHERE TRACK_TYPE = 'ATN'
                           GROUP BY SP_CODE,TRUNC(SUBMIT_DATE),COMPANY_CODE,LONGITUDE,LATITUDE
                           ORDER BY SP_CODE, TRUNC(SUBMIT_DATE) ) A
                       FULL OUTER JOIN
                           (SELECT DISTINCT SP_CODE, TRUNC(SUBMIT_DATE) ATT_DATE,MAX(SUBMIT_DATE) CHECKOUT,COMPANY_CODE,LONGITUDE,LATITUDE   FROM   
                           DIST_LM_LOCATION_TRACKING
                           WHERE TRACK_TYPE = 'EOD'
                           GROUP BY SP_CODE,TRUNC(SUBMIT_DATE),COMPANY_CODE,LONGITUDE,LATITUDE
                           ORDER BY SP_CODE,MAX(SUBMIT_DATE) DESC ) B ON A.SP_CODE = B.SP_CODE AND A.COMPANY_CODE = B.COMPANY_CODE AND A.ATT_DATE = B.ATT_DATE) LT,
                       DIST_LOGIN_USER LU,
                       DIST_GROUP_MASTER GM,
                       DIST_PHOTO_INFO PI
                WHERE LT.SP_CODE = LU.SP_CODE
                       AND LT.COMPANY_CODE = LU.COMPANY_CODE
                       AND LU.GROUPID = GM.GROUPID
                       AND LU.ACTIVE = 'Y' 
                       AND LU.BRANDING='Y'
                       AND LU.COMPANY_CODE = GM.COMPANY_CODE
                       AND LT.COMPANY_CODE = PI.COMPANY_CODE(+)
                       AND LT.ATT_DATE= TRUNC(PI.CREATE_DATE(+))
                       AND LT.SP_CODE = PI.ENTITY_CODE(+)
                       AND PI.CATEGORYID(+) = '1' AND PI.ENTITY_TYPE(+) = 'S'
                       AND LT.COMPANY_CODE IN('{companyCode}')
                       {SalesPersonFilter}
                       AND LT.ATT_DATE >=TO_DATE('{model.FromDate}','YYYY-MM-DD') AND LT.ATT_DATE <=TO_DATE('{model.ToDate}','YYYY-MM-DD')
            GROUP BY  LT.SP_CODE,LT.ATT_DATE,LT.CHECKIN,LT.CHECKOUT,
                             LU.FULL_NAME,LU.USER_NAME,LU.CONTACT_NO,
                             LU.EMAIL,GM.GROUP_EDESC,LT.ATN_LOCATION,LT.EOD_LOCATION
            ORDER BY LT.ATT_DATE DESC,FULL_NAME";
            var result = _objectEntity.SqlQuery<AttendanceModel>(query).ToList();
            return result;
        }

        public List<ClosingReportModel> GetOutletClosingReport(ReportFiltersModel model, User UserInfo)
        {
            var ResellerFilter = string.Empty;
            var CustomerFilter = string.Empty;
            var result = new List<ClosingReportModel>();
            if (UserInfo.LoginType.Trim().ToUpper() == "D" || UserInfo.LoginType.Trim().ToUpper() == "DISTRIBUTOR")
            {
                ResellerFilter = $" AND RS.RESELLER_CODE IN (SELECT RESELLER_CODE FROM DIST_RESELLER_MASTER WHERE DISTRIBUTOR_CODE='{UserInfo.DistributerNo}' AND IS_CLOSED = 'N')";
                if (model.CustomerFilter.Count > 0)
                {
                    CustomerFilter = $@"AND RS.RESELLER_CODE IN ('{string.Join("','", model.CustomerFilter)}')";
                }
            }
            else
            {
                {
                    if (model.CustomerFilter.Count > 0)
                        ResellerFilter = $" AND RS.RESELLER_CODE IN (SELECT RESELLER_CODE FROM DIST_RESELLER_MASTER WHERE DISTRIBUTOR_CODE IN ('{string.Join("','", model.CustomerFilter)}') AND IS_CLOSED = 'N')";
                }
            }

            var spCode = "";
            if (model.ItemBrandFilter.Count > 0)
                spCode = $" AND DRS.SP_CODE IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(UserInfo.sp_codes))
            {
                spCode = $" AND DRS.SP_CODE IN ({UserInfo.sp_codes})";
            }

            //In customer filter CUSTOMER_CODE is replaced by DISTRIBUTOR_CODE ↵
            //since opening stock table has distributer code column
            string OpeningQury = $@"SELECT IMS.ITEM_EDESC,IIUS.MU_CODE,RS.ITEM_CODE,RS.RESELLER_CODE CUSTOMER_CODE,RS.CREATED_DATE ORDER_DATE,RM.RESELLER_NAME CUSTOMER_EDESC,SUM(RS.CURRENT_STOCK) QUANTITY
                FROM (SELECT DISTINCT DRS.*  FROM
                        (SELECT RESELLER_CODE,ITEM_CODE,COMPANY_CODE,MAX(CREATED_DATE) CREATED_DATE
                            FROM DIST_RESELLER_STOCK
                            GROUP BY RESELLER_CODE,ITEM_CODE,COMPANY_CODE) MXD
                            INNER JOIN  DIST_RESELLER_STOCK DRS ON MXD.COMPANY_CODE=DRS.COMPANY_CODE
                                                        AND MXD.CREATED_DATE=DRS.CREATED_DATE
                                                        AND MXD.RESELLER_CODE=DRS.RESELLER_CODE
                                                        AND DRS.ITEM_CODE=MXD.ITEM_CODE WHERE 1=1 {spCode}) RS
                INNER JOIN DIST_RESELLER_MASTER RM ON RS.RESELLER_CODE =RM.RESELLER_CODE AND RM.COMPANY_CODE=RS.COMPANY_CODE
                INNER JOIN IP_ITEM_MASTER_SETUP IMS ON RS.ITEM_CODE=IMS.ITEM_CODE AND RS.COMPANY_CODE=IMS.COMPANY_CODE
                LEFT JOIN IP_ITEM_UNIT_SETUP IIUS ON IMS.ITEM_CODE = IIUS.ITEM_CODE AND IMS.COMPANY_CODE = IIUS.COMPANY_CODE
                WHERE 1=1 AND RM.IS_CLOSED = 'N'
                {ResellerFilter}{CustomerFilter}
                GROUP BY IMS.ITEM_EDESC,IIUS.MU_CODE,RS.ITEM_CODE,RS.RESELLER_CODE,RS.CREATED_DATE,RM.RESELLER_NAME
                ORDER BY RS.RESELLER_CODE";

            string SalesQuery = $@"SELECT IMS.ITEM_EDESC,IIUS.MU_CODE,RS.ITEM_CODE,RS.RESELLER_CODE CUSTOMER_CODE,RM.RESELLER_NAME CUSTOMER_EDESC,SUM(RS.QUANTITY) QUANTITY
                FROM DIST_IP_SSR_PURCHASE_ORDER RS
                    INNER JOIN DIST_RESELLER_MASTER RM ON RS.RESELLER_CODE=RM.RESELLER_CODE AND RS.COMPANY_CODE=RM.COMPANY_CODE
                    INNER JOIN IP_ITEM_MASTER_SETUP IMS ON RS.ITEM_CODE=IMS.ITEM_CODE AND RS.COMPANY_CODE=IMS.COMPANY_CODE
                    LEFT JOIN IP_ITEM_UNIT_SETUP IIUS ON IMS.ITEM_CODE = IIUS.ITEM_CODE AND IMS.COMPANY_CODE = IIUS.COMPANY_CODE
                WHERE TO_DATE(ORDER_DATE)<=TO_DATE('{model.FromDate.ToUpper()}','RRRR-MON-DD') {ResellerFilter}{CustomerFilter}
                    AND RS.APPROVED_FLAG='Y' AND RM.IS_CLOSED = 'N'
                GROUP BY IMS.ITEM_EDESC,IIUS.MU_CODE,RS.ITEM_CODE,RS.RESELLER_CODE,RM.RESELLER_NAME";

            var OpeningData = _objectEntity.SqlQuery<ClosingReportModel>(OpeningQury).ToList();
            var SalesData = _objectEntity.SqlQuery<ClosingReportModel>(SalesQuery).ToList();

            OpeningData.ForEach(x =>
            {
                var salesObj = SalesData.FirstOrDefault(s => s.ITEM_CODE == x.ITEM_CODE && s.CUSTOMER_CODE == x.CUSTOMER_CODE);
                if (salesObj != null)
                {
                    x.OPENING_QTY = x.QUANTITY;
                    x.SALES_QTY = salesObj.QUANTITY;
                    x.CLOSING_QTY = salesObj.QUANTITY - x.QUANTITY;
                    SalesData.Remove(salesObj);
                }
                else
                {
                    x.OPENING_QTY = x.QUANTITY;
                    x.SALES_QTY = 0;
                }
            });
            SalesData.ForEach(x =>
            {
                x.OPENING_QTY = 0;
                x.SALES_QTY = x.QUANTITY;
                x.CLOSING_QTY = 0 - x.QUANTITY;
            });
            result.AddRange(OpeningData);
            result.AddRange(SalesData);
            return result;
        }

        public List<VisitImageModel> GetVisiterList(ReportFiltersModel filter, User userInfo, string Distributor = "", string Reseller = "")
        {

            var selectQuery = string.Empty;
            var TypeFilter = string.Empty;
            if (!string.IsNullOrWhiteSpace(Reseller) && !string.IsNullOrWhiteSpace(Distributor))
                TypeFilter = $" AND TYPE IN ('R','D') AND ENTITY_CODE IN ('{Reseller}','{Distributor}')";
            else if (!string.IsNullOrWhiteSpace(Reseller))
                TypeFilter = $"AND TYPE='R' AND ENTITY_CODE='{Reseller}'";
            else if (!string.IsNullOrWhiteSpace(Distributor))
                TypeFilter = $"AND TYPE='D' AND ENTITY_CODE='{Distributor}'";


            selectQuery = $@"SELECT * FROM (
                    SELECT DVI.IMAGE_CODE,DVI.IMAGE_NAME,DVI.IMAGE_TITLE,DVI.IMAGE_DESC,DVI.UPLOAD_DATE,
                                   DVI.COMPANY_CODE,DRM.RESELLER_NAME AS ENTITY_NAME,DLU.FULL_NAME,DIC.CATEGORY_EDESC,
                                   DIC.CATEGORY_CODE,DVI.TYPE,DVI.SP_CODE,DVI.ENTITY_CODE
                    FROM DIST_VISIT_IMAGE DVI
                    INNER JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DVI.ENTITY_CODE AND DRM.COMPANY_CODE = DVI.COMPANY_CODE
                    INNER JOIN DIST_LOGIN_USER DLU ON DVI.SP_CODE = DLU.SP_CODE AND DVI.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                    INNER JOIN DIST_IMAGE_CATEGORY DIC ON DVI.CATEGORYID = DIC.CATEGORYID AND DVI.COMPANY_CODE = DIC.COMPANY_CODE
            UNION ALL
                    SELECT DVI.IMAGE_CODE,DVI.IMAGE_NAME,DVI.IMAGE_TITLE,DVI.IMAGE_DESC,DVI.UPLOAD_DATE,
                              DVI.COMPANY_CODE,SCS.CUSTOMER_EDESC AS ENTITY_NAME,DLU.FULL_NAME,DIC.CATEGORY_EDESC,
                              DIC.CATEGORY_CODE,DVI.TYPE,DVI.SP_CODE,DVI.ENTITY_CODE
                    FROM DIST_VISIT_IMAGE DVI INNER JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DVI.ENTITY_CODE AND SCS.COMPANY_CODE = DVI.COMPANY_CODE
                    INNER JOIN DIST_LOGIN_USER DLU ON DVI.SP_CODE = DLU.SP_CODE AND DVI.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                    INNER JOIN DIST_IMAGE_CATEGORY DIC ON DVI.CATEGORYID = DIC.CATEGORYID AND DVI.COMPANY_CODE = DIC.COMPANY_CODE)
                    WHERE 1=1
                               AND COMPANY_CODE = '{userInfo.company_code}'
                               AND TRUNC(UPLOAD_DATE) BETWEEN '{filter.FromDate}' AND '{filter.ToDate}'
                               AND SP_CODE IN ('{String.Join("','", filter.SalesPersonFilter)}') {TypeFilter}";
            var result = _objectEntity.SqlQuery<VisitImageModel>(selectQuery).ToList();
            return result;
        }
        public List<VisitImageModel> GetBrandingVisiterList(ReportFiltersModel filter, User userInfo, string Distributor = "", string Reseller = "")
        {

            var selectQuery = string.Empty;
            var TypeFilter = string.Empty;
            if (!string.IsNullOrWhiteSpace(Reseller) && !string.IsNullOrWhiteSpace(Distributor))
                TypeFilter = $" AND TYPE IN ('R','D') AND ENTITY_CODE IN ('{Reseller}','{Distributor}')";
            else if (!string.IsNullOrWhiteSpace(Reseller))
                TypeFilter = $"AND TYPE='R' AND ENTITY_CODE='{Reseller}'";
            else if (!string.IsNullOrWhiteSpace(Distributor))
                TypeFilter = $"AND TYPE='D' AND ENTITY_CODE='{Distributor}'";


            selectQuery = $@"SELECT * FROM (
                    SELECT DVI.IMAGE_CODE,DVI.IMAGE_NAME,DVI.IMAGE_TITLE,DVI.IMAGE_DESC,DVI.UPLOAD_DATE,
                                   DVI.COMPANY_CODE,DRM.RESELLER_NAME AS ENTITY_NAME,DLU.FULL_NAME,DIC.CATEGORY_EDESC,
                                   DIC.CATEGORY_CODE,DVI.TYPE,DVI.SP_CODE,DVI.ENTITY_CODE,DLU.BRANDING
                    FROM DIST_VISIT_IMAGE DVI
                    INNER JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DVI.ENTITY_CODE AND DRM.COMPANY_CODE = DVI.COMPANY_CODE
                    INNER JOIN DIST_LOGIN_USER DLU ON DVI.SP_CODE = DLU.SP_CODE AND DVI.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                    INNER JOIN DIST_IMAGE_CATEGORY DIC ON DVI.CATEGORYID = DIC.CATEGORYID AND DVI.COMPANY_CODE = DIC.COMPANY_CODE
            UNION ALL
                    SELECT DVI.IMAGE_CODE,DVI.IMAGE_NAME,DVI.IMAGE_TITLE,DVI.IMAGE_DESC,DVI.UPLOAD_DATE,
                              DVI.COMPANY_CODE,SCS.CUSTOMER_EDESC AS ENTITY_NAME,DLU.FULL_NAME,DIC.CATEGORY_EDESC,
                              DIC.CATEGORY_CODE,DVI.TYPE,DVI.SP_CODE,DVI.ENTITY_CODE,DLU.BRANDING
                    FROM DIST_VISIT_IMAGE DVI INNER JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DVI.ENTITY_CODE AND SCS.COMPANY_CODE = DVI.COMPANY_CODE
                    INNER JOIN DIST_LOGIN_USER DLU ON DVI.SP_CODE = DLU.SP_CODE AND DVI.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                    INNER JOIN DIST_IMAGE_CATEGORY DIC ON DVI.CATEGORYID = DIC.CATEGORYID AND DVI.COMPANY_CODE = DIC.COMPANY_CODE)
                    WHERE 1=1
                            AND BRANDING='Y'
                               AND COMPANY_CODE = '{userInfo.company_code}'
                               AND TRUNC(UPLOAD_DATE) BETWEEN '{filter.FromDate}' AND '{filter.ToDate}'
                               AND SP_CODE IN ('{String.Join("','", filter.SalesPersonFilter)}') {TypeFilter}";
            var result = _objectEntity.SqlQuery<VisitImageModel>(selectQuery).ToList();
            return result;
        }

        public List<VisitImageModel> GetVisiterListCondition(ReportFiltersModel filter, User userInfo, string Distributor = "", string Reseller = "")
        {

            var selectQuery = string.Empty;
            var TypeFilter = string.Empty;
            if (!string.IsNullOrWhiteSpace(Reseller) && !string.IsNullOrWhiteSpace(Distributor))
                TypeFilter = $" AND TYPE IN ('R','D') AND ENTITY_CODE IN ('{Reseller}','{Distributor}')";
            else if (!string.IsNullOrWhiteSpace(Reseller))
                TypeFilter = $"AND TYPE='R' AND ENTITY_CODE='{Reseller}'";
            else if (!string.IsNullOrWhiteSpace(Distributor))
                TypeFilter = $"AND TYPE='D' AND ENTITY_CODE='{Distributor}'";

            var spCode = string.Empty;
            if (filter.SalesPersonFilter.Count > 0)
                spCode = $" AND SP_CODE IN('{String.Join("', '", filter.SalesPersonFilter)}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                spCode = $@" AND SP_CODE IN ({userInfo.sp_codes})";


            selectQuery = $@"SELECT * FROM (
                    SELECT * FROM (
                    SELECT DVI.IMAGE_CODE,DVI.IMAGE_NAME,DVI.IMAGE_TITLE,DVI.IMAGE_DESC,DVI.UPLOAD_DATE,
                                   DVI.COMPANY_CODE,DRM.RESELLER_NAME AS ENTITY_NAME,DLU.FULL_NAME,DIC.CATEGORY_EDESC,
                                   DIC.CATEGORY_CODE,DVI.TYPE,DVI.SP_CODE,DVI.ENTITY_CODE
                    FROM DIST_VISIT_IMAGE DVI
                    INNER JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DVI.ENTITY_CODE AND DRM.COMPANY_CODE = DVI.COMPANY_CODE
                    INNER JOIN DIST_LOGIN_USER DLU ON DVI.SP_CODE = DLU.SP_CODE AND DVI.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                    INNER JOIN DIST_IMAGE_CATEGORY DIC ON DVI.CATEGORYID = DIC.CATEGORYID AND DVI.COMPANY_CODE = DIC.COMPANY_CODE
                    WHERE DIC.CATEGORY_EDESC='Owner Pic'
                    union all
                                SELECT 0 as IMAGE_CODE,DVI.filename as IMAGE_NAME,dvi.description as IMAGE_TITLE,dvi.description as IMAGE_DESC,DVI.create_date as UPLOAD_DATE,
                                   DVI.COMPANY_CODE,DRM.RESELLER_NAME AS ENTITY_NAME,'' as FULL_NAME,'Reseller' as CATEGORY_EDESC,
                                   'Reseller' as CATEGORY_CODE,DVI.media_type,'' as SP_CODE,DVI.ENTITY_CODE
                    FROM DIST_PHOTO_INFO DVI
                    INNER JOIN DIST_RESELLER_MASTER DRM ON DRM.RESELLER_CODE = DVI.ENTITY_CODE AND DRM.COMPANY_CODE = DVI.COMPANY_CODE
                    and    dvi.media_type in ('PCONTACT','STORE')
            UNION ALL
                    SELECT DVI.IMAGE_CODE,DVI.IMAGE_NAME,DVI.IMAGE_TITLE,DVI.IMAGE_DESC,DVI.UPLOAD_DATE,
                              DVI.COMPANY_CODE,SCS.CUSTOMER_EDESC AS ENTITY_NAME,DLU.FULL_NAME,DIC.CATEGORY_EDESC,
                              DIC.CATEGORY_CODE,DVI.TYPE,DVI.SP_CODE,DVI.ENTITY_CODE
                    FROM DIST_VISIT_IMAGE DVI INNER JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE = DVI.ENTITY_CODE AND SCS.COMPANY_CODE = DVI.COMPANY_CODE
                    INNER JOIN DIST_LOGIN_USER DLU ON DVI.SP_CODE = DLU.SP_CODE AND DVI.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                    INNER JOIN DIST_IMAGE_CATEGORY DIC ON DVI.CATEGORYID = DIC.CATEGORYID AND DVI.COMPANY_CODE = DIC.COMPANY_CODE AND DIC.CATEGORY_EDESC='Owner Pic')
                    WHERE 1=1)
                    WHERE 1=1
                               AND COMPANY_CODE = '{userInfo.company_code}' {spCode}
                               AND TRUNC(UPLOAD_DATE) BETWEEN '{filter.FromDate}' AND '{filter.ToDate}'
                                {TypeFilter}";
            var result = _objectEntity.SqlQuery<VisitImageModel>(selectQuery).ToList();
            if (result.Count > 0)
            {
                if (filter.ItemBrandFilter.Count > 0)
                    result = result.Where(p => filter.ItemBrandFilter.Contains(p.SP_CODE)).ToList();
            }
            return result;
        }

        public List<CompReportModel> GetCompReport(ReportFiltersModel filter, User userInfo, string Item_code)
        {
            var Query = $@"SELECT DCA.ENTITY_CODE,DCA.ENTITY_TYPE,TO_CHAR(DCA.ITEM_CODE) ITEM_CODE,IMS.ITEM_EDESC,DCA.QUESTION_ID,DCF.COL_NAME,DCA.ANSWER
                    FROM DIST_COMP_QA DCA
                    INNER JOIN DIST_COMP_FIELDS DCF ON DCF.ITEM_CODE = DCA.ITEM_CODE AND DCF.COMPANY_CODE = DCA.COMPANY_CODE AND DCF.FIELD_ID = DCA.QUESTION_ID
                    INNER JOIN DIST_COMP_ITEM_MASTER  IMS ON IMS.ITEM_ID = DCA.COMP_ITEM_CODE AND IMS.COMPANY_CODE = DCA.COMPANY_CODE
                    WHERE DCA.COMPANY_CODE = '{userInfo.company_code}'
                        AND DCA.ITEM_CODE IN (SELECT ITEM_CODE FROM IP_ITEM_MASTER_SETUP
                                WHERE MASTER_ITEM_CODE LIKE (SELECT MASTER_ITEM_CODE FROM IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '{Item_code}' AND GROUP_SKU_FLAG = 'G' AND ROWNUM=1)||'%'
                        AND GROUP_SKU_FLAG = 'I') AND TRIM(COL_NAME) = 'Available'";
            var data = _objectEntity.SqlQuery<CompReportTempModel>(Query).GroupBy(x => x.ITEM_EDESC);

            var result = new List<CompReportModel>();
            foreach (var item in data)
            {
                var availableCount = item.Where(x => x.ANSWER == "1").Count();
                var ReportItem = new CompReportModel
                {
                    ITEM_EDESC = item.Key,
                    PERCENT = Math.Round(((decimal)availableCount / (decimal)item.Count()) * 100, 2),
                    QUANTITY = availableCount,
                    TOTAL = item.Count()
                };
                result.Add(ReportItem);
            }

            return result;
        }

        public List<CompReportModel> GetCompReportMonthly(ReportFiltersModel filter, User userInfo, string Item_code, string Category)
        {
            var Query = $@"SELECT DCA.ENTITY_CODE,DCA.ENTITY_TYPE,TO_CHAR(DCA.ITEM_CODE) ITEM_CODE,IMS.ITEM_EDESC,DCA.QUESTION_ID,DCF.COL_NAME,DCA.ANSWER,
                        NVL(IMS.ITEM_CATEGORY,'Empty') CATEGORY,FN_BS_MONTH(SUBSTR(BS_DATE(DCA.CREATED_DATE),6,2)) AS NEPALI_MONTH,
                        TO_CHAR(DCA.CREATED_DATE, 'Month') ENGLISH_MONTH
                    FROM DIST_COMP_QA DCA
                    INNER JOIN DIST_COMP_FIELDS DCF ON DCF.ITEM_CODE = DCA.ITEM_CODE AND DCF.COMPANY_CODE = DCA.COMPANY_CODE AND DCF.FIELD_ID = DCA.QUESTION_ID
                    INNER JOIN DIST_COMP_ITEM_MASTER  IMS ON IMS.ITEM_ID = DCA.COMP_ITEM_CODE AND IMS.COMPANY_CODE = DCA.COMPANY_CODE
                    WHERE DCA.COMPANY_CODE = '{userInfo.company_code}'
                    AND DCA.ITEM_CODE IN (SELECT ITEM_CODE FROM IP_ITEM_MASTER_SETUP
                                WHERE MASTER_ITEM_CODE LIKE (SELECT MASTER_ITEM_CODE FROM IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '{Item_code}' AND GROUP_SKU_FLAG = 'G' AND ROWNUM=1)||'%'
                                AND GROUP_SKU_FLAG = 'I')
                    AND TRIM(COL_NAME) = 'Available'";

            var result = new List<CompReportModel>();
            if (string.IsNullOrWhiteSpace(Category))
            {
                Query += " ORDER BY DCA.CREATED_DATE";
                var tempData = _objectEntity.SqlQuery<CompReportTempModel>(Query);
                var data = from a in tempData
                           group a by
                           new { a.CATEGORY, a.NEPALI_MONTH };
                foreach (var item in data)
                {
                    var availableCount = item.Where(x => x.ANSWER == "1").Count();
                    var ReportItem = new CompReportModel
                    {
                        ITEM_EDESC = item.Key.CATEGORY,
                        PERCENT = Math.Round(((decimal)availableCount / (decimal)item.Count()) * 100, 2),
                        QUANTITY = availableCount,
                        TOTAL = item.Count(),
                        NEPALI_MONTH = item.Key.NEPALI_MONTH
                    };
                    result.Add(ReportItem);
                }
            }
            else
            {
                if (Category == "Empty")
                    Query += $@"  AND IMS.ITEM_CATEGORY IS NULL ORDER BY DCA.CREATED_DATE";
                else
                    Query += $@"  AND IMS.ITEM_CATEGORY = '{Category}' ORDER BY DCA.CREATED_DATE";

                var tempData = _objectEntity.SqlQuery<CompReportTempModel>(Query);
                var data = from a in tempData
                           group a by
                           new { a.ITEM_EDESC, a.NEPALI_MONTH };
                foreach (var item in data)
                {
                    var availableCount = item.Where(x => x.ANSWER == "1").Count();
                    var ReportItem = new CompReportModel
                    {
                        ITEM_EDESC = item.Key.ITEM_EDESC,
                        PERCENT = Math.Round(((decimal)availableCount / (decimal)item.Count()) * 100, 2),
                        QUANTITY = availableCount,
                        TOTAL = item.Count(),
                        NEPALI_MONTH = item.Key.NEPALI_MONTH
                    };
                    result.Add(ReportItem);
                }
            }

            return result;
        }

        public List<SPDistanceModel> GetSPTravelReport(ReportFiltersModel filter, User userInfo)
        {
            var result = new List<SPDistanceModel>();
            var filterString = "";
            //if (filter.DistEmployeeFilter.Count > 0)
            //    filterString += $" AND LOC.SP_CODE IN ('{string.Join("','", filter.DistEmployeeFilter)}')";
            //else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            //    filterString = $" AND LOC.SP_CODE IN ({userInfo.sp_codes})";

            var itemBrandSpCode = filter.ItemBrandFilter;
            if (filter.ItemBrandFilter.Count > 0)
            {
                if (filter.DistEmployeeFilter.Count > 0)
                    itemBrandSpCode = filter.DistEmployeeFilter.Where(p => filter.ItemBrandFilter.Contains(p.ToString())).ToList();
                filterString = $" AND LOC.SP_CODE IN ('{string.Join("','", itemBrandSpCode)}')";
            }
            else if (filter.DistEmployeeFilter.Count > 0)
                filterString += $" AND LOC.SP_CODE IN ('{string.Join("','", filter.DistEmployeeFilter)}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                filterString = $" AND LOC.SP_CODE IN ({userInfo.sp_codes})";


            var Query = $@"SELECT DLU.FULL_NAME AS EMPLOYEE_EDESC, LOC.SP_CODE, LOC.UPDATE_DATE, TO_NUMBER(LOC.LONGITUDE) LONGITUDE,TO_NUMBER(LOC.LATITUDE) LATITUDE
                    FROM (SELECT SP_CODE, SUBMIT_DATE AS UPDATE_DATE, LONGITUDE, LATITUDE, COMPANY_CODE FROM DIST_LM_LOCATION_TRACKING
                    UNION ALL
                    SELECT SP_CODE, UPDATE_DATE, LONGITUDE, LATITUDE, COMPANY_CODE FROM DIST_LOCATION_TRACK) LOC
                    JOIN DIST_LOGIN_USER DLU ON DLU.SP_CODE = LOC.SP_CODE AND DLU.COMPANY_CODE = LOC.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                    WHERE LOC.COMPANY_CODE = '{userInfo.company_code}' AND LONGITUDE IS NOT NULL AND LATITUDE IS NOT NULL {filterString}
                    AND LOC.UPDATE_DATE BETWEEN TO_DATE('{filter.FromDate.ToUpper()}','RRRR-MON-DD') AND TO_DATE('{filter.ToDate.ToUpper()}','RRRR-MON-DD')
                    ORDER BY LOC.UPDATE_DATE";
            var travelData = _objectEntity.SqlQuery<SPTraveModel>(Query).GroupBy(x => x.SP_CODE);

            foreach (var item in travelData)
            {
                var listItems = item.ToList();
                double totalDistance = 0;
                for (int i = 0; i < item.Count() - 2; i++)
                {
                    var distance = CalcDistance(listItems[i].LATITUDE, listItems[i].LONGITUDE, listItems[i + 1].LATITUDE, listItems[i + 1].LONGITUDE);
                    totalDistance += distance;
                }
                result.Add(new SPDistanceModel
                {
                    DISTANCE = Math.Round(totalDistance, 3),
                    EMPLOYEE_EDESC = listItems[0].EMPLOYEE_EDESC,
                    SP_CODE = item.Key
                });
            }

            return result;
        }

        public List<SPTraveModel> GetSPRouteReport(ReportFiltersModel filter, User userInfo, string source)
        {
            var filterString = "";
            var SourceQuery = "";

            var itemBrandSpCode = filter.ItemBrandFilter;
            if (filter.ItemBrandFilter.Count > 0)
            {
                if (filter.DistEmployeeFilter.Count > 0)
                    itemBrandSpCode = filter.DistEmployeeFilter.Where(p => filter.ItemBrandFilter.Contains(p.ToString())).ToList();
                filterString = $" AND LOC.SP_CODE IN ('{string.Join("','", itemBrandSpCode)}')";
            }

            else if (filter.DistEmployeeFilter.Count > 0)
                filterString += $"AND LOC.SP_CODE IN ('{string.Join("','", filter.DistEmployeeFilter)}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                filterString = $" AND LOC.SP_CODE IN ({userInfo.sp_codes})";

            if (source == "V")
                SourceQuery = @"SELECT SP_CODE, UPDATE_DATE AS UPDATE_DATE, LONGITUDE, LATITUDE, COMPANY_CODE,'Extra Visit' customer_edesc,remarks FROM DIST_LOCATION_TRACK
                    UNION ALL
                     select SP_CODE, VISIT_DATE AS UPDATE_DATE,LONGITUDE,LATITUDE, COMPANY_CODE,'Extra Visit' customer_edesc,remarks from DIST_EXTRA_ACTIVITY";
            else if (source == "L")
                SourceQuery = "SELECT SP_CODE, SUBMIT_DATE AS UPDATE_DATE, LONGITUDE, LATITUDE, COMPANY_CODE,track_type customer_edesc,'-' remarks FROM DIST_LM_LOCATION_TRACKING";
            else if (source == "B")
                SourceQuery = @"SELECT SP_CODE, SUBMIT_DATE AS UPDATE_DATE, LONGITUDE, LATITUDE, COMPANY_CODE,track_type customer_edesc,'-' remarks FROM DIST_LM_LOCATION_TRACKING
                    UNION ALL
                     select SP_CODE, VISIT_DATE AS UPDATE_DATE, LONGITUDE, LATITUDE, COMPANY_CODE,'Extra Visit' customer_edesc,remarks from DIST_EXTRA_ACTIVITY
                    UNION ALL
                    select d.SP_CODE, d.UPDATE_DATE, d.LONGITUDE, d.LATITUDE, d.COMPANY_CODE,c.customer_edesc,d.remarks from DIST_LOCATION_TRACK d inner join sa_customer_setup c on 
					d.customer_code=c.customer_code where d.customer_type='D'
					union all
					select d.SP_CODE, d.UPDATE_DATE, d.LONGITUDE, d.LATITUDE, d.COMPANY_CODE,c.RESELLER_NAME customer_edesc,d.remarks from DIST_LOCATION_TRACK d inner join DIST_RESELLER_MASTER c on 
					d.customer_code=c.RESELLER_CODE where d.customer_type='R'";

            var Query = $@"SELECT DISTINCT a.EMPLOYEE_EDESC,a.SP_CODE,a.LONGITUDE,a.LATITUDE,TO_CHAR(a.UPDATE_DATE,'DD-MON-YYYY') UPDATE_DATE1,a.UPDATE_DATE,a.customer_edesc,a.remarks
                    FROM (
                    SELECT DLU.FULL_NAME AS EMPLOYEE_EDESC, LOC.SP_CODE, LOC.UPDATE_DATE,LOC.customer_edesc,LOC.remarks, TO_NUMBER(LOC.LONGITUDE)LONGITUDE,TO_NUMBER(LOC.LATITUDE) LATITUDE
                    FROM ({SourceQuery}) LOC
                    JOIN DIST_LOGIN_USER DLU ON DLU.SP_CODE = LOC.SP_CODE AND DLU.COMPANY_CODE = LOC.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                    WHERE LOC.COMPANY_CODE = '{userInfo.company_code}' AND LONGITUDE IS NOT NULL AND LATITUDE IS NOT NULL {filterString}
                    AND TO_DATE(LOC.UPDATE_DATE) BETWEEN TO_DATE('{filter.FromDate.ToUpper()}','RRRR-MON-DD') AND TO_DATE('{filter.ToDate.ToUpper()}','RRRR-MON-DD')
                    ORDER BY LOC.UPDATE_DATE)   a order by UPDATE_DATE";
            var travelData = _objectEntity.SqlQuery<SPTraveModel>(Query).ToList();

            return travelData;
        }

        public List<SPDistanceModel> GetSPVisitEntity(ReportFiltersModel model, User userInfo)
        {
            var filter = "";
            if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                filter = $" AND DLT.SP_CODE IN  ({userInfo.sp_codes})";
            }
            var query = $@"SELECT DLT.SP_CODE,DLU.FULL_NAME EMPLOYEE_EDESC,
                COUNT(DLT.CUSTOMER_CODE) ENTITY_COUNT
                FROM DIST_LOCATION_TRACK DLT
                INNER JOIN DIST_LOGIN_USER DLU ON DLT.SP_CODE = DLU.SP_CODE AND DLT.COMPANY_CODE = DLU.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                WHERE TRUNC(DLT.UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}','YYYY-MM-DD') AND TO_DATE('{model.ToDate}','YYYY-MM-DD')
                            AND DLT.COMPANY_CODE  = '{userInfo.company_code}' {filter}
                GROUP BY DLT.SP_CODE,DLU.FULL_NAME";
            var data = _objectEntity.SqlQuery<SPDistanceModel>(query).ToList();
            return data;
        }

        #region distanceCalculation
        private double CalcDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var sCoord = new GeoCoordinate(lat1, lon1);
            var eCoord = new GeoCoordinate(lat2, lon2);
            var result = sCoord.GetDistanceTo(eCoord);
            return result / 1000;
        }
        #endregion distanceCalculation

        public List<ResellerListModel> GetResellerDetailReport(ReportFiltersModel filter, User userInfo, string source)
        {
            try
            {
                var query = $@"SELECT DREM.RESELLER_CODE,DREM.RESELLER_NAME,DREM.REG_OFFICE_ADDRESS ADDRESS,DREM.RESELLER_CONTACT,DREM.EMAIL,
                                DAM.AREA_NAME,DOUT.TYPE_EDESC OUTLET_TYPE,DOUT.TYPE_CODE,DOST.SUBTYPE_EDESC OUTLET_SUBTYPE,DOST.SUBTYPE_CODE,
                                (DREM.CONTACT_SUFFIX || ' ' || DREM.CONTACT_NAME || ' : ' || DREM.CONTACT_NO) as PRIMARY_CONTACT,
                                DREM.DELETED_FLAG,DREM.ACTIVE,DGRM.GROUP_EDESC GROUP_NAME,DGRM.GROUP_CODE,DREM.WHOLESELLER,DREM.SOURCE,DREM.CREATED_BY_NAME,DREM.created_date,DREM.LUPDATE_DATE,
                                (SELECT WM_CONCAT(DISTINCT DRM.ROUTE_NAME) FROM DIST_ROUTE_ENTITY DRE,DIST_ROUTE_MASTER DRM
                                        WHERE DRE.ROUTE_CODE = DRM.ROUTE_CODE AND DRE.ENTITY_CODE = DREM.RESELLER_CODE
                                        AND DRE.ENTITY_TYPE = 'R' AND DRM.ROUTE_TYPE='D' and DRM.DELETED_FLAG='N' AND DRE.COMPANY_CODE = '{userInfo.company_code}') ROUTE,
                                (SELECT WM_CONCAT(DISTINCT DLU.FULL_NAME) FROM DIST_ROUTE_ENTITY DRE,DIST_ROUTE_DETAIL DRD, DIST_LOGIN_USER DLU
                                        WHERE DRE.ROUTE_CODE = DRD.ROUTE_CODE AND DRD.EMP_CODE = DLU.SP_CODE AND DLU.ACTIVE = 'Y'
                                        AND DRE.COMPANY_CODE = DRD.COMPANY_CODE AND DRD.COMPANY_CODE = DLU.COMPANY_CODE
                                        AND DRE.ENTITY_CODE =DREM.RESELLER_CODE
                                        AND DRE.ENTITY_TYPE = 'R' AND DRE.COMPANY_CODE = '{userInfo.company_code}') SALES_PERSON,
                                (SELECT WM_CONCAT(DISTINCT SCS.CUSTOMER_EDESC) FROM DIST_RESELLER_ENTITY DRE,SA_CUSTOMER_SETUP SCS,DIST_DISTRIBUTOR_MASTER DM
                                        WHERE DRE.ENTITY_CODE = SCS.CUSTOMER_CODE AND DRE.RESELLER_CODE = DREM.RESELLER_CODE AND DRE.COMPANY_CODE = SCS.COMPANY_CODE
                                        AND DRE.ENTITY_TYPE = 'D' AND DRE.COMPANY_CODE = '{userInfo.company_code}' AND DM.COMPANY_CODE=SCS.COMPANY_CODE AND SCS.CUSTOMER_CODE=DM.DISTRIBUTOR_CODE AND DM.ACTIVE='Y' AND DM.DELETED_FLAG='N') DISTRIBUTER_DETAILS
                    FROM DIST_RESELLER_MASTER DREM
                    LEFT JOIN DIST_AREA_MASTER DAM ON DREM.AREA_CODE =  DAM.AREA_CODE AND DREM.COMPANY_CODE = DAM.COMPANY_CODE
                    LEFT JOIN DIST_OUTLET_TYPE DOUT ON DOUT.TYPE_ID = DREM.OUTLET_TYPE_ID AND DOUT.COMPANY_CODE = DREM.COMPANY_CODE
                    LEFT JOIN DIST_OUTLET_SUBTYPE DOST ON DOST.SUBTYPE_ID = DREM.OUTLET_SUBTYPE_ID AND DOST.COMPANY_CODE = DREM.COMPANY_CODE
                    LEFT JOIN DIST_GROUP_MASTER DGRM ON DREM.GROUPID = DGRM.GROUPID AND DREM.COMPANY_CODE = DGRM.COMPANY_CODE
                    WHERE DREM.COMPANY_CODE = '{userInfo.company_code}' AND  DREM.DELETED_FLAG='N' AND DREM.IS_CLOSED = 'N'";
                var data = _objectEntity.SqlQuery<ResellerListModel>(query).ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public List<ItemWiseMinMaxReport> GetItemsMinMaxList(ReportFiltersModel model, User userInfo)
        {
            var filter = "";
            if (model.ItemBrandFilter.Count > 0)
                filter = $" AND DLU.SP_CODE IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";

            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                filter = $" AND DLU.SP_CODE IN  ({userInfo.sp_codes})";
            }
            var Query = $@"SELECT MAX(TO_NUMBER(MAXANS)) AS MAXANS,MIN(TO_NUMBER(MINANS)) AS MINANS,
            ITEM_EDESC,COMP_ITEM,COL_NAME,COL_DATA_TYPE,AREA_NAME,NVL(SALES_RATE,0) SALES_RATE from (
            SELECT TO_NUMBER(DCQ.ANSWER) AS MAXANS,TO_NUMBER(DCQ.ANSWER) AS MINANS,
            IMS.ITEM_EDESC,DCIM.ITEM_EDESC COMP_ITEM,
            DCF.COL_NAME,DCF.COL_DATA_TYPE,
            DLU.FULL_NAME AREA_NAME, --,ENT.AREA_NAME, sales person name is binded to AREA_NAME field
            (SELECT SALES_RATE FROM IP_ITEM_RATE_APPLICAT_SETUP WHERE ITEM_CODE = DCQ.ITEM_CODE AND COMPANY_CODE = DCQ.COMPANY_CODE AND BRANCH_CODE = '{userInfo.branch_code}' AND ROWNUM = 1) SALES_RATE
                        FROM DIST_COMP_QA DCQ
                        INNER JOIN IP_ITEM_MASTER_SETUP IMS ON DCQ.ITEM_CODE = IMS.ITEM_CODE AND IMS.COMPANY_CODE = DCQ.COMPANY_CODE
                        INNER JOIN DIST_COMP_ITEM_MASTER DCIM ON DCQ.COMP_ITEM_CODE = DCIM.ITEM_ID AND DCQ.COMPANY_CODE = DCIM.COMPANY_CODE
                        INNER JOIN DIST_COMP_FIELDS DCF ON DCQ.QUESTION_ID = DCF.FIELD_ID AND DCQ.COMPANY_CODE = DCF.COMPANY_CODE
                        RIGHT JOIN DIST_LOGIN_USER DLU ON DLU.USERID = DCQ.CREATED_BY AND DLU.COMPANY_CODE = DCQ.COMPANY_CODE AND DLU.ACTIVE = 'Y'
                      WHERE DCQ.COMPANY_CODE = '{userInfo.company_code}' AND  DCF.COL_NAME ='Rate' {filter})
                      GROUP BY ITEM_EDESC,COMP_ITEM,COL_NAME,COL_DATA_TYPE,AREA_NAME,SALES_RATE";
            //if (model.BranchFilter.Count > 0)
            //{
            //    Query = Query + string.Format(@" AND DCQ.BRANCH_CODE IN ('{0}')", string.Join("','", model.BranchFilter).ToString());
            //}
            //if (model.DistAreaFilter.Count > 0)
            //{
            //    Query = Query + string.Format(@" AND DAM.AREA_CODE IN ('{0}')", string.Join("','", model.DistAreaFilter).ToString());
            //}
            //Query += $@" )GROUP BY ITEM_EDESC,COMP_ITEM,COL_NAME,COL_DATA_TYPE,AREA_NAME,SALES_RATE";
            var data = _objectEntity.SqlQuery<ItemWiseMinMaxReport>(Query).ToList();

            return data;
        }

        public DetailColumnModel GetDynamicFields(User userInfo)
        {
            var result = new DetailColumnModel();
            result.OutletTypes = _objectEntity.SqlQuery<ColumnModel>($"SELECT DISTINCT TYPE_EDESC HEADER,REPLACE(REGEXP_REPLACE(TYPE_EDESC,'[^[:alnum:]'' '']', NULL),' ',NULL) FIELDNAME FROM DIST_OUTLET_TYPE WHERE COMPANY_CODE='{userInfo.company_code}' AND DELETED_FLAG = 'N'").ToList();
            result.OutletSubTypes = _objectEntity.SqlQuery<ColumnModel>($"SELECT DISTINCT SUBTYPE_EDESC HEADER,REPLACE(REGEXP_REPLACE(SUBTYPE_EDESC,'[^[:alnum:]'' '']', NULL),' ',NULL) FIELDNAME FROM DIST_OUTLET_SUBTYPE WHERE COMPANY_CODE='{userInfo.company_code}' AND DELETED_FLAG = 'N'").ToList();
            result.Brands = _objectEntity.SqlQuery<ColumnModel>($"SELECT DISTINCT BRAND_NAME HEADER,REPLACE(REGEXP_REPLACE(BRAND_NAME,'[^[:alnum:]'' '']', NULL),' ',NULL) FIELDNAME FROM IP_ITEM_SPEC_SETUP WHERE BRAND_NAME IS NOT NULL AND COMPANY_CODE='{userInfo.company_code}' AND DELETED_FLAG = 'N'").ToList();
            return result;
        }

        public List<CustomerSales> GetCustomerSales()
        {
            throw new NotImplementedException();
        }

        public List<MobileLogModel> GetDeviceLog(filterOption model, User userInfo)
        {
            var filter = "";
            if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
            {
                filter = $" AND DUL.SP_CODE IN  ({userInfo.sp_codes})";
            }
            var query = $@"SELECT DUL.SP_CODE,DLU.FULL_NAME,DUL.SWITCH_STATUS,DUL.BATTERY_PERCENT,TO_CHAR(DUL.CREATED_DATE,'HH:MI:SS A.M.') LOG_TIME,TO_CHAR(DUL.CREATED_DATE,'DD-MON-RRRR') LOG_DATE
                        FROM DIST_USER_DEVICE_LOG DUL
                        INNER JOIN DIST_LOGIN_USER DLU ON DUL.SP_CODE = DLU.SP_CODE AND DUL.COMPANY_CODE = DLU.COMPANY_CODE
                        WHERE DUL.COMPANY_CODE = '{userInfo.company_code}' {filter}
                        AND TRUNC(DUL.CREATED_DATE) BETWEEN to_date('{model.ReportFilters.FromDate}','RRRR-MON-DD') AND to_date('{model.ReportFilters.ToDate}','RRRR-MON-DD')";
            var data = _objectEntity.SqlQuery<MobileLogModel>(query).ToList();
            return data;
        }

        public List<SummaryReport> GetAllSummaryReport(filterOption model, User userInfo)
        {
            var company_code = userInfo.company_code;
            var spCode = string.Empty;
            if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                spCode = $" AND DLU.SP_CODE IN  ({userInfo.sp_codes})";

            var query = $@" SELECT USERID,FULL_NAME,GROUP_NAME,TOTAL_OUTLET,TOTAL_VISIT,TOTAL_COLLECTION,SSDAMOUNT+SDRAMOUNT TOTAL_SALES,SSDEFFECTIVECALL+STREFFECTIVECALL AS EFFECTIVECALLS FROM 
                                (SELECT DLU.USERID, DLU.FULL_NAME ,DGM.GROUP_EDESC GROUP_NAME,
                                (SELECT COUNT(RESELLER_CODE) FROM  DIST_RESELLER_MASTER  WHERE CREATED_BY=DLU.USERID  AND COMPANY_CODE=DLU.COMPANY_CODE AND TRUNC(CREATED_DATE) BETWEEN to_date('{model.ReportFilters.FromDate}','RRRR-MON-DD') AND to_date('{model.ReportFilters.ToDate}','RRRR-MON-DD') ) TOTAL_OUTLET,
                                (SELECT COUNT(SP_CODE) FROM DIST_LOCATION_TRACK WHERE SP_CODE = DLU.SP_CODE AND IS_VISITED='Y' AND TRUNC(UPDATE_DATE) BETWEEN to_date('{model.ReportFilters.FromDate}','RRRR-MON-DD') AND to_date('{model.ReportFilters.ToDate}','RRRR-MON-DD') AND COMPANY_CODE=DLU.COMPANY_CODE ) TOTAL_VISIT,
                                NVL((SELECT SUM(AMOUNT) FROM DIST_COLLECTION WHERE SP_CODE=DLU.SP_CODE  AND TRUNC(CREATED_DATE) BETWEEN to_date('{model.ReportFilters.FromDate}','RRRR-MON-DD') AND to_date('{model.ReportFilters.ToDate}','RRRR-MON-DD') AND COMPANY_CODE=DLU.COMPANY_CODE),0)TOTAL_COLLECTION,
                                 NVL((SELECT SUM(TOTAL_PRICE)AMOUNT FROM DIST_IP_SSD_PURCHASE_ORDER WHERE COMPANY_CODE='{company_code}'  AND TRUNC(ORDER_DATE) BETWEEN to_date('{model.ReportFilters.FromDate}','RRRR-MON-DD') AND to_date('{model.ReportFilters.ToDate}','RRRR-MON-DD') AND CREATED_BY=DLU.USERID ),0) SSDAMOUNT,
                                  NVL((SELECT SUM(TOTAL_PRICE)AMOUNT FROM DIST_IP_SSR_PURCHASE_ORDER WHERE COMPANY_CODE='{company_code}' AND TRUNC(ORDER_DATE) BETWEEN to_date('{model.ReportFilters.FromDate}','RRRR-MON-DD') AND to_date('{model.ReportFilters.ToDate}','RRRR-MON-DD') AND CREATED_BY=DLU.USERID ),0) SDRAMOUNT,
                                 (SELECT COUNT(DISTINCT ORDER_NO)QTY FROM DIST_IP_SSD_PURCHASE_ORDER WHERE COMPANY_CODE='{company_code}' AND TRUNC(ORDER_DATE) BETWEEN to_date('{model.ReportFilters.FromDate}','RRRR-MON-DD') AND to_date('{model.ReportFilters.ToDate}','RRRR-MON-DD') AND CREATED_BY=DLU.USERID ) SSDEFFECTIVECALL,
                                  (SELECT COUNT(DISTINCT ORDER_NO)QTY FROM DIST_IP_SSR_PURCHASE_ORDER WHERE COMPANY_CODE='{company_code}' AND TRUNC(ORDER_DATE) BETWEEN to_date('{model.ReportFilters.FromDate}','RRRR-MON-DD') AND to_date('{model.ReportFilters.ToDate}','RRRR-MON-DD') AND CREATED_BY=DLU.USERID ) STREFFECTIVECALL
                                FROM DIST_LOGIN_USER DLU,DIST_GROUP_MASTER DGM
                                 WHERE DLU.GROUPID=DGM.GROUPID AND DLU.COMPANY_CODE=DGM.COMPANY_CODE AND DLU.COMPANY_CODE='{company_code}' {spCode} )";
            var data = _objectEntity.SqlQuery<SummaryReport>(query).ToList();
            return data;
        }
        public List<DistResellerStockModel> GetDistResellerStockList(filterOption model, User userInfo, string Distributor = "", string Reseller = "")
        {
            try
            {
                var spCode = string.Empty;
                if (model.ReportFilters.ItemBrandFilter.Count > 0)
                    spCode = $@" AND SP_CODE IN ('{string.Join("','", model.ReportFilters.ItemBrandFilter)}')";
                else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                    spCode = $@" AND SP_CODE IN ({userInfo.sp_codes})";

                var TypeFilter = string.Empty;
                if (!string.IsNullOrWhiteSpace(Reseller) && !string.IsNullOrWhiteSpace(Distributor))
                    TypeFilter = $" AND CUSTOMER_CODE IN ('{Reseller}','{Distributor}')";
                else if (!string.IsNullOrWhiteSpace(Reseller))
                    TypeFilter = $" AND CUSTOMER_CODE='{Reseller}'";
                else if (!string.IsNullOrWhiteSpace(Distributor))
                    TypeFilter = $" AND CUSTOMER_CODE='{Distributor}'";


                var query = $@" SELECT * FROM (SELECT CS.CUSTOMER_CODE ,CS.CUSTOMER_EDESC,DDS.MU_CODE, DDS.ITEM_CODE, IMS.ITEM_EDESC,DDS.CURRENT_STOCK,DDS.PURCHASE_QTY,DDS.SP_CODE,NVL(HES.EMPLOYEE_EDESC,'Self') EMPLOYEE_EDESC,DDS.CREATED_DATE,DDS.COMPANY_CODE,'DISTRIBUTOR' TYPE
                                    FROM DIST_DISTRIBUTOR_STOCK DDS
                                    INNER JOIN SA_CUSTOMER_SETUP CS ON DDS.DISTRIBUTOR_CODE = CS.CUSTOMER_CODE AND DDS.COMPANY_CODE = CS.COMPANY_CODE
                                    INNER JOIN IP_ITEM_MASTER_SETUP IMS ON DDS.ITEM_CODE = IMS.ITEM_CODE AND DDS.COMPANY_CODE = IMS.COMPANY_CODE
                                    LEFT JOIN HR_EMPLOYEE_SETUP HES ON DDS.SP_CODE = HES.EMPLOYEE_CODE AND DDS.COMPANY_CODE= HES.COMPANY_CODE
                                    WHERE DDS.DELETED_FLAG='N' AND DDS.CURRENT_STOCK <>0
                                     UNION ALL
                                SELECT CS.RESELLER_CODE CUSTOMER_CODE ,CS.RESELLER_NAME CUSTOMER_EDESC, DRS.MU_CODE,DRS.ITEM_CODE,IMS.ITEM_EDESC,DRS.CURRENT_STOCK,DRS.PURCHASE_QTY,DRS.SP_CODE,NVL(HES.EMPLOYEE_EDESC,'Self') EMPLOYEE_EDESC,DRS.CREATED_DATE,DRS.COMPANY_CODE ,'OUTLET' TYPE
                                FROM DIST_RESELLER_STOCK DRS
                                    INNER JOIN DIST_RESELLER_MASTER CS ON DRS.RESELLER_CODE = CS.RESELLER_CODE AND DRS.COMPANY_CODE = CS.COMPANY_CODE
                                    INNER JOIN IP_ITEM_MASTER_SETUP IMS ON DRS.ITEM_CODE = IMS.ITEM_CODE AND DRS.COMPANY_CODE = IMS.COMPANY_CODE
                                    LEFT JOIN HR_EMPLOYEE_SETUP HES ON DRS.SP_CODE = HES.EMPLOYEE_CODE AND DRS.COMPANY_CODE= HES.COMPANY_CODE
                                    WHERE DRS.DELETED_FLAG='N' AND DRS.CURRENT_STOCK <>0) T
                                    WHERE COMPANY_CODE='{userInfo.company_code}' {spCode} {TypeFilter}
                                    AND TRUNC(CREATED_DATE) BETWEEN to_date('{model.ReportFilters.FromDate}','RRRR-MON-DD') AND to_date('{model.ReportFilters.ToDate}','RRRR-MON-DD')";
                var result = this._objectEntity.SqlQuery<DistResellerStockModel>(query).ToList();
                if (result.Count > 0)
                {
                    if (model.ReportFilters.SalesPersonFilter.Count > 0)
                        result = result.Where(p => model.ReportFilters.SalesPersonFilter.Contains(p.SP_CODE)).ToList();
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<DistResellerStockConversionModel> GetDistResellerBrandItemStockList(filterOption model, User userInfo, string Distributor = "", string Reseller = "")
        {
            try
            {
                var spCode = string.Empty;
                if (model.ReportFilters.ItemBrandFilter.Count > 0)
                    spCode = $@" AND DRS.SP_CODE IN ('{string.Join("','", model.ReportFilters.ItemBrandFilter)}')";
                else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                    spCode = $@" AND DRS.SP_CODE IN ({userInfo.sp_codes})";


                var TypeFilter = string.Empty;
                if (!string.IsNullOrWhiteSpace(Reseller))
                    TypeFilter = $" AND DRS.CUSTOMER_CODE='{Reseller}'";

                var query = $@"SELECT CUSTOMER_CODE,CUSTOMER_EDESC,MU_CODE,ITEM_CODE,ITEM_EDESC,CONVERSION_STOCK CURRENT_STOCK,CREATED_DATE, SP_CODE,GROUP_EDESC,BRAND_NAME,EMPLOYEE_EDESC,COMPANY_CODE,TYPE_EDESC,SUBTYPE_EDESC FROM
                                   (SELECT CS.RESELLER_CODE CUSTOMER_CODE,CS.RESELLER_NAME CUSTOMER_EDESC, DRS.MU_CODE,DRS.ITEM_CODE,IMS.ITEM_EDESC,DRS.CURRENT_STOCK,DRS.PURCHASE_QTY,DRS.SP_CODE,GM.GROUP_EDESC ,ISS.BRAND_NAME,
                                     NVL(HES.EMPLOYEE_EDESC,'Self') EMPLOYEE_EDESC,DRS.CREATED_DATE,DRS.COMPANY_CODE ,OT. TYPE_EDESC,OST. SUBTYPE_EDESC,
    DECODE(DRS.MU_CODE,IU.MU_CODE, ROUND(DRS.CURRENT_STOCK/IU.CONVERSION_FACTOR,2), DRS.CURRENT_STOCK ) CONVERSION_STOCK
                                FROM DIST_RESELLER_STOCK DRS
                                    INNER JOIN DIST_RESELLER_MASTER CS ON DRS.RESELLER_CODE = CS.RESELLER_CODE AND DRS.COMPANY_CODE = CS.COMPANY_CODE
                                    INNER JOIN DIST_OUTLET_TYPE OT ON CS.OUTLET_TYPE_ID = OT.TYPE_ID AND CS.COMPANY_CODE = OT.COMPANY_CODE
                                    INNER JOIN DIST_OUTLET_SUBTYPE OST ON CS.OUTLET_SUBTYPE_ID = OST.SUBTYPE_ID AND CS.COMPANY_CODE = OST.COMPANY_CODE
                                    INNER JOIN DIST_GROUP_MASTER GM ON CS.GROUPID = GM.GROUPID AND CS.COMPANY_CODE = GM.COMPANY_CODE
                                    INNER JOIN IP_ITEM_MASTER_SETUP IMS ON DRS.ITEM_CODE = IMS.ITEM_CODE AND DRS.COMPANY_CODE = IMS.COMPANY_CODE
                                    INNER JOIN IP_ITEM_SPEC_SETUP ISS ON IMS.ITEM_CODE = ISS.ITEM_CODE AND IMS.COMPANY_CODE = ISS.COMPANY_CODE
                                     INNER JOIN IP_ITEM_UNIT_SETUP IU ON DRS.ITEM_CODE=IU.ITEM_CODE AND DRS.COMPANY_CODE=IU.COMPANY_CODE
                                    LEFT JOIN HR_EMPLOYEE_SETUP HES ON DRS.SP_CODE = HES.EMPLOYEE_CODE AND DRS.COMPANY_CODE= HES.COMPANY_CODE
                                    WHERE DRS.DELETED_FLAG='N' AND DRS.CURRENT_STOCK <>0  AND DRS.CREATED_DATE = (SELECT MAX(CREATED_DATE) FROM DIST_RESELLER_STOCK 
WHERE  ITEM_CODE = DRS.ITEM_CODE  
AND CURRENT_STOCK > 0 
AND  RESELLER_CODE = DRS.RESELLER_CODE) AND DRS.COMPANY_CODE='{userInfo.company_code}' {spCode} {TypeFilter}
--AND TRUNC(DRS.CREATED_DATE) BETWEEN to_date('{model.ReportFilters.FromDate}','RRRR-MON-DD') AND to_date('{model.ReportFilters.ToDate}','RRRR-MON-DD')
) 
                                    --GROUP BY CUSTOMER_CODE,CUSTOMER_EDESC,MU_CODE,ITEM_CODE,ITEM_EDESC, SP_CODE,GROUP_EDESC,BRAND_NAME,COMPANY_CODE,CREATED_DATE,EMPLOYEE_EDESC,TYPE_EDESC,SUBTYPE_EDESC";
                var result = this._objectEntity.SqlQuery<DistResellerStockConversionModel>(query).ToList();
                if (result.Count > 0)
                {
                    if (model.ReportFilters.SalesPersonFilter.Count > 0)
                        result = result.Where(p => model.ReportFilters.SalesPersonFilter.Contains(p.SP_CODE)).ToList();
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<DistdistributorStockModel> GetDistDistributorBrandItemStockList(filterOption model, User userInfo, string Distributor = "", string Reseller = "")
        {
            try
            {
                //var spCode = string.Empty;
                //if (model.ReportFilters.ItemBrandFilter.Count > 0)
                //    spCode = $@" AND DRS.SP_CODE IN ('{string.Join("','", model.ReportFilters.ItemBrandFilter)}')";
                //else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                //    spCode = $@" AND DRS.SP_CODE IN ({userInfo.sp_codes})";

                var TypeFilter = string.Empty;
                if (!string.IsNullOrWhiteSpace(Reseller))
                    TypeFilter = $" AND DRS.CUSTOMER_CODE='{Reseller}'";

                var query = $@"SELECT CUSTOMER_CODE,CUSTOMER_EDESC,MU_CODE,ITEM_CODE,ITEM_EDESC,CONVERSION_STOCK CURRENT_STOCK,CREATED_DATE, SP_CODE,GROUP_EDESC,
                                    BRAND_NAME,EMPLOYEE_EDESC,COMPANY_CODE FROM
                                    (SELECT DDM.DISTRIBUTOR_CODE CUSTOMER_CODE ,SCS.CUSTOMER_EDESC,DDS.MU_CODE,DDS.ITEM_CODE,IMS.ITEM_EDESC,DDS.CURRENT_STOCK,
                                    DDS.PURCHASE_QTY,DDS.SP_CODE,GM.GROUP_EDESC,ISS.BRAND_NAME,NVL(HES.EMPLOYEE_EDESC,'Self') EMPLOYEE_EDESC,DDS.CREATED_DATE,DDS.COMPANY_CODE ,
                                        DECODE(DDS.MU_CODE,IU.MU_CODE, ROUND(DDS.CURRENT_STOCK/IU.CONVERSION_FACTOR,2), DDS.CURRENT_STOCK ) CONVERSION_STOCK
                                     FROM DIST_DISTRIBUTOR_STOCK DDS 
                                    INNER JOIN DIST_DISTRIBUTOR_MASTER DDM ON DDS.DISTRIBUTOR_CODE=DDM.DISTRIBUTOR_CODE AND DDM.COMPANY_CODE=DDS.COMPANY_CODE
                                    INNER JOIN SA_CUSTOMER_SETUP SCS ON SCS.CUSTOMER_CODE=DDM.DISTRIBUTOR_CODE AND SCS.COMPANY_CODE=DDM.COMPANY_CODE
                                    INNER JOIN DIST_GROUP_MASTER GM ON DDM.GROUPID = GM.GROUPID AND DDM.COMPANY_CODE = GM.COMPANY_CODE
                                    INNER JOIN IP_ITEM_MASTER_SETUP IMS ON DDS.ITEM_CODE = IMS.ITEM_CODE AND DDS.COMPANY_CODE = IMS.COMPANY_CODE
                                    INNER JOIN IP_ITEM_SPEC_SETUP ISS ON IMS.ITEM_CODE = ISS.ITEM_CODE AND IMS.COMPANY_CODE = ISS.COMPANY_CODE
                                     INNER JOIN IP_ITEM_UNIT_SETUP IU ON DDS.ITEM_CODE=IU.ITEM_CODE AND DDS.COMPANY_CODE=IU.COMPANY_CODE
                                   LEFT JOIN HR_EMPLOYEE_SETUP HES ON DDS.SP_CODE = HES.EMPLOYEE_CODE AND DDS.COMPANY_CODE= HES.COMPANY_CODE
                                      WHERE DDS.DELETED_FLAG='N' AND DDS.CURRENT_STOCK <>0 
                                      AND DDS.CREATED_DATE = (SELECT MAX(CREATED_DATE) FROM DIST_DISTRIBUTOR_STOCK 
WHERE  ITEM_CODE = DDS.ITEM_CODE  
AND CURRENT_STOCK > 0 
AND  DISTRIBUTOR_CODE = DDS.DISTRIBUTOR_CODE)
                                     AND DDS.COMPANY_CODE='{userInfo.company_code}'  
                                   AND TRUNC(DDS.CREATED_DATE) <= to_date('{model.ReportFilters.ToDate}','RRRR-MON-DD')
                                    )";
                var result = this._objectEntity.SqlQuery<DistdistributorStockModel>(query).ToList();
                //if (result.Count > 0)
                //{
                //    if (model.ReportFilters.SalesPersonFilter.Count > 0)
                //        result = result.Where(p => model.ReportFilters.SalesPersonFilter.Contains(p.SP_CODE)).ToList();
                //}
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<AttendanceReportCalendarModel> GetAttendanceForCalendar(filterOption model)
        {
            var query = $@"SELECT TO_CHAR(ATN.ATTENDANCE_DT, 'YYYY-MM-DD') MONTH_DAY,
                  TO_CHAR(ATN.EMPLOYEE_ID)EMPLOYEE_ID,
                  TO_CHAR(ATN.ATTENDANCE_DT, 'YYYY-MM-DD') ATTENDANCE_DT,
                  TO_CHAR(ATN.IN_TIME, 'HH24:MI') IN_TIME,
                  TO_CHAR(ATN.OUT_TIME, 'HH24:MI') OUT_TIME,
                  (
                  CASE
                    WHEN ATN.OVERALL_STATUS = 'DO'
                    THEN 'Day Off'
                    WHEN ATN.OVERALL_STATUS ='HD'
                    THEN 'On Holiday ('
                      ||HMS.HOLIDAY_ENAME
                     ||')'
                    WHEN ATN.OVERALL_STATUS ='LV'
                    THEN 'On Leave ('
                      ||LMS.LEAVE_ENAME
                      || ')'
                    WHEN ATN.OVERALL_STATUS ='TV'
                    THEN 'On Travel ('
                      ||ETR.DESTINATION
                      ||')'
                    WHEN ATN.OVERALL_STATUS ='TN'
                    THEN 'On Training ('
                      ||(CASE WHEN ATN.TRAINING_TYPE = 'A' THEN TMS.TRAINING_NAME ELSE ETN.TITLE END)
                      ||')'
                    WHEN ATN.OVERALL_STATUS ='WD'
                    THEN 'Work On Dayoff'
                    WHEN ATN.OVERALL_STATUS ='WH'
                    THEN 'Work on Holiday ('
                      ||HMS.HOLIDAY_ENAME
                      ||')'
                    WHEN ATN.OVERALL_STATUS ='LP'
                    THEN 'On Partial Leave ('
                      ||LMS.LEAVE_ENAME
                      ||') '
                      ||LATE_STATUS_DESC(ATN.LATE_STATUS) 
                    WHEN ATN.OVERALL_STATUS ='VP'
                    THEN 'Work on Travel ('
                      ||ETR.DESTINATION
                      ||')'
                      ||LATE_STATUS_DESC(ATN.LATE_STATUS)
                    WHEN ATN.OVERALL_STATUS ='TP'
                    THEN 'Present ('
                      || TMS.TRAINING_NAME
                      ||')'
                      ||LATE_STATUS_DESC(ATN.LATE_STATUS)
                    WHEN ATN.OVERALL_STATUS ='PR'
                    THEN 'Present '
                      ||LATE_STATUS_DESC(ATN.LATE_STATUS)
                    WHEN ATN.OVERALL_STATUS ='AB'
                    THEN 'Absent'
                    WHEN ATN.OVERALL_STATUS ='BA'
                    THEN 'Present(Late In and Early Out)'
                    WHEN ATN.OVERALL_STATUS ='LA'
                    THEN 'Present(Late Penalty)'
                  END)AS ATTENDANCE_STATUS,
                  ATN.OVERALL_STATUS
                FROM HRIS_ATTENDANCE_DETAIL ATN
                LEFT JOIN HRIS_LEAVE_MASTER_SETUP LMS
                ON LMS.LEAVE_ID = ATN.LEAVE_ID
                LEFT JOIN HRIS_HOLIDAY_MASTER_SETUP HMS
                ON HMS.HOLIDAY_ID = ATN.HOLIDAY_ID
                LEFT JOIN HRIS_TRAINING_MASTER_SETUP TMS
                ON (TMS.TRAINING_ID = ATN.TRAINING_ID AND ATN.TRAINING_TYPE='A')
                LEFT JOIN HRIS_EMPLOYEE_TRAINING_REQUEST ETN
                ON (ETN.REQUEST_ID=ATN.TRAINING_ID AND ATN.TRAINING_TYPE ='R')
                LEFT JOIN HRIS_EMPLOYEE_TRAVEL_REQUEST ETR
                ON ETR.TRAVEL_ID = ATN.TRAVEL_ID
                WHERE 1          = 1
                AND (ATN.ATTENDANCE_DT BETWEEN TO_DATE('{model.ReportFilters.FromDate}','YYYY-MM-DD') AND TO_DATE('{model.ReportFilters.ToDate}','YYYY-MM-DD') )
                AND ATN.EMPLOYEE_ID = '{string.Join("','", model.ReportFilters.EmployeeFilter)}'
                ORDER BY ATN.ATTENDANCE_DT ASC";
            var data = _objectEntity.SqlQuery<AttendanceReportCalendarModel>(query).ToList();
            return data;
        }
        public List<SURVEY_COLUMN_MODEL> GetWebSurveyReportQUE(User userInfo)
        {
            try
            {
                var query = $@"SELECT DISTINCT QUESTION TITLE, QUESTION FIELD FROM DIST_WEB_QUE_ANS WHERE CREATED_BY ='{userInfo.User_id}' AND COMPANY_CODE='{userInfo.company_code}' ORDER BY QUESTION ASC";
                var result = _objectEntity.SqlQuery<SURVEY_COLUMN_MODEL>(query).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<SURVEY_COLUMN_MODEL> GetSurveyReportCol()
        {
            try
            {
                var query = $@"SELECT DISTINCT TABLE_TITLE TITLE, TABLE_TITLE FIELD FROM SURVEY_REPORT ORDER BY TABLE_TITLE ASC ";
                var result = _objectEntity.SqlQuery<SURVEY_COLUMN_MODEL>(query).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<SURVEY_REPORT_MODEL> GetSurveyReport(filterOption model)
        {
            try
            {
                var query = $@"SELECT SURVEY_EDESC
                                  ,DIST_OUTLET_NAME
                                  ,FULL_NAME
                                  ,CREATED_DATE
                                  ,max(case when table_title = 'Do you have any idea about Hulas Chewda ?' THEN Actual_Answer END) AS Q1
                                  ,max(case when table_title = 'What are the Chewda Product available in your store?' THEN Actual_Answer END) AS Q2
                                  ,max(case when table_title = 'Which Chewda has the highest demand?' THEN Actual_Answer END) AS Q3
                                  ,max(case when table_title = 'What is the reason for highest sale?' THEN Actual_Answer END) AS Q4
                                  ,max(case when table_title = 'If Hulas is Available, how is customer response?' THEN Actual_Answer END) AS Q5
                                  ,max(case when table_title = 'What does the consumer say about the taste of Hulas Chewda?' THEN Actual_Answer END) AS Q6
                                  ,max(case when table_title = 'What does the consumer say about the rate of Hulas Chewda?' THEN Actual_Answer END) AS Q7
                                  ,max(case when table_title = 'Do you have others products of Hulas? If Yes, Then which?' THEN Actual_Answer END) AS Q8
                                  ,max(case when table_title = 'Have you seen Hulas Chewda Advertisement?' THEN Actual_Answer END) AS Q9
                                  ,max(case when table_title = 'If Yes, where have you seen?' THEN Actual_Answer END) AS Q10
                                  ,max(case when table_title = 'If any suggestion, would you like to give for Hulas Chewda?' THEN Actual_Answer END) AS Q11
                                  ,max(case when table_title = 'Any suggestion for other products of Hulas Food?' THEN Actual_Answer END) AS Q12
                                  ,max(case when table_title = 'Do people like Home Made Chewda?' THEN Actual_Answer END) AS Q13
                            FROM SURVEY_REPORT WHERE SURVEY_CODE <> '3'
                            GROUP BY SURVEY_EDESC
                                      ,DIST_OUTLET_NAME
                                      ,FULL_NAME
                                      ,CREATED_DATE";
                var result = _objectEntity.SqlQuery<SURVEY_REPORT_MODEL>(query).ToList();

                foreach (var item in result)
                {
                    if (string.IsNullOrEmpty(item.Q2))
                        item.Q2 = "";
                    item.QN2_LNT = item.Q2.ToUpper().Contains("LNT") ? 1 : 0;
                    item.QN2_TIME_PASS = item.Q2.ToUpper().Contains("TIME PASS") ? 1 : 0;
                    item.QN2_SANDESH = item.Q2.ToUpper().Contains("SANDESH") ? 1 : 0;
                    item.QN2_UPAKAR = item.Q2.ToUpper().Contains("UPAKAR") ? 1 : 0;
                    item.QN2_ANY_OTHER = item.Q2.ToUpper().Contains("ANY OTHER") ? 1 : 0;

                    if (string.IsNullOrEmpty(item.Q4))
                        item.Q4 = "";
                    item.TASTE = item.Q4.ToUpper().Contains("TASTE") ? 1 : 0;
                    item.QUALITY = item.Q4.ToUpper().Contains("QUALITY") ? 1 : 0;
                    item.LOW_PRICE = item.Q4.ToUpper().Contains("LOW PRICE") ? 1 : 0;

                    if (string.IsNullOrEmpty(item.Q5))
                        item.Q5 = "";
                    item.GOOD = item.Q5.ToUpper().Contains("GOOD") ? 1 : 0;
                    item.AVERAGE = item.Q5.ToUpper().Contains("AVERAGE") ? 1 : 0;
                    item.BAD = item.Q5.ToUpper().Contains("BAD") ? 1 : 0;

                    if (string.IsNullOrEmpty(item.Q6))
                        item.Q6 = "";
                    item.TASTY = item.Q6.ToUpper().Contains("TASTY") ? 1 : 0;
                    item.CRUNCHY = item.Q6.ToUpper().Contains("CRUNCHY") ? 1 : 0;

                    if (string.IsNullOrEmpty(item.Q8))
                        item.Q8 = "";
                    item.AATA = item.Q8.ToUpper().Contains("AATA") ? 1 : 0;
                    item.RICE = item.Q8.ToUpper().Contains("RICE") ? 1 : 0;
                    item.DAAL = item.Q8.ToUpper().Contains("DAAL") ? 1 : 0;
                    item.SUJI = item.Q8.ToUpper().Contains("SUJI") ? 1 : 0;

                    if (string.IsNullOrEmpty(item.Q10))
                        item.Q10 = "";
                    item.LNT = item.Q10.ToUpper().Contains("FACEBOOK") ? 1 : 0;
                    item.TIME_PASS = item.Q10.ToUpper().Contains("NEWSPAPER") ? 1 : 0;
                    item.SANDESH = item.Q10.ToUpper().Contains("SHOP") ? 1 : 0;
                    item.UPAKAR = item.Q10.ToUpper().Contains("WORD OF MOUTH") ? 1 : 0;

                }

                //var qList = string.Empty;
                //var q_query = $@"SELECT DISTINCT TABLE_TITLE FROM SURVEY_REPORT";
                //var queList = _objectEntity.SqlQuery<string>(q_query).ToList();
                //if (queList.Count > 0)
                //    qList = string.Join("','", queList).ToString();
                //var actualQuery = $@" SELECT SURVEY_EDESC
                //                      ,DIST_OUTLET_NAME
                //                      ,FULL_NAME
                //                      ,CREATED_DATE
                //                      ,'{qList}'
                //                      FROM SURVEY_REPORT
                //                PIVOT (
                //                  MIN(ACTUAL_ANSWER) FOR TABLE_TITLE  IN ('{qList}')
                //                );";
                //var result = _objectEntity.SqlQuery<SURVEY_REPORT_MODEL>(actualQuery).ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<SURVEY_REPORT_AATA_MODEL> GetSurveyReportAata(filterOption model)
        {
            try
            {
                var query = $@"SELECT SURVEY_EDESC
                                  ,DIST_OUTLET_NAME
                                  ,FULL_NAME
                                  ,CREATED_DATE
                                  ,max(case when table_title = 'Which Atta Packaging you like?' THEN Actual_Answer END) AS Q1
                                  ,max(case when table_title = 'What are the Atta Products available in your store?' THEN Actual_Answer END) AS Q2
                                  ,max(case when table_title = 'Which Atta has the highest demand?' THEN Actual_Answer END) AS Q3
                                  ,max(case when table_title = 'Which Atta you will recommend?' THEN Actual_Answer END) AS Q4
                                  ,max(case when table_title = 'What is the reason for Atta''s highest sale?' THEN Actual_Answer END) AS Q5
                                  ,max(case when table_title = 'Do you Have Hulas Atta?' THEN Actual_Answer END) AS Q6
                                  ,max(case when table_title = 'Which Atta is more Whiteness?' THEN Actual_Answer END) AS Q7
                                  ,max(case when table_title = 'Which Atta is more Softness?' THEN Actual_Answer END) AS Q8
                                  ,max(case when table_title = 'Which Atta is better in Taste?' THEN Actual_Answer END) AS Q9
                                  FROM SURVEY_REPORT WHERE SURVEY_CODE = '3'
                            GROUP BY SURVEY_EDESC
                                      ,DIST_OUTLET_NAME
                                      ,FULL_NAME
                                      ,CREATED_DATE";
                var result = _objectEntity.SqlQuery<SURVEY_REPORT_AATA_MODEL>(query).ToList();

                foreach (var item in result)
                {
                    if (string.IsNullOrEmpty(item.Q1))
                        item.Q1 = "";
                    item.Q1_HULAS = item.Q1.ToUpper().Contains("HULAS") ? 1 : 0;
                    item.Q1_GYAN = item.Q1.ToUpper().Contains("GYAN") ? 1 : 0;
                    item.Q1_KALP = item.Q1.ToUpper().Contains("KALP") ? 1 : 0;
                    item.Q1_FORTUNE = item.Q1.ToUpper().Contains("FORTUNE") ? 1 : 0;
                    item.Q1_OTHERS = item.Q1.ToUpper().Contains("OTHER") ? 1 : 0;

                    if (string.IsNullOrEmpty(item.Q2))
                        item.Q2 = "";
                    item.Q2_HULAS = item.Q2.ToUpper().Contains("HULAS") ? 1 : 0;
                    item.Q2_GYAN = item.Q2.ToUpper().Contains("GYAN") ? 1 : 0;
                    item.Q2_KALP = item.Q2.ToUpper().Contains("KALP") ? 1 : 0;
                    item.Q2_FORTUNE = item.Q2.ToUpper().Contains("FORTUNE") ? 1 : 0;
                    item.Q2_OTHERS = item.Q2.ToUpper().Contains("OTHER") ? 1 : 0;

                    if (string.IsNullOrEmpty(item.Q3))
                        item.Q3 = "";
                    item.Q3_HULAS = item.Q3.ToUpper().Contains("HULAS") ? 1 : 0;
                    item.Q3_GYAN = item.Q3.ToUpper().Contains("GYAN") ? 1 : 0;
                    item.Q3_KALP = item.Q3.ToUpper().Contains("KALP") ? 1 : 0;
                    item.Q3_FORTUNE = item.Q3.ToUpper().Contains("FORTUNE") ? 1 : 0;
                    item.Q3_OTHERS = item.Q3.ToUpper().Contains("OTHER") ? 1 : 0;

                    if (string.IsNullOrEmpty(item.Q4))
                        item.Q4 = "";
                    item.Q4_HULAS = item.Q4.ToUpper().Contains("HULAS") ? 1 : 0;
                    item.Q4_GYAN = item.Q4.ToUpper().Contains("GYAN") ? 1 : 0;
                    item.Q4_KALP = item.Q4.ToUpper().Contains("KALP") ? 1 : 0;
                    item.Q4_FORTUNE = item.Q4.ToUpper().Contains("FORTUNE") ? 1 : 0;
                    item.Q4_OTHERS = item.Q4.ToUpper().Contains("OTHER") ? 1 : 0;

                    if (string.IsNullOrEmpty(item.Q7))
                        item.Q7 = "";
                    item.Q7_HULAS = item.Q7.ToUpper().Contains("HULAS") ? 1 : 0;
                    item.Q7_GYAN = item.Q7.ToUpper().Contains("GYAN") ? 1 : 0;
                    item.Q7_KALP = item.Q7.ToUpper().Contains("KALP") ? 1 : 0;
                    item.Q7_FORTUNE = item.Q7.ToUpper().Contains("FORTUNE") ? 1 : 0;
                    item.Q7_OTHERS = item.Q7.ToUpper().Contains("OTHER") ? 1 : 0;

                    if (string.IsNullOrEmpty(item.Q8))
                        item.Q8 = "";
                    item.Q8_HULAS = item.Q8.ToUpper().Contains("HULAS") ? 1 : 0;
                    item.Q8_GYAN = item.Q8.ToUpper().Contains("GYAN") ? 1 : 0;
                    item.Q8_KALP = item.Q8.ToUpper().Contains("KALP") ? 1 : 0;
                    item.Q8_FORTUNE = item.Q8.ToUpper().Contains("FORTUNE") ? 1 : 0;
                    item.Q8_OTHERS = item.Q8.ToUpper().Contains("OTHER") ? 1 : 0;

                    if (string.IsNullOrEmpty(item.Q9))
                        item.Q9 = "";
                    item.Q9_HULAS = item.Q9.ToUpper().Contains("HULAS") ? 1 : 0;
                    item.Q9_GYAN = item.Q9.ToUpper().Contains("GYAN") ? 1 : 0;
                    item.Q9_KALP = item.Q9.ToUpper().Contains("KALP") ? 1 : 0;
                    item.Q9_FORTUNE = item.Q9.ToUpper().Contains("FORTUNE") ? 1 : 0;
                    item.Q9_OTHERS = item.Q9.ToUpper().Contains("OTHER") ? 1 : 0;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<SURVEY_REPORT_AATA_MODEL> GetSurveyReportAataTab(filterOption model)
        {
            try
            {
                var query = $@" SELECT SURVEY_EDESC, SURVEY_CODE ,DIST_OUTLET_NAME ,FULL_NAME ,CREATED_DATE 
                                  ,max(case when table_title = 'Which Atta Packaging you like?' THEN Actual_Answer END) AS Q1
                                  ,max(case when table_title = 'What are the Atta Products available in your store?' THEN Actual_Answer END) AS Q2
                                  ,max(case when table_title = 'Which Atta has the highest demand?' THEN Actual_Answer END) AS Q3
                                  ,max(case when table_title = 'Which Atta you will recommend?' THEN Actual_Answer END) AS Q4
                                  ,max(case when table_title = 'What is the reason for Atta''s highest sale?' THEN Actual_Answer END) AS Q5
                                  ,max(case when table_title = 'Do you Have Hulas Atta?' THEN Actual_Answer END) AS Q6
                                  ,max(case when table_title = 'Which Atta is more Whiteness?' THEN Actual_Answer END) AS Q7
                                  ,max(case when table_title = 'Which Atta is more Softness?' THEN Actual_Answer END) AS Q8
                                  ,max(case when table_title = 'Which Atta is better in Taste?' THEN Actual_Answer END) AS Q9
                                  FROM (   SELECT TABLE_TITLE, SURVEY_CODE, SURVEY_EDESC ,DIST_OUTLET_NAME ,FULL_NAME ,CREATED_DATE , LISTAGG (ACTUAL_ANSWER, ',') WITHIN GROUP (ORDER BY ACTUAL_ANSWER ASC) AS ACTUAL_ANSWER FROM SURVEY_REPORT where survey_code='5' 
                                  GROUP BY TABLE_TITLE,SURVEY_EDESC ,SURVEY_CODE ,DIST_OUTLET_NAME ,FULL_NAME ,CREATED_DATE) where survey_code='3' 
                                  GROUP BY SURVEY_CODE, SURVEY_EDESC ,DIST_OUTLET_NAME ,FULL_NAME ,CREATED_DATE";
                var result = _objectEntity.SqlQuery<SURVEY_REPORT_AATA_MODEL>(query).ToList();

                foreach (var item in result)
                {
                    if (string.IsNullOrEmpty(item.Q1))
                        item.Q1 = "";
                    item.Q1_HULAS = item.Q1.ToUpper().Contains("HULAS") ? 1 : 0;
                    item.Q1_GYAN = item.Q1.ToUpper().Contains("GYAN") ? 1 : 0;
                    item.Q1_KALP = item.Q1.ToUpper().Contains("KALP") ? 1 : 0;
                    item.Q1_FORTUNE = item.Q1.ToUpper().Contains("FORTUNE") ? 1 : 0;
                    item.Q1_OTHERS = item.Q1.ToUpper().Contains("OTHER") ? 1 : 0;

                    if (string.IsNullOrEmpty(item.Q2))
                        item.Q2 = "";
                    item.Q2_HULAS = item.Q2.ToUpper().Contains("HULAS") ? 1 : 0;
                    item.Q2_GYAN = item.Q2.ToUpper().Contains("GYAN") ? 1 : 0;
                    item.Q2_KALP = item.Q2.ToUpper().Contains("KALP") ? 1 : 0;
                    item.Q2_FORTUNE = item.Q2.ToUpper().Contains("FORTUNE") ? 1 : 0;
                    item.Q2_OTHERS = item.Q2.ToUpper().Contains("OTHER") ? 1 : 0;

                    if (string.IsNullOrEmpty(item.Q3))
                        item.Q3 = "";
                    item.Q3_HULAS = item.Q3.ToUpper().Contains("HULAS") ? 1 : 0;
                    item.Q3_GYAN = item.Q3.ToUpper().Contains("GYAN") ? 1 : 0;
                    item.Q3_KALP = item.Q3.ToUpper().Contains("KALP") ? 1 : 0;
                    item.Q3_FORTUNE = item.Q3.ToUpper().Contains("FORTUNE") ? 1 : 0;
                    item.Q3_OTHERS = item.Q3.ToUpper().Contains("OTHER") ? 1 : 0;

                    if (string.IsNullOrEmpty(item.Q4))
                        item.Q4 = "";
                    item.Q4_HULAS = item.Q4.ToUpper().Contains("HULAS") ? 1 : 0;
                    item.Q4_GYAN = item.Q4.ToUpper().Contains("GYAN") ? 1 : 0;
                    item.Q4_KALP = item.Q4.ToUpper().Contains("KALP") ? 1 : 0;
                    item.Q4_FORTUNE = item.Q4.ToUpper().Contains("FORTUNE") ? 1 : 0;
                    item.Q4_OTHERS = item.Q4.ToUpper().Contains("OTHER") ? 1 : 0;

                    if (string.IsNullOrEmpty(item.Q7))
                        item.Q7 = "";
                    item.Q7_HULAS = item.Q7.ToUpper().Contains("HULAS") ? 1 : 0;
                    item.Q7_GYAN = item.Q7.ToUpper().Contains("GYAN") ? 1 : 0;
                    item.Q7_KALP = item.Q7.ToUpper().Contains("KALP") ? 1 : 0;
                    item.Q7_FORTUNE = item.Q7.ToUpper().Contains("FORTUNE") ? 1 : 0;
                    item.Q7_OTHERS = item.Q7.ToUpper().Contains("OTHER") ? 1 : 0;

                    if (string.IsNullOrEmpty(item.Q8))
                        item.Q8 = "";
                    item.Q8_HULAS = item.Q8.ToUpper().Contains("HULAS") ? 1 : 0;
                    item.Q8_GYAN = item.Q8.ToUpper().Contains("GYAN") ? 1 : 0;
                    item.Q8_KALP = item.Q8.ToUpper().Contains("KALP") ? 1 : 0;
                    item.Q8_FORTUNE = item.Q8.ToUpper().Contains("FORTUNE") ? 1 : 0;
                    item.Q8_OTHERS = item.Q8.ToUpper().Contains("OTHER") ? 1 : 0;

                    if (string.IsNullOrEmpty(item.Q9))
                        item.Q9 = "";
                    item.Q9_HULAS = item.Q9.ToUpper().Contains("HULAS") ? 1 : 0;
                    item.Q9_GYAN = item.Q9.ToUpper().Contains("GYAN") ? 1 : 0;
                    item.Q9_KALP = item.Q9.ToUpper().Contains("KALP") ? 1 : 0;
                    item.Q9_FORTUNE = item.Q9.ToUpper().Contains("FORTUNE") ? 1 : 0;
                    item.Q9_OTHERS = item.Q9.ToUpper().Contains("OTHER") ? 1 : 0;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<SURVEY_REPORT_MODEL> getSurveyReport_JGI(filterOption model)
        {
            try
            {
                var result = new List<SURVEY_REPORT_MODEL>();
                //var query = $@"SELECT * FROM (SELECT SURVEY_EDESC, ENTITY_CODE,ENTITY_TYPE, SURVEY_CODE, DIST_OUTLET_NAME, FULL_NAME, TRUNC(CREATED_DATE) CREATED_DATE,SUBTYPE_EDESC,TYPE_EDESC
                //                  , max(case when table_title = '3D board beer' THEN Actual_Answer END) AS Q1
                //                   , max(case when table_title = 'Availability of Beer' THEN Actual_Answer END) AS Q2
                //                    , max(case when table_title = 'Availability product of competitors' THEN Actual_Answer END) AS Q3
                //                     , max(case when table_title = 'Beer Flex Board' THEN Actual_Answer END) AS Q4
                //                      , max(case when table_title = 'Competitors 3D shop Board' THEN Actual_Answer END) AS Q5
                //                       , max(case when table_title = 'Competitors Flex shop Boards' THEN Actual_Answer END) AS Q6
                //                       , max(case when table_title = 'Is shop open' THEN Actual_Answer END) AS Q7
                //                        , max(case when table_title = 'Light Board Beer' THEN Actual_Answer END) AS Q8
                //                        , max(case when table_title = 'Other Remarks' THEN Actual_Answer END) AS Q9
                //                         , max(case when table_title = 'competitor Light Board' THEN Actual_Answer END) AS Q10
                //                          , max(case when table_title = 'our 3D shop board' THEN Actual_Answer END) AS Q11
                //                          , max(case when table_title = 'our flex shop board' THEN Actual_Answer END) AS Q12
                //                          , max(case when table_title = 'our light board' THEN Actual_Answer END) AS Q13
                //                          , max(case when table_title = 'what are the product available' THEN Actual_Answer END) AS Q14
                //                  FROM(SELECT SR.TABLE_TITLE, OT.TYPE_EDESC, OST.SUBTYPE_EDESC, SR.SURVEY_CODE, SR.ENTITY_CODE, SR.SURVEY_EDESC, SR.DIST_OUTLET_NAME, SR.FULL_NAME, SR.ENTITY_TYPE, TRUNC(SR.CREATED_DATE)CREATED_DATE, WM_CONCAT(SR.ACTUAL_ANSWER)ACTUAL_ANSWER FROM SURVEY_REPORT SR
                //                  INNER JOIN DIST_RESELLER_MASTER RM ON SR.ENTITY_CODE=RM.RESELLER_CODE
                //                  INNER JOIN  DIST_OUTLET_TYPE OT ON RM.OUTLET_TYPE_ID= OT.TYPE_ID
                //                  INNER JOIN  DIST_OUTLET_SUBTYPE OST ON RM.OUTLET_SUBTYPE_ID= OST.SUBTYPE_ID
                //                  WHERE SR.ENTITY_TYPE='R' AND  TRUNC(SR.CREATED_DATE) BETWEEN to_date('{model.ReportFilters.FromDate}','RRRR-MON-DD') AND to_date('{model.ReportFilters.ToDate}','RRRR-MON-DD')
                //                  GROUP BY SR.TABLE_TITLE, SR.SURVEY_CODE, SR.ENTITY_CODE, SR.SURVEY_EDESC, SR.DIST_OUTLET_NAME, SR.FULL_NAME, SR.ENTITY_TYPE, TRUNC(SR.CREATED_DATE),OT.TYPE_EDESC, OST.SUBTYPE_EDESC) 
                //                  GROUP BY SURVEY_CODE, SURVEY_EDESC ,DIST_OUTLET_NAME ,FULL_NAME,ENTITY_CODE ,ENTITY_TYPE,TRUNC(CREATED_DATE),TYPE_EDESC,SUBTYPE_EDESC)";
                var query = $@"  SELECT * FROM (SELECT SURVEY_EDESC, ENTITY_CODE,ENTITY_TYPE, SURVEY_CODE, DIST_OUTLET_NAME, FULL_NAME, TRUNC(CREATED_DATE) CREATED_DATE,SUBTYPE_EDESC,TYPE_EDESC
                                  , max(case when table_title = '3D board beer' THEN Actual_Answer END) AS Q1
                                   , max(case when table_title = 'Availability of Beer' THEN Actual_Answer END) AS Q2
                                    , max(case when table_title = 'Availability product of competitors' THEN Actual_Answer END) AS Q3
                                     , max(case when table_title = 'Beer Flex Board' THEN Actual_Answer END) AS Q4
                                      , max(case when table_title = 'Competitors 3D shop Board' THEN Actual_Answer END) AS Q5
                                       , max(case when table_title = 'Competitors Flex shop Boards' THEN Actual_Answer END) AS Q6
                                       , max(case when table_title = 'Is shop open' THEN Actual_Answer END) AS Q7
                                        , max(case when table_title = 'Light Board Beer' THEN Actual_Answer END) AS Q8
                                        , max(case when table_title = 'Other Remarks' THEN Actual_Answer END) AS Q9
                                         , max(case when table_title = 'competitor Light Board' THEN Actual_Answer END) AS Q10
                                          , max(case when table_title = 'our 3D shop board' THEN Actual_Answer END) AS Q11
                                          , max(case when table_title = 'our flex shop board' THEN Actual_Answer END) AS Q12
                                          , max(case when table_title = 'our light board' THEN Actual_Answer END) AS Q13
                                          , max(case when table_title = 'what are the product available' THEN Actual_Answer END) AS Q14
                                  FROM(SELECT TABLE_TITLE, TYPE_EDESC, SUBTYPE_EDESC, SURVEY_CODE, ENTITY_CODE, SURVEY_EDESC, DIST_OUTLET_NAME, FULL_NAME, ENTITY_TYPE, TRUNC(CREATED_DATE)CREATED_DATE, WM_CONCAT(ACTUAL_ANSWER)ACTUAL_ANSWER  FROM(
                                   SELECT SR.TABLE_TITLE, OT.TYPE_EDESC, OST.SUBTYPE_EDESC, SR.SURVEY_CODE, SR.ENTITY_CODE, SR.SURVEY_EDESC, SR.DIST_OUTLET_NAME, SR.FULL_NAME, SR.ENTITY_TYPE, MAX(SR.CREATED_DATE)CREATED_DATE, SR.ACTUAL_ANSWER FROM SURVEY_REPORT SR
                                  INNER JOIN DIST_RESELLER_MASTER RM ON SR.ENTITY_CODE = RM.RESELLER_CODE
                                  INNER JOIN  DIST_OUTLET_TYPE OT ON RM.OUTLET_TYPE_ID = OT.TYPE_ID
                                  INNER JOIN  DIST_OUTLET_SUBTYPE OST ON RM.OUTLET_SUBTYPE_ID = OST.SUBTYPE_ID
                                  WHERE SR.ENTITY_TYPE = 'R' AND  TRUNC(SR.CREATED_DATE) BETWEEN to_date('{model.ReportFilters.FromDate}', 'RRRR-MON-DD') AND to_date('{model.ReportFilters.ToDate}', 'RRRR-MON-DD')
                                  GROUP BY SR.TABLE_TITLE, SR.SURVEY_CODE, SR.ENTITY_CODE, SR.SURVEY_EDESC, SR.DIST_OUTLET_NAME, SR.FULL_NAME, SR.ENTITY_TYPE, OT.TYPE_EDESC, OST.SUBTYPE_EDESC, SR.ACTUAL_ANSWER)
                                    GROUP BY TABLE_TITLE, TYPE_EDESC, SUBTYPE_EDESC, SURVEY_CODE, ENTITY_CODE, SURVEY_EDESC, DIST_OUTLET_NAME, FULL_NAME, ENTITY_TYPE, TRUNC(CREATED_DATE))
                                  GROUP BY SURVEY_CODE, SURVEY_EDESC ,DIST_OUTLET_NAME ,FULL_NAME,ENTITY_CODE ,ENTITY_TYPE,TRUNC(CREATED_DATE),TYPE_EDESC,SUBTYPE_EDESC)";
                result = _objectEntity.SqlQuery<SURVEY_REPORT_MODEL>(query).ToList();
                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<SURVEY_REPORT_MODEL> GetSurveyReportDynamic(filterOption model)
        {
            var qList = string.Empty;
            var q_query = $@"SELECT DISTINCT TABLE_TITLE FROM SURVEY_REPORT ORDER BY TABLE_TITLE ASC ";
            var queList = _objectEntity.SqlQuery<string>(q_query).ToList();
            List<string> newQueList = new List<string>();
            List<string> newAnsList = new List<string>();
            if (queList.Count > 0)
            {
                for (int i = 0; i < queList.Count(); i++)
                {
                    var question = $@"'{queList[i].Replace("'", "")}' AS Q{i + 1}";
                    var answer = $@" Q{i + 1}";
                    newQueList.Add(question);
                    newAnsList.Add(answer);
                }
            }
            qList = string.Join(",", newQueList).ToString();
            var ansList = string.Join(",", newAnsList).ToString();

            //var actualQuery = $@"SELECT * FROM SURVEY_REPORT PIVOT (max(ACTUAL_ANSWER) FOR (TABLE_TITLE) IN ({qList})) WHERE SET_TYPE='T' AND TRUNC(CREATED_DATE) BETWEEN to_date('{model.ReportFilters.FromDate}','RRRR-MON-DD') AND to_date('{model.ReportFilters.ToDate}','RRRR-MON-DD')";
            var actualQuery = $@"SELECT * FROM (SELECT SURVEY_CODE,SURVEY_EDESC, ENTITY_CODE, OT.TYPE_EDESC,OST.SUBTYPE_EDESC, TABLE_TITLE,SET_TYPE,ENTITY_TYPE,DIST_OUTLET_NAME,FULL_NAME,WM_CONCAT(ACTUAL_ANSWER)ACTUAL_ANSWER,TRUNC(SR.CREATED_DATE)CREATED_DATE FROM SURVEY_REPORT SR
                                INNER JOIN DIST_RESELLER_MASTER RM ON SR.ENTITY_CODE=RM.RESELLER_CODE
                                  INNER JOIN  DIST_OUTLET_TYPE OT ON RM.OUTLET_TYPE_ID= OT.TYPE_ID
                                  INNER JOIN  DIST_OUTLET_SUBTYPE OST ON RM.OUTLET_SUBTYPE_ID= OST.SUBTYPE_ID
                                  WHERE SR.ENTITY_TYPE='R' AND TRUNC(SR.CREATED_DATE) BETWEEN to_date('{model.ReportFilters.FromDate}','RRRR-MON-DD') AND to_date('{model.ReportFilters.ToDate}','RRRR-MON-DD')
                                GROUP BY SURVEY_EDESC, TABLE_TITLE,DIST_OUTLET_NAME,FULL_NAME,ENTITY_CODE ,SET_TYPE,ENTITY_TYPE,SURVEY_CODE,TRUNC(SR.CREATED_DATE),OT.TYPE_EDESC,OST.SUBTYPE_EDESC)T
                               PIVOT (MAX(ACTUAL_ANSWER) FOR (TABLE_TITLE) IN ({qList}))";
            //var actualResult = $@"SELECT * FROM (SELECT SURVEY_CODE,SURVEY_EDESC, ENTITY_CODE, TABLE_TITLE,SET_TYPE,ENTITY_TYPE,DIST_OUTLET_NAME,FULL_NAME,WM_CONCAT(ACTUAL_ANSWER)ACTUAL_ANSWER,TRUNC(CREATED_DATE)CREATED_DATE FROM SURVEY_REPORT
            //                    GROUP BY SURVEY_EDESC, TABLE_TITLE,DIST_OUTLET_NAME,FULL_NAME,ENTITY_CODE ,SET_TYPE,ENTITY_TYPE,SURVEY_CODE,TRUNC(CREATED_DATE))T
            //                   PIVOT (MAX(ACTUAL_ANSWER) FOR (TABLE_TITLE) IN ({qList}))
            //                  WHERE TRUNC(CREATED_DATE) BETWEEN to_date('{model.ReportFilters.FromDate}','RRRR-MON-DD') AND to_date('{model.ReportFilters.ToDate}','RRRR-MON-DD')";
            //var response = _objectEntity.SqlQuery(actualQuery);
            //var list = response.AsEnumerable().ToList();
            var result = _objectEntity.SqlQuery<SURVEY_REPORT_MODEL>(actualQuery).ToList();

            return result;
        }
        public List<SURVEY_REPORT_MODEL> GetDynamicWebSurveyReport(filterOption model, User userInfo)
        {
            var qList = string.Empty;
            var q_query = $@"SELECT DISTINCT QUESTION TITLE FROM DIST_WEB_QUE_ANS WHERE CREATED_BY ='{userInfo.User_id}' AND COMPANY_CODE='{userInfo.company_code}' ORDER BY QUESTION ASC";
            var queList = _objectEntity.SqlQuery<string>(q_query).ToList();
            List<string> newQueList = new List<string>();
            List<string> newAnsList = new List<string>();
            if (queList.Count > 0)
            {
                for (int i = 0; i < queList.Count(); i++)
                {
                    var question = $@"'{queList[i].Replace("'", "")}' AS Q{i + 1}";
                    var answer = $@" Q{i + 1}";
                    newQueList.Add(question);
                    newAnsList.Add(answer);
                }
            }
            qList = string.Join(",", newQueList).ToString();
            var ansList = string.Join(",", newAnsList).ToString();
            var result = new List<SURVEY_REPORT_MODEL>();
            try
            {
                var actualQuery = $@"SELECT * FROM
                                    (
                                      SELECT TRUNC(WQA.CREATED_DATE)CREATED_DATE,QUESTION,ANSWER,WEB_QUE_ANS_ID,SA.LOGIN_EDESC FULL_NAME,WQA.CUSTOMER_NAME DIST_OUTLET_NAME FROM DIST_WEB_QUE_ANS WQA
                                      INNER JOIN SC_APPLICATION_USERS SA ON WQA.CREATED_BY = SA.USER_NO 
                                      WHERE WQA.CREATED_BY ='{userInfo.User_id}' AND WQA.COMPANY_CODE='{userInfo.company_code}' 
                                      AND TRUNC(WQA.CREATED_DATE) BETWEEN to_date('{model.ReportFilters.FromDate}','RRRR-MON-DD') AND to_date('{model.ReportFilters.ToDate}','RRRR-MON-DD')
                                    )
                                    PIVOT
                                    (
                                      MAX(ANSWER)
                                      FOR QUESTION IN ({qList})
                                    )
                                    ORDER BY WEB_QUE_ANS_ID";

                result = _objectEntity.SqlQuery<SURVEY_REPORT_MODEL>(actualQuery).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public List<SURVEY_REPORT_BRANDING_MODEL> GetBrandingSurveyReportTab(filterOption model)
        {
            try
            {
                var result = new List<SURVEY_REPORT_BRANDING_MODEL>();
                var query = $@"SELECT * FROM (SELECT SURVEY_EDESC, ENTITY_CODE,ENTITY_TYPE, SURVEY_CODE, DIST_OUTLET_NAME, FULL_NAME, TRUNC(CREATED_DATE) CREATED_DATE,SUBTYPE_EDESC,TYPE_EDESC
                                  , max(case when table_title = 'Is there any shop board of JGI Brands?' THEN Actual_Answer END) AS Q1
                                   , max(case when table_title = 'If Yes which Brand ?' THEN Actual_Answer END) AS Q2
                                    , max(case when table_title = 'How is the condition of Board ?' THEN Actual_Answer END) AS Q3
                                     , max(case when table_title = 'Is there any other Brands Board Beside JGI ?' THEN Actual_Answer END) AS Q4
                                      , max(case when table_title = 'If Yes, Which Other Brand ?' THEN Actual_Answer END) AS Q5
                                  FROM(SELECT SR.TABLE_TITLE, OT.TYPE_EDESC, OST.SUBTYPE_EDESC, SR.SURVEY_CODE, SR.ENTITY_CODE, SR.SURVEY_EDESC, SR.DIST_OUTLET_NAME, SR.FULL_NAME, SR.ENTITY_TYPE, TRUNC(SR.CREATED_DATE)CREATED_DATE, WM_CONCAT(SR.ACTUAL_ANSWER)ACTUAL_ANSWER FROM SURVEY_REPORT SR
                                  INNER JOIN DIST_RESELLER_MASTER RM ON SR.ENTITY_CODE=RM.RESELLER_CODE
                                  INNER JOIN  DIST_OUTLET_TYPE OT ON RM.OUTLET_TYPE_ID= OT.TYPE_ID
                                  INNER JOIN  DIST_OUTLET_SUBTYPE OST ON RM.OUTLET_SUBTYPE_ID= OST.SUBTYPE_ID
                                  WHERE SR.ENTITY_TYPE='R' AND  TRUNC(SR.CREATED_DATE) BETWEEN to_date('{model.ReportFilters.FromDate}','RRRR-MON-DD') AND to_date('{model.ReportFilters.ToDate}','RRRR-MON-DD')
                                  GROUP BY SR.TABLE_TITLE, SR.SURVEY_CODE, SR.ENTITY_CODE, SR.SURVEY_EDESC, SR.DIST_OUTLET_NAME, SR.FULL_NAME, SR.ENTITY_TYPE, TRUNC(SR.CREATED_DATE),OT.TYPE_EDESC, OST.SUBTYPE_EDESC) 
                                  GROUP BY SURVEY_CODE, SURVEY_EDESC ,DIST_OUTLET_NAME ,FULL_NAME,ENTITY_CODE ,ENTITY_TYPE,TRUNC(CREATED_DATE),TYPE_EDESC,SUBTYPE_EDESC)";
                result = _objectEntity.SqlQuery<SURVEY_REPORT_BRANDING_MODEL>(query).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<MerchandisingStockModel> GetMerchandisingStockReport(filterOption model, User userInfo)
        {
            var result = new List<MerchandisingStockModel>();
            try
            {
                var Query = $@" SELECT T.CUSTOMER_CODE,T.CUSTOMER_EDESC, T.MU_CODE, T.ITEM_CODE, T.ITEM_EDESC, T.CURRENT_STOCK,(T.CURRENT_STOCK-NVL(SUM(BS.QUANTITY),0))REMAINING_STOCK,T.CREATED_DATE, T.SP_CODE, T.GROUP_EDESC, T.BRAND_NAME,T.EMPLOYEE_EDESC, T.COMPANY_CODE,T.TYPE_EDESC, T.SUBTYPE_EDESC,NVL(SUM(BS.QUANTITY),0)QTY  FROM (SELECT CUSTOMER_CODE,CUSTOMER_EDESC,MU_CODE,ITEM_CODE,ITEM_EDESC,MAX(CURRENT_STOCK)CURRENT_STOCK,CREATED_DATE, SP_CODE,GROUP_EDESC,BRAND_NAME,EMPLOYEE_EDESC,COMPANY_CODE,TYPE_EDESC,SUBTYPE_EDESC FROM
                                   (SELECT CS.RESELLER_CODE CUSTOMER_CODE,CS.RESELLER_NAME CUSTOMER_EDESC, DRS.MU_CODE,DRS.ITEM_CODE,IMS.ITEM_EDESC,DRS.CURRENT_STOCK,DRS.PURCHASE_QTY,DRS.SP_CODE,GM.GROUP_EDESC ,ISS.BRAND_NAME,
                                     NVL(HES.EMPLOYEE_EDESC,'Self') EMPLOYEE_EDESC,DRS.CREATED_DATE,DRS.COMPANY_CODE ,OT. TYPE_EDESC,OST. SUBTYPE_EDESC
                                FROM DIST_RESELLER_STOCK DRS
                                    INNER JOIN DIST_RESELLER_MASTER CS ON DRS.RESELLER_CODE = CS.RESELLER_CODE AND DRS.COMPANY_CODE = CS.COMPANY_CODE
                                    INNER JOIN DIST_OUTLET_TYPE OT ON CS.OUTLET_TYPE_ID = OT.TYPE_ID AND CS.COMPANY_CODE = OT.COMPANY_CODE
                                    INNER JOIN DIST_OUTLET_SUBTYPE OST ON CS.OUTLET_SUBTYPE_ID = OST.SUBTYPE_ID AND CS.COMPANY_CODE = OST.COMPANY_CODE
                                    INNER JOIN DIST_GROUP_MASTER GM ON CS.GROUPID = GM.GROUPID AND CS.COMPANY_CODE = GM.COMPANY_CODE
                                    INNER JOIN IP_ITEM_MASTER_SETUP IMS ON DRS.ITEM_CODE = IMS.ITEM_CODE AND DRS.COMPANY_CODE = IMS.COMPANY_CODE
                                    INNER JOIN IP_ITEM_SPEC_SETUP ISS ON IMS.ITEM_CODE = ISS.ITEM_CODE AND IMS.COMPANY_CODE = ISS.COMPANY_CODE
                                    LEFT JOIN HR_EMPLOYEE_SETUP HES ON DRS.SP_CODE = HES.EMPLOYEE_CODE AND DRS.COMPANY_CODE= HES.COMPANY_CODE
                                    WHERE DRS.DELETED_FLAG='N' AND DRS.CURRENT_STOCK <>0  AND DRS.COMPANY_CODE='{userInfo.company_code}' 
                                    AND TRUNC(DRS.CREATED_DATE) BETWEEN to_date('{model.ReportFilters.FromDate}','RRRR-MON-DD') AND to_date('{model.ReportFilters.ToDate}','RRRR-MON-DD')) 
                                    GROUP BY CUSTOMER_CODE,CUSTOMER_EDESC,MU_CODE,ITEM_CODE,ITEM_EDESC, SP_CODE,GROUP_EDESC,BRAND_NAME,COMPANY_CODE,CREATED_DATE,EMPLOYEE_EDESC,TYPE_EDESC,SUBTYPE_EDESC)T
                                     LEFT JOIN BRD_SCHEME BS ON T.CUSTOMER_CODE = BS.RESELLER_CODE AND T.COMPANY_CODE = BS.COMPANY_CODE AND T.ITEM_CODE = BS.ITEM_CODE 
                                     GROUP BY T.CUSTOMER_CODE,T.CUSTOMER_EDESC, T.MU_CODE, T.ITEM_CODE, T.ITEM_EDESC, T.CURRENT_STOCK,T.CREATED_DATE, T.SP_CODE, T.GROUP_EDESC, T.BRAND_NAME,T.EMPLOYEE_EDESC, T.COMPANY_CODE,T.TYPE_EDESC, T.SUBTYPE_EDESC";

                result = _objectEntity.SqlQuery<MerchandisingStockModel>(Query).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        public List<DistAttendanceReportSummary> GetAttendanceSummaryGroup(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Join(",", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            //var BranchFilter = string.Empty;
            //if (model.BranchFilter.Count > 0)
            //{
            //    BranchFilter = string.Format(@" AND  DPO.BRANCH_CODE IN  ('{0}')", string.Join("','", model.BranchFilter).ToString());
            //}
            var filter = "";
            if (model.ItemBrandFilter.Count > 0)
                filter = $" AND DL.SP_CODE  IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                filter = $" AND DL.SP_CODE  IN ({userInfo.sp_codes})";


            var Query = $@"SELECT GroupName,TotalPresent,TOTALABSENT,TOTALLEAVE,DAYOFF,HOLIDAY,WORKATHOLIDAY,WORKATDAYOFF,TARGET,TOTAL_VISIT,      PJP  Productive,
         ROUND (PJP / DECODE (TOTAL_VISIT, 0, 1, TOTAL_VISIT) * 100) Effective,ROUND(TOTAL_VISIT/DECODE(TARGET,0,1,TARGET)*100) Coverage  FROM (SELECT AB.GROUP_EDESC as GroupName,SUM(PRESENT) TotalPresent,SUM(ABSENT) TotalAbsent,SUM(LEAVE) TotalLeave,SUM(DAYOFF) DayOff,SUM(HOLIDAY) Holiday,SUM(WORK_DAYOFF) WorkAtDayOff,SUM( WORK_HOLIDAY) WorkAtHoliday, (select COUNT (*) from DIST_TARGET_ENTITY where TRIM(AB.GROUP_EDESC)=TRIM(GROUP_EDESC)
 AND  TRUNC(ASSIGN_DATE) BETWEEN  TO_DATE('{model.FromDate}','YYYY-MON-DD') AND TO_DATE('{model.ToDate}','YYYY-MON-DD') AND ENTITY_CODE IS NOT NULL  ) Target 
 ,(SELECT COUNT(CUSTOMER_CODE) FROM (SELECT DISTINCT  CUSTOMER_CODE , B.FULL_NAME,A.GROUPID FROM DIST_VISITED_ENTITY  A, DIST_LOGIN_USER B
 WHERE A.SP_CODE=B.SP_CODE AND b.groupid=a.groupid 
 AND   TRUNC(UPDATE_DATE) BETWEEN  TO_DATE('{model.FromDate}','YYYY-MON-DD') AND TO_DATE('{model.ToDate}','YYYY-MON-DD')
 ) CDE WHERE  CDE.groupid =AB.GROUPID) Total_Visit
 ,(SELECT COUNT(*) FROM (SELECT DISTINCT A.RESELLER_CODE ,B.GROUPID FROM DIST_VISITED_PO A,DIST_RESELLER_MASTER B
WHERE A.RESELLER_CODE=B.RESELLER_CODE AND A.COMPANY_CODE=B.COMPANY_CODE  AND TRUNC(A.ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}','YYYY-MON-DD') AND TO_DATE('{model.ToDate}','YYYY-MON-DD')  ) AC WHERE AC.GROUPID=AB.GROUPID)  PJP FROM ( SELECT EMPLOYEE_ID, DL.GROUP_EDESC,DL.GROUPID,DL.FULL_NAME, 
COUNT(  
CASE  
WHEN OVERALL_STATUS IN ('TV','TN','PR','BA','LA','TP','LP','VP')  
THEN 1  
END) AS PRESENT,  
COUNT(  
CASE OVERALL_STATUS  
WHEN 'AB'  
THEN 1  
END) AS ABSENT,  
COUNT(  
CASE OVERALL_STATUS  
WHEN 'LV'  
THEN 1  
END) AS LEAVE,  
COUNT(  
CASE OVERALL_STATUS  
WHEN 'DO'  
THEN 1  
END) AS DAYOFF,  
COUNT(  
CASE OVERALL_STATUS  
WHEN 'HD'  
THEN 1  
END) AS HOLIDAY,  
COUNT(  
CASE OVERALL_STATUS  
WHEN 'WD'  
THEN 1  
END) AS WORK_DAYOFF,  
COUNT(  
CASE OVERALL_STATUS  
WHEN 'WH'  
THEN 1  
END) AS WORK_HOLIDAY  
FROM HRIS_ATTENDANCE_DETAIL  HD,DIST_lOGIN_GROUP DL 
WHERE  HD.EMPLOYEE_ID=DL.SP_CODE   {filter}
and ATTENDANCE_DT BETWEEN TO_DATE('{model.FromDate}','YYYY-MON-DD') AND TO_DATE('{model.ToDate}','YYYY-MON-DD')  
GROUP BY EMPLOYEE_ID, DL.GROUP_EDESC,DL.GROUPID,DL.FULL_NAME) AB 
GROUP   BY AB.GROUP_EDESC,AB.GROUPID order by AB.groupid )";
            var result = _objectEntity.SqlQuery<DistAttendanceReportSummary>(Query).ToList();
            return result;
        }

        public List<DistAttendanceReportSummary> GetAttendanceSummaryEmployeeWise(ReportFiltersModel model, User userInfo, String GroupWise)
        {
            var companyCode = string.Join(",", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            //var BranchFilter = string.Empty;
            //if (model.BranchFilter.Count > 0)
            //{
            //    BranchFilter = string.Format(@" AND  DPO.BRANCH_CODE IN  ('{0}')", string.Join("','", model.BranchFilter).ToString());
            //}
            var filter = "";
            if (model.ItemBrandFilter.Count > 0)
                filter = $" AND DL.SP_CODE  IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                filter = $" AND DL.SP_CODE  IN ({userInfo.sp_codes})";

            string QueryFilter = "";
            if (!string.IsNullOrEmpty(GroupWise))
            {
                QueryFilter = $@" and  DL.GROUP_EDESC='{GroupWise}'";
            }


            var Querystring = $@"SELECT GroupName,GROUPID,FULL_NAME, TARGET,TOTAL_VISIT,TOTALPRESENT,TOTALABSENT,TOTALLEAVE,DAYOFF, HOLIDAY,WORKATDAYOFF,WORKATHOLIDAY,ROUND(PJP/DECODE(TOTAL_VISIT,0,1,TOTAL_VISIT)*100) Productive,ROUND(TOTAL_VISIT/DECODE(TARGET,0,1,TARGET)*100) Coverage FROM (SELECT to_char(EMPLOYEE_ID), DL.GROUP_EDESC GroupName,DL.GROUPID,DL.FULL_NAME, 
(select COUNT (*) from DIST_TARGET_ENTITY where to_char(EMPLOYEE_ID)=sp_code
 AND  TRUNC(ASSIGN_DATE) BETWEEN TO_DATE('{model.FromDate}','YYYY-MON-DD') AND TO_DATE('{model.ToDate}','YYYY-MON-DD')  ) Target ,
 (SELECT COUNT(DISTINCT CUSTOMER_CODE) FROM DIST_VISITED_ENTITY WHERE to_char(EMPLOYEE_ID)=sp_code
 AND  TRUNC(UPDATE_DATE) BETWEEN TO_DATE('{model.FromDate}','YYYY-MON-DD') AND TO_DATE('{model.ToDate}','YYYY-MON-DD') ) Total_Visit
  ,(select COUNT(DISTINCT RESELLER_CODE) from DIST_VISITED_PO  WHERE  TRUNC(ORDER_DATE) BETWEEN TO_DATE('{model.FromDate}','YYYY-MON-DD') AND TO_DATE('{model.ToDate}','YYYY-MON-DD')  AND sp_code=to_char(EMPLOYEE_ID))  PJP,
COUNT(  
CASE  
WHEN OVERALL_STATUS IN ('TV','TN','PR','BA','LA','TP','LP','VP')  
THEN 1  
END) AS TotalPresent,  
COUNT(  
CASE OVERALL_STATUS  
WHEN 'AB'  
THEN 1  
END) AS TotalAbsent,  
COUNT(  
CASE OVERALL_STATUS  
WHEN 'LV'  
THEN 1  
END) AS TotalLeave,  
COUNT(  
CASE OVERALL_STATUS  
WHEN 'DO'  
THEN 1  
END) AS DayOff,  
COUNT(  
CASE OVERALL_STATUS  
WHEN 'HD'  
THEN 1  
END) AS Holiday,  
COUNT(  
CASE OVERALL_STATUS  
WHEN 'WD'  
THEN 1  
END) AS WorkAtDayOff,  
COUNT(  
CASE OVERALL_STATUS  
WHEN 'WH'  
THEN 1  
END) AS WorkAtHoliday  
FROM HRIS_ATTENDANCE_DETAIL  HD,DIST_lOGIN_GROUP DL 
WHERE  HD.EMPLOYEE_ID=DL.SP_CODE {QueryFilter}  {filter}  
AND ATTENDANCE_DT BETWEEN TO_DATE('{model.FromDate}','YYYY-MM-DD') AND TO_DATE('{model.ToDate}','YYYY-MM-DD')
GROUP BY EMPLOYEE_ID, DL.GROUP_EDESC,DL.GROUPID,DL.FULL_NAME)";
            var result = _objectEntity.SqlQuery<DistAttendanceReportSummary>(Querystring).ToList();
            return result;
        }

        public List<AttendanceModel> GetAttendanceReportEmployeeWise(ReportFiltersModel model, User userInfo, string sp_code)
        {
            var companyCode = string.Join("','", model.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            //var BranchFilter = string.Empty;
            //if (model.BranchFilter.Count > 0)
            //{
            //    BranchFilter = string.Format(@" AND  DPO.BRANCH_CODE IN  ('{0}')", string.Join("','", model.BranchFilter).ToString());
            //}
            var SalesPersonFilter = "";

            SalesPersonFilter = $" AND LU.SP_CODE IN ('{sp_code}')";

            //var query = $@"SELECT LT.SP_CODE,LT.ATT_DATE,LT.CHECKIN,LT.CHECKOUT,LT.ATTNCHECKOUT_TIME,
            //            LU.FULL_NAME,LU.USER_NAME,LU.CONTACT_NO,LU.EMAIL,
            //            GM.GROUP_EDESC,wm_concat(PI.FILENAME)  FILENAME,
            //            CASE LT.ATN_LOCATION
            //                WHEN ',' THEN NULL
            //                ELSE LT.ATN_LOCATION
            //            END ATN_LOCATION,
            //            CASE LT.EOD_LOCATION
            //                WHEN ',' THEN NULL
            //                ELSE LT.EOD_LOCATION
            //            END EOD_LOCATION
            //FROM (
            //  select AB.SP_CODE,AB.ATT_DATE,AB.CHECKIN,AB.CHECKOUT,AB.COMPANY_CODE,AB.ATN_LOCATION,AB.EOD_LOCATION,D.ATTNCHECKOUT_TIME from (SELECT A.SP_CODE,A.ATT_DATE,A.CHECKIN,B.CHECKOUT,A.COMPANY_CODE,A.LATITUDE||','||A.LONGITUDE ATN_LOCATION,B.LATITUDE||','||B.LONGITUDE EOD_LOCATION FROM 
            //               (SELECT DISTINCT SP_CODE, TRUNC(SUBMIT_DATE) ATT_DATE,MIN(SUBMIT_DATE) CHECKIN,COMPANY_CODE,LONGITUDE,LATITUDE  FROM   
            //               DIST_LM_LOCATION_TRACKING   
            //               WHERE TRACK_TYPE = 'ATN'
            //               GROUP BY SP_CODE,TRUNC(SUBMIT_DATE),COMPANY_CODE,LONGITUDE,LATITUDE
            //               ORDER BY SP_CODE, TRUNC(SUBMIT_DATE) ) A
            //           FULL OUTER JOIN
            //               (SELECT DISTINCT SP_CODE, TRUNC(SUBMIT_DATE) ATT_DATE,MAX(SUBMIT_DATE) CHECKOUT,COMPANY_CODE,LONGITUDE,LATITUDE   FROM   
            //               DIST_LM_LOCATION_TRACKING
            //               WHERE TRACK_TYPE = 'EOD'
            //               GROUP BY SP_CODE,TRUNC(SUBMIT_DATE),COMPANY_CODE,LONGITUDE,LATITUDE
            //               ORDER BY SP_CODE,MAX(SUBMIT_DATE) DESC ) B ON A.SP_CODE = B.SP_CODE AND A.COMPANY_CODE = B.COMPANY_CODE AND A.ATT_DATE = B.ATT_DATE) AB
            //                FULL OUTER JOIN
            //                 (SELECT EMPLOYEE_ID SP_CODE,TRUNC(ATTENDANCE_DT) ATTNCHECKOUT_DATE,MAX(ATTENDANCE_TIME) ATTNCHECKOUT_TIME FROM HRIS_ATTENDANCE WHERE ATTENDANCE_FROM ='MOBILE'
            //GROUP BY EMPLOYEE_ID,TRUNC(ATTENDANCE_DT) ORDER BY EMPLOYEE_ID,TRUNC(ATTENDANCE_DT)
            //) D on  AB.SP_CODE = D.SP_CODE 
            // AND AB.ATT_DATE = D.ATTNCHECKOUT_DATE ) LT,
            //           DIST_LOGIN_USER LU,
            //           DIST_GROUP_MASTER GM,
            //           DIST_PHOTO_INFO PI
            //    WHERE LT.SP_CODE = LU.SP_CODE
            //           AND LT.COMPANY_CODE = LU.COMPANY_CODE
            //           AND LU.GROUPID = GM.GROUPID
            //           AND LU.ACTIVE = 'Y' 
            //           AND LU.BRANDING='N'
            //           AND LU.COMPANY_CODE = GM.COMPANY_CODE
            //           AND LT.COMPANY_CODE = PI.COMPANY_CODE(+)
            //           AND LT.ATT_DATE= TRUNC(PI.CREATE_DATE(+))
            //           AND LT.SP_CODE = PI.ENTITY_CODE(+)
            //           AND PI.CATEGORYID(+) = '1' AND PI.ENTITY_TYPE(+) = 'S'
            //           AND LT.COMPANY_CODE IN('{companyCode}')
            //          {SalesPersonFilter}
            //          AND LT.ATT_DATE >=TO_DATE('{model.FromDate}','YYYY-MM-DD') AND LT.ATT_DATE <=TO_DATE('{model.ToDate}','YYYY-MM-DD')
            //GROUP BY  LT.SP_CODE,LT.ATT_DATE,LT.CHECKIN,LT.CHECKOUT,
            //                 LU.FULL_NAME,LU.USER_NAME,LU.CONTACT_NO,
            //                 LU.EMAIL,GM.GROUP_EDESC,LT.ATN_LOCATION,LT.EOD_LOCATION,LT.ATTNCHECKOUT_TIME
            //ORDER BY LT.ATT_DATE DESC,FULL_NAME";

            var query = $@"select TA.SP_CODE,TA.ATT_DATE,TA.CHECKIN,TA.CHECKOUT,TA.ATTNCHECKIN_TIME,TA.ATTNCHECKOUT_TIME,C.FIRST_CALL,C.LAST_CALL,TA.FULL_NAME,TA.USER_NAME,TA.CONTACT_NO,TA.EMAIL,TA.GROUP_EDESC,TA.FILENAME,TA.ATN_LOCATION,TA.EOD_LOCATION
 ,CASE TA.ATTNCHECKOUT_TIME WHEN TA.ATTNCHECKIN_TIME THEN NULL ELSE TA.ATTNCHECKOUT_TIME   END ATTNCHECKOUT from(SELECT LT.SP_CODE, LT.ATT_DATE, LT.CHECKIN, LT.CHECKOUT, LT.ATTNCHECKIN_TIME, LT.ATTNCHECKOUT_TIME,
                        LU.FULL_NAME, LU.USER_NAME, LU.CONTACT_NO, LU.EMAIL,
                        GM.GROUP_EDESC, wm_concat(PI.FILENAME)  FILENAME,
                        CASE LT.ATN_LOCATION
                            WHEN ',' THEN NULL
                            ELSE LT.ATN_LOCATION
                        END ATN_LOCATION,
                        CASE LT.EOD_LOCATION
                            WHEN ',' THEN NULL
                            ELSE LT.EOD_LOCATION
                        END EOD_LOCATION
            FROM(
              select AB.SP_CODE, AB.ATT_DATE, AB.CHECKIN, AB.CHECKOUT, AB.COMPANY_CODE, AB.ATN_LOCATION, AB.EOD_LOCATION, D.ATTNCHECKIN_TIME, D.ATTNCHECKOUT_TIME from(
SELECT A.SP_CODE, A.ATT_DATE, A.CHECKIN, B.CHECKOUT, A.COMPANY_CODE, A.LATITUDE || ',' || A.LONGITUDE ATN_LOCATION, B.LATITUDE || ',' || B.LONGITUDE EOD_LOCATION FROM
                           (
                           SELECT DISTINCT SP_CODE, TRUNC(SUBMIT_DATE) ATT_DATE, MIN(SUBMIT_DATE) CHECKIN, COMPANY_CODE, LONGITUDE, LATITUDE  FROM
                           DIST_LM_LOCATION_TRACKING
                           WHERE TRACK_TYPE = 'ATN'
                           GROUP BY SP_CODE, TRUNC(SUBMIT_DATE), COMPANY_CODE, LONGITUDE, LATITUDE
                           ORDER BY SP_CODE, TRUNC(SUBMIT_DATE)) A
                       FULL OUTER JOIN
                           (SELECT DISTINCT SP_CODE, TRUNC(SUBMIT_DATE) ATT_DATE, MAX(SUBMIT_DATE) CHECKOUT, COMPANY_CODE, LONGITUDE, LATITUDE   FROM
                           DIST_LM_LOCATION_TRACKING
                           WHERE TRACK_TYPE = 'EOD'
                           GROUP BY SP_CODE, TRUNC(SUBMIT_DATE), COMPANY_CODE, LONGITUDE, LATITUDE
                           ORDER BY SP_CODE, MAX(SUBMIT_DATE) DESC) B ON A.SP_CODE = B.SP_CODE AND A.COMPANY_CODE = B.COMPANY_CODE AND A.ATT_DATE = B.ATT_DATE) AB
                            FULL OUTER JOIN
                             (
                             SELECT EMPLOYEE_ID SP_CODE, TRUNC(ATTENDANCE_DT) ATTNCHECKOUT_DATE, MIN(ATTENDANCE_TIME) ATTNCHECKIN_TIME, MAX(ATTENDANCE_TIME) ATTNCHECKOUT_TIME
                              FROM HRIS_ATTENDANCE WHERE ATTENDANCE_FROM = 'MOBILE'
            GROUP BY EMPLOYEE_ID, TRUNC(ATTENDANCE_DT) ORDER BY EMPLOYEE_ID, TRUNC(ATTENDANCE_DT)
            ) D on  AB.SP_CODE = D.SP_CODE
             AND AB.ATT_DATE = D.ATTNCHECKOUT_DATE
              ) LT,
                       DIST_LOGIN_USER LU,
                       DIST_GROUP_MASTER GM,
                       DIST_PHOTO_INFO PI
                WHERE LT.SP_CODE = LU.SP_CODE
                       AND LT.COMPANY_CODE = LU.COMPANY_CODE
                       AND LU.GROUPID = GM.GROUPID
                       AND LU.ACTIVE = 'Y'
                       AND LU.BRANDING = 'N'
                       AND LU.COMPANY_CODE = GM.COMPANY_CODE
                       AND LT.COMPANY_CODE = PI.COMPANY_CODE(+)
                       AND LT.ATT_DATE = TRUNC(PI.CREATE_DATE(+))
                       AND LT.SP_CODE = PI.ENTITY_CODE(+)
                       AND PI.CATEGORYID(+) = '1' AND PI.ENTITY_TYPE(+) = 'S'
                          AND LT.COMPANY_CODE IN('{companyCode}')
                      {SalesPersonFilter}
                      AND LT.ATT_DATE >=TO_DATE('{model.FromDate}','YYYY-MM-DD') AND LT.ATT_DATE <=TO_DATE('{model.ToDate}','YYYY-MM-DD')
            GROUP BY  LT.SP_CODE, LT.ATT_DATE, LT.CHECKIN, LT.CHECKOUT,
                             LU.FULL_NAME, LU.USER_NAME, LU.CONTACT_NO,
                             LU.EMAIL, GM.GROUP_EDESC, LT.ATN_LOCATION, LT.EOD_LOCATION, LT.ATTNCHECKIN_TIME, LT.ATTNCHECKOUT_TIME
            ORDER BY LT.ATT_DATE DESC, FULL_NAME) ta INNER JOIN
              (select SP_CODE, TRUNC(UPDATE_DATE) CALL_DATE, MIN(UPDATE_DATE) FIRST_CALL, MAX(UPDATE_DATE) LAST_CALL, COMPANY_CODE from dist_location_track
              GROUP BY SP_CODE, TRUNC(UPDATE_DATE), COMPANY_CODE
              ORDER BY SP_CODE, TRUNC(UPDATE_DATE)) C ON ta.SP_CODE = C.SP_CODE  AND TRUNC(ta.ATT_DATE) = TRUNC(C.FIRST_CALL) ORDER BY SP_CODE,ATT_DATE";
            var result = _objectEntity.SqlQuery<AttendanceModel>(query).ToList();
            return result;
        }

        public List<StockSummary> GetStockGroupSummary(ReportFiltersModel model, User userInfo)
        {
            var filter = "";
            if (model.ItemBrandFilter.Count > 0)
                filter = $" AND A.SP_CODE  IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                filter = $" AND A.SP_CODE  IN ({userInfo.sp_codes})";

            var Query = $@"SELECT * FROM (SELECT GROUPID,GROUP_EDESC,BRAND_NAME,CASE T.MU_CODE
                    WHEN T.INDEX_MU_CODE THEN NVL(SUM(T.CURRENT_STOCK),0)
                    ELSE  round(NVL(SUM(T.CURRENT_STOCK/IUS.CONVERSION_FACTOR),0),2)
                END CURRENT_STOCK FROM (SELECT C.GROUP_EDESC,C.groupid, A.RESELLER_CODE, B.RESELLER_NAME, A.SP_CODE, D.BRAND_NAME, A.ITEM_CODE, A.CURRENT_STOCK, A.MU_CODE ,A.COMPANY_CODE,IP.INDEX_MU_CODE
                            FROM DIST_RESELLER_STOCK A, DIST_RESELLER_MASTER B , DIST_GROUP_MASTER C, IP_ITEM_SPEC_SETUP D,IP_ITEM_MASTER_SETUP IP
                            WHERE TRUNC(A.CREATED_DATE) = (SELECT MAX(TRUNC(CREATED_DATE)) FROM DIST_RESELLER_STOCK
                            WHERE  RESELLER_CODE = A.RESELLER_CODE) 
                            AND A.RESELLER_CODE = B.RESELLER_CODE
                            AND A.COMPANY_CODE = B.COMPANY_CODE
                            AND B.GROUPID = C.GROUPID
                            AND B.COMPANY_CODE = C.COMPANY_CODE
                            AND A.ITEM_CODE = D.ITEM_CODE
                            AND A.COMPANY_CODE = D.COMPANY_CODE
                            AND A.ITEM_CODE=IP.ITEM_CODE
                            AND A.COMPANY_CODE=IP.COMPANY_CODE
                                      {filter}
                          AND TRUNC(A.CREATED_DATE)<=TO_DATE('{model.FromDate}','YYYY-MM-DD') 
                            AND A.CURRENT_STOCK > 0)  T  LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON T.ITEM_CODE = IUS.ITEM_CODE AND T.COMPANY_CODE = IUS.COMPANY_CODE GROUP BY GROUPID,GROUP_EDESC,BRAND_NAME,T.MU_CODE,IUS.MU_CODE,T.INDEX_MU_CODE) 
                            PIVOT 
                           (
                             SUM(CURRENT_STOCK)
                             FOR BRAND_NAME
                              --IN (SELECT WM_CONCAT(''''||  BRAND_NAME||'''') FROM (SELECT  DISTINCT BRAND_NAME FROM IP_ITEM_SPEC_SETUP WHERE COMPANY_CODE = '06' AND ITEM_CODE IN (SELECT ITEM_CODE FROM DIST_RESELLER_STOCK))  )
                             IN ('BO' BO,'RVG' RVG,'RV' RV,'WB' WB,'NB' NB,'RT' RT,'RR' RR,'FGS' FGS,'RVU' RVU,'MI' MI,'RSB' RSB,'CE' CE,'HB' HB,'GO' GO,'UG' UG,'BD' BD,'MS' MS,'GMR' GMR,'HA' HA)
                           )
                        ORDER BY GROUPID";
            var data = _objectEntity.SqlQuery<StockSummary>(Query).ToList();
            return data;

        }

        public List<StockSummary> GetAreaGroupSummary(ReportFiltersModel model, User userInfo, string groupid)
        {
            var filter = "";
            if (model.ItemBrandFilter.Count > 0)
                filter = $" AND A.SP_CODE  IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                filter = $" AND A.SP_CODE  IN ({userInfo.sp_codes})";

            var Query = $@"SELECT * FROM (SELECT AREA_CODE,AREA_NAME,BRAND_NAME, CASE T.MU_CODE
                    WHEN T.INDEX_MU_CODE THEN NVL(SUM(T.CURRENT_STOCK),0)
                    ELSE ROUND(NVL(SUM(T.CURRENT_STOCK/IUS.CONVERSION_FACTOR),0),2)
                END CURRENT_STOCK,(SELECT wm_concat(SA.CUSTOMER_EDESC) FROM DIST_DISTRIBUTOR_MASTER DIS,SA_CUSTOMER_SETUP SA
WHERE DIS.DISTRIBUTOR_CODE=SA.CUSTOMER_CODE
AND DIS.COMPANY_CODE=SA.COMPANY_CODE
AND SA.COMPANY_CODE='{userInfo.company_code}'
AND DIS.DELETED_FLAG='N' and DIS.ACTIVE='Y' AND DIS.AREA_CODE=T.AREA_CODE
GROUP BY DIS.AREA_CODE) DISTRIBUTOR_NAME
                 FROM (SELECT C.AREA_NAME,C.AREA_CODE, A.RESELLER_CODE, B.RESELLER_NAME, A.SP_CODE, D.BRAND_NAME, A.ITEM_CODE, A.CURRENT_STOCK,A.COMPANY_CODE, A.MU_CODE ,IP.INDEX_MU_CODE
FROM DIST_RESELLER_STOCK A, DIST_RESELLER_MASTER B , DIST_AREA_MASTER C, IP_ITEM_SPEC_SETUP D,IP_ITEM_MASTER_SETUP IP
WHERE TRUNC(A.CREATED_DATE) = (SELECT MAX(TRUNC(CREATED_DATE)) FROM DIST_RESELLER_STOCK
WHERE  RESELLER_CODE = A.RESELLER_CODE)
AND A.RESELLER_CODE = B.RESELLER_CODE
AND A.COMPANY_CODE = B.COMPANY_CODE
AND B.AREA_CODE = C.AREA_CODE
AND B.COMPANY_CODE = C.COMPANY_CODE
AND A.ITEM_CODE = D.ITEM_CODE
AND A.COMPANY_CODE = D.COMPANY_CODE
AND B.GROUPID=C.GROUPID
     AND A.ITEM_CODE=IP.ITEM_CODE
                            AND A.COMPANY_CODE=IP.COMPANY_CODE
AND B.GROUPID='{groupid}'
{filter}
AND TRUNC(A.CREATED_DATE)<=TO_DATE('{model.FromDate}','YYYY-MM-DD') 
AND A.CURRENT_STOCK > 0
) T  LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON T.ITEM_CODE = IUS.ITEM_CODE AND T.COMPANY_CODE = IUS.COMPANY_CODE GROUP BY AREA_NAME,AREA_CODE,BRAND_NAME,T.MU_CODE,IUS.MU_CODE,T.INDEX_MU_CODE)
PIVOT 
(
  SUM(CURRENT_STOCK)
  FOR BRAND_NAME
  --IN (SELECT WM_CONCAT(''''||  BRAND_NAME||'''') FROM (SELECT  DISTINCT BRAND_NAME FROM IP_ITEM_SPEC_SETUP WHERE COMPANY_CODE = '06' AND ITEM_CODE IN (SELECT ITEM_CODE FROM DIST_RESELLER_STOCK))  )
 IN ('BO' BO,'RVG' RVG,'RV' RV,'WB' WB,'NB' NB,'RT' RT,'RR' RR,'FGS' FGS,'RVU' RVU,'MI' MI,'RSB' RSB,'CE' CE,'HB' HB,'GO' GO,'UG' UG,'BD' BD,'MS' MS,'GMR' GMR,'HA' HA)
)
ORDER BY AREA_CODE";
            var data = _objectEntity.SqlQuery<StockSummary>(Query).ToList();
            return data;

        }

        public List<StockSummary> GetResellerCodeStock(ReportFiltersModel model, User userInfo, string AreaId)
        {
            var filter = "";
            if (model.ItemBrandFilter.Count > 0)
                filter = $" AND A.SP_CODE  IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                filter = $" AND A.SP_CODE  IN ({userInfo.sp_codes})";

            var Query = $@"SELECT * FROM (SELECT RESELLER_CODE,RESELLER_NAME,BRAND_NAME,CASE T.MU_CODE
                    WHEN T.INDEX_MU_CODE THEN NVL(SUM(T.CURRENT_STOCK),0)
                    ELSE round(NVL(SUM(T.CURRENT_STOCK/IUS.CONVERSION_FACTOR),0),2)
                END CURRENT_STOCK  FROM (SELECT C.AREA_NAME,C.AREA_CODE, A.RESELLER_CODE, B.RESELLER_NAME, A.SP_CODE, D.BRAND_NAME, A.ITEM_CODE, A.CURRENT_STOCK, A.MU_CODE,A.COMPANY_CODE ,IP.INDEX_MU_CODE
FROM DIST_RESELLER_STOCK A, DIST_RESELLER_MASTER B , DIST_AREA_MASTER C, IP_ITEM_SPEC_SETUP D,IP_ITEM_MASTER_SETUP IP
WHERE TRUNC(A.CREATED_DATE) = (SELECT MAX(TRUNC(CREATED_DATE)) FROM DIST_RESELLER_STOCK
WHERE  RESELLER_CODE = A.RESELLER_CODE)
AND A.RESELLER_CODE = B.RESELLER_CODE
AND A.COMPANY_CODE = B.COMPANY_CODE
AND B.AREA_CODE = C.AREA_CODE
AND B.COMPANY_CODE = C.COMPANY_CODE
AND A.ITEM_CODE = D.ITEM_CODE
AND A.COMPANY_CODE = D.COMPANY_CODE
AND B.AREA_CODE='{AreaId}'
     AND A.ITEM_CODE=IP.ITEM_CODE
                            AND A.COMPANY_CODE=IP.COMPANY_CODE
{filter}
AND TRUNC(A.CREATED_DATE)<=TO_DATE('{model.FromDate}','YYYY-MM-DD') 
AND A.CURRENT_STOCK > 0
) T  LEFT JOIN IP_ITEM_UNIT_SETUP IUS ON T.ITEM_CODE = IUS.ITEM_CODE AND T.COMPANY_CODE = IUS.COMPANY_CODE GROUP BY RESELLER_NAME,RESELLER_CODE,BRAND_NAME,T.MU_CODE,IUS.MU_CODE,T.INDEX_MU_CODE )
PIVOT 
(
  SUM(CURRENT_STOCK)
  FOR BRAND_NAME
  --IN (SELECT WM_CONCAT(''''||  BRAND_NAME||'''') FROM (SELECT  DISTINCT BRAND_NAME FROM IP_ITEM_SPEC_SETUP WHERE COMPANY_CODE = '06' AND ITEM_CODE IN (SELECT ITEM_CODE FROM DIST_RESELLER_STOCK))  )
 IN ('BO' BO,'RVG' RVG,'RV' RV,'WB' WB,'NB' NB,'RT' RT,'RR' RR,'FGS' FGS,'RVU' RVU,'MI' MI,'RSB' RSB,'CE' CE,'HB' HB,'GO' GO,'UG' UG,'BD' BD,'MS' MS,'GMR' GMR,'HA' HA)
)
ORDER BY RESELLER_NAME";
            var data = _objectEntity.SqlQuery<StockSummary>(Query).ToList();
            return data;

        }

        public List<StockDetailReort> GetResellerCodeStockDetail(ReportFiltersModel model, User userInfo, string AreaId)
        {
            var filter = "";
            if (model.ItemBrandFilter.Count > 0)
                filter = $" AND A.SP_CODE  IN  ('{ string.Join("','", model.ItemBrandFilter).ToString()}')";
            else if (!string.IsNullOrWhiteSpace(userInfo.sp_codes))
                filter = $" AND A.SP_CODE  IN ({userInfo.sp_codes})";

            var Query = $@"SELECT F.FULL_NAME,trunc(A.CREATED_DATE) CREATED_DATE, A.RESELLER_CODE, B.RESELLER_NAME, A.SP_CODE, D.BRAND_NAME, A.ITEM_CODE, A.CURRENT_STOCK, A.MU_CODE,E.ITEM_EDESC 
FROM DIST_RESELLER_STOCK A, DIST_RESELLER_MASTER B , DIST_AREA_MASTER C, IP_ITEM_SPEC_SETUP D,IP_ITEM_MASTER_SETUP E,DIST_LOGIN_USER F
WHERE TRUNC(A.CREATED_DATE) = (SELECT MAX(TRUNC(CREATED_DATE)) FROM DIST_RESELLER_STOCK
WHERE  RESELLER_CODE = A.RESELLER_CODE)
AND A.RESELLER_CODE = B.RESELLER_CODE
AND A.COMPANY_CODE = B.COMPANY_CODE
AND B.AREA_CODE = C.AREA_CODE
AND B.COMPANY_CODE = C.COMPANY_CODE
AND A.ITEM_CODE = D.ITEM_CODE
AND A.COMPANY_CODE = D.COMPANY_CODE
AND A.ITEM_CODE=E.ITEM_CODE
AND D.ITEM_CODE=E.ITEM_CODE
AND A.COMPANY_CODE=E.COMPANY_CODE
AND D.COMPANY_CODE=E.COMPANY_CODE
AND A.SP_CODE=F.SP_CODE
AND B.AREA_CODE='{AreaId}'
{filter}
AND TRUNC(A.CREATED_DATE)<=TO_DATE('{model.FromDate}','YYYY-MM-DD') 
AND A.CURRENT_STOCK > 0";
            var data = _objectEntity.SqlQuery<StockDetailReort>(Query).ToList();
            return data;

        }

        public List<AttendanceModel> GetEmployeeRoute(ReportFiltersModel model, User userInfo, string sp_code)
        {


            var Query = $@"select DISTINCT ROUTE_NAME FILENAME,B.EMAIL,B.CONTACT_NO,a.assign_date as ATT_DATE from DIST_TARGET_ENTITY A ,DIST_LOGIN_USER B where
                A.SP_CODE=B.SP_CODE 
                AND  A.sp_code='{sp_code}' and
                TRUNC(A.assign_date) BETWEEN TO_DATE('{model.FromDate}','YYYY-MM-DD') AND TO_DATE('{model.FromDate}','YYYY-MM-DD')";
            var data = _objectEntity.SqlQuery<AttendanceModel>(Query).ToList();
            return data;

        }

        public List<SchemeModel> GetSchemeName(User userInfo)
        {

            var schemeQuery = $@"select SCHEME_ID as SchemeID, SCHEME_NAME as SchemeName from DIST_SCHEME where COMPANY_CODE='{userInfo.company_code}' AND BRANCH_CODE='{userInfo.branch_code}' and DELETED_FLAG='N' and APPROVE_FLAG='Y' ";
            var data = _objectEntity.SqlQuery<SchemeModel>(schemeQuery).ToList();
            return data;
        }
        
        
        public List<SchemeReportModel> GetSchemeSalesPersonList(User userInfo, string SchemeID)
        {

            var schemeQuery = $@"select  SCHEME_NAME as SchemeName, START_DATE as StartDate, END_DATE as EndDate, SP_CODE from DIST_SCHEME where COMPANY_CODE='{userInfo.company_code}' AND BRANCH_CODE='{userInfo.branch_code}' and DELETED_FLAG='N' and SCHEME_ID={SchemeID} ";
            var schemedata = _objectEntity.SqlQuery<SchemeModel>(schemeQuery).ToList();

            List<SchemeReportModel> modelst = new List<SchemeReportModel>();
            if (schemedata.Count != 0)
            {

                var schemeDetailQuery = $@"select MAX_VALUE, MIN_VALUE, DISCOUNT, DISCOUNT_TYPE as DiscountType, GIFT_QTY as GiftQty  from DIST_SCHEME_RULE_MAPPING where SCHEME_ID={SchemeID}";
                var schemeDetails = _objectEntity.SqlQuery<SchemeDetailModel>(schemeDetailQuery).ToList();

                string fromDate = schemedata[0].StartDate.ToString("yyyy-MMM-dd");
                string toDate = schemedata[0].EndDate.ToString("yyyy-MMM-dd");
                string query = $@"SELECT * FROM (
                                SELECT DPO1.ORDER_NO, DPO1.ORDER_DATE,BS_DATE(TO_CHAR(DPO1.ORDER_DATE)) MITI, DPO1.CUSTOMER_CODE, CS.CUSTOMER_EDESC, '' RESELLER_NAME, 'D' ORDER_ENTITY, TRIM(IMS.ITEM_EDESC) ITEM_EDESC, 
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
                                INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DPO1.ITEM_CODE AND IMS.COMPANY_CODE = DPO1.COMPANY_CODE AND IMS.CATEGORY_CODE in (select CATEGORY_CODE  from IP_CATEGORY_CODE WHERE CATEGORY_TYPE IN ('FG','TF') and company_code='{userInfo.company_code}') AND IMS.GROUP_SKU_FLAG = 'I'
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
                                      AND TRUNC(DPO1.ORDER_DATE) BETWEEN TO_DATE('{fromDate}','YYYY-MON-DD') AND TO_DATE('{toDate}','YYYY-MON-DD')
                                      AND DPO1.DELETED_FLAG = 'N'
                                     AND DPO1.REJECT_FLAG = 'N'
                                       AND DPO1.APPROVED_FLAG = 'N' 
                                    AND DPO1.CUSTOMER_CODE IN (select ENTITY_CODE from DIST_SCHEME_ENTITY_MAPPING where ENTITY_TYPE='D' and SCHEME_ID={SchemeID})
                                    AND DPO1.ITEM_CODE IN (SELECT ITEM_CODE FROM DIST_SCHEME_ITEMS WHERE  SCHEME_ID={SchemeID})
                                      AND DPO1.COMPANY_CODE IN ('{userInfo.company_code}')
                                       GROUP BY DPO1.ORDER_NO, DPO1.ORDER_DATE,BS_DATE(TO_CHAR(DPO1.ORDER_DATE)), DPO1.CUSTOMER_CODE,  CS.CUSTOMER_EDESC, '', TRIM(IMS.ITEM_EDESC), 
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
                                       DPO2.TOTAL_APPROVE_AMT
                            union all SELECT DPO1.ORDER_NO,DPO1.ORDER_DATE,BS_DATE(TO_CHAR(DPO1.ORDER_DATE)), DPO1.CUSTOMER_CODE,CS.CUSTOMER_EDESC, RM.RESELLER_NAME, 'R' ORDER_ENTITY,TRIM(IMS.ITEM_EDESC) ITEM_EDESC,
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
                            INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DPO1.ITEM_CODE AND IMS.COMPANY_CODE = DPO1.COMPANY_CODE AND IMS.CATEGORY_CODE in(select CATEGORY_CODE  from IP_CATEGORY_CODE WHERE CATEGORY_TYPE IN ('FG','TF') and company_code='{userInfo.company_code}') AND IMS.GROUP_SKU_FLAG = 'I'
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
                              AND TRUNC(DPO1.ORDER_DATE) >= TO_DATE('{fromDate}','YYYY-MM-DD') AND TRUNC(DPO1.ORDER_DATE) <= TO_DATE('{toDate}','YYYY-MM-DD')
                              AND DPO1.DELETED_FLAG = 'N'  
                              AND DPO1.RESELLER_CODE IN (select ENTITY_CODE from DIST_SCHEME_ENTITY_MAPPING where ENTITY_TYPE='R' and SCHEME_ID={SchemeID})
                               AND DPO1.ITEM_CODE IN (SELECT ITEM_CODE FROM DIST_SCHEME_ITEMS WHERE  SCHEME_ID={SchemeID})
                              AND DPO1.COMPANY_CODE IN ('{userInfo.company_code}') 
                              ) ORDER BY EMPLOYEE_EDESC, ITEM_EDESC, ORDER_NO";
                var result = _objectEntity.SqlQuery<SalesPersonPoModel>(query).ToList();

                foreach (var dt in result)
                {
                    SchemeReportModel model = new SchemeReportModel();

                    model.SP_CODE = schemedata[0].SP_CODE;
                    model.MITI = dt.MITI;
                    model.CUSTOMER_EDESC = dt.CUSTOMER_EDESC;
                    model.CUSTOMER_CODE = dt.CUSTOMER_CODE;
                    model.RESELLER_NAME = dt.RESELLER_NAME;
                    model.EMPLOYEE_EDESC = dt.EMPLOYEE_EDESC;
                    model.ITEM_EDESC = dt.ITEM_EDESC;
                    model.UNIT_PRICE = dt.UNIT_PRICE;
                    model.QUANTITY = dt.QUANTITY;
                    model.NET_TOTAL = dt.NET_TOTAL;
                    model.ORDER_NO = dt.ORDER_NO;
                    model.ORDER_DATE = dt.ORDER_DATE;

                    for (int i = 0; i < schemeDetails.Count(); i++)
                    {
                        if (dt.QUANTITY >= int.Parse(schemeDetails[i].Min_Value) && dt.QUANTITY <= int.Parse(schemeDetails[i].Max_Value))
                        {
                            model.SchemeName = schemedata[0].SchemeName;
                            model.CurrentSlot = schemeDetails[i].Min_Value + "-" + schemeDetails[i].Max_Value;
                            model.SchemeDiscount_Qty = schemeDetails[i].Discount;
                            model.DiscountType = schemeDetails[i].DiscountType;
                            if (schemeDetails[i].DiscountType == "QTY")
                            {
                                model.ActualDiscount = (int.Parse(schemeDetails[i].Discount) * dt.UNIT_PRICE).ToString();
                            }
                            else if (schemeDetails[i].DiscountType == "AMT")
                            {
                                model.ActualDiscount = (dt.NET_TOTAL - int.Parse(schemeDetails[i].Discount)).ToString();
                            }
                            else if(schemeDetails[i].DiscountType == "PERCENT")
                            {
                                model.ActualDiscount = (dt.NET_TOTAL * (decimal.Parse(schemeDetails[i].Discount)/100)).ToString();
                            }
                            //if (dt.NET_TOTAL <= decimal.Parse(model.ActualDiscount))
                            //{
                            //    model.ActualAmount = "0";
                            //}
                            //else
                            //{
                            model.ActualAmount = (dt.NET_TOTAL - decimal.Parse(model.ActualDiscount)).ToString();
                            //}
                            if (schemeDetails.ElementAtOrDefault(i + 1) != null)
                            {
                                model.NextScheme = schemeDetails[i + 1].Min_Value + "-" + schemeDetails[i + 1].Max_Value;
                                model.NextSchemeTarget = schemeDetails[i + 1].Discount;
                                model.NextDiscount = (int.Parse(schemeDetails[i + 1].Min_Value) - dt.QUANTITY).ToString();
                                model.NextDiscountType = schemeDetails[i + 1].DiscountType;
                            }
                            else
                            {
                                model.NextScheme = "N/A";
                                model.NextSchemeTarget = "0";
                                model.NextDiscount = "0";
                                model.NextDiscountType = "N/A";

                            }
                            break;


                        }
                        else
                        {
                            model.SchemeName = "N/A";
                            model.CurrentSlot = "N/A";
                            model.SchemeDiscount_Qty = "0";
                            model.DiscountType = "N/A";
                            model.ActualDiscount = "0";
                            model.ActualAmount = "0";
                        }

                    }

                    modelst.Add(model);



                }
            }
            

            return modelst;
        }

        public List<SchemeModel> GetSchemeAndDetails(User userInfo)
        {
            var schemeQuery = $@"select  SCHEME_ID as SchemeID, SCHEME_NAME as SchemeName, START_DATE as StartDate, END_DATE as EndDate, SP_CODE from DIST_SCHEME where COMPANY_CODE='{userInfo.company_code}' AND BRANCH_CODE='{userInfo.branch_code}' and DELETED_FLAG='N' and APPROVE_FLAG='Y'";
            var schemedata = _objectEntity.SqlQuery<SchemeModel>(schemeQuery).ToList();


            foreach (var scheme in schemedata)
            {
                var schemeDetailQuery = $@"select RULE_ID, MAX_VALUE, MIN_VALUE, DISCOUNT, DISCOUNT_TYPE as DiscountType, GIFT_QTY as GiftQty  from DIST_SCHEME_RULE_MAPPING where SCHEME_ID={scheme.SchemeID}";
                scheme.SchemeDetails = _objectEntity.SqlQuery<SchemeDetailModel>(schemeDetailQuery).ToList();
            }
            return schemedata;
        }


        public List<SalesPersonPoModel> GetSchemeReport(User userInfo, string SchemeID, string MinVal, string MaxVal, string fromDate, string toDate)
        {
            
            fromDate = DateTime.Parse(fromDate).ToString("yyyy-MMM-dd");
            toDate = DateTime.Parse(toDate).ToString("yyyy-MMM-dd");


            string query = $@"SELECT * FROM (
                                SELECT DPO1.ORDER_NO, DPO1.ORDER_DATE,BS_DATE(TO_CHAR(DPO1.ORDER_DATE)) MITI, DPO1.CUSTOMER_CODE, CS.CUSTOMER_EDESC, '' RESELLER_NAME, 'D' ORDER_ENTITY, TRIM(IMS.ITEM_EDESC) ITEM_EDESC, 
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
                                INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DPO1.ITEM_CODE AND IMS.COMPANY_CODE = DPO1.COMPANY_CODE AND IMS.CATEGORY_CODE in (select CATEGORY_CODE  from IP_CATEGORY_CODE WHERE CATEGORY_TYPE IN ('FG','TF') and company_code='{userInfo.company_code}') AND IMS.GROUP_SKU_FLAG = 'I'
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
                                      AND TRUNC(DPO1.ORDER_DATE) BETWEEN TO_DATE('{fromDate}','YYYY-MON-DD') AND TO_DATE('{toDate}','YYYY-MON-DD')
                                      AND DPO1.DELETED_FLAG = 'N'
                                     AND DPO1.REJECT_FLAG = 'N'
                                       AND DPO1.APPROVED_FLAG = 'N' 
                                    AND DPO1.CUSTOMER_CODE IN (select ENTITY_CODE from DIST_SCHEME_ENTITY_MAPPING where ENTITY_TYPE='D' and SCHEME_ID={SchemeID})
                                    AND DPO1.ITEM_CODE IN (SELECT ITEM_CODE FROM DIST_SCHEME_ITEMS WHERE  SCHEME_ID={SchemeID})
                                      AND DPO1.COMPANY_CODE IN ('{userInfo.company_code}')
                                       GROUP BY DPO1.ORDER_NO, DPO1.ORDER_DATE,BS_DATE(TO_CHAR(DPO1.ORDER_DATE)), DPO1.CUSTOMER_CODE,  CS.CUSTOMER_EDESC, '', TRIM(IMS.ITEM_EDESC), 
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
                                       DPO2.TOTAL_APPROVE_AMT
                            union all SELECT DPO1.ORDER_NO,DPO1.ORDER_DATE,BS_DATE(TO_CHAR(DPO1.ORDER_DATE)), DPO1.CUSTOMER_CODE,CS.CUSTOMER_EDESC, RM.RESELLER_NAME, 'R' ORDER_ENTITY,TRIM(IMS.ITEM_EDESC) ITEM_EDESC,
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
                            INNER JOIN IP_ITEM_MASTER_SETUP IMS ON IMS.ITEM_CODE = DPO1.ITEM_CODE AND IMS.COMPANY_CODE = DPO1.COMPANY_CODE AND IMS.CATEGORY_CODE in(select CATEGORY_CODE  from IP_CATEGORY_CODE WHERE CATEGORY_TYPE IN ('FG','TF') and company_code='{userInfo.company_code}') AND IMS.GROUP_SKU_FLAG = 'I'
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
                              AND TRUNC(DPO1.ORDER_DATE) >= TO_DATE('{fromDate}','YYYY-MM-DD') AND TRUNC(DPO1.ORDER_DATE) <= TO_DATE('{toDate}','YYYY-MM-DD')
                              AND DPO1.DELETED_FLAG = 'N'  
                              AND DPO1.RESELLER_CODE IN (select ENTITY_CODE from DIST_SCHEME_ENTITY_MAPPING where ENTITY_TYPE='R' and SCHEME_ID={SchemeID})
                               AND DPO1.ITEM_CODE IN (SELECT ITEM_CODE FROM DIST_SCHEME_ITEMS WHERE  SCHEME_ID={SchemeID})
                              AND DPO1.COMPANY_CODE IN ('{userInfo.company_code}') 
                              ) WHERE QUANTITY >= {MinVal} AND QUANTITY <= {MaxVal} ORDER BY EMPLOYEE_EDESC, ITEM_EDESC, ORDER_NO";
            var result = _objectEntity.SqlQuery<SalesPersonPoModel>(query).ToList();
            return result;
        }




    }

}