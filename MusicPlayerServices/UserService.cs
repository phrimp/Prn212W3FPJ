using Microsoft.EntityFrameworkCore;
using MusicPlayerEntities;
using MusicPlayerRepositories;
using MusicPlayerRepositories.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerServices
{
    public class UserService
    {
        public User GetOne(int id) => UserRepository.Instance.GetOne(id);

        public User? SignIn(string accountIdentify, string password)
        => UserRepository.Instance.SignIn(accountIdentify, password);

        public List<User> GetAll()
        => UserRepository.Instance.GetAll();

        public void Add(User user)
        => UserRepository.Instance.Add(user);

        public void Update(User a)
        => UserRepository.Instance.Update(a);

        public void Delete(int id)
        => UserRepository.Instance.Delete(id);

        public List<Song> GetMyFavotites(int id) => UserRepository.Instance.GetMyFavotites(id);

        // Role-related methods
        public bool IsUserInRole(int userId, string roleName)
        => UserRepository.Instance.IsUserInRole(userId, roleName);

        public bool IsUserInRole(int userId, int roleId)
        => UserRepository.Instance.IsUserInRole(userId, roleId);

        public void UpdateUserRole(int userId, int roleId)
        => UserRepository.Instance.UpdateUserRole(userId, roleId);

        public List<Role> GetAllRoles()
        => UserRepository.Instance.GetAllRoles();

        public string GetUserRoleName(int userId)
        => UserRepository.Instance.GetUserRoleName(userId);
    }
}