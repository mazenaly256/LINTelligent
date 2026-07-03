using LINTelligent.Application.DTOs;
using LINTelligent.Application.Services.Implementations;
using LINTelligent.Application.Services.Interfaces;
using LINTelligent.Infrastructure.LLMClients.Interfaces;
using LINTelligent.Infrastructure.Persistence.Repositories.Interfaces;
using Moq;

namespace LINTelligent.UnitTests;

public class ReviewServiceTests
{
    [Fact]
    public async Task RequestProcessingAsync_WhenCalledWithNullOrWhiteSpaceWebhook_CallAllRequiredMethodsWithoutNotificationSending()
    {
        // Arrange
        var fakeReviewRepository = new Mock<IReviewRepository>();

        fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());



        var fakeLLMClient = new Mock<ILLMClient>();

        fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Infrastructure.DTOs.LLMResponse());



        var fakeNotificationService = new Mock<INotificationService>();

       

        var reviewService = new ReviewService(fakeReviewRepository.Object, fakeLLMClient.Object, fakeNotificationService.Object);

        // Act
        await reviewService.RequestProcessingAsync(new Guid(), "", "", null);


        // Assert
        fakeReviewRepository.Verify(mock =>
            mock.ChangeStatusAsync(It.IsAny<Guid>(), "Processing", It.IsAny<CancellationToken>()),
            Times.Once);

        fakeLLMClient.Verify(mock =>
            mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);

        fakeReviewRepository.Verify(mock =>
            mock.AddReportToTheReviewAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);

        fakeReviewRepository.Verify(mock =>
            mock.ChangeStatusAsync(It.IsAny<Guid>(), It.Is<string>(status => status == "Completed" || status == "Failed"), It.IsAny<CancellationToken>()),
            Times.Once);

        fakeNotificationService.Verify(mock =>
            mock.SendAsync(It.IsAny<NotificationMessageDto?>()!, It.IsAny<Uri>(), It.IsAny<CancellationToken>()), 
            Times.Never);

    }

    [Fact]
    public async Task RequestProcessingAsync_WhenCalledWithValidWebhook_CallAllRequiredMethodsAndNotificationSendAsyncMethod()
    {
        // Arrange
        var fakeReviewRepository = new Mock<IReviewRepository>();

        fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());



        var fakeLLMClient = new Mock<ILLMClient>();

        fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Infrastructure.DTOs.LLMResponse());



        var fakeNotificationService = new Mock<INotificationService>();

       

        var reviewService = new ReviewService(fakeReviewRepository.Object, fakeLLMClient.Object, fakeNotificationService.Object);

        // Act
        await reviewService.RequestProcessingAsync(new Guid(), "", "", "https://webhook.site/8ad81a9b-098d-49f8-893b-e1351e362ad7");


        // Assert
        fakeReviewRepository.Verify(mock =>
            mock.ChangeStatusAsync(It.IsAny<Guid>(), "Processing", It.IsAny<CancellationToken>()),
            Times.Once);

        fakeLLMClient.Verify(mock =>
            mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);

        fakeReviewRepository.Verify(mock =>
            mock.AddReportToTheReviewAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);

        fakeReviewRepository.Verify(mock =>
            mock.ChangeStatusAsync(It.IsAny<Guid>(), It.Is<string>(status => status == "Completed" || status == "Failed"), It.IsAny<CancellationToken>()),
            Times.Once);

        fakeNotificationService.Verify(mock =>
            mock.SendAsync(It.IsAny<NotificationMessageDto?>()!, It.IsAny<Uri>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RequestProcessingAsync_WhenCalledWithInvalidWebhook_ThrowsUriFormatException()
    {
        // Arrange
        var fakeReviewRepository = new Mock<IReviewRepository>();

        fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());



        var fakeLLMClient = new Mock<ILLMClient>();

        fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Infrastructure.DTOs.LLMResponse());



        var fakeNotificationService = new Mock<INotificationService>();

       

        var reviewService = new ReviewService(fakeReviewRepository.Object, fakeLLMClient.Object, fakeNotificationService.Object);

        // Act and Assert
        await Assert.ThrowsAsync<UriFormatException>(async () =>
        {
            await reviewService.RequestProcessingAsync(new Guid(), "", "", "any_invalid_webhook");
        });
    }

    [Fact]
    public async Task RequestProcessingAsync_WhenLLMRetrunsSuccessfulResponse_CallChangeStatusAsyncWithCompleted()
    {
        // Arrange
        var fakeReviewRepository = new Mock<IReviewRepository>();

        fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());



        var fakeLLMClient = new Mock<ILLMClient>();

        fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Infrastructure.DTOs.LLMResponse()
            {
                SuccessfulRequest = true
            });



        var fakeNotificationService = new Mock<INotificationService>();

       

        var reviewService = new ReviewService(fakeReviewRepository.Object, fakeLLMClient.Object, fakeNotificationService.Object);

        // Act
        await reviewService.RequestProcessingAsync(new Guid(), "", "", null);


        // Assert
        fakeReviewRepository.Verify(frr =>
            frr.ChangeStatusAsync(It.IsAny<Guid>(), "Completed", It.IsAny<CancellationToken>()),
            Times.Once);

    }

    [Fact]
    public async Task RequestProcessingAsync_WhenLLMRetrunsFailedResponse_CallChangeStatusAsyncWithFailed()
    {
        // Arrange
        var fakeReviewRepository = new Mock<IReviewRepository>();

        fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());



        var fakeLLMClient = new Mock<ILLMClient>();

        fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Infrastructure.DTOs.LLMResponse()
            {
                SuccessfulRequest = false
            });



        var fakeNotificationService = new Mock<INotificationService>();

       

        var reviewService = new ReviewService(fakeReviewRepository.Object, fakeLLMClient.Object, fakeNotificationService.Object);

        // Act
        await reviewService.RequestProcessingAsync(new Guid(), "", "", null);


        // Assert
        fakeReviewRepository.Verify(frr =>
            frr.ChangeStatusAsync(It.IsAny<Guid>(), "Failed", It.IsAny<CancellationToken>()),
            Times.Once);

    }
}
