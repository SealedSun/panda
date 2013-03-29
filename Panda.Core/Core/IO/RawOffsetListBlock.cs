﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Panda.Core.Blocks;

namespace Panda.Core.IO
{
    public class RawOffsetListBlock : RawContinuedBlock, IEmptyListBlock, IFileContinuationBlock
    {
        public RawOffsetListBlock([NotNull] IRawPersistenceSpace space, BlockOffset offset, uint size)
            : base(space, offset, size)
        {
        }

        unsafe uint _getUIntAt(int index)
        {
            var ptr = (uint*) ThisPointer;
            return ptr[index];
        }

        public IEnumerator<BlockOffset> GetEnumerator()
        {
            // Iterate over offsets until we reach a null entry or the end of the block.
            for (var i = 0; i < ListCapacity; i++)
            {
                // The first uint is meta information (total offset count/file size)
                var value = _getUIntAt(i + MetaDataPrefixUInt32Count);
                if(value == 0)
                    yield break;
                else
                    yield return (BlockOffset) value;
            }
        }

        /// <summary>
        /// The number of UInt32 fields at the beginning of the offset block.
        /// Contains information about the block (total offset count, file size, etc.)
        /// </summary>
        protected virtual int MetaDataPrefixUInt32Count
        {
            get { return 1; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            // we don't actually have a better way to compute the count than to parse the block, offset by offset
            // just counting would be a bit more efficient than asking an enumerator, but Count is not
            // a property that will be used often.
// ReSharper disable InvokeAsExtensionMethod
            get { return Enumerable.Count(this); }
// ReSharper restore InvokeAsExtensionMethod
        }

        public unsafe int ListCapacity
        {
            get
            {
                // The number of offsets that fit into the block minus total offset count and continuation offset
                return (int) (BlockSize/sizeof (BlockOffset) - 2);
            }
        }

        public unsafe void ReplaceOffsets(BlockOffset[] offsets)
        {
            if (offsets == null)
                throw new ArgumentNullException("offsets");
            
            if (offsets.Length > ListCapacity)
            {
                throw new ArgumentOutOfRangeException("offsets",offsets.Length,"Not all block offsets fit into this block.");
            }

            // Replace all offsets in the block with the supplied offsets, 
            // padding with 0 if the array is too short.
            // 

            var ptr = ((uint*) ThisPointer)+MetaDataPrefixUInt32Count;
            
            for (var i = 0; i < ListCapacity; i++)
            {
                if (i < offsets.Length)
                {
                    ptr[i] = offsets[i].Offset;
                }
                else
                {
                    ptr[i] = 0u;
                }
            }
        }

        public unsafe int TotalFreeBlockCount
        {
            get { return (int) (*((uint*) ThisPointer)); }
            set
            {
                if(value < 0)
                    throw new ArgumentOutOfRangeException("value",value,"TotalFreeBlockCount cannot be negative.");
                *((uint*) ThisPointer) = (uint) value;
            }
        }

        public BlockOffset[] Remove(int count)
        {
            var offsets = this.ToArray();
            if (offsets.Length < count)
            {
                throw new ArgumentOutOfRangeException("count",count,"Not enough offsets in the block to satisfy the remove request.");
            }

            // Move offsets to result, setting them to 0 in the offset-arry
            var result = new BlockOffset[count];
            for (var i = 0; i < result.Length; i++)
            {
                var revIdx = offsets.Length - 1 - i;
                result[i] = offsets[revIdx];
                offsets[revIdx] = (BlockOffset) 0;
            }

            // Write back the modified offsets
            ReplaceOffsets(offsets);

            // Also update the free block count
            TotalFreeBlockCount -= count;

            return result;
        }

        public void Append(BlockOffset[] freeBlockOffsets)
        {
            if (freeBlockOffsets == null)
                throw new ArgumentNullException("freeBlockOffsets");
            
            // This is a very simplistic implementation of append:
            //  reading the entire blocklist, appending the new offsets in memory and
            //  then writing the entire list back.

            // A more sophisticated implementation would just write the modified entries.
            var offsets = this.ToList();
            offsets.AddRange(freeBlockOffsets);
            if(offsets.Count > ListCapacity)
                throw new ArgumentOutOfRangeException("freeBlockOffsets",freeBlockOffsets.Length,"Not all offsets fit into this block.");

            ReplaceOffsets(offsets.ToArray());
            TotalFreeBlockCount += freeBlockOffsets.Length;
        }
    }
}
