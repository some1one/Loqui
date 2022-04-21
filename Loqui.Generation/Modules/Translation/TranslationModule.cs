using Loqui.Internal;
using Noggog;
using System.Xml.Linq;

namespace Loqui.Generation;

public abstract class TranslationModule<G> : GenerationModule, ITranslationModule
    where G : TranslationGeneration
{
    public LoquiGenerator Gen;
    public abstract string ModuleNickname { get; }
    public override string RegionString => $"{ModuleNickname} Translation";
    public abstract string Namespace { get; }
    public virtual string TranslationTerm => ModuleNickname;
    public abstract bool GenerateAbstractCreates { get; }
    public IEnumerable<TranslationModuleAPI> AllAPI => MainAPI.AsEnumerable().And(MinorAPIs);
    public TranslationModuleAPI MainAPI;
    protected List<TranslationModuleAPI> MinorAPIs = new List<TranslationModuleAPI>();
    public bool TranslationMaskParameter = true;
    public bool DoErrorMasks = true;
    public string TranslationWriteClassName(ObjectGeneration obj) => $"{obj.Name}{ModuleNickname}WriteTranslation";
    public string TranslationWriteClass(ObjectGeneration obj) => $"{TranslationWriteClassName(obj)}";
    public string TranslationCreateClassName(ObjectGeneration obj) => $"{obj.Name}{ModuleNickname}CreateTranslation";
    public string TranslationCreateClass(ObjectGeneration obj) => $"{TranslationCreateClassName(obj)}{obj.GetGenericTypes(MaskType.Normal)}";
    public string TranslationMixInClass(ObjectGeneration obj) => $"{obj.Name}{ModuleNickname}TranslationMixIn";
    public virtual bool DoTranslationInterface(ObjectGeneration obj) => true;
    public virtual bool DirectTranslationReference(ObjectGeneration obj) => false;
    public string TranslationWriteInterface => $"I{ModuleNickname}WriteTranslator";
    public string TranslationItemInterface => $"I{ModuleNickname}Item";
    public string TranslationWriteItemMember => $"{ModuleNickname}WriteTranslator";
    public virtual string TranslatorReference(ObjectGeneration obj, Accessor item) => $"(({TranslationWriteClass(obj)}){item}.{TranslationWriteItemMember})";

    public const string ErrorMaskKey = "ErrorMask";
    public const string ErrorMaskBuilderKey = "ErrorMaskBuilder";
    public const string DoMaskKey = "DoMasks";
    public const string TranslationMaskKey = "TranslationMask";

    public virtual string CreateFromPrefix => "CreateFrom";
    public virtual string CopyInFromPrefix => "CopyInFrom";
    public virtual string WriteToPrefix => "WriteTo";

    bool ITranslationModule.DoErrorMasks => true;

    protected Dictionary<Type, G> _typeGenerations = new Dictionary<Type, G>();

    public List<Func<ObjectGeneration, FileGeneration, Task>> ExtraTranslationTasks = new List<Func<ObjectGeneration, FileGeneration, Task>>();

    public TranslationModule(LoquiGenerator gen)
    {
        Gen = gen;
    }

    public override async Task PostFieldLoad(ObjectGeneration obj, TypeGeneration field, XElement node)
    {
        G transl;
        lock (_typeGenerations)
        {
            if (!_typeGenerations.TryGetValue(field.GetType(), out  transl)) return;
        }
        transl.Load(obj, field, node);
    }

    public override async IAsyncEnumerable<(LoquiInterfaceType Location, string Interface)> Interfaces(ObjectGeneration obj)
    {
        if (DoTranslationInterface(obj))
        {
            yield return (LoquiInterfaceType.IGetter, TranslationItemInterface);
        }
    }

    public override async IAsyncEnumerable<string> RequiredUsingStatements(ObjectGeneration obj)
    {
        yield return "System.Diagnostics";
        yield return "System.Diagnostics.CodeAnalysis";
        if (await AsyncImport(obj))
        {
            yield return "System.Threading.Tasks";
        }
    }

    public override async Task Modify(LoquiGenerator gen)
    {
    }

    public override async Task GenerateInInterface(ObjectGeneration obj, FileGeneration fg, bool internalInterface, bool getter)
    {
    }

    public override async Task GenerateInVoid(ObjectGeneration obj, FileGeneration fg)
    {
        using (new NamespaceWrapper(fg, obj.Namespace, fileScoped: false))
        {
            using (var args = new ClassWrapper(fg, TranslationWriteClass(obj)))
            {
                args.Partial = true;
                args.BaseClass = obj.HasLoquiBaseObject ? TranslationWriteClass(obj.BaseClass) : null;
                if (DoTranslationInterface(obj))
                {
                    args.Interfaces.Add(TranslationWriteInterface);
                }
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"public{obj.NewOverride()}readonly static {TranslationWriteClass(obj)} Instance = new {TranslationWriteClass(obj)}();");
                fg.AppendLine();

                await GenerateInTranslationWriteClass(obj, fg);
            }
            fg.AppendLine();

            using (var args = new ClassWrapper(fg, TranslationCreateClass(obj)))
            {
                args.Public = PermissionLevel.@internal;
                args.Partial = true;
                args.BaseClass = obj.HasLoquiBaseObject ? TranslationCreateClass(obj.BaseClass) : null;
                args.Wheres.AddRange(obj.GenerateWhereClauses(LoquiInterfaceType.ISetter, obj.Generics));
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"public{obj.NewOverride()}readonly static {TranslationCreateClass(obj)} Instance = new {TranslationCreateClass(obj)}();");
                fg.AppendLine();

                await GenerateInTranslationCreateClass(obj, fg);
            }
            fg.AppendLine();
        }

        using (new NamespaceWrapper(fg, obj.Namespace, fileScoped: false))
        {
            using (new RegionWrapper(fg, $"{ModuleNickname} Write Mixins"))
            {
                using (var args = new ClassWrapper(fg, TranslationMixInClass(obj)))
                {
                    args.Static = true;
                }
                using (new BraceWrapper(fg))
                {
                    await GenerateWriteMixIn(obj, fg);
                }
            }
            fg.AppendLine();
        }
    }

    public override async Task GenerateInCommonMixin(ObjectGeneration obj, FileGeneration fg)
    {
        await GenerateCopyInMixIn(obj, fg);
    }

    public async Task GenerateCopyInMixIn(ObjectGeneration obj, FileGeneration fg)
    {
        var asyncImport = await AsyncImport(obj);
        var errorLabel = await ErrorLabel(obj);

        if (DoErrorMasks)
        {
            fg.AppendLine("[DebuggerStepThrough]");
            using (var args = new FunctionWrapper(fg,
                       $"public static {await ObjectReturn(obj, maskReturn: true, hasReturn: false)} {CopyInFromPrefix}{TranslationTerm}{errorLabel}{obj.GetGenericTypes(GetMaskTypes(MaskType.Normal, MaskType.Error))}"))
            {
                args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.Direct, maskTypes: GetMaskTypes(MaskType.Normal, MaskType.Error)));
                args.Add($"this {obj.Interface(getter: false, internalInterface: true)} item");
                foreach (var (API, Public) in MainAPI.ReaderAPI.IterateAPI(
                             obj,
                             TranslationDirection.Reader,
                             new APILine(ErrorMaskKey, resolver: (o) => $"out {o.Mask(MaskType.Error)} errorMask", when: (o, i) => !asyncImport),
                             GetTranslationMaskParameter()))
                {
                    if (Public)
                    {
                        args.Add(API.Result);
                    }
                }
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("ErrorMaskBuilder errorMaskBuilder = new ErrorMaskBuilder();");
                using (var args = new ArgsWrapper(fg,
                           $"{Utility.Await(asyncImport)}{CopyInFromPrefix}{TranslationTerm}",
                           suffixLine: Utility.ConfigAwait(asyncImport)))
                {
                    args.AddPassArg("item");
                    args.Add(MainAPI.PassArgs(obj, TranslationDirection.Reader));
                    foreach (var customArgs in MainAPI.InternalFallbackArgs(obj, TranslationDirection.Reader))
                    {
                        args.Add(customArgs);
                    }
                    args.Add("errorMask: errorMaskBuilder");
                    if (TranslationMaskParameter)
                    {
                        args.Add("translationMask: translationMask?.GetCrystal()");
                    }
                }
                if (asyncImport)
                {
                    fg.AppendLine($"return {obj.Mask(MaskType.Error)}.Factory(errorMaskBuilder);");
                }
                else
                {
                    fg.AppendLine($"errorMask = {obj.Mask(MaskType.Error)}.Factory(errorMaskBuilder);");
                }
            }
            fg.AppendLine();
        }

        using (var args = new FunctionWrapper(fg,
                   $"public static {await ObjectReturn(obj, maskReturn: false, hasReturn: false)} {CopyInFromPrefix}{TranslationTerm}{obj.GetGenericTypes(MaskType.Normal)}"))
        {
            args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.ISetter, MaskType.Normal));
                    
            args.Add($"this {obj.Interface(getter: false, internalInterface: true)} item");
            foreach (var (API, Public) in MainAPI.ReaderAPI.IterateAPI(obj,
                         TranslationDirection.Reader,
                         DoErrorMasks ? new APILine(ErrorMaskKey, "ErrorMaskBuilder? errorMask") : null,
                         new APILine(TranslationMaskKey, $"{nameof(TranslationCrystal)}? translationMask", (o, i) => TranslationMaskParameter)))
            {
                args.Add(API.Result);
            }
        }
        using (new BraceWrapper(fg))
        {
            using (var args = new ArgsWrapper(fg,
                       $"{Utility.Await(asyncImport)}{obj.CommonClassInstance("item", LoquiInterfaceType.ISetter, CommonGenerics.Class)}.{CopyInFromPrefix}{TranslationTerm}"))
            {
                args.AddPassArg("item");
                args.Add(MainAPI.PassArgs(obj, TranslationDirection.Reader));
                foreach (var customArgs in MainAPI.InternalPassArgs(obj, TranslationDirection.Reader))
                {
                    args.Add(customArgs);
                }
                if (DoErrorMasks)
                {
                    args.AddPassArg($"errorMask");
                }
                if (TranslationMaskParameter)
                {
                    args.AddPassArg($"translationMask");
                }
            }
        }
        fg.AppendLine();

        foreach (var minorAPI in MinorAPIs)
        {
            if (!minorAPI.When?.Invoke(obj, TranslationDirection.Reader) ?? false) continue;
            if (obj.CanAssume())
            {
                using (var args = new FunctionWrapper(fg,
                           $"public static {await ObjectReturn(obj, maskReturn: false, hasReturn: false)} {CopyInFromPrefix}{TranslationTerm}{obj.GetGenericTypes(GetMaskTypes(MaskType.Normal))}"))
                {
                    args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.Direct, maskTypes: GetMaskTypes(MaskType.Normal)));
                    args.Add($"this {obj.Interface(getter: false, internalInterface: true)} item");
                    foreach (var (API, Public) in minorAPI.ReaderAPI.IterateAPI(obj, TranslationDirection.Reader))
                    {
                        if (Public)
                        {
                            args.Add(API.Result);
                        }
                    }
                    if (TranslationMaskParameter)
                    {
                        args.Add(GetTranslationMaskParameter().Resolver(obj).Result);
                    }
                }
                using (new BraceWrapper(fg))
                {
                    minorAPI.Funnel.InConverter(obj, fg, (accessor) =>
                    {
                        using (var args = new ArgsWrapper(fg,
                                   $"{Utility.Await(asyncImport)}{CopyInFromPrefix}{TranslationTerm}"))
                        {
                            args.AddPassArg("item");
                            foreach (var item in MainAPI.WrapAccessors(obj, TranslationDirection.Reader, accessor))
                            {
                                args.Add(item);
                            }
                            if (TranslationMaskParameter)
                            {
                                args.Add("translationMask: translationMask");
                            }
                        }
                    });
                }
                fg.AppendLine();
            }

            if (DoErrorMasks)
            {
                using (var args = new FunctionWrapper(fg,
                           $"public static {await ObjectReturn(obj, maskReturn: true, hasReturn: false)} {CopyInFromPrefix}{TranslationTerm}{errorLabel}{obj.GetGenericTypes(GetMaskTypes(MaskType.Normal, MaskType.Error))}"))
                {
                    args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.Direct, maskTypes: GetMaskTypes(MaskType.Normal, MaskType.Error)));
                    args.Add($"this {obj.Interface(getter: false, internalInterface: true)} item");
                    foreach (var (API, Public) in minorAPI.ReaderAPI.IterateAPI(
                                 obj,
                                 TranslationDirection.Reader,
                                 new APILine(ErrorMaskKey, resolver: (o) => $"out {o.Mask(MaskType.Error)} errorMask", when: (o, i) => !asyncImport),
                                 GetTranslationMaskParameter()))
                    {
                        if (Public)
                        {
                            args.Add(API.Result);
                        }
                    }
                }
                using (new BraceWrapper(fg))
                {
                    minorAPI.Funnel.InConverter(obj, fg, (accessor) =>
                    {
                        using (var args = new ArgsWrapper(fg,
                                   $"{(asyncImport ? "return " : null)}{Utility.Await(asyncImport)}{CopyInFromPrefix}{TranslationTerm}{errorLabel}"))
                        using (new DepthWrapper(fg))
                        {
                            args.AddPassArg("item");
                            foreach (var item in MainAPI.WrapAccessors(obj, TranslationDirection.Reader, accessor))
                            {
                                args.Add(item);
                            }
                            if (!asyncImport)
                            {
                                args.Add($"errorMask: out errorMask");
                            }
                            if (TranslationMaskParameter)
                            {
                                args.Add("translationMask: translationMask");
                            }
                        }
                    });
                }
                fg.AppendLine();

                using (var args = new FunctionWrapper(fg,
                           $"public static {await ObjectReturn(obj, maskReturn: false, hasReturn: false)} {CopyInFromPrefix}{TranslationTerm}{obj.GetGenericTypes(GetMaskTypes(MaskType.Normal, MaskType.Error))}"))
                {
                    args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.Direct, maskTypes: GetMaskTypes(MaskType.Normal, MaskType.Error)));
                    args.Add($"this {obj.Interface(getter: false, internalInterface: true)} item");
                    foreach (var (API, Public) in minorAPI.ReaderAPI.IterateAPI(
                                 obj,
                                 TranslationDirection.Reader,
                                 new APILine(ErrorMaskBuilderKey, $"ErrorMaskBuilder? errorMask"),
                                 GetTranslationMaskParameter()))
                    {
                        if (Public)
                        {
                            args.Add(API.Result);
                        }
                    }
                }
                using (new BraceWrapper(fg))
                {
                    minorAPI.Funnel.InConverter(obj, fg, (accessor) =>
                    {
                        using (var args = new ArgsWrapper(fg,
                                   $"{Utility.Await(asyncImport)}{CopyInFromPrefix}{TranslationTerm}"))
                        using (new DepthWrapper(fg))
                        {
                            args.AddPassArg("item");
                            foreach (var item in MainAPI.WrapFinalAccessors(obj, TranslationDirection.Reader, accessor))
                            {
                                args.Add(item);
                            }
                            args.AddPassArg("errorMask");
                            if (TranslationMaskParameter)
                            {
                                args.Add("translationMask: translationMask?.GetCrystal()");
                            }
                        }
                    });
                }
                fg.AppendLine();
            }
        }
    }

    public virtual async Task GenerateInTranslationWriteClass(ObjectGeneration obj, FileGeneration fg)
    {
        await TranslationWrite(obj, fg);
        foreach (var extra in ExtraTranslationTasks)
        {
            await extra(obj, fg);
        }
    }

    public virtual async Task GenerateInTranslationCreateClass(ObjectGeneration obj, FileGeneration fg)
    {
    }

    public override async Task MiscellaneousGenerationActions(ObjectGeneration obj)
    {
    }

    private async Task<string> ErrorLabel(ObjectGeneration obj) => await AsyncImport(obj) ? "WithErrorMask" : null;

    public override async Task GenerateInClass(ObjectGeneration obj, FileGeneration fg)
    {
        if (MainAPI == null)
        {
            throw new ArgumentException("Main API need to be set.");
        }
        if (DoTranslationInterface(obj))
        {
            await GenerateTranslationInterfaceImplementation(obj, fg);
        }
        if (!obj.Abstract || GenerateAbstractCreates)
        {
            await GenerateCreate(obj, fg);
        }
    }

    public async Task GenerateTranslationInterfaceImplementation(ObjectGeneration obj, FileGeneration fg)
    {
        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
        fg.AppendLine($"protected{await obj.FunctionOverride(async c => DoTranslationInterface(c))}object {TranslationWriteItemMember} => {TranslationWriteClass(obj)}.Instance;");
        if (!obj.BaseClassTrail().Any(b => DoTranslationInterface(b)))
        {
            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
            fg.AppendLine($"object {TranslationItemInterface}.{TranslationWriteItemMember} => this.{TranslationWriteItemMember};");
        }
        using (var args = new FunctionWrapper(fg,
                   $"void {TranslationItemInterface}.WriteTo{TranslationTerm}"))
        {
            FillWriterArgs(args, obj, objParam: null);
        }
        using (new BraceWrapper(fg))
        {
            using (var args = new ArgsWrapper(fg,
                       $"{TranslatorReference(obj, "this")}.Write"))
            {
                args.Add("item: this");
                foreach (var item in MainAPI.PassArgs(obj, TranslationDirection.Writer))
                {
                    args.Add(item);
                }
                foreach (var item in MainAPI.InternalPassArgs(obj, TranslationDirection.Writer))
                {
                    args.Add(item);
                }
                if (DoErrorMasks)
                {
                    args.AddPassArg($"errorMask");
                }
                if (TranslationMaskParameter)
                {
                    args.AddPassArg("translationMask");
                }
            }
        }
    }

    protected abstract Task GenerateNewSnippet(ObjectGeneration obj, FileGeneration fg);

    protected abstract Task GenerateCopyInSnippet(ObjectGeneration obj, FileGeneration fg, Accessor accessor);

    public MaskType[] GetMaskTypes(params MaskType[] otherMasks)
    {
        if (TranslationMaskParameter)
        {
            return otherMasks.And(MaskType.Translation).ToArray();
        }
        else
        {
            return otherMasks;
        }
    }

    public APILine GetTranslationMaskParameter(bool nullable = true)
    {
        return new APILine(
            nicknameKey: TranslationMaskKey,
            when: (obj, i) => TranslationMaskParameter,
            resolver: obj => $"{obj.Mask(MaskType.Translation)}{(nullable ? "?" : null)} translationMask{(nullable ? " = null" : null)}");
    }

    public virtual async Task<bool> AsyncImport(ObjectGeneration obj)
    {
        if (obj.HasLoquiBaseObject)
        {
            if (await AsyncImport(obj.BaseClass)) return true;
        }
        foreach (var field in obj.Fields)
        {
            if (!TryGetTypeGeneration(field.GetType(), out var gen)) continue;
            if (gen.IsAsync(field, read: true)) return true;
        }
        return false;
    }

    public virtual async Task<string> ObjectReturn(ObjectGeneration obj, bool maskReturn, bool hasReturn = true)
    {
        if (await AsyncImport(obj))
        {
            if (maskReturn)
            {
                if (hasReturn)
                {
                    return Utility.TaskWrap($"({obj.ObjectName} Object, {obj.Mask(MaskType.Error)} ErrorMask)");
                }
                else
                {
                    return Utility.TaskWrap($"{obj.Mask(MaskType.Error)}");
                }
            }
            else if (hasReturn)
            {
                return Utility.TaskWrap(obj.ObjectName);
            }
            else
            {
                return "async Task";
            }
        }
        else if (hasReturn)
        {
            return obj.ObjectName;
        }
        else
        {
            return "void";
        }
    }

    protected virtual bool GenerateMainCreate(ObjectGeneration obj) => true;

    protected virtual bool GenerateMainWrite(ObjectGeneration obj) => true;

    private async Task GenerateCreate(ObjectGeneration obj, FileGeneration fg)
    {
        using (new RegionWrapper(fg, $"{ModuleNickname} Create"))
        {
            var asyncImport = await AsyncImport(obj);
            var errorLabel = await ErrorLabel(obj);

            if (DoErrorMasks)
            {
                fg.AppendLine("[DebuggerStepThrough]");
                using (var args = new FunctionWrapper(fg,
                           $"public static {await ObjectReturn(obj, maskReturn: DoErrorMasks)} {CreateFromPrefix}{TranslationTerm}{errorLabel}{obj.GetGenericTypes(GetMaskTypes(MaskType.Error))}"))
                {
                    args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.Direct, maskTypes: GetMaskTypes(MaskType.Error)));
                    foreach (var (API, Public) in MainAPI.ReaderAPI.IterateAPI(
                                 obj,
                                 TranslationDirection.Reader,
                                 new APILine(ErrorMaskKey, resolver: (o) => $"out {o.Mask(MaskType.Error)} errorMask", when: (o, d) => !asyncImport),
                                 GetTranslationMaskParameter()))
                    {
                        if (Public)
                        {
                            args.Add(API.Result);
                        }
                    }
                }
                using (new BraceWrapper(fg))
                {
                    if (DoErrorMasks)
                    {
                        fg.AppendLine("ErrorMaskBuilder errorMaskBuilder = new ErrorMaskBuilder();");
                    }
                    using (var args = new ArgsWrapper(fg,
                               $"var ret = {Utility.Await(asyncImport)}{CreateFromPrefix}{TranslationTerm}",
                               suffixLine: Utility.ConfigAwait(asyncImport)))
                    {
                        args.Add(MainAPI.PassArgs(obj, TranslationDirection.Reader));
                        foreach (var customArgs in MainAPI.InternalFallbackArgs(obj, TranslationDirection.Reader))
                        {
                            args.Add(customArgs);
                        }
                        args.Add("errorMask: errorMaskBuilder");
                        if (TranslationMaskParameter)
                        {
                            args.Add("translationMask: translationMask?.GetCrystal()");
                        }
                    }
                    if (asyncImport)
                    {
                        fg.AppendLine($"return (ret, {obj.Mask(MaskType.Error)}.Factory(errorMaskBuilder));");
                    }
                    else
                    {
                        fg.AppendLine($"errorMask = {obj.Mask(MaskType.Error)}.Factory(errorMaskBuilder);");
                        fg.AppendLine("return ret;");
                    }
                }
                fg.AppendLine();
            }

            if (GenerateMainCreate(obj))
            {
                using (var args = new FunctionWrapper(fg,
                           $"public{obj.NewOverride()}static {await ObjectReturn(obj, maskReturn: false)} {CreateFromPrefix}{TranslationTerm}"))
                {
                    foreach (var (API, Public) in MainAPI.ReaderAPI.IterateAPI(obj,
                                 TranslationDirection.Reader,
                                 DoErrorMasks ? new APILine(ErrorMaskKey, "ErrorMaskBuilder? errorMask") : null,
                                 DoErrorMasks ? new APILine(TranslationMaskKey, $"{nameof(TranslationCrystal)}? translationMask", (o, i) => TranslationMaskParameter) : null))
                    {
                        args.Add(API.Result);
                    }
                }
                using (new BraceWrapper(fg))
                {
                    await GenerateNewSnippet(obj, fg);
                    using (var args = new ArgsWrapper(fg,
                               $"{Utility.Await(await AsyncImport(obj))}{obj.CommonClassInstance("ret", LoquiInterfaceType.ISetter, CommonGenerics.Class)}.{CopyInFromPrefix}{TranslationTerm}"))
                    {
                        args.Add("item: ret");
                        foreach (var arg in MainAPI.PassArgs(obj, TranslationDirection.Reader))
                        {
                            args.Add(arg);
                        }
                        foreach (var arg in MainAPI.InternalPassArgs(obj, TranslationDirection.Reader))
                        {
                            args.Add(arg);
                        }
                        if (DoErrorMasks)
                        {
                            args.AddPassArg("errorMask");
                        }
                        if (TranslationMaskParameter)
                        {
                            args.AddPassArg("translationMask");
                        }
                    }
                    fg.AppendLine("return ret;");
                }
                fg.AppendLine();
            }

            foreach (var minorAPI in MinorAPIs)
            {
                if (!minorAPI.When?.Invoke(obj, TranslationDirection.Reader) ?? false) continue;
                if (obj.CanAssume())
                {
                    using (var args = new FunctionWrapper(fg,
                               $"public static {await ObjectReturn(obj, maskReturn: false)} {CreateFromPrefix}{TranslationTerm}{obj.GetGenericTypes(GetMaskTypes())}"))
                    {
                        args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.Direct, maskTypes: GetMaskTypes()));
                        foreach (var (API, Public) in minorAPI.ReaderAPI.IterateAPI(obj, TranslationDirection.Reader))
                        {
                            if (Public)
                            {
                                args.Add(API.Result);
                            }
                        }
                        if (TranslationMaskParameter)
                        {
                            args.Add(GetTranslationMaskParameter().Resolver(obj).Result);
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                        minorAPI.Funnel.InConverter(obj, fg, (accessor) =>
                        {
                            using (var args = new ArgsWrapper(fg,
                                       $"return {Utility.Await(asyncImport)}{CreateFromPrefix}{TranslationTerm}"))
                            {
                                foreach (var item in MainAPI.WrapAccessors(obj, TranslationDirection.Reader, accessor))
                                {
                                    args.Add(item);
                                }
                                if (TranslationMaskParameter)
                                {
                                    args.Add("translationMask: translationMask");
                                }
                            }
                        });
                    }
                    fg.AppendLine();
                }

                if (DoErrorMasks)
                {
                    using (var args = new FunctionWrapper(fg,
                               $"public static {await ObjectReturn(obj, maskReturn: true)} {CreateFromPrefix}{TranslationTerm}{errorLabel}{obj.GetGenericTypes(GetMaskTypes(MaskType.Error))}"))
                    {
                        args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.Direct, maskTypes: GetMaskTypes(MaskType.Error)));
                        foreach (var (API, Public) in minorAPI.ReaderAPI.IterateAPI(
                                     obj,
                                     TranslationDirection.Reader,
                                     new APILine(ErrorMaskKey, resolver: (o) => $"out {o.Mask(MaskType.Error)} errorMask", when: (o, i) => !asyncImport),
                                     GetTranslationMaskParameter()))
                        {
                            if (Public)
                            {
                                args.Add(API.Result);
                            }
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                        minorAPI.Funnel.InConverter(obj, fg, (accessor) =>
                        {
                            using (var args = new ArgsWrapper(fg,
                                       $"return {Utility.Await(asyncImport)}{CreateFromPrefix}{TranslationTerm}{errorLabel}"))
                            using (new DepthWrapper(fg))
                            {
                                foreach (var item in MainAPI.WrapAccessors(obj, TranslationDirection.Reader, accessor))
                                {
                                    args.Add(item);
                                }
                                if (!asyncImport)
                                {
                                    args.Add($"errorMask: out errorMask");
                                }
                                if (TranslationMaskParameter)
                                {
                                    args.Add("translationMask: translationMask");
                                }
                            }
                        });
                    }
                    fg.AppendLine();
                }

                using (var args = new FunctionWrapper(fg,
                           $"public static {await ObjectReturn(obj, maskReturn: false)} {CreateFromPrefix}{TranslationTerm}{obj.GetGenericTypes(GetMaskTypes(MaskType.Error))}"))
                {
                    args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.Direct, maskTypes: GetMaskTypes(MaskType.Error)));
                    foreach (var (API, Public) in minorAPI.ReaderAPI.IterateAPI(
                                 obj,
                                 TranslationDirection.Reader,
                                 new APILine(ErrorMaskBuilderKey, $"ErrorMaskBuilder? errorMask"),
                                 GetTranslationMaskParameter()))
                    {
                        if (Public)
                        {
                            args.Add(API.Result);
                        }
                    }
                }
                using (new BraceWrapper(fg))
                {
                    minorAPI.Funnel.InConverter(obj, fg, (accessor) =>
                    {
                        using (var args = new ArgsWrapper(fg,
                                   $"return {Utility.Await(asyncImport)}{CreateFromPrefix}{TranslationTerm}"))
                        using (new DepthWrapper(fg))
                        {
                            foreach (var item in MainAPI.WrapFinalAccessors(obj, TranslationDirection.Reader, accessor))
                            {
                                args.Add(item);
                            }
                            if (DoErrorMasks)
                            {
                                args.Add($"errorMask: errorMask");
                            }
                            if (TranslationMaskParameter)
                            {
                                args.Add("translationMask: translationMask?.GetCrystal()");
                            }
                        }
                    });
                }
                fg.AppendLine();
            }
        }
    }

    public override async Task GenerateInCommon(ObjectGeneration obj, FileGeneration fg, MaskTypeSet maskTypes)
    {
        if (!maskTypes.Applicable(LoquiInterfaceType.ISetter, CommonGenerics.Class, MaskType.Normal)) return;

        using (var args = new FunctionWrapper(fg,
                   $"public virtual {await ObjectReturn(obj, maskReturn: false, hasReturn: false)} {CopyInFromPrefix}{TranslationTerm}"))
        {
            args.Add($"{obj.Interface(getter: false, internalInterface: true)} item");
            foreach (var (API, Public) in MainAPI.ReaderAPI.IterateAPI(obj,
                         TranslationDirection.Reader,
                         DoErrorMasks ? new APILine(ErrorMaskKey, "ErrorMaskBuilder? errorMask") : null,
                         new APILine(TranslationMaskKey, $"{nameof(TranslationCrystal)}? translationMask", (o, i) => TranslationMaskParameter)))
            {
                args.Add(API.Result);
            }
        }
        using (new BraceWrapper(fg))
        {
            if (!obj.Abstract || GenerateAbstractCreates)
            {
                await GenerateCopyInSnippet(obj, fg, "item");
            }
        }
        fg.AppendLine();

        foreach (var baseObj in obj.BaseClassTrail())
        {
            using (var args = new FunctionWrapper(fg,
                       $"public override {await ObjectReturn(baseObj, maskReturn: false, hasReturn: false)} {CopyInFromPrefix}{TranslationTerm}"))
            {
                args.Add($"{baseObj.Interface(getter: false, internalInterface: true)} item");
                foreach (var (API, Public) in MainAPI.ReaderAPI.IterateAPI(baseObj,
                             TranslationDirection.Reader,
                             DoErrorMasks ? new APILine(ErrorMaskKey, "ErrorMaskBuilder? errorMask") : null,
                             new APILine(TranslationMaskKey, $"{nameof(TranslationCrystal)}? translationMask", (o, i) => TranslationMaskParameter)))
                {
                    args.Add(API.Result);
                }
            }
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg,
                           $"{CopyInFromPrefix}{TranslationTerm}"))
                {
                    args.Add($"item: ({obj.ObjectName})item");
                    args.Add(MainAPI.PassArgs(obj, TranslationDirection.Reader));
                    args.Add(MainAPI.InternalPassArgs(obj, TranslationDirection.Reader));
                    if (DoErrorMasks)
                    {
                        args.AddPassArg($"errorMask");
                    }
                    if (TranslationMaskParameter)
                    {
                        args.AddPassArg($"translationMask");
                    }
                }
            }
            fg.AppendLine();
        }
    }

    private void FillWriterArgs(
        FunctionWrapper args,
        ObjectGeneration obj,
        bool? objParam = false,
        bool doFallbackCustom = true,
        bool addIndex = false)
    {
        foreach (var item in MainAPI.WriterAPI.MajorAPI)
        {
            if (item.TryResolve(obj, TranslationDirection.Writer, out var line))
            {
                args.Add(line.Result);
            }
        }
        if (objParam.HasValue)
        {
            args.Add($"{(objParam.Value ? "object" : obj.Interface(getter: true, internalInterface: true))} item");
        }
        if (doFallbackCustom)
        {
            foreach (var item in MainAPI.WriterAPI.CustomAPI)
            {
                if (item.API.TryResolve(obj, TranslationDirection.Writer, out var line))
                {
                    args.Add(line.Result);
                }
            }
        }
        if (DoErrorMasks)
        {
            args.Add($"ErrorMaskBuilder? errorMask");
        }
        if (DoErrorMasks && addIndex)
        {
            args.Add("int fieldIndex");
        }
        if (TranslationMaskParameter)
        {
            args.Add($"{nameof(TranslationCrystal)}? translationMask");
        }
        foreach (var item in MainAPI.WriterAPI.OptionalAPI)
        {
            if (item.TryResolve(obj, TranslationDirection.Writer, out var line))
            {
                args.Add(line.Result);
            }
        }
    }

    protected virtual async Task TranslationWrite(ObjectGeneration obj, FileGeneration fg)
    {
        using (var args = new FunctionWrapper(fg,
                   $"public{obj.Virtual()}void Write{obj.GetGenericTypes(MaskType.Normal)}"))
        {
            args.Wheres.AddRange(obj.GenerateWhereClauses(LoquiInterfaceType.IGetter, defs: obj.Generics));
            FillWriterArgs(args, obj);
        }
        using (new BraceWrapper(fg))
        {
            await GenerateWriteSnippet(obj, fg);
        }
        fg.AppendLine();

        using (var args = new FunctionWrapper(fg,
                   $"public{obj.FunctionOverride()}void Write"))
        {
            FillWriterArgs(args, obj, objParam: true);
        }
        using (new BraceWrapper(fg))
        {
            if (obj.Generics.Count > 0)
            {
                fg.AppendLine("throw new NotImplementedException();");
            }
            else
            {
                using (var args = new ArgsWrapper(fg, $"Write"))
                {
                    args.Add($"item: ({obj.Interface(getter: true, internalInterface: true)})item");
                    args.Add(MainAPI.PassArgs(obj, TranslationDirection.Writer));
                    args.Add(MainAPI.InternalPassArgs(obj, TranslationDirection.Writer));
                    if (DoErrorMasks)
                    {
                        args.Add($"errorMask: errorMask");
                    }
                    if (TranslationMaskParameter)
                    {
                        args.Add($"translationMask: translationMask");
                    }
                }
            }
        }
        fg.AppendLine();

        foreach (var baseObj in obj.BaseClassTrail())
        {
            using (var args = new FunctionWrapper(fg,
                       $"public override void Write{obj.GetGenericTypes(MaskType.Normal)}"))
            {
                FillWriterArgs(args, baseObj);
            }
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg, $"Write"))
                {
                    args.Add($"item: ({obj.Interface(getter: true, internalInterface: true)})item");
                    args.Add(MainAPI.PassArgs(obj, TranslationDirection.Writer));
                    args.Add(MainAPI.InternalPassArgs(obj, TranslationDirection.Writer));
                    if (DoErrorMasks)
                    {
                        args.Add($"errorMask: errorMask");
                    }
                    if (TranslationMaskParameter)
                    {
                        args.Add($"translationMask: translationMask");
                    }
                }
            }
            fg.AppendLine();
        }

        if (obj.IsTopClass
            && DoErrorMasks)
        {
            using (var args = new FunctionWrapper(fg,
                       $"public void Write{obj.GetGenericTypes(MaskType.Normal)}"))
            {
                args.Wheres.AddRange(obj.GenerateWhereClauses(LoquiInterfaceType.IGetter, defs: obj.Generics));
                FillWriterArgs(args, obj, addIndex: true);
            }
            using (new BraceWrapper(fg))
            {
                MaskGenerationUtility.WrapErrorFieldIndexPush(
                    fg,
                    errorMaskAccessor: "errorMask",
                    indexAccessor: "fieldIndex",
                    toDo: () =>
                    {
                        using (var args = new ArgsWrapper(fg, $"Write"))
                        {
                            args.Add($"item: ({obj.Interface(getter: true, internalInterface: true)})item");
                            args.Add(MainAPI.PassArgs(obj, TranslationDirection.Writer));
                            args.Add(MainAPI.InternalPassArgs(obj, TranslationDirection.Writer));
                            if (DoErrorMasks)
                            {
                                args.Add($"errorMask: errorMask");
                            }
                            if (TranslationMaskParameter)
                            {
                                args.Add($"translationMask: translationMask");
                            }
                        }
                    });
            }
            fg.AppendLine();
        }
    }

    protected abstract Task GenerateWriteSnippet(ObjectGeneration obj, FileGeneration fg);

    private async Task GenerateWriteMixIn(ObjectGeneration obj, FileGeneration fg)
    {
        if (GenerateMainWrite(obj))
        {
            if (DoErrorMasks)
            {
                using (var args = new FunctionWrapper(fg,
                           $"public static void {WriteToPrefix}{TranslationTerm}{obj.GetGenericTypes(GetMaskTypes(MaskType.Normal, MaskType.Error))}"))
                {
                    args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.IGetter, maskTypes: GetMaskTypes(MaskType.Normal, MaskType.Error)));
                    args.Add($"this {obj.Interface(getter: true, internalInterface: true)} item");
                    foreach (var item in MainAPI.WriterAPI.MajorAPI)
                    {
                        if (item.TryResolve(obj, TranslationDirection.Writer, out var line))
                        {
                            args.Add(line.Result);
                        }
                    }
                    foreach (var item in MainAPI.WriterAPI.CustomAPI)
                    {
                        if (!item.Public) continue;
                        if (item.API.TryResolve(obj, TranslationDirection.Writer, out var line))
                        {
                            args.Add(line.Result);
                        }
                    }
                    args.Add($"out {obj.Mask(MaskType.Error)} errorMask");
                    if (TranslationMaskParameter)
                    {
                        args.Add($"{obj.Mask(MaskType.Translation)}? translationMask = null");
                    }
                    foreach (var item in MainAPI.WriterAPI.OptionalAPI)
                    {
                        if (item.TryResolve(obj, TranslationDirection.Writer, out var line))
                        {
                            args.Add(line.Result);
                        }
                    }
                }
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("ErrorMaskBuilder errorMaskBuilder = new ErrorMaskBuilder();");
                    using (var args = new ArgsWrapper(fg,
                               $"{TranslatorReference(obj, "item")}.Write"))
                    {
                        args.Add("item: item");
                        foreach (var item in MainAPI.PassArgs(obj, TranslationDirection.Writer))
                        {
                            args.Add(item);
                        }
                        foreach (var item in MainAPI.InternalFallbackArgs(obj, TranslationDirection.Writer))
                        {
                            args.Add(item);
                        }
                        args.Add($"errorMask: errorMaskBuilder");
                        if (TranslationMaskParameter)
                        {
                            args.Add("translationMask: translationMask?.GetCrystal()");
                        }
                    }
                    fg.AppendLine($"errorMask = {obj.Mask(MaskType.Error)}.Factory(errorMaskBuilder);");
                }
                fg.AppendLine();

                foreach (var minorAPI in MinorAPIs)
                {
                    if (!minorAPI.When?.Invoke(obj, TranslationDirection.Writer) ?? false) continue;
                    using (var args = new FunctionWrapper(fg,
                               $"public static void {WriteToPrefix}{TranslationTerm}{obj.GetGenericTypes(GetMaskTypes(MaskType.Normal, MaskType.Error))}"))
                    {
                        args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.IGetter, maskTypes: GetMaskTypes(MaskType.Normal, MaskType.Error)));
                        args.Add($"this {obj.Interface(getter: true, internalInterface: true)} item");
                        foreach (var item in minorAPI.WriterAPI.IterateAPI(
                                     obj,
                                     TranslationDirection.Writer,
                                     new APILine(ErrorMaskKey, $"out {obj.Mask(MaskType.Error)} errorMask"),
                                     new APILine(TranslationMaskKey, $"{obj.Mask(MaskType.Translation)}? translationMask = null", (o, i) => TranslationMaskParameter)))
                        {
                            if (!item.Public) continue;
                            args.Add(item.API.Result);
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                        minorAPI.Funnel.OutConverter(obj, fg, (accessor) =>
                        {
                            using (var args = new ArgsWrapper(fg,
                                       $"{WriteToPrefix}{TranslationTerm}"))
                            using (new DepthWrapper(fg))
                            {
                                args.Add("item: item");
                                foreach (var item in MainAPI.WrapAccessors(obj, TranslationDirection.Writer, accessor))
                                {
                                    args.Add(item);
                                }
                                args.Add($"errorMask: out errorMask");
                                if (TranslationMaskParameter)
                                {
                                    args.Add("translationMask: translationMask");
                                }
                            }
                        });
                    }
                    fg.AppendLine();

                    if (obj.IsTopClass)
                    {
                        using (var args = new FunctionWrapper(fg,
                                   $"public static void {WriteToPrefix}{TranslationTerm}{obj.GetGenericTypes(MaskType.Normal)}"))
                        {
                            args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.IGetter, maskTypes: MaskType.Normal));
                            args.Add($"this {obj.Interface(getter: true, internalInterface: true)} item");
                            foreach (var item in minorAPI.WriterAPI.IterateAPI(
                                         obj,
                                         TranslationDirection.Writer,
                                         new APILine(ErrorMaskKey, $"{nameof(ErrorMaskBuilder)}? errorMask"),
                                         new APILine(TranslationMaskKey, $"{nameof(TranslationCrystal)}? translationMask = null", (o, i) => TranslationMaskParameter)))
                            {
                                if (!item.Public) continue;
                                args.Add(item.API.Result);
                            }
                        }
                        using (new BraceWrapper(fg))
                        {
                            minorAPI.Funnel.OutConverter(obj, fg, (accessor) =>
                            {
                                using (var args = new ArgsWrapper(fg,
                                           $"{WriteToPrefix}{TranslationTerm}{obj.GetGenericTypes(MaskType.Normal)}"))
                                using (new DepthWrapper(fg))
                                {
                                    args.Add("item: item");
                                    foreach (var item in MainAPI.WrapAccessors(obj, TranslationDirection.Writer, accessor))
                                    {
                                        args.Add(item);
                                    }
                                    args.Add($"errorMask: errorMask");
                                    if (TranslationMaskParameter)
                                    {
                                        args.Add("translationMask: translationMask");
                                    }
                                }
                            });
                        }
                        fg.AppendLine();
                    }
                }
            }
        }

        if (obj.IsTopClass)
        {
            if (GenerateMainWrite(obj))
            {
                if (DoErrorMasks)
                {
                    using (var args = new FunctionWrapper(fg,
                               $"public static void {WriteToPrefix}{TranslationTerm}{obj.GetGenericTypes(MaskType.Normal)}"))
                    {
                        args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.IGetter, maskTypes: MaskType.Normal));
                        args.Add($"this {obj.Interface(getter: true, internalInterface: true)} item");
                        foreach (var item in MainAPI.WriterAPI.MajorAPI)
                        {
                            if (item.TryResolve(obj, TranslationDirection.Writer, out var line))
                            {
                                args.Add(line.Result);
                            }
                        }
                        foreach (var item in MainAPI.WriterAPI.CustomAPI)
                        {
                            if (!item.Public) continue;
                            if (item.API.TryResolve(obj, TranslationDirection.Writer, out var line))
                            {
                                args.Add(line.Result);
                            }
                        }
                        args.Add($"ErrorMaskBuilder? errorMask");
                        if (TranslationMaskParameter)
                        {
                            args.Add($"{nameof(TranslationCrystal)}? translationMask = null");
                        }
                        foreach (var item in MainAPI.WriterAPI.OptionalAPI)
                        {
                            if (item.TryResolve(obj, TranslationDirection.Writer, out var line))
                            {
                                args.Add(line.Result);
                            }
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                        using (var args = new ArgsWrapper(fg,
                                   $"{TranslatorReference(obj, "item")}.Write"))
                        {
                            args.Add("item: item");
                            foreach (var item in MainAPI.PassArgs(obj, TranslationDirection.Writer))
                            {
                                args.Add(item);
                            }
                            foreach (var item in MainAPI.InternalFallbackArgs(obj, TranslationDirection.Writer))
                            {
                                args.Add(item);
                            }
                            args.Add($"errorMask: errorMask");
                            if (TranslationMaskParameter)
                            {
                                args.Add("translationMask: translationMask");
                            }
                        }
                    }
                    fg.AppendLine();
                }

                using (var args = new FunctionWrapper(fg,
                           $"public static void {WriteToPrefix}{TranslationTerm}{obj.GetGenericTypes(GetMaskTypes(MaskType.Normal, MaskType.Error))}"))
                {
                    args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.IGetter, maskTypes: GetMaskTypes(MaskType.Normal, MaskType.Error)));
                    args.Add($"this {obj.Interface(getter: true, internalInterface: true)} item");
                    foreach (var (API, Public) in MainAPI.WriterAPI.IterateAPI(obj, TranslationDirection.Writer))
                    {
                        if (Public)
                        {
                            args.Add(API.Result);
                        }
                    }
                    if (TranslationMaskParameter)
                    {
                        args.Add($"{obj.Mask(MaskType.Translation)}? translationMask = null");
                    }
                }
                using (new BraceWrapper(fg))
                {
                    CustomMainWriteMixInPreLoad(obj, fg);
                    using (var args = new ArgsWrapper(fg,
                               $"{TranslatorReference(obj, "item")}.Write"))
                    {
                        args.Add("item: item");
                        foreach (var item in MainAPI.PassArgs(obj, TranslationDirection.Writer))
                        {
                            args.Add(item);
                        }
                        foreach (var item in MainAPI.InternalFallbackArgs(obj, TranslationDirection.Writer))
                        {
                            args.Add(item);
                        }
                        if (DoErrorMasks)
                        {
                            args.Add($"errorMask: null");
                        }
                        if (TranslationMaskParameter)
                        {
                            args.Add("translationMask: translationMask?.GetCrystal()");
                        }
                    }
                }
                fg.AppendLine();
            }

            foreach (var minorAPI in MinorAPIs)
            {
                if (!minorAPI.When?.Invoke(obj, TranslationDirection.Writer) ?? false) continue;
                using (var args = new FunctionWrapper(fg,
                           $"public static void {WriteToPrefix}{TranslationTerm}{obj.GetGenericTypes(GetMaskTypes(MaskType.Normal))}"))
                {
                    args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.IGetter, maskTypes: GetMaskTypes(MaskType.Normal)));
                    args.Add($"this {obj.Interface(getter: true, internalInterface: true)} item");
                    foreach (var (API, Public) in minorAPI.WriterAPI.IterateAPI(obj, TranslationDirection.Writer))
                    {
                        if (Public)
                        {
                            args.Add(API.Result);
                        }
                    }
                }
                using (new BraceWrapper(fg))
                {
                    minorAPI.Funnel.OutConverter(obj, fg, (accessor) =>
                    {
                        using (var args = new ArgsWrapper(fg,
                                   $"{TranslatorReference(obj, "item")}.Write"))
                        {
                            args.Add("item: item");
                            foreach (var item in MainAPI.WrapAccessors(obj, TranslationDirection.Writer, accessor))
                            {
                                args.Add(item);
                            }
                            foreach (var item in MainAPI.InternalFallbackArgs(obj, TranslationDirection.Writer))
                            {
                                args.Add(item);
                            }
                            if (DoErrorMasks)
                            {
                                args.Add($"errorMask: null");
                            }
                            if (TranslationMaskParameter)
                            {
                                args.Add("translationMask: null");
                            }
                        }
                    });
                }
                fg.AppendLine();
            }
        }
    }

    public void AddTypeAssociation<T>(G transl, bool overrideExisting = false)
        where T : TypeGeneration
    {
        lock (_typeGenerations)
        {
            if (overrideExisting)
            {
                _typeGenerations[typeof(T)] = transl;
            }
            else
            {
                _typeGenerations.Add(typeof(T), transl);
            }
        }
    }

    public bool TryGetTypeGeneration(Type t, out G gen)
    {
        lock (_typeGenerations)
        {
            if (!_typeGenerations.TryGetValue(t, out gen))
            {
                foreach (var kv in _typeGenerations.ToList())
                {
                    if (t.InheritsFrom(kv.Key))
                    {
                        _typeGenerations[t] = kv.Value;
                        gen = kv.Value;
                        return true;
                    }
                }
                return false;
            }
        }
        return true;
    }

    public G GetTypeGeneration(Type t)
    {
        lock (_typeGenerations)
        {
            return _typeGenerations[t];
        }
    }

    public override async Task GenerateInRegistration(ObjectGeneration obj, FileGeneration fg)
    {
        await base.GenerateInRegistration(obj, fg);
        fg.AppendLine($"public static readonly Type {ModuleNickname}WriteTranslation = typeof({TranslationWriteClassName(obj)});");
    }

    public override async Task LoadWrapup(ObjectGeneration obj)
    {
        await base.LoadWrapup(obj);
        foreach (var gen in obj.Generics.Values)
        {
            if (!gen.Loqui) continue;
            gen.Add(TranslationItemInterface);
        }
    }

    public virtual void CustomMainWriteMixInPreLoad(ObjectGeneration obj, FileGeneration fg)
    {
    }

    public abstract void ReplaceTypeAssociation<Target, Replacement>()
        where Target : TypeGeneration
        where Replacement : TypeGeneration;
}