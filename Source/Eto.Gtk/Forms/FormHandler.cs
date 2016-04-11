using Eto.Forms;

namespace Eto.GtkSharp.Forms
{
	public class FormHandler : GtkWindow<Gtk.Window, Form, Form.ICallback>, Form.IHandler
	{
		public FormHandler(Gtk.Window window)
		{
			Control = window;
		}

		public FormHandler()
		{
			Control = new Gtk.Window(Gtk.WindowType.Toplevel);
#if GTK2
			Control.SetPosition(Gtk.WindowPosition.Center);
			Control.AllowGrow = true;
#else
			Control.Resizable = true;
#endif

			var vbox = new Gtk.VBox();
			vbox.PackStart(WindowActionControl, false, true, 0);
			vbox.PackStart(WindowContentControl, true, true, 0);
			Control.Add(vbox);
		}

		public void Show()
		{
			Control.ShowAll();
		}
	}
}
