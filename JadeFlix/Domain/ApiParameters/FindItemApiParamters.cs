using System;
using System.Collections.Generic;
using System.Text;

namespace JadeFlix.Domain.ApiParameters
{
    public class FindItemApiParamters : ApiParamtersBase
    {
        public string ScraperId { get; set; }
        public string Name { get; set; }

        public override bool AreValid
        {
            get
            {
                return !(string.IsNullOrEmpty(ScraperId) || string.IsNullOrEmpty(Name));
            }
        }
    }
}
