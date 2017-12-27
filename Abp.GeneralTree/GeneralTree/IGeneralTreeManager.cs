using System.Threading.Tasks;

namespace Abp.GeneralTree
{
    public interface IGeneralTreeManager<in TTree, TPrimaryKey>
        where TPrimaryKey : struct
        where TTree : class, IGeneralTree<TTree, TPrimaryKey>
    {
        Task CreateAsync(TTree tree);

        Task BulkCreateAsync(TTree tree);

        Task FillUpAsync(TTree tree);

        Task UpdateAsync(TTree tree);

        Task MoveAsync(TPrimaryKey id, TPrimaryKey? parentId);

        Task DeleteAsync(TPrimaryKey id);
    }
}