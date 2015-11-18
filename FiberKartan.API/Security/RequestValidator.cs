using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

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
        #region IParameterInspector Members

        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
            DateTime endCall = DateTime.Now;
            DateTime startCall = (DateTime)correlationState;
            TimeSpan operationDuration = endCall.Subtract(startCall);
        }

        public object BeforeCall(string operationName, object[] inputs)
        {
            OperationContext operationContext = OperationContext.Current;
            ServiceSecurityContext securityContext = ServiceSecurityContext.Current;

            string user = null;
            bool isAnonymous = true;

            if (securityContext != null)
            {
                user = securityContext.PrimaryIdentity.Name;
                isAnonymous = securityContext.IsAnonymous;
            }

            Uri remoteAddress = operationContext.Channel.LocalAddress.Uri;
            string sessionId = operationContext.SessionId;
            MessageVersion messageVersion = operationContext.IncomingMessageVersion;

            Trace.WriteLine("Username: " + user);
            Trace.WriteLine("Is Anonymoys" + isAnonymous);
            Trace.WriteLine("Server address: " + remoteAddress);
            Trace.WriteLine("Session id: " + sessionId);
            Trace.WriteLine("Message version: " + messageVersion);
            Trace.WriteLine("Operation:" + operationName);
            Trace.WriteLine("Arguments:");
            foreach (object input in inputs)
                Trace.WriteLine(input);

            return DateTime.Now;
        }

        #endregion
    }
}