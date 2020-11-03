using Omnia.CLI.Commands.Model.Behaviours;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace UnitTests.Commands.Model.Behaviours
{
    public class CodeDependencyReaderTest
    {
        private const string FileText =
@"using Omnia.Behaviours.GF046.Dtos;
using Omnia.Behaviours.GF046.Internal.System;

namespace Omnia.Behaviours.GF046.Internal.System
{
    using MyCompany;
    namespace CodeDependencies
    {
        public class MyDto
        {
            public string Name { get; set; }
        }
    }
}
";
        [Fact]
        public void ExtractData_Successfully()
        {
            var reader = new CodeDependencyReader();

            var data = reader.ExtractData(FileText);

            data.Expression.ShouldNotBeNull();
            data.Expression.ShouldBe(@"using MyCompany;
    namespace CodeDependencies
    {
        public class MyDto
        {
            public string Name { get; set; }
        }
    }");
        }
    }
}
