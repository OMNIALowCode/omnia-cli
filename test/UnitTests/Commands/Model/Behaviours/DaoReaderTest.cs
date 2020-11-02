using Omnia.CLI.Commands.Model.Behaviours;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace UnitTests.Commands.Model.Behaviours
{
    public class DaoReaderTest
    {

        private const string FileText =
@"

/***************************************************************
****************************************************************
	THIS CODE HAS BEEN AUTOMATICALLY GENERATED
****************************************************************
****************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    public class CustomerDao
    {
		public CustomerDao(Context context)
		{
			this._Context = context;
		}
		
		[JsonIgnore]
		public readonly Context _Context;

		public CustomerDto Create(string identifier, CustomerDto dto, IDictionary<string,object> args, string concurrencyVersion){
			using (StreamWriter file = File.CreateText(@""D:\path.txt""))
            {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(file, dto);
            }

            return new CustomerDto();
		}

		public bool Delete(string identifier, IDictionary<string,object> args, string concurrencyVersion){
			return false;
		}
		
		public CustomerDto Read(string identifier, IDictionary<string,object> args){
			return new CustomerDto();
		}

		public (int totalRecords, IList<IDictionary<string,object>> data) ReadList(QueryContext queryContext, IDictionary<string,object> args, int? page = 1, int? pageSize = 25){
			  return (0, null);
		}

		public CustomerDto Update(string identifier, CustomerDto dto, IDictionary<string,object> args, string concurrencyVersion){
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

            entity.Behaviours.ShouldNotBeNull();
            entity.Behaviours.Count.ShouldBe(5);
        }

        [Fact]
        public void ExtractData_EmptyMethodsAreIgnored()
        {
            var reader = new DaoReader();

            var entity = reader.ExtractData(FileText);

            entity.Behaviours.ShouldNotContain(m => string.IsNullOrEmpty(m.Expression));
        }

        [Fact]
        public void ExtractData_HasInitialize()
        {
            var reader = new DaoReader();

            var entity = reader.ExtractData(FileText);

            entity.Behaviours.ShouldContain(m => m.Name.Equals("Create"));
        }

        [Fact]
        public void ExtractData_ValidExpression()
        {
            var reader = new DaoReader();

            var initialize = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Type == Omnia.CLI.Commands.Model.Behaviours.Data.DataBehaviourType.Create);

            initialize.Expression.ShouldBe(@"using (StreamWriter file = File.CreateText(@""D:\path.txt""))
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

            var initialize = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Name.Equals("Read"));

            initialize.Type.ShouldBe(Omnia.CLI.Commands.Model.Behaviours.Data.DataBehaviourType.Read);
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
