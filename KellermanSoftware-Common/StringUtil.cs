using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace KellermanSoftware.Common
{
    public static class StringUtil
    {
        /// <summary>
        /// Convert milliseconds to elapsed time like 2 days, 4 hours, 10 minutes, 20 seconds, 120 milliseconds
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
		public static String MillisecondsToTimeLapse(long milliseconds)
        {
            var ts = TimeSpan.FromMilliseconds(milliseconds);

            if (ts.TotalDays >= 1)
                return string.Format("{0:n0} days, {1:n0} hours, {2:n0} minutes, {3:n0} seconds, {4:n0} milliseconds", ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);

            if (ts.TotalHours >= 1)
                return string.Format("{0:n0} hours, {1:n0} minutes, {2:n0} seconds, {3:n0} milliseconds", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);

            if (ts.TotalMinutes >= 1)
                return string.Format("{0:n0} minutes, {1:n0} seconds, {2:n0} milliseconds", ts.Minutes, ts.Seconds, ts.Milliseconds);

            if (ts.TotalSeconds >= 1)
                return string.Format("{0:n0} seconds, {1:n0} ms", ts.Seconds, ts.Milliseconds);


            return string.Format("{0:n0} ms", ts.Milliseconds);
        }

        /// <summary>
        /// Convert bytes to string like 12GB
        /// </summary>
        /// <param name="byteCount"></param>
        /// <returns></returns>
        public static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString(CultureInfo.InvariantCulture) + " " + suf[place];
        }
		
        /// <summary>
        /// Get an Int64 hash code for a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Int64 GetInt64HashCode(string text)
        {
            if (text == null)
                return -1;

            if (text == string.Empty)
                return 0;

            byte[] byteContents = Encoding.Unicode.GetBytes(text);
            using (SHA256 hash = SHA256.Create())
            {
                byte[] bytes = hash.ComputeHash(byteContents);

                return new[] { 0, 8, 16, 24 }
                    .Select(i => BitConverter.ToInt64(bytes, i))
                    .Aggregate((x, y) => x ^ y);
            }
        }

        /// <summary>
        /// Reverse the text
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Reverse(string value)
        {
            char[] charArray = value.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        /// Returns true if the text matches any of the wildcards
        /// </summary>
        /// <param name="value"></param>
        /// <param name="wildcards"></param>
        /// <returns></returns>
        public static bool MatchWildcards(string value, List<string> wildcards)
        {
            foreach (var wildcard in wildcards)
            {
                if (MatchWildcard(value, wildcard))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the text matches the wildcard
        /// </summary>
        /// <param name="value"></param>
        /// <param name="wildcard"></param>
        /// <returns></returns>
        public static bool MatchWildcard(string value, string wildcard)
        {
            value = value ?? string.Empty;
            wildcard = wildcard ?? string.Empty;

            value = value.ToLower();
            wildcard = wildcard.ToLower();

            string match;

            if (wildcard.StartsWith("*") && wildcard.EndsWith("*"))
            {
                match = wildcard.Substring(1, wildcard.Length - 2);
                return value.Contains(match);
            }

            if (wildcard.StartsWith("*"))
            {
                match = wildcard.Substring(1);
                return value.EndsWith(match);
            }

            if (wildcard.EndsWith("*"))
            {
                match = wildcard.Substring(0, wildcard.Length - 1);
                return value.StartsWith(match);
            }

            return value == wildcard;

        }
        /// <summary>
        /// Exclude all characters that are not ASCII
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FilterAscii(string value)
        {
            string asAscii = Encoding.ASCII.GetString(
                Encoding.Convert(
                    Encoding.UTF8,
                    Encoding.GetEncoding(
                        Encoding.ASCII.EncodingName,
                        new EncoderReplacementFallback(string.Empty),
                        new DecoderExceptionFallback()
                    ),
                    Encoding.UTF8.GetBytes(value)
                )
            );

            return asAscii;
        }

        /// <summary>
        /// Return true if the string is all numbers
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNumeric(string value)
        {
            if (String.IsNullOrEmpty(value))
                return false;

            foreach (char c in value)
            {
                if (!Char.IsNumber(c))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if the string is a decimal
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsDecimal(string value)
        {
            decimal ouput;

            return Decimal.TryParse(value, out ouput);
        }

        /// <summary>
        /// FormatName decodes a NameString into its component parts and returns it in a requested format.
        /// [H] = Full honorific
        ///	[h] = Abbreviated honorific
        ///	[F] = First name
        ///	[f] = First initial
        ///	[M] = Middle name
        ///	[m] = Middle initial
        ///	[L] = Last name
        ///	[l] = Last initial
        ///	[S] = Full suffix
        ///	[s] = Abbreviated suffix
        ///	[.] = Period
        ///	[,] = Comma
        ///	[ ] = Space
        /// Original TSQL by blindman
        /// Conversion to C# by gfinzer
        /// </summary>
        /// <param name="sName">The raw value to be parsed</param>
        /// <param name="sNameFormat">String that defines the output format.  Each letter in the string represents a component of the name in the order that it is to be returned.</param>
        /// <returns></returns>
        public static string FormatName(string sName, string sNameFormat)
        {
            string sHonorific = "";
            string sFirstName = "";
            string sMiddleName = "";
            string sLastName = "";
            string sSuffix = "";
            string sTempString = "";
            string sWordList = "";
            bool bIgnorePeriod = false;
            System.Text.StringBuilder sbReturn = new System.Text.StringBuilder(100);

            //Prepare the string
            //Make sure each period is followed by a space character
            sName = sName.Replace(".", ". ").Trim();

            //Ensure commas are follows by spaces
            sName = sName.Replace(",", ", ");

            //Eliminate double-spaces.
            sName = sName.Replace("  ", " ");

            //Eliminate periods
            sName = sName.Replace(".", "");

            //If the lastname is listed first, strip it off.
            //Example:  Von Trap, Greg
            sTempString = GetField(sName, 1, " ");
            sWordList = "VAN,VON,MC,Mac,DE".ToUpper();

            if (InWordList(sTempString.ToUpper(), sWordList))
                sTempString = sTempString + GetField(sName, 2, " ");

            if (Right(sTempString, 1) == ",")
            {
                sLastName = TakeOffEnd(sTempString, ",");
                sName = Right(sName, sName.Length - sTempString.Length).Trim();
            }

            //Get rid of any remaining commas
            sName = sName.Replace(",", "");

            //Get Honorific and strip it out of the string
            sTempString = GetField(sName + " ", 1, " ");
            sWordList = "SR,SRA,SRTA,SEÑOR,SEÑORA,SEÑORITA,SENOR,SENORA,SENORITA,MR,MRS,MS,DR,Doctor,REV,Reverend,SIR,HON,Honorable,CPL,Corporal,SGT,Sergeant,GEN,General,CMD,Commander,CPT,CAPT,Captain,MAJ,Major,PVT,Private,LT,Lieutenant,FATHER,SISTER,".ToUpper();

            if (InWordList(sTempString.ToUpper(), sWordList))
            {
                sHonorific = sTempString;
                sName = Right(sName, sName.Length - sTempString.Length).Trim();
            }

            //Get Suffix and strip it out of the string
            sTempString = GetLastWords(sName, " ", NumberOfWords(sName, " "));
            sWordList = "Jr,Sr,II,III,IV,V,Esq,Junior,Senior".ToUpper();

            if (InWordList(sTempString.ToUpper(), sWordList))
            {
                sSuffix = sTempString;
                sName = Left(sName, sName.Length - sTempString.Length).Trim();
            }

            if (sLastName.Length == 0)
            {
                //Get LastName and strip it out of the string
                sLastName = GetLastWords(sName, " ", NumberOfWords(sName, " "));
                sName = Left(sName, sName.Length - sLastName.Length).Trim();

                //Check to see if the last name has two parts
                sTempString = GetLastWords(sName, " ", NumberOfWords(sName, " "));
                sWordList = "VAN,VON,MC,Mac,DE".ToUpper();

                if (InWordList(sTempString.ToUpper(), sWordList))
                {
                    sLastName = sTempString + " " + sLastName;
                    sName = Left(sName, sName.Length - sTempString.Length).Trim();
                }
            }

            //Get FirstName and strip it out of the string
            sFirstName = GetField(sName, 1, " ");
            sName = Right(sName, sName.Length - sFirstName.Length).Trim();

            //Anything remaining is MiddleName
            sMiddleName = sName;

            //Create the output string
            while (sNameFormat.Length > 0)
            {
                if (bIgnorePeriod == false || Left(sNameFormat, 1) != ".")
                {
                    bIgnorePeriod = false;

                    switch (Left(sNameFormat, 1))
                    {
                        case "H":
                            sbReturn.Append(VerboseHonorific(sHonorific));
                            break;
                        case "h":
                            sbReturn.Append(AbbreviatedHonorific(sHonorific));
                            break;
                        case "F":
                            sbReturn.Append(sFirstName);
                            break;
                        case "f":
                            //First Initial
                            sbReturn.Append(Left(sFirstName, 1).ToUpper());
                            break;
                        case "M":
                            sbReturn.Append(sMiddleName);
                            break;
                        case "m":
                            //Middle Initial
                            sbReturn.Append(Left(sMiddleName, 1).ToUpper());
                            break;
                        case "L":
                            sbReturn.Append(sLastName);
                            break;
                        case "l":
                            sbReturn.Append(Left(sLastName, 1).ToUpper());
                            break;
                        case "S":
                            if (sSuffix.ToUpper() == "JR")
                                sbReturn.Append("Junior");
                            else if (sSuffix.ToUpper() == "SR")
                                sbReturn.Append("Senior");
                            else if (sSuffix.ToUpper() == "ESQ")
                                sbReturn.Append("Esquire");
                            else
                                sbReturn.Append(sSuffix);
                            break;
                        case "s":
                            if (sSuffix.ToUpper() == "JUNIOR")
                                sbReturn.Append("Jr");
                            else if (sSuffix.ToUpper() == "SENIOR")
                                sbReturn.Append("Sr");
                            else if (sSuffix.ToUpper() == "ESQUIRE")
                                sbReturn.Append("Esq");
                            else
                                sbReturn.Append(sSuffix);
                            break;
                        case ".":
                            if (Right(sbReturn.ToString(), 1) != " ")
                                sbReturn.Append(".");
                            break;
                        case ",":
                            if (Right(sbReturn.ToString(), 1) != " ")
                                sbReturn.Append(",");
                            break;
                        case " ":
                            if (Right(sbReturn.ToString(), 1) != " ")
                                sbReturn.Append(" ");
                            break;
                    }
                }

                if ((Left(sNameFormat, 1) == "h"
                    && (sHonorific.ToUpper() == "FATHER" || sHonorific.ToUpper() == "SISTER"))
                    || (Left(sNameFormat, 1) == "s"
                    && (sHonorific == "II" || sHonorific == "III" || sHonorific == "IV" || sHonorific == "V")))
                {
                    bIgnorePeriod = true;
                }

                sNameFormat = Mid(sNameFormat, 2);
            }

            return sbReturn.ToString();
        }


       
        /// <summary>
        /// Word wrap text by the number of character width
        /// </summary>
        /// <param name="stringValue"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static string WordWrap(string stringValue, int width)
        {
            int pos, next;
            StringBuilder sb = new StringBuilder();

            // Lucidity check
            if (width < 1)
                return stringValue;

            // Parse each line of text
            for (pos = 0; pos < stringValue.Length; pos = next)
            {
                // Find end of line
                int eol = stringValue.IndexOf(Environment.NewLine, pos, StringComparison.Ordinal);

                if (eol == -1)
                    next = eol = stringValue.Length;
                else
                    next = eol + Environment.NewLine.Length;

                // Copy this line of text, breaking into smaller lines as needed
                if (eol > pos)
                {
                    do
                    {
                        int len = eol - pos;

                        if (len > width)
                            len = BreakLine(stringValue, pos, width);

                        sb.Append(stringValue, pos, len);
                        sb.Append(Environment.NewLine);

                        // Trim whitespace following break
                        pos += len;

                        while (pos < eol && Char.IsWhiteSpace(stringValue[pos]))
                            pos++;

                    } while (eol > pos);
                }
                else sb.Append(Environment.NewLine); // Empty line
            }

            return sb.ToString();
        }

        /// <summary>
        /// Locates position to break the given line so as to avoid
        /// breaking words.
        /// </summary>
        /// <param name="text">String that contains line of text</param>
        /// <param name="pos">Index where line of text starts</param>
        /// <param name="max">Maximum line length</param>
        /// <returns>The modified line length</returns>
        private static int BreakLine(string text, int pos, int max)
        {
            // Find last whitespace in line
            int i = max - 1;
            while (i >= 0 && !Char.IsWhiteSpace(text[pos + i]))
                i--;
            if (i < 0)
                return max; // No whitespace found; break at maximum length
            // Find start of whitespace
            while (i >= 0 && Char.IsWhiteSpace(text[pos + i]))
                i--;
            // Return length of text before whitespace
            return i + 1;
        }

        /// <summary>
        /// Same as VB mid function except it doesn't bomb at all
        /// </summary>
        /// <param name="oString">String to parse</param>
        /// <param name="iStart">Starting position</param>
        /// <returns>The string section</returns>
        public static string Mid(object oString, int iStart)
        {
            string sTemp;

            sTemp = oString.ToString();

            if (sTemp.Length < iStart)
                return "";
            else
                return Microsoft.VisualBasic.Strings.Mid(sTemp, iStart);
        }

        /// <summary>
        /// Returns true if a word is in a comma delimited word list
        /// </summary>
        /// <param name="sFind"></param>
        /// <param name="sWordList"></param>
        /// <returns></returns>
        public static bool InWordList(string sFind, string sWordList)
        {
            int iMax = NumberOfWords(sWordList, ",");

            for (int i = 1; i <= iMax; i++)
            {
                if (GetField(sWordList, i, ",") == sFind)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the abbreviated honorific for the passed honorific
        /// </summary>
        /// <param name="sHonorific"></param>
        /// <returns></returns>
        private static string AbbreviatedHonorific(string sHonorific)
        {
            switch (sHonorific.ToUpper().Trim())
            {
                case "SEÑOR":
                case "SENOR": //Spanish for Mr.
                    return "Sr";
                case "SEÑORA":
                case "SENORA": //Spanish for Mrs.
                    return "Sra";
                case "SEÑORITA":
                case "SENORITA": //Spanish for Ms.
                    return "Srta";
                case "DOCTOR":
                    return "Dr";
                case "REVERAND":
                    return "Rev";
                case "HONORABLE":
                    return "Hon";
                case "MAJOR":
                    return "Maj";
                case "PRIVATE":
                    return "Pvt";
                case "LIEUTENANT":
                    return "Lt";
                case "CAPTAIN":
                    return "Cpt";
                case "COMMANDER":
                    return "Cmd";
                case "GENERAL":
                    return "Gen";
                case "SERGEANT":
                    return "Sgt";
                case "CORPORAL":
                    return "Cpl";
                default:
                    return sHonorific;
            }

        }

        /// <summary>
        /// Returns a verbose honorific for an abbreviated honorific
        /// </summary>
        /// <param name="sAbbreviation">Example: Dr., SGT.</param>
        /// <returns></returns>
        private static string VerboseHonorific(string sAbbreviation)
        {
            switch (sAbbreviation.ToUpper().Replace(".", "").Trim())
            {
                case "Sr": //Spanish for Mr.
                    return "SEÑOR";
                case "SRA": //Spanish for Mrs.
                    return "SEÑORA";
                case "SRTA": //Spanish for Ms.
                    return "SEÑORITA";
                case "DR":
                    return "Doctor";
                case "REV":
                    return "Reverend";
                case "HON":
                    return "Honorable";
                case "MAJ":
                    return "Major";
                case "PVT":
                    return "Private";
                case "LT":
                    return "Lieutenant";
                case "CPT":
                case "CAP":
                    return "Captain";
                case "CMD":
                    return "Commander";
                case "GEN":
                    return "General";
                case "SGT":
                    return "Sergeant";
                case "CPL":
                    return "Corporal";
                default:
                    return sAbbreviation;
            }

        }

        /// <summary>
        /// Removes currency characters from a string like $, and spaces using the current culture
        /// </summary>
        /// <param name="currency">Currency string to parse</param>
        /// <returns>Pure currency with no formatting</returns>
        public static string RemoveCurrency(string currency)
        {
            string mask = @"\s|[$]";
            string symbol = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol.ToString();

            //Get the dollar sign for the current culture
            if (symbol.Length > 0)
            {
                mask = mask.Replace("$", symbol);
            }

            currency = Regex.Replace(currency, mask, "");

            if (currency.StartsWith("(") && currency.EndsWith(")") && currency.Length > 2)
            {
                currency = "-" + currency.Substring(1, currency.Length - 2);
            }

            return currency.Trim();
        }

        /// <summary>
        /// Build a string of characters
        /// </summary>
        /// <param name="amount">Number of Characters</param>
        /// <param name="character">The character to repeat</param>
        /// <returns>The repeated character string</returns>
        public static string CharString(int amount, string character)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(amount);

            for (int i = 0; i < amount; i++)
                sb.Append(character);

            return sb.ToString();
        }

        /// <summary>
        /// Get a string based on the specified ASCII code
        /// </summary>
        /// <param name="ASCIICode">ASCII Code</param>
        /// <returns>A Single Character String</returns>
        public static string Chr(int ASCIICode)
        {
            Byte[] myBytes2 = { (byte)ASCIICode };
            string myStr = System.Text.Encoding.ASCII.GetString(myBytes2);
            return myStr.Substring(0, 1);
        }

        /// <summary>
        /// Get the ASCII value for the character.
        /// </summary>
        /// <param name="inChar">The character to look at.</param>
        /// <returns>ASCII value of the passed character.</returns>
        public static int Asc(char inChar)
        {
            String work;
            work = Convert.ToString(inChar);

            Byte[] myBytes = System.Text.Encoding.ASCII.GetBytes(work);
            Byte asciiCode = myBytes[0];
            return (int)asciiCode;
        }

        /// <summary>
        /// Get the ASCII value for the left most character of the passed string.
        /// </summary>
        /// <param name="inString">The string to parse.</param>
        /// <returns>ACSCII value of the leftmost character</returns>
        public static int Asc(String inString)
        {
            if (inString.Length == 0)
                return 0;
            else
            {
                Byte[] myBytes = System.Text.Encoding.ASCII.GetBytes(inString);
                Byte asciiCode = myBytes[0];
                return (int)asciiCode;
            }
        }

        /// <summary>
        /// Convert HTML special characters into HTML Codes
        /// </summary>
        /// <param name="sValue">The string to parse</param>
        /// <returns>HTML friendly string</returns>
        public static string URLEscape(string sValue)
        {
            string sReturn = "";
            char cChar;

            //Alternate
            //return System.Web.HttpUtility.HtmlEncode(sValue); 

            for (int i = 0; i < sValue.Length; i++)
            {
                cChar = Convert.ToChar(sValue.Substring(i, 1));

                switch (cChar)
                {
                    case '\n':
                        sReturn += "%0D";
                        break;
                    //case 10:
                    //	sReturn+= "%0A";
                    //	break;
                    case ' ':
                        sReturn += "%20";
                        break;
                    case '>':
                        sReturn += "%3E";
                        break;
                    case '%':
                        sReturn += "%25";
                        break;
                    case '\\':
                        sReturn += "%5C";
                        break;
                    case '~':
                        sReturn += "%7E";
                        break;
                    case ']':
                        sReturn += "%5D";
                        break;
                    case ';':
                        sReturn += "%3B";
                        break;
                    case '?':
                        sReturn += "%3F";
                        break;
                    case '@':
                        sReturn += "%40";
                        break;
                    case '&':
                        sReturn += "%26";
                        break;
                    case '<':
                        sReturn += "%3C";
                        break;
                    case '#':
                        sReturn += "%23";
                        break;
                    case '{':
                        sReturn += "%7B";
                        break;
                    case '|':
                        sReturn += "%7C";
                        break;
                    case '^':
                        sReturn += "%5E";
                        break;
                    case '[':
                        sReturn += "%5B";
                        break;
                    case '`':
                        sReturn += "%60";
                        break;
                    case '/':
                        sReturn += "%2F";
                        break;
                    case ':':
                        sReturn += "%3A";
                        break;
                    case '=':
                        sReturn += "%3D";
                        break;
                    case '$':
                        sReturn += "%24";
                        break;
                    default:
                        sReturn += sValue.Substring(i, 1);
                        break;
                }
            }

            return sReturn;

        }

        [Obsolete("Use Pluralization.MakeSingular", true)]
        public static string MakeSingular(string word)
        {
            return string.Empty;
        }

        /// <summary>
        /// Get the left most characters of a string, handling null and dbnull
        /// </summary>
        /// <param name="myObject">String to parse</param>
        /// <param name="length">Left Position</param>
        /// <returns>String section</returns>
        public static string Left(object myObject, int length)
        {

            string results = string.Empty;

            try
            {
                if (length <= 0)
                {
                    return string.Empty;
                }

                if (myObject == null)
                {
                    return string.Empty;
                }

                if (myObject == System.DBNull.Value)
                {
                    return string.Empty;
                }

                if (myObject.ToString().Length == 0)
                {
                    return string.Empty;
                }

                results = myObject.ToString();
                length = Math.Min(results.Length, length);
                return myObject.ToString().Substring(0, length);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get the next word that is not in the passed list
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="wordsToExclude"></param>
        /// <returns></returns>
        public static string GetNextWordNotInList(string sentence, string[] wordsToExclude)
        {
            if (String.IsNullOrEmpty(sentence))
                return string.Empty;

            string[] words = sentence.Split(' ');
            bool matchWord = false; ;

            for (int i = 0; i < words.Length; i++)
            {
                matchWord = false;

                for (int j = 0; j < wordsToExclude.Length; j++)
                {
                    if (words[i].Length == 0 || words[i] == wordsToExclude[j])
                    {
                        matchWord = true;
                        break;
                    }
                }

                if (matchWord == false)
                    return words[i];
            }

            return string.Empty;
        }

        /// <summary>
        /// Get the next word after the current specified word in a sentence
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="currentWord"></param>
        /// <returns></returns>
        public static string GetNextWordAfterCurrentWord(string sentence, string currentWord)
        {
            int wordPosition = sentence.IndexOf(currentWord);
            int endOfWordPosition = wordPosition + currentWord.Length + 1; //Add 1 to include the space

            if (wordPosition < 0 || endOfWordPosition > sentence.Length)
            {
                return string.Empty;
            }
            else
            {
                int spacePosition = sentence.IndexOf(" ", endOfWordPosition);

                if (spacePosition < 0)
                    return sentence.Substring(endOfWordPosition);
                else
                    return sentence.Substring(endOfWordPosition, spacePosition - endOfWordPosition);
            }
        }

        /// <summary>
        /// Get all the words after the current word
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="currentWord"></param>
        /// <returns></returns>
        public static string GetAllWordsAfterCurrentWord(string sentence, string currentWord)
        {
            currentWord = currentWord+" ";
            int wordPosition = sentence.IndexOf(currentWord);
            int endOfWordPosition = wordPosition + currentWord.Length; //Add 1 to include the space

            if (wordPosition < 0 || endOfWordPosition > sentence.Length)
            {
                return string.Empty;
            }
            else
            {
                return sentence.Substring(endOfWordPosition).Trim();
            }
        }

        /// <summary>
        /// Make a string proper case if it is not already proper case
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ProperCase(string value)
        {
            //Do not proper case a string that is already proper cased
            if (value.ToUpper() != value)
                return value;

            System.Text.StringBuilder sb = new System.Text.StringBuilder(value.Length);

            char chNew;
            bool emptyBefore = true;

            foreach (char currentChar in value)
            {
                if (Char.IsWhiteSpace(currentChar))
                {
                    emptyBefore = true;
                    chNew = currentChar;
                }
                else
                {
                    if (Char.IsLetter(currentChar) && emptyBefore)
                        chNew = Char.ToUpper(currentChar);
                    else
                        chNew = Char.ToLower(currentChar);

                    emptyBefore = false;
                }

                sb.Append(chNew);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Return the string between the two words
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="fromWord"></param>
        /// <param name="toWord"></param>
        /// <returns></returns>
        public static string StringBetween(string sentence, string fromWord, string toWord)
        {
            return StringBefore(StringAfter(sentence, fromWord), toWord);
        }

        /// <summary>
        /// Return the string between the two words after specified index
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="startIndex"></param>
        /// <param name="fromWord"></param>
        /// <param name="toWord"></param>
        /// <returns></returns>
        public static string StringBetween(string sentence, int startIndex, string fromWord, string toWord)
        {            
            return StringBefore(StringAfter(sentence.Substring(startIndex), fromWord), toWord);
        }

        /// <summary>
        /// Return the string after the passed word
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string StringAfter(string sentence, string word)
        {
            int pos = sentence.IndexOf(word);

            if (pos >= 0)
                return sentence.Substring(pos + word.Length);
            else
                return string.Empty;
        }

        /// <summary>
        /// Return the string before the passed word
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string StringBefore(string sentence, string word)
        {
            int pos = sentence.IndexOf(word);

            if (pos >= 0)
                return sentence.Substring(0, pos);
            else
                return string.Empty;
        }

        /// <summary>
        /// Replaces the tag value.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="beginTag">The begin tag.</param>
        /// <param name="endTag">The end tag.</param>
        /// <param name="replaceText">The replace text.</param>
        /// <returns>System.String.</returns>
        public static string ReplaceTagValue(string text,
            string beginTag,
            string endTag,
            string replaceText)
        {
            int beginTagPos = text.IndexOf(beginTag);

            if (beginTagPos < 0)
                return text;

            string leftText = string.Empty;
            int endTagPos = text.IndexOf(endTag, beginTagPos+beginTag.Length);

            if (endTagPos <= 0)
                return text;

            string rightText = string.Empty;

            leftText = text.Substring(0, beginTagPos + beginTag.Length);

            rightText = text.Substring(endTagPos);

            return string.Format("{0}{1}{2}", leftText, replaceText, rightText);
        }

        /// <summary>
        /// Replace the text starting at the beginning tag and ending at the end tag
        /// </summary>
        /// <param name="text"></param>
        /// <param name="beginTag"></param>
        /// <param name="endTag"></param>
        /// <param name="replaceText"></param>
        /// <returns></returns>
        public static string ReplaceTag(string text,
            string beginTag,
            string endTag,
            string replaceText)
        {
            int beginTagPos = text.IndexOf(beginTag);
            string leftText = string.Empty;
            int endTagPos = text.IndexOf(endTag);
            string rightText = string.Empty;

            if (beginTagPos < 0)
                return text;
            else if (beginTagPos == 0)
                leftText = string.Empty;
            else
                leftText = text.Substring(0, beginTagPos);

            if (endTagPos <= 0)
                return text;
            else
                rightText = text.Substring(endTagPos + endTag.Length);

            return string.Format("{0}{1}{2}", leftText, replaceText, rightText);
        }

        /// <summary>
        /// Filter all characters except letters and numbers
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FilterAlphaNumeric(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            char[] arr = str.ToCharArray();

            arr = Array.FindAll(arr, char.IsLetterOrDigit);
            return new string(arr);
        }


        ///<summary>
        ///Filter out everything except for alpha, numbers, and underscore
        ///</summary>
        ///<param name="input"></param>
        ///<returns></returns>
        public static string FilterAlphaNumericUnderscore(string input)
        {
            if (String.IsNullOrEmpty(input))
                return string.Empty;

            StringBuilder sb = new StringBuilder(input.Length);

            string[] alphas = Regex.Split(input, @"\w+");

            for (int i = 0; i < alphas.Length; i++)
            {
                sb.Append(alphas[i]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Return a camelCase string
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public static string CamelCase(string sentence)
        {
            string currentChar = string.Empty;
            bool lastSpace = false;
            System.Text.StringBuilder sb = new StringBuilder(sentence.Length);

            sentence = sentence.Replace("_", " ");
            sentence = sentence.Replace("-", " ");
            sentence = sentence.Replace("  ", " ");
            sentence = sentence.Trim();

            for (int i = 0; i < sentence.Length; i++)
            {
                currentChar = sentence.Substring(i, 1);
                if (lastSpace)
                {
                    currentChar = currentChar.ToUpper();
                    lastSpace = false;
                    sb.Append(currentChar);
                }
                else if (currentChar == " ")
                {
                    lastSpace = true;
                    continue;
                }
                else
                {
                    currentChar = currentChar.ToLower();
                    sb.Append(currentChar);
                }
            }

            return sb.ToString();
        }
        
        /// <summary>
        /// Return a PascalCase string
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public static string PascalCase(string sentence)
        {
            string currentChar = string.Empty;
            bool lastSpace = true;
            System.Text.StringBuilder sb = new StringBuilder(sentence.Length);

            sentence = sentence.Replace("_", " ");
            sentence = sentence.Replace("-", " ");
            sentence = sentence.Replace("  ", " ");
            sentence = sentence.Trim();

            for (int i = 0; i < sentence.Length; i++)
            {
                currentChar = sentence.Substring(i, 1);
                if (lastSpace)
                {
                    currentChar = currentChar.ToUpper();
                    lastSpace = false;
                    sb.Append(currentChar);
                }
                else if (currentChar == " ")
                {
                    lastSpace = true;
                    continue;
                }
                else
                {
                    currentChar = currentChar.ToLower();
                    sb.Append(currentChar);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Insert spaces into a string 
        /// </summary>
        /// <example>
        /// OrderDetails = Order Details
        /// 10Net30 = 10 Net 30
        /// FTPHost = FTP Host
        /// </example> 
        /// <param name="input"></param>
        /// <returns></returns>
        public static string InsertSpaces(string input)
        {
            bool isSpace = false;
            bool isUpperOrNumber = false;
            bool isLower = false;
            bool isLastUpper = true;
            bool isNextCharLower = false;

            if (String.IsNullOrEmpty(input))
                return string.Empty;

            StringBuilder sb = new StringBuilder(input.Length + (int)(input.Length / 2));

            //Replace underline with spaces
            input = input.Replace("_", " ");
            input = input.Replace("-", " ");
            input = input.Replace("  ", " ");

            //Trim any spaces
            input = input.Trim();

            char[] chars = input.ToCharArray();

            sb.Append(chars[0]);

            for (int i = 1; i < chars.Length; i++)
            {
                isUpperOrNumber = (chars[i] >= 'A' && chars[i] <= 'Z') || (chars[i] >= '0' && chars[i] <= '9');
                isNextCharLower = i < chars.Length - 1 && (chars[i + 1] >= 'a' && chars[i + 1] <= 'z');
                isSpace = chars[i] == ' ';
                isLower = (chars[i] >= 'a' && chars[i] <= 'z');

                //There was a space already added
                if (isSpace)
                {
                }
                //Look for upper case characters that have lower case characters before
                //Or upper case characters where the next character is lower
                else if ((isUpperOrNumber && isLastUpper == false)
                    || (isUpperOrNumber && isNextCharLower && isLastUpper == true))
                {
                    sb.Append(' ');
                    isLastUpper = true;
                }
                else if (isLower)
                {
                    isLastUpper = false;
                }

                sb.Append(chars[i]);

            }

            //Replace double spaces
            sb.Replace("  ", " ");

            return sb.ToString();
        }

        /// <summary>
        /// Take a string that is delimited by NewLine and make into a list
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<String> StringToList(string value)
        {
            List<String> list = new List<string>();

            int words = NumberOfWords(value, System.Environment.NewLine);

            for (int i = 1; i < words; i++)
            {
                string word = GetField(value, i, System.Environment.NewLine);

                if (String.IsNullOrEmpty(word) == false)
                    list.Add(word);
            }

            return list;
        }

        /// <summary>
        /// Take a string list and combine into a string
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ListToString(List<String> list)
        {
            if (list == null || list.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder(list.Count * 80);

            foreach (string item in list)
            {
                if (String.IsNullOrEmpty(item) == false)
                {
                    sb.Append(item);
                    sb.Append(System.Environment.NewLine);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Same as VB function but doesn't bomb
        /// </summary>
        /// <param name="oString">The string to parse</param>
        /// <param name="iLength">Starting Right Position</param>
        /// <returns>String section</returns>
        public static string Right(object oString, int iLength)
        {
            string sText = oString.ToString();

            if (iLength > sText.Length)
                iLength = sText.Length;

            if (iLength <= 0)
                return "";
            else
                return sText.Substring(sText.Length - iLength, iLength);
        }

        /// <summary>
        /// Parses sentence delimited by separator and returns the word specified by the position. Very old. This is one based instead of zero based.
        /// </summary>
        /// <param name="sentence">The string to parse</param>
        /// <param name="position">Which word to look for</param>
        /// <param name="separator">What the words are delimited by</param>
        /// <returns>"" or the word found</returns>
        public static string GetField(string sentence, int position, string separator)
        {
            int startPos = 0;
            int seperatorPos;
            int fieldCount = 1;
            string field = "";

            seperatorPos = sentence.IndexOf(separator, startPos);

            while (seperatorPos > -1)
            {
                if (fieldCount == position)
                {
                    field = sentence.Substring(startPos, seperatorPos - startPos);
                    break;
                }

                fieldCount++;
                startPos = seperatorPos + separator.Length;
                seperatorPos = sentence.IndexOf(separator, startPos);
            }

            //This will get the last field if it does not have a delimiter at the end
            if (seperatorPos == -1 && startPos <= sentence.Length && fieldCount == position)
            {
                field = sentence.Substring(startPos);
            }

            return field;
        }

        /// <summary>
        /// Calculate the number of words in a string
        /// </summary>
        /// <param name="sentence">String to parse</param>
        /// <param name="separator">Separator character or string</param>
        /// <returns>The number of words in the string</returns>
        public static int NumberOfWords(string sentence, string separator)
        {
            int iSentenceLen = sentence.Length;
            int iStartPos = 0;
            int iWordCount = 0;

            //Count the separator characters
            for (int i = 0; i < iSentenceLen - 1; i++)
            {
                iStartPos = sentence.IndexOf(separator, iStartPos);

                if (iStartPos > -1)
                {
                    iWordCount++;
                    iStartPos++;
                }
                else
                    break;
            }

            //No Separator on the end of the sentence
            if (iSentenceLen > 0)
            {
                if (Right(sentence, 1) != separator && iSentenceLen > 0)
                    iWordCount++;
            }

            return iWordCount;

        }

        /// <summary>
        /// Get the last words of a sentence. Very old, this is one based, not zero based
        /// </summary>
        /// <param name="sentence">Sentence to parse</param>
        /// <param name="separator">Separator Character</param>
        /// <param name="startWord">First word to return</param>
        /// <returns></returns>
        public static string GetLastWords(string sentence,
            string separator,
            int startWord)
        {
            int iMaxWords;
            string sLastWords = "";

            iMaxWords = NumberOfWords(sentence, separator);

            for (int i = startWord; i <= iMaxWords; i++)
            {
                sLastWords += GetField(sentence, i, separator) + separator;
            }

            sLastWords = TakeOffEnd(sLastWords, separator);

            return sLastWords;

        }

        /// <summary>
        /// Gets the last word in a sentence by the separator
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>System.String.</returns>
        public static string GetLastWord(string sentence, string separator)
        {
            return GetLastWords(sentence, separator, NumberOfWords(sentence, separator));
        }

        /// <summary>
        /// Take a string off the end of another string.  Example:  1,2,3,
        /// </summary>
        /// <param name="value">The base string</param>
        /// <param name="takeOff">What to take off</param>
        /// <returns>The resulting string</returns>
        public static string TakeOffEnd(string value, string takeOff)
        {
            string sReturn = value;

            if (value.Length > 0 && value.EndsWith(takeOff))
                sReturn = value.Substring(0, value.Length - takeOff.Length);

            return sReturn;
        }

        /// <summary>
        /// Take a string off the beginning of another string.  Example:  ,1,2,3
        /// </summary>
        /// <param name="value">The base string</param>
        /// <param name="takeOff">What to take off</param>
        /// <returns>The resulting string</returns>
        public static string TakeOffBeginning(string value, string takeOff)
        {
            string sReturn = value;

            if (value.Length > 0 && value.StartsWith(takeOff))
                sReturn = value.Substring(1);

            return sReturn;
        }

        /// <summary>
        /// Get the method name without the generic type
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static string RemoveGenericTypeFromMethod(string methodName)
        {
            return methodName.Replace(("<" + StringUtil.StringBetween(methodName, "<", ">") + ">"), string.Empty);
        }

        /// <summary>
        /// Extract all numbers from a string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ExtractNumbers(string value)
        {
            if (String.IsNullOrEmpty(value))
                return value;

            StringBuilder sb = new StringBuilder(value.Length);

            foreach (char current in value)
            {
                if (char.IsNumber(current))
                    sb.Append(current);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns true if the passed in string can be parsed as a date
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsDate(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            DateTime date;

            return DateTime.TryParse(value, out date);
        }
    }
}
