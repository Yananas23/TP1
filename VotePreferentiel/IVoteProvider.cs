namespace VotePreferentiel;

public interface IVoteProvider
{
    Task<VoteModel> GetVotesAsync(CancellationToken cancellationToken = default);
}
