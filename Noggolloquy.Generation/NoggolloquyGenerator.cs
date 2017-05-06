﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Noggog;

namespace Noggolloquy.Generation
{
    public class NoggolloquyGenerator
    {
        private const string CSPROJ_NAMESPACE = "http://schemas.microsoft.com/developer/msbuild/2003";
        private List<DirectoryInfo> sourceFolders = new List<DirectoryInfo>();
        private HashSet<FileInfo> sourceFiles = new HashSet<FileInfo>();
        Dictionary<ProtocolDefinition, ProtocolGeneration> targetData = new Dictionary<ProtocolDefinition, ProtocolGeneration>();
        Dictionary<ushort, ProtocolDefinition> idUsageDict = new Dictionary<ushort, ProtocolDefinition>();
        List<FileInfo> projectsToModify = new List<FileInfo>();
        private List<DirectoryInfo> addedTargetDirs = new List<DirectoryInfo>();
        Dictionary<StringCaseAgnostic, Type> typeDict = new Dictionary<StringCaseAgnostic, Type>();
        public string DefaultNamespace;
        public List<GenerationInterface> GenerationInterfaces = new List<GenerationInterface>();
        public List<GenerationModule> GenerationModules = new List<GenerationModule>();
        public DirectoryInfo CommonGenerationFolder;
        public Dictionary<StringCaseAgnostic, List<ObjectGeneration>> ObjectGenerationsByDir = new Dictionary<StringCaseAgnostic, List<ObjectGeneration>>();
        public HashSet<StringCaseAgnostic> GeneratedFiles = new HashSet<StringCaseAgnostic>();
        public static string Namespace => "http://tempuri.org/NoggolloquySource.xsd";
        public List<string> Namespaces = new List<string>();
        public NoggInterfaceType InterfaceTypeDefault = NoggInterfaceType.Direct;
        public bool ProtectedDefault;
        public bool DerivativeDefault;
        public bool RaisePropertyChangedDefault = true;

        public NoggolloquyGenerator(DirectoryInfo commonGenerationFolder, bool typical = true)
        {
            this.CommonGenerationFolder = commonGenerationFolder;
            if (typical)
            {
                this.AddTypicalTypeAssociations();
                this.Add(new XmlTranslationGeneration());
                this.Add(new MaskModule());
                this.AddSearchableFolder(this.CommonGenerationFolder);
            }
        }

        public void AddTypicalTypeAssociations()
        {
            AddTypeAssociation<Int8Type>("Int8");
            AddTypeAssociation<Int8NullType>("Int8Null");
            AddTypeAssociation<Int16Type>("Int16");
            AddTypeAssociation<Int16NullType>("Int16Null");
            AddTypeAssociation<Int32Type>("Int32");
            AddTypeAssociation<Int32NullType>("Int32Null");
            AddTypeAssociation<Int64Type>("Int64");
            AddTypeAssociation<Int64NullType>("Int64Null");
            AddTypeAssociation<UInt8Type>("UInt8");
            AddTypeAssociation<UInt8NullType>("UInt8Null");
            AddTypeAssociation<UInt16Type>("UInt16");
            AddTypeAssociation<UInt16NullType>("UInt16Null");
            AddTypeAssociation<UInt32Type>("UInt32");
            AddTypeAssociation<UInt32NullType>("UInt32Null");
            AddTypeAssociation<UInt64Type>("UInt64");
            AddTypeAssociation<UInt64NullType>("UInt64Null");
            AddTypeAssociation<P2IntType>("P2Int");
            AddTypeAssociation<P2IntNullType>("P2IntNull");
            AddTypeAssociation<P3IntType>("P3Int");
            AddTypeAssociation<P3IntNullType>("P3IntNull");
            AddTypeAssociation<P3DoubleType>("P3Double");
            AddTypeAssociation<P3DoubleNullType>("P3DoubleNull");
            AddTypeAssociation<BoolType>("Bool");
            AddTypeAssociation<BoolNullType>("BoolNull");
            AddTypeAssociation<CharType>("Char");
            AddTypeAssociation<CharNullType>("CharNull");
            AddTypeAssociation<RangeIntType>("RangeInt");
            AddTypeAssociation<RangeIntNullType>("RangeIntNull");
            AddTypeAssociation<RangeDoubleType>("RangeDouble");
            AddTypeAssociation<RangeDoubleNullType>("RangeDoubleNull");
            AddTypeAssociation<PercentType>("Percent");
            AddTypeAssociation<PercentNullType>("PercentNull");
            AddTypeAssociation<FloatType>("Float");
            AddTypeAssociation<FloatNullType>("FloatNull");
            AddTypeAssociation<UDoubleType>("UDouble");
            AddTypeAssociation<UDoubleNullType>("UDoubleNull");
            AddTypeAssociation<DoubleType>("Double");
            AddTypeAssociation<DoubleNullType>("DoubleNull");
            AddTypeAssociation<NoggType>("Ref");
            AddTypeAssociation<ListType>("List");
            AddTypeAssociation<DictType>("Dict");
            AddTypeAssociation<Array2DType>("Array2D");
            AddTypeAssociation<EnumType>("Enum");
            AddTypeAssociation<StringType>("String");
            AddTypeAssociation<UnsafeType>("UnsafeObject");
            AddTypeAssociation<WildcardType>("Wildcard");
            AddTypeAssociation<FieldBatchPointerType>("FieldBatch");
        }

