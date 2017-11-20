using System.Threading.Tasks;
using Abp.Domain.Entities;

namespace Abp.GeneralTree.GeneralTree
{
    public interface IGeneralTreeManagerWithReferenceType<in TTree, in TPrimaryKey>
        where TPrimaryKey : class
        where TTree : class, IGeneralTreeWithReferenceType<TTree, TPrimaryKey>, IEntity<TPrimaryKey>
    {
        Task CreateAsync(TTree tree);

        Task BulkCreateAsync(TTree tree);

        Task UpdateAsync(TTree tree);

        Task MoveAsync(TPrimaryKey id, TPrimaryKey parentId);

        Task DeleteAsync(TPrimaryKey id);
    }
}