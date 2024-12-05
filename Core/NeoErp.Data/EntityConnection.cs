using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Web;

namespace NeoErp.Data
{
    
    public partial class eHRMEntities : DbContext
    {
        public eHRMEntities()
            : base(ConnectionManager.BuildConnectionString("NeoErp.Core.Models.NeoErpCoreEntity.NeoErpCoreEntity")) 
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LINKAQDEntities"/> class.
        /// </summary>
        /// <param name="entityConnection">The entity connection.</param>
        public eHRMEntities(EntityConnection entityConnection) :
            base(entityConnection, false)
        {

        }
    }

}