        public void AddTypeAssociation<T>(StringCaseAgnostic key, bool overrideExisting = false)
            where T : TypeGeneration
        {
            if (!overrideExisting && typeDict.ContainsKey(key))
            {
                throw new ArgumentException($"Cannot add two type associations on the same key: {key}");
            }

            typeDict[key] = typeof(T);
        }

        public void AddProtocol(ProtocolGeneration protoGen)
        {
            this.targetData[protoGen.Definition] = protoGen;
        }

        public void AddProjectToModify(FileInfo projFile)
        {
            this.projectsToModify.Add(projFile);
        }

        public void AddSearchableFolder(DirectoryInfo dir)
        {
            AddSpecificFolders(dir);
            foreach (var d in dir.GetDirectories())
            {
                AddSearchableFolder(d);
            }
        }

        public void AddSpecificFolders(params DirectoryInfo[] dirs)
        {
            this.sourceFolders.AddRange(dirs);
        }

        protected void LoadSpecificFolders(IEnumerable<DirectoryInfo> dirs)
        {
            foreach (var dir in dirs)
            {
                addedTargetDirs.Add(dir);
                dir.Refresh();
                LoadSpecificFile(dir.EnumerateFiles().ToArray());
            }
        }
        
        public void AddSpecificFile(params FileInfo[] files)
        {
            this.sourceFiles.Add(files);
        }

        protected void LoadSpecificFile(IEnumerable<FileInfo> files)
        {
            foreach (var protocolGen in this.targetData.Values)
            {
                protocolGen.LoadInitialObjects(files
                    .Where((f) => ".XML".EqualsIgnoreCase(f.Extension))
                    .Select(
                    (f) =>
                    {
                        XDocument doc;
                        using (var stream = new FileStream(f.FullName, FileMode.Open))
                        {
                            doc = XDocument.Load(stream);
                        }
                        return new System.Tuple<XDocument, FileInfo>(doc, f);
                    })
                    .Where((t) =>
                    {
                        var noggNode = t.Item1.Element(XName.Get("Noggolloquy", NoggolloquyGenerator.Namespace));
                        if (noggNode == null) return false;
                        var protoNode = noggNode.Element(XName.Get("Protocol", NoggolloquyGenerator.Namespace));

                        if (!protoNode.TryGetAttribute<ushort>("ProtocolID", out ushort protoID))
                        {
                            throw new ArgumentException();
                        }

                        return protocolGen.Definition.Key.Equals(
                            new ProtocolKey(protoID));
                    }));

                if (protocolGen.Empty) return;

                idUsageDict[protocolGen.Definition.Key.ProtocolID] = protocolGen.Definition;
            }
        }

        public void Add(GenerationModule transl)
        {
            GenerationModules.Add(transl);
        }

        public void Add(GenerationInterface interf)
        {
            GenerationInterfaces.Add(interf);
        }

        public bool TryGetProtocol(ushort protoID, out KeyValuePair<ProtocolDefinition, ProtocolGeneration> protoGen)
        {
            if (!this.idUsageDict.TryGetValue(protoID, out var def)
                || !this.targetData.TryGetValue(def, out var gen))
            {
                protoGen = default(KeyValuePair<ProtocolDefinition, ProtocolGeneration>);
                return false;
            }
            protoGen = new KeyValuePair<ProtocolDefinition, ProtocolGeneration>(
                def,
                gen);
            return true;
        }

