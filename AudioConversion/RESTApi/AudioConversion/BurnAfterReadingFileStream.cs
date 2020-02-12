using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace AudioConversion.RESTApi.AudioConversion
{
    public class TempPhysicalFileResult : PhysicalFileResult
    {
        public TempPhysicalFileResult(string fileName, string contentType)
                     : base(fileName, contentType) { }
        public TempPhysicalFileResult(string fileName, MediaTypeHeaderValue contentType)
                     : base(fileName, contentType) { }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            await base.ExecuteResultAsync(context);
            File.Delete(FileName);
        }
    }
}
