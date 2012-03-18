using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;

namespace MarkPad.Document.Addins
{
	public class ServicesModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SpellCheckAddin>().As<SpellCheckAddin>();
		} 
	}
}
