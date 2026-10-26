namespace VotePreferentiel;

public sealed class VoteModel
{
    public List<VoterVote> Votes { get; init; } = [];
}

public sealed class VoterVote
{
    public List<string> Choices { get; init; } = [];
}
