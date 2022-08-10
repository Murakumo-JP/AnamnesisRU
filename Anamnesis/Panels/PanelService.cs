// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Actor.Panels;
using Anamnesis.Services;
using Anamnesis.Windows;
using System;
using System.Collections.Generic;
using System.Windows;

public class PanelService : ServiceBase<PanelService>
{
	public static T Show<T>(object? context = null)
		where T : PanelBase
	{
		T? panel = Show(typeof(T), context) as T;

		if (panel == null)
			throw new Exception($"Failed to create instance of panel: {typeof(T)}");

		return panel;
	}

	public static PanelBase Show(Type panelType, object? context = null)
	{
		IPanelGroupHost groupHost = CreateHost();
		PanelBase? panel;

		try
		{
			panel = Activator.CreateInstance(panelType, groupHost) as PanelBase;
		}
		catch (Exception)
		{
			panel = Activator.CreateInstance(panelType, groupHost, context) as PanelBase;
		}

		if (panel == null)
			throw new Exception($"Failed to create instance of panel: {panelType}");

		panel.DataContext = context;
		groupHost.PanelGroupArea.Content = panel;
		groupHost.Show();

		// Copy width and height values from the inner panel to the host
		if (panel.CanResize && (double.IsNormal(panel.Width) || double.IsNormal(panel.Height)))
		{
			Rect rect = groupHost.Rect;

			if (double.IsNormal(panel.Width))
				rect.Width = panel.Width;

			if (double.IsNormal(panel.Height))
				rect.Height = panel.Height + 28; // height of panel titlebar

			panel.Width = double.NaN;
			panel.Height = double.NaN;
			groupHost.Rect = rect;
		}

		Rect? lastPos = GetLastPosition(groupHost);
		if (lastPos != null)
		{
			groupHost.RelativeRect = (Rect)lastPos;
		}
		else
		{
			// Center screen
			groupHost.RelativeRect = new Rect(0.5, 0.5, 0, 0);
		}

		return panel;
	}

	public static IPanelGroupHost CreateHost()
	{
		// TODO: if OverlayMode!
		return new OverlayWindow();
	}

	public static Rect? GetLastPosition(IPanelGroupHost panel)
	{
		if (SettingsService.Current.Panels.TryGetValue(panel.Id, out PanelsData? data) && data != null)
		{
			return data.Position;
		}

		return null;
	}

	public static void SavePosition(IPanelGroupHost panel)
	{
		if (SettingsService.Current.Panels.TryGetValue(panel.Id, out PanelsData? data) && data != null)
		{
			data.Position = panel.RelativeRect;
		}
		else
		{
			SettingsService.Current.Panels.Add(panel.Id, new(panel.RelativeRect));
		}

		SettingsService.Save();
	}

	[Serializable]
	public class PanelsData
	{
		public PanelsData()
		{
		}

		public PanelsData(Rect pos)
		{
			this.Position = pos;
		}

		public Rect Position { get; set; }
	}
}
