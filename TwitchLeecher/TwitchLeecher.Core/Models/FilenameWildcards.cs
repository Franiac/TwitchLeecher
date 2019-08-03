﻿namespace TwitchLeecher.Core.Models
{
    public static class FilenameWildcards
    {
        public delegate bool IsFileNameUsedsDelegate(string fileName);

        public static string CHANNEL = "{channel}";

        public static string GAME = "{game}";

        public static string DATE = "{date}";

        public static string TIME = "{time}";

        public static string TIME24 = "{time24}";

        public static string DATE_ = "{date-}";

        public static string TIME_ = "{time-}";

        public static string TIME24_ = "{time24-}";

        public static string TITLE = "{title}";

        public static string RES = "{res}";

        public static string FPS = "{fps}";

        public static string ID = "{id}";

        public static string START = "{start}";

        public static string END = "{end}";

        public static string UNIQNUMBER = "{unumber}";
    }
}