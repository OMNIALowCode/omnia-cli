using Moq;
using Omnia.CLI.Commands.Model.Apply;
using Omnia.CLI.Commands.Model.Apply.Data.UI;
using Omnia.CLI.Infrastructure;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Commands.Model.Apply
{
    public class UIBehavioursApplyServiceTest
    {
        private const string FormMetadata = "{\"name\":\"CustomerForm\",\"type\":\"Form\",\"label\":\"Customer\",\"entity\":\"Customer\",\"elements\":[{\"row\":1,\"name\":\"_code\",\"size\":6,\"type\":\"Input\",\"label\":\"Code\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"1\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Text\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":1,\"name\":\"_name\",\"size\":6,\"type\":\"Input\",\"label\":\"Name\",\"column\":7,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"1\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Text\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":2,\"name\":\"_description\",\"size\":6,\"type\":\"Input\",\"label\":\"Description\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"0\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Text\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":2,\"name\":\"_inactive\",\"size\":6,\"type\":\"Input\",\"label\":\"Inactive\",\"column\":7,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"1\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Boolean\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":4,\"name\":\"Today\",\"size\":6,\"type\":\"Input\",\"label\":\"Today\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"0\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Date\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":5,\"name\":\"_state\",\"size\":6,\"type\":\"Selector\",\"label\":\"State\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"min\",\"value\":\"1\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"enumeration\",\"value\":\"CustomerStateMachineStates\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":6,\"name\":\"_assigned\",\"size\":6,\"type\":\"Input\",\"label\":\"Assigned\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"0\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Text\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":7,\"name\":\"Notes\",\"size\":12,\"type\":\"List\",\"label\":\"Notes\",\"column\":1,\"elements\":[{\"row\":0,\"name\":\"_code\",\"size\":3,\"type\":\"Input\",\"label\":\"Code\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"1\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Text\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":1,\"name\":\"_name\",\"size\":3,\"type\":\"Input\",\"label\":\"Name\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"1\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Text\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":2,\"name\":\"_description\",\"size\":3,\"type\":\"Input\",\"label\":\"Description\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"0\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Text\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null},{\"row\":3,\"name\":\"_inactive\",\"size\":3,\"type\":\"Input\",\"label\":\"Inactive\",\"column\":1,\"elements\":[],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"None\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"1\"},{\"key\":\"min\",\"value\":\"1\"},{\"key\":\"isSensitiveData\",\"value\":\"false\"},{\"key\":\"formattingType\",\"value\":\"Boolean\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null}],\"helpText\":null,\"isHidden\":false,\"attributes\":[{\"key\":\"aggregationKind\",\"value\":\"Composite\"},{\"key\":\"isReadOnly\",\"value\":\"false\"},{\"key\":\"max\",\"value\":\"100\"},{\"key\":\"min\",\"value\":\"0\"},{\"key\":\"definition\",\"value\":\"Notes\"},{\"key\":\"isEditable\",\"value\":\"true\"}],\"behaviours\":[],\"description\":null,\"visibleFrom\":null}],\"helpText\":null,\"attributes\":[],\"behaviours\":[{\"name\":\"onInitialize\",\"type\":\"Initialize\",\"expression\":\"\",\"description\":\"\"},{\"name\":\"onBeforeChange\",\"type\":\"BeforeChange\",\"expression\":\"\",\"description\":\"\"},{\"name\":\"onAfterChange\",\"type\":\"AfterChange\",\"expression\":\"\",\"description\":\"\"},{\"name\":\"BeforeSave2\",\"type\":\"BeforeSave\",\"expression\":\"\",\"description\":\"onBeforeSaveonBeforeSaveonBeforeSaveonBeforeSaveonBeforeSave OLE\"}],\"dataSource\":\"System\",\"description\":null}";

        private const string Tenant = "Template";
        private const string Environment = "PRD";
        private const string Definition = "Form";
        private const string Entity = "CustomerForm";

        [Fact]
        public async Task ReplaceData_WithBehavioursList_Successful()
        {
            var behaviours = new List<UIBehaviour>
            {
                new UIBehaviour()
                {
                    Definition = Entity,
                    Element = "_code",
                    Expression = "_name = _code;",
                    Name = "OnCodeChange",
                    Type = UIBehaviourType.Change
                },
                new UIBehaviour()
                {
                    Definition = "Notes",
                    Element = "_code",
                    Expression = "_name = _code;",
                    Name = "OnNotesCodeChange",
                    Type = UIBehaviourType.Change
                }
            };
            var data = new UIEntity(behaviours);

            var apiClientMock = new Mock<IApiClient>();

            apiClientMock.Setup(r => r.Get($"/api/v1/{Tenant}/{Environment}/model/{Definition}/{Entity}"))
                .ReturnsAsync((new ApiResponse(true), FormMetadata));
            apiClientMock.Setup(r => r.Get($"/api/v1/{Tenant}/{Environment}/model/output/metadata/{Entity}"))
                .ReturnsAsync((new ApiResponse(true), "{\"type\":\"Form\"}"));
            apiClientMock.Setup(r => r.Patch(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .ReturnsAsync((new ApiResponse(true)));

            var service = new UIBehavioursApplyService(apiClientMock.Object);

            await service.ReplaceData(Tenant, Environment, Entity, data)
                .ConfigureAwait(false);

            apiClientMock.Verify(r => r.Patch($"/api/v1/{Tenant}/{Environment}/model/{Definition}/{Entity}",
            It.Is<StringContent>(s => s.ReadAsStringAsync().Result.Equals(
                "[{\"value\":[{\"Name\":\"OnCodeChange\",\"Description\":null,\"Element\":\"_code\",\"Type\":0,\"Expression\":\"_name = _code;\",\"Definition\":\"CustomerForm\"}],\"path\":\"/elements/0/behaviours\",\"op\":\"replace\"},{\"value\":[{\"Name\":\"OnNotesCodeChange\",\"Description\":null,\"Element\":\"_code\",\"Type\":0,\"Expression\":\"_name = _code;\",\"Definition\":\"Notes\"}],\"path\":\"/elements/7/elements/0/behaviours\",\"op\":\"replace\"}]"
                ))
            ));
        }
    }
}
