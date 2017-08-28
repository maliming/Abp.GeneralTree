using Abp.Modules;
using TreeApplication;

namespace TreeTests
{
    [DependsOn(typeof(TreeAppModule))]
    public class TreeTestModule : AbpModule
    {
    }
}