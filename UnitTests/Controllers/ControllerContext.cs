using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.Web.Http.Routing;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Primitives;

namespace UnitTests.Controllers
{
    public class ControllerContextHelper
    {
        public static ControllerContext Create(string fileName)
        {
            string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
            var content = new MultipartFormDataContent(formDataBoundary);
         
            var contentBytes = new ByteArrayContent(File.ReadAllBytes(fileName));

            contentBytes.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                FileName = fileName
            };
            contentBytes.Headers.Add("Content-Type", "multipart/form-data");
            content.Add(contentBytes);

            var httpContext = new DefaultHttpContext();
            var contentStream = content.ReadAsStreamAsync().GetAwaiter().GetResult(); 
            httpContext.Request.Headers.Add("Content-Type", "multipart/form-data; boundary=" + formDataBoundary);
            httpContext.Request.Headers.Add("Content-Length", contentStream.Length.ToString());
            httpContext.Request.Body = contentStream;
        
            var actx = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new ControllerActionDescriptor());
            return new ControllerContext(actx);

        }
    }
}
