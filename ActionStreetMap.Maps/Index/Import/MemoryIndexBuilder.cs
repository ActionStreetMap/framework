using ActionStreetMap.Infrastructure.Diagnostic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActionStreetMap.Maps.Index.Import
{
    internal class MemoryIndexBuilder: IndexBuilder
    {
        public MemoryIndexBuilder(ITrace trace)
            : base(trace)
        {
        }

        public override void Build()
        {
        }
    }
}
