using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyforgeDownloader
{
	/// <summary>
	///		Main program logic
	/// </summary>
	class Program
	{
		private static ProgressDisplay ProgressDisplay = new ProgressDisplay();

		/// <summary>
		///		Prompt the user and process
		/// </summary>
		/// <param name="args">Unused</param>
		static void Main(string[] args)
		{
			string input = ConsoleHelper.Prompt("Enter a deck id or URL");
			if(String.IsNullOrWhiteSpace(input))
				return;

			string path = ConsoleHelper.PromptFile("Save to image file");
			if(String.IsNullOrWhiteSpace(path))
				return;

			Console.WriteLine();

			try
			{
				string id = IdFinder.FindId(input);
				Process(id, path);
			}
			catch(AggregateException ex)
			{
				ConsoleHelper.ShowAggregateException(ex);
			}
			catch(Exception ex)
			{
				ConsoleHelper.ShowException(ex);
			}

			Console.WriteLine();
			Console.WriteLine("Press any key...");
			Console.ReadKey();
		}

		/// <summary>
		///		Run the <see cref="DeckParser"/> and save the output
		/// </summary>
		/// <param name="id">Deck id (GUID format)</param>
		/// <param name="path">Image file path for saving the deck image</param>
		private static void Process(string id, string path)
		{
			var parser = new DeckParser(id);
			parser.OnProgressUpdated += OnProgressUpdated;

			using(Deck deck = parser.Parse())
				deck?.Save(path);
		}

		/// <summary>
		///		Display processing progress
		/// </summary>
		/// <param name="progress">Current progress</param>
		private static void OnProgressUpdated(Progress progress) => ProgressDisplay.Update(progress);
	}
}
