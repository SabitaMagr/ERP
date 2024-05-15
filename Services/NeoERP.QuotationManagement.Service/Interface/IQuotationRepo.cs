using NeoErp.Core.Domain;
using NeoErp.Core.Models;
using System.Collections.Generic;
using NeoErp.Core.Models.CustomModels;
using NeoERP.QuotationManagement.Service.Models;
using System.Net.Http;

namespace NeoERP.QuotationManagement.Service.Interface
{
    public interface IQuotationRepo
    {
        List<Products> GetAllProducts();
        List<Company> GetCompany();
        List<Quotation_setup> GetQuotationId();
        bool InsertQuotationData(Quotation_setup data);
        List<Quotation_setup> GetTenderId(string tenderNo);
        List<Quotation_setup> ListAllTenders();
        List<Quotation_Details> ListQuotationDetails();
        bool DeleteTender(string tenderNo);
        List<Quotation_setup> GetQuotationById(string tenderNo);
        bool updateItemsById(string tenderNo,string id);
        List<Quotation_Details> QuotationDetailsById(string quotationNo);
        List<SummaryReport> TendersItemWise();
        List<Quotation> ItemDetailsTenderNo(string tenderNo);
    }
}
