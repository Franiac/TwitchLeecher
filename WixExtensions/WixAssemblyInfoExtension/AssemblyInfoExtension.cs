using Microsoft.Tools.WindowsInstallerXml;
using WixAssemblyInfoExtension;

[assembly: AssemblyDefaultWixExtension(typeof(AssemblyInfoExtension))]

namespace WixAssemblyInfoExtension
{
    public class AssemblyInfoExtension : WixExtension
    {
        private AssemlbyInfoPreprocessor assemblyInfoPreprocessor;

        public override PreprocessorExtension PreprocessorExtension
        {
            get
            {
                if (this.assemblyInfoPreprocessor == null)
                {
                    this.assemblyInfoPreprocessor = new AssemlbyInfoPreprocessor();
                }

                return this.assemblyInfoPreprocessor;
            }
        }
    }
}