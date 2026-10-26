using VotePreferentiel;

namespace VotePreferentiel.Tests;

public sealed class VoteCounterTests
{
    private readonly VoteCounter _voteCounter = new();

    [Test]
    public async Task GetWinner_ReturnsChloeForExampleVotes()
    {
        var voteModel = CreateVoteModel(
            ["Alice", "Bilal", "Chloé"],
            ["Alice", "Chloé", "Bilal"],
            ["Bilal", "Chloé", "Alice"],
            ["Chloé", "Bilal", "Alice"],
            ["Chloé", "Alice", "Bilal"]);

        await Assert.That(_voteCounter.GetWinner(voteModel)).IsEqualTo("Chloé");
    }

    [Test]
    public async Task GetWinner_ReturnsTheOnlyCandidate()
    {
        var voteModel = CreateVoteModel(["Alice"]);

        await Assert.That(_voteCounter.GetWinner(voteModel)).IsEqualTo("Alice");
    }

    [Test]
    public async Task GetWinner_TransfersPreferencesAfterEliminations()
    {
        var voteModel = CreateVoteModel(
            ["Alice", "Bilal", "Chloé", "Diane"],
            ["Alice", "Bilal", "Chloé", "Diane"],
            ["Alice", "Bilal", "Chloé", "Diane"],
            ["Alice", "Bilal", "Chloé", "Diane"],
            ["Bilal", "Chloé", "Alice", "Diane"],
            ["Bilal", "Chloé", "Alice", "Diane"],
            ["Bilal", "Chloé", "Alice", "Diane"],
            ["Chloé", "Alice", "Bilal", "Diane"],
            ["Chloé", "Alice", "Bilal", "Diane"],
            ["Chloé", "Alice", "Bilal", "Diane"],
            ["Diane", "Chloé", "Bilal", "Alice"]);

        await Assert.That(_voteCounter.GetWinner(voteModel)).IsEqualTo("Chloé");
    }

    [Test]
    public async Task GetWinner_EliminatesCandidatesTiedInLastPlaceSimultaneously()
    {
        var voteModel = CreateVoteModel(
            ["Alice", "Chloé", "Bilal"],
            ["Bilal", "Chloé", "Alice"],
            ["Chloé", "Alice", "Bilal"],
            ["Chloé", "Bilal", "Alice"]);

        await Assert.That(_voteCounter.GetWinner(voteModel)).IsEqualTo("Chloé");
    }

    [Test]
    public async Task GetWinner_ReportsEliminatedCandidatesForEachRound()
    {
        var voteModel = CreateVoteModel(
            ["Alice", "Bilal", "Chloé"],
            ["Alice", "Chloé", "Bilal"],
            ["Bilal", "Chloé", "Alice"],
            ["Chloé", "Bilal", "Alice"],
            ["Chloé", "Alice", "Bilal"]);
        var eliminations = new List<(int Round, string Candidate, int Votes)>();

        _voteCounter.CandidatesEliminated += (round, candidates) =>
        {
            eliminations.AddRange(candidates.Select(candidate =>
                (round, candidate.Key, candidate.Value)));
        };

        _voteCounter.GetWinner(voteModel);

        await Assert.That(eliminations).IsEquivalentTo(
        [
            (1, "Bilal", 1),
            (2, "Alice", 2)
        ]);
    }

    [Test]
    public async Task GetWinner_ThrowsForAnEmptyBallot()
    {
        var voteModel = CreateVoteModel(Array.Empty<string>());

        await Assert.That(() => _voteCounter.GetWinner(voteModel)).ThrowsException();
    }

    private static VoteModel CreateVoteModel(params string[][] ballots) =>
        new()
        {
            Votes = ballots
                .Select(choices => new VoterVote { Choices = [.. choices] })
                .ToList()
        };
}
