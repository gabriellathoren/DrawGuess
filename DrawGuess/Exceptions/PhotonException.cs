using System;
using System.Runtime.Serialization;

namespace DrawGuess.Exceptions
{
    [Serializable]
    internal class PhotonException : Exception
    {
        public PhotonException()
        {
        }

        public PhotonException(string message) : base(message)
        {
        }

        public PhotonException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PhotonException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}