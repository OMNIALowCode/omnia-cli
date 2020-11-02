using Omnia.CLI.Commands.Model.Behaviours;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace UnitTests.Commands.Model.Behaviours
{
    public class BehaviourReaderTest
    {

        private const string FileText =
@"
/***************************************************************
****************************************************************
	THIS CODE HAS BEEN AUTOMATICALLY GENERATED
	10/08/2020 22:18:45
****************************************************************
****************************************************************/

using Omnia.Behaviours.T99.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Omnia.Libraries.Infrastructure.Connector;
using Omnia.Libraries.Infrastructure.Connector.Client;
using System.Net.Http.Formatting;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Omnia.Libraries.Infrastructure.Behaviours;
using Action = Omnia.Libraries.Infrastructure.Behaviours.Action;
using MyCompany.CustomDll;


namespace Omnia.Behaviours.T99.Internal.System.Model
{
    public partial class Customer
    {
		/// <summary>
		/// Initialize
        /// Set the name to:
		/// Hello World!
		/// </summary>
		private void OnInitialize(){
			this._name = ""Hello World!"";
		}

		private void OnBeforeUpdate()
		{
			this._name = ""Hello World 2!"";
		}

		private void OnAfterUpdate()
		{
			this._name = ""Hello World 3!"";
            this._name = ""Hello World 4!"";
		}

        private String Getname(){ 
			return ""New Name"";
		}

		private void On_codePropertyChange(String oldValue, String newValue)
		{
            _name = newValue;
		}	
		private void On_descriptionPropertyChange(String oldValue, String newValue)
		{
		}	
		private void On_inactivePropertyChange(Boolean oldValue, Boolean newValue)
		{
		}	
		private void On_namePropertyChange(String oldValue, String newValue)
		{
		}	
		private void OnAddressPropertyChange(String oldValue, String newValue)
		{
		}	
		private void OnAddress2PropertyChange(String oldValue, String newValue)
		{
		}	
		private void OnAttrDecimalPropertyChange(Decimal oldValue, Decimal newValue)
		{
		}	
		private void OnChildPropertyChange(List<Child> oldValue, List<Child> newValue)
		{
		}	
		private void OnListAddrsPropertyChange(List<String> oldValue, List<String> newValue)
		{
		}	
		private void OnmyinactivePropertyChange(Boolean oldValue, Boolean newValue)
		{
		}	


		public  void OnBeforeSave()
		{
			_name = ""tst"";
		}

		public  async Task<AfterSaveMessage> OnAfterSave()
		{
			return await Task.FromResult(AfterSaveMessage.Empty);
		}

        private void OnBeforecollectionEntityInitialize(Child entry)
		{
			entry._name = ""Child initialized"";
		}
}
}";


        [Fact]
        public void ExtractData_Successfully()
        {
            var reader = new BehaviourReader();

            var entity = reader.ExtractData(FileText);

            entity.Behaviours.ShouldNotBeNull();
            entity.Behaviours.Count.ShouldBe(8);
        }

        [Fact]
        public void ExtractData_EmptyMethodsAreIgnored()
        {
            var reader = new BehaviourReader();

            var entity = reader.ExtractData(FileText);

            entity.Behaviours.ShouldNotContain(m => string.IsNullOrEmpty(m.Expression));
        }

        [Fact]
        public void ExtractData_HasInitialize()
        {
            var reader = new BehaviourReader();

            var entity = reader.ExtractData(FileText);

            entity.Behaviours.ShouldContain(m => m.Name.Equals("Initialize"));
        }

        [Fact]
        public void ExtractData_ValidExpression()
        {
            var reader = new BehaviourReader();

            var initialize = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Type == Omnia.CLI.Commands.Model.Behaviours.Data.EntityBehaviourType.Initialize);

            initialize.Expression.ShouldBe("this._name = \"Hello World!\";");
        }

        [Fact]
        public void ExtractData_UsesCommentName()
        {
            var reader = new BehaviourReader();

            var initialize = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Type == Omnia.CLI.Commands.Model.Behaviours.Data.EntityBehaviourType.Initialize);

            initialize.Name.ShouldBe("Initialize");
        }

        [Fact]
        public void ExtractData_UsesCommentDescription()
        {
            var reader = new BehaviourReader();

            var initialize = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Type == Omnia.CLI.Commands.Model.Behaviours.Data.EntityBehaviourType.Initialize);

            initialize.Description.ShouldBe($"Set the name to:{Environment.NewLine}Hello World!");
        }

        [Fact]
        public void ExtractData_ValidType()
        {
            var reader = new BehaviourReader();

            var initialize = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Name.Equals("Initialize"));

            initialize.Type.ShouldBe(Omnia.CLI.Commands.Model.Behaviours.Data.EntityBehaviourType.Initialize);
        }

        [Fact]
        public void ExtractData_WithPropertyChange_ValidType()
        {
            var reader = new BehaviourReader();

            var initialize = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Name.Equals("On_codePropertyChange"));

            initialize.Type.ShouldBe(Omnia.CLI.Commands.Model.Behaviours.Data.EntityBehaviourType.Action);
        }

        [Fact]
        public void ExtractData_WithPropertyChange_CorrectAttribute()
        {
            var reader = new BehaviourReader();

            var initialize = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Name.Equals("On_codePropertyChange"));

            initialize.Attribute.ShouldBe("_code");
        }

        [Fact]
        public void ExtractData_WithFormula_ValidType()
        {
            var reader = new BehaviourReader();

            var formula = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Name.Equals("Getname"));

            formula.Type.ShouldBe(Omnia.CLI.Commands.Model.Behaviours.Data.EntityBehaviourType.Formula);
        }

        [Fact]
        public void ExtractData_WithFormula_ValidExpression()
        {
            var reader = new BehaviourReader();

            var formula = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Name.Equals("Getname"));

            formula.Expression.ShouldBe("return \"New Name\";");
        }

        [Fact]
        public void ExtractData_WithFormula_CorrectAttribute()
        {
            var reader = new BehaviourReader();

            var formula = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Name.Equals("Getname"));

            formula.Attribute.ShouldBe("name");
        }

        [Fact]
        public void ExtractData_WithBeforeCollectionEntityInitialize_ValidType()
        {
            var reader = new BehaviourReader();

            var intialize = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Name.Equals("OnBeforecollectionEntityInitialize"));

            intialize.Type.ShouldBe(Omnia.CLI.Commands.Model.Behaviours.Data.EntityBehaviourType.BeforeCollectionEntityInitialize);
        }

        [Fact]
        public void ExtractData_WithBeforeCollectionEntityInitialize_ValidExpression()
        {
            var reader = new BehaviourReader();

            var intialize = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Name.Equals("OnBeforecollectionEntityInitialize"));

            intialize.Expression.ShouldBe("entry._name = \"Child initialized\";");
        }

        [Fact]
        public void ExtractData_WithBeforeCollectionEntityInitialize_CorrectAttribute()
        {
            var reader = new BehaviourReader();

            var intialize = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Name.Equals("OnBeforecollectionEntityInitialize"));

            intialize.Attribute.ShouldBe("collection");
        }

        [Fact]
        public void ExtractData_WithAfterChange_ValidType()
        {
            var reader = new BehaviourReader();

            var afterChange = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Name.Equals("OnAfterUpdate"));

            afterChange.Type.ShouldBe(Omnia.CLI.Commands.Model.Behaviours.Data.EntityBehaviourType.AfterChange);
        }

        [Fact]
        public void ExtractData_WithAfterChange_ValidExpression()
        {
            var reader = new BehaviourReader();

            var afterChange = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Name.Equals("OnAfterUpdate"));
            afterChange.Expression.ShouldBe(@"this._name = ""Hello World 3!"";
            this._name = ""Hello World 4!"";");
        }

        [Fact]
        public void ExtractData_WithBeforeChange_ValidType()
        {
            var reader = new BehaviourReader();

            var beforeChange = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Name.Equals("OnBeforeUpdate"));

            beforeChange.Type.ShouldBe(Omnia.CLI.Commands.Model.Behaviours.Data.EntityBehaviourType.BeforeChange);
        }

        [Fact]
        public void ExtractData_WithBeforeChange_ValidExpression()
        {
            var reader = new BehaviourReader();

            var beforeChange = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Name.Equals("OnBeforeUpdate"));
            beforeChange.Expression.ShouldBe("this._name = \"Hello World 2!\";");
        }

        [Fact]
        public void ExtractData_WithBeforeSave_ValidType()
        {
            var reader = new BehaviourReader();

            var beforeSave = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Name.Equals("OnBeforeSave"));

            beforeSave.Type.ShouldBe(Omnia.CLI.Commands.Model.Behaviours.Data.EntityBehaviourType.BeforeSave);
        }

        [Fact]
        public void ExtractData_WithBeforeSave_ValidExpression()
        {
            var reader = new BehaviourReader();

            var beforeSave = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Name.Equals("OnBeforeSave"));
            beforeSave.Expression.ShouldBe("_name = \"tst\";");
        }

        [Fact]
        public void ExtractData_WithAfterSave_ValidType()
        {
            var reader = new BehaviourReader();

            var afterSave = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Name.Equals("OnAfterSave"));

            afterSave.Type.ShouldBe(Omnia.CLI.Commands.Model.Behaviours.Data.EntityBehaviourType.AfterSave);
        }

        [Fact]
        public void ExtractData_WithAfterSave_ValidExpression()
        {
            var reader = new BehaviourReader();

            var afterSave = reader.ExtractData(FileText)
                .Behaviours
                .First(m => m.Name.Equals("OnAfterSave"));
            afterSave.Expression.ShouldBe("return await Task.FromResult(AfterSaveMessage.Empty);");
        }

        [Fact]
        public void ExtractData_SuccessfullyExtractUsings()
        {
            var reader = new BehaviourReader();

            var entity = reader.ExtractData(FileText);

            entity.Usings.ShouldNotBeNull();
            entity.Usings.Count.ShouldBe(1);
            entity.Usings.Single().ShouldBe("MyCompany.CustomDll");
        }

        [Fact]
        public void ExtractData_NamespaceMatch()
        {
            var reader = new BehaviourReader();

            var entity = reader.ExtractData(FileText);

            entity.Namespace.ShouldBe("Omnia.Behaviours.T99.Internal.System.Model");
        }
    }
}
