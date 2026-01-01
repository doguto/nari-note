using Microsoft.AspNetCore.Mvc;

namespace NariNoteBackend.Controller;

public abstract class ApplicationController : ControllerBase
{
    protected int UserId => (int)HttpContext.Items["UserId"]!;
}
