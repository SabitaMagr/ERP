using NeoERP.DocumentTemplate.Service.Interface;
using NeoERP.DocumentTemplate.Service.Models.ProcessSetupBom;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace NeoERP.DocumentTemplate.Controllers.Api
{
    public class ProcessSetupBomApiController : ApiController
    {
        private IProcessSetupBom _processSetupBom;
        public ProcessSetupBomApiController(IProcessSetupBom processSetupBom)
        {
            this._processSetupBom = processSetupBom;
        }

        [HttpGet]
        public List<ProcessSetupBomModel> GetAllProcessCategoryRoutine()
        {
            List<ProcessSetupBomModel> bomModel = new List<ProcessSetupBomModel>();
            try
            {
                bomModel = _processSetupBom.GetAllProcessCategoryRoutine();
                return bomModel;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }

        [HttpGet]
        public List<ProcessCatRoutineForDDL> GetAllProcessForDDL()
        {
            var processDDLData = new List<ProcessCatRoutineForDDL>();
            try
            {
                processDDLData = _processSetupBom.GetAllProcessForDDL();
                return processDDLData;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.StackTrace);
            }
        }

        [HttpGet]
        public List<ProcessTypeCodeModel> GetAllProcessTypeCode()
        {
            List<ProcessTypeCodeModel> ptcModel = new List<ProcessTypeCodeModel>();
            try
            {
                ptcModel = _processSetupBom.GetAllProcessTypeCode();
                return ptcModel;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }

        [HttpGet]
        public List<BillAndOutputMaterialModel> GetBillOfMaterialList(string processCode = "0")
        {
            var bomList = new List<BillAndOutputMaterialModel>();
            try
            {
                bomList = _processSetupBom.GetBillOfMaterialsList(processCode.Replace("\"", ""));
                return bomList;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.StackTrace);
            }
        }



        [HttpGet]
        public List<BillAndOutputMaterialModel> GetOutputMaterialList(string processCode = "0")
        {
            var omList = new List<BillAndOutputMaterialModel>();
            try
            {
                omList = _processSetupBom.GetOutputMaterialsList(processCode.Replace("\"", ""));
                return omList;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.StackTrace);
            }
        }

        [HttpGet]
        public List<BomRoutineModel> GetRoutineBasedOnProcessCode(string processCode = "0")
        {
            List<BomRoutineModel> bomRoutineModel = new List<BomRoutineModel>();
            try
            {
                if (processCode == null)
                {
                    processCode = "0";
                }
                else
                {
                    bomRoutineModel = _processSetupBom.GetRoutineByProcessCode(processCode);
                }

                return bomRoutineModel;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet]
        public List<ProcessPeriodModal> GetProcessPeriod()
        {
            try
            {
                var periodData = _processSetupBom.GetProcessPeriod();
                return periodData;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }


        [HttpGet]
        public List<ProcessMuCodeModel> GetProcessMuCodeList()
        {
            var processMuList = new List<ProcessMuCodeModel>();
            try
            {
                processMuList = _processSetupBom.GetProcessMuCodeList();
                return processMuList;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public ProcessResposponseForRoutine SaveProcessCategoryRoutine(ProcessCategoryRoutineSaveModel model)
        {
            var saveResponse = _processSetupBom.SaveProcessCategoryRoutine(model);
            return saveResponse;
        }



        [HttpPost]
        public string SaveRoutineDetailSetup(RoutineDetailSaveModel model)
        {
            var routineResponse = _processSetupBom.SaveRoutineDetailSetup(model);
            return routineResponse;
        }


        [HttpGet]
        public List<ProcessLocationModal> GetAllLocation()
        {
            var allLocation = _processSetupBom.GetAllLocation();
            return allLocation;
        }

        //[HttpGet]
        //public string[] GetAllItemForInputOutput()
        //{
        //    var allItem = _processSetupBom.GetAllItemForInputOutput();

        //    string[] arr = allItem.Select(x => x.ITEM_EDESC).ToArray();

        //    return arr;
        //}

        [HttpGet]
        public List<ProcessItemModal> GetAllItemForInputOutput()
        {
            var allItem = _processSetupBom.GetAllItemForInputOutput();
            return allItem;
        }


        [HttpGet]
        public string GetChildProcessCode(string processCode)
        {
            if (processCode == null) return "0";
            var chilPCode = _processSetupBom.GetChildProcessCode(processCode);
            return chilPCode;
        }

        [HttpGet]
        public ProcessRoutineDetail GetChildProcessDetail(string processCode)
        {
            if (processCode == null) return new ProcessRoutineDetail();
            else
            {
                var childProcessDetail = _processSetupBom.GetChildProcessDetail(processCode);
                return childProcessDetail;
            }
        }


    }
}
