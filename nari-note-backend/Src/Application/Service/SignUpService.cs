using NariNoteBackend.Application.BackgroundJob;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;

namespace NariNoteBackend.Application.Service;

public class SignUpService
{
    readonly ISignUpJobQueue signUpJobQueue;

    public SignUpService(ISignUpJobQueue signUpJobQueue)
    {
        this.signUpJobQueue = signUpJobQueue;
    }

    public async Task<SignUpResponse> ExecuteAsync(SignUpRequest request)
    {
        // メールアドレスの登録有無によらず同じレスポンス・同程度の応答時間にするため（ユーザー列挙攻撃防止）、
        // リクエスト内ではパスワードのハッシュ化とキュー投入のみを行い、
        // 登録有無の判定・DB書き込み・メール送信はバックグラウンドで実行する
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        await signUpJobQueue.EnqueueAsync(new SignUpJob(request.Name, request.Email, passwordHash));

        return new SignUpResponse();
    }
}
