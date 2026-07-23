using System.Text;
using BloodCenterOS.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BloodCenterOS.API.Filters;

public class AuditLogFilter : IAsyncActionFilter
{
    private readonly IAuditService _audit;

    public AuditLogFilter(IAuditService audit)
    {
        _audit = audit;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var method = context.HttpContext.Request.Method;
        var isWrite = method == "POST" || method == "PUT" || method == "DELETE";
        var controllerType = context.Controller.GetType().Name;
        if (!isWrite || context.Controller is not ControllerBase ||
            controllerType == "AuditLogController" || controllerType == "AuthController")
        {
            await next();
            return;
        }

        var body = await ReadBodyAsync(context);

        var result = await next();

        if (result.Exception != null || result.Result is not ObjectResult objResult)
            return;

        var statusCode = objResult.StatusCode ?? 200;
        if (statusCode < 200 || statusCode > 299)
            return;

        var tableName = context.Controller.GetType().Name.Replace("Controller", "");
        var recordId = context.RouteData.Values["id"]?.ToString();
        var action = method switch
        {
            "POST" => "INSERT",
            "PUT" => "UPDATE",
            "DELETE" => "DELETE",
            _ => method
        };

        var details = $"{action} on {tableName}";

        await _audit.LogAsync(
            tableName: tableName,
            action: action,
            recordId: recordId,
            details: details,
            newValue: body
        );
    }

    private static async Task<string?> ReadBodyAsync(ActionExecutingContext context)
    {
        var req = context.HttpContext.Request;
        if (!req.Body.CanSeek)
        {
            req.EnableBuffering();
        }

        req.Body.Position = 0;
        using var reader = new StreamReader(req.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        req.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(body))
            return null;

        return body.Length > 4000 ? body[..4000] : body;
    }
}
