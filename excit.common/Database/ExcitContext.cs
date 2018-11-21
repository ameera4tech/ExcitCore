using asi.excit.common.Interfaces;
using asi.excit.common.Model.config;
using ASI.Contracts.Excit;
using ASI.Contracts.Excit.Supplier.Version1;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace excit.common.Database
{
    public class ExcitContext : DbContext, IExcitContext
    {
        public ExcitContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<SupplierConfiguration> SupplierConfigurations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //TODO: setting connection string.
                optionsBuilder.UseSqlServer("Server=192.168.1.10; UID=sa; Password=P@$$w0rd123; Database=EXCIT;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Query<SupplierConfiguration>();
        }

        IList<SupplierConfiguration> IExcitContext.GetImplementations(int version, Company company)
        {
            throw new System.NotImplementedException();
        }

        IList<SupplierConfiguration> IExcitContext.GetImplementation(Configuration.API api, int version, Company company)
        {
            //TODO: setting devMode property.
            var devMode = "false";
            var status = devMode == "false" ? 1 : 0;
            var dataFromStoredProc = this.Query<SupplierConfiguration>().FromSql($"EXECUTE dbo.SP_GETSUPPLIERCONFIG  {company.CompanyId}, {version}, {api.ToString()}, {status}, {company.AsiNumber}").ToList();
            return dataFromStoredProc;
        }
    }
}
