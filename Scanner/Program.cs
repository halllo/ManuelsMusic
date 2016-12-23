using System;
using System.Collections.Generic;
using System.Drawing;
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
			{//Exporting
				Console.WriteLine("// Scanning...");
				const string musicFolder = @"C:\Users\manue\OneDrive\Musik\";
				var songs = Scan(directory: musicFolder);


				Console.WriteLine("// Exporting...");
				var xdoc = new XDocument(
					new XElement("songs",
						songs.Select(song => new XElement("song",
							song.Artist != null ? new XAttribute("artist", song.Artist) : null,
							song.AlbumArtists != null ? new XAttribute("albumartists", song.AlbumArtists) : null,
							song.Album != null ? new XAttribute("album", song.Album) : null,
							song.AlbumArt != null ? new XAttribute("cover", song.AlbumArt) : null,
							song.Title != null ? new XAttribute("title", song.Title) : null,
							new XText(song.File)
						))
					)
				);
				xdoc.Save("songs.xml");
			}


			{//Importing
				Console.WriteLine("// Importing...");
				var songs = XDocument.Load("songs.xml")
					.Descendants("song")
					.Select(d => new Song
					{
						Artist = d.Attribute("artist")?.Value,
						Album = d.Attribute("album")?.Value,
						AlbumArtists = d.Attribute("albumartists")?.Value,
						AlbumArt = d.Attribute("cover")?.Value,
						Title = d.Attribute("title")?.Value,
						File = d.Value
					}).ToList();


				//foreach (var song in songs)
				//{
				//	Console.WriteLine($"{song.Artist} - {song.Title}");
				//}
				//Console.WriteLine($"{songs.Count} songs");
				//Console.WriteLine($"{songs.GroupBy(s => s.Album).Count()} albums");
				//Console.WriteLine($"{songs.GroupBy(s => s.Artist).Count()} artists");


				var albums = songs.Where(s => !s.File.StartsWith("AudioBooks\\")).GroupBy(s => new { s.AlbumArtists, s.Album, s.AlbumArt });
				foreach (var album in albums)
				{
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.Write($"{album.Key.AlbumArtists} - {album.Key.Album}");
					Console.ForegroundColor = ConsoleColor.DarkGray;
					Console.WriteLine($" ({album.Key.AlbumArt})");
					Console.ResetColor();
					foreach (var song in album)
					{
						Console.Write($"- {song.Title}");
						Console.ForegroundColor = ConsoleColor.DarkGray;
						Console.WriteLine($" ({string.Concat(song.File.Reverse().Take(80).Reverse())})");
						Console.ResetColor();
					}
					Console.WriteLine();
				}
				Console.ReadLine();
			}
		}


		//RepairPlaylist("playlist.wpl", musicFolder, songs);
		static void RepairPlaylist(string playlist, string basePath, List<Song> songs)
		{
			Console.WriteLine(playlist);
			int index = 1;

			var xDocument = XDocument.Load(playlist);
			foreach (var playlistItem in xDocument.Descendants("media"))
			{
				var artist = playlistItem.Attribute("trackArtist")?.Value;
				var title = playlistItem.Attribute("trackTitle")?.Value;
				var file = playlistItem.Value;

				var source = playlistItem.Attribute("src")?.Value;
				if (!File.Exists(basePath + source))
				{
					var song = songs.Where(s => s.Artist == artist && s.Title == title).ToList();

					if (song.Count == 1)
					{
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine($"{index.ToString().PadLeft(2)}:\t{string.Concat(playlistItem.ToString().Take(40))} -> {string.Concat(song[0].File.Reverse().Take(60).Reverse())}");
						Console.ResetColor();
						playlistItem.Attribute("src").Value = song[0].File.Replace(basePath, "");
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"{index.ToString().PadLeft(2)}:\t{string.Concat(playlistItem.ToString().Take(40))} -> ? ({artist} - {title})");
						Console.ResetColor();
					}
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine($"{index.ToString().PadLeft(2)}:\t{string.Concat(playlistItem.ToString().Take(100))}");
					Console.ResetColor();
					playlistItem.Attribute("src").Value = playlistItem.Attribute("src").Value.Replace(basePath, "");
				}

				index++;
			}
			xDocument.Save(playlist);
		}


		static List<Song> Scan(string directory)
		{
			var songs = new List<Song>();
			var albumart = new Dictionary<string, string>();

			var files = Directory.GetFiles(directory, "*.mp3", SearchOption.AllDirectories);
			foreach (var file in files)
				using (var mp3Id3 = new Id3.Mp3File(file))
				using (var mp3tagLib = TagLib.File.Create(file))
				{
					var tag = mp3Id3.GetTag(Id3.Id3TagFamily.FileStartTag);
					var artist = tag?.Artists.Value;
					var albumArtists = string.Join(", ", mp3tagLib.Tag.AlbumArtists);
					var album = tag?.Album.Value ?? string.Empty;
					var title = tag?.Title.Value;

					if (!albumart.ContainsKey(album) && mp3tagLib.Tag.Pictures.Any())
					{
						var filename = "albumart_" + DateTime.Now.Ticks + ".jpg";
						using (var ms = new MemoryStream(mp3tagLib.Tag.Pictures[0].Data.Data))
						using (var image = Image.FromStream(ms))
						{
							image.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
						}
						albumart.Add(album, filename);
					}
					var albumArtFilename = albumart.ContainsKey(album) ? albumart[album] : null;

					songs.Add(new Song
					{
						File = file.Replace(directory, ""),
						Artist = artist,
						Album = album,
						AlbumArtists = albumArtists,
						AlbumArt = albumArtFilename,
						Title = title
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
		public string Album { get; set; }
		public string AlbumArtists { get; set; }
		public string AlbumArt { get; set; }
		public string Title { get; set; }
	}
}
