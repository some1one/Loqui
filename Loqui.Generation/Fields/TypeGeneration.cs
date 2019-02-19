﻿using Noggog.Notifying;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public abstract class TypeGeneration
    {
        public ObjectGeneration ObjectGen { get; private set; }
        public ProtocolGeneration ProtoGen => ObjectGen.ProtoGen;
        public bool KeyField { get; protected set; }
        public abstract string TypeName { get; }
        public virtual string SetToName => TypeName;
        public virtual string Name { get; set; }
        public virtual string Property => $"{this.Name}_Property";
        public virtual string ProtectedProperty => $"_{this.Name}";
        public virtual string PropertyOrName => $"{(this.Bare ? Name : Property)}";
        public string IndexEnumName => $"{this.ObjectGen.FieldIndexName}.{this.Name}";
        public string ObjectCentralizationEnumName => IndexEnumName;
        public string IndexEnumInt => $"(int){this.IndexEnumName}";
        public bool HasIndex => !string.IsNullOrWhiteSpace(this.Name) && this.IntegrateField;
        public abstract string ProtectedName { get; }
        protected bool _derivative;
        public virtual bool Derivative => this._derivative;
        public virtual bool IntegrateField { get; set; } = true;
        public bool RaisePropertyChanged;
        public bool ReadOnly;
        private bool _copy;
        public virtual bool Copy => _copy;
        public bool TrueReadOnly => this.ObjectGen is StructGeneration;
        public bool GenerateClassMembers = true;
        public abstract bool IsEnumerable { get; }
        public readonly NotifyingSetItem<NotifyingType> NotifyingProperty = new NotifyingSetItem<NotifyingType>();
        public NotifyingType NotifyingType => NotifyingProperty.Item;
        public bool Notifying => NotifyingType != NotifyingType.None;
        public readonly NotifyingSetItem<bool> HasBeenSetProperty = new NotifyingSetItem<bool>();
        public bool HasBeenSet => HasBeenSetProperty.Item;
        public readonly NotifyingSetItem<bool> ObjectCentralizedProperty = new NotifyingSetItem<bool>();
        public bool ObjectCentralized => ObjectCentralizedProperty.Item;
        public bool Bare => this.NotifyingType == NotifyingType.None && !this.HasBeenSet;
        public virtual bool HasProperty => !this.Bare && this.NotifyingType != NotifyingType.ReactiveUI;
        public bool PrefersProperty => HasProperty && !this.ObjectCentralized;
        public Dictionary<object, object> CustomData = new Dictionary<object, object>();
        public XElement Node;
        public virtual bool Namable => true;
        public abstract bool IsClass { get; }
        public abstract bool HasDefault { get; }

        public TypeGeneration()
        {
        }

        public void SetObjectGeneration(ObjectGeneration obj, bool setDefaults)
        {
            this.ObjectGen = obj;
            if (!setDefaults) return;
            this.RaisePropertyChanged = this.ObjectGen.RaisePropertyChangedDefault;
            this.NotifyingProperty.SetIfNotSet(this.ObjectGen.NotifyingDefault, markAsSet: false);
            this.HasBeenSetProperty.SetIfNotSet(this.ObjectGen.HasBeenSetDefault, markAsSet: false);
            this.ObjectCentralizedProperty.SetIfNotSet(this.ObjectGen.ObjectCentralizedDefault, markAsSet: false);
            this._derivative = this.ObjectGen.DerivativeDefault;
            this.ReadOnly = this.ObjectGen.ReadOnlyDefault;
        }

        public virtual async Task Load(XElement node, bool requireName = true)
        {
            this.Node = node;
            LoadTypeGenerationFromNode(node, requireName);
        }

        protected void LoadTypeGenerationFromNode(XElement node, bool requireName = true)
        {
            node.TransferAttribute<bool>(Constants.HIDDEN_FIELD, i => this.IntegrateField = !i);
            Name = node.GetAttribute<string>(Constants.NAME);
            node.TransferAttribute<bool>(Constants.KEY_FIELD, i => this.KeyField = i);
            node.TransferAttribute<bool>(Constants.DERIVATIVE, i => this._derivative = i);
            this.ReadOnly = node.GetAttribute<bool>(Constants.PROTECTED, this.ObjectGen.ReadOnlyDefault || Derivative);
            this._copy = node.GetAttribute<bool>(Constants.COPY, !this.ReadOnly);
            node.TransferAttribute<bool>(Constants.GENERATE_CLASS_MEMBERS, i => this.GenerateClassMembers = i);
            node.TransferAttribute<bool>(Constants.RAISE_PROPERTY_CHANGED, i => this.RaisePropertyChanged = i);
            node.TransferAttribute<NotifyingType>(Constants.NOTIFYING, i => this.NotifyingProperty.Item = i);
            node.TransferAttribute<bool>(Constants.OBJECT_CENTRALIZED, i => this.ObjectCentralizedProperty.Item = i);
            node.TransferAttribute<bool>(Constants.HAS_BEEN_SET, i => this.HasBeenSetProperty.Item = i);
            if (requireName && Namable && Name == null)
            {
                throw new ArgumentException("Type field needs a name.");
            }
        }

        public virtual string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate = false)
        {
            return $"{(negate ? "!" : null)}object.Equals({accessor.DirectAccess}, {rhsAccessor.DirectAccess})";
        }

        public abstract bool IsNullable();

        public void FinalizeField()
        {
            if (this._derivative && !ReadOnly)
            {
                throw new ArgumentException("Cannot mark field as non-readonly if also derivative.  Being derivative implied being readonly.");
            }
        }

        public virtual IEnumerable<string> GetRequiredNamespaces()
        {
            yield break;
        }

        public virtual void GenerateForCtor(FileGeneration fg)
        {

        }

        public abstract void GenerateForClass(FileGeneration fg);

        public abstract void GenerateForInterface(FileGeneration fg);

        public abstract void GenerateForGetterInterface(FileGeneration fg);

        public abstract bool CopyNeedsTryCatch { get; }

        public abstract string SkipCheck(string copyMaskAccessor);

        public abstract void GenerateForCopy(
            FileGeneration fg,
            Accessor accessor,
            string rhsAccessorPrefix,
            string copyMaskAccessor,
            string defaultFallbackAccessor,
            string cmdsAccessor,
            bool protectedMembers);

        public abstract string GenerateACopy(string rhsAccessor);

        public abstract void GenerateSetNth(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdsAccessor, bool internalUse);

        public abstract void GenerateSetNthHasBeenSet(FileGeneration fg, Accessor identifier, string onIdentifier);

        public abstract void GenerateUnsetNth(FileGeneration fg, Accessor identifier, string cmdsAccessor);

        public abstract void GenerateGetNth(FileGeneration fg, Accessor identifier);

        public abstract void GenerateClear(FileGeneration fg, Accessor accessorPrefix, string cmdAccessor);

        public abstract void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor);

        public abstract void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor);

        public abstract void GenerateForHash(FileGeneration fg, string hashResultAccessor);

        public virtual void GenerateForInterfaceExt(FileGeneration fg) { }

        public virtual void GenerateForStaticCtor(FileGeneration fg) { }

        public virtual void GenerateGetNameIndex(FileGeneration fg)
        {
            if (!this.IntegrateField) return;
            fg.AppendLine($"return (ushort){this.ObjectGen.FieldIndexName}.{this.Name};");
        }

        public virtual void GenerateGetNthName(FileGeneration fg)
        {
            if (!this.IntegrateField) return;
            fg.AppendLine($"return \"{this.Name}\";");
        }

        public virtual void GenerateGetNthType(FileGeneration fg)
        {
            if (!this.IntegrateField) return;
            fg.AppendLine($"return typeof({this.TypeName});");
        }

        public abstract void GenerateToString(FileGeneration fg, string name, Accessor accessor, string fgAccessor);

        public abstract void GenerateForHasBeenSetCheck(FileGeneration fg, Accessor accessor, string checkMaskAccessor);

        public abstract void GenerateForHasBeenSetMaskGetter(FileGeneration fg, Accessor accessor, string retAccessor);

        public virtual void GenerateGetNthObjectHasBeenSet(FileGeneration fg)
        {
            if (!this.IntegrateField) return;
            if (this.HasBeenSet)
            {
                fg.AppendLine($"return obj.{this.HasBeenSetAccessor()};");
            }
            else
            {
                fg.AppendLine("return true;");
            }
        }

        public virtual string EqualsMaskAccessor(string accessor) => accessor;

        public virtual string GetName(bool internalUse, bool property)
        {
            if (internalUse)
            {
                if (property)
                {
                    return this.ProtectedProperty;
                }
                else
                {
                    return this.ProtectedName;
                }
            }
            else
            {
                if (property)
                {
                    return this.Property;
                }
                else
                {
                    return this.Name;
                }
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(this.Name)) return base.ToString();
            return $"{base.ToString()}: {this.Name}";
        }

        public virtual async Task Resolve()
        {
            this.ObjectGen.RequiredNamespaces.Add(this.GetRequiredNamespaces());
        }

        public virtual string HasBeenSetAccessor(Accessor accessor = null)
        {
            if (accessor == null)
            {
                if (this.NotifyingType == NotifyingType.ReactiveUI)
                {
                    return $"{this.Name}_IsSet";
                }
                else
                {
                    return $"{this.Property}.HasBeenSet";
                }
            }
            else
            {
                if (this.NotifyingType == NotifyingType.ReactiveUI)
                {
                    return  $"{accessor.DirectAccess}_IsSet";
                }
                else
                {
                    return $"{accessor.PropertyAccess}.HasBeenSet";
                }
            }
        }
    }
}
