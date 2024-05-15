using System;
using System.Collections.Generic;
using NeoErp.Services;
using NeoErp.Data;
using NeoErp.Core.Quotation;
using System.Web.Http;
using System.Web;

namespace NeoErp.Controllers.Api
{
    public class ApiQuotationController : ApiController
    {
        private IDbContext _dbContext;
        private IQuotation _quoTemplate;

        public ApiQuotationController(IDbContext dbContext, IQuotation quoTemplate)
        {
            this._dbContext = dbContext;
            _quoTemplate = quoTemplate;

        }
        public List<Quotation> GetQuotationDetails(string id)
        {
            List<Quotation> quotations = new List<Quotation>();
            try
            {
                quotations = _quoTemplate.GetQuotationDetails(id);
                return quotations;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }

        public List<Employee> getEmployeeDetails(string panNo)
        {
            List<Employee> employeeDetails = _quoTemplate.GetEmployeeDetails(panNo);
            return employeeDetails;
        }
        public List<Company> GetCompanyDetails(string id)
        {
            List<Company> company = _quoTemplate.GetCompanyDetails(id);
            return company;
        }
        [HttpPost]
        public IHttpActionResult SaveFormData(Quotation_Details formData)
        {
            try
            {
                bool isPosted = _quoTemplate.InsertQuotationDetails(formData);
                if (isPosted)
                {
                    return Ok(new { success = true, message = "Quotation Details saved successfully." });
                }
                else
                {
                    return Ok(new { success = true, message = "Failed to save Quotation Details." });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

    }
}
