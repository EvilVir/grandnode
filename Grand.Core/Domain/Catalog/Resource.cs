using System.Collections.Generic;
using Grand.Core.Domain.Localization;

namespace Grand.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a resource of reservation product
    /// </summary>
    public partial class Resource : SubBaseEntity
    {
        public Resource()
        {
        }

        /// <summary>
        /// Human friendly name
        /// </summary>        
        public string Name { get; set; }

        /// <summary>
        /// System codename
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// Latitude of place where resource is located
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude of place where resource is located
        /// </summary>
        public double Longitude { get; set; }
    }

}
