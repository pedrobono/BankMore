using BankMore.ContaCorrente.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BankMore.ContaCorrente.Api.Middleware {
    public class ErrorHandlingMiddleware {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger) {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext) {
            try {
                await _next(httpContext);
            }
            catch (BusinessException ex) {
                _logger.LogWarning(ex, "[BusinessException] {FailureType}: {Message} | Path: {Path}", 
                    ex.FailureType, ex.Message, httpContext.Request.Path);
                await HandleExceptionAsync(httpContext, ex);
            }
            catch (UnauthorizedAccessException ex) {
                _logger.LogWarning(ex, "[UnauthorizedAccess]: {Message} | Path: {Path}", 
                    ex.Message, httpContext.Request.Path);
                await HandleExceptionAsync(httpContext, ex);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "[ERRO INTERNO] {Message} | Path: {Path} | StackTrace: {StackTrace}", 
                    ex.Message, httpContext.Request.Path, ex.StackTrace);
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception) {
            var statusCode = exception switch {
                BusinessException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var failureType = exception switch {
                BusinessException be => be.FailureType,
                UnauthorizedAccessException => "USER_UNAUTHORIZED",
                _ => "INTERNAL_ERROR"
            };

            var response = new {
                message = exception.Message,
                failureType = failureType
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
