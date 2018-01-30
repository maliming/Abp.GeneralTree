using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abp.GeneralTree
{
    public interface IGeneralTreeManagerWithReferenceType<TTree, in TPrimaryKey>
        where TPrimaryKey : class
        where TTree : class, IGeneralTreeWithReferenceType<TTree, TPrimaryKey>
    {
        Task CreateAsync(TTree tree);

        Task BulkCreateAsync(TTree tree, Action<TTree> childrenAction = null);

        Task CreateChildrenAsync(TTree parent, ICollection<TTree> children, Action<TTree> childrenAction = null);

        Task FillUpAsync(TTree tree, Action<TTree> childrenAction = null);

        Task UpdateAsync(TTree tree, Action<TTree> childrenAction = null);

        Task MoveAsync(TPrimaryKey id, TPrimaryKey parentId, Action<TTree> childrenAction = null);

        Task DeleteAsync(TPrimaryKey id);
    }
}