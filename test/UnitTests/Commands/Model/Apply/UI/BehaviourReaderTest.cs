using Omnia.CLI.Commands.Model.Apply.Readers.UI;
using Shouldly;
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

class warehouseBuildings {
	
	constructor(metadata, context) {
		this._metadata = metadata;
		this._context = context;
		
		this._code = null;
		this._description = null;
		this._inactive = null;
		this._name = null;
	}

    /**
     * Initializer
     * Initialize description 
	 */		
	initialize(){
        this._description = 'Hello World!';
    }
	
	
	onChange__code(oldValue, newValue){
		this._code = 'Hello world 2!';

    }


    onChange__description(oldValue, newValue)
    {
        this._code = 'Hello world 3!';
    }


    onChange__inactive(oldValue, newValue)
    {

    }


    onChange__name(oldValue, newValue)
    {

    }



    beforeChange()
    {


    }


    afterChange()
    {


    }


    beforeSave()
    {


    }



}

class WarehouseForm
{

    constructor(metadata, context)
    {
        this._metadata = metadata;
        this._context = context;

        this._code = null;
        this._description = null;
        this._inactive = null;
        this._name = null;
        this.warehouseBuildings = [];
    }

    initialize()
    {
        this._description = 'Hello world 4!';
    }


    onChange__code(oldValue, newValue)
    {
        this._description = 'Hello world 5!';
    }


    onChange__description(oldValue, newValue)
    {

    }


    onChange__inactive(oldValue, newValue)
    {

    }


    onChange__name(oldValue, newValue)
    {

    }



    beforeChange()
    {
        for (let i = 0; i < this.warehouseBuildings.length; i++)
        {
            this.warehouseBuildings[i].beforeChange && this.warehouseBuildings[i].beforeChange();
        }


    }


    afterChange()
    {
        for (let i = 0; i < this.warehouseBuildings.length; i++)
        {
            this.warehouseBuildings[i].afterChange && this.warehouseBuildings[i].afterChange();
        }


    }


    beforeSave()
    {
        for (let i = 0; i < this.warehouseBuildings.length; i++)
        {
            this.warehouseBuildings[i].beforeSave && this.warehouseBuildings[i].beforeSave();
        }


    }


    addTo_warehouseBuildings(metadata)
    {
        const value = new warehouseBuildings(metadata, this._context);
        this.warehouseBuildings.push(value);
        return value;
    }

    removeFrom_warehouseBuildings(index)
    {
        this.warehouseBuildings.splice(index, 1);
    }

}


