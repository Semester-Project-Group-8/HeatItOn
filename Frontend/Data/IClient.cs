using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frontend.Data;

public interface IClient<T>
{
    Task<T?> Get(int id);
    Task<List<T>> GetAll();
    Task Post(T item);
    Task Put(T item);
    Task Delete(int id);
}