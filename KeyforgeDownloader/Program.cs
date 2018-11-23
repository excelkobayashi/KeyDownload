using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyforgeDownloader
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				// Like this: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
				string id = Prompt("Enter a deck id");

				if(String.IsNullOrWhiteSpace(id))
					return;

				string path;
				while(true)
				{
					path = Prompt("Save to image file");
					if(String.IsNullOrWhiteSpace(path))
						return;

					if(File.Exists(path))
					{
						if(PromptYesNo("Overwrite this file", false))
							break;
					}
					else
						break;
				}

				Deck deck = Process(id);
				if(deck != null)
					deck.Save(path);
			}
			catch(AggregateException ex)
			{
				if(ex.InnerExceptions != null)
				{
					foreach(Exception innerEx in ex.InnerExceptions)
						ShowException(innerEx);
				}
			}
			catch(Exception ex)
			{
				ShowException(ex);
			}

			Console.WriteLine();
			Console.WriteLine("Press any key...");
			Console.ReadKey();
		}

		private static Deck Process(string id)
		{
			var parser = new DeckParser(id);
			parser.OnProgressUpdated += OnProgressUpdated;

			Task<Deck> task = parser.Parse();
			task.Wait();

			if(task.Status == TaskStatus.RanToCompletion)
			{
				Deck deck = task.Result;
				if(deck == null)
					throw new InvalidOperationException("Missing deck data");

				return deck;
			}
			else if(task.Status == TaskStatus.Faulted)
			{
				if(task.Exception == null)
					throw new Exception("Unknown exception");
				else
					throw task.Exception;
			}
			else if(task.Status == TaskStatus.Canceled)
			{
				return null;
			}
			else
			{
				throw new Exception("Invalid task status: " + task.Status.ToString(), task.Exception);
			}
		}

		private static void OnProgressUpdated(Progress progress)
		{
			Console.Write(progress.Message);

			if(progress.Total > 0)
				Console.Write($" ({progress.Current} / {progress.Total})");

			Console.WriteLine();
		}

		private static void ShowException(Exception ex)
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

		private static string Prompt(string message)
		{
			Console.Write($"{message}: ");
			return Console.ReadLine();
		}

		private static bool PromptYesNo(string message, bool defaultValue)
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
	}
}
