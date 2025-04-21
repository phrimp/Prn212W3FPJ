using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicPlayerEntities;
using MusicPlayerRepositories.Utils;
using Microsoft.EntityFrameworkCore;

namespace MusicPlayerRepositories
{
    public class UserRepository
    {
        private MusicPlayerAppContext _dbContext;
        private static UserRepository instance;

        public UserRepository()
        {
            _dbContext = new MusicPlayerAppContext();
        }

        public static UserRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UserRepository();
                }
                return instance;
            }
        }

        public User? SignIn(string accountIdentify, string password)
        {
            string encryptedPassword = MyUtils.Encrypt(password);

            return _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefault(u =>
                    (u.Username == accountIdentify || u.Email == accountIdentify) &&
                    u.PasswordHash == encryptedPassword &&
                    u.IsActive == true
                );
        }

        public User? GetOne(int id)
        {
            return _dbContext.Users
                .Include(u => u.Role)
                .SingleOrDefault(a => a.UserId.Equals(id));
        }

        public List<User> GetAll()
        {
            return _dbContext.Users
                .Include(u => u.Role)
                .ToList();
        }

        public void Add(User user)
        {
            if (user == null)
                throw new Exception();

            var existingUser = GetOne(user.UserId);
            if (existingUser != null)
                throw new Exception();

            user.PasswordHash = MyUtils.Encrypt(user.PasswordHash);

            // Set default role as User (1) if not specified
            if (user.RoleId <= 0)
            {
                user.RoleId = 1;
            }

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
        }

        public void Update(User a)
        {
            User? cur = GetOne(a.UserId);
            if (cur == null)
            {
                throw new Exception();
            }
            _dbContext.Entry(cur).CurrentValues.SetValues(a);
            _dbContext.SaveChanges();
        }

        public void Delete(int id)
        {
            User? cur = GetOne(id);
            if (cur != null)
            {
                _dbContext.Users.Remove(cur);
                _dbContext.SaveChanges();
            }
        }

        public List<Song> GetMyFavotites(int id)
        {
            User? cur = GetOne(id);
            if (cur == null)
            {
                throw new Exception("User not found");
            }

            var favoriteSongs = _dbContext.UserFavorites.Where(uf => uf.UserId == id).Select(uf => uf.Song).ToList();
            return favoriteSongs;
        }

        // Role-related methods
        public bool IsUserInRole(int userId, string roleName)
        {
            var user = _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserId == userId);

            return user != null && user.Role.Name == roleName;
        }

        public bool IsUserInRole(int userId, int roleId)
        {
            var user = GetOne(userId);
            return user != null && user.RoleId == roleId;
        }

        public void UpdateUserRole(int userId, int roleId)
        {
            var user = GetOne(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var role = _dbContext.Roles.FirstOrDefault(r => r.RoleId == roleId);
            if (role == null)
            {
                throw new Exception("Role not found");
            }

            user.RoleId = roleId;
            _dbContext.SaveChanges();
        }

        public List<Role> GetAllRoles()
        {
            return _dbContext.Roles.ToList();
        }

        public string GetUserRoleName(int userId)
        {
            var user = _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserId == userId);

            return user?.Role?.Name ?? "Unknown";
        }
    }
}