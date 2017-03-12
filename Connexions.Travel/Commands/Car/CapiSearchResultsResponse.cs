using System;

namespace Connexions.Travel.Commands.Car
{
	/// <summary>
	/// Contains the full results from a search.
	/// </summary>
	class CapiSearchResultsResponse : CapiBaseResponse
	{
		public string sessionId;
		public string currency;

		public class Policy
		{
			public string type;
			public string text;
		}

		public class CarRental
		{
			public string id;
			public string pickUpLocationId;
			public string dropoffLocationId;
			public string vehicleRefId;
			public string vendorCode;
			public string supplierId;
			public string rateCode;
			public string inventoryType;

			public class Fare
			{
				public string type;
				public bool? guaranteeRequired;
				public bool? depositRequired;

				public class FeeTaxDiscount
				{
					public string code;
					public decimal? amount;
					public string desc;
				}

				public class CalculationBasis
				{
					public string unitType;
					public decimal? quantity;
				}

				public class ChargeCoverageEquipmentBase
				{
					public string desc;
					public decimal? amount;
					public bool? taxInclusive;
					public CalculationBasis calculationBasis;
				}

				public class Charges : ChargeCoverageEquipmentBase
				{
					public string type;
				}

				public class Coverages : Charges
				{
					public decimal? deductibleAmount;
				}

				public class Equipment : ChargeCoverageEquipmentBase
				{
					public string code;
					public bool? guaranteeRequired;
				}

				public class Payables
				{
					public FeeTaxDiscount[] fees;
					public FeeTaxDiscount[] taxes;
					public Charges[] charges;
					public Coverages[] coverages;
					public Equipment[] equipment;
				}

				public class DisplayFare
				{
					public decimal? totalFare;

					public class Breakup : Payables
					{
						public class BaseFare
						{
							public decimal? amount;

							public CalculationBasis calculationBasis;
						}

						public BaseFare baseFare;
						public FeeTaxDiscount[] discounts;
					}

					public Breakup breakup;
				}

				public DisplayFare displayFare;

				public Payables optionalCharges;
			}

			public Fare fare;

			public Policy[] policies;

			public class CancellationPolicy
			{
				public class Window
				{
					public DateTimeOffset? start;
					public DateTimeOffset? end;
				}

				public Window window;
				public decimal? penaltyAmount;
				public string text;
			}

			public CancellationPolicy[] cancellationPolicies;

			public class Mileage
			{
				public bool? isUnlimited;

				public class Allowed
				{
					public class Distance
					{
						public string unit;
						public decimal? value;
					}

					public Distance distance;
					public string durationUnit;
				}

				public Allowed[] allowed;
			}

			public Mileage mileage;
		}

		public CarRental[] carRentals;

		public class Vendor
		{
			public string code;
			public string name;
			public string logo;
			public Policy[] policies;
		}

		public Vendor[] vendors;

		public class RentalLocation
		{
			public string id;
			public string code;
			public string name;
			public bool? inTerminal;
			public bool? atAirport;
			public string shuttle;

			public class HoursOfOperation
			{
				public string dayOfWeek;

				public class WorkingHours
				{
					public string openTime;
					public string closeTime;
				}

				public WorkingHours[] workingHours;
			}

			public HoursOfOperation[] hoursOfOperation;

			public class ContactInfo
			{
				public class Address
				{
					public string line1;
					public string line2;

					public class CodeName
					{
						public string code;
						public string name;
					}

					public CodeName city;
					public CodeName state;

					public string countryCode;
					public string postalCode;
				}

				public string email;

				public class Phone
				{
					public string type;
					public string num;
					public string countryCode;
					public string ext;
				}

				public Phone[] phones;
			}
		}

		public RentalLocation[] rentalLocations;

		public class Vehicle
		{
			public string sippCode;
			public string refId;
			public string name;
			public string category;
			public string type;
			public string transmission;
			public string desc;
			public string[] images;
			public bool? airConditioned;
			public string fuelType;
			public string baggageCapacity;
			public string passengerCapacity;
			public string doorCount;
			public string driveType;
			public Policy[] policies;

			public class SpecialEquipment
			{
				public string type;
				public string desc;
			}

			public SpecialEquipment[] specialEquipment;
		}

		public Vehicle[] vehicles;
	}
}