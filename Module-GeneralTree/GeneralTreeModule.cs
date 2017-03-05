using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Abp.Modules;
using Castle.MicroKernel.Registration;

namespace Abp.GeneralTree
{
    [DependsOn(typeof(AbpKernelModule))]
    public class GeneralTreeModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}
