using FluentAssertions;
using LINTelligent.Presentation.DTOs.Request;
using System.Net;
using System.Net.Http.Json;
using System.Reflection.PortableExecutable;

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
    public async Task GetReviewByIdAsync_WhenReviewIdExists_Returns200Ok()
    {
        // Arrange
        var request = new CodeReviewRequestDto()        // Language is considered as mandatory, due to ApiController attribute auto-validates the model (ApiController filter)
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
