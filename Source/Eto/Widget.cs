
// uncomment to track garbage collection of widgets
//#define TRACK_GC

using System;
using System.Globalization;

namespace Eto
{
	/// <summary>
	/// Handler interface for the <see cref="Widget"/> class
	/// </summary>
	/// <copyright>(c) 2012 by Curtis Wensley</copyright>
	/// <license type="BSD-3">See LICENSE for full terms</license>
	public interface IWidget : IPlatformSource
	{
		/// <summary>
		/// Gets or sets an ID for the widget
		/// </summary>
		/// <remarks>
		/// Some platforms may use this to identify controls (e.g. web)
		/// </remarks>
		string ID { get; set; }

		/// <summary>
		/// Gets the widget this handler is implemented for
		/// </summary>
		Widget Widget { get; set; }

		/// <summary>
		/// Called after the widget is constructed
		/// </summary>
		/// <remarks>
		/// This gets called automatically after the control is constructed and the <see cref="Widget"/> and <see cref="Platform"/> properties are set.
		/// When the handler has specialized construction methods, then the <see cref="AutoInitializeAttribute"/> can be used to disable automatic
		/// initialization. In this case, it is the responsibility of the subclass to call <see cref="Eto.Widget.Initialize()"/> 
		/// </remarks>
		void Initialize();

		/// <summary>
		/// Gets or sets the platform that was used to create this handler
		/// </summary>
		new Platform Platform { get; set; }

		/// <summary>
		/// Called to handle a specific event
		/// </summary>
		/// <remarks>
		/// Most events are late bound by this method. Instead of wiring all events, this
		/// will be called with an event string that is defined by the control.
		/// 
		/// This is called automatically when attaching to events, but must be called manually
		/// when users of the control only override the event's On... method.
		/// </remarks>
		/// <param name="id">ID of the event to handle</param>
		/// <param name = "defaultEvent">True if the event is default (e.g. overridden or via an event handler subscription)</param>
		void HandleEvent(string id, bool defaultEvent = false);
	}

	/// <summary>
	/// Interface for widgets that have a control object
	/// </summary>
	/// <copyright>(c) 2012 by Curtis Wensley</copyright>
	/// <license type="BSD-3">See LICENSE for full terms</license>
	public interface IControlObjectSource
	{
		/// <summary>
		/// Gets the control object for this widget
		/// </summary>
		/// <value>The control object for the widget</value>
		object ControlObject { get; }
	}

	/// <summary>
	/// Interface for widgets that have a handler
	/// </summary>
	/// <copyright>(c) 2012 by Curtis Wensley</copyright>
	/// <license type="BSD-3">See LICENSE for full terms</license>
	public interface IHandlerSource
	{
		/// <summary>
		/// Gets the platform handler object for the widget
		/// </summary>
		/// <value>The handler for the widget</value>
		object Handler { get; }
	}

	/// <summary>
	/// Interface for widgets that are created for a specific generator
	/// </summary>
	/// <copyright>(c) 2012 by Curtis Wensley</copyright>
	/// <license type="BSD-3">See LICENSE for full terms</license>
	public interface IPlatformSource
	{
		/// <summary>
		/// Gets the generator associated with the widget
		/// </summary>
		/// <value>The generator</value>
		Platform Platform { get; }
	}

	/// <summary>
	/// Base widget class for all objects requiring a platform-specific implementation
	/// </summary>
	/// <remarks>
	/// The Widget is the base of all abstracted objects that have platform-specific implementations.
	///
	/// To implement the handler for a widget, use the <see cref="Eto.WidgetHandler{TWidget}"/> as the base class.
	/// </remarks>
	/// <copyright>(c) 2012 by Curtis Wensley</copyright>
	/// <license type="BSD-3">See LICENSE for full terms</license>
	public abstract partial class Widget : IHandlerSource, IDisposable, IPlatformSource
	{
		IWidget WidgetHandler { get { return Handler as IWidget; } }


		/// <summary>
		/// Gets the platform that was used to create the <see cref="Handler"/> for this widget
		/// </summary>
		/// <remarks>
		/// This gets set to the current <see cref="Eto.Platform.Instance"/> during the construction of the object
		/// </remarks>
		public Platform Platform { get { return WidgetHandler.Platform; } }

