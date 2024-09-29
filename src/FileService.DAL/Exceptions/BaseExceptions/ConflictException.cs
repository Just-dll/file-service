using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FileService.DAL.Exceptions.BaseExceptions
{

	[Serializable]
	public abstract class ConflictException : Exception
	{
		public ConflictException() { }
		public ConflictException(string message) : base(message) { }
		public ConflictException(string message, Exception inner) : base(message, inner) { }
		protected ConflictException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

    }
}
