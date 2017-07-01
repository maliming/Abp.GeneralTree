using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.GeneralTree.GeneralTree;
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

            IocManager.Register(typeof(IGeneralTreeManager<,>), typeof(GeneralTreeManager<,>),
                DependencyLifeStyle.Transient);

            IocManager.Register(typeof(IGeneralTreeManagerWithReferenceType<,>), typeof(GeneralTreeManagerWithReferenceType<,>),
                DependencyLifeStyle.Transient);
        }
    }
}
