using Grand.Core;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Grand.Plugin.ExternalSystem.BookingCom.Models;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Stores;
using Grand.Services.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Plugin.ExternalSystem.BookingCom.Controller
{
    public class ExternalBookingComController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IScheduleTaskService _scheduleTaskService;

        public ExternalBookingComController(IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            ILocalizationService localizationService,
            IScheduleTaskService scheduleTaskService)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._scheduleTaskService = scheduleTaskService;
        }

        [AuthorizeAdmin]
        [Area("Admin")]
        public async Task<IActionResult> Configure()
        {
            var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var bookingComSettings = _settingService.LoadSetting<BookingComSettings>(storeScope);

            var task = await _scheduleTaskService.GetTaskByType(BookingComApiPlugin.TASK_TYPE_NAME);

            var model = new BookingComSettingsModel() {
                ActiveStoreScopeConfiguration = storeScope,
                Username = bookingComSettings.Username,
                Password = bookingComSettings.Password,
                ScheduledTaskInterval = task.Enabled ? task.TimeInterval : 0
            };

            if (!string.IsNullOrEmpty(storeScope))
            {
                model.Username_OverrideForStore = _settingService.SettingExists(bookingComSettings, x => x.Username, storeScope);
                model.Password_OverrideForStore = _settingService.SettingExists(bookingComSettings, x => x.Password, storeScope);
            }

            return View("~/Plugins/ExternalSystem.BookingCom/Views/ExternalBookingCom/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area("Admin")]
        public async Task<IActionResult> Configure(BookingComSettingsModel model)
        {
            if (!ModelState.IsValid)
            {
                return await Configure();
            }

            var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var bookingComSettings = _settingService.LoadSetting<BookingComSettings>(storeScope);

            bookingComSettings.Username = model.Username;
            bookingComSettings.Password = model.Password;

            if (model.Username_OverrideForStore || string.IsNullOrEmpty(storeScope))
            {
                await _settingService.SaveSetting(bookingComSettings, x => x.Username, storeScope, false);
            }
            else if (!string.IsNullOrEmpty(storeScope))
            {
                await _settingService.DeleteSetting(bookingComSettings, x => x.Username, storeScope);
            }

            if (model.Password_OverrideForStore || string.IsNullOrEmpty(storeScope))
            {
                await _settingService.SaveSetting(bookingComSettings, x => x.Password, storeScope, false);
            }
            else if (!string.IsNullOrEmpty(storeScope))
            {
                await _settingService.DeleteSetting(bookingComSettings, x => x.Password, storeScope);
            }

            await _settingService.ClearCache();

            var task = await _scheduleTaskService.GetTaskByType(BookingComApiPlugin.TASK_TYPE_NAME);
            task.TimeInterval = model.ScheduledTaskInterval;
            task.Enabled = model.ScheduledTaskInterval > 0;

            await _scheduleTaskService.UpdateTask(task);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return await Configure();
        }
    }
}
