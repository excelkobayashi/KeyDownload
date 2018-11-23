using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyforgeDownloader
{
	struct Progress
	{
		public int Current;
		public int Total;
		public string Message;

		public Progress(string message, int total = 0, int current = 0)
		{
			Message = message;
			Current = current;
			Total = total;
		}
	}
}
