using System.Collections.Generic;
using System.Linq;
using Abp.Domain.Entities;
using Abp.GeneralTree;
using Shouldly;
using Xunit;

namespace TreeTests
{
    internal class Regin : Entity<long>, IGeneralTree<Regin, long>
    {
        public string Name { get; set; }

        public string FullName { get; set; }

        public string Code { get; set; }

        public int Level { get; set; }

        public Regin Parent { get; set; }

        public long? ParentId { get; set; }

        public ICollection<Regin> Children { get; set; }
    }

    internal class Regin2 : Entity<string>, IGeneralTreeWithReferenceType<Regin2, string>
    {
        public string Name { get; set; }

        public string FullName { get; set; }

        public string Code { get; set; }

        public int Level { get; set; }

        public Regin2 Parent { get; set; }

        public string ParentId { get; set; }

        public ICollection<Regin2> Children { get; set; }
    }

    internal class ReginDto : IGeneralTreeDto<ReginDto, long>
    {
        public string Name { get; set; }
        public long Id { get; set; }

        public long? ParentId { get; set; }

        public ICollection<ReginDto> Children { get; set; }
    }

    internal class Regin2Dto : IGeneralTreeDtoWithReferenceType<Regin2Dto, string>
    {
        public string Name { get; set; }
        public string Id { get; set; }

        public string ParentId { get; set; }

        public ICollection<Regin2Dto> Children { get; set; }
    }

    public class GeneralTreeExtensions_Tests : TreeTestBase
    {
        [Fact]
        public void ToTree_Children_Test()
        {
            var regions = new List<Regin>
            {
                new Regin
                {
                    Id = 1,
                    Name = "北京",
                    ParentId = 8888
                },
                new Regin
                {
                    Id = 2,
                    Name = "东城区",
                    ParentId = 1
                },
                new Regin
                {
                    Id = 3,
                    Name = "西城区",
                    ParentId = 1
                },
                new Regin
                {
                    Id = 4,
                    Name = "河北",
                    ParentId = 9999
                },
                new Regin
                {
                    Id = 5,
                    Name = "石家庄",
                    ParentId = 4
                },
                new Regin
                {
                    Id = 6,
                    Name = "承德",
                    ParentId = 4
                },
                new Regin
                {
                    Id = 7,
                    Name = "双桥区",
                    ParentId = 6
                }
            };

            var tree = regions.ToTree<Regin, long>().ToList();

            tree.ShouldNotBeNull();
            tree.Count.ShouldBe(2);
            tree.First().Children.Count.ShouldBe(2);
            tree.Last().Children.Count.ShouldBe(2);
            tree.Last().Children.Last().Children.Count.ShouldBe(1);
        }

        [Fact]
        public void ToTree_Test()
        {
            var regions = new List<Regin>
            {
                new Regin
                {
                    Id = 1,
                    Name = "北京"
                },
                new Regin
                {
                    Id = 2,
                    Name = "东城区",
                    ParentId = 1
                },
                new Regin
                {
                    Id = 3,
                    Name = "西城区",
                    ParentId = 1
                },
                new Regin
                {
                    Id = 4,
                    Name = "河北"
                },
                new Regin
                {
                    Id = 5,
                    Name = "石家庄",
                    ParentId = 4
                },
                new Regin
                {
                    Id = 6,
                    Name = "承德",
                    ParentId = 4
                },
                new Regin
                {
                    Id = 7,
                    Name = "双桥区",
                    ParentId = 6
                }
            };

            var tree = regions.ToTree<Regin, long>().ToList();

            tree.ShouldNotBeNull();
            tree.Count.ShouldBe(2);
            tree.First().Children.Count.ShouldBe(2);
            tree.Last().Children.Count.ShouldBe(2);
            tree.Last().Children.Last().Children.Count.ShouldBe(1);
        }

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

        [Fact]
        public void ToTreeOrderByDescending_Test()
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

