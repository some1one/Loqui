/*
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
 * Autogenerated by Noggolloquy.  Do not manually change.
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Noggolloquy;
using Noggog;
using Noggog.Notifying;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Noggog.Xml;
using Noggolloquy.Xml;

namespace Noggolloquy.Tests
{
    #region Class
    public partial class TestGenericObject_SubClass_Defined : TestGenericObject<long, ObjectToRef>, ITestGenericObject_SubClass_Defined, INoggolloquyObjectSetter, IEquatable<TestGenericObject_SubClass_Defined>
    {
        INoggolloquyRegistration INoggolloquyObject.Registration => TestGenericObject_SubClass_Defined_Registration.Instance;
        public new static TestGenericObject_SubClass_Defined_Registration Registration => TestGenericObject_SubClass_Defined_Registration.Instance;

        public TestGenericObject_SubClass_Defined()
        {
            CustomCtor();
        }
        partial void CustomCtor();

        #region Noggolloquy Getter Interface

        protected override object GetNthObject(ushort index) => TestGenericObject_SubClass_DefinedCommon.GetNthObject(index, this);

        protected override bool GetNthObjectHasBeenSet(ushort index) => TestGenericObject_SubClass_DefinedCommon.GetNthObjectHasBeenSet(index, this);

        protected override void UnsetNthObject(ushort index, NotifyingUnsetParameters? cmds) => TestGenericObject_SubClass_DefinedCommon.UnsetNthObject(index, this, cmds);

        #endregion

        #region Noggolloquy Interface
        protected override void SetNthObjectHasBeenSet(ushort index, bool on)
        {
            TestGenericObject_SubClass_DefinedCommon.SetNthObjectHasBeenSet(index, on, this);
        }

        public void CopyFieldsFrom(
            ITestGenericObject_SubClass_DefinedGetter rhs,
            TestGenericObject_SubClass_Defined_CopyMask copyMask = null,
            ITestGenericObject_SubClass_DefinedGetter def = null,
            NotifyingFireParameters? cmds = null)
        {
            TestGenericObject_SubClass_DefinedCommon.CopyFieldsFrom(
                item: this,
                rhs: rhs,
                def: def,
                doErrorMask: false,
                errorMask: null,
                copyMask: copyMask,
                cmds: cmds);
        }

        public void CopyFieldsFrom(
            ITestGenericObject_SubClass_DefinedGetter rhs,
            out TestGenericObject_SubClass_Defined_ErrorMask errorMask,
            TestGenericObject_SubClass_Defined_CopyMask copyMask = null,
            ITestGenericObject_SubClass_DefinedGetter def = null,
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
            TestGenericObject_SubClass_DefinedCommon.CopyFieldsFrom(
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
            return INoggolloquyObjectExt.PrintPretty(this);
        }
        #endregion

        #region Equals and Hash
        public override bool Equals(object obj)
        {
            if (!(obj is TestGenericObject_SubClass_Defined rhs)) return false;
            return Equals(rhs);
        }

        public bool Equals(TestGenericObject_SubClass_Defined rhs)
        {
            return base.Equals(rhs);
        }

        public override int GetHashCode()
        {
            return 
            base.GetHashCode()
            ;
        }

        #endregion

        #region XML Translation
        public new static TestGenericObject_SubClass_Defined Create_XML(XElement root)
        {
            var ret = new TestGenericObject_SubClass_Defined();
            NoggXmlTranslation<TestGenericObject_SubClass_Defined, TestGenericObject_SubClass_Defined_ErrorMask>.Instance.CopyIn(
                root: root,
                item: ret,
                skipReadonly: false,
                doMasks: false,
                mask: out TestGenericObject_SubClass_Defined_ErrorMask errorMask,
                cmds: null);
            return ret;
        }

        public static TestGenericObject_SubClass_Defined Create_XML(XElement root, out TestGenericObject_SubClass_Defined_ErrorMask errorMask)
        {
            var ret = new TestGenericObject_SubClass_Defined();
            NoggXmlTranslation<TestGenericObject_SubClass_Defined, TestGenericObject_SubClass_Defined_ErrorMask>.Instance.CopyIn(
                root: root,
                item: ret,
                skipReadonly: false,
                doMasks: true,
                mask: out errorMask,
                cmds: null);
            return ret;
        }

        public override void CopyIn_XML(XElement root, NotifyingFireParameters? cmds = null)
        {
            NoggXmlTranslation<TestGenericObject_SubClass_Defined, TestGenericObject_SubClass_Defined_ErrorMask>.Instance.CopyIn(
                root: root,
                item: this,
                skipReadonly: true,
                doMasks: false,
                mask: out TestGenericObject_SubClass_Defined_ErrorMask errorMask,
                cmds: cmds);
        }

        public virtual void CopyIn_XML(XElement root, out TestGenericObject_SubClass_Defined_ErrorMask errorMask, NotifyingFireParameters? cmds = null)
        {
            NoggXmlTranslation<TestGenericObject_SubClass_Defined, TestGenericObject_SubClass_Defined_ErrorMask>.Instance.CopyIn(
                root: root,
                item: this,
                skipReadonly: true,
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
            using (var writer = new XmlTextWriter(stream, Encoding.ASCII))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 3;
                Write_XML(writer, out errorMask);
            }
        }

        public void Write_XML(XmlWriter writer, out TestGenericObject_SubClass_Defined_ErrorMask errorMask, string name = null)
        {
            NoggXmlTranslation<TestGenericObject_SubClass_Defined, TestGenericObject_SubClass_Defined_ErrorMask>.Instance.Write(
                writer: writer,
                name: name,
                item: this,
                doMasks: true,
                mask: out errorMask);
        }

        #endregion
        public TestGenericObject_SubClass_Defined Copy(
            TestGenericObject_SubClass_Defined_CopyMask copyMask = null,
            ITestGenericObject_SubClass_DefinedGetter def = null)
        {
            return TestGenericObject_SubClass_Defined.Copy(
                this,
                copyMask: copyMask,
                def: def);
        }

        public static TestGenericObject_SubClass_Defined Copy(
            ITestGenericObject_SubClass_Defined item,
            TestGenericObject_SubClass_Defined_CopyMask copyMask = null,
            ITestGenericObject_SubClass_DefinedGetter def = null)
        {
            TestGenericObject_SubClass_Defined ret;
            if (item.GetType().Equals(typeof(TestGenericObject_SubClass_Defined)))
            {
                ret = new TestGenericObject_SubClass_Defined();
            }
            else
            {
                ret = (TestGenericObject_SubClass_Defined)Activator.CreateInstance(item.GetType());
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
            ITestGenericObject_SubClass_DefinedGetter def = null)
            where CopyType : class, ITestGenericObject_SubClass_Defined
        {
            CopyType ret;
            if (item.GetType().Equals(typeof(TestGenericObject_SubClass_Defined)))
            {
                ret = new TestGenericObject_SubClass_Defined() as CopyType;
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

        public static TestGenericObject_SubClass_Defined Copy_ToNoggolloquy(
            ITestGenericObject_SubClass_DefinedGetter item,
            TestGenericObject_SubClass_Defined_CopyMask copyMask = null,
            ITestGenericObject_SubClass_DefinedGetter def = null)
        {
            var ret = new TestGenericObject_SubClass_Defined();
            ret.CopyFieldsFrom(
                item,
                copyMask: copyMask,
                def: def);
            return ret;
        }

        protected override void SetNthObject(ushort index, object obj, NotifyingFireParameters? cmds = null)
        {
            switch (index)
            {
                default:
                    base.SetNthObject(index, obj, cmds);
                    break;
            }
        }

        public override void Clear(NotifyingUnsetParameters? cmds = null)
        {
            base.Clear(cmds);
        }

        public new static TestGenericObject_SubClass_Defined Create(IEnumerable<KeyValuePair<ushort, object>> fields)
        {
            var ret = new TestGenericObject_SubClass_Defined();
            INoggolloquyObjectExt.CopyFieldsIn(ret, fields, def: null, skipReadonly: false, cmds: null);
            return ret;
        }

        public static void CopyIn(IEnumerable<KeyValuePair<ushort, object>> fields, TestGenericObject_SubClass_Defined obj)
        {
            INoggolloquyObjectExt.CopyFieldsIn(obj, fields, def: null, skipReadonly: false, cmds: null);
        }

    }
    #endregion

    #region Interface
    public interface ITestGenericObject_SubClass_Defined : ITestGenericObject_SubClass_DefinedGetter, ITestGenericObject<long, ObjectToRef>, INoggolloquyClass<ITestGenericObject_SubClass_Defined, ITestGenericObject_SubClass_DefinedGetter>, INoggolloquyClass<TestGenericObject_SubClass_Defined, ITestGenericObject_SubClass_DefinedGetter>
    {
    }

    public interface ITestGenericObject_SubClass_DefinedGetter : ITestGenericObjectGetter<long, ObjectToRef>
    {

    }

    #endregion

    #region Registration
    public class TestGenericObject_SubClass_Defined_Registration : INoggolloquyRegistration
    {
        public static readonly TestGenericObject_SubClass_Defined_Registration Instance = new TestGenericObject_SubClass_Defined_Registration();

        public static ProtocolDefinition ProtocolDefinition => ProtocolDefinition_NoggolloquyTests.Definition;

        public static readonly ObjectKey ObjectKey = new ObjectKey(
            protocolKey: ProtocolDefinition_NoggolloquyTests.ProtocolKey,
            msgID: 5,
            version: 0);

        public const string GUID = "c919b3b2-83e7-400a-8516-50048e9ab51f";

        public const ushort FieldCount = 0;

        public static readonly Type MaskType = typeof(TestGenericObject_SubClass_Defined_Mask<>);

        public static readonly Type ErrorMaskType = typeof(TestGenericObject_SubClass_Defined_ErrorMask);

        public static readonly Type ClassType = typeof(TestGenericObject_SubClass_Defined);

        public const string FullName = "Noggolloquy.Tests.TestGenericObject_SubClass_Defined";

        public const string Name = "TestGenericObject_SubClass_Defined";

        public const byte GenericCount = 0;

        public static readonly Type GenericRegistrationType = null;

        public static ushort? GetNameIndex(StringCaseAgnostic str)
        {
            switch (str.Upper)
            {
                default:
                    throw new ArgumentException($"Queried unknown field: {str}");
            }
        }

        public static bool GetNthIsEnumerable(ushort index)
        {
            switch (index)
            {
                default:
                    return TestGenericObject_Registration.GetNthIsEnumerable(index);
            }
        }

        public static bool GetNthIsNoggolloquy(ushort index)
        {
            switch (index)
            {
                default:
                    return TestGenericObject_Registration.GetNthIsNoggolloquy(index);
            }
        }

        public static bool GetNthIsSingleton(ushort index)
        {
            switch (index)
            {
                default:
                    return TestGenericObject_Registration.GetNthIsSingleton(index);
            }
        }

        public static string GetNthName(ushort index)
        {
            switch (index)
            {
                default:
                    return TestGenericObject_Registration.GetNthName(index);
            }
        }

        public static bool IsNthDerivative(ushort index)
        {
            switch (index)
            {
                default:
                    return TestGenericObject_Registration.IsNthDerivative(index);
            }
        }

        public static bool IsReadOnly(ushort index)
        {
            switch (index)
            {
                default:
                    return TestGenericObject_Registration.IsReadOnly(index);
            }
        }

        public static Type GetNthType(ushort index)
        {
            switch (index)
            {
                default:
                    return TestGenericObject_Registration.GetNthType(index);
            }
        }

        #region Interface
        ProtocolDefinition INoggolloquyRegistration.ProtocolDefinition => ProtocolDefinition;
        ObjectKey INoggolloquyRegistration.ObjectKey => ObjectKey;
        string INoggolloquyRegistration.GUID => GUID;
        int INoggolloquyRegistration.FieldCount => FieldCount;
        Type INoggolloquyRegistration.MaskType => MaskType;
        Type INoggolloquyRegistration.ErrorMaskType => ErrorMaskType;
        Type INoggolloquyRegistration.ClassType => ClassType;
        string INoggolloquyRegistration.FullName => FullName;
        string INoggolloquyRegistration.Name => Name;
        byte INoggolloquyRegistration.GenericCount => GenericCount;
        Type INoggolloquyRegistration.GenericRegistrationType => GenericRegistrationType;
        ushort? INoggolloquyRegistration.GetNameIndex(StringCaseAgnostic name) => GetNameIndex(name);
        bool INoggolloquyRegistration.GetNthIsEnumerable(ushort index) => GetNthIsEnumerable(index);
        bool INoggolloquyRegistration.GetNthIsNoggolloquy(ushort index) => GetNthIsNoggolloquy(index);
        bool INoggolloquyRegistration.GetNthIsSingleton(ushort index) => GetNthIsSingleton(index);
        string INoggolloquyRegistration.GetNthName(ushort index) => GetNthName(index);
        bool INoggolloquyRegistration.IsNthDerivative(ushort index) => IsNthDerivative(index);
        bool INoggolloquyRegistration.IsReadOnly(ushort index) => IsReadOnly(index);
        Type INoggolloquyRegistration.GetNthType(ushort index) => GetNthType(index);
        #endregion
    }
    #endregion
    #region Extensions
    public static class TestGenericObject_SubClass_DefinedCommon
    {
        #region Copy Fields From
        public static void CopyFieldsFrom(
            this ITestGenericObject_SubClass_Defined item,
            ITestGenericObject_SubClass_DefinedGetter rhs,
            ITestGenericObject_SubClass_DefinedGetter def,
            bool doErrorMask,
            Func<TestGenericObject_SubClass_Defined_ErrorMask> errorMask,
            TestGenericObject_SubClass_Defined_CopyMask copyMask,
            NotifyingFireParameters? cmds)
        {
            TestGenericObjectCommon.CopyFieldsFrom<long, ObjectToRef>(
                item,
                rhs,
                def,
                doErrorMask,
                errorMask,
                copyMask,
                cmds);
        }

        #endregion

        public static void SetNthObjectHasBeenSet(
            ushort index,
            bool on,
            ITestGenericObject_SubClass_Defined obj,
            NotifyingFireParameters? cmds = null)
        {
            switch (index)
            {
                default:
                    TestGenericObjectCommon.SetNthObjectHasBeenSet<long, ObjectToRef>(index, on, obj);
                    break;
            }
        }

        public static void UnsetNthObject(
            ushort index,
            ITestGenericObject_SubClass_Defined obj,
            NotifyingUnsetParameters? cmds = null)
        {
            switch (index)
            {
                default:
                    TestGenericObjectCommon.UnsetNthObject<long, ObjectToRef>(index, obj);
                    break;
            }
        }

        public static bool GetNthObjectHasBeenSet(
            ushort index,
            ITestGenericObject_SubClass_Defined obj)
        {
            switch (index)
            {
                default:
                    return TestGenericObjectCommon.GetNthObjectHasBeenSet<long, ObjectToRef>(index, obj);
            }
        }

        public static object GetNthObject(
            ushort index,
            ITestGenericObject_SubClass_DefinedGetter obj)
        {
            switch (index)
            {
                default:
                    return TestGenericObjectCommon.GetNthObject<long, ObjectToRef>(index, obj);
            }
        }

    }
    #endregion

    #region Modules

    #region Mask
    public class TestGenericObject_SubClass_Defined_Mask<T>  : TestGenericObject_Mask<T>
    {
    }

    public class TestGenericObject_SubClass_Defined_ErrorMask : TestGenericObject_ErrorMask
    {

        public override void SetNthException(ushort index, Exception ex)
        {
            switch (index)
            {
                default:
                    base.SetNthException(index, ex);
                    break;
            }
        }

        public override void SetNthMask(ushort index, object obj)
        {
            switch (index)
            {
                default:
                    base.SetNthMask(index, obj);
                    break;
            }
        }
    }
    public class TestGenericObject_SubClass_Defined_CopyMask : TestGenericObject_CopyMask
    {

    }
    #endregion

    #endregion


}
