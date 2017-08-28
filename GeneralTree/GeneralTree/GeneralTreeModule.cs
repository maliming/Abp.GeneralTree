using System.Reflection;
using Abp.Dependency;
using Abp.GeneralTree.GeneralTree;
using Abp.Modules;

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

            IocManager.Register(typeof(IGeneralTreeManagerWithReferenceType<,>),
                typeof(GeneralTreeManagerWithReferenceType<,>),
                DependencyLifeStyle.Transient);
        }
    }
}