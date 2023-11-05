using NeoErp.Core.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NeoErp.Data;
using System.Web.Mvc;

namespace NeoErp.Core.Models
{
    public class AppSettingsModel
    {
        public ConCredential SqlConInfo { get; set; }
        public string ClientUniqueID { get; set; }

        public string ExcelReportWebPath = "/Documents/Reports/";
        public string EmpImagePath = "/Documents/EmployeeImages/";       
        public string FileUploadUrl = "/Home/UploadFile";

        public  string DashboardPage {
            get { return dashBoardPage; }
            set { dashBoardPage = value; }
        }
        public static string dashBoardPage = "/Main/DashBoard";
        public static string noPromissionPage = "/Security/Account/NoPermission";
        public static string loginPage = "/Security/Account/Login";
        public static string connStringName = "connString";

    }
}