using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace Abp.GeneralTree
{
    public interface IGeneralTreeDto<TTree, TPrimaryKey> : IEntityDto<TPrimaryKey>
        where TPrimaryKey : struct
    {
        TPrimaryKey? ParentId { get; set; }

        ICollection<TTree> Children { get; set; }
    }
}