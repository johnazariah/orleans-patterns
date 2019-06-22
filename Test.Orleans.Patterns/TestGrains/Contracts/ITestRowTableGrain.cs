using Orleans.Patterns.TableRowPattern;

namespace Test.Orleans.Patterns.Contracts
{
    public interface ITestRowTableGrain : IRowTableGrain<ITestRowGrain, TestRowState>
    { }
}
