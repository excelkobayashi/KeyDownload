using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyforgeDownloader
{
	/// <summary>
	///		Utilities for formatting console input/output
	/// </summary>
	static class ConsoleHelper
	{
		/// <summary>
		///		Print an exception, along with any inner exceptions
		/// </summary>
		/// <param name="ex">Exception</param>
		public static void ShowException(Exception ex)
		{
			Console.Error.WriteLine();
			Console.Error.Write("Error: ");
			Console.Error.WriteLine(ex.Message);

			ex = ex.InnerException;
			while(ex != null)
			{
				Console.Error.Write("--> ");
				Console.Error.WriteLine(ex.Message);
				ex = ex.InnerException;
			}
		}

		/// <summary>
		///		Print an aggregate exception by printing each of the inner exceptions
		/// </summary>
		/// <param name="ex"></param>
		public static void ShowAggregateException(AggregateException ex)
		{
			if(ex.InnerExceptions == null)
			{
				ShowException(ex);
			}
			else
			{
				foreach(Exception innerEx in ex.InnerExceptions)
					ShowException(innerEx);
			}
		}

		/// <summary>
		///		Prompt for input
		/// </summary>
		/// <param name="message">Message to display</param>
		/// <returns>A line of input</returns>
		public static string Prompt(string message)
		{
			Console.Write($"{message}: ");
			return Console.ReadLine();
		}

		/// <summary>
		///		Prompt the user for a yes/no response.
		///		Also accepts partial inputs.
		///		Invalid inputs map to the default selection.
		/// </summary>
		/// <param name="message">Message to display</param>
		/// <param name="defaultValue">Default selection value</param>
		/// <returns>The user's choice</returns>
		public static bool PromptYesNo(string message, bool defaultValue)
		{
			Console.Write(message);

			string otherTarget;
			if(defaultValue)
			{
				Console.Write(" (Y/n)? ");
				otherTarget = "no";
			}
			else
			{
				Console.Write(" (y/N)? ");
				otherTarget = "yes";
			}

			string input = Console.ReadLine();

			if(String.IsNullOrEmpty(input))
				return defaultValue;

			input = input.ToLower();
			string targetSubset = otherTarget.Substring(0, Math.Min(otherTarget.Length, input.Length));

			if(input == targetSubset)
				return !defaultValue;
			else
				return defaultValue;
		}

		/// <summary>
		///		Prompt the user for a file path.
		///		Will also prompt to overwrite, and repeats the prompt otherwise.
		/// </summary>
		/// <param name="message">Message to display</param>
		/// <returns></returns>
		public static string PromptFile(string message)
		{
			string path;
			while(true)
			{
				path = Prompt("Save to image file");
				if(String.IsNullOrWhiteSpace(path))
					return null;

				if(File.Exists(path))
				{
					if(PromptYesNo("Overwrite this file", false))
						break;
				}
				else if(IsValidFilePath(path))
					break;
				else
					Console.WriteLine("Invalid file path");
			}

			return path;
		}

		/// <summary>
		///		Check if a file can be created at this path
		/// </summary>
		/// <param name="path">File path</param>
		/// <returns>Whether a file could be created</returns>
		private static bool IsValidFilePath(string path)
		{
			try
			{
				using(FileStream file = File.Create(path, 1, FileOptions.DeleteOnClose))
					return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
