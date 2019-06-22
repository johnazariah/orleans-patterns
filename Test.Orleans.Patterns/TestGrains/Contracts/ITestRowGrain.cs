using Orleans.Patterns.TableRowPattern;

namespace Test.Orleans.Patterns.Contracts
{

    public interface ITestRowGrain : IRowGrain<TestRowState>
    { }

}
