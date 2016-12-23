using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace ManuelsMusic
{
	public partial class Area
	{
		AreaViewModel ViewModel;

		public Area()
		{
			InitializeComponent();

			DataContext = ViewModel = new AreaViewModel();
		}
	}


	public class AreaViewModel : ViewModel
	{
		public AreaViewModel()
		{
			var resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resources.Single(r => r.EndsWith("songs.xml"))))
			{
				var songs = XDocument.Load(stream)
					.Descendants("song")
					.Select(d => new Song
					{
						Artist = d.Attribute("artist")?.Value,
						Album = d.Attribute("album")?.Value,
						AlbumArtists = d.Attribute("albumartists")?.Value,
						AlbumArt = d.Attribute("cover")?.Value,
						Title = d.Attribute("title")?.Value,
						File = d.Value
					})
					.Where(song => !song.File.StartsWith("AudioBooks\\"))
					.ToList();

				var albums = songs.GroupBy(s => new { s.AlbumArtists, s.Album, s.AlbumArt });
				Items = albums
					.Select(album => new Item(this, album.Key.AlbumArtists + " - " + album.Key.Album, album.Key.AlbumArt, album))
					.ToList();
			}
		}

		public List<Item> Items { get; set; }
		Item _SelectedItem; public Item SelectedItem { get { return _SelectedItem; } set { _SelectedItem = value; NotifyChanged("SelectedItem"); } }
	}

	public class Item : ViewModel
	{
		public string Name { get; private set; }
		public ImageSource Bild { get; private set; }
		public Command Oeffnen { get; private set; }
		public IEnumerable<Song> Songs { get; set; }

		AreaViewModel _Container;

		public Item(AreaViewModel container, string name, string image, IEnumerable<Song> songs)
		{
			_Container = container;

			Name = name;
			Songs = songs;
			Bild = new BitmapImage(new System.Uri("pack://application:,,,/Images/" + (image ?? "no_albumart.jpg")));
			Oeffnen = new Command(o =>
			{
				_Container.SelectedItem = this;
			});
		}
	}

	public class Song
	{
		public string File { get; set; }
		public string Artist { get; set; }
		public string Album { get; set; }
		public string AlbumArtists { get; set; }
		public string AlbumArt { get; set; }
		public string Title { get; set; }
	}
}
