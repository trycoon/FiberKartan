using System.Net;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Web;
using System.Web.Security;
using FiberKartan.Database;
using log4net;

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
namespace FiberKartan.API.Security
{
    public class RequestValidator : IParameterInspector
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly MsSQL db = new MsSQL();

        /// <summary>
        /// Method executed before REST-operation is executed.
        /// </summary>
        /// <param name="operationName"></param>
        /// <param name="inputs"></param>
        /// <returns>correlationState</returns>
        public object BeforeCall(string operationName, object[] inputs)
        {
            var operationContext = OperationContext.Current;
            CallContext.LogicalSetData("currentUser", null);    // Clear out possible reference, this should never happend...

            if (!string.IsNullOrEmpty(HttpContext.Current.User.Identity.Name)) {
                var user = db.GetUserByUsername(HttpContext.Current.User.Identity.Name);

                if (user != null && !user.IsDeleted) {
                    CallContext.LogicalSetData("currentUser", user);
                }
            }

            return null;
        }

        /// <summary>
        /// Method executed after REST-operation is executed.
        /// </summary>
        /// <param name="operationName"></param>
        /// <param name="outputs"></param>
        /// <param name="returnValue"></param>
        /// <param name="correlationState"></param>
        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
            CallContext.LogicalSetData("currentUser", null);    // This should not be needed since CallContext should be unique for every request, but we do it anyway to be safe.
        }
    }
}