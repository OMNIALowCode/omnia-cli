using Omnia.CLI.Commands.Model.Behaviours.Readers;
using Shouldly;
using System.Linq;
using Xunit;

namespace UnitTests.Commands.Model.Behaviours
{
    public class DependencyReaderTest
    {
        private const string FileText =
@"using Omnia.Behaviours.T99.Dtos;
using Omnia.Behaviours.T99.Internal.System;

namespace Omnia.Behaviours.T99.Internal.System
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
        public void ExtractCodeDependencies_Successfully()
        {
            var reader = new DependencyReader();

            var data = reader.ExtractCodeDependencies(FileText);

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


        [Fact]
        public void ExtractCodeDependencies_NamespaceMatch()
        {
            var reader = new DependencyReader();

            var dependency = reader.ExtractCodeDependencies(FileText);

            dependency.Namespace.ShouldBe("Omnia.Behaviours.T99.Internal.System");
        }

        [Fact]
        public void ExtractFileDependencies_Successfully()
        {
            var reader = new DependencyReader();

            var references = reader.ExtractFileDependencies(@"
<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net472</TargetFramework>
    <AssemblyTitle>MySystem.External.MyCompany</AssemblyTitle>
    <AssemblyVersion>1.0.1</AssemblyVersion>
    <FileVersion>1.0.1</FileVersion>
  </PropertyGroup>

  <PropertyGroup>
    <OutputPath>$(ProjectDir)bin</OutputPath>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Google.Protobuf"" Version=""3.6.1"" />
    <PackageReference Include=""Microsoft.AspNet.WebApi.Client"" Version=""5.2.6"" />
    <PackageReference Include=""Newtonsoft.Json"" Version=""12.0.3"" />
    <PackageReference Include=""Omnia.Libraries.Infrastructure.Connector.Client"" Version=""3.0.237"" />
    <PackageReference Include=""Omnia.Libraries.Infrastructure.Behaviours"" Version=""3.0.118"" />
    <PackageReference Include=""Microsoft.Extensions.DependencyInjection"" Version=""3.1.3"" /> 
    <PackageReference Include=""Microsoft.Extensions.Http"" Version=""3.1.3"" />
	<PackageReference Include=""Microsoft.CSharp"" Version=""4.5.0"" />
  </ItemGroup>

  <ItemGroup>
    <Compile Include=""..\..\_common\**\*.*"" LinkBase=""_common"" />
  </ItemGroup>

	 <ItemGroup>
    <Reference Include=""MyCompany"">
      <HintPath>C:\Program Files (x86)\Common Files\MyCompany\MyCompany.dll</HintPath>
    </Reference>
  </ItemGroup>
	
</Project>");


            references.Count.ShouldBe(1);
            var reference = references.First();
            reference.AssemblyName.ShouldBe("MyCompany");
            reference.Path.ShouldBe(@"C:\Program Files (x86)\Common Files\MyCompany\MyCompany.dll");
        }
    }
}
