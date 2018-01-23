using System;
using System.Threading.Tasks;

namespace Abp.GeneralTree
{
    public interface IGeneralTreeManager<TTree, TPrimaryKey>
        where TPrimaryKey : struct
        where TTree : class, IGeneralTree<TTree, TPrimaryKey>
    {
        Task CreateAsync(TTree tree);

        Task BulkCreateAsync(TTree tree);

        Task FillUpAsync(TTree tree);

        Task UpdateAsync(TTree tree, Action<TTree> childrenAction = null);

        Task MoveAsync(TPrimaryKey id, TPrimaryKey? parentId, Action<TTree> childrenAction = null);

        Task DeleteAsync(TPrimaryKey id);
    }
}