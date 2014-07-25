using System;
using Xunit;

namespace ArgumentsParser
{
    public class CommandLineArgumentTests
    {
        private Arguments _parser;

        public CommandLineArgumentTests()
        {
            _parser = new Arguments("test", GetType().Assembly);
        }

        [Fact]
        public void exception_is_thrown_when_argments_are_not_valid()
        {
            _parser.IsValid = false;

            var arg = new Argument<int>(_parser, "shortName", "longName", "description", true);
            int value;
            Assert.Throws<InvalidOperationException>(() => value = arg.Value);
        }

        [Fact]
        public void Value_returns_default_value_when_IsMissing()
        {
            _parser.IsValid = true;
            var arg = new Argument<string>(_parser, "short", "long", "foo", false);
            arg.IsMissing = true;
            arg.DefaultValue = "gobbles!";
            Assert.Equal("gobbles!", arg.Value);
        }

        [Fact]
        public void IArgumentInfo_DefaultValue_returns_same_as_typed_property()
        {
            var arg = new Argument<string>(_parser, "short", "long", "foo", false);
            arg.DefaultValue = "timmeh!";

            Assert.Equal("timmeh!", ((IArgument)arg).DefaultValue);
        }

        [Fact]
        public void IsMissing_defaults_to_true()
        {
            var arg = new Argument<string>(_parser, "i", "i", "i", true);
            Assert.True(arg.IsMissing);
        }
    }
}