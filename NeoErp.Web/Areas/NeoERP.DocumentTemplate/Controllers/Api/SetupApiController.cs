using NeoERP.DocumentTemplate.Service.Models;
using NeoERP.DocumentTemplate.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NeoErp.Core;
using NeoErp.Core.Caching;
using System.Web;
using NeoErp.Core.Domain;
using NeoErp.Core.Models.Log4NetLoggin;
using System.IO;
using NeoErp.Data;
using Newtonsoft.Json;

namespace NeoERP.DocumentTemplate.Controllers.Api
{
    public class SetupApiController : ApiController
    {

        private IDocumentStup _iDocumentSetup;
        private IFormSetupRepo _iFormSetupRepo;
        private IFormTemplateRepo _FormTemplateRepo;
        private IDbContext _dbContext;
        private IWorkContext _workContext;
        private ICacheManager _cacheManager;
        private readonly ILogErp _logErp;
        private DefaultValueForLog _defaultValueForLog;
        public SetupApiController(IDocumentStup _idocumentSetup, IFormSetupRepo _iFormSetupRepo, IDbContext dbContext, IWorkContext workContext, ICacheManager cacheManager, IFormTemplateRepo FormTemplateRepo)
        {
            this._iDocumentSetup = _idocumentSetup;
            this._iFormSetupRepo = _iFormSetupRepo;
            this._FormTemplateRepo = FormTemplateRepo;
            this._dbContext = dbContext;
            this._workContext = workContext;
            this._cacheManager = cacheManager;
            this._defaultValueForLog = new DefaultValueForLog(this._workContext);
            this._logErp = new LogErp(this,_defaultValueForLog.LogUser,_defaultValueForLog.LogCompany,_defaultValueForLog.LogBranch,_defaultValueForLog.LogTypeCode,_defaultValueForLog.LogModule,_defaultValueForLog.FormCode);
        }

