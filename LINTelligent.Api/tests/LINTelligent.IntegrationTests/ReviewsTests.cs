using FluentAssertions;
using LINTelligent.Presentation.DTOs.Request;
using LINTelligent.Presentation.DTOs.Response;
using System.Net;
using System.Net.Http.Json;

namespace LINTelligent.IntegrationTests;

public class ReviewsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _httpClient;

    public ReviewsTests(CustomWebApplicationFactory factory)
    {
        // the client by which I will communicate with the running application instance during testing
        // It has the application base url by default
        _httpClient = factory.CreateClient();
    }


    [Fact]
    public async Task ReviewCodeSnippetAsync_WhenValidInputData_Returns202Accepted()
    {
        // Arrange
        var request = new CodeReviewRequestDto()
        {
            CodeSnippet = "console.log('hello')",
            Language = "JS"
        };


        // Act
        var response = await _httpClient.PostAsJsonAsync("/reviews", request);


        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response.Headers.Location.Should().NotBeNull();
    }


    [Fact]
    public async Task ReviewCodeSnippetAsync_WhenInvalidInputData_Returns400BadRequest()
    {
        // Arrange
        var request = new CodeReviewRequestDto()        // Language is considered as mandatory, due to ApiController attribute
        {
            CodeSnippet = "console.log('hello')"
        };


        // Act
        var response = await _httpClient.PostAsJsonAsync("/reviews", request);


        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task ReviewCodeSnippetAsync_WhenGitHabRawFileCodeSnippetExceeds5000Characters_PersistsReviewStatusAsFailed()
    {
        // Arrange
        var request = new CodeReviewRequestDto()
        {
            GitHubUserContentFileUrl = "https://raw.githubusercontent.com/mazenaly256/Cinema-Seat-Reservation-System/refs/heads/main/services/reservation-service/src/Services/Implementations/ReservationService.cs",
            Language = "C#"
        };

        using var responsePostRequest = await _httpClient.PostAsJsonAsync("/reviews", request);
        var newReviewLocation = responsePostRequest.Headers.Location;


        // Act
        var timeout = TimeSpan.FromSeconds(10);
        var start = DateTime.UtcNow;
        bool statusAssertedAsFailed = false;

        while (DateTime.UtcNow - start < timeout)
        {
            using var response = await _httpClient.GetAsync(newReviewLocation);

            var review = await response.Content.ReadFromJsonAsync<CodeReviewResponseDto>();

            if (review?.Status == "Failed")
            {
                statusAssertedAsFailed = true;
                break;
            }

            await Task.Delay(2000);
        }


        //Assert
        statusAssertedAsFailed.Should().Be(true);
    }


    [Fact]
    public async Task GetReviewByIdAsync_WhenReviewIdExists_Returns200Ok()
    {
        // Arrange
        var request = new CodeReviewRequestDto()
        {
            CodeSnippet = "console.log('hello')",
            Language = "JS"
        };

        var newReviewLocation = (await _httpClient.PostAsJsonAsync("/reviews", request)).Headers.Location;

        // Act
        var response = await _httpClient.GetAsync(newReviewLocation);


        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }


    [Fact]
    public async Task GetReviewByIdAsync_WhenReviewIdDoesNotExist_Returns404NotFound()
    {
        // Arrange
        


        // Act
        var response = await _httpClient.GetAsync($"/reviews/{Guid.NewGuid()}");


        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
