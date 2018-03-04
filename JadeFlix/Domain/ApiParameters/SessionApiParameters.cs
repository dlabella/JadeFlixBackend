using System;
using System.Collections.Generic;
using System.Text;

namespace JadeFlix.Domain.ApiParameters
{
    public class SessionApiParams : ApiParamtersBase
    {
        public string SessionId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public override bool AreValid {
            get { return !string.IsNullOrEmpty(SessionId); }
        }
    }
}
