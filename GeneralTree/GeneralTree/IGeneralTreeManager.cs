using System.Threading.Tasks;
using Abp.Domain.Entities;

namespace Abp.GeneralTree
{
    public interface IGeneralTreeManager<in TTree, TPrimaryKey>
        where TPrimaryKey : struct
        where TTree : class, IGeneralTree<TTree, TPrimaryKey>, IEntity<TPrimaryKey>
    {
        Task CreateAsync(TTree tree);

        Task UpdateAsync(TTree tree);

        Task MoveAsync(TPrimaryKey id, TPrimaryKey? parentId);

        Task DeleteAsync(TPrimaryKey id);
    }
}