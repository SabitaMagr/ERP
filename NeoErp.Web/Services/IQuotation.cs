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
        List<Employee> GetEmployeeDetails(string panNo);
        List<Company> GetCompanyDetails(string id);
        bool InsertQuotationDetails(Quotation_Details formDatas);

    }
}
