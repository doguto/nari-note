using Microsoft.AspNetCore.Mvc;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Filter;

namespace NariNoteBackend.Controller;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ApplicationController
{
    readonly SaveBookLinesService saveBookLinesService;
    readonly AnalyzeBookPositionService analyzeBookPositionService;
    readonly CreateBookService createBookService;
    readonly DeleteBookMoveService deleteBookMoveService;
    readonly DeleteBookService deleteBookService;
    readonly GetBookPositionService getBookPositionService;
    readonly GetBookService getBookService;
    readonly GetMyBooksService getMyBooksService;
    readonly MergeShapeCandidateService mergeShapeCandidateService;
    readonly RejectShapeCandidateService rejectShapeCandidateService;
    readonly SplitShapeGroupService splitShapeGroupService;
    readonly UpdateBookMoveService updateBookMoveService;
    readonly UpdateShapeGroupService updateShapeGroupService;

    public BooksController(
        SaveBookLinesService saveBookLinesService,
        AnalyzeBookPositionService analyzeBookPositionService,
        CreateBookService createBookService,
        DeleteBookMoveService deleteBookMoveService,
        DeleteBookService deleteBookService,
        GetBookPositionService getBookPositionService,
        GetBookService getBookService,
        GetMyBooksService getMyBooksService,
        MergeShapeCandidateService mergeShapeCandidateService,
        RejectShapeCandidateService rejectShapeCandidateService,
        SplitShapeGroupService splitShapeGroupService,
        UpdateBookMoveService updateBookMoveService,
        UpdateShapeGroupService updateShapeGroupService
    )
    {
        this.saveBookLinesService = saveBookLinesService;
        this.analyzeBookPositionService = analyzeBookPositionService;
        this.createBookService = createBookService;
        this.deleteBookMoveService = deleteBookMoveService;
        this.deleteBookService = deleteBookService;
        this.getBookPositionService = getBookPositionService;
        this.getBookService = getBookService;
        this.getMyBooksService = getMyBooksService;
        this.mergeShapeCandidateService = mergeShapeCandidateService;
        this.rejectShapeCandidateService = rejectShapeCandidateService;
        this.splitShapeGroupService = splitShapeGroupService;
        this.updateBookMoveService = updateBookMoveService;
        this.updateShapeGroupService = updateShapeGroupService;
    }

    [HttpGet("my")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult<GetMyBooksResponse>> GetMyBooks()
    {
        var response = await this.getMyBooksService.ExecuteAsync(UserId!.Value);
        return Ok(response);
    }

    [HttpPost]
    [Auth(AuthLevel.Authenticated)]
    [ValidateModelState]
    public async Task<ActionResult<CreateBookResponse>> CreateBook([FromBody] CreateBookRequest request)
    {
        var response = await this.createBookService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult<GetBookResponse>> GetBook(BookId id)
    {
        var request = new GetBookRequest { Id = id };
        var response = await this.getBookService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult> DeleteBook(BookId id)
    {
        var request = new DeleteBookRequest { Id = id };
        await this.deleteBookService.ExecuteAsync(UserId!.Value, request);
        return NoContent();
    }

    [HttpGet("{id}/positions/{positionId}")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult<GetBookPositionResponse>> GetBookPosition(BookId id, BookPositionId positionId)
    {
        var request = new GetBookPositionRequest { Id = id, PositionId = positionId };
        var response = await this.getBookPositionService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpPost("{id}/analysis")]
    [Auth(AuthLevel.Authenticated)]
    [ValidateModelState]
    public async Task<ActionResult<AnalyzeBookPositionResponse>> AnalyzeBookPosition(BookId id, [FromBody] AnalyzeBookPositionRequest request)
    {
        request.Id = id;
        var response = await this.analyzeBookPositionService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpPost("{id}/lines")]
    [Auth(AuthLevel.Authenticated)]
    [ValidateModelState]
    public async Task<ActionResult<SaveBookLinesResponse>> SaveBookLines(BookId id, [FromBody] SaveBookLinesRequest request)
    {
        request.Id = id;
        var response = await this.saveBookLinesService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpPost("{id}/positions/{positionId}/split")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult<SplitShapeGroupResponse>> SplitShapeGroup(BookId id, BookPositionId positionId)
    {
        var request = new SplitShapeGroupRequest { Id = id, PositionId = positionId };
        var response = await this.splitShapeGroupService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpPut("{id}/moves/{moveId}")]
    [Auth(AuthLevel.Authenticated)]
    [ValidateModelState]
    public async Task<ActionResult<UpdateBookMoveResponse>> UpdateBookMove(
        BookId id,
        BookMoveId moveId,
        [FromBody] UpdateBookMoveRequest request
    )
    {
        request.Id = id;
        request.MoveId = moveId;
        var response = await this.updateBookMoveService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpDelete("{id}/moves/{moveId}")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult> DeleteBookMove(BookId id, BookMoveId moveId)
    {
        var request = new DeleteBookMoveRequest { Id = id, MoveId = moveId };
        await this.deleteBookMoveService.ExecuteAsync(UserId!.Value, request);
        return NoContent();
    }

    [HttpPut("{id}/groups/{groupId}")]
    [Auth(AuthLevel.Authenticated)]
    [ValidateModelState]
    public async Task<ActionResult<UpdateShapeGroupResponse>> UpdateShapeGroup(
        BookId id,
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
    public async Task<ActionResult<MergeShapeCandidateResponse>> MergeShapeCandidate(BookId id, ShapeCandidateId candidateId)
    {
        var request = new MergeShapeCandidateRequest { Id = id, CandidateId = candidateId };
        var response = await this.mergeShapeCandidateService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }

    [HttpPost("{id}/candidates/{candidateId}/reject")]
    [Auth(AuthLevel.Authenticated)]
    public async Task<ActionResult<RejectShapeCandidateResponse>> RejectShapeCandidate(BookId id, ShapeCandidateId candidateId)
    {
        var request = new RejectShapeCandidateRequest { Id = id, CandidateId = candidateId };
        var response = await this.rejectShapeCandidateService.ExecuteAsync(UserId!.Value, request);
        return Ok(response);
    }
}
