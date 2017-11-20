using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Abp.EntityFrameworkCore;
using Abp.Modules;
using Abp.GeneralTree;
using Castle.MicroKernel.Registration;
using Microsoft.EntityFrameworkCore;

namespace TreeApplication
{
    [DependsOn(typeof(AbpEntityFrameworkCoreModule), typeof(GeneralTreeModule))]
    public class TreeAppModule : AbpModule
    {
        public override void PreInitialize()
        {
            IocManager.IocContainer.Register(
                Component
                    .For<DbContextOptions<TreeAppDbContext>>()
                    .Instance(new DbContextOptionsBuilder<TreeAppDbContext>()
                        .UseInMemoryDatabase("MockDB")
                        .Options)
                    .LifestyleSingleton()
            );

        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}
