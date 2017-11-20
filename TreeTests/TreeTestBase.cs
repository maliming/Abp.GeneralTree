using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using Abp;
using Abp.EntityFrameworkCore;
using Abp.TestBase;
using Castle.MicroKernel.Registration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TreeApplication;

namespace TreeTests
{
    public class TreeTestBase : AbpIntegratedTestBase<TreeTestModule>
    {
        protected TreeTestBase()
        {
        }

        protected IDisposable UsingTenantId(int? tenantId)
        {
            var previousTenantId = AbpSession.TenantId;
            AbpSession.TenantId = tenantId;
            return new DisposeAction(() => AbpSession.TenantId = previousTenantId);
        }

        protected void UsingDbContext(Action<TreeAppDbContext> action)
        {
            UsingDbContext(AbpSession.TenantId, action);
        }

        protected Task UsingDbContextAsync(Func<TreeAppDbContext, Task> action)
        {
            return UsingDbContextAsync(AbpSession.TenantId, action);
        }

        protected T UsingDbContext<T>(Func<TreeAppDbContext, T> func)
        {
            return UsingDbContext(AbpSession.TenantId, func);
        }

        protected Task<T> UsingDbContextAsync<T>(Func<TreeAppDbContext, Task<T>> func)
        {
            return UsingDbContextAsync(AbpSession.TenantId, func);
        }

        protected void UsingDbContext(int? tenantId, Action<TreeAppDbContext> action)
        {
            using (UsingTenantId(tenantId)) {
                using (var context = LocalIocManager.Resolve<TreeAppDbContext>()) {
                    //context.DisableAllFilters();
                    action(context);
                    context.SaveChanges();
                }
            }
        }

        protected async Task UsingDbContextAsync(int? tenantId, Func<TreeAppDbContext, Task> action)
        {
            using (UsingTenantId(tenantId)) {
                using (var context = LocalIocManager.Resolve<TreeAppDbContext>()) {
                    //context.DisableAllFilters();
                    await action(context);
                    await context.SaveChangesAsync();
                }
            }
        }

        protected T UsingDbContext<T>(int? tenantId, Func<TreeAppDbContext, T> func)
        {
            T result;

            using (UsingTenantId(tenantId)) {
                using (var context = LocalIocManager.Resolve<TreeAppDbContext>()) {
                    //context.DisableAllFilters();
                    result = func(context);
                    context.SaveChanges();
                }
            }

            return result;
        }

        protected async Task<T> UsingDbContextAsync<T>(int? tenantId, Func<TreeAppDbContext, Task<T>> func)
        {
            T result;

            using (UsingTenantId(tenantId)) {
                using (var context = LocalIocManager.Resolve<TreeAppDbContext>()) {
                    //context.DisableAllFilters();
                    result = await func(context);
                    await context.SaveChangesAsync();
                }
            }

            return result;
        }
    }
}
