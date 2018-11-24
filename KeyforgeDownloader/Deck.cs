using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyforgeDownloader
{
	/// <summary>
	///		Assembles card images into a single deck sheet
	/// </summary>
	public class Deck : IDisposable
	{
		/// <summary>Number of cards on each side</summary>
		private Size SheetSize; //6x6

		/// <summary>Size in pixels of each card</summary>
		private Size CardSize; // 300x420

		/// <summary>Location in card rows/columns where the next card should be rendered</summary>
		private Point CurrentPosition;

		/// <summary>Deck sheet image</summary>
		private Bitmap Bitmap;

		/// <summary>Graphics device context for <see cref="Bitmap"/></summary>
		private Graphics Graphics;

		/// <summary>
		///		Constructor
		/// </summary>
		/// <param name="count">Total number of cards to be added</param>
		public Deck(int count)
		{
			if(count < 1)
				throw new ArgumentOutOfRangeException(nameof(count), count, "Must be positive");

			SetSheetSize(count);
		}

		/// <summary>
		///		Deconstructor
		/// </summary>
		~Deck()
		{
			Dispose();
		}

		/// <summary>
		///		Clean up graphics resources
		/// </summary>
		public void Dispose()
		{
			Graphics?.Dispose();
			Graphics = null;
			Bitmap?.Dispose();
			Bitmap = null;
		}

		/// <summary>
		///		Setup graphics resources
		/// </summary>
		/// <param name="card">A card, which will determine the graphics format for the bitmap</param>
		private void Initialize(Image card)
		{
			if(Bitmap != null || Graphics != null)
				throw new InvalidOperationException("Cannot be initialized again");

			if(card == null)
				throw new ArgumentNullException(nameof(card));

			CurrentPosition = Point.Empty;
			CardSize = card.Size;
			var finalSize = new Size(SheetSize.Width * CardSize.Width, SheetSize.Height * CardSize.Height);

			Bitmap = new Bitmap(card, finalSize);
			Graphics = Graphics.FromImage(Bitmap);

			Graphics.Clear(Color.Black);
		}

		/// <summary>
		///		Determine a roughly square sheet size large enough to hold all of the cards
		/// </summary>
		/// <param name="count">Total number of cards to be added</param>
		private void SetSheetSize(int count)
		{
			double root = Math.Sqrt(count);

			int width = (int)Math.Floor(root);
			int height = Math.DivRem(count, width, out int remainder);

			if(remainder != 0)
				height++;

			SheetSize = new Size(width, height);
		}

		/// <summary>
		///		Determine the next row/column to render a card
		/// </summary>
		private void AdvancePosition()
		{
			CurrentPosition.X++;

			if(CurrentPosition.X >= SheetSize.Width)
			{
				CurrentPosition.X = 0;
				CurrentPosition.Y++;
			}
		}

		/// <summary>
		///		Check whether a card can be added
		/// </summary>
		/// <param name="size">Card size (must be the same for all cards)</param>
		/// <param name="position">Top-left corner in pixels where the card will be drawn</param>
		private void Validate(Size size, Point position)
		{
			if(CurrentPosition.Y >= SheetSize.Height)
				throw new IndexOutOfRangeException("Attempted to add too many cards");

			if(size != CardSize)
				throw new InvalidOperationException($"Cards must have the same dimensions. The first card added was {CardSize}, and this card is {size}.");

			if(position.X + CardSize.Width > Bitmap.Width
				|| position.Y + CardSize.Height > Bitmap.Height)
				throw new InvalidOperationException("This card overflows the edge of the output image");
		}

		/// <summary>
		///		Add a card to the deck image
		/// </summary>
		/// <param name="card">Card image</param>
		public void AddCard(Image card)
		{
			if(Bitmap == null)
				Initialize(card);

			Size size = card.Size;
			var pos = new Point(size.Width * CurrentPosition.X, size.Height * CurrentPosition.Y);
			Validate(size, pos);

			Graphics.DrawImageUnscaled(card, pos);

			AdvancePosition();
		}

		/// <summary>
		///		Save the deck image to a file
		/// </summary>
		/// <param name="path">File path</param>
		public void Save(string path)
		{
			Graphics.Flush();
			Bitmap.Save(path);
		}
	}
}
