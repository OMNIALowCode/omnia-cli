using Moq;
using Omnia.CLI.Commands.Model.Apply;
using Omnia.CLI.Commands.Model.Apply.Data.UI;
using Omnia.CLI.Infrastructure;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Commands.Model.Apply.UI
{
	public class WebComponentApplyServiceTest
	{
		private const string Tenant = "Template";
		private const string Environment = "PRD";
		private const string Entity = "Textarea";

		[Fact]
		public async Task ReplaceData_Successful()
		{
			var data = new WebComponent()
            {
				Expression= @"// OMNIA Low-Code Development Platform
// Text Area Web Component

class TextareaElement extends HTMLElement {

    constructor() {
        super();

        this.textarea = document.createElement('textarea');
        this.textarea.setAttribute('class', 'form-control');

        this.textarea.onchange = this.valueUpdated.bind(this);
    }

    connectedCallback() {
        this.appendChild(this.textarea);
    }

    valueUpdated(event) {
        // When the onChange event is fired, dispatch a new event 'value-updated' to notify the OMNIA Platform that the values has been updated
        this.dispatchEvent(new CustomEvent('value-updated', {
            detail: {
                value: event.target.value
            }
        }));
    }

    set value(newValue) {
        this.textarea.value = newValue;
    }

    set isReadOnly(newValue) {
        this.textarea.disabled = (newValue === true);
    }

    set isPreviousValue(newValue) {
        this.textarea.style.textDecoration = newValue === true ? 'line-through' : null;
    }

    set valueHasChanged(newValue) {
        this.textarea.style.border = newValue === true ? '2px solid #ffe187' : null;
    }
}

customElements.define('omnia-textarea', TextareaElement);",
                CustomElement= "omnia-textarea"
            };

			var apiClientMock = new Mock<IApiClient>();

			apiClientMock.Setup(r => r.Patch(It.IsAny<string>(), It.IsAny<HttpContent>()))
				.ReturnsAsync((new ApiResponse(true)));

			var service = new WebComponentApplyService(apiClientMock.Object);

			await service.ReplaceData(Tenant, Environment, Entity, data)
				.ConfigureAwait(false);

			apiClientMock.Verify(r => r.Patch($"/api/v1/{Tenant}/{Environment}/model/webcomponent/{Entity}",
			It.IsAny<StringContent>()));
		}
	}
}
