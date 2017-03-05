# Abp Module-GeneralTree

[![Build status](https://ci.appveyor.com/api/projects/status/9v1wff4rm6jx7yte?svg=true)](https://ci.appveyor.com/project/maliming/module-generaltree)

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

> 代码大部分来自abp module-zero
:+1:
