using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class GetCurrentUserServiceTest
{
    readonly IImageStorageGateway imageStorageGateway = Substitute.For<IImageStorageGateway>();
    readonly GetCurrentUserService service;

    public GetCurrentUserServiceTest()
    {
        this.service = new GetCurrentUserService(this.imageStorageGateway);
    }

    [Fact]
    public void 認証済みならユーザー情報とアイコンのURLを返す()
    {
        var user = new UserBuilder().WithName("taro").Build();
        this.imageStorageGateway.GetUserIconUrl(user.Id.Value.ToString()).Returns("https://images.test/taro");

        var response = this.service.Execute(new GetCurrentUserRequest(), user.Id, "taro");

        Assert.Equal(user.Id, response.UserId);
        Assert.Equal("taro", response.UserName);
        Assert.Equal("https://images.test/taro", response.UserIconImageUrl);
    }

    [Fact]
    public void 未認証なら空の情報を返す()
    {
        var response = this.service.Execute(new GetCurrentUserRequest(), null, null);

        Assert.Null(response.UserId);
        Assert.Null(response.UserName);
        Assert.Null(response.UserIconImageUrl);
        this.imageStorageGateway.DidNotReceive().GetUserIconUrl(Arg.Any<string>());
    }
}
