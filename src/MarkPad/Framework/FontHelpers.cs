using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace MarkPad.Framework
{
	public static class FontHelpers
	{
		public static FontFamily TryGetFontFamilyFromStack(params string[] sources)
		{
			return sources
				.Select(source => Fonts.SystemFontFamilies.FirstOrDefault(f => f.Source == source))
				.Where(f => f != null)
				.FirstOrDefault();
		}
	}
}
