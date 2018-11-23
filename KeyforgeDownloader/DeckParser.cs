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
	class DeckParser
	{
		public event Action<Progress> OnProgressUpdated;

		private readonly string Id;
		private const string BaseUrl = "https://www.keyforgegame.com/api/decks/{0}/?links=cards";

		private string DeckUrl => String.Format(BaseUrl, Id);

		public DeckParser(string id) => Id = id;

		public async Task<Deck> Parse()
		{
			UpdateProgress("Connecting");

			string json = await GetString(DeckUrl);

			UpdateProgress("Parsing deck data");


			string[] imageUrls = FindImageUrls(json);

			Deck deck = await BuildDeck(imageUrls);
			return deck;
		}

		private string[] FindImageUrls(string json)
		{
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
			catch(MissingMemberException ex)
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

		private async Task<Deck> BuildDeck(string[] imageUrls)
		{
			// Using Microsoft Web Helpers
			var progress = new Progress("Building deck", imageUrls.Length);
			UpdateProgress(progress);

			var deck = new Deck();

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

		private static async Task<Image> GetImage(string url)
		{
			using(var client = new WebClient())
			{
				Stream data = await client.OpenReadTaskAsync(url);
				var image = Image.FromStream(data);
				return image;
			}
		}

		private async Task<string> GetString(string url)
		{
			using(var client = new WebClient())
			{
				client.Encoding = Encoding.UTF8;
				return await client.DownloadStringTaskAsync(url);
			}
		}

		private void UpdateProgress(Progress progress) => OnProgressUpdated?.Invoke(progress);
		private void UpdateProgress(string message) => OnProgressUpdated?.Invoke(new Progress(message));
	}
}
