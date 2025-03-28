using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Swagger
{
    public class ErrorOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            AddCommonErrorResponses(operation);

            var returnType = GetFinalReturnType(context.MethodInfo.ReturnType);

            if (returnType == null || returnType == typeof(IActionResult)) return;

            AddBadRequestExample(operation, returnType);
        }

        /// <summary>
        /// Checks whether a specific type matches the given generic type.
        /// </summary>
        private bool IsGenericTypeOf(Type type, Type genericType)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == genericType;
        }

        /// <summary>
        /// Common error responses (401, 403, 404, 500).
        /// </summary>
        private void AddCommonErrorResponses(OpenApiOperation operation)
        {
            var commonResponses = new Dictionary<string, string>
            {
                { "500", "Internal server error" },
                { "404", "Not Found" },
                { "401", "Unauthorized" },
                { "403", "Forbidden" }
            };

            foreach (var response in commonResponses)
            {
                operation.Responses.TryAdd(response.Key, new OpenApiResponse
                {
                    Description = response.Value
                });
            }
        }

        private void AddBadRequestExample(OpenApiOperation operation, Type returnType)
        {
            // Check if type is IDataResult<T> 
            bool isDataResult = returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(IDataResult<>);
            Type innerType = isDataResult ? returnType.GetGenericArguments()[0] : null;

            // Common properties for response
            var properties = new Dictionary<string, OpenApiSchema>
            {
                { "ResponseCode", new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("00") } },
                { "ResponseDescription", new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("Request Successful.") } }
            };

            // If type is IDataResult<T> add "data" property
            if (isDataResult && innerType != null)
            {
                properties.Add("data", new OpenApiSchema
                {
                    Type = "object",
                    Nullable = true,
                    Example = null/*new Microsoft.OpenApi.Any.OpenApiString($"Example data of type {innerType.Name}")*/
                });
            }

            // Add response to the swagger documentation
            operation.Responses.TryAdd("400", new OpenApiResponse
            {
                Description = "Bad Request",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = properties
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Extracts the final return type (unwrapping Task or ActionResult).
        /// </summary>
        private Type GetFinalReturnType(Type returnType)
        {
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                returnType = returnType.GetGenericArguments()[0];
            }

            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ActionResult<>))
            {
                return returnType.GetGenericArguments()[0];
            }

            return returnType;
        }
    }
}
