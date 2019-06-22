using System;

namespace Orleans.Patterns.TableRowPattern
{
    public interface IStateWithIdentity
    {
        Guid Id { get; set; }
    }
}
