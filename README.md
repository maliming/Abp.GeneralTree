
 <img src="https://raw.githubusercontent.com/maliming/Abp.GeneralTree/master/GeneralTree.png" width="200" height="200" /> 

# Abp GeneralTree

[![Build status](https://ci.appveyor.com/api/projects/status/ftg4ttr825gnmabl?svg=true)](https://ci.appveyor.com/project/maliming/Abp-Generaltree)
[![NuGet](https://img.shields.io/nuget/vpre/abp.GeneralTree.svg)](https://www.nuget.org/packages/Abp.GeneralTree)

[GeneralTree中文文档](https://github.com/maliming/Abp.GeneralTree/blob/master/README.CN.md)

- Based on Abp module system, perfect integration Abp framework.
- Support for custom primary key (value type, reference type).
- Automating the assignment of Code,Level,FullName extends other attributes of the entity.
- Efficient management of entities based on Code, Level features.
- **Suitable for managing a variety of tree structure entities, such as: region, organization, category, industry and other entities with parent-child Entity.**

## Installation

``` c#
Install-Package Abp.GeneralTree
dotnet add package Abp.GeneralTree
```

First you need to add the dependency to your module:
``` c#
[DependsOn(typeof(GeneralTreeModule))]
public class YourProjectModule : AbpModule
{
    //...
}
```

GeneralTree provides a generic `IGeneralTree` interface, which inherits this interface, passing in generic parameter entities and primary keys (primary keys can be value types and reference types)

Value type

``` c#
public interface IGeneralTree<TTree, TPrimaryKey> : IEntity<TPrimaryKey>
    where TPrimaryKey : struct
{
      string Name { get; set; }

      string FullName { get; set; }

      string Code { get; set; }

      int Level { get; set; }

      TTree Parent { get; set; }

      TPrimaryKey? ParentId { get; set; }

      ICollection<TTree> Children { get; set; }
}
```

Reference type

``` c#
public interface IGeneralTreeWithReferenceType<TTree, TPrimaryKey> : IEntity<TPrimaryKey>
    where TPrimaryKey : class
{
      string Name { get; set; }

      string FullName { get; set; }

      string Code { get; set; }

      int Level { get; set; }

      TTree Parent { get; set; }

      TPrimaryKey ParentId { get; set; }

      ICollection<TTree> Children { get; set; }
}
```

Take the Region entity as an example:

``` c#
public class Region : Entity<long>, IGeneralTree<Region, long>
{
      public virtual string Name { get; set; }

      public virtual string FullName { get; set; }

      public virtual string Code { get; set; }

      public virtual int Level { get; set; }

      public virtual Region Parent { get; set; }

      public virtual long? ParentId { get; set; }

      public virtual ICollection<Region> Children { get; set; }
}
```

Entities implement properties under generic interfaces, and GeneralTree automatically maintains these properties (FullName, Code, Level, ParentId...)

To create, update, move, delete, etc., use `IGeneralTreeManager<TTree, TPrimaryKey>`, and the generic parameters of the interface are the same as above.

## Use

We first initialize some regional information.

``` c#
var beijing = new Region
{
      Name = "beijing"
};
await _generalRegionTreeManager.CreateAsync(beijing);
```

At this time, the entity information of beijing is as follows:

Id|Name|FullName|Code|Level|ParentId
:--:|:--:|:--:|:--:|:--:|:--:
1|beijing|beijing|00001|1|NULL

GeneralTree automatically maintains the modified properties. It provides the basis for efficient management later.

Add some areas again.

``` c#
var beijing = new Region
{
      Name = "beijing"
};
await _generalRegionTreeManager.CreateAsync(beijing);
await CurrentUnitOfWork.SaveChangesAsync();

var dongcheng = new Region
{
      Name = "dongcheng",
      ParentId = beijing.Id
};

var xicheng = new Region
{
      Name = "xicheng",
      ParentId = beijing.Id
};
await _generalRegionTreeManager.CreateAsync(dongcheng);
await _generalRegionTreeManager.CreateAsync(xicheng);

var hebei = new Region
{
      Name = "hebei"
};
await _generalRegionTreeManager.CreateAsync(hebei);
await CurrentUnitOfWork.SaveChangesAsync();

var shijianzhuang = new Region
{
      Name = "shijianzhuang",
      ParentId = hebei.Id
};
await _generalRegionTreeManager.CreateAsync(shijianzhuang);
await CurrentUnitOfWork.SaveChangesAsync();

var changanqu = new Region
{
      Name = "changanqu",
      ParentId = shijianzhuang.Id
};
var qiaoxiqu = new Region
{
      Name = "qiaoxiqu",
      ParentId = shijianzhuang.Id
};
await _generalRegionTreeManager.CreateAsync(changanqu);
await _generalRegionTreeManager.CreateAsync(qiaoxiqu);
```

The results are as follows:

Id|Name|FullName|Code|Level|ParentId
:--:|:--:|:--:|:--:|:--:|:--:
1|beijing|beijing|00001|1|NULL
2|dongcheng|beijing-dongcheng|00001.00001|2|1
3|xicheng|beijing-xicheng|00001.00002|2|1
4|hebei|hebei|00002|1|NULL
5|shijianzhuang|hebei-shijianzhuang|00002.00001|2|4
6|changanqu|hebei-shijianzhuang-changanqu|00002.00001.00001|3|5
7|qiaoxiqu|hebei-shijianzhuang-qiaoxiqu|00002.00001.00002|3|5

The above operation has a batch method `BulkCreateAsync`

```c#
var beijing = new Region
{
      Name = "beijing",
      Children = new List<Region>
      {
            new Region
            {
                  Name = "dongcheng"
            },
            new Region
            {
                  Name = "dongcheng"
            }
      }
};
await _generalRegionTreeManager.BulkCreateAsync(beijing);
await CurrentUnitOfWork.SaveChangesAsync();

var hebei = new Region
{
      Name = "hebei",
      Children = new List<Region>
      {
            new Region
            {
                  Name = "shijiazhuang",
                  Children = new List<Region>
                  {
                        new Region
                        {
                              Name = "changanqu"
                        },
                        new Region
                        {
                              Name = "qiaodongqu"
                        }
                  }
            }
      }
};
await _generalRegionTreeManager.BulkCreateAsync(hebei);
await CurrentUnitOfWork.SaveChangesAsync();
```

## Some operations of the tree entity

```csharp
// Query all areas below Beijing does not include Beijing)
var beijing = await _regionRepository.FirstOrDefaultAsync(x => x.Name == "beijing");
var beijingChildren = _regionRepository.GetAll().Where(x => x.Id != beijing.Id && x.Code.StartsWith(beijing.Code));

// Query the area below Beijing (all districts)
var beijing = await _regionRepository.FirstOrDefaultAsync(x => x.Name == "beijing");
var beijingChildren = _regionRepository.GetAll().Where(x => x.Level == beijing.Level - 1 && x.Code.StartsWith(beijing.Code));

// Query Changan and all the parent above
var changanqu = await _regionRepository.FirstOrDefaultAsync(x => x.Name == "changanqu");
var parents = await _regionRepository.GetAllListAsync(x => changanqu.Code.StartsWith(x.Code));

// Query Changan top parent.
var changanqu = await _regionRepository.FirstOrDefaultAsync(x => x.Name == "changanqu");
var hebei =  await _regionRepository.FirstOrDefaultAsync(x => x.Level == 1 && changanqu.Code.Contains(x.Code));
```

## Other

```c#
public interface IGeneralTreeManager<TTree, TPrimaryKey>
      where TPrimaryKey : struct
      where TTree : class, IGeneralTree<TTree, TPrimaryKey>
{
      Task CreateAsync(TTree tree);

      Task BulkCreateAsync(TTree tree, Action<TTree> childrenAction = null);

      Task CreateChildrenAsync(TTree parent, ICollection<TTree> children, Action<TTree> childrenAction = null);

      Task FillUpAsync(TTree tree, Action<TTree> childrenAction = null);

      Task UpdateAsync(TTree tree, Action<TTree> childrenAction = null);

      Task MoveAsync(TPrimaryKey id, TPrimaryKey? parentId, Action<TTree> childrenAction = null);

      Task DeleteAsync(TPrimaryKey id);
}
```

## Custom

``` c#
public override void PreInitialize()
{
      // Custom error message
      Configuration.Modules.GeneralTree<Region, long>().ExceptionMessageFactory = tree => $"{tree.Name} already exists!.";

      // Custom node with the same name additional judgment logic
      Configuration.Modules.GeneralTree<Region, long>().CheckSameNameExpression = (regionThis, regionCheck) => regionThis.SomeForeignKey == regionCheck.SomeForeignKey

      // Custom FullName separator
      Configuration.Modules.GeneralTree<Region, long>().Hyphen = "=>";

}
```

The above code is for the entity's primary key as the value type. If it is a reference type, please use `IGeneralTreeWithReferenceType` and `IGeneralTreeManagerWithReferenceType`

Configure `GeneralTreeCodeGenerate`Code length (default is 5 digits)

``` c#
[Fact]
public void Test_CreateCode_With_Length()
{
      var generate = new GeneralTreeCodeGenerate(new GeneralTreeCodeGenerateConfiguration()
      {
            CodeLength = 3
      });

      generate.CreateCode().ShouldBe(null);
      generate.CreateCode(42).ShouldBe("042");
      generate.CreateCode(1, 2).ShouldBe("001.002");
      generate.CreateCode(1, 2, 3).ShouldBe("001.002.003");
}
```

`GeneralTreeExtensions` `ToTree` converts the Tree collection to TreeDto (has a hierarchical relationship, sortable)

``` c#
[Fact]
public void ToTreeOrderBy_Test()
{
      var regions = new List<Regin>
      {
            new Regin
            {
                  Id = 1,
                  Name = "beijing"
            },
            new Regin
            {
                  Id = 2,
                  Name = "bdongcheng",
                  ParentId = 1
            },
            new Regin
            {
                  Id = 3,
                  Name = "axicheng",
                  ParentId = 1
            },
            new Regin
            {
                  Id = 4,
                  Name = "aHebei"
            },
            new Regin
            {
                  Id = 5,
                  Name = "bShijianzhuang",
                  ParentId = 4
            },
            new Regin
            {
                  Id = 6,
                  Name = "aChengde",
                  ParentId = 4
            },
            new Regin
            {
                  Id = 7,
                  Name = "bShuangqiao",
                  ParentId = 6
            },
            new Regin
            {
                  Id = 8,
                  Name = "aShuangluan",
                  ParentId = 6
            }
      };

      var tree = regions.ToTreeOrderBy<Regin, long, string>(x => x.Name).ToList();

      tree.First().Name.ShouldBe("aHebei");
      tree.First().Children.First().Name.ShouldBe("aChengde");
      tree.First().Children.First().Children.First().Name.ShouldBe("aShuangluan");
}
```
