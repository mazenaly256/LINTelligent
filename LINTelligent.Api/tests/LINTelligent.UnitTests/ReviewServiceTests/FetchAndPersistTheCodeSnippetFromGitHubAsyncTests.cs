using Hangfire;
using LINTelligent.Application.Contracts.DTOs;
using LINTelligent.Application.Contracts.Interfaces;
using LINTelligent.Application.Services.Implementations;
using Moq;

namespace LINTelligent.UnitTests.ReviewServiceTests;

public class FetchAndPersistTheCodeSnippetFromGitHubAsyncTests
{
    private Mock<IReviewRepository> _fakeReviewRepository;
    private Mock<IBackgroundJobClient> _fakeBackgroundJobClient;
    private Mock<ILLMClient> _fakeLLMClient;
    private Mock<IGitHubClient> _fakeGitHubClient;
    private Mock<INotificationService> _fakeNotificationService;
    private ReviewService _reviewService;

    public FetchAndPersistTheCodeSnippetFromGitHubAsyncTests()
    {
        _fakeReviewRepository = new();
        _fakeBackgroundJobClient = new();
        _fakeLLMClient = new();
        _fakeGitHubClient = new();
        _fakeNotificationService = new();
        _reviewService = new(_fakeReviewRepository.Object, _fakeBackgroundJobClient.Object, _fakeLLMClient.Object, _fakeGitHubClient.Object, _fakeNotificationService.Object);
    }


    [Fact]
    public async Task FetchAndPersistTheCodeSnippetFromGitHubAsync_WhenCalled_ChangesStatusToProcessingAndFetchCodeAndPersistIt()
    {
        // Arrange
        _fakeGitHubClient.Setup(mock => mock.FetchCodeSnippetFromUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("");

        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LLMResponseDto());

        // Act
        await _reviewService.FetchAndPersistTheCodeSnippetFromGitHubAsync(Guid.NewGuid(), "", CancellationToken.None);


        // Assert
        _fakeReviewRepository.Verify(mock =>
            mock.ChangeStatusAsync(It.IsAny<Guid>(), "Processing", It.IsAny<CancellationToken>()),
            Times.Once);

        _fakeReviewRepository.Verify(mock =>
            mock.PersistCodeSnippetFromGitHub(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task FetchAndPersistTheCodeSnippetFromGitHubAsync_WhenGitHubClientDoesNotRespondWithSuccessStatusCode_ChangesStatusToFailed()
    {
        // Arrange
        _fakeGitHubClient.Setup(mock => mock.FetchCodeSnippetFromUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException());

        var fakeReviewId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await _reviewService.FetchAndPersistTheCodeSnippetFromGitHubAsync(fakeReviewId, "", CancellationToken.None);        // due to rethrowing
        });

        _fakeReviewRepository.Verify(mock =>
            mock.ChangeStatusAsync(fakeReviewId, "Failed", It.IsAny<CancellationToken>()),
            Times.Once);

        _fakeReviewRepository.Verify(mock =>
            mock.PersistCodeSnippetFromGitHub(fakeReviewId, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
