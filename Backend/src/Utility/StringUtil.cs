using System.Text;

public static class StringUtil
{
	public static string ToSnakeCase(string name)
	{
		if (string.IsNullOrEmpty(name))
			return name;

		var builder = new StringBuilder();

		for (int i = 0; i < name.Length; ++i)
		{
			char rune = name[i];

			if (char.IsUpper(rune) && i > 0)
			{
				builder.Append("_");
				builder.Append(char.ToLower(rune));
			}
			else
				builder.Append(rune);
		}

		return builder.ToString();
	}
}