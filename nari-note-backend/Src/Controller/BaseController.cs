using Microsoft.AspNetCore.Mvc;

namespace NariNoteBackend.Controller;

public abstract class BaseController : ControllerBase
{
    protected int UserId => (int)HttpContext.Items["UserId"]!;
}
