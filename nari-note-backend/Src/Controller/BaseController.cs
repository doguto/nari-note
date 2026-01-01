using Microsoft.AspNetCore.Mvc;

namespace NariNoteBackend.Controller;

public abstract class BaseController : ControllerBase
{
    protected int AuthenticatedUserId => (int)HttpContext.Items["UserId"]!;
}
