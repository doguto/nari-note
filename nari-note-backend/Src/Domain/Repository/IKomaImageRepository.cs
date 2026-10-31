using NariNoteBackend.Domain.Shogi;

namespace NariNoteBackend.Domain.Repository;

public interface IKomaImageRepository
{
    /// <summary>駒の種類（"歩"、"成銀" 等）の画像を返す。存在しない種類なら例外</summary>
    KomaImage FindForceByPieceType(string pieceType);
}
