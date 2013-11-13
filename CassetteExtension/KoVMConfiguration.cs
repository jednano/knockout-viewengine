using System.IO;
using System.Reflection;
using Microsoft.CSharp;

namespace KnockoutExtension
{
    public static class KoVMConfiguration
    {
        // Notes:
        // t4 templates by default execute on file change
        // t4 templates can be setup as a build action
        // t4 generated files are all sub to the t4 file itself which may cause issues with cassette
        // cassette will pick up files that do not exist in the project
        // cassette runs after application_start is completed
        // current KoVMConfiguration requires IIS Express to be shut down for application_start to run
        // Can not use class reflection against the dll, the file structure is not maintained
        // Can not compile a .cs file using CodeDomProvider and CSharpCodeProvider and then use reflection because reflection permissions deny access to source code

        /// <summary>
        /// Converts *.cs files to Knockout ViewModel *.js files; maintaining consistent directory structure
        /// </summary>
        /// <param name="sourceFromRoot">location of *.cs files</param>
        /// <param name="destinationFromRoot">location of resultant *.js files</param>
        public static void Process(string sourceFromRoot, string destinationFromRoot)
        {
            var root = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Replace(@"file:///", ""))).Parent;
            var source = new DirectoryInfo(root.FullName + sourceFromRoot);
            var destination = new DirectoryInfo(root.FullName + destinationFromRoot);

            // More efficient than using .GetFiles
            var sourceFiles = source.EnumerateFiles("*.cs", SearchOption.AllDirectories);

            // More efficient than using recursion through folder tree
            foreach (var file in sourceFiles)
            {
                var endDir = new DirectoryInfo(file.DirectoryName.Replace(source.FullName, destination.FullName));
                if (!endDir.Exists)
                    endDir.Create();
                
                // Use reader to pull in all the cs file text

                /*
                 * Ignore all using declarations
                 * Class Name is Function Name
                 * Constructor is Function Decloration
                 * All constructor assignments are a ko.observable
                 * Variable declarations that are not get; based are ko.computed
                 */

                // Wwrite out to new file

                var endFile = new FileInfo(endDir.FullName + "/" + file.Name.Replace(".cs", ".js"));
                if (endFile.Exists)
                    endFile.Delete();
                endFile.Create();
            }
        }
    }
}