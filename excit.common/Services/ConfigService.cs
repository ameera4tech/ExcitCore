using asi.excit.common.Interfaces;
using asi.excit.common.Model.config;
using ASI.Contracts.Excit;
using ASI.Contracts.Excit.Supplier.Version1;
using System.Collections.Generic;

namespace excit.common.Services
{
    public class ConfigService : IConfigService
    {
        private readonly IExcitContext _excitContext;

        public ConfigService(IExcitContext excitContext)
        {
            _excitContext = excitContext;
        }

        IList<SupplierConfiguration> IConfigService.GetImplementation(Configuration.API api, int version, Company company)
        {
            var supplierConfiguration = _excitContext.GetImplementation(api, version, company);
            return supplierConfiguration;
        }

        IList<SupplierConfiguration> IConfigService.GetImplementations(int version, Company company)
        {
            return _excitContext.GetImplementations(version, company);
        }

        IList<asi.excit.common.Model.config.SupplierAPI> IConfigService.GetSuppliers(int majorVersion, int? companyId)
        {
            throw new System.NotImplementedException();
        }

        void IConfigService.UpdateImplementation(SupplierConfiguration config)
        {
            throw new System.NotImplementedException();
        }
    }
}
