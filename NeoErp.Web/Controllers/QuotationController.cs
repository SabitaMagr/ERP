using System;
using System.Collections.Generic;
using System.Linq;
using PagedList;
using System.Web.Mvc;
using NeoErp.Models;
using NeoErp.Models.Common;
using NeoErp.Services.LogService;
using NeoErp.Core;
using NeoErp.Services;
using NeoErp.Core.Services;
using NeoErp.Core.Domain;
using NeoErp.Core.Caching;
using NeoErp.Core.Plugins;
using System.Xml.Linq;
using System.Xml;
using System.Configuration;
using NeoErp.Data;
using NeoErp.Models.QueryBuilder;
using NeoErp.Core.Services.CommonSetting;
using NeoErp.Core.Models.Log4NetLoggin;
using NeoErp.Core.Quotation;

namespace NeoErp.Controllers
{
    public class QuotationController : Controller
    {
        private IWorkContext _workContext;
        private IAuthenticationService _authenticationService;
        private IMenuModel _menuService;
        private readonly ICacheManager _cacheManager;
        private IPluginFinder _pluginFinder;
        private IQueryBuilder _queryService;
        private IDbContext _dbContext;
        private ISettingService _settingService;
        private ILogViewer _logViewer;
        private ILogErp _logErp;

        public QuotationController(IWorkContext workContext, IAuthenticationService authenticationService, IMenuModel menuService, ICacheManager cacheManager, IPluginFinder pluginFinder, IQueryBuilder queryService, IDbContext dbContext, ISettingService settingService, ILogViewer logViewer, ILogErp logErp)
        {
            this._workContext = workContext;
            this._authenticationService = authenticationService;
            this._menuService = menuService;
            this._cacheManager = cacheManager;
            this._pluginFinder = pluginFinder;
            this._queryService = queryService;
            this._dbContext = dbContext;
            this._settingService = settingService;
            this._logViewer = logViewer;
            this._logErp = new LogErp(this);

        }
        public ActionResult Index(string qo)
        {
            return View();
        }
        public ActionResult Message()
        {
            return View();

        }
    }
}