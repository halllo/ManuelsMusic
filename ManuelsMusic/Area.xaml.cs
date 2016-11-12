using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
			Items = new List<Item>();
			for (int i = 0; i < 300; i++)
			{
				Items.Add(new Item(this, "" + i % 4));
			}
		}

		public List<Item> Items { get; set; }
		Item _SelectedItem; public Item SelectedItem { get { return _SelectedItem; } set { _SelectedItem = value; NotifyChanged("SelectedItem"); } }
	}

	public class Item : ViewModel
	{
		public string Name { get; set; }
		public ImageSource Bild { get; set; }
		public Command Oeffnen { get; set; }

		AreaViewModel _Container;

		public Item(AreaViewModel container, string name)
		{
			_Container = container;

			Name = name;
			Bild = new BitmapImage(new System.Uri("pack://application:,,,/Images/" + name + ".jpg"));
			Oeffnen = new Command(o =>
			{
				System.Diagnostics.Debug.WriteLine(Name);
				_Container.SelectedItem = this;
			});
		}
	}
}
