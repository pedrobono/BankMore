using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Microsoft.AspNetCore.Authorization;

namespace BankMore.ContaCorrente.Api.Swagger;

public class SwaggerResponseOperationFilter : IOperationFilter {
    public void Apply(OpenApiOperation operation, OperationFilterContext context) {

        if (operation.Responses.ContainsKey("200")) {
            var response200 = operation.Responses["200"];
            if (response200.Content == null || !response200.Content.Any()) {
                operation.Responses.Remove("200");
            }
        }

        if (context.MethodInfo.Name != "RegistrarMovimento") {
            operation.Responses.Remove("204");
        }
        else {
            operation.Responses["204"].Description = "Sucesso: Operação realizada com êxito.";
        }

        var httpMethod = context.ApiDescription.HttpMethod;
        if (httpMethod == "POST" || httpMethod == "PUT") {
            operation.Responses["400"] = new OpenApiResponse {
                Description = "Erro de validação ou regra de negócio",
                Content = new Dictionary<string, OpenApiMediaType> {
                    ["application/json"] = new OpenApiMediaType {
                        Example = new OpenApiObject {
                            ["message"] = new OpenApiString("Descrição do erro"),
                            ["failureType"] = new OpenApiString("INVALID_DATA")
                        }
                    }
                }
            };
        }

        if (context.MethodInfo.Name == "CriarConta") {
            operation.Responses["201"].Content = new Dictionary<string, OpenApiMediaType> {
                ["application/json"] = new OpenApiMediaType {
                    Example = new OpenApiObject { ["numeroConta"] = new OpenApiString("85381-6") }
                }
            };
        }

        var hasAuthorize = context.MethodInfo.GetCustomAttributes(true).Any(a => a is AuthorizeAttribute) ||
                           context.MethodInfo.DeclaringType.GetCustomAttributes(true).Any(a => a is AuthorizeAttribute) ||
                           context.MethodInfo.Name == "Login";

        if (hasAuthorize) {
            operation.Responses["401"] = new OpenApiResponse {
                Description = "Não autorizado",
                Content = new Dictionary<string, OpenApiMediaType> {
                    ["application/json"] = new OpenApiMediaType {
                        Example = new OpenApiObject {
                            ["message"] = new OpenApiString("Usuário ou senha inválidos ou Token expirado"),
                            ["failureType"] = new OpenApiString("USER_UNAUTHORIZED")
                        }
                    }
                }
            };
        }
    }
}