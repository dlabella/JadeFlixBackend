using Common;

namespace JadeFlix.Domain.ApiParameters
{
    public class GetItemApiParameters : ApiParamtersBase
    {
        public string ScraperId { get; set; }
        public string Kind { get; set; }
        public string Group { get; set; }
        public string NId { get; set; }
        public string UId { get; set; }
        public string Name => NId.DecodeFromBase64();
        public string Url => UId.DecodeFromBase64();
        public bool OnlyLocal { get; set; }
        public override bool AreValid
        {
            get
            {
                return !(string.IsNullOrEmpty(ScraperId) ||
                    string.IsNullOrEmpty(Kind) ||
                    string.IsNullOrEmpty(Group) ||
                    string.IsNullOrEmpty(NId) ||
                    string.IsNullOrEmpty(UId));
            }
        }
    }
}
