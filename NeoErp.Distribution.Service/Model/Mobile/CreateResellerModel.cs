using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoErp.Distribution.Service.Model.Mobile
{
    public class CreateResellerModel : CommonRequestModel
    {
        public CreateResellerModel()
        {
            contact = new List<ContactModel>();
        }
        public string reseller_code { get; set; }
        public string reseller_name { get; set; }
        public string area_code { get; set; }
        public string address { get; set; }
        public string pan { get; set; }
        public string wholeseller { get; set; }
        public string type_id { get; set; }
        public string subtype_id { get; set; }
        public string Group_id { get; set; }
        public string email { get; set; }
        public string distributor_code { get; set; }
        public string wholeseller_code { get; set; }
        public string Reseller_contact { get; set; }
        public List<ContactModel> contact { get; set; }
        public string ROUTE_CODE { get; set; }
    }
    public class ContactModel
    {
        public string contact_suffix { get; set; }
        public string name { get; set; }
        public string number { get; set; }
        public string designation { get; set; }
        public string primary { get; set; }
        public string Sync_Id { get; set; }
    }

    public class ResellerEntityModel
    {
        public string RESELLER_CODE { get; set; }
        public string ENTITY_CODE { get; set; }
        public string ENTITY_TYPE { get; set; }
        public string COMPANY_CODE { get; set; }
    }
}
