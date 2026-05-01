using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Services
{
    public interface IService<T>
    {
        Task<List<T>> List();
        Task<List<T>> Get(int id);
        Task<T> Post();
        Task Put(int id, T value);
        Task Delete(int id);
    }
}
