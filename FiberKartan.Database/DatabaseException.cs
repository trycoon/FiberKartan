﻿using System;
using System.Runtime.Serialization;

namespace FiberKartan.Database
{
    [Serializable]
    public class DatabaseException : Exception
    {
        public DatabaseException()
            : base() { }

        public DatabaseException(string message)
            : base(message) { }

        public DatabaseException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public DatabaseException(string message, Exception innerException)
            : base(message, innerException) { }

        public DatabaseException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }

        protected DatabaseException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}