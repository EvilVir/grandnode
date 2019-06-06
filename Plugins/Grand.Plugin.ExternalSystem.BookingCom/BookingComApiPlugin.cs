using Grand.Core;
using Grand.Core.Domain.Tasks;
using Grand.Core.Plugins;
using Grand.Plugin.ExternalSystem.BookingCom.Tasks;
using Grand.Services.Common;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Tasks;
using System;
using System.Threading.Tasks;

namespace Grand.Plugin.ExternalSystem.BookingCom
{
    public class BookingComApiPlugin : BasePlugin, IMiscPlugin
    {
        public static readonly string TASK_TYPE_NAME = $"{typeof(ReservationsFetchingTask).FullName}, {typeof(ReservationsFetchingTask).Assembly.GetName().Name}";
        
        protected readonly IScheduleTaskService _scheduleTaskService;
        protected readonly ISettingService _settingService;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IWebHelper _webHelper;

        public BookingComApiPlugin(IScheduleTaskService scheduleTaskService, ISettingService settingService, IServiceProvider serviceProvider, IWebHelper webHelper)
        {
            this._scheduleTaskService = scheduleTaskService;
            this._settingService = settingService;
            this._serviceProvider = serviceProvider;
            this._webHelper = webHelper;
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/ExternalBookingCom/Configure";
        }

        public override async Task Install()
        {
            await _settingService.SaveSetting(new BookingComSettings {
                LastCheck = null,
            });

            await _scheduleTaskService.InsertTask(new ScheduleTask() {
                ScheduleTaskName = "Booking.com Reservations Fetching",
                Type = TASK_TYPE_NAME,
                Enabled = false,
                StopOnError = false,
                TimeInterval = 0
            });

            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.BookingCom.Fields.Username", "Username for Booking.com", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.BookingCom.Fields.Username", "Nazwa użytkownika Booking.com", "pl-PL");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.BookingCom.Fields.Password", "Password for Booking.com", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.BookingCom.Fields.Password", "Hasło do Booking.com", "pl-PL");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.BookingCom.Fields.ScheduledTaskInterval", "Fetch interval in minutes (0 - off)", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.BookingCom.Fields.ScheduledTaskInterval", "Częstość pobierania w minutach (0 - wyłączone)", "pl-PL");

            await base.Install();
        }

        public override async Task Uninstall()
        {
            var task = await _scheduleTaskService.GetTaskByType(TASK_TYPE_NAME);

            if (task != null)
            {
                await _scheduleTaskService.DeleteTask(task);
            }

            await _settingService.DeleteSetting<BookingComSettings>();

            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.Payments.BookingCom.Fields.Username");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.Payments.BookingCom.Fields.Password");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.Payments.BookingCom.Fields.ScheduledTaskInterval");

            await base.Uninstall();
        }
    }
}
