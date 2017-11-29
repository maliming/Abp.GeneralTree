using System;

namespace Abp.GeneralTree
{
    public interface IGeneralTreeConfiguration<TTree, TPrimaryKey>
        where TPrimaryKey : struct
        where TTree : class, IGeneralTree<TTree, TPrimaryKey>
    {
        Func<IGeneralTree<TTree, TPrimaryKey>, string> ExceptionMessageFactory { get; set; }

        string Hyphen { get; set; }
    }

    public interface IGeneralTreeConfigurationWithReferenceType<TTree, TPrimaryKey>
        where TPrimaryKey : class
        where TTree : class, IGeneralTreeWithReferenceType<TTree, TPrimaryKey>
    {
        Func<IGeneralTreeWithReferenceType<TTree, TPrimaryKey>, string> ExceptionMessageFactory { get; set; }

        string Hyphen { get; set; }
    }
}