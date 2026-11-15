using InsuranceAgency.Core.Interfaces;

namespace InsuranceAgency.Core.Repositories;

public class InMemoryRepository<T> : IRepository<T> where T : class
{
    private readonly List<T> _items;

    public InMemoryRepository(List<T> items)
    {
        _items = items;
    }


    public void Add(T item)
    {
        _items.Add(item);
    }

    public bool Remove(T item)
    {
        return _items.Remove(item);
    }

    public List<T> GetAll()
    {
        return _items;
    }
}
