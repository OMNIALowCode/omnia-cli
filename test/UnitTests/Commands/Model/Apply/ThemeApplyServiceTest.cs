using Moq;
using Omnia.CLI.Commands.Model.Apply;
using Omnia.CLI.Commands.Model.Apply.Data;
using Omnia.CLI.Infrastructure;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Commands.Model.Apply
{
	public class ThemeApplyServiceTest
	{
		private const string Tenant = "Template";
		private const string Environment = "PRD";
		private const string Entity = "DarkTheme";

		[Fact]
		public async Task ReplaceData_Successful()
		{
			var data = new Theme()
            {
				Expression= @"// OMNIA Low-Code
// Dark Theme 1.0

//
// Color system
//

$white:    #fff;
$gray-100: #f8f9fa;
$gray-200: #ebebeb;
$gray-300: #dee2e6;
$gray-400: #ced4da;
$gray-500: #e9ecef;
$gray-600: #999;
$gray-700: #444;
$gray-800: #303030;
$gray-900: #222;
$black:    #000;

$blue:    #2998f6;
$indigo:  #6610f2;
$purple:  #6f42c1;
$pink:    #e83e8c;
$red:     #E74C3C;
$orange:  #fd7e14;
$yellow:  #F39C12;
$green:   #00bc8c;
$teal:    #20c997;
$cyan:    #3498DB;

$primary:       $blue;
$secondary:     $gray-600;
$success:       $blue;
$info:          $cyan;
$warning:       $yellow;
$danger:        $red;
$light:         $gray-600;
$dark:          $gray-800;

$yiq-contrasted-threshold:  175;

// Body

$body-bg:                   $gray-900;
$body-color:                $white;

// Links

$link-color:                $blue;

// Fonts

$font-family-sans-serif:      ""Lato"", -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif, ""Apple Color Emoji"", ""Segoe UI Emoji"", ""Segoe UI Symbol"";"

			};

			var apiClientMock = new Mock<IApiClient>();

			apiClientMock.Setup(r => r.Patch(It.IsAny<string>(), It.IsAny<HttpContent>()))
				.ReturnsAsync((new ApiResponse(true)));

			var service = new ThemeApplyService(apiClientMock.Object);

			await service.ReplaceData(Tenant, Environment, Entity, data)
				.ConfigureAwait(false);

			apiClientMock.Verify(r => r.Patch($"/api/v1/{Tenant}/{Environment}/model/theme/{Entity}",
			It.IsAny<StringContent>()));
		}
	}
}
