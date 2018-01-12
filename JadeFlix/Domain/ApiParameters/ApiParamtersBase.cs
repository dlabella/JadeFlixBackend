using System;
using System.Collections.Generic;
using System.Text;

namespace JadeFlix.Domain.ApiParameters
{
    public abstract class ApiParamtersBase
    {
        public abstract bool AreValid { get; }
    }
}
