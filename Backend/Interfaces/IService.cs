using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Models;
namespace Backend.Services
{
    public interface IService<T>
    {
        Task<List<T>> List();
        Task<T> Get(int id);
        Task Post(T model);
        Task Put(int id, T value);
        Task Delete(int id);
    }
}
