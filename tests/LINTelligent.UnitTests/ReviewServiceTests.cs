using LINTelligent.Application.DTOs;
using LINTelligent.Application.Services.Implementations;
using LINTelligent.Application.Services.Interfaces;
using LINTelligent.Infrastructure.LLMClients.Interfaces;
using LINTelligent.Infrastructure.Persistence.Repositories.Interfaces;
using Moq;

namespace LINTelligent.UnitTests;

public class ReviewServiceTests
{
    private Mock<IReviewRepository> _fakeReviewRepository;
    private Mock<ILLMClient> _fakeLLMClient;
    private Mock<INotificationService> _fakeNotificationService;
    private ReviewService _reviewService;

    public ReviewServiceTests()
    {
        _fakeReviewRepository = new();
        _fakeLLMClient = new();
        _fakeNotificationService = new();
        _reviewService = new(_fakeReviewRepository.Object, _fakeLLMClient.Object, _fakeNotificationService.Object);
    }


    [Fact]
    public async Task RequestProcessingAsync_WhenCalled_ChangesStatusToProcessing()
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());

        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Infrastructure.DTOs.LLMResponse());

        // Act
        await _reviewService.RequestProcessingAsync(new Guid(), "", "", "");


        // Assert
        _fakeReviewRepository.Verify(mock =>
            mock.ChangeStatusAsync(It.IsAny<Guid>(), "Processing", It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task RequestProcessingAsync_WhenCalled_ChangesStatusToCompletedOrFailed()
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());


        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Infrastructure.DTOs.LLMResponse());


        // Act
        await _reviewService.RequestProcessingAsync(new Guid(), "", "", "");


        // Assert
        _fakeReviewRepository.Verify(mock =>
            mock.ChangeStatusAsync(It.IsAny<Guid>(), It.Is<string>(status => status == "Completed" || status == "Failed"), It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task RequestProcessingAsync_WhenCalled_ChangesStatusToProcessingBeforeToCompletedOrFailedInOrder()
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
            .ReturnsAsync(new Infrastructure.DTOs.LLMResponse());


        // Act
        await _reviewService.RequestProcessingAsync(new Guid(), "", "", "");


        // Assert
        Assert.Equal(2, returnedStatuses.Count);
        Assert.Equal("Processing", returnedStatuses[0]);
        Assert.True(returnedStatuses[1] == "Completed" || returnedStatuses[1] == "Failed");

    }


    [Fact]
    public async Task RequestProcessingAsync_WhenLLMReturnsTheReviewReport_SavesReportToDatabase()
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());

        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Infrastructure.DTOs.LLMResponse());


        // Act
        await _reviewService.RequestProcessingAsync(new Guid(), "", "", "");


        // Assert
        _fakeReviewRepository.Verify(mock =>
            mock.AddReportToTheReviewAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RequestProcessingAsync_WhenWebhookIsNullOrWhiteSpace_DoesNotSendNotification(string? webhookUrl)
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());


        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Infrastructure.DTOs.LLMResponse());
       

        // Act
        await _reviewService.RequestProcessingAsync(new Guid(), "", "", webhookUrl);


        // Assert
        _fakeNotificationService.Verify(mock =>
            mock.SendAsync(It.IsAny<NotificationMessageDto?>()!, It.IsAny<Uri>(), It.IsAny<CancellationToken>()), 
            Times.Never);

    }


    [Fact]
    public async Task RequestProcessingAsync_WhenWebhookIsValidUrl_SendNotification()
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());


        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Infrastructure.DTOs.LLMResponse());
       

        // Act
        await _reviewService.RequestProcessingAsync(new Guid(), "", "", "https://webhook.site/8ad81a9b-098d-49f8-893b-e1351e362ad7");


        // Assert
        _fakeNotificationService.Verify(mock =>
            mock.SendAsync(It.IsAny<NotificationMessageDto?>()!, It.IsAny<Uri>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task RequestProcessingAsync_WhenWebhookIsInvalidUrl_DoesNotSendNotification()
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());


        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Infrastructure.DTOs.LLMResponse());


        // Act
        await _reviewService.RequestProcessingAsync(new Guid(), "", "", "any_invalid_webhook");

        // Assert
        _fakeNotificationService.Verify(mock =>
            mock.SendAsync(It.IsAny<NotificationMessageDto?>()!, It.IsAny<Uri>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }


    [Fact]
    public async Task RequestProcessingAsync_WhenLLMClientReturnsNull_ThrowsNullReferenceExceptionAndChangesStatusToFailed()
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());



        // Setup without "ReturnsAsync" sets up the method to return default value for the retrun type (which is null here)
        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));



        // Act and Assert
        await Assert.ThrowsAsync<NullReferenceException>(async () =>
        {
            await _reviewService.RequestProcessingAsync(new Guid(), "", "", null);
        });

        // Assert
        _fakeReviewRepository.Verify(frr =>
            frr.ChangeStatusAsync(It.IsAny<Guid>(), "Failed", It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task RequestProcessingAsync_WhenLLMClientThrowsHttpRequestException_ThrowsHttpRequestExceptionAndChangeStatusToFailed()
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());



        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException());


        // Act and Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await _reviewService.RequestProcessingAsync(new Guid(), "", "", null);
        });

        // Assert
        _fakeReviewRepository.Verify(frr =>
            frr.ChangeStatusAsync(It.IsAny<Guid>(), "Failed", It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task RequestProcessingAsync_WhenLLMRetrunsSuccessfulResponse_ChangesStatusToCompleted()
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());


        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Infrastructure.DTOs.LLMResponse()
            {
                SuccessfulRequest = true
            });
       

        // Act
        await _reviewService.RequestProcessingAsync(new Guid(), "", "", null);


        // Assert
        _fakeReviewRepository.Verify(frr =>
            frr.ChangeStatusAsync(It.IsAny<Guid>(), "Completed", It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task RequestProcessingAsync_WhenLLMRetrunsFailedResponse_ChangesStatusToFailed()
    {
        // Arrange
        _fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());

        _fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Infrastructure.DTOs.LLMResponse()
            {
                SuccessfulRequest = false
            });


        // Act
        await _reviewService.RequestProcessingAsync(new Guid(), "", "", null);


        // Assert
        _fakeReviewRepository.Verify(frr =>
            frr.ChangeStatusAsync(It.IsAny<Guid>(), "Failed", It.IsAny<CancellationToken>()),
            Times.Once);

    }
}
