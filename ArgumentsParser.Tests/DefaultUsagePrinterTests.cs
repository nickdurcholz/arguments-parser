using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Moq;
using Xunit;

namespace ArgumentsParser
{
    public class DefaultUsagePrinterTests
    {
        private const string ProgramDescription = "this message will self destruct in 10 seconds";
        private const string ExecutableName = "my-program";

        private List<IArgument> _args;
        private UsagePrinter _printer;

        public DefaultUsagePrinterTests()
        {
            _args = new List<IArgument>();
            _printer = new UsagePrinter();
            _printer.Description = ProgramDescription;
            _printer.Executable = ExecutableName;
        }

        [Fact]
        public void PrintUsage_displays_one_parameter_indented_on_line_separate_from_description()
        {
            _args.Add(CreateArgInfo<int>("p1", "parm1", "the one and only parameter", true).Object);

            string[] lines = GetLines();

            Assert.True(lines.Length >= 2, "At least two lines");
            string parmLine = lines[2];

            //string should 
            //  -start with whitespace (assumption is that said whitespace is indentation)
            //  -both short name and long name are present
            //  -description is present
            Assert.True(Regex.IsMatch(parmLine, @"^\s+"), "parameter line is indented");
            Assert.Contains("p1", parmLine);
            Assert.Contains("parm1", parmLine);
            Assert.Contains("the one and only parameter", parmLine);
        }

        [Fact]
        public void PrintUsage_displays_sample_usage_with_required_indicators()
        {
            _args.Add(CreateArgInfo<int>("p1", "parm1", "optional", false).Object);
            _args.Add(CreateArgInfo<string>("p2", "parm2", "required", true).Object);

            string usage = GetUsage();
            Assert.Contains(ExecutableName + " [-parm1 <int>] -parm2 <string>", usage);
        }

        [Fact]
        public void first_line_contains_program_description_and_name()
        {
            string[] usage = GetLines();

            Assert.Contains(ProgramDescription, usage[0]);
            Assert.Contains(ExecutableName, usage[0]);
        }

        [Fact]
        public void correct_message_is_printed_for_multiple_arguments()
        {
            string ExpectedUsage = string.Join(
                Environment.NewLine,
                new[]
                {
                    "program.exe - <program description>",
                    "",
                    "usage: program.exe [-parm1 <int>] -parm2 <string> ",
                    "",
                    "  p1,parm1 - int; optional.  <p1 description>",
                    "  p2,parm2 - string; required.  <p2 description>",
                    ""
                });
            _printer.Description = "<program description>";
            _printer.Executable = "program.exe";

            _args.Add(CreateArgInfo<int>("p1", "parm1", "<p1 description>", false).Object);
            _args.Add(CreateArgInfo<string>("p2", "parm2", "<p2 description>", true).Object);

            Assert.Equal(ExpectedUsage, GetUsage());
        }

        [Fact]
        public void DefaultValue_is_printed_for_optional_argument()
        {
            var arg1 = CreateArgInfo<string>("p", "parameter", "description", false);
            var arg2 = CreateArgInfo<string>("a", "asdf", "adsf", false);
            var arg3 = CreateArgInfo<DateTime>("b", "date", "date", false);
            var arg4 = CreateArgInfo<int>("i", "int", "int parameter", false);
            arg1.Setup(arg => arg.DefaultValue).Returns("tumbleweed");
            arg2.Setup(arg => arg.DefaultValue).Returns(default(string));
            arg3.Setup(arg => arg.DefaultValue).Returns(default(DateTime));
            arg4.Setup(arg => arg.DefaultValue).Returns(39);
            _args.Add(arg1.Object);
            _args.Add(arg2.Object);
            _args.Add(arg3.Object);
            _args.Add(arg4.Object);

            string usage = GetUsage();
            Assert.Contains("tumbleweed", usage);
            Assert.Contains("39", usage);
            string[] usageLines = usage.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string asdfLine = usageLines[3];
            Assert.False(
                asdfLine.Contains("date"),
                "A default value was printed for an optional parameter that does not have one");
            Assert.False(
                asdfLine.Contains("default"),
                "A default value was printed for an optional parameter that does not have one");
            string iline = usageLines.Single(l => l.Contains("int parameter"));
            Assert.True(iline.Contains(" 39. "));
            string parameterLine = usageLines.Single(l => l.Contains("description"));
            Assert.True(parameterLine.Contains(" tumbleweed. "));
        }

        private Mock<IArgument> CreateArgInfo<T1>(string shortName, string longName, string description, bool isRequired)
        {
            var mock = new Mock<IArgument>();
            mock.Setup(arg => arg.ShortName).Returns(shortName);
            mock.Setup(arg => arg.LongName).Returns(longName);
            mock.Setup(arg => arg.Description).Returns(description);
            mock.Setup(arg => arg.IsRequired).Returns(isRequired);
            mock.Setup(arg => arg.Type).Returns(typeof (T1));
            return mock;
        }

        private string GetUsage()
        {
            var writer = new StringWriter();
            _printer.PrintUsage(writer, _args);
            return writer.ToString();
        }

        private string[] GetLines()
        {
            return GetUsage().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}