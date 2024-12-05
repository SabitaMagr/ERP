﻿using NeoErp.Core;
using NeoErp.Core.Caching;
using NeoErp.Core.Models;
using NeoErp.Core.Models.Log4NetLoggin;
using NeoERP.DocumentTemplate.Service.Interface;
using NeoERP.DocumentTemplate.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
//using NeoErp.Models.Common;
//using System.Data.Entity;
//using System.Data.OracleClient;
using System.Text;
using System.Threading.Tasks;

namespace NeoERP.DocumentTemplate.Service.Services
{
    public class SaveDocTemplateSalesModule : ISaveDocTemplateSalesModule
    {

        private IWorkContext _workContext;
        private NeoErpCoreEntity _objectEntity;
        private ICacheManager _cacheManager;
        private ILogErp _logErp;
        private DefaultValueForLog _defaultValueForLog;

        public SaveDocTemplateSalesModule(ICacheManager cacheManager, IWorkContext workContext, NeoErpCoreEntity coreEntity)
        {
            this._workContext = workContext;
            this._objectEntity = coreEntity;
            this._cacheManager = cacheManager;
            this._defaultValueForLog = new DefaultValueForLog(this._workContext);
            this._logErp = new LogErp(this, _defaultValueForLog.LogUser, _defaultValueForLog.LogCompany, _defaultValueForLog.LogBranch, _defaultValueForLog.LogTypeCode, _defaultValueForLog.LogModule, _defaultValueForLog.FormCode);
        }

        #region ADD ORDER METHOD

