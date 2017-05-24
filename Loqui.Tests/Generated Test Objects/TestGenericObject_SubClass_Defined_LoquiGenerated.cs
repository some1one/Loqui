/*
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
 * Autogenerated by Loqui.  Do not manually change.
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Loqui;
using Noggog;
using Noggog.Notifying;
using Loqui.Tests.Internals;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Noggog.Xml;
using Loqui.Xml;

namespace Loqui.Tests
{
    #region Class
    public partial class TestGenericObject_SubClass_Defined<RBase> : TestGenericObject<long, RBase, ObjectToRef>, ITestGenericObject_SubClass_Defined<RBase>, ILoquiObjectSetter, IEquatable<TestGenericObject_SubClass_Defined<RBase>>
        where RBase : ObjectToRef, ILoquiObject, ILoquiObjectGetter
    {
        ILoquiRegistration ILoquiObject.Registration => TestGenericObject_SubClass_Defined_Registration.Instance;
        public new static TestGenericObject_SubClass_Defined_Registration Registration => TestGenericObject_SubClass_Defined_Registration.Instance;

        public TestGenericObject_SubClass_Defined()
        {
            CustomCtor();
        }
        partial void CustomCtor();

        #region Loqui Getter Interface

        protected override object GetNthObject(ushort index) => TestGenericObject_SubClass_DefinedCommon.GetNthObject<RBase>(index, this);

        protected override bool GetNthObjectHasBeenSet(ushort index) => TestGenericObject_SubClass_DefinedCommon.GetNthObjectHasBeenSet<RBase>(index, this);

        protected override void UnsetNthObject(ushort index, NotifyingUnsetParameters? cmds) => TestGenericObject_SubClass_DefinedCommon.UnsetNthObject<RBase>(index, this, cmds);

        #endregion

        #region Loqui Interface
        protected override void SetNthObjectHasBeenSet(ushort index, bool on)
        {
            TestGenericObject_SubClass_DefinedCommon.SetNthObjectHasBeenSet<RBase>(index, on, this);
        }

        public void CopyFieldsFrom(
            ITestGenericObject_SubClass_DefinedGetter<RBase> rhs,
            TestGenericObject_SubClass_Defined_CopyMask copyMask = null,
            ITestGenericObject_SubClass_DefinedGetter<RBase> def = null,
            NotifyingFireParameters? cmds = null)
        {
            TestGenericObject_SubClass_DefinedCommon.CopyFieldsFrom<RBase>(
                item: this,
                rhs: rhs,
                def: def,
                doErrorMask: false,
                errorMask: null,
                copyMask: copyMask,
                cmds: cmds);
        }

        public void CopyFieldsFrom(
            ITestGenericObject_SubClass_DefinedGetter<RBase> rhs,
            out TestGenericObject_SubClass_Defined_ErrorMask errorMask,
            TestGenericObject_SubClass_Defined_CopyMask copyMask = null,
            ITestGenericObject_SubClass_DefinedGetter<RBase> def = null,
            NotifyingFireParameters? cmds = null)
        {
            TestGenericObject_SubClass_Defined_ErrorMask retErrorMask = null;
            Func<TestGenericObject_SubClass_Defined_ErrorMask> maskGetter = () =>
            {
                if (retErrorMask == null)
                {
                    retErrorMask = new TestGenericObject_SubClass_Defined_ErrorMask();
                }
                return retErrorMask;
            };
            TestGenericObject_SubClass_DefinedCommon.CopyFieldsFrom<RBase>(
                item: this,
                rhs: rhs,
                def: def,
                doErrorMask: false,
                errorMask: maskGetter,
                copyMask: copyMask,
                cmds: cmds);
            errorMask = retErrorMask;
        }

        #endregion

        #region To String
        public override string ToString()
        {
            return ILoquiObjectExt.PrintPretty(this);
        }
        #endregion


        #region Equals and Hash
        public override bool Equals(object obj)
        {
            if (!(obj is TestGenericObject_SubClass_Defined<RBase> rhs)) return false;
            return Equals(rhs);
        }

        public bool Equals(TestGenericObject_SubClass_Defined<RBase> rhs)
        {
            return base.Equals(rhs);
        }

        public override int GetHashCode()
        {
            int ret = 0;
            ret = ret.CombineHashCode(base.GetHashCode());
            return ret;
        }

        #endregion


        #region XML Translation
        public new static TestGenericObject_SubClass_Defined<RBase> Create_XML(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return Create_XML(XElement.Parse(reader.ReadToEnd()));
            }
        }

        public new static TestGenericObject_SubClass_Defined<RBase> Create_XML(XElement root)
        {
            var ret = new TestGenericObject_SubClass_Defined<RBase>();
            LoquiXmlTranslation<TestGenericObject_SubClass_Defined<RBase>, TestGenericObject_SubClass_Defined_ErrorMask>.Instance.CopyIn(
                root: root,
                item: ret,
                skipProtected: false,
                doMasks: false,
                mask: out TestGenericObject_SubClass_Defined_ErrorMask errorMask,
                cmds: null);
            return ret;
        }

        public static TestGenericObject_SubClass_Defined<RBase> Create_XML(XElement root, out TestGenericObject_SubClass_Defined_ErrorMask errorMask)
        {
            var ret = new TestGenericObject_SubClass_Defined<RBase>();
            LoquiXmlTranslation<TestGenericObject_SubClass_Defined<RBase>, TestGenericObject_SubClass_Defined_ErrorMask>.Instance.CopyIn(
                root: root,
                item: ret,
                skipProtected: false,
                doMasks: true,
                mask: out errorMask,
                cmds: null);
            return ret;
        }

        public override void CopyIn_XML(XElement root, NotifyingFireParameters? cmds = null)
        {
            LoquiXmlTranslation<TestGenericObject_SubClass_Defined<RBase>, TestGenericObject_SubClass_Defined_ErrorMask>.Instance.CopyIn(
                root: root,
                item: this,
                skipProtected: true,
                doMasks: false,
                mask: out TestGenericObject_SubClass_Defined_ErrorMask errorMask,
                cmds: cmds);
        }

        public virtual void CopyIn_XML(XElement root, out TestGenericObject_SubClass_Defined_ErrorMask errorMask, NotifyingFireParameters? cmds = null)
        {
            LoquiXmlTranslation<TestGenericObject_SubClass_Defined<RBase>, TestGenericObject_SubClass_Defined_ErrorMask>.Instance.CopyIn(
                root: root,
                item: this,
                skipProtected: true,
                doMasks: true,
                mask: out errorMask,
                cmds: cmds);
        }

        public override void CopyIn_XML(XElement root, out TestGenericObject_ErrorMask errorMask, NotifyingFireParameters? cmds = null)
        {
            CopyIn_XML(root, out TestGenericObject_SubClass_Defined_ErrorMask errMask, cmds: cmds);
            errorMask = errMask;
        }

        public void Write_XML(Stream stream, out TestGenericObject_SubClass_Defined_ErrorMask errorMask)
        {
            TestGenericObject_SubClass_DefinedCommon.Write_XML(
                this,
                stream,
                out errorMask);
        }

        public void Write_XML(XmlWriter writer, out TestGenericObject_SubClass_Defined_ErrorMask errorMask, string name = null)
        {
            TestGenericObject_SubClass_DefinedCommon.Write_XML(
                writer: writer,
                name: name,
                item: this,
                doMasks: true,
                errorMask: out errorMask);
        }

        #endregion

        public TestGenericObject_SubClass_Defined<RBase> Copy(
            TestGenericObject_SubClass_Defined_CopyMask copyMask = null,
            ITestGenericObject_SubClass_DefinedGetter<RBase> def = null)
        {
            return TestGenericObject_SubClass_Defined<RBase>.Copy(
                this,
                copyMask: copyMask,
                def: def);
        }

        public static TestGenericObject_SubClass_Defined<RBase> Copy(
            ITestGenericObject_SubClass_Defined<RBase> item,
            TestGenericObject_SubClass_Defined_CopyMask copyMask = null,
            ITestGenericObject_SubClass_DefinedGetter<RBase> def = null)
        {
            TestGenericObject_SubClass_Defined<RBase> ret;
            if (item.GetType().Equals(typeof(TestGenericObject_SubClass_Defined<RBase>)))
            {
                ret = new TestGenericObject_SubClass_Defined<RBase>();
            }
            else
            {
                ret = (TestGenericObject_SubClass_Defined<RBase>)Activator.CreateInstance(item.GetType());
            }
            ret.CopyFieldsFrom(
                item,
                copyMask: copyMask,
                def: def);
            return ret;
        }

        public static CopyType Copy<CopyType>(
            CopyType item,
            TestGenericObject_SubClass_Defined_CopyMask copyMask = null,
            ITestGenericObject_SubClass_DefinedGetter<RBase> def = null)
            where CopyType : class, ITestGenericObject_SubClass_Defined<RBase>
        {
            CopyType ret;
            if (item.GetType().Equals(typeof(TestGenericObject_SubClass_Defined<RBase>)))
            {
                ret = new TestGenericObject_SubClass_Defined<RBase>() as CopyType;
            }
            else
            {
                ret = (CopyType)Activator.CreateInstance(item.GetType());
            }
            ret.CopyFieldsFrom(
                item,
                copyMask: copyMask,
                doErrorMask: false,
                errorMask: null,
                cmds: null,
                def: def);
            return ret;
        }

        public static TestGenericObject_SubClass_Defined<RBase> Copy_ToLoqui(
            ITestGenericObject_SubClass_DefinedGetter<RBase> item,
            TestGenericObject_SubClass_Defined_CopyMask copyMask = null,
            ITestGenericObject_SubClass_DefinedGetter<RBase> def = null)
        {
            var ret = new TestGenericObject_SubClass_Defined<RBase>();
            ret.CopyFieldsFrom(
                item,
                copyMask: copyMask,
                def: def);
            return ret;
        }

        protected override void SetNthObject(ushort index, object obj, NotifyingFireParameters? cmds = null)
        {
            TestGenericObject_SubClass_Defined_FieldIndex enu = (TestGenericObject_SubClass_Defined_FieldIndex)index;
            switch (enu)
            {
                default:
                    base.SetNthObject(index, obj, cmds);
                    break;
            }
        }

        public override void Clear(NotifyingUnsetParameters? cmds = null)
        {
            CallClearPartial_Internal(cmds);
            TestGenericObject_SubClass_DefinedCommon.Clear(this, cmds);
        }


        public new static TestGenericObject_SubClass_Defined<RBase> Create(IEnumerable<KeyValuePair<ushort, object>> fields)
        {
            var ret = new TestGenericObject_SubClass_Defined<RBase>();
            ILoquiObjectExt.CopyFieldsIn(ret, fields, def: null, skipProtected: false, cmds: null);
            return ret;
        }

        public static void CopyIn(IEnumerable<KeyValuePair<ushort, object>> fields, TestGenericObject_SubClass_Defined<RBase> obj)
        {
            ILoquiObjectExt.CopyFieldsIn(obj, fields, def: null, skipProtected: false, cmds: null);
        }

    }
    #endregion

    #region Interface
    public interface ITestGenericObject_SubClass_Defined<RBase> : ITestGenericObject_SubClass_DefinedGetter<RBase>, ITestGenericObject<long, RBase, ObjectToRef>, ILoquiClass<ITestGenericObject_SubClass_Defined<RBase>, ITestGenericObject_SubClass_DefinedGetter<RBase>>, ILoquiClass<TestGenericObject_SubClass_Defined<RBase>, ITestGenericObject_SubClass_DefinedGetter<RBase>>
        where RBase : ObjectToRef, ILoquiObject, ILoquiObjectGetter
    {
    }

    public interface ITestGenericObject_SubClass_DefinedGetter<RBase> : ITestGenericObjectGetter<long, RBase, ObjectToRef>
        where RBase : ObjectToRef, ILoquiObject, ILoquiObjectGetter
    {

    }

    #endregion

}

namespace Loqui.Tests.Internals
{
    #region Field Index
    public enum TestGenericObject_SubClass_Defined_FieldIndex
    {
    }
    #endregion

    #region Registration
    public class TestGenericObject_SubClass_Defined_Registration : ILoquiRegistration
    {
        public static readonly TestGenericObject_SubClass_Defined_Registration Instance = new TestGenericObject_SubClass_Defined_Registration();

        public static ProtocolDefinition ProtocolDefinition => ProtocolDefinition_LoquiTests.Definition;

        public static readonly ObjectKey ObjectKey = new ObjectKey(
            protocolKey: ProtocolDefinition_LoquiTests.ProtocolKey,
            msgID: 5,
            version: 0);

        public const string GUID = "c919b3b2-83e7-400a-8516-50048e9ab51f";

        public const ushort FieldCount = 0;

        public static readonly Type MaskType = typeof(TestGenericObject_SubClass_Defined_Mask<>);

        public static readonly Type ErrorMaskType = typeof(TestGenericObject_SubClass_Defined_ErrorMask);

        public static readonly Type ClassType = typeof(TestGenericObject_SubClass_Defined<>);

        public const string FullName = "Loqui.Tests.TestGenericObject_SubClass_Defined";

        public const string Name = "TestGenericObject_SubClass_Defined";

        public const byte GenericCount = 1;

        public static readonly Type GenericRegistrationType = typeof(TestGenericObject_SubClass_Defined_Registration<>);

        public static ushort? GetNameIndex(StringCaseAgnostic str)
        {
            switch (str.Upper)
            {
                default:
                    return null;
            }
        }

        public static bool GetNthIsEnumerable(ushort index)
        {
            TestGenericObject_SubClass_Defined_FieldIndex enu = (TestGenericObject_SubClass_Defined_FieldIndex)index;
            switch (enu)
            {
                default:
                    return TestGenericObject_Registration.GetNthIsEnumerable(index);
            }
        }

        public static bool GetNthIsLoqui(ushort index)
        {
            TestGenericObject_SubClass_Defined_FieldIndex enu = (TestGenericObject_SubClass_Defined_FieldIndex)index;
            switch (enu)
            {
                default:
                    return TestGenericObject_Registration.GetNthIsLoqui(index);
            }
        }

        public static bool GetNthIsSingleton(ushort index)
        {
            TestGenericObject_SubClass_Defined_FieldIndex enu = (TestGenericObject_SubClass_Defined_FieldIndex)index;
            switch (enu)
            {
                default:
                    return TestGenericObject_Registration.GetNthIsSingleton(index);
            }
        }

        public static string GetNthName(ushort index)
        {
            TestGenericObject_SubClass_Defined_FieldIndex enu = (TestGenericObject_SubClass_Defined_FieldIndex)index;
            switch (enu)
            {
                default:
                    return TestGenericObject_Registration.GetNthName(index);
            }
        }

        public static bool IsNthDerivative(ushort index)
        {
            TestGenericObject_SubClass_Defined_FieldIndex enu = (TestGenericObject_SubClass_Defined_FieldIndex)index;
            switch (enu)
            {
                default:
                    return TestGenericObject_Registration.IsNthDerivative(index);
            }
        }

        public static bool IsProtected(ushort index)
        {
            TestGenericObject_SubClass_Defined_FieldIndex enu = (TestGenericObject_SubClass_Defined_FieldIndex)index;
            switch (enu)
            {
                default:
                    return TestGenericObject_Registration.IsProtected(index);
            }
        }

        public static Type GetNthType(ushort index) => throw new ArgumentException("Cannot get nth type for a generic object here.  Use generic registration instead.");

        #region Interface
        ProtocolDefinition ILoquiRegistration.ProtocolDefinition => ProtocolDefinition;
        ObjectKey ILoquiRegistration.ObjectKey => ObjectKey;
        string ILoquiRegistration.GUID => GUID;
        int ILoquiRegistration.FieldCount => FieldCount;
        Type ILoquiRegistration.MaskType => MaskType;
        Type ILoquiRegistration.ErrorMaskType => ErrorMaskType;
        Type ILoquiRegistration.ClassType => ClassType;
        string ILoquiRegistration.FullName => FullName;
        string ILoquiRegistration.Name => Name;
        byte ILoquiRegistration.GenericCount => GenericCount;
        Type ILoquiRegistration.GenericRegistrationType => GenericRegistrationType;
        ushort? ILoquiRegistration.GetNameIndex(StringCaseAgnostic name) => GetNameIndex(name);
        bool ILoquiRegistration.GetNthIsEnumerable(ushort index) => GetNthIsEnumerable(index);
        bool ILoquiRegistration.GetNthIsLoqui(ushort index) => GetNthIsLoqui(index);
        bool ILoquiRegistration.GetNthIsSingleton(ushort index) => GetNthIsSingleton(index);
        string ILoquiRegistration.GetNthName(ushort index) => GetNthName(index);
        bool ILoquiRegistration.IsNthDerivative(ushort index) => IsNthDerivative(index);
        bool ILoquiRegistration.IsProtected(ushort index) => IsProtected(index);
        Type ILoquiRegistration.GetNthType(ushort index) => GetNthType(index);
        #endregion

    }

    public class TestGenericObject_SubClass_Defined_Registration<RBase> : TestGenericObject_SubClass_Defined_Registration
        where RBase : ObjectToRef, ILoquiObject, ILoquiObjectGetter
    {
        public static readonly TestGenericObject_SubClass_Defined_Registration<RBase> GenericInstance = new TestGenericObject_SubClass_Defined_Registration<RBase>();

        public new static Type GetNthType(ushort index)
        {
            TestGenericObject_SubClass_Defined_FieldIndex enu = (TestGenericObject_SubClass_Defined_FieldIndex)index;
            switch (enu)
            {
                default:
                    return TestGenericObject_Registration.GetNthType(index);
            }
        }

    }
    #endregion

    #region Extensions
    public static class TestGenericObject_SubClass_DefinedCommon
    {
        #region Copy Fields From
        public static void CopyFieldsFrom<RBase>(
            this ITestGenericObject_SubClass_Defined<RBase> item,
            ITestGenericObject_SubClass_DefinedGetter<RBase> rhs,
            ITestGenericObject_SubClass_DefinedGetter<RBase> def,
            bool doErrorMask,
            Func<TestGenericObject_SubClass_Defined_ErrorMask> errorMask,
            TestGenericObject_SubClass_Defined_CopyMask copyMask,
            NotifyingFireParameters? cmds)
            where RBase : ObjectToRef, ILoquiObject, ILoquiObjectGetter
        {
            TestGenericObjectCommon.CopyFieldsFrom<long, RBase, ObjectToRef>(
                item,
                rhs,
                def,
                doErrorMask,
                errorMask,
                copyMask,
                cmds);
        }

        #endregion

        public static void SetNthObjectHasBeenSet<RBase>(
            ushort index,
            bool on,
            ITestGenericObject_SubClass_Defined<RBase> obj,
            NotifyingFireParameters? cmds = null)
            where RBase : ObjectToRef, ILoquiObject, ILoquiObjectGetter
        {
            TestGenericObject_SubClass_Defined_FieldIndex enu = (TestGenericObject_SubClass_Defined_FieldIndex)index;
            switch (enu)
            {
                default:
                    TestGenericObjectCommon.SetNthObjectHasBeenSet<long, RBase, ObjectToRef>(index, on, obj);
                    break;
            }
        }

        public static void UnsetNthObject<RBase>(
            ushort index,
            ITestGenericObject_SubClass_Defined<RBase> obj,
            NotifyingUnsetParameters? cmds = null)
            where RBase : ObjectToRef, ILoquiObject, ILoquiObjectGetter
        {
            TestGenericObject_SubClass_Defined_FieldIndex enu = (TestGenericObject_SubClass_Defined_FieldIndex)index;
            switch (enu)
            {
                default:
                    TestGenericObjectCommon.UnsetNthObject<long, RBase, ObjectToRef>(index, obj);
                    break;
            }
        }

        public static bool GetNthObjectHasBeenSet<RBase>(
            ushort index,
            ITestGenericObject_SubClass_Defined<RBase> obj)
            where RBase : ObjectToRef, ILoquiObject, ILoquiObjectGetter
        {
            TestGenericObject_SubClass_Defined_FieldIndex enu = (TestGenericObject_SubClass_Defined_FieldIndex)index;
            switch (enu)
            {
                default:
                    return TestGenericObjectCommon.GetNthObjectHasBeenSet<long, RBase, ObjectToRef>(index, obj);
            }
        }

        public static object GetNthObject<RBase>(
            ushort index,
            ITestGenericObject_SubClass_DefinedGetter<RBase> obj)
            where RBase : ObjectToRef, ILoquiObject, ILoquiObjectGetter
        {
            TestGenericObject_SubClass_Defined_FieldIndex enu = (TestGenericObject_SubClass_Defined_FieldIndex)index;
            switch (enu)
            {
                default:
                    return TestGenericObjectCommon.GetNthObject<long, RBase, ObjectToRef>(index, obj);
            }
        }

        public static void Clear<RBase>(
            ITestGenericObject_SubClass_Defined<RBase> item,
            NotifyingUnsetParameters? cmds = null)
            where RBase : ObjectToRef, ILoquiObject, ILoquiObjectGetter
        {
        }

        public static TestGenericObject_SubClass_Defined_Mask<bool?> GetEqualsMask<RBase>(
            this ITestGenericObject_SubClass_DefinedGetter<RBase> item,
            ITestGenericObject_SubClass_DefinedGetter<RBase> rhs)
            where RBase : ObjectToRef, ILoquiObject, ILoquiObjectGetter
        {
            var ret = new TestGenericObject_SubClass_Defined_Mask<bool?>();
            FillEqualsMask(item, rhs, ret);
            return ret;
        }

        public static void FillEqualsMask<RBase>(
            this ITestGenericObject_SubClass_DefinedGetter<RBase> item,
            ITestGenericObject_SubClass_DefinedGetter<RBase> rhs,
            TestGenericObject_SubClass_Defined_Mask<bool?> ret)
            where RBase : ObjectToRef, ILoquiObject, ILoquiObjectGetter
        {
            TestGenericObjectCommon.FillEqualsMask(item, rhs, ret);
        }

        #region XML Translation
        public static void Write_XML<RBase>(
            ITestGenericObject_SubClass_DefinedGetter<RBase> item,
            Stream stream,
            out TestGenericObject_SubClass_Defined_ErrorMask errorMask)
            where RBase : ObjectToRef, ILoquiObject, ILoquiObjectGetter
        {
            using (var writer = new XmlTextWriter(stream, Encoding.ASCII))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 3;
                Write_XML(
                    writer: writer,
                    name: null,
                    item: item,
                    doMasks: true,
                    errorMask: out errorMask);
            }
        }

        public static void Write_XML<RBase>(
            ITestGenericObject_SubClass_DefinedGetter<RBase> item,
            XmlWriter writer,
            out TestGenericObject_SubClass_Defined_ErrorMask errorMask,
            string name = null)
            where RBase : ObjectToRef, ILoquiObject, ILoquiObjectGetter
        {
            Write_XML(
                writer: writer,
                name: name,
                item: item,
                doMasks: true,
                errorMask: out errorMask);
        }

        public static void Write_XML<RBase>(
            ITestGenericObject_SubClass_DefinedGetter<RBase> item,
            XmlWriter writer,
            string name)
            where RBase : ObjectToRef, ILoquiObject, ILoquiObjectGetter
        {
            Write_XML(
                writer: writer,
                name: name,
                item: item,
                doMasks: false,
                errorMask: out TestGenericObject_SubClass_Defined_ErrorMask errorMask);
        }

        public static void Write_XML<RBase>(
            ITestGenericObject_SubClass_DefinedGetter<RBase> item,
            XmlWriter writer)
            where RBase : ObjectToRef, ILoquiObject, ILoquiObjectGetter
        {
            Write_XML(
                writer: writer,
                name: null,
                item: item,
                doMasks: false,
                errorMask: out TestGenericObject_SubClass_Defined_ErrorMask errorMask);
        }

        public static void Write_XML<RBase>(
            XmlWriter writer,
            string name,
            ITestGenericObject_SubClass_DefinedGetter<RBase> item,
            bool doMasks,
            out TestGenericObject_SubClass_Defined_ErrorMask errorMask)
            where RBase : ObjectToRef, ILoquiObject, ILoquiObjectGetter
        {
            TestGenericObject_SubClass_Defined_ErrorMask errMaskRet = null;
            Write_XML_Internal(
                writer: writer,
                name: name,
                item: item,
                doMasks: doMasks,
                errorMask: doMasks ? () => errMaskRet ?? (errMaskRet = new TestGenericObject_SubClass_Defined_ErrorMask()) : default(Func<TestGenericObject_SubClass_Defined_ErrorMask>));
            errorMask = errMaskRet;
        }

        private static void Write_XML_Internal<RBase>(
            XmlWriter writer,
            string name,
            ITestGenericObject_SubClass_DefinedGetter<RBase> item,
            bool doMasks,
            Func<TestGenericObject_SubClass_Defined_ErrorMask> errorMask)
            where RBase : ObjectToRef, ILoquiObject, ILoquiObjectGetter
        {
            try
            {
                using (new ElementWrapper(writer, nameof(TestGenericObject_SubClass_Defined<RBase>)))
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        writer.WriteAttributeString("name", name);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!doMasks) throw;
                errorMask().Overall = ex;
            }
        }
        #endregion

    }
    #endregion

    #region Modules

    #region Mask
    public class TestGenericObject_SubClass_Defined_Mask<T> : TestGenericObject_Mask<T>, IMask<T>
    {

        public bool AllEqual(T t)
        {
            return true;
        }
    }

    public class TestGenericObject_SubClass_Defined_ErrorMask : TestGenericObject_ErrorMask
    {

        public override void SetNthException(ushort index, Exception ex)
        {
            TestGenericObject_SubClass_Defined_FieldIndex enu = (TestGenericObject_SubClass_Defined_FieldIndex)index;
            switch (enu)
            {
                default:
                    base.SetNthException(index, ex);
                    break;
            }
        }

        public override void SetNthMask(ushort index, object obj)
        {
            TestGenericObject_SubClass_Defined_FieldIndex enu = (TestGenericObject_SubClass_Defined_FieldIndex)index;
            switch (enu)
            {
                default:
                    base.SetNthMask(index, obj);
                    break;
            }
        }

        public override string ToString()
        {
            var fg = new FileGeneration();
            ToString(fg);
            return fg.ToString();
        }

        public void ToString(FileGeneration fg)
        {
            fg.AppendLine("TestGenericObject_SubClass_Defined_ErrorMask =>");
            fg.AppendLine("[");
            using (new DepthWrapper(fg))
            {
            }
            fg.AppendLine("]");
        }
    }
    public class TestGenericObject_SubClass_Defined_CopyMask : TestGenericObject_CopyMask
    {

    }
    #endregion


    #endregion

}
