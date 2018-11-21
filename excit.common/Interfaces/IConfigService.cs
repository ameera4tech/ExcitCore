using System.Collections.Generic;
using asi.excit.common.Model.config;
using ASI.Contracts.Excit;

namespace asi.excit.common.Interfaces
{
    public interface IConfigService
    {
        /// <summary>
        /// Gets the configuration data for all apis, version and a company
        /// </summary>
        /// <param name="version"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        IList<SupplierConfiguration> GetImplementations(int version, Company company);

        /// <summary>
        /// Gets the configuration data for api, version and a company
        /// </summary>
        /// <param name="api"></param>
        /// <param name="version"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        IList<SupplierConfiguration> GetImplementation(ASI.Contracts.Excit.Supplier.Version1.Configuration.API api, int version, Company company);

        /// <summary>
        /// Gets list of APIs supported by suppliers for a specific version
        /// </summary>
        /// <param name="majorVersion"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        IList<SupplierAPI> GetSuppliers(int majorVersion, int? companyId = null);

        /// <summary>
        /// Updates the API data for a supplier
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        void UpdateImplementation(SupplierConfiguration config);
    }
}
