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
    public async Task RequestProcessingAsync_WhenCalled_ChangeStatusToProcessing()
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
        await reviewService.RequestProcessingAsync(new Guid(), "", "", "");


        // Assert
        fakeReviewRepository.Verify(mock =>
            mock.ChangeStatusAsync(It.IsAny<Guid>(), "Processing", It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task RequestProcessingAsync_WhenCalled_ChangeStatusToCompletedOrFailed()
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
        await reviewService.RequestProcessingAsync(new Guid(), "", "", "");


        // Assert
        fakeReviewRepository.Verify(mock =>
            mock.ChangeStatusAsync(It.IsAny<Guid>(), It.Is<string>(status => status == "Completed" || status == "Failed"), It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task RequestProcessingAsync_WhenCalled_ChangeStatusToProcessingBeforeToCompletedOrFailedInOrder()
    {
        // Arrange
        var fakeReviewRepository = new Mock<IReviewRepository>();

        fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());

        List<string> returnedStatuses = new();

        fakeReviewRepository.Setup(mock => mock.ChangeStatusAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, string, CancellationToken>((_, status, _) =>
            {
                returnedStatuses.Add(status);
            })
            .Returns(Task.CompletedTask);

        var fakeLLMClient = new Mock<ILLMClient>();

        fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Infrastructure.DTOs.LLMResponse());



        var fakeNotificationService = new Mock<INotificationService>();



        var reviewService = new ReviewService(fakeReviewRepository.Object, fakeLLMClient.Object, fakeNotificationService.Object);



        // Act
        await reviewService.RequestProcessingAsync(new Guid(), "", "", "");


        // Assert
        Assert.Equal(2, returnedStatuses.Count);
        Assert.Equal("Processing", returnedStatuses[0]);
        Assert.True(returnedStatuses[1] == "Completed" || returnedStatuses[1] == "Failed");

    }


    [Fact]
    public async Task RequestProcessingAsync_WhenLLMReturnsTheReviewReport_SavesReportToDatabase()
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
        await reviewService.RequestProcessingAsync(new Guid(), "", "", "");


        // Assert
        fakeReviewRepository.Verify(mock =>
            mock.AddReportToTheReviewAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task RequestProcessingAsync_WhenWebhookIsNullOrWhiteSpace_DoesNotSendNotification(string? webhookUrl)
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
        await reviewService.RequestProcessingAsync(new Guid(), "", "", webhookUrl);


        // Assert
        fakeNotificationService.Verify(mock =>
            mock.SendAsync(It.IsAny<NotificationMessageDto?>()!, It.IsAny<Uri>(), It.IsAny<CancellationToken>()), 
            Times.Never);

    }


    [Fact]
    public async Task RequestProcessingAsync_WhenWebhookIsValidUrl_SendNotification()
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
        fakeNotificationService.Verify(mock =>
            mock.SendAsync(It.IsAny<NotificationMessageDto?>()!, It.IsAny<Uri>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task RequestProcessingAsync_WhenWebhookIsInvalidUrl_ThrowsUriFormatException()
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
    public async Task RequestProcessingAsync_WhenLLMClientReturnsNull_ThrowsNullReferenceExceptionAndChangeStatusToFailed()
    {
        // Arrange
        var fakeReviewRepository = new Mock<IReviewRepository>();

        fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());


        var fakeLLMClient = new Mock<ILLMClient>();

        // Setup without "ReturnsAsync" sets up the method to return default value for the retrun type (which is null here)
        fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));


        var fakeNotificationService = new Mock<INotificationService>();


        var reviewService = new ReviewService(fakeReviewRepository.Object, fakeLLMClient.Object, fakeNotificationService.Object);

        // Act and Assert
        await Assert.ThrowsAsync<NullReferenceException>(async () =>
        {
            await reviewService.RequestProcessingAsync(new Guid(), "", "", null);
        });

        // Assert
        fakeReviewRepository.Verify(frr =>
            frr.ChangeStatusAsync(It.IsAny<Guid>(), "Failed", It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task RequestProcessingAsync_WhenLLMClientThrowsHttpRequestException_ThrowsHttpRequestExceptionAndChangeStatusToFailed()
    {
        // Arrange
        var fakeReviewRepository = new Mock<IReviewRepository>();

        fakeReviewRepository.Setup(mock => mock.GetReviewByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());


        var fakeLLMClient = new Mock<ILLMClient>();

        fakeLLMClient.Setup(mock => mock.GetCodeReviewReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException());


        var fakeNotificationService = new Mock<INotificationService>();


        var reviewService = new ReviewService(fakeReviewRepository.Object, fakeLLMClient.Object, fakeNotificationService.Object);

        // Act and Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await reviewService.RequestProcessingAsync(new Guid(), "", "", null);
        });

        // Assert
        fakeReviewRepository.Verify(frr =>
            frr.ChangeStatusAsync(It.IsAny<Guid>(), "Failed", It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task RequestProcessingAsync_WhenLLMRetrunsSuccessfulResponse_ChangesStatusToCompleted()
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
    public async Task RequestProcessingAsync_WhenLLMRetrunsFailedResponse_ChangesStatusToFailed()
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
