namespace NariNoteBackend.Domain.Shogi;

/// <summary>OG 画像の盤面に貼る駒の画像（先手向きの PNG）。後手の駒は描画時に 180 度回転する</summary>
public record KomaImage(string PieceType, byte[] Png);
