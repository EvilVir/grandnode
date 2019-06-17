using Grand.Core;
using Grand.Framework.Controllers;
using Grand.Plugin.ExternalSystem.ReservationsSynchronization.Services;
using Grand.Services.Configuration;
using Grand.Services.Stores;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Mvc;
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

        public ReservationsCalendarController(
            ReservationsCalendarPublishingService publishingService,
            IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService)
        {
            this._publishingService = publishingService;
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
        }

        public async Task<IActionResult> GetReservations([Required] string id)
        {
            var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var allSettings = _settingService.LoadSetting<ReservationsSynchronizationSettings>(storeScope);
            var settings = allSettings.PublishedCalendars?.Where(x => x.Id == id).FirstOrDefault();

            if (settings != null)
            {
                var calendar = await _publishingService.GetCalendar(settings.ProductId, settings.ResourceSystemName, settings.PastDays, _workContext.WorkingLanguage.LanguageCulture);
                return Content(new CalendarSerializer().SerializeToString(calendar));
            }
            else
            {
                return NotFound();
            }
        }
    }
}
