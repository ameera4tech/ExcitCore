using asi.excit.common.Interfaces.version1;
using asi.excit.common.Model.config;
using asi.excit.common.Util;
using ASI.Contracts.Excit;
using ASI.Contracts.Excit.Additional;
using ASI.Contracts.Excit.Inventory.Version1;
using excit.common.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace asi.excit.common.Integration.StormCreek
{
    public class Inventory : IInventory, ILoginValidate
    {
        public Inventory()
        {
        }

        public async Task<Output> GetByProductAsync(InputByProduct input, IList<SupplierConfiguration> supplierConfiguration)
        {
            var start = DateTime.Now;

            if (input.UserCredentials == null || input.Products == null || input.Products.Count == 0)
                ExceptionUtil.ThrowParameterException("User credentials or products are missing");

            if (input.Products.Any(p => string.IsNullOrEmpty(p.Number) || string.IsNullOrWhiteSpace(p.Number)))
            {
                ExceptionUtil.ThrowParameterException("Could not find the product number for all products");
            }

            string baseUrl = supplierConfiguration[0].Url;
            var output = await ProcessInventory(input, baseUrl).ConfigureAwait(false);

            output.OverallTimings = Math.Round((DateTime.Now - start).TotalMilliseconds, 0);

            return output;
        }

        private async Task<Output> ProcessInventory(InputByProduct input, string baseUrl)
        {
            var output = new Output
            {
                ProductQuantities = new List<ProductQuantities>()
            };

            var variants = from p in input.Products
                           select new
                           {
                               ProductNumber = p.Number ?? "InvalidNumber",
                               SKUs = p.SKU,
                           };

            var productResponseCache = new System.Collections.Concurrent.ConcurrentDictionary<string, IList<SupplierQuantity>>();
            string url = string.Empty;
            string credentials = string.Format("{0}:{1}", input.UserCredentials.Username, input.UserCredentials.Password);

            foreach (var v in variants)
            {
                productResponseCache.TryAdd(v.ProductNumber, new List<SupplierQuantity>());
                url = baseUrl + "&Product_ID=" + v.ProductNumber + "&-find";
                var result = await GetResponseResult(v, url, credentials).ConfigureAwait(false);
                if (result != null)
                {
                    result.ForEach(item =>
                    {
                        if (item != null)
                        {
                            productResponseCache[v.ProductNumber].Add(item);
                        }
                    });
                    if (v.SKUs != null && v.SKUs.Count > 0)
                    {
                        productResponseCache[v.ProductNumber] = productResponseCache[v.ProductNumber].Where(a => v.SKUs.Any(sku => sku.SKU == a.Code)).ToList();
                    }
                }
            }
            ProcessInventoryResult(productResponseCache, output);
            return output;
        }

        private void ProcessInventoryResult(IDictionary<string, IList<SupplierQuantity>> productResponseCache, Output output)
        {
            //iterate through each input product and get the inventory for each one
            if (productResponseCache != null && productResponseCache.Count > 0)
            {
                output.SupplierTimings = productResponseCache.SelectMany(p => p.Value).Aggregate(0d, (total, rec) => total += rec.SupplierTimings);
                foreach (var product in productResponseCache)
                {
                    var outputQuantity = new ProductQuantities
                    {
                        ProductIdentifier = product.Key,
                        ProductDescription = string.Empty,
                    };

                    if (product.Value.Any())
                    {
                        var temp = product.Value[0].Code;
                        //.Quantities[0].PartCode;
                        outputQuantity.Quantities = product.Value.Select(q => new Quantity
                        {
                            Value = q.Quantity,
                            Label = q.Label,
                            PartDescription = q.Description,
                            PartCode = q.Code,
                            Location = q.Location
                        }).ToList();
                    }
                    else
                    {
                        outputQuantity.Quantities = new List<Quantity> { new Quantity { Label = "No inventory data was found", Value = 0 } };
                    }
                    output.ProductQuantities.Add(outputQuantity);
                }
            }
        }

        private async Task<List<SupplierQuantity>> GetResponseResult(dynamic variant, string url, string credentials)
        {
            try
            {
                var startSupplier = DateTime.Now;
                string responseValue;
                var client = HttpClientHelper.GetHttpClient(url, credentials);
                using (var response = await client.GetAsync(url))
                {
                    var responseContent = response.Content.ReadAsStringAsync();
                    responseValue = responseContent.Result.ToString();
                }
                var endSupplier = DateTime.Now;

                List<SupplierQuantity> quantities = null;
                if (!responseValue.Contains("HTTP Status 401 - "))
                {
                    var doc = XDocument.Parse(responseValue);
                    quantities = ParseXML(variant, doc) as List<SupplierQuantity>;
                }

                if (quantities == null) quantities = new List<SupplierQuantity>();
                if (!quantities.Any())
                {
                    quantities.Add(new SupplierQuantity()
                    {
                        Label = responseValue.Contains("HTTP Status 401 - ") ? "Invalid credentials" : null
                    });
                }
                quantities[0].SupplierTimings = Math.Round((endSupplier - startSupplier).TotalMilliseconds, 0);
                return quantities;
            }
            catch (Exception exception)
            {
                throw;
            }

        }

        private List<SupplierQuantity> ParseXML(dynamic variant, XDocument doc)
        {
            var start = DateTime.Now;
            XNamespace nameSpace = doc.Root.Name.NamespaceName;
            List<SupplierQuantity> quantities = null;
            try
            {
                if (doc.Descendants(nameSpace + "resultset").Elements(nameSpace + "record").Any())
                {
                    quantities = doc.Descendants(nameSpace + "fmresultset")
                                    .Descendants(nameSpace + "resultset")
                                    .Elements(nameSpace + "record")
                                    .Select(recordElement => recordElement.Elements(nameSpace + "field"))
                                    .Select(fieldElement => new SupplierQuantity()
                                    {
                                        Code = fieldElement != null && fieldElement.Attributes().Any() ? fieldElement.Where(x => x.Attribute("name").Value == "SKU").FirstOrDefault().Value : string.Empty,
                                        Label = fieldElement != null && fieldElement.Attributes().Any() ?
                                                    !string.IsNullOrEmpty(fieldElement.Where(x => x.Attribute("name").Value == "Qty_Avail_Alloc").FirstOrDefault().Value) ?
                                                        fieldElement.Where(x => x.Attribute("name").Value == "Qty_Avail_Alloc").FirstOrDefault().Value :
                                                    "Quantity not available" :
                                                string.Empty,
                                        Description = fieldElement != null && fieldElement.Attributes().Any() ?
                                                        "Color " + fieldElement.Where(x => x.Attribute("name").Value == "Attrib_2").FirstOrDefault().Value +
                                                        " - Size " + fieldElement.Where(x => x.Attribute("name").Value == "Attrib_4").FirstOrDefault().Value :
                                                      string.Empty,
                                        Location = null,
                                    }).ToList();

                }
                else if (doc.Descendants(nameSpace + "error").Any())
                {
                    quantities = doc.Descendants(nameSpace + "fmresultset")
                                    .Elements(nameSpace + "error")
                                    .Select(x => new SupplierQuantity
                                    {
                                        Label = x.Attribute("code") != null && x.Attribute("code").Value != "401" && x.Attribute("code").Value != "959" ? x.Attribute("code").Value : "Product not found",
                                        Code = variant.ProductNumber,
                                    }).ToList();
                }
            }
            catch (Exception exception)
            {
                quantities = new List<SupplierQuantity>()
                {
                    new SupplierQuantity
                    {
                        Label = exception.Message,
                        Code = variant.ProductNumber
                    }
                };
            }
            return quantities;
        }

        public async Task<ASI.Contracts.Excit.LoginValidate.Version1.Output> ValidateCredentialsAsync(BaseInput input, IList<SupplierConfiguration> supplierConfiguration)
        {
            var output = new ASI.Contracts.Excit.LoginValidate.Version1.Output() { IsValid = false };

            if (supplierConfiguration == null || supplierConfiguration.Count == 0)
                ExceptionUtil.ThrowParameterException("The configuration cannot be null");

            var start = DateTime.Now;

            try
            {
                if (input.UserCredentials != null && input.UserCredentials.Username != null && input.UserCredentials.Password != null)
                {
                    var startSupplier = DateTime.Now;
                    string BaseUrl = supplierConfiguration[0].Url;
                    var productInput = new InputByProduct()
                    {
                        UserCredentials = new User { Username = input.UserCredentials.Username, Password = input.UserCredentials.Password },
                        Products = new List<Product>()
                        {
                            new Product() { Number = "TestingCredentials" }
                        }
                    };

                    var productOutput = await ProcessInventory(productInput, BaseUrl).ConfigureAwait(false);

                    if (productOutput != null &&
                        !productOutput.ProductQuantities.Any(e => e.Quantities.Any(userAuthenticationError => userAuthenticationError.Label.Contains("Invalid credentials"))))
                    {
                        output.SupplierTimings = Math.Round((DateTime.Now - startSupplier).TotalMilliseconds, 0);
                        output.IsValid = true;
                    }
                }
            }
            catch (Exception exception)
            {
                if (!exception.Message.Contains("401"))
                {
                    throw;
                }
            }
            var end = DateTime.Now;
            return output;
        }

        public bool IsSupported
        {
            get { return true; }
        }

        public string[] RequiredProperties
        {
            get
            {
                return new string[] {
                    ASI.Contracts.Excit.Supplier.Version1.Login.UsernameName,
                    ASI.Contracts.Excit.Supplier.Version1.Login.PasswordName
                };
            }
        }

        public string ImplementationName => "StormCreekInventory";
    }
}