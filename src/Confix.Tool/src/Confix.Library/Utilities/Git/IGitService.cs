namespace Confix.Utilities;

public interface IGitService
{
    Task SparseCheckoutAsync(
        GitSparseCheckoutConfiguration configuration,
        CancellationToken cancellationToken);

    Task<string> ShowRefsAsync(
        GitShowRefsConfiguration configuration,
        CancellationToken cancellationToken);

    Task CheckoutAsync(
        GitCheckoutConfiguration configuration,
        CancellationToken cancellationToken);

    Task CloneAsync(
        GitCloneConfiguration configuration,
        CancellationToken cancellationToken);

    Task PullAsync(
        GitPullConfiguration configuration,
        CancellationToken cancellationToken);

    Task AddAsync(
        GitAddConfiguration configuration,
        CancellationToken cancellationToken);

    Task CommitAsync(
        GitCommitConfiguration configuration,
        CancellationToken cancellationToken);

    Task PushAsync(
        GitPushConfiguration configuration,
        CancellationToken cancellationToken);

    Task<GitGetRepoInfoResult?> GetRepoInfoAsync(
        GitGetRepoInfoConfiguration configuration,
        CancellationToken cancellationToken);

    Task<string?> GetBranchAsync(
        GitGetBranchConfiguration configuration,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<string>?> GetTagsAsync(
        GitGetTagsConfiguration configuration,
        CancellationToken cancellationToken);

    Task<string?> GetRootAsync(
        GitGetRootConfiguration configuration,
        CancellationToken cancellationToken);

    Task<string?> GetOriginUrlAsync(
        GitGetOriginUrlConfiguration configuration,
        CancellationToken cancellationToken);
}
