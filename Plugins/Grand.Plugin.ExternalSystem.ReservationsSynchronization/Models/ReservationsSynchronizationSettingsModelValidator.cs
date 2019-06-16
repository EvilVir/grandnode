using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;

namespace Grand.Plugin.ExternalSystem.ReservationsSynchronization.Models
{
    public class ReservationsSynchronizationSettingsModelValidator : BaseGrandValidator<ReservationsSynchronizationSettingsModel>
    {
        public class ExternalCalendarModelValidator : BaseGrandValidator<ReservationsSynchronizationSettingsModel.ExternalCalendarModel>
        {
            public ExternalCalendarModelValidator(ILocalizationService localizationService)
            {
                RuleFor(x => x.ProductId).NotEmpty().WithMessage(localizationService.GetResource("Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendar.ProductId.Required"));
                RuleFor(x => x.Url).NotEmpty().WithMessage(localizationService.GetResource("Plugins.ExternalSystem.ReservationsSynchronization.Fields.ExternalCalendar.Url.Required"));
            }
        }

        public class PublishedCalendarModelValidator : BaseGrandValidator<ReservationsSynchronizationSettingsModel.PublishedCalendarModel>
        {
            public PublishedCalendarModelValidator(ILocalizationService localizationService)
            {
                RuleFor(x => x.ProductId).NotEmpty().WithMessage(localizationService.GetResource("Plugins.ExternalSystem.ReservationsSynchronization.Fields.PublishedCalendar.ProductId.Required"));
            }
        }

        public ReservationsSynchronizationSettingsModelValidator(ILocalizationService localizationService)
        {
        }
    }
}
