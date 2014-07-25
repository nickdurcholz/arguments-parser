using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using Xunit;

namespace ArgumentsParser
{
    public class ArgumentsTests
    {
        private Arguments _args;
        private Mock<UsagePrinter> _usagePrinter;

        public ArgumentsTests()
        {
            _usagePrinter = new Mock<UsagePrinter>();
            _args = new Arguments("Args unit tests", typeof (ArgumentsTests).Assembly);
            _args.UsagePrinter = _usagePrinter.Object;
        }

        [Fact]
        public void can_parse_args_without_slash_or_dash()
        {
            Argument<string> arg1 = _args.Add<string>("p1", "parameter1", "first parameter", true);
            Argument<DateTime> arg2 = _args.Add<DateTime>("p2", "parameter2", "the second parameter", false);
            Argument<int> arg3 = _args.Add<int>("p3", "parameter3", "the third parameter", true);

            _args.Parse(new[] {"p1", "foo", "p2", "01/01/2001", "p3", "1"});
            Assert.True(_args.IsValid);
            Assert.Equal("foo", arg1.Value);
            Assert.Equal(new DateTime(2001, 1, 1), arg2.Value);
            Assert.Equal(1, arg3.Value);
        }

        [Fact]
        public void can_parse_args_without_slash_or_dash_and_colon_separator()
        {
            Argument<string> arg1 = _args.Add<string>("p1", "parameter1", "first parameter", true);
            Argument<DateTime> arg2 = _args.Add<DateTime>("p2", "parameter2", "the second parameter", false);
            Argument<int> arg3 = _args.Add<int>("p3", "parameter3", "the third parameter", true);

            _args.Parse(new[] {"p1:foo", "p2:01/01/2001", "p3:1"}, ArgumentParseOptions.ColonSeparatesArgValues);
            Assert.True(_args.IsValid);
            Assert.Equal("foo", arg1.Value);
            Assert.Equal(new DateTime(2001, 1, 1), arg2.Value);
            Assert.Equal(1, arg3.Value);
        }

        [Fact]
        public void PrintUsage_is_delegated_to_printer()
        {
            TextWriter textWriter = TextWriter.Null;
            _args.PrintUsage(textWriter);
            _usagePrinter.Verify(p => p.PrintUsage(textWriter, It.IsAny<IEnumerable<IArgument>>()));
        }

        [Fact]
        public void correct_argument_info_is_passed_to_usage_printer()
        {
            Argument<int> arg1 = _args.Add<int>("i", "int", "int description", true);
            Argument<string> arg2 = _args.Add<string>("s", "string", "string description", true);

            TextWriter textWriter = TextWriter.Null;
            bool printUsageWasCalledAndVerifiedGood = false;
            _usagePrinter.Setup(p => p.PrintUsage(textWriter, It.IsAny<IEnumerable<IArgument>>())).Callback(
                (TextWriter writer, IEnumerable<IArgument> a) =>
                {
                    List<IArgument> argsPrinted = a.ToList();
                    Assert.NotNull(argsPrinted);
                    Assert.Equal(2, argsPrinted.Count);
                    Assert.Contains(arg1, argsPrinted);
                    Assert.Contains(arg2, argsPrinted);
                    printUsageWasCalledAndVerifiedGood = true;
                });
            _args.PrintUsage(textWriter);
            Assert.True(printUsageWasCalledAndVerifiedGood);
        }

        [Fact]
        public void string_assembly_constructor_sets_UsagePrinter()
        {
            Assert.NotNull(new Arguments("foo", typeof (int).Assembly).UsagePrinter);
        }

        [Fact]
        public void short_form_syntax_is_parsed_correctly()
        {
            Argument<int> p = _args.Add<int>("p", "parm1", "the one and only parameter", true);

            _args.Parse(new[] {"/p", "42"});
            Assert.Equal(42, p.Value);

            _args.Parse(new[] {"-p", "0"});
            Assert.Equal(0, p.Value);

            _args.Parse(new[] {"/p:3"}, ArgumentParseOptions.ColonSeparatesArgValues);
            Assert.Equal(3, p.Value);

            _args.Parse(new[] {"-p:5"}, ArgumentParseOptions.ColonSeparatesArgValues);
            Assert.Equal(5, p.Value);
        }

        [Fact]
        public void long_form_syntax_is_parsed_correctly()
        {
            Argument<int> cla = _args.Add<int>("p", "parm1", "the one and only parameter", true);

            _args.Parse(new[] {"/parm1", "42"});
            Assert.Equal(42, cla.Value);

            _args.Parse(new[] {"-parm1", "0"});
            Assert.Equal(0, cla.Value);

            _args.Parse(new[] {"/parm1:3"}, ArgumentParseOptions.ColonSeparatesArgValues);
            Assert.Equal(3, cla.Value);

            _args.Parse(new[] {"-parm1:5"}, ArgumentParseOptions.ColonSeparatesArgValues);
            Assert.Equal(5, cla.Value);
        }

