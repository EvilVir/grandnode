using Grand.Core;
using Grand.Core.Domain.Orders;
using Grand.Core.Plugins;
using Grand.Plugin.Payments.DotPay.Controllers;
using Grand.Plugin.Payments.DotPay.Services;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Payments;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Plugin.Payments.DotPay
{
    public class DotPayPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Properties

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public virtual async Task<bool> SupportCapture()
        {
            return await Task.FromResult(false);
        }

        public virtual async Task<bool> SupportPartiallyRefund()
        {
            return await Task.FromResult(false);
        }

        public virtual async Task<bool> SupportRefund()
        {
            return await Task.FromResult(false);
        }

        public virtual async Task<bool> SupportVoid()
        {
            return await Task.FromResult(false);
        }

        public virtual async Task<bool> SkipPaymentInfo()
        {
            return await Task.FromResult(false);
        }

        public virtual async Task<bool> CanRePostProcessPayment(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public virtual async Task<string> PaymentMethodDescription()
        {
            return await Task.FromResult(_localizationService.GetResource("Plugins.Payments.DotPay.PaymentMethodDescription"));
        }

        #endregion

        #region Fields
        protected readonly ISettingService _settingService;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly ILocalizationService _localizationService;
        protected readonly IWebHelper _webHelper;
        protected readonly DotPayApiService _dotPayApiService;
        #endregion

        #region Ctor
        public DotPayPaymentProcessor(ISettingService settingService, IServiceProvider serviceProvider, ILocalizationService localizationService, DotPayApiService dotPayApiService, IWebHelper webHelper)
        {
            this._settingService = settingService;
            this._serviceProvider = serviceProvider;
            this._localizationService = localizationService;
            this._dotPayApiService = dotPayApiService;
            this._webHelper = webHelper;
        }
        #endregion

        #region Methods

        public virtual async Task<ProcessPaymentResult> ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            return await Task.FromResult(result);
        }

        public virtual async Task PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            await _dotPayApiService.RedirectToPayGate(postProcessPaymentRequest.Order);
        }

        public virtual async Task<bool> HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            return await Task.FromResult(false);
        }

        public virtual async Task<decimal> GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return await Task.FromResult(0M);
        }

        public virtual async Task<CapturePaymentResult> Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return await Task.FromResult(result);
        }

        public virtual async Task<RefundPaymentResult> Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return await Task.FromResult(result);
        }

        public virtual async Task<VoidPaymentResult> Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return await Task.FromResult(result);
        }

        public virtual async Task<ProcessPaymentResult> ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return await Task.FromResult(result);
        }

        public virtual async Task<CancelRecurringPaymentResult> CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return await Task.FromResult(result);
        }

        public virtual async Task<IList<string>> ValidatePaymentForm(IFormCollection form)
        {
            var output = new List<string>();

            if (!form.ContainsKey("dotpay-regulations-check") || !(form["dotpay-regulations-check"].FirstOrDefault()?.Equals("checked") == true))
            {
                output.Add(_localizationService.GetResource("Plugins.Payments.DotPay.Redirection.Regulations.Required"));
            }

            if (!form.ContainsKey("dotpay-personal-data-check") || !(form["dotpay-personal-data-check"].FirstOrDefault()?.Equals("checked") == true))
            {
                output.Add(_localizationService.GetResource("Plugins.Payments.DotPay.Redirection.PersonalData.Required"));
            }

            return await Task.FromResult(output);
        }

        public virtual async Task<ProcessPaymentRequest> GetPaymentInfo(IFormCollection form)
        {
            return await Task.FromResult(new ProcessPaymentRequest());
        }

        public virtual void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "PaymentDotPay";
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentDotPay/Configure";
        }

        public virtual Type GetControllerType()
        {
            return typeof(PaymentDotPayController);
        }

        #endregion

        #region Install and Uninstall

        public override async Task Install()
        {
            await _settingService.SaveSetting(new DotPaySettings {
                UseSandbox = true
            });

            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Fields.UseSandbox", "Use Sandbox", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Fields.UseSandbox.Hint", "Check to enable Sandbox (testing environment).", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Fields.UseSandbox", "Piaskownica", "pl-PL");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Fields.UseSandbox.Hint", "Zaznacz, aby pracować w trybie testowym.", "pl-PL");

            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Fields.ShopId", "Shop id", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Fields.ShopId.Hint", "Shop identificator as seen in DotPay's administration settings", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Fields.ShopId", "Identyfikator sklepu", "pl-PL");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Fields.ShopId.Hint", "Identyfikator sklepu z ustawień administracyjnych DotPay", "pl-PL");

            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Fields.Pin", "PIN", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Fields.Pin", "PIN", "pl-PL");

            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.PaymentMethodDescription", "Payment through DotPay payments operator", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.PaymentMethodDescription", "Płatność za pomocą operatora płatności DotPay", "pl-PL");

            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Redirection.Regulations", "I accept <a href=\"https://ssl.dotpay.pl/files/regulamin_dotpay_sa_dokonywania_wplat_w_serwisie_dotpay.pdf\" target=\"_blank\" title=\"regulations\">Payments Regulations</a> and <a href=\"https://www.dotpay.pl/polityka-plikow-cookies/\" target=\"_blank\" title=\"Cookies policy\">cookies policy</a> Dotpay Sp. z o.o..", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Redirection.Regulations.Required", "You must accept DotPay regulations", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Redirection.Regulations", "Akceptuję <a href=\"https://ssl.dotpay.pl/files/regulamin_dotpay_sa_dokonywania_wplat_w_serwisie_dotpay.pdf\" target=\"_blank\" title=\"regulamin płatności\">Regulamin płatności</a> oraz <a href=\"https://www.dotpay.pl/polityka-plikow-cookies/\" target=\"_blank\" title=\"Polityka cookies\">politykę cookies</a> Dotpay Sp. z o.o.", "pl-PL");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Redirection.Regulations.Required", "Musisz zaakceptować regulamin DotPay", "pl-PL");

            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Redirection.PersonalData", "I understand that for this payment process the administrator of my personal data is Dotpay sp. z o.o. (KRS 0000296790), 30-552 Kraków (Polska), Wielicka 72, +48126882600, <a href=\"mailto:bok@dotpay.pl\">bok@dotpay.pl</a>, read <a title=\"regulamin\" target=\"_blank\" href=\"https://ssl.dotpay.pl/t2/cloudfs1/magellan_media/rodo\">full information here</a>.", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Redirection.PersonalData.Required", "You must accept DotPay privacy policy", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Redirection.PersonalData", "Przyjmuję do wiadomości, że w celu realizacji procesu płatności Administratorem moich danych osobowych jest Dotpay sp. z o.o. (KRS 0000296790), 30-552 Kraków (Polska), Wielicka 72, +48126882600, <a href=\"mailto:bok@dotpay.pl\">bok@dotpay.pl</a>, zobacz <a title=\"regulamin\" target=\"_blank\" href=\"https://ssl.dotpay.pl/t2/cloudfs1/magellan_media/rodo\">pełną treść klauzuli informacyjnej</a>.", "pl-PL");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Redirection.PersonalData.Required", "Musisz zaakceptować politykę prywatności DotPay", "pl-PL");

            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.CustomerReturn", "Welcome back!", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.CustomerReturn", "Witaj z powrotem!", "pl-PL");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.CustomerReturn.OK", "Thank you for your payment. You can continue to browse our site as we process your payment. You will be notified via an e-mail once your payment is completed.", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.CustomerReturn.OK", "Dziękujemy za dokonanie płatności. Twoja płatność jest teraz przetwarzana, gdy zostanie ono zakończone wyślemy Ci powiadomienie na maila.", "pl-PL");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.CustomerReturn.Failure", "Unfortunatelly there was some problem with your payment. You can retry it or change the payment method. You'll find your orders under by <a href=\"/order/history\">clicking here</a>.", "en-US");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.CustomerReturn.Failure", "Niestety pojawił się jakiś problem z Twoją płatnością. Możesz spróbować ponownie lub zmienić formę płatności. Swoje zamówienie znajdziesz <a href=\"/order/history\">klikając tutaj</a>.", "pl-PL");

            await base.Install();
        }

        public override async Task Uninstall()
        {
            await _settingService.DeleteSetting<DotPaySettings>();

            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Fields.UseSandbox");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Fields.UseSandbox.Hint");

            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Fields.ShopId");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Fields.ShopId.Hint");

            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Fields.Pin");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Fields.Pin.Hint");

            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.PaymentMethodDescription");

            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Redirection.Regulations");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Redirection.Regulations.Required");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Redirection.PersonalData");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.Redirection.PersonalData.Required");

            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.CustomerReturn");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.CustomerReturn.OK");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.Payments.DotPay.CustomerReturn.Failure");

            await base.Uninstall();
        }

        #endregion
    }
}
