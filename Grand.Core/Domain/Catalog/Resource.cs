namespace Grand.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a resource of reservation product
    /// </summary>
    public partial class Resource : SubBaseEntity
    {
        /// <summary>
        /// Human friendly name
        /// </summary>        
        public string Name { get; set; }

        /// <summary>
        /// System codename
        /// </summary>
        public string SystemName { get; set; }
    }

}
