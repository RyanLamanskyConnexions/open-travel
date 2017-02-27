using System;
using System.Collections.Generic;

namespace Connexions.OpenTravel.UserInterface.Commands.Hotel
{
	/// <summary>
	/// Compares CAPI hotel images for quality based on their caption and dimensions.
	/// </summary>
	sealed class ImageComparer : IComparer<CapiSearchResultsResponse.Hotel.Image>
	{
		/// <summary>
		/// Image categories moved to the start of the set.
		/// </summary>
		static readonly ImageRanker preferredImages = new ImageRanker
		{
			"Featured Image",
			"Exterior",
			"Exterior view",
			"Hotel Front",
			"Hotel Entrance",
			"Outdoor Pool",
		};

		/// <summary>
		/// Image categories moved to the end of the set.  Unknown are between this and <see cref="preferredImages"/>.
		/// </summary>
		static readonly ImageRanker avoidImages = new ImageRanker
		{
			"Bathroom",
			"Logo",
		};

		private sealed class ImageRanker : Dictionary<string, int>
		{
			public ImageRanker()
				: base(StringComparer.OrdinalIgnoreCase)
			{
			}

			public void Add(string key)
			{
				Add(key, Count);
			}
		}

		public static readonly ImageComparer Instance = new ImageComparer();

		public static int Compare(CapiSearchResultsResponse.Hotel.Image x, CapiSearchResultsResponse.Hotel.Image y)
		{
			var xScore = Score(x);
			var yScore = Score(y);

			if (xScore != yScore)
				return xScore - yScore;

			return //Bigger image wins tie breaker. 
				(x.width.GetValueOrDefault() * x.height.GetValueOrDefault())
				.CompareTo(y.width.GetValueOrDefault() * y.height.GetValueOrDefault())
				* -1;
		}

		int IComparer<CapiSearchResultsResponse.Hotel.Image>.Compare(CapiSearchResultsResponse.Hotel.Image x, CapiSearchResultsResponse.Hotel.Image y)
			=> Compare(x, y);

		/// <summary>
		/// Calculates a score based on the characteristics of an image.  The lower the score, the better the image.
		/// </summary>
		/// <param name="image">The image to consider.</param>
		/// <returns>A number where lower values (potentially negative) indicates a better image.</returns>
		public static int Score(CapiSearchResultsResponse.Hotel.Image image)
		{
			if (image == null)
				return int.MaxValue;

			var score = 0;
			var width = image.width.GetValueOrDefault(1);
			var height = image.height.GetValueOrDefault(1);
			var basePenalty = preferredImages.Count;

			//Penalize small images.
			if (width < 500)
			{
				score += basePenalty;
				if (width < 250)
					score += basePenalty;
			}
			if (height < 500)
			{
				score += basePenalty;
				if (height < 250)
					score += basePenalty;
			}

			{
				if (string.IsNullOrWhiteSpace(image.imageCaption))
					score += basePenalty * 10; //No way of knowing what this is.
				else if (preferredImages.TryGetValue(image.imageCaption, out var preferredScore))
					score -= preferredImages.Count - preferredScore; //Apply preferred score relative to its ranking.
				else if (avoidImages.TryGetValue(image.imageCaption, out var avoidScore))
					score += (basePenalty * 5) + avoidImages.Count - avoidScore; //Really don't want these unless there is nothing else.
			}

			return score;
		}
	}
}