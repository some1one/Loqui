using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class GenerationModuleCollection : IGenerationModule
    {
        private List<IGenerationModule> subModules = new List<IGenerationModule>();
        public string RegionString => null;
        public string Name => "Generation Submodules";
        public int? TimeoutMS;

        public GenerationModuleCollection(int? timeoutMS = 4000)
        {
            this.TimeoutMS = timeoutMS;
        }

        public M Add<M>(M module)
            where M : IGenerationModule
        {
            this.subModules.Add(module);
            return module;
        }

        public Task GenerateInVoid(ObjectGeneration obj, FileGeneration fg)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInVoid(obj, fg)
                                .TimeoutButContinue(
                                    Utility.TimeoutMS, 
                                    () => System.Console.WriteLine($"{subGen} {obj.Name} generate in void taking a long time."));
                        }
                    }));
        }

        public Task MiscellaneousGenerationActions(ObjectGeneration obj)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.MiscellaneousGenerationActions(obj)
                            .TimeoutButContinue(
                                Utility.TimeoutMS,
                                () => System.Console.WriteLine($"{subGen} {obj.Name} misc actions taking a long time."));
                    }));
        }

        public Task GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInClass(obj, fg)
                                .TimeoutButContinue(
                                    Utility.TimeoutMS,
                                    () => System.Console.WriteLine($"{subGen} {obj.Name} gen in class taking a long time."));
                        }
                    }));
        }

        public Task GenerateInNonGenericClass(ObjectGeneration obj, FileGeneration fg)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInNonGenericClass(obj, fg)
                                .TimeoutButContinue(
                                    Utility.TimeoutMS,
                                    () => System.Console.WriteLine($"{subGen} {obj.Name} gen in non generic class taking a long time."));
                        }
                    }));
        }

        public Task GenerateInCommon(ObjectGeneration obj, FileGeneration fg, MaskTypeSet maskTypes)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInCommon(obj, fg, maskTypes)
                                .TimeoutButContinue(
                                    Utility.TimeoutMS,
                                    () => System.Console.WriteLine($"{subGen} {obj.Name} gen common taking a long time."));
                        }
                    }));
        }

        public Task GenerateInCommonMixin(ObjectGeneration obj, FileGeneration fg)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInCommonMixin(obj, fg)
                                .TimeoutButContinue(
                                    Utility.TimeoutMS,
                                    () => System.Console.WriteLine($"{subGen} {obj.Name} gen common taking a long time."));
                        }
                    }));
        }

        public Task GenerateInCtor(ObjectGeneration obj, FileGeneration fg)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInCtor(obj, fg)
                                .TimeoutButContinue(
                                    Utility.TimeoutMS,
                                    () => System.Console.WriteLine($"{subGen} {obj.Name} gen ctor taking a long time."));
                        }
                    }));
        }

        public Task GenerateInInterface(ObjectGeneration obj, FileGeneration fg, bool internalInterface, bool getter)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInInterface(obj, fg, internalInterface, getter)
                                .TimeoutButContinue(
                                    Utility.TimeoutMS,
                                    () => System.Console.WriteLine($"{subGen} {obj.Name} gen interface getter taking a long time."));
                        }
                    }));
        }

        public Task GenerateInRegistration(ObjectGeneration obj, FileGeneration fg)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInRegistration(obj, fg)
                                .TimeoutButContinue(
                                    Utility.TimeoutMS,
                                    () => System.Console.WriteLine($"{subGen} {obj.Name} gen in registration taking a long time."));
                        }
                    }));
        }

        public Task GenerateInStaticCtor(ObjectGeneration obj, FileGeneration fg)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInStaticCtor(obj, fg)
                                .TimeoutButContinue(
                                    Utility.TimeoutMS,
                                    () => System.Console.WriteLine($"{subGen} {obj.Name} static ctor taking a long time."));
                        }
                    }));
        }

        public IAsyncEnumerable<(LoquiInterfaceType Location, string Interface)> Interfaces(ObjectGeneration obj)
        {
            return this.subModules.ToAsyncEnumerable().SelectMany((subGen) => subGen.Interfaces(obj));
        }

        public Task Modify(LoquiGenerator gen)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.Modify(gen)
                            .TimeoutButContinue(
                                Utility.TimeoutMS,
                                () => System.Console.WriteLine($"{subGen} modify taking a long time."));
                    }));
        }

        public Task PostFieldLoad(ObjectGeneration obj, TypeGeneration field, XElement node)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.PostFieldLoad(obj, field, node)
                            .TimeoutButContinue(
                                Utility.TimeoutMS,
                                () => System.Console.WriteLine($"{subGen} {obj.Name}.{field.Name} post field load taking a long time."));
                    }));
        }

        public Task LoadWrapup(ObjectGeneration obj)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.LoadWrapup(obj)
                            .TimeoutButContinue(
                                Utility.TimeoutMS,
                                () => System.Console.WriteLine($"{subGen} {obj.Name} load wrap up taking a long time."));
                    }));
        }

        public Task PostLoad(ObjectGeneration obj)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.PostLoad(obj)
                            .TimeoutButContinue(
                                Utility.TimeoutMS,
                                () => System.Console.WriteLine($"{subGen} {obj.Name} post load up taking a long time."));
                    }));
        }

        public Task PreLoad(ObjectGeneration obj)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.PreLoad(obj)
                            .TimeoutButContinue(
                                Utility.TimeoutMS,
                                () => System.Console.WriteLine($"{subGen} {obj.Name} pre load taking a long time."));
                    }));
        }

        public IAsyncEnumerable<string> RequiredUsingStatements(ObjectGeneration obj)
        {
            return this.subModules.ToAsyncEnumerable().SelectMany((subGen) => subGen.RequiredUsingStatements(obj));
        }

        public Task Resolve(ObjectGeneration obj)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.Resolve(obj)
                            .TimeoutButContinue(
                                Utility.TimeoutMS,
                                () => System.Console.WriteLine($"{subGen} {obj.Name} resolve taking a long time."));
                    }));
        }

        public Task FinalizeGeneration(ProtocolGeneration proto)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.FinalizeGeneration(proto)
                            .TimeoutButContinue(
                                Utility.TimeoutMS,
                                () => System.Console.WriteLine($"{subGen} finalize taking a long time."));
                    }));
        }

        public Task PrepareGeneration(ProtocolGeneration proto)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.PrepareGeneration(proto)
                            .TimeoutButContinue(
                                Utility.TimeoutMS,
                                () => System.Console.WriteLine($"{subGen} prepare taking a long time."));
                    }));
        }
    }
}
