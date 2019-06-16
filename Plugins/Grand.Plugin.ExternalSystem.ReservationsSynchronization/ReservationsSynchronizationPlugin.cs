using Grand.Core;
using Grand.Core.Domain.Tasks;
using Grand.Core.Plugins;
using Grand.Plugin.ExternalSystem.ReservationsSynchronization.Tasks;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Tasks;
using System;
using System.Threading.Tasks;

namespace Grand.Plugin.ExternalSystem.ReservationsSynchronization
{
    public class ReservationsSynchronizationPlugin : BasePlugin
    {
        public static readonly string TASK_TYPE_NAME = $"{typeof(ReservationsSynchronizationTask).FullName}, {typeof(ReservationsSynchronizationTask).Assembly.GetName().Name}";

        protected readonly IScheduleTaskService _scheduleTaskService;
        protected readonly ISettingService _settingService;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IWebHelper _webHelper;

        public ReservationsSynchronizationPlugin(IScheduleTaskService scheduleTaskService, ISettingService settingService, IServiceProvider serviceProvider, IWebHelper webHelper)
        {
            this._scheduleTaskService = scheduleTaskService;
            this._settingService = settingService;
            this._serviceProvider = serviceProvider;
            this._webHelper = webHelper;
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/ExternalReservationsSynchronization/Configure";
        }

        public override async Task Install()
        {
            await _settingService.SaveSetting(new ReservationsSynchronizationSettings {
            });

            await _scheduleTaskService.InsertTask(new ScheduleTask() {
                ScheduleTaskName = "Reservations Synchronization Fetching Task",
                Type = TASK_TYPE_NAME,
                Enabled = false,
                StopOnError = false,
                TimeInterval = 0
            });

            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendars", "External calendars", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendars", "Published calendars", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.SynchronizationInterval", "Synchronization interval (minutes)", "en-US");

            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendar.ProductId", "Target product", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendar.ProductId.Required", "Target product is required", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendar.ResourceSystemName", "Target resource", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendar.Url", "URL", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendar.Url.Required", "URL is required", "en-US");

            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendar.ProductId", "Source product", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendar.ProductId.Required", "Source product is required", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendar.ResourceSystemName", "Source resource", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendar.Url", "URL", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendar.PastDays", "How many past days to include", "en-US");


            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendars", "Kalendarze zewnętrzne", "pl-PL");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendars", "Opublikowane kalendarze", "pl-PL");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.SynchronizationInterval", "Częstość synchronizacji (minuty)", "pl-PL");

            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendar.ProductId", "Produkt docelowy", "pl-PL");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendar.ProductId.Required", "Produkt docelowy jest wymagany", "pl-PL");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendar.ResourceSystemName", "Zasób docelowy", "pl-PL");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendar.Url", "URL", "pl-PL");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendar.Url.Required", "URL jest wymagany", "pl-PL");

            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendar.ProductId", "Produkt źródłowy", "pl-PL");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendar.ProductId.Required", "Produkt źródłowy jest wymagany", "pl-PL");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendar.ResourceSystemName", "Zasób źródłowy", "pl-PL");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendar.Url", "URL", "pl-PL");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendar.PastDays", "Ilość przeszłych dni", "pl-PL");

            await base.Install();
        }

        public override async Task Uninstall()
        {
            var task = await _scheduleTaskService.GetTaskByType(TASK_TYPE_NAME);

            if (task != null)
            {
                await _scheduleTaskService.DeleteTask(task);
            }

            await _settingService.DeleteSetting<ReservationsSynchronizationSettings>();

            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendars");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendars");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.SynchronizationInterval");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendar.ProductId");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendar.ProductId.Required");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendar.ResourceSystemName");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendar.Url");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendar.Url.Required");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendar.ProductId");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendar.ProductId.Required");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendar.ResourceSystemName");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendar.Url");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendar.PastDays");

            await base.Uninstall();
        }
    }
}
