using System.Threading.Tasks;

namespace Orleans.Patterns.EventSourcing
{
    public interface IEventAggregatorGrain : IGrainWithGuidKey
    {
        Task RefreshState();

        Task<T> GetValue<T>();
    }
}
