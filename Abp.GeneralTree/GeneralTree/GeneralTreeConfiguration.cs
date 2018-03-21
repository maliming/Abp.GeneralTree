using System;
using System.Linq.Expressions;

namespace Abp.GeneralTree
{
    public class GeneralTreeConfiguration<TTree, TPrimaryKey> : IGeneralTreeConfiguration<TTree, TPrimaryKey>
        where TPrimaryKey : struct
        where TTree : class, IGeneralTree<TTree, TPrimaryKey>
    {
        public GeneralTreeConfiguration()
        {
            CheckSameNameExpression = null;

            ExceptionMessageFactory = tree =>
                $"There is already an tree with name {tree.Name}. Two tree with same name can not be created in same level.";

            Hyphen = "-";
        }

        public Func<TTree, TTree, bool> CheckSameNameExpression { get; set; }

        public Func<IGeneralTree<TTree, TPrimaryKey>, string> ExceptionMessageFactory { get; set; }

        public string Hyphen { get; set; }
    }

    public class
        GeneralTreeConfigurationWithReferenceType<TTree, TPrimaryKey> : IGeneralTreeConfigurationWithReferenceType<TTree
            , TPrimaryKey>
        where TPrimaryKey : class
        where TTree : class, IGeneralTreeWithReferenceType<TTree, TPrimaryKey>
    {
        public GeneralTreeConfigurationWithReferenceType()
        {
            CheckSameNameExpression = null;

            ExceptionMessageFactory = tree =>
                $"There is already an tree with name {tree.Name}. Two tree with same name can not be created in same level.";

            Hyphen = "-";
        }

        public Func<TTree, TTree, bool> CheckSameNameExpression { get; set; }

        public Func<IGeneralTreeWithReferenceType<TTree, TPrimaryKey>, string> ExceptionMessageFactory { get; set; }

        public string Hyphen { get; set; }
    }
}