using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Noggog;

namespace Loqui.Generation
{
    public class ArrayType : ListType
    {
        public override bool CopyNeedsTryCatch => false;
        public override bool IsClass => false;
        public override bool HasDefault => true;
        public override string TypeName(bool getter, bool needsCovariance = false)
        {
            if (getter)
            {
                return $"ReadOnlyMemorySlice<{this.ItemTypeName(getter)}>{this.NullChar}";
            }
            else
            {
                return $"{this.ItemTypeName(getter)}[]";
            }
        }

        public int? FixedSize;

        public override async Task Load(XElement node, bool requireName = true)
        {
            this.FixedSize = node.GetAttribute(Constants.FIXED_SIZE, default(int?));
            await base.Load(node, requireName);
        }

        protected override string GetActualItemClass(bool ctor = false)
        {
            if (this.NotifyingType == NotifyingType.ReactiveUI)
            {
                throw new NotImplementedException();
            }
            else
            {
                if (!ctor)
                {
                    return $"{this.ItemTypeName(getter: false)}[]";
                }
                if (this.HasBeenSet)
                {
                    return $"default";
                }
                else if (this.FixedSize.HasValue)
                {
                    if (this.SubTypeGeneration is LoquiType loqui)
                    {
                        return $"ArrayExt.Create({this.FixedSize}, (i) => new {loqui.TargetObjectGeneration.ObjectName}())";
                    }
                    else if (this.SubTypeGeneration.IsNullable
                        || this.SubTypeGeneration.CanBeNullable(getter: false))
                    {
                        return $"new {this.ItemTypeName(getter: false)}[{this.FixedSize}]";
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    return $"new {this.ItemTypeName(getter: false)}[0]";
                }
            }
        }

        public override string Interface(bool getter, bool internalInterface)
        {
            string itemTypeName = this.ItemTypeName(getter: getter);
            if (this.SubTypeGeneration is LoquiType loqui)
            {
                itemTypeName = loqui.TypeNameInternal(getter: getter, internalInterface: internalInterface);
            }
            if (getter)
            {
                return $"ReadOnlyMemorySlice<{itemTypeName}{this.SubTypeGeneration.NullChar}>{this.NullChar}";
            }
            else
            {
                return $"{itemTypeName}{this.SubTypeGeneration.NullChar}[]";
            }
        }

        public override void GenerateClear(FileGeneration fg, Accessor accessorPrefix)
        {
            if (this.HasBeenSet)
            {
                fg.AppendLine($"{accessorPrefix.PropertyAccess}.Unset();");
            }
            else
            {
                if (this.FixedSize.HasValue)
                {
                    if (this.SubTypeGeneration.IsNullable
                        && this.SubTypeGeneration.HasBeenSet)
                    {
                        fg.AppendLine($"{accessorPrefix.DirectAccess}.ResetToNull();");
                    }
                    else if (this.SubTypeGeneration is StringType)
                    {
                        fg.AppendLine($"Array.Fill({accessorPrefix.DirectAccess}, string.Empty);");
                    }
                    else if (this.SubTypeGeneration is LoquiType loqui)
                    {
                        fg.AppendLine($"{accessorPrefix.DirectAccess}.Fill(() => new {loqui.TargetObjectGeneration.ObjectName}());");
                    }
                    else
                    {
                        fg.AppendLine($"{accessorPrefix.DirectAccess}.Reset();");
                    }
                }
                else
                {
                    fg.AppendLine($"{accessorPrefix.DirectAccess}.Clear();");
                }
            }
        }

        public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor)
        {
            if (this.SubTypeGeneration.IsIEquatable)
            {
                fg.AppendLine($"if (!MemoryExtensions.SequenceEqual({accessor.DirectAccess}.Span, {rhsAccessor.DirectAccess}.Span)) return false;");
            }
            else
            {
                fg.AppendLine($"if (!{accessor.DirectAccess}.SequenceEqual({rhsAccessor.DirectAccess})) return false;");
            }
        }

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
            string funcStr;
            if (this.SubTypeGeneration is LoquiType loqui)
            {
                funcStr = $"(loqLhs, loqRhs) => loqLhs.{(loqui.TargetObjectGeneration == null ? nameof(IEqualsMask.GetEqualsMask) : "GetEqualsMask")}(loqRhs, include)";
            }
            else
            {
                funcStr = $"(l, r) => {this.SubTypeGeneration.GenerateEqualsSnippet(new Accessor("l"), new Accessor("r"))}";
            }
            using (var args = new ArgsWrapper(fg,
                $"ret.{this.Name} = item.{this.Name}.SpanEqualsHelper"))
            {
                args.Add($"rhs.{this.Name}");
                args.Add(funcStr);
                args.Add($"include");
            }
        }
    }
}
