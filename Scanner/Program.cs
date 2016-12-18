using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Scanner
{
	class Program
	{
		[STAThread]
		public static void Main()
		{
			//Console.WriteLine("// Scanning...");
			//var songs = Scan(directory: @"C:\Users\manue\OneDrive\Musik");


			//Console.WriteLine("// Exporting...");
			//var xdoc = new XDocument(
			//	new XElement("songs",
			//		songs.Select(song => new XElement("song",
			//			song.Artist != null ? new XAttribute("artist", song.Artist) : null,
			//			song.Title != null ? new XAttribute("title", song.Title) : null,
			//			new XText(song.File)
			//		))
			//	)
			//);
			//xdoc.Save("songs.xml");


			Console.WriteLine("// Importing...");
			var songs = XDocument.Load("songs.xml")
				.Descendants("song")
				.Select(d => new Song
				{
					Artist = d.Attribute("artist")?.Value,
					Title = d.Attribute("title")?.Value,
					File = d.Value
				}).ToList();


			//foreach (var song in songs)
			//{
			//	Console.WriteLine($"{song.Artist} - {song.Title}");
			//}
			//Console.WriteLine($"{songs.Count} songs");


			int index = 1;
			foreach (var playlistItem in XDocument.Load("bestof2012.wpl").Descendants("media"))
			{
				var artist = playlistItem.Attribute("trackArtist")?.Value;
				var title = playlistItem.Attribute("trackTitle")?.Value;
				var file = playlistItem.Value;

				var source = playlistItem.Attribute("src")?.Value;
				if (!File.Exists(source))
				{
					var song = songs.Where(s => s.Artist == artist && s.Title == title).SingleOrDefault();

					if (song != null)
					{
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine($"{index.ToString().PadLeft(2)}:\t{string.Concat(playlistItem.ToString().Take(40))} -> {string.Concat(song.File.Reverse().Take(60).Reverse())}");
						Console.ResetColor();
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"{index.ToString().PadLeft(2)}:\t{string.Concat(playlistItem.ToString().Take(40))} -> NO_FILE: {artist} - {title}");
						Console.ResetColor();
					}
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine($"{index.ToString().PadLeft(2)}:\t{string.Concat(playlistItem.ToString().Take(100))}");
					Console.ResetColor();
				}

				index++;
			}
			Console.ReadLine();
		}

		public static List<Song> Scan(string directory)
		{
			var songs = new List<Song>();

			var files = Directory.GetFiles(directory, "*.mp3", SearchOption.AllDirectories);
			foreach (var file in files)
				using (var mp3 = new Id3.Mp3File(file))
				{
					var tag = mp3.GetTag(Id3.Id3TagFamily.FileStartTag);
					songs.Add(new Song
					{
						File = file,
						Artist = tag?.Artists.Value,
						Title = tag?.Title.Value
					});
					Console.Write(".");
				}

			return songs;
		}
	}

	class Song
	{
		public string File { get; set; }
		public string Artist { get; set; }
		public string Title { get; set; }
	}
}
