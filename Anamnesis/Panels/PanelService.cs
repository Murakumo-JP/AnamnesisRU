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
	public static T Show<T>()
		where T : PanelBase
	{
		T? panel = Show(typeof(T)) as T;

		if (panel == null)
			throw new Exception($"Failed to create instance of panel: {typeof(T)}");

		return panel;
	}

	public static PanelBase Show(Type type)
	{
		IPanelGroupHost groupHost = CreateHost();

		PanelBase? panel = Activator.CreateInstance(type, groupHost) as PanelBase;

		if (panel == null)
			throw new Exception($"Failed to create instance of panel: {type}");

		groupHost.PanelGroupArea.Content = panel;
		groupHost.Show();

		Rect? lastPos = GetLastPosition(panel);
		if (lastPos != null)
			groupHost.RelativeRect = (Rect)lastPos;

		return panel;
	}

	public static IPanelGroupHost CreateHost()
	{
		// TODO: if OverlayMode!
		return new OverlayWindow();
	}

	public static Rect? GetLastPosition(IPanel panel)
	{
		if (SettingsService.Current.Panels.TryGetValue(panel.Id, out PanelsData? data) && data != null)
		{
			return data.Position;
		}

		return null;
	}

	public static void SavePosition(IPanel panel)
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
