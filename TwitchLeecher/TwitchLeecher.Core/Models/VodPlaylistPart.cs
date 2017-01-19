namespace TwitchLeecher.Core.Models
{
    public class VodPlaylistPart : IVodPlaylistPart
    {
        #region Fields

        private int index;
        private string output;

        #endregion Fields

        #region Constructors

        public VodPlaylistPart(int index, string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                this.output = null;
            }
            else
            {
                this.output = input;
            }

            this.index = index;
        }

        #endregion Constructors

        #region Properties

        public int Index
        {
            get
            {
                return this.index;
            }
        }

        #endregion

        #region Methods

        public string GetOutput()
        {
            return this.output;
        }

        #endregion Methods
    }
}