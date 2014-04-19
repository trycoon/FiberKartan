using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiberKartan.Database
{
    public class DAL
    {
        private static FiberDataContext fiberDb = new FiberDataContext();

        public DAL()
        {            
        }

        public static User GetUserByUsername(string username)
        {
            var user = (from u in fiberDb.Users where (u.Username == username) select u).First();

            return user;
        }
    }
}
