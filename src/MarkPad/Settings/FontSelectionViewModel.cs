using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using MarkPad.Services.Interfaces;
using System.Windows.Media;

namespace MarkPad.Settings
{
	public class FontSelectionViewModel:Screen
	{
		private readonly ISettingsService settingsService;

		public IEnumerable<FontSizes> FontSizes { get; set; }
		public IEnumerable<FontFamily> FontFamilies { get; set; }
		public FontSizes SelectedFontSize { get; set; }
		public FontFamily SelectedFontFamily { get; set; }

		public FontSelectionViewModel(ISettingsService settingsService)
		{
			this.settingsService = settingsService;

            FontSizes = Enum.GetValues(typeof(FontSizes)).OfType<FontSizes>().ToArray();
			FontFamilies = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
		}

		public int SelectedActualFontSize
		{
			get
			{
				return Constants.FONT_SIZE_ENUM_ADJUSTMENT + (int)SelectedFontSize;
			}
		}

		public void Accept()
		{
			this.TryClose(true);
		}

		public void Cancel()
		{
			this.TryClose(false);
		}
	}
}
