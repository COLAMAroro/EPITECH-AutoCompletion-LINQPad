<Query Kind="Program">
  <Namespace>Address</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>autoCompletion</Namespace>
</Query>

void Main()
{
	List<AddressInfo> addresses = new List<AddressInfo>{
		new AddressInfo("Paris, 458 boulevard Saint-Germain"),
		new AddressInfo("Paris, 343 boulevard Saint-Germain"),
		new AddressInfo("Marseille, 343 boulevard Camille Flammarion"),
		new AddressInfo("Marseille, 29 rue Camille Desmoulins"),
		new AddressInfo("Marseille, 1 chemin des Aubagnens"),
		new AddressInfo("Paris, 12 rue des singes"),
		new AddressInfo("Paris, 34 quai VoLtAiRe"),
		new AddressInfo("Paris, 34 rue Voltaire"),
		new AddressInfo("Lille, 120 boulevard Victor Hugo"),
		new AddressInfo("Marseille, 50 rue Voltaire"),
		new AddressInfo("Toulouse, 90 rue Voltaire"),
		new AddressInfo("Marseille, 78 boulevard de la libération"),
		new AddressInfo("Lille, 30 rue Victor Danel"),
		new AddressInfo("Mont Saint Martin, 42 rue de Lyon"),
		new AddressInfo("Mont de Marsan, 100 avenue Pierre de Coubertin"),
		new AddressInfo("Strasbourg, 391 boulevard de Nancy"),
		new AddressInfo("Lyon, 56 rue du Docteur Albéric Pont"),
		new AddressInfo("Lille, 90 rue d’Arras"),
		new AddressInfo("Lille, 76 impasse Georges Pompidou"),
		new AddressInfo("Lyon, 2 allée des fleur")
	};
	string city = "";
	while (matcher.getIfDuplicatedCities(addresses))
    {
        string line = Console.ReadLine();
        if (line == "ABORT")
			throw new Exception();
		if (line.Length != 1)
			throw new Exception();
		city += line.ToLower()[0];
		addresses = addresses.
							Where(x => x.City.Any(y => y.StartsWith(city))).
							Select(x => x).ToList();
	}
	addresses.ToList().Dump();
}

namespace Address
{
	[System.Serializable]
	public class WrongAddress : Exception
	{
		public WrongAddress() { }
		public WrongAddress(string message) : base(message) { }
		public WrongAddress(string message, System.Exception inner) : base(message, inner) { }
		protected WrongAddress(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	public enum StreetType
	{
		allée,
		avenue,
		boulevard,
		chemin,
		impasse,
		place,
		quai,
		rue,
		square
	}

	public class AddressInfo
    {
        public AddressInfo(string s)
        {
            string[] parts = s.Split(',');
            if (parts.Length != 2)
                throw new WrongAddress(s);
            City = (parts[0].ToLower(new CultureInfo("fr-FR"))).Split();
            string precision = parts[1].Trim().ToLower((new CultureInfo("fr-FR")));
            string[] split = precision.Split();
            if (split.Length <= 2)
                throw new WrongAddress(s);
            if (!int.TryParse(split[0], out number))
                throw new WrongAddress(s);
            if (!Enum.TryParse(split[1], out type))
                throw new WrongAddress(s);
            Streetname = split.Skip(2).ToArray();
			InputName = s;
        }
        public string InputName { get; }
        public string[] City { get; }
        private int number;
        private StreetType type;
        public string[] Streetname { get; }

        public override string ToString()
        {
			StringBuilder builder = new StringBuilder("Address\n");
			builder.Append($"City -> {City}\n");
			builder.Append($"Number -> {number}\n");
			builder.Append($"Type -> {type}\n");
			string newname = String.Join(' ', Streetname);
			builder.Append($"Streetname -> {newname}\n");
			return builder.ToString();
		}
	}
}

namespace autoCompletion
{
	static class matcher
	{
		static public bool getIfDuplicatedCities(List<AddressInfo> addresses)
		{
			List<string[]> distinct = new List<string[]>();
			foreach (var addr in addresses) {
				bool is_into = false;
				foreach (var item in distinct) {
					is_into = addr.City.SequenceEqual(item);
					if (is_into)
						break;
				}
				if (is_into)
					continue;
				distinct.Add(addr.City);
			}
			return (distinct.Count() != 1);
		}

		private static void printBestCityLetters(List<AddressInfo> addresses)
		{
			Dictionary<char, int> result_dict = new Dictionary<char, int>();
			foreach (AddressInfo address in addresses)
			{
				var s = (from addr in address.City select addr[0]).Distinct();
				foreach (var x in s)
				{
					if (result_dict.ContainsKey(x))
						result_dict[x] += 1;
					else
						result_dict[x] = 1;
				}
			}
			var best_values = result_dict.OrderByDescending(x => x.Value).ThenBy(x => x.Key).Select(x => x.Key).Take(5).Select(x => "{" + x + "}");
			Console.WriteLine(String.Join(' ', best_values));
		}
		public static AddressInfo matchAddress(List<AddressInfo> addresses)
		{
			printBestCityLetters(addresses);
			return null;
		}
	}
}