using System.Text;
using Xunit.Abstractions;

namespace TestMeterLib
{
    public class RedirectOutput(ITestOutputHelper output) : TextWriter
    {
        private readonly ITestOutputHelper _output = output;

        public override Encoding Encoding { get; } = Encoding.UTF8;

        public override void WriteLine(string? value)
        {
            try
            {
                _output.WriteLine(value);
            }
            catch { }
        }
    }

}