using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoErp.Data
{
    public static class ConnectionManager
    {
        public static ConCredential GetConInfo()
        {
            SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder();
            connBuilder.ConnectionString = ConfigurationManager.ConnectionStrings["NeoErpCoreEntity"].ToString();
            ConCredential ConInfo = new ConCredential();

            ConInfo.Database = connBuilder.InitialCatalog;
            ConInfo.Server = connBuilder.DataSource;
            ConInfo.UserName = connBuilder.UserID;
            ConInfo.Password = connBuilder.Password;
            ConInfo.ConType = ConnectionType.SqlServer;
            return ConInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="AssemblyName">Default *, only put dll name if entity in seperate project from original use.</param>
        /// <returns></returns>
        public static String BuildConnectionString(string modelName, string AssemblyName = "*")
        {
            var database = ConfigurationManager.AppSettings["databasetype"].ToString();
            // EntityConnectionStringBuilder esb = new EntityConnectionStringBuilder();
            EntityConnectionStringBuilder esb;
            if (database=="oracle")
            {
               esb = new EntityConnectionStringBuilder(ConfigurationManager.ConnectionStrings["NeoErpCoreEntity"].ToString());
                // esb.ProviderConnectionString = ;
                return esb.ToString();
            }
            else
            {
                
                //string connectionString = new DbConnBase().ConnStr;
                //connectionString += ";App=EntityFramework;";

                // Build the MetaData... feel free to copy/paste it from the connection string in the config file.
                EntityConnectionStringBuilder esb1 = new EntityConnectionStringBuilder(ConfigurationManager.ConnectionStrings["NeoErpSqlEntities"].ToString());
                
                return esb1.ToString();
            }
            

            // Generate the full string and return it
          
        }

    }
}
