using System;
using System.Security.Cryptography;
using System.Text;

namespace HashMash.Shared
{
    /*
     *  The main class that will handle the "mashing" (pathetic hashing) of some input text from the user in the Masher.razor componment.
     *  The class will store a total count for every click the user has made to calculate their erspective level, which is also stored.
     *  As the user progresses through levels, the hashing will get more complex.
     */

    public partial class Masher
    {

        //private MD5 _md5Hash;        // An MD5 object for MD5 hash calculation, unused as its broken
        private SHA256 _sha256Hash = SHA256.Create();  // A SHA256 object for SHA256 hash calculation

        public int bitShift { get; set; }
        public int charOffset { get; set; }
        public int modN { get; set; } = 1;      // Default 1 (no div by 0)


        /*
         *
         */

        public string getHash(HashAlgorithm hashAlgorithm, string input)
        {
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }



        /*
         * Given some input string, return "mashed" string with particular operations. Result should be the digest,
         * the hexadecimal representation of the string.
         */

        private string mashInput()
        {
            string mashed = "";
            string step1 = "";
            string potentialHash = inputValue;
            foreach (char ch in potentialHash)
            {
                int c = ch; // need to copy current ch in foreach to manipulate

                if (charOffsetEnabled) c += charOffset;
                if (bitShiftEnabled) c <<= bitShift;
                if (modNEnabled) c %= modN;
                string a = Convert.ToString(c, 16);
                if (a.Length > 1)
                {
                    step1 += a[0];
                    step1 += a[1];
                }
                else
                {
                    step1 += "0" + a;
                }
            }
            while (step1.Length < 64)
            {
                step1 += "6c";
            }
            if (step1.Length > 64)
            {
                step1 = step1.Substring(0, 64);
            }
            mashed = step1;
            if (sha256Enabled)
            {
                mashed = getHash(_sha256Hash, mashed);
                return mashed;
            }
            else
            {
                return step1;
            }
        }

        /*
         * Mash an individual character with appropriate operations.
         *
         * c is the char (as an int) being mashed
         * b is the base representation which we are converting to e.g. 10 (decimal), 2 (binary), 16 (hexa) etc.
         */

        public string mashCh(int c, int b)
        {
            if (charOffsetEnabled) c += charOffset;
            if (bitShiftEnabled) c <<= bitShift;
            if (modNEnabled) c %= modN;
            return Convert.ToString(c, b);
        }

        //public string shiftCh(int c, int b)
        //{
        //    string returnVal = Convert.ToString((c + _levelCount) << (_totalCount % 16), b);
        //    return returnVal;
        //}

        //public string modCh(int c, int b)
        //{
        //    string returnVal = Convert.ToString(((c + _levelCount) << (_totalCount % 16)) % _currentMod, b);
        //    if (returnVal.Length == 1) returnVal = "0" + returnVal;
        //    return returnVal;
        //}


    }
}
