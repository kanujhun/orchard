﻿using Orchard.Core.Common.Records;
using Orchard.Models;
using Orchard.Security;

namespace Orchard.Core.Common.Models {
    public class CommonModel : ModelPartWithRecord<CommonRecord> {
        public IUser Owner { get; set; }
    }
}