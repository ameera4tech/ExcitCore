namespace asi.excit.common.Model.config
{
    public class SupplierAPI
    {
        /// <summary>
        /// Version of the API the supplier is configured for.
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// The internal company identifier for the supplier.
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// The name of the ompany the API is for
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// Instructions for user on how to get credentials from the supplier
        /// </summary>
        public string LoginInstruction { get; set; }
        /// <summary>
        /// The ASI# for the supplier.
        /// </summary>
        public int AsiNumber { get; set; }
        /// <summary>
        /// A number representing the number of implementations for Inventory API
        /// </summary>
        public int? Inventory { get; set; }
        /// <summary>
        /// Whether the supplier implements the Inventory API
        /// </summary>
        public bool HasInventory
        {
            get
            {
                return Inventory.HasValue && Inventory.Value > 0;
            }
        }
        /// <summary>
        /// Whether the supplier implements the ability to login
        /// </summary>
        public bool HasLogin
        {
            get
            {
                return HasInventory || HasOrderCreation || HasOrderStatus;
            }
        }
        /// <summary>
        /// A number representing the number of implementations for OrderStatus API
        /// </summary>
        public int? OrderStatus { get; set; }
        /// <summary>
        /// Whether the supplier implements the OrderStatus API
        /// </summary>
        public bool HasOrderStatus
        {
            get
            {
                return OrderStatus.HasValue && OrderStatus.Value > 0;
            }
        }
        /// <summary>
        /// A number representing the number of implementations for OrderCreation API
        /// </summary>
        public int? OrderCreation { get; set; }
        /// <summary>
        /// Whether the supplier implements the OrderCreation API
        /// </summary>
        public bool HasOrderCreation
        {
            get
            {
                return OrderCreation.HasValue && OrderCreation.Value > 0;
            }
        }
        /// <summary>
        /// A number representing the number of implementations for Pricing API
        /// </summary>
        public int? Pricing { get; set; }
        /// <summary>
        /// Whether the supplier implements the Pricing API
        /// </summary>
        public bool HasPricing
        {
            get
            {
                return Pricing.HasValue && Pricing.Value > 0;
            }
        }
        /// <summary>
        /// A number representing the number of implementations for supplierLogin API
        /// </summary>
        public int? ServiceProviderLogin { get; set; }
        /// <summary>
        /// Whether the supplier implements the SupplierLogin API
        /// </summary>
        public bool HasServiceProviderLogin
        {
            get
            {
                return ServiceProviderLogin.HasValue && ServiceProviderLogin.Value > 0;
            }
        }
        public int? ProductIntegration { get; set; }
        public bool HasProductIntegration
        {
            get
            {
                return ProductIntegration.HasValue && ProductIntegration.Value > 0;
            }
        }
    }
}