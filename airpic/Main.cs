using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

class AirPic {

	static List<string> files = new List<string> ();
	static Queue<string> dirs = new Queue<string> ();
	static Random rnd = new Random ();

	static int FindJpeg (string dir)
	{
		files.Clear ();
		dirs.Enqueue (dir);
		while (dirs.Count > 0) {
			dir = dirs.Dequeue ();
			foreach (string subdir in Directory.GetDirectories (dir))
				dirs.Enqueue (subdir);
			foreach (string file in Directory.GetFiles (dir, "*.jpg"))
				files.Add (file);
		}
		int c = files.Count;
		Console.WriteLine ("{0} fichiers .jpg", c);
		return c;
	}

	static void PushImage (string file)
	{
		Console.WriteLine (file);
		using (WebClient wc = new WebClient ()) {
			wc.Headers.Add ("X-Apple-AssetKey", "F92F9B91-954E-4D63-BB9A-EEC771ADE6E8");
			wc.Headers.Add ("User-Agent", "AirPlay/160.4 (Photos)");
			wc.Headers.Add ("X-Apple-Session-ID", "1bd6ceeb-fffd-456c-a09c-996053a7a08c");
			var data = File.ReadAllBytes (file);
			// FIXME: change address to match your device
			wc.UploadData ("http://your.apple.tv:7000/photo", "PUT", data);
		}
	}

	static void Help (string message = null)
	{
		if (message != null) {
			Console.WriteLine (message);
			Console.WriteLine ();
		}
		Console.WriteLine ("Usage: airpic delai repertoire");
		Console.WriteLine ("  delai        Nombre, en secondes, entre les images");
		Console.WriteLine ("  repertoire   Emplacement des images (extension .jpg)");
		Environment.Exit (1);
	}

	public static void Main (string [] args)
	{
		if (args.Length < 2)
			Help ();

		int delay = -1;
		if (!Int32.TryParse (args [0], out delay))
			Help ("ERREUR: Delai invalide.");

		if (!Directory.Exists (args [1]))
			Help ("ERREUR: Repertoire inexistant.");

		while (true) {
			string file;
			while (true) {
				int c = files.Count;
				if (c == 0)
					c = FindJpeg (args [1]);

				int n = rnd.Next (0, c);
				file = files [n];
				files.Remove (file);
				// file could have been [re]moved
				if (File.Exists (file))
					break;
			}
			PushImage (file);
			Thread.Sleep (delay * 1000);
		}
	}
}