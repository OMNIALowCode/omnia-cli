using Omnia.CLI.Commands.Model.Apply.Readers.UI;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace UnitTests.Commands.Model.Apply.UI
{
    public class BehaviourReaderTest
    {

        private const string FileText =
@"
/***************************************************************
****************************************************************
	THIS CODE HAS BEEN AUTOMATICALLY GENERATED
****************************************************************
****************************************************************/

class employeeERPConfiguration {
	
	constructor(metadata, context) {
		this._metadata = metadata;
		this._context = context;
		
		this._inactive = null;
		this.company = '';
		this.costCenter = null;
		this.erpCode = null;
		this.primavera = '';
		this.vehicle = null;
	}

	
	initialize(){
		
	}
	
	
	onChange__inactive(oldValue, newValue){
		
	}

	
	onChange_company(oldValue, newValue){
		
	}

	
	onChange_costCenter(oldValue, newValue){
		
	}

	
	onChange_erpCode(oldValue, newValue){
		
	}

	
	onChange_primavera(oldValue, newValue){
		
	}

	
	onChange_vehicle(oldValue, newValue){
		
	}


	
	beforeChange(){
	
		
	}

	
	afterChange(){
	
		
	}

	
	beforeSave(){
	
		
	}


	
}

class EmployeeForm {
	
	constructor(metadata, context) {
		this._metadata = metadata;
		this._context = context;
		
		this._code = null;
		this._inactive = null;
		this._name = null;
		this.contactEmail = null;
		this.defaultCompany = '';
		this.organizationalUnit = null;
		this.outOfOffice = null;
		this.outOfOfficeSubstitute = null;
		this.username = null;
		this.employeeERPConfiguration = [];
	}

    /**
     * Initializer
     * Behaviour executed on form entry to 
     * Change visible attributes 
     * hide options 
	 */	
	initialize(){
		if(!(this._context.authentication.userIsInRole('Approver') || 
        this._context.authentication.userIsInRole('Administration') ||
        this._context.authentication.userIsInRole('HRApprover')) && 
        this._context.authentication.userIsInRole('Contributor')){
    // Hide and disable all elements
    for(const el of Object.values(this._metadata.elements)){
        el.isHidden = true;
        el.attributes.isReadOnly = true;
    }
    
    // Show Code and Name
    this._metadata.elements._code.isHidden = false;
    this._metadata.elements._name.isHidden = false;
    
    // Show and enable OutOfOffice and Substitute
    this._metadata.elements.outOfOffice.isHidden = false;
    this._metadata.elements.outOfOffice.attributes.isReadOnly = false;
    this._metadata.elements.outOfOfficeSubstitute.isHidden = false;
    this._metadata.elements.outOfOfficeSubstitute.attributes.isReadOnly = false;
}

for(const configuration of this.employeeERPConfiguration){
    configuration._metadata.attributes.removeEntry = 'hidden';
}
}


onChange__code(oldValue, newValue)
{

}


onChange__inactive(oldValue, newValue)
{

}


onChange__name(oldValue, newValue)
{

}


onChange_contactEmail(oldValue, newValue)
{

}


onChange_defaultCompany(oldValue, newValue)
{

}


onChange_organizationalUnit(oldValue, newValue)
{

}

/**
 * OnChange_OutOfOffice
 */
onChange_outOfOffice(oldValue, newValue)
{
    if (this.outOfOffice === true)
    {
        this._metadata.elements.outOfOfficeSubstitute.attributes.min = 1;
    }
    else
    {
        this._metadata.elements.outOfOfficeSubstitute.attributes.min = 0;
    }


}


onChange_outOfOfficeSubstitute(oldValue, newValue)
{

}


onChange_username(oldValue, newValue)
{

}



beforeChange()
{
    for (let i = 0; i < this.employeeERPConfiguration.length; i++)
    {
        this.employeeERPConfiguration[i].beforeChange && this.employeeERPConfiguration[i].beforeChange();
    }


}


afterChange()
{
    for (let i = 0; i < this.employeeERPConfiguration.length; i++)
    {
        this.employeeERPConfiguration[i].afterChange && this.employeeERPConfiguration[i].afterChange();
    }


}


beforeSave()
{
    for (let i = 0; i < this.employeeERPConfiguration.length; i++)
    {
        this.employeeERPConfiguration[i].beforeSave && this.employeeERPConfiguration[i].beforeSave();
    }


}


addTo_employeeERPConfiguration(metadata)
{
    const value = new employeeERPConfiguration(metadata, this._context);
    this.employeeERPConfiguration.push(value);
    return value;
}

removeFrom_employeeERPConfiguration(index)
{
    this.employeeERPConfiguration.splice(index, 1);
}
	
}


EmployeeForm;
";


        [Fact]
        public void ExtractData_Successfully()
        {
            var reader = new UIEntityBehaviourReader();

            reader.ExtractData(FileText);

            //entity.EntityBehaviours.ShouldNotBeNull();
            //entity.EntityBehaviours.Count.ShouldBe(8);
        }

        //[Fact]
        //public void ExtractData_EmptyMethodsAreIgnored()
        //{
        //    var reader = new EntityBehaviourReader();

        //    var entity = reader.ExtractData(FileText);

        //    entity.EntityBehaviours.ShouldNotContain(m => string.IsNullOrEmpty(m.Expression));
        //}

        //[Fact]
        //public void ExtractData_HasInitialize()
        //{
        //    var reader = new EntityBehaviourReader();

        //    var entity = reader.ExtractData(FileText);

        //    entity.EntityBehaviours.ShouldContain(m => m.Name.Equals("Initialize"));
        //}

        //[Fact]
        //public void ExtractData_ValidExpression()
        //{
        //    var reader = new EntityBehaviourReader();

        //    var initialize = reader.ExtractData(FileText)
        //        .EntityBehaviours
        //        .First(m => m.Type == Omnia.CLI.Commands.Model.Apply.Data.EntityBehaviourType.Initialize);

        //    initialize.Expression.ShouldBe("this._name = \"Hello World!\";");
        //}

        //[Fact]
        //public void ExtractData_UsesCommentName()
        //{
        //    var reader = new EntityBehaviourReader();

        //    var initialize = reader.ExtractData(FileText)
        //        .EntityBehaviours
        //        .First(m => m.Type == Omnia.CLI.Commands.Model.Apply.Data.EntityBehaviourType.Initialize);

        //    initialize.Name.ShouldBe("Initialize");
        //}

        //[Fact]
        //public void ExtractData_UsesCommentDescription()
        //{
        //    var reader = new EntityBehaviourReader();

        //    var initialize = reader.ExtractData(FileText)
        //        .EntityBehaviours
        //        .First(m => m.Type == Omnia.CLI.Commands.Model.Apply.Data.EntityBehaviourType.Initialize);

        //    initialize.Description.ShouldBe($"Set the name to:{Environment.NewLine}Hello World!");
        //}

        //[Fact]
        //public void ExtractData_ValidType()
        //{
        //    var reader = new EntityBehaviourReader();

        //    var initialize = reader.ExtractData(FileText)
        //        .EntityBehaviours
        //        .First(m => m.Name.Equals("Initialize"));

        //    initialize.Type.ShouldBe(Omnia.CLI.Commands.Model.Apply.Data.EntityBehaviourType.Initialize);
        //}

        //[Fact]
        //public void ExtractData_WithPropertyChange_ValidType()
        //{
        //    var reader = new EntityBehaviourReader();

        //    var initialize = reader.ExtractData(FileText)
        //        .EntityBehaviours
        //        .First(m => m.Name.Equals("On_codePropertyChange"));

        //    initialize.Type.ShouldBe(Omnia.CLI.Commands.Model.Apply.Data.EntityBehaviourType.Action);
        //}

        //[Fact]
        //public void ExtractData_WithPropertyChange_CorrectAttribute()
        //{
        //    var reader = new EntityBehaviourReader();

        //    var initialize = reader.ExtractData(FileText)
        //        .EntityBehaviours
        //        .First(m => m.Name.Equals("On_codePropertyChange"));

        //    initialize.Attribute.ShouldBe("_code");
        //}

        //[Fact]
        //public void ExtractData_WithFormula_ValidType()
        //{
        //    var reader = new EntityBehaviourReader();

        //    var formula = reader.ExtractData(FileText)
        //        .EntityBehaviours
        //        .First(m => m.Name.Equals("Getname"));

        //    formula.Type.ShouldBe(Omnia.CLI.Commands.Model.Apply.Data.EntityBehaviourType.Formula);
        //}

        //[Fact]
        //public void ExtractData_WithFormula_ValidExpression()
        //{
        //    var reader = new EntityBehaviourReader();

        //    var formula = reader.ExtractData(FileText)
        //        .EntityBehaviours
        //        .First(m => m.Name.Equals("Getname"));

        //    formula.Expression.ShouldBe("return \"New Name\";");
        //}

        //[Fact]
        //public void ExtractData_WithFormula_CorrectAttribute()
        //{
        //    var reader = new EntityBehaviourReader();

        //    var formula = reader.ExtractData(FileText)
        //        .EntityBehaviours
        //        .First(m => m.Name.Equals("Getname"));

        //    formula.Attribute.ShouldBe("name");
        //}

        //[Fact]
        //public void ExtractData_WithBeforeCollectionEntityInitialize_ValidType()
        //{
        //    var reader = new EntityBehaviourReader();

        //    var initialize = reader.ExtractData(FileText)
        //        .EntityBehaviours
        //        .First(m => m.Name.Equals("OnBeforecollectionEntityInitialize"));

        //    initialize.Type.ShouldBe(Omnia.CLI.Commands.Model.Apply.Data.EntityBehaviourType.BeforeCollectionEntityInitialize);
        //}

        //[Fact]
        //public void ExtractData_WithAfterChange_ValidType()
        //{
        //    var reader = new EntityBehaviourReader();

        //    var afterChange = reader.ExtractData(FileText)
        //        .EntityBehaviours
        //        .First(m => m.Name.Equals("OnAfterUpdate"));

        //    afterChange.Type.ShouldBe(Omnia.CLI.Commands.Model.Apply.Data.EntityBehaviourType.AfterChange);
        //}

        //[Fact]
        //public void ExtractData_WithAfterChange_ValidExpression()
        //{
        //    var reader = new EntityBehaviourReader();

        //    var afterChange = reader.ExtractData(FileText)
        //        .EntityBehaviours
        //        .First(m => m.Name.Equals("OnAfterUpdate"));
        //    afterChange.Expression.ShouldBe(@"this._name = ""Hello World 3!"";
        //    this._name = ""Hello World 4!"";");
        //}

        //[Fact]
        //public void ExtractData_WithBeforeChange_ValidType()
        //{
        //    var reader = new EntityBehaviourReader();

        //    var beforeChange = reader.ExtractData(FileText)
        //        .EntityBehaviours
        //        .First(m => m.Name.Equals("OnBeforeUpdate"));

        //    beforeChange.Type.ShouldBe(Omnia.CLI.Commands.Model.Apply.Data.EntityBehaviourType.BeforeChange);
        //}

        //[Fact]
        //public void ExtractData_WithBeforeChange_ValidExpression()
        //{
        //    var reader = new EntityBehaviourReader();

        //    var beforeChange = reader.ExtractData(FileText)
        //        .EntityBehaviours
        //        .First(m => m.Name.Equals("OnBeforeUpdate"));
        //    beforeChange.Expression.ShouldBe("this._name = \"Hello World 2!\";");
        //}

        //[Fact]
        //public void ExtractData_WithBeforeSave_ValidType()
        //{
        //    var reader = new EntityBehaviourReader();

        //    var beforeSave = reader.ExtractData(FileText)
        //        .EntityBehaviours
        //        .First(m => m.Name.Equals("OnBeforeSave"));

        //    beforeSave.Type.ShouldBe(Omnia.CLI.Commands.Model.Apply.Data.EntityBehaviourType.BeforeSave);
        //}

        //[Fact]
        //public void ExtractData_WithBeforeSave_ValidExpression()
        //{
        //    var reader = new EntityBehaviourReader();

        //    var beforeSave = reader.ExtractData(FileText)
        //        .EntityBehaviours
        //        .First(m => m.Name.Equals("OnBeforeSave"));
        //    beforeSave.Expression.ShouldBe("_name = \"tst\";");
        //}
    }
}
