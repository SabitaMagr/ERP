using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoErp.Distribution.Service.Model.Mobile
{
    public class UpdateRequestModel:CommonRequestModel
    {
        public string sp_code { get; set; }
        public string customer_code { get; set; }
        public string customer_type { get; set; }
        public string remarks { get; set; }
        public string is_visited { get; set; }
        public string destination { get; set; }
        public string Track_Type { get; set; } = "TRK";
        //bikalp change
        public string PO_DCOUNT { get; set; }
        public string PO_RCOUNT { get; set; }
        public string RES_DETAIL { get; set; }
        public string RES_MASTER { get; set; }
        public string RES_ENTITY { get; set; }
        public string RES_PHOTO { get; set; }
        public string RES_CONTACT_PHOTO { get; set; }
        public string Time_Eod { get; set; } = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
    }
}
