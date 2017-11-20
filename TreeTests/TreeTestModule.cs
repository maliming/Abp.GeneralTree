using System;
using System.Collections.Generic;
using System.Reflection;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore;
using Abp.Modules;
using Abp.Runtime.Session;
using Abp.TestBase;
using Castle.MicroKernel.Registration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using TreeApplication;
using Xunit;
using Castle.Windsor.MsDependencyInjection;

namespace TreeTests
{
    [DependsOn(typeof(TreeAppModule), typeof(AbpTestBaseModule))]
    public class TreeTestModule : AbpModule
    {
        private DbContextOptions<TreeAppDbContext> _hostDbContextOptions;

        private Dictionary<int, DbContextOptions<TreeAppDbContext>> _tenantDbContextOptions;

        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
            SetupInMemoryDb();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }

        private void SetupInMemoryDb()
        {
            var services = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase();

            var serviceProvider = WindsorRegistrationHelper.CreateServiceProvider(
                IocManager.IocContainer,
                services
            );

            var hostDbContextOptionsBuilder = new DbContextOptionsBuilder<TreeAppDbContext>();
            hostDbContextOptionsBuilder.UseInMemoryDatabase("host").UseInternalServiceProvider(serviceProvider);

            _hostDbContextOptions = hostDbContextOptionsBuilder.Options;
            _tenantDbContextOptions = new Dictionary<int, DbContextOptions<TreeAppDbContext>>();

            IocManager.IocContainer.Register(
                Component
                    .For<DbContextOptions<TreeAppDbContext>>()
                    .UsingFactoryMethod((kernel) =>
                    {
                        lock (_tenantDbContextOptions) 
                        {
                            var currentUow = kernel.Resolve<ICurrentUnitOfWorkProvider>().Current;
                            var abpSession = kernel.Resolve<IAbpSession>();

                            var tenantId = currentUow != null ? currentUow.GetTenantId() : abpSession.TenantId;

                            if (tenantId == null) 
                            {
                                return _hostDbContextOptions;
                            }

                            if (!_tenantDbContextOptions.ContainsKey(tenantId.Value)) 
                            {
                                var optionsBuilder = new DbContextOptionsBuilder<TreeAppDbContext>();
                                optionsBuilder.UseInMemoryDatabase(tenantId.Value.ToString()).UseInternalServiceProvider(serviceProvider);
                                _tenantDbContextOptions[tenantId.Value] = optionsBuilder.Options;
                            }

                            return _tenantDbContextOptions[tenantId.Value];
                        }
                    }, true)
                    .LifestyleTransient()
            );
        }
    }
}