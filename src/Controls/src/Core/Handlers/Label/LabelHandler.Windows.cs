﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class LabelHandler : Microsoft.Maui.Handlers.LabelHandler
	{
		public static void MapTextType(LabelHandler handler, Label label) =>
			Platform.TextBlockExtensions.UpdateText(handler.NativeView, label);
	}
}