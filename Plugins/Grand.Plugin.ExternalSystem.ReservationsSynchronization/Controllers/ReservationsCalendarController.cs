using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Grand.Plugin.ExternalSystem.ReservationsSynchronization.Models;
using Grand.Plugin.ExternalSystem.ReservationsSynchronization.Services;
using Grand.Services.Catalog;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Stores;
using Grand.Services.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Plugin.ExternalSystem.ReservationsSynchronization.Controllers
{
    public class ReservationsCalendarController : BasePluginController
    {
        protected readonly ReservationsCalendarPublishingService _publishingService;
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

        public ReservationsCalendarController(
            ReservationsCalendarPublishingService publishingService, 
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
            this._publishingService = publishingService;
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

        #region Administration

        [AuthorizeAdmin]
        [Area("Admin")]
        public async Task<IActionResult> Configure()
        {
            var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<ReservationsSynchronizationSettings>(storeScope);

            var task = await _scheduleTaskService.GetTaskByType(ReservationsSynchronizationPlugin.TASK_TYPE_NAME);

            var model = new ReservationsSynchronizationSettingsModel() {
                ActiveStoreScopeConfiguration = storeScope,

                ExternalCalendars = settings.ExternalCalendars.Select(x => new ReservationsSynchronizationSettingsModel.ExternalCalendarModel() {
                    ProductId = x.ProductId,
                    ResourceSystemName = x.ResourceSystemName,
                    Url = x.Url
                }).ToList(),

                PublishedCalendars = settings.PublishedCalendars.Select(x => new ReservationsSynchronizationSettingsModel.PublishedCalendarModel() {
                    ProductId = x.ProductId,
                    ResourceSystemName = x.ResourceSystemName,
                    PastDays = x.PastDays,
                    Url = !string.IsNullOrEmpty(x.Id) ? $"{_webHelper.GetStoreLocation()}/{UrlHelper.Action(nameof(GetReservations), "ReservationsCalendar", new { x.Id })}" : null,
                }).ToList(),

                SynchronizationInterval = task.Enabled ? task.TimeInterval : 0
            };

            if (!string.IsNullOrEmpty(storeScope))
            {
                model.ExternalCalendars_OverrideForStore = _settingService.SettingExists(settings, x => x.ExternalCalendars, storeScope);
                model.PublishedCalendars_OverrideForStore = _settingService.SettingExists(settings, x => x.PublishedCalendars, storeScope);
            }

            return View("~/Plugins/ExternalSystem.BookingCom/Views/ExternalBookingCom/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area("Admin")]
        public async Task<IActionResult> Configure(ReservationsSynchronizationSettingsModel model)
        {
            if (!ModelState.IsValid)
            {
                return await Configure();
            }

            var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<ReservationsSynchronizationSettings>(storeScope);

            settings.ExternalCalendars = model.ExternalCalendars.Select(x => new ReservationsSynchronizationSettings.ExternalCalendarSettings() {
                ProductId = x.ProductId,
                ResourceSystemName = x.ResourceSystemName,
                Url = x.Url
            }).ToList();

            settings.PublishedCalendars = model.PublishedCalendars.Select(x => new ReservationsSynchronizationSettings.PublishedCalendarSettings() {
                Id = !string.IsNullOrEmpty(x.Id) ? x.Id : Guid.NewGuid().ToString().Replace("-", ""),
                ProductId = x.ProductId,
                ResourceSystemName = x.ResourceSystemName,
                PastDays = x.PastDays
            }).ToList();

            if (model.ExternalCalendars_OverrideForStore || string.IsNullOrEmpty(storeScope))
            {
                await _settingService.SaveSetting(settings, x => x.ExternalCalendars, storeScope, false);
            }
            else if (!string.IsNullOrEmpty(storeScope))
            {
                await _settingService.DeleteSetting(settings, x => x.ExternalCalendars, storeScope);
            }

            if (model.PublishedCalendars_OverrideForStore || string.IsNullOrEmpty(storeScope))
            {
                await _settingService.SaveSetting(settings, x => x.PublishedCalendars, storeScope, false);
            }
            else if (!string.IsNullOrEmpty(storeScope))
            {
                await _settingService.DeleteSetting(settings, x => x.PublishedCalendars, storeScope);
            }

            await _settingService.ClearCache();

            var task = await _scheduleTaskService.GetTaskByType(ReservationsSynchronizationPlugin.TASK_TYPE_NAME);
            task.TimeInterval = model.SynchronizationInterval;
            task.Enabled = model.SynchronizationInterval > 0;

            await _scheduleTaskService.UpdateTask(task);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return await Configure();
        }

        [AuthorizeAdmin]
        [Area("Admin")]
        public async Task<IActionResult> Configure_GetProducts()
        {
            var products = (await _productService.GetProductsOfType(ProductType.Reservation)).Select(x => new
            {
                x.Id,
                Name = x.GetLocalized(y => y.Name, _workContext.WorkingLanguage.Id)
            });

            return Json(products);
        }

        [AuthorizeAdmin]
        [Area("Admin")]
        public async Task<IActionResult> Configure_GetResources(string productId)
        {
            var product = await _productService.GetProductById(productId);
            var resources = product.Resources?.Select(x => new
            {
                x.SystemName,
                x.Name
            });

            return Json(resources);
        }

        #endregion

        #region Publishing

        public async Task<IActionResult> GetReservations([Required] string id)
        {
            var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var allSettings = _settingService.LoadSetting<ReservationsSynchronizationSettings>(storeScope);
            var settings = allSettings.PublishedCalendars?.Where(x => x.Id == id).FirstOrDefault();

            if (settings != null)
            {
                var calendar = await _publishingService.GetCalendar(settings.ProductId, settings.ResourceSystemName, settings.PastDays);
                return Content(calendar.ToString());
            }
            else
            {
                return NotFound();
            }
        }

        #endregion
    }
}
