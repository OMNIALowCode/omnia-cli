﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Omnia.CLI.Commands.Model.Apply;
using Shouldly;
using Xunit;

namespace UnitTests.Commands.Model.Apply
{
    public class FormElementPathFinderTest
    {
        private const string FormMetadata = "{\"name\":\"CustomerForm\",\"type\":\"Form\",\"label\":\"Customer\",\"entity\":\"Customer\",\"elements\":[{\"row\":1,\"name\":\"_code\",\"size\":6,\"type\":\"Input\",\"label\":\"Code\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"1\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Text\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":1,\"name\":\"_name\",\"size\":6,\"type\":\"Input\",\"label\":\"Name\",\"column\":7,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"1\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Text\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":2,\"name\":\"_description\",\"size\":6,\"type\":\"Input\",\"label\":\"Description\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"0\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Text\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":2,\"name\":\"_inactive\",\"size\":6,\"type\":\"Input\",\"label\":\"Inactive\",\"column\":7,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"1\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Boolean\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":5,\"name\":\"_state\",\"size\":6,\"type\":\"Selector\",\"label\":\"State\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"min\",\"value\":\"1\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"enumeration\",\"value\":\"CustomerStateMachineStates\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":6,\"name\":\"_assigned\",\"size\":6,\"type\":\"Input\",\"label\":\"Assigned\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"0\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Text\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":10,\"name\":\"Dates\",\"size\":12,\"type\":\"Container\",\"label\":\"Dates\",\"column\":1,\"elements\":[{\"row\":1,\"name\":\"Today\",\"size\":10,\"type\":\"Input\",\"label\":\"Today\",\"column\":1,\"elements\":[],\"helpText\":\"\",\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"0\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Date\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":\"Mobile\"}],\"helpText\":\"\",\"isHidden\":false,\"attributes\":[],\"behaviours\":[],\"description\":null,\"visibleFrom\":\"Mobile\"},{\"row\":7,\"name\":\"NoteList\",\"size\":12,\"type\":\"List\",\"label\":\"Note List\",\"column\":1,\"elements\":[{\"row\":0,\"name\":\"_code\",\"size\":3,\"type\":\"Input\",\"label\":\"Code\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"1\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Text\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":1,\"name\":\"_name\",\"size\":3,\"type\":\"Input\",\"label\":\"Name\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"1\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Text\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":2,\"name\":\"_description\",\"size\":3,\"type\":\"Input\",\"label\":\"Description\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"0\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Text\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":3,\"name\":\"_inactive\",\"size\":3,\"type\":\"Input\",\"label\":\"Inactive\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"1\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Boolean\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":4,\"name\":\"SubNotes\",\"size\":12,\"type\":\"List\",\"label\":\"Sub Notes\",\"column\":1,\"elements\":[{\"row\":0,\"name\":\"_code\",\"size\":3,\"type\":\"Input\",\"label\":\"Code\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"1\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Text\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":1,\"name\":\"_name\",\"size\":3,\"type\":\"Input\",\"label\":\"Name\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"1\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Text\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":2,\"name\":\"_description\",\"size\":3,\"type\":\"Input\",\"label\":\"Description\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"0\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Text\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":3,\"name\":\"_inactive\",\"size\":3,\"type\":\"Input\",\"label\":\"Inactive\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"1\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Boolean\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null}],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"Composite\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"100\"},{\"key\":\"min\",\"value\":\"0\"},{\"key\":\"definition\",\"value\":\"SubNotes\"},{\"key\":\"isEditable\",\"value\":\"true\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null}],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"Composite\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"100\"},{\"key\":\"min\",\"value\":\"0\"},{\"key\":\"definition\",\"value\":\"Notes\"},{\"key\":\"isEditable\",\"value\":\"true\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null}],\"helpText\":null,\"attributes\":[],\"behaviours\":[{\"name\":\"onInitialize\",\"type\":\"Initialize\",\"expression\":\"//BLA BLA\",\"description\":\"\"},{\"name\":\"onBeforeChange\",\"type\":\"BeforeChange\",\"expression\":\"//onBeforeChange\",\"description\":\"\"},{\"name\":\"onAfterChange\",\"type\":\"AfterChange\",\"expression\":\"//onAfterChange\",\"description\":\"\"},{\"name\":\"BeforeSave2\",\"type\":\"BeforeSave\",\"expression\":\"//BOLA\",\"description\":\"onBeforeSaveonBeforeSaveonBeforeSaveonBeforeSaveonBeforeSave OLE\"}],\"dataSource\":\"System\",\"description\":null}";

        [Theory]
        [InlineData("_code", "/elements/0")]
        [InlineData("_name", "/elements/1")]
        public void Find_TopLevelElements_Successful(string element, string expectedPath)
        {
            var finder = new FormElementPathFinder((JObject)JsonConvert.DeserializeObject(FormMetadata));

            var path = finder.Find("CustomerForm", element);

            path.ShouldBe(expectedPath);
        }

        [Fact]
        public void Find_ChildLevelElements_Successful()
        {
            var finder = new FormElementPathFinder((JObject)JsonConvert.DeserializeObject(FormMetadata));

            var path = finder.Find("NoteList", "_code");

            path.ShouldBe("/elements/7/elements/0");
        }

        [Fact]
        public void Find_ChildLevelElementsUsingDifferentCasing_Successful()
        {
            var finder = new FormElementPathFinder((JObject)JsonConvert.DeserializeObject(FormMetadata));

            var path = finder.Find("NOTELIST", "_CODE");

            path.ShouldBe("/elements/7/elements/0");
        }

        [Fact]
        public void Find_InnerChildLevelElements_Successful()
        {
            var finder = new FormElementPathFinder((JObject)JsonConvert.DeserializeObject(FormMetadata));

            var path = finder.Find("SubNotes", "_code");

            path.ShouldBe("/elements/7/elements/4/elements/0");
        }
    }
}
