// :D
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frontend.Data;

public interface IClient<T>
{
    Task<T?> Get(int id);
    Task<List<T>?> GetAll();
    Task Post(T item);
    Task Update(T item); // Put and Patch
    Task Delete(int id);
}