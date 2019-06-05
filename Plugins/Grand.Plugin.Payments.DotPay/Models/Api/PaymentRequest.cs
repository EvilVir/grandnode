using Grand.Plugin.Payments.DotPay.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Grand.Plugin.Payments.DotPay.Models.Api
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class PaymentRequest
    {
        [UseForSignature(0)]
        public string ApiVersion { get; set; }

        [UseForSignature(2)]
        public int Id { get; set; }

        [UseForSignature(3)]
        public string Pid { get; set; }

        [UseForSignature(4)]
        public decimal Amount { get; set; }

        [UseForSignature(5)]
        public string Currency { get; set; }

        [UseForSignature(6)]
        public string Description { get; set; }

        [JsonProperty("chk")]
        public string Signature { get; set; }

        [UseForSignature(8)]
        public int? Channel { get; set; }

        [UseForSignature(9)]
        [JsonConverter(typeof(DotPayBoolConverter))]
        public bool? ChLock { get; set; }

        [UseForSignature(35)]
        [JsonConverter(typeof(DotPayBoolConverter))]
        public bool? IgnoreLastPaymentChannel { get; set; }

        [UseForSignature(10)]
        public string ChannelGroups { get; set; }

        [UseForSignature(11)]
        public string Url { get; set; }

        [UseForSignature(12)]
        public DotPayRedirectionType Type { get; set; }

        [UseForSignature(13)]
        [JsonProperty("buttontext")]
        public string ButtonText { get; set; }

        [UseForSignature(32)]
        [JsonConverter(typeof(DotPayBoolConverter))]
        [JsonProperty("bylaw")]
        public bool? ByLaw { get; set; }

        [UseForSignature(33)]
        [JsonConverter(typeof(DotPayBoolConverter))]
        public bool? PersonalData { get; set; }

        [UseForSignature(14)]
        [JsonProperty("urlc")]
        public string CallbackUrl { get; set; }

        [UseForSignature(30)]
        [JsonConverter(typeof(DotPayDateTimeConverter))]
        public DateTime? ExpirationDate { get; set; }

        [UseForSignature(7)]
        public string Control { get; set; }

        [UseForSignature(15)]
        public string Firstname { get; set; }

        [UseForSignature(16)]
        public string Lastname { get; set; }

        [UseForSignature(17)]
        public string Email { get; set; }

        [UseForSignature(18)]
        public string Street { get; set; }

        [UseForSignature(19)]
        public string StreetN1 { get; set; }

        [UseForSignature(20)]
        public string StreetN2 { get; set; }

        [UseForSignature(21)]
        public string State { get; set; }

        [UseForSignature(22)]
        public string Addr3 { get; set; }

        [UseForSignature(23)]
        public string City { get; set; }

        [UseForSignature(24)]
        public string Postcode { get; set; }

        [UseForSignature(25)]
        public string Phone { get; set; }

        [UseForSignature(26)]
        public string Country { get; set; }

        [UseForSignature(1)]
        public string Lang { get; set; }

        public string Customer { get; set; }

        [UseForSignature(31)]
        [JsonProperty("deladdr")]
        public string DeliveryAddress { get; set; }

        [UseForSignature(27)]
        public string PInfo { get; set; }

        [UseForSignature(28)]
        public string PEmail { get; set; }

        [UseForSignature(34)]
        public string BlikCode { get; set; }

        [UseForSignature(36)]
        [JsonProperty("gp_token")]
        public string GooglePayToken { get; set; }
    }
}
