// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Panels;

using Anamnesis.Memory;
using Anamnesis.Panels;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using XivToolsWpf.Extensions;

public abstract class ActorPanelBase : PanelBase
{
	protected ActorPanelBase(IPanelGroupHost host)
		: base(host)
	{
		this.DataContextChanged += this.OnDataContextChanged;
	}

	public override string Id => base.Id + "_" + this.Actor?.Names.DisplayName;
	public ActorMemory? Actor { get; private set; }

	protected virtual Task OnActorChanged()
	{
		return Task.CompletedTask;
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (this.Actor != null)
		{
			this.Actor.PropertyChanged -= this.OnActorPropertyChanged;
			this.Actor.Names.PropertyChanged -= this.OnActorPropertyChanged;
		}

		this.Actor = null;

		if (this.DataContext is ActorMemory actor)
			this.Actor = actor;

		if (this.DataContext is PinnedActor pinnedActor)
			this.Actor = pinnedActor.GetMemory();

		if (this.Actor == null)
			return;

		this.Actor.PropertyChanged += this.OnActorPropertyChanged;
		this.Actor.Names.PropertyChanged += this.OnActorPropertyChanged;

		this.UpdateTitle();

		this.OnActorChanged().Run();
	}

	private void OnActorPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(Names.Text) || e.PropertyName == nameof(ActorMemory.Color))
		{
			this.UpdateTitle();
		}
	}

	private void UpdateTitle()
	{
		if (this.Actor == null)
			return;

		this.Title = " - " + this.Actor.Names.Text;
		this.TitleColor = this.Actor.Color;
	}
}
