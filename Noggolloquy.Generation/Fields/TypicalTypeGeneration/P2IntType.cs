﻿using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noggolloquy.Generation
{
    public class P2IntType : TypicalTypeGeneration
    {
        public override Type Type => typeof(P2Int);

        protected override string GenerateDefaultValue() => $"new {TypeName}({DefaultValue})";
    }
}
