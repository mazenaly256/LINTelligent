using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using LINTelligent.Application.Contracts.Interfaces;
using LINTelligent.Application.Services.Implementations;
using Moq;

namespace LINTelligent.UnitTests.ReviewServiceTests;

public class SubmitReviewRequestAsyncTests
{
    private Mock<IReviewRepository> _fakeReviewRepository;
    private Mock<IBackgroundJobClient> _fakeBackgroundJobClient;
    private Mock<ILLMClient> _fakeLLMClient;
    private Mock<IGitHubClient> _fakeGitHubClient;
    private Mock<INotificationService> _fakeNotificationService;
    private ReviewService _reviewService;

    public SubmitReviewRequestAsyncTests()
    {
        _fakeReviewRepository = new();
        _fakeBackgroundJobClient = new();
        _fakeLLMClient = new();
        _fakeGitHubClient = new();
        _fakeNotificationService = new();
        _reviewService = new(_fakeReviewRepository.Object, _fakeBackgroundJobClient.Object, _fakeLLMClient.Object, _fakeGitHubClient.Object, _fakeNotificationService.Object);
    }


    [Fact]
    public async Task SubmitReviewRequestAsync_WhenGitHubFileUrlExists_AddReviewToDatabaseAndEnqueueCodeFetchingThenCallingLLMJobs()
    {
        // Arrange
        var fetchingCodeSnippetJobId = "any_id";

        _fakeReviewRepository.Setup(mock => mock.AddNewReviewAsync(It.IsAny<Domain.Review>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        _fakeBackgroundJobClient.Setup(mock => mock.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()))
            .Returns(fetchingCodeSnippetJobId);


        // Act
        await _reviewService.SubmitReviewRequestAsync(new()
        {
            GitHubContentUrl = "https://raw.githubusercontent.com/"
        }, CancellationToken.None);


        // Assert
        _fakeReviewRepository.Verify(mock =>
            mock.AddNewReviewAsync(It.IsAny<Domain.Review>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _fakeBackgroundJobClient.Verify(mock => mock.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()),
            Times.Once);

        _fakeBackgroundJobClient.Verify(mock =>
            mock.Create(It.IsAny<Job>(), It.Is<AwaitingState>(state => state.ParentId == fetchingCodeSnippetJobId)),
            Times.Once);
    }


    [Fact]
    public async Task SubmitReviewRequestAsync_WhenGitHubFileUrlDoesNotExist_AddReviewToDatabaseAndEnqueueCallingLLMJob()
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.AddNewReviewAsync(It.IsAny<Domain.Review>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());


        // Act
        await _reviewService.SubmitReviewRequestAsync(new(), CancellationToken.None);


        // Assert
        _fakeReviewRepository.Verify(mock =>
            mock.AddNewReviewAsync(It.IsAny<Domain.Review>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _fakeBackgroundJobClient.Verify(mock =>
            mock.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()),
            Times.Once);
    }
}
