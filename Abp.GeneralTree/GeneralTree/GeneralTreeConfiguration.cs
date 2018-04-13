using System;

namespace Abp.GeneralTree
{
    public class GeneralTreeConfiguration<TTree, TPrimaryKey> : IGeneralTreeConfiguration<TTree, TPrimaryKey>
        where TPrimaryKey : struct
        where TTree : class, IGeneralTree<TTree, TPrimaryKey>
    {
        public Func<TTree, TTree, bool> CheckSameNameExpression { get; set; } = null;

        public Func<IGeneralTree<TTree, TPrimaryKey>, string> ExceptionMessageFactory { get; set; } =
            tree =>
                $"There is already an tree with name {tree.Name}. Two tree with same name can not be created in same level.";

        public string Hyphen { get; set; } = "-";
    }

    public class
        GeneralTreeConfigurationWithReferenceType<TTree, TPrimaryKey> : IGeneralTreeConfigurationWithReferenceType<TTree
            , TPrimaryKey>
        where TPrimaryKey : class
        where TTree : class, IGeneralTreeWithReferenceType<TTree, TPrimaryKey>
    {
        public Func<TTree, TTree, bool> CheckSameNameExpression { get; set; } = null;

        public Func<IGeneralTreeWithReferenceType<TTree, TPrimaryKey>, string> ExceptionMessageFactory { get; set; } =
            tree =>
                $"There is already an tree with name {tree.Name}. Two tree with same name can not be created in same level.";

        public string Hyphen { get; set; } = "-";
    }
}