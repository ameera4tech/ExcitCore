using System.Collections.Generic;
using asi.excit.common.Model.config;
using ASI.Contracts.Excit;
using ASI.Contracts.Excit.LoginValidate.Version1;
using System.Threading.Tasks;

namespace asi.excit.common.Interfaces.version1
{
    public interface ILoginValidate : IService
    {
        /// <summary>
        /// Get the valide creadentials of supplier
        /// </summary>
        /// <param name="input"></param>
        /// <param name="supplierConfiguration"></param>
        /// <returns></returns>
        Task<Output> ValidateCredentialsAsync(BaseInput input, IList<SupplierConfiguration> supplierConfiguration);

        /// <summary>
        /// List of required properties for the Login functionality
        /// </summary>
        string[] RequiredProperties { get; }
    }
}
