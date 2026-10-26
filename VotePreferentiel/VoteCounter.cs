namespace VotePreferentiel;

public sealed class VoteCounter : IVoteCounter
{
    public event Action<int, IReadOnlyDictionary<string, int>>? CandidatesEliminated;

    public HashSet<string> GetCandidates(VoteModel voteModel)
    {
        ValidateBallots(voteModel);
        return voteModel.Votes
            .SelectMany(vote => vote.Choices)
            .ToHashSet(StringComparer.Ordinal);
    }

    public string GetWinner(VoteModel voteModel)
    {
        var candidates = GetCandidates(voteModel);
        if (candidates.Count == 0)
        {
            throw new ArgumentException("Au moins un vote requis.", nameof(voteModel));
        }

        var firstChoiceCounts = CountFirstChoices(GetAllFirstChoices(voteModel));
        var activeCandidates = new HashSet<string>(candidates, StringComparer.Ordinal);
        var round = 0;

        while (activeCandidates.Count > 1)
        {
            round++;
            var counts = activeCandidates.ToDictionary(
                candidate => candidate,
                candidate => firstChoiceCounts.TryGetValue(candidate, out var count) ? count : 0,
                StringComparer.Ordinal);

            var candidatesToEliminate = GetCandidatesWithLeastVotes(counts);
            if (candidatesToEliminate.Length == activeCandidates.Count)
            {
                throw new InvalidOperationException(
                    "Exaeqo, aucun gagnant.");
            }

            var eliminatedCounts = candidatesToEliminate.ToDictionary(
                candidate => candidate,
                candidate => counts[candidate],
                StringComparer.Ordinal);
            CandidatesEliminated?.Invoke(round, eliminatedCounts);

            activeCandidates.ExceptWith(candidatesToEliminate);
            if (activeCandidates.Count > 1)
            {
                firstChoiceCounts = CountActivePreferences(voteModel, activeCandidates);
            }
        }

        return activeCandidates.Single();
    }

    public List<string> GetAllFirstChoices(VoteModel voteModel)
    {
        ValidateBallots(voteModel);
        return voteModel.Votes.Select(vote => vote.Choices[0]).ToList();
    }

    public IDictionary<string, int> CountFirstChoices(List<string> firstChoices)
    {
        ArgumentNullException.ThrowIfNull(firstChoices);
        return firstChoices
            .GroupBy(choice => choice, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
    }

    public string[] GetCandidatesWithLeastVotes(IDictionary<string, int> firstChoiceCounts)
    {
        ArgumentNullException.ThrowIfNull(firstChoiceCounts);
        if (firstChoiceCounts.Count == 0)
        {
            throw new ArgumentException("Au moins un candidat est attendu.", nameof(firstChoiceCounts));
        }

        var minimumVotes = firstChoiceCounts.Values.Min();
        return firstChoiceCounts
            .Where(candidate => candidate.Value == minimumVotes)
            .Select(candidate => candidate.Key)
            .ToArray();
    }

    private static IDictionary<string, int> CountActivePreferences(
        VoteModel voteModel,
        HashSet<string> activeCandidates)
    {
        var counts = activeCandidates.ToDictionary(
            candidate => candidate,
            _ => 0,
            StringComparer.Ordinal);

        foreach (var vote in voteModel.Votes)
        {
            var choice = vote.Choices.FirstOrDefault(activeCandidates.Contains);
            if (choice is not null)
            {
                counts[choice]++;
            }
        }

        return counts;
    }

    private static void ValidateBallots(VoteModel voteModel)
    {
        ArgumentNullException.ThrowIfNull(voteModel);
        if (voteModel.Votes is null)
        {
            throw new ArgumentException("La liste ne peut-être vide.", nameof(voteModel));
        }

        if (voteModel.Votes.Any(vote => vote is null || vote.Choices is null || vote.Choices.Count == 0))
        {
            throw new ArgumentException("Chaque vote doit avoir au moins un choix.", nameof(voteModel));
        }

        if (voteModel.Votes.Any(vote => vote.Choices.Any(string.IsNullOrWhiteSpace)))
        {
            throw new ArgumentException("Un choix ne peut-être vide.", nameof(voteModel));
        }
    }
}
