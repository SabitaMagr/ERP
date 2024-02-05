//using Microsoft.Owin;
using NeoErp.Core;
using NeoErp.Core.Caching;
using NeoErp.Core.Models;
using NeoErp.Core.Models.Log4NetLoggin;
using NeoErp.Core.Plugins;
using NeoErp.Core.Services.CommonSetting;
using NeoErp.Data;
using NeoERP.DocumentTemplate.Service.Interface;
using NeoERP.DocumentTemplate.Service.Models;
using NeoERP.DocumentTemplate.Service.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

namespace NeoERP.DocumentTemplate.Controllers.Api
{
    public class TemplateApiController : ApiController
    {
        private const string DT = "DOCUMENT_TEMPLATE";
        private ITestTemplateRepo _TestTemplateRepo;
        private IFormTemplateRepo _FormTemplateRepo;
        private IFormSetupRepo _FormSetupRepo;
        private ISalesOrderRepo _SalesOrderRepo;
        private IDbContext _dbContext;
        private IWorkContext _workContext;
        private ICacheManager _cacheManager;
        private IPluginFinder _pluginFinder;
        private NeoErpCoreEntity _objectEntity;
        private DefaultValueForLog _defaultValueForLog;
        private ILogErp _logErp;
        private ISettingService _settingService;
        private ISaveDocTemplate _saveDocTemplate;
        private ISaveDocTemplateSalesModule _saveDocTemplateSalesModule;
        public TemplateApiController(ITestTemplateRepo TestTemplateRepo, IFormTemplateRepo FormTemplateRepo,
            IFormSetupRepo FormSetupRepo, ISalesOrderRepo SalesOrderRepo, IDbContext dbContext,
            IWorkContext workContext, ICacheManager cacheManager, IPluginFinder pluginFinder,
            NeoErpCoreEntity objectEntity, ISettingService settingService,ISaveDocTemplate saveDocTemplate,ISaveDocTemplateSalesModule saveDocTemplateSalesModule)
        {
            this._TestTemplateRepo = TestTemplateRepo;
            this._FormTemplateRepo = FormTemplateRepo;
            this._FormSetupRepo = FormSetupRepo;
            this._dbContext = dbContext;
            this._workContext = workContext;
            this._cacheManager = cacheManager;
            this._SalesOrderRepo = SalesOrderRepo;
            this._pluginFinder = pluginFinder;
            this._objectEntity = objectEntity;
            this._settingService = settingService;
            this._saveDocTemplate = saveDocTemplate;
            this._saveDocTemplateSalesModule = saveDocTemplateSalesModule;
            this._defaultValueForLog = new DefaultValueForLog(this._workContext);
            this._logErp = new LogErp(this, _defaultValueForLog.LogUser, _defaultValueForLog.LogCompany, _defaultValueForLog.LogBranch, _defaultValueForLog.LogTypeCode, _defaultValueForLog.LogModule,_defaultValueForLog.FormCode);
        }
        [HttpGet]
        public List<TestTemplate> GetAllTemplateList()
        {
            List<TestTemplate> lst = new List<TestTemplate>();
            lst.Add(new TestTemplate { name = "test name" });
            return lst;
        }
        [HttpGet]
        public List<FormDetailSetup> GetAllFormDetailSetup()
        {

            return this._TestTemplateRepo.GetAllFORMDETAILSETUP();
        }
        [HttpGet]
        public List<FormDetailSetup> GetFormDetailSetup(string formCode, string orderno)
        {
            orderno = orderno.Remove(orderno.Length - formCode.Length, formCode.Length);
            _logErp.InfoInFile("Get Form Details Setup for : " + formCode + " formcode");
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;

            var response = new List<FormDetailSetup>();
            var addedFormDetailSetup = new List<FormDetailSetup>();
            var data = new List<FormDetailSetup>();
            var thrdQty = 0;
            var sndQty = 0;

            if (this._cacheManager.IsSet($"fromdetailsetup_{_workContext.CurrentUserinformation.User_id}_{company_code}_{branch_code}_{formCode}"))
            {
                //_cacheManager.Clear();
                 data = _cacheManager.Get<List<FormDetailSetup>>($"fromdetailsetup_{_workContext.CurrentUserinformation.User_id}_{company_code}_{branch_code}_{formCode}");
                var checkchargelist = $@"select * from charge_setup where form_code = '{formCode}' and company_code ='{company_code}'";
                List<ChargeSetup> checkchargelistentity = this._dbContext.SqlQuery<ChargeSetup>(checkchargelist).ToList();
                if (data.Where(x => x.COLUMN_NAME == "THIRD_QUANTITY").Count() > 0)
                    thrdQty = data.Where(x => x.COLUMN_NAME == "THIRD_QUANTITY").FirstOrDefault().SERIAL_NO = data.Where(x => x.COLUMN_NAME == "QUANTITY").Select(p => p.SERIAL_NO).FirstOrDefault();
                if (data.Where(x => x.COLUMN_NAME == "THIRD_QUANTITY").Count() > 0)
                    sndQty = data.Where(x => x.COLUMN_NAME == "SECOND_QUANTITY").FirstOrDefault().SERIAL_NO = data.Where(x => x.COLUMN_NAME == "QUANTITY").Select(p => p.SERIAL_NO).FirstOrDefault();               
                data.RemoveAll(x => checkchargelistentity.Any(t => t.CHARGE_CODE == x.COLUMN_NAME));
                string columname = "";
                if (orderno.Contains("undefined"))
                {
                    columname = $@"select distinct ICH.charge_edesc,CH.charge_code,CH.charge_type_flag,CH.VALUE_PERCENT_FLAG
                                ,CH.VALUE_PERCENT_AMOUNT,CH.ON_ITEM,CH.priority_index_no,ch.manual_calc_charge from charge_setup CH
                                INNER JOIN ip_charge_code ICH ON ICH.charge_code = CH.charge_code
                                where CH.form_code = '{formCode}' and CH.company_code ='{company_code}' and CH.ON_ITEM ='Y' order by CH.priority_index_no";
                }
                else
                {
                    columname = $@"select distinct  ICH.charge_edesc,CH.charge_code,CH.charge_type_flag,CH.VALUE_PERCENT_FLAG
                                ,CH.VALUE_PERCENT_AMOUNT,CH.ON_ITEM,CH.priority_index_no,ch.manual_calc_charge from charge_setup CH
                                LEFT JOIN ip_charge_code ICH ON ICH.charge_code = CH.charge_code 
                                LEFT JOIN charge_transaction CT ON CT.CHARGE_CODE = CH.charge_code
                                where CH.form_code = '{formCode}' and CH.company_code ='{company_code}' AND CT.reference_no ='{orderno}' order by CH.priority_index_no";
                }                            
                List<IPChargeEdesc> columnameentity = this._dbContext.SqlQuery<IPChargeEdesc>(columname).ToList();               
                for (int i = 0; i < columnameentity.Count; i++)
                {
                    var columncharge = new FormDetailSetup();
                    columncharge.SERIAL_NO = columnameentity[i].CHARGE_CODE == "VT" ? 27 : 23 ;
                    columncharge.DISPLAY_FLAG = "Y";
                    columncharge.FORM_CODE= formCode;
                    columncharge.DELETED_FLAG = "N";
                    columncharge.COLUMN_HEADER = columnameentity[i].CHARGE_EDESC;
                    columncharge.COLUMN_NAME = columnameentity[i].CHARGE_CODE;
                    columncharge.MASTER_CHILD_FLAG = "C";
                    columncharge.LEFT_POSITION = 1600;
                    columncharge.CHARGE_TYPE_FLAG = columnameentity[i].CHARGE_TYPE_FLAG;
                    columncharge.VALUE_PERCENT_FLAG = columnameentity[i].VALUE_PERCENT_FLAG;
                    columncharge.VALUE_PERCENT_AMOUNT = columnameentity[i].VALUE_PERCENT_AMOUNT;
                    columncharge.ON_ITEM = columnameentity[i].ON_ITEM;
                    columncharge.MANUAL_CALC_CHARGE = columnameentity[i].MANUAL_CALC_CHARGE;
                    if (!data.Select(x => x.COLUMN_HEADER).Contains(columncharge.COLUMN_HEADER))
                        data.Add(columncharge);
                    //if (columnameentity[i].CHARGE_CODE != "VT")
                    //{
                    //    var manualCalculation = new FormDetailSetup();
                    //    manualCalculation.SERIAL_NO = 25;
                    //    manualCalculation.DISPLAY_FLAG = "Y";
                    //    manualCalculation.FORM_CODE = formCode;
                    //    manualCalculation.DELETED_FLAG = "N";
                    //    manualCalculation.COLUMN_HEADER = "MC." + columncharge.COLUMN_NAME;
                    //    manualCalculation.COLUMN_NAME = "MC" + columncharge.COLUMN_NAME;
                    //    manualCalculation.MASTER_CHILD_FLAG = "C";
                    //    manualCalculation.LEFT_POSITION = 1600;
                    //    if (!data.Select(x => x.COLUMN_NAME).Contains(manualCalculation.COLUMN_NAME))
                    //        data.Add(manualCalculation);
                    //}                 
                }
                _logErp.InfoInFile(data.Count() + " Form Details setup has been fetched from cached for " + formCode + " formcode");
                //response = data;
                response = data.Where(x => !x.COLUMN_NAME.Contains("CALC_QUANTITY") && !x.COLUMN_NAME.Contains("CALC_UNIT_PRICE") && !x.COLUMN_NAME.Contains("CALC_TOTAL_PRICE") && !x.COLUMN_NAME.Contains("COMPLETED_QUANTITY")).ToList();
            }
            else
            {
                var formDetailList = this._FormTemplateRepo.GetFormDetailSetup(formCode, orderno);

                var checkchargelist = $@"select * from charge_setup where form_code = '{formCode}' and company_code ='{company_code}'";
                List<ChargeSetup> checkchargelistentity = this._dbContext.SqlQuery<ChargeSetup>(checkchargelist).ToList();
                
                if(formDetailList.Where(x => x.COLUMN_NAME == "THIRD_QUANTITY").Count() > 0)               
                    thrdQty = formDetailList.Where(x => x.COLUMN_NAME == "THIRD_QUANTITY").FirstOrDefault().SERIAL_NO = formDetailList.Where(x => x.COLUMN_NAME == "QUANTITY").Select(p => p.SERIAL_NO).FirstOrDefault();
                if (formDetailList.Where(x => x.COLUMN_NAME == "THIRD_QUANTITY").Count() > 0)
                    sndQty = formDetailList.Where(x => x.COLUMN_NAME == "SECOND_QUANTITY").FirstOrDefault().SERIAL_NO = formDetailList.Where(x => x.COLUMN_NAME == "QUANTITY").Select(p => p.SERIAL_NO).FirstOrDefault();
                formDetailList.RemoveAll(x => checkchargelistentity.Any(t => t.CHARGE_CODE == x.COLUMN_NAME));
                string columname = "";

                if (orderno.Contains("undefined"))
                {
                    columname = $@"select distinct ICH.charge_edesc,CH.charge_code,CH.charge_type_flag,CH.VALUE_PERCENT_FLAG
                                ,CH.VALUE_PERCENT_AMOUNT,CH.ON_ITEM, CH.priority_index_no,ch.manual_calc_charge from charge_setup CH
                                INNER JOIN ip_charge_code ICH ON ICH.charge_code = CH.charge_code
                                where CH.form_code = '{formCode}' and CH.company_code ='{company_code}' and CH.ON_ITEM ='Y' order by CH.priority_index_no";
                }
                else
                {
                    columname = $@"select distinct  ICH.charge_edesc,CH.charge_code,CH.charge_type_flag,CH.VALUE_PERCENT_FLAG
                                ,CH.VALUE_PERCENT_AMOUNT,CH.ON_ITEM,CH.priority_index_no,ch.manual_calc_charge from charge_setup CH
                                LEFT JOIN ip_charge_code ICH ON ICH.charge_code = CH.charge_code 
                                LEFT JOIN charge_transaction CT ON CT.CHARGE_CODE = CH.charge_code
                                where CH.form_code = '{formCode}' and CH.company_code ='{company_code}' AND CT.reference_no ='{orderno}' order by CH.priority_index_no";
                }

                List<IPChargeEdesc> columnameentity = this._dbContext.SqlQuery<IPChargeEdesc>(columname).ToList();

                for (int i = 0; i < columnameentity.Count; i++)
                {
                    var columncharge = new FormDetailSetup();
                    columncharge.SERIAL_NO = columnameentity[i].CHARGE_CODE == "VT" ? 27 : 23;
                    columncharge.DISPLAY_FLAG = "Y";
                    columncharge.FORM_CODE = formCode;
                    columncharge.DELETED_FLAG = "N";
                    columncharge.COLUMN_HEADER = columnameentity[i].CHARGE_EDESC;
                    columncharge.COLUMN_NAME = columnameentity[i].CHARGE_CODE;
                    columncharge.MASTER_CHILD_FLAG = "C";
                    columncharge.LEFT_POSITION = 1600;
                    columncharge.CHARGE_TYPE_FLAG = columnameentity[i].CHARGE_TYPE_FLAG;
                    columncharge.VALUE_PERCENT_FLAG = columnameentity[i].VALUE_PERCENT_FLAG;
                    columncharge.VALUE_PERCENT_AMOUNT = columnameentity[i].VALUE_PERCENT_AMOUNT;
                    columncharge.MANUAL_CALC_CHARGE = columnameentity[i].MANUAL_CALC_CHARGE;
                    columncharge.ON_ITEM = columnameentity[i].ON_ITEM;                  
                    if (!formDetailList.Select(x => x.COLUMN_HEADER).Contains(columncharge.COLUMN_HEADER))
                        formDetailList.Add(columncharge);
                    //if (columnameentity[i].CHARGE_CODE != "VT")
                    //{
                    //    var manualCalculation = new FormDetailSetup();
                    //    manualCalculation.SERIAL_NO = 25;
                    //    manualCalculation.DISPLAY_FLAG = "Y";
                    //    manualCalculation.FORM_CODE = formCode;
                    //    manualCalculation.DELETED_FLAG = "N";
                    //    //manualCalculation.COLUMN_HEADER = "MC." + columncharge.COLUMN_NAME;
                    //    //manualCalculation.COLUMN_NAME = "MC" + columncharge.COLUMN_NAME;
                    //    manualCalculation.MASTER_CHILD_FLAG = "C";
                    //    manualCalculation.LEFT_POSITION = 1600;
                    //    if (!formDetailList.Select(x => x.COLUMN_NAME).Contains(manualCalculation.COLUMN_NAME))
                    //        formDetailList.Add(manualCalculation);
                    //}
                }
              
                this._cacheManager.Set($"fromdetailsetup_{_workContext.CurrentUserinformation.User_id}_{company_code}_{branch_code}_{formCode}", formDetailList, 20);
                _logErp.InfoInFile(formDetailList.Count() + " form details setup has beed fetched for " + formCode + " formcode");
                //response = formDetailList;
                response = formDetailList.Where(x => !x.COLUMN_NAME.Contains("CALC_QUANTITY") && !x.COLUMN_NAME.Contains("CALC_UNIT_PRICE") && !x.COLUMN_NAME.Contains("CALC_TOTAL_PRICE") && !x.COLUMN_NAME.Contains("COMPLETED_QUANTITY")).ToList();
            }
           

            var ta = new FormDetailSetup();
            ta.SERIAL_NO = 26;
            ta.DISPLAY_FLAG = "Y";
            ta.FORM_CODE = formCode;
            ta.DELETED_FLAG = "N";
            ta.COLUMN_HEADER = "TAXABLE AMOUNT";
            ta.COLUMN_NAME = "TA";
            ta.MASTER_CHILD_FLAG = "C";
            ta.LEFT_POSITION = 1600;
            ta.DISPLAY_RATE = response.Select(p => p.DISPLAY_RATE).FirstOrDefault();
            ta.RATE_SCHEDULE_FIX_PRICE = response.Select(p => p.RATE_SCHEDULE_FIX_PRICE).FirstOrDefault();
            ta.PRICE_CONTROL_FLAG = response.Select(p => p.PRICE_CONTROL_FLAG).FirstOrDefault();
            ta.REF_FIX_PRICE = response.Select(p => p.REF_FIX_PRICE).FirstOrDefault();
            if (!response.Select(x => x.COLUMN_HEADER).Contains(ta.COLUMN_HEADER))
                response.Add(ta);

            var na = new FormDetailSetup();
            na.SERIAL_NO = 28;
            na.DISPLAY_FLAG = "Y";
            na.FORM_CODE = formCode;
            na.DELETED_FLAG = "N";
            na.COLUMN_HEADER = "NET AMOUNT";
            na.COLUMN_NAME = "NA";
            na.MASTER_CHILD_FLAG = "C";
            na.LEFT_POSITION = 4600;
            na.DISPLAY_RATE = response.Select(p => p.DISPLAY_RATE).FirstOrDefault();
            na.RATE_SCHEDULE_FIX_PRICE = response.Select(p => p.RATE_SCHEDULE_FIX_PRICE).FirstOrDefault();
            na.PRICE_CONTROL_FLAG = response.Select(p => p.PRICE_CONTROL_FLAG).FirstOrDefault();
            na.REF_FIX_PRICE = response.Select(p => p.REF_FIX_PRICE).FirstOrDefault();
            if (!response.Select(x => x.COLUMN_HEADER).Contains(na.COLUMN_HEADER))
                response.Add(na);

            return response;
        }
        [HttpGet]
        public List<DraftFormModel> GetDraftFormDetail(string formCode)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<DraftFormModel>();
            //if (this._cacheManager.IsSet($"draftformdetail_{userid}_{company_code}_{branch_code}_{formCode}"))
            //{
            //    var data = _cacheManager.Get<List<DraftFormModel>>($"draftformdetail_{userid}_{company_code}_{branch_code}_{formCode}");
            //    response = data;
            //}
            //else
            //{
            //    var draftDetailList = this._FormTemplateRepo.GetDraftFormDetailSetup(formCode);
            //    this._cacheManager.Set($"draftformdetail_{userid}_{company_code}_{branch_code}_{formCode}", draftDetailList, 20);
            //    response = draftDetailList;
            //}
            var draftDetailList = this._FormTemplateRepo.GetDraftFormDetailSetup(formCode);
            response = draftDetailList;
            return response;
        }
        [HttpGet]
        public List<FormSetup> GetFormSetup()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<FormSetup>();
            if (this._cacheManager.IsSet($"GetForm_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<FormSetup>>($"GetForm_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var fromsetup = this._FormSetupRepo.GetFormSetup();
                this._cacheManager.Set($"GetForm_{userid}_{company_code}_{branch_code}", fromsetup, 20);
                response = fromsetup;
            }
            return response;
        }
        [HttpGet]
        public List<FormSetup> GetFormSetup(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<FormSetup>();
            if (this._cacheManager.IsSet($"GetForms_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<FormSetup>>($"GetForms_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var fromsetup = this._FormSetupRepo.GetFormSetup(filter);
                this._cacheManager.Set($"GetForms_{userid}_{company_code}_{branch_code}_{filter}", fromsetup, 20);
                response = fromsetup;
            }
            return response;
        }
        [HttpGet]
        public List<FormSetup> GetFormSetupByFormCode(string formCode)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<FormSetup>();
            if (this._cacheManager.IsSet($"GetFormSetupByFormCode_{userid}_{company_code}_{branch_code}_{formCode}"))
            {
                var data = _cacheManager.Get<List<FormSetup>>($"GetFormSetupByFormCode_{userid}_{company_code}_{branch_code}_{formCode}");
                response = data;
            }
            else
            {
                var fromsetup = this._FormSetupRepo.GetFormSetupByFormCode(formCode);
                this._cacheManager.Set($"GetFormSetupByFormCode_{userid}_{company_code}_{branch_code}_{formCode}", fromsetup, 20);
                response = fromsetup;
            }
            return response;
        }
        [HttpGet]
        public List<TemplateDraftListModel> GetAllMenuInventoryAssigneeDraftTemplateList()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<TemplateDraftListModel>();
            if (this._cacheManager.IsSet($"GetAllMenuInventoryAssigneeDraftTemplateList_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<TemplateDraftListModel>>($"GetAllMenuInventoryAssigneeDraftTemplateList_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var DraftTemplateList = this._FormSetupRepo.GetAllMenuInventoryAssigneeDraftTemplateList();
                this._cacheManager.Set($"GetAllMenuInventoryAssigneeDraftTemplateList_{userid}_{company_code}_{branch_code}", DraftTemplateList, 20);
                response = DraftTemplateList;
            }
            return response;
        }
        [HttpGet]
        public List<TemplateDraftListModel> GetAllMenuInventoryAssigneeSavedDraftTemplateList()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<TemplateDraftListModel>();
            if (this._cacheManager.IsSet($"GetAllMenuInventoryAssigneeSavedDraftTemplateList_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<TemplateDraftListModel>>($"GetAllMenuInventoryAssigneeSavedDraftTemplateList_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var SavedDraftTemplateList = this._FormSetupRepo.GetAllMenuInventoryAssigneeSavedDraftTemplateList();
                this._cacheManager.Set($"GetAllMenuInventoryAssigneeSavedDraftTemplateList_{userid}_{company_code}_{branch_code}", SavedDraftTemplateList, 20);
                response = SavedDraftTemplateList;
            }
            return response;
        }
        [HttpGet]
        public List<DraftFormModel> GetAllDraftTemplateDatabyTempCode(string tempCode)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<DraftFormModel>();
            if (this._cacheManager.IsSet($"DraftTemplateDatabyTempCode_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<DraftFormModel>>($"DraftTemplateDatabyTempCode_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var DraftTemplateDatabyTempCode = this._FormSetupRepo.GetAllDraftTemplateDatabyTempCode(tempCode);
                this._cacheManager.Set($"DraftTemplateDatabyTempCode_{userid}_{company_code}_{branch_code}", DraftTemplateDatabyTempCode, 20);
                response = DraftTemplateDatabyTempCode;
            }
            return response;
        }
        [HttpGet]
        public List<Customers> GetAllCustomerSetupByFilter(string filter)
        {

            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var Filterdata = new List<Customers>();
            if (filter == "!@$")
                return Filterdata;
            if (filter == null)
                return Filterdata;
            var ShowAdvanceAutoComplete = false;
            //Constants.
            var setting = _settingService.LoadSetting<WebPrefrenceSetting>(Constants.WebPrefranceSetting);
            if (setting != null)
                bool.TryParse(setting.ShowAdvanceAutoComplete, out ShowAdvanceAutoComplete);
            if (ShowAdvanceAutoComplete == false)
            {
                if (this._cacheManager.IsSet($"AllFilterCustomer_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}"))
                {
                    var data = _cacheManager.Get<List<Customers>>($"AllFilterCustomer_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}");
                    Filterdata = data;
                    return Filterdata;
                }
                else
                {
                    var AllFilterCustomer = this._FormTemplateRepo.GetAllCustomerSetup(filter);
                    this._cacheManager.Set($"AllFilterCustomer_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}", AllFilterCustomer, 20);
                    return AllFilterCustomer;
                }
            }

           
            if (filter.Contains("#"))
            {
                var colName = filter.Split('#')[0].ToString().ToLower();
                filter = filter.Split('#')[1].ToString().ToLower();
                if (colName == "code")
                {
                    if (this._cacheManager.IsSet($"AllCustomerSetupByCode_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Customers>>($"AllCustomerSetupByCode_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterCustomer = this._FormTemplateRepo.GetAllCustomerSetup(filter);
                        var CustomerCodes = AllFilterCustomer.Where(x => x.CustomerCode.ToLower().Contains(filter));
                        var StartWithCustomerCode = CustomerCodes.Where(x => x.CustomerCode.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithCustomerCode.Select(x => x.CustomerCode.ToLower()).ToList();
                        var EndWithCustomerCode = CustomerCodes.Where(x => !startWithCodes.Contains(x.CustomerCode.ToLower()) && x.CustomerCode.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithCustomerCode.Select(x => x.CustomerName.ToLower()).ToList();
                        var ContainsCustomerCode = CustomerCodes.Where(x => !startWithCodes.Contains(x.CustomerCode.ToLower()) && !endWithCodes.Contains(x.CustomerCode.ToLower()));

                        Filterdata.AddRange(StartWithCustomerCode);
                        Filterdata.AddRange(ContainsCustomerCode);
                        Filterdata.AddRange(EndWithCustomerCode);
                        Filterdata.ForEach(s => s.Type = "Code");
                        this._cacheManager.Set($"AllCustomerSetupByCode_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
                if (colName == "name")
                {
                    if (this._cacheManager.IsSet($"AllCustomerSetupByName_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Customers>>($"AllCustomerSetupByName_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterCustomer = this._FormTemplateRepo.GetAllCustomerSetup(filter);
                        var CustomerNames = AllFilterCustomer.Where(x => x.CustomerName.ToLower().Contains(filter));
                        var StartWithCustomerName = CustomerNames.Where(x => x.CustomerName.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithCustomerName.Select(x => x.CustomerName.ToLower()).ToList();
                        var EndWithCustomerName = CustomerNames.Where(x => !startWithNames.Contains(x.CustomerName.ToLower()) && x.CustomerName.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithCustomerName.Select(x => x.CustomerName.ToLower()).ToList();
                        var ContainsCustomerName = CustomerNames.Where(x => !startWithNames.Contains(x.CustomerName.ToLower()) && !endWithNames.Contains(x.CustomerName.ToLower()));


                        StartWithCustomerName.ForEach(s => s.Type = "Name");
                        EndWithCustomerName.ToList().ForEach(s => s.Type = "Name");
                        ContainsCustomerName.ToList().ForEach(s => s.Type = "Name");

                        Filterdata.AddRange(StartWithCustomerName);
                        Filterdata.AddRange(ContainsCustomerName);
                        Filterdata.AddRange(EndWithCustomerName);
                        this._cacheManager.Set($"AllCustomerSetupByName_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
                if (colName == "address")
                {
                    if (this._cacheManager.IsSet($"AllCustomerSetupByAddress_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Customers>>($"AllCustomerSetupByAddress_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterAddress = this._FormTemplateRepo.GetAllCustomerSetup(filter);
                        var Address = AllFilterAddress.Where(x => x.REGD_OFFICE_EADDRESS.ToLower().Contains(filter));
                        var StartWithAddressName = Address.Where(x => x.REGD_OFFICE_EADDRESS.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithAddress = StartWithAddressName.Select(x => x.REGD_OFFICE_EADDRESS.ToLower()).ToList();
                        var EndWithAddressName = Address.Where(x => !startWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()) && x.REGD_OFFICE_EADDRESS.ToLower().EndsWith(filter.Trim()));
                        var endWithAddress = EndWithAddressName.Select(x => x.REGD_OFFICE_EADDRESS.ToLower()).ToList();
                        var ContainsAddressName = Address.Where(x => !startWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()) && !endWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()));
                        StartWithAddressName.ForEach(s => s.Type = "Addr");
                        EndWithAddressName.ToList().ForEach(s => s.Type = "Addr");
                        ContainsAddressName.ToList().ForEach(s => s.Type = "Addr");
                        Filterdata.AddRange(StartWithAddressName);
                        Filterdata.AddRange(ContainsAddressName);
                        Filterdata.AddRange(EndWithAddressName);
                        this._cacheManager.Set($"AllCustomerSetupByAddress_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
                if (colName == "phoneno")
                {
                    if (this._cacheManager.IsSet($"AllCustomerSetupByPhoneno_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Customers>>($"AllCustomerSetupByPhoneno_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterPhoneNo = this._FormTemplateRepo.GetAllCustomerSetup(filter);
                        var PhoneNoNames = AllFilterPhoneNo.Where(x => x.TEL_MOBILE_NO1.ToLower().Contains(filter));
                        var StartWithPhoneNoName = PhoneNoNames.Where(x => x.TEL_MOBILE_NO1.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithPhoneNoName.Select(x => x.TEL_MOBILE_NO1.ToLower()).ToList();
                        var EndWithPhoneNoName = PhoneNoNames.Where(x => !startWithNames.Contains(x.TEL_MOBILE_NO1.ToLower()) && x.TEL_MOBILE_NO1.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithPhoneNoName.Select(x => x.TEL_MOBILE_NO1.ToLower()).ToList();
                        var ContainsPhoneNoName = PhoneNoNames.Where(x => !startWithNames.Contains(x.TEL_MOBILE_NO1.ToLower()) && !endWithNames.Contains(x.TEL_MOBILE_NO1.ToLower()));
                        StartWithPhoneNoName.ForEach(s => s.Type = "Ph");
                        EndWithPhoneNoName.ToList().ForEach(s => s.Type = "Ph");
                        ContainsPhoneNoName.ToList().ForEach(s => s.Type = "Ph");
                        Filterdata.AddRange(StartWithPhoneNoName);
                        Filterdata.AddRange(ContainsPhoneNoName);
                        Filterdata.AddRange(EndWithPhoneNoName);
                        this._cacheManager.Set($"AllCustomerSetupByPhoneno_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
            }
            else
            {
                if (this._cacheManager.IsSet($"AllFilterCustomer_{userid}_{company_code}_{branch_code}_{filter}"))
                {
                    var data = _cacheManager.Get<List<Customers>>($"AllFilterCustomer_{userid}_{company_code}_{branch_code}_{filter}");
                    Filterdata = data;
                }
                else
                {
                    var AllFilterCustomer = this._FormTemplateRepo.GetAllCustomerSetup(filter);
                    if (AllFilterCustomer.Count >= 1)
                    {
                        var CustomerCodes = AllFilterCustomer.Where(x => x.CustomerCode.ToLower().Contains(filter));
                        var StartWithCustomerCode = CustomerCodes.Where(x => x.CustomerCode.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithCustomerCode.Select(x => x.CustomerCode.ToLower()).ToList();
                        var EndWithCustomerCode = CustomerCodes.Where(x => !startWithCodes.Contains(x.CustomerCode.ToLower()) && x.CustomerCode.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithCustomerCode.Select(x => x.CustomerName.ToLower()).ToList();
                        var ContainsCustomerCode = CustomerCodes.Where(x => !startWithCodes.Contains(x.CustomerCode.ToLower()) && !endWithCodes.Contains(x.CustomerCode.ToLower()));
                        Filterdata.AddRange(StartWithCustomerCode);
                        Filterdata.AddRange(ContainsCustomerCode);
                        Filterdata.AddRange(EndWithCustomerCode);
                        Filterdata.ForEach(s => s.Type = "Code");
                        var Removedata = AllFilterCustomer.RemoveAll(x => x.CustomerCode.ToLower().Contains(filter));
                        var CustomerNames = AllFilterCustomer.Where(x => x.CustomerName.ToLower().Contains(filter));
                        var StartWithCustomerName = CustomerNames.Where(x => x.CustomerName.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithCustomerName.Select(x => x.CustomerName.ToLower()).ToList();
                        var EndWithCustomerName = CustomerNames.Where(x => !startWithNames.Contains(x.CustomerName.ToLower()) && x.CustomerName.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithCustomerName.Select(x => x.CustomerName.ToLower()).ToList();
                        var ContainsCustomerName = CustomerNames.Where(x => !startWithNames.Contains(x.CustomerName.ToLower()) && !endWithNames.Contains(x.CustomerName.ToLower()));
                        StartWithCustomerName.ForEach(s => s.Type = "Name");
                        EndWithCustomerName.ToList().ForEach(s => s.Type = "Name");
                        ContainsCustomerName.ToList().ForEach(s => s.Type = "Name");
                        Filterdata.AddRange(StartWithCustomerName);
                        Filterdata.AddRange(ContainsCustomerName);
                        Filterdata.AddRange(EndWithCustomerName);
                        AllFilterCustomer.RemoveAll(x => x.CustomerName.ToLower().Contains(filter));
                        var Addresses = AllFilterCustomer.Where(x => x.REGD_OFFICE_EADDRESS.ToLower().Contains(filter));
                        var StartWithAddress = Addresses.Where(x => x.REGD_OFFICE_EADDRESS.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithAddress = StartWithAddress.Select(x => x.REGD_OFFICE_EADDRESS.ToLower()).ToList();
                        var EndWithAddress = Addresses.Where(x => !startWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()) && x.REGD_OFFICE_EADDRESS.ToLower().EndsWith(filter.Trim()));
                        var endWithAddress = EndWithAddress.Select(x => x.REGD_OFFICE_EADDRESS.ToLower()).ToList();
                        var ContainsAddress = Addresses.Where(x => !startWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()) && !endWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()));
                        StartWithAddress.ForEach(s => s.Type = "Addr");
                        EndWithAddress.ToList().ForEach(s => s.Type = "Addr");
                        ContainsAddress.ToList().ForEach(s => s.Type = "Addr");
                        Filterdata.AddRange(StartWithAddress);
                        Filterdata.AddRange(ContainsAddress);
                        Filterdata.AddRange(EndWithAddress);
                        AllFilterCustomer.RemoveAll(x => x.REGD_OFFICE_EADDRESS.ToLower().Contains(filter));
                        var PhoneNoNames = AllFilterCustomer.Where(x => x.TEL_MOBILE_NO1.ToLower().Contains(filter));
                        var StartWithPhoneNoName = PhoneNoNames.Where(x => x.TEL_MOBILE_NO1.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithPhone = StartWithPhoneNoName.Select(x => x.TEL_MOBILE_NO1.ToLower()).ToList();
                        var EndWithPhoneNoName = PhoneNoNames.Where(x => !startWithPhone.Contains(x.TEL_MOBILE_NO1.ToLower()) && x.TEL_MOBILE_NO1.ToLower().EndsWith(filter.Trim()));
                        var endWithPhone = EndWithPhoneNoName.Select(x => x.TEL_MOBILE_NO1.ToLower()).ToList();
                        var ContainsPhoneNoName = PhoneNoNames.Where(x => !startWithPhone.Contains(x.TEL_MOBILE_NO1.ToLower()) && !endWithPhone.Contains(x.TEL_MOBILE_NO1.ToLower()));
                        StartWithPhoneNoName.ForEach(s => s.Type = "Ph");
                        EndWithPhoneNoName.ToList().ForEach(s => s.Type = "Ph");
                        ContainsPhoneNoName.ToList().ForEach(s => s.Type = "Ph");
                        Filterdata.AddRange(StartWithPhoneNoName);
                        Filterdata.AddRange(ContainsPhoneNoName);
                        Filterdata.AddRange(EndWithPhoneNoName);
                        this._cacheManager.Set($"AllFilterCustomer_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                        return Filterdata;
                    }
                    return AllFilterCustomer;
                }
            }
            return Filterdata;
        }
        [HttpGet]
        public List<Suppliers> GetAllSupplierListByFilter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var Filterdata = new List<Suppliers>();
            var ShowAdvanceAutoComplete = false;
            var setting = _settingService.LoadSetting<WebPrefrenceSetting>(Constants.WebPrefranceSetting);
            if (setting != null)
                bool.TryParse(setting.ShowAdvanceAutoComplete, out ShowAdvanceAutoComplete);
            if (ShowAdvanceAutoComplete == false)
            {
                if (this._cacheManager.IsSet($"AllFilterSupplier_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}"))
                {
                    var data = _cacheManager.Get<List<Suppliers>>($"AllFilterSupplier_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}");
                    Filterdata = data;
                    return Filterdata;
                }
                else
                {
                    var AllFilterSupplier = this._FormTemplateRepo.getALLSupplierListByFlter(filter);
                    this._cacheManager.Set($"AllFilterSupplier_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}", AllFilterSupplier, 20);
                    return AllFilterSupplier;
                }

            }
            if (filter == "!@$")
                return Filterdata;
            if (filter == null)
                return this._FormTemplateRepo.getALLSupplierListByFlter(filter);
            if (filter.Contains("#"))
            {
                var colName = filter.Split('#')[0].ToString().ToLower();
                filter = filter.Split('#')[1].ToString().ToLower();
                if (colName == "code")
                {
                    if (this._cacheManager.IsSet($"AllSupplierSetupByCode_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Suppliers>>($"AllSupplierSetupByCode_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterSupplier = this._FormTemplateRepo.getALLSupplierListByFlter(filter);
                        var SupplierCodes = AllFilterSupplier.Where(x => x.SUPPLIER_CODE.ToLower().Contains(filter));
                        var StartWithSupplierCode = SupplierCodes.Where(x => x.SUPPLIER_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithSupplierCode.Select(x => x.SUPPLIER_CODE.ToLower()).ToList();
                        var EndWithSupplierCode = SupplierCodes.Where(x => !startWithCodes.Contains(x.SUPPLIER_CODE.ToLower()) && x.SUPPLIER_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithSupplierCode.Select(x => x.SUPPLIER_CODE.ToLower()).ToList();
                        var ContainsSupplierCode = SupplierCodes.Where(x => !startWithCodes.Contains(x.SUPPLIER_CODE.ToLower()) && !endWithCodes.Contains(x.SUPPLIER_CODE.ToLower()));
                        Filterdata.AddRange(StartWithSupplierCode);
                        Filterdata.AddRange(ContainsSupplierCode);
                        Filterdata.AddRange(EndWithSupplierCode);
                        Filterdata.ForEach(s => s.Type = "SupplierCode");
                        this._cacheManager.Set($"AllSupplierSetupByCode_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
                if (colName == "name")
                {
                    if (this._cacheManager.IsSet($"AllSupplierSetupByName_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Suppliers>>($"AllSupplierSetupByName_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterSupplier = this._FormTemplateRepo.getALLSupplierListByFlter(filter);
                        var SupplierNames = AllFilterSupplier.Where(x => x.SUPPLIER_EDESC.ToLower().Contains(filter));
                        var StartWithSupplierName = SupplierNames.Where(x => x.SUPPLIER_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithSupplierName.Select(x => x.SUPPLIER_EDESC.ToLower()).ToList();
                        var EndWithSupplierName = SupplierNames.Where(x => !startWithNames.Contains(x.SUPPLIER_EDESC.ToLower()) && x.SUPPLIER_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithSupplierName.Select(x => x.SUPPLIER_EDESC.ToLower()).ToList();
                        var ContainsSupplierName = SupplierNames.Where(x => !startWithNames.Contains(x.SUPPLIER_EDESC.ToLower()) && !endWithNames.Contains(x.SUPPLIER_EDESC.ToLower()));
                        StartWithSupplierName.ForEach(s => s.Type = "SupplierName");
                        EndWithSupplierName.ToList().ForEach(s => s.Type = "SupplierName");
                        ContainsSupplierName.ToList().ForEach(s => s.Type = "SupplierName");
                        Filterdata.AddRange(StartWithSupplierName);
                        Filterdata.AddRange(ContainsSupplierName);
                        Filterdata.AddRange(EndWithSupplierName);
                        this._cacheManager.Set($"AllSupplierSetupByName_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
                if (colName == "address")
                {
                    if (this._cacheManager.IsSet($"AllSupplierSetupByAddress_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Suppliers>>($"AllSupplierSetupByAddress_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterAddress = this._FormTemplateRepo.getALLSupplierListByFlter(filter);
                        var Address = AllFilterAddress.Where(x => x.REGD_OFFICE_EADDRESS.ToLower().Contains(filter));
                        var StartWithAddressName = Address.Where(x => x.REGD_OFFICE_EADDRESS.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithAddress = StartWithAddressName.Select(x => x.REGD_OFFICE_EADDRESS.ToLower()).ToList();
                        var EndWithAddressName = Address.Where(x => !startWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()) && x.REGD_OFFICE_EADDRESS.ToLower().EndsWith(filter.Trim()));
                        var endWithAddress = EndWithAddressName.Select(x => x.REGD_OFFICE_EADDRESS.ToLower()).ToList();
                        var ContainsAddressName = Address.Where(x => !startWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()) && !endWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()));
                        StartWithAddressName.ForEach(s => s.Type = "Address");
                        EndWithAddressName.ToList().ForEach(s => s.Type = "Address");
                        ContainsAddressName.ToList().ForEach(s => s.Type = "Address");
                        Filterdata.AddRange(StartWithAddressName);
                        Filterdata.AddRange(ContainsAddressName);
                        Filterdata.AddRange(EndWithAddressName);
                        this._cacheManager.Set($"AllSupplierSetupByAddress_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
                if (colName == "phoneno")
                {
                    if (this._cacheManager.IsSet($"AllSupplierSetupByPhoneno_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Suppliers>>($"AllSupplierSetupByPhoneno_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterPhoneNo = this._FormTemplateRepo.getALLSupplierListByFlter(filter);
                        var PhoneNoNames = AllFilterPhoneNo.Where(x => x.TEL_MOBILE_NO1.ToLower().Contains(filter));
                        var StartWithPhoneNoName = PhoneNoNames.Where(x => x.TEL_MOBILE_NO1.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithPhoneNoName.Select(x => x.TEL_MOBILE_NO1.ToLower()).ToList();
                        var EndWithPhoneNoName = PhoneNoNames.Where(x => !startWithNames.Contains(x.TEL_MOBILE_NO1.ToLower()) && x.TEL_MOBILE_NO1.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithPhoneNoName.Select(x => x.TEL_MOBILE_NO1.ToLower()).ToList();
                        var ContainsPhoneNoName = PhoneNoNames.Where(x => !startWithNames.Contains(x.TEL_MOBILE_NO1.ToLower()) && !endWithNames.Contains(x.TEL_MOBILE_NO1.ToLower()));
                        StartWithPhoneNoName.ForEach(s => s.Type = "PhoneNo");
                        EndWithPhoneNoName.ToList().ForEach(s => s.Type = "PhoneNo");
                        ContainsPhoneNoName.ToList().ForEach(s => s.Type = "PhoneNo");
                        Filterdata.AddRange(StartWithPhoneNoName);
                        Filterdata.AddRange(ContainsPhoneNoName);
                        Filterdata.AddRange(EndWithPhoneNoName);
                        this._cacheManager.Set($"AllSupplierSetupByPhoneno_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
            }
            else
            {
                if (this._cacheManager.IsSet($"AllFilterSupplier_{userid}_{company_code}_{branch_code}_{filter}"))
                {
                    var data = _cacheManager.Get<List<Suppliers>>($"AllFilterSupplier_{userid}_{company_code}_{branch_code}_{filter}");
                    Filterdata = data;
                }
                else
                {
                    var AllFilterSupplier = this._FormTemplateRepo.getALLSupplierListByFlter(filter);
                    if (filter == null)
                        return AllFilterSupplier;
                    if (AllFilterSupplier.Count >= 1)
                    {
                        var SupplierCodes = AllFilterSupplier.Where(x => x.SUPPLIER_CODE.ToLower().Contains(filter));
                        var StartWithSupplierCode = SupplierCodes.Where(x => x.SUPPLIER_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithSupplierCode.Select(x => x.SUPPLIER_CODE.ToLower()).ToList();
                        var EndWithSupplierCode = SupplierCodes.Where(x => !startWithCodes.Contains(x.SUPPLIER_CODE.ToLower()) && x.SUPPLIER_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithSupplierCode.Select(x => x.SUPPLIER_CODE.ToLower()).ToList();
                        var ContainsSupplierCode = SupplierCodes.Where(x => !startWithCodes.Contains(x.SUPPLIER_CODE.ToLower()) && !endWithCodes.Contains(x.SUPPLIER_CODE.ToLower()));
                        Filterdata.AddRange(StartWithSupplierCode);
                        Filterdata.AddRange(ContainsSupplierCode);
                        Filterdata.AddRange(EndWithSupplierCode);
                        Filterdata.ForEach(s => s.Type = "SupplierCode");
                        AllFilterSupplier.RemoveAll(x => x.SUPPLIER_CODE.Contains(filter));
                        var SupplierNames = AllFilterSupplier.Where(x => x.SUPPLIER_EDESC.ToLower().Contains(filter));
                        var StartWithSupplierName = SupplierNames.Where(x => x.SUPPLIER_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithSupplierName.Select(x => x.SUPPLIER_EDESC.ToLower()).ToList();
                        var EndWithSupplierName = SupplierNames.Where(x => !startWithNames.Contains(x.SUPPLIER_EDESC.ToLower()) && x.SUPPLIER_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithSupplierName.Select(x => x.SUPPLIER_EDESC.ToLower()).ToList();
                        var ContainsSupplierName = SupplierNames.Where(x => !startWithNames.Contains(x.SUPPLIER_EDESC.ToLower()) && !endWithNames.Contains(x.SUPPLIER_EDESC.ToLower()));
                        StartWithSupplierName.ForEach(s => s.Type = "SupplierName");
                        EndWithSupplierName.ToList().ForEach(s => s.Type = "SupplierName");
                        ContainsSupplierName.ToList().ForEach(s => s.Type = "SupplierName");
                        Filterdata.AddRange(StartWithSupplierName);
                        Filterdata.AddRange(ContainsSupplierName);
                        Filterdata.AddRange(EndWithSupplierName);
                        AllFilterSupplier.RemoveAll(x => x.SUPPLIER_EDESC.ToLower().Contains(filter));
                        var Address = AllFilterSupplier.Where(x => x.REGD_OFFICE_EADDRESS.ToLower().Contains(filter));
                        var StartWithAddressName = Address.Where(x => x.REGD_OFFICE_EADDRESS.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithAddress = StartWithAddressName.Select(x => x.REGD_OFFICE_EADDRESS.ToLower()).ToList();
                        var EndWithAddressName = Address.Where(x => !startWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()) && x.REGD_OFFICE_EADDRESS.ToLower().EndsWith(filter.Trim()));
                        var endWithAddress = EndWithAddressName.Select(x => x.REGD_OFFICE_EADDRESS.ToLower()).ToList();
                        var ContainsAddressName = Address.Where(x => !startWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()) && !endWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()));
                        StartWithAddressName.ForEach(s => s.Type = "Address");
                        EndWithAddressName.ToList().ForEach(s => s.Type = "Address");
                        ContainsAddressName.ToList().ForEach(s => s.Type = "Address");
                        Filterdata.AddRange(StartWithAddressName);
                        Filterdata.AddRange(ContainsAddressName);
                        Filterdata.AddRange(EndWithAddressName);
                        AllFilterSupplier.RemoveAll(x => x.REGD_OFFICE_EADDRESS.ToLower().Contains(filter));
                        var PhoneNoNames = AllFilterSupplier.Where(x => x.TEL_MOBILE_NO1.ToLower().Contains(filter));
                        var StartWithPhoneNoName = PhoneNoNames.Where(x => x.TEL_MOBILE_NO1.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithPhone = StartWithPhoneNoName.Select(x => x.TEL_MOBILE_NO1.ToLower()).ToList();
                        var EndWithPhoneNoName = PhoneNoNames.Where(x => !startWithPhone.Contains(x.TEL_MOBILE_NO1.ToLower()) && x.TEL_MOBILE_NO1.ToLower().EndsWith(filter.Trim()));
                        var endWithPhone = EndWithPhoneNoName.Select(x => x.TEL_MOBILE_NO1.ToLower()).ToList();
                        var ContainsPhoneNoName = PhoneNoNames.Where(x => !startWithPhone.Contains(x.TEL_MOBILE_NO1.ToLower()) && !endWithPhone.Contains(x.TEL_MOBILE_NO1.ToLower()));
                        StartWithPhoneNoName.ForEach(s => s.Type = "PhoneNo");
                        EndWithPhoneNoName.ToList().ForEach(s => s.Type = "PhoneNo");
                        ContainsPhoneNoName.ToList().ForEach(s => s.Type = "PhoneNo");
                        Filterdata.AddRange(StartWithPhoneNoName);
                        Filterdata.AddRange(ContainsPhoneNoName);
                        Filterdata.AddRange(EndWithPhoneNoName);
                        this._cacheManager.Set($"AllFilterSupplier_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                        return Filterdata;
                    }
                    return AllFilterSupplier;
                }
            }
            return Filterdata;
        }
        [HttpGet]
        public List<Suppliers> GetAllSupplierListByFilterVAT(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var Filterdata = new List<Suppliers>();
            var ShowAdvanceAutoComplete = false;
            var setting = _settingService.LoadSetting<WebPrefrenceSetting>(Constants.WebPrefranceSetting);
            if (setting != null)
                bool.TryParse(setting.ShowAdvanceAutoComplete, out ShowAdvanceAutoComplete);
            if (ShowAdvanceAutoComplete == false)
            {
                if (this._cacheManager.IsSet($"AllFilterSupplier_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}"))
                {
                    var data = _cacheManager.Get<List<Suppliers>>($"AllFilterSupplier_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}");
                    Filterdata = data;
                    return Filterdata;
                }
                else
                {
                    var AllFilterSupplier = this._FormTemplateRepo.getALLSupplierListByFlter(filter);
                    this._cacheManager.Set($"AllFilterSupplier_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}", Filterdata, 20);
                    return AllFilterSupplier;
                }

            }
            if (filter == "!@$")
                return Filterdata;
            if (filter == null)
                return this._FormTemplateRepo.getALLSupplierListByFlter(filter);
            if (filter.Contains("#"))
            {
                var colName = filter.Split('#')[0].ToString().ToLower();
                filter = filter.Split('#')[1].ToString().ToLower();
                if (colName == "code")
                {
                    if (this._cacheManager.IsSet($"AllSupplierSetupByCode_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Suppliers>>($"AllSupplierSetupByCode_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterSupplier = this._FormTemplateRepo.getALLSupplierListByFlter(filter);
                        var SupplierCodes = AllFilterSupplier.Where(x => x.SUPPLIER_CODE.ToLower().Contains(filter));
                        var StartWithSupplierCode = SupplierCodes.Where(x => x.SUPPLIER_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithSupplierCode.Select(x => x.SUPPLIER_CODE.ToLower()).ToList();
                        var EndWithSupplierCode = SupplierCodes.Where(x => !startWithCodes.Contains(x.SUPPLIER_CODE.ToLower()) && x.SUPPLIER_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithSupplierCode.Select(x => x.SUPPLIER_CODE.ToLower()).ToList();
                        var ContainsSupplierCode = SupplierCodes.Where(x => !startWithCodes.Contains(x.SUPPLIER_CODE.ToLower()) && !endWithCodes.Contains(x.SUPPLIER_CODE.ToLower()));
                        Filterdata.AddRange(StartWithSupplierCode);
                        Filterdata.AddRange(ContainsSupplierCode);
                        Filterdata.AddRange(EndWithSupplierCode);
                        Filterdata.ForEach(s => s.Type = "SupplierCode");
                        this._cacheManager.Set($"AllSupplierSetupByCode_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
                if (colName == "name")
                {
                    if (this._cacheManager.IsSet($"AllSupplierSetupByName_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Suppliers>>($"AllSupplierSetupByName_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterSupplier = this._FormTemplateRepo.getALLSupplierListByFlter(filter);
                        var SupplierNames = AllFilterSupplier.Where(x => x.SUPPLIER_EDESC.ToLower().Contains(filter));
                        var StartWithSupplierName = SupplierNames.Where(x => x.SUPPLIER_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithSupplierName.Select(x => x.SUPPLIER_EDESC.ToLower()).ToList();
                        var EndWithSupplierName = SupplierNames.Where(x => !startWithNames.Contains(x.SUPPLIER_EDESC.ToLower()) && x.SUPPLIER_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithSupplierName.Select(x => x.SUPPLIER_EDESC.ToLower()).ToList();
                        var ContainsSupplierName = SupplierNames.Where(x => !startWithNames.Contains(x.SUPPLIER_EDESC.ToLower()) && !endWithNames.Contains(x.SUPPLIER_EDESC.ToLower()));
                        StartWithSupplierName.ForEach(s => s.Type = "SupplierName");
                        EndWithSupplierName.ToList().ForEach(s => s.Type = "SupplierName");
                        ContainsSupplierName.ToList().ForEach(s => s.Type = "SupplierName");
                        Filterdata.AddRange(StartWithSupplierName);
                        Filterdata.AddRange(ContainsSupplierName);
                        Filterdata.AddRange(EndWithSupplierName);
                        this._cacheManager.Set($"AllSupplierSetupByName_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
                if (colName == "address")
                {
                    if (this._cacheManager.IsSet($"AllSupplierSetupByAddress_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Suppliers>>($"AllSupplierSetupByAddress_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterAddress = this._FormTemplateRepo.getALLSupplierListByFlter(filter);
                        var Address = AllFilterAddress.Where(x => x.REGD_OFFICE_EADDRESS.ToLower().Contains(filter));
                        var StartWithAddressName = Address.Where(x => x.REGD_OFFICE_EADDRESS.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithAddress = StartWithAddressName.Select(x => x.REGD_OFFICE_EADDRESS.ToLower()).ToList();
                        var EndWithAddressName = Address.Where(x => !startWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()) && x.REGD_OFFICE_EADDRESS.ToLower().EndsWith(filter.Trim()));
                        var endWithAddress = EndWithAddressName.Select(x => x.REGD_OFFICE_EADDRESS.ToLower()).ToList();
                        var ContainsAddressName = Address.Where(x => !startWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()) && !endWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()));
                        StartWithAddressName.ForEach(s => s.Type = "Address");
                        EndWithAddressName.ToList().ForEach(s => s.Type = "Address");
                        ContainsAddressName.ToList().ForEach(s => s.Type = "Address");
                        Filterdata.AddRange(StartWithAddressName);
                        Filterdata.AddRange(ContainsAddressName);
                        Filterdata.AddRange(EndWithAddressName);
                        this._cacheManager.Set($"AllSupplierSetupByAddress_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
                if (colName == "phoneno")
                {
                    if (this._cacheManager.IsSet($"AllSupplierSetupByPhoneno_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Suppliers>>($"AllSupplierSetupByPhoneno_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterPhoneNo = this._FormTemplateRepo.getALLSupplierListByFlter(filter);
                        var PhoneNoNames = AllFilterPhoneNo.Where(x => x.TEL_MOBILE_NO1.ToLower().Contains(filter));
                        var StartWithPhoneNoName = PhoneNoNames.Where(x => x.TEL_MOBILE_NO1.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithPhoneNoName.Select(x => x.TEL_MOBILE_NO1.ToLower()).ToList();
                        var EndWithPhoneNoName = PhoneNoNames.Where(x => !startWithNames.Contains(x.TEL_MOBILE_NO1.ToLower()) && x.TEL_MOBILE_NO1.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithPhoneNoName.Select(x => x.TEL_MOBILE_NO1.ToLower()).ToList();
                        var ContainsPhoneNoName = PhoneNoNames.Where(x => !startWithNames.Contains(x.TEL_MOBILE_NO1.ToLower()) && !endWithNames.Contains(x.TEL_MOBILE_NO1.ToLower()));
                        StartWithPhoneNoName.ForEach(s => s.Type = "PhoneNo");
                        EndWithPhoneNoName.ToList().ForEach(s => s.Type = "PhoneNo");
                        ContainsPhoneNoName.ToList().ForEach(s => s.Type = "PhoneNo");
                        Filterdata.AddRange(StartWithPhoneNoName);
                        Filterdata.AddRange(ContainsPhoneNoName);
                        Filterdata.AddRange(EndWithPhoneNoName);
                        this._cacheManager.Set($"AllSupplierSetupByPhoneno_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
            }
            else
            {
                if (this._cacheManager.IsSet($"AllFilterSupplier_{userid}_{company_code}_{branch_code}_{filter}"))
                {
                    var data = _cacheManager.Get<List<Suppliers>>($"AllFilterSupplier_{userid}_{company_code}_{branch_code}_{filter}");
                    Filterdata = data;
                }
                else
                {
                    var AllFilterSupplier = this._FormTemplateRepo.getALLSupplierListByFlter(filter);
                    if (filter == null)
                        return AllFilterSupplier;
                    if (AllFilterSupplier.Count >= 1)
                    {
                        var SupplierCodes = AllFilterSupplier.Where(x => x.SUPPLIER_CODE.ToLower().Contains(filter));
                        var StartWithSupplierCode = SupplierCodes.Where(x => x.SUPPLIER_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithSupplierCode.Select(x => x.SUPPLIER_CODE.ToLower()).ToList();
                        var EndWithSupplierCode = SupplierCodes.Where(x => !startWithCodes.Contains(x.SUPPLIER_CODE.ToLower()) && x.SUPPLIER_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithSupplierCode.Select(x => x.SUPPLIER_CODE.ToLower()).ToList();
                        var ContainsSupplierCode = SupplierCodes.Where(x => !startWithCodes.Contains(x.SUPPLIER_CODE.ToLower()) && !endWithCodes.Contains(x.SUPPLIER_CODE.ToLower()));
                        Filterdata.AddRange(StartWithSupplierCode);
                        Filterdata.AddRange(ContainsSupplierCode);
                        Filterdata.AddRange(EndWithSupplierCode);
                        Filterdata.ForEach(s => s.Type = "SupplierCode");
                        AllFilterSupplier.RemoveAll(x => x.SUPPLIER_CODE.Contains(filter));
                        var SupplierNames = AllFilterSupplier.Where(x => x.SUPPLIER_EDESC.ToLower().Contains(filter));
                        var StartWithSupplierName = SupplierNames.Where(x => x.SUPPLIER_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithSupplierName.Select(x => x.SUPPLIER_EDESC.ToLower()).ToList();
                        var EndWithSupplierName = SupplierNames.Where(x => !startWithNames.Contains(x.SUPPLIER_EDESC.ToLower()) && x.SUPPLIER_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithSupplierName.Select(x => x.SUPPLIER_EDESC.ToLower()).ToList();
                        var ContainsSupplierName = SupplierNames.Where(x => !startWithNames.Contains(x.SUPPLIER_EDESC.ToLower()) && !endWithNames.Contains(x.SUPPLIER_EDESC.ToLower()));
                        StartWithSupplierName.ForEach(s => s.Type = "SupplierName");
                        EndWithSupplierName.ToList().ForEach(s => s.Type = "SupplierName");
                        ContainsSupplierName.ToList().ForEach(s => s.Type = "SupplierName");
                        Filterdata.AddRange(StartWithSupplierName);
                        Filterdata.AddRange(ContainsSupplierName);
                        Filterdata.AddRange(EndWithSupplierName);
                        AllFilterSupplier.RemoveAll(x => x.SUPPLIER_EDESC.ToLower().Contains(filter));
                        var Address = AllFilterSupplier.Where(x => x.REGD_OFFICE_EADDRESS.ToLower().Contains(filter));
                        var StartWithAddressName = Address.Where(x => x.REGD_OFFICE_EADDRESS.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithAddress = StartWithAddressName.Select(x => x.REGD_OFFICE_EADDRESS.ToLower()).ToList();
                        var EndWithAddressName = Address.Where(x => !startWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()) && x.REGD_OFFICE_EADDRESS.ToLower().EndsWith(filter.Trim()));
                        var endWithAddress = EndWithAddressName.Select(x => x.REGD_OFFICE_EADDRESS.ToLower()).ToList();
                        var ContainsAddressName = Address.Where(x => !startWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()) && !endWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()));
                        StartWithAddressName.ForEach(s => s.Type = "Address");
                        EndWithAddressName.ToList().ForEach(s => s.Type = "Address");
                        ContainsAddressName.ToList().ForEach(s => s.Type = "Address");
                        Filterdata.AddRange(StartWithAddressName);
                        Filterdata.AddRange(ContainsAddressName);
                        Filterdata.AddRange(EndWithAddressName);
                        AllFilterSupplier.RemoveAll(x => x.REGD_OFFICE_EADDRESS.ToLower().Contains(filter));
                        var PhoneNoNames = AllFilterSupplier.Where(x => x.TEL_MOBILE_NO1.ToLower().Contains(filter));
                        var StartWithPhoneNoName = PhoneNoNames.Where(x => x.TEL_MOBILE_NO1.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithPhone = StartWithPhoneNoName.Select(x => x.TEL_MOBILE_NO1.ToLower()).ToList();
                        var EndWithPhoneNoName = PhoneNoNames.Where(x => !startWithPhone.Contains(x.TEL_MOBILE_NO1.ToLower()) && x.TEL_MOBILE_NO1.ToLower().EndsWith(filter.Trim()));
                        var endWithPhone = EndWithPhoneNoName.Select(x => x.TEL_MOBILE_NO1.ToLower()).ToList();
                        var ContainsPhoneNoName = PhoneNoNames.Where(x => !startWithPhone.Contains(x.TEL_MOBILE_NO1.ToLower()) && !endWithPhone.Contains(x.TEL_MOBILE_NO1.ToLower()));
                        StartWithPhoneNoName.ForEach(s => s.Type = "PhoneNo");
                        EndWithPhoneNoName.ToList().ForEach(s => s.Type = "PhoneNo");
                        ContainsPhoneNoName.ToList().ForEach(s => s.Type = "PhoneNo");
                        Filterdata.AddRange(StartWithPhoneNoName);
                        Filterdata.AddRange(ContainsPhoneNoName);
                        Filterdata.AddRange(EndWithPhoneNoName);
                        this._cacheManager.Set($"AllFilterSupplier_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                        return Filterdata;
                    }
                    return AllFilterSupplier;
                }
            }
            return Filterdata;
        }
        [HttpGet]
        public List<Suppliers> GetAllSupplierListByFilterTDS(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var Filterdata = new List<Suppliers>();
            var ShowAdvanceAutoComplete = false;
            var setting = _settingService.LoadSetting<WebPrefrenceSetting>(Constants.WebPrefranceSetting);
            if (setting != null)
                bool.TryParse(setting.ShowAdvanceAutoComplete, out ShowAdvanceAutoComplete);
            if (ShowAdvanceAutoComplete == false)
            {
                if (this._cacheManager.IsSet($"AllFilterSupplier_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}"))
                {
                    var data = _cacheManager.Get<List<Suppliers>>($"AllFilterSupplier_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}");
                    Filterdata = data;
                    return Filterdata;
                }
                else
                {
                    var AllFilterSupplier = this._FormTemplateRepo.getALLSupplierListByFlter(filter);
                    this._cacheManager.Set($"AllFilterSupplier_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}", Filterdata, 20);
                    return AllFilterSupplier;
                }

            }
            if (filter == "!@$")
                return Filterdata;
            if (filter == null)
                return this._FormTemplateRepo.getALLSupplierListByFlter(filter);
            if (filter.Contains("#"))
            {
                var colName = filter.Split('#')[0].ToString().ToLower();
                filter = filter.Split('#')[1].ToString().ToLower();
                if (colName == "code")
                {
                    if (this._cacheManager.IsSet($"AllSupplierSetupByCode_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Suppliers>>($"AllSupplierSetupByCode_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterSupplier = this._FormTemplateRepo.getALLSupplierListByFlter(filter);
                        var SupplierCodes = AllFilterSupplier.Where(x => x.SUPPLIER_CODE.ToLower().Contains(filter));
                        var StartWithSupplierCode = SupplierCodes.Where(x => x.SUPPLIER_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithSupplierCode.Select(x => x.SUPPLIER_CODE.ToLower()).ToList();
                        var EndWithSupplierCode = SupplierCodes.Where(x => !startWithCodes.Contains(x.SUPPLIER_CODE.ToLower()) && x.SUPPLIER_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithSupplierCode.Select(x => x.SUPPLIER_CODE.ToLower()).ToList();
                        var ContainsSupplierCode = SupplierCodes.Where(x => !startWithCodes.Contains(x.SUPPLIER_CODE.ToLower()) && !endWithCodes.Contains(x.SUPPLIER_CODE.ToLower()));
                        Filterdata.AddRange(StartWithSupplierCode);
                        Filterdata.AddRange(ContainsSupplierCode);
                        Filterdata.AddRange(EndWithSupplierCode);
                        Filterdata.ForEach(s => s.Type = "SupplierCode");
                        this._cacheManager.Set($"AllSupplierSetupByCode_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
                if (colName == "name")
                {
                    if (this._cacheManager.IsSet($"AllSupplierSetupByName_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Suppliers>>($"AllSupplierSetupByName_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterSupplier = this._FormTemplateRepo.getALLSupplierListByFlter(filter);
                        var SupplierNames = AllFilterSupplier.Where(x => x.SUPPLIER_EDESC.ToLower().Contains(filter));
                        var StartWithSupplierName = SupplierNames.Where(x => x.SUPPLIER_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithSupplierName.Select(x => x.SUPPLIER_EDESC.ToLower()).ToList();
                        var EndWithSupplierName = SupplierNames.Where(x => !startWithNames.Contains(x.SUPPLIER_EDESC.ToLower()) && x.SUPPLIER_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithSupplierName.Select(x => x.SUPPLIER_EDESC.ToLower()).ToList();
                        var ContainsSupplierName = SupplierNames.Where(x => !startWithNames.Contains(x.SUPPLIER_EDESC.ToLower()) && !endWithNames.Contains(x.SUPPLIER_EDESC.ToLower()));
                        StartWithSupplierName.ForEach(s => s.Type = "SupplierName");
                        EndWithSupplierName.ToList().ForEach(s => s.Type = "SupplierName");
                        ContainsSupplierName.ToList().ForEach(s => s.Type = "SupplierName");
                        Filterdata.AddRange(StartWithSupplierName);
                        Filterdata.AddRange(ContainsSupplierName);
                        Filterdata.AddRange(EndWithSupplierName);
                        this._cacheManager.Set($"AllSupplierSetupByName_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
                if (colName == "address")
                {
                    if (this._cacheManager.IsSet($"AllSupplierSetupByAddress_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Suppliers>>($"AllSupplierSetupByAddress_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterAddress = this._FormTemplateRepo.getALLSupplierListByFlter(filter);
                        var Address = AllFilterAddress.Where(x => x.REGD_OFFICE_EADDRESS.ToLower().Contains(filter));
                        var StartWithAddressName = Address.Where(x => x.REGD_OFFICE_EADDRESS.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithAddress = StartWithAddressName.Select(x => x.REGD_OFFICE_EADDRESS.ToLower()).ToList();
                        var EndWithAddressName = Address.Where(x => !startWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()) && x.REGD_OFFICE_EADDRESS.ToLower().EndsWith(filter.Trim()));
                        var endWithAddress = EndWithAddressName.Select(x => x.REGD_OFFICE_EADDRESS.ToLower()).ToList();
                        var ContainsAddressName = Address.Where(x => !startWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()) && !endWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()));
                        StartWithAddressName.ForEach(s => s.Type = "Address");
                        EndWithAddressName.ToList().ForEach(s => s.Type = "Address");
                        ContainsAddressName.ToList().ForEach(s => s.Type = "Address");
                        Filterdata.AddRange(StartWithAddressName);
                        Filterdata.AddRange(ContainsAddressName);
                        Filterdata.AddRange(EndWithAddressName);
                        this._cacheManager.Set($"AllSupplierSetupByAddress_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
                if (colName == "phoneno")
                {
                    if (this._cacheManager.IsSet($"AllSupplierSetupByPhoneno_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Suppliers>>($"AllSupplierSetupByPhoneno_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterPhoneNo = this._FormTemplateRepo.getALLSupplierListByFlter(filter);
                        var PhoneNoNames = AllFilterPhoneNo.Where(x => x.TEL_MOBILE_NO1.ToLower().Contains(filter));
                        var StartWithPhoneNoName = PhoneNoNames.Where(x => x.TEL_MOBILE_NO1.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithPhoneNoName.Select(x => x.TEL_MOBILE_NO1.ToLower()).ToList();
                        var EndWithPhoneNoName = PhoneNoNames.Where(x => !startWithNames.Contains(x.TEL_MOBILE_NO1.ToLower()) && x.TEL_MOBILE_NO1.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithPhoneNoName.Select(x => x.TEL_MOBILE_NO1.ToLower()).ToList();
                        var ContainsPhoneNoName = PhoneNoNames.Where(x => !startWithNames.Contains(x.TEL_MOBILE_NO1.ToLower()) && !endWithNames.Contains(x.TEL_MOBILE_NO1.ToLower()));
                        StartWithPhoneNoName.ForEach(s => s.Type = "PhoneNo");
                        EndWithPhoneNoName.ToList().ForEach(s => s.Type = "PhoneNo");
                        ContainsPhoneNoName.ToList().ForEach(s => s.Type = "PhoneNo");
                        Filterdata.AddRange(StartWithPhoneNoName);
                        Filterdata.AddRange(ContainsPhoneNoName);
                        Filterdata.AddRange(EndWithPhoneNoName);
                        this._cacheManager.Set($"AllSupplierSetupByPhoneno_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
            }
            else
            {
                if (this._cacheManager.IsSet($"AllFilterSupplier_{userid}_{company_code}_{branch_code}_{filter}"))
                {
                    var data = _cacheManager.Get<List<Suppliers>>($"AllFilterSupplier_{userid}_{company_code}_{branch_code}_{filter}");
                    Filterdata = data;
                }
                else
                {
                    var AllFilterSupplier = this._FormTemplateRepo.getALLSupplierListByFlter(filter);
                    if (filter == null)
                        return AllFilterSupplier;
                    if (AllFilterSupplier.Count >= 1)
                    {
                        var SupplierCodes = AllFilterSupplier.Where(x => x.SUPPLIER_CODE.ToLower().Contains(filter));
                        var StartWithSupplierCode = SupplierCodes.Where(x => x.SUPPLIER_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithSupplierCode.Select(x => x.SUPPLIER_CODE.ToLower()).ToList();
                        var EndWithSupplierCode = SupplierCodes.Where(x => !startWithCodes.Contains(x.SUPPLIER_CODE.ToLower()) && x.SUPPLIER_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithSupplierCode.Select(x => x.SUPPLIER_CODE.ToLower()).ToList();
                        var ContainsSupplierCode = SupplierCodes.Where(x => !startWithCodes.Contains(x.SUPPLIER_CODE.ToLower()) && !endWithCodes.Contains(x.SUPPLIER_CODE.ToLower()));
                        Filterdata.AddRange(StartWithSupplierCode);
                        Filterdata.AddRange(ContainsSupplierCode);
                        Filterdata.AddRange(EndWithSupplierCode);
                        Filterdata.ForEach(s => s.Type = "SupplierCode");
                        AllFilterSupplier.RemoveAll(x => x.SUPPLIER_CODE.Contains(filter));
                        var SupplierNames = AllFilterSupplier.Where(x => x.SUPPLIER_EDESC.ToLower().Contains(filter));
                        var StartWithSupplierName = SupplierNames.Where(x => x.SUPPLIER_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithSupplierName.Select(x => x.SUPPLIER_EDESC.ToLower()).ToList();
                        var EndWithSupplierName = SupplierNames.Where(x => !startWithNames.Contains(x.SUPPLIER_EDESC.ToLower()) && x.SUPPLIER_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithSupplierName.Select(x => x.SUPPLIER_EDESC.ToLower()).ToList();
                        var ContainsSupplierName = SupplierNames.Where(x => !startWithNames.Contains(x.SUPPLIER_EDESC.ToLower()) && !endWithNames.Contains(x.SUPPLIER_EDESC.ToLower()));
                        StartWithSupplierName.ForEach(s => s.Type = "SupplierName");
                        EndWithSupplierName.ToList().ForEach(s => s.Type = "SupplierName");
                        ContainsSupplierName.ToList().ForEach(s => s.Type = "SupplierName");
                        Filterdata.AddRange(StartWithSupplierName);
                        Filterdata.AddRange(ContainsSupplierName);
                        Filterdata.AddRange(EndWithSupplierName);
                        AllFilterSupplier.RemoveAll(x => x.SUPPLIER_EDESC.ToLower().Contains(filter));
                        var Address = AllFilterSupplier.Where(x => x.REGD_OFFICE_EADDRESS.ToLower().Contains(filter));
                        var StartWithAddressName = Address.Where(x => x.REGD_OFFICE_EADDRESS.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithAddress = StartWithAddressName.Select(x => x.REGD_OFFICE_EADDRESS.ToLower()).ToList();
                        var EndWithAddressName = Address.Where(x => !startWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()) && x.REGD_OFFICE_EADDRESS.ToLower().EndsWith(filter.Trim()));
                        var endWithAddress = EndWithAddressName.Select(x => x.REGD_OFFICE_EADDRESS.ToLower()).ToList();
                        var ContainsAddressName = Address.Where(x => !startWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()) && !endWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()));
                        StartWithAddressName.ForEach(s => s.Type = "Address");
                        EndWithAddressName.ToList().ForEach(s => s.Type = "Address");
                        ContainsAddressName.ToList().ForEach(s => s.Type = "Address");
                        Filterdata.AddRange(StartWithAddressName);
                        Filterdata.AddRange(ContainsAddressName);
                        Filterdata.AddRange(EndWithAddressName);
                        AllFilterSupplier.RemoveAll(x => x.REGD_OFFICE_EADDRESS.ToLower().Contains(filter));
                        var PhoneNoNames = AllFilterSupplier.Where(x => x.TEL_MOBILE_NO1.ToLower().Contains(filter));
                        var StartWithPhoneNoName = PhoneNoNames.Where(x => x.TEL_MOBILE_NO1.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithPhone = StartWithPhoneNoName.Select(x => x.TEL_MOBILE_NO1.ToLower()).ToList();
                        var EndWithPhoneNoName = PhoneNoNames.Where(x => !startWithPhone.Contains(x.TEL_MOBILE_NO1.ToLower()) && x.TEL_MOBILE_NO1.ToLower().EndsWith(filter.Trim()));
                        var endWithPhone = EndWithPhoneNoName.Select(x => x.TEL_MOBILE_NO1.ToLower()).ToList();
                        var ContainsPhoneNoName = PhoneNoNames.Where(x => !startWithPhone.Contains(x.TEL_MOBILE_NO1.ToLower()) && !endWithPhone.Contains(x.TEL_MOBILE_NO1.ToLower()));
                        StartWithPhoneNoName.ForEach(s => s.Type = "PhoneNo");
                        EndWithPhoneNoName.ToList().ForEach(s => s.Type = "PhoneNo");
                        ContainsPhoneNoName.ToList().ForEach(s => s.Type = "PhoneNo");
                        Filterdata.AddRange(StartWithPhoneNoName);
                        Filterdata.AddRange(ContainsPhoneNoName);
                        Filterdata.AddRange(EndWithPhoneNoName);
                        this._cacheManager.Set($"AllFilterSupplier_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                        return Filterdata;
                    }
                    return AllFilterSupplier;
                }
            }
            return Filterdata;
        }
        [HttpGet]
        public List<Customers> GetAllSupplierForReferenceByFilter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var Filterdata = new List<Customers>();
            var ShowAdvanceAutoComplete = false;
            var setting = _settingService.LoadSetting<WebPrefrenceSetting>(Constants.WebPrefranceSetting);
            if (setting != null)
                bool.TryParse(setting.ShowAdvanceAutoComplete, out ShowAdvanceAutoComplete);
            if (ShowAdvanceAutoComplete == false)
            {
                if (this._cacheManager.IsSet($"AllSupplierForReferenceByCode_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}"))
                {
                    var data = _cacheManager.Get<List<Customers>>($"AllSupplierForReferenceByCode_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}");
                    Filterdata = data;
                    return Filterdata;
                }
                else
                {
                    var AllFilterSupplier = this._FormTemplateRepo.getALLSupplierListByFlterForReference(filter);
                    this._cacheManager.Set($"AllSupplierForReferenceByCode_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}", Filterdata, 20);
                    return Filterdata;
                }
            }
            if (filter == "!@$")
                return Filterdata;
            if (filter == null)
                return this._FormTemplateRepo.getALLSupplierListByFlterForReference(filter);
            if (filter.Contains("#"))
            {
                var colName = filter.Split('#')[0].ToString().ToLower();
                filter = filter.Split('#')[1].ToString().ToLower();
                if (colName == "code")
                {
                    if (this._cacheManager.IsSet($"AllSupplierForReferenceByCode_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Customers>>($"AllSupplierForReferenceByCode_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterSupplier = this._FormTemplateRepo.getALLSupplierListByFlterForReference(filter);
                        var SupplierCodes = AllFilterSupplier.Where(x => x.CustomerCode.ToLower().Contains(filter));
                        var StartWithSupplierCode = SupplierCodes.Where(x => x.CustomerCode.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithSupplierCode.Select(x => x.CustomerCode.ToLower()).ToList();
                        var EndWithSupplierCode = SupplierCodes.Where(x => !startWithCodes.Contains(x.CustomerCode.ToLower()) && x.CustomerCode.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithSupplierCode.Select(x => x.CustomerCode.ToLower()).ToList();
                        var ContainsSupplierCode = SupplierCodes.Where(x => !startWithCodes.Contains(x.CustomerCode.ToLower()) && !endWithCodes.Contains(x.CustomerCode.ToLower()));

                        Filterdata.AddRange(StartWithSupplierCode);
                        Filterdata.AddRange(ContainsSupplierCode);
                        Filterdata.AddRange(EndWithSupplierCode);
                        Filterdata.ForEach(s => s.Type = "SupplierCode");
                        this._cacheManager.Set($"AllCustomerSetupByCode_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
                if (colName == "name")
                {
                    if (this._cacheManager.IsSet($"AllSupplierForReferenceByName_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Customers>>($"AllSupplierForReferenceByName_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterSupplier = this._FormTemplateRepo.getALLSupplierListByFlterForReference(filter);
                        var SupplierNames = AllFilterSupplier.Where(x => x.CustomerName.ToLower().Contains(filter));
                        var StartWithSupplierName = SupplierNames.Where(x => x.CustomerName.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithSupplierName.Select(x => x.CustomerName.ToLower()).ToList();
                        var EndWithSupplierName = SupplierNames.Where(x => !startWithNames.Contains(x.CustomerName.ToLower()) && x.CustomerName.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithSupplierName.Select(x => x.CustomerName.ToLower()).ToList();
                        var ContainsSupplierName = SupplierNames.Where(x => !startWithNames.Contains(x.CustomerName.ToLower()) && !endWithNames.Contains(x.CustomerName.ToLower()));
                        StartWithSupplierName.ForEach(s => s.Type = "SupplierName");
                        EndWithSupplierName.ToList().ForEach(s => s.Type = "SupplierName");
                        ContainsSupplierName.ToList().ForEach(s => s.Type = "SupplierName");
                        Filterdata.AddRange(StartWithSupplierName);
                        Filterdata.AddRange(ContainsSupplierName);
                        Filterdata.AddRange(EndWithSupplierName);
                        this._cacheManager.Set($"AllSupplierForReferenceByName_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
                if (colName == "address")
                {
                    if (this._cacheManager.IsSet($"AllSupplierForReferenceByName_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Customers>>($"AllSupplierForReferenceByName_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterAddress = this._FormTemplateRepo.getALLSupplierListByFlterForReference(filter);
                        var Address = AllFilterAddress.Where(x => x.REGD_OFFICE_EADDRESS.ToLower().Contains(filter));
                        var StartWithAddressName = Address.Where(x => x.REGD_OFFICE_EADDRESS.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithAddress = StartWithAddressName.Select(x => x.REGD_OFFICE_EADDRESS.ToLower()).ToList();
                        var EndWithAddressName = Address.Where(x => !startWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()) && x.REGD_OFFICE_EADDRESS.ToLower().EndsWith(filter.Trim()));
                        var endWithAddress = EndWithAddressName.Select(x => x.REGD_OFFICE_EADDRESS.ToLower()).ToList();
                        var ContainsAddressName = Address.Where(x => !startWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()) && !endWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()));
                        StartWithAddressName.ForEach(s => s.Type = "Address");
                        EndWithAddressName.ToList().ForEach(s => s.Type = "Address");
                        ContainsAddressName.ToList().ForEach(s => s.Type = "Address");
                        Filterdata.AddRange(StartWithAddressName);
                        Filterdata.AddRange(ContainsAddressName);
                        Filterdata.AddRange(EndWithAddressName);
                        this._cacheManager.Set($"AllSupplierForReferenceByaddress_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
                if (colName == "phoneno")
                {
                    if (this._cacheManager.IsSet($"AllSupplierForReferenceByphoneno_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Customers>>($"AllSupplierForReferenceByphoneno_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterPhoneNo = this._FormTemplateRepo.getALLSupplierListByFlterForReference(filter);
                        var PhoneNoNames = AllFilterPhoneNo.Where(x => x.TEL_MOBILE_NO1.ToLower().Contains(filter));
                        var StartWithPhoneNoName = PhoneNoNames.Where(x => x.TEL_MOBILE_NO1.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithPhoneNoName.Select(x => x.TEL_MOBILE_NO1.ToLower()).ToList();
                        var EndWithPhoneNoName = PhoneNoNames.Where(x => !startWithNames.Contains(x.TEL_MOBILE_NO1.ToLower()) && x.TEL_MOBILE_NO1.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithPhoneNoName.Select(x => x.TEL_MOBILE_NO1.ToLower()).ToList();
                        var ContainsPhoneNoName = PhoneNoNames.Where(x => !startWithNames.Contains(x.TEL_MOBILE_NO1.ToLower()) && !endWithNames.Contains(x.TEL_MOBILE_NO1.ToLower()));
                        StartWithPhoneNoName.ForEach(s => s.Type = "PhoneNo");
                        EndWithPhoneNoName.ToList().ForEach(s => s.Type = "PhoneNo");
                        ContainsPhoneNoName.ToList().ForEach(s => s.Type = "PhoneNo");
                        Filterdata.AddRange(StartWithPhoneNoName);
                        Filterdata.AddRange(ContainsPhoneNoName);
                        Filterdata.AddRange(EndWithPhoneNoName);
                        this._cacheManager.Set($"AllSupplierForReferenceByphoneno_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
            }
            else
            {
                if (this._cacheManager.IsSet($"AllFilterSupplierForReference_{userid}_{company_code}_{branch_code}_{filter}"))
                {
                    var data = _cacheManager.Get<List<Customers>>($"AllFilterSupplierForReference_{userid}_{company_code}_{branch_code}_{filter}");
                    Filterdata = data;
                }
                else
                {
                    var AllFilterSupplier = this._FormTemplateRepo.getALLSupplierListByFlterForReference(filter);
                    if (filter == null)
                        return AllFilterSupplier;
                    if (AllFilterSupplier.Count >= 1)
                    {
                        var SupplierCodes = AllFilterSupplier.Where(x => x.CustomerCode.ToLower().Contains(filter));
                        var StartWithSupplierCode = SupplierCodes.Where(x => x.CustomerCode.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithSupplierCode.Select(x => x.CustomerCode.ToLower()).ToList();
                        var EndWithSupplierCode = SupplierCodes.Where(x => !startWithCodes.Contains(x.CustomerCode.ToLower()) && x.CustomerCode.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithSupplierCode.Select(x => x.CustomerCode.ToLower()).ToList();
                        var ContainsSupplierCode = SupplierCodes.Where(x => !startWithCodes.Contains(x.CustomerCode.ToLower()) && !endWithCodes.Contains(x.CustomerCode.ToLower()));
                        Filterdata.AddRange(StartWithSupplierCode);
                        Filterdata.AddRange(ContainsSupplierCode);
                        Filterdata.AddRange(EndWithSupplierCode);
                        Filterdata.ForEach(s => s.Type = "SupplierCode");
                        AllFilterSupplier.RemoveAll(x => x.CustomerCode.Contains(filter));
                        var SupplierNames = AllFilterSupplier.Where(x => x.CustomerName.ToLower().Contains(filter));
                        var StartWithSupplierName = SupplierNames.Where(x => x.CustomerName.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithSupplierName.Select(x => x.CustomerName.ToLower()).ToList();
                        var EndWithSupplierName = SupplierNames.Where(x => !startWithNames.Contains(x.CustomerName.ToLower()) && x.CustomerName.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithSupplierName.Select(x => x.CustomerName.ToLower()).ToList();
                        var ContainsSupplierName = SupplierNames.Where(x => !startWithNames.Contains(x.CustomerName.ToLower()) && !endWithNames.Contains(x.CustomerName.ToLower()));
                        StartWithSupplierName.ForEach(s => s.Type = "SupplierName");
                        EndWithSupplierName.ToList().ForEach(s => s.Type = "SupplierName");
                        ContainsSupplierName.ToList().ForEach(s => s.Type = "SupplierName");
                        Filterdata.AddRange(StartWithSupplierName);
                        Filterdata.AddRange(ContainsSupplierName);
                        Filterdata.AddRange(EndWithSupplierName);
                        AllFilterSupplier.RemoveAll(x => x.CustomerName.ToLower().Contains(filter));
                        var Address = AllFilterSupplier.Where(x => x.REGD_OFFICE_EADDRESS.ToLower().Contains(filter));
                        var StartWithAddressName = Address.Where(x => x.REGD_OFFICE_EADDRESS.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithAddress = StartWithAddressName.Select(x => x.REGD_OFFICE_EADDRESS.ToLower()).ToList();
                        var EndWithAddressName = Address.Where(x => !startWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()) && x.REGD_OFFICE_EADDRESS.ToLower().EndsWith(filter.Trim()));
                        var endWithAddress = EndWithAddressName.Select(x => x.REGD_OFFICE_EADDRESS.ToLower()).ToList();
                        var ContainsAddressName = Address.Where(x => !startWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()) && !endWithAddress.Contains(x.REGD_OFFICE_EADDRESS.ToLower()));
                        StartWithAddressName.ForEach(s => s.Type = "Address");
                        EndWithAddressName.ToList().ForEach(s => s.Type = "Address");
                        ContainsAddressName.ToList().ForEach(s => s.Type = "Address");
                        Filterdata.AddRange(StartWithAddressName);
                        Filterdata.AddRange(ContainsAddressName);
                        Filterdata.AddRange(EndWithAddressName);
                        AllFilterSupplier.RemoveAll(x => x.REGD_OFFICE_EADDRESS.ToLower().Contains(filter));
                        var PhoneNoNames = AllFilterSupplier.Where(x => x.TEL_MOBILE_NO1.ToLower().Contains(filter));
                        var StartWithPhoneNoName = PhoneNoNames.Where(x => x.TEL_MOBILE_NO1.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithPhone = StartWithPhoneNoName.Select(x => x.TEL_MOBILE_NO1.ToLower()).ToList();
                        var EndWithPhoneNoName = PhoneNoNames.Where(x => !startWithPhone.Contains(x.TEL_MOBILE_NO1.ToLower()) && x.TEL_MOBILE_NO1.ToLower().EndsWith(filter.Trim()));
                        var endWithPhone = EndWithPhoneNoName.Select(x => x.TEL_MOBILE_NO1.ToLower()).ToList();
                        var ContainsPhoneNoName = PhoneNoNames.Where(x => !startWithPhone.Contains(x.TEL_MOBILE_NO1.ToLower()) && !endWithPhone.Contains(x.TEL_MOBILE_NO1.ToLower()));
                        StartWithPhoneNoName.ForEach(s => s.Type = "PhoneNo");
                        EndWithPhoneNoName.ToList().ForEach(s => s.Type = "PhoneNo");
                        ContainsPhoneNoName.ToList().ForEach(s => s.Type = "PhoneNo");
                        Filterdata.AddRange(StartWithPhoneNoName);
                        Filterdata.AddRange(ContainsPhoneNoName);
                        Filterdata.AddRange(EndWithPhoneNoName);
                        this._cacheManager.Set($"AllFilterSupplierForReference_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                        return Filterdata;
                    }
                    return AllFilterSupplier;
                }
            }
            return Filterdata;
        }
        [HttpGet]
        public List<IssueType> GetAllIssueTypeListByFilter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var Filterdata = new List<IssueType>();
            var ShowAdvanceAutoComplete = false;
            var setting = _settingService.LoadSetting<WebPrefrenceSetting>(Constants.WebPrefranceSetting);
            if (setting != null)
                bool.TryParse(setting.ShowAdvanceAutoComplete, out ShowAdvanceAutoComplete);
            if (ShowAdvanceAutoComplete == false)
            {
                var AllFilterIssues = this._FormTemplateRepo.getAllIssueTypeListByFilter(filter);
                this._cacheManager.Set($"IssueTypeListByCode_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}", AllFilterIssues, 20);
                return AllFilterIssues;
                //if (this._cacheManager.IsSet($"IssueTypeListByCode_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}"))
                //{
                //    var data = _cacheManager.Get<List<IssueType>>($"IssueTypeListByCode_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}");
                //    Filterdata = data;
                //    return Filterdata;
                //}
                //else
                //{

                //}
            }
            if (filter == "!@$")
                return Filterdata;
            if (filter == null)
                filter = "";
            if (filter.Contains("#"))
            {
                var colName = filter.Split('#')[0].ToString().ToLower();
                filter = filter.Split('#')[1].ToString().ToLower();
                if (colName == "code")
                {
                    if (this._cacheManager.IsSet($"IssueTypeListByCode_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<IssueType>>($"IssueTypeListByCode_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterIssueType = this._FormTemplateRepo.getAllIssueTypeListByFilter(filter);
                        var IssueTypeCodes = AllFilterIssueType.Where(x => x.ISSUE_TYPE_CODE.ToLower().Contains(filter));
                        var StartWithIssueTypeCode = IssueTypeCodes.Where(x => x.ISSUE_TYPE_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithIssueTypeCode.Select(x => x.ISSUE_TYPE_CODE.ToLower()).ToList();
                        var EndWithIssueTypeCode = IssueTypeCodes.Where(x => !startWithCodes.Contains(x.ISSUE_TYPE_CODE.ToLower()) && x.ISSUE_TYPE_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithIssueTypeCode.Select(x => x.ISSUE_TYPE_EDESC.ToLower()).ToList();
                        var ContainsIssueTypeCode = IssueTypeCodes.Where(x => !startWithCodes.Contains(x.ISSUE_TYPE_CODE.ToLower()) && !endWithCodes.Contains(x.ISSUE_TYPE_CODE.ToLower()));

                        Filterdata.AddRange(StartWithIssueTypeCode);
                        Filterdata.AddRange(ContainsIssueTypeCode);
                        Filterdata.AddRange(EndWithIssueTypeCode);
                        Filterdata.ForEach(s => s.Type = "IssueTypeCode");
                        this._cacheManager.Set($"IssueTypeListByCode_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }

                    return Filterdata;
                }
                if (colName == "name")
                {
                    if (this._cacheManager.IsSet($"IssueTypeListByName_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<IssueType>>($"IssueTypeListByName_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterIssueType = this._FormTemplateRepo.getAllIssueTypeListByFilter(filter);
                        var IssueTypeNames = AllFilterIssueType.Where(x => x.ISSUE_TYPE_EDESC.ToLower().Contains(filter));
                        var StartWithIssueTypeName = IssueTypeNames.Where(x => x.ISSUE_TYPE_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithIssueTypeName.Select(x => x.ISSUE_TYPE_EDESC.ToLower()).ToList();
                        var EndWithIssueTypeName = IssueTypeNames.Where(x => !startWithNames.Contains(x.ISSUE_TYPE_EDESC.ToLower()) && x.ISSUE_TYPE_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithIssueTypeName.Select(x => x.ISSUE_TYPE_EDESC.ToLower()).ToList();
                        var ContainsIssueTypeName = IssueTypeNames.Where(x => !startWithNames.Contains(x.ISSUE_TYPE_EDESC.ToLower()) && !endWithNames.Contains(x.ISSUE_TYPE_EDESC.ToLower()));
                        StartWithIssueTypeName.ForEach(s => s.Type = "IssueTypeName");
                        EndWithIssueTypeName.ToList().ForEach(s => s.Type = "IssueTypeName");
                        ContainsIssueTypeName.ToList().ForEach(s => s.Type = "IssueTypeName");
                        Filterdata.AddRange(StartWithIssueTypeName);
                        Filterdata.AddRange(ContainsIssueTypeName);
                        Filterdata.AddRange(EndWithIssueTypeName);
                        this._cacheManager.Set($"IssueTypeListByName_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
            }
            else
            {
                if (this._cacheManager.IsSet($"AllIssueTypeList_{userid}_{company_code}_{branch_code}_{filter}"))
                {
                    var data = _cacheManager.Get<List<IssueType>>($"AllIssueTypeList_{userid}_{company_code}_{branch_code}_{filter}");
                    Filterdata = data;
                }
                else
                {
                    var AllFilterIssueType = this._FormTemplateRepo.getAllIssueTypeListByFilter(filter);
                    if (AllFilterIssueType.Count >= 1)
                    {
                        var IssueTypeCodes = AllFilterIssueType.Where(x => x.ISSUE_TYPE_CODE.ToLower().Contains(filter));
                        var StartWithIssueTypeCode = IssueTypeCodes.Where(x => x.ISSUE_TYPE_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithIssueTypeCode.Select(x => x.ISSUE_TYPE_CODE.ToLower()).ToList();
                        var EndWithIssueTypeCode = IssueTypeCodes.Where(x => !startWithCodes.Contains(x.ISSUE_TYPE_CODE.ToLower()) && x.ISSUE_TYPE_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithIssueTypeCode.Select(x => x.ISSUE_TYPE_EDESC.ToLower()).ToList();
                        var ContainsIssueTypeCode = IssueTypeCodes.Where(x => !startWithCodes.Contains(x.ISSUE_TYPE_CODE.ToLower()) && !endWithCodes.Contains(x.ISSUE_TYPE_CODE.ToLower()));
                        Filterdata.AddRange(StartWithIssueTypeCode);
                        Filterdata.AddRange(ContainsIssueTypeCode);
                        Filterdata.AddRange(EndWithIssueTypeCode);
                        Filterdata.ForEach(s => s.Type = "IssueTypeCode");
                        var Removedata = AllFilterIssueType.RemoveAll(x => x.ISSUE_TYPE_CODE.ToLower().Contains(filter));
                        var IssueTypeNames = AllFilterIssueType.Where(x => x.ISSUE_TYPE_EDESC.ToLower().Contains(filter));
                        var StartWithIssueTypeName = IssueTypeNames.Where(x => x.ISSUE_TYPE_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithIssueTypeName.Select(x => x.ISSUE_TYPE_EDESC.ToLower()).ToList();
                        var EndWithIssueTypeName = IssueTypeNames.Where(x => !startWithNames.Contains(x.ISSUE_TYPE_EDESC.ToLower()) && x.ISSUE_TYPE_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithIssueTypeName.Select(x => x.ISSUE_TYPE_EDESC.ToLower()).ToList();
                        var ContainsIssueTypeName = IssueTypeNames.Where(x => !startWithNames.Contains(x.ISSUE_TYPE_EDESC.ToLower()) && !endWithNames.Contains(x.ISSUE_TYPE_EDESC.ToLower()));
                        StartWithIssueTypeName.ForEach(s => s.Type = "IssueTypeName");
                        EndWithIssueTypeName.ToList().ForEach(s => s.Type = "IssueTypeName");
                        ContainsIssueTypeName.ToList().ForEach(s => s.Type = "IssueTypeName");
                        Filterdata.AddRange(StartWithIssueTypeName);
                        Filterdata.AddRange(ContainsIssueTypeName);
                        Filterdata.AddRange(EndWithIssueTypeName);
                        this._cacheManager.Set($"AllIssueTypeList_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                        return Filterdata;
                    }
                    return AllFilterIssueType;
                }
            }
            return Filterdata;
        }
        [HttpGet]
        public List<Department> GetAllDepartmentSetup(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var Filterdata = new List<Department>();
            var ShowAdvanceAutoComplete = false;
            var setting = _settingService.LoadSetting<WebPrefrenceSetting>(Constants.WebPrefranceSetting);
            if (setting != null)
                bool.TryParse(setting.ShowAdvanceAutoComplete, out ShowAdvanceAutoComplete);
            if (ShowAdvanceAutoComplete == false)
            {
                if (this._cacheManager.IsSet($"AllDepartmentSetupByCode_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}"))
                {
                    var data = _cacheManager.Get<List<Department>>($"AllDepartmentSetupByCode_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}");
                    Filterdata = data;
                    return Filterdata;
                }
                else
                {
                    var AllFilterIssues = this._FormTemplateRepo.GetAllDepartmentSetup(filter);
                    this._cacheManager.Set($"AllDepartmentSetupByCode_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}", AllFilterIssues, 20);
                    return AllFilterIssues;
                }
            }

            if (filter == "!@$")
                return Filterdata;
            if (filter == null)
                return this._FormTemplateRepo.GetAllDepartmentSetup(filter);
            if (filter.Contains("#"))
            {
                var colName = filter.Split('#')[0].ToString().ToLower();
                filter = filter.Split('#')[1].ToString().ToLower();
                if (colName == "code")
                {
                    if (this._cacheManager.IsSet($"AllDepartmentSetupByCode_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Department>>($"AllDepartmentSetupByCode_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterDepartment = this._FormTemplateRepo.GetAllDepartmentSetup(filter);
                        var DepartmentCodes = AllFilterDepartment.Where(x => x.DepartmentCode.ToLower().Contains(filter));
                        var StartWithDepartmentCode = DepartmentCodes.Where(x => x.DepartmentCode.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithDepartmentCode.Select(x => x.DepartmentCode.ToLower()).ToList();
                        var EndWithDepartmentCode = DepartmentCodes.Where(x => !startWithCodes.Contains(x.DepartmentCode.ToLower()) && x.DepartmentCode.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithDepartmentCode.Select(x => x.DepartmentName.ToLower()).ToList();
                        var ContainsDepartmentCode = DepartmentCodes.Where(x => !startWithCodes.Contains(x.DepartmentCode.ToLower()) && !endWithCodes.Contains(x.DepartmentCode.ToLower()));
                        Filterdata.AddRange(StartWithDepartmentCode);
                        Filterdata.AddRange(ContainsDepartmentCode);
                        Filterdata.AddRange(EndWithDepartmentCode);
                        Filterdata.ForEach(s => s.Type = "DepartmentCode");
                        this._cacheManager.Set($"AllDepartmentSetupByCode_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
                if (colName == "name")
                {
                    if (this._cacheManager.IsSet($"AllDepartmentSetupByName_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Department>>($"AllDepartmentSetupByName_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterDepartment = this._FormTemplateRepo.GetAllDepartmentSetup(filter);
                        var DepartmentNames = AllFilterDepartment.Where(x => x.DepartmentName.ToLower().Contains(filter));
                        var StartWithDepartmentName = DepartmentNames.Where(x => x.DepartmentName.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithDepartmentName.Select(x => x.DepartmentName.ToLower()).ToList();
                        var EndWithDepartmentName = DepartmentNames.Where(x => !startWithNames.Contains(x.DepartmentName.ToLower()) && x.DepartmentName.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithDepartmentName.Select(x => x.DepartmentName.ToLower()).ToList();
                        var ContainsDepartmentName = DepartmentNames.Where(x => !startWithNames.Contains(x.DepartmentName.ToLower()) && !endWithNames.Contains(x.DepartmentName.ToLower()));
                        StartWithDepartmentName.ForEach(s => s.Type = "DepartmentName");
                        EndWithDepartmentName.ToList().ForEach(s => s.Type = "DepartmentName");
                        ContainsDepartmentName.ToList().ForEach(s => s.Type = "DepartmentName");
                        Filterdata.AddRange(StartWithDepartmentName);
                        Filterdata.AddRange(ContainsDepartmentName);
                        Filterdata.AddRange(EndWithDepartmentName);
                        this._cacheManager.Set($"AllDepartmentSetupByName_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }
                    return Filterdata;
                }
            }

            else
            {
                if (this._cacheManager.IsSet($"AllDepartment_{userid}_{company_code}_{branch_code}_{filter}"))
                {
                    var data = _cacheManager.Get<List<Department>>($"AllDepartment_{userid}_{company_code}_{branch_code}_{filter}");
                    Filterdata = data;
                }
                else
                {
                    var AllFilterDepartment = this._FormTemplateRepo.GetAllDepartmentSetup(filter);
                    if (filter == null)
                        return AllFilterDepartment;
                    if (AllFilterDepartment.Count >= 1)
                    {
                        var DepartmentCodes = AllFilterDepartment.Where(x => x.DepartmentCode.ToLower().Contains(filter));
                        var StartWithDepartmentCode = DepartmentCodes.Where(x => x.DepartmentCode.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithDepartmentCode.Select(x => x.DepartmentCode.ToLower()).ToList();
                        var EndWithDepartmentCode = DepartmentCodes.Where(x => !startWithCodes.Contains(x.DepartmentCode.ToLower()) && x.DepartmentCode.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithDepartmentCode.Select(x => x.DepartmentName.ToLower()).ToList();
                        var ContainsDepartmentCode = DepartmentCodes.Where(x => !startWithCodes.Contains(x.DepartmentCode.ToLower()) && !endWithCodes.Contains(x.DepartmentCode.ToLower()));
                        Filterdata.AddRange(StartWithDepartmentCode);
                        Filterdata.AddRange(ContainsDepartmentCode);
                        Filterdata.AddRange(EndWithDepartmentCode);
                        Filterdata.ForEach(s => s.Type = "DepartmentCode");
                        var Removedata = AllFilterDepartment.RemoveAll(x => x.DepartmentCode.ToLower().Contains(filter));
                        var DepartmentNames = AllFilterDepartment.Where(x => x.DepartmentName.ToLower().Contains(filter));
                        var StartWithDepartmentName = DepartmentNames.Where(x => x.DepartmentName.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithDepartmentName.Select(x => x.DepartmentName.ToLower()).ToList();
                        var EndWithDepartmentName = DepartmentNames.Where(x => !startWithNames.Contains(x.DepartmentName.ToLower()) && x.DepartmentName.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithDepartmentName.Select(x => x.DepartmentName.ToLower()).ToList();
                        var ContainsDepartmentName = DepartmentNames.Where(x => !startWithNames.Contains(x.DepartmentName.ToLower()) && !endWithNames.Contains(x.DepartmentName.ToLower()));
                        StartWithDepartmentName.ForEach(s => s.Type = "DepartmentName");
                        EndWithDepartmentName.ToList().ForEach(s => s.Type = "DepartmentName");
                        ContainsDepartmentName.ToList().ForEach(s => s.Type = "DepartmentName");
                        Filterdata.AddRange(StartWithDepartmentName);
                        Filterdata.AddRange(ContainsDepartmentName);
                        Filterdata.AddRange(EndWithDepartmentName);
                        this._cacheManager.Set($"AllDepartment_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                        return Filterdata;
                    }
                    return AllFilterDepartment;
                }
            }
            return Filterdata;
        }
        [HttpGet]
        public List<Employee> GetAllEmployeeCodeByFilters(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var Filterdata = new List<Employee>();
            var ShowAdvanceAutoComplete = false;
            var setting = _settingService.LoadSetting<WebPrefrenceSetting>(Constants.WebPrefranceSetting);
            if (setting != null)
                bool.TryParse(setting.ShowAdvanceAutoComplete, out ShowAdvanceAutoComplete);
            if (ShowAdvanceAutoComplete == false)
            {
                if (this._cacheManager.IsSet($"AllEmployeeSetupByCode_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}"))
                {
                    var data = _cacheManager.Get<List<Employee>>($"AllEmployeeSetupByCode_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}");
                    Filterdata = data;
                    return Filterdata;
                }
                else
                {
                    var AllFilterIssues = this._FormTemplateRepo.GetAllEmployeeCodeByFilter(filter);
                    this._cacheManager.Set($"AllEmployeeSetupByCode_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}", AllFilterIssues, 20);
                    return AllFilterIssues;
                }
            }

            if (filter == "!@$")
                return Filterdata;
            if (filter == null)
                return this._FormTemplateRepo.GetAllEmployeeCodeByFilter(filter);
            if (filter.Contains("#"))
            {
                var colName = filter.Split('#')[0].ToString().ToLower();
                filter = filter.Split('#')[1].ToString().ToLower();
                if (colName == "code")
                {
                    if (this._cacheManager.IsSet($"AllEmployeeSetupByCode_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Employee>>($"AllEmployeeSetupByCode_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterEmployee = this._FormTemplateRepo.GetAllEmployeeCodeByFilter(filter);
                        var EmployeeCodes = AllFilterEmployee.Where(x => x.EMPLOYEE_CODE.ToLower().Contains(filter));
                        var StartWithEmployeeCode = EmployeeCodes.Where(x => x.EMPLOYEE_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithEmployeeCode.Select(x => x.EMPLOYEE_CODE.ToLower()).ToList();
                        var EndWithEmployeeCode = EmployeeCodes.Where(x => !startWithCodes.Contains(x.EMPLOYEE_CODE.ToLower()) && x.EMPLOYEE_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithEmployeeCode.Select(x => x.EMPLOYEE_EDESC.ToLower()).ToList();
                        var ContainsEmployeeCode = EmployeeCodes.Where(x => !startWithCodes.Contains(x.EMPLOYEE_CODE.ToLower()) && !endWithCodes.Contains(x.EMPLOYEE_CODE.ToLower()));
                        Filterdata.AddRange(StartWithEmployeeCode);
                        Filterdata.AddRange(ContainsEmployeeCode);
                        Filterdata.AddRange(EndWithEmployeeCode);
                        Filterdata.ForEach(s => s.Type = "EmployeeCode");
                        this._cacheManager.Set($"AllEmployeeSetupByCode_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }

                    return Filterdata;
                }
                if (colName == "name")
                {
                    if (this._cacheManager.IsSet($"AllEmployeeSetupByName_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<Employee>>($"AllEmployeeSetupByName_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterEmployee = this._FormTemplateRepo.GetAllEmployeeCodeByFilter(filter);
                        var EmployeeNames = AllFilterEmployee.Where(x => x.EMPLOYEE_EDESC.ToLower().Contains(filter));
                        var StartWithEmployeeName = EmployeeNames.Where(x => x.EMPLOYEE_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithEmployeeName.Select(x => x.EMPLOYEE_EDESC.ToLower()).ToList();
                        var EndWithEmployeeName = EmployeeNames.Where(x => !startWithNames.Contains(x.EMPLOYEE_EDESC.ToLower()) && x.EMPLOYEE_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithEmployeeName.Select(x => x.EMPLOYEE_EDESC.ToLower()).ToList();
                        var ContainsEmployeeName = EmployeeNames.Where(x => !startWithNames.Contains(x.EMPLOYEE_EDESC.ToLower()) && !endWithNames.Contains(x.EMPLOYEE_EDESC.ToLower()));
                        StartWithEmployeeName.ForEach(s => s.Type = "EmployeeName");
                        EndWithEmployeeName.ToList().ForEach(s => s.Type = "EmployeeName");
                        ContainsEmployeeName.ToList().ForEach(s => s.Type = "EmployeeName");
                        Filterdata.AddRange(StartWithEmployeeName);
                        Filterdata.AddRange(ContainsEmployeeName);
                        Filterdata.AddRange(EndWithEmployeeName);
                        this._cacheManager.Set($"AllEmployeeSetupByName_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }

                    return Filterdata;
                }
            }
            else
            {
                if (this._cacheManager.IsSet($"AllEmployee_{userid}_{company_code}_{branch_code}_{filter}"))
                {
                    var data = _cacheManager.Get<List<Employee>>($"AllEmployee_{userid}_{company_code}_{branch_code}_{filter}");
                    Filterdata = data;
                }
                else
                {
                    var AllFilterEmployee = this._FormTemplateRepo.GetAllEmployeeCodeByFilter(filter);
                    if (filter == null)
                        return AllFilterEmployee;
                    if (AllFilterEmployee.Count >= 1)
                    {
                        var EmployeeCodes = AllFilterEmployee.Where(x => x.EMPLOYEE_CODE.ToLower().Contains(filter));
                        var StartWithEmployeeCode = EmployeeCodes.Where(x => x.EMPLOYEE_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithEmployeeCode.Select(x => x.EMPLOYEE_CODE.ToLower()).ToList();
                        var EndWithEmployeeCode = EmployeeCodes.Where(x => !startWithCodes.Contains(x.EMPLOYEE_CODE.ToLower()) && x.EMPLOYEE_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithEmployeeCode.Select(x => x.EMPLOYEE_EDESC.ToLower()).ToList();
                        var ContainsEmployeeCode = EmployeeCodes.Where(x => !startWithCodes.Contains(x.EMPLOYEE_CODE.ToLower()) && !endWithCodes.Contains(x.EMPLOYEE_CODE.ToLower()));
                        Filterdata.AddRange(StartWithEmployeeCode);
                        Filterdata.AddRange(ContainsEmployeeCode);
                        Filterdata.AddRange(EndWithEmployeeCode);
                        Filterdata.ForEach(s => s.Type = "EmployeeCode");
                        AllFilterEmployee.RemoveAll(x => x.EMPLOYEE_CODE.ToLower().Contains(filter));
                        var EmployeeNames = AllFilterEmployee.Where(x => x.EMPLOYEE_EDESC.ToLower().Contains(filter));
                        var StartWithEmployeeName = EmployeeNames.Where(x => x.EMPLOYEE_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithEmployeeName.Select(x => x.EMPLOYEE_EDESC.ToLower()).ToList();
                        var EndWithEmployeeName = EmployeeNames.Where(x => !startWithNames.Contains(x.EMPLOYEE_EDESC.ToLower()) && x.EMPLOYEE_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithEmployeeName.Select(x => x.EMPLOYEE_EDESC.ToLower()).ToList();
                        var ContainsEmployeeName = EmployeeNames.Where(x => !startWithNames.Contains(x.EMPLOYEE_EDESC.ToLower()) && !endWithNames.Contains(x.EMPLOYEE_EDESC.ToLower()));
                        StartWithEmployeeName.ForEach(s => s.Type = "EmployeeName");
                        EndWithEmployeeName.ToList().ForEach(s => s.Type = "EmployeeName");
                        ContainsEmployeeName.ToList().ForEach(s => s.Type = "EmployeeName");
                        Filterdata.AddRange(StartWithEmployeeName);
                        Filterdata.AddRange(ContainsEmployeeName);
                        Filterdata.AddRange(EndWithEmployeeName);
                        this._cacheManager.Set($"AllEmployee_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                        return Filterdata;
                    }
                    return AllFilterEmployee;
                }
            }
            return Filterdata;
        }
        [HttpGet]
        public List<Location> GetAllLocationListByFilter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var ShowAdvanceAutoComplete = false;
            var setting = _settingService.LoadSetting<WebPrefrenceSetting>(Constants.WebPrefranceSetting);
            if (setting != null)
                bool.TryParse(setting.ShowAdvanceAutoComplete, out ShowAdvanceAutoComplete);
            if (ShowAdvanceAutoComplete == false)
            {
                if (this._cacheManager.IsSet($"GetAllLocationListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}"))
                {
                    var data = _cacheManager.Get<List<Location>>($"GetAllLocationListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}");
                    return data;
                }
                else
                {
                    var AllFilterIssues = this._FormTemplateRepo.GetAllLocationSetup(filter);
                    this._cacheManager.Set($"GetAllLocationListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}", AllFilterIssues, 20);
                    return AllFilterIssues;
                }
            }
            var Filterdata = new List<Location>();
            if (filter == "!@$")
                return Filterdata;
            if (filter == null)
                return this._FormTemplateRepo.GetAllLocationSetup(filter);
            if (filter.Contains("#"))
            {
                var colName = filter.Split('#')[0].ToString().ToLower();
                filter = filter.Split('#')[1].ToString().ToLower();
                if (colName == "code")
                {
                    if (this._cacheManager.IsSet($"GetAllLocationListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        Filterdata = this._cacheManager.Get<List<Location>>($"GetAllLocationListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                    }
                    else
                    {
                        var AllFilterLocation = this._FormTemplateRepo.GetAllLocationSetup(filter);
                        var LocationCodes = AllFilterLocation.Where(x => x.LocationCode.ToLower().Contains(filter));
                        var StartWithLocationCode = LocationCodes.Where(x => x.LocationCode.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithLocationCode.Select(x => x.LocationCode.ToLower()).ToList();
                        var EndWithLocationCode = LocationCodes.Where(x => !startWithCodes.Contains(x.LocationCode.ToLower()) && x.LocationCode.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithLocationCode.Select(x => x.LocationName.ToLower()).ToList();
                        var ContainsLocationCode = LocationCodes.Where(x => !startWithCodes.Contains(x.LocationCode.ToLower()) && !endWithCodes.Contains(x.LocationCode.ToLower()));
                        Filterdata.AddRange(StartWithLocationCode);
                        Filterdata.AddRange(ContainsLocationCode);
                        Filterdata.AddRange(EndWithLocationCode);
                        Filterdata.ForEach(s => s.Type = "LocationCode");
                        this._cacheManager.Set($"GetAllLocationListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 30);
                        return Filterdata;
                    }
                }

                if (colName == "name")
                {
                    if (this._cacheManager.IsSet($"GetAllLocationListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        Filterdata = this._cacheManager.Get<List<Location>>($"GetAllLocationListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                    }
                    else
                    {
                        var AllFilterLocation = this._FormTemplateRepo.GetAllLocationSetup(filter);
                        var LocationNames = AllFilterLocation.Where(x => x.LocationName.ToLower().Contains(filter));
                        var StartWithLocationName = LocationNames.Where(x => x.LocationName.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithLocationName.Select(x => x.LocationName.ToLower()).ToList();
                        var EndWithLocationName = LocationNames.Where(x => !startWithNames.Contains(x.LocationName.ToLower()) && x.LocationName.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithLocationName.Select(x => x.LocationName.ToLower()).ToList();
                        var ContainsLocationName = LocationNames.Where(x => !startWithNames.Contains(x.LocationName.ToLower()) && !endWithNames.Contains(x.LocationName.ToLower()));
                        StartWithLocationName.ForEach(s => s.Type = "LocationName");
                        EndWithLocationName.ToList().ForEach(s => s.Type = "LocationName");
                        ContainsLocationName.ToList().ForEach(s => s.Type = "LocationName");
                        Filterdata.AddRange(StartWithLocationName);
                        Filterdata.AddRange(ContainsLocationName);
                        Filterdata.AddRange(EndWithLocationName);
                        this._cacheManager.Set($"GetAllLocationListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 30);
                        return Filterdata;
                    }
                }
                if (colName == "address")
                {
                    if (this._cacheManager.IsSet($"GetAllLocationListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        Filterdata = this._cacheManager.Get<List<Location>>($"GetAllLocationListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                    }
                    else
                    {
                        var AllFilterLocation = this._FormTemplateRepo.GetAllLocationSetup(filter);
                        var Address = AllFilterLocation.Where(x => x.Address.ToLower().Contains(filter));
                        var StartWithAddresses = Address.Where(x => x.Address.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithAddress = StartWithAddresses.Select(x => x.Address.ToLower()).ToList();
                        var EndWithAddresses = Address.Where(x => !startWithAddress.Contains(x.Address.ToLower()) && x.Address.ToLower().EndsWith(filter.Trim()));
                        var endWithAddress = EndWithAddresses.Select(x => x.Address.ToLower()).ToList();
                        var ContainsAddresses = Address.Where(x => !startWithAddress.Contains(x.Address.ToLower()) && !endWithAddress.Contains(x.Address.ToLower()));
                        StartWithAddresses.ForEach(s => s.Type = "Address");
                        EndWithAddresses.ToList().ForEach(s => s.Type = "Address");
                        ContainsAddresses.ToList().ForEach(s => s.Type = "Address");
                        Filterdata.AddRange(StartWithAddresses);
                        Filterdata.AddRange(ContainsAddresses);
                        Filterdata.AddRange(EndWithAddresses);
                        this._cacheManager.Set($"GetAllLocationListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 30);
                        return Filterdata;
                    }
                }
                if (colName == "phoneno")
                {
                    if (this._cacheManager.IsSet($"GetAllLocationListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        Filterdata = this._cacheManager.Get<List<Location>>($"GetAllLocationListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                    }
                    else
                    {
                        var AllFilterLocation = this._FormTemplateRepo.GetAllLocationSetup(filter);
                        var PhoneNo = AllFilterLocation.Where(x => x.Telephone_Mobile_No.ToLower().Contains(filter));
                        var StartWithPhoneNo = PhoneNo.Where(x => x.Telephone_Mobile_No.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithPhone = StartWithPhoneNo.Select(x => x.Telephone_Mobile_No.ToLower()).ToList();
                        var EndWithPhoneNo = PhoneNo.Where(x => !startWithPhone.Contains(x.Telephone_Mobile_No.ToLower()) && x.Telephone_Mobile_No.ToLower().EndsWith(filter.Trim()));
                        var endWithPhone = EndWithPhoneNo.Select(x => x.Telephone_Mobile_No.ToLower()).ToList();
                        var ContainsLocationPhoneno = PhoneNo.Where(x => !startWithPhone.Contains(x.Telephone_Mobile_No.ToLower()) && !endWithPhone.Contains(x.Telephone_Mobile_No.ToLower()));
                        StartWithPhoneNo.ForEach(s => s.Type = "Ph");
                        EndWithPhoneNo.ToList().ForEach(s => s.Type = "Ph");
                        ContainsLocationPhoneno.ToList().ForEach(s => s.Type = "Ph");
                        Filterdata.AddRange(StartWithPhoneNo);
                        Filterdata.AddRange(ContainsLocationPhoneno);
                        Filterdata.AddRange(EndWithPhoneNo);
                        this._cacheManager.Set($"GetAllLocationListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 30);
                        return Filterdata;
                    }
                }
            }
            else
            {
                var AllFilterLocation = this._FormTemplateRepo.GetAllLocationSetup(filter);
                if (filter == null)
                    return AllFilterLocation;
                if (AllFilterLocation.Count >= 1)
                {
                    if (this._cacheManager.IsSet($"GetAllLocationListByFilter_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        Filterdata = this._cacheManager.Get<List<Location>>($"GetAllLocationListByFilter_{userid}_{company_code}_{branch_code}_{filter}");
                    }
                    else
                    {
                        var LocationNames = AllFilterLocation.Where(x => x.LocationName.ToLower().Contains(filter));
                        var StartWithLocationName = LocationNames.Where(x => x.LocationName.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithLocationName.Select(x => x.LocationName.ToLower()).ToList();
                        var EndWithLocationName = LocationNames.Where(x => !startWithNames.Contains(x.LocationName.ToLower()) && x.LocationName.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithLocationName.Select(x => x.LocationName.ToLower()).ToList();
                        var ContainsLocationName = LocationNames.Where(x => !startWithNames.Contains(x.LocationName.ToLower()) && !endWithNames.Contains(x.LocationName.ToLower()));
                        Filterdata.AddRange(StartWithLocationName);
                        Filterdata.AddRange(ContainsLocationName);
                        Filterdata.AddRange(EndWithLocationName);
                        Filterdata.ForEach(s => s.Type = "LocationName");
                        AllFilterLocation.RemoveAll(x => x.LocationName.ToLower().Contains(filter));
                        var LocationCodes = AllFilterLocation.Where(x => x.LocationCode.ToLower().Contains(filter));
                        var StartWithLocationCode = LocationCodes.Where(x => x.LocationCode.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithLocationCode.Select(x => x.LocationCode.ToLower()).ToList();
                        var EndWithLocationCode = LocationCodes.Where(x => !startWithCodes.Contains(x.LocationCode.ToLower()) && x.LocationCode.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithLocationCode.Select(x => x.LocationName.ToLower()).ToList();
                        var ContainsLocationCode = LocationCodes.Where(x => !startWithCodes.Contains(x.LocationCode.ToLower()) && !endWithCodes.Contains(x.LocationCode.ToLower()));
                        StartWithLocationCode.ForEach(s => s.Type = "LocationCode");
                        EndWithLocationCode.ToList().ForEach(s => s.Type = "LocationCode");
                        ContainsLocationCode.ToList().ForEach(s => s.Type = "LocationCode");
                        Filterdata.AddRange(StartWithLocationCode);
                        Filterdata.AddRange(ContainsLocationCode);
                        Filterdata.AddRange(EndWithLocationCode);
                        AllFilterLocation.RemoveAll(x => x.LocationCode.ToLower().Contains(filter));
                        var Address = AllFilterLocation.Where(x => x.Address.ToLower().Contains(filter));
                        var StartWithAddresses = Address.Where(x => x.Address.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithAddress = StartWithAddresses.Select(x => x.Address.ToLower()).ToList();
                        var EndWithAddresses = Address.Where(x => !startWithAddress.Contains(x.Address.ToLower()) && x.Address.ToLower().EndsWith(filter.Trim()));
                        var endWithAddress = EndWithAddresses.Select(x => x.LocationName.ToLower()).ToList();
                        var ContainsAddresses = Address.Where(x => !startWithAddress.Contains(x.Address.ToLower()) && !endWithAddress.Contains(x.Address.ToLower()));
                        StartWithAddresses.ForEach(s => s.Type = "Address");
                        EndWithAddresses.ToList().ForEach(s => s.Type = "Address");
                        ContainsAddresses.ToList().ForEach(s => s.Type = "Address");
                        Filterdata.AddRange(StartWithAddresses);
                        Filterdata.AddRange(ContainsAddresses);
                        Filterdata.AddRange(EndWithAddresses);
                        AllFilterLocation.RemoveAll(x => x.Address.ToLower().Contains(filter));
                        var PhoneNo = AllFilterLocation.Where(x => x.Telephone_Mobile_No.ToLower().Contains(filter));
                        var StartWithPhoneNo = PhoneNo.Where(x => x.Telephone_Mobile_No.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithPhone = StartWithPhoneNo.Select(x => x.Telephone_Mobile_No.ToLower()).ToList();
                        var EndWithPhoneNo = PhoneNo.Where(x => !startWithPhone.Contains(x.Telephone_Mobile_No.ToLower()) && x.Telephone_Mobile_No.ToLower().EndsWith(filter.Trim()));
                        var endWithPhone = EndWithPhoneNo.Select(x => x.Telephone_Mobile_No.ToLower()).ToList();
                        var ContainsLocationPhoneno = PhoneNo.Where(x => !startWithPhone.Contains(x.Telephone_Mobile_No.ToLower()) && !endWithPhone.Contains(x.Telephone_Mobile_No.ToLower()));
                        StartWithPhoneNo.ForEach(s => s.Type = "Ph");
                        EndWithPhoneNo.ToList().ForEach(s => s.Type = "Ph");
                        ContainsLocationPhoneno.ToList().ForEach(s => s.Type = "Ph");
                        Filterdata.AddRange(StartWithPhoneNo);
                        Filterdata.AddRange(ContainsLocationPhoneno);
                        Filterdata.AddRange(EndWithPhoneNo);
                        this._cacheManager.Set($"GetAllLocationListByFilter_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 30);
                        return Filterdata;
                    }
                    return AllFilterLocation;
                }
            }
            return Filterdata;
        }
        [HttpGet]
        public List<Products> GetAllProductsListByFilter(string filter)
        {
           if (string.IsNullOrEmpty(filter))
                return new List<Products>();

            var Filterdata = new List<Products>();
            if (filter == "!@$")
                return Filterdata;
            if(filter.ToLower().Contains("undef"))
                return Filterdata;

            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var ShowAdvanceAutoComplete = false;
            var setting = _settingService.LoadSetting<WebPrefrenceSetting>(Constants.WebPrefranceSetting);
            if (setting != null)
                bool.TryParse(setting.ShowAdvanceAutoComplete, out ShowAdvanceAutoComplete);
            if (ShowAdvanceAutoComplete == false)
            {
                if (this._cacheManager.IsSet($"GetAllProductsListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}"))
                {
                    var data = _cacheManager.Get<List<Products>>($"GetAllProductsListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}");
                    return data;
                }
                else
                {
                    var AllFilterIssues = this._FormTemplateRepo.GetAllProducts(filter);
                    this._cacheManager.Set($"GetAllProductsListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}", AllFilterIssues, 20);
                    return AllFilterIssues;
                }
            }
          //  var Filterdata = new List<Products>();
            //if (filter == "!@$")
            //    return Filterdata;
            if (filter == null)
                return this._FormTemplateRepo.GetAllProducts(filter);
            if (filter.Contains("#"))
            {
                var colName = filter.Split('#')[0].ToString().ToLower();
                filter = filter.Split('#')[1].ToString().ToLower();
                if (colName == "code")
                {
                    if (this._cacheManager.IsSet($"GetAllProductsListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        Filterdata = this._cacheManager.Get<List<Products>>($"GetAllProductsListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                    }
                    else
                    {
                        var AllFilterProduct = this._FormTemplateRepo.GetAllProducts(filter);
                        var ProductCodes = AllFilterProduct.Where(x => x.ItemCode.ToLower().Contains(filter));
                        var StartWithProductCode = ProductCodes.Where(x => x.ItemCode.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithProductCode.Select(x => x.ItemCode.ToLower()).ToList();
                        var EndWithProductCode = ProductCodes.Where(x => !startWithCodes.Contains(x.ItemCode.ToLower()) && x.ItemCode.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithProductCode.Select(x => x.ItemCode.ToLower()).ToList();
                        var ContainsProductCode = ProductCodes.Where(x => !startWithCodes.Contains(x.ItemCode.ToLower()) && !endWithCodes.Contains(x.ItemCode.ToLower()));
                        Filterdata.AddRange(StartWithProductCode);
                        Filterdata.AddRange(ContainsProductCode);
                        Filterdata.AddRange(EndWithProductCode);
                        Filterdata.ForEach(s => s.Type = "ProductCode");
                        this._cacheManager.Set($"GetAllProductsListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 30);
                        return Filterdata;
                    }
                }
                if (colName == "name")
                {
                    if (this._cacheManager.IsSet($"GetAllProductsListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        Filterdata = this._cacheManager.Get<List<Products>>($"GetAllProductsListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                    }
                    else
                    {
                        var AllFilterProduct = this._FormTemplateRepo.GetAllProducts(filter);
                        var ProductNames = AllFilterProduct.Where(x => x.ItemDescription.ToLower().Contains(filter));
                        var StartWithProductName = ProductNames.Where(x => x.ItemDescription.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithProductName.Select(x => x.ItemDescription.ToLower()).ToList();
                        var EndWithProductName = ProductNames.Where(x => !startWithNames.Contains(x.ItemDescription.ToLower()) && x.ItemDescription.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithProductName.Select(x => x.ItemDescription.ToLower()).ToList();
                        var ContainsProductName = ProductNames.Where(x => !startWithNames.Contains(x.ItemDescription.ToLower()) && !endWithNames.Contains(x.ItemDescription.ToLower()));
                        StartWithProductName.ForEach(s => s.Type = "ProductName");
                        EndWithProductName.ToList().ForEach(s => s.Type = "ProductName");
                        ContainsProductName.ToList().ForEach(s => s.Type = "ProductName");
                        Filterdata.AddRange(StartWithProductName);
                        Filterdata.AddRange(ContainsProductName);
                        Filterdata.AddRange(EndWithProductName);
                        this._cacheManager.Set($"GetAllProductsListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 30);
                        return Filterdata;
                    }
                }
            }
            else
            {
                var AllFilterProduct = this._FormTemplateRepo.GetAllProducts(filter);
                if (filter == null)
                    return AllFilterProduct;
                if (AllFilterProduct.Count >= 1)
                {
                    if (this._cacheManager.IsSet($"GetAllProductsListByFilter_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        Filterdata = this._cacheManager.Get<List<Products>>($"GetAllProductsListByFilter_{userid}_{company_code}_{branch_code}_{filter}");
                    }
                    else
                    {
                        var ProductCodes = AllFilterProduct.Where(x => x.ItemCode.ToLower().Contains(filter));
                        var StartWithProductCode = ProductCodes.Where(x => x.ItemCode.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithProductCode.Select(x => x.ItemCode.ToLower()).ToList();
                        var EndWithProductCode = ProductCodes.Where(x => !startWithCodes.Contains(x.ItemCode.ToLower()) && x.ItemCode.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithProductCode.Select(x => x.ItemCode.ToLower()).ToList();
                        var ContainsProductCode = ProductCodes.Where(x => !startWithCodes.Contains(x.ItemCode.ToLower()) && !endWithCodes.Contains(x.ItemCode.ToLower()));
                        Filterdata.AddRange(StartWithProductCode);
                        Filterdata.AddRange(ContainsProductCode);
                        Filterdata.AddRange(EndWithProductCode);
                        Filterdata.ForEach(s => s.Type = "ProductCode");
                        var ProductNames = AllFilterProduct.Where(x => x.ItemDescription.ToLower().Contains(filter));
                        var StartWithProductName = ProductNames.Where(x => x.ItemDescription.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithProductName.Select(x => x.ItemDescription.ToLower()).ToList();
                        var EndWithProductName = ProductNames.Where(x => !startWithNames.Contains(x.ItemDescription.ToLower()) && x.ItemDescription.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithProductName.Select(x => x.ItemDescription.ToLower()).ToList();
                        var ContainsProductName = ProductNames.Where(x => !startWithNames.Contains(x.ItemDescription.ToLower()) && !endWithNames.Contains(x.ItemDescription.ToLower()));
                        StartWithProductName.ForEach(s => s.Type = "ProductName");
                        EndWithProductName.ToList().ForEach(s => s.Type = "ProductName");
                        ContainsProductName.ToList().ForEach(s => s.Type = "ProductName");
                        Filterdata.AddRange(StartWithProductName);
                        Filterdata.AddRange(ContainsProductName);
                        Filterdata.AddRange(EndWithProductName);
                        this._cacheManager.Set($"GetAllProductsListByFilter_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 30);
                        return Filterdata;
                    }
                }
                return AllFilterProduct;
            }
            return Filterdata;
        }
        [HttpGet]
        public List<CostCenter> GetAllCostCenter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var ShowAdvanceAutoComplete = false;
            var setting = _settingService.LoadSetting<WebPrefrenceSetting>(Constants.WebPrefranceSetting);
            if (setting != null)
                bool.TryParse(setting.ShowAdvanceAutoComplete, out ShowAdvanceAutoComplete);
            if (ShowAdvanceAutoComplete == false)
            {
                if (this._cacheManager.IsSet($"GetAllCostCenter_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}"))
                {
                    var data = _cacheManager.Get<List<CostCenter>>($"GetAllCostCenter_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}");
                    return data;
                }
                else
                {
                    var AllFilterIssues = this._FormTemplateRepo.GetAllCostCenter(filter);
                    this._cacheManager.Set($"GetAllCostCenter_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}", AllFilterIssues, 20);
                    return AllFilterIssues;
                }
            }
            var Filterdata = new List<CostCenter>();
            if (filter == "!@$")
                return Filterdata;
            if (filter == null)
                return this._FormTemplateRepo.GetAllCostCenter(filter);
            if (filter.Contains("#"))
            {
                var colName = filter.Split('#')[0].ToString().ToLower();
                filter = filter.Split('#')[1].ToString().ToLower();
                if (colName == "code")
                {
                    if (this._cacheManager.IsSet($"GetAllCostCenter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        Filterdata = this._cacheManager.Get<List<CostCenter>>($"GetAllCostCenter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                    }
                    else
                    {
                        var AllFilterCostCenter = this._FormTemplateRepo.GetAllCostCenter(filter);
                        var CostCenterCodes = AllFilterCostCenter.Where(x => x.BudgetCode.ToLower().Contains(filter));
                        var StartWithCostCenterCode = CostCenterCodes.Where(x => x.BudgetCode.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithCostCenterCode.Select(x => x.BudgetCode.ToLower()).ToList();
                        var EndWithCostCenterCode = CostCenterCodes.Where(x => !startWithCodes.Contains(x.BudgetCode.ToLower()) && x.BudgetCode.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithCostCenterCode.Select(x => x.BudgetName.ToLower()).ToList();
                        var ContainsCostCenterCode = CostCenterCodes.Where(x => !startWithCodes.Contains(x.BudgetCode.ToLower()) && !endWithCodes.Contains(x.BudgetCode.ToLower()));
                        Filterdata.AddRange(StartWithCostCenterCode);
                        Filterdata.AddRange(ContainsCostCenterCode);
                        Filterdata.AddRange(EndWithCostCenterCode);
                        Filterdata.ForEach(s => s.Type = "CostCenterCode");
                        this._cacheManager.Set($"GetAllCostCenter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 30);
                        return Filterdata;
                    }
                }
                if (colName == "name")
                {
                    if (this._cacheManager.IsSet($"GetAllCostCenter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        Filterdata = this._cacheManager.Get<List<CostCenter>>($"GetAllCostCenter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                    }
                    else
                    {
                        var AllFilterCostCenter = this._FormTemplateRepo.GetAllCostCenter(filter);
                        var CostCenterNames = AllFilterCostCenter.Where(x => x.BudgetName.ToLower().Contains(filter));
                        var StartWithCostCenterName = CostCenterNames.Where(x => x.BudgetName.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithCostCenterName.Select(x => x.BudgetName.ToLower()).ToList();
                        var EndWithCostCenterName = CostCenterNames.Where(x => !startWithNames.Contains(x.BudgetName.ToLower()) && x.BudgetName.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithCostCenterName.Select(x => x.BudgetName.ToLower()).ToList();
                        var ContainsCostCenterName = CostCenterNames.Where(x => !startWithNames.Contains(x.BudgetName.ToLower()) && !endWithNames.Contains(x.BudgetName.ToLower()));
                        StartWithCostCenterName.ForEach(s => s.Type = "CostCenterName");
                        EndWithCostCenterName.ToList().ForEach(s => s.Type = "CostCenterName");
                        ContainsCostCenterName.ToList().ForEach(s => s.Type = "CostCenterName");
                        Filterdata.AddRange(StartWithCostCenterName);
                        Filterdata.AddRange(ContainsCostCenterName);
                        Filterdata.AddRange(EndWithCostCenterName);
                        this._cacheManager.Set($"GetAllCostCenter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 30);
                        return Filterdata;
                    }
                }
            }
            else
            {
                var AllFilterCostCenter = this._FormTemplateRepo.GetAllCostCenter(filter);
                if (filter == null)
                    return AllFilterCostCenter;
                if (AllFilterCostCenter.Count >= 1)
                {
                    if (this._cacheManager.IsSet($"GetAllCostCenter_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        Filterdata = this._cacheManager.Get<List<CostCenter>>($"GetAllCostCenter_{userid}_{company_code}_{branch_code}_{filter}");
                    }
                    else
                    {
                        var CostCenterCodes = AllFilterCostCenter.Where(x => x.BudgetCode.ToLower().Contains(filter));
                        var StartWithCostCenterCode = CostCenterCodes.Where(x => x.BudgetCode.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithCostCenterCode.Select(x => x.BudgetCode.ToLower()).ToList();
                        var EndWithCostCenterCode = CostCenterCodes.Where(x => !startWithCodes.Contains(x.BudgetCode.ToLower()) && x.BudgetCode.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithCostCenterCode.Select(x => x.BudgetName.ToLower()).ToList();
                        var ContainsCostCenterCode = CostCenterCodes.Where(x => !startWithCodes.Contains(x.BudgetCode.ToLower()) && !endWithCodes.Contains(x.BudgetCode.ToLower()));
                        Filterdata.AddRange(StartWithCostCenterCode);
                        Filterdata.AddRange(ContainsCostCenterCode);
                        Filterdata.AddRange(EndWithCostCenterCode);
                        Filterdata.ForEach(s => s.Type = "CostCenterCode");
                        var Removedata = AllFilterCostCenter.RemoveAll(x => x.BudgetCode.ToLower().Contains(filter));
                        var CostCenterNames = AllFilterCostCenter.Where(x => x.BudgetName.ToLower().Contains(filter));
                        var StartWithCostCenterName = CostCenterNames.Where(x => x.BudgetName.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithCostCenterName.Select(x => x.BudgetName.ToLower()).ToList();
                        var EndWithCostCenterName = CostCenterNames.Where(x => !startWithNames.Contains(x.BudgetName.ToLower()) && x.BudgetName.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithCostCenterName.Select(x => x.BudgetName.ToLower()).ToList();
                        var ContainsCostCenterName = CostCenterNames.Where(x => !startWithNames.Contains(x.BudgetName.ToLower()) && !endWithNames.Contains(x.BudgetName.ToLower()));
                        StartWithCostCenterName.ForEach(s => s.Type = "CostCenterName");
                        EndWithCostCenterName.ToList().ForEach(s => s.Type = "CostCenterName");
                        ContainsCostCenterName.ToList().ForEach(s => s.Type = "CostCenterName");
                        Filterdata.AddRange(StartWithCostCenterName);
                        Filterdata.AddRange(ContainsCostCenterName);
                        Filterdata.AddRange(EndWithCostCenterName);
                        this._cacheManager.Set($"GetAllCostCenter_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 30);
                        return Filterdata;
                    }
                }
                return AllFilterCostCenter;
            }
            return Filterdata;
        }
        [HttpGet]
        public List<Priority> GetPriorityListByFlter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var Filterdata = new List<Priority>();
            if (filter == "!@$")
                return Filterdata;
            if (filter == null)
                return this._FormTemplateRepo.GetAllPrioritySetup(filter);
            if (filter.Contains("#"))
            {
                var colName = filter.Split('#')[0].ToString().ToLower();
                filter = filter.Split('#')[1].ToString().ToLower();
                if (colName == "code")
                {
                    if (this._cacheManager.IsSet($"GetPriorityListByFlter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        Filterdata = this._cacheManager.Get<List<Priority>>($"GetPriorityListByFlter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                    }
                    else
                    {
                        var AllFilterPriority = this._FormTemplateRepo.GetAllPrioritySetup(filter);
                        var PriorityCodes = AllFilterPriority.Where(x => x.PRIORITY_CODE.ToLower().Contains(filter));
                        var StartWithPriorityCode = PriorityCodes.Where(x => x.PRIORITY_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithPriorityCode.Select(x => x.PRIORITY_CODE.ToLower()).ToList();
                        var EndWithPriorityCode = PriorityCodes.Where(x => !startWithCodes.Contains(x.PRIORITY_CODE.ToLower()) && x.PRIORITY_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithPriorityCode.Select(x => x.PRIORITY_EDESC.ToLower()).ToList();
                        var ContainsPriorityCode = PriorityCodes.Where(x => !startWithCodes.Contains(x.PRIORITY_CODE.ToLower()) && !endWithCodes.Contains(x.PRIORITY_CODE.ToLower()));
                        Filterdata.AddRange(StartWithPriorityCode);
                        Filterdata.AddRange(ContainsPriorityCode);
                        Filterdata.AddRange(EndWithPriorityCode);
                        Filterdata.ForEach(s => s.Type = "PriorityCode");
                        this._cacheManager.Set($"GetPriorityListByFlter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 30);
                        return Filterdata;
                    }
                }
                if (colName == "name")
                {
                    if (this._cacheManager.IsSet($"GetPriorityListByFlter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        Filterdata = this._cacheManager.Get<List<Priority>>($"GetPriorityListByFlter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                    }
                    else
                    {
                        var AllFilterPriority = this._FormTemplateRepo.GetAllPrioritySetup(filter);
                        var PriorityNames = AllFilterPriority.Where(x => x.PRIORITY_EDESC.ToLower().Contains(filter));
                        var StartWithPriorityName = PriorityNames.Where(x => x.PRIORITY_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithPriorityName.Select(x => x.PRIORITY_EDESC.ToLower()).ToList();
                        var EndWithPriorityName = PriorityNames.Where(x => !startWithNames.Contains(x.PRIORITY_EDESC.ToLower()) && x.PRIORITY_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithPriorityName.Select(x => x.PRIORITY_EDESC.ToLower()).ToList();
                        var ContainsPriorityName = PriorityNames.Where(x => !startWithNames.Contains(x.PRIORITY_EDESC.ToLower()) && !endWithNames.Contains(x.PRIORITY_EDESC.ToLower()));
                        StartWithPriorityName.ForEach(s => s.Type = "PriorityName");
                        EndWithPriorityName.ToList().ForEach(s => s.Type = "PriorityName");
                        ContainsPriorityName.ToList().ForEach(s => s.Type = "PriorityName");
                        Filterdata.AddRange(StartWithPriorityName);
                        Filterdata.AddRange(ContainsPriorityName);
                        Filterdata.AddRange(EndWithPriorityName);
                        this._cacheManager.Set($"GetPriorityListByFlter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 30);
                        return Filterdata;
                    }
                }

            }

            else
            {
                var AllFilterPriority = this._FormTemplateRepo.GetAllPrioritySetup(filter);
                if (filter == null)
                    return AllFilterPriority;
                if (AllFilterPriority.Count >= 1)
                {
                    if (this._cacheManager.IsSet($"GetPriorityListByFlter_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        Filterdata = this._cacheManager.Get<List<Priority>>($"GetPriorityListByFlter_{userid}_{company_code}_{branch_code}_{filter}");
                    }
                    else
                    {
                        var PriorityCodes = AllFilterPriority.Where(x => x.PRIORITY_CODE.ToLower().Contains(filter));
                        var StartWithPriorityCode = PriorityCodes.Where(x => x.PRIORITY_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithPriorityCode.Select(x => x.PRIORITY_CODE.ToLower()).ToList();
                        var EndWithPriorityCode = PriorityCodes.Where(x => !startWithCodes.Contains(x.PRIORITY_CODE.ToLower()) && x.PRIORITY_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithPriorityCode.Select(x => x.PRIORITY_EDESC.ToLower()).ToList();
                        var ContainsPriorityCode = PriorityCodes.Where(x => !startWithCodes.Contains(x.PRIORITY_CODE.ToLower()) && !endWithCodes.Contains(x.PRIORITY_CODE.ToLower()));
                        Filterdata.AddRange(StartWithPriorityCode);
                        Filterdata.AddRange(ContainsPriorityCode);
                        Filterdata.AddRange(EndWithPriorityCode);
                        Filterdata.ForEach(s => s.Type = "Code");
                        var Removedata = AllFilterPriority.RemoveAll(x => x.PRIORITY_CODE.ToLower().Contains(filter));
                        var PriorityNames = AllFilterPriority.Where(x => x.PRIORITY_EDESC.ToLower().Contains(filter));
                        var StartWithPriorityName = PriorityNames.Where(x => x.PRIORITY_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithPriorityName.Select(x => x.PRIORITY_EDESC.ToLower()).ToList();
                        var EndWithPriorityName = PriorityNames.Where(x => !startWithNames.Contains(x.PRIORITY_EDESC.ToLower()) && x.PRIORITY_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithPriorityName.Select(x => x.PRIORITY_EDESC.ToLower()).ToList();
                        var ContainsPriorityName = PriorityNames.Where(x => !startWithNames.Contains(x.PRIORITY_EDESC.ToLower()) && !endWithNames.Contains(x.PRIORITY_EDESC.ToLower()));
                        StartWithPriorityName.ForEach(s => s.Type = "Name");
                        EndWithPriorityName.ToList().ForEach(s => s.Type = "Name");
                        ContainsPriorityName.ToList().ForEach(s => s.Type = "Name");
                        Filterdata.AddRange(StartWithPriorityName);
                        Filterdata.AddRange(ContainsPriorityName);
                        Filterdata.AddRange(EndWithPriorityName);
                        this._cacheManager.Set($"GetPriorityListByFlter_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 30);
                        return Filterdata;
                    }
                }
                return AllFilterPriority;
            }
            return Filterdata;
        }

        [HttpGet]
        public List<Agent> GetAgentListByFlter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var Filterdata = new List<Agent>();
            if (filter == "!@$")
                return Filterdata;
            if (filter == null)
                return this._FormTemplateRepo.GetAllAgentSetup(filter);
            if (filter.Contains("#"))
            {
                var colName = filter.Split('#')[0].ToString().ToLower();
                filter = filter.Split('#')[1].ToString().ToLower();
                if (colName == "code")
                {
                    if (this._cacheManager.IsSet($"GetAllAgentSetup{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        Filterdata = this._cacheManager.Get<List<Agent>>($"GetAllAgentSetup{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                    }
                    else
                    {
                        var AllFilterAgent = this._FormTemplateRepo.GetAllAgentSetup(filter);
                        var AgentCodes = AllFilterAgent.Where(x => x.AGENT_CODE.ToLower().Contains(filter));
                        var StartWithAgentCode = AgentCodes.Where(x => x.AGENT_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithAgentCode.Select(x => x.AGENT_CODE.ToLower()).ToList();
                        var EndWithAgentCode = AgentCodes.Where(x => !startWithCodes.Contains(x.AGENT_CODE.ToLower()) && x.AGENT_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithAgentCode.Select(x => x.AGENT_EDESC.ToLower()).ToList();
                        var ContainsAgentCode = AgentCodes.Where(x => !startWithCodes.Contains(x.AGENT_CODE.ToLower()) && !endWithCodes.Contains(x.AGENT_CODE.ToLower()));
                        Filterdata.AddRange(StartWithAgentCode);
                        Filterdata.AddRange(ContainsAgentCode);
                        Filterdata.AddRange(EndWithAgentCode);
                        Filterdata.ForEach(s => s.AGENT_TYPE = "AgentCode");
                        this._cacheManager.Set($"GetAgentListByFlter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 30);
                        return Filterdata;
                    }
                }
                if (colName == "name")
                {
                    if (this._cacheManager.IsSet($"GetAgentListByFlter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        Filterdata = this._cacheManager.Get<List<Agent>>($"GetAgentListByFlter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                    }
                    else
                    {
                        var AllFilterAgent = this._FormTemplateRepo.GetAllAgentSetup(filter);
                        var AgentNames = AllFilterAgent.Where(x => x.AGENT_EDESC.ToLower().Contains(filter));
                        var StartWithAgentName = AgentNames.Where(x => x.AGENT_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithAgentName.Select(x => x.AGENT_EDESC.ToLower()).ToList();
                        var EndWithAgentName = AgentNames.Where(x => !startWithNames.Contains(x.AGENT_EDESC.ToLower()) && x.AGENT_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithAgentName.Select(x => x.AGENT_EDESC.ToLower()).ToList();
                        var ContainsAgentName = AgentNames.Where(x => !startWithNames.Contains(x.AGENT_EDESC.ToLower()) && !endWithNames.Contains(x.AGENT_EDESC.ToLower()));
                        StartWithAgentName.ForEach(s => s.AGENT_TYPE = "AgentName");
                        EndWithAgentName.ToList().ForEach(s => s.AGENT_TYPE = "AgentName");
                        ContainsAgentName.ToList().ForEach(s => s.AGENT_TYPE = "AgentName");
                        Filterdata.AddRange(StartWithAgentName);
                        Filterdata.AddRange(ContainsAgentName);
                        Filterdata.AddRange(EndWithAgentName);
                        this._cacheManager.Set($"GetAgentListByFlter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 30);
                        return Filterdata;
                    }
                }

            }

            else
            {
                var AllFilterAgent = this._FormTemplateRepo.GetAllAgentSetup(filter);
                if (filter == null)
                    return AllFilterAgent;
                if (AllFilterAgent.Count >= 1)
                {
                    if (this._cacheManager.IsSet($"GetAgentListByFlter_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        Filterdata = this._cacheManager.Get<List<Agent>>($"GetAgentListByFlter_{userid}_{company_code}_{branch_code}_{filter}");
                    }
                    else
                    {
                        var AgentCodes = AllFilterAgent.Where(x => x.AGENT_CODE.ToLower().Contains(filter));
                        var StartWithAgentCode = AgentCodes.Where(x => x.AGENT_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithAgentCode.Select(x => x.AGENT_CODE.ToLower()).ToList();
                        var EndWithAgentCode = AgentCodes.Where(x => !startWithCodes.Contains(x.AGENT_CODE.ToLower()) && x.AGENT_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithAgentCode.Select(x => x.AGENT_EDESC.ToLower()).ToList();
                        var ContainsAgentCode = AgentCodes.Where(x => !startWithCodes.Contains(x.AGENT_CODE.ToLower()) && !endWithCodes.Contains(x.AGENT_CODE.ToLower()));
                        Filterdata.AddRange(StartWithAgentCode);
                        Filterdata.AddRange(ContainsAgentCode);
                        Filterdata.AddRange(EndWithAgentCode);
                        Filterdata.ForEach(s => s.AGENT_TYPE = "Code");
                        var Removedata = AllFilterAgent.RemoveAll(x => x.AGENT_CODE.ToLower().Contains(filter));
                        var AgentNames = AllFilterAgent.Where(x => x.AGENT_EDESC.ToLower().Contains(filter));
                        var StartWithAgentName = AgentNames.Where(x => x.AGENT_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithAgentName.Select(x => x.AGENT_EDESC.ToLower()).ToList();
                        var EndWithAgentName = AgentNames.Where(x => !startWithNames.Contains(x.AGENT_EDESC.ToLower()) && x.AGENT_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithAgentName.Select(x => x.AGENT_EDESC.ToLower()).ToList();
                        var ContainsAgentName = AgentNames.Where(x => !startWithNames.Contains(x.AGENT_EDESC.ToLower()) && !endWithNames.Contains(x.AGENT_EDESC.ToLower()));
                        StartWithAgentName.ForEach(s => s.AGENT_TYPE = "Name");
                        EndWithAgentName.ToList().ForEach(s => s.AGENT_TYPE = "Name");
                        ContainsAgentName.ToList().ForEach(s => s.AGENT_TYPE = "Name");
                        Filterdata.AddRange(StartWithAgentName);
                        Filterdata.AddRange(ContainsAgentName);
                        Filterdata.AddRange(EndWithAgentName);
                        this._cacheManager.Set($"GetAgentListByFlter_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 30);
                        return Filterdata;
                    }
                }
                return AllFilterAgent;
            }
            return Filterdata;
        }

        [HttpGet]
        public List<SalesType> GetsaleTypeListByFilter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var Filterdata = new List<SalesType>();
            if (filter == "!@$")
                return Filterdata;
            if (filter == null)
                return this._FormTemplateRepo.GetAllSalesTypeSetup(filter);
            if (filter.Contains("#"))
            {
                var colName = filter.Split('#')[0].ToString().ToLower();
                filter = filter.Split('#')[1].ToString().ToLower();
                if (colName == "code")
                {
                    if (this._cacheManager.IsSet($"GetsaleTypeListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        Filterdata = this._cacheManager.Get<List<SalesType>>($"GetsaleTypeListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                    }
                    else
                    {
                        var AllFilterSalesType = this._FormTemplateRepo.GetAllSalesTypeSetup(filter);
                        var SalesTypeCodes = AllFilterSalesType.Where(x => x.SALES_TYPE_CODE.ToLower().Contains(filter));
                        var StartWithSalesTypeCode = SalesTypeCodes.Where(x => x.SALES_TYPE_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithSalesTypeCode.Select(x => x.SALES_TYPE_CODE.ToLower()).ToList();
                        var EndWithSalesTypeCode = SalesTypeCodes.Where(x => !startWithCodes.Contains(x.SALES_TYPE_CODE.ToLower()) && x.SALES_TYPE_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithSalesTypeCode.Select(x => x.SALES_TYPE_EDESC.ToLower()).ToList();
                        var ContainsSalesTypeCode = SalesTypeCodes.Where(x => !startWithCodes.Contains(x.SALES_TYPE_CODE.ToLower()) && !endWithCodes.Contains(x.SALES_TYPE_CODE.ToLower()));
                        Filterdata.AddRange(StartWithSalesTypeCode);
                        Filterdata.AddRange(ContainsSalesTypeCode);
                        Filterdata.AddRange(EndWithSalesTypeCode);
                        Filterdata.ForEach(s => s.Type = "SalesTypeCode");
                        this._cacheManager.Set($"GetsaleTypeListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 30);
                        return Filterdata;
                    }
                }
                if (colName == "name")
                {
                    if (this._cacheManager.IsSet($"GetsaleTypeListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        Filterdata = this._cacheManager.Get<List<SalesType>>($"GetsaleTypeListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                    }
                    else
                    {
                        var AllFilterSalesType = this._FormTemplateRepo.GetAllSalesTypeSetup(filter);
                        var SalesTypeNames = AllFilterSalesType.Where(x => x.SALES_TYPE_EDESC.ToLower().Contains(filter));
                        var StartWithSalesTypeName = SalesTypeNames.Where(x => x.SALES_TYPE_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithSalesTypeName.Select(x => x.SALES_TYPE_EDESC.ToLower()).ToList();
                        var EndWithSalesTypeName = SalesTypeNames.Where(x => !startWithNames.Contains(x.SALES_TYPE_EDESC.ToLower()) && x.SALES_TYPE_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithSalesTypeName.Select(x => x.SALES_TYPE_EDESC.ToLower()).ToList();
                        var ContainsSalesTypeName = SalesTypeNames.Where(x => !startWithNames.Contains(x.SALES_TYPE_EDESC.ToLower()) && !endWithNames.Contains(x.SALES_TYPE_EDESC.ToLower()));
                        StartWithSalesTypeName.ForEach(s => s.Type = "SalesTypeName");
                        EndWithSalesTypeName.ToList().ForEach(s => s.Type = "SalesTypeName");
                        ContainsSalesTypeName.ToList().ForEach(s => s.Type = "SalesTypeName");
                        Filterdata.AddRange(StartWithSalesTypeName);
                        Filterdata.AddRange(ContainsSalesTypeName);
                        Filterdata.AddRange(EndWithSalesTypeName);
                        this._cacheManager.Set($"GetsaleTypeListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 30);
                        return Filterdata;
                    }
                }
            }
            else
            {
                var AllFilterSalesType = this._FormTemplateRepo.GetAllSalesTypeSetup(filter);
                if (filter == null)
                    return AllFilterSalesType;
                if (AllFilterSalesType.Count >= 1)
                {
                    if (this._cacheManager.IsSet($"GetsaleTypeListByFilter_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        Filterdata = this._cacheManager.Get<List<SalesType>>($"GetsaleTypeListByFilter_{userid}_{company_code}_{branch_code}_{filter}");
                    }
                    else
                    {
                        var SalesTypeCodes = AllFilterSalesType.Where(x => x.SALES_TYPE_CODE.ToLower().Contains(filter));
                        var StartWithSalesTypeCode = SalesTypeCodes.Where(x => x.SALES_TYPE_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithSalesTypeCode.Select(x => x.SALES_TYPE_CODE.ToLower()).ToList();
                        var EndWithSalesTypeCode = SalesTypeCodes.Where(x => !startWithCodes.Contains(x.SALES_TYPE_CODE.ToLower()) && x.SALES_TYPE_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithSalesTypeCode.Select(x => x.SALES_TYPE_EDESC.ToLower()).ToList();
                        var ContainsSalesTypeCode = SalesTypeCodes.Where(x => !startWithCodes.Contains(x.SALES_TYPE_CODE.ToLower()) && !endWithCodes.Contains(x.SALES_TYPE_CODE.ToLower()));
                        Filterdata.AddRange(StartWithSalesTypeCode);
                        Filterdata.AddRange(ContainsSalesTypeCode);
                        Filterdata.AddRange(EndWithSalesTypeCode);
                        Filterdata.ForEach(s => s.Type = "SalesTypeCode");
                        var Removedata = AllFilterSalesType.RemoveAll(x => x.SALES_TYPE_CODE.ToLower().Contains(filter));
                        var SalesTypeNames = AllFilterSalesType.Where(x => x.SALES_TYPE_EDESC.ToLower().Contains(filter));
                        var StartWithSalesTypeName = SalesTypeNames.Where(x => x.SALES_TYPE_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithSalesTypeName.Select(x => x.SALES_TYPE_EDESC.ToLower()).ToList();
                        var EndWithSalesTypeName = SalesTypeNames.Where(x => !startWithNames.Contains(x.SALES_TYPE_EDESC.ToLower()) && x.SALES_TYPE_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithSalesTypeName.Select(x => x.SALES_TYPE_EDESC.ToLower()).ToList();
                        var ContainsSalesTypeName = SalesTypeNames.Where(x => !startWithNames.Contains(x.SALES_TYPE_EDESC.ToLower()) && !endWithNames.Contains(x.SALES_TYPE_EDESC.ToLower()));
                        StartWithSalesTypeName.ForEach(s => s.Type = "SalesTypeName");
                        EndWithSalesTypeName.ToList().ForEach(s => s.Type = "SalesTypeName");
                        ContainsSalesTypeName.ToList().ForEach(s => s.Type = "SalesTypeName");
                        Filterdata.AddRange(StartWithSalesTypeName);
                        Filterdata.AddRange(ContainsSalesTypeName);
                        Filterdata.AddRange(EndWithSalesTypeName);
                        this._cacheManager.Set($"GetsaleTypeListByFilter_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 30);
                        return Filterdata;
                    }
                }
                return AllFilterSalesType;
            }
            return Filterdata;
        }
        [HttpGet]
        public List<Division> GetdivisionListByFilter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var Filterdata = new List<Division>();
            if (filter == "!@$")
                return Filterdata;
            if (filter == null)
                filter = "";
            if (filter.Contains("#"))
            {
                var colName = filter.Split('#')[0].ToString().ToLower();
                filter = filter.Split('#')[1].ToString().ToLower();
                if (colName == "code")
                {
                    if (this._cacheManager.IsSet($"GetdivisionListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        Filterdata = this._cacheManager.Get<List<Division>>($"GetdivisionListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                    }
                    else
                    {
                        var AllFilterDivision = this._FormTemplateRepo.GetAllDivisionSetup(filter);
                        var DivisionCodes = AllFilterDivision.Where(x => x.DIVISION_CODE.ToLower().Contains(filter));
                        var StartWithDivisionCode = DivisionCodes.Where(x => x.DIVISION_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithDivisionCode.Select(x => x.DIVISION_CODE.ToLower()).ToList();
                        var EndWithDivisionCode = DivisionCodes.Where(x => !startWithCodes.Contains(x.DIVISION_CODE.ToLower()) && x.DIVISION_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithDivisionCode.Select(x => x.DIVISION_EDESC.ToLower()).ToList();
                        var ContainsDivisionCode = DivisionCodes.Where(x => !startWithCodes.Contains(x.DIVISION_CODE.ToLower()) && !endWithCodes.Contains(x.DIVISION_CODE.ToLower()));

                        Filterdata.AddRange(StartWithDivisionCode);
                        Filterdata.AddRange(ContainsDivisionCode);
                        Filterdata.AddRange(EndWithDivisionCode);
                        Filterdata.ForEach(s => s.Type = "Code");
                        this._cacheManager.Set($"GetdivisionListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 30);
                        return Filterdata;
                    }
                }
                if (colName == "name")
                {
                    if (this._cacheManager.IsSet($"GetdivisionListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        Filterdata = this._cacheManager.Get<List<Division>>($"GetdivisionListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                    }
                    else
                    {
                        var AllFilterDivision = this._FormTemplateRepo.GetAllDivisionSetup(filter);
                        var DivisionNames = AllFilterDivision.Where(x => x.DIVISION_EDESC.ToLower().Contains(filter));
                        var StartWithDivisionName = DivisionNames.Where(x => x.DIVISION_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithDivisionName.Select(x => x.DIVISION_EDESC.ToLower()).ToList();
                        var EndWithDivisionName = DivisionNames.Where(x => !startWithNames.Contains(x.DIVISION_EDESC.ToLower()) && x.DIVISION_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithDivisionName.Select(x => x.DIVISION_EDESC.ToLower()).ToList();
                        var ContainsDivisionName = DivisionNames.Where(x => !startWithNames.Contains(x.DIVISION_EDESC.ToLower()) && !endWithNames.Contains(x.DIVISION_EDESC.ToLower()));
                        StartWithDivisionName.ForEach(s => s.Type = "Name");
                        EndWithDivisionName.ToList().ForEach(s => s.Type = "Name");
                        ContainsDivisionName.ToList().ForEach(s => s.Type = "Name");
                        Filterdata.AddRange(StartWithDivisionName);
                        Filterdata.AddRange(ContainsDivisionName);
                        Filterdata.AddRange(EndWithDivisionName);
                        this._cacheManager.Set($"GetdivisionListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 30);
                        return Filterdata;
                    }
                }
                if (colName == "address")
                {
                    if (this._cacheManager.IsSet($"GetdivisionListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        Filterdata = this._cacheManager.Get<List<Division>>($"GetdivisionListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                    }
                    else
                    {
                        var AllFilterAddress = this._FormTemplateRepo.GetAllDivisionSetup(filter);
                        var Address = AllFilterAddress.Where(x => x.ADDRESS.ToLower().Contains(filter));
                        var StartWithAddressName = Address.Where(x => x.ADDRESS.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithAddress = StartWithAddressName.Select(x => x.ADDRESS.ToLower()).ToList();
                        var EndWithAddressName = Address.Where(x => !startWithAddress.Contains(x.ADDRESS.ToLower()) && x.ADDRESS.ToLower().EndsWith(filter.Trim()));
                        var endWithAddress = EndWithAddressName.Select(x => x.ADDRESS.ToLower()).ToList();
                        var ContainsAddressName = Address.Where(x => !startWithAddress.Contains(x.ADDRESS.ToLower()) && !endWithAddress.Contains(x.ADDRESS.ToLower()));
                        StartWithAddressName.ForEach(s => s.Type = "Addr");
                        EndWithAddressName.ToList().ForEach(s => s.Type = "Addr");
                        ContainsAddressName.ToList().ForEach(s => s.Type = "Addr");
                        Filterdata.AddRange(StartWithAddressName);
                        Filterdata.AddRange(ContainsAddressName);
                        Filterdata.AddRange(EndWithAddressName);
                        this._cacheManager.Set($"GetdivisionListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 30);
                        return Filterdata;
                    }
                }
                if (colName == "phoneno")
                {
                    if (this._cacheManager.IsSet($"GetdivisionListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}"))
                    {
                        Filterdata = this._cacheManager.Get<List<Division>>($"GetdivisionListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}");
                    }
                    else
                    {
                        var AllFilterPhoneNo = this._FormTemplateRepo.GetAllDivisionSetup(filter);
                        var PhoneNoNames = AllFilterPhoneNo.Where(x => x.TELEPHONE_NO.ToLower().Contains(filter));
                        var StartWithPhoneNoName = PhoneNoNames.Where(x => x.TELEPHONE_NO.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithPhoneNoName.Select(x => x.TELEPHONE_NO.ToLower()).ToList();
                        var EndWithPhoneNoName = PhoneNoNames.Where(x => !startWithNames.Contains(x.TELEPHONE_NO.ToLower()) && x.TELEPHONE_NO.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithPhoneNoName.Select(x => x.TELEPHONE_NO.ToLower()).ToList();
                        var ContainsPhoneNoName = PhoneNoNames.Where(x => !startWithNames.Contains(x.TELEPHONE_NO.ToLower()) && !endWithNames.Contains(x.TELEPHONE_NO.ToLower()));
                        StartWithPhoneNoName.ForEach(s => s.Type = "Ph");
                        EndWithPhoneNoName.ToList().ForEach(s => s.Type = "Ph");
                        ContainsPhoneNoName.ToList().ForEach(s => s.Type = "Ph");
                        Filterdata.AddRange(StartWithPhoneNoName);
                        Filterdata.AddRange(ContainsPhoneNoName);
                        Filterdata.AddRange(EndWithPhoneNoName);
                        this._cacheManager.Set($"GetdivisionListByFilter_{userid}_{company_code}_{branch_code}_{filter}_{colName}", Filterdata, 30);
                        return Filterdata;
                    }
                }
            }
            else
            {
                var AllFilterDivision = this._FormTemplateRepo.GetAllDivisionSetup(filter);
                if (AllFilterDivision.Count >= 1)
                {
                    if (this._cacheManager.IsSet($"GetdivisionListByFilter_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        Filterdata = this._cacheManager.Get<List<Division>>($"GetdivisionListByFilter_{userid}_{company_code}_{branch_code}_{filter}");
                    }
                    else
                    {
                        var DivisionCodes = AllFilterDivision.Where(x => x.DIVISION_CODE.ToLower().Contains(filter));
                        var StartWithDivisionCode = DivisionCodes.Where(x => x.DIVISION_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithDivisionCode.Select(x => x.DIVISION_CODE.ToLower()).ToList();
                        var EndWithDivisionCode = DivisionCodes.Where(x => !startWithCodes.Contains(x.DIVISION_CODE.ToLower()) && x.DIVISION_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithDivisionCode.Select(x => x.DIVISION_EDESC.ToLower()).ToList();
                        var ContainsDivisionCode = DivisionCodes.Where(x => !startWithCodes.Contains(x.DIVISION_CODE.ToLower()) && !endWithCodes.Contains(x.DIVISION_CODE.ToLower()));
                        Filterdata.AddRange(StartWithDivisionCode);
                        Filterdata.AddRange(ContainsDivisionCode);
                        Filterdata.AddRange(EndWithDivisionCode);
                        Filterdata.ForEach(s => s.Type = "Code");
                        var Removedata = AllFilterDivision.RemoveAll(x => x.DIVISION_CODE.ToLower().Contains(filter));
                        var DivisionNames = AllFilterDivision.Where(x => x.DIVISION_EDESC.ToLower().Contains(filter));
                        var StartWithDivisionName = DivisionNames.Where(x => x.DIVISION_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithDivisionName.Select(x => x.DIVISION_EDESC.ToLower()).ToList();
                        var EndWithDivisionName = DivisionNames.Where(x => !startWithNames.Contains(x.DIVISION_EDESC.ToLower()) && x.DIVISION_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithDivisionName.Select(x => x.DIVISION_EDESC.ToLower()).ToList();
                        var ContainsDivisionName = DivisionNames.Where(x => !startWithNames.Contains(x.DIVISION_EDESC.ToLower()) && !endWithNames.Contains(x.DIVISION_EDESC.ToLower()));
                        StartWithDivisionName.ForEach(s => s.Type = "Name");
                        EndWithDivisionName.ToList().ForEach(s => s.Type = "Name");
                        ContainsDivisionName.ToList().ForEach(s => s.Type = "Name");
                        Filterdata.AddRange(StartWithDivisionName);
                        Filterdata.AddRange(ContainsDivisionName);
                        Filterdata.AddRange(EndWithDivisionName);
                        AllFilterDivision.RemoveAll(x => x.DIVISION_EDESC.ToLower().Contains(filter));
                        var Addresses = AllFilterDivision.Where(x => x.ADDRESS.ToLower().Contains(filter));
                        var StartWithAddress = Addresses.Where(x => x.ADDRESS.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithAddress = StartWithAddress.Select(x => x.ADDRESS.ToLower()).ToList();
                        var EndWithAddress = Addresses.Where(x => !startWithAddress.Contains(x.ADDRESS.ToLower()) && x.ADDRESS.ToLower().EndsWith(filter.Trim()));
                        var endWithAddress = EndWithAddress.Select(x => x.ADDRESS.ToLower()).ToList();
                        var ContainsAddress = Addresses.Where(x => !startWithAddress.Contains(x.ADDRESS.ToLower()) && !endWithAddress.Contains(x.ADDRESS.ToLower()));
                        StartWithAddress.ForEach(s => s.Type = "Addr");
                        EndWithAddress.ToList().ForEach(s => s.Type = "Addr");
                        ContainsAddress.ToList().ForEach(s => s.Type = "Addr");
                        Filterdata.AddRange(StartWithAddress);
                        Filterdata.AddRange(ContainsAddress);
                        Filterdata.AddRange(EndWithAddress);
                        AllFilterDivision.RemoveAll(x => x.ADDRESS.ToLower().Contains(filter));
                        var PhoneNoNames = AllFilterDivision.Where(x => x.TELEPHONE_NO.ToLower().Contains(filter));
                        var StartWithPhoneNoName = PhoneNoNames.Where(x => x.TELEPHONE_NO.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithPhone = StartWithPhoneNoName.Select(x => x.TELEPHONE_NO.ToLower()).ToList();
                        var EndWithPhoneNoName = PhoneNoNames.Where(x => !startWithPhone.Contains(x.TELEPHONE_NO.ToLower()) && x.TELEPHONE_NO.ToLower().EndsWith(filter.Trim()));
                        var endWithPhone = EndWithPhoneNoName.Select(x => x.TELEPHONE_NO.ToLower()).ToList();
                        var ContainsPhoneNoName = PhoneNoNames.Where(x => !startWithPhone.Contains(x.TELEPHONE_NO.ToLower()) && !endWithPhone.Contains(x.TELEPHONE_NO.ToLower()));
                        StartWithPhoneNoName.ForEach(s => s.Type = "Ph");
                        EndWithPhoneNoName.ToList().ForEach(s => s.Type = "Ph");
                        ContainsPhoneNoName.ToList().ForEach(s => s.Type = "Ph");
                        Filterdata.AddRange(StartWithPhoneNoName);
                        Filterdata.AddRange(ContainsPhoneNoName);
                        Filterdata.AddRange(EndWithPhoneNoName);
                        this._cacheManager.Set($"GetdivisionListByFilter_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 30);
                        return Filterdata;
                    }
                }
                return AllFilterDivision;
            }
            return Filterdata;
        }
        [HttpGet]
        public List<Location> GetAllLocation()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<Location>();
            if (this._cacheManager.IsSet($"GetAllLocation_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<Location>>($"GetAllLocation_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var AllLocationList = this._FormTemplateRepo.GetAllLocation();
                this._cacheManager.Set($"GetAllLocation_{userid}_{company_code}_{branch_code}", AllLocationList, 20);
                response = AllLocationList;
            }
            return response;
        }
        [HttpGet]
        public List<Currency> GetCurrencyListByFlter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<Currency>();
            if (this._cacheManager.IsSet($"GetCurrencyListByFlter_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<Currency>>($"GetCurrencyListByFlter_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var CurrencyList = this._FormTemplateRepo.getCurrencyListByFlter(filter);
                this._cacheManager.Set($"GetCurrencyListByFlter_{userid}_{company_code}_{branch_code}_{filter}", CurrencyList, 20);
                response = CurrencyList;
            }
            return response;
        }
        [HttpGet]
        public List<PaymentMode> GetPaymentModeListByFlter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<PaymentMode>();
            if (this._cacheManager.IsSet($"GetPaymentModeListByFlterr_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<PaymentMode>>($"GetPaymentModeListByFlterr_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var PaymentModeList = this._FormTemplateRepo.getPaymentModeListByFlter(filter);
                this._cacheManager.Set($"GetPaymentModeListByFlterr_{userid}_{company_code}_{branch_code}_{filter}", PaymentModeList, 20);
                response = PaymentModeList;
            }
            return response;
        }
        [HttpGet]
        public List<Brand> GetBrandListByFlter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<Brand>();
            if (this._cacheManager.IsSet($"GetBrandListByFlter_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<Brand>>($"GetBrandListByFlter_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var BrandList = this._FormTemplateRepo.getBrandListByFlter(filter);
                this._cacheManager.Set($"GetBrandListByFlter_{userid}_{company_code}_{branch_code}_{filter}", BrandList, 20);
                response = BrandList;
            }
            return response;
        }
        [HttpGet]
        public List<SubLedger> GetAllSubLedgerByFilter(string filter, string accCode)
        {
            var Filterdata = new List<SubLedger>();
            if (filter == null)
                filter = "";
            if (filter.Contains("#"))
            {
                var colName = filter.Split('#')[0].ToString().ToLower();
                filter = filter.Split('#')[1].ToString().ToLower();
                if (colName == "code")
                {
                    var AllFilterSubledger = this._FormTemplateRepo.GetAllSubLedgerByFilter(filter, accCode);
                    var SubledgerCodes = AllFilterSubledger.Where(x => x.SUB_EDESC.ToLower().Contains(filter));
                    var StartWithSubledgerCode = SubledgerCodes.Where(x => x.SUB_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                    var startWithCodes = StartWithSubledgerCode.Select(x => x.SUB_CODE.ToLower()).ToList();
                    var EndWithSubledgerCode = SubledgerCodes.Where(x => !startWithCodes.Contains(x.SUB_CODE.ToLower()) && x.SUB_CODE.ToLower().EndsWith(filter.Trim()));
                    var endWithCodes = EndWithSubledgerCode.Select(x => x.SUB_EDESC.ToLower()).ToList();
                    var ContainsSubledgerCode = SubledgerCodes.Where(x => !startWithCodes.Contains(x.SUB_CODE.ToLower()) && !endWithCodes.Contains(x.SUB_CODE.ToLower()));

                    Filterdata.AddRange(StartWithSubledgerCode);
                    Filterdata.AddRange(ContainsSubledgerCode);
                    Filterdata.AddRange(EndWithSubledgerCode);
                    Filterdata.ForEach(s => s.Type = "Code");
                    return Filterdata;
                }
                if (colName == "name")
                {
                    var AllFilterSubledger = this._FormTemplateRepo.GetAllSubLedgerByFilter(filter, accCode);
                    var SubledgerNames = AllFilterSubledger.Where(x => x.SUB_EDESC.ToLower().Contains(filter));
                    var StartWithSubledgerName = SubledgerNames.Where(x => x.SUB_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                    var startWithNames = StartWithSubledgerName.Select(x => x.SUB_EDESC.ToLower()).ToList();
                    var EndWithSubledgerName = SubledgerNames.Where(x => !startWithNames.Contains(x.SUB_EDESC.ToLower()) && x.SUB_EDESC.ToLower().EndsWith(filter.Trim()));
                    var endWithNames = EndWithSubledgerName.Select(x => x.SUB_EDESC.ToLower()).ToList();
                    var ContainsSubledgerName = SubledgerNames.Where(x => !startWithNames.Contains(x.SUB_EDESC.ToLower()) && !endWithNames.Contains(x.SUB_EDESC.ToLower()));
                    StartWithSubledgerName.ForEach(s => s.Type = "Name");
                    EndWithSubledgerName.ToList().ForEach(s => s.Type = "Name");
                    ContainsSubledgerName.ToList().ForEach(s => s.Type = "Name");
                    Filterdata.AddRange(StartWithSubledgerName);
                    Filterdata.AddRange(ContainsSubledgerName);
                    Filterdata.AddRange(EndWithSubledgerName);
                    return Filterdata;
                }
            }
            else
            {
                var AllFilterSubledger = this._FormTemplateRepo.GetAllSubLedgerByFilter(filter, accCode);
                if (AllFilterSubledger.Count >= 1)
                {
                    var SubledgerCodes = AllFilterSubledger.Where(x => x.SUB_CODE.ToLower().Contains(filter));
                    var StartWithSubledgerCode = SubledgerCodes.Where(x => x.SUB_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                    var startWithCodes = StartWithSubledgerCode.Select(x => x.SUB_CODE.ToLower()).ToList();
                    var EndWithSubledgerCode = SubledgerCodes.Where(x => !startWithCodes.Contains(x.SUB_CODE.ToLower()) && x.SUB_CODE.ToLower().EndsWith(filter.Trim()));
                    var endWithCodes = EndWithSubledgerCode.Select(x => x.SUB_EDESC.ToLower()).ToList();
                    var ContainsSubledgerCode = SubledgerCodes.Where(x => !startWithCodes.Contains(x.SUB_CODE.ToLower()) && !endWithCodes.Contains(x.SUB_CODE.ToLower()));
                    Filterdata.AddRange(StartWithSubledgerCode);
                    Filterdata.AddRange(ContainsSubledgerCode);
                    Filterdata.AddRange(EndWithSubledgerCode);
                    Filterdata.ForEach(s => s.Type = "Code");
                    var Removedata = AllFilterSubledger.RemoveAll(x => x.SUB_CODE.ToLower().Contains(filter));
                    var SubledgerNames = AllFilterSubledger.Where(x => x.SUB_EDESC.ToLower().Contains(filter));
                    var StartWithSubledgerName = SubledgerNames.Where(x => x.SUB_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                    var startWithNames = StartWithSubledgerName.Select(x => x.SUB_EDESC.ToLower()).ToList();
                    var EndWithSubledgerName = SubledgerNames.Where(x => !startWithNames.Contains(x.SUB_EDESC.ToLower()) && x.SUB_EDESC.ToLower().EndsWith(filter.Trim()));
                    var endWithNames = EndWithSubledgerName.Select(x => x.SUB_EDESC.ToLower()).ToList();
                    var ContainsSubledgerName = SubledgerNames.Where(x => !startWithNames.Contains(x.SUB_EDESC.ToLower()) && !endWithNames.Contains(x.SUB_EDESC.ToLower()));
                    StartWithSubledgerName.ForEach(s => s.Type = "Name");
                    EndWithSubledgerName.ToList().ForEach(s => s.Type = "Name");
                    ContainsSubledgerName.ToList().ForEach(s => s.Type = "Name");
                    Filterdata.AddRange(StartWithSubledgerName);
                    Filterdata.AddRange(ContainsSubledgerName);
                    Filterdata.AddRange(EndWithSubledgerName);
                    return Filterdata;
                }
                return AllFilterSubledger;
            }
            return Filterdata;
        }
        [HttpGet]
        public List<SubLedger> GetAllSubLedgerByFilterPartyType(string filter, string partyTypeCode)
        {
            var Filterdata = new List<SubLedger>();
            if (filter == null)
                filter = "";
            if (filter.Contains("#"))
            {
                var colName = filter.Split('#')[0].ToString().ToLower();
                filter = filter.Split('#')[1].ToString().ToLower();
                if (colName == "code")
                {
                    var AllFilterSubledger = this._FormTemplateRepo.GetAllSubLedgerByFilterPartyType(filter, partyTypeCode);
                    var SubledgerCodes = AllFilterSubledger.Where(x => x.SUB_EDESC.ToLower().Contains(filter));
                    var StartWithSubledgerCode = SubledgerCodes.Where(x => x.SUB_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                    var startWithCodes = StartWithSubledgerCode.Select(x => x.SUB_CODE.ToLower()).ToList();
                    var EndWithSubledgerCode = SubledgerCodes.Where(x => !startWithCodes.Contains(x.SUB_CODE.ToLower()) && x.SUB_CODE.ToLower().EndsWith(filter.Trim()));
                    var endWithCodes = EndWithSubledgerCode.Select(x => x.SUB_EDESC.ToLower()).ToList();
                    var ContainsSubledgerCode = SubledgerCodes.Where(x => !startWithCodes.Contains(x.SUB_CODE.ToLower()) && !endWithCodes.Contains(x.SUB_CODE.ToLower()));

                    Filterdata.AddRange(StartWithSubledgerCode);
                    Filterdata.AddRange(ContainsSubledgerCode);
                    Filterdata.AddRange(EndWithSubledgerCode);
                    Filterdata.ForEach(s => s.Type = "Code");
                    return Filterdata;
                }
                if (colName == "name")
                {
                    var AllFilterSubledger = this._FormTemplateRepo.GetAllSubLedgerByFilterPartyType(filter, partyTypeCode);
                    var SubledgerNames = AllFilterSubledger.Where(x => x.SUB_EDESC.ToLower().Contains(filter));
                    var StartWithSubledgerName = SubledgerNames.Where(x => x.SUB_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                    var startWithNames = StartWithSubledgerName.Select(x => x.SUB_EDESC.ToLower()).ToList();
                    var EndWithSubledgerName = SubledgerNames.Where(x => !startWithNames.Contains(x.SUB_EDESC.ToLower()) && x.SUB_EDESC.ToLower().EndsWith(filter.Trim()));
                    var endWithNames = EndWithSubledgerName.Select(x => x.SUB_EDESC.ToLower()).ToList();
                    var ContainsSubledgerName = SubledgerNames.Where(x => !startWithNames.Contains(x.SUB_EDESC.ToLower()) && !endWithNames.Contains(x.SUB_EDESC.ToLower()));
                    StartWithSubledgerName.ForEach(s => s.Type = "Name");
                    EndWithSubledgerName.ToList().ForEach(s => s.Type = "Name");
                    ContainsSubledgerName.ToList().ForEach(s => s.Type = "Name");
                    Filterdata.AddRange(StartWithSubledgerName);
                    Filterdata.AddRange(ContainsSubledgerName);
                    Filterdata.AddRange(EndWithSubledgerName);
                    return Filterdata;
                }
            }
            else
            {
                var AllFilterSubledger = this._FormTemplateRepo.GetAllSubLedgerByFilterPartyType(filter, partyTypeCode);
                if (AllFilterSubledger.Count >= 1)
                {
                    var SubledgerCodes = AllFilterSubledger.Where(x => x.SUB_CODE.ToLower().Contains(filter));
                    var StartWithSubledgerCode = SubledgerCodes.Where(x => x.SUB_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                    var startWithCodes = StartWithSubledgerCode.Select(x => x.SUB_CODE.ToLower()).ToList();
                    var EndWithSubledgerCode = SubledgerCodes.Where(x => !startWithCodes.Contains(x.SUB_CODE.ToLower()) && x.SUB_CODE.ToLower().EndsWith(filter.Trim()));
                    var endWithCodes = EndWithSubledgerCode.Select(x => x.SUB_EDESC.ToLower()).ToList();
                    var ContainsSubledgerCode = SubledgerCodes.Where(x => !startWithCodes.Contains(x.SUB_CODE.ToLower()) && !endWithCodes.Contains(x.SUB_CODE.ToLower()));
                    Filterdata.AddRange(StartWithSubledgerCode);
                    Filterdata.AddRange(ContainsSubledgerCode);
                    Filterdata.AddRange(EndWithSubledgerCode);
                    Filterdata.ForEach(s => s.Type = "Code");
                    var Removedata = AllFilterSubledger.RemoveAll(x => x.SUB_CODE.ToLower().Contains(filter));
                    var SubledgerNames = AllFilterSubledger.Where(x => x.SUB_EDESC.ToLower().Contains(filter));
                    var StartWithSubledgerName = SubledgerNames.Where(x => x.SUB_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                    var startWithNames = StartWithSubledgerName.Select(x => x.SUB_EDESC.ToLower()).ToList();
                    var EndWithSubledgerName = SubledgerNames.Where(x => !startWithNames.Contains(x.SUB_EDESC.ToLower()) && x.SUB_EDESC.ToLower().EndsWith(filter.Trim()));
                    var endWithNames = EndWithSubledgerName.Select(x => x.SUB_EDESC.ToLower()).ToList();
                    var ContainsSubledgerName = SubledgerNames.Where(x => !startWithNames.Contains(x.SUB_EDESC.ToLower()) && !endWithNames.Contains(x.SUB_EDESC.ToLower()));
                    StartWithSubledgerName.ForEach(s => s.Type = "Name");
                    EndWithSubledgerName.ToList().ForEach(s => s.Type = "Name");
                    ContainsSubledgerName.ToList().ForEach(s => s.Type = "Name");
                    Filterdata.AddRange(StartWithSubledgerName);
                    Filterdata.AddRange(ContainsSubledgerName);
                    Filterdata.AddRange(EndWithSubledgerName);
                    return Filterdata;
                }
                return AllFilterSubledger;
            }
            return Filterdata;
        }
        [HttpGet]
        public List<AccountSetup> GetAllAccountForBud()
        {
            var result = this._FormTemplateRepo.getALLAccountForInvBudgetTrans();
            return result;
        }

        public List<AccountSetup> GetAllAccountForIntrestCalc()
        {
            var result = this._FormTemplateRepo.getALLAccountGroupForIntrestCalc();
            return result;
        }

        [HttpGet]
        public List<AccountSetup> GetAllAccountSetupByFilter(string filter)
        {
            if (string.IsNullOrEmpty(filter))
                return new List<AccountSetup>();

            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var Filterdata = new List<AccountSetup>();
            var ShowAdvanceAutoComplete = false;
            //Constants.
            var setting = _settingService.LoadSetting<WebPrefrenceSetting>(Constants.WebPrefranceSetting);
            if (setting != null)
                bool.TryParse(setting.ShowAdvanceAutoComplete, out ShowAdvanceAutoComplete);
            if (ShowAdvanceAutoComplete == false)
            {

                if (this._cacheManager.IsSet($"{DT}_AllFilterAccount_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}"))
                {
                    var data = _cacheManager.Get<List<AccountSetup>>($"{DT}_AllFilterAccount_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}");
                    Filterdata = data;
                    return Filterdata;
                }
                else
                {
                    var AllFilterAccount = this._FormTemplateRepo.getALLAccountSetupByFlter(filter);
                    this._cacheManager.Set($"{DT}_AllFilterAccount_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}", AllFilterAccount, 20);
                    return AllFilterAccount;
                }

            }
            if (filter == "!@$")
            {
                return Filterdata;
            }
            if (filter == null)
                filter = "";
            if (filter.Contains("#"))
            {
                var colName = filter.Split('#')[0].ToString().ToLower();
                filter = filter.Split('#')[1].ToString().ToLower();
                if (colName == "code")
                {
                    if (this._cacheManager.IsSet($"AllAccountSetupByCode_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<AccountSetup>>($"AllAccountSetupByCode_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterAccount = this._FormTemplateRepo.getALLAccountSetupByFlter(filter);
                        var AccountCodes = AllFilterAccount.Where(x => x.ACC_CODE.ToLower().Contains(filter));
                        var StartWithAccountCode = AccountCodes.Where(x => x.ACC_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithAccountCode.Select(x => x.ACC_CODE.ToLower()).ToList();
                        var EndWithAccountCode = AccountCodes.Where(x => !startWithCodes.Contains(x.ACC_CODE.ToLower()) && x.ACC_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithAccountCode.Select(x => x.ACC_EDESC.ToLower()).ToList();
                        var ContainsAccountCode = AccountCodes.Where(x => !startWithCodes.Contains(x.ACC_CODE.ToLower()) && !endWithCodes.Contains(x.ACC_CODE.ToLower()));
                        Filterdata.AddRange(StartWithAccountCode);
                        Filterdata.AddRange(ContainsAccountCode);
                        Filterdata.AddRange(EndWithAccountCode);
                        Filterdata.ForEach(s => s.Type = "AccountCode");
                        this._cacheManager.Set($"AllAccountSetupByCode_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }

                    return Filterdata;
                }
                if (colName == "name")
                {
                    if (this._cacheManager.IsSet($"{DT}_AllAccountSetupByName_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<AccountSetup>>($"AllAccountSetupByName_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterAccount = this._FormTemplateRepo.getALLAccountSetupByFlter(filter);
                        var AccountNames = AllFilterAccount.Where(x => x.ACC_EDESC.ToLower().Contains(filter));
                        var StartWithAccountName = AccountNames.Where(x => x.ACC_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithAccountName.Select(x => x.ACC_EDESC.ToLower()).ToList();
                        var EndWithAccountName = AccountNames.Where(x => !startWithNames.Contains(x.ACC_EDESC.ToLower()) && x.ACC_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithAccountName.Select(x => x.ACC_EDESC.ToLower()).ToList();
                        var ContainsAccountName = AccountNames.Where(x => !startWithNames.Contains(x.ACC_EDESC.ToLower()) && !endWithNames.Contains(x.ACC_EDESC.ToLower()));
                        StartWithAccountName.ForEach(s => s.Type = "AccountName");
                        EndWithAccountName.ToList().ForEach(s => s.Type = "AccountName");
                        ContainsAccountName.ToList().ForEach(s => s.Type = "AccountName");
                        Filterdata.AddRange(StartWithAccountName);
                        Filterdata.AddRange(ContainsAccountName);
                        Filterdata.AddRange(EndWithAccountName);
                        this._cacheManager.Set($"AllAccountSetupByName_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }

                    return Filterdata;
                }
            }
            else
            {
                if (this._cacheManager.IsSet($"{DT}_AllFilterAccount_{userid}_{company_code}_{branch_code}_{filter}"))
                {
                    var data = _cacheManager.Get<List<AccountSetup>>($"AllFilterAccount_{userid}_{company_code}_{branch_code}_{filter}");
                    Filterdata = data;
                }
                else
                {
                    var AllFilterAccount = this._FormTemplateRepo.getALLAccountSetupByFlter(filter);
                    if (AllFilterAccount.Count >= 1)
                    {
                        var AccountCodes = AllFilterAccount.Where(x => x.ACC_CODE.ToLower().Contains(filter));
                        var StartWithAccountCode = AccountCodes.Where(x => x.ACC_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithAccountCode.Select(x => x.ACC_CODE.ToLower()).ToList();
                        var EndWithAccountCode = AccountCodes.Where(x => !startWithCodes.Contains(x.ACC_CODE.ToLower()) && x.ACC_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithAccountCode.Select(x => x.ACC_CODE.ToLower()).ToList();
                        var ContainsAccountCode = AccountCodes.Where(x => !startWithCodes.Contains(x.ACC_CODE.ToLower()) && !endWithCodes.Contains(x.ACC_CODE.ToLower()));
                        Filterdata.AddRange(StartWithAccountCode);
                        Filterdata.AddRange(ContainsAccountCode);
                        Filterdata.AddRange(EndWithAccountCode);
                        Filterdata.ForEach(s => s.Type = "AccountCode");
                        var Removedata = AllFilterAccount.RemoveAll(x => x.ACC_CODE.ToLower().Contains(filter));
                        var AccountNames = AllFilterAccount.Where(x => x.ACC_EDESC.ToLower().Contains(filter));
                        var StartWithAccountName = AccountNames.Where(x => x.ACC_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithAccountName.Select(x => x.ACC_EDESC.ToLower()).ToList();
                        var EndWithAccountName = AccountNames.Where(x => !startWithNames.Contains(x.ACC_EDESC.ToLower()) && x.ACC_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithAccountName.Select(x => x.ACC_EDESC.ToLower()).ToList();
                        var ContainsAccountName = AccountNames.Where(x => !startWithNames.Contains(x.ACC_EDESC.ToLower()) && !endWithNames.Contains(x.ACC_EDESC.ToLower()));
                        StartWithAccountName.ForEach(s => s.Type = "AccountName");
                        EndWithAccountName.ToList().ForEach(s => s.Type = "AccountName");
                        ContainsAccountName.ToList().ForEach(s => s.Type = "AccountName");
                        Filterdata.AddRange(StartWithAccountName);
                        Filterdata.AddRange(ContainsAccountName);
                        Filterdata.AddRange(EndWithAccountName);
                        this._cacheManager.Set($"AllFilterAccount_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                        return Filterdata;
                    }
                    return AllFilterAccount;
                }
            }
            return Filterdata;
        }
        [HttpGet]
        public List<AccountSetup> GetAllAccountSetupByFilterCharge(string filter)
        {
            //if (string.IsNullOrEmpty(filter))
            //    return new List<AccountSetup>();

            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var Filterdata = new List<AccountSetup>();
            var ShowAdvanceAutoComplete = false;
            //Constants.
            var setting = _settingService.LoadSetting<WebPrefrenceSetting>(Constants.WebPrefranceSetting);
            if (setting != null)
                bool.TryParse(setting.ShowAdvanceAutoComplete, out ShowAdvanceAutoComplete);
            if (ShowAdvanceAutoComplete == false)
            {

                if (this._cacheManager.IsSet($"{DT}_AllFilterAccountcharge_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}"))
                {
                    var data = _cacheManager.Get<List<AccountSetup>>($"{DT}_AllFilterAccountcharge_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}");
                    Filterdata = data;
                    return Filterdata;
                }
                else
                {
                    var AllFilterAccount = this._FormTemplateRepo.getALLAccountSetupByFlter(filter);
                    this._cacheManager.Set($"{DT}_AllFilterAccountcharge_{userid}_{company_code}_{branch_code}_{filter}_{ShowAdvanceAutoComplete}", AllFilterAccount, 20);
                    return AllFilterAccount;
                }

            }
            if (filter == "!@$")
            {
                return Filterdata;
            }
            if (filter == null)
                filter = "";
            if (filter.Contains("#"))
            {
                var colName = filter.Split('#')[0].ToString().ToLower();
                filter = filter.Split('#')[1].ToString().ToLower();
                if (colName == "code")
                {
                    if (this._cacheManager.IsSet($"AllAccountSetupByCodecharge_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<AccountSetup>>($"AllAccountSetupByCodecharge_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterAccount = this._FormTemplateRepo.getALLAccountSetupByFlter(filter);
                        var AccountCodes = AllFilterAccount.Where(x => x.ACC_CODE.ToLower().Contains(filter));
                        var StartWithAccountCode = AccountCodes.Where(x => x.ACC_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithAccountCode.Select(x => x.ACC_CODE.ToLower()).ToList();
                        var EndWithAccountCode = AccountCodes.Where(x => !startWithCodes.Contains(x.ACC_CODE.ToLower()) && x.ACC_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithAccountCode.Select(x => x.ACC_EDESC.ToLower()).ToList();
                        var ContainsAccountCode = AccountCodes.Where(x => !startWithCodes.Contains(x.ACC_CODE.ToLower()) && !endWithCodes.Contains(x.ACC_CODE.ToLower()));
                        Filterdata.AddRange(StartWithAccountCode);
                        Filterdata.AddRange(ContainsAccountCode);
                        Filterdata.AddRange(EndWithAccountCode);
                        Filterdata.ForEach(s => s.Type = "AccountCode");
                        this._cacheManager.Set($"AllAccountSetupByCodecharge_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }

                    return Filterdata;
                }
                if (colName == "name")
                {
                    if (this._cacheManager.IsSet($"{DT}_AllAccountSetupByNamecharge_{userid}_{company_code}_{branch_code}_{filter}"))
                    {
                        var data = _cacheManager.Get<List<AccountSetup>>($"AllAccountSetupByNamecharge_{userid}_{company_code}_{branch_code}_{filter}");
                        Filterdata = data;
                    }
                    else
                    {
                        var AllFilterAccount = this._FormTemplateRepo.getALLAccountSetupByFlter(filter);
                        var AccountNames = AllFilterAccount.Where(x => x.ACC_EDESC.ToLower().Contains(filter));
                        var StartWithAccountName = AccountNames.Where(x => x.ACC_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithAccountName.Select(x => x.ACC_EDESC.ToLower()).ToList();
                        var EndWithAccountName = AccountNames.Where(x => !startWithNames.Contains(x.ACC_EDESC.ToLower()) && x.ACC_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithAccountName.Select(x => x.ACC_EDESC.ToLower()).ToList();
                        var ContainsAccountName = AccountNames.Where(x => !startWithNames.Contains(x.ACC_EDESC.ToLower()) && !endWithNames.Contains(x.ACC_EDESC.ToLower()));
                        StartWithAccountName.ForEach(s => s.Type = "AccountName");
                        EndWithAccountName.ToList().ForEach(s => s.Type = "AccountName");
                        ContainsAccountName.ToList().ForEach(s => s.Type = "AccountName");
                        Filterdata.AddRange(StartWithAccountName);
                        Filterdata.AddRange(ContainsAccountName);
                        Filterdata.AddRange(EndWithAccountName);
                        this._cacheManager.Set($"AllAccountSetupByNamecharge_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                    }

                    return Filterdata;
                }
            }
            else
            {
                if (this._cacheManager.IsSet($"{DT}_AllFilterAccountcharge_{userid}_{company_code}_{branch_code}_{filter}"))
                {
                    var data = _cacheManager.Get<List<AccountSetup>>($"AllFilterAccountcharge_{userid}_{company_code}_{branch_code}_{filter}");
                    Filterdata = data;
                }
                else
                {
                    var AllFilterAccount = this._FormTemplateRepo.getALLAccountSetupByFlter(filter);
                    if (AllFilterAccount.Count >= 1)
                    {
                        var AccountCodes = AllFilterAccount.Where(x => x.ACC_CODE.ToLower().Contains(filter));
                        var StartWithAccountCode = AccountCodes.Where(x => x.ACC_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithAccountCode.Select(x => x.ACC_CODE.ToLower()).ToList();
                        var EndWithAccountCode = AccountCodes.Where(x => !startWithCodes.Contains(x.ACC_CODE.ToLower()) && x.ACC_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithAccountCode.Select(x => x.ACC_CODE.ToLower()).ToList();
                        var ContainsAccountCode = AccountCodes.Where(x => !startWithCodes.Contains(x.ACC_CODE.ToLower()) && !endWithCodes.Contains(x.ACC_CODE.ToLower()));
                        Filterdata.AddRange(StartWithAccountCode);
                        Filterdata.AddRange(ContainsAccountCode);
                        Filterdata.AddRange(EndWithAccountCode);
                        Filterdata.ForEach(s => s.Type = "AccountCode");
                        var Removedata = AllFilterAccount.RemoveAll(x => x.ACC_CODE.ToLower().Contains(filter));
                        var AccountNames = AllFilterAccount.Where(x => x.ACC_EDESC.ToLower().Contains(filter));
                        var StartWithAccountName = AccountNames.Where(x => x.ACC_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithAccountName.Select(x => x.ACC_EDESC.ToLower()).ToList();
                        var EndWithAccountName = AccountNames.Where(x => !startWithNames.Contains(x.ACC_EDESC.ToLower()) && x.ACC_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithAccountName.Select(x => x.ACC_EDESC.ToLower()).ToList();
                        var ContainsAccountName = AccountNames.Where(x => !startWithNames.Contains(x.ACC_EDESC.ToLower()) && !endWithNames.Contains(x.ACC_EDESC.ToLower()));
                        StartWithAccountName.ForEach(s => s.Type = "AccountName");
                        EndWithAccountName.ToList().ForEach(s => s.Type = "AccountName");
                        ContainsAccountName.ToList().ForEach(s => s.Type = "AccountName");
                        Filterdata.AddRange(StartWithAccountName);
                        Filterdata.AddRange(ContainsAccountName);
                        Filterdata.AddRange(EndWithAccountName);
                        this._cacheManager.Set($"AllFilterAccountcharge_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                        return Filterdata;
                    }
                    return AllFilterAccount;
                }
            }
            return Filterdata;

        }
        [HttpGet]
        public string getSubledgerCodeByAccCode(string accCode)
        {
            var result = this._FormTemplateRepo.getSubledgerCodeByAccCode(accCode);
            return result;
        }
        [HttpGet]
        public string IfIsTdsByAccCode(string accCode)
        {
            var result = this._FormTemplateRepo.CheckIsTDSByAccCode(accCode);
            return result;
        }
        [HttpGet]
        public string IfIsVATByAccCode(string accCode)
        {
            var result = this._FormTemplateRepo.CheckIsVATByAccCode(accCode);
            return result;
        }
        [HttpGet]
        public decimal GetStockQuantityOfItem(string itemcodecode, string voucherdate, string locationcode)
        {
            var result = this._FormTemplateRepo.GetStockQuantity(itemcodecode, voucherdate, locationcode);
            return result;
        }
        [HttpPost]
        public List<COMMON_COLUMN> GetVoucherDetailForReferenceEdit(VoucherRefrence model)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<COMMON_COLUMN>();
            var result = this._FormTemplateRepo.VoucherDetailByReferenceForTemplate(model);
            return result;
        }
        [HttpPost]
        public List<COMMON_COLUMN> GetVoucherDetailForPRoduction(ProductionRefrence model)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<COMMON_COLUMN>();
            decimal ProductQty = 0;
            decimal.TryParse(model.Production_Qty, out ProductQty);
            var result = this._FormTemplateRepo.GetProductionFormDetail(model.FormCode,model.TableName,model.RoutingName, ProductQty);
            return result;
        }
        [HttpPost]
        public List<COMMON_COLUMN> bindReferenceGrid(DOCUMENT_REFERENCE documentReference)
        {
            var fdate = "";
            var tdate = "";
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var doc = documentReference.referenceModel.DOCUMENT;
            var tem = documentReference.referenceModel.TEMPLATE;
            var row = documentReference.referenceModel.ROW;
            if (documentReference.referenceModel.FROM_DATE != null)
            { fdate = documentReference.referenceModel.FROM_DATE.ToString(); }
            if (documentReference.referenceModel.TO_DATE != null)
            { tdate = documentReference.referenceModel.TO_DATE.ToString(); }
            var cname = documentReference.referenceModel.NAME;
            var iname = documentReference.referenceModel.ITEM_DESC;
            var vno = documentReference.referenceModel.VOUCHER_NO;
            var response = new List<COMMON_COLUMN>();
             //if (this._cacheManager.IsSet($"bindReferenceGrid_{userid}_{company_code}_{branch_code}_{doc}_{tem}_{row}_{fdate}_{tdate}_{cname}_{iname}_{vno}"))
            //{
            //    var data = _cacheManager.Get<List<COMMON_COLUMN>>($"bindReferenceGrid_{userid}_{company_code}_{branch_code}_{doc}_{tem}_{row}_{fdate}_{tdate}_{cname}_{iname}_{vno}");
            //    response = data;
            //}
            //else
            //{
            //    var ReferenceGridList = this._FormTemplateRepo.getReferenceGridData(documentReference.referenceModel);
            //    this._cacheManager.Set($"bindReferenceGrid_{userid}_{company_code}_{branch_code}_{doc}_{tem}_{row}_{fdate}_{tdate}_{cname}_{iname}_{vno}", ReferenceGridList, 20);
            //    response = ReferenceGridList;
            //}
            var ReferenceGridList = this._FormTemplateRepo.getReferenceGridData(documentReference.referenceModel);
            response = ReferenceGridList;
            return response;
        }
        [HttpGet]
        public string GetNewOrderNo(string companycode, string formcode, string currentdate, string tablename, string isSequence = "false")
        {
            bool UseSequenceInTransaction = false;
            var setting = _settingService.LoadSetting<WebPrefrenceSetting>(Constants.WebPrefranceSetting);
            if (setting != null)
                bool.TryParse(setting.UseSequenceInTransaction, out UseSequenceInTransaction);
            if (UseSequenceInTransaction)
            {
                var result = this._FormTemplateRepo.GetNewSequence();
                return result;
            }
            else {
                var result = this._FormTemplateRepo.NewVoucherNo(companycode, formcode, currentdate, tablename);
                return result;
            }
          //  return "Error No";
           
        }
        [HttpGet]
        public IHttpActionResult GetAllMenuItems()
        {
            try
            {
                var result = _FormTemplateRepo.GetAllMenuItems();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }

        }
        [HttpGet]
        public IHttpActionResult GetAllInventoryMenuItems()
        {
            try
            {
                var result = _FormTemplateRepo.GetAllInventoryMenuItems();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }

        }
        [HttpGet]
        public IHttpActionResult GetAllSetupMenuItems()
        {
            try
            {
                var result = _FormTemplateRepo.GetAllSetupMenuItems();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }

        }
        [HttpGet]
        public IHttpActionResult GetAllSalesMenuItems()
        {
            try
            {
                var result = _FormTemplateRepo.GetAllSalesMenuItems();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }

        }
        [HttpGet]
        public List<CategoryModel> GetAllItemCategoryFilter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<CategoryModel>();
            if (this._cacheManager.IsSet($"GetAllItemCategoryFilter_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<CategoryModel>>($"GetAllItemCategoryFilter_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var AllItemCategoryFilterList = this._FormTemplateRepo.GetAllItemCategoryFilter(filter);
                this._cacheManager.Set($"GetAllItemCategoryFilter_{userid}_{company_code}_{branch_code}_{filter}", AllItemCategoryFilterList, 20);
                response = AllItemCategoryFilterList;
            }
            return response;
        }
        [HttpGet]
        public List<MuCodeModel> GetAllIndexMuFilter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<MuCodeModel>();
            if (this._cacheManager.IsSet($"GetAllIndexMuFilter_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<MuCodeModel>>($"GetAllIndexMuFilter_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var AllIndexMuFilterList = this._FormTemplateRepo.GetAllIndexMuFilter(filter);
                this._cacheManager.Set($"GetAllIndexMuFilter_{userid}_{company_code}_{branch_code}_{filter}", AllIndexMuFilterList, 20);
                response = AllIndexMuFilterList;
            }
            return response;
        }
        [HttpGet]
        public List<AccountCodeModels> GetAllAccountCode()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<AccountCodeModels>();
          
            var AllAccountCodeList = this._FormTemplateRepo.getAllAccounts();
            response = AllAccountCodeList;
            return response;
            
           
        }
        public List<AccountCodeModels> GetAllAccountCodesupp()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<AccountCodeModels>();

            var AllAccountCodeList = this._FormTemplateRepo.getAllAccountSupp();
            response = AllAccountCodeList;
            return response;


        }
        public List<AccountCodeModels> GetAccCodeByCode(string acccode)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<AccountCodeModels>();
            if (this._cacheManager.IsSet($"GetAllAccountCode_{userid}_{company_code}_{branch_code}_{acccode}"))
            {
                var data = _cacheManager.Get<List<AccountCodeModels>>($"GetAllAccountCode_{userid}_{company_code}_{branch_code}_{acccode}");
                response = data;
            }
            else
            {


                var AllAccountCodeList = this._FormTemplateRepo.getAccountCodeByCode(acccode);
                this._cacheManager.Set($"GetAllAccountCode_{userid}_{company_code}_{branch_code}_{acccode}", AllAccountCodeList, 20);
                response = AllAccountCodeList;
            }
            return response;
        }
        public List<AccountSetup> GetAllChargeAccountSetupByFilter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<AccountSetup>();
            if (this._cacheManager.IsSet($"GetAllChargeAccountSetupByFilter_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<AccountSetup>>($"GetAllChargeAccountSetupByFilter_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var AllChargeAccountSetupList = this._FormTemplateRepo.getALLAccountSetupByFlter(filter);
                this._cacheManager.Set($"GetAllChargeAccountSetupByFilter_{userid}_{company_code}_{branch_code}_{filter}", AllChargeAccountSetupList, 20);
                response = AllChargeAccountSetupList;
            }
            return response;
        }
        [HttpGet]
        public List<BudgetCenter> GetAllBudgetCenterByFilter(string filter, string accCode)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<BudgetCenter>();
            //if (this._cacheManager.IsSet($"GetAllBudgetCenterByFilter_{userid}_{company_code}_{branch_code}_{filter}_{accCode}"))
            //{
            //    var data = _cacheManager.Get<List<BudgetCenter>>($"GetAllBudgetCenterByFilter_{userid}_{company_code}_{branch_code}_{filter}_{accCode}");
            //    response = data;
            //}
            //else
            //{
            //    var AllBudgetCenterByFilterList = this._FormTemplateRepo.GetAllBudgetCenterByFilter(filter, accCode);
            //    this._cacheManager.Set($"GetAllBudgetCenterByFilter_{userid}_{company_code}_{branch_code}_{filter}_{accCode}", AllBudgetCenterByFilterList, 20);
            //    response = AllBudgetCenterByFilterList;
            //}
            var AllBudgetCenterByFilterList = this._FormTemplateRepo.GetAllBudgetCenterByFilter(filter, accCode);
            response = AllBudgetCenterByFilterList;
            return response;
        }
        [HttpGet]
        public List<BudgetCenter> GetAllBudgetCenterForLocationByFilter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<BudgetCenter>();
            if (this._cacheManager.IsSet($"GetAllBudgetCenterForLocationByFilter_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<BudgetCenter>>($"GetAllBudgetCenterForLocationByFilter_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var AllBudgetCenterForLocationByFilterList = this._FormTemplateRepo.GetAllBudgetCenterForLocationByFilter(filter);
                this._cacheManager.Set($"GetAllBudgetCenterForLocationByFilter_{userid}_{company_code}_{branch_code}_{filter}", AllBudgetCenterForLocationByFilterList, 20);
                response = AllBudgetCenterForLocationByFilterList;
            }
            return response;
        }

        [HttpGet]
        public List<SubLedger> GetAllSubLedgerCodeByFilter(string filter)
        {
            if (string.IsNullOrEmpty(filter))
                return new List<SubLedger>();

            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<SubLedger>();
            if (this._cacheManager.IsSet($"GetAllSubLedgerCodeByFilter_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<SubLedger>>($"GetAllSubLedgerCodeByFilter_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var AllSubledgerByFilterList = this._FormTemplateRepo.GetAllSubCodeByFilter(filter);
                this._cacheManager.Set($"GetAllSubLedgerCodeByFilter_{userid}_{company_code}_{branch_code}_{filter}", AllSubledgerByFilterList, 20);
                response = AllSubledgerByFilterList;
            }
            return response;
        }
        [HttpGet]
        public List<PartyType> GetAllPartyTypeByFilter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<PartyType>();
            if (this._cacheManager.IsSet($"GetAllPartyTypeByFilter_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<PartyType>>($"GetAllPartyTypeByFilter_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var AllPartyTypeByFilterList = this._FormTemplateRepo.GetAllPartyTypeByFilter(filter);
                this._cacheManager.Set($"GetAllPartyTypeByFilter_{userid}_{company_code}_{branch_code}_{filter}", AllPartyTypeByFilterList, 20);
                response = AllPartyTypeByFilterList;
            }
            return response;
        }
        [HttpGet]
        public List<PartyType> GetPartyTypeByFilterAndCustomerCode(string filter, string customercode)
        {
            var AllPartyTypeByFilterList = this._FormTemplateRepo.GetAllPartyTypeByFilterAndCustomerCode(filter, customercode);
            return AllPartyTypeByFilterList;
        }

        [HttpGet]
        public List<PartyType> GetPartyTypes()
        {
            var AllPartyTypesList = this._FormTemplateRepo.GetAllPartyTypes();
            return AllPartyTypesList;
        }
        [HttpGet]
        public List<AreaSetup> GetAllAreaSetupByFilter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<AreaSetup>();
            if (this._cacheManager.IsSet($"GetAllAreaSetupByFilter_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<AreaSetup>>($"GetAllAreaSetupByFilter_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var AllAreaSetupByFilterList = this._FormTemplateRepo.GetAllAreaSetupByFilter(filter);
                this._cacheManager.Set($"GetAllAreaSetupByFilter_{userid}_{company_code}_{branch_code}_{filter}", AllAreaSetupByFilterList, 20);
                response = AllAreaSetupByFilterList;
            }
            return response;
        }
        [HttpGet]
        public List<BudgetCenter> GetAllBudgetCenterChildByFilter(string filter, string accCode)
        {
            var a = new List<BudgetCenter>();
            return a;
        }
        public List<BudgetCenter> getBudgetCodeByAccCode(string accCode)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<BudgetCenter>();
            if (this._cacheManager.IsSet($"getBudgetCodeByAccCode_{userid}_{company_code}_{branch_code}_{accCode}"))
            {
                var data = _cacheManager.Get<List<BudgetCenter>>($"getBudgetCodeByAccCode_{userid}_{company_code}_{branch_code}_{accCode}");
                response = data;
            }
            else
            {
                var BudgetCodeByAccCodeList = this._FormTemplateRepo.getBudgetCodeByAccCode(accCode);
                this._cacheManager.Set($"getBudgetCodeByAccCode_{userid}_{company_code}_{branch_code}_{accCode}", BudgetCodeByAccCodeList, 20);
                response = BudgetCodeByAccCodeList;
            }
            return response;
        }
        public List<BudgetCenter> checkBudgetFlagByLocationCode(string locationCode)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<BudgetCenter>();
            if (this._cacheManager.IsSet($"checkBudgetFlagByLocationCode_{userid}_{company_code}_{branch_code}_{locationCode}"))
            {
                var data = _cacheManager.Get<List<BudgetCenter>>($"checkBudgetFlagByLocationCode_{userid}_{company_code}_{branch_code}_{locationCode}");
                response = data;
            }
            else
            {
                var BudgetFlagByLocationCode = this._FormTemplateRepo.checkBudgetFlagAccessByLocationCode(locationCode);
                this._cacheManager.Set($"checkBudgetFlagByLocationCode_{userid}_{company_code}_{branch_code}_{locationCode}", BudgetFlagByLocationCode, 20);
                response = BudgetFlagByLocationCode;
            }
            return response;
        }
        [HttpGet]
        public List<ChargeOnSales> GetChargeDataForEdit(string formCode, string voucherNo)
        {
            _logErp.InfoInFile("GetChargeDataForEdit based on formcode: " + formCode + " and voucher number : " + voucherNo + " started=====");
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<ChargeOnSales>();
            //if (this._cacheManager.IsSet($"GetChargeDataForEdit_{userid}_{company_code}_{branch_code}_{formCode}_{voucherNo}"))
            //{
            //    var data = _cacheManager.Get<List<ChargeOnSales>>($"GetChargeDataForEdit_{userid}_{company_code}_{branch_code}_{formCode}_{voucherNo}");
            //    _logErp.InfoInFile("GetChargeDataForEdit fetched successfully from cached!!!!");
            //    response = data;
            //}
            //else
            //{
            //    var ChargeDataForEdit = this._FormTemplateRepo.GetChargesData(formCode, voucherNo);
            //    _logErp.InfoInFile("charge Data for edit : " + ChargeDataForEdit);
            //    this._cacheManager.Set($"GetChargeDataForEdit_{userid}_{company_code}_{branch_code}_{formCode}_{voucherNo}", ChargeDataForEdit, 20);
            //    response = ChargeDataForEdit;
            //}
            var ChargeDataForEdit = this._FormTemplateRepo.GetChargesData(formCode, voucherNo);
            response = ChargeDataForEdit;
            return response;
        }
        [HttpGet]
        public List<ChargeOnSales> GetItemChargeDataSavedValueWise(string voucherNo, string itemcode)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<ChargeOnSales>();
            if (this._cacheManager.IsSet($"GetItemChargeDataSavedValueWise_{userid}_{company_code}_{branch_code}_{itemcode}_{voucherNo}"))
            {
                var data = _cacheManager.Get<List<ChargeOnSales>>($"GetItemChargeDataSavedValueWise_{userid}_{company_code}_{branch_code}_{itemcode}_{voucherNo}");
                response = data;
            }
            else
            {
                var ChargeDataSavedValueWise = this._FormTemplateRepo.GetInvChargesDataSavedvaluewise(voucherNo, itemcode);
                this._cacheManager.Set($"GetItemChargeDataSavedValueWise_{userid}_{company_code}_{branch_code}_{itemcode}_{voucherNo}", ChargeDataSavedValueWise, 20);
                response = ChargeDataSavedValueWise;
            }
            return response;
        }
        [HttpGet]
        public List<ChargeOnSales> GetItemChargeDataSavedQuantityWise(string voucherNo, string itemcode)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<ChargeOnSales>();
            if (this._cacheManager.IsSet($"GetItemChargeDataSavedQuantityWise_{userid}_{company_code}_{branch_code}_{itemcode}_{voucherNo}"))
            {
                var data = _cacheManager.Get<List<ChargeOnSales>>($"GetItemChargeDataSavedQuantityWise_{userid}_{company_code}_{branch_code}_{itemcode}_{voucherNo}");
                response = data;
            }
            else
            {
                var ChargeDataSavedQuantityWise = this._FormTemplateRepo.GetInvChargesDataSavedvaluewise(voucherNo, itemcode);
                this._cacheManager.Set($"GetItemChargeDataSavedQuantityWise_{userid}_{company_code}_{branch_code}_{itemcode}_{voucherNo}", ChargeDataSavedQuantityWise, 20);
                response = ChargeDataSavedQuantityWise;
            }
            return response;
        }
        [HttpGet]
        public List<ChargeOnSales> GetChargeData(string formCode)
        {
            _logErp.InfoInFile("GetCharge Data for " + formCode + " form code started==========");
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<ChargeOnSales>();
            //if (this._cacheManager.IsSet($"GetChargeData_{userid}_{company_code}_{branch_code}_{formCode}"))
            //{
            //    var data = _cacheManager.Get<List<ChargeOnSales>>($"GetChargeData_{userid}_{company_code}_{branch_code}_{formCode}");
            //    _logErp.InfoInFile(data.Count() + " records of charge data has been fetched from cached");
            //    response = data;
            //}
            //else
            //{
            //    var ChargeData = this._FormTemplateRepo.GetChargesData(formCode);
            //    _logErp.InfoInFile("Charge data for form code contains : " + ChargeData);
            //    this._cacheManager.Set($"GetChargeData_{userid}_{company_code}_{branch_code}_{formCode}", ChargeData, 20);
            //    response = ChargeData;
            //}
            var ChargeData = this._FormTemplateRepo.GetChargesData(formCode);
            response = ChargeData;
            return response;
        }
        [HttpGet]
        public ChargeOnItem GetItemChargeData(string formCode, string itemcode)
        {
            return this._FormTemplateRepo.GetInvItemChargesData(formCode, itemcode);
        }

        [HttpPost]
        public HttpResponseMessage SaveAsDraftFormDataOld(FormDetails model)
        {
            using (var trans = _objectEntity.Database.BeginTransaction())
            {
                try
                {
                    Newtonsoft.Json.Linq.JObject mastercolumn = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(model.Master_COLUMN_VALUE);
                    dynamic childcolumnvalues = JsonConvert.DeserializeObject(model.Child_COLUMN_VALUE);
                    var templateNo = string.Empty;
                    var maxTemplateQry = $@"SELECT MAX(TO_NUMBER(TEMPLATE_NO)+1) MAX_NO FROM FORM_TEMPLATE_SETUP";
                    templateNo = this._dbContext.SqlQuery<int>(maxTemplateQry).FirstOrDefault().ToString();
                    var defaultCol = "CREATED_BY, CREATED_DATE";
                    var defaultVal = $@"'{this._workContext.CurrentUserinformation.login_code}',SYSDATE";
                    if (model.Save_Flag == "0")
                    {
                        var templateInsertQry = $@"INSERT INTO FORM_TEMPLATE_SETUP(TEMPLATE_NO,TEMPLATE_EDESC,TEMPLATE_NDESC,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,ASSIGNEE,ASSIGNED_DATE)
                                        VALUES('{templateNo}','{model.FORM_TEMPLATE.TEMPLATE_EDESC}','{model.FORM_TEMPLATE.TEMPLATE_NDESC}','{model.Form_Code}','{this._workContext.CurrentUserinformation.company_code}','{this._workContext.CurrentUserinformation.branch_code}','{_workContext.CurrentUserinformation.User_id}',SYSDATE,'N','{model.FORM_TEMPLATE.TEMPLATE_ASSIGNEE}',SYSDATE)";
                        var result = this._objectEntity.ExecuteSqlCommand(templateInsertQry);
                    }
                    else
                    {
                        var templateInsertQry = $@"UPDATE FORM_TEMPLATE_SETUP SET TEMPLATE_EDESC='{model.FORM_TEMPLATE.TEMPLATE_EDESC}',TEMPLATE_NDESC='{model.FORM_TEMPLATE.TEMPLATE_NDESC}',MODIFY_BY='{_workContext.CurrentUserinformation.User_id}',MODIFY_DATE=SYSDATE,ASSIGNEE='{model.FORM_TEMPLATE.TEMPLATE_ASSIGNEE}',ASSIGNED_DATE= SYSDATE WHERE TEMPLATE_NO='{model.FORM_TEMPLATE.TEMPLATE_NO}'";
                        var result = this._objectEntity.ExecuteSqlCommand(templateInsertQry);
                        var Qry = $@"SELECT * FROM FORM_TEMPLATE_DETAIL_SETUP WHERE TEMPLATE_NO='{model.FORM_TEMPLATE.TEMPLATE_NO}'";
                        var cResult = this._objectEntity.SqlQuery<DraftFormModel>(Qry).FirstOrDefault();
                        if (cResult != null)
                        {
                            defaultVal = $@"'{cResult.CREATED_BY}',TO_DATE('{cResult.CREATED_DATE}','MM-DD-YYYY HH:MI:SS AM'),'{this._workContext.CurrentUserinformation.login_code}',SYSDATE";
                            defaultCol = "CREATED_BY,CREATED_DATE,MODIFY_BY, MODIFY_DATE";
                        }
                        var deleteQry = $@"DELETE FORM_TEMPLATE_DETAIL_SETUP WHERE TEMPLATE_NO='{model.FORM_TEMPLATE.TEMPLATE_NO}'";
                        this._objectEntity.ExecuteSqlCommand(deleteQry);
                        templateNo = model.FORM_TEMPLATE.TEMPLATE_NO;
                    }
                    foreach (var v in mastercolumn)
                    {
                        string lastName = v.Key.Split('_').Last();
                        if (v.Value.ToString() != "")
                        {
                            var insertDraftQry = $@"INSERT INTO FORM_TEMPLATE_DETAIL_SETUP (TEMPLATE_NO,FORM_CODE,COMPANY_CODE,BRANCH_CODE,SERIAL_NO,COLUMN_NAME,COLUMN_VALUE,TABLE_NAME,DELETED_FLAG,SYN_ROWID ,{defaultCol})
                                                     VALUES('{templateNo}','{model.Form_Code}','{this._workContext.CurrentUserinformation.company_code}','{this._workContext.CurrentUserinformation.branch_code}','0','{v.Key}','{v.Value}','{model.Table_Name}','N','',{defaultVal})";
                            this._objectEntity.ExecuteSqlCommand(insertDraftQry);
                        }
                    }
                    int serialno = 1;
                    foreach (var item in childcolumnvalues)
                    {
                        var itemarray = JsonConvert.DeserializeObject(item.ToString());
                        foreach (var data in itemarray)
                        {
                            var dataname = data.Name.ToString();
                            string[] datanamesplit = dataname.Split('_');
                            string datalastName = datanamesplit.Last();
                            var datavalue = data.Value;
                            datavalue = datavalue.ToString();
                            if (datavalue != "")
                            {
                                var insertDraftQry = $@"INSERT INTO FORM_TEMPLATE_DETAIL_SETUP (TEMPLATE_NO,FORM_CODE,COMPANY_CODE,BRANCH_CODE,SERIAL_NO,COLUMN_NAME,COLUMN_VALUE,TABLE_NAME,DELETED_FLAG,SYN_ROWID,{defaultCol})
                                                     VALUES('{templateNo}','{model.Form_Code}','{this._workContext.CurrentUserinformation.company_code}','{this._workContext.CurrentUserinformation.branch_code}','{serialno}','{dataname}','{datavalue}','{model.Table_Name}','N','',{defaultVal})";
                                this._objectEntity.ExecuteSqlCommand(insertDraftQry);
                            }
                        }
                        serialno++;
                    }

                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("draftformdetail");
                    keystart.Add("DraftTemplateDatabyTempCode");
                    keystart.Add("GetAllMenuInventoryAssigneeSavedDraftTemplateList");
                    keystart.Add("getDraftDataByFormCodeAndTempCode");
                    keystart.Add("GetDraftList");
                    keystart.Add("GetDraftDetailList");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion

                    trans.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
        }


        [HttpPost]
        public HttpResponseMessage SaveAsDraftFormData(FormDetails model)
        {
            using (var trans = _objectEntity.Database.BeginTransaction())
            {
                try
                {

                    DraftFormModel saveFieldForDraft = new DraftFormModel();
                    // var masterColumn = _saveDocTemplate.MapSalesOrderMasterColumnWithValue(model.Master_COLUMN_VALUE, "", "");
                    // var childColumnValues = _saveDocTemplate.MapOrderChildColumnWithValue(model.Child_COLUMN_VALUE);
                    var masterColumn = _saveDocTemplate.MapMasterColumnToDic(model.Master_COLUMN_VALUE);
                    var childColumnValues = _saveDocTemplate.MapChildColumnToDict(model.Child_COLUMN_VALUE);
                    var templateNo = _saveDocTemplate.GetTemplateNo();
                    var defaultCol = "CREATED_BY, CREATED_DATE";
                    var defaultVal = $@"'{this._workContext.CurrentUserinformation.login_code}',SYSDATE";
                    if (model.Save_Flag == "0")
                    {
                        var result = _saveDocTemplate.AddToFormTemplateSetup(model,templateNo);
                       
                    }
                    else
                    {
                        _saveDocTemplate.UpdateFormTemlateSetup(model);
                        var cResult = _saveDocTemplate.GetFormTemplateDetailsByTemplateNo(model.FORM_TEMPLATE.TEMPLATE_NO);
                        if (cResult != null)
                        {
                            saveFieldForDraft.CREATED_BY = cResult.CREATED_BY;
                            saveFieldForDraft.CREATED_DATE = cResult.CREATED_DATE;
                            saveFieldForDraft.MODIFY_BY = _workContext.CurrentUserinformation.login_code;
                            saveFieldForDraft.MODIFY_DATE = DateTime.Now;
                           // defaultVal = $@"'{cResult.CREATED_BY}',TO_DATE('{cResult.CREATED_DATE}','MM-DD-YYYY HH:MI:SS AM'),'{this._workContext.CurrentUserinformation.login_code}',SYSDATE";
                           // defaultCol = "CREATED_BY,CREATED_DATE,MODIFY_BY, MODIFY_DATE";
                        }
                        _saveDocTemplate.DeleteFromFormTemplateSetupByTemplateNo(model.FORM_TEMPLATE.TEMPLATE_NO);
                        templateNo = model.FORM_TEMPLATE.TEMPLATE_NO;
                    }


                    saveFieldForDraft.TEMPLATE_NO = templateNo;
                    saveFieldForDraft.FORM_CODE = model.Form_Code;
                    saveFieldForDraft.COMPANY_CODE = _workContext.CurrentUserinformation.company_code;
                    saveFieldForDraft.SERIAL_NO = 0;
                    saveFieldForDraft.COLUMN_NAME = model.Master_COLUMN_VALUE;
                    saveFieldForDraft.COLUMN_VALUE = "";
                    saveFieldForDraft.TABLE_NAME = model.Table_Name;
                    saveFieldForDraft.DELETED_FLAG = "N";
                    saveFieldForDraft.CREATED_BY = _workContext.CurrentUserinformation.login_code;
                    saveFieldForDraft.CREATED_DATE = DateTime.Now;

                    if (masterColumn != null)
                    {  
                        _saveDocTemplate.AddMasterColumnFormSetup(masterColumn,saveFieldForDraft);
                    }

                    if (childColumnValues != null)
                    {
                        _saveDocTemplate.AddChildColumnFormSetup(childColumnValues,saveFieldForDraft);
                    }
                   

                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("draftformdetail");
                    keystart.Add("DraftTemplateDatabyTempCode");
                    keystart.Add("GetAllMenuInventoryAssigneeSavedDraftTemplateList");
                    keystart.Add("getDraftDataByFormCodeAndTempCode");
                    keystart.Add("GetDraftList");
                    keystart.Add("GetDraftDetailList");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion

                    trans.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
        }


        #region SAVE DOCUMENT TEMPLATE  

        //[HttpPost]
        public HttpResponseMessage SaveFormDataOldMethod(FormDetails model)
        {
            //using (var trans = _objectEntity.Database.BeginTransaction())
        //{
            try
            {
                var orderno = model.Order_No;
                var primarydatecolumn = _FormTemplateRepo.GetPrimaryDateByTableName(model.Table_Name);
                var primarycolumnname = _FormTemplateRepo.GetPrimaryColumnByTableName(model.Table_Name);
                string primarydate = string.Empty, primarycolumn = string.Empty;
                string createddatestring = "TO_DATE('" + DateTime.Now.ToString("dd-MMM-yyyy") + "'" + ",'DD-MON-YYYY hh24:mi:ss')", newOrderNo = _FormTemplateRepo.NewVoucherNo(this._workContext.CurrentUserinformation.company_code, model.Form_Code, DateTime.Now.ToString("dd-MMM-yyyy"), model.Table_Name), manualno = string.Empty, VoucherDate = createddatestring, validation_voucher_date = createddatestring, currencyformat = "NRS", newvoucherNo = string.Empty;
                var from_ref = model.FROM_REF;
                decimal exchangrate = 1;

                Newtonsoft.Json.Linq.JObject mastercolumn = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(model.Master_COLUMN_VALUE);
                Newtonsoft.Json.Linq.JObject customcolumn = new Newtonsoft.Json.Linq.JObject();
                if (model.Custom_COLUMN_VALUE != null)
                {
                    customcolumn = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(model.Custom_COLUMN_VALUE);
                }
                dynamic childcolumnvalues = JsonConvert.DeserializeObject(model.Child_COLUMN_VALUE);
                dynamic charges = JsonConvert.DeserializeObject(model.CHARGES);
                dynamic shippingdetailsvalues = JsonConvert.DeserializeObject(model.SHIPPING_DETAILS_VALUE);
                //dynamic invItemChargeValue = JsonConvert.DeserializeObject(model.INV_ITEM_CHARGE_VALUE);
                SalesOrderMasterModel master = new SalesOrderMasterModel();
                StringBuilder Columnbuilder = new StringBuilder();
                StringBuilder valuesbuilder = new StringBuilder();
                bool insertmaintable = false, updatemaintable = false, insertmastertable = false, updatemastertable = false, insertcustomtable = false, updatecustomtable = false, deletecustomtable = false, deletechargequery = false, deletesdquery = false;
                foreach (var m in mastercolumn)
                {
                    if (m.Key.ToString() == "CURRENCY_CODE")
                    {
                        Columnbuilder.Append(m.Key.ToString()).Append(",");
                    }
                    else if (m.Key.ToString() == "EXCHANGE_RATE")
                    {
                        Columnbuilder.Append(m.Key.ToString()).Append(",");
                    }
                    else Columnbuilder.Append(m.Key.ToString()).Append(",");
                }
                var getPrevDataQuery = $@"SELECT VOUCHER_NO,SESSION_ROWID, CREATED_BY, CREATED_DATE,PRINT_COUNT FROM MASTER_TRANSACTION WHERE VOUCHER_NO= '{orderno}'";
                var defaultData = this._objectEntity.SqlQuery<SalesOrderDetail>(getPrevDataQuery).ToList();
                string defaultCol = "MODIFY_BY,MODIFY_DATE", createdDateForEdit = "", createdByForEdit = "", voucherNoForEdit = "";
                var sessionRowIDForedit = 0;
                int? printcountedit = 1;
                foreach (var def in defaultData)
                {
                    voucherNoForEdit = def.VOUCHER_NO.ToString();
                    createdDateForEdit = "TO_DATE('" + def.CREATED_DATE.ToString() + "', 'MM-DD-YYYY hh12:mi:ss pm')";
                    createdByForEdit = def.CREATED_BY.ToString().ToUpper();
                    sessionRowIDForedit = Convert.ToInt32(def.SESSION_ROWID);
                    printcountedit = printcountedit = def.PRINT_COUNT.HasValue ? def.PRINT_COUNT.Value + 1 : 0;
                }
                Columnbuilder.Append(model.Child_COLUMNS);
                var staticsalesordercolumns = "SERIAL_NO, FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SESSION_ROWID";
                Columnbuilder.Append(staticsalesordercolumns);
                foreach (var v in mastercolumn)
                {
                    if (v.Key == primarydatecolumn)
                    {
                        primarydate = v.Value.ToString();
                    }
                    if (v.Key == primarycolumnname)
                    {
                        primarycolumn = v.Value.ToString();
                    }
                    string lastName = v.Key.Split('_').Last();
                    if (lastName == "DATE")
                    {
                        if (v.Value.ToString() == "")
                        {
                            valuesbuilder.Append("''").Append(",");
                        }
                        else
                        {
                            if (v.Key == primarydatecolumn)
                            {
                                VoucherDate = "trunc(TO_DATE(" + "'" + v.Value + "'" + ",'DD-MON-YYYY hh24:mi:ss'))";
                                validation_voucher_date = v.Value.ToString();
                            }
                            valuesbuilder.Append("TO_DATE(" + "'" + v.Value + "'" + ",'DD-MON-YYYY hh24:mi:ss')").Append(",");
                        }
                    }
                    else if (v.Key.ToString() == "ORDER_NO" || v.Key.ToString() == "SALES_NO" || v.Key.ToString() == "CHALAN_NO" || v.Key.ToString() == "RETURN_NO")
                    {
                        if (orderno == "undefined")
                        {
                            valuesbuilder.Append("'" + newOrderNo + "'").Append(",");
                            //if (voucherNoForEdit == "")
                            //{
                            //                    valuesbuilder.Append("'" + v.Value + "'").Append(",");
                            //    newvoucherNo = v.Value.ToString();
                            //}
                            //else
                            //{
                            //    valuesbuilder.Append("'" + voucherNoForEdit + "'").Append(",");
                            //    newvoucherNo = voucherNoForEdit;
                            //}
                        }
                        else
                        {
                            valuesbuilder.Append("'" + v.Value + "'").Append(",");
                        }
                    }
                    else if (v.Key.ToString() == "MANUAL_NO")
                    {
                        valuesbuilder.Append("'" + v.Value + "'").Append(",");
                        manualno = v.Value.ToString();
                    }
                    else if (v.Key.ToString() == "CURRENCY_CODE")
                    {

                        valuesbuilder.Append("'" + v.Value + "'").Append(",");
                        currencyformat = v.Value.ToString();
                    }
                    else if (v.Key.ToString() == "EXCHANGE_RATE")
                    {
                        valuesbuilder.Append("'" + v.Value + "'").Append(",");
                        exchangrate = Convert.ToDecimal(v.Value.ToString());
                    }
                    //else if (v.Key.ToString() == primarycolumnname)
                    //{

                    //    if (voucherNoForEdit == "")
                    //            {
                    //                valuesbuilder.Append("'" + v.Value + "'").Append(",");
                    //        newvoucherNo = v.Value.ToString();
                    //            }
                    //                        else
                    //                        {
                    //        valuesbuilder.Append("'" + voucherNoForEdit + "'").Append(",");
                    //        newvoucherNo = voucherNoForEdit;
                    //                        }
                    //}
                    else { valuesbuilder.Append("'" + v.Value + "'").Append(","); }
                }
                var validation = checkValidation(model, validation_voucher_date);
                if (validation != "Valid")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = validation, STATUS_CODE = (int)HttpStatusCode.OK });
                }
                if (orderno == "undefined")
                {
                    int serialno = 1;
                    foreach (var item in childcolumnvalues)
                    {
                        StringBuilder childvaluesbuilder = new StringBuilder();
                        StringBuilder masterchildvaluesbuilder = new StringBuilder();
                        var itemarray = JsonConvert.DeserializeObject(item.ToString());
                        foreach (var data in itemarray)
                        {
                            var dataname = data.Name.ToString();
                            string[] datanamesplit = dataname.Split('_');
                            string datalastName = datanamesplit.Last();
                            var datavalue = data.Value;
                            if (datalastName == "DATE")
                            {
                                if (datavalue.Value.ToString() == "")
                                {
                                    childvaluesbuilder.Append("''").Append(",");
                                }
                                else
                                {
                                    childvaluesbuilder.Append("TO_DATE(" + "'" + datavalue.Value + "'" + ",'MM-DD-YYYY')").Append(",");
                                }
                            }
                            else if (datalastName == "PRICE")
                            {
                                if (datavalue.Value == null)
                                {
                                    childvaluesbuilder.Append("''").Append(",");
                                }
                                else
                                {
                                    childvaluesbuilder.Append(datavalue.Value).Append(",");
                                }
                            }
                            else if (datalastName == "QUANTITY")
                            {
                                try
                                {
                                    datavalue.Value = Convert.ToDecimal(datavalue.Value);
                                }
                                catch (Exception ex)
                                {
                                    datavalue.Value = null;
                                }
                                if (datavalue.Value == null)
                                {
                                    childvaluesbuilder.Append("''").Append(",");
                                }
                                else
                                {
                                    childvaluesbuilder.Append(datavalue.Value).Append(",");
                                }
                            }
                            else if (dataname == "MANUAL_NO")
                            {
                                if (datavalue.Value.ToString() == "")
                                {
                                    childvaluesbuilder.Append("' '").Append(",");
                                }
                                else
                                {
                                    childvaluesbuilder.Append("'" + datavalue.Value + "'").Append(",");
                                }
                            }
                            else if (dataname == "STOCK_BLOCK_FLAG")
                            {
                                if (datavalue.Value.ToString() == "")
                                {
                                    childvaluesbuilder.Append("'N'").Append(",");
                                }
                                else
                                {
                                    childvaluesbuilder.Append("'" + datavalue.Value + "'").Append(",");
                                }
                            }
                            else
                            {
                                if (datavalue.Value.ToString() == "")
                                {
                                    childvaluesbuilder.Append("' '").Append(",");
                                }
                                else
                                {
                                    childvaluesbuilder.Append("'" + datavalue.Value + "'").Append(",");
                                }
                            }
                        }
                        masterchildvaluesbuilder.Append(valuesbuilder);
                        masterchildvaluesbuilder.Append(childvaluesbuilder);
                        var values = masterchildvaluesbuilder.ToString().TrimEnd(',');
                        var insertQuery = string.Format(@"insert into " + model.Table_Name + "({0}) values({1},{2},{3},{4},{5},{6},{7},'{8}',{9})", Columnbuilder, values, serialno, "'" + model.Form_Code + "'", "'" + this._workContext.CurrentUserinformation.company_code + "'", "'" + this._workContext.CurrentUserinformation.branch_code + "'", "'" + this._workContext.CurrentUserinformation.login_code + "'", createddatestring, 'N', "'" + newvoucherNo + "'");
                        this._objectEntity.ExecuteSqlCommand(insertQuery);
                        insertmaintable = true;
                        masterchildvaluesbuilder.Length = 0;
                        masterchildvaluesbuilder.Capacity = 0;
                        childvaluesbuilder.Length = 0;
                        childvaluesbuilder.Capacity = 0;
                        serialno++;
                    }
                    //var VoucherNumberGeneratedNo = string.Empty;
                    //if (insertmaintable == true)
                    //{
                    //    string updateTransactionNo = $"select FN_GET_VOUCHER_NO('{this._workContext.CurrentUserinformation.company_code}','{model.Form_Code}',{VoucherDate},'{newvoucherNo}') from dual ";
                    //    VoucherNumberGeneratedNo = _objectEntity.SqlQuery<string>(updateTransactionNo).FirstOrDefault();
                    //}
                    if (model.FROM_REF)
                    {
                        var refInsQry = $@"SELECT * FROM {model.Table_Name} WHERE {primarycolumnname}='{newOrderNo}'";
                        var refResult = this._objectEntity.SqlQuery<COMMON_COLUMN>(refInsQry).ToList();
                        if (refResult.Count > 0)
                        {
                            var srNo = 1;
                            foreach (var it in refResult)
                            {
                                foreach (var item in model.REF_MODEL)
                                {
                                    if (it.ITEM_CODE == item.ITEM_CODE)
                                    {
                                        var maxRefNoQry = $@"SELECT TO_CHAR(SYSDATE,'RRRRMMDD')||'.'||LPAD(MAX(REGEXP_SUBSTR(TRANSACTION_NO,'[^.]+', 1, 2))+1,11,'0')TRANSACTION_NO FROM REFERENCE_DETAIL";
                                        var maxRefNo = this._objectEntity.SqlQuery<string>(maxRefNoQry).FirstOrDefault();
                                        var getRefFormCodeQry = $@"SELECT REF_FORM_CODE FROM FORM_SETUP WHERE FORM_CODE ='{model.Form_Code}' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'";
                                        var REF_FORM_CODE = this._objectEntity.SqlQuery<string>(getRefFormCodeQry).FirstOrDefault();
                                        var refInsQuery = $@"INSERT INTO REFERENCE_DETAIL (TRANSACTION_NO,VOUCHER_NO,FORM_CODE,COMPANY_CODE,SERIAL_NO,REFERENCE_NO,REFERENCE_FORM_CODE,REFERENCE_ITEM_CODE,
                                                      REFERENCE_QUANTITY,REFERENCE_MU_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,REFERENCE_UNIT_PRICE,REFERENCE_TOTAL_PRICE,REFERENCE_CALC_UNIT_PRICE,REFERENCE_CALC_TOTAL_PRICE,                                                        REFERENCE_REMARKS,REFERENCE_DATE,BRANCH_CODE,REFERENCE_BRANCH_CODE,REFERENCE_SERIAL_NO,SYN_ROWID,BATCH_NO,REFERENCE_BATCH_NO,VOUCHER_DATE) 
                                                    VALUES('{maxRefNo}','{newOrderNo}','{model.Form_Code}','{_workContext.CurrentUserinformation.company_code}','{it.SERIAL_NO}','{item.VOUCHER_NO}','{REF_FORM_CODE}','{it.ITEM_CODE}','{it.QUANTITY}','{it.MU_CODE}','{_workContext.CurrentUserinformation.login_code}',SYSDATE,
                                                           'N','{it.UNIT_PRICE}','{it.TOTAL_PRICE}','{it.CALC_UNIT_PRICE}','{it.CALC_TOTAL_PRICE}','{it.REMARKS}',SYSDATE,'{_workContext.CurrentUserinformation.branch_code}','{_workContext.CurrentUserinformation.branch_code}','{srNo}','','','',SYSDATE)";
                                        this._objectEntity.ExecuteSqlCommand(refInsQuery);
                                    }
                                }
                                srNo++;
                            }
                        }
                    }
                    //var VoucherNumberGeneratedNo = string.Empty;
                    if (insertmaintable == true)
                    {
                        //string updateTransactionNo = $"select FN_GET_VOUCHER_NO('{this._workContext.CurrentUserinformation.company_code}','{model.Form_Code}',{VoucherDate},'{newvoucherNo}') from dual ";
                        //VoucherNumberGeneratedNo = _objectEntity.SqlQuery<string>(updateTransactionNo).FirstOrDefault();
                        if (model.Save_Flag == "3")
                        {
                            string insertmasterQuery = string.Format(@"INSERT INTO MASTER_TRANSACTION(VOUCHER_NO,VOUCHER_AMOUNT,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,DELETED_FLAG,CURRENCY_CODE,CREATED_DATE,VOUCHER_DATE,SYN_ROWID,EXCHANGE_RATE,SESSION_ROWID,PRINT_COUNT) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}',TO_DATE('{8}','DD-MON-YYYY hh24:mi:ss'),{9},'{10}',{11},'{12}',{13})",
                          newOrderNo, model.Grand_Total, model.Form_Code, this._workContext.CurrentUserinformation.company_code, this._workContext.CurrentUserinformation.branch_code, this._workContext.CurrentUserinformation.login_code, 'N', currencyformat, DateTime.Now.ToString("dd-MMM-yyyy"), VoucherDate, manualno, exchangrate, newvoucherNo, 1);
                            this._objectEntity.ExecuteSqlCommand(insertmasterQuery);
                        }
                        else
                        {

                            string insertmasterQuery = string.Format(@"INSERT INTO MASTER_TRANSACTION(VOUCHER_NO,VOUCHER_AMOUNT,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,DELETED_FLAG,CURRENCY_CODE,CREATED_DATE,VOUCHER_DATE,SYN_ROWID,EXCHANGE_RATE,SESSION_ROWID) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}',TO_DATE('{8}','DD-MON-YYYY hh24:mi:ss'),{9},'{10}',{11},'{12}')",
                            newOrderNo, model.Grand_Total, model.Form_Code, this._workContext.CurrentUserinformation.company_code, this._workContext.CurrentUserinformation.branch_code, this._workContext.CurrentUserinformation.login_code, 'N', currencyformat, DateTime.Now.ToString("dd-MMM-yyyy"), VoucherDate, manualno, exchangrate, newvoucherNo);
                            this._objectEntity.ExecuteSqlCommand(insertmasterQuery);
                        }

                        insertmastertable = true;
                    }
                    if (customcolumn.Count > 0 && insertmaintable == true && insertmastertable == true)
                    {
                        foreach (var r in customcolumn)
                        {
                            string insertQuery = string.Format(@"INSERT INTO CUSTOM_TRANSACTION(VOUCHER_NO,FIELD_NAME,FIELD_VALUE,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SESSION_ROWID) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}',TO_DATE('{7}','DD-MON-YYYY hh24:mi:ss'),'{8}','{9}')",
                                newOrderNo, r.Key.ToString(), r.Value.ToString(), model.Form_Code, this._workContext.CurrentUserinformation.company_code, this._workContext.CurrentUserinformation.branch_code, this._workContext.CurrentUserinformation.login_code, DateTime.Now.ToString("dd-MMM-yyyy"), 'N', newvoucherNo);
                            this._objectEntity.ExecuteSqlCommand(insertQuery);
                            insertcustomtable = true;
                        }
                    }
                    if (model.CHARGES != "[]")
                    {
                        string applyon = "", chargeapplyon = "", chargecode = "", chargetypeflag = "", amtPercent = "", valuepercentflag = "", accountCode = "";
                        var amount = 0.00;
                        var chargeserialno = 1;
                        foreach (var citem in charges)

                        {
                            var citemrow = JsonConvert.DeserializeObject(citem.ToString());
                            foreach (var data in citemrow)
                            {
                                if (data.Name == "APPLY_ON")
                                {
                                    applyon = "D";
                                }
                                else if (data.Name == "CHARGE_APPLY_ON")
                                {
                                    chargeapplyon = data.Value;
                                }
                                else if (data.Name == "CHARGE_CODE")
                                {
                                    chargecode = data.Value;
                                }
                                else if (data.Name == "CHARGE_TYPE_FLAG")
                                {
                                    chargetypeflag = data.Value;
                                }
                                else if (data.Name == "VALUE_PERCENT_AMOUNT")
                                {
                                    amtPercent = data.Value;
                                }
                                else if (data.Name == "CHARGE_AMOUNT")
                                {
                                    amount = data.Value;
                                }
                                else if (data.Name == "VALUE_PERCENT_FLAG")
                                {
                                    valuepercentflag = data.Value;
                                }
                                else if (data.Name == "ACC_CODE")
                                {
                                    accountCode = data.Value;
                                }
                            }
                            string transquery = string.Format(@"select to_number((max(to_number(TRANSACTION_NO)) + 1)) ORDER_NO from CHARGE_TRANSACTION");
                            int newtransno = this._objectEntity.SqlQuery<int>(transquery).FirstOrDefault();
                            string insertChargeQuery = $@"INSERT INTO CHARGE_TRANSACTION(TRANSACTION_NO,TABLE_NAME,REFERENCE_NO,APPLY_ON,ACC_CODE,CHARGE_CODE,CHARGE_TYPE_FLAG,CHARGE_AMOUNT,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,CURRENCY_CODE,EXCHANGE_RATE,VOUCHER_NO,CALCULATE_BY,SERIAL_NO,SESSION_ROWID) VALUES('{newtransno}','{model.Table_Name}','{newOrderNo}','{applyon}','{accountCode}','{chargecode}','{chargetypeflag}', {amount},'{model.Form_Code}','{this._workContext.CurrentUserinformation.company_code}','{this._workContext.CurrentUserinformation.branch_code}','{this._workContext.CurrentUserinformation.login_code}',SYSDATE,'N','{currencyformat}',{exchangrate},'{newOrderNo}','{valuepercentflag}',{chargeserialno},'{newvoucherNo}')";
                            this._objectEntity.ExecuteSqlCommand(insertChargeQuery);
                            chargeserialno++;
                        }
                    }
                    if (model.INV_ITEM_CHARGE_VALUE != null)
                    {
                        dynamic invItemChargeValue = JsonConvert.DeserializeObject(model.INV_ITEM_CHARGE_VALUE);
                        foreach (var item in invItemChargeValue)
                        {
                            var invcdatas = JsonConvert.DeserializeObject(item.ToString());
                            var Inv_charge_item = "";
                            foreach (var invitemdata in invcdatas)
                            {
                                if (invitemdata.Name.ToString() == "ITEM_CODE")
                                {
                                    Inv_charge_item = invitemdata.Value.ToString();
                                }
                                if (Inv_charge_item != "0")
                                {
                                    if (invitemdata.Name.ToString() == "INV_ITEM_CHARGE_AMOUNT_WISE" || invitemdata.Name.ToString() == "INV_ITEM_CHARGE_QUANTITY_WISE")
                                    {
                                        dynamic invitemdatas = JsonConvert.DeserializeObject(invitemdata.Value.ToString());
                                        int invserialno = 1;
                                        if (invitemdatas.ToString() != "[]")
                                        {
                                            foreach (var invdatas in invitemdatas)
                                            {
                                                var invdatass = JsonConvert.DeserializeObject(invdatas.ToString());

                                                string CHARGE_CODE = "", CHARGE_TYPE = "", IMPACT_ON = "", APPLY_QUANTITY = "", VALUE_PERCENT_FLAG = "", VALUE_PERCENT_AMOUNT = "", CHARGE_AMOUNT = "", SUB_CODE = "", ACC_CODE = "", GL = "", APPORTION = "", APPLY_NO = "", BUDGET_CODE = "";
                                                var CALC = 0;
                                                foreach (var invdata in invdatass)
                                                {
                                                    var invdataname = invdata.Name.ToString();
                                                    var invdatavalue = invdata.Value.ToString();
                                                    if (invdataname == "CHARGE_CODE")
                                                    {
                                                        CHARGE_CODE = invdatavalue;
                                                    }
                                                    if (invdataname == "CHARGE_TYPE")
                                                    {
                                                        CHARGE_TYPE = invdatavalue;
                                                    }
                                                    if (invdataname == "IMPACT_ON")
                                                    {
                                                        IMPACT_ON = invdatavalue;
                                                    }
                                                    if (invdataname == "APPLY_QUANTITY")
                                                    {
                                                        APPLY_QUANTITY = invdatavalue;
                                                    }
                                                    if (invdataname == "VALUE_PERCENT_FLAG")
                                                    {
                                                        VALUE_PERCENT_FLAG = invdatavalue;
                                                    }
                                                    if (invdataname == "VALUE_PERCENT_AMOUNT")
                                                    {
                                                        VALUE_PERCENT_AMOUNT = invdatavalue;
                                                    }
                                                    if (invdataname == "CALC")
                                                    {

                                                        if (invdatavalue != null)
                                                        {
                                                            CHARGE_AMOUNT = invdatavalue;
                                                        }
                                                        else
                                                        {
                                                            CHARGE_AMOUNT = "0";
                                                        }

                                                    }
                                                    if (invdataname == "VALUE_PERCENT_AMOUNT")
                                                    {
                                                        if (invdatavalue != null)
                                                        {
                                                            CHARGE_AMOUNT = invdatavalue;
                                                        }
                                                        else
                                                        {
                                                            CHARGE_AMOUNT = "0";
                                                        }

                                                    }
                                                    if (invdataname == "SUB_CODE")
                                                    {
                                                        SUB_CODE = invdatavalue;
                                                    }
                                                    if (invdataname == "BUDGET_CODE")
                                                    {
                                                        BUDGET_CODE = invdatavalue;
                                                    }
                                                    if (invdataname == "ACC_CODE")
                                                    {
                                                        ACC_CODE = invdatavalue;
                                                    }
                                                    if (invdataname == "GL")
                                                    {
                                                        GL = invdatavalue;
                                                    }
                                                    if (invdataname == "APPORTION")
                                                    {
                                                        APPORTION = "F";
                                                    }
                                                    //if (invdataname == "CALC")
                                                    //{
                                                    //    if (invdatavalue == "")
        //                                            {
                                                    //        CALC = 0;
        //                                                }
        //                                                else
        //                                                {
                                                    //        CALC = Convert.ToInt32(invdatavalue);
        //                                                }
                                                    //}
                                                    if (invdataname == "APPLY_NO")
                                                    {
                                                        APPLY_NO = "I";
                                                    }
                                                }
                                                string transquery = string.Format(@"select to_number((max(to_number(TRANSACTION_NO)) + 1)) TRANSACTIONNO from CHARGE_TRANSACTION");
                                                int newtransno = this._objectEntity.SqlQuery<int>(transquery).FirstOrDefault();
                                                string insertChargeQuery = $@"INSERT INTO CHARGE_TRANSACTION(TRANSACTION_NO,TABLE_NAME,REFERENCE_NO,ITEM_CODE,APPLY_ON,ACC_CODE,CHARGE_CODE,CHARGE_TYPE_FLAG,CHARGE_AMOUNT,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,CURRENCY_CODE,EXCHANGE_RATE,VOUCHER_NO,CALCULATE_BY,SERIAL_NO,IMPACT_ON,VALUE_PERCENT_AMOUNT,NON_GL_FLAG,APPORTION_FLAG,GL_FLAG,SESSION_ROWID,SUB_CODE,BUDGET_CODE) VALUES('{newtransno}','{model.Table_Name}','{newOrderNo}','{Inv_charge_item}','I','{ACC_CODE}','{CHARGE_CODE}','{CHARGE_TYPE}', {CHARGE_AMOUNT},'{model.Form_Code}','{this._workContext.CurrentUserinformation.company_code}','{this._workContext.CurrentUserinformation.branch_code}','{this._workContext.CurrentUserinformation.login_code}',SYSDATE,'N','{currencyformat}',{exchangrate},'{newOrderNo}','{VALUE_PERCENT_FLAG}',{invserialno},'{IMPACT_ON}',{VALUE_PERCENT_AMOUNT},'Y','F','N','{newvoucherNo}','{SUB_CODE}','{BUDGET_CODE}')";
                                                this._objectEntity.ExecuteSqlCommand(insertChargeQuery);
                                                invserialno++;
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }
                    if (model.SHIPPING_DETAILS_VALUE != "[]")
                    {
                        string VEHICLE_CODE = "", VEHICLE_OWNER_NAME = "", VEHICLE_OWNER_NO = "", DRIVER_NAME = "", DRIVER_LICENSE_NO = "", DRIVER_MOBILE_NO = "", TRANSPORTER_CODE = "", START_FORM = "", DESTINATION = "", CN_NO = "", TRANSPORT_INVOICE_NO = "", WB_NO = "", VEHICLE_NO = "", GATE_ENTRY_NO = "";

                        decimal FREGHT_AMOUNT = 0, WB_WEIGHT = 0, FREIGHT_RATE = 0, LOADING_SLIP_NO = 0;

                        string TRANSPORT_INVOICE_DATE = "", DELIVERY_INVOICE_DATE = "", GATE_ENTRY_DATE = "", WB_DATE = "";

                        foreach (var sdval in shippingdetailsvalues)
                        {
                            var sddataname = sdval.Name.ToString();
                            var sdinvdatavalue = sdval.Value.ToString();
                            if (sddataname == "VEHICLE_CODE")
                            {
                                VEHICLE_CODE = sdinvdatavalue;

                            }
                            if (sddataname == "OWNER_NAME")
                            {
                                VEHICLE_OWNER_NAME = sdinvdatavalue;
                            }
                            if (sddataname == "OWNER_MOBILE_NO")
                            {
                                VEHICLE_OWNER_NO = sdinvdatavalue;
                            }
                            if (sddataname == "DRIVER_NAME")
                            {
                                DRIVER_NAME = sdinvdatavalue;
                            }
                            if (sddataname == "DRIVER_LICENCE_NO")
                            {
                                DRIVER_LICENSE_NO = sdinvdatavalue;
                            }
                            if (sddataname == "DRIVER_MOBILE_NO")
                            {
                                DRIVER_MOBILE_NO = sdinvdatavalue;
                            }
                            if (sddataname == "TRANSPORTER_CODE")
                            {
                                TRANSPORTER_CODE = sdinvdatavalue;
                            }
                            if (sddataname == "START_FORM")
                            {
                                START_FORM = sdinvdatavalue;
                            }
                            if (sddataname == "DESTINATION")
                            {
                                DESTINATION = sdinvdatavalue;
                            }
                            if (sddataname == "CN_NO")
                            {
                                CN_NO = sdinvdatavalue;
                            }
                            if (sddataname == "TRANSPORT_INVOICE_NO")
                            {
                                TRANSPORT_INVOICE_NO = sdinvdatavalue;
                            }
                            if (sddataname == "WB_NO")
                            {
                                WB_NO = sdinvdatavalue;
                            }
                            if (sddataname == "VEHICLE_NO")
                            {
                                VEHICLE_NO = sdinvdatavalue;
                            }
                            if (sddataname == "GATE_ENTRY_NO")
                            {
                                GATE_ENTRY_NO = sdinvdatavalue;
                            }
                            if (sddataname == "FREGHT_AMOUNT")
                            {
                                if (sdinvdatavalue != "")
                                {
                                    FREGHT_AMOUNT = Convert.ToDecimal(sdinvdatavalue);
                                }
                            }
                            if (sddataname == "WB_WEIGHT")
                            {
                                if (sdinvdatavalue != "")
                                {
                                    WB_WEIGHT = Convert.ToDecimal(sdinvdatavalue);
                                }
                            }
                            if (sddataname == "FREGHT_RATE")
                            {
                                if (sdinvdatavalue != "")
                                {
                                    FREIGHT_RATE = Convert.ToDecimal(sdinvdatavalue);
                                }
                            }
                            if (sddataname == "LOADING_SLIP_NO")
                            {
                                if (sdinvdatavalue != "")
                                {
                                    LOADING_SLIP_NO = Convert.ToDecimal(sdinvdatavalue);
                                }

                            }
                            if (sddataname == "TRANSPORT_INVOICE_DATE")
                            {
                                if (sdinvdatavalue != "")
                                {
                                    TRANSPORT_INVOICE_DATE = "TO_DATE('" + sdinvdatavalue + "')";
                                }
                                else
                                { TRANSPORT_INVOICE_DATE = "null"; }
                            }
                            if (sddataname == "DELIVERY_INVOICE_DATE")
                            {
                                if (sdinvdatavalue != "")
                                {
                                    DELIVERY_INVOICE_DATE = "TO_DATE('" + sdinvdatavalue + "')";
                                }
                                else
                                { DELIVERY_INVOICE_DATE = "null"; }
                            }
                            if (sddataname == "GATE_ENTRY_DATE")
                            {
                                if (sdinvdatavalue != "")
                                {
                                    GATE_ENTRY_DATE = "TO_DATE('" + sdinvdatavalue + "')";
                                }
                                else
                                { GATE_ENTRY_DATE = "null"; }
                            }
                            if (sddataname == "WB_DATE")
                            {
                                if (sdinvdatavalue != "")
                                {
                                    WB_DATE = "TO_DATE('" + sdinvdatavalue + "')";
                                }
                                else
                                { WB_DATE = "null"; }
                            }
                        }
                        if (VEHICLE_CODE != "")
                        {
                            string insertSDQuery = $@"INSERT INTO SHIPPING_TRANSACTION (VOUCHER_NO, FORM_CODE, VEHICLE_CODE, VEHICLE_OWNER_NAME, VEHICLE_OWNER_NO, DRIVER_NAME, DRIVER_LICENSE_NO, DRIVER_MOBILE_NO, TRANSPORTER_CODE, FREGHT_AMOUNT, START_FORM, DESTINATION, COMPANY_CODE, BRANCH_CODE, CREATED_DATE, CREATED_BY, DELETED_FLAG, TRANSPORT_INVOICE_NO, CN_NO, TRANSPORT_INVOICE_DATE, DELIVERY_INVOICE_DATE,WB_WEIGHT, WB_NO, WB_DATE,FREIGHT_RATE, VOUCHER_DATE,VEHICLE_NO, LOADING_SLIP_NO, GATE_ENTRY_NO, GATE_ENTRY_DATE) VALUES ('{newOrderNo}','{model.Form_Code}','{VEHICLE_CODE}','{VEHICLE_OWNER_NAME}','{VEHICLE_OWNER_NO}','{DRIVER_NAME}','{DRIVER_LICENSE_NO}','{DRIVER_MOBILE_NO}','{TRANSPORTER_CODE}',{FREGHT_AMOUNT},'{START_FORM}','{DESTINATION}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',SYSDATE,'{this._workContext.CurrentUserinformation.login_code}','N','{TRANSPORT_INVOICE_NO}','{CN_NO}',{TRANSPORT_INVOICE_DATE},{DELIVERY_INVOICE_DATE},{WB_WEIGHT},'{WB_NO}',{WB_DATE},{FREIGHT_RATE},{VoucherDate},'{VEHICLE_NO}',{LOADING_SLIP_NO},'{GATE_ENTRY_NO}',{GATE_ENTRY_DATE})";
                            this._objectEntity.ExecuteSqlCommand(insertSDQuery);
                        }
                    }


                    #region CLEAR CACHE
                    List<string> keystart = new List<string>();
                    keystart.Add("GetAllMenuItems");
                    keystart.Add("GetAllSalesOrderDetails");
                    keystart.Add("GetSalesOrderDetailFormDetailByFormCodeAndOrderNo");
                    keystart.Add("GetAllOrederNoByFlter");
                    keystart.Add("GetSubMenuList");
                    keystart.Add("GetSubMenuDetailList");
                    keystart.Add("GetFormCustomSetup");
                    List<string> Record = new List<string>();
                    Record = this._cacheManager.GetAllKeys();
                    this._cacheManager.RemoveCacheByKey(keystart, Record);
                    #endregion
                    if (orderno == "undefined" && insertmaintable == true && insertmaintable == true && model.Save_Flag == "0")
                    {
                        //trans.Commit();
                        return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = newOrderNo, SessionNo = newvoucherNo, VoucherDate = primarydate, FormCode = model.Form_Code });
                    }
                    else if (orderno == "undefined" && insertmaintable == true && insertmaintable == true && model.Save_Flag == "1")
                    {
                        /// trans.Commit();
                        return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTEDANDCONTINUE", STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = newOrderNo, SessionNo = newvoucherNo, VoucherDate = primarydate, FormCode = model.Form_Code });
                    }
                    else if (orderno == "undefined" && insertmaintable == true && insertmaintable == true && model.Save_Flag == "3")
                    {
                        // trans.Commit();
                        return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "SAVEANDPRINT", STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = newOrderNo, SessionNo = newvoucherNo, VoucherDate = primarydate, FormCode = model.Form_Code });
                    }
                    else
                    {
                        //        trans.Rollback();
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                    }
                }
                else
                {
                    if (model.Table_Name == "SA_SALES_ORDER" || model.Table_Name == "SA_SALES_CHALAN")
                    {
                        string deletequery = string.Format(@"DELETE FROM " + model.Table_Name + " where " + model.PRIMARY_COL_NAME + "='{0}' and COMPANY_CODE='{1}'", orderno, this._workContext.CurrentUserinformation.company_code);
                        this._objectEntity.ExecuteSqlCommand(deletequery);
                        int serialno = 1;
                        StringBuilder childvaluesbuilder = new StringBuilder();
                        StringBuilder masterchildvaluesbuilder = new StringBuilder();
                        foreach (var item in childcolumnvalues)
                        {
                            var itemarray = JsonConvert.DeserializeObject(item.ToString());
                            foreach (var data in itemarray)
                            {
                                var dataname = data.Name.ToString();
                                string[] datanamesplit = dataname.Split('_');
                                string datalastName = datanamesplit.Last();
                                var datavalue = data.Value;
                                if (datalastName == "DATE")
                                {
                                    if (datavalue.Value.ToString() == "")
                                    {
                                        childvaluesbuilder.Append("''").Append(",");
                                    }
                                    else
                                    {
                                        childvaluesbuilder.Append("TO_DATE(" + "'" + datavalue.Value + "'" + ",'DD-MON-YYYY hh24:mi:ss')").Append(",");
                                    }
                                }
                                else if (datalastName == "PRICE")
                                {
                                    if (datavalue.Value == null)
                                    {
                                        childvaluesbuilder.Append("''").Append(",");
                                    }
                                    else
                                    {
                                        childvaluesbuilder.Append(datavalue.Value).Append(",");
                                    }
                                }
                                else if (datalastName == "QUANTITY")
                                {
                                    try
                                    {
                                        datavalue.Value = Convert.ToDecimal(datavalue.Value);
                                    }
                                    catch (Exception ex)
                                    {
                                        datavalue.Value = null;
                                    }
                                    if (datavalue.Value == null)
                                    {
                                        childvaluesbuilder.Append("''").Append(",");
                                    }
                                    else
                                    {
                                        childvaluesbuilder.Append(datavalue.Value).Append(",");
                                    }
                                }
                                else
                                {
                                    if (datavalue.Value == null)
                                    {
                                        childvaluesbuilder.Append("' '").Append(",");
                                    }
                                    else
                                    {
                                        childvaluesbuilder.Append("'" + datavalue.Value + "'").Append(",");
                                    }
                                }
                            }
                            masterchildvaluesbuilder.Append(valuesbuilder);
                            masterchildvaluesbuilder.Append(childvaluesbuilder);
                            var values = masterchildvaluesbuilder.ToString().TrimEnd(',');
                            var insertQuery = string.Format(@"insert into " + model.Table_Name + "({0},{12}) values({1},{2},{3},{4},{5},{6},{7},'{8}',{9},{10},{11})", Columnbuilder, values, serialno, "'" + model.Form_Code + "'", "'" + this._workContext.CurrentUserinformation.company_code + "'", "'" + this._workContext.CurrentUserinformation.branch_code + "'", "'" + createdByForEdit + "'", createdDateForEdit, "N", sessionRowIDForedit, "'" + this._workContext.CurrentUserinformation.login_code.ToUpper() + "'", "SYSDATE", defaultCol);
                            this._objectEntity.ExecuteSqlCommand(insertQuery);
                            updatemaintable = true;
                            masterchildvaluesbuilder.Length = 0;
                            masterchildvaluesbuilder.Capacity = 0;
                            childvaluesbuilder.Length = 0;
                            childvaluesbuilder.Capacity = 0;
                            serialno++;
                        }
                        if (updatemaintable == true)
                        {
                            if (model.Save_Flag == "4")
                            {
                                string query = $@"UPDATE MASTER_TRANSACTION SET VOUCHER_AMOUNT='{ model.Grand_Total}',VOUCHER_DATE = {VoucherDate}, MODIFY_BY = '{this._workContext.CurrentUserinformation.login_code}',SYN_ROWID='{manualno}' , MODIFY_DATE = SYSDATE,CURRENCY_CODE='{currencyformat}',EXCHANGE_RATE={exchangrate},PRINT_COUNT={printcountedit} where VOUCHER_NO='{orderno}' and COMPANY_CODE='{this._workContext.CurrentUserinformation.company_code}'";
                                var rowCount = _objectEntity.ExecuteSqlCommand(query);
                            }
                            else
                            {
                                string query = $@"UPDATE MASTER_TRANSACTION SET VOUCHER_AMOUNT='{ model.Grand_Total}',VOUCHER_DATE = {VoucherDate}, MODIFY_BY = '{this._workContext.CurrentUserinformation.login_code}',SYN_ROWID='{manualno}' , MODIFY_DATE = SYSDATE,CURRENCY_CODE='{currencyformat}',EXCHANGE_RATE={exchangrate} where VOUCHER_NO='{orderno}' and COMPANY_CODE='{this._workContext.CurrentUserinformation.company_code}'";
                                var rowCount = _objectEntity.ExecuteSqlCommand(query);
                            }
                            updatemastertable = true;
                        }
                        if (updatemaintable == true && updatemastertable == true)
                        {
                            string deletecustomcolumn = string.Format(@"DELETE FROM CUSTOM_TRANSACTION where VOUCHER_NO='{0}' and COMPANY_CODE='{1}'", orderno, this._workContext.CurrentUserinformation.company_code);
                            this._objectEntity.ExecuteSqlCommand(deletecustomcolumn);
                            deletecustomtable = true;
                            if (customcolumn.Count > 0)
                            {
                                foreach (var r in customcolumn)
                                {
                                    if (deletecustomtable == true)
                                    {
                                        string insertQuery = $@"INSERT INTO CUSTOM_TRANSACTION(VOUCHER_NO,FIELD_NAME,FIELD_VALUE,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,MODIFY_DATE,MODIFY_BY) VALUES('{orderno}','{r.Key.ToString()}','{r.Value.ToString()}','{model.Form_Code}','{this._workContext.CurrentUserinformation.company_code}','{this._workContext.CurrentUserinformation.branch_code}','{createdByForEdit}',{createdDateForEdit},'N',SYSDATE,'{this._workContext.CurrentUserinformation.login_code}')";
                                        this._objectEntity.ExecuteSqlCommand(insertQuery);
                                        updatecustomtable = true;
                                    }
                                }
                            }
                        }
                        var getPrevChargeDataQuery = $@"SELECT CREATED_BY, CREATED_DATE FROM CHARGE_TRANSACTION WHERE VOUCHER_NO= '{orderno}'";
                        var chargeDefaultData = this._objectEntity.SqlQuery<SalesOrderDetail>(getPrevChargeDataQuery).ToList();
                        var chargeCreatedDateForEdit = "SYSDATE";
                        var chargeCreatedByForEdit = this._workContext.CurrentUserinformation.login_code.ToString();
                        foreach (var cdef in chargeDefaultData)
                        {
                            chargeCreatedDateForEdit = "TO_DATE('" + cdef.CREATED_DATE.ToString() + "', 'MM-DD-YYYY hh12:mi:ss pm')";
                            chargeCreatedByForEdit = cdef.CREATED_BY.ToString().ToUpper();
                        }
                        string deleteChargevalus = $@"DELETE FROM CHARGE_TRANSACTION WHERE VOUCHER_NO='{orderno}' AND COMPANY_CODE='{this._workContext.CurrentUserinformation.company_code}' AND BRANCH_CODE='{this._workContext.CurrentUserinformation.branch_code}'";
                        this._objectEntity.ExecuteSqlCommand(deleteChargevalus);
                        deletechargequery = true;
                        if (deletechargequery == true)
                        {
                            if (model.CHARGES != "[]")
                            {
                                var applyon = "";
                                var chargeapplyon = "";
                                var chargecode = "";
                                var chargetypeflag = "";
                                var amount = 0.00;
                                var amtPercent = "";
                                var valuepercentflag = "";
                                var accountCode = "";
                                var uchargeserialno = 1;
                                foreach (var citem in charges)
                                {
                                    string totalAmtQry = $@"SELECT TO_CHAR(SUM(CALC_TOTAL_PRICE)) FROM { model.Table_Name} WHERE {model.PRIMARY_COL_NAME} = '{orderno}' AND COMPANY_CODE={_workContext.CurrentUserinformation.company_code}";
                                    var totalAmt = this._objectEntity.SqlQuery<string>(totalAmtQry).FirstOrDefault();
                                    var citemrow = JsonConvert.DeserializeObject(citem.ToString());
                                    foreach (var data in citemrow)
                                    {
                                        if (data.Name == "APPLY_ON")
                                        {
                                            applyon = "D";
                                        }
                                        else if (data.Name == "CHARGE_APPLY_ON")
                                        {
                                            chargeapplyon = data.Value;
                                        }
                                        else if (data.Name == "CHARGE_CODE")
                                        {
                                            chargecode = data.Value;
                                        }
                                        else if (data.Name == "CHARGE_TYPE_FLAG")
                                        {
                                            chargetypeflag = data.Value;
                                        }
                                        else if (data.Name == "VALUE_PERCENT_AMOUNT")
                                        {
                                            amtPercent = data.Value;
                                        }
                                        else if (data.Name == "CHARGE_AMOUNT")
                                        {
                                            amount = data.Value;
                                        }
                                        else if (data.Name == "VALUE_PERCENT_FLAG")
                                        {
                                            valuepercentflag = data.Value;
                                        }
                                        else if (data.Name == "ACC_CODE")
                                        {
                                            accountCode = data.Value;
                                        }
                                    }
                                    // var newtransno = 0;
                                    string transquery = string.Format(@"select to_number((max(to_number(TRANSACTION_NO)) + 1)) TRANSACTIONNO from CHARGE_TRANSACTION");
                                    int newtransno = this._objectEntity.SqlQuery<int>(transquery).FirstOrDefault();
                                    string insertChargeQuery = $@"INSERT INTO CHARGE_TRANSACTION(TRANSACTION_NO,TABLE_NAME,REFERENCE_NO,APPLY_ON,ACC_CODE,CHARGE_CODE,CHARGE_TYPE_FLAG,CHARGE_AMOUNT,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,CURRENCY_CODE,EXCHANGE_RATE,VOUCHER_NO,CALCULATE_BY,SERIAL_NO,MODIFY_BY,MODIFY_DATE) VALUES('{newtransno}','{model.Table_Name}','{orderno}','{applyon}','{accountCode}','{chargecode}','{chargetypeflag}', {amount},'{model.Form_Code}','{this._workContext.CurrentUserinformation.company_code}','{this._workContext.CurrentUserinformation.branch_code}','{chargeCreatedByForEdit}',{chargeCreatedDateForEdit},'N','{currencyformat}',{exchangrate},'{orderno}','{valuepercentflag}',{uchargeserialno},'{this._workContext.CurrentUserinformation.login_code}',SYSDATE)";
                                    this._objectEntity.ExecuteSqlCommand(insertChargeQuery);
                                    uchargeserialno++;
                                }
                            }
                        }
                        if (model.INV_ITEM_CHARGE_VALUE != null)
                        {
                            dynamic invItemChargeValue = JsonConvert.DeserializeObject(model.INV_ITEM_CHARGE_VALUE);
                            foreach (var item in invItemChargeValue)
                            {
                                var invcdatas = JsonConvert.DeserializeObject(item.ToString());
                                var Inv_charge_item = "";
                                foreach (var invitemdata in invcdatas)
                                {
                                    if (invitemdata.Name.ToString() == "ITEM_CODE")
                                    {
                                        Inv_charge_item = invitemdata.Value.ToString();
                                    }
                                    if (Inv_charge_item != "0")
                                    {
                                        if (invitemdata.Name.ToString() == "INV_ITEM_CHARGE_AMOUNT_WISE" || invitemdata.Name.ToString() == "INV_ITEM_CHARGE_AMOUNT_WISE")
                                        {
                                            dynamic invitemdatas = JsonConvert.DeserializeObject(invitemdata.Value.ToString());
                                            int invserialno = 1;
                                            if (invitemdatas.ToString() != "[]")
                                            {
                                                foreach (var invdatas in invitemdatas)
                                                {
                                                    var invdatass = JsonConvert.DeserializeObject(invdatas.ToString());

                                                    string CHARGE_CODE = "", CHARGE_TYPE = "", IMPACT_ON = "", APPLY_QUANTITY = "", VALUE_PERCENT_FLAG = "", VALUE_PERCENT_AMOUNT = "", CHARGE_AMOUNT = "", SUB_CODE = "", ACC_CODE = "", GL = "", APPORTION = "", APPLY_NO = "", BUDGET_CODE = "";
                                                    var CALC = 0;
                                                    foreach (var invdata in invdatass)
                                                    {
                                                        var invdataname = invdata.Name.ToString();
                                                        var invdatavalue = invdata.Value.ToString();
                                                        if (invdataname == "CHARGE_CODE")
                                                        {
                                                            CHARGE_CODE = invdatavalue;
                                                        }
                                                        if (invdataname == "CHARGE_TYPE")
                                                        {
                                                            CHARGE_TYPE = invdatavalue;
                                                        }
                                                        if (invdataname == "IMPACT_ON")
                                                        {
                                                            IMPACT_ON = invdatavalue;
                                                        }
                                                        if (invdataname == "APPLY_QUANTITY")
                                                        {
                                                            APPLY_QUANTITY = invdatavalue;
                                                        }
                                                        if (invdataname == "VALUE_PERCENT_FLAG")
                                                        {
                                                            VALUE_PERCENT_FLAG = invdatavalue;
                                                        }
                                                        if (invdataname == "CALC")
                                                        {

                                                            if (invdatavalue != null)
                                                            {
                                                                CHARGE_AMOUNT = invdatavalue;
                                                            }
                                                            else
                                                            {
                                                                CHARGE_AMOUNT = "0";
                                                            }

                                                        }
                                                        if (invdataname == "VALUE_PERCENT_AMOUNT")
                                                        {
                                                            if (invdatavalue != null)
                                                            {
                                                                CHARGE_AMOUNT = invdatavalue;
                                                            }
                                                            else
                                                            {
                                                                CHARGE_AMOUNT = "0";
                                                            }

                                                        }
                                                        //if (invdataname == "CALC")
                                                        //{
                                                        //    if (invdatavalue == "")
        //                                                {
                                                        //        CALC = 0;
        //                                                    }
        //                                                    else
        //                                                    {
                                                        //        CALC =Convert.ToInt32(invdatavalue);
        //                                                    }
                                                        //}
                                                        if (invdataname == "SUB_CODE")
                                                        {
                                                            SUB_CODE = invdatavalue;
                                                        }
                                                        if (invdataname == "BUDGET_CODE")
                                                        {
                                                            BUDGET_CODE = invdatavalue;
                                                        }
                                                        if (invdataname == "ACC_CODE")
                                                        {
                                                            ACC_CODE = invdatavalue;
                                                        }
                                                        if (invdataname == "GL")
                                                        {
                                                            GL = invdatavalue;
                                                        }
                                                        if (invdataname == "APPORTION")
                                                        {
                                                            APPORTION = "F";
                                                        }
                                                        if (invdataname == "APPLY_NO")
                                                        {
                                                            APPLY_NO = "I";
                                                        }
                                                    }
                                                    if (CHARGE_CODE != "")
                                                    {
                                                        string transquery = string.Format(@"select to_number((max(to_number(TRANSACTION_NO)) + 1)) TRANSACTIONNO from CHARGE_TRANSACTION");
                                                        int newtransno = this._objectEntity.SqlQuery<int>(transquery).FirstOrDefault();
                                                        string insertChargeQuery = $@"INSERT INTO CHARGE_TRANSACTION(TRANSACTION_NO,TABLE_NAME,REFERENCE_NO,ITEM_CODE,APPLY_ON,ACC_CODE,CHARGE_CODE,CHARGE_TYPE_FLAG,CHARGE_AMOUNT,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,CURRENCY_CODE,EXCHANGE_RATE,VOUCHER_NO,CALCULATE_BY,SERIAL_NO,IMPACT_ON,VALUE_PERCENT_AMOUNT,NON_GL_FLAG,APPORTION_FLAG,GL_FLAG,SUB_CODE,BUDGET_CODE) VALUES('{newtransno}','{model.Table_Name}','{newOrderNo}','{Inv_charge_item}','I','{ACC_CODE}','{CHARGE_CODE}','{CHARGE_TYPE}', {CHARGE_AMOUNT},'{model.Form_Code}','{this._workContext.CurrentUserinformation.company_code}','{this._workContext.CurrentUserinformation.branch_code}','{this._workContext.CurrentUserinformation.login_code}',SYSDATE,'N','{currencyformat}',{exchangrate},'{newOrderNo}','{VALUE_PERCENT_FLAG}',{invserialno},'{IMPACT_ON}',{VALUE_PERCENT_AMOUNT},'Y','F','N','{SUB_CODE}','{BUDGET_CODE}')";
                                                        this._objectEntity.ExecuteSqlCommand(insertChargeQuery);

                                                        invserialno++;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        string deletesdvalus = $@"DELETE FROM SHIPPING_TRANSACTION WHERE VOUCHER_NO='{orderno}' AND COMPANY_CODE='{this._workContext.CurrentUserinformation.company_code}' AND BRANCH_CODE='{this._workContext.CurrentUserinformation.branch_code}'";
                        this._objectEntity.ExecuteSqlCommand(deletesdvalus);
                        deletesdquery = true;
                        if (deletesdquery == true)
                        {
                            if (model.SHIPPING_DETAILS_VALUE != "[]")
                            {
                                string VEHICLE_CODE = "", VEHICLE_OWNER_NAME = "", VEHICLE_OWNER_NO = "", DRIVER_NAME = "", DRIVER_LICENSE_NO = "", DRIVER_MOBILE_NO = "", TRANSPORTER_CODE = "", START_FORM = "", DESTINATION = "", CN_NO = "", TRANSPORT_INVOICE_NO = "", WB_NO = "", VEHICLE_NO = "", GATE_ENTRY_NO = "";

                                decimal FREGHT_AMOUNT = 0, WB_WEIGHT = 0, FREIGHT_RATE = 0, LOADING_SLIP_NO = 0;

                                string TRANSPORT_INVOICE_DATE = "", DELIVERY_INVOICE_DATE = "", GATE_ENTRY_DATE = "", WB_DATE = "";

                                foreach (var sdval in shippingdetailsvalues)
                                {
                                    var sddataname = sdval.Name.ToString();
                                    var sdinvdatavalue = sdval.Value.ToString();
                                    if (sddataname == "VEHICLE_CODE")
                                    {
                                        VEHICLE_CODE = sdinvdatavalue;

                                    }
                                    if (sddataname == "OWNER_NAME")
                                    {
                                        VEHICLE_OWNER_NAME = sdinvdatavalue;
                                    }
                                    if (sddataname == "OWNER_MOBILE_NO")
                                    {
                                        VEHICLE_OWNER_NO = sdinvdatavalue;
                                    }
                                    if (sddataname == "DRIVER_NAME")
                                    {
                                        DRIVER_NAME = sdinvdatavalue;
                                    }
                                    if (sddataname == "DRIVER_LICENCE_NO")
                                    {
                                        DRIVER_LICENSE_NO = sdinvdatavalue;
                                    }
                                    if (sddataname == "DRIVER_MOBILE_NO")
                                    {
                                        DRIVER_MOBILE_NO = sdinvdatavalue;
                                    }
                                    if (sddataname == "TRANSPORTER_CODE")
                                    {
                                        TRANSPORTER_CODE = sdinvdatavalue;
                                    }
                                    if (sddataname == "START_FORM")
                                    {
                                        START_FORM = sdinvdatavalue;
                                    }
                                    if (sddataname == "DESTINATION")
                                    {
                                        DESTINATION = sdinvdatavalue;
                                    }
                                    if (sddataname == "CN_NO")
                                    {
                                        CN_NO = sdinvdatavalue;
                                    }
                                    if (sddataname == "TRANSPORT_INVOICE_NO")
                                    {
                                        TRANSPORT_INVOICE_NO = sdinvdatavalue;
                                    }
                                    if (sddataname == "WB_NO")
                                    {
                                        WB_NO = sdinvdatavalue;
                                    }
                                    if (sddataname == "VEHICLE_NO")
                                    {
                                        VEHICLE_NO = sdinvdatavalue;
                                    }
                                    if (sddataname == "GATE_ENTRY_NO")
                                    {
                                        GATE_ENTRY_NO = sdinvdatavalue;
                                    }
                                    if (sddataname == "FREGHT_AMOUNT")
                                    {
                                        if (sdinvdatavalue != "")
                                        {
                                            FREGHT_AMOUNT = Convert.ToDecimal(sdinvdatavalue);
                                        }
                                    }
                                    if (sddataname == "WB_WEIGHT")
                                    {
                                        if (sdinvdatavalue != "")
                                        {
                                            WB_WEIGHT = Convert.ToDecimal(sdinvdatavalue);
                                        }
                                    }
                                    if (sddataname == "FREGHT_RATE")
                                    {
                                        if (sdinvdatavalue != "")
                                        {
                                            FREIGHT_RATE = Convert.ToDecimal(sdinvdatavalue);
                                        }
                                    }
                                    if (sddataname == "LOADING_SLIP_NO")
                                    {
                                        if (sdinvdatavalue != "")
                                        {
                                            LOADING_SLIP_NO = Convert.ToDecimal(sdinvdatavalue);
                                        }

                                    }
                                    if (sddataname == "TRANSPORT_INVOICE_DATE")
                                    {
                                        if (sdinvdatavalue != "")
                                        {
                                            TRANSPORT_INVOICE_DATE = "TO_DATE('" + sdinvdatavalue + "')";
                                        }
                                        else
                                        { TRANSPORT_INVOICE_DATE = "null"; }
                                    }
                                    if (sddataname == "DELIVERY_INVOICE_DATE")
                                    {
                                        if (sdinvdatavalue != "")
                                        {
                                            DELIVERY_INVOICE_DATE = "TO_DATE('" + sdinvdatavalue + "')";
                                        }
                                        else
                                        { DELIVERY_INVOICE_DATE = "null"; }
                                    }
                                    if (sddataname == "GATE_ENTRY_DATE")
                                    {
                                        if (sdinvdatavalue != "")
                                        {
                                            GATE_ENTRY_DATE = "TO_DATE('" + sdinvdatavalue + "')";
                                        }
                                        else
                                        { GATE_ENTRY_DATE = "null"; }
                                    }
                                    if (sddataname == "WB_DATE")
                                    {
                                        if (sdinvdatavalue != "")
                                        {
                                            WB_DATE = "TO_DATE('" + sdinvdatavalue + "')";
                                        }
                                        else
                                        { WB_DATE = "null"; }
                                    }
                                }
                                if (VEHICLE_CODE != "")
                                {
                                    string insertSDQuery = $@"INSERT INTO SHIPPING_TRANSACTION (VOUCHER_NO, FORM_CODE, VEHICLE_CODE, VEHICLE_OWNER_NAME, VEHICLE_OWNER_NO, DRIVER_NAME, DRIVER_LICENSE_NO, DRIVER_MOBILE_NO, TRANSPORTER_CODE, FREGHT_AMOUNT, START_FORM, DESTINATION, COMPANY_CODE, BRANCH_CODE, CREATED_DATE, CREATED_BY, DELETED_FLAG, TRANSPOTRT_INVOICE_NO, CN_NO, TRANSPORT_INVOICE_DATE, DELIVERY_INVOICE_DATE, TRANSPORT_INVOICE_NO, WB_WEIGHT, WB_NO, WB_DATE,FREGHT_RATE, VOUCHER_DATE,VEHICLE_NO, LOADING_SLIP_NO, GATE_ENTRY_NO, GATE_ENTRY_DATE) VALUES ('{orderno}','{model.Form_Code}','{VEHICLE_CODE}','{VEHICLE_OWNER_NAME}','{VEHICLE_OWNER_NO}','{DRIVER_NAME}','{DRIVER_LICENSE_NO}','{DRIVER_MOBILE_NO}','{TRANSPORTER_CODE}',{FREGHT_AMOUNT},'{START_FORM}','{DESTINATION}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',SYSDATE,'{this._workContext.CurrentUserinformation.login_code}','N','{TRANSPORT_INVOICE_NO}','{CN_NO}',{TRANSPORT_INVOICE_DATE},{DELIVERY_INVOICE_DATE},'{TRANSPORT_INVOICE_NO}',{WB_WEIGHT},'{WB_NO}',{WB_DATE},{FREIGHT_RATE},{VoucherDate},'{VEHICLE_NO}',{LOADING_SLIP_NO},'{GATE_ENTRY_NO}',{GATE_ENTRY_DATE})";
                                    this._objectEntity.ExecuteSqlCommand(insertSDQuery);
                                }
                            }
                        }
                        if (updatemaintable == true && updatemastertable == true && model.Save_Flag == "0")
                        {
                            //trans.Commit();
                            #region CLEAR CACHE
                            List<string> keystart = new List<string>();
                            keystart.Add("GetAllMenuItems");
                            keystart.Add("GetAllSalesOrderDetails");
                            keystart.Add("GetSalesOrderDetailFormDetailByFormCodeAndOrderNo");
                            keystart.Add("GetAllOrederNoByFlter");
                            keystart.Add("GetSubMenuList");
                            keystart.Add("GetSubMenuDetailList");
                            keystart.Add("GetFormCustomSetup");
                            keystart.Add("GetMUCodeByProductId");
                            keystart.Add("GetReferenceList");
                            keystart.Add("GetChargeData");
                            keystart.Add("GetChargeDataForEdit");
                            List<string> Record = new List<string>();
                            Record = this._cacheManager.GetAllKeys();
                            this._cacheManager.RemoveCacheByKey(keystart, Record);
                            #endregion
                            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATED", STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = primarycolumn, VoucherDate = primarydate, FormCode = model.Form_Code });
                        }
                        else if (updatemaintable == true && updatemastertable == true && model.Save_Flag == "4")
                        {
                            //trans.Commit();
                            #region CLEAR CACHE
                            List<string> keystart = new List<string>();
                            keystart.Add("GetAllMenuItems");
                            keystart.Add("GetAllSalesOrderDetails");
                            keystart.Add("GetSalesOrderDetailFormDetailByFormCodeAndOrderNo");
                            keystart.Add("GetAllOrederNoByFlter");
                            keystart.Add("GetSubMenuList");
                            keystart.Add("GetSubMenuDetailList");
                            keystart.Add("GetFormCustomSetup");
                            List<string> Record = new List<string>();
                            Record = this._cacheManager.GetAllKeys();
                            this._cacheManager.RemoveCacheByKey(keystart, Record);
                            #endregion
                            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATEDANDPRINT", STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = primarycolumn, VoucherDate = primarydate, FormCode = model.Form_Code });
                        }
                        else
                        {
                            //trans.Rollback();
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                        }
                    }
                    else
                    {
                        var updatetrig = UpdateTrigerTable(model);
                        if (updatetrig == "UPDATE_TRIGG_TABLES_SUCCESS" && model.Save_Flag == "0")
                        {
                            #region CLEAR CACHE
                            List<string> keystart = new List<string>();
                            keystart.Add("GetAllMenuItems");
                            keystart.Add("GetAllSalesOrderDetails");
                            keystart.Add("GetSalesOrderDetailFormDetailByFormCodeAndOrderNo");
                            keystart.Add("GetAllOrederNoByFlter");
                            keystart.Add("GetSubMenuList");
                            keystart.Add("GetSubMenuDetailList");
                            keystart.Add("GetFormCustomSetup");

                            List<string> Record = new List<string>();
                            Record = this._cacheManager.GetAllKeys();
                            this._cacheManager.RemoveCacheByKey(keystart, Record);
                            #endregion
                            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATED", STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = primarycolumn, VoucherDate = primarydate, FormCode = model.Form_Code });
                        }
                        else if (updatetrig == "UPDATE_TRIGG_TABLES_SUCCESS" && model.Save_Flag == "4")
                        {
                            #region CLEAR CACHE
                            List<string> keystart = new List<string>();
                            keystart.Add("GetAllMenuItems");
                            keystart.Add("GetAllSalesOrderDetails");
                            keystart.Add("GetSalesOrderDetailFormDetailByFormCodeAndOrderNo");
                            keystart.Add("GetAllOrederNoByFlter");
                            keystart.Add("GetSubMenuList");
                            keystart.Add("GetSubMenuDetailList");
                            keystart.Add("GetFormCustomSetup");
                            List<string> Record = new List<string>();
                            Record = this._cacheManager.GetAllKeys();
                            this._cacheManager.RemoveCacheByKey(keystart, Record);
                            #endregion
                            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "UPDATEDANDPRINT", STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = primarycolumn, VoucherDate = primarydate, FormCode = model.Form_Code });
                        }
                        else
                        {

                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        //}
        }



        [HttpPost]
        public HttpResponseMessage SaveFormDataOld(FormDetails model)
        {
            _logErp.WarnInDB("Sales order save method called with Model " + model);
            try
            {
                var orderNumber = model.Order_No;
                _logErp.InfoInFile("Order number while saving sales order========== " + orderNumber);
                var primaryDateColumn = _FormTemplateRepo.GetPrimaryDateByTableName(model.Table_Name);
                _logErp.WarnInDB("Primary Date Column For " + model.Table_Name + " is " + primaryDateColumn);
                var primaryColumnName = _FormTemplateRepo.GetPrimaryColumnByTableName(model.Table_Name);
                _logErp.WarnInDB("Primary Column For " + model.Table_Name + " is " + primaryColumnName);
                string primaryDate = string.Empty, primaryColumn = string.Empty;
                string createdDateString = "TO_DATE('" + DateTime.Now.ToString("dd-MMM-yyyy") + "'" + ",'DD-MON-YYYY hh24:mi:ss')", newOrderNo = _FormTemplateRepo.NewVoucherNo(this._workContext.CurrentUserinformation.company_code, model.Form_Code, DateTime.Now.ToString("dd-MMM-yyyy"), model.Table_Name),
                manualno = string.Empty, VoucherDate = createdDateString, validation_voucher_date = createdDateString, currencyformat = "NRS", newvoucherNo = string.Empty;
                _logErp.InfoInFile("New order number while saving sales order : " + newOrderNo);
                var form_ref = model.FROM_REF;
                decimal exchangeRate = 1;
                var masterColumn = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(model.Master_COLUMN_VALUE);
                _logErp.InfoInFile("Master column value :" + masterColumn);
                var customColumn = new Newtonsoft.Json.Linq.JObject();
                if (model.Custom_COLUMN_VALUE != null)
                {
                    customColumn = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(model.Custom_COLUMN_VALUE);
                }
                var childColumnValues = JsonConvert.DeserializeObject(model.Child_COLUMN_VALUE);
                _logErp.WarnInDB("Child Column Values : " + childColumnValues);
                var charges = JsonConvert.DeserializeObject(model.CHARGES);
                _logErp.WarnInDB("Charges for sales Order ================ : " + charges);
                var shippingDetailValues = JsonConvert.DeserializeObject(model.SHIPPING_DETAILS_VALUE);
                _logErp.WarnInDB("Shipping details for sales order : " + shippingDetailValues);
                SalesOrderMasterModel master = new SalesOrderMasterModel();
                StringBuilder columnBuilder = new StringBuilder();
                StringBuilder valuesBuilder = new StringBuilder();
                bool insertmaintable = false, updatemaintable = false, insertmastertable = false,
                updatemastertable = false, insertcustomtable = false, updatecustomtable = false, deletecustomtable = false, deletechargequery = false;

                //columnBuilder = TemplateAPIService.MapMasterColumn(model.Master_COLUMN_VALUE);
                // var masterTransactionColumn = TemplateAPIService.MapMasterColumn(model.Master_COLUMN_VALUE);
                //var getPrevDataQuery = $@"SELECT VOUCHER_NO,SESSION_ROWID, CREATED_BY, CREATED_DATE FROM MASTER_TRANSACTION WHERE VOUCHER_NO= '{orderNumber}'";
                //var defaultData = this._objectEntity.SqlQuery<SalesOrderDetail>(getPrevDataQuery).ToList();
                var defaultData = _saveDocTemplate.GetMasterTransactionByOrderNo(orderNumber);
                _logErp.InfoInFile(defaultData.Count() + " master records has been found while saving sales order==================== ");
                string defaultCol = "MODIFY_BY,MODIFY_DATE", createdDateForEdit = "", createdByForEdit = "", voucherNoForEdit = "";
                var sessionRowIDForedit = 0;
                if (defaultData.Count() > 0)
                {
                    foreach (var defData in defaultData)
                    {
                        voucherNoForEdit = defData.VOUCHER_NO.ToString();
                        createdDateForEdit = "TO_DATE('" + defData.CREATED_DATE.ToString() + "', 'MM-DD-YYYY hh12:mi:ss pm')";
                        createdByForEdit = defData.CREATED_BY.ToString().ToUpper();
                        sessionRowIDForedit = Convert.ToInt32(defData.SESSION_ROWID);
                    }
                }
                //  columnBuilder.Append(model.Child_COLUMNS);masterColumn
                // var staticsalesordercolumns = "SERIAL_NO, FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SESSION_ROWID";
                // columnBuilder.Append(staticsalesordercolumns);
                // valuesBuilder = TemplateAPIService.MapMasterColumn(orderNumber, voucherNoForEdit, primaryDateColumn, primaryColumnName, masterColumn, out primaryDate, out primaryColumn, out validation_voucher_date, out exchangeRate);
                var masterColumnValues = _saveDocTemplate.MapSalesOrderMasterColumnWithValue(model.Master_COLUMN_VALUE, primaryDateColumn, primaryColumn);
                var validation = checkValidation(model, validation_voucher_date);
                if (validation != "Valid") return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = validation, STATUS_CODE = (int)HttpStatusCode.OK });
                var type = childColumnValues.GetType();
                if (orderNumber == "undefined")
                {
                    if (model.Table_Name.ToLower() == "sa_sales_order")
                    {

                    }
                    int serialNo = 1;
                    StringBuilder masterchildvaluesbuilder = new StringBuilder();
                    var result = /*SaveDocTemplateService.MapChildColumn(childColumnValues);*/
                    masterchildvaluesbuilder.Append(valuesBuilder);
                   masterchildvaluesbuilder.Append(result);
                    var values = masterchildvaluesbuilder.ToString().TrimEnd(',');
                    var insertQuery = string.Format(@"insert into " + model.Table_Name + "({0}) values({1},{2},{3},{4},{5},{6},{7},'{8}',{9})", columnBuilder, values, serialNo, "'" + model.Form_Code + "'", "'" + this._workContext.CurrentUserinformation.company_code + "'", "'" + this._workContext.CurrentUserinformation.branch_code + "'", "'" + this._workContext.CurrentUserinformation.login_code + "'", createdDateString, 'N', "'" + newvoucherNo + "'");
                    this._objectEntity.ExecuteSqlCommand(insertQuery);
                   // insertmaintable = true;
                    masterchildvaluesbuilder.Length = 0;
                    masterchildvaluesbuilder.Capacity = 0;
                    result.Length = 0;
                    result.Capacity = 0;
                    serialNo++;
                    var VoucherNumberGeneratedNo = string.Empty;

                    if (/*insertmaintable == true*/true)
                    {
                        string updateTransactionNo = $"select FN_GET_VOUCHER_NO('{this._workContext.CurrentUserinformation.company_code}','{model.Form_Code}',{VoucherDate},'{newvoucherNo}') from dual ";
                        VoucherNumberGeneratedNo = _objectEntity.SqlQuery<string>(updateTransactionNo).FirstOrDefault();
                    }

                    if (model.FROM_REF)
                    {
                        //SaveDocTemplateService.GetFormReference(model, primaryColumn, VoucherNumberGeneratedNo);
                    }

                    if (/*insertmaintable == true*/true)
                    {
                        string insertmasterQuery = string.Format(@"INSERT INTO MASTER_TRANSACTION(VOUCHER_NO,VOUCHER_AMOUNT,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,DELETED_FLAG,CURRENCY_CODE,CREATED_DATE,VOUCHER_DATE,SYN_ROWID,EXCHANGE_RATE,SESSION_ROWID) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}',TO_DATE('{8}','DD-MON-YYYY hh24:mi:ss'),{9},'{10}',{11},'{12}')",
                            VoucherNumberGeneratedNo, model.Grand_Total, model.Form_Code, this._workContext.CurrentUserinformation.company_code, this._workContext.CurrentUserinformation.branch_code, this._workContext.CurrentUserinformation.login_code, 'N', currencyformat, DateTime.Now.ToString("dd-MMM-yyyy"), VoucherDate, manualno, exchangeRate, newvoucherNo);
                        this._objectEntity.ExecuteSqlCommand(insertmasterQuery);
                      //  insertmastertable = true;
                    }

                    if (customColumn.Count > 0/* && insertmaintable == true && insertmastertable == true*/)
                    {
                        foreach (var r in customColumn)
                        {
                            string insertQueryCustomCol = string.Format(@"INSERT INTO CUSTOM_TRANSACTION(VOUCHER_NO,FIELD_NAME,FIELD_VALUE,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SESSION_ROWID) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}',TO_DATE('{7}','DD-MON-YYYY hh24:mi:ss'),'{8}','{9}')",
                                VoucherNumberGeneratedNo, r.Key.ToString(), r.Value.ToString(), model.Form_Code, this._workContext.CurrentUserinformation.company_code, this._workContext.CurrentUserinformation.branch_code, this._workContext.CurrentUserinformation.login_code, DateTime.Now.ToString("dd-MMM-yyyy"), 'N', newvoucherNo);
                            this._objectEntity.ExecuteSqlCommand(insertQueryCustomCol);
                           // insertcustomtable = true;
                        }
                    }

                    if (model.CHARGES != "[]")
                    {

                    }

                    if (model.INV_ITEM_CHARGE_VALUE != null)
                    {

                    }
                }
                else
                {

                }
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Successfull", STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while saving sales order=========" + ex.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }



        [HttpPost]
        public HttpResponseMessage SaveFormData(FormDetails model)
        {

            _logErp.WarnInDB("Sales order save method called with Model " + model);
            using(var orderTransaction = _objectEntity.Database.BeginTransaction())
            {

                try
                {

                    var orderNumber = model.Order_No;
                    _logErp.WarnInDB("Order number while saving sales order========== " + orderNumber);

                    var primaryDateColumn = _FormTemplateRepo.GetPrimaryDateByTableName(model.Table_Name);
                    _logErp.WarnInDB("Primary Date Column For " + model.Table_Name + " is " + primaryDateColumn);

                    var primaryColumnName = _FormTemplateRepo.GetPrimaryColumnByTableName(model.Table_Name);
                    _logErp.WarnInDB("Primary Column For " + model.Table_Name + " is " + primaryColumnName);

                    string primaryDate = string.Empty, primaryColumn = string.Empty;
                    string createdDateString = "TO_DATE('" + DateTime.Now.ToString("dd-MMM-yyyy") + "'" + ",'DD-MON-YYYY hh24:mi:ss')", newOrderNo = _FormTemplateRepo.NewVoucherNo(this._workContext.CurrentUserinformation.company_code, model.Form_Code, DateTime.Now.ToString("dd-MMM-yyyy"), model.Table_Name),
                    manualno = string.Empty, VoucherDate = createdDateString, validation_voucher_date = createdDateString, currencyformat = "NRS", newvoucherNo = string.Empty;
                    _logErp.WarnInDB("New order number while saving sales order : " + newOrderNo);

                    var form_ref = model.FROM_REF;
                    decimal exchangrate = 1;

                    var customColumn = new Newtonsoft.Json.Linq.JObject();
                    var customColList = new List<CustomOrderColumn>();
                    if (model.Custom_COLUMN_VALUE != null)
                    {

                        customColList = _saveDocTemplate.MapCustomOrderColumnWithValue(model.Custom_COLUMN_VALUE);
                    }
                    _logErp.WarnInDB("Custome column for sales Order ================ : " + customColList);


                    var charges = _saveDocTemplate.MapChargesColumnWithValue(model.CHARGES);
                    _logErp.WarnInDB("Charges for sales Order ================ : " + charges);

                   
                    var shippingDetailValues = _saveDocTemplate.MapShippingDetailsColumnValue(model.SHIPPING_DETAILS_VALUE);
                    _logErp.WarnInDB("Shipping details for sales order : " + shippingDetailValues);

                    var batchTransaction = _saveDocTemplate.MapBatchTransactionValue(model.SERIAL_TRACKING_VALUE);
                    _logErp.WarnInDB("Batch Transaction details for sales order : " + batchTransaction);

                    

                    var masterColumn = _saveDocTemplate.MapSalesOrderMasterColumnWithValue(model.Master_COLUMN_VALUE, primaryDateColumn, primaryColumn);
                    //var validation = checkValidation(model, masterColumn.VOUCHER_DATE.ToString());
                    //if (validation != "Valid")
                    //{
                    //    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = validation, STATUS_CODE = (int)HttpStatusCode.OK });
                    //}

                    if (orderNumber == "undefined")
                    {
                        var commonSaveFieldForSales = new CommonFieldForSales()
                        {
                            OrderNumber = newOrderNo,
                            NewOrderNumber = newOrderNo,
                            VoucherDate = VoucherDate,
                            FormCode = model.Form_Code,
                            ExchangeRate = exchangrate.ToString(),
                            CurrencyFormat = currencyformat,
                            GrandTotal = model.Grand_Total,
                            NewVoucherNumber = newvoucherNo,
                            FormRef = model.FROM_REF,
                            TableName = model.Table_Name,
                            SaveFlag = model.Save_Flag,

                        };
                        var orderSaveResponse = new ResponseMessage();

                        if (model.Table_Name.ToLower() == "sa_sales_order")
                        {
                            var masterColumn1 = _saveDocTemplate.MapSalesOrderMasterColumnWithValue(model.Master_COLUMN_VALUE, primaryDateColumn, primaryColumn);
                            _logErp.WarnInDB("Master column value for sales order:" + masterColumn);
                            var childColVal = _saveDocTemplate.MapOrderChildColumnWithValue(model.Child_COLUMN_VALUE);
                            _logErp.WarnInDB("Child Column Values sales order: " + childColVal);




                            var childLineItemColVal = _saveDocTemplate.MapOrderChildLineItemColumnWithValue(model.Child_COLUMN_VALUE);
                            _logErp.WarnInDB("Child Column Values sales order: " + childLineItemColVal);



                            var validation = checkValidation(model, masterColumn1.ORDER_DATE.ToString());
                            if (validation != "Valid")
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = validation, STATUS_CODE = (int)HttpStatusCode.OK });
                            }
                            var salesOrderData = new SalesOrderDetailModel()
                            {
                                MasterTransaction = masterColumn,
                                ChildTransaction = childColVal,
                                ChargeTransaction = charges,
                                ShippingTransaction = shippingDetailValues,
                                CustomOrderTransaction= customColList,
                                BatchTransaction= batchTransaction

                            };
                            commonSaveFieldForSales.ManualNumber = salesOrderData.MasterTransaction.MANUAL_NO;
                                 orderSaveResponse = _saveDocTemplateSalesModule.SaveSalesOrderFormData(salesOrderData,commonSaveFieldForSales,_objectEntity);
                            _logErp.WarnInDB("Sales Order Saved successfullly :" + orderSaveResponse);

                        }
                        else if (model.Table_Name.ToLower() == "sa_sales_chalan")
                        {
                            var masterChalanColumn = _saveDocTemplate.MapSalesChalanMasterColumnWithValue(model.Master_COLUMN_VALUE, primaryDateColumn, primaryColumn);
                            _logErp.WarnInDB("Master column value sales chalan:" + masterColumn);
                            var childChalanColVal = _saveDocTemplate.MapChalanChildColumnWithValue(model.Child_COLUMN_VALUE);
                            _logErp.WarnInDB("Child Column Values sales chalan: " + childChalanColVal);
                            var validation = checkValidation(model, masterChalanColumn.CHALAN_DATE.ToString());
                            if (validation != "Valid")
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = validation, STATUS_CODE = (int)HttpStatusCode.OK });
                            }
                            var salesChalanData = new SalesChalanDetailModel() {
                                MasterChalanTransaction = masterChalanColumn,
                                ChildChalanTransaction = childChalanColVal,
                                ChargeTransaction = charges,
                                ShippingTransaction = shippingDetailValues,
                               RefenceModel= model.REF_MODEL,
                                BatchTransaction = batchTransaction
                            };
                            commonSaveFieldForSales.ManualNumber = salesChalanData.MasterChalanTransaction.MANUAL_NO;
                            orderSaveResponse = _saveDocTemplateSalesModule.SaveSalesChalanFormData(salesChalanData,commonSaveFieldForSales, _objectEntity);
                            _logErp.WarnInDB("Sales Chalan Saved successfullly :" + orderSaveResponse);

                        }
                        else if (model.Table_Name.ToLower() == "sa_sales_invoice")
                        {
                            if (model.Child_COLUMN_VALUE != null)
                            {
                                //var lineitemcharges = _saveDocTemplate.MapLineItemChargesColumnWithValue(model.Child_COLUMN_VALUE);
                                //string[] lineitemcharges = model.Child_COLUMN_VALUE.Split(',');
                                string[] lineitemcharges = model.Child_COLUMN_VALUE.Split('}');
                                _logErp.WarnInDB("Charges for sales Order ================ : " + lineitemcharges);
                            }

                            var masterInvoiceColumn = _saveDocTemplate.MapSalesInvoiceMasterColumnWithValue(model.Master_COLUMN_VALUE, primaryDateColumn, primaryColumn);

                            masterInvoiceColumn.ChargeList = _saveDocTemplate.MapLineItemChargesColumnWithValue(model.Child_COLUMN_VALUE).ToList();

                            _logErp.WarnInDB("Master column value sales invoice:" + masterColumn);
                            var childColVal = _saveDocTemplate.MapInvoiceChildColumnWithValue(model.Child_COLUMN_VALUE);


                            var childLineItemColVal = _saveDocTemplate.MapInvoiceChildLineItemColumnWithValue(model.Child_COLUMN_VALUE);


                            var validation = checkValidation(model, masterInvoiceColumn.SALES_DATE.ToString());
                            if (validation != "Valid")
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = validation, STATUS_CODE = (int)HttpStatusCode.OK });
                            }
                            _logErp.WarnInDB("Child Column Values sales invoice: " + childColVal);

                            var salesInvoiceData = new SalesInvoiceDetailModel() {
                                MasterInvoiceTransaction = masterInvoiceColumn,
                                ChildInvoiceTransaction = childColVal,
                                ChargeTransaction = charges,
                                ShippingTransaction = shippingDetailValues,
                               CustomOrderTransaction = customColList,
                                RefenceModel = model.REF_MODEL,
                                BatchTransaction = batchTransaction,
                                
                            };
                            commonSaveFieldForSales.ManualNumber = salesInvoiceData.MasterInvoiceTransaction.MANUAL_NO;
                            orderSaveResponse = _saveDocTemplateSalesModule.SaveSalesInvoiceFormData(salesInvoiceData,commonSaveFieldForSales, _objectEntity);
                          
                            _logErp.WarnInDB("Sales Invoice Saved successfullly :" + orderSaveResponse);

                        }
                        else if (model.Table_Name.ToLower() == "sa_sales_return")
                        {
                            var masterReturnColumn = _saveDocTemplate.MapSalesReturnMasterColumnWithValue(model.Master_COLUMN_VALUE, primaryDateColumn, primaryColumn);
                            _logErp.WarnInDB("Master column value sales return:" + masterColumn);
                            var childColVal = _saveDocTemplate.MapReturnChildColumnWithValue(model.Child_COLUMN_VALUE);
                            _logErp.WarnInDB("Child Column Values sales return: " + childColVal);
                            var validation = checkValidation(model, masterReturnColumn.RETURN_DATE.ToString());
                            if (validation != "Valid")
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = validation, STATUS_CODE = (int)HttpStatusCode.OK });
                            }
                            var salesReturnData = new SalesReturnDetailModel() {
                                MasterReturnTransaction = masterReturnColumn,
                                ChildReturnTransaction = childColVal,
                                ChargeTransaction = charges,
                                ShippingTransaction = shippingDetailValues,
                                RefenceModel = model.REF_MODEL,
                                BatchTransaction = batchTransaction
                            };
                            commonSaveFieldForSales.ManualNumber = salesReturnData.MasterReturnTransaction.MANUAL_NO;
                            orderSaveResponse = _saveDocTemplateSalesModule.SaveSalesReturnFormData(salesReturnData,commonSaveFieldForSales, _objectEntity);
                            _logErp.WarnInDB("Sales Return Saved successfullly :" + orderSaveResponse);
                        }
                        else
                        {
                            var oldResponse = SaveFormDataOldMethod(model);
                            var response = oldResponse.Content.ReadAsAsync<ResponseMessage>();
                            if(response.Result.STATUS_CODE=="200" && response.Result.MESSAGE == "INSERTED")
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTED", STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = newOrderNo, SessionNo = newvoucherNo, VoucherDate = response.Result.VoucherDate, FormCode = model.Form_Code });
                            }
                            else if(response.Result.STATUS_CODE=="200" && response.Result.Message == "INSERTEDANDCONTINUE")
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "INSERTEDANDCONTINUE", STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = newOrderNo, SessionNo = newvoucherNo, VoucherDate = response.Result.VoucherDate, FormCode = model.Form_Code });
                            }
                            else if(response.Result.STATUS_CODE=="200" && response.Result.MESSAGE == "SAVEANDPRINT")
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "SAVEANDPRINT", STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = newOrderNo, SessionNo = newvoucherNo, VoucherDate = response.Result.VoucherDate, FormCode = model.Form_Code });
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                            }
                        }

                        if(orderSaveResponse.StatusCode=="200" && orderSaveResponse.SaveFlag == "0")
                        {
                            orderTransaction.Commit();
                            //if (model.Table_Name.ToLower() == "sa_sales_invoice")
                            //{
                            //    var Update = _saveDocTemplateSalesModule.SavePostedTransactionValue(commonSaveFieldForSales, _objectEntity);
                            //}
                            //orderTransaction.Commit();
                            #region CLEAR CACHE
                            List<string> keystart = new List<string>();
                            keystart.Add("GetAllMenuItems");
                            keystart.Add("GetAllSalesOrderDetails");
                            keystart.Add("GetSalesOrderDetailFormDetailByFormCodeAndOrderNo");
                            keystart.Add("GetAllOrederNoByFlter");
                            keystart.Add("GetSubMenuList");
                            keystart.Add("GetSubMenuDetailList");
                            keystart.Add("GetFormCustomSetup");
                            keystart.Add("GetMUCodeByProductId");
                            keystart.Add("GetReferenceList");
                            keystart.Add("GetChargeData");
                            keystart.Add("GetChargeDataForEdit");
                            List<string> Record = new List<string>();
                            Record = this._cacheManager.GetAllKeys();
                            this._cacheManager.RemoveCacheByKey(keystart, Record);
                            #endregion
                            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE =orderSaveResponse.Message, STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = newOrderNo, SessionNo = newvoucherNo, VoucherDate = orderSaveResponse.VoucherDate, FormCode = model.Form_Code });
                        }
                        else if (orderSaveResponse.StatusCode=="200" && orderSaveResponse.SaveFlag == "1")
                        {
                            orderTransaction.Commit();
                            #region CLEAR CACHE
                            List<string> keystart = new List<string>();
                            keystart.Add("GetAllMenuItems");
                            keystart.Add("GetAllSalesOrderDetails");
                            keystart.Add("GetSalesOrderDetailFormDetailByFormCodeAndOrderNo");
                            keystart.Add("GetAllOrederNoByFlter");
                            keystart.Add("GetSubMenuList");
                            keystart.Add("GetSubMenuDetailList");
                            keystart.Add("GetFormCustomSetup");
                            keystart.Add("GetMUCodeByProductId");
                            keystart.Add("GetReferenceList");
                            keystart.Add("GetChargeData");
                            keystart.Add("GetChargeDataForEdit");
                            List<string> Record = new List<string>();
                            Record = this._cacheManager.GetAllKeys();
                            this._cacheManager.RemoveCacheByKey(keystart, Record);
                            #endregion
                            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = orderSaveResponse.Message, STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = newOrderNo, SessionNo = newvoucherNo, VoucherDate = orderSaveResponse.VoucherDate, FormCode = model.Form_Code });

                        }
                        else if (orderSaveResponse.StatusCode == "200" && orderSaveResponse.SaveFlag == "3")
                        {
                            orderTransaction.Commit();
                            #region CLEAR CACHE
                            List<string> keystart = new List<string>();
                            keystart.Add("GetAllMenuItems");
                            keystart.Add("GetAllSalesOrderDetails");
                            keystart.Add("GetSalesOrderDetailFormDetailByFormCodeAndOrderNo");
                            keystart.Add("GetAllOrederNoByFlter");
                            keystart.Add("GetSubMenuList");
                            keystart.Add("GetSubMenuDetailList");
                            keystart.Add("GetFormCustomSetup");
                            keystart.Add("GetMUCodeByProductId");
                            keystart.Add("GetReferenceList");
                            keystart.Add("GetChargeData");
                            keystart.Add("GetChargeDataForEdit");
                            List<string> Record = new List<string>();
                            Record = this._cacheManager.GetAllKeys();
                            this._cacheManager.RemoveCacheByKey(keystart, Record);
                            #endregion
                            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = orderSaveResponse.Message, STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = newOrderNo, SessionNo = newvoucherNo, VoucherDate = orderSaveResponse.VoucherDate, FormCode = model.Form_Code });
                        }
                        else
                        {
                            _logErp.ErrorInDB("Warning , data doesnot saved successfully :" + orderSaveResponse);
                            orderTransaction.Rollback();
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = orderSaveResponse.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                        }
                      
                    }
                    else
                    {
                        var orderUpdateResponse = new ResponseMessage();
                        var commonUpdateFieldForSales = new CommonFieldForSales()
                        {
                            OrderNumber = orderNumber,
                            NewOrderNumber = newOrderNo,
                            VoucherDate = VoucherDate,
                            FormCode = model.Form_Code,
                            ExchangeRate = exchangrate.ToString(),
                            CurrencyFormat = currencyformat,
                            GrandTotal = model.Grand_Total,
                            NewVoucherNumber = newvoucherNo,
                            FormRef = model.FROM_REF,
                            TableName = model.Table_Name,
                            SaveFlag = model.Save_Flag,
                            PrimaryColumn = primaryColumnName,
                            PrimaryDateColumn = primaryDate
                        };
                        if (model.Table_Name.ToLower() == "sa_sales_order")
                        {
                            var masterColumn1 = _saveDocTemplate.MapSalesOrderMasterColumnWithValue(model.Master_COLUMN_VALUE, primaryDateColumn, primaryColumn);
                            _logErp.WarnInDB("Master column value to update sales order:" + masterColumn);
                            var childColVal = _saveDocTemplate.MapOrderChildColumnWithValue(model.Child_COLUMN_VALUE);
                            _logErp.WarnInDB("Child Column Values to update : " + childColVal);

                            var salesOrderData = new SalesOrderDetailModel()
                            {
                                MasterTransaction = masterColumn,
                                ChildTransaction = childColVal,
                                ChargeTransaction = charges,
                                ShippingTransaction = shippingDetailValues,
                                CustomOrderTransaction = customColList,
                                BatchTransaction = batchTransaction


                            };
                            commonUpdateFieldForSales.ManualNumber = salesOrderData.MasterTransaction.MANUAL_NO;
                          orderUpdateResponse =  _saveDocTemplateSalesModule.UpdateSalesOrderFormData(salesOrderData, commonUpdateFieldForSales);
                            //if (orderUpdateResponse.StatusCode != "500")
                            //    orderTransaction.Commit();
                            _logErp.WarnInDB("Sales order updated successfully : " + childColVal);
                            
                        }
                        else if (model.Table_Name.ToLower() == "sa_sales_chalan")
                        {
                            var masterChalanColumn = _saveDocTemplate.MapSalesChalanMasterColumnWithValue(model.Master_COLUMN_VALUE, primaryDateColumn, primaryColumn);
                            _logErp.WarnInDB("Master column value to update sales chalan:" + masterColumn);
                            var childColVal = _saveDocTemplate.MapChalanChildColumnWithValue(model.Child_COLUMN_VALUE);
                            _logErp.WarnInDB("Child Column Values to update sales chalan: " + childColVal);

                            var salesChalanData = new SalesChalanDetailModel() {
                                MasterChalanTransaction = masterChalanColumn,
                                ChildChalanTransaction = childColVal,
                                ChargeTransaction = charges,
                                ShippingTransaction = shippingDetailValues,
                                CustomOrderTransaction = customColList,
                                BatchTransaction = batchTransaction
                            };
                            commonUpdateFieldForSales.ManualNumber = salesChalanData.MasterChalanTransaction.MANUAL_NO;
                            orderUpdateResponse = _saveDocTemplateSalesModule.UpdateSalesChalanFormData(salesChalanData, commonUpdateFieldForSales, _objectEntity);
                            _logErp.WarnInDB("Sales chalan updated successfully : " + childColVal);
                        }
                      else if (model.Table_Name.ToLower() == "sa_sales_invoice")
                        {
                            var masterInvoiceColumn = _saveDocTemplate.MapSalesInvoiceMasterColumnWithValue(model.Master_COLUMN_VALUE, primaryDateColumn, primaryColumn);
                            _logErp.WarnInDB("Master column value to update salse invoice:" + masterColumn);
                            var childColVal = _saveDocTemplate.MapInvoiceChildColumnWithValue(model.Child_COLUMN_VALUE);
                            _logErp.WarnInDB("Child Column Values to update sales invoice: " + childColVal);

                            var salesInvoiceData = new SalesInvoiceDetailModel()
                            {
                                MasterInvoiceTransaction = masterInvoiceColumn,
                                ChildInvoiceTransaction = childColVal,
                                ChargeTransaction = charges,
                                ShippingTransaction = shippingDetailValues,
                                CustomOrderTransaction = customColList,
                                BatchTransaction = batchTransaction
                            };
                            commonUpdateFieldForSales.ManualNumber = salesInvoiceData.MasterInvoiceTransaction.MANUAL_NO;
                            orderUpdateResponse = _saveDocTemplateSalesModule.UpdateSalesInvoiceFormData(salesInvoiceData, commonUpdateFieldForSales, _objectEntity);
                            //if (orderUpdateResponse.StatusCode != "500")
                            //    orderTransaction.Commit();
                            _logErp.WarnInDB("Sales invoice updated successfully : " + childColVal);
                        }
                     else if (model.Table_Name.ToLower() == "sa_sales_return")
                        {
                            var masterReturnColumn = _saveDocTemplate.MapSalesReturnMasterColumnWithValue(model.Master_COLUMN_VALUE, primaryDateColumn, primaryColumn);
                            _logErp.WarnInDB("Master column value to update sales return:" + masterColumn);
                            var childColVal = _saveDocTemplate.MapReturnChildColumnWithValue(model.Child_COLUMN_VALUE);
                            _logErp.WarnInDB("Child Column Values to update sales return: " + childColVal);

                            var salesReturnData = new SalesReturnDetailModel()
                            {
                                MasterReturnTransaction = masterReturnColumn,
                                ChildReturnTransaction = childColVal,
                                ChargeTransaction = charges,
                                ShippingTransaction = shippingDetailValues,
                                CustomOrderTransaction = customColList,
                                BatchTransaction = batchTransaction

                            };
                            commonUpdateFieldForSales.ManualNumber = salesReturnData.MasterReturnTransaction.MANUAL_NO;
                            orderUpdateResponse = _saveDocTemplateSalesModule.UpdateSalesReturnFormData(salesReturnData, commonUpdateFieldForSales, _objectEntity);
                            //if (orderUpdateResponse.StatusCode != "500")
                            //    orderTransaction.Commit();
                            _logErp.WarnInDB("Sales invoice updated successfully : " + childColVal);
                        }
                        else
                        {
                           
                            var updatetrig = UpdateTrigerTable(model);
                            if (updatetrig == "UPDATE_TRIGG_TABLES_SUCCESS" && model.Save_Flag == "0")
                            {
                                
                                var responseMessage = new ResponseMessage()
                                {
                                    StatusCode = 200.ToString(),
                                    Message = "UPDATED",
                                    VoucherNo = model.Order_No,
                                    SessionNo = "123456",
                                    VoucherDate =DateTime.Now.ToShortDateString(),
                                    FormCode =model.Form_Code,
                                    SaveFlag = 0.ToString()
                                };
                            }
                            if (updatetrig == "UPDATE_TRIGG_TABLES_SUCCESS" && model.Save_Flag == "4")
                            {
                               
                                var responseMessage = new ResponseMessage()
                                {
                                    StatusCode = 200.ToString(),
                                    Message = "UPDATEDANDPRINT",
                                    VoucherNo = model.Order_No,
                                    SessionNo = "123456",
                                    VoucherDate = DateTime.Now.ToShortDateString(),
                                    FormCode = model.Form_Code,
                                    SaveFlag = 4.ToString()
                                };
                            }




                        }


                        if (orderUpdateResponse.StatusCode == "200" && orderUpdateResponse.SaveFlag== "0")
                        {
                            orderUpdateResponse.Message = "UPDATED";

                        
                            orderTransaction.Commit();
                            #region CLEAR CACHE
                            List<string> keystart = new List<string>();
                            keystart.Add("GetAllMenuItems");
                            keystart.Add("GetAllSalesOrderDetails");
                            keystart.Add("GetSalesOrderDetailFormDetailByFormCodeAndOrderNo");
                            keystart.Add("GetAllOrederNoByFlter");
                            keystart.Add("GetSubMenuList");
                            keystart.Add("GetSubMenuDetailList");
                            keystart.Add("GetFormCustomSetup");
                            keystart.Add("GetMUCodeByProductId");
                            keystart.Add("GetReferenceList");
                            keystart.Add("GetChargeData");
                            keystart.Add("GetChargeDataForEdit");
                            List<string> Record = new List<string>();
                            Record = this._cacheManager.GetAllKeys();
                            this._cacheManager.RemoveCacheByKey(keystart, Record);
                            #endregion
                            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = orderUpdateResponse.Message, STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo =model.Order_No, VoucherDate = orderUpdateResponse.VoucherDate, FormCode = model.Form_Code });
                        }
                        else if (orderUpdateResponse.StatusCode == "200" && orderUpdateResponse.SaveFlag == "4")
                        {
                            orderUpdateResponse.Message = "UPDATEDANDPRINT";

                            
                            orderTransaction.Commit();
                            #region CLEAR CACHE
                            List<string> keystart = new List<string>();
                            keystart.Add("GetAllMenuItems");
                            keystart.Add("GetAllSalesOrderDetails");
                            keystart.Add("GetSalesOrderDetailFormDetailByFormCodeAndOrderNo");
                            keystart.Add("GetAllOrederNoByFlter");
                            keystart.Add("GetSubMenuList");
                            keystart.Add("GetSubMenuDetailList");
                            keystart.Add("GetFormCustomSetup");
                            keystart.Add("GetMUCodeByProductId");
                            keystart.Add("GetReferenceList");
                            keystart.Add("GetChargeData");
                            keystart.Add("GetChargeDataForEdit");
                            List<string> Record = new List<string>();
                            Record = this._cacheManager.GetAllKeys();
                            this._cacheManager.RemoveCacheByKey(keystart, Record);
                            #endregion
                            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = orderUpdateResponse.Message, STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = model.Order_No, VoucherDate = orderUpdateResponse.VoucherDate, FormCode = model.Form_Code });
                        }
                        else
                        {
                            //orderTransaction.Commit();
                            orderTransaction.Rollback();
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                        }

                    }
                   
                }
                catch (Exception ex)
                {
                    //orderTransaction.Commit();
                    orderTransaction.Rollback();
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                    // throw new Exception(ex.Message);
                }
            }
        }
        [HttpGet]
        public string IsLogedinUserAdmin()
        {
            var user = _FormTemplateRepo.getusertype();
            return user;
        }
        [HttpGet]
        public string GetLogedinUser()
        {
            var user = _FormTemplateRepo.getLoginedUser();
            return user;
        }
        #endregion



        public string UpdateTrigerTable(FormDetails model)
        {
            //using (var uptrans = _objectEntity.Database.BeginTransaction())
            //{
                try
                {
                    bool upmaintable = false, uptranstable = false, upcstmtable = false, upchargetable = false, upinvchargetable = false, upspdtable = false;
                    StringBuilder UpdateMasterQuerybuilder = new StringBuilder();
                    var primarycolumnname = _FormTemplateRepo.GetPrimaryColumnByTableName(model.Table_Name);

                    //UPDATE MAIN TABLE START
                    Newtonsoft.Json.Linq.JObject updatemastercolumn = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(model.Master_COLUMN_VALUE);
                    foreach (var m in updatemastercolumn)
                    {

                        if (m.Key.ToString().Contains("PRICE") == true)
                        {
                            UpdateMasterQuerybuilder.Append(m.Key).Append("=").Append(m.Value).Append(",");
                        }
                        if (m.Key.ToString().Contains("QUANTITY") == true)
                        {
                            UpdateMasterQuerybuilder.Append(m.Key).Append("=").Append(m.Value).Append(",");
                        }
                        if (m.Key.ToString().Contains("DATE") == true)
                        {
                            UpdateMasterQuerybuilder.Append(m.Key).Append("=").Append("TO_DATE(" + "'" + m.Value + "'" + ",'DD-MON-YYYY hh24:mi:ss')").Append(",");
                        }
                        else
                        {
                            if (m.Value != null)
                            {
                                UpdateMasterQuerybuilder.Append(m.Key).Append("=").Append("'" + m.Value + "'").Append(",");
                            }
                            else
                            {
                                UpdateMasterQuerybuilder.Append(m.Key).Append("=").Append(" ").Append(",");
                            }

                        }

                    }

                    dynamic childcolumnvalues = JsonConvert.DeserializeObject(model.Child_COLUMN_VALUE);
                    var userial_no = 1;
                    foreach (var item in childcolumnvalues)
                    {

                        var itemarray = JsonConvert.DeserializeObject(item.ToString());
                        StringBuilder UpdateChildQuerybuilder = new StringBuilder();
                        StringBuilder UpdateStaticQuerybuilder = new StringBuilder();
                        foreach (var data in itemarray)
                        {
                            if (data.Name.ToString().Contains("PRICE") == true)
                            {
                                UpdateChildQuerybuilder.Append(data.Name).Append("=").Append(data.Value).Append(",");
                            }
                            if (data.Name.ToString().Contains("QUANTITY") == true)
                            {
                                UpdateChildQuerybuilder.Append(data.Name).Append("=").Append(data.Value).Append(",");
                            }
                            if (data.Name.ToString().Contains("DATE") == true)
                            {
                                UpdateChildQuerybuilder.Append(data.Name).Append("=").Append("TO_DATE(" + "'" + data.Value + "'" + ",'DD-MON-YYYY hh24:mi:ss')").Append(",");
                            }
                            if (data.Name.ToString().Contains("CODE") == true || data.Name.ToString().Contains("NO") == true || data.Name.ToString().Contains("FLAG") == true || data.Name.ToString().Contains("FLAG") == true)
                            {
                                if (data.Value != null)
                                {
                                    UpdateChildQuerybuilder.Append(data.Name).Append("=").Append("'" + data.Value + "'").Append(",");
                                }
                                else
                                {
                                    UpdateChildQuerybuilder.Append(data.Name).Append("=").Append(" ").Append(",");
                                }

                            }
                        }


                        var getPrevDateQuery = $@"SELECT CREATED_BY, CREATED_DATE FROM MASTER_TRANSACTION WHERE VOUCHER_NO= '{model.Order_No}'";
                        var predateData = this._objectEntity.SqlQuery<SalesOrderDetail>(getPrevDateQuery).ToList();
                        string createdDateForEdit = "", createdByForEdit = "";

                        foreach (var def in predateData)
                        {

                            createdDateForEdit = "TO_DATE('" + def.CREATED_DATE.ToString() + "', 'MM-DD-YYYY hh12:mi:ss pm')";
                            createdByForEdit = def.CREATED_BY.ToString().ToUpper();

                        }
                        UpdateStaticQuerybuilder.Append("CREATED_DATE=").Append(createdDateForEdit).Append(",").Append("CREATED_BY=").Append("'" + createdByForEdit + "'").Append(",").Append("MODIFY_DATE=").Append("SYSDATE").Append(",").Append("MODIFY_BY=").Append("'" + _workContext.CurrentUserinformation.User_id + "'");
                        StringBuilder UpdateFullQuerybuilder = new StringBuilder();
                        UpdateFullQuerybuilder.Append(UpdateMasterQuerybuilder).Append(UpdateChildQuerybuilder).Append(UpdateStaticQuerybuilder);
                        try
                        {
                            string updatemaintablequery = $@"UPDATE {model.Table_Name} SET {UpdateFullQuerybuilder} WHERE {primarycolumnname} = '{model.Order_No}' AND SERIAL_NO={userial_no}";
                            var rowCnt = _objectEntity.ExecuteSqlCommand(updatemaintablequery);
                            userial_no++;
                            upmaintable = true;
                        }
                        catch (Exception ex)
                        {
                            upmaintable = false;
                            throw ex;
                        }

                    }
                    //UPDATE MAIN TABLE END

                    //UPDATE MASTER TABLE START
                    var getPrevDataQuery = $@"SELECT VOUCHER_NO,SESSION_ROWID, CREATED_BY, CREATED_DATE,PRINT_COUNT FROM MASTER_TRANSACTION WHERE VOUCHER_NO= '{model.Order_No}'";
                    var defaultData = this._objectEntity.SqlQuery<SalesOrderDetail>(getPrevDataQuery).ToList();

                    int? printcountedit = 1;
                    foreach (var def in defaultData)
                    {

                        printcountedit = def.PRINT_COUNT.HasValue ? def.PRINT_COUNT.Value + 1 : 0;
                    }
                    try
                    {
                        string query = $@"UPDATE MASTER_TRANSACTION SET VOUCHER_AMOUNT='{ model.Grand_Total}',MODIFY_BY = '{this._workContext.CurrentUserinformation.login_code}',MODIFY_DATE = SYSDATE,PRINT_COUNT={printcountedit} where VOUCHER_NO='{model.Order_No}' and COMPANY_CODE='{this._workContext.CurrentUserinformation.company_code}'";
                        var rowCount = _objectEntity.ExecuteSqlCommand(query);
                        uptranstable = true;


                    }
                    catch (Exception ex)
                    {
                        uptranstable = false;
                        throw ex;
                    }
                    //UPDATE MASTER TABLE END

                    //UPDATE CUSTOM TABLE START
                    Newtonsoft.Json.Linq.JObject customcolumn = new Newtonsoft.Json.Linq.JObject();
                    if (model.Custom_COLUMN_VALUE != null)
                    {
                        customcolumn = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(model.Custom_COLUMN_VALUE);
                    }
                    if (customcolumn.Count > 0)
                    {
                        foreach (var r in customcolumn)
                        {
                            try
                            {
                                string UpdateCSTMquery = $@"UPDATE CUSTOM_TRANSACTION SET FIELD_VALUE='{r.Value.ToString()}',MODIFY_BY = '{this._workContext.CurrentUserinformation.login_code}',MODIFY_DATE = SYSDATE WHERE VOUCHER_NO='{model.Order_No}' AND FIELD_NAME='{r.Key.ToString()}'";
                                this._objectEntity.ExecuteSqlCommand(UpdateCSTMquery);
                                upcstmtable = true;
                            }
                            catch (Exception ex)
                            {
                                upcstmtable = false;
                                throw ex;
                            }


                        }
                    }
                    else
                    { upcstmtable = true; }
                    //UPDATE CUSTOM TABLE END

                    //UPDATE GROUP CHARGE TRANSACTION TABLE START
                    dynamic charges = JsonConvert.DeserializeObject(model.CHARGES);
                    if (model.CHARGES != "[]")
                    {
                        string chargecode = "";
                        var amount = 0.00;

                        foreach (var citem in charges)

                        {
                            var citemrow = JsonConvert.DeserializeObject(citem.ToString());
                            foreach (var data in citemrow)
                            {
                                if (data.Name == "CHARGE_CODE")
                                {
                                    chargecode = data.Value;
                                }
                                if (data.Name == "CHARGE_AMOUNT")
                                {
                                    amount = data.Value;
                                }

                            }
                            try
                            {
                                string chargeupdatequery = $@"UPDATE CHARGE_TRANSACTION SET CHARGE_AMOUNT={amount} WHERE REFERENCE_NO='{model.Order_No}' AND CHARGE_CODE='{chargecode}'";
                                this._objectEntity.ExecuteSqlCommand(chargeupdatequery);
                                upchargetable = true;
                            }
                            catch (Exception ex)
                            {
                                upchargetable = false;
                                throw ex;
                            }


                        }
                    }
                    else { upchargetable = true; }
                    //UPDATE GROUP CHARGE TRANSACTION TABLE END

                    //UPDATE INDIVIDUAL CHARGE TRANSACTION TABLE START
                    if (model.INV_ITEM_CHARGE_VALUE != null)
                    {
                        dynamic invItemChargeValue = JsonConvert.DeserializeObject(model.INV_ITEM_CHARGE_VALUE);
                        foreach (var item in invItemChargeValue)
                        {
                            var invcdatas = JsonConvert.DeserializeObject(item.ToString());
                            var Inv_charge_item = "";
                            foreach (var invitemdata in invcdatas)
                            {
                                if (invitemdata.Name.ToString() == "ITEM_CODE")
                                {
                                    Inv_charge_item = invitemdata.Value.ToString();
                                }
                                if (Inv_charge_item != "0")
                                {
                                    if (invitemdata.Name.ToString() == "INV_ITEM_CHARGE_AMOUNT_WISE" || invitemdata.Name.ToString() == "INV_ITEM_CHARGE_AMOUNT_WISE")
                                    {
                                        dynamic invitemdatas = JsonConvert.DeserializeObject(invitemdata.Value.ToString());
                                        int invserialno = 1;
                                        if (invitemdatas.ToString() != "[]")
                                        {
                                            foreach (var invdatas in invitemdatas)
                                            {
                                                var invdatass = JsonConvert.DeserializeObject(invdatas.ToString());

                                                string CHARGE_CODE = "", CHARGE_AMOUNT = "";
                                                var CALC = 0;
                                                foreach (var invdata in invdatass)
                                                {
                                                    var invdataname = invdata.Name.ToString();
                                                    var invdatavalue = invdata.Value.ToString();
                                                    if (invdataname == "CHARGE_CODE")
                                                    {
                                                        CHARGE_CODE = invdatavalue;
                                                    }

                                                    if (invdataname == "CALC")
                                                    {

                                                        if (invdatavalue != null)
                                                        {
                                                            CHARGE_AMOUNT = invdatavalue;
                                                        }
                                                        else
                                                        {
                                                            CHARGE_AMOUNT = "0";
                                                        }

                                                    }
                                                    if (invdataname == "VALUE_PERCENT_AMOUNT")
                                                    {
                                                        if (invdatavalue != null)
                                                        {
                                                            CHARGE_AMOUNT = invdatavalue;
                                                        }
                                                        else
                                                        {
                                                            CHARGE_AMOUNT = "0";
                                                        }

                                                    }

                                                }
                                                if (CHARGE_CODE != "")
                                                {
                                                    try
                                                    {
                                                        string chargeupdatequery = $@"UPDATE CHARGE_TRANSACTION SET CHARGE_AMOUNT={CHARGE_AMOUNT} WHERE                     REFERENCE_NO='{model.Order_No}' AND CHARGE_CODE='{CHARGE_CODE}' AND ITEM_CODE='{Inv_charge_item}'";
                                                        this._objectEntity.ExecuteSqlCommand(chargeupdatequery);
                                                        upinvchargetable = true;
                                                    }
                                                    catch (Exception)
                                                    {
                                                        upinvchargetable = false;
                                                        throw;
                                                    }


                                                }
                                                else
                                                { upinvchargetable = true; }
                                            }
                                        }
                                    }
                                }
                                
                            }
                        }
                    }
                    else
                    { upinvchargetable = true; }
                    //UPDATE INDIVIDUAL CHARGE TRANSACTION TABLE END

                    //UPDATE SHIPPING TRANSACTION TABLE START
                    dynamic shippingdetailsvalues = JsonConvert.DeserializeObject(model.SHIPPING_DETAILS_VALUE);
                    if (model.SHIPPING_DETAILS_VALUE != "[]")
                    {
                        string VEHICLE_CODE = "", VEHICLE_OWNER_NAME = "", VEHICLE_OWNER_NO = "", DRIVER_NAME = "", DRIVER_LICENSE_NO = "", DRIVER_MOBILE_NO = "", TRANSPORTER_CODE = "", START_FORM = "", DESTINATION = "", CN_NO = "", TRANSPORT_INVOICE_NO = "", WB_NO = "", VEHICLE_NO = "", GATE_ENTRY_NO = "";

                        decimal FREGHT_AMOUNT = 0, WB_WEIGHT = 0, FREIGHT_RATE = 0, LOADING_SLIP_NO = 0;

                        string TRANSPORT_INVOICE_DATE = "", DELIVERY_INVOICE_DATE = "", GATE_ENTRY_DATE = "", WB_DATE = "";

                        foreach (var sdval in shippingdetailsvalues)
                        {
                            var sddataname = sdval.Name.ToString();
                            var sdinvdatavalue = sdval.Value.ToString();
                            if (sddataname == "VEHICLE_CODE")
                            {
                                VEHICLE_CODE = sdinvdatavalue;

                            }
                            if (sddataname == "OWNER_NAME")
                            {
                                VEHICLE_OWNER_NAME = sdinvdatavalue;
                            }
                            if (sddataname == "OWNER_MOBILE_NO")
                            {
                                VEHICLE_OWNER_NO = sdinvdatavalue;
                            }
                            if (sddataname == "DRIVER_NAME")
                            {
                                DRIVER_NAME = sdinvdatavalue;
                            }
                            if (sddataname == "DRIVER_LICENCE_NO")
                            {
                                DRIVER_LICENSE_NO = sdinvdatavalue;
                            }
                            if (sddataname == "DRIVER_MOBILE_NO")
                            {
                                DRIVER_MOBILE_NO = sdinvdatavalue;
                            }
                            if (sddataname == "TRANSPORTER_CODE")
                            {
                                TRANSPORTER_CODE = sdinvdatavalue;
                            }
                            if (sddataname == "START_FORM")
                            {
                                START_FORM = sdinvdatavalue;
                            }
                            if (sddataname == "DESTINATION")
                            {
                                DESTINATION = sdinvdatavalue;
                            }
                            if (sddataname == "CN_NO")
                            {
                                CN_NO = sdinvdatavalue;
                            }
                            if (sddataname == "TRANSPORT_INVOICE_NO")
                            {
                                TRANSPORT_INVOICE_NO = sdinvdatavalue;
                            }
                            if (sddataname == "WB_NO")
                            {
                                WB_NO = sdinvdatavalue;
                            }
                            if (sddataname == "VEHICLE_NO")
                            {
                                VEHICLE_NO = sdinvdatavalue;
                            }
                            if (sddataname == "GATE_ENTRY_NO")
                            {
                                GATE_ENTRY_NO = sdinvdatavalue;
                            }
                            if (sddataname == "FREGHT_AMOUNT")
                            {
                                if (sdinvdatavalue != "")
                                {
                                    FREGHT_AMOUNT = Convert.ToDecimal(sdinvdatavalue);
                                }
                            }
                            if (sddataname == "WB_WEIGHT")
                            {
                                if (sdinvdatavalue != "")
                                {
                                    WB_WEIGHT = Convert.ToDecimal(sdinvdatavalue);
                                }
                            }
                            if (sddataname == "FREIGHT_RATE")
                            {
                                if (sdinvdatavalue != "")
                                {
                                    FREIGHT_RATE = Convert.ToDecimal(sdinvdatavalue);
                                }
                            }
                            if (sddataname == "LOADING_SLIP_NO")
                            {
                                if (sdinvdatavalue != "")
                                {
                                    LOADING_SLIP_NO = Convert.ToDecimal(sdinvdatavalue);
                                }

                            }
                            if (sddataname == "TRANSPORT_INVOICE_DATE")
                            {
                                if (sdinvdatavalue != "")
                                {
                                    TRANSPORT_INVOICE_DATE = "TO_DATE('" + sdinvdatavalue + "','MM-DD-YYYY HH:MI:SS AM')";
                                }
                                else
                                { TRANSPORT_INVOICE_DATE = "null"; }
                            }
                            if (sddataname == "DELIVERY_INVOICE_DATE")
                            {
                                if (sdinvdatavalue != "")
                                {
                                    DELIVERY_INVOICE_DATE = "TO_DATE('" + sdinvdatavalue + "','MM-DD-YYYY HH:MI:SS AM')";
                                }
                                else
                                { DELIVERY_INVOICE_DATE = "null"; }
                            }
                            if (sddataname == "GATE_ENTRY_DATE")
                            {
                                if (sdinvdatavalue != "")
                                {
                                    GATE_ENTRY_DATE = "TO_DATE('" + sdinvdatavalue + "','MM-DD-YYYY HH:MI:SS AM')";
                                }
                                else
                                { GATE_ENTRY_DATE = "null"; }
                            }
                            if (sddataname == "WB_DATE")
                            {
                                if (sdinvdatavalue != "")
                                {
                                    WB_DATE = "TO_DATE('" + sdinvdatavalue + "','MM-DD-YYYY HH:MI:SS AM')";
                                }
                                else
                                { WB_DATE = "null"; }
                            }
                        }
                        if (VEHICLE_CODE != "")
                        {
                            try
                            {
                                string updateSDQuery = $@"UPDATE SHIPPING_TRANSACTION SET VEHICLE_CODE='{VEHICLE_CODE}',VEHICLE_OWNER_NAME='{VEHICLE_OWNER_NAME}',VEHICLE_OWNER_NO='{VEHICLE_OWNER_NO}',DRIVER_NAME='{DRIVER_NAME}',DRIVER_LICENSE_NO='{DRIVER_LICENSE_NO}',DRIVER_MOBILE_NO='{DRIVER_MOBILE_NO}',TRANSPORTER_CODE='{TRANSPORTER_CODE}',FREGHT_AMOUNT={FREGHT_AMOUNT},START_FORM='{START_FORM}',DESTINATION='{DESTINATION}',TRANSPORT_INVOICE_NO='{TRANSPORT_INVOICE_NO}',CN_NO='{CN_NO}',TRANSPORT_INVOICE_DATE={TRANSPORT_INVOICE_DATE},DELIVERY_INVOICE_DATE={DELIVERY_INVOICE_DATE},WB_WEIGHT={WB_WEIGHT},WB_NO='{WB_NO}',WB_DATE={WB_DATE},FREIGHT_RATE={FREIGHT_RATE},VEHICLE_NO='{VEHICLE_NO}',LOADING_SLIP_NO={LOADING_SLIP_NO},GATE_ENTRY_NO='{GATE_ENTRY_NO}',GATE_ENTRY_DATE={GATE_ENTRY_DATE}, MODIFY_DATE=SYSDATE,MODIFY_BY='{this._workContext.CurrentUserinformation.login_code}' WHERE VOUCHER_NO='{model.Order_No}'";
                                this._objectEntity.ExecuteSqlCommand(updateSDQuery);
                                upspdtable = true;
                            }
                            catch (Exception ex)
                            {
                                upspdtable = false;
                                throw ex;
                            }

                        }
                        else { upspdtable = true; }
                    }
                    else { upspdtable = true; }
                    //UPDATE SHIPPING TRANSACTION TABLE END
                    //bool upmaintable = false, uptranstable = false, upcstmtable = false, upchargetable = false, upinvchargetable = false, upspdtable = false;
                    //if (upmaintable == true && uptranstable == true && upcstmtable == true && upchargetable == true && upinvchargetable == true && upspdtable == true)
                    //{
                    //    uptrans.Commit();
                    //    return "UPDATE_TRIGG_TABLES_SUCCESS";

                    //}
                    if (upmaintable == true && uptranstable == true && upcstmtable == true && upchargetable == true && upinvchargetable == true && upspdtable == true)
                    {
                        //uptrans.Commit();
                        return "UPDATE_TRIGG_TABLES_SUCCESS";

                    }
                    else
                    {
                        //uptrans.Rollback();
                        return "UPDATE_TRIGG_TABLES_FAIL";
                    }
                }
                catch (Exception ex)
                {
                    //uptrans.Rollback();
                    throw ex;
                }
            //}

        }

        //public ResponseMessage UpdateTrigerTableNew(FormDetails model, string primaryDateColumn, string primaryColumn, List<CustomOrderColumn> customColList, List<ChargeOnSales> chargeOnSales,ShippingDetails shippingDetailValues, SalesOrderDetail masterColumn,CommonFieldForSales commonUpdateFieldForSales, NeoErpCoreEntity dbcontext = null)
        //{
        //    var orderUpdateResponse = new ResponseMessage();
        //    if (model.Table_Name.ToLower() == "sa_sales_invoice")
        //    {
        //        var masterInvoiceColumn = _saveDocTemplate.MapSalesInvoiceMasterColumnWithValue(model.Master_COLUMN_VALUE, primaryDateColumn, primaryColumn);
        //        _logErp.InfoInFile("Master column value :" + masterColumn);
        //        var childColVal = _saveDocTemplate.MapInvoiceChildColumnWithValue(model.Child_COLUMN_VALUE);
        //        _logErp.InfoInFile("Child Column Values : " + childColVal);

        //        var salesInvoiceData = new SalesInvoiceDetailModel()
        //        {
        //            MasterInvoiceTransaction = masterInvoiceColumn,
        //            ChildInvoiceTransaction = childColVal,
        //            ChargeTransaction = chargeOnSales,
        //            ShippingTransaction = shippingDetailValues,
        //            CustomOrderTransaction = customColList
        //        };
        //        orderUpdateResponse = _saveDocTemplateSalesModule.UpdateSalesInvoiceFormData(salesInvoiceData, commonUpdateFieldForSales);
        //        _logErp.InfoInFile("Sales invoice updated successfully : " + childColVal);
        //    }
        //    if (model.Table_Name.ToLower() == "sa_sales_return")
        //    {
        //        var masterReturnColumn = _saveDocTemplate.MapSalesReturnMasterColumnWithValue(model.Master_COLUMN_VALUE, primaryDateColumn, primaryColumn);
        //        _logErp.InfoInFile("Master column value :" + masterColumn);
        //        var childColVal = _saveDocTemplate.MapReturnChildColumnWithValue(model.Child_COLUMN_VALUE);
        //        _logErp.InfoInFile("Child Column Values : " + childColVal);

        //        var salesReturnData = new SalesReturnDetailModel()
        //        {
        //            MasterReturnTransaction = masterReturnColumn,
        //            ChildReturnTransaction = childColVal,
        //            ChargeTransaction = chargeOnSales,
        //            ShippingTransaction = shippingDetailValues
        //        };
        //        orderUpdateResponse = _saveDocTemplateSalesModule.UpdateSalesReturnFormData(salesReturnData, commonUpdateFieldForSales);
        //        _logErp.InfoInFile("Sales invoice updated successfully : " + childColVal);
        //    }

        //        return orderUpdateResponse;
        //}


        #region Validation
        public string checkValidation(FormDetails model, string voucherDate)
        {
            var resultVal = "Valid";
            voucherDate = DateTime.Now.ToString();
            if (Convert.ToDateTime(voucherDate) > DateTime.Now)
            {
                return resultVal = "Voucher Date exceed today date.";
            }
            #region Back Date Voucher Validation
            var backDateFlagQuery = $@"SELECT BACK_DATE_VNO_SAVE_FLAG,ACCESS_BDFSM_FLAG,TO_CHAR(FREEZE_BACK_DAYS) FROM FORM_SETUP WHERE FORM_CODE = '{model.Form_Code}'";
            var backDateFlag = _dbContext.SqlQuery<VALIDATION_FLAG_MODEL>(backDateFlagQuery).FirstOrDefault();
            var bdQ = $@"SELECT TO_CHAR(BACK_DAYS) FROM SC_FORM_DAYS_CONTROL";
            var bd = _dbContext.SqlQuery<VALIDATION_FLAG_MODEL>(bdQ).FirstOrDefault();
            if (backDateFlag != null)
            {
                if (backDateFlag.BACK_DATE_VNO_SAVE_FLAG == "Y")
                {
                    var backDateQuery = $@"SELECT MAX({_FormTemplateRepo.GetPrimaryDateByTableName(model.Table_Name)})VOUCHER_DATE FROM {model.Table_Name} WHERE FORM_CODE = '{model.Form_Code}'";
                    var backDate = _dbContext.SqlQuery<VALIDATION_FLAG_MODEL>(backDateFlagQuery).FirstOrDefault();
                    if (Convert.ToDateTime(voucherDate) > Convert.ToDateTime(backDate.VOUCHER_DATE))
                    {
                        resultVal = "Voucher Date is not in max date and today date.";
                    }
                }
                if (backDateFlag.FREEZE_BACK_DAYS == "Y")
                {
                    if (Convert.ToDateTime(voucherDate) < Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
                    {
                        resultVal = "Previous Days Voucher Date Not Valid";
                    }
                }
                if (backDateFlag.ACCESS_BDFSM_FLAG == "Y")
                {
                    if (Convert.ToDateTime(voucherDate).Month != DateTime.Now.Month)
                    {
                        resultVal = "Voucher Date should have current month.";
                    }
                }
            }
            if (bd != null)
            {
                if (bd.BACK_DAYS != "" && bd.BACK_DAYS != null)
                {
                    if (Convert.ToDateTime(voucherDate) < DateTime.Now.AddDays(-Convert.ToInt32(backDateFlag.BACK_DAYS)))
                    {
                        resultVal = "Voucher Date exceed Back days.";
                    }
                }
            }
            #endregion
            return resultVal;
        }
        #endregion
        [HttpGet]
        public List<FormCustomSetup> GetFormCustomSetup(string formCode, string voucherNo)
        {
            _logErp.InfoInFile("GetFormCustom Setup for formcode: " + formCode + " and voucher no : " + voucherNo + " started====");
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<FormCustomSetup>();
            if (this._cacheManager.IsSet($"GetFormCustomSetup_{userid}_{company_code}_{branch_code}_{formCode}_{voucherNo}"))
            {
                var data = _cacheManager.Get<List<FormCustomSetup>>($"GetFormCustomSetup_{userid}_{company_code}_{branch_code}_{formCode}_{voucherNo}");
                _logErp.InfoInFile(data.Count() + " custom setup has been fetched for " + formCode + " formcode and " + voucherNo + " voucher number from cached");
                response = data;
            }
            else
            {
                var FormCustomSetup = this._FormTemplateRepo.GetFormCustomSetup(formCode, voucherNo);
                _logErp.InfoInFile(FormCustomSetup.Count() + " custom setup has been fetched for " + formCode + " formcode and " + voucherNo + " voucher number");
                this._cacheManager.Set($"GetFormCustomSetup_{userid}_{company_code}_{branch_code}_{formCode}_{voucherNo}", FormCustomSetup, 20);
                response = FormCustomSetup;
            }
            return response;
        }
        [HttpGet]
        public List<Products> GetMUCodeByProductId(string productId)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<Products>();
            //if (this._cacheManager.IsSet($"GetMUCodeByProductId_{userid}_{company_code}_{branch_code}_{productId}"))
            //{
            //    var data = _cacheManager.Get<List<Products>>($"GetMUCodeByProductId_{userid}_{company_code}_{branch_code}_{productId}");
            //    response = data;
            //}
            //else
            //{
            //    var MUCodeByProductId = this._FormTemplateRepo.GetProductDataByProductCode(productId);
            //    this._cacheManager.Set($"GetMUCodeByProductId_{userid}_{company_code}_{branch_code}_{productId}", MUCodeByProductId, 20);
            //    response = MUCodeByProductId;
            //}
            var MUCodeByProductId = this._FormTemplateRepo.GetProductDataByProductCode(productId);
            response = MUCodeByProductId;
            return response;
        }
        [HttpGet]
        public List<MuCodeModel> GetMuCode()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<MuCodeModel>();
            if (this._cacheManager.IsSet($"GetMuCode_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<MuCodeModel>>($"GetMuCode_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var MuCode = this._FormTemplateRepo.GetMuCode();
                this._cacheManager.Set($"GetMuCode_{userid}_{company_code}_{branch_code}", MuCode, 20);
                response = MuCode;
            }
            return response;
        }
        [HttpGet]
        public List<CustomerModels> customerDropDownForGroupPopup()
         {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<CustomerModels>();
            if (this._cacheManager.IsSet($"customerDropDownForGroupPopup_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<CustomerModels>>($"customerDropDownForGroupPopup_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var customerDropDownForGroupPopup = _FormTemplateRepo.getAllCustomer();
                this._cacheManager.Set($"customerDropDownForGroupPopup_{userid}_{company_code}_{branch_code}", customerDropDownForGroupPopup, 20);
                response = customerDropDownForGroupPopup;
            }
            return response;
        }
        [HttpGet]
        public List<SalesOrderDetailView> GetAllSalesOrderDetails()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<SalesOrderDetailView>();
            if (this._cacheManager.IsSet($"GetAllSalesOrderDetails_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<SalesOrderDetailView>>($"GetAllSalesOrderDetails_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {


                var AllSalesOrderDetails = this._SalesOrderRepo.GetSalesOrderDetails();
                this._cacheManager.Set($"GetAllSalesOrderDetails_{userid}_{company_code}_{branch_code}", AllSalesOrderDetails, 20);
                response = AllSalesOrderDetails;
            }
            return response;
            //return this._SalesOrderRepo.GetSalesOrderDetails();
        }
        //AA
        [HttpGet]
        public List<COMMON_COLUMN> GetSalesOrderDetailFormDetailByFormCodeAndOrderNo(string formCode, string orderno)
         {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<COMMON_COLUMN>();
            //if (this._cacheManager.IsSet($"CALC_TOTAL_PRICE CALC_TOTAL_PRICE-child{userid}_{company_code}_{branch_code}_{formCode}_{orderno}"))
            //{
            //    var data = _cacheManager.Get<List<COMMON_COLUMN>>($"GetSalesOrderDetailFormDetailByFormCodeAndOrderNo_{userid}_{company_code}_{branch_code}_{formCode}_{orderno}");
            //    response = data;
            //}
            //else
            //{
            //    var SalesOrderDetailFormDetailByFormCodeAndOrderNo = this._FormTemplateRepo.GetSalesOrderFormDetail(formCode, orderno);
            //    this._cacheManager.Set($"GetSalesOrderDetailFormDetailByFormCodeAndOrderNo_{userid}_{company_code}_{branch_code}_{formCode}_{orderno}", SalesOrderDetailFormDetailByFormCodeAndOrderNo, 20);
            //    response = SalesOrderDetailFormDetailByFormCodeAndOrderNo;
            //}
            var SalesOrderDetailFormDetailByFormCodeAndOrderNo = this._FormTemplateRepo.GetSalesOrderFormDetail(formCode, orderno);
            response = SalesOrderDetailFormDetailByFormCodeAndOrderNo;
            return response;
        }
        [HttpGet]
        public List<DraftFormModel> getDraftDataByFormCodeAndTempCode(string formCode, string TempCode)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<DraftFormModel>();
            if (this._cacheManager.IsSet($"getDraftDataByFormCodeAndTempCode_{userid}_{company_code}_{branch_code}_{formCode}_{TempCode}"))
            {
                var data = _cacheManager.Get<List<DraftFormModel>>($"getDraftDataByFormCodeAndTempCode_{userid}_{company_code}_{branch_code}_{formCode}_{TempCode}");
                response = data;
            }
            else
            {
                var DraftDataByFormCodeAndTempCode = this._FormTemplateRepo.getDraftDataByFormCodeAndTempCode(formCode, TempCode);
                this._cacheManager.Set($"getDraftDataByFormCodeAndTempCode_{userid}_{company_code}_{branch_code}_{formCode}_{TempCode}", DraftDataByFormCodeAndTempCode, 20);
                response = DraftDataByFormCodeAndTempCode;
            }
            return response;
        }
        [HttpGet]
        public decimal GetGrandTotalByVoucherNo(string voucherno, string formcode)
        {
            return this._FormTemplateRepo.GetGrandTotalByVoucherNo(voucherno, formcode);
        }
        [HttpGet]
        public decimal GetREFGrandTotalByVoucherNo(string voucherno)
        {
            return this._FormTemplateRepo.GetRefGrandTotalByVoucherNo(voucherno);
        }
        [HttpGet]
        public string getBudgetCodeCountCodeByAccCode(string accCode)
        {
            var result = this._FormTemplateRepo.getBudgetCodeCountByAccCode(accCode);
            return result;
        }
        [HttpGet]
        public List<DocumentType> GetAllDocumentTypeListByFilter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<DocumentType>();
            if (this._cacheManager.IsSet($"GetAllDocumentTypeListByFilter_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<DocumentType>>($"getDraftDataByFormCodeAndTempCode_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var AllDocumentTypeListByFilter = this._FormTemplateRepo.getDocumentTypeListByFlter(filter);
                this._cacheManager.Set($"getDraftDataByFormCodeAndTempCode_{userid}_{company_code}_{branch_code}_{filter}", AllDocumentTypeListByFilter, 20);
                response = AllDocumentTypeListByFilter;
            }
            return response;
        }
        [HttpGet]
        public List<FormDetailSetup> GetSubMenuList(string moduleCode)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<FormDetailSetup>();
            //if (this._cacheManager.IsSet($"GetSubMenuList_{userid}_{company_code}_{branch_code}_{moduleCode}"))
            //{
            //    var data = _cacheManager.Get<List<FormDetailSetup>>($"GetSubMenuList_{userid}_{company_code}_{branch_code}_{moduleCode}");
            //    response = data;
            //}
            //else
            //{
            //    var SubMenuList = this._FormTemplateRepo.GetDistFormTransDetailByModuleCode(moduleCode);
            //    this._cacheManager.Set($"GetSubMenuList_{userid}_{company_code}_{branch_code}_{moduleCode}", SubMenuList, 20);
            //    response = SubMenuList;
            //}
            var SubMenuList = this._FormTemplateRepo.GetDistFormTransDetailByModuleCode(moduleCode);
            response = SubMenuList;
            return response;
        }
        [HttpGet]
        public List<TemplateDraftModel> GetDraftList(string moduleCode, string formCode)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<TemplateDraftModel>();
            if (this._cacheManager.IsSet($"GetDraftList_{userid}_{company_code}_{branch_code}_{moduleCode}_{formCode}"))
            {
                var data = _cacheManager.Get<List<TemplateDraftModel>>($"GetDraftList_{userid}_{company_code}_{branch_code}_{moduleCode}_{formCode}");
                response = data;
            }
            else
            {
                var DraftList = this._FormTemplateRepo.GetDraftList(moduleCode, formCode);
                this._cacheManager.Set($"GetDraftList_{userid}_{company_code}_{branch_code}_{moduleCode}_{formCode}", DraftList, 20);
                response = DraftList;
            }
            return response;
        }
        [HttpGet]
        public List<DocumentSubMenu> GetSubMenuDetailList(string formCode, string docVer="All")
        {
            //var userid = _workContext.CurrentUserinformation.User_id;
            //var company_code = _workContext.CurrentUserinformation.company_code;
            //var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<DocumentSubMenu>();
            //if (this._cacheManager.IsSet($"GetSubMenuDetailList_{userid}_{company_code}_{branch_code}_{formCode}"))
            //{
            //    var data = _cacheManager.Get<List<DocumentSubMenu>>($"GetSubMenuDetailList_{userid}_{company_code}_{branch_code}_{formCode}");
            //    response = data;
            //}
            //else
            //{
            var SubMenuDetailList = this._FormTemplateRepo.GetMaseterTransDetailByFormCode(formCode, docVer);
            //this._cacheManager.Set($"GetSubMenuDetailList_{userid}_{company_code}_{branch_code}_{formCode}", SubMenuDetailList, 20);
            response = SubMenuDetailList;
            //}
            return response;
        }
        [HttpGet]
        public List<DocumentSubMenu> GetSubMenuDetailListVer(string formCode, string docVer)
        {
            var response = new List<DocumentSubMenu>();
            var SubMenuDetailList = this._FormTemplateRepo.GetMaseterTransDetailByFormCodeVer(formCode, docVer);
            response = SubMenuDetailList;
            return response;
        }
        [HttpPost]
        public HttpResponseMessage UpdateMasterTranasactionForVerification(string VoucherNo, string formcode, string mode)
        {
            try
            {
                string message = string.Empty;
                bool status = false;
                _FormTemplateRepo.UpdateMasterTranasactionForVerification(VoucherNo, formcode, mode, out message, out status);
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = message, STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpPost]
        public HttpResponseMessage BulkUpdateMasterTranasactionForVerification(List<string> voucherNo, string formcode, string mode)
        {
            string message = string.Empty;
            bool status = false;
            _FormTemplateRepo.BulkUpdateMasterTranasactionForVerification(voucherNo, formcode, mode, out message, out status);
            return Request.CreateResponse(HttpStatusCode.OK, new { message, status });
        }
        [HttpGet]
        public List<string> getSalesVerificationUserWise()
        {
            var result = _FormTemplateRepo.getSalesVerificationUserWise();
            return result;
        }
        [HttpGet]
        public List<FormDetailSetup> getSalesVerificationFormcodeWise(string moduleCode, string docVer)
        {
            var result = _FormTemplateRepo.getSalesVerificationFormcodeWise(moduleCode, docVer);
            return result;
        }
        [HttpGet]
        public List<DocumentSubMenu> GetDraftDetailList(string formCode)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<DocumentSubMenu>();
            if (this._cacheManager.IsSet($"GetDraftDetailList_{userid}_{company_code}_{branch_code}_{formCode}"))
            {
                var data = _cacheManager.Get<List<DocumentSubMenu>>($"GetDraftDetailList_{userid}_{company_code}_{branch_code}_{formCode}");
                response = data;
            }
            else
            {
                var DraftDetailList = this._FormTemplateRepo.GetDraftDetails(formCode);
                this._cacheManager.Set($"GetDraftDetailList_{userid}_{company_code}_{branch_code}_{formCode}", DraftDetailList, 20);
                response = DraftDetailList;
            }
            return response;
        }
        public List<LocationModels> GetLocationByGroup()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<LocationModels>();
            if (this._cacheManager.IsSet($"GetLocationByGroup_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<LocationModels>>($"GetLocationByGroup_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var AllLocation = _FormTemplateRepo.getAllLocation();
                this._cacheManager.Set($"GetLocationByGroup_{userid}_{company_code}_{branch_code}", AllLocation, 20);
                response = AllLocation;
            }
            return response;
        }
        public List<PartyType> GetPartyType()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<PartyType>();
            if (this._cacheManager.IsSet($"GetPartyType_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<PartyType>>($"GetPartyType_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var PartyType = _FormTemplateRepo.GetAllPartyType();
                this._cacheManager.Set($"GetPartyType_{userid}_{company_code}_{branch_code}", PartyType, 20);
                response = PartyType;
            }
            return response;
        }
        public List<PartyRating> GetPartyRating()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<PartyRating>();
            if (this._cacheManager.IsSet($"GetPartyRating_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<PartyRating>>($"GetPartyRating_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var PartyRating = _FormTemplateRepo.GetAllPartyRating();
                this._cacheManager.Set($"GetPartyRating_{userid}_{company_code}_{branch_code}", PartyRating, 20);
                response = PartyRating;
            }
            return response;
        }
        public List<LocationTypeModels> getLocationType()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<LocationTypeModels>();
            if (this._cacheManager.IsSet($"getLocationType_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<LocationTypeModels>>($"getLocationType_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var LocationType = _FormTemplateRepo.getAllLocationType();
                this._cacheManager.Set($"getLocationType_{userid}_{company_code}_{branch_code}", LocationType, 20);
                response = LocationType;
            }
            return response;
        }
        public List<AccountCodeModels> getAccountCodeWithChild()
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
                var AllAccountCode = _FormTemplateRepo.getAllAccountCode();
                this._cacheManager.Set($"getAccountCodeWithChild_{userid}_{company_code}_{branch_code}", AllAccountCode, 20);
                response = AllAccountCode;
            }
            return response;
        }

        public List<AccountCodeModels> getAllAccountCodeWithChild(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<AccountCodeModels>();
            var AllAccountCodeWithChild = _FormTemplateRepo.getAllAccountCodeWithChild(filter);
            response = AllAccountCodeWithChild;
            return response;
            //if (this._cacheManager.IsSet($"getAllAccountCodeWithChild_{userid}_{company_code}_{branch_code}_{filter}"))
            //{
            //    var data = _cacheManager.Get<List<AccountCodeModels>>($"getAllAccountCodeWithChild_{userid}_{company_code}_{branch_code}_{filter}");
            //    response = data;
            //}
            //else
            //{
            //    var AllAccountCodeWithChild = _FormTemplateRepo.getAllAccountCodeWithChild(filter);
            //    this._cacheManager.Set($"getAllAccountCodeWithChild_{userid}_{company_code}_{branch_code}_{filter}", AllAccountCodeWithChild, 20);
            //    response = AllAccountCodeWithChild;
            //}
            //return response;
        }
        public List<AccountCodeModels> getAllAccountCodeForVeh(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<AccountCodeModels>();
            var AllAccountCodeWithChild = _FormTemplateRepo.getAllAccountCodeForVs(filter);
            response = AllAccountCodeWithChild;
            return response;
            //if (this._cacheManager.IsSet($"getAllAccountCodeWithChild_{userid}_{company_code}_{branch_code}_{filter}"))
            //{
            //    var data = _cacheManager.Get<List<AccountCodeModels>>($"getAllAccountCodeWithChild_{userid}_{company_code}_{branch_code}_{filter}");
            //    response = data;
            //}
            //else
            //{
            //    var AllAccountCodeWithChild = _FormTemplateRepo.getAllAccountCodeWithChild(filter);
            //    this._cacheManager.Set($"getAllAccountCodeWithChild_{userid}_{company_code}_{branch_code}_{filter}", AllAccountCodeWithChild, 20);
            //    response = AllAccountCodeWithChild;
            //}
            //return response;
        }
        public List<AccountCodeModels> getAllAccountComboCodeWithChild(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<AccountCodeModels>();
            if (this._cacheManager.IsSet($"getAllAccountComboCodeWithChild_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<AccountCodeModels>>($"getAllAccountComboCodeWithChild_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var AllAccountComboCodeWithChild = _FormTemplateRepo.getAllAccountComboCodeWithChild(filter);
                this._cacheManager.Set($"getAllAccountComboCodeWithChild_{userid}_{company_code}_{branch_code}_{filter}", AllAccountComboCodeWithChild, 20);
                response = AllAccountComboCodeWithChild;
            }
            return response;
        }
        public List<ResourceCodeModels> getResourceCodeWithChild()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<ResourceCodeModels>();
            if (this._cacheManager.IsSet($"getResourceCodeWithChild_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<ResourceCodeModels>>($"getResourceCodeWithChild_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var ResourceCodeWithChild = _FormTemplateRepo.getAllResourceCode();
                this._cacheManager.Set($"getResourceCodeWithChild_{userid}_{company_code}_{branch_code}", ResourceCodeWithChild, 20);
                response = ResourceCodeWithChild;
            }
            return response;
        }
        //
        //Division
        //[HttpGet]
        //public HttpResponseMessage getDivisionCodeWithChild()
        //{
        //    try
        //    {
        //        var result = this._FormTemplateRepo.getAllDivisionCode();
        //        return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
        //    }

        //}
        //// Division
        public List<DivisionModels> getDivisionCodeWithChild()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<DivisionModels>();
            if (this._cacheManager.IsSet($"getDivisionCodeWithChild{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<DivisionModels>>($"getDivisionCodeWithChild{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var ResourceCodeWithChild = _FormTemplateRepo.getAllDivisionCode();
                this._cacheManager.Set($"getDivisionCodeWithChild{userid}_{company_code}_{branch_code}", ResourceCodeWithChild, 20);
                response = ResourceCodeWithChild;
            }
            return response;
        }

        public List<ProcessModels> getProcessCodeWithChild()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<ProcessModels>();
            if (this._cacheManager.IsSet($"getProcessCodeWithChild_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<ProcessModels>>($"getProcessCodeWithChild_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var ProcessCodeWithChild = _FormTemplateRepo.getAllProcess();
                this._cacheManager.Set($"getProcessCodeWithChild_{userid}_{company_code}_{branch_code}", ProcessCodeWithChild, 20);
                response = ProcessCodeWithChild;
            }
            return response;
        }
        //branch under group
        //[HttpGet]
        //public HttpResponseMessage getBranchCodeWithChild()
        //{
        //    try
        //    {
        //        var result = this._FormTemplateRepo.getAllBranch();
        //        return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
        //    }

        //}


        public List<BranchModels> getBranchCodeWithChild()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<BranchModels>();

            var branchCodeWithChild = _FormTemplateRepo.getAllBranch();
            return branchCodeWithChild;
        }
        public List<BranchModels> getBranchCodeforScheme()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<BranchModels>();

            var branchCodeWithChild = _FormTemplateRepo.getAllBranchforscheme();
            return branchCodeWithChild;
        }

        //
        //public List<BranchModels> getBranchCodeWithChild()
        //{
        //    var userid = _workContext.CurrentUserinformation.User_id;
        //    var company_code = _workContext.CurrentUserinformation.company_code;
        //    var branch_code = _workContext.CurrentUserinformation.branch_code;
        //    var response = new List<BranchModels>();
        //    if (this._cacheManager.IsSet($"getBranchCodeWithChild_{userid}_{company_code}_{branch_code}"))
        //    {
        //        var data = _cacheManager.Get<List<BranchModels>>($"getBranchCodeWithChild_{userid}_{company_code}_{branch_code}");
        //        response = data;
        //    }
        //    else
        //    {
        //        var ProcessCodeWithChild = _FormTemplateRepo.getAllBranch();
        //        this._cacheManager.Set($"getBranchCodeWithChild_{userid}_{company_code}_{branch_code}", ProcessCodeWithChild, 20);
        //        response = ProcessCodeWithChild;
        //    }
        //    return response;
        //}

        public List<AreaModels> getAreaCodeWithChild()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<AreaModels>();
            if (this._cacheManager.IsSet($"getAreaCodeWithChild_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<AreaModels>>($"getAreaCodeWithChild_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var AreaCodeWithChild = _FormTemplateRepo.getAllArea();
                this._cacheManager.Set($"getAreaCodeWithChild_{userid}_{company_code}_{branch_code}", AreaCodeWithChild, 20);
                response = AreaCodeWithChild;
            }
            return response;
        }
        public List<AgentModels> getAgentCodeWithChild()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<AgentModels>();
            if (this._cacheManager.IsSet($"getAgentCodeWithChild_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<AgentModels>>($"getAgentCodeWithChild_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var AgentCodeWithChild = _FormTemplateRepo.getAllAgent();
                this._cacheManager.Set($"getAgentCodeWithChild_{userid}_{company_code}_{branch_code}", AgentCodeWithChild, 20);
                response = AgentCodeWithChild;
            }
            return response;
        }

     
        public List<TransporterModels> getTransporterCodeWithChild()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<TransporterModels>();
            if (this._cacheManager.IsSet($"getTransporterCodeWithChild_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<TransporterModels>>($"getTransporterCodeWithChild_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var TransporterCodeWithChild = _FormTemplateRepo.getAllTransporter();
                this._cacheManager.Set($"getTransporterCodeWithChild_{userid}_{company_code}_{branch_code}", TransporterCodeWithChild, 20);
                response = TransporterCodeWithChild;
            }
            return response;
        }
        public List<BudgetCenterCodeModels> getBudgetCenterCodeWithChild()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<BudgetCenterCodeModels>();
            if (this._cacheManager.IsSet($"getBudgetCenterCodeWithChild_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<BudgetCenterCodeModels>>($"getBudgetCenterCodeWithChild_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var BudgetCenterCodeWithChild = _FormTemplateRepo.getAllBudgetCenterCode();
                this._cacheManager.Set($"getBudgetCenterCodeWithChild_{userid}_{company_code}_{branch_code}", BudgetCenterCodeWithChild, 20);
                response = BudgetCenterCodeWithChild;
            }
            return response;
        }
        public List<ProductsModels> getProductCodeWithChild()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<ProductsModels>();
            //if (this._cacheManager.IsSet($"getProductCodeWithChild_{userid}_{company_code}_{branch_code}"))
            //{
            //    var data = _cacheManager.Get<List<ProductsModels>>($"getProductCodeWithChild_{userid}_{company_code}_{branch_code}");
            //    response = data;
            //}
            //else
            //{
                var ProductCodeWithChild = _FormTemplateRepo.getAllProduct();
                this._cacheManager.Set($"getProductCodeWithChild_{userid}_{company_code}_{branch_code}", ProductCodeWithChild, 20);
                response = ProductCodeWithChild;
           // }
            return response;
        }
        public List<SupplierModels> getsupplierCodeWithChild()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<SupplierModels>();
            if (this._cacheManager.IsSet($"getsupplierCodeWithChild_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<SupplierModels>>($"getsupplierCodeWithChild_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var supplierCodeWithChild = _FormTemplateRepo.getAllSupplier();
                this._cacheManager.Set($"getsupplierCodeWithChild_{userid}_{company_code}_{branch_code}", supplierCodeWithChild, 20);
                response = supplierCodeWithChild;
            }
            return response;
        }
        public List<ProductsModels> GetGroupProducts()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<ProductsModels>();
           // if (this._cacheManager.IsSet($"GetGroupProducts_{userid}_{company_code}_{branch_code}"))
            //{
            //    var data = _cacheManager.Get<List<ProductsModels>>($"GetGroupProducts_{userid}_{company_code}_{branch_code}");
            //    response = data;
            //}
            //else
            //{
                var GroupProducts = _FormTemplateRepo.getAllProduct();
                this._cacheManager.Set($"GetGroupProducts_{userid}_{company_code}_{branch_code}", GroupProducts, 20);
                response = GroupProducts;
            //}
            return response;
        }
        public List<RegionalModels> Getregional()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<RegionalModels>();
            if (this._cacheManager.IsSet($"Getregional_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<RegionalModels>>($"Getregional_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var getAllRegionsList = this._FormTemplateRepo.getAllRegions();
                this._cacheManager.Set($"Getregional_{userid}_{company_code}_{branch_code}", getAllRegionsList, 20);
                response = getAllRegionsList;
            }
            return response;
        }
        public FormDetailRefrence GetRefrenceFlag(string formCode)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<FORM_SETUP_REFERENCE>();
            //if (this._cacheManager.IsSet($"GetRefrenceFlag_{userid}_{company_code}_{branch_code}_{formCode}"))
            //{
            //    var data = _cacheManager.Get<List<FORM_SETUP_REFERENCE>>($"GetRefrenceFlag_{userid}_{company_code}_{branch_code}_{formCode}");
            //    response = data;
            //}
            //else
            //{
            var getRefrenceFlagList = this._FormTemplateRepo.GetRefrenceFlag(formCode);
            var vatRegistrationFlag = this._FormSetupRepo.GetFormControls(formCode);
            var dataList = new FormDetailRefrence();
            dataList.FormSetupRefrence = getRefrenceFlagList;
            dataList.FormControlModels = vatRegistrationFlag;
            // this._cacheManager.Set($"GetRefrenceFlag_{userid}_{company_code}_{branch_code}_{formCode}", getRefrenceFlagList, 20);
            return dataList;
            // }
            return dataList;
        }
        public List<SubLedger> GetSubLedger()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<SubLedger>();
            if (this._cacheManager.IsSet($"GetSubLedger_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<SubLedger>>($"GetSubLedger_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var getSubLedgerList = this._FormTemplateRepo.GetSubLedger();
                this._cacheManager.Set($"GetSubLedger_{userid}_{company_code}_{branch_code}", getSubLedgerList, 20);
                response = getSubLedgerList;
            }
            return response;
        }
        public List<ChargeCode> GetChargeCode()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            try
            {
                var response = new List<ChargeCode>();
                if (this._cacheManager.IsSet($"GetChargeCode_{userid}_{company_code}_{branch_code}"))
                {
                    var data = _cacheManager.Get<List<ChargeCode>>($"GetChargeCode_{userid}_{company_code}_{branch_code}");
                    response = data;
                }
                else
                {
                    var getChargeCodeList = this._FormTemplateRepo.GetChargeCode();
                    this._cacheManager.Set($"GetChargeCode_{userid}_{company_code}_{branch_code}", getChargeCodeList, 20);
                    response = getChargeCodeList;
                }
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public List<CategoryModel> GetCategoryCode()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            try
            {
                var response = new List<CategoryModel>();
                if (this._cacheManager.IsSet($"GetCategoryCode_{userid}_{company_code}_{branch_code}"))
                {
                    var data = _cacheManager.Get<List<CategoryModel>>($"GetCategoryCode_{userid}_{company_code}_{branch_code}");
                    response = data;
                }
                else
                {
                    var getCategoryCodeList = this._FormTemplateRepo.GetCategoryCode();
                    this._cacheManager.Set($"GetCategoryCode_{userid}_{company_code}_{branch_code}", getCategoryCodeList, 20);
                    response = getCategoryCodeList;
                }
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        [HttpGet]
        public List<ApplicationUser> GetUserListByFlter(string filter)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<ApplicationUser>();
            if (this._cacheManager.IsSet($"GetUserListByFlter_{userid}_{company_code}_{branch_code}_{filter}"))
            {
                var data = _cacheManager.Get<List<ApplicationUser>>($"GetUserListByFlter_{userid}_{company_code}_{branch_code}_{filter}");
                response = data;
            }
            else
            {
                var getALLUserListByFlterList = this._FormTemplateRepo.getALLUserListByFlter(filter);
                this._cacheManager.Set($"GetUserListByFlter_{userid}_{company_code}_{branch_code}_{filter}", getALLUserListByFlterList, 20);
                response = getALLUserListByFlterList;
            }
            return response;
        }
        [HttpGet]
        public WebDesktopFolder AddNewFolder(string FOLDER, string FOLDER_COLOR, string ICON)
        {
            return this._FormTemplateRepo.AddNewFolder(FOLDER, FOLDER_COLOR, ICON);
        }
        [HttpGet]
        public List<WebDesktopFolder> GetFoldertByUserId()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<WebDesktopFolder>();
            if (this._cacheManager.IsSet($"GetFoldertByUserId_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<WebDesktopFolder>>($"GetFoldertByUserId_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var getFoldertByUserIdList = this._FormTemplateRepo.GetFoldertByUserId();
                this._cacheManager.Set($"GetFoldertByUserId_{userid}_{company_code}_{branch_code}", getFoldertByUserIdList, 20);
                response = getFoldertByUserIdList;
            }
            return response;
        }
        [HttpPost]
        public List<WebDesktopManagement> AddWebDesktopManagement(WebDesktopManagement webDesktopManagement)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<WebDesktopManagement>();
            if (this._cacheManager.IsSet($"AddWebDesktopManagement_{userid}_{company_code}_{branch_code}_{webDesktopManagement}"))
            {
                var data = _cacheManager.Get<List<WebDesktopManagement>>($"AddWebDesktopManagement_{userid}_{company_code}_{branch_code}_{webDesktopManagement}");
                response = data;
            }
            else
            {
                var getWebDesktopManagementList = this._FormTemplateRepo.AddWebDesktopManagement(webDesktopManagement);
                this._cacheManager.Set($"AddWebDesktopManagement_{userid}_{company_code}_{branch_code}_{webDesktopManagement}", getWebDesktopManagementList, 20);
                response = getWebDesktopManagementList;
            }
            return response;
        }
        [HttpGet]
        public List<WebDesktopManagement> GetFolderTemplateByUserId()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<WebDesktopManagement>();
            if (this._cacheManager.IsSet($"GetFolderTemplateByUserId_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<WebDesktopManagement>>($"GetFolderTemplateByUserId_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var getFolderTemplateByUserIdList = this._FormTemplateRepo.GetFolderTemplateByUserId();
                this._cacheManager.Set($"GetFolderTemplateByUserId_{userid}_{company_code}_{branch_code}", getFolderTemplateByUserIdList, 20);
                response = getFolderTemplateByUserIdList;
            }
            return response;
        }
        [HttpGet]
        public List<FormDetails> GetAllOrederNoByFlter(string FormCode, string filter, string Table_name)
        {
            var getRefrenceOrderNoList = _FormTemplateRepo.getRefrenceOrderNo(FormCode, filter, Table_name);
            //var userid = _workContext.CurrentUserinformation.User_id;
            //var company_code = _workContext.CurrentUserinformation.company_code;
            //var branch_code = _workContext.CurrentUserinformation.branch_code;
            //var response = new List<FormDetails>();
            //if (this._cacheManager.IsSet($"GetAllOrederNoByFlter_{userid}_{company_code}_{branch_code}_{FormCode}_{filter}_{Table_name}"))
            //{
            //    var data = _cacheManager.Get<List<FormDetails>>($"GetAllOrederNoByFlter_{userid}_{company_code}_{branch_code}_{FormCode}_{filter}_{Table_name}");
            //    response = data;
            //}
            //else
            //{
            //    var getRefrenceOrderNoList = _FormTemplateRepo.getRefrenceOrderNo(FormCode, filter, Table_name);
            //    this._cacheManager.Set($"GetAllOrederNoByFlter_{userid}_{company_code}_{branch_code}_{FormCode}_{filter}_{Table_name}", getRefrenceOrderNoList, 20);
            //    response = getRefrenceOrderNoList;
            //}
            return getRefrenceOrderNoList;
        }
        public List<RefrenceType> GetRefrenceType(string formCode)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<RefrenceType>();
      
                var getRefrenceList = this._FormTemplateRepo.getRefrence(formCode);
                //this._cacheManager.Set($"GetRefrenceType_{userid}_{company_code}_{branch_code}_{formCode}", getRefrenceList, 20);
                response = getRefrenceList;
     
            return response;
        }
        public List<RefrenceType> GetDocumentNameFromFormCode(string formCode)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<RefrenceType>();

             var getRefrenceList = this._FormTemplateRepo.getRefrence(formCode,true);
            //this._cacheManager.Set($"GetRefrenceType_{userid}_{company_code}_{branch_code}_{formCode}", getRefrenceList, 20);
            response = getRefrenceList;

            return response;
        }
        public List<TemplateType> GetTemplateType(string formCode)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<TemplateType>();
            //if (this._cacheManager.IsSet($"GetTemplateType_{userid}_{company_code}_{branch_code}_{formCode}"))
            //{
            //    var data = _cacheManager.Get<List<TemplateType>>($"GetTemplateType_{userid}_{company_code}_{branch_code}_{formCode}");
            //    response = data;
            //}
            //else
            //{
            //    var getTemplateList = this._FormTemplateRepo.getTemplate(formCode);
            //    this._cacheManager.Set($"GetTemplateType_{userid}_{company_code}_{branch_code}_{formCode}", getTemplateList, 20);
            //    response = getTemplateList;
            //}
            var getTemplateList = this._FormTemplateRepo.getTemplate(formCode);
            //this._cacheManager.Set($"GetTemplateType_{userid}_{company_code}_{branch_code}_{formCode}", getTemplateList, 20);
            response = getTemplateList;
            return response;
        }
        public List<TemplateType> GetTemplateTypeByTableNameAndFormCode(string formCode, string docname)
        {
            var response = new List<TemplateType>();
            if (docname == "SA_LOADING_SLIP_DETAIL")
            {
                response.Add(new TemplateType() { TABLE_CODE = "0", TABLE_EDESC = "SA_LOADING_SLIP_DETAIL" });
                return response;
            }
            return this._FormTemplateRepo.getTemplates(formCode, docname);
        }
        #region Advance Search
      
        public List<CustomersTree> GetCustomers()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var result = _FormTemplateRepo.getAllCustomer();
            var customerNodes = new List<CustomersTree>();
            if (this._cacheManager.IsSet($"GetCustomers_{userid}_{company_code}_{branch_code}"))
            {
                customerNodes = this._cacheManager.Get<List<CustomersTree>>($"GetCustomers_{userid}_{company_code}_{branch_code}");
            }
            else
            {
                var customerNode = generateCustomerTree(result, customerNodes, "00");
                this._cacheManager.Set($"GetCustomers_{userid}_{company_code}_{branch_code}", customerNode, 30);
            }
            return customerNodes;
        }

        //subin change 
        public List<CustomersTreeModel> GetCustomersWithChildren()
        {
           
            List<CustomersTreeModel> customerNodes = new List<CustomersTreeModel>();
            try
            {
                customerNodes = _FormTemplateRepo.getAllCustomerWithChildren();
                return customerNodes;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        private List<CustomersTree> generateCustomerTree(List<CustomerModels> model, List<CustomersTree> customerNodes, string preItemCode)
        {
            foreach (var customer in model.Where(x => x.PRE_CUSTOMER_CODE == preItemCode))
            {
                var customerNodesChild = new List<CustomersTree>();
                customerNodes.Add(new CustomersTree()
                {
                    Level = customer.LEVEL,
                    customerName = customer.CUSTOMER_EDESC,
                    customerId = customer.CUSTOMER_CODE,
                    masterCustomerCode = customer.MASTER_CUSTOMER_CODE,
                    preCustomerCode = customer.PRE_CUSTOMER_CODE,
                    groupSkuFlag = customer.GROUP_SKU_FLAG,
                    hasCustomers = customer.GROUP_SKU_FLAG == "G" ? false : false,
                    CUSTOMER_FLAG = customer.CUSTOMER_FLAG,
                    ACC_CODE = customer.ACC_CODE,
                    CUSTOMER_NDESC = customer.CUSTOMER_NDESC,
                    CUSTOMER_STARTID = customer.CUSTOMER_STARTID,
                    CUSTOMER_PREFIX = customer.CUSTOMER_PREFIX,
                    REMARKS = customer.REMARKS,
                    PARENT_CUSTOMER_CODE = customer.PARENT_CUSTOMER_CODE,
                    Items = customer.GROUP_SKU_FLAG == "G" ? generateCustomerTree(model, customerNodesChild, customer.MASTER_CUSTOMER_CODE) : null,
                });
            }
            return customerNodes;
        }
        public List<SuppliersTree> GetSuppliers()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var result = _FormTemplateRepo.getAllSupplier();
            var supplierNodes = new List<SuppliersTree>();
            if (this._cacheManager.IsSet($"GetSuppliers_{userid}_{company_code}_{branch_code}"))
            {
                supplierNodes = this._cacheManager.Get<List<SuppliersTree>>($"GetSuppliers_{userid}_{company_code}_{branch_code}");
            }
            else
            {
                var supplierNode = generateSupplierTree(result, supplierNodes, "00");
                this._cacheManager.Set($"GetSuppliers_{userid}_{company_code}_{branch_code}", supplierNode, 30);
            }
            return supplierNodes;
        }
        private List<SuppliersTree> generateSupplierTree(List<SupplierModels> model, List<SuppliersTree> supplierNodes, string preItemCode)
        {
            foreach (var supplier in model.Where(x => x.PRE_SUPPLIER_CODE == preItemCode))
            {
                var supplierNodesChild = new List<SuppliersTree>();
                supplierNodes.Add(new SuppliersTree()
                {
                    Level = supplier.LEVEL,
                    supplierName = supplier.SUPPLIER_EDESC,
                    supplierId = supplier.SUPPLIER_CODE,
                    masterSupplierCode = supplier.MASTER_SUPPLIER_CODE,
                    preSupplierCode = supplier.PRE_SUPPLIER_CODE,
                    groupSkuFlag = supplier.GROUP_SKU_FLAG,
                    hasSuppliers = supplier.GROUP_SKU_FLAG == "G" ? true : false,
                    Items = supplier.GROUP_SKU_FLAG == "G" ? generateSupplierTree(model, supplierNodesChild, supplier.MASTER_SUPPLIER_CODE) : null,
                });

            }
            return supplierNodes;
        }
        public List<AccountCodeTree> getAccountCode()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var result = _FormTemplateRepo.getAllAccountCode();
            var accountNodes = new List<AccountCodeTree>();
            if (this._cacheManager.IsSet($"getAccountCode_{userid}_{company_code}_{branch_code}"))
            {
                accountNodes = this._cacheManager.Get<List<AccountCodeTree>>($"getAccountCode_{userid}_{company_code}_{branch_code}");
            }
            else
            {
                var accountNode = generateAccountTree(result, accountNodes, "00");
                this._cacheManager.Set($"getAccountCode_{userid}_{company_code}_{branch_code}", accountNode, 30);
            }
            return accountNodes;
        }
        private List<AccountCodeTree> generateAccountTree(List<AccountCodeModels> model, List<AccountCodeTree> accNodes, string preaccCode)
        {
            foreach (var account in model.Where(x => x.PRE_ACC_CODE == preaccCode))
            {
                var accountNodesChild = new List<AccountCodeTree>();
                accNodes.Add(new AccountCodeTree()
                {
                    Level = account.LEVEL,
                    AccountName = account.ACC_EDESC,
                    AccountId = account.ACC_CODE,
                    masterAccountCode = account.MASTER_ACC_CODE,
                    preAccountCode = account.PRE_ACC_CODE,
                    accounttypeflag = account.ACC_TYPE_FLAG,
                    hasAccount = account.ACC_TYPE_FLAG == "N" ? true : false,
                    Items = account.ACC_TYPE_FLAG == "N" ? generateAccountTree(model, accountNodesChild, account.MASTER_ACC_CODE) : null,
                });
            }
            return accNodes;
        }
        public List<ProcessTree> getProcessCode()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var result = _FormTemplateRepo.getAllProcess();
            var processNodes = new List<ProcessTree>();
            if (this._cacheManager.IsSet($"getProcessCode_{userid}_{company_code}_{branch_code}"))
            {
                processNodes = this._cacheManager.Get<List<ProcessTree>>($"getProcessCode_{userid}_{company_code}_{branch_code}");
            }
            else
            {
                var processNode = generateProcessTree(result, processNodes, "00");
                this._cacheManager.Set($"getProcessCode_{userid}_{company_code}_{branch_code}", processNode, 30);
            }
            return processNodes;
        }
        private List<ProcessTree> generateProcessTree(List<ProcessModels> model, List<ProcessTree> processNodes, string preprocessCode)
        {
            foreach (var process in model.Where(x => x.PRE_PROCESS_CODE == preprocessCode))
            {
                var processNodesChild = new List<ProcessTree>();
                var HAS_PROCESS = false;
                if (process.PROCESS_FLAG == "C" || process.PROCESS_FLAG == "P")
                {
                    HAS_PROCESS = true;
                }
                processNodes.Add(new ProcessTree()
                {
                    Level = process.LEVEL,
                    ProcessName = process.PROCESS_EDESC,
                    ProcessId = process.PROCESS_CODE,
                    preProcessCode = process.PRE_PROCESS_CODE,
                    groupSkuFlag = process.PROCESS_FLAG,
                    hasProcess = HAS_PROCESS,
                    Items = HAS_PROCESS == true ? generateProcessTree(model, processNodesChild, process.PROCESS_CODE) : null,
                });

            }
            return processNodes;
        }
        public List<BudgetCenterCodeTree> getbudgetCenterCode()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var result = _FormTemplateRepo.getAllBudgetCenterCode();
            var budgetCenterNodes = new List<BudgetCenterCodeTree>();
            if (this._cacheManager.IsSet($"getbudgetCenterCode_{userid}_{company_code}_{branch_code}"))
            {
                budgetCenterNodes = this._cacheManager.Get<List<BudgetCenterCodeTree>>($"getbudgetCenterCode_{userid}_{company_code}_{branch_code}");
            }
            else
            {
                var budgetcenterNode = generateBudgetCenterTree(result, budgetCenterNodes, "00");
                this._cacheManager.Set($"getbudgetCenterCode_{userid}_{company_code}_{branch_code}", budgetcenterNode, 30);
            }
            return budgetCenterNodes;
        }

        //Get Branch

        //public List<BranchCodeTree> getbranchCode()
        //{
        //    var userid = _workContext.CurrentUserinformation.User_id;
        //    var company_code = _workContext.CurrentUserinformation.company_code;
        //    //var branch_code = _workContext.CurrentUserinformation.branch_code;
        //    var result = _FormTemplateRepo.getAllBranch();
        //    var branchNodes = new List<BranchCodeTree>();
        //    if (this._cacheManager.IsSet($"getbranchCode{userid}_{company_code}"))
        //    {
        //        branchNodes = this._cacheManager.Get<List<BranchCodeTree>>($"getbranchCode_{userid}_{company_code}");
        //    }
        //    else
        //    {
        //        var branchcenterNode = generateBudgetCenterTree(result, branchNodes, "00");
        //        this._cacheManager.Set($"getbranchCode{userid}_{company_code}", branchcenterNode, 30);
        //    }
        //    return branchNodes;
        //}

        private List<BudgetCenterCodeTree> generateBudgetCenterTree(List<BudgetCenterCodeModels> model, List<BudgetCenterCodeTree> bNodes, string prebCode)
        {
            foreach (var budgetcenter in model.Where(x => x.PRE_BUDGET_CODE == prebCode))
            {
                var budgetcenterNodesChild = new List<BudgetCenterCodeTree>();
                bNodes.Add(new BudgetCenterCodeTree()
                {
                    Level = budgetcenter.LEVEL,
                    BudgetCenterName = budgetcenter.BUDGET_EDESC,
                    BudgetCenterId = budgetcenter.BUDGET_CODE,
                    masterBudgetCenterCode = budgetcenter.MASTER_BUDGET_CODE,
                    preBudgetCenterCode = budgetcenter.PRE_BUDGET_CODE,
                    budgettypeflag = budgetcenter.BUDGET_TYPE_FLAG,
                    hasBudgetCenter = budgetcenter.BUDGET_TYPE_FLAG == "G" ? true : false,
                    Items = budgetcenter.BUDGET_TYPE_FLAG == "G" ? generateBudgetCenterTree(model, budgetcenterNodesChild, budgetcenter.MASTER_BUDGET_CODE) : null,
                });

            }
            return bNodes;
        }
        public List<EmployeeCodeTree> getEmployeeCode()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var result = _FormTemplateRepo.getAllEmployee();
            var employeeNodes = new List<EmployeeCodeTree>();
            if (this._cacheManager.IsSet($"getEmployeeCode_{userid}_{company_code}_{branch_code}"))
            {
                employeeNodes = this._cacheManager.Get<List<EmployeeCodeTree>>($"getEmployeeCode_{userid}_{company_code}_{branch_code}");
            }
            else
            {
                var employeeNode = generateEmployeeTree(result, employeeNodes, "00");
                this._cacheManager.Set($"getEmployeeCode_{userid}_{company_code}_{branch_code}", employeeNode, 30);
            }
            return employeeNodes;
        }
        private List<EmployeeCodeTree> generateEmployeeTree(List<EmployeeCodeModels> model, List<EmployeeCodeTree> employeeNodes, string preItemCode)
        {
            foreach (var employee in model.Where(x => x.PRE_EMPLOYEE_CODE == preItemCode))
            {
                var employeeNodesChild = new List<EmployeeCodeTree>();
                employeeNodes.Add(new EmployeeCodeTree()
                {
                    Level = employee.LEVEL,
                    employeeName = employee.EMPLOYEE_EDESC,
                    employeeId = employee.EMPLOYEE_CODE,
                    masterEmployeeCode = employee.MASTER_EMPLOYEE_CODE,
                    preEmployeeCode = employee.PRE_EMPLOYEE_CODE,
                    groupSkuFlag = employee.GROUP_SKU_FLAG,
                    hasEmployees = employee.GROUP_SKU_FLAG == "G" ? true : false,
                    Items = employee.GROUP_SKU_FLAG == "G" ? generateEmployeeTree(model, employeeNodesChild, employee.MASTER_EMPLOYEE_CODE) : null,
                });
            }
            return employeeNodes;
        }
        //division

        //public HttpResponseMessage GetDivision()
        //{
        //    try
        //    {
        //        var result = this._FormTemplateRepo.getAllDivisionCode();
        //        return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Success", STATUS_CODE = (int)HttpStatusCode.OK, DATA = result });

        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
        //    }

        //}


        public List<DivisionTree> GetDivision()
        {

            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var result = _FormTemplateRepo.getAllDivisionCode();
            var divisionNodes = new List<DivisionTree>();
            var divisionNode = generatedivisionTree(result, divisionNodes, "00");
            return divisionNode;
        }
        //
        //public List<DivisionTree> GetDivision()
        //{
        //    var userid = _workContext.CurrentUserinformation.User_id;
        //    var company_code = _workContext.CurrentUserinformation.company_code;
        //    var branch_code = _workContext.CurrentUserinformation.branch_code;
        //    var result = _FormTemplateRepo.getAllDivisionCode();
        //    var divisionNodes = new List<DivisionTree>();
        //    if (this._cacheManager.IsSet($"getDivisionCode_{userid}_{company_code}_{branch_code}"))
        //    {
        //        divisionNodes = this._cacheManager.Get<List<DivisionTree>>($"getDivisionCode_{userid}_{company_code}_{branch_code}");
        //    }
        //    else
        //    {
        //        var divisionNode = generatedivisionTree(result, divisionNodes, "00");
        //        this._cacheManager.Set($"getDivisionCode_{userid}_{company_code}_{branch_code}", divisionNode, 30);
        //    }
        //    return divisionNodes;
        //}

        public List<DivisionTree> getDivisionCode()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var result = _FormTemplateRepo.getAllDivision();
            var divisionNodes = new List<DivisionTree>();
            if (this._cacheManager.IsSet($"getDivisionCode_{userid}_{company_code}_{branch_code}"))
            {
                divisionNodes = this._cacheManager.Get<List<DivisionTree>>($"getDivisionCode_{userid}_{company_code}_{branch_code}");
            }
            else
            {
                var divisionNode = generatedivisionTree(result, divisionNodes, "00");
                this._cacheManager.Set($"getDivisionCode_{userid}_{company_code}_{branch_code}", divisionNode, 30);
            }
            return divisionNodes;
        }

        private List<DivisionTree> generatedivisionTree(List<DivisionModels> model, List<DivisionTree> divisionNodes, string predivisionCode)
        {
            foreach (var division in model.Where(x => x.PRE_DIVISION_CODE == predivisionCode))
            {
                var divisionNodesChild = new List<DivisionTree>();
                divisionNodes.Add(new DivisionTree()
                {
                    Level = division.LEVEL,
                    divisionName = division.DIVISION_EDESC,
                    divisionId = division.DIVISION_CODE,
                    //masterdivisionCode = customer.MASTER_CUSTOMER_CODE,
                    predivisionCode = division.PRE_DIVISION_CODE,
                    groupSkuFlag = division.GROUP_SKU_FLAG,
                    hasdivision = division.GROUP_SKU_FLAG == "G" ? true : false,
                    Items = division.GROUP_SKU_FLAG == "G" ? generatedivisionTree(model, divisionNodesChild, division.DIVISION_CODE) : null,
                });
            }
            return divisionNodes;
        }
        public List<LocationTree> GetLocation()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var result = _FormTemplateRepo.getAllLocation();
            var locationNodes = new List<LocationTree>();
            if (this._cacheManager.IsSet($"GetLocation_{userid}_{company_code}_{branch_code}"))
            {
                locationNodes = this._cacheManager.Get<List<LocationTree>>($"GetLocation_{userid}_{company_code}_{branch_code}");
            }
            else
            {
                var locationNode = generateLocationTree(result, locationNodes, "00");
                this._cacheManager.Set($"GetLocation_{userid}_{company_code}_{branch_code}", locationNode, 30);
            }
            return locationNodes;
        }
        private List<LocationTree> generateLocationTree(List<LocationModels> model, List<LocationTree> locationNodes, string prelocationCode)
        {
            foreach (var location in model.Where(x => x.PRE_LOCATION_CODE == prelocationCode))
            {
                var locationNodesChild = new List<LocationTree>();
                locationNodes.Add(new LocationTree()
                {
                    Level = location.LEVEL,
                    LocationName = location.LOCATION_EDESC,
                    LocationId = location.LOCATION_CODE,
                    preLocationCode = location.PRE_LOCATION_CODE,
                    groupSkuFlag = location.GROUP_SKU_FLAG,
                    hasLocation = location.GROUP_SKU_FLAG == "G" ? true : false,
                    Items = location.GROUP_SKU_FLAG == "G" ? generateLocationTree(model, locationNodesChild, location.LOCATION_CODE) : null,
                });
            }
            return locationNodes;
        }
        public List<RegionTree> GetTreeRegional()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var result = _FormTemplateRepo.getAllRegions();
            var regionNodes = new List<RegionTree>();
            if (this._cacheManager.IsSet($"GetTreeRegional_{userid}_{company_code}_{branch_code}"))
            {
                regionNodes = this._cacheManager.Get<List<RegionTree>>($"GetTreeRegional_{userid}_{company_code}_{branch_code}");
            }
            else
            {
                var regionNode = generateRegionalTree(result, regionNodes, "00");
                this._cacheManager.Set($"GetTreeRegional_{userid}_{company_code}_{branch_code}", regionNode, 30);
            }
            return regionNodes;
        }
        private List<RegionTree> generateRegionalTree(List<RegionalModels> model, List<RegionTree> regionNodes, string preregionCode)
        {
            foreach (var region in model.Where(x => x.PRE_REGION_CODE == preregionCode))
            {
                var regionNodesChild = new List<RegionTree>();
                regionNodes.Add(new RegionTree()
                {
                    Level = region.LEVEL,
                    RegionName = region.REGION_EDESC,
                    RegionId = region.REGION_CODE,
                    preRegionCode = region.PRE_REGION_CODE,
                    groupSkuFlag = region.GROUP_SKU_FLAG,
                    hasRegion = region.GROUP_SKU_FLAG == "G" ? true : false,
                    Items = region.GROUP_SKU_FLAG == "G" ? generateRegionalTree(model, regionNodesChild, region.REGION_CODE) : null,
                });
            }
            return regionNodes;
        }
        public List<ResourceTree> GetResource()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var result = _FormTemplateRepo.getAllResource();
            var resourceNodes = new List<ResourceTree>();
            if (this._cacheManager.IsSet($"GetResource_{userid}_{company_code}_{branch_code}"))
            {
                resourceNodes = this._cacheManager.Get<List<ResourceTree>>($"GetResource_{userid}_{company_code}_{branch_code}");
            }
            else
            {
                var resourceNode = generateResourceTree(result, resourceNodes, "00");
                this._cacheManager.Set($"GetResource_{userid}_{company_code}_{branch_code}", resourceNode, 30);
            }
            return resourceNodes;
        }
        private List<ResourceTree> generateResourceTree(List<ResourceModels> model, List<ResourceTree> resourceNodes, string preResourceCode)
        {
            foreach (var resource in model.Where(x => x.PRE_RESOURCE_CODE == preResourceCode))
            {
                var resourceNodesChild = new List<ResourceTree>();
                resourceNodes.Add(new ResourceTree()
                {
                    Level = resource.LEVEL,
                    ResourceName = resource.RESOURCE_EDESC,
                    ResourceId = resource.RESOURCE_CODE,
                    preResourceCode = resource.PRE_RESOURCE_CODE,
                    groupSkuFlag = resource.GROUP_SKU_FLAG,
                    hasResource = resource.GROUP_SKU_FLAG == "G" ? true : false,
                    Items = resource.GROUP_SKU_FLAG == "G" ? generateResourceTree(model, resourceNodesChild, resource.RESOURCE_CODE) : null,
                });

            }
            return resourceNodes;
        }

        public List<DealerModel> getDealerParent()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<DealerModel>();

            var dealerCodeWithChild = _FormTemplateRepo.getAllDealer();
            return dealerCodeWithChild;
        }

        public List<DealerTree> GetDealer()
        {

            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var result = _FormTemplateRepo.getAllDealer();
            var dealerNodes = new List<DealerTree>();
            var dealerNode = generateDealerTree(result, dealerNodes, "00");
            return dealerNodes;
        }
        private List<DealerTree> generateDealerTree(List<DealerModel> model, List<DealerTree> dealerNodes, string preDealerCode)
        {
            foreach (var branch in model.Where(x => x.PRE_PARTY_CODE == preDealerCode))
            {
                var branchNodesChild = new List<DealerTree>();
                dealerNodes.Add(new DealerTree()
                {
                    Level = branch.LEVEL,
                    PARTY_TYPE_EDESC = branch.PARTY_TYPE_EDESC,
                    PARTY_TYPE_CODE = branch.PARTY_TYPE_CODE,
                    dealerName = branch.PARTY_TYPE_EDESC,
                    dealerId = branch.PARTY_TYPE_CODE,
                    preDealerCode = branch.PRE_PARTY_CODE,
                    masterDealerCode = branch.MASTER_PARTY_CODE,
                    groupSkuFlag = branch.GROUP_SKU_FLAG,
                    hasDealers = branch.GROUP_SKU_FLAG == "G" ? true : false,
                    Items = branch.GROUP_SKU_FLAG == "G" ? generateDealerTree(model, branchNodesChild, branch.MASTER_PARTY_CODE) : null,
                });
            }
            return dealerNodes;
        }
        //
        public List<BranchTree> GetBranch()
        {

            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var result = _FormTemplateRepo.getAllBranch();
            var branchNodes = new List<BranchTree>();
            var branchNode = generateBranchTree(result, branchNodes, "00");
            return branchNodes;
        }
        //
        //public List<BranchTree> GetBranch()
        //{
        //    var userid = _workContext.CurrentUserinformation.User_id;
        //    var company_code = _workContext.CurrentUserinformation.company_code;
        //    var branch_code = _workContext.CurrentUserinformation.branch_code;
        //    var result = _FormTemplateRepo.getAllBranch();
        //    var branchNodes = new List<BranchTree>();
        //    if (this._cacheManager.IsSet($"GetBranch_{userid}_{company_code}_{branch_code}"))
        //    {
        //        branchNodes = this._cacheManager.Get<List<BranchTree>>($"GetBranch_{userid}_{company_code}_{branch_code}");
        //    }
        //    else
        //    {
        //        var branchNode = generateBranchTree(result, branchNodes, "00");
        //        this._cacheManager.Set($"GetBranch_{userid}_{company_code}_{branch_code}", branchNode, 30);
        //    }
        //    return branchNodes;
        //}
        private List<BranchTree> generateBranchTree(List<BranchModels> model, List<BranchTree> BranchNodes, string prebranchCode)
        {
            foreach (var branch in model.Where(x => x.PRE_BRANCH_CODE == prebranchCode))
            {
                var branchNodesChild = new List<BranchTree>();
                BranchNodes.Add(new BranchTree()
                {
                    Level = branch.LEVEL,
                    BRANCH_EDESC = branch.BRANCH_EDESC,
                    BRANCH_CODE = branch.BRANCH_CODE,
                    BranchName = branch.BRANCH_EDESC,
                    BranchId = branch.BRANCH_CODE,
                    preBranchCode = branch.PRE_BRANCH_CODE,
                    masterBranchCenterCode = branch.MASTER_BRANCH_CODE,
                    groupSkuFlag = branch.GROUP_SKU_FLAG,
                    hasbranch = branch.GROUP_SKU_FLAG == "G" ? true : false,
                    Items = branch.GROUP_SKU_FLAG == "G" ? generateBranchTree(model, branchNodesChild, branch.BRANCH_CODE) : null,
                });
            }
            return BranchNodes;
        }
        public List<ProductsTree> GetProducts()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var result = _FormTemplateRepo.getAllProduct();
            var productNodes = new List<ProductsTree>();
            if (this._cacheManager.IsSet($"GetProducts_{userid}_{company_code}_{branch_code}"))
            {
                productNodes = this._cacheManager.Get<List<ProductsTree>>($"GetProducts_{userid}_{company_code}_{branch_code}");
            }
            else
            {
                var productNode = generateProductTree(result, productNodes, "00");
                this._cacheManager.Set($"GetProducts_{userid}_{company_code}_{branch_code}", productNode, 30);
            }
            return productNodes;
        }
        private List<ProductsTree> generateProductTree(List<ProductsModels> model, List<ProductsTree> productNodes, string preItemCode)
        {
            foreach (var item in model.Where(x => x.PRE_ITEM_CODE == preItemCode))
            {
                var productNodesChild = new List<ProductsTree>();
                productNodes.Add(new ProductsTree()
                {
                    Level = item.LEVEL,
                    itemName = item.ITEM_EDESC,
                    itemCode = item.ITEM_CODE,
                    masterItemCode = item.MASTER_ITEM_CODE,
                    preItemCode = item.PRE_ITEM_CODE,
                    groupSkuFlag = item.GROUP_SKU_FLAG,
                    hasItems = item.GROUP_SKU_FLAG == "G" ? true : false,
                    Items = item.GROUP_SKU_FLAG == "G" ? generateProductTree(model, productNodesChild, item.MASTER_ITEM_CODE) : null,
                });
            }
            return productNodes;
        }
        public List<FormsTree> GetFormsTree()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var result = _FormTemplateRepo.getAllForms();
            var formNodes = new List<FormsTree>();
            var getModules = _FormTemplateRepo.getAllModules();
            var getSetupList = _FormTemplateRepo.GetAllSetupMenuItems();
            var formNode = new List<FormsTree>();
            var datageneration = new List<FormsTree>();
            if (this._cacheManager.IsSet($"GetFormsTree_{userid}_{company_code}"))
            {
                formNode = this._cacheManager.Get<List<FormsTree>>($"GetFormsTree_{userid}_{company_code}");
            }
            else
            {
                foreach (var item in getModules)
                {
                    var moduleCode1 = new List<FormSetup>();
                    moduleCode1 = _FormTemplateRepo.getAllFormsListAccordingToModule(item.MODULE_CODE);
                    foreach (var inv in moduleCode1)
                    {
                        if (inv.MODULE_CODE == "01" && inv.GROUP_SKU_FLAG == "I")
                        {
                            inv.FORM_URL = "/DocumentTemplate/Home/Index#!DT/FinanceVoucher?formcode=" + inv.FORM_CODE;
                        }
                        else if (inv.MODULE_CODE == "02" && inv.GROUP_SKU_FLAG == "I")
                        {
                            inv.FORM_URL = "/DocumentTemplate/Home/Index#!DT/Inventory?formcode=" + inv.FORM_CODE;

                        }
                        else if (inv.MODULE_CODE == "03" && inv.GROUP_SKU_FLAG == "I")
                        {
                            inv.FORM_URL = "/DocumentTemplate/Home/Index#!DT/Inventory?formcode=" + inv.FORM_CODE;
                        }
                        else if (inv.MODULE_CODE == "04" && inv.GROUP_SKU_FLAG == "I")
                        {
                            inv.FORM_URL = "/DocumentTemplate/Home/Index#!DT/formtemplate/" + inv.FORM_CODE;
                        }
                        else { inv.FORM_URL = ""; }
                    }
                    FormSetup form = new FormSetup();
                    form.FORM_EDESC = item.MODULE_EDESC;
                    form.PRE_FORM_CODE = "111111";
                    form.MASTER_FORM_CODE = "00";
                    form.GROUP_SKU_FLAG = "G";
                    moduleCode1.Add(form);
                    datageneration = generateFormsTree(moduleCode1, formNodes, "111111");
                }
                formNode.AddRange(datageneration);
                //formNode.Insert(0, new FormsTree { formName = "Document Template", Items = null, groupSkuFlag = "G" });
                List<FormsTree> formSetup = new List<FormsTree>();

                formSetup.Add(new FormsTree() { formName = "Master Setup", groupSkuFlag = "G", preFormCode = "111111", iconPath = "fa fa-cogs", masterFormCode = "00", hasForms = true });
                foreach (var item in formSetup)
                {
                    foreach (var items in getSetupList)
                    {
                        item.Items.Add(new FormsTree() { formName = items.MENU_OBJECT_NAME, urlForSetup = items.FULL_PATH, iconPath = items.ICON_PATH, groupSkuFlag = "G", preFormCode = "00", masterFormCode = "", moduleCode = "setup", Items = null });
                    }
                }
                formNode.AddRange(formSetup);
                var descriptionList = _pluginFinder.GetPluginDescriptors(LoadPluginsMode.InstalledOnly, 0).ToList();
                var descList = new List<FormsTree>();
                foreach (var desc in descriptionList)
                {
                    List<FormsTree> Planningform = new List<FormsTree>();
                    var installedPluginMenuList = _FormTemplateRepo.GetAllMenuItems(desc.ModuleCode);
                    if (installedPluginMenuList.Count > 0)
                    {
                        foreach (var item in installedPluginMenuList)
                        {
                            if (desc.FriendlyName == "LC")
                            {
                                if (item.FULL_PATH == "javascript:;")
                                {
                                    Planningform.Add(new FormsTree() { formName = item.MENU_EDESC, urlForSetup = null, groupSkuFlag = "G", preFormCode = item.PRE_FORM_CODE, masterFormCode = item.MENU_NO, iconPath = item.ICON_PATH, hasForms = true });
                                }
                                else
                                {
                                    Planningform.Add(new FormsTree() { formName = item.MENU_EDESC, urlForSetup = item.FULL_PATH, groupSkuFlag = "G", preFormCode = item.PRE_FORM_CODE, masterFormCode = item.MENU_NO, iconPath = item.ICON_PATH, hasForms = true });
                                }
                            }
                            else if (desc.FriendlyName == "Business intelligence Tool")
                            {
                                if (item.FULL_PATH == "javascript:;")
                                {
                                    Planningform.Add(new FormsTree() { formName = item.MENU_EDESC, urlForSetup = null, groupSkuFlag = "G", preFormCode = item.PRE_FORM_CODE, masterFormCode = item.MENU_NO, iconPath = item.ICON_PATH, hasForms = true });
                                }
                                else
                                {
                                    Planningform.Add(new FormsTree() { formName = item.MENU_EDESC, urlForSetup = item.FULL_PATH, groupSkuFlag = "G", preFormCode = item.PRE_FORM_CODE, masterFormCode = item.MENU_NO, iconPath = item.ICON_PATH, hasForms = true });
                                }
                            }
                            else
                            {
                                Planningform.Add(new FormsTree() { formName = item.MENU_EDESC, urlForSetup = null, groupSkuFlag = "G", preFormCode = item.PRE_FORM_CODE, masterFormCode = item.MENU_NO, iconPath = item.ICON_PATH, hasForms = true });
                            }
                        }
                        foreach (var pform in Planningform)
                        {
                            var childdatas = _FormTemplateRepo.GetAllMenuItemsByPreMenuCode(pform.masterFormCode, desc.ModuleCode);
                            if (childdatas.Count > 0)
                            {
                                foreach (var cdata in childdatas)
                                {

                                    pform.Items.Add(new FormsTree() { formName = cdata.MENU_EDESC, urlForSetup = cdata.FULL_PATH, groupSkuFlag = cdata.GROUP_SKU_FLAG, preFormCode = cdata.PRE_FORM_CODE, masterFormCode = cdata.MASTER_FORM_CODE, moduleCode = "all", iconPath = cdata.ICON_PATH, hasForms = true });
                                }
                            }
                        }
                        var obj1 = new FormsTree
                        {
                            formName = desc.FriendlyName,
                            groupSkuFlag = "G",
                            Items = Planningform,
                            urlForSetup = null
                        };
                        descList.Add(obj1);
                    }
                }
                formNode.AddRange(descList);
                this._cacheManager.Set($"GetFormsTree_{userid}_{company_code}_{branch_code}", formNode, 30);
            }
            return formNode;
        }
        private List<FormsTree> generateFormsTree(List<FormSetup> model, List<FormsTree> customerNodes, string preItemCode)
        {
            foreach (var form in model.Where(x => x.PRE_FORM_CODE == preItemCode))
            {
                var formNodesChild = new List<FormsTree>();
                if (form.FORM_URL == "")
                {
                    customerNodes.Add(new FormsTree()
                    {
                        formName = form.FORM_EDESC,
                        moduleCode = form.MODULE_CODE,
                        formId = form.FORM_CODE,
                        masterFormCode = form.MASTER_FORM_CODE,
                        preFormCode = form.PRE_FORM_CODE,
                        groupSkuFlag = form.GROUP_SKU_FLAG,
                        urlForSetup = null,
                        hasForms = form.GROUP_SKU_FLAG == "G" ? true : false,
                        Items = form.GROUP_SKU_FLAG == "G" ? generateFormsTree(model, formNodesChild, form.MASTER_FORM_CODE) : null,
                    });
                }
                else
                {
                    customerNodes.Add(new FormsTree()
                    {
                        formName = form.FORM_EDESC,
                        moduleCode = form.MODULE_CODE,
                        formId = form.FORM_CODE,
                        masterFormCode = form.MASTER_FORM_CODE,
                        preFormCode = form.PRE_FORM_CODE,
                        groupSkuFlag = form.GROUP_SKU_FLAG,
                        urlForSetup = form.FORM_URL,
                        hasForms = form.GROUP_SKU_FLAG == "G" ? true : false,
                        Items = form.GROUP_SKU_FLAG == "G" ? generateFormsTree(model, formNodesChild, form.MASTER_FORM_CODE) : null,
                    });
                }
            }
            return customerNodes;
        }
        #endregion
        public IHttpActionResult GetAccountListByAccountCode(string accId, string accMastercode, string searchText)
        {
            var result = _FormTemplateRepo.GetAccountListByAccountCode(accId, accMastercode, searchText);

            return Ok(result);
        }
        public IHttpActionResult GetSupplierListBySupplierCode(string supplierId, string supplierMastercode, string searchText)
        {
            var result = _FormTemplateRepo.GetSupplierListBySupplierCode(supplierId, supplierMastercode, searchText);
            return Ok(result);
        }
        public IHttpActionResult GetCustomerListByCustomerCode(string customerId, string customerMastercode, string searchText)
        {
            var result = _FormTemplateRepo.GetCustomerListByCustomerCode(customerId, customerMastercode, searchText);
            return Ok(result);
        }
        public IHttpActionResult GetProductListByItemCode(string itemCode, string itemMastercode, string searchText)
        {
            var result = _FormTemplateRepo.GetProductListByItemCode(itemCode, itemMastercode, searchText);
            return Ok(result);
        }
        public IHttpActionResult GetlocationListBylocationCode(string locationId, string locationCode, string searchText)
        {
            var result = _FormTemplateRepo.GetLocationListByLocationCode(locationId, locationCode, searchText);
            return Ok(result);
        }
        public IHttpActionResult GetbranchListBybranchCode(string branchId, string branchCode, string searchText)
        {
            var result = _FormTemplateRepo.GetBranchListByBranchCode(branchId, branchCode, searchText);
            return Ok(result);
        }
        public IHttpActionResult GetemployeeListByemployeeCode(string employeeId, string employeeMasterCode, string searchText)
        {
            var result = _FormTemplateRepo.GetEmployeeListByEmployeeCode(employeeId, employeeMasterCode, searchText);
            return Ok(result);
        }
        public IHttpActionResult GetdivisionListBydivisionCode(string divisionId, string divisionMasterCode, string searchText)
        {
            var result = _FormTemplateRepo.GetdivisionListBydivisionCode(divisionId, divisionMasterCode, searchText);
            return Ok(result);
        }
        public HttpResponseMessage GetSubLedgerbyAccountCode(string accountcode)
        {
            var result = _FormTemplateRepo.GetSubLedgerByAccountCode(accountcode);
            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
        }
        public HttpResponseMessage GetChargeCodebyFormCode(string formcode)
        {
            var result = _FormTemplateRepo.GetChargeCodebyFormCode(formcode);
            return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = result, STATUS_CODE = (int)HttpStatusCode.OK });
        }
        [HttpGet]
        public List<TDSCODE> GetAllTDSByFilter(string filter)
        {

            var response = _FormTemplateRepo.getALLTDSByFlter(filter);
            return response;
        }
        [HttpGet]
        public List<VECHILES> GetAllVechileDtlsByFilter(string filter)
        {

            var response = _FormTemplateRepo.GetAllVechDetailsByFilter(filter);
            return response;
        }
        [HttpGet]
        public List<TRANSPORTER> GetAllTransporterDtlsByFilter(string filter)
        {

            var response = _FormTemplateRepo.GetAllTransporterDetailsByFilter(filter);
            return response;
        }
        [HttpGet]
        public List<ShippingDetailsViewModel> GetAllShippingDtlsByFilter(string FormCode, string VoucherNo)
        {

            var response = _FormTemplateRepo.GetShippingData(FormCode, VoucherNo);
            return response;
        }
        [HttpGet]
        public List<ShippingDetailsViewModel> GetAllShippingDtlsByVno(string VoucherNo)
        {

            var response = _FormTemplateRepo.GetShippingDataByVoucherNo(VoucherNo);
            return response;
        }
        [HttpGet]
        public List<CITY> GetAllCityDtlsByFilter(string filter)
        {

            var response = _FormTemplateRepo.GetAllCityDetailsByFilter(filter);
            return response;
        }
        [HttpGet]
        public decimal GetStarndardRate(string customercode, string formcode, string areacode, string itemcode)
        {
            var result = this._FormTemplateRepo.GetSatanderedRateByFilters(customercode, formcode, areacode, itemcode);
            return result;
        }
        [HttpGet]
        public string GetPrintTemplateName(string formCode)
        {
            return _FormTemplateRepo.GetPrintTemplateByFormCode(formCode);
           
        }
        [HttpGet]
        public List<PriceList> GetPriceListByFilterAndCustomerCode(string filter, string customercode)
        {
            var AllPriceListByFilterList = this._FormTemplateRepo.GetAllPriceListByFilterAndCustomerCode(filter, customercode);
            return AllPriceListByFilterList;
        }
        [HttpGet]
        public decimal GetItemRateRateMasterId(string masterid, string itemcode)
        {
            var result = this._FormTemplateRepo.GetItemRateByMasterId(masterid, itemcode);
            return result;
        }
        //[HttpGet]
        //public decimal GetPrintCountByVC(string voucherno)
        //{
        //    var result = this._FormTemplateRepo.GetPrintCountByVoucherNo(voucherno);
        //    return result;
        //}
        //[HttpGet]
        //public int GetPrintCountByVoucherNo(string voucherno, string formcode)
        //{
        //    var result = this._FormTemplateRepo.GetPrintCountByVoucherNo(voucherno,formcode);
        //    return result;
        //}
        //[HttpPost]
        //public HttpResponseMessage UpdatePrintCount(string VoucherNo, string formcode)
        //{
        //    try
        //    {
        //        this._FormTemplateRepo.UpdatePrintCount(VoucherNo, formcode);

        //        return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "SUCCESS", STATUS_CODE = (int)HttpStatusCode.OK });
        //    }
        //    catch (Exception)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
        //    }
        //}
        [HttpGet]
        public int GetPrintCountByVoucherNo(string voucherno, string formcode,string updateCount="false")
        {
            var result = this._FormTemplateRepo.GetPrintCountByVoucherNo(voucherno, formcode, updateCount);
            return result;
        }
        [HttpPost]
        public HttpResponseMessage UpdatePrintCount(string VoucherNo, string formcode)
        {
            try
            {
                this._FormTemplateRepo.UpdatePrintCount(VoucherNo, formcode);

                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "SUCCESS", STATUS_CODE = (int)HttpStatusCode.OK });
            }
            catch (Exception)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "ERROR", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpGet]
        public string getCustEdesc(string code)
        {
            var result = this._FormTemplateRepo.GetcustomerNameByCode(code);
            return result;
        }

        [HttpGet]
        public string getItemEdesc(string code)
        {
            var result = this._FormTemplateRepo.GetItemNameByCode(code);
            return result;
        }
        [HttpGet]
        public string getACCEdesc(string code)
        {
            var result = this._FormTemplateRepo.GetAccNameByCode(code);
            return result;
        }
        [HttpGet]
        public string getPartyTypeEdesc(string partytypecode)
        {
            var result = this._FormTemplateRepo.GetParytTypeNameByCode(partytypecode);
            return result;
        }
        [HttpGet]
        public string GetLoactionNameByCode(string locationcode)
        {
            var result = this._FormTemplateRepo.GetLoactionNameByCode(locationcode);
            return result;
        }
        [HttpGet]
        public string GetBudgetNameByBCode(string code)
        {
            var result = this._FormTemplateRepo.GetBudgetNameByCode(code);
            return result;
        }
        [HttpGet]
        public COMMON_COLUMN getRefNo(string orderno)
        {
            var result = this._FormTemplateRepo.GetReferenceNoByOrderNo(orderno);
            return result;
        }
        [HttpPost]
        public string deleteUploadedFile(DropZoneFile model)
        {
            try
            {
                _FormTemplateRepo.DeleteUploadedFile(model);
                return "sucess";
            }
            catch (Exception)
            {

                return "fail";
            }
        }

        [HttpGet]
        public List<REFERENCE_DETAILS> getRefDetails(string VoucherNo, string formcode)
        {
            var response = _FormTemplateRepo.GetReference_Details_For_VoucherNo(VoucherNo, formcode);
            return response;
        }
        [HttpGet]
        public List<Customers> GetCustomerInfoByCode(string filter)
        {

            var Filterdata = this._FormTemplateRepo.GetCustomerDetail(filter);
            return Filterdata;

        }
        [HttpGet]
        public GuestInfoFromMaterTransaction GetGuestInfoFromMasterTransaction(string formCode, string orderno)
        {
            GuestInfoFromMaterTransaction guestInfos = new GuestInfoFromMaterTransaction();
            var guestInfo = this._FormTemplateRepo.GetGuestInfoFromMasterTransaction(formCode, orderno);
            guestInfos = guestInfo;
            
            return guestInfos;
        }
        [HttpGet]
        public bool getItemCountResult(string code)
        {
            var result = this._FormTemplateRepo.ItemNoExistsOrNot(code);
            return result;
        }

        [HttpGet]
        public bool getBatchItemCountResult(string code)
        {
            var result = this._FormTemplateRepo.BatchItemNoExistsOrNot(code);
            return result;
        }
        [HttpGet]
        public List<BATCHTRANSACTIONDATA> GetDataForBatchModalsales(string itemcode,string loactioncode)
        {
            var response = new List<BATCHTRANSACTIONDATA>();
            if (string.IsNullOrEmpty(itemcode) || string.IsNullOrEmpty(loactioncode))
            {
                return response;
            }
                
                var batchTransactionList = this._FormTemplateRepo.GetbatchdetailByItemCodeAndLocCode(itemcode, loactioncode);
                response = batchTransactionList;
                return response;
            }
           
        

        [HttpGet]
        public List<BATCHTRANSACTIONDATA> GetDataForBatchModalsalesforedit(string itemcode, string loactioncode, string voucherno)
        {
            var response = new List<BATCHTRANSACTIONDATA>();
            if (string.IsNullOrEmpty(itemcode) || string.IsNullOrEmpty(loactioncode) || string.IsNullOrEmpty(voucherno))
            {
                return response;
            }
            else
            {
                var batchTransactionList = this._FormTemplateRepo.GetbatchdetailByItemCodeAndLocCodeforedit(itemcode, loactioncode, voucherno);
                response = batchTransactionList;
                return response;
            }
              
            }
        [HttpGet]
        public CompanyInfo GetCompanyInfo()
        {
            _logErp.InfoInFile("Get CompanyInfo:  For Print");
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;

            var response = new CompanyInfo();
            if (this._cacheManager.IsSet($"CompanyInfo{_workContext.CurrentUserinformation.User_id}_{company_code}_{branch_code}"))
            {

                var data = _cacheManager.Get<CompanyInfo>($"CompanyInfo{_workContext.CurrentUserinformation.User_id}_{company_code}_{branch_code}");
                _logErp.InfoInFile(data + " Company Details setup has been fetched from cached for  formcode");
                response = data;
            }
            else
            {
                var formDetailList = this._FormTemplateRepo.GetCompanyInfo();
                this._cacheManager.Set($"CompanyInfo{_workContext.CurrentUserinformation.User_id}_{company_code}_{branch_code}", formDetailList, 20);
                _logErp.InfoInFile(formDetailList + " form details setup has beed fetched for " + formDetailList.COMPANY_EDESC + " formcode");
                response = formDetailList;
            }
            return response;
        }
        public List<BATCHTRANSACTIONDATA> GetbatchTranDataByItemCodeAndLocCode(string itemcode, string loactioncode, string refernceNo = null)
        {
            var response = new List<BATCHTRANSACTIONDATA>();
            if ((string.IsNullOrEmpty(itemcode) && string.IsNullOrEmpty(loactioncode)) || string.IsNullOrEmpty(itemcode))
            {
                return response;

            }
            else
            {
                var batchTransactionList = this._FormTemplateRepo.GetbatchTranDataByItemCodeAndLocCode(itemcode, loactioncode, refernceNo);
                response = batchTransactionList;
                return response;
            }
        }
        [HttpGet]
        public bool BatchWiseItemCheck(string code)
        {
            var result = this._FormTemplateRepo.BatchWiseItemCheck(code);
            return result;
        }
        [HttpGet]
        public List<LoadingSlipModalForPrint> GetLoadingSlipListByReferenceno(string referenceno)
        {
            List<LoadingSlipModalForPrint> lSModel = new List<LoadingSlipModalForPrint>();
            try
            {
               
                
                lSModel = _FormTemplateRepo.GetLoadingSlipListByReferenceoNo(referenceno);
                return lSModel;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        public List<SchemeModels> getSchemeCodeWithChild()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<SchemeModels>();
            if (this._cacheManager.IsSet($"getSchemeCodeWithChild_{userid}_{company_code}_{branch_code}"))
            {
                var data = _cacheManager.Get<List<SchemeModels>>($"getSchemeCodeWithChild_{userid}_{company_code}_{branch_code}");
                response = data;
            }
            else
            {
                var SchemeCodeWithChild = _FormTemplateRepo.getAllScheme();
                foreach (var data in SchemeCodeWithChild)
                {
                    StringWriter myWriter = new StringWriter();

                    // Decode the encoded string.
                    HttpUtility.HtmlDecode(data.QUERY_STRING, myWriter);

                    string myDecodedString = myWriter.ToString();
                    data.QUERY_STRING = myDecodedString;
                }
                this._cacheManager.Set($"getSchemeCodeWithChild_{userid}_{company_code}_{branch_code}", SchemeCodeWithChild, 20);
                response = SchemeCodeWithChild;
            }
            return response;
        }


        public List<SchemeModels> getSchemeCodenotimplemented()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<SchemeModels>();
            var SchemeCodeWithChild = _FormTemplateRepo.getAllSchemenotimplemented();
            response = SchemeCodeWithChild;
            //if (this._cacheManager.IsSet($"getSchemeCodenotimplemented_{userid}_{company_code}_{branch_code}"))
            //{
            //    var data = _cacheManager.Get<List<SchemeModels>>($"getSchemeCodenotimplemented_{userid}_{company_code}_{branch_code}");
            //    response = data;
            //}
            //else
            //{
            //    var SchemeCodeWithChild = _FormTemplateRepo.getAllSchemenotimplemented();
            //    foreach (var data in SchemeCodeWithChild)
            //    {
            //        StringWriter myWriter = new StringWriter();

            //        // Decode the encoded string.
            //        HttpUtility.HtmlDecode(data.QUERY_STRING, myWriter);

            //        string myDecodedString = myWriter.ToString();
            //        data.QUERY_STRING = myDecodedString;
            //    }
            //    this._cacheManager.Set($"getSchemeCodenotimplemented_{userid}_{company_code}_{branch_code}", SchemeCodeWithChild, 20);
            //    response = SchemeCodeWithChild;
            //}
            return response;
        }

        public List<SchemeModels> getManualScheme(string status,string from,string to)
        {
            var response = new List<SchemeModels>();
            var data = _FormTemplateRepo.getAllManualScheme(status, from,to);
            response = data;
            return response;
        }
        public List<Document> GetDocumentCode()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            try
            {
                var response = new List<Document>();
                if (this._cacheManager.IsSet($"GetDocumentCode_{userid}_{company_code}_{branch_code}"))
                {
                    var data = _cacheManager.Get<List<Document>>($"GetDocumentCode_{userid}_{company_code}_{branch_code}");
                    response = data;
                }
                else
                {
                    var getDocumentCodeList = this._FormTemplateRepo.getAllDocument();
                    this._cacheManager.Set($"GetDocumentCode_{userid}_{company_code}_{branch_code}", getDocumentCodeList, 20);
                    response = getDocumentCodeList;
                }
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public List<Document> GetDocumentByFilter(string filter)
      {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            try
            {
                var Filterdata = new List<Document>();
                if (filter == null)
                    return Filterdata;
                if (this._cacheManager.IsSet($"GetDocumentCode_{userid}_{company_code}_{branch_code}_{filter}"))
                {
                    var data = _cacheManager.Get<List<Document>>($"GetDocumentCode_{userid}_{company_code}_{branch_code}_{filter}");
                    Filterdata = data;
                    return Filterdata;
                }
                else
                {
                    var AllFilterDocument = this._FormTemplateRepo.getDocumentByFilter(filter);
                    if( AllFilterDocument.Count>0 )
                    {
                        var DocumentsCodes = AllFilterDocument.Where(x => x.FORM_CODE.ToLower().Contains(filter));
                        var StartWithDocumentsCode = DocumentsCodes.Where(x => x.FORM_CODE.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithCodes = StartWithDocumentsCode.Select(x => x.FORM_CODE.ToLower()).ToList();
                        var EndWithDocumentCode = DocumentsCodes.Where(x => !startWithCodes.Contains(x.FORM_CODE.ToLower()) && x.FORM_CODE.ToLower().EndsWith(filter.Trim()));
                        var endWithCodes = EndWithDocumentCode.Select(x => x.FORM_CODE.ToLower()).ToList();
                        var ContainsDocumentCode = DocumentsCodes.Where(x => !startWithCodes.Contains(x.FORM_CODE.ToLower()) && !endWithCodes.Contains(x.FORM_CODE.ToLower()));
                        Filterdata.AddRange(StartWithDocumentsCode);
                        Filterdata.AddRange(ContainsDocumentCode);
                        Filterdata.AddRange(EndWithDocumentCode);
                        var Removedata = AllFilterDocument.RemoveAll(x => x.FORM_CODE.ToLower().Contains(filter));
                        var DocumentNames = AllFilterDocument.Where(x => x.FORM_EDESC.ToLower().Contains(filter));
                        var StartWithDocumentName = DocumentNames.Where(x => x.FORM_EDESC.ToLower().StartsWith(filter.Trim())).ToList();
                        var startWithNames = StartWithDocumentName.Select(x => x.FORM_EDESC.ToLower()).ToList();
                        var EndWithDocumentName = DocumentNames.Where(x => !startWithNames.Contains(x.FORM_EDESC.ToLower()) && x.FORM_EDESC.ToLower().EndsWith(filter.Trim()));
                        var endWithNames = EndWithDocumentName.Select(x => x.FORM_EDESC.ToLower()).ToList();
                        var ContainsDocumentName = DocumentNames.Where(x => !startWithNames.Contains(x.FORM_EDESC.ToLower()) && !endWithNames.Contains(x.FORM_EDESC.ToLower()));
                        Filterdata.AddRange(StartWithDocumentName);
                        Filterdata.AddRange(ContainsDocumentName);
                        Filterdata.AddRange(EndWithDocumentName);
                        AllFilterDocument.RemoveAll(x => x.FORM_EDESC.ToLower().Contains(filter));
                        this._cacheManager.Set($"AllFilterDocument_{userid}_{company_code}_{branch_code}_{filter}", Filterdata, 20);
                        return Filterdata;
                    }
                    return AllFilterDocument;
                    //this._cacheManager.Set($"GetDocumentCode_{userid}_{company_code}_{branch_code}_{filter}", getDocumentCodeList, 20);
                    //response = getDocumentCodeList;
                }
                //return Filterdata;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public List<CustomerModels> GetAllCustomerForScheme()
        {
            var result = this._FormTemplateRepo.getAllSchemeCustomer();
            return result;
        }
        public List<CustomerModels> GetCustomersForInterestCalc(string acccodes)
        {
            string codes = string.Empty;

            var result = this._FormTemplateRepo.getAllInterestCalcCustomers(codes);
            return result;
        }
        public List<CustomerModels> GetCustomerForSchemeByCode(string code)
        {
            var result = this._FormTemplateRepo.getSchemeCustomerByCodes(code);
            return result;
        }
        public List<ProductsModels> GetAllItemForScheme()
        {
            var result = this._FormTemplateRepo.getAllProductforScheme();
            return result;
        }
        public List<ProductsModels> GetItemForSchemeByCode(string code)
        {
            var result = this._FormTemplateRepo.getProductforSchemeByCode(code);
            return result;
        }
        public List<PartyType> GetAllDealerCode()
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            try
            {
                var response = new List<PartyType>();
                if (this._cacheManager.IsSet($"GetAllDealerCode_{userid}_{company_code}_{branch_code}"))
                {
                    var data = _cacheManager.Get<List<PartyType>>($"GetAllDealerCode_{userid}_{company_code}_{branch_code}");
                    response = data;
                }
                else
                {
                    var getDealerList = this._FormTemplateRepo.GetAllDealer();
                    this._cacheManager.Set($"GetAllDealerCode_{userid}_{company_code}_{branch_code}", getDealerList, 20);
                    response = getDealerList;
                }
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public List<PartyType> GetDealerCodeForSchemeByCode(string code)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<PartyType>();          
            try
            {

                var getDealerList = this._FormTemplateRepo.GetDealerForSchemeByCode(code);
                response = getDealerList;
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        
        [HttpPost]
        public List<SchemeDetailsModel> bindSchemeDetailGrid(DOCUMENT_SCHEME documentscheme)
        {

            var response = new List<SchemeDetailsModel>();
            var SshemeGridList = this._FormTemplateRepo.getSchemeDetailGridData(documentscheme.SchemeModel);
            response = SshemeGridList;
            return response;
        }
        [HttpPost]
        public List<SchemeDetailsModel> bindSchemeDetailForImpact(DOCUMENT_SCHEME documentscheme)
        {

            var response = new List<SchemeDetailsModel>();
            var SshemeGridList = this._FormTemplateRepo.getSchemeDetailFormImpact(documentscheme.SchemeModel);
            response = SshemeGridList;
            return response;
        }
        [HttpGet]
        public List<PartyType> GetPartyTypeByFilterAndSubCode(string filter, string subCode)
        {
            var AllPartyTypeByFilterList = this._FormTemplateRepo.GetAllPartyTypeByFilterAndSubCode(filter, subCode);
            return AllPartyTypeByFilterList;
        }
        [HttpGet]
        public string GetPTNameByCode(string code)
        {
            var result = this._FormTemplateRepo.GetPartyTypeNameByCode(code);
            return result;
        }
        public List<Document> GetDocumentCodeForSchemeByCode(string code)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<Document>();
            try
            {

                var getDocumentList = this._FormTemplateRepo.GetDocumentForSchemeByCode(code);
                response = getDocumentList;
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public List<AreaSetup> GetAreaCodeForSchemeByCode(string code)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<AreaSetup>();
            try
            {

                var getAreaList = this._FormTemplateRepo.GetAreaForSchemeByCode(code);
                response = getAreaList;
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public List<BranchModels> GetBranchCodeForSchemeByCode(string code)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<BranchModels>();
            try
            {

                var getBranchList = this._FormTemplateRepo.GetBranchForSchemeByCode(code);
                response = getBranchList;
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpGet]
        public string getCustomerCode(string customeredesc)
        {
            var result = this._FormTemplateRepo.GetcustomerCodeByName(customeredesc);
            return result;
        }

       
        [HttpGet]
        public List<CustomerTree> GetAllCustomerNodes()
        {
            var userinfo = this._workContext.CurrentUserinformation;
            //var allCustomerList = _salesRegister.CustomerListAllNodes().ToList();
            var allCustomerList = _FormTemplateRepo.CustomerListAllNodes(userinfo).ToList();
            var customerNodes = new List<CustomerTree>();

            foreach (var cust in allCustomerList)
            {
                customerNodes.Add(new CustomerTree()
                {
                    Level = cust.LEVEL,
                    customerName = cust.CUSTOMER_EDESC,
                    customerId = cust.CUSTOMER_CODE,
                    masterCustomerCode = cust.MASTER_CUSTOMER_CODE,
                    preCustomerCode = cust.PRE_CUSTOMER_CODE,
                    hasCustomers = cust.GROUP_SKU_FLAG == "G" ? true : false
                });
            }

            return customerNodes;
        }

        [HttpGet]
        public List<CustomerTree> GetAllCustomersByCustId(string custId, string level, string masterCode)
        {
            var userinfo = this._workContext.CurrentUserinformation;

            //var preCustId = _salesRegister.GetPreCustCodeByCustomerId(custId).ToString();
            //var allCustomerList = _salesRegister.GetCustomerListByCustomerCode(level,masterCode).ToList();
            var allCustomerList = _FormTemplateRepo.GetCustomerListByCustomerCode(level, masterCode, userinfo).ToList();
            var customerNodes = new List<CustomerTree>();

            foreach (var cust in allCustomerList)
            {
                customerNodes.Add(new CustomerTree()
                {
                    Level = cust.LEVEL,
                    customerName = cust.CUSTOMER_EDESC,
                    customerId = cust.CUSTOMER_CODE,
                    masterCustomerCode = cust.MASTER_CUSTOMER_CODE,
                    preCustomerCode = cust.PRE_CUSTOMER_CODE,
                    hasCustomers = cust.GROUP_SKU_FLAG == "G" ? true : false
                });
            }

            return customerNodes;
        }

        [HttpGet]
        public List<CompanyInfo> GetCompanyList()
        {
            _logErp.InfoInFile("Get CompanyInfo:  For Print");
            var response = _FormTemplateRepo.GetCompanyList();
            return response;
        }

        [HttpGet]
        public List<ChargeOnSales> GetLineItemChargeInfo(string companycode, string FormCode, string CustomerCode, string ItemCode)
        {
            _logErp.InfoInFile("Get LineItemChargeInfo:  For Calculation");
            var response = _FormTemplateRepo.GetLineItemChargeInfo(companycode,FormCode, CustomerCode, ItemCode);
            return response;
        }

        [HttpGet]
        public List<CustomerItemType> GetItemDiscountScheduleInfo(string companycode, string FormCode, string CustomerCode, string ItemCode)
        {
            _logErp.InfoInFile("Get ItemDiscountScheduleInfo:  For Calculation");
            var response = _FormTemplateRepo.GetItemDiscountScheduleInfo(companycode, FormCode, CustomerCode, ItemCode);
            return response;
        }

        [HttpGet]
        public decimal GetSecondQuantity(string companycode, string itemCode, string quantity)
        {
            _logErp.InfoInFile("Get SecondQuantityInfo:  For Print");
            var templateInsertQry = $@"select FN_SECONDUNIT('{itemCode}',{quantity},'{companycode}') as value1 from dual";
            var response = this._dbContext.SqlQuery<decimal>(templateInsertQry).FirstOrDefault();
            return response;
        }

        [HttpGet]
        public decimal GetThirdQuantity(string companycode, string itemCode, string quantity)
        {
            _logErp.InfoInFile("Get ThirdQuantityInfo:  For Print");
            var templateInsertQry = $@"select FN_THIRDUNIT('{itemCode}',{quantity},'{companycode}') as value1 from dual";
            var response = this._dbContext.SqlQuery<decimal>(templateInsertQry).FirstOrDefault();
            return response;
        }

        [HttpGet]
        public decimal GetSecondFirstQuantity(string companycode, string itemCode, string quantity)
        {
            _logErp.InfoInFile("Get SecondFirstQuantityInfo:  For Print");
            var templateInsertQry = $@"select FN_SECONDFIRST('{itemCode}',{quantity},'{companycode}') as value1 from dual";
            var response = this._dbContext.SqlQuery<decimal>(templateInsertQry).FirstOrDefault();
            return response;
        }

        [HttpGet]
        public decimal GetThirdFirstQuantity(string companycode, string itemCode, string quantity)
        {
            _logErp.InfoInFile("Get ThirdFirstQuantityInfo:  For Print");
            var templateInsertQry = $@"select FN_THIRDFIRST('{itemCode}',{quantity},'{companycode}') as value1 from dual";
            var response = this._dbContext.SqlQuery<decimal>(templateInsertQry).FirstOrDefault();
            return response;
        }

        [HttpGet]
        public decimal GetSecondThirdQuantity(string companycode, string itemCode, string quantity)
        {
            _logErp.InfoInFile("Get ThirdFirstQuantityInfo:  For Print");
            var templateInsertQry = $@"select FN_SECONDTHIRD('{itemCode}',{quantity},'{companycode}') as value1 from dual";
            var response = this._dbContext.SqlQuery<decimal>(templateInsertQry).FirstOrDefault();
            return response;
        }

        [HttpGet]
        public decimal GetThirdSecondQuantity(string companycode, string itemCode, string quantity)
        {
            _logErp.InfoInFile("Get ThirdFirstQuantityInfo:  For Print");
            var templateInsertQry = $@"select FN_THIRDSECOND('{itemCode}',{quantity},'{companycode}') as value1 from dual";
            var response = this._dbContext.SqlQuery<decimal>(templateInsertQry).FirstOrDefault();
            return response;
        }

        [HttpGet]
        public List<ChargeOnSales> GetLineItemChargeParticularInfo(string companycode, string FormCode,string ChargeCode, string CustomerCode, string ItemCode)
        {
            _logErp.InfoInFile("Get CompanyInfo:  For Print");
            var response = _FormTemplateRepo.GetLineItemChargeParticularInfo(companycode, FormCode,ChargeCode, CustomerCode, ItemCode);
            return response;
        }

        [HttpGet]
        public decimal GetFreezeRateScheduleInfo(string companycode, string FormCode, string CustomerCode, string ItemCode)
        {
            _logErp.InfoInFile("Get FreezeRateScheduleInfo:  For Print");
            var response = _FormTemplateRepo.GetFreezeRateScheduleInfo(companycode, FormCode, CustomerCode, ItemCode);
            return Convert.ToDecimal(response);
        }

        //[HttpGet]
        //public bool CheckVoucherNoReferencedOrNot(string voucherno)
        //{
        //    var result = this._FormTemplateRepo.CheckVoucherNoReferenced(voucherno);
        //    return result;
        //}

    }
    public class SalesOrderMasterModel
    {
        public string ORDER_NO { get; set; }
        public string ORDER_DATE { get; set; }
        public string MANUAL_NO { get; set; }
        public string CUSTOMER_CODE { get; set; }
        public string REMARKS { get; set; }
        public string DELIVERY_DATE { get; set; }
    }
    public class SalesOrderChildModel
    {
        public string ITEM_CODE { get; set; }
        public string MU_CODE { get; set; }
        public int QUANTITY { get; set; }
        public int UNIT_PRICE { get; set; }
        public int TOTAL_PRICE { get; set; }
        public int CALC_QUANTITY { get; set; }
        public int CALC_UNIT_PRICE { get; set; }
        public int CALC_TOTAL_PRICE { get; set; }
        public int STOCK_BLOCK_FLAG { get; set; }
    }
}
