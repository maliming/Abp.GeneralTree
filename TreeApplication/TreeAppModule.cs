using System.Reflection;
using Abp.EntityFrameworkCore;
using Abp.GeneralTree;
using Abp.Modules;

namespace TreeApplication
{
    [DependsOn(typeof(AbpEntityFrameworkCoreModule), typeof(GeneralTreeModule))]
    public class TreeAppModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Modules.GeneralTree<Region, long>().ExceptionMessageFactory =
                tree => $"{tree.Name}已经存在.";

            Configuration.Modules.GeneralTreeWithReferenceType<Region2, string>().ExceptionMessageFactory =
                tree => $"{tree.Name}已经存在.";
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}