﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Internal
{
    public class TranslationCrystal
    {
        public (bool On, TranslationCrystal SubCrystal)[] Crystal;

        public bool GetShouldTranslate(ushort index)
        {
            if (Crystal.Length <= index) return true;
            return Crystal[index].On;
        }

        public TranslationCrystal GetSubCrystal(ushort index)
        {
            if (Crystal.Length <= index) return null;
            return Crystal[index].SubCrystal;
        }
    }
}
