using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Controller;

public abstract class ApplicationController : ControllerBase
{
    protected UserId UserId => UserId.From((int)HttpContext.Items["UserId"]!);
}
