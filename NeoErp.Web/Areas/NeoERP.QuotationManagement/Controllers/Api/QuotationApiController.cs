using NeoERP.QuotationManagement.Service.Models;
using NeoERP.QuotationManagement.Service.Interface;
using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using NeoErp.Core;
using NeoErp.Core.Caching;
using System.Data.SqlClient;
using NeoErp.Core.Models;
using NeoErp.Core.Models.Log4NetLoggin;
using System.Web.Http;
using NeoErp.Data;
using NeoErp.Core.Services.CommonSetting;
using System.Web;
using System.IO;

namespace NeoERP.QuotationManagement.Controllers.Api
{
    public class QuotationApiController : ApiController
    {
        private const string QM = "Quotation Management";
        private IQuotationRepo _quotRepo;
        private IDbContext _dbContext;
        private IWorkContext _workContext;
        private ICacheManager _cacheManager;
        private NeoErpCoreEntity _objectEntity;
        private readonly ILogErp _logErp;
        private ISettingService _settingService;
        private DefaultValueForLog _defaultValueForLog;
        public QuotationApiController(IQuotationRepo _IQuotRepo, IDbContext dbContext, IWorkContext workContext, ICacheManager cacheManager, NeoErpCoreEntity objectEntity)
        {
            this._quotRepo = _IQuotRepo;
            this._dbContext = dbContext;
            this._objectEntity = objectEntity;
            this._workContext = workContext;
            this._cacheManager = cacheManager;
            this._defaultValueForLog = new DefaultValueForLog(this._workContext);
            this._logErp = new LogErp(this, _defaultValueForLog.LogUser, _defaultValueForLog.LogCompany, _defaultValueForLog.LogBranch, _defaultValueForLog.LogTypeCode, _defaultValueForLog.LogModule, _defaultValueForLog.FormCode);
        }
        [HttpGet]
        //public List<Products> ItemDetails()
        //{
        //    List<Products> itemDeatils = _quotRepo.GetAllProducts();
        //    return itemDeatils;
        //}
        public List<Category> GetCategoryList()
        {
            List<Category> categoryList = _quotRepo.GetCategoryList();
            return categoryList;
        }

