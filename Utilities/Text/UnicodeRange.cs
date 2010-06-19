using System.Collections.Generic;
using Managed.Adb.Utilities.Collections;

namespace Managed.Adb.Utilities.Text {

    /// <summary>
    /// Utility class providing a number of singleton instances of
    /// Range&lt;char&gt; to indicate the various ranges of unicode characters,
    /// as documented at http://msdn.microsoft.com/en-us/library/20bw873z.aspx.
    /// Note that this does not indicate the Unicode category of a character,
    /// merely which range it's in.
    /// TODO: Work out how to include names. Can't derive from Range[char].
    /// </summary>
    public static class UnicodeRange {

        static readonly List<Range<char>> allRanges = new List<Range<char>>();

        private static Range<char> CreateRange(char from,char to)
        {
            // TODO: Check for overlaps
            Range<char> ret = new Range<char>(from, to);
            allRanges.Add(ret);
            return ret;
        }

        static readonly Range<char> basicLatin = CreateRange('\u0000', '\u007f');
        static readonly Range<char> latin1Supplement = CreateRange('\u0080', '\u00ff');
        static readonly Range<char> latinExtendedA = CreateRange('\u0100', '\u017f');
        static readonly Range<char> latinExtendedB = CreateRange('\u0180', '\u024f');
        static readonly Range<char> ipaExtensions = CreateRange('\u0250', '\u02af');
        static readonly Range<char> spacingModifierLetters = CreateRange('\u02b0', '\u02ff');
        static readonly Range<char> combiningDiacriticalMarks = CreateRange('\u0300', '\u036f');
        static readonly Range<char> greekAndCoptic = CreateRange('\u0370', '\u03ff');
        static readonly Range<char> cyrillic = CreateRange('\u0400', '\u04ff');
        static readonly Range<char> cyrillicSupplement = CreateRange('\u0500', '\u052f');
        static readonly Range<char> armenian = CreateRange('\u0530', '\u058f');
        static readonly Range<char> hebrew = CreateRange('\u0590', '\u05FF');
        static readonly Range<char> arabic = CreateRange('\u0600', '\u06ff');
        static readonly Range<char> syriac = CreateRange('\u0700', '\u074f');
        static readonly Range<char> thaana = CreateRange('\u0780', '\u07bf');
        static readonly Range<char> devangari = CreateRange('\u0900', '\u097f');
        static readonly Range<char> bengali = CreateRange('\u0980', '\u09ff');
        static readonly Range<char> gurmukhi = CreateRange('\u0a00', '\u0a7f');
        static readonly Range<char> gujarati = CreateRange('\u0a80', '\u0aff');
        static readonly Range<char> oriya = CreateRange('\u0b00', '\u0b7f');
        static readonly Range<char> tamil = CreateRange('\u0b80', '\u0bff');
        static readonly Range<char> telugu = CreateRange('\u0c00', '\u0c7f');
        static readonly Range<char> kannada = CreateRange('\u0c80', '\u0cff');
        static readonly Range<char> malayalam = CreateRange('\u0d00', '\u0d7f');
        static readonly Range<char> sinhala = CreateRange('\u0d80', '\u0dff');
        static readonly Range<char> thai = CreateRange('\u0e00', '\u0e7f');
        static readonly Range<char> lao = CreateRange('\u0e80', '\u0eff');
        static readonly Range<char> tibetan = CreateRange('\u0f00', '\u0fff');
        static readonly Range<char> myanmar = CreateRange('\u1000', '\u109f');
        static readonly Range<char> georgian = CreateRange('\u10a0', '\u10ff');
        static readonly Range<char> hangulJamo = CreateRange('\u1100', '\u11ff');
        static readonly Range<char> ethiopic = CreateRange('\u1200', '\u137f');
        static readonly Range<char> cherokee = CreateRange('\u13a0', '\u13ff');
        static readonly Range<char> unifiedCanadianAboriginalSyllabics = CreateRange('\u1400', '\u167f');
        static readonly Range<char> ogham = CreateRange('\u1680', '\u169f');
        static readonly Range<char> runic = CreateRange('\u16a0', '\u16ff');
        static readonly Range<char> tagalog = CreateRange('\u1700', '\u171f');
        static readonly Range<char> hanunoo = CreateRange('\u1720', '\u173f');
        static readonly Range<char> buhid = CreateRange('\u1740', '\u175f');
        static readonly Range<char> tagbanwa = CreateRange('\u1760', '\u177f');
        static readonly Range<char> khmer = CreateRange('\u1780', '\u17ff');
        static readonly Range<char> mongolian = CreateRange('\u1800', '\u18af');
        static readonly Range<char> limbu = CreateRange('\u1900', '\u194f');
        static readonly Range<char> taiLe = CreateRange('\u1950', '\u197f');
        static readonly Range<char> khmerSymbols = CreateRange('\u19e0', '\u19ff');
        static readonly Range<char> phoneticExtensions = CreateRange('\u1d00', '\u1d7f');
        static readonly Range<char> latinExtendedAdditional = CreateRange('\u1e00', '\u1eff');
        static readonly Range<char> greekExtended = CreateRange('\u1f00', '\u1fff');
        static readonly Range<char> generalPunctuation = CreateRange('\u2000', '\u206f');
        static readonly Range<char> superscriptsandSubscripts = CreateRange('\u2070', '\u209f');
        static readonly Range<char> currencySymbols = CreateRange('\u20a0', '\u20cf');
        static readonly Range<char> combiningDiacriticalMarksforSymbols = CreateRange('\u20d0', '\u20ff');
        static readonly Range<char> letterlikeSymbols = CreateRange('\u2100', '\u214f');
        static readonly Range<char> numberForms = CreateRange('\u2150', '\u218f');
        static readonly Range<char> arrows = CreateRange('\u2190', '\u21ff');
        static readonly Range<char> mathematicalOperators = CreateRange('\u2200', '\u22ff');
        static readonly Range<char> miscellaneousTechnical = CreateRange('\u2300', '\u23ff');
        static readonly Range<char> controlPictures = CreateRange('\u2400', '\u243f');
        static readonly Range<char> opticalCharacterRecognition = CreateRange('\u2440', '\u245f');
        static readonly Range<char> enclosedAlphanumerics = CreateRange('\u2460', '\u24ff');
        static readonly Range<char> boxDrawing = CreateRange('\u2500', '\u257f');
        static readonly Range<char> blockElements = CreateRange('\u2580', '\u259f');
        static readonly Range<char> geometricShapes = CreateRange('\u25a0', '\u25ff');
        static readonly Range<char> miscellaneousSymbols = CreateRange('\u2600', '\u26ff');
        static readonly Range<char> dingbats = CreateRange('\u2700', '\u27bf');
        static readonly Range<char> miscellaneousMathematicalSymbolsA = CreateRange('\u27c0', '\u27ef');
        static readonly Range<char> supplementalArrowsA = CreateRange('\u27f0', '\u27ff');
        static readonly Range<char> braillePatterns = CreateRange('\u2800', '\u28ff');
        static readonly Range<char> supplementalArrowsB = CreateRange('\u2900', '\u297f');
        static readonly Range<char> miscellaneousMathematicalSymbolsB = CreateRange('\u2980', '\u29ff');
        static readonly Range<char> supplementalMathematicalOperators = CreateRange('\u2a00', '\u2aff');
        static readonly Range<char> miscellaneousSymbolsandArrows = CreateRange('\u2b00', '\u2bff');
        static readonly Range<char> cjkRadicalsSupplement = CreateRange('\u2e80', '\u2eff');
        static readonly Range<char> kangxiRadicals = CreateRange('\u2f00', '\u2fdf');
        static readonly Range<char> ideographicDescriptionCharacters = CreateRange('\u2ff0', '\u2fff');
        static readonly Range<char> cjkSymbolsandPunctuation = CreateRange('\u3000', '\u303f');
        static readonly Range<char> hiragana = CreateRange('\u3040', '\u309f');
        static readonly Range<char> katakana = CreateRange('\u30a0', '\u30ff');
        static readonly Range<char> bopomofo = CreateRange('\u3100', '\u312f');
        static readonly Range<char> hangulCompatibilityJamo = CreateRange('\u3130', '\u318f');
        static readonly Range<char> kanbun = CreateRange('\u3190', '\u319f');
        static readonly Range<char> bopomofoExtended = CreateRange('\u31a0', '\u31bf');
        static readonly Range<char> katakanaPhoneticExtensions = CreateRange('\u31f0', '\u31ff');
        static readonly Range<char> enclosedCjkLettersandMonths = CreateRange('\u3200', '\u32ff');
        static readonly Range<char> cjkCompatibility = CreateRange('\u3300', '\u33ff');
        static readonly Range<char> cjkUnifiedIdeographsExtensionA = CreateRange('\u3400', '\u4dbf');
        static readonly Range<char> yijingHexagramSymbols = CreateRange('\u4dc0', '\u4dff');
        static readonly Range<char> cjkUnifiedIdeographs = CreateRange('\u4e00', '\u9fff');
        static readonly Range<char> yiSyllables = CreateRange('\ua000', '\ua48f');
        static readonly Range<char> yiRadicals = CreateRange('\ua490', '\ua4cf');
        static readonly Range<char> hangulSyllables = CreateRange('\uac00', '\ud7af');
        static readonly Range<char> highSurrogates = CreateRange('\ud800', '\udb7f');
        static readonly Range<char> highPrivateUseSurrogates = CreateRange('\udb80', '\udbff');
        static readonly Range<char> lowSurrogates = CreateRange('\udc00', '\udfff');
        static readonly Range<char> privateUse = CreateRange('\ue000', '\uf8ff');
        static readonly Range<char> privateUseArea = CreateRange('\uf900', '\ufaff');
        static readonly Range<char> cjkCompatibilityIdeographs = CreateRange('\ufb00', '\ufb4f');
        static readonly Range<char> alphabeticPresentationForms = CreateRange('\ufb50', '\ufdff');
        static readonly Range<char> arabicPresentationFormsA = CreateRange('\ufe00', '\ufe0f');
        static readonly Range<char> variationSelectors = CreateRange('\ufe20', '\ufe2f');
        static readonly Range<char> combiningHalfMarks = CreateRange('\ufe30', '\ufe4f');
        static readonly Range<char> cjkCompatibilityForms = CreateRange('\ufe50', '\ufe6f');
        static readonly Range<char> smallFormVariants = CreateRange('\ufe70', '\ufeff');
        static readonly Range<char> arabicPresentationFormsB = CreateRange('\uff00', '\uffef');
        static readonly Range<char> halfwidthandFullwidthForms = CreateRange('\ufff0', '\uffff');

#pragma warning disable 1591
        public static Range<char> BasicLatin { get { return basicLatin; } }
        public static Range<char> Latin1Supplement { get { return latin1Supplement; } }
        public static Range<char> LatinExtendedA { get { return latinExtendedA; } }
        public static Range<char> LatinExtendedB { get { return latinExtendedB; } }
        public static Range<char> IpaExtensions { get { return ipaExtensions; } }
        public static Range<char> SpacingModifierLetters { get { return spacingModifierLetters; } }
        public static Range<char> CombiningDiacriticalMarks { get { return combiningDiacriticalMarks; } }
        public static Range<char> GreekAndCoptic { get { return greekAndCoptic; } }
        public static Range<char> Cyrillic { get { return cyrillic; } }
        public static Range<char> CyrillicSupplement { get { return cyrillicSupplement; } }
        public static Range<char> Armenian { get { return armenian; } }
        public static Range<char> Hebrew { get { return hebrew; } }
        public static Range<char> Arabic { get { return arabic; } }
        public static Range<char> Syriac { get { return syriac; } }
        public static Range<char> Thaana { get { return thaana; } }
        public static Range<char> Devangari { get { return devangari; } }
        public static Range<char> Bengali { get { return bengali; } }
        public static Range<char> Gurmukhi { get { return gurmukhi; } }
        public static Range<char> Gujarati { get { return gujarati; } }
        public static Range<char> Oriya { get { return oriya; } }
        public static Range<char> Tamil { get { return tamil; } }
        public static Range<char> Telugu { get { return telugu; } }
        public static Range<char> Kannada { get { return kannada; } }
        public static Range<char> Malayalam { get { return malayalam; } }
        public static Range<char> Sinhala { get { return sinhala; } }
        public static Range<char> Thai { get { return thai; } }
        public static Range<char> Lao { get { return lao; } }
        public static Range<char> Tibetan { get { return tibetan; } }
        public static Range<char> Myanmar { get { return myanmar; } }
        public static Range<char> Georgian { get { return georgian; } }
        public static Range<char> HangulJamo { get { return hangulJamo; } }
        public static Range<char> Ethiopic { get { return ethiopic; } }
        public static Range<char> Cherokee { get { return cherokee; } }
        public static Range<char> UnifiedCanadianAboriginalSyllabics { get { return unifiedCanadianAboriginalSyllabics; } }
        public static Range<char> Ogham { get { return ogham; } }
        public static Range<char> Runic { get { return runic; } }
        public static Range<char> Tagalog { get { return tagalog; } }
        public static Range<char> Hanunoo { get { return hanunoo; } }
        public static Range<char> Buhid { get { return buhid; } }
        public static Range<char> Tagbanwa { get { return tagbanwa; } }
        public static Range<char> Khmer { get { return khmer; } }
        public static Range<char> Mongolian { get { return mongolian; } }
        public static Range<char> Limbu { get { return limbu; } }
        public static Range<char> TaiLe { get { return taiLe; } }
        public static Range<char> KhmerSymbols { get { return khmerSymbols; } }
        public static Range<char> PhoneticExtensions { get { return phoneticExtensions; } }
        public static Range<char> LatinExtendedAdditional { get { return latinExtendedAdditional; } }
        public static Range<char> GreekExtended { get { return greekExtended; } }
        public static Range<char> GeneralPunctuation { get { return generalPunctuation; } }
        public static Range<char> SuperscriptsandSubscripts { get { return superscriptsandSubscripts; } }
        public static Range<char> CurrencySymbols { get { return currencySymbols; } }
        public static Range<char> CombiningDiacriticalMarksforSymbols { get { return combiningDiacriticalMarksforSymbols; } }
        public static Range<char> LetterlikeSymbols { get { return letterlikeSymbols; } }
        public static Range<char> NumberForms { get { return numberForms; } }
        public static Range<char> Arrows { get { return arrows; } }
        public static Range<char> MathematicalOperators { get { return mathematicalOperators; } }
        public static Range<char> MiscellaneousTechnical { get { return miscellaneousTechnical; } }
        public static Range<char> ControlPictures { get { return controlPictures; } }
        public static Range<char> OpticalCharacterRecognition { get { return opticalCharacterRecognition; } }
        public static Range<char> EnclosedAlphanumerics { get { return enclosedAlphanumerics; } }
        public static Range<char> BoxDrawing { get { return boxDrawing; } }
        public static Range<char> BlockElements { get { return blockElements; } }
        public static Range<char> GeometricShapes { get { return geometricShapes; } }
        public static Range<char> MiscellaneousSymbols { get { return miscellaneousSymbols; } }
        public static Range<char> Dingbats { get { return dingbats; } }
        public static Range<char> MiscellaneousMathematicalSymbolsA { get { return miscellaneousMathematicalSymbolsA; } }
        public static Range<char> SupplementalArrowsA { get { return supplementalArrowsA; } }
        public static Range<char> BraillePatterns { get { return braillePatterns; } }
        public static Range<char> SupplementalArrowsB { get { return supplementalArrowsB; } }
        public static Range<char> MiscellaneousMathematicalSymbolsB { get { return miscellaneousMathematicalSymbolsB; } }
        public static Range<char> SupplementalMathematicalOperators { get { return supplementalMathematicalOperators; } }
        public static Range<char> MiscellaneousSymbolsandArrows { get { return miscellaneousSymbolsandArrows; } }
        public static Range<char> CjkRadicalsSupplement { get { return cjkRadicalsSupplement; } }
        public static Range<char> KangxiRadicals { get { return kangxiRadicals; } }
        public static Range<char> IdeographicDescriptionCharacters { get { return ideographicDescriptionCharacters; } }
        public static Range<char> CjkSymbolsandPunctuation { get { return cjkSymbolsandPunctuation; } }
        public static Range<char> Hiragana { get { return hiragana; } }
        public static Range<char> Katakana { get { return katakana; } }
        public static Range<char> Bopomofo { get { return bopomofo; } }
        public static Range<char> HangulCompatibilityJamo { get { return hangulCompatibilityJamo; } }
        public static Range<char> Kanbun { get { return kanbun; } }
        public static Range<char> BopomofoExtended { get { return bopomofoExtended; } }
        public static Range<char> KatakanaPhoneticExtensions { get { return katakanaPhoneticExtensions; } }
        public static Range<char> EnclosedCjkLettersandMonths { get { return enclosedCjkLettersandMonths; } }
        public static Range<char> CjkCompatibility { get { return cjkCompatibility; } }
        public static Range<char> CjkUnifiedIdeographsExtensionA { get { return cjkUnifiedIdeographsExtensionA; } }
        public static Range<char> YijingHexagramSymbols { get { return yijingHexagramSymbols; } }
        public static Range<char> CjkUnifiedIdeographs { get { return cjkUnifiedIdeographs; } }
        public static Range<char> YiSyllables { get { return yiSyllables; } }
        public static Range<char> YiRadicals { get { return yiRadicals; } }
        public static Range<char> HangulSyllables { get { return hangulSyllables; } }
        public static Range<char> HighSurrogates { get { return highSurrogates; } }
        public static Range<char> HighPrivateUseSurrogates { get { return highPrivateUseSurrogates; } }
        public static Range<char> LowSurrogates { get { return lowSurrogates; } }
        public static Range<char> PrivateUse { get { return privateUse; } }
        public static Range<char> PrivateUseArea { get { return privateUseArea; } }
        public static Range<char> CjkCompatibilityIdeographs { get { return cjkCompatibilityIdeographs; } }
        public static Range<char> AlphabeticPresentationForms { get { return alphabeticPresentationForms; } }
        public static Range<char> ArabicPresentationFormsA { get { return arabicPresentationFormsA; } }
        public static Range<char> VariationSelectors { get { return variationSelectors; } }
        public static Range<char> CombiningHalfMarks { get { return combiningHalfMarks; } }
        public static Range<char> CjkCompatibilityForms { get { return cjkCompatibilityForms; } }
        public static Range<char> SmallFormVariants { get { return smallFormVariants; } }
        public static Range<char> ArabicPresentationFormsB { get { return arabicPresentationFormsB; } }
        public static Range<char> HalfwidthandFullwidthForms { get { return halfwidthandFullwidthForms; } }
#pragma warning restore 1591

        /// <summary>
        /// Returns the unicode range containing the specified character.
        /// </summary>
        /// <param name="c">Character to look for</param>
        /// <returns>The unicode range containing the specified character, or null if the character
        /// is not in a unicode range.</returns>
        public static Range<char> GetRange(char c)
        {
            // TODO: Make this efficient. SortedList should do it with a binary search, but it
            // doesn't give us quite what we want
            foreach (Range<char> range in allRanges)
            {
                if (range.Contains(c)) 
                {
                    return range;
                }
            }
            return null;
        }
    }
}
