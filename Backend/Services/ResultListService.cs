using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
namespace Backend.Services
{
    public class ResultListService
    {
        private readonly BackendDbContext _dbContext;
        public ResultListService(BackendDbContext dbContext)
        {
            _dbContext=dbContext;
        }
        // Need Result service
    }
}