        [Fact]
        public void string_argments_are_parsed_correctly()
        {
            Argument<string> p = _args.Add<string>("p", "parm1", "the one and only parameter", true);

            _args.Parse(new[] {"/p", "a string"});
            Assert.Equal("a string", p.Value);

            _args.Parse(new[] {"-p", "a string"});
            Assert.Equal("a string", p.Value);

            _args.Parse(new[] {"/p:a string"}, ArgumentParseOptions.ColonSeparatesArgValues);
            Assert.Equal("a string", p.Value);

            _args.Parse(new[] {"-p:a string"}, ArgumentParseOptions.ColonSeparatesArgValues);
            Assert.Equal("a string", p.Value);
        }

        [Fact]
        public void quotes_are_trimmed_from_strings_when_TrimQuotes_true()
        {
            Argument<string> p = _args.Add<string>("p", "parm1", "the one and only parameter", true);

            _args.Parse(new[] {"/p", "\"a string\""}, ArgumentParseOptions.TrimQuotes);
            Assert.Equal("a string", p.Value);

            _args.Parse(new[] {"-p", "'a string'"}, ArgumentParseOptions.TrimQuotes);
            Assert.Equal("a string", p.Value);

            _args.Parse(new[] {"/p:'a string'"}, ArgumentParseOptions.TrimQuotes | ArgumentParseOptions.ColonSeparatesArgValues);
            Assert.Equal("a string", p.Value);

            _args.Parse(new[] {"-p:\"a string\""}, ArgumentParseOptions.TrimQuotes | ArgumentParseOptions.ColonSeparatesArgValues);
            Assert.Equal("a string", p.Value);

            _args.Parse(new[] {"-p:\"a string"}, ArgumentParseOptions.TrimQuotes | ArgumentParseOptions.ColonSeparatesArgValues);
            Assert.Equal("a string", p.Value);

            _args.Parse(new[] {"-p:'a string"}, ArgumentParseOptions.TrimQuotes | ArgumentParseOptions.ColonSeparatesArgValues);
            Assert.Equal("a string", p.Value);
        }

        [Fact]
        public void quotes_are_not_trimmed_from_strings_when_TrimQuotes_false()
        {
            Argument<string> p = _args.Add<string>("p", "parm1", "the one and only parameter", true);

            string value = "\"a string\"";
            _args.Parse(new[] {"/p", value}, ArgumentParseOptions.None);
            Assert.Equal(value, p.Value);

            value = "'a string'";
            _args.Parse(new[] {"/p", value}, ArgumentParseOptions.None);
            Assert.Equal(value, p.Value);

            _args.Parse(new[] {"/p:'a string'"}, ArgumentParseOptions.ColonSeparatesArgValues);
            Assert.Equal("'a string'", p.Value);

            _args.Parse(new[] {"-p:\"a string\""}, ArgumentParseOptions.ColonSeparatesArgValues);
            Assert.Equal("\"a string\"", p.Value);

            _args.Parse(new[] {"-p:\"a string"}, ArgumentParseOptions.ColonSeparatesArgValues);
            Assert.Equal("\"a string", p.Value);

            _args.Parse(new[] {"-p:'a string"}, ArgumentParseOptions.ColonSeparatesArgValues);
            Assert.Equal("'a string", p.Value);
        }

        [Fact]
        public void IsValid_false_when_type_mismatch()
        {
            _args.Add<int>("p", "parm1", "the one and only parameter", true);

            _args.Parse(new[] {"/p", "a string"});

            Assert.False(_args.IsValid);
        }

        [Fact]
        public void IsValid_false_when_unrecognized_parameter()
        {
            _args.Add<int>("p", "parm1", "the one and only parameter", true);

            _args.Parse(new[] {"/k", "a string"});

            Assert.False(_args.IsValid);
        }

        [Fact]
        public void IsValid_false_when_required_parameter_missing()
        {
            _args.Add<DateTime>("p1", "parm1", "required paramater", true);
            _args.Add<int>("p2", "parm2", "optional parameter", false);

            _args.Parse(new[] {"p2", "10"});

            Assert.False(_args.IsValid);
        }

        [Fact]
        public void exception_is_thrown_when_shortName_is_duplicated()
        {
            _args.Add<int>("p", "parm1", "foo", true);
            var ex = Assert.Throws<ArgumentException>(() => _args.Add<int>("p", "parm2", "bar", false));
            Assert.Contains("same name", ex.Message);
        }

        [Fact]
        public void exception_is_thrown_when_longName_is_duplicated()
        {
            _args.Add<int>("p1", "parm", "foo", true);
            var ex = Assert.Throws<ArgumentException>(() => _args.Add<int>("p2", "parm", "bar", false));
            Assert.Contains("same name", ex.Message);
        }

