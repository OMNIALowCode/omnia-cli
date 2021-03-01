using Omnia.CLI.Commands.Model.Apply.Readers.Database;
using Shouldly;
using System;
using Xunit;

namespace UnitTests.Commands.Model.Apply.Database
{
    public class QueryReaderTest
    {
        private const string FileText = @"Select _code, _name from vw_usera";

        [Fact]
        public void ExtractData_ExpressionSuccessfully()
        {
            var reader = new QueryReader();

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
			var reader = new QueryReader();

			Assert.Throws<ArgumentNullException>(() => reader.ExtractData(text));
		}

	}
}
