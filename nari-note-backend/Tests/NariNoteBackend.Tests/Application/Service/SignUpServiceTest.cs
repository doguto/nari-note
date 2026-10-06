using Moq;
using NariNoteBackend.Application.BackgroundJob;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Service;

namespace NariNoteBackend.Tests.Application.Service;

public class SignUpServiceTest
{
    readonly Mock<ISignUpJobQueue> signUpJobQueue = new();
    readonly List<SignUpJob> enqueuedJobs = new();

    public SignUpServiceTest()
    {
        this.signUpJobQueue
            .Setup(q => q.EnqueueAsync(It.IsAny<SignUpJob>(), It.IsAny<CancellationToken>()))
            .Callback<SignUpJob, CancellationToken>((job, _) => this.enqueuedJobs.Add(job))
            .Returns(ValueTask.CompletedTask);
    }

    [Fact]
    public async Task 登録有無の判定をせずジョブを投入して空のレスポンスを返す()
    {
        var service = new SignUpService(this.signUpJobQueue.Object);

        var response = await service.ExecuteAsync(new SignUpRequest
        {
            Name = "taro",
            Email = "taro@example.com",
            Password = "password123"
        });

        Assert.IsType<SignUpResponse>(response);
        // レスポンスにユーザーIDなど登録有無を推測できる情報を含めない
        Assert.Empty(typeof(SignUpResponse).GetProperties());

        var job = Assert.Single(this.enqueuedJobs);
        Assert.Equal("taro", job.Name);
        Assert.Equal("taro@example.com", job.Email);
    }

    [Fact]
    public async Task 平文パスワードではなくハッシュをジョブに渡す()
    {
        var service = new SignUpService(this.signUpJobQueue.Object);

        await service.ExecuteAsync(new SignUpRequest
        {
            Name = "taro",
            Email = "taro@example.com",
            Password = "password123"
        });

        var job = Assert.Single(this.enqueuedJobs);
        Assert.NotEqual("password123", job.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("password123", job.PasswordHash));
    }

    [Fact]
    public void ユーザーの存在確認に使える依存を持たない()
    {
        // リクエスト処理中に登録有無で分岐できないことを構造的に保証する
        var parameterTypes = typeof(SignUpService)
            .GetConstructors()
            .SelectMany(c => c.GetParameters())
            .Select(p => p.ParameterType);

        Assert.Equal([typeof(ISignUpJobQueue)], parameterTypes);
    }
}
