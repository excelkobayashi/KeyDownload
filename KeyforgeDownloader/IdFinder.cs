using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KeyforgeDownloader
{
	static class IdFinder
	{
		public static string FindId(string input)
		{
			// Like this: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
			const string idChar = "[0-9a-zA-Z]";
			var re = new Regex(idChar + "{8}-" + idChar + "{4}-" + idChar + "{4}-" + idChar + "{4}-" + idChar + "{12}");

			Match match = re.Match(input);
			if(match == null || !match.Success)
				return input;

			return match.Value;
		}
	}
}