            var tree = regions.ToTreeOrderByDescending<Regin, long, string>(x => x.Name).ToList();

            tree.First().Name.ShouldBe("b北京");
            tree.First().Children.First().Name.ShouldBe("b东城区");
        }

        [Fact]
        public void ToTree_WithReferenceType_Children_Test()
        {
            var regions = new List<Regin2>
            {
                new Regin2
                {
                    Id = "1",
                    Name = "北京",
                    ParentId = "8888"
                },
                new Regin2
                {
                    Id = "2",
                    Name = "东城区",
                    ParentId = "1"
                },
                new Regin2
                {
                    Id = "3",
                    Name = "西城区",
                    ParentId = "1"
                },
                new Regin2
                {
                    Id = "4",
                    Name = "河北",
                    ParentId = "9999"
                },
                new Regin2
                {
                    Id = "5",
                    Name = "石家庄",
                    ParentId = "4"
                },
                new Regin2
                {
                    Id = "6",
                    Name = "承德",
                    ParentId = "4"
                },
                new Regin2
                {
                    Id = "7",
                    Name = "双桥区",
                    ParentId = "6"
                }
            };

            var tree = regions.ToTreeWithReferenceType<Regin2, string>().ToList();

            tree.ShouldNotBeNull();
            tree.Count.ShouldBe(2);
            tree.First().Children.Count.ShouldBe(2);
            tree.Last().Children.Count.ShouldBe(2);
            tree.Last().Children.Last().Children.Count.ShouldBe(1);
        }

        [Fact]
        public void ToTree_WithReferenceType_Test()
        {
            var regions = new List<Regin2>
            {
                new Regin2
                {
                    Id = "1",
                    Name = "北京"
                },
                new Regin2
                {
                    Id = "2",
                    Name = "东城区",
                    ParentId = "1"
                },
                new Regin2
                {
                    Id = "3",
                    Name = "西城区",
                    ParentId = "1"
                },
                new Regin2
                {
                    Id = "4",
                    Name = "河北"
                },
                new Regin2
                {
                    Id = "5",
                    Name = "石家庄",
                    ParentId = "4"
                },
                new Regin2
                {
                    Id = "6",
                    Name = "承德",
                    ParentId = "4"
                },
                new Regin2
                {
                    Id = "7",
                    Name = "双桥区",
                    ParentId = "6"
                }
            };

            var tree = regions.ToTreeWithReferenceType<Regin2, string>().ToList();

            tree.ShouldNotBeNull();
            tree.Count.ShouldBe(2);
            tree.First().Children.Count.ShouldBe(2);
            tree.Last().Children.Count.ShouldBe(2);
            tree.Last().Children.Last().Children.Count.ShouldBe(1);
        }

        [Fact]
        public void ToTree_WithReferenceType_OrderBy_Test()
        {
            var regions = new List<Regin2>
            {
                new Regin2
                {
                    Id = "1",
                    Name = "b北京"
                },
                new Regin2
                {
                    Id = "2",
                    Name = "b东城区",
                    ParentId = "1"
                },
                new Regin2
                {
                    Id = "3",
                    Name = "a西城区",
                    ParentId = "1"
                },
                new Regin2
                {
                    Id = "4",
                    Name = "a河北"
                },
                new Regin2
                {
                    Id = "5",
                    Name = "b石家庄",
                    ParentId = "4"
                },
                new Regin2
                {
                    Id = "6",
                    Name = "a承德",
                    ParentId = "4"
                },
                new Regin2
                {
                    Id = "7",
                    Name = "b双桥区",
                    ParentId = "6"
                },
                new Regin2
                {
                    Id = "8",
                    Name = "a双滦区",
                    ParentId = "6"
                }
            };

            var tree = regions.ToTreeWithReferenceTypeOrderBy<Regin2, string, string>(x => x.Name).ToList();

            tree.First().Name.ShouldBe("a河北");
            tree.First().Children.First().Name.ShouldBe("a承德");
            tree.First().Children.First().Children.First().Name.ShouldBe("a双滦区");
        }

