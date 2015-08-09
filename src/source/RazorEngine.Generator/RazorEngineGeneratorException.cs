using System;
using System.Runtime.Serialization;

namespace RazorEngine.Generator
{
    [Serializable]
    public class RazorEnginegeneratorException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public RazorEnginegeneratorException()
        {
        }

        public RazorEnginegeneratorException(string message) : base(message)
        {
        }

        public RazorEnginegeneratorException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RazorEnginegeneratorException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}