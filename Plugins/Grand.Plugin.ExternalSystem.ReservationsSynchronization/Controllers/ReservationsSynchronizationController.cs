using Grand.Core;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Plugin.ExternalSystem.ReservationsSynchronization.Models;
using Grand.Services.Catalog;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Stores;
using Grand.Services.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Plugin.ExternalSystem.ReservationsSynchronization.Controllers
{

    [AuthorizeAdmin]
    [Area("Admin")]
    public class ReservationsSynchronizationController : BasePluginController
    {
        protected readonly ISettingService _settingService;
        protected readonly IWorkContext _workContext;
        protected readonly IStoreService _storeService;
        protected readonly ILocalizationService _localizationService;
        protected readonly IScheduleTaskService _scheduleTaskService;
        protected readonly IWebHelper _webHelper;
        protected readonly IUrlHelperFactory _urlHelperFactory;
        protected readonly IActionContextAccessor _actionContextAccessor;
        protected readonly IProductService _productService;

        protected IUrlHelper UrlHelper => _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

        public ReservationsSynchronizationController(
            IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            ILocalizationService localizationService,
            IScheduleTaskService scheduleTaskService,
            IWebHelper webHelper,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IProductService productService)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._scheduleTaskService = scheduleTaskService;
            this._webHelper = webHelper;
            this._urlHelperFactory = urlHelperFactory;
            this._actionContextAccessor = actionContextAccessor;
            this._productService = productService;
        }

        #region General Settings

        public async Task<IActionResult> Configure()
        {
            var task = await _scheduleTaskService.GetTaskByType(ReservationsSynchronizationPlugin.TASK_TYPE_NAME);

            var model = new ReservationsSynchronizationSettingsModel() {
                SynchronizationInterval = task.Enabled ? task.TimeInterval : 0
            };

            return View("~/Plugins/ExternalSystem.ReservationsSynchronization/Views/ReservationsSynchronization/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ReservationsSynchronizationSettingsModel model)
        {
            if (!ModelState.IsValid)
            {
                return await Configure();
            }

            var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<ReservationsSynchronizationSettings>(storeScope);

            await _settingService.ClearCache();

            var task = await _scheduleTaskService.GetTaskByType(ReservationsSynchronizationPlugin.TASK_TYPE_NAME);
            task.TimeInterval = model.SynchronizationInterval;
            task.Enabled = model.SynchronizationInterval > 0;

            await _scheduleTaskService.UpdateTask(task);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return await Configure();
        }

        #endregion

        #region External Calendars

        public async Task<IActionResult> ExternalCalendarList()
        {
            var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<ReservationsSynchronizationSettings>(storeScope);

            var output = settings.ExternalCalendars.Select(x => ToData(x, new ReservationsSynchronizationSettingsModel.ExternalCalendarModel())).ToList();

            var gridModel = new DataSourceResult {
                Data = output,
                Total = output.Count
            };

            return Json(gridModel);
        }

        public async Task<IActionResult> ExternalCalendarAdd(ReservationsSynchronizationSettingsModel.ExternalCalendarModel data)
        {
            var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<ReservationsSynchronizationSettings>(storeScope);

            settings.ExternalCalendars.Add(ToModel(data, new ReservationsSynchronizationSettings.ExternalCalendarSettings()));

            await _settingService.SaveSetting(settings, x => x.ExternalCalendars, storeScope, true);

            return new NullJsonResult();
        }

        public async Task<IActionResult> ExternalCalendarUpdate(ReservationsSynchronizationSettingsModel.ExternalCalendarModel data)
        {
            var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<ReservationsSynchronizationSettings>(storeScope);
            var target = settings.ExternalCalendars.Where(x => x.Id == data.Id).FirstOrDefault();

            if (target != null)
            {
                ToModel(data, target);
                await _settingService.SaveSetting(settings, x => x.ExternalCalendars, storeScope, true);
            }

            return new NullJsonResult();
        }

        public async Task<IActionResult> ExternalCalendarDelete(ReservationsSynchronizationSettingsModel.ExternalCalendarModel data)
        {
            var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<ReservationsSynchronizationSettings>(storeScope);
            var target = settings.ExternalCalendars.Where(x => x.Id == data.Id).FirstOrDefault();

            if (target != null)
            {
                settings.ExternalCalendars.Remove(target);
                await _settingService.SaveSetting(settings, x => x.ExternalCalendars, storeScope, true);
            }

            return new NullJsonResult();
        }

        protected ReservationsSynchronizationSettings.ExternalCalendarSettings ToModel(ReservationsSynchronizationSettingsModel.ExternalCalendarModel data, ReservationsSynchronizationSettings.ExternalCalendarSettings target)
        {
            target.Id = !string.IsNullOrEmpty(data.Id) ? data.Id : Guid.NewGuid().ToString().Replace("-", "");
            target.ProductId = data.ProductId;
            target.ResourceSystemName = data.ResourceSystemName;
            target.Url = data.Url;

            return target;
        }

        protected ReservationsSynchronizationSettingsModel.ExternalCalendarModel ToData(ReservationsSynchronizationSettings.ExternalCalendarSettings model, ReservationsSynchronizationSettingsModel.ExternalCalendarModel target)
        {
            target.Id = model.Id;
            target.ProductId = model.ProductId;
            target.ResourceSystemName = model.ResourceSystemName;
            target.Url = model.Url;

            return target;
        }

        #endregion

        #region Published Calendars

        public async Task<IActionResult> PublishedCalendarList()
        {
            var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<ReservationsSynchronizationSettings>(storeScope);

            var output = settings.PublishedCalendars.Select(x => ToData(x, new ReservationsSynchronizationSettingsModel.PublishedCalendarModel())).ToList();

            var gridModel = new DataSourceResult {
                Data = output,
                Total = output.Count
            };

            return Json(gridModel);
        }

        public async Task<IActionResult> PublishedCalendarAdd(ReservationsSynchronizationSettingsModel.PublishedCalendarModel data)
        {
            var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<ReservationsSynchronizationSettings>(storeScope);

            settings.PublishedCalendars.Add(ToModel(data, new ReservationsSynchronizationSettings.PublishedCalendarSettings()));

            await _settingService.SaveSetting(settings, x => x.PublishedCalendars, storeScope, true);

            return new NullJsonResult();
        }

        public async Task<IActionResult> PublishedCalendarUpdate(ReservationsSynchronizationSettingsModel.PublishedCalendarModel data)
        {
            var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<ReservationsSynchronizationSettings>(storeScope);
            var target = settings.PublishedCalendars.Where(x => x.Id == data.Id).FirstOrDefault();

            if (target != null)
            {
                ToModel(data, target);
                await _settingService.SaveSetting(settings, x => x.PublishedCalendars, storeScope, true);
            }

            return new NullJsonResult();
        }

        public async Task<IActionResult> PublishedCalendarDelete(ReservationsSynchronizationSettingsModel.PublishedCalendarModel data)
        {
            var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<ReservationsSynchronizationSettings>(storeScope);
            var target = settings.PublishedCalendars.Where(x => x.Id == data.Id).FirstOrDefault();

            if (target != null)
            {
                settings.PublishedCalendars.Remove(target);
                await _settingService.SaveSetting(settings, x => x.PublishedCalendars, storeScope, true);
            }

            return new NullJsonResult();
        }

        protected ReservationsSynchronizationSettings.PublishedCalendarSettings ToModel(ReservationsSynchronizationSettingsModel.PublishedCalendarModel data, ReservationsSynchronizationSettings.PublishedCalendarSettings target)
        {
            target.Id = !string.IsNullOrEmpty(data.Id) ? data.Id : Guid.NewGuid().ToString().Replace("-", "");
            target.ProductId = data.ProductId;
            target.ResourceSystemName = data.ResourceSystemName;
            target.PastDays = data.PastDays;

            return target;
        }

        protected ReservationsSynchronizationSettingsModel.PublishedCalendarModel ToData(ReservationsSynchronizationSettings.PublishedCalendarSettings model, ReservationsSynchronizationSettingsModel.PublishedCalendarModel target)
        {
            target.Id = model.Id;
            target.ProductId = model.ProductId;
            target.ResourceSystemName = model.ResourceSystemName;
            target.PastDays = model.PastDays;
            target.Url = !string.IsNullOrEmpty(model.Id) ? $"{_webHelper.GetStoreLocation()}Api/ReservationsCalendar/{model.Id}" : "";

            return target;
        }

        #endregion
    }
}
