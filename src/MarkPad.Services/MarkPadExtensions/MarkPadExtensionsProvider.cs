using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkPad.Services.MarkPadExtensions
{
	public static class MarkPadExtensionsProvider
	{
		static IEnumerable<IMarkPadExtension> _extensions = new IMarkPadExtension[0];
		public static IEnumerable<IMarkPadExtension> Extensions
		{
			get { return _extensions; }
			set { _extensions = value; }
		}
	}
}