		[Obsolete("Use Platform instead")]
		public Platform Generator { get { return ((IWidget)Handler).Platform; } }

		/// <summary>
		/// Gets the platform-specific handler for this widget
		/// </summary>
		public object Handler { get; internal set; }

		#if TRACK_GC
		~Widget()
		{
			Dispose(false);
		}
		#endif

		/// <summary>
		/// Initializes a new instance of the Widget class
		/// </summary>
		/// <param name="generator">Generator the widget handler was created with, or null to use <see cref="Eto.Generator.Current"/></param>
		/// <param name="handler">Handler to assign to this widget for its implementation</param>
		/// <param name="initialize">True to initialize the widget, false to defer that to the caller</param>
		[Obsolete("Use Widget(IHandler) instead")]
		protected Widget(Generator generator, IWidget handler, bool initialize = true)
		{
			if (generator == null)
				generator = Platform.Instance;
			this.Handler = handler;
			if (handler != null)
			{
				handler.Platform = (Platform)generator;
				handler.Widget = this; // tell the handler who we are
			}
			if (initialize)
				Initialize();
		}

		/// <summary>
		/// Initializes a new instance of the Widget class
		/// </summary>
		/// <param name="generator">Generator to create the handler with, or null to use <see cref="Eto.Generator.Current"/></param>
		/// <param name="type">Type of widget handler to create from the generator for this widget</param>
		/// <param name="initialize">True to initialize the widget, false to defer that to the caller</param>
		[Obsolete("Use default constructor and HandlerAttribute to specify handler to use")]
		protected Widget(Generator generator, Type type, bool initialize = true)
		{
			var platform = (Platform)generator ?? Platform.Instance;
			this.Handler = platform.Create(type);
			var widgetHandler = WidgetHandler;
			if (widgetHandler != null)
			{
				widgetHandler.Platform = (Platform)generator;
				widgetHandler.Widget = this;
			}
		}

		/// <summary>
		/// Initializes a new instance of the Widget class
		/// </summary>
		protected Widget()
		{
			var info = Platform.Instance.FindHandler(GetType());
			if (info == null)
				throw new HandlerInvalidException(string.Format(CultureInfo.CurrentCulture, "type for '{0}' could not be found in this platform", GetType().FullName));
			this.Handler = info.Instantiator();
			var widgetHandler = this.Handler as IWidget;
			if (widgetHandler != null)
			{
				widgetHandler.Platform = Platform.Instance;
				widgetHandler.Widget = this;
			}
			if (info.Initialize)
				Initialize();
		}

		/// <summary>
		/// Initializes a new instance of the Widget class
		/// </summary>
		/// <param name="handler">Handler to assign to this widget for its implementation</param>
		protected Widget(IWidget handler)
		{
			this.Handler = handler;
			if (handler != null)
			{
				handler.Platform = Platform.Instance;
				handler.Widget = this; // tell the handler who we are
			}
			Initialize();
		}

		/// <summary>
		/// Initializes the widget handler
		/// </summary>
		/// <remarks>
		/// This is typically called from the constructor after all of the logic is completed to construct
		/// the object.
		/// 
		/// If your handler interface has the <see cref="AutoInitializeAttribute"/> set to false, then you are responsible
		/// for calling this method in your constructor after calling the creation method on your custom handler.
		/// </remarks>
		protected void Initialize()
		{
			var handler = Handler as IWidget;
			if (handler != null)
				handler.Initialize();
			Eto.Style.OnStyleWidgetDefaults(this);
			EventLookup.HookupEvents(this);
		}

		PropertyStore properties;

		/// <summary>
		/// Gets the dictionary of properties for this widget
		/// </summary>
		public PropertyStore Properties
		{ 
			get { return properties ?? (properties = new PropertyStore(this)); } 
		}

		/// <summary>
		/// Gets or sets the ID of this widget
		/// </summary>
		public virtual string ID
		{
			get { return WidgetHandler.ID; }
			set { WidgetHandler.ID = value; }
		}

