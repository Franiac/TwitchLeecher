namespace TwitchLeecher.Core.Models
{
    public class VodPlaylistPart : IVodPlaylistPart
    {
        #region Fields

        private int _index;
        private string _output;

        #endregion Fields

        #region Constructors

        public VodPlaylistPart(int index, string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                _output = null;
            }
            else
            {
                _output = input;
            }

            _index = index;
        }

        #endregion Constructors

        #region Properties

        public int Index
        {
            get
            {
                return _index;
            }
        }

        #endregion

        #region Methods

        public string GetOutput()
        {
            return _output;
        }

        #endregion Methods
    }
}