        public void Generate()
        {
            this.LoadSpecificFolders(this.sourceFolders);
            this.LoadSpecificFile(this.sourceFiles);

            foreach (var mod in this.GenerationModules)
            {
                mod.Modify(this);
            }

            ResolveIDs();

            foreach (var protoGen in this.targetData.Values)
            {
                protoGen.Generate();
            }

            foreach (var file in this.projectsToModify)
            {
                ModifyProject(file);
            }

            DeleteOldAutogenerated();
        }

        private void ResolveIDs()
        {
            foreach (var proto in this.targetData.Values)
            {
                HashSet<ushort> usedIDs = new HashSet<ushort>();
                foreach (var obj in proto.ObjectGenerationsByID.Values)
                {
                    if (!obj.ID.HasValue) continue;
                    if (!usedIDs.Add(obj.ID.Value))
                    {
                        throw new ArgumentException($"Two objects in protocol {proto.Definition} have the same ID {obj.ID.Value}");
                    }
                }

                ushort max = (ushort)(usedIDs.Count > 0 ? (usedIDs.Max() + 1) : 1);

                foreach (var obj in proto.ObjectGenerationsByID.Values)
                {
                    if (!obj.ID.HasValue)
                    {
                        obj.ID = max++;
                    }
                }
            }
        }

        public bool TryGetTypeGeneration(StringCaseAgnostic name, out TypeGeneration gen)
        {
            if (!typeDict.TryGetValue(name, out Type t))
            {
                gen = null;
                return false;
            }

            gen = Activator.CreateInstance(t) as TypeGeneration;
            return true;
        }

        private void DeleteOldAutogenerated()
        {
            foreach (var dir in addedTargetDirs)
            {
                foreach (var file in dir.GetFiles())
                {
                    if (file.Name.Contains(ObjectGeneration.AUTOGENERATED)
                        && !file.Name.EndsWith(".meta")
                        && !this.GeneratedFiles.Contains(file.FullName))
                    {
                        file.Delete();
                    }
                }
            }
        }

        private void ModifyProject(FileInfo projFile)
        {
            XDocument doc;
            using (var stream = new FileStream(projFile.FullName, FileMode.Open))
            {
                doc = XDocument.Load(stream);
            }
            bool modified = false;
            var projNode = doc.Element(XName.Get("Project", CSPROJ_NAMESPACE));
            foreach (var compile in projNode.Elements(XName.Get("ItemGroup", CSPROJ_NAMESPACE))
                .SelectMany((itemGroup) => itemGroup.Elements(XName.Get("Compile", CSPROJ_NAMESPACE))))
            {
                if (!compile.TryGetAttribute("Include", out XAttribute includeAttr)) continue;
                FileInfo file = new FileInfo(projFile.Directory.FullName + "/" + includeAttr.Value);
                if (!TryGetMatchingObjectGeneration(file, out ObjectGeneration objGen)) continue;
                if (file.Name.EqualsIgnoreCase(objGen.SourceXMLFile.Name)) continue;
                var depName = XName.Get("DependentUpon", CSPROJ_NAMESPACE);
                if (compile.Element(depName) != null) continue;

                var depElem = new XElement(depName)
                {
                    Value = objGen.SourceXMLFile.Name
                };
                compile.Add(depElem);
                modified = true;
            }

            if (!modified) return;

            using (XmlTextWriter writer = new XmlTextWriter(
                new FileStream(projFile.FullName, FileMode.Create), Encoding.ASCII))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 3;
                doc.WriteTo(writer);
            }
        }

        public bool TryGetMatchingObjectGeneration(FileInfo csFile, out ObjectGeneration objGen)
        {
            if (this.ObjectGenerationsByDir.TryGetValue(csFile.Directory.FullName, out List<ObjectGeneration> objs))
            {
                foreach (var obj in objs)
                {
                    if (csFile.Name.Equals($"{obj.Name}.cs")
                        || csFile.Name.Equals($"{obj.Name}_{ObjectGeneration.AUTOGENERATED}.cs"))
                    {
                        objGen = obj;
                        return true;
                    }
                }
            }

            objGen = null;
            return false;
        }
    }
}