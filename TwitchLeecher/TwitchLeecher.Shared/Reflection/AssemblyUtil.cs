using System;
using System.Reflection;

namespace TwitchLeecher.Shared.Reflection
{
    public class AssemblyUtil
    {
        #region Fields

        private static AssemblyUtil _instance;

        private string _product;
        private Version _version;

        #endregion Fields

        #region Properties

        public static AssemblyUtil Get
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AssemblyUtil();
                }

                return _instance;
            }
        }

        #endregion Properties

        #region Methods

        public Version GetAssemblyVersion()
        {
            if (_version == null)
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

                if (!Version.TryParse(att.Version, out _version))
                {
                    throw new ApplicationException("Error while parsing assembly file version!");
                }
            }

            return _version;
        }

        public string GetProductName()
        {
            if (string.IsNullOrEmpty(_product))
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

                _product = att.Product;
            }

            return _product;
        }

        #endregion Methods
    }
}