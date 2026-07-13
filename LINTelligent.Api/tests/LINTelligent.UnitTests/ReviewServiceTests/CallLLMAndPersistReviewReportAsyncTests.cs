using Hangfire;
using LINTelligent.Application.Contracts.DTOs;
using LINTelligent.Application.Contracts.Interfaces;
using LINTelligent.Application.Services.Implementations;
using Moq;

namespace LINTelligent.UnitTests.ReviewServiceTests;

public class CallLLMAndPersistReviewReportAsyncTests
{
    private Mock<IReviewRepository> _fakeReviewRepository;
    private Mock<IBackgroundJobClient> _fakeBackgroundJobClient;
    private Mock<ILLMClient> _fakeLLMClient;
    private Mock<IGitHubClient> _fakeGitHubClient;
    private Mock<INotificationService> _fakeNotificationService;
    private ReviewService _reviewService;

    public CallLLMAndPersistReviewReportAsyncTests()
    {
        _fakeReviewRepository = new();
        _fakeBackgroundJobClient = new();
        _fakeLLMClient = new();
        _fakeGitHubClient = new();
        _fakeNotificationService = new();
        _reviewService = new(_fakeReviewRepository.Object, _fakeBackgroundJobClient.Object, _fakeLLMClient.Object, _fakeGitHubClient.Object, _fakeNotificationService.Object);
    }


    [Fact]
    public async Task CallLLMAndPersistReviewReportAsync_WhenCalled_ChangesStatusToProcessingBeforeToCompletedOrFailedInOrder()
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());

        List<string> returnedStatuses = new();

        _fakeReviewRepository.Setup(mock => mock.ChangeStatusAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, string, CancellationToken>((_, status, _) =>
            {
                returnedStatuses.Add(status);
            })
            .Returns(Task.CompletedTask);

        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LLMResponseDto());


        // Act
        await _reviewService.CallLLMAndPersistReviewReportAsync(new Guid(), CancellationToken.None);


        // Assert
        Assert.Equal(2, returnedStatuses.Count);
        Assert.Equal("Processing", returnedStatuses[0]);
        Assert.True(returnedStatuses[1] == "Completed" || returnedStatuses[1] == "Failed");

    }


    [Fact]
    public async Task CallLLMAndPersistReviewReportAsync_WhenLLMReturnsTheReviewReport_SavesReportToDatabase()
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());

        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LLMResponseDto());


        // Act
        await _reviewService.CallLLMAndPersistReviewReportAsync(new Guid(), CancellationToken.None);


        // Assert
        _fakeReviewRepository.Verify(mock =>
            mock.AddReportToTheReviewAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task CallLLMAndPersistReviewReportAsync_WhenCalled_ChangesStatusToCompletedOrFailed()
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());


        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LLMResponseDto());


        // Act
        await _reviewService.CallLLMAndPersistReviewReportAsync(new Guid(), CancellationToken.None);


        // Assert
        _fakeReviewRepository.Verify(mock =>
            mock.ChangeStatusAsync(It.IsAny<Guid>(), It.Is<string>(status => status == "Completed" || status == "Failed"), It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task CallLLMAndPersistReviewReportAsync_WhenCalled_ChangesStatusToProcessing()
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());

        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LLMResponseDto());

        // Act
        await _reviewService.CallLLMAndPersistReviewReportAsync(new Guid(), CancellationToken.None);


        // Assert
        _fakeReviewRepository.Verify(mock =>
            mock.ChangeStatusAsync(It.IsAny<Guid>(), "Processing", It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CallLLMAndPersistReviewReportAsync_WhenWebhookIsNullOrWhiteSpace_DoesNotSendNotification(string? webhookUrl)
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review()
            {
                WebhookUrl = webhookUrl
            });


        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LLMResponseDto());


        // Act
        await _reviewService.CallLLMAndPersistReviewReportAsync(new Guid(), CancellationToken.None);


        // Assert
        _fakeNotificationService.Verify(mock =>
            mock.SendAsync(It.IsAny<NotificationMessageDto?>()!, It.IsAny<Uri>(), It.IsAny<CancellationToken>()), 
            Times.Never);
    }


    [Fact]
    public async Task CallLLMAndPersistReviewReportAsync_WhenWebhookIsValidUrl_SendNotification()
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review()
            {
                WebhookUrl = "https://webhook.site/8ad81a9b-098d-49f8-893b-e1351e362ad7"
            });


        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LLMResponseDto());


        // Act
        await _reviewService.CallLLMAndPersistReviewReportAsync(new Guid(), CancellationToken.None);


        // Assert
        _fakeNotificationService.Verify(mock =>
            mock.SendAsync(It.IsAny<NotificationMessageDto?>()!, It.IsAny<Uri>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task CallLLMAndPersistReviewReportAsync_WhenWebhookIsInvalidUrl_DoesNotSendNotification()
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review()
            {
                WebhookUrl = "any_invalid_webhook_url"
            });


        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LLMResponseDto());


        // Act
        await _reviewService.CallLLMAndPersistReviewReportAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        _fakeNotificationService.Verify(mock =>
            mock.SendAsync(It.IsAny<NotificationMessageDto?>()!, It.IsAny<Uri>(), It.IsAny<CancellationToken>()),
            Times.Never);
        // The catch block swallows the exception that happened due to to invalid Url
    }


    [Fact]
    public async Task CallLLMAndPersistReviewReportAsync_WhenLLMClientReturnsNull_ChangesStatusToFailed()
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());

        // Setup without "ReturnsAsync" sets up the method to return default value for the retrun type (which is null here)
        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));


        // Act
        await _reviewService.CallLLMAndPersistReviewReportAsync(Guid.NewGuid(), CancellationToken.None);


        // Assert
        _fakeReviewRepository.Verify(mock =>
            mock.ChangeStatusAsync(It.IsAny<Guid>(), "Failed", It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task CallLLMAndPersistReviewReportAsync_WhenLLMClientThrowsHttpRequestException_ThrowsHttpRequestExceptionAndChangeStatusToFailed()
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());

        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException());


        // Act and Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await _reviewService.CallLLMAndPersistReviewReportAsync(Guid.NewGuid(), CancellationToken.None);        // due to rethrowing
        });


        // Assert
        _fakeReviewRepository.Verify(mock =>
            mock.ChangeStatusAsync(It.IsAny<Guid>(), "Failed", It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task CallLLMAndPersistReviewReportAsync_WhenLLMRetrunsSuccessfully_ChangesStatusToCompleted()
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());


        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LLMResponseDto()
            {
                SuccessfulRequest = true
            });


        // Act
        await _reviewService.CallLLMAndPersistReviewReportAsync(Guid.NewGuid(), CancellationToken.None);


        // Assert
        _fakeReviewRepository.Verify(mock =>
            mock.ChangeStatusAsync(It.IsAny<Guid>(), "Completed", It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task CallLLMAndPersistReviewReportAsync_WhenLLMRetrunsFailed_ChangesStatusToFailed()
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());

        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LLMResponseDto()
            {
                SuccessfulRequest = false
            });


        // Act
        await _reviewService.CallLLMAndPersistReviewReportAsync(Guid.NewGuid(), CancellationToken.None);


        // Assert
        _fakeReviewRepository.Verify(mock =>
            mock.ChangeStatusAsync(It.IsAny<Guid>(), "Failed", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
