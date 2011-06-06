#region License Statement
// Copyright (c) L.A.B.Soft.  All rights reserved.
//
// The use and distribution terms for this software are covered by the 
// Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by 
// the terms of this license.
//
// You must not remove this notice, or any other, from this software.
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
#endregion


namespace Textile.Blocks
{
    public class NoTextileBlockModifier : BlockModifier
    {
        public override string ModifyLine(string line)
        {
            line = NoTextileEncoder.EncodeNoTextileZones(line, @"(?<=^|\s)<notextile>", @"</notextile>(?=(\s|$)?)");
            line = NoTextileEncoder.EncodeNoTextileZones(line, @"==", @"==");
            return line;
        }

        public override string Conclude(string line)
        {
            // Recode "x"... we can safely replace all occurences because there's no reason
            // we should leave it encoded.
            line = line.Replace("&#120;", "x");
            // Same with parenthesis
            line = line.Replace("&#40;", "(");
            line = line.Replace("&#41;", ")");
            // And same with period.
            line = line.Replace("&#46;", ".");
            return line;
        }
    }
}
