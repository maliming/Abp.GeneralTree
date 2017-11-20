using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;

namespace Abp.GeneralTree.GeneralTree
{
    public class
        GeneralTreeManagerWithReferenceType<TTree, TPrimaryKey> : IGeneralTreeManagerWithReferenceType<TTree,
            TPrimaryKey>
        where TPrimaryKey : class
        where TTree : class, IGeneralTreeWithReferenceType<TTree, TPrimaryKey>, IEntity<TPrimaryKey>
    {
        private readonly IRepository<TTree, TPrimaryKey> _generalTreeRepository;

        public GeneralTreeManagerWithReferenceType(IRepository<TTree, TPrimaryKey> generalTreeRepository)
        {
            _generalTreeRepository = generalTreeRepository;
        }

        [UnitOfWork]
        public virtual async Task CreateAsync(TTree tree)
        {
            tree.Code = await GetNextChildCodeAsync(tree.ParentId);
            tree.Level = tree.Code.Split('.').Length;

            if (tree.ParentId != null) {
                var parent =
                    await _generalTreeRepository.FirstOrDefaultAsync(x => x.Id == tree.ParentId);
                Check.NotNull(parent, nameof(parent));

                tree.FullName = parent.FullName + "-" + tree.Name;
            }
            else {
                //root
                tree.FullName = tree.Name;
            }

            CheckSameName(tree);
            await _generalTreeRepository.InsertAsync(tree);
        }

        [UnitOfWork]
        public virtual async Task UpdateAsync(TTree tree)
        {
            CheckSameName(tree);

            var children = await GetChildrenAsync(tree.Id, true);
            var oldFullName = tree.FullName;

            if (tree.ParentId != null) {
                var parent = await _generalTreeRepository.FirstOrDefaultAsync(x => x.Id == tree.ParentId);
                Check.NotNull(parent, nameof(parent));

                tree.FullName = parent.FullName + "-" + tree.Name;
            }
            else {
                tree.FullName = tree.Name;
            }

            foreach (var child in children) {
                child.FullName = GeneralTreeCodeGenerate.MergeFullName(tree.FullName,
                    GeneralTreeCodeGenerate.RemoveParentCode(child.FullName, oldFullName));
            }
        }

        [UnitOfWork]
        public virtual async Task MoveAsync(TPrimaryKey id, TPrimaryKey parentId)
        {
            var tree = await _generalTreeRepository.GetAsync(id);
            if (tree.ParentId.Equals(parentId)) {
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

            CheckSameName(tree);

            //Update Children Codes and FullName
            foreach (var child in children) {
                child.Code = GeneralTreeCodeGenerate.MergeCode(tree.Code,
                    GeneralTreeCodeGenerate.RemoveParentCode(child.Code, oldCode));
                child.FullName = GeneralTreeCodeGenerate.MergeFullName(tree.FullName,
                    GeneralTreeCodeGenerate.RemoveParentCode(child.FullName, oldFullName));
                child.Level = child.Code.Split('.').Length;
            }
        }

        [UnitOfWork]
        public virtual async Task DeleteAsync(TPrimaryKey id)
        {
            var tree = await _generalTreeRepository.FirstOrDefaultAsync(id);
            if (tree != null) {
                await _generalTreeRepository.DeleteAsync(x => x.Code.StartsWith(tree.Code));
            }
        }

        /// <summary>
        ///     Get next child code
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        private async Task<string> GetNextChildCodeAsync(TPrimaryKey parentId)
        {
            var lastChild =
                _generalTreeRepository.GetAll()
                    .Where(x => x.ParentId == parentId)
                    .OrderByDescending(x => x.Code)
                    .FirstOrDefault();
            if (lastChild != null) {
                //Get the next code
                return GeneralTreeCodeGenerate.GetNextCode(lastChild.Code);
            }

            //Generate a code
            var parentCode = parentId != null ? await GetCodeAsync(parentId) : null;
            return GeneralTreeCodeGenerate.MergeCode(parentCode, GeneralTreeCodeGenerate.CreateCode(1));
        }

        /// <summary>
        ///     Get Code
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<string> GetCodeAsync(TPrimaryKey id)
        {
            return (await _generalTreeRepository.GetAsync(id)).Code;
        }

        /// <summary>
        ///     Get all children, can be recursively
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        private async Task<List<TTree>> GetChildrenAsync(TPrimaryKey parentId, bool recursive = false)
        {
            if (!recursive) {
                return await _generalTreeRepository.GetAllListAsync(x => x.ParentId == parentId);
            }

            if (parentId == null) {
                return await _generalTreeRepository.GetAllListAsync();
            }

            var code = await GetCodeAsync(parentId);

            return _generalTreeRepository.GetAll().Where(x => x.Code.StartsWith(code) && x.Id != parentId)
                .ToList();
        }

        /// <summary>
        ///     Check if there are same names at the same tree level
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        private void CheckSameName(TTree tree)
        {
            if (_generalTreeRepository.GetAll().Where(x => x.ParentId == tree.ParentId && x.Id != tree.Id)
                .Any(x => x.Name == tree.Name)) {
                throw new UserFriendlyException(
                    $"There is already an tree with name {tree.Name}. Two tree with same name can not be created in same level.");
            }
        }

        /// <summary>
        ///     Get Child FullName Async
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="childFullName"></param>
        /// <returns></returns>
        private async Task<string> GetChildFullNameAsync(TPrimaryKey parentId, string childFullName)
        {
            var parent = await _generalTreeRepository.FirstOrDefaultAsync(x => x.Id == parentId);
            return parent != null ? parent.FullName + "-" + childFullName : childFullName;
        }
    }
}