using System;
using System.Collections.Generic;
using System.Text;
using Abp.GeneralTree;
using Shouldly;
using Xunit;

namespace TreeTests
{
    public class GeneralTreeCodeGenerate_Tests : TreeTestBase
    {
        [Fact]
        public void Test_CreateCode()
        {
            GeneralTreeCodeGenerate.CreateCode().ShouldBe(null);
            GeneralTreeCodeGenerate.CreateCode(42).ShouldBe("00042");
            GeneralTreeCodeGenerate.CreateCode(1, 2).ShouldBe("00001.00002");
            GeneralTreeCodeGenerate.CreateCode(1, 2, 3).ShouldBe("00001.00002.00003");
        }

        [Fact]
        public void Test_GetLastCode()
        {
            GeneralTreeCodeGenerate.GetLastCode("00001").ShouldBe("00001");
            GeneralTreeCodeGenerate.GetLastCode("00001.00002.00003").ShouldBe("00003");
        }

        [Fact]
        public void Test_GetNextCode()
        {
            GeneralTreeCodeGenerate.GetNextCode("00001").ShouldBe("00002");
            GeneralTreeCodeGenerate.GetNextCode("00001.00001").ShouldBe("00001.00002");
        }

        [Fact]
        public void Test_GetParentCode()
        {
            GeneralTreeCodeGenerate.GetParentCode("00001").ShouldBe(null);
            GeneralTreeCodeGenerate.GetParentCode("00001.00002.00003").ShouldBe("00001.00002");
        }

        [Fact]
        public void Test_MergeCode()
        {
            GeneralTreeCodeGenerate.MergeCode(null, "00002").ShouldBe("00002");
            GeneralTreeCodeGenerate.MergeCode("00001", "00002").ShouldBe("00001.00002");
        }

        [Fact]
        public void Test_MergeFullName()
        {
            GeneralTreeCodeGenerate.MergeFullName(null, "beijing").ShouldBe("beijing");
            GeneralTreeCodeGenerate.MergeFullName("beijing", "xicheng").ShouldBe("beijing-xicheng");
        }

        [Fact]
        public void Test_RemoveParentCode()
        {
            GeneralTreeCodeGenerate.RemoveParentCode("00001.00002.00003", "00001").ShouldBe("00002.00003");
            GeneralTreeCodeGenerate.RemoveParentCode("00001.00002.00003", null).ShouldBe("00001.00002.00003");
        }
    }
}
