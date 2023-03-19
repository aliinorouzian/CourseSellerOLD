using CourseSeller.DataLayer.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CourseSeller.DataLayer.Contexts
{
    public class MSSQLSContext : DbContext
    {
        public MSSQLSContext(DbContextOptions<MSSQLSContext> options)
            : base(options) // pass opt to father class
        {
        }


        #region Users

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }


        #endregion
    }
}
