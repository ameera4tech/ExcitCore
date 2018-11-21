using asi.excit.common.Interfaces;
using asi.excit.common.Interfaces.version1;
using asi.excit.common.Model.config;
using ASI.Contracts.Excit;
using ASI.Contracts.Excit.Inventory.Version1;
using excit.common.Integration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EXCITCoreSample.Controllers
{
    [Route("v1/products/inventory")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IConfigService _configService;
        private readonly IServiceProvider _service;
        public static string InValidValuesForInventory = "InValidValuesForInventory";
        /// <summary>
        /// Constructor and instance
        /// </summary>
        /// <param name="configService"></param>
        /// <param name="notificationService"></param>
        /// <param name="statisticalService"></param>
        /// <param name="logService"></param>
        public InventoryController(IConfigService configService, IServiceProvider service)
        {
            _configService = configService;
            _service = service;
        }

        [HttpPost, Route("")]
        public async Task<Output> GetByProduct([FromBody] InputByProduct input)
        {
            if (input.Products == null) ExceptionUtil.ThrowParameterException("The input needs to include a list of products");
            var stopwatch = Stopwatch.StartNew();
            var output = await ProcessRequest(input).ConfigureAwait(false);
            stopwatch.Stop();
            return new Output();
        }


        private async Task<Output> ProcessRequest(BaseInput input)
        {
            Output output = null;
            IList<SupplierConfiguration> supplierConfiguration = null;
            var start = DateTime.Now;
            var stopwatch = Stopwatch.StartNew();
            Exception capturedException = null;
            try
            {
                if (input == null)
                    throw new Exception(System.Net.HttpStatusCode.BadRequest.ToString());
                if (input.Company == null) ExceptionUtil.ThrowParameterException("The company cannot be null");
                supplierConfiguration = _configService.GetImplementation(ASI.Contracts.Excit.Supplier.Version1.Configuration.API.Inventory, 1, input.Company);
                if (supplierConfiguration == null || supplierConfiguration.Count == 0 || string.IsNullOrEmpty(supplierConfiguration[0].Implementation))
                    ExceptionUtil.ThrowImplementationException("Could not find the configuration");
                if (input.Company.CompanyId == 0) input.Company.CompanyId = supplierConfiguration[0].CompanyId;
                //TODO: dynamic injection via Microsoft dependency injections.
                var implementation = _service.GetService<IInventory>();
                if (implementation == null) ExceptionUtil.ThrowImplementationException("Could not find the implementation for '" + supplierConfiguration[0].Implementation + "'");
                if (!implementation.IsSupported) ExceptionUtil.ThrowGoneException("The implementation is no longer valid '" + supplierConfiguration[0].Implementation + "'");
                if ((input.UserCredentials == null || string.IsNullOrEmpty(input.UserCredentials.Username)) && !string.IsNullOrEmpty(supplierConfiguration[0].DefaultCredentials))
                {
                    string[] defaultCredentials = supplierConfiguration[0].DefaultCredentials.Split(';');
                    input.UserCredentials = new User { Username = defaultCredentials[0], Password = defaultCredentials[1], AccountNumber = defaultCredentials.Length > 2 ? defaultCredentials[2] : string.Empty };
                }
                if (input.UserCredentials == null) input.UserCredentials = new User();
                output = await (implementation.GetByProductAsync((InputByProduct)input, supplierConfiguration).ConfigureAwait(false));
                //The output validate methods validate and sets IsValid property.                
                //output.IsValid = IsValidInventory(output, supplierConfiguration, System.Configuration.ConfigurationManager.AppSettings[InValidValuesForInventory]);
                if (!output.IsValid)
                {
                }
            }
            catch (Exception ex)
            {
                capturedException = ex;
            }
            if (capturedException != null)
            {
                string exceptionMessage = capturedException.Message;
                var asiNumber = input.Company.AsiNumber;
                if (supplierConfiguration.Count > 0) asiNumber = supplierConfiguration[0].ASINumber;
                throw capturedException;
            }
            stopwatch.Stop();
            output.OverallTimings = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 0);
            output.SupplierTimings = Math.Round(output.SupplierTimings, 0);
            return output;
        }

    }
}