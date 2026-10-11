using Microsoft.AspNetCore.Http;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class WithdrawServiceTest
{
    const string Password = "password123";

    readonly HttpResponse httpResponse = new DefaultHttpContext().Response;
    readonly IImageStorageGateway imageStorageGateway = Substitute.For<IImageStorageGateway>();
    readonly IUserRepository userRepository = Substitute.For<IUserRepository>();

    WithdrawService CreateService(User user)
    {
        this.userRepository.FindForceByIdAsync(user.Id).Returns(user);
        return new WithdrawService(this.userRepository, this.imageStorageGateway);
    }

    [Fact]
    public async Task パスワードが正しければユーザーを削除し認証Cookieを消す()
    {
        var user = new UserBuilder().WithPassword(Password).Build();
        var service = CreateService(user);

        await service.ExecuteAsync(user.Id, new WithdrawRequest { Password = Password }, this.httpResponse);

        await this.userRepository.Received(1).DeleteAsync(user.Id);
        Assert.StartsWith("authToken=;", this.httpResponse.Headers.SetCookie.ToString());
    }

    [Fact]
    public async Task アイコン画像が設定されていればユーザーの削除後に画像も削除する()
    {
        var user = new UserBuilder().WithPassword(Password).WithProfileImage("https://images.test/icon").Build();
        var service = CreateService(user);

        await service.ExecuteAsync(user.Id, new WithdrawRequest { Password = Password }, this.httpResponse);

        Received.InOrder(() =>
        {
            this.userRepository.DeleteAsync(user.Id);
            this.imageStorageGateway.DeleteUserIconAsync(user.Id.Value.ToString());
        });
    }

    [Fact]
    public async Task アイコン画像が未設定なら画像の削除は行わない()
    {
        var user = new UserBuilder().WithPassword(Password).Build();
        var service = CreateService(user);

        await service.ExecuteAsync(user.Id, new WithdrawRequest { Password = Password }, this.httpResponse);

        await this.imageStorageGateway.DidNotReceive().DeleteUserIconAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task パスワードが誤っていれば何も削除しない()
    {
        var user = new UserBuilder().WithPassword(Password).WithProfileImage("https://images.test/icon").Build();
        var service = CreateService(user);
        var request = new WithdrawRequest { Password = "wrong-password" };

        await Assert.ThrowsAsync<ArgumentException>(() => service.ExecuteAsync(user.Id, request, this.httpResponse));

        await this.userRepository.DidNotReceiveWithAnyArgs().DeleteAsync(user.Id);
        await this.imageStorageGateway.DidNotReceive().DeleteUserIconAsync(Arg.Any<string>());
        Assert.Equal("", this.httpResponse.Headers.SetCookie.ToString());
    }
}
