﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Windows;

using Anamnesis.Extensions;
using Anamnesis.Memory;
using Anamnesis.Panels;
using Anamnesis.Services;
using FontAwesome.Sharp;
using MaterialDesignThemes.Wpf;
using PropertyChanged;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using XivToolsWpf;

using MediaColor = System.Windows.Media.Color;

[AddINotifyPropertyChangedInterface]
public partial class FloatingWindow : Window, IPanelGroupHost
{
	private readonly WindowInteropHelper windowInteropHelper;
	private readonly List<IPanelGroupHost> children = new();

	private string? titleKey;
	private string? titleText;

	private IconChar icon;
	private bool canResize = true;
	private bool playOpenAnimation = true;
	private bool isOpeningAnimation = true;

	public FloatingWindow()
	{
		this.windowInteropHelper = new(this);
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.WindowContextMenu.DataContext = this;
		base.Topmost = false;

		this.TitleColor = Application.Current.Resources.GetTheme().ToolForeground;
	}

	// TODO: should be a combination of all child ids?
	public string Id => ((IPanel)this.PanelGroupArea.Content).Id;

	public IPanelGroupHost? ParentHost { get; set; }
	public ContentPresenter PanelGroupArea => this.ContentPresenter;
	public bool ShowBackground { get; set; } = true;
	public bool IsOpen { get; private set; }

	public IEnumerable<IPanelGroupHost> Children => this.children;

	public string? TitleKey
	{
		get => this.titleKey;
		set
		{
			this.titleKey = value;
			this.UpdateTitle();
		}
	}

	public new string? Title
	{
		get => this.titleText;
		set
		{
			this.titleText = value;
			this.UpdateTitle();
		}
	}

	public new IconChar Icon
	{
		get => this.icon;
		set
		{
			this.icon = value;
			this.TitleIcon.Icon = value;
		}
	}

	public new bool Topmost
	{
		get => base.Topmost;
		set
		{
			base.Topmost = value;
			this.UpdatePosition();
		}
	}

	public bool CanResize
	{
		get => this.canResize;
		set
		{
			this.canResize = value;
			this.ResizeMode = this.canResize ? ResizeMode.CanResize : ResizeMode.NoResize;
		}
	}

	public virtual Rect Rect
	{
		get
		{
			return new Rect(this.Left, this.Top, this.Width, this.Height);
		}
		set
		{
			if (value.Height != this.Height || value.Width != this.Width)
			{
				this.SizeToContent = SizeToContent.Manual;
			}

			this.Left = (int)value.X;
			this.Top = (int)value.Y;
			this.Width = value.Width;
			this.Height = value.Height;
			this.UpdatePosition();
		}
	}

