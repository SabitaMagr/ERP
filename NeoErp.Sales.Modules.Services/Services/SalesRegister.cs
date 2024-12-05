using AutoMapper;
using NeoErp.Core;
using NeoErp.Core.Caching;
using NeoErp.Core.Domain;
using NeoErp.Core.Helpers;
using NeoErp.Core.Models;
using NeoErp.Core.Models.CustomModels.SettingsEntities;
using NeoErp.Core.MongoDBRepository.Repository;
using NeoErp.Core.Services.CommonSetting;
using NeoErp.Sales.Modules.Services.Models;
using NeoErp.Sales.Modules.Services.Models.AgeingReport;
using NeoErp.Sales.Modules.Services.Models.Settings;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using  System.Web.Http;
using System.Net.Http.Formatting;

namespace NeoErp.Sales.Modules.Services.Services
{
    public class SalesRegister : ReportJsonFilterAbstract, ISalesRegister
    {

        private NeoErpCoreEntity _objectEntity;
        private IWorkContext _workContext;
        private ISettingService _setting;
        private readonly IMapper _mapper;
        private ICacheManager _cacheManager;
        public SalesRegister(NeoErpCoreEntity objectEntity, IWorkContext workContext, ISettingService service, IMapper mapper,ICacheManager cacheManager)
        {
            this._objectEntity = objectEntity;
            this._workContext = workContext;
            this._setting = service;
            this._mapper = mapper;
            this._cacheManager = cacheManager;
        }

        /// <summary>
        /// For Mobile Api
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public List<SalesRegisterCustomerModel> SaleRegisterCustomers(string companyCode, string branchCode)
        {
            string query = $@"SELECT DISTINCT INITCAP(CS.CUSTOMER_EDESC)AS CustomerName, CS.CUSTOMER_CODE AS CustomerCode,
            CS.GROUP_SKU_FLAG, CS.MASTER_CUSTOMER_CODE, CS.PRE_CUSTOMER_CODE
            FROM SA_CUSTOMER_SETUP CS
            WHERE CS.DELETED_FLAG = 'N'  
            AND CS.GROUP_SKU_FLAG= 'I'   
            AND CS.COMPANY_CODE IN ({companyCode})";

            if (!string.IsNullOrEmpty(branchCode))
            {
                query += $@" AND BRANCH_CODE IN ({branchCode})";
            }
            query += $@" ORDER BY CS.CUSTOMER_EDESC, MASTER_CUSTOMER_CODE, PRE_CUSTOMER_CODE";

            var salesRegisterCustomers = _objectEntity.SqlQuery<SalesRegisterCustomerModel>(query).ToList();
            return salesRegisterCustomers;
        }

        public List<SalesRegisterCustomerModel> SaleRegisterSuppliers(string companyCode, string branchCode)
        {
            string query = $@"SELECT DISTINCT INITCAP(SUPPLIER_EDESC)AS CUSTOMERNAME, SUPPLIER_CODE AS CUSTOMERCODE,
                GROUP_SKU_FLAG,PRE_SUPPLIER_CODE,MASTER_SUPPLIER_CODE
            FROM IP_SUPPLIER_SETUP
            WHERE DELETED_FLAG = 'N'  
            AND GROUP_SKU_FLAG= 'I'   
            AND COMPANY_CODE IN ('{companyCode}')";

            if (!string.IsNullOrEmpty(branchCode))
            {
                query += $@" AND BRANCH_CODE IN ({branchCode})";
            }
            query += $@" ORDER BY SUPPLIER_EDESC, MASTER_SUPPLIER_CODE, PRE_SUPPLIER_CODE";

            var salesRegisterCustomers = _objectEntity.SqlQuery<SalesRegisterCustomerModel>(query).ToList();
            return salesRegisterCustomers;
        }

        public List<SalesRegisterCustomerModel> SaleRegisterCustomers()
        {

            var companyCode = _workContext.CurrentUserinformation.company_code;
            string query = $@"SELECT DISTINCT INITCAP(CS.CUSTOMER_EDESC)AS CustomerName, CS.CUSTOMER_CODE AS CustomerCode,
            CS.GROUP_SKU_FLAG, CS.MASTER_CUSTOMER_CODE, CS.PRE_CUSTOMER_CODE
            FROM SA_CUSTOMER_SETUP CS
            WHERE CS.DELETED_FLAG = 'N'            
            AND CS.COMPANY_CODE = '{companyCode}'
            ORDER BY MASTER_CUSTOMER_CODE, PRE_CUSTOMER_CODE";
            var salesRegisterCustomers = _objectEntity.SqlQuery<SalesRegisterCustomerModel>(query).ToList();
            return salesRegisterCustomers;
        }

        public List<SalesRegisterCustomerModel> SaleRegisterGroupCustomers()
        {
            string query = @"SELECT DISTINCT INITCAP(CS.CUSTOMER_EDESC)AS CustomerName, CS.CUSTOMER_CODE AS CustomerCode,
            CS.GROUP_SKU_FLAG, CS.MASTER_CUSTOMER_CODE, CS.PRE_CUSTOMER_CODE
            FROM SA_CUSTOMER_SETUP CS
            WHERE CS.DELETED_FLAG = 'N'
            ORDER BY MASTER_CUSTOMER_CODE, PRE_CUSTOMER_CODE";
            var salesRegisterCustomers = _objectEntity.SqlQuery<SalesRegisterCustomerModel>(query).ToList();
            return salesRegisterCustomers;
        }

        public List<SalesRegisterProductModel> SalesRegisterProductsIndividual()
        {
            string query = @"SELECT DISTINCT LEVEL, 
            INITCAP(ITEM_EDESC) AS ItemName,
            ITEM_CODE AS ItemCode,
            MASTER_ITEM_CODE AS MasterItemCode, 
            PRE_ITEM_CODE AS PreItemCode, 
            GROUP_SKU_FLAG AS GroupFlag
            FROM IP_ITEM_MASTER_SETUP ims
            WHERE ims.DELETED_FLAG = 'N' 
            AND GROUP_SKU_FLAG = 'I'      
            START WITH PRE_ITEM_CODE = '00'
            CONNECT BY PRIOR MASTER_ITEM_CODE = PRE_ITEM_CODE";
            var productListNodes = _objectEntity.SqlQuery<SalesRegisterProductModel>(query).ToList();
            return productListNodes;
        }

        //       public List<SalesRegisterProductModel> GetDistributorItems(User userInfo)
        //       {
        //           //CheckFlag 
        //           string flagQuery = $@"SELECT PO_DISPLAY_DIST_ITEM FROM DIST_PREFERENCE_SETUP WHERE COMPANY_CODE ='{userInfo.company_code}'";
        //           var result = _objectEntity.SqlQuery<SalesRegisterProductModel>(flagQuery).ToList();
        //           //return result;
        //           var itemflag = result[0].PO_DISPLAY_DIST_ITEM;

        //           if (itemflag == "Y")
        //           {
        //               string query = $@"SELECT DISTINCT IT.ITEM_EDESC AS ItemName, IT.ITEM_CODE AS ItemCode
        // FROM IP_ITEM_MASTER_SETUP IT, DIST_DISTRIBUTOR_ITEM DD
        //WHERE DD.ITEM_CODE = IT.ITEM_CODE AND DD.COMPANY_CODE = IT.COMPANY_CODE ORDER BY IT.ITEM_EDESC ASC";
        //               var product = _objectEntity.SqlQuery<SalesRegisterProductModel>(query).ToList();
        //               return product;
        //           }
        //           else
        //           {

        //               string query = @"SELECT DISTINCT LEVEL, 
        //           INITCAP(ITEM_EDESC) AS ItemName,
        //           ITEM_CODE AS ItemCode,
        //           MASTER_ITEM_CODE AS MasterItemCode, 
        //           PRE_ITEM_CODE AS PreItemCode, 
        //           GROUP_SKU_FLAG AS GroupFlag
        //           FROM IP_ITEM_MASTER_SETUP ims
        //           WHERE ims.DELETED_FLAG = 'N' 
        //           AND GROUP_SKU_FLAG = 'I'      
        //           START WITH PRE_ITEM_CODE = '00'
        //           CONNECT BY PRIOR MASTER_ITEM_CODE = PRE_ITEM_CODE ORDER BY ItemName ASC";

        //               var product = _objectEntity.SqlQuery<SalesRegisterProductModel>(query).ToList();
        //               return product;
        //           }


        //       }

        public List<SalesRegisterProductModel> SalesRegisterProducts()
        {
            string query = @"SELECT DISTINCT LEVEL, 
            INITCAP(ITEM_EDESC) AS ItemName,
            ITEM_CODE AS ItemCode,
            MASTER_ITEM_CODE AS MasterItemCode, 
            PRE_ITEM_CODE AS PreItemCode, 
            GROUP_SKU_FLAG AS GroupFlag
            FROM IP_ITEM_MASTER_SETUP ims
            WHERE ims.DELETED_FLAG = 'N'             
            START WITH PRE_ITEM_CODE = '00'
            CONNECT BY PRIOR MASTER_ITEM_CODE = PRE_ITEM_CODE";
            var productListNodes = _objectEntity.SqlQuery<SalesRegisterProductModel>(query).ToList();
            return productListNodes;
        }

        public List<SalesRegisterProductModel> SalesRegisterProducts(User userinfo)
        {

            string query = $@"SELECT DISTINCT 
            --LEVEL, 
            INITCAP(ITEM_EDESC) AS ItemName,
            ITEM_CODE AS ItemCode,
            MASTER_ITEM_CODE AS MasterItemCode, 
            PRE_ITEM_CODE AS PreItemCode, 
            GROUP_SKU_FLAG AS GroupFlag,
             CATEGORY_CODE
            FROM IP_ITEM_MASTER_SETUP ims
            WHERE ims.DELETED_FLAG = 'N' 
            AND ims.COMPANY_CODE = '{userinfo.company_code}'           
            --START WITH PRE_ITEM_CODE = '00'
            --CONNECT BY PRIOR MASTER_ITEM_CODE = PRE_ITEM_CODE";
            var productListNodes = _objectEntity.SqlQuery<SalesRegisterProductModel>(query).ToList();
            return productListNodes;
        }
        public List<SalesRegisterProductModel> SalesRegisterProductsByCategory(User userinfo, string category)
        {
            if (userinfo == null)
            {
                userinfo = new Core.Domain.User();
                userinfo.company_code = "01";
                userinfo.branch_code = "01.01";

            }
            else if (string.IsNullOrEmpty(userinfo.company_code))
            {
                userinfo.company_code = "01";
                userinfo.branch_code = "01.01";
            }
            else if (string.IsNullOrEmpty(userinfo.branch_code))
            {
                userinfo.company_code = "01";
                userinfo.branch_code = "01.01";
            }
            string query = @"SELECT LEVEL, 
            INITCAP(ITEM_EDESC) AS ItemName,
            ITEM_CODE AS ItemCode,
            MASTER_ITEM_CODE AS MasterItemCode, 
            PRE_ITEM_CODE AS PreItemCode, 
            GROUP_SKU_FLAG AS GroupFlag
            FROM IP_ITEM_MASTER_SETUP ims
            WHERE ims.DELETED_FLAG = 'N' 
            AND CATEGORY_CODE = '" + category + @"'
            AND ims.COMPANY_CODE = '" + userinfo.company_code + @"'
            AND ims.Branch_Code = '" + userinfo.branch_code + @"'
            --AND GROUP_SKU_FLAG = 'G'
            START WITH PRE_ITEM_CODE = '00'
            CONNECT BY PRIOR MASTER_ITEM_CODE = PRE_ITEM_CODE";
            var productListNodes = _objectEntity.SqlQuery<SalesRegisterProductModel>(query).ToList();
            return productListNodes;
        }

        public List<CategoryModel> GetSalesRegisterItemCategory()
        {
            string query = @"SELECT DISTINCT CATEGORY_CODE AS CategoryCode, 
                                CATEGORY_EDESC AS CategoryName
                                FROM IP_CATEGORY_CODE
                                WHERE DELETED_FLAG = 'N'";
            var categoryList = _objectEntity.SqlQuery<CategoryModel>(query).ToList();
            return categoryList;
        }
        public List<CategoryModel> GetSalesRegisterItemCategory(User userinfo)
        {

            string query = @"SELECT CATEGORY_CODE AS CategoryCode, 
                                CATEGORY_EDESC AS CategoryName
                                FROM IP_CATEGORY_CODE
                                WHERE DELETED_FLAG = 'N'
                                AND COMPANY_CODE = '" + userinfo.company_code + "' ";
            var categoryList = _objectEntity.SqlQuery<CategoryModel>(query).ToList();
            return categoryList;
        }

        public List<PartyTypeModel> GetSalesRegisterPartyTypes()
        {
            string query = @"SELECT DISTINCT PARTY_TYPE_CODE AS PartyTypeCode,
                                 PARTY_TYPE_EDESC AS PartyTypeName
                                 FROM IP_PARTY_TYPE_CODE
                                 WHERE DELETED_FLAG = 'N' and Party_type_flag='D'";
            var partyTypeList = _objectEntity.SqlQuery<PartyTypeModel>(query).ToList();
            return partyTypeList;
        }
        public List<PartyTypeModel> GetSalesRegisterPartyTypes(User userinfo)
        {
            string query = @"SELECT PARTY_TYPE_CODE AS PartyTypeCode,
                                 PARTY_TYPE_EDESC AS PartyTypeName
                                 FROM IP_PARTY_TYPE_CODE
                                 WHERE DELETED_FLAG = 'N'
                                 AND COMPANY_CODE = '" + userinfo.company_code + "' and Party_type_flag='D'";
            var partyTypeList = _objectEntity.SqlQuery<PartyTypeModel>(query).ToList();
            return partyTypeList;
        }

        public List<AreaTypeModel> GetAreaTypes(User userinfo)
        {
            string query = @"SELECT AREA_CODE,
                                 AREA_EDESC
                                 FROM AREA_SETUP
                                 WHERE DELETED_FLAG = 'N'
                                 AND COMPANY_CODE = '" + userinfo.company_code + "' ";
            var areaTypeList = _objectEntity.SqlQuery<AreaTypeModel>(query).ToList();
            return areaTypeList;
        }

        //Query for the Branch 

        public List<BranchModel> getSalesRegisterBranch()
        {
            string query = @"SELECT BRANCH_CODE AS BranchCode ,
                             BRANCH_EDESC AS  BranchName
                             from FA_BRANCH_SETUP
                             WHERE COMPANY_CODE = '01'
                             AND DELETED_FLAG = 'N' ";
            var BranchList = _objectEntity.SqlQuery<BranchModel>(query).ToList();
            return BranchList;
        }
        public List<BranchModel> getSalesRegisterBranch(User userinfo)
        {
            string query = @"SELECT BRANCH_CODE AS BranchCode ,
                             BRANCH_EDESC AS  BranchName
                             from FA_BRANCH_SETUP
                             WHERE COMPANY_CODE = '" + userinfo.company_code + @"'
                             AND DELETED_FLAG = 'N' ";
            var BranchList = _objectEntity.SqlQuery<BranchModel>(query).ToList();
            return BranchList;
        }
        public List<VoucherModel> SalesRegisterVouchers()
        {
            var companyCode = this._workContext.CurrentUserinformation.company_code;
            string query = $@"SELECT 
                            DISTINCT FS.FORM_CODE VoucherCode, 
                            INITCAP(FS.FORM_EDESC) VoucherName
                            FROM FORM_DETAIL_SETUP DS, FORM_SETUP FS
                            WHERE table_name  IN ( 'SA_SALES_INVOICE', 'SA_SALES_RETURN')                           
                            AND FS.DELETED_FLAG = 'N'
                            AND FS.FORM_CODE = DS.FORM_CODE
                            AND FS.COMPANY_CODE = DS.COMPANY_CODE
                            AND FS.COMPANY_CODE = '{companyCode}'
                            ORDER BY INITCAP(FS.FORM_EDESC)";
            var voucherList = _objectEntity.SqlQuery<VoucherModel>(query).ToList();
            return voucherList;
        }

        public List<VoucherSetupModel> GetAllVoucherNodes()
        {
            var companyCode = this._workContext.CurrentUserinformation.company_code;
            string query = $@"SELECT DISTINCT LEVEL, COMPANY_CODE,DELETED_FLAG,
                                        INITCAP(FORM_NDESC) AS VoucherName,
                                        FORM_CODE AS VoucherCode,
                                        MASTER_FORM_CODE AS MasterFormCode, 
                                        PRE_FORM_CODE AS PreFormCode, 
                                        GROUP_SKU_FLAG AS GroupFlag
                                        FROM FORM_SETUP fs
                                        WHERE fs.DELETED_FLAG = 'N'                                         
                                        AND GROUP_SKU_FLAG = 'G' 
                                        AND LEVEL = 1 
                                        AND fs.COMPANY_CODE = '{companyCode}'
                                        START WITH PRE_FORM_CODE = '00'
                                        CONNECT BY PRIOR MASTER_FORM_CODE = PRE_FORM_CODE
                                        ORDER SIBLINGS BY FORM_NDESC";
            var VoucherListNodes = _objectEntity.SqlQuery<VoucherSetupModel>(query).ToList();
            return VoucherListNodes;
        }