        [Fact]
        public void ToTree_WithReferenceType_OrderByDescending_Test()
        {
            var regions = new List<Regin2>
            {
                new Regin2
                {
                    Id = "1",
                    Name = "b北京"
                },
                new Regin2
                {
                    Id = "2",
                    Name = "b东城区",
                    ParentId = "1"
                },
                new Regin2
                {
                    Id = "3",
                    Name = "a西城区",
                    ParentId = "1"
                },
                new Regin2
                {
                    Id = "4",
                    Name = "a河北"
                },
                new Regin2
                {
                    Id = "5",
                    Name = "b石家庄",
                    ParentId = "4"
                },
                new Regin2
                {
                    Id = "6",
                    Name = "a承德",
                    ParentId = "4"
                },
                new Regin2
                {
                    Id = "7",
                    Name = "b双桥区",
                    ParentId = "6"
                },
                new Regin2
                {
                    Id = "8",
                    Name = "a双滦区",
                    ParentId = "6"
                }
            };

            var tree = regions.ToTreeWithReferenceTypeOrderByDescending<Regin2, string, string>(x => x.Name).ToList();

            tree.First().Name.ShouldBe("b北京");
            tree.First().Children.First().Name.ShouldBe("b东城区");
        }


        [Fact]
        public void ToTreeDto_Children_Test()
        {
            var regions = new List<ReginDto>
            {
                new ReginDto
                {
                    Id = 1,
                    Name = "北京",
                    ParentId = 8888
                },
                new ReginDto
                {
                    Id = 2,
                    Name = "东城区",
                    ParentId = 1
                },
                new ReginDto
                {
                    Id = 3,
                    Name = "西城区",
                    ParentId = 1
                },
                new ReginDto
                {
                    Id = 4,
                    Name = "河北",
                    ParentId = 9999
                },
                new ReginDto
                {
                    Id = 5,
                    Name = "石家庄",
                    ParentId = 4
                },
                new ReginDto
                {
                    Id = 6,
                    Name = "承德",
                    ParentId = 4
                },
                new ReginDto
                {
                    Id = 7,
                    Name = "双桥区",
                    ParentId = 6
                }
            };

            var tree = regions.ToTreeDto<ReginDto, long>().ToList();

            tree.ShouldNotBeNull();
            tree.Count.ShouldBe(2);
            tree.First().Children.Count.ShouldBe(2);
            tree.Last().Children.Count.ShouldBe(2);
            tree.Last().Children.Last().Children.Count.ShouldBe(1);
        }

        [Fact]
        public void ToTreeDto_Test()
        {
            var regions = new List<ReginDto>
            {
                new ReginDto
                {
                    Id = 1,
                    Name = "北京"
                },
                new ReginDto
                {
                    Id = 2,
                    Name = "东城区",
                    ParentId = 1
                },
                new ReginDto
                {
                    Id = 3,
                    Name = "西城区",
                    ParentId = 1
                },
                new ReginDto
                {
                    Id = 4,
                    Name = "河北"
                },
                new ReginDto
                {
                    Id = 5,
                    Name = "石家庄",
                    ParentId = 4
                },
                new ReginDto
                {
                    Id = 6,
                    Name = "承德",
                    ParentId = 4
                },
                new ReginDto
                {
                    Id = 7,
                    Name = "双桥区",
                    ParentId = 6
                }
            };

            var tree = regions.ToTreeDto<ReginDto, long>().ToList();

            tree.ShouldNotBeNull();
            tree.Count.ShouldBe(2);
            tree.First().Children.Count.ShouldBe(2);
            tree.Last().Children.Count.ShouldBe(2);
            tree.Last().Children.Last().Children.Count.ShouldBe(1);
        }

