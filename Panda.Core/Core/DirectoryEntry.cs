﻿using System;
using JetBrains.Annotations;

namespace Panda.Core
{
    public sealed class DirectoryEntry : IEquatable<DirectoryEntry>
    {
        [NotNull]
        private readonly string _name;

        private readonly long _blockOffset;

        public DirectoryEntry([NotNull] string name, long blockOffset)
        {
            if (name == null) throw new ArgumentNullException("name");
            _name = name;
            _blockOffset = blockOffset;
        }

        [NotNull]
        public string Name
        {
            get { return _name; }
        }

        public long BlockOffset
        {
            get { return _blockOffset; }
        }

        public bool Equals(DirectoryEntry other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(_name, other._name) && _blockOffset == other._blockOffset;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is DirectoryEntry && Equals((DirectoryEntry) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_name.GetHashCode()*397) ^ _blockOffset.GetHashCode();
            }
        }
    }
}