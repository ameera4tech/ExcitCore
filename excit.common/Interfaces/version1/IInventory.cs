using asi.excit.common.Model.config;
using ASI.Contracts.Excit.Inventory.Version1;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace asi.excit.common.Interfaces.version1
{
    /// <summary>
    /// Interface for specific implementations of Inventory
    /// </summary>
    public interface IInventory : IService
    {
        /// <summary>
        /// Get the inventory details of a product from the supplier
        /// </summary>
        /// <param name="input"></param>
        /// <param name="supplierConfiguration"></param>
        /// <returns></returns>
	    Task<Output> GetByProductAsync(InputByProduct input, IList<SupplierConfiguration> supplierConfiguration);
        
        /// <summary>
        /// Name of the implementation.
        /// </summary>
        string ImplementationName { get; }
    }
}
