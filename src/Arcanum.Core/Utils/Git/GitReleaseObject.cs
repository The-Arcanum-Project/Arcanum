namespace Arcanum.Core.Utils.Git;

public class GitReleaseObject : GitDataObjectBase
{
   public ReleaseData? Data { get; set; }
   public override bool IsDataAvailable() => Data is not null;
}

public abstract class GitDataObjectBase
{
   public string DataKey { get; init; } = string.Empty;
   public TimeSpan FetchInterval { get; set; } = TimeSpan.FromDays(1);
   public string RepositoryOwner { get; init; } = "Minnator";
   public string RepositoryName { get; init; } = "Arcanum";
   public string RepositoryInternalUrl { get; init; } = string.Empty;
   public DateTime LastFetch { get; set; } = DateTime.MinValue;

   public abstract bool IsDataAvailable();
   public bool IsDataOutdated => LastFetch + FetchInterval < DateTime.Now;
}

public class ReleaseData
{
   public string Name { get; set; } = string.Empty;
   public string TagName { get; set; } = string.Empty;
   public string Body { get; set; } = string.Empty;
}