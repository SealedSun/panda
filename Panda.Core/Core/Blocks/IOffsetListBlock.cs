﻿using System.Collections;
using System.Collections.Generic;

namespace Panda.Core.Blocks
{
    public interface IOffsetListBlock : IBlock, IReadOnlyCollection<BlockOffset>, IContinuationBlock
    {
        /// <summary>
        ///  Indicates how many empty block offsets this empty block list can contain.
        ///  </summary><remarks><see cref="ListCapacity" /> minus <see cref="P:System.Collections.Generic.IReadOnlyCollection`1.Count" /> 
        ///  equals number of empty block offsets you can add to this block until
        ///  a continuation block needs to be created.</remarks>
        int ListCapacity { get; }
    }
}