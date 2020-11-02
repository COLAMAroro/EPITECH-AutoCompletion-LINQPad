<Query Kind="Program">
  <Namespace>Address</Namespace>
  <Namespace>System.Globalization</Namespace>
</Query>

namespace autoCompletion
{
    class Program
	{
		private static readonly string help_string =
		"USAGE\n" +
		"\t./autoCompletion dictionary\n" +
		"\n" +
		"DESCRIPTION\n" +
		"\tdictionary\tfile containing one address per line, as knowledge base";
		static int Main(string[] args)
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
				new AddressInfo("Lille, 90 rue d'Arras"),
				new AddressInfo("Lille, 76 impasse Georges Pompidou"),
				new AddressInfo("Lyon, 2 allée des fleur")
			};
			List<AddressInfo> result;
			try
			{
				result = matcher.matchAddress(addresses);
			}
			catch (AbortException _)
			{
				return 0;
			}
			if (result != null)
			{
				result.Dump();
				return 0;
			}
			return 84;
		}
	}
}

namespace autoCompletion
{
    [System.Serializable]
    public class NoMatchException : System.Exception
    {
        public NoMatchException() { }
        public NoMatchException(string message) : base(message) { }
        public NoMatchException(string message, System.Exception inner) : base(message, inner) { }
        protected NoMatchException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [System.Serializable]
    public class SyntaxException : System.Exception
    {
        public SyntaxException() { }
        public SyntaxException(string message) : base(message) { }
        public SyntaxException(string message, System.Exception inner) : base(message, inner) { }
        protected SyntaxException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [System.Serializable]
    public class AbortException : System.Exception
    {
        public AbortException() { }
        public AbortException(string message) : base(message) { }
        public AbortException(string message, System.Exception inner) : base(message, inner) { }
        protected AbortException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    static class matcher
	{
		private static string[] stringSorter(string[] input)
		{
			Array.Sort(input, StringComparer.Create(new CultureInfo("fr-FR"), true));
			return input;
		}

		private static string capitalize(string str)
		{
			if (str.Length == 0)
				return str;
			else if (str.Length == 1)
				return (str.ToUpper());
			else
				return (char.ToUpper(str[0]) + str.Substring(1));
		}

		private static List<AddressInfo> getDeduplictedCities(List<AddressInfo> addresses)
        {
            List<AddressInfo> distinct = new List<AddressInfo>();
            foreach (var addr in addresses)
            {
                bool is_into = false;
                foreach (var item in distinct.Select(x => x.City))
                {
                    is_into = addr.City.SequenceEqual(item);
                    if (is_into)
                        break;
                }
                if (is_into)
                    continue;
                distinct.Add(addr);
            }
            return distinct;
        }
		
        private static bool getIfDuplicatedCities(List<AddressInfo> addresses)
        {
            var distinct = getDeduplictedCities(addresses);
            return (distinct.Count() > 1);
        }
		
        private static Dictionary<char, int> getCityDictionnary(List<AddressInfo> addresses, string current_content)
        {
            current_content = current_content.ToUpper(new CultureInfo("fr-FR"));
            Dictionary<char, int> result_dict = new Dictionary<char, int>();
            foreach (AddressInfo address in addresses)
            {
                foreach (var x in address.City.Where(x => x.Length > current_content.Length).Where(x => x.ToUpper().StartsWith(current_content)).Select(x => x[current_content.Length]).Distinct())
                {
                    if (result_dict.ContainsKey(x))
                        result_dict[x] += 1;
                    else
                        result_dict[x] = 1;
                }
            }
            return result_dict;
        }

        private static void printCityRecommendation(Dictionary<char, int> result_dict, string current_content)
        {
            Console.WriteLine(String.Join(' ', result_dict.
                                OrderByDescending(x => x.Value).
                                ThenBy(x => x.Key).
                                Select(x => x.Key).
                                Take(5).
                                Select(x => "{" + current_content.ToUpper() + x + "}")));
        }

        private static string[] getCityFormat(AddressInfo addr, string city)
        {
            return addr.PrintCity.
                    Select(x => x.ToUpper().StartsWith(city.ToUpper()) ? x.ToUpper() : x.ToLower()).
                    ToArray();
        }

		private static bool isCityPrintIdentical(string[] city, string[] format)
		{
			if (city.Length != format.Length)
				return false;
			for (int i = 0; i < city.Length; i += 1)
			{
				if (city[i] != format[i])
					return false;
			}
			return true;
		}

        private static (char, List<AddressInfo>) printAndGetCitySelection(List<AddressInfo> addresses, string city)
        {
			List<AddressInfo> deduplicated = getDeduplictedCities(addresses);
			string[] pre_sorted = stringSorter(deduplicated.Select(x => String.Join(' ', getCityFormat(x, city))).ToArray());
			List<string[]> formated = pre_sorted.
										Select(x => x.Split()).
										ToList();
			List<string> print_formated = new List<string>();
			for (int i = 0; i < formated.Count; i += 1)
                print_formated.Add($"{{{i + 1} : {String.Join(' ', formated[i])}}}");
            Console.WriteLine(String.Join(' ', print_formated));
            string input = Console.ReadLine();
            int parsed_int;
			if (input.Length == 1 && Char.IsLetter(input[0]))
				return (input[0], null);
            if (!int.TryParse(input, out parsed_int))
                throw new SyntaxException();
            if (parsed_int < 1 || parsed_int > formated.Count)
                throw new SyntaxException();
            List<AddressInfo> result = addresses.Where(x=>isCityPrintIdentical(getCityFormat(x, city), formated[parsed_int - 1])).ToList();
            return ('-', result);
        }

        private static (string[], List<AddressInfo>) parse_city(List<AddressInfo> addresses)
        {
            string city = "";
            while (getIfDuplicatedCities(addresses))
			{
				char c = '-';
				List<AddressInfo> tmpaddr = addresses;
				if (addresses.Any(x =>
				   x.City.Any(y => y.ToUpper() == city.ToUpper())
				))
				{
					(c, tmpaddr) = printAndGetCitySelection(addresses, city);
					if (tmpaddr != null)
					{
						addresses = tmpaddr;
						break;
					}
					else
					{
						city += c.ToString().ToLower()[0];
						addresses = addresses.
											Where(x => x.City.Any
												(y => y.StartsWith(city)
											)).ToList();
						continue;
					}
				}
                var cities = getCityDictionnary(addresses, city);
                if (cities.Count == 1)
                {
                    city += cities.Keys.ToArray()[0];
                    continue;
                }
                printCityRecommendation(cities, city);
                string line = Console.ReadLine();
                if (line == "ABORT")
                    throw new AbortException();
                if (line.Length != 1)
                    throw new SyntaxException();
                city += line.ToLower()[0];
                addresses = addresses.
                                    Where(x => x.City.Any
                                        (y => y.StartsWith(city)
                                    )).ToList();
            }
            if (addresses.Count() == 0)
                throw new NoMatchException();
            var print_format = getCityFormat(addresses[0], city);
            return (print_format, addresses);
        }

/*===========================================================================================================================*/

		private static List<AddressInfo> getDeduplictedStreets(List<AddressInfo> addresses)
		{
			List<AddressInfo> distinct = new List<AddressInfo>();
			foreach (var addr in addresses)
			{
				bool is_into = false;
				foreach (var item in distinct.Select(x => x.Streetname))
				{
					is_into = addr.Streetname.SequenceEqual(item);
					if (is_into)
						break;
				}
				if (is_into)
					continue;
				distinct.Add(addr);
			}
			return distinct;
		}
		
		private static bool getIfDuplicatedStreets(List<AddressInfo> addresses)
		{
			var distinct = getDeduplictedStreets(addresses);
			return (distinct.Count() > 1);
		}
		
		private static bool getIfIdenticalStreets(List<AddressInfo> addresses)
		{
			for (int i = 0; i < addresses.Count - 1; i += 1) {
				if (addresses[i].Streetname.Length != addresses[i+1].Streetname.Length)
					return false;
				if (addresses[i].type != addresses[i+1].type)
					return false;
				for (int j = 0; j < addresses[i].Streetname.Length; j += 1) {
					if (addresses[i].Streetname[j] != addresses[i+1].Streetname[j])
						return false;
				}
			}
			return true;
		}

		private static Dictionary<char, int> getStreetDictionnary(List<AddressInfo> addresses, string current_content)
		{
			current_content = current_content.ToUpper(new CultureInfo("fr-FR"));
			Dictionary<char, int> result_dict = new Dictionary<char, int>();
			foreach (AddressInfo address in addresses)
			{
				foreach (var x in address.Streetname.Where(x => x.Length > current_content.Length).Where(x => x.ToUpper().StartsWith(current_content)).Select(x => x[current_content.Length]).Distinct())
				{
					if (result_dict.ContainsKey(x))
						result_dict[x] += 1;
					else
						result_dict[x] = 1;
				}
			}
			return result_dict;
		}

		private static void printStreetRecommendation(Dictionary<char, int> result_dict, string current_content, string[] city)
		{
			Console.WriteLine(String.Join(' ', result_dict.
								OrderByDescending(x => x.Value).
								ThenBy(x => x.Key).
								Select(x => x.Key).
								Take(5).
								Select(x => "{" + String.Join(' ', city) + ", " + current_content.ToUpper() + x + "}")));
		}

		private static string[] getStreetFormat(AddressInfo addr, string street="", bool final=false)
		{
			if (final)
			{
				List<String> copy = new List<string>();
				for (int i = 0; i < addr.Streetname.Length; i += 1) {
					if (addr.Streetname[i].ToLower().StartsWith(street.ToLower()))
						copy.Add(addr.PrintStreet[i].ToUpper());
					else
						copy.Add(addr.PrintStreet[i]);
				}
				return copy.ToArray();
			}
			return addr.PrintStreet.
					Select(x => x.ToUpper().StartsWith(street.ToUpper()) ? x.ToUpper() : x.ToLower()).
					ToArray();
		}

		private static bool isStreetPrintIdentical(string[] street, string[] format)
		{
			if (street.Length != format.Length)
				return false;
			for (int i = 0; i < street.Length; i += 1)
			{
				if (street[i] != format[i])
					return false;
			}
			return true;
		}

		private static (char, List<AddressInfo>) printAndGetStreetSelection(List<AddressInfo> addresses, string street, string[] city)
		{
			addresses = addresses.
							OrderBy(x=>x.number).
							ThenBy(x=>x.type.ToString()).
							ThenBy(x=>String.Join(' ', x.Streetname).ToLower()).
							ToList();
			List<string> print_formated = new List<string>();
			for (int i = 0; i < addresses.Count; i += 1)
				print_formated.Add($"{{{i + 1} : {String.Join(' ', city)}, {addresses[i].number} {addresses[i].type} {String.Join(' ', getStreetFormat(addresses[i], street))}}}");
			Console.WriteLine(String.Join(' ', print_formated));
			string input = Console.ReadLine();
			int parsed_int;
			if (input.Length == 1 && Char.IsLetter(input[0]))
				return (input[0], null);
			if (!int.TryParse(input, out parsed_int))
				throw new SyntaxException();
			if (parsed_int < 1 || parsed_int > addresses.Count)
				throw new SyntaxException();
			return ('-', new List<AddressInfo> { addresses[parsed_int - 1]});
		}

		private static List<AddressInfo> printAndGetSameStreetNumber(List<AddressInfo> addresses, string street, string[] city)
		{
			addresses = addresses.OrderBy(x => x.number).ToList();
			List<string> print_formated = new List<string>();
			for (int i = 0; i < addresses.Count; i += 1)
				print_formated.Add($"{{{i + 1} : {String.Join(' ', addresses[i].PrintCity).ToUpper()}, {addresses[i].number} {addresses[i].type.ToString().ToUpper()} {String.Join(' ', addresses[i].PrintStreet).ToUpper()}}}");
			Console.WriteLine(String.Join(' ', print_formated));
			string input = Console.ReadLine();
			int parsed_int;
			if (!int.TryParse(input, out parsed_int))
				throw new SyntaxException();
			if (parsed_int < 1 || parsed_int > addresses.Count)
				throw new SyntaxException();
			return new List<AddressInfo> { addresses[parsed_int-1] };
		}

		private static List<AddressInfo> printAndGetSimilarStreet(List<AddressInfo> addresses, string street, string[] city)
        {
            addresses = addresses.OrderBy(x => x.type.ToString() + String.Join(' ', x.Streetname).ToUpper(new CultureInfo("fr-FR"))).ToList();
            List<string> print_formated = new List<string>();
            for (int i = 0; i < addresses.Count; i += 1)
                print_formated.Add($"{{{i + 1} : {String.Join(' ', city)}, {addresses[i].number} {addresses[i].type} {String.Join(' ', getStreetFormat(addresses[i], street, true))}}}");
            Console.WriteLine(String.Join(' ', print_formated));
            string input = Console.ReadLine();
            int parsed_int;
            if (!int.TryParse(input, out parsed_int))
				throw new SyntaxException();
			if (parsed_int < 1 || parsed_int > addresses.Count)
				throw new SyntaxException();
			return new List<AddressInfo> { addresses[parsed_int - 1] };
		}

		private static List<AddressInfo> parse_street(List<AddressInfo> addresses, string[] city)
		{
			string street = "";
			while (getIfDuplicatedStreets(addresses))
			{
				char c = '-';
				List<AddressInfo> tmpaddr = addresses;
				if (addresses.Any(x =>
				   x.Streetname.Any(y => y.ToUpper() == street.ToUpper())
				))
				{
					(c, tmpaddr) = printAndGetStreetSelection(addresses, street, city);
					if (tmpaddr != null) {
						addresses = tmpaddr;
						break;
					}
					else
					{
						street += c.ToString().ToLower()[0];
						addresses = addresses.
											Where(x => x.Streetname.Any
												(y => y.StartsWith(street)
											)).ToList();
						continue;
					}
				}
				var streets = getStreetDictionnary(addresses, street);
				if (streets.Count == 1)
				{
					street += streets.Keys.ToArray()[0];
					continue;
				}
				printStreetRecommendation(streets, street, city);
				string line = Console.ReadLine();
				if (line == "ABORT")
					throw new AbortException();
				if (line.Length != 1)
					throw new SyntaxException();
				street += line.ToLower()[0];
				addresses = addresses.
									Where(x => x.Streetname.Any
										(y => y.StartsWith(street)
									)).ToList();
			}
			if (addresses.Count() == 0)
				throw new NoMatchException();
			if (addresses.Count() > 1)
				addresses = getIfIdenticalStreets(addresses) ? printAndGetSameStreetNumber(addresses, street, city) : printAndGetSimilarStreet(addresses, street, city);
			return (addresses);
		}

/*===========================================================================================================================*/

		public static List<AddressInfo> matchAddress(List<AddressInfo> addresses)
		{
			try
			{
				string[] city;
                (city, addresses) = parse_city(addresses);
				addresses = parse_street(addresses, city);
                return (addresses);
            }
            catch (NoMatchException _)
            {
                Console.Error.WriteLine("Unkown address");
                return null;
            }
            catch (SyntaxException _)
            {
                return null;
            }
        }
    }
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
			string[] printparts = s.Split(',');
			InputName = s;
			string[] parts = s.Split(',');
			if (parts.Length != 2)
				throw new WrongAddress(s);
			PrintCity = (parts[0].ToLower(new CultureInfo("fr-FR"))).Split();
			string precision = parts[1].Trim().ToLower((new CultureInfo("fr-FR")));
			string[] split = precision.Split();
			if (split.Length <= 2)
				throw new WrongAddress(s);
			int number;
			if (!int.TryParse(split[0], out number))
				throw new WrongAddress(s);
			this.number = number;
			StreetType type;
			if (!Enum.TryParse(split[1], out type))
				throw new WrongAddress(s);
			this.type = type;
			PrintStreet = split.Skip(2).ToArray();
			City = (string[])PrintCity.Clone();
			for (int i = 0; i < City.Length; i += 1)
				City[i] = City[i].Replace("'", "").Replace("-", "");
			Streetname = (string[])PrintStreet.Clone();
			for (int i = 0; i < Streetname.Length; i += 1)
				Streetname[i] = Streetname[i].Replace("'", "").Replace("-", "");
		}
		public string InputName { get; }
		public string[] City { get; }
		public string[] PrintCity { get; }
		public int number { get; }
		public StreetType type { get; }
		public string[] Streetname { get; }
		public string[] PrintStreet { get; }

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