using System;
using System.IO;
using System.Linq;
using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length < 2)
            throw new ArgumentException("Need to provide path to assembly");

        var assembly = Assembly.LoadFile(args[0]);

        var version = assembly.GetCustomAttributes(true).OfType<AssemblyFileVersionAttribute>().FirstOrDefault() ?? new AssemblyFileVersionAttribute("0.0.0.0");

        File.WriteAllText(args[1], string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Include>
<?define COMPANY=""Code52""?>
<?define VERSION=""{0}""?>
</Include>", version.Version));
    }
}