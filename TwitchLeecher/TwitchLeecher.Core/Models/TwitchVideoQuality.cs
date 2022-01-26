using System;

namespace TwitchLeecher.Core.Models
{
    public class TwitchVideoQuality : IComparable<TwitchVideoQuality>
    {
        #region Constructors

        public TwitchVideoQuality(string id, string name, string resolution)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(resolution))
            {
                throw new ArgumentNullException(nameof(resolution));
            }

            Initialize(id, name, resolution);
        }

        #endregion Constructors

        #region Properties

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Resolution { get; private set; }

        public int VerticalResolution { get; private set; }

        public string DisplayString { get; private set; }

        #endregion Properties

        #region Methods

        private void Initialize(string id, string name, string resolution)
        {
            Id = id;
            Name = name;
            Resolution = resolution;
            DisplayString = $"{Name} ({Resolution})";
            VerticalResolution = GetVerticalResolution(resolution);
        }

        private int GetVerticalResolution(string resolution)
        {
            int start = resolution.IndexOf("x") + 1;
            return int.Parse(resolution.Substring(start, resolution.Length - start));
        }

        public int CompareTo(TwitchVideoQuality other)
        {
            if (other == null)
            {
                return -1;
            }

            if (VerticalResolution > other.VerticalResolution)
            {
                return -1;
            }
            else if (VerticalResolution < other.VerticalResolution)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TwitchVideoQuality other))
            {
                return false;
            }

            return Id.Equals(other.Id, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return DisplayString;
        }

        #endregion Methods
    }
}