        public ResponseMessage SaveGenericTableFormData(SalesFieldsForSavingFormData salesFields, CommonFieldForSales commonFieldForSales)
        {
            try
            {
                if ((salesFields.MasterFields.Count > 0) && (salesFields.ChildFields.Count > 0))
                {
                    foreach (var sc in salesFields.ChildFields)
                    {

                    }
                    var mailColumnSet = salesFields.MasterFields.Keys;
                    var childColumnSet = salesFields.ChildFields[0].Keys;
                    var masterValueSet = salesFields.MasterFields.Values;
                    var childValueSet = salesFields.ChildFields[0].Values;
                    //var querySalesorder = $@"Insert into SA_SALES_ORDER (ORDER_NO, ORDER_DATE, CUSTOMER_CODE, SERIAL_NO, ITEM_CODE, MU_CODE, QUANTITY, UNIT_PRICE, TOTAL_PRICE, CALC_QUANTITY, CALC_UNIT_PRICE, CALC_TOTAL_PRICE, 
                    //  REMARKS, FORM_CODE, COMPANY_CODE, BRANCH_CODE, CREATED_BY, CREATED_DATE, DELETED_FLAG, DELIVERY_DATE, CURRENCY_CODE, EXCHANGE_RATE,TRACKING_NO, STOCK_BLOCK_FLAG, SESSION_ROWID, MODIFY_BY, DIVISION_CODE) 
                    // Values ('{salesChildOrder.ORDER_NO}',TO_DATE('{salesChildOrder.ORDER_DATE}','DD-MON-YYYY hh24:mi:ss'), '{salesChildOrder.CUSTOMER_CODE}', {salesChildOrder.SERIAL_NO}, 
                    //'{salesChildOrder.ITEM_CODE}', '{salesChildOrder.MU_CODE}', {salesChildOrder.QUANTITY}, {salesChildOrder.UNIT_PRICE}, {salesChildOrder.TOTAL_PRICE}, 
                    // {salesChildOrder.CALC_QUANTITY}, {salesChildOrder.CALC_UNIT_PRICE}, {salesChildOrder.CALC_TOTAL_PRICE}, '{salesChildOrder.REMARKS}','{salesChildOrder.FORM_CODE}', '{salesChildOrder.COMPANY_CODE}', '{salesChildOrder.BRANCH_CODE}', '{salesChildOrder.CREATED_BY}', SYSDATE, 
                    //'N', TO_DATE('{salesChildOrder.DELIVERY_DATE}','DD-MON-YYYY hh24:mi:ss'), '{salesChildOrder.CURRENCY_CODE}', {salesChildOrder.EXCHANGE_RATE},'{salesChildOrder.TRACKING_NO}', '{salesChildOrder.STOCK_BLOCK_FLAG}', '{salesChildOrder.SESSION_ROWID}', '', '{salesChildOrder.DIVISION_CODE}')";
                    //var InsertedData = _objectEntity.ExecuteSqlCommand(querySalesorder);
                    //serialNo++;
                }
                if (salesFields.CustomFields.Count > 0)
                {

                }
                if (salesFields.ChildFields.Count > 0)
                {

                }
                if (salesFields.InvItemChargeFields.Count > 0)
                {

                }
                if (salesFields.ShippingFields.Count > 0)
                {

                }

                return new ResponseMessage();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }

        public ResponseMessage SaveSalesChalanFormData(SalesChalanDetailModel salesChalanDetailModel, CommonFieldForSales commonFieldForSales, NeoErpCoreEntity dbcontext = null)
        {
            _logErp.WarnInDB("Save sales chalan form data started===");

            try
            {
                bool insertedToChild = false;
                bool insertedToMaster = false;
                bool insertedToPosted = false;
                bool insertedToCustom = false;
                bool insertedToCharges = false;
                bool insertedToShipping = false;
                bool insertedToInvItem = false;
                bool insertedToBatch = false;

                insertedToChild = SaveSalesChalanValues(salesChalanDetailModel.ChildChalanTransaction, salesChalanDetailModel.MasterChalanTransaction, commonFieldForSales.OrderNumber, commonFieldForSales.FormCode, dbcontext);

                _logErp.InfoInFile("child value for sales chalana saved successfully:" + insertedToChild);
                if (commonFieldForSales.FormRef)
                {
                    var salesOrderDetailModel = new SalesOrderDetailModel();
                    
                    foreach(var model in salesChalanDetailModel.RefenceModel)
                    {
                        var childData = salesChalanDetailModel.ChildChalanTransaction.Where(x => x.ITEM_CODE == model.ITEM_CODE).FirstOrDefault();
                        model.SERIAL_NO = childData.SERIAL_NO.ToString();
                        model.Qty = childData.QUANTITY;
                        model.calc_qty = childData.CALC_QUANTITY;
                        model.cal_unit_price = childData.CALC_UNIT_PRICE;
                        model.cal_total_price = childData.CALC_TOTAL_PRICE;
                        model.Total_price = childData.TOTAL_PRICE;
                    }
                    salesOrderDetailModel.RefenceModel = salesChalanDetailModel.RefenceModel;
                    salesOrderDetailModel.TotalChild = salesChalanDetailModel.ChildChalanTransaction.Count;
                    GetFormReference(salesOrderDetailModel, commonFieldForSales, dbcontext);
                }

                if (insertedToChild)
                {
                    commonFieldForSales.Division_code = salesChalanDetailModel.MasterChalanTransaction.DIVISION_CODE;
                    insertedToMaster = SaveMasterTransactionValue(commonFieldForSales, dbcontext);
                    _logErp.InfoInFile("master value for sales chalana saved successfully:" + insertedToChild);
                }
               
                if (salesChalanDetailModel.CustomOrderTransaction != null)
                {
                    if (salesChalanDetailModel.CustomOrderTransaction.Count > 0)
                    {
                        insertedToCustom = SaveCostomColumnValue(salesChalanDetailModel.CustomOrderTransaction, commonFieldForSales, dbcontext);
                        _logErp.InfoInFile("Custom order value for sales chalana saved successfully:" + insertedToChild);
                    }
                    
                }

                if (salesChalanDetailModel.ChargeTransaction != null)
                {
                    if (salesChalanDetailModel.ChargeTransaction.Count > 0)
                    {
                        insertedToCharges = SaveChargeColumnValue(salesChalanDetailModel.ChargeTransaction, commonFieldForSales, dbcontext);
                        _logErp.InfoInFile("Charge transaction for sales chalan saved successfullly");
                    }
                   
                }

                if (salesChalanDetailModel.BatchTransaction != null )
                {
                    if (salesChalanDetailModel.BatchTransaction.Count > 0)
                    {
                        insertedToBatch = SaveBatchTransactionValues(salesChalanDetailModel.BatchTransaction, commonFieldForSales, dbcontext);
                    }
                }
                if (salesChalanDetailModel.InvItemChargeTransaction != null)
                {
                    insertedToInvItem = true;
                    _logErp.InfoInFile("Charge transaction for sales chalan saved successfullly");
                }
                //if (insertedToMaster)
                //{

                //    insertedToPosted = SavePostedTransactionValue(commonFieldForSales, dbcontext);
                //    _logErp.InfoInFile("Posted value for sales chalan saved successfully:" + insertedToPosted);
                //}
                if (salesChalanDetailModel.ShippingTransaction != null)
                {
                    insertedToShipping = SaveShippingDetailsColumnValue(salesChalanDetailModel.ShippingTransaction, commonFieldForSales, dbcontext);
                    _logErp.InfoInFile("Shipping details for sales chalan saved successfullly");
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


                if (commonFieldForSales.OrderNumber != "undefined" && insertedToChild == true && insertedToMaster == true && commonFieldForSales.SaveFlag == "0")
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 200.ToString(),
                        Message = "INSERTED",
                        VoucherNo = commonFieldForSales.OrderNumber,
                        SessionNo = commonFieldForSales.NewVoucherNumber,
                        VoucherDate = salesChalanDetailModel.MasterChalanTransaction.ORDER_DATE,
                        FormCode = commonFieldForSales.FormCode,
                        SaveFlag = 0.ToString()
                    };
                    return responseMessage;
                }
                else if (commonFieldForSales.OrderNumber != "undefined" && insertedToChild == true && insertedToMaster == true && commonFieldForSales.SaveFlag == "1")
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 200.ToString(),
                        Message = "INSERTEDANDCONTINUE",
                        VoucherNo = commonFieldForSales.OrderNumber,
                        SessionNo = commonFieldForSales.NewVoucherNumber,
                        VoucherDate = salesChalanDetailModel.MasterChalanTransaction.ORDER_DATE,
                        FormCode = commonFieldForSales.FormCode,
                        SaveFlag = 1.ToString()
                    };
                    return responseMessage;
                }
                else if (commonFieldForSales.OrderNumber != "undefined" && insertedToChild == true && insertedToMaster == true && commonFieldForSales.SaveFlag == "3")
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 200.ToString(),
                        Message = "SAVEANDPRINT",
                        VoucherNo = commonFieldForSales.OrderNumber,
                        SessionNo = commonFieldForSales.NewVoucherNumber,
                        VoucherDate = salesChalanDetailModel.MasterChalanTransaction.ORDER_DATE,
                        FormCode = commonFieldForSales.FormCode,
                        SaveFlag = 3.ToString()
                    };
                    return responseMessage;
                }else if(commonFieldForSales.OrderNumber !="undefined" && insertedToChild==true && insertedToMaster==true && commonFieldForSales.SaveFlag=="4")
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 200.ToString(),
                        Message = "UPDATEDANDPRINT",
                        VoucherNo = commonFieldForSales.OrderNumber,
                        SessionNo = commonFieldForSales.NewVoucherNumber,
                        VoucherDate = salesChalanDetailModel.MasterChalanTransaction.ORDER_DATE,
                        FormCode = commonFieldForSales.FormCode,
                        SaveFlag = 4.ToString()
                    };
                    return responseMessage;
                }
                else
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 500.ToString(),
                        Message = "ERROR",
                        SaveFlag = "ErrorOnSave"
                    };
                    return responseMessage;
                }


            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while saving sales chalan form data : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }

        public ResponseMessage SaveSalesInvoiceFormData(SalesInvoiceDetailModel salesInvoiceDetailModel, CommonFieldForSales commonFieldForSales, NeoErpCoreEntity dbcontext = null)
        {
            try
            {
                bool insertedToChild = false;
                bool insertedToPosted = true;
                bool insertedToMaster = false;
                bool insertedToCustom = false;
                bool insertedToCharges = false;
                bool insertedToShipping = false;
                bool insertedToInvItem = false;
                bool insertedToBatch = false;
                insertedToChild = SaveSalesInvoiceValues(salesInvoiceDetailModel.ChildInvoiceTransaction, salesInvoiceDetailModel.MasterInvoiceTransaction, commonFieldForSales.OrderNumber, commonFieldForSales.FormCode);
                if (commonFieldForSales.FormRef)
                {
                  
                    var salesOrderDetailModel = new SalesOrderDetailModel();

                    foreach (var model in salesInvoiceDetailModel.RefenceModel)
                    {
                        var childData = salesInvoiceDetailModel.ChildInvoiceTransaction.Where(x => x.ITEM_CODE == model.ITEM_CODE).FirstOrDefault();
                        model.SERIAL_NO = childData.SERIAL_NO.ToString();
                        model.Qty = childData.QUANTITY;
                        model.calc_qty = childData.CALC_QUANTITY;
                        model.cal_unit_price = childData.CALC_UNIT_PRICE;
                        model.cal_total_price = childData.CALC_TOTAL_PRICE;
                        model.Total_price = childData.TOTAL_PRICE;
                        //model.EXCISE_AMOUNT = childData.ED;
                        //model.DISCOUNT_AMOUNT = childData.SD;
                        //model.VAT_AMOUNT = childData.VT;
                        //model.TAXABLE_AMOUNT = childData.TA;
                        //model.NET_AMOUNT = childData.NA;
                    }
                    salesOrderDetailModel.RefenceModel = salesInvoiceDetailModel.RefenceModel;
                    salesOrderDetailModel.TotalChild = salesInvoiceDetailModel.ChildInvoiceTransaction.Count;
                    GetFormReference(salesOrderDetailModel, commonFieldForSales, dbcontext);
                   
                }

                if (insertedToChild)
                {
                    commonFieldForSales.Division_code = salesInvoiceDetailModel.MasterInvoiceTransaction.DIVISION_CODE;
                    insertedToMaster = SaveMasterTransactionValue(commonFieldForSales, dbcontext);
                }
               

                if (salesInvoiceDetailModel.CustomOrderTransaction != null)
                {
                    insertedToCustom = SaveCostomColumnValue(salesInvoiceDetailModel.CustomOrderTransaction, commonFieldForSales, dbcontext);
                }

                if (salesInvoiceDetailModel.ChargeTransaction != null)
                {
                    insertedToCharges = SaveChargeColumnValue(salesInvoiceDetailModel.ChargeTransaction, commonFieldForSales, dbcontext);
                }
               
                if (salesInvoiceDetailModel.BatchTransaction != null )
                {
                    if (salesInvoiceDetailModel.BatchTransaction.Count > 0)
                    {
                        insertedToBatch = SaveBatchTransactionValues(salesInvoiceDetailModel.BatchTransaction, commonFieldForSales, dbcontext);
                    }
                }
                if (salesInvoiceDetailModel.InvItemChargeTransaction != null)
                {
                    insertedToInvItem = true;
                }
                //if (insertedToMaster)
                //{

                //    insertedToPosted = SavePostedTransactionValue(commonFieldForSales, dbcontext);
                //}
                if (salesInvoiceDetailModel.ShippingTransaction != null)
                {
                    insertedToShipping = SaveShippingDetailsColumnValue(salesInvoiceDetailModel.ShippingTransaction, commonFieldForSales, dbcontext);
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


                if (commonFieldForSales.OrderNumber != "undefined" && insertedToChild == true && insertedToMaster == true && commonFieldForSales.SaveFlag == "0")
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 200.ToString(),
                        Message = "INSERTED",
                        VoucherNo = commonFieldForSales.OrderNumber,
                        SessionNo = commonFieldForSales.NewVoucherNumber,
                        VoucherDate = salesInvoiceDetailModel.MasterInvoiceTransaction.SALES_DATE,
                        FormCode = commonFieldForSales.FormCode,
                        SaveFlag = 0.ToString()
                    };
                    return responseMessage;
                }
                else if (commonFieldForSales.OrderNumber != "undefined" && insertedToChild == true && insertedToMaster == true && commonFieldForSales.SaveFlag == "1")
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 200.ToString(),
                        Message = "INSERTEDANDCONTINUE",
                        VoucherNo = commonFieldForSales.OrderNumber,
                        SessionNo = commonFieldForSales.NewVoucherNumber,
                        VoucherDate = salesInvoiceDetailModel.MasterInvoiceTransaction.SALES_DATE,
                        FormCode = commonFieldForSales.FormCode,
                        SaveFlag = 1.ToString()
                    };
                    return responseMessage;
                }
                else if (commonFieldForSales.OrderNumber != "undefined" && insertedToChild == true && insertedToMaster == true && commonFieldForSales.SaveFlag == "3")
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 200.ToString(),
                        Message = "SAVEANDPRINT",
                        VoucherNo = commonFieldForSales.OrderNumber,
                        SessionNo = commonFieldForSales.NewVoucherNumber,
                        VoucherDate = salesInvoiceDetailModel.MasterInvoiceTransaction.SALES_DATE,
                        FormCode = commonFieldForSales.FormCode,
                        SaveFlag = 3.ToString()
                    };
                    return responseMessage;
                }
                else
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 500.ToString(),
                        Message = "ERROR",
                        SaveFlag = "ErrorOnSave"
                    };
                    return responseMessage;
                }


            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while saving sales invoice form data : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }

        public ResponseMessage SaveSalesOrderFormData(SalesOrderDetailModel salesOrderDetailModel, CommonFieldForSales commonFieldForSales, NeoErpCoreEntity dbcontext = null)
        {
            try
            {
                bool insertedToChild = false;
                bool insertedToMaster = false;
                bool insertedToCustom = false;
                bool insertedToCharges = false;
                bool insertedToBatch = false;
                bool insertedToShipping = false;
                bool insertedToInvItem = false;
                insertedToChild = SaveSalesOrderValues(salesOrderDetailModel.ChildTransaction, salesOrderDetailModel.MasterTransaction, commonFieldForSales.OrderNumber, commonFieldForSales.FormCode, dbcontext);
                if (commonFieldForSales.FormRef)
                {
                    GetFormReference(salesOrderDetailModel, commonFieldForSales, dbcontext);
                }

                if (insertedToChild)
                {
                    commonFieldForSales.Division_code = salesOrderDetailModel.MasterTransaction.DIVISION_CODE;
                    insertedToMaster = SaveMasterTransactionValue(commonFieldForSales, dbcontext);
                }

                if (salesOrderDetailModel.CustomOrderTransaction != null)
                {
                    if (salesOrderDetailModel.CustomOrderTransaction.Count > 0)
                    {
                        insertedToCustom = SaveCostomColumnValue(salesOrderDetailModel.CustomOrderTransaction, commonFieldForSales, dbcontext);
                    }
                    
                }

                if (salesOrderDetailModel.ChargeTransaction != null)
                {
                    if (salesOrderDetailModel.ChargeTransaction.Count > 0)
                    {
                        insertedToCharges = SaveChargeColumnValue(salesOrderDetailModel.ChargeTransaction, commonFieldForSales, dbcontext);
                    }
                   
                }
                if (salesOrderDetailModel.BatchTransaction != null)
                {
                    if (salesOrderDetailModel.BatchTransaction.Count > 0)
                    {
                        insertedToBatch = SaveBatchTransactionValues(salesOrderDetailModel.BatchTransaction, commonFieldForSales, dbcontext);
                    }
                }
                if (salesOrderDetailModel.InvItemChargeTransaction != null)
                {
                    insertedToInvItem = true;
                }

                if (salesOrderDetailModel.ShippingTransaction != null)
                {
                    insertedToShipping = SaveShippingDetailsColumnValue(salesOrderDetailModel.ShippingTransaction, commonFieldForSales, dbcontext);
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


                if (commonFieldForSales.OrderNumber != "undefined" && insertedToChild == true && insertedToMaster == true && commonFieldForSales.SaveFlag == "0")
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 200.ToString(),
                        Message = "INSERTED",
                        VoucherNo = commonFieldForSales.OrderNumber,
                        SessionNo = commonFieldForSales.NewVoucherNumber,
                        VoucherDate = salesOrderDetailModel.MasterTransaction.ORDER_DATE,
                        FormCode = commonFieldForSales.FormCode,
                        SaveFlag = 0.ToString()
                    };
                    return responseMessage;
                }
                else if (commonFieldForSales.OrderNumber != "undefined" && insertedToChild == true && insertedToMaster == true && commonFieldForSales.SaveFlag == "1")
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 200.ToString(),
                        Message = "INSERTEDANDCONTINUE",
                        VoucherNo = commonFieldForSales.OrderNumber,
                        SessionNo = commonFieldForSales.NewVoucherNumber,
                        VoucherDate = salesOrderDetailModel.MasterTransaction.ORDER_DATE,
                        FormCode = commonFieldForSales.FormCode,
                        SaveFlag = 1.ToString()
                    };
                    return responseMessage;
                }
                else if (commonFieldForSales.OrderNumber != "undefined" && insertedToChild == true && insertedToMaster == true && commonFieldForSales.SaveFlag == "3")
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 200.ToString(),
                        Message = "SAVEANDPRINT",
                        VoucherNo = commonFieldForSales.OrderNumber,
                        SessionNo = commonFieldForSales.NewVoucherNumber,
                        VoucherDate = salesOrderDetailModel.MasterTransaction.ORDER_DATE,
                        FormCode = commonFieldForSales.FormCode,
                        SaveFlag = 3.ToString()
                    };
                    return responseMessage;
                }
                else if (commonFieldForSales.OrderNumber != "undefined" && insertedToChild == true && insertedToMaster == true && commonFieldForSales.SaveFlag == "4")
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 200.ToString(),
                        Message = "UPDATEDANDPRINT",
                        VoucherNo = commonFieldForSales.OrderNumber,
                        SessionNo = commonFieldForSales.NewVoucherNumber,
                        VoucherDate = salesOrderDetailModel.MasterTransaction.ORDER_DATE,
                        FormCode = commonFieldForSales.FormCode,
                        SaveFlag = 4.ToString()
                    };
                    return responseMessage;
                }
                else
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 500.ToString(),
                        Message = "ERROR",
                        SaveFlag = "ErrorOnSave"
                    };
                    return responseMessage;
                }


            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while saving sales order form data: " + ex.StackTrace);
                throw new Exception(ex.Message);
            }

        }


        public ResponseMessage SaveSalesReturnFormData(SalesReturnDetailModel salesReturnDetailModel, CommonFieldForSales commonFieldForSales, NeoErpCoreEntity dbcontext = null)
        {
            try
            {
                bool insertedToChild = false;
                bool insertedToPosted = true;
                bool insertedToMaster = false;
                bool insertedToCustom = false;
                bool insertedToCharges = false;
                bool insertedToShipping = false;
                bool insertedToInvItem = false;
                bool insertedToBatch = false;
                insertedToChild = SaveSalesReturnValues(salesReturnDetailModel.ChildReturnTransaction, salesReturnDetailModel.MasterReturnTransaction, commonFieldForSales.OrderNumber, commonFieldForSales.FormCode);
                if (commonFieldForSales.FormRef)
                {
                 
                    var salesOrderDetailModel = new SalesOrderDetailModel();

                    foreach (var model in salesReturnDetailModel.RefenceModel)
                    {
                        var childData = salesReturnDetailModel.ChildReturnTransaction.Where(x => x.ITEM_CODE == model.ITEM_CODE).FirstOrDefault();
                        model.SERIAL_NO = childData.SERIAL_NO.ToString();
                        model.Qty = childData.QUANTITY;
                        model.calc_qty = childData.CALC_QUANTITY;
                        model.cal_unit_price = childData.CALC_UNIT_PRICE;
                        model.cal_total_price = childData.CALC_TOTAL_PRICE;
                        model.Total_price = childData.TOTAL_PRICE;
                    }
                    salesOrderDetailModel.RefenceModel = salesReturnDetailModel.RefenceModel;
                    salesOrderDetailModel.TotalChild = salesReturnDetailModel.ChildReturnTransaction.Count;
                    GetFormReference(salesOrderDetailModel, commonFieldForSales, dbcontext);
                }

                if (insertedToChild)
                {
                    insertedToMaster = SaveMasterTransactionValue(commonFieldForSales, dbcontext);
                }
               
                if (salesReturnDetailModel.CustomOrderTransaction != null)
                {
                    insertedToCustom = SaveCostomColumnValue(salesReturnDetailModel.CustomOrderTransaction, commonFieldForSales, dbcontext);
                    _logErp.WarnInDB("Custome column for " + commonFieldForSales.TableName + " saved successfully ");
                }

                if (salesReturnDetailModel.ChargeTransaction != null)
                {
                    insertedToCharges = SaveChargeColumnValue(salesReturnDetailModel.ChargeTransaction, commonFieldForSales, dbcontext);
                    _logErp.WarnInDB("Charge column for " + commonFieldForSales.TableName + " saved successfully ");
                }
                if (salesReturnDetailModel.BatchTransaction != null)
                {
                    
                    if(salesReturnDetailModel.BatchTransaction.Count>0)
                    {
                        insertedToBatch = SaveBatchTransactionValues(salesReturnDetailModel.BatchTransaction, commonFieldForSales, dbcontext);
                    }
                  
                }
                if (salesReturnDetailModel.InvItemChargeTransaction != null)
                {
                    insertedToInvItem = true;
                }
                //if (insertedToMaster)
                //{

                //    insertedToPosted = SavePostedTransactionValue(commonFieldForSales, dbcontext);
                //}
                if (salesReturnDetailModel.ShippingTransaction != null)
                {
                    insertedToShipping = SaveShippingDetailsColumnValue(salesReturnDetailModel.ShippingTransaction, commonFieldForSales, dbcontext);
                    _logErp.WarnInDB("Shipping details for " + commonFieldForSales.TableName + " saved successfully ");
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


                if (commonFieldForSales.OrderNumber != "undefined" && insertedToChild == true && insertedToMaster == true && commonFieldForSales.SaveFlag == "0")
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 200.ToString(),
                        Message = "INSERTED",
                        VoucherNo = commonFieldForSales.OrderNumber,
                        SessionNo = commonFieldForSales.NewVoucherNumber,
                        VoucherDate = salesReturnDetailModel.MasterReturnTransaction.RETURN_DATE,
                        FormCode = commonFieldForSales.FormCode,
                        SaveFlag = 0.ToString()
                    };
                    return responseMessage;
                }
                else if (commonFieldForSales.OrderNumber != "undefined" && insertedToChild == true && insertedToMaster == true && commonFieldForSales.SaveFlag == "1")
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 200.ToString(),
                        Message = "INSERTEDANDCONTINUE",
                        VoucherNo = commonFieldForSales.OrderNumber,
                        SessionNo = commonFieldForSales.NewVoucherNumber,
                        VoucherDate = salesReturnDetailModel.MasterReturnTransaction.RETURN_DATE,
                        FormCode = commonFieldForSales.FormCode,
                        SaveFlag = 1.ToString()
                    };
                    return responseMessage;
                }
                else if (commonFieldForSales.OrderNumber != "undefined" && insertedToChild == true && insertedToMaster == true && commonFieldForSales.SaveFlag == "3")
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 200.ToString(),
                        Message = "SAVEANDPRINT",
                        VoucherNo = commonFieldForSales.OrderNumber,
                        SessionNo = commonFieldForSales.NewVoucherNumber,
                        VoucherDate = salesReturnDetailModel.MasterReturnTransaction.RETURN_DATE,
                        FormCode = commonFieldForSales.FormCode,
                        SaveFlag = 3.ToString()
                    };
                    return responseMessage;
                }
                else if (commonFieldForSales.OrderNumber != "undefined" && insertedToChild == true && insertedToMaster == true && commonFieldForSales.SaveFlag == "4")
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 200.ToString(),
                        Message = "UPDATEDANDPRINT",
                        VoucherNo = commonFieldForSales.OrderNumber,
                        SessionNo = commonFieldForSales.NewVoucherNumber,
                        VoucherDate = salesReturnDetailModel.MasterReturnTransaction.RETURN_DATE,
                        FormCode = commonFieldForSales.FormCode,
                        SaveFlag = 4.ToString()
                    };
                    return responseMessage;
                }
                else
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 500.ToString(),
                        Message = "ERROR",
                        SaveFlag = "ErrorOnSave"
                    };
                    return responseMessage;
                }


            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while saving sales return form data : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }



        #endregion



        #region UPDATE ORDER METHOD

        public ResponseMessage UpdateGenericTableFormData()
        {
            throw new NotImplementedException();
        }

        public ResponseMessage UpdateSalesChalanFormData(SalesChalanDetailModel salesChalanDetailModel, CommonFieldForSales commonFieldForSales, NeoErpCoreEntity dbcontext = null)
        {
            try
            {

                var isDeleted = DeleteSalesOrderBeforeUpdate(commonFieldForSales.TableName, commonFieldForSales.PrimaryColumn, commonFieldForSales.OrderNumber);
                if (commonFieldForSales.FormRef)
                {
                    // GetFormReference(salesChalanDetailModel);
                }

                if (salesChalanDetailModel.CustomOrderTransaction != null || salesChalanDetailModel.CustomOrderTransaction.Count > 0)
                {
                    DeleteCustomColumnBeforeUpdate(salesChalanDetailModel.CustomOrderTransaction, commonFieldForSales.OrderNumber);
                }

                if (salesChalanDetailModel.ChargeTransaction != null || salesChalanDetailModel.ChargeTransaction.Count > 0)
                {
                    DeleteChargeColumnBeforeUpdate(salesChalanDetailModel.ChargeTransaction, commonFieldForSales.OrderNumber);
                }

                if (salesChalanDetailModel.InvItemChargeTransaction != null)
                {
                    DeleteInvItemColumnBeforeUpdate("");
                }

                if (salesChalanDetailModel.ShippingTransaction != null)
                {
                    DeleteShippingDetailsBeforeUpdate(salesChalanDetailModel.ShippingTransaction, commonFieldForSales.OrderNumber);
                }
                if (salesChalanDetailModel.BatchTransaction != null)
                {
                    DeletSerialdataBeforeUpdate(salesChalanDetailModel.BatchTransaction, commonFieldForSales.OrderNumber);
                }
                var response = new ResponseMessage();
                //  var isDeleted = DeleteSalesOrderBeforeUpdate(commonFieldForSales.TableName, commonFieldForSales.PrimaryColumn, commonFieldForSales.OrderNumber);
                if (isDeleted) response = SaveSalesChalanFormData(salesChalanDetailModel, commonFieldForSales, dbcontext);
                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }

        public ResponseMessage UpdateSalesInvoiceFormData(SalesInvoiceDetailModel salesInvoiceDetailModel, CommonFieldForSales commonFieldForSales, NeoErpCoreEntity dbcontext = null)
        {
            try
            {
                bool updatedToChild = false;
                bool insertedToMaster = false;
                bool updatedToCustom = false;
                bool updatedToCharges = false;
                bool updatedShippingDetails = false;
                bool insertedToInvItem = false;
                updatedToChild = UpdateSalesInvoiceValues(salesInvoiceDetailModel.ChildInvoiceTransaction, salesInvoiceDetailModel.MasterInvoiceTransaction, commonFieldForSales.OrderNumber, commonFieldForSales.FormCode);
                if (commonFieldForSales.FormRef)
                {
                    // GetFormReference(salesChalanDetailModel);
                }

                if (updatedToChild)
                {
                    commonFieldForSales.Division_code = salesInvoiceDetailModel.MasterInvoiceTransaction.DIVISION_CODE;
                    insertedToMaster = SaveMasterTransactionValue(commonFieldForSales, dbcontext);
                }

                if (salesInvoiceDetailModel.CustomOrderTransaction != null)
                {
                    bool checkCustomCountFlag = CheckAlreadyInsertedCustomValues(commonFieldForSales.OrderNumber, commonFieldForSales.FormCode);
                    if (checkCustomCountFlag == true)
                    {
                        updatedToCustom = UpdateCostomColumnValue(salesInvoiceDetailModel.CustomOrderTransaction, commonFieldForSales, dbcontext);
                    }
                    else
                    {
                        updatedToCustom = SaveCostomColumnValue(salesInvoiceDetailModel.CustomOrderTransaction, commonFieldForSales, dbcontext);
                    }
                }




                if (salesInvoiceDetailModel.ChargeTransaction != null)
                {
                    DeleteChargeColumnBeforeUpdate(salesInvoiceDetailModel.ChargeTransaction, commonFieldForSales.OrderNumber);
                    updatedToCharges = SaveChargeColumnValue(salesInvoiceDetailModel.ChargeTransaction, commonFieldForSales, dbcontext);
                    //bool checkChargeCountFlag = CheckAlreadyInsertedChargeValues(commonFieldForSales.OrderNumber, commonFieldForSales.FormCode);
                    //if (checkChargeCountFlag == true)
                    //{
                    //    updatedToCharges = UpdateChargeColumnValue(salesInvoiceDetailModel.ChargeTransaction, commonFieldForSales, dbcontext);
                    //}
                    //else {
                    //    updatedToCharges = SaveChargeColumnValue(salesInvoiceDetailModel.ChargeTransaction, commonFieldForSales, dbcontext);
                    //}

                }

                if (salesInvoiceDetailModel.InvItemChargeTransaction != null)
                {
                    insertedToInvItem = true;
                }

                if (salesInvoiceDetailModel.ShippingTransaction != null)
                {
                    bool checkShippingCountFlag = CheckAlreadyInsertedShippingValues(commonFieldForSales.OrderNumber, commonFieldForSales.FormCode);
                    if (checkShippingCountFlag == true)
                    {
                        updatedShippingDetails = UpdateShippingDetailsColumnValue(salesInvoiceDetailModel.ShippingTransaction, commonFieldForSales, dbcontext);
                    }
                    else
                    {
                        updatedShippingDetails = SaveShippingDetailsColumnValue(salesInvoiceDetailModel.ShippingTransaction, commonFieldForSales, dbcontext);
                    }

                }
                if (salesInvoiceDetailModel.BatchTransaction != null)
                {
                    DeletSerialdataBeforeUpdate(salesInvoiceDetailModel.BatchTransaction, commonFieldForSales.OrderNumber);
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


                if (commonFieldForSales.OrderNumber != "undefined" && updatedToChild == true && insertedToMaster == true && commonFieldForSales.SaveFlag == "0")
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 200.ToString(),
                        Message = "UPDATED",
                        VoucherNo = commonFieldForSales.OrderNumber,
                        SessionNo = commonFieldForSales.NewVoucherNumber,
                        VoucherDate = salesInvoiceDetailModel.MasterInvoiceTransaction.SALES_DATE,
                        FormCode = commonFieldForSales.FormCode,
                        SaveFlag = 0.ToString()
                    };
                    return responseMessage;
                }
                else if (commonFieldForSales.OrderNumber != "undefined" && updatedToChild == true && insertedToMaster == true && commonFieldForSales.SaveFlag == "4")
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 200.ToString(),
                        Message = "UPDATEDANDPRINT",
                        VoucherNo = commonFieldForSales.OrderNumber,
                        SessionNo = commonFieldForSales.NewVoucherNumber,
                        VoucherDate = salesInvoiceDetailModel.MasterInvoiceTransaction.SALES_DATE,
                        FormCode = commonFieldForSales.FormCode,
                        SaveFlag = 4.ToString()
                    };
                    return responseMessage;
                }

                else
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 500.ToString(),
                        Message = "ERROR",
                        SaveFlag = "ErrorOnSave"
                    };
                    return responseMessage;
                }


            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }

        public ResponseMessage UpdateSalesOrderFormData(SalesOrderDetailModel salesOrderDetailModel, CommonFieldForSales commonFieldForSales)
        {
            try
            {
                var isDeleted = DeleteSalesOrderBeforeUpdate(commonFieldForSales.TableName, commonFieldForSales.PrimaryColumn, commonFieldForSales.OrderNumber);
                if (commonFieldForSales.FormRef)
                {
                    // GetFormReference(salesChalanDetailModel);
                }

                if (salesOrderDetailModel.CustomOrderTransaction != null || salesOrderDetailModel.CustomOrderTransaction.Count > 0)
                {
                    DeleteCustomColumnBeforeUpdate(salesOrderDetailModel.CustomOrderTransaction, commonFieldForSales.OrderNumber);
                }

                if (salesOrderDetailModel.ChargeTransaction != null || salesOrderDetailModel.ChargeTransaction.Count > 0)
                {
                    DeleteChargeColumnBeforeUpdate(salesOrderDetailModel.ChargeTransaction, commonFieldForSales.OrderNumber);
                }

                if (salesOrderDetailModel.InvItemChargeTransaction != null)
                {
                    DeleteInvItemColumnBeforeUpdate("");
                }

                if (salesOrderDetailModel.ShippingTransaction != null)
                {
                    DeleteShippingDetailsBeforeUpdate(salesOrderDetailModel.ShippingTransaction, commonFieldForSales.OrderNumber);
                }
                if (salesOrderDetailModel.BatchTransaction != null)
                {
                    DeletSerialdataBeforeUpdate(salesOrderDetailModel.BatchTransaction, commonFieldForSales.OrderNumber);
                }
                var response = new ResponseMessage();
                if (isDeleted) response = SaveSalesOrderFormData(salesOrderDetailModel, commonFieldForSales, _objectEntity);
                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }

        public ResponseMessage UpdateSalesReturnFormData(SalesReturnDetailModel salesReturnDetailModel, CommonFieldForSales commonFieldForSales, NeoErpCoreEntity dbcontext = null)
        {

            try
            {
                bool updatedToChild = false;
                bool insertedToMaster = false;
                bool updatedToCustom = false;
                bool updatedToCharges = false;
                bool updatedShippingDetails = false;
                bool insertedToInvItem = false;
                updatedToChild = UpdateSalesReturnValues(salesReturnDetailModel.ChildReturnTransaction, salesReturnDetailModel.MasterReturnTransaction, commonFieldForSales.OrderNumber, commonFieldForSales.FormCode);
                if (commonFieldForSales.FormRef)
                {
                    // GetFormReference(salesChalanDetailModel);
                }

                if (updatedToChild)
                {
                    commonFieldForSales.Division_code = salesReturnDetailModel.MasterReturnTransaction.DIVISION_CODE;
                    insertedToMaster = SaveMasterTransactionValue(commonFieldForSales, dbcontext);
                }

                if (salesReturnDetailModel.CustomOrderTransaction != null)
                {

                    bool checkCustomCountFlagRet = CheckAlreadyInsertedCustomValues(commonFieldForSales.OrderNumber, commonFieldForSales.FormCode);
                    if (checkCustomCountFlagRet == true)
                    {
                        updatedToCustom = UpdateCostomColumnValue(salesReturnDetailModel.CustomOrderTransaction, commonFieldForSales, dbcontext);
                    }
                    else
                    {
                        updatedToCustom = SaveCostomColumnValue(salesReturnDetailModel.CustomOrderTransaction, commonFieldForSales, dbcontext);
                    }
                }

                if (salesReturnDetailModel.ChargeTransaction != null)
                {
                    DeleteChargeColumnBeforeUpdate(salesReturnDetailModel.ChargeTransaction, commonFieldForSales.OrderNumber);
                    updatedToCharges = SaveChargeColumnValue(salesReturnDetailModel.ChargeTransaction, commonFieldForSales, dbcontext);
                    //bool checkChargeCountFlagRet = CheckAlreadyInsertedChargeValues(commonFieldForSales.OrderNumber, commonFieldForSales.FormCode);
                    //if (checkChargeCountFlagRet == true)
                    //{
                    //    updatedToCharges = UpdateChargeColumnValue(salesReturnDetailModel.ChargeTransaction, commonFieldForSales, dbcontext);
                    //}
                    //else
                    //{
                    //    updatedToCharges = SaveChargeColumnValue(salesReturnDetailModel.ChargeTransaction, commonFieldForSales, dbcontext);
                    //}

                }

                if (salesReturnDetailModel.InvItemChargeTransaction != null)
                {
                    insertedToInvItem = true;
                }

                if (salesReturnDetailModel.ShippingTransaction != null)
                {
                    bool checkShippingCountFlagRet = CheckAlreadyInsertedShippingValues(commonFieldForSales.OrderNumber, commonFieldForSales.FormCode);

                    if (checkShippingCountFlagRet == true)
                    {
                        updatedShippingDetails = UpdateShippingDetailsColumnValue(salesReturnDetailModel.ShippingTransaction, commonFieldForSales, dbcontext);
                    }
                    else
                    {
                        updatedShippingDetails = SaveShippingDetailsColumnValue(salesReturnDetailModel.ShippingTransaction, commonFieldForSales, dbcontext);
                    }


                }
                if (salesReturnDetailModel.BatchTransaction != null)
                {
                    DeletSerialdataBeforeUpdate(salesReturnDetailModel.BatchTransaction, commonFieldForSales.OrderNumber);
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


                if (commonFieldForSales.OrderNumber != "undefined" && updatedToChild == true && insertedToMaster == true && commonFieldForSales.SaveFlag == "0")
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 200.ToString(),
                        Message = "UPDATED",
                        VoucherNo = commonFieldForSales.OrderNumber,
                        SessionNo = commonFieldForSales.NewVoucherNumber,
                        VoucherDate = salesReturnDetailModel.MasterReturnTransaction.RETURN_DATE,
                        FormCode = commonFieldForSales.FormCode,
                        SaveFlag = 0.ToString()
                    };
                    return responseMessage;
                }
                else if (commonFieldForSales.OrderNumber != "undefined" && updatedToChild == true && insertedToMaster == true && commonFieldForSales.SaveFlag == "4")
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 200.ToString(),
                        Message = "UPDATEDANDPRINT",
                        VoucherNo = commonFieldForSales.OrderNumber,
                        SessionNo = commonFieldForSales.NewVoucherNumber,
                        VoucherDate = salesReturnDetailModel.MasterReturnTransaction.RETURN_DATE,
                        FormCode = commonFieldForSales.FormCode,
                        SaveFlag = 4.ToString()
                    };
                    return responseMessage;
                }

                else
                {
                    var responseMessage = new ResponseMessage()
                    {
                        StatusCode = 500.ToString(),
                        Message = "ERROR",
                        SaveFlag = "ErrorOnSave"
                    };
                    return responseMessage;
                }


            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }


        #endregion



        #region PIRVATE METHOD TO ADD SALES ORDER


        private bool SaveSalesOrderValues(List<SalesOrderDetail> childValue, SalesOrderDetail masterValue, string orderNumber, string formCode, NeoErpCoreEntity dbcontext = null)
        {
            try
            {
                _logErp.WarnInDB("Method to save sales order data started===============");
                int serialNo = 1;             
                bool insertedToChild = false;
                SalesOrderDetail salesChildOrder = new SalesOrderDetail();
                foreach (var cCol in childValue)
                {
                    salesChildOrder.ORDER_NO = orderNumber;
                    salesChildOrder.ORDER_DATE = string.IsNullOrEmpty(cCol.ORDER_DATE) ? masterValue.ORDER_DATE : cCol.ORDER_DATE;
                    salesChildOrder.CUSTOMER_CODE = string.IsNullOrEmpty(cCol.CUSTOMER_CODE) ? masterValue.CUSTOMER_CODE : cCol.CUSTOMER_CODE;
                    salesChildOrder.SERIAL_NO = cCol.SERIAL_NO > 0 ? cCol.SERIAL_NO : serialNo;
                    salesChildOrder.ITEM_CODE = string.IsNullOrEmpty(cCol.ITEM_CODE) ? "" : cCol.ITEM_CODE;
                    salesChildOrder.MU_CODE = string.IsNullOrEmpty(cCol.MU_CODE) ? "" : cCol.MU_CODE;
                    salesChildOrder.QUANTITY = cCol.QUANTITY > 0 ? cCol.QUANTITY : cCol.CALC_QUANTITY > 0 ? cCol.CALC_QUANTITY : 0;
                    //salesChildOrder.UNIT_PRICE = cCol.UNIT_PRICE > 0 ? cCol.UNIT_PRICE : cCol.CALC_UNIT_PRICE > 0 ? cCol.CALC_UNIT_PRICE : 1;
                    salesChildOrder.UNIT_PRICE = cCol.UNIT_PRICE;
                    salesChildOrder.TOTAL_PRICE = cCol.TOTAL_PRICE;
                    salesChildOrder.CALC_QUANTITY = cCol.CALC_QUANTITY > 0 ? cCol.CALC_QUANTITY : cCol.QUANTITY > 0 ? cCol.QUANTITY : 0;
                    //salesChildOrder.CALC_UNIT_PRICE = cCol.CALC_UNIT_PRICE > 0 ? cCol.CALC_UNIT_PRICE : cCol.UNIT_PRICE > 0 ? cCol.UNIT_PRICE : 1;
                    salesChildOrder.CALC_UNIT_PRICE = cCol.CALC_UNIT_PRICE;
                    salesChildOrder.CALC_TOTAL_PRICE = (cCol.CALC_TOTAL_PRICE > 0) ? cCol.CALC_TOTAL_PRICE : (cCol.TOTAL_PRICE > 0) ? cCol.TOTAL_PRICE : 0;
                    salesChildOrder.REMARKS = string.IsNullOrEmpty(cCol.REMARKS) ? masterValue.REMARKS : cCol.REMARKS;
                    salesChildOrder.FORM_CODE = string.IsNullOrEmpty(cCol.FORM_CODE) ? formCode : cCol.FORM_CODE;
                    salesChildOrder.COMPANY_CODE = _workContext.CurrentUserinformation.company_code;
                    salesChildOrder.BRANCH_CODE = _workContext.CurrentUserinformation.branch_code;
                    salesChildOrder.CREATED_BY = _workContext.CurrentUserinformation.login_code;
                    salesChildOrder.CREATED_DATE = DateTime.Now;
                    salesChildOrder.DELETED_FLAG = "N";
                    salesChildOrder.DELIVERY_DATE = string.IsNullOrEmpty(cCol.DELIVERY_DATE) ? masterValue.DELIVERY_DATE : cCol.DELIVERY_DATE;
                    //salesChildOrder.CURRENCY_CODE = cCol.CURRENCY_CODE;
                    //salesChildOrder.EXCHANGE_RATE = cCol.EXCHANGE_RATE;
                    salesChildOrder.DELIVERY_DATE = string.IsNullOrEmpty(cCol.DELIVERY_DATE) ? masterValue.DELIVERY_DATE : cCol.DELIVERY_DATE;
                    salesChildOrder.CURRENCY_CODE = string.IsNullOrEmpty(cCol.CURRENCY_CODE) ? "NRS" : cCol.CURRENCY_CODE;
                    salesChildOrder.EXCHANGE_RATE = cCol.EXCHANGE_RATE > 0 ? cCol.QUANTITY : 1;
                    salesChildOrder.TRACKING_NO = cCol.TRACKING_NO;
                    salesChildOrder.STOCK_BLOCK_FLAG = cCol.STOCK_BLOCK_FLAG;
                    salesChildOrder.SESSION_ROWID = cCol.SESSION_ROWID;
                    salesChildOrder.MODIFY_BY = cCol.MODIFY_BY;
                    salesChildOrder.DIVISION_CODE = string.IsNullOrEmpty(cCol.DIVISION_CODE) ? masterValue.DIVISION_CODE : cCol.DIVISION_CODE;
                    salesChildOrder.PARTY_TYPE_CODE = string.IsNullOrEmpty(cCol.PARTY_TYPE_CODE) ? masterValue.PARTY_TYPE_CODE : cCol.PARTY_TYPE_CODE;
                    salesChildOrder.SALES_TYPE_CODE = string.IsNullOrEmpty(cCol.SALES_TYPE_CODE) ? masterValue.SALES_TYPE_CODE : cCol.SALES_TYPE_CODE;
                    salesChildOrder.MODIFY_DATE = DateTime.Now;
                    salesChildOrder.EMPLOYEE_CODE = string.IsNullOrEmpty(cCol.EMPLOYEE_CODE) ? masterValue.EMPLOYEE_CODE : cCol.EMPLOYEE_CODE;
                    salesChildOrder.MANUAL_NO = masterValue.MANUAL_NO;
                    // salesChildOrder.EMPLOYEE_CODE = masterValue.EMPLOYEE_CODE;
                    //salesChildOrder.SHIPPING_ADDRESS = string.IsNullOrEmpty(masterValue.SHIPPING_ADDRESS)?"": masterValue.SHIPPING_ADDRESS.Contains("'") ? masterValue.SHIPPING_ADDRESS.Replace("'", "' || '''' || '") : masterValue.SHIPPING_ADDRESS; 
                    if (masterValue.SHIPPING_ADDRESS != null)
                    {
                        salesChildOrder.SHIPPING_ADDRESS = masterValue.SHIPPING_ADDRESS.Contains("'") ? masterValue.SHIPPING_ADDRESS.Replace("'", "' || '''' || '") : masterValue.SHIPPING_ADDRESS;
                    }
                    else
                    {
                        salesChildOrder.SHIPPING_ADDRESS = "";
                    }
                    salesChildOrder.SHIPPING_CONTACT_NO = masterValue.SHIPPING_CONTACT_NO;
                    salesChildOrder.AREA_CODE = masterValue.AREA_CODE;
                    salesChildOrder.AGENT_CODE = masterValue.AGENT_CODE;
                    salesChildOrder.PRIORITY_CODE = string.IsNullOrEmpty(cCol.PRIORITY_CODE) ? masterValue.PRIORITY_CODE : cCol.PRIORITY_CODE;
                    salesChildOrder.LINE_ITEM_DISCOUNT = masterValue.LINE_ITEM_DISCOUNT;
                    salesChildOrder.SECTOR_CODE = masterValue.SECTOR_CODE;
                    //salesChildOrder.SECOND_QUANTITY = cCol.SECOND_QUANTITY > 0 ? cCol.SECOND_QUANTITY : 1;
                    // salesChildOrder



                    //var querySalesorder = $@"Insert into SA_SALES_ORDER ({column}) Values('{value}')";
                    //var InsertedData = _objectEntity.ExecuteSqlCommand(querySalesorder);

                    var querySalesorder = $@"Insert into SA_SALES_ORDER (ORDER_NO, ORDER_DATE, CUSTOMER_CODE, SERIAL_NO, ITEM_CODE, MU_CODE, QUANTITY, UNIT_PRICE, TOTAL_PRICE, CALC_QUANTITY, CALC_UNIT_PRICE, CALC_TOTAL_PRICE, 
                      REMARKS, FORM_CODE, COMPANY_CODE, BRANCH_CODE, CREATED_BY, CREATED_DATE, DELETED_FLAG, DELIVERY_DATE, CURRENCY_CODE, 
                    EXCHANGE_RATE,TRACKING_NO, STOCK_BLOCK_FLAG, SESSION_ROWID, MODIFY_BY, DIVISION_CODE,MANUAL_NO,SHIPPING_ADDRESS,SHIPPING_CONTACT_NO,
                    AREA_CODE,SALES_TYPE_CODE,EMPLOYEE_CODE,PARTY_TYPE_CODE,PRIORITY_CODE,LINE_ITEM_DISCOUNT,AGENT_CODE,SECTOR_CODE,SECOND_QUANTITY,THIRD_QUANTITY) 
                     Values ('{salesChildOrder.ORDER_NO}',TO_DATE('{salesChildOrder.ORDER_DATE}','DD-MON-YYYY hh24:mi:ss'), '{salesChildOrder.CUSTOMER_CODE}', {salesChildOrder.SERIAL_NO}, 
                    '{salesChildOrder.ITEM_CODE}', '{salesChildOrder.MU_CODE}', {salesChildOrder.QUANTITY}, {salesChildOrder.UNIT_PRICE}, {salesChildOrder.TOTAL_PRICE}, 
                     {salesChildOrder.CALC_QUANTITY}, {salesChildOrder.CALC_UNIT_PRICE}, {salesChildOrder.CALC_TOTAL_PRICE}, '{salesChildOrder.REMARKS}','{salesChildOrder.FORM_CODE}', '{salesChildOrder.COMPANY_CODE}', '{salesChildOrder.BRANCH_CODE}', '{salesChildOrder.CREATED_BY}', SYSDATE, 
                    'N', TO_DATE('{salesChildOrder.DELIVERY_DATE}','DD-MON-YYYY hh24:mi:ss'), '{salesChildOrder.CURRENCY_CODE}', {salesChildOrder.EXCHANGE_RATE},'{salesChildOrder.TRACKING_NO}', '{salesChildOrder.STOCK_BLOCK_FLAG}', '{salesChildOrder.SESSION_ROWID}', '', '{salesChildOrder.DIVISION_CODE}','{salesChildOrder.MANUAL_NO}','{salesChildOrder.SHIPPING_ADDRESS}','{salesChildOrder.SHIPPING_CONTACT_NO}',
                    '{salesChildOrder.AREA_CODE}','{salesChildOrder.SALES_TYPE_CODE}','{salesChildOrder.EMPLOYEE_CODE}','{salesChildOrder.PARTY_TYPE_CODE}','{salesChildOrder.PRIORITY_CODE}','{salesChildOrder.LINE_ITEM_DISCOUNT}','{salesChildOrder.AGENT_CODE}','{salesChildOrder.SECTOR_CODE}','{cCol.SECOND_QUANTITY}','{cCol.THIRD_QUANTITY}')";
                    if (dbcontext == null)
                    {
                        var InsertedData = _objectEntity.ExecuteSqlCommand(querySalesorder);
                        _logErp.WarnInDB("Sales order data saved successfullly by : " + _workContext.CurrentUserinformation.login_code);
                    }
                    else
                    {
                        var InsertedData = dbcontext.ExecuteSqlCommand(querySalesorder);
                        _logErp.WarnInDB("Sales order data saved successfullly by : " + _workContext.CurrentUserinformation.login_code);
                    }
                    serialNo++;
                }


                insertedToChild = true;
                return insertedToChild;


            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while saving sales order data : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }

        private void GetFormReference(SalesOrderDetailModel model, CommonFieldForSales commonFieldForSales, NeoErpCoreEntity dbcontext = null)
        {
            int serialNo = 0;
            foreach (var Ref in model.RefenceModel)
            {
                if (model.TotalChild >= serialNo)
                    serialNo = serialNo + 1;

                if (Ref.TABLE_NAME == "SA_SALES_ORDER"||Ref.TABLE_NAME== "SA_LOADING_SLIP_DETAIL")
                {
                    var salesOrderRef = $@"select REMARKS,ORDER_NO,TO_CHAR(ORDER_DATE) ORDER_DATE ,ORDER_DATE VOUCHER_DATE,MANUAL_NO,CUSTOMER_CODE,SERIAL_NO,ITEM_CODE,MU_CODE,QUANTITY,UNIT_PRICE,TOTAL_PRICE,CALC_QUANTITY,CALC_UNIT_PRICE,CALC_TOTAL_PRICE,FORM_CODE,COMPANY_CODE,BRANCH_CODE,EMPLOYEE_CODE from SA_SALES_ORDER where ITEM_CODE='{Ref.ITEM_CODE}'  AND order_no='{Ref.VOUCHER_NO}'";
                    var RefSalesOrder = dbcontext.SqlQuery<SalesOrderDetail>(salesOrderRef).FirstOrDefault();
                    if (RefSalesOrder != null)
                    {
                        var maxRefNoQry = $@"SELECT TO_CHAR(SYSDATE,'RRRRMMDD')||'.'||LPAD(MAX(REGEXP_SUBSTR(TRANSACTION_NO,'[^.]+', 1, 2))+1,11,'0')TRANSACTION_NO FROM REFERENCE_DETAIL";
                        var maxRefNo = dbcontext.SqlQuery<string>(maxRefNoQry).FirstOrDefault();
                        var refInsQuery = $@"INSERT INTO REFERENCE_DETAIL (TRANSACTION_NO,VOUCHER_NO,FORM_CODE,COMPANY_CODE,SERIAL_NO,REFERENCE_NO,REFERENCE_FORM_CODE,
                                                    REFERENCE_ITEM_CODE,
                                                     REFERENCE_QUANTITY,REFERENCE_MU_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,REFERENCE_UNIT_PRICE,REFERENCE_TOTAL_PRICE,REFERENCE_CALC_UNIT_PRICE,REFERENCE_CALC_TOTAL_PRICE, REFERENCE_REMARKS,REFERENCE_DATE,BRANCH_CODE,REFERENCE_BRANCH_CODE,REFERENCE_SERIAL_NO,SYN_ROWID,BATCH_NO,REFERENCE_BATCH_NO,VOUCHER_DATE) 
                                                   VALUES('{maxRefNo}','{commonFieldForSales.NewOrderNumber}','{commonFieldForSales.FormCode}','{_workContext.CurrentUserinformation.company_code}','{serialNo}','{RefSalesOrder.ORDER_NO}','{RefSalesOrder.FORM_CODE}','{RefSalesOrder.ITEM_CODE}',
                                                    '{Ref.calc_qty}','{RefSalesOrder.MU_CODE}','{_workContext.CurrentUserinformation.login_code}',SYSDATE,
                                                          'N','{Ref.cal_unit_price}','{Ref.Total_price}','{Ref.cal_unit_price}','{Ref.cal_total_price}','{RefSalesOrder.REMARKS}',SYSDATE,'{_workContext.CurrentUserinformation.branch_code}','{RefSalesOrder.BRANCH_CODE}','{RefSalesOrder.SERIAL_NO}','','','',TO_DATE('{RefSalesOrder.ORDER_DATE}','DD-MON-YYYY hh24:mi:ss'))";

                        dbcontext.ExecuteSqlCommand(refInsQuery);
                        _logErp.WarnInDB("Reference for sales order saved successfully by : " + _workContext.CurrentUserinformation.login_code);
                    }
                }
                else if (Ref.TABLE_NAME == "SA_SALES_CHALAN")
                {
                    var salesOrderRef = $@" select REMARKS,CHALAN_NO ORDER_NO, TO_CHAR(CHALAN_DATE) ORDER_DATE ,CHALAN_DATE VOUCHER_DATE,MANUAL_NO,CUSTOMER_CODE,SERIAL_NO,ITEM_CODE,MU_CODE,QUANTITY,UNIT_PRICE,TOTAL_PRICE,CALC_QUANTITY,CALC_UNIT_PRICE
                                                          ,CALC_TOTAL_PRICE,FORM_CODE,COMPANY_CODE,BRANCH_CODE,EMPLOYEE_CODE from SA_SALES_CHALAN where ITEM_CODE='{Ref.ITEM_CODE}'  AND CHALAN_NO='{Ref.VOUCHER_NO}'";
                    var RefSalesOrder = dbcontext.SqlQuery<SalesOrderDetail>(salesOrderRef).FirstOrDefault();
                    if (RefSalesOrder != null)
                    {
                        var maxRefNoQry = $@"SELECT TO_CHAR(SYSDATE,'RRRRMMDD')||'.'||LPAD(MAX(REGEXP_SUBSTR(TRANSACTION_NO,'[^.]+', 1, 2))+1,11,'0')TRANSACTION_NO FROM REFERENCE_DETAIL";
                        var maxRefNo = dbcontext.SqlQuery<string>(maxRefNoQry).FirstOrDefault();
                        var refInsQuery = $@"INSERT INTO REFERENCE_DETAIL (TRANSACTION_NO,VOUCHER_NO,FORM_CODE,COMPANY_CODE,SERIAL_NO,REFERENCE_NO,REFERENCE_FORM_CODE,
                                                    REFERENCE_ITEM_CODE,
                                                     REFERENCE_QUANTITY,REFERENCE_MU_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,REFERENCE_UNIT_PRICE,REFERENCE_TOTAL_PRICE,REFERENCE_CALC_UNIT_PRICE,REFERENCE_CALC_TOTAL_PRICE, REFERENCE_REMARKS,REFERENCE_DATE,BRANCH_CODE,REFERENCE_BRANCH_CODE,REFERENCE_SERIAL_NO,SYN_ROWID,BATCH_NO,REFERENCE_BATCH_NO,VOUCHER_DATE) 
                                                   VALUES('{maxRefNo}','{commonFieldForSales.NewOrderNumber}','{commonFieldForSales.FormCode}','{_workContext.CurrentUserinformation.company_code}','{serialNo}','{RefSalesOrder.ORDER_NO}','{RefSalesOrder.FORM_CODE}','{RefSalesOrder.ITEM_CODE}',
                                                    '{Ref.calc_qty}','{RefSalesOrder.MU_CODE}','{_workContext.CurrentUserinformation.login_code}',SYSDATE,
                                                          'N','{Ref.cal_unit_price}','{Ref.Total_price}','{Ref.cal_unit_price}','{Ref.cal_total_price}','{RefSalesOrder.REMARKS}',SYSDATE,'{_workContext.CurrentUserinformation.branch_code}','{RefSalesOrder.BRANCH_CODE}','{RefSalesOrder.SERIAL_NO}','','','',TO_DATE('{RefSalesOrder.ORDER_DATE}','DD-MON-YYYY hh24:mi:ss'))";

                        dbcontext.ExecuteSqlCommand(refInsQuery);
                        _logErp.WarnInDB("Reference for sales chalan saved successfully by : " + _workContext.CurrentUserinformation.login_code);
                    }
                }
                else if (Ref.TABLE_NAME == "SA_SALES_INVOICE")
                {
                    var salesOrderRef = $@" select REMARKS,SALES_NO ORDER_NO, TO_CHAR(SALES_DATE) ORDER_DATE ,SALES_DATE VOUCHER_DATE,MANUAL_NO,CUSTOMER_CODE,SERIAL_NO,ITEM_CODE,MU_CODE,QUANTITY,UNIT_PRICE,TOTAL_PRICE,CALC_QUANTITY,CALC_UNIT_PRICE
                                                          ,CALC_TOTAL_PRICE,FORM_CODE,COMPANY_CODE,BRANCH_CODE,EMPLOYEE_CODE from SA_SALES_INVOICE where ITEM_CODE='{Ref.ITEM_CODE}'  AND SALES_NO='{Ref.VOUCHER_NO}'";
                    var RefSalesOrder = dbcontext.SqlQuery<SalesOrderDetail>(salesOrderRef).FirstOrDefault();
                    if (RefSalesOrder != null)
                    {
                        var maxRefNoQry = $@"SELECT TO_CHAR(SYSDATE,'RRRRMMDD')||'.'||LPAD(MAX(REGEXP_SUBSTR(TRANSACTION_NO,'[^.]+', 1, 2))+1,11,'0')TRANSACTION_NO FROM REFERENCE_DETAIL";
                        var maxRefNo = dbcontext.SqlQuery<string>(maxRefNoQry).FirstOrDefault();
                        var refInsQuery = $@"INSERT INTO REFERENCE_DETAIL (TRANSACTION_NO,VOUCHER_NO,FORM_CODE,COMPANY_CODE,SERIAL_NO,REFERENCE_NO,REFERENCE_FORM_CODE,
                                                    REFERENCE_ITEM_CODE,
                                                     REFERENCE_QUANTITY,REFERENCE_MU_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,REFERENCE_UNIT_PRICE,REFERENCE_TOTAL_PRICE,REFERENCE_CALC_UNIT_PRICE,REFERENCE_CALC_TOTAL_PRICE, REFERENCE_REMARKS,REFERENCE_DATE,BRANCH_CODE,REFERENCE_BRANCH_CODE,REFERENCE_SERIAL_NO,SYN_ROWID,BATCH_NO,REFERENCE_BATCH_NO,VOUCHER_DATE) 
                                                   VALUES('{maxRefNo}','{commonFieldForSales.NewOrderNumber}','{commonFieldForSales.FormCode}','{_workContext.CurrentUserinformation.company_code}','{serialNo}','{RefSalesOrder.ORDER_NO}','{RefSalesOrder.FORM_CODE}','{RefSalesOrder.ITEM_CODE}',
                                                    '{Ref.calc_qty}','{RefSalesOrder.MU_CODE}','{_workContext.CurrentUserinformation.login_code}',SYSDATE,
                                                          'N','{Ref.cal_unit_price}','{Ref.Total_price}','{Ref.cal_unit_price}','{Ref.cal_total_price}','{RefSalesOrder.REMARKS}',SYSDATE,'{_workContext.CurrentUserinformation.branch_code}','{RefSalesOrder.BRANCH_CODE}','{RefSalesOrder.SERIAL_NO}','','','',TO_DATE('{RefSalesOrder.ORDER_DATE}','DD-MON-YYYY hh24:mi:ss'))";

                        dbcontext.ExecuteSqlCommand(refInsQuery);
                        _logErp.WarnInDB("Reference for sales chalan saved successfully by : " + _workContext.CurrentUserinformation.login_code);
                    }
                }
            }

            //var refInsQry = $@"SELECT * FROM {commonFieldForSales.TableName} WHERE {commonFieldForSales.PrimaryColumn}='{commonFieldForSales.VoucherDate}'";
            //var refResult = dbcontext.SqlQuery<COMMON_COLUMN>(refInsQry).ToList();
            //string voucherNo = "";
            //if (refResult.Count > 0)
            //{
            //    var srNo = 1;
            //    foreach (var it in refResult)
            //    {
            //        foreach (var item in model.ReferenceTransaction)
            //        {
            //            if (it.ITEM_CODE == "itemCode" /*item.ITEM_CODE*/)
            //            {
            //                var maxRefNoQry = $@"SELECT TO_CHAR(SYSDATE,'RRRRMMDD')||'.'||LPAD(MAX(REGEXP_SUBSTR(TRANSACTION_NO,'[^.]+', 1, 2))+1,11,'0')TRANSACTION_NO FROM REFERENCE_DETAIL";
            //                var maxRefNo = dbcontext.SqlQuery<string>(maxRefNoQry).FirstOrDefault();
            //                var getRefFormCodeQry = $@"SELECT REF_FORM_CODE FROM FORM_SETUP WHERE FORM_CODE ='{commonFieldForSales.FormCode}' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'";
            //                var REF_FORM_CODE = dbcontext.SqlQuery<string>(getRefFormCodeQry).FirstOrDefault();
            //                var refInsQuery = $@"INSERT INTO REFERENCE_DETAIL (TRANSACTION_NO,VOUCHER_NO,FORM_CODE,COMPANY_CODE,SERIAL_NO,REFERENCE_NO,REFERENCE_FORM_CODE,REFERENCE_ITEM_CODE,
            //                                         REFERENCE_QUANTITY,REFERENCE_MU_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,REFERENCE_UNIT_PRICE,REFERENCE_TOTAL_PRICE,REFERENCE_CALC_UNIT_PRICE,REFERENCE_CALC_TOTAL_PRICE,                                                        REFERENCE_REMARKS,REFERENCE_DATE,BRANCH_CODE,REFERENCE_BRANCH_CODE,REFERENCE_SERIAL_NO,SYN_ROWID,BATCH_NO,REFERENCE_BATCH_NO,VOUCHER_DATE) 
            //                                       VALUES('{maxRefNo}','{commonFieldForSales.VoucherDate}','{commonFieldForSales.FormCode}','{_workContext.CurrentUserinformation.company_code}','{it.SERIAL_NO}','{/*item.VOUCHER_NO*/voucherNo}','{REF_FORM_CODE}','{it.ITEM_CODE}','{it.QUANTITY}','{it.MU_CODE}','{_workContext.CurrentUserinformation.login_code}',SYSDATE,
            //                                              'N','{it.UNIT_PRICE}','{it.TOTAL_PRICE}','{it.CALC_UNIT_PRICE}','{it.CALC_TOTAL_PRICE}','{it.REMARKS}',SYSDATE,'{_workContext.CurrentUserinformation.branch_code}','{_workContext.CurrentUserinformation.branch_code}','{srNo}','','','',SYSDATE)";

            //                dbcontext.ExecuteSqlCommand(refInsQuery);
            //            }
            //        }
            //        srNo++;
            //    }
            //}
        }

        private bool SaveMasterTransactionValue(/*SalesOrderDetailModel masterValue,*/ CommonFieldForSales commonFieldForSales, NeoErpCoreEntity dbcontext = null)
        {
            try
            {
                bool insertedToMaster = false;

                var defaultData = GetMasterTransactionByOrderNo(commonFieldForSales.OrderNumber);
                _logErp.WarnInDB(defaultData.Count() + " master records has been found while saving sales order==================== ");
                string defaultCol = "MODIFY_BY,MODIFY_DATE", createdDateForEdit = "", createdByForEdit = "", voucherNoForEdit = "";
                var sessionRowIDForedit = 0;
                if (defaultData.Count() > 0)
                {
                    foreach (var defData in defaultData)
                    {
                        voucherNoForEdit = defData.VOUCHER_NO.ToString();
                        createdByForEdit = "TO_DATE('" + defData.CREATED_DATE.ToString() + "','MM-DD-YYYY hh12:mi:ss pm')";
                        createdByForEdit = defData.CREATED_BY.ToString().ToUpper();
                        //sessionRowIDForedit = Convert.ToInt32(defData.SESSION_ROWID);
                    }

                    if (commonFieldForSales.SaveFlag == "4")
                    {
                        string query = $@"UPDATE MASTER_TRANSACTION SET VOUCHER_AMOUNT='{ commonFieldForSales.GrandTotal}',VOUCHER_DATE = {commonFieldForSales.VoucherDate}, MODIFY_BY = '{_workContext.CurrentUserinformation.login_code}',REFERENCE_NO='{commonFieldForSales.ManualNumber}' , MODIFY_DATE = SYSDATE,CURRENCY_CODE='{commonFieldForSales.CurrencyFormat}',EXCHANGE_RATE={commonFieldForSales.ExchangeRate},PRINT_COUNT={1} where VOUCHER_NO='{commonFieldForSales.OrderNumber}' and COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}'";
                        var rowCount = dbcontext.ExecuteSqlCommand(query);
                    }
                    else
                    {
                        string query = $@"UPDATE MASTER_TRANSACTION SET VOUCHER_AMOUNT='{ commonFieldForSales.GrandTotal}',VOUCHER_DATE = {commonFieldForSales.VoucherDate}, MODIFY_BY = '{this._workContext.CurrentUserinformation.login_code}',REFERENCE_NO='{commonFieldForSales.ManualNumber}' , MODIFY_DATE = SYSDATE,CURRENCY_CODE='{commonFieldForSales.CurrencyFormat}',EXCHANGE_RATE={commonFieldForSales.ExchangeRate} where VOUCHER_NO='{commonFieldForSales.OrderNumber}' and COMPANY_CODE='{this._workContext.CurrentUserinformation.company_code}'";
                        var rowCount = dbcontext.ExecuteSqlCommand(query);
                    }

                    insertedToMaster = true;
                    _logErp.WarnInDB("Master record for " + commonFieldForSales.TableName + "saved successfully by : " + _workContext.CurrentUserinformation.login_code);
                    return insertedToMaster;
                }
                if (commonFieldForSales.TableName == "SA_SALES_ORDER")
                {
                    if (commonFieldForSales.SaveFlag == "3")
                    {

                        string insertmasterQuery = $@"INSERT INTO MASTER_TRANSACTION(VOUCHER_NO,VOUCHER_AMOUNT,FORM_CODE,
   COMPANY_CODE,BRANCH_CODE,CREATED_BY,DELETED_FLAG,CURRENCY_CODE,CREATED_DATE,VOUCHER_DATE,SYN_ROWID,EXCHANGE_RATE,
   REFERENCE_NO,PRINT_COUNT,division_code) VALUES('{commonFieldForSales.OrderNumber}','{commonFieldForSales.GrandTotal}',
   '{commonFieldForSales.FormCode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',
   '{_workContext.CurrentUserinformation.login_code}','N','{commonFieldForSales.CurrencyFormat}',SYSDATE,
   {commonFieldForSales.VoucherDate},'{commonFieldForSales.SYN_ROWID}',
   {commonFieldForSales.ExchangeRate},'{commonFieldForSales.ManualNumber}',1,'{commonFieldForSales.Division_code}')";

                        dbcontext.ExecuteSqlCommand(insertmasterQuery);
                        _logErp.WarnInDB("Master record for" + commonFieldForSales.TableName + " saved successfully by : " + _workContext.CurrentUserinformation.login_code);
                        //AutoCVP(commonFieldForSales, dbcontext);
                        insertedToMaster = true;
                    }
                    else
                    {

                        string insertmasterQuery = $@"INSERT INTO MASTER_TRANSACTION(VOUCHER_NO,VOUCHER_AMOUNT,FORM_CODE,
   COMPANY_CODE,BRANCH_CODE,CREATED_BY,DELETED_FLAG,CURRENCY_CODE,CREATED_DATE,VOUCHER_DATE,SYN_ROWID,EXCHANGE_RATE,
   REFERENCE_NO,division_code) VALUES('{commonFieldForSales.OrderNumber}','{commonFieldForSales.GrandTotal}',
   '{commonFieldForSales.FormCode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',
   '{_workContext.CurrentUserinformation.login_code}','N','{commonFieldForSales.CurrencyFormat}',SYSDATE,
   {commonFieldForSales.VoucherDate},'{commonFieldForSales.NewVoucherNumber}',
   {commonFieldForSales.ExchangeRate},'{commonFieldForSales.ManualNumber}','{commonFieldForSales.Division_code}')";
                        dbcontext.ExecuteSqlCommand(insertmasterQuery);
                        _logErp.WarnInDB("Master record for " + commonFieldForSales.TableName + " saved successfully by : " + _workContext.CurrentUserinformation.login_code);
                        //AutoCVP(commonFieldForSales, dbcontext);
                        insertedToMaster = true;
                    }
                }
                else {
                    if (commonFieldForSales.SaveFlag == "3")
                    {

                        string insertmasterQuery = $@"INSERT INTO MASTER_TRANSACTION(VOUCHER_NO,VOUCHER_AMOUNT,FORM_CODE,
   COMPANY_CODE,BRANCH_CODE,CREATED_BY,DELETED_FLAG,CURRENCY_CODE,CREATED_DATE,VOUCHER_DATE,SYN_ROWID,EXCHANGE_RATE,
   REFERENCE_NO,PRINT_COUNT,division_code) VALUES('{commonFieldForSales.OrderNumber}','{commonFieldForSales.GrandTotal}',
   '{commonFieldForSales.FormCode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',
   '{_workContext.CurrentUserinformation.login_code}','N','{commonFieldForSales.CurrencyFormat}',SYSDATE,
   {commonFieldForSales.VoucherDate},'{commonFieldForSales.SYN_ROWID}',
   {commonFieldForSales.ExchangeRate},'{commonFieldForSales.ManualNumber}',1,'{commonFieldForSales.Division_code}')";

                        dbcontext.ExecuteSqlCommand(insertmasterQuery);
                        _logErp.WarnInDB("Master record for" + commonFieldForSales.TableName + " saved successfully by : " + _workContext.CurrentUserinformation.login_code);
                        //AutoCVP(commonFieldForSales, dbcontext);
                        insertedToMaster = true;
                    }
                    else
                    {

                        string insertmasterQuery = $@"INSERT INTO MASTER_TRANSACTION(VOUCHER_NO,VOUCHER_AMOUNT,FORM_CODE,
   COMPANY_CODE,BRANCH_CODE,CREATED_BY,DELETED_FLAG,CURRENCY_CODE,CREATED_DATE,VOUCHER_DATE,SYN_ROWID,EXCHANGE_RATE,
   REFERENCE_NO,division_code) VALUES('{commonFieldForSales.OrderNumber}','{commonFieldForSales.GrandTotal}',
   '{commonFieldForSales.FormCode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',
   '{_workContext.CurrentUserinformation.login_code}','N','{commonFieldForSales.CurrencyFormat}',SYSDATE,
   {commonFieldForSales.VoucherDate},'{commonFieldForSales.NewVoucherNumber}',
   {commonFieldForSales.ExchangeRate},'{commonFieldForSales.ManualNumber}','{commonFieldForSales.Division_code}')";
                        dbcontext.ExecuteSqlCommand(insertmasterQuery);
                        _logErp.WarnInDB("Master record for " + commonFieldForSales.TableName + " saved successfully by : " + _workContext.CurrentUserinformation.login_code);
                        //AutoCVP(commonFieldForSales, dbcontext);
                        insertedToMaster = true;
                    }
                }
               

                return insertedToMaster;

            }
            catch (Exception ex)
            {
                _logErp.ErrorInDB("Error while saving master transaction : " + ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }
        public void AutoCVP(CommonFieldForSales commonFieldForSales, NeoErpCoreEntity dbcontext = null)
        {
            string query = $@"EXEC SA_POST.PR_SALES_INVOICE('01','01.01','21','SRSI/00001/76-77','SUMMIT')";
            dbcontext.ExecuteSqlCommand(query);
        }
        public bool SavePostedTransactionValue(/*SalesOrderDetailModel masterValue,*/ CommonFieldForSales commonFieldForSales, NeoErpCoreEntity dbcontext = null)
        {
            bool insertedToPosted = false;
            string insertPostedQuery = $@"INSERT INTO SA_POSTED_TRANSACTION(VOUCHER_NO,FORM_CODE,POSTED_BY,
   COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG) VALUES('{commonFieldForSales.OrderNumber}',  '{commonFieldForSales.FormCode}','{_workContext.CurrentUserinformation.login_code}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}','{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N')";
            dbcontext.ExecuteSqlCommand(insertPostedQuery);
            _logErp.WarnInDB("Posted record for" + commonFieldForSales.TableName + " saved successfully by : " + _workContext.CurrentUserinformation.login_code);
            insertedToPosted = true;
            return insertedToPosted;
        }

        private List<SalesOrderDetail> GetMasterTransactionByOrderNo(string orderNumber)
        {
            try
            {
                var getPrevDataQuery = $@"SELECT VOUCHER_NO,SESSION_ROWID, CREATED_BY, CREATED_DATE FROM MASTER_TRANSACTION WHERE VOUCHER_NO= '{orderNumber}' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' AND BRANCH_CODE='{_workContext.CurrentUserinformation.branch_code}'";
                var defaultData = this._objectEntity.SqlQuery<SalesOrderDetail>(getPrevDataQuery).ToList();
                return defaultData;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        private bool SaveCostomColumnValue(List<CustomOrderColumn> orderColumn, CommonFieldForSales commonFieldForSales, NeoErpCoreEntity dbcontext = null)
        {
            try
            {
                bool insertedToCustom = false;
                //var  orderColumn1 = JsonConvert.DeserializeObject(orderColumn);
                foreach (var oc in orderColumn)
                {

                    string insertQuery = string.Format(@"INSERT INTO CUSTOM_TRANSACTION(VOUCHER_NO,FIELD_NAME,FIELD_VALUE,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SESSION_ROWID) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}',TO_DATE('{7}','DD-MON-YYYY hh24:mi:ss'),'{8}','{9}')",
                              commonFieldForSales.OrderNumber, oc.FieldName, oc.FieldValue, commonFieldForSales.FormCode, _workContext.CurrentUserinformation.company_code, _workContext.CurrentUserinformation.branch_code, _workContext.CurrentUserinformation.login_code, DateTime.Now.ToString("dd-MMM-yyyy"), 'N', "");
                    dbcontext.ExecuteSqlCommand(insertQuery);
                    _logErp.WarnInDB("Custom column for sales order saved successfully by : " + _workContext.CurrentUserinformation.login_code);
                    insertedToCustom = true;

                }
                return insertedToCustom;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private bool UpdateCostomColumnValue(List<CustomOrderColumn> orderColumn, CommonFieldForSales commonFieldForSales, NeoErpCoreEntity dbcontext = null)
        {
            try
            {
                bool UpdatedToCustom = false;
                //var  orderColumn1 = JsonConvert.DeserializeObject(orderColumn);
                foreach (var oc in orderColumn)
                {

                    //string insertQuery = string.Format(@"INSERT INTO CUSTOM_TRANSACTION(VOUCHER_NO,FIELD_NAME,FIELD_VALUE,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,SESSION_ROWID) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}',TO_DATE('{7}','DD-MON-YYYY hh24:mi:ss'),'{8}','{9}')",
                    //          commonFieldForSales.OrderNumber, oc.FieldName, oc.FieldValue, commonFieldForSales.FormCode, _workContext.CurrentUserinformation.company_code, _workContext.CurrentUserinformation.branch_code, _workContext.CurrentUserinformation.login_code, DateTime.Now.ToString("dd-MMM-yyyy"), 'N', "");
                    string UpdateCSTMquery = $@"UPDATE CUSTOM_TRANSACTION SET FIELD_VALUE='{oc.FieldValue}',MODIFY_BY = '{this._workContext.CurrentUserinformation.login_code}',MODIFY_DATE = SYSDATE WHERE VOUCHER_NO='{commonFieldForSales.OrderNumber}' AND FIELD_NAME='{oc.FieldName}'";
                    dbcontext.ExecuteSqlCommand(UpdateCSTMquery);
                    UpdatedToCustom = true;

                }
                return UpdatedToCustom;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private bool SaveChargeColumnValue(List<ChargeOnSales> chargeCol, CommonFieldForSales commonFieldForSales, NeoErpCoreEntity dbcontext = null)
        {
            try
            {
                bool insertedToCharges = false;
                string currencyformat = "NRS";
                int chargeSerialNo = 0;
                // var chargeCol = JsonConvert.DeserializeObject<List<ChargeOnSales>>(chargeOnSales);
                foreach (var cc in chargeCol)
                {
                    //string transquery = string.Format(@"select to_number((max(to_number(TRANSACTION_NO)) + 1)) ORDER_NO from CHARGE_TRANSACTION");
                    string transquery = string.Format(@"select TO_NUMBER(TO_CHAR(NVL(MAX(TO_NUMBER(TRANSACTION_NO )+1),1))) ORDER_NO from CHARGE_TRANSACTION");
             
                    int newtransno = dbcontext.SqlQuery<int>(transquery).FirstOrDefault();
                    string insertChargeQuery = $@"INSERT INTO CHARGE_TRANSACTION(TRANSACTION_NO,TABLE_NAME,REFERENCE_NO,APPLY_ON,ACC_CODE,CHARGE_CODE,CHARGE_TYPE_FLAG,CHARGE_AMOUNT,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,CURRENCY_CODE,EXCHANGE_RATE,CALCULATE_BY,SESSION_ROWID,GL_FLAG,NON_GL_FLAG,SERIAL_NO) VALUES('{newtransno}','{commonFieldForSales.TableName}','{commonFieldForSales.OrderNumber}','{cc.APPLY_ON}','{cc.ACC_CODE}','{cc.CHARGE_CODE}','{cc.CHARGE_TYPE_FLAG}', {cc.CHARGE_AMOUNT},'{commonFieldForSales.FormCode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}','{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','{currencyformat}',{commonFieldForSales.ExchangeRate},'{cc.VALUE_PERCENT_FLAG}','{commonFieldForSales.NewVoucherNumber}','{cc.GL_FLAG}','{cc.NON_GL_FLAG}',1)";
                    dbcontext.ExecuteSqlCommand(insertChargeQuery);
                    insertedToCharges = true;
                    chargeSerialNo++;
                }
                _logErp.WarnInDB("Charges for sales order saved successfully by : " + _workContext.CurrentUserinformation.login_code);
                return insertedToCharges;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private bool SaveBatchTransactionValues(List<BATCHTRANSACTIONDATA> batchTransaction, CommonFieldForSales commonValue, NeoErpCoreEntity dbcontext = null)
        {           
            try
            {
                bool insertedTobatch = false;
                var batchSerial = 1;
                foreach (var btrans in batchTransaction)
                {
                    if (btrans.SERIAL_TRACKING_FLAG == 'N' && btrans.BATCH_TRACKING_FLAG == 'Y')
                    {
                        if (btrans.REFERNCE_FROM_BATCH)
                        {
                            string query = $@"SELECT REFERENCE_NO FROM BATCH_TRANSACTION WHERE DELETED_FLAG = 'N' AND BATCH_NO = '{btrans.BATCH_NO}' AND REFERENCE_NO != '0' AND DELETED_FLAG = 'N' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' AND BRANCH_CODE='{_workContext.CurrentUserinformation.branch_code}' AND ITEM_CODE = '{btrans.ITEM_CODE}'";
                            var result = dbcontext.SqlQuery<string>(query).ToList();
                            if (result.Count > 0)
                            {
                                string updateQuery = $@"UPDATE BATCH_TRANSACTION SET DELETED_FLAG = 'Y' WHERE REFERENCE_NO = '{result[0].ToString()}' AND DELETED_FLAG = 'N' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' AND BRANCH_CODE='{_workContext.CurrentUserinformation.branch_code}' AND ITEM_CODE = '{btrans.ITEM_CODE}'";
                                dbcontext.ExecuteSqlCommand(updateQuery);
                            }
                        }
                        else
                        {
                            string updateQuery = $@"UPDATE BATCH_TRANSACTION SET DELETED_FLAG = 'Y' WHERE ITEM_CODE = '{btrans.ITEM_CODE}' AND FORM_CODE = '{commonValue.FormCode}' AND REFERENCE_NO = '{commonValue.OrderNumber}' AND DELETED_FLAG = 'N' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' AND BRANCH_CODE='{_workContext.CurrentUserinformation.branch_code}'";
                            dbcontext.ExecuteSqlCommand(updateQuery);
                        }
                    }
                }
                foreach (var btrans in batchTransaction)
                {
                    if (btrans.ITEM_CODE != "" && !string.IsNullOrEmpty(btrans.TRACKING_SERIAL_NO))
                    {
                        var VoucherNumber = (commonValue.OrderNumber == "undefined") ? commonValue.NewVoucherNumber : commonValue.OrderNumber;
                        string maxtransnoquery = string.Format(@"SELECT TO_NUMBER(TO_CHAR(NVL(MAX(TO_NUMBER(TRANSACTION_NO )+1),1))) as TRANSACTIONNO FROM BATCH_TRANSACTION");
                        int newMaxTransNoForBatch = dbcontext.SqlQuery<int>(maxtransnoquery).FirstOrDefault();
                        var mucode = btrans.MU_CODE == "" ? "PCS" : btrans.MU_CODE;
                        string insertbatchtransQuery = $@"INSERT INTO BATCH_TRANSACTION(
                                                                              TRANSACTION_NO,FORM_CODE,REFERENCE_NO,SERIAL_NO,ITEM_CODE,
                                                                              BATCH_NO,MU_CODE,QUANTITY,SOURCE_FLAG,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,
                                                                              ITEM_SERIAL_NO,NON_STOCK_FLAG,LOCATION_CODE)
                                                                              VALUES('{newMaxTransNoForBatch}','{commonValue.FormCode}','{VoucherNumber}',{btrans.SERIAL_NO},'{btrans.ITEM_CODE}',
                                                                             '{mucode}','{mucode}',1,'O','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}','{_workContext.CurrentUserinformation.login_code.ToUpper()}',SYSDATE,'N','{btrans.TRACKING_SERIAL_NO}','N','{btrans.LOCATION_CODE}')";
                        dbcontext.ExecuteSqlCommand(insertbatchtransQuery);
                    }
                    //if (btrans.TRACK != null)
                    //{
                    //if (btrans.TRACK.Count > 0)
                    //{
                    //    foreach (var bud in btrans.TRACK)
                    //    {
                    //        if (bud.TRACKING_SERIAL_NO != "")
                    //        {
                    //            var VoucherNumber = (commonValue.VoucherNumber == "undefined") ? commonValue.NewVoucherNumber : commonValue.VoucherNumber;
                    //            string maxtransnoquery = string.Format(@"SELECT TO_NUMBER(TO_CHAR(NVL(MAX(TO_NUMBER(TRANSACTION_NO )+1),1))) as TRANSACTIONNO FROM BATCH_TRANSACTION");
                    //            int newMaxTransNoForBatch = dbcontext.SqlQuery<int>(maxtransnoquery).FirstOrDefault();
                    //            var mucode = btrans.MU_CODE == "" ? "PCS" : btrans.MU_CODE;
                    //            string insertbatchtransQuery = $@"INSERT INTO BATCH_TRANSACTION(
                    //                                                      TRANSACTION_NO,FORM_CODE,REFERENCE_NO,SERIAL_NO,ITEM_CODE,
                    //                                                      BATCH_NO,MU_CODE,QUANTITY,SOURCE_FLAG,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,
                    //                                                      ITEM_SERIAL_NO,NON_STOCK_FLAG,LOCATION_CODE)
                    //                                                      VALUES('{newMaxTransNoForBatch}','{commonValue.FormCode}','{VoucherNumber}',{bud.SERIAL_NO},'{btrans.ITEM_CODE}',
                    //                                                     '{mucode}','{mucode}',1,'I','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}','{_workContext.CurrentUserinformation.login_code.ToUpper()}',SYSDATE,'N','{bud.TRACKING_SERIAL_NO}','N','{btrans.LOCATION_CODE}')";
                    //            dbcontext.ExecuteSqlCommand(insertbatchtransQuery);


                    //        }

                    //    }
                    //}
                    //}
                    if (btrans.SERIAL_TRACKING_FLAG == 'N' && btrans.BATCH_TRACKING_FLAG == 'Y')
                    {
                        for (int i = 0; i < btrans.QUANTITY; i++)
                        {
                            //var VoucherNumber = (commonValue.NewOrderNumber == "undefined") ? commonValue.NewVoucherNumber : commonValue.NewOrderNumber;

                            //string updateQuery = $@"UPDATE BATCH_TRANSACTION SET DELETED_FLAG = 'Y' WHERE ITEM_CODE = '{btrans.ITEM_CODE}' AND FORM_CODE = '{commonValue.FormCode}' AND REFERENCE_NO = '{commonValue.OrderNumber}' AND DELETED_FLAG = 'N' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' AND BRANCH_CODE='{_workContext.CurrentUserinformation.branch_code}' AND CREATED_BY = '{_workContext.CurrentUserinformation.login_code.ToUpper()}'";
                            //dbcontext.ExecuteSqlCommand(updateQuery);

                            string maxtransnoquery = string.Format(@"SELECT TO_NUMBER(TO_CHAR(NVL(MAX(TO_NUMBER(TRANSACTION_NO )+1),1))) as TRANSACTIONNO FROM BATCH_TRANSACTION");
                            int newMaxTransNoForBatch = dbcontext.SqlQuery<int>(maxtransnoquery).FirstOrDefault();
                            var mucode = btrans.MU_CODE == "" ? "PCS" : btrans.MU_CODE;
                            string insertbatchtransQuery = $@"INSERT INTO BATCH_TRANSACTION(
                                                                              TRANSACTION_NO,FORM_CODE,REFERENCE_NO,SERIAL_NO,ITEM_CODE,
                                                                              BATCH_NO,MU_CODE,QUANTITY,SOURCE_FLAG,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,
                                                                              ITEM_SERIAL_NO,NON_STOCK_FLAG,LOCATION_CODE,EXPIRY_DATE,BATCH_SERIAL_FLAG)
                                                                              VALUES('{newMaxTransNoForBatch}','{commonValue.FormCode}','{commonValue.OrderNumber}',{btrans.SERIAL_NO},'{btrans.ITEM_CODE}',
                                                                             '{btrans.BATCH_NO}','{mucode}',1,'O','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}','{_workContext.CurrentUserinformation.login_code.ToUpper()}',SYSDATE,'N','{btrans.TRACKING_SERIAL_NO}','N','{btrans.LOCATION_CODE}',TO_DATE('{btrans.EXPIRY_DATE.ToString("MM-dd-yyyy")}','MM-DD-YYYY'),'Y')";
                            dbcontext.ExecuteSqlCommand(insertbatchtransQuery);
                        }
                    }
                    insertedTobatch = true;
                }
               return insertedTobatch;
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }
        private bool UpdateChargeColumnValue(List<ChargeOnSales> chargeCol, CommonFieldForSales commonFieldForSales, NeoErpCoreEntity dbcontext = null)
        {
            try
            {
                bool updatedToCharges = false;
                string currencyformat = "NRS";
                int chargeSerialNo = 0;
                // var chargeCol = JsonConvert.DeserializeObject<List<ChargeOnSales>>(chargeOnSales);
                foreach (var cc in chargeCol)
                {
                    string transquery = string.Format(@"select TO_NUMBER(TO_CHAR(NVL(MAX(TO_NUMBER(TRANSACTION_NO )+1),1))) ORDER_NO from CHARGE_TRANSACTION");
                    int newtransno = dbcontext.SqlQuery<int>(transquery).FirstOrDefault();
                    //string insertChargeQuery = $@"INSERT INTO CHARGE_TRANSACTION(TRANSACTION_NO,TABLE_NAME,REFERENCE_NO,APPLY_ON,ACC_CODE,CHARGE_CODE,CHARGE_TYPE_FLAG,CHARGE_AMOUNT,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,CURRENCY_CODE,EXCHANGE_RATE,VOUCHER_NO,CALCULATE_BY,SERIAL_NO,SESSION_ROWID) VALUES('{newtransno}','{commonFieldForSales.TableName}','{commonFieldForSales.OrderNumber}','{cc.APPLY_ON}','{cc.ACC_CODE}','{cc.CHARGE_CODE}','{cc.CHARGE_TYPE_FLAG}', {cc.CHARGE_AMOUNT},'{commonFieldForSales.FormCode}','{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}','{_workContext.CurrentUserinformation.login_code}',SYSDATE,'N','{currencyformat}',{commonFieldForSales.ExchangeRate},'{commonFieldForSales.OrderNumber}','{cc.VALUE_PERCENT_FLAG}',{chargeSerialNo},'{commonFieldForSales.NewVoucherNumber}')";
                    string chargeupdatequery = $@"UPDATE CHARGE_TRANSACTION SET CHARGE_AMOUNT={cc.CHARGE_AMOUNT} WHERE REFERENCE_NO='{commonFieldForSales.OrderNumber}' AND CHARGE_CODE='{cc.CHARGE_CODE}'";
                    dbcontext.ExecuteSqlCommand(chargeupdatequery);
                    updatedToCharges = true;
                    chargeSerialNo++;
                }
                return updatedToCharges;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private bool SaveShippingDetailsColumnValue(ShippingDetails shippingDetails, CommonFieldForSales commonFieldForSales, NeoErpCoreEntity Dbcontext = null)
        {
            try
            {
                bool insertedShippingDetails = false;
                // var shippingCol = JsonConvert.DeserializeObject<ShippingDetails>(shippingDetails);
                if (string.IsNullOrEmpty(shippingDetails.VEHICLE_CODE) || string.IsNullOrEmpty(commonFieldForSales.OrderNumber) || string.IsNullOrEmpty(commonFieldForSales.FormCode))
                    return insertedShippingDetails;
                //subin_ship
                shippingDetails.TRANSPORT_INVOICE_DATE = string.IsNullOrEmpty(shippingDetails.TRANSPORT_INVOICE_DATE) ? "''" : shippingDetails.WB_DATE == "Invalid date"?"''": "TO_DATE('" + shippingDetails.TRANSPORT_INVOICE_DATE + "', 'DD-MON-YYYY')";


                shippingDetails.WB_DATE = string.IsNullOrEmpty(shippingDetails.WB_DATE) ? "''" : shippingDetails.WB_DATE== "Invalid date"?"''": "TO_DATE('" + shippingDetails.WB_DATE + "', 'DD-MON-YYYY')";

                shippingDetails.GATE_ENTRY_DATE = string.IsNullOrEmpty(shippingDetails.GATE_ENTRY_DATE) ? "''" : shippingDetails.GATE_ENTRY_DATE == "Invalid date" ? "''" : "TO_DATE('" + shippingDetails.GATE_ENTRY_DATE + "', 'DD-MON-YYYY')";

                string insertSDQuery = $@"INSERT INTO SHIPPING_TRANSACTION (VOUCHER_NO, FORM_CODE, VEHICLE_CODE, VEHICLE_OWNER_NAME, VEHICLE_OWNER_NO, DRIVER_NAME, DRIVER_LICENSE_NO, DRIVER_MOBILE_NO, TRANSPORTER_CODE, FREGHT_AMOUNT, START_FORM, DESTINATION, COMPANY_CODE, BRANCH_CODE, CREATED_DATE, CREATED_BY, DELETED_FLAG, TRANSPORT_INVOICE_NO, CN_NO, TRANSPORT_INVOICE_DATE, DELIVERY_INVOICE_DATE,WB_WEIGHT, WB_NO, WB_DATE,FREIGHT_RATE, VOUCHER_DATE,VEHICLE_NO, LOADING_SLIP_NO, GATE_ENTRY_NO, GATE_ENTRY_DATE,SHIPPING_TERMS) 
                VALUES ('{commonFieldForSales.OrderNumber}','{commonFieldForSales.FormCode}','{shippingDetails.VEHICLE_CODE}','{shippingDetails.VEHICLE_OWNER_NAME}','{shippingDetails.VEHICLE_OWNER_NO}','{shippingDetails.DRIVER_NAME}',
                '{shippingDetails.DRIVER_LICENCE_NO}','{shippingDetails.DRIVER_MOBILE_NO}','{shippingDetails.TRANSPORTER_CODE}','{shippingDetails.FREGHT_AMOUNT}','{shippingDetails.START_FORM}','{shippingDetails.DESTINATION}',
                 '{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',SYSDATE,'{_workContext.CurrentUserinformation.login_code}','N',
                '{shippingDetails.TRANSPORT_INVOICE_NO}','{shippingDetails.CN_NO}',{shippingDetails.TRANSPORT_INVOICE_DATE},sysdate,'{shippingDetails.WB_WEIGHT}',
                '{shippingDetails.WB_NO}',{shippingDetails.WB_DATE},'{shippingDetails.FREIGHT_RATE}',{commonFieldForSales.VoucherDate},'{shippingDetails.VEHICLE_NO}','{shippingDetails.LOADING_SLIP_NO}','{shippingDetails.GATE_ENTRY_NO}',{shippingDetails.GATE_ENTRY_DATE},'{shippingDetails.SHIPPING_TERMS}')";
                Dbcontext.ExecuteSqlCommand(insertSDQuery);
                insertedShippingDetails = true;
                _logErp.WarnInDB("shipping details for sales order saved successfully by : " + _workContext.CurrentUserinformation.login_code);
                return insertedShippingDetails;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private bool UpdateShippingDetailsColumnValue(ShippingDetails shippingDetails, CommonFieldForSales commonFieldForSales, NeoErpCoreEntity Dbcontext = null)
        {
            try
            {
                bool updatedShippingDetails = false;
                // var shippingCol = JsonConvert.DeserializeObject<ShippingDetails>(shippingDetails);
                if (string.IsNullOrEmpty(shippingDetails.VEHICLE_CODE) || string.IsNullOrEmpty(commonFieldForSales.OrderNumber) || string.IsNullOrEmpty(commonFieldForSales.FormCode))
                    return updatedShippingDetails;
                //subin_ship
                shippingDetails.TRANSPORT_INVOICE_DATE = string.IsNullOrEmpty(shippingDetails.TRANSPORT_INVOICE_DATE) ? "null" : "TO_DATE('" + shippingDetails.TRANSPORT_INVOICE_DATE + "', 'DD-MON-YYYY')";


                shippingDetails.WB_DATE = string.IsNullOrEmpty(shippingDetails.WB_DATE) ? "null" : "TO_DATE('" + shippingDetails.WB_DATE + "', 'DD-MON-YYYY')";

                shippingDetails.GATE_ENTRY_DATE = string.IsNullOrEmpty(shippingDetails.GATE_ENTRY_DATE) ? "null" : "TO_DATE('" + shippingDetails.GATE_ENTRY_DATE + "', 'DD-MON-YYYY')";

                //string insertSDQuery = $@"INSERT INTO SHIPPING_TRANSACTION (VOUCHER_NO, FORM_CODE, VEHICLE_CODE, VEHICLE_OWNER_NAME, VEHICLE_OWNER_NO, DRIVER_NAME, DRIVER_LICENSE_NO, DRIVER_MOBILE_NO, TRANSPORTER_CODE, FREGHT_AMOUNT, START_FORM, DESTINATION, COMPANY_CODE, BRANCH_CODE, CREATED_DATE, CREATED_BY, DELETED_FLAG, TRANSPORT_INVOICE_NO, CN_NO, TRANSPORT_INVOICE_DATE, DELIVERY_INVOICE_DATE,WB_WEIGHT, WB_NO, WB_DATE,FREIGHT_RATE, VOUCHER_DATE,VEHICLE_NO, LOADING_SLIP_NO, GATE_ENTRY_NO, GATE_ENTRY_DATE) 
                //VALUES ('{commonFieldForSales.OrderNumber}','{commonFieldForSales.FormCode}','{shippingDetails.VEHICLE_CODE}','{shippingDetails.VEHICLE_OWNER_NAME}','{shippingDetails.VEHICLE_OWNER_NO}','{shippingDetails.DRIVER_NAME}',
                //'{shippingDetails.DRIVER_LICENCE_NO}','{shippingDetails.DRIVER_MOBILE_NO}','{shippingDetails.TRANSPORTER_CODE}','{shippingDetails.FREGHT_AMOUNT}','{shippingDetails.START_FORM}','{shippingDetails.DESTINATION}',
                // '{_workContext.CurrentUserinformation.company_code}','{_workContext.CurrentUserinformation.branch_code}',SYSDATE,'{_workContext.CurrentUserinformation.login_code}','N',
                //'{shippingDetails.TRANSPORT_INVOICE_NO}','{shippingDetails.CN_NO}',{shippingDetails.TRANSPORT_INVOICE_DATE},sysdate,'{shippingDetails.WB_WEIGHT}',
                //'{shippingDetails.WB_NO}',{shippingDetails.WB_DATE},'{shippingDetails.FREIGHT_RATE}',{commonFieldForSales.VoucherDate},'{shippingDetails.VEHICLE_NO}','{shippingDetails.LOADING_SLIP_NO}','{shippingDetails.GATE_ENTRY_NO}',{shippingDetails.GATE_ENTRY_DATE})";
                string updateSDQuery = $@"UPDATE SHIPPING_TRANSACTION SET VEHICLE_CODE='{shippingDetails.VEHICLE_CODE}',VEHICLE_OWNER_NAME='{shippingDetails.VEHICLE_OWNER_NAME}',VEHICLE_OWNER_NO='{shippingDetails.VEHICLE_OWNER_NO}',DRIVER_NAME='{shippingDetails.DRIVER_NAME}',DRIVER_LICENSE_NO='{shippingDetails.DRIVER_LICENCE_NO}',DRIVER_MOBILE_NO='{shippingDetails.DRIVER_MOBILE_NO}',TRANSPORTER_CODE='{shippingDetails.TRANSPORTER_CODE}',FREGHT_AMOUNT={shippingDetails.FREGHT_AMOUNT},START_FORM='{shippingDetails.START_FORM}',DESTINATION='{shippingDetails.DESTINATION}',TRANSPORT_INVOICE_NO='{shippingDetails.TRANSPORT_INVOICE_NO}',CN_NO='{shippingDetails.CN_NO}',TRANSPORT_INVOICE_DATE={shippingDetails.TRANSPORT_INVOICE_DATE},DELIVERY_INVOICE_DATE=TO_DATE('{shippingDetails.DELIVERY_INVOICE_DATE}','DD-MON-YYYY'),WB_WEIGHT={shippingDetails.WB_WEIGHT},WB_NO='{shippingDetails.WB_NO}',WB_DATE={shippingDetails.WB_DATE},FREIGHT_RATE={shippingDetails.FREIGHT_RATE},VEHICLE_NO='{shippingDetails.VEHICLE_NO}',LOADING_SLIP_NO={shippingDetails.LOADING_SLIP_NO},GATE_ENTRY_NO='{shippingDetails.GATE_ENTRY_NO}',GATE_ENTRY_DATE={shippingDetails.GATE_ENTRY_DATE}, MODIFY_DATE=SYSDATE,MODIFY_BY='{this._workContext.CurrentUserinformation.login_code}' WHERE VOUCHER_NO='{commonFieldForSales.OrderNumber}'";
                this._objectEntity.ExecuteSqlCommand(updateSDQuery);

                Dbcontext.ExecuteSqlCommand(updateSDQuery);
                updatedShippingDetails = true;
                return updatedShippingDetails;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        private bool DeleteSalesOrderBeforeUpdate(string tableName, string primaryCol, string orderNumber)
        {
            try
            {
                string deletequery = string.Format(@"DELETE FROM " + tableName + " where " + primaryCol + "='{0}' and COMPANY_CODE='{1}'", orderNumber, _workContext.CurrentUserinformation.company_code);
                this._objectEntity.ExecuteSqlCommand(deletequery);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private bool DeleteCustomColumnBeforeUpdate(List<CustomOrderColumn> customOrderColumns, string orderNumber)
        {
            foreach (var coc in customOrderColumns)
            {
                string deletecustomcolumn = string.Format(@"DELETE FROM CUSTOM_TRANSACTION where VOUCHER_NO='{0}' and COMPANY_CODE='{1}'", orderNumber, this._workContext.CurrentUserinformation.company_code);
                this._objectEntity.ExecuteSqlCommand(deletecustomcolumn);
            }

            return true;
        }

        private bool DeleteChargeColumnBeforeUpdate(List<ChargeOnSales> chargeOnSales, string orderNumber)
        {
            foreach (var cos in chargeOnSales)
            {
                string deleteChargevalus = $@"DELETE FROM CHARGE_TRANSACTION WHERE REFERENCE_NO='{orderNumber}' AND COMPANY_CODE='{this._workContext.CurrentUserinformation.company_code}' AND BRANCH_CODE='{this._workContext.CurrentUserinformation.branch_code}'";
                this._objectEntity.ExecuteSqlCommand(deleteChargevalus);
            }

            return true;
        }
        private bool DeletSerialdataBeforeUpdate(List<BATCHTRANSACTIONDATA> batchtransactiondata, string orderNumber)
        {
            foreach (var bt in batchtransactiondata)
            {
                string deleteChargevalus = $@"DELETE FROM BATCH_TRANSACTION WHERE REFERENCE_NO='{orderNumber}' AND COMPANY_CODE='{this._workContext.CurrentUserinformation.company_code}' AND BRANCH_CODE='{this._workContext.CurrentUserinformation.branch_code}' AND ITEM_CODE='{bt.ITEM_CODE}' AND LOCATION_CODE='{bt.LOCATION_CODE}'";
                this._objectEntity.ExecuteSqlCommand(deleteChargevalus);
            }

            return true;
        }

        private bool DeleteInvItemColumnBeforeUpdate(string invItemCharge)
        {
            return true;
        }

        private bool DeleteShippingDetailsBeforeUpdate(ShippingDetails shippingDetails, string orderNumber)
        {
            string deletesdvalus = $@"DELETE FROM SHIPPING_TRANSACTION WHERE VOUCHER_NO='{orderNumber}' AND COMPANY_CODE='{this._workContext.CurrentUserinformation.company_code}' AND BRANCH_CODE='{this._workContext.CurrentUserinformation.branch_code}'";
            this._objectEntity.ExecuteSqlCommand(deletesdvalus);
            return true;
        }


        #endregion


        #region PRIVATE METHOD TO ADD SALES CHALAN

        private bool SaveSalesChalanValues(List<SalesChalanDetail> childValue, SalesChalanDetail masterValue, string orderNumber, string formCode, NeoErpCoreEntity dbcontext = null)
        {
            try
            {
                int serialNo = 1;
                bool insertedToChild = false;
                SalesChalanDetail salesChildChalan = new SalesChalanDetail();
                foreach (var cCol in childValue)
                {
                    salesChildChalan.CHALAN_NO = orderNumber;
                    salesChildChalan.CHALAN_DATE = string.IsNullOrEmpty(cCol.CHALAN_DATE) ? masterValue.CHALAN_DATE.ToString() : cCol.CHALAN_DATE;
                    salesChildChalan.CUSTOMER_CODE = string.IsNullOrEmpty(cCol.CUSTOMER_CODE) ? masterValue.CUSTOMER_CODE : cCol.CUSTOMER_CODE;
                    salesChildChalan.SERIAL_NO = cCol.SERIAL_NO > 0 ? cCol.SERIAL_NO : serialNo;
                    salesChildChalan.ITEM_CODE = string.IsNullOrEmpty(cCol.ITEM_CODE) ? "" : cCol.ITEM_CODE;
                    salesChildChalan.MU_CODE = string.IsNullOrEmpty(cCol.MU_CODE) ? "" : cCol.MU_CODE;
                    salesChildChalan.QUANTITY = cCol.QUANTITY > 0 ? cCol.QUANTITY : cCol.CALC_QUANTITY > 0 ? cCol.CALC_QUANTITY : 0;
                    salesChildChalan.UNIT_PRICE = cCol.UNIT_PRICE > 0 ? cCol.UNIT_PRICE : cCol.CALC_UNIT_PRICE > 0 ? cCol.CALC_UNIT_PRICE : 0;
                    salesChildChalan.TOTAL_PRICE = cCol.TOTAL_PRICE;
                    salesChildChalan.CALC_QUANTITY = cCol.CALC_QUANTITY > 0 ? cCol.CALC_QUANTITY : cCol.QUANTITY > 0 ? cCol.QUANTITY : 1;
                    salesChildChalan.CALC_UNIT_PRICE = cCol.CALC_UNIT_PRICE > 0 ? cCol.CALC_UNIT_PRICE : cCol.UNIT_PRICE > 0 ? cCol.UNIT_PRICE : 0;
                    salesChildChalan.CALC_TOTAL_PRICE = cCol.CALC_TOTAL_PRICE > 0 ? cCol.CALC_TOTAL_PRICE : cCol.TOTAL_PRICE > 0 ? cCol.TOTAL_PRICE : 0;
                    salesChildChalan.REMARKS = string.IsNullOrEmpty(cCol.REMARKS) ? masterValue.REMARKS : cCol.REMARKS;
                    salesChildChalan.FORM_CODE = string.IsNullOrEmpty(cCol.FORM_CODE) ? formCode : cCol.FORM_CODE;
                    salesChildChalan.COMPANY_CODE = _workContext.CurrentUserinformation.company_code;
                    salesChildChalan.BRANCH_CODE = _workContext.CurrentUserinformation.branch_code;
                    salesChildChalan.CREATED_BY = _workContext.CurrentUserinformation.login_code;
                    salesChildChalan.CREATED_DATE = DateTime.Now;
                    salesChildChalan.DELETED_FLAG = "N";
                    salesChildChalan.DELIVERY_DATE = string.IsNullOrEmpty(cCol.DELIVERY_DATE) ? masterValue.DELIVERY_DATE : cCol.DELIVERY_DATE;
                    //salesChildChalan.CURRENCY_CODE = cCol.CURRENCY_CODE;
                    //salesChildChalan.EXCHANGE_RATE = cCol.EXCHANGE_RATE;
                    salesChildChalan.DELIVERY_DATE = string.IsNullOrEmpty(cCol.DELIVERY_DATE) ? masterValue.DELIVERY_DATE : cCol.DELIVERY_DATE;
                    salesChildChalan.CURRENCY_CODE = string.IsNullOrEmpty(cCol.CURRENCY_CODE) ? "NRS" : cCol.CURRENCY_CODE;
                    salesChildChalan.EXCHANGE_RATE = cCol.EXCHANGE_RATE > 0 ? cCol.QUANTITY : 1;
                    salesChildChalan.TRACKING_NO = cCol.TRACKING_NO;
                    salesChildChalan.STOCK_BLOCK_FLAG = cCol.STOCK_BLOCK_FLAG;
                    salesChildChalan.SESSION_ROWID = cCol.SESSION_ROWID;
                    salesChildChalan.MODIFY_BY = cCol.MODIFY_BY;
                    salesChildChalan.DIVISION_CODE = cCol.DIVISION_CODE;
                    salesChildChalan.PARTY_TYPE_CODE = string.IsNullOrEmpty(cCol.PARTY_TYPE_CODE) ? masterValue.PARTY_TYPE_CODE : cCol.PARTY_TYPE_CODE;
                    salesChildChalan.SALES_TYPE_CODE = string.IsNullOrEmpty(cCol.SALES_TYPE_CODE) ? masterValue.SALES_TYPE_CODE : cCol.SALES_TYPE_CODE;
                    salesChildChalan.MODIFY_DATE = DateTime.Now;
                    salesChildChalan.EMPLOYEE_CODE = string.IsNullOrEmpty(cCol.EMPLOYEE_CODE) ? masterValue.EMPLOYEE_CODE : cCol.EMPLOYEE_CODE;
                    salesChildChalan.MANUAL_NO = masterValue.MANUAL_NO;
                    // salesChildOrder.EMPLOYEE_CODE = masterValue.EMPLOYEE_CODE;
                    //salesChildChalan.SHIPPING_ADDRESS = masterValue.SHIPPING_ADDRESS;
                    if (masterValue.SHIPPING_ADDRESS != null)
                    {
                        salesChildChalan.SHIPPING_ADDRESS = masterValue.SHIPPING_ADDRESS.Contains("'") ? masterValue.SHIPPING_ADDRESS.Replace("'", "' || '''' || '") : masterValue.SHIPPING_ADDRESS;
                    }
                    else
                    {
                        salesChildChalan.SHIPPING_ADDRESS = "";
                    }
                    salesChildChalan.SHIPPING_CONTACT_NO = masterValue.SHIPPING_CONTACT_NO;
                    salesChildChalan.AREA_CODE = masterValue.AREA_CODE;
                    salesChildChalan.PRIORITY_CODE = string.IsNullOrEmpty(cCol.PRIORITY_CODE) ? masterValue.PRIORITY_CODE : cCol.PRIORITY_CODE;
                    salesChildChalan.LINE_ITEM_DISCOUNT = masterValue.LINE_ITEM_DISCOUNT;
                    salesChildChalan.FROM_LOCATION_CODE = string.IsNullOrEmpty(cCol.FROM_LOCATION_CODE) ? masterValue.FROM_LOCATION_CODE : cCol.FROM_LOCATION_CODE;
                    salesChildChalan.SECTOR_CODE = string.IsNullOrEmpty(cCol.SECTOR_CODE) ? masterValue.SECTOR_CODE : cCol.SECTOR_CODE;
                    salesChildChalan.ALT1_MU_CODE = cCol.ALT1_MU_CODE;
                    salesChildChalan.ALT1_QUANTITY = cCol.ALT1_QUANTITY;
                    //salesChildChalan.SECOND_QUANTITY = cCol.SECOND_QUANTITY > 0 ? cCol.SECOND_QUANTITY : 1;

                    //var querySalesorder = $@"Insert into SA_SALES_ORDER ({column}) Values('{value}')";
                    //var InsertedData = _objectEntity.ExecuteSqlCommand(querySalesorder);

                    var querySalesChalan = $@"Insert into SA_SALES_CHALAN
                                        (CHALAN_NO, CHALAN_DATE, CUSTOMER_CODE, FROM_LOCATION_CODE, 
                                            SERIAL_NO, ITEM_CODE, MU_CODE, QUANTITY, UNIT_PRICE, TOTAL_PRICE, CALC_QUANTITY, CALC_UNIT_PRICE, CALC_TOTAL_PRICE, FORM_CODE, COMPANY_CODE, BRANCH_CODE, CREATED_BY, CREATED_DATE, 
                                            DELETED_FLAG, CURRENCY_CODE, EXCHANGE_RATE, TRACKING_NO, SESSION_ROWID, MODIFY_BY, PARTY_TYPE_CODE, PRIORITY_CODE, 
                                            SHIPPING_ADDRESS, SHIPPING_CONTACT_NO, SALES_TYPE_CODE, EMPLOYEE_CODE, AREA_CODE,MANUAL_NO,REMARKS,AGENT_CODE,DIVISION_CODE,SECTOR_CODE,ALT1_MU_CODE,ALT1_QUANTITY,SECOND_QUANTITY,THIRD_QUANTITY)
                                         Values
                                       ('{salesChildChalan.CHALAN_NO}', TO_DATE('{salesChildChalan.CHALAN_DATE}', 'DD-MON-YYYY hh24:mi:ss'), '{salesChildChalan.CUSTOMER_CODE}', '{salesChildChalan.FROM_LOCATION_CODE}', 
                                        {salesChildChalan.SERIAL_NO}, '{salesChildChalan.ITEM_CODE}', '{salesChildChalan.MU_CODE}', {salesChildChalan.QUANTITY}, {salesChildChalan.UNIT_PRICE}, 
                                            {salesChildChalan.TOTAL_PRICE}, {salesChildChalan.CALC_QUANTITY}, {salesChildChalan.CALC_UNIT_PRICE}, {salesChildChalan.CALC_TOTAL_PRICE}, 
    '{salesChildChalan.FORM_CODE}', '{salesChildChalan.COMPANY_CODE}', '{salesChildChalan.BRANCH_CODE}', '{salesChildChalan.CREATED_BY}', 
    sysdate, 'N', '{salesChildChalan.CURRENCY_CODE}', {salesChildChalan.EXCHANGE_RATE}, 
    '{salesChildChalan.TRACKING_NO}', '{salesChildChalan.SYN_ROWID}', '', '{salesChildChalan.PARTY_TYPE_CODE}', '{salesChildChalan.PRIORITY_CODE}', 
                        '{salesChildChalan.SHIPPING_ADDRESS}', '{salesChildChalan.SHIPPING_CONTACT_NO}', '{salesChildChalan.SALES_TYPE_CODE}', '{salesChildChalan.EMPLOYEE_CODE}', '{salesChildChalan.AREA_CODE}','{salesChildChalan.MANUAL_NO}','{salesChildChalan.REMARKS}','{salesChildChalan.AGENT_CODE}','{salesChildChalan.DIVISION_CODE}','{salesChildChalan.SECTOR_CODE}','{salesChildChalan.ALT1_MU_CODE}','{salesChildChalan.ALT1_QUANTITY}','{cCol.SECOND_QUANTITY}','{cCol.THIRD_QUANTITY}')";

                    //var querySalesorder = $@"Insert into SA_SALES_CHALAN (CHALAN_NO, CHALAN_DATE, CUSTOMER_CODE, SERIAL_NO, ITEM_CODE, MU_CODE, QUANTITY, UNIT_PRICE, TOTAL_PRICE, CALC_QUANTITY, CALC_UNIT_PRICE, CALC_TOTAL_PRICE, 
                    //  REMARKS, FORM_CODE, COMPANY_CODE, BRANCH_CODE, CREATED_BY, CREATED_DATE, DELETED_FLAG, DELIVERY_DATE, CURRENCY_CODE, EXCHANGE_RATE,TRACKING_NO, STOCK_BLOCK_FLAG, SESSION_ROWID, MODIFY_BY, DIVISION_CODE) 
                    // Values ('{salesChildChalan.CHALAN_NO}',TO_DATE('{salesChildChalan.ORDER_DATE}','DD-MON-YYYY hh24:mi:ss'), '{salesChildChalan.CUSTOMER_CODE}', {salesChildChalan.SERIAL_NO}, 
                    //'{salesChildChalan.ITEM_CODE}', '{salesChildChalan.MU_CODE}', {salesChildChalan.QUANTITY}, {salesChildChalan.UNIT_PRICE}, {salesChildChalan.TOTAL_PRICE}, 
                    // {salesChildChalan.CALC_QUANTITY}, {salesChildChalan.CALC_UNIT_PRICE}, {salesChildChalan.CALC_TOTAL_PRICE}, '{salesChildChalan.REMARKS}','{salesChildChalan.FORM_CODE}', '{salesChildChalan.COMPANY_CODE}', '{salesChildChalan.BRANCH_CODE}', '{salesChildChalan.CREATED_BY}', SYSDATE, 
                    //'N', TO_DATE('{salesChildChalan.DELIVERY_DATE}','DD-MON-YYYY hh24:mi:ss'), '{salesChildChalan.CURRENCY_CODE}', {salesChildChalan.EXCHANGE_RATE},'{salesChildChalan.TRACKING_NO}', '{salesChildChalan.STOCK_BLOCK_FLAG}', '{salesChildChalan.SESSION_ROWID}', '', '{salesChildChalan.DIVISION_CODE}')";
                    var InsertedData = dbcontext.ExecuteSqlCommand(querySalesChalan);
                    serialNo++;
                }


                insertedToChild = true;
                return insertedToChild;


            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }

        //private bool SaveChalanMasterTransactionValue(SalesChalanDetail salesChalanDetail,CommonFieldForSales commonFieldForSales)
        //{
        //    try
        //    {
        //        bool insertedToMaster = false;
        //        if (commonFieldForSales.SaveFlag == "3")
        //        {
        //            string insertmasterQuery = string.Format(@"INSERT INTO MASTER_TRANSACTION(VOUCHER_NO,VOUCHER_AMOUNT,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,DELETED_FLAG,CURRENCY_CODE,CREATED_DATE,VOUCHER_DATE,SYN_ROWID,EXCHANGE_RATE,SESSION_ROWID,PRINT_COUNT) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}',TO_DATE('{8}','DD-MON-YYYY hh24:mi:ss'),{9},'{10}',{11},'{12}',{13})",
        //                   commonFieldForSales.OrderNumber, commonFieldForSales.GrandTotal, commonFieldForSales.FormCode, _workContext.CurrentUserinformation.company_code, _workContext.CurrentUserinformation.branch_code, _workContext.CurrentUserinformation.login_code, 'N', commonFieldForSales.CurrencyFormat, DateTime.Now.ToString("dd-MMM-yyyy"), commonFieldForSales.VoucherDate, commonFieldForSales.ManualNumber, commonFieldForSales.ExchangeRate, commonFieldForSales.NewVoucherNumber, 1);
        //            this._objectEntity.ExecuteSqlCommand(insertmasterQuery);
        //            insertedToMaster = true;
        //        }
        //        else
        //        {
        //            string insertmasterQuery = string.Format(@"INSERT INTO MASTER_TRANSACTION(VOUCHER_NO,VOUCHER_AMOUNT,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,DELETED_FLAG,CURRENCY_CODE,CREATED_DATE,VOUCHER_DATE,SYN_ROWID,EXCHANGE_RATE,SESSION_ROWID) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}',TO_DATE('{8}','DD-MON-YYYY hh24:mi:ss'),{9},'{10}',{11},'{12}')",
        //                        commonFieldForSales.OrderNumber, commonFieldForSales.GrandTotal, commonFieldForSales.FormCode, _workContext.CurrentUserinformation.company_code, _workContext.CurrentUserinformation.branch_code, _workContext.CurrentUserinformation.login_code, 'N', commonFieldForSales.CurrencyFormat, DateTime.Now.ToString("dd-MMM-yyyy"), commonFieldForSales.VoucherDate, commonFieldForSales.ManualNumber, commonFieldForSales.ExchangeRate, commonFieldForSales.NewVoucherNumber);
        //            this._objectEntity.ExecuteSqlCommand(insertmasterQuery);
        //            insertedToMaster = true;
        //        }
        //        return insertedToMaster;

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}


        #endregion


        #region PRIVATE METHOD TO ADD SALES RETURN

        private bool SaveSalesReturnValues(List<SalesReturnDetail> childValue, SalesReturnDetail masterValue, string orderNumber, string formCode, NeoErpCoreEntity dbcontext = null)
        {
            try
            {
                int serialNo = 1;
                bool insertedToChild = false;
                SalesReturnDetail salesChildReturn = new SalesReturnDetail();
                foreach (var cCol in childValue)
                {
                    salesChildReturn.RETURN_NO = orderNumber;
                    salesChildReturn.RETURN_DATE = string.IsNullOrEmpty(cCol.RETURN_DATE) ? masterValue.RETURN_DATE : cCol.RETURN_DATE;
                    salesChildReturn.CUSTOMER_CODE = string.IsNullOrEmpty(cCol.CUSTOMER_CODE) ? masterValue.CUSTOMER_CODE : cCol.CUSTOMER_CODE;
                    salesChildReturn.SERIAL_NO = cCol.SERIAL_NO > 0 ? cCol.SERIAL_NO : serialNo;
                    salesChildReturn.ITEM_CODE = string.IsNullOrEmpty(cCol.ITEM_CODE) ? "" : cCol.ITEM_CODE;
                    salesChildReturn.MU_CODE = string.IsNullOrEmpty(cCol.MU_CODE) ? "" : cCol.MU_CODE;
                    salesChildReturn.QUANTITY = cCol.QUANTITY > 0 ? cCol.QUANTITY : cCol.CALC_QUANTITY > 0 ? cCol.CALC_QUANTITY : 0;
                    salesChildReturn.UNIT_PRICE = cCol.UNIT_PRICE > 0 ? cCol.UNIT_PRICE : cCol.CALC_UNIT_PRICE > 0 ? cCol.CALC_UNIT_PRICE : 0;
                    salesChildReturn.TOTAL_PRICE = cCol.TOTAL_PRICE;
                    salesChildReturn.CALC_QUANTITY = cCol.CALC_QUANTITY > 0 ? cCol.CALC_QUANTITY : cCol.QUANTITY > 0 ? cCol.QUANTITY : 0;
                    salesChildReturn.CALC_UNIT_PRICE = cCol.CALC_UNIT_PRICE > 0 ? cCol.CALC_UNIT_PRICE : cCol.UNIT_PRICE > 0 ? cCol.UNIT_PRICE : 0;
                    salesChildReturn.CALC_TOTAL_PRICE = cCol.CALC_TOTAL_PRICE > 0 ? cCol.CALC_TOTAL_PRICE : cCol.TOTAL_PRICE > 0 ? cCol.TOTAL_PRICE : 0;
                    salesChildReturn.ED = _objectEntity.SqlQuery<ChargeSetup>($@"select * from charge_setup where form_code = '{formCode}' and company_code ='{_workContext.CurrentUserinformation.company_code}' and ON_ITEM = 'Y' and charge_code ='ED'").ToList().Count() > 0 ? (cCol.ED > 0 ? cCol.ED : cCol.ED > 0 ? cCol.ED : 0) : 0;
                    salesChildReturn.BC = _objectEntity.SqlQuery<ChargeSetup>($@"select * from charge_setup where form_code = '{formCode}' and company_code ='{_workContext.CurrentUserinformation.company_code}' and ON_ITEM = 'Y' and charge_code ='BC'").ToList().Count() > 0 ? (cCol.BC > 0 ? cCol.BC : cCol.BC > 0 ? cCol.BC : 0) : 0;
                    salesChildReturn.SD = _objectEntity.SqlQuery<ChargeSetup>($@"select * from charge_setup where form_code = '{formCode}' and company_code ='{_workContext.CurrentUserinformation.company_code}' and ON_ITEM = 'Y' and charge_code ='SD'").ToList().Count() > 0 ? (cCol.SD > 0 ? cCol.SD : cCol.SD > 0 ? cCol.SD : 0) : 0;
                    salesChildReturn.VT = _objectEntity.SqlQuery<ChargeSetup>($@"select * from charge_setup where form_code = '{formCode}' and company_code ='{_workContext.CurrentUserinformation.company_code}' and ON_ITEM = 'Y' and charge_code ='VT'").ToList().Count() > 0 ? (cCol.VT > 0 ? cCol.VT : cCol.VT > 0 ? cCol.VT : 0) : 0;
                    salesChildReturn.TA = cCol.TA > 0 ? cCol.TA : cCol.TA > 0 ? cCol.TA : 0;
                    salesChildReturn.NA = cCol.NA > 0 ? cCol.NA : cCol.NA > 0 ? cCol.NA : 0;
                    salesChildReturn.REMARKS = string.IsNullOrEmpty(cCol.REMARKS) ? masterValue.REMARKS : cCol.REMARKS;
                    salesChildReturn.FORM_CODE = string.IsNullOrEmpty(cCol.FORM_CODE) ? formCode : cCol.FORM_CODE;
                    salesChildReturn.COMPANY_CODE = _workContext.CurrentUserinformation.company_code;
                    salesChildReturn.BRANCH_CODE = _workContext.CurrentUserinformation.branch_code;
                    salesChildReturn.CREATED_BY = _workContext.CurrentUserinformation.login_code;
                    salesChildReturn.CREATED_DATE = DateTime.Now;
                    salesChildReturn.DELETED_FLAG = "N";
                    salesChildReturn.DELIVERY_DATE = string.IsNullOrEmpty(cCol.DELIVERY_DATE) ? masterValue.DELIVERY_DATE : cCol.DELIVERY_DATE;
                    //salesChildReturn.CURRENCY_CODE = cCol.CURRENCY_CODE;
                    //salesChildReturn.EXCHANGE_RATE = cCol.EXCHANGE_RATE;
                    salesChildReturn.DELIVERY_DATE = string.IsNullOrEmpty(cCol.DELIVERY_DATE) ? masterValue.DELIVERY_DATE : cCol.DELIVERY_DATE;
                    salesChildReturn.CURRENCY_CODE = string.IsNullOrEmpty(cCol.CURRENCY_CODE) ? "NRS" : cCol.CURRENCY_CODE;
                    salesChildReturn.EXCHANGE_RATE = cCol.EXCHANGE_RATE > 0 ? cCol.QUANTITY : 1;
                    salesChildReturn.TRACKING_NO = cCol.TRACKING_NO;
                    salesChildReturn.STOCK_BLOCK_FLAG = cCol.STOCK_BLOCK_FLAG;
                    salesChildReturn.SESSION_ROWID = cCol.SESSION_ROWID;
                    salesChildReturn.MODIFY_BY = cCol.MODIFY_BY;
                    salesChildReturn.DIVISION_CODE = cCol.DIVISION_CODE;
                    salesChildReturn.PARTY_TYPE_CODE = string.IsNullOrEmpty(masterValue.PARTY_TYPE_CODE) ? "" : masterValue.PARTY_TYPE_CODE;
                    salesChildReturn.SALES_TYPE_CODE = string.IsNullOrEmpty(cCol.SALES_TYPE_CODE) ? "" : cCol.SALES_TYPE_CODE;
                    salesChildReturn.MODIFY_DATE = DateTime.Now;
                    salesChildReturn.EMPLOYEE_CODE = string.IsNullOrEmpty(cCol.EMPLOYEE_CODE) ? masterValue.EMPLOYEE_CODE : cCol.EMPLOYEE_CODE;
                    salesChildReturn.TO_LOCATION_CODE = string.IsNullOrEmpty(cCol.TO_LOCATION_CODE) ? masterValue.TO_LOCATION_CODE : cCol.TO_LOCATION_CODE;
                    if (masterValue.SHIPPING_ADDRESS != null)
                    {
                        salesChildReturn.SHIPPING_ADDRESS = masterValue.SHIPPING_ADDRESS.Contains("'") ? masterValue.SHIPPING_ADDRESS.Replace("'", "' || '''' || '") : masterValue.SHIPPING_ADDRESS;
                    }
                    else
                    {
                        salesChildReturn.SHIPPING_ADDRESS = "";
                    }
                    //salesChildReturn.SECOND_QUANTITY = cCol.SECOND_QUANTITY > 0 ? cCol.SECOND_QUANTITY : 1;
                    //var querySalesorder = $@"Insert into SA_SALES_ORDER ({column}) Values('{value}')";
                    //var InsertedData = _objectEntity.ExecuteSqlCommand(querySalesorder);

                    var querySalesorder = $@"Insert into SA_SALES_RETURN (RETURN_NO, RETURN_DATE, CUSTOMER_CODE, SERIAL_NO, ITEM_CODE, MU_CODE, QUANTITY, UNIT_PRICE, TOTAL_PRICE, CALC_QUANTITY, CALC_UNIT_PRICE, CALC_TOTAL_PRICE, 
                      REMARKS, FORM_CODE, COMPANY_CODE, BRANCH_CODE, CREATED_BY, CREATED_DATE, DELETED_FLAG, CURRENCY_CODE, EXCHANGE_RATE,TRACKING_NO,SESSION_ROWID, MODIFY_BY, DIVISION_CODE,TO_LOCATION_CODE,PARTY_TYPE_CODE,SECOND_QUANTITY,THIRD_QUANTITY,EXCISE_ITEM_AMOUNT,BANKCHARGE_ITEM_AMOUNT,DISCOUNT_ITEM_AMOUNT,VAT_ITEM_AMOUNT,TAXABLE_AMOUNT,NET_AMOUNT) 
                     Values ('{salesChildReturn.RETURN_NO}',TO_DATE('{salesChildReturn.RETURN_DATE}','DD-MON-YYYY hh24:mi:ss'), '{salesChildReturn.CUSTOMER_CODE}', {salesChildReturn.SERIAL_NO}, 
                    '{salesChildReturn.ITEM_CODE}', '{salesChildReturn.MU_CODE}', {salesChildReturn.QUANTITY}, {salesChildReturn.UNIT_PRICE}, {salesChildReturn.TOTAL_PRICE}, 
                     {salesChildReturn.CALC_QUANTITY}, {salesChildReturn.CALC_UNIT_PRICE}, {salesChildReturn.CALC_TOTAL_PRICE}, '{salesChildReturn.REMARKS}','{salesChildReturn.FORM_CODE}', '{salesChildReturn.COMPANY_CODE}', '{salesChildReturn.BRANCH_CODE}', '{salesChildReturn.CREATED_BY}', SYSDATE, 
                    'N', '{salesChildReturn.CURRENCY_CODE}', {salesChildReturn.EXCHANGE_RATE},'{salesChildReturn.TRACKING_NO}', '{salesChildReturn.SESSION_ROWID}', '', '{salesChildReturn.DIVISION_CODE}','{salesChildReturn.TO_LOCATION_CODE}','{salesChildReturn.PARTY_TYPE_CODE}','{cCol.SECOND_QUANTITY}','{cCol.THIRD_QUANTITY}','{cCol.ED}','{cCol.BC}','{cCol.SD}','{cCol.VT}','{cCol.TA}','{cCol.NA}')";
                    var InsertedData = _objectEntity.ExecuteSqlCommand(querySalesorder);
                    serialNo++;

                    //var insertQuery = string.Format(@"insert into " + model.Table_Name + "({0}) values({1},{2},{3},{4},{5},{6},{7},'{8}',{9})", Columnbuilder, values, serialno, "'" + model.Form_Code + "'", "'" + this._workContext.CurrentUserinformation.company_code + "'", "'" + this._workContext.CurrentUserinformation.branch_code + "'", "'" + this._workContext.CurrentUserinformation.login_code + "'", createddatestring, 'N', "'" + newvoucherNo + "'");
                    // var insertQuery = $@"Insert Into {model.Table_Name} (ORDER_NO,ORDER_DATE,CUSTOMER_CODE,SERIAL_NO,ITEM_CODE,MU_CODE,QUANTITY,UNIT_PRICE,TOTAL_PRICE,
                    // CALC_QUANTITY,CALC_UNIT_PRICE,CALC_TOTAL_PRICE,REMARKS,FORM_CODE,COMPANY_CODE,BRANCH_CODE,CREATED_BY,CREATED_DATE,DELETED_FLAG,CURRENCY_CODE,EXCHANGE_RATE,
                    // TRACKING_NO,STOCK_BLOCK_FLAG,SESSION_ROWID,MODIFY_BY,DIVISION_CODE) 
                    // VALUES('{newOrderNo}','{cCol.ORDER_DATE}','{cCol.CUSTOMER_CODE}','{cCol.SERIAL_NO}','{cCol.ITEM_CODE}','{cCol.MU_CODE}','{cCol.QUANTITY}','{cCol.UNIT_PRICE}','{cCol.TOTAL_PRICE}',
                    //'{cCol.CALC_QUANTITY}','{cCol.CALC_UNIT_PRICE}','{cCol.CALC_TOTAL_PRICE}','{cCol.REMARKS}','{cCol.FORM_CODE}','{cCol.COMPANY_CODE}','{cCol.BRANCH_CODE}','{cCol.CREATED_BY}','{cCol.CREATED_DATE}',
                    //'N','{cCol.CURRENCY_CODE}','{cCol.EXCHANGE_RATE}','{cCol.TRACKING_NO}','{cCol.STOCK_BLOCK_FLAG}',
                    // '{cCol.SESSION_ROWID}','{cCol.MODIFY_BY}','{cCol.DIVISION_CODE}')";
                    // this._objectEntity.ExecuteSqlCommand(insertQuery);
                    // insertedToChild = true;
                }


                insertedToChild = true;
                return insertedToChild;


            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        private bool UpdateSalesReturnValues(List<SalesReturnDetail> childValue, SalesReturnDetail masterValue, string orderNumber, string formCode, NeoErpCoreEntity dbcontext = null)
        {
            try
            {
                int serialNo = 1;
                bool updatedToChild = false;
                SalesReturnDetail salesChildReturn = new SalesReturnDetail();
                foreach (var cCol in childValue)
                {
                    salesChildReturn.RETURN_NO = orderNumber;
                    salesChildReturn.RETURN_DATE = string.IsNullOrEmpty(cCol.RETURN_DATE) ? masterValue.RETURN_DATE : cCol.RETURN_DATE;
                    salesChildReturn.CUSTOMER_CODE = string.IsNullOrEmpty(cCol.CUSTOMER_CODE) ? masterValue.CUSTOMER_CODE : cCol.CUSTOMER_CODE;
                    salesChildReturn.SERIAL_NO = cCol.SERIAL_NO > 0 ? cCol.SERIAL_NO : serialNo;
                    salesChildReturn.ITEM_CODE = string.IsNullOrEmpty(cCol.ITEM_CODE) ? "" : cCol.ITEM_CODE;
                    salesChildReturn.MU_CODE = string.IsNullOrEmpty(cCol.MU_CODE) ? "" : cCol.MU_CODE;
                    salesChildReturn.QUANTITY = cCol.QUANTITY > 0 ? cCol.QUANTITY : cCol.CALC_QUANTITY > 0 ? cCol.CALC_QUANTITY : 1;
                    salesChildReturn.UNIT_PRICE = cCol.UNIT_PRICE > 0 ? cCol.UNIT_PRICE : cCol.CALC_UNIT_PRICE > 0 ? cCol.CALC_UNIT_PRICE : 1;
                    salesChildReturn.TOTAL_PRICE = cCol.TOTAL_PRICE;
                    salesChildReturn.CALC_QUANTITY = cCol.CALC_QUANTITY > 0 ? cCol.CALC_QUANTITY : cCol.QUANTITY > 0 ? cCol.QUANTITY : 1;
                    salesChildReturn.CALC_UNIT_PRICE = cCol.CALC_UNIT_PRICE > 0 ? cCol.CALC_UNIT_PRICE : cCol.UNIT_PRICE > 0 ? cCol.UNIT_PRICE : 1;
                    salesChildReturn.CALC_TOTAL_PRICE = cCol.CALC_TOTAL_PRICE > 0 ? cCol.CALC_TOTAL_PRICE : cCol.TOTAL_PRICE > 0 ? cCol.TOTAL_PRICE : 1;
                    salesChildReturn.REMARKS = string.IsNullOrEmpty(cCol.REMARKS) ? masterValue.REMARKS : cCol.REMARKS;
                    salesChildReturn.FORM_CODE = string.IsNullOrEmpty(cCol.FORM_CODE) ? formCode : cCol.FORM_CODE;
                    salesChildReturn.COMPANY_CODE = _workContext.CurrentUserinformation.company_code;
                    salesChildReturn.BRANCH_CODE = _workContext.CurrentUserinformation.branch_code;
                    salesChildReturn.CREATED_BY = _workContext.CurrentUserinformation.login_code;
                    salesChildReturn.CREATED_DATE = DateTime.Now;
                    salesChildReturn.DELETED_FLAG = "N";
                    salesChildReturn.DELIVERY_DATE = string.IsNullOrEmpty(cCol.DELIVERY_DATE) ? masterValue.DELIVERY_DATE : cCol.DELIVERY_DATE;
                    //salesChildReturn.CURRENCY_CODE = cCol.CURRENCY_CODE;
                    //salesChildReturn.EXCHANGE_RATE = cCol.EXCHANGE_RATE;
                    salesChildReturn.DELIVERY_DATE = string.IsNullOrEmpty(cCol.DELIVERY_DATE) ? masterValue.DELIVERY_DATE : cCol.DELIVERY_DATE;
                    salesChildReturn.CURRENCY_CODE = string.IsNullOrEmpty(cCol.CURRENCY_CODE) ? "NRS" : cCol.CURRENCY_CODE;
                    salesChildReturn.EXCHANGE_RATE = cCol.EXCHANGE_RATE > 0 ? cCol.QUANTITY : 1;
                    salesChildReturn.TRACKING_NO = cCol.TRACKING_NO;
                    salesChildReturn.STOCK_BLOCK_FLAG = cCol.STOCK_BLOCK_FLAG;
                    salesChildReturn.SESSION_ROWID = cCol.SESSION_ROWID;
                    salesChildReturn.MODIFY_BY = cCol.MODIFY_BY;
                    salesChildReturn.DIVISION_CODE = cCol.DIVISION_CODE;
                    salesChildReturn.PARTY_TYPE_CODE = string.IsNullOrEmpty(masterValue.PARTY_TYPE_CODE) ? "" : masterValue.PARTY_TYPE_CODE;
                    salesChildReturn.SALES_TYPE_CODE = string.IsNullOrEmpty(cCol.SALES_TYPE_CODE) ? "" : cCol.SALES_TYPE_CODE;
                    salesChildReturn.MODIFY_DATE = DateTime.Now;
                    salesChildReturn.EMPLOYEE_CODE = string.IsNullOrEmpty(cCol.EMPLOYEE_CODE) ? masterValue.EMPLOYEE_CODE : cCol.EMPLOYEE_CODE;
                    salesChildReturn.TO_LOCATION_CODE = string.IsNullOrEmpty(cCol.TO_LOCATION_CODE) ? masterValue.TO_LOCATION_CODE : cCol.TO_LOCATION_CODE;



                    //var querySalesorder = $@"Insert into SA_SALES_RETURN (RETURN_NO, RETURN_DATE, CUSTOMER_CODE, SERIAL_NO, ITEM_CODE, MU_CODE, QUANTITY, UNIT_PRICE, TOTAL_PRICE, CALC_QUANTITY, CALC_UNIT_PRICE, CALC_TOTAL_PRICE, 
                    //  REMARKS, FORM_CODE, COMPANY_CODE, BRANCH_CODE, CREATED_BY, CREATED_DATE, DELETED_FLAG, CURRENCY_CODE, EXCHANGE_RATE,TRACKING_NO,SESSION_ROWID, MODIFY_BY, DIVISION_CODE,TO_LOCATION_CODE) 
                    // Values ('{salesChildReturn.RETURN_NO}',TO_DATE('{salesChildReturn.RETURN_DATE}','DD-MON-YYYY hh24:mi:ss'), '{salesChildReturn.CUSTOMER_CODE}', {salesChildReturn.SERIAL_NO}, 
                    //'{salesChildReturn.ITEM_CODE}', '{salesChildReturn.MU_CODE}', {salesChildReturn.QUANTITY}, {salesChildReturn.UNIT_PRICE}, {salesChildReturn.TOTAL_PRICE}, 
                    // {salesChildReturn.CALC_QUANTITY}, {salesChildReturn.CALC_UNIT_PRICE}, {salesChildReturn.CALC_TOTAL_PRICE}, '{salesChildReturn.REMARKS}','{salesChildReturn.FORM_CODE}', '{salesChildReturn.COMPANY_CODE}', '{salesChildReturn.BRANCH_CODE}', '{salesChildReturn.CREATED_BY}', SYSDATE, 
                    //'N', '{salesChildReturn.CURRENCY_CODE}', {salesChildReturn.EXCHANGE_RATE},'{salesChildReturn.TRACKING_NO}', '{salesChildReturn.SESSION_ROWID}', '', '{salesChildReturn.DIVISION_CODE}','{salesChildReturn.TO_LOCATION_CODE}')";
                    //var InsertedData = _objectEntity.ExecuteSqlCommand(querySalesorder);
                    var querySalesorder = $@"UPDATE SA_SALES_RETURN SET RETURN_DATE=TO_DATE('{salesChildReturn.RETURN_DATE}','DD-MON-YYYY hh24:mi:ss'),CUSTOMER_CODE='{salesChildReturn.CUSTOMER_CODE}',ITEM_CODE='{salesChildReturn.ITEM_CODE}',MU_CODE='{salesChildReturn.MU_CODE}',CURRENCY_CODE='{salesChildReturn.CURRENCY_CODE}',EXCHANGE_RATE={salesChildReturn.EXCHANGE_RATE},MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',MODIFY_DATE=SYSDATE,DIVISION_CODE='{salesChildReturn.DIVISION_CODE}',TO_LOCATION_CODE='{salesChildReturn.TO_LOCATION_CODE}',PARTY_TYPE_CODE='{salesChildReturn.PARTY_TYPE_CODE}' WHERE RETURN_NO='{salesChildReturn.RETURN_NO}' AND SERIAL_NO={serialNo} ";
                    var InsertedData = _objectEntity.ExecuteSqlCommand(querySalesorder);
                    serialNo++;


                }


                updatedToChild = true;
                return updatedToChild;


            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }


        #endregion


        #region PRIVATE METHOD TO ADD SALES INVOICE

        private bool SaveSalesInvoiceValues(List<SalesInvoiceDetail> childValue, SalesInvoiceDetail masterValue, string orderNumber, string formCode)
        {
            try
            {
                int serialNo = 1;
                bool insertedToChild = false;
                SalesInvoiceDetail salesChildInvoice = new SalesInvoiceDetail();
                foreach (var cCol in childValue)
                {
                    salesChildInvoice.SALES_NO = orderNumber;
                    salesChildInvoice.SALES_DATE = string.IsNullOrEmpty(cCol.SALES_DATE) ? masterValue.SALES_DATE : cCol.SALES_DATE;
                    salesChildInvoice.CUSTOMER_CODE = string.IsNullOrEmpty(cCol.CUSTOMER_CODE) ? masterValue.CUSTOMER_CODE : cCol.CUSTOMER_CODE;
                    salesChildInvoice.SERIAL_NO = cCol.SERIAL_NO > 0 ? cCol.SERIAL_NO : serialNo;
                    salesChildInvoice.ITEM_CODE = string.IsNullOrEmpty(cCol.ITEM_CODE) ? "" : cCol.ITEM_CODE;
                    salesChildInvoice.MU_CODE = string.IsNullOrEmpty(cCol.MU_CODE) ? "" : cCol.MU_CODE;
                    salesChildInvoice.QUANTITY = cCol.QUANTITY > 0 ? cCol.QUANTITY : cCol.CALC_QUANTITY > 0 ? cCol.CALC_QUANTITY :0;

                    salesChildInvoice.SECOND_QUANTITY = cCol.SECOND_QUANTITY > 0 ? cCol.SECOND_QUANTITY : 0;
                    salesChildInvoice.THIRD_QUANTITY = cCol.THIRD_QUANTITY > 0 ? cCol.THIRD_QUANTITY  : 0;

                    salesChildInvoice.UNIT_PRICE = cCol.UNIT_PRICE > 0 ? cCol.UNIT_PRICE : cCol.CALC_UNIT_PRICE > 0 ? cCol.CALC_UNIT_PRICE : 0;
                    salesChildInvoice.TOTAL_PRICE = cCol.TOTAL_PRICE;
                    salesChildInvoice.CALC_QUANTITY = cCol.CALC_QUANTITY > 0 ? cCol.CALC_QUANTITY : cCol.QUANTITY > 0 ? cCol.QUANTITY : 0;
                    salesChildInvoice.CALC_UNIT_PRICE = cCol.CALC_UNIT_PRICE > 0 ? cCol.CALC_UNIT_PRICE : cCol.UNIT_PRICE > 0 ? cCol.UNIT_PRICE : 0;
                    salesChildInvoice.CALC_TOTAL_PRICE = cCol.CALC_TOTAL_PRICE > 0 ? cCol.CALC_TOTAL_PRICE : cCol.TOTAL_PRICE > 0 ? cCol.TOTAL_PRICE : 0;
                    salesChildInvoice.ED = _objectEntity.SqlQuery<ChargeSetup>($@"select * from charge_setup where form_code = '{formCode}' and company_code ='{_workContext.CurrentUserinformation.company_code}' and ON_ITEM = 'Y' and charge_code ='ED'").ToList().Count() > 0 ? (cCol.ED > 0 ? cCol.ED : cCol.ED > 0 ? cCol.ED : 0) : 0;
                    salesChildInvoice.SD = _objectEntity.SqlQuery<ChargeSetup>($@"select * from charge_setup where form_code = '{formCode}' and company_code ='{_workContext.CurrentUserinformation.company_code}' and ON_ITEM = 'Y' and charge_code ='SD'").ToList().Count() > 0 ? (cCol.SD > 0 ? cCol.SD : cCol.SD > 0 ? cCol.SD : 0) : 0;
                    
                    salesChildInvoice.BC = _objectEntity.SqlQuery<ChargeSetup>($@"select * from charge_setup where form_code = '{formCode}' and company_code ='{_workContext.CurrentUserinformation.company_code}' and ON_ITEM = 'Y' and charge_code ='BC'").ToList().Count() > 0 ? (cCol.BC > 0 ? cCol.BC : cCol.BC > 0 ? cCol.BC : 0) : 0;
                    salesChildInvoice.VT = _objectEntity.SqlQuery<ChargeSetup>($@"select * from charge_setup where form_code = '{formCode}' and company_code ='{_workContext.CurrentUserinformation.company_code}' and ON_ITEM = 'Y' and charge_code ='VT'").ToList().Count() > 0 ? (cCol.VT > 0 ? cCol.VT : cCol.VT > 0 ? cCol.VT : 0) : 0;
                    salesChildInvoice.TA = cCol.TA > 0 ? cCol.TA : cCol.TA > 0 ? cCol.TA : 0;
                    salesChildInvoice.NA = cCol.NA > 0 ? cCol.NA : cCol.NA > 0 ? cCol.NA : 0;
                    salesChildInvoice.REMARKS = string.IsNullOrEmpty(cCol.REMARKS) ? masterValue.REMARKS : cCol.REMARKS;
                    salesChildInvoice.FORM_CODE = string.IsNullOrEmpty(cCol.FORM_CODE) ? formCode : cCol.FORM_CODE;
                    salesChildInvoice.COMPANY_CODE = _workContext.CurrentUserinformation.company_code;
                    salesChildInvoice.BRANCH_CODE = _workContext.CurrentUserinformation.branch_code;
                    salesChildInvoice.CREATED_BY = _workContext.CurrentUserinformation.login_code;
                    salesChildInvoice.CREATED_DATE = DateTime.Now;
                    salesChildInvoice.DELETED_FLAG = "N";
                    salesChildInvoice.DELIVERY_DATE = string.IsNullOrEmpty(cCol.DELIVERY_DATE) ? masterValue.DELIVERY_DATE : cCol.DELIVERY_DATE;
                    salesChildInvoice.CURRENCY_CODE = string.IsNullOrEmpty(cCol.CURRENCY_CODE) ? "NRS" : cCol.CURRENCY_CODE;
                    salesChildInvoice.EXCHANGE_RATE = cCol.EXCHANGE_RATE > 0 ? cCol.QUANTITY : 1;
                    salesChildInvoice.TRACKING_NO = cCol.TRACKING_NO;
                    salesChildInvoice.STOCK_BLOCK_FLAG = cCol.STOCK_BLOCK_FLAG;
                    salesChildInvoice.SESSION_ROWID = cCol.SESSION_ROWID;
                    salesChildInvoice.MODIFY_BY = cCol.MODIFY_BY;
                    salesChildInvoice.DIVISION_CODE = string.IsNullOrEmpty(cCol.DIVISION_CODE) ? masterValue.DIVISION_CODE : cCol.DIVISION_CODE;
                    salesChildInvoice.PARTY_TYPE_CODE = string.IsNullOrEmpty(cCol.PARTY_TYPE_CODE) ? masterValue.PARTY_TYPE_CODE : masterValue.PARTY_TYPE_CODE;
                    salesChildInvoice.SALES_TYPE_CODE = string.IsNullOrEmpty(cCol.SALES_TYPE_CODE) ? masterValue.SALES_TYPE_CODE : cCol.SALES_TYPE_CODE;
                    salesChildInvoice.MODIFY_DATE = DateTime.Now;
                    salesChildInvoice.EMPLOYEE_CODE = string.IsNullOrEmpty(cCol.EMPLOYEE_CODE) ? masterValue.EMPLOYEE_CODE : cCol.EMPLOYEE_CODE;
                    //salesChildInvoice.SHIPPING_ADDRESS = masterValue.SHIPPING_ADDRESS;
                    //if (masterValue.SHIPPING_ADDRESS != null)
                    //{
                    //    salesChildInvoice.SHIPPING_ADDRESS = masterValue.SHIPPING_ADDRESS.Contains("'") ? masterValue.SHIPPING_ADDRESS.Replace("'", "' || '''' || '") : masterValue.SHIPPING_ADDRESS;
                    //}
                    if (masterValue.SHIPPING_ADDRESS != null)
                    {
                        salesChildInvoice.SHIPPING_ADDRESS = masterValue.SHIPPING_ADDRESS.Contains("'") ? masterValue.SHIPPING_ADDRESS.Replace("'", "' || '''' || '") : masterValue.SHIPPING_ADDRESS;
                    }
                    else
                    {
                        salesChildInvoice.SHIPPING_ADDRESS = "";
                    }
                    salesChildInvoice.SHIPPING_CONTACT_NO = masterValue.SHIPPING_CONTACT_NO;
                    salesChildInvoice.AREA_CODE = masterValue.AREA_CODE;
                    salesChildInvoice.PRIORITY_CODE = string.IsNullOrEmpty(cCol.PRIORITY_CODE) ? masterValue.PRIORITY_CODE : cCol.PRIORITY_CODE;
                    salesChildInvoice.LINE_ITEM_DISCOUNT = masterValue.LINE_ITEM_DISCOUNT;
                    salesChildInvoice.FROM_LOCATION_CODE = string.IsNullOrEmpty(cCol.FROM_LOCATION_CODE) ? masterValue.FROM_LOCATION_CODE : cCol.FROM_LOCATION_CODE;
                    salesChildInvoice.MANUAL_NO = string.IsNullOrEmpty(cCol.MANUAL_NO) ? masterValue.MANUAL_NO : cCol.MANUAL_NO;
                    salesChildInvoice.BUDGET_FLAG = string.IsNullOrEmpty(cCol.BUDGET_FLAG) ? masterValue.BUDGET_FLAG : cCol.BUDGET_FLAG;
                    salesChildInvoice.CREDIT_DAYS = string.IsNullOrEmpty(cCol.CREDIT_DAYS) ? masterValue.CREDIT_DAYS : cCol.CREDIT_DAYS;
                    salesChildInvoice.BATCH_NO = string.IsNullOrEmpty(cCol.BATCH_NO) ? masterValue.BATCH_NO : cCol.BATCH_NO;
                    salesChildInvoice.PAYMENT_MODE = string.IsNullOrEmpty(cCol.PAYMENT_MODE) ? masterValue.PAYMENT_MODE : cCol.PAYMENT_MODE;
                    salesChildInvoice.MEMBER_SHIP_CARD = string.IsNullOrEmpty(cCol.MEMBER_SHIP_CARD) ? masterValue.MEMBER_SHIP_CARD : cCol.MEMBER_SHIP_CARD;
                    salesChildInvoice.PAYMODE_VALUE = string.IsNullOrEmpty(cCol.PAYMODE_VALUE) ? masterValue.PAYMODE_VALUE : cCol.PAYMODE_VALUE;
                    salesChildInvoice.REASON = string.IsNullOrEmpty(cCol.REASON) ? masterValue.REASON : cCol.REASON;
                    salesChildInvoice.MISC_CODE = string.IsNullOrEmpty(cCol.MISC_CODE) ? masterValue.MISC_CODE : cCol.MISC_CODE;
                    salesChildInvoice.AGENT_CODE = string.IsNullOrEmpty(cCol.AGENT_CODE) ? masterValue.AGENT_CODE : cCol.AGENT_CODE;
                    salesChildInvoice.SECTOR_CODE = string.IsNullOrEmpty(cCol.SECTOR_CODE) ? masterValue.SECTOR_CODE : cCol.SECTOR_CODE;
                    //salesChildInvoice.SECOND_QUANTITY = cCol.SECOND_QUANTITY > 0 ? cCol.SECOND_QUANTITY : 1;

                    
                   
                    //     salesChildInvoice.pay


                    //var querySalesorder = $@"Insert into SA_SALES_ORDER ({column}) Values('{value}')";
                    //var InsertedData = _objectEntity.ExecuteSqlCommand(querySalesorder);


                    var querySalesorder = $@"Insert into SA_SALES_INVOICE (SALES_NO, SALES_DATE, CUSTOMER_CODE, SERIAL_NO, ITEM_CODE, MU_CODE,QUANTITY, UNIT_PRICE, TOTAL_PRICE, CALC_QUANTITY, CALC_UNIT_PRICE, CALC_TOTAL_PRICE, 
                      REMARKS, FORM_CODE, COMPANY_CODE, BRANCH_CODE, CREATED_BY, CREATED_DATE, DELETED_FLAG,CURRENCY_CODE, EXCHANGE_RATE,TRACKING_NO,SESSION_ROWID, MODIFY_BY, DIVISION_CODE,MANUAL_NO,BUDGET_FLAG,CREDIT_DAYS,
                           SYN_ROWID,FROM_LOCATION_CODE, BATCH_NO,SMS_FLAG,DESCRIPTION,PAYMENT_MODE,PRIORITY_CODE,MEMBER_SHIP_CARD,PAYMODE_VALUE,
                            SHIPPING_ADDRESS,SHIPPING_CONTACT_NO,SALES_TYPE_CODE,EMPLOYEE_CODE,REASON,MISC_CODE,AGENT_CODE,AREA_CODE,SECTOR_CODE,PARTY_TYPE_CODE,SECOND_QUANTITY,THIRD_QUANTITY,EXCISE_ITEM_AMOUNT,BANKCHARGE_ITEM_AMOUNT,DISCOUNT_ITEM_AMOUNT,VAT_ITEM_AMOUNT,TAXABLE_AMOUNT,NET_AMOUNT) 
                     Values ('{salesChildInvoice.SALES_NO}',TO_DATE('{salesChildInvoice.SALES_DATE}','DD-MON-YYYY hh24:mi:ss'), '{salesChildInvoice.CUSTOMER_CODE}', {salesChildInvoice.SERIAL_NO}, 
                    '{salesChildInvoice.ITEM_CODE}', '{salesChildInvoice.MU_CODE}', {salesChildInvoice.QUANTITY}, {salesChildInvoice.UNIT_PRICE}, {salesChildInvoice.TOTAL_PRICE}, 
                     {salesChildInvoice.CALC_QUANTITY}, {salesChildInvoice.CALC_UNIT_PRICE}, {salesChildInvoice.CALC_TOTAL_PRICE}, '{salesChildInvoice.REMARKS}','{salesChildInvoice.FORM_CODE}', '{salesChildInvoice.COMPANY_CODE}', '{salesChildInvoice.BRANCH_CODE}', '{salesChildInvoice.CREATED_BY}', SYSDATE, 
                    'N','{salesChildInvoice.CURRENCY_CODE}', {salesChildInvoice.EXCHANGE_RATE},'{salesChildInvoice.TRACKING_NO}', '{salesChildInvoice.SESSION_ROWID}', '', '{salesChildInvoice.DIVISION_CODE}','{salesChildInvoice.MANUAL_NO}','L','{salesChildInvoice.CREDIT_DAYS}'
                       ,'{salesChildInvoice.SYN_ROWID}','{salesChildInvoice.FROM_LOCATION_CODE}','{salesChildInvoice.BATCH_NO}','N','{salesChildInvoice.DESCRIPTION}','{salesChildInvoice.PAYMENT_MODE}','{salesChildInvoice.PRIORITY_CODE}','{salesChildInvoice.MEMBER_SHIP_CARD}','{salesChildInvoice.PAYMODE_VALUE}',
                           '{salesChildInvoice.SHIPPING_ADDRESS}','{salesChildInvoice.SHIPPING_CONTACT_NO}','{salesChildInvoice.SALES_TYPE_CODE}','{salesChildInvoice.EMPLOYEE_CODE}','{salesChildInvoice.REASON}','{salesChildInvoice.MISC_CODE}','{salesChildInvoice.AGENT_CODE}','{salesChildInvoice.AREA_CODE}','{salesChildInvoice.SECTOR_CODE}','{salesChildInvoice.PARTY_TYPE_CODE}','{cCol.SECOND_QUANTITY}',{cCol.THIRD_QUANTITY},'{cCol.ED}','{cCol.BC}','{cCol.SD}','{cCol.VT}','{cCol.TA}','{cCol.NA}' )";
                    var InsertedData = _objectEntity.ExecuteSqlCommand(querySalesorder);
                    serialNo++;


                    //var querySalesorder = $@"Insert into SA_SALES_INVOICE (SALES_NO, SALES_DATE, CUSTOMER_CODE, SERIAL_NO, ITEM_CODE, MU_CODE, QUANTITY, UNIT_PRICE, TOTAL_PRICE, CALC_QUANTITY, CALC_UNIT_PRICE, CALC_TOTAL_PRICE, 
                    //  REMARKS, FORM_CODE, COMPANY_CODE, BRANCH_CODE, CREATED_BY, CREATED_DATE, DELETED_FLAG,CURRENCY_CODE, EXCHANGE_RATE,TRACKING_NO,SESSION_ROWID, MODIFY_BY, DIVISION_CODE,MANUAL_NO,BUDGET_FLAG,CREDIT_DAYS,
                    //       SYN_ROWID,FROM_LOCATION_CODE, BATCH_NO,SMS_FLAG,DESCRIPTION,PAYMENT_MODE,PRIORITY_CODE,MEMBER_SHIP_CARD,PAYMODE_VALUE,
                    //        SHIPPING_ADDRESS,SHIPPING_CONTACT_NO,SALES_TYPE_CODE,EMPLOYEE_CODE,REASON,MISC_CODE,AGENT_CODE,AREA_CODE,SECTOR_CODE,PARTY_TYPE_CODE,SECOND_QUANTITY) 
                    // Values ('{salesChildInvoice.SALES_NO}',TO_DATE('{salesChildInvoice.SALES_DATE}','DD-MON-YYYY hh24:mi:ss'), '{salesChildInvoice.CUSTOMER_CODE}', {salesChildInvoice.SERIAL_NO}, 
                    //'{salesChildInvoice.ITEM_CODE}', '{salesChildInvoice.MU_CODE}', {salesChildInvoice.QUANTITY}, {salesChildInvoice.UNIT_PRICE}, {salesChildInvoice.TOTAL_PRICE}, 
                    // {salesChildInvoice.CALC_QUANTITY}, {salesChildInvoice.CALC_UNIT_PRICE}, {salesChildInvoice.CALC_TOTAL_PRICE}, '{salesChildInvoice.REMARKS}','{salesChildInvoice.FORM_CODE}', '{salesChildInvoice.COMPANY_CODE}', '{salesChildInvoice.BRANCH_CODE}', '{salesChildInvoice.CREATED_BY}', SYSDATE, 
                    //'N','{salesChildInvoice.CURRENCY_CODE}', {salesChildInvoice.EXCHANGE_RATE},'{salesChildInvoice.TRACKING_NO}', '{salesChildInvoice.SESSION_ROWID}', '', '{salesChildInvoice.DIVISION_CODE}','{salesChildInvoice.MANUAL_NO}','L','{salesChildInvoice.CREDIT_DAYS}'
                    //   ,'{salesChildInvoice.SYN_ROWID}','{salesChildInvoice.FROM_LOCATION_CODE}','{salesChildInvoice.BATCH_NO}','N','{salesChildInvoice.DESCRIPTION}','{salesChildInvoice.PAYMENT_MODE}','{salesChildInvoice.PRIORITY_CODE}','{salesChildInvoice.MEMBER_SHIP_CARD}','{salesChildInvoice.PAYMODE_VALUE}',
                    //       '{salesChildInvoice.SHIPPING_ADDRESS}','{salesChildInvoice.SHIPPING_CONTACT_NO}','{salesChildInvoice.SALES_TYPE_CODE}','{salesChildInvoice.EMPLOYEE_CODE}','{salesChildInvoice.REASON}','{salesChildInvoice.MISC_CODE}','{salesChildInvoice.AGENT_CODE}','{salesChildInvoice.AREA_CODE}','{salesChildInvoice.SECTOR_CODE}','{salesChildInvoice.PARTY_TYPE_CODE}','{cCol.SECOND_QUANTITY}' )";
                    //var InsertedData = _objectEntity.ExecuteSqlCommand(querySalesorder);
                    //serialNo++;
                }


                insertedToChild = true;
                return insertedToChild;


            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
        private bool UpdateSalesInvoiceValues(List<SalesInvoiceDetail> childValue, SalesInvoiceDetail masterValue, string orderNumber, string formCode)
        {
            try
            {
                int serialNo = 1;
                bool updatedToChild = false;
                SalesInvoiceDetail salesChildInvoice = new SalesInvoiceDetail();
                foreach (var cCol in childValue)
                {
                    salesChildInvoice.SALES_NO = string.IsNullOrEmpty(cCol.SALES_NO) ? orderNumber : cCol.SALES_NO; ;
                    salesChildInvoice.SALES_DATE = string.IsNullOrEmpty(cCol.SALES_DATE) ? masterValue.SALES_DATE : cCol.SALES_DATE;
                    salesChildInvoice.CUSTOMER_CODE = string.IsNullOrEmpty(cCol.CUSTOMER_CODE) ? masterValue.CUSTOMER_CODE : cCol.CUSTOMER_CODE;
                    salesChildInvoice.SERIAL_NO = cCol.SERIAL_NO > 0 ? cCol.SERIAL_NO : serialNo;
                    salesChildInvoice.ITEM_CODE = string.IsNullOrEmpty(cCol.ITEM_CODE) ? "" : cCol.ITEM_CODE;
                    salesChildInvoice.MU_CODE = string.IsNullOrEmpty(cCol.MU_CODE) ? "" : cCol.MU_CODE;
                    salesChildInvoice.QUANTITY = cCol.QUANTITY > 0 ? cCol.QUANTITY : cCol.CALC_QUANTITY > 0 ? cCol.CALC_QUANTITY : 1;
                    salesChildInvoice.UNIT_PRICE = cCol.UNIT_PRICE > 0 ? cCol.UNIT_PRICE : cCol.CALC_UNIT_PRICE > 0 ? cCol.CALC_UNIT_PRICE : 1;
                    salesChildInvoice.TOTAL_PRICE = cCol.TOTAL_PRICE;
                    salesChildInvoice.ED = cCol.ED;
                    salesChildInvoice.BC = cCol.BC;
                    salesChildInvoice.SD = cCol.SD;
                    salesChildInvoice.VT = cCol.VT;
                    salesChildInvoice.TA = cCol.TA;
                    salesChildInvoice.NA = cCol.NA;
                    salesChildInvoice.CALC_QUANTITY = cCol.CALC_QUANTITY > 0 ? cCol.CALC_QUANTITY : cCol.QUANTITY > 0 ? cCol.QUANTITY : 1;
                    salesChildInvoice.CALC_UNIT_PRICE = cCol.CALC_UNIT_PRICE > 0 ? cCol.CALC_UNIT_PRICE : cCol.UNIT_PRICE > 0 ? cCol.UNIT_PRICE : 1;
                    salesChildInvoice.CALC_TOTAL_PRICE = cCol.CALC_TOTAL_PRICE > 0 ? cCol.CALC_TOTAL_PRICE : cCol.TOTAL_PRICE > 0 ? cCol.TOTAL_PRICE : 1;
                    salesChildInvoice.REMARKS = string.IsNullOrEmpty(cCol.REMARKS) ? masterValue.REMARKS : cCol.REMARKS;
                    salesChildInvoice.FORM_CODE = string.IsNullOrEmpty(cCol.FORM_CODE) ? formCode : cCol.FORM_CODE;
                    salesChildInvoice.COMPANY_CODE = _workContext.CurrentUserinformation.company_code;
                    salesChildInvoice.BRANCH_CODE = _workContext.CurrentUserinformation.branch_code;
                    salesChildInvoice.CREATED_BY = _workContext.CurrentUserinformation.login_code;
                    salesChildInvoice.CREATED_DATE = DateTime.Now;
                    salesChildInvoice.DELETED_FLAG = "N";
                    salesChildInvoice.DELIVERY_DATE = string.IsNullOrEmpty(cCol.DELIVERY_DATE) ? masterValue.DELIVERY_DATE : cCol.DELIVERY_DATE;
                    salesChildInvoice.CURRENCY_CODE = string.IsNullOrEmpty(cCol.CURRENCY_CODE) ? "NRS" : cCol.CURRENCY_CODE;
                    salesChildInvoice.EXCHANGE_RATE = cCol.EXCHANGE_RATE > 0 ? cCol.QUANTITY : 1;
                    salesChildInvoice.TRACKING_NO = cCol.TRACKING_NO;
                    salesChildInvoice.STOCK_BLOCK_FLAG = cCol.STOCK_BLOCK_FLAG;
                    salesChildInvoice.SESSION_ROWID = cCol.SESSION_ROWID;
                    salesChildInvoice.MODIFY_BY = cCol.MODIFY_BY;
                    salesChildInvoice.DIVISION_CODE = cCol.DIVISION_CODE;
                    salesChildInvoice.PARTY_TYPE_CODE = string.IsNullOrEmpty(masterValue.PARTY_TYPE_CODE) ? "" : masterValue.PARTY_TYPE_CODE;
                    salesChildInvoice.SALES_TYPE_CODE = string.IsNullOrEmpty(cCol.SALES_TYPE_CODE) ? "" : cCol.SALES_TYPE_CODE;
                    salesChildInvoice.MODIFY_DATE = DateTime.Now;
                    salesChildInvoice.EMPLOYEE_CODE = string.IsNullOrEmpty(cCol.EMPLOYEE_CODE) ? masterValue.EMPLOYEE_CODE : cCol.EMPLOYEE_CODE;

                    //var querySalesorder = $@"Insert into SA_SALES_ORDER ({column}) Values('{value}')";
                    //var InsertedData = _objectEntity.ExecuteSqlCommand(querySalesorder);

                    //var querySalesorder = $@"Insert into SA_SALES_INVOICE (SALES_NO, SALES_DATE, CUSTOMER_CODE, SERIAL_NO, ITEM_CODE, MU_CODE, QUANTITY, UNIT_PRICE, TOTAL_PRICE, CALC_QUANTITY, CALC_UNIT_PRICE, CALC_TOTAL_PRICE, 
                    //  REMARKS, FORM_CODE, COMPANY_CODE, BRANCH_CODE, CREATED_BY, CREATED_DATE, DELETED_FLAG,CURRENCY_CODE, EXCHANGE_RATE,TRACKING_NO,SESSION_ROWID, MODIFY_BY, DIVISION_CODE) 
                    // Values ('{salesChildInvoice.SALES_NO}',TO_DATE('{salesChildInvoice.SALES_DATE}','DD-MON-YYYY hh24:mi:ss'), '{salesChildInvoice.CUSTOMER_CODE}', {salesChildInvoice.SERIAL_NO}, 
                    //'{salesChildInvoice.ITEM_CODE}', '{salesChildInvoice.MU_CODE}', {salesChildInvoice.QUANTITY}, {salesChildInvoice.UNIT_PRICE}, {salesChildInvoice.TOTAL_PRICE}, 
                    // {salesChildInvoice.CALC_QUANTITY}, {salesChildInvoice.CALC_UNIT_PRICE}, {salesChildInvoice.CALC_TOTAL_PRICE}, '{salesChildInvoice.REMARKS}','{salesChildInvoice.FORM_CODE}', '{salesChildInvoice.COMPANY_CODE}', '{salesChildInvoice.BRANCH_CODE}', '{salesChildInvoice.CREATED_BY}', SYSDATE, 
                    //'N','{salesChildInvoice.CURRENCY_CODE}', {salesChildInvoice.EXCHANGE_RATE},'{salesChildInvoice.TRACKING_NO}', '{salesChildInvoice.SESSION_ROWID}', '', '{salesChildInvoice.DIVISION_CODE}')";
                    //var InsertedData = _objectEntity.ExecuteSqlCommand(querySalesorder);
                    var querySalesorder = $@"UPDATE SA_SALES_INVOICE SET SALES_DATE=TO_DATE('{salesChildInvoice.SALES_DATE}','DD-MON-YYYY hh24:mi:ss'), CUSTOMER_CODE='{salesChildInvoice.CUSTOMER_CODE}',ITEM_CODE='{salesChildInvoice.ITEM_CODE}',MU_CODE='{salesChildInvoice.MU_CODE}',QUANTITY={salesChildInvoice.QUANTITY},UNIT_PRICE={salesChildInvoice.UNIT_PRICE},TOTAL_PRICE={salesChildInvoice.TOTAL_PRICE},CALC_QUANTITY={salesChildInvoice.CALC_QUANTITY},CALC_UNIT_PRICE={salesChildInvoice.CALC_UNIT_PRICE},CALC_TOTAL_PRICE={salesChildInvoice.CALC_TOTAL_PRICE},EXCISE_ITEM_AMOUNT={salesChildInvoice.ED},BANKCHARGE_ITEM_AMOUNT={salesChildInvoice.BC},DISCOUNT_ITEM_AMOUNT={salesChildInvoice.SD},VAT_ITEM_AMOUNT={salesChildInvoice.VT},TAXABLE_AMOUNT={salesChildInvoice.TA},NET_AMOUNT={salesChildInvoice.NA},REMARKS='{salesChildInvoice.REMARKS}',CURRENCY_CODE='{salesChildInvoice.CURRENCY_CODE}',EXCHANGE_RATE={salesChildInvoice.EXCHANGE_RATE},MODIFY_BY='{_workContext.CurrentUserinformation.login_code}',PARTY_TYPE_CODE='{salesChildInvoice.PARTY_TYPE_CODE}',MODIFY_DATE=SYSDATE WHERE SALES_NO='{salesChildInvoice.SALES_NO}' AND SERIAL_NO='{serialNo}'";
                    var InsertedData = _objectEntity.ExecuteSqlCommand(querySalesorder);
                    serialNo++;
                }


                updatedToChild = true;
                return updatedToChild;


            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }

        #endregion

        #region CHECK ALREADY INSERTED
        private bool CheckAlreadyInsertedCustomValues(string voucherNo, string formCode)
        {
            var queryCheckCustom = $@"SELECT COUNT(*) from CUSTOM_TRANSACTION WHERE FORM_CODE='{formCode}' AND VOUCHER_NO='{voucherNo}' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' AND BRANCH_CODE='{_workContext.CurrentUserinformation.branch_code}' AND DELETED_FLAG='N'";
            int newcustomcount = _objectEntity.SqlQuery<int>(queryCheckCustom).FirstOrDefault();

            if (newcustomcount > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool CheckAlreadyInsertedChargeValues(string voucherNo, string formCode)
        {
            var queryCheckCharge = $@"SELECT COUNT(*) from CHARGE_TRANSACTION WHERE FORM_CODE='{formCode}' AND REFERENCE_NO='{voucherNo}' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' AND BRANCH_CODE='{_workContext.CurrentUserinformation.branch_code}' AND DELETED_FLAG='N'";
            int newchargecount = _objectEntity.SqlQuery<int>(queryCheckCharge).FirstOrDefault();

            if (newchargecount > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool CheckAlreadyInsertedShippingValues(string voucherNo, string formCode)
        {
            var queryCheckShipping = $@"SELECT COUNT(*) from SHIPPING_TRANSACTION WHERE FORM_CODE='{formCode}' AND VOUCHER_NO='{voucherNo}' AND COMPANY_CODE='{_workContext.CurrentUserinformation.company_code}' AND BRANCH_CODE='{_workContext.CurrentUserinformation.branch_code}' AND DELETED_FLAG='N'";
            int newshippingcount = _objectEntity.SqlQuery<int>(queryCheckShipping).FirstOrDefault();

            if (newshippingcount > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion


    }

    public class ResponseMessage
    {
        public string StatusCode { get; set; }
        public string STATUS_CODE { get; set; }
        public string Message { get; set; }
        public string MESSAGE { get; set; }
        public string VoucherNo { get; set; }
        public string NewOrderNo { get; set; }
        public string SessionNo { get; set; }
        public string NewvoucherNo { get; set; }
        public string VoucherDate { get; set; }
        public string FormCode { get; set; }
        public string SaveFlag { get; set; }
    }


    public class CommonFieldForSales
    {
        public string OrderNumber { get; set; }
        public string NewOrderNumber { get; set; }
        public string FormCode { get; set; }
        public string SaveFlag { get; set; }
        public string ExchangeRate { get; set; }
        public string ManualNumber { get; set; }
        public string GrandTotal { get; set; }
        public string CurrencyFormat { get; set; }
        public string VoucherDate { get; set; }
        public string NewVoucherNumber { get; set; }
        public string TableName { get; set; }
        public bool FormRef { get; set; }
        public string PrimaryColumn { get; set; }
        public string PrimaryDateColumn { get; set; }
        public string Division_code { get; set; }
        public string SYN_ROWID { get; set; }
    }
}
