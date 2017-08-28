using System.Reflection;
using Abp.EntityFramework;
using Abp.GeneralTree;
using Abp.Modules;

namespace TreeApplication
{
    [DependsOn(typeof(AbpEntityFrameworkModule), typeof(GeneralTreeModule))]
    public class TreeAppModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}