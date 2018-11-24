using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyforgeDownloader
{
	class ProgressDisplay
	{
		/// <summary>Previous progress displayed</summary>
		private Progress? LastProgress = null;

		/// <summary>Whether a progress bar is current visible</summary>
		private bool BarVisible = false;

		/// <summary>
		///		Show new progress, including a progress bar (when indicated)
		/// </summary>
		/// <param name="progress">New progress</param>
		public void Update(Progress progress)
		{
			if(IsNewOperation(progress))
				NextOperation(progress);

			ShowBar(progress);
		}

		/// <summary>
		///		Check if this progress represents a new operation, 
		///		which should be displayed on a new line.
		///		Otherwise, only the progress bar should be updated.
		/// </summary>
		/// <param name="progress">New progress</param>
		/// <returns>False if the new progress has the same message as the last, and a step count is provided</returns>
		private bool IsNewOperation(Progress progress)
		{
			if(progress.Total == 0)
				return true;

			if(LastProgress == null || progress.Message != LastProgress.Value.Message)
				return true;

			return false;
		}

		/// <summary>
		///		Show the progress message on a new line
		/// </summary>
		/// <param name="progress">New progress</param>
		private void NextOperation(Progress progress)
		{
			ClearBar();
			Console.WriteLine(progress.Message);
			LastProgress = progress;
		}

		/// <summary>
		///		Remove the visible progress bar
		/// </summary>
		private void ClearBar()
		{
			if(!BarVisible)
				return;

			BarVisible = false;

			int width = Console.BufferWidth;
			string line = new String(' ', width);
			WriteBar(line);
		}

		/// <summary>
		///		Show a progress bar that represents the difference
		///		between the total step count and the current step count.
		/// </summary>
		/// <param name="progress">New progress</param>
		private void ShowBar(Progress progress)
		{
			if(progress.Total == 0)
			{
				ClearBar();
				return;
			}

			BarVisible = true;

			int width = Console.BufferWidth;
			var line = new StringBuilder("[", width);
			width -= 2;

			double scale = (double)width / progress.Total;
			int completed = Convert.ToInt32(progress.Current * scale);
			completed = Math.Max(Math.Min(completed, width), 0);
			int remaining = width - completed;

			if(completed > 0)
				line.Append('#', completed);

			if(remaining > 0)
				line.Append(' ', remaining);

			line.Append(']');
			WriteBar(line.ToString());
		}

		/// <summary>
		///		Render text that spans an entire line on the screen
		///		that can be replaced with subsequent calls
		/// </summary>
		/// <param name="line">Text to display</param>
		private static void WriteBar(string line)
		{
			int startTop = Console.CursorTop;
			Console.Write(line);
			Console.CursorLeft = 0;
			Console.CursorTop = startTop;
		}
	}
}
