using Microsoft.VisualStudio.TestTools.UnitTesting;

using Image = Connexions.Travel.Capi.Hotel.SearchResultsResponse.Hotel.Image;

namespace Connexions.Travel.Commands.Hotel
{
	using Capi.Hotel;

	[TestClass]
	public class ImageComparerTests
	{
		[TestMethod]
		public void ImageComparer_Basics()
		{
			var exteriorSmall = new Image
			{
				imageCaption = "Exterior",
				width = 100,
				height = 100,
			};
			var exteriorMedium = new Image
			{
				imageCaption = "Exterior",
				width = 350,
				height = 233,
			};
			var exteriorLarge = new Image
			{
				imageCaption = "Exterior",
				width = 1000,
				height = 1000,
			};
			var bathroom = new Image
			{
				imageCaption = "Bathroom",
				width = 2500,
				height = 2500,
			};

			exteriorSmall.IsEqualTo(exteriorSmall);
			exteriorMedium.IsEqualTo(exteriorMedium);
			exteriorLarge.IsEqualTo(exteriorLarge);
			bathroom.IsEqualTo(bathroom);

			exteriorLarge.IsLessThan(exteriorSmall);
			exteriorLarge.IsLessThan(exteriorMedium);
			exteriorMedium.IsLessThan(exteriorSmall);
			exteriorMedium.IsGreaterThan(exteriorLarge);
			exteriorSmall.IsGreaterThan(exteriorLarge);
			exteriorSmall.IsGreaterThan(exteriorMedium);
			exteriorSmall.IsLessThan(bathroom);
			bathroom.IsGreaterThan(exteriorLarge);
			bathroom.IsGreaterThan(exteriorMedium);
			bathroom.IsGreaterThan(exteriorSmall);

			var nullCaption = new Image
			{
				imageCaption = null,
				width = 3000,
				height = 3000,
			};

			nullCaption.IsGreaterThan(exteriorSmall);
			nullCaption.IsGreaterThan(exteriorLarge);
		}
	}

	static class ImageComparerTestsExtensions
	{
		public static void IsLessThan(this Image x, Image y)
		{
			Assert.IsTrue(ImageComparer.Compare(x, y) < 0, x.ToString() + " < " + y.ToString());
		}

		public static void IsGreaterThan(this Image x, Image y)
		{
			Assert.IsTrue(ImageComparer.Compare(x, y) > 0, x.ToString() + " > " + y.ToString());
		}

		public static void IsEqualTo(this Image x, Image y)
		{
			Assert.AreEqual(0, ImageComparer.Compare(x, y), x.ToString() + " = " + y.ToString());
		}
	}
}