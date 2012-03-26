using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;

namespace MarkPad.Extensions
{
	public class ExtensionsAutofacModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SpellCheckExtension>().As<SpellCheckExtension>();
		}
	}
}
