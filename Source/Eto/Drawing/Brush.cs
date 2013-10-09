using System;

namespace Eto.Drawing
{
	/// <summary>
	/// Base interface for brush handlers of type <see cref="Brush"/>
	/// </summary>
	/// <copyright>(c) 2012 by Curtis Wensley</copyright>
	/// <license type="BSD-3">See LICENSE for full terms</license>
	public interface IBrush
	{
	}

	/// <summary>
	/// Base brush class to use when calling fill methods of a <see cref="Graphics"/> object
	/// </summary>
	/// <copyright>(c) 2012 by Curtis Wensley</copyright>
	/// <license type="BSD-3">See LICENSE for full terms</license>
	public abstract class Brush : IHandlerSource, IControlObjectSource, IDisposable
	{
		/// <summary>
		/// Gets or sets the control object for this widget
		/// </summary>
		/// <value>The control object for the widget</value>
		public abstract object ControlObject { get; set; }

		/// <summary>
		/// Gets the platform handler object for the widget
		/// </summary>
		/// <value>The handler for the widget</value>
		public abstract object Handler { get; }

		/// <summary>
		/// Releases all resource used by the <see cref="Eto.Drawing.Brush"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Eto.Drawing.Brush"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="Eto.Drawing.Brush"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the <see cref="Eto.Drawing.Brush"/> so the garbage
		/// collector can reclaim the memory that the <see cref="Eto.Drawing.Brush"/> was occupying.</remarks>
		public void Dispose ()
		{
			var controlDispose = ControlObject as IDisposable;
			if (controlDispose != null)
				controlDispose.Dispose ();
			GC.SuppressFinalize(this);
		}
	}
}

