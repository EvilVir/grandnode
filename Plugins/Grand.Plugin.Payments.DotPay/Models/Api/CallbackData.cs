using Grand.Plugin.Payments.DotPay.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Grand.Plugin.Payments.DotPay.Models.Api
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class CallbackData
    {
        [UseForSignature(0)]
        public int Id { get; set; }

        [UseForSignature(1)]
        public string OperationNumber { get; set; }

        [UseForSignature(2)]
        public DotPayOperationType OperationType { get; set; }

        [UseForSignature(3)]
        public DotPayOperationStatus OperationStatus { get; set; }

        [UseForSignature(4)]
        public decimal OperationAmount { get; set; }

        [UseForSignature(5)]
        public string OperationCurrency { get; set; }

        [UseForSignature(6)]
        public decimal? OperationWithdrawalAmount { get; set; }

        [UseForSignature(7)]
        public decimal? OperationCommissionAmount { get; set; }

        [UseForSignature(8)]
        [JsonConverter(typeof(DotPayBoolConverter))]
        public bool? IsCompleted { get; set; }

        [UseForSignature(9)]
        public decimal OperationOriginalAmount { get; set; }

        [UseForSignature(10)]
        public string OperationOriginalCurrency { get; set; }

        [JsonConverter(typeof(DotPayDateTimeConverter))]
        [UseForSignature(11)]
        public DateTime OperationDatetime { get; set; }

        [UseForSignature(12)]
        public string OperationRelatedNumber { get; set; }

        [UseForSignature(13)]
        public string Control { get; set; }

        [UseForSignature(14)]
        public string Description { get; set; }

        [UseForSignature(15)]
        public string Email { get; set; }

        [UseForSignature(16)]
        public string PInfo { get; set; }

        [UseForSignature(17)]
        public string PEmail { get; set; }

        [UseForSignature(18)]
        public string CreditCardIssuerIdentificationNumber { get; set; }

        [UseForSignature(19)]
        public string CreditCardMaskedNumber { get; set; }

        [UseForSignature(20)]
        public short? CreditCardExpirationYear { get; set; }

        [UseForSignature(21)]
        public short? CreditCardExpirationMonth { get; set; }

        [UseForSignature(22)]
        public string CreditCardBrandCodename { get; set; }

        [UseForSignature(23)]
        public string CreditCardBrandCode { get; set; }

        [UseForSignature(24)]
        public string CreditCardUniqueIdentifier { get; set; }

        [UseForSignature(25)]
        public string CreditCardId { get; set; }

        [UseForSignature(26)]
        public int Channel { get; set; }

        [UseForSignature(27)]
        public string ChannelCountry { get; set; }

        [UseForSignature(28)]
        public string GeoipCountry { get; set; }

        public string Signature { get; set; }
    }
}
