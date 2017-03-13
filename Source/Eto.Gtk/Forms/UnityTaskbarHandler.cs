using System;
using System.Runtime.InteropServices;
using Eto.Forms;

namespace Eto.GtkSharp
{
	public class UnityTaskbarHandler : WidgetHandler<Widget>, Taskbar.IHandler
	{
		private const string libunity = "libunity.so.9";

		[DllImport(libunity, CallingConvention = CallingConvention.Cdecl)]
		protected extern static IntPtr unity_launcher_entry_get_for_desktop_id(string app_name);

		[DllImport(libunity, CallingConvention = CallingConvention.Cdecl)]
		protected extern static void unity_launcher_entry_set_progress_visible(IntPtr self, bool visible);

		[DllImport(libunity, CallingConvention = CallingConvention.Cdecl)]
		protected extern static void unity_launcher_entry_set_progress(IntPtr self, double progress);

		[DllImport(libunity, CallingConvention = CallingConvention.Cdecl)]
		protected extern static void unity_launcher_entry_set_urgent(IntPtr self, bool urgent);

		private static IntPtr handle;

		static UnityTaskbarHandler()
		{
			var desktopEntry = Environment.GetEnvironmentVariable("DESKTOP_ENTRY");

			if (string.IsNullOrEmpty(desktopEntry))
				Console.WriteLine("Please set DESKTOP_ENTRY to point to your apps .desktop launcher in order to use application progressbar.");

			try
			{
				handle = unity_launcher_entry_get_for_desktop_id(desktopEntry);
			}
			catch
			{
				Console.WriteLine("Failed to init Unity LauncherEntry API.");
			}
		}

		public void SetProgress(TaskbarProgressState state, float progress)
		{
			if (handle == IntPtr.Zero)
				return;

			unity_launcher_entry_set_progress(handle, progress);
			unity_launcher_entry_set_progress_visible(handle, state != TaskbarProgressState.Default);
			unity_launcher_entry_set_urgent(handle, state == TaskbarProgressState.Error || progress == 1.0f);
		}
	}
}