WarehouseForm;
";


        [Fact]
        public void ExtractData_Successfully()
        {
            var reader = new UIEntityBehaviourReader();

            var entity = reader.ExtractData(FileText);

            entity.EntityBehaviours.ShouldNotBeNull();
            entity.EntityBehaviours.Count.ShouldBe(8);
        }

        [Fact]
        public void ExtractData_EmptyMethodsAreIgnored()
        {
            var reader = new UIEntityBehaviourReader();

            var entity = reader.ExtractData(FileText);

            entity.EntityBehaviours.ShouldNotContain(m => string.IsNullOrEmpty(m.Expression));
        }

        [Fact]
        public void ExtractData_HasInitialize()
        {
            var reader = new UIEntityBehaviourReader();

            var entity = reader.ExtractData(FileText);

            entity.EntityBehaviours.ShouldContain(m => m.Name.Equals("initialize"));
        }

        [Fact]
        public void ExtractData_ValidExpression()
        {
            var reader = new UIEntityBehaviourReader();

            var initialize = reader.ExtractData(FileText)
                .EntityBehaviours
                .First(m => m.Type == Omnia.CLI.Commands.Model.Apply.Data.UI.UIBehaviourType.Initialize);

            initialize.Expression.ShouldBe("this._description = 'Hello World!';");
        }

        [Fact]
        public void ExtractData_UsesCommentName()
        {
            var reader = new UIEntityBehaviourReader();

            var initialize = reader.ExtractData(FileText)
                .EntityBehaviours
                .First(m => m.Type == Omnia.CLI.Commands.Model.Apply.Data.UI.UIBehaviourType.Initialize);

            initialize.Name.ShouldBe("Initializer");
        }

        [Fact]
        public void ExtractData_UsesCommentDescription()
        {
            var reader = new UIEntityBehaviourReader();

            var initialize = reader.ExtractData(FileText)
                .EntityBehaviours
                .First(m => m.Type == Omnia.CLI.Commands.Model.Apply.Data.UI.UIBehaviourType.Initialize);

            initialize.Description.ShouldBe($"Initialize description");
        }

        [Fact]
        public void ExtractData_ValidType()
        {
            var reader = new UIEntityBehaviourReader();

            var initialize = reader.ExtractData(FileText)
                .EntityBehaviours
                .First(m => m.Name.Equals("initialize"));

            initialize.Type.ShouldBe(Omnia.CLI.Commands.Model.Apply.Data.UI.UIBehaviourType.Initialize);
        }

        [Fact]
        public void ExtractData_WithPropertyChange_ValidType()
        {
            var reader = new UIEntityBehaviourReader();

            var initialize = reader.ExtractData(FileText)
                .EntityBehaviours
                .First(m => m.Name.Equals("WarehouseForm_onChange__code"));

            initialize.Type.ShouldBe(Omnia.CLI.Commands.Model.Apply.Data.UI.UIBehaviourType.Change);
        }

        [Fact]
        public void ExtractData_WithPropertyChange_CorrectAttribute()
        {
            var reader = new UIEntityBehaviourReader();

            var initialize = reader.ExtractData(FileText)
                .EntityBehaviours
                .First(m => m.Name.Equals("WarehouseForm_onChange__code"));

            initialize.Element.ShouldBe("_code");
        }


        [Fact]
        public void ExtractData_WithAfterChange_ValidType()
        {
            var reader = new UIEntityBehaviourReader();

            var afterChange = reader.ExtractData(FileText)
                .EntityBehaviours
                .First(m => m.Name.Equals("afterChange"));

            afterChange.Type.ShouldBe(Omnia.CLI.Commands.Model.Apply.Data.UI.UIBehaviourType.AfterChange);
        }

        [Fact]
        public void ExtractData_WithAfterChange_ValidExpression()
        {
            var reader = new UIEntityBehaviourReader();

            var afterChange = reader.ExtractData(FileText)
                .EntityBehaviours
                .First(m => m.Name.Equals("afterChange"));
            afterChange.Expression.ShouldBe(@"this._name = ""Hello World 3!"";
            this._name = ""Hello World 4!"";");
        }

        [Fact]
        public void ExtractData_WithBeforeChange_ValidType()
        {
            var reader = new UIEntityBehaviourReader();

            var beforeChange = reader.ExtractData(FileText)
                .EntityBehaviours
                .First(m => m.Name.Equals("beforeChange"));

            beforeChange.Type.ShouldBe(Omnia.CLI.Commands.Model.Apply.Data.UI.UIBehaviourType.BeforeChange);
        }

        [Fact]
        public void ExtractData_WithBeforeChange_ValidExpression()
        {
            var reader = new UIEntityBehaviourReader();

            var beforeChange = reader.ExtractData(FileText)
                .EntityBehaviours
                .First(m => m.Name.Equals("beforeChange"));
            beforeChange.Expression.ShouldBe("this._name = \"Hello World 2!\";");
        }

        [Fact]
        public void ExtractData_WithBeforeSave_ValidType()
        {
            var reader = new UIEntityBehaviourReader();

            var beforeSave = reader.ExtractData(FileText)
                .EntityBehaviours
                .First(m => m.Name.Equals("beforeSave"));

            beforeSave.Type.ShouldBe(Omnia.CLI.Commands.Model.Apply.Data.UI.UIBehaviourType.BeforeSave);
        }

        [Fact]
        public void ExtractData_WithBeforeSave_ValidExpression()
        {
            var reader = new UIEntityBehaviourReader();

            var beforeSave = reader.ExtractData(FileText)
                .EntityBehaviours
                .First(m => m.Name.Equals("OnBeforeSave"));
            beforeSave.Expression.ShouldBe("_name = \"tst\";");
        }
    }
}
