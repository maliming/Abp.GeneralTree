using System.Reflection;
using Abp.EntityFrameworkCore;
using Abp.GeneralTree;
using Abp.Modules;

namespace TreeApplication
{
    [DependsOn(typeof(AbpEntityFrameworkCoreModule), typeof(GeneralTreeModule))]
    public class TreeAppModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}