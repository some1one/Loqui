﻿using Noggog;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class RangeUInt32XmlTranslation : PrimitiveXmlTranslation<RangeUInt32>
    {
        public readonly static RangeUInt32XmlTranslation Instance = new RangeUInt32XmlTranslation();
        public const string MIN = "Min";
        public const string MAX = "Max";

        protected override void WriteValue(XmlWriter writer, RangeUInt32? item)
        {
            if (!item.HasValue) return;
            writer.WriteAttributeString(MIN, item.Value.Min.ToString());
            writer.WriteAttributeString(MAX, item.Value.Max.ToString());
        }

        protected override string GetItemStr(RangeUInt32 item)
        {
            throw new NotImplementedException();
        }

        protected override RangeUInt32 ParseNonNullString(string str)
        {
            throw new NotImplementedException();
        }

        protected override RangeUInt32? ParseValue(XElement root)
        {
            uint? min, max;
            if (root.TryGetAttribute(MIN, out XAttribute val))
            {
                if (uint.TryParse(val.Value, out var i))
                {
                    min = i;
                }
                else
                {
                    throw new ArgumentException("Min value was malformed: " + val.Value);
                }
            }
            else
            {
                min = null;
            }
            if (root.TryGetAttribute(MAX, out val))
            {
                if (uint.TryParse(val.Value, out var i))
                {
                    max = i;
                }
                else
                {
                    throw new ArgumentException("Min value was malformed: " + val.Value);
                }
            }
            else
            {
                max = null;
            }
            if (!min.HasValue && !max.HasValue) return null;
            return new RangeUInt32(min, max);
        }
    }
}
