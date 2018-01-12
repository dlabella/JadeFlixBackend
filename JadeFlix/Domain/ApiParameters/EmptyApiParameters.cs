using System;
using System.Collections.Generic;
using System.Text;

namespace JadeFlix.Domain.ApiParameters
{
    public class EmptyApiParameters : ApiParamtersBase
    {
        public override bool AreValid => true;
    }
}
