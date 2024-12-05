using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoERP.DocumentTemplate.Service.Models.ProcessSetupBom
{
    public class ProcessSetupBomModel
    {
        public int LEVEL { get; set; }
        public string PROCESS_CODE { get; set; }
        public string PROCESS_EDESC { get; set; }
        public string PROCESS_TYPE_CODE { get; set; }
        public string PROCESS_FLAG { get; set; }
        public string PRE_PROCESS_CODE { get; set; }
        public string COMPANY_CODE { get; set; }
        public string CREATED_BY { get; set; }
        public DateTime CREATED_DATE { get; set; }

        public bool HAS_BRANCH { get; set; }
        public string LOCATION_CODE { get; set; }
        public string LOCATION_EDESC { get; set; }
        public string PRIORITY_ORDER_NO { get; set; }

        

        public List<ProcessSetupBomModel> ITEMS { get; set; }

    }
    public class ProcessTypeCodeModel
    {
        public string PROCESS_TYPE_CODE { get; set; }
        public string PROCESS_TYPE_EDESC { get; set; }
        public string COMPANY_CODE { get; set; }
        public string CREATED_BY { get; set; }
        public DateTime CREATED_DATE { get; set; }

    }
    public class BillAndOutputMaterialModel
    {
        public string PROCESS_CODE { get; set; }
        public string PROCESS_EDESC { get; set; }
        public string MODEL_INFO { get; set; }
        public string MODAL_INFO { get; set; }
        public string SKU_ITEM { get; set; }
        public int QUANTITY { get; set; }
        public string MU_CODE { get; set; }
        public string MU_EDESC { get; set; }
        public string UNIT_CODE { get; set; }
        public string REMARK { get; set; }
        public string ITEM_CODE { get; set; }
        public string ITEM_EDESC { get; set; }
        public int OUT_PUT { get; set; }
        public string VALUATION_FLAG { get; set; }
        public string PERIOD_CODE { get; set; }
        public string PERIOD_EDESC { get; set; }
        public string INDEX_CAPACITY { get; set; }
        public string INDEX_TIME_REQUIRED { get; set; }
    }
    public class BomRoutineModel
    {
        public string SHORT_CUT { get; set; }
        public string PROCESS_DESCRIPTION { get; set; }
        public string ITEM_DESCRIPTION { get; set; }
        public double CAPACITY { get; set; }
        public string MU_CODE { get; set; }
        public string LOCATION_EDESC { get; set; }
    }
    public class ProcessCatRoutineForDDL
    {
        public string PRE_PROCESS_CODE { get; set; }
        public string PROCESS_CODE { get; set; }
        public string PROCESS_EDESC { get; set; }
        public string PROCESS_TYPE_CODE { get; set; }
        public string PROCESS_FLAG { get; set; }
    }
    public class ProcessCategoryRoutineSaveModel
    {
        public string ROOT_UNDER { get; set; }
        public string PROCESS_CODE { get; set; }
        public string IN_ENGLISH { get; set; }
        public string IN_NEPALI { get; set; }
        public string PRIORITY_NUMBER { get; set; }
        public string PROCESS_FLAG { get; set; }
        public string PROCESS_TYPE { get; set; }
        public string LOCATION { get; set; }
        public string REMARK { get; set; }
        public bool IS_EDIT { get; set; } = false;

    }
    public class InputOutMaterialSaveModel
    {
        public string MODAL_INFO { get; set; }
        public string PROCESS { get; set; }
        public string PROCESS_CODE { get; set; }
        public string ITEM_NAME { get; set; }
        public string ITEM_CODE { get; set; }
        public string CAPACITY { get; set; }
        public string QUANTITY { get; set; }
        public string UNIT { get; set; }
        public string UNIT_CODE { get; set; }
        public string REMARK { get; set; }
        public string OUTPUT { get; set; }
        public string VALUATION_FLAG { get; set; } 
    }
    public class TempRoutineDetail
    {
        public string ROUTINE_NAME { get; set; }
        public string ROUTINE_CODE { get; set; }
        public string BELONGS { get; set; }
        public string BELONGS_CODE { get; set; }
        public string INPUT_INDEX_ITEM { get; set; }
        public string OUTPUT_INDEX_ITEM { get; set; }
        public string INPUT_CAPACITY { get; set; }
        public string INPUT_UNIT { get; set; }
        public string INPUT_IN_PERIOD { get; set; }
        public string OUTPUT_CAPACITY { get; set; }
        public string OUTPUT_UNIT { get; set; }
        public string OUTPUT_IN_PERIOD { get; set; }
        public string PROCESS_FLAG { get; set; }

    }
    public class RoutineDetailSaveModel
    {
        public TempRoutineDetail RoutineDetail { get; set; }
        public List<InputOutMaterialSaveModel> InputModel { get; set; } = new List<InputOutMaterialSaveModel>();
        public List<InputOutMaterialSaveModel> OutputModel { get; set; } = new List<InputOutMaterialSaveModel>();
    }
    public class ProcessMuCodeModel
    {
        public string MU_CODE { get; set; }
        public string MU_EDESC { get; set; }
    }
    public class ProcessLocationModal
    {
        public string LOCATION_CODE { get; set; }
        public string LOCATION_EDESC { get; set; }

    }
    public class ProcessItemModal
    {
        public string ITEM_CODE { get; set; }
        public string ITEM_EDESC { get; set; }
        public string MU_CODE { get; set; }
        public string MU_EDESC { get; set; }
    }
    public class ProcessResposponseForRoutine
    {
        public string MESSAGE { get; set; }
        public string PROCESS_FLAG { get; set; }
        public string ROOT_UNDER { get; set; }
        public ProcessCategoryRoutineSaveModel SAVED_MODAL { get; set; }
    }
    public class ProcessRoutineDetail
    {
        public string PROCESS_CODE { get; set; }
        public string PROCESS_EDESC { get; set; }
        public string PROCESS_TYPE_CODE { get; set; }
        public string PROCESS_TYPE_EDESC { get; set; }
        public string PROCESS_FLAG { get; set; }
        public string LOCATION_CODE { get; set; }

        public string PRE_LOCATION_CODE { get; set; }
        public string LOCATION_EDESC { get; set; }
        public string REMARKS { get; set; }
        public string PRIORITY_ORDER_NO { get; set; }

    }

    public class ProcessPeriodModal
    {
        public string PERIOD_CODE { get; set; }
        public string PERIOD_EDESC { get; set; }
        public decimal YEARLY_PERIOD_NO { get; set; }
        public decimal YEARLY_DAYS_NO { get; set; }

    }
}
