using System;
using Abp.Domain.Entities;

namespace Abp.GeneralTree
{
    public class GeneralTreeConfiguration<TTree, TPrimaryKey> : IGeneralTreeConfiguration<TTree, TPrimaryKey>
        where TPrimaryKey : struct
        where TTree : class, IGeneralTree<TTree, TPrimaryKey>, IEntity<TPrimaryKey>
    {
        public GeneralTreeConfiguration()
        {
            ExceptionMessageFactory = tree =>
                $"There is already an tree with name {tree.Name}. Two tree with same name can not be created in same level.";
        }

        public Func<IGeneralTree<TTree, TPrimaryKey>, string> ExceptionMessageFactory { get; set; }
    }

    public class
        GeneralTreeConfigurationWithReferenceType<TTree, TPrimaryKey> : IGeneralTreeConfigurationWithReferenceType<TTree
            , TPrimaryKey>
        where TPrimaryKey : class
        where TTree : class, IGeneralTreeWithReferenceType<TTree, TPrimaryKey>, IEntity<TPrimaryKey>
    {
        public GeneralTreeConfigurationWithReferenceType()
        {
            ExceptionMessageFactory = tree =>
                $"There is already an tree with name {tree.Name}. Two tree with same name can not be created in same level.";
        }

        public Func<IGeneralTreeWithReferenceType<TTree, TPrimaryKey>, string> ExceptionMessageFactory { get; set; }
    }
}