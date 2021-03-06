﻿
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioConversion.RESTApi.AudioConversion
{
    public class SwaggerUploadFileParametersFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var parameters = operation.Parameters;
            if (parameters == null || parameters.Count == 0)
            {
                return;
            }
            var isUploadFile = context.ApiDescription.RelativePath.Contains("api/v1/audioconversion/");

            if (isUploadFile)
            {
                operation.RequestBody = new OpenApiRequestBody()
                {
                    Content =
                {
                    ["multipart/form-data"] = new OpenApiMediaType()
                    {
                        Schema = new OpenApiSchema()
                        {
                            Type = "object",
                            Properties =
                            {
                                ["file"] = new OpenApiSchema()
                                {
                                    Description = "Select file", Type = "string", Format = "binary"
                                }
                            }
                        }
                    }
                }
                };
            }
        }
    }
}
