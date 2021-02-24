using System;
using System.Runtime.Serialization;

namespace StoreDAL
{    
    [Serializable]
    public class DataServiceException : Exception
    {       
        public DataServiceException(string message) : base(message)
        {
        }

        public DataServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DataServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}