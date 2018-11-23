using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyforgeDownloader
{
	class Deck : IDisposable
	{
		private static readonly Size SheetSize = new Size(6, 6);
		private Size CardSize;
		private Point CurrentPosition;
		private Bitmap Bitmap;
		private Graphics Graphics;

		~Deck()
		{
			Dispose();
		}

		public void Dispose()
		{
			Graphics?.Dispose();
			Graphics = null;
			Bitmap?.Dispose();
			Bitmap = null;
		}

		private void Initialize(Image card)
		{
			if(Bitmap != null || Graphics != null)
				throw new InvalidOperationException("Cannot be initialized again");

			CurrentPosition = Point.Empty;
			CardSize = card.Size; // 300x420
			var finalSize = new Size(SheetSize.Width * CardSize.Width, SheetSize.Height * CardSize.Height);

			Bitmap = new Bitmap(card, finalSize);
			Graphics = Graphics.FromImage(Bitmap);

			Graphics.Clear(Color.Black);
		}

		private void AdvancePosition()
		{
			CurrentPosition.X++;

			if(CurrentPosition.X >= SheetSize.Width)
			{
				CurrentPosition.X = 0;
				CurrentPosition.Y++;
			}
		}

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

		public void Save(string path)
		{
			Graphics.Flush();
			Bitmap.Save(path);
		}
	}
}
