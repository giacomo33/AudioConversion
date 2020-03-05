using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioConversion.RESTApi.AudioConversion
{
    public class BinaryContentAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryContentAttribute"/> class.
        /// </summary>
        public BinaryContentAttribute()
        {
            ParameterName = "payload";
            Required = true;
            MediaType = "application/octet-stream";
            Format = "binary";
        }

        /// <summary>
        /// Gets or sets the payload format.
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the payload media type.
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// Gets or sets a required flag.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets a parameter name.
        /// </summary>
        public string ParameterName { get; set; }
    }

    public class BinaryContentFilter : IOperationFilter
    {
        /// <summary>
        /// Configures operations decorated with the <see cref="BinaryContentAttribute" />.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="context">The context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo.GetCustomAttributes(typeof(BinaryContentAttribute), false).FirstOrDefault();
            if (attribute == null)
            {
                return;
            }

            operation.RequestBody = new OpenApiRequestBody() { Required = true };
            operation.RequestBody.Content.Add("application/octet-stream", new OpenApiMediaType()
            {
                Schema = new OpenApiSchema()
                {
                    Type = "string",
                    Format = "binary",
                },
            });
        }
    }
}
