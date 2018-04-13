using Abp.Configuration.Startup;
using Abp.GeneralTree.GeneralTree;

namespace Abp.GeneralTree
{
    public static class GeneralTreeConfigurationExtensions
    {
        public static IGeneralTreeCodeGenerateConfiguration CodeGenerate(
            this IModuleConfigurations moduleConfigurations)
        {
            return moduleConfigurations.AbpConfiguration
                .Get<IGeneralTreeCodeGenerateConfiguration>();
        }

        public static IGeneralTreeConfiguration<TTree, TPrimaryKey> GeneralTree<TTree, TPrimaryKey>(
            this IModuleConfigurations moduleConfigurations)
            where TPrimaryKey : struct
            where TTree : class, IGeneralTree<TTree, TPrimaryKey>
        {
            return moduleConfigurations.AbpConfiguration.Get<IGeneralTreeConfiguration<TTree, TPrimaryKey>>();
        }

        public static IGeneralTreeConfigurationWithReferenceType<TTree, TPrimaryKey> GeneralTreeWithReferenceType<TTree,
            TPrimaryKey>(
            this IModuleConfigurations moduleConfigurations)
            where TPrimaryKey : class
            where TTree : class, IGeneralTreeWithReferenceType<TTree, TPrimaryKey>
        {
            return moduleConfigurations.AbpConfiguration
                .Get<IGeneralTreeConfigurationWithReferenceType<TTree, TPrimaryKey>>();
        }
    }
}