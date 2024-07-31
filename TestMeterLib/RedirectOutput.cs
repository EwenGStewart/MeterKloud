using System.Text;
using Xunit.Abstractions;

namespace TestMeterLib
{
    public class RedirectOutput : TextWriter
    {
        private readonly ITestOutputHelper _output;

        public RedirectOutput(ITestOutputHelper output)
        {
            _output = output;
        }

        public override Encoding Encoding { get; } = Encoding.UTF8;

        public override void WriteLine(string? value)
        {
            _output.WriteLine(value);
        }
    }

}