using NeoErp.Core;
using NeoErp.Core.Caching;
using NeoErp.Core.Domain;
using NeoErp.Core.Models;
using NeoErp.Core.Models.CustomModels;
using NeoErp.Core.Models.Log4NetLoggin;
using NeoErp.Core.Services.CommonSetting;
using NeoErp.Data;
using NeoERP.DocumentTemplate.Service.Interface;
using NeoERP.DocumentTemplate.Service.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using OracleCommand = System.Data.OracleClient.OracleCommand;
using OracleConnection = System.Data.OracleClient.OracleConnection;

namespace NeoERP.DocumentTemplate.Service.Repository
{
    public class DocumentSetupRepo : IDocumentStup
    {
        private NeoErpCoreEntity _coreEntity;
        private IDbContext _dbContext;
        private IWorkContext _workContext;
        private ICacheManager _cacheManager;
        private ISettingService _settingService;
        private ILogErp _logErp;
        private DefaultValueForLog _defaultValueForLog;
        public DocumentSetupRepo(NeoErpCoreEntity coreEntity, IDbContext dbContext,
            IWorkContext workContext, ICacheManager cacheManager, ISettingService settingService)
        {
            _coreEntity = coreEntity;
            this._dbContext = dbContext;
            this._workContext = workContext;
            this._cacheManager = cacheManager;
            this._defaultValueForLog = new DefaultValueForLog(_workContext);
            _logErp = new LogErp(this, _defaultValueForLog.LogUser, _defaultValueForLog.LogCompany, _defaultValueForLog.LogBranch, _defaultValueForLog.LogTypeCode, _defaultValueForLog.LogModule, _defaultValueForLog.FormCode);
            this._settingService = settingService;
        }
        #region Account
        public string DeleteAccountSetupByAccCode(string accCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(accCode)) { accCode = string.Empty; }
                var masterQry = $@"SELECT MASTER_ACC_CODE, ACC_TYPE_FLAG FROM FA_CHART_OF_ACCOUNTS_SETUP WHERE ACC_CODE = '{accCode}' and COMPANY_CODE= '{companyCode}'";
                var masterAccCode = _dbContext.SqlQuery<AccountSetupModel>(masterQry).FirstOrDefault();
                if (masterAccCode.ACC_TYPE_FLAG == "N")
                {
                    var childQry = $@"SELECT COUNT(*) FROM FA_CHART_OF_ACCOUNTS_SETUP WHERE PRE_ACC_CODE like ('{masterAccCode.MASTER_ACC_CODE}%') and DELETED_FLAG='N'";
                    var childResult = _dbContext.SqlQuery<int>(childQry).FirstOrDefault();
                    if (childResult > 0)
                    {
                        return "HAS_CHILD";
                    }
                }
                var sqlquery = $@"UPDATE FA_CHART_OF_ACCOUNTS_SETUP SET DELETED_FLAG = 'Y' WHERE ACC_CODE='{accCode}' AND COMPANY_CODE ='{companyCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                return "DELETED";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public AccountSetupModel GetAccountDataByAccCode(string acccode)
        {
            try
            {
                if (string.IsNullOrEmpty(acccode)) { acccode = string.Empty; }
                string Query = $@"SELECT ACC_CODE,ACC_EDESC,ACC_NDESC,ACC_NATURE,TRANSACTION_TYPE,TPB_FLAG,ACC_TYPE_FLAG,MASTER_ACC_CODE,PRE_ACC_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SYN_ROWID,FREEZE_FLAG,CURRENT_BALANCE,LIMIT,MODIFY_DATE,ACC_NATURE,BRANCH_CODE,SHARE_VALUE,MODIFY_BY,IND_VAT_FLAG,BANK_ACCOUNT_NO,PRINTING_FLAG,ACC_SNAME,TEL_NO,IND_TDS_FLAG,ACC_ID,MOBILE_NO,EMAIL_ID,LINK_ID,CONTACT_PERSON,GROUP_START_CODE,GROUP_END_CODE,PREFIX_TEXT,CONTACT_PERSON,TEL_NO,MOBILE_NO,EMAIL_ID,LINK_ID,IND_MDF_FLAG FROM FA_CHART_OF_ACCOUNTS_SETUP WHERE ACC_CODE='{acccode}' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'   ORDER BY TO_NUMBER(ACC_CODE) ASC";
                AccountSetupModel entity = this._dbContext.SqlQuery<AccountSetupModel>(Query).FirstOrDefault();

                string ParentAccCodeQuery = $@"SELECT ACC_CODE FROM FA_CHART_OF_ACCOUNTS_SETUP where MASTER_ACC_CODE = (SELECT PRE_ACC_CODE FROM FA_CHART_OF_ACCOUNTS_SETUP WHERE ACC_CODE='{acccode}' 
                                AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}')";
                entity.PARENT_ACC_CODE = this._dbContext.SqlQuery<string>(ParentAccCodeQuery).FirstOrDefault();

                return entity;

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public BudgetCenterSetupModel GetBudgetCenterDetailByBudgetCode(string budgetcode)
        {
            try
            {
                if (string.IsNullOrEmpty(budgetcode)) { budgetcode = string.Empty; }
                string Query = $@"SELECT BUDGET_CODE,BUDGET_EDESC,BUDGET_NDESC,ASSIGN_TO,GROUP_SKU_FLAG,PRE_BUDGET_CODE,REMARKS,FREEZE_FLAG,ACC_CODE from bc_budget_center_setup WHERE BUDGET_CODE='{budgetcode}' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'   ORDER BY TO_NUMBER(BUDGET_CODE) ASC";
                BudgetCenterSetupModel entity = this._dbContext.SqlQuery<BudgetCenterSetupModel>(Query).FirstOrDefault();


                string parentbudgetQuery = $@"SELECT BUDGET_CODE FROM bc_budget_center_setup where BUDGET_CODE = (SELECT PRE_BUDGET_CODE FROM bc_budget_center_setup WHERE BUDGET_CODE='{budgetcode}' 
                                AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}') and company_code='{_workContext.CurrentUserinformation.company_code}'";
                entity.PARENT_BUDGET_CODE = this._dbContext.SqlQuery<string>(parentbudgetQuery).FirstOrDefault();

                return entity;

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }






        public string createNewAccountSetup(AccountSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var currentbalance = (model.CURRENT_BALANCE != null) ? model.CURRENT_BALANCE : 0;
                    var newmaxacccode = string.Empty;

                    var newmaxacccodequery = $@"SELECT MAX(TO_NUMBER(ACC_CODE))+1 as MAX_ACC_CODE FROM fa_chart_of_accounts_setup";
                    newmaxacccode = this._coreEntity.SqlQuery<int>(newmaxacccodequery).FirstOrDefault().ToString();
                    if (model.MASTER_ACC_CODE != null && model.MASTER_ACC_CODE != "")
                    {
                        var maxPreCode = string.Empty;
                        var newprecode = string.Empty;
                        var newmastercode = string.Empty;
                        if (model.ACC_TYPE_FLAG == "N")
                        {

                            if (model.MASTER_ACC_CODE != null && model.MASTER_ACC_CODE != "")
                            {
                                var maxprecodequery = $@"select (count(*) + 1) as MAXCODE from fa_chart_of_accounts_setup where pre_acc_code like '{model.MASTER_ACC_CODE}' and company_code = '{_workContext.CurrentUserinformation.company_code}'";
                                maxPreCode = this._dbContext.SqlQuery<int>(maxprecodequery).FirstOrDefault().ToString();
                                if (maxPreCode != null)
                                {
                                    if (Convert.ToInt32(maxPreCode) <= 9)
                                    {
                                        maxPreCode = "0" + maxPreCode.ToString();
                                    }

                                }
                                newprecode = model.MASTER_ACC_CODE;
                                newmastercode = model.MASTER_ACC_CODE + "." + maxPreCode;

                                var rootchildsqlquery = $@"INSERT INTO FA_CHART_OF_ACCOUNTS_SETUP (ACC_CODE,ACC_EDESC,ACC_NDESC,ACC_SNAME,TRANSACTION_TYPE,TPB_FLAG,ACC_TYPE_FLAG,MASTER_ACC_CODE,PRE_ACC_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,DELTA_FLAG,FREEZE_FLAG,ACC_NATURE,LIMIT,SHARE_VALUE,IND_VAT_FLAG,PRINTING_FLAG,IND_TDS_FLAG,CURRENT_BALANCE,BANK_ACCOUNT_NO,GROUP_START_CODE,GROUP_END_CODE,PREFIX_TEXT,ACC_ID,CONTACT_PERSON,TEL_NO,MOBILE_NO,EMAIL_ID,LINK_ID,IND_MDF_FLAG)
                                                 VALUES('{newmaxacccode}','{model.ACC_EDESC}','{model.ACC_NDESC}','{model.ACC_SNAME}','{model.TRANSACTION_TYPE}','{model.TPB_FLAG}','{model.ACC_TYPE_FLAG}','{newmastercode}','{newprecode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','N',
                                               '{model.FREEZE_FLAG}','{model.ACC_NATURE}','{model.LIMIT}','{model.SHARE_VALUE}','{model.IND_VAT_FLAG}','{model.PRINTING_FLAG}','{model.IND_TDS_FLAG}','{model.CURRENT_BALANCE}','{model.BANK_ACCOUNT_NO}','{model.GROUP_START_CODE}','{model.GROUP_END_CODE}','{model.PREFIX_TEXT}','{model.ACC_ID}','{model.CONTACT_PERSON}','{model.TEL_NO}','{model.MOBILE_NO}','{model.EMAIL_ID}','{model.LINK_ID}','{model.IND_MDF_FLAG}')";
                                var insertrootchild = _dbContext.ExecuteSqlCommand(rootchildsqlquery);

                            }

                        }
                        else
                        {
                            newprecode = model.MASTER_ACC_CODE;
                            newmastercode = model.MASTER_ACC_CODE + "." + "00";
                            var rootchildsqlquery = $@"INSERT INTO FA_CHART_OF_ACCOUNTS_SETUP (ACC_CODE,ACC_EDESC,ACC_NDESC,ACC_SNAME,TRANSACTION_TYPE,TPB_FLAG,ACC_TYPE_FLAG,MASTER_ACC_CODE,PRE_ACC_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,DELTA_FLAG,FREEZE_FLAG,ACC_NATURE,LIMIT,SHARE_VALUE,IND_VAT_FLAG,PRINTING_FLAG,IND_TDS_FLAG,CURRENT_BALANCE,BANK_ACCOUNT_NO,GROUP_START_CODE,GROUP_END_CODE,PREFIX_TEXT,ACC_ID,CONTACT_PERSON,TEL_NO,MOBILE_NO,EMAIL_ID,LINK_ID,IND_MDF_FLAG)
                                                 VALUES('{newmaxacccode}','{model.ACC_EDESC}','{model.ACC_NDESC}','{model.ACC_SNAME}','{model.TRANSACTION_TYPE}','{model.TPB_FLAG}','{model.ACC_TYPE_FLAG}','{newmastercode}','{newprecode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','N',
                                               '{model.FREEZE_FLAG}','{model.ACC_NATURE}','{model.LIMIT}','{model.SHARE_VALUE}','{model.IND_VAT_FLAG}','{model.PRINTING_FLAG}','{model.IND_TDS_FLAG}','{model.CURRENT_BALANCE}','{model.BANK_ACCOUNT_NO}','{model.GROUP_START_CODE}','{model.GROUP_END_CODE}','{model.PREFIX_TEXT}','{model.ACC_ID}','{model.CONTACT_PERSON}','{model.TEL_NO}','{model.MOBILE_NO}','{model.EMAIL_ID}','{model.LINK_ID}','{model.IND_MDF_FLAG}')";
                            var insertrootchild = _dbContext.ExecuteSqlCommand(rootchildsqlquery);


                        }

                    }
                    else
                    {
                        var newpre = string.Empty;
                        var newmaster = string.Empty;
                        var newprequery = $@"SELECT  max(REGEXP_SUBSTR(MASTER_ACC_CODE, '[^.]+', 1, 1))+1 col_one FROM FA_CHART_OF_ACCOUNTS_SETUP";
                        newpre = this._coreEntity.SqlQuery<int>(newprequery).FirstOrDefault().ToString();
                        if (Convert.ToInt32(newpre) <= 9)
                        {
                            newpre = "0" + newpre.ToString();
                        }
                        if (model.ACC_TYPE_FLAG == "N")
                        {
                            newmaster = newpre + ".01";
                        }
                        else
                        {
                            newmaster = newpre + ".00";
                        }
                        var rootchildsqlquery = $@"INSERT INTO FA_CHART_OF_ACCOUNTS_SETUP (ACC_CODE,ACC_EDESC,ACC_NDESC,ACC_SNAME,TRANSACTION_TYPE,TPB_FLAG,ACC_TYPE_FLAG,MASTER_ACC_CODE,PRE_ACC_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,DELTA_FLAG,FREEZE_FLAG,ACC_NATURE,LIMIT,SHARE_VALUE,IND_VAT_FLAG,PRINTING_FLAG,IND_TDS_FLAG,CURRENT_BALANCE,BANK_ACCOUNT_NO,GROUP_START_CODE,GROUP_END_CODE,PREFIX_TEXT,ACC_ID,CONTACT_PERSON,TEL_NO,MOBILE_NO,EMAIL_ID,LINK_ID,IND_MDF_FLAG)
                                                 VALUES('{newmaxacccode}','{model.ACC_EDESC}','{model.ACC_NDESC}','{model.ACC_SNAME}','{model.TRANSACTION_TYPE}','{model.TPB_FLAG}','{model.ACC_TYPE_FLAG}','{newpre}','00','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','N',
                                               '{model.FREEZE_FLAG}','{model.ACC_NATURE}','{model.LIMIT}','{model.SHARE_VALUE}','{model.IND_VAT_FLAG}','{model.PRINTING_FLAG}','{model.IND_TDS_FLAG}','{model.CURRENT_BALANCE}','{model.BANK_ACCOUNT_NO}','{model.GROUP_START_CODE}','{model.GROUP_END_CODE}','{model.PREFIX_TEXT}','{model.ACC_ID}','{model.CONTACT_PERSON}','{model.TEL_NO}','{model.MOBILE_NO}','{model.EMAIL_ID}','{model.LINK_ID}','{model.IND_MDF_FLAG}')";
                        var insertrootchild = _dbContext.ExecuteSqlCommand(rootchildsqlquery);
                    }
                    trans.Commit();
                    return "INSERTED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        public string udpateAccountSetup(AccountSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var companyCode = _workContext.CurrentUserinformation.company_code;

                    var sqlquery = $@"UPDATE FA_CHART_OF_ACCOUNTS_SETUP SET ACC_EDESC='{model.ACC_EDESC}', ACC_NDESC='{model.ACC_NDESC}', ACC_SNAME='{model.ACC_SNAME}',TRANSACTION_TYPE='{model.TRANSACTION_TYPE}',TPB_FLAG='{model.TPB_FLAG}',ACC_TYPE_FLAG='{model.ACC_TYPE_FLAG}',CURRENT_BALANCE='{model.CURRENT_BALANCE}',LIMIT='{model.LIMIT}',FREEZE_FLAG='{model.FREEZE_FLAG}',PRINTING_FLAG='{model.PRINTING_FLAG}',IND_TDS_FLAG='{model.IND_TDS_FLAG}',IND_VAT_FLAG='{model.IND_VAT_FLAG}',ACC_NATURE='{model.ACC_NATURE}',PREFIX_TEXT='{model.PREFIX_TEXT}',ACC_ID='{model.ACC_ID}',BANK_ACCOUNT_NO='{model.BANK_ACCOUNT_NO}',GROUP_START_CODE='{model.GROUP_START_CODE}',GROUP_END_CODE='{model.GROUP_END_CODE}',CONTACT_PERSON='{model.CONTACT_PERSON}',TEL_NO='{model.TEL_NO}',MOBILE_NO='{model.MOBILE_NO}',EMAIL_ID='{model.EMAIL_ID}',LINK_ID='{model.LINK_ID}',SHARE_VALUE='{model.SHARE_VALUE}',IND_MDF_FLAG='{model.IND_MDF_FLAG}'  WHERE ACC_CODE = '{model.ACC_CODE}'";
                    var result = _coreEntity.ExecuteSqlCommand(sqlquery);
                    trans.Commit();
                    return "UPDATED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        public List<AccountSetupModel> GetAccountListByGroupCode(string groupId)
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                string query = $@"SELECT DISTINCT 
                        INITCAP(ACC_EDESC) AS ACC_EDESC,
                        ACC_CODE ,UPPER(CREATED_BY) AS CREATED_BY ,CREATED_DATE,MODIFY_BY,MODIFY_DATE,FREEZE_FLAG,
                        MASTER_ACC_CODE, PRE_ACC_CODE,ACC_TYPE_FLAG,ACC_NATURE,TRANSACTION_TYPE,CONTACT_PERSON,TEL_NO,MOBILE_NO,EMAIL_ID,LINK_ID
                        FROM FA_CHART_OF_ACCOUNTS_SETUP
                        WHERE DELETED_FLAG = 'N' 
                        AND ACC_TYPE_FLAG='T'
                        AND PRE_ACC_CODE = '{groupId}'
                        AND COMPANY_CODE='{company_code}'
                        ORDER BY ACC_EDESC";
                var accountCodeList = _dbContext.SqlQuery<AccountSetupModel>(query).ToList();
                return accountCodeList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public List<AccountSetupModel> GetAccountList(string searchtext)
        {
            try
            {
                if (!string.IsNullOrEmpty(searchtext))
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    string query = $@"SELECT DISTINCT 
                        INITCAP(ACC_EDESC) AS ACC_EDESC,
                        ACC_CODE ,UPPER(CREATED_BY) AS CREATED_BY ,CREATED_DATE,MODIFY_BY,MODIFY_DATE,FREEZE_FLAG,
                        MASTER_ACC_CODE, PRE_ACC_CODE,ACC_TYPE_FLAG,ACC_NATURE,TRANSACTION_TYPE,CONTACT_PERSON,TEL_NO,MOBILE_NO,EMAIL_ID,LINK_ID
                        FROM FA_CHART_OF_ACCOUNTS_SETUP
                        WHERE DELETED_FLAG = 'N'
                        AND UPPER(ACC_EDESC) LIKE UPPER('%{ searchtext }%')
                        AND COMPANY_CODE='{company_code}'
                        ORDER BY ACC_EDESC";
                    var accountCodeList = _dbContext.SqlQuery<AccountSetupModel>(query).ToList();
                    return accountCodeList;
                }
                return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public string GetNewAccountCode()
        {
            var newmaxacccodequery = $@"SELECT MAX(TO_NUMBER(ACC_CODE))+1 as MAX_ACC_CODE FROM fa_chart_of_accounts_setup";
            return this._coreEntity.SqlQuery<int>(newmaxacccodequery).FirstOrDefault().ToString();
        }
        public List<ProcessSetupModel> GetProcessListByprocessCode(string groupId)
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                string query = $@"SELECT DISTINCT 
                        INITCAP(PROCESS_EDESC) AS PROCESS_EDESC,
                        PROCESS_CODE ,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,
                        PRE_PROCESS_CODE,PROCESS_FLAG
                        FROM MP_PROCESS_SETUP
                        WHERE DELETED_FLAG = 'N' 
                        AND PROCESS_FLAG='R'
                        AND PRE_PROCESS_CODE = '{groupId}'
                        AND COMPANY_CODE='{company_code}'
                        ORDER BY ACC_EDESC";
                var processCodeList = _dbContext.SqlQuery<ProcessSetupModel>(query).ToList();
                return processCodeList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public List<BudgetCenterSetupModel> GetBudgetCenterListByGroupCode(string groupId)
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                string query = $@"SELECT DISTINCT 
                        INITCAP(BUDGET_EDESC) AS BUDGET_EDESC,
                        BUDGET_CODE ,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,
                        BUDGET_CODE AS MASTER_BUDGET_CODE, PRE_BUDGET_CODE,GROUP_SKU_FLAG AS BUDGET_TYPE_FLAG,FREEZE_FLAG
                        FROM bc_budget_center_setup
                        WHERE DELETED_FLAG = 'N' 
                        AND GROUP_SKU_FLAG='I'
                        AND PRE_BUDGET_CODE = '{groupId}'
                        AND COMPANY_CODE='{company_code}'
                        ORDER BY BUDGET_EDESC";
                var budgetCenterCodeList = _dbContext.SqlQuery<BudgetCenterSetupModel>(query).ToList();
                return budgetCenterCodeList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public List<BudgetCenterSetupModel> GetAllBudgetCenterList(string searchText)
        {
            try
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    string query = $@"SELECT DISTINCT 
                        INITCAP(BUDGET_EDESC) AS BUDGET_EDESC,
                        BUDGET_CODE ,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,
                        BUDGET_CODE AS MASTER_BUDGET_CODE, PRE_BUDGET_CODE,GROUP_SKU_FLAG AS BUDGET_TYPE_FLAG,FREEZE_FLAG
                        FROM bc_budget_center_setup
                        WHERE DELETED_FLAG = 'N'
                        AND COMPANY_CODE='{company_code}'
                        AND UPPER(BUDGET_EDESC) LIKE UPPER('%{ searchText }%')
                        ORDER BY BUDGET_EDESC";
                    var budgetCenterCodeList = _dbContext.SqlQuery<BudgetCenterSetupModel>(query).ToList();
                    return budgetCenterCodeList;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public string DeleteBudgetCenterByBudgetCode(string budgetCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(budgetCode)) { budgetCode = string.Empty; }

                var masterQry = $@"SELECT BUDGET_CODE, GROUP_SKU_FLAG FROM bc_budget_center_setup WHERE BUDGET_CODE = '{budgetCode}' and COMPANY_CODE= '{companyCode}'";
                var masterBudgetCode = _dbContext.SqlQuery<BudgetCenterSetupModel>(masterQry).FirstOrDefault();
                if (masterBudgetCode.GROUP_SKU_FLAG == "G")
                {
                    var childQry = $@"SELECT COUNT(*) FROM bc_budget_center_setup WHERE PRE_BUDGET_CODE like ('{masterBudgetCode.BUDGET_CODE}%') AND DELETED_FLAG = 'N'";
                    var childResult = _dbContext.SqlQuery<int>(childQry).FirstOrDefault();
                    if (childResult > 0)
                    {
                        return "HAS_CHILD";
                    }
                }
                var sqlquery = $@"UPDATE bc_budget_center_setup SET DELETED_FLAG = 'Y' WHERE BUDGET_CODE='{budgetCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                return "DELETED";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string createNewBudgetSetup(BudgetCenterSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    if (model.FREEZE_FLAG == null)
                    {
                        model.FREEZE_FLAG = "N";
                    }

                    if (model.MASTER_BUDGET_CODE != null && model.MASTER_BUDGET_CODE != "")
                    {
                        var maxPreCode = string.Empty;
                        var newprecode = string.Empty;
                        var newmastercode = string.Empty;
                        if (model.GROUP_SKU_FLAG == "G")
                        {

                            if (model.MASTER_BUDGET_CODE != null && model.MASTER_BUDGET_CODE != "")
                            {
                                var maxprecodequery = $@"select (count(*) + 1) as MAXCODE from bc_budget_center_setup where pre_budget_code like '{model.MASTER_BUDGET_CODE}' and company_code = '{_workContext.CurrentUserinformation.company_code}'";
                                maxPreCode = this._coreEntity.SqlQuery<int>(maxprecodequery).FirstOrDefault().ToString();
                                if (maxPreCode != null)
                                {
                                    if (Convert.ToInt32(maxPreCode) <= 9)
                                    {
                                        maxPreCode = "0" + maxPreCode.ToString();
                                    }

                                }
                                newprecode = model.MASTER_BUDGET_CODE;
                                newmastercode = model.MASTER_BUDGET_CODE + "." + maxPreCode;
                                var childsqlquery = $@"INSERT INTO BC_BUDGET_CENTER_SETUP (BUDGET_CODE,BUDGET_EDESC,BUDGET_NDESC,GROUP_SKU_FLAG,PRE_BUDGET_CODE,REMARKS,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,FREEZE_FLAG,ASSIGN_TO,ACC_CODE) VALUES('{newmastercode}','{model.BUDGET_EDESC}','{model.BUDGET_EDESC}','{model.GROUP_SKU_FLAG}','{newprecode}','{model.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','{model.FREEZE_FLAG}','{model.ASSIGN_TO}','{model.ACC_CODE}')";
                                var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);

                            }

                        }
                        else
                        {
                            var precount = $@"select  count(*)  as count from bc_budget_center_setup where pre_budget_code = '{model.MASTER_BUDGET_CODE}' and company_code = '{_workContext.CurrentUserinformation.company_code}' and  group_sku_flag='I'";
                            var PreCodeNumber = this._coreEntity.SqlQuery<int>(precount).FirstOrDefault().ToString();
                            if (PreCodeNumber != null)
                            {
                                if (Convert.ToInt32(PreCodeNumber) > 0)
                                {
                                    newprecode = model.MASTER_BUDGET_CODE;
                                    if (Convert.ToInt32(PreCodeNumber) <= 9)
                                    {
                                        newmastercode = model.MASTER_BUDGET_CODE + "." + "0" + PreCodeNumber.ToString();
                                    }
                                    else
                                    {
                                        newmastercode = model.MASTER_BUDGET_CODE + "." + PreCodeNumber.ToString();
                                    }

                                    var rootchildsqlquery = $@"INSERT INTO BC_BUDGET_CENTER_SETUP (BUDGET_CODE,BUDGET_EDESC,BUDGET_NDESC,GROUP_SKU_FLAG,PRE_BUDGET_CODE,REMARKS,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,FREEZE_FLAG,ASSIGN_TO,ACC_CODE) VALUES('{newmastercode}','{model.BUDGET_EDESC}','{model.BUDGET_EDESC}','{model.GROUP_SKU_FLAG}','{newprecode}','{model.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','{model.FREEZE_FLAG}','{model.ASSIGN_TO}','{model.ACC_CODE}')";
                                    var insertrootchild = _coreEntity.ExecuteSqlCommand(rootchildsqlquery);

                                }
                                else
                                {
                                    newprecode = model.MASTER_BUDGET_CODE;
                                    newmastercode = model.MASTER_BUDGET_CODE + "." + "00";
                                    var rootchildsqlquery = $@"INSERT INTO BC_BUDGET_CENTER_SETUP (BUDGET_CODE,BUDGET_EDESC,BUDGET_NDESC,GROUP_SKU_FLAG,PRE_BUDGET_CODE,REMARKS,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,FREEZE_FLAG,ASSIGN_TO,ACC_CODE) VALUES('{newmastercode}','{model.BUDGET_EDESC}','{model.BUDGET_EDESC}','{model.GROUP_SKU_FLAG}','{newprecode}','{model.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','{model.FREEZE_FLAG}','{model.ASSIGN_TO}','{model.ACC_CODE}')";
                                    var insertrootchild = _coreEntity.ExecuteSqlCommand(rootchildsqlquery);
                                }
                            }



                        }

                    }
                    else
                    {
                        var newpre = string.Empty;
                        var newmaster = string.Empty;
                        var newprequery = $@"SELECT COUNT(BUDGET_CODE)+1 as MAX_BUDGET_CODE FROM BC_BUDGET_CENTER_SETUP";
                        newpre = this._coreEntity.SqlQuery<int>(newprequery).FirstOrDefault().ToString();
                        if (Convert.ToInt32(newpre) <= 9)
                        {
                            newpre = "0" + newpre.ToString();
                        }
                        if (model.GROUP_SKU_FLAG == "I")
                        {
                            newmaster = newpre + ".01";
                        }
                        else
                        {
                            newmaster = newpre;
                        }
                        newpre = "00";
                        var rootsqlquery = $@"INSERT INTO BC_BUDGET_CENTER_SETUP (BUDGET_CODE,BUDGET_EDESC,BUDGET_NDESC,GROUP_SKU_FLAG,PRE_BUDGET_CODE,REMARKS,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,FREEZE_FLAG,ASSIGN_TO,ACC_CODE) VALUES('{newmaster}','{model.BUDGET_EDESC}','{model.BUDGET_EDESC}','{model.GROUP_SKU_FLAG}','{newpre}','{model.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','{model.FREEZE_FLAG}','{model.ASSIGN_TO}','{model.ACC_CODE}')";
                        var insertroot = _coreEntity.ExecuteSqlCommand(rootsqlquery);
                    }
                    trans.Commit();
                    return "INSERTED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }


        }
        public string udpateBudgetSetup(BudgetCenterSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var companyCode = _workContext.CurrentUserinformation.company_code;

                    var sqlquery = $@"UPDATE BC_BUDGET_CENTER_SETUP SET BUDGET_EDESC='{model.BUDGET_EDESC}', BUDGET_NDESC='{model.BUDGET_NDESC}', ASSIGN_TO='{model.ASSIGN_TO}', ACC_CODE='{model.ACC_CODE}', REMARKS='{model.REMARKS}', FREEZE_FLAG='{model.FREEZE_FLAG}'  WHERE BUDGET_CODE = '{model.BUDGET_CODE}'";
                    var result = _coreEntity.ExecuteSqlCommand(sqlquery);
                    trans.Commit();
                    return "UPDATED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        #endregion
        #region Location
        public string DeleteLocationSetupByLocationCode(string locationCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(locationCode)) { locationCode = string.Empty; }
                var masterQry = $@"SELECT LOCATION_CODE, GROUP_SKU_FLAG FROM IP_LOCATION_SETUP WHERE LOCATION_CODE = '{locationCode}' and COMPANY_CODE= '{companyCode}'";
                var masterLocationCode = _dbContext.SqlQuery<LocationSetupModel>(masterQry).FirstOrDefault();
                if (masterLocationCode.GROUP_SKU_FLAG == "G")
                {
                    var childQry = $@"SELECT COUNT(*) FROM IP_LOCATION_SETUP WHERE PRE_LOCATION_CODE like ('{masterLocationCode.LOCATION_CODE}%') AND DELETED_FLAG = 'N'";
                    var childResult = _dbContext.SqlQuery<int>(childQry).FirstOrDefault();
                    if (childResult > 0)
                    {
                        return "HAS_CHILD";
                    }
                }
                var sqlquery = $@"UPDATE IP_LOCATION_SETUP SET DELETED_FLAG = 'Y' WHERE LOCATION_CODE='{locationCode}' AND COMPANY_CODE ='{companyCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                return "DELETED";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public LocationSetupModel GetLocationDataByLocationCode(string locationcode)
        {
            try
            {
                if (string.IsNullOrEmpty(locationcode)) { locationcode = string.Empty; }
                string Query = $@"SELECT * FROM IP_LOCATION_SETUP WHERE LOCATION_CODE='{locationcode}' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'   ORDER BY TO_NUMBER(LOCATION_CODE) ASC";
                LocationSetupModel entity = this._dbContext.SqlQuery<LocationSetupModel>(Query).FirstOrDefault();

                string parentlocationQuery = $@"SELECT LOCATION_CODE FROM IP_LOCATION_SETUP where LOCATION_CODE = (SELECT PRE_LOCATION_CODE FROM IP_LOCATION_SETUP WHERE LOCATION_CODE='{locationcode}' 
                                AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}') and company_code='{_workContext.CurrentUserinformation.company_code}'";
                entity.PARENT_LOCATION_CODE = this._dbContext.SqlQuery<string>(parentlocationQuery).FirstOrDefault();

                return entity;

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public string createNewLocationSetup(LocationSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var newmaxlocationcode = string.Empty;
                    var newmaxlocationcodequery = $@"SELECT COUNT(LOCATION_CODE)+1 as MAX_LOCATION_CODE FROM IP_LOCATION_SETUP WHERE PRE_LOCATION_CODE = '{model.LOCATION_CODE}'";
                    newmaxlocationcode = this._coreEntity.SqlQuery<int>(newmaxlocationcodequery).FirstOrDefault().ToString();
                    if (model.LOCATION_CODE != null && model.PRE_LOCATION_CODE != "")
                    {
                        var maxPreCode = string.Empty;
                        var newprecode = string.Empty;
                        var newmastercode = string.Empty;
                        //if (model.GROUP_SKU_FLAG == "G")
                        //{

                        if (model.LOCATION_CODE != null && model.LOCATION_CODE != "")
                        {
                            var maxprecodequery = $@"select (count(*) + 1) as MAXCODE from IP_LOCATION_SETUP where pre_location_code like '{model.LOCATION_CODE}' and company_code = '{_workContext.CurrentUserinformation.company_code}'";
                            maxPreCode = this._coreEntity.SqlQuery<int>(maxprecodequery).FirstOrDefault().ToString();
                            if (maxPreCode != null)
                            {
                                if (Convert.ToInt32(maxPreCode) <= 9)
                                {
                                    maxPreCode = "0" + maxPreCode.ToString();
                                }

                            }
                            newprecode = model.PRE_LOCATION_CODE;
                            newmastercode = model.PRE_LOCATION_CODE + "." + maxPreCode;
                            var childsqlquery = $@"INSERT INTO IP_LOCATION_SETUP (LOCATION_CODE,
                                                    LOCATION_EDESC,LOCATION_NDESC,PRE_LOCATION_CODE,ADDRESS,
                                                    AUTH_CONTACT_PERSON,TELEPHONE_MOBILE_NO,EMAIL,FAX,
                                                    LOCATION_TYPE_CODE,GROUP_SKU_FLAG,REMARKS,COMPANY_CODE,
                                                    CREATED_BY,CREATED_DATE,DELETED_FLAG,BRANCH_CODE,STORAGE_CAPACITY,MU_CODE,LOCATION_ID)
                                                    VALUES('{newmastercode}','{model.LOCATION_EDESC}','{model.LOCATION_NDESC}','{newprecode}','{model.ADDRESS}','{model.AUTH_CONTACT_PERSON}','{model.TELEPHONE_MOBILE_NO}'
                                                        ,'{model.EMAIL}','{model.FAX}','{model.LOCATION_TYPE_CODE}','{model.GROUP_SKU_FLAG}','{model.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),
                                                         'N','{model.BRANCH_CODE}','{model.STORAGE_CAPACITY}','{model.MU_CODE}','{model.LOCATION_ID}')";
                            var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);

                        }

                        //}
                        //else
                        //{
                        //    newprecode = model.LOCATION_CODE;
                        //    newmastercode = model.LOCATION_CODE + "." + "00";
                        //    var rootchildsqlquery = $@"INSERT INTO IP_LOCATION_SETUP (LOCATION_CODE,LOCATION_EDESC,LOCATION_NDESC,TRANSACTION_TYPE,TPB_FLAG,GROUP_SKU_FLAG,LOCATION_CODE,PRE_LOCATION_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,DELTA_FLAG,FREEZE_FLAG,LOCATION_NATURE,LIMIT,SHARE_VALUE,IND_VAT_FLAG,PRINTING_FLAG,IND_TDS_FLAG) VALUES('{newmaxlocationcode}','{model.LOCATION_EDESC}','{model.LOCATION_EDESC}','{model.GROUP_SKU_FLAG}','','{model.GROUP_SKU_FLAG}','{newmastercode}','{newprecode}','{_workContext.CurrentUserinformation.company_code}','ADMIN',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','N','N','','',0,'N','N','N')";
                        //    var insertrootchild = _dbContext.ExecuteSqlCommand(rootchildsqlquery);

                        //}

                    }
                    else
                    {
                        var newpre = string.Empty;
                        var newmaster = string.Empty;
                        var newprequery = $@"SELECT  max(REGEXP_SUBSTR(LOCATION_CODE, '[^.]+', 1, 1))+1 col_one FROM IP_LOCATION_SETUP";
                        newpre = this._coreEntity.SqlQuery<int>(newprequery).FirstOrDefault().ToString();
                        if (Convert.ToInt32(newpre) <= 9)
                        {
                            newpre = "0" + newpre.ToString();
                        }
                        if (model.GROUP_SKU_FLAG == "G")
                        {
                            newmaster = newpre + ".01";
                        }
                        else
                        {
                            newmaster = newpre + ".00";
                        }
                        if (model.STORAGE_CAPACITY == null)
                        {
                            model.STORAGE_CAPACITY = 0;
                        }
                        var rootquery = $@"INSERT INTO IP_LOCATION_SETUP (LOCATION_CODE,
                                                    LOCATION_EDESC,LOCATION_NDESC,PRE_LOCATION_CODE,ADDRESS,
                                                    AUTH_CONTACT_PERSON,TELEPHONE_MOBILE_NO,EMAIL,FAX,
                                                    LOCATION_TYPE_CODE,GROUP_SKU_FLAG,REMARKS,COMPANY_CODE,
                                                    CREATED_BY,CREATED_DATE,DELETED_FLAG,BRANCH_CODE,STORAGE_CAPACITY,MU_CODE,LOCATION_ID)
                                                    VALUES('{newmaster}','{model.LOCATION_EDESC}','{model.LOCATION_NDESC}','00','{model.ADDRESS}','{model.AUTH_CONTACT_PERSON}','{model.TELEPHONE_MOBILE_NO}'
                                                        ,'{model.EMAIL}','{model.FAX}','{model.LOCATION_TYPE_CODE}','{model.GROUP_SKU_FLAG}','{model.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),
                                                         'N','{_workContext.CurrentUserinformation.branch_code}',{model.STORAGE_CAPACITY},'{model.MU_CODE}','{model.LOCATION_ID}')";
                        var insertroot = _coreEntity.ExecuteSqlCommand(rootquery);
                    }
                    trans.Commit();
                    return "INSERTED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        public string udpateLocationSetup(LocationSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var companyCode = _workContext.CurrentUserinformation.company_code;

                    var sqlquery = $@"UPDATE IP_LOCATION_SETUP SET LOCATION_EDESC='{model.LOCATION_EDESC}',LOCATION_NDESC='{model.LOCATION_NDESC}',ADDRESS='{model.ADDRESS}',AUTH_CONTACT_PERSON='{model.AUTH_CONTACT_PERSON}',
                                   TELEPHONE_MOBILE_NO='{model.TELEPHONE_MOBILE_NO}',EMAIL='{model.EMAIL}', FAX= '{model.FAX}',LOCATION_TYPE_CODE='{model.LOCATION_TYPE_CODE}',MODIFY_BY='{_workContext.CurrentUserinformation.login_code}', MODIFY_DATE=SYSDATE,
                                   BRANCH_CODE='{model.BRANCH_CODE}',STORAGE_CAPACITY='{model.STORAGE_CAPACITY}',REMARKS='{model.REMARKS}',MU_CODE='{model.MU_CODE}',LOCATION_ID='{model.LOCATION_ID}' WHERE LOCATION_CODE = '{model.LOCATION_CODE}' AND COMPANY_CODE='{companyCode}'";
                    var result = _coreEntity.ExecuteSqlCommand(sqlquery);
                    trans.Commit();
                    return "UPDATED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        public List<LocationSetupModel> GetLocationListByGroupCode(string groupId)
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                string query = $@"SELECT DISTINCT 
                        INITCAP(LOCATION_EDESC) AS LOCATION_EDESC,AUTH_CONTACT_PERSON,TELEPHONE_MOBILE_NO,
                        LOCATION_CODE ,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,
                         PRE_LOCATION_CODE,GROUP_SKU_FLAG
                        FROM IP_LOCATION_SETUP
                        WHERE DELETED_FLAG = 'N' 
                        AND GROUP_SKU_FLAG='I'
                        AND PRE_LOCATION_CODE = '{groupId}'
                        AND COMPANY_CODE='{company_code}'
                        ORDER BY LOCATION_EDESC";
                var locationountCodeList = _dbContext.SqlQuery<LocationSetupModel>(query).ToList();
                return locationountCodeList;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public List<LocationSetupModel> GetAllLocationList(string searchText)
        {
            try
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    string query = $@"SELECT DISTINCT 
                        INITCAP(LOCATION_EDESC) AS LOCATION_EDESC,AUTH_CONTACT_PERSON,TELEPHONE_MOBILE_NO,
                        LOCATION_CODE ,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,
                         PRE_LOCATION_CODE,GROUP_SKU_FLAG
                        FROM IP_LOCATION_SETUP
                        WHERE DELETED_FLAG = 'N'
                        AND COMPANY_CODE='{company_code}'
                        AND UPPER(LOCATION_EDESC) LIKE UPPER('%{ searchText }%')
                        ORDER BY LOCATION_EDESC";
                    var locationountCodeList = _dbContext.SqlQuery<LocationSetupModel>(query).ToList();
                    return locationountCodeList;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region Regional
        public string DeleteRegionalSetupByRegionalCode(string regionCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(regionCode)) { regionCode = string.Empty; }
                var masterQry = $@"SELECT REGION_CODE, GROUP_SKU_FLAG FROM SA_REGIONAL_SETUP WHERE REGION_CODE = '{regionCode}' and COMPANY_CODE= '{companyCode}'";
                var masterRegionalCode = _dbContext.SqlQuery<RegionalSetupModel>(masterQry).FirstOrDefault();
                if (masterRegionalCode.GROUP_SKU_FLAG == "G")
                {
                    var childQry = $@"SELECT COUNT(*) FROM SA_REGIONAL_SETUP WHERE PRE_REGION_CODE like ('{masterRegionalCode.REGION_CODE}%') and DELETED_FLAG='N'";
                    var childResult = _dbContext.SqlQuery<int>(childQry).FirstOrDefault();
                    if (childResult > 0)
                    {
                        return "HAS_CHILD";
                    }
                }
                var sqlquery = $@"UPDATE SA_REGIONAL_SETUP SET DELETED_FLAG = 'Y' WHERE REGION_CODE='{regionCode}' AND COMPANY_CODE ='{companyCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                return "DELETED";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public RegionalSetupModel GetRegionalDataByRegionalCode(string regioncode)
        {
            try
            {
                if (string.IsNullOrEmpty(regioncode)) { regioncode = string.Empty; }
                string Query = $@"SELECT * from sa_regional_setup WHERE REGION_CODE='{regioncode}' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'   ORDER BY TO_NUMBER(REGION_CODE) ASC";
                RegionalSetupModel entity = this._dbContext.SqlQuery<RegionalSetupModel>(Query).FirstOrDefault();

                string PARENT_REGION_CODE = $@"  SELECT REGION_CODE FROM sa_regional_setup where REGION_CODE = (SELECT PRE_REGION_CODE FROM sa_regional_setup WHERE REGION_CODE='{regioncode}' 
                                AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}') and company_code='{_workContext.CurrentUserinformation.company_code}'";
                entity.PARENT_REGION_CODE = this._dbContext.SqlQuery<string>(PARENT_REGION_CODE).FirstOrDefault();

                return entity;

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public string createNewRegionalSetup(RegionalSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    //var newmaxregioncode = string.Empty;
                    //var newmaxregioncodequery = $@"SELECT MAX(REGION_CODE)+1 as MAX_REGION_CODE FROM SA_REGIONAL_SETUP";
                    //newmaxregioncode = this._dbContext.SqlQuery<int>(newmaxregioncodequery).FirstOrDefault().ToString();
                    if (model.REGION_CODE != null && model.REGION_CODE != "")
                    {
                        var maxPreCode = string.Empty;
                        var newprecode = string.Empty;
                        var newmastercode = string.Empty;
                        if (model.REGION_CODE != null && model.REGION_CODE != "")
                        {
                            var maxprecodequery = $@"select (count(*) + 1) as MAXCODE from SA_REGIONAL_SETUP where pre_region_code like '{model.REGION_CODE}' and company_code = '{_workContext.CurrentUserinformation.company_code}'";
                            maxPreCode = this._coreEntity.SqlQuery<int>(maxprecodequery).FirstOrDefault().ToString();
                            if (maxPreCode != null)
                            {
                                if (Convert.ToInt32(maxPreCode) <= 9)
                                {
                                    maxPreCode = "0" + maxPreCode.ToString();
                                }

                            }
                            newprecode = model.REGION_CODE;
                            newmastercode = model.REGION_CODE + "." + maxPreCode;
                            var childsqlquery = $@"INSERT INTO SA_REGIONAL_SETUP (REGION_EDESC,REGION_NDESC,GROUP_SKU_FLAG,REGION_CODE,PRE_REGION_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,REMARKS) VALUES('{model.REGION_EDESC}','{model.REGION_NDESC}','{model.GROUP_SKU_FLAG}','{newmastercode}','{newprecode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','{model.REMARKS}')";
                            var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);

                        }
                    }
                    else
                    {
                        var newpre = string.Empty;
                        var newmaster = string.Empty;
                        var newprequery = $@"SELECT  max(REGEXP_SUBSTR(REGION_CODE, '[^.]+', 1, 1))+1 col_one FROM SA_REGIONAL_SETUP";
                        newpre = this._coreEntity.SqlQuery<int>(newprequery).FirstOrDefault().ToString();
                        if (Convert.ToInt32(newpre) <= 9)
                        {
                            newpre = "0" + newpre.ToString();
                        }
                        if (model.GROUP_SKU_FLAG == "G")
                        {
                            newmaster = newpre + ".01";
                        }
                        else
                        {
                            newmaster = newpre + ".00";
                        }
                        var rootsqlquery = $@"INSERT INTO SA_REGIONAL_SETUP (REGION_EDESC,REGION_NDESC,GROUP_SKU_FLAG,REGION_CODE,PRE_REGION_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,REMARKS) VALUES('{model.REGION_EDESC}','{model.REGION_NDESC}','{model.GROUP_SKU_FLAG}','{newmaster}','00','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','{model.REMARKS}')";
                        var insertroot = _coreEntity.ExecuteSqlCommand(rootsqlquery);
                    }
                    trans.Commit();
                    return "INSERTED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        public string udpateRegionalSetup(RegionalSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var companyCode = _workContext.CurrentUserinformation.company_code;

                    var sqlquery = $@"UPDATE SA_REGIONAL_SETUP SET REGION_EDESC='{model.REGION_EDESC}', REGION_NDESC='{model.REGION_NDESC}',GROUP_SKU_FLAG='{model.GROUP_SKU_FLAG}',REMARKS='{model.REMARKS}',MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',MODIFY_DATE=SYSDATE WHERE REGION_CODE = '{model.REGION_CODE}'";
                    var result = _coreEntity.ExecuteSqlCommand(sqlquery);
                    trans.Commit();
                    return "UPDATED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        public List<RegionalSetupModel> GetRegionalListByGroupCode(string groupId)
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                string query = $@"SELECT DISTINCT 
                        INITCAP(REGION_EDESC) AS REGION_EDESC,REMARKS,
                        REGION_CODE ,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,
                        REGION_CODE, PRE_REGION_CODE,GROUP_SKU_FLAG
                        FROM SA_REGIONAL_SETUP
                        WHERE DELETED_FLAG = 'N' 
                        AND GROUP_SKU_FLAG='I'
                        AND PRE_REGION_CODE = '{groupId}'
                        AND COMPANY_CODE='{company_code}'
                        ORDER BY REGION_EDESC";
                var regionountCodeList = _dbContext.SqlQuery<RegionalSetupModel>(query).ToList();
                return regionountCodeList;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public List<RegionalSetupModel> GetAllRegionalList(string searchText)
        {
            try
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    string query = $@"SELECT DISTINCT 
                        INITCAP(REGION_EDESC) AS REGION_EDESC,REMARKS,
                        REGION_CODE ,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,
                        REGION_CODE, PRE_REGION_CODE,GROUP_SKU_FLAG
                        FROM SA_REGIONAL_SETUP
                        WHERE DELETED_FLAG = 'N'
                        AND COMPANY_CODE='{company_code}'
                        AND UPPER(REGION_EDESC) LIKE UPPER('%{ searchText }%')
                        ORDER BY REGION_EDESC";
                    var regionountCodeList = _dbContext.SqlQuery<RegionalSetupModel>(query).ToList();
                    return regionountCodeList;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region Process
        public string DeleteProcessSetupByProcessCode(string processCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(processCode)) { processCode = string.Empty; }
                var masterQry = $@"SELECT PROCESS_CODE, PROCESS_FLAG FROM MP_PROCESS_SETUP WHERE PROCESS_CODE = '{processCode}' and COMPANY_CODE= '{companyCode}'";
                var masterProcessCode = _dbContext.SqlQuery<ProcessSetupModel>(masterQry).FirstOrDefault();
                if (masterProcessCode.PROCESS_FLAG == "P" || masterProcessCode.PROCESS_FLAG == "C")
                {
                    var childQry = $@"SELECT COUNT(*) FROM MP_PROCESS_SETUP WHERE PRE_PROCESS_CODE like ('{masterProcessCode.PROCESS_CODE}%') AND DELETED_FLAG = 'N'";
                    var childResult = _dbContext.SqlQuery<int>(childQry).FirstOrDefault();
                    if (childResult > 0)
                    {
                        return "HAS_CHILD";
                    }
                }
                var sqlquery = $@"UPDATE MP_PROCESS_SETUP SET DELETED_FLAG = 'Y' WHERE PROCESS_CODE='{processCode}' AND COMPANY_CODE ='{companyCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                return "DELETED";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ProcessSetupModel GetProcessDataByProcessCode(string processcode)
        {
            try
            {
                if (string.IsNullOrEmpty(processcode)) { processcode = string.Empty; }
                string Query = $@"SELECT PROCESS_CODE,
                                PROCESS_EDESC,PROCESS_NDESC,PROCESS_TYPE_CODE,PROCESS_FLAG
                                ,PRE_PROCESS_CODE,INDEX_ITEM_CODE,INDEX_CAPACITY,INDEX_MU_CODE
                                ,REMARKS,LOCATION_CODE,COMPANY_CODE,CREATED_BY
                                ,CREATED_DATE,DELETED_FLAG,INDEX_PERIOD_CODE,INDEX_TIME_REQUIRED
                                ,INDEX_TIME_MU_CODE,INDEX_TIME_PERIOD_CODE,INPUT_INDEX_ITEM_CODE
                                ,PRIORITY_ORDER_NO,SYN_ROWID,MODIFY_DATE
                                ,MODIFY_BY FROM MP_PROCESS_SETUP WHERE PROCESS_CODE='{processcode}'
                                AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'   ORDER BY TO_NUMBER(PROCESS_CODE) ASC";
                ProcessSetupModel entity = this._dbContext.SqlQuery<ProcessSetupModel>(Query).FirstOrDefault();

                string parentproccessquery = $@" SELECT PROCESS_CODE FROM MP_PROCESS_SETUP where PROCESS_CODE = (SELECT PRE_PROCESS_CODE FROM MP_PROCESS_SETUP WHERE PROCESS_CODE='{processcode}' 
                                AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}') and company_code='{_workContext.CurrentUserinformation.company_code}'";
                entity.PARENT_PROCESS_CODE = this._dbContext.SqlQuery<string>(parentproccessquery).FirstOrDefault();
                return entity;

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public string createNewProcessSetup(ProcessSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var newmaxprocesscode = string.Empty;
                    var newmaxprocesscodequery = $@"SELECT COUNT(PROCESS_CODE)+1 as MAX_PROCESS_COD FROM MP_PROCESS_SETUP WHERE COMPANY_CODE= '{_workContext.CurrentUserinformation.company_code}'";
                    newmaxprocesscode = this._coreEntity.SqlQuery<int>(newmaxprocesscodequery).FirstOrDefault().ToString();
                    if (newmaxprocesscode != "" && Convert.ToInt32(newmaxprocesscode) <= 9)
                    {
                        newmaxprocesscode = '0' + newmaxprocesscode;
                    }
                    if (model.PROCESS_CODE != null && model.PROCESS_CODE != "")
                    {
                        var maxPreCode = string.Empty;
                        var newprecode = string.Empty;
                        var newmastercode = string.Empty;


                        if (model.PROCESS_CODE != null && model.PROCESS_CODE != "")
                        {
                            var maxprecodequery = $@"select (count(*) + 1) as MAXCODE from MP_PROCESS_SETUP where PRE_PROCESS_CODE like '{model.PROCESS_CODE}' and company_code = '{_workContext.CurrentUserinformation.company_code}'";
                            maxPreCode = this._coreEntity.SqlQuery<int>(maxprecodequery).FirstOrDefault().ToString();
                            if (maxPreCode != null)
                            {
                                if (Convert.ToInt32(maxPreCode) <= 9)
                                {
                                    maxPreCode = "0" + maxPreCode.ToString();
                                }

                            }
                            newprecode = model.PROCESS_CODE;
                            newmastercode = model.PROCESS_CODE + "." + maxPreCode;
                            var childsqlquery = $@"INSERT INTO MP_PROCESS_SETUP (PROCESS_CODE,PROCESS_EDESC,PROCESS_NDESC,PROCESS_TYPE_CODE,PROCESS_FLAG,PRE_PROCESS_CODE,REMARKS,LOCATION_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,PRIORITY_ORDER_NO) VALUES('{newmastercode}','{model.PROCESS_EDESC}','{model.PROCESS_NDESC}','{model.PROCESS_TYPE_CODE}','{model.PROCESS_FLAG}','{model.PROCESS_CODE}','{model.REMARKS}','{model.LOCATION_CODE}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','{model.PRIORITY_ORDER_NO}')";
                            var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);

                        }



                    }
                    else
                    {
                        var newpre = string.Empty;
                        var newmaster = string.Empty;
                        var newprequery = $@"SELECT NVL(max(REGEXP_SUBSTR(PROCESS_CODE, '[^.]+', 1, 1)),0)+1 col_one FROM MP_PROCESS_SETUP";
                        newpre = this._coreEntity.SqlQuery<int>(newprequery).FirstOrDefault().ToString();
                        if (Convert.ToInt32(newpre) <= 9)
                        {
                            newpre = "0" + newpre.ToString();
                        }
                        if (model.PROCESS_FLAG == "C" || model.PROCESS_FLAG == "P")
                        {
                            newmaster = newpre + ".01";
                        }
                        else
                        {
                            newmaster = newpre + ".00";
                        }
                        var rootsqlquery = $@"INSERT INTO MP_PROCESS_SETUP (PROCESS_CODE,PROCESS_EDESC,PROCESS_NDESC,PROCESS_TYPE_CODE,PROCESS_FLAG,PRE_PROCESS_CODE,REMARKS,LOCATION_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,PRIORITY_ORDER_NO) VALUES('{newmaxprocesscode}','{model.PROCESS_EDESC}','{model.PROCESS_NDESC}','{model.PROCESS_TYPE_CODE}','{model.PROCESS_FLAG}','00','{model.REMARKS}','{model.LOCATION_CODE}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','{model.PRIORITY_ORDER_NO}')";
                        var insertroot = _coreEntity.ExecuteSqlCommand(rootsqlquery);
                    }
                    trans.Commit();
                    return "INSERTED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }


        public string udpateProcessSetup(ProcessSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var companyCode = _workContext.CurrentUserinformation.company_code;

                    var sqlquery = $@"UPDATE MP_PROCESS_SETUP SET PROCESS_EDESC='{model.PROCESS_EDESC}', PROCESS_NDESC='{model.PROCESS_NDESC}',PROCESS_TYPE_CODE='{model.PROCESS_TYPE_CODE}',PROCESS_FLAG='{model.PROCESS_FLAG}',REMARKS='{model.REMARKS}',LOCATION_CODE='{model.LOCATION_CODE}',PRIORITY_ORDER_NO='{model.PRIORITY_ORDER_NO}' WHERE PROCESS_CODE = '{model.PROCESS_CODE}'";
                    var result = _coreEntity.ExecuteSqlCommand(sqlquery);
                    trans.Commit();
                    return "UPDATED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        // input routine data 
        public List<RoutineInputModel> GetProcessInputGriddata()
        {

            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                string query = $@"SELECT * FROM MP_ROUTINE_INPUT_SETUP
                        WHERE DELETED_FLAG = 'N' 
                         AND COMPANY_CODE='{company_code}'";
                var processountCodeList = _dbContext.SqlQuery<RoutineInputModel>(query).ToList();
                return processountCodeList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public List<ProcessSetupModel> GetProcessListByGroupCode(string groupId)
        {

            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                //string query = $@"SELECT DISTINCT 
                //        INITCAP(PROCESS_EDESC) AS PROCESS_EDESC,
                //        LOCATION_CODE,
                //        PROCESS_CODE ,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,
                //        PROCESS_CODE, PRE_PROCESS_CODE,PROCESS_FLAG
                //        FROM MP_PROCESS_SETUP
                //        WHERE DELETED_FLAG = 'N' 
                //        AND PROCESS_FLAG='R'
                //        AND PRE_PROCESS_CODE = '{groupId}'
                //        AND COMPANY_CODE='{company_code}'
                //        ORDER BY PROCESS_EDESC";
                //var processountCodeList = _dbContext.SqlQuery<ProcessSetupModel>(query).ToList();

                string query = $@"SELECT DISTINCT 
                        INITCAP(MPS.PROCESS_EDESC) AS PROCESS_EDESC,
                        ILS.LOCATION_EDESC  LOCATION_EDESC,
                        MPS.PROCESS_CODE PROCESS_CODE ,MPS.CREATED_BY CREATED_BY,MPS.CREATED_DATE CREATED_DATE,MPS.MODIFY_BY MODIFY_BY,MPS.MODIFY_DATE MODIFY_DATE,
                        MPS.PRE_PROCESS_CODE PRE_PROCESS_CODE,MPS.PROCESS_FLAG PROCESS_FLAG
                        FROM MP_PROCESS_SETUP MPS
                        INNER JOIN IP_LOCATION_SETUP ILS ON MPS.LOCATION_CODE=ILS.LOCATION_CODE
                        WHERE MPS.DELETED_FLAG = 'N' 
                        AND MPS.PRE_PROCESS_CODE = '{groupId}'
                        AND MPS.COMPANY_CODE='{company_code}'
                        ORDER BY PROCESS_EDESC";
                var processountCodeList = _dbContext.SqlQuery<ProcessSetupModel>(query).ToList();
                return processountCodeList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public List<ProcessSetupModel> GetAllProcessList(string searchText)
        {
            try
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    string query = $@"SELECT DISTINCT 
                        INITCAP(MPS.PROCESS_EDESC) AS PROCESS_EDESC,
                        ILS.LOCATION_EDESC  LOCATION_EDESC,
                        MPS.PROCESS_CODE PROCESS_CODE ,MPS.CREATED_BY CREATED_BY,MPS.CREATED_DATE CREATED_DATE,MPS.MODIFY_BY MODIFY_BY,MPS.MODIFY_DATE MODIFY_DATE,
                        MPS.PRE_PROCESS_CODE PRE_PROCESS_CODE,MPS.PROCESS_FLAG PROCESS_FLAG
                        FROM MP_PROCESS_SETUP MPS
                        INNER JOIN IP_LOCATION_SETUP ILS ON MPS.LOCATION_CODE=ILS.LOCATION_CODE
                        WHERE MPS.DELETED_FLAG = 'N' 
                        AND MPS.COMPANY_CODE='{company_code}'
                        AND UPPER(PROCESS_EDESC) LIKE UPPER('%{ searchText }%')
                        ORDER BY PROCESS_EDESC";
                    var processountCodeList = _dbContext.SqlQuery<ProcessSetupModel>(query).ToList();
                    return processountCodeList;
                }
                return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        #endregion
        #region Resource
        public string DeleteResourceSetupByResourceCode(string resourceCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(resourceCode)) { resourceCode = string.Empty; }
                var masterQry = $@"SELECT RESOURCE_CODE, GROUP_SKU_FLAG FROM MP_RESOURCE_SETUP WHERE RESOURCE_CODE = '{resourceCode}' and COMPANY_CODE= '{companyCode}'";
                var masterResourceCode = _dbContext.SqlQuery<ResourceSetupModel>(masterQry).FirstOrDefault();
                if (masterResourceCode.GROUP_SKU_FLAG == "G")
                {
                    var childQry = $@"SELECT COUNT(*) FROM MP_RESOURCE_SETUP WHERE PRE_RESOURCE_CODE like ('{masterResourceCode.RESOURCE_CODE}%') AND DELETED_FLAG = 'N'";
                    var childResult = _dbContext.SqlQuery<int>(childQry).FirstOrDefault();
                    if (childResult > 0)
                    {
                        return "HAS_CHILD";
                    }
                }
                var sqlquery = $@"UPDATE MP_RESOURCE_SETUP SET DELETED_FLAG = 'Y' WHERE RESOURCE_CODE='{resourceCode}' AND COMPANY_CODE ='{companyCode}' ";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                return "DELETED";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ResourceSetupModel GetResourceDataByResourceCode(string resourcecode)
        {
            try
            {
                if (string.IsNullOrEmpty(resourcecode)) { resourcecode = string.Empty; }
                string Query = $@"SELECT * FROM MP_RESOURCE_SETUP WHERE RESOURCE_CODE='{resourcecode}' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'   ORDER BY TO_NUMBER(RESOURCE_CODE) ASC";
                ResourceSetupModel entity = this._dbContext.SqlQuery<ResourceSetupModel>(Query).FirstOrDefault();

                string parentresourcequery = $@"SELECT RESOURCE_CODE FROM MP_RESOURCE_SETUP where RESOURCE_CODE = (SELECT PRE_RESOURCE_CODE FROM MP_RESOURCE_SETUP WHERE RESOURCE_CODE='{resourcecode}' 
                                AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}') and company_code='{_workContext.CurrentUserinformation.company_code}'";
                entity.PARENT_RESOURCE_CODE = this._dbContext.SqlQuery<string>(parentresourcequery).FirstOrDefault();

                return entity;

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public string createNewResourceSetup(ResourceSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var newmaxlocationcode = string.Empty;
                    var newmaxlocationcodequery = $@"SELECT COUNT(RESOURCE_CODE)+1 as MAX_LOCATION_CODE FROM MP_RESOURCE_SETUP WHERE PRE_RESOURCE_CODE = '{model.RESOURCE_CODE}'";
                    newmaxlocationcode = this._coreEntity.SqlQuery<int>(newmaxlocationcodequery).FirstOrDefault().ToString();
                    if (model.REMARKS != "")
                    {
                        model.REMARKS = model.REMARKS.Contains("'") ? model.REMARKS.Replace("'", "' || '''' || '") : model.REMARKS;
                    }
                    else
                    {
                        model.REMARKS = "";
                    }

                    if (model.RESOURCE_CODE != null && model.PRE_RESOURCE_CODE != "")
                    {
                        var maxPreCode = string.Empty;
                        var newprecode = string.Empty;
                        var newmastercode = string.Empty;

                        if (model.RESOURCE_CODE != null && model.RESOURCE_CODE != "")
                        {
                            var maxprecodequery = $@"select (count(*) + 1) as MAXCODE from MP_RESOURCE_SETUP where PRE_RESOURCE_CODE like '{model.RESOURCE_CODE}' and company_code = '{_workContext.CurrentUserinformation.company_code}'";
                            maxPreCode = this._coreEntity.SqlQuery<int>(maxprecodequery).FirstOrDefault().ToString();
                            if (maxPreCode != null)
                            {
                                if (Convert.ToInt32(maxPreCode) <= 9)
                                {
                                    maxPreCode = "0" + maxPreCode.ToString();
                                }

                            }

                            newprecode = model.PRE_RESOURCE_CODE;
                            newmastercode = model.PRE_RESOURCE_CODE + "." + maxPreCode;
                            var childsqlquery = $@"INSERT INTO MP_RESOURCE_SETUP (RESOURCE_CODE, RESOURCE_EDESC,RESOURCE_NDESC,PRE_RESOURCE_CODE,RESOURCE_TYPE,GROUP_SKU_FLAG,REMARKS,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG)
                                                    VALUES('{newmastercode}','{model.RESOURCE_EDESC}','{model.RESOURCE_NDESC}','{newprecode}','{model.RESOURCE_TYPE}','{model.GROUP_SKU_FLAG}','{model.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N')";
                            var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);

                        }

                    }
                    else
                    {
                        var newpre = string.Empty;
                        var newmaster = string.Empty;
                        var newprequery = $@"SELECT NVL(max(REGEXP_SUBSTR(RESOURCE_CODE, '[^.]+', 1, 1)),0)+1 col_one FROM MP_RESOURCE_SETUP";
                        newpre = this._coreEntity.SqlQuery<int>(newprequery).FirstOrDefault().ToString();
                        if (Convert.ToInt32(newpre) <= 9)
                        {
                            newpre = "0" + newpre.ToString();
                        }
                        if (model.GROUP_SKU_FLAG == "G")
                        {
                            newmaster = newpre + ".01";
                        }
                        var rootquery = $@"INSERT INTO MP_RESOURCE_SETUP (RESOURCE_CODE, RESOURCE_EDESC,RESOURCE_NDESC,PRE_RESOURCE_CODE,RESOURCE_TYPE,GROUP_SKU_FLAG,REMARKS,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG)
                                                    VALUES('{newmaster}','{model.RESOURCE_EDESC}','{model.RESOURCE_NDESC}','00','{model.RESOURCE_TYPE}','{model.GROUP_SKU_FLAG}','{model.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N')";
                        var insertroot = _coreEntity.ExecuteSqlCommand(rootquery);
                    }
                    trans.Commit();
                    return "INSERTED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        public string udpateResourceSetup(ResourceSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var companyCode = _workContext.CurrentUserinformation.company_code;

                    var sqlquery = $@"UPDATE MP_RESOURCE_SETUP SET RESOURCE_EDESC='{model.RESOURCE_EDESC}',RESOURCE_NDESC='{model.RESOURCE_NDESC}', RESOURCE_TYPE='{model.RESOURCE_TYPE}',REMARKS='{model.REMARKS}',MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',MODIFY_DATE=SYSDATE WHERE RESOURCE_CODE = '{model.RESOURCE_CODE}'";
                    var result = _coreEntity.ExecuteSqlCommand(sqlquery);
                    trans.Commit();
                    return "UPDATED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }

        //Kendo tree view for child grid


        public List<ResourceSetupModel> GetResourceListByGroupCode(string groupId)
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                string query = $@"SELECT DISTINCT 
                        INITCAP(RESOURCE_EDESC) AS RESOURCE_EDESC,
                        REMARKS,
                        RESOURCE_CODE ,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,
                        RESOURCE_CODE, PRE_RESOURCE_CODE,RESOURCE_FLAG,GROUP_SKU_FLAG
                        FROM MP_RESOURCE_SETUP
                        WHERE DELETED_FLAG = 'N' 
                        AND GROUP_SKU_FLAG ='I'
                        AND PRE_RESOURCE_CODE = '{groupId}'
                        --AND COMPANY_CODE='{company_code}'
                        ORDER BY RESOURCE_EDESC";
                var resourceountCodeList = _dbContext.SqlQuery<ResourceSetupModel>(query).ToList();
                return resourceountCodeList;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public List<ResourceSetupModel> GetAllResourceList(string searchText)
        {
            try
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    string query = $@" SELECT DISTINCT 
                        INITCAP(RESOURCE_EDESC) AS RESOURCE_EDESC,
                        REMARKS,
                        RESOURCE_CODE ,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,
                        RESOURCE_CODE, PRE_RESOURCE_CODE,RESOURCE_FLAG,GROUP_SKU_FLAG
                        FROM MP_RESOURCE_SETUP
                        WHERE DELETED_FLAG = 'N'
                        AND COMPANY_CODE='{company_code}'
                        AND UPPER(RESOURCE_EDESC) LIKE UPPER('%{ searchText }%')
                        ORDER BY RESOURCE_EDESC";
                    var resourceountCodeList = _dbContext.SqlQuery<ResourceSetupModel>(query).ToList();
                    return resourceountCodeList;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region Item
        public ItemSetupModel GetItemDataByItemCode(string ItemCode)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            try
            {

                ItemSetupModel entity = new ItemSetupModel();
                //if (this._cacheManager.IsSet($"GetItemDataByItemCode_{userid}_{company_code}_{branch_code}_{ItemCode}"))
                //{
                //    var data = _cacheManager.Get<ItemSetupModel>($"GetItemDataByItemCode_{userid}_{company_code}_{branch_code}_{ItemCode}");
                //    entity = data;
                //}
                //else
                //{

                if (string.IsNullOrEmpty(ItemCode)) { ItemCode = string.Empty; };
                string Query = $@"SELECT * FROM ip_item_master_setup where ITEM_CODE='{ItemCode}' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' ORDER BY TO_NUMBER(ITEM_CODE) ASC";
                entity = this._dbContext.SqlQuery<ItemSetupModel>(Query).FirstOrDefault();
                string spQuery = $@"SELECT * FROM IP_ITEM_SPEC_SETUP WHERE DELETED_FLAG='N' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' AND ITEM_CODE ='{ItemCode}'";
                var spResult = this._dbContext.SqlQuery<ItemSetupSpecModel>(spQuery).FirstOrDefault();
                if (spResult != null)
                { entity.specModel = spResult; }
                string muQuery = $@"SELECT * FROM IP_ITEM_UNIT_SETUP WHERE DELETED_FLAG='N' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' AND ITEM_CODE ='{ItemCode}'";
                var muResult = this._dbContext.SqlQuery<ItemMultiMuCodeModel>(muQuery).ToList();
                entity.multiMu = muResult;
                string assoQuery = $@"SELECT * FROM IP_INTEGRATION_SETUP WHERE DELETED_FLAG='N' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' AND ITEM_CODE ='{ItemCode}'";
                var assoResult = this._dbContext.SqlQuery<ItemSetupIntegrationModel>(assoQuery).ToList();
                entity.assoModel = assoResult;

                //string ItemQuery = $@"SELECT ITEM_CODE FROM ip_item_master_setup where MASTER_ITEM_CODE = (SELECT PRE_ITEM_CODE FROM ip_item_master_setup WHERE ITEM_CODE='{ItemCode}' 
                //            AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}')";

                string ItemQuery = $@"SELECT ITEM_CODE FROM ip_item_master_setup where MASTER_ITEM_CODE = (SELECT PRE_ITEM_CODE FROM ip_item_master_setup WHERE ITEM_CODE='{ItemCode}' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' and deleted_flag='N')  AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' and deleted_flag='N' and group_sku_flag='G'";
                entity.PARENT_ITEM_CODE = this._dbContext.SqlQuery<string>(ItemQuery).FirstOrDefault();

                string itemchargeQuery = $@"SELECT * FROM ip_item_charge_setup WHERE DELETED_FLAG='N' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' AND ITEM_CODE ='{ItemCode}'";
                var itemcharge = this._dbContext.SqlQuery<ItemChargesModel>(itemchargeQuery).ToList();
                entity.charges = itemcharge;
                //    this._cacheManager.Set($"GetItemDataByItemCode_{userid}_{company_code}_{branch_code}_{ItemCode}", entity, 20);
                //}
                return entity;





            }
            catch (Exception ex)
            {

                throw ex;
            }

        }


        public string GetMaxItemCode(string gFlag)
        {
            try
            {
                string Query = $@" select to_char(NVL(MAX(REGEXP_SUBSTR(TO_NUMBER(ITEM_CODE), '[^.]+', 1, 1)),0)+1) as MAX_ITEM_CODE from IP_ITEM_MASTER_SETUP  WHERE GROUP_SKU_FLAG='{gFlag}'";
                var max_item_code = this._dbContext.SqlQuery<string>(Query).FirstOrDefault();
                return max_item_code;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public string GetItemDescByItemCode(string masterid)
        {
            try
            {
                if (string.IsNullOrEmpty(masterid)) { masterid = string.Empty; };
                string Query = $@"select I.ITEM_EDESC as ITEM_EDESC from ip_item_master_setup I where master_item_code='{masterid}'";
                var ITEM_EDESC = this._dbContext.SqlQuery<string>(Query).FirstOrDefault();
                return ITEM_EDESC;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public string DeleteItemSetupByItemCode(string ItemCode)
        {
            var companyCode = _workContext.CurrentUserinformation.company_code;
            try
            {

                var sqlquery1 = $@"select count(*) from ip_item_master_setup where pre_item_code in (  select master_item_code from ip_item_master_setup where item_code = '{ItemCode}' AND COMPANY_CODE='{companyCode}') and DELETED_FLAG='N'";
                int count = _dbContext.SqlQuery<int>(sqlquery1).FirstOrDefault();
                if (count > 0)
                {
                    return "HAS_CHILD";
                }

                if (string.IsNullOrEmpty(ItemCode)) { ItemCode = string.Empty; }

                var sqlquery = $@"UPDATE ip_item_master_setup SET DELETED_FLAG = 'Y' WHERE ITEM_CODE='{ItemCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                return "DELETED";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #region item insert
        public string createNewItemSetup(ItemSetupModel model)
        {

            var charge_item_mu_code = model.INDEX_MU_CODE;
            if (model.PURCHASE_PRICE == null)
            {
                model.PURCHASE_PRICE = 0;
            }
            if (model.SALES_PRICE == null)
            {
                model.SALES_PRICE = 0;
            }
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var newmaxitemcode = string.Empty;
                    var newmaxGroupitemcodequery = $@"SELECT MAX(TO_NUMBER(ITEM_CODE))+1 as MAX_ITEM_CODE FROM IP_ITEM_MASTER_SETUP WHERE COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'";
                    //var newmaxGroupitemcodequery = $@"  select NVL(MAX(REGEXP_SUBSTR(TO_NUMBER(ITEM_CODE), '[^.]+', 1, 1)),0)+1 as MAX_ITEM_CODE from IP_ITEM_MASTER_SETUP";
                    newmaxitemcode = this._coreEntity.SqlQuery<int>(newmaxGroupitemcodequery).FirstOrDefault().ToString();
                    if (model.MASTER_ITEM_CODE != null && model.MASTER_ITEM_CODE != "")
                    {
                        var maxPreCode = string.Empty;
                        var newpreacccode = string.Empty;
                        var newmasteracccode = string.Empty;
                        if (model.GROUP_SKU_FLAG == "G")
                        {
                            if (model.MASTER_ITEM_CODE != null)
                            {
                                var maxprecodequery = $@"SELECT NVL(MAX(substr(MASTER_ITEM_CODE,-instr(reverse(MASTER_ITEM_CODE),'.')+1))+1,0) as MAXCODE FROM IP_ITEM_MASTER_SETUP 
                                                         WHERE PRE_ITEM_CODE ='{model.MASTER_ITEM_CODE}' and company_code = '{_workContext.CurrentUserinformation.company_code}'";
                                var maxCode = this._coreEntity.SqlQuery<int>(maxprecodequery).FirstOrDefault();
                                maxPreCode = maxCode == 0 ? "1" : maxCode.ToString();
                                if (Convert.ToInt32(maxPreCode) <= 9)
                                {
                                    maxPreCode = "0" + maxPreCode.ToString();
                                }
                                newpreacccode = model.MASTER_ITEM_CODE;
                                newmasteracccode = model.MASTER_ITEM_CODE + "." + maxPreCode;
                                #region Item insert query
                                insertItem(model, newmaxitemcode, newpreacccode, newmasteracccode);

                                #endregion
                                #region Item Integration insert query
                                insertIntegration(model, newmaxitemcode);

                                #endregion
                                #region Item Spec Insert Query
                                insertItemSpec(model, newmaxitemcode);

                                #endregion
                                #region Item Multi Mu insert query
                                insertMultiMu(model, newmaxitemcode);
                                #endregion
                                #region Item Charge insert query 
                                insertCharges(model, newmaxitemcode, charge_item_mu_code);
                                #endregion
                            }

                        }
                        else
                        {
                            newpreacccode = model.MASTER_ITEM_CODE;
                            newmasteracccode = model.MASTER_ITEM_CODE + "." + "00";

                            #region Item insert query
                            insertItem(model, newmaxitemcode, newpreacccode, newmasteracccode);
                            #endregion
                            #region Item Integration insert query
                            insertIntegration(model, newmaxitemcode);
                            #endregion
                            #region Item Multi Mu insert query
                            insertMultiMu(model, newmaxitemcode);
                            #endregion
                            #region Item Spec Insert Query
                            insertItemSpec(model, newmaxitemcode);
                            #endregion
                            #region Item Charge insert query 
                            insertCharges(model, newmaxitemcode, charge_item_mu_code);
                            #endregion
                        }

                    }
                    else
                    {
                        var newpre = string.Empty;
                        var newmaster = string.Empty;
                        var newprequery = $@"SELECT NVL(max(REGEXP_SUBSTR(MASTER_ITEM_CODE, '[^.]+', 1, 1)),0)+1 col_one FROM ip_item_master_setup";
                        newpre = this._coreEntity.SqlQuery<int>(newprequery).FirstOrDefault().ToString();
                        if (Convert.ToInt32(newpre) <= 9)
                        {
                            newpre = "0" + newpre.ToString();
                        }
                        if (model.GROUP_SKU_FLAG == "G")
                        {
                            newmaster = newpre + ".01";
                        }
                        else
                        {
                            newmaster = newpre + ".00";
                        }

                        #region Item insert query
                        insertItem(model, newmaxitemcode, "00", newmaster);

                        #endregion

                        #region Item Integration insert query
                        insertIntegration(model, newmaxitemcode);

                        #endregion

                        #region Item Spec Insert Query
                        insertItemSpec(model, newmaxitemcode);

                        #endregion

                        #region Item Multi Mu insert query
                        insertMultiMu(model, newmaxitemcode);
                        #endregion

                        #region Item Charge insert query 
                        insertCharges(model, newmaxitemcode, charge_item_mu_code);
                        #endregion
                    }
                    trans.Commit();
                    return "INSERTED";
                }
                catch (Exception ex)
                {

                    trans.Rollback();
                    throw ex;
                }
            }
        }
        private string insertItem(ItemSetupModel model, string newmaxitemcode, string newpre, string newmasteracccode)
        {
            try
            {
                var rootsqlquery = $@"INSERT INTO ip_item_master_setup
                                      (ITEM_CODE,ITEM_EDESC ,ITEM_NDESC ,CATEGORY_CODE,INDEX_MU_CODE ,           
                                      MASTER_ITEM_CODE,PRE_ITEM_CODE,COSTING_METHOD_FLAG,GROUP_SKU_FLAG,     
                                      LINK_SUB_CODE ,REMARKS ,COMPANY_CODE,CREATED_BY ,CREATED_DATE,             
                                      DELETED_FLAG ,SERVICE_ITEM_FLAG,PURCHASE_PRICE,SALES_PRICE ,   
                                      REMARKS2ND,CURRENT_STOCK, SYN_ROWID ,BATCH_FLAG  ,           
                                      MULTI_MU_CODE, SERIAL_FLAG ,BATCH_SERIAL_FLAG,MIN_VALUE,MAX_VALUE,REORDER_LEVEL,DANGER_LEVEL,ECO_ORDER_QUANTITY,MAX_LEVEL,MIN_LEVEL,MAX_USAGE,MIN_USAGE,NORMAL_USAGE,LEAD_TIME,DEFAULT_WIP_STOCK,PREFERRED_SUPPLIER_CODE,SHELF_LIFE_DAYS,IMAGE_FILE_NAME)  
                                      VALUES('{newmaxitemcode}','{model.ITEM_EDESC}','{model.ITEM_NDESC}','{model.CATEGORY_CODE}',
                                             '{model.INDEX_MU_CODE}','{newmasteracccode}','{newpre}','{model.COSTING_METHOD_FLAG}','{model.GROUP_SKU_FLAG}',
                                            '{model.LINK_SUB_CODE}','{model.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',
                                            SYSDATE,'N','{model.SERVICE_ITEM_FLAG}','{model.PURCHASE_PRICE}','{model.SALES_PRICE}',
                                            '{model.REMARKS2ND}','{model.CURRENT_STOCK}','{model.SYN_ROWID}','{model.BATCH_FLAG}','{model.MULTI_MU_CODE}','{model.SERIAL_FLAG}','{model.BATCH_SERIAL_FLAG}','{model.MIN_VALUE}','{model.MAX_VALUE}','{model.REORDER_LEVEL}','{model.DANGER_LEVEL}','{model.ECO_ORDER_QUANTITY}','{model.MAX_LEVEL}','{model.MIN_LEVEL}','{model.MAX_USAGE}','{model.MIN_USAGE}','{model.NORMAL_USAGE}','{model.LEAD_TIME}','{model.DEFAULT_WIP_STOCK}','{model.PREFERRED_SUPPLIER_CODE}','{model.SHELF_LIFE_DAYS}','{model.IMAGE_FILE_NAME}')";
                var insertroot = _coreEntity.ExecuteSqlCommand(rootsqlquery);
                return "SUCCESS";
            }
            catch (Exception ex)
            {
                _logErp.InfoInFile($@" Error in insertItem {model.ITEM_EDESC} Error {ex.Message}");

                return "FAILED";
            }

        }
        private string insertItemSpec(ItemSetupModel model, string newmaxitemcode)
        {
            try
            {
                if (model.specModel != null)
                {
                    if (model.specModel.PART_NUMBER != null || model.specModel.PART_NUMBER != "")
                    {


                        var itemSpecquery = $@"INSERT INTO IP_ITEM_SPEC_SETUP
                                      (ITEM_CODE, PART_NUMBER, BRAND_NAME ,ITEM_SPECIFICATION,            
                                      ITEM_APPLY_ON ,INTERFACE,TYPE, LAMINATION,ITEM_SIZE,THICKNESS ,           
                                      COLOR,GRADE,REMARKS,COMPANY_CODE,CREATED_BY ,CREATED_DATE,             
                                      DELETED_FLAG ,GSM ,SYN_ROWID)    
                                      VALUES('{newmaxitemcode}','{model.specModel.PART_NUMBER}',
                                             '{model.specModel.BRAND_NAME}','{model.specModel.ITEM_SPECIFICATION}',
                                             '{model.specModel.ITEM_APPLY_ON}','{model.specModel.INTERFACE}',
                                             '{model.specModel.TYPE}','{model.specModel.LAMINATION}','{model.specModel.ITEM_SIZE}',
                                            '{model.specModel.THICKNESS}','{model.specModel.COLOR}','{model.specModel.GRADE}','{model.specModel.REMARKS}','{_workContext.CurrentUserinformation.company_code}',
                                            '{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','{model.specModel.GSM}','{model.specModel.SYN_ROWID}')";
                        var insertispec = _coreEntity.ExecuteSqlCommand(itemSpecquery);
                    }
                }
                return "SUCCESS";

            }
            catch (Exception ex)
            {
                _logErp.InfoInFile($@" Error in insertItemSpec {model.ITEM_EDESC} Error {ex.Message}");
                return "FAILED";
            }

        }
        private string insertIntegration(ItemSetupModel model, string newmaxitemcode)
        {
            try
            {
                if (model.assoModel != null)
                {
                    foreach (var assoModel in model.assoModel)
                    {
                        if (assoModel.ACC_CODE != "" && assoModel.FORM_CODE != "")
                        {

                            var itemIntequery = $@"INSERT INTO IP_INTEGRATION_SETUP
                                      (ITEM_CODE, ACC_CODE, FORM_CODE, COMPANY_CODE,CREATED_BY ,CREATED_DATE,             
                                      DELETED_FLAG,INSERT_FLAG,SYN_ROWID)    
                                      VALUES('{newmaxitemcode}','{assoModel.ACC_CODE}',
                                             '{assoModel.FORM_CODE}','{_workContext.CurrentUserinformation.company_code}',
                                            '{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','{assoModel.INSERT_FLAG}','{assoModel.SYN_ROWID}')";
                            var insertinte = _coreEntity.ExecuteSqlCommand(itemIntequery);
                        }
                    }
                }

                return "SUCCESS";
            }
            catch (Exception ex)
            {
                _logErp.InfoInFile($@" Error in insertIntegration {model.ITEM_EDESC} Error {ex.Message}");
                return "FAILED";
            }
        }
        private string insertMultiMu(ItemSetupModel model, string newmaxitemcode)
        {
            try
            {
                foreach (var multiMu in model.multiMu)
                {
                    if (multiMu.ITEM_CODE != null || multiMu.ITEM_CODE != "" || multiMu.CONVERSION_FACTOR != null)
                    {

                        if (!string.IsNullOrEmpty(multiMu.MU_CODE))
                        {

                            var itemMulMuquery = $@"INSERT INTO IP_ITEM_UNIT_SETUP
                                      (ITEM_CODE, MU_CODE, CONVERSION_FACTOR,FRACTION,REMARKS,COMPANY_CODE,CREATED_BY ,CREATED_DATE,             
                                      DELETED_FLAG,SYN_ROWID,SERIAL_NO)    
                                      VALUES('{newmaxitemcode}','{multiMu.MU_CODE}','{multiMu.CONVERSION_FACTOR}','{multiMu.FRACTION}',
                                             '{multiMu.REMARKS}','{_workContext.CurrentUserinformation.company_code}',
                                            '{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','{multiMu.SYN_ROWID}','1')";
                            var insertMultiMu = _coreEntity.ExecuteSqlCommand(itemMulMuquery);
                        }
                    }
                }
                return "SUCCESS";
            }
            catch (Exception ex)
            {
                _logErp.InfoInFile($@" Error in insertIntegration {model.ITEM_EDESC} Error {ex.Message}");
                return "FAILED";
            }

        }
        private string insertCharges(ItemSetupModel model, string newmaxitemcode, string CHARGE_INDEX_UNIT)
        {
            var SERIAL_NO = 0;
            var COMPANY_CODE = _workContext.CurrentUserinformation.company_code;

            try
            {
                foreach (var item in model.charges)
                {

                    if (item.FORM_CODE != null || item.FORM_CODE != "")
                    {

                        ++SERIAL_NO;
                        int VALUE_PERCENT_AMOUNT = Convert.ToInt32(item.VALUE_PERCENT_AMOUNT);
                        var Itemchargesquery = $@"INSERT INTO IP_ITEM_CHARGE_SETUP(
                                          SERIAL_NO,             
                                          ITEM_CODE,          
                                          FORM_CODE,          
                                          CHARGE_CODE,         
                                          CHARGE_TYPE,        
                                          VALUE_QUANTITY_BASED,
                                          VALUE_PERCENT_FLAG,  
                                          VALUE_PERCENT_AMOUNT,
                                          CHARGE_INDEX_UNIT,   
                                          COMPANY_CODE,        
                                          CREATED_BY,          
                                          CREATED_DATE,        
                                          DELETED_FLAG,        
                                          ACC_CODE,            
                                          SUB_CODE,            
                                          IMPACT_ON,           
                                          APPLY_QUANTITY,      
                                          SYN_ROWID,           
                                          CHARGE_ACTIVE_FLAG)
                VALUES('{SERIAL_NO}','{newmaxitemcode}','{item.FORM_CODE}','{item.CHARGE_CODE}','{item.CHARGE_TYPE}','{item.VALUE_QUANTITY_BASED}','{item.VALUE_PERCENT_FLAG}',{VALUE_PERCENT_AMOUNT},
                '{CHARGE_INDEX_UNIT}','{COMPANY_CODE}','{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','{item.ACC_CODE}','{item.SUB_CODE}',
                '{item.IMPACT_ON}','{item.APPLY_QUANTITY}','{item.SYN_ROWID}','{item.CHARGE_ACTIVE_FLAG}')";
                        var insertcharge = _coreEntity.ExecuteSqlCommand(Itemchargesquery);
                    }
                }
                return "SUCCESS";
            }
            catch (Exception Ex)
            {
                _logErp.InfoInFile($@" Error in insertCharges {model.ITEM_EDESC} Error {Ex.Message}");
                return "FAILED";
            }
        }
        #endregion
        #region item update
        public string udpateItemSetup(ItemSetupModel model)
        {
            string CHARGE_INDEX_UNIT = model.INDEX_MU_CODE;
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    if (model.PURCHASE_PRICE == null)
                    {
                        model.PURCHASE_PRICE = 0;
                    }
                    if (model.SALES_PRICE == null)
                    {
                        model.SALES_PRICE = 0;
                    }
                    if (model.SHELF_LIFE_DAYS == null)
                    {
                        model.SHELF_LIFE_DAYS = 0;
                    }
                    if (model.REORDER_LEVEL == null)
                    {
                        model.REORDER_LEVEL = 0;
                    }
                    if (model.PREFERRED_LEVEL == null)
                    {
                        model.PREFERRED_LEVEL = 0;
                    }
                    if (model.NORMAL_USAGE == null)
                    {
                        model.NORMAL_USAGE = 0;
                    }
                    if (model.MIN_VALUE == null)
                    {
                        model.MIN_VALUE = 0;
                    }
                    if (model.MAX_VALUE == null)
                    {
                        model.MAX_VALUE = 0;
                    }
                    if (model.MAX_USAGE == null)
                    {
                        model.MAX_USAGE = 0;
                    }
                    if (model.MIN_USAGE == null)
                    {
                        model.MIN_USAGE = 0;
                    }
                    if (model.MIN_LEVEL == null)
                    {
                        model.MIN_LEVEL = 0;
                    }
                    if (model.MAX_LEVEL == null)
                    {
                        model.MAX_LEVEL = 0;
                    }
                    if (model.LEAD_TIME == null)
                    {
                        model.LEAD_TIME = 0;
                    }
                    if (model.FRACTION_VALUE == null)
                    {
                        model.FRACTION_VALUE = 0;
                    }
                    if (model.ECO_ORDER_QUANTITY == null)
                    {
                        model.ECO_ORDER_QUANTITY = 0;
                    }
                    if (model.DEFAULT_WIP_STOCK == null)
                    {
                        model.DEFAULT_WIP_STOCK = 0;
                    }
                    if (model.DANGER_LEVEL == null)
                    {
                        model.DANGER_LEVEL = 0;
                    }
                    if (model.CURRENT_STOCK == null)
                    {
                        model.CURRENT_STOCK = 0;
                    }
                    if (model.AVG_RATE == null)
                    {
                        model.AVG_RATE = 0;
                    }
                    String Query = $@"update ip_item_master_setup
                                 set  ITEM_EDESC='{model.ITEM_EDESC}', ITEM_NDESC='{model.ITEM_NDESC}',DIMENSION='{model.DIMENSION}',       
                                      CATEGORY_CODE='{model.CATEGORY_CODE}',INDEX_MU_CODE='{model.INDEX_MU_CODE}',FRACTION_VALUE={model.FRACTION_VALUE},        
                                      COSTING_METHOD_FLAG='{model.COSTING_METHOD_FLAG}',GROUP_SKU_FLAG='{model.GROUP_SKU_FLAG}',LINK_SUB_CODE='{model.LINK_SUB_CODE}' ,
                                      HS_CODE='{model.HS_CODE}',REMARKS='{model.REMARKS}',MODIFY_BY ='{_workContext.CurrentUserinformation.login_code}',         
                                      MODIFY_DATE=TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),DEFAULT_WIP_STOCK={model.DEFAULT_WIP_STOCK},         
                                      PURCHASE_PRICE={model.PURCHASE_PRICE},SALES_PRICE={model.SALES_PRICE},MULTI_MU_CODE='{model.MULTI_MU_CODE}',
                                      REORDER_LEVEL={model.REORDER_LEVEL},DANGER_LEVEL={model.DANGER_LEVEL},ECO_ORDER_QUANTITY={model.ECO_ORDER_QUANTITY},MAX_LEVEL={model.MAX_LEVEL} ,MIN_LEVEL={model.MIN_LEVEL},MAX_USAGE={model.MAX_USAGE},MIN_USAGE={model.MIN_USAGE},NORMAL_USAGE={model.NORMAL_USAGE},LEAD_TIME={model.LEAD_TIME} ,PREFERRED_SUPPLIER_CODE='{model.PREFERRED_SUPPLIER_CODE}',SHELF_LIFE_DAYS={model.SHELF_LIFE_DAYS},IMAGE_FILE_NAME='{model.IMAGE_FILE_NAME}',BATCH_FLAG='{model.BATCH_FLAG}',SERVICE_ITEM_FLAG='{model.SERVICE_ITEM_FLAG}',SERIAL_FLAG='{model.SERIAL_FLAG}',BATCH_SERIAL_FLAG='{model.BATCH_SERIAL_FLAG}'
                                      where  ITEM_CODE='{model.ITEM_CODE}'";
                    _coreEntity.ExecuteSqlCommand(Query);
                    updateItemSpec(model);
                    updateItemInte(model);
                    updateItemMu(model);
                    updateItemCharge(model, CHARGE_INDEX_UNIT);
                    trans.Commit();
                    return "UPDATED";

                }

                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }


        }
        public string updateItemSpec(ItemSetupModel model)
        {
            var Query = $@"DELETE FROM IP_ITEM_SPEC_SETUP WHERE ITEM_CODE='{model.ITEM_CODE}'";
            _coreEntity.ExecuteSqlCommand(Query);
            insertItemSpec(model, model.ITEM_CODE);
            return "UPDATED";
        }
        public string updateItemMu(ItemSetupModel model)
        {
            var Query = $@"DELETE FROM IP_ITEM_UNIT_SETUP WHERE ITEM_CODE='{model.ITEM_CODE}'";
            _coreEntity.ExecuteSqlCommand(Query);
            insertMultiMu(model, model.ITEM_CODE);
            return "UPDATED";
        }
        public string updateItemInte(ItemSetupModel model)
        {
            var Query = $@"DELETE FROM IP_INTEGRATION_SETUP WHERE ITEM_CODE='{model.ITEM_CODE}'";
            _coreEntity.ExecuteSqlCommand(Query);
            insertIntegration(model, model.ITEM_CODE);
            return "UPDATED";
        }
        public string updateItemCharge(ItemSetupModel model, string CHARGE_INDEX_UNIT)
        {
            var Query = $@"DELETE FROM ip_item_charge_setup WHERE ITEM_CODE='{model.ITEM_CODE}'";
            _coreEntity.ExecuteSqlCommand(Query);
            insertCharges(model, model.ITEM_CODE, CHARGE_INDEX_UNIT);
            return "UPDATED";
        }
        #endregion
        public List<ItemSetupModel> GetItemListByGroupCode(string groupId)
        {
            try
            {
                List<ItemSetupModel> ItemCodeList = new List<ItemSetupModel>();
                var company_code = _workContext.CurrentUserinformation.company_code;
                string query = $@" SELECT DISTINCT
                        INITCAP(A.ITEM_EDESC) AS ITEM_EDESC,
                        INITCAP(A.ITEM_NDESC)AS ITEM_NDESC,
                        A.ITEM_CODE AS ITEM_CODE,
                        A.MASTER_ITEM_CODE AS MASTER_ITEM_CODE,
                        A.PRE_ITEM_CODE AS PRE_ITEM_CODE,
                        A.GROUP_SKU_FLAG AS GROUP_SKU_FLAG,
                        A.CREATED_DATE AS CREATED_DATE,
                        A.CREATED_BY AS CREATED_BY,
                        A.MAX_VALUE as MAX_LEVEL,
                        A.MIN_VALUE as MIN_LEVEL,
                        A.REORDER_LEVEL,A.DANGER_LEVEL,A.ECO_ORDER_QUANTITY,A.MAX_LEVEL,
                        A.MIN_LEVEL,A.MAX_USAGE,A.MIN_USAGE,A.NORMAL_USAGE,
                        A.LEAD_TIME,A.DEFAULT_WIP_STOCK,A.PREFERRED_SUPPLIER_CODE,A.SHELF_LIFE_DAYS,
                        A.PURCHASE_PRICE AS PURCHASE_PRICE,
                        C.CATEGORY_EDESC AS CATEGORY_EDESC
                        FROM IP_ITEM_MASTER_SETUP A, IP_MU_CODE B, IP_CATEGORY_CODE C
                        WHERE A.DELETED_FLAG = 'N'
                        AND A.INDEX_MU_CODE=B.MU_CODE
                        AND A.CATEGORY_CODE(+)=C.CATEGORY_CODE 
                        AND A.GROUP_SKU_FLAG = 'I'
                        AND A.COMPANY_CODE='{company_code}'
                        AND A.PRE_ITEM_CODE = '{groupId}'
                        ORDER BY ITEM_EDESC";

                ItemCodeList = _dbContext.SqlQuery<ItemSetupModel>(query).ToList();
                return ItemCodeList;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public List<ItemSetupModel> GetAllItemList(string searchText)
        {
            try
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    List<ItemSetupModel> ItemCodeList = new List<ItemSetupModel>();
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    string query = $@" SELECT DISTINCT
                        INITCAP(A.ITEM_EDESC) AS ITEM_EDESC,
                        INITCAP(A.ITEM_NDESC)AS ITEM_NDESC,
                        A.ITEM_CODE AS ITEM_CODE,
                        A.MASTER_ITEM_CODE AS MASTER_ITEM_CODE,
                        A.PRE_ITEM_CODE AS PRE_ITEM_CODE,
                        A.GROUP_SKU_FLAG AS GROUP_SKU_FLAG,
                        A.CREATED_DATE AS CREATED_DATE,
                        A.CREATED_BY AS CREATED_BY,
                        A.MAX_VALUE as MAX_LEVEL,
                        A.MIN_VALUE as MIN_LEVEL,
                        A.REORDER_LEVEL,A.DANGER_LEVEL,A.ECO_ORDER_QUANTITY,A.MAX_LEVEL,
                        A.MIN_LEVEL,A.MAX_USAGE,A.MIN_USAGE,A.NORMAL_USAGE,
                        A.LEAD_TIME,A.DEFAULT_WIP_STOCK,A.PREFERRED_SUPPLIER_CODE,A.SHELF_LIFE_DAYS,
                        A.PURCHASE_PRICE AS PURCHASE_PRICE,
                        C.CATEGORY_EDESC AS CATEGORY_EDESC
                        FROM IP_ITEM_MASTER_SETUP A, IP_MU_CODE B, IP_CATEGORY_CODE C
                        WHERE A.DELETED_FLAG = 'N'
                        AND A.INDEX_MU_CODE=B.MU_CODE
                        AND A.CATEGORY_CODE(+)=C.CATEGORY_CODE 
                        AND A.COMPANY_CODE='{company_code}'
                        AND UPPER(ITEM_EDESC) LIKE UPPER('%{ searchText }%')
                        ORDER BY ITEM_EDESC";
                    ItemCodeList = _dbContext.SqlQuery<ItemSetupModel>(query).ToList();
                    return ItemCodeList;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region Supplier
        public SuplierSetupModel GetSupplierDataBysupplierCode(string SupplierCode)
        {

            try
            {
                if (string.IsNullOrEmpty(SupplierCode)) { SupplierCode = string.Empty; };
                string query = $@"select SUPPLIER_CODE,
                                  SUPPLIER_EDESC,
                                  SUPPLIER_NDESC,
                                  REGD_OFFICE_EADDRESS,
                                  REGD_OFFICE_NADDRESS,
                                  TEL_MOBILE_NO1,
                                  TEL_MOBILE_NO2,
                                  FAX_NO,
                                  EMAIL,
                                  PARTY_TYPE_CODE,
                                  LINK_SUB_CODE,
                                  REMARKS,
                                  ACTIVE_FLAG,
                                  GROUP_SKU_FLAG,
                                  MASTER_SUPPLIER_CODE,
                                  PRE_SUPPLIER_CODE,
                                  COMPANY_CODE,
                                  CREATED_BY,
                                  TO_CHAR(CREATED_DATE),
                                  DELETED_FLAG,
                                  TO_NUMBER(CREDIT_DAYS),
                                  CREDIT_ACTION_FLAG,
                                  ACC_CODE,
                                  PR_CODE,
                                  TPIN_VAT_NO,
                                  DELTA_FLAG,
                                  TO_NUMBER(CREDIT_LIMIT),
                                  BRANCH_CODE,
                                  TO_NUMBER(M_DAYS) 
                                  from IP_SUPPLIER_SETUP where SUPPLIER_CODE={SupplierCode}";


                SuplierSetupModel entity = this._dbContext.SqlQuery<SuplierSetupModel>(query).FirstOrDefault();



                return entity;

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public List<SuplierSetupModel> GetSupplyListByGroupCode(string groupId)
        {
            try
            {
                List<SuplierSetupModel> SupplierCodeList = new List<SuplierSetupModel>();
                var company_code = _workContext.CurrentUserinformation.company_code;


                string query = $@"SELECT DISTINCT  ISS.CREATED_DATE,
                        INITCAP(ISS.SUPPLIER_EDESC) AS SUPPLIER_EDESC,
                        INITCAP(ISS.SUPPLIER_NDESC) AS SUPPLIER_NDESC,
                        INITCAP(ISS.REGD_OFFICE_EADDRESS) AS REGD_OFFICE_EADDRESS,
                        ISS.TEL_MOBILE_NO1,
                        ISS.TEL_MOBILE_NO2,
                        ISS.FAX_NO,
                        ISS.EMAIL,
                        ISS.TPIN_VAT_NO,
                       
                        
                        ISS.SUPPLIER_CODE ,
                        ISS.PARTY_TYPE_CODE,
                        IPTC.PARTY_TYPE_EDESC,
                        ISS.MASTER_SUPPLIER_CODE, 
                        ISS.PRE_SUPPLIER_CODE,                    
                        ISS.GROUP_SKU_FLAG ,                      
                        ISS.CREATED_BY
                        FROM IP_SUPPLIER_SETUP ISS,IP_PARTY_TYPE_CODE IPTC
                        WHERE ISS.DELETED_FLAG = 'N' 
                        AND IPTC.PARTY_TYPE_CODE(+)=ISS.PARTY_TYPE_CODE
                        AND IPTC.COMPANY_CODE(+)=ISS.COMPANY_CODE
                        AND ISS.GROUP_SKU_FLAG = 'I'
                        AND ISS.COMPANY_CODE='{company_code}'
                        AND ISS.PRE_SUPPLIER_CODE = '{groupId}'
                        ORDER BY CREATED_DATE desc";

                SupplierCodeList = _dbContext.SqlQuery<SuplierSetupModel>(query).ToList();
                return SupplierCodeList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public List<SuplierSetupModel> GetAllSupplyList(string searchText)
        {
            try
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    List<SuplierSetupModel> SupplierCodeList = new List<SuplierSetupModel>();
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    string query = $@"SELECT DISTINCT 
                        INITCAP(ISS.SUPPLIER_EDESC) AS SUPPLIER_EDESC,
                        INITCAP(ISS.SUPPLIER_NDESC) AS SUPPLIER_NDESC,
                        INITCAP(ISS.REGD_OFFICE_EADDRESS) AS REGD_OFFICE_EADDRESS,
                        ISS.TEL_MOBILE_NO1,
                        ISS.TEL_MOBILE_NO2,
                        ISS.FAX_NO,
                        ISS.EMAIL,
                        ISS.TPIN_VAT_NO, 
                        ISS.SUPPLIER_CODE ,
                        ISS.PARTY_TYPE_CODE,
                        IPTC.PARTY_TYPE_EDESC,
                        ISS.MASTER_SUPPLIER_CODE, 
                        ISS.PRE_SUPPLIER_CODE,                    
                        ISS.GROUP_SKU_FLAG,                       
                        ISS.CREATED_BY
                        FROM IP_SUPPLIER_SETUP ISS,IP_PARTY_TYPE_CODE IPTC
                        WHERE ISS.DELETED_FLAG = 'N' 
                        AND IPTC.PARTY_TYPE_CODE(+)=ISS.PARTY_TYPE_CODE
                        AND IPTC.COMPANY_CODE(+)=ISS.COMPANY_CODE
                        AND ISS.COMPANY_CODE='{company_code}'
                        AND UPPER(ISS.SUPPLIER_EDESC) LIKE UPPER('%{ searchText }%')
                        ORDER BY SUPPLIER_EDESC";
                    SupplierCodeList = _dbContext.SqlQuery<SuplierSetupModel>(query).ToList();
                    return SupplierCodeList;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #region insert supplier setup
        public string createNewSupplierSetup(SuplierSetupModalSet model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var newmaxitemcode = string.Empty;
                    //var newmaxitemcodequery = $@"SELECT MAX(SUPPLIER_CODE)+1 as MASTER_SUPPLIER_CODE FROM IP_SUPPLIER_SETUP";
                    var newmaxitemcodequery = $@"select NVL(MAX(TO_NUMBER(REGEXP_SUBSTR(TO_NUMBER(SUPPLIER_CODE), '[^.]+', 1, 1))),0)+1 as MASTER_SUPPLIER_CODE from IP_SUPPLIER_SETUP  WHERE COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'";
                    newmaxitemcode = this._coreEntity.SqlQuery<int>(newmaxitemcodequery).FirstOrDefault().ToString();
                    if (model.suplierSetupModel.MASTER_SUPPLIER_CODE != null && model.suplierSetupModel.MASTER_SUPPLIER_CODE != "")
                    {
                        var maxPreCode = string.Empty;
                        var newpre = string.Empty;
                        var newmasteracccode = string.Empty;
                        if (model.suplierSetupModel.GROUP_SKU_FLAG == "G")
                        {

                            if (model.suplierSetupModel.MASTER_SUPPLIER_CODE != null)
                            {
                                var maxprecodequery = $@"SELECT NVL(MAX(substr(MASTER_SUPPLIER_CODE,-instr(reverse(MASTER_SUPPLIER_CODE),'.')+1))+1,0) as MAXCODE FROM IP_SUPPLIER_SETUP 
                                                         WHERE PRE_SUPPLIER_CODE like '{model.suplierSetupModel.MASTER_SUPPLIER_CODE}' and company_code = '{_workContext.CurrentUserinformation.company_code}'";


                                //var maxprecodequery = $@"select (MAX(substr(MASTER_SUPPLIER_CODE,-instr(reverse(MASTER_SUPPLIER_CODE),'.')+1))+1) as MAXCODE from IP_SUPPLIER_SETUP where PRE_SUPPLIER_CODE like '{model.suplierSetupModel.MASTER_SUPPLIER_CODE}' and company_code = '{_workContext.CurrentUserinformation.company_code}' AND GROUP_SKU_FLAG = 'G'";
                                //var maxprecodequery = $@"select (count(*) + 1) as MAXCODE from IP_SUPPLIER_SETUP where PRE_SUPPLIER_CODE like '{model.MASTER_SUPPLIER_CODE}' and company_code = '{_workContext.CurrentUserinformation.company_code}'";
                                var maxCode = this._coreEntity.SqlQuery<int>(maxprecodequery).FirstOrDefault();
                                maxPreCode = maxCode == 0 ? "1" : maxCode.ToString();


                                if (maxPreCode == null)
                                    maxPreCode = "1";
                                if (Convert.ToInt32(maxPreCode) <= 9)
                                {
                                    maxPreCode = "0" + maxPreCode.ToString();
                                }
                                newpre = model.suplierSetupModel.MASTER_SUPPLIER_CODE;
                                newmasteracccode = model.suplierSetupModel.MASTER_SUPPLIER_CODE + "." + maxPreCode;
                                #region  insert IP_SUPPLIER_SETUP query
                                insertSupplier(model, newmaxitemcode, newpre, newmasteracccode);

                                #endregion

                                #region  insert   ip_supplier_owner_info
                                insertSupplierOwnerInfo(model, newmaxitemcode);
                                #endregion
                                #region  insert   IP_SUPPLIER_SISTER_CONCERN
                                insertSupplierSisterConcern(model, newmaxitemcode);
                                #endregion
                                #region  insert    IP_SUPPLIER_BUDGETCENTER_INFO
                                insertSupplierBudgetCenterInfo(model, newmaxitemcode);
                                #endregion

                                #region  insert   IP_SUPPLIER_OPENING_SETUP
                                insertSupplierOpeningSetup(model, newmaxitemcode);
                                #endregion
                            }

                        }
                        else
                        {
                            newpre = model.suplierSetupModel.MASTER_SUPPLIER_CODE;
                            newmasteracccode = model.suplierSetupModel.MASTER_SUPPLIER_CODE + "." + "00";


                            #region  insert IP_SUPPLIER_SETUP query
                            insertSupplier(model, newmaxitemcode, newpre, newmasteracccode);

                            #endregion

                            #region  insert   ip_supplier_owner_info
                            insertSupplierOwnerInfo(model, newmaxitemcode);
                            #endregion
                            #region  insert   IP_SUPPLIER_SISTER_CONCERN
                            insertSupplierSisterConcern(model, newmaxitemcode);
                            #endregion
                            #region  insert    IP_SUPPLIER_BUDGETCENTER_INFO
                            insertSupplierBudgetCenterInfo(model, newmaxitemcode);
                            #endregion

                            #region  insert   IP_SUPPLIER_OPENING_SETUP
                            insertSupplierOpeningSetup(model, newmaxitemcode);
                            #endregion
                        }

                    }
                    else
                    {
                        var newpre = string.Empty;
                        var newmaster = string.Empty;
                        var newprequery = $@"SELECT  MAX(substr(MASTER_SUPPLIER_CODE,-instr(reverse(MASTER_SUPPLIER_CODE),'.')+1))+1 col_one FROM IP_SUPPLIER_SETUP WHERE COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'";


                        newpre = this._coreEntity.SqlQuery<int>(newprequery).FirstOrDefault().ToString();
                        if (Convert.ToInt32(newpre) <= 9)
                        {
                            newpre = "0" + newpre.ToString();
                        }
                        if (model.suplierSetupModel.GROUP_SKU_FLAG == "G")
                        {
                            newmaster = newpre + ".01";
                        }
                        else
                        {
                            newmaster = newpre;
                        }
                        //newpre = "00";
                        #region  insert IP_SUPPLIER_SETUP query
                        insertSupplier(model, newmaxitemcode, "00", newpre);

                        #endregion

                        #region  insert   ip_supplier_owner_info
                        insertSupplierOwnerInfo(model, newmaxitemcode);
                        #endregion
                        #region  insert   IP_SUPPLIER_SISTER_CONCERN
                        insertSupplierSisterConcern(model, newmaxitemcode);
                        #endregion
                        #region  insert    IP_SUPPLIER_BUDGETCENTER_INFO
                        insertSupplierBudgetCenterInfo(model, newmaxitemcode);
                        #endregion

                        #region  insert   IP_SUPPLIER_OPENING_SETUP
                        insertSupplierOpeningSetup(model, newmaxitemcode);
                        #endregion
                    }
                    trans.Commit();
                    return "INSERTED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        #endregion
        #region update supplier setup
        public string udpateSupplierSetup(SuplierSetupModalSet model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    updateSupplier(model);
                    updateSupplierOwnerInfo(model);
                    updateSupplierSisterConcern(model);
                    updateSupplierBudgetCenter(model);
                    updateSupplierOpeningSetup(model);
                    trans.Commit();
                    return "UPDATED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }


        }
        public string updateSupplier(SuplierSetupModalSet model)
        {

            try
            {
                if (model.suplierSetupModel.GROUP_START_NO == null)
                {
                    model.suplierSetupModel.GROUP_START_NO = 0;
                }
                if (model.suplierSetupModel.MATURITY_DATE == null)
                {
                    model.suplierSetupModel.MATURITY_DATE = DateTime.Now;
                }
                if (model.suplierSetupModel.INTEREST_RATE == null)
                {
                    model.suplierSetupModel.INTEREST_RATE = 0;
                }
                String UpdateQuery = $@"update IP_SUPPLIER_SETUP
                                 set  SUPPLIER_EDESC='{model.suplierSetupModel.SUPPLIER_EDESC}', SUPPLIER_NDESC='{model.suplierSetupModel.SUPPLIER_NDESC}'
                                     ,REGD_OFFICE_EADDRESS='{model.suplierSetupModel.REGD_OFFICE_EADDRESS}',REGD_OFFICE_NADDRESS='{model.suplierSetupModel.REGD_OFFICE_NADDRESS}'
                                     ,TEL_MOBILE_NO1='{model.suplierSetupModel.TEL_MOBILE_NO1}',TEL_MOBILE_NO2='{model.suplierSetupModel.TEL_MOBILE_NO2}',FAX_NO='{model.suplierSetupModel.FAX_NO}'
                                     ,EMAIL='{model.suplierSetupModel.EMAIL}',PARTY_TYPE_CODE='{model.suplierSetupModel.PARTY_TYPE_CODE}'
                                     ,LINK_SUB_CODE='{model.suplierSetupModel.LINK_SUB_CODE}',REMARKS='{model.suplierSetupModel.REMARKS}',ACTIVE_FLAG='{model.suplierSetupModel.ACTIVE_FLAG}'
                                     ,GROUP_SKU_FLAG='{model.suplierSetupModel.GROUP_SKU_FLAG}',MASTER_SUPPLIER_CODE='{model.suplierSetupModel.MASTER_SUPPLIER_CODE}',PRE_SUPPLIER_CODE='{model.suplierSetupModel.PRE_SUPPLIER_CODE}'
                                     ,COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}', CREATED_BY='{_workContext.CurrentUserinformation.login_code}'
                                     ,DELETED_FLAG='N', CREDIT_DAYS='{model.suplierSetupModel.CREDIT_DAYS}', CREDIT_ACTION_FLAG='{model.suplierSetupModel.CREDIT_ACTION_FLAG}',ACC_CODE='{model.suplierSetupModel.ACC_CODE}', PR_CODE='{model.suplierSetupModel.PR_CODE}'
                                     ,TPIN_VAT_NO='{model.suplierSetupModel.TPIN_VAT_NO}', DELTA_FLAG='{model.suplierSetupModel.DELTA_FLAG}', CREDIT_LIMIT='{model.suplierSetupModel.CREDIT_LIMIT}',BRANCH_CODE='{_workContext.CurrentUserinformation.branch_code}'
                                     ,M_DAYS='{model.suplierSetupModel.M_DAYS}',SUBSTITUTE_NAME='{model.suplierSetupModel.SUBSTITUTE_NAME}',EXCISE_NO='{model.suplierSetupModel.EXCISE_NO}',MATURITY_DATE=TO_DATE('{model.suplierSetupModel.MATURITY_DATE}', 'MM/dd/yyyy hh:mi:ss PM')
                                     ,IMAGE_FILE_NAME='{model.suplierSetupModel.IMAGE_FILE_NAME}',MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',MODIFY_DATE=SYSDATE, PREFIX_TEXT='{model.suplierSetupModel.PREFIX_TEXT}',GROUP_START_NO={model.suplierSetupModel.GROUP_START_NO}
                                     ,SUPPLIER_ID='{model.suplierSetupModel.SUPPLIER_ID}',APPROVED_FLAG='{model.suplierSetupModel.APPROVED_FLAG}',CASH_SUPPLIER_FLAG='{model.suplierSetupModel.CASH_SUPPLIER_FLAG}',INTEREST_RATE={model.suplierSetupModel.INTEREST_RATE},TIN='{model.suplierSetupModel.TIN}' where  SUPPLIER_CODE='{model.suplierSetupModel.SUPPLIER_CODE}'";
                var updatesuppliersetup = _coreEntity.ExecuteSqlCommand(UpdateQuery);

                return "UPDATED";
            }
            catch (Exception ex)
            {

                return "FAILED";
            }

        }
        public string updateSupplierOwnerInfo(SuplierSetupModalSet model)
        {
            try
            {
                var Query = $@"DELETE FROM   ip_supplier_owner_info WHERE SUPPLIER_CODE='{model.suplierSetupModel.SUPPLIER_CODE}'";
                _coreEntity.ExecuteSqlCommand(Query);

                foreach (var info in model.suplierSetupModel.suplierContactmodelList)
                {
                    var childsqlquery = $@"INSERT INTO ip_supplier_owner_info (SUPPLIER_CODE,OWNER_NAME,DESIGNATION,CONTACT_PERSON,ADDRESS,TEL_MOBILE_NO,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,MODIFY_DATE,MODIFY_BY)
                                VALUES('{model.suplierSetupModel.SUPPLIER_CODE}','{info.OWNER_NAME}','{info.DESIGNATION}','{info.CONTACT_PERSON}','{info.ADDRESS}','{info.TEL_MOBILE_NO}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{info.CREATED_DATE.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'{_workContext.CurrentUserinformation.login_code}')";
                    var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);
                }
                return "UPDATED";
            }
            catch (Exception ex)
            {

                return "FAILED";
            }
        }
        public string updateSupplierSisterConcern(SuplierSetupModalSet model)
        {
            try
            {
                var Query = $@"DELETE FROM   IP_SUPPLIER_SISTER_CONCERN WHERE SUPPLIER_CODE='{model.suplierSetupModel.SUPPLIER_CODE}'";
                _coreEntity.ExecuteSqlCommand(Query);

                foreach (var sister in model.suplierSetupModel.supplierSisterConcernmodelList)
                {
                    var childsqlquery = $@"INSERT INTO IP_SUPPLIER_SISTER_CONCERN (SUPPLIER_CODE,SISTER_CONCERN_EDESC,SISTER_CONCERN_NDESC,REMARKS,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,MODIFY_DATE,MODIFY_BY)
                                VALUES('{model.suplierSetupModel.SUPPLIER_CODE}','{sister.SISTER_CONCERN_EDESC}','{sister.SISTER_CONCERN_EDESC}','{sister.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{sister.CREATED_DATE.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'{_workContext.CurrentUserinformation.login_code}')";
                    var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);
                }
                return "UPDATED";
            }
            catch (Exception ex)
            {

                return "FAILED";
            }
        }
        public string updateSupplierBudgetCenter(SuplierSetupModalSet model)
        {
            try
            {
                var Query = $@"DELETE FROM IP_SUPPLIER_BUDGETCENTER_INFO WHERE SUPPLIER_CODE='{model.suplierSetupModel.SUPPLIER_CODE}'";
                _coreEntity.ExecuteSqlCommand(Query);

                foreach (var budget in model.suplierSetupModel.supplierBudgetCenterInfoList)
                {
                    var childsqlquery = $@"INSERT INTO IP_SUPPLIER_BUDGETCENTER_INFO (SUPPLIER_CODE,BUDGET_CODE,REMARKS,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,MODIFY_DATE,MODIFY_BY)
                                VALUES('{model.suplierSetupModel.SUPPLIER_CODE}','{budget.BUDGET_CODE}','{budget.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{budget.CREATED_DATE.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'{_workContext.CurrentUserinformation.login_code}')";
                    var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);
                }
                return "UPDATED";
            }
            catch (Exception ex)
            {

                return "FAILED";
            }
        }
        public string updateSupplierOpeningSetup(SuplierSetupModalSet model)
        {
            try
            {
                var Query = $@"DELETE FROM IP_SUPPLIER_OPENING_SETUP WHERE SUPPLIER_CODE='{model.suplierSetupModel.SUPPLIER_CODE}'";
                _coreEntity.ExecuteSqlCommand(Query);

                foreach (var ob in model.suplierSetupModel.supplierOpeningBalanceModelList)
                {
                    var childsqlquery = $@"INSERT INTO IP_SUPPLIER_OPENING_SETUP (SUPPLIER_CODE,REFERENCE_NO,INVOICE_DATE,BALANCE_AMOUNT,REMARKS,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,DUE_DATE,TRANSACTION_TYPE,BRANCH_CODE,ACC_CODE,MODIFY_BY,MODIFY_DATE)
                                VALUES('{model.suplierSetupModel.SUPPLIER_CODE}','{ob.REFERENCE_NO}',TO_DATE('{ob.INVOICE_DATE}','dd/MON/yyyy'),{ob.BALANCE_AMOUNT},'{ob.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{model.suplierSetupModel.CREATED_DATE.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N',TO_DATE('{ob.DUE_DATE}','MM/dd/yyyy hh:mi:ss AM'),'{ob.TRANSACTION_TYPE}','{_workContext.CurrentUserinformation.branch_code}','{ob.ACC_CODE}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'))";
                    var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);
                }
                return "UPDATED";
            }
            catch (Exception ex)
            {

                return "FAILED";
            }
        }
        #endregion
        public string DeleteSupplierSetupBySupplierCode(string suppliercode)
        {
            var companyCode = _workContext.CurrentUserinformation.company_code;

            try
            {
                var sqlquery1 = $@"select count(*) from IP_SUPPLIER_SETUP where PRE_SUPPLIER_CODE in (  select MASTER_SUPPLIER_CODE from IP_SUPPLIER_SETUP where SUPPLIER_CODE = '{suppliercode}' AND COMPANY_CODE='{companyCode}') AND DELETED_FLAG = 'N'";
                int count = _dbContext.SqlQuery<int>(sqlquery1).FirstOrDefault();
                if (count > 0)
                {
                    return "HAS_CHILD";
                }

                if (string.IsNullOrEmpty(suppliercode)) { suppliercode = string.Empty; }

                var sqlquery = $@"UPDATE IP_SUPPLIER_SETUP SET DELETED_FLAG = 'Y' WHERE SUPPLIER_CODE='{suppliercode}' AND COMPANY_CODE ='{companyCode}'";
                var result0 = _dbContext.ExecuteSqlCommand(sqlquery);
                var sqlsofquery = $@"UPDATE ip_supplier_owner_info SET DELETED_FLAG = 'Y' WHERE SUPPLIER_CODE='{suppliercode}' AND COMPANY_CODE ='{companyCode}'";
                var result1 = _dbContext.ExecuteSqlCommand(sqlsofquery);
                var sqlsscquery = $@"UPDATE     IP_SUPPLIER_BUDGETCENTER_INFO SET DELETED_FLAG = 'Y' WHERE SUPPLIER_CODE='{suppliercode}' AND COMPANY_CODE ='{companyCode}'";
                var result2 = _dbContext.ExecuteSqlCommand(sqlsscquery);
                var sqlsosquery = $@"UPDATE IP_SUPPLIER_OPENING_SETUP SET DELETED_FLAG = 'Y' WHERE SUPPLIER_CODE='{suppliercode}' AND COMPANY_CODE ='{companyCode}'";
                var result3 = _dbContext.ExecuteSqlCommand(sqlsosquery);

                return "DELETED";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string getNewSupplierCode()
        {
            var newmaxitemcodequery = $@"SELECT MAX(SUPPLIER_CODE)+1 as MAX_ITEM_CODE FROM IP_SUPPLIER_SETUP";
            var newmaxitemcode = this._dbContext.SqlQuery<int>(newmaxitemcodequery).FirstOrDefault().ToString();
            return newmaxitemcode;
        }
        public SuplierSetupModel GetSupplierDataBySupplierCode(string SupplierCode)
        {
            try
            {
                if (string.IsNullOrEmpty(SupplierCode)) { SupplierCode = string.Empty; };
                string Query = $@"SELECT * FROM IP_SUPPLIER_SETUP where SUPPLIER_CODE={SupplierCode} AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'   ORDER BY TO_NUMBER(SUPPLIER_CODE) ASC";
                SuplierSetupModel entity = this._dbContext.SqlQuery<SuplierSetupModel>(Query).FirstOrDefault();
                string soiQuery = $@"SELECT * FROM ip_supplier_owner_info WHERE DELETED_FLAG='N' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' AND SUPPLIER_CODE ='{SupplierCode}'";
                var spResult = this._dbContext.SqlQuery<SuplierContactmodel>(soiQuery).ToList();
                entity.suplierContactmodelList = spResult;
                string muQuery = $@"SELECT * FROM   IP_SUPPLIER_SISTER_CONCERN WHERE DELETED_FLAG='N' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' AND SUPPLIER_CODE ='{SupplierCode}'";
                var sscResult = this._dbContext.SqlQuery<SupplierSisterConcernmodel>(muQuery).ToList();
                entity.supplierSisterConcernmodelList = sscResult;
                string sbi = $@"SELECT * FROM IP_SUPPLIER_BUDGETCENTER_INFO WHERE DELETED_FLAG='N' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' AND SUPPLIER_CODE ='{SupplierCode}'";
                var assoResult = this._dbContext.SqlQuery<SupplierBudgetCenterInfo>(sbi).ToList();
                entity.supplierBudgetCenterInfoList = assoResult;
                string sbobq = $@"SELECT * FROM IP_SUPPLIER_OPENING_SETUP WHERE DELETED_FLAG='N' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' AND SUPPLIER_CODE ='{SupplierCode}'";
                var sbobqResult = this._dbContext.SqlQuery<SupplierOpeningBalanceModel>(sbobq).ToList();
                entity.supplierOpeningBalanceModelList = sbobqResult;
                var parent_supplier_code = GET_PARENT_SUPPLIER_CODE(SupplierCode);
                entity.PARENT_SUPPLIER_CODE = parent_supplier_code;
                return entity;

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        private string insertSupplier(SuplierSetupModalSet model, string newmaxsuppliercode, string newpre, string newmastersuppliercode)
        {

            if (model.suplierSetupModel.GROUP_START_NO == null)
            {
                model.suplierSetupModel.GROUP_START_NO = 0;
            }

            if (model.suplierSetupModel.INTEREST_RATE == null)
            {
                model.suplierSetupModel.INTEREST_RATE = 0;
            }


            try
            {

                var suppliersetupquery = $@"INSERT INTO IP_SUPPLIER_SETUP (SUPPLIER_CODE,
                                  SUPPLIER_EDESC,SUPPLIER_NDESC,REGD_OFFICE_EADDRESS,
                                  REGD_OFFICE_NADDRESS,
                                  TEL_MOBILE_NO1,
                                  TEL_MOBILE_NO2,
                                  FAX_NO,
                                  EMAIL,
                                  PARTY_TYPE_CODE,
                                  LINK_SUB_CODE,
                                  REMARKS,
                                  ACTIVE_FLAG,
                                  GROUP_SKU_FLAG,
                                  MASTER_SUPPLIER_CODE,
                                  PRE_SUPPLIER_CODE,
                                  COMPANY_CODE,
                                  CREATED_BY,
                                  CREATED_DATE,
                                  DELETED_FLAG,
                                  CREDIT_DAYS,
                                  CREDIT_ACTION_FLAG,
                                  ACC_CODE,
                                  PR_CODE,
                                  TPIN_VAT_NO,
                                  DELTA_FLAG,
                                  CREDIT_LIMIT,
                                  BRANCH_CODE,
                                  M_DAYS,
                                  SUBSTITUTE_NAME,
                                  EXCISE_NO,
                                  MATURITY_DATE,
                                  IMAGE_FILE_NAME,
                                  PREFIX_TEXT,
                                  GROUP_START_NO,SUPPLIER_ID,CASH_SUPPLIER_FLAG,APPROVED_FLAG,TIN,INTEREST_RATE)
                                VALUES('{newmaxsuppliercode}','{model.suplierSetupModel.SUPPLIER_EDESC}','{model.suplierSetupModel.SUPPLIER_NDESC}','{model.suplierSetupModel.REGD_OFFICE_EADDRESS}',
                                        '{model.suplierSetupModel.REGD_OFFICE_NADDRESS}','{model.suplierSetupModel.TEL_MOBILE_NO1}','{model.suplierSetupModel.TEL_MOBILE_NO2}','{model.suplierSetupModel.FAX_NO}',
                                        '{model.suplierSetupModel.EMAIL}','{model.suplierSetupModel.PARTY_TYPE_CODE}','{model.suplierSetupModel.LINK_SUB_CODE}','{model.suplierSetupModel.REMARKS}','{model.suplierSetupModel.ACTIVE_FLAG}','{model.suplierSetupModel.GROUP_SKU_FLAG}','{newmastersuppliercode}',
                                        '{newpre}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','0','{model.suplierSetupModel.CREDIT_ACTION_FLAG}',
                                        '{model.suplierSetupModel.ACC_CODE}','{model.suplierSetupModel.PR_CODE}','{model.suplierSetupModel.TPIN_VAT_NO}','N','0', {_workContext.CurrentUserinformation.branch_code},'{model.suplierSetupModel.M_DAYS}','{model.suplierSetupModel.SUBSTITUTE_NAME}',
                                        '{model.suplierSetupModel.EXCISE_NO}',TO_DATE('{model.suplierSetupModel.MATURITY_DATE}', 'MM/dd/yyyy hh:mi:ss PM'),'{model.suplierSetupModel.IMAGE_FILE_NAME}',
                                        '{model.suplierSetupModel.PREFIX_TEXT}',{model.suplierSetupModel.GROUP_START_NO},'{model.suplierSetupModel.SUPPLIER_ID}','{model.suplierSetupModel.CASH_SUPPLIER_FLAG}','{model.suplierSetupModel.APPROVED_FLAG}','{model.suplierSetupModel.TIN}',{model.suplierSetupModel.INTEREST_RATE})";

                var insertsuppliersetup = _coreEntity.ExecuteSqlCommand(suppliersetupquery);
                return "SUCCESS";
            }
            catch (Exception ex)
            {
                return "FAILED";
            }



        }
        private string insertSupplierOwnerInfo(SuplierSetupModalSet model, string newmaxsuppliercode)
        {

            try
            {
                foreach (var info in model.suplierSetupModel.suplierContactmodelList)
                {
                    var childsqlquery = $@"INSERT INTO ip_supplier_owner_info (SUPPLIER_CODE,OWNER_NAME,DESIGNATION,CONTACT_PERSON,ADDRESS,TEL_MOBILE_NO,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG)
                                VALUES('{newmaxsuppliercode}','{info.OWNER_NAME}','{info.DESIGNATION}','{info.CONTACT_PERSON}','{info.ADDRESS}','{info.TEL_MOBILE_NO}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N')";
                    var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);
                }
                return "SUCCESS";
            }
            catch (Exception ex)
            {
                return "FAILED";
            }



        }
        private string insertSupplierSisterConcern(SuplierSetupModalSet model, string newmaxsuppliercode)
        {

            try
            {
                foreach (var sister in model.suplierSetupModel.supplierSisterConcernmodelList)
                {
                    var childsqlquery = $@"INSERT INTO IP_SUPPLIER_SISTER_CONCERN (SUPPLIER_CODE,SISTER_CONCERN_EDESC,SISTER_CONCERN_NDESC,REMARKS,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG)
                                VALUES('{newmaxsuppliercode}','{sister.SISTER_CONCERN_EDESC}','{sister.SISTER_CONCERN_EDESC}','{sister.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N')";
                    var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);
                }
                return "SUCCESS";
            }
            catch (Exception ex)
            {
                return "FAILED";
            }



        }
        private string insertSupplierBudgetCenterInfo(SuplierSetupModalSet model, string newmaxsuppliercode)
        {

            try
            {
                foreach (var budget in model.suplierSetupModel.supplierBudgetCenterInfoList)
                {
                    var childsqlquery = $@"INSERT INTO IP_SUPPLIER_BUDGETCENTER_INFO (SUPPLIER_CODE,BUDGET_CODE,REMARKS,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG)
                                VALUES('{newmaxsuppliercode}','{budget.BUDGET_CODE}','{budget.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N')";
                    var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);
                }
                return "SUCCESS";
            }
            catch (Exception ex)
            {
                return "FAILED";
            }

        }
        private string insertSupplierOpeningSetup(SuplierSetupModalSet model, string newmaxsuppliercode)
        {

            try
            {
                foreach (var ob in model.suplierSetupModel.supplierOpeningBalanceModelList)
                {
                    var childsqlquery = $@"INSERT INTO IP_SUPPLIER_OPENING_SETUP (SUPPLIER_CODE,REFERENCE_NO,INVOICE_DATE,BALANCE_AMOUNT,REMARKS,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,DUE_DATE,TRANSACTION_TYPE,BRANCH_CODE,ACC_CODE)
                                VALUES('{newmaxsuppliercode}','{ob.REFERENCE_NO}',TO_DATE('{ob.INVOICE_DATE}','dd/MON/yyyy'),'{ob.BALANCE_AMOUNT}','{ob.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N',TO_DATE('{ob.DUE_DATE}','MM/dd/yyyy hh:mi:ss AM'),'{ob.TRANSACTION_TYPE}','{_workContext.CurrentUserinformation.branch_code}','{ob.ACC_CODE}')";
                    var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);
                }
                return "SUCCESS";
            }
            catch (Exception ex)
            {
                return "FAILED";
            }

        }
        private string GET_PARENT_SUPPLIER_CODE(string SUPPLIER_CODE)
        {
            string Query = $@"SELECT SUPPLIER_CODE AS PARENT_SUPPLIER_CODE FROM ip_supplier_setup where master_supplier_code = (SELECT pre_supplier_code FROM IP_SUPPLIER_SETUP where SUPPLIER_CODE = '{SUPPLIER_CODE}' AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}')";
            var SUPPLIERCODE = this._dbContext.SqlQuery<string>(Query).FirstOrDefault();
            return SUPPLIERCODE;
        }
        //public string GetSupplierCodeByPreSupplierCode(string precode)
        //{
        //    string Query = $@"SELECT SUPPLIER_CODE FROM IP_SUPPLIER_SETUP where MASTER_SUPPLIER_CODE='{precode}' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'";
        //    var SUPPLIERCODE = this._dbContext.SqlQuery<string>(Query).FirstOrDefault();
        //    return SUPPLIERCODE;
        //}
        #endregion
        #region Area
        public List<AreaModels> getAllAreaCodeDetail()
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                string Query = $@"SELECT * FROM AREA_SETUP WHERE DELETED_FLAG='N' AND COMPANY_CODE = '{company_code}'";
                var entity = this._dbContext.SqlQuery<AreaModels>(Query).ToList();
                return entity;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public string getMaxAreaCode()
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                var newmaxacccodequery = $@"SELECT NVL(MAX(TO_NUMBER(AREA_CODE))+1, 1) as MAX_AREA_CODE FROM AREA_SETUP";
                var newmaxacccode = this._dbContext.SqlQuery<int>(newmaxacccodequery).FirstOrDefault();
                return newmaxacccode.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string createNewAreaSetup(AreaModels model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var newmaxacccode = string.Empty;
                    var message = string.Empty;
                    if (model.AREA_CODE == "")
                    {
                        var newmaxacccodequery = $@"SELECT NVL(MAX(TO_NUMBER(AREA_CODE))+1, 1) as MAX_AREA_CODE FROM AREA_SETUP";
                        newmaxacccode = this._coreEntity.SqlQuery<int>(newmaxacccodequery).FirstOrDefault().ToString();
                    }
                    else
                    {
                        var existQry = $@"SELECT count(*) FROM AREA_SETUP WHERE AREA_CODE = '{model.AREA_CODE}'";
                        var isExist = this._coreEntity.SqlQuery<int>(existQry).FirstOrDefault();
                        if (isExist > 0)
                        {
                            return message = "This Code already exists";
                        }
                        newmaxacccode = model.AREA_CODE;
                    }

                    string Query = $@"INSERT INTO AREA_SETUP (AREA_CODE,AREA_EDESC,REMARKS, COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG) VALUES('{newmaxacccode}','{model.AREA_EDESC}','{model.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N')";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "INSERTED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        public string updateAreaSetup(AreaModels model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var message = string.Empty;
                    string Query = $@"UPDATE AREA_SETUP SET AREA_EDESC='{model.AREA_EDESC}', REMARKS='{model.REMARKS}', MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',MODIFY_DATE = SYSDATE  WHERE AREA_CODE = '{model.AREA_CODE}'";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "UPDATED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        public string deleteAreaSetup(string areaCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(areaCode)) { areaCode = string.Empty; }
                string message = string.Empty;
                var sqlquery = $@"UPDATE AREA_SETUP SET DELETED_FLAG='Y' WHERE AREA_CODE='{areaCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                if (result > 0)
                {
                    message = "DELETED";
                }
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region Agent
        public List<AgentModels> getAllAgentCodeDetail()
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                string Query = $@"SELECT * FROM AGENT_SETUP WHERE DELETED_FLAG='N' AND COMPANY_CODE = '{company_code}'";
                var entity = this._dbContext.SqlQuery<AgentModels>(Query).ToList();
                return entity;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public string getMaxAgentCode()
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                var newmaxacccodequery = $@"SELECT NVL(MAX(TO_NUMBER(regexp_replace(AGENT_CODE, '[^[:digit:]]', '')))+1, 1) as MAX_AGENT_CODE FROM AGENT_SETUP";
                var newmaxacccode = this._dbContext.SqlQuery<int>(newmaxacccodequery).FirstOrDefault();
                return newmaxacccode.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string createNewAgentSetup(AgentModels model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var newmaxacccode = string.Empty;
                    var message = string.Empty;
                    if (model.AGENT_CODE == "")
                    {
                        try
                        {
                            var newmaxacccodequery = $@"SELECT NVL(MAX(TO_NUMBER(AGENT_CODE))+1, 1) as MAX_AREA_CODE FROM AGENT_SETUP";
                            newmaxacccode = this._coreEntity.SqlQuery<int>(newmaxacccodequery).FirstOrDefault().ToString();
                        }
                        catch (Exception)
                        {
                            var newmaxacccodequery = $@"SELECT  NVL(MAX(TO_NUMBER(AGENT_CODE))+1, 1) as MAX_AREA_CODE FROM AGENT_SETUP WHERE REGEXP_LIKE(AGENT_CODE, '^[[:digit:]]$')";
                            newmaxacccode = this._coreEntity.SqlQuery<int>(newmaxacccodequery).FirstOrDefault().ToString();
                        }
                    }
                    else
                    {
                        var existQry = $@"SELECT count(*) FROM AGENT_SETUP WHERE AGENT_CODE = '{model.AGENT_CODE}'";
                        var isExist = this._coreEntity.SqlQuery<int>(existQry).FirstOrDefault();
                        if (isExist > 0)
                        {
                            return message = "This Code already exists";
                        }
                        newmaxacccode = model.AGENT_CODE;
                    }

                    string Query = $@"INSERT INTO AGENT_SETUP (AGENT_CODE,AGENT_EDESC,AGENT_ID,AGENT_TYPE,REMARKS,PAN_NO,CREDIT_LIMIT, CREDIT_DAYS, COMPANY_CODE, BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG)
                            VALUES('{newmaxacccode}','{model.AGENT_EDESC}','{model.AGENT_ID}','{model.AGENT_TYPE}','{model.REMARKS}','{model.PAN_NO}','{model.CREDIT_LIMIT}','{model.CREDIT_DAYS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N')";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "INSERTED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        public string updateAgentSetup(AgentModels model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var message = string.Empty;
                    string Query = $@"UPDATE AGENT_SETUP SET AGENT_EDESC='{model.AGENT_EDESC}',AGENT_ID='{model.AGENT_ID}',AGENT_TYPE='{model.AGENT_TYPE}',PAN_NO='{model.PAN_NO}',CREDIT_LIMIT='{model.CREDIT_LIMIT}',CREDIT_DAYS='{model.CREDIT_DAYS}', REMARKS='{model.REMARKS}', MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',MODIFY_DATE = SYSDATE  WHERE AGENT_CODE = '{model.AGENT_CODE}'";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "UPDATED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        public string deleteAgentSetup(string agentCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(agentCode)) { agentCode = string.Empty; }
                string message = string.Empty;
                var sqlquery = $@"UPDATE AGENT_SETUP SET DELETED_FLAG='Y' WHERE AGENT_CODE='{agentCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                if (result > 0)
                {
                    message = "DELETED";
                }
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region Transporter
        public List<TransporterModels> getAllTransporterCodeDetail()
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                string Query = $@"SELECT * FROM TRANSPORTER_SETUP WHERE DELETED_FLAG='N' AND COMPANY_CODE = '{company_code}'";
                var entity = this._dbContext.SqlQuery<TransporterModels>(Query).ToList();
                return entity;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public string getMaxTransporterCode()
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                var newmaxacccodequery = $@"SELECT LPAD(NVL(MAX(TO_NUMBER(TRANSPORTER_CODE))+1, 1),5,'0') as MAX_AREA_CODE FROM TRANSPORTER_SETUP";
                var newmaxacccode = this._dbContext.SqlQuery<string>(newmaxacccodequery).FirstOrDefault();
                return newmaxacccode.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string createNewTransporterSetup(TransporterModels model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var newmaxacccode = string.Empty;
                    var message = string.Empty;
                    if (model.TRANSPORTER_CODE == "")
                    {
                        var newmaxacccodequery = $@"SELECT NVL(MAX(TO_NUMBER(TRANSPORTER_CODE))+1, 1) as MAX_AREA_CODE FROM TRANSPORTER_SETUP";
                        newmaxacccode = this._coreEntity.SqlQuery<int>(newmaxacccodequery).FirstOrDefault().ToString();
                    }
                    else
                    {
                        var existQry = $@"SELECT count(*) FROM TRANSPORTER_SETUP WHERE TRANSPORTER_CODE = '{model.TRANSPORTER_CODE}'";
                        var isExist = this._coreEntity.SqlQuery<int>(existQry).FirstOrDefault();
                        if (isExist > 0)
                        {
                            return message = "This Code already exists";
                        }
                        newmaxacccode = model.TRANSPORTER_CODE;
                    }
                    string Query = $@"INSERT INTO TRANSPORTER_SETUP (TRANSPORTER_CODE,TRANSPORTER_EDESC,TRANSPORTER_NDESC, PAN_NO, DEPOSIT_AMOUNT, PROPRITER_NAME, PHONE_NO,ADDRESS, REMARKS,PRIORITY, COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG) 
                            VALUES(LPAD('{newmaxacccode}', 5, '0'),'{model.TRANSPORTER_EDESC}','{model.TRANSPORTER_NDESC}','{model.PAN_NO}','{model.DEPOSIT_AMOUNT}','{model.PROPRITER_NAME}','{model.PHONE_NO}','{model.ADDRESS}','{model.REMARKS}','{model.PRIORITY}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N')";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "INSERTED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        public string updateTransporterSetup(TransporterModels model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var message = string.Empty;
                    string Query = $@"UPDATE TRANSPORTER_SETUP SET TRANSPORTER_EDESC='{model.TRANSPORTER_EDESC}',TRANSPORTER_NDESC='{model.TRANSPORTER_NDESC}',PAN_NO='{model.PAN_NO}',DEPOSIT_AMOUNT='{model.DEPOSIT_AMOUNT}',
                                PROPRITER_NAME='{model.PROPRITER_NAME}',PHONE_NO='{model.PHONE_NO}',ADDRESS='{model.ADDRESS}', REMARKS='{model.REMARKS}',PRIORITY='{model.PRIORITY}', MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',MODIFY_DATE = SYSDATE  WHERE TRANSPORTER_CODE = '{model.TRANSPORTER_CODE}'";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "UPDATED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        public string deleteTransporterSetup(string transporterCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(transporterCode)) { transporterCode = string.Empty; }
                string message = string.Empty;
                var sqlquery = $@"UPDATE TRANSPORTER_SETUP SET DELETED_FLAG='Y' WHERE TRANSPORTER_CODE='{transporterCode}' AND COMPANY_CODE= '{companyCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                if (result > 0)
                {
                    message = "DELETED";
                }
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region Customer
        public List<CustomerSetupModel> GetAccountListByCustomerCode(string groupId, string wholeSearchText)
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                //string query = $@"SELECT DISTINCT 
                //       CS.CUSTOMER_CODE, INITCAP(CS.CUSTOMER_EDESC) AS CUSTOMER_EDESC,CS.REGD_OFFICE_EADDRESS,CS.TEL_MOBILE_NO1 as TELEPHONE,CS.TEL_MOBILE_NO2 as TELEPHONE2,CS.FAX_NO,CS.EMAIL,PC.PARTY_TYPE_EDESC,CAS.ACC_EDESC
                //        ,CS.CREATED_BY,CS.CREATED_DATE,CS.MODIFY_BY,CS.MODIFY_DATE,
                //        CS.MASTER_CUSTOMER_CODE, CS.PRE_CUSTOMER_CODE,CS.GROUP_SKU_FLAG
                //        FROM SA_CUSTOMER_SETUP CS,IP_PARTY_TYPE_CODE PC,FA_CHART_OF_ACCOUNTS_SETUP CAS
                //        WHERE
                //        CS.PARTY_TYPE_CODE=PC.PARTY_TYPE_CODE AND
                //         CS.ACC_CODE=CAS.ACC_CODE AND
                //        CS.COMPANY_CODE=PC.COMPANY_CODE AND
                //         CS.COMPANY_CODE=CAS.COMPANY_CODE AND
                //         CS.DELETED_FLAG = 'N' 
                //        AND CS.GROUP_SKU_FLAG='I'
                //        AND CS.PRE_CUSTOMER_CODE = '{groupId}'
                //        AND CS.COMPANY_CODE='{company_code}'
                //        ";
                string query = string.Empty;
                wholeSearchText = char.ToUpper(wholeSearchText[0]) + wholeSearchText.Substring(1);
                if (groupId == "undefined")
                {
                    query = $@"SELECT DISTINCT CS.CUSTOMER_CODE
                                ,INITCAP(CS.CUSTOMER_EDESC) AS CUSTOMER_EDESC
                                ,CS.REGD_OFFICE_EADDRESS
                                ,CS.TEL_MOBILE_NO1 AS TELEPHONE
                                ,CS.TEL_MOBILE_NO2 AS TELEPHONE2
                                ,CS.FAX_NO
                                ,CS.EMAIL
                                ,PC.PARTY_TYPE_EDESC
                                ,CAS.ACC_EDESC
                                ,CS.CREATED_BY
                                ,CS.CREATED_DATE
                                ,CS.MODIFY_BY
                                ,CS.MODIFY_DATE
                                ,CS.MASTER_CUSTOMER_CODE
                                ,CS.PRE_CUSTOMER_CODE
                                ,CS.GROUP_SKU_FLAG
                                FROM SA_CUSTOMER_SETUP CS
                                LEFT OUTER JOIN IP_PARTY_TYPE_CODE PC ON CS.PARTY_TYPE_CODE = PC.PARTY_TYPE_CODE
                                AND CS.COMPANY_CODE = PC.COMPANY_CODE
                                LEFT OUTER JOIN FA_CHART_OF_ACCOUNTS_SETUP CAS ON CS.ACC_CODE = CAS.ACC_CODE
                                AND CS.COMPANY_CODE = CAS.COMPANY_CODE
                                WHERE CS.DELETED_FLAG = 'N'
                                AND CS.GROUP_SKU_FLAG = 'I'
                                AND CS.COMPANY_CODE = '{company_code}'
                                AND CS.CUSTOMER_EDESC like '%{wholeSearchText}%'";
                }
                else
                {
                    query = $@"SELECT DISTINCT CS.CUSTOMER_CODE
                                ,INITCAP(CS.CUSTOMER_EDESC) AS CUSTOMER_EDESC
                                ,CS.REGD_OFFICE_EADDRESS
                                ,CS.TEL_MOBILE_NO1 AS TELEPHONE
                                ,CS.TEL_MOBILE_NO2 AS TELEPHONE2
                                ,CS.FAX_NO
                                ,CS.EMAIL
                                ,PC.PARTY_TYPE_EDESC
                                ,CAS.ACC_EDESC
                                ,CS.CREATED_BY
                                ,CS.CREATED_DATE
                                ,CS.MODIFY_BY
                                ,CS.MODIFY_DATE
                                ,CS.MASTER_CUSTOMER_CODE
                                ,CS.PRE_CUSTOMER_CODE
                                ,CS.GROUP_SKU_FLAG
                                FROM SA_CUSTOMER_SETUP CS
                                LEFT OUTER JOIN IP_PARTY_TYPE_CODE PC ON CS.PARTY_TYPE_CODE = PC.PARTY_TYPE_CODE
                                AND CS.COMPANY_CODE = PC.COMPANY_CODE
                                LEFT OUTER JOIN FA_CHART_OF_ACCOUNTS_SETUP CAS ON CS.ACC_CODE = CAS.ACC_CODE
                                AND CS.COMPANY_CODE = CAS.COMPANY_CODE
                                WHERE CS.DELETED_FLAG = 'N'
                                AND CS.GROUP_SKU_FLAG = 'I'
                                AND CS.PRE_CUSTOMER_CODE = '{groupId}'
                                AND CS.COMPANY_CODE = '{company_code}'";
                }

                var customerCodeList = _dbContext.SqlQuery<CustomerSetupModel>(query).ToList();
                return customerCodeList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public string createNewCustomerSetup(CustomerModels model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {

                    var newMaxCustomerCode = string.Empty;

                    if (model.CUSTOMER_CODE != null && model.PRE_CUSTOMER_CODE != "")
                    {
                        var newprecode = string.Empty;
                        var newMaxMasterCustomerCode = string.Empty;
                        var newmastercode = string.Empty;

                        if (model.CUSTOMER_CODE != null && model.CUSTOMER_CODE != "")
                        {
                            var newMaxCustomerCodeQuery = $@"SELECT nvl(max(to_number(CUSTOMER_CODE))+1,1) as MAX_CUSTOMER_CODE FROM SA_CUSTOMER_SETUP WHERE COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'";
                            newMaxCustomerCode = this._coreEntity.SqlQuery<int>(newMaxCustomerCodeQuery).FirstOrDefault().ToString();
                            var maxprecodequery = $@"SELECT NVL(MAX(substr(MASTER_CUSTOMER_CODE,-instr(reverse(MASTER_CUSTOMER_CODE),'.')+1)),0)+1 FROM SA_CUSTOMER_SETUP WHERE PRE_CUSTOMER_CODE = '{model.MASTER_CUSTOMER_CODE}' ";
                            var maxPreCode = this._coreEntity.SqlQuery<int>(maxprecodequery).FirstOrDefault().ToString();

                            if (maxPreCode != null)
                            {
                                if (Convert.ToInt32(maxPreCode) <= 9)
                                {
                                    maxPreCode = "0" + maxPreCode.ToString();
                                }

                            }
                            newmastercode = model.MASTER_CUSTOMER_CODE + "." + maxPreCode;
                            var childsqlquery = $@"INSERT INTO SA_CUSTOMER_SETUP (CUSTOMER_CODE,
                                                    CUSTOMER_EDESC,CUSTOMER_NDESC,PRE_CUSTOMER_CODE,ACC_CODE,
                                                    CUSTOMER_FLAG,PREFIX_TEXT,GROUP_START_NO,REMARKS,
                                                    MASTER_CUSTOMER_CODE,GROUP_SKU_FLAG,COMPANY_CODE,
                                                    CREATED_BY,CREATED_DATE,DELETED_FLAG,BRANCH_CODE)
                                                    VALUES('{newMaxCustomerCode}','{model.CUSTOMER_EDESC}','{model.CUSTOMER_NDESC}','{model.MASTER_CUSTOMER_CODE}','{model.CUSTOMER_ACCOUNT}','{model.CUSTOMER_TYPE}','{model.CUSTOMER_PREFIX}'
                                                        ,'{model.CUSTOMER_STARTID}','{model.REMARKS}','{newmastercode}','{model.GROUP_SKU_FLAG}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),
                                                         'N','{_workContext.CurrentUserinformation.branch_code}')";
                            var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);

                        }


                    }
                    else
                    {
                        var maxCustomerCode = string.Empty;
                        var maxC_code = string.Empty;
                        var newMaxCustomerCodeQuery1 = $@" SELECT nvl(max(to_number(CUSTOMER_CODE))+1,1)  as MAX_CUSTOMER_CODE FROM SA_CUSTOMER_SETUP WHERE COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'";
                        var newMaxCustomerCode1 = this._coreEntity.SqlQuery<int>(newMaxCustomerCodeQuery1).FirstOrDefault().ToString();
                        var maxMasterCustomerCode = string.Empty;
                        var newprequery = $@"SELECT  max(REGEXP_SUBSTR(MASTER_CUSTOMER_CODE, '[^.]+', 1, 1))+1 maxCustomerMasterCode  FROM SA_CUSTOMER_SETUP WHERE   COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'";
                        var newMasterAndCustomerCode = this._coreEntity.SqlQuery<CustomerModels>(newprequery).FirstOrDefault();



                        if (Convert.ToInt32(newMasterAndCustomerCode.maxCustomerCode) <= 9)
                        {
                            maxCustomerCode = "0" + newMasterAndCustomerCode.maxCustomerCode;

                        }
                        else
                        {
                            maxCustomerCode = newMasterAndCustomerCode.maxCustomerCode.ToString();
                        }
                        if (Convert.ToInt32(newMasterAndCustomerCode.maxCustomerMasterCode) <= 9)
                        {
                            maxMasterCustomerCode = "0" + newMasterAndCustomerCode.maxCustomerMasterCode;
                        }

                        else
                        {
                            maxMasterCustomerCode = newMasterAndCustomerCode.maxCustomerMasterCode.ToString();

                        }

                        var childsqlquery = $@"INSERT INTO SA_CUSTOMER_SETUP (CUSTOMER_CODE,
                                                    CUSTOMER_EDESC,CUSTOMER_NDESC,PRE_CUSTOMER_CODE,ACC_CODE,
                                                    CUSTOMER_FLAG,PREFIX_TEXT,GROUP_START_NO,REMARKS,
                                                    MASTER_CUSTOMER_CODE,GROUP_SKU_FLAG,COMPANY_CODE,
                                                    CREATED_BY,CREATED_DATE,DELETED_FLAG,BRANCH_CODE)
                                                    VALUES('{newMaxCustomerCode1}','{model.CUSTOMER_EDESC}','{model.CUSTOMER_NDESC}','00','{model.CUSTOMER_ACCOUNT}','{model.CUSTOMER_TYPE}','{model.CUSTOMER_PREFIX}'
                                                        ,'{model.CUSTOMER_STARTID}','{model.REMARKS}','{maxMasterCustomerCode}','{model.GROUP_SKU_FLAG}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),
                                                         'N','{_workContext.CurrentUserinformation.branch_code}')";
                        var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);
                    }
                    trans.Commit();
                    return "INSERTED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }



        }
        public string updateCustomerSetup(CustomerModels model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var companyCode = _workContext.CurrentUserinformation.company_code;

                    var sqlquery = $@"UPDATE SA_CUSTOMER_SETUP SET CUSTOMER_EDESC='{model.CUSTOMER_EDESC}', CUSTOMER_NDESC='{model.CUSTOMER_NDESC}', ACC_CODE='{model.CUSTOMER_ACCOUNT}',CUSTOMER_FLAG='{model.CUSTOMER_TYPE}',PREFIX_TEXT='{model.CUSTOMER_PREFIX}',GROUP_START_NO='{model.CUSTOMER_STARTID}',REMARKS='{model.REMARKS}' WHERE CUSTOMER_CODE= '{model.CUSTOMER_CODE}'";
                    var result = _coreEntity.ExecuteSqlCommand(sqlquery);
                    trans.Commit();
                    return "UPDATED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        public string DeleteCustomerTreeByCustCode(string custCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(custCode)) { custCode = string.Empty; }
                var masterQry = $@"SELECT MASTER_CUSTOMER_CODE,GROUP_SKU_FLAG FROM SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{custCode}' and COMPANY_CODE= '{companyCode}'";
                var masterAccCode = _dbContext.SqlQuery<CustomerModels>(masterQry).FirstOrDefault();
                if (masterAccCode.GROUP_SKU_FLAG == "G")
                {
                    var childQry = $@"SELECT COUNT(*) FROM SA_CUSTOMER_SETUP WHERE PRE_CUSTOMER_CODE like ('{masterAccCode.MASTER_CUSTOMER_CODE}%') AND DELETED_FLAG = 'N'";
                    var childResult = _dbContext.SqlQuery<int>(childQry).FirstOrDefault();
                    if (childResult > 0)
                    {
                        return "HAS_CHILD";
                    }
                }
                var sqlquery = $@"UPDATE SA_CUSTOMER_SETUP SET DELETED_FLAG = 'Y' WHERE CUSTOMER_CODE='{custCode}' AND COMPANY_CODE ='{companyCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                return "DELETED";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int? GetMaxCustomer()
        {
            var Query = $@"SELECT COUNT(CUSTOMER_CODE) +1  FROM SA_CUSTOMER_SETUP WHERE  COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'";
            var maxCustomerResult = _dbContext.SqlQuery<int?>(Query).FirstOrDefault();
            return maxCustomerResult;
        }
        public int? GetMaxChildCustomer()
        {
            var Query = $@"SELECT nvl(max(to_number(CUSTOMER_CODE))+1,1) FROM SA_CUSTOMER_SETUP WHERE  COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'";
            var maxCustomerResult = _dbContext.SqlQuery<int?>(Query).FirstOrDefault();
            return maxCustomerResult;
        }
        public List<CountryModels> getAllCountry(string filter)
        {
            try
            {
                if (string.IsNullOrEmpty(filter))
                {
                    filter = string.Empty;

                }

                var countryQuery = $@"select distinct
                             COALESCE(COUNTRY_CODE,' ') COUNTRY_CODE
                            ,COALESCE(COUNTRY_EDESC,' ') COUNTRY_EDESC
                            FROM COUNTRY_SETUP 
                            WHERE (COUNTRY_CODE like '%{filter.ToUpperInvariant()}%' 
                            or upper(COUNTRY_EDESC) like '%{filter.ToUpperInvariant()}%')";
                var result = _dbContext.SqlQuery<CountryModels>(countryQuery).ToList();
                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }
        public List<LocationModels> getAllLocation(string filter)
        {
            try
            {
                if (string.IsNullOrEmpty(filter))
                {
                    filter = string.Empty;

                }

                var countryQuery = $@"select distinct
                             COALESCE(LOCATION_CODE,' ') LOCATION_CODE
                            ,COALESCE(LOCATION_EDESC,' ') LOCATION_EDESC
                            FROM IP_LOCATION_SETUP 
                            WHERE DELETED_FLAG = 'N' AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}' AND (LOCATION_CODE like '%{filter.ToUpperInvariant()}%' 
                            or upper(LOCATION_EDESC) like '%{filter.ToUpperInvariant()}%') ";
                var result = _dbContext.SqlQuery<LocationModels>(countryQuery).ToList();
                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }
        public List<ZoneModels> getAllZones(string filter)
        {
            try
            {
                if (string.IsNullOrEmpty(filter))
                {
                    filter = string.Empty;

                }

                var zoneQuery = $@"select distinct
                             COALESCE(ZONE_CODE,' ') ZONE_CODE
                            ,COALESCE(ZONE_EDESC,' ') ZONE_EDESC
                            FROM ZONE_CODE 
                            WHERE (ZONE_CODE like '%{filter.ToUpperInvariant()}%' 
                            or upper(ZONE_EDESC) like '%{filter.ToUpperInvariant()}%')";
                var result = _dbContext.SqlQuery<ZoneModels>(zoneQuery).ToList();
                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }
        public List<RegionalModels> getAllRegions(string filter)
        {
            try
            {
                if (string.IsNullOrEmpty(filter))
                {
                    filter = string.Empty;

                }

                var regionQuery = $@"select distinct
                             COALESCE(REGION_CODE,' ') REGION_CODE
                            ,COALESCE(REGION_EDESC,' ') REGION_EDESC
                            FROM REGION_CODE 
                            WHERE (REGION_CODE like '%{filter.ToUpperInvariant()}%' 
                            or upper(REGION_EDESC) like '%{filter.ToUpperInvariant()}%')";
                var result = _dbContext.SqlQuery<RegionalModels>(regionQuery).ToList();
                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }
        public List<DistrictModels> getAllDistricts(string filter)
        {
            try
            {
                if (string.IsNullOrEmpty(filter))
                {
                    filter = string.Empty;

                }

                var districtQuery = $@"select distinct
                             COALESCE(DISTRICT_CODE,' ') DISTRICT_CODE
                            ,COALESCE(DISTRICT_EDESC,' ') DISTRICT_EDESC
                            FROM DISTRICT_CODE 
                            WHERE (DISTRICT_CODE like '%{filter.ToUpperInvariant()}%' 
                            or upper(DISTRICT_EDESC) like '%{filter.ToUpperInvariant()}%')";
                var result = _dbContext.SqlQuery<DistrictModels>(districtQuery).ToList();
                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }
        public List<CityModels> getAllCities(string filter)
        {
            try
            {
                if (string.IsNullOrEmpty(filter))
                {
                    filter = string.Empty;

                }

                var cityQuery = $@"select distinct
                             COALESCE(CITY_CODE,' ') CITY_CODE
                            ,COALESCE(CITY_EDESC,' ') CITY_EDESC
                            FROM CITY_CODE 
                            WHERE (CITY_CODE like '%{filter.ToUpperInvariant()}%' 
                            or upper(CITY_EDESC) like '%{filter.ToUpperInvariant()}%')";
                var result = _dbContext.SqlQuery<CityModels>(cityQuery).ToList();
                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }
        public List<PartyTypeModels> getAllPartyTypes(string filter)
        {
            try
            {
                if (string.IsNullOrEmpty(filter))
                {
                    filter = string.Empty;

                }

                var partyTypeQuery = $@"select COALESCE(PARTY_TYPE_CODE,' ') PARTY_TYPE_CODE
                            ,COALESCE(PARTY_TYPE_EDESC,' ') PARTY_TYPE_EDESC,ACC_CODE
                            FROM IP_PARTY_TYPE_CODE 
                            WHERE DELETED_FLAG = 'N' AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}' AND (PARTY_TYPE_CODE like '%{filter.ToUpperInvariant()}%' 
                            or upper(PARTY_TYPE_EDESC) like '%{filter.ToUpperInvariant()}%')";
                var result = _dbContext.SqlQuery<PartyTypeModels>(partyTypeQuery).ToList();
                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }
        public List<AgentModels> getAllAgents(string filter)
        {
            try
            {
                if (string.IsNullOrEmpty(filter))
                {
                    filter = string.Empty;

                }

                var partyTypeQuery = $@"select COALESCE(AGENT_CODE,' ') AGENT_CODE
                            ,COALESCE(AGENT_EDESC,' ') AGENT_EDESC
                            FROM AGENT_SETUP 
                            WHERE DELETED_FLAG = 'N' AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}' AND (AGENT_CODE like '%{filter.ToUpperInvariant()}%' 
                            or upper(AGENT_EDESC) like '%{filter.ToUpperInvariant()}%')";
                var result = _dbContext.SqlQuery<AgentModels>(partyTypeQuery).ToList();
                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }
        public List<BranchModels> getAllBranchs(string filter)
        {
            try
            {
                if (string.IsNullOrEmpty(filter))
                {
                    filter = string.Empty;

                }

                var partyTypeQuery = $@"select COALESCE(BRANCH_CODE,' ') BRANCH_CODE
                            ,COALESCE(BRANCH_EDESC,' ') BRANCH_EDESC
                            FROM FA_BRANCH_SETUP 
                            WHERE DELETED_FLAG = 'N' AND GROUP_SKU_FLAG = 'I' AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}' AND (BRANCH_CODE like '%{filter.ToUpperInvariant()}%' 
                            or upper(BRANCH_EDESC) like '%{filter.ToUpperInvariant()}%')";
                var result = _dbContext.SqlQuery<BranchModels>(partyTypeQuery).ToList();
                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }

        //Get price list data
        public List<MasterFieldForUpdate> getAllPricelist(string filter)
        {
            try
            {
                if (string.IsNullOrEmpty(filter))
                {
                    filter = string.Empty;

                }

                var partyTypeQuery = $@"select  MASTER_ID
                            , PRICE_LIST_NAME
                            FROM PRICE_SETUP_MASTER 
                            WHERE STATUS = '1'  AND (MASTER_ID like '%{filter.ToUpperInvariant()}%' 
                            or upper(PRICE_LIST_NAME) like '%{filter.ToUpperInvariant()}%')";
                var result = _dbContext.SqlQuery<MasterFieldForUpdate>(partyTypeQuery).ToList();
                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public List<AccTypeModels> getAllAccountMaps(string filter)
        {
            try
            {
                if (string.IsNullOrEmpty(filter))
                {
                    filter = string.Empty;

                }

                var partyTypeQuery = $@"select COALESCE(ACC_CODE,' ') ACC_CODE
                            ,COALESCE(ACC_EDESC,' ') ACC_EDESC
                            FROM FA_CHART_OF_ACCOUNTS_SETUP 
                            WHERE DELETED_FLAG = 'N' AND ACC_TYPE_FLAG = 'T' AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}' AND (ACC_CODE like '%{filter.ToUpperInvariant()}%' 
                            or upper(ACC_EDESC) like '%{filter.ToUpperInvariant()}%')";
                var result = _dbContext.SqlQuery<AccTypeModels>(partyTypeQuery).ToList();
                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }
        public List<DivisionModels> getAllDivisions(string filter)
        {
            try
            {
                if (string.IsNullOrEmpty(filter))
                {
                    filter = string.Empty;

                }

                var partyTypeQuery = $@"select COALESCE(DIVISION_CODE,' ') DIVISION_CODE
                            ,COALESCE(DIVISION_EDESC,' ') DIVISION_EDESC
                            FROM FA_DIVISION_SETUP 
                            WHERE DELETED_FLAG = 'N'  AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}' AND (DIVISION_CODE like '%{filter.ToUpperInvariant()}%' 
                            or upper(DIVISION_EDESC) like '%{filter.ToUpperInvariant()}%')";
                var result = _dbContext.SqlQuery<DivisionModels>(partyTypeQuery).ToList();
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<DivisionModels> getAllComboDivisions(string filter)
        {
            try
            {
                if (string.IsNullOrEmpty(filter))
                {
                    filter = string.Empty;

                }

                var partyTypeQuery = $@"select COALESCE(DIVISION_CODE,' ') DIVISION_CODE
                            ,COALESCE(DIVISION_EDESC,' ') DIVISION_EDESC
                            FROM FA_DIVISION_SETUP 
                            WHERE DELETED_FLAG = 'N'  AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}' AND (DIVISION_CODE like '%{filter.ToUpperInvariant()}%' 
                            or upper(DIVISION_EDESC) like '%{filter.ToUpperInvariant()}%')";
                var result = _dbContext.SqlQuery<DivisionModels>(partyTypeQuery).ToList();
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public string createNewChildCustomerSetup(CustomerModels model)
        {
            var data = string.Empty;
            _logErp.WarnInDB("method for creating child customer started by : " + _workContext.CurrentUserinformation.UserName);
            if (model.ACTIVE_FLAG == "True")
            {
                model.ACTIVE_FLAG = "Y";
            }
            else
            {
                model.ACTIVE_FLAG = "N";
            }

            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {

                    if (model.CHILD_UPDATE_CUSTOMER_CODE != null && model.CHILD_UPDATE_CUSTOMER_CODE != "")
                    {
                        model.AfterSaveCustomerCode = model.CHILD_UPDATE_CUSTOMER_CODE;
                        UpdateCustomerTable(model, model.CHILD_UPDATE_CUSTOMER_CODE);
                        if (model.ownerInfo.Count > 0)
                        {
                            var deleteOwnerInfoQuery = $@"DELETE FROM  SA_CUSTOMER_OWNER_INFO WHERE CUSTOMER_CODE = '{model.CHILD_UPDATE_CUSTOMER_CODE}'";
                            var insertchild = _coreEntity.ExecuteSqlCommand(deleteOwnerInfoQuery);
                        }
                        if (model.customerInvoiceWiseOpening.Count > 0)
                        {
                            var deleteCustomerInvoiceQuery = $@"DELETE FROM SA_CUSTOMER_OPENING_SETUP WHERE CUSTOMER_CODE = '{model.CHILD_UPDATE_CUSTOMER_CODE}'";
                            var insertchild = _coreEntity.ExecuteSqlCommand(deleteCustomerInvoiceQuery);
                        }
                        if (model.customerDivision.Count > 0)
                        {
                            var deleteDivisionQuery = $@"DELETE FROM SA_CUSTOMER_DIVISION_CR_LIMIT WHERE CUSTOMER_CODE = '{model.CHILD_UPDATE_CUSTOMER_CODE}'";
                            var insertchild = _coreEntity.ExecuteSqlCommand(deleteDivisionQuery);
                        }
                        if (model.customerInfoList.Count > 0)
                        {
                            var deleteCustomerInfoQuery = $@"DELETE FROM SA_CUSTOMER_OTHER_INFO WHERE CUSTOMER_CODE = '{model.CHILD_UPDATE_CUSTOMER_CODE}'";
                            var insertchild = _coreEntity.ExecuteSqlCommand(deleteCustomerInfoQuery);
                        }
                        if (model.alternativeLocationInfoList.Count > 0)
                        {
                            var deleteAlternativeLocationQuery = $@"DELETE FROM SA_CUSTOMER_ALT_LOCATION_INFO WHERE CUSTOMER_CODE = '{model.CHILD_UPDATE_CUSTOMER_CODE}'";
                            var insertchild = _coreEntity.ExecuteSqlCommand(deleteAlternativeLocationQuery);
                        }
                        if (model.budgetCenterList.Count > 0)
                        {
                            var deleteBudgetCenterQuery = $@"DELETE FROM SA_CUSTOMER_BUDGETCENTER_INFO WHERE CUSTOMER_CODE = '{model.CHILD_UPDATE_CUSTOMER_CODE}'";
                            var insertchild = _coreEntity.ExecuteSqlCommand(deleteBudgetCenterQuery);
                        }
                        if (model.sisterConcernsList.Count > 0)
                        {
                            var deleteSisterConcernQuery = $@"DELETE FROM SA_CUSTOMER_SISTER_CONCERN WHERE CUSTOMER_CODE = '{model.CHILD_UPDATE_CUSTOMER_CODE}'";
                            var insertchild = _coreEntity.ExecuteSqlCommand(deleteSisterConcernQuery);
                        }
                        if (model.otherTermsConditionsList.Count > 0)
                        {
                            var deleteOtherTermsConditionsQuery = $@"DELETE FROM SA_CUSTOMER_TERMS_CONDITIONS WHERE CUSTOMER_CODE = '{model.CHILD_UPDATE_CUSTOMER_CODE}'";
                            var insertchild = _coreEntity.ExecuteSqlCommand(deleteOtherTermsConditionsQuery);
                        }
                        if (model.customerStockStatusList.Count > 0)
                        {
                            var deleteCustomerStockQuery = $@"DELETE FROM SA_CUSTOMER_STOCK_INFO WHERE CUSTOMER_CODE = '{model.CHILD_UPDATE_CUSTOMER_CODE}'";
                            var insertchild = _coreEntity.ExecuteSqlCommand(deleteCustomerStockQuery);
                        }
                        DynamicTableInserts(model);
                        data = "UPDATED";
                    }
                    else
                    {

                        var newMaxCustomerCodeQuery = $@" SELECT nvl(max(to_number(CUSTOMER_CODE))+1,1)  as MAX_CUSTOMER_CODE FROM SA_CUSTOMER_SETUP WHERE COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'";

                        var newMaxCustomerCode = this._coreEntity.SqlQuery<int>(newMaxCustomerCodeQuery).FirstOrDefault().ToString();
                        string MasterCustomerCode = string.Empty;
                        string exclusiveFlag = string.Empty;

                        if (model.AfterSaveCustomerCode == "")
                        {
                            //if(model.ACTIVE_FLAG=="true")
                            //{
                            //    model.ACTIVE_FLAG = "Y";
                            //}
                            //else
                            //{
                            //    model.ACTIVE_FLAG = "N";
                            //}

                            MasterCustomerCode = model.MASTER_CUSTOMER_CODE + ".00";
                            var childsqlquery = $@"INSERT INTO SA_CUSTOMER_SETUP (CUSTOMER_CODE,CUSTOMER_ID,CUSTOMER_GROUP_ID,
                                                    CUSTOMER_EDESC,CUSTOMER_NDESC,REGD_OFFICE_EADDRESS,REGD_OFFICE_NADDRESS,
                                                    TEL_MOBILE_NO1,TEL_MOBILE_NO2,TEL_MOBILE_NO3,FAX_NO,EMAIL,PARTY_TYPE_CODE,ACTIVE_FLAG,GROUP_SKU_FLAG,MASTER_CUSTOMER_CODE,PRE_CUSTOMER_CODE,TPIN_VAT_NO,EXCISE_NO,ACC_CODE,
                                        CASH_CUSTOMER_FLAG,COUNTRY_CODE,ZONE_CODE,DISTRICT_CODE,CITY_CODE,REGION_CODE,DEALING_PERSON,AGENT_CODE,BRANCH_CODE,
                                        COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,PRICE_LIST_ID,APPROVED_FLAG)VALUES('{newMaxCustomerCode}','{model.CUSTOMER_ID}','{model.CUSTOMER_GROUP_ID}','{model.CUSTOMER_EDESC}','{model.CUSTOMER_NDESC}','{model.REGD_OFFICE_EADDRESS}','{model.REGD_OFFICE_NADDRESS}','{model.TEL_MOBILE_NO1}','{model.TEL_MOBILE_NO2}','{model.TEL_MOBILE_NO3}'
                                                        ,'{model.FAX}','{model.EMAIL}','{model.PARTY_TYPE_CODE}','{model.ACTIVE_FLAG}','{model.GROUP_SKU_FLAG}','{MasterCustomerCode}','{model.PRE_CUSTOMER_CODE}','{model.PAN_VAT}','{model.EXCISE}','{model.ACC_CODE}','{model.CASH_CUSTOMER_FLAG}','{model.COUNTRY_CODE}','{model.ZONE_CODE}','{model.DISTRICT_CODE}','{model.CITY_CODE}','{model.REGION_CODE}','{model.DEALING_PERSON}','{model.AGENT_CODE}','{model.BRANCH_CODE}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),
                                                         'N','{model.PRICE_LIST_ID}','{model.APPROVED_FLAG}')";
                            var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);
                            _logErp.WarnInDB("Customer created succssfully by user : " + _workContext.CurrentUserinformation.LOGIN_EDESC);
                            if (insertchild > 0)
                            {
                                var fa_sub_ledger_map_query = $@"INSERT INTO FA_SUB_LEDGER_MAP(SUB_CODE,ACC_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SYN_ROWID,MODIFY_DATE,BRANCH_CODE,MODIFY_BY) VALUES('C{newMaxCustomerCode}','{model.ACC_CODE}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','','','{_workContext.CurrentUserinformation.branch_code}','')";
                                var insertedRow = _coreEntity.ExecuteSqlCommand(fa_sub_ledger_map_query);
                                _logErp.WarnInDB("Mapping customer to fa sub ledger map completed by : " + _workContext.CurrentUserinformation.LOGIN_EDESC);
                                data = "INSERTED";

                            }
                        }
                        else
                        {

                            UpdateCustomerTable(model, model.AfterSaveCustomerCode);
                            data = "UPDATED";
                        }

                        DynamicTableInserts(model);
                    }
                    //}
                    trans.Commit();
                    return data;
                }
                catch (Exception ex)
                {
                    _logErp.ErrorInDB("Error while creating child customer : " + ex.StackTrace);
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        public void DynamicTableInserts(CustomerModels model)
        {
            _logErp.InfoInFile("Dynamic customer table insertation started by : " + _workContext.CurrentUserinformation.LOGIN_EDESC);
            try
            {

                if (model.customerDivision.Count > 0)
                {
                    foreach (var division in model.customerDivision)
                    {

                        var insertDivisionQuery = $@"INSERT INTO SA_CUSTOMER_DIVISION_CR_LIMIT (CUSTOMER_CODE,
                                                    CREDIT_LIMIT,DIVISION_CODE,BLOCK_FLAG,REMARKS,
                                        COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG)VALUES('{model.AfterSaveCustomerCode}','{division.CREDIT_LIMIT}','{division.DIVISION_CODE}','{division.BLOCK_FLAG}','{division.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),
                                                         'N')";
                        var divisionInserted = _coreEntity.ExecuteSqlCommand(insertDivisionQuery);
                        _logErp.WarnInDB("Customer Division Saved successfully by : " + _workContext.CurrentUserinformation.LOGIN_EDESC);
                    }

                }
                if (model.ownerInfo.Count > 0)
                {
                    foreach (var owner in model.ownerInfo)
                    {
                        var insertOwnerQuery = $@"INSERT INTO SA_CUSTOMER_OWNER_INFO (CUSTOMER_CODE,
                                                    OWNER_NAME,CONTACT_PERSON,ADDRESS,DESIGNATION,EMAIL,FAX_NO, TEL_MOBILE_NO,IMAGE_FILE_CITIZENSHIP,IMAGE_FILE_COMPANY_PAN,IMAGE_FILE_COMPANY_REG,IMAGE_FILE_NAME,REMARKS,
                                        COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG)VALUES('{model.AfterSaveCustomerCode}','{owner.OWNER_NAME}','{owner.CONTACT_PERSON}','{owner.ADDRESS}','{owner.DESIGNATION}','{owner.EMAIL}','{owner.FAX_NO}','{owner.TEL_MOBILE_NO}','{owner.IMAGE_FILE_CITIZENSHIP}','{owner.IMAGE_FILE_COMPANY_PAN}','{owner.IMAGE_FILE_COMPANY_REG}','{owner.IMAGE_FILE_NAME}','{owner.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),
                                                         'N')";
                        var ownerInserted = _coreEntity.ExecuteSqlCommand(insertOwnerQuery);
                        _logErp.WarnInDB("Customer owner info Saved successfully by : " + _workContext.CurrentUserinformation.LOGIN_EDESC);
                    }
                }
                if (model.customerInvoiceWiseOpening.Count > 0)
                {
                    foreach (var invoiceOpening in model.customerInvoiceWiseOpening)
                    {
                        var invoiceOpeningQuery = $@"INSERT INTO SA_CUSTOMER_OPENING_SETUP (CUSTOMER_CODE,
                                                    REFERENCE_NO,INVOICE_DATE,BALANCE_AMOUNT,CURRENCY_CODE,EXCHANGE_RATE,DUE_DATE, TRANSACTION_TYPE,ACC_CODE,DIVISION_CODE,PARTY_TYPE_CODE,EMPLOYEE_CODE,SYN_ROWID,
                                        COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,BRANCH_CODE)VALUES('{model.AfterSaveCustomerCode}','{invoiceOpening.REFERENCE_NO}',TO_DATE('{invoiceOpening.INVOICE_DATE.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'{invoiceOpening.BALANCE_AMOUNT}','{invoiceOpening.CURRENCY_CODE}','{invoiceOpening.EXCHANGE_RATE}',TO_DATE('{invoiceOpening.DUE_DATE}','MM/dd/yyyy HH:MI:SS AM'),'{invoiceOpening.TRANSACTION_TYPE}','{invoiceOpening.ACC_CODE}','{invoiceOpening.DIVISION_CODE}','{invoiceOpening.PARTY_TYPE_CODE}','{invoiceOpening.EMPLOYEE_CODE}','{invoiceOpening.SYN_ROWID}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),
                                                         'N','{_workContext.CurrentUserinformation.branch_code}')";
                        var invoiceOpeningInserted = _coreEntity.ExecuteSqlCommand(invoiceOpeningQuery);
                        _logErp.WarnInDB("Customer invoice wise opening status Saved successfully by : " + _workContext.CurrentUserinformation.LOGIN_EDESC);
                    }
                }
                if (model.alternativeLocationInfoList.Count > 0)
                {
                    foreach (var alternativeLocation in model.alternativeLocationInfoList)
                    {
                        var insertAlternativeLocation = $@"INSERT INTO SA_CUSTOMER_ALT_LOCATION_INFO (CUSTOMER_CODE,
                                                    OFFICE_EDESC,CONTACT_PERSON,ADDRESS,TEL_MOBILE_NO,FAX_NO, EMAIL,REMARKS,LOCATION_CODE,
                                        COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG)VALUES('{model.AfterSaveCustomerCode}','{alternativeLocation.OFFICE_EDESC}','{alternativeLocation.CONTACT_PERSON}','{alternativeLocation.ADDRESS}','{alternativeLocation.TEL_MOBILE_NO}','{alternativeLocation.FAX_NO}','{alternativeLocation.EMAIL}','{alternativeLocation.REMARKS}','{alternativeLocation.LOCATION_CODE}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),
                                                         'N')";
                        var alternativeLocationInserted = _coreEntity.ExecuteSqlCommand(insertAlternativeLocation);
                        _logErp.WarnInDB("Customer alternative location Saved successfully by : " + _workContext.CurrentUserinformation.LOGIN_EDESC);
                    }
                }
                if (model.budgetCenterList.Count > 0)
                {
                    foreach (var budgetCenter in model.budgetCenterList)
                    {
                        var insertbudgetCenter = $@"INSERT INTO SA_CUSTOMER_BUDGETCENTER_INFO (CUSTOMER_CODE,
                                                    BUDGET_CODE,REMARKS,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG)VALUES('{model.AfterSaveCustomerCode}','{budgetCenter.BUDGET_CODE}','{budgetCenter.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),
                                                         'N')";
                        var budgetCenterInserted = _coreEntity.ExecuteSqlCommand(insertbudgetCenter);
                        _logErp.WarnInDB("Customer budget center Saved successfully by : " + _workContext.CurrentUserinformation.LOGIN_EDESC);
                    }
                }
                if (model.sisterConcernsList.Count > 0)
                {
                    foreach (var sisterConcern in model.sisterConcernsList)
                    {
                        var insertSisterConcern = $@"INSERT INTO SA_CUSTOMER_SISTER_CONCERN (CUSTOMER_CODE,
                                                    SISTER_CONCERN_EDESC,SISTER_CONCERN_NDESC,REMARKS,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG)VALUES('{model.AfterSaveCustomerCode}','{sisterConcern.SISTER_CONCERN_EDESC}','{sisterConcern.SISTER_CONCERN_NDESC}','{sisterConcern.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),
                                                         'N')";
                        var sisterConcernInserted = _coreEntity.ExecuteSqlCommand(insertSisterConcern);
                        _logErp.WarnInDB("Customer sister concern Saved successfully by : " + _workContext.CurrentUserinformation.LOGIN_EDESC);
                    }
                }
                if (model.customerInfoList.Count > 0)
                {
                    foreach (var customerInfo in model.customerInfoList)
                    {
                        var customerInfoQuery = $@"INSERT INTO SA_CUSTOMER_OTHER_INFO (CUSTOMER_CODE,FIELD_NAME,FIELD_VALUE,REMARKS,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG)VALUES('{model.AfterSaveCustomerCode}','{customerInfo.FIELD_NAME}','{customerInfo.FIELD_VALUE}','{customerInfo.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),
                                                         'N')";
                        var sisterConcernInserted = _coreEntity.ExecuteSqlCommand(customerInfoQuery);
                        _logErp.WarnInDB("Customer info list Saved successfully by : " + _workContext.CurrentUserinformation.LOGIN_EDESC);
                    }
                }
                if (model.otherTermsConditionsList.Count > 0)
                {
                    foreach (var otherTermsConditions in model.otherTermsConditionsList)
                    {
                        var insertOtherTermsConditions = $@"INSERT INTO SA_CUSTOMER_TERMS_CONDITIONS (CUSTOMER_CODE,FIELD_NAME,FIELD_VALUE,REMARKS,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG)VALUES('{model.AfterSaveCustomerCode}','{otherTermsConditions.FIELD_NAME}','{otherTermsConditions.FIELD_VALUE}','{otherTermsConditions.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),
                                                         'N')";
                        var sisterConcernInserted = _coreEntity.ExecuteSqlCommand(insertOtherTermsConditions);
                        _logErp.WarnInDB("Customer term and condition Saved successfully by : " + _workContext.CurrentUserinformation.LOGIN_EDESC);
                    }
                }
                if (model.customerStockStatusList.Count > 0)
                {
                    foreach (var customerStockStatus in model.customerStockStatusList)
                    {
                        var insertCustomerStatusQuery = $@"INSERT INTO SA_CUSTOMER_STOCK_INFO (CUSTOMER_CODE,
                                                    ITEM_CODE,STOCK_DATE,QUANTITY,REMARKS,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG)VALUES('{model.AfterSaveCustomerCode}','{customerStockStatus.ITEM_CODE}',TO_DATE('{customerStockStatus.STOCK_DATE}','MM/dd/yyyy HH:MI:SS AM'),'{customerStockStatus.QUANTITY}','{customerStockStatus.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),
                                                         'N')";
                        var customerStatusInserted = _coreEntity.ExecuteSqlCommand(insertCustomerStatusQuery);
                        _logErp.WarnInDB("Customer stock status Saved successfully by : " + _workContext.CurrentUserinformation.LOGIN_EDESC);
                    }
                }

            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while saving dynamic customer table by : " + _workContext.CurrentUserinformation.LOGIN_EDESC + " With Error : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }

        }
        public void UpdateCustomerTable(CustomerModels model, string CustomerCode)
        {
            try
            {
                var updatesqlquery = $@"UPDATE SA_CUSTOMER_SETUP SET REGD_OFFICE_EADDRESS ='{model.REGD_OFFICE_EADDRESS}',REGD_OFFICE_NADDRESS = '{model.REGD_OFFICE_NADDRESS}',PRICE_LIST_ID='{model.PRICE_LIST_ID}', TEL_MOBILE_NO1 = '{model.TEL_MOBILE_NO1}', CUSTOMER_ID='{model.CUSTOMER_ID}', CUSTOMER_GROUP_ID='{model.CUSTOMER_GROUP_ID}',CUSTOMER_EDESC='{model.CUSTOMER_EDESC}',CUSTOMER_NDESC = '{model.CUSTOMER_NDESC}',TEL_MOBILE_NO2 = '{model.TEL_MOBILE_NO2}',TEL_MOBILE_NO3='{model.TEL_MOBILE_NO3}',FAX_NO='{model.FAX}',EMAIL='{model.EMAIL}', PARTY_TYPE_CODE='{model.PARTY_TYPE_CODE}',ACTIVE_FLAG='{model.ACTIVE_FLAG}', TPIN_VAT_NO='{model.PAN_VAT}', EXCISE_NO='{model.EXCISE}',ACC_CODE='{model.ACC_CODE}',CASH_CUSTOMER_FLAG='{model.CASH_CUSTOMER_FLAG}',             COUNTRY_CODE='{model.COUNTRY_CODE}',ZONE_CODE='{model.ZONE_CODE}',DISTRICT_CODE='{model.DISTRICT_CODE}',CITY_CODE='{model.CITY_CODE}',REGION_CODE='{model.REGION_CODE}',DEALING_PERSON='{model.DEALING_PERSON}',AGENT_CODE='{model.AGENT_CODE}',BRANCH_CODE='{model.BRANCH_CODE}', CREDIT_RATE='{model.CREDIT_RATE}',CREDIT_LIMIT='{model.CREDIT_LIMIT}',CUSHION_PERCENT='{model.CUSHION_PERCENT}',PRE_CREDIT_LIMIT='{model.PRE_CREDIT_LIMIT}',DUE_BILL_COUNT='{model.DUE_BILL_COUNT}',EXCEED_LIMIT_PERCENTAGE='{model.EXCEED_LIMIT_PERCENTAGE}',INTEREST_RATE='{model.INTEREST_RATE}',BANK_GUARANTEE='{model.BANK_GUARANTEE}',OPENING_DATE=TO_DATE('{model.OPENING_DATE}','MM/dd/yyyy HH:MI:SS AM'),EXPIRY_DATE=TO_DATE('{model.EXPIRY_DATE}','MM/dd/yyyy HH:MI:SS AM'),MATURITY_DATE=TO_DATE('{model.MATURITY_DATE}','MM/dd/yyyy HH:MI:SS AM'),DISCOUNT_FLAT_RATE='{model.DISCOUNT_FLAT_RATE}',DISCOUNT_DAYS='{model.DISCOUNT_DAYS}',DISCOUNT_PERCENT='{model.DISCOUNT_PERCENT}',EXCLUSIVE_FLAG='{model.EXCLUSIVE_FLAG}',CREDIT_DAYS='{model.CREDIT_DAYS}',CREDIT_ACTION_FLAG='{model.CREDIT_ACTION_FLAG}',TERMS_CONDITIONS='{model.TERMS_CONDITIONS}',MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',MODIFY_DATE=TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy') WHERE CUSTOMER_CODE = '{CustomerCode}'";
                var updateChilds = _coreEntity.ExecuteSqlCommand(updatesqlquery);
                var updateFaSubLedgerMap = $@"UPDATE FA_SUB_LEDGER_MAP SET ACC_CODE='{model.ACC_CODE}' where SUB_CODE='C{CustomerCode}'";
                var updatedLedgerChilds = _coreEntity.ExecuteSqlCommand(updateFaSubLedgerMap);

            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while updating customer by :  " + _workContext.CurrentUserinformation.LOGIN_EDESC + ' ' + ex.StackTrace);
                throw new Exception(ex.Message);
            }

        }
        public List<EmployeeCodeModels> getAllComboEmployees(string filter)
        {
            try
            {
                if (string.IsNullOrEmpty(filter))
                {
                    filter = string.Empty;

                }

                var partyTypeQuery = $@"select COALESCE(EMPLOYEE_CODE,' ') EMPLOYEE_CODE
                            ,COALESCE(EMPLOYEE_EDESC,' ') EMPLOYEE_EDESC
                            FROM HR_EMPLOYEE_SETUP
                            WHERE DELETED_FLAG = 'N' AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}' AND (EMPLOYEE_CODE like '%{filter.ToUpperInvariant()}%' 
                            or upper(EMPLOYEE_EDESC) like '%{filter.ToUpperInvariant()}%')";
                var result = _dbContext.SqlQuery<EmployeeCodeModels>(partyTypeQuery).ToList();
                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }
        public List<DealerModels> getAllComboDealers(string filter)
        {
            try
            {
                if (string.IsNullOrEmpty(filter))
                {
                    filter = string.Empty;

                }

                var partyTypeQuery = $@"select COALESCE(PARTY_TYPE_CODE,' ') PARTY_TYPE_CODE
                            ,COALESCE(PARTY_TYPE_EDESC,' ') PARTY_TYPE_EDESC
                            FROM IP_PARTY_TYPE_CODE
                            WHERE DELETED_FLAG = 'N' AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}' AND (PARTY_TYPE_CODE like '%{filter.ToUpperInvariant()}%' 
                            or upper(PARTY_TYPE_EDESC) like '%{filter.ToUpperInvariant()}%')";
                var result = _dbContext.SqlQuery<DealerModels>(partyTypeQuery).ToList();
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<BudgetCenter> getAllBudgetCenter(string filter)
        {
            try
            {
                if (string.IsNullOrEmpty(filter))
                {
                    filter = string.Empty;

                }

                var partyTypeQuery = $@"SELECT BUDGET_CODE,BUDGET_EDESC
                            FROM BC_BUDGET_CENTER_SETUP
                            WHERE DELETED_FLAG = 'N' AND GROUP_SKU_FLAG = 'I' AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}' AND (BUDGET_CODE like '%{filter.ToUpperInvariant()}%' 
                            or upper(BUDGET_EDESC) like '%{filter.ToUpperInvariant()}%')";
                var result = _dbContext.SqlQuery<BudgetCenter>(partyTypeQuery).ToList();
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<ItemSetupModel> getAllItemsForCustomerStock(string filter)
        {
            try
            {
                if (string.IsNullOrEmpty(filter))
                {
                    filter = string.Empty;

                }

                var partyTypeQuery = $@"SELECT ITEM_CODE,ITEM_EDESC
                            FROM IP_ITEM_MASTER_SETUP
                            WHERE DELETED_FLAG = 'N' AND GROUP_SKU_FLAG = 'I' AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}' AND (ITEM_CODE like '%{filter.ToUpperInvariant()}%' 
                            or upper(ITEM_EDESC) like '%{filter.ToUpperInvariant()}%')";
                var result = _dbContext.SqlQuery<ItemSetupModel>(partyTypeQuery).ToList();
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public KYCFORM GetKYCFORM(string customerCode)
        {
            var getCustomersQuery = $@"  select  CUSTOMER_CODE as CustomerId,
                                                    CUSTOMER_EDESC as KYCCustomerName,BIRTH_DATE as BirthDate_bs,Gender,MaritalStatus,
                                                    Religion,Bloadgroup,telephoneNo,MobileNo,
                                                    Companyname,EmailOffice,Address,
                                                    PermanentHouseNo,PWARDNO,PSTEETADDRESS,PZONE,PDIStrict,PVDCMunicipality,THouseNo,TWARDNO
                                                        ,TSTEETADDRESS,TZONE,TDIStrict,TVDCMunicipality,EmergencyName,Emergencyrelationship,Emergencyaddress,Emergencyphoneno,FamilyName,FamilyMotherName,FamilyspouseName,weddingDate,Childname,Organizationtype,organizationname,Position,FromDate,TO_CHAR (BIRTH_DATE_AD, 'dd-Mon-yyyy') AS birthdate
                                                        from SA_CUSTOMER_KYC  WHERE CUSTOMER_CODE = '{customerCode}'";

            var result = _dbContext.SqlQuery<KYCFORM>(getCustomersQuery).FirstOrDefault();
            return result;
        }
        public CustomerModels GetChildCustomerByCustomerCode(string customerCode)
        {
            //var getCustomersQueryOld = $@"SELECT GROUP_SKU_FLAG,MASTER_CUSTOMER_CODE,PRE_CUSTOMER_CODE,DISCOUNT_FLAT_RATE,EXCLUSIVE_FLAG,DISCOUNT_DAYS,DISCOUNT_PERCENT,DELETED_FLAG,REGION_CODE,CREDIT_DAYS,CREDIT_ACTION_FLAG,ACC_CODE,PR_CODE,TPIN_VAT_NO,SYN_ROWID,DELTA_FLAG,TERMS_CONDITIONS,BANK_GUARANTEE,PRE_CREDIT_LIMIT,BRANCH_CODE,AGENT_CODE,APPROVED_FLAG,PAN_NO,EXPIRY_DATE,EXCEED_LIMIT_PERCENTAGE,MAX_SALES_VALUE,TEL_MOBILE_NO3,CASH_CUSTOMER_FLAG,CUSTOMER_ID,GROUP_START_NO,PREFIX_TEXT,INTEREST_RATE,OPENING_DATE,MATURITY_DATE,CUSTOMER_GROUP_ID,COUNTRY_CODE,ZONE_CODE,DISTRICT_CODE,CITY_CODE,DEALING_PERSON,EXCISE_NO,CUSTOMER_CODE,CUSTOMER_EDESC,CUSTOMER_NDESC,REGD_OFFICE_EADDRESS,REGD_OFFICE_NADDRESS,TEL_MOBILE_NO1,TEL_MOBILE_NO2,FAX_NO,EMAIL,PARTY_TYPE_CODE,CUSTOMER_FLAG,LINK_SUB_CODE,CREDIT_RATE,CREDIT_LIMIT,CUSHION_PERCENT,DUE_BILL_COUNT,ACTIVE_FLAG,REMARKS,PRICE_LIST_ID FROM SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{customerCode}'";
            var getCustomersQuery = $@"SELECT GROUP_SKU_FLAG,MASTER_CUSTOMER_CODE,PRE_CUSTOMER_CODE,DISCOUNT_FLAT_RATE,EXCLUSIVE_FLAG,DISCOUNT_DAYS,DISCOUNT_PERCENT,DELETED_FLAG,REGION_CODE,CREDIT_DAYS,CREDIT_ACTION_FLAG,ACC_CODE,PR_CODE,TPIN_VAT_NO,SYN_ROWID,TERMS_CONDITIONS,BANK_GUARANTEE,PRE_CREDIT_LIMIT,BRANCH_CODE,AGENT_CODE,APPROVED_FLAG,PAN_NO,EXPIRY_DATE,EXCEED_LIMIT_PERCENTAGE,MAX_SALES_VALUE,TEL_MOBILE_NO3,CASH_CUSTOMER_FLAG,CUSTOMER_ID,GROUP_START_NO,PREFIX_TEXT,INTEREST_RATE,OPENING_DATE,MATURITY_DATE,CUSTOMER_GROUP_ID,COUNTRY_CODE,ZONE_CODE,DISTRICT_CODE,CITY_CODE,DEALING_PERSON,EXCISE_NO,CUSTOMER_CODE,CUSTOMER_EDESC,CUSTOMER_NDESC,REGD_OFFICE_EADDRESS,REGD_OFFICE_NADDRESS,TEL_MOBILE_NO1,TEL_MOBILE_NO2,FAX_NO,EMAIL,PARTY_TYPE_CODE,CUSTOMER_FLAG,LINK_SUB_CODE,CREDIT_RATE,CREDIT_LIMIT,CUSHION_PERCENT,DUE_BILL_COUNT,ACTIVE_FLAG,REMARKS,PRICE_LIST_ID FROM SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE = '{customerCode}'";

            var result = _dbContext.SqlQuery<CustomerModels>(getCustomersQuery).FirstOrDefault();

            #region Owner Info 
            var getOwnerInfoQuery = $@"SELECT CUSTOMER_CODE,OWNER_NAME,DESIGNATION,CONTACT_PERSON,ADDRESS,TEL_MOBILE_NO,FAX_NO,EMAIL,IMAGE_FILE_NAME,REMARKS,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SYN_ROWID,MODIFY_DATE,MODIFY_BY,IMAGE_FILE_CITIZENSHIP,IMAGE_FILE_COMPANY_REG,IMAGE_FILE_COMPANY_PAN FROM SA_CUSTOMER_OWNER_INFO WHERE CUSTOMER_CODE = '{customerCode}' AND DELETED_FLAG = 'N'";
            var ownerInfoResult = _dbContext.SqlQuery<OwnerModels>(getOwnerInfoQuery).ToList();
            if (ownerInfoResult.Count > 0)
            {
                result.ownerInfo.AddRange(ownerInfoResult);
            }
            #endregion

            #region Other Information
            var getOtherInfoQuery = $@"SELECT CUSTOMER_CODE,FIELD_NAME,FIELD_VALUE,REMARKS,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SYN_ROWID,MODIFY_DATE,MODIFY_BY FROM SA_CUSTOMER_OTHER_INFO WHERE CUSTOMER_CODE = '{customerCode}' AND DELETED_FLAG = 'N'";
            var otherInfoResult = _dbContext.SqlQuery<CustomerInfo>(getOtherInfoQuery).ToList();
            if (otherInfoResult.Count > 0)
            {
                result.customerInfoList.AddRange(otherInfoResult);
            }
            #endregion End Owner Info 

            #region Alternative Locaiton Information
            var getAlternativeLocationQuery = $@"SELECT COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SYN_ROWID,LOCATION_CODE,MODIFY_DATE,MODIFY_BY,CUSTOMER_CODE,OFFICE_EDESC,OFFICE_NDESC,CONTACT_PERSON,ADDRESS,TEL_MOBILE_NO,FAX_NO,EMAIL,REMARKS FROM SA_CUSTOMER_ALT_LOCATION_INFO WHERE CUSTOMER_CODE = '{customerCode}' AND DELETED_FLAG = 'N'";
            var alternativeLocationQueryResult = _dbContext.SqlQuery<AlternativeLocationInfo>(getAlternativeLocationQuery).ToList();
            if (alternativeLocationQueryResult.Count > 0)
            {
                result.alternativeLocationInfoList.AddRange(alternativeLocationQueryResult);
            }
            #endregion 

            #region Budget Center Information
            var getBudgetCenterQuery = $@"SELECT CUSTOMER_CODE,BUDGET_CODE,REMARKS,COMPANY_CODE,DELETED_FLAG,SYN_ROWID FROM SA_CUSTOMER_BUDGETCENTER_INFO WHERE CUSTOMER_CODE = '{customerCode}' AND DELETED_FLAG = 'N'";
            var budgetCenterResult = _dbContext.SqlQuery<CustomerBudgetCenter>(getBudgetCenterQuery).ToList();
            if (budgetCenterResult.Count > 0)
            {
                result.budgetCenterList.AddRange(budgetCenterResult);
            }
            #endregion 

            #region Sister Concern Information
            var sisterConcernsQuery = $@"SELECT CUSTOMER_CODE,SISTER_CONCERN_EDESC,SISTER_CONCERN_NDESC,REMARKS,COMPANY_CODE,DELETED_FLAG,SYN_ROWID FROM SA_CUSTOMER_SISTER_CONCERN WHERE CUSTOMER_CODE = '{customerCode}' AND DELETED_FLAG= 'N'";
            var sisterConcernsResult = _dbContext.SqlQuery<CustomerSisterConcern>(sisterConcernsQuery).ToList();
            if (sisterConcernsResult.Count > 0)
            {
                result.sisterConcernsList.AddRange(sisterConcernsResult);
            }
            #endregion 

            #region Division Information
            var divisionQuery = $@"SELECT CUSTOMER_CODE,CREDIT_LIMIT,REMARKS,COMPANY_CODE,DIVISION_CODE,DELETED_FLAG,CURRENCY_CODE,EXCHANGE_RATE,BLOCK_FLAG FROM SA_CUSTOMER_DIVISION_CR_LIMIT WHERE CUSTOMER_CODE = '{customerCode}' AND DELETED_FLAG = 'N'";
            var divisionInfoResult = _dbContext.SqlQuery<CustomerDivisionModels>(divisionQuery).ToList();
            if (divisionInfoResult.Count > 0)
            {
                result.customerDivision.AddRange(divisionInfoResult);
            }
            #endregion 

            #region Invoice Wise Information
            var invoiceWiseOpeningQuery = $@"SELECT DIVISION_CODE,PARTY_TYPE_CODE,EMPLOYEE_CODE,CUSTOMER_CODE,REFERENCE_NO,INVOICE_DATE,BALANCE_AMOUNT,REMARKS,COMPANY_CODE,DELETED_FLAG,CURRENCY_CODE,EXCHANGE_RATE,DUE_DATE,TRANSACTION_TYPE,SYN_ROWID,BRANCH_CODE,ACC_CODE FROM SA_CUSTOMER_OPENING_SETUP WHERE CUSTOMER_CODE = '{customerCode}' AND DELETED_FLAG = 'N'";
            var invoiceWiseInfoResult = _dbContext.SqlQuery<CustomerInvoiceWiseOpeningSetup>(invoiceWiseOpeningQuery).ToList();
            if (invoiceWiseInfoResult.Count > 0)
            {
                result.customerInvoiceWiseOpening.AddRange(invoiceWiseInfoResult);
            }
            #endregion

            #region Other Terms Conditions Information
            var otherTermsConditionsQuery = $@"SELECT CUSTOMER_CODE,FIELD_NAME,FIELD_VALUE,REMARKS,COMPANY_CODE,DELETED_FLAG,SYN_ROWID FROM SA_CUSTOMER_TERMS_CONDITIONS WHERE CUSTOMER_CODE = '{customerCode}' AND DELETED_FLAG='N'";
            var otherTermsConditionsResult = _dbContext.SqlQuery<CustomerOtherTermsConditions>(otherTermsConditionsQuery).ToList();
            if (otherTermsConditionsResult.Count > 0)
            {
                result.otherTermsConditionsList.AddRange(otherTermsConditionsResult);
            }
            #endregion

            #region Customer Stock Status
            var customerStockStatusQuery = $@"SELECT CUSTOMER_CODE,ITEM_CODE,STOCK_DATE,QUANTITY,REMARKS,COMPANY_CODE,DELETED_FLAG,SYN_ROWID FROM SA_CUSTOMER_STOCK_INFO WHERE CUSTOMER_CODE = '{customerCode}' AND DELETED_FLAG = 'N'";
            var customerStockResult = _dbContext.SqlQuery<CustomerStockStatus>(customerStockStatusQuery).ToList();
            if (customerStockResult.Count > 0)
            {
                result.customerStockStatusList.AddRange(customerStockResult);
            }
            #endregion 

            return result;
        }
        public List<Currency> getAllCurrency()
        {
            try
            {

                var partyTypeQuery = $@"SELECT CURRENCY_CODE,CURRENCY_EDESC
                            FROM CURRENCY_SETUP
                            WHERE DELETED_FLAG = 'N' AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'";
                var result = _dbContext.SqlQuery<Currency>(partyTypeQuery).ToList();
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public string DeleteCustomerByCustomerCode(string CustomerCode)
        {
            var companyCode = _workContext.CurrentUserinformation.company_code;

            try
            {
                var sqlquery1 = $@"select count(*) from SA_CUSTOMER_SETUP where PRE_CUSTOMER_CODE in (  select MASTER_CUSTOMER_CODE from SA_CUSTOMER_SETUP where CUSTOMER_CODE = '{CustomerCode}' AND COMPANY_CODE='{companyCode}')";
                int count = _dbContext.SqlQuery<int>(sqlquery1).FirstOrDefault();
                if (count > 0)
                {
                    return "HAS_CHILD";
                }

                if (string.IsNullOrEmpty(companyCode)) { companyCode = string.Empty; }
                var sqlquerycustomer = $@"UPDATE SA_CUSTOMER_SETUP SET DELETED_FLAG = 'Y' WHERE CUSTOMER_CODE='{CustomerCode}' AND COMPANY_CODE ='{companyCode}'";
                var resultcustomer = _dbContext.ExecuteSqlCommand(sqlquerycustomer);
                var sqlquery = $@"UPDATE SA_CUSTOMER_DIVISION_CR_LIMIT SET DELETED_FLAG = 'Y' WHERE CUSTOMER_CODE='{CustomerCode}' AND COMPANY_CODE ='{companyCode}'";
                var result0 = _dbContext.ExecuteSqlCommand(sqlquery);
                var sqlsofquery = $@"UPDATE SA_CUSTOMER_OWNER_INFO SET DELETED_FLAG = 'Y' WHERE CUSTOMER_CODE='{CustomerCode}' AND COMPANY_CODE ='{companyCode}'";
                var result1 = _dbContext.ExecuteSqlCommand(sqlsofquery);
                var sqlsscquery = $@"UPDATE SA_CUSTOMER_OPENING_SETUP SET DELETED_FLAG = 'Y' WHERE CUSTOMER_CODE='{CustomerCode}' AND COMPANY_CODE ='{companyCode}'";
                var result2 = _dbContext.ExecuteSqlCommand(sqlsscquery);
                var sqlsosquery = $@"UPDATE SA_CUSTOMER_TERMS_CONDITIONS SET DELETED_FLAG = 'Y' WHERE CUSTOMER_CODE='{CustomerCode}' AND COMPANY_CODE ='{companyCode}'";
                var result3 = _dbContext.ExecuteSqlCommand(sqlsosquery);
                var sqlscsquery = $@"UPDATE SA_CUSTOMER_OTHER_INFO SET DELETED_FLAG = 'Y' WHERE CUSTOMER_CODE='{CustomerCode}' AND COMPANY_CODE ='{companyCode}'";
                var result4 = _dbContext.ExecuteSqlCommand(sqlscsquery);
                var sqlscalquery = $@"UPDATE SA_CUSTOMER_ALT_LOCATION_INFO SET DELETED_FLAG = 'Y' WHERE CUSTOMER_CODE='{CustomerCode}' AND COMPANY_CODE ='{companyCode}'";
                var result5 = _dbContext.ExecuteSqlCommand(sqlscalquery);
                var sqlscbiquery = $@"UPDATE SA_CUSTOMER_BUDGETCENTER_INFO SET DELETED_FLAG = 'Y' WHERE CUSTOMER_CODE='{CustomerCode}' AND COMPANY_CODE ='{companyCode}'";
                var result6 = _dbContext.ExecuteSqlCommand(sqlscbiquery);
                var sqlscscquery = $@"UPDATE SA_CUSTOMER_SISTER_CONCERN SET DELETED_FLAG = 'Y' WHERE CUSTOMER_CODE='{CustomerCode}' AND COMPANY_CODE ='{companyCode}'";
                var result7 = _dbContext.ExecuteSqlCommand(sqlscscquery);
                var sqlscsiquery = $@"UPDATE SA_CUSTOMER_STOCK_INFO SET DELETED_FLAG = 'Y' WHERE CUSTOMER_CODE='{CustomerCode}' AND COMPANY_CODE ='{companyCode}'";
                var result8 = _dbContext.ExecuteSqlCommand(sqlscsiquery);
                return "DELETED";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        public string InsertQuickSetup(QuickSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    if (model.FLAG == "C")
                    {
                        var newmaxitemcode = string.Empty;
                        var newmaxitemcodequery = $@"SELECT MAX(TO_NUMBER(CUSTOMER_CODE))+1 as MASTER_CUSTOMER_CODE FROM SA_CUSTOMER_SETUP";
                        newmaxitemcode = this._coreEntity.SqlQuery<int>(newmaxitemcodequery).FirstOrDefault().ToString();
                        var newprecustomercode = model.MASTER_CODE;
                        var newmastercustomercode = model.MASTER_CODE + "." + "00";
                        var customersetupquery = $@"INSERT INTO SA_CUSTOMER_SETUP (CUSTOMER_CODE,
                                  CUSTOMER_EDESC,CUSTOMER_NDESC,
                                   REGD_OFFICE_EADDRESS,TEL_MOBILE_NO1,EMAIL,
                                  GROUP_SKU_FLAG,
                                  MASTER_CUSTOMER_CODE,
                                  PRE_CUSTOMER_CODE,
                                  COMPANY_CODE,
                                  CREATED_BY,
                                  CREATED_DATE,
                                  DELETED_FLAG
                                 )
                                VALUES('{newmaxitemcode}','{model.ENG_NAME}','{model.NEP_NAME}','{model.REGD_OFFICE_EADDRESS}','{model.TEL_MOBILE_NO1}','{model.EMAIL}','I','{newmastercustomercode}',
                                       '{newprecustomercode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),                                      'N')";

                        var insertcustomersetup = _coreEntity.ExecuteSqlCommand(customersetupquery);

                        return "C_SUCCESS";


                    }
                    else if (model.FLAG == "I")
                    {
                        var newmaxitemcode = string.Empty;
                        var newmaxitemcodequery = $@"SELECT MAX(TO_NUMBER(ITEM_CODE))+1 as MASTER_ITEM_CODE FROM IP_ITEM_MASTER_SETUP";
                        newmaxitemcode = this._coreEntity.SqlQuery<int>(newmaxitemcodequery).FirstOrDefault().ToString();
                        var newpreitemcode = model.MASTER_CODE;
                        var newmasteritemcode = model.MASTER_CODE + "." + "00";
                        var suppliersetupquery = $@"INSERT INTO IP_ITEM_MASTER_SETUP (ITEM_CODE,
                                  ITEM_EDESC,ITEM_NDESC,
                                    CATEGORY_CODE,INDEX_MU_CODE,
                                  GROUP_SKU_FLAG,
                                  MASTER_ITEM_CODE,
                                  PRE_ITEM_CODE,
                                  COMPANY_CODE,
                                  CREATED_BY,
                                  CREATED_DATE,
                                  DELETED_FLAG
                                 )
                                VALUES('{newmaxitemcode}','{model.ENG_NAME}','{model.NEP_NAME}','{model.CATEGORY_CODE}','{model.INDEX_MU_CODE}','I','{newmasteritemcode}',
                                       '{newpreitemcode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),                                      'N')";

                        var insertsuppliersetup = _coreEntity.ExecuteSqlCommand(suppliersetupquery);

                        return "I_SUCCESS";
                    }
                    else if (model.FLAG == "S")
                    {
                        var newmaxitemcode = string.Empty;
                        var newmaxitemcodequery = $@"SELECT MAX(TO_NUMBER(SUPPLIER_CODE))+1 as MASTER_SUPPLIER_CODE FROM IP_SUPPLIER_SETUP";
                        newmaxitemcode = this._coreEntity.SqlQuery<int>(newmaxitemcodequery).FirstOrDefault().ToString();

                        var newpresuppliercode = model.MASTER_CODE;
                        var newmastersuppliercode = model.MASTER_CODE + "." + "00";
                        var suppliersetupquery = $@"INSERT INTO IP_SUPPLIER_SETUP (SUPPLIER_CODE,
                                  SUPPLIER_EDESC,SUPPLIER_NDESC,
                                    REGD_OFFICE_EADDRESS,TEL_MOBILE_NO1,EMAIL,
                                  GROUP_SKU_FLAG,
                                  MASTER_SUPPLIER_CODE,
                                  PRE_SUPPLIER_CODE,
                                  COMPANY_CODE,
                                  CREATED_BY,
                                  CREATED_DATE,
                                  DELETED_FLAG
                                 )
                                VALUES('{newmaxitemcode}','{model.ENG_NAME}','{model.NEP_NAME}','{model.REGD_OFFICE_EADDRESS}','{model.TEL_MOBILE_NO1}','{model.EMAIL}','I','{newmastersuppliercode}',
                                       '{newpresuppliercode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N')";

                        var insertsuppliersetup = _coreEntity.ExecuteSqlCommand(suppliersetupquery);

                        return "S_SUCCESS";
                    }
                    trans.Commit();
                    return "INSERTED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }



        }

        public SubmitResponse SaveWebPrefrence(WebPrefrence model)
        {
            var response = new SubmitResponse();
            response.Success = true;
            response.Message = "Saved successFully";
            var filename = Constants.WebPrefranceSetting;
            try
            {
                // var setting = _settingService.LoadSetting<UserDashboardSetting>(filename);
                var userDashboardSetting = new WebPrefrenceSetting();
                userDashboardSetting.Userid = model.Userid;
                userDashboardSetting.ShowAdvanceSearch = model.ShowAdvanceSearch;
                userDashboardSetting.ShowAdvanceAutoComplete = model.ShowAdvanceAutoComplete;
                if (_settingService.DeleteSetting(filename))
                {
                    _settingService.SaveSetting<WebPrefrenceSetting>(userDashboardSetting, filename);
                }


            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;

        }





        #region Prefference Setup

        public PreferenceModel GetPreferenceDetailByCompanyCode(string cmpanyId)
        {
            try
            {
                if (!cmpanyId.StartsWith("0"))
                {
                    cmpanyId = 0 + cmpanyId;
                }
                if (string.IsNullOrEmpty(cmpanyId)) { cmpanyId = string.Empty; }
                string Query = $@"SELECT 
                          PS.COMPANY_CODE,cs.COMPANY_EDESC,BS.BRANCH_EDESC,PS.BRANCH_CODE,PS.FY_START_DATE,PS.FY_END_DATE,PS.DEFAULT_CURRENCY_CODE,PS.EXCHANGE_RATE,PS.FISCAL_YEAR,
                          PS.CASH_ACC_CODE,PS.METHOD,PS.PRINT_TYPE,PS.PASS_EXPIRY_DAYS,PS.DECIMAL_PLACE,PS.FREQUENT_NO,PS.ALERT_REFRESH_RATE,PS.SECURED_BACK_DAYS,TO_CHAR(PS.FREEZE_ENTRY_DATE),
                          PS.SUB_NARRATION_FLAG,PS.NEGATIVE_STOCK_FLAG,PS.REFERENCE_EDIT_FLAG,PS.CUSTOMER_APPROVAL_FLAG,PS.MASTER_MODIFY,PS.SUPPLIER_APPROVAL_FLAG,
                          PS.RATE3_INFO_FLAG,PS.FREEZE_PUR_EXP_OPTION,PS.SUBLEDGER_BILL_ADJUST_FLAG,PS.REFERENCE_TRAN_FLAG,PS.BRANCH_BCMAP_FLAG,PS.STOCK_GENERIC_FLAG,
                          PS.GL_FLAG, PS.SL_FLAG,PS.AUTO_LEDGER_FLAG,PS.AUTO_CUSTOMER_FLAG,PS. AUTO_SUPPLIER_FLAG,PS.AUTO_ITEM_FLAG,PS.GB_FLAG,PS.TB_GENERIC_FLAG,
                          PSS.COST_BUDGET_INFO_FLAG,PSS.COA_BUDGET_CONTROL_FLAG,PSS.COA_BUDGET_INFO_FLAG,PSS.USER_TARGET_CONTROL_FLAG,PSS.TRANS_ALERT_CAPTURE_FLAG,PSS.PARTY_ACTIVE_INFO_FLAG,
                          PSS.LEDGER_VAT_FLAG,PSS.COST_CATEGORY_FLAG,PSS.SUBLEDGER_ORDER_FLAG,PSS.AUTO_GENERATE_LS_FLAG,PSS.TB_SUB_LEDGER_LOAD,PSS.IND_VOUCHER_VERIFY,PSS.PRINT_DATE_TIME,
                          PSS.TABLE_NOT_UNPOST_FLAG,PSS.VAT_DR_CR_ENTRY,PSS.PP_DETAIL_ENTRY,PSS.DOCUMENT_HISTORY_FLAG,PSS.DIVISION_FLAG,PSS.BILL_QTY_FLAG,PSS.EXCHANGE_RATE_EDITABLE_FLAG,
                          PSS.ORDER_TAXABLE_RATE,PSS.VNO_AS_SESSION_ID_CONTROL,PSS.DEALER_SYSTEM_FLAG,PSS.DEFAULT_DIVISION,PSS.DEFAULT_BRANCH_CONSOLE,PSS.PUR_RATE_VERIANCE_FLAG,
                          PSS.CASH_BANK_NEGATIVE_FLAG,PSS.SALES_CUPON_SCHEME_FLAG,PSS.AFP_BUDGET_CONTROL_FLAG,PSS.MASTER_FIELD_MANDATORY_FLAG,PSS.PAN_VAT_CONTROL_FLAG,
                          PSS.SYNC_WITH_IRD,PSS.LS_BACK_ACCESS_DAYS,PSS.RATE_SCHEDULE_TYPE,PSS.CREDIT_CONTROL_FLAG,PSS.DEFAULT_COGS_VALUE,PSS.FREIGHT_RATE_FLAG,PSS.FREIGHT_NEGATIVE_FLAG
                          FROM preference_setup PS
                          INNER JOIN PREFERENCE_SUB_SETUP PSS ON PS.COMPANY_CODE = PSS.COMPANY_CODE AND PS.BRANCH_CODE = PSS.BRANCH_CODE
                          INNER JOIN company_setup CS ON PS.COMPANY_CODE=CS.COMPANY_CODE 
                          INNER JOIN FA_BRANCH_SETUP BS ON PS.BRANCH_CODE=BS.BRANCH_CODE 
                          WHERE PS.COMPANY_CODE='{cmpanyId}'  ORDER BY TO_NUMBER(PS.COMPANY_CODE) ASC";
                PreferenceModel entity = this._dbContext.SqlQuery<PreferenceModel>(Query).FirstOrDefault();

                return entity;

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        //preffcompany
        public List<CompanySetupModel> getCompanyPreff()
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                string query = $@"SELECT DISTINCT 
                        INITCAP(COMPANY_EDESC) AS COMPANY_EDESC,
                        COMPANY_CODE ,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,ABBR_CODE
                        FROM COMPANY_SETUP
                        WHERE DELETED_FLAG = 'N' 
                        ORDER BY COMPANY_EDESC";
                var result = _dbContext.SqlQuery<CompanySetupModel>(query).ToList();
                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public List<BranchSetupModel> getBranchPreff(string COMPANY_CODE)
        {
            if (string.IsNullOrWhiteSpace(COMPANY_CODE))
            {
                var result = _dbContext.SqlQuery<BranchSetupModel>($"SELECT BRANCH_EDESC,BRANCH_CODE  FROM FA_BRANCH_SETUP WHERE DELETED_FLAG= 'N' AND GROUP_SKU_FLAG = 'I'").ToList();
                return result;
            }
            else
            {
                var result = _dbContext.SqlQuery<BranchSetupModel>($"SELECT BRANCH_EDESC,BRANCH_CODE,COMPANY_CODE FROM FA_BRANCH_SETUP WHERE DELETED_FLAG= 'N' AND GROUP_SKU_FLAG = 'I' AND COMPANY_CODE IN ('{COMPANY_CODE}')").ToList();
                return result;
            }
        }


        public List<CurrencymultiModel> getAllCurrencymulti()
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                string query = $@"SELECT DISTINCT 
                        INITCAP(CURRENCY_EDESC) AS CURRENCY_EDESC,
                        CURRENCY_CODE,COUNTRY,CURRENCY_SYMBOL 
                        FROM currency_setup
                        WHERE DELETED_FLAG = 'N' 
                        AND COMPANY_CODE='{company_code}'
                        ORDER BY CURRENCY_EDESC";
                var currencyList = _dbContext.SqlQuery<CurrencymultiModel>(query).ToList();
                return currencyList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        //preferencesetup 
        public List<PreferenceModel> getAllPreference()
        {
            try
            {
                string query = $@" SELECT 
                          PS.COMPANY_CODE,cs.COMPANY_EDESC,BS.BRANCH_EDESC,PS.BRANCH_CODE,PS.FY_START_DATE,PS.FY_END_DATE,PS.DEFAULT_CURRENCY_CODE,PS.EXCHANGE_RATE,PS.FISCAL_YEAR,
                          PS.CASH_ACC_CODE,PS.METHOD,PS.PRINT_TYPE,PS.NEGATIVE_STOCK_FLAG,PS.PASS_EXPIRY_DAYS,PS.DECIMAL_PLACE,PS.FREQUENT_NO,PS.ALERT_REFRESH_RATE,PS.SECURED_BACK_DAYS,
                          PS.SUB_NARRATION_FLAG,PS.REFERENCE_EDIT_FLAG,PS.CUSTOMER_APPROVAL_FLAG,PS.MASTER_MODIFY,PS.SUPPLIER_APPROVAL_FLAG,PS.BRANCH_SCMAP_FLAG,
                          PSS.FREIGHT_NEGATIVE_FLAG,PSS.LEDGER_VAT_FLAG,PSS.COST_CATEGORY_FLAG,PSS.DOCUMENT_HISTORY_FLAG,PSS.DIVISION_FLAG,PSS.ORDER_TAXABLE_RATE,
                          PSS.EXCHANGE_RATE_EDITABLE_FLAG,PSS.VNO_AS_SESSION_ID_CONTROL,PSS.DEALER_SYSTEM_FLAG,PSS.PAN_VAT_CONTROL_FLAG,PSS.CREDIT_CONTROL_FLAG,PSS.CREDIT_CONTROL_AS,PSS.CREDIT_CONTROL_AT
                         FROM preference_setup PS
                         INNER JOIN PREFERENCE_SUB_SETUP PSS ON PS.COMPANY_CODE = PSS.COMPANY_CODE AND PS.BRANCH_CODE = PSS.BRANCH_CODE
                          INNER JOIN company_setup CS ON PS.COMPANY_CODE=CS.COMPANY_CODE 
                           INNER JOIN FA_BRANCH_SETUP BS ON PS.BRANCH_CODE=BS.BRANCH_CODE 
                         WHERE PS.DELETED_FLAG = 'N'";
                var companyList = _dbContext.SqlQuery<PreferenceModel>(query).ToList();
                return companyList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        //update preff
        public string updatePreferenceSetup(PreferenceModel model, User userInfo)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var preffquery = $@"UPDATE  PREFERENCE_SETUP  SET COMPANY_CODE='{model.COMPANY_CODE}',BRANCH_CODE='{model.BRANCH_CODE}',
                                        FY_START_DATE=TO_DATE('{model.FY_START_DATE.ToString("MM/dd/yyyy")}', 'MM/dd/yyyy'),FY_END_DATE=TO_DATE('{model.FY_END_DATE.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),
                                        DEFAULT_CURRENCY_CODE='{model.DEFAULT_CURRENCY_CODE}',EXCHANGE_RATE='{model.EXCHANGE_RATE}',CASH_ACC_CODE='{model.CASH_ACC_CODE}',METHOD='{model.METHOD}',
                                        PRINT_TYPE='{model.PRINT_TYPE}',PASS_EXPIRY_DAYS='{model.PASS_EXPIRY_DAYS}',FISCAL_YEAR='{model.FISCAL_YEAR}',MODIFY_DATE=TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),
                                        FREQUENT_NO='{model.FREQUENT_NO}',ALERT_REFRESH_RATE='{model.ALERT_REFRESH_RATE}',SECURED_BACK_DAYS='{model.SECURED_BACK_DAYS}',DECIMAL_PLACE='{model.DECIMAL_PLACE}' ,
                                        NEGATIVE_STOCK_FLAG='{model.NEGATIVE_STOCK_FLAG}',SUB_NARRATION_FLAG='{model.SUB_NARRATION_FLAG}',REFERENCE_EDIT_FLAG='{model.REFERENCE_EDIT_FLAG}',
                                        CUSTOMER_APPROVAL_FLAG='{model.CUSTOMER_APPROVAL_FLAG}',MASTER_MODIFY='{model.MASTER_MODIFY}',SUPPLIER_APPROVAL_FLAG='{model.SUPPLIER_APPROVAL_FLAG}',
                                        RATE3_INFO_FLAG='{model.RATE3_INFO_FLAG}',FREEZE_PUR_EXP_OPTION='{model.FREEZE_PUR_EXP_OPTION}',SUBLEDGER_BILL_ADJUST_FLAG='{model.SUBLEDGER_BILL_ADJUST_FLAG}',
                                        REFERENCE_TRAN_FLAG='{model.REFERENCE_TRAN_FLAG}',BRANCH_BCMAP_FLAG='{model.BRANCH_BCMAP_FLAG}',STOCK_GENERIC_FLAG='{model.STOCK_GENERIC_FLAG}',
                                        GL_FLAG='{model.GL_FLAG}',SL_FLAG='{model.SL_FLAG}',AUTO_LEDGER_FLAG='{model.AUTO_LEDGER_FLAG}',
                                        AUTO_CUSTOMER_FLAG='{model.AUTO_CUSTOMER_FLAG}',AUTO_SUPPLIER_FLAG='{model.AUTO_SUPPLIER_FLAG}',AUTO_ITEM_FLAG='{model.AUTO_ITEM_FLAG}',GB_FLAG='{model.GB_FLAG}',
                                        TB_GENERIC_FLAG='{model.TB_GENERIC_FLAG}',  BRANCH_SCMAP_FLAG='{model.BRANCH_SCMAP_FLAG}',FREEZE_ENTRY_DATE=TO_DATE('{model.FREEZE_ENTRY_DATE.ToString("MM/dd/yyyy")}', 'MM/dd/yyyy')
                                        WHERE BRANCH_CODE='{model.BRANCH_CODE}'";
                    var insertpreff = _coreEntity.ExecuteSqlCommand(preffquery);


                    var subpreff = $@"UPDATE  PREFERENCE_SUB_SETUP SET COMPANY_CODE='{model.COMPANY_CODE}',BRANCH_CODE='{model.BRANCH_CODE}',COST_BUDGET_INFO_FLAG='{model.COST_BUDGET_INFO_FLAG}',
                                      COA_BUDGET_CONTROL_FLAG='{model.COA_BUDGET_CONTROL_FLAG}',COA_BUDGET_INFO_FLAG='{model.COA_BUDGET_INFO_FLAG}',USER_TARGET_CONTROL_FLAG='{model.USER_TARGET_CONTROL_FLAG}',
                                      TRANS_ALERT_CAPTURE_FLAG='{model.TRANS_ALERT_CAPTURE_FLAG}',PARTY_ACTIVE_INFO_FLAG='{model.PARTY_ACTIVE_INFO_FLAG}',LEDGER_VAT_FLAG='{model.LEDGER_VAT_FLAG}',
                                      COST_CATEGORY_FLAG='{model.COST_CATEGORY_FLAG}',SUBLEDGER_ORDER_FLAG='{model.SUBLEDGER_ORDER_FLAG}',AUTO_GENERATE_LS_FLAG='{model.AUTO_GENERATE_LS_FLAG}',
                                      TB_SUB_LEDGER_LOAD='{model.TB_SUB_LEDGER_LOAD}',IND_VOUCHER_VERIFY='{model.IND_VOUCHER_VERIFY}',PRINT_DATE_TIME='{model.PRINT_DATE_TIME}',
                                      TABLE_NOT_UNPOST_FLAG='{model.TABLE_NOT_UNPOST_FLAG}',VAT_DR_CR_ENTRY='{model.VAT_DR_CR_ENTRY}',PP_DETAIL_ENTRY='{model.PP_DETAIL_ENTRY}',
                                      DOCUMENT_HISTORY_FLAG='{model.DOCUMENT_HISTORY_FLAG}',DIVISION_FLAG='{model.DIVISION_FLAG}',BILL_QTY_FLAG='{model.BILL_QTY_FLAG}',
                                      EXCHANGE_RATE_EDITABLE_FLAG='{model.EXCHANGE_RATE_EDITABLE_FLAG}',ORDER_TAXABLE_RATE='{model.ORDER_TAXABLE_RATE}',VNO_AS_SESSION_ID_CONTROL='{model.VNO_AS_SESSION_ID_CONTROL}',
                                      DEALER_SYSTEM_FLAG='{model.DEALER_SYSTEM_FLAG}',DEFAULT_DIVISION='{model.DEFAULT_DIVISION}',DEFAULT_BRANCH_CONSOLE='{model.DEFAULT_BRANCH_CONSOLE}',
                                      PUR_RATE_VERIANCE_FLAG='{model.PUR_RATE_VERIANCE_FLAG}',CASH_BANK_NEGATIVE_FLAG='{model.CASH_BANK_NEGATIVE_FLAG}',SALES_CUPON_SCHEME_FLAG='{model.SALES_CUPON_SCHEME_FLAG}',
                                      AFP_BUDGET_CONTROL_FLAG='{model.AFP_BUDGET_CONTROL_FLAG}',MASTER_FIELD_MANDATORY_FLAG='{model.MASTER_FIELD_MANDATORY_FLAG}',PAN_VAT_CONTROL_FLAG='{model.PAN_VAT_CONTROL_FLAG}',
                                      SYNC_WITH_IRD='{model.SYNC_WITH_IRD}',LS_BACK_ACCESS_DAYS='{model.LS_BACK_ACCESS_DAYS}',RATE_SCHEDULE_TYPE='{model.RATE_SCHEDULE_TYPE}',
                                      CREDIT_CONTROL_FLAG='{model.CREDIT_CONTROL_FLAG}',DEFAULT_COGS_VALUE='{model.DEFAULT_COGS_VALUE}',FREIGHT_RATE_FLAG='{model.FREIGHT_RATE_FLAG}',FREIGHT_NEGATIVE_FLAG='{model.FREIGHT_NEGATIVE_FLAG}'
                                      WHERE BRANCH_CODE='{model.BRANCH_CODE}'";
                    var insertsubpreff = _coreEntity.ExecuteSqlCommand(subpreff);


                    trans.Commit();
                    return "UPDATED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        //CREATE PREFERENCE SETUP
        public string createNewPreferenceSetup(PreferenceModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var preffquery = $@"INSERT INTO PREFERENCE_SETUP (COMPANY_CODE,BRANCH_CODE,FY_START_DATE,FY_END_DATE,DEFAULT_CURRENCY_CODE,EXCHANGE_RATE,CASH_ACC_CODE,METHOD,PRINT_TYPE,
                                       PASS_EXPIRY_DAYS,FISCAL_YEAR,CREATED_BY,CREATED_DATE,DELETED_FLAG,SYSTEM_NAV_TITLE,DEFINITION_TITLE,SETUP_TITLE,TRANSACTION_TITLE,REPORT_TITLE,PREFERENCE_TITLE,
                                       DECIMAL_PLACE,NEGATIVE_STOCK_FLAG,SUB_NARRATION_FLAG,REFERENCE_EDIT_FLAG,CUSTOMER_APPROVAL_FLAG,MASTER_MODIFY,SUPPLIER_APPROVAL_FLAG,FREQUENT_NO,
                                       ALERT_REFRESH_RATE,SECURED_BACK_DAYS,RATE3_INFO_FLAG,FREEZE_PUR_EXP_OPTION,SUBLEDGER_BILL_ADJUST_FLAG,REFERENCE_TRAN_FLAG,BRANCH_BCMAP_FLAG,STOCK_GENERIC_FLAG, 
                                      GL_FLAG, SL_FLAG,AUTO_LEDGER_FLAG,AUTO_CUSTOMER_FLAG,AUTO_SUPPLIER_FLAG, AUTO_ITEM_FLAG,GB_FLAG,TB_GENERIC_FLAG,BRANCH_SCMAP_FLAG,FREEZE_ENTRY_DATE)
                                       VALUES('{model.COMPANY_CODE}','{model.BRANCH_CODE}',TO_DATE('{model.FY_START_DATE.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),TO_DATE('{model.FY_END_DATE.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),
                                       '{model.DEFAULT_CURRENCY_CODE}','{model.EXCHANGE_RATE}','{model.CASH_ACC_CODE}','{model.METHOD}','{model.PRINT_TYPE}','{model.PASS_EXPIRY_DAYS}','{model.FISCAL_YEAR}',
                                      '{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','System','Defination','Setup','Transaction','Report','Preference','{model.DECIMAL_PLACE}',
                                      '{model.NEGATIVE_STOCK_FLAG}','{model.SUB_NARRATION_FLAG}','{model.REFERENCE_EDIT_FLAG}','{model.CUSTOMER_APPROVAL_FLAG}','{model.MASTER_MODIFY}','{model.SUPPLIER_APPROVAL_FLAG}',
                                      '{model.FREQUENT_NO}','{model.ALERT_REFRESH_RATE}','{model.SECURED_BACK_DAYS}','{model.RATE3_INFO_FLAG}','{model.FREEZE_PUR_EXP_OPTION}','{model.SUBLEDGER_BILL_ADJUST_FLAG}'
                                      ,'{model.REFERENCE_TRAN_FLAG}','{model.BRANCH_BCMAP_FLAG}','{model.STOCK_GENERIC_FLAG}',
                                           '{model.GL_FLAG}','{model.SL_FLAG}','{model.AUTO_LEDGER_FLAG}','{model.AUTO_CUSTOMER_FLAG}','{model.AUTO_SUPPLIER_FLAG}','{model.BRANCH_SCMAP_FLAG}'
                                         '{model.AUTO_ITEM_FLAG}','{model.GB_FLAG}','{model.TB_GENERIC_FLAG}',TO_DATE('{model.FREEZE_ENTRY_DATE.ToString("MM/dd/yyyy")}', 'MM/dd/yyyy'))";
                    var insertpreff = _coreEntity.ExecuteSqlCommand(preffquery);


                    var subpreff = $@"INSERT INTO PREFERENCE_SUB_SETUP (COMPANY_CODE,BRANCH_CODE,COST_BUDGET_INFO_FLAG,COA_BUDGET_CONTROL_FLAG,COA_BUDGET_INFO_FLAG,USER_TARGET_CONTROL_FLAG,
                                      TRANS_ALERT_CAPTURE_FLAG,PARTY_ACTIVE_INFO_FLAG,LEDGER_VAT_FLAG,COST_CATEGORY_FLAG,SUBLEDGER_ORDER_FLAG,AUTO_GENERATE_LS_FLAG,TB_SUB_LEDGER_LOAD,IND_VOUCHER_VERIFY,
                                      PRINT_DATE_TIME,TABLE_NOT_UNPOST_FLAG,VAT_DR_CR_ENTRY,PP_DETAIL_ENTRY,DOCUMENT_HISTORY_FLAG,DIVISION_FLAG,BILL_QTY_FLAG,EXCHANGE_RATE_EDITABLE_FLAG,ORDER_TAXABLE_RATE,
                                      VNO_AS_SESSION_ID_CONTROL,DEALER_SYSTEM_FLAG,DEFAULT_DIVISION,DEFAULT_BRANCH_CONSOLE,PUR_RATE_VERIANCE_FLAG,CASH_BANK_NEGATIVE_FLAG,SALES_CUPON_SCHEME_FLAG,
                                      AFP_BUDGET_CONTROL_FLAG,MASTER_FIELD_MANDATORY_FLAG,PAN_VAT_CONTROL_FLAG,SYNC_WITH_IRD,LS_BACK_ACCESS_DAYS,RATE_SCHEDULE_TYPE,CREDIT_CONTROL_FLAG,DEFAULT_COGS_VALUE,
                                      FREIGHT_RATE_FLAG,FREIGHT_NEGATIVE_FLAG)
                                      VALUES('{model.COMPANY_CODE}','{model.BRANCH_CODE}','{model.COST_BUDGET_INFO_FLAG}','{model.COA_BUDGET_CONTROL_FLAG}','{model.COA_BUDGET_INFO_FLAG}',
                                     '{model.USER_TARGET_CONTROL_FLAG}','{model.TRANS_ALERT_CAPTURE_FLAG}','{model.PARTY_ACTIVE_INFO_FLAG}','{model.LEDGER_VAT_FLAG}','{model.COST_CATEGORY_FLAG}',
                                     '{model.SUBLEDGER_ORDER_FLAG}','{model.AUTO_GENERATE_LS_FLAG}','{model.TB_SUB_LEDGER_LOAD}','{model.IND_VOUCHER_VERIFY}','{model.PRINT_DATE_TIME}',
                                     '{model.TABLE_NOT_UNPOST_FLAG}','{model.VAT_DR_CR_ENTRY}','{model.PP_DETAIL_ENTRY}','{model.DOCUMENT_HISTORY_FLAG}','{model.DIVISION_FLAG}','{model.BILL_QTY_FLAG}','{model.DIVISION_FLAG}',
                                     '{model.EXCHANGE_RATE_EDITABLE_FLAG}','{model.ORDER_TAXABLE_RATE}','{model.VNO_AS_SESSION_ID_CONTROL}','{model.DEALER_SYSTEM_FLAG}',
                                     '{model.DEFAULT_BRANCH_CONSOLE}','{model.PUR_RATE_VERIANCE_FLAG}','{model.CASH_BANK_NEGATIVE_FLAG}','{model.SALES_CUPON_SCHEME_FLAG}','{model.AFP_BUDGET_CONTROL_FLAG}',
                                     '{model.MASTER_FIELD_MANDATORY_FLAG}','{model.PAN_VAT_CONTROL_FLAG}','{model.SYNC_WITH_IRD}','{model.LS_BACK_ACCESS_DAYS}','{model.RATE_SCHEDULE_TYPE}',
                                      '{model.CREDIT_CONTROL_FLAG}','{model.DEFAULT_COGS_VALUE}','{model.FREIGHT_RATE_FLAG}','{model.FREIGHT_NEGATIVE_FLAG}')";
                    var insertsubpreff = _coreEntity.ExecuteSqlCommand(subpreff);


                    trans.Commit();
                    return "INSERTED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        //delete prefeerence Setup
        public string DeletepreffBybranchCode(string companyCode)
        {

            if (!companyCode.StartsWith("0"))
            {
                companyCode = 0 + companyCode;
            }
            try
            {

                if (string.IsNullOrEmpty(companyCode)) { companyCode = string.Empty; }


                var sqlquery = $@"UPDATE preference_setup SET DELETED_FLAG = 'Y' WHERE BRANCH_CODE='{companyCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                return "DELETED";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<PreferenceModel> getFormLoad(User userInfo)
        {
            var company_code = _workContext.CurrentUserinformation.company_code;
            string query = $@"SELECT * FROM  (SELECT 
                          PS.COMPANY_CODE,PS.BRANCH_CODE,PS.FY_START_DATE,PS.FY_END_DATE,PS.DEFAULT_CURRENCY_CODE,PS.EXCHANGE_RATE,PS.FISCAL_YEAR,
                          PS.CASH_ACC_CODE,PS.METHOD,PS.PRINT_TYPE,PS.NEGATIVE_STOCK_FLAG,PS.PASS_EXPIRY_DAYS,PS.DECIMAL_PLACE,PS.FREQUENT_NO,PS.ALERT_REFRESH_RATE,PS.SECURED_BACK_DAYS,
                          PS.SUB_NARRATION_FLAG,PS.REFERENCE_EDIT_FLAG,PS.CUSTOMER_APPROVAL_FLAG,PS.MASTER_MODIFY,PS.SUPPLIER_APPROVAL_FLAG,
                          PSS.FREIGHT_NEGATIVE_FLAG,PSS.LEDGER_VAT_FLAG,PSS.COST_CATEGORY_FLAG,PSS.DOCUMENT_HISTORY_FLAG,PSS.DIVISION_FLAG,PSS.ORDER_TAXABLE_RATE,
                          PSS.EXCHANGE_RATE_EDITABLE_FLAG,PSS.VNO_AS_SESSION_ID_CONTROL,PSS.DEALER_SYSTEM_FLAG,PSS.PAN_VAT_CONTROL_FLAG,PSS.CREDIT_CONTROL_FLAG,PSS.CREDIT_CONTROL_AS,PSS.CREDIT_CONTROL_AT
                         FROM preference_setup PS
                         INNER JOIN PREFERENCE_SUB_SETUP PSS ON PS.COMPANY_CODE = PSS.COMPANY_CODE AND PS.BRANCH_CODE = PSS.BRANCH_CODE
                         WHERE PS.DELETED_FLAG = 'N' 
                         ORDER BY PS.CREATED_DATE DESC)
                         WHERE ROWNUM = 1";
            var result = _dbContext.SqlQuery<PreferenceModel>(query).ToList();

            return result;

        }
        #endregion

        #region Company Setup
        //Create Cmpany
        public string createNewCompanySetup(CompanySetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var newcompanycode = string.Empty;

                    var newmaxlocationcodequery = $@"SELECT COUNT(COMPANY_CODE)+1 as MAX_COMPANY_CODE FROM COMPANY_SETUP";
                    newcompanycode = this._coreEntity.SqlQuery<int>(newmaxlocationcodequery).FirstOrDefault().ToString();

                    if (!newcompanycode.StartsWith("0"))
                    {
                        newcompanycode = 0 + newcompanycode;
                    }



                    var childsqlquery = $@"INSERT INTO COMPANY_SETUP (COMPANY_CODE,COMPANY_EDESC,COMPANY_NDESC,ADDRESS,TELEPHONE,EMAIL,REMARKS,FAX,WEB,TPIN_VAT_NO,FOOTER_LOGO_FILE_NAME,ABBR_CODE,REGISTRATION_NO,LOGO_FILE_NAME,VALID_DATE,CREATED_BY,CREATED_DATE,DELETED_FLAG,PRE_COMPANY_CODE)
                                        VALUES('{newcompanycode}','{model.COMPANY_EDESC}','{model.COMPANY_NDESC}','{model.ADDRESS}','{model.TELEPHONE}','{model.EMAIL}','{model.REMARKS}','{model.FAX}','{model.WEB}','{model.TPIN_VAT_NO}','{model.FOOTER_LOGO_FILE_NAME}','{model.ABBR_CODE}','{model.REGISTRATION_NO}','{model.LOGO_FILE_NAME}',TO_DATE('{model.VALID_DATE.Value.ToString("MM/dd/yyyy")}', 'MM/dd/yyyy'),'{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','00')";
                    var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);

                    trans.Commit();
                    return "INSERTED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        //company update setup

        public string updateCompanySetup(CompanySetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {


                    var sqlquery = $@"UPDATE COMPANY_SETUP SET COMPANY_EDESC='{model.COMPANY_EDESC}', COMPANY_NDESC='{model.COMPANY_NDESC}',ADDRESS='{model.ADDRESS}', TELEPHONE='{model.TELEPHONE}',EMAIL='{model.EMAIL}', REMARKS='{model.REMARKS}',FAX='{model.FAX}', WEB='{model.WEB}',VALID_DATE=TO_DATE('{model.VALID_DATE.Value.ToString("MM/dd/yyyy")}', 'MM/dd/yyyy'), TPIN_VAT_NO='{model.TPIN_VAT_NO}',FOOTER_LOGO_FILE_NAME='{model.FOOTER_LOGO_FILE_NAME}', ABBR_CODE='{model.ABBR_CODE}',REGISTRATION_NO='{model.REGISTRATION_NO}', LOGO_FILE_NAME='{model.LOGO_FILE_NAME}'   WHERE COMPANY_CODE = '{model.COMPANY_CODE}'";
                    var result = _coreEntity.ExecuteSqlCommand(sqlquery);
                    trans.Commit();
                    return "UPDATED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }

        //delete company

        public string DeleteCompanyByCompanyCode(string companyCode)
        {

            if (!companyCode.StartsWith("0"))
            {
                companyCode = 0 + companyCode;
            }
            try
            {

                if (string.IsNullOrEmpty(companyCode)) { companyCode = string.Empty; }


                var sqlquery = $@"UPDATE COMPANY_SETUP SET DELETED_FLAG = 'Y' WHERE COMPANY_CODE='{companyCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                return "DELETED";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public CompanySetupModel GetCompnyDetailByCompanyCode(string cmpanyId)
        {
            try
            {
                if (!cmpanyId.StartsWith("0"))
                {
                    cmpanyId = 0 + cmpanyId;
                }
                if (string.IsNullOrEmpty(cmpanyId)) { cmpanyId = string.Empty; }
                string Query = $@"SELECT COMPANY_CODE,COMPANY_EDESC,COMPANY_NDESC,TELEPHONE,REMARKS,ADDRESS,CREATED_DATE,EMAIL,SYN_ROWID,ABBR_CODE,FAX,WEB,VALID_DATE,TPIN_VAT_NO,FOOTER_LOGO_FILE_NAME,REGISTRATION_NO,LOGO_FILE_NAME,CONSOLIDATE_FLAG from COMPANY_SETUP WHERE COMPANY_CODE='{cmpanyId}'  ORDER BY TO_NUMBER(COMPANY_CODE) ASC";
                CompanySetupModel entity = this._dbContext.SqlQuery<CompanySetupModel>(Query).FirstOrDefault();
                entity.LOGO_FILE_NAME = "/Areas/NeoERP.DocumentTemplate/images/" + entity.LOGO_FILE_NAME;
                entity.FOOTER_LOGO_FILE_NAME = "/Areas/NeoERP.DocumentTemplate/images/" + entity.FOOTER_LOGO_FILE_NAME;



                return entity;

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public List<CompanySetupModel> getAllCompanychild()
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                string query = $@"SELECT DISTINCT 
                        INITCAP(COMPANY_EDESC) AS COMPANY_EDESC,
                        COMPANY_CODE ,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,ABBR_CODE
                        TELEPHONE, FAX, ADDRESS,EMAIL,REMARKS,WEB,REGISTRATION_NO,VALID_DATE,TPIN_VAT_NO
                        FROM COMPANY_SETUP
                        WHERE DELETED_FLAG = 'N' 
                        ORDER BY COMPANY_EDESC";
                var companyList = _dbContext.SqlQuery<CompanySetupModel>(query).ToList();
                return companyList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        #endregion

        #region Division Setup
        //Division Setup
        public List<DivisionSetupModel> getAllDivisionschild(string groupId)
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                string query = $@"SELECT DISTINCT 
                        INITCAP(DIVISION_EDESC) AS DIVISION_EDESC,
                        DIVISION_CODE ,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,
                        DIVISION_CODE AS MASTER_DIVISION_CODE, ADDRESS,EMAIL,REMARKS,GROUP_SKU_FLAG
                        FROM FA_DIVISION_SETUP
                        WHERE DELETED_FLAG = 'N' 
                        AND GROUP_SKU_FLAG='I'
                        AND PRE_DIVISION_CODE = '{groupId}'
                        AND COMPANY_CODE='{company_code}'
                        ORDER BY DIVISION_EDESC";
                var branchCenterCodeList = _dbContext.SqlQuery<DivisionSetupModel>(query).ToList();
                return branchCenterCodeList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public List<DivisionSetupModel> getAllDivisionsList(string searchText)
        {
            try
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    string query = $@"SELECT DISTINCT 
                        INITCAP(DIVISION_EDESC) AS DIVISION_EDESC,
                        DIVISION_CODE ,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,
                        DIVISION_CODE AS MASTER_DIVISION_CODE, ADDRESS,EMAIL,REMARKS,GROUP_SKU_FLAG
                        FROM FA_DIVISION_SETUP
                        WHERE DELETED_FLAG = 'N'
                        AND COMPANY_CODE='{company_code}'
                        AND UPPER(DIVISION_EDESC) LIKE UPPER('%{ searchText }%')
                        ORDER BY DIVISION_EDESC";
                    var branchCenterCodeList = _dbContext.SqlQuery<DivisionSetupModel>(query).ToList();
                    return branchCenterCodeList;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public string DeleteDivisionCenterByDivisionCode(string divisionCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(divisionCode)) { divisionCode = string.Empty; }

                var masterQry = $@"SELECT DIVISION_CODE, GROUP_SKU_FLAG FROM FA_DIVISION_SETUP WHERE DIVISION_CODE = '{divisionCode}' and COMPANY_CODE= '{companyCode}'";
                var masterBranchCode = _dbContext.SqlQuery<DivisionSetupModel>(masterQry).FirstOrDefault();
                if (masterBranchCode.GROUP_SKU_FLAG == "G")
                {
                    var childQry = $@"SELECT COUNT(*) FROM FA_DIVISION_SETUP WHERE PRE_DIVISION_CODE like ('{masterBranchCode.DIVISION_CODE}%') AND DELETED_FLAG = 'N'";
                    var childResult = _dbContext.SqlQuery<int>(childQry).FirstOrDefault();
                    if (childResult > 0)
                    {
                        return "HAS_CHILD";
                    }
                }
                var sqlquery = $@"UPDATE FA_DIVISION_SETUP SET DELETED_FLAG = 'Y' WHERE DIVISION_CODE='{divisionCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                return "DELETED";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //new create
        public string createNewDivisionSetup(DivisionSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var newmaxlocationcode = string.Empty;
                    var newmaxlocationcodequery = $@"SELECT COUNT(DIVISION_CODE)+1 as MAX_DIVISION_CODE FROM FA_DIVISION_SETUP WHERE PRE_DIVISION_CODE = '{model.DIVISION_CODE}'";
                    newmaxlocationcode = this._coreEntity.SqlQuery<int>(newmaxlocationcodequery).FirstOrDefault().ToString();

                    if (model.DIVISION_CODE != null && model.PRE_DIVISION_CODE != "")
                    {
                        var maxPreCode = string.Empty;
                        var newprecode = string.Empty;
                        var newmastercode = string.Empty;

                        if (model.DIVISION_CODE != null && model.DIVISION_CODE != "")
                        {
                            var maxprecodequery = $@"select (count(*) + 1) as MAXCODE from FA_DIVISION_SETUP where PRE_DIVISION_CODE like '{model.DIVISION_CODE}' and company_code = '{_workContext.CurrentUserinformation.company_code}'";
                            maxPreCode = this._coreEntity.SqlQuery<int>(maxprecodequery).FirstOrDefault().ToString();
                            if (maxPreCode != null)
                            {
                                if (Convert.ToInt32(maxPreCode) <= 9)
                                {
                                    maxPreCode = "0" + maxPreCode.ToString();
                                }

                            }
                            newprecode = model.PRE_DIVISION_CODE;
                            newmastercode = model.PRE_DIVISION_CODE + "." + maxPreCode;

                            var childsqlquery = $@"INSERT INTO FA_DIVISION_SETUP (DIVISION_CODE,DIVISION_EDESC,ADDRESS,TELEPHONE_NO,EMAIL,REMARKS,GROUP_SKU_FLAG,PRE_DIVISION_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SYN_ROWID) VALUES('{newmastercode}','{model.DIVISION_EDESC}','{model.ADDRESS}','{model.TELEPHONE_NO}','{model.EMAIL}','{model.REMARKS}','{model.GROUP_SKU_FLAG}','{newprecode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','N')";
                            var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);




                        }

                    }
                    else
                    {
                        var newpre = string.Empty;
                        var newmaster = string.Empty;
                        var newprequery = $@"SELECT NVL(max(REGEXP_SUBSTR(DIVISION_CODE, '[^.]+', 1, 1)),0)+1 col_one FROM FA_DIVISION_SETUP";
                        newpre = this._coreEntity.SqlQuery<int>(newprequery).FirstOrDefault().ToString();
                        if (Convert.ToInt32(newpre) <= 9)
                        {
                            newpre = "0" + newpre.ToString();
                        }
                        if (model.GROUP_SKU_FLAG == "G")
                        {
                            newmaster = newpre + ".01";
                        }

                        var rootsqlquery = $@"INSERT INTO FA_DIVISION_SETUP (DIVISION_CODE,DIVISION_EDESC,ADDRESS,TELEPHONE_NO,EMAIL,REMARKS,GROUP_SKU_FLAG,PRE_DIVISION_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SYN_ROWID) VALUES('{newmaster}','{model.DIVISION_EDESC}','{model.ADDRESS}','{model.TELEPHONE_NO}','{model.EMAIL}','{model.REMARKS}','{model.GROUP_SKU_FLAG}','00','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','N')";
                        var insertroot1 = _coreEntity.ExecuteSqlCommand(rootsqlquery);
                    }
                    trans.Commit();
                    return "INSERTED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }

        //update Divsison
        public string udpateDivisionSetup(DivisionSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var companyCode = _workContext.CurrentUserinformation.company_code;

                    var sqlquery = $@"UPDATE FA_DIVISION_SETUP SET DIVISION_EDESC='{model.DIVISION_EDESC}',ADDRESS='{model.ADDRESS}', TELEPHONE_NO='{model.TELEPHONE_NO}',EMAIL='{model.EMAIL}', REMARKS='{model.REMARKS}'  WHERE DIVISION_CODE = '{model.DIVISION_CODE}'";
                    var result = _coreEntity.ExecuteSqlCommand(sqlquery);
                    trans.Commit();
                    return "UPDATED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }


        public DivisionSetupModel GetDivisionCenterDetailByDivisionCode(string divisionCode)
        {
            try
            {
                if (string.IsNullOrEmpty(divisionCode)) { divisionCode = string.Empty; }
                string Query = $@"SELECT DIVISION_CODE,DIVISION_EDESC,GROUP_SKU_FLAG,PRE_DIVISION_CODE,REMARKS,ADDRESS,CREATED_DATE,TELEPHONE_NO,EMAIL,SYN_ROWID,ABBR_CODE from FA_DIVISION_SETUP WHERE DIVISION_CODE='{divisionCode}' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'   ORDER BY TO_NUMBER(DIVISION_CODE) ASC";
                DivisionSetupModel entity = this._dbContext.SqlQuery<DivisionSetupModel>(Query).FirstOrDefault();


                string parentbudgetQuery = $@"SELECT DIVISION_CODE FROM FA_DIVISION_SETUP where DIVISION_CODE = (SELECT PRE_DIVISION_CODE FROM FA_DIVISION_SETUP WHERE DIVISION_CODE='{divisionCode}' 
                                AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}')";
                entity.PARENT_DIVISION_CODE = this._dbContext.SqlQuery<string>(parentbudgetQuery).FirstOrDefault();

                return entity;

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        #endregion

        #region Branch Setup
        //Delete Branch
        public string DeleteBranchCenterByBranchCode(string branchCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(branchCode)) { branchCode = string.Empty; }

                var masterQry = $@"SELECT BRANCH_CODE, GROUP_SKU_FLAG FROM FA_BRANCH_SETUP WHERE BRANCH_CODE = '{branchCode}' and COMPANY_CODE= '{companyCode}'";
                var masterBranchCode = _dbContext.SqlQuery<BranchSetupModel>(masterQry).FirstOrDefault();
                if (masterBranchCode.GROUP_SKU_FLAG == "G")
                {
                    var childQry = $@"SELECT COUNT(*) FROM FA_BRANCH_SETUP WHERE PRE_BRANCH_CODE like ('{masterBranchCode.BRANCH_CODE}%') AND DELETED_FLAG = 'N'";
                    var childResult = _dbContext.SqlQuery<int>(childQry).FirstOrDefault();
                    if (childResult > 0)
                    {
                        return "HAS_CHILD";
                    }
                }
                var sqlquery = $@"UPDATE FA_BRANCH_SETUP SET DELETED_FLAG = 'Y' WHERE BRANCH_CODE='{branchCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                return "DELETED";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //update Company

        //Update Branch Setup
        public string udpateBranchSetup(BranchSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var companyCode = _workContext.CurrentUserinformation.company_code;

                    var sqlquery = $@"UPDATE FA_BRANCH_SETUP SET BRANCH_EDESC='{model.BRANCH_EDESC}', BRANCH_NDESC='{model.BRANCH_NDESC}',ADDRESS='{model.ADDRESS}', TELEPHONE_NO='{model.TELEPHONE_NO}',EMAIL='{model.EMAIL}', REMARKS='{model.REMARKS}'  WHERE BRANCH_CODE = '{model.BRANCH_CODE}'";
                    var result = _coreEntity.ExecuteSqlCommand(sqlquery);
                    trans.Commit();
                    return "UPDATED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        //new create
        public string createNewBranchSetup(BranchSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var newmaxlocationcode = string.Empty;
                    var newmaxlocationcodequery = $@"SELECT COUNT(BRANCH_CODE)+1 as MAX_BRANCH_CODE FROM FA_BRANCH_SETUP WHERE PRE_BRANCH_CODE = '{model.BRANCH_CODE}'";
                    newmaxlocationcode = this._coreEntity.SqlQuery<int>(newmaxlocationcodequery).FirstOrDefault().ToString();

                    if (model.BRANCH_CODE != null && model.PRE_BRANCH_CODE != "")
                    {
                        var maxPreCode = string.Empty;
                        var newprecode = string.Empty;
                        var newmastercode = string.Empty;

                        if (model.BRANCH_CODE != null && model.BRANCH_CODE != "")
                        {
                            var maxprecodequery = $@"select (count(*) + 1) as MAXCODE from FA_BRANCH_SETUP where PRE_BRANCH_CODE like '{model.BRANCH_CODE}' and company_code = '{_workContext.CurrentUserinformation.company_code}'";
                            maxPreCode = this._coreEntity.SqlQuery<int>(maxprecodequery).FirstOrDefault().ToString();
                            if (maxPreCode != null)
                            {
                                if (Convert.ToInt32(maxPreCode) <= 9)
                                {
                                    maxPreCode = "0" + maxPreCode.ToString();
                                }

                            }
                            newprecode = model.PRE_BRANCH_CODE;
                            newmastercode = model.PRE_BRANCH_CODE + "." + maxPreCode;

                            var childsqlquery = $@"INSERT INTO FA_BRANCH_SETUP (BRANCH_CODE,BRANCH_EDESC,BRANCH_NDESC,ADDRESS,TELEPHONE_NO,EMAIL,REMARKS,GROUP_SKU_FLAG,PRE_BRANCH_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SYN_ROWID,DELTA_FLAG) VALUES('{newmastercode}','{model.BRANCH_EDESC}','{model.BRANCH_NDESC}','{model.ADDRESS}','{model.TELEPHONE_NO}','{model.EMAIL}','{model.REMARKS}','{model.GROUP_SKU_FLAG}','{newprecode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','N','N')";
                            var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);




                        }

                    }
                    else
                    {
                        var newpre = string.Empty;
                        var newmaster = string.Empty;
                        var newprequery = $@"SELECT NVL(max(REGEXP_SUBSTR(BRANCH_CODE, '[^.]+', 1, 1)),0)+1 col_one FROM FA_BRANCH_SETUP";
                        newpre = this._coreEntity.SqlQuery<int>(newprequery).FirstOrDefault().ToString();
                        if (Convert.ToInt32(newpre) <= 9)
                        {
                            newpre = "0" + newpre.ToString();
                        }
                        if (model.GROUP_SKU_FLAG == "G")
                        {
                            newmaster = newpre + ".01";
                        }

                        var rootsqlquery = $@"INSERT INTO FA_BRANCH_SETUP (BRANCH_CODE,BRANCH_EDESC,BRANCH_NDESC,ADDRESS,TELEPHONE_NO,EMAIL,REMARKS,GROUP_SKU_FLAG,PRE_BRANCH_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SYN_ROWID,DELTA_FLAG) VALUES('{newmaster}','{model.BRANCH_EDESC}','{model.BRANCH_NDESC}','{model.ADDRESS}','{model.TELEPHONE_NO}','{model.EMAIL}','{model.REMARKS}','{model.GROUP_SKU_FLAG}','00','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','N','N')";
                        var insertroot = _coreEntity.ExecuteSqlCommand(rootsqlquery);

                    }
                    trans.Commit();
                    return "INSERTED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }

        //Branch setup
        public BranchSetupModel GetBranchCenterDetailBybranchCode(string branchCode)
        {
            try
            {
                if (string.IsNullOrEmpty(branchCode)) { branchCode = string.Empty; }
                string Query = $@"SELECT BRANCH_CODE,BRANCH_EDESC,BRANCH_NDESC,GROUP_SKU_FLAG,PRE_BRANCH_CODE,REMARKS,ADDRESS,CREATED_DATE,TELEPHONE_NO,EMAIL,SYN_ROWID,ABBR_CODE from FA_BRANCH_SETUP WHERE BRANCH_CODE='{branchCode}' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'   ORDER BY TO_NUMBER(BRANCH_CODE) ASC";
                BranchSetupModel entity = this._dbContext.SqlQuery<BranchSetupModel>(Query).FirstOrDefault();


                string parentbudgetQuery = $@"SELECT BRANCH_CODE FROM FA_BRANCH_SETUP where BRANCH_CODE = (SELECT PRE_BRANCH_CODE FROM FA_BRANCH_SETUP WHERE BRANCH_CODE='{branchCode}' 
                                AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}')";
                entity.PARENT_BRANCH_CODE = this._dbContext.SqlQuery<string>(parentbudgetQuery).FirstOrDefault();

                return entity;

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        //Get Branch Child grid data
        public List<BranchSetupModel> GetBranchCenterListByGroupCode(string groupId)
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                string query = $@"SELECT DISTINCT 
                        INITCAP(BRANCH_EDESC) AS BRANCH_EDESC,
                        BRANCH_CODE ,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,
                        BRANCH_CODE AS MASTER_BRANCH_CODE, ADDRESS,EMAIL,REMARKS,GROUP_SKU_FLAG
                        FROM FA_BRANCH_SETUP
                        WHERE DELETED_FLAG = 'N' 
                        AND GROUP_SKU_FLAG='I'
                        AND PRE_BRANCH_CODE = '{groupId}'
                        AND COMPANY_CODE='{company_code}'
                        ORDER BY BRANCH_EDESC";
                var branchCenterCodeList = _dbContext.SqlQuery<BranchSetupModel>(query).ToList();
                return branchCenterCodeList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public List<BranchSetupModel> GetAllBranchCenterList(string searchText)
        {
            try
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    string query = $@"SELECT DISTINCT 
                        INITCAP(BRANCH_EDESC) AS BRANCH_EDESC,
                        BRANCH_CODE ,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,
                        BRANCH_CODE AS MASTER_BRANCH_CODE, ADDRESS,EMAIL,REMARKS,GROUP_SKU_FLAG
                        FROM FA_BRANCH_SETUP
                        WHERE DELETED_FLAG = 'N'
                        AND COMPANY_CODE='{company_code}'
                        AND UPPER(BRANCH_EDESC) LIKE UPPER('%{ searchText }%')
                        ORDER BY BRANCH_EDESC";
                    var branchCenterCodeList = _dbContext.SqlQuery<BranchSetupModel>(query).ToList();
                    return branchCenterCodeList;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region Party Type Setup


        public string deletePartySetup(string partyCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(partyCode)) { partyCode = string.Empty; }
                string message = string.Empty;
                var sqlquery = $@"UPDATE IP_PARTY_TYPE_CODE SET DELETED_FLAG='Y' WHERE PARTY_TYPE_CODE='{partyCode}' AND COMPANY_CODE= '{companyCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                if (result > 0)
                {
                    message = "DELETED";
                }
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string updatePartyType(PartyTypeModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var message = string.Empty;
                    var companyCode = _workContext.CurrentUserinformation.company_code;

                    var sqlquery = $@"UPDATE IP_PARTY_TYPE_CODE SET PARTY_TYPE_EDESC='{model.PARTY_TYPE_EDESC}', ACC_CODE='{model.ACC_CODE}',
                                      REMARKS='{model.REMARKS}', TEL_NO='{model.TEL_NO}' ,
                                      CREDIT_LIMIT='{model.CREDIT_LIMIT}',
                                      CREDIT_DAYS='{model.CREDIT_DAYS}',
                                     ADDRESS='{model.ADDRESS}'
                                      WHERE PARTY_TYPE_CODE = '{model.PARTY_TYPE_CODE}'";
                    var result = _coreEntity.ExecuteSqlCommand(sqlquery);
                    message = "UPDATED";
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }

        public string createNewPaetyType(PartyTypeModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var message = string.Empty;
                    var newmaxitemcode = string.Empty;
                    //var newmaxitemcodequery = $@"SELECT MAX(SUPPLIER_CODE)+1 as MASTER_SUPPLIER_CODE FROM IP_SUPPLIER_SETUP";
                    var newmaxitemcodequery = $@"select NVL(MAX(TO_NUMBER(REGEXP_SUBSTR(TO_NUMBER(PARTY_TYPE_CODE), '[^.]+', 1, 1))),0)+1 as MASTER_DEALER_CODE from IP_PARTY_TYPE_CODE  WHERE COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'";
                    newmaxitemcode = this._coreEntity.SqlQuery<int>(newmaxitemcodequery).FirstOrDefault().ToString();

                    var rootchildsqlquery = $@"INSERT INTO IP_PARTY_TYPE_CODE (PARTY_TYPE_CODE,PARTY_TYPE_EDESC,GROUP_SKU_FLAG,ACC_CODE,REMARKS,TEL_NO,CREDIT_LIMIT,COMPANY_CODE,CREATED_BY,
                                                 CREATED_DATE,DELETED_FLAG,SYN_ROWID,CREDIT_DAYS,ADDRESS,PRE_PARTY_CODE,MASTER_PARTY_CODE,PARTY_TYPE_FLAG) 
                                                 VALUES('{newmaxitemcode}','{model.PARTY_TYPE_EDESC}','I','{model.ACC_CODE}','{model.REMARKS}',
                                                 '{model.TEL_NO}','{model.CREDIT_LIMIT}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',
                                                 TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','N',
                                                 '{model.CREDIT_DAYS}','{model.ADDRESS}','01','01.00','P')";
                    var insertrootchild = _dbContext.ExecuteSqlCommand(rootchildsqlquery);
                    message = "INSERTED";
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }


        }


        public List<AccountCodeModels> getAllAccountCodeParty()
        {
            string query = @"SELECT DISTINCT 
                        INITCAP(ACC_EDESC) AS ACC_EDESC,
                        ACC_CODE ,
                        MASTER_ACC_CODE, 
                        PRE_ACC_CODE,
                        ACC_TYPE_FLAG
                        FROM FA_CHART_OF_ACCOUNTS_SETUP
                        WHERE DELETED_FLAG = 'N' 
                        CONNECT BY PRIOR MASTER_ACC_CODE = PRE_ACC_CODE
                        ORDER BY PRE_ACC_CODE";
            var accountCodeList = _dbContext.SqlQuery<AccountCodeModels>(query).ToList();
            return accountCodeList;
        }

        public List<PartyTypeModel> partyTypeList()
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                string query = $@"SELECT DISTINCT 
                        INITCAP(PARTY_TYPE_EDESC) AS PARTY_TYPE_EDESC,
                        PARTY_TYPE_CODE ,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,
                        PARTY_TYPE_CODE AS MASTER_PARTY_CODE, REMARKS,ACC_CODE,CREDIT_LIMIT,GROUP_SKU_FLAG,ADDRESS,TEL_NO,CREDIT_DAYS
                        FROM IP_PARTY_TYPE_CODE
                        WHERE DELETED_FLAG = 'N' 
                        AND GROUP_SKU_FLAG='I'
                        AND COMPANY_CODE='{company_code}'
                        ORDER BY PARTY_TYPE_EDESC";
                var partylist = _dbContext.SqlQuery<PartyTypeModel>(query).ToList();
                return partylist;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        #endregion
        #region Dealer Setup


        //create Customers

        public string CreateNewCustomer1234(MappedCustomerModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    foreach (var cst in model.CustSubList)
                    {
                        var csucode = "C" + cst.CUSTOMER_CODE;
                        var childsqlquery = $@"INSERT INTO FA_SUB_LEDGER_DEALER_MAP (PARTY_TYPE_CODE,SUB_CODE,CUSTOMER_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG)
                                        VALUES('{model.PARTY_TYPE_CODE}','{csucode}','{cst.CUSTOMER_CODE}','{_workContext.CurrentUserinformation.company_code}',
                                               '{_workContext.CurrentUserinformation.branch_code}','{_workContext.CurrentUserinformation.login_code}',
                                               SYSDATE,'N')";
                        var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);
                    }
                    trans.Commit();
                    return "INSERTED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }


        }

        public List<CustomerSetupModel> GetAccountListByCustomerCode123()
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;


                string query = $@"SELECT *
                                FROM SA_CUSTOMER_SETUP  where 
                                 DELETED_FLAG = 'N'
                                AND GROUP_SKU_FLAG = 'I'
                                AND COMPANY_CODE ='{_workContext.CurrentUserinformation.company_code}'";
                var customerCodeList = _dbContext.SqlQuery<CustomerSetupModel>(query).ToList();
                return customerCodeList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public List<CustomerSetupModel> GetAllAccountListByCustomerCode123(string searchText)
        {
            try
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    string query = $@"SELECT *
                                FROM SA_CUSTOMER_SETUP  where 
                                 DELETED_FLAG = 'N'
                                AND UPPER(CUSTOMER_EDESC) LIKE UPPER('%{ searchText }%')
                                AND COMPANY_CODE ='{_workContext.CurrentUserinformation.company_code}'";
                    var customerCodeList = _dbContext.SqlQuery<CustomerSetupModel>(query).ToList();
                    return customerCodeList;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public List<CustomerSetupModel> GetAccountListByCustomerCodeByDealerCode(string CustomerCode)
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;


                string query = $@"   SELECT *
                                FROM SA_CUSTOMER_SETUP  where 
                                 DELETED_FLAG = 'N'
                                AND GROUP_SKU_FLAG = 'I'
                                and customer_code not in (select customer_code from FA_SUB_LEDGER_DEALER_MAP where party_type_code='{CustomerCode}' and  COMPANY_CODE ='{_workContext.CurrentUserinformation.company_code}' )
                                AND COMPANY_CODE ='{_workContext.CurrentUserinformation.company_code}'";
                var customerCodeList = _dbContext.SqlQuery<CustomerSetupModel>(query).ToList();
                return customerCodeList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public DealerModel GetDealerDetailBydealerCode(string dealerCode)
        {
            try
            {
                if (string.IsNullOrEmpty(dealerCode)) { dealerCode = string.Empty; }
                string Query = $@"SELECT PARTY_TYPE_CODE,PARTY_TYPE_EDESC,REMARKS,GROUP_SKU_FLAG,ACC_CODE,
                                  PARTY_TYPE_FLAG,PAN_NO,ADDRESS,TEL_NO2,BRANCH_CODE,OWNER_NAME,LINK_BRANCH_CODE,AREA_CODE,ZONE_CODE,CREDIT_LIMIT,TEL_NO,CREDIT_DAYS,
                                  BG_AMOUNT,TERMS_CONDITIONS,APPROVED_FLAG,PRE_PARTY_CODE,MASTER_PARTY_CODE,EXCEED_LIMIT_PERCENTAGE,TRADE_DISCOUNT,
                                  ANNUAL_BONUS,BG_PER_UNIT,CD_PER_UNIT,PDC_CHEQUE_AMT,SALES_TARGET 
                                  from IP_PARTY_TYPE_CODE WHERE PARTY_TYPE_CODE='{dealerCode}' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'   ORDER BY TO_NUMBER(PARTY_TYPE_CODE) ASC";
                DealerModel entity = this._dbContext.SqlQuery<DealerModel>(Query).FirstOrDefault();


                string parentbudgetQuery = $@"SELECT PARTY_TYPE_CODE FROM IP_PARTY_TYPE_CODE where 
                                           MASTER_PARTY_CODE = (SELECT PRE_PARTY_CODE FROM IP_PARTY_TYPE_CODE WHERE PARTY_TYPE_CODE='{dealerCode}' 
                                           AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}')";
                entity.PARENT_DEALER_CODE = this._dbContext.SqlQuery<string>(parentbudgetQuery).FirstOrDefault();

                return entity;

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        // Mapped Dealer customers

        public List<CustSubList> GetDealerMapped(string dealerCode)
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                string query = $@"SELECT FDM.CUSTOMER_CODE,SCS.CUSTOMER_EDESC,SCS.TPIN_VAT_NO,SCS.REGD_OFFICE_EADDRESS
                                  FROM FA_SUB_LEDGER_DEALER_MAP FDM
                                  INNER JOIN sa_customer_setup SCS ON FDM.COMPANY_CODE = SCS.COMPANY_CODE AND FDM.BRANCH_CODE = SCS.BRANCH_CODE AND FDM.CUSTOMER_CODE = SCS.CUSTOMER_CODE
                                  WHERE FDM.DELETED_FLAG = 'N'
                                  AND FDM.PARTY_TYPE_CODE = '{dealerCode}'
                                  AND FDM.COMPANY_CODE='{company_code}'";

                var customersCodeList = _dbContext.SqlQuery<CustSubList>(query).ToList();
                return customersCodeList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<DealerModel> GetDealerListByGroupCode(string groupId)
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                string query = $@"SELECT DISTINCT 
                        INITCAP(PARTY_TYPE_EDESC) AS PARTY_TYPE_EDESC,
                        PARTY_TYPE_CODE ,CREATED_BY,CREATED_DATE,MODIFY_BY,MODIFY_DATE,
                        PARTY_TYPE_CODE AS MASTER_PARTY_CODE, REMARKS,ACC_CODE,CREDIT_LIMIT,GROUP_SKU_FLAG
                        FROM IP_PARTY_TYPE_CODE
                        WHERE DELETED_FLAG = 'N' 
                        AND GROUP_SKU_FLAG='I'
                        AND PRE_PARTY_CODE = '{groupId}'
                        AND COMPANY_CODE='{company_code}'
                        ORDER BY PARTY_TYPE_EDESC";
                var dealerCodeList = _dbContext.SqlQuery<DealerModel>(query).ToList();
                return dealerCodeList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public string DeleteDealerCenterByDealerCode(string dealerCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(dealerCode)) { dealerCode = string.Empty; }

                var masterQry = $@"SELECT PARTY_TYPE_CODE, GROUP_SKU_FLAG FROM IP_PARTY_TYPE_CODE WHERE PARTY_TYPE_CODE = '{dealerCode}' and COMPANY_CODE= '{companyCode}'";
                var masterDealerCode = _dbContext.SqlQuery<DealerModel>(masterQry).FirstOrDefault();
                if (masterDealerCode.GROUP_SKU_FLAG == "G")
                {
                    var childQry = $@"SELECT COUNT(*) FROM IP_PARTY_TYPE_CODE WHERE PRE_PARTY_CODE like ('{masterDealerCode.PARTY_TYPE_CODE}%') AND DELETED_FLAG = 'N'";
                    var childResult = _dbContext.SqlQuery<int>(childQry).FirstOrDefault();
                    if (childResult > 0)
                    {
                        return "HAS_CHILD";
                    }
                }
                var sqlquery = $@"UPDATE IP_PARTY_TYPE_CODE SET DELETED_FLAG = 'Y' WHERE PARTY_TYPE_CODE='{dealerCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                return "DELETED";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public string createNewDealerSetup(DealerModel model)
        {
            if (model.PARTY_TYPE_FLAG == "True")
            {
                model.PARTY_TYPE_FLAG = "P";
            }
            else
            {
                model.PARTY_TYPE_FLAG = "D";
            }
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var newmaxitemcode = string.Empty;
                    //var newmaxitemcodequery = $@"SELECT MAX(SUPPLIER_CODE)+1 as MASTER_SUPPLIER_CODE FROM IP_SUPPLIER_SETUP";
                    var newmaxitemcodequery = $@"select NVL(MAX(TO_NUMBER(REGEXP_SUBSTR(TO_NUMBER(PARTY_TYPE_CODE), '[^.]+', 1, 1))),0)+1 as MASTER_DEALER_CODE from IP_PARTY_TYPE_CODE  WHERE COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'";
                    newmaxitemcode = this._coreEntity.SqlQuery<int>(newmaxitemcodequery).FirstOrDefault().ToString();
                    if (model.MASTER_PARTY_CODE != null && model.MASTER_PARTY_CODE != "")
                    {
                        var maxPreCode = string.Empty;
                        var newpre = string.Empty;
                        var newmasteracccode = string.Empty;
                        if (model.GROUP_SKU_FLAG == "G")
                        {

                            if (model.MASTER_PARTY_CODE != null)
                            {
                                var maxprecodequery = $@"SELECT NVL(MAX(substr(MASTER_PARTY_CODE,-instr(reverse(MASTER_PARTY_CODE),'.')+1))+1,0) as MAXCODE FROM IP_PARTY_TYPE_CODE 
                                                         WHERE PRE_PARTY_CODE like '{model.MASTER_PARTY_CODE}' and company_code = '{_workContext.CurrentUserinformation.company_code}'";


                                //var maxprecodequery = $@"select (MAX(substr(MASTER_SUPPLIER_CODE,-instr(reverse(MASTER_SUPPLIER_CODE),'.')+1))+1) as MAXCODE from IP_SUPPLIER_SETUP where PRE_SUPPLIER_CODE like '{model.suplierSetupModel.MASTER_SUPPLIER_CODE}' and company_code = '{_workContext.CurrentUserinformation.company_code}' AND GROUP_SKU_FLAG = 'G'";
                                //var maxprecodequery = $@"select (count(*) + 1) as MAXCODE from IP_SUPPLIER_SETUP where PRE_SUPPLIER_CODE like '{model.MASTER_SUPPLIER_CODE}' and company_code = '{_workContext.CurrentUserinformation.company_code}'";
                                var maxCode = this._coreEntity.SqlQuery<int>(maxprecodequery).FirstOrDefault();
                                maxPreCode = maxCode == 0 ? "1" : maxCode.ToString();


                                if (maxPreCode == null)
                                    maxPreCode = "1";
                                if (Convert.ToInt32(maxPreCode) <= 9)
                                {
                                    maxPreCode = "0" + maxPreCode.ToString();
                                }
                                newpre = model.MASTER_PARTY_CODE;
                                newmasteracccode = model.MASTER_PARTY_CODE + "." + maxPreCode;
                                #region  insert SUB GROUP DEALER
                                //insertSupplier(model, newmaxitemcode, newpre, newmasteracccode);
                                var rootchildsqlquery = $@"INSERT INTO IP_PARTY_TYPE_CODE (PARTY_TYPE_CODE,PARTY_TYPE_EDESC,PARTY_TYPE_NDESC,GROUP_SKU_FLAG,ACC_CODE,REMARKS,TEL_NO,CREDIT_LIMIT,COMPANY_CODE,CREATED_BY,
                                                 CREATED_DATE,DELETED_FLAG,SYN_ROWID,CREDIT_DAYS,PAN_NO,TEL_NO2,ADDRESS,OWNER_NAME,LINK_BRANCH_CODE,AREA_CODE,ZONE_CODE,BG_AMOUNT,TERMS_CONDITIONS,APPROVED_FLAG,
                                                 EXCEED_LIMIT_PERCENTAGE,TRADE_DISCOUNT,ANNUAL_BONUS,BG_PER_UNIT,CD_PER_UNIT,PDC_CHEQUE_AMT,SALES_TARGET,PRE_PARTY_CODE,MASTER_PARTY_CODE) 
                                                 VALUES('{newmaxitemcode}','{model.PARTY_TYPE_EDESC}','{model.PARTY_TYPE_NDESC}','{model.GROUP_SKU_FLAG}','{model.ACC_CODE}','{model.REMARKS}',
                                                 '{model.TEL_NO}','{model.CREDIT_LIMIT}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',
                                                 TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','N',
                                                 '{model.CREDIT_DAYS}','{model.PAN_NO}','{model.TEL_NO2}','{model.ADDRESS}','{model.OWNER_NAME}','{model.LINK_BRANCH_CODE}',
                                                  '{model.AREA_CODE}','{model.ZONE_CODE}','{model.BG_AMOUNT}','{model.TERMS_CONDITIONS}','{model.APPROVED_FLAG}','{model.EXCEED_LIMIT_PERCENTAGE}',
                                                  '{model.TRADE_DISCOUNT}','{model.ANNUAL_BONUS}','{model.BG_PER_UNIT}','{model.CD_PER_UNIT}','{model.PDC_CHEQUE_AMT}','{model.SALES_TARGET}','{newpre}','{newmasteracccode}')";
                                var insertrootchild = _dbContext.ExecuteSqlCommand(rootchildsqlquery);

                                #endregion


                            }

                        }
                        else
                        {
                            newpre = model.MASTER_PARTY_CODE;
                            newmasteracccode = model.MASTER_PARTY_CODE + "." + "00";


                            #region  insert GRID CHILD INSERT
                            //insertSupplier(model, newmaxitemcode, newpre, newmasteracccode);

                            var rootchildsqlquery = $@"INSERT INTO IP_PARTY_TYPE_CODE (PARTY_TYPE_CODE,PARTY_TYPE_EDESC,PARTY_TYPE_NDESC,GROUP_SKU_FLAG,ACC_CODE,REMARKS,TEL_NO,CREDIT_LIMIT,COMPANY_CODE,CREATED_BY,
                                                 CREATED_DATE,DELETED_FLAG,SYN_ROWID,CREDIT_DAYS,PAN_NO,TEL_NO2,ADDRESS,OWNER_NAME,LINK_BRANCH_CODE,AREA_CODE,ZONE_CODE,BG_AMOUNT,TERMS_CONDITIONS,APPROVED_FLAG,
                                                 EXCEED_LIMIT_PERCENTAGE,TRADE_DISCOUNT,ANNUAL_BONUS,BG_PER_UNIT,CD_PER_UNIT,PDC_CHEQUE_AMT,SALES_TARGET,PRE_PARTY_CODE,MASTER_PARTY_CODE,PARTY_TYPE_FLAG) 
                                                 VALUES('{newmaxitemcode}','{model.PARTY_TYPE_EDESC}','{model.PARTY_TYPE_NDESC}','{model.GROUP_SKU_FLAG}','{model.ACC_CODE}','{model.REMARKS}',
                                                 '{model.TEL_NO}','{model.CREDIT_LIMIT}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',
                                                 TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','N',
                                                 '{model.CREDIT_DAYS}','{model.PAN_NO}','{model.TEL_NO2}','{model.ADDRESS}','{model.OWNER_NAME}','{model.LINK_BRANCH_CODE}',
                                                  '{model.AREA_CODE}','{model.ZONE_CODE}','{model.BG_AMOUNT}','{model.TERMS_CONDITIONS}','{model.APPROVED_FLAG}','{model.EXCEED_LIMIT_PERCENTAGE}',
                                                  '{model.TRADE_DISCOUNT}','{model.ANNUAL_BONUS}','{model.BG_PER_UNIT}','{model.CD_PER_UNIT}','{model.PDC_CHEQUE_AMT}','{model.SALES_TARGET}','{newpre}','{newmasteracccode}','D')";
                            var insertrootchild = _dbContext.ExecuteSqlCommand(rootchildsqlquery);


                            #endregion


                        }

                    }
                    else
                    {
                        var newpre = string.Empty;
                        var newmaster = string.Empty;
                        var newprequery = $@"SELECT  MAX(substr(MASTER_PARTY_CODE,-instr(reverse(MASTER_PARTY_CODE),'.')+1))+1 col_one FROM IP_PARTY_TYPE_CODE WHERE COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'";


                        newpre = this._coreEntity.SqlQuery<int>(newprequery).FirstOrDefault().ToString();
                        if (Convert.ToInt32(newpre) <= 9)
                        {
                            newpre = "0" + newpre.ToString();
                        }
                        if (model.GROUP_SKU_FLAG == "G")
                        {
                            newmaster = newpre + ".01";
                        }
                        else
                        {
                            newmaster = newpre;
                        }
                        //newpre = "00";
                        #region  insert IP_SUPPLIER_SETUP query
                        //insertSupplier(model, newmaxitemcode, "00", newpre);


                        var rootchildsqlquery = $@"INSERT INTO IP_PARTY_TYPE_CODE (PARTY_TYPE_CODE,PARTY_TYPE_EDESC,PARTY_TYPE_NDESC,GROUP_SKU_FLAG,ACC_CODE,REMARKS,TEL_NO,CREDIT_LIMIT,COMPANY_CODE,CREATED_BY,
                                                 CREATED_DATE,DELETED_FLAG,SYN_ROWID,CREDIT_DAYS,PAN_NO,TEL_NO2,ADDRESS,OWNER_NAME,LINK_BRANCH_CODE,AREA_CODE,ZONE_CODE,BG_AMOUNT,TERMS_CONDITIONS,APPROVED_FLAG,
                                                 EXCEED_LIMIT_PERCENTAGE,TRADE_DISCOUNT,ANNUAL_BONUS,BG_PER_UNIT,CD_PER_UNIT,PDC_CHEQUE_AMT,SALES_TARGET,PRE_PARTY_CODE,MASTER_PARTY_CODE) 
                                                 VALUES('{newmaxitemcode}','{model.PARTY_TYPE_EDESC}','{model.PARTY_TYPE_NDESC}','{model.GROUP_SKU_FLAG}','{model.ACC_CODE}','{model.REMARKS}',
                                                 '{model.TEL_NO}','{model.CREDIT_LIMIT}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',
                                                 TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','N',
                                                 '{model.CREDIT_DAYS}','{model.PAN_NO}','{model.TEL_NO2}','{model.ADDRESS}','{model.OWNER_NAME}','{model.LINK_BRANCH_CODE}',
                                                  '{model.AREA_CODE}','{model.ZONE_CODE}','{model.BG_AMOUNT}','{model.TERMS_CONDITIONS}','{model.APPROVED_FLAG}','{model.EXCEED_LIMIT_PERCENTAGE}',
                                                  '{model.TRADE_DISCOUNT}','{model.ANNUAL_BONUS}','{model.BG_PER_UNIT}','{model.CD_PER_UNIT}','{model.PDC_CHEQUE_AMT}','{model.SALES_TARGET}','00','{newpre}')";
                        var insertrootchild = _dbContext.ExecuteSqlCommand(rootchildsqlquery);


                        #endregion

                    }
                    trans.Commit();
                    return "INSERTED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }


        }

        public string udpateDealerSetup(DealerModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var companyCode = _workContext.CurrentUserinformation.company_code;

                    var sqlquery = $@"UPDATE IP_PARTY_TYPE_CODE SET PARTY_TYPE_EDESC='{model.PARTY_TYPE_EDESC}',PARTY_TYPE_NDESC='{model.PARTY_TYPE_NDESC}', ACC_CODE='{model.ACC_CODE}',
                                      REMARKS='{model.REMARKS}', TEL_NO='{model.TEL_NO}' ,
                                      PRE_PARTY_CODE='{model.PRE_PARTY_CODE}',MASTER_PARTY_CODE='{model.MASTER_PARTY_CODE}', CREDIT_LIMIT='{model.CREDIT_LIMIT}',
                                      CREDIT_DAYS='{model.CREDIT_DAYS}', PAN_NO='{model.PAN_NO}',
                                      TEL_NO2='{model.TEL_NO2}',ADDRESS='{model.ADDRESS}', OWNER_NAME='{model.OWNER_NAME}',
                                      LINK_BRANCH_CODE='{model.LINK_BRANCH_CODE}', AREA_CODE='{model.AREA_CODE}' ,
                                      ZONE_CODE='{model.ZONE_CODE}',BG_AMOUNT='{model.BG_AMOUNT}', TERMS_CONDITIONS='{model.TERMS_CONDITIONS}',
                                      APPROVED_FLAG='{model.APPROVED_FLAG}', EXCEED_LIMIT_PERCENTAGE='{model.EXCEED_LIMIT_PERCENTAGE}',
                                      TRADE_DISCOUNT='{model.TRADE_DISCOUNT}',ANNUAL_BONUS='{model.ANNUAL_BONUS}', BG_PER_UNIT='{model.BG_PER_UNIT}',
                                      CD_PER_UNIT='{model.CD_PER_UNIT}', PDC_CHEQUE_AMT='{model.PDC_CHEQUE_AMT}', SALES_TARGET='{model.SALES_TARGET}'
                                      WHERE PARTY_TYPE_CODE = '{model.PARTY_TYPE_CODE}'";
                    var result = _coreEntity.ExecuteSqlCommand(sqlquery);
                    trans.Commit();
                    return "UPDATED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }

        }
        #endregion

        #region Vehicle Setup
        public string deleteVehicleSetups(string vehicleCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(vehicleCode)) { vehicleCode = string.Empty; }
                string message = string.Empty;
                var sqlquery = $@"UPDATE IP_VEHICLE_CODE SET DELETED_FLAG='Y' WHERE VEHICLE_CODE='{vehicleCode}' AND COMPANY_CODE= '{companyCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                if (result > 0)
                {
                    message = "DELETED";
                }
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public string createVehicleSetup(VehicleSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;

                    var message = string.Empty;

                    string Query = $@"INSERT INTO IP_VEHICLE_CODE (VEHICLE_CODE,VEHICLE_TYPE,VEHICLE_EDESC, VEHICLE_ID, OWNER_NAME, OWNER_MOBILE_NO, DRIVER_NAME,DRIVER_LICENCE_NO, DRIVER_MOBILE_NO,GROUP_EDESC, COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,ACC_CODE,VEHICLE_LABOUR_EDESC,REMARKS) 
                            VALUES('{model.VEHICLE_CODE}','{model.VEHICLE_TYPE}','{model.VEHICLE_EDESC}','{model.VEHICLE_ID}','{model.OWNER_NAME}','{model.OWNER_MOBILE_NO}','{model.DRIVER_NAME}','{model.DRIVER_LICENCE_NO}','{model.DRIVER_MOBILE_NO}','{model.GROUP_EDESC}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','{model.ACC_CODE}','{model.VEHICLE_LABOUR_EDESC}','{model.REMARKS}')";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "INSERTED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }


        public string updateVehicleSetup(VehicleSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var message = string.Empty;
                    string Query = $@"UPDATE IP_VEHICLE_CODE SET VEHICLE_TYPE='{model.VEHICLE_TYPE}',VEHICLE_EDESC='{model.VEHICLE_EDESC}',VEHICLE_ID='{model.VEHICLE_ID}',OWNER_NAME='{model.OWNER_NAME}',
                                OWNER_MOBILE_NO='{model.OWNER_MOBILE_NO}',DRIVER_NAME='{model.DRIVER_NAME}',DRIVER_LICENCE_NO='{model.DRIVER_LICENCE_NO}', DRIVER_MOBILE_NO='{model.DRIVER_MOBILE_NO}',GROUP_EDESC='{model.GROUP_EDESC}', MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',MODIFY_DATE = SYSDATE,ACC_CODE='{model.ACC_CODE}',VEHICLE_LABOUR_EDESC='{model.VEHICLE_LABOUR_EDESC}',REMARKS='{model.REMARKS}'  WHERE VEHICLE_CODE = '{model.VEHICLE_CODE}'";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "UPDATED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }

        public List<VehicleSetupModel> getAllVehicle()
        {
            string query = $@"SELECT * FROM IP_VEHICLE_CODE
                        WHERE DELETED_FLAG = 'N'
                        AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'";
            var List = _dbContext.SqlQuery<VehicleSetupModel>(query).ToList();
            return List;
        }


        public string GetVehicleCode1(string vehicletype)
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                string query = $@" select count(*)  as num from IP_VEHICLE_CODE where company_code={company_code} and  VEHICLE_TYPE='{vehicletype}'";
                var List = _dbContext.SqlQuery<int>(query).FirstOrDefault();
                var data = vehicletype + (List + 1);
                return data;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        #endregion


        #region ISSUE TYPE SETUP

        public List<IssueType> GetSavedIssueType()
        {
            try
            {
                string query = $@"SELECT * FROM IP_ISSUE_TYPE_CODE
                               WHERE DELETED_FLAG='N' 
                               and COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'";
                var typeList = _dbContext.SqlQuery<IssueType>(query).ToList();
                _logErp.WarnInDB("Issue type successfullly fetch by User : " + _workContext.CurrentUserinformation.LOGIN_EDESC);
                return typeList;
            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while getting issue type : " + ex.StackTrace);
                throw new Exception(ex.Message);

            }
        }


        public string SaveIssueType(IssueTypeSetupModel typeModal)
        {
            try
            {
                string message = string.Empty;
                string insertIssue = $@"INSERT INTO IP_ISSUE_TYPE_CODE(ISSUE_TYPE_CODE,ISSUE_TYPE_EDESC,ISSUE_TYPE_NDESC,REMARKS,
                 COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,MODIFY_DATE,MODIFY_BY,ISSUE_TYPE_FLAG)
                 VALUES('{typeModal.ISSUE_TYPE_CODE}','{typeModal.ISSUE_TYPE_EDESC}','{typeModal.ISSUE_TYPE_NDESC}','{typeModal.REMARKS}',
                 '{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','','','')";
                var insertedRow = _coreEntity.ExecuteSqlCommand(insertIssue);
                message = "INSERTED";
                return message;
            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while saving issue type : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }


        public string UpdateIssueTypeSetup(IssueTypeSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var message = string.Empty;
                    string Query = $@"UPDATE IP_ISSUE_TYPE_CODE SET ISSUE_TYPE_CODE='{model.ISSUE_TYPE_CODE}',ISSUE_TYPE_EDESC='{model.ISSUE_TYPE_EDESC}',ISSUE_TYPE_NDESC='{model.ISSUE_TYPE_NDESC}',
                                 MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',MODIFY_DATE = SYSDATE,REMARKS='{model.REMARKS}'  WHERE ISSUE_TYPE_CODE = '{model.ISSUE_TYPE_CODE}'";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "UPDATED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }

        public string DeleteIssueTypeSetups(string issueTypeCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(issueTypeCode)) { issueTypeCode = string.Empty; }
                string message = string.Empty;
                var sqlquery = $@"UPDATE IP_ISSUE_TYPE_CODE SET DELETED_FLAG='Y' WHERE ISSUE_TYPE_CODE='{issueTypeCode}' AND COMPANY_CODE= '{companyCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                if (result > 0)
                {
                    message = "DELETED";
                }
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region MEASUREMENT UNIT SETUP

        public List<MeasurementUnit> GetAllMeasurementUnit()
        {
            try
            {
                string query = $@"SELECT * FROM IP_MU_CODE
                               WHERE DELETED_FLAG='N' 
                               and COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'";
                var unitList = _dbContext.SqlQuery<MeasurementUnit>(query).ToList();
                _logErp.WarnInDB("measurement unit successfullly fetch by User : " + _workContext.CurrentUserinformation.LOGIN_EDESC);
                return unitList;
            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while getting measurement unit : " + ex.StackTrace);
                throw new Exception(ex.Message);

            }
        }


        public string SaveMeasurementUnit(MeasurementUnit unitModal)
        {
            try
            {
                string message = string.Empty;
                string insertIssue = $@"INSERT INTO IP_MU_CODE(MU_CODE,MU_EDESC,MU_NDESC,REMARKS,
                 COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,MODIFY_DATE,MODIFY_BY)
                 VALUES('{unitModal.MU_CODE}','{unitModal.MU_EDESC}','{unitModal.MU_NDESC}','{unitModal.REMARKS}',
                 '{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','','')";
                var insertedRow = _coreEntity.ExecuteSqlCommand(insertIssue);
                message = "INSERTED";
                return message;
            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while saving measurement unit : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }


        public string UpdateMeasurementUnit(MeasurementUnit model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var message = string.Empty;
                    string Query = $@"UPDATE IP_MU_CODE SET MU_EDESC='{model.MU_EDESC}',MU_NDESC='{model.MU_NDESC}',
                                 MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',MODIFY_DATE = SYSDATE,REMARKS='{model.REMARKS}'  WHERE MU_CODE = '{model.MU_CODE}'";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "UPDATED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }

        public string DeleteMeasurementUnit(string unitCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(unitCode)) { unitCode = string.Empty; }
                string message = string.Empty;
                var sqlquery = $@"UPDATE IP_MU_CODE SET DELETED_FLAG='Y' WHERE MU_CODE='{unitCode}' AND COMPANY_CODE= '{companyCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                if (result > 0)
                {
                    message = "DELETED";
                }
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region CITY SETUP

        public List<CityModels> GetCities()
        {
            try
            {
                string query = $@"SELECT * FROM CITY_CODE
                               WHERE DELETED_FLAG='N' 
                               ";
                var typeList = _dbContext.SqlQuery<CityModels>(query).ToList();
                _logErp.WarnInDB("Issue type successfullly fetch by User : " + _workContext.CurrentUserinformation.LOGIN_EDESC);
                return typeList;
            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while getting issue type : " + ex.StackTrace);
                throw new Exception(ex.Message);

            }
        }


        public string SaveCitySetup(CityModelForSave typeModal)
        {
            try
            {
                string message = string.Empty;
                string insertIssue = $@"INSERT INTO CITY_CODE(CITY_CODE,CITY_EDESC,CITY_NDESC,CITY_VDC_FLAG,DISTRICT_CODE,REMARKS,
                 MODIFY_DATE,MODIFY_BY,DELETED_FLAG)
                 VALUES('{typeModal.CITY_CODE}','{typeModal.CITY_EDESC}','{typeModal.CITY_NDESC}','{typeModal.CITY_VDC_FLAG}',
                 '{typeModal.DISTRICT_CODE}','{typeModal.REMARKS}',SYSDATE,'{_workContext.CurrentUserinformation.login_code}','N')";
                var insertedRow = _coreEntity.ExecuteSqlCommand(insertIssue);
                message = "INSERTED";
                return message;
            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while saving issue type : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }


        public string UpdateCitySetup(CityModelForSave model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var message = string.Empty;
                    string Query = $@"UPDATE CITY_CODE SET CITY_EDESC='{model.CITY_EDESC}',CITY_NDESC='{model.CITY_NDESC}',
                                 CITY_VDC_FLAG='{model.CITY_VDC_FLAG}',DISTRICT_CODE='{model.DISTRICT_CODE}',
                                 MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',MODIFY_DATE = SYSDATE,REMARKS='{model.REMARKS}' 
                                 WHERE CITY_CODE = '{model.CITY_CODE}'";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "UPDATED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logErp.ErrorInDB("Error while updating city setup : " + ex.StackTrace);
                    throw ex;
                }
            }
        }

        public string DeleteCitySetup(string cityCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(cityCode)) { cityCode = string.Empty; }
                string message = string.Empty;
                var sqlquery = $@"UPDATE CITY_CODE SET DELETED_FLAG='Y' WHERE CITY_CODE='{cityCode}' ";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                if (result > 0)
                {
                    message = "DELETED";
                }
                return message;
            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while deleting city setup : " + ex.StackTrace);
                throw ex;
            }
        }


        public List<DistrictModels> GetDistricts()
        {
            try
            {
                string disQuery = $@"SELECT DISTRICT_CODE , DISTRICT_EDESC FROM DISTRICT_CODE";
                var disList = _dbContext.SqlQuery<DistrictModels>(disQuery).ToList();
                return disList;
            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while getting district : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region Vehicle Registration Setup
        // Vehicle Registration

        public string getMaxTransactionNo(string gFlag)
        {
            try
            {
                string Query = $@"SELECT TO_CHAR(MAX(TO_NUMBER(TRANSACTION_NO)+1)) TRANSACTION_NO FROM IP_VEHICLE_TRACK WHERE DELETED_FLAG='{gFlag}'";
                var max_item_code = this._dbContext.SqlQuery<string>(Query).FirstOrDefault();
                return max_item_code;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public List<VehicleRegistrationModel> GetVehicleReg(string from)
        {
            try
            {
                if (from == "LoadingSlip")
                {
                    //string query = $@"SELECT IVT.TRANSACTION_NO,IVC.VEHICLE_EDESC as VEHICLE_NAME,IVT.TRANSACTION_DATE,IVT.DESTINATION,IVC.VEHICLE_CODE,
                    //   IVT.LOAD_IN_TIME,IVT.LOAD_OUT_TIME,IVT.REMARKS,IVT.DRIVER_NAME,IVT.DRIVER_LICENCE_NO,IVT.QUANTITY,IVT.TRANSPORT_NAME,IVT.DRIVER_MOBILE_NO,
                    //   IVT.TEAR_WT,IVT.GROSS_WT,IVT.NET_WT,IVT.VEHICLE_IN_DATE,IVT.VEHICLE_OUT_DATE FROM IP_VEHICLE_TRACK IVT
                    //   INNER JOIN IP_VEHICLE_CODE IVC ON IVT.VEHICLE_NAME =IVC.VEHICLE_CODE AND IVC.COMPANY_CODE=IVT.COMPANY_CODE
                    //    WHERE IVT.DELETED_FLAG = 'N'
                    //    AND IVT.COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'";
                    string query = $@"SELECT IVT.TRANSACTION_NO,IVC.VEHICLE_EDESC as VEHICLE_NAME,IVT.DESTINATION,IVC.VEHICLE_CODE,
                       IVT.REMARKS,IVT.DRIVER_NAME,IVT.DRIVER_LICENCE_NO,IVT.QUANTITY,IVT.TRANSPORT_NAME,IVT.DRIVER_MOBILE_NO,
                       IVT.TEAR_WT,IVT.GROSS_WT,IVT.NET_WT FROM IP_VEHICLE_TRACK IVT
                       INNER JOIN IP_VEHICLE_CODE IVC ON IVT.VEHICLE_NAME =IVC.VEHICLE_CODE AND IVC.COMPANY_CODE=IVT.COMPANY_CODE
                        WHERE IVT.DELETED_FLAG = 'N' 
                        AND IVT.COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'";
                    var List = _dbContext.SqlQuery<VehicleRegistrationModel>(query).ToList();
                    return List;
                }
                else
                {
                    string query = $@"SELECT IVT.TRANSACTION_NO,IVC.VEHICLE_EDESC as VEHICLE_NAME,IVT.TRANSACTION_DATE,IVT.DESTINATION,IVC.VEHICLE_CODE,IVT.REMARKS,
                                IVT.ACCESS_FLAG, BS_DATE (TRANSACTION_DATE) MITI,IVT.READ_FLAG,IVT.REFERENCE_NO,IVT.IN_TIME,IVT.OUT_TIME,IVT.TRANSPORTER_CODE,IVT.TRANSPORT_NAME
                       FROM IP_VEHICLE_TRACK IVT
                       INNER JOIN IP_VEHICLE_CODE IVC ON IVT.VEHICLE_NAME =IVC.VEHICLE_CODE AND IVC.COMPANY_CODE=IVT.COMPANY_CODE
                        WHERE IVT.DELETED_FLAG = 'N' and (READ_FLAG IS NULL OR READ_FLAG = 'N')
                        AND IVT.COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'";
                    var List = _dbContext.SqlQuery<VehicleRegistrationModel>(query).ToList();
                    return List;
                }

            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while getting registered vehicle : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }

        //Create Vehicle Registration 
        public string createNewVehicleReg(VehicleRegistrationModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var branch_code = _workContext.CurrentUserinformation.branch_code;
                    var newmaxtransactionNo = string.Empty;



                    var inTime = "";
                    if (model.IN_TIME != null)
                    {
                        if (model.IN_TIME != "")
                        {
                            DateTime dt = DateTime.Parse(model.IN_TIME);
                            dt = dt.AddMinutes(-15);
                            inTime = dt.ToString("HH:mm");
                        }
                    }
                    var inTime1 = "";
                    if (model.OUT_TIME != null)
                    {

                        if (model.OUT_TIME != "")
                        {
                            DateTime dt = DateTime.Parse(model.OUT_TIME);
                            dt = dt.AddMinutes(-15);
                            inTime1 = dt.ToString("HH:mm");
                        }

                    }

                    var inTime2 = "";
                    if (model.LOAD_IN_TIME != null)
                    {
                        if (model.LOAD_IN_TIME != "")
                        {
                            DateTime dt = DateTime.Parse(model.LOAD_IN_TIME);
                            dt = dt.AddMinutes(-15);
                            inTime2 = dt.ToString("HH:mm");
                        }

                    }

                    var inTime3 = "";
                    if (model.LOAD_OUT_TIME != null)
                    {
                        if (model.LOAD_OUT_TIME != "")
                        {
                            DateTime dt = DateTime.Parse(model.LOAD_OUT_TIME);
                            dt = dt.AddMinutes(-15);
                            inTime3 = dt.ToString("HH:mm");
                        }

                    }
                    var newmaxTransactionquery = $@"  SELECT NVL(MAX(TO_NUMBER(REGEXP_SUBSTR(TO_NUMBER(TRANSACTION_NO), '[^.]+', 1, 1))),0)+1 as MAX_ITEM_CODE from IP_VEHICLE_TRACK WHERE COMPANY_CODE = '{company_code}'";
                    newmaxtransactionNo = this._coreEntity.SqlQuery<int>(newmaxTransactionquery).FirstOrDefault().ToString();

                    var message = string.Empty;
                    DateTime VEHICLE_IN_DATE = model.VEHICLE_IN_DATE ?? DateTime.Now;
                    DateTime Transaction_date = model.TRANSACTION_DATE ?? DateTime.Now;
                    var vechileout = model.VEHICLE_OUT_DATE ?? null;
                    var vehicleoutColumn = "''";
                    if (vechileout != null)
                    {
                        vehicleoutColumn = $@"TO_DATE('{model.VEHICLE_OUT_DATE.Value.ToString("MM/dd/yyyy")}', 'MM/dd/yyyy')";
                    }


                    //model.VEHICLE_IN_DATE.HasValue ? model.VEHICLE_IN_DATE : DateTime.Now;
                    string Query = $@"INSERT INTO IP_VEHICLE_TRACK (TRANSACTION_NO,VEHICLE_NAME,REMARKS, VEHICLE_OWNER_NAME, VEHICLE_OWNER_NO, DRIVER_NAME,DRIVER_LICENCE_NO, DRIVER_MOBILE_NO,IN_TIME, COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,OUT_TIME,LOAD_IN_TIME,LOAD_OUT_TIME,TEAR_WT,GROSS_WT,NET_WT,QUANTITY,DESTINATION,BROKER_NAME,WB_SLIP_NO,TRANSPORT_NAME,VEHICLE_IN_DATE,VEHICLE_OUT_DATE,TRANSACTION_DATE,TOTAL_VEHICLE_HR) 
                            VALUES('{newmaxtransactionNo}','{model.VEHICLE_NAME}','{model.REMARKS}','{model.VEHICLE_OWNER_NAME}','{model.VEHICLE_OWNER_NO}','{model.DRIVER_NAME}','{model.DRIVER_LICENCE_NO}','{model.DRIVER_MOBILE_NO}','{inTime}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N',
                            '{inTime1}','{inTime2}','{inTime3}','{model.TEAR_WT}','{model.GROSS_WT}','{model.NET_WT}','{model.QUANTITY}','{model.DESTINATION}','{model.BROKER_NAME}','{model.WB_SLIP_NO}','{model.TRANSPORT_NAME}',TO_DATE('{ VEHICLE_IN_DATE.ToString("MM/dd/yyyy")}', 'MM/dd/yyyy'),{vehicleoutColumn},TO_DATE('{Transaction_date.ToString("MM/dd/yyyy")}', 'MM/dd/yyyy'),'{model.TOTAL_VEHICLE_HR}')";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "INSERTED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }

        //Update  vehicle Registration
        public string updateVehicleReg(VehicleRegistrationModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var inTime = "";
                    if (model.IN_TIME != null)
                    {
                        if (model.IN_TIME != "")
                        {
                            DateTime dt = DateTime.Parse(model.IN_TIME);
                            dt = dt.AddMinutes(-15);
                            inTime = dt.ToString("HH:mm");
                        }
                    }
                    var inTime1 = "";
                    if (model.OUT_TIME != null)
                    {
                        if (model.OUT_TIME != "")
                        {
                            DateTime dt = DateTime.Parse(model.OUT_TIME);
                            dt = dt.AddMinutes(-15);
                            inTime1 = dt.ToString("HH:mm");
                        }

                    }

                    var inTime2 = "";
                    if (model.LOAD_IN_TIME != null)
                    {
                        if (model.LOAD_IN_TIME != "")
                        {
                            DateTime dt = DateTime.Parse(model.LOAD_IN_TIME);
                            dt = dt.AddMinutes(-15);
                            inTime2 = dt.ToString("HH:mm");
                        }

                    }

                    var inTime3 = "";
                    if (model.LOAD_OUT_TIME != null)
                    {
                        if (model.LOAD_OUT_TIME != "")
                        {
                            DateTime dt = DateTime.Parse(model.LOAD_OUT_TIME);
                            dt = dt.AddMinutes(-15);
                            inTime3 = dt.ToString("HH:mm");
                        }

                    }
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var message = string.Empty;
                    var vehicleindate = model.VEHICLE_IN_DATE.HasValue ? model.VEHICLE_IN_DATE.Value.ToString("MM/dd/yyyy") : DateTime.Now.ToString("MM/dd/yyyy");
                    var vehicleOutDate = model.VEHICLE_OUT_DATE.HasValue ? $"TO_DATE('{model.VEHICLE_OUT_DATE.Value.ToString("MM/dd/yyyy")}', 'MM/dd/yyyy')" : "''";
                    var transactionDate = model.TRANSACTION_DATE.HasValue ? model.TRANSACTION_DATE.Value.ToString("MM/dd/yyyy") : DateTime.Now.ToString("MM/dd/yyyy");
                    string Query = $@"UPDATE IP_VEHICLE_TRACK SET VEHICLE_NAME='{model.VEHICLE_NAME}',REMARKS='{model.REMARKS}',VEHICLE_OWNER_NAME='{model.VEHICLE_OWNER_NAME}',VEHICLE_OWNER_NO='{model.VEHICLE_OWNER_NO}',
                                      DRIVER_NAME='{model.DRIVER_NAME}',DRIVER_LICENCE_NO='{model.DRIVER_LICENCE_NO}', DRIVER_MOBILE_NO='{model.DRIVER_MOBILE_NO}',IN_TIME='{inTime}', MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',
                                      MODIFY_DATE = SYSDATE,OUT_TIME='{inTime1}',LOAD_OUT_TIME='{inTime3}',LOAD_IN_TIME='{inTime2}',TEAR_WT='{model.TEAR_WT}',GROSS_WT='{model.GROSS_WT}',NET_WT='{model.NET_WT}',QUANTITY='{model.QUANTITY}',
                                      DESTINATION='{model.DESTINATION}',BROKER_NAME='{model.BROKER_NAME}',WB_SLIP_NO='{model.WB_SLIP_NO}',TRANSPORT_NAME='{model.TRANSPORT_NAME}',
                                      VEHICLE_IN_DATE=TO_DATE('{vehicleindate}', 'MM/dd/yyyy'),VEHICLE_OUT_DATE={vehicleOutDate},
                                      TRANSACTION_DATE=TO_DATE('{transactionDate}', 'MM/dd/yyyy'),TOTAL_VEHICLE_HR='{model.TOTAL_VEHICLE_HR}'  WHERE TRANSACTION_NO = '{model.TRANSACTION_NO}'";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "UPDATED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }
        //Delete Vehicle Registration
        public string DeleteVehicleRegistration(string vehicleCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(vehicleCode)) { vehicleCode = string.Empty; }
                string message = string.Empty;
                var sqlquery = $@"UPDATE IP_VEHICLE_TRACK SET DELETED_FLAG='Y' WHERE TRANSACTION_NO='{vehicleCode}' AND COMPANY_CODE= '{companyCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                if (result > 0)
                {
                    message = "DELETED";
                }
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public VehicleRegistrationModel GetVehicleDetailBytrCode(string transactionCode)
        {
            try
            {

                string Query = $@"SELECT  TRANSACTION_NO,VEHICLE_NAME,REMARKS, VEHICLE_OWNER_NAME, VEHICLE_OWNER_NO,TRANSACTION_DATE, DRIVER_NAME,DRIVER_LICENCE_NO, DRIVER_MOBILE_NO,IN_TIME,OUT_TIME,LOAD_IN_TIME,LOAD_OUT_TIME,TEAR_WT,GROSS_WT,NET_WT,QUANTITY,DESTINATION,BROKER_NAME,WB_SLIP_NO,TRANSPORT_NAME,VEHICLE_IN_DATE,VEHICLE_OUT_DATE,TOTAL_VEHICLE_HR from IP_VEHICLE_TRACK WHERE TRANSACTION_NO='{transactionCode}'  ORDER BY TO_NUMBER(TRANSACTION_NO) ASC";
                VehicleRegistrationModel entity = this._dbContext.SqlQuery<VehicleRegistrationModel>(Query).FirstOrDefault();

                return entity;



            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        #endregion

        #region Rejetable item setup
        public List<RejectableItem> getRejectlbleitems()
        {
            string query = $@"SELECT ITEM_ID,ITEM_CODE,ITEM_NAME,REMARKS FROM REJECTABLE_ITEMS
                        WHERE DELETED_FLAG = 'N'
                        AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'";
            var List = _dbContext.SqlQuery<RejectableItem>(query).ToList();
            return List;
        }



        public string createRejectableItemSetup(RejectableItem model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var newmaxRejectedItemNo = string.Empty;
                    var newmaxTransactionquery = $@"  SELECT NVL(MAX(TO_NUMBER(REGEXP_SUBSTR(TO_NUMBER(ITEM_ID), '[^.]+', 1, 1))),0)+1 as MAX_ITEM_CODE from REJECTABLE_ITEMS WHERE COMPANY_CODE = '{company_code}'";
                    newmaxRejectedItemNo = this._coreEntity.SqlQuery<int>(newmaxTransactionquery).FirstOrDefault().ToString();

                    var message = string.Empty;

                    string Query = $@"INSERT INTO REJECTABLE_ITEMS (ITEM_ID,ITEM_CODE,ITEM_NAME, REMARKS, COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG) 
                            VALUES('{newmaxRejectedItemNo}','{model.ITEM_CODE}','{model.ITEM_NAME}','{model.REMARKS}','{_workContext.CurrentUserinformation.company_code}',
                                   '{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N')";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "INSERTED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }

        public string updateRejetableItemSetup(RejectableItem model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var message = string.Empty;
                    string Query = $@"UPDATE REJECTABLE_ITEMS SET ITEM_CODE='{model.ITEM_CODE}',ITEM_NAME='{model.ITEM_NAME}',REMARKS='{model.REMARKS}',
                                      MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',MODIFY_DATE = SYSDATE WHERE ITEM_ID = '{model.ITEM_ID}'";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "UPDATED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }

        public string deleteRejectableSetup(string itemId)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(itemId)) { itemId = string.Empty; }
                string message = string.Empty;
                var sqlquery = $@"UPDATE REJECTABLE_ITEMS SET DELETED_FLAG='Y' WHERE ITEM_ID='{itemId}' AND COMPANY_CODE= '{companyCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                if (result > 0)
                {
                    message = "DELETED";
                }
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region TDS setup  

        public string getMaxTdsCode()
        {
            try
            {
                string Query = $@"SELECT TO_CHAR(MAX(TO_NUMBER(TDS_CODE)+1)) TDS_CODE FROM FA_TDS_CODE";
                var max_tds_code = this._dbContext.SqlQuery<string>(Query).FirstOrDefault();
                return max_tds_code;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public List<TDSTypeModel> getAllTDS()
        {
            string query = $@"SELECT * FROM FA_TDS_CODE
                        WHERE DELETED_FLAG = 'N'
                        AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'";
            var List = _dbContext.SqlQuery<TDSTypeModel>(query).ToList();
            return List;
        }

        public string createTDSSetup(TDSTypeModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;

                    var message = string.Empty;
                    //string Query1 = $@"SELECT TO_CHAR(MAX(TO_NUMBER(TDS_CODE)+1)) TDS_CODE FROM FA_TDS_CODE WHERE COMPANY_CODE='{company_code}'";
                    //var max_tds_code = this._dbContext.SqlQuery<string>(Query1).FirstOrDefault();

                    string Query = $@"INSERT INTO FA_TDS_CODE (TDS_CODE,TDS_EDESC,TDS_TYPE_CODE, REMARKS, COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG) 
                            VALUES('{model.TDS_CODE}','{model.TDS_EDESC}','{model.TDS_TYPE_CODE}','{model.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N')";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "INSERTED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }


        public string updatetdsSetup(TDSTypeModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var message = string.Empty;
                    string Query = $@"UPDATE FA_TDS_CODE SET TDS_EDESC='{model.TDS_EDESC}',TDS_TYPE_CODE='{model.TDS_TYPE_CODE}',REMARKS='{model.REMARKS}',
                                   MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',MODIFY_DATE = SYSDATE  WHERE TDS_CODE = '{model.TDS_CODE}'";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "UPDATED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }


        public string deleteTDsSetup(string tdsCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(tdsCode)) { tdsCode = string.Empty; }
                string message = string.Empty;
                var sqlquery = $@"UPDATE FA_TDS_CODE SET DELETED_FLAG='Y' WHERE TDS_CODE='{tdsCode}' AND COMPANY_CODE= '{companyCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                if (result > 0)
                {
                    message = "DELETED";
                }
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Priority setup
        public List<PrioritySeupModel> getAllPriority()
        {
            string query = $@"SELECT * FROM IP_PRIORITY_CODE
                        WHERE DELETED_FLAG = 'N'
                        AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'";
            var List = _dbContext.SqlQuery<PrioritySeupModel>(query).ToList();
            return List;
        }
        //save
        public string createPriority(PrioritySeupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;

                    var message = string.Empty;

                    string Query = $@"INSERT INTO IP_PRIORITY_CODE (PRIORITY_CODE,PRIORITY_EDESC,PRIORITY_NDESC, REMARKS, COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG) 
                            VALUES('{model.PRIORITY_CODE}','{model.PRIORITY_EDESC}','{model.PRIORITY_NDESC}','{model.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N')";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "INSERTED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }


        public string updatePrioritySetup(PrioritySeupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var message = string.Empty;
                    string Query = $@"UPDATE IP_PRIORITY_CODE SET PRIORITY_EDESC='{model.PRIORITY_EDESC}',PRIORITY_NDESC='{model.PRIORITY_NDESC}',REMARKS='{model.REMARKS}',
                                   MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',MODIFY_DATE = SYSDATE  WHERE PRIORITY_CODE = '{model.PRIORITY_CODE}'";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "UPDATED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }

        public string deletePrioritySetups(string priorityCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(priorityCode)) { priorityCode = string.Empty; }
                string message = string.Empty;
                var sqlquery = $@"UPDATE IP_PRIORITY_CODE SET DELETED_FLAG='Y' WHERE PRIORITY_CODE='{priorityCode}' AND COMPANY_CODE= '{companyCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                if (result > 0)
                {
                    message = "DELETED";
                }
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
        //create customer for symphony
        public string createNewCustomerSetup1(CustomerModels model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var newpre = string.Empty;
                    var newmaster = string.Empty;
                    var maxCustomerCode = string.Empty;
                    var newprequery = $@"select NVL(MAX(TO_NUMBER(REGEXP_SUBSTR(TO_NUMBER(CUSTOMER_CODE), '[^.]+', 1, 1))),0)+1 as maxCustomerCode from SA_CUSTOMER_SETUP  order by TO_NUMBER(CUSTOMER_CODE) desc";
                    maxCustomerCode = this._coreEntity.SqlQuery<int>(newprequery).FirstOrDefault().ToString();

                    var newprequery1 = $@"SELECT NVL(max(REGEXP_SUBSTR(MASTER_CUSTOMER_CODE, '[^.]+', 1, 1)),0)+1 col_one FROM SA_CUSTOMER_SETUP";
                    newpre = this._coreEntity.SqlQuery<int>(newprequery1).FirstOrDefault().ToString();

                    if (Convert.ToInt32(newpre) <= 9)
                    {
                        newpre = "0" + newpre.ToString();
                    }
                    if (model.GROUP_SKU_FLAG == "G")
                    {
                        newmaster = newpre + ".01";
                    }
                    else
                    {
                        newmaster = newpre + ".00";
                    }
                    var childsqlquery = $@"INSERT INTO SA_CUSTOMER_SETUP (CUSTOMER_CODE,CUSTOMER_EDESC,TEL_MOBILE_NO1,GROUP_SKU_FLAG,CREDIT_ACTION_FLAG,CASH_CUSTOMER_FLAG,
                                               MASTER_CUSTOMER_CODE,PRE_CUSTOMER_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,
                                               BRANCH_CODE,PARTY_TYPE_CODE,APPROVED_FLAG,CUSTOMER_ID,REGD_OFFICE_EADDRESS,ACC_CODE)
                                               VALUES ('{maxCustomerCode}','OperaSymphonyCustomer','','G','','','{newmaster}','00','{_workContext.CurrentUserinformation.company_code}',
                                               '{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N',
                                               '{_workContext.CurrentUserinformation.branch_code}','CP','Y','81','LOCAL','20')";
                    var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);

                    trans.Commit();
                    return "INSERTED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }



        }

        public string createNewItemSetup1(ItemSetupModel model)
        {


            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var newmaxitemcode = string.Empty;
                    var newmaxGroupitemcodequery = $@"  select NVL(MAX(TO_NUMBER(REGEXP_SUBSTR(TO_NUMBER(ITEM_CODE), '[^.]+', 1, 1))),0)+1 as MAX_ITEM_CODE from IP_ITEM_MASTER_SETUP  order by TO_NUMBER(ITEM_CODE) desc";
                    newmaxitemcode = this._coreEntity.SqlQuery<int>(newmaxGroupitemcodequery).FirstOrDefault().ToString();


                    var newpre = string.Empty;
                    var newmaster = string.Empty;
                    var newprequery = $@"SELECT NVL(max(REGEXP_SUBSTR(MASTER_ITEM_CODE, '[^.]+', 1, 1)),0)+1 col_one FROM ip_item_master_setup";
                    newpre = this._coreEntity.SqlQuery<int>(newprequery).FirstOrDefault().ToString();
                    if (Convert.ToInt32(newpre) <= 9)
                    {
                        newpre = "0" + newpre.ToString();
                    }
                    if (model.GROUP_SKU_FLAG == "G")
                    {
                        newmaster = newpre + ".01";
                    }
                    else
                    {
                        newmaster = newpre + ".00";
                    }


                    var rootsqlquery = $@"INSERT INTO IP_ITEM_MASTER_SETUP (ITEM_CODE,PRODUCT_CODE,ITEM_EDESC,CATEGORY_CODE,GROUP_SKU_FLAG,MASTER_ITEM_CODE,PRE_ITEM_CODE,
                                   COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,BRANCH_CODE)
                                   VALUES('{newmaxitemcode}','','OpraSymphonyItem','FF','G','{newmaster}','00','{_workContext.CurrentUserinformation.company_code}',
                                  '{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','{_workContext.CurrentUserinformation.branch_code}')";
                    var insertroot = _coreEntity.ExecuteSqlCommand(rootsqlquery);


                    trans.Commit();
                    return "INSERTED";
                }
                catch (Exception ex)
                {

                    trans.Rollback();
                    throw ex;
                }
            }
        }
        public string CreateKYCForm(KYCFORM model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {

                    var newMaxCustomerCode = string.Empty;

                    var query = $@"delete from SA_CUSTOMER_KYC where customer_code='{model.CustomerId}' ";

                    var insertchild1 = _coreEntity.ExecuteSqlCommand(query);

                    var childsqlquery = $@"INSERT INTO SA_CUSTOMER_KYC (CUSTOMER_CODE,
                                                    CUSTOMER_EDESC,BIRTH_DATE,Gender,MaritalStatus,
                                                    Religion,Bloadgroup,telephoneNo,MobileNo,
                                                    Companyname,EmailOffice,Address,
                                                    PermanentHouseNo,PWARDNO,PSTEETADDRESS,PZONE,PDIStrict,PVDCMunicipality,THouseNo,TWARDNO
                                                        ,TSTEETADDRESS,TZONE,TDIStrict,TVDCMunicipality,EmergencyName,Emergencyrelationship,Emergencyaddress,Emergencyphoneno,FamilyName,FamilyMotherName,FamilyspouseName,weddingDate,Childname,Organizationtype,organizationname,Position,FromDate,MODIFY_DATE,BRANCH_CODE,MODIFY_BY,CREATED_DATE,COMPANY_CODE,CREATED_BY,BIRTH_DATE_AD)
                                                    VALUES('{model.CustomerId}',
                                                        '{model.KYCCustomerName}',BS_DATE('{model.BirthDate}'),'{model.Gender}','{model.MaritalStatus}',
                                                    '{model.Religion}', '{model.Bloadgroup}', '{model.telephoneNo}','{model.MobileNo}','{model.Companyname}','{model.EmailOffice}','{model.Address}',
                                                    '{model.PermanentHouseNo}','{model.PWARDNO}','{model.PSTEETADDRESS}','{model.PZONE}','{model.PDIStrict}','{model.PVDCMunicipality}',
                                                   '{model.THouseNo}', '{model.TWARDNO}','{model.TSTEETADDRESS}','{model.TZONE}','{model.TDIStrict}','{model.TVDCMunicipality}','{model.EmergencyName}','{model.Emergencyrelationship}',
                                                        '{model.Emergencyaddress}','{model.Emergencyphoneno}','{model.FamilyName}','{model.FamilyMotherName}','{model.FamilyspouseName}','{model.weddingDate}','{model.Childname}','{model.Organizationtype}','{model.organizationname}','{model.Position}','{model.FromDate}','','01.01','admin',sysdate,'01','admin','{model.BirthDate}')";
                    var insertchild = _coreEntity.ExecuteSqlCommand(childsqlquery);
                    trans.Commit();
                    return "INSERTED";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }



        }

        #region Currency Setup
        public string createCurrencySetup(CurrencySetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;

                    var message = string.Empty;

                    string Query = $@"INSERT INTO CURRENCY_SETUP (CURRENCY_CODE, CURRENCY_EDESC, CURRENCY_NDESC, COUNTRY, REMARKS, COMPANY_CODE,  CREATED_BY, CREATED_DATE, DELETED_FLAG, CURRENCY_SYMBOL) 
                            VALUES('{model.CURRENCY_CODE}','{model.CURRENCY_EDESC}','{model.CURRENCY_NDESC}','{model.COUNTRY}','{model.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','{model.CURRENCY_SYMBOL}')";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "INSERTED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }
        public string updateCurrencySetup(CurrencySetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var message = string.Empty;
                    string Query = $@"UPDATE CURRENCY_SETUP SET CURRENCY_CODE='{model.CURRENCY_CODE}',CURRENCY_EDESC='{model.CURRENCY_EDESC}',CURRENCY_NDESC='{model.CURRENCY_NDESC}',CURRENCY_SYMBOL='{model.CURRENCY_SYMBOL}',COUNTRY='{model.COUNTRY}',
                                REMARKS='{model.REMARKS}', MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',MODIFY_DATE = SYSDATE  WHERE CURRENCY_CODE = '{model.CURRENCY_CODE}'";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "UPDATED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }
        public string deleteCurrencySetup(string currencyCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(currencyCode)) { currencyCode = string.Empty; }
                string message = string.Empty;
                var sqlquery = $@"UPDATE CURRENCY_SETUP SET DELETED_FLAG='Y' WHERE CURRENCY_CODE='{currencyCode}' AND COMPANY_CODE= '{companyCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                if (result > 0)
                {
                    message = "DELETED";
                }
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<CurrencySetupModel> getAllCurrencyCode()
        {
            string query = $@"SELECT * FROM CURRENCY_SETUP
                        WHERE DELETED_FLAG = 'N'
                        AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'";
            var List = _dbContext.SqlQuery<CurrencySetupModel>(query).ToList();
            return List;
        }
        #endregion

        #region Category Setup
        public string createCategorySetup(CategorySetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;

                    var message = string.Empty;

                    string Query = $@"INSERT INTO IP_CATEGORY_CODE (CATEGORY_CODE, CATEGORY_EDESC, CATEGORY_NDESC, REMARKS, COMPANY_CODE,  CREATED_BY, CREATED_DATE, DELETED_FLAG,CATEGORY_TYPE,PREFIX_TEXT) 
                            VALUES('{model.CATEGORY_CODE}','{model.CATEGORY_EDESC}','{model.CATEGORY_NDESC}','{model.REMARKS}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','{model.CATEGORY_TYPE}','{model.PREFIX_TEXT}')";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "INSERTED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }
        public string updateCategorySetup(CategorySetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var message = string.Empty;
                    string Query = $@"UPDATE IP_CATEGORY_CODE SET CATEGORY_CODE='{model.CATEGORY_CODE}',CATEGORY_EDESC='{model.CATEGORY_EDESC}',CATEGORY_NDESC='{model.CATEGORY_NDESC}',
                                REMARKS='{model.REMARKS}', MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',MODIFY_DATE = SYSDATE ,CATEGORY_TYPE ='{model.CATEGORY_TYPE}',PREFIX_TEXT ='{model.PREFIX_TEXT}' WHERE CATEGORY_CODE = '{model.CATEGORY_CODE}'";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "UPDATED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }
        public string deleteCategorySetup(string categoryCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(categoryCode)) { categoryCode = string.Empty; }
                string message = string.Empty;
                var sqlquery = $@"UPDATE IP_CATEGORY_CODE SET DELETED_FLAG='Y' WHERE CATEGORY_CODE='{categoryCode}' AND COMPANY_CODE= '{companyCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                if (result > 0)
                {
                    message = "DELETED";
                }
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<CategorySetupModel> getAllCategoryCode()
        {
            string query = $@"SELECT * FROM IP_CATEGORY_CODE
                        WHERE DELETED_FLAG = 'N'
                        AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}'";
            var List = _dbContext.SqlQuery<CategorySetupModel>(query).ToList();
            return List;
        }
        #endregion


        #region CHARGE TYPE SETUP

        public List<ChargeSetupModel> GetCharges()
        {
            try
            {
                string query = $@"SELECT * FROM IP_CHARGE_CODE
                               WHERE DELETED_FLAG='N' 
                               and COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'";
                var chargeTypeList = _dbContext.SqlQuery<ChargeSetupModel>(query).ToList();
                _logErp.WarnInDB("Charge type successfullly fetch by User : " + _workContext.CurrentUserinformation.LOGIN_EDESC);
                return chargeTypeList;
            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while getting charge type : " + ex.StackTrace);
                throw new Exception(ex.Message);

            }
        }


        public string SaveChargeType(ChargeSetupModel typeModal)
        {
            try
            {
                string message = string.Empty;
                string insertChargeType = $@"INSERT INTO IP_CHARGE_CODE(CHARGE_CODE,CHARGE_EDESC,CHARGE_NDESC,REMARKS,
                 COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,MODIFY_DATE,MODIFY_BY,SPECIFIC_CHARGE_FLAG)
                 VALUES('{typeModal.CHARGE_CODE}','{typeModal.CHARGE_EDESC}','{typeModal.CHARGE_NDESC}','{typeModal.REMARKS}',
                 '{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','','','{typeModal.SPECIFIC_CHARGE_FLAG}')";
                var insertedRow = _coreEntity.ExecuteSqlCommand(insertChargeType);
                message = "INSERTED";
                return message;
            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while saving charge type : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }


        public string UpdateChargeSetup(ChargeSetupModel model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var message = string.Empty;
                    string Query = $@"UPDATE IP_CHARGE_CODE SET CHARGE_EDESC='{model.CHARGE_EDESC}',CHARGE_NDESC='{model.CHARGE_NDESC}',
                                 MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',MODIFY_DATE = SYSDATE,REMARKS='{model.REMARKS}',SPECIFIC_CHARGE_FLAG='{model.SPECIFIC_CHARGE_FLAG}'  WHERE CHARGE_CODE = '{model.CHARGE_CODE}'";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "UPDATED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }

        public string DeleteChargeSetup(string chargeCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(chargeCode)) { chargeCode = string.Empty; }
                string message = string.Empty;
                var sqlquery = $@"UPDATE IP_CHARGE_CODE SET DELETED_FLAG='Y' WHERE CHARGE_CODE='{chargeCode}' AND COMPANY_CODE= '{companyCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                if (result > 0)
                {
                    message = "DELETED";
                }
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Scheme setup
        public string getMaxSchemeCode()
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                var newmaxacccodequery = $@"SELECT NVL(MAX(TO_NUMBER(regexp_replace(SCHEME_CODE, '[^[:digit:]]', '')))+1, 1) as SCHEME_AGENT_CODE FROM SCHEME_SETUP";
                var newmaxacccode = this._dbContext.SqlQuery<int>(newmaxacccodequery).FirstOrDefault();
                return newmaxacccode.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<SchemeModels> getAllSchemeCodeDetail()
        {
            try
            {
                var company_code = _workContext.CurrentUserinformation.company_code;
                string Query = $@"SELECT * FROM SCHEME_SETUP WHERE DELETED_FLAG='N' AND COMPANY_CODE = '{company_code}'";
                var entity = this._dbContext.SqlQuery<SchemeModels>(Query).ToList();
                return entity;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string createNewSchemeSetup(SchemeModels model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var newmaxschemecode = string.Empty;
                    var message = string.Empty;
                    string myEncodedString = string.Empty;
                    string mycCodeEncodedString = string.Empty;
                    string myICodeEncodedString = string.Empty;
                    string myDCodeEncodedString = string.Empty;
                    string myFCodeEncodedString = string.Empty;
                    string myACodeEncodedString = string.Empty;
                    string myBCodeEncodedString = string.Empty;
                    string deleted_flag = "N";


                    if (model.QUERY_STRING != "")
                    {
                        myEncodedString = HttpUtility.HtmlEncode(model.QUERY_STRING);
                    }
                    if (model.CUSTOMER_CODE != "")
                    {
                        mycCodeEncodedString = HttpUtility.HtmlEncode(model.CUSTOMER_CODE);
                    }
                    if (model.FORM_CODE != "")
                    {
                        myFCodeEncodedString = HttpUtility.HtmlEncode(model.FORM_CODE);
                    }
                    if (model.BRANCH_CODE != "")
                    {
                        myBCodeEncodedString = HttpUtility.HtmlEncode(model.BRANCH_CODE);
                    }
                    if (model.AREA_CODE != "")
                    {
                        myACodeEncodedString = HttpUtility.HtmlEncode(model.AREA_CODE);
                    }
                    if (model.ITEM_CODE != "")
                    {
                        myICodeEncodedString = HttpUtility.HtmlEncode(model.ITEM_CODE);
                    }
                    if (model.PARTY_TYPE_CODE != "")
                    {
                        myDCodeEncodedString = HttpUtility.HtmlEncode(model.PARTY_TYPE_CODE);
                    }
                    if (model.CHARGE_RATE == null)
                    {
                        model.CHARGE_RATE = 0;
                    }
                    if (model.SCHEME_CODE == "")
                    {
                        try
                        {
                            var newmaxschemecodequery = $@"SELECT NVL(MAX(TO_NUMBER(SCHEME_CODE))+1, 1) as MAX_SCHEME_CODE FROM SCHEME_SETUP";
                            newmaxschemecode = this._coreEntity.SqlQuery<int>(newmaxschemecodequery).FirstOrDefault().ToString();
                        }
                        catch (Exception)
                        {
                            var newmaxschemecodequery = $@"SELECT  NVL(MAX(TO_NUMBER(SCHEME_CODE))+1, 1) as MAX_SCHEME_CODE FROM SCHEME_SETUP WHERE REGEXP_LIKE(SCHEME_CODE, '^[[:digit:]]$')";
                            newmaxschemecode = this._coreEntity.SqlQuery<int>(newmaxschemecodequery).FirstOrDefault().ToString();
                        }
                    }
                    else
                    {
                        var existQry = $@"SELECT count(*) FROM SCHEME_SETUP WHERE SCHEME_CODE = '{model.SCHEME_CODE}'";
                        var isExist = this._coreEntity.SqlQuery<int>(existQry).FirstOrDefault();
                        if (isExist > 0)
                        {
                            return message = "This Code already exists";
                        }
                        newmaxschemecode = model.SCHEME_CODE;
                    }

                    string Query = $@"INSERT INTO SCHEME_SETUP (SCHEME_CODE,SCHEME_EDESC,FORM_CODE,STATUS,SCHEME_TYPE,TYPE,CALCULATION_DAYS, ACCOUNT_CODE, CUSTOMER_CODE,ITEM_CODE,AREA_CODE,PARTY_TYPE_CODE,CHARGE_CODE,CHARGE_ACCOUNT_CODE,CHARGE_RATE ,QUERY_STRING,EFFECTIVE_FROM,EFFECTIVE_TO,COMPANY_CODE, BRANCH_CODE,USER_ID,CREATED_BY,CREATED_DATE,DELETED_FLAG,IMPLEMENT_FLAG,REMARKS,FORM_EDESC,AREA_EDESC,BRANCH_EDESC,CUSTOMER_EDESC,ITEM_EDESC,PARTY_TYPE_EDESC)
                            VALUES('{newmaxschemecode}','{model.SCHEME_EDESC}','{myFCodeEncodedString}','{model.STATUS}','{model.SCHEME_TYPE}','{model.TYPE}',TO_DATE('{model.CALCULATION_DAYS}','MM/dd/yyyy hh12:mi:ss am'),'{model.ACCOUNT_CODE}','{mycCodeEncodedString}','{myICodeEncodedString}','{myACodeEncodedString}','{myDCodeEncodedString}','{model.CHARGE_CODE}','{model.CHARGE_ACCOUNT_CODE}',{model.CHARGE_RATE},'{myEncodedString}',TO_DATE('{model.EFFECTIVE_FROM}','MM/dd/yyyy hh12:mi:ss am'),TO_DATE('{model.EFFECTIVE_TO}','MM/dd/yyyy hh12:mi:ss am'),'{_workContext.CurrentUserinformation.company_code}','{myBCodeEncodedString}','{_workContext.CurrentUserinformation.User_id}','{_workContext.CurrentUserinformation.login_code}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),'N','N','{model.REMARKS}','{model.FORM_EDESC}','{model.AREA_EDESC}','{model.BRANCH_EDESC}','{model.CUSTOMER_EDESC}','{model.ITEM_EDESC}','{model.PARTY_TYPE_EDESC}')";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "INSERTED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }

        public string deleteSchemeSetup(string schemeCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(schemeCode)) { schemeCode = string.Empty; }
                string message = string.Empty;
                var sqlquery = $@"UPDATE SCHEME_SETUP SET DELETED_FLAG='Y' WHERE SCHEME_CODE='{schemeCode}'";
                var result = _dbContext.ExecuteSqlCommand(sqlquery);
                if (result > 0)
                {
                    message = "DELETED";
                }
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //public string ImplementScheme(string schemeCode)
        //{
        //    try
        //    {
        //        var companyCode = _workContext.CurrentUserinformation.company_code;
        //        if (string.IsNullOrEmpty(schemeCode)) { schemeCode = string.Empty; }
        //        string message = string.Empty;
        //        var sqlquery = $@"SELECT * FROM SCHEME_SETUP WHERE SCHEME_CODE='{schemeCode}'";
        //        var result = _dbContext.SqlQuery<SchemeModels>(sqlquery).FirstOrDefault();
        //        StringBuilder dynamicconditionbuilder = new StringBuilder();
        //        String[] customer_codes = new String[100];
        //        String[] party_type_codes = new String[100];
        //        String[] item_codes = new String[100];
        //        String[] dealer_code = new String[100];
        //        String[] splited_query = new String[10];
        //        string syssate = "SYSDATE";
        //        string fromeffectivedatedate = string.Empty;

        //        string toeffectivedate = string.Empty;
        //        string deleted_flag = "N";
        //        string s2 = "where";
        //        string effecivedatequery = string.Empty;
        //        string customercodequery = string.Empty;
        //        string itemcodequery = string.Empty;
        //        string partytypecodequery = string.Empty;
        //        string documentcodequery = string.Empty;
        //        string areacodequery = string.Empty;
        //        string branchcodequery = string.Empty;
        //        string dataaaa = "data.* from ";
        //        bool implementedflag = false;

        //        if (result.EFFECTIVE_FROM != null && result.EFFECTIVE_TO != null)
        //        {
        //            fromeffectivedatedate = "trunc(TO_DATE(" + "'" + result.EFFECTIVE_FROM + "'" + ",'MM/dd/yyyy hh12:mi:ss am'))";
        //            toeffectivedate = "trunc(TO_DATE(" + "'" + result.EFFECTIVE_TO + "'" + ",'MM/dd/yyyy hh12:mi:ss am'))";
        //            effecivedatequery = " and SALES_DATE>" + fromeffectivedatedate + " and SALES_DATE=" + toeffectivedate;
        //        }
        //        if (result.FORM_CODE != "" && result.FORM_CODE != null)
        //        {
        //            if (result.FORM_CODE.Contains(','))
        //            {
        //                documentcodequery = " AND FORM_CODE IN(" + result.FORM_CODE + ")";
        //            }
        //            else
        //            {
        //                documentcodequery = " AND FORM_CODE =" + "'" + result.FORM_CODE + "'";
        //            }
        //        }
        //        if (result.AREA_CODE != "" && result.AREA_CODE != null)
        //        {
        //            if (result.AREA_CODE.Contains(','))
        //            {
        //                areacodequery = " AND AREA_CODE IN(" + result.AREA_CODE + ")";
        //            }
        //            else
        //            {
        //                areacodequery = " AND AREA_CODE =" + "'" + result.AREA_CODE + "'";
        //            }
        //        }
        //        if (result.BRANCH_CODE != "" && result.BRANCH_CODE != null)
        //        {
        //            if (result.BRANCH_CODE.Contains(','))
        //            {
        //                branchcodequery = " AND BRANCH_CODE IN(" + result.BRANCH_CODE + ")";
        //            }
        //            else
        //            {
        //                branchcodequery = " AND BRANCH_CODE =" + "'" + result.BRANCH_CODE + "'";
        //            }
        //        }

        //        if (result.QUERY_STRING != "" && result.QUERY_STRING != null)
        //        {

        //            if (result.QUERY_STRING.ToLower().Contains(s2))
        //            {
        //                StringWriter myWriter = new StringWriter();


        //                HttpUtility.HtmlDecode(result.QUERY_STRING, myWriter);
        //                string myDecodedString = myWriter.ToString();
        //                splited_query = myDecodedString.Split('^');
        //            }
        //        }
        //        else
        //        {
        //            return "QueryFail";
        //        }
        //        if (result.CUSTOMER_CODE != "" && result.CUSTOMER_CODE != null)
        //        {
        //            if (result.CUSTOMER_CODE.Contains(','))
        //            {
        //                customer_codes = result.CUSTOMER_CODE.Split(',');
        //            }
        //            else
        //            {
        //                customer_codes[0] = result.CUSTOMER_CODE;
        //            }

        //            foreach (var customer_code in customer_codes)
        //            {
        //                var customeroption = GETCUSTOMEROPTION(customer_code);
        //                if (customeroption.GROUP_SKU_FLAG == "I")
        //                {
        //                    customercodequery = "CUSTOMER_CODE='" + customer_code + "'";
        //                }
        //                else
        //                {
        //                    customercodequery = "MASTER_CUSTOMER_CODE like'%" + customeroption.MASTER_CUSTOMER_CODE + ".%'";
        //                }
        //                if (result.ITEM_CODE != "" && result.ITEM_CODE != null)
        //                {
        //                    if (result.ITEM_CODE.Contains(','))
        //                    {
        //                        item_codes = result.ITEM_CODE.Split(',');
        //                    }
        //                    else
        //                    {
        //                        item_codes[0] = result.ITEM_CODE;
        //                    }

        //                    foreach (var item_code in item_codes)
        //                    {
        //                        if (item_code != null)
        //                        {
        //                            var itemoption = GETITEMOPTION(item_code);
        //                            if (itemoption.GROUP_SKU_FLAG == "I")
        //                            {
        //                                itemcodequery = " AND ITEM_CODE='" + item_code + "'";
        //                            }
        //                            else
        //                            {
        //                                itemcodequery = " AND MASTER_ITEM_CODE like'%" + itemoption.MASTER_ITEM_CODE + ".%'";
        //                            }
        //                            var SchemeVoucherNo = 1;
        //                            //dynamicconditionbuilder.Append("INSERT INTO SCHEME_SETUP_DETAIL ").Append("(").Append("SCHEME_CODE,SCHEMEVOUCHERNO,STATUS,FORM_CODE,ACCOUNT_CODE,CUSTOMER_CODE,ITEM_CODE,AREA_CODE, BRANCH_CODE, CHARGE_CODE, CHARGE_ACCOUNT_CODE, CHARGE_RATE, COMPANY_CODE, CREATED_BY, CREATED_DATE, DELETED_FLAG, QAUNTITY, SALES_SCHEME_VALUE, TOTAL_SALES, SALES_DISCOUNT").Append(")").Append("SELECT").Append(" '").Append(schemeCode).Append("' ").Append(" SCHEME_CODE, ").Append("'").Append(SchemeVoucherNo).Append("' ").Append(" SCHEMEVOUCHERNO, ").Append(" '").Append(result.STATUS).Append("'").Append(" STATUS, ").Append(" '").Append(result.FORM_CODE).Append("'").Append(" FORM_CODE, ").Append(" '").Append(result.ACCOUNT_CODE).Append("'").Append(" ACCOUNT_CODE,  ").Append(" '").Append(customer_code).Append("'").Append(" CUSTOMER_CODE, ").Append(" '").Append(item_code).Append("'").Append(" ITEM_CODE, ").Append(" '").Append(result.AREA_CODE).Append("'").Append(" AREA_CODE, ").Append(" '").Append(result.BRANCH_CODE).Append("'").Append(" BRANCH_CODE, ").Append(" '").Append(result.CHARGE_CODE).Append("'").Append(" CHARGE_CODE, ").Append(" '").Append(result.CHARGE_ACCOUNT_CODE).Append("'").Append(" CHARGE_ACCOUNT_CODE, ").Append(" '").Append(result.CHARGE_RATE).Append("'").Append(" CHARGE_RATE, ").Append(" '").Append(result.COMPANY_CODE).Append("'").Append(" COMPANY_CODE, ").Append(" '").Append(result.CREATED_BY).Append("'").Append(" CREATED_BY, ").Append(syssate).Append(" CREATED_DATE, ").Append(" '").Append(deleted_flag).Append("'").Append(" DELETED_FLAG, ").Append(dataaaa).Append(splited_query[0]).Append(customercodequery).Append(itemcodequery).Append(documentcodequery).Append(areacodequery).Append(branchcodequery).Append(effecivedatequery).Append(splited_query[1]).Append("CUSTOMER_CODE, ITEM_CODE").Append(splited_query[2]);

        //                            //dynamicconditionbuilder.Append("INSERT INTO SCHEME_SETUP_DETAIL ").Append("(").Append("SCHEME_CODE,SCHEMEVOUCHERNO,STATUS,FORM_CODE,ACCOUNT_CODE,CUSTOMER_CODE,ITEM_CODE,AREA_CODE, BRANCH_CODE, CHARGE_CODE, CHARGE_ACCOUNT_CODE, CHARGE_RATE, COMPANY_CODE, CREATED_BY, CREATED_DATE, DELETED_FLAG, QAUNTITY, SALES_SCHEME_VALUE, TOTAL_SALES, SALES_DISCOUNT").Append(")").Append("SELECT").Append(" '").Append(schemeCode).Append("' ").Append(" SCHEME_CODE, ").Append("'").Append(SchemeVoucherNo).Append("' ").Append(" SCHEMEVOUCHERNO, ").Append(" '").Append(result.STATUS).Append("'").Append(" STATUS, ").Append(" '").Append(result.FORM_CODE).Append("'").Append(" FORM_CODE, ").Append(" '").Append(result.ACCOUNT_CODE).Append("'").Append(" ACCOUNT_CODE,  ").Append(" '").Append(customer_code).Append("'").Append(" CUSTOMER_CODE, ").Append(" '").Append(item_code).Append("'").Append(" ITEM_CODE, ").Append(" '").Append(result.AREA_CODE).Append("'").Append(" AREA_CODE, ").Append(" '").Append(result.BRANCH_CODE).Append("'").Append(" BRANCH_CODE, ").Append(" '").Append(result.CHARGE_CODE).Append("'").Append(" CHARGE_CODE, ").Append(" '").Append(result.CHARGE_ACCOUNT_CODE).Append("'").Append(" CHARGE_ACCOUNT_CODE, ").Append(" '").Append(result.CHARGE_RATE).Append("'").Append(" CHARGE_RATE, ").Append(" '").Append(result.COMPANY_CODE).Append("'").Append(" COMPANY_CODE, ").Append(" '").Append(result.CREATED_BY).Append("'").Append(" CREATED_BY, ").Append(syssate).Append(" CREATED_DATE, ").Append(" '").Append(deleted_flag).Append("'").Append(" DELETED_FLAG, ").Append(dataaaa).Append(splited_query[0]).Append(customercodequery).Append(effecivedatequery).Append(splited_query[1]).Append("CUSTOMER_CODE").Append(splited_query[2]);

        //                            string final_query = string.Empty;
        //                            final_query = dynamicconditionbuilder.ToString();
        //                            this._dbContext.ExecuteSqlCommand(final_query);
        //                            SchemeVoucherNo++;

        //                        }



        //                    }
        //                }
        //                else
        //                {
        //                    return "ItemFail";
        //                }
        //            }

        //            implementedflag = true;
        //        }


        //        if (result.PARTY_TYPE_CODE != "" && result.PARTY_TYPE_CODE!=null)
        //        {
        //            if (result.PARTY_TYPE_CODE.Contains(','))
        //            {
        //                party_type_codes = result.PARTY_TYPE_CODE.Split(',');
        //            }
        //            else
        //            {
        //                party_type_codes[0] = result.PARTY_TYPE_CODE;
        //            }

        //            foreach (var party_type_code in party_type_codes)
        //            {
        //                var partytypeoption = GETPARTYOPTION(party_type_code);
        //                if (partytypeoption.GROUP_SKU_FLAG == "I")
        //                {
        //                    partytypecodequery = "PARTY_TYPE_CODE='" + party_type_code + "'";
        //                }
        //                else
        //                {
        //                    customercodequery = "MASTER_PARTY_CODE like'%" + partytypeoption.MASTER_PARTY_CODE + ".%'";
        //                }
        //                if (result.ITEM_CODE != "" && result.ITEM_CODE != null)
        //                {
        //                    if (result.ITEM_CODE.Contains(','))
        //                    {
        //                        item_codes = result.ITEM_CODE.Split(',');
        //                    }
        //                    else
        //                    {
        //                        item_codes[0] = result.ITEM_CODE;
        //                    }

        //                    foreach (var item_code in item_codes)
        //                    {
        //                        if (item_code != null)
        //                        {
        //                            var itemoption = GETITEMOPTION(item_code);
        //                            if (itemoption.GROUP_SKU_FLAG == "I")
        //                            {
        //                                itemcodequery = " AND ITEM_CODE='" + item_code + "'";
        //                            }
        //                            else
        //                            {
        //                                itemcodequery = " AND MASTER_ITEM_CODE like'%" + itemoption.MASTER_ITEM_CODE + ".%'";
        //                            }
        //                            var SchemeVoucherNo = 1;
        //                            dynamicconditionbuilder.Append("INSERT INTO SCHEME_SETUP_DETAIL ").Append("(").Append("SCHEME_CODE,SCHEMEVOUCHERNO,STATUS,FORM_CODE,ACCOUNT_CODE,PARTY_TYPE_CODE,ITEM_CODE,AREA_CODE, BRANCH_CODE, CHARGE_CODE, CHARGE_ACCOUNT_CODE, CHARGE_RATE, COMPANY_CODE, CREATED_BY, CREATED_DATE, DELETED_FLAG, QAUNTITY, SALES_SCHEME_VALUE, TOTAL_SALES, SALES_DISCOUNT").Append(")").Append("SELECT").Append(" '").Append(schemeCode).Append("' ").Append(" SCHEME_CODE, ").Append("'").Append(SchemeVoucherNo).Append("' ").Append(" SCHEMEVOUCHERNO, ").Append(" '").Append(result.STATUS).Append("'").Append(" STATUS, ").Append(" '").Append(result.FORM_CODE).Append("'").Append(" FORM_CODE, ").Append(" '").Append(result.ACCOUNT_CODE).Append("'").Append(" ACCOUNT_CODE,  ").Append(" '").Append(party_type_code).Append("'").Append(" PARTY_TYPE_CODE, ").Append(" '").Append(item_code).Append("'").Append(" ITEM_CODE, ").Append(" '").Append(result.AREA_CODE).Append("'").Append(" AREA_CODE, ").Append(" '").Append(result.BRANCH_CODE).Append("'").Append(" BRANCH_CODE, ").Append(" '").Append(result.CHARGE_CODE).Append("'").Append(" CHARGE_CODE, ").Append(" '").Append(result.CHARGE_ACCOUNT_CODE).Append("'").Append(" CHARGE_ACCOUNT_CODE, ").Append(" '").Append(result.CHARGE_RATE).Append("'").Append(" CHARGE_RATE, ").Append(" '").Append(result.COMPANY_CODE).Append("'").Append(" COMPANY_CODE, ").Append(" '").Append(result.CREATED_BY).Append("'").Append(" CREATED_BY, ").Append(syssate).Append(" CREATED_DATE, ").Append(" '").Append(deleted_flag).Append("'").Append(" DELETED_FLAG, ").Append(dataaaa).Append(splited_query[0]).Append(partytypecodequery).Append(itemcodequery).Append(documentcodequery).Append(areacodequery).Append(branchcodequery).Append(effecivedatequery).Append(splited_query[1]).Append("PARTY_TYPE_CODE, ITEM_CODE").Append(splited_query[2]);

        //                            string final_query = string.Empty;
        //                            final_query = dynamicconditionbuilder.ToString();
        //                            this._dbContext.ExecuteSqlCommand(final_query);
        //                            SchemeVoucherNo++;
        //                        }

        //                    }
        //                }
        //                else
        //                {
        //                    return "ItemFail";
        //                }

        //            }
        //            implementedflag = true;
        //        }



        //        //}
        //        if (implementedflag == true)
        //        {
        //            var updatesqlquery = $@"UPDATE SCHEME_SETUP SET IMPLEMENT_FLAG='Y' WHERE SCHEME_CODE='{schemeCode}'";
        //            var updateresult = _dbContext.ExecuteSqlCommand(updatesqlquery);
        //        }





        //        //if (result > 0)
        //        //{
        //        //    message = "DELETED";
        //        //}
        //        return message;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}


        public string ImplementScheme(string schemeCode)
        {
            try
            {
                var companyCode = _workContext.CurrentUserinformation.company_code;
                if (string.IsNullOrEmpty(schemeCode)) { schemeCode = string.Empty; }
                string message = string.Empty;
                string selectquery = string.Empty;
                var sqlquery = $@"SELECT * FROM SCHEME_SETUP WHERE SCHEME_CODE='{schemeCode}'";
                var result = _dbContext.SqlQuery<SchemeModels>(sqlquery).FirstOrDefault();
                StringBuilder dynamicconditionbuilder = new StringBuilder();
                String[] customer_codes = new String[10000];
                String[] party_type_codes = new String[10000];
                String[] item_codes = new String[100];
                String[] dealer_code = new String[100];
                String[] splited_query = new String[10];
                string syssate = "SYSDATE";
                string fromeffectivedatedate = string.Empty;

                string toeffectivedate = string.Empty;
                string deleted_flag = "N";
                string s2 = "where";
                string effecivedatequery = string.Empty;
                string customercodequery = string.Empty;
                string itemcodequery = string.Empty;
                string partytypecodequery = string.Empty;
                string documentcodequery = string.Empty;
                string areacodequery = string.Empty;
                string branchcodequery = string.Empty;
                string cust_codeq = string.Empty;
                string form_codeq = string.Empty;
                string area_codeq = string.Empty;
                string branch_codeq = string.Empty;
                string party_type_codeq = string.Empty;
                string dataaaa = "data.* from ";
                bool implementedflag = false;

                if (result.EFFECTIVE_FROM != null && result.EFFECTIVE_TO != null)
                {
                    fromeffectivedatedate = "trunc(TO_DATE(" + "'" + result.EFFECTIVE_FROM + "'" + ",'MM/dd/yyyy hh12:mi:ss am'))";
                    toeffectivedate = "trunc(TO_DATE(" + "'" + result.EFFECTIVE_TO + "'" + ",'MM/dd/yyyy hh12:mi:ss am'))";
                    effecivedatequery = " and SALES_DATE>=" + fromeffectivedatedate + " and SALES_DATE<=" + toeffectivedate;
                }
                if (result.FORM_CODE != "" && result.FORM_CODE != null)
                {
                    form_codeq = "FORM_CODE,";
                    if (result.FORM_CODE.Contains(','))
                    {
                        documentcodequery = " AND FORM_CODE IN(" + result.FORM_CODE + ")";
                    }
                    else
                    {
                        documentcodequery = " AND FORM_CODE =" + "'" + result.FORM_CODE + "'";
                    }
                }
                if (result.AREA_CODE != "" && result.AREA_CODE != null)
                {
                    area_codeq = "AREA_CODE,";
                    if (result.AREA_CODE.Contains(','))
                    {
                        areacodequery = " AND AREA_CODE IN(" + result.AREA_CODE + ")";
                    }
                    else
                    {
                        areacodequery = " AND AREA_CODE =" + "'" + result.AREA_CODE + "'";
                    }
                }
                if (result.BRANCH_CODE != "" && result.BRANCH_CODE != null)
                {
                    branch_codeq = "BRANCH_CODE,";
                    if (result.BRANCH_CODE.Contains(','))
                    {
                        branchcodequery = " AND BRANCH_CODE IN(" + result.BRANCH_CODE + ")";
                    }
                    else
                    {
                        branchcodequery = " AND BRANCH_CODE =" + "'" + result.BRANCH_CODE + "'";
                    }
                }

                if (result.QUERY_STRING != "" && result.QUERY_STRING != null)
                {

                    if (result.QUERY_STRING.ToLower().Contains(s2))
                    {
                        StringWriter myWriter = new StringWriter();


                        HttpUtility.HtmlDecode(result.QUERY_STRING, myWriter);
                        string myDecodedString = myWriter.ToString();
                        splited_query = myDecodedString.Split('^');
                    }
                }
                else
                {
                    return "WRONGQUERYFAIL";
                }
                if (result.CUSTOMER_CODE != "" && result.CUSTOMER_CODE != null)
                {
                    cust_codeq = "CUSTOMER_CODE,";
                    if (result.CUSTOMER_CODE.Contains(','))
                    {
                        customercodequery = " CUSTOMER_CODE IN(" + result.CUSTOMER_CODE + ")";
                    }
                    else
                    {
                        customercodequery = " CUSTOMER_CODE =" + "'" + result.CUSTOMER_CODE + "'";
                    }
                    //selectquery = $@"{splited_query[0]}  {cust_codeq}{form_codeq}{branch_codeq}{area_codeq} {splited_query[1]} {customercodequery}{documentcodequery}{areacodequery}{branchcodequery} {splited_query[2]} {cust_codeq}{form_codeq}{branch_codeq}{area_codeq}";
                    selectquery = $@"{splited_query[0]}  {cust_codeq} {splited_query[1]} {customercodequery}{effecivedatequery} {splited_query[2]} CUSTOMER_CODE)";
                    //selectquery = selectquery.TrimEnd(',')+")";
                    var entity = this._dbContext.SqlQuery<SchemeListModel>(selectquery).ToList();
                    if (entity.Count > 0)
                    {
                        foreach (var i in entity)
                        {
                            string Query = $@" select to_char(NVL(MAX(REGEXP_SUBSTR(TO_NUMBER(scheme_code), '[^.]+', 1, 1)),0)+1) as max_scheme_code from scheme_setup_details";
                            var max_scheme_code = this._dbContext.SqlQuery<string>(Query).FirstOrDefault();


                            var insertimplementqry = $@"INSERT INTO scheme_setup_details(scheme_code,
        schemevoucherno,status,form_code,account_code,customer_code,item_code,area_code,branch_code,charge_code,
        charge_account_code,charge_rate,company_code,created_by,created_date,deleted_flag,qauntity,
          sales_scheme_value,total_sales,IMPACT_CREATED,REMARKS) VALUES('{result.SCHEME_CODE}','{max_scheme_code}','{result.STATUS}','{result.FORM_CODE}','{result.ACCOUNT_CODE}','{i.CUSTOMER_CODE}','{result.ITEM_CODE}','{result.AREA_CODE}','{result.BRANCH_CODE}','{result.CHARGE_CODE}','{result.CHARGE_ACCOUNT_CODE}',{result.CHARGE_RATE},'{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N',{i.QTY},{i.BONUS_AMT},{i.SALES_AMT},'N','{result.REMARKS}')";
                            _dbContext.ExecuteSqlCommand(insertimplementqry);

                        }
                        implementedflag = true;
                    }
                    else
                    {
                        message = "EMPTYDATAFAIL";
                        return message;
                    }

                }






                if (result.PARTY_TYPE_CODE != "" && result.PARTY_TYPE_CODE != null)
                {
                    party_type_codeq = "PARTY_TYPE_CODE,";

                    if (result.PARTY_TYPE_CODE.Contains(','))
                    {
                        StringBuilder sb = new StringBuilder();

                        char[] delimiterChars = { ',' };
                        string cb = string.Empty;
                        string bb = string.Empty;
                        string[] ptList = result.PARTY_TYPE_CODE.Split(delimiterChars);
                        foreach (var p in ptList)
                        {
                            cb = cb + "'" + p + "'" + ",";
                        }
                        bb = cb.TrimEnd(',');
                        partytypecodequery = " PARTY_TYPE_CODE IN(" + bb + ")";
                    }
                    else
                    {
                        partytypecodequery = " PARTY_TYPE_CODE =" + "'" + result.PARTY_TYPE_CODE + "'";
                    }
                    //selectquery = $@"{splited_query[0]}  {party_type_codeq}{form_codeq}{branch_codeq}{area_codeq} {splited_query[1]} {partytypecodequery}{documentcodequery}{areacodequery}{branchcodequery} {splited_query[2]} {cust_codeq}{form_codeq}{branch_codeq}{area_codeq}";
                    //selectquery = selectquery.TrimEnd(',')+")";
                    selectquery = $@"{splited_query[0]}  {party_type_codeq} {splited_query[1]} {partytypecodequery}{effecivedatequery}{splited_query[2]} PARTY_TYPE_CODE";
                    var entity1 = this._dbContext.SqlQuery<SchemeListModel>(selectquery).ToList();
                    if (entity1.Count > 0)
                    {
                        foreach (var j in entity1)
                        {
                            string Query = $@" select to_char(NVL(MAX(REGEXP_SUBSTR(TO_NUMBER(scheme_code), '[^.]+', 1, 1)),0)+1) as max_scheme_code from scheme_setup_details";
                            var max_scheme_code = this._dbContext.SqlQuery<string>(Query).FirstOrDefault();
                            var SALES_AMT = (j.SALES_AMT == null) ? 0 : j.SALES_AMT;
                            var QTY = (j.QTY == null) ? 0 : j.QTY;
                            var BONUS_AMT = (j.BONUS_AMT == null) ? 0 : j.BONUS_AMT;

                            var insertimplementqry = $@"INSERT INTO scheme_setup_details(scheme_code,
        schemevoucherno,status,form_code,account_code,party_type_code,item_code,area_code,branch_code,charge_code,
        charge_account_code,charge_rate,company_code,created_by,created_date,deleted_flag,qauntity,
          sales_scheme_value,total_sales,IMPACT_CREATED,REMARKS) VALUES('{result.SCHEME_CODE}','{max_scheme_code}','{result.STATUS}','{result.FORM_CODE}','{result.ACCOUNT_CODE}','{j.PARTY_TYPE_CODE}','{result.ITEM_CODE}','{result.AREA_CODE}','{result.BRANCH_CODE}','{result.CHARGE_CODE}','{result.CHARGE_ACCOUNT_CODE}',{result.CHARGE_RATE},'{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N',{QTY},{BONUS_AMT},{SALES_AMT},'N','{result.REMARKS}')";
                            _dbContext.ExecuteSqlCommand(insertimplementqry);

                        }
                        implementedflag = true;
                    }
                    else
                    {
                        message = "FAIL";
                        return message;
                    }

                }



                //}


                //}
                if (implementedflag == true)
                {
                    var updatesqlquery = $@"UPDATE SCHEME_SETUP SET IMPLEMENT_FLAG='Y' WHERE SCHEME_CODE='{schemeCode}'";
                    var updateresult = _dbContext.ExecuteSqlCommand(updatesqlquery);
                    message = "INSERTED";
                }
                else
                {
                    message = "FAIL";
                }




                //if (result > 0)
                //{
                //    message = "DELETED";
                //}

                return message;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string updateSchemeSetup(SchemeModels model)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    string myEncodedString = string.Empty;
                    string mycCodeEncodedString = string.Empty;
                    string myICodeEncodedString = string.Empty;
                    string myDCodeEncodedString = string.Empty;

                    if (model.QUERY_STRING != "")
                    {
                        myEncodedString = HttpUtility.HtmlEncode(model.QUERY_STRING);
                    }
                    if (model.CUSTOMER_CODE != "")
                    {
                        mycCodeEncodedString = HttpUtility.HtmlEncode(model.CUSTOMER_CODE);
                    }
                    if (model.ITEM_CODE != "")
                    {
                        myICodeEncodedString = HttpUtility.HtmlEncode(model.ITEM_CODE);
                    }
                    if (model.PARTY_TYPE_CODE != "")
                    {
                        myDCodeEncodedString = HttpUtility.HtmlEncode(model.PARTY_TYPE_CODE);
                    }
                    var company_code = _workContext.CurrentUserinformation.company_code;
                    var message = string.Empty;
                    string Query = $@"UPDATE SCHEME_SETUP SET SCHEME_EDESC='{model.SCHEME_EDESC}',FORM_CODE='{model.FORM_CODE}',STATUS='{model.STATUS}',SCHEME_TYPE='{model.SCHEME_TYPE}',TYPE='{model.TYPE}',CALCULATION_DAYS=TO_DATE('{model.CALCULATION_DAYS}','MM/dd/yyyy hh12:mi:ss am'), ACCOUNT_CODE='{model.ACCOUNT_CODE}',CUSTOMER_CODE='{mycCodeEncodedString}', PARTY_TYPE_CODE ='{myDCodeEncodedString}', ITEM_CODE='{myICodeEncodedString}', BRANCH_CODE ='{model.BRANCH_CODE}', CHARGE_ACCOUNT_CODE ='{model.CHARGE_ACCOUNT_CODE}', CHARGE_RATE ={model.CHARGE_RATE }, AREA_CODE='{model.AREA_CODE}',CHARGE_CODE='{model.CHARGE_CODE}',QUERY_STRING='{myEncodedString}',EFFECTIVE_FROM=TO_DATE('{model.EFFECTIVE_FROM}','MM/dd/yyyy hh12:mi:ss am'),EFFECTIVE_TO=TO_DATE('{model.EFFECTIVE_TO}','MM/dd/yyyy hh12:mi:ss am'),MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',MODIFY_DATE = SYSDATE,REMARKS='{model.REMARKS}',FORM_EDESC='{model.FORM_EDESC}',AREA_EDESC='{model.AREA_EDESC}',BRANCH_EDESC='{model.BRANCH_EDESC}',CUSTOMER_EDESC='{model.CUSTOMER_EDESC}',ITEM_EDESC='{model.ITEM_EDESC}',PARTY_TYPE_EDESC='{model.PARTY_TYPE_EDESC}'  WHERE SCHEME_CODE = '{model.SCHEME_CODE}'";
                    var entity = this._coreEntity.ExecuteSqlCommand(Query);
                    if (entity > 0)
                    {
                        message = "UPDATED";
                    }
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }

        public List<SchemeModels> getAllScheme(string filter)
        {
            try
            {
                if (string.IsNullOrEmpty(filter))
                {
                    filter = string.Empty;

                }

                var partyTypeQuery = $@"select COALESCE(SCHEME_CODE,' ') SCHEME_CODE
                            ,COALESCE(SCHEME_EDESC,' ') SCHEME_EDESC
                            FROM SCHEME_SETUP 
                            WHERE DELETED_FLAG = 'N' AND COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}' AND (SCHEME_CODE like '%{filter.ToUpperInvariant()}%' 
                            or upper(SCHEME_EDESC) like '%{filter.ToUpperInvariant()}%')";
                var result = _dbContext.SqlQuery<SchemeModels>(partyTypeQuery).ToList();
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private CUSTOMEROPTION GETCUSTOMEROPTION(string customercode)
        {
            var query = $@"SELECT GROUP_SKU_FLAG,MASTER_CUSTOMER_CODE,PRE_CUSTOMER_CODE FROM SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE='{customercode}'";
            var result = _dbContext.SqlQuery<CUSTOMEROPTION>(query).FirstOrDefault();
            return result;
        }
        private ITEMOPTION GETITEMOPTION(string itemcode)
        {
            var query = $@"SELECT GROUP_SKU_FLAG,MASTER_ITEM_CODE,PRE_ITEM_CODE FROM IP_ITEM_MASTER_SETUP WHERE ITEM_CODE='{itemcode}'";
            var result = _dbContext.SqlQuery<ITEMOPTION>(query).FirstOrDefault();
            return result;
        }
        private PARTYTYPEOPTION GETPARTYOPTION(string partytypecode)
        {
            var query = $@"SELECT GROUP_SKU_FLAG,MASTER_ITEM_CODE,PRE_ITEM_CODE FROM IP_PARTY_TYPE_CODE WHERE PARTY_TYPE_CODE='{partytypecode}'";
            var result = _dbContext.SqlQuery<PARTYTYPEOPTION>(query).FirstOrDefault();
            return result;
        }
        public string ImpactSchemeOnVoucher(List<SchemeImplementModel> models, string formcode, string acccode, string chargecode, string chargeAmount)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    var message = string.Empty;
                    var voucherdate = "TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd") + "', 'YYYY-MM-DD')";

                    foreach (var model in models)
                    {
                        int serialno = 1;
                        var newVoucherNo = NewVoucherNo(this._workContext.CurrentUserinformation.company_code, formcode, DateTime.Now.ToString("dd-MMM-yyyy"), "FA_DOUBLE_VOUCHER");
                        if (chargeAmount != "")
                        {
                            if (model.SALES_SCHEME_VALUE > 0)
                            {
                                var ca = (Convert.ToDecimal(model.SALES_SCHEME_VALUE) / 100) * Convert.ToDecimal(chargeAmount);
                                var afterdeduction_ca = Convert.ToDecimal(model.SALES_SCHEME_VALUE) - ca;

                                if (model.CUSTOMER_CODE != null)
                                {
                                    //FA_DOUBLE_VOUCHER
                                    var insertQuery = $@"INSERT INTO FA_DOUBLE_VOUCHER (EFFECTIVE_DATE,DIVISION_CODE,REMARKS,MANUAL_NO,VOUCHER_DATE,VOUCHER_NO,TRANSACTION_TYPE,PARTICULARS,
                       BUDGET_FLAG,AMOUNT,ACC_CODE,SERIAL_NO,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SESSION_ROWID,
					   CURRENCY_CODE,EXCHANGE_RATE)
                       VALUES (SYSDATE,'','SCHEME','78',{voucherdate},'{newVoucherNo}','DR','SCHEME','L',{model.SALES_SCHEME_VALUE},'{model.ACCOUNT_CODE}','{serialno}',
					   '{formcode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',
					   '{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','123456789','NRS',1)";
                                    _dbContext.ExecuteSqlCommand(insertQuery);

                                    //FA_VOUCHER_SUB_DETAIL
                                    var subledgeracccodecnt = getSubledgerCodeByAccCodeForScheme(model.ACCOUNT_CODE);
                                    var sub_code = GetSubCodeByCustomerCode(model.CUSTOMER_CODE);

                                    if (sub_code.Count > 0)
                                    {

                                        var party_type_code = getPartyTypeCodeBySubCode(model.CUSTOMER_CODE);
                                        foreach (var scode in sub_code)
                                        {
                                            //var s_no = serialno + 1;
                                            if (scode.SUB_CODE != "" && scode.ACC_CODE != "")
                                            {
                                                string acccodequery = $@"select acc_code from fa_chart_of_accounts_setup where company_code='{_workContext.CurrentUserinformation.company_code}' and ind_tds_flag='N' and ind_vat_flag='N' and acc_code='{scode.ACC_CODE}'";
                                                var resultacccode = _dbContext.SqlQuery<string>(acccodequery).FirstOrDefault();


                                                if (resultacccode != null)
                                                {
                                                    var insertQuery1 = $@"INSERT INTO FA_DOUBLE_VOUCHER (EFFECTIVE_DATE,DIVISION_CODE,REMARKS,MANUAL_NO,VOUCHER_DATE,VOUCHER_NO,TRANSACTION_TYPE,PARTICULARS,
                       BUDGET_FLAG,AMOUNT,ACC_CODE,SERIAL_NO,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SESSION_ROWID,
					   CURRENCY_CODE,EXCHANGE_RATE)
                       VALUES (SYSDATE,'','SCHEME','78',{voucherdate},'{newVoucherNo}','CR','SCHEME','E',{afterdeduction_ca},'{resultacccode}','{serialno + 1}',
					   '{formcode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',
					   '{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','123456789','NRS',1)";

                                                    _dbContext.ExecuteSqlCommand(insertQuery1);



                                                    string maxtransnoquerySubledger = string.Format(@"SELECT TO_CHAR(NVL(MAX(TO_NUMBER(TRANSACTION_NO)+1),1))  as TRANSACTIONNO  FROM FA_VOUCHER_SUB_DETAIL  WHERE TRANSACTION_NO IS NOT NULL");
                                                    string newMaxTransNoForSubLedger = _dbContext.SqlQuery<string>(maxtransnoquerySubledger).FirstOrDefault();

                                                    string insertSubLedgerQuery = $@"INSERT INTO FA_VOUCHER_SUB_DETAIL (TRANSACTION_NO,FORM_CODE,
                                                                   VOUCHER_DATE,VOUCHER_NO,SUB_CODE,ACC_CODE,
                                                                   PARTICULARS,TRANSACTION_TYPE,DR_AMOUNT,CR_AMOUNT,
                                                                   BRANCH_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,
                                                                   SERIAL_NO,CURRENCY_CODE,EXCHANGE_RATE,SYN_ROWID,PARTY_TYPE_CODE) 
                                                                   VALUES('{newMaxTransNoForSubLedger}','{formcode}',{voucherdate},'{newVoucherNo}','{scode.SUB_CODE}',
																   '{resultacccode}','SCHEME','DR',{model.SALES_SCHEME_VALUE},0,'{_workContext.CurrentUserinformation.branch_code}',
																   '{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',
																   SYSDATE,'N','{serialno + 1}','NRS',1,'','{party_type_code}'
																   )";
                                                    _dbContext.ExecuteSqlCommand(insertSubLedgerQuery);
                                                }



                                            }

                                        }

                                    }
                                    else
                                    {
                                        trans.Rollback();
                                        message = "NOTMAPPED_C" + model.CUSTOMER_CODE;
                                        return message;
                                    }
                                    var insertmasterQuery = $@"INSERT INTO MASTER_TRANSACTION(VOUCHER_NO,VOUCHER_AMOUNT,FORM_CODE,
                COMPANY_CODE,BRANCH_CODE,CREATED_BY,DELETED_FLAG,CURRENCY_CODE,CREATED_DATE,VOUCHER_DATE,EXCHANGE_RATE,
                REFERENCE_NO) VALUES('{newVoucherNo}',{model.SALES_SCHEME_VALUE},'{formcode}','{_workContext.CurrentUserinformation.company_code}',
				'{_workContext.CurrentUserinformation.branch_code}','{_workContext.CurrentUserinformation.login_code}','N','NRS',SYSDATE,{voucherdate},1,'78')";
                                    _dbContext.ExecuteSqlCommand(insertmasterQuery);
                                    //}



                                    //TDS
                                    var tdcount = CheckIsTDSByAccCodeScheme(model.CHARGE_ACCOUNT_CODE);
                                    if (tdcount > 0)
                                    {
                                        if (model.CHARGE_RATE == "" && model.CHARGE_RATE == null)
                                        {
                                            model.CHARGE_RATE = "0";
                                            if (model.SALES_DISCOUNT != null)
                                            {
                                                string inserttdsQuery = $@"INSERT INTO FA_DC_TDS_INVOICE(SERIAL_NO,INVOICE_NO,INVOICE_DATE,FORM_CODE,CS_CODE,ACC_CODE,TDS_TYPE_CODE,TAXABLE_AMOUNT,TDS_PERCENT,TDS_AMOUNT,CURRENCY_CODE,EXCHANGE_RATE,COMPANY_CODE,BRANCH_CODE,DELETED_FLAG,CREATED_BY,CREATED_DATE,TRAN_TYPE)
                                                                              VALUES({serialno},'{newVoucherNo}',trunc(SYSDATE),'{formcode}','{model.CUSTOMER_CODE}',
                                                                             '{acccode}','{chargecode}',{model.SALES_SCHEME_VALUE},{chargeAmount},{afterdeduction_ca},'NRS',{1},'{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}','N','{_workContext.CurrentUserinformation.login_code.ToUpper()}',SYSDATE,'CR')";
                                                _dbContext.ExecuteSqlCommand(inserttdsQuery);
                                            }
                                        }
                                    }
                                }
                                if (model.PARTY_TYPE_CODE != null)
                                {


                                    //FA_DOUBLE_VOUCHER
                                    var insertQuery = $@"INSERT INTO FA_DOUBLE_VOUCHER (EFFECTIVE_DATE,DIVISION_CODE,REMARKS,MANUAL_NO,VOUCHER_DATE,VOUCHER_NO,TRANSACTION_TYPE,PARTICULARS,
                       BUDGET_FLAG,AMOUNT,ACC_CODE,SERIAL_NO,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SESSION_ROWID,
					   CURRENCY_CODE,EXCHANGE_RATE)
                       VALUES (SYSDATE,'','SCHEME','78',{voucherdate},'{newVoucherNo}','DR','SCHEME','E',{model.SALES_SCHEME_VALUE},'{model.ACCOUNT_CODE}','{serialno}',
					   '{formcode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',
					   '{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','123456789','NRS',1)";
                                    _dbContext.ExecuteSqlCommand(insertQuery);

                                    var sub_code = GetSubCodeByPartyTypeVatno(model.PARTY_TYPE_CODE);
                                    if (sub_code.Count > 0)
                                    {
                                        var s_no = serialno + 1;
                                        var party_type_code = getPartyTypeCodeBySubCode(model.PARTY_TYPE_CODE);
                                        foreach (var scode in sub_code)
                                        {

                                            if (scode.SUB_CODE != "" && scode.ACC_CODE != "")
                                            {
                                                string acccodequery = $@"select acc_code from fa_chart_of_accounts_setup where company_code='{_workContext.CurrentUserinformation.company_code}' and  acc_code='{scode.ACC_CODE}' and ind_tds_flag='N' and ind_vat_flag='N'";
                                                var resultacccode = _dbContext.SqlQuery<string>(acccodequery).FirstOrDefault();


                                                if (resultacccode != null)
                                                {
                                                    var insertQuery1 = $@"INSERT INTO FA_DOUBLE_VOUCHER (EFFECTIVE_DATE,DIVISION_CODE,REMARKS,MANUAL_NO,VOUCHER_DATE,VOUCHER_NO,TRANSACTION_TYPE,PARTICULARS,
                                               BUDGET_FLAG,AMOUNT,ACC_CODE,SERIAL_NO,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SESSION_ROWID,
                                CURRENCY_CODE,EXCHANGE_RATE)
                                               VALUES (SYSDATE,'','SCHEME','78',{voucherdate},'{newVoucherNo}','CR','SCHEME','L',{afterdeduction_ca},'{scode.ACC_CODE}','{s_no}',
                                '{formcode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',
                                '{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','123456789','NRS',1)";

                                                    _dbContext.ExecuteSqlCommand(insertQuery1);



                                                    string maxtransnoquerySubledger = string.Format(@"SELECT TO_CHAR(NVL(MAX(TO_NUMBER(TRANSACTION_NO)+1),1))  as TRANSACTIONNO  FROM FA_VOUCHER_SUB_DETAIL  WHERE TRANSACTION_NO IS NOT NULL");
                                                    string newMaxTransNoForSubLedger = _dbContext.SqlQuery<string>(maxtransnoquerySubledger).FirstOrDefault();

                                                    string insertSubLedgerQuery = $@"INSERT INTO FA_VOUCHER_SUB_DETAIL (TRANSACTION_NO,FORM_CODE,
                                                                                           VOUCHER_DATE,VOUCHER_NO,SUB_CODE,ACC_CODE,
                                                                                           PARTICULARS,TRANSACTION_TYPE,DR_AMOUNT,CR_AMOUNT,
                                                                                           BRANCH_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,
                                                                                           SERIAL_NO,CURRENCY_CODE,EXCHANGE_RATE,SYN_ROWID,PARTY_TYPE_CODE) 
                                                                                           VALUES('{newMaxTransNoForSubLedger}','{formcode}',{voucherdate},'{newVoucherNo}','{scode.SUB_CODE}',
                                								   '{scode.ACC_CODE}','SCHEME','CR',0,{afterdeduction_ca},'{_workContext.CurrentUserinformation.branch_code}',
                                								   '{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',
                                								   SYSDATE,'N','{s_no}','NRS',1,'','{model.PARTY_TYPE_CODE}'
                                								   )";
                                                    _dbContext.ExecuteSqlCommand(insertSubLedgerQuery);
                                                }
                                                else
                                                {
                                                    var insertQuery1 = $@"INSERT INTO FA_DOUBLE_VOUCHER (EFFECTIVE_DATE,DIVISION_CODE,REMARKS,MANUAL_NO,VOUCHER_DATE,VOUCHER_NO,TRANSACTION_TYPE,PARTICULARS,
                                               BUDGET_FLAG,AMOUNT,ACC_CODE,SERIAL_NO,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SESSION_ROWID,
                                CURRENCY_CODE,EXCHANGE_RATE)
                                               VALUES (SYSDATE,'','SCHEME','78',{voucherdate},'{newVoucherNo}','CR','SCHEME','L',{ca},'{scode.ACC_CODE}','{s_no}',
                                '{formcode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',
                                '{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','123456789','NRS',1)";

                                                    _dbContext.ExecuteSqlCommand(insertQuery1);



                                                    string maxtransnoquerySubledger = string.Format(@"SELECT TO_CHAR(NVL(MAX(TO_NUMBER(TRANSACTION_NO)+1),1))  as TRANSACTIONNO  FROM FA_VOUCHER_SUB_DETAIL  WHERE TRANSACTION_NO IS NOT NULL");
                                                    string newMaxTransNoForSubLedger = _dbContext.SqlQuery<string>(maxtransnoquerySubledger).FirstOrDefault();

                                                    string insertSubLedgerQuery = $@"INSERT INTO FA_VOUCHER_SUB_DETAIL (TRANSACTION_NO,FORM_CODE,
                                                                                           VOUCHER_DATE,VOUCHER_NO,SUB_CODE,ACC_CODE,
                                                                                           PARTICULARS,TRANSACTION_TYPE,DR_AMOUNT,CR_AMOUNT,
                                                                                           BRANCH_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,
                                                                                           SERIAL_NO,CURRENCY_CODE,EXCHANGE_RATE,SYN_ROWID,PARTY_TYPE_CODE) 
                                                                                           VALUES('{newMaxTransNoForSubLedger}','{formcode}',{voucherdate},'{newVoucherNo}','{scode.SUB_CODE}',
                                								   '{scode.ACC_CODE}','SCHEME','CR',0,{ca},'{_workContext.CurrentUserinformation.branch_code}',
                                								   '{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',
                                								   SYSDATE,'N','{s_no}','NRS',1,'','{model.PARTY_TYPE_CODE}'
                                								   )";
                                                    _dbContext.ExecuteSqlCommand(insertSubLedgerQuery);
                                                }

                                                s_no++;

                                            }

                                        }


                                    }
                                    else
                                    {
                                        trans.Rollback();
                                        message = "NOTMAPPED_P_" + model.PARTY_TYPE_CODE;
                                        return message;
                                    }
                                    var insertmasterQuery = $@"INSERT INTO MASTER_TRANSACTION(VOUCHER_NO,VOUCHER_AMOUNT,FORM_CODE,
                COMPANY_CODE,BRANCH_CODE,CREATED_BY,DELETED_FLAG,CURRENCY_CODE,CREATED_DATE,VOUCHER_DATE,EXCHANGE_RATE,
                REFERENCE_NO) VALUES('{newVoucherNo}',{model.SALES_SCHEME_VALUE},'{formcode}','{_workContext.CurrentUserinformation.company_code}',
				'{_workContext.CurrentUserinformation.branch_code}','{_workContext.CurrentUserinformation.login_code}','N','NRS',SYSDATE,SYSDATE,1,'78')";
                                    _dbContext.ExecuteSqlCommand(insertmasterQuery);

                                    if (chargeAmount != "")
                                    {
                                        model.CHARGE_RATE = "0";
                                        if (afterdeduction_ca != null)
                                        {
                                            var sub_code1 = GetSubCodeByPartyTypeVatno(model.PARTY_TYPE_CODE);
                                            if (sub_code1.Count > 0)
                                            {
                                                foreach (var scode1 in sub_code1)
                                                {
                                                    if (scode1.SUB_CODE != "" && scode1.ACC_CODE != "")
                                                    {
                                                        string acccodequery = $@"select acc_code from fa_chart_of_accounts_setup where company_code='{_workContext.CurrentUserinformation.company_code}' and  acc_code='{scode1.ACC_CODE}' and ind_tds_flag='Y' and ind_vat_flag='N'";
                                                        var resultacccode1 = _dbContext.SqlQuery<string>(acccodequery).FirstOrDefault();
                                                        if (resultacccode1 != null)
                                                        {
                                                            string inserttdsQuery = $@"INSERT INTO FA_DC_TDS_INVOICE(SERIAL_NO,INVOICE_NO,INVOICE_DATE,MANUAL_NO,FORM_CODE,CS_CODE,ACC_CODE,TDS_TYPE_CODE,TAXABLE_AMOUNT,TDS_PERCENT,TDS_AMOUNT,CURRENCY_CODE,EXCHANGE_RATE,COMPANY_CODE,BRANCH_CODE,DELETED_FLAG,CREATED_BY,CREATED_DATE,TRAN_TYPE)
                                                                              VALUES({serialno + 1},'{newVoucherNo}',trunc(SYSDATE),'{newVoucherNo}',{formcode},'{scode1.SUB_CODE}',
                                                                             '{resultacccode1}','101',{model.SALES_SCHEME_VALUE},{chargeAmount},{ca},'NRS',{1},'{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}','N','{_workContext.CurrentUserinformation.login_code.ToUpper()}',SYSDATE,'CR')";
                                                            _dbContext.ExecuteSqlCommand(inserttdsQuery);
                                                        }

                                                    }
                                                }
                                            }


                                        }
                                    }
                                    //}                               
                                }
                            }
                        }
                        else
                        {

                            if (model.CUSTOMER_CODE != null)
                            {
                                //foreach (var customer_code in customer_codes)
                                // {
                                //FA_DOUBLE_VOUCHER
                                var insertQuery = $@"INSERT INTO FA_DOUBLE_VOUCHER (EFFECTIVE_DATE,DIVISION_CODE,REMARKS,MANUAL_NO,VOUCHER_DATE,VOUCHER_NO,TRANSACTION_TYPE,PARTICULARS,
                       BUDGET_FLAG,AMOUNT,ACC_CODE,SERIAL_NO,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SESSION_ROWID,
					   CURRENCY_CODE,EXCHANGE_RATE)
                       VALUES (SYSDATE,'','SCHEME','78',{voucherdate},'{newVoucherNo}','DR','SCHEME','L',{model.SALES_SCHEME_VALUE},'{model.ACCOUNT_CODE}','{serialno}',
					   '{formcode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',
					   '{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','123456789','NRS',1)";
                                _dbContext.ExecuteSqlCommand(insertQuery);
                                //            //MASTER_TRANSACTION
                                //            var insertmasterQuery = $@"INSERT INTO MASTER_TRANSACTION(VOUCHER_NO,VOUCHER_AMOUNT,FORM_CODE,
                                //COMPANY_CODE,BRANCH_CODE,CREATED_BY,DELETED_FLAG,CREATED_DATE,VOUCHER_DATE,CURRENCY_CODE,EXCHANGE_RATE,REFERENCE_NO) VALUES('{newVoucherNo}',{model.SALES_SCHEME_VALUE},
                                //'{formcode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',
                                //'{_workContext.CurrentUserinformation.login_code}','N',SYSDATE,SYSDATE,'NRS',1,'{newVoucherNo}')";
                                //                _dbContext.ExecuteSqlCommand(insertmasterQuery);

                                //FA_VOUCHER_SUB_DETAIL
                                var subledgeracccodecnt = getSubledgerCodeByAccCodeForScheme(model.ACCOUNT_CODE);
                                //if (subledgeracccodecnt > 0)
                                //{
                                var sub_code = GetSubCodeByCustomerCode(model.CUSTOMER_CODE);

                                if (sub_code.Count > 0)
                                {

                                    var party_type_code = getPartyTypeCodeBySubCode(model.CUSTOMER_CODE);
                                    foreach (var scode in sub_code)
                                    {
                                        if (scode.SUB_CODE != "" && scode.ACC_CODE != "")
                                        {
                                            string acccodequery = $@"select acc_code from fa_chart_of_accounts_setup where company_code='{_workContext.CurrentUserinformation.company_code}' and ind_tds_flag='N' and ind_vat_flag='N' and acc_code='{scode.ACC_CODE}' and ind_tds_flag='N' and ind_vat_flag='N'";
                                            var resultacccode = _dbContext.SqlQuery<string>(acccodequery).FirstOrDefault();


                                            if (resultacccode != null)
                                            {
                                                var insertQuery1 = $@"INSERT INTO FA_DOUBLE_VOUCHER (EFFECTIVE_DATE,DIVISION_CODE,REMARKS,MANUAL_NO,VOUCHER_DATE,VOUCHER_NO,TRANSACTION_TYPE,PARTICULARS,
                       BUDGET_FLAG,AMOUNT,ACC_CODE,SERIAL_NO,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SESSION_ROWID,
					   CURRENCY_CODE,EXCHANGE_RATE)
                       VALUES (SYSDATE,'','SCHEME','78',{voucherdate},'{newVoucherNo}','CR','SCHEME','E',{model.SALES_SCHEME_VALUE},'{resultacccode}','{serialno + 1}',
					   '{formcode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',
					   '{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','123456789','NRS',1)";

                                                _dbContext.ExecuteSqlCommand(insertQuery1);



                                                string maxtransnoquerySubledger = string.Format(@"SELECT TO_CHAR(NVL(MAX(TO_NUMBER(TRANSACTION_NO)+1),1))  as TRANSACTIONNO  FROM FA_VOUCHER_SUB_DETAIL  WHERE TRANSACTION_NO IS NOT NULL");
                                                string newMaxTransNoForSubLedger = _dbContext.SqlQuery<string>(maxtransnoquerySubledger).FirstOrDefault();

                                                string insertSubLedgerQuery = $@"INSERT INTO FA_VOUCHER_SUB_DETAIL (TRANSACTION_NO,FORM_CODE,
                                                                   VOUCHER_DATE,VOUCHER_NO,SUB_CODE,ACC_CODE,
                                                                   PARTICULARS,TRANSACTION_TYPE,DR_AMOUNT,CR_AMOUNT,
                                                                   BRANCH_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,
                                                                   SERIAL_NO,CURRENCY_CODE,EXCHANGE_RATE,SYN_ROWID,PARTY_TYPE_CODE) 
                                                                   VALUES('{newMaxTransNoForSubLedger}','{formcode}',{voucherdate},'{newVoucherNo}','{scode.SUB_CODE}',
																   '{resultacccode}','SCHEME','DR',{model.SALES_SCHEME_VALUE},0,'{_workContext.CurrentUserinformation.branch_code}',
																   '{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',
																   SYSDATE,'N','{serialno + 1}','NRS',1,'','{party_type_code}'
																   )";
                                                _dbContext.ExecuteSqlCommand(insertSubLedgerQuery);
                                            }



                                        }

                                    }

                                }
                                else
                                {
                                    trans.Rollback();
                                    message = "NOTMAPPED";
                                    return message;
                                }
                                var insertmasterQuery = $@"INSERT INTO MASTER_TRANSACTION(VOUCHER_NO,VOUCHER_AMOUNT,FORM_CODE,
                COMPANY_CODE,BRANCH_CODE,CREATED_BY,DELETED_FLAG,CURRENCY_CODE,CREATED_DATE,VOUCHER_DATE,EXCHANGE_RATE,
                REFERENCE_NO) VALUES('{newVoucherNo}',{model.SALES_SCHEME_VALUE},'{formcode}','{_workContext.CurrentUserinformation.company_code}',
				'{_workContext.CurrentUserinformation.branch_code}','{_workContext.CurrentUserinformation.login_code}','N','NRS',SYSDATE,{voucherdate},1,'78')";
                                _dbContext.ExecuteSqlCommand(insertmasterQuery);
                                //}



                                //TDS
                                var tdcount = CheckIsTDSByAccCodeScheme(model.CHARGE_ACCOUNT_CODE);
                                if (tdcount > 0)
                                {
                                    if (model.CHARGE_RATE == "" && model.CHARGE_RATE == null)
                                    {
                                        model.CHARGE_RATE = "0";
                                        if (model.SALES_DISCOUNT != null)
                                        {
                                            string inserttdsQuery = $@"INSERT INTO FA_DC_TDS_INVOICE(SERIAL_NO,INVOICE_NO,INVOICE_DATE,FORM_CODE,CS_CODE,ACC_CODE,TDS_TYPE_CODE,TAXABLE_AMOUNT,TDS_PERCENT,TDS_AMOUNT,CURRENCY_CODE,EXCHANGE_RATE,COMPANY_CODE,BRANCH_CODE,DELETED_FLAG,CREATED_BY,CREATED_DATE,TRAN_TYPE)
                                                                              VALUES({serialno},'{newVoucherNo}',trunc(SYSDATE),'{formcode}','{model.CUSTOMER_CODE}',
                                                                             '{acccode}','{chargecode}',{model.SALES_SCHEME_VALUE},{model.CHARGE_RATE},{model.SALES_DISCOUNT},'NRS',{1},'{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}','N','{_workContext.CurrentUserinformation.login_code.ToUpper()}',SYSDATE,'CR')";
                                            _dbContext.ExecuteSqlCommand(inserttdsQuery);
                                        }
                                    }
                                }
                                string updateSchemedtlqey = $@"UPDATE SCHEME_SETUP_DETAILS SET IMPACT_CREATED='Y' WHERE SCHEME_CODE='{models[0].SCHEME_CODE}' AND CUSTOMER_CODE='{model.PARTY_TYPE_CODE}'";
                                _dbContext.ExecuteSqlCommand(updateSchemedtlqey);
                            }
                            if (model.PARTY_TYPE_CODE != null)
                            {


                                //FA_DOUBLE_VOUCHER
                                var insertQuery = $@"INSERT INTO FA_DOUBLE_VOUCHER (EFFECTIVE_DATE,DIVISION_CODE,REMARKS,MANUAL_NO,VOUCHER_DATE,VOUCHER_NO,TRANSACTION_TYPE,PARTICULARS,
                       BUDGET_FLAG,AMOUNT,ACC_CODE,SERIAL_NO,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SESSION_ROWID,
					   CURRENCY_CODE,EXCHANGE_RATE)
                       VALUES (SYSDATE,'','SCHEME','78',{voucherdate},'{newVoucherNo}','DR','SCHEME','E',{model.SALES_SCHEME_VALUE},'{model.ACCOUNT_CODE}','{serialno}',
					   '{formcode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',
					   '{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','123456789','NRS',1)";
                                _dbContext.ExecuteSqlCommand(insertQuery);

                                var sub_code = GetSubCodeByPartyTypeVatno(model.PARTY_TYPE_CODE);
                                if (sub_code.Count > 0)
                                {
                                    var party_type_code = getPartyTypeCodeBySubCode(model.PARTY_TYPE_CODE);
                                    foreach (var scode in sub_code)
                                    {
                                        if (scode.SUB_CODE != "" && scode.ACC_CODE != "")
                                        {
                                            string acccodequery = $@"select acc_code from fa_chart_of_accounts_setup where company_code='{_workContext.CurrentUserinformation.company_code}' and ind_tds_flag='N' and ind_vat_flag='N' and acc_code='{scode.ACC_CODE}'";
                                            var resultacccode = _dbContext.SqlQuery<string>(acccodequery).FirstOrDefault();


                                            if (resultacccode != null)
                                            {
                                                var insertQuery1 = $@"INSERT INTO FA_DOUBLE_VOUCHER (EFFECTIVE_DATE,DIVISION_CODE,REMARKS,MANUAL_NO,VOUCHER_DATE,VOUCHER_NO,TRANSACTION_TYPE,PARTICULARS,
                                               BUDGET_FLAG,AMOUNT,ACC_CODE,SERIAL_NO,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SESSION_ROWID,
                                CURRENCY_CODE,EXCHANGE_RATE)
                                               VALUES (SYSDATE,'','SCHEME','78',{voucherdate},'{newVoucherNo}','CR','SCHEME','L',{model.SALES_SCHEME_VALUE},'{resultacccode}','{serialno + 1}',
                                '{formcode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',
                                '{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','123456789','NRS',1)";

                                                _dbContext.ExecuteSqlCommand(insertQuery1);



                                                string maxtransnoquerySubledger = string.Format(@"SELECT TO_CHAR(NVL(MAX(TO_NUMBER(TRANSACTION_NO)+1),1))  as TRANSACTIONNO  FROM FA_VOUCHER_SUB_DETAIL  WHERE TRANSACTION_NO IS NOT NULL");
                                                string newMaxTransNoForSubLedger = _dbContext.SqlQuery<string>(maxtransnoquerySubledger).FirstOrDefault();

                                                string insertSubLedgerQuery = $@"INSERT INTO FA_VOUCHER_SUB_DETAIL (TRANSACTION_NO,FORM_CODE,
                                                                                           VOUCHER_DATE,VOUCHER_NO,SUB_CODE,ACC_CODE,
                                                                                           PARTICULARS,TRANSACTION_TYPE,DR_AMOUNT,CR_AMOUNT,
                                                                                           BRANCH_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,
                                                                                           SERIAL_NO,CURRENCY_CODE,EXCHANGE_RATE,SYN_ROWID,PARTY_TYPE_CODE) 
                                                                                           VALUES('{newMaxTransNoForSubLedger}','{formcode}',{voucherdate},'{newVoucherNo}','{scode.SUB_CODE}',
                                								   '{resultacccode}','SCHEME','CR',{model.SALES_SCHEME_VALUE},0,'{_workContext.CurrentUserinformation.branch_code}',
                                								   '{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',
                                								   SYSDATE,'N','{serialno + 1}','NRS',1,'','{party_type_code}'
                                								   )";
                                                _dbContext.ExecuteSqlCommand(insertSubLedgerQuery);
                                            }



                                        }

                                    }


                                }

                                else
                                {
                                    trans.Rollback();
                                    message = "NOTMAPPED";
                                    return message;
                                }
                                var insertmasterQuery = $@"INSERT INTO MASTER_TRANSACTION(VOUCHER_NO,VOUCHER_AMOUNT,FORM_CODE,
                COMPANY_CODE,BRANCH_CODE,CREATED_BY,DELETED_FLAG,CURRENCY_CODE,CREATED_DATE,VOUCHER_DATE,EXCHANGE_RATE,
                REFERENCE_NO) VALUES('{newVoucherNo}',{model.SALES_SCHEME_VALUE},'{formcode}','{_workContext.CurrentUserinformation.company_code}',
				'{_workContext.CurrentUserinformation.branch_code}','{_workContext.CurrentUserinformation.login_code}','N','NRS',SYSDATE,SYSDATE,1,'78')";
                                _dbContext.ExecuteSqlCommand(insertmasterQuery);

                                if (model.CHARGE_RATE == "" && model.CHARGE_RATE == null)
                                {
                                    model.CHARGE_RATE = "0";
                                    if (model.SALES_DISCOUNT != null)
                                    {
                                        string inserttdsQuery = $@"INSERT INTO FA_DC_TDS_INVOICE(SERIAL_NO,INVOICE_NO,INVOICE_DATE,FORM_CODE,CS_CODE,ACC_CODE,TDS_TYPE_CODE,TAXABLE_AMOUNT,TDS_PERCENT,TDS_AMOUNT,CURRENCY_CODE,EXCHANGE_RATE,COMPANY_CODE,BRANCH_CODE,DELETED_FLAG,CREATED_BY,CREATED_DATE,TRAN_TYPE)
                                                                              VALUES({serialno + 1},'{newVoucherNo}',trunc(SYSDATE),{formcode},'{model.CUSTOMER_CODE}',
                                                                             '{acccode}','{chargecode}',{model.SALES_SCHEME_VALUE},{model.CHARGE_RATE},{model.SALES_DISCOUNT},'NRS',{1},'{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}','N','{_workContext.CurrentUserinformation.login_code.ToUpper()}',SYSDATE,'CR')";
                                        _dbContext.ExecuteSqlCommand(inserttdsQuery);
                                    }
                                }

                                string updateSchemedtlqey = $@"UPDATE SCHEME_SETUP_DETAILS SET IMPACT_CREATED='Y' WHERE SCHEME_CODE='{models[0].SCHEME_CODE}' AND PARTY_TYPE_CODE='{model.PARTY_TYPE_CODE}'";
                                _dbContext.ExecuteSqlCommand(updateSchemedtlqey);
                                //}                               
                            }
                        }

                        serialno++;
                    }

                    //message = "INSERTED";
                    //string updateSchemedtlqey = $@"UPDATE SCHEME_SETUP_DETAILS SET IMPACT_CREATED='Y' WHERE SCHEME_CODE='{models[0].SCHEME_CODE}'";
                    //_dbContext.ExecuteSqlCommand(updateSchemedtlqey);
                    message = "INSERTED";
                    trans.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw;
                }
            }

        }


        public string ImpactSchemeOnVoucherCustomer(SchemeImplementModel model, string formcode, string acccode, string chargecode, string chargeAmount)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    if (chargeAmount != "")
                    {
                        int serialno = 1;
                        var newVoucherNo = NewVoucherNo(this._workContext.CurrentUserinformation.company_code, formcode, DateTime.Now.ToString("dd-MMM-yyyy"), "FA_DOUBLE_VOUCHER");
                        var message = string.Empty;
                        var voucherdate = "TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd") + "', 'YYYY-MM-DD')";
                        var ca = (Convert.ToDecimal(model.SALES_SCHEME_VALUE) / 100) * Convert.ToDecimal(chargeAmount);
                        var afterdeduction_ca = Convert.ToDecimal(model.SALES_SCHEME_VALUE) - ca;

                        //FA_DOUBLE_VOUCHER
                        var insertQuery = $@"INSERT INTO FA_DOUBLE_VOUCHER (EFFECTIVE_DATE,DIVISION_CODE,REMARKS,MANUAL_NO,VOUCHER_DATE,VOUCHER_NO,TRANSACTION_TYPE,PARTICULARS,
                       BUDGET_FLAG,AMOUNT,ACC_CODE,SERIAL_NO,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SESSION_ROWID,
					   CURRENCY_CODE,EXCHANGE_RATE)
                       VALUES (SYSDATE,'','SCHEME','78',{voucherdate},'{newVoucherNo}','DR','SCHEME','L',{model.SALES_SCHEME_VALUE},'{model.ACCOUNT_CODE}','{serialno}',
					   '{formcode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',
					   '{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','123456789','NRS',1)";
                        _dbContext.ExecuteSqlCommand(insertQuery);

                        //FA_VOUCHER_SUB_DETAIL
                        var subledgeracccodecnt = getSubledgerCodeByAccCodeForScheme(model.ACCOUNT_CODE);

                        var sub_code = GetSubCodeByCustomerCode(model.CUSTOMER_CODE);

                        if (sub_code.Count > 0)
                        {

                            var party_type_code = getPartyTypeCodeBySubCode(model.CUSTOMER_CODE);
                            foreach (var scode in sub_code)
                            {

                                if (scode.SUB_CODE != "" && scode.ACC_CODE != "")
                                {
                                    string acccodequery = $@"select acc_code from fa_chart_of_accounts_setup where company_code='{_workContext.CurrentUserinformation.company_code}' and ind_tds_flag='N' and ind_vat_flag='N' and acc_code='{scode.ACC_CODE}'";
                                    var resultacccode = _dbContext.SqlQuery<string>(acccodequery).FirstOrDefault();


                                    if (resultacccode != null)
                                    {
                                        var insertQuery1 = $@"INSERT INTO FA_DOUBLE_VOUCHER (EFFECTIVE_DATE,DIVISION_CODE,REMARKS,MANUAL_NO,VOUCHER_DATE,VOUCHER_NO,TRANSACTION_TYPE,PARTICULARS,
                       BUDGET_FLAG,AMOUNT,ACC_CODE,SERIAL_NO,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SESSION_ROWID,
					   CURRENCY_CODE,EXCHANGE_RATE)
                       VALUES (SYSDATE,'','SCHEME','78',{voucherdate},'{newVoucherNo}','CR','SCHEME','E',{afterdeduction_ca},'{resultacccode}','{serialno + 1}',
					   '{formcode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',
					   '{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','123456789','NRS',1)";

                                        _dbContext.ExecuteSqlCommand(insertQuery1);



                                        string maxtransnoquerySubledger = string.Format(@"SELECT TO_CHAR(NVL(MAX(TO_NUMBER(TRANSACTION_NO)+1),1))  as TRANSACTIONNO  FROM FA_VOUCHER_SUB_DETAIL  WHERE TRANSACTION_NO IS NOT NULL");
                                        string newMaxTransNoForSubLedger = _dbContext.SqlQuery<string>(maxtransnoquerySubledger).FirstOrDefault();

                                        string insertSubLedgerQuery = $@"INSERT INTO FA_VOUCHER_SUB_DETAIL (TRANSACTION_NO,FORM_CODE,
                                                                   VOUCHER_DATE,VOUCHER_NO,SUB_CODE,ACC_CODE,
                                                                   PARTICULARS,TRANSACTION_TYPE,DR_AMOUNT,CR_AMOUNT,
                                                                   BRANCH_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,
                                                                   SERIAL_NO,CURRENCY_CODE,EXCHANGE_RATE,SYN_ROWID,PARTY_TYPE_CODE) 
                                                                   VALUES('{newMaxTransNoForSubLedger}','{formcode}',{voucherdate},'{newVoucherNo}','{scode.SUB_CODE}',
																   '{resultacccode}','SCHEME','DR',{model.SALES_SCHEME_VALUE},0,'{_workContext.CurrentUserinformation.branch_code}',
																   '{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',
																   SYSDATE,'N','{serialno + 1}','NRS',1,'','{party_type_code}'
																   )";
                                        _dbContext.ExecuteSqlCommand(insertSubLedgerQuery);
                                    }



                                }

                            }

                        }
                        else
                        {
                            trans.Rollback();
                            message = "NOTMAPPED";
                            return message;
                        }
                        var insertmasterQuery = $@"INSERT INTO MASTER_TRANSACTION(VOUCHER_NO,VOUCHER_AMOUNT,FORM_CODE,
                COMPANY_CODE,BRANCH_CODE,CREATED_BY,DELETED_FLAG,CURRENCY_CODE,CREATED_DATE,VOUCHER_DATE,EXCHANGE_RATE,
                REFERENCE_NO) VALUES('{newVoucherNo}',{model.SALES_SCHEME_VALUE},'{formcode}','{_workContext.CurrentUserinformation.company_code}',
				'{_workContext.CurrentUserinformation.branch_code}','{_workContext.CurrentUserinformation.login_code}','N','NRS',SYSDATE,{voucherdate},1,'78')";
                        _dbContext.ExecuteSqlCommand(insertmasterQuery);
                        //}



                        //TDS
                        var tdcount = CheckIsTDSByAccCodeScheme(model.CHARGE_ACCOUNT_CODE);
                        if (tdcount > 0)
                        {
                            if (model.CHARGE_RATE == "" && model.CHARGE_RATE == null)
                            {
                                model.CHARGE_RATE = "0";
                                if (model.SALES_DISCOUNT != null)
                                {
                                    string inserttdsQuery = $@"INSERT INTO FA_DC_TDS_INVOICE(SERIAL_NO,INVOICE_NO,INVOICE_DATE,FORM_CODE,CS_CODE,ACC_CODE,TDS_TYPE_CODE,TAXABLE_AMOUNT,TDS_PERCENT,TDS_AMOUNT,CURRENCY_CODE,EXCHANGE_RATE,COMPANY_CODE,BRANCH_CODE,DELETED_FLAG,CREATED_BY,CREATED_DATE,TRAN_TYPE)
                                                                              VALUES({serialno},'{newVoucherNo}',trunc(SYSDATE),'{formcode}','{model.CUSTOMER_CODE}',
                                                                             '{acccode}','{chargecode}',{model.SALES_SCHEME_VALUE},{chargeAmount},{afterdeduction_ca},'NRS',{1},'{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}','N','{_workContext.CurrentUserinformation.login_code.ToUpper()}',SYSDATE,'CR')";
                                    _dbContext.ExecuteSqlCommand(inserttdsQuery);
                                }
                            }
                        }


                    }
                    else
                    {

                    }

                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }

            return "";
        }
        public string ImpactSchemeOnVoucherPartyType(SchemeImplementModel model, string formcode, string acccode, string chargecode, string chargeAmount)
        {
            using (var trans = _coreEntity.Database.BeginTransaction())
            {
                try
                {
                    if (chargeAmount != "")
                    {
                        int serialno = 1;
                        var newVoucherNo = NewVoucherNo(this._workContext.CurrentUserinformation.company_code, formcode, DateTime.Now.ToString("dd-MMM-yyyy"), "FA_DOUBLE_VOUCHER");
                        var message = string.Empty;
                        var voucherdate = "TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd") + "', 'YYYY-MM-DD')";
                        var ca = (Convert.ToDecimal(model.SALES_SCHEME_VALUE) / 100) * Convert.ToDecimal(chargeAmount);
                        var afterdeduction_ca = Convert.ToDecimal(model.SALES_SCHEME_VALUE) - ca;
                        //FA_DOUBLE_VOUCHER
                        var insertQuery = $@"INSERT INTO FA_DOUBLE_VOUCHER (EFFECTIVE_DATE,DIVISION_CODE,REMARKS,MANUAL_NO,VOUCHER_DATE,VOUCHER_NO,TRANSACTION_TYPE,PARTICULARS,
                       BUDGET_FLAG,AMOUNT,ACC_CODE,SERIAL_NO,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SESSION_ROWID,
					   CURRENCY_CODE,EXCHANGE_RATE)
                       VALUES (SYSDATE,'','SCHEME','78',{voucherdate},'{newVoucherNo}','DR','SCHEME','L',{model.SALES_SCHEME_VALUE},'{model.ACCOUNT_CODE}','{serialno}',
					   '{formcode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',
					   '{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','123456789','NRS',1)";
                        _dbContext.ExecuteSqlCommand(insertQuery);

                        var sub_code = GetSubCodeByPartyTypeVatno(model.PARTY_TYPE_CODE);
                        if (sub_code.Count > 0)
                        {
                            var party_type_code = getPartyTypeCodeBySubCode(model.PARTY_TYPE_CODE);
                            foreach (var scode in sub_code)
                            {

                                if (scode.SUB_CODE != "" && scode.ACC_CODE != "")
                                {
                                    string acccodequery = $@"select acc_code from fa_chart_of_accounts_setup where company_code='{_workContext.CurrentUserinformation.company_code}' and  acc_code='{scode.ACC_CODE}' and ind_tds_flag='N' and ind_vat_flag='N'";
                                    var resultacccode = _dbContext.SqlQuery<string>(acccodequery).FirstOrDefault();


                                    if (resultacccode != null)
                                    {
                                        var insertQuery1 = $@"INSERT INTO FA_DOUBLE_VOUCHER (EFFECTIVE_DATE,DIVISION_CODE,REMARKS,MANUAL_NO,VOUCHER_DATE,VOUCHER_NO,TRANSACTION_TYPE,PARTICULARS,
                                               BUDGET_FLAG,AMOUNT,ACC_CODE,SERIAL_NO,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SESSION_ROWID,
                                CURRENCY_CODE,EXCHANGE_RATE)
                                               VALUES (SYSDATE,'','SCHEME','78',{voucherdate},'{newVoucherNo}','CR','SCHEME','E',{model.SALES_SCHEME_VALUE},'{scode.ACC_CODE}','{serialno + 1}',
                                '{formcode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',
                                '{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','123456789','NRS',1)";

                                        _dbContext.ExecuteSqlCommand(insertQuery1);



                                        string maxtransnoquerySubledger = string.Format(@"SELECT TO_CHAR(NVL(MAX(TO_NUMBER(TRANSACTION_NO)+1),1))  as TRANSACTIONNO  FROM FA_VOUCHER_SUB_DETAIL  WHERE TRANSACTION_NO IS NOT NULL");
                                        string newMaxTransNoForSubLedger = _dbContext.SqlQuery<string>(maxtransnoquerySubledger).FirstOrDefault();

                                        string insertSubLedgerQuery = $@"INSERT INTO FA_VOUCHER_SUB_DETAIL (TRANSACTION_NO,FORM_CODE,
                                                                                           VOUCHER_DATE,VOUCHER_NO,SUB_CODE,ACC_CODE,
                                                                                           PARTICULARS,TRANSACTION_TYPE,DR_AMOUNT,CR_AMOUNT,
                                                                                           BRANCH_CODE,COMPANY_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,
                                                                                           SERIAL_NO,CURRENCY_CODE,EXCHANGE_RATE,SYN_ROWID,PARTY_TYPE_CODE) 
                                                                                           VALUES('{newMaxTransNoForSubLedger}','{formcode}',{voucherdate},'{newVoucherNo}','{scode.SUB_CODE}',
                                								   '{scode.ACC_CODE}','SCHEME','DR',{model.SALES_SCHEME_VALUE},0,'{_workContext.CurrentUserinformation.branch_code}',
                                								   '{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.login_code}',
                                								   SYSDATE,'N','{serialno + 1}','NRS',1,'','{model.PARTY_TYPE_CODE}'
                                								   )";
                                        _dbContext.ExecuteSqlCommand(insertSubLedgerQuery);
                                    }



                                }

                            }


                        }
                        else
                        {
                            trans.Rollback();
                            message = "NOTMAPPED";
                            return message;
                        }
                        var insertmasterQuery = $@"INSERT INTO MASTER_TRANSACTION(VOUCHER_NO,VOUCHER_AMOUNT,FORM_CODE,
                COMPANY_CODE,BRANCH_CODE,CREATED_BY,DELETED_FLAG,CURRENCY_CODE,CREATED_DATE,VOUCHER_DATE,EXCHANGE_RATE,
                REFERENCE_NO) VALUES('{newVoucherNo}',{model.SALES_SCHEME_VALUE},'{formcode}','{_workContext.CurrentUserinformation.company_code}',
				'{_workContext.CurrentUserinformation.branch_code}','{_workContext.CurrentUserinformation.login_code}','N','NRS',SYSDATE,SYSDATE,1,'78')";
                        _dbContext.ExecuteSqlCommand(insertmasterQuery);

                        if (chargeAmount != "")
                        {
                            model.CHARGE_RATE = "0";
                            if (afterdeduction_ca != null)
                            {
                                var sub_code1 = GetSubCodeByPartyTypeVatno(model.PARTY_TYPE_CODE);
                                if (sub_code1.Count > 0)
                                {
                                    foreach (var scode1 in sub_code1)
                                    {
                                        if (scode1.SUB_CODE != "" && scode1.ACC_CODE != "")
                                        {
                                            string acccodequery = $@"select acc_code from fa_chart_of_accounts_setup where company_code='{_workContext.CurrentUserinformation.company_code}' and  acc_code='{scode1.ACC_CODE}' and ind_tds_flag='Y' and ind_vat_flag='N'";
                                            var resultacccode1 = _dbContext.SqlQuery<string>(acccodequery).FirstOrDefault();
                                            if (resultacccode1 != null)
                                            {
                                                string inserttdsQuery = $@"INSERT INTO FA_DC_TDS_INVOICE(SERIAL_NO,INVOICE_NO,INVOICE_DATE,FORM_CODE,CS_CODE,ACC_CODE,TDS_TYPE_CODE,TAXABLE_AMOUNT,TDS_PERCENT,TDS_AMOUNT,CURRENCY_CODE,EXCHANGE_RATE,COMPANY_CODE,BRANCH_CODE,DELETED_FLAG,CREATED_BY,CREATED_DATE,TRAN_TYPE)
                                                                              VALUES({serialno},'{newVoucherNo}',trunc(SYSDATE),{formcode},'{model.CUSTOMER_CODE}',
                                                                             '{resultacccode1}','101',{model.SALES_SCHEME_VALUE},{chargeAmount},{afterdeduction_ca},'NRS',{1},'{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}','N','{_workContext.CurrentUserinformation.login_code.ToUpper()}',SYSDATE,'CR')";
                                                _dbContext.ExecuteSqlCommand(inserttdsQuery);
                                            }

                                        }
                                    }
                                }


                            }
                        }
                        //} 
                        trans.Commit();
                        message = "INSERTED";
                        return message;
                    }
                    else
                    {

                    }

                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }

            return "INSERTED";
        }
        private string NewVoucherNo(string companycode, string formcode, string transactiondate, string tablename)
        {
            try
            {

                if (companycode != "" && formcode != "" && transactiondate != "" && tablename != "")
                {
                    string query = string.Format(@"select FN_NEW_VOUCHER_NO('{0}','{1}','{2}','{3}') FROM DUAL", companycode, formcode, transactiondate, tablename);
                    string voucherNo = this._dbContext.SqlQuery<string>(query).First();
                    return voucherNo;
                }
                else
                { return ""; }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private List<SUBLEDGEROPT> GetSubCodeByCustomerCode(string customercode)
        {
            try
            {
                if (!string.IsNullOrEmpty(customercode))
                {
                    var query = $@"SELECT SUB_CODE,ACC_CODE FROM FA_SUB_LEDGER_MAP WHERE SUB_CODE='C{customercode.Trim()}'";
                    var subcodedetail = this._dbContext.SqlQuery<SUBLEDGEROPT>(query).ToList();
                    return subcodedetail;
                }
                return null;




            }
            catch (Exception)
            {
                throw;
            }
        }
        private List<SUBLEDGEROPT> GetSubCodeByPartyTypeVatno(string partytypecode)
        {
            try
            {
                if (!string.IsNullOrEmpty(partytypecode))
                {
                    var query = $@"SELECT CUSTOMER_CODE FROM SA_CUSTOMER_SETUP WHERE TPIN_VAT_NO=(SELECT PAN_NO FROM IP_PARTY_TYPE_CODE WHERE PARTY_TYPE_CODE='{partytypecode}' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}')";
                    string customerCode = this._dbContext.SqlQuery<string>(query).First();
                    var squery = $@"SELECT SUB_CODE,ACC_CODE FROM FA_SUB_LEDGER_MAP WHERE SUB_CODE='C{customerCode.Trim()}'  AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'";
                    var subcodedetail = this._dbContext.SqlQuery<SUBLEDGEROPT>(squery).ToList();
                    return subcodedetail;
                }
                return null;




            }
            catch (Exception ex)
            {
                //throw ex;

                List<SUBLEDGEROPT> subcodedetail = new List<SUBLEDGEROPT>();
                return subcodedetail;
            }
        }
        private int getSubledgerCodeByAccCodeForScheme(string accCode)
        {
            try
            {
                var result = string.Empty;
                string query = $@"SELECT TO_CHAR(count(*))
                        FROM FA_SUB_LEDGER_MAP
                        where DELETED_FLAG='N' AND COMPANY_CODE ='{_workContext.CurrentUserinformation.company_code}' AND BRANCH_CODE ='{_workContext.CurrentUserinformation.branch_code}' AND ACC_CODE = '{accCode}'";
                result = this._dbContext.SqlQuery<string>(query).FirstOrDefault();
                return Convert.ToInt32(result);
            }
            catch (Exception)
            {

                return 0;
            }

        }
        private int CheckIsTDSByAccCodeScheme(string accCode)
        {
            try
            {
                var result = string.Empty;
                string query = $@"SELECT TO_CHAR(count(*))
                        FROM FA_CHART_OF_ACCOUNTS_SETUP
                        where IND_TDS_FLAG='Y' AND DELETED_FLAG='N' AND COMPANY_CODE ='{_workContext.CurrentUserinformation.company_code}' AND BRANCH_CODE ='{_workContext.CurrentUserinformation.branch_code}' AND ACC_CODE = '{accCode}'";
                result = this._dbContext.SqlQuery<string>(query).FirstOrDefault();
                return Convert.ToInt32(result);
            }
            catch (Exception)
            {

                return 0;
            }

        }
        public int CheckIsVATByAccCodescheme(string accCode)
        {
            try
            {
                var result = string.Empty;
                string query = $@"SELECT TO_CHAR(count(*))
                        FROM FA_CHART_OF_ACCOUNTS_SETUP
                        where IND_VAT_FLAG='Y' AND DELETED_FLAG='N' AND COMPANY_CODE ='{_workContext.CurrentUserinformation.company_code}' AND BRANCH_CODE ='{_workContext.CurrentUserinformation.branch_code}' AND ACC_CODE = '{accCode}'";
                result = this._dbContext.SqlQuery<string>(query).FirstOrDefault();
                return Convert.ToInt32(result);
            }
            catch (Exception)
            {

                return 0;
            }

        }

        private string getPartyTypeCodeBySubCode(string customercode)
        {
            try
            {
                string Query = $@"   select PARTY_TYPE_CODE from FA_SUB_LEDGER_DEALER_MAP where sub_code='C{customercode.Trim()}'";
                var max_tds_code = this._dbContext.SqlQuery<string>(Query).FirstOrDefault();
                return max_tds_code;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string GetcustomerNameByCode(string code)
        {
            if (code != "undefined" && code != "null")
            {

                string CUSQuery = $@"SELECT CUSTOMER_EDESC FROM SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE='{code}' AND  COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'  AND DELETED_FLAG='N'";
                string CUSdata = this._dbContext.SqlQuery<string>(CUSQuery).FirstOrDefault().ToString();
                return CUSdata;
            }
            else
            {
                return "";
            }
        }
        public string GetParytTypeNameByCode(string code)
        {
            try
            {
                if (code != "undefined" && code != "null")
                {
                    string ACCQuery = $@"SELECT PARTY_TYPE_EDESC FROM ip_party_type_code WHERE PARTY_TYPE_CODE='{code}' AND  COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'  AND DELETED_FLAG='N'";
                    var ACCdata = this._dbContext.SqlQuery<string>(ACCQuery).FirstOrDefault().ToString();
                    return string.IsNullOrEmpty(ACCdata) ? "" : ACCdata;

                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }


        }
        #endregion
        #region Interest Calculation
        public List<InterestCalculationResultModel> CalculateInterestByPara(InterestCalculationModel model)
        {
            if (model.COMPANY_CODE == null)
            {
                var uptodatetimestring = model.UPTO_DATE.ToShortDateString();
                //var datetimestring = generatedDate.HasValue ? generatedDate.Value.ToString("MM/dd/yyyy") : "[N/A]";
                var customerGroupSku = getCustomerGroupSKUFlag(model.CUSTOMER_CODE);
                if (model.CUSTOMER_CODE != null)
                {
                    if (customerGroupSku == "I")
                    {
                        var query = $@"SELECT CUSTOMER_CODE,CUSTOMER_EDESC,CREDIT_DAYS,SUM(BALANCE) BALANCE,SUM(INTEREST) INTEREST FROM (SELECT C.*
		,CASE WHEN DUE_DAYS > CREDIT_DAYS AND CREDIT_DAYS <> 0 AND BALANCE>0 THEN ROUND(((DUE_DAYS - CREDIT_DAYS) * {model.RATE}) * TO_NUMBER(BALANCE) / (100 * 365), 2)
			ELSE 0 END INTEREST FROM (SELECT B.CUSTOMER_CODE,B.CUSTOMER_EDESC,B.VOUCHER_NO VOUCHER_NO,B.MANUAL_NO,TO_CHAR(B.VOUCHER_DATE) VOUCHER_DATE
			,B.CREDIT_DAYS CREDIT_DAYS,TO_CHAR(ROUND(TRUNC(TO_DATE('{uptodatetimestring}','MM/DD/YYYY')) - (VOUCHER_DATE))) DUE_DAYS,B.COMPANY_CODE,TO_CHAR(B.PENDING_AMT) PENDING_AMT
			,TO_CHAR(B.SALES_AMT) SALES_AMT,TO_CHAR(CASE WHEN B.MAXSAL - B.PRE_SALES_AMT > 0 THEN B.MAXSAL - B.PRE_SALES_AMT ELSE 0 END) AS REC_AMT
			,TO_CHAR(B.SALES_AMT - (CASE WHEN B.MAXSAL - B.PRE_SALES_AMT > 0 THEN B.MAXSAL - B.PRE_SALES_AMT ELSE 0 END)) BALANCE
		FROM (SELECT A.CUSTOMER_CODE,A.CUSTOMER_EDESC,A.VOUCHER_NO,A.MANUAL_NO,A.VOUCHER_DATE,A.CREDIT_DAYS,A.DUE_DAYS,A.SALES_AMT
				,A.CUR_SALES_AMT,A.PRE_SALES_AMT,MAX(CUR_COL_AMT) OVER (PARTITION BY CUSTOMER_EDESC) AS MAXSAL,CUR_SALES_AMT - CUR_COL_AMT PENDING_AMT
				,A.COMPANY_CODE FROM (SELECT CUSTOMER_CODE,CUSTOMER_EDESC,VOUCHER_NO,MANUAL_NO,VOUCHER_DATE,CREDIT_DAYS,ROUND(TRUNC(TO_DATE('{uptodatetimestring}','MM/DD/YYYY')) - (VOUCHER_DATE + CREDIT_DAYS)) DUE_DAYS
				,FORM_CODE,CUSTOMER_DRAMOUNT SALES_AMT,CUSTOMER_CRAMOUNT,COMPANY_CODE,NVL(SUM(CUSTOMER_DRAMOUNT) OVER (PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE
				,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0) PRE_SALES_AMT ,NVL(SUM(CUSTOMER_DRAMOUNT) OVER (PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE
				,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) CUR_SALES_AMT,NVL(SUM(CUSTOMER_CRAMOUNT) OVER (
				PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0) PRE_COL_AMT
					,NVL(SUM(CUSTOMER_CRAMOUNT) OVER (PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING
					 AND CURRENT ROW), 0) CUR_COL_AMT FROM V$QUICK_CUSTOMER_BALANCE WHERE DELETED_FLAG = 'N'  ORDER BY VOUCHER_DATE) A ORDER BY A.VOUCHER_DATE
			) B WHERE B.CUR_SALES_AMT - B.MAXSAL > 0 ORDER BY B.VOUCHER_DATE) C WHERE SALES_AMT <> 0)
          WHERE 1 = 1 AND CUSTOMER_CODE = NVL('{model.CUSTOMER_CODE}',CUSTOMER_CODE)	GROUP BY CUSTOMER_CODE,CUSTOMER_EDESC,CREDIT_DAYS";
                        var Interestdetail = this._dbContext.SqlQuery<InterestCalculationResultModel>(query).ToList();
                        return Interestdetail;
                    }
                    else
                    {


                        var mastercodequery = $@"SELECT MASTER_CUSTOMER_CODE FROM SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE='{model.CUSTOMER_CODE}'";
                        var mastercode = this._dbContext.SqlQuery<string>(mastercodequery).FirstOrDefault();


                        var query = $@"SELECT CUSTOMER_CODE,CUSTOMER_EDESC,CREDIT_DAYS,SUM(BALANCE) BALANCE,SUM(INTEREST) INTEREST FROM (SELECT C.*
		,CASE WHEN DUE_DAYS > CREDIT_DAYS AND CREDIT_DAYS <> 0 AND BALANCE>0  THEN ROUND(((DUE_DAYS - CREDIT_DAYS) * {model.RATE}) * TO_NUMBER(BALANCE) / (100 * 365), 2)
			ELSE 0 END INTEREST FROM (SELECT B.CUSTOMER_CODE,B.CUSTOMER_EDESC,B.VOUCHER_NO VOUCHER_NO,B.MANUAL_NO,TO_CHAR(B.VOUCHER_DATE) VOUCHER_DATE
			,B.CREDIT_DAYS CREDIT_DAYS,TO_CHAR(ROUND(TRUNC(TO_DATE('{uptodatetimestring}','MM/DD/YYYY')) - (VOUCHER_DATE))) DUE_DAYS,B.COMPANY_CODE,TO_CHAR(B.PENDING_AMT) PENDING_AMT
			,TO_CHAR(B.SALES_AMT) SALES_AMT,TO_CHAR(CASE WHEN B.MAXSAL - B.PRE_SALES_AMT > 0 THEN B.MAXSAL - B.PRE_SALES_AMT ELSE 0 END) AS REC_AMT
			,TO_CHAR(B.SALES_AMT - (CASE WHEN B.MAXSAL - B.PRE_SALES_AMT > 0 THEN B.MAXSAL - B.PRE_SALES_AMT ELSE 0 END)) BALANCE
		FROM (SELECT A.CUSTOMER_CODE,A.CUSTOMER_EDESC,A.VOUCHER_NO,A.MANUAL_NO,A.VOUCHER_DATE,A.CREDIT_DAYS,A.DUE_DAYS,A.SALES_AMT
				,A.CUR_SALES_AMT,A.PRE_SALES_AMT,MAX(CUR_COL_AMT) OVER (PARTITION BY CUSTOMER_EDESC) AS MAXSAL,CUR_SALES_AMT - CUR_COL_AMT PENDING_AMT
				,A.COMPANY_CODE FROM (SELECT CUSTOMER_CODE,CUSTOMER_EDESC,VOUCHER_NO,MANUAL_NO,VOUCHER_DATE,CREDIT_DAYS,ROUND(TRUNC(TO_DATE('{uptodatetimestring}','MM/DD/YYYY')) - (VOUCHER_DATE + CREDIT_DAYS)) DUE_DAYS
				,FORM_CODE,CUSTOMER_DRAMOUNT SALES_AMT,CUSTOMER_CRAMOUNT,COMPANY_CODE,NVL(SUM(CUSTOMER_DRAMOUNT) OVER (PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE
				,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0) PRE_SALES_AMT ,NVL(SUM(CUSTOMER_DRAMOUNT) OVER (PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE
				,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) CUR_SALES_AMT,NVL(SUM(CUSTOMER_CRAMOUNT) OVER (
				PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0) PRE_COL_AMT
					,NVL(SUM(CUSTOMER_CRAMOUNT) OVER (PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING
					 AND CURRENT ROW), 0) CUR_COL_AMT FROM V$QUICK_CUSTOMER_BALANCE WHERE DELETED_FLAG = 'N'  ORDER BY VOUCHER_DATE) A ORDER BY A.VOUCHER_DATE
			) B WHERE B.CUR_SALES_AMT - B.MAXSAL > 0 ORDER BY B.VOUCHER_DATE) C WHERE SALES_AMT <> 0)
          WHERE 1 = 1 AND INTEREST > 0 AND CUSTOMER_CODE IN(SELECT CUSTOMER_CODE FROM SA_CUSTOMER_SETUP WHERE MASTER_CUSTOMER_CODE LIKE '{mastercode}%')
                GROUP BY CUSTOMER_CODE, CUSTOMER_EDESC, CREDIT_DAYS";
                        var Interestdetail = this._dbContext.SqlQuery<InterestCalculationResultModel>(query).ToList();
                        return Interestdetail;
                    }

                }
                else
                {
                    string cquery = $@"SELECT SUBSTR(SUB_CODE,2,LENGTH(SUB_CODE)) CUSTOMER_CODE FROM FA_SUB_LEDGER_MAP
                                WHERE DELETED_FLAG='N' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' AND ACC_CODE IN({model.GROUP_CODES})";
                    var customerCodeList = _dbContext.SqlQuery<CustomerModels>(cquery).ToList();
                    string str = string.Empty;
                    foreach (var item in customerCodeList)
                        str = str + item.CUSTOMER_CODE + ",";

                    str = str.Remove(str.Length - 1);
                    var query = $@"SELECT CUSTOMER_CODE,CUSTOMER_EDESC,CREDIT_DAYS,SUM(BALANCE) BALANCE,SUM(INTEREST) INTEREST FROM (SELECT C.*
		,CASE WHEN DUE_DAYS > CREDIT_DAYS AND CREDIT_DAYS <> 0 AND BALANCE>0  THEN ROUND(((DUE_DAYS - CREDIT_DAYS) * {model.RATE}) * TO_NUMBER(BALANCE) / (100 * 365), 2)
			ELSE 0 END INTEREST FROM (SELECT B.CUSTOMER_CODE,B.CUSTOMER_EDESC,B.VOUCHER_NO VOUCHER_NO,B.MANUAL_NO,TO_CHAR(B.VOUCHER_DATE) VOUCHER_DATE
			,B.CREDIT_DAYS CREDIT_DAYS,TO_CHAR(ROUND(TRUNC(TO_DATE('{uptodatetimestring}','MM/DD/YYYY')) - (VOUCHER_DATE))) DUE_DAYS,B.COMPANY_CODE,TO_CHAR(B.PENDING_AMT) PENDING_AMT
			,TO_CHAR(B.SALES_AMT) SALES_AMT,TO_CHAR(CASE WHEN B.MAXSAL - B.PRE_SALES_AMT > 0 THEN B.MAXSAL - B.PRE_SALES_AMT ELSE 0 END) AS REC_AMT
			,TO_CHAR(B.SALES_AMT - (CASE WHEN B.MAXSAL - B.PRE_SALES_AMT > 0 THEN B.MAXSAL - B.PRE_SALES_AMT ELSE 0 END)) BALANCE
		FROM (SELECT A.CUSTOMER_CODE,A.CUSTOMER_EDESC,A.VOUCHER_NO,A.MANUAL_NO,A.VOUCHER_DATE,A.CREDIT_DAYS,A.DUE_DAYS,A.SALES_AMT
				,A.CUR_SALES_AMT,A.PRE_SALES_AMT,MAX(CUR_COL_AMT) OVER (PARTITION BY CUSTOMER_EDESC) AS MAXSAL,CUR_SALES_AMT - CUR_COL_AMT PENDING_AMT
				,A.COMPANY_CODE FROM (SELECT CUSTOMER_CODE,CUSTOMER_EDESC,VOUCHER_NO,MANUAL_NO,VOUCHER_DATE,CREDIT_DAYS,ROUND(TRUNC(TO_DATE('{uptodatetimestring}','MM/DD/YYYY')) - (VOUCHER_DATE + CREDIT_DAYS)) DUE_DAYS
				,FORM_CODE,CUSTOMER_DRAMOUNT SALES_AMT,CUSTOMER_CRAMOUNT,COMPANY_CODE,NVL(SUM(CUSTOMER_DRAMOUNT) OVER (PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE
				,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0) PRE_SALES_AMT ,NVL(SUM(CUSTOMER_DRAMOUNT) OVER (PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE
				,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) CUR_SALES_AMT,NVL(SUM(CUSTOMER_CRAMOUNT) OVER (
				PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0) PRE_COL_AMT
					,NVL(SUM(CUSTOMER_CRAMOUNT) OVER (PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING
					 AND CURRENT ROW), 0) CUR_COL_AMT FROM V$QUICK_CUSTOMER_BALANCE WHERE DELETED_FLAG = 'N'  ORDER BY VOUCHER_DATE) A ORDER BY A.VOUCHER_DATE
			) B WHERE B.CUR_SALES_AMT - B.MAXSAL > 0 ORDER BY B.VOUCHER_DATE) C WHERE SALES_AMT <> 0)
          WHERE 1 = 1 AND CUSTOMER_CODE IN({str})	GROUP BY CUSTOMER_CODE,CUSTOMER_EDESC,CREDIT_DAYS";
                    var Interestdetail = this._dbContext.SqlQuery<InterestCalculationResultModel>(query).ToList();
                    return Interestdetail;


                }
            }
            else {
                var uptodatetimestring = model.UPTO_DATE.ToShortDateString();
                //var datetimestring = generatedDate.HasValue ? generatedDate.Value.ToString("MM/dd/yyyy") : "[N/A]";
                var customerGroupSku = getCustomerGroupSKUFlag(model.CUSTOMER_CODE);
                if (model.CUSTOMER_CODE != null)
                {
                    if (customerGroupSku == "I")
                    {
                        var query = $@"SELECT CUSTOMER_CODE,CUSTOMER_EDESC,CREDIT_DAYS,SUM(BALANCE) BALANCE,SUM(INTEREST) INTEREST FROM (SELECT C.*
		,CASE WHEN DUE_DAYS > CREDIT_DAYS AND CREDIT_DAYS <> 0 AND BALANCE>0  THEN ROUND(((DUE_DAYS - CREDIT_DAYS) * {model.RATE}) * TO_NUMBER(BALANCE) / (100 * 365), 2)
			ELSE 0 END INTEREST FROM (SELECT B.CUSTOMER_CODE,B.CUSTOMER_EDESC,B.VOUCHER_NO VOUCHER_NO,B.MANUAL_NO,TO_CHAR(B.VOUCHER_DATE) VOUCHER_DATE
			,B.CREDIT_DAYS CREDIT_DAYS,TO_CHAR(ROUND(TRUNC(TO_DATE('{uptodatetimestring}','MM/DD/YYYY')) - (VOUCHER_DATE))) DUE_DAYS,B.COMPANY_CODE,TO_CHAR(B.PENDING_AMT) PENDING_AMT
			,TO_CHAR(B.SALES_AMT) SALES_AMT,TO_CHAR(CASE WHEN B.MAXSAL - B.PRE_SALES_AMT > 0 THEN B.MAXSAL - B.PRE_SALES_AMT ELSE 0 END) AS REC_AMT
			,TO_CHAR(B.SALES_AMT - (CASE WHEN B.MAXSAL - B.PRE_SALES_AMT > 0 THEN B.MAXSAL - B.PRE_SALES_AMT ELSE 0 END)) BALANCE
		FROM (SELECT A.CUSTOMER_CODE,A.CUSTOMER_EDESC,A.VOUCHER_NO,A.MANUAL_NO,A.VOUCHER_DATE,A.CREDIT_DAYS,A.DUE_DAYS,A.SALES_AMT
				,A.CUR_SALES_AMT,A.PRE_SALES_AMT,MAX(CUR_COL_AMT) OVER (PARTITION BY CUSTOMER_EDESC) AS MAXSAL,CUR_SALES_AMT - CUR_COL_AMT PENDING_AMT
				,A.COMPANY_CODE FROM (SELECT CUSTOMER_CODE,CUSTOMER_EDESC,VOUCHER_NO,MANUAL_NO,VOUCHER_DATE,CREDIT_DAYS,ROUND(TRUNC(TO_DATE('{uptodatetimestring}','MM/DD/YYYY')) - (VOUCHER_DATE + CREDIT_DAYS)) DUE_DAYS
				,FORM_CODE,CUSTOMER_DRAMOUNT SALES_AMT,CUSTOMER_CRAMOUNT,COMPANY_CODE,NVL(SUM(CUSTOMER_DRAMOUNT) OVER (PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE
				,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0) PRE_SALES_AMT ,NVL(SUM(CUSTOMER_DRAMOUNT) OVER (PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE
				,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) CUR_SALES_AMT,NVL(SUM(CUSTOMER_CRAMOUNT) OVER (
				PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0) PRE_COL_AMT
					,NVL(SUM(CUSTOMER_CRAMOUNT) OVER (PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING
					 AND CURRENT ROW), 0) CUR_COL_AMT FROM V$QUICK_CUSTOMER_BALANCE WHERE DELETED_FLAG = 'N' AND COMPANY_CODE='{model.COMPANY_CODE}'  ORDER BY VOUCHER_DATE) A ORDER BY A.VOUCHER_DATE
			) B WHERE B.CUR_SALES_AMT - B.MAXSAL > 0 ORDER BY B.VOUCHER_DATE) C WHERE SALES_AMT <> 0)
          WHERE 1 = 1 AND CUSTOMER_CODE = NVL('{model.CUSTOMER_CODE}',CUSTOMER_CODE)	GROUP BY CUSTOMER_CODE,CUSTOMER_EDESC,CREDIT_DAYS";
                        var Interestdetail = this._dbContext.SqlQuery<InterestCalculationResultModel>(query).ToList();
                        return Interestdetail;
                    }
                    else
                    {


                        var mastercodequery = $@"SELECT MASTER_CUSTOMER_CODE FROM SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE='{model.CUSTOMER_CODE}'";
                        var mastercode = this._dbContext.SqlQuery<string>(mastercodequery).FirstOrDefault();


                        var query = $@"SELECT CUSTOMER_CODE,CUSTOMER_EDESC,CREDIT_DAYS,SUM(BALANCE) BALANCE,SUM(INTEREST) INTEREST FROM (SELECT C.*
		,CASE WHEN DUE_DAYS > CREDIT_DAYS AND CREDIT_DAYS <> 0 AND BALANCE>0  THEN ROUND(((DUE_DAYS - CREDIT_DAYS) * {model.RATE}) * TO_NUMBER(BALANCE) / (100 * 365), 2)
			ELSE 0 END INTEREST FROM (SELECT B.CUSTOMER_CODE,B.CUSTOMER_EDESC,B.VOUCHER_NO VOUCHER_NO,B.MANUAL_NO,TO_CHAR(B.VOUCHER_DATE) VOUCHER_DATE
			,B.CREDIT_DAYS CREDIT_DAYS,TO_CHAR(ROUND(TRUNC(TO_DATE('{uptodatetimestring}','MM/DD/YYYY')) - (VOUCHER_DATE))) DUE_DAYS,B.COMPANY_CODE,TO_CHAR(B.PENDING_AMT) PENDING_AMT
			,TO_CHAR(B.SALES_AMT) SALES_AMT,TO_CHAR(CASE WHEN B.MAXSAL - B.PRE_SALES_AMT > 0 THEN B.MAXSAL - B.PRE_SALES_AMT ELSE 0 END) AS REC_AMT
			,TO_CHAR(B.SALES_AMT - (CASE WHEN B.MAXSAL - B.PRE_SALES_AMT > 0 THEN B.MAXSAL - B.PRE_SALES_AMT ELSE 0 END)) BALANCE
		FROM (SELECT A.CUSTOMER_CODE,A.CUSTOMER_EDESC,A.VOUCHER_NO,A.MANUAL_NO,A.VOUCHER_DATE,A.CREDIT_DAYS,A.DUE_DAYS,A.SALES_AMT
				,A.CUR_SALES_AMT,A.PRE_SALES_AMT,MAX(CUR_COL_AMT) OVER (PARTITION BY CUSTOMER_EDESC) AS MAXSAL,CUR_SALES_AMT - CUR_COL_AMT PENDING_AMT
				,A.COMPANY_CODE FROM (SELECT CUSTOMER_CODE,CUSTOMER_EDESC,VOUCHER_NO,MANUAL_NO,VOUCHER_DATE,CREDIT_DAYS,ROUND(TRUNC(TO_DATE('{uptodatetimestring}','MM/DD/YYYY')) - (VOUCHER_DATE + CREDIT_DAYS)) DUE_DAYS
				,FORM_CODE,CUSTOMER_DRAMOUNT SALES_AMT,CUSTOMER_CRAMOUNT,COMPANY_CODE,NVL(SUM(CUSTOMER_DRAMOUNT) OVER (PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE
				,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0) PRE_SALES_AMT ,NVL(SUM(CUSTOMER_DRAMOUNT) OVER (PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE
				,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) CUR_SALES_AMT,NVL(SUM(CUSTOMER_CRAMOUNT) OVER (
				PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0) PRE_COL_AMT
					,NVL(SUM(CUSTOMER_CRAMOUNT) OVER (PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING
					 AND CURRENT ROW), 0) CUR_COL_AMT FROM V$QUICK_CUSTOMER_BALANCE WHERE DELETED_FLAG = 'N' AND COMPANY_CODE='{model.COMPANY_CODE}'  ORDER BY VOUCHER_DATE) A ORDER BY A.VOUCHER_DATE
			) B WHERE B.CUR_SALES_AMT - B.MAXSAL > 0 ORDER BY B.VOUCHER_DATE) C WHERE SALES_AMT <> 0)
          WHERE 1 = 1 AND INTEREST > 0 AND CUSTOMER_CODE IN(SELECT CUSTOMER_CODE FROM SA_CUSTOMER_SETUP WHERE MASTER_CUSTOMER_CODE LIKE '{mastercode}%')
                GROUP BY CUSTOMER_CODE, CUSTOMER_EDESC, CREDIT_DAYS";
                        var Interestdetail = this._dbContext.SqlQuery<InterestCalculationResultModel>(query).ToList();
                        return Interestdetail;
                    }

                }
                else
                {
                    string cquery = $@"SELECT SUBSTR(SUB_CODE,2,LENGTH(SUB_CODE)) CUSTOMER_CODE FROM FA_SUB_LEDGER_MAP
                                WHERE DELETED_FLAG='N' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' AND ACC_CODE IN({model.GROUP_CODES})";
                    //var customerCodeList = _dbContext.SqlQuery<CustomerModels>(cquery).ToList();
                    //string str = string.Empty;
                    //foreach (var item in customerCodeList)
                    //    str = str + item.CUSTOMER_CODE + ",";

                    //str = str.Remove(str.Length - 1);
                    //string[] splitvalue = str.Split(',');
                    //Console.WriteLine(splitvalue.Length);
                    //string cscode1=null;
                    //string cscode2 = null;

                    //for (int i=0; i<splitvalue.Length/2; i++)
                    //{
                    //    cscode1 = cscode1 + splitvalue[i] + ",";

                    //}
                    //cscode1 = cscode1.Remove(cscode1.Length - 1);

                    //for (int i = splitvalue.Length / 2; i < splitvalue.Length ; i++)
                    //{
                    //    cscode2 = cscode2 + splitvalue[i] + ",";

                    //}
                    //cscode2 = cscode2.Remove(cscode2.Length - 1);


                    var query = $@"SELECT CUSTOMER_CODE,CUSTOMER_EDESC,CREDIT_DAYS,SUM(BALANCE) BALANCE,SUM(INTEREST) INTEREST FROM (SELECT C.*
		,CASE WHEN DUE_DAYS > CREDIT_DAYS AND CREDIT_DAYS <> 0 AND BALANCE>0  THEN ROUND(((DUE_DAYS - CREDIT_DAYS) * {model.RATE}) * TO_NUMBER(BALANCE) / (100 * 365), 2)
			ELSE 0 END INTEREST FROM (SELECT B.CUSTOMER_CODE,B.CUSTOMER_EDESC,B.VOUCHER_NO VOUCHER_NO,B.MANUAL_NO,TO_CHAR(B.VOUCHER_DATE) VOUCHER_DATE
			,B.CREDIT_DAYS CREDIT_DAYS,TO_CHAR(ROUND(TRUNC(TO_DATE('{uptodatetimestring}','MM/DD/YYYY')) - (VOUCHER_DATE))) DUE_DAYS,B.COMPANY_CODE,TO_CHAR(B.PENDING_AMT) PENDING_AMT
			,TO_CHAR(B.SALES_AMT) SALES_AMT,TO_CHAR(CASE WHEN B.MAXSAL - B.PRE_SALES_AMT > 0 THEN B.MAXSAL - B.PRE_SALES_AMT ELSE 0 END) AS REC_AMT
			,TO_CHAR(B.SALES_AMT - (CASE WHEN B.MAXSAL - B.PRE_SALES_AMT > 0 THEN B.MAXSAL - B.PRE_SALES_AMT ELSE 0 END)) BALANCE
		FROM (SELECT A.CUSTOMER_CODE,A.CUSTOMER_EDESC,A.VOUCHER_NO,A.MANUAL_NO,A.VOUCHER_DATE,A.CREDIT_DAYS,A.DUE_DAYS,A.SALES_AMT
				,A.CUR_SALES_AMT,A.PRE_SALES_AMT,MAX(CUR_COL_AMT) OVER (PARTITION BY CUSTOMER_EDESC) AS MAXSAL,CUR_SALES_AMT - CUR_COL_AMT PENDING_AMT
				,A.COMPANY_CODE FROM (SELECT CUSTOMER_CODE,CUSTOMER_EDESC,VOUCHER_NO,MANUAL_NO,VOUCHER_DATE,CREDIT_DAYS,ROUND(TRUNC(TO_DATE('{uptodatetimestring}','MM/DD/YYYY')) - (VOUCHER_DATE + CREDIT_DAYS)) DUE_DAYS
				,FORM_CODE,CUSTOMER_DRAMOUNT SALES_AMT,CUSTOMER_CRAMOUNT,COMPANY_CODE,NVL(SUM(CUSTOMER_DRAMOUNT) OVER (PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE
				,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0) PRE_SALES_AMT ,NVL(SUM(CUSTOMER_DRAMOUNT) OVER (PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE
				,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) CUR_SALES_AMT,NVL(SUM(CUSTOMER_CRAMOUNT) OVER (
				PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0) PRE_COL_AMT
					,NVL(SUM(CUSTOMER_CRAMOUNT) OVER (PARTITION BY CUSTOMER_EDESC ORDER BY VOUCHER_DATE,VOUCHER_NO ROWS BETWEEN UNBOUNDED PRECEDING
					 AND CURRENT ROW), 0) CUR_COL_AMT FROM V$QUICK_CUSTOMER_BALANCE WHERE DELETED_FLAG = 'N' AND COMPANY_CODE='{model.COMPANY_CODE}'  ORDER BY VOUCHER_DATE) A ORDER BY A.VOUCHER_DATE
			) B WHERE B.CUR_SALES_AMT - B.MAXSAL > 0 ORDER BY B.VOUCHER_DATE) C WHERE SALES_AMT <> 0)
          WHERE 1 = 1 AND CUSTOMER_CODE IN({cquery}) GROUP BY CUSTOMER_CODE,CUSTOMER_EDESC,CREDIT_DAYS";
                    var Interestdetail = this._dbContext.SqlQuery<InterestCalculationResultModel>(query).ToList();
                    return Interestdetail;


                }
            }
            



        }

        public List<InterestCalcResDetailModel> CalculateInterestDetailsByPara(InterestCalculationModel model)
        {
            var query = string.Empty;
            if (model.COMPANY_CODE == null)
            {
                var uptodatetimestring = model.UPTO_DATE.ToShortDateString();
                var customerGroupSku = getCustomerGroupSKUFlag(model.CUSTOMER_CODE);
                 query = $@"SELECT customer_code ,customer_edesc ,voucher_no ,voucher_date ,credit_days ,due_date ,(due_days - credit_days) due_days 
,SUM(balance) balance ,SUM(interest) interest FROM ( SELECT c.* ,CASE WHEN due_days > credit_days 
AND credit_days <> 0 AND balance > 0 THEN round(((due_days - credit_days) * {model.RATE})* to_number(balance) / (100 * 365), 2) ELSE 0 END interest 
FROM ( SELECT b.customer_code ,b.customer_edesc ,b.voucher_no voucher_no ,b.manual_no ,b.voucher_date ,
b.credit_days credit_days ,(b.voucher_date + credit_days) due_date 
,ROUND(TRUNC(TO_DATE('{uptodatetimestring}', 'MM/DD/YYYY')) - (voucher_date)) due_days ,b.company_code ,
TO_CHAR(b.pending_amt) pending_amt ,TO_CHAR(b.sales_amt) sales_amt ,TO_CHAR(CASE WHEN b.maxsal - b.pre_sales_amt > 0 
THEN b.maxsal - b.pre_sales_amt ELSE 0 END) AS rec_amt ,TO_CHAR(b.sales_amt - ( CASE WHEN b.maxsal - b.pre_sales_amt > 0 THEN b.maxsal - b.pre_sales_amt ELSE 0 END )) balance 
FROM ( SELECT a.customer_code ,a.customer_edesc ,a.voucher_no ,a.manual_no ,a.voucher_date ,a.credit_days ,a.due_days ,
a.sales_amt ,a.cur_sales_amt ,a.pre_sales_amt ,MAX(cur_col_amt) OVER (PARTITION BY customer_edesc) AS maxsal ,
cur_sales_amt - cur_col_amt pending_amt ,a.company_code FROM ( SELECT customer_code ,customer_edesc ,voucher_no ,
manual_no ,voucher_date ,credit_days ,ROUND(TRUNC(TO_DATE('{uptodatetimestring}', 'MM/DD/YYYY')) - (voucher_date + credit_days))
due_days ,form_code ,customer_dramount sales_amt ,customer_cramount ,company_code ,nvl(SUM(customer_dramount)
OVER ( PARTITION BY customer_edesc ORDER BY voucher_date ,voucher_no ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING ), 0)
pre_sales_amt ,nvl(SUM(customer_dramount) OVER ( PARTITION BY customer_edesc ORDER BY voucher_date ,voucher_no ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW ), 0) cur_sales_amt 
,nvl(SUM(customer_cramount) OVER ( PARTITION BY customer_edesc ORDER BY voucher_date ,voucher_no ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING ), 0) 
pre_col_amt ,nvl(SUM(customer_cramount) OVER ( PARTITION BY customer_edesc ORDER BY voucher_date ,voucher_no ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW ), 0) cur_col_amt 
FROM v$quick_customer_balance WHERE deleted_flag = 'N' ORDER BY voucher_date ) a ORDER BY a.voucher_date ) b WHERE b.cur_sales_amt - b.maxsal > 0 ORDER BY b.voucher_date ) 
c WHERE sales_amt <> 0 ) WHERE 1 = 1 AND customer_code = nvl('{model.CUSTOMER_CODE}', customer_code) AND interest > 0 GROUP BY customer_code ,customer_edesc ,credit_days ,due_date ,voucher_no ,
voucher_date ,due_days ORDER BY voucher_date";
            }
            else
            {
                var uptodatetimestring = model.UPTO_DATE.ToShortDateString();
                var customerGroupSku = getCustomerGroupSKUFlag(model.CUSTOMER_CODE);
                 query = $@"SELECT customer_code ,customer_edesc ,voucher_no ,voucher_date ,credit_days ,due_date ,(due_days - credit_days) due_days 
,SUM(balance) balance ,SUM(interest) interest FROM ( SELECT c.* ,CASE WHEN due_days > credit_days 
AND credit_days <> 0 AND balance > 0 THEN round(((due_days - credit_days) * {model.RATE})* to_number(balance) / (100 * 365), 2) ELSE 0 END interest 
FROM ( SELECT b.customer_code ,b.customer_edesc ,b.voucher_no voucher_no ,b.manual_no ,b.voucher_date ,
b.credit_days credit_days ,(b.voucher_date + credit_days) due_date 
,ROUND(TRUNC(TO_DATE('{uptodatetimestring}', 'MM/DD/YYYY')) - (voucher_date)) due_days ,b.company_code ,
TO_CHAR(b.pending_amt) pending_amt ,TO_CHAR(b.sales_amt) sales_amt ,TO_CHAR(CASE WHEN b.maxsal - b.pre_sales_amt > 0 
THEN b.maxsal - b.pre_sales_amt ELSE 0 END) AS rec_amt ,TO_CHAR(b.sales_amt - ( CASE WHEN b.maxsal - b.pre_sales_amt > 0 THEN b.maxsal - b.pre_sales_amt ELSE 0 END )) balance 
FROM ( SELECT a.customer_code ,a.customer_edesc ,a.voucher_no ,a.manual_no ,a.voucher_date ,a.credit_days ,a.due_days ,
a.sales_amt ,a.cur_sales_amt ,a.pre_sales_amt ,MAX(cur_col_amt) OVER (PARTITION BY customer_edesc) AS maxsal ,
cur_sales_amt - cur_col_amt pending_amt ,a.company_code FROM ( SELECT customer_code ,customer_edesc ,voucher_no ,
manual_no ,voucher_date ,credit_days ,ROUND(TRUNC(TO_DATE('{uptodatetimestring}', 'MM/DD/YYYY')) - (voucher_date + credit_days))
due_days ,form_code ,customer_dramount sales_amt ,customer_cramount ,company_code ,nvl(SUM(customer_dramount)
OVER ( PARTITION BY customer_edesc ORDER BY voucher_date ,voucher_no ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING ), 0)
pre_sales_amt ,nvl(SUM(customer_dramount) OVER ( PARTITION BY customer_edesc ORDER BY voucher_date ,voucher_no ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW ), 0) cur_sales_amt 
,nvl(SUM(customer_cramount) OVER ( PARTITION BY customer_edesc ORDER BY voucher_date ,voucher_no ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING ), 0) 
pre_col_amt ,nvl(SUM(customer_cramount) OVER ( PARTITION BY customer_edesc ORDER BY voucher_date ,voucher_no ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW ), 0) cur_col_amt 
FROM v$quick_customer_balance WHERE deleted_flag = 'N' AND company_code='{model.COMPANY_CODE}' ORDER BY voucher_date ) a ORDER BY a.voucher_date ) b WHERE b.cur_sales_amt - b.maxsal > 0  AND b.company_code ='{model.COMPANY_CODE}' ORDER BY b.voucher_date ) 
c WHERE sales_amt <> 0 ) WHERE 1 = 1 AND customer_code = nvl('{model.CUSTOMER_CODE}', customer_code) AND interest > 0 GROUP BY customer_code ,customer_edesc ,credit_days ,due_date ,voucher_no ,
voucher_date ,due_days ORDER BY voucher_date";
            }


            var Interestdetail = this._dbContext.SqlQuery<InterestCalcResDetailModel>(query).ToList();
                return Interestdetail;
        }


        public string CreateInterestImpact(List<InterestCalculationResultModel> result, InterestCalculcImpacttModel model)
        {
            string msg = string.Empty;
            if (result.Count > 0)
            {

                //foreach (var data in result)
                //{

                string sConnStr1 = ConfigurationManager.ConnectionStrings["NeoErpCoreEntity"].ToString();
               
                string[] tokens = sConnStr1.Split('"');
                //using (OracleConnection objConn = new OracleConnection("DATA SOURCE=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 1521))(CONNECT_DATA =(SERVER = DEDICATED)(SERVICE_NAME = DELL))); PASSWORD=NEOERPQA;USER ID=NEOERPQA"))
                using (OracleConnection objConn = new OracleConnection(tokens[1]))
                {
                    foreach (var data in result)
                    {
                        var CustomerExist = SelectCustomerExist(data.CUSTOMER_CODE, data.UPTO_DATE);
                        if (CustomerExist == true)
                        {
                            msg= "CustomerExist";

                        }
                        else
                        {
                            var uptodatetimestring = data.UPTO_DATE.ToString("dd-MMM-yyyy");
                            OracleCommand objCmd = new OracleCommand();
                            objCmd.Connection = objConn;
                            objCmd.CommandText = "PR_INTEREST_CALC";
                            objCmd.CommandType = CommandType.StoredProcedure;
                            objCmd.Parameters.Add("V_COMPANY_CODE", OracleType.NVarChar).Value = model.COMPANY_CODE;
                            objCmd.Parameters.Add("V_FORM_CODE", OracleType.Number).Value = Convert.ToInt32(model.FORM_CODE);
                            objCmd.Parameters.Add("V_LEDGER_CODE", OracleType.Number).Value = Convert.ToInt32(model.LEDGER_CODE);
                            objCmd.Parameters.Add("V_ACC_CODE", OracleType.Number).Value = Convert.ToInt32(model.ACCOUNT_CODE);
                            objCmd.Parameters.Add("V_BRANCH_CODE", OracleType.NVarChar).Value = model.BRANCH_CODE;
                            Convert.ToInt32(model.CHARGE_ACCOUNT_CODE);
                            objCmd.Parameters.Add("V_TDS_LEDGER_CODE", OracleType.NVarChar).Value = model.CHARGE_ACCOUNT_CODE;
                            objCmd.Parameters.Add("V_CUSTOMER_CODE", OracleType.Number).Value = data.CUSTOMER_CODE;
                            objCmd.Parameters.Add("V_RATE", OracleType.Number).Value = data.RATE;
                            objCmd.Parameters.Add("CONSOLIDATE", OracleType.Char).Value = "";
                            //UPTO_DATE DATE
                            objCmd.Parameters.Add("V_DATE", OracleType.DateTime).Value = uptodatetimestring;
                            try
                            {
                                objConn.Open();
                                objCmd.ExecuteNonQuery();
                                msg= "INSERTED";
                            }
                            catch (Exception ex)
                            {
                                msg= "FAIL";
                            }
                            objConn.Close();


                        }

                    }
                   

                }
                return msg;
                //}

            }

            return msg;
        }

        private bool SelectCustomerExist(string customerCode, DateTime? generatedDate)
        {
            var datetimestring = generatedDate.HasValue ? generatedDate.Value.ToString("MM/dd/yyyy") : "SYSDATE";

            var query = $@"select COUNT(*) AS NUM from FA_INTEREST_CALC_LOG WHERE CUSTOMER_CODE='{customerCode}' AND GENERATED_DATE=TRUNC(to_date('{datetimestring}','mm/dd/yyyy'))";
            var countdtls = this._dbContext.SqlQuery<int>(query).FirstOrDefault();
            if (countdtls > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        private string getCustomerGroupSKUFlag(string customerCode)
        {
            var query = $@"select GROUP_SKU_FLAG AS NUM from SA_CUSTOMER_SETUP WHERE CUSTOMER_CODE='{customerCode}'";
            var gsf = this._dbContext.SqlQuery<string>(query).FirstOrDefault();
            return gsf;



        }

        public List<InterestCalcLogModel> GetInterestCalcLog()
        {
            var query = $@"SELECT ficl.interest_log_code,ficl.customer_code,ficl.interest_amount,fcs.customer_edesc,ficl.generated_date,ficl.created_date,ficl.voucher_no,ficl.voucher_date FROM fa_interest_calc_log ficl,sa_customer_setup fcs WHERE ficl.customer_code = fcs.customer_code and ficl.company_code=fcs.company_code and fcs.deleted_flag='N' and ficl.company_code='{_workContext.CurrentUserinformation.company_code}' order by created_date desc";
            var InterestCalcLog = this._dbContext.SqlQuery<InterestCalcLogModel>(query).ToList();
            return InterestCalcLog;
        }
        #endregion
    }
}
#region Commented Lines
//if (model.CUSTOMER_CODE != null && model.PRE_CUSTOMER_CODE != "")
//{
//    var newprecode = string.Empty;
//    var newMaxMasterCustomerCode = string.Empty;
//    var newmastercode = string.Empty;

//    if (model.CUSTOMER_CODE != null && model.CUSTOMER_CODE != "")
//    {
//        var newMaxCustomerCodeQuery = $@"SELECT MAX(CUSTOMER_CODE)+1 as MAX_CUSTOMER_CODE FROM SA_CUSTOMER_SETUP WHERE COMPANY_CODE = '{_workContext.CurrentUserinformation.company_code}' AND GROUP_SKU_FLAG = 'G'";
//        newMaxCustomerCode = this._dbContext.SqlQuery<int>(newMaxCustomerCodeQuery).FirstOrDefault().ToString();
//        var maxprecodequery = $@"SELECT MAX(substr(MASTER_CUSTOMER_CODE,-instr(reverse(MASTER_CUSTOMER_CODE),'.')+1))+1 FROM SA_CUSTOMER_SETUP WHERE PRE_CUSTOMER_CODE = '{model.PRE_CUSTOMER_CODE}' ";
//        var maxPreCode = this._dbContext.SqlQuery<int>(maxprecodequery).FirstOrDefault().ToString();

//        if (maxPreCode != null)
//        {
//            if (Convert.ToInt32(maxPreCode) <= 9)
//            {
//                newmastercode = "0" + maxPreCode.ToString();
//            }

//        }
//        newmastercode = model.MASTER_CUSTOMER_CODE + "." + maxPreCode;
//        var childsqlquery = $@"INSERT INTO SA_CUSTOMER_SETUP (CUSTOMER_CODE,
//                                    CUSTOMER_EDESC,CUSTOMER_NDESC,PRE_CUSTOMER_CODE,ACC_CODE,
//                                    CUSTOMER_FLAG,PREFIX_TEXT,GROUP_START_NO,REMARKS,
//                                    MASTER_CUSTOMER_CODE,GROUP_SKU_FLAG,COMPANY_CODE,
//                                    CREATED_BY,CREATED_DATE,DELETED_FLAG,BRANCH_CODE)
//                                    VALUES('{newMaxCustomerCode}','{model.CUSTOMER_EDESC}','{model.CUSTOMER_NDESC}','{model.MASTER_CUSTOMER_CODE}','{model.CUSTOMER_ACCOUNT}','{model.CUSTOMER_TYPE}','{model.CUSTOMER_PREFIX}'
//                                        ,'{model.CUSTOMER_STARTID}','{model.REMARKS}','{newmastercode}','{model.GROUP_SKU_FLAG}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.User_id}',TO_DATE('{DateTime.Now.ToString("MM/dd/yyyy")}','MM/dd/yyyy'),
//                                         'N','{_workContext.CurrentUserinformation.branch_code}')";
//        var insertchild = _dbContext.ExecuteSqlCommand(childsqlquery);

//    }


//}
//else
//{
#endregion





