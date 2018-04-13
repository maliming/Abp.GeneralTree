using Abp.GeneralTree;
using Abp.GeneralTree.GeneralTree;
using Shouldly;
using Xunit;

namespace TreeTests
{
    public class GeneralTreeCodeGenerate_Tests : TreeTestBase
    {
        public GeneralTreeCodeGenerate_Tests()
        {
            _generalTreeCodeGenerate = LocalIocManager.Resolve<IGeneralTreeCodeGenerate>();
        }

        private readonly IGeneralTreeCodeGenerate _generalTreeCodeGenerate;

        [Fact]
        public void Test_CreateCode()
        {
            _generalTreeCodeGenerate.CreateCode().ShouldBe(null);
            _generalTreeCodeGenerate.CreateCode(42).ShouldBe("00042");
            _generalTreeCodeGenerate.CreateCode(1, 2).ShouldBe("00001.00002");
            _generalTreeCodeGenerate.CreateCode(1, 2, 3).ShouldBe("00001.00002.00003");
        }

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

        [Fact]
        public void Test_GetLastCode()
        {
            _generalTreeCodeGenerate.GetLastCode("00001").ShouldBe("00001");
            _generalTreeCodeGenerate.GetLastCode("00001.00002.00003").ShouldBe("00003");
        }

        [Fact]
        public void Test_GetNextCode()
        {
            _generalTreeCodeGenerate.GetNextCode("00001").ShouldBe("00002");
            _generalTreeCodeGenerate.GetNextCode("00001.00001").ShouldBe("00001.00002");
        }

        [Fact]
        public void Test_GetParentCode()
        {
            _generalTreeCodeGenerate.GetParentCode("00001").ShouldBe(null);
            _generalTreeCodeGenerate.GetParentCode("00001.00002.00003").ShouldBe("00001.00002");
        }

        [Fact]
        public void Test_MergeCode()
        {
            _generalTreeCodeGenerate.MergeCode(null, "00002").ShouldBe("00002");
            _generalTreeCodeGenerate.MergeCode("00001", "00002").ShouldBe("00001.00002");
        }

        [Fact]
        public void Test_MergeFullName()
        {
            _generalTreeCodeGenerate.MergeFullName(null, "beijing").ShouldBe("beijing");
            _generalTreeCodeGenerate.MergeFullName("beijing", "xicheng").ShouldBe("beijing-xicheng");
        }

        [Fact]
        public void Test_RemoveParentCode()
        {
            _generalTreeCodeGenerate.RemoveParentCode("00001.00002.00003", "00001").ShouldBe("00002.00003");
            _generalTreeCodeGenerate.RemoveParentCode("00001.00002.00003", null).ShouldBe("00001.00002.00003");
        }
    }
}