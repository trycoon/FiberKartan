﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
    public class Role
    {
        public virtual int RoleId { get; set; }
        public virtual string Description { get; set; }
        public virtual IList<Right> Rights { get; set; }

        protected Role() { }

        public Role(int roleid, string roleDescription)
        {
            RoleId = roleid;
            Description = roleDescription;
        }
    }
}