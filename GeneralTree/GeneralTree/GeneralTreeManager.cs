using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.Domain.Uow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.UI;

namespace Abp.GeneralTree
{
    public class GeneralTreeManager<TTree, TPrimaryKey> : IGeneralTreeManager<TTree, TPrimaryKey>
        where TPrimaryKey : struct 
        where TTree : class, IGeneralTree<TTree, TPrimaryKey>, IEntity<TPrimaryKey>
    {
        private readonly IRepository<TTree, TPrimaryKey> _generalTreeRepository;

        public GeneralTreeManager(IRepository<TTree, TPrimaryKey> generalTreeRepository)
        {
            _generalTreeRepository = generalTreeRepository;
        }

        [UnitOfWork]
        public virtual async Task CreateAsync(TTree tree)
        {
            tree.Code = await GetNextChildCodeAsync(tree.ParentId);
            tree.Level = tree.Code.Split('.').Length;

            if (tree.ParentId.HasValue)
            {
                var parent =
                    await _generalTreeRepository.FirstOrDefaultAsync(x => x.Id == tree.ParentId.Value);
                Check.NotNull(parent, nameof(parent));

                tree.FullName = parent.FullName + "-" + tree.Name;
            }
            else
            {
                //root
                tree.FullName = tree.Name;
            }

            await CheckSameNameAsync(tree);
            await _generalTreeRepository.InsertAsync(tree);
        }

        [UnitOfWork]
        public virtual async Task UpdateAsync(TTree tree)
        {
            await CheckSameNameAsync(tree);

            var children = await GetChildrenAsync(tree.Id, true);
            var oldFullName = tree.FullName;

            if (tree.ParentId.HasValue)
            {
                var parent = await _generalTreeRepository.FirstOrDefaultAsync(x => x.Id == tree.ParentId.Value);
                Check.NotNull(parent, nameof(parent));

                tree.FullName = parent.FullName + "-" + tree.Name;
            }
            else
            {
                tree.FullName = tree.Name;
            }

            foreach (var child in children)
            {
                child.FullName = GeneralTreeCodeGenerate.MergeFullName(tree.FullName,
                    GeneralTreeCodeGenerate.RemoveParentCode(child.FullName, oldFullName));
            }
        }

        [UnitOfWork]
        public virtual async Task MoveAsync(long id, long? parentId)
        {
            var tree = await _generalTreeRepository.GetAsync(id);
            if (tree.ParentId == parentId)
            {
                return;
            }

            //Should find children before Code change
            var children = await GetChildrenAsync(id, true);

            //Store old code and full name of Tree
            var oldCode = tree.Code;
            var oldFullName = tree.FullName;

            //Move Tree
            tree.Code = await GetNextChildCodeAsync(parentId);
            tree.Level = tree.Code.Split('.').Length;
            tree.ParentId = parentId;
            tree.FullName = await GetChildFullNameAsync(parentId, tree.Name);

            await CheckSameNameAsync(tree);

            //Update Children Codes and FullName
            foreach (var child in children)
            {
                child.Code = GeneralTreeCodeGenerate.MergeCode(tree.Code,
                    GeneralTreeCodeGenerate.RemoveParentCode(child.Code, oldCode));
                child.FullName = GeneralTreeCodeGenerate.MergeFullName(tree.FullName,
                    GeneralTreeCodeGenerate.RemoveParentCode(child.FullName, oldFullName));
                child.Level = child.Code.Split('.').Length;
            }
        }

        [UnitOfWork]
        public virtual async Task DeleteAsync(long id)
        {
            var children = await GetChildrenAsync(id, true);
            foreach (var child in children)
            {
                await _generalTreeRepository.DeleteAsync(child);
            }
            await _generalTreeRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Get next child code
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        private async Task<string> GetNextChildCodeAsync(long? parentId)
        {
            var lastChild =
                _generalTreeRepository.GetAll()
                    .Where(x => x.ParentId == parentId)
                    .OrderBy(x => x.Code)
                    .ToList()
                    .LastOrDefault();
            if (lastChild != null)
            {
                //Get the next code
                return GeneralTreeCodeGenerate.GetNextCode(lastChild.Code);
            }

            //Generate a code
            var parentCode = parentId != null ? await GetCodeAsync(parentId.Value) : null;
            return GeneralTreeCodeGenerate.MergeCode(parentCode, GeneralTreeCodeGenerate.CreateCode(1));
        }

        /// <summary>
        /// Get Code
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<string> GetCodeAsync(long id)
        {
            return (await _generalTreeRepository.GetAsync(id)).Code;
        }

        /// <summary>
        /// Get all children, can be recursively
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        private async Task<List<TTree>> GetChildrenAsync(long? parentId, bool recursive = false)
        {
            if (!recursive)
            {
                return await _generalTreeRepository.GetAllListAsync(ou => ou.ParentId == parentId);
            }

            if (!parentId.HasValue)
            {
                return await _generalTreeRepository.GetAllListAsync();
            }

            var code = await GetCodeAsync(parentId.Value);

            return await _generalTreeRepository.GetAllListAsync(
                x => x.Code.StartsWith(code) && x.Id != parentId.Value
            );
        }

        /// <summary>
        /// Check if there are same names at the same tree level
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        private async Task CheckSameNameAsync(TTree tree)
        {
            var siblings = (await GetChildrenAsync(tree.ParentId))
                .Where(x => x.Id != tree.Id)
                .ToList();

            if (siblings.Any(x => x.Name == tree.Name))
            {
                throw new UserFriendlyException($"There is already an tree with name { tree.Name }. Two tree with same name can not be created in same level.");
            }
        }

        /// <summary>
        /// Get Child FullName Async
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="childFullName"></param>
        /// <returns></returns>
        private async Task<string> GetChildFullNameAsync(long? parentId, string childFullName)
        {
            var parent = await _generalTreeRepository.FirstOrDefaultAsync(x => x.Id == parentId);
            return parent != null ? parent.FullName + "-" + childFullName : childFullName;
        }
    }
}
