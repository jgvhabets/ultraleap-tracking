using System.Collections.Generic;

public static class ExtensionMethods {

	private static readonly System.Random rnd = new System.Random();    // instance of the random class to draw random elements, static readonly so that randomness is preserved

	public static void Shuffle<T>(this IList<T> list) {
		int n = list.Count;
		while (n > 1) {
			n--;
			int k = rnd.Next(n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
			}
		}

	}