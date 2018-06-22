﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class ObjectCentralizationModule : GenerationModule
    {
        public override IEnumerable<string> RequiredUsingStatements(ObjectGeneration obj)
        {
            foreach (var e in base.RequiredUsingStatements(obj))
            {
                yield return e;
            }
            yield return "Loqui.Internal";
            yield return "System.Collections.Specialized";
        }

        public static Dictionary<string, List<TypeGeneration>> GetContainedTypes(ObjectGeneration obj)
        {
            Dictionary<string, List<TypeGeneration>> containedTypes = new Dictionary<string, List<TypeGeneration>>();

            foreach (var field in obj.IterateFields())
            {
                if (field is ContainerType) continue;
                if (field is DictType) continue;
                if (field.Notifying != NotifyingType.ObjectCentralized) continue;
                containedTypes.TryCreateValue(field.TypeName).Add(field);
            }
            return containedTypes;
        }

        public override IEnumerable<string> Interfaces(ObjectGeneration obj)
        {
            foreach (var ret in base.Interfaces(obj))
            {
                yield return ret;
            }
            foreach (var type in GetContainedTypes(obj))
            {
                yield return $"IPropertySupporter<{type.Key}>";
            }
        }

        public override async Task GenerateInCtor(ObjectGeneration obj, FileGeneration fg)
        {
            if (!GetContainedTypes(obj).Any()) return;
            if (!ParentHasImplementation(obj))
            {
                fg.AppendLine($"_hasBeenSetTracker = new BitArray(((ILoquiObject)this).Registration.FieldCount);");
            }
        }

        public async override Task GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {
            if (!GetContainedTypes(obj).Any()) return;
            if (!ParentHasImplementation(obj))
            {
                fg.AppendLine($"protected readonly BitArray _hasBeenSetTracker;");
            }
            foreach (var type in GetContainedTypes(obj))
            {
                using (new RegionWrapper(fg, $"IPropertySupporter {type.Key}"))
                {
                    await GenerateForType(fg, obj, type.Key, type.Value);
                }
            }
        }

        private static bool HasImplementation(ObjectGeneration obj, string type)
        {
            return obj.IterateFields().Any(
                    (f) => f.Notifying == NotifyingType.ObjectCentralized
                            && f.TypeName.Equals(type));
        }

        private static bool ParentHasImplementation(ObjectGeneration obj, string type)
        {
            return obj.BaseClassTrail().Any(
                (b) => HasImplementation(b, type));
        }

        private static bool ParentHasImplementation(ObjectGeneration obj)
        {
            return obj.BaseClassTrail().Any((b) => GetContainedTypes(b).Any());
        }

        private static async Task GenerateForType(
            FileGeneration fg,
            ObjectGeneration obj,
            string type,
            IEnumerable<TypeGeneration> fields)
        {
            if (!ParentHasImplementation(obj, type))
            {
                fg.AppendLine($"protected ObjectCentralizationSubscriptions<{type}> _{Utility.MemberNameSafety(type)}_subscriptions;");
            }
            await GenerateGet(fg, obj, type, fields);
            await GenerateSet(fg, obj, type, fields);
            await GenerateGetHasBeenSet(fg, obj, type, fields);
            await GenerateSetHasBeenSet(fg, obj, type, fields);
            await GenerateUnset(fg, obj, type, fields);
            await GenerateSubscribe(fg, obj, type, fields);
            await GenerateUnsubscribe(fg, obj, type, fields);
            await GenerateSetCurrentAsDefault(fg, obj, type, fields);
            await GenerateGetDefault(fg, obj, type, fields);
        }

        private static async Task GenerateGet(
            FileGeneration fg,
            ObjectGeneration obj,
            string type,
            IEnumerable<TypeGeneration> fields)
        {
            using (var args = new FunctionWrapper(fg,
                $"{type} IPropertySupporter<{type}>.Get"))
            {
                args.Add("int index");
            }
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg,
                    $"return Get{Utility.MemberNameSafety(type)}"))
                {
                    args.AddPassArg("index");
                }
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"protected{await obj.FunctionOverride(async (b) => HasImplementation(b, type))}{type} Get{Utility.MemberNameSafety(type)}"))
            {
                args.Add("int index");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"switch (({obj.FieldIndexName})index)");
                using (new BraceWrapper(fg))
                {
                    foreach (var field in fields)
                    {
                        fg.AppendLine($"case {field.IndexEnumName}:");
                        using (new DepthWrapper(fg))
                        {
                            fg.AppendLine($"return {field.Name};");
                        }
                    }
                    fg.AppendLine("default:");
                    using (new DepthWrapper(fg))
                    {
                        if (ParentHasImplementation(obj, type))
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"return base.Get{Utility.MemberNameSafety(type)}"))
                            {
                                args.AddPassArg("index");
                            }
                        }
                        else
                        {
                            fg.AppendLine($"throw new ArgumentException($\"Unknown index for field type {type}: {{index}}\");");
                        }
                    }
                }
            }
            fg.AppendLine();
        }

        private static async Task GenerateGetHasBeenSet(
            FileGeneration fg,
            ObjectGeneration obj,
            string type,
            IEnumerable<TypeGeneration> fields)
        {
            using (var args = new FunctionWrapper(fg,
                $"bool IPropertySupporter<{type}>.GetHasBeenSet"))
            {
                args.Add("int index");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("return _hasBeenSetTracker[index];");
            }
            fg.AppendLine();
        }

        private static async Task GenerateSetHasBeenSet(
            FileGeneration fg,
            ObjectGeneration obj,
            string type,
            IEnumerable<TypeGeneration> fields)
        {
            using (var args = new FunctionWrapper(fg,
                $"void IPropertySupporter<{type}>.SetHasBeenSet"))
            {
                args.Add("int index");
                args.Add("bool on");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("_hasBeenSetTracker[index] = on;");
            }
            fg.AppendLine();
        }

        private static async Task GenerateSet(
            FileGeneration fg,
            ObjectGeneration obj,
            string type,
            IEnumerable<TypeGeneration> fields)
        {
            using (var args = new FunctionWrapper(fg,
                $"void IPropertySupporter<{type}>.Set"))
            {
                args.Add("int index");
                args.Add($"{type} item");
                args.Add($"bool hasBeenSet");
                args.Add($"NotifyingFireParameters cmds");
            }
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg,
                    $"Set{Utility.MemberNameSafety(type)}"))
                {
                    args.AddPassArg("index");
                    args.AddPassArg("item");
                    args.AddPassArg("hasBeenSet");
                    args.AddPassArg("cmds");
                }
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"protected{await obj.FunctionOverride(async (b) => HasImplementation(b, type))}void Set{Utility.MemberNameSafety(type)}"))
            {
                args.Add("int index");
                args.Add($"{type} item");
                args.Add($"bool hasBeenSet");
                args.Add($"NotifyingFireParameters cmds");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"switch (({obj.FieldIndexName})index)");
                using (new BraceWrapper(fg))
                {
                    foreach (var field in fields)
                    {
                        fg.AppendLine($"case {field.IndexEnumName}:");
                        using (new DepthWrapper(fg))
                        {
                            fg.AppendLine($"Set{field.Name}(item, hasBeenSet, cmds);");
                            fg.AppendLine($"break;");
                        }
                    }
                    fg.AppendLine("default:");
                    using (new DepthWrapper(fg))
                    {
                        if (ParentHasImplementation(obj, type))
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"base.Set{Utility.MemberNameSafety(type)}"))
                            {
                                args.AddPassArg("index");
                                args.AddPassArg("item");
                                args.AddPassArg("hasBeenSet");
                                args.AddPassArg("cmds");
                            }
                            fg.AppendLine("break;");
                        }
                        else
                        {
                            fg.AppendLine($"throw new ArgumentException($\"Unknown index for field type {type}: {{index}}\");");
                        }
                    }
                }
            }
            fg.AppendLine();
        }

        private static async Task GenerateUnset(
            FileGeneration fg,
            ObjectGeneration obj,
            string type,
            IEnumerable<TypeGeneration> fields)
        {
            using (var args = new FunctionWrapper(fg,
                $"void IPropertySupporter<{type}>.Unset"))
            {
                args.Add("int index");
                args.Add("NotifyingUnsetParameters cmds");
            }
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg,
                    $"Unset{Utility.MemberNameSafety(type)}"))
                {
                    args.AddPassArg("index");
                    args.AddPassArg("cmds");
                }
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"protected{await obj.FunctionOverride(async (b) => HasImplementation(b, type))}void Unset{Utility.MemberNameSafety(type)}"))
            {
                args.Add("int index");
                args.Add("NotifyingUnsetParameters cmds");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"switch (({obj.FieldIndexName})index)");
                using (new BraceWrapper(fg))
                {
                    foreach (var field in fields)
                    {
                        fg.AppendLine($"case {field.IndexEnumName}:");
                        using (new DepthWrapper(fg))
                        {
                            fg.AppendLine("_hasBeenSetTracker[index] = false;");
                            fg.AppendLine($"{field.Name} = default({type});");
                            fg.AppendLine("break;");
                        }
                    }
                    fg.AppendLine("default:");
                    using (new DepthWrapper(fg))
                    {
                        if (ParentHasImplementation(obj, type))
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"base.Unset{Utility.MemberNameSafety(type)}"))
                            {
                                args.AddPassArg("index");
                                args.AddPassArg("cmds");
                            }
                            fg.AppendLine("break;");
                        }
                        else
                        {
                            fg.AppendLine($"throw new ArgumentException($\"Unknown index for field type {type}: {{index}}\");");
                        }
                    }
                }
            }
            fg.AppendLine();
        }

        private static async Task GenerateUnsubscribe(
            FileGeneration fg,
            ObjectGeneration obj,
            string type,
            IEnumerable<TypeGeneration> fields)
        {
            fg.AppendLine("[DebuggerStepThrough]");
            using (var args = new FunctionWrapper(fg,
                $"void IPropertySupporter<{type}>.Unsubscribe"))
            {
                args.Add("int index");
                args.Add("object owner");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"_{Utility.MemberNameSafety(type)}_subscriptions?.Unsubscribe(index, owner);");
            }
            fg.AppendLine();
        }

        private static async Task GenerateSubscribe(
            FileGeneration fg,
            ObjectGeneration obj,
            string type,
            IEnumerable<TypeGeneration> fields)
        {
            fg.AppendLine("[DebuggerStepThrough]");
            using (var args = new FunctionWrapper(fg,
                $"void IPropertySupporter<{type}>.Subscribe"))
            {
                args.Add("int index");
                args.Add("object owner");
                args.Add($"NotifyingSetItemInternalCallback<{type}> callback");
                args.Add("NotifyingSubscribeParameters cmds");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"if (_{Utility.MemberNameSafety(type)}_subscriptions == null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"_{Utility.MemberNameSafety(type)}_subscriptions = new ObjectCentralizationSubscriptions<{type}>();");
                }
                using (var args = new ArgsWrapper(fg,
                    $"_{Utility.MemberNameSafety(type)}_subscriptions.Subscribe"))
                {
                    args.Add("index: index");
                    args.Add("owner: owner");
                    args.Add("prop: this");
                    args.Add("callback: callback");
                    args.Add("cmds: cmds");
                }
            }
            fg.AppendLine();
        }

        private static async Task GenerateSetCurrentAsDefault(
            FileGeneration fg,
            ObjectGeneration obj,
            string type,
            IEnumerable<TypeGeneration> fields)
        {
            using (var args = new FunctionWrapper(fg,
                $"void IPropertySupporter<{type}>.SetCurrentAsDefault"))
            {
                args.Add("int index");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"throw new NotImplementedException();");
            }
            fg.AppendLine();
        }

        private static async Task GenerateGetDefault(
            FileGeneration fg,
            ObjectGeneration obj,
            string type,
            IEnumerable<TypeGeneration> fields)
        {
            using (var args = new FunctionWrapper(fg,
                $"{type} IPropertySupporter<{type}>.DefaultValue"))
            {
                args.Add("int index");
            }
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg,
                    $"return DefaultValue{Utility.MemberNameSafety(type)}"))
                {
                    args.AddPassArg("index");
                }
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"protected{await obj.FunctionOverride(async (b) => HasImplementation(b, type))}{type} DefaultValue{Utility.MemberNameSafety(type)}"))
            {
                args.Add("int index");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"switch (({obj.FieldIndexName})index)");
                using (new BraceWrapper(fg))
                {
                    foreach (var field in fields.Where((f) => !f.HasDefault))
                    {
                        fg.AppendLine($"case {field.IndexEnumName}:");
                    }
                    if (fields.Any((f) => !f.HasDefault))
                    {
                        using (new DepthWrapper(fg))
                        {
                            fg.AppendLine($"return default({type});");
                        }
                    }
                    foreach (var field in fields.Where((f) => f.HasDefault))
                    {
                        fg.AppendLine($"case {field.IndexEnumName}:");
                        using (new DepthWrapper(fg))
                        {
                            fg.AppendLine($"return _{field.Name}_Default;");
                        }
                    }
                    fg.AppendLine("default:");
                    using (new DepthWrapper(fg))
                    {
                        if (ParentHasImplementation(obj, type))
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"return base.DefaultValue{Utility.MemberNameSafety(type)}"))
                            {
                                args.AddPassArg("index");
                            }
                        }
                        else
                        {
                            fg.AppendLine($"throw new ArgumentException($\"Unknown index for field type {type}: {{index}}\");");
                        }
                    }
                }
            }
            fg.AppendLine();
        }
    }
}