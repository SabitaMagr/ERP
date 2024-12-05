using NeoERP.DocumentTemplate.Service.Models.ProcessSetupBom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoERP.DocumentTemplate.Service.Interface
{
    public interface IProcessSetupBom
    {
        List<ProcessSetupBomModel> GetAllProcessCategoryRoutine();

        List<ProcessCatRoutineForDDL> GetAllProcessForDDL();

        List<ProcessTypeCodeModel> GetAllProcessTypeCode();

        List<BillAndOutputMaterialModel> GetBillOfMaterialsList(string processCode);

        List<BillAndOutputMaterialModel> GetOutputMaterialsList(string processCode);

        List<BomRoutineModel> GetRoutineByProcessCode(string processCode);

        ProcessResposponseForRoutine SaveProcessCategoryRoutine(ProcessCategoryRoutineSaveModel model);

        // ProcessResposponseForRoutine SaveProcessModel(ProcessCategoryRoutineSaveModel model);

        //   string SaveInputOutMaterial(InputOutMaterialSaveModel materialModel);

        //get routine for editing
        // BillAndOutputMaterialModel GetRoutineDetail(string processCode);

        string SaveRoutineDetailSetup(RoutineDetailSaveModel routineModel);

        List<ProcessMuCodeModel> GetProcessMuCodeList();

        List<ProcessLocationModal> GetAllLocation();

        List<ProcessItemModal> GetAllItemForInputOutput();

        string GetChildProcessCode(string processCode);
        ProcessRoutineDetail GetChildProcessDetail(string processCode);

        List<ProcessPeriodModal> GetProcessPeriod();
    }
}
