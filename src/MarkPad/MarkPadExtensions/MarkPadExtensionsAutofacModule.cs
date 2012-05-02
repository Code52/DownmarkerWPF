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
			builder
				.RegisterType<MarkPad.MarkPadExtensions.SpellCheck.SpellCheckExtension>()
				.As<MarkPad.MarkPadExtensions.SpellCheck.SpellCheckExtension>();
			builder.RegisterType<PluginManager>().As<IPluginManager>().SingleInstance();
		}
	}
}
