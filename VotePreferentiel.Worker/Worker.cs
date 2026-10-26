using VotePreferentiel;

namespace VotePreferentiel.Worker;

public sealed class Worker(
    IVoteProvider voteProvider,
    IVoteCounter voteCounter,
    IHostApplicationLifetime applicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var voteModel = await voteProvider.GetVotesAsync(stoppingToken);

        voteCounter.CandidatesEliminated += (round, eliminatedCandidates) =>
        {
            var eliminated = string.Join(
                ", ",
                eliminatedCandidates
                    .OrderBy(candidate => candidate.Key, StringComparer.Ordinal)
                    .Select(candidate => $"{candidate.Key} ({candidate.Value} voix)"));
            Console.WriteLine($"Tour {round} : candidat(s) éliminé(s) : {eliminated}.");
        };

        var winner = voteCounter.GetWinner(voteModel);

        Console.WriteLine($"La personne gagnante est : {winner}");
        applicationLifetime.StopApplication();
    }
}
