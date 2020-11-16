using Omnia.CLI.Commands.Model.Apply.Readers;
using Shouldly;
using System.Linq;
using Xunit;

namespace UnitTests.Commands.Model.Apply
{
    public class DaoReaderTest
    {

        private const string FileText =
@"/***************************************************************
****************************************************************
	THIS CODE HAS BEEN AUTOMATICALLY GENERATED
****************************************************************
****************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Omnia.Behaviours.IMP_L2.External.Primavera;
using Omnia.Behaviours.IMP_L2.Dtos;
using Omnia.Libraries.Infrastructure.Connector;
using Omnia.Libraries.Infrastructure.Connector.Client;
using Omnia.Libraries.Infrastructure.Behaviours;
using Omnia.Libraries.Infrastructure.Behaviours.Query;
using Action = Omnia.Libraries.Infrastructure.Behaviours.Action;

using MySystem;

namespace Omnia.Behaviours.T99.External.LocalSys.Daos
{
    public partial class CustomerDao
    {
        /// <summary>
		/// Create
        /// Create path.txt file
		/// </summary>
		public async Task<CustomerDto> CreateAsync(string identifier, CustomerDto dto, IDictionary<string,object> args, string concurrencyVersion){
			using (StreamWriter file = File.CreateText(@""D:\path.txt""))
            {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(file, dto);
            }

            return new CustomerDto();
		}

		public async Task<bool> DeleteAsync(string identifier, IDictionary<string,object> args, string concurrencyVersion){
			return false;
		}
		
		public async Task<CustomerDto> ReadAsync(string identifier, IDictionary<string,object> args){
			return new CustomerDto();
		}

		public async Task<(int totalRecords, IList<IDictionary<string,object>> data)> ReadListAsync(QueryContext queryContext, IDictionary<string,object> args, int? page = 1, int? pageSize = 25){
			  return (0, null);
		}

		public async Task<CustomerDto> UpdateAsync(string identifier, CustomerDto dto, IDictionary<string,object> args, string concurrencyVersion){
			return new CustomerDto();
		}
	}
}
";

        [Fact]
        public void ExtractData_Successfully()
        {
            var reader = new DaoReader();

            var entity = reader.ExtractData(FileText);

            entity.DataBehaviours.ShouldNotBeNull();
            entity.DataBehaviours.Count.ShouldBe(5);
        }

        [Fact]
        public void ExtractData_EmptyMethodsAreIgnored()
        {
            var reader = new DaoReader();

            var entity = reader.ExtractData(FileText);

            entity.DataBehaviours.ShouldNotContain(m => string.IsNullOrEmpty(m.Expression));
        }

        [Fact]
        public void ExtractData_HasCreate()
        {
            var reader = new DaoReader();

            var entity = reader.ExtractData(FileText);

            entity.DataBehaviours.ShouldContain(m => m.Name.Equals("Create"));
        }

        [Fact]
        public void ExtractData_ValidExpression()
        {
            var reader = new DaoReader();

            var create = reader.ExtractData(FileText)
                .DataBehaviours
                .First(m => m.Type == Omnia.CLI.Commands.Model.Apply.Data.DataBehaviourType.Create);

            create.Expression.ShouldBe(@"using (StreamWriter file = File.CreateText(@""D:\path.txt""))
            {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(file, dto);
            }

            return new CustomerDto();");
        }


        [Fact]
        public void ExtractData_ValidType()
        {
            var reader = new DaoReader();

            var read = reader.ExtractData(FileText)
                .DataBehaviours
                .First(m => m.Name.Equals("Read"));

            read.Type.ShouldBe(Omnia.CLI.Commands.Model.Apply.Data.DataBehaviourType.Read);
        }

        [Fact]
        public void ExtractData_UsesCommentDescription()
        {
            var reader = new DaoReader();

            var create = reader.ExtractData(FileText)
                .DataBehaviours
                .First(m => m.Type == Omnia.CLI.Commands.Model.Apply.Data.DataBehaviourType.Create);

            create.Description.ShouldBe("Create path.txt file");
        }

        [Fact]
        public void ExtractData_SuccessfullyExtractUsings()
        {
            var reader = new DaoReader();

            var entity = reader.ExtractData(FileText);

            entity.Usings.ShouldNotBeNull();
            entity.Usings.Count.ShouldBe(1);
            entity.Usings.Single().ShouldBe("MySystem");
        }

        [Fact]
        public void ExtractData_NamespaceMatch()
        {
            var reader = new DaoReader();

            var entity = reader.ExtractData(FileText);

            entity.Namespace.ShouldBe("Omnia.Behaviours.T99.External.LocalSys.Daos");
        }
    }
}
