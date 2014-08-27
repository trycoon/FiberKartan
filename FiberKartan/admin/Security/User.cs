using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;

/*
Copyright (c) 2012, Henrik Östman.

This file is part of FiberKartan.

FiberKartan is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

FiberKartan is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with FiberKartan.  If not, see <http://www.gnu.org/licenses/>.
*/
namespace FiberKartan.Admin.Security
{
    public class User : IPrincipal
    {
        public virtual int UserId { get; set; }
        public virtual string UserName { get; set; }
        public virtual string FullName { get; set; }
        public virtual string Password { get; set; }
        public virtual IIdentity Identity { get; set; }
        public virtual string Description { get; set; }
        public virtual DateTime LastLoggedOn { get; set; }

        protected User() { }

        public User(int userId, string userName, string fullName, string password)
        {
            UserId = userId;
            UserName = userName;
            FullName = fullName;
            Password = password;
        }        

        public virtual bool IsInRole(string role)
        {            
            return false;
        }
    }
}