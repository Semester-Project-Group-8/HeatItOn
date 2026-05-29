using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frontend.Interfaces;

public interface IClient<T>
{
    Task<T?> Get(int id);
    Task<List<T>> GetAll();
    Task<bool> Post(T item);
    Task<bool> Put(T item);
    Task<bool> Delete(int id);
}