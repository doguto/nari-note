namespace NariNoteBackend.Application.Dto;

public class KifuDto
{
    public required string Name { get; set; }
    public required string KifuText { get; set; }
    public int SortOrder { get; set; }
}
