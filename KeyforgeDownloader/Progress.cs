using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyforgeDownloader
{
	/// <summary>
	///		Progress for a long-running operation
	/// </summary>
	struct Progress
	{
		/// <summary>Current step number</summary>
		public int Current;

		/// <summary>Total number of steps</summary>
		public int Total;

		/// <summary>Description of the current operation</summary>
		public string Message;

		public Progress(string message, int total = 0, int current = 0)
		{
			Message = message;
			Current = current;
			Total = total;
		}
	}
}
