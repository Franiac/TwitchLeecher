using System;
using System.Reflection;

namespace TwitchLeecher.Shared.Reflection
{
    public class AssemblyUtil
    {
        #region Fields

        private static AssemblyUtil instance;

        private string product;
        private Version version;

        #endregion Fields

        #region Properties

        public static AssemblyUtil Get
        {
            get
            {
                if (instance == null)
                {
                    instance = new AssemblyUtil();
                }

                return instance;
            }
        }

        #endregion Properties

        #region Methods

        public Version GetAssemblyVersion()
        {
            if (this.version == null)
            {
                Assembly a = Assembly.GetExecutingAssembly();

                if (a == null)
                {
                    throw new ApplicationException("Executing assembly is null!");
                }

                AssemblyFileVersionAttribute att = a.GetCustomAttribute<AssemblyFileVersionAttribute>();

                if (att == null)
                {
                    throw new ApplicationException("Could not find attribute of type '" + typeof(AssemblyFileVersionAttribute).FullName + "'!");
                }

                if (!Version.TryParse(att.Version, out this.version))
                {
                    throw new ApplicationException("Error while parsing assembly file version!");
                }
            }

            return this.version;
        }

        public string GetProductName()
        {
            if (string.IsNullOrEmpty(this.product))
            {
                Assembly a = Assembly.GetExecutingAssembly();

                if (a == null)
                {
                    throw new ApplicationException("Executing assembly is null!");
                }

                AssemblyProductAttribute att = a.GetCustomAttribute<AssemblyProductAttribute>();

                if (att == null)
                {
                    throw new ApplicationException("Could not find attribute of type '" + typeof(AssemblyProductAttribute).FullName + "'!");
                }

                this.product = att.Product;
            }

            return this.product;
        }

        #endregion Methods
    }
}