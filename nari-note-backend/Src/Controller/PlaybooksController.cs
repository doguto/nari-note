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
    readonly SavePlaybookLinesService savePlaybookLinesService;
    readonly AnalyzePlaybookPositionService analyzePlaybookPositionService;
    readonly CreatePlaybookService createPlaybookService;
    readonly DeletePlaybookMoveService deletePlaybookMoveService;
    readonly DeletePlaybookService deletePlaybookService;
    readonly GetPlaybookPositionService getPlaybookPositionService;
    readonly GetPlaybookService getPlaybookService;
    readonly GetMyPlaybooksService getMyPlaybooksService;
    readonly MergeShapeCandidateService mergeShapeCandidateService;
    readonly RejectShapeCandidateService rejectShapeCandidateService;
    readonly SplitShapeGroupService splitShapeGroupService;
    readonly UpdatePlaybookMoveService updatePlaybookMoveService;
    readonly UpdateShapeGroupService updateShapeGroupService;

    public PlaybooksController(
        SavePlaybookLinesService savePlaybookLinesService,
        AnalyzePlaybookPositionService analyzePlaybookPositionService,
        CreatePlaybookService createPlaybookService,
        DeletePlaybookMoveService deletePlaybookMoveService,
        DeletePlaybookService deletePlaybookService,
        GetPlaybookPositionService getPlaybookPositionService,
        GetPlaybookService getPlaybookService,
        GetMyPlaybooksService getMyPlaybooksService,
        MergeShapeCandidateService mergeShapeCandidateService,
        RejectShapeCandidateService rejectShapeCandidateService,
        SplitShapeGroupService splitShapeGroupService,
        UpdatePlaybookMoveService updatePlaybookMoveService,
        UpdateShapeGroupService updateShapeGroupService
    )
    {
        this.savePlaybookLinesService = savePlaybookLinesService;
        this.analyzePlaybookPositionService = analyzePlaybookPositionService;
        this.createPlaybookService = createPlaybookService;
        this.deletePlaybookMoveService = deletePlaybookMoveService;
        this.deletePlaybookService = deletePlaybookService;
        this.getPlaybookPositionService = getPlaybookPositionService;
        this.getPlaybookService = getPlaybookService;
        this.getMyPlaybooksService = getMyPlaybooksService;
        this.mergeShapeCandidateService = mergeShapeCandidateService;
        this.rejectShapeCandidateService = rejectShapeCandidateService;
        this.splitShapeGroupService = splitShapeGroupService;
        this.updatePlaybookMoveService = updatePlaybookMoveService;
        this.updateShapeGroupService = updateShapeGroupService;
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

    [HttpGet("{id}/positions/{positionId}")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult<GetPlaybookPositionResponse>> GetPlaybookPosition(PlaybookId id, PlaybookPositionId positionId)
    {
        var request = new GetPlaybookPositionRequest { Id = id, PositionId = positionId };
        var response = await this.getPlaybookPositionService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpPost("{id}/analysis")]
    [Auth(AuthLevel.Authenticated)]
    [ValidateModelState]
    public async Task<ActionResult<AnalyzePlaybookPositionResponse>> AnalyzePlaybookPosition(PlaybookId id, [FromBody] AnalyzePlaybookPositionRequest request)
    {
        request.Id = id;
        var response = await this.analyzePlaybookPositionService.ExecuteAsync(UserId!.Value, request);
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

    [HttpPost("{id}/positions/{positionId}/split")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult<SplitShapeGroupResponse>> SplitShapeGroup(PlaybookId id, PlaybookPositionId positionId)
    {
        var request = new SplitShapeGroupRequest { Id = id, PositionId = positionId };
        var response = await this.splitShapeGroupService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpPut("{id}/moves/{moveId}")]
    [Auth(AuthLevel.Authenticated)]
    [ValidateModelState]
    public async Task<ActionResult<UpdatePlaybookMoveResponse>> UpdatePlaybookMove(
        PlaybookId id,
        PlaybookMoveId moveId,
        [FromBody] UpdatePlaybookMoveRequest request
    )
    {
        request.Id = id;
        request.MoveId = moveId;
        var response = await this.updatePlaybookMoveService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpDelete("{id}/moves/{moveId}")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult> DeletePlaybookMove(PlaybookId id, PlaybookMoveId moveId)
    {
        var request = new DeletePlaybookMoveRequest { Id = id, MoveId = moveId };
        await this.deletePlaybookMoveService.ExecuteAsync(UserId!.Value, request);
        return NoContent();
    }

    [HttpPut("{id}/groups/{groupId}")]
    [Auth(AuthLevel.Authenticated)]
    [ValidateModelState]
    public async Task<ActionResult<UpdateShapeGroupResponse>> UpdateShapeGroup(
        PlaybookId id,
        ShapeGroupId groupId,
        [FromBody] UpdateShapeGroupRequest request
    )
    {
        request.Id = id;
        request.GroupId = groupId;
        var response = await this.updateShapeGroupService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpPost("{id}/candidates/{candidateId}/merge")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult<MergeShapeCandidateResponse>> MergeShapeCandidate(PlaybookId id, ShapeCandidateId candidateId)
    {
        var request = new MergeShapeCandidateRequest { Id = id, CandidateId = candidateId };
        var response = await this.mergeShapeCandidateService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpPost("{id}/candidates/{candidateId}/reject")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult<RejectShapeCandidateResponse>> RejectShapeCandidate(PlaybookId id, ShapeCandidateId candidateId)
    {
        var request = new RejectShapeCandidateRequest { Id = id, CandidateId = candidateId };
        var response = await this.rejectShapeCandidateService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }
}
