using Microsoft.Extensions.Logging;
using Orleans.Patterns.TableRowPattern;
using Test.Orleans.Patterns.Contracts;

namespace Test.Orleans.Patterns.Grains
{
    public class TestRowTableGrain : RowTableGrain<ITestRowGrain, TestRowState>, ITestRowTableGrain
    {
        public TestRowTableGrain(ILogger<RowTableGrain<ITestRowGrain, TestRowState>> logger) : base(logger)
        {
        }
    }
}
