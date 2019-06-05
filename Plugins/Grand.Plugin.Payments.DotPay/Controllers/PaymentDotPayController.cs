using Grand.Core;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Grand.Plugin.Payments.DotPay.Models;
using Grand.Plugin.Payments.DotPay.Models.Api;
using Grand.Plugin.Payments.DotPay.Services;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Stores;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Plugin.Payments.DotPay.Controllers
{
    public class PaymentDotPayController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly DotPayApiService _dotPayApiService;

        public PaymentDotPayController(IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            ILocalizationService localizationService,
            DotPayApiService dotPayApiService)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._dotPayApiService = dotPayApiService;
        }

        [AuthorizeAdmin]
        [Area("Admin")]
        public async Task<IActionResult> Configure()
        {
            var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var dotPaySettings = _settingService.LoadSetting<DotPaySettings>(storeScope);

            var model = new DotPaySettingsModel() {
                UseSandbox = dotPaySettings.UseSandbox,
                Pin = dotPaySettings.Pin,
                ShopId = dotPaySettings.ShopId,
                ActiveStoreScopeConfiguration = storeScope,
            };

            if (!string.IsNullOrEmpty(storeScope))
            {
                model.UseSandbox_OverrideForStore = _settingService.SettingExists(dotPaySettings, x => x.UseSandbox, storeScope);
                model.ShopId_OverrideForStore = _settingService.SettingExists(dotPaySettings, x => x.ShopId, storeScope);
                model.Pin_OverrideForStore = _settingService.SettingExists(dotPaySettings, x => x.Pin, storeScope);
            }

            return View("~/Plugins/Payments.DotPay/Views/PaymentDotPay/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area("Admin")]
        public async Task<IActionResult> Configure(DotPaySettingsModel model)
        {
            if (!ModelState.IsValid)
            {
                return await Configure();
            }

            var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var dotPaySettings = _settingService.LoadSetting<DotPaySettings>(storeScope);

            dotPaySettings.UseSandbox = model.UseSandbox;
            dotPaySettings.Pin = model.Pin;
            dotPaySettings.ShopId = model.ShopId;

            if (model.UseSandbox_OverrideForStore || string.IsNullOrEmpty(storeScope))
            {
                await _settingService.SaveSetting(dotPaySettings, x => x.UseSandbox, storeScope, false);
            }
            else if (!string.IsNullOrEmpty(storeScope))
            {
                await _settingService.DeleteSetting(dotPaySettings, x => x.UseSandbox, storeScope);
            }

            if (model.ShopId_OverrideForStore || string.IsNullOrEmpty(storeScope))
            {
                await _settingService.SaveSetting(dotPaySettings, x => x.ShopId, storeScope, false);
            }
            else if (!string.IsNullOrEmpty(storeScope))
            {
                await _settingService.DeleteSetting(dotPaySettings, x => x.ShopId, storeScope);
            }

            if (model.Pin_OverrideForStore || string.IsNullOrEmpty(storeScope))
            {
                await _settingService.SaveSetting(dotPaySettings, x => x.Pin, storeScope, false);
            }
            else if (!String.IsNullOrEmpty(storeScope))
            {
                await _settingService.DeleteSetting(dotPaySettings, x => x.Pin, storeScope);
            }

            await _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return await Configure();
        }

        public IActionResult HandleCustomerReturn(string status)
        {
            return View("~/Plugins/Payments.DotPay/Views/PaymentDotPay/CustomerReturn.cshtml", status.Equals("OK"));
        }

        [HttpPost]
        public async Task<IActionResult> HandleCallback()
        {
            try
            {
                // Yeah, I could use IModelBinder here :)
                var formValues = HttpContext.Request.Form.ToDictionary(k => k.Key, v => v.Value.First());
                var serializedFormValues = JsonConvert.SerializeObject(formValues);
                var data = JsonConvert.DeserializeObject<CallbackData>(serializedFormValues);

                await _dotPayApiService.HandleCallback(data);
                return Content("OK");
            }
            catch (Exception e)
            {
                return Content($"ERR: [{e.GetType().Name}] {e.Message}");
            }
        }
    }
}
