using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Textile.Blocks
{
    public static class NoTextileEncoder
    {
        private static readonly string[,] TextileModifiers = {
								{ "\"", "&#34;" },
								{ "%", "&#37;" },
            					{ "*", "&#42;" },
            					{ "+", "&#43;" },
            					{ "-", "&#45;" },
            					{ "<", "&lt;" },   // or "&#60;"
            					{ "=", "&#61;" },
            					{ ">", "&gt;" },   // or "&#62;"
            					{ "?", "&#63;" },     
            					{ "^", "&#94;" },
            					{ "_", "&#95;" },
            					{ "~", "&#126;" },
                                { "@", "&#64;" },
                                { "'", "&#39;" },
                                { "|", "&#124;" },
                                { "!", "&#33;" },
                                { "(", "&#40;" },
                                { ")", "&#41;" },
                                { ".", "&#46;" },
                                { "x", "&#120;" }
							};

        private static string[] m_encodeExceptions = null;
        private static string[] m_decodeExceptions = null;

        public static string EncodeNoTextileZones(string tmp, string patternPrefix, string patternSuffix)
        {
            return EncodeNoTextileZones(tmp, patternPrefix, patternSuffix, null);
        }

        public static string EncodeNoTextileZones(string tmp, string patternPrefix, string patternSuffix, string[] exceptions)
        {
            m_encodeExceptions = exceptions;
            MatchEvaluator me = new MatchEvaluator(EncodeNoTextileZonesMatchEvaluator);
            tmp = Regex.Replace(tmp, string.Format("({0}(?<notex>.+?){1})*", patternPrefix, patternSuffix), me);
            return tmp;
        }

        public static string DecodeNoTextileZones(string tmp, string patternPrefix, string patternSuffix)
        {
            return DecodeNoTextileZones(tmp, patternPrefix, patternSuffix, null);
        }

        public static string DecodeNoTextileZones(string tmp, string patternPrefix, string patternSuffix, string[] exceptions)
        {
            m_decodeExceptions = exceptions;
            MatchEvaluator me = new MatchEvaluator(DecodeNoTextileZonesMatchEvaluator);
            tmp = Regex.Replace(tmp, string.Format("({0}(?<notex>.+?){1})*", patternPrefix, patternSuffix), me);
            return tmp;
        }

        private static string EncodeNoTextileZonesMatchEvaluator(Match m)
        {
            string toEncode = m.Groups["notex"].Value;
            if (toEncode == string.Empty)
                return string.Empty;

            for (int i = 0; i < TextileModifiers.GetLength(0); ++i)
            {
                if (m_encodeExceptions == null || Array.IndexOf(m_encodeExceptions, TextileModifiers[i, 0]) < 0)
                    toEncode = toEncode.Replace(TextileModifiers[i, 0], TextileModifiers[i, 1]);
            }
            return toEncode;
        }

        private static string DecodeNoTextileZonesMatchEvaluator(Match m)
        {
            string toEncode = m.Groups["notex"].Value;
            for (int i = 0; i < TextileModifiers.GetLength(0); ++i)
            {
                if (m_decodeExceptions == null || Array.IndexOf(m_decodeExceptions, TextileModifiers[i, 0]) < 0)
                    toEncode = toEncode.Replace(TextileModifiers[i, 1], TextileModifiers[i, 0]);
            }
            return toEncode;
        }
    }
}
