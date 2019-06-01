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

        /// <summary>
        /// If true, then customers will be allowed to book this resource from last moment of it's previous reservation and till first moment of the next reservation.
        /// This is useful for hotels, when room is available again the same day previous customer leaves.
        /// </summary>
        public bool AllowReservationTimeOverlap { get; set; }
    }

}
