using System.Net.Http.Json;

namespace VotePreferentiel;

public sealed class VoteProvider(HttpClient httpClient) : IVoteProvider
{
    public const string VotesUrl =
        "https://raw.githubusercontent.com/JLou/dotnet-course/main/eval-1/votes.example.json";

    public async Task<VoteModel> GetVotesAsync(CancellationToken cancellationToken = default)
    {
        var voteModel = await httpClient.GetFromJsonAsync<VoteModel>(VotesUrl, cancellationToken);
        return voteModel ?? throw new InvalidDataException("Impossible de récupérer les données JSON");
    }
}
