using Omnia.CLI.Commands.Model.Apply.Readers.UI;
using Shouldly;
using System;
using Xunit;

namespace UnitTests.Commands.Model.Apply.UI
{
    public class ThemeReaderTest
    {
        private const string FileText =
@"
// OMNIA Low-Code
// Original Theme 1.0
// Developer Note: the styles below are just a suggestion, you can choose to use them or not as a base for your new theme.

// Bootstrap Default Color System
$white:    #fff;
$gray-100: #f8f9fa;
$gray-200: #ecf0f1;
$gray-300: #dee2e6;
$gray-400: #ced4da;
$gray-500: #b4bcc2;
$gray-600: #95a5a6;
$gray-700: #7b8a8b;
$gray-800: #343a40;
$gray-900: #212529;
$black:    #000;

$blue:    #2C3E50;
$indigo:  #6610f2;
$purple:  #6f42c1;
$pink:    #e83e8c;
$red:     #E74C3C;
$orange:  #fd7e14;
$yellow:  #F39C12;
$green:   #18BC9C;
$teal:    #20c997;
$cyan:    #3498DB;

$primary:       $blue;
$secondary:     $gray-600;
$success:       $green;
$info:          $cyan;
$warning:       $yellow;
$danger:        $red;
$light:         $gray-200;
$dark:          $gray-700;

// BODY
$body-bg: #fff;
$body-color: #212529;

 
// GENERAL
$content-size: 96%;
$color-page-indicator-bar-height: 0.25rem;
$section-border-color: rgba(0, 0, 0, 0.16);


// OMNIA Custom Variables

// SIDE PANEL
// Right-side panel that opens when the user accesses Notifications or History Tab.
$side-panel-bg-color: $body-bg;
$side-panel-min-size-open: 430px;
$side-panel-size-sm-open: 70%;
$side-panel-border-color: $section-border-color;
 
// SIDEBAR
// Sidebar Menu
$sidebar-bg-color: $body-bg;
$sidebar-border-color: $section-border-color;
$sidebar-color: $body-color;
$sidebar-icon-color: lighten($sidebar-color, 35%);
$sidebar-min-size: 423px;
$sidebar-min-size-open: $side-panel-min-size-open;
$sidebar-height-sm-open: 50px;
$sidebar-size-sm: $sidebar-min-size;
$sidebar-size-sm-open: $side-panel-size-sm-open;
 
// TOPBAR
// Topbar navigation
$topbar-height: 45px;
$topbar-bg-color: $body-bg;
$topbar-border-color: $section-border-color;
$topbar-color: $body-color;
";

        [Fact]
        public void ExtractData_ExpressionSuccessfully()
        {
            var reader = new ThemeReader();

            var component = reader.ExtractData(FileText);

            component.Expression.ShouldNotBeNull();
            component.Expression.ShouldBe(FileText);
        }

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		public void ExtractData_WhenTextIsEmpty_ExceptionRaised(string text)
		{
			var reader = new ThemeReader();

			Assert.Throws<ArgumentNullException>(() => reader.ExtractData(text));
		}

	}
}
