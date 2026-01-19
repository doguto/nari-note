using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Controller;

public abstract class ApplicationController : ControllerBase
{
    protected UserId UserId => (UserId)HttpContext.Items["UserId"]!;
    protected UserId? NullableUserId => HttpContext.Items["UserId"] as UserId?;
    protected bool HasUserId => HttpContext.Items["UserId"] != null;
}
