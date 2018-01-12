using Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace JadeFlix.Domain.ApiParameters
{
    public class GetLocalApiParameters : ApiParamtersBase
    {
        public string Kind { get; set; }
        public string Group { get; set; }

        public override bool AreValid
        {
            get
            {
                return !(string.IsNullOrEmpty(Kind) ||
                    string.IsNullOrEmpty(Group));
            }
        }
    }
}
