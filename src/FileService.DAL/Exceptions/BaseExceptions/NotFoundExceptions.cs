using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FileService.DAL.Exceptions.BaseExceptions
{

	[Serializable]
	public abstract class NotFoundException : HttpException
	{
		public NotFoundException() { }
		public NotFoundException(string message) : base(message) { }
		public NotFoundException(string message, Exception inner) : base(message, inner) { }
		protected NotFoundException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
