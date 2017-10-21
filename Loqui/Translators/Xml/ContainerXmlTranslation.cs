﻿using Loqui.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noggog;
using System.Xml;
using System.Xml.Linq;
using Noggog.Notifying;
using Noggog.Xml;

namespace Loqui.Xml
{
    public abstract class ContainerXmlTranslation<T, M> : IXmlTranslation<IEnumerable<T>, MaskItem<Exception, IEnumerable<M>>>
    {
        public abstract string ElementName { get; }

        public TryGet<IEnumerable<T>> Parse(XElement root, bool doMasks, out MaskItem<Exception, IEnumerable<M>> maskObj)
        {
            var transl = XmlTranslator<T, M>.Translator;
            if (transl.Item.Failed)
            {
                throw new ArgumentException($"No XML Translator available for {typeof(T)}. {transl.Item.Reason}");
            }
            return Parse(
                root,
                doMasks,
                out maskObj,
                transl: (XElement r, bool internalDoMasks, out M obj) => transl.Item.Value.Parse(root: r, doMasks: internalDoMasks, maskObj: out obj));
        }

        public TryGet<IEnumerable<T>> Parse(
            XElement root,
            bool doMasks,
            out MaskItem<Exception, IEnumerable<M>> maskObj,
            XmlSubParseDelegate<T, M> transl)
        {
            try
            {
                List<M> maskList = null;
                var ret = new List<T>();
                foreach (var listElem in root.Elements())
                {
                    var get = transl(listElem, doMasks, out var subMaskObj);
                    if (get.Succeeded)
                    {
                        ret.Add(get.Value);
                    }
                    if (subMaskObj != null)
                    {
                        if (!doMasks)
                        { // This shouldn't actually throw, as subparse is expected to throw if doMasks is off
                            throw new ArgumentException("Error parsing list.  Could not parse subitem.");
                        }
                        if (maskList == null)
                        {
                            maskList = new List<M>();
                        }
                        maskList.Add(subMaskObj);
                    }
                }
                maskObj = maskList == null ? null : new MaskItem<Exception, IEnumerable<M>>(null, maskList);
                return TryGet<IEnumerable<T>>.Succeed(ret);
            }
            catch (Exception ex)
            when (doMasks)
            {
                maskObj = new MaskItem<Exception, IEnumerable<M>>(ex, null);
                return TryGet<IEnumerable<T>>.Failure;
            }
        }

        public abstract TryGet<T> ParseSingleItem(XElement root, XmlSubParseDelegate<T, M> transl, bool doMasks, out M maskObj);

        public void Write(
            XmlWriter writer,
            string name,
            IEnumerable<T> item,
            bool doMasks,
            out MaskItem<Exception, IEnumerable<M>> maskObj)
        {
            try
            {
                var transl = XmlTranslator<T, M>.Translator;
                if (transl.Item.Failed)
                {
                    throw new ArgumentException($"No XML Translator available for {typeof(T)}. {transl.Item.Reason}");
                }
                this.Write(
                    writer: writer,
                    name: name,
                    item: item,
                    doMasks: doMasks,
                    maskObj: out maskObj,
                    transl: (T item1, bool internalDoMasks, out M obj) => transl.Item.Value.Write(writer: writer, name: "Item", item: item1, doMasks: internalDoMasks, maskObj: out obj));
            }
            catch (Exception ex)
            when (doMasks)
            {
                maskObj = new MaskItem<Exception, IEnumerable<M>>(ex, null);
            }
        }

        public void Write(
            XmlWriter writer,
            string name,
            IEnumerable<T> item,
            bool doMasks,
            out MaskItem<Exception, IEnumerable<M>> maskObj,
            XmlSubWriteDelegate<T, M> transl)
        {
            try
            {
                List<M> maskList = null;
                using (new ElementWrapper(writer, name))
                {
                    foreach (var listObj in item)
                    {
                        WriteSingleItem(writer, transl, listObj, doMasks, out M subMaskObj);
                        if (subMaskObj != null)
                        {
                            if (maskList == null)
                            {
                                maskList = new List<M>();
                            }
                            maskList.Add(subMaskObj);
                        }
                    }
                }
                if (maskList != null)
                {
                    maskObj = new MaskItem<Exception, IEnumerable<M>>(null, maskList);
                }
                else
                {
                    maskObj = null;
                }
            }
            catch (Exception ex)
            when (doMasks)
            {
                maskObj = new MaskItem<Exception, IEnumerable<M>>(ex, null);
            }
        }

        public abstract void WriteSingleItem<ErrMask>(XmlWriter writer, XmlSubWriteDelegate<T, ErrMask> transl, T item, bool doMasks, out ErrMask maskObj);
    }
}
