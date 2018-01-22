using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace Abp.GeneralTree
{
    public interface IGeneralTreeDtoWithReferenceType<TTree, TPrimaryKey> : IEntityDto<TPrimaryKey>
        where TPrimaryKey : class
    {
        TPrimaryKey ParentId { get; set; }

        ICollection<TTree> Children { get; set; }
    }
}