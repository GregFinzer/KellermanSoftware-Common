using System;
using System.Text.RegularExpressions;

namespace KellermanSoftware.Common
{
    public static class Validation
    {
        /// <summary>
        /// Returns true if version is either 1.0, 2.2.2 or 3.3.3.3
        /// </summary>
        /// <param name="versionString"></param>
        /// <returns></returns>
        public static bool IsValidVersion(string versionString)
        {
            Regex versionPattern = new Regex(@"^(\d+\.){1,3}(\d+)$");
            return versionPattern.IsMatch(versionString);
        }

        public static bool IsValidURL(string url)
        {
            if (String.IsNullOrEmpty(url))
                return false;

            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && ((uriResult.Scheme == Uri.UriSchemeHttp) || (uriResult.Scheme == Uri.UriSchemeHttps));
            return result;
        }

        /// <summary>
        /// Used for seeing when users type in invalid data into a description box
        /// </summary>
        /// <param name="sSentence"></param>
        /// <param name="sDelimiter"></param>
        /// <param name="iMinDupes"></param>
        /// <returns></returns>
        public static bool WordPattern(string sSentence, string sDelimiter, int iMinDupes)
        {
            int iWords = StringUtil.NumberOfWords(sSentence, sDelimiter);
            string sWord;
            string sLastWord = "";
            int iDupes = 0;
            string sCatParse = "";

            for (int i = 1; i <= iWords; i++)
            {
                sWord = StringUtil.GetField(sSentence, i, sDelimiter);

                //Catch dupes right next to each other
                //Example:  blah blah blah
                if (sWord == sLastWord)
                {
                    iDupes++;
                }
                else
                {
                    iDupes = 0;
                }

                if (iDupes >= iMinDupes)
                    return true;

                sLastWord = sWord;

                //Catch multiple word duplicate patterns
                //Example:  I don't know I don't know
                if (sCatParse.Length > 0)
                {
                    sCatParse += sDelimiter + sWord;
                }
                else
                {
                    sCatParse = sWord;
                }

                if (i > 2 && sSentence.Length > sCatParse.Length + 1)
                {
                    if (sSentence.IndexOf(sCatParse, sCatParse.Length) > 0)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Robust e-mail validation (takes into account ip address and valid characters)
        /// </summary>
        /// <param name="emailAddress">E-Mail Address to check</param>
        /// <returns>Blank if E-mail Address is valid, Error message if not.</returns>
        public static string ValidEmail(string emailAddress)
        {
            string errorMessage;

            try
            {
                if (emailAddress.Length == 0)
                {
                    errorMessage = "E-Mail Address Is Blank";
                    return errorMessage;
                }

                /* The following variable tells the rest of the function whether or not
                to verify that the address ends in a two-letter country or well-known
                TLD.  1 means check it, 0 means don't. */
                const int checkTLD = 1;

                /* The following is the list of known TLDs that an e-mail address must end with. */
                string knownDomsPat = "^(com|net|org|edu|int|mil|gov|arpa|biz|aero|name|coop|info|pro|museum)$";

                /* The following pattern is used to check if the entered e-mail address
                fits the user@domain format.  It also is used to separate the username
                from the domain. */
                Regex emailPat = new Regex("^([^@]+)@([^@]+)$");

                /* The following string represents the pattern for matching all special
                characters.  We don't want to allow special characters in the address. 
                These characters include ( ) < > @ , ; : \ " . [ ] */
                //string specialChars="\\(\\)><@,;:\\\\\\\"\\.\\[\\]";  
                string specialChars = @"\(\)<>@,;:\\\.\[\]\" + StringUtil.Chr(34);

                /* The following string represents the range of characters allowed in a 
                username or domainname.  It really states which chars aren't allowed.*/
                //string validChars=@"\[^\\s" + specialChars + @"\]";	  
                string validChars = @"[^\s" + specialChars + "]";

                /* The following pattern applies if the "user" is a quoted string (in
                which case, there are no rules about which characters are allowed
                and which aren't; anything goes).  E.g. "jiminy cricket"@disney.com
                is a legal e-mail address. */
                //string quotedUser="(\"[^\"]*\")";
                string quotedUser = "(\\" + StringUtil.Chr(34) + "[^\\" + StringUtil.Chr(34) + "]*\\" + StringUtil.Chr(34) + ")";

                /* The following pattern applies for domains that are IP addresses,
                rather than symbolic names.  E.g. joe@[123.124.233.4] is a legal
                e-mail address. NOTE: The square brackets are required. */
                string ipDomainPat = @"^\[(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})\]$";  

                /* The following string represents an atom (basically a series of non-special characters.) */
                string atom = validChars + '+';

                /* The following string represents one word in the typical username.
                For example, in john.doe@somewhere.com, john and doe are words.
                Basically, a word is either an atom or quoted string. */
                string word = "(" + atom + "|" + quotedUser + ")";

                // The following pattern describes the structure of the user
                Regex userPat = new Regex("^" + word + "(\\." + word + ")*$");


                /* The following pattern describes the structure of a normal symbolic
                domain, as opposed to ipDomainPat, shown above. */
                Regex domainPat = new Regex("^" + atom + "(\\." + atom + ")*$");

                /* Finally, let's start trying to figure out if the supplied address is valid. */

                /* Begin with the coarse pattern to simply break up user@domain into
                different pieces that are easy to analyze. */
                Match myMatch;
                myMatch = emailPat.Match(emailAddress);

                //Original
                //var matchArray=emailStr.match(emailPat);

                if (myMatch.Groups.Count != 3)
                {
                    /* Too many/few @'s or something; basically, this address doesn't
                    even fit the general mould of a valid e-mail address. */
                    errorMessage = "Email address is incorrect (check @ and .'s)";
                    return errorMessage;
                }

                string user = myMatch.Groups[1].Value;
                string domain = myMatch.Groups[2].Value;

                // Start by checking that only basic ASCII characters are in the strings (0-127).
                for (int i = 0; i < user.Length; i++)
                {
                    if (StringUtil.Asc(user.Substring(i, 1)) > 127)
                    {
                        errorMessage = "E-mail user name contains invalid characters.";
                        return errorMessage;
                    }
                }

                for (int i = 0; i < domain.Length; i++)
                {
                    if (StringUtil.Asc(domain.Substring(i, 1)) > 127)
                    {
                        errorMessage = "The E-mail domain name contains invalid characters.";
                        return errorMessage;
                    }
                }

                // See if "user" is valid 
                if (userPat.IsMatch(user) == false)
                {
                    // user is not valid
                    errorMessage = "The E-mail username is not valid.";
                    return errorMessage;
                }

                /* if the e-mail address is at an IP address (as opposed to a symbolic
                host name) make sure the IP address is valid. */
                Match IPMatch;
                IPMatch = Regex.Match(domain, ipDomainPat);

                if (IPMatch.Groups.Count > 1)
                {
                    // this is an IP address
                    for (int i = 1; i <= 4; i++)
                    {
                        if (Convert.ToInt16(IPMatch.Groups[i].Value) > 255)
                        {
                            errorMessage = "E-Mail Destination IP address is invalid";
                            return errorMessage;
                        }
                    }

                    return "";
                }

                // Domain is symbolic name.  Check if it's valid.
                if (domainPat.IsMatch(domain) == false)
                {
                    errorMessage = "E-mail domain name is not valid.";
                    return errorMessage;
                }

                int len = StringUtil.NumberOfWords(domain, ".");

                /* domain name seems valid, but now make sure that it ends in a
                known top-level domain (like com, edu, gov) or a two-letter word,
                representing country (uk, nl), and that there's a hostname preceding 
                the domain or country. */
                if (checkTLD == 1 && StringUtil.GetField(domain, len, ".").Length != 2
                    && !Regex.IsMatch(StringUtil.GetField(domain, len, "."), knownDomsPat))
                {
                    errorMessage = "The E-Mail address must end in a well-known domain or two letter " + "country.";
                    return errorMessage;
                }


                // If we've gotten this far, everything's valid!
                return "";
            }
            catch (Exception ex)
            {
                return "Error in E-Mail Validation routine: " + ex.Message;
            }
        }

        /// <summary>
        /// Returns true if the SMTP Server is well formatted
        /// </summary>
        /// <param name="serverName"></param>
        /// <returns></returns>
        public static bool ValidSmtpServer(string serverName)
        {
            Regex smtpPattern = new Regex(@"^([\w\-]+\.)+([\w]{2,3})$");
            return smtpPattern.IsMatch(serverName);
        }
    }
}
