using Common;

namespace JadeFlix.Domain.ApiParameters
{
    public class DownloadApiParameters : ApiParamtersBase
    {
        public string Id { get; set; }
        public string Group { get; set; }
        public string Kind { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string File { get; set; }
        public string FiletPath {
            get
            {
                var folder = System.IO.Path.Combine(AppContext.Config.MediaPath, Group, Kind, Name.ToSafeName());
                return System.IO.Path.Combine(folder, File.ToSafeName());
            }
        }
        public override bool AreValid
        {
            get
            {
                if (string.IsNullOrEmpty(Group) ||
                    string.IsNullOrEmpty(Kind) ||
                    string.IsNullOrEmpty(Url) ||
                    string.IsNullOrEmpty(File) ||
                    string.IsNullOrEmpty(Name))
                {
                    return false;
                }
                return true;
            }
        }
    }
}
