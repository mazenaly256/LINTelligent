namespace LINTelligent.Services.Interfaces;

public interface ILLMClient
{
    public string GetCodeReview(string language, string codeSnippet);
}
