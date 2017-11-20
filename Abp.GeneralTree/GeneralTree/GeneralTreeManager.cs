using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
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
            await _generalTreeRepository.InsertAsync(await GenerateTree(tree));
        }
        
        [UnitOfWork]
        public virtual async Task BulkCreateAsync(TTree tree)
        {
            //Traverse Tree
            TraverseTree(await GenerateTree(tree), tree.Children);

            await _generalTreeRepository.InsertAsync(tree);
        }

        private async Task<TTree> GenerateTree(TTree tree)
        {
            tree.Code = await GetNextChildCodeAsync(tree.ParentId);
            tree.Level = tree.Code.Split('.').Length;

            if (tree.ParentId.HasValue) {
                var parent =
                    await _generalTreeRepository.FirstOrDefaultAsync(EqualId(tree.ParentId.Value));
                Check.NotNull(parent, nameof(parent));

                tree.FullName = parent.FullName + "-" + tree.Name;
            }
            else {
                //root
                tree.FullName = tree.Name;
            }

            CheckSameName(tree);

            return tree;
        }

        private static void TraverseTree(TTree parent, ICollection<TTree> children)
        {
            if (children == null || !children.Any())
            {
                return;
            }

            children.ForEach((tree, index) =>
            {
                //CheckSameName
                if (children.Count(x => x.Name == tree.Name) > 1)
                {
                    throw new UserFriendlyException(
                        $"There is already an tree with name {tree.Name}. Two tree with same name can not be created in same level.");
                }

                tree.Code = index == 0
                    ? GeneralTreeCodeGenerate.MergeCode(parent.Code, GeneralTreeCodeGenerate.CreateCode(1))
                    : GeneralTreeCodeGenerate.GetNextCode(children.ElementAt(index - 1).Code);

                tree.Level = tree.Code.Split('.').Length;
                tree.FullName = parent.FullName + "-" + tree.Name;
            });

            children.ForEach(tree =>
            {
                TraverseTree(tree, tree.Children);
            });
        }

        [UnitOfWork]
        public virtual async Task UpdateAsync(TTree tree)
        {
            CheckSameName(tree);

            var children = await GetChildrenAsync(tree.Id, true);
            var oldFullName = tree.FullName;

            if (tree.ParentId.HasValue) {
                var parent = await _generalTreeRepository.FirstOrDefaultAsync(EqualId(tree.ParentId.Value));
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
        public virtual async Task MoveAsync(TPrimaryKey id, TPrimaryKey? parentId)
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
        private async Task<string> GetNextChildCodeAsync(TPrimaryKey? parentId)
        {
            var lastChild =
                _generalTreeRepository.GetAll()
                    .Where(Equal(parentId, "ParentId"))
                    .OrderByDescending(x => x.Code)
                    .FirstOrDefault();
            if (lastChild != null) {
                //Get the next code
                return GeneralTreeCodeGenerate.GetNextCode(lastChild.Code);
            }

            //Generate a code
            var parentCode = parentId != null ? await GetCodeAsync(parentId.Value) : null;
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
        private async Task<List<TTree>> GetChildrenAsync(TPrimaryKey? parentId, bool recursive = false)
        {
            if (!recursive) {
                return await _generalTreeRepository.GetAllListAsync(Equal(parentId, "ParentId"));
            }

            if (!parentId.HasValue) {
                return await _generalTreeRepository.GetAllListAsync();
            }

            var code = await GetCodeAsync(parentId.Value);

            return _generalTreeRepository.GetAll().Where(x => x.Code.StartsWith(code))
                .Where(NotEqualId(parentId.Value)).ToList();
        }

        /// <summary>
        ///     Check if there are same names at the same tree level
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        private void CheckSameName(TTree tree)
        {
            if (_generalTreeRepository.GetAll().Where(Equal(tree.ParentId, "ParentId")).Where(NotEqualId(tree.Id))
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
        private async Task<string> GetChildFullNameAsync(TPrimaryKey? parentId, string childFullName)
        {
            var parent = await _generalTreeRepository.FirstOrDefaultAsync(EqualId(parentId));
            return parent != null ? parent.FullName + "-" + childFullName : childFullName;
        }

        #region EqualExpression

        private static Expression<Func<TTree, bool>> EqualId(TPrimaryKey id)
        {
            var lambdaParam = Expression.Parameter(typeof(TTree));

            var lambdaBody = Expression.Equal(
                Expression.PropertyOrField(lambdaParam, "Id"),
                Expression.Constant(id, typeof(TPrimaryKey))
            );

            return Expression.Lambda<Func<TTree, bool>>(lambdaBody, lambdaParam);
        }

        private static Expression<Func<TTree, bool>> EqualId(TPrimaryKey? id)
        {
            var lambdaParam = Expression.Parameter(typeof(TTree));

            var lambdaBody = Expression.Equal(
                Expression.PropertyOrField(lambdaParam, "Id"),
                Expression.Constant(id, typeof(TPrimaryKey))
            );

            return Expression.Lambda<Func<TTree, bool>>(lambdaBody, lambdaParam);
        }

        private static Expression<Func<TTree, bool>> NotEqualId(TPrimaryKey id)
        {
            var lambdaParam = Expression.Parameter(typeof(TTree));

            var lambdaBody = Expression.NotEqual(
                Expression.PropertyOrField(lambdaParam, "Id"),
                Expression.Constant(id, typeof(TPrimaryKey))
            );

            return Expression.Lambda<Func<TTree, bool>>(lambdaBody, lambdaParam);
        }

        private static Expression<Func<TTree, bool>> NotEqualId(TPrimaryKey? id)
        {
            var lambdaParam = Expression.Parameter(typeof(TTree));

            var lambdaBody = Expression.NotEqual(
                Expression.PropertyOrField(lambdaParam, "Id"),
                Expression.Constant(id, typeof(TPrimaryKey))
            );

            return Expression.Lambda<Func<TTree, bool>>(lambdaBody, lambdaParam);
        }

        private static Expression<Func<TTree, bool>> Equal(TPrimaryKey id, string property)
        {
            var lambdaParam = Expression.Parameter(typeof(TTree));

            var lambdaBody = Expression.Equal(
                Expression.PropertyOrField(lambdaParam, property),
                Expression.Constant(id, typeof(TPrimaryKey))
            );

            return Expression.Lambda<Func<TTree, bool>>(lambdaBody, lambdaParam);
        }

        private static Expression<Func<TTree, bool>> Equal(TPrimaryKey? id, string property)
        {
            var lambdaParam = Expression.Parameter(typeof(TTree));

            var lambdaBody = Expression.Equal(
                Expression.PropertyOrField(lambdaParam, property),
                Expression.Constant(id, typeof(TPrimaryKey?))
            );

            return Expression.Lambda<Func<TTree, bool>>(lambdaBody, lambdaParam);
        }

        private static Expression<Func<TTree, bool>> NotEqual(TPrimaryKey id, string property)
        {
            var lambdaParam = Expression.Parameter(typeof(TTree));

            var lambdaBody = Expression.NotEqual(
                Expression.PropertyOrField(lambdaParam, property),
                Expression.Constant(id, typeof(TPrimaryKey))
            );

            return Expression.Lambda<Func<TTree, bool>>(lambdaBody, lambdaParam);
        }

        private static Expression<Func<TTree, bool>> NotEqual(TPrimaryKey? id, string property)
        {
            var lambdaParam = Expression.Parameter(typeof(TTree));

            var lambdaBody = Expression.NotEqual(
                Expression.PropertyOrField(lambdaParam, property),
                Expression.Constant(id, typeof(TPrimaryKey?))
            );

            return Expression.Lambda<Func<TTree, bool>>(lambdaBody, lambdaParam);
        }

        #endregion
    }
}