        public List<VoucherSetupModel> GetVoucherListByFormCode(string level, string masterSupplierCode)
        {
            var companyCode = this._workContext.CurrentUserinformation.company_code;
            string query = string.Format(@"SELECT DISTINCT LEVEL, COMPANY_CODE,DELETED_FLAG,
                                        INITCAP(FORM_NDESC) AS VoucherName,
                                        FORM_CODE AS VoucherCode,
                                        MASTER_FORM_CODE AS MasterFormCode, 
                                        PRE_FORM_CODE AS PreFormCode, 
                                        GROUP_SKU_FLAG AS GroupFlag
                                        FROM FORM_SETUP fs
                                        WHERE fs.DELETED_FLAG = 'N'    
                                        AND fs.COMPANY_CODE = '" + companyCode + @"'                                     
                                        --AND GROUP_SKU_FLAG = 'G'
                                        AND LEVEL = {0} 
                                       -- AND fs.FORM_CODE IN 
                                       --   (
                                       --    SELECT DISTINCT FS.FORM_CODE
                                       --       FROM form_detail_setup DS, FORM_SETUP FS
                                       --      WHERE  FS.COMPANY_CODE = '01'
                                       --      AND FS.DELETED_FLAG = 'N'
                                       --      AND table_name IN  ( 'SA_SALES_INVOICE','SA_SALES_RETURN')
                                       --      AND FS.FORM_CODE = DS.FORM_CODE
                                       --      AND FS.COMPANY_CODE = DS.COMPANY_CODE
                                       --   ) 
                                        START WITH PRE_FORM_CODE = {1}
                                        CONNECT BY PRIOR MASTER_FORM_CODE = PRE_FORM_CODE
                                        ORDER SIBLINGS BY FORM_NDESC", level.ToString(), masterSupplierCode.ToString());
            var voucherListNodes = _objectEntity.SqlQuery<VoucherSetupModel>(query).ToList();
            return voucherListNodes;
        }

        public List<SupplierSetupModel> SupplierAllNodes()
        {
            string query = @"SELECT DISTINCT LEVEL, COMPANY_CODE,DELETED_FLAG,
                             INITCAP(SUPPLIER_EDESC) AS SupplierName,
                             SUPPLIER_CODE AS SupplierCode,
                             MASTER_SUPPLIER_CODE AS MasterSupplierCode, 
                             PRE_SUPPLIER_CODE AS PreSupplierCode, 
                             GROUP_SKU_FLAG AS GroupFlag
                             FROM IP_SUPPLIER_SETUP ims
                             WHERE ims.DELETED_FLAG = 'N'                              
                             AND GROUP_SKU_FLAG = 'G'              
                             START WITH PRE_SUPPLIER_CODE = '00'
                             CONNECT BY PRIOR MASTER_SUPPLIER_CODE = PRE_SUPPLIER_CODE";
            var SupplierListNodes = _objectEntity.SqlQuery<SupplierSetupModel>(query).ToList();
            return SupplierListNodes;
        }

        public List<SupplierSetupModel> SupplierAllNodes(User userinfo)
        {
            string query = @"SELECT LEVEL, COMPANY_CODE,DELETED_FLAG,
                             INITCAP(SUPPLIER_EDESC) AS SupplierName,
                             SUPPLIER_CODE AS SupplierCode,
                             MASTER_SUPPLIER_CODE AS MasterSupplierCode, 
                             PRE_SUPPLIER_CODE AS PreSupplierCode, 
                             GROUP_SKU_FLAG AS GroupFlag
                             FROM IP_SUPPLIER_SETUP ims
                             WHERE ims.DELETED_FLAG = 'N' 
                             AND ims.COMPANY_CODE = '" + userinfo.company_code + @"'                             
                             AND GROUP_SKU_FLAG = 'G'              
                             START WITH PRE_SUPPLIER_CODE = '00'
                             CONNECT BY PRIOR MASTER_SUPPLIER_CODE = PRE_SUPPLIER_CODE";
            var SupplierListNodes = _objectEntity.SqlQuery<SupplierSetupModel>(query).ToList();
            return SupplierListNodes;
        }
        public List<SupplierSetupModel> DealerAllNodes(User userinfo)
        {
            //var query = @"SELECT LEVEL, COMPANY_CODE,DELETED_FLAG,
            //                INITCAP(PARTY_TYPE_EDESC) AS SupplierName,
            //                PARTY_TYPE_CODE AS SupplierCode,
            //                MASTER_PARTY_CODE AS MasterSupplierCode, 
            //                PRE_PARTY_CODE AS PreSupplierCode, 
            //                GROUP_SKU_FLAG AS GroupFlag
            //                FROM IP_PARTY_TYPE_CODE fs
            //                WHERE fs.DELETED_FLAG = 'N' 
            //                AND fs.COMPANY_CODE = '" + userinfo.company_code + @"'  
            //                --AND GROUP_SKU_FLAG = 'G'              
            //                START WITH PRE_PARTY_CODE = '00'
            //                CONNECT BY PRIOR MASTER_PARTY_CODE = PRE_PARTY_CODE";

            //var SupplierListNodes = _objectEntity.SqlQuery<SupplierSetupModel>(query).ToList();
            //return SupplierListNodes;
            if (userinfo == null)
            {
                userinfo = new Core.Domain.User();
                userinfo.company_code = "01";
                userinfo.branch_code = "01.01";

            }
            else if (string.IsNullOrEmpty(userinfo.company_code))
            {
                userinfo.company_code = "01";
                userinfo.branch_code = "01.01";
            }
            string query = @"SELECT LEVEL, COMPANY_CODE,DELETED_FLAG,
                             INITCAP(PARTY_TYPE_EDESC) AS SupplierName,
                             PARTY_TYPE_CODE AS SupplierCode,
                             MASTER_PARTY_CODE AS MasterSupplierCode, 
                             PRE_PARTY_CODE AS PreSupplierCode, 
                             GROUP_SKU_FLAG AS GroupFlag,
                             (SELECT COUNT(*) FROM IP_PARTY_TYPE_CODE WHERE  PRE_PARTY_CODE = IMS.MASTER_PARTY_CODE) as Childrens
                             FROM IP_PARTY_TYPE_CODE ims
                             WHERE ims.DELETED_FLAG = 'N' 
                             AND ims.COMPANY_CODE = '" + userinfo.company_code + @"'
                            -- AND ims.branch_code = '" + userinfo.branch_code + @"'
                             AND GROUP_SKU_FLAG = 'G'
                             AND LEVEL='1'           
                             START WITH PRE_PARTY_CODE = '00'
                             CONNECT BY PRIOR MASTER_PARTY_CODE = PRE_PARTY_CODE";
            var SupplierListNodes = _objectEntity.SqlQuery<SupplierSetupModel>(query).ToList();
            return SupplierListNodes;
        }

        public List<SupplierSetupModel> SupplierAllNodesGroup()
        {
            string query = @"SELECT SELECT LEVEL, COMPANY_CODE,DELETED_FLAG,
                             INITCAP(SUPPLIER_EDESC) AS SupplierName,
                             SUPPLIER_CODE AS SupplierCode,
                             MASTER_SUPPLIER_CODE AS MasterSupplierCode, 
                             PRE_SUPPLIER_CODE AS PreSupplierCode, 
                             GROUP_SKU_FLAG AS GroupFlag,
                             (SELECT COUNT(*) FROM IP_SUPPLIER_SETUP WHERE GROUP_SKU_FLAG='G' AND PRE_SUPPLIER_CODE = IMS.MASTER_SUPPLIER_CODE) as Childrens
                             FROM IP_SUPPLIER_SETUP ims
                             WHERE ims.DELETED_FLAG = 'N'                              
                             AND GROUP_SKU_FLAG = 'G'
                             AND LEVEL='1'           
                             START WITH PRE_SUPPLIER_CODE = '00'
                             CONNECT BY PRIOR MASTER_SUPPLIER_CODE = PRE_SUPPLIER_CODE";
            var SupplierListNodes = _objectEntity.SqlQuery<SupplierSetupModel>(query).ToList();
            return SupplierListNodes;
        }

        public List<SupplierSetupModel> SupplierAllNodesGroup(User userinfo)
        {
            if (userinfo == null)
            {
                userinfo = new Core.Domain.User();
                userinfo.company_code = "01";
                userinfo.branch_code = "01.01";

            }
            else if (string.IsNullOrEmpty(userinfo.company_code))
            {
                userinfo.company_code = "01";
                userinfo.branch_code = "01.01";
            }
            string query = @"SELECT LEVEL, COMPANY_CODE,DELETED_FLAG,
                             INITCAP(SUPPLIER_EDESC) AS SupplierName,
                             SUPPLIER_CODE AS SupplierCode,
                             MASTER_SUPPLIER_CODE AS MasterSupplierCode, 
                             PRE_SUPPLIER_CODE AS PreSupplierCode, 
                             GROUP_SKU_FLAG AS GroupFlag,
                             (SELECT COUNT(*) FROM IP_SUPPLIER_SETUP WHERE GROUP_SKU_FLAG='G' AND PRE_SUPPLIER_CODE = IMS.MASTER_SUPPLIER_CODE) as Childrens
                             FROM IP_SUPPLIER_SETUP ims
                             WHERE ims.DELETED_FLAG = 'N' 
                             AND ims.COMPANY_CODE = '" + userinfo.company_code + @"'
                             AND ims.branch_code = '" + userinfo.branch_code + @"'
                             AND GROUP_SKU_FLAG = 'G'
                             AND LEVEL='1'           
                             START WITH PRE_SUPPLIER_CODE = '00'
                             CONNECT BY PRIOR MASTER_SUPPLIER_CODE = PRE_SUPPLIER_CODE";
            var SupplierListNodes = _objectEntity.SqlQuery<SupplierSetupModel>(query).ToList();
            return SupplierListNodes;
        }

        public List<SalesRegisterSupplierModel> SalesRegisterSuppliers()
        {
            string query = @"SELECT DISTINCT LEVEL, COMPANY_CODE,DELETED_FLAG,
                            INITCAP(SUPPLIER_EDESC) AS SupplierName,
                            SUPPLIER_CODE AS SupplierCode,
                            MASTER_SUPPLIER_CODE AS MasterItemCode, 
                            PRE_SUPPLIER_CODE AS PreItemCode, 
                            GROUP_SKU_FLAG AS GroupFlag
                            FROM IP_SUPPLIER_SETUP fs
                            WHERE fs.DELETED_FLAG = 'N'                             
                            --AND GROUP_SKU_FLAG = 'G'              
                            START WITH PRE_SUPPLIER_CODE = '00'
                            CONNECT BY PRIOR MASTER_SUPPLIER_CODE = PRE_SUPPLIER_CODE";
            var SupplierListNodes = _objectEntity.SqlQuery<SalesRegisterSupplierModel>(query).ToList();
            return SupplierListNodes;
        }

        public List<SalesRegisterSupplierModel> SalesRegisterSuppliers(User userinfo)
        {

            string query = @"SELECT LEVEL, COMPANY_CODE,DELETED_FLAG,
                            INITCAP(SUPPLIER_EDESC) AS SupplierName,
                            SUPPLIER_CODE AS SupplierCode,
                            MASTER_SUPPLIER_CODE AS MasterSuplierCode, 
                            PRE_SUPPLIER_CODE AS PreSuplierCode, 
                            GROUP_SKU_FLAG AS GroupFlag
                            FROM IP_SUPPLIER_SETUP fs
                            WHERE fs.DELETED_FLAG = 'N' 
                            AND fs.COMPANY_CODE = '" + userinfo.company_code + @"'
                            --AND GROUP_SKU_FLAG = 'G'              
                            START WITH PRE_SUPPLIER_CODE = '00'
                            CONNECT BY PRIOR MASTER_SUPPLIER_CODE = PRE_SUPPLIER_CODE";
            var SupplierListNodes = _objectEntity.SqlQuery<SalesRegisterSupplierModel>(query).ToList();
            return SupplierListNodes;
        }
        public List<SalesRegisterSupplierModel> SalesRegisterDealer(User userinfo)
        {
            var query = @"SELECT DISTINCT LEVEL, COMPANY_CODE,DELETED_FLAG,
                            INITCAP(PARTY_TYPE_EDESC) AS SupplierName,
                            PARTY_TYPE_CODE AS SupplierCode,
                            MASTER_PARTY_CODE AS MasterSupplierCode, 
                            PRE_PARTY_CODE AS PreSupplierCode, 
                            GROUP_SKU_FLAG AS GroupFlag
                            FROM IP_PARTY_TYPE_CODE fs
                            WHERE fs.DELETED_FLAG = 'N' 
                            AND fs.COMPANY_CODE = '" + userinfo.company_code + @"'
                            --AND GROUP_SKU_FLAG = 'G'              
                            START WITH PRE_PARTY_CODE = '00'
                            CONNECT BY PRIOR MASTER_PARTY_CODE = PRE_PARTY_CODE";
            var SupplierListNodes = _objectEntity.SqlQuery<SalesRegisterSupplierModel>(query).ToList();
            return SupplierListNodes;
        }


        public List<SalesRegisterSupplierModel> SalesRegisterGroupSuppliers()
        {
            string query = @"SELECT LEVEL, COMPANY_CODE,DELETED_FLAG,
                            INITCAP(SUPPLIER_EDESC) AS SupplierName,
                            SUPPLIER_CODE AS SupplierCode,
                            MASTER_SUPPLIER_CODE AS MasterItemCode, 
                            PRE_SUPPLIER_CODE AS PreItemCode, 
                            GROUP_SKU_FLAG AS GroupFlag
                            FROM IP_SUPPLIER_SETUP fs
                            WHERE fs.DELETED_FLAG = 'N' 
                            AND fs.COMPANY_CODE = '01'
                            AND GROUP_SKU_FLAG = 'G'              
                            START WITH PRE_SUPPLIER_CODE = '00'
                            CONNECT BY PRIOR MASTER_SUPPLIER_CODE = PRE_SUPPLIER_CODE";
            var SupplierListNodes = _objectEntity.SqlQuery<SalesRegisterSupplierModel>(query).ToList();
            return SupplierListNodes;
        }

        public List<SalesRegisterSupplierModel> SalesRegisterGroupSuppliers(User userinfo)
        {
            if (userinfo == null)
            {
                userinfo = new Core.Domain.User();
                userinfo.company_code = "01";
                userinfo.branch_code = "01.01";

            }
            else if (string.IsNullOrEmpty(userinfo.company_code))
            {
                userinfo.company_code = "01";
                userinfo.branch_code = "01.01";
            }
            else if (string.IsNullOrEmpty(userinfo.branch_code))
            {
                userinfo.company_code = "01";
                userinfo.branch_code = "01.01";
            }
            string query = @"SELECT LEVEL, COMPANY_CODE,DELETED_FLAG,
                            INITCAP(SUPPLIER_EDESC) AS SupplierName,
                            SUPPLIER_CODE AS SupplierCode,
                            MASTER_SUPPLIER_CODE AS MasterItemCode, 
                            PRE_SUPPLIER_CODE AS PreItemCode, 
                            GROUP_SKU_FLAG AS GroupFlag
                            FROM IP_SUPPLIER_SETUP fs
                            WHERE fs.DELETED_FLAG = 'N' 
                            AND fs.COMPANY_CODE = '" + userinfo.company_code + @"'
                            AND GROUP_SKU_FLAG = 'G'              
                            START WITH PRE_SUPPLIER_CODE = '00'
                            CONNECT BY PRIOR MASTER_SUPPLIER_CODE = PRE_SUPPLIER_CODE";
            var SupplierListNodes = _objectEntity.SqlQuery<SalesRegisterSupplierModel>(query).ToList();
            return SupplierListNodes;
        }

        public List<CustomerSetupModel> GetGroupCustomerListByCustomerCode(string level, string masterCustomerCode)
        {
            string query = string.Format(@"SELECT DISTINCT LEVEL,INITCAP(CS.CUSTOMER_EDESC) CUSTOMER_EDESC,GROUP_SKU_FLAG,
            CUSTOMER_CODE, CS.MASTER_CUSTOMER_CODE, CS.PRE_CUSTOMER_CODE,
            (SELECT COUNT(*) FROM SA_CUSTOMER_SETUP WHERE GROUP_SKU_FLAG='G' AND PRE_CUSTOMER_CODE = CS.MASTER_CUSTOMER_CODE) as Childrens
            FROM SA_CUSTOMER_SETUP CS
            WHERE CS.DELETED_FLAG = 'N'
            AND GROUP_SKU_FLAG='G'            
            AND LEVEL = {0}
            START WITH PRE_CUSTOMER_CODE = '{1}'
            CONNECT BY PRIOR MASTER_CUSTOMER_CODE = PRE_CUSTOMER_CODE
            ORDER SIBLINGS BY CUSTOMER_EDESC", level.ToString(), masterCustomerCode.ToString());
            var customerListNodes = _objectEntity.SqlQuery<CustomerSetupModel>(query).ToList();
            return customerListNodes;
        }

        public List<CustomerSetupModel> GetCustomerListByCustomerCode(string level, string masterCustomerCode)
        {
            string query = string.Format(@"SELECT DISTINCT LEVEL,INITCAP(CS.CUSTOMER_EDESC) CUSTOMER_EDESC,GROUP_SKU_FLAG,
            CUSTOMER_CODE, CS.MASTER_CUSTOMER_CODE, CS.PRE_CUSTOMER_CODE
            FROM SA_CUSTOMER_SETUP CS
            WHERE CS.DELETED_FLAG = 'N'            
            AND LEVEL = {0}
            START WITH PRE_CUSTOMER_CODE = '{1}'
            CONNECT BY PRIOR MASTER_CUSTOMER_CODE = PRE_CUSTOMER_CODE
            ORDER SIBLINGS BY CUSTOMER_EDESC", level.ToString(), masterCustomerCode.ToString());
            var customerListNodes = _objectEntity.SqlQuery<CustomerSetupModel>(query).ToList();
            return customerListNodes;
        }

        public List<CustomerSetupModel> GetCustomerListByCustomerCode(string level, string masterCustomerCode, User userinfo)
        {

            string query = string.Format(@"SELECT LEVEL,INITCAP(CS.CUSTOMER_EDESC) CUSTOMER_EDESC,GROUP_SKU_FLAG,
            CUSTOMER_CODE, CS.MASTER_CUSTOMER_CODE, CS.PRE_CUSTOMER_CODE, CS.BRANCH_CODE,
            (SELECT COUNT(*) FROM SA_CUSTOMER_SETUP WHERE  GROUP_SKU_FLAG='G' AND COMPANY_CODE='" + userinfo.company_code + @"' AND DELETED_FLAG='N' AND PRE_CUSTOMER_CODE = CS.MASTER_CUSTOMER_CODE) as Childrens
            FROM SA_CUSTOMER_SETUP CS
            WHERE CS.DELETED_FLAG = 'N'
            AND CS.COMPANY_CODE = '{0}'
            AND LEVEL = {1}
            START WITH PRE_CUSTOMER_CODE = '{2}'
            CONNECT BY PRIOR MASTER_CUSTOMER_CODE = PRE_CUSTOMER_CODE
            ORDER SIBLINGS BY CUSTOMER_EDESC", userinfo.company_code, level.ToString(), masterCustomerCode.ToString());
            var customerListNodes = _objectEntity.SqlQuery<CustomerSetupModel>(query).ToList();
            return customerListNodes;
        }
        public List<CustomerSetupModel> CustomerListAllNodes()
        {
            string query = @"SELECT DISTINCT LEVEL,INITCAP(CS.CUSTOMER_EDESC) CUSTOMER_EDESC,CS.CUSTOMER_CODE,
            CS.GROUP_SKU_FLAG,CS.MASTER_CUSTOMER_CODE,CS.PRE_CUSTOMER_CODE, LEVEL
            FROM SA_CUSTOMER_SETUP CS
            WHERE CS.DELETED_FLAG = 'N'
            AND GROUP_SKU_FLAG = 'G'
            AND LEVEL = 1
            START WITH PRE_CUSTOMER_CODE = '00'
            CONNECT BY PRIOR MASTER_CUSTOMER_CODE = PRE_CUSTOMER_CODE
            ORDER SIBLINGS BY CUSTOMER_EDESC";
            var customerListNodes = _objectEntity.SqlQuery<CustomerSetupModel>(query).ToList();
            return customerListNodes;
        }
        public List<CustomerSetupModel> CustomerListAllNodes(User userinfo)
        {
            string query = @"SELECT LEVEL,INITCAP(CS.CUSTOMER_EDESC) CUSTOMER_EDESC,CS.CUSTOMER_CODE,
            CS.GROUP_SKU_FLAG,CS.MASTER_CUSTOMER_CODE,CS.PRE_CUSTOMER_CODE, CS.BRANCH_CODE, LEVEL,
            (SELECT COUNT(*) FROM SA_CUSTOMER_SETUP WHERE  GROUP_SKU_FLAG='G' AND COMPANY_CODE='" + userinfo.company_code + @"' AND DELETED_FLAG='N' AND PRE_CUSTOMER_CODE = CS.MASTER_CUSTOMER_CODE) as Childrens
            FROM SA_CUSTOMER_SETUP CS
            WHERE CS.DELETED_FLAG = 'N' AND CS.COMPANY_CODE = '" + userinfo.company_code + @"'
            AND GROUP_SKU_FLAG = 'G'
            AND LEVEL = 1
            START WITH PRE_CUSTOMER_CODE = '00'
            CONNECT BY PRIOR MASTER_CUSTOMER_CODE = PRE_CUSTOMER_CODE
            ORDER SIBLINGS BY CUSTOMER_EDESC";
            var customerListNodes = _objectEntity.SqlQuery<CustomerSetupModel>(query).ToList();
            return customerListNodes;
        }



        public List<ConsolidateTree> CompanyListAllNodes(User userinfo, string userNo = null)

        {


            // string query = $@"SELECT INITCAP(CC.COMPANY_EDESC) AS BRANCH_EDESC,CS.BRANCH_CODE,
            //CS.PRE_BRANCH_CODE,CC.ABBR_CODE
            // FROM FA_BRANCH_SETUP CS, COMPANY_SETUP CC
            // WHERE CS.BRANCH_CODE = CC.COMPANY_CODE AND CS.DELETED_FLAG = 'N'
            // AND GROUP_SKU_FLAG = 'G' AND CC.COMPANY_CODE IN (SELECT COMPANY_CODE FROM SC_COMPANY_CONTROL WHERE ACCESS_FLAG='Y' AND USER_NO='{userinfo.User_id}')";

            String query = $@"SELECT CC.COMPANY_CODE BRANCH_CODE, INITCAP(CC.COMPANY_EDESC) AS BRANCH_EDESC,'00' PRE_BRANCH_CODE,CC.ABBR_CODE
                            FROM COMPANY_SETUP CC
                            WHERE  CC.DELETED_FLAG = 'N'
                            AND (CC.COMPANY_CODE IN (SELECT COMPANY_CODE FROM SC_COMPANY_CONTROL WHERE ACCESS_FLAG='Y' AND USER_NO='{userinfo.User_id}')
                            OR  CC.COMPANY_CODE IN (SELECT COMPANY_CODE FROM COMPANY_SETUP WHERE 1 = (SELECT COUNT(DISTINCT USER_NO) FROM SC_APPLICATION_USERS WHERE USER_TYPE <> 'GENERAL' AND USER_NO = '{userinfo.User_id}' )))
                            UNION    
                            SELECT DISTINCT  CS.BRANCH_CODE, INITCAP(CS.BRANCH_EDESC) AS BRANCH_EDESC ,CS.PRE_BRANCH_CODE,CS.ABBR_CODE
                            FROM FA_BRANCH_SETUP CS   
                            WHERE  CS.DELETED_FLAG = 'N'  
                            AND CS.PRE_BRANCH_CODE <> '00'     
                            AND(CS.BRANCH_CODE IN(SELECT BRANCH_CODE FROM SC_BRANCH_CONTROL WHERE ACCESS_FLAG = 'Y' AND USER_NO = '{userinfo.User_id}' )
                            OR CS.BRANCH_CODE IN(SELECT BRANCH_CODE FROM FA_BRANCH_SETUP WHERE 1 = (SELECT COUNT(DISTINCT USER_NO) FROM SC_APPLICATION_USERS WHERE USER_TYPE <> 'GENERAL' AND USER_NO = '{userinfo.User_id}' )))";

            var consolidateListNodes = _objectEntity.SqlQuery<ConsolidateTree>(query).ToList();
            //var accessedData = new List<AccessedControl>();
            //if (userNo != null)
            //{
            //    string accessedQuery = $@"SELECT scc.COMPANY_CODE,sac.USER_NO,sbc.BRANCH_CODE FROM SC_COMPANY_CONTROL scc
            //                              INNER JOIN SC_APPLICATION_USERS sac on scc.USER_NO=sac.USER_NO 
            //                              INNER JOIN SC_BRANCH_CONTROL sbc on sbc.USER_NO = sac.USER_NO WHERE sac.USER_NO={userNo}";
            //    accessedData = _objectEntity.SqlQuery<AccessedControl>(accessedQuery).ToList();

            //    foreach (var cln in consolidateListNodes)
            //    {
            //        if (cln.branch_Code == accessedData.Where(x => x.BRANCH_CODE == cln.branch_Code).Select(x => x.BRANCH_CODE).FirstOrDefault())
            //        {
            //            cln.@checked = true;

            //        }
            //    }
            //}
            return consolidateListNodes;
        }



        public List<ConsolidateTree> branchListByCompanyCode(User userinfo, string company_code)
        {

            string query = @"SELECT INITCAP(CS.BRANCH_EDESC) AS BRANCH_EDESC,CS.BRANCH_CODE,
            CS.PRE_BRANCH_CODE,CC.ABBR_CODE
            FROM FA_BRANCH_SETUP CS, COMPANY_SETUP CC
            WHERE  CS.COMPANY_CODE = CC.COMPANY_CODE
            AND CS.DELETED_FLAG = 'N'
            AND CS.GROUP_SKU_FLAG = 'I' AND CS.COMPANY_CODE = '" + company_code + "'  AND CC.COMPANY_CODE IN (SELECT COMPANY_CODE FROM SC_COMPANY_CONTROL WHERE ACCESS_FLAG='Y' AND USER_NO='" + userinfo.User_id + "')";
            var consolidateListNodes = _objectEntity.SqlQuery<ConsolidateTree>(query).ToList();
            return consolidateListNodes;
        }


        public List<CustomerSetupModel> CustomerGroupListAllNodes()
        {
            string query = @"SELECT DISTINCT LEVEL,INITCAP(CS.CUSTOMER_EDESC) CUSTOMER_EDESC,CS.CUSTOMER_CODE,
            CS.GROUP_SKU_FLAG,CS.MASTER_CUSTOMER_CODE,CS.PRE_CUSTOMER_CODE, CS.BRANCH_CODE, LEVEL,
            (SELECT COUNT(*) FROM SA_CUSTOMER_SETUP WHERE GROUP_SKU_FLAG='G' AND PRE_CUSTOMER_CODE = CS.MASTER_CUSTOMER_CODE) as Childrens
            FROM SA_CUSTOMER_SETUP CS
            WHERE CS.DELETED_FLAG = 'N'
            AND GROUP_SKU_FLAG = 'G'
            AND LEVEL = 1
            START WITH PRE_CUSTOMER_CODE = '00'
            CONNECT BY PRIOR MASTER_CUSTOMER_CODE = PRE_CUSTOMER_CODE
            ORDER SIBLINGS BY CUSTOMER_EDESC";
            var customerListNodes = _objectEntity.SqlQuery<CustomerSetupModel>(query).ToList();
            return customerListNodes;
        }

        public List<ProductSetupModel> ProductListAllNodes()
        {
            string query = @"SELECT DISTINCT LEVEL, 
                 INITCAP(ITEM_EDESC) AS ItemName,
                 ITEM_CODE AS ItemCode,
                 MASTER_ITEM_CODE AS MasterItemCode, 
                 PRE_ITEM_CODE AS PreItemCode, 
                 GROUP_SKU_FLAG AS GroupFlag
                 FROM IP_ITEM_MASTER_SETUP ims
                 WHERE ims.DELETED_FLAG = 'N'                  
                 AND GROUP_SKU_FLAG = 'G'
                 AND LEVEL = 1
                 START WITH PRE_ITEM_CODE = '00'
                 CONNECT BY PRIOR MASTER_ITEM_CODE = PRE_ITEM_CODE";
            var productListNodes = _objectEntity.SqlQuery<ProductSetupModel>(query).ToList();
            return productListNodes;
        }

        public List<ProductSetupModel> ProductListAllNodes(User userinfo)
        {

            string query = @"SELECT LEVEL, 
                 INITCAP(ITEM_EDESC) AS ItemName,
                 ITEM_CODE AS ItemCode,
                 MASTER_ITEM_CODE AS MasterItemCode, 
                 PRE_ITEM_CODE AS PreItemCode, 
                 GROUP_SKU_FLAG AS GroupFlag,
                (SELECT COUNT(*) FROM IP_ITEM_MASTER_SETUP WHERE  
                GROUP_SKU_FLAG='G' AND COMPANY_CODE='" + userinfo.company_code + @"' AND DELETED_FLAG='N' AND PRE_ITEM_CODE = ims.MASTER_ITEM_CODE) as Childrens 
                 FROM IP_ITEM_MASTER_SETUP ims
                 WHERE ims.DELETED_FLAG = 'N' 
                 AND LEVEL=1
                 AND ims.COMPANY_CODE = '" + userinfo.company_code + @"'
                 AND GROUP_SKU_FLAG = 'G'
                 START WITH PRE_ITEM_CODE = '00'
                 CONNECT BY PRIOR MASTER_ITEM_CODE = PRE_ITEM_CODE";
            var productListNodes = _objectEntity.SqlQuery<ProductSetupModel>(query).ToList();
            return productListNodes;
        }

        public List<ProductSetupModel> GetProductsListByProductCode(string level, string masterProductCode)
        {
            string query = string.Format(@"SELECT DISTINCT LEVEL, 
            INITCAP(ITEM_EDESC) AS ItemName,
            ITEM_CODE AS ItemCode,
            MASTER_ITEM_CODE AS MasterItemCode, 
            PRE_ITEM_CODE AS PreItemCode,
            GROUP_SKU_FLAG AS GroupFlag
            FROM IP_ITEM_MASTER_SETUP
            WHERE DELETED_FLAG = 'N'             
            AND LEVEL = {0}
            START WITH PRE_ITEM_CODE = '{1}'
            CONNECT BY PRIOR MASTER_ITEM_CODE = PRE_ITEM_CODE
            ORDER BY INITCAP(ITEM_EDESC)", level.ToString(), masterProductCode.ToString());
            var productListNodes = _objectEntity.SqlQuery<ProductSetupModel>(query).ToList();
            return productListNodes;
        }

        public List<ProductSetupModel> GetProductsListByProductCode(string level, string masterProductCode, User userinfo)
        {
            string query = string.Format(@"SELECT LEVEL, 
            INITCAP(ITEM_EDESC) AS ItemName,
            ITEM_CODE AS ItemCode,
            MASTER_ITEM_CODE AS MasterItemCode, 
            PRE_ITEM_CODE AS PreItemCode, 
            GROUP_SKU_FLAG AS GroupFlag,
            (SELECT COUNT(*) FROM IP_ITEM_MASTER_SETUP WHERE  
             COMPANY_CODE='" + userinfo.company_code + @"' AND DELETED_FLAG='N' AND PRE_ITEM_CODE = ims.MASTER_ITEM_CODE) as Childrens 
            FROM IP_ITEM_MASTER_SETUP ims
            WHERE ims.DELETED_FLAG = 'N' 
            AND ims.COMPANY_CODE = '" + userinfo.company_code + @"'
            AND LEVEL = {0}
            START WITH PRE_ITEM_CODE = '{1}'
            CONNECT BY PRIOR MASTER_ITEM_CODE = PRE_ITEM_CODE
            ORDER SIBLINGS BY ITEM_EDESC", level.ToString(), masterProductCode.ToString());
            var productListNodes = _objectEntity.SqlQuery<ProductSetupModel>(query).ToList();
            return productListNodes;
        }


        public List<ProductSetupModel> GetProductsListWithChild(string level, string masterProductCode, User userinfo)
        {
            string query = string.Format(@"SELECT LEVEL, 
            INITCAP(ITEM_EDESC) AS ItemName,
            ITEM_CODE AS ItemCode,
            MASTER_ITEM_CODE AS MasterItemCode, 
            PRE_ITEM_CODE AS PreItemCode, 
            GROUP_SKU_FLAG AS GroupFlag,
            (SELECT COUNT(*) FROM IP_ITEM_MASTER_SETUP WHERE  
             COMPANY_CODE='" + userinfo.company_code + @"' AND DELETED_FLAG='N' AND PRE_ITEM_CODE = ims.MASTER_ITEM_CODE) as Childrens 
            FROM IP_ITEM_MASTER_SETUP ims
            WHERE ims.DELETED_FLAG = 'N' 
            AND ims.COMPANY_CODE = '" + userinfo.company_code + @"'
            --AND LEVEL = {0}
            START WITH PRE_ITEM_CODE = '{1}'
            CONNECT BY PRIOR MASTER_ITEM_CODE = PRE_ITEM_CODE
            ORDER SIBLINGS BY ITEM_EDESC", level.ToString(), masterProductCode.ToString());
            var productListNodes = _objectEntity.SqlQuery<ProductSetupModel>(query).ToList();
            return productListNodes;
        }

        // Supplier query for the SupplierTree
        public List<SupplierSetupModel> GetSupplierListBySupplierCode(string level, string masterSupplierCode)
        {
            string query = string.Format(@"SELECT DISTINCT LEVEL, COMPANY_CODE,DELETED_FLAG,
                                        INITCAP(SUPPLIER_EDESC) AS SupplierName,
                                        SUPPLIER_CODE AS SupplierCode,
                                        MASTER_SUPPLIER_CODE AS MasterSupplierCode, 
                                        PRE_SUPPLIER_CODE AS PreSupplierCode, 
                                        GROUP_SKU_FLAG AS GroupFlag
                                        FROM IP_SUPPLIER_SETUP ims
                                        WHERE ims.DELETED_FLAG = 'N'                                         
                                        --AND GROUP_SKU_FLAG = 'G'         
                                        AND LEVEL = {0} 
                                        START WITH PRE_SUPPLIER_CODE = {1}
                                        CONNECT BY PRIOR MASTER_SUPPLIER_CODE = PRE_SUPPLIER_CODE
                                        /*ORDER SIBLINGS BY SUPPLIER_EDESC*/", level.ToString(), masterSupplierCode.ToString());
            var SupplierListNodes = _objectEntity.SqlQuery<SupplierSetupModel>(query).ToList();
            return SupplierListNodes;
        }

        public List<SupplierSetupModel> GetSupplierListBySupplierCode(string level, string masterSupplierCode, User userinfo)
        {
            string query = string.Format(@"SELECT LEVEL, COMPANY_CODE,DELETED_FLAG,
                                        INITCAP(SUPPLIER_EDESC) AS SupplierName,
                                        SUPPLIER_CODE AS SupplierCode,
                                        MASTER_SUPPLIER_CODE AS MasterSupplierCode, 
                                        PRE_SUPPLIER_CODE AS PreSupplierCode, 
                                        GROUP_SKU_FLAG AS GroupFlag
                                        FROM IP_SUPPLIER_SETUP ims
                                        WHERE ims.DELETED_FLAG = 'N' 
                                        AND ims.COMPANY_CODE = '" + userinfo.company_code + @"'
                                        --AND ims.Branch_Code = '" + userinfo.branch_code + @"'
                                        --AND GROUP_SKU_FLAG = 'G'         
                                        AND LEVEL = {0} 
                                        START WITH PRE_SUPPLIER_CODE = {1}
                                        CONNECT BY PRIOR MASTER_SUPPLIER_CODE = PRE_SUPPLIER_CODE
                                        ORDER SIBLINGS BY SUPPLIER_EDESC", level.ToString(), masterSupplierCode.ToString());
            var SupplierListNodes = _objectEntity.SqlQuery<SupplierSetupModel>(query).ToList();
            return SupplierListNodes;
        }
        public List<SupplierSetupModel> GetDealerListBySupplierCode(string level, string masterSupplierCode, User userinfo)
        {
            string query = string.Format(@"SELECT LEVEL, COMPANY_CODE,DELETED_FLAG,
                                        INITCAP(PARTY_TYPE_EDESC) AS SupplierName,
                                        PARTY_TYPE_CODE AS SupplierCode,
                                        MASTER_PARTY_CODE AS MasterSupplierCode, 
                                        PRE_PARTY_CODE AS PreSupplierCode, 
                                        GROUP_SKU_FLAG AS GroupFlag
                                        FROM IP_PARTY_TYPE_CODE ims
                                        WHERE ims.DELETED_FLAG = 'N' 
                                        AND ims.COMPANY_CODE = '" + userinfo.company_code + @"'
                                        --AND ims.Branch_Code = '" + userinfo.branch_code + @"'
                                        --AND GROUP_SKU_FLAG = 'G'         
                                        AND LEVEL = {0} 
                                        START WITH PRE_PARTY_CODE = '{1}'
                                        CONNECT BY PRIOR MASTER_PARTY_CODE = PRE_PARTY_CODE
                                        ORDER SIBLINGS BY PARTY_TYPE_EDESC", level.ToString(), masterSupplierCode.ToString());
            var SupplierListNodes = _objectEntity.SqlQuery<SupplierSetupModel>(query).ToList();
            return SupplierListNodes;
        }
        public List<SupplierSetupModel> GetSupplierListBySupplierCodeGroup(string masterSupplierCode)
        {
            string query = string.Format(@"SELECT LEVEL, COMPANY_CODE,DELETED_FLAG,
                                        INITCAP(SUPPLIER_EDESC) AS SupplierName,
                                        SUPPLIER_CODE AS SupplierCode,
                                        MASTER_SUPPLIER_CODE AS MasterSupplierCode, 
                                        PRE_SUPPLIER_CODE AS PreSupplierCode, 
                                        GROUP_SKU_FLAG AS GroupFlag,
                                        (SELECT COUNT(*) FROM IP_SUPPLIER_SETUP WHERE GROUP_SKU_FLAG='G' AND PRE_SUPPLIER_CODE = IMS.MASTER_SUPPLIER_CODE) as Childrens
                                        FROM IP_SUPPLIER_SETUP ims
                                        WHERE ims.DELETED_FLAG = 'N' 
                                        AND ims.COMPANY_CODE = '01'
                                        AND GROUP_SKU_FLAG = 'G'
                                        START WITH PRE_SUPPLIER_CODE = {0}
                                        CONNECT BY PRIOR MASTER_SUPPLIER_CODE = PRE_SUPPLIER_CODE
                                        ORDER SIBLINGS BY SUPPLIER_EDESC", masterSupplierCode.ToString());
            var SupplierListNodes = _objectEntity.SqlQuery<SupplierSetupModel>(query).ToList();
            return SupplierListNodes;
        }

        public List<SupplierSetupModel> GetSupplierListBySupplierCodeGroup(string masterSupplierCode, User userinfo)
        {
            if (userinfo == null)
            {
                userinfo = new Core.Domain.User();
                userinfo.company_code = "01";
                userinfo.branch_code = "01.01";

            }
            else if (string.IsNullOrEmpty(userinfo.company_code))
            {
                userinfo.company_code = "01";
                userinfo.branch_code = "01.01";
            }
            else if (string.IsNullOrEmpty(userinfo.branch_code))
            {
                userinfo.company_code = "01";
                userinfo.branch_code = "01.01";
            }
            string query = string.Format(@"SELECT LEVEL, COMPANY_CODE,DELETED_FLAG,
                                        INITCAP(SUPPLIER_EDESC) AS SupplierName,
                                        SUPPLIER_CODE AS SupplierCode,
                                        MASTER_SUPPLIER_CODE AS MasterSupplierCode, 
                                        PRE_SUPPLIER_CODE AS PreSupplierCode, 
                                        GROUP_SKU_FLAG AS GroupFlag,
                                        (SELECT COUNT(*) FROM IP_SUPPLIER_SETUP WHERE GROUP_SKU_FLAG='G' AND PRE_SUPPLIER_CODE = IMS.MASTER_SUPPLIER_CODE) as Childrens
                                        FROM IP_SUPPLIER_SETUP ims
                                        WHERE ims.DELETED_FLAG = 'N' 
                                        AND ims.COMPANY_CODE = '" + userinfo.company_code + @"'
                                        AND ims.Branch_Code = '" + userinfo.branch_code + @"'
                                        AND GROUP_SKU_FLAG = 'G'
                                        START WITH PRE_SUPPLIER_CODE = {0}
                                        CONNECT BY PRIOR MASTER_SUPPLIER_CODE = PRE_SUPPLIER_CODE
                                        ORDER SIBLINGS BY SUPPLIER_EDESC", masterSupplierCode.ToString());
            var SupplierListNodes = _objectEntity.SqlQuery<SupplierSetupModel>(query).ToList();
            return SupplierListNodes;
        }

        public List<SalesRegisterModel> SaleRegisters()
        {
            string query = @"SELECT A.SALES_DATE, BS_DATE(A.SALES_DATE) MITI, A.SALES_NO, B.CUSTOMER_EDESC, C.ITEM_EDESC, A.MU_CODE, A.CALC_QUANTITY, A.CALC_UNIT_PRICE, A.CALC_TOTAL_PRICE,
            (SELECT SUM(CHARGE_AMOUNT) FROM CHARGE_TRANSACTION WHERE REFERENCE_NO = A.SALES_NO AND CHARGE_CODE = 'DC' and APPLY_ON='D' AND   1 = A.SERIAL_NO) AS DISCOUNT,
            (SELECT CHARGE_AMOUNT FROM CHARGE_TRANSACTION WHERE REFERENCE_NO = A.SALES_NO AND CHARGE_CODE = 'VT' and APPLY_ON='D' AND  1 = A.SERIAL_NO) AS VAT
            FROM SA_SALES_INVOICE A, SA_CUSTOMER_SETUP B, IP_ITEM_MASTER_SETUP C
            WHERE A.CUSTOMER_CODE = B.CUSTOMER_CODE
            AND A.COMPANY_CODE = B.COMPANY_CODE
            AND A.ITEM_CODE = C.ITEM_CODE
            AND A.COMPANY_CODE = C.COMPANY_CODE
            AND A.DELETED_FLAG = 'N'
            ORDER BY A.SALES_NO";
            var salesRegisters = _objectEntity.SqlQuery<SalesRegisterModel>(query).ToList();
            foreach (var salesReg in salesRegisters)
            {
                salesReg.CALC_UNIT_PRICE = salesReg.CALC_UNIT_PRICE == null ? 0 : salesReg.CALC_UNIT_PRICE;
                salesReg.CALC_QUANTITY = salesReg.CALC_QUANTITY == null ? 0 : salesReg.CALC_QUANTITY;
                salesReg.CALC_TOTAL_PRICE = salesReg.CALC_TOTAL_PRICE == null ? 0 : salesReg.CALC_TOTAL_PRICE;
                salesReg.DISCOUNT = salesReg.DISCOUNT == null ? 0 : salesReg.DISCOUNT;
                salesReg.VAT = salesReg.VAT == null ? 0 : salesReg.VAT;
                salesReg.NetAmount = salesReg.CALC_TOTAL_PRICE - salesReg.DISCOUNT;
                salesReg.InvoiceAmount = salesReg.NetAmount + salesReg.VAT;
            }

            return salesRegisters;
        }
        public List<SalesRegisterModel> SaleRegistersDateWiseFilter(string formDate, string toDate)
        {
            //if(string.IsNullOrEmpty(formDate))

            string query = @"SELECT A.SALES_DATE, BS_DATE(A.SALES_DATE) MITI, A.SALES_NO, B.CUSTOMER_EDESC, C.ITEM_EDESC, A.MU_CODE, A.CALC_QUANTITY, A.CALC_UNIT_PRICE, A.CALC_TOTAL_PRICE,
            (SELECT SUM(CHARGE_AMOUNT) FROM CHARGE_TRANSACTION WHERE REFERENCE_NO = A.SALES_NO AND CHARGE_CODE = 'DC' and APPLY_ON='D' AND   1 = A.SERIAL_NO) AS DISCOUNT,
            (SELECT CHARGE_AMOUNT FROM CHARGE_TRANSACTION WHERE REFERENCE_NO = A.SALES_NO AND CHARGE_CODE = 'VT' and APPLY_ON='D' AND  1 = A.SERIAL_NO) AS VAT
            FROM SA_SALES_INVOICE A, SA_CUSTOMER_SETUP B, IP_ITEM_MASTER_SETUP C
            WHERE A.CUSTOMER_CODE = B.CUSTOMER_CODE
            AND A.COMPANY_CODE = B.COMPANY_CODE
            AND A.ITEM_CODE = C.ITEM_CODE
            AND A.COMPANY_CODE = C.COMPANY_CODE
            AND A.DELETED_FLAG = 'N'";
            if (!string.IsNullOrEmpty(formDate))
                query = query + " and A.SALES_DATE>=TO_DATE('" + formDate + "', 'YYYY-MM-DD') and A.SALES_DATE <= TO_DATE('" + toDate + "', 'YYYY-MM-DD')";
            var salesRegisters = _objectEntity.SqlQuery<SalesRegisterModel>(query).ToList();
            foreach (var salesReg in salesRegisters)
            {
                salesReg.CALC_UNIT_PRICE = salesReg.CALC_UNIT_PRICE == null ? 0 : salesReg.CALC_UNIT_PRICE;
                salesReg.CALC_QUANTITY = salesReg.CALC_QUANTITY == null ? 0 : salesReg.CALC_QUANTITY;
                salesReg.CALC_TOTAL_PRICE = salesReg.CALC_TOTAL_PRICE == null ? 0 : salesReg.CALC_TOTAL_PRICE;
                salesReg.DISCOUNT = salesReg.DISCOUNT == null ? 0 : salesReg.DISCOUNT;
                salesReg.VAT = salesReg.VAT == null ? 0 : salesReg.VAT;
                salesReg.NetAmount = salesReg.CALC_TOTAL_PRICE - salesReg.DISCOUNT;
                salesReg.InvoiceAmount = salesReg.NetAmount + salesReg.VAT;
            }

            return salesRegisters;
        }
        public List<SalesRegisterModel> GetSaleRegisters(ReportFiltersModel filters)
        {
            //if(string.IsNullOrEmpty(formDate))

            string query = @"SELECT A.SALES_DATE, BS_DATE(A.SALES_DATE) MITI, A.SALES_NO, B.CUSTOMER_EDESC, C.ITEM_EDESC, A.MU_CODE, Round(A.CALC_QUANTITY/{0},{1}), A.CALC_UNIT_PRICE, A.CALC_TOTAL_PRICE,
                (SELECT SUM(CHARGE_AMOUNT) FROM CHARGE_TRANSACTION WHERE REFERENCE_NO = A.SALES_NO AND CHARGE_CODE = 'DC' and APPLY_ON='D' AND   1 = A.SERIAL_NO) AS DISCOUNT,
                (SELECT CHARGE_AMOUNT FROM CHARGE_TRANSACTION WHERE REFERENCE_NO = A.SALES_NO AND CHARGE_CODE = 'VT' and APPLY_ON='D' AND  1 = A.SERIAL_NO) AS VAT
                FROM SA_SALES_INVOICE A, SA_CUSTOMER_SETUP B, IP_ITEM_MASTER_SETUP C
                WHERE A.CUSTOMER_CODE = B.CUSTOMER_CODE
                AND A.COMPANY_CODE = B.COMPANY_CODE
                AND A.ITEM_CODE = C.ITEM_CODE
                AND A.COMPANY_CODE = C.COMPANY_CODE
                AND A.DELETED_FLAG = 'N'";
            if (!string.IsNullOrEmpty(filters.FromDate))
                query = query + " and A.SALES_DATE>=TO_DATE('" + filters.FromDate + "', 'YYYY-MM-DD') and A.SALES_DATE <= TO_DATE('" + filters.ToDate + "', 'YYYY-MM-DD')";
            var salesRegisters = _objectEntity.SqlQuery<SalesRegisterModel>(query).ToList();
            foreach (var salesReg in salesRegisters)
            {
                salesReg.CALC_UNIT_PRICE = salesReg.CALC_UNIT_PRICE == null ? 0 : salesReg.CALC_UNIT_PRICE;
                salesReg.CALC_QUANTITY = salesReg.CALC_QUANTITY == null ? 0 : salesReg.CALC_QUANTITY;
                salesReg.CALC_TOTAL_PRICE = salesReg.CALC_TOTAL_PRICE == null ? 0 : salesReg.CALC_TOTAL_PRICE;
                salesReg.DISCOUNT = salesReg.DISCOUNT == null ? 0 : salesReg.DISCOUNT;
                salesReg.VAT = salesReg.VAT == null ? 0 : salesReg.VAT;
                salesReg.NetAmount = salesReg.CALC_TOTAL_PRICE - salesReg.DISCOUNT;
                salesReg.InvoiceAmount = salesReg.NetAmount + salesReg.VAT;
            }

            return salesRegisters;
        }
        public List<Charges> GetSalesCharges()
        {
            string query = @"SELECT  CT.charge_code, CC.CHARGE_EDESC,CT.CHARGE_AMOUNT as CHARGE_AMOUNT,CT.APPLY_ON,CT.REFERENCE_NO,CT.CHARGE_TYPE_FLAG  FROM CHARGE_TRANSACTION CT, IP_CHARGE_CODE CC
            WHERE 1=1
            AND CT.CHARGE_CODE = CC.CHARGE_CODE
            and form_code in (select form_code from form_detail_setup where table_name = 'SA_SALES_INVOICE') and CT.apply_on='D'";
            var salesRegisters = _objectEntity.SqlQuery<Charges>(query).ToList();
            return salesRegisters;
        }
        public List<Charges> GetSalesCharges(ReportFiltersModel filters)
        {
            var user = this._workContext.CurrentUserinformation;
            // var companyCode = string.Join(",", filters.CompanyFilter);
            var companyCode = string.Empty;
            foreach (var company in filters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }

            companyCode = companyCode == "" ? $@"'{this._workContext.CurrentUserinformation.company_code}'" : companyCode.Remove(companyCode.Length - 1);
            // companyCode = companyCode == "" ? this._workContext.CurrentUserinformation.company_code : companyCode;
            string query = @"SELECT  CT.charge_code||CT.APPLY_ON as charge_code,  CC.CHARGE_EDESC ||'(' || CT.APPLY_ON ||')' as CHARGE_EDESC,Round(NVL(FN_CONVERT_CURRENCY(NVL(CT.CHARGE_AMOUNT,0),'NRS',(SELECT SALES_DATE FROM SA_SALES_INVOICE WHERE SALES_NO = CT.REFERENCE_NO and company_code=ct.company_code AND ROWNUM=1)),0)/{0},{1}) as CHARGE_AMOUNT,CT.APPLY_ON,CT.REFERENCE_NO,CT.CHARGE_TYPE_FLAG  FROM CHARGE_TRANSACTION CT, IP_CHARGE_CODE CC
            WHERE 1=1
            AND CT.CHARGE_CODE = CC.CHARGE_CODE AND  CT.COMPANY_CODE=CC.COMPANY_CODE AND CT.COMPANY_CODE in({2}) and   TRUNC(CT.created_date)>=TRUNC(TO_DATE('" + filters.FromDate + "', 'YYYY-MM-DD')) and TRUNC(CT.created_date) <= TRUNC(TO_DATE('" + filters.ToDate + "', 'YYYY-MM-DD'))";
            if (filters.BranchFilter.Count > 0)
            {
                query = query + string.Format(@" AND CT.BRANCH_CODE IN ('{0}')", string.Join("','", filters.BranchFilter).ToString());
            }

            query = query + " and CT.form_code in (select form_code from form_detail_setup where table_name = 'SA_SALES_INVOICE' and company_code="+companyCode+")";
            query = string.Format(query, ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter), ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter), companyCode);
            var salesRegisters = _objectEntity.SqlQuery<Charges>(query).ToList();
            return salesRegisters;

        }
        public List<Charges> GetSalesItemCharges(ReportFiltersModel filters, string salesNo)
        {
            var user = this._workContext.CurrentUserinformation;
            var companyCode = string.Empty;
            foreach (var company in filters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }

            companyCode = companyCode == "" ? $@"'{this._workContext.CurrentUserinformation.company_code}'" : companyCode.Remove(companyCode.Length - 1);
            string query = @"SELECT DISTINCT  CT.charge_code||CT.APPLY_ON as charge_code,  CC.CHARGE_EDESC ||'(' || CT.APPLY_ON ||')' as CHARGE_EDESC,Round(NVL(FN_CONVERT_CURRENCY(NVL(CT.CHARGE_AMOUNT,0),'NRS',(SELECT SALES_DATE FROM SA_SALES_INVOICE WHERE SALES_NO = CT.REFERENCE_NO AND ROWNUM=1)),0)/{0},{1}) as CHARGE_AMOUNT,CT.APPLY_ON,CT.REFERENCE_NO,CT.CHARGE_TYPE_FLAG,CT.ITEM_CODE  FROM CHARGE_TRANSACTION CT, IP_CHARGE_CODE CC
            WHERE 1=1
            AND CT.CHARGE_CODE = CC.CHARGE_CODE AND CT.COMPANY_CODE in({2}) and   CT.created_date>=TO_DATE('" + filters.FromDate + "', 'YYYY-MM-DD') and CT.created_date <= TO_DATE('" + filters.ToDate + "', 'YYYY-MM-DD')";
            if (filters.BranchFilter.Count > 0)
            {
                query = query + string.Format(@" AND CT.BRANCH_CODE IN ('{0}')", string.Join("','", filters.BranchFilter).ToString());
            }

            query = query + " and CT.REFERENCE_NO='" + salesNo + "' and CT.form_code in (select form_code from form_detail_setup where table_name = 'SA_SALES_INVOICE') and CT.apply_on='I'";
            query = string.Format(query, ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter), ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter), companyCode);
            var salesRegisters = _objectEntity.SqlQuery<Charges>(query).ToList();
            return salesRegisters;

        }
        public List<ChargesTitle> GetChargesTitle()
        {
            var user = this._workContext.CurrentUserinformation;
            string query = @"   SELECT distinct CT.charge_code||CT.APPLY_ON as ChargesHeaderNo, CC.CHARGE_EDESC ||'(' || CT.APPLY_ON ||')' as ChargesHeaderTitle,CT.APPLY_ON
          FROM CHARGE_TRANSACTION CT, IP_CHARGE_CODE CC
        WHERE 1=1
        AND CT.CHARGE_CODE = CC.CHARGE_CODE
        and form_code in (select form_code from form_detail_setup where table_name = 'SA_SALES_INVOICE')";
            var chargeTitles = _objectEntity.SqlQuery<ChargesTitle>(query).ToList();
            return chargeTitles;
        }
        public List<ChargesTitle> GetChargesItemTitle()
        {
            var user = this._workContext.CurrentUserinformation;
            string query = @"SELECT distinct CT.charge_code||CT.APPLY_ON as ChargesHeaderNo,  CC.CHARGE_EDESC ||'(' || CT.APPLY_ON ||')' as ChargesHeaderTitle
          FROM CHARGE_TRANSACTION CT, IP_CHARGE_CODE CC
        WHERE 1=1
        AND CT.CHARGE_CODE = CC.CHARGE_CODE
        and form_code in (select form_code from form_detail_setup where table_name = 'SA_SALES_INVOICE') and CT.apply_on='I'";
            var chargeTitles = _objectEntity.SqlQuery<ChargesTitle>(query).ToList();
            return chargeTitles;
        }
        public List<VatRegisterModel> GetVatRegister()
        {
            string query = @"SELECT BS_DATE(SALES_DATE) MITI, INVOICE_NO as InvoiceNo, PARTY_NAME as PartyName, VAT_NO as PANNo, 
                FN_CONVERT_CURRENCY(NVL(GROSS_SALES,0) * NVL(EXCHANGE_RATE,1),'NRS', SALES_DATE), 
                FN_CONVERT_CURRENCY(NVL(TAXABLE_SALES,1) * NVL(EXCHANGE_RATE,1),'NRS', SALES_DATE) as TaxableSales,
                 FN_CONVERT_CURRENCY(NVL(VAT,0) * NVL(EXCHANGE_RATE,1),'NRS', SALES_DATE) as VatAmount , 
                 FN_CONVERT_CURRENCY(NVL(TOTAL_SALES,0) * NVL(EXCHANGE_RATE,1),'NRS', SALES_DATE) as NetSales ,
                 FORM_CODE, BRANCH_CODE,CREDIT_DAYS,DELETED_FLAG,SALES_DISCOUNT as Discount, MANUAL_NO,DELETED_FLAG 
                 FROM V$SALES_INVOICE_REPORT3 WHERE SALES_DATE >= '13-Apr-2014' 
                 AND SALES_DATE <= '13-May-2016' AND COMPANY_CODE='01'  AND BRANCH_CODE='01.01'   ORDER BY BS_DATE(SALES_DATE), INVOICE_NO";
            var vatRegisters = _objectEntity.SqlQuery<VatRegisterModel>(query).ToList();
            return vatRegisters;
        }
        public List<VatRegisterModel> GetVatRegisterDateWiseFilter(string formDate, string toDate)
        {
            string query = @"SELECT BS_DATE(SALES_DATE) MITI, INVOICE_NO as InvoiceNo, PARTY_NAME as PartyName, VAT_NO as PANNo, 
            FN_CONVERT_CURRENCY(NVL(GROSS_SALES,0) * NVL(EXCHANGE_RATE,1),'NRS', SALES_DATE), 
            FN_CONVERT_CURRENCY(NVL(TAXABLE_SALES,1) * NVL(EXCHANGE_RATE,1),'NRS', SALES_DATE) as TaxableSales,
             FN_CONVERT_CURRENCY(NVL(VAT,0) * NVL(EXCHANGE_RATE,1),'NRS', SALES_DATE) as VatAmount , 
             FN_CONVERT_CURRENCY(NVL(TOTAL_SALES,0) * NVL(EXCHANGE_RATE,1),'NRS', SALES_DATE) as NetSales ,
             FORM_CODE, BRANCH_CODE,CREDIT_DAYS,DELETED_FLAG,SALES_DISCOUNT as Discount, MANUAL_NO,DELETED_FLAG 
             FROM V$SALES_INVOICE_REPORT3 WHERE SALES_DATE >= TO_DATE('" + formDate + "', 'YYYY-MM-DD') AND SALES_DATE <= TO_DATE('" + toDate + "', 'YYYY-MM-DD') AND COMPANY_CODE='01'  AND BRANCH_CODE='01.01'   ORDER BY BS_DATE(SALES_DATE), INVOICE_NO";
            var vatRegisters = _objectEntity.SqlQuery<VatRegisterModel>(query).ToList();
            return vatRegisters;
        }

        public List<VatRegisterModel> GetVatRegister(ReportFiltersModel filters, NeoErp.Core.Domain.User userinfo)
        {
            var figureFilter = ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter).ToString();
            var roundUpFilter = ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter).ToString();
            //var companyCode = string.Join(",", filters.CompanyFilter);
            //companyCode = companyCode == "" ? userinfo.company_code : companyCode;
            var companyCode = string.Empty;
            foreach (var company in filters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }

            companyCode = companyCode == "" ? $@"'{userinfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);

            string query = string.Format(@"SELECT BS_DATE(SALES_DATE) MITI, INVOICE_NO as InvoiceNo, PARTY_NAME as PartyName, VAT_NO as PANNo, 
            FN_CONVERT_CURRENCY(Round((NVL(GROSS_SALES,0) * NVL(EXCHANGE_RATE,1))/{0},{1}),'NRS', SALES_DATE), 
            FN_CONVERT_CURRENCY(Round((NVL(TAXABLE_SALES,1) * NVL(EXCHANGE_RATE,1))/{2},{3}),'NRS', SALES_DATE) as TaxableSales,
             FN_CONVERT_CURRENCY(Round((NVL(VAT,0) * NVL(EXCHANGE_RATE,1))/{4},{5}),'NRS', SALES_DATE) as VatAmount , 
             FN_CONVERT_CURRENCY(Round((NVL(TOTAL_SALES,0) * NVL(EXCHANGE_RATE,1))/{6},{7}),'NRS', SALES_DATE) as NetSales ,

               FN_CONVERT_CURRENCY (NVL (TOTAL_SALES, 0) * NVL (EXCHANGE_RATE, 1),
                              'NRS',
                              SALES_DATE)
         - FN_CONVERT_CURRENCY (
              NVL (CASE WHEN TABLE_NAME = 'SALES_RETURN' THEN -NVL(SALES_DISCOUNT,0)
          ELSE NVL(SALES_DISCOUNT,0) END, 0) * NVL (EXCHANGE_RATE, 1),
              'NRS',
              SALES_DATE)
            EXCISEABLE_SALES,
         FN_CONVERT_CURRENCY (NVL (EXCISE_AMOUNT, 0) * NVL (EXCHANGE_RATE, 1),
                              'NRS',
                              SALES_DATE)
            TaxExempSales,
             FORM_CODE, BRANCH_CODE,CREDIT_DAYS,DELETED_FLAG,Round(CASE WHEN TABLE_NAME = 'SALES_RETURN' THEN -NVL(SALES_DISCOUNT,0)
          ELSE NVL(SALES_DISCOUNT,0) END/{8},{9}) as Discount, NVL(MANUAL_NO,'-') MANUAL_NO,DELETED_FLAG 
             FROM V$SALES_INVOICE_REPORT3 WHERE SALES_DATE >= TO_DATE('{10}', 'YYYY-MM-DD') AND SALES_DATE <= TO_DATE('{11}', 'YYYY-MM-DD') 
             and DELETED_FLAG='N' AND COMPANY_CODE IN(" + companyCode + ")", figureFilter, roundUpFilter, figureFilter, roundUpFilter, figureFilter, roundUpFilter, figureFilter, roundUpFilter, figureFilter, roundUpFilter, filters.FromDate, filters.ToDate);

            if (filters.CustomerFilter.Count > 0)
            {

                var customers = filters.CustomerFilter;
                var customerConditionQuery = string.Empty;
                for (int i = 0; i < customers.Count; i++)
                {

                    if (i == 0)
                        customerConditionQuery += string.Format("MASTER_CUSTOMER_CODE like (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%' from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG = 'G')", customers[i], companyCode);
                    else
                    {
                        customerConditionQuery += string.Format(" OR  MASTER_CUSTOMER_CODE like (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG = 'G')", customers[i], companyCode);
                    }
                }

                query = query + string.Format(@" AND CUSTOMER_CODE IN (SELECT DISTINCT(CUSTOMER_CODE) FROM SA_CUSTOMER_SETUP WHERE  {0} OR (CUSTOMER_CODE IN ('{1}') AND GROUP_SKU_FLAG='I')) ", customerConditionQuery, string.Join("','", customers));

            }

            if (filters.PartyTypeFilter.Count > 0)
            {
                query = query + string.Format(@" AND PARTY_TYPE_CODE IN ('{0}') ", string.Join("','", filters.PartyTypeFilter).ToString());
            }

            if (filters.BranchFilter.Count > 0)
            {
                query += string.Format(@" AND BRANCH_CODE IN ('{0}')", string.Join("','", filters.BranchFilter).ToString());
            }
            if (filters.DivisionFilter.Count > 0)
            {
                query += string.Format(@" AND DIVISION_CODE IN ('{0}')", string.Join("','", filters.DivisionFilter).ToString());
            }
            var min = 0;
            var max = 0;
            ReportFilterHelper.RangeFilterValue(filters.AmountRangeFilter, out min, out max);

            if (!(min == 0 && max == 0))
            {
                query = query + string.Format(@" AND FN_CONVERT_CURRENCY((NVL(TOTAL_SALES,0) * NVL(EXCHANGE_RATE,1)),'NRS',SALES_DATE) >={0} AND FN_CONVERT_CURRENCY((NVL(TOTAL_SALES,0) * NVL(EXCHANGE_RATE,1)),'NRS',SALES_DATE)<= {1}", min, max);
            }

            query += "ORDER BY BS_DATE(SALES_DATE), INVOICE_NO";

            // var salesRegisters = _objectEntity.SqlQuery<SalesRegisterMasterModel>(query).ToList();


            var vatRegisters = _objectEntity.SqlQuery<VatRegisterModel>(query).ToList();
            return vatRegisters;
        }


        public List<SalesRegisterMasterModel> SaleRegistersMasterDateWiseFilter(string formDate, string toDate)
        {
            string query = @"SELECT A.SALES_DATE as SalesDate ,A.MITI as Miti,A.SALES_NO as InvoiceNumber,A.CUSTOMER_EDESC as CustomerName,A.GROSS_AMOUNT as GrossAmount,NVL( A.DISCOUNT,0) as Discount, NVL(A.GROSS_AMOUNT,0)- NVL(A.DISCOUNT,0) NetAmount,NVL( A.VAT ,0) as VAT,NVL(A.GROSS_AMOUNT,0)- NVL(A.DISCOUNT,0)+NVL(A.VAT,0) InvoiceAmount FROM
                         (SELECT A.SALES_DATE, BS_DATE(A.SALES_DATE) MITI, A.SALES_NO, B.CUSTOMER_EDESC,
                         SUM(NVL(A.CALC_TOTAL_PRICE,0))  GROSS_AMOUNT,
                        (SELECT SUM(CHARGE_AMOUNT) FROM CHARGE_TRANSACTION WHERE REFERENCE_NO = A.SALES_NO AND CHARGE_CODE = 'DC' and APPLY_ON='D' ) AS DISCOUNT,
                        (SELECT CHARGE_AMOUNT FROM CHARGE_TRANSACTION WHERE REFERENCE_NO = A.SALES_NO AND CHARGE_CODE = 'VT' and APPLY_ON='D'  ) AS VAT
                        FROM SA_SALES_INVOICE A, SA_CUSTOMER_SETUP B, IP_ITEM_MASTER_SETUP C
                        WHERE A.CUSTOMER_CODE = B.CUSTOMER_CODE
                        AND A.COMPANY_CODE = B.COMPANY_CODE
                        AND A.ITEM_CODE = C.ITEM_CODE
                        AND A.COMPANY_CODE = C.COMPANY_CODE
                        AND A.DELETED_FLAG = 'N'";
            if (!string.IsNullOrEmpty(formDate))
                query = query + " and A.SALES_DATE>=TO_DATE('" + formDate + "', 'YYYY-MM-DD') and A.SALES_DATE <= TO_DATE('" + toDate + "', 'YYYY-MM-DD')";
            query = query + "GROUP BY A.SALES_DATE,  A.SALES_NO, B.CUSTOMER_EDESC) A ";
            var salesRegisters = _objectEntity.SqlQuery<SalesRegisterMasterModel>(query).ToList();


            return salesRegisters;
        }
        public List<SalesChildModel> GetSalesItemBySalesId(filterOption filters, string salesId, string ItemCompanyCode)
        {
            if (string.IsNullOrEmpty(salesId))
                throw new Exception();
            var companyCode = string.Join(",", filters.ReportFilters.CompanyFilter);
            companyCode = companyCode == "" ? this._workContext.CurrentUserinformation.company_code : companyCode;

            string query = @"SELECT  C.ITEM_EDESC AS ProductName,C.ITEM_CODE,A.SALES_NO AS INVOICENO,NVL(A.CALC_QUANTITY,0) AS Quanity,A.CALC_UNIT_PRICE AS Rate,A.CALC_TOTAL_PRICE AS GrossAmount
                ,A.MU_CODE AS UNIT
                FROM SA_SALES_INVOICE A, SA_CUSTOMER_SETUP B, IP_ITEM_MASTER_SETUP C
                WHERE A.CUSTOMER_CODE = B.CUSTOMER_CODE
                AND A.COMPANY_CODE = B.COMPANY_CODE
                AND A.ITEM_CODE = C.ITEM_CODE
                AND A.COMPANY_CODE = C.COMPANY_CODE
                AND a.COMPANY_CODE IN(" + ItemCompanyCode + ")";


            if (filters.ReportFilters.BranchFilter.Count > 0)
            {
                query += string.Format(@" AND A.BRANCH_CODE IN ('{0}')", string.Join("','", filters.ReportFilters.BranchFilter).ToString());
            }
            query += " AND A.DELETED_FLAG = 'N' AND A.SALES_NO='" + salesId + "'";
            return _objectEntity.SqlQuery<SalesChildModel>(query).ToList();



        }
        public List<ChargesMap> GetChargesMapList()
        {
            string query = @"SELECT distinct CT.charge_code as chargeFieldSystemName, CC.CHARGE_EDESC as chargeFieldName,CT.CHARGE_TYPE_FLAG as chargeType
              FROM CHARGE_TRANSACTION CT, IP_CHARGE_CODE CC
            WHERE 1=1
            AND CT.CHARGE_CODE = CC.CHARGE_CODE
            and form_code in (select form_code from form_detail_setup where table_name = 'SA_SALES_INVOICE') and CT.apply_on='D'";
            var chargeTitles = _objectEntity.SqlQuery<ChargesMap>(query).ToList();
            return chargeTitles;
        }

        public List<SalesVatWiseSummaryModel> GetSalesVatWiseSummaryDateWise(string formDate, string toDate)
        {
            string query = @"SELECT  B.CUSTOMER_EDESC as CustomerName ,B.TPIN_VAT_NO as VatNo,A.CUSTOMER_CODE as CustomerId,
             SUM(NVL(A.CALC_TOTAL_PRICE,0))  GrossAmount,SUM(NVL(A.CALC_QUANTITY,0))  Quantity
            FROM SA_SALES_INVOICE A, SA_CUSTOMER_SETUP B 
            WHERE A.CUSTOMER_CODE = B.CUSTOMER_CODE
            AND A.COMPANY_CODE = B.COMPANY_CODE
            AND A.DELETED_FLAG = 'N'";
            if (!string.IsNullOrEmpty(formDate))
                query = query + " and A.SALES_DATE>=TO_DATE('" + formDate + "', 'YYYY-MM-DD') and A.SALES_DATE <= TO_DATE('" + toDate + "', 'YYYY-MM-DD')";
            query = query + " GROUP BY  B.CUSTOMER_EDESC, A.CUSTOMER_CODE, A.COMPANY_CODE,B.TPIN_VAT_NO";
            var salesRegisters = _objectEntity.SqlQuery<SalesVatWiseSummaryModel>(query).ToList();
            return salesRegisters;
        }

        public List<SalesVatWiseSummaryModel> GetSalesVatWiseSummary(ReportFiltersModel filters)
        {

            var companyCode = string.Empty;
            foreach (var company in filters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }
            companyCode = companyCode == "" ? $@"'{this._workContext.CurrentUserinformation.company_code}'" : companyCode.Remove(companyCode.Length - 1);


            StringBuilder query = new StringBuilder();
            query.Append("SELECT  B.CUSTOMER_EDESC as CustomerName , NVL( B.TPIN_VAT_NO,'-') as VatNo,A.CUSTOMER_CODE as CustomerId,");
            query.AppendFormat("Round(SUM(FN_CONVERT_CURRENCY(NVL(A.CALC_TOTAL_PRICE,0),'NRS',A.SALES_DATE)/{1}),{0})  as GrossAmount,", ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter), ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter));
            query.AppendFormat("SUM(NVL(A.CALC_QUANTITY,0))/{0} Quantity ", ReportFilterHelper.FigureFilterValue(filters.QuantityFigureFilter));
            query.AppendFormat(" FROM SA_SALES_INVOICE A, SA_CUSTOMER_SETUP B,IP_ITEM_MASTER_SETUP C WHERE A.CUSTOMER_CODE = B.CUSTOMER_CODE AND A.COMPANY_CODE = B.COMPANY_CODE AND A.ITEM_CODE = C.ITEM_CODE AND A.COMPANY_CODE = C.COMPANY_CODE AND A.DELETED_FLAG = 'N' AND A.COMPANY_CODE IN({0}) ", companyCode);
            if (!string.IsNullOrEmpty(filters.FromDate))
                query.AppendFormat(" and A.SALES_DATE>=TO_DATE('{0}', 'YYYY-MM-DD') and A.SALES_DATE <= TO_DATE('{1}', 'YYYY-MM-DD')", filters.FromDate, filters.ToDate);

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
                        customerConditionQuery += string.Format(" OR  MASTER_CUSTOMER_CODE like (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G')", customers[i], companyCode);
                    }
                }

                query = query.AppendFormat(@" AND A.CUSTOMER_CODE IN (SELECT DISTINCT(CUSTOMER_CODE) FROM SA_CUSTOMER_SETUP WHERE  {0} OR (CUSTOMER_CODE IN ('{1}') AND GROUP_SKU_FLAG='I')) ", customerConditionQuery, string.Join("','", customers));
                //query = query.AppendFormat(@" AND A.CUSTOMER_CODE IN ({0}) ", string.Join(",", filters.CustomerFilter).ToString());
            }
            if (filters.ProductFilter.Count > 0)
            {

                var products = filters.ProductFilter;
                var productConditionQuery = string.Empty;
                for (int i = 0; i < products.Count; i++)
                {

                    if (i == 0)
                        productConditionQuery += string.Format("MASTER_ITEM_CODE like (Select DISTINCT(MASTER_ITEM_CODE) || '%' from IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G' )", products[i], companyCode);
                    else
                        productConditionQuery += string.Format(" OR  MASTER_ITEM_CODE like (Select DISTINCT(MASTER_ITEM_CODE) || '%'  from IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G' )", products[i], companyCode);

                }
                query = query.AppendFormat(@" AND A.ITEM_CODE IN (SELECT DISTINCT(ITEM_CODE) FROM IP_ITEM_MASTER_SETUP WHERE {0} OR (ITEM_CODE IN ('{1}') AND GROUP_SKU_FLAG='I')) ", productConditionQuery, string.Join("','", products));

                // query = query.AppendFormat(@" AND A.ITEM_CODE IN ({0}) ", string.Join(",", filters.ProductFilter).ToString());
            }
            if (filters.PartyTypeFilter.Count > 0)
            {
                query = query.AppendFormat(@" AND A.PARTY_TYPE_CODE IN ('{0}') ", string.Join("','", filters.PartyTypeFilter).ToString());
            }
            if (filters.AreaTypeFilter.Count > 0)
            {
                query = query.AppendFormat(@" AND A.AREA_CODE IN ('{0}') ", string.Join("','", filters.AreaTypeFilter).ToString());
            }
            if (filters.CategoryFilter.Count > 0)
            {
                query = query.AppendFormat(@" AND C.CATEGORY_CODE IN ('{0}') ", string.Join("','", filters.CategoryFilter).ToString());
            }
            if (filters.EmployeeFilter.Count > 0)
            {
                query = query.AppendFormat(@" AND A.EMPLOYEE_CODE IN ('{0}')", string.Join("','", filters.EmployeeFilter).ToString());
            }
            if (filters.AgentFilter.Count > 0)
            {
                query = query.AppendFormat(@" AND A.AGENT_CODE IN ('{0}')", string.Join("','", filters.AgentFilter).ToString());
            }
            if (filters.DivisionFilter.Count > 0)
            {
                query = query.AppendFormat(@" AND A.DIVISION_CODE IN ('{0}')", string.Join("','", filters.DivisionFilter).ToString());
            }
            if (filters.DocumentFilter.Count > 0)
            {
                query = query.AppendFormat(@" AND A.FORM_CODE IN ('{0}')", string.Join("','", filters.DocumentFilter).ToString());
            }
            if (filters.LocationFilter.Count > 0)
            {

                var locations = filters.LocationFilter;
                var locationConditionQuery = string.Empty;
                for (int i = 0; i < locations.Count; i++)
                {

                    if (i == 0)
                        locationConditionQuery += string.Format("SELECT LOCATION_CODE FROM IP_LOCATION_SETUP WHERE LOCATION_CODE LIKE '{0}%' ", locations[i]);
                    else
                    {
                        locationConditionQuery += string.Format(" OR LOCATION_CODE like '{0}%' ", locations[i]);
                    }
                }
                query = query.AppendFormat(@" AND A.FROM_LOCATION_CODE IN ({0} OR LOCATION_CODE IN ('{1}'))", locationConditionQuery, string.Join("','", locations));
                //query = query.AppendFormat(@" AND A.FROM_LOCATION_CODE IN ('{0}')", string.Join("','", filters.LocationFilter).ToString());
            }

            if (filters.BranchFilter.Count > 0)
            {
                query = query.AppendFormat(@" AND A.BRANCH_CODE IN ('{0}')", string.Join("','", filters.BranchFilter).ToString());
            }

            query.Append(" GROUP BY  B.CUSTOMER_EDESC, A.CUSTOMER_CODE, A.COMPANY_CODE,B.TPIN_VAT_NO");
            var min = 0; var max = 0;
            ReportFilterHelper.RangeFilterValue(filters.AmountRangeFilter, out min, out max);
            if (!(min == 0 && max == 0))
                query.AppendFormat(@" Having SUM(FN_CONVERT_CURRENCY(NVL(A.CALC_TOTAL_PRICE,0),'NRS',A.SALES_DATE)) >= {0} and SUM(FN_CONVERT_CURRENCY(NVL(A.CALC_TOTAL_PRICE,0),'NRS',A.SALES_DATE)) <= {1}", (decimal)min, (decimal)max);
            var salesRegisters = _objectEntity.SqlQuery<SalesVatWiseSummaryModel>(query.ToString()).ToList();
            return salesRegisters;
        }

        public List<SalesRegisterMasterModel> SaleRegistersMasterDynamicDateWiseFilter(string formDate, string toDate)
        {
            string query = @"SELECT A.SALES_DATE as SalesDate ,A.MITI as Miti,A.SALES_NO as InvoiceNumber,A.CUSTOMER_EDESC as CustomerName,A.GROSS_AMOUNT as GrossAmount FROM
             (SELECT A.SALES_DATE, BS_DATE(A.SALES_DATE) MITI, A.SALES_NO, B.CUSTOMER_EDESC,
             SUM(NVL(A.CALC_TOTAL_PRICE,0))  GROSS_AMOUNT
            FROM SA_SALES_INVOICE A, SA_CUSTOMER_SETUP B, IP_ITEM_MASTER_SETUP C
            WHERE A.CUSTOMER_CODE = B.CUSTOMER_CODE
            AND A.COMPANY_CODE = B.COMPANY_CODE
            AND A.ITEM_CODE = C.ITEM_CODE
            AND A.COMPANY_CODE = C.COMPANY_CODE
            AND A.DELETED_FLAG = 'N'";
            if (!string.IsNullOrEmpty(formDate))
                query = query + " and A.SALES_DATE>=TO_DATE('" + formDate + "', 'YYYY-MM-DD') and A.SALES_DATE <= TO_DATE('" + toDate + "', 'YYYY-MM-DD')";
            query = query + "GROUP BY A.SALES_DATE,  A.SALES_NO, B.CUSTOMER_EDESC) A ";
            var salesRegisters = _objectEntity.SqlQuery<SalesRegisterMasterModel>(query).ToList();
            return salesRegisters;
        }

        public List<SalesRegisterMasterModel> SaleRegistersMasterDynamic(ReportFiltersModel filters)
        {
            //var companyCode = string.Join(",", filters.CompanyFilter);
            ////companyCode = companyCode == "" ? this._workContext.CurrentUserinformation.company_code : companyCode;
            //if (string.IsNullOrEmpty(companyCode))
            //    companyCode = this._workContext.CurrentUserinformation.company_code;
            var companyCode = string.Empty;
            foreach (var company in filters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }

            companyCode = companyCode == "" ? $@"'{this._workContext.CurrentUserinformation.company_code}'" : companyCode.Remove(companyCode.Length - 1);
            string query = string.Format(@"SELECT 
                                             A.FORM_CODE,
                                            -- A.FROM_LOCATION_CODE,
                                            A.COMPANY_CODE,A.BRANCH_CODE,
                                             to_char( A.SALES_DATE )as SalesDate ,A.MITI as Miti,A.SALES_NO as InvoiceNumber,A.CUSTOMER_EDESC as CustomerName,
                                              Round(NVL(FN_CONVERT_CURRENCY(NVL(A.GROSS_AMOUNT,0),'NRS',A.SALES_DATE),0)/{0},{1}) as GrossAmount 
                                              FROM
                                               (SELECT 
                                                  A.FORM_CODE,A.FROM_LOCATION_CODE,A.COMPANY_CODE,A.BRANCH_CODE,
                                                  A.PARTY_TYPE_CODE, A.CUSTOMER_CODE, 
                                                  A.SALES_DATE, BS_DATE(A.SALES_DATE) MITI, A.SALES_NO, B.CUSTOMER_EDESC,
                                                  SUM(NVL(A.TOTAL_PRICE,0))  GROSS_AMOUNT
                                                  FROM SA_SALES_INVOICE A, SA_CUSTOMER_SETUP B, IP_ITEM_MASTER_SETUP C
                                                  WHERE A.CUSTOMER_CODE = B.CUSTOMER_CODE
                                                  AND A.COMPANY_CODE = B.COMPANY_CODE
                                                  AND A.ITEM_CODE = C.ITEM_CODE
                                                  AND A.COMPANY_CODE = C.COMPANY_CODE
                                                  AND A.COMPANY_CODE IN({2})
                                                  AND A.DELETED_FLAG = 'N'",
                                        ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter), ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter), companyCode);


            var min = 0;
            var max = 0;


            ReportFilterHelper.RangeFilterValue(filters.QuantityRangeFilter, out min, out max);
            if (!(min == 0 && max == 0))
            {
                query += string.Format(" and NVL(A.CALC_QUANTITY, 0)>={0} and NVL(A.CALC_QUANTITY, 0)<={1}", min, max);

            }

            ReportFilterHelper.RangeFilterValue(filters.RateRangeFilter, out min, out max);

            if (!(min == 0 && max == 0))
            {
                query += string.Format(" and NVL(FN_CONVERT_CURRENCY(NVL(A.CALC_UNIT_PRICE,0),'NRS',A.SALES_DATE),0) >={0} and NVL(FN_CONVERT_CURRENCY(NVL(A.CALC_UNIT_PRICE,0),'NRS',A.SALES_DATE),0) <={1}", min, max);

            }

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
                        customerConditionQuery += string.Format(" OR  MASTER_CUSTOMER_CODE like (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G')", customers[i], companyCode);
                    }
                }
                var customerfilter = string.Empty;
                foreach (var product in customers)
                {
                    customerfilter += $@"'{product}',";
                }
                customerfilter = customerfilter.Remove(customerfilter.Length - 1);
                query = query + string.Format(@" AND A.CUSTOMER_CODE IN (SELECT DISTINCT(CUSTOMER_CODE) FROM SA_CUSTOMER_SETUP WHERE  {0}  OR (CUSTOMER_CODE IN ({1}) AND GROUP_SKU_FLAG = 'I')) ", customerConditionQuery, customerfilter);

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
                var productfilter = string.Empty;
                foreach (var product in products)
                {
                    productfilter += $@"'{product}',";
                }
                productfilter = productfilter.Remove(productfilter.Length - 1);
                query = query + string.Format(@" AND A.ITEM_CODE IN (SELECT DISTINCT(ITEM_CODE) FROM IP_ITEM_MASTER_SETUP WHERE {0} OR (ITEM_CODE IN ({1}) AND GROUP_SKU_FLAG = 'I')) ", productConditionQuery, productfilter);



                // query = query + string.Format(@" AND A.ITEM_CODE IN ({0}) ", string.Join(",", filters.ProductFilter).ToString());
            }
            if (filters.PartyTypeFilter.Count > 0)
            {
                query = query + string.Format(@" AND A.PARTY_TYPE_CODE IN ('{0}') ", string.Join("','", filters.PartyTypeFilter).ToString());
            }
            if (filters.AreaTypeFilter.Count > 0)
            {
                query = query + string.Format(@" AND A.AREA_CODE IN ('{0}') ", string.Join("','", filters.AreaTypeFilter).ToString());
            }
            if (filters.CategoryFilter.Count > 0)
            {
                query = query + string.Format(@" AND C.CATEGORY_CODE IN ('{0}') ", string.Join("','", filters.CategoryFilter).ToString());
            }
            if (filters.EmployeeFilter.Count > 0)
            {
                query = query + string.Format(@" AND  A.EMPLOYEE_CODE IN  ('{0}')", string.Join("','", filters.EmployeeFilter).ToString());
            }
            if (filters.AgentFilter.Count > 0)
            {
                query = query + string.Format(@" AND  A.AGENT_CODE IN  ('{0}')", string.Join("','", filters.AgentFilter).ToString());
            }
            if (filters.DivisionFilter.Count > 0)
            {
                query = query + string.Format(@" AND A.DIVISION_CODE IN ('{0}')", string.Join("','", filters.DivisionFilter).ToString());
            }
            if (filters.DocumentFilter.Count > 0)
            {
                query = query + string.Format(@" AND  A.FORM_CODE  IN  ('{0}')", string.Join("','", filters.DocumentFilter).ToString());
            }
            if (filters.LocationFilter.Count > 0)
            {

                var locations = filters.LocationFilter;
                var locationConditionQuery = string.Empty;
                for (int i = 0; i < locations.Count; i++)
                {

                    if (i == 0)
                        locationConditionQuery += string.Format("SELECT LOCATION_CODE FROM IP_LOCATION_SETUP WHERE LOCATION_CODE LIKE '{0}%' ", locations[i]);
                    else
                    {
                        locationConditionQuery += string.Format(" OR LOCATION_CODE like '{0}%' ", locations[i]);
                    }
                }
                var locationsfilter = string.Empty;
                foreach (var product in locations)
                {
                    locationsfilter += $@"'{product}',";
                }
                locationsfilter = locationsfilter.Remove(locationsfilter.Length - 1);
                query = query + string.Format(@" AND A.FROM_LOCATION_CODE IN ({0} OR LOCATION_CODE IN ('{1}'))", locationConditionQuery, locationsfilter);
            }
            if (filters.BranchFilter.Count > 0)
            {
                query = query + string.Format(@" AND A.BRANCH_CODE IN ('{0}')", string.Join("','", filters.BranchFilter).ToString());
            }


            if (!string.IsNullOrEmpty(filters.FromDate))
                query = query + " and A.SALES_DATE>=TO_DATE('" + filters.FromDate + "', 'YYYY-MM-DD') and A.SALES_DATE <= TO_DATE('" + filters.ToDate + "', 'YYYY-MM-DD')";
            query = query + "GROUP BY A.SALES_DATE,  A.SALES_NO, B.CUSTOMER_EDESC,  A.PARTY_TYPE_CODE, A.CUSTOMER_CODE,A.FORM_CODE,A.COMPANY_CODE,A.BRANCH_CODE";

            ReportFilterHelper.RangeFilterValue(filters.AmountRangeFilter, out min, out max);

            if (!(min == 0 && max == 0))
            {
                query = query + string.Format(@" Having  SUM(NVL(FN_CONVERT_CURRENCY(NVL(A.CALC_TOTAL_PRICE, 0), 'NRS', A.SALES_DATE), 0))/{0} >= {1} and SUM(NVL(FN_CONVERT_CURRENCY(NVL(A.CALC_TOTAL_PRICE, 0), 'NRS', A.SALES_DATE), 0))/{0} <= {2}"
                    , ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter), (decimal)min / ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter), (decimal)max / ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter));
            }


            query += " ) A ORDER BY A.SALES_DATE ASC ";


            var salesRegisters = _objectEntity.SqlQuery<SalesRegisterMasterModel>(query).ToList();


            return salesRegisters;
        }
        public List<Charges> GetSumChargesDateWise(string formDate, string toDate)
        {
            string Query = @"SELECT CS.CUSTOMER_CODE as CustomerId, CS.CUSTOMER_EDESC ,
                 CT.charge_code as CHARGE_CODE,
                 CC.CHARGE_EDESC as ,
                 SUM (CT.CHARGE_AMOUNT) CHARGE_AMOUNT,
                 CT.APPLY_ON as APPLY_ON ,
                 CT.CHARGE_TYPE_FLAG as CHARGE_TYPE_FLAG
            FROM CHARGE_TRANSACTION CT, IP_CHARGE_CODE CC, SA_SALES_INVOICE SI, SA_CUSTOMER_SETUP CS
           WHERE     1 = 1
                 AND CT.CHARGE_CODE = CC.CHARGE_CODE
                 AND CT.form_code IN (SELECT form_code
                                        FROM form_detail_setup
                                       WHERE table_name = 'SA_SALES_INVOICE')
                 AND CT.apply_on = 'D'
                  AND SI.SALES_NO = CT.REFERENCE_NO
                 AND SI.CUSTOMER_CODE = CS.CUSTOMER_CODE";
            if (string.IsNullOrEmpty(formDate))
                Query = Query + "  AND CT.CREATED_DATE >= TO_DATE ('" + formDate + "', 'YYYY-MM-DD') AND CT.CREATED_DATE <= TO_DATE ('" + toDate + "', 'YYYY-MM-DD')";
            Query = Query + " GROUP BY CS.CUSTOMER_CODE,CS.CUSTOMER_EDESC,CT.charge_code,CC.CHARGE_EDESC,CT.APPLY_ON, CT.CHARGE_TYPE_FLAG ORDER BY CS.CUSTOMER_EDESC";

            var charges = _objectEntity.SqlQuery<Charges>(Query).ToList();
            return charges;
        }
        public List<Charges> GetSumCharges(ReportFiltersModel filters)
        {

            // var companyCode = string.Join(",", filters.CompanyFilter);

            //  if (string.IsNullOrEmpty(companyCode))
            //    companyCode = this._workContext.CurrentUserinformation.company_code;

            var companyCode = string.Empty;
            foreach (var company in filters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }

            companyCode = companyCode == "" ? $@"'{this._workContext.CurrentUserinformation.company_code}'" : companyCode.Remove(companyCode.Length - 1);

            string Query = string.Format(@"SELECT CS.CUSTOMER_CODE as CustomerId, CS.CUSTOMER_EDESC ,
                 CT.charge_code||CT.APPLY_ON as charge_code,  CC.CHARGE_EDESC ||'(' || CT.APPLY_ON ||')' as CHARGE_EDESC,
                 NVL(Round(SUM (NVL(FN_CONVERT_CURRENCY(NVL(CT.CHARGE_AMOUNT,0),'NRS', SI.SALES_DATE),0))/{0},{1}),0) as CHARGE_AMOUNT,
                 CT.APPLY_ON as APPLY_ON ,
                 CT.CHARGE_TYPE_FLAG as CHARGE_TYPE_FLAG
            FROM CHARGE_TRANSACTION CT, IP_CHARGE_CODE CC, SA_SALES_INVOICE SI, SA_CUSTOMER_SETUP CS
           WHERE     1 = 1
   AND SI.SERIAL_NO = 1
                 AND CT.CHARGE_CODE = CC.CHARGE_CODE 
                 AND CT.COMPANY_CODE=CC.COMPANY_CODE
                 AND CT.COMPANY_CODE=SI.COMPANY_CODE
                 AND CT.COMPANY_CODE=CS.COMPANY_CODE
                 AND CT.CHARGE_CODE = CC.CHARGE_CODE AND CT.COMPANY_CODE IN({2})
                 AND CT.form_code IN (SELECT form_code
                                        FROM form_detail_setup
                                       WHERE table_name = 'SA_SALES_INVOICE')
                 AND CT.apply_on = 'D'
                  AND SI.SALES_NO = CT.REFERENCE_NO
                 AND SI.CUSTOMER_CODE = CS.CUSTOMER_CODE", ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter), ReportFilterHelper.FigureFilterValue(filters.AmountRoundUpFilter), companyCode);
            if (!string.IsNullOrEmpty(filters.FromDate))
                Query = Query + string.Format(@"  AND CT.CREATED_DATE >= TO_DATE ('{0}', 'YYYY-MM-DD') AND CT.CREATED_DATE <= TO_DATE ('{1}', 'YYYY-MM-DD')", filters.FromDate, filters.ToDate);
            Query = Query + " GROUP BY CS.CUSTOMER_CODE,CS.CUSTOMER_EDESC,CT.charge_code,CC.CHARGE_EDESC,CT.APPLY_ON, CT.CHARGE_TYPE_FLAG ORDER BY CS.CUSTOMER_EDESC";

            var charges = _objectEntity.SqlQuery<Charges>(Query).ToList();
            return charges;
        }
        public List<SalesRegistersDetail> GetSalesRegisterDateWise(string formDate, string toDate)
        {
            string query = @"SELECT sales_date as SalesDate,
       bs_date (sales_date) Miti ,
       sales_no as InvoiceNumber,
       INITCAP (CS.CUSTOMER_EDESC) CustomerName,
       INITCAP (IMS.ITEM_EDESC) ItemName,
       INITCAP (ls.location_edesc) LocationName,
       SI.MANUAL_NO as ManualNo,
       SI.REMARKS as REMARKS,
       INITCAP (ES.EMPLOYEE_EDESC) Dealer,
       INITCAP (PTC.PARTY_TYPE_EDESC) PartyType,
       SI.SHIPPING_ADDRESS as ShippingAddress,
       SI.SHIPPING_CONTACT_NO as ShippingContactNo,
       INITCAP (MC.MU_EDESC) Unit ,
       SI.QUANTITY as Quantity,
       SI.UNIT_PRICE as UnitPrice,
       SI.TOTAL_PRICE as TotalPrice
  FROM SA_SALES_INVOICE si,
       IP_ITEM_MASTER_SETUP ims,
       SA_CUSTOMER_SETUP cs,
       IP_LOCATION_SETUP ls,
       IP_MU_CODE mc,
       HR_EMPLOYEE_SETUP es,
       IP_PARTY_TYPE_CODE ptc
WHERE     SI.ITEM_CODE = IMS.ITEM_CODE
       AND SI.CUSTOMER_CODE = CS.CUSTOMER_CODE
       AND SI.FROM_LOCATION_CODE = LS.LOCATION_CODE
       AND SI.MU_CODE = MC.MU_CODE
  and si.Deleted_flag='N'
      AND SI.EMPLOYEE_CODE = ES.EMPLOYEE_CODE
       AND SI.PARTY_TYPE_CODE = PTC.PARTY_TYPE_CODE(+)";
            if (!string.IsNullOrEmpty(formDate))
                query = query + " and SALES_DATE>=TO_DATE('" + formDate + "', 'YYYY-MM-DD') and SALES_DATE <= TO_DATE('" + toDate + "', 'YYYY-MM-DD')";
            var salesRegisters = _objectEntity.SqlQuery<SalesRegistersDetail>(query).ToList();
            return salesRegisters;
        }
        public List<SalesRegistersDetail> GetSalesRegister(ReportFiltersModel filters, NeoErp.Core.Domain.User userinfo)
        {
            var companyCode = string.Empty;
            foreach (var company in filters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }
            companyCode = companyCode == "" ? $@"'{userinfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);
            string query = string.Format(@"SELECT TO_CHAR(sales_date) as SalesDate,
                       bs_date (sales_date) Miti ,
                       sales_no as InvoiceNumber,
                       INITCAP (CS.CUSTOMER_EDESC) CustomerName,
                       INITCAP (IMS.ITEM_EDESC) ItemName,
                       INITCAP (ls.location_edesc) LocationName,
                       NVL(SI.MANUAL_NO,'n/a') as ManualNo,
                       NVL(SI.REMARKS,'n/a') as REMARKS,
                       NVL(INITCAP (ES.EMPLOYEE_EDESC),'n/a') Dealer,
                       NVL(INITCAP (PTC.PARTY_TYPE_EDESC),'n/a') PartyType,
                       SI.SHIPPING_ADDRESS as SHIPPINGCODE,
                       NVL(SI.SHIPPING_CONTACT_NO,'-') as ShippingContactNo,
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
                       HR_EMPLOYEE_SETUP es,
                       IP_PARTY_TYPE_CODE ptc,
                             CITY_CODE ct,
                        AREA_SETUP ast
                       WHERE SI.ITEM_CODE = IMS.ITEM_CODE
                        and si.company_code=IMS.company_code
                        and si.AREA_CODE = ast.AREA_CODE
                       and SI.COMPANY_CODE=cs.company_code
                       and SI.company_code=ls.company_code
                          and  SI.SHIPPING_ADDRESS= ct.city_code
                       AND si.COMPANY_CODE IN(" + companyCode + @") AND si.CUSTOMER_CODE = cs.CUSTOMER_CODE"
                     , ReportFilterHelper.FigureFilterValue(filters.QuantityFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.QuantityRoundUpFilter), ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter)
                        , ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter));
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
                        customerConditionQuery += string.Format(" OR  MASTER_CUSTOMER_CODE like (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G')", customers[i], companyCode);
                    }
                }

                query = query + string.Format(@" AND SI.CUSTOMER_CODE IN (SELECT DISTINCT(CUSTOMER_CODE) FROM SA_CUSTOMER_SETUP WHERE  {0} OR (CUSTOMER_CODE IN ('{1}') AND GROUP_SKU_FLAG='I')) ", customerConditionQuery, string.Join("','", customers));
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

                query = query + string.Format(@" AND SI.ITEM_CODE IN (SELECT DISTINCT(ITEM_CODE) FROM IP_ITEM_MASTER_SETUP WHERE {0} OR (ITEM_CODE IN ('{1}') AND GROUP_SKU_FLAG='I')) ", productConditionQuery, string.Join("','", products));

                //query = query + string.Format(@" AND SI.ITEM_CODE IN ({0})", string.Join(",", filters.ProductFilter).ToString());
            }
            if (filters.DocumentFilter.Count > 0)
            {
                query = query + string.Format(@" AND  SI. FORM_CODE  IN  ('{0}')", string.Join("','", filters.DocumentFilter).ToString());
            }
            if (filters.PartyTypeFilter.Count > 0)
            {
                query = query + string.Format(@" AND SI.PARTY_TYPE_CODE IN ('{0}') ", string.Join("','", filters.PartyTypeFilter).ToString());
            }
            if (filters.AreaTypeFilter.Count > 0)
            {
                query = query + string.Format(@" AND SI.AREA_CODE IN ('{0}') ", string.Join("','", filters.AreaTypeFilter).ToString());
            }
            if (filters.CategoryFilter.Count > 0)
            {
                query = query + string.Format(@" AND ims.CATEGORY_CODE IN ('{0}') ", string.Join("','", filters.CategoryFilter).ToString());
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
            //EMPLOYEE_CODE doesn't exist on the NNPL &WLINK database , So We have manually created column of EMPLOYEE_CODE for the both database .
            query = query + @" AND SI.FROM_LOCATION_CODE = LS.LOCATION_CODE
                        and si.Deleted_flag = 'N'
                      AND SI.EMPLOYEE_CODE = ES.EMPLOYEE_CODE(+)
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


            var salesRegisters = _objectEntity.SqlQuery<SalesRegistersDetail>(query).ToList();
            return salesRegisters;
        }
        public List<SalesRegistersDetail> GetSalesRegisterDateWisePaging(string formDate, string toDate, int pageSize, int pageNumber)
        {
            string query = @" select *
                        from (
                                select rownum as rn, a.*
                                from (
                                      SELECT sales_date as SalesDate,
                               bs_date (sales_date) Miti ,
                               sales_no as InvoiceNumber,
                               INITCAP (CS.CUSTOMER_EDESC) CustomerName,
                               INITCAP (IMS.ITEM_EDESC) ItemName,
                               INITCAP (ls.location_edesc) LocationName,
                               SI.MANUAL_NO as ManualNo,
                               SI.REMARKS as REMARKS,
                               INITCAP (ES.EMPLOYEE_EDESC) Dealer,
                               INITCAP (PTC.PARTY_TYPE_EDESC) PartyType,
                               SI.SHIPPING_ADDRESS as ShippingAddress,
                               SI.SHIPPING_CONTACT_NO as ShippingContactNo,
                               INITCAP (MC.MU_EDESC) Unit ,
                               SI.QUANTITY as Quantity,
                               SI.UNIT_PRICE as UnitPrice,
                               SI.TOTAL_PRICE as TotalPrice
                          FROM SA_SALES_INVOICE si,
                               IP_ITEM_MASTER_SETUP ims,
                               SA_CUSTOMER_SETUP cs,
                               IP_LOCATION_SETUP ls,
                               IP_MU_CODE mc,
                               HR_EMPLOYEE_SETUP es,
                               IP_PARTY_TYPE_CODE ptc
                        WHERE     SI.ITEM_CODE = IMS.ITEM_CODE
                               AND SI.CUSTOMER_CODE = CS.CUSTOMER_CODE
                               AND SI.FROM_LOCATION_CODE = LS.LOCATION_CODE
                               AND SI.MU_CODE = MC.MU_CODE
                               and si.Deleted_flag='N'
                               AND SI.EMPLOYEE_CODE = ES.EMPLOYEE_CODE
                               AND SI.PARTY_TYPE_CODE = PTC.PARTY_TYPE_CODE(+)";
            if (!string.IsNullOrEmpty(formDate))
                query = query + " and SALES_DATE>=TO_DATE('" + formDate + "', 'YYYY-MM-DD') and SALES_DATE <= TO_DATE('" + toDate + "', 'YYYY-MM-DD')";
            query = query + @") a)
                        where rownum <= " + pageSize + " and rn > (" + pageNumber + " - 1)*" + pageSize + "";
            var salesRegisters = _objectEntity.SqlQuery<SalesRegistersDetail>(query).ToList();
            return salesRegisters;
        }
        public int TotalSalesRegister(string formDate, string toDate)
        {
            string query = @"select count(sales_date) from SA_SALES_INVOICE";
            if (!string.IsNullOrEmpty(formDate))
                query = query + " where SALES_DATE>=TO_DATE('" + formDate + "', 'YYYY-MM-DD') and SALES_DATE <= TO_DATE('" + toDate + "', 'YYYY-MM-DD')";
            var salesRegisters = _objectEntity.SqlQuery<int>(query).First();
            return salesRegisters;
        }
        public List<ChartSalesModel> GetCategorySales()
        {
            string query = @" SELECT IPC.CATEGORY_CODE Code,IPC.CATEGORY_EDESC DESCRIPTION, SUM(SSI.CALC_TOTAL_PRICE) TOTAL , SUM(SSI.CALC_QUANTITY) QUANTITY FROM IP_CATEGORY_CODE  IPC
                                  INNER JOIN  IP_ITEM_MASTER_SETUP IIMS on IPC.CATEGORY_CODE = IIMS.CATEGORY_CODE
                                  INNER JOIN SA_SALES_INVOICE SSI on SSI.ITEM_CODE = IIMS.ITEM_CODE WHERE IPC.DELETED_FLAG = 'N'
                                  GROUP BY IPC.CATEGORY_CODE, IPC.CATEGORY_EDESC";

            var categorySales = _objectEntity.SqlQuery<ChartSalesModel>(query);

            return categorySales.ToList();

        }
        public List<ChartSalesModel> GetAreasSales()
        {
            string query = @"  SELECT * FROM (select A.AREA_CODE Code,A.AREA_EDESC DESCRIPTION, ROUND(SUM(SA.NET_SALES_RATE),2) TOTAL, ROUND(SUM(SA.CALC_QUANTITY),2) QUANTITY from SA_SALES_INVOICE SA,AREA_SETUP A
                                  WHERE A.AREA_CODE=SA.AREA_CODE
                                  AND A.COMPANY_CODE=SA.COMPANY_CODE
                                  AND SA.DELETED_FLAG='N'
                                  GROUP BY A.AREA_CODE,A.AREA_EDESC) AR ORDER BY TOTAL DESC";

            var categorySales = _objectEntity.SqlQuery<ChartSalesModel>(query);

            return categorySales.ToList();

        }


        public List<ChartSalesModel> GetProductSalesByMonth(ReportFiltersModel reportFilters, User userinfo, string dateFormat, string month)
        {
            var companyCode = string.Join(",", reportFilters.CompanyFilter);
            companyCode = companyCode == "" ? this._workContext.CurrentUserinformation.company_code : companyCode;

            string query = string.Empty;
            if (dateFormat == "AD")
            {
                query = string.Format(@"SELECT IIMS.ITEM_CODE Code, IIMS.ITEM_EDESC DESCRIPTION,
                            SUM (nvl(SSI.CALC_TOTAL_PRICE,0))/{0} GrossAmount , 
                           SUM (nvl(SSI.CALC_QUANTITY,0))/{0} QUANTITY FROM
                           IP_ITEM_MASTER_SETUP IIMS 
                          INNER JOIN SA_SALES_INVOICE SSI on SSI.ITEM_CODE = IIMS.ITEM_CODE  and ssi.Company_code=iims.company_code
                          WHERE IIMS.DELETED_FLAG = 'N' AND TO_CHAR(SSI.sales_date, 'YYYYMM') = '{1}'
                          and ssi.company_code IN({2})
                          GROUP BY IIMS.ITEM_CODE, IIMS.ITEM_EDESC", ReportFilterHelper.FigureFilterValue(reportFilters.AmountFigureFilter), month, companyCode);
            }
            else
            {
                query = string.Format(@"SELECT IIMS.ITEM_CODE Code, IIMS.ITEM_EDESC DESCRIPTION,
                            SUM (nvl(SSI.CALC_TOTAL_PRICE,0))/{0} GrossAmount , 
                           SUM (nvl(SSI.CALC_QUANTITY,0))/{0} QUANTITY FROM
                           IP_ITEM_MASTER_SETUP IIMS 
                          INNER JOIN SA_SALES_INVOICE SSI on SSI.ITEM_CODE = IIMS.ITEM_CODE  and ssi.Company_code=iims.company_code
                          WHERE IIMS.DELETED_FLAG = 'N' AND SUBSTR(BS_DATE(SSI.sales_date),6,2) = '{1}'
                          and ssi.company_code IN({2})
                          GROUP BY IIMS.ITEM_CODE, IIMS.ITEM_EDESC", ReportFilterHelper.FigureFilterValue(reportFilters.AmountFigureFilter), month, companyCode);
            }



            var productSales = _objectEntity.SqlQuery<ChartSalesModel>(query);

            return productSales.ToList();
        }



        public List<ChartSalesModel> GetProductSalesByCategory(ReportFiltersModel reportFilters, User userinfo, string categoryCode)
        {
            var companyCode = string.Join(",", reportFilters.CompanyFilter);
            companyCode = companyCode == null ? this._workContext.CurrentUserinformation.company_code : companyCode;

            string query = string.Format(@"SELECT IIMS.ITEM_CODE Code, IIMS.ITEM_EDESC DESCRIPTION,
                            SUM (nvl(SSI.CALC_TOTAL_PRICE,0))/{0} TOTAL , 
                           SUM (nvl(SSI.CALC_QUANTITY,0))/{0} QUANTITY FROM
                           IP_ITEM_MASTER_SETUP IIMS 
                          INNER JOIN SA_SALES_INVOICE SSI on SSI.ITEM_CODE = IIMS.ITEM_CODE 
                          WHERE IIMS.DELETED_FLAG = 'N' AND IIMS.CATEGORY_CODE= '{1}'
                          and ssi.company_code IN({2})
                          GROUP BY IIMS.ITEM_CODE, IIMS.ITEM_EDESC", ReportFilterHelper.FigureFilterValue(reportFilters.AmountFigureFilter), categoryCode, companyCode);

            var productSales = _objectEntity.SqlQuery<ChartSalesModel>(query);

            return productSales.ToList();
        }

        public List<ChartSalesModel> GetProductSalesByCategory(ReportFiltersModel reportFilters, User userInfo, string categoryCode, string customerCode, string itemCode, string categoryCode2, string companyCode, string branchCode, string partyTypeCode, string formCode)
        {

            companyCode = string.Join(",", reportFilters.CompanyFilter);
            companyCode = companyCode == "" ? this._workContext.CurrentUserinformation.company_code : companyCode;

            string Query = string.Format(@"SELECT  IM.ITEM_CODE Code, IM.ITEM_EDESC DESCRIPTION,
                                                    SUM(nvl(SI.CALC_TOTAL_PRICE,0))/{0} TOTAL , 
                                                    SUM(nvl(SI.CALC_QUANTITY,0))/1 QUANTITY 
                                            FROM SA_SALES_INVOICE SI                         
                                            INNER JOIN  IP_ITEM_MASTER_SETUP IM on SI.ITEM_CODE = IM.ITEM_CODE                         
                                            WHERE SI.DELETED_FLAG = 'N' AND IM.DELETED_FLAG= 'N'                           
                                              AND IM.ITEM_CODE = SI.ITEM_CODE  
                                              AND IM.COMPANY_CODE = SI.COMPANY_CODE
                                              AND SI.company_code IN({2}) 
                                              AND IM.CATEGORY_CODE= '{1}'"
                                             , ReportFilterHelper.FigureFilterValue(reportFilters.AmountFigureFilter), categoryCode, companyCode);


            //for customer Filter
            var customerFilter = string.Empty;
            if (reportFilters.CustomerFilter.Count() > 0)
            {
                customerFilter = @"select  DISTINCT(customer_code) from sa_customer_setup where (";
                //IF CUSTOMER_SKU_FLAG = G
                foreach (var item in reportFilters.CustomerFilter)
                {
                    customerFilter += "master_customer_code like  (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + companyCode + ")) OR ";
                }
                customerFilter = customerFilter.Substring(0, customerFilter.Length - 3);
                //IF CUSTOMER_SKU_FLAG = I                
                customerFilter += " or (customer_code in (" + string.Join(",", reportFilters.CustomerFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + companyCode + "))) ";


                Query += " and SI.customer_code IN(" + customerFilter + ")";
            }




            ////for Product Filter
            var productFilter = string.Empty;
            if (reportFilters.ProductFilter.Count() > 0)
            {
                productFilter = @"select  DISTINCT item_code from IP_ITEM_MASTER_SETUP where (";
                //IF PRODUCT_SKU_FLAG = G
                foreach (var item in reportFilters.ProductFilter)
                {
                    productFilter += "MASTER_ITEM_CODE like  (Select DISTINCT MASTER_ITEM_CODE || '%'  from IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + companyCode + ")) OR ";
                }
                productFilter = productFilter.Substring(0, productFilter.Length - 3);
                //IF PRODUCT_SKU_FLAG = I                
                productFilter += " or (ITEM_CODE in (" + string.Join(",", reportFilters.ProductFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + companyCode + "))) ";


                Query += " and SI.ITEM_CODE IN(" + productFilter + ")";
            }




            //FOR BRANCH FILTER
            if (reportFilters.BranchFilter.Count() > 0)
            {
                Query += " AND SI.BRANCH_CODE IN (" + string.Join(",", reportFilters.BranchFilter) + ")";

            }
            //For Area Filter
            if (reportFilters.AreaTypeFilter.Count > 0)
            {
                Query += string.Format(@" AND SI.AREA_CODE IN ('{0}')", string.Join("','", reportFilters.AreaTypeFilter).ToString());
            }
            if (reportFilters.EmployeeFilter.Count > 0)
            {
                Query = Query + string.Format(@" AND SI.EMPLOYEE_CODE IN  ('{0}')", string.Join("','", reportFilters.EmployeeFilter).ToString());
            }
            if (reportFilters.AgentFilter.Count > 0)
            {
                Query = Query + string.Format(@" AND SI.AGENT_CODE IN  ('{0}')", string.Join("','", reportFilters.AgentFilter).ToString());
            }
            string locationFilter = string.Empty;
            if (reportFilters.LocationFilter.Count > 0)
            {

                var locations = reportFilters.LocationFilter;
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
                Query = Query + locationFilter;
            }

            Query += " GROUP BY IM.ITEM_CODE , IM.ITEM_EDESC";

            //  Query += string.Format(Query, ReportFilterHelper.FigureFilterValue(reportFilters.AmountFigureFilter));
            var productSales = _objectEntity.SqlQuery<ChartSalesModel>(Query);

            return productSales.ToList();
        }

        public List<ChartSalesModel> GetStockLevelByCategory(ReportFiltersModel reportFilters, User userInfo, string categoryCode, string customerCode, string itemCode, string categoryCode2, string companyCode, string branchCode, string partyTypeCode, string formCode)
        {

            companyCode = string.Empty;
            foreach (var company in reportFilters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }

            companyCode = companyCode == "" ? $@"'{userInfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);
            var FiscalYearCode = string.Empty;
            if(reportFilters.FiscalYearFilter.Count>0)
            {
                FiscalYearCode = $"{reportFilters.FiscalYearFilter.FirstOrDefault().DBName}.";
            }

            var query = $@"select ITEM_CODE Code,ITEM_EDESC Description,stock_value Total from(  select ITEM_CODE, ITEM_EDESC,ROUND(SUM({FiscalYearCode}FN_AVG_RATE_INFO (company_code, branch_code ,ITEM_CODE,sysdate) * QTY),2) stock_value from (
                       select VI.ITEM_CODE,IP.ITEM_EDESC, VI.company_code, VI.branch_code, sum(VI.IN_QUANTITY -  VI.OUT_QUANTITY ) QTY
                        from {FiscalYearCode}V$VIRTUAL_STOCK_WIP_LEDGER VI , {FiscalYearCode}IP_ITEM_MASTER_SETUP  IP 
                       WHERE  VI.COMPANY_CODE=IP.COMPANY_CODE
                       AND VI.ITEM_CODE=IP.ITEM_CODE
                       AND IP.GROUP_SKU_FLAG='I'
                       AND VI.DELETED_FLAG='N'
                       AND VI.BRANCH_CODE=IP.BRANCH_CODE
                        AND VI.COMPANY_CODE IN({companyCode})  
                        AND IP.CATEGORY_CODE='{categoryCode}'";
            ////for Product Filter
            var productFilter = string.Empty;
            if (reportFilters.ProductFilter.Count() > 0)
            {
                productFilter = $@"select  DISTINCT item_code from {FiscalYearCode}IP_ITEM_MASTER_SETUP where (";
                //IF PRODUCT_SKU_FLAG = G
                foreach (var item in reportFilters.ProductFilter)
                {
                    productFilter += $"MASTER_ITEM_CODE like  (Select DISTINCT MASTER_ITEM_CODE || '%'  from {FiscalYearCode}IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + companyCode + ")) OR ";
                }
                productFilter = productFilter.Substring(0, productFilter.Length - 3);
                //IF PRODUCT_SKU_FLAG = I                
                productFilter += " or (ITEM_CODE in (" + string.Join(",", reportFilters.ProductFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + companyCode + "))) ";


                query += " and VI.ITEM_CODE IN(" + productFilter + ")";
            }





            query += @" group by VI.item_code,IP.ITEM_EDESC, VI.company_code, VI.branch_code) 
                        group by ITEM_CODE, ITEM_EDESC) where stock_value<>0";

            //  Query += string.Format(Query, ReportFilterHelper.FigureFilterValue(reportFilters.AmountFigureFilter));
            var productSales = _objectEntity.SqlQuery<ChartSalesModel>(query);

            return productSales.ToList();
        }
        public List<ChartSalesModel> GetProductSalesByArea(ReportFiltersModel reportFilters, User userInfo, string categoryCode, string customerCode, string itemCode, string categoryCode2, string companyCode, string branchCode, string partyTypeCode, string formCode)
        {

            companyCode = string.Join(",", reportFilters.CompanyFilter);
            companyCode = companyCode == "" ? this._workContext.CurrentUserinformation.company_code : companyCode;

            string Query = string.Format($@"SELECT  IM.PARTY_TYPE_CODE Code, IM.PARTY_TYPE_EDESC DESCRIPTION,
                                                    SUM(nvl(SI.CALC_TOTAL_PRICE,0))/{ReportFilterHelper.FigureFilterValue(reportFilters.AmountFigureFilter)} TOTAL , 
                                                    SUM(nvl(SI.CALC_QUANTITY,0))/{ReportFilterHelper.FigureFilterValue(reportFilters.AmountFigureFilter)} QUANTITY 
                                            FROM SA_SALES_INVOICE SI                         
                                            INNER JOIN  IP_PARTY_TYPE_CODE IM on SI.party_type_code = IM.PARTY_TYPE_CODE                         
                                            WHERE SI.DELETED_FLAG = 'N' AND IM.DELETED_FLAG= 'N'                           
                                              AND IM.PARTY_TYPE_CODE = SI.PARTY_TYPE_CODE  
                                              AND IM.COMPANY_CODE = SI.COMPANY_CODE
                                              AND SI.company_code IN({companyCode}) 
                                              AND SI.AREA_CODE= '{categoryCode}'");


            //for customer Filter
            var customerFilter = string.Empty;
            if (reportFilters.CustomerFilter.Count() > 0)
            {
                customerFilter = @"select  DISTINCT(customer_code) from sa_customer_setup where (";
                //IF CUSTOMER_SKU_FLAG = G
                foreach (var item in reportFilters.CustomerFilter)
                {
                    customerFilter += "master_customer_code like  (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + companyCode + ")) OR ";
                }
                customerFilter = customerFilter.Substring(0, customerFilter.Length - 3);
                //IF CUSTOMER_SKU_FLAG = I                
                customerFilter += " or (customer_code in (" + string.Join(",", reportFilters.CustomerFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + companyCode + "))) ";


                Query += " and SI.customer_code IN(" + customerFilter + ")";
            }




            ////for Product Filter
            var productFilter = string.Empty;
            if (reportFilters.ProductFilter.Count() > 0)
            {
                productFilter = @"select  DISTINCT item_code from IP_ITEM_MASTER_SETUP where (";
                //IF PRODUCT_SKU_FLAG = G
                foreach (var item in reportFilters.ProductFilter)
                {
                    productFilter += "MASTER_ITEM_CODE like  (Select DISTINCT MASTER_ITEM_CODE || '%'  from IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + companyCode + ")) OR ";
                }
                productFilter = productFilter.Substring(0, productFilter.Length - 3);
                //IF PRODUCT_SKU_FLAG = I                
                productFilter += " or (ITEM_CODE in (" + string.Join(",", reportFilters.ProductFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + companyCode + "))) ";


                Query += " and SI.ITEM_CODE IN(" + productFilter + ")";
            }




            //FOR BRANCH FILTER
            if (reportFilters.BranchFilter.Count() > 0)
            {
                Query += " AND SI.BRANCH_CODE IN (" + string.Join(",", reportFilters.BranchFilter) + ")";

            }
            //FOR AREA FILTER
            if (reportFilters.AreaTypeFilter.Count() > 0)
            {
                Query += " AND SI.AREA_CODE IN (" + string.Join(",", reportFilters.AreaTypeFilter) + ")";

            }
            if (reportFilters.EmployeeFilter.Count > 0)
            {
                Query = Query + string.Format(@" AND SI.EMPLOYEE_CODE IN  ('{0}')", string.Join("','", reportFilters.EmployeeFilter).ToString());
            }
            if (reportFilters.AgentFilter.Count > 0)
            {
                Query = Query + string.Format(@" AND SI.AGENT_CODE IN  ('{0}')", string.Join("','", reportFilters.AgentFilter).ToString());
            }
            string locationFilter = string.Empty;
            if (reportFilters.LocationFilter.Count > 0)
            {

                var locations = reportFilters.LocationFilter;
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
                Query = Query + locationFilter;
            }

            Query += " GROUP BY IM.PARTY_TYPE_CODE , IM.PARTY_TYPE_EDESC";

            //  Query += string.Format(Query, ReportFilterHelper.FigureFilterValue(reportFilters.AmountFigureFilter));
            var productSales = _objectEntity.SqlQuery<ChartSalesModel>(Query);

            return productSales.ToList();
        }

        public List<ChartSalesModel> GetProductSalesByAreaEmployee(ReportFiltersModel reportFilters, User userInfo, string categoryCode, string customerCode, string itemCode, string categoryCode2, string companyCode, string branchCode, string partyTypeCode, string formCode)
        {

            companyCode = string.Join(",", reportFilters.CompanyFilter);
            companyCode = companyCode == "" ? this._workContext.CurrentUserinformation.company_code : companyCode;

            string Query = string.Format($@"SELECT  IM.employee_code Code, IM.employee_edesc DESCRIPTION,
                                                    SUM(nvl(SI.CALC_TOTAL_PRICE,0))/{ReportFilterHelper.FigureFilterValue(reportFilters.AmountFigureFilter)} TOTAL , 
                                                    SUM(nvl(SI.CALC_QUANTITY,0))/{ReportFilterHelper.FigureFilterValue(reportFilters.AmountFigureFilter)} QUANTITY 
                                            FROM SA_SALES_INVOICE SI                         
                                            INNER JOIN  HR_EMPLOYEE_SETUP IM on SI.employee_code = IM.employee_code                         
                                            WHERE SI.DELETED_FLAG = 'N' AND IM.DELETED_FLAG= 'N'                           
                                              AND IM.employee_code = SI.employee_code  
                                              AND IM.COMPANY_CODE = SI.COMPANY_CODE
                                              AND SI.company_code IN({companyCode}) 
                                              AND SI.AREA_CODE= '{categoryCode}'");


            //for customer Filter
            //var customerFilter = string.Empty;
            //if (reportFilters.CustomerFilter.Count() > 0)
            //{
            //    customerFilter = @"select  DISTINCT(customer_code) from sa_customer_setup where (";
            //    //IF CUSTOMER_SKU_FLAG = G
            //    foreach (var item in reportFilters.CustomerFilter)
            //    {
            //        customerFilter += "master_customer_code like  (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + companyCode + ")) OR ";
            //    }
            //    customerFilter = customerFilter.Substring(0, customerFilter.Length - 3);
            //    //IF CUSTOMER_SKU_FLAG = I                
            //    customerFilter += " or (customer_code in (" + string.Join(",", reportFilters.CustomerFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + companyCode + "))) ";


            //    Query += " and SI.customer_code IN(" + customerFilter + ")";
            //}




            ////for Product Filter
            var productFilter = string.Empty;
            if (reportFilters.ProductFilter.Count() > 0)
            {
                productFilter = @"select  DISTINCT item_code from IP_ITEM_MASTER_SETUP where (";
                //IF PRODUCT_SKU_FLAG = G
                foreach (var item in reportFilters.ProductFilter)
                {
                    productFilter += "MASTER_ITEM_CODE like  (Select DISTINCT MASTER_ITEM_CODE || '%'  from IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + companyCode + ")) OR ";
                }
                productFilter = productFilter.Substring(0, productFilter.Length - 3);
                //IF PRODUCT_SKU_FLAG = I                
                productFilter += " or (ITEM_CODE in (" + string.Join(",", reportFilters.ProductFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + companyCode + "))) ";


                Query += " and SI.ITEM_CODE IN(" + productFilter + ")";
            }




            //FOR BRANCH FILTER
            if (reportFilters.BranchFilter.Count() > 0)
            {
                Query += " AND SI.BRANCH_CODE IN (" + string.Join(",", reportFilters.BranchFilter) + ")";

            }
            //FOR AREA FILTER
            if (reportFilters.AreaTypeFilter.Count() > 0)
            {
                Query += " AND SI.AREA_CODE IN (" + string.Join(",", reportFilters.AreaTypeFilter) + ")";

            }
            if (reportFilters.EmployeeFilter.Count > 0)
            {
                Query = Query + string.Format(@" AND SI.EMPLOYEE_CODE IN  ('{0}')", string.Join("','", reportFilters.EmployeeFilter).ToString());
            }
            //if (reportFilters.AgentFilter.Count > 0)
            //{
            //    Query = Query + string.Format(@" AND SI.AGENT_CODE IN  ('{0}')", string.Join("','", reportFilters.AgentFilter).ToString());
            //}
            //string locationFilter = string.Empty;
            //if (reportFilters.LocationFilter.Count > 0)
            //{

            //    var locations = reportFilters.LocationFilter;
            //    for (int i = 0; i < locations.Count; i++)
            //    {

            //        if (i == 0)
            //            locationFilter += string.Format("SELECT LOCATION_CODE FROM IP_LOCATION_SETUP WHERE LOCATION_CODE LIKE '{0}%' ", locations[i]);
            //        else
            //        {
            //            locationFilter += string.Format(" OR LOCATION_CODE like '{0}%' ", locations[i]);
            //        }
            //    }
            //    locationFilter = string.Format(@" AND SI.FROM_LOCATION_CODE IN ({0} OR LOCATION_CODE IN ('{1}'))", locationFilter, string.Join("','", locations));
            //    //query = query.AppendFormat(@" AND A.FROM_LOCATION_CODE IN ('{0}')", string.Join("','", filters.LocationFilter).ToString());
            //    Query = Query + locationFilter;
            //}

            Query += " GROUP BY  IM.employee_code , IM.employee_edesc order by IM.employee_edesc";

            //  Query += string.Format(Query, ReportFilterHelper.FigureFilterValue(reportFilters.AmountFigureFilter));
            var productSales = _objectEntity.SqlQuery<ChartSalesModel>(Query);

            return productSales.ToList();
        }

        public List<ChartSalesModel> GetCategorySales(ReportFiltersModel reportFilters, User userInfo)
        {
            var companyCode = string.Join(",", reportFilters.CompanyFilter);
            companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            string query = string.Format(@"SELECT  IPC.CATEGORY_CODE Code,
                                    IPC.CATEGORY_EDESC DESCRIPTION, 
                                    SUM(nvl(SSI.CALC_TOTAL_PRICE,0))/{0} TOTAL , 
                                    SUM(nvl(SSI.CALC_QUANTITY,0))/{0} QUANTITY 
                            FROM IP_CATEGORY_CODE  IPC
                            INNER JOIN  IP_ITEM_MASTER_SETUP IIMS on IPC.CATEGORY_CODE = IIMS.CATEGORY_CODE
                            INNER JOIN SA_SALES_INVOICE SSI on SSI.ITEM_CODE = IIMS.ITEM_CODE 
                            WHERE IPC.DELETED_FLAG = 'N'
                            AND SSI.COMPANY_CODE IN({1})
                            GROUP BY IPC.CATEGORY_CODE, IPC.CATEGORY_EDESC", ReportFilterHelper.FigureFilterValue(reportFilters.AmountFigureFilter), companyCode);

            var categorySales = _objectEntity.SqlQuery<ChartSalesModel>(query);
            return categorySales.ToList();
        }


        public List<CategoryWiseSalesModel> GetCategoryStockLevel(ReportFiltersModel reportFilters, User userInfo, string customerCode, string itemCode, string categoryCode, string companyCode, string branchCode, string partyTypeCode, string formCode)
        {
            //companyCode = string.Join(",", reportFilters.CompanyFilter);
            //companyCode = companyCode == "" ? userInfo.company_code : companyCode;
            //var companyCode = string.Join(",'", reportFilters.CompanyFilter);
            companyCode = string.Empty;
            foreach (var company in reportFilters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }

            var FiscalYearCode = string.Empty;
            if (reportFilters.FiscalYearFilter.Count>0)
            {
                FiscalYearCode += $@"{reportFilters.FiscalYearFilter.FirstOrDefault().DBName}.";
            }

            companyCode = companyCode == "" ? $@"'{userInfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);
            string Query = $@" SELECT CATEGORY_CODE as Code,CATEGORY_EDESC as Description,round(STOCK_VALUE,2) as Total FROM( select CATEGORY_CODE,CATEGORY_EDESC, SUM({FiscalYearCode}FN_AVG_RATE_INFO (company_code, branch_code ,ITEM_CODE,sysdate) * QTY) stock_value from (
                       select VI.ITEM_CODE, VI.company_code, VI.branch_code,CT.CATEGORY_CODE,CT.CATEGORY_EDESC, sum(VI.IN_QUANTITY -  VI.OUT_QUANTITY ) QTY
                        from {FiscalYearCode}V$VIRTUAL_STOCK_WIP_LEDGER VI , {FiscalYearCode}IP_ITEM_MASTER_SETUP  IP 
                       ,{FiscalYearCode}IP_CATEGORY_CODE CT 
                       WHERE  VI.COMPANY_CODE=IP.COMPANY_CODE
                       AND VI.ITEM_CODE=IP.ITEM_CODE
                       AND TRIM(IP.CATEGORY_CODE)= TRIM(CT.CATEGORY_CODE)
                       AND IP.COMPANY_CODE=CT.COMPANY_CODE
                       AND IP.GROUP_SKU_FLAG='I'
                       AND VI.DELETED_FLAG='N'
                       AND VI.COMPANY_CODE=CT.COMPANY_CODE
                       AND VI.BRANCH_CODE=IP.BRANCH_CODE
                        AND VI.COMPANY_CODE IN({companyCode})";



            ////for Product Filter
            var productFilter = string.Empty;
            if (reportFilters.ProductFilter.Count() > 0)
            {
                productFilter = $@"select  DISTINCT item_code from {FiscalYearCode}IP_ITEM_MASTER_SETUP where (";
                //IF PRODUCT_SKU_FLAG = G
                foreach (var item in reportFilters.ProductFilter)
                {
                    productFilter += $"MASTER_ITEM_CODE like  (Select DISTINCT MASTER_ITEM_CODE || '%'  from {FiscalYearCode}IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + companyCode + ")) OR ";
                }
                productFilter = productFilter.Substring(0, productFilter.Length - 3);
                //IF PRODUCT_SKU_FLAG = I                
                productFilter += " or (ITEM_CODE in (" + string.Join(",", reportFilters.ProductFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + companyCode + "))) ";


                Query += " and VI.ITEM_CODE IN(" + productFilter + ")";
            }




            Query += @"  group by CT.CATEGORY_CODE,CT.CATEGORY_EDESC,VI.item_code, VI.company_code, VI.branch_code)    
                       GROUP BY     CATEGORY_CODE,CATEGORY_EDESC) WHERE STOCK_VALUE<>0 ORDER BY CATEGORY_EDESC";


            //  Query = string.Format(Query);

            var categorySales = _objectEntity.SqlQuery<CategoryWiseSalesModel>(Query).ToList();

            return categorySales.ToList();
        }
        public List<ChartSalesModel> GetCategorySales(ReportFiltersModel reportFilters, User userInfo, string customerCode, string itemCode, string categoryCode, string companyCode, string branchCode, string partyTypeCode, string formCode)
        {
            //companyCode = string.Join(",", reportFilters.CompanyFilter);
            //companyCode = companyCode == "" ? userInfo.company_code : companyCode;
            //var companyCode = string.Join(",'", reportFilters.CompanyFilter);
            companyCode = string.Empty;
            foreach (var company in reportFilters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }

            companyCode = companyCode == "" ? $@"'{userInfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);
            var FiscalYearCode = string.Empty;
            if (reportFilters.FiscalYearFilter.Count > 0)
            {
                FiscalYearCode += $@"{reportFilters.FiscalYearFilter.FirstOrDefault().DBName}.";
            }
            string Query = $@"SELECT  IPC.CATEGORY_CODE Code,
                                    IPC.CATEGORY_EDESC DESCRIPTION, 
                                    --to_number(round(SUM(NET_SALES_RATE*CALC_QUANTITY),2)) TOTAL , 
                                    --to_number(round(SUM(CALC_UNIT_PRICE*CALC_QUANTITY),2)) TOTAL ,
                                    TO_NUMBER (ROUND (SUM (CALC_UNIT_PRICE * CALC_QUANTITY), 2))-TO_NUMBER (ROUND (SUM (nvl(c.CHARGE_AMOUNT,0)), 2)) TOTAL,
                                    SUM(nvl(SI.CALC_QUANTITY,0)) QUANTITY 
                            FROM {FiscalYearCode}IP_CATEGORY_CODE  IPC                           
                            INNER JOIN  {FiscalYearCode}IP_ITEM_MASTER_SETUP IM on IPC.CATEGORY_CODE = IM.CATEGORY_CODE
                            INNER JOIN {FiscalYearCode}SA_SALES_INVOICE SI on SI.ITEM_CODE = IM.ITEM_CODE  
                            -- added by chandra for get total amount after discount
                             LEFT JOIN charge_transaction c
                            ON     SI.SALES_NO = C.REFERENCE_NO
                               AND SI.FORM_CODE = C.FORM_CODE
                               AND SI.COMPANY_CODE = C.COMPANY_CODE
                               AND CHARGE_CODE IN (SELECT DISTINCT CHARGE_CODE
                                                     FROM IP_CHARGE_CODE
                                                    WHERE SPECIFIC_CHARGE_FLAG = 'D')
                               AND APPLY_ON = 'D'
                               AND c.DELETED_FLAG = 'N'
                            --end added by chandra
                            WHERE IPC.DELETED_FLAG = 'N' AND SI.DELETED_FLAG = 'N' AND IM.DELETED_FLAG= 'N'
                              AND IPC.COMPANY_CODE = SI.COMPANY_CODE
                              AND IM.COMPANY_CODE = IPC.COMPANY_CODE
                              AND IM.ITEM_CODE = SI.ITEM_CODE     
                              AND SI.company_code IN(" + companyCode + ")";

            //for customer Filter
            var customerFilter = string.Empty;
            if (reportFilters.CustomerFilter.Count() > 0)
            {
                customerFilter = $@"select  DISTINCT(customer_code) from {FiscalYearCode}sa_customer_setup where (";
                //IF CUSTOMER_SKU_FLAG = G
                foreach (var item in reportFilters.CustomerFilter)
                {
                    customerFilter += $"master_customer_code like  (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from {FiscalYearCode}SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + companyCode + ")) OR ";
                }
                customerFilter = customerFilter.Substring(0, customerFilter.Length - 3);
                //IF CUSTOMER_SKU_FLAG = I                
                customerFilter += " or (customer_code in (" + string.Join(",", reportFilters.CustomerFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + companyCode + "))) ";


                Query += " and SI.customer_code IN(" + customerFilter + ")";
            }


            ////for Product Filter
            var productFilter = string.Empty;
            if (reportFilters.ProductFilter.Count() > 0)
            {
                productFilter = $@"select  DISTINCT item_code from {FiscalYearCode}IP_ITEM_MASTER_SETUP where (";
                //IF PRODUCT_SKU_FLAG = G
                foreach (var item in reportFilters.ProductFilter)
                {
                    productFilter += $"MASTER_ITEM_CODE like  (Select DISTINCT MASTER_ITEM_CODE || '%'  from {FiscalYearCode}IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + companyCode + ")) OR ";
                }
                productFilter = productFilter.Substring(0, productFilter.Length - 3);
                //IF PRODUCT_SKU_FLAG = I                
                productFilter += " or (ITEM_CODE in (" + string.Join(",", reportFilters.ProductFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + companyCode + "))) ";


                Query += " and SI.ITEM_CODE IN(" + productFilter + ")";
            }


            //for category Filter
            if (!string.IsNullOrEmpty(categoryCode))
            {
                Query += " and (";
                foreach (var item in categoryCode.Split(','))
                {
                    Query += "IM.CATEGORY_CODE = '" + item + "' OR ";
                }
                Query = Query.Substring(0, Query.Length - 3) + ") ";
            }




            //FOR BRANCH FILTER
            if (reportFilters.BranchFilter.Count() > 0)
            {
                Query += " AND SI.BRANCH_CODE IN (" + string.Join(",", reportFilters.BranchFilter) + ")";

            }
            if (reportFilters.AreaTypeFilter.Count > 0)
            {
                Query += string.Format(@" AND SI.AREA_CODE IN ('{0}')", string.Join("','", reportFilters.AreaTypeFilter).ToString());
            }
            if (reportFilters.EmployeeFilter.Count > 0)
            {
                Query = Query + string.Format(@" AND SI.EMPLOYEE_CODE IN  ('{0}')", string.Join("','", reportFilters.EmployeeFilter).ToString());
            }
            if (reportFilters.AgentFilter.Count > 0)
            {
                Query = Query + string.Format(@" AND SI.AGENT_CODE IN  ('{0}')", string.Join("','", reportFilters.AgentFilter).ToString());
            }
            string locationFilter = string.Empty;
            if (reportFilters.LocationFilter.Count > 0)
            {

                var locations = reportFilters.LocationFilter;
                for (int i = 0; i < locations.Count; i++)
                {

                    if (i == 0)
                        locationFilter += string.Format($"SELECT LOCATION_CODE FROM {FiscalYearCode}IP_LOCATION_SETUP WHERE LOCATION_CODE LIKE '{locations[i]}%' ");
                    else
                    {
                        locationFilter += string.Format(" OR LOCATION_CODE like '{0}%' ", locations[i]);
                    }
                }
                locationFilter = string.Format(@" AND SI.FROM_LOCATION_CODE IN ({0} OR LOCATION_CODE IN ('{1}'))", locationFilter, string.Join("','", locations));
                //query = query.AppendFormat(@" AND A.FROM_LOCATION_CODE IN ('{0}')", string.Join("','", filters.LocationFilter).ToString());
                Query = Query + locationFilter;
            }


            Query += " GROUP BY IPC.CATEGORY_CODE, IPC.CATEGORY_EDESC";


            Query = string.Format(Query);

            var categorySales = _objectEntity.SqlQuery<ChartSalesModel>(Query);

            return categorySales.ToList();
        }

        public List<ChartSalesModel> GetAreaSales(ReportFiltersModel reportFilters, User userInfo, string customerCode, string itemCode, string categoryCode, string companyCode, string branchCode, string partyTypeCode, string formCode)
        {
            companyCode = string.Empty;
            foreach (var company in reportFilters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }

            companyCode = companyCode == "" ? $@"'{userInfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);
            string Query = $@"SELECT  TO_CHAR(IPC.area_code) Code,
                                    IPC.area_edesc DESCRIPTION, 
                                   ROUND(SUM(nvl(SI.NET_SALES_RATE*SI.CALC_QUANTITY,0))/{ReportFilterHelper.FigureFilterValue(reportFilters.AmountFigureFilter)},2) TOTAL , 
                                   ROUND( SUM(nvl(SI.CALC_QUANTITY,0))/{ReportFilterHelper.FigureFilterValue(reportFilters.AmountFigureFilter)},2) QUANTITY 
                            FROM AREA_SETUP  IPC                           
                            INNER JOIN SA_SALES_INVOICE SI on SI.AREA_CODE = IPC.AREA_CODE            
                            INNER JOIN IP_ITEM_MASTER_SETUP IP on IP.ITEM_CODE=SI.ITEM_CODE                 
                            WHERE IPC.DELETED_FLAG = 'N' AND SI.DELETED_FLAG = 'N' 
                              AND IPC.COMPANY_CODE = SI.COMPANY_CODE
                              AND IP.COMPANY_CODE=SI.COMPANY_CODE
                            --  AND IM.COMPANY_CODE = IPC.COMPANY_CODE
                         --     AND IM.ITEM_CODE = SI.ITEM_CODE     
                              AND SI.company_code IN({companyCode})";

            //for customer Filter
            var customerFilter = string.Empty;
            if (reportFilters.CustomerFilter.Count() > 0)
            {
                customerFilter = @"select  DISTINCT(customer_code) from sa_customer_setup where (";
                //IF CUSTOMER_SKU_FLAG = G
                foreach (var item in reportFilters.CustomerFilter)
                {
                    customerFilter += "master_customer_code like  (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + companyCode + ")) OR ";
                }
                customerFilter = customerFilter.Substring(0, customerFilter.Length - 3);
                //IF CUSTOMER_SKU_FLAG = I                
                customerFilter += " or (customer_code in (" + string.Join(",", reportFilters.CustomerFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + companyCode + "))) ";


                Query += " and SI.customer_code IN(" + customerFilter + ")";
            }


            ////for Product Filter
            var productFilter = string.Empty;
            if (reportFilters.ProductFilter.Count() > 0)
            {
                productFilter = @"select  DISTINCT item_code from IP_ITEM_MASTER_SETUP where (";
                //IF PRODUCT_SKU_FLAG = G
                foreach (var item in reportFilters.ProductFilter)
                {
                    productFilter += "MASTER_ITEM_CODE like  (Select DISTINCT MASTER_ITEM_CODE || '%'  from IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE IN(" + companyCode + ")) OR ";
                }
                productFilter = productFilter.Substring(0, productFilter.Length - 3);
                //IF PRODUCT_SKU_FLAG = I                
                productFilter += " or (ITEM_CODE in (" + string.Join(",", reportFilters.ProductFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + companyCode + "))) ";


                Query += " and SI.ITEM_CODE IN(" + productFilter + ")";
            }


            //for category Filter
            if (!string.IsNullOrEmpty(categoryCode))
            {
                Query += " and (";
                foreach (var item in categoryCode.Split(','))
                {
                    Query += "IM.CATEGORY_CODE = '" + item + "' OR ";
                }
                Query = Query.Substring(0, Query.Length - 3) + ") ";
            }




            //FOR BRANCH FILTER
            if (reportFilters.BranchFilter.Count() > 0)
            {
                Query += " AND SI.BRANCH_CODE IN (" + string.Join(",", reportFilters.BranchFilter) + ")";

            }
            //FOR AREA FILTER
            if (reportFilters.AreaTypeFilter.Count() > 0)
            {
                Query += " AND SI.AREA_CODE IN (" + string.Join(",", reportFilters.AreaTypeFilter) + ")";

            }
            if (reportFilters.EmployeeFilter.Count > 0)
            {
                Query = Query + string.Format(@" AND SI.EMPLOYEE_CODE IN  ('{0}')", string.Join("','", reportFilters.EmployeeFilter).ToString());
            }
            if (reportFilters.AgentFilter.Count > 0)
            {
                Query = Query + string.Format(@" AND SI.AGENT_CODE IN  ('{0}')", string.Join("','", reportFilters.AgentFilter).ToString());
            }
            string locationFilter = string.Empty;
            if (reportFilters.LocationFilter.Count > 0)
            {

                var locations = reportFilters.LocationFilter;
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
                Query = Query + locationFilter;
            }


            Query += " GROUP BY IPC.AREA_CODE, IPC.AREA_EDESC";


            // Query = string.Format(Query, ReportFilterHelper.FigureFilterValue(reportFilters.AmountFigureFilter));

            var categorySales = _objectEntity.SqlQuery<ChartSalesModel>(Query);

            return categorySales.ToList();
        }

        public List<ChartSalesModel> GetNoOfbills(ReportFiltersModel reportFilters, User userInfo, string customerCode, string itemCode, string categoryCode, string companyCode, string branchCode, string partyTypeCode, string formCode)
        {
            companyCode = string.Empty;
            foreach (var company in reportFilters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }

            companyCode = companyCode == "" ? $@"'{userInfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);

            string Query = $@"    select BI.BRANCH_CODE CODE,count(MI.voucher_no) TOTAL,BI.BRANCH_EDESC DESCRIPTION
                               from master_transaction MI,FA_BRANCH_SETUP BI
                              where MI.BRANCH_CODE=BI.BRANCH_CODE AND MI.COMPANY_CODE=BI.COMPANY_CODE AND MI.COMPANY_CODE IN ({companyCode}) AND
                               MI.form_code  in
                                (select distinct form_code from form_detail_setup where  table_name='SA_SALES_INVOICE' and deleted_flag='N' and  COMPANY_CODE IN ({companyCode}) )
                               GROUP BY BI.BRANCH_CODE,BI.BRANCH_EDESC";


            // Query = string.Format(Query, ReportFilterHelper.FigureFilterValue(reportFilters.AmountFigureFilter));

            var categorySales = _objectEntity.SqlQuery<ChartSalesModel>(Query);

            return categorySales.ToList();
        }

        public IList<CustomerWisePriceListModel> GetCustomerWisePriceList(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Empty;
            foreach (var company in model.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }
            companyCode = companyCode == "" ? $@"'{userInfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);
            var Query = $@"SELECT IR.CS_CODE as Customer_Code, CS.CUSTOMER_EDESC AS CustomerName,IR.ITEM_CODE,
                               IMS.ITEM_EDESC,
                        Round((NVL(IR.STANDARD_RATE, 0)) / {ReportFilterHelper.FigureFilterValue(model.AmountFigureFilter)},{ReportFilterHelper.RoundUpFilterValue(model.AmountRoundUpFilter)}) Sales_Rate
                              -- IR.STANDARD_RATE as Sales_Rate
                          FROM IP_ITEM_RATE_SCHEDULE_SETUP IR, IP_ITEM_MASTER_SETUP IMS, SA_CUSTOMER_SETUP CS
                        WHERE     IR.ITEM_CODE = IMS.ITEM_CODE AND CS.CUSTOMER_CODE = IR.CS_CODE
                               AND EFFECTIVE_DATE = (SELECT MAX (EFFECTIVE_DATE)
                                                       FROM IP_ITEM_RATE_SCHEDULE_SETUP
                                                      WHERE ITEM_CODE = IMS.ITEM_CODE)
                               AND IR.COMPANY_CODE = IMS.COMPANY_CODE
                               AND IMS.DELETED_FLAG = 'N'
                               AND IMS.COMPANY_CODE IN({companyCode})
                    AND IR.EFFECTIVE_DATE>= TO_DATE('{model.FromDate}', 'YYYY-MON-DD')
                    AND IR.EFFECTIVE_DATE <= TO_DATE('{ model.ToDate}',' YYYY-MON-DD') AND IR.COMPANY_CODE IN ({companyCode}) ";

            //for customer Filter
            if (model.CustomerFilter.Count() > 0)
            {
                var customers = model.CustomerFilter;
                var customerConditionQuery = string.Empty;
                for (int i = 0; i < customers.Count; i++)
                {

                    if (i == 0)
                        customerConditionQuery += string.Format("MASTER_CUSTOMER_CODE like (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%' from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG = 'G')", customers[i], companyCode);
                    else
                    {
                        customerConditionQuery += string.Format(" OR  MASTER_CUSTOMER_CODE like (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG = 'G')", customers[i], companyCode);
                    }
                }

                Query = Query + string.Format(@" AND CS.CUSTOMER_CODE IN (SELECT DISTINCT(CUSTOMER_CODE) FROM SA_CUSTOMER_SETUP WHERE  {0} OR (CS.CUSTOMER_CODE IN ({1}) AND GROUP_SKU_FLAG='I')) ", customerConditionQuery, string.Join(",", customers));
                //Query += " and (";
                ////IF CUSTOMER_SKU_FLAG = G
                //foreach (var item in model.CustomerFilter)
                //{
                //    Query += "cs.master_customer_code like  (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND COMPANY_CODE IN(" + companyCode + ") ) OR ";
                //}
                ////IF CUSTOMER_SKU_FLAG = I                
                //Query += "(cs.CUSTOMER_CODE IN ('" + string.Join(",", model.CustomerFilter) + "') AND cs.GROUP_SKU_FLAG = 'I' AND cs.COMPANY_CODE IN(" + companyCode + ") )) ";

                //Query = Query.Substring(0, Query.Length - 1);
            }
            int min = 0, max = 0;
            ReportFilterHelper.RangeFilterValue(model.AmountRangeFilter, out min, out max);

            if (!(min == 0 && max == 0))
            {
                Query += string.Format(@" AND NVL(IR.STANDARD_RATE, 0) >= {0} AND NVL(IR.STANDARD_RATE,0) <= {1}", min, max);
            }

            if (model.BranchFilter.Count > 0)
            {
                Query += string.Format(@" AND CS.BRANCH_CODE IN ('{0}')", string.Join("','", model.BranchFilter).ToString());
            }
            Query = string.Format(Query, userInfo.company_code, userInfo.branch_code, model.FromDate, model.ToDate);
            var stockData = this._objectEntity.SqlQuery<CustomerWisePriceListModel>(Query).ToList();

            return stockData;
        }


        public IList<ProductWisePriceListModel> GetProductWisePriceList(ReportFiltersModel model, User userInfo)
        {
            //var companyCode = string.Join(",", model.CompanyFilter);
            //companyCode = companyCode == "" ? userInfo.company_code : companyCode;

            var companyCode = string.Empty;
            foreach (var company in model.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }

            companyCode = companyCode == "" ? $@"'{userInfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);
            //first check in IP_ITEM_RATE_SCHEDULE_SETUP  table
            var Query = @"SELECT IR.ITEM_CODE,IMS.ITEM_EDESC,MAX(IR.STANDARD_RATE) as Sales_Rate
                          FROM IP_ITEM_RATE_SCHEDULE_SETUP IR, IP_ITEM_MASTER_SETUP IMS
                        WHERE     IR.ITEM_CODE = IMS.ITEM_CODE
                               AND EFFECTIVE_DATE = (SELECT MAX (EFFECTIVE_DATE)
                                                       FROM IP_ITEM_RATE_SCHEDULE_SETUP
                                                      WHERE ITEM_CODE = IMS.ITEM_CODE )
                               AND IR.COMPANY_CODE = IMS.COMPANY_CODE
                               AND IMS.DELETED_FLAG = 'N'   
                               AND IMS.COMPANY_CODE IN({0}) 
                               ";

            //AND TRIM(IMS.MASTER_ITEM_CODE) LIKE: V_MIC || '%'      -- WAS COMMENTED IN QUERY
            //AND IMS.ITEM_CODE IN(SELECT ITEM_CODE FROM SA_SALES_INVOICE)


            ////for Product Filter
            if (model.ProductFilter.Count() > 0)
            {
                var products = model.ProductFilter;
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

                Query = Query + string.Format(@" AND IR.ITEM_CODE IN (SELECT DISTINCT(ITEM_CODE) FROM IP_ITEM_MASTER_SETUP WHERE {0} OR (ITEM_CODE IN ('{1}') AND GROUP_SKU_FLAG = 'I')) ", productConditionQuery, string.Join("','", products));
                //Query += " and (";
                ////IF PRODUCT_SKU_FLAG = G
                //foreach (var item in model.ProductFilter)
                //{
                //    Query += "IMS.MASTER_ITEM_CODE like  (Select DISTINCT(MASTER_ITEM_CODE) || '%'  from IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND COMPANY_CODE IN({0}) ) OR ";
                //}
                ////IF PRODUCT_SKU_FLAG = I                
                //Query += "(IMS.ITEM_CODE IN (" + string.Join(",", model.ProductFilter) + ") AND IMS.GROUP_SKU_FLAG = 'I')) ";

                //Query = Query.Substring(0, Query.Length - 1);
            }

            if (model.BranchFilter.Count > 0)
            {
                Query += string.Format(@" AND IMS.BRANCH_CODE IN ('{0}')", string.Join("','", model.BranchFilter).ToString());
            }

            Query += @" 
                GROUP BY IR.ITEM_CODE, IMS.ITEM_EDESC";
            Query = string.Format(Query, companyCode, userInfo.branch_code, model.FromDate, model.ToDate);

            var Data = this._objectEntity.SqlQuery<ProductWisePriceListModel>(Query).ToList();


            if (Data.Count() > 0)
            {
                return Data;
            }
            else
            {
                Query = @"SELECT  IR.ITEM_CODE,INITCAP(IMS.ITEM_EDESC) as ITEM_EDESC, IR.SALES_RATE FROM IP_ITEM_RATE_APPLICAT_SETUP  IR, IP_ITEM_MASTER_SETUP IMS
                                WHERE IR.ITEM_CODE = IMS.ITEM_CODE
                                AND APP_DATE = (SELECT MAX(APP_DATE) FROM  IP_ITEM_RATE_APPLICAT_SETUP WHERE ITEM_CODE = IMS.ITEM_CODE)
                                AND IR.COMPANY_CODE = IMS.COMPANY_CODE
                                AND IR.BRANCH_CODE = IMS.BRANCH_CODE
                                 AND IMS.COMPANY_CODE IN({0}) ";


                ////for Product Filter
                if (model.ProductFilter.Count() > 0)
                {

                    var products = model.ProductFilter;
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

                    Query = Query + string.Format(@" AND IR.ITEM_CODE IN (SELECT DISTINCT(ITEM_CODE) FROM IP_ITEM_MASTER_SETUP WHERE {0} OR (ITEM_CODE IN ('{1}') AND GROUP_SKU_FLAG = 'I')) ", productConditionQuery, string.Join("','", products));
                    //Query += " and (";
                    ////IF PRODUCT_SKU_FLAG = G
                    //foreach (var item in model.ProductFilter)
                    //{
                    //    Query += "IMS.MASTER_ITEM_CODE like  (Select DISTINCT(MASTER_ITEM_CODE) || '%'  from IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND COMPANY_CODE IN({0}) ) OR ";
                    //}
                    ////IF PRODUCT_SKU_FLAG = I                
                    //Query += "(IMS.ITEM_CODE IN (" + string.Join(",", model.ProductFilter) + ") AND IMS.GROUP_SKU_FLAG = 'I')) ";

                    //Query = Query.Substring(0, Query.Length - 1);
                }
                if (model.BranchFilter.Count > 0)
                {
                    Query += string.Format(@" AND IMS.BRANCH_CODE IN ('{0}')", string.Join("','", model.BranchFilter).ToString());
                }

                Query = string.Format(Query, companyCode, userInfo.branch_code, model.FromDate, model.ToDate);
                var newData = this._objectEntity.SqlQuery<ProductWisePriceListModel>(Query).ToList();

                if (newData.Count() > 0)
                    return newData;
                else
                    return Data;
            }

        }



        public IList<CustomerWiseProfileAnalysisModel> GetCustomerWiseProfitAnalysis(ReportFiltersModel model, User userInfo)
        {
            var companyCode = string.Empty;
            foreach (var company in model.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }

            companyCode = companyCode == "" ? $@"'{userInfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);
            var Query = @"SELECT CUSTOMER_CODE
	,CustomerName
	,ITEM_CODE
	,Unit
	,Quantity
	,Rate
	,SalesAmount
	,ITEM_EDESC
	,UnitCost
	,LANDED_COST
	,ROUND((NVL(RATE, 0) - NVL(UnitCost, 0)) * NVL(QUANTITY, 0), 2) GrossProfit
	,ROUND(((NVL(RATE, 0) - NVL(UnitCost, 0)) / NVL(UnitCost, 1)) * 100, 2)GrossPercent
FROM (SELECT a.CUSTOMER_CODE, c.CUSTOMER_EDESC as CustomerName, a.ITEM_CODE, a.MU_CODE as Unit,A.COMPANY_CODE
			,A.BRANCH_CODE
                                 ,SUM(NVL(a.QUANTITY, 0)) as Quantity  ,                                               
                                 Round(SUM(NVL(FN_CONVERT_CURRENCY(NVL(a.TOTAL_PRICE, 0) * NVL(a.EXCHANGE_RATE, 1), 'NRS', a.SALES_DATE), 0)) / DECODE(SUM(NVL(a.QUANTITY, 1)), 0, 1, SUM(NVL(a.QUANTITY, 1)))/ {4},{5}) as Rate ,                                                 
                                 Round(SUM(NVL(FN_CONVERT_CURRENCY(NVL(a.TOTAL_PRICE, 0) * NVL(a.EXCHANGE_RATE, 1), 'NRS', a.SALES_DATE), 0)) / {4},{5}) SalesAmount ,
                                 b.ITEM_EDESC,
                                 FN_UNIT_COST(A.ITEM_CODE, NULL, A.COMPANY_CODE, A.BRANCH_CODE, TO_DATE('{2}', 'YYYY-MM-DD'), TO_DATE('{3}',' YYYY-MM-DD')) UnitCost,
                                 Round(SUM(NVL(a.QUANTITY, 0) * NVL(d.LANDED_COST, 0)) /{4},{5}) LANDED_COST   ,       
                                 Round((SUM(NVL(FN_CONVERT_CURRENCY(NVL(a.TOTAL_PRICE, 0) * NVL(a.EXCHANGE_RATE, 1), 'NRS', a.SALES_DATE), 0)) - SUM(NVL(a.QUANTITY, 0) * NVL(d.LANDED_COST, 0)))/{4},{5}) GrossProfit,
                                 CASE (NVL(d.LANDED_COST, 0)) WHEN  0 THEN 0
                                     ELSE 
                                        ROUND(((SUM(NVL(FN_CONVERT_CURRENCY(NVL(a.TOTAL_PRICE, 0) * NVL(a.EXCHANGE_RATE, 1), 'NRS', a.SALES_DATE), 0)) - SUM(NVL(a.QUANTITY, 0) * NVL(d.LANDED_COST, 0))) * 100) / SUM(NVL(a.QUANTITY, 0) * NVL(d.LANDED_COST, 1)), {5})
                                END GrossPercent
                         FROM SA_SALES_INVOICE a, IP_ITEM_MASTER_SETUP b, SA_CUSTOMER_SETUP c, MP_ITEM_STD_RATE d
                         WHERE  a.ITEM_CODE = b.ITEM_CODE AND a.CUSTOMER_CODE = c.CUSTOMER_CODE AND a.COMPANY_CODE = b.COMPANY_CODE
                                AND a.COMPANY_CODE = c.COMPANY_CODE AND a.ITEM_CODE = d.ITEM_CODE(+) AND a.COMPANY_CODE = d.COMPANY_CODE(+)
                                AND a.DELETED_FLAG = 'N'
                                AND a.COMPANY_CODE IN({0})
                                --AND a.BRANCH_CODE = '{1}'                          
                                AND a.SALES_DATE >= TO_DATE('{2}', 'YYYY-MM-DD')
                                AND a.SALES_DATE <= TO_DATE('{3}',' YYYY-MM-DD')";

            //for customer Filter
            if (model.CustomerFilter.Count() > 0)
            {
                Query += " and (";
                foreach (var item in model.CustomerFilter)
                {
                    Query += "c.master_customer_code like  (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '" + item + "' AND COMPANY_CODE IN(" + companyCode + ") ) OR ";
                }
                Query = Query.Substring(0, Query.Length - 3) + ") ";
            }

            //for item Filter
            if (model.ProductFilter.Count() > 0)
            {
                Query += " and (";
                foreach (var item in model.ProductFilter)
                {
                    Query += "b.MASTER_ITEM_CODE LIKE (Select DISTINCT DISTINCT(MASTER_ITEM_CODE) || '%'  from IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '" + item + "' AND COMPANY_CODE IN(" + companyCode + ") ) OR ";
                }
                Query = Query.Substring(0, Query.Length - 3) + ") ";
            }


            //for category Filter
            if (model.CategoryFilter.Count() > 0)
            {
                Query += " and (";
                foreach (var item in model.CategoryFilter)
                {
                    Query += "b.CATEGORY_CODE = '" + item + "' OR ";
                }
                Query = Query.Substring(0, Query.Length - 3) + ") ";
            }

            if (model.BranchFilter.Count > 0)
            {
                Query += string.Format(@" AND a.BRANCH_CODE IN ('{0}')", string.Join("','", model.BranchFilter).ToString());
            }

            Query += @"
                            GROUP BY a.ITEM_CODE, a.MU_CODE, b.ITEM_EDESC,A.COMPANY_CODE
			,A.BRANCH_CODE, c.CUSTOMER_EDESC, a.CUSTOMER_CODE,d.LANDED_COST
                            ORDER BY c.CUSTOMER_EDESC, b.ITEM_EDESC)";

            Query = string.Format(Query, companyCode, userInfo.branch_code, model.FromDate, model.ToDate, ReportFilterHelper.FigureFilterValue(model.AmountFigureFilter), ReportFilterHelper.RoundUpFilterValue(model.AmountRoundUpFilter));

            var Data = this._objectEntity.SqlQuery<CustomerWiseProfileAnalysisModel>(Query).ToList();
            return Data;

        }

        public List<DynamicMenu> GetDynamicMenu(int userId, int level, string modular_code)
        {
            var dynamicMenu = new List<DynamicMenu>();
            var query = $@"select menu_no,FULL_PATH,VIRTUAL_PATH,MENU_EDESC,GROUP_SKU_FLAG,ICON_PATH,MODULE_CODE
from WEB_MENU_MANAGEMENT where menu_no in (SELECT mc.MENU_NO from WEB_MENU_CONTROL mc 
INNER JOIN WEB_MENU_MANAGEMENT mm on mm.MENU_NO = mc.MENU_NO and mm.group_sku_flag='G'
 WHERE mc.USER_NO ='{userId}' and mm.module_code='{modular_code}' ) and module_code='{modular_code}' order by ORDERBY asc";
            //  string query = "SELECT mc.MENU_NO, mm.VIRTUAL_PATH, mm.MENU_EDESC,mm.GROUP_SKU_FLAG,  mm.ICON_PATH,mm.MODULE_CODE from WEB_MENU_CONTROL mc INNER JOIN WEB_MENU_MANAGEMENT mm on mm.MENU_NO = mc.MENU_NO WHERE mc.USER_NO = " + userId + " and mm.module_code='" + modular_code + "' order by mm.ORDERBY asc";
            try
            {
                dynamicMenu = _objectEntity.SqlQuery<DynamicMenu>(query).ToList();
            }
            catch (Exception e)
            {
                throw e;
            }
            return dynamicMenu;
        }

        public List<DynamicMenu> GetChlidMenu(string menuNo, int userid, string module_code)
        {
            var dynamicMenu = new List<DynamicMenu>();
            var query = $@"SELECT mc.MENU_NO,mm.FULL_PATH, mm.VIRTUAL_PATH, mm.MENU_EDESC,mm.GROUP_SKU_FLAG, mm.DASHBOARD_FLAG,
 mm.ICON_PATH,mm.MODULE_CODE,mm.MODULE_ABBR,mm.COLOR,mm.DESCRIPTION from WEB_MENU_CONTROL mc 
INNER JOIN WEB_MENU_MANAGEMENT mm on mm.MENU_NO = mc.MENU_NO
 WHERE mc.USER_NO ='{userid}' and mm.module_code='{module_code}' and PRE_MENU_NO='{menuNo}' ORDER BY mm.ORDERBY ASC";
            //  string query = "SELECT MENU_NO, VIRTUAL_PATH, MENU_EDESC, GROUP_SKU_FLAG, ICON_PATH,MODULE_CODE,MODULE_ABBR,COLOR,DESCRIPTION  FROM WEB_MENU_MANAGEMENT WHERE PRE_MENU_NO=" + menuNo + " ORDER BY ORDERBY ASC";
            try
            {
                dynamicMenu = _objectEntity.SqlQuery<DynamicMenu>(query).ToList();
            }
            catch (Exception e)
            {
                throw e;
            }
            return dynamicMenu;
        }
        public IList<SalesRegisterDetailModel> GetSalesRegisterModelPrivot(ReportFiltersModel model, User userInfo)
        {

            var companyCode = string.Empty;
            foreach (var company in model.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }
            companyCode = companyCode == "" ? $@"'{userInfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);

            var Query = string.Format(@"SELECT F.* ,ROUND(TO_NUMBER(F.DISCOUNTED_AMOUNT),2)-F.SPECIAL_DISCOUNT_SCHEME  +  F.EXCISE_DUTY+ F.VAT_AMOUNT TOTAL_BILL_VALUE  FROM (
                                SELECT A.* ,((round(TO_NUMBER(DISCOUNTED_AMOUNT),2)-SPECIAL_DISCOUNT_SCHEME  + EXCISE_DUTY )* NVL((SELECT DISTINCT DECODE(CHARGE_AMOUNT,0,0,13) FROM CHARGE_TRANSACTION X
                                WHERE X.REFERENCE_NO = A.SALES_NO
                                AND X.FORM_CODE = A.FORM_CODE
                                AND X.BRANCH_CODE = A.BRANCH_CODE
                                AND X.CHARGE_CODE='VT'),0)/100 ) VAT_AMOUNT FROM(
                                SELECT F.BRANCH_EDESC, E.DIVISION_EDESC DIVISION_NAME,
                                FN_CHARTAD_MONTH(SUBSTR(A.SALES_DATE,4,3)) EMONTH ,      
                                FN_CHARTBS_MONTH(SUBSTR(BS_DATE(A.SALES_DATE),6,2)) BSMONTH,  BS_DATE(A.SALES_DATE) MITI,    
                                A.SALES_DATE INV_DATE, A.SALES_NO INVOICE,  A.MANUAL_NO,ASM.AREA_EDESC, C.CUSTOMER_EDESC,  
                                B.ITEM_EDESC MODULE, A.QUANTITY,   A.UNIT_PRICE, A.TOTAL_PRICE,
                                (NVL((SELECT DISTINCT NVL(SUM(CHARGE_AMOUNT),0) FROM CHARGE_TRANSACTION X
                                WHERE X.REFERENCE_NO = A.SALES_NO
                                AND X.FORM_CODE = A.FORM_CODE
                                AND X.BRANCH_CODE = A.BRANCH_CODE
                                AND X.CHARGE_CODE='DC'),0)/(SELECT NVL(SUM(CALC_TOTAL_PRICE),0) 
                                FROM SA_SALES_INVOICE WHERE COMPANY_CODE = A.COMPANY_CODE 
                                AND SALES_NO = A.SALES_NO) * NVL(A.TOTAL_PRICE,1)) DISCOUNT_AMT,
                                to_char(NVL(A.TOTAL_PRICE,0) - (NVL((SELECT DISTINCT NVL(SUM(CHARGE_AMOUNT),0) FROM CHARGE_TRANSACTION X
                                WHERE X.REFERENCE_NO = A.SALES_NO
                                AND X.FORM_CODE = A.FORM_CODE
                                AND X.BRANCH_CODE = A.BRANCH_CODE
                                AND X.CHARGE_CODE='DC'),0)/(SELECT NVL(SUM(CALC_TOTAL_PRICE),0) 
                                FROM SA_SALES_INVOICE WHERE COMPANY_CODE = A.COMPANY_CODE 
                                AND SALES_NO = A.SALES_NO) * NVL(A.TOTAL_PRICE,1))) DISCOUNTED_AMOUNT,
                                (NVL((SELECT DISTINCT NVL(SUM(CHARGE_AMOUNT),0) FROM CHARGE_TRANSACTION X
                                WHERE X.REFERENCE_NO = A.SALES_NO
                                AND X.FORM_CODE = A.FORM_CODE
                                AND X.BRANCH_CODE = A.BRANCH_CODE
                                AND X.CHARGE_CODE='SDD'),0)/(SELECT NVL(SUM(CALC_TOTAL_PRICE),0) 
                                FROM SA_SALES_INVOICE WHERE COMPANY_CODE = A.COMPANY_CODE 
                                AND SALES_NO = A.SALES_NO) * NVL(A.TOTAL_PRICE,1)) SPECIAL_DISCOUNT_SCHEME,
                                (SELECT CASE WHEN VALUE_PERCENT_FLAG  = 'Q'
                                    THEN (NVL((SELECT DISTINCT MANUAL_CALC_VALUE FROM CHARGE_TRANSACTION X
                                WHERE X.REFERENCE_NO = A.SALES_NO
                                AND X.FORM_CODE = A.FORM_CODE
                                AND X.BRANCH_CODE = A.BRANCH_CODE
                                AND X.CHARGE_CODE='ED'),0)*(SELECT NVL(QUANTITY,0) 
                                FROM SA_SALES_INVOICE WHERE COMPANY_CODE = A.COMPANY_CODE 
                                AND SALES_NO = A.SALES_NO
                                AND SERIAL_NO = A.SERIAL_NO
                                AND FORM_CODE = A.FORM_CODE))
                                    WHEN VALUE_PERCENT_FLAG = 'V'
                                    THEN (NVL((SELECT DISTINCT NVL(SUM(CHARGE_AMOUNT),0) FROM CHARGE_TRANSACTION X
                                WHERE X.REFERENCE_NO = A.SALES_NO
                                AND X.FORM_CODE = A.FORM_CODE
                                AND X.BRANCH_CODE = A.BRANCH_CODE
                                AND X.CHARGE_CODE='ED'),0)/(SELECT NVL(SUM(CALC_TOTAL_PRICE),0) 
                                FROM SA_SALES_INVOICE WHERE COMPANY_CODE = A.COMPANY_CODE 
                                AND SALES_NO = A.SALES_NO) * NVL(A.TOTAL_PRICE,1)
                                )    WHEN VALUE_PERCENT_FLAG = 'P'
                                    THEN (NVL((SELECT DISTINCT NVL(SUM(CHARGE_AMOUNT),0) FROM CHARGE_TRANSACTION X
                                WHERE X.REFERENCE_NO = A.SALES_NO
                                AND X.FORM_CODE = A.FORM_CODE
                                AND X.BRANCH_CODE = A.BRANCH_CODE
                                AND X.CHARGE_CODE='ED'),0)/(SELECT NVL(SUM(CALC_TOTAL_PRICE),0) 
                                FROM SA_SALES_INVOICE WHERE COMPANY_CODE = A.COMPANY_CODE 
                                AND SALES_NO = A.SALES_NO) * NVL(A.TOTAL_PRICE,1)
                                )
                                  END AS STATUSTEXT FROM CHARGE_TRANSACTION X, CHARGE_SETUP CS
                                  WHERE X.CHARGE_CODE='ED'
                                  AND X.CHARGE_CODE = CS.CHARGE_CODE
                                  AND X.FORM_CODE = CS.FORM_CODE
                                  AND X.COMPANY_CODE = CS.COMPANY_CODE
                                  AND X.FORM_CODE =A.FORM_CODE
                                  AND X.REFERENCE_NO = A.SALES_NO) EXCISE_DUTY,
                                SUBSTR(FN_FETCH_GROUP_DESC(A.COMPANY_CODE,'IP_ITEM_MASTER_SETUP', B.PRE_ITEM_CODE),1,30) ITEM_GROUP_EDESC,
                                SUBSTR(FN_FETCH_PRE_DESC(A.COMPANY_CODE,'IP_ITEM_MASTER_SETUP', B.PRE_ITEM_CODE),1,30) ITEM_SUBGROUP_EDESC,
                                FN_FETCH_DESC(A.COMPANY_CODE,'IP_CATEGORY_CODE',B.CATEGORY_CODE) CATEGORY_EDESC,
                                G.EMPLOYEE_EDESC MR_NAME, A.BRANCH_CODE, A.FORM_CODE, A.COMPANY_CODE, A.SALES_NO,
                                SUBSTR(FN_FETCH_GROUP_DESC(A.COMPANY_CODE,'SA_CUSTOMER_SETUP', C.PRE_CUSTOMER_CODE),1,30) CUSTOMER_GROUP_EDESC,
                                SUBSTR(FN_FETCH_PRE_DESC(A.COMPANY_CODE,'SA_CUSTOMER_SETUP', C.PRE_CUSTOMER_CODE),1,30) CUSTOMER_SUBGROUP_EDESC, 
                                FN_FETCH_DESC(A.COMPANY_CODE,'IP_PARTY_TYPE_CODE',A.PARTY_TYPE_CODE) DEALER_NAME,
                                S.DESTINATION, FN_FETCH_DESC(S.COMPANY_CODE,'IP_VEHICLE_CODE',S.VEHICLE_CODE) VEHICLE_NAME, S.VEHICLE_OWNER_NAME, S.VEHICLE_OWNER_NO,S.DRIVER_NAME, S.DRIVER_LICENSE_NO, 
                                S.DRIVER_MOBILE_NO, FN_FETCH_DESC(S.COMPANY_CODE,'TRANSPORTER_SETUP',S.TRANSPORTER_CODE) TRANSPORTER_NAME, nvl(S.FREGHT_AMOUNT,0) FREGHT_AMOUNT, nvl(S.WB_WEIGHT,0) WB_WEIGHT, S.WB_NO, S.WB_DATE
                                FROM SA_SALES_INVOICE A, IP_ITEM_MASTER_SETUP B, SA_CUSTOMER_SETUP C, 
                                FA_DIVISION_SETUP E, FA_BRANCH_SETUP F, HR_EMPLOYEE_SETUP G , SHIPPING_TRANSACTION S, AREA_SETUP ASM
                                WHERE A.ITEM_CODE = B.ITEM_CODE(+) AND A.CUSTOMER_CODE = C.CUSTOMER_CODE(+) 
                                AND A.COMPANY_CODE = B.COMPANY_CODE(+) 
                                AND A.DELETED_FLAG='N'
                                AND A.COMPANY_CODE = C.COMPANY_CODE(+)
                                AND A.DIVISION_CODE = E.DIVISION_CODE(+)
                                AND A.BRANCH_CODE = F.BRANCH_CODE(+)
                                AND A.EMPLOYEE_CODE = G.EMPLOYEE_CODE(+)
                                AND A.COMPANY_CODE = G.COMPANY_CODE(+)
                                AND A.COMPANY_CODE = S.COMPANY_CODE(+)
                                AND A.FORM_CODE = S.FORM_CODE (+)
                                AND A.SALES_NO = S.VOUCHER_NO (+)
                                AND A.AREA_CODE = ASM.AREA_CODE (+)
                                AND a.SALES_DATE >= TO_DATE('{0}', 'YYYY-MON-DD')
                                AND a.SALES_DATE <= TO_DATE('{1}',' YYYY-MON-DD')", model.FromDate, model.ToDate, companyCode);

            if (model.CustomerFilter.Count > 0)
            {

                var customers = model.CustomerFilter;
                var customerConditionQuery = string.Empty;
                for (int i = 0; i < customers.Count; i++)
                {

                    if (i == 0)
                        customerConditionQuery += string.Format("MASTER_CUSTOMER_CODE like (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%' from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G')", customers[i], companyCode);
                    else
                    {
                        customerConditionQuery += string.Format(" OR  MASTER_CUSTOMER_CODE like (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G')", customers[i], companyCode);
                    }
                }

                Query = Query + string.Format(@" AND C.CUSTOMER_CODE IN (SELECT DISTINCT(CUSTOMER_CODE) FROM SA_CUSTOMER_SETUP WHERE  {0} OR (CUSTOMER_CODE IN ('{1}') AND GROUP_SKU_FLAG='I')) ", customerConditionQuery, string.Join("','", customers));
            }

            if (model.DocumentFilter.Count > 0)
            {
                Query = Query + string.Format(@" AND  A.FORM_CODE  IN  ('{0}')", string.Join("','", model.DocumentFilter).ToString());
            }
            if (model.AreaTypeFilter.Count > 0)
            {
                Query = Query + string.Format(@" AND  A.AREA_CODE  IN  ({0})", string.Join(",", model.AreaTypeFilter).ToString());
            }
            if (model.PartyTypeFilter.Count > 0)
            {
                Query = Query + string.Format(@" AND A.PARTY_TYPE_CODE IN ('{0}') ", string.Join("','", model.PartyTypeFilter).ToString());
            }
            if (model.CategoryFilter.Count > 0)
            {
                Query = Query + string.Format(@" AND B.CATEGORY_CODE IN ('{0}') ", string.Join("','", model.CategoryFilter).ToString());
            }
            if (model.EmployeeFilter.Count > 0)
            {
                Query = Query + string.Format(@" AND A.EMPLOYEE_CODE IN  ('{0}')", string.Join("','", model.EmployeeFilter).ToString());
            }
            if (model.AgentFilter.Count > 0)
            {
                Query = Query + string.Format(@" AND A.AGENT_CODE IN  ('{0}')", string.Join("','", model.AgentFilter).ToString());
            }
            if (model.DivisionFilter.Count > 0)
            {
                Query = Query + string.Format(@" AND A.DIVISION_CODE IN ('{0}')", string.Join("','", model.DivisionFilter).ToString());
            }
            if (model.LocationFilter.Count > 0)
            {
                var locations = model.LocationFilter;
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
                Query = Query + string.Format(@" AND A.FROM_LOCATION_CODE IN ({0} OR LOCATION_CODE IN ('{1}'))", locationConditionQuery, string.Join("','", locations));
                //query = query + string.Format(@" AND (SI.FROM_LOCATION_CODE = LS.LOCATION_CODE OR SI.FROM_LOCATION_CODE IN ('{0}'))", string.Join("','", filters.LocationFilter).ToString());
            }

            if (model.ProductFilter.Count() > 0)
            {
                var products = model.ProductFilter;
                var productConditionQuery = string.Empty;
                for (int i = 0; i < products.Count; i++)
                {

                    if (i == 0)
                        productConditionQuery += string.Format("MASTER_ITEM_CODE like (Select DISTINCT(MASTER_ITEM_CODE) || '%' from IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G' )", products[i], companyCode);
                    else
                    {
                        productConditionQuery += string.Format(" OR  MASTER_ITEM_CODE like (Select DISTINCT(MASTER_ITEM_CODE) || '%'  from IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G')", products[i], companyCode);
                    }
                }

                Query = Query + string.Format(@" AND A.ITEM_CODE IN (SELECT DISTINCT(ITEM_CODE) FROM IP_ITEM_MASTER_SETUP WHERE {0} OR (ITEM_CODE IN ('{1}') AND GROUP_SKU_FLAG='I'))", productConditionQuery, string.Join("','", products));
                //var productFilter = @"select  DISTINCT TRIM(item_code) from IP_ITEM_MASTER_SETUP where (";
                ////IF PRODUCT_SKU_FLAG = G
                //foreach (var company in model.CompanyFilter)
                //{
                //    foreach (var item in model.ProductFilter)
                //    {
                //        productFilter += "MASTER_ITEM_CODE like  (Select DISTINCT MASTER_ITEM_CODE || '%'  from IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '" + item + "' AND GROUP_SKU_FLAG = 'G' AND DELETED_FLAG= 'N' AND COMPANY_CODE ='" + company + "') OR ";
                //    }
                //}
                //productFilter = productFilter.Substring(0, productFilter.Length - 3);
                ////IF PRODUCT_SKU_FLAG = I                
                //productFilter += " OR (ITEM_CODE in (" + string.Join(",", model.ProductFilter) + ") and group_sku_flag = 'I' AND DELETED_FLAG = 'N' AND COMPANY_CODE IN(" + string.Join(",", model.CompanyFilter) + "))) ";
                //productFilter = " AND A.ITEM_CODE IN(" + productFilter + ")";

                //Query += productFilter;
            }
            if (model.BranchFilter.Count > 0)
            {
                Query += string.Format(@" AND a.BRANCH_CODE IN ('{0}')", string.Join("','", model.BranchFilter).ToString());
            }
            Query += $@" AND A.COMPANY_CODE IN({companyCode})) A) F";
            var Data = new List<SalesRegisterDetailModel>();
            try
            {
                 Data = this._objectEntity.SqlQuery<SalesRegisterDetailModel>(Query).ToList();
            }
            catch(Exception ex)
            {
                return Data;
            }
            return Data;
        }

        public IList<DailySalesTreeList> GetSalesRegisterDailyReport(ReportFiltersModel model, User userInfo)
        {
            var cachedata = new List<DailySalesTreeList>();
            var cachekey = $"NCR-report-{userInfo.company_code}{model.FromDate}-{model.ToDate}";
            if (model.CustomerFilter.Count > 0)
            {
                this._cacheManager.Remove(cachekey);
            }
            if (this._cacheManager.IsSet(cachekey))
            {
                
                cachedata = this._cacheManager.Get<List<DailySalesTreeList>>(cachekey);
                return cachedata;
            }
            else
            {
              
                
                var figureAmountFilter = ReportFilterHelper.FigureFilterValue(model.AmountFigureFilter);
                var roundUpAmountFilter = ReportFilterHelper.RoundUpFilterValue(model.AmountRoundUpFilter);
                var companyCode = string.Empty;
                foreach (var company in model.CompanyFilter)
                {
                    companyCode += $@"'{company}',";
                }
                companyCode = companyCode == "" ? $@"'{userInfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);
                var Query = string.Format(@"SELECT F.* ,F.DISCOUNTED_AMOUNT - F.SPECIAL_DISCOUNT_SCHEME  +  F.EXCISE_DUTY+ F.VAT_AMOUNT TOTAL_BILL_VALUE  FROM (
                                SELECT A.* ,((DISCOUNTED_AMOUNT - SPECIAL_DISCOUNT_SCHEME + EXCISE_DUTY )* NVL((SELECT DISTINCT DECODE(CHARGE_AMOUNT,0,0,13) FROM CHARGE_TRANSACTION X
                                WHERE X.REFERENCE_NO = A.SALES_NO
                                AND X.FORM_CODE = A.FORM_CODE
                                AND X.BRANCH_CODE = A.BRANCH_CODE
                                AND X.CHARGE_CODE='VT'),0)/100 ) VAT_AMOUNT FROM(
                                SELECT F.BRANCH_EDESC,
                                FN_CHARTAD_MONTH(SUBSTR(A.SALES_DATE,4,3)) EMONTH ,      
                                FN_CHARTBS_MONTH(SUBSTR(BS_DATE(A.SALES_DATE),6,2)) BSMONTH,  BS_DATE(A.SALES_DATE) MITI,    
                                A.SALES_DATE INV_DATE, A.SALES_NO INVOICE,  A.MANUAL_NO,A.PARTY_TYPE_CODE,C.CUSTOMER_CODE, C.CUSTOMER_EDESC,  
                                B.ITEM_EDESC MODULE,A.QUANTITY,A.UNIT_PRICE,Round(NVL(FN_CONVERT_CURRENCY(NVL(A.TOTAL_PRICE,0),'NRS',A.SALES_DATE),0)/{3},{4}) AS TOTAL_PRICE,
                                (NVL((SELECT DISTINCT NVL(SUM(CHARGE_AMOUNT),0) FROM CHARGE_TRANSACTION X
                                WHERE X.REFERENCE_NO = A.SALES_NO
                                AND X.FORM_CODE = A.FORM_CODE
                                AND X.BRANCH_CODE = A.BRANCH_CODE
                                AND X.CHARGE_CODE='DC'),0)/(SELECT NVL(SUM(CALC_TOTAL_PRICE),0) 
                                FROM SA_SALES_INVOICE WHERE COMPANY_CODE = A.COMPANY_CODE 
                                AND SALES_NO = A.SALES_NO) * NVL(A.TOTAL_PRICE,1)) DISCOUNT_AMT,
                                NVL(A.TOTAL_PRICE,0) - (NVL((SELECT DISTINCT NVL(SUM(CHARGE_AMOUNT),0) FROM CHARGE_TRANSACTION X
                                WHERE X.REFERENCE_NO = A.SALES_NO
                                AND X.FORM_CODE = A.FORM_CODE
                                AND X.BRANCH_CODE = A.BRANCH_CODE
                                AND X.CHARGE_CODE='DC'),0)/(SELECT NVL(SUM(CALC_TOTAL_PRICE),0) 
                                FROM SA_SALES_INVOICE WHERE COMPANY_CODE = A.COMPANY_CODE 
                                AND SALES_NO = A.SALES_NO) * NVL(A.TOTAL_PRICE,1)) DISCOUNTED_AMOUNT,
                                (NVL((SELECT DISTINCT NVL(SUM(CHARGE_AMOUNT),0) FROM CHARGE_TRANSACTION X
                                WHERE X.REFERENCE_NO = A.SALES_NO
                                AND X.FORM_CODE = A.FORM_CODE
                                AND X.BRANCH_CODE = A.BRANCH_CODE
                                AND X.CHARGE_CODE='SDD'),0)/(SELECT NVL(SUM(CALC_TOTAL_PRICE),0) 
                                FROM SA_SALES_INVOICE WHERE COMPANY_CODE = A.COMPANY_CODE 
                                AND SALES_NO = A.SALES_NO) * NVL(A.TOTAL_PRICE,1)) SPECIAL_DISCOUNT_SCHEME,
                                (SELECT CASE WHEN VALUE_PERCENT_FLAG  = 'Q'
                                    THEN (NVL((SELECT DISTINCT MANUAL_CALC_VALUE FROM CHARGE_TRANSACTION X
                                WHERE X.REFERENCE_NO = A.SALES_NO
                                AND X.FORM_CODE = A.FORM_CODE
                                AND X.BRANCH_CODE = A.BRANCH_CODE
                                AND X.CHARGE_CODE='ED'),0)*(SELECT NVL(QUANTITY,0) 
                                FROM SA_SALES_INVOICE WHERE COMPANY_CODE = A.COMPANY_CODE 
                                AND SALES_NO = A.SALES_NO
                                AND SERIAL_NO = A.SERIAL_NO
                                AND FORM_CODE = A.FORM_CODE))
                                    WHEN VALUE_PERCENT_FLAG = 'V'
                                    THEN (NVL((SELECT DISTINCT NVL(SUM(CHARGE_AMOUNT),0) FROM CHARGE_TRANSACTION X
                                WHERE X.REFERENCE_NO = A.SALES_NO
                                AND X.FORM_CODE = A.FORM_CODE
                                AND X.BRANCH_CODE = A.BRANCH_CODE
                                AND X.CHARGE_CODE='ED'),0)/(SELECT NVL(SUM(CALC_TOTAL_PRICE),0) 
                                FROM SA_SALES_INVOICE WHERE COMPANY_CODE = A.COMPANY_CODE 
                                AND SALES_NO = A.SALES_NO) * NVL(A.TOTAL_PRICE,1)
                                )    WHEN VALUE_PERCENT_FLAG = 'P'
                                    THEN (NVL((SELECT DISTINCT NVL(SUM(CHARGE_AMOUNT),0) FROM CHARGE_TRANSACTION X
                                WHERE X.REFERENCE_NO = A.SALES_NO
                                AND X.FORM_CODE = A.FORM_CODE
                                AND X.BRANCH_CODE = A.BRANCH_CODE
                                AND X.CHARGE_CODE='ED'),0)/(SELECT NVL(SUM(CALC_TOTAL_PRICE),0) 
                                FROM SA_SALES_INVOICE WHERE COMPANY_CODE = A.COMPANY_CODE 
                                AND SALES_NO = A.SALES_NO) * NVL(A.TOTAL_PRICE,1)
                                )
                                  END AS STATUSTEXT FROM CHARGE_TRANSACTION X, CHARGE_SETUP CS
                                  WHERE X.CHARGE_CODE='ED'
                                  AND X.CHARGE_CODE = CS.CHARGE_CODE
                                  AND X.FORM_CODE = CS.FORM_CODE
                                  AND X.COMPANY_CODE = CS.COMPANY_CODE
                                  AND X.FORM_CODE =A.FORM_CODE
                                  AND X.REFERENCE_NO = A.SALES_NO) EXCISE_DUTY,
                              A.BRANCH_CODE, A.FORM_CODE, A.COMPANY_CODE, A.SALES_NO,
                                C.MASTER_CUSTOMER_CODE as MasterCode,C.PRE_CUSTOMER_CODE as PreCode
                                FROM SA_SALES_INVOICE A, IP_ITEM_MASTER_SETUP B, SA_CUSTOMER_SETUP C,  FA_BRANCH_SETUP F
                                WHERE A.ITEM_CODE = B.ITEM_CODE(+) AND A.CUSTOMER_CODE = C.CUSTOMER_CODE(+) 
                                AND A.COMPANY_CODE = B.COMPANY_CODE(+) 
                                AND A.DELETED_FLAG='N'
                                AND A.COMPANY_CODE = C.COMPANY_CODE(+)
                                AND A.BRANCH_CODE = F.BRANCH_CODE(+)
                                AND a.SALES_DATE >= TO_DATE('{0}', 'YYYY-MON-DD')
                                AND a.SALES_DATE <= TO_DATE('{1}',' YYYY-MON-DD')", model.FromDate, model.ToDate, companyCode, figureAmountFilter, roundUpAmountFilter);

                if (model.CustomerFilter.Count > 0)
                {

                    var customers = model.CustomerFilter;
                    var customerConditionQuery = string.Empty;
                    for (int i = 0; i < customers.Count; i++)
                    {

                        if (i == 0)
                            customerConditionQuery += string.Format("MASTER_CUSTOMER_CODE like (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%' from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G')", customers[i], companyCode);
                        else
                        {
                            customerConditionQuery += string.Format(" OR  MASTER_CUSTOMER_CODE like (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G')", customers[i], companyCode);
                        }
                    }

                    Query = Query + string.Format(@" AND C.CUSTOMER_CODE IN (SELECT DISTINCT(CUSTOMER_CODE) FROM SA_CUSTOMER_SETUP WHERE  {0} OR (CUSTOMER_CODE IN ('{1}') AND GROUP_SKU_FLAG='I')) ", customerConditionQuery, string.Join("','", customers));
                }

                if (model.DocumentFilter.Count > 0)
                {
                    Query = Query + string.Format(@" AND  A.FORM_CODE  IN  ('{0}')", string.Join("','", model.DocumentFilter).ToString());
                }
                if (model.AreaTypeFilter.Count > 0)
                {
                    Query = Query + string.Format(@" AND  A.AREA_CODE  IN  ({0})", string.Join(",", model.AreaTypeFilter).ToString());
                }
                if (model.PartyTypeFilter.Count > 0)
                {
                    Query = Query + string.Format(@" AND A.PARTY_TYPE_CODE IN ('{0}') ", string.Join("','", model.PartyTypeFilter).ToString());
                }
                if (model.CategoryFilter.Count > 0)
                {
                    Query = Query + string.Format(@" AND B.CATEGORY_CODE IN ('{0}') ", string.Join("','", model.CategoryFilter).ToString());
                }
                if (model.EmployeeFilter.Count > 0)
                {
                    Query = Query + string.Format(@" AND A.EMPLOYEE_CODE IN  ('{0}')", string.Join("','", model.EmployeeFilter).ToString());
                }
                if (model.AgentFilter.Count > 0)
                {
                    Query = Query + string.Format(@" AND A.AGENT_CODE IN  ('{0}')", string.Join("','", model.AgentFilter).ToString());
                }
                if (model.DivisionFilter.Count > 0)
                {
                    Query = Query + string.Format(@" AND A.DIVISION_CODE IN ('{0}')", string.Join("','", model.DivisionFilter).ToString());
                }
                if (model.LocationFilter.Count > 0)
                {
                    var locations = model.LocationFilter;
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
                    Query = Query + string.Format(@" AND A.FROM_LOCATION_CODE IN ({0} OR LOCATION_CODE IN ('{1}'))", locationConditionQuery, string.Join("','", locations));
                    //query = query + string.Format(@" AND (SI.FROM_LOCATION_CODE = LS.LOCATION_CODE OR SI.FROM_LOCATION_CODE IN ('{0}'))", string.Join("','", filters.LocationFilter).ToString());
                }

                if (model.ProductFilter.Count() > 0)
                {
                    var products = model.ProductFilter;
                    var productConditionQuery = string.Empty;
                    for (int i = 0; i < products.Count; i++)
                    {

                        if (i == 0)
                            productConditionQuery += string.Format("MASTER_ITEM_CODE like (Select DISTINCT(MASTER_ITEM_CODE) || '%' from IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G' )", products[i], companyCode);
                        else
                        {
                            productConditionQuery += string.Format(" OR  MASTER_ITEM_CODE like (Select DISTINCT(MASTER_ITEM_CODE) || '%'  from IP_ITEM_MASTER_SETUP WHERE ITEM_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G')", products[i], companyCode);
                        }
                    }

                    Query = Query + string.Format(@" AND A.ITEM_CODE IN (SELECT DISTINCT(ITEM_CODE) FROM IP_ITEM_MASTER_SETUP WHERE {0} OR (ITEM_CODE IN ('{1}') AND GROUP_SKU_FLAG='I'))", productConditionQuery, string.Join("','", products));

                }
                if (model.BranchFilter.Count > 0)
                {
                    Query += string.Format(@" AND a.BRANCH_CODE IN ('{0}')", string.Join("','", model.BranchFilter).ToString());
                }
                int min = 0, max = 0;
                ReportFilterHelper.RangeFilterValue(model.AmountRangeFilter, out min, out max);

                if (!(min == 0 && max == 0))
                {
                    Query += string.Format(@" AND A.TOTAL_PRICE >= {0} AND A.TOTAL_PRICE <= {1}", min, max);
                }

                Query += $@" AND A.COMPANY_CODE IN({companyCode})  ORDER BY A.AREA_CODE) A) F";
                var Data = this._objectEntity.SqlQuery<SalesRegisterDetailModel>(Query).ToList();

                string querydynamic = string.Format(@"select  * from (SELECT distinct   CUSTOMER_EDESC AS Description,MASTER_CUSTOMER_CODE as MasterCodeWithoutReplace,PRE_CUSTOMER_CODE as PreCodeWithoutReplace,
                                   TO_CHAR(TO_NUMBER(REPLACE(MASTER_CUSTOMER_CODE,'.',''))) as MasterCode, TO_CHAR(TO_NUMBER(CUSTOMER_CODE)) as Code, TO_CHAR(TO_NUMBER(REPLACE(PRE_CUSTOMER_CODE,'.',''))) as PreCode ,group_sku_flag
                                    FROM SA_CUSTOMER_SETUP WHERE  DELETED_FLAG = 'N' )  start with PreCode='0'  CONNECT BY PRIOR MasterCode = PreCode ");


                //querydynamic += string.Format(" AND COMPANY_CODE IN('{0}') ", companyCode);


                var dynamicdata = _objectEntity.SqlQuery<AgeingGroupDataNCR>(querydynamic);

                var dynamicColumnQuery = $@"select v.sub_edesc,s.customer_code,v.party_type_code,sum(v.dr_amount) dramount,
                            FN_FETCH_DESC(v.company_code,'FA_CHART_OF_ACCOUNTS_SETUP',v.acc_code) accountname from V$VIRTUAL_SUB_dealer_LEDGER  v ,
                            sa_customer_setup s 
                            where v.company_code=s.company_code
                            and trim(v.sub_code)=trim(s.link_sub_code)
                            and v.deleted_flag='N'
                            and v.acc_code in('201399','201396','201398','201397','201395','201394')
                            AND v.voucher_date >= TO_DATE('{model.FromDate}', 'YYYY-MON-DD')
                                AND v.voucher_date <= TO_DATE('{model.ToDate}',' YYYY-MON-DD')
                            group by v.sub_edesc,s.customer_code,v.party_type_code,FN_FETCH_DESC(v.company_code,'FA_CHART_OF_ACCOUNTS_SETUP',v.acc_code)";

                var columnsname = this._objectEntity.SqlQuery<DynamicColumns>(dynamicColumnQuery).ToList();

                var finaldata = Data.GroupBy(customer => customer.CUSTOMER_CODE).Select(group => group.First()).ToList();
                foreach (var item in finaldata)
                {
                    Decimal resultt;
                    decimal result;
                    item.QUANTITY = Data.Where(x => x.CUSTOMER_CODE == item.CUSTOMER_CODE).Sum(x => x.QUANTITY);
                    item.DISCOUNT_AMT = Data.Where(x => x.CUSTOMER_CODE == item.CUSTOMER_CODE).Sum(x => x.DISCOUNT_AMT);
                    item.DISCOUNTED_AMOUNT = Data.Where(x => x.CUSTOMER_CODE == item.CUSTOMER_CODE).Sum(x => decimal.TryParse(x.DISCOUNTED_AMOUNT,out resultt)?resultt:0).ToString();
                    item.Special_Discount_Scheme = Data.Where(x => x.CUSTOMER_CODE == item.CUSTOMER_CODE).Sum(x => x.Special_Discount_Scheme);
                    item.TOTAL_PRICE = Data.Where(x => x.CUSTOMER_CODE == item.CUSTOMER_CODE).Sum(x => x.TOTAL_PRICE);
                    item.EXCISE_DUTY = Data.Where(x => x.CUSTOMER_CODE == item.CUSTOMER_CODE).Sum(x => x.EXCISE_DUTY);
                    item.VAT_AMOUNT = Data.Where(x => x.CUSTOMER_CODE == item.CUSTOMER_CODE).Sum(x => x.VAT_AMOUNT);
                    item.TOTAL_BILL_VALUE = Data.Where(x => x.CUSTOMER_CODE == item.CUSTOMER_CODE).Sum(x => x.TOTAL_BILL_VALUE);
                    item.GROSS_REALISATION_AMOUNT = item.TOTAL_PRICE - (Decimal.TryParse(item.DISCOUNT_AMT.ToString(), out result) ? result : 0) ;
                    item.GROSS_REALISATION_PER_QUANTITY = item.GROSS_REALISATION_AMOUNT / item.QUANTITY;
                    var ledgerData = columnsname.Where(x => x.customer_code == item.CUSTOMER_CODE).ToList();
                    if (ledgerData.Count > 0)
                    {
                        if (ledgerData.Any(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-Target".ToUpper().Trim())))
                        {
                            item.TargetBonus = ledgerData.Where(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-Target".ToUpper().Trim())).Sum(x => x.dramount);
                        }
                        if (ledgerData.Any(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-Monopoly".ToUpper().Trim())))
                        {
                            item.MonopolyBonus = ledgerData.Where(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-Monopoly".ToUpper().Trim())).Sum(x => x.dramount);
                        }
                        if (ledgerData.Any(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-CMTP Scheme".ToUpper().Trim())))
                        {
                            item.CMTPScheme = ledgerData.Where(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-CMTP Scheme".ToUpper().Trim())).Sum(x => x.dramount);
                        }
                        if (ledgerData.Any(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-VPB Scheme".ToUpper().Trim())))
                        {
                            item.VPBScheme = ledgerData.Where(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-VPB Scheme".ToUpper().Trim())).Sum(x => x.dramount);
                        }
                        if (ledgerData.Any(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-BG".ToUpper().Trim())))
                        {
                            item.BgBonus = ledgerData.Where(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-BG".ToUpper().Trim())).Sum(x => x.dramount);
                        }
                        if (ledgerData.Any(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-Collection".ToUpper().Trim())))
                        {
                            item.CollectionBonus = ledgerData.Where(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-Collection".ToUpper().Trim())).Sum(x => x.dramount);
                        }
                    }
                }

               foreach( var col in columnsname)
                {
                    if(finaldata.Where(x => x.CUSTOMER_CODE == col.customer_code).FirstOrDefault()==null)
                    {
                        var customerdata = new SalesRegisterDetailModel();
                        var getcustomerdata = dynamicdata.Where(x => x.Code == col.customer_code).FirstOrDefault();
                        if (getcustomerdata == null)
                            continue;

                        customerdata.CUSTOMER_CODE = getcustomerdata.Code;
                        customerdata.CUSTOMER_EDESC = getcustomerdata.Description;
                        customerdata.MasterCode = getcustomerdata.MasterCodeWithoutReplace;
                        customerdata.PreCode = getcustomerdata.PreCodeWithoutReplace;
                        customerdata.QUANTITY =0;
                        customerdata.DISCOUNT_AMT = 0;
                        customerdata.DISCOUNTED_AMOUNT = "0".ToString();
                        customerdata.Special_Discount_Scheme = 0;
                        customerdata.TOTAL_PRICE = 0;
                        customerdata.EXCISE_DUTY = 0;
                        customerdata.VAT_AMOUNT = 0;
                        customerdata.TOTAL_BILL_VALUE = 0;
                        Decimal result;
                        customerdata.GROSS_REALISATION_AMOUNT = customerdata.TOTAL_PRICE - (Decimal.TryParse(customerdata.DISCOUNT_AMT.ToString(), out result) ? result : 0);
                        customerdata.GROSS_REALISATION_PER_QUANTITY = customerdata.QUANTITY>0?customerdata.GROSS_REALISATION_AMOUNT / customerdata.QUANTITY: customerdata.GROSS_REALISATION_AMOUNT / 1;
                        var ledgerData = columnsname.Where(x => x.customer_code == col.customer_code).ToList();
                        if (ledgerData.Count > 0)
                        {
                            if (ledgerData.Any(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-Target".ToUpper().Trim())))
                            {
                                customerdata.TargetBonus = ledgerData.Where(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-Target".ToUpper().Trim())).Sum(x => x.dramount);
                            }
                            if (ledgerData.Any(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-Monopoly".ToUpper().Trim())))
                            {
                                customerdata.MonopolyBonus = ledgerData.Where(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-Monopoly".ToUpper().Trim())).Sum(x => x.dramount);
                            }
                            if (ledgerData.Any(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-CMTP Scheme".ToUpper().Trim())))
                            {
                                customerdata.CMTPScheme = ledgerData.Where(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-CMTP Scheme".ToUpper().Trim())).Sum(x => x.dramount);
                            }
                            if (ledgerData.Any(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-VPB Scheme".ToUpper().Trim())))
                            {
                                customerdata.VPBScheme = ledgerData.Where(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-VPB Scheme".ToUpper().Trim())).Sum(x => x.dramount);
                            }
                            if (ledgerData.Any(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-BG".ToUpper().Trim())))
                            {
                                customerdata.BgBonus = ledgerData.Where(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-BG".ToUpper().Trim())).Sum(x => x.dramount);
                            }
                            if (ledgerData.Any(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-Collection".ToUpper().Trim())))
                            {
                                customerdata.CollectionBonus = ledgerData.Where(x => x.accountname.ToUpper().Trim().Equals("Sales Commission-Collection".ToUpper().Trim())).Sum(x => x.dramount);
                            }
                        }
                        finaldata.Add(customerdata);
                    }
                }

                List<DailySalesTreeList> newTotalList = new List<DailySalesTreeList>();
                try
                {
                    foreach (var groupItem in dynamicdata)
                    {
                        var dataSales = new DailySalesTreeList();

                        if (groupItem.PreCode == "0")
                        {
                            dataSales.parentId = null;
                        }
                        else
                        {
                            dataSales.parentId = groupItem.PreCode.ToString();
                        }

                        dataSales.Description = groupItem.Description;
                        if (groupItem.group_sku_flag == "G")
                        {
                            dataSales.Id = groupItem.MasterCode.ToString();
                            dataSales.QUANTITY = finaldata.Where(x => x.PreCode.StartsWith(groupItem.MasterCodeWithoutReplace.ToString())).Sum(x => x.QUANTITY);
                            if (dataSales.QUANTITY == 0)
                                continue;
                            decimal result;
                            decimal test;
                            decimal DISCOUNTED_AMOUNT;
                            decimal Special_Discount_Scheme;
                            decimal exciseduty;
                            dataSales.DISCOUNT_AMT = decimal.TryParse(finaldata.Where(x => x.PreCode.StartsWith(groupItem.MasterCodeWithoutReplace.ToString())).Sum(x => x.DISCOUNT_AMT).ToString(),out result)?result:0;
                            dataSales.DISCOUNTED_AMOUNT =decimal.TryParse(finaldata.Where(x => x.PreCode.StartsWith(groupItem.MasterCodeWithoutReplace.ToString())).Sum(x => decimal.TryParse(x.DISCOUNTED_AMOUNT,out test)?test:0).ToString(),out DISCOUNTED_AMOUNT) ? DISCOUNTED_AMOUNT : 0;
                            dataSales.Special_Discount_Scheme = decimal.TryParse(finaldata.Where(x => x.PreCode.StartsWith(groupItem.MasterCodeWithoutReplace.ToString())).Sum(x => x.Special_Discount_Scheme).ToString(), out Special_Discount_Scheme) ? Special_Discount_Scheme : 0 ;
                            dataSales.TOTAL_PRICE = finaldata.Where(x => x.PreCode.StartsWith(groupItem.MasterCodeWithoutReplace.ToString())).Sum(x => x.TOTAL_PRICE);
                            dataSales.EXCISE_DUTY = decimal.TryParse(finaldata.Where(x => x.PreCode.StartsWith(groupItem.MasterCodeWithoutReplace.ToString())).Sum(x => x.EXCISE_DUTY).ToString(),out exciseduty)?exciseduty:0;
                            dataSales.VAT_AMOUNT = finaldata.Where(x => x.PreCode.StartsWith(groupItem.MasterCodeWithoutReplace.ToString())).Sum(x => x.VAT_AMOUNT);
                            dataSales.TOTAL_BILL_VALUE = finaldata.Where(x => x.PreCode.StartsWith(groupItem.MasterCodeWithoutReplace.ToString())).Sum(x => x.TOTAL_BILL_VALUE);
                            dataSales.GROSS_REALISATION_AMOUNT = finaldata.Where(x => x.PreCode.StartsWith(groupItem.MasterCodeWithoutReplace.ToString())).Sum(x => x.GROSS_REALISATION_AMOUNT);
                            dataSales.GROSS_REALISATION_PER_QUANTITY = finaldata.Where(x => x.PreCode.StartsWith(groupItem.MasterCodeWithoutReplace.ToString())).Sum(x => x.GROSS_REALISATION_PER_QUANTITY);
                            dataSales.TargetBonus = finaldata.Where(x => x.PreCode.StartsWith(groupItem.MasterCodeWithoutReplace.ToString())).Sum(x => x.TargetBonus);
                            dataSales.CollectionBonus = finaldata.Where(x => x.PreCode.StartsWith(groupItem.MasterCodeWithoutReplace.ToString())).Sum(x => x.CollectionBonus);
                            dataSales.MonopolyBonus = finaldata.Where(x => x.PreCode.StartsWith(groupItem.MasterCodeWithoutReplace.ToString())).Sum(x => x.MonopolyBonus);
                            dataSales.BgBonus = finaldata.Where(x => x.PreCode.StartsWith(groupItem.MasterCodeWithoutReplace.ToString())).Sum(x => x.BgBonus);
                            dataSales.CMTPScheme = finaldata.Where(x => x.PreCode.StartsWith(groupItem.MasterCodeWithoutReplace.ToString())).Sum(x => x.CMTPScheme);
                            dataSales.VPBScheme = finaldata.Where(x => x.PreCode.StartsWith(groupItem.MasterCodeWithoutReplace.ToString())).Sum(x => x.VPBScheme);
                            dataSales.TotalBonus = dataSales.TargetBonus + dataSales.CollectionBonus + dataSales.MonopolyBonus + dataSales.BgBonus + dataSales.CMTPScheme + dataSales.VPBScheme;
                            dataSales.TotalBonusPerQty = dataSales.QUANTITY>0? dataSales.TotalBonus / dataSales.QUANTITY: dataSales.TotalBonus / 1;
                            dataSales.NCRAmount = dataSales.GROSS_REALISATION_AMOUNT - dataSales.TotalBonus;
                            dataSales.NCRPerQty = dataSales.QUANTITY > 0 ? dataSales.NCRAmount / dataSales.QUANTITY: dataSales.NCRAmount;
                        }
                        else
                        {
                            dataSales.Id = groupItem.Code.ToString();
                            dataSales.QUANTITY = finaldata.Where(x => x.CUSTOMER_CODE.Equals(groupItem.Code.ToString())).Sum(x => x.QUANTITY);
                            if (dataSales.QUANTITY == 0)
                                continue;
                            decimal customerDiscount;
                            decimal disAmount;
                            decimal cusDiscounted;
                            decimal specialDiscount;
                            decimal custExcise;
                            dataSales.DISCOUNT_AMT = decimal.TryParse(finaldata.Where(x => x.CUSTOMER_CODE.Equals(groupItem.Code.ToString())).Sum(x => x.DISCOUNT_AMT).ToString(),out customerDiscount)?customerDiscount:0;
                            dataSales.DISCOUNTED_AMOUNT = decimal.TryParse(finaldata.Where(x => x.CUSTOMER_CODE.Equals(groupItem.Code.ToString())).Sum(x => decimal.TryParse(x.DISCOUNTED_AMOUNT,out disAmount)?disAmount:0).ToString(),out cusDiscounted)?customerDiscount:0;
                            dataSales.Special_Discount_Scheme = decimal.TryParse(finaldata.Where(x => x.CUSTOMER_CODE.Equals(groupItem.Code.ToString())).Sum(x => x.Special_Discount_Scheme).ToString(),out specialDiscount)?specialDiscount:0;
                            dataSales.TOTAL_PRICE = finaldata.Where(x => x.CUSTOMER_CODE.Equals(groupItem.Code.ToString())).Sum(x => x.TOTAL_PRICE);
                            dataSales.EXCISE_DUTY = decimal.TryParse(finaldata.Where(x => x.CUSTOMER_CODE.Equals(groupItem.Code.ToString())).Sum(x => x.EXCISE_DUTY).ToString(),out custExcise)?custExcise:0;
                            dataSales.VAT_AMOUNT = finaldata.Where(x => x.CUSTOMER_CODE.Equals(groupItem.Code.ToString())).Sum(x => x.VAT_AMOUNT);
                            dataSales.TOTAL_BILL_VALUE = finaldata.Where(x => x.CUSTOMER_CODE.Equals(groupItem.Code.ToString())).Sum(x => x.TOTAL_BILL_VALUE);
                            dataSales.GROSS_REALISATION_AMOUNT = finaldata.Where(x => x.CUSTOMER_CODE.Equals(groupItem.Code.ToString())).Sum(x => x.GROSS_REALISATION_AMOUNT);
                            dataSales.GROSS_REALISATION_PER_QUANTITY = finaldata.Where(x => x.CUSTOMER_CODE.Equals(groupItem.Code.ToString())).Sum(x => x.GROSS_REALISATION_PER_QUANTITY);
                            dataSales.TargetBonus = finaldata.Where(x => x.CUSTOMER_CODE.Equals(groupItem.Code.ToString())).Sum(x => x.TargetBonus);
                            dataSales.CollectionBonus = finaldata.Where(x => x.CUSTOMER_CODE.Equals(groupItem.Code.ToString())).Sum(x => x.CollectionBonus);
                            dataSales.MonopolyBonus = finaldata.Where(x => x.CUSTOMER_CODE.Equals(groupItem.Code.ToString())).Sum(x => x.MonopolyBonus);
                            dataSales.BgBonus = finaldata.Where(x => x.CUSTOMER_CODE.Equals(groupItem.Code.ToString())).Sum(x => x.BgBonus);
                            dataSales.CMTPScheme = finaldata.Where(x => x.CUSTOMER_CODE.Equals(groupItem.Code.ToString())).Sum(x => x.CMTPScheme);
                            dataSales.VPBScheme = finaldata.Where(x => x.CUSTOMER_CODE.Equals(groupItem.Code.ToString())).Sum(x => x.VPBScheme);
                            dataSales.TotalBonus = dataSales.TargetBonus + dataSales.CollectionBonus + dataSales.MonopolyBonus + dataSales.BgBonus + dataSales.CMTPScheme + dataSales.VPBScheme;
                            dataSales.TotalBonusPerQty = dataSales.QUANTITY > 0 ? dataSales.TotalBonus / dataSales.QUANTITY: dataSales.TotalBonus;
                            dataSales.NCRAmount = dataSales.GROSS_REALISATION_AMOUNT - dataSales.TotalBonus;
                            dataSales.NCRPerQty = dataSales.QUANTITY > 0 ? dataSales.NCRAmount / dataSales.QUANTITY: dataSales.NCRAmount;
                        }
                        newTotalList.Add(dataSales);


                    }
                }
                catch (Exception ex)
                {

                }
                if (newTotalList.Count > 0)
                {

                    var TotalColumndata = new DailySalesTreeList();
                    TotalColumndata.Description = "Total";
                    TotalColumndata.parentId = null;
                    TotalColumndata.Id = "9856985";
                    TotalColumndata.QUANTITY = newTotalList.Where(x => x.parentId == null).Sum(x => x.QUANTITY);

                    TotalColumndata.DISCOUNT_AMT = newTotalList.Where(x => x.parentId == null).Sum(x => x.DISCOUNT_AMT);
                    TotalColumndata.DISCOUNTED_AMOUNT = newTotalList.Where(x => x.parentId == null).Sum(x => x.DISCOUNTED_AMOUNT);
                    TotalColumndata.Special_Discount_Scheme = newTotalList.Where(x => x.parentId == null).Sum(x => x.Special_Discount_Scheme);
                    TotalColumndata.TOTAL_PRICE = newTotalList.Where(x => x.parentId == null).Sum(x => x.TOTAL_PRICE);
                    TotalColumndata.EXCISE_DUTY = newTotalList.Where(x => x.parentId == null).Sum(x => x.EXCISE_DUTY);
                    TotalColumndata.VAT_AMOUNT = newTotalList.Where(x => x.parentId == null).Sum(x => x.VAT_AMOUNT);
                    TotalColumndata.TOTAL_BILL_VALUE = newTotalList.Where(x => x.parentId == null).Sum(x => x.TOTAL_BILL_VALUE);
                    TotalColumndata.GROSS_REALISATION_AMOUNT = newTotalList.Where(x => x.parentId == null).Sum(x => x.GROSS_REALISATION_AMOUNT);
                    TotalColumndata.GROSS_REALISATION_PER_QUANTITY = newTotalList.Where(x => x.parentId == null).Sum(x => x.GROSS_REALISATION_PER_QUANTITY);
                    TotalColumndata.TargetBonus = newTotalList.Where(x => x.parentId == null).Sum(x => x.TargetBonus);
                    TotalColumndata.CollectionBonus = newTotalList.Where(x => x.parentId == null).Sum(x => x.CollectionBonus);
                    TotalColumndata.MonopolyBonus = newTotalList.Where(x => x.parentId == null).Sum(x => x.MonopolyBonus);
                    TotalColumndata.BgBonus = newTotalList.Where(x => x.parentId == null).Sum(x => x.BgBonus);
                    TotalColumndata.CMTPScheme = newTotalList.Where(x => x.parentId == null).Sum(x => x.CMTPScheme);
                    TotalColumndata.VPBScheme = newTotalList.Where(x => x.parentId == null).Sum(x => x.VPBScheme);
                    TotalColumndata.TotalBonus = newTotalList.Where(x => x.parentId == null).Sum(x => x.TotalBonus);
                    TotalColumndata.TotalBonusPerQty = newTotalList.Where(x => x.parentId == null).Sum(x => x.TotalBonusPerQty);
                    TotalColumndata.NCRAmount = newTotalList.Where(x => x.parentId == null).Sum(x => x.NCRAmount);
                    TotalColumndata.NCRPerQty = newTotalList.Where(x => x.parentId == null).Sum(x => x.NCRPerQty);
                    newTotalList.Add(TotalColumndata);
                }
                this._cacheManager.Set(cachekey, newTotalList, 20);
                return newTotalList;
            }
         
          //  return newTotalList;
        }

        public IEnumerable<GoodsReceiptNotesDetailModel> GetGoodsReceiptNotesData(ReportFiltersModel reportFilters, User userInfo, bool liveData)
        {

            //var companyCode = string.Join(", ", reportFilters.CompanyFilter);
            //companyCode = companyCode == "" ? userInfo.company_code : companyCode;
            var companyCode = string.Empty;
            foreach (var company in reportFilters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }

            companyCode = companyCode == "" ? $@"'{userInfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);


            IEnumerable<GoodsReceiptNotesDetailModel> data = Enumerable.Empty<GoodsReceiptNotesDetailModel>();
            var data1 = new List<GoodsReceiptNotesDetailModel>();

            string path = System.Web.HttpContext.Current.Server.MapPath("~/App_Files/json/GoodsReceiptNotesReport.json");
            IMongoRepository<GoodsReceiptNotesDetailModelMongo> _ItemRepo = new MongoRepository<GoodsReceiptNotesDetailModelMongo>();
            if (liveData == false)
            {


                var datatest = _ItemRepo.ToList();
                foreach (var d in datatest)
                {
                    var mongodb = new GoodsReceiptNotesDetailModel();
                    mongodb.Id = Convert.ToInt16(d.Id);
                    mongodb.MRR_DATE = d.MRR_DATE;
                    mongodb.BRANCH_CODE = d.BRANCH_CODE;
                    mongodb.BRANCH_EDESC = d.BRANCH_EDESC;
                    mongodb.MRR_NO = d.MRR_NO;
                    mongodb.MANUAL_NO = d.MANUAL_NO;
                    mongodb.SUPPLIER_CODE = d.SUPPLIER_CODE;
                    mongodb.SUPPLIER_NAME = d.SUPPLIER_NAME;
                    mongodb.SUPPLIER_INV_NO = d.SUPPLIER_INV_NO;
                    mongodb.SUPPLIER_MRR_NO = d.SUPPLIER_MRR_NO;
                    mongodb.SUPPLIER_INV_DATE = d.SUPPLIER_INV_DATE;
                    mongodb.PP_NO = d.PP_NO;
                    mongodb.REMARKS = d.REMARKS;
                    mongodb.CURRENCY_CODE = d.CURRENCY_CODE;
                    mongodb.EXCHANGE_RATE = d.EXCHANGE_RATE;
                    mongodb.LOCATION_EDESC = d.LOCATION_EDESC;
                    mongodb.ITEM_CODE = d.ITEM_CODE;
                    mongodb.ITEM_EDESC = d.ITEM_EDESC;
                    mongodb.QUANTITY = d.QUANTITY;
                    mongodb.UNIT_PRICE = d.UNIT_PRICE;
                    mongodb.TOTAL_PRICE = d.TOTAL_PRICE;
                    mongodb.FORM_EDESC = d.FORM_EDESC;
                    mongodb.ITEM_GROUP_EDESC = d.ITEM_GROUP_EDESC;
                    mongodb.ITEM_SUBGROUP_EDESC = d.ITEM_SUBGROUP_EDESC;
                    mongodb.CATEGORY_CODE = d.CATEGORY_CODE;
                    mongodb.CATEGORY_EDESC = d.CATEGORY_EDESC;
                    mongodb.COMPANY_CODE = d.COMPANY_CODE;
                    mongodb.COMPANY_EDESC = d.COMPANY_EDESC;
                    mongodb.BRANCH_CODE = d.BRANCH_CODE;
                    mongodb.BRANCH_EDESC = d.BRANCH_EDESC;
                    data1.Add(mongodb);
                    //  var acc = _mapper.Map<GoodsReceiptNotesDetailModelMongo>(d);
                    ///Mongodata.Add(_mapper.Map<GoodsReceiptNotesDetailModelMongo>(d));
                }
                data = data1.AsEnumerable<GoodsReceiptNotesDetailModel>();
                // var data1=   _mapper.Map<GoodsReceiptNotesDetailModel>(datatest.FirstOrDefault());
                //using (StreamReader r = new StreamReader(path))
                //{
                //    string jsonData = r.ReadToEnd();
                //    data = Newtonsoft.Json.JsonConvert.DeserializeObject<EnumerableQuery<GoodsReceiptNotesDetailModel>>(jsonData);
                //}
            }
            else
            {
                var Query = $@"SELECT ROWNUM Id, A.MRR_NO,TO_DATE(A.MRR_DATE) MRR_DATE,A.MANUAL_NO,A.SUPPLIER_CODE , B.SUPPLIER_EDESC SUPPLIER_NAME ,A.SUPPLIER_INV_NO,
                            A.SUPPLIER_MRR_NO, TO_DATE(A.SUPPLIER_INV_DATE) SUPPLIER_INV_DATE , 
                            A.PP_NO, A.REMARKS,  A.CURRENCY_CODE, A.EXCHANGE_RATE ,C.LOCATION_EDESC,D.ITEM_CODE,
                            D.ITEM_EDESC , A.QUANTITY ,A.UNIT_PRICE, A.TOTAL_PRICE, E.FORM_EDESC,
                            SUBSTR(FN_FETCH_GROUP_DESC(A.COMPANY_CODE,'IP_ITEM_MASTER_SETUP', D.PRE_ITEM_CODE),1,100) ITEM_GROUP_EDESC,
                            SUBSTR(FN_FETCH_PRE_DESC(A.COMPANY_CODE,'IP_ITEM_MASTER_SETUP', D.PRE_ITEM_CODE),1,100) ITEM_SUBGROUP_EDESC,
                            D.CATEGORY_CODE,FN_FETCH_DESC(A.COMPANY_CODE,'IP_CATEGORY_CODE',D.CATEGORY_CODE) CATEGORY_EDESC,
                            A.COMPANY_CODE,FN_FETCH_DESC(A.COMPANY_CODE,'COMPANY_SETUP',A.COMPANY_CODE) COMPANY_EDESC,
                            A.BRANCH_CODE,FN_FETCH_DESC(A.COMPANY_CODE,'FA_BRANCH_SETUP',A.BRANCH_CODE) BRANCH_EDESC
                            FROM IP_PURCHASE_MRR A , IP_SUPPLIER_SETUP B, IP_LOCATION_SETUP C, IP_ITEM_MASTER_SETUP D , FORM_SETUP E
                            WHERE A.SUPPLIER_CODE = B.SUPPLIER_CODE
                            AND A.COMPANY_CODE = B.COMPANY_CODE
                            AND A.TO_LOCATION_CODE = C.LOCATION_CODE (+)
                            AND A.COMPANY_CODE = C.COMPANY_CODE (+)
                            AND A.ITEM_CODE = D.ITEM_CODE
                            AND A.COMPANY_CODE = D.COMPANY_CODE
                            AND A.FORM_CODE = E.FORM_CODE
                            AND A.COMPANY_CODE = E.COMPANY_CODE";
                data = this._objectEntity.SqlQuery<GoodsReceiptNotesDetailModel>(Query).ToList();
                List<GoodsReceiptNotesDetailModelMongo> Mongodata = new List<GoodsReceiptNotesDetailModelMongo>();
                foreach (var d in data)
                {
                    var mongodb = new GoodsReceiptNotesDetailModelMongo();
                    mongodb.Id = d.Id.ToString();
                    mongodb.MRR_DATE = d.MRR_DATE;
                    mongodb.BRANCH_CODE = d.BRANCH_CODE;
                    mongodb.BRANCH_EDESC = d.BRANCH_EDESC;
                    mongodb.MRR_NO = d.MRR_NO;
                    mongodb.MANUAL_NO = d.MANUAL_NO;
                    mongodb.SUPPLIER_CODE = d.SUPPLIER_CODE;
                    mongodb.SUPPLIER_NAME = d.SUPPLIER_NAME;
                    mongodb.SUPPLIER_INV_NO = d.SUPPLIER_INV_NO;
                    mongodb.SUPPLIER_MRR_NO = d.SUPPLIER_MRR_NO;
                    mongodb.SUPPLIER_INV_DATE = d.SUPPLIER_INV_DATE;
                    mongodb.PP_NO = d.PP_NO;
                    mongodb.REMARKS = d.REMARKS;
                    mongodb.CURRENCY_CODE = d.CURRENCY_CODE;
                    mongodb.EXCHANGE_RATE = d.EXCHANGE_RATE;
                    mongodb.LOCATION_EDESC = d.LOCATION_EDESC;
                    mongodb.ITEM_CODE = d.ITEM_CODE;
                    mongodb.ITEM_EDESC = d.ITEM_EDESC;
                    mongodb.QUANTITY = d.QUANTITY;
                    mongodb.UNIT_PRICE = d.UNIT_PRICE;
                    mongodb.TOTAL_PRICE = d.TOTAL_PRICE;
                    mongodb.FORM_EDESC = d.FORM_EDESC;
                    mongodb.ITEM_GROUP_EDESC = d.ITEM_GROUP_EDESC;
                    mongodb.ITEM_SUBGROUP_EDESC = d.ITEM_SUBGROUP_EDESC;
                    mongodb.CATEGORY_CODE = d.CATEGORY_CODE;
                    mongodb.CATEGORY_EDESC = d.CATEGORY_EDESC;
                    mongodb.COMPANY_CODE = d.COMPANY_CODE;
                    mongodb.COMPANY_EDESC = d.COMPANY_EDESC;
                    mongodb.BRANCH_CODE = d.BRANCH_CODE;
                    mongodb.BRANCH_EDESC = d.BRANCH_EDESC;
                    Mongodata.Add(mongodb);
                    //  var acc = _mapper.Map<GoodsReceiptNotesDetailModelMongo>(d);
                    ///Mongodata.Add(_mapper.Map<GoodsReceiptNotesDetailModelMongo>(d));
                }
                //var mongodata=  _mapper.Map<List<GoodsReceiptNotesDetailModel>,List<GoodsReceiptNotesDetailModelMongo>>(data.ToList());
                _ItemRepo.Drop();
                _ItemRepo.AddMany(Mongodata);

                //save to jsonfile
                //  string file = System.Web.HttpContext.Current.Server.MapPath("~/App_Files/json/GoodsReceiptNotesReport.json");
                //  string json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
                // File.WriteAllText(file, json);

                //save property               
                //  var fs = File.GetAccessControl(file);
                //  var sid = fs.GetOwner(typeof(SecurityIdentifier));                
                //var ntAccount = sid.Translate(typeof(NTAccount)).ToString();

                //var setting = new GoodsReceiptNotesReportJsonSetting()
                //{
                //     SID = sid.ToString(),
                //    CreatedBy = ntAccount,
                //   CreatedDate = File.GetCreationTime(file),
                //    ModifyDate = File.GetLastWriteTime(file)
                //  };               
                // _setting.SaveSetting(setting);

            }



            //****************************
            //CONDITIONS FITLER START HERE
            //****************************

            //companyFilter
            data = data.Where(x => x.COMPANY_CODE.Contains(companyCode));

            //branchFilter
            if (reportFilters.BranchFilter.Count() > 0)
            {
                data = data.Where(x => reportFilters.BranchFilter.Contains(x.BRANCH_CODE));
            }


            //supplierFilter
            if (reportFilters.SupplierFilter.Count() > 0)
            {
                var selectedSupplier = getSelectedSuplierFromJsonData(_objectEntity, reportFilters);
                data = data.Where(x => selectedSupplier.Contains(x.SUPPLIER_CODE));

            }

            //productFilter
            if (reportFilters.ProductFilter.Count() > 0)
            {
                var selectedItems = getSelectedProductFromJsonData(_objectEntity, reportFilters);
                data = data.Where(x => selectedItems.Contains(x.ITEM_CODE));

            }

            //categoryFilter
            if (reportFilters.CategoryFilter.Count() > 0)
            {
                data = data.Where(x => reportFilters.CategoryFilter.Contains(x.CATEGORY_CODE));
            }




            //range filter
            int min = 0, max = 0;
            ReportFilterHelper.RangeFilterValue(reportFilters.QuantityRangeFilter, out min, out max);
            if (!(min == 0 && max == 0))
            {
                data = data.Where(x => x.TOTAL_PRICE >= min && x.TOTAL_PRICE <= max);
                data = data.Where(x => x.QUANTITY >= min && x.QUANTITY <= max);
            }




            //dateFilter
            if (!string.IsNullOrEmpty(reportFilters.FromDate))
            {
                DateTime fromDate = Convert.ToDateTime(reportFilters.FromDate);
                data = data.Where(x => x.MRR_DATE >= fromDate);
            }
            if (!string.IsNullOrEmpty(reportFilters.ToDate))
            {
                DateTime toDate = Convert.ToDateTime(reportFilters.ToDate);
                data = data.Where(x => x.MRR_DATE <= toDate);
            }

            //amountFormat
            var temp = ReportFilterHelper.FigureFilterValue(reportFilters.AmountFigureFilter);


            ////****************************
            ////CONDITIONS FITLER END HERE
            ////****************************



            return data;
        }

        public List<DynamicColumnForNCR> GetDynamiColumns()
        {
         
            List<DynamicColumnForNCR> staticColumns = new List<DynamicColumnForNCR>();
            staticColumns.Add(new DynamicColumnForNCR { Name = "TargetBonus" });
            staticColumns.Add(new DynamicColumnForNCR { Name = "CollectionBonus" });
            staticColumns.Add(new DynamicColumnForNCR { Name = "MonopolyBonus" });
            staticColumns.Add(new DynamicColumnForNCR { Name = "BgBonus" });
            staticColumns.Add(new DynamicColumnForNCR { Name = "CMTPScheme" });
            staticColumns.Add(new DynamicColumnForNCR { Name = "VPBScheme" });

            return staticColumns;
        }


        public List<MaterializeModel> GetMaterializeReprot(ReportFiltersModel filters, User userInfo,bool sync=false)
        {

            var figureAmountFilter = ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter);
            var roundUpAmountFilter = ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter);
            var companyCode = string.Empty;
            foreach (var company in filters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }

            companyCode = companyCode == "" ? $@"'{userInfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);
            string query = string.Format($@"SELECT FISCAL_YEAR,
            BILL_NO,
            CUSTOMER_NAME,
            CUSTOMER_PAN,
            BS_DATE (BILL_DATE) BILL_DATE,
to_char(BILL_DATE) BILL_DATEAD,
            AMOUNT,
            DISCOUNT,
            TAXABLE_AMOUNT,
               DECODE (IS_BILL_ACTIVE, 'Y', TAX_AMOUNT,0) TAX_AMOUNT,
            TOTAL_AMOUNT,
            SYNC_WITH_IRD,
            IS_BILL_PRINTED,
            IS_BILL_ACTIVE,
            PRINTED_TIME,
            ENTERED_BY,
            PRINTED_BY,
            IS_REAL_TIME,
            COMPANY_CODE,
            BRANCH_CODE,
            FORM_CODE,
(select table_name from form_detail_setup where form_code=v.form_code and company_code=v.company_code and rownum=1)  as TableName
       FROM V_IRD_CBMS_VAT_REPORT v
      WHERE COMPANY_CODE  IN ({companyCode}) ");
            if (filters.BranchFilter.Count > 0)
            {
                query += string.Format(@" AND BRANCH_CODE  IN ('{0}')", string.Join("','", filters.BranchFilter).ToString());
            }

            if (!string.IsNullOrEmpty(filters.FromDate))
                query = query + " AND TO_DATE(BILL_DATE, 'DD/MM/RRRR')>=TO_DATE('" + filters.FromDate + "', 'YYYY-MM-DD') AND TO_DATE(BILL_DATE, 'DD/MM/RRRR') <= TO_DATE('" + filters.ToDate + "', 'YYYY-MM-DD')";

            if (sync)
            {
                query = query + "  and  SYNC_WITH_IRD='N' ";
            }
            else
            {
                query = query + "  ORDER BY FISCAL_YEAR, BILL_DATE, BILL_NO ";
              
            }
          

            var materializeData = _objectEntity.SqlQuery<MaterializeModel>(query).ToList();
            if(sync)
            {
                
                foreach (var data in materializeData.Where(x=>x.SYNC_WITH_IRD=="N"))
                {
                    SynFunctionIRD(data);
                }
               
            }
       
            return materializeData;
        }

        public string SynFunctionIRD(MaterializeModel Model)
        {
            using (var client = new HttpClient())
            {
                //FiscalYear
                var username = ConfigurationManager.AppSettings["username"];
                var password = ConfigurationManager.AppSettings["password"];
                var seller_pan = ConfigurationManager.AppSettings["seller_pan"];
                var fiscal_year = ConfigurationManager.AppSettings["FiscalYear"];
                var IRDUrl = ConfigurationManager.AppSettings["IRDUrl"];
                var invoice_date = Convert.ToDateTime(Model.BILL_DATEAD).ToString("yyyy.MM.dd");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                IRDModel billViewModel = new IRDModel
                {
                    username = username,
                    password = password,
                    seller_pan = seller_pan,
                    buyer_pan = Model.CUSTOMER_PAN,
                    buyer_name = Model.CUSTOMER_NAME,
                    fiscal_year = fiscal_year,
                    invoice_number = Model.BILL_NO,
                    invoice_date = invoice_date,
                    total_sales = Model.TAXABLE_AMOUNT??0,
                    taxable_sales_vat = Model.TAXABLE_AMOUNT ?? 0,
                    vat = Model.TAX_AMOUNT ?? 0,
                    excisable_amount = 0,
                    excise = 0,
                    taxable_sales_hst = 0,
                    hst = 0,
                    amount_for_esf = 0,
                    esf = 0,
                    export_sales = 0,
                    tax_exempted_sales = 0,
                    isrealtime = true,
                    datetimeClient = DateTime.Now
                };
                //if(Model.TableName.ToUpper()=="SA_SALES_RETURN")
                //{
                //    IRDUrl = ConfigurationManager.AppSettings["IRDUrlReturn"];
                //}
                var response = client.PostAsJsonAsync(IRDUrl, billViewModel).Result;
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync();
                    if (result.Result.ToString() == "100")
                    {
                        SaveIRDLog(Model, "API credentials do not match");
                        return "API credentials do not match";
                    }
                    else if (result.Result.ToString() == "101")
                    {
                        SaveIRDLog(Model, "bill already exists");
                        ifIsRealTimeFalse(Model);
                        return "bill already exists";
                    }
                    else if (result.Result.ToString() == "102")
                    {
                        SaveIRDLog(Model, "exception while saving bill details");
                        return "exception while saving bill details";
                    }
                    else if (result.Result.ToString() == "103")
                    {
                        SaveIRDLog(Model, "Unknown exceptions");
                        //ifIsRealTimeFalse(Model);
                        return "Unknown exceptions";
                    }
                    else if (result.Result.ToString() == "104")
                    {
                        SaveIRDLog(Model, "model invalid");
                        // ifIsRealTimeFalse(Model);
                        return "model invalid";
                    }
                    else if (result.Result.ToString() == "200")
                    {
                        SaveIRDLog(Model, "Success");
                        ifIsRealTimeFalse(Model);
                        return result.Status.ToString();
                    }
                    else
                    {
                        SaveIRDLog(Model, "Error NOt Define");
                        ifIsRealTimeFalse(Model);
                        return result.Status.ToString();
                    }
                }
                else
                {
                    var result = response.Content.ReadAsStringAsync();
                    SaveIRDLog(Model, result.Result.ToString());
                    ifIsRealTimeFalse(Model);
                    return "Error";
                }
            }
        }

        public void ifIsRealTimeFalse(MaterializeModel Record)
        {
           
                try
                {
                    var updatMasterTransactionQuery = $@"UPDATE MASTER_TRANSACTION SET IS_SYNC_WITH_IRD='{"Y"}', IS_REAL_TIME='{"N"}' WHERE VOUCHER_NO='{Record.BILL_NO}' AND FORM_CODE='{Record.FORM_CODE}'";
                _objectEntity.ExecuteSqlCommand(updatMasterTransactionQuery);
                   
                }
                catch (Exception)
                {
                    throw;
                }

            
        }
        public void SaveIRDLog(MaterializeModel Record,string Message)
        {

            try
            {
                var updatMasterTransactionQuery = $@"insert into IRD_LOG(VOUCHER_NO,MESSAGE,FORM_CODE,CREATED_DATE) values ('{Record.BILL_NO}','{Message}','{Record.FORM_CODE}',sysdate)";
                _objectEntity.ExecuteSqlCommand(updatMasterTransactionQuery);

            }
            catch (Exception)
            {
                throw;
            }


        }
        public List<MaterializedViewMasterModel> MaterializedViewReport(ReportFiltersModel filters)
        {
            //var companyCode = string.Join(",", filters.CompanyFilter);List<MaterializedViewMasterModel>
            ////companyCode = companyCode == "" ? this._workContext.CurrentUserinformation.company_code : companyCode;
            //if (string.IsNullOrEmpty(companyCode))
            //    companyCode = this._workContext.CurrentUserinformation.company_code;
            var companyCode = string.Empty;
            foreach (var company in filters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }

            companyCode = companyCode == "" ? $@"'{this._workContext.CurrentUserinformation.company_code}'" : companyCode.Remove(companyCode.Length - 1);
            //string query = string.Format(@"SELECT 
            //                                 A.FORM_CODE,
            //                                -- A.FROM_LOCATION_CODE,
            //                                A.COMPANY_CODE,A.BRANCH_CODE,
            //                                  A.SALES_DATE as SalesDate ,A.MITI as Miti,A.SALES_NO as InvoiceNumber,A.CUSTOMER_EDESC as CustomerName,
            //                                  Round(NVL(FN_CONVERT_CURRENCY(NVL(A.GROSS_AMOUNT,0),'NRS',A.SALES_DATE),0)/{0},{1}) as GrossAmount 
            //                                  FROM
            //                                   (SELECT 
            //                                      A.FORM_CODE,A.FROM_LOCATION_CODE,A.COMPANY_CODE,A.BRANCH_CODE,
            //                                      A.PARTY_TYPE_CODE, A.CUSTOMER_CODE, 
            //                                      A.SALES_DATE, BS_DATE(A.SALES_DATE) MITI, A.SALES_NO, B.CUSTOMER_EDESC,
            //                                      SUM(NVL(A.TOTAL_PRICE,0))  GROSS_AMOUNT
            //                                      FROM SA_SALES_INVOICE A, SA_CUSTOMER_SETUP B, IP_ITEM_MASTER_SETUP C
            //                                      WHERE A.CUSTOMER_CODE = B.CUSTOMER_CODE
            //                                      AND A.COMPANY_CODE = B.COMPANY_CODE
            //                                      AND A.ITEM_CODE = C.ITEM_CODE
            //                                      AND A.COMPANY_CODE = C.COMPANY_CODE
            //                                      AND A.COMPANY_CODE IN({2})
            //                                      AND A.DELETED_FLAG = 'N'",
            //                            ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter), ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter), companyCode);

            string query = string.Format($@"SELECT FISCAL_YEAR,
         BILL_NO,
         CUSTOMER_NAME,
         CUSTOMER_PAN,
         BS_DATE (BILL_DATE) BILL_DATE,
         AMOUNT,
         DISCOUNT,
         TAXABLE_AMOUNT,
         TAX_AMOUNT,
         TOTAL_AMOUNT,
         SYNC_WITH_IRD,
         IS_BILL_PRINTED,
         IS_BILL_ACTIVE,
         PRINTED_TIME,
         ENTERED_BY,
         PRINTED_BY,
         IS_REAL_TIME,
         COMPANY_CODE,
         BRANCH_CODE,
         FORM_CODE
    FROM V_IRD_CBMS_VAT_REPORT A WHERE A.COMPANY_CODE IN({companyCode})  ");

            //if (filters.CustomerFilter.Count > 0)
            //{
            //    var customers = filters.CustomerFilter;
            //    var customerConditionQuery = string.Empty;
            //    for (int i = 0; i < customers.Count; i++)
            //    {

            //        if (i == 0)
            //            customerConditionQuery += string.Format("MASTER_CUSTOMER_CODE like (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%' from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G')", customers[i], companyCode);
            //        else
            //        {
            //            customerConditionQuery += string.Format(" OR  MASTER_CUSTOMER_CODE like (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G')", customers[i], companyCode);
            //        }
            //    }
            //    var customerfilter = string.Empty;
            //    foreach (var product in customers)
            //    {
            //        customerfilter += $@"'{product}',";
            //    }
            //    customerfilter = customerfilter.Remove(customerfilter.Length - 1);
            //    query = query + string.Format(@" AND A.CUSTOMER_CODE IN (SELECT DISTINCT(CUSTOMER_CODE) FROM SA_CUSTOMER_SETUP WHERE  {0}  OR (CUSTOMER_CODE IN ({1}) AND GROUP_SKU_FLAG = 'I')) ", customerConditionQuery, customerfilter);

            //}
            if (filters.DocumentFilter.Count > 0)
            {
                query = query + string.Format(@" AND  A.FORM_CODE  IN  ('{0}')", string.Join("','", filters.DocumentFilter).ToString());
            }

            if (filters.BranchFilter.Count > 0)
            {
                query = query + string.Format(@" AND A.BRANCH_CODE IN ('{0}')", string.Join("','", filters.BranchFilter).ToString());
            }
            if (!string.IsNullOrEmpty(filters.FromDate))
                query = query + " and TO_CHAR (TO_DATE (BILL_DATE, 'DD-MM-YYYY')) >=TO_DATE('" + filters.FromDate + "', 'YYYY-MM-DD') and TO_CHAR (TO_DATE (BILL_DATE, 'DD-MM-YYYY'))  <= TO_DATE('" + filters.ToDate + "', 'YYYY-MM-DD')";
            //query = query + " GROUP BY A.SALES_DATE,  A.SALES_NO, B.CUSTOMER_EDESC,  A.PARTY_TYPE_CODE, A.CUSTOMER_CODE,A.FORM_CODE,A.COMPANY_CODE,A.BRANCH_CODE";




            query += " ORDER BY FISCAL_YEAR, BILL_DATE, BILL_NO ";


            var salesRegisters = _objectEntity.SqlQuery<MaterializedViewMasterModel>(query).ToList();


            return salesRegisters;
        }
        public List<VatRegistrationIRDMasterModel> VatRegisterIRDReport(ReportFiltersModel filters)
        {
            var companyCode = string.Empty;
            foreach (var company in filters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }

            companyCode = companyCode == "" ? $@"'{this._workContext.CurrentUserinformation.company_code}'" : companyCode.Remove(companyCode.Length - 1);
            
            string query = string.Format($@"SELECT BS_DATE (SALES_DATE) MITI,INVOICE_NO,PARTY_NAME,VAT_NO,
         FN_CONVERT_CURRENCY (NVL (GROSS_SALES, 0) * NVL (EXCHANGE_RATE, 1),'NRS',SALES_DATE)GROSS_SALES,
         FN_CONVERT_CURRENCY (NVL (TAXABLE_SALES, 1) * NVL (EXCHANGE_RATE, 1),'NRS',SALES_DATE)TAXABLE_SALES,
         FN_CONVERT_CURRENCY (NVL (VAT, 0) * NVL (EXCHANGE_RATE, 1),'NRS',SALES_DATE)VAT,
         FN_CONVERT_CURRENCY (NVL (TOTAL_SALES, 0) * NVL (EXCHANGE_RATE, 1),'NRS',SALES_DATE)TOTAL_SALES,
         FORM_CODE,
         BRANCH_CODE,
         CREDIT_DAYS,
         DELETED_FLAG,
         SALES_DISCOUNT,
         MANUAL_NO,
         0 ZERO_RATE_EXPORT
        FROM V$SALES_INVOICE_REPORT3
       WHERE     COMPANY_CODE IN({companyCode})  ");

          
            if (filters.DocumentFilter.Count > 0)
            {
                query = query + string.Format(@" AND  FORM_CODE  IN  ('{0}')", string.Join("','", filters.DocumentFilter).ToString());
            }

            if (filters.BranchFilter.Count > 0)
            {
                query = query + string.Format(@" AND BRANCH_CODE IN ('{0}')", string.Join("','", filters.BranchFilter).ToString());
            }
            if (!string.IsNullOrEmpty(filters.FromDate))
                query = query + " and TO_CHAR (TO_DATE (SALES_DATE, 'DD-MM-YYYY')) >=TO_DATE('" + filters.FromDate + "', 'YYYY-MM-DD') and TO_CHAR (TO_DATE (SALES_DATE, 'DD-MM-YYYY'))  <= TO_DATE('" + filters.ToDate + "', 'YYYY-MM-DD')";
            




            query += " ORDER BY BS_DATE (SALES_DATE), INVOICE_NO ";


            var salesRegisters = _objectEntity.SqlQuery<VatRegistrationIRDMasterModel>(query).ToList();


            return salesRegisters;
        }
        public List<PurchaseReturnRegistersDetail> GetPurchaseReturnRegister(ReportFiltersModel filters, NeoErp.Core.Domain.User userinfo)
        {
            var companyCode = string.Empty;
            foreach (var company in filters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }
            companyCode = companyCode == "" ? $@"'{userinfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);
            string query = string.Format(@"SELECT RETURN_date as ReturnDate,
                        bs_date (RETURN_date) Miti ,
                       RETURN_no as InvoiceNumber,
                       INITCAP (IMS.ITEM_EDESC) ItemName,
                       INITCAP (ls.location_edesc) LocationName,
                       SI.MANUAL_NO as ManualNo,
                       SI.REMARKS as REMARKS,
                        INITCAP (PTC.SUPPLIER_EDESC) SUPPLIERNAME,
                       INITCAP (SI.MU_CODE) Unit ,
                       Round(NVL(SI.QUANTITY,0)/{0},{1}) as Quantity,
                       Round(NVL(FN_CONVERT_CURRENCY(NVL(SI.CALC_UNIT_PRICE,0),'NRS',SI.Return_DATE),0)/{2},{3}) as UnitPrice,
                       Round(NVL(FN_CONVERT_CURRENCY(NVL(SI.CALC_TOTAL_PRICE,0),'NRS',SI.Return_DATE),0)/{4},{5}) as TotalPrice
                        FROM IP_PURCHASE_RETURN si,
                       IP_ITEM_MASTER_SETUP ims,
                       IP_LOCATION_SETUP ls,
                       ip_supplier_setup ptc
                       WHERE SI.ITEM_CODE = IMS.ITEM_CODE
                        and si.company_code=IMS.company_code
                        --and si.AREA_CODE = ast.AREA_CODE
                       --and SI.COMPANY_CODE=cs.company_code
                       and SI.company_code=ls.company_code
                          --and  SI.SHIPPING_ADDRESS= ct.city_code
                       AND si.COMPANY_CODE IN(" + companyCode + @") "
                     , ReportFilterHelper.FigureFilterValue(filters.QuantityFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.QuantityRoundUpFilter), ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter)
                        , ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter));
            //if (filters.CustomerFilter.Count > 0)
            //{

            //    var customers = filters.CustomerFilter;
            //    var customerConditionQuery = string.Empty;
            //    for (int i = 0; i < customers.Count; i++)
            //    {

            //        if (i == 0)
            //            customerConditionQuery += string.Format("MASTER_CUSTOMER_CODE like (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%' from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G')", customers[i], companyCode);
            //        else
            //        {
            //            customerConditionQuery += string.Format(" OR  MASTER_CUSTOMER_CODE like (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G')", customers[i], companyCode);
            //        }
            //    }

            //    query = query + string.Format(@" AND SI.CUSTOMER_CODE IN (SELECT DISTINCT(CUSTOMER_CODE) FROM SA_CUSTOMER_SETUP WHERE  {0} OR (CUSTOMER_CODE IN ('{1}') AND GROUP_SKU_FLAG='I')) ", customerConditionQuery, string.Join("','", customers));
            //}
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

                query = query + string.Format(@" AND SI.ITEM_CODE IN (SELECT DISTINCT(ITEM_CODE) FROM IP_ITEM_MASTER_SETUP WHERE {0} OR (ITEM_CODE IN ('{1}') AND GROUP_SKU_FLAG='I')) ", productConditionQuery, string.Join("','", products));

                //query = query + string.Format(@" AND SI.ITEM_CODE IN ({0})", string.Join(",", filters.ProductFilter).ToString());
            }
            //if (filters.DocumentFilter.Count > 0)
            //{
            //    query = query + string.Format(@" AND  SI. FORM_CODE  IN  ('{0}')", string.Join("','", filters.DocumentFilter).ToString());
            //}
            //if (filters.PartyTypeFilter.Count > 0)
            //{
            //    query = query + string.Format(@" AND SI.PARTY_TYPE_CODE IN ('{0}') ", string.Join("','", filters.PartyTypeFilter).ToString());
            //}
            //if (filters.AreaTypeFilter.Count > 0)
            //{
            //    query = query + string.Format(@" AND SI.AREA_CODE IN ('{0}') ", string.Join("','", filters.AreaTypeFilter).ToString());
            //}
            if (filters.CategoryFilter.Count > 0)
            {
                query = query + string.Format(@" AND ims.CATEGORY_CODE IN ('{0}') ", string.Join("','", filters.CategoryFilter).ToString());
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
            //EMPLOYEE_CODE doesn't exist on the NNPL &WLINK database , So We have manually created column of EMPLOYEE_CODE for the both database .
            query = query + @" AND SI.FROM_LOCATION_CODE = LS.LOCATION_CODE
                        and si.Deleted_flag = 'N'
                     -- AND SI.SUPPLIER_CODE = ES.SUPPLIER_CODE(+)
                       AND SI.SUPPLIER_CODE = PTC.SUPPLIER_CODE(+)";
            if (!string.IsNullOrEmpty(filters.FromDate))
                query = query + " and Return_DATE>=TO_DATE('" + filters.FromDate + "', 'YYYY-MM-DD') and Return_DATE <= TO_DATE('" + filters.ToDate + "', 'YYYY-MM-DD')";


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


            var salesRegisters = _objectEntity.SqlQuery<PurchaseReturnRegistersDetail>(query).ToList();
            return salesRegisters;
        }
        public List<SalesRegistersDetail> GetAgentWiseSalesRegister(ReportFiltersModel filters, NeoErp.Core.Domain.User userinfo)
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
                       --CT.CITY_EDESC as ShippingAddress,
                       -- ast.AREA_EDESC,
                        ags.AGENT_EDESC,
                        iss.Brand_name BRAND_EDESC,
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
                        AGENT_SETUP ags,
                        IP_ITEM_SPEC_SETUP iss
                           --  CITY_CODE ct,
                       -- AREA_SETUP ast
                       WHERE SI.ITEM_CODE = IMS.ITEM_CODE
                        and si.company_code=IMS.company_code
                       -- and si.AREA_CODE = ast.AREA_CODE
                       and SI.COMPANY_CODE=cs.company_code
                       and SI.company_code=ls.company_code
                       and si.agent_code=ags.agent_code and si.company_code=ags.company_code
                        and si.item_code=iss.item_code and si.company_code=iss.company_code
                        --  and  SI.SHIPPING_ADDRESS= ct.city_code
                       AND si.COMPANY_CODE IN(" + companyCode + @") AND si.CUSTOMER_CODE = cs.CUSTOMER_CODE"
                     , ReportFilterHelper.FigureFilterValue(filters.QuantityFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.QuantityRoundUpFilter), ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter)
                        , ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter));
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
                        customerConditionQuery += string.Format(" OR  MASTER_CUSTOMER_CODE like (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G')", customers[i], companyCode);
                    }
                }

                query = query + string.Format(@" AND SI.CUSTOMER_CODE IN (SELECT DISTINCT(CUSTOMER_CODE) FROM SA_CUSTOMER_SETUP WHERE  {0} OR (CUSTOMER_CODE IN ('{1}') AND GROUP_SKU_FLAG='I')) ", customerConditionQuery, string.Join("','", customers));
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

                query = query + string.Format(@" AND SI.ITEM_CODE IN (SELECT DISTINCT(ITEM_CODE) FROM IP_ITEM_MASTER_SETUP WHERE {0} OR (ITEM_CODE IN ('{1}') AND GROUP_SKU_FLAG='I')) ", productConditionQuery, string.Join("','", products));

                //query = query + string.Format(@" AND SI.ITEM_CODE IN ({0})", string.Join(",", filters.ProductFilter).ToString());
            }
            if (filters.DocumentFilter.Count > 0)
            {
                query = query + string.Format(@" AND  SI. FORM_CODE  IN  ('{0}')", string.Join("','", filters.DocumentFilter).ToString());
            }
            if (filters.PartyTypeFilter.Count > 0)
            {
                query = query + string.Format(@" AND SI.PARTY_TYPE_CODE IN ('{0}') ", string.Join("','", filters.PartyTypeFilter).ToString());
            }
            //if (filters.AreaTypeFilter.Count > 0)
            //{
            //    query = query + string.Format(@" AND SI.AREA_CODE IN ('{0}') ", string.Join("','", filters.AreaTypeFilter).ToString());
            //}
            if (filters.CategoryFilter.Count > 0)
            {
                query = query + string.Format(@" AND ims.CATEGORY_CODE IN ('{0}') ", string.Join("','", filters.CategoryFilter).ToString());
            }
            //if (filters.EmployeeFilter.Count > 0)
            //{
            //    query = query + string.Format(@" AND  SI.EMPLOYEE_CODE IN  ('{0}')", string.Join("','", filters.EmployeeFilter).ToString());
            //}
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
            //EMPLOYEE_CODE doesn't exist on the NNPL &WLINK database , So We have manually created column of EMPLOYEE_CODE for the both database .
            query = query + @" AND SI.FROM_LOCATION_CODE = LS.LOCATION_CODE
                        and si.Deleted_flag = 'N'
                     -- AND SI.EMPLOYEE_CODE = ES.EMPLOYEE_CODE(+)
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


            var salesRegisters = _objectEntity.SqlQuery<SalesRegistersDetail>(query).ToList();
            foreach(var item in salesRegisters)
            {
                item.AGENT_EDESC = item.AGENT_EDESC == null ? "" : item.AGENT_EDESC;
                if(item.AGENT_EDESC.Split('-').Count()>1)
                {
                    item.AGENT_CODE = item.AGENT_EDESC.Split('-')[0];
                    item.AGENT_EDESC = item.AGENT_EDESC.Split('-')[1];
                }
                else
                {
                    item.AGENT_CODE = "-";
                }
            }
            return salesRegisters;
        }
        public List<PurchaseReturnRegistersDetail> GetPurchaseRegister(ReportFiltersModel filters, NeoErp.Core.Domain.User userinfo)
        {
            var companyCode = string.Empty;
            foreach (var company in filters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }
            companyCode = companyCode == "" ? $@"'{userinfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);
            string query = string.Format(@"SELECT INVOICE_DATE as ReturnDate,
                        bs_date (INVOICE_DATE) Miti ,
                       INVOICE_NO as InvoiceNumber,
                       INITCAP (IMS.ITEM_EDESC) ItemName,
                       INITCAP (ls.location_edesc) LocationName,
                       SI.MANUAL_NO as ManualNo,
                       SI.REMARKS as REMARKS,
                        INITCAP (PTC.SUPPLIER_EDESC) SUPPLIERNAME,
                       INITCAP (SI.MU_CODE) Unit ,
                       Round(NVL(SI.QUANTITY,0)/{0},{1}) as Quantity,
                       Round(NVL(FN_CONVERT_CURRENCY(NVL(SI.CALC_UNIT_PRICE,0),'NRS',SI.INVOICE_DATE),0)/{2},{3}) as UnitPrice,
                       Round(NVL(FN_CONVERT_CURRENCY(NVL(SI.CALC_TOTAL_PRICE,0),'NRS',SI.INVOICE_DATE),0)/{4},{5}) as TotalPrice
                        FROM IP_PURCHASE_INVOICE si,
                       IP_ITEM_MASTER_SETUP ims,
                       IP_LOCATION_SETUP ls,
                       ip_supplier_setup ptc
                       WHERE SI.ITEM_CODE = IMS.ITEM_CODE
                        and si.company_code=IMS.company_code
                        --and si.AREA_CODE = ast.AREA_CODE
                       --and SI.COMPANY_CODE=cs.company_code
                       and SI.company_code=ls.company_code
                          --and  SI.SHIPPING_ADDRESS= ct.city_code
                       AND si.COMPANY_CODE IN(" + companyCode + @") "
                     , ReportFilterHelper.FigureFilterValue(filters.QuantityFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.QuantityRoundUpFilter), ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter)
                        , ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter));
            //if (filters.CustomerFilter.Count > 0)
            //{

            //    var customers = filters.CustomerFilter;
            //    var customerConditionQuery = string.Empty;
            //    for (int i = 0; i < customers.Count; i++)
            //    {

            //        if (i == 0)
            //            customerConditionQuery += string.Format("MASTER_CUSTOMER_CODE like (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%' from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G')", customers[i], companyCode);
            //        else
            //        {
            //            customerConditionQuery += string.Format(" OR  MASTER_CUSTOMER_CODE like (Select DISTINCT(MASTER_CUSTOMER_CODE) || '%'  from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{0}' AND COMPANY_CODE IN({1}) AND GROUP_SKU_FLAG='G')", customers[i], companyCode);
            //        }
            //    }

            //    query = query + string.Format(@" AND SI.CUSTOMER_CODE IN (SELECT DISTINCT(CUSTOMER_CODE) FROM SA_CUSTOMER_SETUP WHERE  {0} OR (CUSTOMER_CODE IN ('{1}') AND GROUP_SKU_FLAG='I')) ", customerConditionQuery, string.Join("','", customers));
            //}
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

                query = query + string.Format(@" AND SI.ITEM_CODE IN (SELECT DISTINCT(ITEM_CODE) FROM IP_ITEM_MASTER_SETUP WHERE {0} OR (ITEM_CODE IN ('{1}') AND GROUP_SKU_FLAG='I')) ", productConditionQuery, string.Join("','", products));

                //query = query + string.Format(@" AND SI.ITEM_CODE IN ({0})", string.Join(",", filters.ProductFilter).ToString());
            }
            //if (filters.DocumentFilter.Count > 0)
            //{
            //    query = query + string.Format(@" AND  SI. FORM_CODE  IN  ('{0}')", string.Join("','", filters.DocumentFilter).ToString());
            //}
            //if (filters.PartyTypeFilter.Count > 0)
            //{
            //    query = query + string.Format(@" AND SI.PARTY_TYPE_CODE IN ('{0}') ", string.Join("','", filters.PartyTypeFilter).ToString());
            //}
            //if (filters.AreaTypeFilter.Count > 0)
            //{
            //    query = query + string.Format(@" AND SI.AREA_CODE IN ('{0}') ", string.Join("','", filters.AreaTypeFilter).ToString());
            //}
            if (filters.CategoryFilter.Count > 0)
            {
                query = query + string.Format(@" AND ims.CATEGORY_CODE IN ('{0}') ", string.Join("','", filters.CategoryFilter).ToString());
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
            //EMPLOYEE_CODE doesn't exist on the NNPL &WLINK database , So We have manually created column of EMPLOYEE_CODE for the both database .
            query = query + @" AND SI.TO_LOCATION_CODE = LS.LOCATION_CODE
                        and si.Deleted_flag = 'N'
                     -- AND SI.SUPPLIER_CODE = ES.SUPPLIER_CODE(+)
                       AND SI.SUPPLIER_CODE = PTC.SUPPLIER_CODE(+)";
            if (!string.IsNullOrEmpty(filters.FromDate))
                query = query + " and INVOICE_DATE>=TO_DATE('" + filters.FromDate + "', 'YYYY-MM-DD') and INVOICE_DATE <= TO_DATE('" + filters.ToDate + "', 'YYYY-MM-DD')";


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


            var salesRegisters = _objectEntity.SqlQuery<PurchaseReturnRegistersDetail>(query).ToList();
            return salesRegisters;
        }
        public List<PurchasePendingDetailModel> GetPurchasePendingReport(ReportFiltersModel filters, NeoErp.Core.Domain.User userinfo)
        {
            var companyCode = string.Empty;
            foreach (var company in filters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }
            companyCode = companyCode == "" ? $@"'{userinfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);
            string query = string.Format(@"SELECT TO_CHAR(ORDER_DATE) ORDER_DATE,
                        bs_date (ORDER_DATE) Miti ,
                       ORDER_NO ,
                        MANUAL_NO,
                       INITCAP (IMS.ITEM_EDESC) ITEM_EDESC,
                       --INITCAP (ls.location_edesc) LocationName,
                       SI.MANUAL_NO ,
                       SI.REMARKS ,
                        INITCAP (PTC.SUPPLIER_EDESC) SUPPLIER_EDESC,
                       INITCAP (SI.MU_CODE) MU_CODE ,
                       Round(NVL(SI.QUANTITY,0)/{0},{1}) as QUANTITY,
                       Round(NVL(FN_CONVERT_CURRENCY(NVL(SI.CALC_UNIT_PRICE,0),'NRS',SI.ORDER_DATE),0)/{2},{3}) as UNIT_PRICE,
                       Round(NVL(FN_CONVERT_CURRENCY(NVL(SI.CALC_TOTAL_PRICE,0),'NRS',SI.ORDER_DATE),0)/{4},{5}) as TOTAL_PRICE
                        FROM IP_PURCHASE_ORDER si,
                       IP_ITEM_MASTER_SETUP ims,
                      -- IP_LOCATION_SETUP ls,
                       ip_supplier_setup ptc
                       WHERE SI.ITEM_CODE = IMS.ITEM_CODE
                        and si.company_code=IMS.company_code
                        and si.supplier_code=ptc.supplier_code 
                        and si.company_code=ptc.company_code
                        and si.ORDER_NO NOT IN (SELECT REFERENCE_NO FROM REFERENCE_DETAIL)
                       AND si.COMPANY_CODE IN(" + companyCode + @") "
                     , ReportFilterHelper.FigureFilterValue(filters.QuantityFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.QuantityRoundUpFilter), ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter)
                        , ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter));
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

                query = query + string.Format(@" AND SI.ITEM_CODE IN (SELECT DISTINCT(ITEM_CODE) FROM IP_ITEM_MASTER_SETUP WHERE {0} OR (ITEM_CODE IN ('{1}') AND GROUP_SKU_FLAG='I')) ", productConditionQuery, string.Join("','", products));

                
            }
           
            //if (filters.CategoryFilter.Count > 0)
            //{
            //    query = query + string.Format(@" AND ims.CATEGORY_CODE IN ('{0}') ", string.Join("','", filters.CategoryFilter).ToString());
            //}
            //if (filters.EmployeeFilter.Count > 0)
            //{
            //    query = query + string.Format(@" AND  SI.EMPLOYEE_CODE IN  ('{0}')", string.Join("','", filters.EmployeeFilter).ToString());
            //}
            //if (filters.AgentFilter.Count > 0)
            //{
            //    query = query + string.Format(@" AND  SI.AGENT_CODE IN  ('{0}')", string.Join("','", filters.AgentFilter).ToString());
            //}
            //if (filters.DivisionFilter.Count > 0)
            //{
            //    query = query + string.Format(@" AND  SI.DIVISION_CODE IN  ('{0}')", string.Join("','", filters.DivisionFilter).ToString());
            //}
            //if (filters.LocationFilter.Count > 0)
            //{

            //    var locations = filters.LocationFilter;
            //    var locationConditionQuery = string.Empty;
            //    for (int i = 0; i < locations.Count; i++)
            //    {

            //        if (i == 0)
            //            locationConditionQuery += string.Format("SELECT LOCATION_CODE FROM IP_LOCATION_SETUP WHERE LOCATION_CODE LIKE '{0}%'", locations[i]);
            //        else
            //        {
            //            locationConditionQuery += string.Format(" OR  LOCATION_CODE like '{0}%' ", locations[i]);
            //        }
            //    }
            //    query = query + string.Format(@" AND SI.FROM_LOCATION_CODE IN ({0} OR LOCATION_CODE IN ('{1}'))", locationConditionQuery, string.Join("','", locations));
            //    //query = query + string.Format(@" AND (SI.FROM_LOCATION_CODE = LS.LOCATION_CODE OR SI.FROM_LOCATION_CODE IN ('{0}'))", string.Join("','", filters.LocationFilter).ToString());
            //}
            if (filters.CompanyFilter.Count > 0)
            {
                query = query + string.Format(@" AND si.COMPANY_CODE = cmps.COMPANY_CODE AND SI.COMPANY_CODE IN ('{0}')", string.Join("','", filters.CompanyFilter).ToString());
            }
            if (filters.BranchFilter.Count > 0)
            {
                query = query + string.Format(@" AND SI.BRANCH_CODE IN ('{0}')", string.Join("','", filters.BranchFilter).ToString());
            }
            //EMPLOYEE_CODE doesn't exist on the NNPL &WLINK database , So We have manually created column of EMPLOYEE_CODE for the both database .
            //query = query + @" AND SI.TO_LOCATION_CODE = LS.LOCATION_CODE
            //            and si.Deleted_flag = 'N'
            //         -- AND SI.SUPPLIER_CODE = ES.SUPPLIER_CODE(+)
            //           AND SI.SUPPLIER_CODE = PTC.SUPPLIER_CODE(+)";
            if (!string.IsNullOrEmpty(filters.FromDate))
                query = query + " and ORDER_DATE>=TO_DATE('" + filters.FromDate + "', 'YYYY-MM-DD') and ORDER_DATE <= TO_DATE('" + filters.ToDate + "', 'YYYY-MM-DD')";


            int min = 0, max = 0;

            ReportFilterHelper.RangeFilterValue(filters.AmountRangeFilter, out min, out max);
            if (!(min == 0 && max == 0))
                query = query + string.Format(@" and NVL(FN_CONVERT_CURRENCY(NVL(SI.TOTAL_PRICE,0),'NRS',SI.ORDER_DATE),0) >= {0} and NVL(FN_CONVERT_CURRENCY(NVL(SI.TOTAL_PRICE,0),'NRS',SI.ORDER_DATE),0)  <= {1}", min, max);

            ReportFilterHelper.RangeFilterValue(filters.QuantityRangeFilter, out min, out max);
            if (!(min == 0 && max == 0))
                query = query + string.Format(@" and NVL(SI.QUANTITY,0) >= {0} and NVL(SI.QUANTITY,0) <= {1}", min, max);

            ReportFilterHelper.RangeFilterValue(filters.RateRangeFilter, out min, out max);
            if (!(min == 0 && max == 0))
                query = query + string.Format(@" and NVL(FN_CONVERT_CURRENCY(NVL(SI.UNIT_PRICE,0),'NRS',SI.ORDER_DATE),0) >= {0} and NVL(FN_CONVERT_CURRENCY(NVL(SI.UNIT_PRICE,0),'NRS',SI.ORDER_DATE),0) <= {1}", min, max);


            var salesRegisters = _objectEntity.SqlQuery<PurchasePendingDetailModel>(query).ToList();
            return salesRegisters;
        }
        public List<PurchasePendingDetailModel> GetPurchaseOrderReport(ReportFiltersModel filters, NeoErp.Core.Domain.User userinfo)
        {
            var companyCode = string.Empty;
            foreach (var company in filters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }
            companyCode = companyCode == "" ? $@"'{userinfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);
            string query = string.Format(@"SELECT TO_CHAR(ORDER_DATE) ORDER_DATE,
                        bs_date (ORDER_DATE) Miti ,
                       ORDER_NO ,
                        MANUAL_NO,
                       INITCAP (IMS.ITEM_EDESC) ITEM_EDESC,
                       --INITCAP (ls.location_edesc) LocationName,
                       SI.MANUAL_NO ,
                       SI.REMARKS ,
                        INITCAP (PTC.SUPPLIER_EDESC) SUPPLIER_EDESC,
                       INITCAP (SI.MU_CODE) MU_CODE ,
                       Round(NVL(SI.QUANTITY,0)/{0},{1}) as QUANTITY,
                       Round(NVL(FN_CONVERT_CURRENCY(NVL(SI.CALC_UNIT_PRICE,0),'NRS',SI.ORDER_DATE),0)/{2},{3}) as UNIT_PRICE,
                       Round(NVL(FN_CONVERT_CURRENCY(NVL(SI.CALC_TOTAL_PRICE,0),'NRS',SI.ORDER_DATE),0)/{4},{5}) as TOTAL_PRICE
                        FROM IP_PURCHASE_ORDER si,
                       IP_ITEM_MASTER_SETUP ims,
                      -- IP_LOCATION_SETUP ls,
                       ip_supplier_setup ptc
                       WHERE SI.ITEM_CODE = IMS.ITEM_CODE
                        and si.company_code=IMS.company_code
                        and si.supplier_code=ptc.supplier_code 
                        and si.company_code=ptc.company_code
                        and si.ORDER_NO  IN (SELECT REFERENCE_NO FROM REFERENCE_DETAIL)
                       AND si.COMPANY_CODE IN(" + companyCode + @") "
                     , ReportFilterHelper.FigureFilterValue(filters.QuantityFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.QuantityRoundUpFilter), ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter)
                        , ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter),
                        ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter));
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

                query = query + string.Format(@" AND SI.ITEM_CODE IN (SELECT DISTINCT(ITEM_CODE) FROM IP_ITEM_MASTER_SETUP WHERE {0} OR (ITEM_CODE IN ('{1}') AND GROUP_SKU_FLAG='I')) ", productConditionQuery, string.Join("','", products));


            }

            //if (filters.CategoryFilter.Count > 0)
            //{
            //    query = query + string.Format(@" AND ims.CATEGORY_CODE IN ('{0}') ", string.Join("','", filters.CategoryFilter).ToString());
            //}
            //if (filters.EmployeeFilter.Count > 0)
            //{
            //    query = query + string.Format(@" AND  SI.EMPLOYEE_CODE IN  ('{0}')", string.Join("','", filters.EmployeeFilter).ToString());
            //}
            //if (filters.AgentFilter.Count > 0)
            //{
            //    query = query + string.Format(@" AND  SI.AGENT_CODE IN  ('{0}')", string.Join("','", filters.AgentFilter).ToString());
            //}
            //if (filters.DivisionFilter.Count > 0)
            //{
            //    query = query + string.Format(@" AND  SI.DIVISION_CODE IN  ('{0}')", string.Join("','", filters.DivisionFilter).ToString());
            //}
            //if (filters.LocationFilter.Count > 0)
            //{

            //    var locations = filters.LocationFilter;
            //    var locationConditionQuery = string.Empty;
            //    for (int i = 0; i < locations.Count; i++)
            //    {

            //        if (i == 0)
            //            locationConditionQuery += string.Format("SELECT LOCATION_CODE FROM IP_LOCATION_SETUP WHERE LOCATION_CODE LIKE '{0}%'", locations[i]);
            //        else
            //        {
            //            locationConditionQuery += string.Format(" OR  LOCATION_CODE like '{0}%' ", locations[i]);
            //        }
            //    }
            //    query = query + string.Format(@" AND SI.FROM_LOCATION_CODE IN ({0} OR LOCATION_CODE IN ('{1}'))", locationConditionQuery, string.Join("','", locations));
            //    //query = query + string.Format(@" AND (SI.FROM_LOCATION_CODE = LS.LOCATION_CODE OR SI.FROM_LOCATION_CODE IN ('{0}'))", string.Join("','", filters.LocationFilter).ToString());
            //}
            if (filters.CompanyFilter.Count > 0)
            {
                query = query + string.Format(@" AND si.COMPANY_CODE = cmps.COMPANY_CODE AND SI.COMPANY_CODE IN ('{0}')", string.Join("','", filters.CompanyFilter).ToString());
            }
            if (filters.BranchFilter.Count > 0)
            {
                query = query + string.Format(@" AND SI.BRANCH_CODE IN ('{0}')", string.Join("','", filters.BranchFilter).ToString());
            }
            //EMPLOYEE_CODE doesn't exist on the NNPL &WLINK database , So We have manually created column of EMPLOYEE_CODE for the both database .
            //query = query + @" AND SI.TO_LOCATION_CODE = LS.LOCATION_CODE
            //            and si.Deleted_flag = 'N'
            //         -- AND SI.SUPPLIER_CODE = ES.SUPPLIER_CODE(+)
            //           AND SI.SUPPLIER_CODE = PTC.SUPPLIER_CODE(+)";
            if (!string.IsNullOrEmpty(filters.FromDate))
                query = query + " and ORDER_DATE>=TO_DATE('" + filters.FromDate + "', 'YYYY-MM-DD') and ORDER_DATE <= TO_DATE('" + filters.ToDate + "', 'YYYY-MM-DD')";


            int min = 0, max = 0;

            ReportFilterHelper.RangeFilterValue(filters.AmountRangeFilter, out min, out max);
            if (!(min == 0 && max == 0))
                query = query + string.Format(@" and NVL(FN_CONVERT_CURRENCY(NVL(SI.TOTAL_PRICE,0),'NRS',SI.ORDER_DATE),0) >= {0} and NVL(FN_CONVERT_CURRENCY(NVL(SI.TOTAL_PRICE,0),'NRS',SI.ORDER_DATE),0)  <= {1}", min, max);

            ReportFilterHelper.RangeFilterValue(filters.QuantityRangeFilter, out min, out max);
            if (!(min == 0 && max == 0))
                query = query + string.Format(@" and NVL(SI.QUANTITY,0) >= {0} and NVL(SI.QUANTITY,0) <= {1}", min, max);

            ReportFilterHelper.RangeFilterValue(filters.RateRangeFilter, out min, out max);
            if (!(min == 0 && max == 0))
                query = query + string.Format(@" and NVL(FN_CONVERT_CURRENCY(NVL(SI.UNIT_PRICE,0),'NRS',SI.ORDER_DATE),0) >= {0} and NVL(FN_CONVERT_CURRENCY(NVL(SI.UNIT_PRICE,0),'NRS',SI.ORDER_DATE),0) <= {1}", min, max);


            var salesRegisters = _objectEntity.SqlQuery<PurchasePendingDetailModel>(query).ToList();
            return salesRegisters;
        }
        public List<PurchaseVatRegistrationDetailModel> GetPurchaseVatRegisterReport(ReportFiltersModel filters, NeoErp.Core.Domain.User userinfo)
        {
            var companyCode = string.Empty;
            foreach (var company in filters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }
            companyCode = companyCode == "" ? $@"'{userinfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);
            string query = $@"SELECT BS_DATE (INVOICE_DATE) MITI,
                                 INVOICE_NO,
                                 PARTY_NAME,
                                 VAT_NO,
                                 FN_CONVERT_CURRENCY (NVL (GROSS_PURCHASE, 0) * NVL (EXCHANGE_RATE, 1),
                                                      'NRS',
                                                      INVOICE_DATE)
                                    GROSS_AMOUNT,
                                 FN_CONVERT_CURRENCY (
                                    NVL (TAXABLE_PURCHASE, 0) * NVL (EXCHANGE_RATE, 1),
                                    'NRS',
                                    INVOICE_DATE)
                                    TAXABLE_AMOUNT,
                                 FN_CONVERT_CURRENCY (NVL (VAT, 0) * NVL (EXCHANGE_RATE, 1),
                                                      'NRS',
                                                      INVOICE_DATE)
                                    VAT_AMOUNT,
                                 --  FN_CONVERT_CURRENCY (NVL (TOTAL_PURCHASE, 0) * NVL (EXCHANGE_RATE, 1),
                                 --    'NRS',
                                 --     INVOICE_DATE)TOTAL_AMOUNT,
                                 NVL (
                                    FN_CONVERT_CURRENCY (NVL (VAT, 0) * NVL (EXCHANGE_RATE, 1),
                                                         'NRS',
                                                         INVOICE_DATE),
                                    0)
                                 + NVL (
                                      FN_CONVERT_CURRENCY (
                                         NVL (TAXABLE_PURCHASE, 0) * NVL (EXCHANGE_RATE, 1),
                                         'NRS',
                                         INVOICE_DATE),
                                      0)
                                    TOTAL_AMOUNT,
                                 FORM_CODE,
                                 P_TYPE,
                                 MANUAL_NO,
                                 BS_DATE (VOUCHER_DATE) VOUCHER_DATE,
                                 TO_CHAR(INVOICE_DATE)INVOICE_DATE,
                                 TABLE_NAME,
                                 SUPPLIER_CODE,
                                 CASE
                                    WHEN TABLE_NAME = 'IP_PURCHASE_INVOICE'
                                    THEN
                                       (SELECT LISTAGG (ACC_EDESC, ',')
                                                  WITHIN GROUP (ORDER BY ACC_EDESC)
                                          FROM FA_DOUBLE_VOUCHER X, FA_CHART_OF_ACCOUNTS_SETUP Y
                                         WHERE     X.MANUAL_NO = A.MANUAL_NO
                                               AND X.COMPANY_CODE = A.COMPANY_CODE
                                               AND X.TRANSACTION_TYPE = 'DR'
                                               AND X.COMPANY_CODE = Y.COMPANY_CODE
                                               AND X.ACC_CODE = Y.ACC_CODE
                                               AND Y.ACC_NATURE = 'SB')
                                    ELSE
                                       (SELECT LISTAGG (ACC_EDESC, ',')
                                                  WITHIN GROUP (ORDER BY ACC_EDESC)
                                          FROM FA_DOUBLE_VOUCHER X, FA_CHART_OF_ACCOUNTS_SETUP Y
                                         WHERE     X.MANUAL_NO = A.INVOICE_NO
                                               AND X.COMPANY_CODE = A.COMPANY_CODE
                                               AND X.TRANSACTION_TYPE = 'CR'
                                               AND X.COMPANY_CODE = Y.COMPANY_CODE
                                               AND X.ACC_CODE = Y.ACC_CODE
                                               AND Y.ACC_NATURE = 'SB')
                                 END
                                    ACC_INT_VOUCHER
                            FROM V$PURCHASE_INVOICE_REPORT3 A
                           WHERE 1=1 ";
           

            
            if (filters.CompanyFilter.Count > 0)
            {
                query = query + string.Format(@"  AND A.COMPANY_CODE IN ('{0}')", string.Join("','", filters.CompanyFilter).ToString());
            }
            if (filters.BranchFilter.Count > 0)
            {
                query = query + string.Format(@" AND A.BRANCH_CODE IN ('{0}')", string.Join("','", filters.BranchFilter).ToString());
            }
            //EMPLOYEE_CODE doesn't exist on the NNPL &WLINK database , So We have manually created column of EMPLOYEE_CODE for the both database .
            //query = query + @" AND SI.TO_LOCATION_CODE = LS.LOCATION_CODE
            //            and si.Deleted_flag = 'N'
            //         -- AND SI.SUPPLIER_CODE = ES.SUPPLIER_CODE(+)
            //           AND SI.SUPPLIER_CODE = PTC.SUPPLIER_CODE(+)";
            if (!string.IsNullOrEmpty(filters.FromDate))
                query = query + " and A.INVOICE_DATE>=TO_DATE('" + filters.FromDate + "', 'YYYY-MM-DD') and A.INVOICE_DATE <= TO_DATE('" + filters.ToDate + "', 'YYYY-MM-DD')";

            query += " ORDER BY BS_DATE (INVOICE_DATE), MANUAL_NO, INVOICE_NO";


            //int min = 0, max = 0;

            //ReportFilterHelper.RangeFilterValue(filters.AmountRangeFilter, out min, out max);
            //if (!(min == 0 && max == 0))
            //    query = query + string.Format(@" and NVL(FN_CONVERT_CURRENCY(NVL(SI.TOTAL_PRICE,0),'NRS',SI.ORDER_DATE),0) >= {0} and NVL(FN_CONVERT_CURRENCY(NVL(SI.TOTAL_PRICE,0),'NRS',SI.ORDER_DATE),0)  <= {1}", min, max);

            //ReportFilterHelper.RangeFilterValue(filters.QuantityRangeFilter, out min, out max);
            //if (!(min == 0 && max == 0))
            //    query = query + string.Format(@" and NVL(SI.QUANTITY,0) >= {0} and NVL(SI.QUANTITY,0) <= {1}", min, max);

            //ReportFilterHelper.RangeFilterValue(filters.RateRangeFilter, out min, out max);
            //if (!(min == 0 && max == 0))
            //    query = query + string.Format(@" and NVL(FN_CONVERT_CURRENCY(NVL(SI.UNIT_PRICE,0),'NRS',SI.ORDER_DATE),0) >= {0} and NVL(FN_CONVERT_CURRENCY(NVL(SI.UNIT_PRICE,0),'NRS',SI.ORDER_DATE),0) <= {1}", min, max);


            var salesRegisters = _objectEntity.SqlQuery<PurchaseVatRegistrationDetailModel>(query).ToList();
            return salesRegisters;
        }
        #region NewReports
        public List<SalesExciseRegisterModel> SalesExciseRegister(ReportFiltersModel filters, NeoErp.Core.Domain.User userinfo)
        {
            var figureFilter = ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter).ToString();
            var roundUpFilter = ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter).ToString();
            //var companyCode = string.Join(",", filters.CompanyFilter);
            //companyCode = companyCode == "" ? userinfo.company_code : companyCode;
            var companyCode = string.Empty;
            foreach (var company in filters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }

            companyCode = companyCode == "" ? $@"'{userinfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);

            string query = string.Format(@"SELECT BS_DATE(SALES_DATE) MITI
	                                                ,INVOICE_NO AS InvoiceNo
	                                                ,PARTY_NAME AS PartyName
	                                                ,VAT_NO AS PANNo
	                                                ,FORM_CODE
	                                                ,BRANCH_CODE
	                                                ,TO_CHAR(CREDIT_DAYS) CREDIT_DAYS
	                                                ,DELETED_FLAG
	                                                ,Round(CASE 
			                                            WHEN TABLE_NAME = 'SALES_RETURN'
				                                            THEN - NVL(SALES_DISCOUNT, 0)
			                                            ELSE NVL(SALES_DISCOUNT, 0)
			                                            END / 1, 2) AS Discount
	                                                ,NVL(MANUAL_NO,'n/a') MANUAL_NO
	                                                ,DELETED_FLAG
                                                    ,QUANTITY 
	                                                ,FN_CONVERT_CURRENCY(Round((NVL(TOTAL_SALES, 0) * NVL(EXCHANGE_RATE, 1)) / 1, 2), 'NRS', SALES_DATE) AS NetSales
	                                                ,FN_CONVERT_CURRENCY(NVL(EXCISE_AMOUNT, 0) * NVL(EXCHANGE_RATE, 1), 'NRS', SALES_DATE) TaxExempSales
                                                FROM V$SALES_INVOICE_REPORT3 WHERE SALES_DATE >= TO_DATE('{10}', 'YYYY-MM-DD') AND SALES_DATE <= TO_DATE('{11}', 'YYYY-MM-DD') 
             and DELETED_FLAG='N'AND EXCISE_AMOUNT <> 0 AND TABLE_NAME IN ('SALES','SALES_RETURN') AND COMPANY_CODE IN(" + companyCode + ")", figureFilter, roundUpFilter, figureFilter, roundUpFilter, figureFilter, roundUpFilter, figureFilter, roundUpFilter, figureFilter, roundUpFilter, filters.FromDate, filters.ToDate);
            

            if (filters.PartyTypeFilter.Count > 0)
            {
                query = query + string.Format(@" AND PARTY_TYPE_CODE IN ('{0}') ", string.Join("','", filters.PartyTypeFilter).ToString());
            }

            if (filters.BranchFilter.Count > 0)
            {
                query += string.Format(@" AND BRANCH_CODE IN ('{0}')", string.Join("','", filters.BranchFilter).ToString());
            }
            var min = 0;
            var max = 0;
            ReportFilterHelper.RangeFilterValue(filters.AmountRangeFilter, out min, out max);

            if (!(min == 0 && max == 0))
            {
                query = query + string.Format(@" AND FN_CONVERT_CURRENCY((NVL(TOTAL_SALES,0) * NVL(EXCHANGE_RATE,1)),'NRS',SALES_DATE) >={0} AND FN_CONVERT_CURRENCY((NVL(TOTAL_SALES,0) * NVL(EXCHANGE_RATE,1)),'NRS',SALES_DATE)<= {1}", min, max);
            }

            query += "ORDER BY BS_DATE(SALES_DATE), INVOICE_NO";

            var salesExciseRegisterList = _objectEntity.SqlQuery<SalesExciseRegisterModel>(query).ToList();
            return salesExciseRegisterList;
        }

        public List<AuditTrailModel> AuditTrailReport(ReportFiltersModel filters, NeoErp.Core.Domain.User userinfo)
        {
            var figureFilter = ReportFilterHelper.FigureFilterValue(filters.AmountFigureFilter).ToString();
            var roundUpFilter = ReportFilterHelper.RoundUpFilterValue(filters.AmountRoundUpFilter).ToString();
            var companyCode = string.Empty;
            foreach (var company in filters.CompanyFilter)
            {
                companyCode += $@"'{company}',";
            }

            companyCode = companyCode == "" ? $@"'{userinfo.company_code}'" : companyCode.Remove(companyCode.Length - 1);

            string query = string.Format(@"SELECT LOG_ID
	                                                ,LOG_USER
	                                                ,BS_DATE(TRUNC(LOG_DATE)) LOG_DATE
	                                                ,LOG_MESSAGE
                                                FROM LOG_DOC_TEMPLATE WHERE LOG_DATE >= TO_DATE('{0}', 'YYYY-MM-DD') AND LOG_DATE <= TO_DATE('{1}', 'YYYY-MM-DD') 
                  ", filters.FromDate, filters.ToDate);


            //if (filters.PartyTypeFilter.Count > 0)
            //{
            //    query = query + string.Format(@" AND PARTY_TYPE_CODE IN ('{0}') ", string.Join("','", filters.PartyTypeFilter).ToString());
            //}

            //if (filters.BranchFilter.Count > 0)
            //{
            //    query += string.Format(@" AND BRANCH_CODE IN ('{0}')", string.Join("','", filters.BranchFilter).ToString());
            //}
            //var min = 0;
            //var max = 0;
            //ReportFilterHelper.RangeFilterValue(filters.AmountRangeFilter, out min, out max);

            //if (!(min == 0 && max == 0))
            //{
            //    query = query + string.Format(@" AND FN_CONVERT_CURRENCY((NVL(TOTAL_SALES,0) * NVL(EXCHANGE_RATE,1)),'NRS',SALES_DATE) >={0} AND FN_CONVERT_CURRENCY((NVL(TOTAL_SALES,0) * NVL(EXCHANGE_RATE,1)),'NRS',SALES_DATE)<= {1}", min, max);
            //}

            query += "ORDER BY LOG_DATE DESC";

            var auditTrailList = _objectEntity.SqlQuery<AuditTrailModel>(query).ToList();
            return auditTrailList;
        }
        #endregion


    }
}
