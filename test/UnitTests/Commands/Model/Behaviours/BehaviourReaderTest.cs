
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Moq;
using Omnia.CLI;
using Omnia.CLI.Commands.Model.Behaviours;
using Omnia.CLI.Commands.Model.Import;
using Omnia.CLI.Infrastructure;
using Shouldly;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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


namespace Omnia.Behaviours.T99.Internal.System.Model
{
    public partial class Customer
    {
		private void ExecuteInitialize(){
			this._name = ""Hello World!"";
		}

		private void ExecuteBeforeUpdate(){
		}

		private void ExecuteAfterUpdate(){
		}

		private void On_codePropertyChange(String oldValue, String newValue)
		{
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


		public  void ExecuteBeforeSave(){
				Child.ForEach(a => a.ExecuteBeforeSave());
	
		}

		public  async Task<AfterSaveMessage> ExecuteAfterSave(){
				Child.ForEach(async a => await a.ExecuteAfterSave());
			return await Task.FromResult(AfterSaveMessage.Empty);
	
		}

        		private void BeforeChildEntityInitialize(Child entry)
		{
		}	
	}
}";


        [Fact]
        public void ExtractMethods_Successfully()
        {
            var reader = new BehaviourReader();

            var behaviours = reader.ExtractMethods(FileText);

            behaviours.ShouldNotBeNull();
            behaviours.Count.ShouldBe(16);
        }

        [Fact]
        public void ExtractMethods_HasInitialize()
        {
            var reader = new BehaviourReader();

            var behaviours = reader.ExtractMethods(FileText);

            behaviours.ShouldContain(m => m.Name.Equals("ExecuteInitialize"));
        }

        [Fact]
        public void ExtractMethods_ValidExpression()
        {
            var reader = new BehaviourReader();

            var initialize = reader.ExtractMethods(FileText)
                .First(m => m.Name.Equals("ExecuteInitialize"));

            initialize.Expression.ShouldBe("\t\t\tthis._name = \"Hello World!\";\r\n");
        }

        [Fact]
        public void ExtractMethods_ValidType()
        {
            var reader = new BehaviourReader();

            var initialize = reader.ExtractMethods(FileText)
                .First(m => m.Name.Equals("ExecuteInitialize"));

            initialize.Type.ShouldBe(Omnia.CLI.Commands.Model.Behaviours.Data.BehaviourType.Initialize);
        }

        [Fact]
        public void ExtractMethods_WithPropertyChange_ValidType()
        {
            var reader = new BehaviourReader();

            var initialize = reader.ExtractMethods(FileText)
                .First(m => m.Name.Equals("On_codePropertyChange"));

            initialize.Type.ShouldBe(Omnia.CLI.Commands.Model.Behaviours.Data.BehaviourType.Action);
        }

        public void ExtractMethods_WithPropertyChange_CorrectAttribute()
        {
            var reader = new BehaviourReader();

            var initialize = reader.ExtractMethods(FileText)
                .First(m => m.Name.Equals("On_codePropertyChange"));

            initialize.Attribute.ShouldBe("_code");
        }

    }
}
