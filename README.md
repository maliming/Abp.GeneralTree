# Abp Module-GeneralTree

[![Build status](https://ci.appveyor.com/api/projects/status/9v1wff4rm6jx7yte?svg=true)](https://ci.appveyor.com/project/maliming/module-generaltree)
[![NuGet](https://img.shields.io/nuget/vpre/abp.GeneralTree.svg)](https://www.nuget.org/packages/Abp.GeneralTree)

根据 [abp module-zero](https://github.com/aspnetboilerplate/module-zero) 中 [Organizations](http://www.aspnetboilerplate.com/Pages/Documents/Zero/Organization-Units) 功能

实现了对树形Entity通用的管理.

```csharp
public interface IGeneralTree<TTree>
{
      string Name { get; set; }

      string FullName { get; set; }

      string Code { get; set; }

      int Level { get; set; }

      TTree Parent { get; set; }

      long? ParentId { get; set; }

      ICollection<TTree> Children { get; set; }
}
```
### 主要特性

- 基于Abp
- 自动处理Code,Level,FullName的赋值
- 根据Code特性快速查询

适合管理各种树结构如:地区,组织,类别,行业等拥有父子层次的各种Entity.

数据:

Id|Name|FullName|Code|Level|ParentId
:--:|:--:|:--:|:--:|:--:|:--:
1|北京|北京|00001|1|NULL
2|东城区|北京-东城区|00001.00001|2|1
3|西城区|北京-西城区|00001.00002|2|1
4|朝阳区|北京-朝阳区|00001.00003|2|1
5|丰台区|北京-丰台区|00001.00004|2|1
6|石景山区|北京-石景山区|00001.00005|2|1
7|海淀区|北京-海淀区|00001.00006|2|1
8|门头沟区|北京-门头沟区|00001.00007|2|1
9|房山区|北京-房山区|00001.00008|2|1
10|通州区|北京-通州区|00001.00009|2|1
11|顺义区|北京-顺义区|00001.00010|2|1
12|昌平区|北京-昌平区|00001.00011|2|1
13|大兴区|北京-大兴区|00001.00012|2|1
14|平谷区|北京-平谷区|00001.00013|2|1
15|怀柔区|北京-怀柔区|00001.00014|2|1
16|密云区|北京-密云区|00001.00015|2|1
17|延庆区|北京-延庆区|00001.00016|2|1
18|天津|天津|00002|1|NULL
19|和平区|天津-和平区|00002.00001|2|18
20|河东区|天津-河东区|00002.00002|2|18
21|河西区|天津-河西区|00002.00003|2|18
22|南开区|天津-南开区|00002.00004|2|18
23|河北区|天津-河北区|00002.00005|2|18
24|红桥区|天津-红桥区|00002.00006|2|18
25|滨海新区|天津-滨海新区|00002.00007|2|18
26|东丽区|天津-东丽区|00002.00008|2|18
27|西青区|天津-西青区|00002.00009|2|18
28|津南区|天津-津南区|00002.00010|2|18
29|北辰区|天津-北辰区|00002.00011|2|18
30|宁河区|天津-宁河区|00002.00012|2|18
31|武清区|天津-武清区|00002.00013|2|18
32|静海区|天津-静海区|00002.00014|2|18
33|宝坻区|天津-宝坻区|00002.00015|2|18
34|蓟县区|天津-蓟县区|00002.00016|2|18
35|河北|河北|00003|1|NULL
36|石家庄|河北-石家庄|00003.00001|2|35
37|长安区|河北-石家庄-长安区|00003.00001.00001|3|36
38|桥东区|河北-石家庄-桥东区|00003.00001.00002|3|36
39|桥西区|河北-石家庄-桥西区|00003.00001.00003|3|36
40|新华区|河北-石家庄-新华区|00003.00001.00004|3|36

## 使用 ##
```csharp
public interface IGeneralTreeManager<in TTree> 
	where TTree : class, IGeneralTree<TTree>, IEntity<long>
{
	Task CreateAsync(TTree tree);

	Task UpdateAsync(TTree tree);

	Task MoveAsync(long id, long? parentId);

	Task DeleteAsync(long id);
}
```
创建,更新,删除,移动(如改变父节点) 自动维护Code,Level,FullName.

Code特性对于树状结构处理层级关系非常方便.
```csharp
//查询北京所有子数据
var beijing = await _regionRepository.FirstOrDefaultAsync(x => x.Name == "北京");
var beijingChildren = _regionRepository.GetAll()
	.Where(x => x.Id != beijing.Id && x.Code.StartsWith(beijing.Code));

//根据子查询顶级
var changanqu = await _regionRepository.FirstOrDefaultAsync(x => x.Name == "长安区");
var hebei =
	await _regionRepository.FirstOrDefaultAsync(x => x.Level == 1 && changanqu.Code.Contains(x.Code));
```
详细Code特性介绍请访问:[ou-code](https://aspnetboilerplate.com/Pages/Documents/Zero/Organization-Units#ou-code)
