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

    public class GeneralTreeExtensions_Tests : TreeTestBase
    {
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
    }
}