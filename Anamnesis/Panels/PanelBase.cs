﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Services;
using FontAwesome.Sharp;
using Serilog;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using XivToolsWpf;
using XivToolsWpf.DependencyProperties;

public abstract class PanelBase : UserControl, IPanel, INotifyPropertyChanged
{
	public static readonly IBind<string?> TitleDp = Binder.Register<string?, PanelBase>(nameof(Title), OnTitleChanged, BindMode.OneWay);

	public PanelBase(IPanelGroupHost host)
	{
		this.Host = host;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public ServiceManager Services => App.Services;
	public IPanelGroupHost Host { get; set; }
	public bool IsOpen => this.Host.IsOpen;

	public string? TitleKey
	{
		get => this.Host.TitleKey;
		set => this.Host.TitleKey = value;
	}

	public string? Title
	{
		get => TitleDp.Get(this);
		set => TitleDp.Set(this, value);
	}

	public IconChar Icon
	{
		get => this.Host.Icon;
		set => this.Host.Icon = value;
	}

	public Rect Rect
	{
		get => this.Host.Rect;
		set => this.Host.Rect = value;
	}

	public bool ShowBackground
	{
		get => this.Host.ShowBackground;
		set => this.Host.ShowBackground = value;
	}

	public CloseModes CloseMode
	{
		get => this.Host.CloseMode;
		set => this.Host.CloseMode = value;
	}

	public bool Topmost
	{
		get => this.Host.Topmost;
		set => this.Host.Topmost = value;
	}

	public bool CanResize
	{
		get => this.Host.CanResize;
		set => this.Host.CanResize = value;
	}

	public Color? TitleColor
	{
		get => this.Host.TitleColor;
		set => this.Host.TitleColor = value;
	}

	protected ILogger Log => Serilog.Log.ForContext(this.GetType());

	public void DragMove() => this.Host.DragMove();

	public virtual Point GetSubPanelDockOffset()
	{
		return new(this.Rect.Width, 0);
	}

	public void SetParent(IPanel other) => other.Host.AddChild(this);

	public void Close() => this.Host.Close();

	public async Task WhileOpen()
	{
		await Dispatch.NonUiThread();

		while (this.IsOpen)
		{
			await Task.Delay(500);
		}
	}

	private static void OnTitleChanged(PanelBase sender, string? value)
	{
		sender.Host.Title = value;
	}
}