	public virtual Rect ScreenRect
	{
		get
		{
			// We uuuuh... might need to know what screen we are on to begin with? idk.
			return new Rect(0, 0, SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
		}
	}

	public IPanelGroupHost Host => this;
	public MediaColor? TitleColor { get; set; }
	public virtual bool CanPopOut => false;
	public virtual bool CanPopIn => true;
	public bool AutoClose { get; set; } = false;

	public Rect RelativeRect
	{
		get
		{
			Rect screen = this.ScreenRect;
			Rect pos = this.Rect;
			Rect value = new();
			value.X = (pos.X - screen.X) / (screen.Width - pos.Width);
			value.Y = (pos.Y - screen.Y) / (screen.Height - pos.Height);
			value.Width = pos.Width;
			value.Height = pos.Height;

			Log.Information("< " + value);

			return value;
		}

		set
		{
			Log.Information("> " + value);

			Rect screen = this.ScreenRect;
			Rect pos = this.Rect;
			pos.X = screen.X + (screen.Width * value.X) - (pos.Width * value.X);
			pos.Y = screen.Y + (screen.Height * value.Y) - (pos.Height * value.Y);

			if (value.Height > 0 || value.Width > 0)
			{
				this.SizeToContent = SizeToContent.Manual;
			}
			else if (value.Height > 0)
			{
				this.SizeToContent = SizeToContent.Width;
			}
			else if (value.Width > 0)
			{
				this.SizeToContent = SizeToContent.Height;
			}

			if (value.Height > 0)
				pos.Height = value.Height;

			if (value.Width > 0)
				pos.Width = value.Width;

			this.Rect = pos;
		}
	}

	public virtual new void Show()
	{
		base.Show();

		Rect screen = this.ScreenRect;
		Rect pos = this.Rect;

		double maxHeight = Math.Max(screen.Height - pos.Top, 0);

		if (maxHeight == 0)
			maxHeight = 1080;

		this.MaxHeight = maxHeight;
		this.IsOpen = true;
	}

	public virtual void Show(IPanelGroupHost copy)
	{
		this.PanelGroupArea.Content = copy.PanelGroupArea.Content;

		if (this.PanelGroupArea.Content is PanelBase panel)
			panel.Host = this;

		this.playOpenAnimation = false;
		this.Show();

		this.TitleKey = copy.TitleKey;
		this.Title = copy.Title;
		this.Icon = copy.Icon;
		this.TitleColor = copy.TitleColor;
		this.ShowBackground = copy.ShowBackground;
		this.Topmost = copy.Topmost;
		this.CanResize = copy.CanResize;

		if (copy is FloatingWindow wnd)
		{
			this.AutoClose = wnd.AutoClose;
		}

		this.Rect = copy.Rect;
	}

	public void AddChild(IPanel panel)
	{
		panel.Host.ParentHost = this;
		this.children.Add(panel.Host);
	}

	public void RemoveChild(IPanel panel)
	{
		this.children.Remove(panel.Host);
		panel.Host.ParentHost = null;
	}

	public new void Close()
	{
		if (!this.isOpeningAnimation)
			PanelService.SavePosition(this);

		this.BeginStoryboard("CloseStoryboard");
		this.IsOpen = false;
	}

	protected virtual void OnWindowLoaded()
	{
	}

	protected virtual void UpdatePosition()
	{
	}

	protected override async void OnDeactivated(EventArgs e)
	{
		base.OnDeactivated(e);

		if (this.AutoClose)
		{
			// If we have docked panels that are active,
			// then we dont close yet.
			foreach (IPanel docked in this.children)
			{
				if (docked.Host.IsVisible)
				{
					return;
				}
			}

			this.Close();

			if (this.ParentHost != null)
			{
				this.ParentHost.RemoveChild(this);

				// wait a moment to see if the parent is actually being focused
				await Task.Delay(250);
				await Dispatch.MainThread();

				if (this.ParentHost is FloatingWindow wnd)
				{
					if (!wnd.IsActive)
					{
						wnd.OnDeactivated(e);
					}
				}
			}
		}
	}

	private void UpdateTitle()
	{
		StringBuilder sb = new();

		if (this.titleKey != null)
			sb.Append(LocalizationService.GetString(this.titleKey, true));

		if (this.titleKey != null && this.titleText != null)
			sb.Append(" ");

		if (this.titleText != null)
			sb.Append(this.titleText);

		base.Title = sb.ToString();
		this.TitleText.Text = base.Title;
	}

	private void OnWindowLoaded(object sender, RoutedEventArgs e)
	{
		this.UpdatePosition();
		this.Activate();

		this.OnWindowLoaded();

		if (this.playOpenAnimation)
		{
			this.BeginStoryboard("OpenStoryboard");
		}
		else
		{
			this.Opacity = 1.0;
		}
	}

	private void OnTitleMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			this.DragMove();
		}
	}

	private void OnTitlebarContextButtonClicked(object sender, RoutedEventArgs e)
	{
		this.WindowContextMenu.IsOpen = true;
	}

	private void OnTitlebarCloseButtonClicked(object sender, RoutedEventArgs e)
	{
		this.Close();
	}

	private void OnMouseEnter(object sender, MouseEventArgs e)
	{
		if (this.Opacity == 1.0)
			return;

		this.Animate(Window.OpacityProperty, 1.0, 100);
	}

	private void OnMouseLeave(object sender, MouseEventArgs e)
	{
		if (SettingsService.Current.Opacity != 1.0)
		{
			this.Animate(Window.OpacityProperty, SettingsService.Current.Opacity, 250);
		}
	}

	private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		this.Activate();

		////this.UpdatePosition();
		////PanelService.SavePosition(this);
	}

	private void OnCloseStoryboardCompleted(object sender, EventArgs e)
	{
		base.Close();
	}

	private void OnOpenStoryboardCompleted(object sender, EventArgs e)
	{
		this.isOpeningAnimation = false;
	}

	private void OnPopOutClicked(object sender, RoutedEventArgs e)
	{
		FloatingWindow wnd = new();
		wnd.Show(this);
		base.Close();
	}

	private void OnPopInClicked(object sender, RoutedEventArgs e)
	{
		OverlayWindow wnd = new();
		wnd.Show(this);
		base.Close();
	}
}
