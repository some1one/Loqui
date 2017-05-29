﻿using System;

namespace Loqui.Generation
{
    public class DictMaskFieldGeneration : MaskModuleField
    {
        public static string GetMaskString(IDictType dictType, string typeStr)
        {
            LoquiType keyLoquiType = dictType.KeyTypeGen as LoquiType;
            LoquiType valueLoquiType = dictType.ValueTypeGen as LoquiType;
            string keyStr = $"{(keyLoquiType == null ? typeStr : $"MaskItem<{typeStr}, {keyLoquiType.RefGen.Obj.GetMaskString(typeStr)}>")}";
            string valueStr = $"{(valueLoquiType == null ? typeStr : $"MaskItem<{typeStr}, {valueLoquiType.RefGen.Obj.GetMaskString(typeStr)}>")}";

            string itemStr;
            switch (dictType.Mode)
            {
                case DictMode.KeyValue:
                    itemStr = $"KeyValuePair<{keyStr}, {valueStr}>";
                    break;
                case DictMode.KeyedValue:
                    itemStr = valueStr;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return $"MaskItem<{typeStr}, IEnumerable<{itemStr}>>";
        }

        public override void GenerateForField(FileGeneration fg, TypeGeneration field, string typeStr)
        {
            fg.AppendLine($"public {GetMaskString(field as IDictType, typeStr)} {field.Name};");
        }

        public override void GenerateSetException(FileGeneration fg, TypeGeneration field)
        {
            fg.AppendLine($"this.{field.Name} = new {GetMaskString(field as IDictType, "Exception")}(ex, null);");
        }

        public override void GenerateSetMask(FileGeneration fg, TypeGeneration field)
        {
            fg.AppendLine($"this.{field.Name} = ({GetMaskString(field as IDictType, "Exception")})obj;");
        }

        public override void GenerateForCopyMask(FileGeneration fg, TypeGeneration field)
        {
            DictType dictType = field as DictType;
            LoquiType keyLoquiType = dictType.KeyTypeGen as LoquiType;
            LoquiType valueLoquiType = dictType.ValueTypeGen as LoquiType;

            switch (dictType.Mode)
            {
                case DictMode.KeyValue:
                    if (keyLoquiType == null && valueLoquiType == null)
                    {
                        fg.AppendLine($"public bool {field.Name};");
                    }
                    else if (keyLoquiType != null && valueLoquiType != null)
                    {
                        fg.AppendLine($"public MaskItem<bool, KeyValuePair<({nameof(RefCopyType)} Type, {keyLoquiType.RefGen.Obj.CopyMask} Mask), ({nameof(RefCopyType)} Type, {valueLoquiType.RefGen.Obj.CopyMask} Mask)>> {field.Name};");
                    }
                    else
                    {
                        LoquiType loqui = keyLoquiType ?? valueLoquiType;
                        fg.AppendLine($"public MaskItem<bool, ({nameof(RefCopyType)} Type, {loqui.RefGen.Obj.CopyMask} Mask)> {field.Name};");
                    }
                    break;
                case DictMode.KeyedValue:
                    fg.AppendLine($"public MaskItem<{nameof(CopyOption)}, {valueLoquiType.RefGen.Obj.CopyMask}> {field.Name};");
                    break;
                default:
                    break;
            }
        }

        public override void GenerateForErrorMaskToString(FileGeneration fg, TypeGeneration field, string accessor, bool topLevel)
        {
            DictType dictType = field as DictType;
            LoquiType keyLoquiType = dictType.KeyTypeGen as LoquiType;
            LoquiType valueLoquiType = dictType.ValueTypeGen as LoquiType;

            if (topLevel)
            {
                fg.AppendLine($"if ({accessor}.Overall != null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}({accessor}.Overall.ToString());");
                }
            }
            fg.AppendLine($"if ({accessor}.Specific != null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"foreach (var subItem in {accessor}{(topLevel ? ".Specific" : string.Empty)})");
                using (new BraceWrapper(fg))
                {
                    var keyFieldGen = this.Module.GetMaskModule(dictType.KeyTypeGen.GetType());
                    var valFieldGen = this.Module.GetMaskModule(dictType.ValueTypeGen.GetType());
                    fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"[\");");
                    fg.AppendLine($"using (new DepthWrapper(fg))");
                    using (new BraceWrapper(fg))
                    {
                        switch (dictType.Mode)
                        {
                            case DictMode.KeyValue:
                                fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"Key => [\");");
                                fg.AppendLine($"using (new DepthWrapper(fg))");
                                using (new BraceWrapper(fg))
                                {
                                    keyFieldGen.GenerateForErrorMaskToString(fg, dictType.KeyTypeGen, "subItem.Key", false);
                                }
                                fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"]\");");
                                fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"Value => [\");");
                                fg.AppendLine($"using (new DepthWrapper(fg))");
                                using (new BraceWrapper(fg))
                                {
                                    valFieldGen.GenerateForErrorMaskToString(fg, dictType.ValueTypeGen, "subItem.Value", false);
                                }
                                fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"]\");");
                                break;
                            case DictMode.KeyedValue:
                                keyFieldGen.GenerateForErrorMaskToString(fg, dictType.KeyTypeGen, "subItem", false);
                                break;
                            default:
                                break;
                        }
                    }
                    fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"]\");");
                }
            }
        }

        public override void GenerateForAllEqual(FileGeneration fg, TypeGeneration field)
        {
            DictType dictType = field as DictType;

            fg.AppendLine($"if ({field.Name} != null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"if (!eval(this.{field.Name}.Overall)) return false;");
                fg.AppendLine($"if ({field.Name}.Specific != null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"foreach (var item in {field.Name}.Specific)");
                    using (new BraceWrapper(fg))
                    {
                        switch (dictType.Mode)
                        {
                            case DictMode.KeyValue:
                                if (dictType.KeyTypeGen is LoquiType loquiKey)
                                {
                                    fg.AppendLine($"if (item.Key != null)");
                                    using (new BraceWrapper(fg))
                                    {
                                        fg.AppendLine($"if (!eval(item.Key.Overall)) return false;");
                                        fg.AppendLine($"if (!item.Key.Specific?.AllEqual(eval) ?? false) return false;");
                                    }
                                }
                                else
                                {
                                    fg.AppendLine($"if (!eval(item.Key)) return false;");
                                }
                                if (dictType.ValueTypeGen is LoquiType loquiVal)
                                {
                                    fg.AppendLine($"if (item.Value != null)");
                                    using (new BraceWrapper(fg))
                                    {
                                        fg.AppendLine($"if (!eval(item.Value.Overall)) return false;");
                                        fg.AppendLine($"if (!item.Value.Specific?.AllEqual(eval) ?? false) return false;");
                                    }
                                }
                                else
                                {
                                    fg.AppendLine($"if (!eval(item.Value)) return false;");
                                }
                                break;
                            case DictMode.KeyedValue:
                                fg.AppendLine($"if (!eval(item.Overall)) return false;");
                                fg.AppendLine($"if (!item.Specific?.AllEqual(eval) ?? false) return false;");
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
    }
}