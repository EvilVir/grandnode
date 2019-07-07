using Grand.Core;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Payments;
using Grand.Plugin.Payments.DotPay.Models.Api;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Grand.Plugin.Payments.DotPay.Services
{
    public class DotPayApiService
    {
        protected readonly DotPaySettings _settings;
        protected readonly ILocalizationService _localizationService;
        protected readonly IUrlHelperFactory _urlHelperFactory;
        protected readonly IActionContextAccessor _actionContextAccessor;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly IOrderService _orderService;
        protected readonly IOrderProcessingService _orderProcessingService;
        protected readonly IWorkContext _workContext;
        protected readonly IWebHelper _webHelper;

        protected string RedirectionUrl => _settings.UseSandbox ? "https://ssl.dotpay.pl/test_payment/" : "https://ssl.dotpay.pl/t2/";

        public DotPayApiService(DotPaySettings settings, 
            ILocalizationService localizationService, 
            IUrlHelperFactory urlHelperFactory, 
            IActionContextAccessor actionContextAccessor, 
            IHttpContextAccessor httpContextAccessor, 
            IOrderService orderService, 
            IOrderProcessingService orderProcessingService,
            IWorkContext workContext,
            IWebHelper webHelper)
        {
            this._settings = settings;
            this._localizationService = localizationService;
            this._urlHelperFactory = urlHelperFactory;
            this._actionContextAccessor = actionContextAccessor;
            this._httpContextAccessor = httpContextAccessor;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._workContext = workContext;
            this._webHelper = webHelper;
        }

        public async Task RedirectToPayGate(Order order)
        {
            var storeLocation = _webHelper.GetStoreLocation();
            var redirectUrl = $"{storeLocation}/{GetUrlHelper().Action("HandleCustomerReturn", "PaymentDotPay")}";
            redirectUrl = new Regex("(?<!:)/{2,}").Replace(redirectUrl, "/");

            var paymentRequest = new PaymentRequest() {
                Id = _settings.ShopId,
                Amount = order.OrderTotal,
                Currency = order.CustomerCurrencyCode.ToUpperInvariant(),
                Description = $"{_localizationService.GetResource("Checkout.OrderNumber")} {order.OrderNumber}",
                Url = redirectUrl,
                Type = DotPayRedirectionType.BackButton,
                ByLaw = true,
                PersonalData = true,
                Control = order.Id,
                Lang = _workContext.WorkingLanguage.UniqueSeoCode,
                Firstname = order.FirstName,
                Lastname = order.LastName,
                Email = order.CustomerEmail
            };

            paymentRequest.Signature = CalculateSignature(_settings.Pin, paymentRequest);

            var queryParameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(paymentRequest)).Where(x => !string.IsNullOrEmpty(x.Value)).ToDictionary(k => k.Key, v => v.Value);
            var finalUrl = QueryHelpers.AddQueryString(RedirectionUrl, queryParameters);

            await _orderService.InsertOrderNote(new OrderNote {
                Note = $"Redirecting client to DotPay paygate: {finalUrl}",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,
                GenericAttributes = ConvertToGenericAttributesList(paymentRequest)
            });

            _httpContextAccessor.HttpContext.Response.Redirect(finalUrl);

            await Task.CompletedTask;
        }

        public async Task HandleCallback(CallbackData data)
        {
            if (!CheckSignature(_settings.Pin, data.Signature, data))
            {
                throw new GrandException("Signature verfication failed");
            }

            if (data.OperationType == DotPayOperationType.Payment)
            {
                await HandlePaymentCallback(data);
            }
            else
            {
                throw new GrandException($"Unsupported operation type {data.OperationType}");
            }
        }

        protected async Task HandlePaymentCallback(CallbackData data)
        {
            var order = await _orderService.GetOrderById(data.Control);

            if (order == null)
            {
                throw new GrandException($"Order {data.Control} not found");
            }

            await _orderService.InsertOrderNote(new OrderNote {
                Note = $"Received callback from DotPay",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id,
                GenericAttributes = ConvertToGenericAttributesList(data)
            });

            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                throw new GrandException($"Order {order.Id} already paid");
            }

            var newStatus = order.PaymentStatus;

            switch (data.OperationStatus)
            {
                case DotPayOperationStatus.Completed:
                    newStatus = PaymentStatus.Paid;
                    break;

                default:
                case DotPayOperationStatus.New:
                case DotPayOperationStatus.Processing:
                case DotPayOperationStatus.ProcessingRealization:
                case DotPayOperationStatus.ProcessingRealizationWaiting:
                    newStatus = PaymentStatus.Pending;
                    break;


                case DotPayOperationStatus.Rejected:
                    newStatus = PaymentStatus.Voided;
                    break;

            }

            if (newStatus == PaymentStatus.Paid)
            {
                if (!Math.Round(data.OperationAmount, 2).Equals(Math.Round(order.OrderTotal, 2)) || !data.OperationCurrency.ToUpperInvariant().Equals(order?.CustomerCurrencyCode.ToUpperInvariant()))
                {
                    var error = $"Payment operation [{data.OperationNumber}] value or currency [{data.OperationAmount} {data.OperationCurrency}] doesn't match order total and currency [{order.OrderTotal} {order.CustomerCurrencyCode}]";

                    await _orderService.InsertOrderNote(new OrderNote {
                        Note = error,
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = order.Id,
                    });

                    throw new Exception(error);
                }
                else
                {
                    if (await _orderProcessingService.CanMarkOrderAsPaid(order))
                    {
                        order.AuthorizationTransactionId = data.OperationNumber;
                        await _orderService.UpdateOrder(order);
                        await _orderProcessingService.MarkOrderAsPaid(order);
                    }
                    else
                    {
                        var error = $"Order {order.Id} can't be marked as paid";

                        await _orderService.InsertOrderNote(new OrderNote {
                            Note = error,
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow,
                            OrderId = order.Id,
                        });

                        throw new Exception(error);
                    }
                }
            }
        }

        protected IList<GenericAttribute> ConvertToGenericAttributesList(object data)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(data))
                              .Select(x => new GenericAttribute() { Key = x.Key, Value = x.Value }).ToList();
        }

        protected bool CheckSignature<T>(string pin, string signature, T data)
        {
            var calculatedSignature = CalculateSignature(pin, data);
            return signature?.Equals(calculatedSignature) == true;
        }

        protected string CalculateSignature<T>(string pin, T data)
        {
            var valuesToSign = new Dictionary<string, string> {
                { "pin", pin }
            };

            var serializedObject = JsonConvert.SerializeObject(data);
            var allValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(serializedObject).ToDictionary(k => k.Key.Replace("_", "").ToLowerInvariant(), v => v.Value);
            var allProperties = typeof(T).GetProperties().Select(x => new { Property = x, Attribute = x.GetCustomAttribute<UseForSignatureAttribute>(true) })
                                                        .Where(x => x.Attribute != null)
                                                        .OrderBy(x => x.Attribute.Order)
                                                        .Select(x => x.Property.Name.ToLowerInvariant())
                                                        .ToList();

            foreach (var propertyName in allProperties)
            {
                if (allValues.ContainsKey(propertyName) && !string.IsNullOrEmpty(allValues[propertyName]))
                {
                    valuesToSign.Add(propertyName, allValues[propertyName]);
                }
            }

            var stringToSign = string.Join("", valuesToSign.Values);
            var hash = new SHA256Managed();
            var plainTextBytes = Encoding.UTF8.GetBytes(stringToSign);
            var plainHash = hash.ComputeHash(plainTextBytes);
            var hashString = BitConverter.ToString(plainHash).Replace("-", "").ToLower();
            return hashString;
        }

        protected IUrlHelper GetUrlHelper()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
        }
    }
}
