using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AudioConversion.AudioConversionService
{
    public static class Utilities
    {
            public static string GetTempFilenameSafe()
            {
                return Path.GetTempPath() + Guid.NewGuid().ToString().Replace("-", "");
            }
            public static string GetTempFilenameSafe(string sExtension)
            {
                // Check for bad parameters.
                if (sExtension == null)
                    throw new ArgumentNullException("sExtension");
                return Path.GetTempPath() + Guid.NewGuid().ToString().Replace("-", "") + sExtension;
            }
    }
}
