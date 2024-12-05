using NeoErp.Core.Quotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoErp.Services
{
    public interface IQuotation
    {
        List<Quotation> GetQuotationDetails(string id);
        List<Supplier> GetSupplierDetails(string panNo);
        List<Company> GetCompanyDetails(string id);
        int? InsertQuotationDetails(Quotation_Details formDatas);
        int? DocumentIfExists(string tenderNo, string quotationNo);
        int? GetSupplierCount(string panNo,string tenderNo);
        string InsertQuotationImage(QuotationTranscation quotationdetail);
        List<FormDetailSetup> GetFormDetailSetup(string companyCode);
        List<COMMON_COLUMN> GetQuestOrderFormDetail(string id,string companyCode);
    }
}
