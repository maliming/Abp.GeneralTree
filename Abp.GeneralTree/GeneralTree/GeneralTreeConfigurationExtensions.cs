using Abp.Configuration.Startup;
using Abp.Domain.Entities;

namespace Abp.GeneralTree
{
    public static class GeneralTreeConfigurationExtensions
    {
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
            where TTree : class, IGeneralTreeWithReferenceType<TTree, TPrimaryKey>, IEntity<TPrimaryKey>
        {
            return moduleConfigurations.AbpConfiguration
                .Get<IGeneralTreeConfigurationWithReferenceType<TTree, TPrimaryKey>>();
        }
    }
}