using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Controller;

public abstract class ApplicationController : ControllerBase
{
    protected UserId? UserId => HttpContext.Items["UserId"] as UserId?;
    protected string? UserName => UserId.HasValue ? HttpContext.Items["UserName"] as string : null;
}
