using Microsoft.Extensions.Logging;
using Orleans.Patterns.TableRowPattern;
using Test.Orleans.Patterns.Contracts;

namespace Test.Orleans.Patterns.Grains
{
    public class TestRowGrain : RowGrain<TestRowState>, ITestRowGrain
    {
        public TestRowGrain(ILogger<RowGrain<TestRowState>> logger) : base(logger)
        {
        }
    }

}