        #region Account
        [HttpPost]
        public HttpResponseMessage DeleteAccountSetupByAccCode(string accCode)
        {
            try
            {
                var result = _iDocumentSetup.DeleteAccountSetupByAccCode(accCode);
                #region CLEAR CACHE
                List<string> keystart = new List<string>();
                keystart.Add("GetChildOfAccountByGroup");
                keystart.Add("getAllAccountMaps");
                keystart.Add("getAccountCode");
                keystart.Add("AllAccountSetupByCode");
                keystart.Add("AllAccountSetupByName");
                keystart.Add("AllFilterAccount");
                keystart.Add("GetAllAccountCode");
                keystart.Add("GetAllChargeAccountSetupByFilter");
                keystart.Add("getAccountCodeWithChild");
                keystart.Add("getAllAccountCodeWithChild");
                keystart.Add("getAllAccountComboCodeWithChild");
                keystart.Add("GetAccountListByAccountCode");
                keystart.Add("GetSubLedgerByAccountCode");
                List<string> Record = new List<string>();
                Record = this._cacheManager.GetAllKeys();
                this._cacheManager.RemoveCacheByKey(keystart, Record);
                #endregion
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpPost]
        public HttpResponseMessage updateAccountByAccCode(AccountSetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.udpateAccountSetup(model);
                if (result == "UPDATED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetChildOfAccountByGroup");
                    keystart.Add("getAllAccountMaps");
                    keystart.Add("getAccountCode");
                    keystart.Add("AllAccountSetupByCode");
                    keystart.Add("AllAccountSetupByName");
                    keystart.Add("AllFilterAccount");
                    keystart.Add("GetAllAccountCode");
                    keystart.Add("GetAllChargeAccountSetupByFilter");
                    keystart.Add("getAccountCodeWithChild");
                    keystart.Add("getAllAccountCodeWithChild");
                    keystart.Add("getAllAccountComboCodeWithChild");
                    keystart.Add("GetAccountListByAccountCode");
                    keystart.Add("GetSubLedgerByAccountCode");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpPost]
        public HttpResponseMessage createNewAccountHead(AccountSetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.createNewAccountSetup(model);
                if (result == "INSERTED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetChildOfAccountByGroup");
                    keystart.Add("getAllAccountMaps");
                    keystart.Add("getAccountCode");
                    keystart.Add("AllAccountSetupByCode");
                    keystart.Add("AllAccountSetupByName");
                    keystart.Add("AllFilterAccount");
                    keystart.Add("GetAllAccountCode");
                    keystart.Add("GetAllChargeAccountSetupByFilter");
                    keystart.Add("getAccountCodeWithChild");
                    keystart.Add("getAllAccountCodeWithChild");
                    keystart.Add("getAllAccountComboCodeWithChild");
                    keystart.Add("GetAccountListByAccountCode");
                    keystart.Add("GetSubLedgerByAccountCode");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpGet]
        public HttpResponseMessage getAccountDetailsByAccCode(string accCode)
        {
            try
            {
                var result = this._iDocumentSetup.GetAccountDataByAccCode(accCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });


            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpGet]
        public List<AccountSetupModel> GetChildOfAccountByGroup(string groupId)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<AccountSetupModel>();
            //if (this._cacheManager.IsSet($"GetChildOfAccountByGroup_{userid}_{company_code}_{branch_code}+{groupId}"))
            //{
            //    var data = _cacheManager.Get<List<AccountSetupModel>>($"GetChildOfAccountByGroup_{userid}_{company_code}_{branch_code}+{groupId}");
            //    response = data;
            //}
            //else
            //{
            //    var accountListByGroupCodeList = this._iDocumentSetup.GetAccountListByGroupCode(groupId);
            //    this._cacheManager.Set($"GetChildOfAccountByGroup_{userid}_{company_code}_{branch_code}_{groupId}", accountListByGroupCodeList, 20);
            //    response = accountListByGroupCodeList;
            //}
            //return response;
            var result = this._iDocumentSetup.GetAccountListByGroupCode(groupId);
            return result;
        }
        [HttpGet]
        public List<AccTypeModels> getAllAccountMaps(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<AccTypeModels>();
            if (this._cacheManager.IsSet($"getAllAccountMaps_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<AccTypeModels>>($"getAllAccountMaps_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var getAllAccountMapsList = this._iDocumentSetup.getAllAccountMaps(filter);
                this._cacheManager.Set($"getAllAccountMaps_{userid}_{company_code}_{branch_code}_{filter}", getAllAccountMapsList, 20);
                response = getAllAccountMapsList;
            }
            return response;
            //var result = this._iDocumentSetup.getAllAccountMaps(filter);
            //return result;
        }
        [HttpGet]
        public List<AccountSetupModel> GetAccountList(string searchtext)
        {
            var result = this._iDocumentSetup.GetAccountList(searchtext);
            return result;
        }
        [HttpGet]
        public string GetNewAccountCode()
        {
            return this._iDocumentSetup.GetNewAccountCode();
        }
        #endregion

        #region Customer

        [HttpGet]
        public List<CustomerSetupModel> GetChildOfCustomerByGroup(string groupId,string wholeSearchText="nothings")
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<CustomerSetupModel>();
            //if (this._cacheManager.IsSet($"GetChildOfCustomerByGroup_{userid}_{company_code}_{branch_code}_{groupId}"))
            //{
            //    var data = _cacheManager.Get<List<CustomerSetupModel>>($"GetChildOfCustomerByGroup_{userid}_{company_code}_{branch_code}_{groupId}");
            //    response = data;
            //}
            //else
            //{
            //    var accountListByCustomerCodeList = this._iDocumentSetup.GetAccountListByCustomerCode(groupId);
            //    this._cacheManager.Set($"GetChildOfCustomerByGroup_{userid}_{company_code}_{branch_code}_{groupId}", accountListByCustomerCodeList, 20);
            //    response = accountListByCustomerCodeList;
            //}
            //return response;
            var result = this._iDocumentSetup.GetAccountListByCustomerCode(groupId,wholeSearchText);
            return result;
        }
        [HttpGet]
        public List<CustomerSetupModel> GetAllAccountListByCustomerCode123(string searchText)
        {
            return this._iDocumentSetup.GetAllAccountListByCustomerCode123(searchText);
        }
        [HttpGet]
        public List<ItemSetupModel> getAllItemsForCustomerStock(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<ItemSetupModel>();
            if (this._cacheManager.IsSet($"getAllItemsForCustomerStock_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<ItemSetupModel>>($"getAllItemsForCustomerStock_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var getAllItemsForCustomerStockList = this._iDocumentSetup.getAllItemsForCustomerStock(filter);
                this._cacheManager.Set($"getAllItemsForCustomerStock_{userid}_{company_code}_{branch_code}_{filter}", getAllItemsForCustomerStockList, 20);
                response = getAllItemsForCustomerStockList;
            }
            return response;
            //var result = this._iDocumentSetup.getAllItemsForCustomerStock(filter);
            //return result;
        }
        [HttpGet]
        public List<EmployeeCodeModels> getAllComboEmployees(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<EmployeeCodeModels>();
            if (this._cacheManager.IsSet($"getAllComboEmployees_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<EmployeeCodeModels>>($"getAllComboEmployees_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var getAllComboEmployeesList = this._iDocumentSetup.getAllComboEmployees(filter);
                this._cacheManager.Set($"getAllComboEmployees_{userid}_{company_code}_{branch_code}_{filter}", getAllComboEmployeesList, 20);
                response = getAllComboEmployeesList;
            }
            return response;
            //var result = this._iDocumentSetup.getAllComboEmployees(filter);
            //return result;
        }
        [HttpGet]
        public List<DealerModels> getAllComboDealers(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<DealerModels>();
            if (this._cacheManager.IsSet($"getAllComboDealers_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<DealerModels>>($"getAllComboDealers_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var getAllComboDealersList = this._iDocumentSetup.getAllComboDealers(filter);
                this._cacheManager.Set($"getAllComboDealers_{userid}_{company_code}_{branch_code}_{filter}", getAllComboDealersList, 20);
                response = getAllComboDealersList;
            }
            return response;
            //var result = this._iDocumentSetup.getAllComboDealers(filter);
            //return result;
        }
        [HttpGet]
        public CustomerModels GetChildCustomerByCustomerCode(string customerCode)
        {
            var result = this._iDocumentSetup.GetChildCustomerByCustomerCode(customerCode);
            return result;
        }
        [HttpGet]
        public int? MaxCustomer()
        {
            var result = this._iDocumentSetup.GetMaxCustomer();
            return result;
        }
        [HttpGet]
        public int? MaxCustomerChild()
        {
            var result = this._iDocumentSetup.GetMaxChildCustomer();
            return result;
        }
        [HttpPost]
        public HttpResponseMessage createNewCustomerGroup(CustomerModels model)
        {
            try
            {
                var result = this._iDocumentSetup.createNewCustomerSetup(model);
                if (result == "INSERTED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetChildOfCustomerByGroup");
                    keystart.Add("getAllItemsForCustomerStock");
                    keystart.Add("getAllComboEmployees");
                    keystart.Add("getAllComboDealers");
                    keystart.Add("GetCustomers");
                    keystart.Add("AllCustomerSetupByCode");
                    keystart.Add("AllCustomerSetupByName");
                    keystart.Add("AllCustomerSetupByAddress");
                    keystart.Add("AllCustomerSetupByPhoneno");
                    keystart.Add("AllFilterCustomer");
                    keystart.Add("customerDropDownForGroupPopup");
                    keystart.Add("GetCustomerListByCustomerCode");
                    keystart.Add("GetAllCustomerSetupByFilter");


                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        [HttpPost]
        public HttpResponseMessage createKYCForm(KYCFORM model)
        {
            try
            {
                var result = this._iDocumentSetup.CreateKYCForm(model);

                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpPost]
        public HttpResponseMessage createChildCustomer(CustomerModels model)
        {
            try
            {
                var result = this._iDocumentSetup.createNewChildCustomerSetup(model);
                if (result == "INSERTED" || result == "UPDATED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetChildOfCustomerByGroup");
                    keystart.Add("getAllItemsForCustomerStock");
                    keystart.Add("getAllComboEmployees");
                    keystart.Add("getAllComboDealers");
                    keystart.Add("GetCustomers");
                    keystart.Add("AllCustomerSetupByCode");
                    keystart.Add("AllCustomerSetupByName");
                    keystart.Add("AllCustomerSetupByAddress");
                    keystart.Add("AllCustomerSetupByPhoneno");
                    keystart.Add("AllFilterCustomer");
                    keystart.Add("customerDropDownForGroupPopup");
                    keystart.Add("GetCustomerListByCustomerCode");
                    keystart.Add("GetAllCustomerSetupByFilter");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpPost]
        public HttpResponseMessage updateCustomerByCustomerCode(CustomerModels model)
        {
            try
            {
                var result = this._iDocumentSetup.updateCustomerSetup(model);
                if (result == "UPDATED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetChildOfCustomerByGroup");
                    keystart.Add("getAllItemsForCustomerStock");
                    keystart.Add("getAllComboEmployees");
                    keystart.Add("getAllComboDealers");
                    keystart.Add("GetCustomers");
                    keystart.Add("AllCustomerSetupByCode");
                    keystart.Add("AllCustomerSetupByName");
                    keystart.Add("AllCustomerSetupByAddress");
                    keystart.Add("AllCustomerSetupByPhoneno");
                    keystart.Add("AllFilterCustomer");
                    keystart.Add("customerDropDownForGroupPopup");
                    keystart.Add("GetCustomerListByCustomerCode");
                    keystart.Add("GetAllCustomerSetupByFilter");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpPost]
        public HttpResponseMessage DeleteCustomerTreeByCustomerCode(string customerCode)
        {
            try
            {
                var result = _iDocumentSetup.DeleteCustomerTreeByCustCode(customerCode);
                #region CLEAR CACHE
                List<string> keystart = new List<string>();
                keystart.Add("GetChildOfCustomerByGroup");
                keystart.Add("getAllItemsForCustomerStock");
                keystart.Add("getAllComboEmployees");
                keystart.Add("getAllComboDealers");
                keystart.Add("GetCustomers");
                keystart.Add("AllCustomerSetupByCode");
                keystart.Add("AllCustomerSetupByName");
                keystart.Add("AllCustomerSetupByAddress");
                keystart.Add("AllCustomerSetupByPhoneno");
                keystart.Add("AllFilterCustomer");
                keystart.Add("customerDropDownForGroupPopup");
                keystart.Add("GetCustomerListByCustomerCode");
                keystart.Add("GetAllCustomerSetupByFilter");
                List<string> Record = new List<string>();
                Record = this._cacheManager.GetAllKeys();
                this._cacheManager.RemoveCacheByKey(keystart, Record);
                #endregion
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpPost]
        public HttpResponseMessage DeleteCustomerByCustomerCode(string customerCode)
        {
            try
            {
                var result = _iDocumentSetup.DeleteCustomerByCustomerCode(customerCode);
                #region CLEAR CACHE
                List<string> keystart = new List<string>();
                keystart.Add("GetChildOfCustomerByGroup");
                keystart.Add("getAllItemsForCustomerStock");
                keystart.Add("getAllComboEmployees");
                keystart.Add("getAllComboDealers");
                keystart.Add("GetCustomers");
                keystart.Add("AllCustomerSetupByCode");
                keystart.Add("AllCustomerSetupByName");
                keystart.Add("AllCustomerSetupByAddress");
                keystart.Add("AllCustomerSetupByPhoneno");
                keystart.Add("AllFilterCustomer");
                keystart.Add("customerDropDownForGroupPopup");
                keystart.Add("GetCustomerListByCustomerCode");
                keystart.Add("GetAllCustomerSetupByFilter");
                List<string> Record = new List<string>();
                Record = this._cacheManager.GetAllKeys();
                this._cacheManager.RemoveCacheByKey(keystart, Record);
                #endregion
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        #endregion

        #region Location
        [HttpPost]
        public HttpResponseMessage DeleteLocationSetupByLocationCode(string locationCode)
        {
            try
            {
                var result = _iDocumentSetup.DeleteLocationSetupByLocationCode(locationCode);
                #region CLEAR CACHE
                List<string> keystart = new List<string>();
                keystart.Add("getAllLocation");
                keystart.Add("GetChildOfLocationByGroup");
                keystart.Add("GetLocation");
                keystart.Add("GetAllLocationListByFilter");
                keystart.Add("GetAllBudgetCenterForLocationByFilter");
                keystart.Add("checkBudgetFlagByLocationCode");
                keystart.Add("GetLocationByGroup");
                keystart.Add("getLocationType");
                keystart.Add("GetLocationListByLocationCode");
                List<string> Record = new List<string>();
                Record = this._cacheManager.GetAllKeys();
                this._cacheManager.RemoveCacheByKey(keystart, Record);
                #endregion
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpPost]
        public HttpResponseMessage updateLocationByLocationCode(LocationSetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.udpateLocationSetup(model);
                if (result == "UPDATED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("getAllLocation");
                    keystart.Add("GetChildOfLocationByGroup");
                    keystart.Add("GetLocation");
                    keystart.Add("GetAllLocationListByFilter");
                    keystart.Add("GetAllBudgetCenterForLocationByFilter");
                    keystart.Add("checkBudgetFlagByLocationCode");
                    keystart.Add("GetLocationByGroup");
                    keystart.Add("getLocationType");
                    keystart.Add("GetLocationListByLocationCode");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpPost]
        public HttpResponseMessage createNewLocationHead(LocationSetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.createNewLocationSetup(model);
                if (result == "INSERTED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("getAllLocation");
                    keystart.Add("GetChildOfLocationByGroup");
                    keystart.Add("GetLocation");
                    keystart.Add("GetAllLocationListByFilter");
                    keystart.Add("GetAllBudgetCenterForLocationByFilter");
                    keystart.Add("checkBudgetFlagByLocationCode");
                    keystart.Add("GetLocationByGroup");
                    keystart.Add("getLocationType");
                    keystart.Add("GetLocationListByLocationCode");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpGet]
        public HttpResponseMessage getLocationDetailsByLocationCode(string locationCode)
        {
            try
            {
                var result = this._iDocumentSetup.GetLocationDataByLocationCode(locationCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpGet]
        public List<LocationSetupModel> GetChildOfLocationByGroup(string groupId)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<LocationSetupModel>();
            if (this._cacheManager.IsSet($"GetChildOfLocationByGroup_{userid}_{company_code}_{branch_code}_{groupId}"))
            {
                var data = _cacheManager.Get<List<LocationSetupModel>>($"GetChildOfLocationByGroup_{userid}_{company_code}_{branch_code}_{groupId}");
                response = data;
            }
            else
            {
                var locationListByGroupCodeList = this._iDocumentSetup.GetLocationListByGroupCode(groupId);
                this._cacheManager.Set($"GetChildOfLocationByGroup_{userid}_{company_code}_{branch_code}_{groupId}", locationListByGroupCodeList, 20);
                response = locationListByGroupCodeList;
            }
            return response;
            //var result = this._iDocumentSetup.GetLocationListByGroupCode(groupId);
            //return result;
        }
        [HttpGet]
        public List<LocationSetupModel> GetAllLocationList(string searchText)
        {
            return this._iDocumentSetup.GetAllLocationList(searchText);
        }
        [HttpGet]
        public List<LocationModels> getAllLocation(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<LocationModels>();
            if (this._cacheManager.IsSet($"getAllLocation_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<LocationModels>>($"getAllLocation_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var getAllLocationList = this._iDocumentSetup.getAllLocation(filter);
                this._cacheManager.Set($"getAllLocation_{userid}_{company_code}_{branch_code}_{filter}", getAllLocationList, 20);
                response = getAllLocationList;
            }
            return response;
            //var result = this._iDocumentSetup.getAllLocation(filter);
            //return result;
        }
        #endregion

        #region Regional
        [HttpPost]
        public HttpResponseMessage DeleteRegionalSetupByRegionalCode(string regionalCode)
        {
            try
            {
                var result = _iDocumentSetup.DeleteRegionalSetupByRegionalCode(regionalCode);
                #region CLEAR CACHE
                List<string> keystart = new List<string>();
                keystart.Add("GetChildOfRegionalByGroup");
                keystart.Add("Getregional");
                keystart.Add("GetTreeRegional");
                List<string> Record = new List<string>();
                Record = this._cacheManager.GetAllKeys();
                this._cacheManager.RemoveCacheByKey(keystart, Record);
                #endregion
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpPost]
        public HttpResponseMessage updateRegionalByRegionalCode(RegionalSetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.udpateRegionalSetup(model);
                if (result == "UPDATED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetChildOfRegionalByGroup");
                    keystart.Add("Getregional");
                    keystart.Add("GetTreeRegional");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpPost]
        public HttpResponseMessage createNewRegionalHead(RegionalSetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.createNewRegionalSetup(model);
                if (result == "INSERTED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetChildOfRegionalByGroup");
                    keystart.Add("Getregional");
                    keystart.Add("GetTreeRegional");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpGet]
        public HttpResponseMessage getRegionalDetailsByRegionalCode(string regionCode)
        {
            try
            {
                var result = this._iDocumentSetup.GetRegionalDataByRegionalCode(regionCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpGet]
        public List<RegionalSetupModel> GetChildOfRegionalByGroup(string groupId)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<RegionalSetupModel>();
            if (this._cacheManager.IsSet($"GetChildOfRegionalByGroup_{userid}_{company_code}_{branch_code}_{groupId}"))
            {
                var data = _cacheManager.Get<List<RegionalSetupModel>>($"GetChildOfRegionalByGroup_{userid}_{company_code}_{branch_code}_{groupId}");
                response = data;
            }
            else
            {
                var getRegionalListByGroupCodeList = this._iDocumentSetup.GetRegionalListByGroupCode(groupId);
                this._cacheManager.Set($"GetChildOfRegionalByGroup_{userid}_{company_code}_{branch_code}_{groupId}", getRegionalListByGroupCodeList, 20);
                response = getRegionalListByGroupCodeList;
            }
            return response;
            //var result = this._iDocumentSetup.GetRegionalListByGroupCode(groupId);
            //return result;
        }
        [HttpGet]
        public List<RegionalSetupModel> GetAllRegionalList(string searchText)
        {
            return this._iDocumentSetup.GetAllRegionalList(searchText);
        }
        #endregion

        #region Resource
        [HttpPost]
        public HttpResponseMessage DeleteResourceSetupByResourceCode(string resourceCode)
        {
            try
            {
                var result = _iDocumentSetup.DeleteResourceSetupByResourceCode(resourceCode);
                #region CLEAR CACHE
                List<string> keystart = new List<string>();
                keystart.Add("GetChildOfResourceByGroup");
                keystart.Add("GetResource");
                keystart.Add("getResourceCodeWithChild");
                List<string> Record = new List<string>();
                Record = this._cacheManager.GetAllKeys();
                this._cacheManager.RemoveCacheByKey(keystart, Record);
                #endregion
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpPost]
        public HttpResponseMessage updateResourceByResourceCode(ResourceSetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.udpateResourceSetup(model);
                if (result == "UPDATED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetChildOfResourceByGroup");
                    keystart.Add("GetResource");
                    keystart.Add("getResourceCodeWithChild");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpPost]
        public HttpResponseMessage createNewResourceHead(ResourceSetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.createNewResourceSetup(model);
                if (result == "INSERTED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetChildOfResourceByGroup");
                    keystart.Add("GetResource");
                    keystart.Add("getResourceCodeWithChild");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpGet]
        public HttpResponseMessage getResourceDetailsByResourceCode(string resourceCode)
        {
            try
            {
                var result = this._iDocumentSetup.GetResourceDataByResourceCode(resourceCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpGet]
        public List<ResourceSetupModel> GetChildOfResourceByGroup(string groupId)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<ResourceSetupModel>();
            if (this._cacheManager.IsSet($"GetChildOfResourceByGroup_{userid}_{company_code}_{branch_code}_{groupId}"))
            {
                var data = _cacheManager.Get<List<ResourceSetupModel>>($"GetChildOfResourceByGroup_{userid}_{company_code}_{branch_code}_{groupId}");
                response = data;
            }
            else
            {
                var getResourceListByGroupCodeList = this._iDocumentSetup.GetResourceListByGroupCode(groupId);
                this._cacheManager.Set($"GetChildOfResourceByGroup_{userid}_{company_code}_{branch_code}_{groupId}", getResourceListByGroupCodeList, 20);
                response = getResourceListByGroupCodeList;
            }
            return response;
            //var result = this._iDocumentSetup.GetResourceListByGroupCode(groupId);
            //return result;
        }
        public List<ResourceSetupModel> GetAllResourceList(string searchText)
        {
            return this._iDocumentSetup.GetAllResourceList(searchText);
        }
        #endregion

        #region Process
        [HttpPost]
        public HttpResponseMessage DeleteProcessSetupByProcessCode(string processCode)
        {
            try
            {
                var result = _iDocumentSetup.DeleteProcessSetupByProcessCode(processCode);
                //#region CLEAR CACHE
                //List<string> keystart = new List<string>();
                //keystart.Add("GetChildOfProcessByGroup");
                //keystart.Add("getProcessCode");
                //keystart.Add("getProcessCodeWithChild");
                //List<string> Record = new List<string>();
                //Record = this._cacheManager.GetAllKeys();
                //this._cacheManager.RemoveCacheByKey(keystart, Record);
                //#endregion
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpPost]
        public HttpResponseMessage updateProcessByProcessCode(ProcessSetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.udpateProcessSetup(model);
                if (result == "UPDATED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetChildOfProcessByGroup");
                    keystart.Add("getProcessCode");
                    keystart.Add("getProcessCodeWithChild");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpPost]
        public HttpResponseMessage createNewProcessHead(ProcessSetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.createNewProcessSetup(model);
                if (result == "INSERTED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetChildOfProcessByGroup");
                    keystart.Add("getProcessCode");
                    keystart.Add("getProcessCodeWithChild");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpGet]
        public HttpResponseMessage getProcessDetailsByProcessCode(string processCode)
        {
            try
            {
                var result = this._iDocumentSetup.GetProcessDataByProcessCode(processCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        //get Routine Input data
        [HttpGet]
        public HttpResponseMessage GetProcessInputdata()
        {
            try
            {
                var result = this._iDocumentSetup.GetProcessInputGriddata();
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpGet]
        public List<ProcessSetupModel> GetChildOfProcessByGroup(string groupId)
        {
            //var userid = _workContext.CurrentUserinformation.User_id;
            //var company_code = _workContext.CurrentUserinformation.company_code;
            //var branch_code = _workContext.CurrentUserinformation.branch_code;
            //var response = new List<ProcessSetupModel>();
            //if (this._cacheManager.IsSet($"GetChildOfProcessByGroup_{userid}_{company_code}_{branch_code}_{groupId}"))
            //{
            //    var data = _cacheManager.Get<List<ProcessSetupModel>>($"GetChildOfProcessByGroup_{userid}_{company_code}_{branch_code}_{groupId}");
            //    response = data;
            //}
            //else
            //{
            //    var processListByGroupCodeList = this._iDocumentSetup.GetProcessListByGroupCode(groupId);
            //    this._cacheManager.Set($"GetChildOfProcessByGroup_{userid}_{company_code}_{branch_code}_{groupId}", processListByGroupCodeList, 20);
            //    response = processListByGroupCodeList;
            //}
            //return response;
            var result = this._iDocumentSetup.GetProcessListByGroupCode(groupId);
            return result;
        }
        //[HttpGet]
        //public List<ProcessSetupModel> GetChildOfProcessByGroup(string groupId)
        //{
        //    var result = this._iDocumentSetup.GetProcessListByGroupCode(groupId);
        //    return result;
        //}
        [HttpGet]
        public List<ProcessSetupModel> GetAllProcessList(string searchText)
        {
            return this._iDocumentSetup.GetAllProcessList(searchText);
        }
        #endregion

        #region Item
        [HttpGet]
        public HttpResponseMessage getMaxItemCode(string gFlag)
        {
            try
            {
                var result = this._iDocumentSetup.GetMaxItemCode(gFlag);
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpGet]
        public HttpResponseMessage getitemDetailsByItemCode(string accCode)
        {
            try
            {
                var result = this._iDocumentSetup.GetItemDataByItemCode(accCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        

        [HttpGet]
        public List<ItemSetupModel> GetChildOfItemByGroup(string groupId)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<ItemSetupModel>();
            //if (this._cacheManager.IsSet($"GetChildOfItemByGroup_{userid}_{company_code}_{branch_code}_{groupId}"))
            //{
            //    var data = _cacheManager.Get<List<ItemSetupModel>>($"GetChildOfItemByGroup_{userid}_{company_code}_{branch_code}_{groupId}");
            //    response = data;
            //}
            //else
            //{
                var getChildOfItemByGroupList = this._iDocumentSetup.GetItemListByGroupCode(groupId);
                this._cacheManager.Set($"GetChildOfItemByGroup_{userid}_{company_code}_{branch_code}_{groupId}", getChildOfItemByGroupList, 20);
                response = getChildOfItemByGroupList;
         //   }
            return response;
            //var result = this._iDocumentSetup.GetItemListByGroupCode(groupId);
            //return result;
        }
        [HttpGet]
        public List<ItemSetupModel> GetAllItemList(string searchText)
        {
           return this._iDocumentSetup.GetAllItemList(searchText);
        }
        [HttpPost]
        public HttpResponseMessage createNewitem(ItemSetupModalSet model)
        {

            try
            {

                var result = this._iDocumentSetup.createNewItemSetup(model.model);
                if (result == "INSERTED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetChildOfItemByGroup");
                    keystart.Add("GetProducts");
                    keystart.Add("GetItemChargeDataSavedValueWise");
                    keystart.Add("GetItemChargeDataSavedQuantityWise");
                    keystart.Add("GetInvItemChargesData");
                    keystart.Add("GetMUCodeByProductId");
                    keystart.Add("GetMuCode");
                    keystart.Add("GetProductListByItemCode");
                    keystart.Add("GetAllProductsListByFilter");
                    keystart.Add("getProductCodeWithChild");
                    keystart.Add("GetGroupProducts");
                    keystart.Add("GetItemDataByItemCode");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpPost]
        public HttpResponseMessage updateitemByItemCode(ItemSetupModalSet model)

        {

            try
            {
                var result = this._iDocumentSetup.udpateItemSetup(model.model);
                if (result == "UPDATED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetChildOfItemByGroup");
                    keystart.Add("GetProducts");
                    keystart.Add("GetItemChargeDataSavedValueWise");
                    keystart.Add("GetItemChargeDataSavedQuantityWise");
                    keystart.Add("GetInvItemChargesData");
                    keystart.Add("GetMUCodeByProductId");
                    keystart.Add("GetMuCode");
                    keystart.Add("GetProductListByItemCode");
                    keystart.Add("GetAllProductsListByFilter");
                    keystart.Add("getProductCodeWithChild");
                    keystart.Add("GetGroupProducts");
                    keystart.Add("GetItemDataByItemCode");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        //[HttpGet]
        //public HttpResponseMessage getitemCodeByitemcode(string itemcode)
        //{
        //    try
        //    {
        //        string result = this._iDocumentSetup.GetItemCodeByItemCode(itemcode);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
        //    }


        //}
        [HttpPost]
        public HttpResponseMessage DeleteitemsetupByItemcode(string itemcode)
        {
            try
            {
                var result = _iDocumentSetup.DeleteItemSetupByItemCode(itemcode);
                #region CLEAR CACHE
                List<string> keystart = new List<string>();
                keystart.Add("GetChildOfItemByGroup");
                keystart.Add("GetProducts");
                keystart.Add("GetItemChargeDataSavedValueWise");
                keystart.Add("GetItemChargeDataSavedQuantityWise");
                keystart.Add("GetInvItemChargesData");
                keystart.Add("GetMUCodeByProductId");
                keystart.Add("GetMuCode");
                keystart.Add("GetProductListByItemCode");
                keystart.Add("GetAllProductsListByFilter");
                keystart.Add("getProductCodeWithChild");
                keystart.Add("GetGroupProducts");
                List<string> Record = new List<string>();
                Record = this._cacheManager.GetAllKeys();
                this._cacheManager.RemoveCacheByKey(keystart, Record);
                #endregion
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        #endregion

        #region Area
        [HttpPost]
        public HttpResponseMessage deleteAreaSetup(string areaCode)
        {
            try
            {
                var result = _iDocumentSetup.deleteAreaSetup(areaCode);
                #region CLEAR CACHE
                List<string> keystart = new List<string>();
                keystart.Add("GetAllAreaCode");
                keystart.Add("GetAllAreaSetupByFilter");
                keystart.Add("getAreaCodeWithChild");
                List<string> Record = new List<string>();
                Record = this._cacheManager.GetAllKeys();
                this._cacheManager.RemoveCacheByKey(keystart, Record);
                #endregion
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpPost]
        public HttpResponseMessage createNewAreaSetup(AreaModels model)
        {
            try
            {
                var result = this._iDocumentSetup.createNewAreaSetup(model);
                if (result == "INSERTED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetAllAreaCode");
                    keystart.Add("GetAllAreaSetupByFilter");
                    keystart.Add("getAreaCodeWithChild");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpPost]
        public HttpResponseMessage updateAreaSetup(AreaModels model)
        {
            try
            {
                var result = this._iDocumentSetup.updateAreaSetup(model);
                if (result == "UPDATED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetAllAreaCode");
                    keystart.Add("GetAllAreaSetupByFilter");
                    keystart.Add("getAreaCodeWithChild");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpGet]
        public HttpResponseMessage getAreaDetailsByAreaCode(string areaCode)
        {
            try
            {
                var result = this._iDocumentSetup.GetAccountDataByAccCode(areaCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpGet]
        public HttpResponseMessage getMaxAreaCode()
        {
            try
            {
                var result = this._iDocumentSetup.getMaxAreaCode();
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpGet]
        public List<AreaModels> GetAllAreaCode()
        {

            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<AreaModels>();
            if (this._cacheManager.IsSet($"GetAllAreaCode_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<AreaModels>>($"GetAllAreaCode_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var getAllAreaCodeList = this._iDocumentSetup.getAllAreaCodeDetail();
                this._cacheManager.Set($"GetAllAreaCode_{userid}_{company_code}_{branch_code}", getAllAreaCodeList, 20);
                response = getAllAreaCodeList;
            }
            return response;
            //var result = this._iDocumentSetup.getAllAreaCodeDetail();
            //return result;
        }
        #endregion

        #region Agent
        [HttpPost]
        public HttpResponseMessage deleteAgentSetup(string agentCode)
        {
            try
            {
                var result = _iDocumentSetup.deleteAgentSetup(agentCode);
                #region CLEAR CACHE
                List<string> keystart = new List<string>();
                keystart.Add("getAllAgents");
                keystart.Add("getAgentCodeWithChild");
                List<string> Record = new List<string>();
                Record = this._cacheManager.GetAllKeys();
                this._cacheManager.RemoveCacheByKey(keystart, Record);
                #endregion
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpPost]
        public HttpResponseMessage createNewAgentSetup(AgentModels model)
        {
            try
            {
                var result = this._iDocumentSetup.createNewAgentSetup(model);
                if (result == "INSERTED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("getAllAgents");
                    keystart.Add("getAgentCodeWithChild");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpPost]
        public HttpResponseMessage updateAgentSetup(AgentModels model)
        {
            try
            {
                var result = this._iDocumentSetup.updateAgentSetup(model);
                if (result == "UPDATED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("getAllAgents");
                    keystart.Add("getAgentCodeWithChild");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpGet]
        public HttpResponseMessage getMaxAgentCode()
        {
            try
            {
                var result = this._iDocumentSetup.getMaxAgentCode();
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpGet]
        public List<AgentModels> GetAllAgentCode()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<AgentModels>();
            if (this._cacheManager.IsSet($"GetAllAgentCode_{userid}_{ company_code}_{ branch_code}"))
            {
                var data = _cacheManager.Get<List<AgentModels>>($"GetAllAgentCode_{userid}_{ company_code}_{ branch_code}");
                response = data;
            }
            else
            {
                var getAllAgentCodeList = this._iDocumentSetup.getAllAgentCodeDetail();
                this._cacheManager.Set($"GetAllAgentCode_{userid}_{ company_code}_{ branch_code}", getAllAgentCodeList, 20);
                response = getAllAgentCodeList;
            }
            return response;
            //var result = this._iDocumentSetup.getAllAgentCodeDetail();
            //return result;
        }
        [HttpGet]
        public List<AgentModels> getAllAgents(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<AgentModels>();
            if (this._cacheManager.IsSet($"getAllAgents_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<AgentModels>>($"getAllAgents_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var getAllAgentsList = this._iDocumentSetup.getAllAgents(filter);
                this._cacheManager.Set($"getAllAgents_{userid}_{company_code}_{branch_code}_{filter}", getAllAgentsList, 20);
                response = getAllAgentsList;
            }
            return response;
            //var result = this._iDocumentSetup.getAllAgents(filter);
            //return result;
        }
        #endregion

        #region Transporter
        [HttpPost]
        public HttpResponseMessage deleteTransporterSetup(string transporterCode)
        {
            try
            {
                var result = _iDocumentSetup.deleteTransporterSetup(transporterCode);
                #region CLEAR CACHE
                List<string> keystart = new List<string>();
                keystart.Add("GetAllTransporterCode");
                keystart.Add("getTransporterCodeWithChild");
                List<string> Record = new List<string>();
                Record = this._cacheManager.GetAllKeys();
                this._cacheManager.RemoveCacheByKey(keystart, Record);
                #endregion
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpPost]
        public HttpResponseMessage createNewTransporterSetup(TransporterModels model)
        {
            try
            {
                var result = this._iDocumentSetup.createNewTransporterSetup(model);
                if (result == "INSERTED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetAllTransporterCode");
                    keystart.Add("getTransporterCodeWithChild");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpPost]
        public HttpResponseMessage updateTransporterSetup(TransporterModels model)
        {
            try
            {
                var result = this._iDocumentSetup.updateTransporterSetup(model);
                if (result == "UPDATED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetAllTransporterCode");
                    keystart.Add("getTransporterCodeWithChild");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpGet]
        public HttpResponseMessage getMaxTransporterCode()
        {
            try
            {
                var result = this._iDocumentSetup.getMaxTransporterCode();
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpGet]
        public List<TransporterModels> GetAllTransporterCode()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<TransporterModels>();
            if (this._cacheManager.IsSet($"GetAllTransporterCode_{ userid}_{ company_code}_{ branch_code}"))
            {
                var data = _cacheManager.Get<List<TransporterModels>>($"GetAllTransporterCode_{ userid}_{ company_code}_{ branch_code}");
                response = data;
            }
            else
            {
                var getAllTransporterCodeList = this._iDocumentSetup.getAllTransporterCodeDetail();
                this._cacheManager.Set($"GetAllTransporterCode_{ userid}_{ company_code}_{ branch_code}", getAllTransporterCodeList, 20);
                response = getAllTransporterCodeList;
            }
            return response;
            //var result = this._iDocumentSetup.getAllTransporterCodeDetail();
            //return result;
        }
        #endregion

        #region Supplier 
        [HttpGet]
        public HttpResponseMessage getsupplierDetailsBysupplierCode(string suppliercode)
        {
            try
            {
                //var result = this._iDocumentSetup.GetSupplierDataBysupplierCode(suppliercode);
                var result = this._iDocumentSetup.GetSupplierDataBySupplierCode(suppliercode);
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpGet]
        public List<SuplierSetupModel> GetChildOfsupplierByGroup(string groupId)
        {
            //var userid = _workContext.CurrentUserinformation.User_id;
            //var company_code = _workContext.CurrentUserinformation.company_code;
            //var branch_code = _workContext.CurrentUserinformation.branch_code;
            //var response = new List<SuplierSetupModel>();
            //if (this._cacheManager.IsSet($"GetChildOfsupplierByGroup_{userid}_{company_code}_{branch_code}_{groupId}"))
            //{
            //    var data = _cacheManager.Get<List<SuplierSetupModel>>($"GetChildOfsupplierByGroup_{userid}_{company_code}_{branch_code}_{groupId}");
            //    response = data;
            //}
            //else
            //{
            //    var getSupplyListByGroupCodeList = this._iDocumentSetup.GetSupplyListByGroupCode(groupId);
            //    this._cacheManager.Set($"GetChildOfsupplierByGroup_{userid}_{company_code}_{branch_code}_{groupId}", getSupplyListByGroupCodeList, 20);
            //    response = getSupplyListByGroupCodeList;
            //}
            //return response;
            var result = this._iDocumentSetup.GetSupplyListByGroupCode(groupId);
            return result;
        }
        [HttpGet]
        public List<SuplierSetupModel> GetAllSupplyList(string searchText)
        {
            return this._iDocumentSetup.GetAllSupplyList(searchText);
        }
        [HttpPost]
        public HttpResponseMessage createNewsupplier(SuplierSetupModalSet suplierSetupModalSet)
        {
            try
            {
                var result = this._iDocumentSetup.createNewSupplierSetup(suplierSetupModalSet);
                if (result == "INSERTED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetChildOfsupplierByGroup");
                    keystart.Add("GetSuppliers");
                    keystart.Add("AllSupplierForReferenceByCode");
                    keystart.Add("AllSupplierForReferenceByName");
                    keystart.Add("AllSupplierForReferenceByaddress");
                    keystart.Add("AllSupplierForReferenceByphoneno");
                    keystart.Add("AllFilterSupplierForReference");
                    keystart.Add("AllSupplierSetupByCode");
                    keystart.Add("AllSupplierSetupByName");
                    keystart.Add("AllSupplierSetupByAddress");
                    keystart.Add("AllSupplierSetupByPhoneno");
                    keystart.Add("AllFilterSupplier");
                    keystart.Add("getsupplierCodeWithChild");
                    keystart.Add("GetSupplierListBySupplierCode");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpPost]
        public HttpResponseMessage updatesupplierbysupplierCode(SuplierSetupModalSet model)
        {
            try
            {
                var result = this._iDocumentSetup.udpateSupplierSetup(model);
                if (result == "UPDATED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetChildOfsupplierByGroup");
                    keystart.Add("GetSuppliers");
                    keystart.Add("AllSupplierForReferenceByCode");
                    keystart.Add("AllSupplierForReferenceByName");
                    keystart.Add("AllSupplierForReferenceByaddress");
                    keystart.Add("AllSupplierForReferenceByphoneno");
                    keystart.Add("AllFilterSupplierForReference");
                    keystart.Add("AllSupplierSetupByCode");
                    keystart.Add("AllSupplierSetupByName");
                    keystart.Add("AllSupplierSetupByAddress");
                    keystart.Add("AllSupplierSetupByPhoneno");
                    keystart.Add("AllFilterSupplier");
                    keystart.Add("getsupplierCodeWithChild");
                    keystart.Add("GetSupplierListBySupplierCode");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpPost]
        public HttpResponseMessage DeletesuppliersetupBysuppliercode(string suppliercode)
        {
            try
            {
                var result = _iDocumentSetup.DeleteSupplierSetupBySupplierCode(suppliercode);
                #region CLEAR CACHE
                List<string> keystart = new List<string>();
                keystart.Add("GetChildOfsupplierByGroup");
                keystart.Add("GetSuppliers");
                keystart.Add("AllSupplierForReferenceByCode");
                keystart.Add("AllSupplierForReferenceByName");
                keystart.Add("AllSupplierForReferenceByaddress");
                keystart.Add("AllSupplierForReferenceByphoneno");
                keystart.Add("AllFilterSupplierForReference");
                keystart.Add("AllSupplierSetupByCode");
                keystart.Add("AllSupplierSetupByName");
                keystart.Add("AllSupplierSetupByAddress");
                keystart.Add("AllSupplierSetupByPhoneno");
                keystart.Add("AllFilterSupplier");
                keystart.Add("getsupplierCodeWithChild");
                keystart.Add("GetSupplierListBySupplierCode");
                List<string> Record = new List<string>();
                Record = this._cacheManager.GetAllKeys();
                this._cacheManager.RemoveCacheByKey(keystart, Record);
                #endregion
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpGet]
        public HttpResponseMessage getNewSupplierId()
        {
            try
            {
                var result = this._iDocumentSetup.getNewSupplierCode();
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "OK", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        #endregion

        #region Division Setup
        // Delete Division Setup
        [HttpPost]
        public HttpResponseMessage DeleteDivisionSetupByDivisionCode(string divisionCode)
        {
            try
            {
                var result = _iDocumentSetup.DeleteDivisionCenterByDivisionCode(divisionCode);


                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        //create Division
        [HttpPost]
        public HttpResponseMessage createNewDivisionHead(DivisionSetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.createNewDivisionSetup(model);
                if (result == "INSERTED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        //Update division

        [HttpPost]
        public HttpResponseMessage updateDivisionCode(DivisionSetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.udpateDivisionSetup(model);
                if (result == "UPDATED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }


        //Division
        [HttpGet]
        public HttpResponseMessage getDivisionDetailsByDivisionCode(string divisionCode)
        {
            try
            {
                var result = this._iDocumentSetup.GetDivisionCenterDetailByDivisionCode(divisionCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }


        //child grid
        [HttpGet]
        public List<DivisionSetupModel> GetChildOfDivisionByGroup(string groupId)
        {
            var result = this._iDocumentSetup.getAllDivisionschild(groupId);
            return result;
        }
        [HttpGet]
        public List<DivisionSetupModel> getAllDivisionsList(string searchText)
        {
            return this._iDocumentSetup.getAllDivisionsList(searchText);
        }
        #endregion

        #region Branch Setup
        //Delete Branch
        [HttpPost]
        public HttpResponseMessage DeleteBranchCenterSetupByBranchCode(string branchCode)
        {
            try
            {
                var result = _iDocumentSetup.DeleteBranchCenterByBranchCode(branchCode);


                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }

        //Create Branch Setup
        [HttpPost]
        public HttpResponseMessage createNewBranchHead(BranchSetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.createNewBranchSetup(model);
                if (result == "INSERTED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        //Update Branch setup
        [HttpPost]
        public HttpResponseMessage updateBranchByBranchCode(BranchSetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.udpateBranchSetup(model);
                if (result == "UPDATED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }


        [HttpGet]
        public HttpResponseMessage getBranchCenterDetailBybranchCode(string branchCode)
        {
            try
            {
                var result = this._iDocumentSetup.GetBranchCenterDetailBybranchCode(branchCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }


        //BranchChild Grid data
        [HttpGet]
        public List<BranchSetupModel> GetChildOfBranchCenterByGroup(string groupId)
        {
            var result = this._iDocumentSetup.GetBranchCenterListByGroupCode(groupId);
            return result;
        }
        [HttpGet]
        public List<BranchSetupModel> GetAllBranchCenterList(string searchText)
        {
            return this._iDocumentSetup.GetAllBranchCenterList(searchText);
        }

        #endregion

        #region Dealer Setup
        //customer
        [HttpGet]
        public List<CustomerSetupModel> GetChildOfCustomerByGroup1(string dealercode="")
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<CustomerSetupModel>();
            if (string.IsNullOrEmpty(dealercode))
            {
                var results = this._iDocumentSetup.GetAccountListByCustomerCode123();
                return results;
            }
            var result = this._iDocumentSetup.GetAccountListByCustomerCodeByDealerCode(dealercode);
            return result;
        }

        [HttpPost]
        public HttpResponseMessage updateDealerCode(DealerModel model)
        {
            try
            {
                var result = this._iDocumentSetup.udpateDealerSetup(model);
                if (result == "UPDATED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpPost]
        public HttpResponseMessage createCustomerMapped(MappedCustomerModel model)
        {
            try
            {
                var result = this._iDocumentSetup.CreateNewCustomer1234(model);
                if (result == "INSERTED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }


        [HttpPost]
        public HttpResponseMessage createNewDealer(DealerModel model)
        {
            try
            {
                var result = this._iDocumentSetup.createNewDealerSetup(model);
                if (result == "INSERTED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        [HttpPost]
        public HttpResponseMessage DeleteDealer(string dealerCode)
        {
            try
            {
                var result = _iDocumentSetup.DeleteDealerCenterByDealerCode(dealerCode);


                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }


        [HttpGet]
        public HttpResponseMessage getDealerData(string dealerCode)
        {
            try
            {
                var result = this._iDocumentSetup.GetDealerDetailBydealerCode(dealerCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }


        [HttpGet]
        public List<DealerModel> GetChildOfDealerByGroup(string groupId)
        {

            var result = this._iDocumentSetup.GetDealerListByGroupCode(groupId);

            return result;


        }
        //Grid data from mapped dealer

        [HttpGet]
        public List<CustSubList> MappedDealerData(string dealerCode)
        {

            var result = this._iDocumentSetup.GetDealerMapped(dealerCode);

            return result;


        }
        #endregion

        #region Party type setup
        public List<AccountCodeModels> getAccountCodPrtyType()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<AccountCodeModels>();
            if (this._cacheManager.IsSet($"getAccountCodeWithChild_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<AccountCodeModels>>($"getAccountCodeWithChild_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var AllAccountCode = _iDocumentSetup.getAllAccountCodeParty();
                this._cacheManager.Set($"getAccountCodeWithChild_{userid}_{company_code}_{branch_code}", AllAccountCode, 20);
                response = AllAccountCode;
            }
            return response;
        }

        [HttpGet]
        public List<PartyTypeModel> partyTypeList()
        {

            var result = this._iDocumentSetup.partyTypeList();

            return result;


        }

        [HttpPost]
        public HttpResponseMessage createNewPaetyType(PartyTypeModel model)
        {
            try
            {
                var result = this._iDocumentSetup.createNewPaetyType(model);
                if (result == "INSERTED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }


        [HttpPost]
        public HttpResponseMessage updatePartyType(PartyTypeModel model)
        {
            try
            {
                var result = this._iDocumentSetup.updatePartyType(model);
                if (result == "UPDATED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }

        [HttpPost]
        public HttpResponseMessage deletePartySetup(string partyCode)
        {
            try
            {
                var result = _iDocumentSetup.deletePartySetup(partyCode);

                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        #endregion

        #region Budget Center
        [HttpPost]
        public HttpResponseMessage DeleteBudgetCenterSetupByBudgetCode(string budgetCode)
        {
            try
            {
                var result = _iDocumentSetup.DeleteBudgetCenterByBudgetCode(budgetCode);
                #region CLEAR CACHE
                List<string> keystart = new List<string>();
                keystart.Add("GetChildOfBudgetCenterByGroup");
                keystart.Add("GetAllBudgetCenterByFilter");
                keystart.Add("GetAllBudgetCenterForLocationByFilter");
                keystart.Add("getBudgetCodeByAccCode");
                keystart.Add("checkBudgetFlagByLocationCode");
                keystart.Add("getBudgetCenterCodeWithChild");
                keystart.Add("getbudgetCenterCode");
                keystart.Add("getAllBudgetCenter");
                List<string> Record = new List<string>();
                Record = this._cacheManager.GetAllKeys();
                this._cacheManager.RemoveCacheByKey(keystart, Record);
                #endregion

                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpPost]
        public HttpResponseMessage updateBudgetByAccCode(BudgetCenterSetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.udpateBudgetSetup(model);
                if (result == "UPDATED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetChildOfBudgetCenterByGroup");
                    keystart.Add("getAllBudgetCenter");
                    keystart.Add("GetAllBudgetCenterByFilter");
                    keystart.Add("GetAllBudgetCenterForLocationByFilter");
                    keystart.Add("getBudgetCodeByAccCode");
                    keystart.Add("checkBudgetFlagByLocationCode");
                    keystart.Add("getBudgetCenterCodeWithChild");
                    keystart.Add("getbudgetCenterCode");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpPost]
        public HttpResponseMessage createNewBudgetHead(BudgetCenterSetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.createNewBudgetSetup(model);
                if (result == "INSERTED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetChildOfBudgetCenterByGroup");
                    keystart.Add("getAllBudgetCenter");
                    keystart.Add("GetAllBudgetCenterByFilter");
                    keystart.Add("GetAllBudgetCenterForLocationByFilter");
                    keystart.Add("getBudgetCodeByAccCode");
                    keystart.Add("checkBudgetFlagByLocationCode");
                    keystart.Add("getBudgetCenterCodeWithChild");
                    keystart.Add("getbudgetCenterCode");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpGet]
        public HttpResponseMessage getBudgetCenterDetailBybudgetCode(string budgetCode)
        {
            try
            {
                var result = this._iDocumentSetup.GetBudgetCenterDetailByBudgetCode(budgetCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }











        [HttpGet]
        public List<BudgetCenterSetupModel> GetChildOfBudgetCenterByGroup(string groupId)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<BudgetCenterSetupModel>();
            if (this._cacheManager.IsSet($"GetChildOfBudgetCenterByGroup_{userid}_{company_code}_{branch_code}_{groupId}"))
            {
                var data = _cacheManager.Get<List<BudgetCenterSetupModel>>($"GetChildOfBudgetCenterByGroup_{userid}_{company_code}_{branch_code}_{groupId}");
                response = data;
            }
            else
            {
                var budgetCenterListByGroupCodeList = this._iDocumentSetup.GetBudgetCenterListByGroupCode(groupId);
                this._cacheManager.Set($"GetChildOfBudgetCenterByGroup_{userid}_{company_code}_{branch_code}_{groupId}", budgetCenterListByGroupCodeList, 20);
                response = budgetCenterListByGroupCodeList;
            }
            return response;
            //var result = this._iDocumentSetup.GetBudgetCenterListByGroupCode(groupId);
            //return result;
        }
        [HttpGet]
        public List<BudgetCenterSetupModel> GetAllBudgetCenterList(string searchText)
        {
            return this._iDocumentSetup.GetAllBudgetCenterList(searchText);
        }

        [HttpGet]
        public List<BudgetCenter> getAllBudgetCenter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<BudgetCenter>();
            if (this._cacheManager.IsSet($"getAllBudgetCenter_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<BudgetCenter>>($"getAllBudgetCenter_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var getAllBudgetCenterList = this._iDocumentSetup.getAllBudgetCenter(filter);
                this._cacheManager.Set($"getAllBudgetCenter_{userid}_{company_code}_{branch_code}_{filter}", getAllBudgetCenterList, 20);
                response = getAllBudgetCenterList;
            }
            return response;
            //var result = this._iDocumentSetup.getAllBudgetCenter(filter);
            //return result;
        }
        #endregion

        #region Company Setup
        //company setup
        [HttpPost]
        public HttpResponseMessage updateCompanyHead(CompanySetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.updateCompanySetup(model);
                if (result == "UPDATED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }

        [HttpPost]
        public HttpResponseMessage createNewCompanyHead(CompanySetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.createNewCompanySetup(model);
                if (result == "INSERTED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        [HttpGet]
        public List<CompanySetupModel> GetCompanyGridData()
        {

            var result = this._iDocumentSetup.getAllCompanychild();

            return result;


        }

        [HttpPost]
        public HttpResponseMessage DeleteCompany(string companyCode)
        {
            try
            {
                var result = _iDocumentSetup.DeleteCompanyByCompanyCode(companyCode);


                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        //company
        [HttpGet]
        public HttpResponseMessage getCompanyDetailsByCompanyCode(string cmpanyId)
        {
            try
            {
                var result = this._iDocumentSetup.GetCompnyDetailByCompanyCode(cmpanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        #endregion

        #region Preference Setup



        //preferencesetup 
        //delete  preff
        [HttpPost]
        public HttpResponseMessage DeletePreference(string companyCode)
        {
            try
            {
                var result = _iDocumentSetup.DeletepreffBybranchCode(companyCode);


                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        //update
        [HttpPost]
        public HttpResponseMessage updatePreference(PreferenceModel model)
        {
            try
            {
                var result = this._iDocumentSetup.updatePreferenceSetup(model, _workContext.CurrentUserinformation);
                if (result == "UPDATED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        //save
        [HttpPost]
        public HttpResponseMessage createPreferenceSetup(PreferenceModel model)
        {
            try
            {
                var result = this._iDocumentSetup.createNewPreferenceSetup(model);
                if (result == "INSERTED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }


        [HttpGet]
        public HttpResponseMessage getpreferenceDetailsByCompanyCode(string cmpanyId)
        {
            try
            {
                var result = this._iDocumentSetup.GetPreferenceDetailByCompanyCode(cmpanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }


        //preference grid data 
        [HttpGet]
        public List<PreferenceModel> GetPrrffGridData()
        {

            var result = this._iDocumentSetup.getAllPreference();

            return result;


        }


        //Preference form load  
        [HttpGet]
        public List<PreferenceModel> GetPrefffromload(User userInf)
        {

            var result = this._iDocumentSetup.getFormLoad(_workContext.CurrentUserinformation);

            return result;


        }

        [HttpGet]
        public HttpResponseMessage GetCompanyPreff()
        {
            try
            {
                var data = _iDocumentSetup.getCompanyPreff();
                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new List<string>());
            }
        }
        //currency
        [HttpGet]
        public List<CurrencymultiModel> currencymultiselect()
        {

            var result = this._iDocumentSetup.getAllCurrencymulti();

            return result;


        }


        ////branch 
        [HttpGet]
        public HttpResponseMessage GetBranchPreff(string COMPANY_CODE = "")
        {
            try
            {
                var data = _iDocumentSetup.getBranchPreff(COMPANY_CODE);
                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new List<string>());
            }
        }

        #endregion


        #region Vehicle  Setup
        //VEHOICLE SETUP 
        [HttpGet]
        public List<VehicleSetupModel> getVehicleList()
        {

            var result = this._iDocumentSetup.getAllVehicle();

            return result;


        }

        [HttpGet]
        public string GetVehicleCode(string vehicletype)
        {

            var result = this._iDocumentSetup.GetVehicleCode1(vehicletype);

            return result;


        }


        //CREATE VEHICLE
        [HttpPost]
        public HttpResponseMessage createNewVehicleSetup(VehicleSetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.createVehicleSetup(model);
                if (result == "INSERTED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        //UPDATE VEHICLE
        [HttpPost]
        public HttpResponseMessage updateNewVehicleSetup(VehicleSetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.updateVehicleSetup(model);
                if (result == "UPDATED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        //DELETE VEHICLE
        [HttpPost]
        public HttpResponseMessage deleteVehicleSetup(string vehicleCode)
        {
            try
            {
                var result = _iDocumentSetup.deleteVehicleSetups(vehicleCode);

                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }

        #endregion

        #region Vehicle Registration   Setup
        //vehicle Registration

        [HttpGet]
        public HttpResponseMessage getMaxTransactionCode(string gFlag)
        {
            try
            {
                var result = this._iDocumentSetup.getMaxTransactionNo(gFlag);
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }


        [HttpGet]

        public List<VehicleRegistrationModel> GetVehicleRegistration(string from = "Others")
        {

            var result = this._iDocumentSetup.GetVehicleReg(from);

            return result;


        }

        //create Vehicle Registration
        [HttpPost]
        public HttpResponseMessage createVehicleRegistration(VehicleRegistrationModel model)
        {
            try
            {
                var result = this._iDocumentSetup.createNewVehicleReg(model);
                if (result == "INSERTED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        //Update Vehicle Restration 

        [HttpPost]
        public HttpResponseMessage updateVehicleRegistration(VehicleRegistrationModel model)
        {
            try
            {
                var result = this._iDocumentSetup.updateVehicleReg(model);
                if (result == "UPDATED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        //Delete Vehicle Registration

        [HttpPost]
        public HttpResponseMessage DeleteVehicle(string vehicleCode)
        {
            try
            {
                var result = _iDocumentSetup.DeleteVehicleRegistration(vehicleCode);


                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }

        //Get vehicle reg
        [HttpGet]
        public HttpResponseMessage getVehicleDetailsByvehicleCode(string transactionCode)
        {
            try
            {
                var result = this._iDocumentSetup.GetVehicleDetailBytrCode(transactionCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }


        #endregion


        #region Create Customer and Item Gruop fro Opera Symphony
        //save Group customer for Opera and Symphiny
        [HttpPost]
        public HttpResponseMessage createNewCustomerGroup1(CustomerModels model)
        {
            try
            {
                var result = this._iDocumentSetup.createNewCustomerSetup1(model);
                if (result == "INSERTED")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        //Create Group item for Opera and Symphony

        [HttpPost]
        public HttpResponseMessage createNewitem1(ItemSetupModalSet model)
        {

            try
            {

                var result = this._iDocumentSetup.createNewItemSetup1(model.model);
                if (result == "INSERTED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        #endregion

        #region upload
        [Route("SetupApi/PostUserImage")]
        [AllowAnonymous]
        public HttpResponseMessage PostUserImage(string id)
        {
            String fileSavePath = "";
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                // Get the uploaded image from the Files collection
                var httpPostedFile = HttpContext.Current.Request.Files["UploadedImage"];

                if (httpPostedFile != null)
                {
                    string strMappath = "~/Areas/NeoERP.DocumentTemplate/images/supplier/" + id + "/";
                    // Get the complete file path
                    fileSavePath = HttpContext.Current.Server.MapPath("~/Areas/NeoERP.DocumentTemplate/images/supplier/") + id + "\\" + httpPostedFile.FileName.ToString();

                    if (!Directory.Exists(strMappath))
                    {
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath(strMappath));
                    }

                    // Save the uploaded file to "UploadedFiles" folder
                    httpPostedFile.SaveAs(fileSavePath);

                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "OK", STATUS_CODE = (int)HttpStatusCode.OK, DATA = fileSavePath });
        }
        [Route("SetupApi/ItemPostUserImage")]
        [AllowAnonymous]
        public HttpResponseMessage ItemPostUserImage(string id)
        {
            String fileSavePath = "";
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                // Get the uploaded image from the Files collection
                var httpPostedFile = HttpContext.Current.Request.Files["UploadedImage"];

                if (httpPostedFile != null)
                {
                    string strMappath = "~/Areas/NeoERP.DocumentTemplate/images/item/" + id + "/";
                    // Get the complete file path
                    fileSavePath = HttpContext.Current.Server.MapPath("~/Areas/NeoERP.DocumentTemplate/images/item/") + id + "\\" + httpPostedFile.FileName.ToString();

                    if (!Directory.Exists(strMappath))
                    {
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath(strMappath));
                    }

                    // Save the uploaded file to "UploadedFiles" folder
                    httpPostedFile.SaveAs(fileSavePath);

                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "OK", STATUS_CODE = (int)HttpStatusCode.OK, DATA = fileSavePath });
        }
        #endregion

        #region FormControl

        [HttpGet]
        public List<FormControlModels> GetFormControlByFormCode(string formcode)
        {

            _logErp.InfoInFile("Get Form Control by Form code " + formcode + " is started");
            try
            {
                var userid = _workContext.CurrentUserinformation.User_id;
                var company_code = _workContext.CurrentUserinformation.company_code;
                var branch_code = _workContext.CurrentUserinformation.branch_code;
                var response = new List<FormControlModels>();
                if (this._cacheManager.IsSet($"GetFormControlByFormCode_{userid}_{company_code}_{branch_code}_{formcode}"))
                {
                    var data = _cacheManager.Get<List<FormControlModels>>($"GetFormControlByFormCode_{userid}_{company_code}_{branch_code}_{formcode}");
                    _logErp.InfoInFile("Form control by form code is fetched from cache");
                    response = data;
                }
                else
                {
                    var getFormControlByFormCodeList = this._iFormSetupRepo.GetFormControls(formcode);
                    _logErp.InfoInFile(getFormControlByFormCodeList.Count() + " form controls is fetched using : " + formcode);
                    this._cacheManager.Set($"GetFormControlByFormCode_{userid}_{company_code}_{branch_code}_{formcode}", getFormControlByFormCodeList, 20);
                    response = getFormControlByFormCodeList;
                }
                return response;

            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while getting form control by " + formcode + ex.Message);
                throw new Exception(ex.Message);
            }
            //var result = this._iFormSetupRepo.GetFormControls(formcode);
            //return result;
        }
        #endregion

        #region Quick Setup
        [HttpPost]
        public HttpResponseMessage insertQuickSetup(QuickSetupModel model)
        {
            try
            {

                var result = this._iDocumentSetup.InsertQuickSetup(model);
                if (result == "C_SUCCESS")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetChildOfCustomerByGroup");
                    keystart.Add("getAllItemsForCustomerStock");
                    keystart.Add("getAllComboEmployees");
                    keystart.Add("getAllComboDealers");
                    keystart.Add("GetCustomers");
                    keystart.Add("AllCustomerSetupByCode");
                    keystart.Add("AllCustomerSetupByName");
                    keystart.Add("AllCustomerSetupByAddress");
                    keystart.Add("AllCustomerSetupByPhoneno");
                    keystart.Add("AllFilterCustomer");
                    keystart.Add("customerDropDownForGroupPopup");
                    keystart.Add("GetCustomerListByCustomerCode");
                    keystart.Add("GetAllCustomerSetupByFilter");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "C_SUCCESS", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else if (result == "I_SUCCESS")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetChildOfItemByGroup");
                    keystart.Add("GetProducts");
                    keystart.Add("GetItemChargeDataSavedValueWise");
                    keystart.Add("GetItemChargeDataSavedQuantityWise");
                    keystart.Add("GetInvItemChargesData");
                    keystart.Add("GetMUCodeByProductId");
                    keystart.Add("GetMuCode");
                    keystart.Add("GetProductListByItemCode");
                    keystart.Add("GetAllProductsListByFilter");
                    keystart.Add("getProductCodeWithChild");
                    keystart.Add("GetGroupProducts");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "I_SUCCESS", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else if (result == "S_SUCCESS")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetChildOfsupplierByGroup");
                    keystart.Add("GetSuppliers");
                    keystart.Add("AllSupplierForReferenceByCode");
                    keystart.Add("AllSupplierForReferenceByName");
                    keystart.Add("AllSupplierForReferenceByaddress");
                    keystart.Add("AllSupplierForReferenceByphoneno");
                    keystart.Add("AllFilterSupplierForReference");
                    keystart.Add("AllSupplierSetupByCode");
                    keystart.Add("AllSupplierSetupByName");
                    keystart.Add("AllSupplierSetupByAddress");
                    keystart.Add("AllSupplierSetupByPhoneno");
                    keystart.Add("AllFilterSupplier");
                    keystart.Add("getsupplierCodeWithChild");
                    keystart.Add("GetSupplierListBySupplierCode");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "S_SUCCESS", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        #endregion

        #region Party type
        [HttpGet]
        public List<PartyTypeModels> getAllPartyTypes(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<PartyTypeModels>();
            if (this._cacheManager.IsSet($"getAllPartyTypes_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<PartyTypeModels>>($"getAllPartyTypes_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var getAllPartyTypesList = this._iDocumentSetup.getAllPartyTypes(filter);
                this._cacheManager.Set($"getAllPartyTypes_{userid}_{company_code}_{branch_code}_{filter}", getAllPartyTypesList, 20);
                response = getAllPartyTypesList;
            }
            return response;
            //var result = this._iDocumentSetup.getAllPartyTypes(filter);
            //return result;
        }
        #endregion

        #region currency
        [HttpGet]
        public List<Currency> getAllCurrency()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<Currency>();
            if (this._cacheManager.IsSet($"getAllCurrency_{userid}_{ company_code}_{ branch_code}"))
            {
                var data = _cacheManager.Get<List<Currency>>($"getAllCurrency_{userid}_{ company_code}_{ branch_code}");
                response = data;
            }
            else
            {
                var ggetAllCurrencyList = this._iDocumentSetup.getAllCurrency();
                this._cacheManager.Set($"getAllCurrency_{userid}_{ company_code}_{ branch_code}", ggetAllCurrencyList, 20);
                response = ggetAllCurrencyList;
            }
            return response;
            //var result = this._iDocumentSetup.getAllCurrency();
            //return result;
        }
        #endregion

        #region Country/Zone/District
        [HttpGet]
        public List<ZoneModels> getAllZones(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<ZoneModels>();
            if (this._cacheManager.IsSet($"getAllZones_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<ZoneModels>>($"getAllZones_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var getAllZonesList = this._iDocumentSetup.getAllZones(filter);
                this._cacheManager.Set($"getAllZones_{userid}_{company_code}_{branch_code}_{filter}", getAllZonesList, 20);
                response = getAllZonesList;
            }
            return response;
            //var result = this._iDocumentSetup.getAllZones(filter);
            //return result;
        }
        [HttpGet]
        public List<RegionalModels> getAllRegions(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<RegionalModels>();
            if (this._cacheManager.IsSet($"getAllRegions_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<RegionalModels>>($"getAllRegions_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var getAllRegionsList = this._iDocumentSetup.getAllRegions(filter);
                this._cacheManager.Set($"getAllRegions_{userid}_{company_code}_{branch_code}_{filter}", getAllRegionsList, 20);
                response = getAllRegionsList;
            }
            return response;
            //var result = this._iDocumentSetup.getAllRegions(filter);
            //return result;
        }
        [HttpGet]
        public List<DistrictModels> getAllDistricts(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<DistrictModels>();
            if (this._cacheManager.IsSet($"getAllDistricts_{userid} + {company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<DistrictModels>>($"getAllDistricts_{userid} + {company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var getAllDistrictsList = this._iDocumentSetup.getAllDistricts(filter);
                this._cacheManager.Set($"getAllDistricts_{userid} + {company_code}_{branch_code}_{filter}", getAllDistrictsList, 20);
                response = getAllDistrictsList;
            }
            return response;
            //var result = this._iDocumentSetup.getAllDistricts(filter);
            //return result;
        }
        [HttpGet]
        public List<CityModels> getAllCities(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<CityModels>();
            if (this._cacheManager.IsSet($"getAllCities_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<CityModels>>($"getAllCities_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var getAllCitiesList = this._iDocumentSetup.getAllCities(filter);
                this._cacheManager.Set($"getAllCities_{userid}_{company_code}_{branch_code}_{filter}", getAllCitiesList, 20);
                response = getAllCitiesList;
            }
            return response;
            //var result = this._iDocumentSetup.getAllCities(filter);
            //return result;
        }
        [HttpGet]
        public List<CountryModels> getAllCountry(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<CountryModels>();
            if (this._cacheManager.IsSet($"getAllCountry_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<CountryModels>>($"getAllCountry_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var getAllCountryList = this._iDocumentSetup.getAllCountry(filter);
                this._cacheManager.Set($"getAllCountry_{userid}_{company_code}_{branch_code}_{filter}", getAllCountryList, 20);
                response = getAllCountryList;
            }
            return response;
            //var result = this._iDocumentSetup.getAllCountry(filter);
            //return result;
        }
        [HttpGet]
        public List<BranchModels> getAllBranchs(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<BranchModels>();
            if (this._cacheManager.IsSet($"getAllBranchs_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<BranchModels>>($"getAllBranchs_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var getAllBranchsList = this._iDocumentSetup.getAllBranchs(filter);
                this._cacheManager.Set($"getAllBranchs_{userid}_{company_code}_{branch_code}_{filter}", getAllBranchsList, 20);
                response = getAllBranchsList;
            }
            return response;
            //var result = this._iDocumentSetup.getAllBranchs(filter);
            //return result;
        }


        //price List

        [HttpGet]
        public List<MasterFieldForUpdate> getAllPricelist(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<MasterFieldForUpdate>();
            if (this._cacheManager.IsSet($"getAllPricelist_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<MasterFieldForUpdate>>($"getAllPricelist_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var getAllPricelist1 = this._iDocumentSetup.getAllPricelist(filter);
                this._cacheManager.Set($"getAllPricelist_{userid}_{company_code}_{branch_code}_{filter}", getAllPricelist1, 20);
                response = getAllPricelist1;
            }
            return response;
            //var result = this._iDocumentSetup.getAllBranchs(filter);
            //return result;
        }
        [HttpGet]
        public int Backdays(string formcode)
        {
            _logErp.InfoInFile("Sales backdays calculator method started========");
            var result = this._iFormSetupRepo.GetBackDaysByFormCode(formcode);
            _logErp.InfoInFile(result + " backdays fetched");
            return result;
        }
        #endregion

        #region WebConfiguration
        [HttpPost]
        public HttpResponseMessage SaveWebPrefrence(WebPrefrence model)
        {
            try
            {

                var result = this._iDocumentSetup.SaveWebPrefrence(model);
                //  if(result.Success)
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result.Success, STATUS_CODE = (int)HttpStatusCode.OK });


                //    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });


            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        #endregion

        #region Rejectable Item Setup
        [HttpGet]
        public List<RejectableItem> getRejetableItem()
        {

            var result = this._iDocumentSetup.getRejectlbleitems();

            return result;


        }

        [HttpPost]
        public HttpResponseMessage createNewRejectableSetup(RejectableItem model)
        {
            try
            {
                var result = this._iDocumentSetup.createRejectableItemSetup(model);
                if (result == "INSERTED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        [HttpPost]
        public HttpResponseMessage updateRejectableSetup(RejectableItem model)
        {
            try
            {
                var result = this._iDocumentSetup.updateRejetableItemSetup(model);
                if (result == "UPDATED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        [HttpPost]
        public HttpResponseMessage deleteRejectableItemSetup(string itemId)
        {
            try
            {
                var result = _iDocumentSetup.deleteRejectableSetup(itemId);

                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }

        [HttpGet]
        public KYCFORM GetKfcFormByCustomerCode(string customerCode)
        {
            var result = this._iDocumentSetup.GetKYCFORM(customerCode);
            return result;
        }
        #endregion

        #region ISSUE TYPE SETUP

        [HttpGet]
        public List<IssueType> GetSavedIssueType()
        {
            try
            {
                var typeList = _iDocumentSetup.GetSavedIssueType();
                return typeList;
            }catch(Exception ex)
            {
                _logErp.ErrorInDB("Error while getting issue type : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }


        [HttpPost]
        public HttpResponseMessage SaveIssueType(IssueTypeSetupModel typeModal)
        {
            try
            {
                var message = _iDocumentSetup.SaveIssueType(typeModal);
                if (message == "INSERTED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = message, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }
            }
            catch(Exception ex)
            {
                _logErp.ErrorInDB("Error while saving issue type : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage UpdateIssueTypeSetup(IssueTypeSetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.UpdateIssueTypeSetup(model);
                if (result == "UPDATED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
       
        [HttpPost]
        public HttpResponseMessage DeleteIssueTypeSetup(string issueTypeCode)
        {
            try
            {
                var result = _iDocumentSetup.DeleteIssueTypeSetups(issueTypeCode);

                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }

        #endregion

        #region MEASUREMENT UNIT SETUP

        [HttpGet]
        public List<MeasurementUnit> GetAllMeasurementUnit()
        {
            try
            {
                var unitList = _iDocumentSetup.GetAllMeasurementUnit();
                return unitList;
            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while getting measurement unit : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }


        [HttpPost]
        public HttpResponseMessage SaveMeasurementUnit(MeasurementUnit unitModel)
        {
            try
            {
                var message = _iDocumentSetup.SaveMeasurementUnit(unitModel);
                if (message == "INSERTED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = message, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }
            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while saving measurement unit : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage UpdateMeasurementUnit(MeasurementUnit model)
        {
            try
            {
                var result = this._iDocumentSetup.UpdateMeasurementUnit(model);
                if (result == "UPDATED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        [HttpPost]
        public HttpResponseMessage DeleteMeasurementUnit(string unitCode)
        {
            try
            {
                var result = _iDocumentSetup.DeleteMeasurementUnit(unitCode);

                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }

        #endregion

        #region CITY SETUP

        [HttpGet]
        public List<CityModels> GetCities()
        {
            try
            {
                var cityList = _iDocumentSetup.GetCities();
                return cityList;
            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while getting city list : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }

        [HttpGet]
        public List<DistrictModels> GetDistricts()
        {
            try
            {
                var districtList = _iDocumentSetup.GetDistricts();
                return districtList;
            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while getting city list : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }



        [HttpPost]
        public HttpResponseMessage SaveCitySetup(CityModelForSave cityModal)
        {
            try
            {
                var message = _iDocumentSetup.SaveCitySetup(cityModal);
                if (message == "INSERTED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = message, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }
            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while saving city setup : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage UpdateCitySetup(CityModelForSave model)
        {
            try
            {
                var result = this._iDocumentSetup.UpdateCitySetup(model);
                if (result == "UPDATED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        [HttpPost]
        public HttpResponseMessage DeleteCitySetup(string cityCode)
        {
            try
            {
                var result = _iDocumentSetup.DeleteCitySetup(cityCode);

                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        #endregion

        #region Currency  Setup
        //Get list
        [HttpGet]
        public List<CurrencySetupModel> getCurrencyList()
        {
            var result = this._iDocumentSetup.getAllCurrencyCode();
            return result;
        }
        //CREATE Currency
        [HttpPost]
        public HttpResponseMessage createNewCurrencySetup(CurrencySetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.createCurrencySetup(model);
                if (result == "INSERTED")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        //UPDATE Currency
        [HttpPost]
        public HttpResponseMessage updateNewCurrencySetup(CurrencySetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.updateCurrencySetup(model);
                if (result == "UPDATED")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        //DELETE Currency
        [HttpPost]
        public HttpResponseMessage deleteCurrencySetup(string currencyCode)
        {
            try
            {
                var result = _iDocumentSetup.deleteCurrencySetup(currencyCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        #endregion

        #region Category Setup
        //Get list
        [HttpGet]
        public List<CategorySetupModel> getCategoryList()
        {
            var result = this._iDocumentSetup.getAllCategoryCode();
            return result;
        }
        //CREATE Currency
        [HttpPost]
        public HttpResponseMessage createNewCategorySetup(CategorySetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.createCategorySetup(model);
                if (result == "INSERTED")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        //UPDATE Currency
        [HttpPost]
        public HttpResponseMessage updateNewCategorySetup(CategorySetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.updateCategorySetup(model);
                if (result == "UPDATED")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        //DELETE Currency
        [HttpPost]
        public HttpResponseMessage deleteCategorySetup(string categoryCode)
        {
            try
            {
                var result = _iDocumentSetup.deleteCategorySetup(categoryCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        #endregion

        #region CHARGE TYPE SETUP

        [HttpGet]
        public List<ChargeSetupModel> GetCharges()
        {
            try
            {
                var chargeTypeList = _iDocumentSetup.GetCharges();
                return chargeTypeList;
            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while getting charge type : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }


        [HttpPost]
        public HttpResponseMessage SaveChargeSetup(ChargeSetupModel chargeModal)
        {
            try
            {
                var message = _iDocumentSetup.SaveChargeType(chargeModal);
                if (message == "INSERTED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = message, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }
            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while saving charge type : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage UpdateChargeSetup(ChargeSetupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.UpdateChargeSetup(model);
                if (result == "UPDATED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        [HttpPost]
        public HttpResponseMessage DeleteChargeSetup(string chargeCode)
        {
            try
            {
                var result = _iDocumentSetup.DeleteChargeSetup(chargeCode);

                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }

        #endregion


        #region TDS Type  Setup   

        [HttpGet]
        public HttpResponseMessage getMaxTdsCode()
        {
            try
            {
                var result = this._iDocumentSetup.getMaxTdsCode();
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }


        [HttpGet]
        public List<TDSTypeModel> getTDSList()
        {

            var result = this._iDocumentSetup.getAllTDS();

            return result;


        }

        [HttpPost]
        public HttpResponseMessage createTDSSetup(TDSTypeModel model)
        {
            try
            {
                var result = this._iDocumentSetup.createTDSSetup(model);
                if (result == "INSERTED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        [HttpPost]
        public HttpResponseMessage updateTdsSetup(TDSTypeModel model)
        {
            try
            {
                var result = this._iDocumentSetup.updatetdsSetup(model);
                if (result == "UPDATED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }


        [HttpPost]
        public HttpResponseMessage deleteTDsSetup(string tdsCode)
        {
            try
            {
                var result = _iDocumentSetup.deleteTDsSetup(tdsCode);

                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }


        #endregion

        #region Priority  Setup

        [HttpGet]
        public List<PrioritySeupModel> getPriorityList()
        {

            var result = this._iDocumentSetup.getAllPriority();

            return result;


        }

        [HttpPost]
        public HttpResponseMessage createPrioritySetup(PrioritySeupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.createPriority(model);
                if (result == "INSERTED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        [HttpPost]
        public HttpResponseMessage updateNewPrioritySetup(PrioritySeupModel model)
        {
            try
            {
                var result = this._iDocumentSetup.updatePrioritySetup(model);
                if (result == "UPDATED")
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        [HttpPost]
        public HttpResponseMessage deletePrioritySetup(string priorityCode)
        {
            try
            {
                var result = _iDocumentSetup.deletePrioritySetups(priorityCode);

                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }

        #endregion

        #region Scheme setup
        [HttpGet]
        public HttpResponseMessage getMaxSchemeCode()
        {
            try
            {
                var result = this._iDocumentSetup.getMaxSchemeCode();
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpPost]
        public HttpResponseMessage createNewSchemeSetup(SchemeModels model)
        {
            
            try
            {
                var result = this._iDocumentSetup.createNewSchemeSetup(model);
                if (result == "INSERTED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("getAllSchemes");
                    keystart.Add("getSchemeCodeWithChild");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpPost]
        public HttpResponseMessage updateSchemeSetup(SchemeModels model)
        {
            try
            {
                var result = this._iDocumentSetup.updateSchemeSetup(model);
                if (result == "UPDATED")
                {
                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("getAllSchemes");
                    keystart.Add("getSchemeCodeWithChild");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                { return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError }); }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }

        [HttpPost]
        public HttpResponseMessage deleteschemeSetup(string schemeCode)
        {
            try
            {
                var result = _iDocumentSetup.deleteSchemeSetup(schemeCode);
                #region CLEAR CACHE
                List<string> keystart = new List<string>();
                keystart.Add("getAllSchemes");
                keystart.Add("getSchemeCodeWithChild");
                List<string> Record = new List<string>();
                Record = this._cacheManager.GetAllKeys();
                this._cacheManager.RemoveCacheByKey(keystart, Record);
                #endregion
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }
        [HttpPost]
        public HttpResponseMessage implementschemeSetup(string schemeCode)
        {
            try
            {
                var result = _iDocumentSetup.ImplementScheme(schemeCode);
                #region CLEAR CACHE
                List<string> keystart = new List<string>();
                keystart.Add("getAllSchemes");
                keystart.Add("getSchemeCodeWithChild");
                List<string> Record = new List<string>();
                Record = this._cacheManager.GetAllKeys();
                this._cacheManager.RemoveCacheByKey(keystart, Record);
                #endregion
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }

        }

        [HttpGet]
        public List<SchemeModels> GetAllSchemeCode()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<SchemeModels>();
            if (this._cacheManager.IsSet($"GetAllSchemeCode_{userid}_{ company_code}_{ branch_code}"))
            {
                var data = _cacheManager.Get<List<SchemeModels>>($"GetAllSchemeCode_{userid}_{ company_code}_{ branch_code}");
                response = data;
            }
            else
            {
                var getAllSchemeCodeList = this._iDocumentSetup.getAllSchemeCodeDetail();
                foreach (var data in getAllSchemeCodeList)
                {
                    StringWriter myWriter = new StringWriter();

                    // Decode the encoded string.
                    HttpUtility.HtmlDecode(data.QUERY_STRING, myWriter);

                    string myDecodedString = myWriter.ToString();
                    data.QUERY_STRING = myDecodedString;
                }
                this._cacheManager.Set($"GetAllSchemeCode_{userid}_{ company_code}_{ branch_code}", getAllSchemeCodeList, 20);
                response = getAllSchemeCodeList;
            }
            return response;
           
        }
        [HttpGet]
        public List<SchemeModels> getAllSchemes(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<SchemeModels>();
            if (this._cacheManager.IsSet($"getAllSchemes_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<SchemeModels>>($"getAllSchemes_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var getAllSchemesList = this._iDocumentSetup.getAllScheme(filter);
                this._cacheManager.Set($"getAllSchemes_{userid}_{company_code}_{branch_code}_{filter}", getAllSchemesList, 20);
                response = getAllSchemesList;
            }
            return response;
           
        }
        [HttpPost]
        public HttpResponseMessage ImpactVoucherBySchemeAll(ImpactVoucherModel model)
        {
            var schemeImplementVal = JsonConvert.DeserializeObject<List<SchemeImplementModel>>(model.SCHEME_IMPLEMENT_VALUE);
            List<FinancialSubLedger> fa = null;
            if (model.SUB_LEDGER_VALUE != null) fa = JsonConvert.DeserializeObject<List<FinancialSubLedger>>(model.SUB_LEDGER_VALUE);
            var result = "SUCCESS";
            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
        }

        [HttpPost]
        public HttpResponseMessage ImpactVoucherByScheme(List<SchemeImplementModel> models)
        {
           
            try
            {
                string FormCodeQuery = $@"SELECT FORM_CODE,CHARGE_ACCOUNT_CODE,CHARGE_CODE,TO_CHAR(CHARGE_RATE) AS CHARGE_RATE FROM SCHEME_SETUP WHERE SCHEME_CODE='{models[0].SCHEME_CODE}'";
                var FormCodeData = this._dbContext.SqlQuery<SchemeImplementModel>(FormCodeQuery).FirstOrDefault();
                String[] form_codes = new String[100];
                if (FormCodeData.FORM_CODE != "" && FormCodeData.FORM_CODE != null)
                {
                    if (FormCodeData.FORM_CODE.Contains(','))
                    {
                        form_codes = FormCodeData.FORM_CODE.Split(',');
                    }
                    else
                    {
                        form_codes[0] = FormCodeData.FORM_CODE;
                    }
                    List<string> y = form_codes.ToList<string>();
                    y.RemoveAll(p => string.IsNullOrEmpty(p));
                    form_codes = y.ToArray();
                }
                var result = string.Empty;
                var customerCode= string.Empty;
                var ParTypeCode = string.Empty;

                foreach (var form_code in form_codes)
                {
                    //foreach (var model in models)
                    //{
                    //    if (model.CUSTOMER_CODE != null)
                    //    {
                    //        result = _iDocumentSetup.ImpactSchemeOnVoucherCustomer(model, form_code, FormCodeData.CHARGE_ACCOUNT_CODE, FormCodeData.CHARGE_CODE, FormCodeData.CHARGE_RATE);
                    //        if (result == "NOTMAPPED")
                    //        {
                    //            string customer_name = _iDocumentSetup.GetcustomerNameByCode(model.CUSTOMER_CODE);
                    //            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, CUSTOMER_CODE= model.CUSTOMER_CODE, CUSTOMER_NAME= customer_name, PARTY_TYPE_CODE = "", PARTY_TYPE_NAME = "", STATUS_CODE = (int)HttpStatusCode.OK });
                    //        }

                    //    }
                    //    if (model.PARTY_TYPE_CODE != null)
                    //    {
                    //        result = _iDocumentSetup.ImpactSchemeOnVoucherPartyType(model, form_code, FormCodeData.CHARGE_ACCOUNT_CODE, FormCodeData.CHARGE_CODE, FormCodeData.CHARGE_RATE);
                    //        if(result== "NOTMAPPED")
                    //        {
                    //            string party_type_name = _iDocumentSetup.GetParytTypeNameByCode(model.PARTY_TYPE_CODE);
                    //            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, CUSTOMER_CODE = "", CUSTOMER_NAME = "", PARTY_TYPE_CODE =model.PARTY_TYPE_CODE, PARTY_TYPE_NAME= party_type_name, STATUS_CODE = (int)HttpStatusCode.OK });
                    //        }
                    //    }

                    //}
                    result = _iDocumentSetup.ImpactSchemeOnVoucher(models, form_code,FormCodeData.CHARGE_ACCOUNT_CODE, FormCodeData.CHARGE_CODE, FormCodeData.CHARGE_RATE);
                  
                }
                   
             
                
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
            
        }

        #endregion


        #region calculateInterest
        [HttpGet]
        public List<InterestCalculationResultModel> bindCalculateInterestDetails(int RATE, string CUSTOMER_CODE,string GROUP_CODES, DateTime UPTO_DATE,string COMPANY_CODE,string BRANCH_CODE)
        //public List<InterestCalculationResultModel> bindCalculateInterestDetails()
        {
            var BsDateQuery = $@"select to_bs(sysdate) from dual";
            var BsDate = this._dbContext.SqlQuery<string>(BsDateQuery).FirstOrDefault();
            var todaytime= DateTime.Now.ToShortTimeString();
            CompanyBranchInfo branchinfo = new CompanyBranchInfo();
            var branchinfoQuery=$@"select cs.tpin_vat_no as C_TPIN_VAT_NO,bs.address as B_ADDRESS,bs.telephone_no as B_TELEPHONE_NO,bs.email as B_EMAIL from company_setup cs,fa_branch_setup bs where cs.company_code=bs.company_code and bs.branch_code='{BRANCH_CODE}'";

            branchinfo = this._dbContext.SqlQuery<CompanyBranchInfo>(branchinfoQuery).FirstOrDefault();

            InterestCalculationModel model = new InterestCalculationModel();
            model.RATE =Convert.ToDecimal(RATE);
            model.CUSTOMER_CODE = CUSTOMER_CODE;
            model.UPTO_DATE = Convert.ToDateTime(UPTO_DATE);
            model.COMPANY_CODE = COMPANY_CODE;
            model.BRANCH_CODE = BRANCH_CODE;
            model.GROUP_CODES = GROUP_CODES;


            var response = new List<InterestCalculationResultModel>();
            var InterestGridList = this._iDocumentSetup.CalculateInterestByPara(model);
           
            //var customerDetail = this._FormTemplateRepo.GetCustomerDetail(CUSTOMER_CODE).FirstOrDefault();
            foreach (var InterestGrid in InterestGridList)
            {
                var customerDetail = this._FormTemplateRepo.GetCustomerDetail(InterestGrid.CUSTOMER_CODE).FirstOrDefault();
                InterestGrid.REGD_OFFICE_EADDRESS = customerDetail.REGD_OFFICE_EADDRESS;
                InterestGrid.TEL_MOBILE_NO1 = customerDetail.TEL_MOBILE_NO1;
                InterestGrid.TPIN_VAT_NO = customerDetail.TPIN_VAT_NO;
                InterestGrid.TOTL_INT_PARENT= bindCustomerInterestDetails(RATE, InterestGrid.CUSTOMER_CODE, model.UPTO_DATE, model.COMPANY_CODE, model.BRANCH_CODE).Select(x => x.INTEREST).Sum();
                //InterestGrid.TOTAL_INTEREST = InterestGridList.Select(x => x.INTEREST).Sum();
                //InterestGrid.TOTAL_OUTSTANDING_BEF= InterestGridList.Select(x => x.BALANCE).Sum();
                //InterestGrid.TOTAL_OUTSTANDING_AF = InterestGrid.TOTAL_OUTSTANDING_BEF + InterestGrid.TOTAL_INTEREST;
                InterestGrid.TODAY_DATE = BsDate;
                InterestGrid.EMAIL = customerDetail.EMAIL;
                InterestGrid.TODAY_TIME = todaytime;
            }
            if (branchinfo != null)
            {
                foreach (var IG in InterestGridList)
                {
                    IG.C_TPIN_VAT_NO = branchinfo.C_TPIN_VAT_NO;
                    IG.B_TELEPHONE_NO = branchinfo.B_TELEPHONE_NO;
                    IG.B_ADDRESS = branchinfo.B_ADDRESS;
                    IG.B_EMAIL = branchinfo.B_EMAIL;
                }
            }

            response = InterestGridList;
            return response;
        }

        [HttpGet]
        public List<InterestCalcResDetailModel> bindCustomerInterestDetails(int RATE, string CUSTOMER_CODE, DateTime UPTO_DATE, string COMPANY_CODE, string BRANCH_CODE)
        //public List<InterestCalculationResultModel> bindCalculateInterestDetails()
        {
            var customerDetail = this._FormTemplateRepo.GetCustomerDetail(CUSTOMER_CODE).FirstOrDefault();
            InterestCalculationModel model = new InterestCalculationModel();
            model.RATE = Convert.ToDecimal(RATE);
            model.CUSTOMER_CODE = CUSTOMER_CODE;
            model.UPTO_DATE = Convert.ToDateTime(UPTO_DATE);
            model.COMPANY_CODE = COMPANY_CODE;
            model.BRANCH_CODE = BRANCH_CODE;

            var response = new List<InterestCalcResDetailModel>();
            var InterestGridList = this._iDocumentSetup.CalculateInterestDetailsByPara(model);
            foreach (var InterestGrid in InterestGridList)
            {
                InterestGrid.TOTAL_INTEREST = InterestGridList.Select(x => x.INTEREST).Sum();
                InterestGrid.TOTAL_OUTSTANDING_BEF = InterestGridList.Select(x => x.BALANCE).Sum();
                InterestGrid.TOTAL_OUTSTANDING_AF = InterestGrid.TOTAL_OUTSTANDING_BEF + InterestGrid.TOTAL_INTEREST;
                InterestGrid.REGD_OFFICE_EADDRESS = customerDetail.REGD_OFFICE_EADDRESS;
                InterestGrid.TEL_MOBILE_NO1 = customerDetail.TEL_MOBILE_NO1;
                InterestGrid.TPIN_VAT_NO = customerDetail.TPIN_VAT_NO;
            }
                response = InterestGridList;
           
            return response;
        }
        [HttpPost]
        public HttpResponseMessage ImpactInterestCalculation(InterestCalcPostModel model)
        {
            var InterestCalculatedVal = JsonConvert.DeserializeObject<List<InterestCalculationResultModel>>(model.INTERESET_DATA);
            var InterestCalcParaVal = JsonConvert.DeserializeObject<InterestCalculcImpacttModel>(model.INTEREST_PARAM_DATA);
            var result = _iDocumentSetup.CreateInterestImpact(InterestCalculatedVal, InterestCalcParaVal);
            if (result == "INSERTED")
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
            }
            else if (result == "CustomerExist")
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "CustomerExist", STATUS_CODE = (int)HttpStatusCode.OK });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "FAIL", STATUS_CODE = (int)HttpStatusCode.OK });
            }
        }

        [HttpGet]
        public List<InterestCalcLogModel> bindInterestCalcLogDetails()
        {
            var result = _iDocumentSetup.GetInterestCalcLog();
            return result;
        }

        #endregion

    }
}
