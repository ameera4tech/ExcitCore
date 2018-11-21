using asi.excit.common.Model.config;
using ASI.Contracts.Excit;
using ASI.Contracts.Excit.Supplier.Version1;
using System.Collections.Generic;

namespace asi.excit.common.Interfaces
{
    public interface IExcitContext
    {
        IList<SupplierConfiguration> GetImplementations(int version, Company company);
        IList<SupplierConfiguration> GetImplementation(Configuration.API api, int version, Company company);
    }
}