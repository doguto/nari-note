using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Filter;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class PlaybooksController : ApplicationController
{
    readonly AnalyzePlaybookBoardService analyzePlaybookBoardService;
    readonly CreatePlaybookService createPlaybookService;
    readonly DeletePlaybookService deletePlaybookService;
    readonly GetMyPlaybooksService getMyPlaybooksService;
    readonly GetPlaybookService getPlaybookService;
    readonly InspectPlaybookBoardService inspectPlaybookBoardService;
    readonly MergePlaybookNodesService mergePlaybookNodesService;
    readonly RemovePlaybookMoveService removePlaybookMoveService;
    readonly SavePlaybookLinesService savePlaybookLinesService;
    readonly SplitPlaybookBoardService splitPlaybookBoardService;
    readonly UpdatePlaybookNodeService updatePlaybookNodeService;
    readonly UpdatePlaybookTransitionService updatePlaybookTransitionService;

    public PlaybooksController(
        AnalyzePlaybookBoardService analyzePlaybookBoardService,
        CreatePlaybookService createPlaybookService,
        DeletePlaybookService deletePlaybookService,
        GetMyPlaybooksService getMyPlaybooksService,
        GetPlaybookService getPlaybookService,
        InspectPlaybookBoardService inspectPlaybookBoardService,
        MergePlaybookNodesService mergePlaybookNodesService,
        RemovePlaybookMoveService removePlaybookMoveService,
        SavePlaybookLinesService savePlaybookLinesService,
        SplitPlaybookBoardService splitPlaybookBoardService,
        UpdatePlaybookNodeService updatePlaybookNodeService,
        UpdatePlaybookTransitionService updatePlaybookTransitionService
    )
    {
        this.analyzePlaybookBoardService = analyzePlaybookBoardService;
        this.createPlaybookService = createPlaybookService;
        this.deletePlaybookService = deletePlaybookService;
        this.getMyPlaybooksService = getMyPlaybooksService;
        this.getPlaybookService = getPlaybookService;
        this.inspectPlaybookBoardService = inspectPlaybookBoardService;
        this.mergePlaybookNodesService = mergePlaybookNodesService;
        this.removePlaybookMoveService = removePlaybookMoveService;
        this.savePlaybookLinesService = savePlaybookLinesService;
        this.splitPlaybookBoardService = splitPlaybookBoardService;
        this.updatePlaybookNodeService = updatePlaybookNodeService;
        this.updatePlaybookTransitionService = updatePlaybookTransitionService;
    }

    [HttpGet("my")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult<GetMyPlaybooksResponse>> GetMyPlaybooks()
    {
        var response = await this.getMyPlaybooksService.ExecuteAsync(UserId!.Value);
        return Ok(response);
    }

    [HttpPost]
    [Auth(AuthLevel.Authenticated)]
    [ValidateModelState]
    public async Task<ActionResult<CreatePlaybookResponse>> CreatePlaybook([FromBody] CreatePlaybookRequest request)
    {
        var response = await this.createPlaybookService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult<GetPlaybookResponse>> GetPlaybook(PlaybookId id)
    {
        var request = new GetPlaybookRequest { Id = id };
        var response = await this.getPlaybookService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult> DeletePlaybook(PlaybookId id)
    {
        var request = new DeletePlaybookRequest { Id = id };
        await this.deletePlaybookService.ExecuteAsync(UserId!.Value, request);
        return NoContent();
    }

    [HttpGet("{id}/inspection")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult<InspectPlaybookBoardResponse>> InspectPlaybookBoard(PlaybookId id, [FromQuery] string sfen)
    {
        var request = new InspectPlaybookBoardRequest { Id = id, Sfen = sfen };
        var response = await this.inspectPlaybookBoardService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpPost("{id}/analysis")]
    [Auth(AuthLevel.Authenticated)]
    [ValidateModelState]
    public async Task<ActionResult<AnalyzePlaybookBoardResponse>> AnalyzePlaybookBoard(PlaybookId id, [FromBody] AnalyzePlaybookBoardRequest request)
    {
        request.Id = id;
        var response = await this.analyzePlaybookBoardService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpPost("{id}/lines")]
    [Auth(AuthLevel.Authenticated)]
    [ValidateModelState]
    public async Task<ActionResult<SavePlaybookLinesResponse>> SavePlaybookLines(PlaybookId id, [FromBody] SavePlaybookLinesRequest request)
    {
        request.Id = id;
        var response = await this.savePlaybookLinesService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpPost("{id}/moves/remove")]
    [Auth(AuthLevel.Authenticated)]
    [ValidateModelState]
    public async Task<ActionResult<RemovePlaybookMoveResponse>> RemovePlaybookMove(PlaybookId id, [FromBody] RemovePlaybookMoveRequest request)
    {
        request.Id = id;
        var response = await this.removePlaybookMoveService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpPut("{id}/nodes")]
    [Auth(AuthLevel.Authenticated)]
    [ValidateModelState]
    public async Task<ActionResult<UpdatePlaybookNodeResponse>> UpdatePlaybookNode(PlaybookId id, [FromBody] UpdatePlaybookNodeRequest request)
    {
        request.Id = id;
        var response = await this.updatePlaybookNodeService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpPost("{id}/nodes/merge")]
    [Auth(AuthLevel.Authenticated)]
    [ValidateModelState]
    public async Task<ActionResult<MergePlaybookNodesResponse>> MergePlaybookNodes(PlaybookId id, [FromBody] MergePlaybookNodesRequest request)
    {
        request.Id = id;
        var response = await this.mergePlaybookNodesService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpPost("{id}/boards/{boardId}/split")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult<SplitPlaybookBoardResponse>> SplitPlaybookBoard(PlaybookId id, PlaybookBoardId boardId)
    {
        var request = new SplitPlaybookBoardRequest { Id = id, BoardId = boardId };
        var response = await this.splitPlaybookBoardService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpPut("{id}/transitions/{transitionId}")]
    [Auth(AuthLevel.Authenticated)]
    [ValidateModelState]
    public async Task<ActionResult<UpdatePlaybookTransitionResponse>> UpdatePlaybookTransition(PlaybookId id, PlaybookBoardTransitionId transitionId, [FromBody] UpdatePlaybookTransitionRequest request)
    {
        request.Id = id;
        request.TransitionId = transitionId;
        var response = await this.updatePlaybookTransitionService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }
}