        [Fact]
        public void IsValid_false_when_too_many_argments()
        {
            _args.Add<int>("i", "int", "int description", false);
            _args.Add<string>("s", "string", "string description", false);

            _args.Parse(new[] {"/s", "sval", "/i:1", "3"});
            Assert.False(_args.IsValid);
        }

        [Fact]
        public void IsValid_false_when_positional_parameter_is_passed()
        {
            _args.Add<int>("i", "int", "int description", false);
            _args.Add<string>("s", "string", "string description", false);

            _args.Parse(new[] {"sval", "/i:3"});
            Assert.False(_args.IsValid);
        }

        [Fact]
        public void Value_is_set_during_Parse()
        {
            Argument<int> parameter = _args.Add<int>("i", "int", "description", false);
            _args.Parse(new[] {"-i", "92"});
            Assert.Equal(92, parameter.Value);
        }

        [Fact]
        public void IsValid_defaults_to_false()
        {
            var arguments = new Arguments("foo", typeof (string).Assembly);
            Assert.False(arguments.IsValid);
        }

        [Fact]
        public void IsMissing_is_set_during_Parse()
        {
            Argument<int> i = _args.Add<int>("i", "int", "description", false);
            Argument<int> j = _args.Add<int>("j", "j", "description", false);
            Argument<int> k = _args.Add<int>("k", "k", "description", true);
            _args.Parse(new[] {"-i", "92", "-k", "3"});
            Assert.False(i.IsMissing);
            Assert.True(j.IsMissing);
            Assert.False(k.IsMissing);
        }

        [Fact]
        public void PrintErrors_prints_message_for_extra_args()
        {
            _args.Parse(new[] {"-i", "2"});
            Assert.False(_args.IsValid);
            var writer = new StringWriter();
            _args.PrintErrors(writer);
            Assert.True(writer.ToString().Contains("unknown argument 'i'"));
        }

        [Fact]
        public void PrintErrors_prints_message_for_missing_require_argument()
        {
            _args.Add<int>("i", "i", "i", true);
            _args.Add<int>("j", "j", "j", true);
            _args.Parse(new[] {"-i", "2"});
            Assert.False(_args.IsValid);
            var writer = new StringWriter();
            _args.PrintErrors(writer);
            Assert.True(writer.ToString().Contains("missing argument 'j'"));
        }

        [Fact]
        public void PrintErrors_prints_message_for_invalid_type()
        {
            _args.Add<DateTime>("d", "date", "date", true);
            _args.Parse(new[] {"-d", "false"});
            Assert.False(_args.IsValid);
            var writer = new StringWriter();
            _args.PrintErrors(writer);
            Assert.Contains("invalid argument value for -d: false", writer.ToString());
        }

        [Fact]
        public void PrintErrors_prints_message_for_extraneous_parameters()
        {
            _args.Add<bool>("b", "bool", "bool", true);
            _args.Parse(new[] {"-b", "false", "foo"});
            Assert.False(_args.IsValid);
            var writer = new StringWriter();
            _args.PrintErrors(writer);
            Assert.Contains("unknown argument 'foo'", writer.ToString());
        }

        [Fact]
        public void Can_omit_a_non_required_switch()
        {
            SwitchArgument switchArg = _args.AddSwitch("r", "recurse", "recursive operation");
            _args.Parse(new string[0]);
            Assert.True(_args.IsValid, "Valid arguments reported as not valid");
            Assert.False(switchArg.Value);
            Assert.True(switchArg.IsMissing);
        }

        [Fact]
        public void Can_provide_a_non_required_switch()
        {
            SwitchArgument switchArg = _args.AddSwitch("r", "recurse", "recursive operation");
            _args.Parse(new[] {"-r"});
            Assert.True(_args.IsValid);
            Assert.True(switchArg.Value);
            Assert.False(switchArg.IsMissing);
        }

        [Fact]
        public void Can_provide_a_non_required_switch_with_other_args()
        {
            Argument<int> num = _args.Add<int>("n", "num", "a number", true);
            SwitchArgument recurse = _args.AddSwitch("r", "recurse", "recursive operation");
            _args.Parse(new[] {"-r", "-n", "1"});
            Assert.True(_args.IsValid);
            Assert.True(recurse.Value);
            Assert.False(recurse.IsMissing);
            Assert.Equal(1, num.Value);
            Assert.False(num.IsMissing);
        }

        [Fact]
        public void Unmatched_argument_with_a_colon_is_invalid()
        {
            _args.Add<int>("n", "n", "a number", false);
            _args.Parse(new[] {"-m", "http://foo"});
            Assert.False(_args.IsValid);
        }

        [Fact]
        public void Can_pass_multiple_values_for_single_argument()
        {
            Argument<int> n = _args.Add<int>("n", "num", "a number", true);
            _args.Parse(new[] {"-n", "0", "-n", "9", "-n", "3"});

            Assert.True(_args.IsValid);
            Assert.Equal(new[] {0, 9, 3}, n.Values);
        }
    }
}