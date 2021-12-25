using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModI18n
{

    static class STRINGS
    {
        public static class OPTIONS
        {
            public static class LANG
            {
                public static LocString NAME = "Prefered Language";
                public static LocString DESC = "Select the language that suits you best.";
            }
            public static class LOCAL_ONLY
            {
                public static LocString NAME = "Use Local Translations Only";
                public static LocString DESC = "ModI18n will fetch the latest translation files online when started. " +
                    "If fails, it will try to find local translations to load. " +
                    "If set to true, ModI18n will skip downloading translation files.";
            }
        }

    }
}