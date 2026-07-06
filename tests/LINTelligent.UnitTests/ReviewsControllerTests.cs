using LINTelligent.Application.Contracts.DTOs;
using LINTelligent.Application.Services.Interfaces;
using LINTelligent.Presentation.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace LINTelligent.UnitTests;

public class ReviewsControllerTests
{
    private Mock<IReviewService> _fakeReviewService;
    private ReviewsController _reviewsController;

    public ReviewsControllerTests()
    {
        _fakeReviewService = new();
        _reviewsController = new(_fakeReviewService.Object);
    }

    [Fact]
    public async Task ReviewCodeSnippetAsync_WhenCodeSnippetLengthExceeds500Charaters_Returns400BadRequest()
    {
        // Arrange



        // Act
        var result = await _reviewsController.ReviewCodeSnippetAsync(new()
        {
            CodeSnippet = new string('a', 600)
        }, CancellationToken.None);


        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }


    [Fact]
    public async Task ReviewCodeSnippetAsync_WhenValidRequest_Returns202Accepted()
    {
        // Arrange


        // Act
        var result = await _reviewsController.ReviewCodeSnippetAsync(new()
        {
            CodeSnippet = new string('a', 200),
            Language = "any_string"
        }, CancellationToken.None);


        // Assert
        _fakeReviewService.Verify(mock => mock.SubmitReviewRequestAsync(It.IsAny<NewReviewRequestDto>(), It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.IsType<AcceptedResult>(result);
    }


    [Fact]
    public async Task GetReviewByIdAsync_WhenReviewDoesNotExist_Returns404NotFound()
    {
        // Arrange

        // GetReviewByIdAsync method through the fake object always returns null, no need to Setup.


        // Act
        var result = await _reviewsController.GetReviewByIdAsync(Guid.NewGuid(), CancellationToken.None);


        // Assert
        _fakeReviewService.Verify(mock => mock.GetReviewDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }


    [Fact]
    public async Task GetReviewByIdAsync_WhenReviewExists_Returns200Ok()
    {
        // Arrange
        _fakeReviewService.Setup(mock => mock.GetReviewDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Review());

        // Act
        var result = await _reviewsController.GetReviewByIdAsync(Guid.NewGuid(), CancellationToken.None);


        // Assert
        _fakeReviewService.Verify(mock => mock.GetReviewDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.IsType<OkObjectResult>(result.Result);
    }
}
