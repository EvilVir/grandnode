using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Grand.Plugin.Payments.DotPay.Models.Api
{
    [JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
    public enum DotPayOperationStatus
    {
        New,
        Processing,
        Completed,
        Rejected,
        ProcessingRealizationWaiting,
        ProcessingRealization
    }
}