        [Fact]
        public void ToTreeDtoOrderBy_Test()
        {
            var regions = new List<ReginDto>
            {
                new ReginDto
                {
                    Id = 1,
                    Name = "b北京"
                },
                new ReginDto
                {
                    Id = 2,
                    Name = "b东城区",
                    ParentId = 1
                },
                new ReginDto
                {
                    Id = 3,
                    Name = "a西城区",
                    ParentId = 1
                },
                new ReginDto
                {
                    Id = 4,
                    Name = "a河北"
                },
                new ReginDto
                {
                    Id = 5,
                    Name = "b石家庄",
                    ParentId = 4
                },
                new ReginDto
                {
                    Id = 6,
                    Name = "a承德",
                    ParentId = 4
                },
                new ReginDto
                {
                    Id = 7,
                    Name = "b双桥区",
                    ParentId = 6
                },
                new ReginDto
                {
                    Id = 8,
                    Name = "a双滦区",
                    ParentId = 6
                }
            };

            var tree = regions.ToTreeDtoOrderBy<ReginDto, long, string>(x => x.Name).ToList();

            tree.First().Name.ShouldBe("a河北");
            tree.First().Children.First().Name.ShouldBe("a承德");
            tree.First().Children.First().Children.First().Name.ShouldBe("a双滦区");
        }

        [Fact]
        public void ToTreeDtoOrderByDescending_Test()
        {
            var regions = new List<ReginDto>
            {
                new ReginDto
                {
                    Id = 1,
                    Name = "b北京"
                },
                new ReginDto
                {
                    Id = 2,
                    Name = "b东城区",
                    ParentId = 1
                },
                new ReginDto
                {
                    Id = 3,
                    Name = "a西城区",
                    ParentId = 1
                },
                new ReginDto
                {
                    Id = 4,
                    Name = "a河北"
                },
                new ReginDto
                {
                    Id = 5,
                    Name = "b石家庄",
                    ParentId = 4
                },
                new ReginDto
                {
                    Id = 6,
                    Name = "a承德",
                    ParentId = 4
                },
                new ReginDto
                {
                    Id = 7,
                    Name = "b双桥区",
                    ParentId = 6
                },
                new ReginDto
                {
                    Id = 8,
                    Name = "a双滦区",
                    ParentId = 6
                }
            };

            var tree = regions.ToTreeDtoOrderByDescending<ReginDto, long, string>(x => x.Name).ToList();

            tree.First().Name.ShouldBe("b北京");
            tree.First().Children.First().Name.ShouldBe("b东城区");
        }

        [Fact]
        public void ToTreeDto_WithReferenceType_Children_Test()
        {
            var regions = new List<Regin2Dto>
            {
                new Regin2Dto
                {
                    Id = "1",
                    Name = "北京",
                    ParentId = "8888"
                },
                new Regin2Dto
                {
                    Id = "2",
                    Name = "东城区",
                    ParentId = "1"
                },
                new Regin2Dto
                {
                    Id = "3",
                    Name = "西城区",
                    ParentId = "1"
                },
                new Regin2Dto
                {
                    Id = "4",
                    Name = "河北",
                    ParentId = "9999"
                },
                new Regin2Dto
                {
                    Id = "5",
                    Name = "石家庄",
                    ParentId = "4"
                },
                new Regin2Dto
                {
                    Id = "6",
                    Name = "承德",
                    ParentId = "4"
                },
                new Regin2Dto
                {
                    Id = "7",
                    Name = "双桥区",
                    ParentId = "6"
                }
            };

            var tree = regions.ToTreeDtoWithReferenceType<Regin2Dto, string>().ToList();

            tree.ShouldNotBeNull();
            tree.Count.ShouldBe(2);
            tree.First().Children.Count.ShouldBe(2);
            tree.Last().Children.Count.ShouldBe(2);
            tree.Last().Children.Last().Children.Count.ShouldBe(1);
        }

