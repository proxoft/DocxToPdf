﻿using System;
using System.Diagnostics;

namespace Proxoft.DocxToPdf.Core
{
    [DebuggerDisplay("PageNumber: {_number}")]
    internal class PageNumber : IEquatable<PageNumber>, IComparable<PageNumber>
    {
        public static readonly PageNumber None = new PageNumber(0);
        public static readonly PageNumber First = new PageNumber(1);

        private readonly int _number;

        public PageNumber(int number)
        {
            if(number < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(number), "number must be zero or higher");
            }

            _number = number;
        }

        public PageNumber Next() => new PageNumber(_number + 1);

        public bool Equals(PageNumber? other)
        {
            return other is object
                && other._number == _number;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_number);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as PageNumber);
        }

        public int CompareTo(PageNumber other)
        {
            return _number - other._number;
        }

        public static bool operator==(PageNumber p1, PageNumber p2)
        {
            return  p1.Equals(p2);
        }

        public static bool operator !=(PageNumber p1, PageNumber p2)
        {
            return !(p1 == p2);
        }

        public static implicit operator int(PageNumber pageNumber) => pageNumber._number;

        public static explicit operator PageNumber(int value) => new PageNumber(value);
    }
}
