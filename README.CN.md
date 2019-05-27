
 <img src="https://raw.githubusercontent.com/maliming/Abp.GeneralTree/master/GeneralTree.png" width="200" height="200" /> 

# Abp GeneralTree

[![Build status](https://ci.appveyor.com/api/projects/status/5fvpyg756aushkv7?svg=true)](https://ci.appveyor.com/project/maliming/Abp-Generaltree)
[![NuGet](https://img.shields.io/nuget/vpre/abp.GeneralTree.svg)](https://www.nuget.org/packages/Abp.GeneralTree)

- 基于Abp模块系统,完美集成Abp框架.
- 支持自定义主键(值类型，引用类型).
- 自动分配Code,Level,FullName扩展了实体的其他属性.
- 基于Code,Level对实体进行有效管理.
- **适用于管理各种树结构实体，例如：区域，组织，类别，行业和具有父子实体的其他实体.**

## 安装

``` c#
Install-Package Abp.GeneralTree
dotnet add package Abp.GeneralTree
```

在模块中加入`GeneralTreeModule`依赖
``` c#
[DependsOn(typeof(GeneralTreeModule))]
public class YourProjectModule : AbpModule
{
    //...
}
```

GeneralTree提供一个泛型`IGeneralTree`接口,实体继承此接口,传入泛型参数实体和主键(主键可以是值类型和引用类型)

值类型

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

引用类型

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

以Region地区实体为例:

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

实体实现泛型接口下的属性,GeneralTree会自动维护这些属性(FullName,Code,Level,ParentId...)

创建,更新,移动,删除等操作请使用`IGeneralTreeManager<TTree, TPrimaryKey>`, 接口的泛型参数同上.

## 使用

我们首先初始化一些地区信息.[自动获取中华人民共和国商务部的最新中华人民共和国行政区划代码数据](https://github.com/maliming/RegionTree)

``` c#
var beijing = new Region
{
      Name = "北京"
};
await _generalRegionTreeManager.CreateAsync(beijing);
```

此时beijing的实体信息如下:

Id|Name|FullName|Code|Level|ParentId
:--:|:--:|:--:|:--:|:--:|:--:
1|北京|北京|00001|1|NULL

GeneralTree自动维护了修改属性.对后面的高效管理提供基础.

再补充一些地区

``` c#
var beijing = new Region
{
      Name = "北京"
};
await _generalRegionTreeManager.CreateAsync(beijing);
await CurrentUnitOfWork.SaveChangesAsync();

var dongcheng = new Region
{
      Name = "东城区",
      ParentId = beijing.Id
};

var xicheng = new Region
{
      Name = "西城区",
      ParentId = beijing.Id
};
await _generalRegionTreeManager.CreateAsync(dongcheng);
await _generalRegionTreeManager.CreateAsync(xicheng);

var hebei = new Region
{
      Name = "河北"
};
await _generalRegionTreeManager.CreateAsync(hebei);
await CurrentUnitOfWork.SaveChangesAsync();

var shijianzhuang = new Region
{
      Name = "石家庄",
      ParentId = hebei.Id
};
await _generalRegionTreeManager.CreateAsync(shijianzhuang);
await CurrentUnitOfWork.SaveChangesAsync();

var changanqu = new Region
{
      Name = "长安区",
      ParentId = shijianzhuang.Id
};
var qiaoxiqu = new Region
{
      Name = "桥东区",
      ParentId = shijianzhuang.Id
};
await _generalRegionTreeManager.CreateAsync(changanqu);
await _generalRegionTreeManager.CreateAsync(qiaoxiqu);
```

结果如下:

Id|Name|FullName|Code|Level|ParentId
:--:|:--:|:--:|:--:|:--:|:--:
1|北京|北京|00001|1|NULL
2|东城区|北京-东城区|00001.00001|2|1
3|西城区|北京-西城区|00001.00002|2|1
4|河北|河北|00002|1|NULL
5|石家庄|河北-石家庄|00002.00001|2|4
6|长安区|河北-石家庄-长安区|00002.00001.00001|3|5
7|桥东区|河北-石家庄-桥东区|00002.00001.00002|3|5

上面的操作有批量的方法`BulkCreateAsync`

```c#
var beijing = new Region
{
      Name = "北京",
      Children = new List<Region>
      {
            new Region
            {
                  Name = "东城区"
            },
            new Region
            {
                  Name = "东城区"
            }
      }
};
await _generalRegionTreeManager.BulkCreateAsync(beijing);
await CurrentUnitOfWork.SaveChangesAsync();

var hebei = new Region
{
      Name = "河北",
      Children = new List<Region>
      {
            new Region
            {
                  Name = "石家庄",
                  Children = new List<Region>
                  {
                        new Region
                        {
                              Name = "长安区"
                        },
                        new Region
                        {
                              Name = "桥东区"
                        }
                  }
            }
      }
};
await _generalRegionTreeManager.BulkCreateAsync(hebei);
await CurrentUnitOfWork.SaveChangesAsync();
```

## 树形实体的一些操作

```csharp
// 查询北京下面的所有地区(不包括北京)
var beijing = await _regionRepository.FirstOrDefaultAsync(x => x.Name == "北京");
var beijingChildren = _regionRepository.GetAll().Where(x => x.Id != beijing.Id && x.Code.StartsWith(beijing.Code));

// 查询北京下面的地区(所有的区)
var beijing = await _regionRepository.FirstOrDefaultAsync(x => x.Name == "北京");
var beijingChildren = _regionRepository.GetAll().Where(x => x.Level == beijing.Level - 1 && x.Code.StartsWith(beijing.Code));

// 查询长安区的最顶级父地区
var changanqu = await _regionRepository.FirstOrDefaultAsync(x => x.Name == "长安区");
var parents = await _regionRepository.GetAllListAsync(x => changanqu.Code.StartsWith(x.Code));

// 查询长安区和上面所有的父地区
var changanqu = await _regionRepository.FirstOrDefaultAsync(x => x.Name == "长安区");
var hebei =  await _regionRepository.FirstOrDefaultAsync(x => x.Level == 1 && changanqu.Code.Contains(x.Code));
```

## 其它操作

```c#
public interface IGeneralTreeManager<TTree, TPrimaryKey>
      where TPrimaryKey : struct
      where TTree : class, IGeneralTree<TTree, TPrimaryKey>
{
      // 创建
      Task CreateAsync(TTree tree);

      // 批量创建
      Task BulkCreateAsync(TTree tree, Action<TTree> childrenAction = null);

      // 给parnet增加子节点
      Task CreateChildrenAsync(TTree parent, ICollection<TTree> children, Action<TTree> childrenAction = null);

      // 填充Code,Level,FullName...等属性
      Task FillUpAsync(TTree tree, Action<TTree> childrenAction = null);

      // 更新属性(如Name, 当前和所有的子节点FullName都会统一更新)
      Task UpdateAsync(TTree tree, Action<TTree> childrenAction = null);

      // 移动节点的父节点(相关的子节点都会更新Code,Level,FullName...等属性)
      Task MoveAsync(TPrimaryKey id, TPrimaryKey? parentId, Action<TTree> childrenAction = null);

      // 删除节点
      Task DeleteAsync(TPrimaryKey id);
}
```

## 自定义配置

``` c#
public override void PreInitialize()
{
      // 自定义错误信息
      Configuration.Modules.GeneralTree<Region, long>().ExceptionMessageFactory = tree => $"{tree.Name} already exists!.";

      // 自定义同级节点同名额外检查逻辑
      Configuration.Modules.GeneralTree<Region, long>().CheckSameNameExpression = (regionThis, regionCheck) => regionThis.SomeForeignKey == regionCheck.SomeForeignKey

      // 自定义FullName分隔符
      Configuration.Modules.GeneralTree<Region, long>().Hyphen = "=>";

}
```

以上代码都是针对实体的主键为值类型.如果是引用类型请使用`IGeneralTreeWithReferenceType`和`IGeneralTreeManagerWithReferenceType`

## 其它

配置`GeneralTreeCodeGenerate`Code长度(默认是5位)

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

`GeneralTreeExtensions` `ToTree`转换Tree集合到TreeDto(拥有层级关系,可排序)

``` c#
[Fact]
public void ToTreeOrderBy_Test()
{
      var regions = new List<Regin>
      {
            new Regin
            {
                  Id = 1,
                  Name = "b北京"
            },
            new Regin
            {
                  Id = 2,
                  Name = "b东城区",
                  ParentId = 1
            },
            new Regin
            {
                  Id = 3,
                  Name = "a西城区",
                  ParentId = 1
            },
            new Regin
            {
                  Id = 4,
                  Name = "a河北"
            },
            new Regin
            {
                  Id = 5,
                  Name = "b石家庄",
                  ParentId = 4
            },
            new Regin
            {
                  Id = 6,
                  Name = "a承德",
                  ParentId = 4
            },
            new Regin
            {
                  Id = 7,
                  Name = "b双桥区",
                  ParentId = 6
            },
            new Regin
            {
                  Id = 8,
                  Name = "a双滦区",
                  ParentId = 6
            }
      };

      var tree = regions.ToTreeOrderBy<Regin, long, string>(x => x.Name).ToList();

      tree.First().Name.ShouldBe("a河北");
      tree.First().Children.First().Name.ShouldBe("a承德");
      tree.First().Children.First().Children.First().Name.ShouldBe("a双滦区");
}
```
