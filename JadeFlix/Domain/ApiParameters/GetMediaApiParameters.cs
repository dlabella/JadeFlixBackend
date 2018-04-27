namespace JadeFlix.Domain.ApiParameters
{
    public class GetMediaApiParameters : ApiParamtersBase
    {
        public string ScraperId { get; set; }
        public string Url { get; set; }

        public override bool AreValid
        {
            get
            {
                return !(string.IsNullOrEmpty(ScraperId) ||
                    string.IsNullOrEmpty(Url));
            }
        }
    }
}
