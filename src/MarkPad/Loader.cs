using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MarkPad
{
    internal class Loader
    {
        private const string LIBSFOLDER = "Libs";

        private static Dictionary<string, Assembly> libs = new Dictionary<string, Assembly>();
        private static Dictionary<string, Assembly> reflectionOnlyLibs = new Dictionary<string, Assembly>();

        [System.STAThreadAttribute()]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += Loader.FindAssembly;

            PreloadUnmanagedLibraries();

            App app = new App();
            app.Run();
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        private static void PreloadUnmanagedLibraries()
        {
            // Preload correct library
            var bittyness = "x86";
            if (IntPtr.Size == 8)
                bittyness = "x64";

            var assemblyName = Assembly.GetExecutingAssembly().GetName();

            var libraries = Assembly.GetExecutingAssembly().GetManifestResourceNames()
                .Where(s => s.StartsWith(String.Format("{1}.{2}.{0}.", bittyness, assemblyName.Name, LIBSFOLDER)))
                .ToArray();

            string dirName = Path.Combine(Path.GetTempPath(), String.Format("{2}.{1}.{0}", assemblyName.Version.ToString(), bittyness, assemblyName.Name));
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            foreach (var lib in libraries)
            {
                string dllPath = Path.Combine(dirName, String.Join(".", lib.Split('.').Skip(3)));

                if (!File.Exists(dllPath))
                {
                    using (Stream stm = Assembly.GetExecutingAssembly().GetManifestResourceStream(lib))
                    {
                        // Copy the assembly to the temporary file
                        try
                        {
                            using (Stream outFile = File.Create(dllPath))
                            {
                                stm.CopyTo(outFile);
                            }
                        }
                        catch
                        {
                            // This may happen if another process has already created and loaded the file.
                            // Since the directory includes the version number of this assembly we can
                            // assume that it's the same bits, so we just ignore the excecption here and
                            // load the DLL.
                        }
                    }
                }

                // We must explicitly load the DLL here because the temporary directory 
                // is not in the PATH.
                // Once it is loaded, the DllImport directives that use the DLL will use
                // the one that is already loaded into the process.
                IntPtr h = LoadLibrary(dllPath);
            }
        }

        internal static Assembly LoadAssembly(string fullName)
        {
            Assembly a;

            var executingAssembly = Assembly.GetExecutingAssembly();

            var assemblyName = executingAssembly.GetName();

            string shortName = new AssemblyName(fullName).Name;
            if (libs.ContainsKey(shortName))
                return libs[shortName];

            var resourceName = String.Format("{0}.{2}.{1}.dll", assemblyName.Name, shortName, LIBSFOLDER);
            var actualName = executingAssembly.GetManifestResourceNames().Where(n => string.Equals(n, resourceName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (string.IsNullOrEmpty(actualName))
            {
                // The library might be a mixed mode assembly. Try loading from the bitty folders.
                var bittyness = "x86";
                if (IntPtr.Size == 8)
                    bittyness = "x64";

                resourceName = String.Format("{0}.{3}.{1}.{2}.dll", assemblyName.Name, bittyness, shortName, LIBSFOLDER);
                actualName = executingAssembly.GetManifestResourceNames().Where(n => string.Equals(n, resourceName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if (string.IsNullOrEmpty(actualName))
                {
                    libs[shortName] = null;
                    return null;
                }

                // Ok, mixed mode assemblies cannot be loaded through Assembly.Load.
                // See http://stackoverflow.com/questions/2945080/ and http://connect.microsoft.com/VisualStudio/feedback/details/97801/
                // But, since it's an unmanaged library we've already dumped it to disk to preload it into the process.
                // So, we'll just load it from there.
                string dirName = Path.Combine(Path.GetTempPath(), String.Format("{2}.{1}.{0}", assemblyName.Version.ToString(), bittyness, assemblyName.Name));
                string dllPath = Path.Combine(dirName, String.Join(".", actualName.Split('.').Skip(3)));

                if (!File.Exists(dllPath))
                {
                    libs[shortName] = null;
                    return null;
                }

                a = Assembly.LoadFile(dllPath);
                libs[shortName] = a;
                return a;
            }

            using (Stream s = executingAssembly.GetManifestResourceStream(actualName))
            {
                byte[] data = new BinaryReader(s).ReadBytes((int)s.Length);

                byte[] debugData = null;
                if (executingAssembly.GetManifestResourceNames().Contains(String.Format("{0}.{2}.{1}.pdb", assemblyName.Name, shortName, LIBSFOLDER)))
                {
                    using (Stream ds = executingAssembly.GetManifestResourceStream(String.Format("{0}.{2}.{1}.pdb", assemblyName.Name, shortName, LIBSFOLDER)))
                    {
                        debugData = new BinaryReader(ds).ReadBytes((int)ds.Length);
                    }
                }

                if (debugData != null)
                {
                    a = Assembly.Load(data, debugData);
                    libs[shortName] = a;
                    return a;
                }
                a = Assembly.Load(data);
                libs[shortName] = a;
                return a;
            }
        }

        internal static Assembly ReflectionOnlyLoadAssembly(string fullName)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();

            var assemblyName = Assembly.GetExecutingAssembly().GetName();

            string shortName = new AssemblyName(fullName).Name;
            if (reflectionOnlyLibs.ContainsKey(shortName))
                return reflectionOnlyLibs[shortName];

            var resourceName = String.Format("{0}.{2}.{1}.dll", assemblyName.Name, shortName, LIBSFOLDER);

            if (!executingAssembly.GetManifestResourceNames().Contains(resourceName))
            {
                reflectionOnlyLibs[shortName] = null;
                return null;
            }

            using (Stream s = executingAssembly.GetManifestResourceStream(resourceName))
            {
                byte[] data = new BinaryReader(s).ReadBytes((int)s.Length);

                Assembly a = Assembly.ReflectionOnlyLoad(data);
                reflectionOnlyLibs[shortName] = a;

                return a;
            }
        }

        internal static Assembly FindAssembly(object sender, ResolveEventArgs args)
        {
            return LoadAssembly(args.Name);
        }

        internal static Assembly FindReflectionOnlyAssembly(object sender, ResolveEventArgs args)
        {
            return ReflectionOnlyLoadAssembly(args.Name);
        }
    }
}
