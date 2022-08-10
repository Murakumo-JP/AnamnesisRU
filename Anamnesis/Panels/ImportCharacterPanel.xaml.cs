// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Files;

public partial class ImportCharacterPanel : PanelBase
{
	public ImportCharacterPanel(IPanelGroupHost host, OpenResult openFile)
		: base(host)
	{
		this.InitializeComponent();
		this.Title = openFile.Path?.Name;
	}
}
