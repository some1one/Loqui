﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public abstract class ContainerType : TypeGeneration
    {
        protected bool singleType;
        public TypeGeneration SingleTypeGen;
        protected bool isLoquiSingle;
        protected LoquiType LoquiTypeSingleton => SingleTypeGen as LoquiType;
        public override bool HasProperty => true;

        public override string Property => $"{this.Name}";
        public override string ProtectedName => $"{this.ProtectedProperty}";

        public virtual string ItemTypeName(bool getter)
        {
            if (singleType)
            {
                return SingleTypeGen.TypeName(getter);
            }
            throw new NotImplementedException();
        }

        public TypeGeneration SubTypeGeneration => SingleTypeGen;

        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);

            if (!node.Elements().Any())
            {
                throw new ArgumentException("List had no elements.");
            }

            if (node.Elements().Any())
            {
                var typeGen = await ObjectGen.LoadField(
                    node.Elements().First(),
                    requireName: false,
                    setDefaults: false);
                if (typeGen.Succeeded)
                {
                    SingleTypeGen = typeGen.Value;
                    singleType = true;
                    isLoquiSingle = SingleTypeGen as LoquiType != null;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            if (SingleTypeGen is ContainerType
                || SingleTypeGen is DictType)
            {
                throw new NotImplementedException();
            }

            this.NotifyingProperty.OnNext(NotifyingType.ReactiveUI);
        }

        public override IEnumerable<string> GetRequiredNamespaces()
        {
            return this.SingleTypeGen.GetRequiredNamespaces();
        }

        public void AddMaskException(FileGeneration fg, string errorMaskAccessor, string exception)
        {
            fg.AppendLine($"{errorMaskAccessor}?.{this.Name}.Specific.Value.Add({exception});");
        }

        public override void GenerateSetNthHasBeenSet(FileGeneration fg, Accessor identifier, string onIdentifier)
        {
            if (!this.ReadOnly)
            {
                fg.AppendLine($"{identifier.PropertyAccess}.HasBeenSet = {onIdentifier};");
            }
            fg.AppendLine("break;");
        }

        public override void GenerateUnsetNth(FileGeneration fg, Accessor identifier)
        {
            if (!this.ReadOnly)
            {
                fg.AppendLine($"{identifier.PropertyAccess}.Unset();");
            }
            fg.AppendLine("break;");
        }

        public override string GetName(bool internalUse, bool property = true)
        {
            if (internalUse)
            {
                return $"_{this.Name}";
            }
            else
            {
                return this.Name;
            }
        }

        public override string GenerateACopy(string rhsAccessor)
        {
            throw new NotImplementedException();
        }

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate = false)
        {
            return $"{(negate ? "!" : null)}{accessor.DirectAccess}.SequenceEqual({rhsAccessor.DirectAccess})";
        }

        public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor)
        {
            fg.AppendLine($"if (!{accessor.DirectAccess}.SequenceEqual({rhsAccessor.DirectAccess})) return false;");
        }

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
            string funcStr;
            if (this.SubTypeGeneration is LoquiType loqui)
            {
                funcStr = $"(loqLhs, loqRhs) => loqLhs.{(loqui.TargetObjectGeneration == null ? nameof(IEqualsMask.GetEqualsIMask) : "GetEqualsMask")}(loqRhs, include)";
            }
            else
            {
                funcStr = $"(l, r) => {this.SubTypeGeneration.GenerateEqualsSnippet(new Accessor("l"), new Accessor("r"))}";
            }
            using (var args = new ArgsWrapper(fg,
                $"ret.{this.Name} = item.{this.Name}.CollectionEqualsHelper"))
            {
                args.Add($"rhs.{this.Name}");
                args.Add(funcStr);
                args.Add($"include");
            }
        }

        public void GenerateForEqualsMask(FileGeneration fg, string retAccessor, bool on)
        {
            fg.AppendLine($"{retAccessor} = new {ContainerMaskFieldGeneration.GetMaskString(this, "bool")}();");
            fg.AppendLine($"{retAccessor}.Overall = {(on ? "true" : "false")};");
        }

        public override void GenerateForHash(FileGeneration fg, Accessor accessor, string hashResultAccessor)
        {
            fg.AppendLine($"{hashResultAccessor} = HashHelper.GetHashCode({accessor}).CombineHashCode({hashResultAccessor});");
        }
    }
}
