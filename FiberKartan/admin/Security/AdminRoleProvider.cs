﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

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
    public class AdminRoleProvider : RoleProvider
    {
        //IUserRepository _repository;
        public AdminRoleProvider()
            //: this(UserRepositoryFactory.GetRepository())
        {

        }
        /*public AdminRoleProvider(IUserRepository repository)
            : base()
        {
            _repository = repository ?? UserRepositoryFactory.GetRepository();
        }*/
        public override bool IsUserInRole(string username, string roleName)
        {
            /*User user = _repository.GetByUserName(username);
            if (user != null)
                return user.IsInRole(roleName);
            else*/
                return false;
        }

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        public override string[] GetRolesForUser(string username)
        {
            return new string[] {};
            /*User user = _repository.GetByUserName(username);
            string[] roles = new string[user.Role.Rights.Count + 1];
            roles[0] = user.Role.Description;
            int idx = 0;
            foreach (Right right in user.Role.Rights)
                roles[++idx] = right.Description;

            return roles;*/
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();

        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();

        }
    }
}