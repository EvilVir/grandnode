using System;

namespace Grand.Plugin.Payments.DotPay.Models.Api
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class UseForSignatureAttribute : Attribute
    {
        public int Order { get; protected set; }

        public UseForSignatureAttribute(int order)
        {
            Order = order;
        }
    }
}
