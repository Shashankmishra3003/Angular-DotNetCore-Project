using DatingApp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Data
{
    public interface IDatingRepository
    {
        // A method with type of T(User, Photo) where T will be a class 
        void Add<T>(T entity) where T : class;

        void Delete<T>(T entity) where T : class;

        Task<bool> SaveAll();

        Task<IEnumerable<User>> GetUsers(); // Get al lusers

        Task<User> GetUser(int id);  // Get a single user
    }
}
