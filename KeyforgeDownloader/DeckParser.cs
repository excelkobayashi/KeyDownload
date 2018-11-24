using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace KeyforgeDownloader
{
	/// <summary>
	///		Parses deck JSON and builds a <see cref="Deck"/>
	/// </summary>
	class DeckParser
	{
		/// <summary>Event fired to monitor the progress of downloading and parsing the deck</summary>
		public event Action<Progress> OnProgressUpdated;

		/// <summary>Source deck id (GUID)</summary>
		private readonly string Id;

		/// <summary>URL where deck JSON can be found</summary>
		/// <remarks>{0} - deck id</remarks>
		private const string BaseUrl = "https://www.keyforgegame.com/api/decks/{0}/?links=cards";

		/// <summary>Source deck JSON URL</summary>
		private string DeckUrl => String.Format(BaseUrl, Id);

		/// <summary>
		///		Constructor
		/// </summary>
		/// <param name="id">Deck id</param>
		public DeckParser(string id) => Id = id;

		/// <summary>
		///		Parse the deck synchronously
		/// </summary>
		/// <returns>Deck with all card images</returns>
		public Deck Parse()
		{
			Task<Deck> task = ParseAsync();
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

		/// <summary>
		///		Parse the deck asynchronously
		/// </summary>
		/// <returns>Deck with all card images</returns>
		public async Task<Deck> ParseAsync()
		{
			UpdateProgress("Connecting");
			string json = await GetString(DeckUrl);

			UpdateProgress("Parsing deck data");
			string[] imageUrls = FindImageUrls(json);

			Deck deck = await BuildDeck(imageUrls);
			return deck;
		}

		/// <summary>
		///		Find all card image URLs
		/// </summary>
		/// <param name="json">Source deck JSON data</param>
		/// <returns>An array of card URLs to card images</returns>
		private string[] FindImageUrls(string json)
		{
			// Using Microsoft Web Helpers
			dynamic data = Json.Decode(json);

			try
			{
				DynamicJsonArray cardDefinitions = data._linked.cards;

				var cardImageLookup = cardDefinitions.Cast<dynamic>().ToDictionary<dynamic, string, string>(card => card.id, card => card.front_image);

				DynamicJsonArray cardsObj = data.data._links.cards;
				IEnumerable<string> cards = cardsObj.Select(id => id.ToString());

				var imageUrls = cards.Select(id => cardImageLookup[id]).ToArray();
				return imageUrls;
			}
			catch(Exception ex)
			{
				throw new FormatException("Invalid JSON format", ex);
			}
		}


		/*private string[] FindImageUrls(string json)
		{
			// Using JSON.net
			var settings = new JsonLoadSettings()
			{
				CommentHandling = CommentHandling.Ignore,
				LineInfoHandling = LineInfoHandling.Ignore
			};

			var data = JObject.Parse(json, settings);

			JToken cardDefinitions = data["_linked"]["cards"];
			var cardImageLookup = cardDefinitions.ToDictionary(card => card["id"].ToString(), card => card["front_image"].ToString());

			IEnumerable<string> deckCards = data["data"]["_links"]["cards"].Select(c => c.ToString()).OrderBy(c => c);
			var imageUrls = deckCards.Select(card => cardImageLookup[card]).ToArray();
			return imageUrls;
		}*/

		/// <summary>
		///		Construct the deck from card images
		/// </summary>
		/// <param name="imageUrls">Array of card image URLs</param>
		/// <returns>Deck with all card images</returns>
		private async Task<Deck> BuildDeck(string[] imageUrls)
		{
			int count = imageUrls.Length;
			var progress = new Progress("Building deck", count);
			UpdateProgress(progress);

			var deck = new Deck(count);

			string lastUrl = null;
			Image card = null;

			foreach(string imageUrl in imageUrls)
			{
				if(lastUrl != imageUrl)
					card = await GetImage(imageUrl);

				deck.AddCard(card);

				progress.Current++;
				UpdateProgress(progress);
			}

			UpdateProgress("Saving");
			return deck;
		}

		/// <summary>
		///		Download an image
		/// </summary>
		/// <param name="url">Image URL</param>
		/// <returns>The processed image</returns>
		private static async Task<Image> GetImage(string url)
		{
			using(var client = new WebClient())
			{
				Stream data = await client.OpenReadTaskAsync(url);
				var image = Image.FromStream(data);
				return image;
			}
		}

		/// <summary>
		///		Download a string
		/// </summary>
		/// <param name="url">String URL</param>
		/// <returns>The downloaded string in UTF-8 format</returns>
		private static async Task<string> GetString(string url)
		{
			using(var client = new WebClient())
			{
				client.Encoding = Encoding.UTF8;
				return await client.DownloadStringTaskAsync(url);
			}
		}

		/// <summary>
		///		Invoke the <see cref="OnProgressUpdated"/> event
		/// </summary>
		/// <param name="progress">Current progress</param>
		private void UpdateProgress(Progress progress) => OnProgressUpdated?.Invoke(progress);

		/// <summary>
		///		Invoke the <see cref="OnProgressUpdated"/> event without a task count
		/// </summary>
		/// <param name="message">Status message</param>
		private void UpdateProgress(string message) => OnProgressUpdated?.Invoke(new Progress(message));
	}
}