        [Fact]
        public void ToTreeDto_WithReferenceType_Test()
        {
            var regions = new List<Regin2Dto>
            {
                new Regin2Dto
                {
                    Id = "1",
                    Name = "北京"
                },
                new Regin2Dto
                {
                    Id = "2",
                    Name = "东城区",
                    ParentId = "1"
                },
                new Regin2Dto
                {
                    Id = "3",
                    Name = "西城区",
                    ParentId = "1"
                },
                new Regin2Dto
                {
                    Id = "4",
                    Name = "河北"
                },
                new Regin2Dto
                {
                    Id = "5",
                    Name = "石家庄",
                    ParentId = "4"
                },
                new Regin2Dto
                {
                    Id = "6",
                    Name = "承德",
                    ParentId = "4"
                },
                new Regin2Dto
                {
                    Id = "7",
                    Name = "双桥区",
                    ParentId = "6"
                }
            };

            var tree = regions.ToTreeDtoWithReferenceType<Regin2Dto, string>().ToList();

            tree.ShouldNotBeNull();
            tree.Count.ShouldBe(2);
            tree.First().Children.Count.ShouldBe(2);
            tree.Last().Children.Count.ShouldBe(2);
            tree.Last().Children.Last().Children.Count.ShouldBe(1);
        }

        [Fact]
        public void ToTreeWithReferenceTypeDtoOrderBy_Test()
        {
            var regions = new List<Regin2Dto>
            {
                new Regin2Dto
                {
                    Id = "1",
                    Name = "b北京"
                },
                new Regin2Dto
                {
                    Id = "2",
                    Name = "b东城区",
                    ParentId = "1"
                },
                new Regin2Dto
                {
                    Id = "3",
                    Name = "a西城区",
                    ParentId = "1"
                },
                new Regin2Dto
                {
                    Id = "4",
                    Name = "a河北"
                },
                new Regin2Dto
                {
                    Id = "5",
                    Name = "b石家庄",
                    ParentId = "4"
                },
                new Regin2Dto
                {
                    Id = "6",
                    Name = "a承德",
                    ParentId = "4"
                },
                new Regin2Dto
                {
                    Id = "7",
                    Name = "b双桥区",
                    ParentId = "6"
                },
                new Regin2Dto
                {
                    Id = "8",
                    Name = "a双滦区",
                    ParentId = "6"
                }
            };

            var tree = regions.ToTreeDtoWithReferenceTypeOrderBy<Regin2Dto, string, string>(x => x.Name).ToList();

            tree.First().Name.ShouldBe("a河北");
            tree.First().Children.First().Name.ShouldBe("a承德");
            tree.First().Children.First().Children.First().Name.ShouldBe("a双滦区");
        }

        [Fact]
        public void ToTreeWithReferenceTypeDtoOrderByDescending_Test()
        {
            var regions = new List<Regin2Dto>
            {
                new Regin2Dto
                {
                    Id = "1",
                    Name = "b北京"
                },
                new Regin2Dto
                {
                    Id = "2",
                    Name = "b东城区",
                    ParentId = "1"
                },
                new Regin2Dto
                {
                    Id = "3",
                    Name = "a西城区",
                    ParentId = "1"
                },
                new Regin2Dto
                {
                    Id = "4",
                    Name = "a河北"
                },
                new Regin2Dto
                {
                    Id = "5",
                    Name = "b石家庄",
                    ParentId = "4"
                },
                new Regin2Dto
                {
                    Id = "6",
                    Name = "a承德",
                    ParentId = "4"
                },
                new Regin2Dto
                {
                    Id = "7",
                    Name = "b双桥区",
                    ParentId = "6"
                },
                new Regin2Dto
                {
                    Id = "8",
                    Name = "a双滦区",
                    ParentId = "6"
                }
            };

            var tree = regions.ToTreeDtoWithReferenceTypeOrderByDescending<Regin2Dto, string, string>(x => x.Name)
                .ToList();

            tree.First().Name.ShouldBe("b北京");
            tree.First().Children.First().Name.ShouldBe("b东城区");
        }
    }
}