        public List<Quotation_setup> getTenderNo()
        {
            List<Quotation_setup> quotations = _quotRepo.GetQuotationId();
            return quotations;
        }
        public List<Quotation_setup> getTenderId(string tenderNo)
        {
            List<Quotation_setup> quotation = _quotRepo.GetTenderId(tenderNo);
            return quotation;
        }
        [HttpPost]
        public HttpResponseMessage deleteQuotationId(string tenderNo)
        {
            try
            {
                bool isDeleted = _quotRepo.deleteQuotationId(tenderNo);
                if (isDeleted)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Quotation deleted successfully!!", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    // Handle case where project was not posted successfully
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "Failed to delete project!!", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occurred during the operation
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpPost]
        public HttpResponseMessage SaveItemData(FormDetails model)
        {
            try
            {
                var msg = string.Empty;
                var voucherno = model.Order_No;
                decimal exchangrate = 1;
                bool insertedToMaster = false;
                bool insertToMaster = false;
                bool isPosted = false;
                var childColumn = _quotRepo.MapChildColumnWithValue(model.Child_COLUMN_VALUE);
                var masterColumn = _quotRepo.MapMasterColumnWithValue(model.Master_COLUMN_VALUE);
                var VoucherNumberGeneratedNo = string.Empty;
                List<Quotation_setup> quotations = _quotRepo.GetQuotationId();
                VoucherNumberGeneratedNo = quotations[0].TENDER_NO;
                //string newvoucherNo =
                string createdDateForEdit = string.Empty, createdByForEdit = string.Empty, voucherNoForEdit = string.Empty;

                string primarydate = string.Empty, primarycolumn = string.Empty, today = DateTime.Now.ToString("dd-MMM-yyyy"), createddatestring = "SYSDATE", todaystring = System.DateTime.Now.ToString("yyyyMMddHHmmss"), manualno = string.Empty, currencyformat = "NRS", VoucherDate = createddatestring, grandtotal = model.Grand_Total;
                if (voucherno == "undefined")
                {
                    var commonValue = new CommonFieldsForInventory
                    {
                        FormCode = model.Form_Code,
                        TableName = model.Table_Name,
                        ExchangeRate = exchangrate,
                        //CurrencyFormat = currencyformat,
                        VoucherNumber = VoucherNumberGeneratedNo,
                        NewVoucherNumber = VoucherNumberGeneratedNo,
                        TempCode = model.TempCode,
                        VoucherDate = VoucherDate,
                        Grand_Total = model.Grand_Total,
                        FormRef = model.FROM_REF,
                        ManualNumber = masterColumn.MANUAL_NO,
                        MODULE_CODE = model.MODULE_CODE
                    };
                    //bool isPosted = _quotRepo.InsertQuotationData(formData);
                    if (masterColumn != null)
                    {
                        insertedToMaster = _quotRepo.SaveColumnValue(masterColumn, commonValue);
                    }
                    if (insertedToMaster)
                    {
                        isPosted = _quotRepo.InsertQuotationData(childColumn, masterColumn, commonValue, model);

                    }
                    if (isPosted)
                    {
                        insertToMaster = _quotRepo.SaveMasterColumnValue(masterColumn, commonValue);

                    }
                    if (isPosted == true && insertedToMaster == true && insertToMaster == true)
                    {
                        msg = "INSERTED";
                    }
                }
                else {
                    bool updatedChild = false;
                    bool updatedPost = false;
                    bool updatedToMaster = false;

                    var defaultData = _quotRepo.GetMasterTransactionByVoucherNo(voucherno);
                    foreach (var def in defaultData)
                    {
                        voucherNoForEdit = def.VOUCHER_NO.ToString();
                        createdDateForEdit = "TO_DATE('" + def.CREATED_DATE.ToString() + "', 'DD-MON-YY')";
                        createdByForEdit = def.CREATED_BY.ToString().ToUpper();
                    }
                    var commonUpdateValue = new CommonFieldsForInventory
                    {
                        TableName = model.Table_Name,
                        ExchangeRate = exchangrate,
                        CurrencyFormat = currencyformat,
                        VoucherNumber = voucherno,
                        NewVoucherNumber = voucherno,
                        VoucherDate = createdDateForEdit,
                        ManualNumber = masterColumn.MANUAL_NO,
                    };
                    masterColumn.MODIFY_DATE = VoucherDate;
                    masterColumn.MODIFY_BY = _workContext.CurrentUserinformation.login_code.ToUpper();
                    masterColumn.CREATED_BY = createdByForEdit;
                    
                    if (masterColumn != null)
                    {
                        updatedToMaster = _quotRepo.UpdateColumnValue(masterColumn, commonUpdateValue);
                    }
                    if (updatedToMaster)
                    {
                        updatedChild = _quotRepo.UpdateItemData(childColumn, masterColumn, commonUpdateValue, model);
                    }
                    if (updatedChild)
                    {
                        updatedPost = _quotRepo.UpdateMasterTransaction(commonUpdateValue);
                    }
                    if (updatedPost == true && updatedToMaster == true && updatedChild == true)
                    {
                        msg = "UPDATED";
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = msg, STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = voucherno });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpGet]
        public List<Quotation_setup> ListAllTenders()
        {
            List<Quotation_setup> response = new List<Quotation_setup>();
            try
            {
                response = _quotRepo.ListAllTenders();
                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        public List<Quotation_setup> GetQuotationById(string tenderNo)
        {
            List<Quotation_setup> response = new List<Quotation_setup>();
            try
            {
                response = _quotRepo.GetQuotationById(tenderNo);
                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        [HttpPost]
        public HttpResponseMessage updateItemsById(string id)
        {
            try
            {
                bool isDeleted = _quotRepo.updateItemsById(id);
                if (isDeleted)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Product deleted successfully!!", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    // Handle case where project was not posted successfully
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "Failed to delete product!!", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occurred during the operation
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        public List<Quotation_Details> ListQuotationDetails()
        {
            List<Quotation_Details> response = new List<Quotation_Details>();
            try
            {
                response = _quotRepo.ListQuotationDetails();
                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        [HttpGet]
        public List<Quotation_Details> QuotationDetailsById(string quotationNo,string tenderNo)
        {
            List<Quotation_Details> response = new List<Quotation_Details>();
            try
            {
                response = _quotRepo.QuotationDetailsById(quotationNo, tenderNo);
                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        [HttpGet]
        public List<Quotation_Details> QuotationDetailsId(string quotationNo, string tenderNo)
        {
            List<Quotation_Details> response = new List<Quotation_Details>();
            try
            {
                response = _quotRepo.QuotationDetailsId(quotationNo, tenderNo);
                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        public List<SummaryReport> TendersItemWise()
        {
            List<SummaryReport> response = new List<SummaryReport>();
            try
            {
                response = _quotRepo.TendersItemWise();
                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        [HttpGet]
        public List<Quotation> ItemDetailsByTender(string tenderNo)
        {
            List<Quotation> response = new List<Quotation>();
            try
            {
                 response = _quotRepo.ItemDetailsTenderNo(tenderNo);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        [HttpPost]
        public HttpResponseMessage updateQuotationStatus(string quotationNo, string status)
        {
            try
            {
                bool isDeleted=false;
                if (status == "AP")
                {
                     isDeleted = _quotRepo.acceptQuotation(quotationNo, status);
                }
                else if(status=="R")
                {
                     isDeleted = _quotRepo.rejectQuotation(quotationNo, status);
                }
                if (isDeleted)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Quotation !!", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    // Handle case where project was not posted successfully
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "Quotation !!", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occurred during the operation
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpPost]
        public IHttpActionResult saveTender(Tender formData)
        {
            try
            {
                bool isPosted = _quotRepo.InsertTenderData(formData);
                if (isPosted)
                {
                    return Ok(new { success = true, message = "Tender data saved successfully." });
                }
                else
                {
                    return Ok(new { success = true, message = "Failed to save Quotation Tender." });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        [HttpGet]
        public List<Tender> TenderDetails()
        {
            List<Tender> response = new List<Tender>();
            try
            {
                response = _quotRepo.getTenderDetails();
                return response;
            }
            catch(Exception ex) {
                throw new Exception(ex.StackTrace);
            }
        }
        [HttpPost]
        public HttpResponseMessage deleteTenderId(string tenderNo)
        {
            try
            {
                bool isDeleted = _quotRepo.deleteTenderId(tenderNo);
                if (isDeleted)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "Tender No. deleted successfully!!", STATUS_CODE = (int)HttpStatusCode.OK });
                }
                else
                {
                    // Handle case where project was not posted successfully
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = "Failed to delete Tender No.!!", STATUS_CODE = (int)HttpStatusCode.InternalServerError });
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occurred during the operation
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpGet]
        public List<Tender> getTenderById(string tenderNo)
        {
            List<Tender> response = new List<Tender>();
            try
            {
                response = _quotRepo.getTenderById(tenderNo);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        public List<QuotationCount> GetQuotationCount()
        {
            List<QuotationCount> response = new List<QuotationCount>();
            try
            {
                response = _quotRepo.GetQuotationCount();
                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        public List<Quotation_Details> QuotationById(string quotationNo, string tenderNo)
        {
            List<Quotation_Details> response = new List<Quotation_Details>();
            try
            {
                response = _quotRepo.QuotationDetailsById(quotationNo, tenderNo);
                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        [HttpGet]
        public List<FormDetailSetup> GetFormDetailSetup()
        {

            var response = new List<FormDetailSetup>();
                var formDetailList = this._quotRepo.GetFormDetailSetup();
                response = formDetailList;
            return response;
        }
        public List<Products> GetMUCodeByProductId(string productId)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<Products>();
            var MUCodeByProductId = this._quotRepo.GetProductDataByProductCode(productId);
            response = MUCodeByProductId;
            return response;
        }
        [HttpPost]
        public HttpResponseMessage DeleteQuotVoucher(string voucherno)
        {
            try
            {
                string checkreferenced = _quotRepo.CheckVoucherNoReferenced(voucherno);
                if (checkreferenced != "" && checkreferenced != null && checkreferenced != "undefined")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "REFERENCED", STATUS_CODE = (int)HttpStatusCode.OK, VoucherNo = checkreferenced });
                }
                else
                {
                    bool deleteres = _quotRepo.deletevouchernoInv(voucherno);

                    return Request.CreateResponse(HttpStatusCode.OK, new { MESSAGE = "DELETED", STATUS_CODE = (int)HttpStatusCode.OK });
                }


            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { MESSAGE = ex.Message, STATUS_CODE = (int)HttpStatusCode.InternalServerError });
            }
        }
        [HttpGet]
        public List<COMMON_COLUMN> GetQuotationDetailByOrderno(string voucherNo)
        {
            var userid = _workContext.CurrentUserinformation.User_id;
            var company_code = _workContext.CurrentUserinformation.company_code;
            var branch_code = _workContext.CurrentUserinformation.branch_code;
            var response = new List<COMMON_COLUMN>();
            var SalesOrderDetailFormDetailByFormCodeAndOrderNo = this._quotRepo.GetQuestOrderFormDetail(voucherNo);
            response = SalesOrderDetailFormDetailByFormCodeAndOrderNo;
            return response;
        }
    }
}
