using Newtonsoft.Json;

namespace AzureCloner.Data
{
    public class DevOpsRepo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        [JsonProperty("project")] public DevOpsProject DevOpsProject { get; set; }
        public string DefaultBranch { get; set; }
        public long Size { get; set; }
        public string RemoteUrl { get; set; }
        public string SshUrl { get; set; }
        public string WebUrl { get; set; }
    }
}
