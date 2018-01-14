﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class StructGeneration : ObjectGeneration
    {
        public override bool Abstract => false;

        public override bool NotifyingDefault => false;
        public override bool HasBeenSetDefault => false;

        public override string ProtectedKeyword => "private";

        public StructGeneration(LoquiGenerator gen, ProtocolGeneration protoGen, FileInfo sourceFile)
            : base(gen, protoGen, sourceFile)
        {
        }

        protected override async Task GenerateCtor(FileGeneration fg)
        {
            if (this.GeneratePublicBasicCtor)
            {
                fg.AppendLine($"public {this.Name}(");
                List<string> lines = new List<string>();
                foreach (var field in this.IterateFields())
                {
                    lines.Add($"{field.TypeName} {field.Name} = default({field.TypeName})");
                }
                for (int i = 0; i < lines.Count; i++)
                {
                    using (new DepthWrapper(fg))
                    {
                        using (new LineWrapper(fg))
                        {
                            fg.Append(lines[i]);
                            if (i != lines.Count - 1)
                            {
                                fg.Append(",");
                            }
                            else
                            {
                                fg.Append(")");
                            }
                        }
                    }
                }

                using (new BraceWrapper(fg))
                {
                    foreach (var field in this.IterateFields())
                    {
                        fg.AppendLine($"this.{field.Name} = {field.Name};");
                    }
                    foreach (var mod in this.gen.GenerationModules)
                    {
                        await mod.GenerateInCtor(this, fg);
                    }
                    fg.AppendLine("CustomCtor();");
                }
                fg.AppendLine();
            }

            fg.AppendLine($"{(this.GeneratePublicBasicCtor ? "public" : "private")} {this.Name}({this.Getter_InterfaceStr} rhs)");
            using (new BraceWrapper(fg))
            {
                foreach (var field in this.IterateFields())
                {
                    fg.AppendLine($"this.{field.Name} = {field.GenerateACopy("rhs." + field.Name)};");
                }
            }
            fg.AppendLine();

            fg.AppendLine("partial void CustomCtor();");
            fg.AppendLine();
        }

        protected override void GenerateClassLine(FileGeneration fg)
        {
            // Generate class header and interfaces
            using (new LineWrapper(fg))
            {
                fg.Append($"public partial struct {Name}{this.GenericTypes} : ");

                List<string> list = new List<string>
                {
                    this.Getter_InterfaceStr
                };
                list.AddRange(
                    this.Interfaces
                        .Union(this.gen.GenerationModules
                            .SelectMany((tr) => tr.Interfaces(this)))
                        .Union(this.gen.GenerationModules
                            .SelectMany((tr) => tr.GetWriterInterfaces(this)))
                        .Union(this.GenerationInterfaces
                            .SelectMany((tr) => tr.Interfaces(this))));
                list.Add($"IEquatable<{this.ObjectName}>");
                fg.Append(string.Join(", ", list));
            }
        }

        protected override void GenerateEqualsCode(FileGeneration fg)
        {
            fg.AppendLine($"if (!(obj is {this.ObjectName} rhs)) return false;");
            fg.AppendLine($"return Equals(rhs);");
        }

        public override async Task Load()
        {
            this.Interfaces.Add($"ILoquiWriterSerializer<{this.Mask(MaskType.Error)}>");

            await base.Load();
            foreach (var field in this.IterateFields())
            {
                field.NotifyingProperty.Item = false;
                field.HasBeenSetProperty.Item = false;
                field.ReadOnly = true;
            }
        }

        protected override void GenerateLoquiSetterInterface(FileGeneration fg)
        {
        }

        protected override async Task GenerateSetterInterface(FileGeneration fg)
        {
        }

        protected override void GenerateGetNthObjectHasBeenSet(FileGeneration fg)
        {
        }

        protected override void GenerateClear(FileGeneration fg, bool classFile)
        {
        }

        public override void GenerateCopy(FileGeneration fg)
        {
            fg.AppendLine($"public static {this.ObjectName} Copy({this.Getter_InterfaceStr} item)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"return new {this.ObjectName}(");
                List<string> lines = new List<string>();
                foreach (var field in this.IterateFields())
                {
                    lines.Add($"{field.Name}: {field.GenerateACopy("item." + field.Name)}");
                }

                for (int i = 0; i < lines.Count; i++)
                {
                    using (new DepthWrapper(fg))
                    {
                        using (new LineWrapper(fg))
                        {
                            fg.Append(lines[i]);
                            if (i != lines.Count - 1)
                            {
                                fg.Append(",");
                            }
                            else
                            {
                                fg.Append(");");
                            }
                        }
                    }
                }
            }
            fg.AppendLine();
        }

        protected override void GenerateCopyFieldsFrom(FileGeneration fg)
        {
        }

        protected override void GenerateSetNthObjectHasBeenSet(FileGeneration fg)
        {
        }

        public void GenerateCopyCtor(FileGeneration fg, string accessor, string rhs)
        {
        }

        protected override void GenerateStaticCopy_ToLoqui(FileGeneration fg)
        {
            fg.AppendLine($"return {this.ObjectName}.Copy(item);");
        }
    }
}
