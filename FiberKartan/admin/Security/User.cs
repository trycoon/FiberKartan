using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;

/*
The zlib/libpng License
Copyright (c) 2012 Henrik Östman

This software is provided 'as-is', without any express or implied warranty. In no event will the authors be held liable for any damages arising from the use of this software.
Permission is granted to anyone to use this software for any purpose, including commercial applications, and to alter it and redistribute it freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software. If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
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