		/// <summary>
		/// Gets or sets the style of this widget
		/// </summary>
		/// <remarks>
		/// Styles allow you to attach custom platform-specific logic to a widget.
		/// In your platform-specific assembly, use <see cref="M:Style.Add{H}(string, StyleHandler{H})"/>
		/// to add the style logic with the same id.
		/// </remarks>
		/// <example>
		/// <code><![CDATA[
		/// // in your UI
		/// var control = new Button { Style = "mystyle" };
		/// 
		/// // in your platform assembly
		/// using Eto.Mac.Forms.Controls;
		/// 
		/// Styles.AddHandler<ButtonHandler>("mystyle", handler => {
		///		// this is where you can use handler.Control to set properties, handle events, etc.
		///		handler.Control.BezelStyle = NSBezelStyle.SmallSquare;
		/// });
		/// ]]></code>
		/// </example>
		public string Style
		{
			get { return Properties.Get<string>(StyleKey); }
			set
			{
				var style = Style;
				if (style != value)
				{
					Properties[StyleKey] = value;
					OnStyleChanged(EventArgs.Empty);
				}
			}
		}

		static readonly object StyleKey = new object();

		#region Events

		static readonly object StyleChangedKey = new object();

		/// <summary>
		/// Occurs when the <see cref="Widget.Style"/> property has changed
		/// </summary>
		public event EventHandler<EventArgs> StyleChanged
		{
			add { Properties.AddEvent(StyleChangedKey, value); }
			remove { Properties.RemoveEvent(StyleChangedKey, value); }
		}

		/// <summary>
		/// Handles when the <see cref="Style"/> is changed.
		/// </summary>
		protected virtual void OnStyleChanged(EventArgs e)
		{
			Eto.Style.OnStyleWidget(this);
			Properties.TriggerEvent(StyleChangedKey, this, e);
		}

		#endregion

		/// <summary>
		/// Gets the instance of the platform-specific object
		/// </summary>
		/// <remarks>
		/// This can sometimes be useful to get the platform-specific object.
		/// Some handlers may not have any backing object for its functionality, so this may be null.
		/// 
		/// It is more preferred to use the <see cref="Widget.Handler"/> and cast that to the platform-specific
		/// handler class which can give you additional methods and helpers to do common tasks.
		/// 
		/// For example, the <see cref="Forms.Application"/> object's handler for OS X has a AddFullScreenMenuItem
		/// property to specify if you want full screen support in your app.
		/// </remarks>
		public object ControlObject
		{
			get
			{ 
				var controlObjectSource = Handler as IControlObjectSource;
				return controlObjectSource != null ? controlObjectSource.ControlObject : null;
			}
		}

		/// <summary>
		/// Attaches the specified late-bound event to the control to be handled
		/// </summary>
		/// <remarks>
		/// This needs to be called when you want to override the On... methods instead of attaching 
		/// to the associated event.
		/// </remarks>
		/// <example>
		/// <code><![CDATA[
		/// // this will call HandleEvent automatically
		/// var textBox = new TextBox ();
		/// textBox.TextChanged += MyTextChangedHandler;
		/// 
		/// // must call HandleEvent when overriding OnTextChanged
		/// public class MyTextBox : TextBox
		/// {
		///		public MyTextBox()
		///		{
		///			HandleEvent (TextChangedEvent);
		///		}
		///		
		///		protected override void OnTextChanged (EventArgs e)
		///		{
		///			// your logic
		///		}
		/// }
		/// 
		/// ]]></code>
		/// </example>
		/// <param name="id">ID of the event to handle.  Usually a constant in the form of [Control].[EventName]Event (e.g. TextBox.TextChangedEvent)</param>
		internal void HandleEvent(string id)
		{
			WidgetHandler.HandleEvent(id, false);
		}

		/// <summary>
		/// Disposes of this widget, supressing the finalizer
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Handles the disposal of this widget
		/// </summary>
		/// <param name="disposing">True if the caller called <see cref="Dispose()"/> manually, false if being called from a finalizer</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				var handler = Handler as IDisposable;
				if (handler != null)
					handler.Dispose();
				Handler = null;
			}
			#if TRACK_GC
			Console.WriteLine ("{0}: {1}", disposing ? "Dispose" : "GC", GetType().Name);
			#endif
		}
	}
}

