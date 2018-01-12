using Common;
using System;
using System.Collections.Generic;
using System.Text;

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
