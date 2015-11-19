using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.Web;
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
    public class RequestProfiler : IParameterInspector
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Method executed before REST-operation is executed.
        /// </summary>
        /// <param name="operationName"></param>
        /// <param name="inputs"></param>
        /// <returns>correlationState</returns>
        public object BeforeCall(string operationName, object[] inputs)
        {
            var operationContext = OperationContext.Current;
            var securityContext = ServiceSecurityContext.Current;

            string user = null;

            if (securityContext != null)
            {
                user = securityContext.PrimaryIdentity.Name;
            }

            log.InfoFormat("Begin request to resource \"{0}\" for user \"{1}\" having IP: {2}, mapping to operation \"{3}\".", operationContext.IncomingMessageHeaders.To.AbsolutePath, user, HttpContext.Current.Request.UserHostAddress, operationName);
            
            return DateTime.Now;
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
            var operationDuration = DateTime.Now.Subtract((DateTime)correlationState);
            var operationContext = OperationContext.Current;
            var securityContext = ServiceSecurityContext.Current;

            string user = null;

            if (securityContext != null)
            {
                user = securityContext.PrimaryIdentity.Name;
            }

            log.InfoFormat("End request to resource \"{0}\" for user \"{1}\" having IP: {2}, request duration: {3}", operationContext.IncomingMessageHeaders.To.AbsolutePath, user, HttpContext.Current.Request.UserHostAddress, operationDuration);
        }
    }
}