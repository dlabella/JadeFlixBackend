namespace JadeFlix.Domain.ApiParameters
{
    public class GetRecentApiParameters : ApiParamtersBase
    {
        public string ScraperId { get; set; }
        public override bool AreValid
        {
            get
            {
                return !(string.IsNullOrEmpty(ScraperId));
            }
        }
    }
}
