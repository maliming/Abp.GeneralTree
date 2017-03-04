using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.GeneralTree;
using Abp.EntityFramework;
using Abp.GeneralTree;
using Abp.Modules;
using Castle.MicroKernel.Registration;

namespace TreeApplication
{
    [DependsOn(typeof(AbpEntityFrameworkModule), typeof(GeneralTreeModule))]
    public class TreeModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}
