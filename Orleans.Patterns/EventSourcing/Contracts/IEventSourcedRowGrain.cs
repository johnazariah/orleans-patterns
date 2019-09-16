using System;
using System.Threading.Tasks;

namespace Orleans.Patterns.EventSourcing
{
    public interface IEventSourcedRowGrain : IEventSourcedGrain
    {
        Task<Guid> GetIdentity();

        Task MarkDeleted();

        Task MarkUndeleted();

        Task<bool> IsDeleted();

        Task<string> GetRowJson();

        Task SetRowJson(string payload);

        //Task<List<string>> GetColumnNames();

        //Task<T> GetColumnValue<T>(string columnName) where T : ISerializable;

        //Task SetColumnValue<T>(string columnName, T value) where T : ISerializable;
    }
}
