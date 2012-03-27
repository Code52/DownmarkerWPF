using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;

namespace MarkPad.MarkPadExtensions
{
	public class MarkPadExtensionsAutofacModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SpellCheckExtension>().As<SpellCheckExtension>();
		}
	}
}
